using System.Buffers.Binary;
using System.Collections.Immutable;

namespace Windvale.Bootstrap;

public static class Kernelˉpagingˉcontract
{
    public const int FORMAT_VERSION = 5;
    public const string TARGET_NAME = "x86-64-kernel-paging-v5";
    public const string INSTALL_SYMBOL = "Windvale_kernel_x64_paging_install";
    public const string PROTECTION_ENABLE_SYMBOL = "Windvale_kernel_x64_page_protection_enable";
    public const string PAGE_TABLE_ACTIVATE_SYMBOL = "Windvale_kernel_x64_page_table_activate";
    public const ulong PAGE_BYTES = 4_096;
    public const ulong LARGE_PAGE_BYTES = 2 * 1024 * 1024;
    public const ulong IDENTITY_BYTES = 1UL << 30;
    public const ulong EXECUTABLE_BYTES = 768 * 1024;
    public const ulong TABLE_PAGES = 7;
    public const ulong TABLE_BYTES = TABLE_PAGES * PAGE_BYTES;
    public const ulong PML4_PAGE = 0;
    public const ulong PDPT_PAGE = 1;
    public const ulong PD_PAGE = 2;
    public const ulong NULL_PT_PAGE = 3;
    public const ulong CODE_PT0_PAGE = 4;
    public const ulong CODE_PT1_PAGE = 5;
    public const ulong MMIO_PD_PAGE = 6;
    public const ulong HPET_PAGE_ADDRESS = 0xFED0_0000;
    public const ulong LOCAL_APIC_PAGE_ADDRESS = 0xFEE0_0000;
    public const ulong MMIO_HPET_LARGE_PAGE_ADDRESS = 0xFEC0_0000;
    public const ulong MMIO_LOCAL_APIC_LARGE_PAGE_ADDRESS = 0xFEE0_0000;
    public const int MMIO_PDPT_INDEX = 3;
    public const int MMIO_HPET_PD_INDEX = 502;
    public const int MMIO_LOCAL_APIC_PD_INDEX = 503;
    public const ulong ENTRY_PRESENT = 1UL << 0;
    public const ulong ENTRY_WRITABLE = 1UL << 1;
    public const ulong ENTRY_ACCESSED = 1UL << 5;
    public const ulong ENTRY_LARGE_PAGE = 1UL << 7;
    public const ulong ENTRY_WRITE_THROUGH = 1UL << 3;
    public const ulong ENTRY_CACHE_DISABLE = 1UL << 4;
    public const ulong ENTRY_NO_EXECUTE = 1UL << 63;
    public const ulong ENTRY_ADDRESS_MASK = 0x000F_FFFF_FFFF_F000;
    public const uint RECORD_OFFSET = 128;
    public const uint RECORD_BYTES = 64;
    public const ulong RECORD_MAGIC = 0x3530_4741_504B_5657;
    public const uint RECORD_VERSION = 5;
    public const ulong RECORD_FLAG_NX = 1UL << 0;
    public const ulong RECORD_FLAG_WRITE_PROTECT = 1UL << 1;
    public const ulong RECORD_FLAG_NULL_GUARD = 1UL << 2;
    public const ulong RECORD_FLAG_TIMER_MMIO = 1UL << 3;
    public const ulong RECORD_FLAGS =
        RECORD_FLAG_NX | RECORD_FLAG_WRITE_PROTECT | RECORD_FLAG_NULL_GUARD |
        RECORD_FLAG_TIMER_MMIO;
}

public sealed record Kernelˉpagingˉdiagnostic(string Code, string Message);

public sealed record Kernelˉpagingˉplan(
    ulong Rootˉaddress,
    ulong Executableˉaddress,
    ImmutableArray<byte> Tableˉbytes,
    ImmutableArray<byte> Ownershipˉrecord);

public sealed record Kernelˉpagingˉplanˉresult(
    Kernelˉpagingˉplan? Plan,
    ImmutableArray<Kernelˉpagingˉdiagnostic> Diagnostics)
{
    public bool Success => Plan is not null && Diagnostics.IsEmpty;
}

public static class Kernelˉpagingˉplanner
{
    private const int ENTRIES_PER_TABLE = 512;

