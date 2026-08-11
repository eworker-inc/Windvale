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
    private static void Windvaleˉnativeˉhostedˉverifierˉpublisherˉwindowsˉmaterializes()
    {
        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(), $"windvale-native-publisher-windows-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Publisher-Windows-Materialization.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Verifier-Publisher-Windows-Materialization.wvproj"),
                Moduleˉpath);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            var Moduleˉbytes = File.ReadAllBytes(Moduleˉpath);
            Equal(20_079, Moduleˉbytes.Length);
            Equal(
                "44d07d46a0280e6a7591179abd062649144d3b2dfbf487b55f7353df5bdb8640",
                Moduleˉdigest.Calculateˉsha256(Moduleˉbytes));
            var Module = Moduleˉcodec.Readˉandˉverify(Moduleˉbytes);
            var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;
            True(Native.Requiredˉservices.IsEmpty,
                "Publisher Windows materialization unexpectedly requires a native service.");
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
                Publisherˉfragment, Nativeˉserviceˉplatform.Windows);
            var Base = Windowsˉhostedˉverifierˉapplicationˉbuilder.Build(
                Publisher.Module.Capabilities, Bundle, Nativeˉentry).ToArray();
            Equal(248_832, Base.Length);
            Equal(
                "2afd9d92422b063abd3cd20d8da6056efbbbff9e7ac8baeef9c8b60b391686c5",
                Convert.ToHexString(SHA256.HashData(Base)).ToLowerInvariant());

            var Expected = File.ReadAllBytes(Path.Combine(
                Repository,
                "Artifacts",
                "Native-Hosted-Verifier-Application-Publisher-Candidate",
                "windows-x64-wvhostverifierpublish.exe"));
            var Construction = Buildˉwindowsˉpublisherˉconstructionˉrecord();
            var Objects = Buildˉwindowsˉpublisherˉobjectsˉrecord(Expected);
            var Metadata = Expected.AsSpan(252_896, 128).ToArray();
            var Imports = Buildˉwindowsˉpublisherˉimportsˉrecord(Expected);
            var Request = Buildˉwindowsˉpublisherˉmaterializationˉrequest(
                Base, Construction, Objects, Metadata, Imports);
            var Executed = X64ˉnativeˉexecutor.Executeˉbytes(
                Native, Request, maximumˉinstructions: 20_000_000);
            Sequenceˉequal(Reference.Runˉmainˉbytes(Request).Bytes, Executed);
            Equal(256_032, Executed.Length);
            Equal(0x4f57_5657u, Readˉpublisherˉrequestˉu32(Executed, 0));
            Equal(0u, Readˉpublisherˉrequestˉu32(Executed, 12));
            Sequenceˉequal(Expected, Executed.AsSpan()[32..].ToArray());
            Equal(
                "17cb5c4228e8448693b17f1b73695fd0ecfd03d7ada922794a5bf3bd7594fc96",
                Convert.ToHexString(SHA256.HashData(Executed.AsSpan()[32..])).ToLowerInvariant());

            Expectˉwindowsˉpublisherˉmaterializationˉfailure(
                Native, Reference, Request[..63], 1u);
            Expectˉwindowsˉpublisherˉmaterializationˉfailure(
                Native, Reference, Mutateˉpublisherˉbyte(Request, 64), 2u);
            Expectˉwindowsˉpublisherˉmaterializationˉfailure(
                Native, Reference, Replaceˉpublisherˉu32(Request, 249_324, 1u), 2u);
            Expectˉwindowsˉpublisherˉmaterializationˉfailure(
                Native, Reference, Replaceˉpublisherˉu32(Request, 256_364, 2u), 2u);
            Expectˉwindowsˉpublisherˉmaterializationˉfailure(
                Native, Reference, Replaceˉpublisherˉu32(Request, 256_508, 249_856u), 2u);
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static byte[] Buildˉwindowsˉpublisherˉconstructionˉrecord()
    {
        var Result = new byte[416];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, 0x5243_5657u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), 1u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), 416u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), 1u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(72), 235_394u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(132), 240_016u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(136), 243_600u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(156), 256_000u);
        return Result;
    }

    private static byte[] Buildˉwindowsˉpublisherˉobjectsˉrecord(byte[] application)
    {
        var Result = new byte[7_040];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, 0x4f49_5657u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), 1u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), 7_040u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(20), 1u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(24), 64u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(28), 5u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(32), 69u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(36), 5_286u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(40), 5_355u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(44), 1_685u);
        application.AsSpan(512, 5).CopyTo(Result.AsSpan(64));
        application.AsSpan(240_016, 5_286).CopyTo(Result.AsSpan(69));
        application.AsSpan(245_312, 1_685).CopyTo(Result.AsSpan(5_355));
        return Result;
    }

    private static byte[] Buildˉwindowsˉpublisherˉimportsˉrecord(byte[] application)
    {
        var Result = new byte[4_128];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, 0x4d49_5657u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), 1u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), 4_128u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), 16u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(20), 32u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(24), 4_096u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(28), 253_952u);
        application.AsSpan(247_296, 4_096).CopyTo(Result.AsSpan(32));
        return Result;
    }

    private static ImmutableArray<byte> Buildˉwindowsˉpublisherˉmaterializationˉrequest(
        byte[] baseˉapplication,
        byte[] construction,
        byte[] objects,
        byte[] metadata,
        byte[] imports)
    {
        var Result = new byte[260_608];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, 0x4d57_5657u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), 1u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), 260_608u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), 1u);
        var Values = new[] { 64u, 248_832u, 248_896u, 416u, 249_312u, 7_040u, 256_352u, 128u, 256_480u, 4_128u };
        for (var Index = 0; Index < Values.Length; Index++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16 + Index * 4), Values[Index]);
        }
        baseˉapplication.CopyTo(Result.AsSpan(64));
        construction.CopyTo(Result.AsSpan(248_896));
        objects.CopyTo(Result.AsSpan(249_312));
        metadata.CopyTo(Result.AsSpan(256_352));
        imports.CopyTo(Result.AsSpan(256_480));
        return Result.ToImmutableArray();
    }

    private static void Expectˉwindowsˉpublisherˉmaterializationˉfailure(
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
