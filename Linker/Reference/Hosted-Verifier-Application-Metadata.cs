using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal enum Hostedˉverifierˉcapability : uint
{
    Consoleˉwriteˉline = 1,
    Diagnosticˉwriteˉline = 2,
    Fileˉreadˉbytes = 3,
    Processˉargument = 4,
    Processˉargumentˉcount = 5,
}

internal enum Hostedˉverifierˉcapabilityˉsignature : uint
{
    Textˉtoˉvoid = 1,
    Textˉtoˉbytes = 2,
    U32ˉtoˉtext = 3,
    Voidˉtoˉu32 = 4,
}

internal enum Hostedˉverifierˉapplicationˉprofile : uint
{
    Compilerˉwvbˉverifier = 2,
    Wvbˉinspector = 4,
    Wvbˉrunner = 5,
}

internal sealed record Verifiedˉhostedˉverifierˉmetadata(
    Consoleˉapplicationˉtarget Target,
    Hostedˉverifierˉapplicationˉprofile Profile,
    uint Bundleˉoffset,
    uint Bundleˉbytes,
    uint Nativeˉimageˉbytes,
    uint Nativeˉentryˉoffset,
    ulong Maximumˉinstructions,
    ImmutableArray<Capabilityˉdeclaration> Capabilities,
    ImmutableArray<Nativeˉserviceˉbundleˉplacement> Services);

internal static class Hostedˉverifierˉapplicationˉmetadata
{
    internal const uint MAGIC = 0x5648_5657;
    internal const uint FORMAT_VERSION = 1;
    internal const uint CONTAINER_FORMAT_VERSION = 4;
    internal const int SIZE = 1024;
    internal const int HEADER_BYTES = 128;
    internal const int CAPABILITY_RECORD_BYTES = 16;
    internal const int SERVICE_RECORD_BYTES = 64;
    internal const int CAPABILITY_COUNT = 5;
    internal const int SERVICE_COUNT = 6;
    internal const int INSPECTOR_SERVICE_COUNT = 11;
    internal const int CAPABILITY_OFFSET = HEADER_BYTES;
    internal const int SERVICE_OFFSET =
        CAPABILITY_OFFSET + CAPABILITY_COUNT * CAPABILITY_RECORD_BYTES;
    internal const int NATIVE_SHA256_OFFSET = 96;
    internal const uint VERIFIER_PROFILE_FLAGS = 2;
    internal const uint INSPECTOR_PROFILE_FLAGS = 4;
    internal const ulong VERIFIER_MAXIMUM_INSTRUCTIONS = 16_000_000_000;

    private static readonly ImmutableArray<Hostedˉverifierˉcapabilityˉcontract>
        CAPABILITY_CONTRACTS =
        [
            new(
                Hostedˉverifierˉcapability.Consoleˉwriteˉline,
                Capabilityˉcatalog.CONSOLE_WRITE_LINE,
                Nativeˉservice.Consoleˉwriteˉline,
                Hostedˉverifierˉcapabilityˉsignature.Textˉtoˉvoid),
            new(
                Hostedˉverifierˉcapability.Diagnosticˉwriteˉline,
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
                Nativeˉservice.Diagnosticˉwriteˉline,
                Hostedˉverifierˉcapabilityˉsignature.Textˉtoˉvoid),
            new(
                Hostedˉverifierˉcapability.Fileˉreadˉbytes,
                Capabilityˉcatalog.FILE_READ_BYTES,
                Nativeˉservice.Fileˉreadˉbytes,
                Hostedˉverifierˉcapabilityˉsignature.Textˉtoˉbytes),
            new(
                Hostedˉverifierˉcapability.Processˉargument,
                Capabilityˉcatalog.PROCESS_ARGUMENT,
                Nativeˉservice.Processˉargument,
                Hostedˉverifierˉcapabilityˉsignature.U32ˉtoˉtext),
            new(
                Hostedˉverifierˉcapability.Processˉargumentˉcount,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
                Nativeˉservice.Processˉargumentˉcount,
                Hostedˉverifierˉcapabilityˉsignature.Voidˉtoˉu32),
        ];
    private static readonly ImmutableArray<Nativeˉservice> REQUIRED_SERVICES =
    [
        Nativeˉservice.Consoleˉwriteˉline,
        Nativeˉservice.Processˉargumentˉcount,
        Nativeˉservice.Processˉargument,
        Nativeˉservice.Fileˉreadˉbytes,
        Nativeˉservice.Textˉutf8ˉisˉvalid,
        Nativeˉservice.Diagnosticˉwriteˉline,
    ];
    private static readonly ImmutableArray<Nativeˉservice> INSPECTOR_REQUIRED_SERVICES =
    [
        Nativeˉservice.Consoleˉwriteˉline,
        Nativeˉservice.Processˉargumentˉcount,
        Nativeˉservice.Processˉargument,
        Nativeˉservice.Fileˉreadˉbytes,
        Nativeˉservice.Textˉutf8ˉisˉvalid,
        Nativeˉservice.Diagnosticˉwriteˉline,
        Nativeˉservice.Enumˉname,
        Nativeˉservice.Textˉconcat,
        Nativeˉservice.Textˉquote,
        Nativeˉservice.I32ˉformat,
        Nativeˉservice.U32ˉformat,
    ];
    private static readonly ImmutableArray<Nativeˉservice> RUNNER_REQUIRED_SERVICES =
    [
        Nativeˉservice.Consoleˉwriteˉline,
        Nativeˉservice.Processˉargumentˉcount,
        Nativeˉservice.Processˉargument,
        Nativeˉservice.Fileˉreadˉbytes,
        Nativeˉservice.Textˉutf8ˉisˉvalid,
        Nativeˉservice.Diagnosticˉwriteˉline,
        Nativeˉservice.Textˉconcat,
        Nativeˉservice.I32ˉformat,
        Nativeˉservice.U32ˉformat,
    ];

