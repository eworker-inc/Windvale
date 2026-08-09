using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_HOSTED_VERIFIER_PUBLISHER_METADATA_BYTES = 10_441;
    private const string NATIVE_HOSTED_VERIFIER_PUBLISHER_METADATA_SHA256 =
        "208b2724a10f2e497ef13be51d254426e86afda99600c61dd937cdf4171d3bbd";

    private static void Windvaleˉnativeˉhostedˉverifierˉpublisherˉmetadataˉruns()
    {
        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-wvvp-metadata-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        byte[] Bridgeˉbytes;
        try
        {
            var Bridgeˉpath = Path.Combine(Directoryˉpath, "WVVP-Metadata.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Verifier-Application-Publisher-Metadata.wvproj"),
                Bridgeˉpath);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            Bridgeˉbytes = File.ReadAllBytes(Bridgeˉpath);
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
        Equal(NATIVE_HOSTED_VERIFIER_PUBLISHER_METADATA_BYTES, Bridgeˉbytes.Length);
        Equal(
            NATIVE_HOSTED_VERIFIER_PUBLISHER_METADATA_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉbytes));

        var Bridge = Moduleˉcodec.Readˉandˉverify(Bridgeˉbytes);
        var Native = X64ˉnativeˉbackend.Compile(Bridge).Fragment;
        True(Native.Requiredˉservices.IsEmpty,
            "The WVVP metadata constructor requires a service.");
        Equal(
            new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor),
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Native));
        var Reference = new Referenceˉruntime(
            Bridge,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);

        ImmutableArray<byte>? Windowsˉrequest = null;
        foreach (var Target in Enum.GetValues<Consoleˉapplicationˉtarget>())
        {
            var Windows = Target == Consoleˉapplicationˉtarget.Windowsˉx64;
            var Applicationˉpath = Path.Combine(
                Repository,
                "Artifacts",
                "Native-Hosted-Verifier-Application-Publisher-Candidate",
                Windows
                    ? "windows-x64-wvhostverifierpublish.exe"
                    : "linux-x64-wvhostverifierpublish.elf");
            var Application = File.ReadAllBytes(Applicationˉpath);
            var Metadataˉoffset = Windows ? 252_896 : 247_264;
            var Metadata = Application.AsSpan(Metadataˉoffset, 128).ToArray();
            Equal(1347835479u, BinaryPrimitives.ReadUInt32LittleEndian(Metadata));
            Equal(
                Windows
                    ? "40e73f9c4ac9e27c9dea7f9bed8217be125159f89cb2ea314a91bc66da389b74"
                    : "393253dab73387a0c96fd33c278b350fe43e5466a243eabe3f62a6652c946035",
                Objectˉdigest.Calculateˉsha256(Metadata));

            var Request = Buildˉpublisherˉmetadataˉrequest(Metadata);
            Equal(
                Windows
                    ? "4533fb4c90bab03d5aeb39f6bd8943424f228fb66846d467d333c182d2a2b8f2"
                    : "a285e192992a5239495fc4046cc59390504ffefbe1ab7b863da2d370e615c500",
                Objectˉdigest.Calculateˉsha256(Request.AsSpan()));
            var Interpreted = Reference.Runˉmainˉbytes(Request).Bytes;
            var Executed = X64ˉnativeˉexecutor.Executeˉbytes(Native, Request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(160, Executed.Length);
            Equal(1146115671u,
                BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()));
            Equal(1u,
                BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[4..]));
            Equal(160u,
                BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[8..]));
            Equal(0u,
                BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(112u,
                BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
            Equal(128u,
                BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[20..]));
            Sequenceˉequal(Metadata, Executed[32..]);
            if (Windows)
            {
                Windowsˉrequest = Request;
            }
        }

        var Valid = Windowsˉrequest ?? throw new InvalidOperationException();
        void Expectˉfailure(
            ImmutableArray<byte> request,
            uint status,
            uint failureˉoffset)
        {
            var Interpreted = Reference.Runˉmainˉbytes(request).Bytes;
            var Executed = X64ˉnativeˉexecutor.Executeˉbytes(Native, request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(32, Executed.Length);
            Equal(status,
                BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(
                failureˉoffset,
                BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
        }

        Expectˉfailure(Valid[..^1], 1, 111);
        Expectˉfailure(Replaceˉpublisherˉmetadataˉu32(Valid, 0, 0), 2, 0);
        Expectˉfailure(Replaceˉpublisherˉmetadataˉu32(Valid, 4, 2), 3, 4);
        Expectˉfailure(Replaceˉpublisherˉmetadataˉu32(Valid, 8, 111), 1, 8);
        Expectˉfailure(Replaceˉpublisherˉmetadataˉu32(Valid, 12, 0), 4, 12);
        Expectˉfailure(Replaceˉpublisherˉmetadataˉu32(Valid, 16, 0), 4, 16);
        Expectˉfailure(Replaceˉpublisherˉmetadataˉu32(Valid, 36, 1), 4, 16);
        Expectˉfailure(Replaceˉpublisherˉmetadataˉu32(Valid, 48, 0), 4, 48);
        Expectˉfailure(Replaceˉpublisherˉmetadataˉu32(Valid, 80, 0), 4, 80);
    }

    private static ImmutableArray<byte> Buildˉpublisherˉmetadataˉrequest(
        ReadOnlySpan<byte> metadata)
    {
        var Result = new byte[112];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, 1297110615u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), 1u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), 112u);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(12),
            BinaryPrimitives.ReadUInt32LittleEndian(metadata[12..]));
        metadata[20..40].CopyTo(Result.AsSpan(16));
        metadata[48..112].CopyTo(Result.AsSpan(48));
        return Result.ToImmutableArray();
    }

    private static ImmutableArray<byte> Replaceˉpublisherˉmetadataˉu32(
        ImmutableArray<byte> input,
        int offset,
        uint value)
    {
        var Result = input.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(offset), value);
        return Result.ToImmutableArray();
    }
}
