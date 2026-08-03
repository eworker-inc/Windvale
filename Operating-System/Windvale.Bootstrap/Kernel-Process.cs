using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;

namespace Windvale.Bootstrap;

public static class Kernelˉprocessˉcontract
{
    public const int FORMAT_VERSION = 14;
    public const string TARGET_NAME = "x86-64-kernel-process-v14";
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
    public const int POLICY_TOKEN = 97;
    public const int EXPECTED_RESULT = 6;
    public const string USER_FAULT_CONTAINED_MARKER = "user-fault=contained\n";
    public const uint INIT_PROCESS_ID = 1;
    public const uint INIT_THREAD_ID = 1;
    public const uint INIT_PROCESS_GENERATION = 1;
    public const uint CLIENT_PROCESS_ID = 2;
    public const uint CLIENT_THREAD_ID = 2;
    public const uint FIRST_CLIENT_GENERATION = 1;
    public const uint SECOND_CLIENT_GENERATION = 2;
    public const uint INIT_PROCESS_REFERENCE =
        (INIT_PROCESS_GENERATION << 16) | INIT_PROCESS_ID;
    public const uint FIRST_CLIENT_PROCESS_REFERENCE =
        (FIRST_CLIENT_GENERATION << 16) | CLIENT_PROCESS_ID;
    public const uint SECOND_CLIENT_PROCESS_REFERENCE =
        (SECOND_CLIENT_GENERATION << 16) | CLIENT_PROCESS_ID;
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
    public const uint INIT_MEMORY_PAGE_BUDGET = 9;
    public const uint CLIENT_MEMORY_PAGE_BUDGET = 120;
    public const uint INIT_INSTRUCTION_BUDGET = 64;
    public const uint CLIENT_INSTRUCTION_BUDGET = 189_114;
    public const uint INIT_CALL_DEPTH_BUDGET = 1;
    public const uint CLIENT_CALL_DEPTH_BUDGET = 5;
    public const int CLIENT_INTERPRETER_FRAME_SLOTS = 755;
    public const ulong CLIENT_NATIVE_STACK_USED_BYTES = 24_240;
    public const uint HANDLE_BUDGET = 1;
    public const uint INIT_SYSCALL_BUDGET = 11;
    public const uint CLIENT_SYSCALL_BUDGET = 4;
    public const uint CHANNEL_CAPACITY = 1;
    public const uint CAPABILITY_SLOT = 0;
    public const uint CAPABILITY_GENERATION = 1;
    public const uint CAPABILITY_RIGHT_SEND = 1U << 0;
    public const uint CAPABILITY_RIGHT_RECEIVE = 1U << 1;
    public const uint CAPABILITY_RIGHT_GRANT_BOOT_RESOURCE = 1U << 2;
    public const uint CAPABILITY_RIGHT_RECEIVE_SERVICE_REQUEST = 1U << 3;
    public const uint CAPABILITY_RIGHT_CALL_SERVICE = 1U << 4;
    public const uint CAPABILITY_RIGHT_REPLY_SERVICE_REQUEST = 1U << 5;
    public const uint INIT_CAPABILITY_RIGHTS =
        CAPABILITY_RIGHT_RECEIVE | CAPABILITY_RIGHT_GRANT_BOOT_RESOURCE |
        CAPABILITY_RIGHT_RECEIVE_SERVICE_REQUEST | CAPABILITY_RIGHT_REPLY_SERVICE_REQUEST;
    public const uint CLIENT_CAPABILITY_RIGHTS =
        CAPABILITY_RIGHT_SEND | CAPABILITY_RIGHT_CALL_SERVICE;
    public const uint CAPABILITY_REFERENCE = (CAPABILITY_GENERATION << 16) | CAPABILITY_SLOT;
    public const uint SYSCALL_SEND = 1;
    public const uint SYSCALL_RECEIVE = 2;
    public const uint SYSCALL_EXIT = 3;
    public const uint SYSCALL_GRANT_BOOT_RESOURCE = 4;
    public const uint SYSCALL_RECEIVE_SERVICE_REQUEST = 5;
    public const uint SYSCALL_CALL_SERVICE = 6;
    public const uint SYSCALL_REPLY_SERVICE_REQUEST = 7;
    public const uint RESOURCE_SET_TOKEN = 0x0002_0001;
    public const ulong INIT_ALLOCATION_PAGES = 13;
    public const ulong CLIENT_ALLOCATION_PAGES = 122;
    public const ulong TABLE_PAGES = 4;
    public const ulong TABLE_BYTES = TABLE_PAGES * Kernelˉpagingˉcontract.PAGE_BYTES;
    public const ulong PML4_PAGE = 0;
    public const ulong PDPT_PAGE = 1;
    public const ulong PD_PAGE = 2;
    public const ulong USER_PT_PAGE = 3;
    public const ulong USER_CODE_PAGE = 4;
    public const ulong INIT_CODE_PAGES = 2;
    public const ulong CLIENT_CODE_PAGES = 110;
    public const ulong INIT_STACK_PAGES = 1;
    public const ulong CLIENT_STACK_PAGES = 6;
    public const ulong CLIENT_STACK_BYTES = CLIENT_STACK_PAGES * Kernelˉpagingˉcontract.PAGE_BYTES;
    public const ulong INIT_STACK_PAGE = USER_CODE_PAGE + INIT_CODE_PAGES;
    public const ulong INIT_DATA_PAGE = INIT_STACK_PAGE + INIT_STACK_PAGES;
    public const ulong INIT_RUNTIME_INPUT_PAGE = INIT_DATA_PAGE + 1;
    public const ulong INIT_RUNTIME_BUDGET_PAGE = INIT_RUNTIME_INPUT_PAGE + 1;
    public const ulong INIT_RESOURCE_STORE_PAGE = INIT_RUNTIME_BUDGET_PAGE + 1;
    public const ulong INIT_DIRECTORY_SNAPSHOT_PAGE = INIT_RESOURCE_STORE_PAGE + 1;
    public const ulong INIT_SERVICE_RESPONSE_PAGE = INIT_DIRECTORY_SNAPSHOT_PAGE + 1;
    public const ulong CLIENT_STACK_PAGE = USER_CODE_PAGE + CLIENT_CODE_PAGES;
    public const ulong CLIENT_DATA_PAGE = CLIENT_STACK_PAGE + CLIENT_STACK_PAGES;
    public const ulong CLIENT_SERVICE_RESPONSE_PAGE = CLIENT_DATA_PAGE + 1;
    public const ulong CLIENT_RUNTIME_INPUT_PAGE = CLIENT_SERVICE_RESPONSE_PAGE + 1;
    public const ulong CLIENT_RUNTIME_BUDGET_PAGE = CLIENT_RUNTIME_INPUT_PAGE + 1;
    public const ulong CLIENT_CODE_BYTES = CLIENT_CODE_PAGES * Kernelˉpagingˉcontract.PAGE_BYTES;
    public const int MAXIMUM_RUNTIME_INPUT_BYTES = 4_096;
    public const uint BOOT_RESOURCE_SERVICE_BYTES = 347;
    public const uint RUNTIME_SERVICE_TABLE_OFFSET = 128;
    public const uint BOOT_RESOURCE_TABLE_OFFSET = 256;
    public const uint BOOT_RESOURCE_TABLE_MAGIC = 0x5242_5657;
    public const uint BOOT_RESOURCE_TABLE_VERSION = 2;
    public const uint BOOT_RESOURCE_TABLE_BYTES = 80;
    public const uint BOOT_RESOURCE_COUNT_OFFSET = 12;
    public const uint BOOT_RESOURCE_ENTRY_BYTES = 32;
    public const uint BOOT_RESOURCE_FIRST_ENTRY_OFFSET = 16;
    public const uint BOOT_RESOURCE_SECOND_ENTRY_OFFSET = 48;
    public const uint BOOT_RESOURCE_ENTRY_ID_OFFSET = 0;
    public const uint BOOT_RESOURCE_ENTRY_KIND_OFFSET = 4;
    public const uint BOOT_RESOURCE_DATA_POINTER_OFFSET = 8;
    public const uint BOOT_RESOURCE_DATA_LENGTH_OFFSET = 16;
    public const uint BOOT_RESOURCE_ENTRY_FLAGS_OFFSET = 20;
    public const uint BOOT_RESOURCE_RESERVED_OFFSET = 24;
    public const uint CLIENT_RECORD_ARENA_OFFSET = 512;
    public const uint CLIENT_RECORD_ARENA_BYTES = 1_024;
    public const uint CLIENT_RECORD_ARENA_USED_BYTES = 0;
    public const ulong ENTRY_USER = 1UL << 2;
    public const uint INIT_RECORD_OFFSET = 256;
    public const uint CLIENT_RECORD_OFFSET = 768;
    public const uint CHANNEL_RECORD_OFFSET = 1_040;
    public const uint RECORD_BYTES = 272;
    public const ulong RECORD_MAGIC = 0x3431_434F_5250_5657;
    public const uint RECORD_VERSION = 14;
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
    public const uint PROCESS_GENERATION_OFFSET = 256;
    public const uint USER_SERVICE_RESPONSE_ADDRESS_OFFSET = 264;
    public const uint RUNTIME_PROFILE_RESOURCE_DIRECTORY_OWNER = 2;
    public const uint RUNTIME_PROFILE_GRANTED_RESOURCE_DIRECTORY_INTERPRETER = 7;
    public const uint WAIT_REASON_NONE = 0;
    public const uint WAIT_REASON_CHANNEL_RECEIVE = 1;
    public const uint WAIT_REASON_SERVICE_REQUEST = 2;
    public const uint WAIT_REASON_SERVICE_REPLY = 3;
    public const ulong CHANNEL_MAGIC = 0x3330_4E41_4843_5657;
    public const uint CHANNEL_VERSION = 3;
    public const uint CHANNEL_RECORD_BYTES = 112;
    public const uint CHANNEL_STATE_OFFSET = 16;
    public const uint CHANNEL_MESSAGE_OFFSET = 20;
    public const uint CHANNEL_SENDER_OFFSET = 24;
    public const uint CHANNEL_RECEIVER_OFFSET = 28;
    public const uint CHANNEL_SEND_COUNT_OFFSET = 32;
    public const uint CHANNEL_RECEIVE_COUNT_OFFSET = 36;
    public const uint CHANNEL_WAITER_OFFSET = 40;
    public const uint CHANNEL_WAKE_COUNT_OFFSET = 44;
    public const uint CHANNEL_CAPACITY_OFFSET = 48;
    public const uint CHANNEL_REQUEST_COUNT_OFFSET = 52;
    public const uint CHANNEL_REPLY_COUNT_OFFSET = 56;
    public const uint CHANNEL_BYTE_LENGTH_OFFSET = 60;
    public const uint CHANNEL_SERVICE_DESTINATION_OFFSET = 64;
    public const uint CHANNEL_SERVICE_CAPACITY_OFFSET = 72;
    public const uint CHANNEL_CLIENT_DESTINATION_OFFSET = 80;
    public const uint CHANNEL_CLIENT_CAPACITY_OFFSET = 88;
    public const uint CHANNEL_PEER_STATUS_OFFSET = 96;
    public const uint CHANNEL_PEER_PROCESS_OFFSET = 100;
    public const uint CHANNEL_CLOSE_COUNT_OFFSET = 104;
    public const uint CHANNEL_RESERVED_OFFSET = 108;
    public const uint CHANNEL_STATE_REQUEST_DELIVERED = 2;
    public const uint CHANNEL_PEER_STATUS_OPEN = 0;
    public const uint CHANNEL_PEER_STATUS_EXITED = 1;
    public const uint CHANNEL_PEER_STATUS_FAULTED = 2;
    public const uint MAXIMUM_CHANNEL_MESSAGE_BYTES = 4_096;
    public const uint RESOURCE_CONFIGURATION_REQUEST_BYTES = 55;
    public const uint RESOURCE_CONFIGURATION_REPLY_BYTES = 116;
    public const uint DIRECTORY_READ_REQUEST_BYTES = 37;
    public const uint DIRECTORY_READ_REPLY_BYTES = 3_096;
    public const uint RESOURCE_RECORD_OFFSET = CHANNEL_RECORD_OFFSET + CHANNEL_RECORD_BYTES;
    public const uint SECOND_RESOURCE_RECORD_OFFSET = RESOURCE_RECORD_OFFSET + RESOURCE_RECORD_BYTES;
    public const uint STORE_RESOURCE_RECORD_OFFSET = SECOND_RESOURCE_RECORD_OFFSET + RESOURCE_RECORD_BYTES;
    public const uint DIRECTORY_RESOURCE_RECORD_OFFSET = STORE_RESOURCE_RECORD_OFFSET + RESOURCE_RECORD_BYTES;
    public const uint RESOURCE_COUNT = 2;
    public const uint RESOURCE_RECORD_COUNT = 4;
    public const uint RESOURCE_RECORD_BYTES = 128;
    public const uint RESOURCE_RECORD_SET_BYTES = RESOURCE_COUNT * RESOURCE_RECORD_BYTES;
    public const uint RESOURCE_STATE_BYTES = RESOURCE_RECORD_COUNT * RESOURCE_RECORD_BYTES;
    public const ulong RESOURCE_MAGIC = 0x3630_3053_4552_5657;
    public const uint RESOURCE_VERSION = 6;
    public const uint RESOURCE_STATE_OWNED = 1;
    public const uint RESOURCE_STATE_BORROWED = 2;
    public const uint RESOURCE_STATE_ATTACHED = 3;
    public const uint MODULE_RESOURCE_ID = 1;
    public const uint BUDGET_RESOURCE_ID = 2;
    public const uint STORE_RESOURCE_ID = 4;
    public const uint DIRECTORY_RESOURCE_ID = 5;
    public const uint RESOURCE_KIND_WVB_MODULE = 1;
    public const uint RESOURCE_KIND_U32_EXECUTION_BUDGET = 2;
    public const uint RESOURCE_KIND_WVRS_STORE = 3;
    public const uint RESOURCE_KIND_WVDS_SNAPSHOT = 4;
    public const uint EXECUTION_BUDGET = 199;
    public const uint MAXIMUM_EXECUTION_BUDGET = 256;
    public const uint EXECUTION_BUDGET_BYTES = sizeof(uint);
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
    public const uint RESOURCE_BASE_FLAGS =
        RESOURCE_FLAG_IMMUTABLE | RESOURCE_FLAG_READ_ONLY | RESOURCE_FLAG_NO_EXECUTE;
    public const uint RESOURCE_KIND_SHIFT = 8;
    public const uint RESOURCE_KIND_MASK = 0x0000_FF00;
    public const uint MODULE_RESOURCE_ATTRIBUTES =
        RESOURCE_BASE_FLAGS | (RESOURCE_KIND_WVB_MODULE << (int)RESOURCE_KIND_SHIFT);
    public const uint BUDGET_RESOURCE_ATTRIBUTES =
        RESOURCE_BASE_FLAGS | (RESOURCE_KIND_U32_EXECUTION_BUDGET << (int)RESOURCE_KIND_SHIFT);
    public const uint STORE_RESOURCE_ATTRIBUTES =
        RESOURCE_BASE_FLAGS | (RESOURCE_KIND_WVRS_STORE << (int)RESOURCE_KIND_SHIFT);
    public const uint DIRECTORY_RESOURCE_ATTRIBUTES =
        RESOURCE_BASE_FLAGS | (RESOURCE_KIND_WVDS_SNAPSHOT << (int)RESOURCE_KIND_SHIFT);
    public const uint INIT_STORE_DESCRIPTOR_OFFSET = 384;
    public const uint INIT_DIRECTORY_DESCRIPTOR_OFFSET = 416;
    public const uint INIT_DIRECTORY_DESCRIPTOR_GENERATION = 1;
    public const uint INIT_REQUEST_BUFFER_OFFSET = 1_024;
    public const uint INIT_REQUEST_BUFFER_BYTES = 1_056;
    public const uint INIT_RESPONSE_BUFFER_OFFSET = 2_080;
    public const uint INIT_RESPONSE_BUFFER_BYTES = 2_016;
    public const uint GDT_OFFSET = 528;
    public const uint GDT_BYTES = 56;
    public const uint GDTR_OFFSET = 592;
    public const uint TSS_OFFSET = 608;
    public const uint TSS_BYTES = 104;
}

