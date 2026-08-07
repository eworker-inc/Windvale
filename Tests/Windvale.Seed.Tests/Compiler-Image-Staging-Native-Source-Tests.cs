using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Linker;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Compilerˉimageˉstagingˉbuildsˉnatively()
    {
        var Repository = Findˉrepositoryˉroot();
        var Project = Path.Combine(
            Repository,
            "Windvale-Compiler-Image-Staging.wvproj");
        True(File.Exists(Project), "The compiler-image staging project is missing.");
        var Stageˉzero = Compileˉcompilerˉwvoˉsegmentedˉstagingˉtool();
        Equal(
            Compilerˉimageˉstagingˉapplicationˉcontract.MODULE_BYTES,
            Stageˉzero.Length);
        Equal(
            Compilerˉimageˉstagingˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Stageˉzero));

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-compiler-image-native-source-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Outputˉpath = Path.Combine(Directoryˉpath, "Staging.wvb");
            File.WriteAllBytes(Outputˉpath, [1, 2, 3, 4]);
            var Result = Runˉnativeˉfrontˉdoor(
                Repository,
                Project,
                Outputˉpath);
            Equal(0, Result.Exitˉcode);
            Contains(
                Result.Output,
                "build status=Published verification=compiler-aligned");
            Equal(string.Empty, Result.Error);
            var Native = File.ReadAllBytes(Outputˉpath);
            Equal(
                Compilerˉimageˉstagingˉapplicationˉcontract.MODULE_BYTES,
                Native.Length);
            Equal(
                Compilerˉimageˉstagingˉapplicationˉcontract.MODULE_SHA256,
                Convert.ToHexString(SHA256.HashData(Native)).ToLowerInvariant());
            Sequenceˉequal(Stageˉzero, Native);
            var Verified = Moduleˉcodec.Readˉandˉverify(Native);
            Equal(
                Compilerˉimageˉstagingˉapplicationˉcontract.MODULE_NAME,
                Verified.Module.Name);
            Equal(Moduleˉprofile.Hosted, Verified.Module.Profile);
            Equal(6, Verified.Module.Capabilities.Length);
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
