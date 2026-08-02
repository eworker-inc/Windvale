using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Windvale.Bytecode;
using Windvale.Bootstrap;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime;

namespace Windvale.Os.Tests;

internal static class Program
{
    private static readonly ImmutableArray<Testˉcase> TESTS =
    [
        new("UEFI writer emits deterministic verified PE32+", Writerˉemitsˉdeterministicˉimage),
        new("UEFI writer accepts linked relative-only code", Writerˉacceptsˉlinkedˉrelativeˉcode),
        new("UEFI writer admits linked read-only native data", Writerˉadmitsˉlinkedˉreadˉonlyˉdata),
        new("UEFI writer rejects unsupported link shapes", Writerˉrejectsˉunsupportedˉlinks),
        new("UEFI verifier rejects malformed and noncanonical images", Verifierˉrejectsˉmalformedˉimages),
        new("UEFI verifier contains bounded hostile input", Verifierˉcontainsˉhostileˉinput),
        new("kernel memory planner selects one bounded conventional arena", Memoryˉplannerˉselectsˉboundedˉarena),
        new("kernel memory planner rejects malformed and hostile maps", Memoryˉplannerˉrejectsˉmalformedˉmaps),
        new("kernel page allocator is bounded deterministic and zeroing", Pageˉallocatorˉisˉboundedˉandˉzeroing),
        new("kernel paging planner enforces bounded W^X identity tables", Pagingˉplannerˉenforcesˉboundedˉidentityˉtables),
        new("kernel WVA shims bridge Main, normalized traps, and Q35 shutdown", Kernelˉassemblyˉshimˉbridgesˉmain),
        new("AOT Windvale admission rejects changed WVB before execution", Kernelˉwvbˉadmissionˉisˉbounded),
        new("portable WVB lowers into the bounded kernel native probe", Kernelˉnativeˉprobeˉisˉportableˉandˉbounded),
        new("x86-64 kernel compiler emits deterministic verified WVO", Kernelˉcompilerˉemitsˉverifiedˉobject),
        new("x86-64 kernel compiler rejects unsupported source shapes", Kernelˉcompilerˉrejectsˉunsupportedˉsource),
        new("x86-64 kernel paging boundary emits deterministic verified WVO", Kernelˉpagingˉboundaryˉemitsˉverifiedˉobject),
        new("x86-64 kernel exception boundary emits deterministic verified WVO", Kernelˉexceptionˉboundaryˉemitsˉverifiedˉobject),
        new("Windvale process policy binds init service, interpreter, and admitted WVB", Kernelˉprocessˉpolicyˉbindsˉimage),
        new("protected process planner isolates and revokes the interpreter resource", Kernelˉprocessˉplannerˉisolatesˉuserˉpages),
        new("x86-64 protected process boundary emits deterministic verified WVO", Kernelˉprocessˉboundaryˉemitsˉverifiedˉobject),
        new("firmware probe builds reproducibly", Firmwareˉprobeˉbuildsˉreproducibly),
        new("invalid-opcode firmware probe builds reproducibly", Invalidˉopcodeˉfirmwareˉprobeˉbuildsˉreproducibly),
        new("general-protection firmware probe builds reproducibly", Generalˉprotectionˉfirmwareˉprobeˉbuildsˉreproducibly),
        new("user-fault firmware probe builds reproducibly", Userˉfaultˉfirmwareˉprobeˉbuildsˉreproducibly),
        new("firmware probe carries compiled Windvale past the kernel handoff", Firmwareˉprobeˉcarriesˉcompiledˉsource),
    ];

    public static int Main()
    {
        var Failures = 0;
        foreach (var Test in TESTS)
        {
            try
            {
                Test.Body();
                Console.WriteLine($"PASS  {Test.Name}");
            }
            catch (Exception Exception)
            {
                Failures++;
                Console.Error.WriteLine($"FAIL  {Test.Name}");
                Console.Error.WriteLine($"      {Exception.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Tests: {TESTS.Length}, Passed: {TESTS.Length - Failures}, Failed: {Failures}");
        return Failures == 0 ? 0 : 1;
    }

    private static void Writerˉemitsˉdeterministicˉimage()
    {
        var Link = Linkˉcode([0x31, 0xC0, 0xC3]);
        var First = Uefiˉapplicationˉwriter.Write(Link);
        var Second = Uefiˉapplicationˉwriter.Write(Link);
        True(First.Success, First.Diagnostics.IsEmpty ? "UEFI encoding failed." : First.Diagnostics[0].Message);
        True(Second.Success, "Repeated UEFI encoding failed.");
        Sequenceˉequal(First.Imageˉbytes, Second.Imageˉbytes);
        Equal(1_536, First.Imageˉbytes.Length);

        var Verified = Uefiˉapplicationˉverifier.Verify(First.Imageˉbytes.AsSpan());
        Equal(0u, Verified.Entryˉcodeˉoffset);
        Sequenceˉequal([0x31, 0xC0, 0xC3], Verified.Codeˉbytes);
    }

    private static void Writerˉrejectsˉunsupportedˉlinks()
    {
        var Invalid = Linkˉcompiler.Link([], new(0, "Main"));
        Equal("WVU1001", Uefiˉapplicationˉwriter.Write(Invalid).Diagnostics[0].Code);
        Equal("WVU1001", Uefiˉapplicationˉwriter.Write(null!).Diagnostics[0].Code);

        var Nonzeroˉbase = Linkˉcode([0xC3], 1);
        Equal("WVU1002", Uefiˉapplicationˉwriter.Write(Nonzeroˉbase).Diagnostics[0].Code);

        var Twoˉsectionsˉobject = Objectˉcodec.Write(new(
            Objectˉarchitecture.X86ˉ64,
            [
                new(".text", Objectˉsectionˉkind.Code, 1, 1, [0xC3]),
                new(".data", Objectˉsectionˉkind.Writableˉdata, 1, 1, [0]),
            ],
            [new("Main", Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, 0, 1)],
            []));
        var Twoˉsections = Linkˉcompiler.Link(
            [new(Twoˉsectionsˉobject.ToImmutableArray())],
            new(0, "Main"));
        Equal("WVU1002", Uefiˉapplicationˉwriter.Write(Twoˉsections).Diagnostics[0].Code);

        var Relocatingˉobject = Objectˉcodec.Write(new(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 1, 5, [0, 0, 0, 0, 0xC3])],
            [new("Main", Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, 0, 5)],
            [new(Objectˉrelocationˉkind.Absoluteˉu32, 0, 0, 0, 0)]));
        var Relocating = Linkˉcompiler.Link(
            [new(Relocatingˉobject.ToImmutableArray())],
            new(0, "Main"));
        Equal("WVU1002", Uefiˉapplicationˉwriter.Write(Relocating).Diagnostics[0].Code);
    }

    private static void Writerˉadmitsˉlinkedˉreadˉonlyˉdata()
    {
        var Objectˉbytes = Objectˉcodec.Write(new(
            Objectˉarchitecture.X86ˉ64,
            [
                new(".text", Objectˉsectionˉkind.Code, 1, 1, [0xC3]),
                new(".rodata", Objectˉsectionˉkind.Readˉonlyˉdata, 1, 4, [3, 5, 8, 13]),
            ],
            [new("Main", Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, 0, 1)],
            [])).ToImmutableArray();
        var Link = Linkˉcompiler.Link([new(Objectˉbytes)], new(0, "Main"));
        True(Link.Success, "The code/read-only-data object did not link.");
        Equal(1, Link.Codeˉsectionˉcount);
        Equal(1, Link.Readˉonlyˉsectionˉcount);

        var Application = Uefiˉapplicationˉwriter.Write(Link);
        True(Application.Success, "The UEFI adapter rejected read-only native data.");
        var Verified = Uefiˉapplicationˉverifier.Verify(Application.Imageˉbytes.AsSpan());
        Sequenceˉequal([0xC3, 3, 5, 8, 13], Verified.Codeˉbytes);
        Equal(0u, Verified.Entryˉcodeˉoffset);
    }

