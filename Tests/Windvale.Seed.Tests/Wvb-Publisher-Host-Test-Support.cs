namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Createˉtestˉhardˉlink(string path, string target)
    {
        var Result = OperatingSystem.IsWindows()
            ? Createˉhardˉlinkˉwindows(path, target, 0) ? 0 :
                System.Runtime.InteropServices.Marshal.GetLastPInvokeError()
            : Createˉhardˉlinkˉlinux(target, path) == 0 ? 0 :
                System.Runtime.InteropServices.Marshal.GetLastPInvokeError();
        if (Result != 0)
        {
            throw new System.ComponentModel.Win32Exception(
                Result,
                "The publisher test could not create its hard-link alias.");
        }
    }

    [System.Runtime.InteropServices.DllImport(
        "kernel32.dll",
        EntryPoint = "CreateHardLinkW",
        CharSet = System.Runtime.InteropServices.CharSet.Unicode,
        SetLastError = true)]
    [return: System.Runtime.InteropServices.MarshalAs(
        System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool Createˉhardˉlinkˉwindows(
        string path,
        string target,
        nint securityˉattributes);

    [System.Runtime.InteropServices.DllImport(
        "libc",
        EntryPoint = "link",
        SetLastError = true)]
    private static extern int Createˉhardˉlinkˉlinux(
        string target,
        string path);
}
