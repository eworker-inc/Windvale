using System.Buffers.Binary;
using System.Collections.Immutable;

namespace Windvale.Bootstrap;

public static class Kernelˉmemoryˉcontract
{
    public const int FORMAT_VERSION = 17;
    public const string TARGET_NAME = "x86-64-kernel-memory-v17";
    public const string MEMORY_ENTER_SYMBOL = "Windvale_kernel_memory_enter";
    public const string ALLOCATE_PAGES_SYMBOL = "Windvale_kernel_allocate_pages";
    public const string ALLOCATE_MEMORY_OBJECT_SYMBOL =
        "Windvale_kernel_allocate_memory_object";
    public const string RELEASE_MEMORY_OBJECT_SYMBOL =
        "Windvale_kernel_release_memory_object";
    public const uint EFI_CONVENTIONAL_MEMORY = 7;
    public const ulong PAGE_BYTES = 4_096;
    public const ulong ARENA_ALIGNMENT_BYTES = 2 * 1024 * 1024;
    public const ulong MINIMUM_PHYSICAL_ADDRESS = 1 * 1024 * 1024;
    public const ulong MAXIMUM_PHYSICAL_ADDRESS_EXCLUSIVE = 1UL << 32;
    public const ulong ARENA_PAGES = 157;
    public const ulong ARENA_BYTES = ARENA_PAGES * PAGE_BYTES;
    public const ulong STATE_PAGES = 1;
    public const ulong STACK_PAGES = 4;
    public const ulong FIRST_FREE_PAGE = STATE_PAGES + STACK_PAGES;
    public const ulong INITIAL_FREE_PAGES = ARENA_PAGES - FIRST_FREE_PAGE;
    public const ulong BOOTSTRAP_FIXED_PAGES = 8;
    public const ulong FIRST_MEMORY_OBJECT_PAGE = FIRST_FREE_PAGE + BOOTSTRAP_FIXED_PAGES;
    public const uint STATE_HEADER_BYTES = 64;
    public const uint HANDOFF_COPY_OFFSET = STATE_HEADER_BYTES;
    public const ulong STATE_MAGIC = 0x3731_4D45_4D4B_5657;
    public const uint STATE_VERSION = 17;
    public const uint ALLOCATION_BITMAP_OFFSET = 0xBA0;
    public const uint ALLOCATION_BITMAP_BYTES = 20;
    public const uint PAGE_OWNER_OFFSET = 0xBC0;
    public const uint PAGE_OWNER_BYTES = 157;
    public const uint MEMORY_OBJECT_RECORD_BYTES = 304;
    public const uint MEMORY_OBJECT_VECTOR_OFFSET = 56;
    public const uint MAXIMUM_MEMORY_OBJECT_PAGES = 122;
    public const uint INIT_MEMORY_OBJECT_OFFSET = 0xC60;
    public const uint CLIENT_MEMORY_OBJECT_OFFSET = 0xD90;
    public const uint DIRECTORY_MEMORY_OBJECT_OFFSET = 0xEC0;
    public const ulong MEMORY_OBJECT_MAGIC = 0x3130_4F4D_454D_5657;
    public const uint MEMORY_OBJECT_VERSION = 1;
    public const uint MEMORY_OBJECT_STATE_ACTIVE = 1;
    public const uint MEMORY_OBJECT_STATE_RELEASED = 2;
    public const byte PAGE_OWNER_FREE = 0;
    public const byte PAGE_OWNER_KERNEL = 255;
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
    private readonly bool[] Allocated;
    private readonly byte[] Owners;

    public Kernelˉpageˉallocator(Kernelˉmemoryˉplan plan, byte[] arena)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(arena);
        if (plan.Arenaˉpages != Kernelˉmemoryˉcontract.ARENA_PAGES ||
            plan.Firstˉfreeˉpage != Kernelˉmemoryˉcontract.FIRST_FREE_PAGE ||
            plan.Freeˉpages != Kernelˉmemoryˉcontract.INITIAL_FREE_PAGES ||
            arena.Length != checked((int)Kernelˉmemoryˉcontract.ARENA_BYTES))
        {
            throw new ArgumentException("The allocator requires one canonical kernel arena.", nameof(plan));
        }

