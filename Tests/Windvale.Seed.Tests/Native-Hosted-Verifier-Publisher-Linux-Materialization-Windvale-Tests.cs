using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Windvaleˉnativeˉhostedˉverifierˉpublisherˉlinuxˉmaterializes()
    {
        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(), $"windvale-native-publisher-linux-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Publisher-Linux-Materialization.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Verifier-Publisher-Linux-Materialization.wvproj"),
                Moduleˉpath);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            var Moduleˉbytes = File.ReadAllBytes(Moduleˉpath);
            Equal(14_950, Moduleˉbytes.Length);
            Equal(
                "74cadd840368532e1267454a3ffd551bfe2c0fbc320f472e103afb3cdd0dd639",
                Moduleˉdigest.Calculateˉsha256(Moduleˉbytes));
            var Module = Moduleˉcodec.Readˉandˉverify(Moduleˉbytes);
            var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;
            True(Native.Requiredˉservices.IsEmpty,
                "Publisher Linux materialization unexpectedly requires a native service.");
            Equal(
                new Nativeˉentryˉshape(
                    Nativeˉentryˉinputˉkind.Bytes,
                    Nativeˉentryˉresultˉkind.Descriptor),
                Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Native));
            var Reference = new Referenceˉruntime(
                Module,
                new Referenceˉcapabilityˉhost(TextWriter.Null),
                Runtimeˉoptions.Portableˉdefaults);

            var Publisherˉbytes = File.ReadAllBytes(Path.Combine(
                Repository,
                "Artifacts",
                "Native-Hosted-Verifier-Application-Publisher-Candidate",
                "Hosted-Verifier-Application-Publisher.wvb"));
            var Publisher = Moduleˉcodec.Readˉandˉverify(Publisherˉbytes);
            var Publisherˉfragment = X64ˉnativeˉbackend.Compile(Publisher).Fragment;
            var Nativeˉentry = Publisherˉfragment.Symbols.Single(Symbol =>
                Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
                Symbol.Kind == Nativeˉsymbolˉkind.Function &&
                Symbol.Name == "Main").Offset;
            var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉverifier(
                Publisherˉfragment, Nativeˉserviceˉplatform.Linux);
            var Base = Linuxˉhostedˉverifierˉapplicationˉbuilder.Build(
                Publisher.Module.Capabilities, Bundle, Nativeˉentry).ToArray();
            Equal(249_856, Base.Length);
            Equal(
                "687338281ca78c9d3a4d08b601c1efbcc198ec3c8fcc96fbf34f5dc349cafae2",
                Convert.ToHexString(SHA256.HashData(Base)).ToLowerInvariant());

            var Expected = File.ReadAllBytes(Path.Combine(
                Repository,
                "Artifacts",
                "Native-Hosted-Verifier-Application-Publisher-Candidate",
                "linux-x64-wvhostverifierpublish.elf"));
            var Construction = Buildˉlinuxˉpublisherˉconstructionˉrecord();
            var Objects = Buildˉlinuxˉpublisherˉobjectsˉrecord(Expected);
            var Metadata = Expected.AsSpan(247_264, 128).ToArray();
            var Request = Buildˉlinuxˉpublisherˉmaterializationˉrequest(
                Base, Construction, Objects, Metadata);
            var Executed = X64ˉnativeˉexecutor.Executeˉbytes(
                Native, Request, maximumˉinstructions: 20_000_000);
            Sequenceˉequal(Reference.Runˉmainˉbytes(Request).Bytes, Executed);
            Equal(254_949, Executed.Length);
            Equal(0x4f4c_5657u, Readˉpublisherˉrequestˉu32(Executed, 0));
            Equal(0u, Readˉpublisherˉrequestˉu32(Executed, 12));
            Sequenceˉequal(Expected, Executed.AsSpan()[32..].ToArray());
            Equal(
                "babe721a573e29f89ec095c35677880077ff465d4e2129063f6742cd47591a97",
                Convert.ToHexString(SHA256.HashData(Executed.AsSpan()[32..])).ToLowerInvariant());

            Expectˉlinuxˉpublisherˉmaterializationˉfailure(
                Native, Reference, Request[..63], 1u);
            Expectˉlinuxˉpublisherˉmaterializationˉfailure(
                Native, Reference, Mutateˉpublisherˉbyte(Request, 64), 2u);
            Expectˉlinuxˉpublisherˉmaterializationˉfailure(
                Native, Reference, Replaceˉpublisherˉu32(Request, 250_348, 1u), 2u);
            Expectˉlinuxˉpublisherˉmaterializationˉfailure(
                Native, Reference, Replaceˉpublisherˉu32(Request, 255_465, 1u), 2u);
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static byte[] Buildˉlinuxˉpublisherˉconstructionˉrecord()
    {
        var Result = new byte[416];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, 0x5243_5657u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), 1u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), 416u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), 2u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(72), 235_077u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(132), 249_856u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(136), 142_929_920u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(156), 254_917u);
        return Result;
    }

    private static byte[] Buildˉlinuxˉpublisherˉobjectsˉrecord(byte[] application)
    {
        var Result = new byte[5_117];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, 0x4f49_5657u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), 1u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), 5_117u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(20), 2u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(24), 64u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(28), 5u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(32), 69u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(36), 3_363u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(40), 3_432u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(44), 1_685u);
        application.AsSpan(4_096, 5).CopyTo(Result.AsSpan(64));
        application.AsSpan(249_856, 3_363).CopyTo(Result.AsSpan(69));
        application.AsSpan(253_232, 1_685).CopyTo(Result.AsSpan(3_432));
        return Result;
    }

    private static ImmutableArray<byte> Buildˉlinuxˉpublisherˉmaterializationˉrequest(
        byte[] baseˉapplication,
        byte[] construction,
        byte[] objects,
        byte[] metadata)
    {
        var Result = new byte[255_581];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, 0x4d4c_5657u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), 1u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), 255_581u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), 2u);
        var Values = new[] { 64u, 249_856u, 249_920u, 416u, 250_336u, 5_117u, 255_453u, 128u };
        for (var Index = 0; Index < Values.Length; Index++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16 + Index * 4), Values[Index]);
        }
        baseˉapplication.CopyTo(Result.AsSpan(64));
        construction.CopyTo(Result.AsSpan(249_920));
        objects.CopyTo(Result.AsSpan(250_336));
        metadata.CopyTo(Result.AsSpan(255_453));
        return Result.ToImmutableArray();
    }

    private static ImmutableArray<byte> Mutateˉpublisherˉbyte(
        ImmutableArray<byte> input,
        int offset)
    {
        var Result = input.ToArray();
        Result[offset] ^= 1;
        return Result.ToImmutableArray();
    }

    private static void Expectˉlinuxˉpublisherˉmaterializationˉfailure(
        Nativeˉfragment native,
        Referenceˉruntime reference,
        ImmutableArray<byte> input,
        uint status)
    {
        var Executed = X64ˉnativeˉexecutor.Executeˉbytes(
            native, input, maximumˉinstructions: 20_000_000);
        Sequenceˉequal(reference.Runˉmainˉbytes(input).Bytes, Executed);
        Equal(32, Executed.Length);
        Equal(status, Readˉpublisherˉrequestˉu32(Executed, 12));
    }
}
