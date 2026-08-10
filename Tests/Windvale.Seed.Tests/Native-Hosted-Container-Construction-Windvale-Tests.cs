using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Windvaleˉnativeˉhostedˉcontainerˉconstructionˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));

        var Root = Source(
            "Linker/Windvale/Native-Hosted-Container-Construction-Core.wv",
            "Native-Hosted-Container-Construction-Core.wv");
        Sourceˉmoduleˉinput[] Dependencies =
        [
            Source("Foundation/Byte-Construction.wv", "Byte-Construction.wv"),
            Source(
                "Linker/Windvale/Native-Hosted-Container-Byte-Construction.wv",
                "Native-Hosted-Container-Byte-Construction.wv"),
            Source(
                "Linker/Windvale/Native-Hosted-Container-Layout.wv",
                "Native-Hosted-Container-Layout.wv"),
            Source(
                "Runtime/Windvale/Native-Hosted-Tool-Metadata-Admission.wv",
                "Native-Hosted-Tool-Metadata-Admission.wv"),
        ];
        var Compiled = Seedˉcompiler.Compileˉmodules(Root, Dependencies);
        True(Compiled.Success, string.Join(" | ", Compiled.Diagnostics));
        Equal(
            Nativeˉhostedˉcontainerˉconstructor.CONSUMER_CANONICAL_SIZE,
            Compiled.Moduleˉbytes.Length);
        Equal(
            Nativeˉhostedˉcontainerˉconstructor.CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Compiled.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Linker/Reference/Consumers/Native-Hosted-Container-Construction.wvb")));
        var Retainedˉartifact = Readˉembeddedˉnativeˉartifact(
            typeof(Nativeˉhostedˉcontainerˉconstructor),
            "Windvale.Linker.Native-Hosted-Container-Construction.wvnf");
        Equal(
            Nativeˉhostedˉcontainerˉconstructor.CONSUMER_ARTIFACT_CANONICAL_SIZE,
            Retainedˉartifact.Length);
        Equal(
            Nativeˉhostedˉcontainerˉconstructor.CONSUMER_ARTIFACT_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Retainedˉartifact.AsSpan()));
        False(
            typeof(Nativeˉhostedˉcontainerˉconstructor).Assembly
                .GetManifestResourceNames()
                .Contains(
                    "Windvale.Linker.Native-Hosted-Container-Construction.wvb",
                    StringComparer.Ordinal),
            "The normal linker embeds the hosted-container constructor WVB.");

        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;
        Sequenceˉequal(Retainedˉartifact, Nativeˉfragmentˉartifactˉcodec.Write(Native));
        True(Native.Requiredˉservices.IsEmpty, "The hosted-container constructor requires a service.");
        Equal(
            new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor),
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Native));
        var Reference = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);

        (ImmutableArray<byte> Bytes, Verifiedˉmodule Module, Referenceˉruntime Reference) Platform(
            string path,
            string resource,
            int expectedˉbytes,
            string expectedˉsha256,
            int expectedˉartifactˉbytes,
            string expectedˉartifactˉsha256,
            string retainedˉname,
            string artifactˉresource)
        {
            var Result = Seedˉcompiler.Compileˉmodules(
                Source(path, resource),
                [
                    Source("Foundation/Byte-Construction.wv", "Byte-Construction.wv"),
                    Source(
                        "Linker/Windvale/Native-Hosted-Container-Byte-Construction.wv",
                        "Native-Hosted-Container-Byte-Construction.wv"),
                ]);
            True(Result.Success, string.Join(" | ", Result.Diagnostics));
            Equal(expectedˉbytes, Result.Moduleˉbytes.Length);
            Equal(expectedˉsha256, Moduleˉdigest.Calculateˉsha256(Result.Moduleˉbytes.AsSpan()));
            Sequenceˉequal(
                Result.Moduleˉbytes,
                File.ReadAllBytes(Path.Combine(Repository, "Linker/Reference/Consumers", retainedˉname)));
            var Artifact = Readˉembeddedˉnativeˉartifact(
                typeof(Nativeˉhostedˉcontainerˉbytesˉconstructor),
                artifactˉresource);
            Equal(expectedˉartifactˉbytes, Artifact.Length);
            Equal(expectedˉartifactˉsha256, Moduleˉdigest.Calculateˉsha256(Artifact.AsSpan()));
            var Verified = Moduleˉcodec.Readˉandˉverify(Result.Moduleˉbytes.AsSpan());
            var Fragment = X64ˉnativeˉbackend.Compile(Verified).Fragment;
            Sequenceˉequal(Artifact, Nativeˉfragmentˉartifactˉcodec.Write(Fragment));
            return (
                Result.Moduleˉbytes,
                Verified,
                new Referenceˉruntime(
                    Verified,
                    new Referenceˉcapabilityˉhost(TextWriter.Null),
                    Runtimeˉoptions.Portableˉdefaults));
        }

        var Windowsˉplatform = Platform(
            "Linker/Windvale/Native-Hosted-Container-Windows.wv",
            "Native-Hosted-Container-Windows.wv",
            Nativeˉhostedˉcontainerˉbytesˉconstructor.WINDOWS_CANONICAL_SIZE,
            Nativeˉhostedˉcontainerˉbytesˉconstructor.WINDOWS_CANONICAL_SHA256,
            Nativeˉhostedˉcontainerˉbytesˉconstructor.WINDOWS_ARTIFACT_SIZE,
            Nativeˉhostedˉcontainerˉbytesˉconstructor.WINDOWS_ARTIFACT_SHA256,
            "Native-Hosted-Container-Windows.wvb",
            "Windvale.Linker.Native-Hosted-Container-Windows.wvnf");
        var Linuxˉplatform = Platform(
            "Linker/Windvale/Native-Hosted-Container-Linux.wv",
            "Native-Hosted-Container-Linux.wv",
            Nativeˉhostedˉcontainerˉbytesˉconstructor.LINUX_CANONICAL_SIZE,
            Nativeˉhostedˉcontainerˉbytesˉconstructor.LINUX_CANONICAL_SHA256,
            Nativeˉhostedˉcontainerˉbytesˉconstructor.LINUX_ARTIFACT_SIZE,
            Nativeˉhostedˉcontainerˉbytesˉconstructor.LINUX_ARTIFACT_SHA256,
            "Native-Hosted-Container-Linux.wvb",
            "Windvale.Linker.Native-Hosted-Container-Linux.wvnf");

        var Capabilities = Hostedˉtoolˉtestˉcapabilities();
        var Windowsˉbundle = Hostedˉtoolˉtestˉbundle(
            Consoleˉapplicationˉtarget.Windowsˉx64);
        var Linuxˉbundle = Hostedˉtoolˉtestˉbundle(
            Consoleˉapplicationˉtarget.Linuxˉx64);
        ImmutableArray<byte> Windowsˉplan = default;
        ImmutableArray<byte> Linuxˉplan = default;
        foreach (var Profile in Enum.GetValues<Hostedˉcompilerˉapplicationˉprofile>())
        {
            var Windowsˉruntime = Hostedˉcompilerˉruntimeˉdata.Build(
                Consoleˉapplicationˉtarget.Windowsˉx64,
                Capabilities,
                Windowsˉbundle,
                0,
                Profile);
            var Windowsˉlayout = Hostedˉcompilerˉruntimeˉdata.Plan(
                Consoleˉapplicationˉtarget.Windowsˉx64,
                Profile);
            var Expectedˉtextˉarenaˉbytes =
                Hostedˉcompilerˉapplicationˉmetadata.Usesˉlargeˉruntimeˉgeometry(Profile)
                    ? Hostedˉcompilerˉapplicationˉmetadata.LARGE_TOOL_TEXT_ARENA_BYTES
                    : Nativeˉconsoleˉapplicationˉcontract.HOSTED_TEXT_ARENA_BYTES;
            Equal(Expectedˉtextˉarenaˉbytes, Windowsˉlayout.Textˉarenaˉbytes);
            Equal(
                Hostedˉcompilerˉapplicationˉmetadata.Usesˉlargeˉruntimeˉgeometry(Profile)
                    ? Hostedˉcompilerˉruntimeˉdata.LARGE_TOOL_NAME_ARENA_STRIDE_BYTES
                    : Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_BYTES,
                Windowsˉlayout.Nameˉarenaˉstrideˉbytes);
            Equal(
                Hostedˉcompilerˉapplicationˉmetadata.Usesˉlargeˉruntimeˉgeometry(Profile)
                    ? 510_214_144u
                    : 476_135_424u,
                Windowsˉlayout.Virtualˉbytes);
            if (Hostedˉcompilerˉapplicationˉmetadata.Usesˉlargeˉruntimeˉgeometry(Profile))
            {
                Equal(237_051_904u, Windowsˉlayout.Nameˉarenaˉoffset);
                Equal(237_576_192u, Windowsˉlayout.Dataˉarenaˉoffset);
                Equal(506_011_648u, Windowsˉlayout.Fileˉinputˉscratchˉoffset);
                Equal(508_112_896u, Windowsˉlayout.Fileˉoutputˉscratchˉoffset);
            }
            var Windowsˉrequest = Nativeˉhostedˉcontainerˉconstructor.Buildˉrequest(
                Consoleˉapplicationˉtarget.Windowsˉx64,
                Profile,
                Windowsˉbundle,
                0,
                Windowsˉruntime);
            var Windowsˉresponse = Nativeˉhostedˉcontainerˉconstructor.Execute(Windowsˉrequest);
            Sequenceˉequal(Reference.Runˉmainˉbytes(Windowsˉrequest).Bytes, Windowsˉresponse);
            if (Profile == Hostedˉcompilerˉapplicationˉprofile.Compiler)
            {
                Windowsˉplan = Windowsˉresponse;
                Sequenceˉequal(
                    Windowsˉplatform.Reference.Runˉmainˉbytes(Windowsˉresponse).Bytes,
                    Nativeˉhostedˉcontainerˉbytesˉconstructor.Execute(
                        Consoleˉapplicationˉtarget.Windowsˉx64,
                        Windowsˉresponse));
            }
            var Windowsˉapplication = Nativeˉhostedˉcontainerˉconstructor.Materialize(
                Consoleˉapplicationˉtarget.Windowsˉx64,
                Profile,
                Windowsˉbundle,
                0,
                Windowsˉruntime,
                Windowsˉrequest,
                Windowsˉresponse);
            Sequenceˉequal(
                Windowsˉhostedˉcompilerˉapplicationˉbuilder.Buildˉstage0(
                    Capabilities,
                    Windowsˉbundle,
                    0,
                    Profile),
                Windowsˉapplication);
            _ = Windowsˉhostedˉcompilerˉapplicationˉverifier.Verify(
                Windowsˉapplication.AsSpan(),
                Windowsˉbundle,
                Profile);

            var Linuxˉruntime = Hostedˉcompilerˉruntimeˉdata.Build(
                Consoleˉapplicationˉtarget.Linuxˉx64,
                Capabilities,
                Linuxˉbundle,
                0,
                Profile);
            var Linuxˉlayout = Hostedˉcompilerˉruntimeˉdata.Plan(
                Consoleˉapplicationˉtarget.Linuxˉx64,
                Profile);
            Equal(Expectedˉtextˉarenaˉbytes, Linuxˉlayout.Textˉarenaˉbytes);
            Equal(
                Hostedˉcompilerˉapplicationˉmetadata.Usesˉlargeˉruntimeˉgeometry(Profile)
                    ? 508_116_992u
                    : 474_038_272u,
                Linuxˉlayout.Virtualˉbytes);
            if (Hostedˉcompilerˉapplicationˉmetadata.Usesˉlargeˉruntimeˉgeometry(Profile))
            {
                Equal(237_051_904u, Linuxˉlayout.Nameˉarenaˉoffset);
                Equal(237_576_192u, Linuxˉlayout.Dataˉarenaˉoffset);
                Equal(506_011_648u, Linuxˉlayout.Fileˉinputˉscratchˉoffset);
                Equal(507_064_320u, Linuxˉlayout.Fileˉoutputˉscratchˉoffset);
            }
            var Linuxˉrequest = Nativeˉhostedˉcontainerˉconstructor.Buildˉrequest(
                Consoleˉapplicationˉtarget.Linuxˉx64,
                Profile,
                Linuxˉbundle,
                0,
                Linuxˉruntime);
            var Linuxˉresponse = Nativeˉhostedˉcontainerˉconstructor.Execute(Linuxˉrequest);
            Sequenceˉequal(Reference.Runˉmainˉbytes(Linuxˉrequest).Bytes, Linuxˉresponse);
            if (Profile == Hostedˉcompilerˉapplicationˉprofile.Compiler)
            {
                Linuxˉplan = Linuxˉresponse;
                Sequenceˉequal(
                    Linuxˉplatform.Reference.Runˉmainˉbytes(Linuxˉresponse).Bytes,
                    Nativeˉhostedˉcontainerˉbytesˉconstructor.Execute(
                        Consoleˉapplicationˉtarget.Linuxˉx64,
                        Linuxˉresponse));
            }
            var Linuxˉapplication = Nativeˉhostedˉcontainerˉconstructor.Materialize(
                Consoleˉapplicationˉtarget.Linuxˉx64,
                Profile,
                Linuxˉbundle,
                0,
                Linuxˉruntime,
                Linuxˉrequest,
                Linuxˉresponse);
            Sequenceˉequal(
                Linuxˉhostedˉcompilerˉapplicationˉbuilder.Buildˉstage0(
                    Capabilities,
                    Linuxˉbundle,
                    0,
                    Profile),
                Linuxˉapplication);
            _ = Linuxˉhostedˉcompilerˉapplicationˉverifier.Verify(
                Linuxˉapplication.AsSpan(),
                Linuxˉbundle,
                Profile);
        }

        var Runtime = Hostedˉcompilerˉruntimeˉdata.Build(
            Consoleˉapplicationˉtarget.Windowsˉx64,
            Capabilities,
            Windowsˉbundle,
            0,
            Hostedˉcompilerˉapplicationˉprofile.Compiler);
        var Request = Nativeˉhostedˉcontainerˉconstructor.Buildˉrequest(
            Consoleˉapplicationˉtarget.Windowsˉx64,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            Windowsˉbundle,
            0,
            Runtime);

        static ImmutableArray<byte> Replaceˉu32(
            ImmutableArray<byte> input,
            int offset,
            uint value)
        {
            var Result = input.ToArray();
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(offset), value);
            return Result.ToImmutableArray();
        }
        static ImmutableArray<byte> Mutateˉbyte(ImmutableArray<byte> input, int offset)
        {
            var Result = input.ToArray();
            Result[offset] ^= 1;
            return Result.ToImmutableArray();
        }
        void Expectˉfailure(ImmutableArray<byte> request, uint status, uint offset)
        {
            var Interpreted = Reference.Runˉmainˉbytes(request).Bytes;
            var Executed = Nativeˉhostedˉcontainerˉconstructor.Execute(request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(128, Executed.Length);
            Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(offset, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
        }

        Expectˉfailure(Request[..31], 1, 31);
        Expectˉfailure(Request.Add(0), 1, 8);
        Expectˉfailure(Replaceˉu32(Request, 0, 0), 2, 0);
        Expectˉfailure(Replaceˉu32(Request, 4, 2), 3, 4);
        Expectˉfailure(Replaceˉu32(Request, 8, 0), 1, 8);
        Expectˉfailure(Replaceˉu32(Request, 12, 0), 4, 12);
        Expectˉfailure(Replaceˉu32(Request, 16, 0), 4, 12);
        Expectˉfailure(Replaceˉu32(Request, 20, 0), 4, 12);
        Expectˉfailure(Replaceˉu32(Request, 28, 1), 4, 12);
        Expectˉfailure(Replaceˉu32(Request, 24, 1), 4, 12);
        Expectˉfailure(Mutateˉbyte(Request, 32 + 480), 5, 32 + 480);
        Expectˉfailure(Replaceˉu32(Request, 32 + 56, 0), 5, 32 + 56);
        Expectˉfailure(Replaceˉu32(Request, 32 + 304, 0), 5, 32 + 304);
        Expectˉfailure(Replaceˉu32(Request, 32 + 336, 0), 5, 32 + 336);
        Expectˉfailure(Replaceˉu32(Request, 32 + 424, 0), 5, 32 + 424);

        void Expectˉrelayˉfailure(ImmutableArray<byte> plan)
        {
            Throwsˉinvalidˉoperation(
                "The Windvale hosted-container construction response is invalid.",
                () => _ = Nativeˉhostedˉcontainerˉconstructor.Materialize(
                    Consoleˉapplicationˉtarget.Windowsˉx64,
                    Hostedˉcompilerˉapplicationˉprofile.Compiler,
                    Windowsˉbundle,
                    0,
                    Runtime,
                    Request,
                    plan));
        }
        Expectˉrelayˉfailure(Replaceˉu32(Windowsˉplan, 28, uint.MaxValue));
        Expectˉrelayˉfailure(Replaceˉu32(Windowsˉplan, 48, 512));
        Expectˉrelayˉfailure(Replaceˉu32(Windowsˉplan, 64, 512));
        Expectˉrelayˉfailure(Replaceˉu32(Windowsˉplan, 84, 0));

        void Expectˉplatformˉfailure(
            Consoleˉapplicationˉtarget target,
            Referenceˉruntime reference,
            ImmutableArray<byte> plan)
        {
            var Interpreted = reference.Runˉmainˉbytes(plan).Bytes;
            var Executed = Nativeˉhostedˉcontainerˉbytesˉconstructor.Execute(target, plan);
            Sequenceˉequal(Interpreted, Executed);
            Equal(32, Executed.Length);
            Equal(1u, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
        }
        Expectˉplatformˉfailure(
            Consoleˉapplicationˉtarget.Windowsˉx64,
            Windowsˉplatform.Reference,
            Windowsˉplan[..^1]);
        Expectˉplatformˉfailure(
            Consoleˉapplicationˉtarget.Windowsˉx64,
            Windowsˉplatform.Reference,
            Windowsˉplan.Add(0));
        Expectˉplatformˉfailure(
            Consoleˉapplicationˉtarget.Windowsˉx64,
            Windowsˉplatform.Reference,
            Mutateˉbyte(Windowsˉplan, 0));
        Expectˉplatformˉfailure(
            Consoleˉapplicationˉtarget.Windowsˉx64,
            Windowsˉplatform.Reference,
            Replaceˉu32(Windowsˉplan, 124, 0));
        Expectˉplatformˉfailure(
            Consoleˉapplicationˉtarget.Linuxˉx64,
            Linuxˉplatform.Reference,
            Linuxˉplan[..^1]);
        Expectˉplatformˉfailure(
            Consoleˉapplicationˉtarget.Linuxˉx64,
            Linuxˉplatform.Reference,
            Linuxˉplan.Add(0));
        Expectˉplatformˉfailure(
            Consoleˉapplicationˉtarget.Linuxˉx64,
            Linuxˉplatform.Reference,
            Mutateˉbyte(Linuxˉplan, 20));
        Expectˉplatformˉfailure(
            Consoleˉapplicationˉtarget.Linuxˉx64,
            Linuxˉplatform.Reference,
            Replaceˉu32(Linuxˉplan, 124, 0));

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-hosted-container-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Native-Hosted-Container.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Container-Construction.wvproj"),
                Nativeˉpath);
            Equal(0, Nativeˉbuild.Exitˉcode);
            Equal(string.Empty, Nativeˉbuild.Error);
            Sequenceˉequal(Compiled.Moduleˉbytes, File.ReadAllBytes(Nativeˉpath));
            var Windowsˉnativeˉpath = Path.Combine(Directoryˉpath, "Windows.wvb");
            var Windowsˉnativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(Repository, "Windvale-Native-Hosted-Container-Windows.wvproj"),
                Windowsˉnativeˉpath);
            Equal(0, Windowsˉnativeˉbuild.Exitˉcode);
            Equal(string.Empty, Windowsˉnativeˉbuild.Error);
            Sequenceˉequal(
                Windowsˉplatform.Bytes,
                File.ReadAllBytes(Windowsˉnativeˉpath));
            var Linuxˉnativeˉpath = Path.Combine(Directoryˉpath, "Linux.wvb");
            var Linuxˉnativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(Repository, "Windvale-Native-Hosted-Container-Linux.wvproj"),
                Linuxˉnativeˉpath);
            Equal(0, Linuxˉnativeˉbuild.Exitˉcode);
            Equal(string.Empty, Linuxˉnativeˉbuild.Error);
            Sequenceˉequal(
                Linuxˉplatform.Bytes,
                File.ReadAllBytes(Linuxˉnativeˉpath));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
