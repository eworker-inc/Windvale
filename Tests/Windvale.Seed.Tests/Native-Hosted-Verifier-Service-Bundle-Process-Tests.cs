using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_HOSTED_VERIFIER_BUNDLE_REQUEST_BYTES = 21_323;
    private const string NATIVE_HOSTED_VERIFIER_BUNDLE_REQUEST_SHA256 =
        "bc1afc45e407d08c7a42073224f7d839bae6562d21f7f28ecf82e1980c388e06";

    private static void Nativeˉhostedˉverifierˉserviceˉbundleˉprocessˉruns()
    {
        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-verifier-bundle-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Toolˉpath = Path.Combine(Directoryˉpath, "Bundle-Request.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Verifier-Service-Bundle-Request-Tool.wvproj"),
                Toolˉpath);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            var Toolˉbytes = File.ReadAllBytes(Toolˉpath);
            Equal(NATIVE_HOSTED_VERIFIER_BUNDLE_REQUEST_BYTES, Toolˉbytes.Length);
            Equal(
                NATIVE_HOSTED_VERIFIER_BUNDLE_REQUEST_SHA256,
                Moduleˉdigest.Calculateˉsha256(Toolˉbytes));
            var Tool = Moduleˉcodec.Readˉandˉverify(Toolˉbytes);
            var Nativeˉtool = X64ˉnativeˉbackend.Compile(Tool).Fragment;
            Equal(
                new Nativeˉentryˉshape(
                    Nativeˉentryˉinputˉkind.None,
                    Nativeˉentryˉresultˉkind.Scalar),
                Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Nativeˉtool));

            var Verifierˉbytes = File.ReadAllBytes(Path.Combine(
                Repository,
                "Artifacts",
                "Native-Front-Door",
                "Wvb",
                "Compiler-Wvb-Verifier.wvb"));
            var Verifier = Moduleˉcodec.Readˉandˉverify(Verifierˉbytes);
            var Verifierˉfragment = X64ˉnativeˉbackend.Compile(Verifier).Fragment;

            int Run(ImmutableArray<string> arguments)
            {
                var Resources = new Hostedˉresourceˉcontext(
                    arguments,
                    TextWriter.Null,
                    TextWriter.Null);
                var Services = new Nativeˉhostˉservices(
                    Nativeˉoutputˉchannel.Processˉstandardˉoutput(),
                    Tool.Module.Capabilities.Select(Item => Item.Name),
                    Resources,
                    Nativeˉoutputˉchannel.Processˉdiagnosticˉoutput(),
                    Nativeˉfileˉinput.Hostˉfileˉsystem(),
                    Nativeˉfileˉoutput.Hostˉfileˉsystem());
                return X64ˉnativeˉexecutor.Executeˉi32(
                    Nativeˉtool,
                    maximumˉinstructions: 48_000_000_000,
                    hostˉservices: Services);
            }

            foreach (var Platform in Enum.GetValues<Nativeˉserviceˉplatform>())
            {
                var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉverifier(
                    Verifierˉfragment,
                    Platform);
                Equal(6, Bundle.Placements.Length);
                var Fragment = Bundle.Imageˉbytes[..Bundle.Nativeˉimageˉbytes];
                var Serviceˉcode = Bundle.Placements.Select(Placement =>
                    new Nativeˉserviceˉcode(
                        Placement.Service,
                        Placement.Adapter,
                        Bundle.Imageˉbytes[
                            Placement.Imageˉoffset..
                            (Placement.Imageˉoffset + Placement.Codeˉbytes)]))
                    .ToImmutableArray();
                var Services = Serviceˉcode.Select(Service =>
                    new Nativeˉpublicationˉservice(
                        Service.Service,
                        Service.Code.Length)).ToImmutableArray();
                var Plan = X64ˉnativeˉpublicationˉlayout.Plan(
                    Fragment.Length,
                    Services);
                var Expected =
                    Nativeˉserviceˉbundleˉmaterializationˉsession.Buildˉrequest(
                        Fragment,
                        Serviceˉcode,
                        Plan,
                        0);

                var Prefix = Path.Combine(
                    Directoryˉpath,
                    $"{(uint)Platform}-Source-");
                var Paths = Enumerable.Range(0, 7)
                    .Select(Index => Prefix + Index)
                    .ToArray();
                var Outputˉpath = Path.Combine(
                    Directoryˉpath,
                    $"{(uint)Platform}-Request.wvsq");
                File.WriteAllBytes(Paths[0], Fragment.AsSpan());
                for (var Index = 0; Index < Serviceˉcode.Length; Index++)
                {
                    File.WriteAllBytes(Paths[Index + 1], Serviceˉcode[Index].Code.AsSpan());
                }

                Equal(0, Run([.. Paths, Outputˉpath]));
                var Actual = File.ReadAllBytes(Outputˉpath).ToImmutableArray();
                Sequenceˉequal(Expected, Actual);
                var Response =
                    X64ˉnativeˉserviceˉbundleˉmaterialization.Buildˉwithˉwindvale(
                        Actual);
                var Image =
                    Nativeˉserviceˉbundleˉmaterializationˉsession.Verifyˉresponse(
                        Fragment,
                        Serviceˉcode,
                        Plan,
                        0,
                        Actual.Length,
                        Response);
                Sequenceˉequal(Bundle.Imageˉbytes, Image);

                byte[] Sentinel = [0x57, 0x56, 0x53, 0x51];
                File.WriteAllBytes(Outputˉpath, Sentinel);
                Equal(2, Run([
                    Paths[0], Paths[1], Paths[1], Paths[3],
                    Paths[4], Paths[5], Paths[6], Outputˉpath,
                ]));
                Sequenceˉequal(Sentinel, File.ReadAllBytes(Outputˉpath));
                Equal(64, Run([
                    Paths[0], Paths[1], Paths[2], Paths[3],
                    Paths[4], Paths[5], Paths[6], Paths[0],
                ]));
                Sequenceˉequal(Fragment, File.ReadAllBytes(Paths[0]));
            }
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