public sealed record Kernelˉprocessˉdefinition(
    uint Processˉid,
    uint Threadˉid,
    uint Processˉgeneration,
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
    ulong Userˉruntimeˉbudgetˉaddress,
    ImmutableArray<byte> Userˉruntimeˉbudgetˉbytes,
    ulong Userˉresourceˉstoreˉaddress,
    ImmutableArray<byte> Userˉresourceˉstoreˉbytes,
    ulong Userˉdirectoryˉsnapshotˉaddress,
    ImmutableArray<byte> Userˉdirectoryˉsnapshotˉbytes,
    ulong Userˉserviceˉresponseˉaddress,
    ImmutableArray<byte> Userˉserviceˉresponseˉbytes,
    ImmutableArray<byte> Processˉrecord);

public sealed record Kernelˉprocessˉplanˉresult(
    Kernelˉprocessˉplan? Plan,
    ImmutableArray<Kernelˉprocessˉdiagnostic> Diagnostics)
{
    public bool Success => Plan is not null && Diagnostics.IsEmpty;
}

public sealed record Kernelˉresourceˉgrantˉplan(
    ImmutableArray<byte> Resourceˉrecords,
    ImmutableArray<byte> Clientˉtableˉbytes,
    ImmutableArray<byte> Clientˉdataˉbytes);