    internal static ImmutableArray<byte> Build(
        Consoleˉapplicationˉtarget target,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        Nativeˉserviceˉbundle bundle,
        uint bundleˉoffset,
        uint nativeˉentryˉoffset,
        Hostedˉverifierˉapplicationˉprofile profile =
            Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier)
    {
        Validateˉinputs(
            target,
            capabilities,
            bundle,
            bundleˉoffset,
            nativeˉentryˉoffset,
            profile);
        var Services = Requiredˉservices(profile);
        var Bytes = new byte[SIZE];
        Writeˉu32(Bytes, 0, MAGIC);
        Writeˉu32(Bytes, 4, FORMAT_VERSION);
        Writeˉu32(Bytes, 8, SIZE);
        Writeˉu32(Bytes, 12, (uint)target);
        Writeˉu32(Bytes, 16, CONTAINER_FORMAT_VERSION);
        Writeˉu32(Bytes, 20, Nativeˉcontract.ABI_VERSION);
        Writeˉu32(Bytes, 24, Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION);
        Writeˉu32(Bytes, 28, Nativeˉserviceˉtableˉcontract.FORMAT_VERSION);
        Writeˉu32(Bytes, 32, CAPABILITY_COUNT);
        Writeˉu32(Bytes, 36, checked((uint)Services.Length));
        Writeˉu32(Bytes, 40, CAPABILITY_OFFSET);
        Writeˉu32(Bytes, 44, CAPABILITY_RECORD_BYTES);
        Writeˉu32(Bytes, 48, SERVICE_OFFSET);
        Writeˉu32(Bytes, 52, SERVICE_RECORD_BYTES);
        Writeˉu32(Bytes, 56, bundleˉoffset);
        Writeˉu32(Bytes, 60, checked((uint)bundle.Imageˉbytes.Length));
        Writeˉu32(Bytes, 64, 0);
        Writeˉu32(Bytes, 68, checked((uint)bundle.Nativeˉimageˉbytes));
        Writeˉu32(Bytes, 72, nativeˉentryˉoffset);
        Writeˉu32(Bytes, 76, Nativeˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES);
        Writeˉu32(Bytes, 80, Nativeˉconsoleˉapplicationˉcontract.HOSTED_TEXT_ARENA_BYTES);
        Writeˉu32(Bytes, 84, (uint)profile);
        Writeˉu64(Bytes, 88, VERIFIER_MAXIMUM_INSTRUCTIONS);

        for (var Index = 0; Index < CAPABILITY_CONTRACTS.Length; Index++)
        {
            var Contract = CAPABILITY_CONTRACTS[Index];
            var Offset = CAPABILITY_OFFSET + Index * CAPABILITY_RECORD_BYTES;
            Writeˉu32(Bytes, Offset, (uint)Contract.Identity);
            Writeˉu32(Bytes, Offset + 4, (uint)Contract.Service);
            Writeˉu32(Bytes, Offset + 8, (uint)Contract.Signature);
            Writeˉu32(Bytes, Offset + 12, 1);
        }

        for (var Index = 0; Index < bundle.Placements.Length; Index++)
        {
            var Placement = bundle.Placements[Index];
            var Offset = SERVICE_OFFSET + Index * SERVICE_RECORD_BYTES;
            var Capability = Capabilityˉforˉservice(Placement.Service);
            Writeˉu32(Bytes, Offset, (uint)Placement.Service);
            Writeˉu32(Bytes, Offset + 4, (uint)Capability);
            Writeˉu32(Bytes, Offset + 8, checked((uint)Placement.Serviceˉtableˉoffset));
            Writeˉu32(Bytes, Offset + 12, (uint)Placement.Adapter);
            Writeˉu32(Bytes, Offset + 16, checked((uint)Placement.Imageˉoffset));
            Writeˉu32(Bytes, Offset + 20, checked((uint)Placement.Codeˉbytes));
            Writeˉu32(Bytes, Offset + 24,
                Capability == 0 ? 2u : 1u);
            Convert.FromHexString(Placement.Sha256).CopyTo(Bytes.AsSpan(Offset + 32, 32));
        }

        SHA256.HashData(bundle.Imageˉbytes.AsSpan(0, bundle.Nativeˉimageˉbytes))
            .CopyTo(Bytes.AsSpan(NATIVE_SHA256_OFFSET, 32));
        return Bytes.ToImmutableArray();
    }

