using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime;

namespace Windvale.Runtime.Native;

public enum Nativeˉpublicationˉlifetimeˉstatus : uint
{
    Valid = 0,
    Invalidˉsize = 1,
    Invalidˉmagic = 2,
    Invalidˉversion = 3,
    Invalidˉreserved = 4,
    Invalidˉimage = 5,
}

public enum Nativeˉpublicationˉstate : uint
{
    Unallocated = 0,
    Writable = 1,
    Copied = 2,
    Executable = 3,
    Invoked = 4,
    Released = 5,
}

public enum Nativeˉpublicationˉaction : uint
{
    Allocateˉwritable = 1,
    Copyˉimage = 2,
    Sealˉexecutable = 3,
    Invoke = 4,
    Release = 5,
    Complete = 6,
}

public sealed record Nativeˉpublicationˉtransition(
    Nativeˉpublicationˉstate State,
    Nativeˉpublicationˉaction Action,
    Nativeˉpublicationˉstate Nextˉstate);

public sealed record Nativeˉpublicationˉlifetimeˉplan(
    int Imageˉbytes,
    ImmutableArray<Nativeˉpublicationˉtransition> Transitions);

public static class X64ˉnativeˉpublicationˉlifetime
{
    public const uint REQUEST_MAGIC = 0x514C_5657;
    public const uint RESPONSE_MAGIC = 0x544C_5657;
    public const uint FORMAT_VERSION = 1;
    public const int REQUEST_BYTES = 20;
    public const int RESPONSE_HEADER_BYTES = 32;
    public const int TRANSITION_RECORD_BYTES = 12;
    public const int TRANSITION_COUNT = 9;
    public const int RESPONSE_BYTES = RESPONSE_HEADER_BYTES + TRANSITION_COUNT * TRANSITION_RECORD_BYTES;
    public const int PLANNER_CANONICAL_SIZE = 4_564;
    public const string PLANNER_CANONICAL_SHA256 =
        "9f7b3c331f4afb56d2e55d51cdea32c5b1536e6856f8da20ade1479e75682bcf";

