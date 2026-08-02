using System.Buffers.Binary;
using System.Collections.Immutable;

namespace Windvale.Bootstrap;

public static class Kernelˉmemoryˉcontract
{
    public const int FORMAT_VERSION = 6;
    public const string TARGET_NAME = "x86-64-kernel-memory-v6";
    public const string MEMORY_ENTER_SYMBOL = "Windvale_kernel_memory_enter";
    public const string ALLOCATE_PAGES_SYMBOL = "Windvale_kernel_allocate_pages";
    public const uint EFI_CONVENTIONAL_MEMORY = 7;
    public const ulong PAGE_BYTES = 4_096;
    public const ulong ARENA_ALIGNMENT_BYTES = 2 * 1024 * 1024;
    public const ulong MINIMUM_PHYSICAL_ADDRESS = 1 * 1024 * 1024;
    public const ulong MAXIMUM_PHYSICAL_ADDRESS_EXCLUSIVE = 1UL << 32;
    public const ulong ARENA_PAGES = 63;
    public const ulong ARENA_BYTES = ARENA_PAGES * PAGE_BYTES;
    public const ulong STATE_PAGES = 1;
    public const ulong STACK_PAGES = 4;
    public const ulong FIRST_FREE_PAGE = STATE_PAGES + STACK_PAGES;
    public const ulong INITIAL_FREE_PAGES = ARENA_PAGES - FIRST_FREE_PAGE;
    public const uint STATE_HEADER_BYTES = 64;
    public const uint HANDOFF_COPY_OFFSET = STATE_HEADER_BYTES;
    public const ulong STATE_MAGIC = 0x3630_4D45_4D4B_5657;
    public const uint STATE_VERSION = 6;
}

public sealed record Kernelˉmemoryˉdiagnostic(string Code, string Message);

public sealed record Kernelˉmemoryˉplan(
    ulong Arenaˉaddress,
    ulong Arenaˉpages,
    ulong Stateˉaddress,
    ulong Handoffˉcopyˉaddress,
    ulong Stackˉaddress,
    ulong Stackˉbytes,
    ulong Stackˉtop,
    ulong Firstˉfreeˉpage,
    ulong Freeˉpages);

public sealed record Kernelˉmemoryˉplanˉresult(
    Kernelˉmemoryˉplan? Plan,
    ImmutableArray<Kernelˉmemoryˉdiagnostic> Diagnostics)
{
    public bool Success => Plan is not null && Diagnostics.IsEmpty;
}

public static class Kernelˉmemoryˉplanner
{
    public const int MAXIMUM_MAP_BYTES = 1024 * 1024;
    public const ulong MINIMUM_DESCRIPTOR_BYTES = 40;
    public const ulong MAXIMUM_DESCRIPTOR_BYTES = 256;

