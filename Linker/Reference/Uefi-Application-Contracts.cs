using System.Collections.Immutable;

namespace Windvale.Linker;

public sealed record Uefiˉapplicationˉdiagnostic(
    string Code,
    string Message);

public sealed class Uefiˉapplicationˉresult
{
    private Uefiˉapplicationˉresult(
        ImmutableArray<byte> imageˉbytes,
        ImmutableArray<Uefiˉapplicationˉdiagnostic> diagnostics)
    {
        Imageˉbytes = imageˉbytes;
        Diagnostics = diagnostics;
    }

    public bool Success => Diagnostics.IsEmpty;

    public ImmutableArray<byte> Imageˉbytes { get; }

    public ImmutableArray<Uefiˉapplicationˉdiagnostic> Diagnostics { get; }

    internal static Uefiˉapplicationˉresult Succeeded(ImmutableArray<byte> imageˉbytes) =>
        new(imageˉbytes, []);

    internal static Uefiˉapplicationˉresult Failed(string code, string message) =>
        new([], [new(code, message)]);
}

public sealed class Verifiedˉuefiˉapplication
{
    internal Verifiedˉuefiˉapplication(
        ImmutableArray<byte> codeˉbytes,
        uint entryˉcodeˉoffset)
    {
        Codeˉbytes = codeˉbytes;
        Entryˉcodeˉoffset = entryˉcodeˉoffset;
    }

    public ImmutableArray<byte> Codeˉbytes { get; }

    public uint Entryˉcodeˉoffset { get; }
}

public sealed class Uefiˉapplicationˉexception : Exception
{
    public Uefiˉapplicationˉexception(string code, string message, int? byteˉoffset = null)
        : base(byteˉoffset is null ? $"{code}: {message}" : $"{code} at byte {byteˉoffset}: {message}")
    {
        Code = code;
        Byteˉoffset = byteˉoffset;
    }

    public string Code { get; }

    public int? Byteˉoffset { get; }
}

public static class Uefiˉapplicationˉcontract
{
    public const int FORMAT_VERSION = 3;
    public const string TARGET_NAME = "pe32-plus-x86-64-uefi-application-v3";
    public const uint REQUIRED_LINK_BASE_ADDRESS = 0;
    public const int MAX_LINKED_IMAGE_BYTES = Linkˉlimits.MAX_IMAGE_BYTES;
    public const int MAX_APPLICATION_BYTES = 4_195_328;
}
