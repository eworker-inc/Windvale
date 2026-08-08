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
    private static void Windvaleˉnativeˉhostedˉstartupˉinstantiationˉruns()
    {
        var Coreˉinput = new Sourceˉmoduleˉinput(
            "Linker/Windvale/Native-Hosted-Startup-Instantiation-Core.wv",
            Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Native-Hosted-Startup-Instantiation-Core.wv"));
        var Coreˉresult = Seedˉcompiler.Compileˉmodules(Coreˉinput, []);
        True(Coreˉresult.Success, string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(
            Nativeˉhostedˉstartupˉinstantiator.CONSUMER_CANONICAL_SIZE,
            Coreˉresult.Moduleˉbytes.Length);
        Equal(
            Nativeˉhostedˉstartupˉinstantiator.CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Coreˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Linker/Reference/Consumers/Native-Hosted-Startup-Instantiation.wvb")));
        var Retainedˉartifact = Readˉembeddedˉnativeˉartifact(
            typeof(Nativeˉhostedˉstartupˉinstantiator),
            "Windvale.Linker.Native-Hosted-Startup-Instantiation.wvnf");
        Equal(
            Nativeˉhostedˉstartupˉinstantiator.CONSUMER_ARTIFACT_CANONICAL_SIZE,
            Retainedˉartifact.Length);
        Equal(
            Nativeˉhostedˉstartupˉinstantiator.CONSUMER_ARTIFACT_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Retainedˉartifact.AsSpan()));
        False(
            typeof(Nativeˉhostedˉstartupˉinstantiator).Assembly
                .GetManifestResourceNames()
                .Contains(
                    "Windvale.Linker.Native-Hosted-Startup-Instantiation.wvb",
                    StringComparer.Ordinal),
            "The normal linker embeds the hosted-startup instantiator WVB.");

        var Module = Moduleˉcodec.Readˉandˉverify(Coreˉresult.Moduleˉbytes.AsSpan());
        var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;
        Sequenceˉequal(Retainedˉartifact, Nativeˉfragmentˉartifactˉcodec.Write(Native));
        True(Native.Requiredˉservices.IsEmpty, "The startup instantiator requires a service.");
        Equal(
            new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor),
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Native));
        var Reference = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);

        var Windowsˉbundle = Hostedˉtoolˉtestˉbundle(
            Consoleˉapplicationˉtarget.Windowsˉx64);
        var Windowsˉlayout = Windowsˉhostedˉcompilerˉapplicationˉcontract.Plan(
            Windowsˉbundle,
            0);
        var Windowsˉruntime = Hostedˉcompilerˉruntimeˉdata.Plan(
            Consoleˉapplicationˉtarget.Windowsˉx64);
        var Windowsˉinputs = Windowsˉhostedˉcompilerˉstartup.Buildˉinputs(
            Windowsˉlayout.Textˉaddress,
            Windowsˉlayout.Importˉaddress,
            Windowsˉlayout.Runtimeˉaddress,
            Windowsˉruntime,
            Windowsˉbundle,
            0);
        Sequenceˉequal(
            Assembleˉsuccess(WINDOWS_HOSTED_COMPILER_STARTUP_SOURCE),
            Windowsˉinputs.Object);
        var Windowsˉrequest = Nativeˉhostedˉstartupˉinstantiator.Buildˉrequest(
            Windowsˉinputs);
        var Windowsˉexecuted = Nativeˉhostedˉstartupˉinstantiator.Buildˉwithˉwindvale(
            Windowsˉrequest);
        Sequenceˉequal(Reference.Runˉmainˉbytes(Windowsˉrequest).Bytes, Windowsˉexecuted);
        var Windowsˉstartup = Nativeˉhostedˉstartupˉinstantiator.Verifyˉresponse(
            Windowsˉinputs,
            Windowsˉrequest.Length,
            Windowsˉexecuted);
        Sequenceˉequal(
            Windowsˉhostedˉcompilerˉstartup.Buildˉstage0(
                Windowsˉlayout.Textˉaddress,
                Windowsˉlayout.Importˉaddress,
                Windowsˉlayout.Runtimeˉaddress,
                Windowsˉruntime,
                Windowsˉbundle,
                0),
            Windowsˉstartup);
        Sequenceˉequal(
            Windowsˉstartup,
            Windowsˉhostedˉcompilerˉstartup.Build(
                Windowsˉlayout.Textˉaddress,
                Windowsˉlayout.Importˉaddress,
                Windowsˉlayout.Runtimeˉaddress,
                Windowsˉruntime,
                Windowsˉbundle,
                0));

        var Linuxˉbundle = Hostedˉtoolˉtestˉbundle(Consoleˉapplicationˉtarget.Linuxˉx64);
        var Linuxˉlayout = Linuxˉhostedˉcompilerˉapplicationˉcontract.Plan(Linuxˉbundle, 0);
        var Linuxˉruntime = Hostedˉcompilerˉruntimeˉdata.Plan(
            Consoleˉapplicationˉtarget.Linuxˉx64);
        var Linuxˉinputs = Linuxˉhostedˉcompilerˉstartup.Buildˉinputs(
            Linuxˉlayout.Textˉaddress,
            Linuxˉlayout.Dataˉaddress,
            Linuxˉruntime,
            Linuxˉbundle,
            0);
        Sequenceˉequal(
            Assembleˉsuccess(LINUX_HOSTED_COMPILER_STARTUP_SOURCE),
            Linuxˉinputs.Object);
        var Linuxˉrequest = Nativeˉhostedˉstartupˉinstantiator.Buildˉrequest(Linuxˉinputs);
        var Linuxˉexecuted = Nativeˉhostedˉstartupˉinstantiator.Buildˉwithˉwindvale(
            Linuxˉrequest);
        Sequenceˉequal(Reference.Runˉmainˉbytes(Linuxˉrequest).Bytes, Linuxˉexecuted);
        var Linuxˉstartup = Nativeˉhostedˉstartupˉinstantiator.Verifyˉresponse(
            Linuxˉinputs,
            Linuxˉrequest.Length,
            Linuxˉexecuted);
        Sequenceˉequal(
            Linuxˉhostedˉcompilerˉstartup.Buildˉstage0(
                Linuxˉlayout.Textˉaddress,
                Linuxˉlayout.Dataˉaddress,
                Linuxˉruntime,
                Linuxˉbundle,
                0),
            Linuxˉstartup);
        Sequenceˉequal(
            Linuxˉstartup,
            Linuxˉhostedˉcompilerˉstartup.Build(
                Linuxˉlayout.Textˉaddress,
                Linuxˉlayout.Dataˉaddress,
                Linuxˉruntime,
                Linuxˉbundle,
                0));

        var Capabilities = Hostedˉtoolˉtestˉcapabilities();
        var Windowsˉapplication = Windowsˉhostedˉcompilerˉapplicationˉbuilder.Build(
            Capabilities,
            Windowsˉbundle,
            0);
        _ = Windowsˉhostedˉcompilerˉapplicationˉverifier.Verify(
            Windowsˉapplication.AsSpan(),
            Windowsˉbundle);
        var Linuxˉapplication = Linuxˉhostedˉcompilerˉapplicationˉbuilder.Build(
            Capabilities,
            Linuxˉbundle,
            0);
        _ = Linuxˉhostedˉcompilerˉapplicationˉverifier.Verify(
            Linuxˉapplication.AsSpan(),
            Linuxˉbundle);

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
            Result[offset] ^= 0x01;
            return Result.ToImmutableArray();
        }

        void Expectˉfailure(ImmutableArray<byte> request, uint status, uint failureˉoffset)
        {
            var Interpreted = Reference.Runˉmainˉbytes(request).Bytes;
            var Executed = Nativeˉhostedˉstartupˉinstantiator.Buildˉwithˉwindvale(request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(32, Executed.Length);
            Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(failureˉoffset, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
        }

        var Objectˉoffset = 40 + Windowsˉinputs.Targets.Length * sizeof(uint);
        var Relocationˉoffset = Objectˉoffset + Windowsˉinputs.Object.Length -
            Windowsˉinputs.Targets.Length * 20;
        Expectˉfailure(Windowsˉrequest[..39], 1, 39);
        Expectˉfailure(Replaceˉu32(Windowsˉrequest, 0, 0), 2, 0);
        Expectˉfailure(Replaceˉu32(Windowsˉrequest, 4, 2), 3, 4);
        Expectˉfailure(Replaceˉu32(Windowsˉrequest, 8, 0), 1, 8);
        Expectˉfailure(Replaceˉu32(Windowsˉrequest, 12, 0), 4, 12);
        Expectˉfailure(Replaceˉu32(Windowsˉrequest, 16, 0), 4, 12);
        Expectˉfailure(Replaceˉu32(Windowsˉrequest, 20, 0), 4, 12);
        Expectˉfailure(Replaceˉu32(Windowsˉrequest, 28, 0), 4, 12);
        Expectˉfailure(Replaceˉu32(Windowsˉrequest, 36, 1), 4, 12);
        Expectˉfailure(Replaceˉu32(Windowsˉrequest, 40, 0), 4, 40);
        Expectˉfailure(Mutateˉbyte(Windowsˉrequest, Objectˉoffset), 5, (uint)Objectˉoffset);
        Expectˉfailure(
            Replaceˉu32(Windowsˉrequest, Objectˉoffset + 12, 2),
            5,
            (uint)(Objectˉoffset + 12));
        Expectˉfailure(
            Mutateˉbyte(Windowsˉrequest, Relocationˉoffset),
            5,
            (uint)Relocationˉoffset);
        Expectˉfailure(Replaceˉu32(Windowsˉrequest, 40, uint.MaxValue), 6, 40);

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-hosted-startup-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Native-Hosted-Startup.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Startup-Instantiation.wvproj"),
                Nativeˉpath);
            Equal(0, Nativeˉbuild.Exitˉcode);
            Equal(string.Empty, Nativeˉbuild.Error);
            Sequenceˉequal(Coreˉresult.Moduleˉbytes, File.ReadAllBytes(Nativeˉpath));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
