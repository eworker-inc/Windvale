using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Windvale.Bytecode;
using Windvale.Bootstrap;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime;
using Windvale.Runtime.Native;

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
        new("resource store emits deterministic verified WVRS", Resourceˉstoreˉemitsˉdeterministicˉimage),
        new("resource store rejects malformed and hostile images", Resourceˉstoreˉrejectsˉmalformedˉimages),
        new("Windvale resource service resolves a third typed boot resource", Resourceˉstoreˉserviceˉresolvesˉthirdˉresource),
        new("resource IPC emits bounded verified WVRQ and WVRY", Resourceˉipcˉemitsˉboundedˉrequestˉreply),
        new("resource IPC rejects malformed and hostile envelopes", Resourceˉipcˉrejectsˉmalformedˉenvelopes),
        new("Windvale resource service completes one live IPC lookup", Resourceˉserviceˉcompletesˉliveˉipcˉlookup),
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
            new Memoryˉdescriptorˉinput(7, 0x0030_0000, 438),
            new Memoryˉdescriptorˉinput(2, 0x0010_0000, 8),
            new Memoryˉdescriptorˉinput(7, 0x0020_0000, 16));
        var Result = Kernelˉmemoryˉplanner.Plan(Map, 40);
        True(Result.Success, Result.Diagnostics.IsEmpty ? "Memory planning failed." : Result.Diagnostics[0].Message);
        var Plan = Result.Plan!;
        Equal(0x0040_0000UL, Plan.Arenaˉaddress);
        Equal(Kernelˉmemoryˉcontract.ARENA_PAGES, Plan.Arenaˉpages);
        Equal(0x0040_0000UL, Plan.Stateˉaddress);
        Equal(0x0040_0040UL, Plan.Handoffˉcopyˉaddress);
        Equal(0x0040_1000UL, Plan.Stackˉaddress);
        Equal(16_384UL, Plan.Stackˉbytes);
        Equal(0x0040_5000UL, Plan.Stackˉtop);
        Equal(5UL, Plan.Firstˉfreeˉpage);
        Equal(Kernelˉmemoryˉcontract.INITIAL_FREE_PAGES, Plan.Freeˉpages);
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
        Memoryˉplanˉfails(Buildˉmemoryˉmap(new Memoryˉdescriptorˉinput(7, 0x0030_0000, 389)), 40, "WVOS4005");
        Memoryˉplanˉfails(
            Buildˉmemoryˉmap(
                new Memoryˉdescriptorˉinput(7, 0x0020_0000, Kernelˉmemoryˉcontract.ARENA_PAGES),
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

    private static void Resourceˉstoreˉemitsˉdeterministicˉimage()
    {
        var Entries = Buildˉresourceˉstoreˉentries();
        var First = Resourceˉstoreˉcodec.Write(Entries);
        var Second = Resourceˉstoreˉcodec.Write(Entries.Reverse());
        Sequenceˉequal(First, Second);

        Equal(Resourceˉstoreˉcontract.MAGIC,
            BinaryPrimitives.ReadUInt32LittleEndian(First.AsSpan()));
        Equal(Resourceˉstoreˉcontract.FORMAT_VERSION,
            BinaryPrimitives.ReadUInt32LittleEndian(First.AsSpan()[4..]));
        Equal((uint)First.Length,
            BinaryPrimitives.ReadUInt32LittleEndian(First.AsSpan()[8..]));
        Equal(3u, BinaryPrimitives.ReadUInt32LittleEndian(First.AsSpan()[12..]));
        Equal(3u * Resourceˉstoreˉcontract.ENTRY_BYTES,
            BinaryPrimitives.ReadUInt32LittleEndian(First.AsSpan()[16..]));

        var Verified = Resourceˉstoreˉverifier.Verify(First.AsSpan());
        Equal(3, Verified.Entries.Length);
        Equal("boot:main.budget", Verified.Entries[0].Name);
        Equal("boot:main.configuration", Verified.Entries[1].Name);
        Equal("boot:main.wvb", Verified.Entries[2].Name);
        Equal(2u, Verified.Entries[0].Identifier);
        Equal(3u, Verified.Entries[1].Identifier);
        Equal(1u, Verified.Entries[2].Identifier);
        True(Verified.Tryˉlookup("boot:main.configuration", out var Configuration),
            "The verified store did not resolve its third resource.");
        True(Configuration!.Kind == Resourceˉstoreˉkind.Opaqueˉbytes,
            "The third resource has the wrong kind.");
        Sequenceˉequal([(byte)3, 5, 8, 13], Configuration.Data);
        True(!Verified.Tryˉlookup("boot:missing", out _),
            "The verified store resolved an unknown resource name.");

        Throwsˉresourceˉstore("WVRS2001", () => _ = Resourceˉstoreˉcodec.Write([]));
        Throwsˉresourceˉstore(
            "WVRS2002",
            () => _ = Resourceˉstoreˉcodec.Write(
            [
                new(1, Resourceˉstoreˉkind.Opaqueˉbytes, "boot:one", []),
                new(1, Resourceˉstoreˉkind.Opaqueˉbytes, "boot:two", []),
            ]));
        Throwsˉresourceˉstore(
            "WVRS2002",
            () => _ = Resourceˉstoreˉcodec.Write(
            [
                new(1, Resourceˉstoreˉkind.Opaqueˉbytes, "boot:same", []),
                new(2, Resourceˉstoreˉkind.Opaqueˉbytes, "boot:same", []),
            ]));
        Throwsˉresourceˉstore(
            "WVRS2002",
            () => _ = Resourceˉstoreˉcodec.Write(
                [new(1, Resourceˉstoreˉkind.Opaqueˉbytes, "boot:\0bad", [])]));

        var Maximumˉdataˉbytes = Resourceˉstoreˉcontract.MAXIMUM_STORE_BYTES -
            Resourceˉstoreˉcontract.HEADER_BYTES - Resourceˉstoreˉcontract.ENTRY_BYTES - 1;
        var Maximum = Resourceˉstoreˉcodec.Write(
            [new(1, Resourceˉstoreˉkind.Opaqueˉbytes, "r", new byte[Maximumˉdataˉbytes].ToImmutableArray())]);
        Equal(Resourceˉstoreˉcontract.MAXIMUM_STORE_BYTES, Maximum.Length);
        Equal(Maximumˉdataˉbytes, Resourceˉstoreˉverifier.Verify(Maximum.AsSpan()).Entries[0].Data.Length);
        Throwsˉresourceˉstore(
            "WVRS2003",
            () => _ = Resourceˉstoreˉcodec.Write(
                [new(1, Resourceˉstoreˉkind.Opaqueˉbytes, "r", new byte[Maximumˉdataˉbytes + 1].ToImmutableArray())]));
    }

    private static void Resourceˉstoreˉrejectsˉmalformedˉimages()
    {
        var Canonical = Resourceˉstoreˉcodec.Write(Buildˉresourceˉstoreˉentries());
        const int FIRST_ENTRY = Resourceˉstoreˉcontract.HEADER_BYTES;
        const int SECOND_ENTRY = FIRST_ENTRY + Resourceˉstoreˉcontract.ENTRY_BYTES;
        const int NAME_REGION = Resourceˉstoreˉcontract.HEADER_BYTES +
            3 * Resourceˉstoreˉcontract.ENTRY_BYTES;
        const int SECOND_NAME = NAME_REGION + 16;

        Rejectˉresourceˉstore(ImmutableArray<byte>.Empty, "WVRS1001");
        Rejectˉresourceˉstore(new byte[Resourceˉstoreˉcontract.MAXIMUM_STORE_BYTES + 1], "WVRS1001");
        Rejectˉresourceˉstore(Replaceˉu32(Canonical, 0, 0), "WVRS1002");
        Rejectˉresourceˉstore(Replaceˉu32(Canonical, 4, 2), "WVRS1002");
        Rejectˉresourceˉstore(Replaceˉu32(Canonical, 8, (uint)Canonical.Length + 1), "WVRS1003");
        Rejectˉresourceˉstore(Replaceˉu32(Canonical, 12, 0), "WVRS1003");
        Rejectˉresourceˉstore(Replaceˉu32(Canonical, 16, 0), "WVRS1003");
        Rejectˉresourceˉstore(Replaceˉu32(Canonical, 28, 1), "WVRS1003");
        Rejectˉresourceˉstore(Replaceˉu32(Canonical, FIRST_ENTRY, 0), "WVRS1004");
        Rejectˉresourceˉstore(
            Replaceˉu32(Canonical, SECOND_ENTRY, 2),
            "WVRS1004");
        Rejectˉresourceˉstore(
            Replaceˉu32(Canonical, FIRST_ENTRY + Resourceˉstoreˉcontract.KIND_OFFSET, 4),
            "WVRS1005");
        Rejectˉresourceˉstore(
            Replaceˉu32(Canonical, FIRST_ENTRY + Resourceˉstoreˉcontract.FLAGS_OFFSET, 3),
            "WVRS1005");
        Rejectˉresourceˉstore(
            Replaceˉu32(Canonical, FIRST_ENTRY + Resourceˉstoreˉcontract.RESERVED_OFFSET, 1),
            "WVRS1005");
        Rejectˉresourceˉstore(
            Replaceˉu32(Canonical, FIRST_ENTRY + Resourceˉstoreˉcontract.NAME_OFFSET_OFFSET, 0),
            "WVRS1006");
        Rejectˉresourceˉstore(Replaceˉbyte(Canonical, SECOND_NAME, 0), "WVRS1006");
        Rejectˉresourceˉstore(Replaceˉbyte(Canonical, SECOND_NAME, 0xC0), "WVRS1006");
        Rejectˉresourceˉstore(
            Replaceˉu32(Canonical, FIRST_ENTRY + Resourceˉstoreˉcontract.DATA_OFFSET_OFFSET, 0),
            "WVRS1007");
        Rejectˉresourceˉstore(
            Replaceˉu32(Canonical, FIRST_ENTRY + Resourceˉstoreˉcontract.DATA_LENGTH_OFFSET, uint.MaxValue),
            "WVRS1007");
        Rejectˉresourceˉstore(
            Replaceˉbyte(Canonical, FIRST_ENTRY + Resourceˉstoreˉcontract.DIGEST_OFFSET, (byte)'g'),
            "WVRS1008");
        Rejectˉresourceˉstore(
            Replaceˉbyte(Canonical, Canonical.Length - 1, (byte)(Canonical[^1] ^ 1)),
            "WVRS1008");

        var Random = new Random(0x57565253);
        for (var Case = 0; Case < 256; Case++)
        {
            var Bytes = new byte[Random.Next(0, 8_193)];
            Random.NextBytes(Bytes);
            try
            {
                _ = Resourceˉstoreˉverifier.Verify(Bytes);
            }
            catch (Resourceˉstoreˉexception)
            {
            }
        }
    }

    private static void Resourceˉstoreˉserviceˉresolvesˉthirdˉresource()
    {
        var Coreˉsource = Loadˉresourceˉstoreˉsource("Windvale.Os.Services.Resource-Store-Core.wv");
        var Serviceˉsource = Loadˉresourceˉstoreˉsource("Windvale.Os.Services.Resource-Store-Service.wv");
        var Coreˉresult = Seedˉcompiler.Compileˉmodules(
            new("Libraries/Foundation/Resources/Resource-Store.wv", Coreˉsource),
            []);
        True(Coreˉresult.Success,
            "The Windvale resource-store core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        var Serviceˉresult = Seedˉcompiler.Compileˉmodules(
            new("Operating-System/Services/Resource-Store-Service.wv", Serviceˉsource),
            [new("Libraries/Foundation/Resources/Resource-Store.wv", Coreˉsource)]);
        True(Serviceˉresult.Success,
            "The Windvale resource-store service did not compile: " +
                string.Join(" | ", Serviceˉresult.Diagnostics));

        var Module = Moduleˉcodec.Readˉandˉverify(Serviceˉresult.Moduleˉbytes.AsSpan());
        True(Module.Module.Profile == Moduleˉprofile.Hosted,
            "The Windvale resource-store service is not hosted.");
        Equal(Capabilityˉcatalog.FILE_READ_BYTES, Module.Module.Capabilities.Single().Name);
        var Canonical = Resourceˉstoreˉcodec.Write(Buildˉresourceˉstoreˉentries());
        Equal(12_612, Coreˉresult.Moduleˉbytes.Length);
        Equal(
            "46350c610db3e1a2a445e0ee839bd6a7ffc37dc17afec6e1d21c086d25e78dc6",
            Objectˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));
        Equal(13_629, Serviceˉresult.Moduleˉbytes.Length);
        Equal(
            "3e366ad9888674188ca679c0c10ca5583478d2a382b6b756bd4013b30a1b73e1",
            Objectˉdigest.Calculateˉsha256(Serviceˉresult.Moduleˉbytes.AsSpan()));
        Equal(556, Canonical.Length);
        Equal(
            "ee2ee737db4f4ab480430616032c0d71e6eec1ee66dc2ee33b1d22ac5b3cde2f",
            Objectˉdigest.Calculateˉsha256(Canonical.AsSpan()));
        Equal(93, Runˉresourceˉstoreˉservice(Module, Canonical).Exitˉcode);
        Equal(1, Runˉresourceˉstoreˉservice(Module, []).Exitˉcode);

        const int FIRST_ENTRY = Resourceˉstoreˉcontract.HEADER_BYTES;
        const int SECOND_ENTRY = FIRST_ENTRY + Resourceˉstoreˉcontract.ENTRY_BYTES;
        const int NAME_REGION = Resourceˉstoreˉcontract.HEADER_BYTES +
            3 * Resourceˉstoreˉcontract.ENTRY_BYTES;
        const int SECOND_NAME = NAME_REGION + 16;
        Equal(2, Runˉresourceˉstoreˉservice(Module, Replaceˉu32(Canonical, 0, 0)).Exitˉcode);
        Equal(3, Runˉresourceˉstoreˉservice(Module, Replaceˉu32(Canonical, 4, 2)).Exitˉcode);
        Equal(4, Runˉresourceˉstoreˉservice(
            Module, Replaceˉu32(Canonical, 8, (uint)Canonical.Length + 1)).Exitˉcode);
        Equal(5, Runˉresourceˉstoreˉservice(
            Module, Replaceˉu32(Canonical, SECOND_ENTRY, 0)).Exitˉcode);
        Equal(6, Runˉresourceˉstoreˉservice(
            Module,
            Replaceˉu32(Canonical, SECOND_ENTRY + Resourceˉstoreˉcontract.KIND_OFFSET, 4)).Exitˉcode);
        Equal(7, Runˉresourceˉstoreˉservice(
            Module,
            Replaceˉu32(Canonical, SECOND_ENTRY + Resourceˉstoreˉcontract.FLAGS_OFFSET, 3)).Exitˉcode);
        Equal(8, Runˉresourceˉstoreˉservice(Module, Replaceˉbyte(Canonical, SECOND_NAME, 0)).Exitˉcode);
        Equal(9, Runˉresourceˉstoreˉservice(
            Module, Replaceˉbyte(Canonical, SECOND_NAME, (byte)'a')).Exitˉcode);
        Equal(10, Runˉresourceˉstoreˉservice(
            Module,
            Replaceˉu32(Canonical, SECOND_ENTRY + Resourceˉstoreˉcontract.DATA_OFFSET_OFFSET, 0)).Exitˉcode);
        Equal(11, Runˉresourceˉstoreˉservice(
            Module,
            Replaceˉbyte(Canonical, SECOND_ENTRY + Resourceˉstoreˉcontract.DIGEST_OFFSET, (byte)'g')).Exitˉcode);
        Equal(12, Runˉresourceˉstoreˉservice(
            Module, Replaceˉbyte(Canonical, SECOND_NAME + 10, (byte)'d')).Exitˉcode);

        Assertˉruntimeˉfailure("WVR3010", () => _ = Runˉresourceˉstoreˉservice(
            Module, Canonical, authorize: false));
        Assertˉruntimeˉfailure("WVR3022", () => _ = Runˉresourceˉstoreˉservice(
            Module, Canonical, includeˉstore: false));
    }

    private static void Resourceˉipcˉemitsˉboundedˉrequestˉreply()
    {
        const string CONFIGURATION = "boot:main.configuration";
        var First = Resourceˉserviceˉipcˉcodec.Writeˉrequest(42, CONFIGURATION, 4);
        var Second = Resourceˉserviceˉipcˉcodec.Writeˉrequest(42, CONFIGURATION, 4);
        Sequenceˉequal(First, Second);
        Equal(Resourceˉserviceˉipcˉcontract.REQUEST_HEADER_BYTES +
            Encoding.UTF8.GetByteCount(CONFIGURATION), First.Length);
        Equal(Resourceˉserviceˉipcˉcontract.REQUEST_MAGIC,
            BinaryPrimitives.ReadUInt32LittleEndian(First.AsSpan()));
        Equal(Resourceˉserviceˉipcˉcontract.FORMAT_VERSION,
            BinaryPrimitives.ReadUInt32LittleEndian(First.AsSpan()[4..]));

        var Request = Resourceˉserviceˉipcˉcodec.Verifyˉrequest(First.AsSpan());
        Equal(42u, Request.Requestˉid);
        Equal(4u, Request.Maximumˉdataˉbytes);
        Equal(CONFIGURATION, Request.Name);

        var Store = Resourceˉstoreˉcodec.Write(Buildˉresourceˉstoreˉentries());
        var Reply = Resourceˉserviceˉhandler.Handle(Store.AsSpan(), First.AsSpan());
        var Response = Resourceˉserviceˉipcˉcodec.Verifyˉresponse(Reply.AsSpan());
        Equal(42u, Response.Requestˉid);
        Equal(Resourceˉserviceˉstatus.Success, Response.Status);
        Equal(Resourceˉserviceˉfailureˉdomain.None, Response.Failureˉdomain);
        Equal(3u, Response.Identifier);
        Equal((uint)Resourceˉstoreˉkind.Opaqueˉbytes, Response.Kind);
        Equal(Resourceˉstoreˉcontract.ENTRY_FLAGS, Response.Attributes);
        Sequenceˉequal([(byte)3, 5, 8, 13], Response.Data);
        Equal("992cfe75c02f11b17a6ce1c2dd83d34948be3b82c11b62e8302c8bbf53c93cbd",
            Response.Digest);

        var Limited = Resourceˉserviceˉipcˉcodec.Writeˉrequest(43, CONFIGURATION, 3);
        var Limitedˉresponse = Resourceˉserviceˉipcˉcodec.Verifyˉresponse(
            Resourceˉserviceˉhandler.Handle(Store.AsSpan(), Limited.AsSpan()).AsSpan());
        Equal(Resourceˉserviceˉstatus.Responseˉlimit, Limitedˉresponse.Status);
        Equal(Resourceˉserviceˉfailureˉdomain.Request, Limitedˉresponse.Failureˉdomain);
        Equal((uint)Resourceˉserviceˉipcˉcontract.REQUEST_MAXIMUM_DATA_OFFSET,
            Limitedˉresponse.Failureˉoffset);

        var Missing = Resourceˉserviceˉipcˉcodec.Writeˉrequest(44, "boot:missing", 4);
        Equal(Resourceˉserviceˉstatus.Notˉfound,
            Resourceˉserviceˉipcˉcodec.Verifyˉresponse(
                Resourceˉserviceˉhandler.Handle(Store.AsSpan(), Missing.AsSpan()).AsSpan()).Status);

        var Maximumˉdata = new byte[Resourceˉserviceˉipcˉcontract.MAXIMUM_DATA_BYTES]
            .ToImmutableArray();
        var Maximumˉstore = Resourceˉstoreˉcodec.Write(
            [new(7, Resourceˉstoreˉkind.Opaqueˉbytes, "r", Maximumˉdata)]);
        var Maximumˉrequest = Resourceˉserviceˉipcˉcodec.Writeˉrequest(
            45, "r", Resourceˉserviceˉipcˉcontract.MAXIMUM_DATA_BYTES);
        var Maximumˉreply = Resourceˉserviceˉhandler.Handle(
            Maximumˉstore.AsSpan(), Maximumˉrequest.AsSpan());
        Equal(Resourceˉserviceˉipcˉcontract.MAXIMUM_MESSAGE_BYTES, Maximumˉreply.Length);
        Equal(Resourceˉserviceˉstatus.Success,
            Resourceˉserviceˉipcˉcodec.Verifyˉresponse(Maximumˉreply.AsSpan()).Status);

        var Tooˉlargeˉstore = Resourceˉstoreˉcodec.Write(
            [new(8, Resourceˉstoreˉkind.Opaqueˉbytes, "r",
                new byte[Resourceˉserviceˉipcˉcontract.MAXIMUM_DATA_BYTES + 1].ToImmutableArray())]);
        Equal(Resourceˉserviceˉstatus.Responseˉlimit,
            Resourceˉserviceˉipcˉcodec.Verifyˉresponse(
                Resourceˉserviceˉhandler.Handle(
                    Tooˉlargeˉstore.AsSpan(), Maximumˉrequest.AsSpan()).AsSpan()).Status);

        Throwsˉresourceˉserviceˉipc("WVRI3001", () =>
            _ = Resourceˉserviceˉipcˉcodec.Writeˉrequest(0, "r", 0));
        Throwsˉresourceˉserviceˉipc("WVRI3001", () =>
            _ = Resourceˉserviceˉipcˉcodec.Writeˉrequest(1, "", 0));
        Throwsˉresourceˉserviceˉipc("WVRI3001", () =>
            _ = Resourceˉserviceˉipcˉcodec.Writeˉrequest(1, "bad\0name", 0));
        Throwsˉresourceˉserviceˉipc("WVRI3001", () =>
            _ = Resourceˉserviceˉipcˉcodec.Writeˉrequest(
                1, new string('x', Resourceˉserviceˉipcˉcontract.MAXIMUM_NAME_BYTES + 1), 0));
        Throwsˉresourceˉserviceˉipc("WVRI3001", () =>
            _ = Resourceˉserviceˉipcˉcodec.Writeˉrequest(
                1, "r", Resourceˉserviceˉipcˉcontract.MAXIMUM_DATA_BYTES + 1));
    }

    private static void Resourceˉipcˉrejectsˉmalformedˉenvelopes()
    {
        var Store = Resourceˉstoreˉcodec.Write(Buildˉresourceˉstoreˉentries());
        var Request = Resourceˉserviceˉipcˉcodec.Writeˉrequest(
            51, "boot:main.configuration", 4);
        Rejectˉresourceˉrequest(ImmutableArray<byte>.Empty, "WVRI1001");
        Rejectˉresourceˉrequest(
            new byte[Resourceˉserviceˉipcˉcontract.REQUEST_HEADER_BYTES +
                Resourceˉserviceˉipcˉcontract.MAXIMUM_NAME_BYTES + 1], "WVRI1001");
        Rejectˉresourceˉrequest(Replaceˉu32(Request, 0, 0), "WVRI1002");
        Rejectˉresourceˉrequest(Replaceˉu32(Request, 4, 2), "WVRI1002");
        Rejectˉresourceˉrequest(Replaceˉu32(Request, 8, (uint)Request.Length + 1), "WVRI1003");
        Rejectˉresourceˉrequest(Replaceˉu32(Request, 12, 0), "WVRI1003");
        Rejectˉresourceˉrequest(Replaceˉu32(Request, 16, 2), "WVRI1003");
        Rejectˉresourceˉrequest(
            Replaceˉu32(Request, 20, Resourceˉserviceˉipcˉcontract.MAXIMUM_DATA_BYTES + 1),
            "WVRI1003");
        Rejectˉresourceˉrequest(Replaceˉu32(Request, 24, 0), "WVRI1003");
        Rejectˉresourceˉrequest(Replaceˉu32(Request, 28, 1), "WVRI1003");
        Rejectˉresourceˉrequest(
            Replaceˉbyte(Request, Resourceˉserviceˉipcˉcontract.REQUEST_HEADER_BYTES, 0),
            "WVRI1004");
        Rejectˉresourceˉrequest(
            Replaceˉbyte(Request, Resourceˉserviceˉipcˉcontract.REQUEST_HEADER_BYTES, 0xC0),
            "WVRI1004");

        var Malformedˉreply = Resourceˉserviceˉipcˉcodec.Verifyˉresponse(
            Resourceˉserviceˉhandler.Handle(
                Store.AsSpan(), Replaceˉu32(Request, 0, 0).AsSpan()).AsSpan());
        Equal(51u, Malformedˉreply.Requestˉid);
        Equal(Resourceˉserviceˉstatus.Malformedˉrequest, Malformedˉreply.Status);
        Equal(Resourceˉserviceˉfailureˉdomain.Request, Malformedˉreply.Failureˉdomain);
        Equal(0u, Malformedˉreply.Failureˉoffset);

        var Invalidˉstore = Resourceˉserviceˉipcˉcodec.Verifyˉresponse(
            Resourceˉserviceˉhandler.Handle([], Request.AsSpan()).AsSpan());
        Equal(Resourceˉserviceˉstatus.Invalidˉstore, Invalidˉstore.Status);
        Equal(Resourceˉserviceˉfailureˉdomain.Store, Invalidˉstore.Failureˉdomain);

        var Reply = Resourceˉserviceˉhandler.Handle(Store.AsSpan(), Request.AsSpan());
        Rejectˉresourceˉresponse(ImmutableArray<byte>.Empty, "WVRI2001");
        Rejectˉresourceˉresponse(
            new byte[Resourceˉserviceˉipcˉcontract.MAXIMUM_MESSAGE_BYTES + 1], "WVRI2001");
        Rejectˉresourceˉresponse(Replaceˉu32(Reply, 0, 0), "WVRI2002");
        Rejectˉresourceˉresponse(Replaceˉu32(Reply, 4, 2), "WVRI2002");
        Rejectˉresourceˉresponse(Replaceˉu32(Reply, 8, (uint)Reply.Length + 1), "WVRI2003");
        Rejectˉresourceˉresponse(Replaceˉu32(Reply, 40, 0), "WVRI2003");
        Rejectˉresourceˉresponse(Replaceˉu32(Reply, 44, 1), "WVRI2003");
        Rejectˉresourceˉresponse(Replaceˉu32(Reply, 16, 9), "WVRI2004");
        Rejectˉresourceˉresponse(Replaceˉu32(Reply, 20, 9), "WVRI2004");
        Rejectˉresourceˉresponse(Replaceˉu32(Reply, 12, 0), "WVRI2005");
        Rejectˉresourceˉresponse(Replaceˉu32(Reply, 28, 0), "WVRI2005");
        Rejectˉresourceˉresponse(Replaceˉu32(Reply, 32, 0), "WVRI2005");
        Rejectˉresourceˉresponse(Replaceˉu32(Reply, 36, 0), "WVRI2005");
        Rejectˉresourceˉresponse(
            Replaceˉbyte(Reply, Resourceˉserviceˉipcˉcontract.RESPONSE_DIGEST_OFFSET, (byte)'g'),
            "WVRI2005");

        var Missing = Resourceˉserviceˉhandler.Handle(
            Store.AsSpan(),
            Resourceˉserviceˉipcˉcodec.Writeˉrequest(52, "missing", 4).AsSpan());
        Rejectˉresourceˉresponse(Replaceˉu32(Missing, 20, 1), "WVRI2006");
        Rejectˉresourceˉresponse(Replaceˉu32(Missing, 28, 1), "WVRI2006");
        Rejectˉresourceˉresponse(
            Replaceˉbyte(Missing, Resourceˉserviceˉipcˉcontract.RESPONSE_DIGEST_OFFSET, 1),
            "WVRI2006");

        var Random = new Random(0x57565249);
        for (var Case = 0; Case < 256; Case++)
        {
            var Bytes = new byte[Random.Next(0, 4_097)];
            Random.NextBytes(Bytes);
            try
            {
                _ = Resourceˉserviceˉipcˉcodec.Verifyˉrequest(Bytes);
            }
            catch (Resourceˉserviceˉipcˉexception)
            {
            }
            try
            {
                _ = Resourceˉserviceˉipcˉcodec.Verifyˉresponse(Bytes);
            }
            catch (Resourceˉserviceˉipcˉexception)
            {
            }
        }
    }

    private static void Resourceˉserviceˉcompletesˉliveˉipcˉlookup()
    {
        var Storeˉsource = Loadˉresourceˉstoreˉsource("Windvale.Os.Services.Resource-Store-Core.wv");
        var Serviceˉsource = Loadˉresourceˉstoreˉsource("Windvale.Os.Services.Resource-Service-Core.wv");
        var Bridgeˉsource = Loadˉresourceˉstoreˉsource("Windvale.Os.Services.Resource-Service-Bridge.wv");
        var Serviceˉresult = Seedˉcompiler.Compileˉmodules(
            new("Operating-System/Services/Resource-Service-Core.wv", Serviceˉsource),
            [new("Libraries/Foundation/Resources/Resource-Store.wv", Storeˉsource)]);
        True(Serviceˉresult.Success,
            "The Windvale resource-service core did not compile: " +
                string.Join(" | ", Serviceˉresult.Diagnostics));
        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(
            new("Operating-System/Services/Resource-Service-Bridge.wv", Bridgeˉsource),
            [
                new("Libraries/Foundation/Resources/Resource-Store.wv", Storeˉsource),
                new("Operating-System/Services/Resource-Service-Core.wv", Serviceˉsource),
            ]);
        True(Bridgeˉresult.Success,
            "The hosted Windvale resource-service bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));

        var Module = Moduleˉcodec.Readˉandˉverify(Bridgeˉresult.Moduleˉbytes.AsSpan());
        True(Module.Module.Profile == Moduleˉprofile.Hosted,
            "The Windvale resource-service bridge is not hosted.");
        Equal(Capabilityˉcatalog.FILE_READ_BYTES, Module.Module.Capabilities.Single().Name);

        var Store = Resourceˉstoreˉcodec.Write(Buildˉresourceˉstoreˉentries());
        var Request = Resourceˉserviceˉipcˉcodec.Writeˉrequest(
            0x5749, "boot:main.configuration", 4);
        var Exchange = new Resourceˉserviceˉexchange();
        Exchange.Sendˉrequest(
            Resourceˉserviceˉexchangeˉcontract.CLIENT_ENDPOINT,
            Resourceˉserviceˉexchangeˉcontract.CLIENT_RIGHTS,
            Request.AsSpan());
        Equal(Resourceˉserviceˉexchangeˉstate.Requestˉready, Exchange.State);
        var Serviceˉrequest = Exchange.Receiveˉrequest(
            Resourceˉserviceˉexchangeˉcontract.SERVICE_ENDPOINT,
            Resourceˉserviceˉexchangeˉcontract.SERVICE_RIGHTS);
        Sequenceˉequal(Request, Serviceˉrequest);
        Equal(Resourceˉserviceˉexchangeˉstate.Serviceˉprocessing, Exchange.State);

        var Interpreted = Runˉresourceˉserviceˉbridge(Module, Store, Serviceˉrequest);
        var Oracle = Resourceˉserviceˉhandler.Handle(Store.AsSpan(), Serviceˉrequest.AsSpan());
        Sequenceˉequal(Oracle, Interpreted.Bytes);
        var Malformedˉrequest = Replaceˉu32(Request, 0, 0);
        Sequenceˉequal(
            Resourceˉserviceˉhandler.Handle(Store.AsSpan(), Malformedˉrequest.AsSpan()),
            Runˉresourceˉserviceˉbridge(Module, Store, Malformedˉrequest).Bytes);
        var Oversizedˉrequest = new byte[Resourceˉserviceˉipcˉcontract.MAXIMUM_MESSAGE_BYTES + 1]
            .ToImmutableArray();
        Sequenceˉequal(
            Resourceˉserviceˉhandler.Handle(Store.AsSpan(), Oversizedˉrequest.AsSpan()),
            Runˉresourceˉserviceˉbridge(Module, Store, Oversizedˉrequest).Bytes);
        var Missingˉrequest = Resourceˉserviceˉipcˉcodec.Writeˉrequest(0x574A, "missing", 4);
        Sequenceˉequal(
            Resourceˉserviceˉhandler.Handle(Store.AsSpan(), Missingˉrequest.AsSpan()),
            Runˉresourceˉserviceˉbridge(Module, Store, Missingˉrequest).Bytes);
        var Limitedˉrequest = Resourceˉserviceˉipcˉcodec.Writeˉrequest(
            0x574B, "boot:main.configuration", 3);
        Sequenceˉequal(
            Resourceˉserviceˉhandler.Handle(Store.AsSpan(), Limitedˉrequest.AsSpan()),
            Runˉresourceˉserviceˉbridge(Module, Store, Limitedˉrequest).Bytes);
        Sequenceˉequal(
            Resourceˉserviceˉhandler.Handle([], Request.AsSpan()),
            Runˉresourceˉserviceˉbridge(Module, [], Request).Bytes);
        Exchange.Sendˉreply(
            Resourceˉserviceˉexchangeˉcontract.SERVICE_ENDPOINT,
            Resourceˉserviceˉexchangeˉcontract.SERVICE_RIGHTS,
            Interpreted.Bytes.AsSpan());
        var Clientˉreply = Exchange.Receiveˉreply(
            Resourceˉserviceˉexchangeˉcontract.CLIENT_ENDPOINT,
            Resourceˉserviceˉexchangeˉcontract.CLIENT_RIGHTS);
        var Response = Resourceˉserviceˉipcˉcodec.Verifyˉresponse(Clientˉreply.AsSpan());
        Equal(Resourceˉserviceˉstatus.Success, Response.Status);
        Equal(3u, Response.Identifier);
        Sequenceˉequal([(byte)3, 5, 8, 13], Response.Data);
        Equal(Resourceˉserviceˉexchangeˉstate.Completed, Exchange.State);
        Exchange.Close();
        Equal(Resourceˉserviceˉexchangeˉstate.Closed, Exchange.State);

        var Opaque = new Resourceˉserviceˉexchange();
        Opaque.Sendˉrequest(
            Resourceˉserviceˉexchangeˉcontract.CLIENT_ENDPOINT,
            Resourceˉserviceˉexchangeˉcontract.CLIENT_RIGHTS,
            [0xA5]);
        Sequenceˉequal([0xA5], Opaque.Receiveˉrequest(
            Resourceˉserviceˉexchangeˉcontract.SERVICE_ENDPOINT,
            Resourceˉserviceˉexchangeˉcontract.SERVICE_RIGHTS));
        Opaque.Peerˉexit();
        Opaque.Close();

        Throwsˉresourceˉserviceˉipc("WVRI4001", () =>
            new Resourceˉserviceˉexchange().Sendˉrequest(
                Resourceˉserviceˉexchangeˉcontract.SERVICE_ENDPOINT,
                Resourceˉserviceˉexchangeˉcontract.SERVICE_RIGHTS,
                Request.AsSpan()));
        Throwsˉresourceˉserviceˉipc("WVRI4002", () =>
            new Resourceˉserviceˉexchange().Receiveˉreply(
                Resourceˉserviceˉexchangeˉcontract.CLIENT_ENDPOINT,
                Resourceˉserviceˉexchangeˉcontract.CLIENT_RIGHTS));
        Throwsˉresourceˉserviceˉipc("WVRI4003", () =>
            new Resourceˉserviceˉexchange().Sendˉrequest(
                Resourceˉserviceˉexchangeˉcontract.CLIENT_ENDPOINT,
                Resourceˉserviceˉexchangeˉcontract.CLIENT_RIGHTS,
                new byte[Resourceˉserviceˉipcˉcontract.MAXIMUM_MESSAGE_BYTES + 1]));

        Assertˉruntimeˉfailure("WVR3010", () =>
            _ = Runˉresourceˉserviceˉbridge(Module, Store, Request, authorize: false));
        Assertˉruntimeˉfailure("WVR3022", () =>
            _ = Runˉresourceˉserviceˉbridge(Module, Store, Request, includeˉstore: false));
        Assertˉruntimeˉfailure("WVR3022", () =>
            _ = Runˉresourceˉserviceˉbridge(Module, Store, Request, includeˉrequest: false));

        Equal(19_515, Serviceˉresult.Moduleˉbytes.Length);
        Equal(
            "f151fd559b607b3f0dd8b3ae06399c91b2864a9ce7b30a07da1cffa0dc75e129",
            Objectˉdigest.Calculateˉsha256(Serviceˉresult.Moduleˉbytes.AsSpan()));
        Equal(19_457, Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            "c13d94aa5fc02676ddbaac315c4b55f0c26dbfd28bbd4f821123f67112db1b3f",
            Objectˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));
    }

    private static void Pageˉallocatorˉisˉboundedˉandˉzeroing()
    {
        var Plan = Kernelˉmemoryˉplanner.Plan(
            Buildˉmemoryˉmap(new Memoryˉdescriptorˉinput(
                7, 0x0020_0000, Kernelˉmemoryˉcontract.ARENA_PAGES)),
            40).Plan!;
        var Arena = Enumerable.Repeat((byte)0xA5, checked((int)Kernelˉmemoryˉcontract.ARENA_BYTES)).ToArray();
        var Allocator = new Kernelˉpageˉallocator(Plan, Arena);
        True(Arena.All(Value => Value == 0), "Kernel arena initialization did not clear every byte.");

        Array.Fill(Arena, (byte)0x5A, 5 * 4_096, 4_096);
        Equal(0x0020_5000UL, Allocator.Allocateˉpages(1)!.Value);
        True(Arena.AsSpan(5 * 4_096, 4_096).IndexOfAnyExcept((byte)0) < 0, "Allocated page was not zeroed.");
        Equal(Kernelˉmemoryˉcontract.INITIAL_FREE_PAGES - 1, Allocator.Remainingˉpages);
        True(Allocator.Allocateˉpages(0) is null, "A zero-page allocation succeeded.");
        Equal(0x0020_6000UL, Allocator.Allocateˉpages(
            Kernelˉmemoryˉcontract.INITIAL_FREE_PAGES - 1)!.Value);
        Equal(0UL, Allocator.Remainingˉpages);
        True(Allocator.Allocateˉpages(1) is null, "An exhausted allocator returned a page.");
        Array.Fill(Arena, (byte)0xC7, 23 * 4_096,
            checked((int)Kernelˉprocessˉcontract.CLIENT_ALLOCATION_PAGES) * 4_096);
        True(!Allocator.Releaseˉtailˉpages(0x0021_6000UL,
                Kernelˉprocessˉcontract.CLIENT_ALLOCATION_PAGES),
            "A non-tail extent was released.");
        True(!Allocator.Releaseˉtailˉpages(0x0021_7000UL, 0),
            "A zero-page extent was released.");
        True(Allocator.Releaseˉtailˉpages(0x0021_7000UL,
                Kernelˉprocessˉcontract.CLIENT_ALLOCATION_PAGES),
            "The exact client-sized tail extent was not released.");
        Equal(Kernelˉprocessˉcontract.CLIENT_ALLOCATION_PAGES, Allocator.Remainingˉpages);
        True(Arena.AsSpan(23 * 4_096,
                checked((int)Kernelˉprocessˉcontract.CLIENT_ALLOCATION_PAGES) * 4_096)
            .IndexOfAnyExcept((byte)0) < 0,
            "The released tail extent retained stale bytes.");
        Equal(0x0021_7000UL, Allocator.Allocateˉpages(
            Kernelˉprocessˉcontract.CLIENT_ALLOCATION_PAGES)!.Value);
        Equal(0UL, Allocator.Remainingˉpages);
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
            Kernelˉpagingˉplanner.Readˉentry(Plan, 5, 188));

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
            "acd75cde1392238445fa925019af91ff34f27eceb456c1b79f4b4bd21736ba5e",
            Objectˉdigest.Calculateˉsha256(First.Objectˉbytes.AsSpan()));
        Equal(851, First.Codeˉbytes.Length);
        Equal(
            "eda35ff5fe6e910e1a2582f30f3b8508457863baa20246de1fe6da3387c08363",
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
        Equal(1, Countˉsequence(First.Codeˉbytes, [0xB9, 0xC0, 0x00, 0x00, 0x00]));
        Equal(1, Countˉsequence(First.Codeˉbytes, [0x57, 0x56, 0x4B, 0x50, 0x41, 0x47, 0x30, 0x34]));
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
        Equal(222u, First.Installerˉbytes);
        Equal(222, First.Codeˉbytes.Length);
        Equal(483, First.Objectˉbytes.Length);
        Equal(
            "9caeb7ce353bca33e3bbac729ecca0423d59f8ce6b65ccd6b54fa53c381d617c",
            Objectˉdigest.Calculateˉsha256(First.Objectˉbytes.AsSpan()));
        Equal(
            "b7fd72dbc78d6f02511f9769a727421c7cb91409eef2164b4a51aced4dc2df14",
            Objectˉdigest.Calculateˉsha256(First.Codeˉbytes.AsSpan()));

        Equal(3, Kernelˉexceptionˉcontract.FORMAT_VERSION);
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
        Equal(3, Object.Symbols.Length);
        Equal(Kernelˉexceptionˉcontract.INSTALL_SYMBOL, Object.Symbols[0].Name);
        True(Object.Symbols[0].Binding == Objectˉsymbolˉbinding.Export, "The exception installer is not exported.");
        Equal(First.Installerˉbytes, Object.Symbols[0].Size);
        Equal(Kernelˉexceptionˉcontract.GENERAL_PROTECTION_ENTRY_SYMBOL, Object.Symbols[1].Name);
        True(Object.Symbols[1].Binding == Objectˉsymbolˉbinding.Import, "The vector-13 WVA entry is not imported.");
        Equal(Kernelˉexceptionˉcontract.INVALID_OPCODE_ENTRY_SYMBOL, Object.Symbols[2].Name);
        True(Object.Symbols[2].Binding == Objectˉsymbolˉbinding.Import, "The vector-6 WVA entry is not imported.");
        Equal(2, Object.Relocations.Length);
        True(Object.Relocations[0].Kind == Objectˉrelocationˉkind.Relativeˉi32, "The vector-6 gate target is not relative.");
        Equal(2u, Object.Relocations[0].Symbolˉindex);
        Equal(63u, Object.Relocations[0].Offset);
        Equal(-4L, Object.Relocations[0].Addend);
        True(Object.Relocations[1].Kind == Objectˉrelocationˉkind.Relativeˉi32, "The vector-13 gate target is not relative.");
        Equal(1u, Object.Relocations[1].Symbolˉindex);
        Equal(115u, Object.Relocations[1].Offset);
        Equal(-4L, Object.Relocations[1].Addend);

        Equal(1, Countˉsequence(First.Codeˉbytes, [0xB9, 0x00, 0x02, 0x00, 0x00, 0xFC, 0xF3, 0x48, 0xAB]));
        Equal(1, Countˉsequence(First.Codeˉbytes, [0x31, 0xC0, 0x8C, 0xC8, 0x85, 0xC0]));
        Equal(1, Countˉsequence(First.Codeˉbytes, [0x41, 0xC6, 0x40, 0x65, 0x8E]));
        Equal(1, Countˉsequence(First.Codeˉbytes, [0x41, 0xC6, 0x80, 0xD5, 0x00, 0x00, 0x00, 0x8E]));
        Equal(1, Countˉsequence(First.Codeˉbytes, [0x66, 0x41, 0xC7, 0x80, 0xE0, 0x00, 0x00, 0x00, 0xDF, 0x00]));
        Equal(1, Countˉsequence(First.Codeˉbytes, [0xFA, 0x41, 0x0F, 0x01, 0x98, 0xE0, 0x00, 0x00, 0x00]));
        Equal(0, Countˉsequence(First.Codeˉbytes, [0xEC]));
        Equal(0, Countˉsequence(First.Codeˉbytes, [0xEE]));
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
        Equal(6_973, First.Policyˉmoduleˉbytes.Length);
        Equal("04c91ebca24d72ba13ab3b8c6d3d0fb4a1ad0be807de58584caa4df5005ab956",
            Objectˉdigest.Calculateˉsha256(First.Policyˉmoduleˉbytes.AsSpan()));
        Equal(47_496, First.Policyˉnativeˉobjectˉbytes.Length);
        Equal("6ec4ca02eb59959ebcfc245fee07dd0a8d1c26e5e2f6d9de1566c37ea65c5f9b",
            Objectˉdigest.Calculateˉsha256(First.Policyˉnativeˉobjectˉbytes.AsSpan()));
        Equal(525, First.Initˉserviceˉmoduleˉbytes.Length);
        Equal("0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467",
            Objectˉdigest.Calculateˉsha256(First.Initˉserviceˉmoduleˉbytes.AsSpan()));
        Equal(3_455, First.Initˉserviceˉnativeˉobjectˉbytes.Length);
        Equal("4b8126d1baa38054fc70165be3c2f9519e7bea7e1f4d5596bcae36f2567ddf11",
            Objectˉdigest.Calculateˉsha256(First.Initˉserviceˉnativeˉobjectˉbytes.AsSpan()));
        Equal(1_929, First.Initˉserviceˉshimˉobjectˉbytes.Length);
        Equal("93d212d61723e5d43d30a4fbe3319b5b448cb7f52e147cd02f95a69cb722b53f",
            Objectˉdigest.Calculateˉsha256(First.Initˉserviceˉshimˉobjectˉbytes.AsSpan()));
        Equal(5_015, First.Initˉserviceˉimageˉbytes.Length);
        Equal("0e4afe4990bb6c4dfe1f255ec51594a58a6aaa1ef857d9ea48d44eb5e58e9a5e",
            Objectˉdigest.Calculateˉsha256(First.Initˉserviceˉimageˉbytes.AsSpan()));
        Equal(1_195, First.Resourceˉstoreˉbytes.Length);
        Equal("e06cb88bc97c8a8c8413c476c41ec86eafb8d1ee3fab0daee8e3b50e788023b8",
            Objectˉdigest.Calculateˉsha256(First.Resourceˉstoreˉbytes.AsSpan()));
        Equal("e06cb88bc97c8a8c8413c476c41ec86eafb8d1ee3fab0daee8e3b50e788023b8",
            Convert.ToHexString(First.Resourceˉstoreˉdigest.AsSpan()).ToLowerInvariant());
        Equal(56_165, First.Interpreterˉmoduleˉbytes.Length);
        Equal("3669e94d712bd5a78f0061e29d8054ed3b54b687efc9508114f79bb78aa8832f",
            Objectˉdigest.Calculateˉsha256(First.Interpreterˉmoduleˉbytes.AsSpan()));
        Equal(445_684, First.Interpreterˉnativeˉobjectˉbytes.Length);
        Equal("3840f10bacf8b7b498f28646b947a53841baf00241cd21bc94423ab5a43e8e31",
            Objectˉdigest.Calculateˉsha256(First.Interpreterˉnativeˉobjectˉbytes.AsSpan()));
        Equal(462, First.Bootˉresourceˉserviceˉstencilˉobjectˉbytes.Length);
        Equal("fde44aad9549731d53c5ccf3a57733b3619df94369b61ef27a693e1059784bc9",
            Objectˉdigest.Calculateˉsha256(First.Bootˉresourceˉserviceˉstencilˉobjectˉbytes.AsSpan()));
        Equal(462, First.Bootˉresourceˉserviceˉobjectˉbytes.Length);
        Equal("ecb940abb9de8086d50ae418853021cf1f7566a9415a5a3a3b4e5cc45ed5e78c",
            Objectˉdigest.Calculateˉsha256(First.Bootˉresourceˉserviceˉobjectˉbytes.AsSpan()));
        Equal(1_008, First.Clientˉshimˉobjectˉbytes.Length);
        Equal("1082c2990879b0ef63b9a25392ea1f7f1afd00628faeb92af915d7e4611cdd39",
            Objectˉdigest.Calculateˉsha256(First.Clientˉshimˉobjectˉbytes.AsSpan()));
        Equal(977, Faultˉfirst.Clientˉshimˉobjectˉbytes.Length);
        Equal("6e89b39fe3cb4258b0c2061f0d48a4de5d098801bf379d2a61fe094349118530",
            Objectˉdigest.Calculateˉsha256(Faultˉfirst.Clientˉshimˉobjectˉbytes.AsSpan()));
        Equal(445_789, First.Clientˉimageˉbytes.Length);
        Equal("c3046836c9048f8aef2765337a2831a34dd8014489afcbcc1aceddd1ce019578",
            Objectˉdigest.Calculateˉsha256(First.Clientˉimageˉbytes.AsSpan()));
        Equal(445_773, Faultˉfirst.Clientˉimageˉbytes.Length);
        Equal("dd880728016b305c002cd6270e18168de613c513eb444943c1429a30e037a19e",
            Objectˉdigest.Calculateˉsha256(Faultˉfirst.Clientˉimageˉbytes.AsSpan()));

        Sequenceˉequal(First.Policyˉmoduleˉbytes, Second.Policyˉmoduleˉbytes);
        Sequenceˉequal(First.Policyˉnativeˉobjectˉbytes, Second.Policyˉnativeˉobjectˉbytes);
        Sequenceˉequal(First.Initˉserviceˉmoduleˉbytes, Second.Initˉserviceˉmoduleˉbytes);
        Sequenceˉequal(First.Initˉserviceˉshimˉobjectˉbytes, Second.Initˉserviceˉshimˉobjectˉbytes);
        Sequenceˉequal(First.Initˉserviceˉimageˉbytes, Second.Initˉserviceˉimageˉbytes);
        Sequenceˉequal(First.Interpreterˉmoduleˉbytes, Second.Interpreterˉmoduleˉbytes);
        Sequenceˉequal(First.Interpreterˉnativeˉobjectˉbytes, Second.Interpreterˉnativeˉobjectˉbytes);
        Sequenceˉequal(First.Bootˉresourceˉserviceˉstencilˉobjectˉbytes,
            Second.Bootˉresourceˉserviceˉstencilˉobjectˉbytes);
        Sequenceˉequal(First.Bootˉresourceˉserviceˉobjectˉbytes,
            Second.Bootˉresourceˉserviceˉobjectˉbytes);
        Equal(First.Bootˉresourceˉserviceˉoffset, Second.Bootˉresourceˉserviceˉoffset);
        Sequenceˉequal(First.Admittedˉprogramˉbytes, Second.Admittedˉprogramˉbytes);
        Sequenceˉequal(First.Resourceˉstoreˉbytes, Second.Resourceˉstoreˉbytes);
        Sequenceˉequal(First.Resourceˉstoreˉdigest, Second.Resourceˉstoreˉdigest);
        Sequenceˉequal(First.Clientˉshimˉobjectˉbytes, Second.Clientˉshimˉobjectˉbytes);
        Sequenceˉequal(First.Clientˉimageˉbytes, Second.Clientˉimageˉbytes);
        Sequenceˉequal(Faultˉfirst.Clientˉimageˉbytes, Faultˉsecond.Clientˉimageˉbytes);
        Sequenceˉequal(First.Policyˉmoduleˉbytes, Faultˉfirst.Policyˉmoduleˉbytes);
        Sequenceˉequal(First.Policyˉnativeˉobjectˉbytes, Faultˉfirst.Policyˉnativeˉobjectˉbytes);
        Sequenceˉequal(First.Resourceˉstoreˉbytes, Faultˉfirst.Resourceˉstoreˉbytes);
        True(!First.Clientˉimageˉbytes.AsSpan().SequenceEqual(Faultˉfirst.Clientˉimageˉbytes.AsSpan()),
            "The normal and deliberate-fault user images are identical.");

        var Policy = Moduleˉcodec.Readˉandˉverify(First.Policyˉmoduleˉbytes.AsSpan());
        Equal("Processˉfoundation", Policy.Module.Name);
        True(Policy.Module.Profile == Moduleˉprofile.Portable, "The process policy is not portable Windvale.");
        Equal(0, Policy.Module.Capabilities.Length);
        Equal(Kernelˉprocessˉcontract.POLICY_TOKEN,
            Runˉportableˉmain(First.Policyˉmoduleˉbytes).Exitˉcode);
        Equal((int)Kernelˉprocessˉcontract.RESOURCE_SET_TOKEN,
            Runˉportableˉmain(First.Initˉserviceˉmoduleˉbytes).Exitˉcode);
        var Interpreterˉmodule = Moduleˉcodec.Readˉandˉverify(
            First.Interpreterˉmoduleˉbytes.AsSpan());
        True(Interpreterˉmodule.Module.Profile == Moduleˉprofile.Hosted,
            "The bytecode interpreter is not an explicitly hosted module.");
        Equal(1, Interpreterˉmodule.Module.Capabilities.Length);
        Equal(Capabilityˉcatalog.FILE_READ_BYTES,
            Interpreterˉmodule.Module.Capabilities[0].Name);
        Equal(2, Interpreterˉmodule.Module.Data.Length);
        Equal(1, Interpreterˉmodule.Module.Data.Count(Data => Data is Textˉdataˉdeclaration
            { Name: "Runtimeˉinputˉname", Value: "boot:main.wvb" }));
        Equal(1, Interpreterˉmodule.Module.Data.Count(Data => Data is Textˉdataˉdeclaration
            { Name: "Runtimeˉbudgetˉname", Value: "boot:main.budget" }));
        Equal(2, Interpreterˉmodule.Functions.SelectMany(Function => Function.Instructions)
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
        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            var Nativeˉdirectory = Path.Combine(
                Path.GetTempPath(), $"windvale-os-interpreter-{Guid.NewGuid():N}");
            var Previousˉdirectory = Environment.CurrentDirectory;
            Directory.CreateDirectory(Nativeˉdirectory);
            try
            {
                Environment.CurrentDirectory = Nativeˉdirectory;
                if (OperatingSystem.IsWindows())
                {
                    File.WriteAllBytes("boot", []);
                }
                File.WriteAllBytes("boot:main.wvb", Admission.Embeddedˉmoduleˉbytes.AsSpan());
                File.WriteAllBytes(
                    "boot:main.budget",
                    [(byte)Kernelˉprocessˉcontract.EXECUTION_BUDGET, 0, 0, 0]);
                var Nativeˉhost = new Nativeˉhostˉservices(
                    null,
                    [Capabilityˉcatalog.FILE_READ_BYTES],
                    new Hostedˉresourceˉcontext([], TextWriter.Null, TextWriter.Null),
                    fileˉinput: Nativeˉfileˉinput.Hostˉfileˉsystem());
                Equal(
                    Kernelˉprocessˉcontract.EXPECTED_RESULT,
                    X64ˉnativeˉexecutor.Executeˉi32(
                        X64ˉnativeˉbackend.Compile(Interpreterˉmodule).Fragment,
                        maximumˉinstructions: Kernelˉprocessˉcontract.CLIENT_INSTRUCTION_BUDGET,
                        maximumˉcallˉdepth: (int)Kernelˉprocessˉcontract.CLIENT_CALL_DEPTH_BUDGET,
                        hostˉservices: Nativeˉhost));
            }
            finally
            {
                Environment.CurrentDirectory = Previousˉdirectory;
                Directory.Delete(Nativeˉdirectory, recursive: true);
            }
        }
        Equal(-34, Runˉinterpreter(
            First.Interpreterˉmoduleˉbytes, Admission.Embeddedˉmoduleˉbytes,
            [198, 0, 0, 0]).Exitˉcode);
        Equal(-17, Runˉinterpreter(
            First.Interpreterˉmoduleˉbytes, Admission.Embeddedˉmoduleˉbytes,
            [0, 0, 0, 0]).Exitˉcode);
        Equal(-17, Runˉinterpreter(
            First.Interpreterˉmoduleˉbytes, Admission.Embeddedˉmoduleˉbytes,
            [1, 1, 0, 0]).Exitˉcode);
        Equal(-16, Runˉinterpreter(
            First.Interpreterˉmoduleˉbytes, Admission.Embeddedˉmoduleˉbytes,
            [203, 0, 0]).Exitˉcode);
        var Missingˉbudgetˉrejected = false;
        try
        {
            _ = Runˉinterpreter(
                First.Interpreterˉmoduleˉbytes, Admission.Embeddedˉmoduleˉbytes,
                [199, 0, 0, 0], includeˉbudget: false);
        }
        catch (Runtimeˉexception Exception)
        {
            Missingˉbudgetˉrejected = Exception.Code == "WVR3022";
        }
        True(Missingˉbudgetˉrejected, "A missing execution-budget resource was accepted.");
        Sequenceˉequal(First.Interpreterˉmoduleˉbytes,
            Kernelˉprocessˉimage.Compileˉinterpreterˉmodule());

        var Codeˉvariant = Mutate(Admission.Embeddedˉmoduleˉbytes, 395).ToImmutableArray();
        Equal(9, Runˉinterpreter(
            First.Interpreterˉmoduleˉbytes, Codeˉvariant).Exitˉcode);

        Equal(-8, Runˉinterpreter(First.Interpreterˉmoduleˉbytes,
            Admission.Embeddedˉmoduleˉbytes[..^1]).Exitˉcode);
        Equal(-2, Runˉinterpreter(First.Interpreterˉmoduleˉbytes,
            Mutate(Admission.Embeddedˉmoduleˉbytes, 0).ToImmutableArray()).Exitˉcode);
        Equal(-6, Runˉinterpreter(First.Interpreterˉmoduleˉbytes,
            Mutate(Admission.Embeddedˉmoduleˉbytes, 12).ToImmutableArray()).Exitˉcode);
        Equal(-6, Runˉinterpreter(First.Interpreterˉmoduleˉbytes,
            Mutate(Admission.Embeddedˉmoduleˉbytes, 69).ToImmutableArray()).Exitˉcode);
        Equal(9, Runˉinterpreter(First.Interpreterˉmoduleˉbytes,
            Codeˉvariant).Exitˉcode);
        Equal(-15, Runˉinterpreter(First.Interpreterˉmoduleˉbytes,
            Mutate(Admission.Embeddedˉmoduleˉbytes, 276).ToImmutableArray()).Exitˉcode);
        Sequenceˉequal(Admission.Embeddedˉmoduleˉbytes,
            Kernelˉwvbˉadmission.Build().Embeddedˉmoduleˉbytes);
        Equal(
            "9ccfed0509e84bfc63979c6dc13170c14762efbdaa448b4c5894325f31aa7761",
            Convert.ToHexString(First.Admittedˉprogramˉdigest.AsSpan()).ToLowerInvariant());
        Equal(
            "3669e94d712bd5a78f0061e29d8054ed3b54b687efc9508114f79bb78aa8832f",
            Convert.ToHexString(First.Interpreterˉdigest.AsSpan()).ToLowerInvariant());
        Equal(
            "0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467",
            Convert.ToHexString(First.Initˉserviceˉdigest.AsSpan()).ToLowerInvariant());
        Equal(
            "add7f2a4843f8c512c0e2875546581db11b9ba227ee008b5f719dfacb125de76",
            Convert.ToHexString(First.Executionˉbudgetˉdigest.AsSpan()).ToLowerInvariant());

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
        var Configurationˉrequest = Resourceˉserviceˉipcˉcodec.Writeˉrequest(
            1, "boot:main.configuration", 4);
        var Configurationˉreply = Resourceˉserviceˉhandler.Handle(
            Resourceˉstoreˉcodec.Write(Buildˉresourceˉstoreˉentries()).AsSpan(),
            Configurationˉrequest.AsSpan());
        Equal(1, Serviceˉobject.Sections.Length);
        True(Serviceˉobject.Sections[0].Kind == Objectˉsectionˉkind.Code,
            "The init resource service shim is not code-only dynamic lookup logic.");
        Equal(0, Countˉsequence(Serviceˉobject.Sections[0].Data, Configurationˉrequest));
        Equal(0, Countˉsequence(Serviceˉobject.Sections[0].Data, Configurationˉreply));
        var Verifiedˉstore = Resourceˉstoreˉverifier.Verify(First.Resourceˉstoreˉbytes.AsSpan());
        Equal(3, Verifiedˉstore.Entries.Length);
        True(Verifiedˉstore.Tryˉlookup("boot:main.configuration", out var Configuration) &&
            Configuration is not null, "The process image store lacks its third resource.");
        Equal(3u, Configuration!.Identifier);
        True(Configuration.Kind == Resourceˉstoreˉkind.Opaqueˉbytes,
            "The third process-image resource has the wrong kind.");
        Sequenceˉequal([3, 5, 8, 13], Configuration.Data);
        True(Verifiedˉstore.Tryˉlookup("boot:main.wvb", out var Storedˉmodule) &&
            Storedˉmodule is not null, "The process image store lacks its admitted WVB.");
        Sequenceˉequal(First.Admittedˉprogramˉbytes, Storedˉmodule!.Data);
        Sequenceˉequal(
            Configurationˉrequest,
            Userˉobject.Sections.Single(Section =>
                Section.Kind == Objectˉsectionˉkind.Readˉonlyˉdata).Data);
        Sequenceˉequal(
            Configurationˉrequest,
            Faultˉobject.Sections.Single(Section =>
                Section.Kind == Objectˉsectionˉkind.Readˉonlyˉdata).Data);
        Equal(7, Countˉsequence(Serviceˉobject.Sections[0].Data, [0x0F, 0x05]));
        Equal(3, Countˉsequence(Userˉobject.Sections[0].Data, [0x0F, 0x05]));
        Equal(2, Countˉsequence(Faultˉobject.Sections[0].Data, [0x0F, 0x05]));
        Equal(1, Countˉsequence(Faultˉobject.Sections[0].Data, [0xFA]));
        Equal(1, Countˉsequence(Faultˉobject.Sections[0].Data, [0xCC]));
        Equal(1, Bootˉresourceˉstencil.Sections.Length);
        True(Bootˉresourceˉstencil.Sections[0].Kind == Objectˉsectionˉkind.Readˉonlyˉdata,
            "The WVA boot-resource stencil is not read-only data.");
        Equal((int)Kernelˉprocessˉcontract.BOOT_RESOURCE_SERVICE_BYTES,
            Bootˉresourceˉstencil.Sections[0].Data.Length);
        Equal("b43bc2457fd5b5622095bad6d59ad3cd2aa045bde1cc79576afbb419bac02fd7",
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
        True((ulong)First.Initˉserviceˉimageˉbytes.Length <=
                Kernelˉprocessˉcontract.INIT_CODE_PAGES * Kernelˉpagingˉcontract.PAGE_BYTES,
            "The init service image exceeds its RX extent.");
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
            Kernelˉprocessˉcontract.INIT_PROCESS_GENERATION,
            Kernelˉprocessˉcontract.ROLE_INIT_SERVICE,
            Kernelˉprocessˉcontract.INIT_CAPABILITY_RIGHTS,
            CHANNEL);
        var Clientˉdefinition = new Kernelˉprocessˉdefinition(
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID,
            Kernelˉprocessˉcontract.CLIENT_THREAD_ID,
            Kernelˉprocessˉcontract.FIRST_CLIENT_GENERATION,
            Kernelˉprocessˉcontract.ROLE_BYTECODE_INTERPRETER,
            Kernelˉprocessˉcontract.CLIENT_CAPABILITY_RIGHTS,
            CHANNEL);
        var First = Kernelˉprocessˉplanner.Plan(
            Paging, ALLOCATION, Image.Initˉserviceˉimageˉbytes.AsSpan(),
            Image.Initˉserviceˉdigest.AsSpan(), Image.Admittedˉprogramˉdigest.AsSpan(),
            Image.Admittedˉprogramˉbytes.AsSpan(), Image.Executionˉbudgetˉbytes.AsSpan(),
            Image.Resourceˉstoreˉbytes.AsSpan(), 0, Initˉdefinition);
        var Second = Kernelˉprocessˉplanner.Plan(
            Paging, ALLOCATION, Image.Initˉserviceˉimageˉbytes.AsSpan(),
            Image.Initˉserviceˉdigest.AsSpan(), Image.Admittedˉprogramˉdigest.AsSpan(),
            Image.Admittedˉprogramˉbytes.AsSpan(), Image.Executionˉbudgetˉbytes.AsSpan(),
            Image.Resourceˉstoreˉbytes.AsSpan(), 0, Initˉdefinition);
        var Client = Kernelˉprocessˉplanner.Plan(
            Paging, CLIENT_ALLOCATION, Image.Clientˉimageˉbytes.AsSpan(),
            Image.Interpreterˉdigest.AsSpan(), Image.Admittedˉprogramˉdigest.AsSpan(),
            [], [], [], Image.Bootˉresourceˉserviceˉoffset,
            Clientˉdefinition);
        True(First.Success, First.Diagnostics.IsEmpty ? "Process planning failed." : First.Diagnostics[0].Message);
        True(Second.Success, "Repeated process planning failed.");
        True(Client.Success, "Client process planning failed.");
        var Plan = First.Plan!;
        Sequenceˉequal(Plan.Tableˉbytes, Second.Plan!.Tableˉbytes);
        Sequenceˉequal(Plan.Processˉrecord, Second.Plan.Processˉrecord);
        Equal(ALLOCATION, Plan.Rootˉaddress);
        Equal(ALLOCATION + 0x4000UL, Plan.Userˉcodeˉaddress);
        Equal(ALLOCATION + 0x6000UL, Plan.Userˉstackˉaddress);
        Equal(ALLOCATION + 0x7000UL, Plan.Userˉdataˉaddress);
        Equal(ALLOCATION + 0x8000UL, Plan.Userˉruntimeˉinputˉaddress);
        Equal(ALLOCATION + 0x9000UL, Plan.Userˉruntimeˉbudgetˉaddress);
        Equal(ALLOCATION + 0xA000UL, Plan.Userˉresourceˉstoreˉaddress);
        Equal(CLIENT_ALLOCATION, Client.Plan!.Rootˉaddress);
        Equal(CLIENT_ALLOCATION + 0x71000UL, Client.Plan.Userˉstackˉaddress);
        Equal(CLIENT_ALLOCATION + 0x77000UL, Client.Plan.Userˉdataˉaddress);
        Equal(CLIENT_ALLOCATION + 0x78000UL, Client.Plan.Userˉruntimeˉinputˉaddress);
        Equal(CLIENT_ALLOCATION + 0x79000UL, Client.Plan.Userˉruntimeˉbudgetˉaddress);
        Equal(Kernelˉprocessˉcontract.CLIENT_CODE_PAGES, Client.Plan.Userˉcodeˉpages);
        Equal(Kernelˉprocessˉcontract.CLIENT_STACK_PAGES, Client.Plan.Userˉstackˉpages);
        True(Client.Plan.Rootˉaddress != Plan.Rootˉaddress, "The service and client share a root.");

        Equal(ALLOCATION + 0x1000UL | 7UL, Kernelˉprocessˉplanner.Readˉentry(Plan, 0, 0));
        Equal(ALLOCATION + 0x2000UL | 7UL, Kernelˉprocessˉplanner.Readˉentry(Plan, 1, 0));
        Equal(ALLOCATION + 0x3000UL | 7UL, Kernelˉprocessˉplanner.Readˉentry(Plan, 2, 4));
        Equal(Plan.Userˉcodeˉaddress | 5UL, Kernelˉprocessˉplanner.Readˉentry(Plan, 3, 4));
        Equal(
            Plan.Userˉstackˉaddress | 7UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            Kernelˉprocessˉplanner.Readˉentry(Plan, 3, 6));
        Equal(
            Plan.Userˉdataˉaddress | 7UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            Kernelˉprocessˉplanner.Readˉentry(Plan, 3, 7));
        Equal(
            Plan.Userˉruntimeˉinputˉaddress | 5UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            Kernelˉprocessˉplanner.Readˉentry(Plan, 3, 8));
        Equal(
            Plan.Userˉruntimeˉbudgetˉaddress | 5UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            Kernelˉprocessˉplanner.Readˉentry(Plan, 3, 9));
        Equal(
            Plan.Userˉresourceˉstoreˉaddress | 5UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            Kernelˉprocessˉplanner.Readˉentry(Plan, 3, 10));
        for (ulong Page = 0; Page < Kernelˉprocessˉcontract.CLIENT_STACK_PAGES; Page++)
        {
            Equal(
                Client.Plan.Userˉstackˉaddress + Page * Kernelˉpagingˉcontract.PAGE_BYTES |
                    7UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
                Kernelˉprocessˉplanner.Readˉentry(
                    Client.Plan,
                    3,
                    checked((int)(Kernelˉprocessˉcontract.CLIENT_STACK_PAGE + Page))));
        }
        Equal(
            Client.Plan.Userˉdataˉaddress | 7UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            Kernelˉprocessˉplanner.Readˉentry(
                Client.Plan, 3, (int)Kernelˉprocessˉcontract.CLIENT_DATA_PAGE));
        Equal(0UL, Kernelˉprocessˉplanner.Readˉentry(
            Client.Plan, 3, (int)Kernelˉprocessˉcontract.CLIENT_RUNTIME_INPUT_PAGE));
        Equal(0UL, Kernelˉprocessˉplanner.Readˉentry(
            Client.Plan, 3, (int)Kernelˉprocessˉcontract.CLIENT_RUNTIME_BUDGET_PAGE));

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
        Equal(7, Userˉleaves);
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
        Equal((int)Kernelˉprocessˉcontract.CLIENT_MEMORY_PAGE_BUDGET - 2,
            Clientˉuserˉleaves);
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
        Sequenceˉequal(Image.Executionˉbudgetˉbytes,
            Plan.Userˉruntimeˉbudgetˉbytes[..Image.Executionˉbudgetˉbytes.Length]);
        True(!Plan.Userˉruntimeˉbudgetˉbytes.AsSpan()[Image.Executionˉbudgetˉbytes.Length..]
                .ContainsAnyExcept((byte)0),
            "The unused runtime-budget page tail is not zeroed.");
        True(Client.Plan.Userˉruntimeˉbudgetˉbytes.IsEmpty,
            "The interpreter owns runtime-budget bytes before init grants them.");
        Sequenceˉequal(Image.Resourceˉstoreˉbytes,
            Plan.Userˉresourceˉstoreˉbytes[..Image.Resourceˉstoreˉbytes.Length]);
        True(!Plan.Userˉresourceˉstoreˉbytes.AsSpan()[Image.Resourceˉstoreˉbytes.Length..]
                .ContainsAnyExcept((byte)0),
            "The unused resource-store page tail is not zeroed.");
        True(Client.Plan.Userˉresourceˉstoreˉbytes.IsEmpty,
            "The interpreter owns the init resource store.");
        Equal(Plan.Userˉresourceˉstoreˉaddress,
            BinaryPrimitives.ReadUInt64LittleEndian(Plan.Userˉdataˉbytes.AsSpan()[
                (int)Kernelˉprocessˉcontract.INIT_STORE_DESCRIPTOR_OFFSET..]));
        Equal((uint)Image.Resourceˉstoreˉbytes.Length,
            BinaryPrimitives.ReadUInt32LittleEndian(Plan.Userˉdataˉbytes.AsSpan()[
                ((int)Kernelˉprocessˉcontract.INIT_STORE_DESCRIPTOR_OFFSET + sizeof(ulong))..]));
        True(
            Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_OFFSET >=
                Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET +
                Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_BYTES &&
            Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_OFFSET +
                Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_BYTES <=
                Kernelˉpagingˉcontract.PAGE_BYTES &&
            Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_USED_BYTES <=
                Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_BYTES,
            "The interpreter record arena overlaps context publication or exceeds its data page.");
        Equal(1_024U, Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_BYTES);
        Equal(0U, Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_USED_BYTES);
        Equal(24_240UL, Kernelˉprocessˉcontract.CLIENT_NATIVE_STACK_USED_BYTES);
        True(
            Kernelˉprocessˉcontract.CLIENT_NATIVE_STACK_USED_BYTES <=
                Kernelˉprocessˉcontract.CLIENT_STACK_BYTES &&
            Kernelˉprocessˉcontract.CLIENT_NATIVE_STACK_USED_BYTES >
                Kernelˉprocessˉcontract.CLIENT_STACK_BYTES - Kernelˉpagingˉcontract.PAGE_BYTES,
            "The interpreter stack is not the minimal whole-page envelope for its native call path.");
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
        Equal(0UL,
            BinaryPrimitives.ReadUInt64LittleEndian(
                Plan.Userˉdataˉbytes.AsSpan()[
                    Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_POINTER_OFFSET..]));
        Equal(0U,
            BinaryPrimitives.ReadUInt32LittleEndian(
                Plan.Userˉdataˉbytes.AsSpan()[
                    Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET..]));
        Equal(0U,
            BinaryPrimitives.ReadUInt32LittleEndian(
                Plan.Userˉdataˉbytes.AsSpan()[
                    Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET..]));
        Equal(Client.Plan.Userˉdataˉaddress + Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_OFFSET,
            BinaryPrimitives.ReadUInt64LittleEndian(
                Client.Plan.Userˉdataˉbytes.AsSpan()[
                    Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_POINTER_OFFSET..]));
        Equal(Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_BYTES,
            BinaryPrimitives.ReadUInt32LittleEndian(
                Client.Plan.Userˉdataˉbytes.AsSpan()[
                    Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET..]));
        Equal(0U,
            BinaryPrimitives.ReadUInt32LittleEndian(
                Client.Plan.Userˉdataˉbytes.AsSpan()[
                    Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET..]));

        var Grant = Kernelˉresourceˉgrantˉplanner.Plan(
            Plan, Client.Plan, Image.Admittedˉprogramˉdigest.AsSpan(),
            Image.Executionˉbudgetˉdigest.AsSpan(), Image.Admittedˉprogramˉbytes.Length,
            Kernelˉprocessˉcontract.RESOURCE_SET_TOKEN, Image.Bootˉresourceˉserviceˉoffset);
        var Repeatedˉgrant = Kernelˉresourceˉgrantˉplanner.Plan(
            Plan, Client.Plan, Image.Admittedˉprogramˉdigest.AsSpan(),
            Image.Executionˉbudgetˉdigest.AsSpan(), Image.Admittedˉprogramˉbytes.Length,
            Kernelˉprocessˉcontract.RESOURCE_SET_TOKEN, Image.Bootˉresourceˉserviceˉoffset);
        True(Grant.Success, Grant.Diagnostics.IsEmpty
            ? "Resource grant planning failed."
            : Grant.Diagnostics[0].Message);
        True(Repeatedˉgrant.Success, "Repeated resource grant planning failed.");
        Sequenceˉequal(Grant.Plan!.Resourceˉrecords, Repeatedˉgrant.Plan!.Resourceˉrecords);
        Sequenceˉequal(Grant.Plan.Clientˉtableˉbytes, Repeatedˉgrant.Plan.Clientˉtableˉbytes);
        Sequenceˉequal(Grant.Plan.Clientˉdataˉbytes, Repeatedˉgrant.Plan.Clientˉdataˉbytes);
        Equal(
            Plan.Userˉruntimeˉinputˉaddress | 5UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            BinaryPrimitives.ReadUInt64LittleEndian(Grant.Plan.Clientˉtableˉbytes.AsSpan()[
                checked((int)(Kernelˉprocessˉcontract.USER_PT_PAGE *
                    Kernelˉpagingˉcontract.PAGE_BYTES +
                    Kernelˉprocessˉcontract.CLIENT_RUNTIME_INPUT_PAGE * sizeof(ulong)))..]));
        Equal(
            Plan.Userˉruntimeˉbudgetˉaddress | 5UL | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE,
            BinaryPrimitives.ReadUInt64LittleEndian(Grant.Plan.Clientˉtableˉbytes.AsSpan()[
                checked((int)(Kernelˉprocessˉcontract.USER_PT_PAGE *
                    Kernelˉpagingˉcontract.PAGE_BYTES +
                    Kernelˉprocessˉcontract.CLIENT_RUNTIME_BUDGET_PAGE * sizeof(ulong)))..]));
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
        Equal(Client.Plan.Userˉdataˉaddress + Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_OFFSET,
            BinaryPrimitives.ReadUInt64LittleEndian(
                Grant.Plan.Clientˉdataˉbytes.AsSpan()[
                    Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_POINTER_OFFSET..]));
        Equal(Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_BYTES,
            BinaryPrimitives.ReadUInt32LittleEndian(
                Grant.Plan.Clientˉdataˉbytes.AsSpan()[
                    Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET..]));
        Equal(0U,
            BinaryPrimitives.ReadUInt32LittleEndian(
                Grant.Plan.Clientˉdataˉbytes.AsSpan()[
                    Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET..]));
        var Resourceˉtable = Grant.Plan.Clientˉdataˉbytes.AsSpan()[
            (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET..];
        Equal(Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_MAGIC,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉtable));
        Equal(Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_VERSION,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉtable[4..]));
        Equal(Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_BYTES,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉtable[8..]));
        Equal(Kernelˉprocessˉcontract.RESOURCE_COUNT,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉtable[
                (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_COUNT_OFFSET..]));
        var Moduleˉentry = Resourceˉtable[
            (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_FIRST_ENTRY_OFFSET..];
        Equal(Kernelˉprocessˉcontract.MODULE_RESOURCE_ID,
            BinaryPrimitives.ReadUInt32LittleEndian(Moduleˉentry));
        Equal(Kernelˉprocessˉcontract.RESOURCE_KIND_WVB_MODULE,
            BinaryPrimitives.ReadUInt32LittleEndian(Moduleˉentry[4..]));
        Equal(Client.Plan.Userˉruntimeˉinputˉaddress,
            BinaryPrimitives.ReadUInt64LittleEndian(Moduleˉentry[
                (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_DATA_POINTER_OFFSET..]));
        Equal((uint)Image.Admittedˉprogramˉbytes.Length,
            BinaryPrimitives.ReadUInt32LittleEndian(Moduleˉentry[
                (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_DATA_LENGTH_OFFSET..]));
        Equal(Kernelˉprocessˉcontract.RESOURCE_BASE_FLAGS,
            BinaryPrimitives.ReadUInt32LittleEndian(Moduleˉentry[
                (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_ENTRY_FLAGS_OFFSET..]));
        Equal(0U, BinaryPrimitives.ReadUInt32LittleEndian(
            Moduleˉentry[(int)Kernelˉprocessˉcontract.BOOT_RESOURCE_RESERVED_OFFSET..]));
        var Budgetˉentry = Resourceˉtable[
            (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_SECOND_ENTRY_OFFSET..];
        Equal(Kernelˉprocessˉcontract.BUDGET_RESOURCE_ID,
            BinaryPrimitives.ReadUInt32LittleEndian(Budgetˉentry));
        Equal(Kernelˉprocessˉcontract.RESOURCE_KIND_U32_EXECUTION_BUDGET,
            BinaryPrimitives.ReadUInt32LittleEndian(Budgetˉentry[4..]));
        Equal(Client.Plan.Userˉruntimeˉbudgetˉaddress,
            BinaryPrimitives.ReadUInt64LittleEndian(Budgetˉentry[
                (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_DATA_POINTER_OFFSET..]));
        Equal(Kernelˉprocessˉcontract.EXECUTION_BUDGET_BYTES,
            BinaryPrimitives.ReadUInt32LittleEndian(Budgetˉentry[
                (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_DATA_LENGTH_OFFSET..]));
        Equal(Kernelˉprocessˉcontract.RESOURCE_BASE_FLAGS,
            BinaryPrimitives.ReadUInt32LittleEndian(Budgetˉentry[
                (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_ENTRY_FLAGS_OFFSET..]));
        Equal(0UL, BinaryPrimitives.ReadUInt64LittleEndian(Budgetˉentry[
            (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_RESERVED_OFFSET..]));

        Equal((int)Kernelˉprocessˉcontract.RESOURCE_RECORD_SET_BYTES,
            Grant.Plan.Resourceˉrecords.Length);
        var Resourceˉrecord = Grant.Plan.Resourceˉrecords.AsSpan()[
            ..(int)Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES];
        Equal(Kernelˉprocessˉcontract.RESOURCE_MAGIC,
            BinaryPrimitives.ReadUInt64LittleEndian(Resourceˉrecord));
        Equal(Kernelˉprocessˉcontract.RESOURCE_VERSION,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉrecord[8..]));
        Equal(Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉrecord[12..]));
        Equal(Kernelˉprocessˉcontract.RESOURCE_STATE_BORROWED,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET..]));
        Equal(Kernelˉprocessˉcontract.MODULE_RESOURCE_ID,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_ID_OFFSET..]));
        Equal(Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE,
            BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_OWNER_OFFSET..]));
        Equal(Kernelˉprocessˉcontract.FIRST_CLIENT_PROCESS_REFERENCE,
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
        Equal(Kernelˉprocessˉcontract.MODULE_RESOURCE_ATTRIBUTES,
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
            Grant.Plan.Resourceˉrecords[
                (int)Kernelˉprocessˉcontract.RESOURCE_DIGEST_OFFSET..
                (int)Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET]);
        Equal(1U, BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉrecord[
            (int)Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET..]));
        Equal(1U, BinaryPrimitives.ReadUInt32LittleEndian(Resourceˉrecord[
            (int)Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET..]));
        Equal(Client.Plan.Rootˉaddress + Kernelˉprocessˉcontract.USER_PT_PAGE *
                Kernelˉpagingˉcontract.PAGE_BYTES +
                Kernelˉprocessˉcontract.CLIENT_RUNTIME_INPUT_PAGE * sizeof(ulong),
            BinaryPrimitives.ReadUInt64LittleEndian(Resourceˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_TARGET_PTE_OFFSET..]));
        var Budgetˉrecord = Grant.Plan.Resourceˉrecords.AsSpan()[
            (int)Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES..];
        Equal(Kernelˉprocessˉcontract.RESOURCE_MAGIC,
            BinaryPrimitives.ReadUInt64LittleEndian(Budgetˉrecord));
        Equal(Kernelˉprocessˉcontract.BUDGET_RESOURCE_ID,
            BinaryPrimitives.ReadUInt32LittleEndian(Budgetˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_ID_OFFSET..]));
        Equal(Kernelˉprocessˉcontract.BUDGET_RESOURCE_ATTRIBUTES,
            BinaryPrimitives.ReadUInt32LittleEndian(Budgetˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_FLAGS_OFFSET..]));
        Equal(Plan.Userˉruntimeˉbudgetˉaddress,
            BinaryPrimitives.ReadUInt64LittleEndian(Budgetˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_SOURCE_ADDRESS_OFFSET..]));
        Equal(Client.Plan.Userˉruntimeˉbudgetˉaddress,
            BinaryPrimitives.ReadUInt64LittleEndian(Budgetˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_TARGET_ADDRESS_OFFSET..]));
        Equal(Kernelˉprocessˉcontract.EXECUTION_BUDGET_BYTES,
            BinaryPrimitives.ReadUInt32LittleEndian(Budgetˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET..]));
        Sequenceˉequal(Image.Executionˉbudgetˉdigest,
            Grant.Plan.Resourceˉrecords[
                (int)(Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES +
                    Kernelˉprocessˉcontract.RESOURCE_DIGEST_OFFSET)..
                (int)(Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES +
                    Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET)]);
        Equal(Client.Plan.Rootˉaddress + Kernelˉprocessˉcontract.USER_PT_PAGE *
                Kernelˉpagingˉcontract.PAGE_BYTES +
                Kernelˉprocessˉcontract.CLIENT_RUNTIME_BUDGET_PAGE * sizeof(ulong),
            BinaryPrimitives.ReadUInt64LittleEndian(Budgetˉrecord[
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
            Kernelˉpagingˉcontract.PAGE_BYTES +
            Kernelˉprocessˉcontract.CLIENT_RUNTIME_INPUT_PAGE * sizeof(ulong)));
        var Budgetˉtargetˉpteˉoffset = checked((int)(Kernelˉprocessˉcontract.USER_PT_PAGE *
            Kernelˉpagingˉcontract.PAGE_BYTES +
            Kernelˉprocessˉcontract.CLIENT_RUNTIME_BUDGET_PAGE * sizeof(ulong)));
        var Accessedˉtables = Grant.Plan.Clientˉtableˉbytes.ToArray();
        BinaryPrimitives.WriteUInt64LittleEndian(
            Accessedˉtables.AsSpan(Targetˉpteˉoffset),
            BinaryPrimitives.ReadUInt64LittleEndian(Accessedˉtables.AsSpan(Targetˉpteˉoffset)) |
                Kernelˉpagingˉcontract.ENTRY_ACCESSED);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Accessedˉtables.AsSpan(Budgetˉtargetˉpteˉoffset),
            BinaryPrimitives.ReadUInt64LittleEndian(
                Accessedˉtables.AsSpan(Budgetˉtargetˉpteˉoffset)) |
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
        Sequenceˉequal(Revoked.Plan!.Resourceˉrecords, Repeatedˉrevocation.Plan!.Resourceˉrecords);
        Sequenceˉequal(Revoked.Plan.Clientˉtableˉbytes, Repeatedˉrevocation.Plan.Clientˉtableˉbytes);
        Sequenceˉequal(Revoked.Plan.Clientˉdataˉbytes, Repeatedˉrevocation.Plan.Clientˉdataˉbytes);
        Sequenceˉequal(Revoked.Plan.Resourceˉrecords, Faultˉrevocation.Plan!.Resourceˉrecords);
        Sequenceˉequal(Revoked.Plan.Clientˉtableˉbytes, Faultˉrevocation.Plan.Clientˉtableˉbytes);
        Sequenceˉequal(Revoked.Plan.Clientˉdataˉbytes, Faultˉrevocation.Plan.Clientˉdataˉbytes);
        Sequenceˉequal(Revoked.Plan.Resourceˉrecords, Accessedˉrevocation.Plan!.Resourceˉrecords);
        Sequenceˉequal(Revoked.Plan.Clientˉtableˉbytes, Accessedˉrevocation.Plan.Clientˉtableˉbytes);
        Sequenceˉequal(Revoked.Plan.Clientˉdataˉbytes, Accessedˉrevocation.Plan.Clientˉdataˉbytes);
        foreach (var Recordˉoffset in new[]
        {
            0,
            (int)Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES,
        })
        {
            var Revokedˉrecord = Revoked.Plan.Resourceˉrecords.AsSpan()[Recordˉoffset..];
            Equal(Kernelˉprocessˉcontract.RESOURCE_STATE_OWNED,
                BinaryPrimitives.ReadUInt32LittleEndian(Revokedˉrecord[
                    (int)Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET..]));
            Equal(0U, BinaryPrimitives.ReadUInt32LittleEndian(Revokedˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET..]));
            Equal(1U, BinaryPrimitives.ReadUInt32LittleEndian(Revokedˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET..]));
            Equal(0U, BinaryPrimitives.ReadUInt32LittleEndian(Revokedˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET..]));
        }
        Sequenceˉequal(Image.Admittedˉprogramˉdigest,
            Revoked.Plan.Resourceˉrecords[
                (int)Kernelˉprocessˉcontract.RESOURCE_DIGEST_OFFSET..
                (int)Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET]);
        Equal(0UL, BinaryPrimitives.ReadUInt64LittleEndian(
            Revoked.Plan.Clientˉtableˉbytes.AsSpan()[
                Targetˉpteˉoffset..]));
        Equal(0UL, BinaryPrimitives.ReadUInt64LittleEndian(
            Revoked.Plan.Clientˉtableˉbytes.AsSpan()[
                Budgetˉtargetˉpteˉoffset..]));
        Equal(0UL, BinaryPrimitives.ReadUInt64LittleEndian(
            Revoked.Plan.Clientˉdataˉbytes.AsSpan()[
                Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET..]));
        Equal(0UL, BinaryPrimitives.ReadUInt64LittleEndian(
            Revoked.Plan.Clientˉdataˉbytes.AsSpan()[
                Nativeˉexecutionˉcontextˉcontract.FILE_INPUT_TABLE_POINTER_OFFSET..]));
        Equal(Client.Plan.Userˉdataˉaddress + Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_OFFSET,
            BinaryPrimitives.ReadUInt64LittleEndian(
                Revoked.Plan.Clientˉdataˉbytes.AsSpan()[
                    Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_POINTER_OFFSET..]));
        Equal(Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_BYTES,
            BinaryPrimitives.ReadUInt32LittleEndian(
                Revoked.Plan.Clientˉdataˉbytes.AsSpan()[
                    Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET..]));
        Equal(0U,
            BinaryPrimitives.ReadUInt32LittleEndian(
                Revoked.Plan.Clientˉdataˉbytes.AsSpan()[
                    Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET..]));
        True(!Revoked.Plan.Clientˉdataˉbytes.AsSpan(
                (int)Kernelˉprocessˉcontract.RUNTIME_SERVICE_TABLE_OFFSET,
                (int)Nativeˉserviceˉtableˉcontract.SIZE).ContainsAnyExcept((byte)0),
            "The revoked client service table was not cleared.");
        True(!Revoked.Plan.Clientˉdataˉbytes.AsSpan(
                (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET,
                (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_BYTES).ContainsAnyExcept((byte)0),
            "The revoked client resource table was not cleared.");

        var Replacement = Kernelˉprocessˉplanner.Plan(
            Paging, CLIENT_ALLOCATION, Image.Clientˉimageˉbytes.AsSpan(),
            Image.Interpreterˉdigest.AsSpan(), Image.Admittedˉprogramˉdigest.AsSpan(),
            [], [], [], Image.Bootˉresourceˉserviceˉoffset,
            Clientˉdefinition with
            {
                Processˉgeneration = Kernelˉprocessˉcontract.SECOND_CLIENT_GENERATION,
            });
        True(Replacement.Success, Replacement.Diagnostics.IsEmpty
            ? "Generation-two process planning failed."
            : Replacement.Diagnostics[0].Message);
        Equal(Client.Plan.Rootˉaddress, Replacement.Plan!.Rootˉaddress);
        Sequenceˉequal(Client.Plan.Userˉcodeˉbytes, Replacement.Plan.Userˉcodeˉbytes);
        Equal(Kernelˉprocessˉcontract.SECOND_CLIENT_GENERATION,
            BinaryPrimitives.ReadUInt32LittleEndian(Replacement.Plan.Processˉrecord.AsSpan()[
                (int)Kernelˉprocessˉcontract.PROCESS_GENERATION_OFFSET..]));
        True(!Client.Plan.Processˉrecord.AsSpan().SequenceEqual(
                Replacement.Plan.Processˉrecord.AsSpan()),
            "The replacement process retained the stale generation-one identity.");

        var Regrant = Kernelˉresourceˉgrantˉplanner.Plan(
            Plan, Replacement.Plan,
            Image.Admittedˉprogramˉdigest.AsSpan(),
            Image.Executionˉbudgetˉdigest.AsSpan(),
            Image.Admittedˉprogramˉbytes.Length,
            Kernelˉprocessˉcontract.RESOURCE_SET_TOKEN,
            Image.Bootˉresourceˉserviceˉoffset,
            Revoked.Plan.Resourceˉrecords.AsSpan());
        True(Regrant.Success, Regrant.Diagnostics.IsEmpty
            ? "Generation-two resource grant failed."
            : Regrant.Diagnostics[0].Message);
        foreach (var Recordˉoffset in new[]
        {
            0,
            (int)Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES,
        })
        {
            var Regrantedˉrecord = Regrant.Plan!.Resourceˉrecords.AsSpan()[Recordˉoffset..];
            Equal(Kernelˉprocessˉcontract.SECOND_CLIENT_PROCESS_REFERENCE,
                BinaryPrimitives.ReadUInt32LittleEndian(Regrantedˉrecord[
                    (int)Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET..]));
            Equal(2U, BinaryPrimitives.ReadUInt32LittleEndian(Regrantedˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET..]));
        }
        var Replacementˉrevoked = Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Replacement.Plan, Regrant.Plan!,
            Kernelˉprocessˉcontract.PROCESS_STATE_EXITED,
            Kernelˉprocessˉcontract.THREAD_STATE_EXITED);
        True(Replacementˉrevoked.Success, Replacementˉrevoked.Diagnostics.IsEmpty
            ? "Generation-two resource cleanup failed."
            : Replacementˉrevoked.Diagnostics[0].Message);
        Equal(2U, BinaryPrimitives.ReadUInt32LittleEndian(
            Replacementˉrevoked.Plan!.Resourceˉrecords.AsSpan()[
                (int)Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET..]));
        Equal("WVOS6105", Kernelˉresourceˉgrantˉplanner.Plan(
            Plan, Client.Plan,
            Image.Admittedˉprogramˉdigest.AsSpan(),
            Image.Executionˉbudgetˉdigest.AsSpan(),
            Image.Admittedˉprogramˉbytes.Length,
            Kernelˉprocessˉcontract.RESOURCE_SET_TOKEN,
            Image.Bootˉresourceˉserviceˉoffset,
            Revoked.Plan.Resourceˉrecords.AsSpan()).Diagnostics[0].Code);
        Equal("WVOS6202", Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Replacement.Plan,
            Regrant.Plan! with { Resourceˉrecords = Grant.Plan.Resourceˉrecords },
            Kernelˉprocessˉcontract.PROCESS_STATE_EXITED,
            Kernelˉprocessˉcontract.THREAD_STATE_EXITED).Diagnostics[0].Code);

        Equal("WVOS6201", Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Client.Plan, Grant.Plan,
            Kernelˉprocessˉcontract.PROCESS_STATE_RUNNING,
            Kernelˉprocessˉcontract.THREAD_STATE_RUNNING).Diagnostics[0].Code);
        var Changedˉgrantˉrecord = Grant.Plan with
        {
            Resourceˉrecords = Mutate(Grant.Plan.Resourceˉrecords, 0).ToImmutableArray(),
        };
        Equal("WVOS6202", Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Client.Plan, Changedˉgrantˉrecord,
            Kernelˉprocessˉcontract.PROCESS_STATE_EXITED,
            Kernelˉprocessˉcontract.THREAD_STATE_EXITED).Diagnostics[0].Code);
        var Changedˉbudgetˉkind = Grant.Plan.Resourceˉrecords.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(
            Changedˉbudgetˉkind.AsSpan(checked((int)(
                Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES +
                Kernelˉprocessˉcontract.RESOURCE_FLAGS_OFFSET))),
            Kernelˉprocessˉcontract.MODULE_RESOURCE_ATTRIBUTES);
        Equal("WVOS6202", Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Client.Plan,
            Grant.Plan with { Resourceˉrecords = Changedˉbudgetˉkind.ToImmutableArray() },
            Kernelˉprocessˉcontract.PROCESS_STATE_EXITED,
            Kernelˉprocessˉcontract.THREAD_STATE_EXITED).Diagnostics[0].Code);
        var Changedˉbudgetˉlength = Grant.Plan.Resourceˉrecords.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(
            Changedˉbudgetˉlength.AsSpan(checked((int)(
                Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES +
                Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET))),
            Kernelˉprocessˉcontract.EXECUTION_BUDGET_BYTES - 1);
        Equal("WVOS6202", Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Client.Plan,
            Grant.Plan with { Resourceˉrecords = Changedˉbudgetˉlength.ToImmutableArray() },
            Kernelˉprocessˉcontract.PROCESS_STATE_EXITED,
            Kernelˉprocessˉcontract.THREAD_STATE_EXITED).Diagnostics[0].Code);
        var Changedˉbudgetˉdigest = Mutate(
            Grant.Plan.Resourceˉrecords,
            checked((int)(Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES +
                Kernelˉprocessˉcontract.RESOURCE_DIGEST_OFFSET))).ToImmutableArray();
        Equal("WVOS6203", Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Client.Plan,
            Grant.Plan with { Resourceˉrecords = Changedˉbudgetˉdigest },
            Kernelˉprocessˉcontract.PROCESS_STATE_EXITED,
            Kernelˉprocessˉcontract.THREAD_STATE_EXITED).Diagnostics[0].Code);
        var Outsideˉpteˉrecord = Grant.Plan.Resourceˉrecords.ToArray();
        BinaryPrimitives.WriteUInt64LittleEndian(
            Outsideˉpteˉrecord.AsSpan((int)Kernelˉprocessˉcontract.RESOURCE_TARGET_PTE_OFFSET),
            Client.Plan.Rootˉaddress - sizeof(ulong));
        Equal("WVOS6203", Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Client.Plan,
            Grant.Plan with { Resourceˉrecords = Outsideˉpteˉrecord.ToImmutableArray() },
            Kernelˉprocessˉcontract.PROCESS_STATE_EXITED,
            Kernelˉprocessˉcontract.THREAD_STATE_EXITED).Diagnostics[0].Code);
        var Changedˉgrantˉtables = Grant.Plan with
        {
            Clientˉtableˉbytes = Mutate(Grant.Plan.Clientˉtableˉbytes,
                checked((int)(Kernelˉprocessˉcontract.USER_PT_PAGE *
                    Kernelˉpagingˉcontract.PAGE_BYTES +
                    Kernelˉprocessˉcontract.CLIENT_RUNTIME_INPUT_PAGE * sizeof(ulong)))).ToImmutableArray(),
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
        var Changedˉbudgetˉentry = Grant.Plan.Clientˉdataˉbytes.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(
            Changedˉbudgetˉentry.AsSpan(checked((int)(
                Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET +
                Kernelˉprocessˉcontract.BOOT_RESOURCE_SECOND_ENTRY_OFFSET +
                Kernelˉprocessˉcontract.BOOT_RESOURCE_ENTRY_KIND_OFFSET))),
            Kernelˉprocessˉcontract.RESOURCE_KIND_WVB_MODULE);
        Equal("WVOS6203", Kernelˉresourceˉrevocationˉplanner.Plan(
            Plan, Client.Plan,
            Grant.Plan with { Clientˉdataˉbytes = Changedˉbudgetˉentry.ToImmutableArray() },
            Kernelˉprocessˉcontract.PROCESS_STATE_EXITED,
            Kernelˉprocessˉcontract.THREAD_STATE_EXITED).Diagnostics[0].Code);
        var Replayˉrelease = new Kernelˉresourceˉgrantˉplan(
            Revoked.Plan.Resourceˉrecords,
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
        Equal(Kernelˉprocessˉcontract.CLIENT_CAPABILITY_RIGHTS,
            BinaryPrimitives.ReadUInt32LittleEndian(Client.Plan.Processˉrecord.AsSpan()[120..]));
        Equal(3U, Kernelˉprocessˉcontract.CHANNEL_VERSION);
        Equal(112U, Kernelˉprocessˉcontract.CHANNEL_RECORD_BYTES);
        Equal(17U, Kernelˉprocessˉcontract.CLIENT_CAPABILITY_RIGHTS);
        Equal(46U, Kernelˉprocessˉcontract.INIT_CAPABILITY_RIGHTS);
        Equal(
            Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET +
                Kernelˉprocessˉcontract.CHANNEL_RECORD_BYTES,
            Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET);
        Equal(CHANNEL, BinaryPrimitives.ReadUInt64LittleEndian(
            Plan.Processˉrecord.AsSpan()[(int)Kernelˉprocessˉcontract.CHANNEL_ADDRESS_OFFSET..]));
        var Channelˉbytes = Kernelˉchannelˉpeerˉlifecycle.Create().ToArray();
        foreach (var Offset in new[]
        {
            Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_MESSAGE_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_SENDER_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_RECEIVER_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_BYTE_LENGTH_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_SERVICE_CAPACITY_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_CLIENT_CAPACITY_OFFSET,
        })
        {
            BinaryPrimitives.WriteUInt32LittleEndian(Channelˉbytes.AsSpan((int)Offset), 0xA5A5_A5A5);
        }
        BinaryPrimitives.WriteUInt64LittleEndian(Channelˉbytes.AsSpan(
            (int)Kernelˉprocessˉcontract.CHANNEL_SERVICE_DESTINATION_OFFSET), 0x0102_0304_0506_0708);
        BinaryPrimitives.WriteUInt64LittleEndian(Channelˉbytes.AsSpan(
            (int)Kernelˉprocessˉcontract.CHANNEL_CLIENT_DESTINATION_OFFSET), 0x1112_1314_1516_1718);
        var Channel = Channelˉbytes.ToImmutableArray();
        var Clientˉterminated = Kernelˉchannelˉpeerˉlifecycle.Terminateˉpeer(
            Channel,
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID,
            Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_FAULTED);
        Equal(Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_FAULTED,
            BinaryPrimitives.ReadUInt32LittleEndian(Clientˉterminated.AsSpan()[
                (int)Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_OFFSET..]));
        Equal(1U, BinaryPrimitives.ReadUInt32LittleEndian(Clientˉterminated.AsSpan()[
            (int)Kernelˉprocessˉcontract.CHANNEL_CLOSE_COUNT_OFFSET..]));
        foreach (var Offset in new[]
        {
            Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_MESSAGE_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_SENDER_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_RECEIVER_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_BYTE_LENGTH_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_SERVICE_CAPACITY_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_CLIENT_CAPACITY_OFFSET,
        })
        {
            Equal(0U, BinaryPrimitives.ReadUInt32LittleEndian(
                Clientˉterminated.AsSpan()[(int)Offset..]));
        }
        Equal(0UL, BinaryPrimitives.ReadUInt64LittleEndian(Clientˉterminated.AsSpan()[
            (int)Kernelˉprocessˉcontract.CHANNEL_SERVICE_DESTINATION_OFFSET..]));
        Equal(0UL, BinaryPrimitives.ReadUInt64LittleEndian(Clientˉterminated.AsSpan()[
            (int)Kernelˉprocessˉcontract.CHANNEL_CLIENT_DESTINATION_OFFSET..]));
        var Reopened = Kernelˉchannelˉpeerˉlifecycle.Reopen(Clientˉterminated);
        Equal(Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_OPEN,
            BinaryPrimitives.ReadUInt32LittleEndian(Reopened.AsSpan()[
                (int)Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_OFFSET..]));
        var Serviceˉterminated = Kernelˉchannelˉpeerˉlifecycle.Terminateˉpeer(
            Reopened,
            Kernelˉprocessˉcontract.INIT_PROCESS_ID,
            Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_EXITED);
        Equal(Kernelˉprocessˉcontract.INIT_PROCESS_ID,
            BinaryPrimitives.ReadUInt32LittleEndian(Serviceˉterminated.AsSpan()[
                (int)Kernelˉprocessˉcontract.CHANNEL_PEER_PROCESS_OFFSET..]));
        Equal(2U, BinaryPrimitives.ReadUInt32LittleEndian(Serviceˉterminated.AsSpan()[
            (int)Kernelˉprocessˉcontract.CHANNEL_CLOSE_COUNT_OFFSET..]));

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
            Image.Executionˉbudgetˉdigest.AsSpan(), Image.Admittedˉprogramˉbytes.Length,
            Kernelˉprocessˉcontract.RESOURCE_SET_TOKEN, Image.Bootˉresourceˉserviceˉoffset)
            .Diagnostics[0].Code);
        var Changedˉbudgetˉowner = Plan with
        {
            Userˉruntimeˉbudgetˉbytes = Mutate(
                Plan.Userˉruntimeˉbudgetˉbytes, 0).ToImmutableArray(),
        };
        Equal("WVOS6101", Kernelˉresourceˉgrantˉplanner.Plan(
            Changedˉbudgetˉowner, Client.Plan, Image.Admittedˉprogramˉdigest.AsSpan(),
            Image.Executionˉbudgetˉdigest.AsSpan(), Image.Admittedˉprogramˉbytes.Length,
            Kernelˉprocessˉcontract.RESOURCE_SET_TOKEN, Image.Bootˉresourceˉserviceˉoffset)
            .Diagnostics[0].Code);
        Equal("WVOS6101", Kernelˉresourceˉgrantˉplanner.Plan(
            Plan, Client.Plan, Image.Admittedˉprogramˉdigest.AsSpan(),
            Mutate(Image.Executionˉbudgetˉdigest, 0), Image.Admittedˉprogramˉbytes.Length,
            Kernelˉprocessˉcontract.RESOURCE_SET_TOKEN, Image.Bootˉresourceˉserviceˉoffset)
            .Diagnostics[0].Code);
        foreach (var Invalidˉresourceˉset in new uint[]
        {
            0x0003_0001,
            0x0001_0001,
            0x0001_0002,
            0x0000_0001,
        })
        {
            Equal("WVOS6104", Kernelˉresourceˉgrantˉplanner.Plan(
                Plan, Client.Plan, Image.Admittedˉprogramˉdigest.AsSpan(),
                Image.Executionˉbudgetˉdigest.AsSpan(), Image.Admittedˉprogramˉbytes.Length,
                Invalidˉresourceˉset, Image.Bootˉresourceˉserviceˉoffset)
                .Diagnostics[0].Code);
        }
        var Premappedˉclient = Client.Plan with { Tableˉbytes = Grant.Plan.Clientˉtableˉbytes };
        Equal("WVOS6102", Kernelˉresourceˉgrantˉplanner.Plan(
            Plan, Premappedˉclient, Image.Admittedˉprogramˉdigest.AsSpan(),
            Image.Executionˉbudgetˉdigest.AsSpan(), Image.Admittedˉprogramˉbytes.Length,
            Kernelˉprocessˉcontract.RESOURCE_SET_TOKEN, Image.Bootˉresourceˉserviceˉoffset)
            .Diagnostics[0].Code);
        Equal("WVOS6103", Kernelˉresourceˉgrantˉplanner.Plan(
            Plan, Client.Plan, Image.Admittedˉprogramˉdigest.AsSpan(),
            Image.Executionˉbudgetˉdigest.AsSpan(), Image.Admittedˉprogramˉbytes.Length,
            Kernelˉprocessˉcontract.RESOURCE_SET_TOKEN,
            (uint)Client.Plan.Userˉcodeˉbytes.Length)
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
        Equal(480_666, First.Objectˉbytes.Length);
        Equal("7c445a204aa906b0411f1fbd15f7df5aea4feae2c36a76cf914e39f6b59645fe",
            Objectˉdigest.Calculateˉsha256(First.Objectˉbytes.AsSpan()));
        Equal(26_259, First.Codeˉbytes.Length);
        Equal("1919b75ffd87c62e7c7a57c9a6d75cd23813169f660b5fb977df15827f2dfd97",
            Objectˉdigest.Calculateˉsha256(First.Codeˉbytes.AsSpan()));
        Equal(480_714, Faultˉfirst.Objectˉbytes.Length);
        Equal("64577556b02f59e03a3645292402a40d57821313a618e39d60f8d8b92aa513a8",
            Objectˉdigest.Calculateˉsha256(Faultˉfirst.Objectˉbytes.AsSpan()));
        Equal(26_323, Faultˉfirst.Codeˉbytes.Length);
        Equal("54cb8b759fab1deba50c28f4a968efe7790f62201765880d9a4db488dde1d82e",
            Objectˉdigest.Calculateˉsha256(Faultˉfirst.Codeˉbytes.AsSpan()));
        Equal(24, First.Relocations.Length);
        Equal(24, Faultˉfirst.Relocations.Length);
        Equal(1, First.Relocations.Count(Relocation => Relocation.Symbolˉindex == 13));
        Equal(1, First.Relocations.Count(Relocation => Relocation.Symbolˉindex == 14));
        Equal(1, First.Relocations.Count(Relocation => Relocation.Symbolˉindex == 15));
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
        Processˉmachineˉrejects(Normalˉimage with
        {
            Executionˉbudgetˉbytes = Mutate(
                Normalˉimage.Executionˉbudgetˉbytes, 0).ToImmutableArray(),
        });
        Processˉmachineˉrejects(Normalˉimage with
        {
            Executionˉbudgetˉdigest = Mutate(
                Normalˉimage.Executionˉbudgetˉdigest, 0).ToImmutableArray(),
        });
        Processˉmachineˉrejects(Normalˉimage with
        {
            Resourceˉstoreˉbytes = Mutate(
                Normalˉimage.Resourceˉstoreˉbytes, 0).ToImmutableArray(),
        });
        Processˉmachineˉrejects(Normalˉimage with
        {
            Resourceˉstoreˉdigest = Mutate(
                Normalˉimage.Resourceˉstoreˉdigest, 0).ToImmutableArray(),
        });

        var Object = Objectˉcodec.Readˉandˉverify(First.Objectˉbytes.AsSpan()).Value;
        Equal(6, Object.Sections.Length);
        True(Object.Sections[0].Kind == Objectˉsectionˉkind.Code, "The process machine section is not code.");
        True(Object.Sections[1].Kind == Objectˉsectionˉkind.Readˉonlyˉdata,
            "The user image is not retained as read-only link data.");
        Sequenceˉequal(First.Codeˉbytes, Object.Sections[0].Data);
        Sequenceˉequal(Normalˉimage.Initˉserviceˉimageˉbytes, Object.Sections[1].Data);
        Sequenceˉequal(Normalˉimage.Clientˉimageˉbytes, Object.Sections[2].Data);
        Sequenceˉequal(Normalˉimage.Admittedˉprogramˉbytes, Object.Sections[3].Data);
        Sequenceˉequal(Normalˉimage.Executionˉbudgetˉbytes, Object.Sections[4].Data);
        Sequenceˉequal(Normalˉimage.Resourceˉstoreˉbytes, Object.Sections[5].Data);
        Equal(16, Object.Symbols.Length);
        Equal("Windvale_resource_init_boot", Object.Symbols[2].Name);
        Equal("Windvale_resource_init_budget", Object.Symbols[3].Name);
        Equal("Windvale_resource_init_store", Object.Symbols[4].Name);
        Equal(Kernelˉprocessˉcontract.ENTER_SYMBOL, Object.Symbols[5].Name);
        Equal(Kernelˉprocessˉcontract.EXCEPTION_ENTRY_SYMBOL, Object.Symbols[6].Name);
        Equal(Kernelˉprocessˉcontract.SYSCALL_ENTRY_SYMBOL, Object.Symbols[7].Name);
        Equal(Kernelˉmemoryˉcontract.ALLOCATE_PAGES_SYMBOL, Object.Symbols[8].Name);
        Equal(Kernelˉprocessˉcontract.POLICY_SYMBOL, Object.Symbols[9].Name);
        Equal(Kernelˉmemoryˉcontract.RELEASE_TAIL_PAGES_SYMBOL, Object.Symbols[10].Name);
        Equal(Kernelˉexceptionˉcontract.TERMINAL_SYMBOL, Object.Symbols[11].Name);
        Equal(Kernelˉpagingˉcontract.PAGE_TABLE_ACTIVATE_SYMBOL, Object.Symbols[12].Name);
        Equal(Kernelˉprocessˉcontract.EXCEPTION_13_ENTRY_SYMBOL, Object.Symbols[13].Name);
        Equal(Kernelˉprocessˉcontract.EXCEPTION_14_ENTRY_SYMBOL, Object.Symbols[14].Name);
        Equal(Kernelˉprocessˉcontract.EXCEPTION_6_ENTRY_SYMBOL, Object.Symbols[15].Name);
        True(Object.Symbols.Skip(8).All(Symbol => Symbol.Binding == Objectˉsymbolˉbinding.Import),
            "A protected-process machine dependency is not an explicit import.");
        True(Object.Relocations.All(Relocation =>
                Relocation.Kind == Objectˉrelocationˉkind.Relativeˉi32 &&
                Relocation.Sectionˉindex == 0 &&
                Relocation.Addend == -4),
            "The protected-process machine object contains a noncanonical relocation.");
        Equal(10, Countˉsequence(First.Codeˉbytes, [0x48, 0x0F, 0x07]));
        Equal(12, Countˉsequence(First.Codeˉbytes, [0x0F, 0x01, 0xF8]));
        Equal(7, Countˉsequence(Normalˉimage.Initˉserviceˉimageˉbytes, [0x0F, 0x05]));
        Equal(3, Countˉsequence(Normalˉimage.Clientˉimageˉbytes, [0x0F, 0x05]));
        Equal(2, Countˉsequence(Faultˉimage.Clientˉimageˉbytes, [0x0F, 0x05]));
    }

    private static void Userˉfaultˉfirmwareˉprobeˉbuildsˉreproducibly()
    {
        var First = Firmwareˉprobe.Buildˉapplication(Firmwareˉprobeˉscenario.Userˉfault);
        var Second = Firmwareˉprobe.Buildˉapplication(Firmwareˉprobeˉscenario.Userˉfault);
        Sequenceˉequal(First, Second);
        Equal(566_272, First.Length);
        Equal(
            "19267825e530f0a950033f7e61602efd259e01c8fab45b049ee441da740b804a",
            Objectˉdigest.Calculateˉsha256(First.AsSpan()));
        True(!First.AsSpan().SequenceEqual(Firmwareˉprobe.Buildˉapplication().AsSpan()),
            "The normal and deliberate user-fault images are identical.");
        Equal(
            "windvale-os-boot 34\nentry=pass\nsystem-table=pass\nmemory-map=pass\nboot-services=exited\nmemory-owned=pass\nallocator=pass\nkernel-stack=pass\npaging=owned\nwvb-admission=pass\nprocesses=isolated\nresource-grant=pass\ntyped-resources=pass\nresource-revoked=pass\nprocess-reuse=pass\nwvb-runtime=interpreted\ninit-service=pass\nipc=dynamic-resource-store\nHello from Windvale\ncpu-exceptions=armed\nnative-context=pass\nnative-wvb=pass\nwindvale-source=pass\nuser-fault=contained\nstatus=pass\nshutdown=poweroff\n",
            Firmwareˉprobe.USER_FAULT_SERIAL_MARKER);
    }


    private static void Firmwareˉprobeˉbuildsˉreproducibly()
    {
        var First = Firmwareˉprobe.Buildˉapplication();
        var Second = Firmwareˉprobe.Buildˉapplication();
        Sequenceˉequal(First, Second);
        Equal(565_760, First.Length);
        Equal(
            "603c193ffacb5272c918d5c931598889cf58d12a80ff9292095b354b3541302c",
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
        Equal(565_760, First.Length);
        Equal(
            "aa50d9b3836ca4444a434bd0b7d55d230054680149031660ed2ce9288609472d",
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
        Equal(565_760, First.Length);
        Equal(
            "169a8fffae25b2143a5971c1e1927ff49d3038597566ffe0173313ea0a78f43a",
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
            "windvale-os-boot 34\nentry=pass\nsystem-table=pass\nmemory-map=pass\nboot-services=exited\nmemory-owned=pass\nallocator=pass\nkernel-stack=pass\npaging=owned\nwvb-admission=pass\nprocesses=isolated\nresource-grant=pass\ntyped-resources=pass\nresource-revoked=pass\nprocess-reuse=pass\nwvb-runtime=interpreted\ninit-service=pass\nipc=dynamic-resource-store\nHello from Windvale\ncpu-exceptions=armed\nnative-context=pass\nnative-wvb=pass\nwindvale-source=pass\nstatus=pass\nshutdown=poweroff\n",
            Firmwareˉprobe.SERIAL_MARKER);
        var Application = Firmwareˉprobe.Buildˉapplication();
        var Code = Uefiˉapplicationˉverifier.Verify(Application.AsSpan()).Codeˉbytes;
        Equal(1, Countˉsequence(Code, [0x48, 0x81, 0xEC, 0x88, 0x00, 0x00, 0x00]));
        Equal(1, Countˉsequence(Code, [0x49, 0x42, 0x49, 0x20, 0x53, 0x59, 0x53, 0x54]));
        Equal(1, Countˉsequence(Code, [0x42, 0x4F, 0x4F, 0x54, 0x53, 0x45, 0x52, 0x56]));
        Equal(1, Countˉsequence(Code, [0x81, 0x79, 0x0C, 0xF0, 0x00, 0x00, 0x00]));
        Equal(1, Countˉsequence(Code, [0x48, 0x83, 0xB9, 0xE8, 0x00, 0x00, 0x00, 0x00]));
        Equal(4, Countˉsequence(Code, [0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80]));
        Equal(1, Countˉsequence(Code, [0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80]));
        Equal(1, Countˉsequence(Code, [0xC7, 0x44, 0x24, 0x70, 0x03, 0x00, 0x00, 0x00]));
        Equal(3, Countˉsequence(Code, [0xFF, 0x50, 0x38]));
        Equal(1, Countˉsequence(Code, [0xFF, 0x50, 0x40]));
        Equal(1, Countˉsequence(Code, [0xFF, 0x50, 0x48]));
        Equal(1, Countˉsequence(Code, [0xFF, 0x90, 0xE8, 0x00, 0x00, 0x00]));
        Equal(3, Countˉsequence(Code, [0x57, 0x56, 0x4B, 0x48, 0x41, 0x4E, 0x44, 0x31]));
        Equal(8, Countˉsequence(Code, [0x57, 0x56, 0x4B, 0x4D, 0x45, 0x4D, 0x31, 0x32]));
        Equal(1, Countˉsequence(Code, [0xC7, 0x44, 0x24, 0x2C, 0x30, 0x00, 0x00, 0x00]));
        Equal(3, Countˉsequence(Code, [0x48, 0x83, 0xEC, 0x28]));
        Equal(2, Countˉsequence(Code, [0x48, 0x83, 0xEC, 0x78]));
        Equal(1, Countˉsequence(Code, [0x49, 0x8D, 0xA6, 0x00, 0x50, 0x00, 0x00]));
        Equal(9, Countˉsequence(Code, [0xFC, 0xF3, 0x48, 0xAB]));
        Equal(1,
            Countˉsequence(Code, [0xBA, 0xFD, 0x03, 0x00, 0x00, 0xEC, 0xA8, 0x20, 0x0F, 0x84]));
        Equal(1,
            Countˉsequence(Code, [0xBA, 0xFD, 0x03, 0x00, 0x00, 0xEC, 0xF6, 0xC0, 0x20, 0x0F, 0x84]));
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
        Equal(1_894, First.Length);
        Equal(
            "845d45d6787ec819ca300ffc81a9ffe3e86c7b3998f3dd2a50a017a353d86193",
            Objectˉdigest.Calculateˉsha256(First.AsSpan()));

        var Object = Objectˉcodec.Readˉandˉverify(First.AsSpan()).Value;
        Equal(2, Object.Sections.Length);
        True(Object.Sections[0].Kind == Objectˉsectionˉkind.Code, "The WVA shim is not code.");
        Equal(333u, Object.Sections[0].Memoryˉsize);
        True(Object.Sections[1].Kind == Objectˉsectionˉkind.Readˉonlyˉdata,
            "The WVA terminal markers are not read-only data.");
        Equal(162u, Object.Sections[1].Memoryˉsize);
        Sequenceˉequal(
            Encoding.ASCII.GetBytes(
                Kernelˉexceptionˉcontract.INVALID_OPCODE_PANIC_MARKER +
                Kernelˉexceptionˉcontract.GENERAL_PROTECTION_PANIC_MARKER +
                Kernelˉexceptionˉcontract.MALFORMED_FRAME_PANIC_MARKER).ToImmutableArray(),
            Object.Sections[1].Data);
        Equal(18, Object.Symbols.Length);
        Equal("Windvale_kernel_x64_exception_serial_write", Object.Symbols[0].Name);
        True(Object.Symbols[0].Binding == Objectˉsymbolˉbinding.Local,
            "The WVA serial loop is not object-local.");
        Equal(48u, Object.Symbols[0].Size);
        Equal(Kernelˉexceptionˉcontract.TERMINAL_SYMBOL, Object.Symbols[8].Name);
        True(Object.Symbols[8].Binding == Objectˉsymbolˉbinding.Export,
            "The normalized terminal handler is not WVA-owned.");
        Equal(164u, Object.Symbols[8].Size);
        Equal(Kernelˉassemblyˉcontract.Q35_SHUTDOWN_SYMBOL, Object.Symbols[14].Name);
        Equal(Kernelˉprocessˉcontract.EXCEPTION_ENTRY_SYMBOL, Object.Symbols[15].Name);
        True(Object.Symbols[15].Binding == Objectˉsymbolˉbinding.Import,
            "The process exception handler is not imported by WVA.");
        Equal(14, Object.Relocations.Length);
        Equal(new Objectˉrelocation(Objectˉrelocationˉkind.Relativeˉi32, 0, 16, 8, -4),
            Object.Relocations[2]);
        Equal(new Objectˉrelocation(Objectˉrelocationˉkind.Relativeˉi32, 0, 31, 8, -4),
            Object.Relocations[3]);
        Equal(3, Object.Relocations.Count(Relocation => Relocation.Symbolˉindex == 0));
        Equal(1, Countˉsequence(Object.Sections[0].Data,
            [0xBA, 0xFD, 0x03, 0, 0, 0xEC, 0xF6, 0xC0, 0x20, 0x0F, 0x84]));
        Equal(1, Countˉsequence(Object.Sections[0].Data,
            [0x8A, 0x84, 0x26, 0, 0, 0, 0, 0xEE]));
        Equal(1, Countˉsequence(Object.Sections[0].Data,
            [0xBA, 0xF4, 0, 0, 0, 0xB8, 1, 0, 0, 0, 0x66, 0xEF, 0xFA, 0xF4]));
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
        Equal(7_226, First.Nativeˉobjectˉbytes.Length);
        Equal(
            "0596755161d4a6f02e97148b73030de8e02bb64103395523cc1458fca7e8351c",
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

        Equal(815, First.Embeddedˉmoduleˉbytes.Length);
        Equal(
            "9ccfed0509e84bfc63979c6dc13170c14762efbdaa448b4c5894325f31aa7761",
            Objectˉdigest.Calculateˉsha256(First.Embeddedˉmoduleˉbytes.AsSpan()));
        Equal(4_068, First.Admissionˉmoduleˉbytes.Length);
        Equal(
            "f8f92352abed3c042c6ca6e5cbfd65b650a87837dd252802014b3a787cdb75cf",
            Objectˉdigest.Calculateˉsha256(First.Admissionˉmoduleˉbytes.AsSpan()));
        Equal(6_241, First.Embeddedˉnativeˉobjectˉbytes.Length);
        Equal(
            "04a827a46f333f00cf754ba1d470c1363a08a8e9b0cabb319d766639539bb28d",
            Objectˉdigest.Calculateˉsha256(First.Embeddedˉnativeˉobjectˉbytes.AsSpan()));
        Equal(20_191, First.Admissionˉnativeˉobjectˉbytes.Length);
        Equal(
            "10e176ad34ba585d9cc6a595cff8d3ae0ca076fc4768748c12e530279e5c4a61",
            Objectˉdigest.Calculateˉsha256(First.Admissionˉnativeˉobjectˉbytes.AsSpan()));
        Equal(484, First.Bridgeˉobjectˉbytes.Length);
        Equal(
            "2b5b67bfe04ba87c473d7a9c9fbcc213e864a9bcf39bbd58d0c10f5314aad606",
            Objectˉdigest.Calculateˉsha256(First.Bridgeˉobjectˉbytes.AsSpan()));

        var Admission = Runˉportableˉmain(First.Admissionˉmoduleˉbytes);
        Equal(Kernelˉwvbˉadmissionˉcontract.ADMISSION_TOKEN, Admission.Exitˉcode);
        Equal(
            (long)Kernelˉwvbˉadmissionˉcontract.EXACT_INSTRUCTION_BUDGET,
            Admission.Executedˉinstructions);
        var Embedded = Runˉportableˉmain(First.Embeddedˉmoduleˉbytes);
        Equal(Kernelˉwvbˉadmissionˉcontract.EXPECTED_RESULT, Embedded.Exitˉcode);
        Equal(199L, Embedded.Executedˉinstructions);

        var Changedˉmagic = Mutate(First.Embeddedˉmoduleˉbytes, 0).ToImmutableArray();
        var Changedˉsection = Mutate(First.Embeddedˉmoduleˉbytes, 16).ToImmutableArray();
        var Changedˉcode = Mutate(First.Embeddedˉmoduleˉbytes, 395).ToImmutableArray();
        var Truncated = First.Embeddedˉmoduleˉbytes[..^1];
        Assertˉadmissionˉresult(Changedˉmagic, 0);
        Assertˉadmissionˉresult(Changedˉsection, 0);
        Assertˉadmissionˉresult(Changedˉcode, 0);
        Assertˉadmissionˉresult(Truncated, 0);
        Rejectˉwvb(Changedˉmagic);
        Rejectˉwvb(Changedˉsection);
        Rejectˉwvb(Truncated);
        Equal(9, Runˉportableˉmain(Changedˉcode).Exitˉcode);

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
        ImmutableArray<byte> runtimeˉinput,
        ImmutableArray<byte> runtimeˉbudget = default,
        bool includeˉbudget = true)
    {
        if (runtimeˉbudget.IsDefault)
        {
            runtimeˉbudget = [(byte)Kernelˉprocessˉcontract.EXECUTION_BUDGET, 0, 0, 0];
        }
        var Resources = new Hostedˉresourceˉcontext(
            [],
            TextWriter.Null,
            TextWriter.Null,
            new Bootˉresourceˉreader(runtimeˉinput, runtimeˉbudget, includeˉbudget));
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
        var Runtimeˉbudget = definition.Role == Kernelˉprocessˉcontract.ROLE_INIT_SERVICE
            ? ImmutableArray.Create<byte>((byte)Kernelˉprocessˉcontract.EXECUTION_BUDGET, 0, 0, 0)
            : [];
        var Resourceˉstore = definition.Role == Kernelˉprocessˉcontract.ROLE_INIT_SERVICE
            ? Resourceˉstoreˉcodec.Write(Buildˉresourceˉstoreˉentries())
            : [];
        var Result = Kernelˉprocessˉplanner.Plan(
            paging, allocationˉaddress, userˉimage.AsSpan(), moduleˉdigest.AsSpan(),
            programˉdigest.AsSpan(), runtimeˉinput.AsSpan(), Runtimeˉbudget.AsSpan(),
            Resourceˉstore.AsSpan(), bootˉresourceˉserviceˉoffset, definition);
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

    private static ImmutableArray<Resourceˉstoreˉentry> Buildˉresourceˉstoreˉentries()
    {
        var Program = Seedˉcompiler.Compile(
            "module Resourceˉstoreˉfixture profile portable; " +
                "export fn Main() -> i32 { return 6; }",
            "Resource-Store-Fixture.wv");
        True(Program.Success, "The resource-store WVB fixture did not compile.");
        return
        [
            new(1, Resourceˉstoreˉkind.Wvbˉmodule, "boot:main.wvb", Program.Moduleˉbytes),
            new(2, Resourceˉstoreˉkind.U32ˉexecutionˉbudget, "boot:main.budget", [199, 0, 0, 0]),
            new(3, Resourceˉstoreˉkind.Opaqueˉbytes, "boot:main.configuration", [3, 5, 8, 13]),
        ];
    }

    private static string Loadˉresourceˉstoreˉsource(string resourceˉname)
    {
        using var Stream = typeof(Resourceˉstoreˉcodec).Assembly.GetManifestResourceStream(resourceˉname) ??
            throw new InvalidOperationException($"Embedded resource-store source '{resourceˉname}' is missing.");
        using var Reader = new StreamReader(Stream, new UTF8Encoding(false, true), false);
        return Reader.ReadToEnd();
    }

    private static Runtimeˉresult Runˉresourceˉstoreˉservice(
        Verifiedˉmodule module,
        ImmutableArray<byte> store,
        bool authorize = true,
        bool includeˉstore = true)
    {
        var Resources = new Hostedˉresourceˉcontext(
            [],
            TextWriter.Null,
            TextWriter.Null,
            new Resourceˉstoreˉreader(store, includeˉstore));
        var Grants = authorize
            ? ImmutableHashSet.Create(StringComparer.Ordinal, Capabilityˉcatalog.FILE_READ_BYTES)
            : ImmutableHashSet.Create<string>(StringComparer.Ordinal);
        return new Referenceˉruntime(
            module,
            new Referenceˉcapabilityˉhost(Resources),
            new(Grants)).Runˉmain();
    }

    private static Runtimeˉbytesˉresult Runˉresourceˉserviceˉbridge(
        Verifiedˉmodule module,
        ImmutableArray<byte> store,
        ImmutableArray<byte> request,
        bool authorize = true,
        bool includeˉstore = true,
        bool includeˉrequest = true)
    {
        var Resources = new Hostedˉresourceˉcontext(
            [],
            TextWriter.Null,
            TextWriter.Null,
            new Resourceˉserviceˉreader(store, request, includeˉstore, includeˉrequest));
        var Grants = authorize
            ? ImmutableHashSet.Create(StringComparer.Ordinal, Capabilityˉcatalog.FILE_READ_BYTES)
            : ImmutableHashSet.Create<string>(StringComparer.Ordinal);
        return new Referenceˉruntime(
            module,
            new Referenceˉcapabilityˉhost(Resources),
            new(Grants)).Runˉmainˉbytes();
    }

    private static ImmutableArray<byte> Replaceˉu32(
        ImmutableArray<byte> source,
        int offset,
        uint value)
    {
        var Result = source.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(offset), value);
        return Result.ToImmutableArray();
    }

    private static ImmutableArray<byte> Replaceˉbyte(
        ImmutableArray<byte> source,
        int offset,
        byte value)
    {
        var Result = source.ToArray();
        Result[offset] = value;
        return Result.ToImmutableArray();
    }

    private static void Rejectˉresourceˉstore(ImmutableArray<byte> source, string expectedˉcode) =>
        Rejectˉresourceˉstore(source.AsSpan(), expectedˉcode);

    private static void Rejectˉresourceˉstore(byte[] source, string expectedˉcode) =>
        Rejectˉresourceˉstore(source.AsSpan(), expectedˉcode);

    private static void Rejectˉresourceˉstore(ReadOnlySpan<byte> source, string expectedˉcode)
    {
        try
        {
            _ = Resourceˉstoreˉverifier.Verify(source);
        }
        catch (Resourceˉstoreˉexception Exception)
        {
            Equal(expectedˉcode, Exception.Code);
            return;
        }
        throw new InvalidOperationException(
            $"A malformed resource store was accepted instead of producing {expectedˉcode}.");
    }

    private static void Throwsˉresourceˉstore(string expectedˉcode, Action action)
    {
        try
        {
            action();
        }
        catch (Resourceˉstoreˉexception Exception)
        {
            Equal(expectedˉcode, Exception.Code);
            return;
        }
        throw new InvalidOperationException(
            $"A resource-store operation succeeded instead of producing {expectedˉcode}.");
    }

    private static void Rejectˉresourceˉrequest(ImmutableArray<byte> source, string expectedˉcode) =>
        Rejectˉresourceˉrequest(source.AsSpan(), expectedˉcode);

    private static void Rejectˉresourceˉrequest(byte[] source, string expectedˉcode) =>
        Rejectˉresourceˉrequest(source.AsSpan(), expectedˉcode);

    private static void Rejectˉresourceˉrequest(ReadOnlySpan<byte> source, string expectedˉcode)
    {
        try
        {
            _ = Resourceˉserviceˉipcˉcodec.Verifyˉrequest(source);
        }
        catch (Resourceˉserviceˉipcˉexception Exception)
        {
            Equal(expectedˉcode, Exception.Code);
            return;
        }
        throw new InvalidOperationException(
            $"A malformed resource request was accepted instead of producing {expectedˉcode}.");
    }

    private static void Rejectˉresourceˉresponse(ImmutableArray<byte> source, string expectedˉcode) =>
        Rejectˉresourceˉresponse(source.AsSpan(), expectedˉcode);

    private static void Rejectˉresourceˉresponse(byte[] source, string expectedˉcode) =>
        Rejectˉresourceˉresponse(source.AsSpan(), expectedˉcode);

    private static void Rejectˉresourceˉresponse(ReadOnlySpan<byte> source, string expectedˉcode)
    {
        try
        {
            _ = Resourceˉserviceˉipcˉcodec.Verifyˉresponse(source);
        }
        catch (Resourceˉserviceˉipcˉexception Exception)
        {
            Equal(expectedˉcode, Exception.Code);
            return;
        }
        throw new InvalidOperationException(
            $"A malformed resource response was accepted instead of producing {expectedˉcode}.");
    }

    private static void Throwsˉresourceˉserviceˉipc(string expectedˉcode, Action action)
    {
        try
        {
            action();
        }
        catch (Resourceˉserviceˉipcˉexception Exception)
        {
            Equal(expectedˉcode, Exception.Code);
            return;
        }
        throw new InvalidOperationException(
            $"A resource IPC operation succeeded instead of producing {expectedˉcode}.");
    }

    private static void Assertˉruntimeˉfailure(string expectedˉcode, Action action)
    {
        try
        {
            action();
        }
        catch (Runtimeˉexception Exception)
        {
            Equal(expectedˉcode, Exception.Code);
            return;
        }
        throw new InvalidOperationException(
            $"A runtime operation succeeded instead of producing {expectedˉcode}.");
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
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
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

    private sealed class Bootˉresourceˉreader(
        ImmutableArray<byte> moduleˉbytes,
        ImmutableArray<byte> budgetˉbytes,
        bool includeˉbudget) : IHostedˉfileˉreader
    {
        public ImmutableArray<byte> Readˉbytes(string resourceˉname, int maximumˉbytes)
        {
            var Bytes = resourceˉname switch
            {
                "boot:main.wvb" => moduleˉbytes,
                "boot:main.budget" when includeˉbudget => budgetˉbytes,
                _ => default,
            };
            if (Bytes.IsDefault || Bytes.Length > maximumˉbytes)
            {
                throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Notˉfound,
                    "The fixed typed Windvale OS boot resource was not found.");
            }
            return Bytes;
        }
    }

    private sealed class Resourceˉstoreˉreader(
        ImmutableArray<byte> store,
        bool includeˉstore) : IHostedˉfileˉreader
    {
        public ImmutableArray<byte> Readˉbytes(string resourceˉname, int maximumˉbytes)
        {
            if (!includeˉstore || resourceˉname != "boot:resources.wvrs" || store.Length > maximumˉbytes)
            {
                throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Notˉfound,
                    "The bounded Windvale resource store was not found.");
            }
            return store;
        }
    }

    private sealed class Resourceˉserviceˉreader(
        ImmutableArray<byte> store,
        ImmutableArray<byte> request,
        bool includeˉstore,
        bool includeˉrequest) : IHostedˉfileˉreader
    {
        public ImmutableArray<byte> Readˉbytes(string resourceˉname, int maximumˉbytes)
        {
            var Bytes = resourceˉname switch
            {
                "boot:resources.wvrs" when includeˉstore => store,
                "ipc:resource-request.wvrq" when includeˉrequest => request,
                _ => default,
            };
            if (Bytes.IsDefault || Bytes.Length > maximumˉbytes)
            {
                throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Notˉfound,
                    "The bounded resource-service input was not found.");
            }
            return Bytes;
        }
    }

    private sealed record Testˉcase(string Name, Action Body);

    private sealed record Memoryˉdescriptorˉinput(
        uint Type,
        ulong Physicalˉaddress,
        ulong Pages);
}
