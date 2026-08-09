using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_HOSTED_VERIFIER_REQUEST_TOOL_BYTES = 17_010;
    private const string NATIVE_HOSTED_VERIFIER_REQUEST_TOOL_SHA256 =
        "955907aa104c057d89071ee386d00913c05077bc4463c88a6d14547bd0539fad";

    private static void Nativeˉhostedˉverifierˉevidenceˉprocessesˉrun()
    {
        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-wvhv-evidence-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            byte[] Build(string project, string name)
            {
                var Pathˉvalue = Path.Combine(Directoryˉpath, name);
                var Result = Runˉnativeˉfrontˉdoor(
                    Repository,
                    Path.Combine(Repository, project),
                    Pathˉvalue);
                Equal(0, Result.Exitˉcode);
                Equal(string.Empty, Result.Error);
                return File.ReadAllBytes(Pathˉvalue);
            }

            var Evidenceˉtoolˉbytes = Build(
                "Windvale-Native-Hosted-Tool-Metadata-Request.wvproj",
                "Evidence-Tool.wvb");
            Equal(
                Hostedˉmetadataˉrequestˉapplicationˉcontract.MODULE_BYTES,
                Evidenceˉtoolˉbytes.Length);
            Equal(
                Hostedˉmetadataˉrequestˉapplicationˉcontract.MODULE_SHA256,
                Moduleˉdigest.Calculateˉsha256(Evidenceˉtoolˉbytes));
            var Requestˉtoolˉbytes = Build(
                "Windvale-Native-Hosted-Verifier-Metadata-Request-Tool.wvproj",
                "Request-Tool.wvb");
            Equal(NATIVE_HOSTED_VERIFIER_REQUEST_TOOL_BYTES, Requestˉtoolˉbytes.Length);
            Equal(
                NATIVE_HOSTED_VERIFIER_REQUEST_TOOL_SHA256,
                Moduleˉdigest.Calculateˉsha256(Requestˉtoolˉbytes));

            var Evidenceˉmodule = Moduleˉcodec.Readˉandˉverify(Evidenceˉtoolˉbytes);
            var Requestˉmodule = Moduleˉcodec.Readˉandˉverify(Requestˉtoolˉbytes);
            var Evidenceˉnative = X64ˉnativeˉbackend.Compile(Evidenceˉmodule).Fragment;
            var Requestˉnative = X64ˉnativeˉbackend.Compile(Requestˉmodule).Fragment;
            Equal(
                new Nativeˉentryˉshape(
                    Nativeˉentryˉinputˉkind.None,
                    Nativeˉentryˉresultˉkind.Scalar),
                Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Evidenceˉnative));
            Equal(
                new Nativeˉentryˉshape(
                    Nativeˉentryˉinputˉkind.None,
                    Nativeˉentryˉresultˉkind.Scalar),
                Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Requestˉnative));

            int Run(
                Verifiedˉmodule module,
                Nativeˉfragment fragment,
                ImmutableArray<string> arguments)
            {
                var Resources = new Hostedˉresourceˉcontext(
                    arguments,
                    TextWriter.Null,
                    TextWriter.Null);
                var Services = new Nativeˉhostˉservices(
                    Nativeˉoutputˉchannel.Processˉstandardˉoutput(),
                    module.Module.Capabilities.Select(Item => Item.Name),
                    Resources,
                    Nativeˉoutputˉchannel.Processˉdiagnosticˉoutput(),
                    Nativeˉfileˉinput.Hostˉfileˉsystem(),
                    Nativeˉfileˉoutput.Hostˉfileˉsystem());
                return X64ˉnativeˉexecutor.Executeˉi32(
                    fragment,
                    maximumˉinstructions: 48_000_000_000,
                    hostˉservices: Services);
            }

            var Verifierˉbytes = File.ReadAllBytes(Path.Combine(
                Repository,
                "Artifacts",
                "Native-Front-Door",
                "Wvb",
                "Compiler-Wvb-Verifier.wvb"));
            var Verifier = Moduleˉcodec.Readˉandˉverify(Verifierˉbytes);
            var Verifierˉfragment = X64ˉnativeˉbackend.Compile(Verifier).Fragment;
            var Nativeˉentry = Verifierˉfragment.Symbols.Single(Symbol =>
                Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
                Symbol.Kind == Nativeˉsymbolˉkind.Function &&
                Symbol.Name == "Main").Offset;

            foreach (var Target in Enum.GetValues<Consoleˉapplicationˉtarget>())
            {
                var Platform = Target == Consoleˉapplicationˉtarget.Windowsˉx64
                    ? Nativeˉserviceˉplatform.Windows
                    : Nativeˉserviceˉplatform.Linux;
                var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉverifier(
                    Verifierˉfragment,
                    Platform);
                var Services = Bundle.Placements.Select(Placement =>
                    new Nativeˉpublicationˉservice(
                        Placement.Service,
                        Placement.Codeˉbytes)).ToImmutableArray();
                var Plan = X64ˉnativeˉpublicationˉlayout.Buildˉrequest(
                    Bundle.Nativeˉimageˉbytes,
                    Services);
                var Fragment = Bundle.Imageˉbytes[..Bundle.Nativeˉimageˉbytes].ToArray();
                var Fragmentˉresources = Enumerable.Range(0, 8)
                    .Select(Index => Fragment[
                        (Index * Fragment.Length / 8)..
                        ((Index + 1) * Fragment.Length / 8)])
                    .ToArray();
                var Serviceˉresources = Bundle.Placements.Select(Placement =>
                    Bundle.Imageˉbytes[
                        Placement.Imageˉoffset..
                        (Placement.Imageˉoffset + Placement.Codeˉbytes)].ToArray());
                var Sourceˉresources = Fragmentˉresources
                    .Concat(Serviceˉresources)
                    .ToArray();
                var Regions = new (int Offset, int Bytes)[7];
                var Logicalˉoffset = 0;
                Regions[0] = (0, Bundle.Nativeˉimageˉbytes);
                Logicalˉoffset += Bundle.Nativeˉimageˉbytes;
                for (var Index = 0; Index < Bundle.Placements.Length; Index++)
                {
                    Regions[Index + 1] = (
                        Logicalˉoffset,
                        Bundle.Placements[Index].Codeˉbytes);
                    Logicalˉoffset += Bundle.Placements[Index].Codeˉbytes;
                }
                var Manifest = Buildˉmetadataˉrequestˉmanifest(
                    Sourceˉresources,
                    Regions);

                var Inputs = new byte[32];
                BinaryPrimitives.WriteUInt32LittleEndian(Inputs, 1230394967);
                BinaryPrimitives.WriteUInt32LittleEndian(Inputs.AsSpan(4), 1);
                BinaryPrimitives.WriteUInt32LittleEndian(Inputs.AsSpan(8), 32);
                BinaryPrimitives.WriteUInt32LittleEndian(
                    Inputs.AsSpan(12),
                    (uint)Target);
                BinaryPrimitives.WriteUInt32LittleEndian(Inputs.AsSpan(16), 2);
                BinaryPrimitives.WriteUInt32LittleEndian(
                    Inputs.AsSpan(20),
                    Nativeˉentry);

                var Suffix = ((uint)Target).ToString();
                var Inputsˉpath = Path.Combine(Directoryˉpath, $"Inputs-{Suffix}.wvvi");
                var Planˉpath = Path.Combine(Directoryˉpath, $"Plan-{Suffix}.wvpq");
                var Manifestˉpath = Path.Combine(Directoryˉpath, $"Sources-{Suffix}.wvhs");
                var Prefix = Path.Combine(Directoryˉpath, $"Sources-{Suffix}");
                var Evidenceˉpath = Path.Combine(Directoryˉpath, $"Evidence-{Suffix}.wvve");
                var Requestˉpath = Path.Combine(Directoryˉpath, $"Request-{Suffix}.wvvr");
                File.WriteAllBytes(Inputsˉpath, Inputs);
                File.WriteAllBytes(Planˉpath, Plan.AsSpan());
                File.WriteAllBytes(Manifestˉpath, Manifest);
                for (var Index = 0; Index < Sourceˉresources.Length; Index++)
                {
                    File.WriteAllBytes(Prefix + $".chunk-{Index}", Sourceˉresources[Index]);
                }

                Equal(0, Run(Evidenceˉmodule, Evidenceˉnative,
                    [Inputsˉpath, Planˉpath, Manifestˉpath, Prefix, Evidenceˉpath]));
                Equal(352, File.ReadAllBytes(Evidenceˉpath).Length);
                Equal(0, Run(Requestˉmodule, Requestˉnative,
                    [Evidenceˉpath, Requestˉpath]));
                Sequenceˉequal(
                    Buildˉhostedˉverifierˉmetadataˉrequest(
                        Target,
                        Bundle,
                        Nativeˉentry),
                    File.ReadAllBytes(Requestˉpath));
            }
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