    internal static Verifiedˉhostedˉverifierˉmetadata Verify(
        ReadOnlySpan<byte> bytes,
        Consoleˉapplicationˉtarget expectedˉtarget,
        Nativeˉserviceˉbundle expectedˉbundle,
        ReadOnlySpan<byte> actualˉbundleˉimage,
        Hostedˉverifierˉapplicationˉprofile expectedˉprofile =
            Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier)
    {
        var Expectedˉservices = Requiredˉservices(expectedˉprofile);
        if (bytes.Length != SIZE)
        {
            throw Invalid("The hosted verifier metadata has an invalid size.");
        }
        if (!Enum.IsDefined(expectedˉtarget))
        {
            throw new ArgumentOutOfRangeException(nameof(expectedˉtarget), expectedˉtarget, null);
        }
        if (!actualˉbundleˉimage.SequenceEqual(expectedˉbundle.Imageˉbytes.AsSpan()))
        {
            throw Invalid("The hosted verifier service bundle does not match its verified input.");
        }
        Require(bytes, 0, MAGIC, "magic");
        Require(bytes, 4, FORMAT_VERSION, "metadata version");
        Require(bytes, 8, SIZE, "metadata size");
        Require(bytes, 12, (uint)expectedˉtarget, "target");
        Require(bytes, 16, CONTAINER_FORMAT_VERSION, "container version");
        Require(bytes, 20, Nativeˉcontract.ABI_VERSION, "native ABI version");
        Require(bytes, 24, Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION,
            "execution-context version");
        Require(bytes, 28, Nativeˉserviceˉtableˉcontract.FORMAT_VERSION,
            "service-table version");
        Require(bytes, 32, CAPABILITY_COUNT, "capability count");
        Require(bytes, 36, checked((uint)Expectedˉservices.Length), "service count");
        Require(bytes, 40, CAPABILITY_OFFSET, "capability offset");
        Require(bytes, 44, CAPABILITY_RECORD_BYTES, "capability-record size");
        Require(bytes, 48, SERVICE_OFFSET, "service offset");
        Require(bytes, 52, SERVICE_RECORD_BYTES, "service-record size");
        var Bundleˉoffset = Readˉu32(bytes, 56);
        Require(bytes, 60, checked((uint)expectedˉbundle.Imageˉbytes.Length), "bundle size");
        Require(bytes, 64, 0, "native-image offset");
        Require(bytes, 68, checked((uint)expectedˉbundle.Nativeˉimageˉbytes),
            "native-image size");
        var Nativeˉentry = Readˉu32(bytes, 72);
        Require(bytes, 76, Nativeˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES,
            "record-arena size");
        Require(bytes, 80, Nativeˉconsoleˉapplicationˉcontract.HOSTED_TEXT_ARENA_BYTES,
            "text-arena size");
        Require(bytes, 84, (uint)expectedˉprofile, "profile flags");
        Requireˉu64(bytes, 88, VERIFIER_MAXIMUM_INSTRUCTIONS, "instruction budget");
        if (Bundleˉoffset % 16 != 0 || Nativeˉentry >= expectedˉbundle.Nativeˉimageˉbytes)
        {
            throw Invalid("The hosted verifier bundle offset or native entry is invalid.");
        }
        if (!bytes.Slice(NATIVE_SHA256_OFFSET, 32).SequenceEqual(
                SHA256.HashData(expectedˉbundle.Imageˉbytes.AsSpan(
                    0,
                    expectedˉbundle.Nativeˉimageˉbytes))))
        {
            throw Invalid("The hosted verifier native-image digest is invalid.");
        }

        for (var Index = 0; Index < CAPABILITY_CONTRACTS.Length; Index++)
        {
            var Contract = CAPABILITY_CONTRACTS[Index];
            var Offset = CAPABILITY_OFFSET + Index * CAPABILITY_RECORD_BYTES;
            Require(bytes, Offset, (uint)Contract.Identity, "capability identity");
            Require(bytes, Offset + 4, (uint)Contract.Service, "capability service");
            Require(bytes, Offset + 8, (uint)Contract.Signature, "capability signature");
            Require(bytes, Offset + 12, 1, "capability contract version");
        }

        if (expectedˉbundle.Placements.Length != Expectedˉservices.Length ||
            expectedˉbundle.Platform != Platform(expectedˉtarget) ||
            !expectedˉbundle.Placements.Select(Placement => Placement.Service)
                .SequenceEqual(Expectedˉservices))
        {
            throw Invalid("The expected hosted verifier service bundle is invalid.");
        }
        for (var Index = 0; Index < expectedˉbundle.Placements.Length; Index++)
        {
            var Placement = expectedˉbundle.Placements[Index];
            var Offset = SERVICE_OFFSET + Index * SERVICE_RECORD_BYTES;
            var Capability = Capabilityˉforˉservice(Placement.Service);
            Require(bytes, Offset, (uint)Placement.Service, "service identity");
            Require(bytes, Offset + 4, (uint)Capability, "service capability");
            Require(bytes, Offset + 8, checked((uint)Placement.Serviceˉtableˉoffset),
                "service-table slot");
            Require(bytes, Offset + 12, (uint)Placement.Adapter, "service adapter");
            Require(bytes, Offset + 16, checked((uint)Placement.Imageˉoffset),
                "service image offset");
            Require(bytes, Offset + 20, checked((uint)Placement.Codeˉbytes),
                "service code size");
            Require(bytes, Offset + 24, Capability == 0 ? 2u : 1u, "service flags");
            Require(bytes, Offset + 28, 0, "reserved service field");
            if (!bytes.Slice(Offset + 32, 32).SequenceEqual(
                    Convert.FromHexString(Placement.Sha256)))
            {
                throw Invalid("A hosted verifier service digest is invalid.");
            }
        }
        Requireˉzero(
            bytes,
            SERVICE_OFFSET + Expectedˉservices.Length * SERVICE_RECORD_BYTES,
            SIZE - SERVICE_OFFSET - Expectedˉservices.Length * SERVICE_RECORD_BYTES,
            "reserved metadata tail");

        return new(
            expectedˉtarget,
            expectedˉprofile,
            Bundleˉoffset,
            checked((uint)expectedˉbundle.Imageˉbytes.Length),
            checked((uint)expectedˉbundle.Nativeˉimageˉbytes),
            Nativeˉentry,
            VERIFIER_MAXIMUM_INSTRUCTIONS,
            [.. CAPABILITY_CONTRACTS.Select(Contract => Declaration(Contract.Name))],
            expectedˉbundle.Placements);
    }

