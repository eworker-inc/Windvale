using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Compiler.Native;

namespace Windvale.Bootstrap;

public static class Kernelˉprocessˉcontract
{
    public const int FORMAT_VERSION = 2;
    public const string TARGET_NAME = "x86-64-kernel-process-v2";
    public const string ENTER_SYMBOL = "Windvale_kernel_x64_process_enter";
    public const string POLICY_SYMBOL = "Windvale_kernel_process_policy";
    public const string USER_ENTRY_SYMBOL = "Windvale_process_user_entry";
    public const string INIT_SERVICE_ENTRY_SYMBOL = "Windvale_init_resource_user_entry";
    public const string INIT_SERVICE_MAIN_SYMBOL = "Windvale_init_resource_service_main";
    public const string SYSCALL_ENTRY_SYMBOL = "Windvale_kernel_x64_process_syscall";
    public const string EXCEPTION_ENTRY_SYMBOL = "Windvale_kernel_x64_process_exception";
    public const string EXCEPTION_6_ENTRY_SYMBOL = "Windvale_kernel_x64_process_exception_6_entry";
    public const string EXCEPTION_13_ENTRY_SYMBOL = "Windvale_kernel_x64_process_exception_13_entry";
    public const string EXCEPTION_14_ENTRY_SYMBOL = "Windvale_kernel_x64_process_exception_14_entry";
    public const int POLICY_TOKEN = 92;
    public const int EXPECTED_RESULT = 29;
    public const string USER_FAULT_CONTAINED_MARKER = "user-fault=contained\n";
    public const uint INIT_PROCESS_ID = 1;
    public const uint INIT_THREAD_ID = 1;
    public const uint CLIENT_PROCESS_ID = 2;
    public const uint CLIENT_THREAD_ID = 2;
    public const uint ROLE_INIT_SERVICE = 1;
    public const uint ROLE_CLIENT = 2;
    public const uint PROCESS_STATE_READY = 1;
    public const uint PROCESS_STATE_RUNNING = 2;
    public const uint PROCESS_STATE_EXITED = 3;
    public const uint PROCESS_STATE_FAULTED = 4;
    public const uint THREAD_STATE_READY = 1;
    public const uint THREAD_STATE_RUNNING = 2;
    public const uint THREAD_STATE_EXITED = 3;
    public const uint THREAD_STATE_FAULTED = 4;
    public const uint THREAD_STATE_WAITING = 5;
    public const uint MEMORY_PAGE_BUDGET = 3;
    public const uint INSTRUCTION_BUDGET = 64;
    public const uint CALL_DEPTH_BUDGET = 1;
    public const uint HANDLE_BUDGET = 1;
    public const uint SYSCALL_BUDGET = 2;
    public const uint CHANNEL_CAPACITY = 1;
    public const uint CAPABILITY_SLOT = 0;
    public const uint CAPABILITY_GENERATION = 1;
    public const uint CAPABILITY_RIGHT_SEND = 1U << 0;
    public const uint CAPABILITY_RIGHT_RECEIVE = 1U << 1;
    public const uint CAPABILITY_REFERENCE = (CAPABILITY_GENERATION << 16) | CAPABILITY_SLOT;
    public const uint SYSCALL_SEND = 1;
    public const uint SYSCALL_RECEIVE = 2;
    public const uint SYSCALL_EXIT = 3;
    public const ulong ALLOCATION_PAGES = 7;
    public const ulong TABLE_PAGES = 4;
    public const ulong TABLE_BYTES = TABLE_PAGES * Kernelˉpagingˉcontract.PAGE_BYTES;
    public const ulong PML4_PAGE = 0;
    public const ulong PDPT_PAGE = 1;
    public const ulong PD_PAGE = 2;
    public const ulong USER_PT_PAGE = 3;
    public const ulong USER_CODE_PAGE = 4;
    public const ulong USER_STACK_PAGE = 5;
    public const ulong USER_DATA_PAGE = 6;
    public const ulong ENTRY_USER = 1UL << 2;
    public const uint INIT_RECORD_OFFSET = 256;
    public const uint CLIENT_RECORD_OFFSET = 768;
    public const uint CHANNEL_RECORD_OFFSET = 1_024;
    public const uint RECORD_BYTES = 256;
    public const ulong RECORD_MAGIC = 0x3230_434F_5250_5657;
    public const uint RECORD_VERSION = 2;
    public const int MODULE_DIGEST_BYTES = 32;
    public const uint PROCESS_STATE_OFFSET = 16;
    public const uint THREAD_STATE_OFFSET = 20;
    public const uint ROOT_ADDRESS_OFFSET = 64;
    public const uint USER_CODE_ADDRESS_OFFSET = 72;
    public const uint USER_STACK_ADDRESS_OFFSET = 80;
    public const uint USER_DATA_ADDRESS_OFFSET = 88;
    public const uint CAPABILITY_RIGHTS_OFFSET = 120;
    public const uint KERNEL_STACK_OFFSET = 128;
    public const uint KERNEL_RESUME_OFFSET = 136;
    public const uint USER_STACK_POINTER_OFFSET = 144;
    public const uint USER_INSTRUCTION_POINTER_OFFSET = 152;
    public const uint USER_FLAGS_OFFSET = 160;
    public const uint SYSCALL_COUNT_OFFSET = 168;
    public const uint RESULT_OFFSET = 180;
    public const uint FAULT_VECTOR_OFFSET = 184;
    public const uint FAULT_ERROR_OFFSET = 188;
    public const uint CHANNEL_ADDRESS_OFFSET = 192;
    public const uint ROLE_OFFSET = 200;
    public const uint WAIT_REASON_OFFSET = 204;
    public const uint USER_CONTEXT_POINTER_OFFSET = 208;
    public const uint WAIT_REASON_NONE = 0;
    public const uint WAIT_REASON_CHANNEL_RECEIVE = 1;
    public const ulong CHANNEL_MAGIC = 0x3130_4E41_4843_5657;
    public const uint CHANNEL_VERSION = 1;
    public const uint CHANNEL_RECORD_BYTES = 64;
    public const uint CHANNEL_STATE_OFFSET = 16;
    public const uint CHANNEL_MESSAGE_OFFSET = 20;
    public const uint CHANNEL_SENDER_OFFSET = 24;
    public const uint CHANNEL_RECEIVER_OFFSET = 28;
    public const uint CHANNEL_SEND_COUNT_OFFSET = 32;
    public const uint CHANNEL_RECEIVE_COUNT_OFFSET = 36;
    public const uint CHANNEL_WAITER_OFFSET = 40;
    public const uint CHANNEL_WAKE_COUNT_OFFSET = 44;
    public const uint CHANNEL_CAPACITY_OFFSET = 48;
    public const uint GDT_OFFSET = 512;
    public const uint GDT_BYTES = 56;
    public const uint GDTR_OFFSET = 576;
    public const uint TSS_OFFSET = 640;
    public const uint TSS_BYTES = 104;
}

