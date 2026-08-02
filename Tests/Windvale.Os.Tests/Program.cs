using System.Buffers.Binary;
using System.Collections.Immutable;
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
        new("Windvale process policy binds one admitted user image", Kernelˉprocessˉpolicyˉbindsˉimage),
        new("protected process planner isolates bounded W^X user pages", Kernelˉprocessˉplannerˉisolatesˉuserˉpages),
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
            new Memoryˉdescriptorˉinput(7, 0x0030_0000, 32),
            new Memoryˉdescriptorˉinput(2, 0x0010_0000, 8),
            new Memoryˉdescriptorˉinput(7, 0x0020_0000, 16));
        var Result = Kernelˉmemoryˉplanner.Plan(Map, 40);
        True(Result.Success, Result.Diagnostics.IsEmpty ? "Memory planning failed." : Result.Diagnostics[0].Message);
        var Plan = Result.Plan!;
        Equal(0x0030_0000UL, Plan.Arenaˉaddress);
        Equal(32UL, Plan.Arenaˉpages);
        Equal(0x0030_0000UL, Plan.Stateˉaddress);
        Equal(0x0030_0040UL, Plan.Handoffˉcopyˉaddress);
        Equal(0x0030_1000UL, Plan.Stackˉaddress);
        Equal(8_192UL, Plan.Stackˉbytes);
        Equal(0x0030_3000UL, Plan.Stackˉtop);
        Equal(3UL, Plan.Firstˉfreeˉpage);
        Equal(29UL, Plan.Freeˉpages);
    }

    private static void Memoryˉplannerˉrejectsˉmalformedˉmaps()
    {
        Memoryˉplanˉfails([], 40, "WVOS4001");
        Memoryˉplanˉfails(new byte[40], 39, "WVOS4001");
        Memoryˉplanˉfails(new byte[41], 40, "WVOS4001");
        Memoryˉplanˉfails(Buildˉmemoryˉmap(new Memoryˉdescriptorˉinput(7, 0x0010_0001, 16)), 40, "WVOS4002");
        Memoryˉplanˉfails(Buildˉmemoryˉmap(new Memoryˉdescriptorˉinput(7, 0x0010_0000, 0)), 40, "WVOS4003");
        Memoryˉplanˉfails(Buildˉmemoryˉmap(new Memoryˉdescriptorˉinput(7, 0xFFFF_FFFF_FFFF_F000, 2)), 40, "WVOS4004");
        Memoryˉplanˉfails(Buildˉmemoryˉmap(new Memoryˉdescriptorˉinput(2, 0x0010_0000, 32)), 40, "WVOS4005");
        Memoryˉplanˉfails(
            Buildˉmemoryˉmap(
                new Memoryˉdescriptorˉinput(7, 0x0020_0000, 32),
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
            Buildˉmemoryˉmap(new Memoryˉdescriptorˉinput(7, 0x0020_0000, 32)),
            40).Plan!;
        var Arena = Enumerable.Repeat((byte)0xA5, checked((int)Kernelˉmemoryˉcontract.ARENA_BYTES)).ToArray();
        var Allocator = new Kernelˉpageˉallocator(Plan, Arena);
        True(Arena.All(Value => Value == 0), "Kernel arena initialization did not clear every byte.");

        Array.Fill(Arena, (byte)0x5A, 3 * 4_096, 4_096);
        Equal(0x0020_3000UL, Allocator.Allocateˉpages(1)!.Value);
        True(Arena.AsSpan(3 * 4_096, 4_096).IndexOfAnyExcept((byte)0) < 0, "Allocated page was not zeroed.");
        Equal(28UL, Allocator.Remainingˉpages);
        True(Allocator.Allocateˉpages(0) is null, "A zero-page allocation succeeded.");
        Equal(0x0020_4000UL, Allocator.Allocateˉpages(28)!.Value);
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
            Kernelˉpagingˉplanner.Readˉentry(Plan, 5, 28));

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
            "43bc3a191ebaec3944bb1fa47927e9623341dbb11085ea3c76fbe70b6ca16cb0",
            Objectˉdigest.Calculateˉsha256(First.Objectˉbytes.AsSpan()));
        Equal(851, First.Codeˉbytes.Length);
        Equal(
            "c77b367b120299f39ca65e4b6955d48ab57408440ad762e3deab17988e01606d",
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
        Equal(1, Countˉsequence(First.Codeˉbytes, [0xB9, 0x20, 0x00, 0x00, 0x00]));
        Equal(1, Countˉsequence(First.Codeˉbytes, [0x57, 0x56, 0x4B, 0x50, 0x41, 0x47, 0x30, 0x32]));
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
        Equal(2_780, First.Policyˉmoduleˉbytes.Length);
        Equal("fc47ce2d256bea69edbc086bf288136dd7f557d8250e397a2a9e82a66c23078d",
            Objectˉdigest.Calculateˉsha256(First.Policyˉmoduleˉbytes.AsSpan()));
        Equal(25_062, First.Policyˉnativeˉobjectˉbytes.Length);
        Equal("7b3096698ec6730ffa8c17488e00155a1700ecd879ae5fe8130a096b40311aff",
            Objectˉdigest.Calculateˉsha256(First.Policyˉnativeˉobjectˉbytes.AsSpan()));
        Equal(202, First.Userˉshimˉobjectˉbytes.Length);
        Equal("1a3065bcfa9ddcd973ede2b36ac918544a1c4c63aa44729ce1f1d970413fba76",
            Objectˉdigest.Calculateˉsha256(First.Userˉshimˉobjectˉbytes.AsSpan()));
        Equal(195, Faultˉfirst.Userˉshimˉobjectˉbytes.Length);
        Equal("b67bbdaa78f492564d21d18bd5fc2abd75978c89bafe882418237e53503bc14f",
            Objectˉdigest.Calculateˉsha256(Faultˉfirst.Userˉshimˉobjectˉbytes.AsSpan()));
        Equal(454, First.Userˉimageˉbytes.Length);
        Equal("6558145ea3bfecc4f9f312ba886ffbdc7a902ed14e66684c5e992d4bc5653947",
            Objectˉdigest.Calculateˉsha256(First.Userˉimageˉbytes.AsSpan()));
        Equal(438, Faultˉfirst.Userˉimageˉbytes.Length);
        Equal("973ead836f588bc6ef2b0fa31754f75e12a88ab229048d85075884f654ed5356",
            Objectˉdigest.Calculateˉsha256(Faultˉfirst.Userˉimageˉbytes.AsSpan()));

        Sequenceˉequal(First.Policyˉmoduleˉbytes, Second.Policyˉmoduleˉbytes);
        Sequenceˉequal(First.Policyˉnativeˉobjectˉbytes, Second.Policyˉnativeˉobjectˉbytes);
        Sequenceˉequal(First.Userˉshimˉobjectˉbytes, Second.Userˉshimˉobjectˉbytes);
        Sequenceˉequal(First.Userˉimageˉbytes, Second.Userˉimageˉbytes);
        Sequenceˉequal(Faultˉfirst.Userˉimageˉbytes, Faultˉsecond.Userˉimageˉbytes);
        Sequenceˉequal(First.Policyˉmoduleˉbytes, Faultˉfirst.Policyˉmoduleˉbytes);
        Sequenceˉequal(First.Policyˉnativeˉobjectˉbytes, Faultˉfirst.Policyˉnativeˉobjectˉbytes);
        True(!First.Userˉimageˉbytes.AsSpan().SequenceEqual(Faultˉfirst.Userˉimageˉbytes.AsSpan()),
            "The normal and deliberate-fault user images are identical.");

        var Policy = Moduleˉcodec.Readˉandˉverify(First.Policyˉmoduleˉbytes.AsSpan());
        Equal("Processˉfoundation", Policy.Module.Name);
        True(Policy.Module.Profile == Moduleˉprofile.Portable, "The process policy is not portable Windvale.");
        Equal(0, Policy.Module.Capabilities.Length);
        Equal(Kernelˉprocessˉcontract.POLICY_TOKEN,
            Runˉportableˉmain(First.Policyˉmoduleˉbytes).Exitˉcode);
        Sequenceˉequal(Admission.Embeddedˉmoduleˉbytes,
            Kernelˉwvbˉadmission.Build().Embeddedˉmoduleˉbytes);
        Equal(
            "7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2",
            Convert.ToHexString(First.Moduleˉdigest.AsSpan()).ToLowerInvariant());

        var Policyˉobject = Objectˉcodec.Readˉandˉverify(First.Policyˉnativeˉobjectˉbytes.AsSpan()).Value;
        Equal(1, Policyˉobject.Symbols.Count(Symbol =>
            Symbol.Binding == Objectˉsymbolˉbinding.Export &&
            Symbol.Name == Kernelˉprocessˉcontract.POLICY_SYMBOL));
        var Userˉobject = Objectˉcodec.Readˉandˉverify(First.Userˉshimˉobjectˉbytes.AsSpan()).Value;
        var Faultˉobject = Objectˉcodec.Readˉandˉverify(Faultˉfirst.Userˉshimˉobjectˉbytes.AsSpan()).Value;
        Equal(3, Countˉsequence(Userˉobject.Sections[0].Data, [0x0F, 0x05]));
        Equal(2, Countˉsequence(Faultˉobject.Sections[0].Data, [0x0F, 0x05]));
        Equal(1, Countˉsequence(Faultˉobject.Sections[0].Data, [0xFA]));
        Equal(0, Countˉsequence(Faultˉobject.Sections[0].Data, [0xCC]));
        True((ulong)First.Userˉimageˉbytes.Length <= Kernelˉpagingˉcontract.PAGE_BYTES,
            "The first user image exceeds one page.");
        True((ulong)Faultˉfirst.Userˉimageˉbytes.Length <= Kernelˉpagingˉcontract.PAGE_BYTES,
            "The deliberate-fault user image exceeds one page.");
    }

    private static void Kernelˉprocessˉplannerˉisolatesˉuserˉpages()
    {
        const ulong KERNEL_ROOT = 0x0020_0000;
        const ulong EXECUTABLE = 0x0040_0000;
        const ulong ALLOCATION = 0x0080_0000;
        var Paging = Kernelˉpagingˉplanner.Plan(KERNEL_ROOT, EXECUTABLE).Plan!;
        var Image = Kernelˉprocessˉimage.Build(Kernelˉwvbˉadmission.Build(), false);
        var First = Kernelˉprocessˉplanner.Plan(
            Paging, ALLOCATION, Image.Userˉimageˉbytes.AsSpan(), Image.Moduleˉdigest.AsSpan());
        var Second = Kernelˉprocessˉplanner.Plan(
            Paging, ALLOCATION, Image.Userˉimageˉbytes.AsSpan(), Image.Moduleˉdigest.AsSpan());
        True(First.Success, First.Diagnostics.IsEmpty ? "Process planning failed." : First.Diagnostics[0].Message);
        True(Second.Success, "Repeated process planning failed.");
        var Plan = First.Plan!;
        Sequenceˉequal(Plan.Tableˉbytes, Second.Plan!.Tableˉbytes);
        Sequenceˉequal(Plan.Processˉrecord, Second.Plan.Processˉrecord);
        Equal(ALLOCATION, Plan.Rootˉaddress);
        Equal(ALLOCATION + 0x4000UL, Plan.Userˉcodeˉaddress);
        Equal(ALLOCATION + 0x5000UL, Plan.Userˉstackˉaddress);
        Equal(ALLOCATION + 0x6000UL, Plan.Userˉdataˉaddress);

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
        Equal(3, Userˉleaves);
        Sequenceˉequal(Image.Userˉimageˉbytes, Plan.Userˉcodeˉbytes[..Image.Userˉimageˉbytes.Length]);
        True(Plan.Userˉstackˉbytes.All(Value => Value == 0), "The initial user stack is not zeroed.");
        Equal((uint)Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION,
            BinaryPrimitives.ReadUInt32LittleEndian(Plan.Userˉdataˉbytes.AsSpan()));
        Equal((uint)Nativeˉexecutionˉcontextˉcontract.SIZE,
            BinaryPrimitives.ReadUInt32LittleEndian(Plan.Userˉdataˉbytes.AsSpan()[4..]));
        Equal(Kernelˉprocessˉcontract.RECORD_MAGIC,
            BinaryPrimitives.ReadUInt64LittleEndian(Plan.Processˉrecord.AsSpan()));
        Equal(Kernelˉprocessˉcontract.PROCESS_STATE_READY,
            BinaryPrimitives.ReadUInt32LittleEndian(Plan.Processˉrecord.AsSpan()[16..]));
        Sequenceˉequal(Image.Moduleˉdigest, Plan.Processˉrecord[32..64]);

        Processˉplanˉfails(Paging, 0, Image.Userˉimageˉbytes, Image.Moduleˉdigest, "WVOS6001");
        Processˉplanˉfails(Paging, 0x009F_C000, Image.Userˉimageˉbytes, Image.Moduleˉdigest, "WVOS6002");
        Processˉplanˉfails(Paging, ALLOCATION, [], Image.Moduleˉdigest, "WVOS6003");
        Processˉplanˉfails(Paging, ALLOCATION, Image.Userˉimageˉbytes, Image.Moduleˉdigest[..^1], "WVOS6004");
        Processˉplanˉfails(Paging, EXECUTABLE, Image.Userˉimageˉbytes, Image.Moduleˉdigest, "WVOS6005");
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
        Equal(3_846, First.Objectˉbytes.Length);
        Equal("641500ad40d2ab7cf36d12ac9cc51163c690341ec031c847497874cf9f0c3576",
            Objectˉdigest.Calculateˉsha256(First.Objectˉbytes.AsSpan()));
        Equal(2_514, First.Codeˉbytes.Length);
        Equal("ef7af1c127dd0973e78f4f57ad7f77c6e16f4fe8f2427a359f177311b028bbb6",
            Objectˉdigest.Calculateˉsha256(First.Codeˉbytes.AsSpan()));
        Equal(3_862, Faultˉfirst.Objectˉbytes.Length);
        Equal("2b74ecabe417e4a3917f90e0b49a472faab2fb36d7fc9813808cb260acc0a327",
            Objectˉdigest.Calculateˉsha256(Faultˉfirst.Objectˉbytes.AsSpan()));
        Equal(2_546, Faultˉfirst.Codeˉbytes.Length);
        Equal("7381d1f14bcbab99e8194fcd1248b43494543a16c53d06f2ea59ee9d4cc5f0e8",
            Objectˉdigest.Calculateˉsha256(Faultˉfirst.Codeˉbytes.AsSpan()));
        Equal(8, First.Relocations.Length);
        Equal(8, Faultˉfirst.Relocations.Length);
        Sequenceˉequal(First.Objectˉbytes, Second.Objectˉbytes);
        Sequenceˉequal(First.Codeˉbytes, Second.Codeˉbytes);
        Sequenceˉequal(Faultˉfirst.Objectˉbytes, Faultˉsecond.Objectˉbytes);
        True(!First.Objectˉbytes.AsSpan().SequenceEqual(Faultˉfirst.Objectˉbytes.AsSpan()),
            "The normal and deliberate-fault process objects are identical.");

        var Object = Objectˉcodec.Readˉandˉverify(First.Objectˉbytes.AsSpan()).Value;
        Equal(2, Object.Sections.Length);
        True(Object.Sections[0].Kind == Objectˉsectionˉkind.Code, "The process machine section is not code.");
        True(Object.Sections[1].Kind == Objectˉsectionˉkind.Readˉonlyˉdata,
            "The user image is not retained as read-only link data.");
        Sequenceˉequal(First.Codeˉbytes, Object.Sections[0].Data);
        Sequenceˉequal(Normalˉimage.Userˉimageˉbytes, Object.Sections[1].Data);
        Equal(11, Object.Symbols.Length);
        Equal(Kernelˉprocessˉcontract.ENTER_SYMBOL, Object.Symbols[1].Name);
        Equal(Kernelˉprocessˉcontract.EXCEPTION_ENTRY_SYMBOL, Object.Symbols[2].Name);
        Equal(Kernelˉprocessˉcontract.SYSCALL_ENTRY_SYMBOL, Object.Symbols[3].Name);
        Equal(Kernelˉmemoryˉcontract.ALLOCATE_PAGES_SYMBOL, Object.Symbols[4].Name);
        Equal(Kernelˉprocessˉcontract.POLICY_SYMBOL, Object.Symbols[5].Name);
        Equal(Kernelˉexceptionˉcontract.TERMINAL_SYMBOL, Object.Symbols[6].Name);
        Equal(Kernelˉpagingˉcontract.PAGE_TABLE_ACTIVATE_SYMBOL, Object.Symbols[7].Name);
        Equal(Kernelˉprocessˉcontract.EXCEPTION_13_ENTRY_SYMBOL, Object.Symbols[8].Name);
        Equal(Kernelˉprocessˉcontract.EXCEPTION_14_ENTRY_SYMBOL, Object.Symbols[9].Name);
        Equal(Kernelˉprocessˉcontract.EXCEPTION_6_ENTRY_SYMBOL, Object.Symbols[10].Name);
        True(Object.Symbols.Skip(4).All(Symbol => Symbol.Binding == Objectˉsymbolˉbinding.Import),
            "A protected-process machine dependency is not an explicit import.");
        True(Object.Relocations.All(Relocation =>
                Relocation.Kind == Objectˉrelocationˉkind.Relativeˉi32 &&
                Relocation.Sectionˉindex == 0 &&
                Relocation.Addend == -4),
            "The protected-process machine object contains a noncanonical relocation.");
        Equal(2, Countˉsequence(First.Codeˉbytes, [0x48, 0x0F, 0x07]));
        Equal(3, Countˉsequence(First.Codeˉbytes, [0x0F, 0x01, 0xF8]));
        Equal(3, Countˉsequence(Normalˉimage.Userˉimageˉbytes, [0x0F, 0x05]));
        Equal(2, Countˉsequence(Faultˉimage.Userˉimageˉbytes, [0x0F, 0x05]));
    }

    private static void Userˉfaultˉfirmwareˉprobeˉbuildsˉreproducibly()
    {
        var First = Firmwareˉprobe.Buildˉapplication(Firmwareˉprobeˉscenario.Userˉfault);
        var Second = Firmwareˉprobe.Buildˉapplication(Firmwareˉprobeˉscenario.Userˉfault);
        Sequenceˉequal(First, Second);
        Equal(75_264, First.Length);
        Equal(
            "ed2c7cf51ef9a9e5c93e163f94b56283afb717f7f8541d3f7ee9ab8c75cb5a0a",
            Objectˉdigest.Calculateˉsha256(First.AsSpan()));
        True(!First.AsSpan().SequenceEqual(Firmwareˉprobe.Buildˉapplication().AsSpan()),
            "The normal and deliberate user-fault images are identical.");
        Equal(
            "windvale-os-boot 22\nentry=pass\nsystem-table=pass\nmemory-map=pass\nboot-services=exited\nmemory-owned=pass\nallocator=pass\nkernel-stack=pass\npaging=owned\nwvb-admission=pass\nprocess=isolated\nipc=pass\nHello from Windvale\ncpu-exceptions=armed\nnative-context=pass\nnative-wvb=pass\nwindvale-source=pass\nuser-fault=contained\nstatus=pass\nshutdown=poweroff\n",
            Firmwareˉprobe.USER_FAULT_SERIAL_MARKER);
    }


    private static void Firmwareˉprobeˉbuildsˉreproducibly()
    {
        var First = Firmwareˉprobe.Buildˉapplication();
        var Second = Firmwareˉprobe.Buildˉapplication();
        Sequenceˉequal(First, Second);
        Equal(74_752, First.Length);
        Equal(
            "a8a9bebac3fba95d964187e43d67791c36cb0d541aa222150bf71790526c7f03",
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
        Equal(74_752, First.Length);
        Equal(
            "5c7d924c4bb336137213b28c7c5876d85c8f46272e4cb114494044a146b574e7",
            Objectˉdigest.Calculateˉsha256(First.AsSpan()));
        True(
            !First.AsSpan().SequenceEqual(Firmwareˉprobe.Buildˉapplication().AsSpan()),
            "The normal and invalid-opcode firmware scenarios produced identical images.");

        Equal(
            "panic=invalid-opcode\nvector=6\nerror-code=0\nstatus=panic\n",
            Firmwareˉprobe.INVALID_OPCODE_PANIC_MARKER);
        var Code = Uefiˉapplicationˉverifier.Verify(First.AsSpan()).Codeˉbytes;
        Equal(1, Countˉsequence(Code, [0x0F, 0x0B]));
        Equal(0, Countˉsequence(
            Uefiˉapplicationˉverifier.Verify(Firmwareˉprobe.Buildˉapplication().AsSpan()).Codeˉbytes,
            [0x0F, 0x0B]));
    }

    private static void Generalˉprotectionˉfirmwareˉprobeˉbuildsˉreproducibly()
    {
        var First = Firmwareˉprobe.Buildˉapplication(Firmwareˉprobeˉscenario.Generalˉprotection);
        var Second = Firmwareˉprobe.Buildˉapplication(Firmwareˉprobeˉscenario.Generalˉprotection);
        Sequenceˉequal(First, Second);
        Equal(74_752, First.Length);
        Equal(
            "4db4aa08f3b8d736a04a4d46fb4810d9a5b9e19b2f953cded520822ad2b64d6a",
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
            "windvale-os-boot 22\nentry=pass\nsystem-table=pass\nmemory-map=pass\nboot-services=exited\nmemory-owned=pass\nallocator=pass\nkernel-stack=pass\npaging=owned\nwvb-admission=pass\nprocess=isolated\nipc=pass\nHello from Windvale\ncpu-exceptions=armed\nnative-context=pass\nnative-wvb=pass\nwindvale-source=pass\nstatus=pass\nshutdown=poweroff\n",
            Firmwareˉprobe.SERIAL_MARKER);
        var Application = Firmwareˉprobe.Buildˉapplication();
        var Code = Uefiˉapplicationˉverifier.Verify(Application.AsSpan()).Codeˉbytes;
        Equal(1, Countˉsequence(Code, [0x48, 0x81, 0xEC, 0x88, 0x00, 0x00, 0x00]));
        Equal(1, Countˉsequence(Code, [0x49, 0x42, 0x49, 0x20, 0x53, 0x59, 0x53, 0x54]));
        Equal(1, Countˉsequence(Code, [0x42, 0x4F, 0x4F, 0x54, 0x53, 0x45, 0x52, 0x56]));
        Equal(1, Countˉsequence(Code, [0x81, 0x79, 0x0C, 0xF0, 0x00, 0x00, 0x00]));
        Equal(1, Countˉsequence(Code, [0x48, 0x83, 0xB9, 0xE8, 0x00, 0x00, 0x00, 0x00]));
        Equal(1, Countˉsequence(Code, [0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80]));
        Equal(1, Countˉsequence(Code, [0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80]));
        Equal(1, Countˉsequence(Code, [0xC7, 0x44, 0x24, 0x70, 0x03, 0x00, 0x00, 0x00]));
        Equal(3, Countˉsequence(Code, [0xFF, 0x50, 0x38]));
        Equal(1, Countˉsequence(Code, [0xFF, 0x50, 0x40]));
        Equal(1, Countˉsequence(Code, [0xFF, 0x50, 0x48]));
        Equal(1, Countˉsequence(Code, [0xFF, 0x90, 0xE8, 0x00, 0x00, 0x00]));
        Equal(3, Countˉsequence(Code, [0x57, 0x56, 0x4B, 0x48, 0x41, 0x4E, 0x44, 0x31]));
        Equal(4, Countˉsequence(Code, [0x57, 0x56, 0x4B, 0x4D, 0x45, 0x4D, 0x30, 0x32]));
        Equal(1, Countˉsequence(Code, [0xC7, 0x44, 0x24, 0x2C, 0x30, 0x00, 0x00, 0x00]));
        Equal(2, Countˉsequence(Code, [0x48, 0x83, 0xEC, 0x28]));
        Equal(2, Countˉsequence(Code, [0x48, 0x83, 0xEC, 0x78]));
        Equal(1, Countˉsequence(Code, [0x49, 0x8D, 0xA6, 0x00, 0x30, 0x00, 0x00]));
        Equal(4, Countˉsequence(Code, [0xFC, 0xF3, 0x48, 0xAB]));
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
        string code)
    {
        var Result = Kernelˉprocessˉplanner.Plan(
            paging, allocationˉaddress, userˉimage.AsSpan(), moduleˉdigest.AsSpan());
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

    private static void Equal<T>(T expected, T actual)
        where T : IEquatable<T>
    {
        if (!expected.Equals(actual))
        {
            throw new InvalidOperationException($"Expected '{expected}', received '{actual}'.");
        }
    }

    private static void Sequenceˉequal(ImmutableArray<byte> expected, ImmutableArray<byte> actual)
    {
        if (!expected.AsSpan().SequenceEqual(actual.AsSpan()))
        {
            throw new InvalidOperationException("Byte sequences differ.");
        }
    }

    private sealed record Testˉcase(string Name, Action Body);

    private sealed record Memoryˉdescriptorˉinput(
        uint Type,
        ulong Physicalˉaddress,
        ulong Pages);
}
