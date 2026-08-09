using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Compilerˉimageˉtransportˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-compiler-image-transport-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-compiler-image-transport-v1";
    public const string MODULE_NAME =
        "Linkerˉcompilerˉimageˉcanonicalˉtransportˉtool";
    public const int MODULE_BYTES = 23_836;
    public const string MODULE_SHA256 =
        "dc5f460ce89bcce2678092030376c8ddc928e682b263af2a73ba2a57034b6d4d";
    public const int WINDOWS_APPLICATION_BYTES = 269_312;
    public const string WINDOWS_APPLICATION_SHA256 =
        "6c204b9b3ee90a4d73ecdaa1ae0f0c4d5f3056973f3ccd3a8489789c6b46ef6d";
    public const int LINUX_APPLICATION_BYTES = 270_336;
    public const string LINUX_APPLICATION_SHA256 =
        "4b7aa91e78880617c3abc8a1cbd59c098cfb274c020d2ecbe7dee214ed9576cd";
}

public static class Compilerˉimageˉtransportˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Verifiedˉmodule module,
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes) =>
        Compilerˉimageˉprocessˉapplicationˉwriter.Writeˉwindows(
            module,
            fragment,
            moduleˉbytes,
            Compilerˉimageˉtransportˉapplicationˉcontract.MODULE_NAME,
            Compilerˉimageˉtransportˉapplicationˉcontract.MODULE_BYTES,
            Compilerˉimageˉtransportˉapplicationˉcontract.MODULE_SHA256,
            Compilerˉimageˉtransportˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Compilerˉimageˉtransportˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "WVW4161",
            "compiler-image transport");

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Verifiedˉmodule module,
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes) =>
        Compilerˉimageˉprocessˉapplicationˉwriter.Writeˉlinux(
            module,
            fragment,
            moduleˉbytes,
            Compilerˉimageˉtransportˉapplicationˉcontract.MODULE_NAME,
            Compilerˉimageˉtransportˉapplicationˉcontract.MODULE_BYTES,
            Compilerˉimageˉtransportˉapplicationˉcontract.MODULE_SHA256,
            Compilerˉimageˉtransportˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Compilerˉimageˉtransportˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "WVL4161",
            "compiler-image transport");
}