        Plan = plan;
        Arena = arena;
        Array.Clear(Arena);
        Allocated = new bool[checked((int)plan.Arenaˉpages)];
        Owners = new byte[Allocated.Length];
        for (ulong Page = 0; Page < plan.Firstˉfreeˉpage; Page++)
        {
            Allocated[checked((int)Page)] = true;
            Owners[checked((int)Page)] = Kernelˉmemoryˉcontract.PAGE_OWNER_KERNEL;
        }
        Remainingˉpages = plan.Freeˉpages;
    }

    public ulong Remainingˉpages { get; private set; }

    public ulong? Allocateˉpages(ulong pages)
    {
        if (pages == 0 || pages > Remainingˉpages)
        {
            return null;
        }

        ulong Runˉstart = 0;
        ulong Runˉpages = 0;
        for (var Page = Plan.Firstˉfreeˉpage; Page < Plan.Arenaˉpages; Page++)
        {
            if (Allocated[checked((int)Page)])
            {
                Runˉpages = 0;
                continue;
            }

            if (Runˉpages == 0)
            {
                Runˉstart = Page;
            }
            Runˉpages++;
            if (Runˉpages == pages)
            {
                break;
            }
        }
        if (Runˉpages != pages)
        {
            return null;
        }

        var Byteˉoffset = checked(Runˉstart * Kernelˉmemoryˉcontract.PAGE_BYTES);
        var Byteˉlength = checked(pages * Kernelˉmemoryˉcontract.PAGE_BYTES);
        var Address = checked(Plan.Arenaˉaddress + Byteˉoffset);
        Array.Clear(Arena, checked((int)Byteˉoffset), checked((int)Byteˉlength));
        for (ulong Index = 0; Index < pages; Index++)
        {
            var Page = checked((int)(Runˉstart + Index));
            Allocated[Page] = true;
            Owners[Page] = Kernelˉmemoryˉcontract.PAGE_OWNER_KERNEL;
        }
        Remainingˉpages -= pages;
        return Address;
    }

    public Kernelˉmemoryˉobjectˉallocation? Allocateˉmemoryˉobject(
        uint reference,
        ulong pages,
        Kernelˉmemoryˉobject? prior = null)
    {
        var Objectˉid = reference & 0xFFFF;
        var Generation = reference >> 16;
        if (Objectˉid is 0 or > 254 ||
            Generation == 0 ||
            pages == 0 ||
            pages > Kernelˉmemoryˉcontract.MAXIMUM_MEMORY_OBJECT_PAGES ||
            (prior is not null &&
                (prior.State != Kernelˉmemoryˉcontract.MEMORY_OBJECT_STATE_RELEASED ||
                    prior.Objectˉid != Objectˉid ||
                    prior.Generation == uint.MaxValue ||
                    Generation != prior.Generation + 1 ||
                    prior.Allocationˉcount != prior.Releaseˉcount)))
        {
            return null;
        }

        var Address = Allocateˉpages(pages);
        if (Address is null)
        {
            return null;
        }

        var Firstˉpage = (Address.Value - Plan.Arenaˉaddress) /
            Kernelˉmemoryˉcontract.PAGE_BYTES;
        var Pageˉindices = ImmutableArray.CreateBuilder<ushort>(checked((int)pages));
        for (ulong Index = 0; Index < pages; Index++)
        {
            var Page = checked(Firstˉpage + Index);
            Owners[checked((int)Page)] = checked((byte)Objectˉid);
            Pageˉindices.Add(checked((ushort)Page));
        }

        var Allocationˉcount = checked((prior?.Allocationˉcount ?? 0) + 1);
        var Releaseˉcount = prior?.Releaseˉcount ?? 0;
        var Object = new Kernelˉmemoryˉobject(
            Kernelˉmemoryˉcontract.MEMORY_OBJECT_STATE_ACTIVE,
            reference,
            reference,
            checked((uint)Objectˉid),
            Generation,
            checked((uint)pages),
            Allocationˉcount,
            Releaseˉcount,
            Address.Value,
            Pageˉindices.ToImmutable());
        return new(Address.Value, Object);
    }

    public Kernelˉmemoryˉobject? Releaseˉmemoryˉobject(
        Kernelˉmemoryˉobject value,
        uint reference)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.State != Kernelˉmemoryˉcontract.MEMORY_OBJECT_STATE_ACTIVE ||
            value.Reference != reference ||
            value.Ownerˉreference != reference ||
            value.Objectˉid != (reference & 0xFFFF) ||
            value.Generation != (reference >> 16) ||
            value.Pageˉcount == 0 ||
            value.Pageˉcount != value.Pageˉindices.Length ||
            value.Pageˉcount > Kernelˉmemoryˉcontract.MAXIMUM_MEMORY_OBJECT_PAGES ||
            value.Baseˉaddress != Plan.Arenaˉaddress +
                checked((ulong)value.Pageˉindices[0] * Kernelˉmemoryˉcontract.PAGE_BYTES))
        {
            return null;
        }

        ushort Priorˉpage = 0;
        for (var Index = 0; Index < value.Pageˉindices.Length; Index++)
        {
            var Page = value.Pageˉindices[Index];
            if (Page < Plan.Firstˉfreeˉpage ||
                Page >= Plan.Arenaˉpages ||
                (Index != 0 && Page != Priorˉpage + 1) ||
                !Allocated[Page] ||
                Owners[Page] != checked((byte)value.Objectˉid))
            {
                return null;
            }
            Priorˉpage = Page;
        }
        if (value.Pageˉcount > Plan.Freeˉpages - Remainingˉpages)
        {
            return null;
        }

        foreach (var Page in value.Pageˉindices)
        {
            var Byteˉoffset = checked((int)((ulong)Page * Kernelˉmemoryˉcontract.PAGE_BYTES));
            Array.Clear(Arena, Byteˉoffset, checked((int)Kernelˉmemoryˉcontract.PAGE_BYTES));
            Allocated[Page] = false;
            Owners[Page] = Kernelˉmemoryˉcontract.PAGE_OWNER_FREE;
        }
        Remainingˉpages += value.Pageˉcount;
        return value with
        {
            State = Kernelˉmemoryˉcontract.MEMORY_OBJECT_STATE_RELEASED,
            Pageˉcount = 0,
            Releaseˉcount = checked(value.Releaseˉcount + 1),
            Baseˉaddress = 0,
            Pageˉindices = [],
        };
    }

    public ImmutableArray<byte> Allocationˉbitmap()
    {
        var Result = new byte[Kernelˉmemoryˉcontract.ALLOCATION_BITMAP_BYTES];
        for (var Page = 0; Page < Allocated.Length; Page++)
        {
            if (Allocated[Page])
            {
                Result[Page >> 3] |= checked((byte)(1 << (Page & 7)));
            }
        }
        for (var Page = Allocated.Length; Page < Result.Length * 8; Page++)
        {
            Result[Page >> 3] |= checked((byte)(1 << (Page & 7)));
        }
        return Result.ToImmutableArray();
    }
}

