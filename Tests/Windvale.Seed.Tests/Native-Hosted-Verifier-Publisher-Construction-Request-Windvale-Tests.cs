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
    private static void Windvaleˉnativeˉhostedˉverifierˉpublisherˉconstructionˉrequestsˉrun()
    {
        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-publisher-construction-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        var Contracts = new (string Name, int Bytes, string Sha256)[]
        {
            ("Identity-Request", 55_950,
                "039ce6e5e23f74d1345b9324324b48b32ca78ac9678c656d603558a6f059504c"),
            ("Structure-Request", 24_735,
                "43aaf82899d3a3160337d580535c7ee48832dba68025b7cea46681b32ea6388d"),
            ("Construction-Request", 30_656,
                "98e54e0ff807baa189bf2e75a1abd9b913c668a49c91a088946e10cf710a842d"),
            ("Target-Request", 25_826,
                "15a56d4ba8524c93beccb5e7341fbe486851eba63453ff93ea422d83ea9a415b"),
        };
        var Tools = new (string Name, int Bytes, string Sha256)[]
        {
            ("Identity-Request", 58_743,
                "7f534a4ac03f2c1d902ca9c12f958bfd95a3c06a5b503bbc8eaf6b7d17b4bd7f"),
            ("Structure-Request", 26_105,
                "18f990e7bb9cab3653ac280e318d3b1c8b2c77ba1e09b13df3c7b80e46671de3"),
            ("Construction-Request", 32_165,
                "da3d3639557c2d986aefe787030717076dccf735229bece01ea9ff44e0dbe685"),
            ("Target-Request", 27_256,
                "b6d7c50d70b17bb5cf7a6a8c8cf5542c3c96c8bd2266e3dfc047271f4802a6ec"),
        };
        var Modules = new List<Verifiedˉmodule>();
        var Native = new List<Nativeˉfragment>();
        try
        {
            foreach (var Contract in Contracts)
            {
                var Output = Path.Combine(Directoryˉpath, Contract.Name + ".wvb");
                var Build = Runˉnativeˉfrontˉdoor(
                    Repository,
                    Path.Combine(
                        Repository,
                        $"Windvale-Native-Hosted-Verifier-Publisher-{Contract.Name}.wvproj"),
                    Output);
                Equal(0, Build.Exitˉcode);
                Equal(string.Empty, Build.Error);
                var Bytes = File.ReadAllBytes(Output);
                Equal(Contract.Bytes, Bytes.Length);
                Equal(Contract.Sha256, Moduleˉdigest.Calculateˉsha256(Bytes));
                var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
                var Fragment = X64ˉnativeˉbackend.Compile(Module).Fragment;
                True(Fragment.Requiredˉservices.IsEmpty,
                    $"{Contract.Name} unexpectedly requires a native service.");
                Equal(
                    new Nativeˉentryˉshape(
                        Nativeˉentryˉinputˉkind.Bytes,
                        Nativeˉentryˉresultˉkind.Descriptor),
                    Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Fragment));
                Modules.Add(Module);
                Native.Add(Fragment);
            }
            foreach (var Tool in Tools)
            {
                var Output = Path.Combine(Directoryˉpath, Tool.Name + "-Tool.wvb");
                var Build = Runˉnativeˉfrontˉdoor(
                    Repository,
                    Path.Combine(
                        Repository,
                        $"Windvale-Native-Hosted-Verifier-Publisher-{Tool.Name}-Tool.wvproj"),
                    Output);
                Equal(0, Build.Exitˉcode);
                Equal(string.Empty, Build.Error);
                var Bytes = File.ReadAllBytes(Output);
                Equal(Tool.Bytes, Bytes.Length);
                Equal(Tool.Sha256, Moduleˉdigest.Calculateˉsha256(Bytes));
            }

            ImmutableArray<byte> Run(int stage, ImmutableArray<byte> input)
            {
                var Executed = X64ˉnativeˉexecutor.Executeˉbytes(
                    Native[stage],
                    input,
                    maximumˉinstructions: stage == 0 ? 500_000_000 : 1_000_000);
                if (stage != 0)
                {
                    var Reference = new Referenceˉruntime(
                        Modules[stage],
                        new Referenceˉcapabilityˉhost(TextWriter.Null),
                        Runtimeˉoptions.Portableˉdefaults);
                    Sequenceˉequal(
                        Reference.Runˉmainˉbytes(input).Bytes,
                        Executed);
                }
                return Executed;
            }

            foreach (var Target in Enum.GetValues<Consoleˉapplicationˉtarget>())
            {
                var Windows = Target == Consoleˉapplicationˉtarget.Windowsˉx64;
                var Identity = Buildˉpublisherˉidentityˉinput(
                    Repository, Target);
                var Admitted = Run(0, Identity);
                Sequenceˉequal(Identity, Admitted);
                Equal(Windows ? 275_054 : 271_013, Admitted.Length);

                var Structure = Run(1, Admitted);
                Equal(128, Structure.Length);
                Equal(0x5350_5657u, Readˉpublisherˉrequestˉu32(Structure, 0));
                Equal((uint)Target, Readˉpublisherˉrequestˉu32(Structure, 12));
                Equal(3_001u, Readˉpublisherˉrequestˉu32(Structure, 36));
                Equal(789u, Readˉpublisherˉrequestˉu32(Structure, 40));
                Equal(0u, Readˉpublisherˉrequestˉu32(Structure, 44));

                var Construction = Run(2, Structure);
                Equal(416, Construction.Length);
                Equal(0x5243_5657u,
                    Readˉpublisherˉrequestˉu32(Construction, 0));
                Equal((uint)Target,
                    Readˉpublisherˉrequestˉu32(Construction, 12));
                Equal(3_001u,
                    Readˉpublisherˉrequestˉu32(Construction, 48));
                Equal(789u,
                    Readˉpublisherˉrequestˉu32(Construction, 52));
                Equal(0u,
                    Readˉpublisherˉrequestˉu32(Construction, 56));
                Equal(Windows ? 235_394u : 235_077u,
                    Readˉpublisherˉrequestˉu32(Construction, 72));
                Equal(Windows ? 240_016u : 249_856u,
                    Readˉpublisherˉrequestˉu32(Construction, 132));
                Equal(Windows ? 243_600u : 142_929_920u,
                    Readˉpublisherˉrequestˉu32(Construction, 136));
                Equal(Windows ? 256_000u : 254_917u,
                    Readˉpublisherˉrequestˉu32(Construction, 156));
                var Resources = Publisherˉidentityˉresources(Identity);
                var Application = File.ReadAllBytes(Path.Combine(
                    Repository,
                    "Artifacts",
                    "Native-Hosted-Verifier-Application-Publisher-Candidate",
                    Windows
                        ? "windows-x64-wvhostverifierpublish.exe"
                        : "linux-x64-wvhostverifierpublish.elf"));
                var Digestˉinputs = Resources.Append(Application).ToArray();
                for (var Index = 0; Index < Digestˉinputs.Length; Index++)
                {
                    Sequenceˉequal(
                        SHA256.HashData(Digestˉinputs[Index].AsSpan()),
                        Construction.AsSpan()[
                            (192 + Index * 32)..(224 + Index * 32)].ToArray());
                }

                var Targets = Run(3, Structure);
                Equal(Windows ? 192 : 124, Targets.Length);
                Equal(0x5450_5657u,
                    Readˉpublisherˉrequestˉu32(Targets, 0));
                Equal((uint)Target,
                    Readˉpublisherˉrequestˉu32(Targets, 12));
                var Expectedˉtargets = Windows
                    ? WINDOWS_PUBLISHER_TARGETS
                    : LINUX_PUBLISHER_TARGETS;
                Equal(Expectedˉtargets.Length, (Targets.Length - 16) / 4);
                for (var Index = 0; Index < Expectedˉtargets.Length; Index++)
                {
                    Equal(
                        Expectedˉtargets[Index],
                        Readˉpublisherˉrequestˉu32(Targets, 16 + Index * 4));
                }

                var Corrupted = Identity.ToArray();
                var Moduleˉoffset = checked((int)Readˉpublisherˉrequestˉu32(
                    Identity, 16));
                Corrupted[Moduleˉoffset] ^= 1;
                Equal(
                    0,
                    X64ˉnativeˉexecutor.Executeˉbytes(
                        Native[0],
                        Corrupted.ToImmutableArray(),
                        maximumˉinstructions: 100_000_000).Length);
                var Badˉstructure = Structure.ToArray();
                Badˉstructure[0] ^= 1;
                Equal(0, X64ˉnativeˉexecutor.Executeˉbytes(
                    Native[2], Badˉstructure.ToImmutableArray()).Length);
                Equal(0, X64ˉnativeˉexecutor.Executeˉbytes(
                    Native[3], Badˉstructure.ToImmutableArray()).Length);
            }
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static ImmutableArray<byte> Buildˉpublisherˉidentityˉinput(
        string repository,
        Consoleˉapplicationˉtarget target)
    {
        var Windows = target == Consoleˉapplicationˉtarget.Windowsˉx64;
        var Application = File.ReadAllBytes(Path.Combine(
            repository,
            "Artifacts",
            "Native-Hosted-Verifier-Application-Publisher-Candidate",
            Windows
                ? "windows-x64-wvhostverifierpublish.exe"
                : "linux-x64-wvhostverifierpublish.elf"));
        var Resources = new byte[][]
        {
            File.ReadAllBytes(Path.Combine(
                repository,
                "Artifacts",
                "Native-Hosted-Verifier-Application-Publisher-Candidate",
                "Hosted-Verifier-Application-Publisher.wvb")),
            File.ReadAllBytes(Path.Combine(
                repository,
                "Artifacts",
                "Native-Hosted-Verifier-Publisher-Construction-Candidate",
                "Publisher.wvo")),
            File.ReadAllBytes(Path.Combine(
                repository,
                "Linker",
                "Reference",
                "Consumers",
                $"{(Windows ? "Windows" : "Linux")}-X64-Wvb-Publisher.wvo")),
            File.ReadAllBytes(Path.Combine(
                repository,
                "Linker",
                "Reference",
                "Consumers",
                $"{(Windows ? "Windows" : "Linux")}-X64-Wvb-Publication-Adapter.wvo")),
            File.ReadAllBytes(Path.Combine(
                repository,
                "Linker",
                "Reference",
                "Consumers",
                "X64-Wvb-Publication-Sha256.wvo")),
            Application.AsSpan(Windows ? 252_896 : 247_264, 128).ToArray(),
        };
        var Total = 64 + Resources.Sum(Resource => Resource.Length);
        var Result = new byte[Total];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, 0x4950_5657u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), 1u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), (uint)Total);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), (uint)target);
        var Offset = 64;
        for (var Index = 0; Index < Resources.Length; Index++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(16 + Index * 8), (uint)Offset);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(20 + Index * 8), (uint)Resources[Index].Length);
            Resources[Index].CopyTo(Result.AsSpan(Offset));
            Offset += Resources[Index].Length;
        }
        Equal(Total, Offset);
        return Result.ToImmutableArray();
    }

    private static byte[][] Publisherˉidentityˉresources(
        ImmutableArray<byte> identity)
    {
        var Result = new byte[6][];
        for (var Index = 0; Index < Result.Length; Index++)
        {
            var Offset = checked((int)Readˉpublisherˉrequestˉu32(
                identity, 16 + Index * 8));
            var Length = checked((int)Readˉpublisherˉrequestˉu32(
                identity, 20 + Index * 8));
            Result[Index] = identity.AsSpan(Offset, Length).ToArray();
        }
        return Result;
    }

    private static uint Readˉpublisherˉrequestˉu32(
        ImmutableArray<byte> input,
        int offset) => BinaryPrimitives.ReadUInt32LittleEndian(input.AsSpan()[offset..]);

    private static readonly uint[] WINDOWS_PUBLISHER_TARGETS =
    [
        243851, 263216, 262144, 137695232, 258048, 141889536, 258312,
        136646656, 11193, 8192, 8981, 258264, 331776, 240928, 243328,
        241296, 241216, 241200, 258160, 242528, 328752, 2428928, 254192,
        254336, 254200, 254208, 254216, 254224, 254232, 254240, 254248,
        254256, 254264, 254320, 254272, 254280, 254288, 254296, 254304,
        250536, 250537, 250573, 248896, 250186,
    ];

    private static readonly uint[] LINUX_PUBLISHER_TARGETS =
    [
        142929980, 250928, 249856, 137682944, 245760, 141877248, 246024,
        136634368, 11193, 8192, 8981, 245976, 319488, 240928, 243056,
        241248, 241168, 241152, 245872, 242256, 316464, 2416640,
        142934936, 142934937, 142934973, 142933296, 142934586,
    ];
}
