using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Windvale.Assembler;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime;

namespace Windvale.Tool;

internal static class Program
{
    private const int EXIT_SUCCESS = 0;
    private const int EXIT_COMPILATION = 1;
    private const int EXIT_VERIFICATION = 2;
    private const int EXIT_RUNTIME = 3;
    private const int EXIT_USAGE = 64;
    private const int EXIT_SOFTWARE = 70;
    private const int EXIT_IO = 74;
    private const int MAX_SOURCE_FILE_BYTES = 16 * 1024 * 1024;
    private const long MAX_SOURCE_SET_FILE_BYTES = 64L * 1024 * 1024;

    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public static int Main(string[] arguments)
    {
        try
        {
            if (arguments.Length == 0 || arguments[0] is "help" or "--help" or "-h")
            {
                Writeˉhelp(Console.Out);
                return EXIT_SUCCESS;
            }

            return arguments[0] switch
            {
                "compile" => Compile(arguments[1..]),
                "assemble" => Assemble(arguments[1..]),
                "link" => Link(arguments[1..]),
                "inspect" => Inspect(arguments[1..]),
                "verify" => Verify(arguments[1..]),
                "object-inspect" => Inspectˉobject(arguments[1..]),
                "object-verify" => Verifyˉobject(arguments[1..]),
                "run" => Run(arguments[1..]),
                _ => Usageˉerror($"Unknown command '{arguments[0]}'."),
            };
        }
        catch (Moduleˉformatˉexception Exception)
        {
            Console.Error.WriteLine(Exception.Message);
            return EXIT_VERIFICATION;
        }
        catch (Moduleˉverificationˉexception Exception)
        {
            Console.Error.WriteLine(Exception.Message);
            return EXIT_VERIFICATION;
        }
        catch (Objectˉexception Exception)
        {
            Console.Error.WriteLine(Exception.Message);
            return EXIT_VERIFICATION;
        }
        catch (Runtimeˉexception Exception)
        {
            Console.Error.WriteLine(Exception.Message);
            return EXIT_RUNTIME;
        }
        catch (DecoderFallbackException Exception)
        {
            Console.Error.WriteLine($"Input is not strict UTF-8: {Exception.Message}");
            return EXIT_IO;
        }
        catch (IOException Exception)
        {
            Console.Error.WriteLine($"I/O failed: {Exception.Message}");
            return EXIT_IO;
        }
        catch (UnauthorizedAccessException Exception)
        {
            Console.Error.WriteLine($"I/O access was denied: {Exception.Message}");
            return EXIT_IO;
        }
        catch (Exception Exception)
        {
            Console.Error.WriteLine($"Internal failure: {Exception.Message}");
            return EXIT_SOFTWARE;
        }
    }