    private static void Writerˉacceptsˉlinkedˉrelativeˉcode()
    {
        var Caller = Objectˉcodec.Write(new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 16, 6, [0xE8, 0, 0, 0, 0, 0xC3])],
            [
                new("Main", Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, 0, 6),
                new(
                    "Worker",
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
            ],
            [new(Objectˉrelocationˉkind.Relativeˉi32, 0, 1, 1, -4)])).ToImmutableArray();
        var Worker = Objectˉcodec.Write(new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 16, 3, [0x31, 0xC0, 0xC3])],
            [new("Worker", Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, 0, 3)],
            [])).ToImmutableArray();
        var Link = Linkˉcompiler.Link([new(Caller), new(Worker)], new(0, "Main"));
        True(Link.Success, "The relative-only multi-object link failed.");
        Equal(2, Link.Sectionˉcount);
        Equal(2, Link.Codeˉsectionˉcount);
        Equal(1, Link.Importˉcount);
        Equal(1, Link.Relocationˉcount);
        Equal(0, Link.Absoluteˉrelocationˉcount);
        Equal(1, Link.Relativeˉrelocationˉcount);

        var Application = Uefiˉapplicationˉwriter.Write(Link);
        True(Application.Success, "The UEFI adapter rejected resolved relative-only code.");
        var Verified = Uefiˉapplicationˉverifier.Verify(Application.Imageˉbytes.AsSpan());
        Equal(0u, Verified.Entryˉcodeˉoffset);
        Equal(19, Verified.Codeˉbytes.Length);
        Sequenceˉequal([0xE8, 0x0B, 0, 0, 0, 0xC3], Verified.Codeˉbytes[..6]);
        Sequenceˉequal([0x31, 0xC0, 0xC3], Verified.Codeˉbytes[16..]);
    }

    private static void Verifierˉrejectsˉmalformedˉimages()
    {
        var Canonical = Uefiˉapplicationˉwriter.Write(Linkˉcode([0x31, 0xC0, 0xC3])).Imageˉbytes;
        Reject(Canonical.AsSpan(0, Canonical.Length - 1).ToArray(), "WVU2001");
        Reject(new byte[Uefiˉapplicationˉcontract.MAX_APPLICATION_BYTES + 1], "WVU2001");
        Reject(Mutate(Canonical, 0x00), "WVU2002");
        Reject(Mutate(Canonical, 0x40), "WVU2002");
        Reject(Mutate(Canonical, 0x80), "WVU2003");
        Reject(Mutate(Canonical, 0x84), "WVU2003");
        Reject(Mutate(Canonical, 0x98), "WVU2004");
        Reject(Mutate(Canonical, 0x9A), "WVU2004");
        Reject(Mutate(Canonical, 0xDC), "WVU2004");
        Reject(Mutate(Canonical, 0x188), "WVU2005");
        Reject(Mutate(Canonical, 0x1B0), "WVU2005");
        Reject(Mutate(Canonical, 0x203), "WVU2007");
        Reject(Mutate(Canonical, 0x400), "WVU2006");
        Reject(Mutate(Canonical, 0x40C), "WVU2007");

        var Trailing = Canonical.Add(0);
        Reject(Trailing.AsSpan(), "WVU2001");
    }

    private static void Verifierˉcontainsˉhostileˉinput()
    {
        var Random = new Random(0x574F53);
        for (var Case = 0; Case < 256; Case++)
        {
            var Bytes = new byte[Random.Next(0, 4_097)];
            Random.NextBytes(Bytes);
            try
            {
                _ = Uefiˉapplicationˉverifier.Verify(Bytes);
            }
            catch (Uefiˉapplicationˉexception)
            {
            }
        }
    }

    private static void Memoryˉplannerˉselectsˉboundedˉarena()
    {
        var Map = Buildˉmemoryˉmap(
            new Memoryˉdescriptorˉinput(7, 0x0030_0000, 316),
            new Memoryˉdescriptorˉinput(2, 0x0010_0000, 8),
            new Memoryˉdescriptorˉinput(7, 0x0020_0000, 16));
        var Result = Kernelˉmemoryˉplanner.Plan(Map, 40);
        True(Result.Success, Result.Diagnostics.IsEmpty ? "Memory planning failed." : Result.Diagnostics[0].Message);
        var Plan = Result.Plan!;
        Equal(0x0040_0000UL, Plan.Arenaˉaddress);
        Equal(60UL, Plan.Arenaˉpages);
        Equal(0x0040_0000UL, Plan.Stateˉaddress);
        Equal(0x0040_0040UL, Plan.Handoffˉcopyˉaddress);
        Equal(0x0040_1000UL, Plan.Stackˉaddress);
        Equal(8_192UL, Plan.Stackˉbytes);
        Equal(0x0040_3000UL, Plan.Stackˉtop);
        Equal(3UL, Plan.Firstˉfreeˉpage);
        Equal(57UL, Plan.Freeˉpages);
    }

    private static void Memoryˉplannerˉrejectsˉmalformedˉmaps()
    {
        Memoryˉplanˉfails([], 40, "WVOS4001");
        Memoryˉplanˉfails(new byte[40], 39, "WVOS4001");
        Memoryˉplanˉfails(new byte[41], 40, "WVOS4001");
        Memoryˉplanˉfails(Buildˉmemoryˉmap(new Memoryˉdescriptorˉinput(7, 0x0010_0001, 16)), 40, "WVOS4002");
        Memoryˉplanˉfails(Buildˉmemoryˉmap(new Memoryˉdescriptorˉinput(7, 0x0010_0000, 0)), 40, "WVOS4003");
        Memoryˉplanˉfails(Buildˉmemoryˉmap(new Memoryˉdescriptorˉinput(7, 0xFFFF_FFFF_FFFF_F000, 2)), 40, "WVOS4004");
        Memoryˉplanˉfails(Buildˉmemoryˉmap(new Memoryˉdescriptorˉinput(2, 0x0010_0000, 56)), 40, "WVOS4005");
        Memoryˉplanˉfails(Buildˉmemoryˉmap(new Memoryˉdescriptorˉinput(7, 0x0030_0000, 314)), 40, "WVOS4005");
        Memoryˉplanˉfails(
            Buildˉmemoryˉmap(
                new Memoryˉdescriptorˉinput(7, 0x0020_0000, 60),
                new Memoryˉdescriptorˉinput(2, 0x0020_8000, 1)),
            40,
            "WVOS4006");

        var Random = new Random(0x4D454D);
        for (var Case = 0; Case < 256; Case++)
        {
            var Bytes = new byte[Random.Next(0, 4_097)];
            Random.NextBytes(Bytes);
            _ = Kernelˉmemoryˉplanner.Plan(Bytes, (ulong)Random.Next(0, 300));
        }
    }

    private static void Pageˉallocatorˉisˉboundedˉandˉzeroing()
    {
        var Plan = Kernelˉmemoryˉplanner.Plan(
            Buildˉmemoryˉmap(new Memoryˉdescriptorˉinput(7, 0x0020_0000, 60)),
            40).Plan!;
        var Arena = Enumerable.Repeat((byte)0xA5, checked((int)Kernelˉmemoryˉcontract.ARENA_BYTES)).ToArray();
        var Allocator = new Kernelˉpageˉallocator(Plan, Arena);
        True(Arena.All(Value => Value == 0), "Kernel arena initialization did not clear every byte.");

        Array.Fill(Arena, (byte)0x5A, 3 * 4_096, 4_096);
        Equal(0x0020_3000UL, Allocator.Allocateˉpages(1)!.Value);
        True(Arena.AsSpan(3 * 4_096, 4_096).IndexOfAnyExcept((byte)0) < 0, "Allocated page was not zeroed.");
        Equal(56UL, Allocator.Remainingˉpages);
        True(Allocator.Allocateˉpages(0) is null, "A zero-page allocation succeeded.");
        Equal(0x0020_4000UL, Allocator.Allocateˉpages(56)!.Value);
        Equal(0UL, Allocator.Remainingˉpages);
        True(Allocator.Allocateˉpages(1) is null, "An exhausted allocator returned a page.");
    }

    private static void Pagingˉplannerˉenforcesˉboundedˉidentityˉtables()
    {
        const ulong ROOT = 0x0020_0000;
        const ulong EXECUTABLE = 0x03FF_C000;
        var First = Kernelˉpagingˉplanner.Plan(ROOT, EXECUTABLE);
        var Second = Kernelˉpagingˉplanner.Plan(ROOT, EXECUTABLE);
        True(First.Success, First.Diagnostics.IsEmpty ? "Paging planning failed." : First.Diagnostics[0].Message);
        True(Second.Success, "Repeated paging planning failed.");
        Sequenceˉequal(First.Plan!.Tableˉbytes, Second.Plan!.Tableˉbytes);
        Sequenceˉequal(First.Plan.Ownershipˉrecord, Second.Plan.Ownershipˉrecord);
        Equal(24_576, First.Plan.Tableˉbytes.Length);
        Equal(64, First.Plan.Ownershipˉrecord.Length);

        var Plan = First.Plan;
        Equal(ROOT + 0x1000UL | 3UL, Kernelˉpagingˉplanner.Readˉentry(Plan, 0, 0));
        Equal(ROOT + 0x2000UL | 3UL, Kernelˉpagingˉplanner.Readˉentry(Plan, 1, 0));
        Equal(ROOT + 0x3000UL | 3UL, Kernelˉpagingˉplanner.Readˉentry(Plan, 2, 0));
        Equal(0UL, Kernelˉpagingˉplanner.Readˉentry(Plan, 3, 0));
        Equal(
            0x1000UL | 3UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            Kernelˉpagingˉplanner.Readˉentry(Plan, 3, 1));
        Equal(
            0x0020_0000UL | 0x83UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            Kernelˉpagingˉplanner.Readˉentry(Plan, 2, 1));
        Equal(ROOT + 0x4000UL | 3UL, Kernelˉpagingˉplanner.Readˉentry(Plan, 2, 31));
        Equal(ROOT + 0x5000UL | 3UL, Kernelˉpagingˉplanner.Readˉentry(Plan, 2, 32));
        Equal(
            EXECUTABLE | Kernelˉpagingˉcontract.ENTRY_PRESENT,
            Kernelˉpagingˉplanner.Readˉentry(Plan, 4, 508));
        Equal(
            (EXECUTABLE - Kernelˉpagingˉcontract.PAGE_BYTES) | 3UL |
                Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            Kernelˉpagingˉplanner.Readˉentry(Plan, 4, 507));
        Equal(
            (EXECUTABLE + Kernelˉpagingˉcontract.EXECUTABLE_BYTES) | 3UL |
                Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            Kernelˉpagingˉplanner.Readˉentry(Plan, 5, 60));

        for (ulong Page = 3; Page <= 5; Page++)
        {
            for (var Entry = 0; Entry < 512; Entry++)
            {
                var Value = Kernelˉpagingˉplanner.Readˉentry(Plan, Page, Entry);
                True(
                    (Value & (Kernelˉpagingˉcontract.ENTRY_PRESENT |
                        Kernelˉpagingˉcontract.ENTRY_WRITABLE |
                        Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE)) !=
                    (Kernelˉpagingˉcontract.ENTRY_PRESENT |
                        Kernelˉpagingˉcontract.ENTRY_WRITABLE),
                    "The page tables contain a writable executable leaf.");
            }
        }

        Equal(Kernelˉpagingˉcontract.RECORD_MAGIC,
            BinaryPrimitives.ReadUInt64LittleEndian(Plan.Ownershipˉrecord.AsSpan()));
        Equal(Kernelˉpagingˉcontract.RECORD_VERSION,
            BinaryPrimitives.ReadUInt32LittleEndian(Plan.Ownershipˉrecord.AsSpan()[8..]));
        Equal(ROOT, BinaryPrimitives.ReadUInt64LittleEndian(Plan.Ownershipˉrecord.AsSpan()[16..]));
        Equal(EXECUTABLE, BinaryPrimitives.ReadUInt64LittleEndian(Plan.Ownershipˉrecord.AsSpan()[40..]));
        Equal(Kernelˉpagingˉcontract.RECORD_FLAGS,
            BinaryPrimitives.ReadUInt64LittleEndian(Plan.Ownershipˉrecord.AsSpan()[56..]));

        Pagingˉplanˉfails(0, EXECUTABLE, "WVOS5001");
        Pagingˉplanˉfails(ROOT + 1, EXECUTABLE, "WVOS5001");
        Pagingˉplanˉfails(Kernelˉpagingˉcontract.IDENTITY_BYTES - 0x5000, EXECUTABLE, "WVOS5001");
        Pagingˉplanˉfails(ROOT, 0x001F_F000, "WVOS5002");
        Pagingˉplanˉfails(ROOT, EXECUTABLE + 1, "WVOS5002");
        Pagingˉplanˉfails(ROOT, Kernelˉpagingˉcontract.IDENTITY_BYTES - 0x200000, "WVOS5002");
        Pagingˉplanˉfails(ROOT, ROOT, "WVOS5003");
    }

    private static void Kernelˉcompilerˉemitsˉverifiedˉobject()
    {
        var First = X64ˉkernelˉcompiler.Compile(HELLO_WORLD_SOURCE, "Hello-World.wv");
        var Second = X64ˉkernelˉcompiler.Compile(HELLO_WORLD_SOURCE, "Hello-World.wv");
        True(First.Success, First.Diagnostics.IsEmpty ? "Native compilation failed." : First.Diagnostics[0].ToString());
        True(Second.Success, "Repeated native compilation failed.");
        Sequenceˉequal(First.Objectˉbytes, Second.Objectˉbytes);
        Equal(2_954, First.Objectˉbytes.Length);
        Equal(
            "61df8691c2b1c6eff31a6782cca144669aad32c26294e60fb97b8d5b15ff4de4",
            Objectˉdigest.Calculateˉsha256(First.Objectˉbytes.AsSpan()));

        var Object = Objectˉcodec.Readˉandˉverify(First.Objectˉbytes.AsSpan()).Value;
        True(Object.Architecture == Objectˉarchitecture.X86ˉ64, "The native object architecture is not x86-64.");
        Equal(1, Object.Sections.Length);
        True(Object.Sections[0].Kind == Objectˉsectionˉkind.Code, "The native object section is not code.");
        Equal(4, Object.Symbols.Length);
        Equal(X64ˉkernelˉcontract.KERNEL_ENTRY_SYMBOL, Object.Symbols[0].Name);
        True(Object.Symbols[0].Binding == Objectˉsymbolˉbinding.Export, "The kernel entry is not exported.");
        Equal(X64ˉkernelˉcontract.KERNEL_MAIN_SYMBOL, Object.Symbols[1].Name);
        True(Object.Symbols[1].Binding == Objectˉsymbolˉbinding.Export, "The kernel Main body is not exported.");
        Equal(X64ˉkernelˉcontract.MEMORY_ENTER_SYMBOL, Object.Symbols[2].Name);
        True(Object.Symbols[2].Binding == Objectˉsymbolˉbinding.Import, "The memory entry is not imported.");
        Equal(X64ˉkernelˉcontract.WRITE_BYTE_SYMBOL, Object.Symbols[3].Name);
        True(Object.Symbols[3].Binding == Objectˉsymbolˉbinding.Import, "The byte writer is not imported.");
        Equal(85, Object.Relocations.Length);
        True(
            Object.Relocations.All(Relocation =>
                Relocation.Kind == Objectˉrelocationˉkind.Relativeˉi32 &&
                Relocation.Sectionˉindex == 0 &&
                Relocation.Addend == -4),
            "The generated output calls do not use the canonical relative relocation contract.");
        Equal(2u, Object.Relocations[0].Symbolˉindex);
        True(
            Object.Relocations.Skip(1).All(Relocation => Relocation.Symbolˉindex == 3),
            "The generated Main body does not call only the imported byte writer.");
        Equal(1, Countˉsequence(Object.Sections[0].Data, [0xB9, (byte)'H', 0, 0, 0, 0xE8]));

        var Changed = X64ˉkernelˉcompiler.Compile(
            HELLO_WORLD_SOURCE.Replace("Hello from Windvale", "Hi", StringComparison.Ordinal),
            "Changed.wv");
        True(Changed.Success, "The supported source variation did not compile.");
        True(
            !Changed.Objectˉbytes.AsSpan().SequenceEqual(First.Objectˉbytes.AsSpan()),
            "Changing source output did not change the native object.");
        Equal(68, Objectˉcodec.Readˉandˉverify(Changed.Objectˉbytes.AsSpan()).Value.Relocations.Length);
    }

    private static void Kernelˉcompilerˉrejectsˉunsupportedˉsource()
    {
        var Hosted = X64ˉkernelˉcompiler.Compile(
            HELLO_WORLD_SOURCE.Replace("profile system", "profile hosted", StringComparison.Ordinal),
            "hosted.wv");
        True(!Hosted.Success, "A hosted module was accepted by the system-profile kernel target.");
        True(
            Hosted.Diagnostics.Any(Diagnostic => Diagnostic.Code == "WVN1001"),
            "A hosted kernel target did not produce WVN1001.");

        var Unicode = X64ˉkernelˉcompiler.Compile(
            HELLO_WORLD_SOURCE.Replace("Hello from Windvale", "Héllo from Windvale", StringComparison.Ordinal),
            "unicode.wv");
        True(!Unicode.Success, "Non-ASCII kernel console output was accepted.");
        True(
            Unicode.Diagnostics.Any(Diagnostic => Diagnostic.Code == "WVN1005"),
            "Non-ASCII kernel output did not produce WVN1005.");

        var Branch = X64ˉkernelˉcompiler.Compile(
            HELLO_WORLD_SOURCE.Replace(
                "console.write_line(Greeting);",
                "if true { console.write_line(Greeting); }",
                StringComparison.Ordinal),
            "branch.wv");
        True(!Branch.Success, "A branching kernel entry was accepted by the linear target.");
        True(
            Branch.Diagnostics.Any(Diagnostic => Diagnostic.Code == "WVN1003"),
            "A branching kernel entry did not produce WVN1003.");
    }

    private static void Kernelˉpagingˉboundaryˉemitsˉverifiedˉobject()
    {
        var First = Kernelˉpagingˉx64.Build();
        var Second = Kernelˉpagingˉx64.Build();
        Sequenceˉequal(First.Objectˉbytes, Second.Objectˉbytes);
        Sequenceˉequal(First.Codeˉbytes, Second.Codeˉbytes);
        Equal(1_244, First.Objectˉbytes.Length);
        Equal(
            "63e3cbd8cfb0f5a6260b660d4f2253c3f14b3a5f71271fe99ecf04644c4b6c2d",
            Objectˉdigest.Calculateˉsha256(First.Objectˉbytes.AsSpan()));
        Equal(851, First.Codeˉbytes.Length);
        Equal(
            "fc841c0eb94adce393014597a404e1ffb6f5cb53dd472f8fb87bc837276e4b88",
            Objectˉdigest.Calculateˉsha256(First.Codeˉbytes.AsSpan()));

        var Object = Objectˉcodec.Readˉandˉverify(First.Objectˉbytes.AsSpan()).Value;
        Equal(1, Object.Sections.Length);
        True(Object.Sections[0].Kind == Objectˉsectionˉkind.Code, "The paging object is not code.");
        Sequenceˉequal(First.Codeˉbytes, Object.Sections[0].Data);
        Equal(5, Object.Symbols.Length);
        Equal(Kernelˉpagingˉcontract.INSTALL_SYMBOL, Object.Symbols[0].Name);
        True(Object.Symbols[0].Binding == Objectˉsymbolˉbinding.Export, "The paging installer is not exported.");
        Equal(Firmwareˉprobe.ENTRY_SYMBOL, Object.Symbols[1].Name);
        Equal(Kernelˉmemoryˉcontract.ALLOCATE_PAGES_SYMBOL, Object.Symbols[2].Name);
        Equal(Kernelˉpagingˉcontract.PROTECTION_ENABLE_SYMBOL, Object.Symbols[3].Name);
        Equal(Kernelˉpagingˉcontract.PAGE_TABLE_ACTIVATE_SYMBOL, Object.Symbols[4].Name);
        True(Object.Symbols.Skip(1).All(Symbol => Symbol.Binding == Objectˉsymbolˉbinding.Import),
            "A paging dependency is not an explicit import.");
        Equal(4, Object.Relocations.Length);
        Equal(new Objectˉrelocation(Objectˉrelocationˉkind.Relativeˉi32, 0, 254, 1, -4), Object.Relocations[0]);
        Equal(new Objectˉrelocation(Objectˉrelocationˉkind.Relativeˉi32, 0, 306, 2, -4), Object.Relocations[1]);
        Equal(new Objectˉrelocation(Objectˉrelocationˉkind.Relativeˉi32, 0, 667, 3, -4), Object.Relocations[2]);
        Equal(new Objectˉrelocation(Objectˉrelocationˉkind.Relativeˉi32, 0, 675, 4, -4), Object.Relocations[3]);
        Equal(1, Countˉsequence(First.Codeˉbytes, [0x0F, 0x01, 0x44, 0x24, 0x20]));
        Equal(2, Countˉsequence(First.Codeˉbytes, [0x0F, 0xA2]));
        Equal(1, Countˉsequence(First.Codeˉbytes, [0xB9, 0x00, 0x02, 0x00, 0x00]));
        Equal(1, Countˉsequence(First.Codeˉbytes, [0xB9, 0x00, 0x04, 0x00, 0x00]));
        Equal(1, Countˉsequence(First.Codeˉbytes, [0xB9, 0x40, 0x00, 0x00, 0x00]));
        Equal(1, Countˉsequence(First.Codeˉbytes, [0x57, 0x56, 0x4B, 0x50, 0x41, 0x47, 0x30, 0x33]));
        Equal(0, Countˉsequence(First.Codeˉbytes, [0x0F, 0x22, 0xD8]));
        Equal(0, Countˉsequence(First.Codeˉbytes, [0x0F, 0x32]));
    }

    private static void Kernelˉexceptionˉboundaryˉemitsˉverifiedˉobject()
    {
        var First = Kernelˉexceptionˉx64.Build();
        var Second = Kernelˉexceptionˉx64.Build();
        Sequenceˉequal(First.Objectˉbytes, Second.Objectˉbytes);
        Sequenceˉequal(First.Codeˉbytes, Second.Codeˉbytes);
        Equal(First.Installerˉbytes, Second.Installerˉbytes);
        Equal(First.Terminalˉoffset, Second.Terminalˉoffset);
        True(First.Installerˉbytes <= First.Terminalˉoffset, "The terminal handler overlaps the exception installer.");
        Equal(4_667, First.Objectˉbytes.Length);
        Equal(
            "49f15606d2cd41236f87e8a7a7e24a9532683ffe9d5a59795dc8084288b2f84a",
            Objectˉdigest.Calculateˉsha256(First.Objectˉbytes.AsSpan()));
        Equal(4_348, First.Codeˉbytes.Length);
        Equal(
            "9307e2e9e4471d15448326ab2a86464f652fadfe60901226ef32b02e4dc9f8b9",
            Objectˉdigest.Calculateˉsha256(First.Codeˉbytes.AsSpan()));
        Equal(222u, First.Installerˉbytes);
        Equal(224u, First.Terminalˉoffset);
        Equal(0u, First.Terminalˉoffset % 16);

        Equal(2, Kernelˉexceptionˉcontract.FORMAT_VERSION);
        Equal(6u, Kernelˉexceptionˉcontract.INVALID_OPCODE_VECTOR);
        Equal(13u, Kernelˉexceptionˉcontract.GENERAL_PROTECTION_VECTOR);
        Equal(4_096u, Kernelˉexceptionˉcontract.IDT_PAGE_BYTES);
        Equal(16u, Kernelˉexceptionˉcontract.IDT_GATE_BYTES);
        Equal(96u, Kernelˉexceptionˉcontract.INVALID_OPCODE_GATE_OFFSET);
        Equal(208u, Kernelˉexceptionˉcontract.GENERAL_PROTECTION_GATE_OFFSET);
        Equal(224u, Kernelˉexceptionˉcontract.IDT_DESCRIPTOR_OFFSET);
        Equal((ushort)223, Kernelˉexceptionˉcontract.IDT_LIMIT);
        Equal((byte)0x8E, Kernelˉexceptionˉcontract.INTERRUPT_GATE_ATTRIBUTES);
        Equal(0u, Kernelˉexceptionˉcontract.NORMALIZED_VECTOR_OFFSET);
        Equal(8u, Kernelˉexceptionˉcontract.NORMALIZED_ERROR_CODE_OFFSET);
        Equal(16u, Kernelˉexceptionˉcontract.NORMALIZED_INSTRUCTION_POINTER_OFFSET);
        Equal(24u, Kernelˉexceptionˉcontract.NORMALIZED_CODE_SELECTOR_OFFSET);
        Equal(32u, Kernelˉexceptionˉcontract.NORMALIZED_FLAGS_OFFSET);
        Equal(40u, Kernelˉexceptionˉcontract.NORMALIZED_FRAME_BYTES);
        Equal(
            "panic=invalid-opcode\nvector=6\nerror-code=0\nstatus=panic\n",
            Kernelˉexceptionˉcontract.INVALID_OPCODE_PANIC_MARKER);
        Equal(
            "panic=general-protection\nvector=13\nerror-code=0\nstatus=panic\n",
            Kernelˉexceptionˉcontract.GENERAL_PROTECTION_PANIC_MARKER);

        var Object = Objectˉcodec.Readˉandˉverify(First.Objectˉbytes.AsSpan()).Value;
        True(Object.Architecture == Objectˉarchitecture.X86ˉ64, "The exception object architecture is not x86-64.");
        Equal(1, Object.Sections.Length);
        True(Object.Sections[0].Kind == Objectˉsectionˉkind.Code, "The exception object section is not code.");
        Sequenceˉequal(First.Codeˉbytes, Object.Sections[0].Data);
        Equal(4, Object.Symbols.Length);
        Equal(Kernelˉexceptionˉcontract.INSTALL_SYMBOL, Object.Symbols[0].Name);
        True(Object.Symbols[0].Binding == Objectˉsymbolˉbinding.Export, "The exception installer is not exported.");
        Equal(First.Installerˉbytes, Object.Symbols[0].Size);
        Equal(Kernelˉexceptionˉcontract.TERMINAL_SYMBOL, Object.Symbols[1].Name);
        True(Object.Symbols[1].Binding == Objectˉsymbolˉbinding.Export, "The normalized terminal handler is not exported.");
        Equal(First.Terminalˉoffset, Object.Symbols[1].Offset);
        Equal(Kernelˉexceptionˉcontract.GENERAL_PROTECTION_ENTRY_SYMBOL, Object.Symbols[2].Name);
        True(Object.Symbols[2].Binding == Objectˉsymbolˉbinding.Import, "The vector-13 WVA entry is not imported.");
        Equal(Kernelˉexceptionˉcontract.INVALID_OPCODE_ENTRY_SYMBOL, Object.Symbols[3].Name);
        True(Object.Symbols[3].Binding == Objectˉsymbolˉbinding.Import, "The vector-6 WVA entry is not imported.");
        Equal(2, Object.Relocations.Length);
        True(Object.Relocations[0].Kind == Objectˉrelocationˉkind.Relativeˉi32, "The vector-6 gate target is not relative.");
        Equal(3u, Object.Relocations[0].Symbolˉindex);
        Equal(63u, Object.Relocations[0].Offset);
        Equal(-4L, Object.Relocations[0].Addend);
        True(Object.Relocations[1].Kind == Objectˉrelocationˉkind.Relativeˉi32, "The vector-13 gate target is not relative.");
        Equal(2u, Object.Relocations[1].Symbolˉindex);
        Equal(115u, Object.Relocations[1].Offset);
        Equal(-4L, Object.Relocations[1].Addend);

        Equal(1, Countˉsequence(First.Codeˉbytes, [0xB9, 0x00, 0x02, 0x00, 0x00, 0xFC, 0xF3, 0x48, 0xAB]));
        Equal(1, Countˉsequence(First.Codeˉbytes, [0x31, 0xC0, 0x8C, 0xC8, 0x85, 0xC0]));
        Equal(1, Countˉsequence(First.Codeˉbytes, [0x41, 0xC6, 0x40, 0x65, 0x8E]));
        Equal(1, Countˉsequence(First.Codeˉbytes, [0x41, 0xC6, 0x80, 0xD5, 0x00, 0x00, 0x00, 0x8E]));
        Equal(1, Countˉsequence(First.Codeˉbytes, [0x66, 0x41, 0xC7, 0x80, 0xE0, 0x00, 0x00, 0x00, 0xDF, 0x00]));
        Equal(1, Countˉsequence(First.Codeˉbytes, [0xFA, 0x41, 0x0F, 0x01, 0x98, 0xE0, 0x00, 0x00, 0x00]));
        Equal(1, Countˉsequence(First.Codeˉbytes, [0x48, 0x83, 0x3C, 0x24, 0x06]));
        Equal(1, Countˉsequence(First.Codeˉbytes, [0x48, 0x83, 0x3C, 0x24, 0x0D]));
        Equal(
            Kernelˉexceptionˉcontract.INVALID_OPCODE_PANIC_MARKER.Length +
                Kernelˉexceptionˉcontract.GENERAL_PROTECTION_PANIC_MARKER.Length +
                Kernelˉexceptionˉcontract.MALFORMED_FRAME_PANIC_MARKER.Length,
            Countˉsequence(First.Codeˉbytes, [0xBA, 0xFD, 0x03, 0x00, 0x00, 0xEC, 0xA8, 0x20, 0x0F, 0x84]));
        Equal(1, Countˉsequence(
            First.Codeˉbytes,
            [0xBA, 0xF4, 0x00, 0x00, 0x00, 0xB8, 0x01, 0x00, 0x00, 0x00, 0xEF, 0xFA, 0xF4, 0xE9]));
        Equal(0, Countˉsequence(First.Codeˉbytes, [0x0F, 0x0B]));
        Equal(0, Countˉsequence(First.Codeˉbytes, [0x48, 0xCF]));
    }

    private static void Kernelˉprocessˉpolicyˉbindsˉimage()
    {
        var Admission = Kernelˉwvbˉadmission.Build();
        var First = Kernelˉprocessˉimage.Build(Admission, false);
        var Second = Kernelˉprocessˉimage.Build(Admission, false);
        var Faultˉfirst = Kernelˉprocessˉimage.Build(Admission, true);
        var Faultˉsecond = Kernelˉprocessˉimage.Build(Admission, true);
        Equal(5_152, First.Policyˉmoduleˉbytes.Length);
        Equal("c4aacb9036f825ecd3d038954c1d07c573b43eb4f6ee831d0b81d188f0682679",
            Objectˉdigest.Calculateˉsha256(First.Policyˉmoduleˉbytes.AsSpan()));
        Equal(45_886, First.Policyˉnativeˉobjectˉbytes.Length);
        Equal("d6290156c6d7cf709ba44b3035fac4eea75995ecf3ddfbcb8a0a9b73c0612509",
            Objectˉdigest.Calculateˉsha256(First.Policyˉnativeˉobjectˉbytes.AsSpan()));
        Equal(273, First.Initˉserviceˉmoduleˉbytes.Length);
        Equal("0fe423c499ce4f573095ddb9ff03355ee8b6ad927941f764ddaf2eaf9537f78b",
            Objectˉdigest.Calculateˉsha256(First.Initˉserviceˉmoduleˉbytes.AsSpan()));
        Equal(1_441, First.Initˉserviceˉnativeˉobjectˉbytes.Length);
        Equal("bccf48af1600cf3be8b93c8f132f227a064a324ac47b23d8ff9cdcf7f21d799a",
            Objectˉdigest.Calculateˉsha256(First.Initˉserviceˉnativeˉobjectˉbytes.AsSpan()));
        Equal(214, First.Initˉserviceˉshimˉobjectˉbytes.Length);
        Equal("914327761fee08c69979c0da8a2ef513ac569bd39ab76597590fdf65a5df0511",
            Objectˉdigest.Calculateˉsha256(First.Initˉserviceˉshimˉobjectˉbytes.AsSpan()));
        Equal(1_385, First.Initˉserviceˉimageˉbytes.Length);
        Equal("ba2a2abe03d420506c79af61cc917f4b0124a2ad7687fa80117e353dde475727",
            Objectˉdigest.Calculateˉsha256(First.Initˉserviceˉimageˉbytes.AsSpan()));
        Equal(12_265, First.Interpreterˉmoduleˉbytes.Length);
        Equal("25a223346c6357290680476a39a4e67821e5efc9420933a90486f993aef46bf2",
            Objectˉdigest.Calculateˉsha256(First.Interpreterˉmoduleˉbytes.AsSpan()));
        Equal(128_340, First.Interpreterˉnativeˉobjectˉbytes.Length);
        Equal("5157b4446422d37597b16b5f29b5aae3f05920fc4718af1a9759efe29f4e73b7",
            Objectˉdigest.Calculateˉsha256(First.Interpreterˉnativeˉobjectˉbytes.AsSpan()));
        Equal(314, First.Bootˉresourceˉserviceˉstencilˉobjectˉbytes.Length);
        Equal("1e690b8eebe6a21e4c4f6b697258c33c47370eb6b1277bdd40959cc077c29816",
            Objectˉdigest.Calculateˉsha256(First.Bootˉresourceˉserviceˉstencilˉobjectˉbytes.AsSpan()));
        Equal(314, First.Bootˉresourceˉserviceˉobjectˉbytes.Length);
        Equal("610b861538697ca15c7f2b5fac5bc222be5697a2063509ffb7ab5b0e669a226d",
            Objectˉdigest.Calculateˉsha256(First.Bootˉresourceˉserviceˉobjectˉbytes.AsSpan()));
        Equal(205, First.Clientˉshimˉobjectˉbytes.Length);
        Equal("6a22069adef6f9a4b58d1dda2bfe0c2b35e8563bb4e7e73641f050c2eeae058d",
            Objectˉdigest.Calculateˉsha256(First.Clientˉshimˉobjectˉbytes.AsSpan()));
        Equal(193, Faultˉfirst.Clientˉshimˉobjectˉbytes.Length);
        Equal("c57327ddf897fb32cc57dd1266c467283273eddafd8d4b78edfc43e59fc8eeee",
            Objectˉdigest.Calculateˉsha256(Faultˉfirst.Clientˉshimˉobjectˉbytes.AsSpan()));
        Equal(128_157, First.Clientˉimageˉbytes.Length);
        Equal("5a0acf3db339df5c3308f51a2e7ce182ee884d9b528db2998e9d0dcbf3b30655",
            Objectˉdigest.Calculateˉsha256(First.Clientˉimageˉbytes.AsSpan()));
        Equal(128_157, Faultˉfirst.Clientˉimageˉbytes.Length);
        Equal("1a56e471c06702e479ec7c1cee49d98415734e7d5fca24f46fbc3c66c8175a83",
            Objectˉdigest.Calculateˉsha256(Faultˉfirst.Clientˉimageˉbytes.AsSpan()));

        Sequenceˉequal(First.Policyˉmoduleˉbytes, Second.Policyˉmoduleˉbytes);
        Sequenceˉequal(First.Policyˉnativeˉobjectˉbytes, Second.Policyˉnativeˉobjectˉbytes);
        Sequenceˉequal(First.Initˉserviceˉmoduleˉbytes, Second.Initˉserviceˉmoduleˉbytes);
        Sequenceˉequal(First.Initˉserviceˉimageˉbytes, Second.Initˉserviceˉimageˉbytes);
        Sequenceˉequal(First.Interpreterˉmoduleˉbytes, Second.Interpreterˉmoduleˉbytes);
        Sequenceˉequal(First.Interpreterˉnativeˉobjectˉbytes, Second.Interpreterˉnativeˉobjectˉbytes);
        Sequenceˉequal(First.Bootˉresourceˉserviceˉstencilˉobjectˉbytes,
            Second.Bootˉresourceˉserviceˉstencilˉobjectˉbytes);
        Sequenceˉequal(First.Bootˉresourceˉserviceˉobjectˉbytes,
            Second.Bootˉresourceˉserviceˉobjectˉbytes);
        Equal(First.Bootˉresourceˉserviceˉoffset, Second.Bootˉresourceˉserviceˉoffset);
        Sequenceˉequal(First.Admittedˉprogramˉbytes, Second.Admittedˉprogramˉbytes);
        Sequenceˉequal(First.Clientˉshimˉobjectˉbytes, Second.Clientˉshimˉobjectˉbytes);
        Sequenceˉequal(First.Clientˉimageˉbytes, Second.Clientˉimageˉbytes);
        Sequenceˉequal(Faultˉfirst.Clientˉimageˉbytes, Faultˉsecond.Clientˉimageˉbytes);
        Sequenceˉequal(First.Policyˉmoduleˉbytes, Faultˉfirst.Policyˉmoduleˉbytes);
        Sequenceˉequal(First.Policyˉnativeˉobjectˉbytes, Faultˉfirst.Policyˉnativeˉobjectˉbytes);
        True(!First.Clientˉimageˉbytes.AsSpan().SequenceEqual(Faultˉfirst.Clientˉimageˉbytes.AsSpan()),
            "The normal and deliberate-fault user images are identical.");

        var Policy = Moduleˉcodec.Readˉandˉverify(First.Policyˉmoduleˉbytes.AsSpan());
        Equal("Processˉfoundation", Policy.Module.Name);
        True(Policy.Module.Profile == Moduleˉprofile.Portable, "The process policy is not portable Windvale.");
        Equal(0, Policy.Module.Capabilities.Length);
        Equal(Kernelˉprocessˉcontract.POLICY_TOKEN,
            Runˉportableˉmain(First.Policyˉmoduleˉbytes).Exitˉcode);
        Equal((int)Kernelˉprocessˉcontract.RESOURCE_ID,
            Runˉportableˉmain(First.Initˉserviceˉmoduleˉbytes).Exitˉcode);
        var Interpreterˉmodule = Moduleˉcodec.Readˉandˉverify(
            First.Interpreterˉmoduleˉbytes.AsSpan());
        True(Interpreterˉmodule.Module.Profile == Moduleˉprofile.Hosted,
            "The bytecode interpreter is not an explicitly hosted module.");
        Equal(1, Interpreterˉmodule.Module.Capabilities.Length);
        Equal(Capabilityˉcatalog.FILE_READ_BYTES,
            Interpreterˉmodule.Module.Capabilities[0].Name);
        Equal(1, Interpreterˉmodule.Module.Data.Length);
        True(Interpreterˉmodule.Module.Data[0] is Textˉdataˉdeclaration
            {
                Name: "Runtimeˉinputˉname",
                Value: "boot:main.wvb",
            }, "The interpreter does not name its one runtime-owned WVB resource.");
        Equal(1, Interpreterˉmodule.Functions.SelectMany(Function => Function.Instructions)
            .Count(Instruction => Instruction.Opcode == Opcode.Callˉcapability));
        Equal(0, Countˉsequence(
            First.Interpreterˉmoduleˉbytes, First.Admittedˉprogramˉbytes));
        Equal(0, Countˉsequence(
            First.Initˉserviceˉimageˉbytes, First.Admittedˉprogramˉbytes));
        Equal(0, Countˉsequence(
            First.Clientˉimageˉbytes, First.Admittedˉprogramˉbytes));
        var Interpreter = Runˉinterpreter(
            First.Interpreterˉmoduleˉbytes, Admission.Embeddedˉmoduleˉbytes);
        Equal(Kernelˉprocessˉcontract.EXPECTED_RESULT, Interpreter.Exitˉcode);
        Equal((long)Kernelˉprocessˉcontract.CLIENT_INSTRUCTION_BUDGET,
            Interpreter.Executedˉinstructions);
        Sequenceˉequal(First.Interpreterˉmoduleˉbytes,
            Kernelˉprocessˉimage.Compileˉinterpreterˉmodule());

        var Offsetˉvariant = Seedˉcompiler.Compile("""
            module Offsetˉindependentˉembeddedˉprogram profile portable;

            export fn Main() -> i32 {
                return 29;
            }
            """, "Offset-Independent-Embedded-Program.wv");
        True(Offsetˉvariant.Success,
            "The offset-variant embedded program did not compile: " +
                string.Join(" | ", Offsetˉvariant.Diagnostics));
        True(Offsetˉvariant.Moduleˉbytes.Length != Admission.Embeddedˉmoduleˉbytes.Length,
            "The offset-variant WVB unexpectedly retained the canonical byte length.");
        True(
            Findˉwvbˉsectionˉpayload(Offsetˉvariant.Moduleˉbytes, 5) !=
                Findˉwvbˉsectionˉpayload(Admission.Embeddedˉmoduleˉbytes, 5),
            "The offset-variant WVB did not move the code-section payload.");
        Equal(Kernelˉprocessˉcontract.EXPECTED_RESULT,
            Runˉinterpreter(First.Interpreterˉmoduleˉbytes,
                Offsetˉvariant.Moduleˉbytes).Exitˉcode);

        Equal(-8, Runˉinterpreter(First.Interpreterˉmoduleˉbytes,
            Admission.Embeddedˉmoduleˉbytes[..^1]).Exitˉcode);
        Equal(-2, Runˉinterpreter(First.Interpreterˉmoduleˉbytes,
            Mutate(Admission.Embeddedˉmoduleˉbytes, 0).ToImmutableArray()).Exitˉcode);
        Equal(-6, Runˉinterpreter(First.Interpreterˉmoduleˉbytes,
            Mutate(Admission.Embeddedˉmoduleˉbytes, 117).ToImmutableArray()).Exitˉcode);
        Equal(-32, Runˉinterpreter(First.Interpreterˉmoduleˉbytes,
            Mutate(Admission.Embeddedˉmoduleˉbytes, 121).ToImmutableArray()).Exitˉcode);
        Equal(28, Runˉinterpreter(First.Interpreterˉmoduleˉbytes,
            Mutate(Admission.Embeddedˉmoduleˉbytes, 122).ToImmutableArray()).Exitˉcode);
        Equal(-24, Runˉinterpreter(First.Interpreterˉmoduleˉbytes,
            Mutate(Admission.Embeddedˉmoduleˉbytes, 127).ToImmutableArray()).Exitˉcode);
        Sequenceˉequal(Admission.Embeddedˉmoduleˉbytes,
            Kernelˉwvbˉadmission.Build().Embeddedˉmoduleˉbytes);
        Equal(
            "7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2",
            Convert.ToHexString(First.Admittedˉprogramˉdigest.AsSpan()).ToLowerInvariant());
        Equal(
            "25a223346c6357290680476a39a4e67821e5efc9420933a90486f993aef46bf2",
            Convert.ToHexString(First.Interpreterˉdigest.AsSpan()).ToLowerInvariant());
        Equal(
            "0fe423c499ce4f573095ddb9ff03355ee8b6ad927941f764ddaf2eaf9537f78b",
            Convert.ToHexString(First.Initˉserviceˉdigest.AsSpan()).ToLowerInvariant());

        var Policyˉobject = Objectˉcodec.Readˉandˉverify(First.Policyˉnativeˉobjectˉbytes.AsSpan()).Value;
        Equal(1, Policyˉobject.Symbols.Count(Symbol =>
            Symbol.Binding == Objectˉsymbolˉbinding.Export &&
            Symbol.Name == Kernelˉprocessˉcontract.POLICY_SYMBOL));
        var Serviceˉobject = Objectˉcodec.Readˉandˉverify(First.Initˉserviceˉshimˉobjectˉbytes.AsSpan()).Value;
        var Bootˉresourceˉstencil = Objectˉcodec.Readˉandˉverify(
            First.Bootˉresourceˉserviceˉstencilˉobjectˉbytes.AsSpan()).Value;
        var Bootˉresourceˉservice = Objectˉcodec.Readˉandˉverify(
            First.Bootˉresourceˉserviceˉobjectˉbytes.AsSpan()).Value;
        var Userˉobject = Objectˉcodec.Readˉandˉverify(First.Clientˉshimˉobjectˉbytes.AsSpan()).Value;
        var Faultˉobject = Objectˉcodec.Readˉandˉverify(Faultˉfirst.Clientˉshimˉobjectˉbytes.AsSpan()).Value;
        Equal(3, Countˉsequence(Serviceˉobject.Sections[0].Data, [0x0F, 0x05]));
        Equal(2, Countˉsequence(Userˉobject.Sections[0].Data, [0x0F, 0x05]));
        Equal(1, Countˉsequence(Faultˉobject.Sections[0].Data, [0x0F, 0x05]));
        Equal(1, Countˉsequence(Faultˉobject.Sections[0].Data, [0xFA]));
        Equal(0, Countˉsequence(Faultˉobject.Sections[0].Data, [0xCC]));
        Equal(1, Bootˉresourceˉstencil.Sections.Length);
        True(Bootˉresourceˉstencil.Sections[0].Kind == Objectˉsectionˉkind.Readˉonlyˉdata,
            "The WVA boot-resource stencil is not read-only data.");
        Equal((int)Kernelˉprocessˉcontract.BOOT_RESOURCE_SERVICE_BYTES,
            Bootˉresourceˉstencil.Sections[0].Data.Length);
        Equal("8fccee8f5fc7369f88c5fa018d8b05eeb4c6b0317c7e1a5ad9d7cb88b95b2422",
            Objectˉdigest.Calculateˉsha256(Bootˉresourceˉstencil.Sections[0].Data.AsSpan()));
        Equal(1, Bootˉresourceˉstencil.Symbols.Length);
        Equal("Windvale_os_boot_resource_read_bytes_stencil",
            Bootˉresourceˉstencil.Symbols[0].Name);
        True(Bootˉresourceˉstencil.Symbols[0].Kind == Objectˉsymbolˉkind.Data,
            "The WVA boot-resource stencil is not a data symbol.");
        Equal(1, Bootˉresourceˉservice.Sections.Length);
        True(Bootˉresourceˉservice.Sections[0].Kind == Objectˉsectionˉkind.Code,
            "The published boot-resource service is not executable code.");
        Sequenceˉequal(Bootˉresourceˉstencil.Sections[0].Data,
            Bootˉresourceˉservice.Sections[0].Data);
        Equal(1, Bootˉresourceˉservice.Symbols.Length);
        Equal(Kernelˉprocessˉcontract.BOOT_RESOURCE_SERVICE_SYMBOL,
            Bootˉresourceˉservice.Symbols[0].Name);
        True(Bootˉresourceˉservice.Symbols[0].Kind == Objectˉsymbolˉkind.Function,
            "The published boot-resource service is not a function symbol.");
        True(
            First.Bootˉresourceˉserviceˉoffset + Kernelˉprocessˉcontract.BOOT_RESOURCE_SERVICE_BYTES <=
                (uint)First.Clientˉimageˉbytes.Length,
            "The linked boot-resource service extends outside the client image.");
        Sequenceˉequal(
            Bootˉresourceˉservice.Sections[0].Data,
            First.Clientˉimageˉbytes[
                (int)First.Bootˉresourceˉserviceˉoffset..
                (int)(First.Bootˉresourceˉserviceˉoffset +
                    Kernelˉprocessˉcontract.BOOT_RESOURCE_SERVICE_BYTES)]);
        True((ulong)First.Initˉserviceˉimageˉbytes.Length <= Kernelˉpagingˉcontract.PAGE_BYTES,
            "The init service image exceeds one page.");
        True((ulong)First.Clientˉimageˉbytes.Length <= Kernelˉprocessˉcontract.CLIENT_CODE_BYTES,
            "The interpreter client image exceeds its RX extent.");
        True((ulong)Faultˉfirst.Clientˉimageˉbytes.Length <= Kernelˉprocessˉcontract.CLIENT_CODE_BYTES,
            "The deliberate-fault interpreter image exceeds its RX extent.");
    }

    private static void Kernelˉprocessˉplannerˉisolatesˉuserˉpages()
    {
        const ulong KERNEL_ROOT = 0x0020_0000;
        const ulong EXECUTABLE = 0x0040_0000;
        const ulong ALLOCATION = 0x0080_0000;
        const ulong CLIENT_ALLOCATION = 0x00A0_0000;
        const ulong CHANNEL = 0x0060_0400;
        var Paging = Kernelˉpagingˉplanner.Plan(KERNEL_ROOT, EXECUTABLE).Plan!;
        var Image = Kernelˉprocessˉimage.Build(Kernelˉwvbˉadmission.Build(), false);
        var Initˉdefinition = new Kernelˉprocessˉdefinition(
            Kernelˉprocessˉcontract.INIT_PROCESS_ID,
            Kernelˉprocessˉcontract.INIT_THREAD_ID,
            Kernelˉprocessˉcontract.ROLE_INIT_SERVICE,
            Kernelˉprocessˉcontract.INIT_CAPABILITY_RIGHTS,
            CHANNEL);
        var Clientˉdefinition = new Kernelˉprocessˉdefinition(
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID,
            Kernelˉprocessˉcontract.CLIENT_THREAD_ID,
            Kernelˉprocessˉcontract.ROLE_BYTECODE_INTERPRETER,
            Kernelˉprocessˉcontract.CAPABILITY_RIGHT_SEND,
            CHANNEL);
        var First = Kernelˉprocessˉplanner.Plan(
            Paging, ALLOCATION, Image.Initˉserviceˉimageˉbytes.AsSpan(),
            Image.Initˉserviceˉdigest.AsSpan(), Image.Admittedˉprogramˉdigest.AsSpan(),
            Image.Admittedˉprogramˉbytes.AsSpan(), 0, Initˉdefinition);
        var Second = Kernelˉprocessˉplanner.Plan(
            Paging, ALLOCATION, Image.Initˉserviceˉimageˉbytes.AsSpan(),
            Image.Initˉserviceˉdigest.AsSpan(), Image.Admittedˉprogramˉdigest.AsSpan(),
            Image.Admittedˉprogramˉbytes.AsSpan(), 0, Initˉdefinition);
        var Client = Kernelˉprocessˉplanner.Plan(
            Paging, CLIENT_ALLOCATION, Image.Clientˉimageˉbytes.AsSpan(),
            Image.Interpreterˉdigest.AsSpan(), Image.Admittedˉprogramˉdigest.AsSpan(),
            [], Image.Bootˉresourceˉserviceˉoffset,
            Clientˉdefinition);
        True(First.Success, First.Diagnostics.IsEmpty ? "Process planning failed." : First.Diagnostics[0].Message);
        True(Second.Success, "Repeated process planning failed.");
        True(Client.Success, "Client process planning failed.");
        var Plan = First.Plan!;
        Sequenceˉequal(Plan.Tableˉbytes, Second.Plan!.Tableˉbytes);
        Sequenceˉequal(Plan.Processˉrecord, Second.Plan.Processˉrecord);
        Equal(ALLOCATION, Plan.Rootˉaddress);
        Equal(ALLOCATION + 0x4000UL, Plan.Userˉcodeˉaddress);
        Equal(ALLOCATION + 0x5000UL, Plan.Userˉstackˉaddress);
        Equal(ALLOCATION + 0x6000UL, Plan.Userˉdataˉaddress);
        Equal(ALLOCATION + 0x7000UL, Plan.Userˉruntimeˉinputˉaddress);
        Equal(CLIENT_ALLOCATION, Client.Plan!.Rootˉaddress);
        Equal(CLIENT_ALLOCATION + 0x24000UL, Client.Plan.Userˉstackˉaddress);
        Equal(CLIENT_ALLOCATION + 0x28000UL, Client.Plan.Userˉdataˉaddress);
        Equal(CLIENT_ALLOCATION + 0x29000UL, Client.Plan.Userˉruntimeˉinputˉaddress);
        Equal(Kernelˉprocessˉcontract.CLIENT_CODE_PAGES, Client.Plan.Userˉcodeˉpages);
        Equal(Kernelˉprocessˉcontract.CLIENT_STACK_PAGES, Client.Plan.Userˉstackˉpages);
        True(Client.Plan.Rootˉaddress != Plan.Rootˉaddress, "The service and client share a root.");

        Equal(ALLOCATION + 0x1000UL | 7UL, Kernelˉprocessˉplanner.Readˉentry(Plan, 0, 0));
        Equal(ALLOCATION + 0x2000UL | 7UL, Kernelˉprocessˉplanner.Readˉentry(Plan, 1, 0));
        Equal(ALLOCATION + 0x3000UL | 7UL, Kernelˉprocessˉplanner.Readˉentry(Plan, 2, 4));
        Equal(Plan.Userˉcodeˉaddress | 5UL, Kernelˉprocessˉplanner.Readˉentry(Plan, 3, 4));
        Equal(
            Plan.Userˉstackˉaddress | 7UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            Kernelˉprocessˉplanner.Readˉentry(Plan, 3, 5));
        Equal(
            Plan.Userˉdataˉaddress | 7UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            Kernelˉprocessˉplanner.Readˉentry(Plan, 3, 6));
        Equal(
            Plan.Userˉruntimeˉinputˉaddress | 5UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            Kernelˉprocessˉplanner.Readˉentry(Plan, 3, 7));
        Equal(
            Client.Plan.Userˉstackˉaddress | 7UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            Kernelˉprocessˉplanner.Readˉentry(Client.Plan, 3, 36));
        Equal(
            Client.Plan.Userˉstackˉaddress + Kernelˉpagingˉcontract.PAGE_BYTES |
                7UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            Kernelˉprocessˉplanner.Readˉentry(Client.Plan, 3, 37));
        Equal(
            Client.Plan.Userˉstackˉaddress + 2 * Kernelˉpagingˉcontract.PAGE_BYTES |
                7UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            Kernelˉprocessˉplanner.Readˉentry(Client.Plan, 3, 38));
        Equal(
            Client.Plan.Userˉstackˉaddress + 3 * Kernelˉpagingˉcontract.PAGE_BYTES |
                7UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            Kernelˉprocessˉplanner.Readˉentry(Client.Plan, 3, 39));
        Equal(
            Client.Plan.Userˉdataˉaddress | 7UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            Kernelˉprocessˉplanner.Readˉentry(Client.Plan, 3, 40));
        Equal(0UL, Kernelˉprocessˉplanner.Readˉentry(Client.Plan, 3, 41));

        var Userˉleaves = 0;
        for (var Index = 0; Index < 512; Index++)
        {
            var Entry = Kernelˉprocessˉplanner.Readˉentry(Plan, 3, Index);
            if ((Entry & Kernelˉprocessˉcontract.ENTRY_USER) != 0)
            {
                Userˉleaves++;
            }
            True(
                (Entry & (Kernelˉpagingˉcontract.ENTRY_PRESENT |
                    Kernelˉpagingˉcontract.ENTRY_WRITABLE |
                    Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE)) !=
                (Kernelˉpagingˉcontract.ENTRY_PRESENT |
                    Kernelˉpagingˉcontract.ENTRY_WRITABLE),
                "The process table contains a writable executable leaf.");
        }
        Equal(4, Userˉleaves);
        var Clientˉuserˉleaves = 0;
        for (var Index = 0; Index < 512; Index++)
        {
            var Entry = Kernelˉprocessˉplanner.Readˉentry(Client.Plan, 3, Index);
            if ((Entry & Kernelˉprocessˉcontract.ENTRY_USER) != 0)
            {
                Clientˉuserˉleaves++;
            }
            True(
                (Entry & (Kernelˉpagingˉcontract.ENTRY_PRESENT |
                    Kernelˉpagingˉcontract.ENTRY_WRITABLE |
                    Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE)) !=
                (Kernelˉpagingˉcontract.ENTRY_PRESENT |
                    Kernelˉpagingˉcontract.ENTRY_WRITABLE),
                "The interpreter table contains a writable executable leaf.");
        }
        Equal(37, Clientˉuserˉleaves);
        Sequenceˉequal(Image.Initˉserviceˉimageˉbytes,
            Plan.Userˉcodeˉbytes[..Image.Initˉserviceˉimageˉbytes.Length]);
        Sequenceˉequal(Image.Clientˉimageˉbytes,
            Client.Plan.Userˉcodeˉbytes[..Image.Clientˉimageˉbytes.Length]);
        True(Plan.Userˉstackˉbytes.All(Value => Value == 0), "The initial user stack is not zeroed.");
        Equal((int)(Kernelˉprocessˉcontract.CLIENT_STACK_PAGES * Kernelˉpagingˉcontract.PAGE_BYTES),
            Client.Plan.Userˉstackˉbytes.Length);
        True(Client.Plan.Userˉstackˉbytes.All(Value => Value == 0), "The interpreter user stack is not zeroed.");
        Sequenceˉequal(Image.Admittedˉprogramˉbytes,
            Plan.Userˉruntimeˉinputˉbytes[..Image.Admittedˉprogramˉbytes.Length]);
        True(
            Plan.Userˉruntimeˉinputˉbytes[Image.Admittedˉprogramˉbytes.Length..]
                .All(Value => Value == 0),
            "The unused runtime-input page tail is not zeroed.");
        True(Client.Plan.Userˉruntimeˉinputˉbytes.IsEmpty,
            "The interpreter owns runtime-input bytes before init grants them.");
        Equal((uint)Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION,
            BinaryPrimitives.ReadUInt32LittleEndian(Plan.Userˉdataˉbytes.AsSpan()));
        Equal((uint)Nativeˉexecutionˉcontextˉcontract.SIZE,
            BinaryPrimitives.ReadUInt32LittleEndian(Plan.Userˉdataˉbytes.AsSpan()[4..]));
        Equal(0UL, BinaryPrimitives.ReadUInt64LittleEndian(
            Plan.Userˉdataˉbytes.AsSpan()[Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET..]));
        Equal(0UL, BinaryPrimitives.ReadUInt64LittleEndian(
            Plan.Userˉdataˉbytes.AsSpan()[Nativeˉexecutionˉcontextˉcontract.FILE_INPUT_TABLE_POINTER_OFFSET..]));
        Equal(0UL,
            BinaryPrimitives.ReadUInt64LittleEndian(
                Client.Plan.Userˉdataˉbytes.AsSpan()[
                    Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET..]));
        Equal(0UL,
            BinaryPrimitives.ReadUInt64LittleEndian(
                Client.Plan.Userˉdataˉbytes.AsSpan()[
                    Nativeˉexecutionˉcontextˉcontract.FILE_INPUT_TABLE_POINTER_OFFSET..]));

        var Grant = Kernelˉresourceˉgrantˉplanner.Plan(
            Plan, Client.Plan, Image.Admittedˉprogramˉdigest.AsSpan(),
            Image.Admittedˉprogramˉbytes.Length, Image.Bootˉresourceˉserviceˉoffset);
        var Repeatedˉgrant = Kernelˉresourceˉgrantˉplanner.Plan(
            Plan, Client.Plan, Image.Admittedˉprogramˉdigest.AsSpan(),
            Image.Admittedˉprogramˉbytes.Length, Image.Bootˉresourceˉserviceˉoffset);
        True(Grant.Success, Grant.Diagnostics.IsEmpty
            ? "Resource grant planning failed."
            : Grant.Diagnostics[0].Message);
        True(Repeatedˉgrant.Success, "Repeated resource grant planning failed.");
        Sequenceˉequal(Grant.Plan!.Resourceˉrecord, Repeatedˉgrant.Plan!.Resourceˉrecord);
        Sequenceˉequal(Grant.Plan.Clientˉtableˉbytes, Repeatedˉgrant.Plan.Clientˉtableˉbytes);
        Sequenceˉequal(Grant.Plan.Clientˉdataˉbytes, Repeatedˉgrant.Plan.Clientˉdataˉbytes);
        Equal(
            Plan.Userˉruntimeˉinputˉaddress | 5UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            BinaryPrimitives.ReadUInt64LittleEndian(Grant.Plan.Clientˉtableˉbytes.AsSpan()[
                checked((int)(Kernelˉprocessˉcontract.USER_PT_PAGE *
                    Kernelˉpagingˉcontract.PAGE_BYTES + 41UL * sizeof(ulong)))..]));
        var Serviceˉtable = Grant.Plan.Clientˉdataˉbytes.AsSpan()[
            (int)Kernelˉprocessˉcontract.RUNTIME_SERVICE_TABLE_OFFSET..];
        Equal(Nativeˉserviceˉtableˉcontract.FORMAT_VERSION,
            BinaryPrimitives.ReadUInt32LittleEndian(Serviceˉtable));
        Equal(Nativeˉserviceˉtableˉcontract.SIZE,
            BinaryPrimitives.ReadUInt32LittleEndian(Serviceˉtable[4..]));
        Equal(Client.Plan.Userˉcodeˉaddress + Image.Bootˉresourceˉserviceˉoffset,
            BinaryPrimitives.ReadUInt64LittleEndian(
                Serviceˉtable[Nativeˉserviceˉtableˉcontract.FILE_READ_BYTES_POINTER_OFFSET..]));
        var Serviceˉtableˉbytes = Serviceˉtable[..(int)Nativeˉserviceˉtableˉcontract.SIZE].ToArray();
        Serviceˉtableˉbytes.AsSpan(
            Nativeˉserviceˉtableˉcontract.FILE_READ_BYTES_POINTER_OFFSET,
            sizeof(ulong)).Clear();
        True(Serviceˉtableˉbytes[8..].All(Value => Value == 0),
            "An undeclared native service slot is nonzero.");
        Equal(Client.Plan.Userˉdataˉaddress + Kernelˉprocessˉcontract.RUNTIME_SERVICE_TABLE_OFFSET,
            BinaryPrimitives.ReadUInt64LittleEndian(
                Grant.Plan.Clientˉdataˉbytes.AsSpan()[
                    Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET..]));
        Equal(Client.Plan.Userˉdataˉaddress + Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET,
            BinaryPrimitives.ReadUInt64LittleEndian(
                Grant.Plan.Clientˉdataˉbytes.AsSpan()[
                    Nativeˉexecutionˉcontextˉcontract.FILE_INPUT_TABLE_POINTER_OFFSET..]));
        var Resourceˉtable = Grant.Plan.Clientˉdataˉbytes.AsSpan()[
            (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET..];
        Equal(Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_MAGIC,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉtable));
        Equal(Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_VERSION,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉtable[4..]));
        Equal(Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_BYTES,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉtable[8..]));
        Equal(Client.Plan.Userˉruntimeˉinputˉaddress,
            BinaryPrimitives.ReadUInt64LittleEndian(
                Resourceˉtable[(int)Kernelˉprocessˉcontract.BOOT_RESOURCE_DATA_POINTER_OFFSET..]));
        Equal((uint)Image.Admittedˉprogramˉbytes.Length,
            BinaryPrimitives.ReadUInt32LittleEndian(
                Resourceˉtable[(int)Kernelˉprocessˉcontract.BOOT_RESOURCE_DATA_LENGTH_OFFSET..]));
        Equal(0U, BinaryPrimitives.ReadUInt32LittleEndian(
            Resourceˉtable[(int)Kernelˉprocessˉcontract.BOOT_RESOURCE_RESERVED_OFFSET..]));
        var Resourceˉrecord = Grant.Plan.Resourceˉrecord.AsSpan();
        Equal(Kernelˉprocessˉcontract.RESOURCE_MAGIC,
            BinaryPrimitives.ReadUInt64LittleEndian(Resourceˉrecord));
        Equal(Kernelˉprocessˉcontract.RESOURCE_VERSION,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉrecord[8..]));
        Equal(Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉrecord[12..]));
        Equal(Kernelˉprocessˉcontract.RESOURCE_STATE_BORROWED,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET..]));
        Equal(Kernelˉprocessˉcontract.RESOURCE_ID,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_ID_OFFSET..]));
        Equal(Kernelˉprocessˉcontract.INIT_PROCESS_ID,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_OWNER_OFFSET..]));
        Equal(Kernelˉprocessˉcontract.CLIENT_PROCESS_ID,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET..]));
        Equal(Plan.Userˉruntimeˉinputˉaddress,
            BinaryPrimitives.ReadUInt64LittleEndian(Resourceˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_SOURCE_ADDRESS_OFFSET..]));
        Equal((uint)Image.Admittedˉprogramˉbytes.Length,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET..]));
        Equal(Client.Plan.Userˉruntimeˉinputˉaddress,
            BinaryPrimitives.ReadUInt64LittleEndian(Resourceˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_TARGET_ADDRESS_OFFSET..]));
        Equal(Kernelˉprocessˉcontract.RESOURCE_FLAGS,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_FLAGS_OFFSET..]));
        Equal(Client.Plan.Rootˉaddress,
            BinaryPrimitives.ReadUInt64LittleEndian(Resourceˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_TARGET_ROOT_OFFSET..]));
        Equal(Client.Plan.Userˉdataˉaddress,
            BinaryPrimitives.ReadUInt64LittleEndian(Resourceˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_TARGET_DATA_OFFSET..]));
        Equal(Client.Plan.Userˉcodeˉaddress + Image.Bootˉresourceˉserviceˉoffset,
            BinaryPrimitives.ReadUInt64LittleEndian(Resourceˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_SERVICE_ADDRESS_OFFSET..]));
        Sequenceˉequal(Image.Admittedˉprogramˉdigest,
            Grant.Plan.Resourceˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_DIGEST_OFFSET..
                (int)Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET]);
        Equal(1U, BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉrecord[
            (int)Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET..]));
        Equal(1U, BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉrecord[
            (int)Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET..]));
        Equal(Client.Plan.Rootˉaddress + Kernelˉprocessˉcontract.USER_PT_PAGE *
                Kernelˉpagingˉcontract.PAGE_BYTES + 41UL * sizeof(ulong),
            BinaryPrimitives.ReadUInt64LittleEndian(Resourceˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_TARGET_PTE_OFFSET..]));

        var Revoked = Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Client.Plan, Grant.Plan,
            Kernelˉprocessˉcontract.PROCESS_STATE_EXITED,
            Kernelˉprocessˉcontract.THREAD_STATE_EXITED);
        var Repeatedˉrevocation = Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Client.Plan, Grant.Plan,
            Kernelˉprocessˉcontract.PROCESS_STATE_EXITED,
            Kernelˉprocessˉcontract.THREAD_STATE_EXITED);
        var Faultˉrevocation = Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Client.Plan, Grant.Plan,
            Kernelˉprocessˉcontract.PROCESS_STATE_FAULTED,
            Kernelˉprocessˉcontract.THREAD_STATE_FAULTED);
        var Targetˉpteˉoffset = checked((int)(Kernelˉprocessˉcontract.USER_PT_PAGE *
            Kernelˉpagingˉcontract.PAGE_BYTES + 41UL * sizeof(ulong)));
        var Accessedˉtables = Grant.Plan.Clientˉtableˉbytes.ToArray();
        BinaryPrimitives.WriteUInt64LittleEndian(
            Accessedˉtables.AsSpan(Targetˉpteˉoffset),
            BinaryPrimitives.ReadUInt64LittleEndian(Accessedˉtables.AsSpan(Targetˉpteˉoffset)) |
                Kernelˉpagingˉcontract.ENTRY_ACCESSED);
        var Accessedˉrevocation = Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Client.Plan,
            Grant.Plan with { Clientˉtableˉbytes = Accessedˉtables.ToImmutableArray() },
            Kernelˉprocessˉcontract.PROCESS_STATE_EXITED,
            Kernelˉprocessˉcontract.THREAD_STATE_EXITED);
        True(Revoked.Success, Revoked.Diagnostics.IsEmpty
            ? "Resource revocation planning failed."
            : Revoked.Diagnostics[0].Message);
        True(Repeatedˉrevocation.Success, "Repeated resource revocation planning failed.");
        True(Faultˉrevocation.Success, "Fault-terminal resource revocation planning failed.");
        True(Accessedˉrevocation.Success,
            "Hardware-accessed resource revocation planning failed.");
        Sequenceˉequal(Revoked.Plan!.Resourceˉrecord, Repeatedˉrevocation.Plan!.Resourceˉrecord);
        Sequenceˉequal(Revoked.Plan.Clientˉtableˉbytes, Repeatedˉrevocation.Plan.Clientˉtableˉbytes);
        Sequenceˉequal(Revoked.Plan.Clientˉdataˉbytes, Repeatedˉrevocation.Plan.Clientˉdataˉbytes);
        Sequenceˉequal(Revoked.Plan.Resourceˉrecord, Faultˉrevocation.Plan!.Resourceˉrecord);
        Sequenceˉequal(Revoked.Plan.Clientˉtableˉbytes, Faultˉrevocation.Plan.Clientˉtableˉbytes);
        Sequenceˉequal(Revoked.Plan.Clientˉdataˉbytes, Faultˉrevocation.Plan.Clientˉdataˉbytes);
        Sequenceˉequal(Revoked.Plan.Resourceˉrecord, Accessedˉrevocation.Plan!.Resourceˉrecord);
        Sequenceˉequal(Revoked.Plan.Clientˉtableˉbytes, Accessedˉrevocation.Plan.Clientˉtableˉbytes);
        Sequenceˉequal(Revoked.Plan.Clientˉdataˉbytes, Accessedˉrevocation.Plan.Clientˉdataˉbytes);
        var Revokedˉrecord = Revoked.Plan.Resourceˉrecord.AsSpan();
        Equal(Kernelˉprocessˉcontract.RESOURCE_STATE_OWNED,
            BinaryPrimitives.ReadUInt32LittleEndian(Revokedˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET..]));
        Equal(0U, BinaryPrimitives.ReadUInt32LittleEndian(Revokedˉrecord[
            (int)Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET..]));
        Equal(1U, BinaryPrimitives.ReadUInt32LittleEndian(Revokedˉrecord[
            (int)Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET..]));
        Equal(0U, BinaryPrimitives.ReadUInt32LittleEndian(Revokedˉrecord[
            (int)Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET..]));
        Sequenceˉequal(Image.Admittedˉprogramˉdigest,
            Revoked.Plan.Resourceˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_DIGEST_OFFSET..
                (int)Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET]);
        Equal(0UL, BinaryPrimitives.ReadUInt64LittleEndian(
            Revoked.Plan.Clientˉtableˉbytes.AsSpan()[
                Targetˉpteˉoffset..]));
        Equal(0UL, BinaryPrimitives.ReadUInt64LittleEndian(
            Revoked.Plan.Clientˉdataˉbytes.AsSpan()[
                Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET..]));
        Equal(0UL, BinaryPrimitives.ReadUInt64LittleEndian(
            Revoked.Plan.Clientˉdataˉbytes.AsSpan()[
                Nativeˉexecutionˉcontextˉcontract.FILE_INPUT_TABLE_POINTER_OFFSET..]));
        True(!Revoked.Plan.Clientˉdataˉbytes.AsSpan(
                (int)Kernelˉprocessˉcontract.RUNTIME_SERVICE_TABLE_OFFSET,
                (int)Nativeˉserviceˉtableˉcontract.SIZE).ContainsAnyExcept((byte)0),
            "The revoked client service table was not cleared.");
        True(!Revoked.Plan.Clientˉdataˉbytes.AsSpan(
                (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET,
                (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_BYTES).ContainsAnyExcept((byte)0),
            "The revoked client resource table was not cleared.");

        Equal("WVOS6201", Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Client.Plan, Grant.Plan,
            Kernelˉprocessˉcontract.PROCESS_STATE_RUNNING,
            Kernelˉprocessˉcontract.THREAD_STATE_RUNNING).Diagnostics[0].Code);
        var Changedˉgrantˉrecord = Grant.Plan with
        {
            Resourceˉrecord = Mutate(Grant.Plan.Resourceˉrecord, 0).ToImmutableArray(),
        };
        Equal("WVOS6202", Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Client.Plan, Changedˉgrantˉrecord,
            Kernelˉprocessˉcontract.PROCESS_STATE_EXITED,
            Kernelˉprocessˉcontract.THREAD_STATE_EXITED).Diagnostics[0].Code);
        var Outsideˉpteˉrecord = Grant.Plan.Resourceˉrecord.ToArray();
        BinaryPrimitives.WriteUInt64LittleEndian(
            Outsideˉpteˉrecord.AsSpan((int)Kernelˉprocessˉcontract.RESOURCE_TARGET_PTE_OFFSET),
            Client.Plan.Rootˉaddress - sizeof(ulong));
        Equal("WVOS6203", Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Client.Plan,
            Grant.Plan with { Resourceˉrecord = Outsideˉpteˉrecord.ToImmutableArray() },
            Kernelˉprocessˉcontract.PROCESS_STATE_EXITED,
            Kernelˉprocessˉcontract.THREAD_STATE_EXITED).Diagnostics[0].Code);
        var Changedˉgrantˉtables = Grant.Plan with
        {
            Clientˉtableˉbytes = Mutate(Grant.Plan.Clientˉtableˉbytes,
                checked((int)(Kernelˉprocessˉcontract.USER_PT_PAGE *
                    Kernelˉpagingˉcontract.PAGE_BYTES + 41UL * sizeof(ulong)))).ToImmutableArray(),
        };
        Equal("WVOS6203", Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Client.Plan, Changedˉgrantˉtables,
            Kernelˉprocessˉcontract.PROCESS_STATE_FAULTED,
            Kernelˉprocessˉcontract.THREAD_STATE_FAULTED).Diagnostics[0].Code);
        var Changedˉgrantˉdata = Grant.Plan with
        {
            Clientˉdataˉbytes = Mutate(Grant.Plan.Clientˉdataˉbytes,
                (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET).ToImmutableArray(),
        };
        Equal("WVOS6203", Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Client.Plan, Changedˉgrantˉdata,
            Kernelˉprocessˉcontract.PROCESS_STATE_EXITED,
            Kernelˉprocessˉcontract.THREAD_STATE_EXITED).Diagnostics[0].Code);
        var Replayˉrelease = new Kernelˉresourceˉgrantˉplan(
            Revoked.Plan.Resourceˉrecord,
            Revoked.Plan.Clientˉtableˉbytes,
            Revoked.Plan.Clientˉdataˉbytes);
        Equal("WVOS6202", Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Client.Plan, Replayˉrelease,
            Kernelˉprocessˉcontract.PROCESS_STATE_EXITED,
            Kernelˉprocessˉcontract.THREAD_STATE_EXITED).Diagnostics[0].Code);
        Equal(Kernelˉprocessˉcontract.RECORD_MAGIC,
            BinaryPrimitives.ReadUInt64LittleEndian(Plan.Processˉrecord.AsSpan()));
        Equal(Kernelˉprocessˉcontract.PROCESS_STATE_READY,
            BinaryPrimitives.ReadUInt32LittleEndian(Plan.Processˉrecord.AsSpan()[16..]));
        Sequenceˉequal(Image.Initˉserviceˉdigest, Plan.Processˉrecord[32..64]);
        Sequenceˉequal(Image.Interpreterˉdigest, Client.Plan.Processˉrecord[32..64]);
        Sequenceˉequal(Image.Admittedˉprogramˉdigest,
            Plan.Processˉrecord[(int)Kernelˉprocessˉcontract.PROGRAM_DIGEST_OFFSET..
                (int)Kernelˉprocessˉcontract.CODE_PAGE_COUNT_OFFSET]);
        Sequenceˉequal(Image.Admittedˉprogramˉdigest,
            Client.Plan.Processˉrecord[(int)Kernelˉprocessˉcontract.PROGRAM_DIGEST_OFFSET..
                (int)Kernelˉprocessˉcontract.CODE_PAGE_COUNT_OFFSET]);
        Equal(Kernelˉprocessˉcontract.RUNTIME_KIND_BYTECODE_INTERPRETER,
            BinaryPrimitives.ReadUInt32LittleEndian(
                Client.Plan.Processˉrecord.AsSpan()[(int)Kernelˉprocessˉcontract.RUNTIME_KIND_OFFSET..]));
        Equal(Kernelˉprocessˉcontract.RUNTIME_PROFILE_BOOT_RESOURCE_OWNER,
            BinaryPrimitives.ReadUInt32LittleEndian(
                Plan.Processˉrecord.AsSpan()[(int)Kernelˉprocessˉcontract.RUNTIME_PROFILE_OFFSET..]));
        Equal(Kernelˉprocessˉcontract.RUNTIME_PROFILE_GRANTED_BOOT_RESOURCE_INTERPRETER,
            BinaryPrimitives.ReadUInt32LittleEndian(
                Client.Plan.Processˉrecord.AsSpan()[(int)Kernelˉprocessˉcontract.RUNTIME_PROFILE_OFFSET..]));
        Equal((uint)Kernelˉprocessˉcontract.CLIENT_STACK_PAGES,
            BinaryPrimitives.ReadUInt32LittleEndian(
                Client.Plan.Processˉrecord.AsSpan()[(int)Kernelˉprocessˉcontract.STACK_PAGE_COUNT_OFFSET..]));
        Equal(Kernelˉprocessˉcontract.INIT_CAPABILITY_RIGHTS,
            BinaryPrimitives.ReadUInt32LittleEndian(Plan.Processˉrecord.AsSpan()[120..]));
        Equal(Kernelˉprocessˉcontract.CAPABILITY_RIGHT_SEND,
            BinaryPrimitives.ReadUInt32LittleEndian(Client.Plan.Processˉrecord.AsSpan()[120..]));
        Equal(CHANNEL, BinaryPrimitives.ReadUInt64LittleEndian(
            Plan.Processˉrecord.AsSpan()[(int)Kernelˉprocessˉcontract.CHANNEL_ADDRESS_OFFSET..]));

        Processˉplanˉfails(Paging, 0, Image.Initˉserviceˉimageˉbytes,
            Image.Initˉserviceˉdigest, Image.Admittedˉprogramˉdigest,
            Image.Admittedˉprogramˉbytes, 0, Initˉdefinition, "WVOS6001");
        Processˉplanˉfails(Paging, 0x009F_C000, Image.Initˉserviceˉimageˉbytes,
            Image.Initˉserviceˉdigest, Image.Admittedˉprogramˉdigest,
            Image.Admittedˉprogramˉbytes, 0, Initˉdefinition, "WVOS6002");
        Processˉplanˉfails(Paging, ALLOCATION, [], Image.Initˉserviceˉdigest,
            Image.Admittedˉprogramˉdigest, Image.Admittedˉprogramˉbytes, 0,
            Initˉdefinition, "WVOS6003");
        Processˉplanˉfails(Paging, ALLOCATION, Image.Initˉserviceˉimageˉbytes,
            Image.Initˉserviceˉdigest[..^1], Image.Admittedˉprogramˉdigest,
            Image.Admittedˉprogramˉbytes, 0, Initˉdefinition, "WVOS6004");
        Processˉplanˉfails(Paging, EXECUTABLE, Image.Initˉserviceˉimageˉbytes,
            Image.Initˉserviceˉdigest, Image.Admittedˉprogramˉdigest,
            Image.Admittedˉprogramˉbytes, 0, Initˉdefinition, "WVOS6005");
        Processˉplanˉfails(Paging, ALLOCATION, Image.Initˉserviceˉimageˉbytes,
            Image.Initˉserviceˉdigest, Image.Admittedˉprogramˉdigest,
            Image.Admittedˉprogramˉbytes, 0,
            Initˉdefinition with { Capabilityˉrights = 3 }, "WVOS6006");
        Processˉplanˉfails(Paging, CLIENT_ALLOCATION, Image.Clientˉimageˉbytes,
            Image.Interpreterˉdigest,
            ImmutableArray.CreateRange(new byte[Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES]),
            [], Image.Bootˉresourceˉserviceˉoffset, Clientˉdefinition, "WVOS6007");
        Processˉplanˉfails(Paging, CLIENT_ALLOCATION, Image.Clientˉimageˉbytes,
            Image.Interpreterˉdigest, Image.Admittedˉprogramˉdigest[..^1], Clientˉdefinition, "WVOS6007");
        Processˉplanˉfails(Paging, CLIENT_ALLOCATION, Image.Clientˉimageˉbytes,
            Image.Interpreterˉdigest, Image.Admittedˉprogramˉdigest,
            Image.Admittedˉprogramˉbytes,
            Image.Bootˉresourceˉserviceˉoffset, Clientˉdefinition, "WVOS6008");
        Processˉplanˉfails(Paging, ALLOCATION, Image.Initˉserviceˉimageˉbytes,
            Image.Initˉserviceˉdigest, Image.Admittedˉprogramˉdigest,
            Mutate(Image.Admittedˉprogramˉbytes, 0).ToImmutableArray(),
            0, Initˉdefinition, "WVOS6008");
        Processˉplanˉfails(Paging, CLIENT_ALLOCATION, Image.Clientˉimageˉbytes,
            Image.Interpreterˉdigest, Image.Admittedˉprogramˉdigest, [],
            (uint)Image.Clientˉimageˉbytes.Length,
            Clientˉdefinition, "WVOS6008");

        var Changedˉowner = Plan with
        {
            Userˉruntimeˉinputˉbytes = Mutate(Plan.Userˉruntimeˉinputˉbytes, 0).ToImmutableArray(),
        };
        Equal("WVOS6101", Kernelˉresourceˉgrantˉplanner.Plan(
            Changedˉowner, Client.Plan, Image.Admittedˉprogramˉdigest.AsSpan(),
            Image.Admittedˉprogramˉbytes.Length, Image.Bootˉresourceˉserviceˉoffset)
            .Diagnostics[0].Code);
        var Premappedˉclient = Client.Plan with { Tableˉbytes = Grant.Plan.Clientˉtableˉbytes };
        Equal("WVOS6102", Kernelˉresourceˉgrantˉplanner.Plan(
            Plan, Premappedˉclient, Image.Admittedˉprogramˉdigest.AsSpan(),
            Image.Admittedˉprogramˉbytes.Length, Image.Bootˉresourceˉserviceˉoffset)
            .Diagnostics[0].Code);
        Equal("WVOS6103", Kernelˉresourceˉgrantˉplanner.Plan(
            Plan, Client.Plan, Image.Admittedˉprogramˉdigest.AsSpan(),
            Image.Admittedˉprogramˉbytes.Length, (uint)Client.Plan.Userˉcodeˉbytes.Length)
            .Diagnostics[0].Code);
    }

    private static void Kernelˉprocessˉboundaryˉemitsˉverifiedˉobject()
    {
        var Admission = Kernelˉwvbˉadmission.Build();
        var Normalˉimage = Kernelˉprocessˉimage.Build(Admission, false);
        var Faultˉimage = Kernelˉprocessˉimage.Build(Admission, true);
        var First = Kernelˉprocessˉx64.Build(Normalˉimage, false);
        var Second = Kernelˉprocessˉx64.Build(Normalˉimage, false);
        var Faultˉfirst = Kernelˉprocessˉx64.Build(Faultˉimage, true);
        var Faultˉsecond = Kernelˉprocessˉx64.Build(Faultˉimage, true);
        Equal(138_751, First.Objectˉbytes.Length);
        Equal("19da37cdc044505a92410449a14e72c35ed8573b409c095b0b9a8a8f9d21f065",
            Objectˉdigest.Calculateˉsha256(First.Objectˉbytes.AsSpan()));
        Equal(7_885, First.Codeˉbytes.Length);
        Equal("52a5729eca1c36d5ba004dc69fc30d250eea91ad59888d04c4b0c240af9cbfac",
            Objectˉdigest.Calculateˉsha256(First.Codeˉbytes.AsSpan()));
        Equal(138_783, Faultˉfirst.Objectˉbytes.Length);
        Equal("bedd2a06969d3df295efe8eefa626dd1c97737731ef1056fbcb6df152f259138",
            Objectˉdigest.Calculateˉsha256(Faultˉfirst.Objectˉbytes.AsSpan()));
        Equal(7_917, Faultˉfirst.Codeˉbytes.Length);
        Equal("9df1702699d3f9da8a3926c2406f84cff06e638b6a176401c70b82ddce4e634c",
            Objectˉdigest.Calculateˉsha256(Faultˉfirst.Codeˉbytes.AsSpan()));
        Equal(13, First.Relocations.Length);
        Equal(13, Faultˉfirst.Relocations.Length);
        Sequenceˉequal(First.Objectˉbytes, Second.Objectˉbytes);
        Sequenceˉequal(First.Codeˉbytes, Second.Codeˉbytes);
        Sequenceˉequal(Faultˉfirst.Objectˉbytes, Faultˉsecond.Objectˉbytes);
        True(!First.Objectˉbytes.AsSpan().SequenceEqual(Faultˉfirst.Objectˉbytes.AsSpan()),
            "The normal and deliberate-fault process objects are identical.");
        Processˉmachineˉrejects(Normalˉimage with
        {
            Bootˉresourceˉserviceˉobjectˉbytes = Mutate(
                Normalˉimage.Bootˉresourceˉserviceˉobjectˉbytes, 0).ToImmutableArray(),
        });
        Processˉmachineˉrejects(Normalˉimage with
        {
            Clientˉimageˉbytes = Mutate(
                Normalˉimage.Clientˉimageˉbytes,
                (int)Normalˉimage.Bootˉresourceˉserviceˉoffset).ToImmutableArray(),
        });
        Processˉmachineˉrejects(Normalˉimage with
        {
            Admittedˉprogramˉbytes = Mutate(
                Normalˉimage.Admittedˉprogramˉbytes, 0).ToImmutableArray(),
        });
        Processˉmachineˉrejects(Normalˉimage with
        {
            Admittedˉprogramˉdigest = Mutate(
                Normalˉimage.Admittedˉprogramˉdigest, 0).ToImmutableArray(),
        });

        var Object = Objectˉcodec.Readˉandˉverify(First.Objectˉbytes.AsSpan()).Value;
        Equal(4, Object.Sections.Length);
        True(Object.Sections[0].Kind == Objectˉsectionˉkind.Code, "The process machine section is not code.");
        True(Object.Sections[1].Kind == Objectˉsectionˉkind.Readˉonlyˉdata,
            "The user image is not retained as read-only link data.");
        Sequenceˉequal(First.Codeˉbytes, Object.Sections[0].Data);
        Sequenceˉequal(Normalˉimage.Initˉserviceˉimageˉbytes, Object.Sections[1].Data);
        Sequenceˉequal(Normalˉimage.Clientˉimageˉbytes, Object.Sections[2].Data);
        Sequenceˉequal(Normalˉimage.Admittedˉprogramˉbytes, Object.Sections[3].Data);
        Equal(13, Object.Symbols.Length);
        Equal("Windvale_resource_init_boot", Object.Symbols[2].Name);
        Equal(Kernelˉprocessˉcontract.ENTER_SYMBOL, Object.Symbols[3].Name);
        Equal(Kernelˉprocessˉcontract.EXCEPTION_ENTRY_SYMBOL, Object.Symbols[4].Name);
        Equal(Kernelˉprocessˉcontract.SYSCALL_ENTRY_SYMBOL, Object.Symbols[5].Name);
        Equal(Kernelˉmemoryˉcontract.ALLOCATE_PAGES_SYMBOL, Object.Symbols[6].Name);
        Equal(Kernelˉprocessˉcontract.POLICY_SYMBOL, Object.Symbols[7].Name);
        Equal(Kernelˉexceptionˉcontract.TERMINAL_SYMBOL, Object.Symbols[8].Name);
        Equal(Kernelˉpagingˉcontract.PAGE_TABLE_ACTIVATE_SYMBOL, Object.Symbols[9].Name);
        Equal(Kernelˉprocessˉcontract.EXCEPTION_13_ENTRY_SYMBOL, Object.Symbols[10].Name);
        Equal(Kernelˉprocessˉcontract.EXCEPTION_14_ENTRY_SYMBOL, Object.Symbols[11].Name);
        Equal(Kernelˉprocessˉcontract.EXCEPTION_6_ENTRY_SYMBOL, Object.Symbols[12].Name);
        True(Object.Symbols.Skip(6).All(Symbol => Symbol.Binding == Objectˉsymbolˉbinding.Import),
            "A protected-process machine dependency is not an explicit import.");
        True(Object.Relocations.All(Relocation =>
                Relocation.Kind == Objectˉrelocationˉkind.Relativeˉi32 &&
                Relocation.Sectionˉindex == 0 &&
                Relocation.Addend == -4),
            "The protected-process machine object contains a noncanonical relocation.");
        Equal(4, Countˉsequence(First.Codeˉbytes, [0x48, 0x0F, 0x07]));
        Equal(6, Countˉsequence(First.Codeˉbytes, [0x0F, 0x01, 0xF8]));
        Equal(3, Countˉsequence(Normalˉimage.Initˉserviceˉimageˉbytes, [0x0F, 0x05]));
        Equal(2, Countˉsequence(Normalˉimage.Clientˉimageˉbytes, [0x0F, 0x05]));
        Equal(1, Countˉsequence(Faultˉimage.Clientˉimageˉbytes, [0x0F, 0x05]));
    }

    private static void Userˉfaultˉfirmwareˉprobeˉbuildsˉreproducibly()
    {
        var First = Firmwareˉprobe.Buildˉapplication(Firmwareˉprobeˉscenario.Userˉfault);
        var Second = Firmwareˉprobe.Buildˉapplication(Firmwareˉprobeˉscenario.Userˉfault);
        Sequenceˉequal(First, Second);
        Equal(231_424, First.Length);
        Equal(
            "221c710d741565c7113a7b8c2ea94c66358018e144ae59f583a3c1ce10225494",
            Objectˉdigest.Calculateˉsha256(First.AsSpan()));
        True(!First.AsSpan().SequenceEqual(Firmwareˉprobe.Buildˉapplication().AsSpan()),
            "The normal and deliberate user-fault images are identical.");
        Equal(
            "windvale-os-boot 28\nentry=pass\nsystem-table=pass\nmemory-map=pass\nboot-services=exited\nmemory-owned=pass\nallocator=pass\nkernel-stack=pass\npaging=owned\nwvb-admission=pass\nprocesses=isolated\nresource-grant=pass\nresource-revoked=pass\nwvb-runtime=interpreted\ninit-service=pass\nipc=cross-process\nHello from Windvale\ncpu-exceptions=armed\nnative-context=pass\nnative-wvb=pass\nwindvale-source=pass\nuser-fault=contained\nstatus=pass\nshutdown=poweroff\n",
            Firmwareˉprobe.USER_FAULT_SERIAL_MARKER);
    }


    private static void Firmwareˉprobeˉbuildsˉreproducibly()
    {
        var First = Firmwareˉprobe.Buildˉapplication();
        var Second = Firmwareˉprobe.Buildˉapplication();
        Sequenceˉequal(First, Second);
        Equal(230_912, First.Length);
        Equal(
            "bc5f04c0e75fb217c9339bcc2a391bbe68f9f79ad97c18a93e35e310dab62d46",
            Objectˉdigest.Calculateˉsha256(First.AsSpan()));
        var Verified = Uefiˉapplicationˉverifier.Verify(First.AsSpan());
        True(Verified.Codeˉbytes.Length > 1, "The firmware probe has no executable body.");
        Equal(0u, Verified.Entryˉcodeˉoffset);
    }

    private static void Invalidˉopcodeˉfirmwareˉprobeˉbuildsˉreproducibly()
    {
        var First = Firmwareˉprobe.Buildˉapplication(Firmwareˉprobeˉscenario.Invalidˉopcode);
        var Second = Firmwareˉprobe.Buildˉapplication(Firmwareˉprobeˉscenario.Invalidˉopcode);
        Sequenceˉequal(First, Second);
        Equal(230_912, First.Length);
        Equal(
            "2c9c6c60543d3729f7401720c0b03e98dc2c5e3654e45668bfcd4559650bc543",
            Objectˉdigest.Calculateˉsha256(First.AsSpan()));
        True(
            !First.AsSpan().SequenceEqual(Firmwareˉprobe.Buildˉapplication().AsSpan()),
            "The normal and invalid-opcode firmware scenarios produced identical images.");

        Equal(
            "panic=invalid-opcode\nvector=6\nerror-code=0\nstatus=panic\n",
            Firmwareˉprobe.INVALID_OPCODE_PANIC_MARKER);
        var Code = Uefiˉapplicationˉverifier.Verify(First.AsSpan()).Codeˉbytes;
        var Normalˉcode = Uefiˉapplicationˉverifier.Verify(
            Firmwareˉprobe.Buildˉapplication().AsSpan()).Codeˉbytes;
        Equal(
            Countˉsequence(Normalˉcode, [0x0F, 0x0B]) + 1,
            Countˉsequence(Code, [0x0F, 0x0B]));
    }

    private static void Generalˉprotectionˉfirmwareˉprobeˉbuildsˉreproducibly()
    {
        var First = Firmwareˉprobe.Buildˉapplication(Firmwareˉprobeˉscenario.Generalˉprotection);
        var Second = Firmwareˉprobe.Buildˉapplication(Firmwareˉprobeˉscenario.Generalˉprotection);
        Sequenceˉequal(First, Second);
        Equal(230_912, First.Length);
        Equal(
            "bdccbd123dd457d88c4902f33e727320b08c93e6acd62ac0f706912d5f1163ca",
            Objectˉdigest.Calculateˉsha256(First.AsSpan()));
        True(
            !First.AsSpan().SequenceEqual(Firmwareˉprobe.Buildˉapplication().AsSpan()),
            "The normal and general-protection firmware scenarios produced identical images.");
        True(
            !First.AsSpan().SequenceEqual(
                Firmwareˉprobe.Buildˉapplication(Firmwareˉprobeˉscenario.Invalidˉopcode).AsSpan()),
            "The two explicit fault scenarios produced identical images.");

        Equal(
            "panic=general-protection\nvector=13\nerror-code=0\nstatus=panic\n",
            Firmwareˉprobe.GENERAL_PROTECTION_PANIC_MARKER);
        var Code = Uefiˉapplicationˉverifier.Verify(First.AsSpan()).Codeˉbytes;
        Equal(1, Countˉsequence(
            Code,
            [0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x8A, 0x00]));
        Equal(0, Countˉsequence(
            Uefiˉapplicationˉverifier.Verify(Firmwareˉprobe.Buildˉapplication().AsSpan()).Codeˉbytes,
            [0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x8A, 0x00]));
    }

    private static void Firmwareˉprobeˉcarriesˉcompiledˉsource()
    {
        Equal(
            "windvale-os-boot 28\nentry=pass\nsystem-table=pass\nmemory-map=pass\nboot-services=exited\nmemory-owned=pass\nallocator=pass\nkernel-stack=pass\npaging=owned\nwvb-admission=pass\nprocesses=isolated\nresource-grant=pass\nresource-revoked=pass\nwvb-runtime=interpreted\ninit-service=pass\nipc=cross-process\nHello from Windvale\ncpu-exceptions=armed\nnative-context=pass\nnative-wvb=pass\nwindvale-source=pass\nstatus=pass\nshutdown=poweroff\n",
            Firmwareˉprobe.SERIAL_MARKER);
        var Application = Firmwareˉprobe.Buildˉapplication();
        var Code = Uefiˉapplicationˉverifier.Verify(Application.AsSpan()).Codeˉbytes;
        Equal(1, Countˉsequence(Code, [0x48, 0x81, 0xEC, 0x88, 0x00, 0x00, 0x00]));
        Equal(1, Countˉsequence(Code, [0x49, 0x42, 0x49, 0x20, 0x53, 0x59, 0x53, 0x54]));
        Equal(1, Countˉsequence(Code, [0x42, 0x4F, 0x4F, 0x54, 0x53, 0x45, 0x52, 0x56]));
        Equal(1, Countˉsequence(Code, [0x81, 0x79, 0x0C, 0xF0, 0x00, 0x00, 0x00]));
        Equal(1, Countˉsequence(Code, [0x48, 0x83, 0xB9, 0xE8, 0x00, 0x00, 0x00, 0x00]));
        Equal(2, Countˉsequence(Code, [0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80]));
        Equal(1, Countˉsequence(Code, [0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80]));
        Equal(1, Countˉsequence(Code, [0xC7, 0x44, 0x24, 0x70, 0x03, 0x00, 0x00, 0x00]));
        Equal(3, Countˉsequence(Code, [0xFF, 0x50, 0x38]));
        Equal(1, Countˉsequence(Code, [0xFF, 0x50, 0x40]));
        Equal(1, Countˉsequence(Code, [0xFF, 0x50, 0x48]));
        Equal(1, Countˉsequence(Code, [0xFF, 0x90, 0xE8, 0x00, 0x00, 0x00]));
        Equal(3, Countˉsequence(Code, [0x57, 0x56, 0x4B, 0x48, 0x41, 0x4E, 0x44, 0x31]));
        Equal(4, Countˉsequence(Code, [0x57, 0x56, 0x4B, 0x4D, 0x45, 0x4D, 0x30, 0x35]));
        Equal(1, Countˉsequence(Code, [0xC7, 0x44, 0x24, 0x2C, 0x30, 0x00, 0x00, 0x00]));
        Equal(2, Countˉsequence(Code, [0x48, 0x83, 0xEC, 0x28]));
        Equal(2, Countˉsequence(Code, [0x48, 0x83, 0xEC, 0x78]));
        Equal(1, Countˉsequence(Code, [0x49, 0x8D, 0xA6, 0x00, 0x30, 0x00, 0x00]));
        Equal(7, Countˉsequence(Code, [0xFC, 0xF3, 0x48, 0xAB]));
        Equal(
            1 + Kernelˉexceptionˉcontract.INVALID_OPCODE_PANIC_MARKER.Length +
                Kernelˉexceptionˉcontract.GENERAL_PROTECTION_PANIC_MARKER.Length +
                Kernelˉexceptionˉcontract.MALFORMED_FRAME_PANIC_MARKER.Length,
            Countˉsequence(Code, [0xBA, 0xFD, 0x03, 0x00, 0x00, 0xEC, 0xA8, 0x20, 0x0F, 0x84]));
        Equal(1, Countˉsequence(
            Code,
            [0xBA, 0x04, 0x06, 0x00, 0x00, 0xB8, 0x00, 0x20, 0x00, 0x00,
                0x66, 0xEF, 0xFA, 0xF4, 0xE9]));
        Equal(0, Countˉsequence(
            Code,
            [0xBA, 0xF4, 0x00, 0x00, 0x00, 0xB8, 0x00, 0x00, 0x00, 0x00, 0xEF]));
        Equal(3, Countˉsequence(Code, [0xFA, 0xF4, 0xE9]));
    }

    private const string HELLO_WORLD_SOURCE = """
        module Helloˉwindvale profile system;

        capability console.write_line;

        data Greeting: text = "Hello from Windvale";
        data Memoryˉmarker: text = "memory-owned=pass";
        data Allocatorˉmarker: text = "allocator=pass";
        data Stackˉmarker: text = "kernel-stack=pass";
        data Pagingˉmarker: text = "paging=owned";

        export fn Main() -> i32 {
            console.write_line(Memoryˉmarker);
            console.write_line(Allocatorˉmarker);
            console.write_line(Stackˉmarker);
            console.write_line(Pagingˉmarker);
            console.write_line(Greeting);
            return 0;
        }
        """;

    private static byte[] Buildˉmemoryˉmap(params Memoryˉdescriptorˉinput[] descriptors)
    {
        const int DESCRIPTOR_BYTES = 40;
        var Result = new byte[checked(descriptors.Length * DESCRIPTOR_BYTES)];
        for (var Index = 0; Index < descriptors.Length; Index++)
        {
            var Descriptor = Result.AsSpan(Index * DESCRIPTOR_BYTES, DESCRIPTOR_BYTES);
            BinaryPrimitives.WriteUInt32LittleEndian(Descriptor, descriptors[Index].Type);
            BinaryPrimitives.WriteUInt64LittleEndian(Descriptor[8..], descriptors[Index].Physicalˉaddress);
            BinaryPrimitives.WriteUInt64LittleEndian(Descriptor[24..], descriptors[Index].Pages);
        }
        return Result;
    }

    private static void Kernelˉassemblyˉshimˉbridgesˉmain()
    {
        var First = Kernelˉassemblyˉshim.Buildˉobject();
        var Second = Kernelˉassemblyˉshim.Buildˉobject();
        Sequenceˉequal(First, Second);
        Equal(1_123, First.Length);
        Equal(
            "8a6f54950f15c7331107a5bfa7bd2d863f64b25d395b7cfd9983c31130599363",
            Objectˉdigest.Calculateˉsha256(First.AsSpan()));

        var Object = Objectˉcodec.Readˉandˉverify(First.AsSpan()).Value;
        Equal(1, Object.Sections.Length);
        True(Object.Sections[0].Kind == Objectˉsectionˉkind.Code, "The WVA shim is not code.");
        Sequenceˉequal(
            [(byte)0xE9, 0, 0, 0, 0, 0xE9, 0, 0, 0, 0,
                0x68, 0x0D, 0x00, 0x00, 0x00, 0xE9, 0, 0, 0, 0,
                0x68, 0x00, 0x00, 0x00, 0x00, 0x68, 0x06, 0x00, 0x00, 0x00,
                0xE9, 0, 0, 0, 0,
                0xB9, 0x80, 0x00, 0x00, 0xC0, 0x0F, 0x32, 0x0F, 0xBA, 0xE8, 0x0B,
                0x0F, 0x30, 0x0F, 0x20, 0xC0, 0x48, 0x0F, 0xBA, 0xE8, 0x10,
                0x0F, 0x22, 0xC0, 0xC3,
                0x0F, 0x22, 0xD8, 0x0F, 0x20, 0xD8, 0xC3,
                0x68, 0x0D, 0x00, 0x00, 0x00, 0xE9, 0, 0, 0, 0,
                0x68, 0x0E, 0x00, 0x00, 0x00, 0xE9, 0, 0, 0, 0,
                0x68, 0x00, 0x00, 0x00, 0x00, 0x68, 0x06, 0x00, 0x00, 0x00,
                0xE9, 0, 0, 0, 0,
                0xBA, 0x04, 0x06, 0x00, 0x00, 0xB8, 0x00, 0x20, 0x00, 0x00,
                0x66, 0xEF, 0xFA, 0xF4, 0xE9, 0, 0, 0, 0],
            Object.Sections[0].Data);
        Equal(14, Object.Symbols.Length);
        Equal(X64ˉkernelˉcontract.WRITE_BYTE_SYMBOL, Object.Symbols[0].Name);
        True(Object.Symbols[0].Binding == Objectˉsymbolˉbinding.Export, "The WVA console shim is not exported.");
        Equal(Kernelˉassemblyˉcontract.MAIN_SHIM_SYMBOL, Object.Symbols[1].Name);
        True(Object.Symbols[1].Binding == Objectˉsymbolˉbinding.Export, "The WVA Main shim is not exported.");
        Equal(Kernelˉexceptionˉcontract.GENERAL_PROTECTION_ENTRY_SYMBOL, Object.Symbols[2].Name);
        True(Object.Symbols[2].Binding == Objectˉsymbolˉbinding.Export, "The vector-13 WVA entry is not exported.");
        Equal(10u, Object.Symbols[2].Size);
        Equal(Kernelˉexceptionˉcontract.INVALID_OPCODE_ENTRY_SYMBOL, Object.Symbols[3].Name);
        True(Object.Symbols[3].Binding == Objectˉsymbolˉbinding.Export, "The vector-6 WVA entry is not exported.");
        Equal(15u, Object.Symbols[3].Size);
        Equal(Kernelˉpagingˉcontract.PROTECTION_ENABLE_SYMBOL, Object.Symbols[4].Name);
        True(Object.Symbols[4].Binding == Objectˉsymbolˉbinding.Export, "The page-protection shim is not exported.");
        Equal(25u, Object.Symbols[4].Size);
        Equal(Kernelˉpagingˉcontract.PAGE_TABLE_ACTIVATE_SYMBOL, Object.Symbols[5].Name);
        True(Object.Symbols[5].Binding == Objectˉsymbolˉbinding.Export, "The page-table activation shim is not exported.");
        Equal(7u, Object.Symbols[5].Size);
        Equal(Kernelˉprocessˉcontract.EXCEPTION_13_ENTRY_SYMBOL, Object.Symbols[6].Name);
        True(Object.Symbols[6].Binding == Objectˉsymbolˉbinding.Export,
            "The process vector-13 WVA entry is not exported.");
        Equal(10u, Object.Symbols[6].Size);
        Equal(Kernelˉprocessˉcontract.EXCEPTION_14_ENTRY_SYMBOL, Object.Symbols[7].Name);
        True(Object.Symbols[7].Binding == Objectˉsymbolˉbinding.Export,
            "The process vector-14 WVA entry is not exported.");
        Equal(10u, Object.Symbols[7].Size);
        Equal(Kernelˉprocessˉcontract.EXCEPTION_6_ENTRY_SYMBOL, Object.Symbols[8].Name);
        True(Object.Symbols[8].Binding == Objectˉsymbolˉbinding.Export,
            "The process vector-6 WVA entry is not exported.");
        Equal(15u, Object.Symbols[8].Size);
        Equal(Kernelˉassemblyˉcontract.Q35_SHUTDOWN_SYMBOL, Object.Symbols[9].Name);
        True(Object.Symbols[9].Binding == Objectˉsymbolˉbinding.Export, "The Q35 shutdown shim is not exported.");
        Equal(19u, Object.Symbols[9].Size);
        Equal(Kernelˉexceptionˉcontract.TERMINAL_SYMBOL, Object.Symbols[10].Name);
        True(Object.Symbols[10].Binding == Objectˉsymbolˉbinding.Import,
            "The normalized terminal handler is not imported by WVA.");
        Equal(Kernelˉprocessˉcontract.EXCEPTION_ENTRY_SYMBOL, Object.Symbols[11].Name);
        True(Object.Symbols[11].Binding == Objectˉsymbolˉbinding.Import,
            "The process exception handler is not imported by WVA.");
        Equal(Kernelˉassemblyˉcontract.X64_WRITE_BYTE_SYMBOL, Object.Symbols[12].Name);
        True(Object.Symbols[12].Binding == Objectˉsymbolˉbinding.Import, "The x64 byte writer is not imported by WVA.");
        Equal(Kernelˉwvbˉadmissionˉcontract.BRIDGE_SYMBOL, Object.Symbols[13].Name);
        True(Object.Symbols[13].Binding == Objectˉsymbolˉbinding.Import, "The WVB admission bridge is not imported by WVA.");
        Equal(8, Object.Relocations.Length);
        Equal(new Objectˉrelocation(Objectˉrelocationˉkind.Relativeˉi32, 0, 1, 12, -4), Object.Relocations[0]);
        Equal(new Objectˉrelocation(Objectˉrelocationˉkind.Relativeˉi32, 0, 6, 13, -4), Object.Relocations[1]);
        Equal(new Objectˉrelocation(Objectˉrelocationˉkind.Relativeˉi32, 0, 16, 10, -4), Object.Relocations[2]);
        Equal(new Objectˉrelocation(Objectˉrelocationˉkind.Relativeˉi32, 0, 31, 10, -4), Object.Relocations[3]);
        Equal(new Objectˉrelocation(Objectˉrelocationˉkind.Relativeˉi32, 0, 73, 11, -4), Object.Relocations[4]);
        Equal(new Objectˉrelocation(Objectˉrelocationˉkind.Relativeˉi32, 0, 83, 11, -4), Object.Relocations[5]);
        Equal(new Objectˉrelocation(Objectˉrelocationˉkind.Relativeˉi32, 0, 98, 11, -4), Object.Relocations[6]);
        Equal(new Objectˉrelocation(Objectˉrelocationˉkind.Relativeˉi32, 0, 117, 9, -4), Object.Relocations[7]);
    }

    private static void Kernelˉnativeˉprobeˉisˉportableˉandˉbounded()
    {
        var First = Kernelˉnativeˉprobe.Build();
        var Second = Kernelˉnativeˉprobe.Build();
        Sequenceˉequal(First.Moduleˉbytes, Second.Moduleˉbytes);
        Sequenceˉequal(First.Nativeˉobjectˉbytes, Second.Nativeˉobjectˉbytes);
        Sequenceˉequal(First.Bridgeˉobjectˉbytes, Second.Bridgeˉobjectˉbytes);

        var Verifiedˉmodule = Moduleˉcodec.Readˉandˉverify(First.Moduleˉbytes.AsSpan());
        var Interpreted = new Referenceˉruntime(
            Verifiedˉmodule,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(Kernelˉnativeˉprobeˉcontract.EXPECTED_RESULT, Interpreted.Exitˉcode);
        Equal(
            (long)Kernelˉnativeˉprobeˉcontract.EXACT_INSTRUCTION_BUDGET,
            Interpreted.Executedˉinstructions);

        var Nativeˉobject = Objectˉcodec.Readˉandˉverify(First.Nativeˉobjectˉbytes.AsSpan()).Value;
        Equal(2, Nativeˉobject.Sections.Length);
        True(
            Nativeˉobject.Sections[1].Kind == Objectˉsectionˉkind.Readˉonlyˉdata,
            "The native probe does not retain read-only data.");
        Equal(4, Nativeˉobject.Relocations.Length);
        Equal(1, Nativeˉobject.Symbols.Count(Symbol => Symbol.Name == "Main"));

        var Bridgeˉobject = Objectˉcodec.Readˉandˉverify(First.Bridgeˉobjectˉbytes.AsSpan()).Value;
        Equal(143u, Bridgeˉobject.Sections[0].Memoryˉsize);
        Equal(2, Bridgeˉobject.Relocations.Length);
        Equal(929, First.Moduleˉbytes.Length);
        Equal(
            "0653613d868abbba99b5e31230fb2a1f92581c4989318577cb77a6d6e60f8339",
            Objectˉdigest.Calculateˉsha256(First.Moduleˉbytes.AsSpan()));
        Equal(8_010, First.Nativeˉobjectˉbytes.Length);
        Equal(
            "f3d0d2aec5b7fb81d02e4188fb6ba48b6a21dc91c89bdf7f00daaf7b0a981038",
            Objectˉdigest.Calculateˉsha256(First.Nativeˉobjectˉbytes.AsSpan()));
        Equal(355, First.Bridgeˉobjectˉbytes.Length);
        Equal(
            "bfa2b522b2bf22b3681b523c66c2986b1af366ba042adeddd8a106b5a96a5225",
            Objectˉdigest.Calculateˉsha256(First.Bridgeˉobjectˉbytes.AsSpan()));
    }

    private static void Kernelˉwvbˉadmissionˉisˉbounded()
    {
        var First = Kernelˉwvbˉadmission.Build();
        var Second = Kernelˉwvbˉadmission.Build();
        Sequenceˉequal(First.Embeddedˉmoduleˉbytes, Second.Embeddedˉmoduleˉbytes);
        Sequenceˉequal(First.Admissionˉmoduleˉbytes, Second.Admissionˉmoduleˉbytes);
        Sequenceˉequal(First.Embeddedˉnativeˉobjectˉbytes, Second.Embeddedˉnativeˉobjectˉbytes);
        Sequenceˉequal(First.Admissionˉnativeˉobjectˉbytes, Second.Admissionˉnativeˉobjectˉbytes);
        Sequenceˉequal(First.Bridgeˉobjectˉbytes, Second.Bridgeˉobjectˉbytes);

        Equal(174, First.Embeddedˉmoduleˉbytes.Length);
        Equal(
            "7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2",
            Objectˉdigest.Calculateˉsha256(First.Embeddedˉmoduleˉbytes.AsSpan()));
        Equal(2_786, First.Admissionˉmoduleˉbytes.Length);
        Equal(
            "231a4001dc316ae965a851aa27eabacaba7ef57d4f72d18ee0e7eaa4d90d2e54",
            Objectˉdigest.Calculateˉsha256(First.Admissionˉmoduleˉbytes.AsSpan()));
        Equal(504, First.Embeddedˉnativeˉobjectˉbytes.Length);
        Equal(
            "461361ba8853faa59d7b8f841308fd88b5e7ee837a2654ab3e534771c189a834",
            Objectˉdigest.Calculateˉsha256(First.Embeddedˉnativeˉobjectˉbytes.AsSpan()));
        Equal(24_445, First.Admissionˉnativeˉobjectˉbytes.Length);
        Equal(
            "5b11e97e5bb9746daa911559ea9a7a204419fe2cded44977163430185e7d150d",
            Objectˉdigest.Calculateˉsha256(First.Admissionˉnativeˉobjectˉbytes.AsSpan()));
        Equal(484, First.Bridgeˉobjectˉbytes.Length);
        Equal(
            "7b53fc11e4e99966386994c247c3a2a19f99ef8da751dbd9dc53f5575871a00d",
            Objectˉdigest.Calculateˉsha256(First.Bridgeˉobjectˉbytes.AsSpan()));

        var Admission = Runˉportableˉmain(First.Admissionˉmoduleˉbytes);
        Equal(Kernelˉwvbˉadmissionˉcontract.ADMISSION_TOKEN, Admission.Exitˉcode);
        Equal(
            (long)Kernelˉwvbˉadmissionˉcontract.EXACT_INSTRUCTION_BUDGET,
            Admission.Executedˉinstructions);
        var Embedded = Runˉportableˉmain(First.Embeddedˉmoduleˉbytes);
        Equal(Kernelˉwvbˉadmissionˉcontract.EXPECTED_RESULT, Embedded.Exitˉcode);
        Equal(4L, Embedded.Executedˉinstructions);

        var Changedˉmagic = Mutate(First.Embeddedˉmoduleˉbytes, 0).ToImmutableArray();
        var Changedˉsection = Mutate(First.Embeddedˉmoduleˉbytes, 16).ToImmutableArray();
        var Changedˉcode = Mutate(First.Embeddedˉmoduleˉbytes, 122).ToImmutableArray();
        var Truncated = First.Embeddedˉmoduleˉbytes[..^1];
        Assertˉadmissionˉresult(Changedˉmagic, 0);
        Assertˉadmissionˉresult(Changedˉsection, 0);
        Assertˉadmissionˉresult(Changedˉcode, 0);
        Assertˉadmissionˉresult(Truncated, 0);
        Rejectˉwvb(Changedˉmagic);
        Rejectˉwvb(Changedˉsection);
        Rejectˉwvb(Truncated);
        Equal(28, Runˉportableˉmain(Changedˉcode).Exitˉcode);

        var Admissionˉobject = Objectˉcodec.Readˉandˉverify(
            First.Admissionˉnativeˉobjectˉbytes.AsSpan()).Value;
        var Embeddedˉobject = Objectˉcodec.Readˉandˉverify(
            First.Embeddedˉnativeˉobjectˉbytes.AsSpan()).Value;
        Equal(1, Admissionˉobject.Symbols.Count(Symbol =>
            Symbol.Binding == Objectˉsymbolˉbinding.Export &&
            Symbol.Name == Kernelˉwvbˉadmissionˉcontract.ADMISSION_SYMBOL));
        Equal(1, Embeddedˉobject.Symbols.Count(Symbol =>
            Symbol.Binding == Objectˉsymbolˉbinding.Export &&
            Symbol.Name == Kernelˉwvbˉadmissionˉcontract.EMBEDDED_MAIN_SYMBOL));
        True(
            Admissionˉobject.Symbols.Concat(Embeddedˉobject.Symbols).All(Symbol =>
                Symbol.Binding != Objectˉsymbolˉbinding.Export || Symbol.Name != "Main"),
            "A renamed admission object leaked the source-level Main symbol.");

        var Bridge = Objectˉcodec.Readˉandˉverify(First.Bridgeˉobjectˉbytes.AsSpan()).Value;
        Equal(162u, Bridge.Sections[0].Memoryˉsize);
        Equal(3, Bridge.Relocations.Length);
    }

    private static Runtimeˉresult Runˉportableˉmain(ImmutableArray<byte> moduleˉbytes)
    {
        return new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(moduleˉbytes.AsSpan()),
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
    }

    private static Runtimeˉresult Runˉinterpreter(
        ImmutableArray<byte> interpreterˉmoduleˉbytes,
        ImmutableArray<byte> runtimeˉinput)
    {
        var Resources = new Hostedˉresourceˉcontext(
            [],
            TextWriter.Null,
            TextWriter.Null,
            new Bootˉresourceˉreader(runtimeˉinput));
        return new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(interpreterˉmoduleˉbytes.AsSpan()),
            new Referenceˉcapabilityˉhost(Resources),
            new(ImmutableHashSet.Create(StringComparer.Ordinal, Capabilityˉcatalog.FILE_READ_BYTES)))
            .Runˉmain();
    }

    private static int Findˉwvbˉsectionˉpayload(ImmutableArray<byte> moduleˉbytes, byte sectionˉkind)
    {
        var Cursor = 12;
        while (Cursor + 8 <= moduleˉbytes.Length)
        {
            var Payloadˉlength = BinaryPrimitives.ReadUInt32LittleEndian(moduleˉbytes.AsSpan()[(Cursor + 4)..]);
            if (moduleˉbytes[Cursor] == sectionˉkind)
            {
                return Cursor + 8;
            }

            Cursor = checked(Cursor + 8 + checked((int)Payloadˉlength));
        }

        throw new InvalidOperationException($"WVB section {sectionˉkind} was not found.");
    }

    private static void Assertˉadmissionˉresult(
        ImmutableArray<byte> embeddedˉmoduleˉbytes,
        int expectedˉresult)
    {
        var Admissionˉmoduleˉbytes = Kernelˉwvbˉadmission.Compileˉadmissionˉmodule(
            embeddedˉmoduleˉbytes);
        Equal(expectedˉresult, Runˉportableˉmain(Admissionˉmoduleˉbytes).Exitˉcode);
    }

    private static void Rejectˉwvb(ImmutableArray<byte> moduleˉbytes)
    {
        try
        {
            _ = Moduleˉcodec.Readˉandˉverify(moduleˉbytes.AsSpan());
        }
        catch (Bytecodeˉexception)
        {
            return;
        }
        throw new InvalidOperationException("Malformed WVB was accepted by the reference decoder.");
    }

    private static void Processˉplanˉfails(
        Kernelˉpagingˉplan paging,
        ulong allocationˉaddress,
        ImmutableArray<byte> userˉimage,
        ImmutableArray<byte> moduleˉdigest,
        ImmutableArray<byte> programˉdigest,
        Kernelˉprocessˉdefinition definition,
        string code)
    {
        Processˉplanˉfails(
            paging, allocationˉaddress, userˉimage, moduleˉdigest, programˉdigest,
            [], 0, definition, code);
    }

    private static void Processˉplanˉfails(
        Kernelˉpagingˉplan paging,
        ulong allocationˉaddress,
        ImmutableArray<byte> userˉimage,
        ImmutableArray<byte> moduleˉdigest,
        ImmutableArray<byte> programˉdigest,
        ImmutableArray<byte> runtimeˉinput,
        uint bootˉresourceˉserviceˉoffset,
        Kernelˉprocessˉdefinition definition,
        string code)
    {
        var Result = Kernelˉprocessˉplanner.Plan(
            paging, allocationˉaddress, userˉimage.AsSpan(), moduleˉdigest.AsSpan(),
            programˉdigest.AsSpan(), runtimeˉinput.AsSpan(), bootˉresourceˉserviceˉoffset,
            definition);
        True(!Result.Success, $"Process planning unexpectedly succeeded instead of producing {code}.");
        Equal(code, Result.Diagnostics[0].Code);
    }


    private static void Memoryˉplanˉfails(
        byte[] memoryˉmap,
        ulong descriptorˉbytes,
        string expectedˉcode)
    {
        var Result = Kernelˉmemoryˉplanner.Plan(memoryˉmap, descriptorˉbytes);
        True(!Result.Success, "A malformed memory map produced a kernel arena.");
        Equal(expectedˉcode, Result.Diagnostics[0].Code);
    }

    private static void Pagingˉplanˉfails(
        ulong rootˉaddress,
        ulong executableˉaddress,
        string expectedˉcode)
    {
        var Result = Kernelˉpagingˉplanner.Plan(rootˉaddress, executableˉaddress);
        True(!Result.Success, "An invalid paging request produced a page-table plan.");
        Equal(expectedˉcode, Result.Diagnostics[0].Code);
    }

    private static Linkˉresult Linkˉcode(ImmutableArray<byte> code, uint baseˉaddress = 0)
    {
        var Objectˉbytes = Objectˉcodec.Write(new(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 16, (uint)code.Length, code)],
            [new("Main", Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, 0, (uint)code.Length)],
            [])).ToImmutableArray();
        var Result = Linkˉcompiler.Link([new(Objectˉbytes)], new(baseˉaddress, "Main"));
        True(Result.Success, "The test object did not link.");
        return Result;
    }

    private static byte[] Mutate(ImmutableArray<byte> source, int offset)
    {
        var Result = source.ToArray();
        Result[offset] ^= 0x01;
        return Result;
    }

    private static void Processˉmachineˉrejects(Kernelˉprocessˉimageˉartifacts image)
    {
        try
        {
            _ = Kernelˉprocessˉx64.Build(image, false);
        }
        catch (InvalidOperationException)
        {
            return;
        }
        throw new InvalidOperationException("A changed boot-resource service was accepted.");
    }

    private static int Countˉsequence(ImmutableArray<byte> source, ImmutableArray<byte> pattern)
    {
        var Count = 0;
        for (var Offset = 0; Offset <= source.Length - pattern.Length; Offset++)
        {
            if (source.AsSpan(Offset, pattern.Length).SequenceEqual(pattern.AsSpan()))
            {
                Count++;
            }
        }
        return Count;
    }

    private static void Reject(ReadOnlySpan<byte> bytes, string expectedˉcode)
    {
        try
        {
            _ = Uefiˉapplicationˉverifier.Verify(bytes);
        }
        catch (Uefiˉapplicationˉexception Exception)
        {
            Equal(expectedˉcode, Exception.Code);
            return;
        }
        throw new InvalidOperationException("The malformed UEFI application was accepted.");
    }

    private static void True(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void Equal<T>(
        T expected,
        T actual,
        [CallerArgumentExpression(nameof(actual))] string? actualˉexpression = null)
        where T : IEquatable<T>
    {
        if (!expected.Equals(actual))
        {
            throw new InvalidOperationException(
                $"Expected '{expected}', received '{actual}' from {actualˉexpression}.");
        }
    }

    private static void Sequenceˉequal(ImmutableArray<byte> expected, ImmutableArray<byte> actual)
    {
        if (!expected.AsSpan().SequenceEqual(actual.AsSpan()))
        {
            throw new InvalidOperationException("Byte sequences differ.");
        }
    }

    private sealed class Bootˉresourceˉreader(ImmutableArray<byte> bytes) : IHostedˉfileˉreader
    {
        public ImmutableArray<byte> Readˉbytes(string resourceˉname, int maximumˉbytes)
        {
            if (!StringComparer.Ordinal.Equals(resourceˉname, "boot:main.wvb") ||
                bytes.IsDefault || bytes.Length > maximumˉbytes)
            {
                throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Notˉfound,
                    "The fixed Windvale OS boot resource was not found.");
            }
            return bytes;
        }
    }

    private sealed record Testˉcase(string Name, Action Body);

    private sealed record Memoryˉdescriptorˉinput(
        uint Type,
        ulong Physicalˉaddress,
        ulong Pages);
}
