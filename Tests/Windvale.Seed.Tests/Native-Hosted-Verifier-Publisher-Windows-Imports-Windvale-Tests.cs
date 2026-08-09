using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Windvaleˉnativeˉhostedˉverifierˉpublisherˉwindowsˉimportsˉconstruct()
    {
        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(), $"windvale-native-publisher-imports-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Publisher-Windows-Imports.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Verifier-Publisher-Windows-Imports.wvproj"),
                Moduleˉpath);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            var Moduleˉbytes = File.ReadAllBytes(Moduleˉpath);
            Equal(9_310, Moduleˉbytes.Length);
            Equal(
                "8d233b54d0387e9a1348447f9095e683415075da31104f4b80c935b09c960831",
                Moduleˉdigest.Calculateˉsha256(Moduleˉbytes));
            var Module = Moduleˉcodec.Readˉandˉverify(Moduleˉbytes);
            var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;
            True(Native.Requiredˉservices.IsEmpty,
                "Publisher Windows import construction unexpectedly requires a native service.");
            Equal(
                new Nativeˉentryˉshape(
                    Nativeˉentryˉinputˉkind.Bytes,
                    Nativeˉentryˉresultˉkind.Descriptor),
                Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Native));
            var Reference = new Referenceˉruntime(
                Module,
                new Referenceˉcapabilityˉhost(TextWriter.Null),
                Runtimeˉoptions.Portableˉdefaults);
            var Request = new byte[16];
            BinaryPrimitives.WriteUInt32LittleEndian(Request, 0x5249_5657u);
            BinaryPrimitives.WriteUInt32LittleEndian(Request.AsSpan(4), 1u);
            BinaryPrimitives.WriteUInt32LittleEndian(Request.AsSpan(8), 16u);
            BinaryPrimitives.WriteUInt32LittleEndian(Request.AsSpan(12), 253_952u);
            var Input = Request.ToImmutableArray();
            var Executed = X64ˉnativeˉexecutor.Executeˉbytes(Native, Input);
            Sequenceˉequal(Reference.Runˉmainˉbytes(Input).Bytes, Executed);
            Equal(4_128, Executed.Length);
            Equal(0x4d49_5657u, Readˉpublisherˉrequestˉu32(Executed, 0));
            Equal(0u, Readˉpublisherˉrequestˉu32(Executed, 12));
            Equal(32u, Readˉpublisherˉrequestˉu32(Executed, 20));
            Equal(4_096u, Readˉpublisherˉrequestˉu32(Executed, 24));
            Equal(253_952u, Readˉpublisherˉrequestˉu32(Executed, 28));
            var Page = Executed.AsSpan()[32..];
            var Application = File.ReadAllBytes(Path.Combine(
                Repository,
                "Artifacts",
                "Native-Hosted-Verifier-Application-Publisher-Candidate",
                "windows-x64-wvhostverifierpublish.exe"));
            Sequenceˉequal(Application.AsSpan(247_296, 4_096).ToArray(), Page.ToArray());
            Equal(
                "ff9b9a84ea0d74386337ab605a4d1afc76bd426bff49d6dfd96845b06207bee5",
                Convert.ToHexString(SHA256.HashData(Page)).ToLowerInvariant());

            Expectˉpublisherˉimportsˉfailure(Native, Reference, Input[..15]);
            Expectˉpublisherˉimportsˉfailure(
                Native, Reference, Replaceˉpublisherˉu32(Input, 0, 0u));
            Expectˉpublisherˉimportsˉfailure(
                Native, Reference, Replaceˉpublisherˉu32(Input, 4, 2u));
            Expectˉpublisherˉimportsˉfailure(
                Native, Reference, Replaceˉpublisherˉu32(Input, 12, 249_856u));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static void Expectˉpublisherˉimportsˉfailure(
        Nativeˉfragment native,
        Referenceˉruntime reference,
        ImmutableArray<byte> input)
    {
        var Executed = X64ˉnativeˉexecutor.Executeˉbytes(native, input);
        Sequenceˉequal(reference.Runˉmainˉbytes(input).Bytes, Executed);
        Equal(32, Executed.Length);
        Equal(1u, Readˉpublisherˉrequestˉu32(Executed, 12));
    }
}
