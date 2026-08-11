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
    private static void Windvaleˉnativeˉhostedˉverifierˉpublisherˉobjectsˉinstantiate()
    {
        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-publisher-instantiation-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Publisher-Object-Instantiation.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Verifier-Publisher-Object-Instantiation.wvproj"),
                Moduleˉpath);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            var Moduleˉbytes = File.ReadAllBytes(Moduleˉpath);
            Equal(16_961, Moduleˉbytes.Length);
            Equal(
                "7da724a1cef230fbc815e38edf46fc8f4d9b53a257276cc04ea8858d3902b874",
                Moduleˉdigest.Calculateˉsha256(Moduleˉbytes));
            var Module = Moduleˉcodec.Readˉandˉverify(Moduleˉbytes);
            var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;
            True(Native.Requiredˉservices.IsEmpty,
                "Publisher object instantiation unexpectedly requires a native service.");
            Equal(
                new Nativeˉentryˉshape(
                    Nativeˉentryˉinputˉkind.Bytes,
                    Nativeˉentryˉresultˉkind.Descriptor),
                Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Native));
            var Reference = new Referenceˉruntime(
                Module,
                new Referenceˉcapabilityˉhost(TextWriter.Null),
                Runtimeˉoptions.Portableˉdefaults);

            foreach (var Target in Enum.GetValues<Consoleˉapplicationˉtarget>())
            {
                var Windows = Target == Consoleˉapplicationˉtarget.Windowsˉx64;
                var Request = Buildˉpublisherˉobjectˉinstantiationˉrequest(
                    Repository, Target);
                var Executed = X64ˉnativeˉexecutor.Executeˉbytes(
                    Native, Request, maximumˉinstructions: 10_000_000);
                Sequenceˉequal(Reference.Runˉmainˉbytes(Request).Bytes, Executed);
                Equal(Windows ? 7_040 : 5_169, Executed.Length);
                Equal(0x4f49_5657u, Readˉpublisherˉrequestˉu32(Executed, 0));
                Equal(1u, Readˉpublisherˉrequestˉu32(Executed, 4));
                Equal((uint)Executed.Length, Readˉpublisherˉrequestˉu32(Executed, 8));
                Equal(0u, Readˉpublisherˉrequestˉu32(Executed, 12));
                Equal((uint)Request.Length, Readˉpublisherˉrequestˉu32(Executed, 16));
                Equal((uint)Target, Readˉpublisherˉrequestˉu32(Executed, 20));
                Equal(5u, Readˉpublisherˉrequestˉu32(Executed, 28));
                Equal(Windows ? 5_286u : 3_415u,
                    Readˉpublisherˉrequestˉu32(Executed, 36));
                Equal(1_685u, Readˉpublisherˉrequestˉu32(Executed, 44));

                var Application = File.ReadAllBytes(Path.Combine(
                    Repository,
                    "Artifacts",
                    "Native-Hosted-Verifier-Application-Publisher-Candidate",
                    Windows
                        ? "windows-x64-wvhostverifierpublish.exe"
                        : "linux-x64-wvhostverifierpublish.elf"));
                var Startup = Executed.AsSpan(64, 5);
                var Adapter = Executed.AsSpan(69, Windows ? 5_286 : 3_415);
                var Sha256 = Executed.AsSpan(Windows ? 5_355 : 3_484, 1_685);
                Equal(
                    Windows
                        ? "3f3f7c4230724bf6e2692f232ed3a904705174ed7ba1174012dc1d1ebfa1be93"
                        : "b9540cacf3005d402f438805ab66822c725977d3dc27f66cf1916631f1ba9245",
                    Convert.ToHexString(SHA256.HashData(Adapter)).ToLowerInvariant());
                Sequenceˉequal(
                    Application.AsSpan(Windows ? 512 : 4_096, 5).ToArray(), Startup.ToArray());
                Sequenceˉequal(
                    Application.AsSpan(Windows ? 240_016 : 249_856, Adapter.Length).ToArray(),
                    Adapter.ToArray());
                Sequenceˉequal(
                    Application.AsSpan(Windows ? 245_312 : 253_280, 1_685).ToArray(),
                    Sha256.ToArray());
                Equal(
                    Windows
                        ? "dbb15bae305f7eda414e935e3fcc8ef9ce9a25e9f3fa4d142814545d36fc9e9e"
                        : "22ef5439e468626dc1b46c6c92fed269681b76d2b34325bba4bb1c13dc26b6d7",
                    Convert.ToHexString(SHA256.HashData(Startup)).ToLowerInvariant());
                Equal(
                    Windows
                        ? "3f3f7c4230724bf6e2692f232ed3a904705174ed7ba1174012dc1d1ebfa1be93"
                        : "b9540cacf3005d402f438805ab66822c725977d3dc27f66cf1916631f1ba9245",
                    Convert.ToHexString(SHA256.HashData(Adapter)).ToLowerInvariant());
                Equal(
                    "513d73834e2c6358adad022a31a386be59391874e73e4ad5bf74c70ec0b170ce",
                    Convert.ToHexString(SHA256.HashData(Sha256)).ToLowerInvariant());
                Equal(
                    Windows
                        ? "41591b9e04457c46aa449fb1a2ab8415a29e9146bdb775f46dcf6f9c38a94a16"
                        : "dbe367f7cb9dc4beb39ea14ed3d0fa0f1b470cf2b0f4360d3947ba4ec3f4d0f6",
                    Convert.ToHexString(SHA256.HashData(Executed.AsSpan()[64..])).ToLowerInvariant());
            }

            var Valid = Buildˉpublisherˉobjectˉinstantiationˉrequest(
                Repository, Consoleˉapplicationˉtarget.Windowsˉx64);
            Expectˉpublisherˉobjectˉfailure(Native, Reference, Valid[..47], 1u);
            Expectˉpublisherˉobjectˉfailure(
                Native, Reference, Replaceˉpublisherˉu32(Valid, 20, 0u), 2u);
            Expectˉpublisherˉobjectˉfailure(
                Native, Reference, Replaceˉpublisherˉu32(Valid, 48, 0u), 4u);
            var Corruptˉobject = Valid.ToArray();
            Corruptˉobject[48 + WINDOWS_PUBLISHER_TARGETS.Length * 4] ^= 1;
            Expectˉpublisherˉobjectˉfailure(
                Native, Reference, Corruptˉobject.ToImmutableArray(), 4u);
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static ImmutableArray<byte> Buildˉpublisherˉobjectˉinstantiationˉrequest(
        string repository,
        Consoleˉapplicationˉtarget target)
    {
        var Windows = target == Consoleˉapplicationˉtarget.Windowsˉx64;
        var Targets = Windows ? WINDOWS_PUBLISHER_TARGETS : LINUX_PUBLISHER_TARGETS;
        var Objects = new[]
        {
            File.ReadAllBytes(Path.Combine(repository, "Linker", "Reference", "Consumers",
                $"{(Windows ? "Windows" : "Linux")}-X64-Wvb-Publisher.wvo")),
            File.ReadAllBytes(Path.Combine(repository, "Linker", "Reference", "Consumers",
                $"{(Windows ? "Windows" : "Linux")}-X64-Wvb-Publication-Adapter.wvo")),
            File.ReadAllBytes(Path.Combine(repository, "Linker", "Reference", "Consumers",
                "X64-Wvb-Publication-Sha256.wvo")),
        };
        var Result = new byte[48 + Targets.Length * 4 + Objects.Sum(Object => Object.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, 0x5849_5657u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), 1u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), (uint)Result.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), (uint)target);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), 4_096u);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(20), Windows ? 243_600u : 142_929_920u);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(24), Windows ? 248_896u : 142_933_344u);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(28), (uint)Targets.Length);
        for (var Index = 0; Index < Objects.Length; Index++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(32 + Index * 4),
                (uint)Objects[Index].Length);
        }
        var Offset = 48;
        foreach (var Address in Targets)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(Offset), Address);
            Offset += 4;
        }
        foreach (var Object in Objects)
        {
            Object.CopyTo(Result.AsSpan(Offset));
            Offset += Object.Length;
        }
        Equal(Result.Length, Offset);
        return Result.ToImmutableArray();
    }

    private static ImmutableArray<byte> Replaceˉpublisherˉu32(
        ImmutableArray<byte> input,
        int offset,
        uint value)
    {
        var Result = input.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(offset), value);
        return Result.ToImmutableArray();
    }

    private static void Expectˉpublisherˉobjectˉfailure(
        Nativeˉfragment native,
        Referenceˉruntime reference,
        ImmutableArray<byte> request,
        uint status)
    {
        var Executed = X64ˉnativeˉexecutor.Executeˉbytes(
            native, request, maximumˉinstructions: 10_000_000);
        Sequenceˉequal(reference.Runˉmainˉbytes(request).Bytes, Executed);
        Equal(64, Executed.Length);
        Equal(status, Readˉpublisherˉrequestˉu32(Executed, 12));
    }
}
