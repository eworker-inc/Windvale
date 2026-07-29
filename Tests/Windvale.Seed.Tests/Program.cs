using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Runtime;

namespace Windvale.Seed.Tests;

internal static class Program
{
    private const string SUM_SHA256 = "faf44208d41c852f575e4f3025b0722c8fe6ee2d1c1a55b71b9e109c3eb54ef2";
    private const string HELLO_SHA256 = "fafbc14e7e82626bcfacf358f777c1b6ce6821a335677a35148da9f857eefed5";

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

    private static readonly List<(string Name, Action Body)> TESTS =
    [
        ("portable source compiles, verifies, and returns the data sum", Portableˉprogramˉruns),
        ("hosted source requires authorization and writes text", Hostedˉprogramˉruns),
        ("compiler output is deterministic and canonical", Compilerˉisˉdeterministic),
        ("module codec round-trips exact canonical bytes", Moduleˉroundˉtrip),
        ("inspector exposes module metadata and disassembly", Inspectorˉisˉuseful),
        ("bool, if, text literals, and calls execute", Additionalˉsemanticsˉrun),
        ("macron names and explicit local mutability execute", Namingˉandˉmutabilityˉrun),
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
        Equal($"Hello from Windvale{Environment.NewLine}", Output.ToString());
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
        Equal($"answer{Environment.NewLine}", Output.ToString());
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
        var Sumˉhash = Moduleˉdigest.Calculateˉsha256(Sumˉbytes);
        var Helloˉhash = Moduleˉdigest.Calculateˉsha256(Helloˉbytes);
        Equal(SUM_SHA256, Sumˉhash);
        Equal(HELLO_SHA256, Helloˉhash);

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
        Equal(29, Sumˉresult.Exitˉcode);
        Equal("Hello from Windvale\n", Normalizedˉhelloˉoutput);
        Equal(0, Helloˉresult.Exitˉcode);
        Contract = new(
            $"{Moduleˉcodec.MAJOR_VERSION}.{Moduleˉcodec.MINOR_VERSION}",
            Sumˉhash,
            Sumˉresult.Exitˉcode,
            Helloˉhash,
            Normalizedˉhelloˉoutput,
            Helloˉresult.Exitˉcode);
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

    private sealed record Conformanceˉcontract(
        [property: JsonPropertyName("moduleFormat")] string Moduleˉformat,
        [property: JsonPropertyName("sumSha256")] string Sumˉsha256,
        [property: JsonPropertyName("sumResult")] int Sumˉresult,
        [property: JsonPropertyName("helloSha256")] string Helloˉsha256,
        [property: JsonPropertyName("helloOutput")] string Helloˉoutput,
        [property: JsonPropertyName("helloResult")] int Helloˉresult);

    private sealed record Hostˉreport(
        [property: JsonPropertyName("operatingSystemFamily")] string Operatingˉsystemˉfamily,
        [property: JsonPropertyName("operatingSystem")] string Operatingˉsystem,
        [property: JsonPropertyName("architecture")] string Architecture,
        [property: JsonPropertyName("framework")] string Framework);

    private sealed record Conformanceˉreport(
        [property: JsonPropertyName("contract")] Conformanceˉcontract Contract,
        [property: JsonPropertyName("host")] Hostˉreport Host);
}