    private const string REQUEST_NAME = "native-publication-lifetime-request.bin";
    private const string PLANNER_RESOURCE = "Windvale.Native.Native-Publication-Lifetime-Bridge.wvb";
    private const long MAXIMUM_PLANNER_INSTRUCTIONS = 100_000;
    private static readonly ImmutableHashSet<string> AUTHORIZED_CAPABILITIES =
        ImmutableHashSet.Create(StringComparer.Ordinal, Capabilityˉcatalog.FILE_READ_BYTES);
    private static readonly ImmutableArray<Nativeˉpublicationˉtransition> EXPECTED_TRANSITIONS =
    [
        new(Nativeˉpublicationˉstate.Unallocated, Nativeˉpublicationˉaction.Allocateˉwritable, Nativeˉpublicationˉstate.Writable),
        new(Nativeˉpublicationˉstate.Writable, Nativeˉpublicationˉaction.Copyˉimage, Nativeˉpublicationˉstate.Copied),
        new(Nativeˉpublicationˉstate.Writable, Nativeˉpublicationˉaction.Release, Nativeˉpublicationˉstate.Released),
        new(Nativeˉpublicationˉstate.Copied, Nativeˉpublicationˉaction.Sealˉexecutable, Nativeˉpublicationˉstate.Executable),
        new(Nativeˉpublicationˉstate.Copied, Nativeˉpublicationˉaction.Release, Nativeˉpublicationˉstate.Released),
        new(Nativeˉpublicationˉstate.Executable, Nativeˉpublicationˉaction.Invoke, Nativeˉpublicationˉstate.Invoked),
        new(Nativeˉpublicationˉstate.Executable, Nativeˉpublicationˉaction.Release, Nativeˉpublicationˉstate.Released),
        new(Nativeˉpublicationˉstate.Invoked, Nativeˉpublicationˉaction.Release, Nativeˉpublicationˉstate.Released),
        new(Nativeˉpublicationˉstate.Released, Nativeˉpublicationˉaction.Complete, Nativeˉpublicationˉstate.Released),
    ];
    private static readonly Lazy<Verifiedˉmodule> PLANNER = new(
        Readˉplanner,
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static Nativeˉpublicationˉlifetimeˉplan Plan(int imageˉbytes)
    {
        Validateˉimage(imageˉbytes);
        var Request = Buildˉrequest(imageˉbytes);
        var Response = Evaluateˉrequest(Request);
        return Verifyˉresponse(imageˉbytes, Response);
    }

    public static ImmutableArray<byte> Buildˉrequest(int imageˉbytes)
    {
        Validateˉimage(imageˉbytes);
        var Bytes = new byte[REQUEST_BYTES];
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(0), REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(4), FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(8), REQUEST_BYTES);
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(12), checked((uint)imageˉbytes));
        return Bytes.ToImmutableArray();
    }

    public static ImmutableArray<byte> Evaluateˉrequest(ImmutableArray<byte> request)
    {
        if (request.IsDefault || request.Length > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
        {
            throw new ArgumentException(
                "The native publication-lifetime request must be an initialized bounded byte value.",
                nameof(request));
        }

        var Resources = new Hostedˉresourceˉcontext(
            [],
            TextWriter.Null,
            TextWriter.Null,
            new Lifetimeˉrequestˉreader(request));
        var Result = new Referenceˉruntime(
            PLANNER.Value,
            new Referenceˉcapabilityˉhost(Resources),
            new Runtimeˉoptions(
                AUTHORIZED_CAPABILITIES,
                MAXIMUM_PLANNER_INSTRUCTIONS,
                Nativeˉcontract.DEFAULT_MAXIMUM_CALL_DEPTH))
            .Runˉmainˉbytes();
        return Result.Bytes;
    }

    public static Nativeˉpublicationˉlifetimeˉplan Verifyˉresponse(
        int imageˉbytes,
        ImmutableArray<byte> response)
    {
        Validateˉimage(imageˉbytes);
        if (response.IsDefault || response.Length < RESPONSE_HEADER_BYTES)
        {
            throw Invalidˉresponse("The Windvale publication-lifetime response is truncated.");
        }

        var Span = response.AsSpan();
        if (BinaryPrimitives.ReadUInt32LittleEndian(Span) != RESPONSE_MAGIC ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[4..]) != FORMAT_VERSION)
        {
            throw Invalidˉresponse("The Windvale publication-lifetime response envelope is invalid.");
        }
        var Declaredˉsize = BinaryPrimitives.ReadUInt32LittleEndian(Span[8..]);
        var Status = (Nativeˉpublicationˉlifetimeˉstatus)BinaryPrimitives.ReadUInt32LittleEndian(Span[12..]);
        var Failureˉoffset = BinaryPrimitives.ReadUInt32LittleEndian(Span[16..]);
        if (!Enum.IsDefined(Status))
        {
            throw Invalidˉresponse("The Windvale publication-lifetime response status is unknown.");
        }
        if (Status != Nativeˉpublicationˉlifetimeˉstatus.Valid)
        {
            throw new Nativeˉbackendˉexception(
                "WVN4015",
                $"The Windvale publication-lifetime planner rejected its verified host request with " +
                $"status {Status} at offset {Failureˉoffset}.");
        }
        if (Declaredˉsize != RESPONSE_BYTES ||
            response.Length != RESPONSE_BYTES ||
            Failureˉoffset != REQUEST_BYTES ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[20..]) != (uint)imageˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[24..]) != TRANSITION_COUNT ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[28..]) != 0)
        {
            throw Invalidˉresponse("The Windvale publication-lifetime response shape is inconsistent.");
        }

        var Transitions = ImmutableArray.CreateBuilder<Nativeˉpublicationˉtransition>(TRANSITION_COUNT);
        for (var Index = 0; Index < TRANSITION_COUNT; Index++)
        {
            var Offset = checked(RESPONSE_HEADER_BYTES + Index * TRANSITION_RECORD_BYTES);
            var Transition = new Nativeˉpublicationˉtransition(
                (Nativeˉpublicationˉstate)BinaryPrimitives.ReadUInt32LittleEndian(Span[Offset..]),
                (Nativeˉpublicationˉaction)BinaryPrimitives.ReadUInt32LittleEndian(Span[(Offset + sizeof(uint))..]),
                (Nativeˉpublicationˉstate)BinaryPrimitives.ReadUInt32LittleEndian(Span[(Offset + 2 * sizeof(uint))..]));
            if (Transition != EXPECTED_TRANSITIONS[Index])
            {
                throw Invalidˉresponse("A Windvale publication-lifetime transition is inconsistent.");
            }
            Transitions.Add(Transition);
        }
        var Plan = new Nativeˉpublicationˉlifetimeˉplan(
            imageˉbytes,
            Transitions.MoveToImmutable());
        Verifyˉplan(Plan);
        return Plan;
    }

    public static void Verifyˉplan(Nativeˉpublicationˉlifetimeˉplan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        Validateˉimage(plan.Imageˉbytes);
        if (plan.Transitions.IsDefault ||
            plan.Transitions.Length != EXPECTED_TRANSITIONS.Length)
        {
            throw Invalidˉresponse("The native publication-lifetime plan is incomplete.");
        }
        for (var Index = 0; Index < EXPECTED_TRANSITIONS.Length; Index++)
        {
            if (plan.Transitions[Index] != EXPECTED_TRANSITIONS[Index])
            {
                throw Invalidˉresponse("The native publication-lifetime plan is inconsistent.");
            }
        }
    }

    private static void Validateˉimage(int imageˉbytes)
    {
        if (imageˉbytes is < 1 or > X64ˉnativeˉpublicationˉlayout.MAXIMUM_IMAGE_BYTES)
        {
            throw new Nativeˉbackendˉexception(
                "WVN4015",
                "The native publication-lifetime image extent is outside its bounded range.");
        }
    }

    private static Verifiedˉmodule Readˉplanner()
    {
        using var Stream = typeof(X64ˉnativeˉpublicationˉlifetime).Assembly
            .GetManifestResourceStream(PLANNER_RESOURCE) ??
            throw Invalidˉplanner();
        if (Stream.Length != PLANNER_CANONICAL_SIZE)
        {
            throw Invalidˉplanner();
        }
        var Bytes = new byte[PLANNER_CANONICAL_SIZE];
        Stream.ReadExactly(Bytes);
        var Hash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, PLANNER_CANONICAL_SHA256))
        {
            throw Invalidˉplanner();
        }
        return Moduleˉcodec.Readˉandˉverify(Bytes);
    }

    private static Nativeˉbackendˉexception Invalidˉresponse(string message) =>
        new("WVN4016", message);

    private static InvalidOperationException Invalidˉplanner() =>
        new("The retained Windvale native publication-lifetime planner failed its exact identity contract.");

    private sealed class Lifetimeˉrequestˉreader(ImmutableArray<byte> request)
        : IHostedˉfileˉreader
    {
        public ImmutableArray<byte> Readˉbytes(string resourceˉname, int maximumˉbytes)
        {
            if (!StringComparer.Ordinal.Equals(resourceˉname, REQUEST_NAME))
            {
                throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Notˉfound,
                    "The native publication-lifetime planner requested an unknown resource.");
            }
            if (request.Length > maximumˉbytes)
            {
                throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Tooˉlarge,
                    "The native publication-lifetime request exceeds the hosted byte limit.");
            }
            return request;
        }
    }
}
