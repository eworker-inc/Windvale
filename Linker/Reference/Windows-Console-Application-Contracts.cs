using System.Collections.Immutable;

namespace Windvale.Linker;

public sealed record Windowsˉconsoleˉapplicationˉdiagnostic(
    string Code,
    string Message);

public sealed class Windowsˉconsoleˉapplicationˉresult
{
    private Windowsˉconsoleˉapplicationˉresult(
        ImmutableArray<byte> imageˉbytes,
        ImmutableArray<Windowsˉconsoleˉapplicationˉdiagnostic> diagnostics)
    {
        Imageˉbytes = imageˉbytes;
        Diagnostics = diagnostics;
    }

    public bool Success => Diagnostics.IsEmpty;

    public ImmutableArray<byte> Imageˉbytes { get; }

    public ImmutableArray<Windowsˉconsoleˉapplicationˉdiagnostic> Diagnostics { get; }

    internal static Windowsˉconsoleˉapplicationˉresult Succeeded(
        ImmutableArray<byte> imageˉbytes) =>
        new(imageˉbytes, []);

    internal static Windowsˉconsoleˉapplicationˉresult Failed(
        string code,
        string message) =>
        new([], [new(code, message)]);
}

public sealed class Verifiedˉwindowsˉconsoleˉapplication
{
    internal Verifiedˉwindowsˉconsoleˉapplication(
        ImmutableArray<byte> nativeˉimageˉbytes,
        uint nativeˉentryˉoffset)
    {
        Nativeˉimageˉbytes = nativeˉimageˉbytes;
        Nativeˉentryˉoffset = nativeˉentryˉoffset;
    }

    public ImmutableArray<byte> Nativeˉimageˉbytes { get; }

    public uint Nativeˉentryˉoffset { get; }
}

public sealed class Windowsˉconsoleˉapplicationˉexception : Exception
{
    public Windowsˉconsoleˉapplicationˉexception(
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

public static class Windowsˉconsoleˉapplicationˉcontract
{
    public const int FORMAT_VERSION = 1;
    public const string TARGET_NAME = "windows-x64-console-v1";
    public const int STARTUP_BYTES = 67;
    public const int NATIVE_IMAGE_OFFSET = 80;
    public const uint RECORD_ARENA_BYTES = 2 * 1024 * 1024;
    public const uint TEXT_ARENA_BYTES = 16 * 1024 * 1024;
    public const uint DATA_VIRTUAL_BYTES =
        112 + RECORD_ARENA_BYTES + TEXT_ARENA_BYTES;
    public const int MAX_APPLICATION_BYTES = 4_196_352;
}