public sealed record Kernelˉmemoryˉobject(
    uint State,
    uint Reference,
    uint Ownerˉreference,
    uint Objectˉid,
    uint Generation,
    uint Pageˉcount,
    uint Allocationˉcount,
    uint Releaseˉcount,
    ulong Baseˉaddress,
    ImmutableArray<ushort> Pageˉindices);

public sealed record Kernelˉmemoryˉobjectˉallocation(
    ulong Address,
    Kernelˉmemoryˉobject Object);

public static class Kernelˉmemoryˉobjectˉcodec
{
    public static byte[] Write(Kernelˉmemoryˉobject value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Validate(value);
        var Result = new byte[Kernelˉmemoryˉcontract.MEMORY_OBJECT_RECORD_BYTES];
        BinaryPrimitives.WriteUInt64LittleEndian(Result, Kernelˉmemoryˉcontract.MEMORY_OBJECT_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8),
            Kernelˉmemoryˉcontract.MEMORY_OBJECT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12),
            Kernelˉmemoryˉcontract.MEMORY_OBJECT_RECORD_BYTES);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), value.State);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(20), value.Reference);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(24), value.Ownerˉreference);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(28), value.Pageˉcount);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(32), value.Allocationˉcount);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(36), value.Releaseˉcount);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(40),
            checked((uint)value.Pageˉindices.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(44), value.Objectˉid);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(48), value.Baseˉaddress);
        for (var Index = 0; Index < value.Pageˉindices.Length; Index++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(
                Result.AsSpan(checked((int)Kernelˉmemoryˉcontract.MEMORY_OBJECT_VECTOR_OFFSET +
                    (Index * sizeof(ushort)))),
                value.Pageˉindices[Index]);
        }
        return Result;
    }

    public static Kernelˉmemoryˉobject Read(ReadOnlySpan<byte> source)
    {
        if (source.Length != Kernelˉmemoryˉcontract.MEMORY_OBJECT_RECORD_BYTES ||
            BinaryPrimitives.ReadUInt64LittleEndian(source) !=
                Kernelˉmemoryˉcontract.MEMORY_OBJECT_MAGIC ||
            BinaryPrimitives.ReadUInt32LittleEndian(source[8..]) !=
                Kernelˉmemoryˉcontract.MEMORY_OBJECT_VERSION ||
            BinaryPrimitives.ReadUInt32LittleEndian(source[12..]) !=
                Kernelˉmemoryˉcontract.MEMORY_OBJECT_RECORD_BYTES)
        {
            throw new InvalidDataException("The memory-object envelope is malformed.");
        }

        var Vectorˉcount = BinaryPrimitives.ReadUInt32LittleEndian(source[40..]);
        if (Vectorˉcount > Kernelˉmemoryˉcontract.MAXIMUM_MEMORY_OBJECT_PAGES)
        {
            throw new InvalidDataException("The memory-object page vector is oversized.");
        }
        var Pageˉindices = ImmutableArray.CreateBuilder<ushort>(checked((int)Vectorˉcount));
        for (var Index = 0; Index < Vectorˉcount; Index++)
        {
            Pageˉindices.Add(BinaryPrimitives.ReadUInt16LittleEndian(source[
                checked((int)Kernelˉmemoryˉcontract.MEMORY_OBJECT_VECTOR_OFFSET +
                    (checked((int)Index) * sizeof(ushort)))..]));
        }
        var Usedˉbytes = checked((int)Kernelˉmemoryˉcontract.MEMORY_OBJECT_VECTOR_OFFSET +
            (checked((int)Vectorˉcount) * sizeof(ushort)));
        if (!source[Usedˉbytes..].IsEmpty && source[Usedˉbytes..].IndexOfAnyExcept((byte)0) >= 0)
        {
            throw new InvalidDataException("The memory-object unused page vector is not zero.");
        }

        var Reference = BinaryPrimitives.ReadUInt32LittleEndian(source[20..]);
        var Value = new Kernelˉmemoryˉobject(
            BinaryPrimitives.ReadUInt32LittleEndian(source[16..]),
            Reference,
            BinaryPrimitives.ReadUInt32LittleEndian(source[24..]),
            BinaryPrimitives.ReadUInt32LittleEndian(source[44..]),
            Reference >> 16,
            BinaryPrimitives.ReadUInt32LittleEndian(source[28..]),
            BinaryPrimitives.ReadUInt32LittleEndian(source[32..]),
            BinaryPrimitives.ReadUInt32LittleEndian(source[36..]),
            BinaryPrimitives.ReadUInt64LittleEndian(source[48..]),
            Pageˉindices.ToImmutable());
        Validate(Value);
        return Value;
    }

    private static void Validate(Kernelˉmemoryˉobject value)
    {
        if (value.Reference == 0 ||
            value.Ownerˉreference != value.Reference ||
            value.Objectˉid is 0 or > 254 ||
            value.Objectˉid != (value.Reference & 0xFFFF) ||
            value.Generation == 0 ||
            value.Generation != (value.Reference >> 16) ||
            value.Allocationˉcount == 0 ||
            value.Releaseˉcount > value.Allocationˉcount)
        {
            throw new InvalidDataException("The memory-object identity or history is malformed.");
        }

        if (value.State == Kernelˉmemoryˉcontract.MEMORY_OBJECT_STATE_ACTIVE)
        {
            if (value.Pageˉcount == 0 ||
                value.Pageˉcount != value.Pageˉindices.Length ||
                value.Pageˉcount > Kernelˉmemoryˉcontract.MAXIMUM_MEMORY_OBJECT_PAGES ||
                value.Releaseˉcount + 1 != value.Allocationˉcount ||
                value.Baseˉaddress == 0 ||
                (value.Baseˉaddress & (Kernelˉmemoryˉcontract.PAGE_BYTES - 1)) != 0 ||
                (value.Baseˉaddress &
                    (Kernelˉmemoryˉcontract.ARENA_ALIGNMENT_BYTES - 1)) !=
                    checked((ulong)value.Pageˉindices[0] *
                        Kernelˉmemoryˉcontract.PAGE_BYTES))
            {
                throw new InvalidDataException("The active memory-object state is malformed.");
            }

            ushort Prior = 0;
            for (var Index = 0; Index < value.Pageˉindices.Length; Index++)
            {
                var Page = value.Pageˉindices[Index];
                if (Page < Kernelˉmemoryˉcontract.FIRST_FREE_PAGE ||
                    Page >= Kernelˉmemoryˉcontract.ARENA_PAGES ||
                    (Index != 0 && Page != Prior + 1))
                {
                    throw new InvalidDataException("The active memory-object page vector is malformed.");
                }
                Prior = Page;
            }
        }
        else if (value.State == Kernelˉmemoryˉcontract.MEMORY_OBJECT_STATE_RELEASED)
        {
            if (value.Pageˉcount != 0 ||
                !value.Pageˉindices.IsEmpty ||
                value.Allocationˉcount != value.Releaseˉcount ||
                value.Baseˉaddress != 0)
            {
                throw new InvalidDataException("The released memory-object state is malformed.");
            }
        }
        else
        {
            throw new InvalidDataException("The memory-object lifecycle state is unknown.");
        }
    }
}
