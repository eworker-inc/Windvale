using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.ObjectModel;
using Windvale.Runtime;

namespace Windvale.Seed.Tests;

internal static class Program
{
    private const string SUM_SHA256 = "64134dfd779b353c5e501c9c23337a0c3849bfef2c97a63a07913705b0f10c6b";
    private const string HELLO_SHA256 = "43d565c304cf2e2f5d886ee30b1fabf0b2fbfb0c8cd28bd932d85d5add0bf504";
    private const string FOUNDATION_SHA256 = "0cdf05f6c9e1fb1db0d5ab449207870b5e47cc248f187cd43cd9a5c3c9eee995";
    private const string WVDUMP_CORE_SHA256 = "2957fc5523ae3ca16cf1aaeb9104c14a3342a0aefde9ac591bb689f744f1467f";
    private const string WVO_SAMPLE_SHA256 = "006fd80183da7fbc71d3c6d63b65e6f3551765508fe9dba6f38ba80e002eb28a";
    private const string WVO_CORE_SHA256 = "a5d574ea646946b159d95bd7e51434bfcbf7545083a54541438a79a2e5e999df";

    private const string SUM_SOURCE = """
        module Sumˉdata profile portable;

        data Values: [i32] = [3, 5, 8, 13];

        fn Add(Left: i32, Right: i32) -> i32 {
            return Left + Right;
        }

        export fn Main() -> i32 {
            var Index: i32 = 0;
            var Total: i32 = 0;

            while Index < length(Values) {
                Total = Add(Total, Values[Index]);
                Index = Index + 1;
            }

            return Total;
        }
        """;

    private const string HELLO_SOURCE = """
        module Helloˉwindvale profile hosted;

        capability console.write_line;

        data Greeting: text = "Hello from Windvale";

        export fn Main() -> i32 {
            console.write_line(Greeting);
            return 0;
        }
        """;

    private const string FOUNDATION_SOURCE = """
        module Readˉwvbˉheader profile portable;

        data Moduleˉheader: bytes = [87, 86, 66, 49, 1, 0, 5, 0, 7, 0, 0, 0];

        fn Headerˉisˉvalid(Input: bytes) -> bool {
            if Bytesˉlength(Input) != 12u32 {
                return false;
            }

            let Magic: bytes = Bytesˉslice(Input, 0u32, 4u32);
            if Bytesˉreadˉu8(Magic, 0u32) != 87u8 {
                return false;
            }
            if Bytesˉreadˉu8(Magic, 1u32) != 86u8 {
                return false;
            }
            if Bytesˉreadˉu8(Magic, 2u32) != 66u8 {
                return false;
            }
            if Bytesˉreadˉu8(Magic, 3u32) != 49u8 {
                return false;
            }

            let Version: u32 = Bytesˉreadˉu16ˉlittle(Input, 4u32);
            let Minorˉversion: u32 = Bytesˉreadˉu16ˉlittle(Input, 6u32);
            let Sectionˉcount: u32 = Bytesˉreadˉu32ˉlittle(Input, 8u32);
            if Version != 1u32 {
                return false;
            }
            if Minorˉversion != 5u32 {
                return false;
            }
            if Sectionˉcount != 7u32 {
                return false;
            }

            let Arithmeticˉcheck: u32 = 3u32 * 4u32 - 8u32;
            if Arithmeticˉcheck <= 3u32 {
                return false;
            }
            if Arithmeticˉcheck > 4u32 {
                return false;
            }
            if Arithmeticˉcheck >= 5u32 {
                return false;
            }

            var Checkedˉbytes: u32 = 0u32;
            while Checkedˉbytes < 4u32 {
                Checkedˉbytes = Checkedˉbytes + 1u32;
            }

            return Checkedˉbytes == 4u32;
        }

        export fn Main() -> i32 {
            if Headerˉisˉvalid(Moduleˉheader) {
                return 1;
            }

            return 0;
        }
        """;

    private static readonly string WVDUMP_CORE_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Wv-Dump-Core.wv");

    private static readonly string WVO_CORE_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Wvo-Object-Core.wv");

    private static readonly List<(string Name, Action Body)> TESTS =
    [
        ("portable source compiles, verifies, and returns the data sum", Portableˉprogramˉruns),
        ("hosted source requires authorization and writes text", Hostedˉprogramˉruns),
        ("hosted resources are explicit, separated, and bounded", Hostedˉresourcesˉareˉbounded),
        ("compiler output is deterministic and canonical", Compilerˉisˉdeterministic),
        ("module codec round-trips exact canonical bytes", Moduleˉroundˉtrip),
        ("inspector exposes module metadata and disassembly", Inspectorˉisˉuseful),
        ("bool, if, text literals, and calls execute", Additionalˉsemanticsˉrun),
        ("macron names and explicit local mutability execute", Namingˉandˉmutabilityˉrun),
        ("Foundation byte values, slices, and little-endian reads execute", Foundationˉbytesˉrun),
        ("Foundation signed reads and strict UTF-8 text operations execute", Foundationˉtextˉrun),
        ("Foundation constructs deterministic immutable byte values", Foundationˉbyteˉconstructionˉrun),
        ("Windvale wvdump decodes bounded payloads and instructions", Wvˉdumpˉcoreˉwalksˉsections),
        ("Windvale object codec validates canonical symbols and relocations", Objectˉmodelˉroundˉtrip),
        ("Windvale-written object core matches the Stage 0 oracle", Wvoˉobjectˉcoreˉmatchesˉoracle),
        ("immutable nominal records cross function boundaries", Immutableˉrecordsˉrun),
        ("nominal enums and bounded formatting execute", Enumsˉandˉformattingˉrun),
        ("Seed arithmetic and comparison operators execute", Operatorsˉrun),
        ("source diagnostics contain stable codes and locations", Sourceˉdiagnosticsˉareˉuseful),
        ("binary reader rejects malformed envelopes and UTF-8", Malformedˉmodulesˉareˉrejected),
        ("verifier rejects unsafe instruction streams", Unsafeˉbytecodeˉisˉrejected),
        ("runtime traps overflow and data bounds", Runtimeˉtrapsˉareˉdeterministic),
        ("runtime enforces instruction and call-depth limits", Runtimeˉlimitsˉareˉenforced),
        ("bounded random input never escapes diagnostic boundaries", Randomˉinputˉisˉcontained),
        ("golden hashes identify the cross-host contract", Goldenˉhashesˉmatch),
    ];

    private static Conformanceˉcontract? Contract;

    public static int Main(string[] arguments)
    {
        if (arguments.Length == 3 && arguments[0] == "--compare-reports")
        {
            return Compareˉreports(arguments[1], arguments[2]);
        }

        if (arguments.Length is not (0 or 2) || (arguments.Length == 2 && arguments[0] != "--report"))
        {
            Console.Error.WriteLine(
                "Usage: Windvale.Seed.Tests [--report <path>] | --compare-reports <first> <second>");
            return 64;
        }

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
        Console.WriteLine($"Tests: {TESTS.Count}, Passed: {TESTS.Count - Failures}, Failed: {Failures}");
        if (Failures != 0)
        {
            return 1;
        }

        if (arguments.Length == 2)
        {
            Writeˉreport(arguments[1]);
        }

        return 0;
    }

    private static void Portableˉprogramˉruns()
    {
        var Bytes = Compileˉsuccess(SUM_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        Equal(Moduleˉprofile.Portable, Module.Module.Profile);
        Equal(0, Module.Module.Capabilities.Length);
        var Output = new StringWriter();
        var Runtime = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(Output),
            Runtimeˉoptions.Portableˉdefaults);
        var Result = Runtime.Runˉmain();
        Equal(29, Result.Exitˉcode);
        Equal(string.Empty, Output.ToString());
        True(Result.Executedˉinstructions > 0, "The runtime did not count executed instructions.");
    }

