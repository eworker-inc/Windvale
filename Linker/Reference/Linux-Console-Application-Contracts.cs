using System.Collections.Immutable;
using Windvale.Compiler.Native;

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
        uint nativeˉentryˉoffset,
        int formatˉversion = Linuxˉconsoleˉapplicationˉcontract.FORMAT_VERSION,
        ImmutableArray<Nativeˉservice> requiredˉservices = default)
    {
        Nativeˉimageˉbytes = nativeˉimageˉbytes;
        Nativeˉentryˉoffset = nativeˉentryˉoffset;
        Formatˉversion = formatˉversion;
        Requiredˉservices = requiredˉservices.IsDefault ? [] : requiredˉservices;
    }

    public ImmutableArray<byte> Nativeˉimageˉbytes { get; }

    public uint Nativeˉentryˉoffset { get; }

    public int Formatˉversion { get; }

    public ImmutableArray<Nativeˉservice> Requiredˉservices { get; }
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
    public const int STARTUP_BYTES = 158;
    public const int NATIVE_IMAGE_OFFSET = 160;
    public const uint HEADER_BYTES = 0x1000;
    public const uint TEXT_VIRTUAL_ADDRESS = 0x1000;
    public const uint RECORD_ARENA_BYTES = Nativeˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES;
    public const uint TEXT_ARENA_BYTES = Nativeˉconsoleˉapplicationˉcontract.TEXT_ARENA_BYTES;
    public const uint DATA_VIRTUAL_BYTES = Nativeˉconsoleˉapplicationˉcontract.DATA_VIRTUAL_BYTES;
    public const ulong STACK_BYTES = Nativeˉconsoleˉapplicationˉcontract.STACK_BYTES;
    public const int MAX_APPLICATION_BYTES = 4_202_608;
    public const int HOSTED_FORMAT_VERSION = 2;
    public const string HOSTED_TARGET_NAME = "linux-x64-console-v2";
    public const int COMPILER_FORMAT_VERSION = 3;
    public const string COMPILER_TARGET_NAME = "linux-x64-console-v3";
    public const int VERIFIER_FORMAT_VERSION = 4;
    public const string VERIFIER_TARGET_NAME = "linux-x64-verifier-v1";
    public const int HOSTED_STARTUP_BYTES = 217;
    public const int HOSTED_OUTPUT_SERVICE_OFFSET = 224;
    public const int HOSTED_NATIVE_IMAGE_OFFSET = 448;
    public const uint HOSTED_DATA_FILE_BYTES =
        Nativeˉconsoleˉapplicationˉcontract.HOSTED_DATA_HEADER_BYTES;
    public const uint HOSTED_DATA_VIRTUAL_BYTES =
        Nativeˉconsoleˉapplicationˉcontract.HOSTED_DATA_VIRTUAL_BYTES;
    public const int HOSTED_MAX_APPLICATION_BYTES = 4_203_520;
}
