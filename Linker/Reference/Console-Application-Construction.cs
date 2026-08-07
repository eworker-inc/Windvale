using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime;

namespace Windvale.Linker;

internal enum Consoleˉapplicationˉconstructionˉsegmentˉkind : uint
{
    Literal = 1,
    Nativeˉimage = 2,
}

internal sealed record Consoleˉapplicationˉconstructionˉsegment(
    Consoleˉapplicationˉconstructionˉsegmentˉkind Kind,
    int Offset,
    int Length);

internal sealed class Consoleˉapplicationˉconstructionˉexception(string message) : Exception(message);

internal static class Consoleˉapplicationˉconstruction
{
    internal const uint RESPONSE_MAGIC = 0x4343_5657;
    internal const uint FORMAT_VERSION = 1;
    internal const int HEADER_BYTES = 40;
    internal const int SEGMENT_BYTES = 12;
    internal const int WINDOWS_RESPONSE_BYTES = 834;
    internal const int LINUX_RESPONSE_BYTES = 4_454;
    internal const int CONSTRUCTOR_CANONICAL_SIZE = 29_322;
    internal const string CONSTRUCTOR_CANONICAL_SHA256 =
        "4729dd849c72aaa4250d7d54024e8820f827fd08588fa7ba9e5493ab4b8a5d8d";

    private const string REQUEST_NAME = "console-application-construction-request.bin";
    private const string CONSTRUCTOR_RESOURCE =
        "Windvale.Linker.Console-Application-Construction-Bridge.wvb";
    private const long MAXIMUM_CONSTRUCTOR_INSTRUCTIONS = 5_000_000;
    private static readonly ImmutableHashSet<string> AUTHORIZED_CAPABILITIES =
        ImmutableHashSet.Create(StringComparer.Ordinal, Capabilityˉcatalog.FILE_READ_BYTES);
    private static readonly Lazy<Verifiedˉmodule> CONSTRUCTOR = new(
        Readˉconstructor,
        LazyThreadSafetyMode.ExecutionAndPublication);

    internal static byte[] Construct(
        Consoleˉapplicationˉtarget target,
        ReadOnlySpan<byte> nativeˉimage,
        uint nativeˉentryˉoffset,
        Consoleˉapplicationˉplan plan,
        ReadOnlySpan<byte> recoveryˉimage)
    {
        var Request = Consoleˉapplicationˉlayout.Buildˉrequest(
            target,
            nativeˉimage.Length,
            nativeˉentryˉoffset);
        var Response = Evaluateˉrequest(Request);
        return Verifyˉandˉmaterialize(
            target,
            nativeˉimage,
            nativeˉentryˉoffset,
            plan,
            Response,
            recoveryˉimage);
    }

