using System.Collections.Immutable;
using Windvale.Bootstrap;
using Windvale.ObjectModel;

const string Usage =
    "Usage: Windvale.Bootstrap " +
    "<--output <BOOTX64.EFI>|--linked-output <IMAGE.BIN>|--object-directory <DIRECTORY>|" +
    "--object-directory-native-wva <DIRECTORY>> " +
    "[--process-wva-directory <DIRECTORY>] " +
    "[--scenario <normal|invalid-opcode|general-protection|user-fault|service-fault>]";
var Mode = args.Length == 0 ? string.Empty : args[0];
var Hasˉprocessˉwvaˉdirectory =
    Mode == "--object-directory-native-wva" &&
    args.Length >= 4 &&
    args[2] == "--process-wva-directory";
var Baseˉargumentˉcount = Hasˉprocessˉwvaˉdirectory ? 4 : 2;
if ((args.Length != Baseˉargumentˉcount && args.Length != Baseˉargumentˉcount + 2) ||
    (Mode != "--output" &&
        Mode != "--linked-output" &&
        Mode != "--object-directory" &&
        Mode != "--object-directory-native-wva") ||
    string.IsNullOrWhiteSpace(args[1]) ||
    (Hasˉprocessˉwvaˉdirectory && string.IsNullOrWhiteSpace(args[3])) ||
    (args.Length == Baseˉargumentˉcount + 2 &&
        (args[Baseˉargumentˉcount] != "--scenario" ||
            string.IsNullOrWhiteSpace(args[Baseˉargumentˉcount + 1]))))
{
    Console.Error.WriteLine(Usage);
    return 64;
}

var Scenario = Firmwareˉprobeˉscenario.Normal;
if (args.Length == Baseˉargumentˉcount + 2)
{
    Scenario = args[Baseˉargumentˉcount + 1] switch
    {
        "normal" => Firmwareˉprobeˉscenario.Normal,
        "invalid-opcode" => Firmwareˉprobeˉscenario.Invalidˉopcode,
        "general-protection" => Firmwareˉprobeˉscenario.Generalˉprotection,
        "user-fault" => Firmwareˉprobeˉscenario.Userˉfault,
        "service-fault" => Firmwareˉprobeˉscenario.Serviceˉfault,
        _ => (Firmwareˉprobeˉscenario)(-1),
    };
    if (Scenario is not Firmwareˉprobeˉscenario.Normal and
        not Firmwareˉprobeˉscenario.Invalidˉopcode and
        not Firmwareˉprobeˉscenario.Generalˉprotection and
        not Firmwareˉprobeˉscenario.Userˉfault and
        not Firmwareˉprobeˉscenario.Serviceˉfault)
    {
        Console.Error.WriteLine(Usage);
        return 64;
    }
}

