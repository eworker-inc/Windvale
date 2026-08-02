using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;
using Windvale.ObjectModel;

namespace Windvale.Bootstrap;

public sealed record Kernelˉprocessˉx64ˉartifacts(
    ImmutableArray<byte> Objectˉbytes,
    ImmutableArray<byte> Codeˉbytes,
    ImmutableArray<Objectˉrelocation> Relocations);

public static class Kernelˉprocessˉx64
{
    private const string BOOT_RESOURCE_SERVICE_OBJECT_SHA256 =
        "610B861538697CA15C7F2B5FAC5BC222BE5697A2063509FFB7AB5B0E669A226D";
    private const string FAILURE_LABEL = "process_failure";
    private const string SERVICE_BLOCKED_LABEL = "process_service_blocked";
    private const string CLIENT_COMPLETION_LABEL = "process_client_completion";
    private const string COMPLETION_LABEL = "process_completion";
    private const string PT_LOOP_LABEL = "process_pt_loop";
    private const string PT_NULL_DONE_LABEL = "process_pt_null_done";
    private const string SYSCALL_SEND_LABEL = "process_syscall_send";
    private const string SYSCALL_RECEIVE_LABEL = "process_syscall_receive";
    private const string SYSCALL_EXIT_LABEL = "process_syscall_exit";
    private const string SYSCALL_GRANT_RESOURCE_LABEL = "process_syscall_grant_resource";
    private const string SYSCALL_RESUME_LABEL = "process_syscall_resume";
    private const string SYSCALL_FAILURE_LABEL = "process_syscall_failure";
    private const string EXCEPTION_KERNEL_LABEL = "process_exception_kernel";
    private const string EXCEPTION_FAILURE_LABEL = "process_exception_failure";
    private const byte CONDITION_BELOW = 0x82;
    private const byte CONDITION_EQUAL = 0x84;
    private const byte CONDITION_NOT_EQUAL = 0x85;
    private const byte CONDITION_ABOVE = 0x87;
    private const uint POLICY_INSTRUCTION_BUDGET = 8_192;
    private const uint POLICY_CALL_DEPTH_BUDGET = 2;
    private const uint FRAME_BYTES = 0xA0;
    private const uint CONTEXT_OFFSET = 0x20;
    private const uint IDT_DESCRIPTOR_OFFSET = 15 * Kernelˉexceptionˉcontract.IDT_GATE_BYTES;
    private const ushort IDT_LIMIT = (ushort)(IDT_DESCRIPTOR_OFFSET - 1);
    private const ushort KERNEL_CODE_SELECTOR = 0x08;
    private const ushort KERNEL_DATA_SELECTOR = 0x10;
    private const ushort TSS_SELECTOR = 0x28;

    public static Kernelˉprocessˉx64ˉartifacts Build(
        Kernelˉprocessˉimageˉartifacts image,
        bool userˉfault)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (image.Initˉserviceˉimageˉbytes.IsEmpty ||
            image.Initˉserviceˉimageˉbytes.Length > (int)Kernelˉpagingˉcontract.PAGE_BYTES ||
            image.Clientˉimageˉbytes.IsEmpty ||
            (ulong)image.Clientˉimageˉbytes.Length > Kernelˉprocessˉcontract.CLIENT_CODE_BYTES ||
            image.Initˉserviceˉdigest.Length != Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES ||
            image.Interpreterˉdigest.Length != Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES ||
            image.Admittedˉprogramˉdigest.Length != Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES ||
            image.Admittedˉprogramˉbytes.Length is < 12 or >
                Kernelˉprocessˉcontract.MAXIMUM_RUNTIME_INPUT_BYTES ||
            !SHA256.HashData(image.Admittedˉprogramˉbytes.AsSpan()).AsSpan()
                .SequenceEqual(image.Admittedˉprogramˉdigest.AsSpan()) ||
            image.Bootˉresourceˉserviceˉoffset > (uint)image.Clientˉimageˉbytes.Length ||
            Kernelˉprocessˉcontract.BOOT_RESOURCE_SERVICE_BYTES >
                (uint)image.Clientˉimageˉbytes.Length - image.Bootˉresourceˉserviceˉoffset)
        {
            throw new InvalidOperationException("The process machine seam received an invalid user image.");
        }
        var Bootˉresourceˉleaf = Readˉbootˉresourceˉleaf(
            image.Bootˉresourceˉserviceˉobjectˉbytes);
        if (!image.Clientˉimageˉbytes.AsSpan(
                (int)image.Bootˉresourceˉserviceˉoffset,
                Bootˉresourceˉleaf.Length).SequenceEqual(Bootˉresourceˉleaf.AsSpan()))
        {
            throw new InvalidOperationException(
                "The process machine seam received a client with a changed boot-resource service leaf.");
        }

        var Output = new X64ˉcodeˉbuilder();
        var Relocations = ImmutableArray.CreateBuilder<Objectˉrelocation>();
        Emitˉenter(Output, Relocations, image, userˉfault);
        var Enterˉbytes = Output.Position;
        Output.Align(16);
        var Exceptionˉoffset = Output.Position;
        Emitˉexceptionˉentry(Output, Relocations);
        var Exceptionˉbytes = Output.Position - Exceptionˉoffset;
        Output.Align(16);
        var Syscallˉoffset = Output.Position;
        Emitˉsyscallˉentry(Output);
        var Syscallˉbytes = Output.Position - Syscallˉoffset;
        var Code = Output.Build();
        var Frozenˉrelocations = Relocations.ToImmutable();

