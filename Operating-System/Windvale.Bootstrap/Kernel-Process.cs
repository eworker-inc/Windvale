using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;

namespace Windvale.Bootstrap;

public static class Kernelˉprocessˉcontract
{
    public const int FORMAT_VERSION = 6;
    public const string TARGET_NAME = "x86-64-kernel-process-v6";
    public const string ENTER_SYMBOL = "Windvale_kernel_x64_process_enter";
    public const string POLICY_SYMBOL = "Windvale_kernel_process_policy";
    public const string USER_ENTRY_SYMBOL = "Windvale_process_user_entry";
    public const string INIT_SERVICE_ENTRY_SYMBOL = "Windvale_init_resource_user_entry";
    public const string INIT_SERVICE_MAIN_SYMBOL = "Windvale_init_resource_service_main";
    public const string BYTECODE_INTERPRETER_MAIN_SYMBOL = "Windvale_user_bytecode_interpreter_main";
    public const string BOOT_RESOURCE_SERVICE_SYMBOL = "Windvale_os_boot_resource_read_bytes";
    public const string SYSCALL_ENTRY_SYMBOL = "Windvale_kernel_x64_process_syscall";
    public const string EXCEPTION_ENTRY_SYMBOL = "Windvale_kernel_x64_process_exception";
    public const string EXCEPTION_6_ENTRY_SYMBOL = "Windvale_kernel_x64_process_exception_6_entry";
    public const string EXCEPTION_13_ENTRY_SYMBOL = "Windvale_kernel_x64_process_exception_13_entry";
    public const string EXCEPTION_14_ENTRY_SYMBOL = "Windvale_kernel_x64_process_exception_14_entry";
    public const int POLICY_TOKEN = 95;
    public const int EXPECTED_RESULT = 29;
    public const string USER_FAULT_CONTAINED_MARKER = "user-fault=contained\n";
    public const uint INIT_PROCESS_ID = 1;
    public const uint INIT_THREAD_ID = 1;
    public const uint CLIENT_PROCESS_ID = 2;
    public const uint CLIENT_THREAD_ID = 2;
    public const uint ROLE_INIT_SERVICE = 1;
    public const uint ROLE_BYTECODE_INTERPRETER = 2;
    public const uint RUNTIME_KIND_AOT_SERVICE = 1;
    public const uint RUNTIME_KIND_BYTECODE_INTERPRETER = 2;
    public const uint PROCESS_STATE_READY = 1;
    public const uint PROCESS_STATE_RUNNING = 2;
    public const uint PROCESS_STATE_EXITED = 3;
    public const uint PROCESS_STATE_FAULTED = 4;
    public const uint THREAD_STATE_READY = 1;
    public const uint THREAD_STATE_RUNNING = 2;
    public const uint THREAD_STATE_EXITED = 3;
    public const uint THREAD_STATE_FAULTED = 4;
    public const uint THREAD_STATE_WAITING = 5;
    public const uint INIT_MEMORY_PAGE_BUDGET = 4;
    public const uint CLIENT_MEMORY_PAGE_BUDGET = 38;
    public const uint INIT_INSTRUCTION_BUDGET = 64;
    public const uint CLIENT_INSTRUCTION_BUDGET = 4_678;
    public const uint INIT_CALL_DEPTH_BUDGET = 1;
    public const uint CLIENT_CALL_DEPTH_BUDGET = 3;
    public const uint HANDLE_BUDGET = 1;
    public const uint INIT_SYSCALL_BUDGET = 3;
    public const uint CLIENT_SYSCALL_BUDGET = 2;
    public const uint CHANNEL_CAPACITY = 1;
    public const uint CAPABILITY_SLOT = 0;
    public const uint CAPABILITY_GENERATION = 1;
    public const uint CAPABILITY_RIGHT_SEND = 1U << 0;
    public const uint CAPABILITY_RIGHT_RECEIVE = 1U << 1;
    public const uint CAPABILITY_RIGHT_GRANT_BOOT_RESOURCE = 1U << 2;
    public const uint INIT_CAPABILITY_RIGHTS =
        CAPABILITY_RIGHT_RECEIVE | CAPABILITY_RIGHT_GRANT_BOOT_RESOURCE;
    public const uint CAPABILITY_REFERENCE = (CAPABILITY_GENERATION << 16) | CAPABILITY_SLOT;
    public const uint SYSCALL_SEND = 1;
    public const uint SYSCALL_RECEIVE = 2;
    public const uint SYSCALL_EXIT = 3;
    public const uint SYSCALL_GRANT_BOOT_RESOURCE = 4;
    public const ulong INIT_ALLOCATION_PAGES = 8;
    public const ulong CLIENT_ALLOCATION_PAGES = 42;
    public const ulong TABLE_PAGES = 4;
    public const ulong TABLE_BYTES = TABLE_PAGES * Kernelˉpagingˉcontract.PAGE_BYTES;
    public const ulong PML4_PAGE = 0;
    public const ulong PDPT_PAGE = 1;
    public const ulong PD_PAGE = 2;
    public const ulong USER_PT_PAGE = 3;
    public const ulong USER_CODE_PAGE = 4;
    public const ulong INIT_CODE_PAGES = 1;
    public const ulong CLIENT_CODE_PAGES = 32;
    public const ulong INIT_STACK_PAGES = 1;
    public const ulong CLIENT_STACK_PAGES = 4;
    public const ulong INIT_STACK_PAGE = USER_CODE_PAGE + INIT_CODE_PAGES;
    public const ulong INIT_DATA_PAGE = INIT_STACK_PAGE + INIT_STACK_PAGES;
    public const ulong INIT_RUNTIME_INPUT_PAGE = INIT_DATA_PAGE + 1;
    public const ulong CLIENT_STACK_PAGE = USER_CODE_PAGE + CLIENT_CODE_PAGES;
    public const ulong CLIENT_DATA_PAGE = CLIENT_STACK_PAGE + CLIENT_STACK_PAGES;
    public const ulong CLIENT_RUNTIME_INPUT_PAGE = CLIENT_DATA_PAGE + 1;
    public const ulong CLIENT_CODE_BYTES = CLIENT_CODE_PAGES * Kernelˉpagingˉcontract.PAGE_BYTES;
    public const int MAXIMUM_RUNTIME_INPUT_BYTES = 4_096;
    public const uint BOOT_RESOURCE_SERVICE_BYTES = 199;
    public const uint RUNTIME_SERVICE_TABLE_OFFSET = 128;
    public const uint BOOT_RESOURCE_TABLE_OFFSET = 256;
    public const uint BOOT_RESOURCE_TABLE_MAGIC = 0x5242_5657;
    public const uint BOOT_RESOURCE_TABLE_VERSION = 1;
    public const uint BOOT_RESOURCE_TABLE_BYTES = 32;
    public const uint BOOT_RESOURCE_DATA_POINTER_OFFSET = 16;
    public const uint BOOT_RESOURCE_DATA_LENGTH_OFFSET = 24;
    public const uint BOOT_RESOURCE_RESERVED_OFFSET = 28;
    public const ulong ENTRY_USER = 1UL << 2;
    public const uint INIT_RECORD_OFFSET = 256;
    public const uint CLIENT_RECORD_OFFSET = 768;
    public const uint CHANNEL_RECORD_OFFSET = 1_024;
    public const uint RECORD_BYTES = 256;
    public const ulong RECORD_MAGIC = 0x3630_434F_5250_5657;
    public const uint RECORD_VERSION = 6;
    public const int MODULE_DIGEST_BYTES = 32;
    public const uint PROCESS_STATE_OFFSET = 16;
    public const uint THREAD_STATE_OFFSET = 20;
    public const uint ROOT_ADDRESS_OFFSET = 64;
    public const uint USER_CODE_ADDRESS_OFFSET = 72;
    public const uint USER_STACK_ADDRESS_OFFSET = 80;
    public const uint USER_DATA_ADDRESS_OFFSET = 88;
    public const uint CAPABILITY_RIGHTS_OFFSET = 120;
    public const uint SYSCALL_BUDGET_OFFSET = 108;
    public const uint KERNEL_STACK_OFFSET = 128;
    public const uint KERNEL_RESUME_OFFSET = 136;
    public const uint USER_STACK_POINTER_OFFSET = 144;
    public const uint USER_INSTRUCTION_POINTER_OFFSET = 152;
    public const uint USER_FLAGS_OFFSET = 160;
    public const uint SYSCALL_COUNT_OFFSET = 168;
    public const uint STACK_PAGE_COUNT_OFFSET = 172;
    public const uint RUNTIME_PROFILE_OFFSET = 176;
    public const uint RESULT_OFFSET = 180;
    public const uint FAULT_VECTOR_OFFSET = 184;
    public const uint FAULT_ERROR_OFFSET = 188;
    public const uint CHANNEL_ADDRESS_OFFSET = 192;
    public const uint ROLE_OFFSET = 200;
    public const uint WAIT_REASON_OFFSET = 204;
    public const uint USER_CONTEXT_POINTER_OFFSET = 208;
    public const uint PROGRAM_DIGEST_OFFSET = 216;
    public const uint CODE_PAGE_COUNT_OFFSET = 248;
    public const uint RUNTIME_KIND_OFFSET = 252;
    public const uint RUNTIME_PROFILE_BOOT_RESOURCE_OWNER = 1;
    public const uint RUNTIME_PROFILE_GRANTED_BOOT_RESOURCE_INTERPRETER = 4;
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
    public const uint RESOURCE_RECORD_OFFSET = CHANNEL_RECORD_OFFSET + CHANNEL_RECORD_BYTES;
    public const uint RESOURCE_RECORD_BYTES = 128;
    public const ulong RESOURCE_MAGIC = 0x3130_3053_4552_5657;
    public const uint RESOURCE_VERSION = 1;
    public const uint RESOURCE_STATE_OWNED = 1;
    public const uint RESOURCE_STATE_BORROWED = 2;
    public const uint RESOURCE_ID = 1;
    public const uint RESOURCE_STATE_OFFSET = 16;
    public const uint RESOURCE_ID_OFFSET = 20;
    public const uint RESOURCE_OWNER_OFFSET = 24;
    public const uint RESOURCE_BORROWER_OFFSET = 28;
    public const uint RESOURCE_SOURCE_ADDRESS_OFFSET = 32;
    public const uint RESOURCE_LENGTH_OFFSET = 40;
    public const uint RESOURCE_FLAGS_OFFSET = 44;
    public const uint RESOURCE_TARGET_ROOT_OFFSET = 48;
    public const uint RESOURCE_TARGET_DATA_OFFSET = 56;
    public const uint RESOURCE_TARGET_ADDRESS_OFFSET = 64;
    public const uint RESOURCE_SERVICE_ADDRESS_OFFSET = 72;
    public const uint RESOURCE_DIGEST_OFFSET = 80;
    public const uint RESOURCE_GRANT_COUNT_OFFSET = 112;
    public const uint RESOURCE_MAPPING_COUNT_OFFSET = 116;
    public const uint RESOURCE_TARGET_PTE_OFFSET = 120;
    public const uint RESOURCE_FLAG_IMMUTABLE = 1U << 0;
    public const uint RESOURCE_FLAG_READ_ONLY = 1U << 1;
    public const uint RESOURCE_FLAG_NO_EXECUTE = 1U << 2;
    public const uint RESOURCE_FLAGS =
        RESOURCE_FLAG_IMMUTABLE | RESOURCE_FLAG_READ_ONLY | RESOURCE_FLAG_NO_EXECUTE;
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
    ulong Userˉcodeˉpages,
    ulong Userˉstackˉpages,
    ImmutableArray<byte> Tableˉbytes,
    ImmutableArray<byte> Userˉcodeˉbytes,
    ImmutableArray<byte> Userˉstackˉbytes,
    ImmutableArray<byte> Userˉdataˉbytes,
    ulong Userˉruntimeˉinputˉaddress,
    ImmutableArray<byte> Userˉruntimeˉinputˉbytes,
    ImmutableArray<byte> Processˉrecord);

