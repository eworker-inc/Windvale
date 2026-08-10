using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal readonly record struct Nativeˉhostedˉstartupˉinputs(
    uint Startupˉaddress,
    uint Codeˉbytes,
    uint Symbolˉcount,
    ImmutableArray<uint> Targets,
    ImmutableArray<byte> Object);

internal static class Nativeˉhostedˉstartupˉinstantiator
{
    // Transitional managed transport; native outer-container orchestration owns its removal.
    internal const int CONSUMER_CANONICAL_SIZE = 21_143;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "933864be78b28394b9fc8e495b5ac872311ebca2a624db6e6731cdb8b399d309";
    internal const int CONSUMER_ARTIFACT_CANONICAL_SIZE = 193_891;
    internal const string CONSUMER_ARTIFACT_CANONICAL_SHA256 =
        "ad1c049bdf77cb410b95cb638aa401874cca1a21b496e36ecab32ceef1539ffd";

    private const uint REQUEST_MAGIC = 0x4953_5657;
    private const uint RESPONSE_MAGIC = 0x4453_5657;
    private const uint FORMAT_VERSION = 1;
    private const int REQUEST_HEADER_BYTES = 40;
    private const int RESPONSE_HEADER_BYTES = 32;
    private const int MAXIMUM_OBJECT_BYTES = 65_536;
    private const int MAXIMUM_RELOCATIONS = 256;
    private const long MAXIMUM_INSTRUCTIONS = 20_000_000;
    private const string CONSUMER_RESOURCE =
        "Windvale.Linker.Native-Hosted-Startup-Instantiation.wvnf";
    private static readonly Lazy<Nativeˉfragment> CONSUMER = new(
        Readˉconsumer,
        LazyThreadSafetyMode.ExecutionAndPublication);

    internal static ImmutableArray<byte> Build(Nativeˉhostedˉstartupˉinputs inputs)
    {
        var Request = Buildˉrequest(inputs);
        var Response = Buildˉwithˉwindvale(Request);
        return Verifyˉresponse(inputs, Request.Length, Response);
    }

    internal static ImmutableArray<byte> Buildˉrequest(
        Nativeˉhostedˉstartupˉinputs inputs)
    {
        Verifyˉinputs(inputs);
        var Result = new byte[checked(
            REQUEST_HEADER_BYTES + inputs.Targets.Length * sizeof(uint) + inputs.Object.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), checked((uint)Result.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), inputs.Startupˉaddress);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), inputs.Codeˉbytes);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(20), inputs.Symbolˉcount);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(24), checked((uint)inputs.Targets.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(28), checked((uint)inputs.Targets.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(32), checked((uint)inputs.Object.Length));
        for (var Index = 0; Index < inputs.Targets.Length; Index++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(REQUEST_HEADER_BYTES + Index * sizeof(uint)),
                inputs.Targets[Index]);
        }
        inputs.Object.AsSpan().CopyTo(
            Result.AsSpan(REQUEST_HEADER_BYTES + inputs.Targets.Length * sizeof(uint)));
        return Result.ToImmutableArray();
    }

    internal static ImmutableArray<byte> Buildˉwithˉwindvale(
        ImmutableArray<byte> request) =>
        X64ˉnativeˉexecutor.Executeˉserviceˉfreeˉbootstrapˉbytes(
            CONSUMER.Value,
            request,
            MAXIMUM_INSTRUCTIONS);

    internal static ImmutableArray<byte> Verifyˉresponse(
        Nativeˉhostedˉstartupˉinputs inputs,
        int requestˉbytes,
        ImmutableArray<byte> response)
    {
        Verifyˉinputs(inputs);
        if (response.IsDefault || response.Length < RESPONSE_HEADER_BYTES)
        {
            throw Invalidˉresponse();
        }
        var Span = response.AsSpan();
        if (BinaryPrimitives.ReadUInt32LittleEndian(Span) != RESPONSE_MAGIC ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[4..]) != FORMAT_VERSION ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[8..]) != response.Length ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[24..]) != 0 ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[28..]) != 0)
        {
            throw Invalidˉresponse();
        }
        var Status = BinaryPrimitives.ReadUInt32LittleEndian(Span[12..]);
        var Failureˉoffset = BinaryPrimitives.ReadUInt32LittleEndian(Span[16..]);
        if (Status != 0)
        {
            throw new InvalidOperationException(
                $"Windvale rejected the hosted-startup request with status " +
                    $"{Status} at offset {Failureˉoffset}.");
        }
        if (Failureˉoffset != requestˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[20..]) != inputs.Codeˉbytes ||
            response.Length != RESPONSE_HEADER_BYTES + inputs.Codeˉbytes)
        {
            throw Invalidˉresponse();
        }
        return Span[RESPONSE_HEADER_BYTES..].ToArray().ToImmutableArray();
    }

    internal static ImmutableArray<byte> Readˉobject(
        Type owner,
        string resource,
        int expectedˉbytes,
        string expectedˉsha256)
    {
        using var Stream = owner.Assembly.GetManifestResourceStream(resource) ??
            throw Invalidˉobject();
        if (Stream.Length != expectedˉbytes)
        {
            throw Invalidˉobject();
        }
        var Bytes = new byte[expectedˉbytes];
        Stream.ReadExactly(Bytes);
        var Hash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, expectedˉsha256))
        {
            throw Invalidˉobject();
        }
        return Bytes.ToImmutableArray();
    }

    internal static void Verifyˉinputs(Nativeˉhostedˉstartupˉinputs inputs)
    {
        if (inputs.Startupˉaddress != 4096 ||
            inputs.Codeˉbytes is < 4 or > 4096 ||
            inputs.Symbolˉcount is < 1 or > 128 ||
            inputs.Targets.IsDefaultOrEmpty ||
            inputs.Targets.Length > MAXIMUM_RELOCATIONS ||
            inputs.Targets.Any(Target => Target == 0) ||
            inputs.Object.IsDefaultOrEmpty ||
            inputs.Object.Length > MAXIMUM_OBJECT_BYTES)
        {
            throw new ArgumentException("The hosted-startup instantiation inputs are invalid.");
        }
    }

    private static Nativeˉfragment Readˉconsumer()
    {
        using var Stream = typeof(Nativeˉhostedˉstartupˉinstantiator).Assembly
            .GetManifestResourceStream(CONSUMER_RESOURCE) ??
            throw Invalidˉconsumer();
        if (Stream.Length != CONSUMER_ARTIFACT_CANONICAL_SIZE)
        {
            throw Invalidˉconsumer();
        }
        var Bytes = new byte[CONSUMER_ARTIFACT_CANONICAL_SIZE];
        Stream.ReadExactly(Bytes);
        var Hash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, CONSUMER_ARTIFACT_CANONICAL_SHA256))
        {
            throw Invalidˉconsumer();
        }
        var Fragment = Nativeˉfragmentˉartifactˉcodec.Readˉandˉverify(Bytes);
        if (!Fragment.Requiredˉservices.IsEmpty ||
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Fragment) != new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor))
        {
            throw Invalidˉconsumer();
        }
        return Fragment;
    }

    private static InvalidOperationException Invalidˉresponse() =>
        new("The Windvale hosted-startup response is invalid.");

    private static InvalidOperationException Invalidˉobject() =>
        new("The retained hosted-startup object failed its exact identity contract.");

    private static InvalidOperationException Invalidˉconsumer() =>
        new("The retained Windvale hosted-startup instantiator failed its exact identity contract.");
}
