using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Runtime;

namespace Windvale.Linker;

internal enum Consoleˉapplicationˉverificationˉstatus : uint
{
    Valid = 0,
    Invalidˉchunk = 1,
    Invalidˉsize = 2,
    Invalidˉidentity = 3,
    Invalidˉnativeˉimage = 4,
    Invalidˉentry = 5,
    Invalidˉrecipe = 6,
    Invalidˉcontainer = 7,
}

internal sealed record Verifiedˉconsoleˉapplication(
    Consoleˉapplicationˉtarget Target,
    ImmutableArray<byte> Nativeˉimageˉbytes,
    uint Nativeˉentryˉoffset);

internal sealed class Consoleˉapplicationˉverificationˉexception(
    Consoleˉapplicationˉverificationˉstatus status,
    uint failureˉoffset,
    string message) : Exception(message)
{
    internal Consoleˉapplicationˉverificationˉstatus Status { get; } = status;

    internal uint Failureˉoffset { get; } = failureˉoffset;
}

internal static class Consoleˉapplicationˉverification
{
    internal const uint EVIDENCE_MAGIC = 0x5643_5657;
    internal const uint FORMAT_VERSION = 1;
    internal const int EVIDENCE_BYTES = 36;
    internal const int FIRST_CHUNK_BYTES = Bytecodeˉlimits.MAX_BYTE_DATA_BYTES;
    internal const int SECOND_CHUNK_MAXIMUM_BYTES = 8_304;
    internal const int VERIFIER_CANONICAL_SIZE = 46_150;
    internal const string VERIFIER_CANONICAL_SHA256 =
        "74542907a1b7a90d6d13ee157e7a9e7a4e60e83c042a5486e2f0ab3113ad6013";

    private const string FIRST_RESOURCE_NAME = "console-application-first.bin";
    private const string SECOND_RESOURCE_NAME = "console-application-second.bin";
    private const string NATIVE_IMAGE_RESOURCE_NAME = "console-application-native-image.bin";
    private const string VERIFIER_RESOURCE =
        "Windvale.Linker.Console-Application-Verification-Bridge.wvb";
    private const long MAXIMUM_VERIFIER_INSTRUCTIONS = 10_000_000;
    private static readonly ImmutableHashSet<string> AUTHORIZED_CAPABILITIES =
        ImmutableHashSet.Create(
            StringComparer.Ordinal,
            Capabilityˉcatalog.FILE_READ_BYTES,
            Capabilityˉcatalog.FILE_WRITE_BYTES);
    private static readonly Lazy<Verifiedˉmodule> VERIFIER = new(
        Readˉverifier,
        LazyThreadSafetyMode.ExecutionAndPublication);

    internal static Verifiedˉconsoleˉapplication Verify(ReadOnlySpan<byte> application)
    {
        if (application.Length > FIRST_CHUNK_BYTES + SECOND_CHUNK_MAXIMUM_BYTES)
        {
            throw Invalidˉapplication(
                Consoleˉapplicationˉverificationˉstatus.Invalidˉchunk,
                FIRST_CHUNK_BYTES,
                "The console application exceeds the segmented verifier boundary.");
        }

        var Firstˉbytes = Math.Min(application.Length, FIRST_CHUNK_BYTES);
        var First = application[..Firstˉbytes].ToArray().ToImmutableArray();
        var Second = application[Firstˉbytes..].ToArray().ToImmutableArray();
        var Evaluation = Evaluateˉchunks(First, Second);
        return Verifyˉevidence(application.Length, Evaluation.Evidence, Evaluation.Writes);
    }