    private static int Compile(string[] arguments)
    {
        const string Usage =
            "Usage: windvale compile <source.wv> [--module <dependency.wv>]... [-o <module.wvb>]";
        if (arguments.Length == 0 || arguments[0].StartsWith("-", StringComparison.Ordinal))
        {
            return Usageˉerror(Usage);
        }

        var Sourceˉpath = Path.GetFullPath(arguments[0]);
        var Dependencyˉpaths = new List<string>();
        string? Requestedˉoutputˉpath = null;
        for (var Index = 1; Index < arguments.Length; Index += 2)
        {
            if (Index + 1 >= arguments.Length)
            {
                return Usageˉerror(Usage);
            }
            if (arguments[Index] == "--module")
            {
                Dependencyˉpaths.Add(Path.GetFullPath(arguments[Index + 1]));
                continue;
            }
            if (arguments[Index] == "-o" && Requestedˉoutputˉpath is null)
            {
                Requestedˉoutputˉpath = Path.GetFullPath(arguments[Index + 1]);
                continue;
            }

            return Usageˉerror($"Unknown, duplicate, or incomplete compile option '{arguments[Index]}'.");
        }

        var Outputˉpath = Requestedˉoutputˉpath ?? Path.ChangeExtension(Sourceˉpath, ".wvb");
        if (Dependencyˉpaths.Count >= Seedˉcompiler.MAX_SOURCE_MODULES)
        {
            Console.Error.WriteLine(
                $"A compilation may contain at most {Seedˉcompiler.MAX_SOURCE_MODULES} source modules.");
            return EXIT_COMPILATION;
        }
        if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(Outputˉpath), ".wvb"))
        {
            return Usageˉerror("The compile output must use the .wvb module extension.");
        }

        var Pathˉcomparer = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
        var Sourceˉpaths = new List<string>(Dependencyˉpaths.Count + 1) { Sourceˉpath };
        Sourceˉpaths.AddRange(Dependencyˉpaths);
        var Uniqueˉsourceˉpaths = new HashSet<string>(Pathˉcomparer);
        foreach (var Path in Sourceˉpaths)
        {
            if (!StringComparer.OrdinalIgnoreCase.Equals(System.IO.Path.GetExtension(Path), ".wv"))
            {
                return Usageˉerror("Every compile input must use the .wv source extension.");
            }
            if (!Uniqueˉsourceˉpaths.Add(Path))
            {
                return Usageˉerror($"The compile source path is supplied more than once: {Path}");
            }
            if (Pathˉcomparer.Equals(Path, Outputˉpath))
            {
                return Usageˉerror("The compile output path must differ from every source path.");
            }
        }

        var Sourceˉinputs = new List<Sourceˉmoduleˉinput>(Sourceˉpaths.Count);
        long Totalˉsourceˉbytes = 0;
        foreach (var Path in Sourceˉpaths)
        {
            var Sourceˉlength = new FileInfo(Path).Length;
            if (Sourceˉlength > MAX_SOURCE_FILE_BYTES)
            {
                Console.Error.WriteLine(
                    $"Source file '{Path}' exceeds the {MAX_SOURCE_FILE_BYTES} byte input limit.");
                return EXIT_COMPILATION;
            }
            Totalˉsourceˉbytes += Sourceˉlength;
            if (Totalˉsourceˉbytes > MAX_SOURCE_SET_FILE_BYTES)
            {
                Console.Error.WriteLine(
                    $"The source-file set exceeds the {MAX_SOURCE_SET_FILE_BYTES} byte input limit.");
                return EXIT_COMPILATION;
            }
            Sourceˉinputs.Add(new(Path, File.ReadAllText(Path, STRICT_UTF8)));
        }

        var Result = Seedˉcompiler.Compileˉmodules(Sourceˉinputs[0], Sourceˉinputs.Skip(1).ToArray());
        if (!Result.Success)
        {
            foreach (var Diagnostic in Result.Diagnostics)
            {
                var Diagnosticˉsource = string.IsNullOrEmpty(Diagnostic.Span.Sourceˉname)
                    ? Sourceˉpath
                    : Diagnostic.Span.Sourceˉname;
                Console.Error.WriteLine(
                    $"{Diagnosticˉsource}({Diagnostic.Span.Line},{Diagnostic.Span.Column}): " +
                    $"error {Diagnostic.Code} [{Diagnostic.Phase}]: {Diagnostic.Message}");
            }

            return EXIT_COMPILATION;
        }

        var Bytes = Result.Moduleˉbytes.ToArray();
        File.WriteAllBytes(Outputˉpath, Bytes);
        Console.WriteLine($"Compiled: {Outputˉpath}");
        Console.WriteLine($"SHA-256: {Moduleˉdigest.Calculateˉsha256(Bytes)}");
        return EXIT_SUCCESS;
    }

    private static int Inspect(string[] arguments)
    {
        if (arguments.Length != 1)
        {
            return Usageˉerror("Usage: windvale inspect <module.wvb>");
        }

        var Bytes = Readˉmoduleˉbytes(arguments[0]);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        Console.Write(Moduleˉinspector.Inspect(Module, Bytes));
        return EXIT_SUCCESS;
    }

    private static int Assemble(string[] arguments)
    {
        if (arguments.Length is not (1 or 3) ||
            (arguments.Length == 3 && arguments[1] != "-o"))
        {
            return Usageˉerror("Usage: windvale assemble <source.wva> [-o <object.wvo>]");
        }

        var Sourceˉpath = Path.GetFullPath(arguments[0]);
        var Outputˉpath = arguments.Length == 3
            ? Path.GetFullPath(arguments[2])
            : Path.ChangeExtension(Sourceˉpath, ".wvo");
        if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(Sourceˉpath), ".wva"))
        {
            return Usageˉerror("The assemble input must use the .wva source extension.");
        }
        if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(Outputˉpath), ".wvo"))
        {
            return Usageˉerror("The assemble output must use the .wvo object extension.");
        }

        var Pathˉcomparer = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
        if (Pathˉcomparer.Equals(Sourceˉpath, Outputˉpath))
        {
            return Usageˉerror("The assemble output path must differ from the source path.");
        }
        if (new FileInfo(Sourceˉpath).Length > Assemblyˉlimits.MAX_SOURCE_BYTES)
        {
            Console.Error.WriteLine(
                $"Assembly source exceeds the {Assemblyˉlimits.MAX_SOURCE_BYTES} byte input limit.");
            return EXIT_COMPILATION;
        }

        var Source = File.ReadAllText(Sourceˉpath, STRICT_UTF8);
        var Result = Assemblyˉcompiler.Assemble(Source);
        if (!Result.Success)
        {
            foreach (var Diagnostic in Result.Diagnostics)
            {
                Console.Error.WriteLine(
                    $"{Sourceˉpath}({Diagnostic.Line},{Diagnostic.Column}): " +
                    $"error {Diagnostic.Code} [assembler]: {Diagnostic.Message}");
            }
            return EXIT_COMPILATION;
        }

        File.WriteAllBytes(Outputˉpath, Result.Objectˉbytes.AsSpan());
        Console.WriteLine($"Assembled: {Outputˉpath}");
        Console.WriteLine($"SHA-256: {Objectˉdigest.Calculateˉsha256(Result.Objectˉbytes.AsSpan())}");
        return EXIT_SUCCESS;
    }

    private static int Verify(string[] arguments)
    {
        if (arguments.Length != 1)
        {
            return Usageˉerror("Usage: windvale verify <module.wvb>");
        }

        var Bytes = Readˉmoduleˉbytes(arguments[0]);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        Console.WriteLine($"Verified: {Module.Module.Name}");
        Console.WriteLine($"SHA-256: {Moduleˉdigest.Calculateˉsha256(Bytes)}");
        return EXIT_SUCCESS;
    }

    private static int Link(string[] arguments)
    {
        if (arguments.Length < 7 ||
            arguments[0] != "--base-address" ||
            arguments[2] != "--entry" ||
            arguments[4] != "-o" ||
            !uint.TryParse(
                arguments[1],
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var Baseˉaddress))
        {
            return Usageˉerror(
                "Usage: windvale link --base-address <u32> --entry <export> -o <image.bin> <object.wvo>...");
        }

        var Entryˉsymbol = arguments[3];
        var Outputˉpath = Path.GetFullPath(arguments[5]);
        var Inputˉpaths = arguments[6..].Select(Path.GetFullPath).ToArray();
        if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(Outputˉpath), ".bin"))
        {
            return Usageˉerror("The link output must use the .bin flat-image extension.");
        }
        if (Inputˉpaths.Any(Inputˉpath =>
            !StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(Inputˉpath), ".wvo")))
        {
            return Usageˉerror("Every link input must use the .wvo object extension.");
        }
        if (Inputˉpaths.Length > Linkˉlimits.MAX_INPUT_OBJECTS)
        {
            return Usageˉerror($"A link accepts at most {Linkˉlimits.MAX_INPUT_OBJECTS} object inputs.");
        }

        var Pathˉcomparer = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
        if (Inputˉpaths.Any(Inputˉpath => Pathˉcomparer.Equals(Inputˉpath, Outputˉpath)))
        {
            return Usageˉerror("The link output path must differ from every input path.");
        }

        var Inputs = ImmutableArray.CreateBuilder<Linkˉinput>(Inputˉpaths.Length);
        for (var Inputˉindex = 0; Inputˉindex < Inputˉpaths.Length; Inputˉindex++)
        {
            var Inputˉpath = Inputˉpaths[Inputˉindex];
            if (new FileInfo(Inputˉpath).Length > Objectˉlimits.MAX_OBJECT_BYTES)
            {
                Console.Error.WriteLine(
                    $"input[{Inputˉindex}]: error WVL1002 [linker]: " +
                    $"The input object exceeds {Objectˉlimits.MAX_OBJECT_BYTES} bytes.");
                return EXIT_COMPILATION;
            }
            Inputs.Add(new(File.ReadAllBytes(Inputˉpath).ToImmutableArray()));
        }
        var Result = Linkˉcompiler.Link(Inputs.ToImmutable(), new(Baseˉaddress, Entryˉsymbol));
        if (!Result.Success)
        {
            foreach (var Diagnostic in Result.Diagnostics)
            {
                var Scope = Diagnostic.Inputˉindex < 0
                    ? "link"
                    : $"input[{Diagnostic.Inputˉindex}]";
                Console.Error.WriteLine(
                    $"{Scope}: error {Diagnostic.Code} [linker]: {Diagnostic.Message}");
            }
            return EXIT_COMPILATION;
        }

        File.WriteAllBytes(Outputˉpath, Result.Imageˉbytes.AsSpan());
        Console.Write(STRICT_UTF8.GetString(Result.Mapˉbytes.AsSpan()));
        return EXIT_SUCCESS;
    }

    private static int Run(string[] arguments)
    {
        if (arguments.Length == 0)
        {
            return Usageˉerror(
                "Usage: windvale run <module.wvb> [--allow <capability>]... [--max-steps <count>] [-- <argument>...]");
        }

        var Moduleˉpath = arguments[0];
        var Authorized = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);
        var Programˉarguments = ImmutableArray.CreateBuilder<string>();
        long Maximumˉsteps = 1_000_000;
        for (var Index = 1; Index < arguments.Length; Index++)
        {
            switch (arguments[Index])
            {
                case "--allow" when Index + 1 < arguments.Length:
                    var Capability = arguments[++Index];
                    if (!Capabilityˉcatalog.Tryˉget(Capability, out _))
                    {
                        return Usageˉerror($"Unknown Seed capability '{Capability}'.");
                    }

                    Authorized.Add(Capability);
                    break;
                case "--max-steps" when Index + 1 < arguments.Length:
                    if (!long.TryParse(arguments[++Index], out Maximumˉsteps) || Maximumˉsteps <= 0)
                    {
                        return Usageˉerror("--max-steps requires a positive integer.");
                    }

                    break;
                case "--":
                    Programˉarguments.AddRange(arguments[(Index + 1)..]);
                    Index = arguments.Length;
                    break;
                default:
                    return Usageˉerror($"Unknown or incomplete run option '{arguments[Index]}'.");
            }
        }

        var Bytes = Readˉmoduleˉbytes(Moduleˉpath);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        var Runtime = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                Programˉarguments.ToImmutable(),
                Console.Out,
                Console.Error,
                new Nativeˉhostedˉfileˉreader(),
                new Nativeˉhostedˉfileˉwriter())),
            new(Authorized.ToImmutable(), Maximumˉsteps));
        var Result = Runtime.Runˉmain();
        Console.WriteLine($"Result: {Result.Exitˉcode}");
        return EXIT_SUCCESS;
    }

    private static int Inspectˉobject(string[] arguments)
    {
        if (arguments.Length != 1)
        {
            return Usageˉerror("Usage: windvale object-inspect <object.wvo>");
        }

        var Bytes = Readˉobjectˉbytes(arguments[0]);
        var Value = Objectˉcodec.Readˉandˉverify(Bytes);
        Console.Write(Objectˉinspector.Inspect(Value, Bytes));
        return EXIT_SUCCESS;
    }

    private static int Verifyˉobject(string[] arguments)
    {
        if (arguments.Length != 1)
        {
            return Usageˉerror("Usage: windvale object-verify <object.wvo>");
        }

        var Bytes = Readˉobjectˉbytes(arguments[0]);
        var Value = Objectˉcodec.Readˉandˉverify(Bytes);
        Console.WriteLine($"Verified object: {Value.Value.Architecture}");
        Console.WriteLine($"SHA-256: {Objectˉdigest.Calculateˉsha256(Bytes)}");
        return EXIT_SUCCESS;
    }

    private static byte[] Readˉmoduleˉbytes(string path)
    {
        var Fullˉpath = Path.GetFullPath(path);
        var Length = new FileInfo(Fullˉpath).Length;
        if (Length > Bytecodeˉlimits.MAX_MODULE_BYTES)
        {
            throw new Moduleˉformatˉexception(
                "WVB1001",
                "The module exceeds the module-size limit.");
        }

        return File.ReadAllBytes(Fullˉpath);
    }

    private static byte[] Readˉobjectˉbytes(string path)
    {
        var Fullˉpath = Path.GetFullPath(path);
        var Length = new FileInfo(Fullˉpath).Length;
        if (Length > Objectˉlimits.MAX_OBJECT_BYTES)
        {
            throw new Objectˉformatˉexception("WVO1001", "The object exceeds the object-size limit.");
        }

        return File.ReadAllBytes(Fullˉpath);
    }

    private static int Usageˉerror(string message)
    {
        Console.Error.WriteLine(message);
        Console.Error.WriteLine("Run 'windvale help' for command details.");
        return EXIT_USAGE;
    }

    private static void Writeˉhelp(TextWriter output)
    {
        output.WriteLine("Windvale Seed tool");
        output.WriteLine();
        output.WriteLine("Commands:");
        output.WriteLine("  windvale compile <source.wv> [--module <dependency.wv>]... [-o <module.wvb>]");
        output.WriteLine("  windvale assemble <source.wva> [-o <object.wvo>]");
        output.WriteLine("  windvale link --base-address <u32> --entry <export> -o <image.bin> <object.wvo>...");
        output.WriteLine("  windvale inspect <module.wvb>");
        output.WriteLine("  windvale verify <module.wvb>");
        output.WriteLine("  windvale object-inspect <object.wvo>");
        output.WriteLine("  windvale object-verify <object.wvo>");
        output.WriteLine("  windvale run <module.wvb> [--allow <capability>]... [--max-steps <count>] [-- <argument>...]");
        output.WriteLine("  windvale help");
    }
}
