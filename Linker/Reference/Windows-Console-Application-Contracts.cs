using System.Collections.Immutable;
using Windvale.Compiler.Native;

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
        uint nativeˉentryˉoffset,
        int formatˉversion = Windowsˉconsoleˉapplicationˉcontract.FORMAT_VERSION,
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
    public const int STARTUP_BYTES = 98;
    public const int NATIVE_IMAGE_OFFSET = 112;
    public const uint RECORD_ARENA_BYTES = Nativeˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES;
    public const uint TEXT_ARENA_BYTES = Nativeˉconsoleˉapplicationˉcontract.TEXT_ARENA_BYTES;
    public const uint DATA_VIRTUAL_BYTES = Nativeˉconsoleˉapplicationˉcontract.DATA_VIRTUAL_BYTES;
    public const ulong STACK_BYTES = Nativeˉconsoleˉapplicationˉcontract.STACK_BYTES;
    public const int MAX_APPLICATION_BYTES = 4_196_352;
    public const int HOSTED_FORMAT_VERSION = 2;
    public const string HOSTED_TARGET_NAME = "windows-x64-console-v2";
    public const int COMPILER_FORMAT_VERSION = 3;
    public const string COMPILER_TARGET_NAME = "windows-x64-console-v3";
    public const int VERIFIER_FORMAT_VERSION = 4;
    public const string VERIFIER_TARGET_NAME = "windows-x64-verifier-v1";
    public const string INSPECTOR_TARGET_NAME = "windows-x64-wvb-inspector-v1";
    public const string WVB_RUNNER_TARGET_NAME = "windows-x64-wvb-runner-v1";
    public const string WVO_INSPECTOR_TARGET_NAME = "windows-x64-wvo-inspector-v1";
    public const int BUILD_DRIVER_FORMAT_VERSION = 5;
    public const string BUILD_DRIVER_TARGET_NAME = "windows-x64-build-driver-v1";
    public const int WVA_ASSEMBLER_FORMAT_VERSION = 6;
    public const string WVA_ASSEMBLER_TARGET_NAME = "windows-x64-wva-assembler-v1";
    public const int WV_LINKER_FORMAT_VERSION = 7;
    public const string WV_LINKER_TARGET_NAME = "windows-x64-wv-linker-v1";
    public const string CONSOLE_PACKAGER_TARGET_NAME =
        "windows-x64-console-packager-v1";
    public const string WVB_TO_WVO_TARGET_NAME = "windows-x64-wvb-to-wvo-v1";
    public const int HOSTED_STARTUP_BYTES = 224;
    public const int HOSTED_OUTPUT_SERVICE_OFFSET = 224;
    public const int HOSTED_NATIVE_IMAGE_OFFSET = 496;
    public const uint HOSTED_DATA_FILE_BYTES =
        Nativeˉconsoleˉapplicationˉcontract.HOSTED_DATA_HEADER_BYTES;
    public const uint HOSTED_DATA_VIRTUAL_BYTES =
        Nativeˉconsoleˉapplicationˉcontract.HOSTED_DATA_VIRTUAL_BYTES;
    public const int HOSTED_MAX_APPLICATION_BYTES = 4_196_864;
}
