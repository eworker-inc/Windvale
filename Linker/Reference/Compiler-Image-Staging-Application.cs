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
    public const int MODULE_BYTES = 75_503;
    public const string MODULE_SHA256 =
        "e43e2cc868b5f7ac3ffbee322bef60ce748c736e666889aaeda7c06a90daa5bb";
    public const int WINDOWS_APPLICATION_BYTES = 851_968;
    public const string WINDOWS_APPLICATION_SHA256 =
        "967827e4592c23f30e2a70b9a60a43837c1dfec6112584596c09d382058e2752";
    public const int LINUX_APPLICATION_BYTES = 851_968;
    public const string LINUX_APPLICATION_SHA256 =
        "02b07d23b763fa4dd2d11bb9c9ca94be32bdbd698b1f9ce7b466af90b768eef8";
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