    public static Kernelˉmemoryˉplanˉresult Plan(
        ReadOnlySpan<byte> memoryˉmap,
        ulong descriptorˉbytes)
    {
        if (memoryˉmap.IsEmpty ||
            memoryˉmap.Length > MAXIMUM_MAP_BYTES ||
            descriptorˉbytes is < MINIMUM_DESCRIPTOR_BYTES or > MAXIMUM_DESCRIPTOR_BYTES ||
            (ulong)memoryˉmap.Length % descriptorˉbytes != 0)
        {
            return Fail("WVOS4001", "The memory-map envelope is empty, oversized, truncated, or has an invalid descriptor stride.");
        }

        var Descriptorˉcount = (ulong)memoryˉmap.Length / descriptorˉbytes;
        ulong? Arenaˉaddress = null;
        ulong Arenaˉdescriptor = 0;
        for (ulong Index = 0; Index < Descriptorˉcount; Index++)
        {
            var Descriptor = memoryˉmap.Slice(
                checked((int)(Index * descriptorˉbytes)),
                checked((int)descriptorˉbytes));
            var Physicalˉaddress = BinaryPrimitives.ReadUInt64LittleEndian(Descriptor[8..]);
            var Virtualˉaddress = BinaryPrimitives.ReadUInt64LittleEndian(Descriptor[16..]);
            var Pages = BinaryPrimitives.ReadUInt64LittleEndian(Descriptor[24..]);
            if ((Physicalˉaddress & (Kernelˉmemoryˉcontract.PAGE_BYTES - 1)) != 0 ||
                (Virtualˉaddress & (Kernelˉmemoryˉcontract.PAGE_BYTES - 1)) != 0)
            {
                return Fail("WVOS4002", $"Memory descriptor {Index} contains an unaligned physical or virtual address.");
            }
            if (Pages == 0)
            {
                return Fail("WVOS4003", $"Memory descriptor {Index} contains zero pages.");
            }
            if (Pages - 1 > (ulong.MaxValue - Physicalˉaddress) / Kernelˉmemoryˉcontract.PAGE_BYTES)
            {
                return Fail("WVOS4004", $"Memory descriptor {Index} exceeds the physical address range.");
            }

            var Type = BinaryPrimitives.ReadUInt32LittleEndian(Descriptor);
            var Alignmentˉmask = Kernelˉmemoryˉcontract.ARENA_ALIGNMENT_BYTES - 1;
            var Candidateˉaddress = Physicalˉaddress > ulong.MaxValue - Alignmentˉmask
                ? ulong.MaxValue
                : (Physicalˉaddress + Alignmentˉmask) & ~Alignmentˉmask;
            var Lastˉpageˉaddress =
                Physicalˉaddress + ((Pages - 1) * Kernelˉmemoryˉcontract.PAGE_BYTES);
            if (Type != Kernelˉmemoryˉcontract.EFI_CONVENTIONAL_MEMORY ||
                Physicalˉaddress < Kernelˉmemoryˉcontract.MINIMUM_PHYSICAL_ADDRESS ||
                Pages < Kernelˉmemoryˉcontract.ARENA_PAGES ||
                Candidateˉaddress >
                    Kernelˉmemoryˉcontract.MAXIMUM_PHYSICAL_ADDRESS_EXCLUSIVE -
                    Kernelˉmemoryˉcontract.ARENA_BYTES ||
                Candidateˉaddress > Lastˉpageˉaddress ||
                Kernelˉmemoryˉcontract.ARENA_BYTES - Kernelˉmemoryˉcontract.PAGE_BYTES >
                    Lastˉpageˉaddress - Candidateˉaddress)
            {
                continue;
            }

            if (Arenaˉaddress is null || Candidateˉaddress < Arenaˉaddress.Value)
            {
                Arenaˉaddress = Candidateˉaddress;
                Arenaˉdescriptor = Index;
            }
        }

        if (Arenaˉaddress is null)
        {
            return Fail(
                "WVOS4005",
                $"The map contains no eligible 2 MiB-aligned " +
                $"{Kernelˉmemoryˉcontract.ARENA_BYTES / 1024} KiB conventional-memory arena below 4 GiB.");
        }

        var Arenaˉend = Arenaˉaddress.Value + Kernelˉmemoryˉcontract.ARENA_BYTES;
        for (ulong Index = 0; Index < Descriptorˉcount; Index++)
        {
            if (Index == Arenaˉdescriptor)
            {
                continue;
            }

            var Descriptor = memoryˉmap.Slice(
                checked((int)(Index * descriptorˉbytes)),
                checked((int)descriptorˉbytes));
            var Physicalˉaddress = BinaryPrimitives.ReadUInt64LittleEndian(Descriptor[8..]);
            var Pages = BinaryPrimitives.ReadUInt64LittleEndian(Descriptor[24..]);
            var Lastˉpageˉaddress = Physicalˉaddress + ((Pages - 1) * Kernelˉmemoryˉcontract.PAGE_BYTES);
            if (Physicalˉaddress < Arenaˉend && Lastˉpageˉaddress >= Arenaˉaddress.Value)
            {
                return Fail("WVOS4006", $"Memory descriptor {Index} overlaps the selected kernel arena.");
            }
        }

        var Stateˉaddress = Arenaˉaddress.Value;
        var Stackˉaddress = Stateˉaddress + Kernelˉmemoryˉcontract.PAGE_BYTES;
        var Stackˉbytes = Kernelˉmemoryˉcontract.STACK_PAGES * Kernelˉmemoryˉcontract.PAGE_BYTES;
        return new(
            new(
                Arenaˉaddress.Value,
                Kernelˉmemoryˉcontract.ARENA_PAGES,
                Stateˉaddress,
                Stateˉaddress + Kernelˉmemoryˉcontract.HANDOFF_COPY_OFFSET,
                Stackˉaddress,
                Stackˉbytes,
                Stackˉaddress + Stackˉbytes,
                Kernelˉmemoryˉcontract.FIRST_FREE_PAGE,
                Kernelˉmemoryˉcontract.INITIAL_FREE_PAGES),
            []);
    }

    private static Kernelˉmemoryˉplanˉresult Fail(string code, string message) =>
        new(null, [new(code, message)]);
}

public sealed class Kernelˉpageˉallocator
{
    private readonly Kernelˉmemoryˉplan Plan;
    private readonly byte[] Arena;
    private ulong Nextˉpage;

    public Kernelˉpageˉallocator(Kernelˉmemoryˉplan plan, byte[] arena)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(arena);
        if (plan.Arenaˉpages != Kernelˉmemoryˉcontract.ARENA_PAGES ||
            plan.Firstˉfreeˉpage != Kernelˉmemoryˉcontract.FIRST_FREE_PAGE ||
            plan.Freeˉpages != Kernelˉmemoryˉcontract.INITIAL_FREE_PAGES ||
            arena.Length != checked((int)Kernelˉmemoryˉcontract.ARENA_BYTES))
        {
            throw new ArgumentException("The allocator requires one canonical version 2 kernel arena.", nameof(plan));
        }

        Plan = plan;
        Arena = arena;
        Array.Clear(Arena);
        Nextˉpage = plan.Firstˉfreeˉpage;
        Remainingˉpages = plan.Freeˉpages;
    }

    public ulong Remainingˉpages { get; private set; }

    public ulong? Allocateˉpages(ulong pages)
    {
        if (pages == 0 || pages > Remainingˉpages)
        {
            return null;
        }

        var Byteˉoffset = checked(Nextˉpage * Kernelˉmemoryˉcontract.PAGE_BYTES);
        var Byteˉlength = checked(pages * Kernelˉmemoryˉcontract.PAGE_BYTES);
        var Address = checked(Plan.Arenaˉaddress + Byteˉoffset);
        Array.Clear(Arena, checked((int)Byteˉoffset), checked((int)Byteˉlength));
        Nextˉpage += pages;
        Remainingˉpages -= pages;
        return Address;
    }
}
