using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

public static class X64ˉnativeˉargumentˉservices
{
    public const int ARGUMENT_COUNT_CANONICAL_SIZE = 5;
    public const string ARGUMENT_COUNT_CANONICAL_SHA256 =
        "2358e7e2c72d6476cfe05134db4f0eb5e6987fcca1b10894a8588a28d3929829";
    public const int ARGUMENT_CANONICAL_SIZE = 70;
    public const string ARGUMENT_CANONICAL_SHA256 =
        "2253e1435f141df5b68f9f7e9e9aa0de448410c42dcf33ad76dcf131afea65d1";

    // ABI-12 supplies an execution-owned immutable descriptor table through R15's context.
    // These leaves preserve R10, R11, and R15 and have no platform-specific instructions.
    public static ImmutableArray<byte> Build(Nativeˉservice service) => service switch
    {
        Nativeˉservice.Processˉargumentˉcount =>
        [
            0x41, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.ARGUMENT_COUNT_OFFSET,
            0xC3,
        ],
        Nativeˉservice.Processˉargument =>
        [
            // Clear the service detail, then reject index >= count before loading the table.
            0x41, 0xC7, 0x47, Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            0x00, 0x00, 0x00, 0x00,
            0x45, 0x3B, 0x47, Nativeˉexecutionˉcontextˉcontract.ARGUMENT_COUNT_OFFSET,
            0x0F, 0x83, 0x26, 0x00, 0x00, 0x00,

            // Copy one verified 16-byte borrowed-text descriptor into R9's result cell.
            0x49, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.ARGUMENT_TABLE_POINTER_OFFSET,
            0x44, 0x89, 0xC1,
            0x48, 0xC1, 0xE1, 0x04,
            0x48, 0x01, 0xC8,
            0x48, 0x8B, 0x08,
            0x49, 0x89, 0x09,
            0x8B, 0x48, Nativeˉcontract.BORROWED_TEXT_LENGTH_OFFSET,
            0x41, 0x89, 0x49, Nativeˉcontract.BORROWED_TEXT_LENGTH_OFFSET,
            0x41, 0xC7, 0x41, Nativeˉcontract.BORROWED_TEXT_RESERVED_OFFSET,
            0x00, 0x00, 0x00, 0x00,
            0x31, 0xC0,
            0xC3,

            // Publish the exact range failure and return service status one.
            0x41, 0xC7, 0x47, Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            (byte)Nativeˉserviceˉfailureˉdetail.Argumentˉindexˉoutˉofˉrange,
            0x00, 0x00, 0x00,
            0xB8, 0x01, 0x00, 0x00, 0x00,
            0xC3,
        ],
        _ => throw new ArgumentOutOfRangeException(
            nameof(service),
            service,
            "The requested service is not an ABI-12 native argument leaf."),
    };

    public static void Verify(Nativeˉservice service, ReadOnlySpan<byte> code)
    {
        var (Expectedˉsize, Expectedˉhash) = service switch
        {
            Nativeˉservice.Processˉargumentˉcount =>
                (ARGUMENT_COUNT_CANONICAL_SIZE, ARGUMENT_COUNT_CANONICAL_SHA256),
            Nativeˉservice.Processˉargument =>
                (ARGUMENT_CANONICAL_SIZE, ARGUMENT_CANONICAL_SHA256),
            _ => throw new ArgumentOutOfRangeException(
                nameof(service),
                service,
                "The requested service is not an ABI-12 native argument leaf."),
        };
        var Hash = Convert.ToHexString(SHA256.HashData(code)).ToLowerInvariant();
        if (code.Length != Expectedˉsize ||
            !StringComparer.Ordinal.Equals(Hash, Expectedˉhash) ||
            !code.SequenceEqual(Build(service).AsSpan()))
        {
            throw new InvalidOperationException(
                $"Native {service} service identity is {code.Length} bytes / {Hash}; " +
                $"expected {Expectedˉsize} bytes / {Expectedˉhash}.");
        }
    }
}