public sealed record Kernelˉresourceˉgrantˉresult(
    Kernelˉresourceˉgrantˉplan? Plan,
    ImmutableArray<Kernelˉprocessˉdiagnostic> Diagnostics)
{
    public bool Success => Plan is not null && Diagnostics.IsEmpty;
}

public sealed record Kernelˉresourceˉrevocationˉplan(
    ImmutableArray<byte> Resourceˉrecords,
    ImmutableArray<byte> Clientˉtableˉbytes,
    ImmutableArray<byte> Clientˉdataˉbytes);

public sealed record Kernelˉresourceˉrevocationˉresult(
    Kernelˉresourceˉrevocationˉplan? Plan,
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
        ReadOnlySpan<byte> runtimeˉbudget,
        ReadOnlySpan<byte> resourceˉstore,
        ReadOnlySpan<byte> directoryˉsnapshot,
        uint bootˉresourceˉserviceˉoffset,
        Kernelˉprocessˉdefinition definition)
    {
        ArgumentNullException.ThrowIfNull(kernelˉpaging);
        ArgumentNullException.ThrowIfNull(definition);
        var Isˉinit = definition is
        {
            Processˉid: Kernelˉprocessˉcontract.INIT_PROCESS_ID,
            Threadˉid: Kernelˉprocessˉcontract.INIT_THREAD_ID,
            Processˉgeneration: Kernelˉprocessˉcontract.INIT_PROCESS_GENERATION,
            Role: Kernelˉprocessˉcontract.ROLE_INIT_SERVICE,
            Capabilityˉrights: Kernelˉprocessˉcontract.INIT_CAPABILITY_RIGHTS,
        };
        var Isˉclient =
            definition.Processˉid == Kernelˉprocessˉcontract.CLIENT_PROCESS_ID &&
            definition.Threadˉid == Kernelˉprocessˉcontract.CLIENT_THREAD_ID &&
            definition.Processˉgeneration is
                Kernelˉprocessˉcontract.FIRST_CLIENT_GENERATION or
                Kernelˉprocessˉcontract.SECOND_CLIENT_GENERATION &&
            definition.Role == Kernelˉprocessˉcontract.ROLE_BYTECODE_INTERPRETER &&
            definition.Capabilityˉrights == Kernelˉprocessˉcontract.CLIENT_CAPABILITY_RIGHTS;
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
            ? Kernelˉprocessˉcontract.RUNTIME_PROFILE_RESOURCE_DIRECTORY_OWNER
            : Kernelˉprocessˉcontract.RUNTIME_PROFILE_GRANTED_RESOURCE_DIRECTORY_INTERPRETER;
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
                 runtimeˉbudget.Length != Kernelˉprocessˉcontract.EXECUTION_BUDGET_BYTES ||
                 BinaryPrimitives.ReadUInt32LittleEndian(runtimeˉbudget) !=
                    Kernelˉprocessˉcontract.EXECUTION_BUDGET ||
                  resourceˉstore.Length is < Resourceˉstoreˉcontract.HEADER_BYTES or >
                    (int)Kernelˉpagingˉcontract.PAGE_BYTES ||
                  directoryˉsnapshot.Length is < 68 or >
                    (int)Kernelˉpagingˉcontract.PAGE_BYTES ||
                  bootˉresourceˉserviceˉoffset != 0 ||
                  !SHA256.HashData(runtimeˉinput).AsSpan().SequenceEqual(programˉdigest))) ||
            (Isˉclient &&
                (!runtimeˉinput.IsEmpty || !runtimeˉbudget.IsEmpty || !resourceˉstore.IsEmpty ||
                 !directoryˉsnapshot.IsEmpty ||
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
        var Userˉruntimeˉbudgetˉaddress = Pageˉaddress(
            allocationˉaddress,
            Isˉinit
                ? Kernelˉprocessˉcontract.INIT_RUNTIME_BUDGET_PAGE
                : Kernelˉprocessˉcontract.CLIENT_RUNTIME_BUDGET_PAGE);
        var Userˉresourceˉstoreˉaddress = Isˉinit
            ? Pageˉaddress(allocationˉaddress, Kernelˉprocessˉcontract.INIT_RESOURCE_STORE_PAGE)
            : 0;
        var Userˉdirectoryˉsnapshotˉaddress = Isˉinit
            ? Pageˉaddress(allocationˉaddress, Kernelˉprocessˉcontract.INIT_DIRECTORY_SNAPSHOT_PAGE)
            : 0;
        var Userˉserviceˉresponseˉaddress = Pageˉaddress(
            allocationˉaddress,
            Isˉinit
                ? Kernelˉprocessˉcontract.INIT_SERVICE_RESPONSE_PAGE
                : Kernelˉprocessˉcontract.CLIENT_SERVICE_RESPONSE_PAGE);
        var Executableˉend = kernelˉpaging.Executableˉaddress + Kernelˉpagingˉcontract.EXECUTABLE_BYTES;
        if (allocationˉaddress < Executableˉend && Allocationˉend > kernelˉpaging.Executableˉaddress)
        {
            return Fail("WVOS6005", "The process allocation overlaps the retained kernel executable window.");
        }
        if (Isˉinit)
        {
            try
            {
                _ = Resourceˉstoreˉverifier.Verify(resourceˉstore);
            }
            catch (Resourceˉstoreˉexception)
            {
                return Fail("WVOS6008", "The init resource-store capability is malformed.");
            }
            try
            {
                _ = Directoryˉsnapshotˉcodec.Verify(directoryˉsnapshot);
            }
            catch (Directoryˉsnapshotˉexception)
            {
                return Fail("WVOS6008", "The init directory-snapshot capability is malformed.");
            }
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
        Writeˉuserˉentry(Tables, Regionˉaddress, Userˉserviceˉresponseˉaddress,
            Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉpagingˉcontract.ENTRY_WRITABLE |
            Kernelˉprocessˉcontract.ENTRY_USER | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        if (Isˉinit)
        {
            Writeˉuserˉentry(Tables, Regionˉaddress, Userˉruntimeˉinputˉaddress,
                Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER |
                Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
            Writeˉuserˉentry(Tables, Regionˉaddress, Userˉruntimeˉbudgetˉaddress,
                Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER |
                Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
            Writeˉuserˉentry(Tables, Regionˉaddress, Userˉresourceˉstoreˉaddress,
                Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER |
                Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
            Writeˉuserˉentry(Tables, Regionˉaddress, Userˉdirectoryˉsnapshotˉaddress,
                Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER |
                Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        }
        else
        {
            foreach (var Address in new[] { Userˉruntimeˉinputˉaddress, Userˉruntimeˉbudgetˉaddress })
            {
                Writeˉentry(
                    Tables,
                    Kernelˉprocessˉcontract.USER_PT_PAGE,
                    checked((int)((Address - Regionˉaddress) /
                        Kernelˉpagingˉcontract.PAGE_BYTES)),
                    0);
            }
        }

        var Code = new byte[checked((int)(Codeˉpages * Kernelˉpagingˉcontract.PAGE_BYTES))];
        userˉimage.CopyTo(Code);
        var Stack = new byte[checked((int)(Stackˉpages * Kernelˉpagingˉcontract.PAGE_BYTES))];
        var Data = new byte[Kernelˉpagingˉcontract.PAGE_BYTES];
        BinaryPrimitives.WriteUInt32LittleEndian(Data, Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Data.AsSpan(4), Nativeˉexecutionˉcontextˉcontract.SIZE);
        BinaryPrimitives.WriteUInt64LittleEndian(Data.AsSpan(8), Instructionˉbudget);
        BinaryPrimitives.WriteUInt64LittleEndian(Data.AsSpan(16), Callˉdepthˉbudget);
        if (!Isˉinit)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(
                Data.AsSpan(Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_POINTER_OFFSET),
                Userˉdataˉaddress + Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_OFFSET);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Data.AsSpan(Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET),
                Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_BYTES);
        }
        var Runtimeˉinputˉbytes = Isˉinit
            ? new byte[Kernelˉpagingˉcontract.PAGE_BYTES]
            : [];
        var Runtimeˉbudgetˉbytes = Isˉinit
            ? new byte[Kernelˉpagingˉcontract.PAGE_BYTES]
            : [];
        var Resourceˉstoreˉbytes = Isˉinit
            ? new byte[Kernelˉpagingˉcontract.PAGE_BYTES]
            : [];
        var Directoryˉsnapshotˉbytes = Isˉinit
            ? new byte[Kernelˉpagingˉcontract.PAGE_BYTES]
            : [];
        var Serviceˉresponseˉbytes = new byte[Kernelˉpagingˉcontract.PAGE_BYTES];
        if (Isˉinit)
        {
            runtimeˉinput.CopyTo(Runtimeˉinputˉbytes);
            runtimeˉbudget.CopyTo(Runtimeˉbudgetˉbytes);
            resourceˉstore.CopyTo(Resourceˉstoreˉbytes);
            directoryˉsnapshot.CopyTo(Directoryˉsnapshotˉbytes);
            BinaryPrimitives.WriteUInt64LittleEndian(
                Data.AsSpan((int)Kernelˉprocessˉcontract.INIT_STORE_DESCRIPTOR_OFFSET),
                Userˉresourceˉstoreˉaddress);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Data.AsSpan((int)Kernelˉprocessˉcontract.INIT_STORE_DESCRIPTOR_OFFSET + sizeof(ulong)),
                checked((uint)resourceˉstore.Length));
            BinaryPrimitives.WriteUInt64LittleEndian(
                Data.AsSpan((int)Kernelˉprocessˉcontract.INIT_DIRECTORY_DESCRIPTOR_OFFSET),
                Userˉdirectoryˉsnapshotˉaddress);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Data.AsSpan((int)Kernelˉprocessˉcontract.INIT_DIRECTORY_DESCRIPTOR_OFFSET + sizeof(ulong)),
                checked((uint)directoryˉsnapshot.Length));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Data.AsSpan((int)Kernelˉprocessˉcontract.INIT_DIRECTORY_DESCRIPTOR_OFFSET + 12),
                Kernelˉprocessˉcontract.INIT_DIRECTORY_DESCRIPTOR_GENERATION);
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
        BinaryPrimitives.WriteUInt32LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.PROCESS_GENERATION_OFFSET),
            definition.Processˉgeneration);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.USER_SERVICE_RESPONSE_ADDRESS_OFFSET),
            Userˉserviceˉresponseˉaddress);

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
            Userˉruntimeˉbudgetˉaddress,
            Runtimeˉbudgetˉbytes.ToImmutableArray(),
            Userˉresourceˉstoreˉaddress,
            Resourceˉstoreˉbytes.ToImmutableArray(),
            Userˉdirectoryˉsnapshotˉaddress,
            Directoryˉsnapshotˉbytes.ToImmutableArray(),
            Userˉserviceˉresponseˉaddress,
            Serviceˉresponseˉbytes.ToImmutableArray(),
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
        ReadOnlySpan<byte> budgetˉdigest,
        int programˉlength,
        uint resourceˉsetˉtoken,
        uint serviceˉoffset) =>
        Plan(
            owner, client, programˉdigest, budgetˉdigest, programˉlength,
            resourceˉsetˉtoken, serviceˉoffset, []);

    public static Kernelˉresourceˉgrantˉresult Plan(
        Kernelˉprocessˉplan owner,
        Kernelˉprocessˉplan client,
        ReadOnlySpan<byte> programˉdigest,
        ReadOnlySpan<byte> budgetˉdigest,
        int programˉlength,
        uint resourceˉsetˉtoken,
        uint serviceˉoffset,
        ReadOnlySpan<byte> ownedˉresourceˉrecords)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(client);
        var Clientˉgeneration = Readˉprocessˉgeneration(client.Processˉrecord);
        var Isˉfirstˉgrant =
            Clientˉgeneration == Kernelˉprocessˉcontract.FIRST_CLIENT_GENERATION &&
            ownedˉresourceˉrecords.IsEmpty;
        var Isˉsecondˉgrant =
            Clientˉgeneration == Kernelˉprocessˉcontract.SECOND_CLIENT_GENERATION &&
            ownedˉresourceˉrecords.Length == Kernelˉprocessˉcontract.RESOURCE_RECORD_SET_BYTES;
        if (!Isˉfirstˉgrant && !Isˉsecondˉgrant)
        {
            return Fail("WVOS6105", "The resource grant does not match the borrower's bounded lifecycle generation.");
        }
        var Previousˉgrantˉcount = Clientˉgeneration - 1;
        var Borrowerˉreference =
            (Clientˉgeneration << 16) | Kernelˉprocessˉcontract.CLIENT_PROCESS_ID;
        if (resourceˉsetˉtoken != Kernelˉprocessˉcontract.RESOURCE_SET_TOKEN)
        {
            return Fail("WVOS6104", "The requested resource set is unknown, partial, duplicate, or out of order.");
        }
        if (programˉdigest.Length != Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES ||
            budgetˉdigest.Length != Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES ||
            programˉlength is < 12 or > Kernelˉprocessˉcontract.MAXIMUM_RUNTIME_INPUT_BYTES ||
            owner.Userˉruntimeˉinputˉbytes.Length != (int)Kernelˉpagingˉcontract.PAGE_BYTES ||
            owner.Userˉruntimeˉbudgetˉbytes.Length != (int)Kernelˉpagingˉcontract.PAGE_BYTES ||
            !client.Userˉruntimeˉinputˉbytes.IsEmpty || !client.Userˉruntimeˉbudgetˉbytes.IsEmpty ||
            !SHA256.HashData(owner.Userˉruntimeˉinputˉbytes.AsSpan()[..programˉlength])
                .AsSpan().SequenceEqual(programˉdigest) ||
            owner.Userˉruntimeˉinputˉbytes.AsSpan()[programˉlength..].ContainsAnyExcept((byte)0) ||
            BinaryPrimitives.ReadUInt32LittleEndian(owner.Userˉruntimeˉbudgetˉbytes.AsSpan()) !=
                Kernelˉprocessˉcontract.EXECUTION_BUDGET ||
            owner.Userˉruntimeˉbudgetˉbytes.AsSpan()[
                (int)Kernelˉprocessˉcontract.EXECUTION_BUDGET_BYTES..].ContainsAnyExcept((byte)0) ||
            !SHA256.HashData(owner.Userˉruntimeˉbudgetˉbytes.AsSpan()[
                ..(int)Kernelˉprocessˉcontract.EXECUTION_BUDGET_BYTES])
                .AsSpan().SequenceEqual(budgetˉdigest))
        {
            return Fail("WVOS6101", "The immutable typed-resource owner, bytes, or digest is invalid.");
        }
        if (!Hasˉrecord(
                owner.Processˉrecord,
                Kernelˉprocessˉcontract.INIT_PROCESS_ID,
                Kernelˉprocessˉcontract.INIT_PROCESS_GENERATION,
                Kernelˉprocessˉcontract.ROLE_INIT_SERVICE,
                Kernelˉprocessˉcontract.RUNTIME_PROFILE_RESOURCE_DIRECTORY_OWNER,
                programˉdigest) ||
            !Hasˉrecord(
                client.Processˉrecord,
                Kernelˉprocessˉcontract.CLIENT_PROCESS_ID,
                Clientˉgeneration,
                Kernelˉprocessˉcontract.ROLE_BYTECODE_INTERPRETER,
                Kernelˉprocessˉcontract.RUNTIME_PROFILE_GRANTED_RESOURCE_DIRECTORY_INTERPRETER,
                programˉdigest) ||
            new[]
            {
                owner.Userˉruntimeˉinputˉaddress,
                owner.Userˉruntimeˉbudgetˉaddress,
                client.Userˉruntimeˉinputˉaddress,
                client.Userˉruntimeˉbudgetˉaddress,
            }.Any(Address => Address == 0) ||
            new[]
            {
                owner.Userˉruntimeˉinputˉaddress,
                owner.Userˉruntimeˉbudgetˉaddress,
                client.Userˉruntimeˉinputˉaddress,
                client.Userˉruntimeˉbudgetˉaddress,
            }.Distinct().Count() != 4)
        {
            return Fail("WVOS6101", "The typed-resource owner and borrower records are inconsistent.");
        }

        var Resourceˉaddresses = new[]
        {
            (Owner: owner.Userˉruntimeˉinputˉaddress, Client: client.Userˉruntimeˉinputˉaddress),
            (Owner: owner.Userˉruntimeˉbudgetˉaddress, Client: client.Userˉruntimeˉbudgetˉaddress),
        };
        if (Resourceˉaddresses.Any(Resource =>
                Kernelˉprocessˉplanner.Readˉentry(
                    owner, Kernelˉprocessˉcontract.USER_PT_PAGE, Entryˉindex(Resource.Owner)) !=
                    (Resource.Owner | Kernelˉpagingˉcontract.ENTRY_PRESENT |
                        Kernelˉprocessˉcontract.ENTRY_USER | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE) ||
                Kernelˉprocessˉplanner.Readˉentry(
                    client, Kernelˉprocessˉcontract.USER_PT_PAGE, Entryˉindex(Resource.Client)) != 0) ||
            client.Userˉdataˉbytes.Length != (int)Kernelˉpagingˉcontract.PAGE_BYTES ||
            BinaryPrimitives.ReadUInt64LittleEndian(client.Userˉdataˉbytes.AsSpan()[
                Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET..]) != 0 ||
            BinaryPrimitives.ReadUInt64LittleEndian(client.Userˉdataˉbytes.AsSpan()[
                Nativeˉexecutionˉcontextˉcontract.FILE_INPUT_TABLE_POINTER_OFFSET..]) != 0)
        {
            return Fail("WVOS6102", "The typed resources are not exclusively mapped by init before the grant.");
        }
        if (serviceˉoffset > (uint)client.Userˉcodeˉbytes.Length ||
            Kernelˉprocessˉcontract.BOOT_RESOURCE_SERVICE_BYTES >
                (uint)client.Userˉcodeˉbytes.Length - serviceˉoffset)
        {
            return Fail("WVOS6103", "The granted resource service leaf is outside the client image.");
        }

        var Tables = client.Tableˉbytes.ToArray();
        foreach (var Resource in Resourceˉaddresses)
        {
            var Entryˉoffset = checked((int)(Kernelˉprocessˉcontract.USER_PT_PAGE *
                Kernelˉpagingˉcontract.PAGE_BYTES) + Entryˉindex(Resource.Client) * sizeof(ulong));
            BinaryPrimitives.WriteUInt64LittleEndian(
                Tables.AsSpan(Entryˉoffset),
                Resource.Owner | Kernelˉpagingˉcontract.ENTRY_PRESENT |
                    Kernelˉprocessˉcontract.ENTRY_USER | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        }

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
        BinaryPrimitives.WriteUInt32LittleEndian(
            Data.AsSpan((int)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET +
                (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_COUNT_OFFSET),
            Kernelˉprocessˉcontract.RESOURCE_COUNT);
        Writeˉdirectoryˉentry(
            Data,
            Kernelˉprocessˉcontract.BOOT_RESOURCE_FIRST_ENTRY_OFFSET,
            Kernelˉprocessˉcontract.MODULE_RESOURCE_ID,
            Kernelˉprocessˉcontract.RESOURCE_KIND_WVB_MODULE,
            client.Userˉruntimeˉinputˉaddress,
            checked((uint)programˉlength));
        Writeˉdirectoryˉentry(
            Data,
            Kernelˉprocessˉcontract.BOOT_RESOURCE_SECOND_ENTRY_OFFSET,
            Kernelˉprocessˉcontract.BUDGET_RESOURCE_ID,
            Kernelˉprocessˉcontract.RESOURCE_KIND_U32_EXECUTION_BUDGET,
            client.Userˉruntimeˉbudgetˉaddress,
            Kernelˉprocessˉcontract.EXECUTION_BUDGET_BYTES);

        if (Isˉsecondˉgrant)
        {
            var Expectedˉowned = Buildˉresourceˉrecords(
                owner, client, programˉdigest, budgetˉdigest, programˉlength,
                serviceˉoffset,
                Kernelˉprocessˉcontract.FIRST_CLIENT_PROCESS_REFERENCE,
                Previousˉgrantˉcount);
            Releaseˉrecords(Expectedˉowned);
            if (!Expectedˉowned.AsSpan().SequenceEqual(ownedˉresourceˉrecords))
            {
                return Fail("WVOS6105", "The prior owned resource state cannot authorize generation-two reuse.");
            }
        }

        var Records = Buildˉresourceˉrecords(
            owner, client, programˉdigest, budgetˉdigest, programˉlength,
            serviceˉoffset, Borrowerˉreference, Clientˉgeneration);
        return new(new(Records.ToImmutableArray(), Tables.ToImmutableArray(), Data.ToImmutableArray()), []);
    }

    private static byte[] Buildˉresourceˉrecords(
        Kernelˉprocessˉplan owner,
        Kernelˉprocessˉplan client,
        ReadOnlySpan<byte> programˉdigest,
        ReadOnlySpan<byte> budgetˉdigest,
        int programˉlength,
        uint serviceˉoffset,
        uint borrowerˉreference,
        uint grantˉcount)
    {
        var Serviceˉaddress = client.Userˉcodeˉaddress + serviceˉoffset;
        var Records = new byte[Kernelˉprocessˉcontract.RESOURCE_RECORD_SET_BYTES];
        Writeˉrecord(
            Records.AsSpan(0, (int)Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES),
            Kernelˉprocessˉcontract.MODULE_RESOURCE_ID,
            Kernelˉprocessˉcontract.MODULE_RESOURCE_ATTRIBUTES,
            owner.Userˉruntimeˉinputˉaddress,
            client.Userˉruntimeˉinputˉaddress,
            checked((uint)programˉlength),
            Serviceˉaddress,
            programˉdigest,
            client,
            borrowerˉreference,
            grantˉcount);
        Writeˉrecord(
            Records.AsSpan((int)Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES),
            Kernelˉprocessˉcontract.BUDGET_RESOURCE_ID,
            Kernelˉprocessˉcontract.BUDGET_RESOURCE_ATTRIBUTES,
            owner.Userˉruntimeˉbudgetˉaddress,
            client.Userˉruntimeˉbudgetˉaddress,
            Kernelˉprocessˉcontract.EXECUTION_BUDGET_BYTES,
            Serviceˉaddress,
            budgetˉdigest,
            client,
            borrowerˉreference,
            grantˉcount);
        return Records;
    }

    private static void Releaseˉrecords(byte[] records)
    {
        for (var Index = 0; Index < Kernelˉprocessˉcontract.RESOURCE_COUNT; Index++)
        {
            var Record = records.AsSpan(
                checked((int)(Index * Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES)),
                (int)Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Record[(int)Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET..],
                Kernelˉprocessˉcontract.RESOURCE_STATE_OWNED);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Record[(int)Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET..], 0);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Record[(int)Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET..], 0);
        }
    }

    private static void Writeˉdirectoryˉentry(
        byte[] data,
        uint entryˉoffset,
        uint resourceˉid,
        uint kind,
        ulong pointer,
        uint length)
    {
        var Offset = checked((int)(Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET + entryˉoffset));
        BinaryPrimitives.WriteUInt32LittleEndian(
            data.AsSpan(Offset + (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_ENTRY_ID_OFFSET), resourceˉid);
        BinaryPrimitives.WriteUInt32LittleEndian(
            data.AsSpan(Offset + (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_ENTRY_KIND_OFFSET), kind);
        BinaryPrimitives.WriteUInt64LittleEndian(
            data.AsSpan(Offset + (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_DATA_POINTER_OFFSET), pointer);
        BinaryPrimitives.WriteUInt32LittleEndian(
            data.AsSpan(Offset + (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_DATA_LENGTH_OFFSET), length);
        BinaryPrimitives.WriteUInt32LittleEndian(
            data.AsSpan(Offset + (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_ENTRY_FLAGS_OFFSET),
            Kernelˉprocessˉcontract.RESOURCE_BASE_FLAGS);
    }

    private static void Writeˉrecord(
        Span<byte> record,
        uint resourceˉid,
        uint attributes,
        ulong sourceˉaddress,
        ulong targetˉaddress,
        uint length,
        ulong serviceˉaddress,
        ReadOnlySpan<byte> digest,
        Kernelˉprocessˉplan client,
        uint borrowerˉreference,
        uint grantˉcount)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(record, Kernelˉprocessˉcontract.RESOURCE_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(record[8..], Kernelˉprocessˉcontract.RESOURCE_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(record[12..], Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES);
        BinaryPrimitives.WriteUInt32LittleEndian(record[(int)Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET..],
            Kernelˉprocessˉcontract.RESOURCE_STATE_BORROWED);
        BinaryPrimitives.WriteUInt32LittleEndian(record[(int)Kernelˉprocessˉcontract.RESOURCE_ID_OFFSET..], resourceˉid);
        BinaryPrimitives.WriteUInt32LittleEndian(record[(int)Kernelˉprocessˉcontract.RESOURCE_OWNER_OFFSET..],
            Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE);
        BinaryPrimitives.WriteUInt32LittleEndian(record[(int)Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET..],
            borrowerˉreference);
        BinaryPrimitives.WriteUInt64LittleEndian(record[(int)Kernelˉprocessˉcontract.RESOURCE_SOURCE_ADDRESS_OFFSET..],
            sourceˉaddress);
        BinaryPrimitives.WriteUInt32LittleEndian(record[(int)Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET..], length);
        BinaryPrimitives.WriteUInt32LittleEndian(record[(int)Kernelˉprocessˉcontract.RESOURCE_FLAGS_OFFSET..], attributes);
        BinaryPrimitives.WriteUInt64LittleEndian(record[(int)Kernelˉprocessˉcontract.RESOURCE_TARGET_ROOT_OFFSET..],
            client.Rootˉaddress);
        BinaryPrimitives.WriteUInt64LittleEndian(record[(int)Kernelˉprocessˉcontract.RESOURCE_TARGET_DATA_OFFSET..],
            client.Userˉdataˉaddress);
        BinaryPrimitives.WriteUInt64LittleEndian(record[(int)Kernelˉprocessˉcontract.RESOURCE_TARGET_ADDRESS_OFFSET..],
            targetˉaddress);
        BinaryPrimitives.WriteUInt64LittleEndian(record[(int)Kernelˉprocessˉcontract.RESOURCE_SERVICE_ADDRESS_OFFSET..],
            serviceˉaddress);
        digest.CopyTo(record[(int)Kernelˉprocessˉcontract.RESOURCE_DIGEST_OFFSET..]);
        BinaryPrimitives.WriteUInt32LittleEndian(
            record[(int)Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET..], grantˉcount);
        BinaryPrimitives.WriteUInt32LittleEndian(record[(int)Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET..], 1);
        BinaryPrimitives.WriteUInt64LittleEndian(record[(int)Kernelˉprocessˉcontract.RESOURCE_TARGET_PTE_OFFSET..],
            checked(client.Rootˉaddress + Kernelˉprocessˉcontract.USER_PT_PAGE *
                Kernelˉpagingˉcontract.PAGE_BYTES + (ulong)Entryˉindex(targetˉaddress) * sizeof(ulong)));
    }

    private static bool Hasˉrecord(
        ImmutableArray<byte> record,
        uint processˉid,
        uint processˉgeneration,
        uint role,
        uint profile,
        ReadOnlySpan<byte> digest) =>
        record.Length == Kernelˉprocessˉcontract.RECORD_BYTES &&
        BinaryPrimitives.ReadUInt64LittleEndian(record.AsSpan()) == Kernelˉprocessˉcontract.RECORD_MAGIC &&
        BinaryPrimitives.ReadUInt32LittleEndian(record.AsSpan()[8..]) ==
            Kernelˉprocessˉcontract.RECORD_VERSION &&
        BinaryPrimitives.ReadUInt32LittleEndian(record.AsSpan()[24..]) == processˉid &&
        BinaryPrimitives.ReadUInt32LittleEndian(record.AsSpan()[
            (int)Kernelˉprocessˉcontract.PROCESS_GENERATION_OFFSET..]) == processˉgeneration &&
        BinaryPrimitives.ReadUInt32LittleEndian(record.AsSpan()[
            (int)Kernelˉprocessˉcontract.ROLE_OFFSET..]) == role &&
        BinaryPrimitives.ReadUInt32LittleEndian(record.AsSpan()[
            (int)Kernelˉprocessˉcontract.RUNTIME_PROFILE_OFFSET..]) == profile &&
        record.AsSpan((int)Kernelˉprocessˉcontract.PROGRAM_DIGEST_OFFSET,
            Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES).SequenceEqual(digest);

    private static uint Readˉprocessˉgeneration(ImmutableArray<byte> record) =>
        record.Length == Kernelˉprocessˉcontract.RECORD_BYTES
            ? BinaryPrimitives.ReadUInt32LittleEndian(record.AsSpan()[
                (int)Kernelˉprocessˉcontract.PROCESS_GENERATION_OFFSET..])
            : 0;

    private static int Entryˉindex(ulong address) => checked((int)(
        (address & (Kernelˉpagingˉcontract.LARGE_PAGE_BYTES - 1)) /
        Kernelˉpagingˉcontract.PAGE_BYTES));

    private static Kernelˉresourceˉgrantˉresult Fail(string code, string message) =>
        new(null, [new(code, message)]);
}

public static class Kernelˉresourceˉrevocationˉplanner
{
    public static Kernelˉresourceˉrevocationˉresult Plan(
        Kernelˉprocessˉplan owner,
        Kernelˉprocessˉplan client,
        Kernelˉresourceˉgrantˉplan granted,
        uint clientˉprocessˉstate,
        uint clientˉthreadˉstate)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(granted);
        var Clientˉgeneration = client.Processˉrecord.Length == Kernelˉprocessˉcontract.RECORD_BYTES
            ? BinaryPrimitives.ReadUInt32LittleEndian(client.Processˉrecord.AsSpan()[
                (int)Kernelˉprocessˉcontract.PROCESS_GENERATION_OFFSET..])
            : 0;
        if (Clientˉgeneration is not
            Kernelˉprocessˉcontract.FIRST_CLIENT_GENERATION and not
            Kernelˉprocessˉcontract.SECOND_CLIENT_GENERATION)
        {
            return Fail("WVOS6204", "The terminal borrower has no valid lifecycle generation.");
        }
        var Borrowerˉreference =
            (Clientˉgeneration << 16) | Kernelˉprocessˉcontract.CLIENT_PROCESS_ID;
        var Terminal =
            (clientˉprocessˉstate == Kernelˉprocessˉcontract.PROCESS_STATE_EXITED &&
                clientˉthreadˉstate == Kernelˉprocessˉcontract.THREAD_STATE_EXITED) ||
            (clientˉprocessˉstate == Kernelˉprocessˉcontract.PROCESS_STATE_FAULTED &&
                clientˉthreadˉstate == Kernelˉprocessˉcontract.THREAD_STATE_FAULTED);
        if (!Terminal)
        {
            return Fail("WVOS6201", "The resource borrower is not in one coherent terminal state.");
        }
        if (granted.Resourceˉrecords.Length != Kernelˉprocessˉcontract.RESOURCE_RECORD_SET_BYTES ||
            granted.Clientˉtableˉbytes.Length != (int)Kernelˉprocessˉcontract.TABLE_BYTES ||
            granted.Clientˉdataˉbytes.Length != (int)Kernelˉpagingˉcontract.PAGE_BYTES)
        {
            return Fail("WVOS6202", "The borrowed resource set or client publication has an invalid extent.");
        }

        var Records = granted.Resourceˉrecords.AsSpan();
        var Moduleˉrecord = Records[..(int)Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES];
        var Budgetˉrecord = Records[(int)Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES..];
        if (!Hasˉliveˉrecord(
                Moduleˉrecord,
                Kernelˉprocessˉcontract.MODULE_RESOURCE_ID,
                Kernelˉprocessˉcontract.MODULE_RESOURCE_ATTRIBUTES,
                Borrowerˉreference,
                Clientˉgeneration) ||
            !Hasˉliveˉrecord(
                Budgetˉrecord,
                Kernelˉprocessˉcontract.BUDGET_RESOURCE_ID,
                Kernelˉprocessˉcontract.BUDGET_RESOURCE_ATTRIBUTES,
                Borrowerˉreference,
                Clientˉgeneration))
        {
            return Fail("WVOS6202", "The resource set is not two exact live immutable typed borrows.");
        }

        var Programˉlength = BinaryPrimitives.ReadUInt32LittleEndian(Moduleˉrecord[
            (int)Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET..]);
        var Budgetˉlength = BinaryPrimitives.ReadUInt32LittleEndian(Budgetˉrecord[
            (int)Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET..]);
        var Serviceˉaddress = BinaryPrimitives.ReadUInt64LittleEndian(Moduleˉrecord[
            (int)Kernelˉprocessˉcontract.RESOURCE_SERVICE_ADDRESS_OFFSET..]);
        if (Programˉlength is < 12 or > Kernelˉprocessˉcontract.MAXIMUM_RUNTIME_INPUT_BYTES ||
            Budgetˉlength != Kernelˉprocessˉcontract.EXECUTION_BUDGET_BYTES ||
            BinaryPrimitives.ReadUInt64LittleEndian(Budgetˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_SERVICE_ADDRESS_OFFSET..]) != Serviceˉaddress ||
            Serviceˉaddress < client.Userˉcodeˉaddress ||
            Serviceˉaddress - client.Userˉcodeˉaddress > uint.MaxValue)
        {
            return Fail("WVOS6202", "The borrowed resource lengths or shared service address are invalid.");
        }
        var Programˉdigest = Moduleˉrecord.Slice(
            (int)Kernelˉprocessˉcontract.RESOURCE_DIGEST_OFFSET,
            Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES);
        var Budgetˉdigest = Budgetˉrecord.Slice(
            (int)Kernelˉprocessˉcontract.RESOURCE_DIGEST_OFFSET,
            Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES);
        var Previousˉowned = Clientˉgeneration == Kernelˉprocessˉcontract.SECOND_CLIENT_GENERATION
            ? Buildˉpreviousˉownedˉrecords(granted.Resourceˉrecords)
            : [];
        var Expected = Kernelˉresourceˉgrantˉplanner.Plan(
            owner,
            client,
            Programˉdigest,
            Budgetˉdigest,
            checked((int)Programˉlength),
            Kernelˉprocessˉcontract.RESOURCE_SET_TOKEN,
            checked((uint)(Serviceˉaddress - client.Userˉcodeˉaddress)),
            Previousˉowned.AsSpan());
        if (!Expected.Success)
        {
            return Fail("WVOS6203", "The live aliases or client resource publication are inconsistent.");
        }

        var Targetˉptes = new[]
        {
            BinaryPrimitives.ReadUInt64LittleEndian(Moduleˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_TARGET_PTE_OFFSET..]),
            BinaryPrimitives.ReadUInt64LittleEndian(Budgetˉrecord[
                (int)Kernelˉprocessˉcontract.RESOURCE_TARGET_PTE_OFFSET..]),
        };
        if (Targetˉptes.Distinct().Count() != Kernelˉprocessˉcontract.RESOURCE_COUNT ||
            Targetˉptes.Any(Targetˉpte =>
                Targetˉpte < client.Rootˉaddress ||
                Targetˉpte - client.Rootˉaddress >
                    checked((ulong)(granted.Clientˉtableˉbytes.Length - sizeof(ulong)))))
        {
            return Fail("WVOS6203", "A live alias PTE is duplicate or outside the borrower table extent.");
        }

        var Normalizedˉtables = granted.Clientˉtableˉbytes.ToArray();
        foreach (var Targetˉpte in Targetˉptes)
        {
            var Targetˉpteˉoffset = checked((int)(Targetˉpte - client.Rootˉaddress));
            var Liveˉentry = BinaryPrimitives.ReadUInt64LittleEndian(
                Normalizedˉtables.AsSpan(Targetˉpteˉoffset));
            BinaryPrimitives.WriteUInt64LittleEndian(
                Normalizedˉtables.AsSpan(Targetˉpteˉoffset),
                Liveˉentry & ~Kernelˉpagingˉcontract.ENTRY_ACCESSED);
        }
        if (!Expected.Plan!.Resourceˉrecords.AsSpan().SequenceEqual(Records) ||
            !Expected.Plan.Clientˉtableˉbytes.AsSpan().SequenceEqual(Normalizedˉtables) ||
            !Expected.Plan.Clientˉdataˉbytes.AsSpan().SequenceEqual(
                granted.Clientˉdataˉbytes.AsSpan()))
        {
            return Fail("WVOS6203", "The live aliases or client resource publication are inconsistent.");
        }

        var Releasedˉrecords = granted.Resourceˉrecords.ToArray();
        for (var Index = 0; Index < Kernelˉprocessˉcontract.RESOURCE_COUNT; Index++)
        {
            var Recordˉoffset = checked((int)(Index * Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES));
            var Record = Releasedˉrecords.AsSpan(
                Recordˉoffset,
                (int)Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Record[(int)Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET..],
                Kernelˉprocessˉcontract.RESOURCE_STATE_OWNED);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Record[(int)Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET..], 0);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Record[(int)Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET..], 0);
        }

        var Releasedˉtables = granted.Clientˉtableˉbytes.ToArray();
        foreach (var Targetˉpte in Targetˉptes)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(
                Releasedˉtables.AsSpan(checked((int)(Targetˉpte - client.Rootˉaddress))), 0);
        }

        var Releasedˉdata = granted.Clientˉdataˉbytes.ToArray();
        BinaryPrimitives.WriteUInt64LittleEndian(Releasedˉdata.AsSpan(
            Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET), 0);
        BinaryPrimitives.WriteUInt64LittleEndian(Releasedˉdata.AsSpan(
            Nativeˉexecutionˉcontextˉcontract.FILE_INPUT_TABLE_POINTER_OFFSET), 0);
        Releasedˉdata.AsSpan(
            (int)Kernelˉprocessˉcontract.RUNTIME_SERVICE_TABLE_OFFSET,
            (int)Nativeˉserviceˉtableˉcontract.SIZE).Clear();
        Releasedˉdata.AsSpan(
            (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET,
            (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_BYTES).Clear();
        return new(new(
            Releasedˉrecords.ToImmutableArray(),
            Releasedˉtables.ToImmutableArray(),
            Releasedˉdata.ToImmutableArray()), []);
    }

    private static bool Hasˉliveˉrecord(
        ReadOnlySpan<byte> record,
        uint resourceˉid,
        uint attributes,
        uint borrowerˉreference,
        uint grantˉcount) =>
        BinaryPrimitives.ReadUInt64LittleEndian(record) == Kernelˉprocessˉcontract.RESOURCE_MAGIC &&
        BinaryPrimitives.ReadUInt32LittleEndian(record[8..]) == Kernelˉprocessˉcontract.RESOURCE_VERSION &&
        BinaryPrimitives.ReadUInt32LittleEndian(record[12..]) ==
            Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES &&
        BinaryPrimitives.ReadUInt32LittleEndian(record[
            (int)Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET..]) ==
            Kernelˉprocessˉcontract.RESOURCE_STATE_BORROWED &&
        BinaryPrimitives.ReadUInt32LittleEndian(record[
            (int)Kernelˉprocessˉcontract.RESOURCE_ID_OFFSET..]) == resourceˉid &&
        BinaryPrimitives.ReadUInt32LittleEndian(record[
            (int)Kernelˉprocessˉcontract.RESOURCE_OWNER_OFFSET..]) ==
            Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE &&
        BinaryPrimitives.ReadUInt32LittleEndian(record[
            (int)Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET..]) == borrowerˉreference &&
        BinaryPrimitives.ReadUInt32LittleEndian(record[
            (int)Kernelˉprocessˉcontract.RESOURCE_FLAGS_OFFSET..]) == attributes &&
        BinaryPrimitives.ReadUInt32LittleEndian(record[
            (int)Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET..]) == grantˉcount &&
        BinaryPrimitives.ReadUInt32LittleEndian(record[
            (int)Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET..]) == 1;

    private static ImmutableArray<byte> Buildˉpreviousˉownedˉrecords(
        ImmutableArray<byte> liveˉrecords)
    {
        var Previous = liveˉrecords.ToArray();
        for (var Index = 0; Index < Kernelˉprocessˉcontract.RESOURCE_COUNT; Index++)
        {
            var Record = Previous.AsSpan(
                checked((int)(Index * Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES)),
                (int)Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Record[(int)Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET..],
                Kernelˉprocessˉcontract.RESOURCE_STATE_OWNED);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Record[(int)Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET..], 0);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Record[(int)Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET..],
                Kernelˉprocessˉcontract.FIRST_CLIENT_GENERATION);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Record[(int)Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET..], 0);
        }
        return Previous.ToImmutableArray();
    }

    private static Kernelˉresourceˉrevocationˉresult Fail(string code, string message) =>
        new(null, [new(code, message)]);
}

