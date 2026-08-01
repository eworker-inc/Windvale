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

    // ABI-14 retains the execution-owned immutable descriptor table through R15's context.
    // These leaves preserve R10, R11, and R15 and have no platform-specific instructions.
    public static ImmutableArray<byte> Build(Nativeˉservice service) => service switch
    {
        Nativeˉservice.Processˉargumentˉcount =>
            X64ˉnativeˉstencil.Buildˉprocessˉargumentˉcount(),
        Nativeˉservice.Processˉargument =>
            X64ˉnativeˉstencil.Buildˉprocessˉargument(),
        _ => throw new ArgumentOutOfRangeException(
            nameof(service),
            service,
            "The requested service is not an ABI-14 native argument leaf."),
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
                "The requested service is not an ABI-14 native argument leaf."),
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
