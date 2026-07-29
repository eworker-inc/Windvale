using System.Collections.Immutable;
using System.Text;
using Windvale.Bytecode;
using Windvale.Compiler;
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
                "inspect" => Inspect(arguments[1..]),
                "verify" => Verify(arguments[1..]),
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
        if (arguments.Length is not (1 or 3) ||
            (arguments.Length == 3 && arguments[1] != "-o"))
        {
            return Usageˉerror("Usage: windvale compile <source.wv> [-o <module.wvb>]");
        }

        var Sourceˉpath = Path.GetFullPath(arguments[0]);
        var Outputˉpath = arguments.Length == 3
            ? Path.GetFullPath(arguments[2])
            : Path.ChangeExtension(Sourceˉpath, ".wvb");
        if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(Sourceˉpath), ".wv"))
        {
            return Usageˉerror("The compile input must use the .wv source extension.");
        }

        if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(Outputˉpath), ".wvb"))
        {
            return Usageˉerror("The compile output must use the .wvb module extension.");
        }

        var Pathˉcomparer = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
        if (Pathˉcomparer.Equals(Sourceˉpath, Outputˉpath))
        {
            return Usageˉerror("The compile output path must differ from the source path.");
        }

        var Sourceˉlength = new FileInfo(Sourceˉpath).Length;
        if (Sourceˉlength > MAX_SOURCE_FILE_BYTES)
        {
            Console.Error.WriteLine(
                $"Source file exceeds the {MAX_SOURCE_FILE_BYTES} byte input limit.");
            return EXIT_COMPILATION;
        }

        var Source = File.ReadAllText(Sourceˉpath, STRICT_UTF8);
        var Result = Seedˉcompiler.Compile(Source, Sourceˉpath);
        if (!Result.Success)
        {
            foreach (var Diagnostic in Result.Diagnostics)
            {
                Console.Error.WriteLine(
                    $"{Sourceˉpath}({Diagnostic.Span.Line},{Diagnostic.Span.Column}): " +
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

    private static int Run(string[] arguments)
    {
        if (arguments.Length == 0)
        {
            return Usageˉerror(
                "Usage: windvale run <module.wvb> [--allow <capability>]... [--max-steps <count>]");
        }

        var Moduleˉpath = arguments[0];
        var Authorized = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);
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
                default:
                    return Usageˉerror($"Unknown or incomplete run option '{arguments[Index]}'.");
            }
        }

        var Bytes = Readˉmoduleˉbytes(Moduleˉpath);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        var Runtime = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(Console.Out),
            new(Authorized.ToImmutable(), Maximumˉsteps));
        var Result = Runtime.Runˉmain();
        Console.WriteLine($"Result: {Result.Exitˉcode}");
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
        output.WriteLine("  windvale compile <source.wv> [-o <module.wvb>]");
        output.WriteLine("  windvale inspect <module.wvb>");
        output.WriteLine("  windvale verify <module.wvb>");
        output.WriteLine("  windvale run <module.wvb> [--allow <capability>]... [--max-steps <count>]");
        output.WriteLine("  windvale help");
    }
}