    public static Kernelˉpagingˉplanˉresult Plan(
        ulong rootˉaddress,
        ulong executableˉaddress)
    {
        if (rootˉaddress == 0 ||
            (rootˉaddress & (Kernelˉpagingˉcontract.PAGE_BYTES - 1)) != 0 ||
            rootˉaddress > Kernelˉpagingˉcontract.IDENTITY_BYTES - Kernelˉpagingˉcontract.TABLE_BYTES)
        {
            return Fail("WVOS5001", "The page-table allocation must be a complete aligned seven-page range below 1 GiB.");
        }

        if ((executableˉaddress & (Kernelˉpagingˉcontract.PAGE_BYTES - 1)) != 0 ||
            executableˉaddress < Kernelˉpagingˉcontract.LARGE_PAGE_BYTES ||
            executableˉaddress >= Kernelˉpagingˉcontract.IDENTITY_BYTES - Kernelˉpagingˉcontract.LARGE_PAGE_BYTES)
        {
            return Fail("WVOS5002", "The executable window must be aligned and leave two complete 2 MiB page-table regions within [2 MiB, 1 GiB).");
        }

        var Executableˉend = executableˉaddress + Kernelˉpagingˉcontract.EXECUTABLE_BYTES;
        var Tableˉend = rootˉaddress + Kernelˉpagingˉcontract.TABLE_BYTES;
        if (rootˉaddress < Executableˉend && Tableˉend > executableˉaddress)
        {
            return Fail("WVOS5003", "The page-table allocation overlaps the executable window.");
        }

        var Tables = new byte[checked((int)Kernelˉpagingˉcontract.TABLE_BYTES)];
        Writeˉentry(Tables, Kernelˉpagingˉcontract.PML4_PAGE, 0,
            Tableˉaddress(rootˉaddress, Kernelˉpagingˉcontract.PDPT_PAGE) |
            Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉpagingˉcontract.ENTRY_WRITABLE);
        Writeˉentry(Tables, Kernelˉpagingˉcontract.PDPT_PAGE, 0,
            Tableˉaddress(rootˉaddress, Kernelˉpagingˉcontract.PD_PAGE) |
            Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉpagingˉcontract.ENTRY_WRITABLE);
        Writeˉentry(Tables, Kernelˉpagingˉcontract.PDPT_PAGE,
            Kernelˉpagingˉcontract.MMIO_PDPT_INDEX,
            Tableˉaddress(rootˉaddress, Kernelˉpagingˉcontract.MMIO_PD_PAGE) |
            Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉpagingˉcontract.ENTRY_WRITABLE);

        for (var Index = 0; Index < ENTRIES_PER_TABLE; Index++)
        {
            Writeˉentry(Tables, Kernelˉpagingˉcontract.PD_PAGE, Index,
                checked((ulong)Index * Kernelˉpagingˉcontract.LARGE_PAGE_BYTES) |
                Kernelˉpagingˉcontract.ENTRY_PRESENT |
                Kernelˉpagingˉcontract.ENTRY_WRITABLE |
                Kernelˉpagingˉcontract.ENTRY_LARGE_PAGE |
                Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        }

        Writeˉentry(Tables, Kernelˉpagingˉcontract.PD_PAGE, 0,
            Tableˉaddress(rootˉaddress, Kernelˉpagingˉcontract.NULL_PT_PAGE) |
            Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉpagingˉcontract.ENTRY_WRITABLE);
        for (var Index = 1; Index < ENTRIES_PER_TABLE; Index++)
        {
            Writeˉentry(Tables, Kernelˉpagingˉcontract.NULL_PT_PAGE, Index,
                checked((ulong)Index * Kernelˉpagingˉcontract.PAGE_BYTES) |
                Kernelˉpagingˉcontract.ENTRY_PRESENT |
                Kernelˉpagingˉcontract.ENTRY_WRITABLE |
                Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        }

        var Codeˉdirectoryˉindex = checked((int)(executableˉaddress / Kernelˉpagingˉcontract.LARGE_PAGE_BYTES));
        Populateˉcodeˉtable(Tables, Kernelˉpagingˉcontract.CODE_PT0_PAGE, Codeˉdirectoryˉindex);
        Populateˉcodeˉtable(Tables, Kernelˉpagingˉcontract.CODE_PT1_PAGE, Codeˉdirectoryˉindex + 1);
        Writeˉentry(Tables, Kernelˉpagingˉcontract.PD_PAGE, Codeˉdirectoryˉindex,
            Tableˉaddress(rootˉaddress, Kernelˉpagingˉcontract.CODE_PT0_PAGE) |
            Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉpagingˉcontract.ENTRY_WRITABLE);
        Writeˉentry(Tables, Kernelˉpagingˉcontract.PD_PAGE, Codeˉdirectoryˉindex + 1,
            Tableˉaddress(rootˉaddress, Kernelˉpagingˉcontract.CODE_PT1_PAGE) |
            Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉpagingˉcontract.ENTRY_WRITABLE);

        for (ulong Address = executableˉaddress; Address < Executableˉend; Address += Kernelˉpagingˉcontract.PAGE_BYTES)
        {
            var Directoryˉindex = checked((int)(Address / Kernelˉpagingˉcontract.LARGE_PAGE_BYTES));
            var Tableˉpage = Directoryˉindex == Codeˉdirectoryˉindex
                ? Kernelˉpagingˉcontract.CODE_PT0_PAGE
                : Kernelˉpagingˉcontract.CODE_PT1_PAGE;
            var Pageˉindex = checked((int)((Address % Kernelˉpagingˉcontract.LARGE_PAGE_BYTES) /
                Kernelˉpagingˉcontract.PAGE_BYTES));
            Writeˉentry(Tables, Tableˉpage, Pageˉindex,
                Address | Kernelˉpagingˉcontract.ENTRY_PRESENT);
        }

        var Mmioˉflags = Kernelˉpagingˉcontract.ENTRY_PRESENT |
            Kernelˉpagingˉcontract.ENTRY_WRITABLE |
            Kernelˉpagingˉcontract.ENTRY_WRITE_THROUGH |
            Kernelˉpagingˉcontract.ENTRY_CACHE_DISABLE |
            Kernelˉpagingˉcontract.ENTRY_LARGE_PAGE |
            Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE;
        Writeˉentry(Tables, Kernelˉpagingˉcontract.MMIO_PD_PAGE,
            Kernelˉpagingˉcontract.MMIO_HPET_PD_INDEX,
            Kernelˉpagingˉcontract.MMIO_HPET_LARGE_PAGE_ADDRESS | Mmioˉflags);
        Writeˉentry(Tables, Kernelˉpagingˉcontract.MMIO_PD_PAGE,
            Kernelˉpagingˉcontract.MMIO_LOCAL_APIC_PD_INDEX,
            Kernelˉpagingˉcontract.MMIO_LOCAL_APIC_LARGE_PAGE_ADDRESS | Mmioˉflags);

        var Record = new byte[Kernelˉpagingˉcontract.RECORD_BYTES];
        BinaryPrimitives.WriteUInt64LittleEndian(Record, Kernelˉpagingˉcontract.RECORD_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(8), Kernelˉpagingˉcontract.RECORD_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(12), Kernelˉpagingˉcontract.RECORD_BYTES);
        BinaryPrimitives.WriteUInt64LittleEndian(Record.AsSpan(16), rootˉaddress);
        BinaryPrimitives.WriteUInt64LittleEndian(Record.AsSpan(24), Kernelˉpagingˉcontract.TABLE_PAGES);
        BinaryPrimitives.WriteUInt64LittleEndian(Record.AsSpan(32), Kernelˉpagingˉcontract.IDENTITY_BYTES);
        BinaryPrimitives.WriteUInt64LittleEndian(Record.AsSpan(40), executableˉaddress);
        BinaryPrimitives.WriteUInt64LittleEndian(Record.AsSpan(48), Kernelˉpagingˉcontract.EXECUTABLE_BYTES);
        BinaryPrimitives.WriteUInt64LittleEndian(Record.AsSpan(56), Kernelˉpagingˉcontract.RECORD_FLAGS);

        return new(new(rootˉaddress, executableˉaddress, Tables.ToImmutableArray(), Record.ToImmutableArray()), []);
    }

    public static ulong Readˉentry(Kernelˉpagingˉplan plan, ulong tableˉpage, int entryˉindex)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (tableˉpage >= Kernelˉpagingˉcontract.TABLE_PAGES || entryˉindex is < 0 or >= ENTRIES_PER_TABLE)
        {
            throw new ArgumentOutOfRangeException(nameof(entryˉindex));
        }
        var Offset = checked((int)(tableˉpage * Kernelˉpagingˉcontract.PAGE_BYTES) + (entryˉindex * sizeof(ulong)));
        return BinaryPrimitives.ReadUInt64LittleEndian(plan.Tableˉbytes.AsSpan().Slice(Offset));
    }

    private static void Populateˉcodeˉtable(byte[] tables, ulong tableˉpage, int directoryˉindex)
    {
        var Baseˉaddress = checked((ulong)directoryˉindex * Kernelˉpagingˉcontract.LARGE_PAGE_BYTES);
        for (var Index = 0; Index < ENTRIES_PER_TABLE; Index++)
        {
            Writeˉentry(tables, tableˉpage, Index,
                Baseˉaddress + checked((ulong)Index * Kernelˉpagingˉcontract.PAGE_BYTES) |
                Kernelˉpagingˉcontract.ENTRY_PRESENT |
                Kernelˉpagingˉcontract.ENTRY_WRITABLE |
                Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        }
    }

    private static ulong Tableˉaddress(ulong rootˉaddress, ulong page) =>
        rootˉaddress + (page * Kernelˉpagingˉcontract.PAGE_BYTES);

    private static void Writeˉentry(byte[] tables, ulong tableˉpage, int entryˉindex, ulong value)
    {
        var Offset = checked((int)(tableˉpage * Kernelˉpagingˉcontract.PAGE_BYTES) + (entryˉindex * sizeof(ulong)));
        BinaryPrimitives.WriteUInt64LittleEndian(tables.AsSpan(Offset), value);
    }

    private static Kernelˉpagingˉplanˉresult Fail(string code, string message) =>
        new(null, [new(code, message)]);
}
