using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉcontainerˉplannerˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-container-planner-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-container-planner-v1";
    public const string MODULE_NAME = "Nativeˉhostedˉcontainerˉplannerˉtool";
    public const int MODULE_BYTES = 37_289;
    public const string MODULE_SHA256 =
        "81cf3932c5e1d4f711b779c515a718ec1acd32c09ae17031aa63b8a66f5ce788";
    public const int WINDOWS_APPLICATION_BYTES = 584_704;
    public const string WINDOWS_APPLICATION_SHA256 =
        "e401ad5aef792a49be72cf711cfc427a859fe4a534aa780ad47d3b4a2c12a5dc";
    public const int LINUX_APPLICATION_BYTES = 585_728;
    public const string LINUX_APPLICATION_SHA256 =
        "8032370c7391bbc6afa94c1e8804db78f682da4e57144a2907394e202806c0d3";
}

public static class Hostedˉcontainerˉplannerˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉwindows(
            fragment,
            capabilities,
            moduleˉname,
            Hostedˉcontainerˉplannerˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-planner",
            "WVW2251");
        return Requireˉwindowsˉidentity(Result);
    }

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉlinux(
            fragment,
            capabilities,
            moduleˉname,
            Hostedˉcontainerˉplannerˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-planner",
            "WVL2251");
        return Requireˉlinuxˉidentity(Result);
    }

    private static Windowsˉconsoleˉapplicationˉresult Requireˉwindowsˉidentity(
        Windowsˉconsoleˉapplicationˉresult result)
    {
        if (!result.Success) { return result; }
        if (!Identityˉmatches(result.Imageˉbytes.AsSpan(),
            Hostedˉcontainerˉplannerˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉcontainerˉplannerˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256))
        {
            return Windowsˉconsoleˉapplicationˉresult.Failed(
                "WVW2251",
                $"Windows hosted-container planner identity is invalid " +
                $"(bytes={result.Imageˉbytes.Length}, " +
                $"sha256={Calculateˉsha256(result.Imageˉbytes.AsSpan())}).");
        }
        return result;
    }

    private static Linuxˉconsoleˉapplicationˉresult Requireˉlinuxˉidentity(
        Linuxˉconsoleˉapplicationˉresult result)
    {
        if (!result.Success) { return result; }
        if (!Identityˉmatches(result.Imageˉbytes.AsSpan(),
            Hostedˉcontainerˉplannerˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉcontainerˉplannerˉapplicationˉcontract.LINUX_APPLICATION_SHA256))
        {
            return Linuxˉconsoleˉapplicationˉresult.Failed(
                "WVL2251",
                $"Linux hosted-container planner identity is invalid " +
                $"(bytes={result.Imageˉbytes.Length}, " +
                $"sha256={Calculateˉsha256(result.Imageˉbytes.AsSpan())}).");
        }
        return result;
    }

    private static bool Identityˉmatches(
        ReadOnlySpan<byte> bytes,
        int expectedˉbytes,
        string expectedˉsha256) =>
        bytes.Length == expectedˉbytes &&
        StringComparer.Ordinal.Equals(Calculateˉsha256(bytes), expectedˉsha256);

    private static string Calculateˉsha256(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