    internal static ImmutableArray<byte> Evaluateˉrequest(ImmutableArray<byte> request)
    {
        if (request.IsDefault || request.Length > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
        {
            throw new ArgumentException(
                "The console-application construction request must be an initialized bounded byte value.",
                nameof(request));
        }

        var Resources = new Hostedˉresourceˉcontext(
            [],
            TextWriter.Null,
            TextWriter.Null,
            new Constructionˉrequestˉreader(request));
        return new Referenceˉruntime(
            CONSTRUCTOR.Value,
            new Referenceˉcapabilityˉhost(Resources),
            new Runtimeˉoptions(
                AUTHORIZED_CAPABILITIES,
                MAXIMUM_CONSTRUCTOR_INSTRUCTIONS,
                1_024))
            .Runˉmainˉbytes()
            .Bytes;
    }

    internal static byte[] Verifyˉandˉmaterialize(
        Consoleˉapplicationˉtarget target,
        ReadOnlySpan<byte> nativeˉimage,
        uint nativeˉentryˉoffset,
        Consoleˉapplicationˉplan plan,
        ImmutableArray<byte> response,
        ReadOnlySpan<byte> recoveryˉimage)
    {
        if (plan.Target != target ||
            plan.Nativeˉimageˉbytes != nativeˉimage.Length ||
            plan.Nativeˉentryˉoffset != nativeˉentryˉoffset)
        {
            throw new ArgumentException(
                "The verified console-application layout does not match the construction input.",
                nameof(plan));
        }
        if (response.IsDefault || response.Length < HEADER_BYTES)
        {
            throw Invalidˉresponse("The Windvale console-application construction is truncated.");
        }

        var Span = response.AsSpan();
        if (Readˉu32(Span, 0) != RESPONSE_MAGIC ||
            Readˉu32(Span, 4) != FORMAT_VERSION)
        {
            throw Invalidˉresponse("The Windvale console-application construction envelope is invalid.");
        }

        var Status = (Consoleˉapplicationˉplanˉstatus)Readˉu32(Span, 12);
        var Failureˉoffset = Readˉu32(Span, 16);
        if (!Enum.IsDefined(Status))
        {
            throw Invalidˉresponse("The Windvale console-application construction status is unknown.");
        }
        if (Status != Consoleˉapplicationˉplanˉstatus.Valid)
        {
            throw new Consoleˉapplicationˉconstructionˉexception(
                $"The Windvale console-application constructor rejected its host request with " +
                $"status {Status} at offset {Failureˉoffset}.");
        }

        var Expectedˉsegments = Buildˉexpectedˉsegments(target, nativeˉimage.Length, plan);
        var Expectedˉresponseˉbytes = target == Consoleˉapplicationˉtarget.Windowsˉx64
            ? WINDOWS_RESPONSE_BYTES
            : LINUX_RESPONSE_BYTES;
        if (response.Length != Expectedˉresponseˉbytes ||
            Readˉu32(Span, 8) != Expectedˉresponseˉbytes ||
            Failureˉoffset != Consoleˉapplicationˉlayout.REQUEST_BYTES ||
            Readˉu32(Span, 20) != (uint)target ||
            Readˉu32(Span, 24) != plan.Applicationˉbytes ||
            Readˉu32(Span, 28) != nativeˉimage.Length ||
            Readˉu32(Span, 32) != nativeˉentryˉoffset ||
            Readˉu32(Span, 36) != Expectedˉsegments.Length)
        {
            throw Invalidˉresponse("The Windvale console-application construction shape is inconsistent.");
        }

        if (recoveryˉimage.Length != plan.Applicationˉbytes)
        {
            throw new ArgumentException(
                "The recovery console application does not match the verified layout.",
                nameof(recoveryˉimage));
        }

        var Result = new byte[plan.Applicationˉbytes];
        var Payloadˉoffset = checked(HEADER_BYTES + (Expectedˉsegments.Length * SEGMENT_BYTES));
        var Previousˉend = 0;
        for (var Index = 0; Index < Expectedˉsegments.Length; Index++)
        {
            var Descriptorˉoffset = checked(HEADER_BYTES + (Index * SEGMENT_BYTES));
            var Actual = new Consoleˉapplicationˉconstructionˉsegment(
                (Consoleˉapplicationˉconstructionˉsegmentˉkind)Readˉu32(
                    Span,
                    Descriptorˉoffset),
                Readˉint(Span, Descriptorˉoffset + 4),
                Readˉint(Span, Descriptorˉoffset + 8));
            var Expected = Expectedˉsegments[Index];
            if (Actual != Expected ||
                !Enum.IsDefined(Actual.Kind) ||
                Actual.Offset < Previousˉend ||
                Actual.Offset > Result.Length ||
                Actual.Length > Result.Length - Actual.Offset)
            {
                throw Invalidˉresponse(
                    $"The Windvale console-application construction segment {Index} is invalid.");
            }

            if (Actual.Kind == Consoleˉapplicationˉconstructionˉsegmentˉkind.Literal)
            {
                if (Actual.Length > response.Length - Payloadˉoffset)
                {
                    throw Invalidˉresponse(
                        "The Windvale console-application construction literal is truncated.");
                }
                Span.Slice(Payloadˉoffset, Actual.Length).CopyTo(
                    Result.AsSpan(Actual.Offset, Actual.Length));
                Payloadˉoffset = checked(Payloadˉoffset + Actual.Length);
            }
            else
            {
                nativeˉimage.CopyTo(Result.AsSpan(Actual.Offset, Actual.Length));
            }
            Previousˉend = checked(Actual.Offset + Actual.Length);
        }

        if (Payloadˉoffset != response.Length)
        {
            throw Invalidˉresponse(
                "The Windvale console-application construction has trailing payload bytes.");
        }
        if (!Result.AsSpan().SequenceEqual(recoveryˉimage))
        {
            throw Invalidˉresponse(
                "The Windvale console-application construction disagrees with the recovery oracle.");
        }
        return Result;
    }

    private static ImmutableArray<Consoleˉapplicationˉconstructionˉsegment> Buildˉexpectedˉsegments(
        Consoleˉapplicationˉtarget target,
        int nativeˉimageˉbytes,
        Consoleˉapplicationˉplan plan)
    {
        if (target == Consoleˉapplicationˉtarget.Windowsˉx64)
        {
            return
            [
                new(Consoleˉapplicationˉconstructionˉsegmentˉkind.Literal, 0, plan.Headerˉbytes),
                new(
                    Consoleˉapplicationˉconstructionˉsegmentˉkind.Literal,
                    plan.Textˉfileˉoffset,
                    plan.Startupˉbytes),
                new(
                    Consoleˉapplicationˉconstructionˉsegmentˉkind.Nativeˉimage,
                    checked(plan.Textˉfileˉoffset + plan.Nativeˉimageˉoffset),
                    nativeˉimageˉbytes),
                new(
                    Consoleˉapplicationˉconstructionˉsegmentˉkind.Literal,
                    checked((int)plan.Dataˉfileˉoffset),
                    checked((int)Nativeˉexecutionˉcontextˉcontract.SIZE)),
                new(
                    Consoleˉapplicationˉconstructionˉsegmentˉkind.Literal,
                    checked((int)plan.Metadataˉfileˉoffset),
                    checked((int)plan.Metadataˉvirtualˉbytes)),
            ];
        }

        return
        [
            new(Consoleˉapplicationˉconstructionˉsegmentˉkind.Literal, 0, plan.Headerˉbytes),
            new(
                Consoleˉapplicationˉconstructionˉsegmentˉkind.Literal,
                plan.Textˉfileˉoffset,
                plan.Startupˉbytes),
            new(
                Consoleˉapplicationˉconstructionˉsegmentˉkind.Nativeˉimage,
                checked(plan.Textˉfileˉoffset + plan.Nativeˉimageˉoffset),
                nativeˉimageˉbytes),
            new(
                Consoleˉapplicationˉconstructionˉsegmentˉkind.Literal,
                checked((int)plan.Dataˉfileˉoffset),
                checked((int)Nativeˉexecutionˉcontextˉcontract.SIZE)),
        ];
    }

    private static Verifiedˉmodule Readˉconstructor()
    {
        using var Stream = typeof(Consoleˉapplicationˉconstruction).Assembly
            .GetManifestResourceStream(CONSTRUCTOR_RESOURCE) ??
            throw Invalidˉconstructor();
        if (Stream.Length != CONSTRUCTOR_CANONICAL_SIZE)
        {
            throw Invalidˉconstructor();
        }
        var Bytes = new byte[CONSTRUCTOR_CANONICAL_SIZE];
        Stream.ReadExactly(Bytes);
        var Hash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, CONSTRUCTOR_CANONICAL_SHA256))
        {
            throw Invalidˉconstructor();
        }
        return Moduleˉcodec.Readˉandˉverify(Bytes);
    }

    private static int Readˉint(ReadOnlySpan<byte> bytes, int offset)
    {
        var Value = Readˉu32(bytes, offset);
        if (Value > int.MaxValue)
        {
            throw Invalidˉresponse(
                "A Windvale console-application construction field exceeds the host bound.");
        }
        return (int)Value;
    }

    private static uint Readˉu32(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(offset, sizeof(uint)));

    private static Consoleˉapplicationˉconstructionˉexception Invalidˉresponse(string message) =>
        new(message);

    private static InvalidOperationException Invalidˉconstructor() =>
        new("The retained Windvale console-application constructor failed its exact identity contract.");

    private sealed class Constructionˉrequestˉreader(
        ImmutableArray<byte> request) : IHostedˉfileˉreader
    {
        public ImmutableArray<byte> Readˉbytes(string resourceˉname, int maximumˉbytes)
        {
            if (!StringComparer.Ordinal.Equals(resourceˉname, REQUEST_NAME))
            {
                throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Notˉfound,
                    "The console-application constructor requested an unknown resource.");
            }
            if (request.Length > maximumˉbytes)
            {
                throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Tooˉlarge,
                    "The console-application construction request exceeds the hosted byte limit.");
            }
            return request;
        }
    }
}
