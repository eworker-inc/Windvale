using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

public static class Nativeˉdescriptorˉallocatorˉcontract
{
    public const uint FORMAT_VERSION = 1;
    public const uint STATE_MAGIC = 0x5341_5657;
    public const uint FREE_BLOCK_MAGIC = 0x5246_5657;
    public const uint ALLOCATED_BLOCK_MAGIC = 0x4C41_5657;

    public const int STATE_BYTES = 32;
    public const int STATE_ARENA_POINTER_OFFSET = 0;
    public const int STATE_ARENA_LENGTH_OFFSET = 8;
    public const int STATE_MAGIC_OFFSET = 12;
    public const int STATE_FREE_HEAD_OFFSET = 16;
    public const int STATE_ALLOCATED_BLOCKS_OFFSET = 20;
    public const int STATE_CHARGED_BYTES_OFFSET = 24;
    public const int STATE_RESERVED_OFFSET = 28;

    public const int REQUEST_BYTES = 40;
    public const int REQUEST_FORMAT_VERSION_OFFSET = 0;
    public const int REQUEST_SIZE_OFFSET = 4;
    public const int REQUEST_OPERATION_OFFSET = 8;
    public const int REQUEST_PAYLOAD_BYTES_OFFSET = 12;
    public const int REQUEST_OWNER_TOKEN_OFFSET = 16;
    public const int REQUEST_STATUS_OFFSET = 20;
    public const int REQUEST_DATA_POINTER_OFFSET = 24;
    public const int REQUEST_CHARGED_BYTES_OFFSET = 32;
    public const int REQUEST_RESERVED_OFFSET = 36;

    public const int BLOCK_HEADER_BYTES = 16;
    public const int BLOCK_CHARGED_BYTES_OFFSET = 0;
    public const int BLOCK_REFERENCE_COUNT_OFFSET = 4;
    public const int BLOCK_NEXT_FREE_OFFSET = 8;
    public const int BLOCK_MAGIC_OFFSET = 12;
    public const uint MAXIMUM_BLOCKS =
        Nativeˉcontract.MAXIMUM_TEXT_ARENA_BYTES / BLOCK_HEADER_BYTES;
}

public enum Nativeˉdescriptorˉallocatorˉoperation : uint
{
    Acquire = 1,
    Retain = 2,
    Release = 3,
}

public enum Nativeˉdescriptorˉallocatorˉstatus : uint
{
    Success = 0,
    Invalidˉrequest = 1,
    Exhausted = 2,
    Corruptˉstate = 3,
    Invalidˉowner = 4,
    Referenceˉoverflow = 5,
}

public sealed record Nativeˉdescriptorˉallocatorˉprojection(
    int Ownershipˉactions,
    int Acquireˉcalls,
    int Retainˉcalls,
    int Releaseˉcalls,
    int Allocatorˉleafˉcalls,
    int Ownershipˉmovementˉactions);

public static class Nativeˉdescriptorˉallocatorˉprojector
{
    public static Nativeˉdescriptorˉallocatorˉprojection Project(
        Nativeˉdescriptorˉownershipˉplan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (plan.Formatˉversion != Nativeˉdescriptorˉownershipˉplanner.FORMAT_VERSION ||
            !plan.Terminalˉfailureˉdiscardsˉarena ||
            plan.Functions.IsDefault ||
            plan.Totalˉactions < 0 ||
            plan.Totalˉactions != plan.Functions.Sum(Function => Function.Actions.Length))
        {
            throw new Nativeˉbackendˉexception(
                "WVN2904",
                "The descriptor allocator projector received an invalid ownership plan.");
        }

        var Acquire = 0;
        var Retain = 0;
        var Release = 0;
        foreach (var Action in plan.Functions.SelectMany(Function => Function.Actions))
        {
            switch (Action.Kind)
            {
                case Nativeˉdescriptorˉownershipˉactionˉkind.Acquire:
                    Acquire = checked(Acquire + 1);
                    break;
                case Nativeˉdescriptorˉownershipˉactionˉkind.Retain:
                    Retain = checked(Retain + 1);
                    break;
                case Nativeˉdescriptorˉownershipˉactionˉkind.Release:
                    Release = checked(Release + 1);
                    break;
                case Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉstatic:
                case Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉhost:
                case Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉcall:
                case Nativeˉdescriptorˉownershipˉactionˉkind.Acceptˉreturn:
                case Nativeˉdescriptorˉownershipˉactionˉkind.Transferˉreturn:
                    break;
                default:
                    throw new Nativeˉbackendˉexception(
                        "WVN2904",
                        "The descriptor allocator projector received an unknown ownership action.");
            }
        }
        var Calls = checked(Acquire + Retain + Release);
        return new(
            plan.Totalˉactions,
            Acquire,
            Retain,
            Release,
            Calls,
            checked(plan.Totalˉactions - Calls));
    }
}