        var Object = new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [
                new(".text.process", Objectˉsectionˉkind.Code, 16, (uint)Code.Length, Code),
                new(".rodata.ainit", Objectˉsectionˉkind.Readˉonlyˉdata, 16,
                    (uint)image.Initˉserviceˉimageˉbytes.Length, image.Initˉserviceˉimageˉbytes),
                new(".rodata.bclient", Objectˉsectionˉkind.Readˉonlyˉdata, 16,
                    (uint)image.Clientˉimageˉbytes.Length, image.Clientˉimageˉbytes),
                new(".rodata.cresource", Objectˉsectionˉkind.Readˉonlyˉdata, 16,
                    (uint)image.Admittedˉprogramˉbytes.Length, image.Admittedˉprogramˉbytes),
            ],
            [
                new("Windvale_init_resource_user_image", Objectˉsymbolˉbinding.Local,
                    Objectˉsymbolˉkind.Data, 1, 0, (uint)image.Initˉserviceˉimageˉbytes.Length),
                new("Windvale_process_client_image", Objectˉsymbolˉbinding.Local,
                    Objectˉsymbolˉkind.Data, 2, 0, (uint)image.Clientˉimageˉbytes.Length),
                new("Windvale_resource_init_boot", Objectˉsymbolˉbinding.Local,
                    Objectˉsymbolˉkind.Data, 3, 0, (uint)image.Admittedˉprogramˉbytes.Length),
                new(Kernelˉprocessˉcontract.ENTER_SYMBOL, Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function, 0, 0, Enterˉbytes),
                new(Kernelˉprocessˉcontract.EXCEPTION_ENTRY_SYMBOL, Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function, 0, Exceptionˉoffset, Exceptionˉbytes),
                new(Kernelˉprocessˉcontract.SYSCALL_ENTRY_SYMBOL, Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function, 0, Syscallˉoffset, Syscallˉbytes),
                Import(Kernelˉmemoryˉcontract.ALLOCATE_PAGES_SYMBOL),
                Import(Kernelˉprocessˉcontract.POLICY_SYMBOL),
                Import(Kernelˉexceptionˉcontract.TERMINAL_SYMBOL),
                Import(Kernelˉpagingˉcontract.PAGE_TABLE_ACTIVATE_SYMBOL),
                Import(Kernelˉprocessˉcontract.EXCEPTION_13_ENTRY_SYMBOL),
                Import(Kernelˉprocessˉcontract.EXCEPTION_14_ENTRY_SYMBOL),
                Import(Kernelˉprocessˉcontract.EXCEPTION_6_ENTRY_SYMBOL),
            ],
            Frozenˉrelocations);
        var Objectˉbytes = Objectˉcodec.Write(Object).ToImmutableArray();
        Verifyˉobject(Objectˉbytes, Code, image.Initˉserviceˉimageˉbytes,
            image.Clientˉimageˉbytes, image.Admittedˉprogramˉbytes, Frozenˉrelocations);
        return new(Objectˉbytes, Code, Frozenˉrelocations);
    }

    private static void Emitˉenter(
        X64ˉcodeˉbuilder output,
        ImmutableArray<Objectˉrelocation>.Builder relocations,
        Kernelˉprocessˉimageˉartifacts image,
        bool userˉfault)
    {
        // Preserve every nonvolatile register and retain the fixed coordinator
        // frame throughout both CPL3 round trips.
        output.Emit(0x53, 0x55, 0x56, 0x57, 0x41, 0x54, 0x41, 0x55, 0x41, 0x56, 0x41, 0x57);
        output.Emit(0x48, 0x81, 0xEC);
        output.Emitˉu32(FRAME_BYTES);
        output.Emit(0x49, 0x89, 0xCC, 0x49, 0x83, 0xEC, (byte)Kernelˉmemoryˉcontract.HANDOFF_COPY_OFFSET);
        Emitˉvalidateˉmemoryˉstate(output);
        output.Emit(0x4D, 0x8D, 0xAC, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.INIT_RECORD_OFFSET);
        output.Emit(0x4C, 0x89, 0x24, 0x24, 0x4C, 0x89, 0x6C, 0x24, 0x08);
        output.Emit(0x49, 0x8D, 0x84, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.CLIENT_RECORD_OFFSET);
        output.Emit(0x48, 0x89, 0x44, 0x24, 0x10);

        // Windvale policy binds both WVB identities, roles, budgets, and reduced
        // endpoints before any process page or machine state is published.
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(((ulong)Nativeˉexecutionˉcontextˉcontract.SIZE << 32) |
            Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION);
        Emitˉstoreˉstackˉrax(output, CONTEXT_OFFSET);
        output.Emit(0xB8);
        output.Emitˉu32(POLICY_INSTRUCTION_BUDGET);
        Emitˉstoreˉstackˉrax(output, CONTEXT_OFFSET + 8);
        output.Emit(0xB8);
        output.Emitˉu32(POLICY_CALL_DEPTH_BUDGET);
        Emitˉstoreˉstackˉrax(output, CONTEXT_OFFSET + 16);
        output.Emit(0x31, 0xC0);
        for (uint Offset = CONTEXT_OFFSET + 24;
            Offset < CONTEXT_OFFSET + Nativeˉexecutionˉcontextˉcontract.SIZE;
            Offset += sizeof(ulong))
        {
            Emitˉstoreˉstackˉrax(output, Offset);
        }
        output.Emit(0x48, 0x8D, 0x54, 0x24, (byte)CONTEXT_OFFSET);
        Emitˉexternalˉcall(output, relocations, 7);
        output.Emit(0x48, 0x83, 0xF8, (byte)Kernelˉprocessˉcontract.POLICY_TOKEN);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);

        Emitˉinitializeˉchannel(output);

        // Build the init/resource-owner root first. The boot WVB is visible only
        // in this root until Windvale init authorizes one immutable borrow.
        Emitˉallocateˉextent(output, relocations, Kernelˉprocessˉcontract.INIT_ALLOCATION_PAGES);
        Emitˉinitializeˉrecord(
            output,
            image.Initˉserviceˉdigest,
            image.Admittedˉprogramˉdigest,
            Kernelˉprocessˉcontract.INIT_PROCESS_ID,
            Kernelˉprocessˉcontract.INIT_THREAD_ID,
            Kernelˉprocessˉcontract.INIT_CAPABILITY_RIGHTS,
            Kernelˉprocessˉcontract.ROLE_INIT_SERVICE);
        Emitˉcopyˉkernelˉtables(output);
        Emitˉpopulateˉprocessˉtable(
            output, "service", Kernelˉprocessˉcontract.INIT_CODE_PAGES,
            Kernelˉprocessˉcontract.INIT_STACK_PAGE, Kernelˉprocessˉcontract.INIT_STACK_PAGES,
            Kernelˉprocessˉcontract.INIT_DATA_PAGE,
            Kernelˉprocessˉcontract.INIT_RUNTIME_INPUT_PAGE, true);
        Emitˉcopyˉuserˉbytes(
            output, relocations, image.Initˉserviceˉimageˉbytes.Length, 0,
            Kernelˉprocessˉcontract.USER_CODE_PAGE);
        Emitˉcopyˉuserˉbytes(
            output, relocations, image.Admittedˉprogramˉbytes.Length, 2,
            Kernelˉprocessˉcontract.INIT_RUNTIME_INPUT_PAGE);
        Emitˉinitializeˉuserˉcontext(
            output, Kernelˉprocessˉcontract.INIT_DATA_PAGE,
            Kernelˉprocessˉcontract.INIT_INSTRUCTION_BUDGET,
            Kernelˉprocessˉcontract.INIT_CALL_DEPTH_BUDGET);

        // Build a distinct send-only interpreter root. Its target resource PTE
        // and ABI-16 resource tables remain zero until the init grant syscall.
        output.Emit(0x4C, 0x8B, 0x6C, 0x24, 0x10);
        Emitˉallocateˉextent(output, relocations, Kernelˉprocessˉcontract.CLIENT_ALLOCATION_PAGES);
        Emitˉinitializeˉrecord(
            output,
            image.Interpreterˉdigest,
            image.Admittedˉprogramˉdigest,
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID,
            Kernelˉprocessˉcontract.CLIENT_THREAD_ID,
            Kernelˉprocessˉcontract.CAPABILITY_RIGHT_SEND,
            Kernelˉprocessˉcontract.ROLE_BYTECODE_INTERPRETER);
        Emitˉcopyˉkernelˉtables(output);
        Emitˉpopulateˉprocessˉtable(
            output, "client", Kernelˉprocessˉcontract.CLIENT_CODE_PAGES,
            Kernelˉprocessˉcontract.CLIENT_STACK_PAGE, Kernelˉprocessˉcontract.CLIENT_STACK_PAGES,
            Kernelˉprocessˉcontract.CLIENT_DATA_PAGE,
            Kernelˉprocessˉcontract.CLIENT_RUNTIME_INPUT_PAGE, false);
        Emitˉcopyˉuserˉbytes(
            output, relocations, image.Clientˉimageˉbytes.Length, 1,
            Kernelˉprocessˉcontract.USER_CODE_PAGE);
        Emitˉinitializeˉuserˉcontext(
            output, Kernelˉprocessˉcontract.CLIENT_DATA_PAGE,
            Kernelˉprocessˉcontract.CLIENT_INSTRUCTION_BUDGET,
            Kernelˉprocessˉcontract.CLIENT_CALL_DEPTH_BUDGET);
        Emitˉinitializeˉresourceˉrecord(output, image);

        Emitˉinstallˉdescriptorˉstate(output, relocations);
        Emitˉconfigureˉsyscallˉmsrs(output);

        // Windvale init grants its resource, then deterministically blocks on
        // the still-empty result channel.
        output.Emit(0x4C, 0x8B, 0x6C, 0x24, 0x08);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        Emitˉenterˉinitialˉprocess(output, SERVICE_BLOCKED_LABEL, Kernelˉprocessˉcontract.INIT_STACK_PAGES);

        output.Mark(SERVICE_BLOCKED_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_RUNNING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_WAITING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 2);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_CHANNEL_RECEIVE);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉgsˉchannelˉr10(output);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉvalidateˉborrowedˉresource(
            output, FAILURE_LABEL, checked((uint)image.Admittedˉprogramˉbytes.Length));

        // Return GS to its user value, install the client as the next syscall
        // destination, and run it under its own root.
        output.Emit(0x0F, 0x01, 0xF8);
        output.Emit(0x4C, 0x8B, 0x6C, 0x24, 0x10);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        Emitˉenterˉinitialˉprocess(output, CLIENT_COMPLETION_LABEL, Kernelˉprocessˉcontract.CLIENT_STACK_PAGES);

        output.Mark(CLIENT_COMPLETION_LABEL);
        if (userˉfault)
        {
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
                Kernelˉprocessˉcontract.PROCESS_STATE_FAULTED);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
                Kernelˉprocessˉcontract.THREAD_STATE_FAULTED);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.FAULT_VECTOR_OFFSET,
                Kernelˉexceptionˉcontract.GENERAL_PROTECTION_VECTOR);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.FAULT_ERROR_OFFSET, 0);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 1);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        }
        else
        {
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
                Kernelˉprocessˉcontract.PROCESS_STATE_EXITED);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
                Kernelˉprocessˉcontract.THREAD_STATE_EXITED);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET,
                Kernelˉprocessˉcontract.CLIENT_SYSCALL_BUDGET);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        }
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.ROLE_OFFSET,
            Kernelˉprocessˉcontract.ROLE_BYTECODE_INTERPRETER);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.RESULT_OFFSET,
            Kernelˉprocessˉcontract.EXPECTED_RESULT);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉgsˉchannelˉr10(output);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_MESSAGE_OFFSET,
            Kernelˉprocessˉcontract.EXPECTED_RESULT);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_SEND_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);

        // The client is now terminal. Switch back to the blocked service root,
        // consume the one message, and resume the saved receive with EAX=29.
        output.Emit(0x0F, 0x01, 0xF8);
        output.Emit(0x4C, 0x8B, 0x6C, 0x24, 0x08);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        output.Loadˉripˉrelativeˉrdx(COMPLETION_LABEL);
        output.Emit(0x49, 0x89, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_RUNNING);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_NONE);
        Emitˉloadˉrecordˉchannelˉr10(output);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉchannelˉeax(output, Kernelˉprocessˉcontract.CHANNEL_MESSAGE_OFFSET);
        output.Emit(0x83, 0xF8, (byte)Kernelˉprocessˉcontract.EXPECTED_RESULT);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_RECEIVER_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_ID);
        Emitˉincrementˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_RECEIVE_COUNT_OFFSET);
        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET, 0);
        Emitˉincrementˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAKE_COUNT_OFFSET);
        Emitˉresumeˉsavedˉprocess(output);

        output.Mark(COMPLETION_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_EXITED);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_EXITED);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET,
            Kernelˉprocessˉcontract.INIT_SYSCALL_BUDGET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.RESULT_OFFSET,
            Kernelˉprocessˉcontract.EXPECTED_RESULT);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉgsˉchannelˉr10(output);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_MESSAGE_OFFSET,
            Kernelˉprocessˉcontract.EXPECTED_RESULT);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_RECEIVE_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAKE_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);

        // Restore the kernel GS state and independently re-check the terminal client.
        output.Emit(0x0F, 0x01, 0xF8);
        output.Emit(0x4C, 0x8B, 0x6C, 0x24, 0x10);
        Emitˉcompareˉrecordˉu32(output, Kernelˉprocessˉcontract.RESULT_OFFSET,
            Kernelˉprocessˉcontract.EXPECTED_RESULT);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0xB8);
        output.Emitˉu32(Kernelˉprocessˉcontract.EXPECTED_RESULT);
        Emitˉepilogue(output);

        output.Mark(FAILURE_LABEL);
        output.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);
        Emitˉepilogue(output);
    }

    private static void Emitˉexceptionˉentry(
        X64ˉcodeˉbuilder output,
        ImmutableArray<Objectˉrelocation>.Builder relocations)
    {
        // WVA has normalized [vector,error,rip,cs,rflags,(rsp,ss)]. Ring-0 faults
        // retain the qualified terminal path; CPL3 faults become process state.
        output.Emit(0xF6, 0x44, 0x24, (byte)Kernelˉexceptionˉcontract.NORMALIZED_CODE_SELECTOR_OFFSET, 0x03);
        output.Jumpˉif(CONDITION_EQUAL, EXCEPTION_KERNEL_LABEL);
        output.Emit(0x0F, 0x01, 0xF8);
        output.Emit(0x8B, 0x04, 0x24);
        Emitˉstoreˉgsˉeax(output, Kernelˉprocessˉcontract.FAULT_VECTOR_OFFSET);
        output.Emit(0x8B, 0x44, 0x24, (byte)Kernelˉexceptionˉcontract.NORMALIZED_ERROR_CODE_OFFSET);
        Emitˉstoreˉgsˉeax(output, Kernelˉprocessˉcontract.FAULT_ERROR_OFFSET);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.ROLE_OFFSET,
            Kernelˉprocessˉcontract.ROLE_BYTECODE_INTERPRETER);
        output.Jumpˉif(CONDITION_NOT_EQUAL, EXCEPTION_FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, EXCEPTION_FAILURE_LABEL);
        Emitˉloadˉgsˉchannelˉr10(output);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, EXCEPTION_FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_MESSAGE_OFFSET,
            Kernelˉprocessˉcontract.EXPECTED_RESULT);
        output.Jumpˉif(CONDITION_NOT_EQUAL, EXCEPTION_FAILURE_LABEL);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_FAULTED);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_FAULTED);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.RESULT_OFFSET,
            Kernelˉprocessˉcontract.EXPECTED_RESULT);
        Emitˉloadˉgsˉrsp(output, Kernelˉprocessˉcontract.KERNEL_STACK_OFFSET);
        Emitˉjumpˉgs(output, Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);

        output.Mark(EXCEPTION_FAILURE_LABEL);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_FAULTED);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_FAULTED);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.RESULT_OFFSET, 1);
        Emitˉloadˉgsˉrsp(output, Kernelˉprocessˉcontract.KERNEL_STACK_OFFSET);
        Emitˉjumpˉgs(output, Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);

        output.Mark(EXCEPTION_KERNEL_LABEL);
        var Jumpˉfield = output.Emitˉjumpˉplaceholder();
        relocations.Add(Relocation(Jumpˉfield, 8));
    }

    private static void Emitˉsyscallˉentry(X64ˉcodeˉbuilder output)
    {
        output.Mark("process_syscall_machine_entry");
        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉstoreˉgsˉrsp(output, Kernelˉprocessˉcontract.USER_STACK_POINTER_OFFSET);
        Emitˉstoreˉgsˉrcx(output, Kernelˉprocessˉcontract.USER_INSTRUCTION_POINTER_OFFSET);
        Emitˉstoreˉgsˉr11(output, Kernelˉprocessˉcontract.USER_FLAGS_OFFSET);
        Emitˉstoreˉgsˉrdx(output, Kernelˉprocessˉcontract.USER_CONTEXT_POINTER_OFFSET);
        Emitˉloadˉgsˉrsp(output, Kernelˉprocessˉcontract.KERNEL_STACK_OFFSET);
        output.Emit(0x83, 0xFB, (byte)Kernelˉprocessˉcontract.SYSCALL_SEND);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_SEND_LABEL);
        output.Emit(0x83, 0xFB, (byte)Kernelˉprocessˉcontract.SYSCALL_RECEIVE);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_RECEIVE_LABEL);
        output.Emit(0x83, 0xFB, (byte)Kernelˉprocessˉcontract.SYSCALL_EXIT);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_EXIT_LABEL);
        output.Emit(0x83, 0xFB, (byte)Kernelˉprocessˉcontract.SYSCALL_GRANT_BOOT_RESOURCE);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_GRANT_RESOURCE_LABEL);
        output.Jump(SYSCALL_FAILURE_LABEL);

        output.Mark(SYSCALL_GRANT_RESOURCE_LABEL);
        Emitˉvalidateˉcapability(
            output, Kernelˉprocessˉcontract.CAPABILITY_RIGHT_GRANT_BOOT_RESOURCE);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.ROLE_OFFSET,
            Kernelˉprocessˉcontract.ROLE_INIT_SERVICE);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        output.Emit(0x83, 0xF8, (byte)Kernelˉprocessˉcontract.RESOURCE_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉrequireˉsyscallˉbudget(output);
        Emitˉgrantˉbootˉresource(output);
        Emitˉincrementˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET);
        output.Emit(0x31, 0xC0);
        output.Jump(SYSCALL_RESUME_LABEL);

        output.Mark(SYSCALL_SEND_LABEL);
        Emitˉvalidateˉcapability(output, Kernelˉprocessˉcontract.CAPABILITY_RIGHT_SEND);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.ROLE_OFFSET,
            Kernelˉprocessˉcontract.ROLE_BYTECODE_INTERPRETER);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        output.Emit(0x83, 0xF8, (byte)Kernelˉprocessˉcontract.EXPECTED_RESULT);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉrequireˉsyscallˉbudget(output);
        Emitˉloadˉgsˉchannelˉr10(output);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉstoreˉchannelˉeax(output, Kernelˉprocessˉcontract.CHANNEL_MESSAGE_OFFSET);
        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 1);
        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_SENDER_OFFSET,
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID);
        Emitˉincrementˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_SEND_COUNT_OFFSET);
        Emitˉincrementˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET);
        output.Emit(0x31, 0xC0);
        output.Jump(SYSCALL_RESUME_LABEL);

        output.Mark(SYSCALL_RECEIVE_LABEL);
        Emitˉvalidateˉcapability(output, Kernelˉprocessˉcontract.CAPABILITY_RIGHT_RECEIVE);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.ROLE_OFFSET,
            Kernelˉprocessˉcontract.ROLE_INIT_SERVICE);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉrequireˉsyscallˉbudget(output);
        Emitˉloadˉgsˉchannelˉr10(output);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_ID);
        Emitˉincrementˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_WAITING);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_CHANNEL_RECEIVE);
        Emitˉjumpˉgs(output, Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);

        output.Mark(SYSCALL_EXIT_LABEL);
        Emitˉrequireˉsyscallˉbudget(output);
        output.Emit(0x83, 0xF8, (byte)Kernelˉprocessˉcontract.EXPECTED_RESULT);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉincrementˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET);
        Emitˉcompareˉgsˉu32ˉtoˉgsˉu32(
            output,
            Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET,
            Kernelˉprocessˉcontract.SYSCALL_BUDGET_OFFSET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_EXITED);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_EXITED);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.RESULT_OFFSET,
            Kernelˉprocessˉcontract.EXPECTED_RESULT);
        Emitˉjumpˉgs(output, Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);

        output.Mark(SYSCALL_FAILURE_LABEL);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_FAULTED);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_FAULTED);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.FAULT_VECTOR_OFFSET, uint.MaxValue);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.RESULT_OFFSET, 1);
        Emitˉjumpˉgs(output, Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);

        output.Mark(SYSCALL_RESUME_LABEL);
        Emitˉloadˉgsˉrcx(output, Kernelˉprocessˉcontract.USER_INSTRUCTION_POINTER_OFFSET);
        Emitˉloadˉgsˉr11(output, Kernelˉprocessˉcontract.USER_FLAGS_OFFSET);
        Emitˉloadˉgsˉrdx(output, Kernelˉprocessˉcontract.USER_CONTEXT_POINTER_OFFSET);
        Emitˉloadˉgsˉrsp(output, Kernelˉprocessˉcontract.USER_STACK_POINTER_OFFSET);
        output.Emit(0x0F, 0x01, 0xF8, 0x48, 0x0F, 0x07);
    }

    private static void Emitˉvalidateˉmemoryˉstate(X64ˉcodeˉbuilder output)
    {
        output.Emit(0x4D, 0x85, 0xE4);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(Kernelˉmemoryˉcontract.STATE_MAGIC);
        output.Emit(0x49, 0x39, 0x04, 0x24);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x41, 0x83, 0x7C, 0x24, 0x08, (byte)Kernelˉmemoryˉcontract.STATE_VERSION);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
    }

    private static void Emitˉinitializeˉchannel(X64ˉcodeˉbuilder output)
    {
        output.Emit(0x49, 0x8D, 0xBC, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET);
        output.Emit(0x31, 0xC0, 0xB9);
        output.Emitˉu32(Kernelˉprocessˉcontract.CHANNEL_RECORD_BYTES / sizeof(ulong));
        output.Emit(0xFC, 0xF3, 0x48, 0xAB, 0x48, 0xB8);
        output.Emitˉu64(Kernelˉprocessˉcontract.CHANNEL_MAGIC);
        output.Emit(0x49, 0x89, 0x84, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET);
        Emitˉstoreˉstateˉu32(output, Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET + 8,
            Kernelˉprocessˉcontract.CHANNEL_VERSION);
        Emitˉstoreˉstateˉu32(output, Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET + 12,
            Kernelˉprocessˉcontract.CHANNEL_RECORD_BYTES);
        Emitˉstoreˉstateˉu32(
            output,
            Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET + Kernelˉprocessˉcontract.CHANNEL_CAPACITY_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_CAPACITY);
    }

    private static void Emitˉinitializeˉresourceˉrecord(
        X64ˉcodeˉbuilder output,
        Kernelˉprocessˉimageˉartifacts image)
    {
        output.Emit(0x49, 0x8D, 0xBC, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET);
        output.Emit(0x31, 0xC0, 0xB9);
        output.Emitˉu32(Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES / sizeof(ulong));
        output.Emit(0xFC, 0xF3, 0x48, 0xAB);
        output.Emit(0x4D, 0x8D, 0x8C, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET);
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(Kernelˉprocessˉcontract.RESOURCE_MAGIC);
        output.Emit(0x49, 0x89, 0x01);
        Emitˉstoreˉresourceˉu32(output, 8, Kernelˉprocessˉcontract.RESOURCE_VERSION);
        Emitˉstoreˉresourceˉu32(output, 12, Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES);
        Emitˉstoreˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_STATE_OWNED);
        Emitˉstoreˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_ID_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_ID);
        Emitˉstoreˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_OWNER_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_ID);
        Emitˉstoreˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_FLAGS_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_FLAGS);

        output.Emit(0x48, 0x8B, 0x44, 0x24, 0x08, 0x48, 0x8B, 0x40,
            (byte)Kernelˉprocessˉcontract.ROOT_ADDRESS_OFFSET, 0x48, 0x05);
        output.Emitˉu32(checked((uint)(Kernelˉprocessˉcontract.INIT_RUNTIME_INPUT_PAGE *
            Kernelˉpagingˉcontract.PAGE_BYTES)));
        output.Emit(0x49, 0x89, 0x41, (byte)Kernelˉprocessˉcontract.RESOURCE_SOURCE_ADDRESS_OFFSET);
        Emitˉstoreˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET,
            checked((uint)image.Admittedˉprogramˉbytes.Length));

        output.Emit(0x48, 0x8B, 0x44, 0x24, 0x10, 0x48, 0x8B, 0x50,
            (byte)Kernelˉprocessˉcontract.ROOT_ADDRESS_OFFSET);
        output.Emit(0x49, 0x89, 0x51, (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_ROOT_OFFSET);
        output.Emit(0x48, 0x8B, 0x40, (byte)Kernelˉprocessˉcontract.USER_DATA_ADDRESS_OFFSET);
        output.Emit(0x49, 0x89, 0x41, (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_DATA_OFFSET);
        output.Emit(0x48, 0x8D, 0x82);
        output.Emitˉu32(checked((uint)(Kernelˉprocessˉcontract.CLIENT_RUNTIME_INPUT_PAGE *
            Kernelˉpagingˉcontract.PAGE_BYTES)));
        output.Emit(0x49, 0x89, 0x41, (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_ADDRESS_OFFSET);
        output.Emit(0x48, 0x8D, 0x82);
        output.Emitˉu32(checked((uint)(Kernelˉprocessˉcontract.USER_CODE_PAGE *
            Kernelˉpagingˉcontract.PAGE_BYTES + image.Bootˉresourceˉserviceˉoffset)));
        output.Emit(0x49, 0x89, 0x41, (byte)Kernelˉprocessˉcontract.RESOURCE_SERVICE_ADDRESS_OFFSET);
        for (var Offset = 0; Offset < image.Admittedˉprogramˉdigest.Length; Offset += sizeof(ulong))
        {
            output.Emit(0x48, 0xB8);
            output.Emitˉu64(BinaryPrimitives.ReadUInt64LittleEndian(
                image.Admittedˉprogramˉdigest.AsSpan().Slice(Offset)));
            output.Emit(0x49, 0x89, 0x41,
                checked((byte)(Kernelˉprocessˉcontract.RESOURCE_DIGEST_OFFSET + Offset)));
        }

        output.Emit(0x49, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_ADDRESS_OFFSET);
        output.Emit(0x48, 0x25, 0x00, 0xF0, 0x1F, 0x00, 0x48, 0xC1, 0xE8, 0x09);
        output.Emit(0x49, 0x03, 0x41, (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_ROOT_OFFSET);
        output.Emit(0x48, 0x05);
        output.Emitˉu32(checked((uint)(Kernelˉprocessˉcontract.USER_PT_PAGE *
            Kernelˉpagingˉcontract.PAGE_BYTES)));
        output.Emit(0x49, 0x89, 0x41, (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_PTE_OFFSET);
    }

    private static void Emitˉallocateˉextent(
        X64ˉcodeˉbuilder output,
        ImmutableArray<Objectˉrelocation>.Builder relocations,
        ulong allocationˉpages)
    {
        output.Emit(0x4C, 0x89, 0xE1, 0xBA);
        output.Emitˉu32(checked((uint)allocationˉpages));
        Emitˉexternalˉcall(output, relocations, 6);
        output.Emit(0x48, 0x85, 0xC0);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        output.Emit(0x49, 0x89, 0xC6);
        output.Emit(0x48, 0xF7, 0xC0, 0xFF, 0x0F, 0x00, 0x00);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x49, 0x8D, 0x86);
        output.Emitˉu32(checked((uint)(allocationˉpages * Kernelˉpagingˉcontract.PAGE_BYTES)));
        output.Emit(0x48, 0x3D);
        output.Emitˉu32((uint)Kernelˉpagingˉcontract.IDENTITY_BYTES);
        output.Jumpˉif(CONDITION_ABOVE, FAILURE_LABEL);
        output.Emit(0x4C, 0x89, 0xF0, 0x48, 0xC1, 0xE8, 0x15, 0x49, 0x8D, 0x8E);
        output.Emitˉu32(checked((uint)(allocationˉpages * Kernelˉpagingˉcontract.PAGE_BYTES - 1)));
        output.Emit(0x48, 0xC1, 0xE9, 0x15, 0x48, 0x39, 0xC8);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
    }

    private static void Emitˉinitializeˉrecord(
        X64ˉcodeˉbuilder output,
        ImmutableArray<byte> digest,
        ImmutableArray<byte> programˉdigest,
        uint processˉid,
        uint threadˉid,
        uint capabilityˉrights,
        uint role)
    {
        var Isˉinit = role == Kernelˉprocessˉcontract.ROLE_INIT_SERVICE;
        var Stackˉpage = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_STACK_PAGE
            : Kernelˉprocessˉcontract.CLIENT_STACK_PAGE;
        var Stackˉpages = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_STACK_PAGES
            : Kernelˉprocessˉcontract.CLIENT_STACK_PAGES;
        var Dataˉpage = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_DATA_PAGE
            : Kernelˉprocessˉcontract.CLIENT_DATA_PAGE;
        var Codeˉpages = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_CODE_PAGES
            : Kernelˉprocessˉcontract.CLIENT_CODE_PAGES;
        var Memoryˉpageˉbudget = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_MEMORY_PAGE_BUDGET
            : Kernelˉprocessˉcontract.CLIENT_MEMORY_PAGE_BUDGET;
        var Instructionˉbudget = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_INSTRUCTION_BUDGET
            : Kernelˉprocessˉcontract.CLIENT_INSTRUCTION_BUDGET;
        var Runtimeˉkind = Isˉinit
            ? Kernelˉprocessˉcontract.RUNTIME_KIND_AOT_SERVICE
            : Kernelˉprocessˉcontract.RUNTIME_KIND_BYTECODE_INTERPRETER;
        var Runtimeˉprofile = Isˉinit
            ? Kernelˉprocessˉcontract.RUNTIME_PROFILE_BOOT_RESOURCE_OWNER
            : Kernelˉprocessˉcontract.RUNTIME_PROFILE_GRANTED_BOOT_RESOURCE_INTERPRETER;
        var Syscallˉbudget = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_SYSCALL_BUDGET
            : Kernelˉprocessˉcontract.CLIENT_SYSCALL_BUDGET;
        output.Emit(0x4C, 0x89, 0xEF, 0x31, 0xC0, 0xB9);
        output.Emitˉu32(Kernelˉprocessˉcontract.RECORD_BYTES / sizeof(ulong));
        output.Emit(0xFC, 0xF3, 0x48, 0xAB);
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(Kernelˉprocessˉcontract.RECORD_MAGIC);
        output.Emit(0x49, 0x89, 0x45, 0x00);
        Emitˉstoreˉrecordˉu32(output, 8, Kernelˉprocessˉcontract.RECORD_VERSION);
        Emitˉstoreˉrecordˉu32(output, 12, Kernelˉprocessˉcontract.RECORD_BYTES);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_READY);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_READY);
        Emitˉstoreˉrecordˉu32(output, 24, processˉid);
        Emitˉstoreˉrecordˉu32(output, 28, threadˉid);
        for (var Offset = 0; Offset < digest.Length; Offset += sizeof(ulong))
        {
            output.Emit(0x48, 0xB8);
            output.Emitˉu64(BinaryPrimitives.ReadUInt64LittleEndian(digest.AsSpan().Slice(Offset)));
            output.Emit(0x49, 0x89, 0x85);
            output.Emitˉu32(checked((uint)(32 + Offset)));
        }
        output.Emit(0x4D, 0x89, 0xB5);
        output.Emitˉu32(Kernelˉprocessˉcontract.ROOT_ADDRESS_OFFSET);
        Emitˉstoreˉrecordˉaddress(output, Kernelˉprocessˉcontract.USER_CODE_ADDRESS_OFFSET,
            Kernelˉprocessˉcontract.USER_CODE_PAGE);
        Emitˉstoreˉrecordˉaddress(output, Kernelˉprocessˉcontract.USER_STACK_ADDRESS_OFFSET,
            Stackˉpage);
        Emitˉstoreˉrecordˉaddress(output, Kernelˉprocessˉcontract.USER_DATA_ADDRESS_OFFSET,
            Dataˉpage);
        Emitˉstoreˉrecordˉu32(output, 96, Memoryˉpageˉbudget);
        Emitˉstoreˉrecordˉu32(output, 100, Instructionˉbudget);
        Emitˉstoreˉrecordˉu32(output, 104, Kernelˉprocessˉcontract.HANDLE_BUDGET);
        Emitˉstoreˉrecordˉu32(
            output, Kernelˉprocessˉcontract.SYSCALL_BUDGET_OFFSET, Syscallˉbudget);
        Emitˉstoreˉrecordˉu32(output, 112, Kernelˉprocessˉcontract.CAPABILITY_SLOT);
        Emitˉstoreˉrecordˉu32(output, 116, Kernelˉprocessˉcontract.CAPABILITY_GENERATION);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.CAPABILITY_RIGHTS_OFFSET,
            capabilityˉrights);
        Emitˉstoreˉrecordˉu32(output, 124, Kernelˉprocessˉcontract.CHANNEL_CAPACITY);
        output.Emit(0x49, 0x8D, 0x84, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET);
        output.Emit(0x49, 0x89, 0x85);
        output.Emitˉu32(Kernelˉprocessˉcontract.CHANNEL_ADDRESS_OFFSET);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.ROLE_OFFSET, role);
        Emitˉstoreˉrecordˉu32(
            output, Kernelˉprocessˉcontract.STACK_PAGE_COUNT_OFFSET, checked((uint)Stackˉpages));
        Emitˉstoreˉrecordˉu32(
            output, Kernelˉprocessˉcontract.RUNTIME_PROFILE_OFFSET, Runtimeˉprofile);
        for (var Offset = 0; Offset < programˉdigest.Length; Offset += sizeof(ulong))
        {
            output.Emit(0x48, 0xB8);
            output.Emitˉu64(BinaryPrimitives.ReadUInt64LittleEndian(programˉdigest.AsSpan().Slice(Offset)));
            output.Emit(0x49, 0x89, 0x85);
            output.Emitˉu32(checked(Kernelˉprocessˉcontract.PROGRAM_DIGEST_OFFSET + (uint)Offset));
        }
        Emitˉstoreˉrecordˉu32(
            output, Kernelˉprocessˉcontract.CODE_PAGE_COUNT_OFFSET, checked((uint)Codeˉpages));
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.RUNTIME_KIND_OFFSET, Runtimeˉkind);
    }

    private static void Emitˉcopyˉkernelˉtables(X64ˉcodeˉbuilder output)
    {
        output.Emit(0x49, 0x8B, 0xB4, 0x24);
        output.Emitˉu32(Kernelˉpagingˉcontract.RECORD_OFFSET + 16);
        output.Emit(0x4C, 0x89, 0xF7, 0xB9, 0x00, 0x02, 0x00, 0x00, 0xFC, 0xF3, 0x48, 0xA5);
        output.Emit(0x49, 0x8B, 0xB4, 0x24);
        output.Emitˉu32(Kernelˉpagingˉcontract.RECORD_OFFSET + 16);
        output.Emit(0x48, 0x81, 0xC6, 0x00, 0x10, 0x00, 0x00, 0x49, 0x8D, 0xBE, 0x00, 0x10, 0x00, 0x00,
            0xB9, 0x00, 0x02, 0x00, 0x00, 0xF3, 0x48, 0xA5);
        output.Emit(0x49, 0x8B, 0xB4, 0x24);
        output.Emitˉu32(Kernelˉpagingˉcontract.RECORD_OFFSET + 16);
        output.Emit(0x48, 0x81, 0xC6, 0x00, 0x20, 0x00, 0x00, 0x49, 0x8D, 0xBE, 0x00, 0x20, 0x00, 0x00,
            0xB9, 0x00, 0x02, 0x00, 0x00, 0xF3, 0x48, 0xA5);
    }

    private static void Emitˉpopulateˉprocessˉtable(
        X64ˉcodeˉbuilder output,
        string labelˉsuffix,
        ulong codeˉpages,
        ulong stackˉpage,
        ulong stackˉpages,
        ulong dataˉpage,
        ulong runtimeˉinputˉpage,
        bool runtimeˉinputˉpresent)
    {
        var Loopˉlabel = $"{PT_LOOP_LABEL}_{labelˉsuffix}";
        var Nullˉdoneˉlabel = $"{PT_NULL_DONE_LABEL}_{labelˉsuffix}";
        output.Emit(0x49, 0x8D, 0x86, 0x07, 0x10, 0x00, 0x00, 0x49, 0x89, 0x06);
        output.Emit(0x49, 0x8D, 0x86, 0x07, 0x20, 0x00, 0x00, 0x49, 0x89, 0x86, 0x00, 0x10, 0x00, 0x00);
        output.Emit(0x4C, 0x89, 0xF3, 0x48, 0x81, 0xE3, 0x00, 0x00, 0xE0, 0xFF);
        output.Emit(0x4D, 0x8D, 0xBE, 0x00, 0x30, 0x00, 0x00, 0x48, 0x89, 0xD8);
        output.Emit(0x48, 0x83, 0xC8, 0x03, 0x48, 0x0F, 0xBA, 0xE8, 0x3F);
        output.Emit(0xB9, 0x00, 0x02, 0x00, 0x00);
        output.Mark(Loopˉlabel);
        output.Emit(0x49, 0x89, 0x07, 0x49, 0x83, 0xC7, 0x08, 0x48, 0x05, 0x00, 0x10, 0x00, 0x00, 0xFF, 0xC9);
        output.Jumpˉif(CONDITION_NOT_EQUAL, Loopˉlabel);
        output.Emit(0x4D, 0x8D, 0xBE, 0x00, 0x30, 0x00, 0x00);
        output.Emit(0x48, 0x85, 0xDB);
        output.Jumpˉif(CONDITION_NOT_EQUAL, Nullˉdoneˉlabel);
        output.Emit(0x49, 0xC7, 0x86, 0x00, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00);
        output.Mark(Nullˉdoneˉlabel);
        output.Emit(0x4C, 0x89, 0xF0, 0x48, 0xC1, 0xE8, 0x15, 0x48, 0xC1, 0xE0, 0x03,
            0x49, 0x8D, 0x96, 0x07, 0x30, 0x00, 0x00, 0x49, 0x89, 0x94, 0x06, 0x00, 0x20, 0x00, 0x00);
        for (ulong Page = 0; Page < codeˉpages; Page++)
        {
            Emitˉwriteˉuserˉpte(output, Kernelˉprocessˉcontract.USER_CODE_PAGE + Page,
                Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER);
        }
        for (ulong Page = 0; Page < stackˉpages; Page++)
        {
            Emitˉwriteˉuserˉpte(output, stackˉpage + Page,
                Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉpagingˉcontract.ENTRY_WRITABLE |
                Kernelˉprocessˉcontract.ENTRY_USER | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        }
        Emitˉwriteˉuserˉpte(output, dataˉpage,
            Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉpagingˉcontract.ENTRY_WRITABLE |
            Kernelˉprocessˉcontract.ENTRY_USER | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        if (runtimeˉinputˉpresent)
        {
            Emitˉwriteˉuserˉpte(output, runtimeˉinputˉpage,
                Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER |
                Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        }
        else
        {
            Emitˉclearˉprocessˉpte(output, runtimeˉinputˉpage);
        }
    }

    private static void Emitˉcopyˉuserˉbytes(
        X64ˉcodeˉbuilder output,
        ImmutableArray<Objectˉrelocation>.Builder relocations,
        int byteˉcount,
        uint symbolˉindex,
        ulong destinationˉpage)
    {
        output.Emit(0x48, 0x8D, 0x35);
        var Field = output.Position;
        output.Emitˉu32(0);
        relocations.Add(Relocation(Field, symbolˉindex));
        output.Emit(0x49, 0x8D, 0xBE);
        output.Emitˉu32(checked((uint)(destinationˉpage * Kernelˉpagingˉcontract.PAGE_BYTES)));
        output.Emit(0xB9);
        output.Emitˉu32((uint)byteˉcount);
        output.Emit(0xFC, 0xF3, 0xA4);
    }

    private static void Emitˉinitializeˉuserˉcontext(
        X64ˉcodeˉbuilder output,
        ulong dataˉpage,
        uint instructionˉbudget,
        uint callˉdepthˉbudget)
    {
        output.Emit(0x49, 0x8D, 0xBE);
        output.Emitˉu32(checked((uint)(dataˉpage * Kernelˉpagingˉcontract.PAGE_BYTES)));
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(((ulong)Nativeˉexecutionˉcontextˉcontract.SIZE << 32) |
            Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION);
        output.Emit(0x48, 0x89, 0x07, 0x48, 0xC7, 0x47, 0x08);
        output.Emitˉu32(instructionˉbudget);
        output.Emit(0x48, 0xC7, 0x47, 0x10);
        output.Emitˉu32(callˉdepthˉbudget);
    }

    private static void Emitˉinstallˉdescriptorˉstate(
        X64ˉcodeˉbuilder output,
        ImmutableArray<Objectˉrelocation>.Builder relocations)
    {
        // Private GDT: null, kernel code/data, user data/code, and one 64-bit TSS.
        output.Emit(0x49, 0x8D, 0xBC, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.GDT_OFFSET);
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(0x00AF_9A00_0000_FFFF);
        output.Emit(0x48, 0x89, 0x47, 0x08, 0x48, 0xB8);
        output.Emitˉu64(0x00CF_9200_0000_FFFF);
        output.Emit(0x48, 0x89, 0x47, 0x10, 0x48, 0xB8);
        output.Emitˉu64(0x00CF_F200_0000_FFFF);
        output.Emit(0x48, 0x89, 0x47, 0x18, 0x48, 0xB8);
        output.Emitˉu64(0x00AF_FA00_0000_FFFF);
        output.Emit(0x48, 0x89, 0x47, 0x20);
        output.Emit(0x49, 0x8D, 0x84, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.TSS_OFFSET);
        output.Emit(0x48, 0x89, 0xC2, 0x48, 0x25, 0xFF, 0xFF, 0xFF, 0x00, 0x48, 0xC1, 0xE0, 0x10,
            0x48, 0xB9);
        output.Emitˉu64(0x0000_8900_0000_0067);
        output.Emit(0x48, 0x09, 0xC8, 0x48, 0x89, 0xD1, 0x48, 0x81, 0xE1, 0x00, 0x00, 0x00, 0xFF,
            0x48, 0xC1, 0xE1, 0x20, 0x48, 0x09, 0xC8, 0x48, 0x89, 0x47, 0x28,
            0x48, 0xC1, 0xEA, 0x20, 0x89, 0x57, 0x30);
        output.Emit(0x49, 0x89, 0xA4, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.TSS_OFFSET + 4);
        output.Emit(0x66, 0x41, 0xC7, 0x84, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.TSS_OFFSET + 102);
        output.Emit((byte)Kernelˉprocessˉcontract.TSS_BYTES, 0x00);
        output.Emit(0x66, 0x41, 0xC7, 0x84, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.GDTR_OFFSET);
        output.Emit((byte)(Kernelˉprocessˉcontract.GDT_BYTES - 1), 0x00);
        output.Emit(0x49, 0x8D, 0x84, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.GDT_OFFSET);
        output.Emit(0x49, 0x89, 0x84, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.GDTR_OFFSET + 2);

        // Reuse the existing kernel-owned zeroed IDT page, extending it through
        // page fault and routing 6/13/14 through WVA-normalized process stubs.
        output.Emit(0x4D, 0x8B, 0x44, 0x24, 0x38);
        Emitˉgate(output, relocations, Kernelˉexceptionˉcontract.INVALID_OPCODE_GATE_OFFSET, 12);
        Emitˉgate(output, relocations, Kernelˉexceptionˉcontract.GENERAL_PROTECTION_GATE_OFFSET, 10);
        Emitˉgate(output, relocations, 14 * Kernelˉexceptionˉcontract.IDT_GATE_BYTES, 11);
        output.Emit(0x66, 0x41, 0xC7, 0x80);
        output.Emitˉu32(IDT_DESCRIPTOR_OFFSET);
        output.Emit((byte)IDT_LIMIT, (byte)(IDT_LIMIT >> 8));
        output.Emit(0x4D, 0x89, 0x80);
        output.Emitˉu32(IDT_DESCRIPTOR_OFFSET + 2);
        output.Emit(0xFA, 0x41, 0x0F, 0x01, 0x98);
        output.Emitˉu32(IDT_DESCRIPTOR_OFFSET);
        output.Emit(0x41, 0x0F, 0x01, 0x94, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.GDTR_OFFSET);
        output.Emit(0xB8, (byte)KERNEL_DATA_SELECTOR, 0x00, 0x00, 0x00,
            0x8E, 0xD8, 0x8E, 0xC0, 0x8E, 0xD0,
            0xB8, (byte)TSS_SELECTOR, 0x00, 0x00, 0x00, 0x0F, 0x00, 0xD8);
    }

    private static void Emitˉconfigureˉsyscallˉmsrs(X64ˉcodeˉbuilder output)
    {
        output.Emit(0xB8);
        output.Emitˉu32(0x8000_0000);
        output.Emit(0x0F, 0xA2, 0x3D);
        output.Emitˉu32(0x8000_0001);
        output.Jumpˉif(CONDITION_BELOW, FAILURE_LABEL);
        output.Emit(0xB8);
        output.Emitˉu32(0x8000_0001);
        output.Emit(0x0F, 0xA2, 0xF7, 0xC2);
        output.Emitˉu32(1U << 11);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        output.Emit(0xB9);
        output.Emitˉu32(0xC000_0080);
        output.Emit(0x0F, 0x32, 0x0F, 0xBA, 0xE8, 0x00, 0x0F, 0x30);
        output.Emit(0xB9);
        output.Emitˉu32(0xC000_0081);
        output.Emit(0x31, 0xC0, 0xBA);
        output.Emitˉu32(((uint)KERNEL_DATA_SELECTOR << 16) | KERNEL_CODE_SELECTOR);
        output.Emit(0x0F, 0x30);
        output.Loadˉripˉrelativeˉrdx("process_syscall_machine_entry");
        output.Emit(0x48, 0x89, 0xD0, 0x48, 0xC1, 0xEA, 0x20, 0xB9);
        output.Emitˉu32(0xC000_0082);
        output.Emit(0x0F, 0x30, 0xB9);
        output.Emitˉu32(0xC000_0084);
        output.Emit(0xB8, 0x00, 0x07, 0x00, 0x00, 0x31, 0xD2, 0x0F, 0x30);
        output.Emit(0xB9);
        output.Emitˉu32(0xC000_0101);
        output.Emit(0x31, 0xC0, 0x31, 0xD2, 0x0F, 0x30);
    }

    private static void Emitˉactivateˉrecordˉroot(
        X64ˉcodeˉbuilder output,
        ImmutableArray<Objectˉrelocation>.Builder relocations)
    {
        output.Emit(0x49, 0x8B, 0x85);
        output.Emitˉu32(Kernelˉprocessˉcontract.ROOT_ADDRESS_OFFSET);
        Emitˉexternalˉcall(output, relocations, 9);
        output.Emit(0x49, 0x3B, 0x85);
        output.Emitˉu32(Kernelˉprocessˉcontract.ROOT_ADDRESS_OFFSET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
    }

    private static void Emitˉsetˉkernelˉgsˉbase(X64ˉcodeˉbuilder output)
    {
        output.Emit(0x4C, 0x89, 0xE8, 0x48, 0x89, 0xC2, 0x48, 0xC1, 0xEA, 0x20, 0xB9);
        output.Emitˉu32(0xC000_0102);
        output.Emit(0x0F, 0x30);
    }

    private static void Emitˉenterˉinitialˉprocess(
        X64ˉcodeˉbuilder output,
        string resumeˉlabel,
        ulong stackˉpages)
    {
        output.Emit(0x49, 0x89, 0xA5);
        output.Emitˉu32(Kernelˉprocessˉcontract.KERNEL_STACK_OFFSET);
        output.Loadˉripˉrelativeˉrdx(resumeˉlabel);
        output.Emit(0x49, 0x89, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_RUNNING);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_RUNNING);
        output.Emit(0x49, 0x8B, 0x8D);
        output.Emitˉu32(Kernelˉprocessˉcontract.USER_CODE_ADDRESS_OFFSET);
        output.Emit(0x49, 0x8B, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.USER_DATA_ADDRESS_OFFSET);
        output.Emit(0x49, 0x8B, 0xA5);
        output.Emitˉu32(Kernelˉprocessˉcontract.USER_STACK_ADDRESS_OFFSET);
        output.Emit(0x48, 0x81, 0xC4);
        output.Emitˉu32(checked((uint)(stackˉpages * Kernelˉpagingˉcontract.PAGE_BYTES)));
        output.Emit(0x49, 0xC7, 0xC3, 0x02, 0x00, 0x00, 0x00, 0x48, 0x0F, 0x07);
    }

    private static void Emitˉresumeˉsavedˉprocess(X64ˉcodeˉbuilder output)
    {
        output.Emit(0x49, 0x8B, 0x8D);
        output.Emitˉu32(Kernelˉprocessˉcontract.USER_INSTRUCTION_POINTER_OFFSET);
        output.Emit(0x4D, 0x8B, 0x9D);
        output.Emitˉu32(Kernelˉprocessˉcontract.USER_FLAGS_OFFSET);
        output.Emit(0x49, 0x8B, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.USER_CONTEXT_POINTER_OFFSET);
        output.Emit(0x49, 0x8B, 0xA5);
        output.Emitˉu32(Kernelˉprocessˉcontract.USER_STACK_POINTER_OFFSET);
        output.Emit(0x48, 0x0F, 0x07);
    }

    private static void Emitˉgate(
        X64ˉcodeˉbuilder output,
        ImmutableArray<Objectˉrelocation>.Builder relocations,
        uint offset,
        uint symbolˉindex)
    {
        output.Emit(0x48, 0x8D, 0x15);
        var Field = output.Position;
        output.Emitˉu32(0);
        relocations.Add(Relocation(Field, symbolˉindex));
        output.Emit(0x66, 0x41, 0x89, 0x90);
        output.Emitˉu32(offset);
        output.Emit(0x66, 0x41, 0xC7, 0x80);
        output.Emitˉu32(offset + 2);
        output.Emit((byte)KERNEL_CODE_SELECTOR, 0x00);
        output.Emit(0x41, 0xC6, 0x80);
        output.Emitˉu32(offset + 4);
        output.Emit(0x00, 0x41, 0xC6, 0x80);
        output.Emitˉu32(offset + 5);
        output.Emit(Kernelˉexceptionˉcontract.INTERRUPT_GATE_ATTRIBUTES);
        output.Emit(0x48, 0xC1, 0xEA, 0x10, 0x66, 0x41, 0x89, 0x90);
        output.Emitˉu32(offset + 6);
        output.Emit(0x48, 0xC1, 0xEA, 0x10, 0x41, 0x89, 0x90);
        output.Emitˉu32(offset + 8);
        output.Emit(0x41, 0xC7, 0x80);
        output.Emitˉu32(offset + 12);
        output.Emitˉu32(0);
    }

    private static void Emitˉwriteˉuserˉpte(X64ˉcodeˉbuilder output, ulong page, ulong flags)
    {
        output.Emit(0x49, 0x8D, 0x86);
        output.Emitˉu32(checked((uint)(page * Kernelˉpagingˉcontract.PAGE_BYTES)));
        output.Emit(0x48, 0x89, 0xC2, 0x48, 0x29, 0xDA, 0x48, 0xC1, 0xEA, 0x0C, 0x48, 0xB9);
        output.Emitˉu64(flags);
        output.Emit(0x48, 0x09, 0xC8, 0x49, 0x89, 0x84, 0xD7, 0x00, 0x00, 0x00, 0x00);
    }

    private static void Emitˉclearˉprocessˉpte(X64ˉcodeˉbuilder output, ulong page)
    {
        output.Emit(0x49, 0x8D, 0x86);
        output.Emitˉu32(checked((uint)(page * Kernelˉpagingˉcontract.PAGE_BYTES)));
        output.Emit(0x48, 0x29, 0xD8, 0x48, 0xC1, 0xE8, 0x0C,
            0x49, 0xC7, 0x84, 0xC7, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00);
    }

    private static void Emitˉstoreˉrecordˉaddress(X64ˉcodeˉbuilder output, uint offset, ulong page)
    {
        output.Emit(0x49, 0x8D, 0x86);
        output.Emitˉu32(checked((uint)(page * Kernelˉpagingˉcontract.PAGE_BYTES)));
        output.Emit(0x49, 0x89, 0x85);
        output.Emitˉu32(offset);
    }

    private static void Emitˉvalidateˉcapability(X64ˉcodeˉbuilder output, uint right)
    {
        output.Emit(0x81, 0xFE);
        output.Emitˉu32(Kernelˉprocessˉcontract.CAPABILITY_REFERENCE);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        output.Emit(0x65, 0xF7, 0x04, 0x25);
        output.Emitˉu32(Kernelˉprocessˉcontract.CAPABILITY_RIGHTS_OFFSET);
        output.Emitˉu32(right);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_FAILURE_LABEL);
    }

    private static void Emitˉgrantˉbootˉresource(X64ˉcodeˉbuilder output)
    {
        Emitˉloadˉgsˉchannelˉr10(output);
        output.Emit(0x4D, 0x8D, 0x4A,
            checked((byte)(Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET -
                Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET)));
        Emitˉvalidateˉresourceˉheader(output, SYSCALL_FAILURE_LABEL);
        Emitˉcompareˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_STATE_OWNED);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉcompareˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_ID_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉcompareˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_OWNER_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉcompareˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉcompareˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_FLAGS_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_FLAGS);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉcompareˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉcompareˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);

        output.Emit(0x49, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_SOURCE_ADDRESS_OFFSET);
        output.Emit(0x48, 0x85, 0xC0);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_FAILURE_LABEL);
        output.Emit(0x48, 0xA9, 0xFF, 0x0F, 0x00, 0x00);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        output.Emit(0x41, 0x83, 0x79,
            (byte)Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET, 12);
        output.Jumpˉif(CONDITION_BELOW, SYSCALL_FAILURE_LABEL);
        output.Emit(0x41, 0x81, 0x79,
            (byte)Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET);
        output.Emitˉu32(Kernelˉprocessˉcontract.MAXIMUM_RUNTIME_INPUT_BYTES);
        output.Jumpˉif(CONDITION_ABOVE, SYSCALL_FAILURE_LABEL);
        foreach (var Offset in new[]
        {
            Kernelˉprocessˉcontract.RESOURCE_TARGET_ROOT_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_TARGET_DATA_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_TARGET_ADDRESS_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_SERVICE_ADDRESS_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_TARGET_PTE_OFFSET,
        })
        {
            output.Emit(0x49, 0x83, 0x79, checked((byte)Offset), 0x00);
            output.Jumpˉif(CONDITION_EQUAL, SYSCALL_FAILURE_LABEL);
        }
        output.Emit(0x4D, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_PTE_OFFSET);
        output.Emit(0x49, 0x83, 0x38, 0x00);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);

        output.Emit(0x49, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_SOURCE_ADDRESS_OFFSET);
        output.Emit(0x48, 0x83, 0xC8,
            (byte)(Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER));
        output.Emit(0x48, 0x0F, 0xBA, 0xE8, 0x3F, 0x49, 0x89, 0x00);

        output.Emit(0x4D, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_DATA_OFFSET);
        output.Emit(0x49, 0x8D, 0x80);
        output.Emitˉu32(Kernelˉprocessˉcontract.RUNTIME_SERVICE_TABLE_OFFSET);
        output.Emit(0x49, 0x89, 0x40,
            (byte)Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET);
        output.Emit(0x49, 0x8D, 0x80);
        output.Emitˉu32(Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET);
        output.Emit(0x49, 0x89, 0x40,
            (byte)Nativeˉexecutionˉcontextˉcontract.FILE_INPUT_TABLE_POINTER_OFFSET);
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(((ulong)Nativeˉserviceˉtableˉcontract.SIZE << 32) |
            Nativeˉserviceˉtableˉcontract.FORMAT_VERSION);
        output.Emit(0x49, 0x89, 0x80);
        output.Emitˉu32(Kernelˉprocessˉcontract.RUNTIME_SERVICE_TABLE_OFFSET);
        output.Emit(0x49, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_SERVICE_ADDRESS_OFFSET);
        output.Emit(0x49, 0x89, 0x80);
        output.Emitˉu32(Kernelˉprocessˉcontract.RUNTIME_SERVICE_TABLE_OFFSET +
            Nativeˉserviceˉtableˉcontract.FILE_READ_BYTES_POINTER_OFFSET);
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(((ulong)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_VERSION << 32) |
            Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_MAGIC);
        output.Emit(0x49, 0x89, 0x80);
        output.Emitˉu32(Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET);
        output.Emit(0x41, 0xC7, 0x80);
        output.Emitˉu32(Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET + 8);
        output.Emitˉu32(Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_BYTES);
        output.Emit(0x49, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_ADDRESS_OFFSET);
        output.Emit(0x49, 0x89, 0x80);
        output.Emitˉu32(Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET +
            Kernelˉprocessˉcontract.BOOT_RESOURCE_DATA_POINTER_OFFSET);
        output.Emit(0x41, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET);
        output.Emit(0x41, 0x89, 0x80);
        output.Emitˉu32(Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET +
            Kernelˉprocessˉcontract.BOOT_RESOURCE_DATA_LENGTH_OFFSET);

        Emitˉstoreˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_STATE_BORROWED);
        Emitˉstoreˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET,
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID);
        Emitˉstoreˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET, 1);
        Emitˉstoreˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET, 1);
    }

    private static void Emitˉvalidateˉborrowedˉresource(
        X64ˉcodeˉbuilder output,
        string failureˉlabel,
        uint resourceˉlength)
    {
        output.Emit(0x4D, 0x8D, 0x8C, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET);
        Emitˉvalidateˉresourceˉheader(output, failureˉlabel);
        foreach (var Field in new (uint Offset, uint Value)[]
        {
            (Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET,
                Kernelˉprocessˉcontract.RESOURCE_STATE_BORROWED),
            (Kernelˉprocessˉcontract.RESOURCE_ID_OFFSET, Kernelˉprocessˉcontract.RESOURCE_ID),
            (Kernelˉprocessˉcontract.RESOURCE_OWNER_OFFSET, Kernelˉprocessˉcontract.INIT_PROCESS_ID),
            (Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET,
                Kernelˉprocessˉcontract.CLIENT_PROCESS_ID),
            (Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET, resourceˉlength),
            (Kernelˉprocessˉcontract.RESOURCE_FLAGS_OFFSET, Kernelˉprocessˉcontract.RESOURCE_FLAGS),
            (Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET, 1),
            (Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET, 1),
        })
        {
            Emitˉcompareˉresourceˉu32(output, Field.Offset, Field.Value);
            output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        }
        output.Emit(0x49, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_SOURCE_ADDRESS_OFFSET);
        output.Emit(0x4D, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_PTE_OFFSET);
        output.Emit(0x48, 0x83, 0xC8,
            (byte)(Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER));
        output.Emit(0x48, 0x0F, 0xBA, 0xE8, 0x3F, 0x49, 0x39, 0x00);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);

        output.Emit(0x4D, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_DATA_OFFSET);
        output.Emit(0x49, 0x8D, 0x90);
        output.Emitˉu32(Kernelˉprocessˉcontract.RUNTIME_SERVICE_TABLE_OFFSET);
        output.Emit(0x49, 0x39, 0x50,
            (byte)Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        output.Emit(0x49, 0x8D, 0x90);
        output.Emitˉu32(Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET);
        output.Emit(0x49, 0x39, 0x50,
            (byte)Nativeˉexecutionˉcontextˉcontract.FILE_INPUT_TABLE_POINTER_OFFSET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(((ulong)Nativeˉserviceˉtableˉcontract.SIZE << 32) |
            Nativeˉserviceˉtableˉcontract.FORMAT_VERSION);
        output.Emit(0x49, 0x39, 0x80);
        output.Emitˉu32(Kernelˉprocessˉcontract.RUNTIME_SERVICE_TABLE_OFFSET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        output.Emit(0x49, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_SERVICE_ADDRESS_OFFSET);
        output.Emit(0x49, 0x39, 0x80);
        output.Emitˉu32(Kernelˉprocessˉcontract.RUNTIME_SERVICE_TABLE_OFFSET +
            Nativeˉserviceˉtableˉcontract.FILE_READ_BYTES_POINTER_OFFSET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(((ulong)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_VERSION << 32) |
            Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_MAGIC);
        output.Emit(0x49, 0x39, 0x80);
        output.Emitˉu32(Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        output.Emit(0x49, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_ADDRESS_OFFSET);
        output.Emit(0x49, 0x39, 0x80);
        output.Emitˉu32(Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET +
            Kernelˉprocessˉcontract.BOOT_RESOURCE_DATA_POINTER_OFFSET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        output.Emit(0x41, 0x81, 0xB8);
        output.Emitˉu32(Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET +
            Kernelˉprocessˉcontract.BOOT_RESOURCE_DATA_LENGTH_OFFSET);
        output.Emitˉu32(resourceˉlength);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
    }

    private static void Emitˉvalidateˉresourceˉheader(
        X64ˉcodeˉbuilder output,
        string failureˉlabel)
    {
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(Kernelˉprocessˉcontract.RESOURCE_MAGIC);
        output.Emit(0x49, 0x39, 0x01);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        Emitˉcompareˉresourceˉu32(output, 8, Kernelˉprocessˉcontract.RESOURCE_VERSION);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        Emitˉcompareˉresourceˉu32(output, 12, Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
    }

    private static void Emitˉcompareˉresourceˉu32(
        X64ˉcodeˉbuilder output,
        uint offset,
        uint value)
    {
        output.Emit(0x41, 0x81, 0x79, checked((byte)offset));
        output.Emitˉu32(value);
    }

    private static void Emitˉstoreˉresourceˉu32(
        X64ˉcodeˉbuilder output,
        uint offset,
        uint value)
    {
        output.Emit(0x41, 0xC7, 0x41, checked((byte)offset));
        output.Emitˉu32(value);
    }

    private static void Emitˉrequireˉsyscallˉbudget(X64ˉcodeˉbuilder output)
    {
        output.Emit(0x65, 0x8B, 0x14, 0x25);
        output.Emitˉu32(Kernelˉprocessˉcontract.SYSCALL_BUDGET_OFFSET);
        output.Emit(0x65, 0x39, 0x14, 0x25);
        output.Emitˉu32(Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET);
        output.Jumpˉif(CONDITION_ABOVE, SYSCALL_FAILURE_LABEL);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_FAILURE_LABEL);
    }

    private static void Emitˉcompareˉgsˉu32ˉtoˉgsˉu32(
        X64ˉcodeˉbuilder output,
        uint leftˉoffset,
        uint rightˉoffset)
    {
        output.Emit(0x65, 0x8B, 0x14, 0x25);
        output.Emitˉu32(rightˉoffset);
        output.Emit(0x65, 0x39, 0x14, 0x25);
        output.Emitˉu32(leftˉoffset);
    }

    private static void Emitˉstoreˉrecordˉu32(X64ˉcodeˉbuilder output, uint offset, uint value)
    {
        output.Emit(0x41, 0xC7, 0x85);
        output.Emitˉu32(offset);
        output.Emitˉu32(value);
    }

    private static void Emitˉcompareˉrecordˉu32(X64ˉcodeˉbuilder output, uint offset, uint value)
    {
        output.Emit(0x41, 0x81, 0xBD);
        output.Emitˉu32(offset);
        output.Emitˉu32(value);
    }

    private static void Emitˉstoreˉstateˉu32(X64ˉcodeˉbuilder output, uint offset, uint value)
    {
        output.Emit(0x41, 0xC7, 0x84, 0x24);
        output.Emitˉu32(offset);
        output.Emitˉu32(value);
    }

    private static void Emitˉloadˉgsˉchannelˉr10(X64ˉcodeˉbuilder output)
    {
        output.Emit(0x65, 0x4C, 0x8B, 0x14, 0x25);
        output.Emitˉu32(Kernelˉprocessˉcontract.CHANNEL_ADDRESS_OFFSET);
    }

    private static void Emitˉloadˉrecordˉchannelˉr10(X64ˉcodeˉbuilder output)
    {
        output.Emit(0x4D, 0x8B, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.CHANNEL_ADDRESS_OFFSET);
    }

    private static void Emitˉcompareˉchannelˉu32(X64ˉcodeˉbuilder output, uint offset, uint value)
    {
        output.Emit(0x41, 0x81, 0x7A, checked((byte)offset));
        output.Emitˉu32(value);
    }

    private static void Emitˉstoreˉchannelˉu32(X64ˉcodeˉbuilder output, uint offset, uint value)
    {
        output.Emit(0x41, 0xC7, 0x42, checked((byte)offset));
        output.Emitˉu32(value);
    }

    private static void Emitˉstoreˉchannelˉeax(X64ˉcodeˉbuilder output, uint offset) =>
        output.Emit(0x41, 0x89, 0x42, checked((byte)offset));

    private static void Emitˉloadˉchannelˉeax(X64ˉcodeˉbuilder output, uint offset) =>
        output.Emit(0x41, 0x8B, 0x42, checked((byte)offset));

    private static void Emitˉincrementˉchannelˉu32(X64ˉcodeˉbuilder output, uint offset) =>
        output.Emit(0x41, 0xFF, 0x42, checked((byte)offset));

    private static void Emitˉstoreˉstackˉrax(X64ˉcodeˉbuilder output, uint offset)
    {
        output.Emit(0x48, 0x89, 0x84, 0x24);
        output.Emitˉu32(offset);
    }

    private static void Emitˉstoreˉgsˉu32(X64ˉcodeˉbuilder output, uint offset, uint value)
    {
        output.Emit(0x65, 0xC7, 0x04, 0x25);
        output.Emitˉu32(offset);
        output.Emitˉu32(value);
    }

    private static void Emitˉcompareˉgsˉu32(X64ˉcodeˉbuilder output, uint offset, uint value)
    {
        output.Emit(0x65, 0x81, 0x3C, 0x25);
        output.Emitˉu32(offset);
        output.Emitˉu32(value);
    }

    private static void Emitˉincrementˉgsˉu32(X64ˉcodeˉbuilder output, uint offset)
    {
        output.Emit(0x65, 0xFF, 0x04, 0x25);
        output.Emitˉu32(offset);
    }

    private static void Emitˉstoreˉgsˉeax(X64ˉcodeˉbuilder output, uint offset)
    {
        output.Emit(0x65, 0x89, 0x04, 0x25);
        output.Emitˉu32(offset);
    }

    private static void Emitˉloadˉgsˉeax(X64ˉcodeˉbuilder output, uint offset)
    {
        output.Emit(0x65, 0x8B, 0x04, 0x25);
        output.Emitˉu32(offset);
    }

    private static void Emitˉstoreˉgsˉrsp(X64ˉcodeˉbuilder output, uint offset)
    {
        output.Emit(0x65, 0x48, 0x89, 0x24, 0x25);
        output.Emitˉu32(offset);
    }

    private static void Emitˉloadˉgsˉrsp(X64ˉcodeˉbuilder output, uint offset)
    {
        output.Emit(0x65, 0x48, 0x8B, 0x24, 0x25);
        output.Emitˉu32(offset);
    }

    private static void Emitˉstoreˉgsˉrcx(X64ˉcodeˉbuilder output, uint offset)
    {
        output.Emit(0x65, 0x48, 0x89, 0x0C, 0x25);
        output.Emitˉu32(offset);
    }

    private static void Emitˉloadˉgsˉrcx(X64ˉcodeˉbuilder output, uint offset)
    {
        output.Emit(0x65, 0x48, 0x8B, 0x0C, 0x25);
        output.Emitˉu32(offset);
    }

    private static void Emitˉstoreˉgsˉr11(X64ˉcodeˉbuilder output, uint offset)
    {
        output.Emit(0x65, 0x4C, 0x89, 0x1C, 0x25);
        output.Emitˉu32(offset);
    }

    private static void Emitˉloadˉgsˉr11(X64ˉcodeˉbuilder output, uint offset)
    {
        output.Emit(0x65, 0x4C, 0x8B, 0x1C, 0x25);
        output.Emitˉu32(offset);
    }

    private static void Emitˉstoreˉgsˉrdx(X64ˉcodeˉbuilder output, uint offset)
    {
        output.Emit(0x65, 0x48, 0x89, 0x14, 0x25);
        output.Emitˉu32(offset);
    }

    private static void Emitˉloadˉgsˉrdx(X64ˉcodeˉbuilder output, uint offset)
    {
        output.Emit(0x65, 0x48, 0x8B, 0x14, 0x25);
        output.Emitˉu32(offset);
    }

    private static void Emitˉjumpˉgs(X64ˉcodeˉbuilder output, uint offset)
    {
        output.Emit(0x65, 0xFF, 0x24, 0x25);
        output.Emitˉu32(offset);
    }

    private static void Emitˉexternalˉcall(
        X64ˉcodeˉbuilder output,
        ImmutableArray<Objectˉrelocation>.Builder relocations,
        uint symbolˉindex)
    {
        var Field = output.Emitˉcallˉplaceholder();
        relocations.Add(Relocation(Field, symbolˉindex));
    }

    private static Objectˉrelocation Relocation(uint offset, uint symbolˉindex) =>
        new(Objectˉrelocationˉkind.Relativeˉi32, 0, offset, symbolˉindex, -4);

    private static Objectˉsymbol Import(string name) =>
        new(name, Objectˉsymbolˉbinding.Import, Objectˉsymbolˉkind.Function,
            Objectˉlimits.UNDEFINED_SECTION, 0, 0);

    private static void Emitˉepilogue(X64ˉcodeˉbuilder output)
    {
        output.Emit(0x48, 0x81, 0xC4);
        output.Emitˉu32(FRAME_BYTES);
        output.Emit(0x41, 0x5F, 0x41, 0x5E, 0x41, 0x5D, 0x41, 0x5C, 0x5F, 0x5E, 0x5D, 0x5B, 0xC3);
    }

    private static void Verifyˉobject(
        ImmutableArray<byte> objectˉbytes,
        ImmutableArray<byte> code,
        ImmutableArray<byte> serviceˉimage,
        ImmutableArray<byte> clientˉimage,
        ImmutableArray<byte> runtimeˉinput,
        ImmutableArray<Objectˉrelocation> relocations)
    {
        var Object = Objectˉcodec.Readˉandˉverify(objectˉbytes.AsSpan()).Value;
        if (Object.Sections.Length != 4 ||
            Object.Sections[0].Kind != Objectˉsectionˉkind.Code ||
            Object.Sections[1].Kind != Objectˉsectionˉkind.Readˉonlyˉdata ||
            Object.Sections[2].Kind != Objectˉsectionˉkind.Readˉonlyˉdata ||
            Object.Sections[3].Kind != Objectˉsectionˉkind.Readˉonlyˉdata ||
            !Object.Sections[0].Data.AsSpan().SequenceEqual(code.AsSpan()) ||
            !Object.Sections[1].Data.AsSpan().SequenceEqual(serviceˉimage.AsSpan()) ||
            !Object.Sections[2].Data.AsSpan().SequenceEqual(clientˉimage.AsSpan()) ||
            !Object.Sections[3].Data.AsSpan().SequenceEqual(runtimeˉinput.AsSpan()) ||
            Object.Symbols.Length != 13 ||
            Object.Symbols[2].Name != "Windvale_resource_init_boot" ||
            Object.Symbols[3].Name != Kernelˉprocessˉcontract.ENTER_SYMBOL ||
            Object.Symbols[4].Name != Kernelˉprocessˉcontract.EXCEPTION_ENTRY_SYMBOL ||
            Object.Symbols[5].Name != Kernelˉprocessˉcontract.SYSCALL_ENTRY_SYMBOL ||
            !Object.Relocations.AsSpan().SequenceEqual(relocations.AsSpan()))
        {
            throw new InvalidOperationException(
                $"The protected-process object violated '{Kernelˉprocessˉcontract.TARGET_NAME}'.");
        }
    }

    private static ImmutableArray<byte> Readˉbootˉresourceˉleaf(
        ImmutableArray<byte> objectˉbytes)
    {
        if (objectˉbytes.IsDefault ||
            !Convert.ToHexString(SHA256.HashData(objectˉbytes.AsSpan())).Equals(
                BOOT_RESOURCE_SERVICE_OBJECT_SHA256,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The process machine seam received an unknown boot-resource service object.");
        }
        var Object = Objectˉcodec.Readˉandˉverify(objectˉbytes.AsSpan()).Value;
        if (Object.Sections.Length != 1 ||
            Object.Sections[0] is not
            {
                Kind: Objectˉsectionˉkind.Code,
                Data.Length: (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_SERVICE_BYTES,
            } ||
            Object.Symbols.Length != 1 ||
            Object.Symbols[0] is not
            {
                Name: Kernelˉprocessˉcontract.BOOT_RESOURCE_SERVICE_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
                Offset: 0,
                Size: Kernelˉprocessˉcontract.BOOT_RESOURCE_SERVICE_BYTES,
            } ||
            !Object.Relocations.IsEmpty)
        {
            throw new InvalidOperationException(
                "The process machine seam received a malformed boot-resource service object.");
        }
        return Object.Sections[0].Data;
    }
}
