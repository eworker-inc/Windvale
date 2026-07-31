using Windvale.Bootstrap;
using Windvale.ObjectModel;

if (args.Length != 2 || args[0] != "--output" || string.IsNullOrWhiteSpace(args[1]))
{
    Console.Error.WriteLine("Usage: Windvale.Bootstrap --output <BOOTX64.EFI>");
    return 64;
}

try
{
    var Application = Firmwareˉprobe.Buildˉapplication();
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