try
{
    if (args[0] is "--object-directory" or "--object-directory-native-wva")
    {
        if (!Directory.Exists(args[1]) || Directory.EnumerateFileSystemEntries(args[1]).Any())
        {
            throw new InvalidOperationException(
                "The object-inventory destination must be an existing empty directory.");
        }

        var Scope = args[0] == "--object-directory-native-wva"
            ? Firmwareˉprobeˉobjectˉinventoryˉscope.Nativeˉwvaˉexternal
            : Firmwareˉprobeˉobjectˉinventoryˉscope.Complete;
        Kernelˉprocessˉimageˉwvaˉobjects? Processˉwvaˉobjects = null;
        if (Hasˉprocessˉwvaˉdirectory)
        {
            var Processˉwvaˉdirectory = args[3];
            if (!Directory.Exists(Processˉwvaˉdirectory))
            {
                throw new InvalidOperationException(
                    "The native process-WVA input directory does not exist.");
            }

            var Clientˉobjectˉname = Scenario switch
            {
                Firmwareˉprobeˉscenario.Userˉfault => "Process-User-Fault-Shim.wvo",
                Firmwareˉprobeˉscenario.Serviceˉfault => "Process-Service-Fault-Shim.wvo",
                _ => "Process-User-Shim.wvo",
            };
            var Expectedˉnames = new[]
            {
                "Init-Resource-Service-Shim.wvo",
                "Directory-Process-Service-Shim.wvo",
                "Boot-Resource-Service.wvo",
                Clientˉobjectˉname,
            };
            var Actualˉnames = Directory.EnumerateFileSystemEntries(Processˉwvaˉdirectory)
                .Select(Path.GetFileName)
                .OrderBy(Name => Name, StringComparer.Ordinal);
            if (!Expectedˉnames.OrderBy(Name => Name, StringComparer.Ordinal)
                    .SequenceEqual(Actualˉnames, StringComparer.Ordinal))
            {
                throw new InvalidOperationException(
                    "The native process-WVA input directory does not contain the exact reviewed objects.");
            }

            Processˉwvaˉobjects = new(
                Readˉobject(Processˉwvaˉdirectory, "Init-Resource-Service-Shim.wvo"),
                Readˉobject(Processˉwvaˉdirectory, "Directory-Process-Service-Shim.wvo"),
                Readˉobject(Processˉwvaˉdirectory, "Boot-Resource-Service.wvo"),
                Readˉobject(Processˉwvaˉdirectory, Clientˉobjectˉname));
        }

        var Inventory = Firmwareˉprobe.Buildˉobjectˉinventory(
            Scenario,
            Scope,
            Processˉwvaˉobjects);
        foreach (var Object in Inventory.Objects)
        {
            var Objectˉpath = Path.Combine(args[1], Object.Fileˉname);
            using var Output = new FileStream(
                Objectˉpath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);
            Output.Write(Object.Bytes.AsSpan());
        }

        var Inventoryˉname = Scope == Firmwareˉprobeˉobjectˉinventoryˉscope.Complete
            ? "windvale-os-probe-object-inventory"
            : "windvale-os-probe-native-wva-inventory";
        Console.WriteLine($"{Inventoryˉname} {Firmwareˉprobe.FORMAT_VERSION}");
        Console.WriteLine($"entry-symbol={Inventory.Entryˉsymbol}");
        Console.WriteLine($"object-count={Inventory.Objects.Length}");
        foreach (var Object in Inventory.Objects)
        {
            Console.WriteLine($"object={Object.Fileˉname}");
        }
        return 0;
    }

    if (args[0] == "--linked-output")
    {
        var Link = Firmwareˉprobe.Buildˉlinkedˉimage(Scenario);
        File.WriteAllBytes(args[1], Link.Imageˉbytes.AsSpan());
        Console.WriteLine($"windvale-os-probe-linked-image {Firmwareˉprobe.FORMAT_VERSION}");
        Console.WriteLine($"native-image-bytes={Link.Imageˉbytes.Length}");
        Console.WriteLine($"entry-offset={Link.Entryˉaddress}");
        Console.WriteLine(
            $"native-image-sha256={Objectˉdigest.Calculateˉsha256(Link.Imageˉbytes.AsSpan())}");
        return 0;
    }

    var Application = Firmwareˉprobe.Buildˉapplication(Scenario);
    File.WriteAllBytes(args[1], Application.AsSpan());
    Console.WriteLine($"windvale-os-probe-builder {Firmwareˉprobe.FORMAT_VERSION}");
    Console.WriteLine($"efi-bytes={Application.Length}");
    Console.WriteLine($"efi-sha256={Objectˉdigest.Calculateˉsha256(Application.AsSpan())}");
    return 0;
}
catch (Exception Exception)
{
    Console.Error.WriteLine($"WVOS2001: The firmware probe could not be written: {Exception.Message}");
    return 1;
}

static ImmutableArray<byte> Readˉobject(string directory, string name)
{
    return File.ReadAllBytes(Path.Combine(directory, name)).ToImmutableArray();
}