    internal static Consoleˉapplicationˉverificationˉevaluation Evaluateˉchunks(
        ImmutableArray<byte> first,
        ImmutableArray<byte> second)
    {
        if (first.IsDefault || second.IsDefault ||
            first.Length > FIRST_CHUNK_BYTES ||
            second.Length > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
        {
            throw new ArgumentException(
                "Console-application verifier chunks must be initialized bounded byte values.");
        }

        var Reader = new Verificationˉreader(first, second);
        var Writer = new Verificationˉwriter();
        var Resources = new Hostedˉresourceˉcontext(
            [],
            TextWriter.Null,
            TextWriter.Null,
            Reader,
            Writer);
        var Evidence = new Referenceˉruntime(
            VERIFIER.Value,
            new Referenceˉcapabilityˉhost(Resources),
            new Runtimeˉoptions(
                AUTHORIZED_CAPABILITIES,
                MAXIMUM_VERIFIER_INSTRUCTIONS,
                1_024))
            .Runˉmainˉbytes()
            .Bytes;
        return new(Evidence, Writer.Writes);
    }

    internal static Verifiedˉconsoleˉapplication Verifyˉevidence(
        int applicationˉbytes,
        ImmutableArray<byte> evidence,
        ImmutableArray<Consoleˉapplicationˉverificationˉwrite> writes)
    {
        if (evidence.IsDefault || evidence.Length != EVIDENCE_BYTES)
        {
            throw Invalidˉverifier("The Windvale console-application verifier returned malformed evidence.");
        }

        var Span = evidence.AsSpan();
        if (Readˉu32(Span, 0) != EVIDENCE_MAGIC ||
            Readˉu32(Span, 4) != FORMAT_VERSION ||
            Readˉu32(Span, 8) != EVIDENCE_BYTES)
        {
            throw Invalidˉverifier("The Windvale console-application verifier evidence envelope is invalid.");
        }

        var Status = (Consoleˉapplicationˉverificationˉstatus)Readˉu32(Span, 12);
        if (!Enum.IsDefined(Status))
        {
            throw Invalidˉverifier("The Windvale console-application verifier status is unknown.");
        }

        var Failureˉoffset = Readˉu32(Span, 16);
        var Targetˉvalue = Readˉu32(Span, 20);
        var Target = (Consoleˉapplicationˉtarget)Targetˉvalue;
        var Claimedˉapplicationˉbytes = Readˉu32(Span, 24);
        var Nativeˉimageˉbytes = Readˉu32(Span, 28);
        var Nativeˉentryˉoffset = Readˉu32(Span, 32);
        if (Claimedˉapplicationˉbytes != checked((uint)applicationˉbytes))
        {
            throw Invalidˉverifier("The Windvale verifier evidence reports the wrong application length.");
        }

        if (Status != Consoleˉapplicationˉverificationˉstatus.Valid)
        {
            if (Failureˉoffset > Claimedˉapplicationˉbytes ||
                Targetˉvalue > (uint)Consoleˉapplicationˉtarget.Linuxˉx64 ||
                writes.Length != 0 ||
                Nativeˉimageˉbytes != 0 ||
                Nativeˉentryˉoffset != 0)
            {
                throw Invalidˉverifier(
                    "A rejected Windvale console application produced native-image output.");
            }
            throw Invalidˉapplication(
                Status,
                Failureˉoffset,
                $"Windvale rejected the console application with status {Status} at offset {Failureˉoffset}.");
        }

        if (!Enum.IsDefined(Target) ||
            Failureˉoffset != Claimedˉapplicationˉbytes ||
            Nativeˉimageˉbytes is 0 or > Consoleˉapplicationˉlayout.MAXIMUM_NATIVE_IMAGE_BYTES ||
            Nativeˉentryˉoffset >= Nativeˉimageˉbytes ||
            writes.Length != 1 ||
            !StringComparer.Ordinal.Equals(writes[0].Resourceˉname, NATIVE_IMAGE_RESOURCE_NAME) ||
            writes[0].Bytes.IsDefault ||
            writes[0].Bytes.Length != Nativeˉimageˉbytes)
        {
            throw Invalidˉverifier("The accepted Windvale console-application evidence is inconsistent.");
        }

        return new(Target, writes[0].Bytes, Nativeˉentryˉoffset);
    }

    private static Verifiedˉmodule Readˉverifier()
    {
        using var Stream = typeof(Consoleˉapplicationˉverification).Assembly
            .GetManifestResourceStream(VERIFIER_RESOURCE) ??
            throw Invalidˉverifier();
        if (Stream.Length != VERIFIER_CANONICAL_SIZE)
        {
            throw Invalidˉverifier();
        }
        var Bytes = new byte[VERIFIER_CANONICAL_SIZE];
        Stream.ReadExactly(Bytes);
        var Hash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, VERIFIER_CANONICAL_SHA256))
        {
            throw Invalidˉverifier();
        }
        return Moduleˉcodec.Readˉandˉverify(Bytes);
    }

    private static uint Readˉu32(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(offset, sizeof(uint)));

    private static Consoleˉapplicationˉverificationˉexception Invalidˉapplication(
        Consoleˉapplicationˉverificationˉstatus status,
        uint failureˉoffset,
        string message) => new(status, failureˉoffset, message);

    private static InvalidOperationException Invalidˉverifier(
        string message = "The retained Windvale console-application verifier failed its exact identity contract.") =>
        new(message);

    private sealed class Verificationˉreader(
        ImmutableArray<byte> first,
        ImmutableArray<byte> second) : IHostedˉfileˉreader
    {
        public ImmutableArray<byte> Readˉbytes(string resourceˉname, int maximumˉbytes)
        {
            ImmutableArray<byte> Result;
            if (StringComparer.Ordinal.Equals(resourceˉname, FIRST_RESOURCE_NAME))
            {
                Result = first;
            }
            else if (StringComparer.Ordinal.Equals(resourceˉname, SECOND_RESOURCE_NAME))
            {
                Result = second;
            }
            else
            {
                throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Notˉfound,
                    "The console-application verifier requested an unknown input resource.");
            }
            if (Result.Length > maximumˉbytes)
            {
                throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Tooˉlarge,
                    "A console-application verifier input chunk exceeds the hosted byte limit.");
            }
            return Result;
        }
    }

    private sealed class Verificationˉwriter : IHostedˉfileˉwriter
    {
        private readonly ImmutableArray<Consoleˉapplicationˉverificationˉwrite>.Builder Items =
            ImmutableArray.CreateBuilder<Consoleˉapplicationˉverificationˉwrite>();

        internal ImmutableArray<Consoleˉapplicationˉverificationˉwrite> Writes =>
            Items.ToImmutable();

        public void Writeˉbytes(
            string resourceˉname,
            ImmutableArray<byte> bytes,
            int maximumˉbytes)
        {
            if (bytes.IsDefault || bytes.Length > maximumˉbytes)
            {
                throw Invalidˉverifier("The Windvale verifier wrote an invalid native-image value.");
            }
            Items.Add(new(resourceˉname, bytes));
        }
    }
}

internal sealed record Consoleˉapplicationˉverificationˉwrite(
    string Resourceˉname,
    ImmutableArray<byte> Bytes);

internal sealed record Consoleˉapplicationˉverificationˉevaluation(
    ImmutableArray<byte> Evidence,
    ImmutableArray<Consoleˉapplicationˉverificationˉwrite> Writes);