public static class X64ˉnativeˉdescriptorˉallocator
{
    public const string SYMBOL = "Windvale_descriptor_allocator";
    public const int CANONICAL_CODE_BYTES = 2_989;
    public const string CANONICAL_OBJECT_SHA256 =
        "75d82e97fcc6652b0153ebed1b849569248ca4371c3c365605f32092a17f4cfb";
    public const string CANONICAL_CODE_SHA256 =
        "67a8b6648389589b59ca1dd6b6b87e80fafaa31696d496e14be9fcf4711ccf70";

    private const string RESOURCE =
        "Windvale.NativeCompiler.Descriptor-Allocator.wvo";

    public static ImmutableArray<byte> Code { get; } = Load();

    public static ImmutableArray<byte> Read(Verifiedˉobject value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var Object = value.Value;
        if (Object.Architecture != Objectˉarchitecture.X86ˉ64 ||
            Object.Sections.Length != 1 ||
            Object.Symbols.Length != 1 ||
            Object.Relocations.Length != 0)
        {
            throw Invalid();
        }

        var Section = Object.Sections[0];
        if (Section.Name != ".text" ||
            Section.Kind != Objectˉsectionˉkind.Code ||
            Section.Alignment != 16 ||
            Section.Memoryˉsize != CANONICAL_CODE_BYTES ||
            Section.Data.Length != CANONICAL_CODE_BYTES ||
            Object.Symbols[0] != new Objectˉsymbol(
                SYMBOL,
                Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Function,
                0,
                0,
                CANONICAL_CODE_BYTES))
        {
            throw Invalid();
        }

        var Hash = Convert.ToHexString(SHA256.HashData(Section.Data.AsSpan()))
            .ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, CANONICAL_CODE_SHA256))
        {
            throw new InvalidOperationException(
                $"The WVA descriptor allocator code is {Section.Data.Length} bytes / {Hash}; " +
                $"expected {CANONICAL_CODE_BYTES} bytes / {CANONICAL_CODE_SHA256}.");
        }
        return Section.Data;
    }

    private static ImmutableArray<byte> Load()
    {
        using var Stream = typeof(X64ˉnativeˉdescriptorˉallocator).Assembly
            .GetManifestResourceStream(RESOURCE) ?? throw Invalid();
        if (Stream.Length > Objectˉlimits.MAX_OBJECT_BYTES)
        {
            throw Invalid();
        }
        using var Buffer = new MemoryStream(checked((int)Stream.Length));
        Stream.CopyTo(Buffer);
        var Bytes = Buffer.GetBuffer().AsSpan(0, checked((int)Buffer.Length));
        var Objectˉhash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Objectˉhash, CANONICAL_OBJECT_SHA256))
        {
            throw Invalid();
        }
        return Read(Objectˉcodec.Readˉandˉverify(Bytes));
    }

    private static InvalidOperationException Invalid() => new(
        "The WVA descriptor allocator does not match its bounded machine contract.");
}
