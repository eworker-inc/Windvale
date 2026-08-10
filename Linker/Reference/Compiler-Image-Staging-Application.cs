using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Compilerˉimageˉstagingˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-compiler-image-staging-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-compiler-image-staging-v1";
    public const string MODULE_NAME =
        "Linkerˉcompilerˉwvoˉsegmentedˉflatˉimageˉstagingˉtool";
    public const int MODULE_BYTES = 75_553;
    public const string MODULE_SHA256 =
        "14521acae6052d08add386833a35dd22c36e0dd07a1fad494961ee8064119d1c";
    public const int WINDOWS_APPLICATION_BYTES = 852_480;
    public const string WINDOWS_APPLICATION_SHA256 =
        "7f4be5d6b1236b5f5171e52f3861540432c4781140d154e28d52f804aa8cbcde";
    public const int LINUX_APPLICATION_BYTES = 851_968;
    public const string LINUX_APPLICATION_SHA256 =
        "845402fb71bbf7a76524fd90b771b7c6e2d88b92ff9fe7440efe5839304a6ab3";
}

public static class Compilerˉimageˉstagingˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Verifiedˉmodule module,
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes) =>
        Compilerˉimageˉprocessˉapplicationˉwriter.Writeˉwindows(
            module,
            fragment,
            moduleˉbytes,
            Compilerˉimageˉstagingˉapplicationˉcontract.MODULE_NAME,
            Compilerˉimageˉstagingˉapplicationˉcontract.MODULE_BYTES,
            Compilerˉimageˉstagingˉapplicationˉcontract.MODULE_SHA256,
            Compilerˉimageˉstagingˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Compilerˉimageˉstagingˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "WVW3501",
            "compiler-image staging");

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Verifiedˉmodule module,
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes) =>
        Compilerˉimageˉprocessˉapplicationˉwriter.Writeˉlinux(
            module,
            fragment,
            moduleˉbytes,
            Compilerˉimageˉstagingˉapplicationˉcontract.MODULE_NAME,
            Compilerˉimageˉstagingˉapplicationˉcontract.MODULE_BYTES,
            Compilerˉimageˉstagingˉapplicationˉcontract.MODULE_SHA256,
            Compilerˉimageˉstagingˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Compilerˉimageˉstagingˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "WVL3501",
            "compiler-image staging");
}