    private static void Hostedˉprogramˉruns()
    {
        var Bytes = Compileˉsuccess(HELLO_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        Equal(Moduleˉprofile.Hosted, Module.Module.Profile);
        Equal(Capabilityˉcatalog.CONSOLE_WRITE_LINE, Module.Module.Capabilities.Single().Name);

        var Unauthorized = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults);
        Throwsˉruntime("WVR3010", () => _ = Unauthorized.Runˉmain());

        var Output = new StringWriter();
        var Authorized = ImmutableHashSet.Create(
            StringComparer.Ordinal,
            Capabilityˉcatalog.CONSOLE_WRITE_LINE);
        var Runtime = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(Output),
            new(Authorized));
        var Result = Runtime.Runˉmain();
        Equal(0, Result.Exitˉcode);
        Equal("Hello from Windvale\n", Output.ToString());
    }

    private static void Hostedˉresourcesˉareˉbounded()
    {
        const string Source = """
            module Hostedˉresources profile hosted;

            capability console.write;
            capability console.write_line;
            capability diagnostic.write_line;
            capability file.read_bytes;
            capability file.write_bytes;
            capability process.argument;
            capability process.argument_count;

            export fn Main() -> i32 {
                if process.argument_count() != 2u32 {
                    return 1;
                }

                let Resourceˉname: text = process.argument(0u32);
                console.write(Resourceˉname);
                console.write_line(Textˉconcat(":", process.argument(1u32)));
                let Input: bytes = file.read_bytes(Resourceˉname);
                file.write_bytes(process.argument(1u32), Input);
                console.write_line(Textˉconcat("bytes=", U32ˉformat(Bytesˉlength(Input))));
                diagnostic.write_line("note");
                return 0;
            }
            """;

        var Module = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Source));
        Sequenceˉequal(
            [
                Capabilityˉcatalog.CONSOLE_WRITE,
                Capabilityˉcatalog.CONSOLE_WRITE_LINE,
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
                Capabilityˉcatalog.FILE_READ_BYTES,
                Capabilityˉcatalog.FILE_WRITE_BYTES,
                Capabilityˉcatalog.PROCESS_ARGUMENT,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
            ],
            Module.Module.Capabilities.Select(Capability => Capability.Name));
        var Authorized = Module.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Output = new StringWriter();
        var Diagnostics = new StringWriter();
        var Files = new Testˉfileˉreader((Resourceˉname, Maximumˉbytes) =>
        {
            Equal("input.wvb", Resourceˉname);
            Equal(Bytecodeˉlimits.MAX_BYTE_DATA_BYTES, Maximumˉbytes);
            return [87, 86, 66];
        });
        var Fileˉwriter = new Capturingˉfileˉwriter();
        var Runtime = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["input.wvb", "tail"],
                Output,
                Diagnostics,
                Files,
                Fileˉwriter)),
            new(Authorized));
        Equal(0, Runtime.Runˉmain().Exitˉcode);
        Equal("input.wvb:tail\nbytes=3\n", Output.ToString());
        Equal("note\n", Diagnostics.ToString());
        Equal(1, Fileˉwriter.Writeˉcount);
        Equal("tail", Fileˉwriter.Resourceˉname);
        Sequenceˉequal<byte>([87, 86, 66], Fileˉwriter.Bytes);

        var Unsupported = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            new(Authorized));
        Throwsˉruntime("WVR3001", () => _ = Unsupported.Runˉmain());

        const string Badˉargument = """
            module Badˉargument profile hosted;
            capability process.argument;
            export fn Main() -> i32 {
                process.argument(0u32);
                return 0;
            }
            """;
        var Badˉargumentˉmodule = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Badˉargument));
        var Badˉargumentˉruntime = new Referenceˉruntime(
            Badˉargumentˉmodule,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                TextWriter.Null)),
            new(ImmutableHashSet.Create(StringComparer.Ordinal, Capabilityˉcatalog.PROCESS_ARGUMENT)));
        Throwsˉruntime("WVR3020", () => _ = Badˉargumentˉruntime.Runˉmain());

        const string Fileˉsource = """
            module Fileˉresource profile hosted;
            capability file.read_bytes;
            export fn Main() -> i32 {
                file.read_bytes("input.wvb");
                return 0;
            }
            """;
        var Fileˉmodule = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Fileˉsource));
        var Fileˉauthorization = new Runtimeˉoptions(
            ImmutableHashSet.Create(StringComparer.Ordinal, Capabilityˉcatalog.FILE_READ_BYTES));
        var Missingˉruntime = new Referenceˉruntime(
            Fileˉmodule,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                TextWriter.Null,
                new Testˉfileˉreader((_, _) => throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Notˉfound,
                    "The requested test resource was not found.")))),
            Fileˉauthorization);
        Throwsˉruntime("WVR3022", () => _ = Missingˉruntime.Runˉmain());

        var Oversizedˉruntime = new Referenceˉruntime(
            Fileˉmodule,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                TextWriter.Null,
                new Testˉfileˉreader((_, Maximumˉbytes) =>
                    ImmutableArray.Create(new byte[Maximumˉbytes + 1])))),
            Fileˉauthorization);
        Throwsˉruntime("WVR3025", () => _ = Oversizedˉruntime.Runˉmain());

        const string Invalidˉresult = """
            module Invalidˉhostˉresult profile hosted;
            capability process.argument_count;
            export fn Main() -> i32 {
                process.argument_count();
                return 0;
            }
            """;
        var Invalidˉresultˉmodule = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Invalidˉresult));
        var Invalidˉresultˉruntime = new Referenceˉruntime(
            Invalidˉresultˉmodule,
            new Invalidˉresultˉcapabilityˉhost(),
            new(ImmutableHashSet.Create(
                StringComparer.Ordinal,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT)));
        Throwsˉruntime("WVR3013", () => _ = Invalidˉresultˉruntime.Runˉmain());

        Throwsˉruntime(
            "WVR3027",
            () => _ = new Hostedˉresourceˉcontext(
                [.. Enumerable.Repeat("a", Hostedˉresourceˉlimits.MAX_ARGUMENTS + 1)],
                TextWriter.Null,
                TextWriter.Null));
        Throwsˉruntime(
            "WVR3027",
            () => _ = new Hostedˉresourceˉcontext(
                [new string('a', Hostedˉresourceˉlimits.MAX_ARGUMENT_UTF8_BYTES + 1)],
                TextWriter.Null,
                TextWriter.Null));
        Throwsˉruntime(
            "WVR3027",
            () => _ = new Hostedˉresourceˉcontext(
                [.. Enumerable.Repeat(new string('a', 4096), 17)],
                TextWriter.Null,
                TextWriter.Null));
        Throwsˉruntime(
            "WVR3027",
            () => _ = new Hostedˉresourceˉcontext(
                ["\uD800"],
                TextWriter.Null,
                TextWriter.Null));
    }

    private static void Compilerˉisˉdeterministic()
    {
        var First = Compileˉsuccess(SUM_SOURCE);
        var Second = Compileˉsuccess(SUM_SOURCE);
        Sequenceˉequal(First, Second);

        const string Reorderedˉsource = """
            module Canonical profile portable;
            data Zed: text = "z";
            data Alpha: [i32] = [1];
            export fn Main() -> i32 { return Zebra(); }
            fn Zebra() -> i32 { return Alpha[0]; }
            """;
        var Module = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Reorderedˉsource));
        Sequenceˉequal(["Alpha", "Zed"], Module.Module.Data.Select(Data => Data.Name));
        Sequenceˉequal(["Main", "Zebra"], Module.Module.Functions.Select(Function => Function.Name));
    }

    private static void Moduleˉroundˉtrip()
    {
        var Bytes = Compileˉsuccess(SUM_SOURCE);
        var Parsed = Moduleˉcodec.Read(Bytes);
        var Rewritten = Moduleˉcodec.Write(Parsed);
        Sequenceˉequal(Bytes, Rewritten);
    }

    private static void Inspectorˉisˉuseful()
    {
        var Bytes = Compileˉsuccess(SUM_SOURCE);
        var Inspection = Moduleˉinspector.Inspect(Moduleˉcodec.Readˉandˉverify(Bytes), Bytes);
        Contains(Inspection, "Module: Sumˉdata");
        Contains(Inspection, "Data (1)");
        Contains(Inspection, "data.load.i32");
        Contains(Inspection, "call function[0] (Add)");
        Contains(Inspection, $"SHA-256: {SUM_SHA256}");

        var Unicodeˉsource = $$"""
            module Unicodeˉpreview profile portable;
            data Message: text = "{{new string('a', 79)}}😀";
            export fn Main() -> i32 { return 0; }
            """;
        var Unicodeˉbytes = Compileˉsuccess(Unicodeˉsource);
        var Unicodeˉinspection = Moduleˉinspector.Inspect(
            Moduleˉcodec.Readˉandˉverify(Unicodeˉbytes),
            Unicodeˉbytes);
        Contains(Unicodeˉinspection, "\\uD83D\\uDE00");
        False(
            Unicodeˉinspection.Contains("\\uFFFD", StringComparison.OrdinalIgnoreCase),
            "The inspector split a Unicode scalar while creating its preview.");
    }

    private static void Additionalˉsemanticsˉrun()
    {
        const string Source = """
            module Conditions profile hosted;
            capability console.write_line;
            fn Isˉanswer(Value: i32) -> bool { return !(Value != 42); }
            export fn Main() -> i32 {
                if Isˉanswer(6 * 7) {
                    console.write_line("answer");
                    return 42;
                } else {
                    return 1;
                }
            }
            """;
        var Module = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Source));
        var Output = new StringWriter();
        var Runtime = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(Output),
            new(ImmutableHashSet.Create(StringComparer.Ordinal, Capabilityˉcatalog.CONSOLE_WRITE_LINE)));
        Equal(42, Runtime.Runˉmain().Exitˉcode);
        Equal("answer\n", Output.ToString());
    }

    private static void Namingˉandˉmutabilityˉrun()
    {
        const string Source = """
            module Namingˉandˉmutability profile portable;
            fn Addˉone(Value: i32) -> i32 { return Value + 1; }
            export fn Main() -> i32 {
                let Baseˉvalue: i32 = 40;
                var Resultˉvalue: i32 = Baseˉvalue;
                Resultˉvalue = Addˉone(Resultˉvalue);
                return Resultˉvalue;
            }
            """;
        Equal(41, Runˉportable(Source));

        const string Immutableˉassignment = """
            module Immutableˉassignment profile portable;
            export fn Main() -> i32 {
                let Value: i32 = 1;
                Value = 2;
                return Value;
            }
            """;
        Hasˉdiagnostic(Immutableˉassignment, "WVC2042");

        const string Parameterˉassignment = """
            module Parameterˉassignment profile portable;
            fn Change(Value: i32) -> i32 {
                Value = 2;
                return Value;
            }
            export fn Main() -> i32 { return Change(1); }
            """;
        Hasˉdiagnostic(Parameterˉassignment, "WVC2042");

        const string Malformedˉseparator = """
            module Badˉˉname profile portable;
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Malformedˉseparator, "WVC2004");

        const string Confusableˉseparator = """
            module Bad¯name profile portable;
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Confusableˉseparator, "WVC1002");

        const string Unknownˉrecord = """
            module Broken profile portable;
            export fn Main(Value: Missing) -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Unknownˉrecord, "WVC2085");

        const string Duplicateˉrecordˉfield = """
            module Broken profile portable;
            record Pair { Value: i32; Value: u32; }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Duplicateˉrecordˉfield, "WVC2082");

        const string Emptyˉrecord = """
            module Broken profile portable;
            record Empty { }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Emptyˉrecord, "WVC2084");

        const string Nestedˉrecord = """
            module Broken profile portable;
            record Inner { Value: i32; }
            record Outer { Value: Inner; }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Nestedˉrecord, "WVC2083");

        const string Wrongˉconstructorˉtype = """
            module Broken profile portable;
            record Pair { Value: i32; }
            export fn Main() -> i32 { Pair(1u32); return 0; }
            """;
        Hasˉdiagnostic(Wrongˉconstructorˉtype, "WVC2070");

        const string Missingˉfield = """
            module Broken profile portable;
            record Pair { Value: i32; }
            export fn Main() -> i32 {
                let Pairˉvalue: Pair = Pair(1);
                return Pairˉvalue.Missing;
            }
            """;
        Hasˉdiagnostic(Missingˉfield, "WVC2087");

        const string Constructorˉnameˉconflict = """
            module Broken profile portable;
            record Pair { Value: i32; }
            fn Pair(Value: i32) -> i32 { return Value; }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Constructorˉnameˉconflict, "WVC2025");

        const string Nominalˉmismatch = """
            module Broken profile portable;
            record Left { Value: i32; }
            record Right { Value: i32; }
            export fn Main() -> i32 {
                let Value: Left = Right(1);
                return 0;
            }
            """;
        Hasˉdiagnostic(Nominalˉmismatch, "WVC2070");

        const string Duplicateˉenumˉmember = """
            module Broken profile portable;
            enum State { Ready = 0; Ready = 1; }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Duplicateˉenumˉmember, "WVC2093");

        const string Duplicateˉenumˉvalue = """
            module Broken profile portable;
            enum State { Ready = 0; Failed = 0; }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Duplicateˉenumˉvalue, "WVC2094");

        const string Emptyˉenum = """
            module Broken profile portable;
            enum State { }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Emptyˉenum, "WVC2095");

        const string Unsignedˉenumˉvalue = """
            module Broken profile portable;
            enum State { Ready = 0u32; }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Unsignedˉenumˉvalue, "WVC2099");

        const string Missingˉenumˉmember = """
            module Broken profile portable;
            enum State { Ready = 0; }
            export fn Main() -> i32 { State.Missing; return 0; }
            """;
        Hasˉdiagnostic(Missingˉenumˉmember, "WVC2097");

        const string Nameˉnonˉenum = """
            module Broken profile portable;
            export fn Main() -> i32 { Enumˉname(1); return 0; }
            """;
        Hasˉdiagnostic(Nameˉnonˉenum, "WVC2098");

        const string Enumˉnominalˉmismatch = """
            module Broken profile portable;
            enum Left { Value = 0; }
            enum Right { Value = 0; }
            export fn Main() -> i32 {
                let Value: Left = Right.Value;
                return 0;
            }
            """;
        Hasˉdiagnostic(Enumˉnominalˉmismatch, "WVC2070");
    }

    private static void Foundationˉbytesˉrun()
    {
        var Bytes = Compileˉsuccess(FOUNDATION_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        Equal(Dataˉtype.Bytes, Module.Module.Data.Single().Type);
        var Data = (Bytesˉdataˉdeclaration)Module.Module.Data.Single();
        Sequenceˉequal<byte>([87, 86, 66, 49, 1, 0, 5, 0, 7, 0, 0, 0], Data.Values);
        True(
            Module.Module.Functions.SelectMany(Function => Function.Allˉlocalˉtypes)
                .Contains(Valueˉtype.Bytes),
            "The Foundation module did not preserve its bytes value type.");
        True(
            Module.Module.Functions.SelectMany(Function => Function.Allˉlocalˉtypes)
                .Contains(Valueˉtype.U8),
            "The Foundation module did not preserve its u8 value type.");
        True(
            Module.Module.Functions.SelectMany(Function => Function.Allˉlocalˉtypes)
                .Contains(Valueˉtype.U32),
            "The Foundation module did not preserve its u32 value type.");

        var Rewritten = Moduleˉcodec.Write(Module.Module);
        Sequenceˉequal(Bytes, Rewritten);
        var Inspection = Moduleˉinspector.Inspect(Module, Bytes);
        Contains(Inspection, "bytes.read_u32_little");
        Contains(Inspection, "bytes.slice");
        Equal(FOUNDATION_SHA256, Moduleˉdigest.Calculateˉsha256(Bytes));
        Equal(1, new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);
    }

    private static void Foundationˉtextˉrun()
    {
        const string Source = """
            module Foundationˉtext profile hosted;

            capability console.write_line;

            data Encoded: bytes = [
                87, 105, 110, 100, 118, 97, 108, 101, 32,
                226, 152, 131,
                240, 159, 152, 128
            ];
            data Invalid: bytes = [195, 40];
            data Signed: bytes = [249, 255, 255, 255];
            data Escaped: bytes = [34, 92, 10, 9];

            export fn Main() -> i32 {
                if U32ˉfromˉu8(Bytesˉreadˉu8(Encoded, 0u32)) != 87u32 {
                    return 3;
                }
                if !Textˉutf8ˉisˉvalid(Encoded) {
                    return 1;
                }
                if Textˉutf8ˉisˉvalid(Invalid) {
                    return 2;
                }

                console.write_line(Textˉquote(Textˉfromˉutf8(Encoded)));
                console.write_line(Textˉquote(Textˉfromˉutf8(Escaped)));
                return Bytesˉreadˉi32ˉlittle(Signed, 0u32) + 7;
            }
            """;

        var Bytes = Compileˉsuccess(Source);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        var Inspection = Moduleˉinspector.Inspect(Module, Bytes);
        Contains(Inspection, "bytes.read_i32_little");
        Contains(Inspection, "text.utf8_is_valid");
        Contains(Inspection, "text.from_utf8");
        Contains(Inspection, "text.quote");
        Contains(Inspection, "u32.from_u8");
        var Output = new StringWriter();
        var Result = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(Output),
            new(ImmutableHashSet.Create(
                StringComparer.Ordinal,
                Capabilityˉcatalog.CONSOLE_WRITE_LINE))).Runˉmain();
        Equal(0, Result.Exitˉcode);
        Equal("\"Windvale \\u2603\\uD83D\\uDE00\"\n\"\\\"\\\\\\n\\t\"\n", Output.ToString());

        const string Invalidˉdecode = """
            module Invalidˉutf8 profile portable;
            data Invalid: bytes = [195, 40];
            export fn Main() -> i32 {
                Textˉfromˉutf8(Invalid);
                return 0;
            }
            """;
        Throwsˉruntime("WVR3014", () => Runˉportable(Invalidˉdecode));
    }

    private static void Foundationˉbyteˉconstructionˉrun()
    {
        const string Source = """
            module Foundationˉbyteˉconstruction profile portable;

            export fn Main() -> i32 {
                var Encoded: bytes = Bytesˉfromˉu8(171u8);
                Encoded = Bytesˉconcat(Encoded, Bytesˉfromˉu16ˉlittle(4660u32));
                Encoded = Bytesˉconcat(Encoded, Bytesˉfromˉu32ˉlittle(2309737967u32));
                Encoded = Bytesˉconcat(Encoded, Bytesˉfromˉi32ˉlittle(-7));
                Encoded = Bytesˉconcat(Encoded, Textˉtoˉutf8("WVO"));
                if Bytesˉlength(Encoded) != 14u32 { return 1; }
                if Bytesˉreadˉu8(Encoded, 0u32) != 171u8 { return 2; }
                if Bytesˉreadˉu16ˉlittle(Encoded, 1u32) != 4660u32 { return 3; }
                if Bytesˉreadˉu32ˉlittle(Encoded, 3u32) != 2309737967u32 { return 4; }
                if Bytesˉreadˉi32ˉlittle(Encoded, 7u32) != -7 { return 5; }
                if Bytesˉreadˉu8(Encoded, 11u32) != 87u8 { return 6; }
                if Bytesˉreadˉu8(Encoded, 12u32) != 86u8 { return 7; }
                if Bytesˉreadˉu8(Encoded, 13u32) != 79u8 { return 8; }
                return 0;
            }
            """;

        var Bytes = Compileˉsuccess(Source);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        var Inspection = Moduleˉinspector.Inspect(Module, Bytes);
        Contains(Inspection, "bytes.concat");
        Contains(Inspection, "bytes.from_u8");
        Contains(Inspection, "bytes.from_u16_little");
        Contains(Inspection, "bytes.from_u32_little");
        Contains(Inspection, "bytes.from_i32_little");
        Contains(Inspection, "text.to_utf8");
        Equal(0, new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);

        const string U16ˉoverflow = """
            module U16ˉoverflow profile portable;
            export fn Main() -> i32 {
                Bytesˉfromˉu16ˉlittle(65536u32);
                return 0;
            }
            """;
        Throwsˉruntime("WVR3016", () => Runˉportable(U16ˉoverflow));
    }

    private static void Wvˉdumpˉcoreˉwalksˉsections()
    {
        var Bytes = Compileˉsuccess(WVDUMP_CORE_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        Equal("Wvˉdumpˉcore", Module.Module.Name);
        Equal(Moduleˉprofile.Hosted, Module.Module.Profile);
        Equal(10, Module.Module.Data.OfType<Bytesˉdataˉdeclaration>().Count());
        Sequenceˉequal(
            [
                Capabilityˉcatalog.CONSOLE_WRITE_LINE,
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
                Capabilityˉcatalog.FILE_READ_BYTES,
                Capabilityˉcatalog.PROCESS_ARGUMENT,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
            ],
            Module.Module.Capabilities.Select(Capability => Capability.Name));
        Equal(5, Module.Module.Types.Length);
        Equal("Wvbˉinspection", Module.Module.Types[0].Name);
        Equal("Wvbˉpayloadˉinspection", Module.Module.Types[1].Name);
        Equal("Wvbˉscan", Module.Module.Types[2].Name);
        Equal("Wvbˉsection", Module.Module.Types[3].Name);
        Equal("Wvbˉstatus", Module.Module.Types[4].Name);
        Equal(3, ((Recordˉtypeˉdeclaration)Module.Module.Types[0]).Fields.Length);
        Equal(4, ((Recordˉtypeˉdeclaration)Module.Module.Types[1]).Fields.Length);
        Equal(4, ((Recordˉtypeˉdeclaration)Module.Module.Types[2]).Fields.Length);
        Equal(6, ((Recordˉtypeˉdeclaration)Module.Module.Types[3]).Fields.Length);
        Equal(
            Valueˉshape.Forˉenum(4),
            ((Recordˉtypeˉdeclaration)Module.Module.Types[0]).Fields[0].Type);
        Equal(19, ((Enumˉtypeˉdeclaration)Module.Module.Types[4]).Members.Length);

        var Inspectˉfunction = Module.Module.Functions.Single(
            Function => Function.Name == "Inspectˉwvbˉenvelope");
        Equal(Valueˉshape.Forˉrecord(0), Inspectˉfunction.Returnˉtype);

        var Validˉdata = (Bytesˉdataˉdeclaration)Module.Module.Data.Single(
            Data => Data.Name == "Validˉmodule");
        var Embeddedˉmodule = Moduleˉcodec.Readˉandˉverify(Validˉdata.Values.AsSpan());
        Equal("A", Embeddedˉmodule.Module.Name);
        Equal(Moduleˉprofile.Portable, Embeddedˉmodule.Module.Profile);
        Equal(0, Embeddedˉmodule.Module.Functions.Length);

        var Hostileˉlength = (Bytesˉdataˉdeclaration)Module.Module.Data.Single(
            Data => Data.Name == "Hostileˉlengthˉmodule");
        Sequenceˉequal<byte>([255, 255, 255, 255], Hostileˉlength.Values.TakeLast(4));

        var Inspection = Moduleˉinspector.Inspect(Module, Bytes);
        Contains(Inspection, "Inspectˉwvbˉenvelope");
        Contains(Inspection, "bytes.read_u32_little");
        Contains(Inspection, "u32.less_equal");
        Contains(Inspection, "Nominal types (5)");
        Contains(Inspection, "record.create");
        Contains(Inspection, "record.field");
        Contains(Inspection, "enum Wvbˉstatus");
        Contains(Inspection, "enum.const");
        Contains(Inspection, "enum.name");
        Contains(Inspection, "u32.format");
        Contains(Inspection, "text.concat");
        Contains(Inspection, "bytes.read_i32_little");
        Contains(Inspection, "text.utf8_is_valid");
        Contains(Inspection, "text.from_utf8");
        Contains(Inspection, "text.quote");
        Contains(Inspection, "u32.from_u8");
        Equal(WVDUMP_CORE_SHA256, Moduleˉdigest.Calculateˉsha256(Bytes));
        var Authorized = Module.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        Equal(0, new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                TextWriter.Null,
                new Testˉfileˉreader((_, _) => throw new InvalidOperationException(
                    "The no-argument WvDump self-test must not read a hosted file.")))),
            new(Authorized)).Runˉmain().Exitˉcode);

        var Hostedˉoutput = new StringWriter();
        var Hostedˉdiagnostics = new StringWriter();
        var Hostedˉrun = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["real.wvb"],
                Hostedˉoutput,
                Hostedˉdiagnostics,
                new Testˉfileˉreader((Name, Maximumˉbytes) =>
                {
                    Equal("real.wvb", Name);
                    True(Validˉdata.Values.Length <= Maximumˉbytes, "The hosted byte limit was too small.");
                    return Validˉdata.Values;
                }))),
            new(Authorized)).Runˉmain();
        Equal(0, Hostedˉrun.Exitˉcode);
        Equal(
            """
            wvdump 1
            module version=1.5 profile=portable name="A"
            section name=module offset=20 bytes=6 count=1
            section name=capabilities offset=34 bytes=4 count=0
            section name=data offset=46 bytes=4 count=0
            section name=functions offset=58 bytes=4 count=0
            section name=code offset=70 bytes=0 count=0
            section name=exports offset=78 bytes=4 count=0
            section name=types offset=90 bytes=4 count=0
            """.Replace("\r\n", "\n", StringComparison.Ordinal) + "\n",
            Hostedˉoutput.ToString());
        Equal(string.Empty, Hostedˉdiagnostics.ToString());

        var Malformedˉpayload = Validˉdata.Values.ToArray();
        var Dataˉpayload = Findˉsectionˉpayload(Malformedˉpayload, Sectionˉkind.Data);
        BinaryPrimitives.WriteUInt32LittleEndian(Malformedˉpayload.AsSpan(Dataˉpayload), 1u);
        var Malformedˉpayloadˉoutput = new StringWriter();
        var Malformedˉpayloadˉdiagnostics = new StringWriter();
        var Malformedˉpayloadˉrun = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["bad-payload.wvb"],
                Malformedˉpayloadˉoutput,
                Malformedˉpayloadˉdiagnostics,
                new Testˉfileˉreader((_, _) => Malformedˉpayload.ToImmutableArray()))),
            new(Authorized)).Runˉmain();
        Equal(2, Malformedˉpayloadˉrun.Exitˉcode);
        Equal(string.Empty, Malformedˉpayloadˉoutput.ToString());
        Equal(
            $"Outˉofˉbounds declarations=1 instructions=0 offset={Dataˉpayload + sizeof(uint)}\n",
            Malformedˉpayloadˉdiagnostics.ToString());

        var Invalidˉutf8 = Validˉdata.Values.ToArray();
        var Moduleˉpayload = Findˉsectionˉpayload(Invalidˉutf8, Sectionˉkind.Module);
        Invalidˉutf8[Moduleˉpayload + 5] = byte.MaxValue;
        var Invalidˉutf8ˉoutput = new StringWriter();
        var Invalidˉutf8ˉdiagnostics = new StringWriter();
        var Invalidˉutf8ˉrun = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["bad-utf8.wvb"],
                Invalidˉutf8ˉoutput,
                Invalidˉutf8ˉdiagnostics,
                new Testˉfileˉreader((_, _) => Invalidˉutf8.ToImmutableArray()))),
            new(Authorized)).Runˉmain();
        Equal(2, Invalidˉutf8ˉrun.Exitˉcode);
        Equal(string.Empty, Invalidˉutf8ˉoutput.ToString());
        Equal(
            $"Invalidˉutf8 declarations=0 instructions=0 offset={Moduleˉpayload + 5}\n",
            Invalidˉutf8ˉdiagnostics.ToString());

        var Malformedˉopcode = Compileˉsuccess(SUM_SOURCE);
        var Codeˉpayload = Findˉsectionˉpayload(Malformedˉopcode, Sectionˉkind.Code);
        Malformedˉopcode[Codeˉpayload] = byte.MaxValue;
        var Malformedˉopcodeˉoutput = new StringWriter();
        var Malformedˉopcodeˉdiagnostics = new StringWriter();
        var Malformedˉopcodeˉrun = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["bad-opcode.wvb"],
                Malformedˉopcodeˉoutput,
                Malformedˉopcodeˉdiagnostics,
                new Testˉfileˉreader((_, _) => Malformedˉopcode.ToImmutableArray()))),
            new(Authorized)).Runˉmain();
        Equal(2, Malformedˉopcodeˉrun.Exitˉcode);
        Equal(string.Empty, Malformedˉopcodeˉoutput.ToString());
        Equal(
            $"Unknownˉopcode declarations=2 instructions=0 offset={Codeˉpayload}\n",
            Malformedˉopcodeˉdiagnostics.ToString());

        var Invalidˉoutput = new StringWriter();
        var Invalidˉdiagnostics = new StringWriter();
        var Invalidˉrun = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["bad.wvb"],
                Invalidˉoutput,
                Invalidˉdiagnostics,
                new Testˉfileˉreader((_, _) => Hostileˉlength.Values))),
            new(Authorized)).Runˉmain();
        Equal(2, Invalidˉrun.Exitˉcode);
        Equal(string.Empty, Invalidˉoutput.ToString());
        Equal("Outˉofˉbounds sections=0 offset=20\n", Invalidˉdiagnostics.ToString());
    }

    private static void Objectˉmodelˉroundˉtrip()
    {
        var Value = Buildˉsampleˉobject();
        var Bytes = Objectˉcodec.Write(Value);
        Equal(189, Bytes.Length);
        Equal(WVO_SAMPLE_SHA256, Objectˉdigest.Calculateˉsha256(Bytes));

        var Verified = Objectˉcodec.Readˉandˉverify(Bytes);
        Sequenceˉequal(Bytes, Objectˉcodec.Write(Verified.Value));
        Equal(Objectˉarchitecture.X86ˉ64, Verified.Value.Architecture);
        Equal(2, Verified.Value.Sections.Length);
        Equal(".text", Verified.Value.Sections[0].Name);
        Equal(Objectˉsectionˉkind.Readˉonlyˉdata, Verified.Value.Sections[1].Kind);
        Equal(3, Verified.Value.Symbols.Length);
        Equal(Objectˉlimits.UNDEFINED_SECTION, Verified.Value.Symbols[2].Sectionˉindex);
        Equal(Objectˉrelocationˉkind.Relativeˉi32, Verified.Value.Relocations.Single().Kind);
        Equal(-4, Verified.Value.Relocations.Single().Addend);
        var Inspection = Objectˉinspector.Inspect(Verified, Bytes);
        Contains(Inspection, "Sections (2)");
        Contains(Inspection, "Console_write binding=Import");
        Contains(Inspection, "kind=Relativeˉi32 section=0 offset=1 symbol=2 addend=-4");

        var Badˉmagic = Bytes.ToArray();
        Badˉmagic[0] = 0;
        Throwsˉobject("WVO1002", () => Objectˉcodec.Readˉandˉverify(Badˉmagic));

        var Badˉversion = Bytes.ToArray();
        Badˉversion[6] = 1;
        Throwsˉobject("WVO1003", () => Objectˉcodec.Readˉandˉverify(Badˉversion));

        var Badˉcount = Bytes.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(Badˉcount.AsSpan(12), uint.MaxValue);
        Throwsˉobject("WVO1013", () => Objectˉcodec.Readˉandˉverify(Badˉcount));

        var Badˉsectionˉkind = Bytes.ToArray();
        Badˉsectionˉkind[24] = byte.MaxValue;
        Throwsˉobject("WVO1007", () => Objectˉcodec.Readˉandˉverify(Badˉsectionˉkind));

        var Badˉutf8 = Bytes.ToArray();
        Badˉutf8[44] = byte.MaxValue;
        Throwsˉobject("WVO1014", () => Objectˉcodec.Readˉandˉverify(Badˉutf8));
        Throwsˉobject("WVO1016", () => Objectˉcodec.Readˉandˉverify(Bytes.AsSpan(0, Bytes.Length - 1)));
        Throwsˉobject("WVO1015", () => Objectˉcodec.Readˉandˉverify([.. Bytes, (byte)0]));

        var Noncanonicalˉsections = Value with
        {
            Sections = [Value.Sections[1], Value.Sections[0]],
        };
        Throwsˉobject("WVO2012", () => Objectˉverifier.Verify(Noncanonicalˉsections));

        var Badˉsymbol = Value with
        {
            Symbols =
            [
                Value.Symbols[0] with { Offset = 4 },
                Value.Symbols[1],
                Value.Symbols[2],
            ],
        };
        Throwsˉobject("WVO2025", () => Objectˉverifier.Verify(Badˉsymbol));

        var Badˉplaceholder = Value with
        {
            Sections =
            [
                Value.Sections[0] with { Data = [232, 1, 0, 0, 0, 195] },
                Value.Sections[1],
            ],
        };
        Throwsˉobject("WVO2035", () => Objectˉverifier.Verify(Badˉplaceholder));

        var Overlappingˉrelocations = Value with
        {
            Relocations =
            [
                Value.Relocations[0],
                Value.Relocations[0] with { Offset = 2 },
            ],
        };
        Throwsˉobject("WVO2033", () => Objectˉverifier.Verify(Overlappingˉrelocations));
    }

    private static void Wvoˉobjectˉcoreˉmatchesˉoracle()
    {
        var Moduleˉbytes = Compileˉsuccess(WVO_CORE_SOURCE);
        Equal(WVO_CORE_SHA256, Moduleˉdigest.Calculateˉsha256(Moduleˉbytes));
        var Module = Moduleˉcodec.Readˉandˉverify(Moduleˉbytes);
        Equal("Wvoˉobjectˉcore", Module.Module.Name);
        Sequenceˉequal(
            [
                Capabilityˉcatalog.CONSOLE_WRITE_LINE,
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
                Capabilityˉcatalog.FILE_WRITE_BYTES,
                Capabilityˉcatalog.PROCESS_ARGUMENT,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
            ],
            Module.Module.Capabilities.Select(Capability => Capability.Name));
        var Moduleˉinspection = Moduleˉinspector.Inspect(Module, Moduleˉbytes);
        Contains(Moduleˉinspection, "bytes.concat");
        Contains(Moduleˉinspection, "bytes.from_u16_little");
        Contains(Moduleˉinspection, "bytes.from_i32_little");
        Contains(Moduleˉinspection, "text.to_utf8");
        Contains(Moduleˉinspection, "call.capability capability[2] (file.write_bytes)");

        var Authorized = Module.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Selfˉtestˉwriter = new Capturingˉfileˉwriter();
        var Selfˉtestˉresult = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                TextWriter.Null,
                null,
                Selfˉtestˉwriter)),
            new(Authorized, Maximumˉinstructions: 10_000_000)).Runˉmain();
        Equal(0, Selfˉtestˉresult.Exitˉcode);
        Equal(0, Selfˉtestˉwriter.Writeˉcount);

        var Hostedˉwriter = new Capturingˉfileˉwriter();
        var Hostedˉoutput = new StringWriter();
        var Hostedˉdiagnostics = new StringWriter();
        var Hostedˉresult = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["sample.wvo"],
                Hostedˉoutput,
                Hostedˉdiagnostics,
                null,
                Hostedˉwriter)),
            new(Authorized, Maximumˉinstructions: 10_000_000)).Runˉmain();
        Equal(0, Hostedˉresult.Exitˉcode);
        Equal("Wrote WVO 1.0 bytes=189\n", Hostedˉoutput.ToString());
        Equal(string.Empty, Hostedˉdiagnostics.ToString());
        Equal(1, Hostedˉwriter.Writeˉcount);
        Equal("sample.wvo", Hostedˉwriter.Resourceˉname);
        var Oracleˉbytes = Objectˉcodec.Write(Buildˉsampleˉobject());
        Sequenceˉequal(Oracleˉbytes, Hostedˉwriter.Bytes);
        Equal(WVO_SAMPLE_SHA256, Objectˉdigest.Calculateˉsha256(Hostedˉwriter.Bytes.AsSpan()));
        _ = Objectˉcodec.Readˉandˉverify(Hostedˉwriter.Bytes.AsSpan());
    }

    private static void Immutableˉrecordsˉrun()
    {
        const string Source = """
            module Recordˉflow profile portable;

            record Pair {
                Left: i32;
                Right: u32;
            }

            fn Make(Left: i32, Right: u32) -> Pair {
                return Pair(Left, Right);
            }

            fn Readˉleft(Value: Pair) -> i32 {
                return Value.Left;
            }

            export fn Main() -> i32 {
                let Value: Pair = Make(42, 9u32);
                if Value.Right != 9u32 {
                    return 0;
                }

                return Readˉleft(Value);
            }
            """;

        var Bytes = Compileˉsuccess(Source);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        var Pair = (Recordˉtypeˉdeclaration)Module.Module.Types.Single();
        Equal("Pair", Pair.Name);
        Equal("Left", Pair.Fields[0].Name);
        Equal(Valueˉtype.I32, Pair.Fields[0].Type);
        Equal("Right", Pair.Fields[1].Name);
        Equal(Valueˉtype.U32, Pair.Fields[1].Type);
        Equal(Valueˉshape.Forˉrecord(0), Module.Module.Functions.Single(
            Function => Function.Name == "Make").Returnˉtype);
        Equal(Valueˉshape.Forˉrecord(0), Module.Module.Functions.Single(
            Function => Function.Name == "Readˉleft").Parameterˉtypes.Single());
        Sequenceˉequal(Bytes, Moduleˉcodec.Write(Module.Module));

        var Inspection = Moduleˉinspector.Inspect(Module, Bytes);
        Contains(Inspection, "record Pair");
        Contains(Inspection, "[0] Left: i32");
        Contains(Inspection, "[1] Right: u32");
        Contains(Inspection, "record.create type[0] (Pair)");
        Contains(Inspection, "record.field 0");
        Equal(42, new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);
    }

    private static void Enumsˉandˉformattingˉrun()
    {
        const string Source = """
            module Enumˉformat profile hosted;

            capability console.write_line;

            enum Runˉstatus {
                Ready = 7;
                Failed = 9;
            }

            record Runˉresult {
                Status: Runˉstatus;
                Count: u32;
                Delta: i32;
                Byte: u8;
            }

            fn Describe(Value: Runˉresult) -> text {
                return Textˉconcat(
                    Enumˉname(Value.Status),
                    Textˉconcat(
                        " count=",
                        Textˉconcat(
                            U32ˉformat(Value.Count),
                            Textˉconcat(
                                " delta=",
                                Textˉconcat(
                                    I32ˉformat(Value.Delta),
                                    Textˉconcat(" byte=", U8ˉformat(Value.Byte))
                                )
                            )
                        )
                    )
                );
            }

            export fn Main() -> i32 {
                let Value: Runˉresult = Runˉresult(
                    Runˉstatus.Ready,
                    42u32,
                    -7,
                    255u8
                );
                if Value.Status != Runˉstatus.Ready {
                    return 1;
                }
                if Value.Status == Runˉstatus.Failed {
                    return 2;
                }

                console.write_line(Describe(Value));
                return 0;
            }
            """;

        var Bytes = Compileˉsuccess(Source);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        Equal(2, Module.Module.Types.Length);
        var Record = (Recordˉtypeˉdeclaration)Module.Module.Types[0];
        var Enum = (Enumˉtypeˉdeclaration)Module.Module.Types[1];
        Equal("Runˉresult", Record.Name);
        Equal("Runˉstatus", Enum.Name);
        Equal(Valueˉshape.Forˉenum(1), Record.Fields[0].Type);
        Equal("Ready", Enum.Members[0].Name);
        Equal(7, Enum.Members[0].Value);
        Equal("Failed", Enum.Members[1].Name);
        Equal(9, Enum.Members[1].Value);
        Sequenceˉequal(Bytes, Moduleˉcodec.Write(Module.Module));

        var Inspection = Moduleˉinspector.Inspect(Module, Bytes);
        Contains(Inspection, "enum Runˉstatus");
        Contains(Inspection, "enum.const type[1] (Runˉstatus)");
        Contains(Inspection, "enum.not_equal");
        Contains(Inspection, "enum.equal");
        Contains(Inspection, "enum.name");
        Contains(Inspection, "i32.format");
        Contains(Inspection, "u8.format");
        Contains(Inspection, "u32.format");
        Contains(Inspection, "text.concat");

        var Output = new StringWriter();
        var Result = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(Output),
            Runtimeˉoptions.Portableˉdefaults with
            {
                Authorizedˉcapabilities = ImmutableHashSet.Create(
                    StringComparer.Ordinal,
                    Capabilityˉcatalog.CONSOLE_WRITE_LINE),
            }).Runˉmain();
        Equal(0, Result.Exitˉcode);
        Equal("Ready count=42 delta=-7 byte=255\n", Output.ToString());
    }

    private static void Sourceˉdiagnosticsˉareˉuseful()
    {
        const string Typeˉmismatch = """
            module Broken profile portable;
            export fn Main() -> i32 {
                let Wrong: bool = 1;
                return 0;
            }
            """;
        var Typeˉresult = Seedˉcompiler.Compile(Typeˉmismatch);
        False(Typeˉresult.Success, "Type-invalid source compiled successfully.");
        var Typeˉdiagnostic = Typeˉresult.Diagnostics.Single(Diagnostic => Diagnostic.Code == "WVC2070");
        Equal(3, Typeˉdiagnostic.Span.Line);
        True(Typeˉdiagnostic.Span.Column > 1, "The diagnostic column was not preserved.");

        const string Missingˉcapability = """
            module Broken profile hosted;
            export fn Main() -> i32 {
                console.write_line("no declaration");
                return 0;
            }
            """;
        Hasˉdiagnostic(Missingˉcapability, "WVC2064");

        const string Missingˉreturn = """
            module Broken profile portable;
            export fn Main() -> i32 { let Value: i32 = 1; }
            """;
        Hasˉdiagnostic(Missingˉreturn, "WVC2030");

        const string Badˉescape = """
            module Broken profile portable;
            data Text: text = "\q";
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Badˉescape, "WVC1003");

        const string U8ˉoverflow = """
            module Broken profile portable;
            export fn Main() -> i32 { 256u8; return 0; }
            """;
        Hasˉdiagnostic(U8ˉoverflow, "WVC1001");

        const string U32ˉoverflow = """
            module Broken profile portable;
            export fn Main() -> i32 { 4294967296u32; return 0; }
            """;
        Hasˉdiagnostic(U32ˉoverflow, "WVC1001");

        const string Byteˉdataˉoverflow = """
            module Broken profile portable;
            data Values: bytes = [256];
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Byteˉdataˉoverflow, "WVC1106");

        const string Intrinsicˉtypeˉmismatch = """
            module Broken profile portable;
            export fn Main() -> i32 { Bytesˉlength(1u32); return 0; }
            """;
        Hasˉdiagnostic(Intrinsicˉtypeˉmismatch, "WVC2070");

        const string Reservedˉintrinsic = """
            module Broken profile portable;
            fn Bytesˉlength(Value: i32) -> i32 { return Value; }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Reservedˉintrinsic, "WVC2024");

        const string Reservedˉenumˉname = """
            module Broken profile portable;
            fn Enumˉname(Value: i32) -> text { return "bad"; }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Reservedˉenumˉname, "WVC2024");

        const string Reservedˉrecordˉconstructor = """
            module Broken profile portable;
            record Bytesˉlength { Value: i32; }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Reservedˉrecordˉconstructor, "WVC2090");
    }

    private static void Operatorsˉrun()
    {
        const string Source = """
            module Operators profile portable;
            export fn Main() -> i32 {
                var Score: i32 = 0;
                let Seven: i32 = 10 - 3;
                if Seven == 7 { Score = Score + 1; }
                if Seven != 8 { Score = Score + 1; }
                if Seven <= 7 { Score = Score + 1; }
                if Seven > 6 { Score = Score + 1; }
                if Seven >= 7 { Score = Score + 1; }
                if -Seven < 0 { Score = Score + 1; }
                if true == true { Score = Score + 1; }
                if true != false { Score = Score + 1; }
                return Score;
            }
            """;
        Equal(8, Runˉportable(Source));
    }

    private static void Malformedˉmodulesˉareˉrejected()
    {
        var Valid = Compileˉsuccess(SUM_SOURCE);

        var Badˉmagic = (byte[])Valid.Clone();
        Badˉmagic[0] ^= 0xFF;
        Throwsˉbytecode("WVB1002", () => Moduleˉcodec.Readˉandˉverify(Badˉmagic));

        var Badˉversion = (byte[])Valid.Clone();
        Badˉversion[4] = 2;
        Throwsˉbytecode("WVB1003", () => Moduleˉcodec.Readˉandˉverify(Badˉversion));

        var Badˉsectionˉcount = (byte[])Valid.Clone();
        BinaryPrimitives.WriteUInt32LittleEndian(Badˉsectionˉcount.AsSpan(8), 5);
        Throwsˉbytecode("WVB1004", () => Moduleˉcodec.Readˉandˉverify(Badˉsectionˉcount));

        var Badˉflags = (byte[])Valid.Clone();
        Badˉflags[13] = 1;
        Throwsˉbytecode("WVB1009", () => Moduleˉcodec.Readˉandˉverify(Badˉflags));

        var Badˉutf8 = (byte[])Valid.Clone();
        var Moduleˉpayload = Findˉsectionˉpayload(Badˉutf8, Sectionˉkind.Module);
        Badˉutf8[Moduleˉpayload + 5] = 0xFF;
        Throwsˉbytecode("WVB1016", () => Moduleˉcodec.Readˉandˉverify(Badˉutf8));

        var Truncated = Valid[..^1];
        Throwsˉbytecode("WVB1018", () => Moduleˉcodec.Readˉandˉverify(Truncated));

        var Trailing = new byte[Valid.Length + 1];
        Valid.CopyTo(Trailing, 0);
        Throwsˉbytecode("WVB1017", () => Moduleˉcodec.Readˉandˉverify(Trailing));

        var Oversized = new byte[Bytecodeˉlimits.MAX_MODULE_BYTES + 1];
        Throwsˉbytecode("WVB1001", () => Moduleˉcodec.Readˉandˉverify(Oversized));

        var Badˉcount = Compileˉsuccess(HELLO_SOURCE);
        var Capabilityˉpayload = Findˉsectionˉpayload(Badˉcount, Sectionˉkind.Capabilities);
        BinaryPrimitives.WriteUInt32LittleEndian(Badˉcount.AsSpan(Capabilityˉpayload), uint.MaxValue);
        Throwsˉbytecode("WVB1011", () => Moduleˉcodec.Readˉandˉverify(Badˉcount));

        var Badˉtypeˉcount = Compileˉsuccess(SUM_SOURCE);
        var Typesˉpayload = Findˉsectionˉpayload(Badˉtypeˉcount, Sectionˉkind.Types);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Badˉtypeˉcount.AsSpan(Typesˉpayload),
            Bytecodeˉlimits.MAX_NOMINAL_TYPES + 1u);
        Throwsˉbytecode("WVB1012", () => Moduleˉcodec.Readˉandˉverify(Badˉtypeˉcount));

        const string Enumˉsource = """
            module Enumˉbinary profile portable;
            enum State { Ready = 0; }
            export fn Main() -> i32 { return 0; }
            """;
        var Badˉtypeˉkind = Compileˉsuccess(Enumˉsource);
        var Badˉtypeˉpayload = Findˉsectionˉpayload(Badˉtypeˉkind, Sectionˉkind.Types);
        Badˉtypeˉkind[Badˉtypeˉpayload + sizeof(uint)] = byte.MaxValue;
        Throwsˉbytecode("WVB1020", () => Moduleˉcodec.Readˉandˉverify(Badˉtypeˉkind));
    }

    private static void Unsafeˉbytecodeˉisˉrejected()
    {
        Throwsˉbytecode(
            "WVB2003",
            () => Moduleˉverifier.Verify(Buildˉmodule([0xFF], Valueˉtype.Void, maximumˉstack: 0)));

        Throwsˉbytecode(
            "WVB2006",
            () => Moduleˉverifier.Verify(Buildˉmodule([(byte)Opcode.I32ˉconst], Valueˉtype.I32, maximumˉstack: 1)));

        Throwsˉbytecode(
            "WVB2231",
            () => Moduleˉverifier.Verify(Buildˉmodule(
                [.. U32ˉinstruction(Opcode.Jump, 999)],
                Valueˉtype.Void,
                maximumˉstack: 0)));

        Throwsˉbytecode(
            "WVB2210",
            () => Moduleˉverifier.Verify(Buildˉmodule(
                [.. U32ˉinstruction(Opcode.Localˉload, 0), (byte)Opcode.Pop, (byte)Opcode.Return],
                Valueˉtype.Void,
                maximumˉstack: 1)));

        Throwsˉbytecode(
            "WVB2201",
            () => Moduleˉverifier.Verify(Buildˉmodule(
                [(byte)Opcode.Return, (byte)Opcode.Return],
                Valueˉtype.Void,
                maximumˉstack: 0)));

        var Mismatchedˉmerge = new List<byte>();
        Mismatchedˉmerge.AddRange(Boolˉinstruction(true));
        Mismatchedˉmerge.AddRange(U32ˉinstruction(Opcode.Branchˉfalse, 17));
        Mismatchedˉmerge.AddRange(I32ˉinstruction(1));
        Mismatchedˉmerge.AddRange(U32ˉinstruction(Opcode.Jump, 17));
        Mismatchedˉmerge.Add((byte)Opcode.Return);
        Throwsˉbytecode(
            "WVB2232",
            () => Moduleˉverifier.Verify(Buildˉmodule(
                [.. Mismatchedˉmerge],
                Valueˉtype.Void,
                maximumˉstack: 1)));

        Throwsˉbytecode(
            "WVB2202",
            () => Moduleˉverifier.Verify(Buildˉmodule(
                [.. I32ˉinstruction(1), (byte)Opcode.Pop, (byte)Opcode.Return],
                Valueˉtype.Void,
                maximumˉstack: 0)));

        Throwsˉbytecode(
            "WVB2220",
            () => Moduleˉverifier.Verify(Buildˉmodule(
                [.. I32ˉinstruction(0), (byte)Opcode.Bytesˉlength, (byte)Opcode.Pop, (byte)Opcode.Return],
                Valueˉtype.Void,
                maximumˉstack: 1)));

        var Invalidˉrecordˉshape = Buildˉmodule(
            [(byte)Opcode.Return],
            Valueˉtype.Void,
            maximumˉstack: 0) with
        {
            Functions = [new(
                "Main",
                [Valueˉshape.Forˉrecord(0)],
                Valueˉtype.Void,
                [],
                0,
                1,
                0)],
        };
        Throwsˉbytecode("WVB2242", () => Moduleˉverifier.Verify(Invalidˉrecordˉshape));

        ImmutableArray<Nominalˉtypeˉdeclaration> Oneˉu32ˉfield =
        [
            new Recordˉtypeˉdeclaration(
                "Pair",
                [new Recordˉfieldˉdeclaration("Value", Valueˉtype.U32)]),
        ];
        var Wrongˉrecordˉfieldˉtype = Buildˉmodule(
            [
                .. I32ˉinstruction(1),
                .. U32ˉinstruction(Opcode.Recordˉcreate, 0),
                (byte)Opcode.Pop,
                (byte)Opcode.Return,
            ],
            Valueˉtype.Void,
            maximumˉstack: 1) with
        {
            Types = Oneˉu32ˉfield,
        };
        Throwsˉbytecode("WVB2220", () => Moduleˉverifier.Verify(Wrongˉrecordˉfieldˉtype));

        var Fieldˉonˉprimitive = Buildˉmodule(
            [
                .. I32ˉinstruction(1),
                .. U32ˉinstruction(Opcode.Recordˉfield, 0),
                (byte)Opcode.Pop,
                (byte)Opcode.Return,
            ],
            Valueˉtype.Void,
            maximumˉstack: 1) with
        {
            Types = Oneˉu32ˉfield,
        };
        Throwsˉbytecode("WVB2222", () => Moduleˉverifier.Verify(Fieldˉonˉprimitive));

        var Invalidˉrecordˉfield = Buildˉmodule(
            [
                .. U32ˉinstruction(Opcode.U32ˉconst, 1),
                .. U32ˉinstruction(Opcode.Recordˉcreate, 0),
                .. U32ˉinstruction(Opcode.Recordˉfield, 1),
                (byte)Opcode.Pop,
                (byte)Opcode.Return,
            ],
            Valueˉtype.Void,
            maximumˉstack: 1) with
        {
            Types = Oneˉu32ˉfield,
        };
        Throwsˉbytecode("WVB2223", () => Moduleˉverifier.Verify(Invalidˉrecordˉfield));

        var Duplicateˉrecordˉmetadata = Buildˉmodule(
            [(byte)Opcode.Return],
            Valueˉtype.Void,
            maximumˉstack: 0) with
        {
            Types = [new Recordˉtypeˉdeclaration(
                "Pair",
                [new("Value", Valueˉtype.I32), new("Value", Valueˉtype.U32)])],
        };
        Throwsˉbytecode("WVB2152", () => Moduleˉverifier.Verify(Duplicateˉrecordˉmetadata));

        ImmutableArray<Nominalˉtypeˉdeclaration> Oneˉenum =
        [
            new Enumˉtypeˉdeclaration(
                "State",
                [new("Ready", 0), new("Failed", 1)]),
        ];
        var Invalidˉenumˉmember = Buildˉmodule(
            [
                .. Twoˉu32ˉinstruction(Opcode.Enumˉconst, 0, 2),
                (byte)Opcode.Pop,
                (byte)Opcode.Return,
            ],
            Valueˉtype.Void,
            maximumˉstack: 1) with
        {
            Types = Oneˉenum,
        };
        Throwsˉbytecode("WVB2225", () => Moduleˉverifier.Verify(Invalidˉenumˉmember));

        var Enumˉconstantˉonˉrecord = Buildˉmodule(
            [
                .. Twoˉu32ˉinstruction(Opcode.Enumˉconst, 0, 0),
                (byte)Opcode.Pop,
                (byte)Opcode.Return,
            ],
            Valueˉtype.Void,
            maximumˉstack: 1) with
        {
            Types = Oneˉu32ˉfield,
        };
        Throwsˉbytecode("WVB2217", () => Moduleˉverifier.Verify(Enumˉconstantˉonˉrecord));

        var Enumˉnameˉonˉprimitive = Buildˉmodule(
            [
                .. I32ˉinstruction(0),
                (byte)Opcode.Enumˉname,
                (byte)Opcode.Pop,
                (byte)Opcode.Return,
            ],
            Valueˉtype.Void,
            maximumˉstack: 1) with
        {
            Types = Oneˉenum,
        };
        Throwsˉbytecode("WVB2226", () => Moduleˉverifier.Verify(Enumˉnameˉonˉprimitive));

        ImmutableArray<Nominalˉtypeˉdeclaration> Twoˉenums =
        [
            new Enumˉtypeˉdeclaration("First", [new("Value", 0)]),
            new Enumˉtypeˉdeclaration("Second", [new("Value", 0)]),
        ];
        var Mismatchedˉenumˉcomparison = Buildˉmodule(
            [
                .. Twoˉu32ˉinstruction(Opcode.Enumˉconst, 0, 0),
                .. Twoˉu32ˉinstruction(Opcode.Enumˉconst, 1, 0),
                (byte)Opcode.Enumˉequal,
                (byte)Opcode.Pop,
                (byte)Opcode.Return,
            ],
            Valueˉtype.Void,
            maximumˉstack: 2) with
        {
            Types = Twoˉenums,
        };
        Throwsˉbytecode("WVB2224", () => Moduleˉverifier.Verify(Mismatchedˉenumˉcomparison));

        var Wrongˉnominalˉkind = Buildˉmodule(
            [(byte)Opcode.Return],
            Valueˉtype.Void,
            maximumˉstack: 0) with
        {
            Types = Oneˉenum,
            Functions = [new(
                "Main",
                [Valueˉshape.Forˉrecord(0)],
                Valueˉtype.Void,
                [],
                0,
                1,
                0)],
        };
        Throwsˉbytecode("WVB2244", () => Moduleˉverifier.Verify(Wrongˉnominalˉkind));

        var Duplicateˉenumˉmetadata = Buildˉmodule(
            [(byte)Opcode.Return],
            Valueˉtype.Void,
            maximumˉstack: 0) with
        {
            Types = [new Enumˉtypeˉdeclaration(
                "State",
                [new("Ready", 0), new("Failed", 0)])],
        };
        Throwsˉbytecode("WVB2156", () => Moduleˉverifier.Verify(Duplicateˉenumˉmetadata));

        var Duplicateˉnominalˉname = Buildˉmodule(
            [(byte)Opcode.Return],
            Valueˉtype.Void,
            maximumˉstack: 0) with
        {
            Types =
            [
                new Recordˉtypeˉdeclaration("Same", [new("Value", Valueˉtype.I32)]),
                new Enumˉtypeˉdeclaration("Same", [new("Value", 0)]),
            ],
        };
        Throwsˉbytecode("WVB2159", () => Moduleˉverifier.Verify(Duplicateˉnominalˉname));

        var Oversizedˉbyteˉdata = Buildˉmodule(
            [(byte)Opcode.Return],
            Valueˉtype.Void,
            maximumˉstack: 0) with
        {
            Data = [new Bytesˉdataˉdeclaration(
                "Oversizedˉbytes",
                ImmutableArray.Create<byte>(new byte[Bytecodeˉlimits.MAX_BYTE_DATA_BYTES + 1]))],
        };
        Throwsˉbytecode("WVB2125", () => Moduleˉverifier.Verify(Oversizedˉbyteˉdata));

        var Invalidˉtext = new Textˉdataˉdeclaration("Text", "\uD800");
        Throwsˉbytecode(
            "WVB2124",
            () => Moduleˉverifier.Verify(new(
                "Invalidˉtext",
                Moduleˉprofile.Portable,
                [],
                [Invalidˉtext],
                [new("Main", [], Valueˉtype.Void, [], 0, 1, 0)],
                [(byte)Opcode.Return],
                [new("Main", Exportˉkind.Function, 0)])));
    }

    private static void Runtimeˉtrapsˉareˉdeterministic()
    {
        const string Overflow = """
            module Overflow profile portable;
            export fn Main() -> i32 { return 2147483647 + 1; }
            """;
        Throwsˉruntime("WVR3007", () => Runˉportable(Overflow));

        const string Bounds = """
            module Bounds profile portable;
            data Values: [i32] = [1];
            export fn Main() -> i32 { return Values[2]; }
            """;
        Throwsˉruntime("WVR3005", () => Runˉportable(Bounds));

        const string Byteˉbounds = """
            module Byteˉbounds profile portable;
            data Values: bytes = [1, 2, 3];
            export fn Main() -> i32 {
                Bytesˉreadˉu32ˉlittle(Values, 0u32);
                return 0;
            }
            """;
        Throwsˉruntime("WVR3008", () => Runˉportable(Byteˉbounds));

        const string Sliceˉbounds = """
            module Sliceˉbounds profile portable;
            data Values: bytes = [1, 2, 3];
            export fn Main() -> i32 {
                Bytesˉslice(Values, 2u32, 2u32);
                return 0;
            }
            """;
        Throwsˉruntime("WVR3008", () => Runˉportable(Sliceˉbounds));

        const string U32ˉoverflow = """
            module U32ˉoverflow profile portable;
            export fn Main() -> i32 {
                4294967295u32 + 1u32;
                return 0;
            }
            """;
        Throwsˉruntime("WVR3007", () => Runˉportable(U32ˉoverflow));

        const string U32ˉunderflow = """
            module U32ˉunderflow profile portable;
            export fn Main() -> i32 {
                0u32 - 1u32;
                return 0;
            }
            """;
        Throwsˉruntime("WVR3007", () => Runˉportable(U32ˉunderflow));

        var Oversizedˉtextˉresult = Buildˉmodule(
            [
                .. U32ˉinstruction(Opcode.Textˉconst, 0),
                .. U32ˉinstruction(Opcode.Textˉconst, 1),
                (byte)Opcode.Textˉconcat,
                (byte)Opcode.Pop,
                .. I32ˉinstruction(0),
                (byte)Opcode.Return,
            ],
            Valueˉtype.I32,
            maximumˉstack: 2) with
        {
            Data =
            [
                new Textˉdataˉdeclaration("Left", new string('a', 600_000)),
                new Textˉdataˉdeclaration("Right", new string('b', 600_000)),
            ],
        };
        var Verifiedˉoversizedˉtext = Moduleˉverifier.Verify(Oversizedˉtextˉresult);
        Throwsˉruntime(
            "WVR3012",
            () => new Referenceˉruntime(
                Verifiedˉoversizedˉtext,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain());

        var Oversizedˉquoteˉresult = Buildˉmodule(
            [
                .. U32ˉinstruction(Opcode.Textˉconst, 0),
                (byte)Opcode.Textˉquote,
                (byte)Opcode.Pop,
                .. I32ˉinstruction(0),
                (byte)Opcode.Return,
            ],
            Valueˉtype.I32,
            maximumˉstack: 1) with
        {
            Data = [new Textˉdataˉdeclaration("Quoted", new string('\u0100', 200_000))],
        };
        var Verifiedˉoversizedˉquote = Moduleˉverifier.Verify(Oversizedˉquoteˉresult);
        Throwsˉruntime(
            "WVR3012",
            () => new Referenceˉruntime(
                Verifiedˉoversizedˉquote,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain());

        var Oversizedˉdecodeˉresult = Buildˉmodule(
            [
                .. U32ˉinstruction(Opcode.Bytesˉconst, 0),
                (byte)Opcode.Textˉfromˉutf8,
                (byte)Opcode.Pop,
                .. I32ˉinstruction(0),
                (byte)Opcode.Return,
            ],
            Valueˉtype.I32,
            maximumˉstack: 1) with
        {
            Data =
            [
                new Bytesˉdataˉdeclaration(
                    "Encoded",
                    ImmutableArray.Create<byte>(new byte[Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES + 1])),
            ],
        };
        var Verifiedˉoversizedˉdecode = Moduleˉverifier.Verify(Oversizedˉdecodeˉresult);
        Throwsˉruntime(
            "WVR3012",
            () => new Referenceˉruntime(
                Verifiedˉoversizedˉdecode,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain());

        var Oversizedˉbytesˉresult = Buildˉmodule(
            [
                .. U32ˉinstruction(Opcode.Bytesˉconst, 0),
                .. U32ˉinstruction(Opcode.Bytesˉconst, 1),
                (byte)Opcode.Bytesˉconcat,
                (byte)Opcode.Pop,
                .. I32ˉinstruction(0),
                (byte)Opcode.Return,
            ],
            Valueˉtype.I32,
            maximumˉstack: 2) with
        {
            Data =
            [
                new Bytesˉdataˉdeclaration("Left", ImmutableArray.Create<byte>(new byte[3_000_000])),
                new Bytesˉdataˉdeclaration("Right", ImmutableArray.Create<byte>(new byte[3_000_000])),
            ],
        };
        var Verifiedˉoversizedˉbytes = Moduleˉverifier.Verify(Oversizedˉbytesˉresult);
        Throwsˉruntime(
            "WVR3015",
            () => new Referenceˉruntime(
                Verifiedˉoversizedˉbytes,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain());
    }

    private static void Runtimeˉlimitsˉareˉenforced()
    {
        var Sumˉmodule = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(SUM_SOURCE));
        var Limited = new Referenceˉruntime(
            Sumˉmodule,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities, Maximumˉinstructions: 5));
        Throwsˉruntime("WVR3011", () => _ = Limited.Runˉmain());

        const string Recursion = """
            module Recursion profile portable;
            fn Recurse(Value: i32) -> i32 { return Recurse(Value + 1); }
            export fn Main() -> i32 { return Recurse(0); }
            """;
        var Recursiveˉmodule = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Recursion));
        var Depthˉlimited = new Referenceˉruntime(
            Recursiveˉmodule,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities, Maximumˉcallˉdepth: 8));
        Throwsˉruntime("WVR3004", () => _ = Depthˉlimited.Runˉmain());
    }

    private static void Goldenˉhashesˉmatch()
    {
        var Sumˉbytes = Compileˉsuccess(SUM_SOURCE);
        var Helloˉbytes = Compileˉsuccess(HELLO_SOURCE);
        var Foundationˉbytes = Compileˉsuccess(FOUNDATION_SOURCE);
        var Wvˉdumpˉbytes = Compileˉsuccess(WVDUMP_CORE_SOURCE);
        var Wvoˉcoreˉbytes = Compileˉsuccess(WVO_CORE_SOURCE);
        var Wvoˉsampleˉbytes = Objectˉcodec.Write(Buildˉsampleˉobject());
        var Sumˉhash = Moduleˉdigest.Calculateˉsha256(Sumˉbytes);
        var Helloˉhash = Moduleˉdigest.Calculateˉsha256(Helloˉbytes);
        var Foundationˉhash = Moduleˉdigest.Calculateˉsha256(Foundationˉbytes);
        var Wvˉdumpˉhash = Moduleˉdigest.Calculateˉsha256(Wvˉdumpˉbytes);
        var Wvoˉcoreˉhash = Moduleˉdigest.Calculateˉsha256(Wvoˉcoreˉbytes);
        var Wvoˉsampleˉhash = Objectˉdigest.Calculateˉsha256(Wvoˉsampleˉbytes);
        Equal(SUM_SHA256, Sumˉhash);
        Equal(HELLO_SHA256, Helloˉhash);
        Equal(FOUNDATION_SHA256, Foundationˉhash);
        Equal(WVDUMP_CORE_SHA256, Wvˉdumpˉhash);
        Equal(WVO_CORE_SHA256, Wvoˉcoreˉhash);
        Equal(WVO_SAMPLE_SHA256, Wvoˉsampleˉhash);

        var Sumˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Sumˉbytes),
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        var Helloˉoutput = new StringWriter();
        var Helloˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Helloˉbytes),
            new Referenceˉcapabilityˉhost(Helloˉoutput),
            new(ImmutableHashSet.Create(StringComparer.Ordinal, Capabilityˉcatalog.CONSOLE_WRITE_LINE)))
            .Runˉmain();
        var Normalizedˉhelloˉoutput = Helloˉoutput.ToString()
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        var Foundationˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Foundationˉbytes),
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        var Wvˉdumpˉmodule = Moduleˉcodec.Readˉandˉverify(Wvˉdumpˉbytes);
        var Wvˉdumpˉcapabilities = Wvˉdumpˉmodule.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Wvˉdumpˉresult = new Referenceˉruntime(
            Wvˉdumpˉmodule,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                TextWriter.Null,
                new Testˉfileˉreader((_, _) => throw new InvalidOperationException(
                    "The golden WvDump self-test must not read a hosted file.")))),
            new(Wvˉdumpˉcapabilities)).Runˉmain();
        var Wvˉdumpˉhostedˉoutput = new StringWriter();
        var Wvˉdumpˉhostedˉdiagnostics = new StringWriter();
        var Wvˉdumpˉhostedˉresult = new Referenceˉruntime(
            Wvˉdumpˉmodule,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["sum.wvb"],
                Wvˉdumpˉhostedˉoutput,
                Wvˉdumpˉhostedˉdiagnostics,
                new Testˉfileˉreader((Name, Maximumˉbytes) =>
                {
                    Equal("sum.wvb", Name);
                    True(Sumˉbytes.Length <= Maximumˉbytes, "The golden WvDump byte limit was too small.");
                    return Sumˉbytes.ToImmutableArray();
                }))),
            new(Wvˉdumpˉcapabilities, Maximumˉinstructions: 10_000_000)).Runˉmain();
        var Normalizedˉwvdumpˉoutput = Wvˉdumpˉhostedˉoutput.ToString()
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        var Wvoˉmodule = Moduleˉcodec.Readˉandˉverify(Wvoˉcoreˉbytes);
        var Wvoˉcapabilities = Wvoˉmodule.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Wvoˉselfˉtestˉwriter = new Capturingˉfileˉwriter();
        var Wvoˉselfˉtestˉresult = new Referenceˉruntime(
            Wvoˉmodule,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                TextWriter.Null,
                null,
                Wvoˉselfˉtestˉwriter)),
            new(Wvoˉcapabilities, Maximumˉinstructions: 10_000_000)).Runˉmain();
        var Wvoˉhostedˉwriter = new Capturingˉfileˉwriter();
        var Wvoˉhostedˉoutput = new StringWriter();
        var Wvoˉhostedˉdiagnostics = new StringWriter();
        var Wvoˉhostedˉresult = new Referenceˉruntime(
            Wvoˉmodule,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["sample.wvo"],
                Wvoˉhostedˉoutput,
                Wvoˉhostedˉdiagnostics,
                null,
                Wvoˉhostedˉwriter)),
            new(Wvoˉcapabilities, Maximumˉinstructions: 10_000_000)).Runˉmain();
        var Normalizedˉwvoˉoutput = Wvoˉhostedˉoutput.ToString()
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        Equal(29, Sumˉresult.Exitˉcode);
        Equal("Hello from Windvale\n", Normalizedˉhelloˉoutput);
        Equal(0, Helloˉresult.Exitˉcode);
        Equal(1, Foundationˉresult.Exitˉcode);
        Equal(0, Wvˉdumpˉresult.Exitˉcode);
        Equal(0, Wvˉdumpˉhostedˉresult.Exitˉcode);
        Equal(string.Empty, Wvˉdumpˉhostedˉdiagnostics.ToString());
        Contains(Normalizedˉwvdumpˉoutput, "module version=1.5 profile=portable name=\"Sum\\u02C9data\"");
        Contains(Normalizedˉwvdumpˉoutput, "instruction function=1 offset=141 opcode=call operand=0");
        Contains(Normalizedˉwvdumpˉoutput, "export index=0 name=\"Main\" kind=function target=1");
        Equal(0, Wvoˉselfˉtestˉresult.Exitˉcode);
        Equal(0, Wvoˉselfˉtestˉwriter.Writeˉcount);
        Equal(0, Wvoˉhostedˉresult.Exitˉcode);
        Equal("Wrote WVO 1.0 bytes=189\n", Normalizedˉwvoˉoutput);
        Equal(string.Empty, Wvoˉhostedˉdiagnostics.ToString());
        Sequenceˉequal(Wvoˉsampleˉbytes, Wvoˉhostedˉwriter.Bytes);
        Contract = new(
            $"{Moduleˉcodec.MAJOR_VERSION}.{Moduleˉcodec.MINOR_VERSION}",
            $"{Objectˉcodec.MAJOR_VERSION}.{Objectˉcodec.MINOR_VERSION}",
            Sumˉhash,
            Sumˉresult.Exitˉcode,
            Helloˉhash,
            Normalizedˉhelloˉoutput,
            Helloˉresult.Exitˉcode,
            Foundationˉhash,
            Foundationˉresult.Exitˉcode,
            Wvˉdumpˉhash,
            Wvˉdumpˉresult.Exitˉcode,
            Normalizedˉwvdumpˉoutput,
            Wvoˉsampleˉhash,
            Wvoˉcoreˉhash,
            Wvoˉselfˉtestˉresult.Exitˉcode,
            Normalizedˉwvoˉoutput);
    }

    private static void Randomˉinputˉisˉcontained()
    {
        const string Sourceˉalphabet =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789" +
            "{}[]();:,.+-*!<>=_ˉ \t\r\n\\\"";
        var Random = new Random(0x57_56_42);
        for (var Case = 0; Case < 500; Case++)
        {
            var Length = Random.Next(0, 512);
            var Characters = new char[Length];
            for (var Index = 0; Index < Characters.Length; Index++)
            {
                Characters[Index] = Sourceˉalphabet[Random.Next(Sourceˉalphabet.Length)];
            }

            _ = Seedˉcompiler.Compile(new string(Characters), $"fuzz-{Case}.wv");
        }

        for (var Case = 0; Case < 1000; Case++)
        {
            var Bytes = new byte[Random.Next(0, 512)];
            Random.NextBytes(Bytes);
            try
            {
                _ = Moduleˉcodec.Readˉandˉverify(Bytes);
            }
            catch (Bytecodeˉexception)
            {
                // Rejection through the stable bytecode boundary is the expected result.
            }
        }

        for (var Case = 0; Case < 500; Case++)
        {
            var Bytes = new byte[Random.Next(0, 512)];
            Random.NextBytes(Bytes);
            try
            {
                _ = Objectˉcodec.Readˉandˉverify(Bytes);
            }
            catch (Objectˉexception)
            {
                // Rejection through the stable object boundary is the expected result.
            }
        }
    }

    private static int Runˉportable(string source)
    {
        var Module = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(source));
        return new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode;
    }

    private static byte[] Compileˉsuccess(string source)
    {
        var Result = Seedˉcompiler.Compile(source);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Compilation failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static void Hasˉdiagnostic(string source, string code)
    {
        var Result = Seedˉcompiler.Compile(source);
        False(Result.Success, $"Source expected to produce {code} compiled successfully.");
        True(Result.Diagnostics.Any(Diagnostic => Diagnostic.Code == code),
            $"Expected diagnostic {code}; found {string.Join(", ", Result.Diagnostics.Select(Item => Item.Code))}.");
    }

    private static Objectˉfile Buildˉsampleˉobject()
    {
        return new(
            Objectˉarchitecture.X86ˉ64,
            [
                new(".text", Objectˉsectionˉkind.Code, 16, 6, [232, 0, 0, 0, 0, 195]),
                new(".rodata", Objectˉsectionˉkind.Readˉonlyˉdata, 1, 3, [72, 105, 10]),
            ],
            [
                new("Message", Objectˉsymbolˉbinding.Local, Objectˉsymbolˉkind.Data, 1, 0, 3),
                new("Main", Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, 0, 6),
                new(
                    "Console_write",
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
            ],
            [new(Objectˉrelocationˉkind.Relativeˉi32, 0, 1, 2, -4)]);
    }

    private static Bytecodeˉmodule Buildˉmodule(
        ImmutableArray<byte> code,
        Valueˉtype returnˉtype,
        int maximumˉstack)
    {
        return new(
            "Verifierˉcase",
            Moduleˉprofile.Portable,
            [],
            [],
            [new("Main", [], returnˉtype, [], 0, code.Length, maximumˉstack)],
            code,
            [new("Main", Exportˉkind.Function, 0)]);
    }

    private static byte[] I32ˉinstruction(int value)
    {
        var Result = new byte[5];
        Result[0] = (byte)Opcode.I32ˉconst;
        BinaryPrimitives.WriteInt32LittleEndian(Result.AsSpan(1), value);
        return Result;
    }

    private static byte[] Boolˉinstruction(bool value)
    {
        return [(byte)Opcode.Boolˉconst, value ? (byte)1 : (byte)0];
    }

    private static byte[] U32ˉinstruction(Opcode opcode, uint value)
    {
        var Result = new byte[5];
        Result[0] = (byte)opcode;
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(1), value);
        return Result;
    }

    private static byte[] Twoˉu32ˉinstruction(Opcode opcode, uint first, uint second)
    {
        var Result = new byte[9];
        Result[0] = (byte)opcode;
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(1), first);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(5), second);
        return Result;
    }

    private static int Findˉsectionˉpayload(byte[] bytes, Sectionˉkind kind)
    {
        var Offset = 12;
        for (var Index = 0; Index < Bytecodeˉlimits.SECTION_COUNT; Index++)
        {
            var Currentˉkind = (Sectionˉkind)bytes[Offset];
            var Length = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(Offset + 4)));
            if (Currentˉkind == kind)
            {
                return Offset + 8;
            }

            Offset = checked(Offset + 8 + Length);
        }

        throw new InvalidOperationException($"Section '{kind}' was not found.");
    }

    private static void Throwsˉbytecode(string expectedˉcode, Action action)
    {
        try
        {
            action();
        }
        catch (Bytecodeˉexception Exception)
        {
            Equal(expectedˉcode, Exception.Code);
            return;
        }

        throw new InvalidOperationException($"Expected bytecode failure {expectedˉcode}.");
    }

    private static void Throwsˉobject(string expectedˉcode, Action action)
    {
        try
        {
            action();
        }
        catch (Objectˉexception Exception)
        {
            Equal(expectedˉcode, Exception.Code);
            return;
        }

        throw new InvalidOperationException($"Expected object failure {expectedˉcode}.");
    }

    private static void Throwsˉruntime(string expectedˉcode, Action action)
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

        throw new InvalidOperationException($"Expected runtime failure {expectedˉcode}.");
    }

    private static void Writeˉreport(string path)
    {
        var Report = new Conformanceˉreport(
            Contract ?? throw new InvalidOperationException("The golden contract test did not run."),
            new(
                Getˉosˉfamily(),
                RuntimeInformation.OSDescription,
                RuntimeInformation.OSArchitecture.ToString(),
                RuntimeInformation.FrameworkDescription));
        var Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };
        var Fullˉpath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(Fullˉpath)!);
        File.WriteAllText(Fullˉpath, JsonSerializer.Serialize(Report, Options) + Environment.NewLine);
        Console.WriteLine($"Conformance report: {Fullˉpath}");
    }

    private static int Compareˉreports(string firstˉpath, string secondˉpath)
    {
        var Options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var First = JsonSerializer.Deserialize<Conformanceˉreport>(File.ReadAllText(firstˉpath), Options)
            ?? throw new InvalidOperationException("The first report is invalid.");
        var Second = JsonSerializer.Deserialize<Conformanceˉreport>(File.ReadAllText(secondˉpath), Options)
            ?? throw new InvalidOperationException("The second report is invalid.");
        if (First.Contract != Second.Contract)
        {
            Console.Error.WriteLine("Cross-host conformance contracts differ.");
            Console.Error.WriteLine($"First:  {JsonSerializer.Serialize(First.Contract)}");
            Console.Error.WriteLine($"Second: {JsonSerializer.Serialize(Second.Contract)}");
            return 1;
        }

        var Hostˉfamilies = new HashSet<string>(StringComparer.Ordinal)
        {
            First.Host.Operatingˉsystemˉfamily,
            Second.Host.Operatingˉsystemˉfamily,
        };
        if (!Hostˉfamilies.SetEquals(["windows", "linux"]))
        {
            Console.Error.WriteLine(
                "Cross-host comparison requires one Windows report and one Linux report.");
            return 1;
        }

        Console.WriteLine("Cross-host conformance contracts match.");
        Console.WriteLine($"First host:  {First.Host.Operatingˉsystem} / {First.Host.Architecture}");
        Console.WriteLine($"Second host: {Second.Host.Operatingˉsystem} / {Second.Host.Architecture}");
        return 0;
    }

    private static string Getˉosˉfamily()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "windows";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "linux";
        }

        return "other";
    }

    private static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"Expected '{expected}', found '{actual}'.");
        }
    }

    private static void Sequenceˉequal<T>(IEnumerable<T> expected, IEnumerable<T> actual)
    {
        if (!expected.SequenceEqual(actual))
        {
            throw new InvalidOperationException(
                $"Sequences differ. Expected [{string.Join(", ", expected)}], " +
                $"found [{string.Join(", ", actual)}].");
        }
    }

    private static void Contains(string value, string expectedˉfragment)
    {
        if (!value.Contains(expectedˉfragment, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Text does not contain '{expectedˉfragment}'.");
        }
    }

    private static string Readˉembeddedˉsource(string name)
    {
        using var Stream = typeof(Program).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded source '{name}' was not found.");
        using var Reader = new StreamReader(Stream);
        return Reader.ReadToEnd();
    }

    private static void True(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void False(bool condition, string message)
    {
        True(!condition, message);
    }

    private sealed class Testˉfileˉreader(
        Func<string, int, ImmutableArray<byte>> read) : IHostedˉfileˉreader
    {
        public ImmutableArray<byte> Readˉbytes(string resourceˉname, int maximumˉbytes)
        {
            return read(resourceˉname, maximumˉbytes);
        }
    }

    private sealed class Capturingˉfileˉwriter : IHostedˉfileˉwriter
    {
        public int Writeˉcount { get; private set; }

        public string Resourceˉname { get; private set; } = string.Empty;

        public ImmutableArray<byte> Bytes { get; private set; } = [];

        public void Writeˉbytes(
            string resourceˉname,
            ImmutableArray<byte> bytes,
            int maximumˉbytes)
        {
            if (bytes.IsDefault || bytes.Length > maximumˉbytes)
            {
                throw new InvalidOperationException("The runtime passed invalid bytes to the hosted writer.");
            }
            Writeˉcount++;
            Resourceˉname = resourceˉname;
            Bytes = bytes;
        }
    }

    private sealed class Invalidˉresultˉcapabilityˉhost : ICapabilityˉhost
    {
        public bool Supports(string capabilityˉname) => true;

        public Runtimeˉvalue? Invoke(
            Capabilityˉdeclaration capability,
            ImmutableArray<Runtimeˉvalue> arguments)
        {
            return Runtimeˉvalue.Fromˉi32(1);
        }
    }

    private sealed record Conformanceˉcontract(
        [property: JsonPropertyName("moduleFormat")] string Moduleˉformat,
        [property: JsonPropertyName("objectFormat")] string Objectˉformat,
        [property: JsonPropertyName("sumSha256")] string Sumˉsha256,
        [property: JsonPropertyName("sumResult")] int Sumˉresult,
        [property: JsonPropertyName("helloSha256")] string Helloˉsha256,
        [property: JsonPropertyName("helloOutput")] string Helloˉoutput,
        [property: JsonPropertyName("helloResult")] int Helloˉresult,
        [property: JsonPropertyName("foundationSha256")] string Foundationˉsha256,
        [property: JsonPropertyName("foundationResult")] int Foundationˉresult,
        [property: JsonPropertyName("wvdumpCoreSha256")] string Wvˉdumpˉcoreˉsha256,
        [property: JsonPropertyName("wvdumpCoreResult")] int Wvˉdumpˉcoreˉresult,
        [property: JsonPropertyName("wvdumpHostedOutput")] string Wvˉdumpˉhostedˉoutput,
        [property: JsonPropertyName("wvoSampleSha256")] string Wvoˉsampleˉsha256,
        [property: JsonPropertyName("wvoCoreSha256")] string Wvoˉcoreˉsha256,
        [property: JsonPropertyName("wvoCoreResult")] int Wvoˉcoreˉresult,
        [property: JsonPropertyName("wvoHostedOutput")] string Wvoˉhostedˉoutput);

    private sealed record Hostˉreport(
        [property: JsonPropertyName("operatingSystemFamily")] string Operatingˉsystemˉfamily,
        [property: JsonPropertyName("operatingSystem")] string Operatingˉsystem,
        [property: JsonPropertyName("architecture")] string Architecture,
        [property: JsonPropertyName("framework")] string Framework);

    private sealed record Conformanceˉreport(
        [property: JsonPropertyName("contract")] Conformanceˉcontract Contract,
        [property: JsonPropertyName("host")] Hostˉreport Host);
}
