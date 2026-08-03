using Windvale.Bootstrap;
using Windvale.ObjectModel;

if ((args.Length != 2 && args.Length != 4) ||
    args[0] != "--output" ||
    string.IsNullOrWhiteSpace(args[1]) ||
    (args.Length == 4 && (args[2] != "--scenario" || string.IsNullOrWhiteSpace(args[3]))))
{
    Console.Error.WriteLine(
        "Usage: Windvale.Bootstrap --output <BOOTX64.EFI> [--scenario <normal|invalid-opcode|general-protection|user-fault|service-fault>]");
    return 64;
}

var Scenario = Firmwareˉprobeˉscenario.Normal;
if (args.Length == 4)
{
    Scenario = args[3] switch
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
        Console.Error.WriteLine(
            "Usage: Windvale.Bootstrap --output <BOOTX64.EFI> [--scenario <normal|invalid-opcode|general-protection|user-fault|service-fault>]");
        return 64;
    }
}

try
{
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
