using System.Collections.Immutable;

namespace Windvale.Linker;

public sealed record Linuxˉconsoleˉapplicationˉdiagnostic(
    string Code,
    string Message);

public sealed class Linuxˉconsoleˉapplicationˉresult
{
    private Linuxˉconsoleˉapplicationˉresult(
        ImmutableArray<byte> imageˉbytes,
        ImmutableArray<Linuxˉconsoleˉapplicationˉdiagnostic> diagnostics)
    {
        Imageˉbytes = imageˉbytes;
        Diagnostics = diagnostics;
    }

    public bool Success => Diagnostics.IsEmpty;

    public ImmutableArray<byte> Imageˉbytes { get; }

    public ImmutableArray<Linuxˉconsoleˉapplicationˉdiagnostic> Diagnostics { get; }

    internal static Linuxˉconsoleˉapplicationˉresult Succeeded(
        ImmutableArray<byte> imageˉbytes) =>
        new(imageˉbytes, []);

    internal static Linuxˉconsoleˉapplicationˉresult Failed(
        string code,
        string message) =>
        new([], [new(code, message)]);
}

public sealed class Verifiedˉlinuxˉconsoleˉapplication
{
    internal Verifiedˉlinuxˉconsoleˉapplication(
        ImmutableArray<byte> nativeˉimageˉbytes,
        uint nativeˉentryˉoffset)
    {
        Nativeˉimageˉbytes = nativeˉimageˉbytes;
        Nativeˉentryˉoffset = nativeˉentryˉoffset;
    }

    public ImmutableArray<byte> Nativeˉimageˉbytes { get; }

    public uint Nativeˉentryˉoffset { get; }
}

public sealed class Linuxˉconsoleˉapplicationˉexception : Exception
{
    public Linuxˉconsoleˉapplicationˉexception(
        string code,
        string message,
        int? byteˉoffset = null)
        : base(byteˉoffset is null
            ? $"{code}: {message}"
            : $"{code} at byte {byteˉoffset}: {message}")
    {
        Code = code;
        Byteˉoffset = byteˉoffset;
    }

    public string Code { get; }

    public int? Byteˉoffset { get; }
}

public static class Linuxˉconsoleˉapplicationˉcontract
{
    public const int FORMAT_VERSION = 1;
    public const string TARGET_NAME = "linux-x64-console-v1";
    public const int STARTUP_BYTES = 131;
    public const int NATIVE_IMAGE_OFFSET = 144;
    public const uint HEADER_BYTES = 0x1000;
    public const uint TEXT_VIRTUAL_ADDRESS = 0x1000;
    public const uint RECORD_ARENA_BYTES = Nativeˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES;
    public const uint TEXT_ARENA_BYTES = Nativeˉconsoleˉapplicationˉcontract.TEXT_ARENA_BYTES;
    public const uint DATA_VIRTUAL_BYTES = Nativeˉconsoleˉapplicationˉcontract.DATA_VIRTUAL_BYTES;
    public const ulong STACK_BYTES = Nativeˉconsoleˉapplicationˉcontract.STACK_BYTES;
    public const int MAX_APPLICATION_BYTES = 4_202_608;
}