    private static void Validateˉinputs(
        Consoleˉapplicationˉtarget target,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        Nativeˉserviceˉbundle bundle,
        uint bundleˉoffset,
        uint nativeˉentryˉoffset,
        Hostedˉverifierˉapplicationˉprofile profile)
    {
        var Services = Requiredˉservices(profile);
        if (!Enum.IsDefined(target) ||
            bundle is null ||
            bundle.Platform != Platform(target) ||
            bundleˉoffset % 16 != 0 ||
            nativeˉentryˉoffset >= bundle.Nativeˉimageˉbytes ||
            bundle.Placements.Length != Services.Length ||
            !bundle.Placements.Select(Placement => Placement.Service)
                .SequenceEqual(Services))
        {
            throw new ArgumentException("The hosted verifier bundle contract is invalid.");
        }
        if (capabilities.IsDefault || capabilities.Length != CAPABILITY_COUNT)
        {
            throw new ArgumentException("The hosted verifier capability set is invalid.");
        }
        for (var Index = 0; Index < CAPABILITY_CONTRACTS.Length; Index++)
        {
            var Expected = Declaration(CAPABILITY_CONTRACTS[Index].Name);
            var Actual = capabilities[Index];
            if (Actual is null ||
                !StringComparer.Ordinal.Equals(Actual.Name, Expected.Name) ||
                !Actual.Parameterˉtypes.SequenceEqual(Expected.Parameterˉtypes) ||
                Actual.Returnˉtype != Expected.Returnˉtype)
            {
                throw new ArgumentException("The hosted verifier capability set is noncanonical.");
            }
        }
    }

