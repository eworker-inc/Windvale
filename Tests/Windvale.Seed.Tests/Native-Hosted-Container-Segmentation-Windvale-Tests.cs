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
    private static void Windvaleˉnativeˉhostedˉcontainerˉsegmentationˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));

        var Compiled = Seedˉcompiler.Compileˉmodules(
            Source(
                "Linker/Windvale/Native-Hosted-Container-Segmentation.wv",
                "Native-Hosted-Container-Segmentation.wv"),
            [
                Source("Foundation/Byte-Construction.wv", "Byte-Construction.wv"),
                Source(
                    "Linker/Windvale/Native-Hosted-Container-Byte-Construction.wv",
                    "Native-Hosted-Container-Byte-Construction.wv"),
                Source(
                    "Linker/Windvale/Native-Hosted-Container-Layout.wv",
                    "Native-Hosted-Container-Layout.wv"),
            ]);
        True(Compiled.Success, string.Join(" | ", Compiled.Diagnostics));
        Equal(
            Nativeˉhostedˉcontainerˉsegmentˉconstructor.CONSUMER_CANONICAL_SIZE,
            Compiled.Moduleˉbytes.Length);
        Equal(
            Nativeˉhostedˉcontainerˉsegmentˉconstructor.CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Compiled.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Linker/Reference/Consumers/Native-Hosted-Container-Segmentation.wvb")));
        var Retainedˉartifact = Readˉembeddedˉnativeˉartifact(
            typeof(Nativeˉhostedˉcontainerˉsegmentˉconstructor),
            "Windvale.Linker.Native-Hosted-Container-Segmentation.wvnf");
        Equal(
            Nativeˉhostedˉcontainerˉsegmentˉconstructor.CONSUMER_ARTIFACT_SIZE,
            Retainedˉartifact.Length);
        Equal(
            Nativeˉhostedˉcontainerˉsegmentˉconstructor.CONSUMER_ARTIFACT_SHA256,
            Moduleˉdigest.Calculateˉsha256(Retainedˉartifact.AsSpan()));
        False(
            typeof(Nativeˉhostedˉcontainerˉsegmentˉconstructor).Assembly
                .GetManifestResourceNames()
                .Contains(
                    "Windvale.Linker.Native-Hosted-Container-Segmentation.wvb",
                    StringComparer.Ordinal),
            "The normal linker embeds the hosted-container segmentation WVB.");

        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;
        Sequenceˉequal(Retainedˉartifact, Nativeˉfragmentˉartifactˉcodec.Write(Native));
        var Reference = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);
        var Capabilities = Hostedˉtoolˉtestˉcapabilities();
        ImmutableArray<byte> Malformedˉrequest = default;

        foreach (var Target in Enum.GetValues<Consoleˉapplicationˉtarget>())
        {
            var Bundle = Hostedˉtoolˉtestˉbundle(Target);
            var Runtime = Hostedˉcompilerˉruntimeˉdata.Build(
                Target,
                Capabilities,
                Bundle,
                0,
                Hostedˉcompilerˉapplicationˉprofile.Compiler);
            var Plannerˉrequest = Nativeˉhostedˉcontainerˉconstructor.Buildˉrequest(
                Target,
                Hostedˉcompilerˉapplicationˉprofile.Compiler,
                Bundle,
                0,
                Runtime);
            var Plan = Nativeˉhostedˉcontainerˉconstructor.Execute(Plannerˉrequest);
            uint Read(int offset) =>
                BinaryPrimitives.ReadUInt32LittleEndian(Plan.AsSpan()[offset..]);
            var Header = Enumerable.Repeat((byte)0x11, checked((int)Read(36)))
                .ToImmutableArray();
            var Startup = Enumerable.Repeat((byte)0x22, checked((int)Read(44)))
                .ToImmutableArray();
            var Imports = Enumerable.Repeat((byte)0x33, checked((int)Read(60)))
                .ToImmutableArray();
            var Relocation = Enumerable.Repeat((byte)0x44, checked((int)Read(76)))
                .ToImmutableArray();
            var Requests = Nativeˉhostedˉcontainerˉmaterializationˉsession.Buildˉrequests(
                Plan,
                Header,
                Startup,
                Bundle.Imageˉbytes,
                Imports,
                Runtime,
                Relocation);
            Equal(1, Requests.Length);
            var Interpreted = Reference.Runˉmainˉbytes(Requests[0]).Bytes;
            var Executed = Nativeˉhostedˉcontainerˉsegmentˉconstructor.Execute(Requests[0]);
            Sequenceˉequal(Interpreted, Executed);
            var Segment = Nativeˉhostedˉcontainerˉmaterializationˉsession.Verifyˉresponse(
                Plan,
                Header,
                Startup,
                Bundle.Imageˉbytes,
                Imports,
                Runtime,
                Relocation,
                0,
                Requests[0].Length,
                Executed);
            Sequenceˉequal(
                Segment,
                Nativeˉhostedˉcontainerˉmaterializationˉsession.Build(
                    Plan,
                    Header,
                    Startup,
                    Bundle.Imageˉbytes,
                    Imports,
                    Runtime,
                    Relocation));
            if (Target == Consoleˉapplicationˉtarget.Windowsˉx64)
            {
                Malformedˉrequest = Requests[0];
            }
        }

        var Baseˉbundle = Hostedˉtoolˉtestˉbundle(
            Consoleˉapplicationˉtarget.Windowsˉx64);
        var Largeˉimageˉarray = new byte[
            Nativeˉhostedˉcontainerˉmaterializationˉsession.MAXIMUM_SEGMENT_BYTES + 512];
        Baseˉbundle.Imageˉbytes.CopyTo(Largeˉimageˉarray);
        Largeˉimageˉarray.AsSpan(Baseˉbundle.Imageˉbytes.Length).Fill(0x5A);
        var Largeˉbundle = Baseˉbundle with
        {
            Imageˉbytes = Largeˉimageˉarray.ToImmutableArray(),
        };
        var Largeˉruntime = Hostedˉcompilerˉruntimeˉdata.Build(
            Consoleˉapplicationˉtarget.Windowsˉx64,
            Capabilities,
            Largeˉbundle,
            0,
            Hostedˉcompilerˉapplicationˉprofile.Compiler);
        var Largeˉplan = Nativeˉhostedˉcontainerˉconstructor.Execute(
            Nativeˉhostedˉcontainerˉconstructor.Buildˉrequest(
                Consoleˉapplicationˉtarget.Windowsˉx64,
                Hostedˉcompilerˉapplicationˉprofile.Compiler,
                Largeˉbundle,
                0,
                Largeˉruntime));
        uint Readˉlarge(int offset) =>
            BinaryPrimitives.ReadUInt32LittleEndian(Largeˉplan.AsSpan()[offset..]);
        var Largeˉheader = Enumerable.Repeat((byte)0x11, checked((int)Readˉlarge(36)))
            .ToImmutableArray();
        var Largeˉstartup = Enumerable.Repeat((byte)0x22, checked((int)Readˉlarge(44)))
            .ToImmutableArray();
        var Largeˉimports = Enumerable.Repeat((byte)0x33, checked((int)Readˉlarge(60)))
            .ToImmutableArray();
        var Largeˉrelocation = Enumerable.Repeat((byte)0x44, checked((int)Readˉlarge(76)))
            .ToImmutableArray();
        var Largeˉrequests =
            Nativeˉhostedˉcontainerˉmaterializationˉsession.Buildˉrequests(
                Largeˉplan,
                Largeˉheader,
                Largeˉstartup,
                Largeˉbundle.Imageˉbytes,
                Largeˉimports,
                Largeˉruntime,
                Largeˉrelocation);
        Equal(2, Largeˉrequests.Length);
        True(
            Largeˉrequests.All(Request => Request.Length <= Bytecodeˉlimits.MAX_BYTE_DATA_BYTES),
            "A hosted-container segment request exceeded the byte-value limit.");
        var Largeˉapplication = Nativeˉhostedˉcontainerˉmaterializationˉsession.Build(
            Largeˉplan,
            Largeˉheader,
            Largeˉstartup,
            Largeˉbundle.Imageˉbytes,
            Largeˉimports,
            Largeˉruntime,
            Largeˉrelocation);
        Equal(checked((int)Readˉlarge(28)), Largeˉapplication.Length);
        Sequenceˉequal(
            Largeˉbundle.Imageˉbytes,
            Largeˉapplication.AsSpan(
                checked((int)Readˉlarge(48)),
                Largeˉbundle.Imageˉbytes.Length).ToArray());

        static ImmutableArray<byte> Replaceˉu32(
            ImmutableArray<byte> input,
            int offset,
            uint value)
        {
            var Result = input.ToArray();
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(offset), value);
            return Result.ToImmutableArray();
        }
        void Expectˉfailure(ImmutableArray<byte> request, uint status, uint offset)
        {
            var Interpreted = Reference.Runˉmainˉbytes(request).Bytes;
            var Executed = Nativeˉhostedˉcontainerˉsegmentˉconstructor.Execute(request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(40, Executed.Length);
            Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(offset, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
        }
        Expectˉfailure(Malformedˉrequest[..159], 1, 159);
        Expectˉfailure(Malformedˉrequest.Add(0), 1, 8);
        Expectˉfailure(Replaceˉu32(Malformedˉrequest, 0, 0), 2, 0);
        Expectˉfailure(Replaceˉu32(Malformedˉrequest, 4, 2), 3, 4);
        Expectˉfailure(Replaceˉu32(Malformedˉrequest, 12, 0), 4, 12);
        Expectˉfailure(Replaceˉu32(Malformedˉrequest, 24, 0), 4, 24);
        Expectˉfailure(Replaceˉu32(Malformedˉrequest, 32 + 28, 0), 5, 32);
        Expectˉfailure(Replaceˉu32(Malformedˉrequest, 16, 1), 6, 16);

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-hosted-segmentation-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Output = Path.Combine(Directoryˉpath, "Segmentation.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Container-Segmentation.wvproj"),
                Output);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            Sequenceˉequal(Compiled.Moduleˉbytes, File.ReadAllBytes(Output));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