public sealed record Kernelˉprocessˉdefinition(
    uint Processˉid,
    uint Threadˉid,
    uint Role,
    uint Capabilityˉrights,
    ulong Channelˉaddress);

public sealed record Kernelˉprocessˉdiagnostic(string Code, string Message);

public sealed record Kernelˉprocessˉplan(
    ulong Rootˉaddress,
    ulong Userˉcodeˉaddress,
    ulong Userˉstackˉaddress,
    ulong Userˉdataˉaddress,
    ImmutableArray<byte> Tableˉbytes,
    ImmutableArray<byte> Userˉcodeˉbytes,
    ImmutableArray<byte> Userˉstackˉbytes,
    ImmutableArray<byte> Userˉdataˉbytes,
    ImmutableArray<byte> Processˉrecord);

public sealed record Kernelˉprocessˉplanˉresult(
    Kernelˉprocessˉplan? Plan,
    ImmutableArray<Kernelˉprocessˉdiagnostic> Diagnostics)
{
    public bool Success => Plan is not null && Diagnostics.IsEmpty;
}

public static class Kernelˉprocessˉplanner
{
    private const int ENTRIES_PER_TABLE = 512;

    public static Kernelˉprocessˉplanˉresult Plan(
        Kernelˉpagingˉplan kernelˉpaging,
        ulong allocationˉaddress,
        ReadOnlySpan<byte> userˉimage,
        ReadOnlySpan<byte> moduleˉdigest,
        Kernelˉprocessˉdefinition definition)
    {
        ArgumentNullException.ThrowIfNull(kernelˉpaging);
        ArgumentNullException.ThrowIfNull(definition);
        var Allocationˉbytes = Kernelˉprocessˉcontract.ALLOCATION_PAGES * Kernelˉpagingˉcontract.PAGE_BYTES;
        if (allocationˉaddress == 0 ||
            (allocationˉaddress & (Kernelˉpagingˉcontract.PAGE_BYTES - 1)) != 0 ||
            allocationˉaddress > Kernelˉpagingˉcontract.IDENTITY_BYTES - Allocationˉbytes)
        {
            return Fail("WVOS6001", "The process allocation must be a complete aligned seven-page range below 1 GiB.");
        }
        var Allocationˉend = allocationˉaddress + Allocationˉbytes;
        if ((allocationˉaddress / Kernelˉpagingˉcontract.LARGE_PAGE_BYTES) !=
            ((Allocationˉend - 1) / Kernelˉpagingˉcontract.LARGE_PAGE_BYTES))
        {
            return Fail("WVOS6002", "The process allocation must fit one 2 MiB identity-table region.");
        }
        if (userˉimage.IsEmpty || userˉimage.Length > (int)Kernelˉpagingˉcontract.PAGE_BYTES)
        {
            return Fail("WVOS6003", "The process image must occupy 1 through 4,096 bytes.");
        }
        if (moduleˉdigest.Length != Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES)
        {
            return Fail("WVOS6004", "The process module identity must be one SHA-256 digest.");
        }
        var Isˉinit = definition is
        {
            Processˉid: Kernelˉprocessˉcontract.INIT_PROCESS_ID,
            Threadˉid: Kernelˉprocessˉcontract.INIT_THREAD_ID,
            Role: Kernelˉprocessˉcontract.ROLE_INIT_SERVICE,
            Capabilityˉrights: Kernelˉprocessˉcontract.CAPABILITY_RIGHT_RECEIVE,
        };
        var Isˉclient = definition is
        {
            Processˉid: Kernelˉprocessˉcontract.CLIENT_PROCESS_ID,
            Threadˉid: Kernelˉprocessˉcontract.CLIENT_THREAD_ID,
            Role: Kernelˉprocessˉcontract.ROLE_CLIENT,
            Capabilityˉrights: Kernelˉprocessˉcontract.CAPABILITY_RIGHT_SEND,
        };
        if ((!Isˉinit && !Isˉclient) ||
            definition.Channelˉaddress == 0 ||
            (definition.Channelˉaddress & (sizeof(ulong) - 1)) != 0 ||
            definition.Channelˉaddress >= Kernelˉpagingˉcontract.IDENTITY_BYTES)
        {
            return Fail("WVOS6006", "The process identity, role, reduced endpoint rights, or shared-channel address is invalid.");
        }

        var Userˉcodeˉaddress = Pageˉaddress(allocationˉaddress, Kernelˉprocessˉcontract.USER_CODE_PAGE);
        var Userˉstackˉaddress = Pageˉaddress(allocationˉaddress, Kernelˉprocessˉcontract.USER_STACK_PAGE);
        var Userˉdataˉaddress = Pageˉaddress(allocationˉaddress, Kernelˉprocessˉcontract.USER_DATA_PAGE);
        var Executableˉend = kernelˉpaging.Executableˉaddress + Kernelˉpagingˉcontract.EXECUTABLE_BYTES;
        if (allocationˉaddress < Executableˉend && Allocationˉend > kernelˉpaging.Executableˉaddress)
        {
            return Fail("WVOS6005", "The process allocation overlaps the retained kernel executable window.");
        }

        var Tables = new byte[checked((int)Kernelˉprocessˉcontract.TABLE_BYTES)];
        Copyˉtable(kernelˉpaging, Kernelˉpagingˉcontract.PML4_PAGE, Tables, Kernelˉprocessˉcontract.PML4_PAGE);
        Copyˉtable(kernelˉpaging, Kernelˉpagingˉcontract.PDPT_PAGE, Tables, Kernelˉprocessˉcontract.PDPT_PAGE);
        Copyˉtable(kernelˉpaging, Kernelˉpagingˉcontract.PD_PAGE, Tables, Kernelˉprocessˉcontract.PD_PAGE);
        Writeˉentry(Tables, Kernelˉprocessˉcontract.PML4_PAGE, 0,
            Pageˉaddress(allocationˉaddress, Kernelˉprocessˉcontract.PDPT_PAGE) |
            Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉpagingˉcontract.ENTRY_WRITABLE |
            Kernelˉprocessˉcontract.ENTRY_USER);
        Writeˉentry(Tables, Kernelˉprocessˉcontract.PDPT_PAGE, 0,
            Pageˉaddress(allocationˉaddress, Kernelˉprocessˉcontract.PD_PAGE) |
            Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉpagingˉcontract.ENTRY_WRITABLE |
            Kernelˉprocessˉcontract.ENTRY_USER);

        var Regionˉaddress = allocationˉaddress & ~(Kernelˉpagingˉcontract.LARGE_PAGE_BYTES - 1);
        for (var Index = 0; Index < ENTRIES_PER_TABLE; Index++)
        {
            var Address = Regionˉaddress + checked((ulong)Index * Kernelˉpagingˉcontract.PAGE_BYTES);
            Writeˉentry(Tables, Kernelˉprocessˉcontract.USER_PT_PAGE, Index,
                Address == 0
                    ? 0
                    : Address | Kernelˉpagingˉcontract.ENTRY_PRESENT |
                        Kernelˉpagingˉcontract.ENTRY_WRITABLE |
                        Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        }
        var Directoryˉindex = checked((int)(allocationˉaddress / Kernelˉpagingˉcontract.LARGE_PAGE_BYTES));
        Writeˉentry(Tables, Kernelˉprocessˉcontract.PD_PAGE, Directoryˉindex,
            Pageˉaddress(allocationˉaddress, Kernelˉprocessˉcontract.USER_PT_PAGE) |
            Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉpagingˉcontract.ENTRY_WRITABLE |
            Kernelˉprocessˉcontract.ENTRY_USER);
        Writeˉuserˉentry(Tables, Regionˉaddress, Userˉcodeˉaddress,
            Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER);
        Writeˉuserˉentry(Tables, Regionˉaddress, Userˉstackˉaddress,
            Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉpagingˉcontract.ENTRY_WRITABLE |
            Kernelˉprocessˉcontract.ENTRY_USER | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        Writeˉuserˉentry(Tables, Regionˉaddress, Userˉdataˉaddress,
            Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉpagingˉcontract.ENTRY_WRITABLE |
            Kernelˉprocessˉcontract.ENTRY_USER | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);

        var Code = new byte[Kernelˉpagingˉcontract.PAGE_BYTES];
        userˉimage.CopyTo(Code);
        var Stack = new byte[Kernelˉpagingˉcontract.PAGE_BYTES];
        var Data = new byte[Kernelˉpagingˉcontract.PAGE_BYTES];
        BinaryPrimitives.WriteUInt32LittleEndian(Data, Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Data.AsSpan(4), Nativeˉexecutionˉcontextˉcontract.SIZE);
        BinaryPrimitives.WriteUInt64LittleEndian(Data.AsSpan(8), Kernelˉprocessˉcontract.INSTRUCTION_BUDGET);
        BinaryPrimitives.WriteUInt64LittleEndian(Data.AsSpan(16), Kernelˉprocessˉcontract.CALL_DEPTH_BUDGET);

        var Record = new byte[Kernelˉprocessˉcontract.RECORD_BYTES];
        BinaryPrimitives.WriteUInt64LittleEndian(Record, Kernelˉprocessˉcontract.RECORD_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(8), Kernelˉprocessˉcontract.RECORD_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(12), Kernelˉprocessˉcontract.RECORD_BYTES);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(16), Kernelˉprocessˉcontract.PROCESS_STATE_READY);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(20), Kernelˉprocessˉcontract.THREAD_STATE_READY);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(24), definition.Processˉid);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(28), definition.Threadˉid);
        moduleˉdigest.CopyTo(Record.AsSpan(32, Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES));
        BinaryPrimitives.WriteUInt64LittleEndian(Record.AsSpan(64), allocationˉaddress);
        BinaryPrimitives.WriteUInt64LittleEndian(Record.AsSpan(72), Userˉcodeˉaddress);
        BinaryPrimitives.WriteUInt64LittleEndian(Record.AsSpan(80), Userˉstackˉaddress);
        BinaryPrimitives.WriteUInt64LittleEndian(Record.AsSpan(88), Userˉdataˉaddress);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(96), Kernelˉprocessˉcontract.MEMORY_PAGE_BUDGET);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(100), Kernelˉprocessˉcontract.INSTRUCTION_BUDGET);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(104), Kernelˉprocessˉcontract.HANDLE_BUDGET);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(108), Kernelˉprocessˉcontract.SYSCALL_BUDGET);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(112), Kernelˉprocessˉcontract.CAPABILITY_SLOT);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(116), Kernelˉprocessˉcontract.CAPABILITY_GENERATION);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(120), definition.Capabilityˉrights);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(124), Kernelˉprocessˉcontract.CHANNEL_CAPACITY);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.CHANNEL_ADDRESS_OFFSET), definition.Channelˉaddress);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.ROLE_OFFSET), definition.Role);

        return new(new(
            allocationˉaddress,
            Userˉcodeˉaddress,
            Userˉstackˉaddress,
            Userˉdataˉaddress,
            Tables.ToImmutableArray(),
            Code.ToImmutableArray(),
            Stack.ToImmutableArray(),
            Data.ToImmutableArray(),
            Record.ToImmutableArray()), []);
    }

    public static ulong Readˉentry(Kernelˉprocessˉplan plan, ulong tableˉpage, int entryˉindex)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (tableˉpage >= Kernelˉprocessˉcontract.TABLE_PAGES || entryˉindex is < 0 or >= ENTRIES_PER_TABLE)
        {
            throw new ArgumentOutOfRangeException(nameof(entryˉindex));
        }
        var Offset = checked((int)(tableˉpage * Kernelˉpagingˉcontract.PAGE_BYTES) + entryˉindex * sizeof(ulong));
        return BinaryPrimitives.ReadUInt64LittleEndian(plan.Tableˉbytes.AsSpan().Slice(Offset));
    }

    private static void Copyˉtable(
        Kernelˉpagingˉplan source,
        ulong sourceˉpage,
        byte[] destination,
        ulong destinationˉpage) =>
        source.Tableˉbytes.AsSpan().Slice(
            checked((int)(sourceˉpage * Kernelˉpagingˉcontract.PAGE_BYTES)),
            checked((int)Kernelˉpagingˉcontract.PAGE_BYTES)).CopyTo(
                destination.AsSpan(checked((int)(destinationˉpage * Kernelˉpagingˉcontract.PAGE_BYTES))));

    private static ulong Pageˉaddress(ulong allocationˉaddress, ulong page) =>
        allocationˉaddress + page * Kernelˉpagingˉcontract.PAGE_BYTES;

    private static void Writeˉuserˉentry(byte[] tables, ulong regionˉaddress, ulong address, ulong flags)
    {
        var Index = checked((int)((address - regionˉaddress) / Kernelˉpagingˉcontract.PAGE_BYTES));
        Writeˉentry(Tables: tables, tableˉpage: Kernelˉprocessˉcontract.USER_PT_PAGE,
            entryˉindex: Index, value: address | flags);
    }

    private static void Writeˉentry(byte[] Tables, ulong tableˉpage, int entryˉindex, ulong value)
    {
        var Offset = checked((int)(tableˉpage * Kernelˉpagingˉcontract.PAGE_BYTES) + entryˉindex * sizeof(ulong));
        BinaryPrimitives.WriteUInt64LittleEndian(Tables.AsSpan(Offset), value);
    }

    private static Kernelˉprocessˉplanˉresult Fail(string code, string message) =>
        new(null, [new(code, message)]);
}