public sealed record Kernelˉprocessˉplanˉresult(
    Kernelˉprocessˉplan? Plan,
    ImmutableArray<Kernelˉprocessˉdiagnostic> Diagnostics)
{
    public bool Success => Plan is not null && Diagnostics.IsEmpty;
}

public sealed record Kernelˉresourceˉgrantˉplan(
    ImmutableArray<byte> Resourceˉrecord,
    ImmutableArray<byte> Clientˉtableˉbytes,
    ImmutableArray<byte> Clientˉdataˉbytes);

public sealed record Kernelˉresourceˉgrantˉresult(
    Kernelˉresourceˉgrantˉplan? Plan,
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
        ReadOnlySpan<byte> programˉdigest,
        ReadOnlySpan<byte> runtimeˉinput,
        uint bootˉresourceˉserviceˉoffset,
        Kernelˉprocessˉdefinition definition)
    {
        ArgumentNullException.ThrowIfNull(kernelˉpaging);
        ArgumentNullException.ThrowIfNull(definition);
        var Isˉinit = definition is
        {
            Processˉid: Kernelˉprocessˉcontract.INIT_PROCESS_ID,
            Threadˉid: Kernelˉprocessˉcontract.INIT_THREAD_ID,
            Role: Kernelˉprocessˉcontract.ROLE_INIT_SERVICE,
            Capabilityˉrights: Kernelˉprocessˉcontract.INIT_CAPABILITY_RIGHTS,
        };
        var Isˉclient = definition is
        {
            Processˉid: Kernelˉprocessˉcontract.CLIENT_PROCESS_ID,
            Threadˉid: Kernelˉprocessˉcontract.CLIENT_THREAD_ID,
            Role: Kernelˉprocessˉcontract.ROLE_BYTECODE_INTERPRETER,
            Capabilityˉrights: Kernelˉprocessˉcontract.CAPABILITY_RIGHT_SEND,
        };
        if ((!Isˉinit && !Isˉclient) ||
            definition.Channelˉaddress == 0 ||
            (definition.Channelˉaddress & (sizeof(ulong) - 1)) != 0 ||
            definition.Channelˉaddress >= Kernelˉpagingˉcontract.IDENTITY_BYTES)
        {
            return Fail("WVOS6006", "The process identity, role, reduced endpoint rights, or shared-channel address is invalid.");
        }

        var Allocationˉpages = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_ALLOCATION_PAGES
            : Kernelˉprocessˉcontract.CLIENT_ALLOCATION_PAGES;
        var Codeˉpages = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_CODE_PAGES
            : Kernelˉprocessˉcontract.CLIENT_CODE_PAGES;
        var Stackˉpage = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_STACK_PAGE
            : Kernelˉprocessˉcontract.CLIENT_STACK_PAGE;
        var Stackˉpages = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_STACK_PAGES
            : Kernelˉprocessˉcontract.CLIENT_STACK_PAGES;
        var Dataˉpage = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_DATA_PAGE
            : Kernelˉprocessˉcontract.CLIENT_DATA_PAGE;
        var Memoryˉpageˉbudget = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_MEMORY_PAGE_BUDGET
            : Kernelˉprocessˉcontract.CLIENT_MEMORY_PAGE_BUDGET;
        var Instructionˉbudget = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_INSTRUCTION_BUDGET
            : Kernelˉprocessˉcontract.CLIENT_INSTRUCTION_BUDGET;
        var Callˉdepthˉbudget = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_CALL_DEPTH_BUDGET
            : Kernelˉprocessˉcontract.CLIENT_CALL_DEPTH_BUDGET;
        var Runtimeˉkind = Isˉinit
            ? Kernelˉprocessˉcontract.RUNTIME_KIND_AOT_SERVICE
            : Kernelˉprocessˉcontract.RUNTIME_KIND_BYTECODE_INTERPRETER;
        var Runtimeˉprofile = Isˉinit
            ? Kernelˉprocessˉcontract.RUNTIME_PROFILE_BOOT_RESOURCE_OWNER
            : Kernelˉprocessˉcontract.RUNTIME_PROFILE_GRANTED_BOOT_RESOURCE_INTERPRETER;
        var Syscallˉbudget = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_SYSCALL_BUDGET
            : Kernelˉprocessˉcontract.CLIENT_SYSCALL_BUDGET;
        var Allocationˉbytes = Allocationˉpages * Kernelˉpagingˉcontract.PAGE_BYTES;
        if (allocationˉaddress == 0 ||
            (allocationˉaddress & (Kernelˉpagingˉcontract.PAGE_BYTES - 1)) != 0 ||
            allocationˉaddress > Kernelˉpagingˉcontract.IDENTITY_BYTES - Allocationˉbytes)
        {
            return Fail("WVOS6001", "The process allocation must be its complete aligned role-specific range below 1 GiB.");
        }
        var Allocationˉend = allocationˉaddress + Allocationˉbytes;
        if ((allocationˉaddress / Kernelˉpagingˉcontract.LARGE_PAGE_BYTES) !=
            ((Allocationˉend - 1) / Kernelˉpagingˉcontract.LARGE_PAGE_BYTES))
        {
            return Fail("WVOS6002", "The process allocation must fit one 2 MiB identity-table region.");
        }
        if (userˉimage.IsEmpty || (ulong)userˉimage.Length > Codeˉpages * Kernelˉpagingˉcontract.PAGE_BYTES)
        {
            return Fail("WVOS6003", "The process image must fit its bounded role-specific RX extent.");
        }
        if (moduleˉdigest.Length != Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES)
        {
            return Fail("WVOS6004", "The process module identity must be one SHA-256 digest.");
        }
        if (programˉdigest.Length != Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES ||
            !programˉdigest.ContainsAnyExcept((byte)0))
        {
            return Fail("WVOS6007", "The process runtime-input identity is invalid for its role.");
        }
        if ((Isˉinit &&
                (runtimeˉinput.Length is < 12 or > Kernelˉprocessˉcontract.MAXIMUM_RUNTIME_INPUT_BYTES ||
                 bootˉresourceˉserviceˉoffset != 0 ||
                 !SHA256.HashData(runtimeˉinput).AsSpan().SequenceEqual(programˉdigest))) ||
            (Isˉclient &&
                (!runtimeˉinput.IsEmpty ||
                 bootˉresourceˉserviceˉoffset > (uint)userˉimage.Length ||
                 Kernelˉprocessˉcontract.BOOT_RESOURCE_SERVICE_BYTES >
                    (uint)userˉimage.Length - bootˉresourceˉserviceˉoffset)))
        {
            return Fail("WVOS6008", "The process runtime-input resource or service leaf is invalid for its role.");
        }

        var Userˉcodeˉaddress = Pageˉaddress(allocationˉaddress, Kernelˉprocessˉcontract.USER_CODE_PAGE);
        var Userˉstackˉaddress = Pageˉaddress(allocationˉaddress, Stackˉpage);
        var Userˉdataˉaddress = Pageˉaddress(allocationˉaddress, Dataˉpage);
        var Userˉruntimeˉinputˉaddress = Pageˉaddress(
            allocationˉaddress,
            Isˉinit
                ? Kernelˉprocessˉcontract.INIT_RUNTIME_INPUT_PAGE
                : Kernelˉprocessˉcontract.CLIENT_RUNTIME_INPUT_PAGE);
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
        for (ulong Page = 0; Page < Codeˉpages; Page++)
        {
            Writeˉuserˉentry(Tables, Regionˉaddress,
                Userˉcodeˉaddress + Page * Kernelˉpagingˉcontract.PAGE_BYTES,
                Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER);
        }
        for (ulong Page = 0; Page < Stackˉpages; Page++)
        {
            Writeˉuserˉentry(Tables, Regionˉaddress,
                Userˉstackˉaddress + Page * Kernelˉpagingˉcontract.PAGE_BYTES,
                Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉpagingˉcontract.ENTRY_WRITABLE |
                Kernelˉprocessˉcontract.ENTRY_USER | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        }
        Writeˉuserˉentry(Tables, Regionˉaddress, Userˉdataˉaddress,
            Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉpagingˉcontract.ENTRY_WRITABLE |
            Kernelˉprocessˉcontract.ENTRY_USER | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        if (Isˉinit)
        {
            Writeˉuserˉentry(Tables, Regionˉaddress, Userˉruntimeˉinputˉaddress,
                Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER |
                Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        }
        else
        {
            Writeˉentry(
                Tables,
                Kernelˉprocessˉcontract.USER_PT_PAGE,
                checked((int)((Userˉruntimeˉinputˉaddress - Regionˉaddress) /
                    Kernelˉpagingˉcontract.PAGE_BYTES)),
                0);
        }

        var Code = new byte[checked((int)(Codeˉpages * Kernelˉpagingˉcontract.PAGE_BYTES))];
        userˉimage.CopyTo(Code);
        var Stack = new byte[checked((int)(Stackˉpages * Kernelˉpagingˉcontract.PAGE_BYTES))];
        var Data = new byte[Kernelˉpagingˉcontract.PAGE_BYTES];
        BinaryPrimitives.WriteUInt32LittleEndian(Data, Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Data.AsSpan(4), Nativeˉexecutionˉcontextˉcontract.SIZE);
        BinaryPrimitives.WriteUInt64LittleEndian(Data.AsSpan(8), Instructionˉbudget);
        BinaryPrimitives.WriteUInt64LittleEndian(Data.AsSpan(16), Callˉdepthˉbudget);
        var Runtimeˉinputˉbytes = Isˉinit
            ? new byte[Kernelˉpagingˉcontract.PAGE_BYTES]
            : [];
        if (Isˉinit)
        {
            runtimeˉinput.CopyTo(Runtimeˉinputˉbytes);
        }

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
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(96), Memoryˉpageˉbudget);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(100), Instructionˉbudget);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(104), Kernelˉprocessˉcontract.HANDLE_BUDGET);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.SYSCALL_BUDGET_OFFSET), Syscallˉbudget);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(112), Kernelˉprocessˉcontract.CAPABILITY_SLOT);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(116), Kernelˉprocessˉcontract.CAPABILITY_GENERATION);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(120), definition.Capabilityˉrights);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(124), Kernelˉprocessˉcontract.CHANNEL_CAPACITY);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.CHANNEL_ADDRESS_OFFSET), definition.Channelˉaddress);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.ROLE_OFFSET), definition.Role);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.STACK_PAGE_COUNT_OFFSET), checked((uint)Stackˉpages));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.RUNTIME_PROFILE_OFFSET), Runtimeˉprofile);
        programˉdigest.CopyTo(Record.AsSpan((int)Kernelˉprocessˉcontract.PROGRAM_DIGEST_OFFSET,
            Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.CODE_PAGE_COUNT_OFFSET), checked((uint)Codeˉpages));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.RUNTIME_KIND_OFFSET), Runtimeˉkind);

        return new(new(
            allocationˉaddress,
            Userˉcodeˉaddress,
            Userˉstackˉaddress,
            Userˉdataˉaddress,
            Codeˉpages,
            Stackˉpages,
            Tables.ToImmutableArray(),
            Code.ToImmutableArray(),
            Stack.ToImmutableArray(),
            Data.ToImmutableArray(),
            Userˉruntimeˉinputˉaddress,
            Runtimeˉinputˉbytes.ToImmutableArray(),
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

public static class Kernelˉresourceˉgrantˉplanner
{
    public static Kernelˉresourceˉgrantˉresult Plan(
        Kernelˉprocessˉplan owner,
        Kernelˉprocessˉplan client,
        ReadOnlySpan<byte> programˉdigest,
        int resourceˉlength,
        uint serviceˉoffset)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(client);
        if (programˉdigest.Length != Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES ||
            resourceˉlength is < 12 or > Kernelˉprocessˉcontract.MAXIMUM_RUNTIME_INPUT_BYTES ||
            owner.Userˉruntimeˉinputˉbytes.Length != (int)Kernelˉpagingˉcontract.PAGE_BYTES ||
            !client.Userˉruntimeˉinputˉbytes.IsEmpty ||
            !SHA256.HashData(owner.Userˉruntimeˉinputˉbytes.AsSpan()[..resourceˉlength])
                .AsSpan().SequenceEqual(programˉdigest) ||
            !owner.Userˉruntimeˉinputˉbytes.AsSpan()[resourceˉlength..].IsEmpty &&
                owner.Userˉruntimeˉinputˉbytes.AsSpan()[resourceˉlength..].ContainsAnyExcept((byte)0))
        {
            return Fail("WVOS6101", "The immutable boot-resource owner, bytes, or digest is invalid.");
        }
        if (!Hasˉrecord(owner.Processˉrecord, Kernelˉprocessˉcontract.ROLE_INIT_SERVICE,
                Kernelˉprocessˉcontract.RUNTIME_PROFILE_BOOT_RESOURCE_OWNER, programˉdigest) ||
            !Hasˉrecord(client.Processˉrecord, Kernelˉprocessˉcontract.ROLE_BYTECODE_INTERPRETER,
                Kernelˉprocessˉcontract.RUNTIME_PROFILE_GRANTED_BOOT_RESOURCE_INTERPRETER,
                programˉdigest) ||
            owner.Userˉruntimeˉinputˉaddress == 0 || client.Userˉruntimeˉinputˉaddress == 0 ||
            owner.Userˉruntimeˉinputˉaddress == client.Userˉruntimeˉinputˉaddress)
        {
            return Fail("WVOS6101", "The resource owner and borrower records are inconsistent.");
        }

        var Ownerˉentry = Kernelˉprocessˉplanner.Readˉentry(
            owner,
            Kernelˉprocessˉcontract.USER_PT_PAGE,
            Entryˉindex(owner.Userˉruntimeˉinputˉaddress));
        var Expectedˉownerˉentry = owner.Userˉruntimeˉinputˉaddress |
            Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER |
            Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE;
        var Clientˉentryˉindex = Entryˉindex(client.Userˉruntimeˉinputˉaddress);
        if (Ownerˉentry != Expectedˉownerˉentry ||
            Kernelˉprocessˉplanner.Readˉentry(
                client, Kernelˉprocessˉcontract.USER_PT_PAGE, Clientˉentryˉindex) != 0 ||
            client.Userˉdataˉbytes.Length != (int)Kernelˉpagingˉcontract.PAGE_BYTES ||
            BinaryPrimitives.ReadUInt64LittleEndian(client.Userˉdataˉbytes.AsSpan()[
                Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET..]) != 0 ||
            BinaryPrimitives.ReadUInt64LittleEndian(client.Userˉdataˉbytes.AsSpan()[
                Nativeˉexecutionˉcontextˉcontract.FILE_INPUT_TABLE_POINTER_OFFSET..]) != 0)
        {
            return Fail("WVOS6102", "The resource is not exclusively mapped by init before the grant.");
        }
        if (serviceˉoffset > (uint)client.Userˉcodeˉbytes.Length ||
            Kernelˉprocessˉcontract.BOOT_RESOURCE_SERVICE_BYTES >
                (uint)client.Userˉcodeˉbytes.Length - serviceˉoffset)
        {
            return Fail("WVOS6103", "The granted resource service leaf is outside the client image.");
        }

        var Tables = client.Tableˉbytes.ToArray();
        var Entryˉoffset = checked((int)(Kernelˉprocessˉcontract.USER_PT_PAGE *
            Kernelˉpagingˉcontract.PAGE_BYTES) + Clientˉentryˉindex * sizeof(ulong));
        BinaryPrimitives.WriteUInt64LittleEndian(
            Tables.AsSpan(Entryˉoffset),
            owner.Userˉruntimeˉinputˉaddress | Kernelˉpagingˉcontract.ENTRY_PRESENT |
                Kernelˉprocessˉcontract.ENTRY_USER | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);

        var Data = client.Userˉdataˉbytes.ToArray();
        var Serviceˉtableˉaddress = client.Userˉdataˉaddress +
            Kernelˉprocessˉcontract.RUNTIME_SERVICE_TABLE_OFFSET;
        var Resourceˉtableˉaddress = client.Userˉdataˉaddress +
            Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET;
        var Serviceˉaddress = client.Userˉcodeˉaddress + serviceˉoffset;
        BinaryPrimitives.WriteUInt64LittleEndian(
            Data.AsSpan(Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET),
            Serviceˉtableˉaddress);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Data.AsSpan(Nativeˉexecutionˉcontextˉcontract.FILE_INPUT_TABLE_POINTER_OFFSET),
            Resourceˉtableˉaddress);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Data.AsSpan((int)Kernelˉprocessˉcontract.RUNTIME_SERVICE_TABLE_OFFSET),
            Nativeˉserviceˉtableˉcontract.FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Data.AsSpan((int)Kernelˉprocessˉcontract.RUNTIME_SERVICE_TABLE_OFFSET + 4),
            Nativeˉserviceˉtableˉcontract.SIZE);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Data.AsSpan((int)Kernelˉprocessˉcontract.RUNTIME_SERVICE_TABLE_OFFSET +
                Nativeˉserviceˉtableˉcontract.FILE_READ_BYTES_POINTER_OFFSET),
            Serviceˉaddress);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Data.AsSpan((int)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET),
            Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Data.AsSpan((int)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET + 4),
            Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Data.AsSpan((int)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET + 8),
            Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_BYTES);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Data.AsSpan((int)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET +
                (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_DATA_POINTER_OFFSET),
            client.Userˉruntimeˉinputˉaddress);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Data.AsSpan((int)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET +
                (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_DATA_LENGTH_OFFSET),
            checked((uint)resourceˉlength));

        var Record = new byte[Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES];
        BinaryPrimitives.WriteUInt64LittleEndian(Record, Kernelˉprocessˉcontract.RESOURCE_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(8), Kernelˉprocessˉcontract.RESOURCE_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Record.AsSpan(12), Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET),
            Kernelˉprocessˉcontract.RESOURCE_STATE_BORROWED);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.RESOURCE_ID_OFFSET),
            Kernelˉprocessˉcontract.RESOURCE_ID);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.RESOURCE_OWNER_OFFSET),
            Kernelˉprocessˉcontract.INIT_PROCESS_ID);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET),
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.RESOURCE_SOURCE_ADDRESS_OFFSET),
            owner.Userˉruntimeˉinputˉaddress);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET),
            checked((uint)resourceˉlength));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.RESOURCE_FLAGS_OFFSET),
            Kernelˉprocessˉcontract.RESOURCE_FLAGS);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.RESOURCE_TARGET_ROOT_OFFSET),
            client.Rootˉaddress);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.RESOURCE_TARGET_DATA_OFFSET),
            client.Userˉdataˉaddress);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.RESOURCE_TARGET_ADDRESS_OFFSET),
            client.Userˉruntimeˉinputˉaddress);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.RESOURCE_SERVICE_ADDRESS_OFFSET),
            Serviceˉaddress);
        programˉdigest.CopyTo(Record.AsSpan((int)Kernelˉprocessˉcontract.RESOURCE_DIGEST_OFFSET));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET), 1);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.RESOURCE_TARGET_PTE_OFFSET),
            checked(client.Rootˉaddress + Kernelˉprocessˉcontract.USER_PT_PAGE *
                Kernelˉpagingˉcontract.PAGE_BYTES + (ulong)Clientˉentryˉindex * sizeof(ulong)));
        return new(new(Record.ToImmutableArray(), Tables.ToImmutableArray(), Data.ToImmutableArray()), []);
    }

    private static bool Hasˉrecord(
        ImmutableArray<byte> record,
        uint role,
        uint profile,
        ReadOnlySpan<byte> digest) =>
        record.Length == Kernelˉprocessˉcontract.RECORD_BYTES &&
        BinaryPrimitives.ReadUInt64LittleEndian(record.AsSpan()) == Kernelˉprocessˉcontract.RECORD_MAGIC &&
        BinaryPrimitives.ReadUInt32LittleEndian(record.AsSpan()[8..]) ==
            Kernelˉprocessˉcontract.RECORD_VERSION &&
        BinaryPrimitives.ReadUInt32LittleEndian(record.AsSpan()[
            (int)Kernelˉprocessˉcontract.ROLE_OFFSET..]) == role &&
        BinaryPrimitives.ReadUInt32LittleEndian(record.AsSpan()[
            (int)Kernelˉprocessˉcontract.RUNTIME_PROFILE_OFFSET..]) == profile &&
        record.AsSpan((int)Kernelˉprocessˉcontract.PROGRAM_DIGEST_OFFSET,
            Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES).SequenceEqual(digest);

    private static int Entryˉindex(ulong address) => checked((int)(
        (address & (Kernelˉpagingˉcontract.LARGE_PAGE_BYTES - 1)) /
        Kernelˉpagingˉcontract.PAGE_BYTES));

    private static Kernelˉresourceˉgrantˉresult Fail(string code, string message) =>
        new(null, [new(code, message)]);
}