public static class Kernelˉchannelˉpeerˉlifecycle
{
    public static ImmutableArray<byte> Create()
    {
        var Record = new byte[Kernelˉprocessˉcontract.CHANNEL_RECORD_BYTES];
        BinaryPrimitives.WriteUInt64LittleEndian(Record, Kernelˉprocessˉcontract.CHANNEL_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(8), Kernelˉprocessˉcontract.CHANNEL_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Record.AsSpan(12), Kernelˉprocessˉcontract.CHANNEL_RECORD_BYTES);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Record.AsSpan((int)Kernelˉprocessˉcontract.CHANNEL_CAPACITY_OFFSET),
            Kernelˉprocessˉcontract.CHANNEL_CAPACITY);
        return Record.ToImmutableArray();
    }

    public static ImmutableArray<byte> Terminateˉpeer(
        ImmutableArray<byte> record,
        uint processˉid,
        uint peerˉstatus)
    {
        if (!Hasˉheader(record) ||
            processˉid is not (Kernelˉprocessˉcontract.INIT_PROCESS_ID or
                Kernelˉprocessˉcontract.CLIENT_PROCESS_ID) ||
            peerˉstatus is not (Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_EXITED or
                Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_FAULTED) ||
            BinaryPrimitives.ReadUInt32LittleEndian(record.AsSpan()[
                (int)Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_OFFSET..]) !=
                Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_OPEN)
        {
            throw new InvalidOperationException("The channel peer transition is invalid or stale.");
        }

        var Result = record.ToArray();
        foreach (var Offset in new[]
        {
            Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_MESSAGE_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_SENDER_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_RECEIVER_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_BYTE_LENGTH_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_SERVICE_CAPACITY_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_CLIENT_CAPACITY_OFFSET,
        })
        {
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan((int)Offset), 0);
        }
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(
            (int)Kernelˉprocessˉcontract.CHANNEL_SERVICE_DESTINATION_OFFSET), 0);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(
            (int)Kernelˉprocessˉcontract.CHANNEL_CLIENT_DESTINATION_OFFSET), 0);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(
            (int)Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_OFFSET), peerˉstatus);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(
            (int)Kernelˉprocessˉcontract.CHANNEL_PEER_PROCESS_OFFSET), processˉid);
        var Closeˉcount = BinaryPrimitives.ReadUInt32LittleEndian(Result.AsSpan(
            (int)Kernelˉprocessˉcontract.CHANNEL_CLOSE_COUNT_OFFSET));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(
            (int)Kernelˉprocessˉcontract.CHANNEL_CLOSE_COUNT_OFFSET), checked(Closeˉcount + 1));
        return Result.ToImmutableArray();
    }

    public static ImmutableArray<byte> Reopen(ImmutableArray<byte> record)
    {
        if (!Hasˉheader(record) ||
            BinaryPrimitives.ReadUInt32LittleEndian(record.AsSpan()[
                (int)Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_OFFSET..]) ==
                Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_OPEN)
        {
            throw new InvalidOperationException("Only one cleanly terminated channel generation can be reopened.");
        }
        var Result = record.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(
            (int)Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_OFFSET), 0);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(
            (int)Kernelˉprocessˉcontract.CHANNEL_PEER_PROCESS_OFFSET), 0);
        return Result.ToImmutableArray();
    }

    private static bool Hasˉheader(ImmutableArray<byte> record) =>
        record.Length == Kernelˉprocessˉcontract.CHANNEL_RECORD_BYTES &&
        BinaryPrimitives.ReadUInt64LittleEndian(record.AsSpan()) == Kernelˉprocessˉcontract.CHANNEL_MAGIC &&
        BinaryPrimitives.ReadUInt32LittleEndian(record.AsSpan()[8..]) == Kernelˉprocessˉcontract.CHANNEL_VERSION &&
        BinaryPrimitives.ReadUInt32LittleEndian(record.AsSpan()[12..]) ==
            Kernelˉprocessˉcontract.CHANNEL_RECORD_BYTES;
}
