using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_ENUM_NAME_CORE_SIZE = 625;
    private const string NATIVE_ENUM_NAME_CORE_SHA256 =
        "b404104b8e5ca174841b47d02ea45f197599179e0cb23ba778d6a2cdf7846948";
    private const int NATIVE_ENUM_METADATA_CORE_SIZE = 13_946;
    private const string NATIVE_ENUM_METADATA_CORE_SHA256 =
        "9c61f7d436854ace71ab17fcf33da73c40d37d612f68ba08bfa929ab4e710ef1";

    private static void Windvaleˉnativeˉenumˉnameˉserviceˉruns()
    {
        var Coreˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Enum-Name-Service.wv");
        var Bridgeˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Enum-Name-Service-Bridge.wv");
        var Coreˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-X64-Enum-Name-Service.wv",
            Coreˉsource);
        var Metadataˉcoreˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-Enum-Metadata-Core.wv");
        var Metadataˉbridgeˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-Enum-Metadata-Bridge.wv");
        var Metadataˉcoreˉinput = new Sourceˉmoduleˉinput(
            "Compiler/Windvale/Native-Enum-Metadata-Core.wv",
            Metadataˉcoreˉsource);

        var Coreˉresult = Seedˉcompiler.Compileˉmodules(Coreˉinput, []);
        True(
            Coreˉresult.Success,
            "The Windvale enum-name service core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(NATIVE_ENUM_NAME_CORE_SIZE, Coreˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_ENUM_NAME_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(
            new(
                "Runtime/Windvale/Native-X64-Enum-Name-Service-Bridge.wv",
                Bridgeˉsource),
            [Coreˉinput]);
        True(
            Bridgeˉresult.Success,
            "The Windvale enum-name service bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            X64ˉnativeˉtextˉservices.ENUM_NAME_CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            X64ˉnativeˉtextˉservices.ENUM_NAME_CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Metadataˉcoreˉresult = Seedˉcompiler.Compileˉmodules(
            Metadataˉcoreˉinput,
            []);
        True(
            Metadataˉcoreˉresult.Success,
            "The Windvale enum-metadata core did not compile: " +
                string.Join(" | ", Metadataˉcoreˉresult.Diagnostics));
        Equal(NATIVE_ENUM_METADATA_CORE_SIZE, Metadataˉcoreˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_ENUM_METADATA_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Metadataˉcoreˉresult.Moduleˉbytes.AsSpan()));

        var Metadataˉbridgeˉresult = Seedˉcompiler.Compileˉmodules(
            new(
                "Compiler/Windvale/Native-Enum-Metadata-Bridge.wv",
                Metadataˉbridgeˉsource),
            [Metadataˉcoreˉinput]);
        True(
            Metadataˉbridgeˉresult.Success,
            "The Windvale enum-metadata bridge did not compile: " +
                string.Join(" | ", Metadataˉbridgeˉresult.Diagnostics));
        Equal(
            Nativeˉenumˉmetadataˉbuilder.CONSUMER_CANONICAL_SIZE,
            Metadataˉbridgeˉresult.Moduleˉbytes.Length);
        Equal(
            Nativeˉenumˉmetadataˉbuilder.CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(
                Metadataˉbridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/Native-X64-Enum-Name-Service-Bridge.wvb")));

        ImmutableArray<byte> Retainedˉenumˉleaf;
        using (var Stream = typeof(X64ˉnativeˉtextˉservices).Assembly
            .GetManifestResourceStream(
                "Windvale.Native.Native-X64-Enum-Name-Service.bin") ??
            throw new InvalidOperationException(
                "The retained Windvale enum-name service leaf was not embedded."))
        {
            var Retained = new byte[checked((int)Stream.Length)];
            Stream.ReadExactly(Retained);
            Retainedˉenumˉleaf = Retained.ToImmutableArray();
        }
        using (var Stream = typeof(X64ˉnativeˉtextˉservices).Assembly
            .GetManifestResourceStream(
                "Windvale.Native.Native-Enum-Metadata-Bridge.wvb") ??
            throw new InvalidOperationException(
                "The retained Windvale enum-metadata bridge was not embedded."))
        {
            var Retained = new byte[checked((int)Stream.Length)];
            Stream.ReadExactly(Retained);
            Sequenceˉequal(Metadataˉbridgeˉresult.Moduleˉbytes, Retained);
        }

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-enum-name-service-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Enum-Name.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-X64-Enum-Name-Service.wvproj"),
                Nativeˉpath);
            Equal(0, Nativeˉbuild.Exitˉcode);
            Equal(string.Empty, Nativeˉbuild.Error);
            Sequenceˉequal(
                Bridgeˉresult.Moduleˉbytes,
                File.ReadAllBytes(Nativeˉpath));

            var Metadataˉpath = Path.Combine(Directoryˉpath, "Enum-Metadata.wvb");
            var Metadataˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Enum-Metadata.wvproj"),
                Metadataˉpath);
            Equal(0, Metadataˉbuild.Exitˉcode);
            Equal(string.Empty, Metadataˉbuild.Error);
            Sequenceˉequal(
                Metadataˉbridgeˉresult.Moduleˉbytes,
                File.ReadAllBytes(Metadataˉpath));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }

        var Types = ImmutableArray.Create<Nominalˉtypeˉdeclaration>(
            new Recordˉtypeˉdeclaration(
                "Nativeˉrecord",
                ImmutableArray<Recordˉfieldˉdeclaration>.Empty),
            new Enumˉtypeˉdeclaration(
                "Nativeˉstate",
                ImmutableArray.Create(
                    new Enumˉmemberˉdeclaration("Stopped", -1),
                    new Enumˉmemberˉdeclaration("Running", 2))));
        var Bundle = X64ˉnativeˉtextˉservices.Build(
            Nativeˉservice.Enumˉname,
            Types);
        X64ˉnativeˉtextˉservices.Verify(
            Nativeˉservice.Enumˉname,
            Bundle.AsSpan(),
            Types);
        var Expected = Bundle.AsSpan(
            0,
            X64ˉnativeˉtextˉservices.ENUM_NAME_CANONICAL_SIZE).ToImmutableArray();
        Equal(X64ˉnativeˉtextˉservices.ENUM_NAME_CANONICAL_SIZE, Expected.Length);
        Equal(
            X64ˉnativeˉtextˉservices.ENUM_NAME_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Expected.AsSpan()));
        Sequenceˉequal(Expected, Retainedˉenumˉleaf);

        var Metadataˉrequests = Nativeˉenumˉmetadataˉbuilder.Buildˉrequests(Types);
        Equal(1, Metadataˉrequests.Length);
        var Metadataˉrequest = Metadataˉrequests[0];
        var Metadataˉresponse =
            Nativeˉenumˉmetadataˉbuilder.Buildˉwithˉwindvale(Metadataˉrequest);
        var Windvaleˉmetadata = Nativeˉenumˉmetadataˉbuilder.Build(Types);
        var Expectedˉmetadata = Convert.FromHexString(
            "5756454E0100000056000000020000000200000018000000" +
            "00000000000000000000000002000000" +
            "FFFFFFFF480000000700000000000000" +
            "020000004F0000000700000000000000" +
            "53746F7070656452756E6E696E67");
        Sequenceˉequal(Expectedˉmetadata, Windvaleˉmetadata);
        Sequenceˉequal(
            Windvaleˉmetadata,
            Bundle.AsSpan()[X64ˉnativeˉtextˉservices.ENUM_NAME_CANONICAL_SIZE..]
                .ToImmutableArray());

        var Bridge = Moduleˉcodec.Readˉandˉverify(
            Bridgeˉresult.Moduleˉbytes.AsSpan());
        var Interpreted = new Referenceˉruntime(
            Bridge,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmainˉbytes();
        Sequenceˉequal(Expected, Interpreted.Bytes);

        var Native = X64ˉnativeˉbackend.Compile(Bridge);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Sequenceˉequal(
            Expected,
            X64ˉnativeˉexecutor.Executeˉbytes(
                Native.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions));

        var Metadataˉbridge = Moduleˉcodec.Readˉandˉverify(
            Metadataˉbridgeˉresult.Moduleˉbytes.AsSpan());
        var Metadataˉreference = new Referenceˉruntime(
            Metadataˉbridge,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);
        var Metadataˉnative = X64ˉnativeˉbackend.Compile(Metadataˉbridge).Fragment;
        Sequenceˉequal(
            Metadataˉresponse,
            Metadataˉreference.Runˉmainˉbytes(Metadataˉrequest).Bytes);
        Sequenceˉequal(
            Metadataˉresponse,
            X64ˉnativeˉexecutor.Executeˉbytes(Metadataˉnative, Metadataˉrequest));

        void Rejectˉrequest(ImmutableArray<byte> request)
        {
            Equal(0, Metadataˉreference.Runˉmainˉbytes(request).Bytes.Length);
            Equal(0, X64ˉnativeˉexecutor.Executeˉbytes(Metadataˉnative, request).Length);
        }

        var Badˉmagic = Metadataˉrequest.ToArray();
        Badˉmagic[0] ^= 0x01;
        Rejectˉrequest(Badˉmagic.ToImmutableArray());
        Rejectˉrequest(Metadataˉrequest.AsSpan()[..^1].ToImmutableArray());

        var Duplicateˉvalue = Metadataˉrequest.ToArray();
        const int Secondˉmemberˉoffset = 48 + 2 * 8 + 16;
        BinaryPrimitives.WriteInt32LittleEndian(
            Duplicateˉvalue.AsSpan(Secondˉmemberˉoffset),
            -1);
        Rejectˉrequest(Duplicateˉvalue.ToImmutableArray());

        var Duplicateˉname = Metadataˉrequest.ToArray();
        var Duplicateˉrank = Metadataˉrequest.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(
            Duplicateˉrank.AsSpan(Secondˉmemberˉoffset + 12),
            1);
        Rejectˉrequest(Duplicateˉrank.ToImmutableArray());

        const int Namesˉoffset = 48 + 2 * 8 + 2 * 16;
        Duplicateˉname.AsSpan(Namesˉoffset, 7).CopyTo(
            Duplicateˉname.AsSpan(Namesˉoffset + 7, 7));
        Rejectˉrequest(Duplicateˉname.ToImmutableArray());

        var Invalidˉidentifier = Metadataˉrequest.ToArray();
        Invalidˉidentifier[Namesˉoffset] = 203;
        Rejectˉrequest(Invalidˉidentifier.ToImmutableArray());

        var Boundaryˉmembers = ImmutableArray.CreateBuilder<Enumˉmemberˉdeclaration>(
            Bytecodeˉlimits.MAX_ENUM_MEMBERS);
        for (var Index = 0; Index < Bytecodeˉlimits.MAX_ENUM_MEMBERS; Index++)
        {
            Boundaryˉmembers.Add(new(
                $"N{new string('a', 251)}{Index:D3}",
                Index));
        }
        var Boundaryˉtypes = ImmutableArray.Create<Nominalˉtypeˉdeclaration>(
            new Enumˉtypeˉdeclaration(
                "Boundary",
                Boundaryˉmembers.MoveToImmutable()));
        var Boundaryˉmetadata = Nativeˉenumˉmetadataˉbuilder.Build(Boundaryˉtypes);
        Nativeˉenumˉmetadataˉbuilder.Verify(
            Boundaryˉtypes,
            Boundaryˉmetadata.AsSpan());

        var Maximumˉmembers = Enumerable.Range(0, Bytecodeˉlimits.MAX_ENUM_MEMBERS)
            .Select(Index => new Enumˉmemberˉdeclaration(
                Index == Bytecodeˉlimits.MAX_ENUM_MEMBERS - 1
                    ? $"N{new string('a', 254)}"
                    : $"M{new string('a', Index)}",
                Index))
            .ToImmutableArray();
        const int Oversizedˉtypeˉcount = 114;
        var Oversizedˉtypes = ImmutableArray.CreateBuilder<Nominalˉtypeˉdeclaration>(
            Oversizedˉtypeˉcount);
        for (var Index = 0; Index < Oversizedˉtypeˉcount; Index++)
        {
            Oversizedˉtypes.Add(new Enumˉtypeˉdeclaration($"Type{Index}", Maximumˉmembers));
        }
        var Oversized = Oversizedˉtypes.MoveToImmutable();
        var Oversizedˉrequests = Nativeˉenumˉmetadataˉbuilder.Buildˉrequests(Oversized);
        Equal(15, Oversizedˉrequests.Length);
        True(
            Oversizedˉrequests.All(
                Request => Request.Length <= Bytecodeˉlimits.MAX_BYTE_DATA_BYTES),
            "The oversized WVEN case did not cross the segmented input seam.");
        var Oversizedˉmetadata = Nativeˉenumˉmetadataˉbuilder.Build(Oversized);
        True(
            Oversizedˉmetadata.Length > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES,
            "The segmented WVEN case no longer exceeds one Windvale byte value.");
        Nativeˉenumˉmetadataˉbuilder.Verify(Oversized, Oversizedˉmetadata.AsSpan());
    }
}