    internal static ImmutableArray<Nativeˉservice> Requiredˉservices(
        Hostedˉverifierˉapplicationˉprofile profile) => profile switch
    {
        Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier => REQUIRED_SERVICES,
        Hostedˉverifierˉapplicationˉprofile.Wvbˉinspector => INSPECTOR_REQUIRED_SERVICES,
        Hostedˉverifierˉapplicationˉprofile.Wvbˉrunner => RUNNER_REQUIRED_SERVICES,
        _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null),
    };

    private static Capabilityˉdeclaration Declaration(string name)
    {
        if (!Capabilityˉcatalog.Tryˉget(name, out var Declaration))
        {
            throw new InvalidOperationException($"Capability '{name}' left the canonical catalog.");
        }
        return Declaration;
    }

    private static Hostedˉverifierˉcapability Capabilityˉforˉservice(
        Nativeˉservice service) => service switch
    {
        Nativeˉservice.Consoleˉwriteˉline =>
            Hostedˉverifierˉcapability.Consoleˉwriteˉline,
        Nativeˉservice.Processˉargumentˉcount =>
            Hostedˉverifierˉcapability.Processˉargumentˉcount,
        Nativeˉservice.Processˉargument =>
            Hostedˉverifierˉcapability.Processˉargument,
        Nativeˉservice.Fileˉreadˉbytes =>
            Hostedˉverifierˉcapability.Fileˉreadˉbytes,
        Nativeˉservice.Diagnosticˉwriteˉline =>
            Hostedˉverifierˉcapability.Diagnosticˉwriteˉline,
        _ => 0,
    };

    private static Nativeˉserviceˉplatform Platform(Consoleˉapplicationˉtarget target) =>
        target switch
        {
            Consoleˉapplicationˉtarget.Windowsˉx64 => Nativeˉserviceˉplatform.Windows,
            Consoleˉapplicationˉtarget.Linuxˉx64 => Nativeˉserviceˉplatform.Linux,
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null),
        };

    private static void Require(
        ReadOnlySpan<byte> bytes,
        int offset,
        uint expected,
        string field)
    {
        if (Readˉu32(bytes, offset) != expected)
        {
            throw Invalid($"The hosted verifier {field} is invalid.");
        }
    }

    private static void Requireˉu64(
        ReadOnlySpan<byte> bytes,
        int offset,
        ulong expected,
        string field)
    {
        if (Readˉu64(bytes, offset) != expected)
        {
            throw Invalid($"The hosted verifier {field} is invalid.");
        }
    }

    private static void Requireˉzero(
        ReadOnlySpan<byte> bytes,
        int offset,
        int length,
        string field)
    {
        if (!bytes.Slice(offset, length).SequenceEqual(new byte[length]))
        {
            throw Invalid($"The hosted verifier {field} is invalid.");
        }
    }

    private static uint Readˉu32(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(offset, sizeof(uint)));

    private static ulong Readˉu64(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(offset, sizeof(ulong)));

    private static void Writeˉu32(byte[] bytes, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset, sizeof(uint)), value);

    private static void Writeˉu64(byte[] bytes, int offset, ulong value) =>
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(offset, sizeof(ulong)), value);

    private static InvalidDataException Invalid(string message) => new(message);

    private sealed record Hostedˉverifierˉcapabilityˉcontract(
        Hostedˉverifierˉcapability Identity,
        string Name,
        Nativeˉservice Service,
        Hostedˉverifierˉcapabilityˉsignature Signature);
}
