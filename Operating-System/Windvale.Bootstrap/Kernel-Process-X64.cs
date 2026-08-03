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
        "ECB940ABB9DE8086D50AE418853021CF1F7566A9415A5A3A3B4E5CC45ED5E78C";
    private const string DIRECTORY_SERVICE_IMAGE_SHA256 =
        "BF25040B4925A13C4A919FFD5A53DE8FF281E4452132A9F7CD9BB3624740C883";
    private const string FAILURE_LABEL = "process_failure";
    private const string SERVICE_BLOCKED_LABEL = "process_service_blocked";
    private const string DIRECTORY_SERVICE_BLOCKED_LABEL = "process_directory_service_blocked";
    private const string CLIENT_COMPLETION_LABEL = "process_client_completion";
    private const string SERVICE_REPLY_BLOCKED_LABEL = "process_service_reply_blocked";
    private const string SERVICE_RESULT_BLOCKED_LABEL = "process_service_result_blocked";
    private const string CLIENT_DIRECTORY_COMPLETION_LABEL = "process_client_directory_completion";
    private const string SERVICE_DIRECTORY_REPLY_BLOCKED_LABEL =
        "process_service_directory_reply_blocked";
    private const string DIRECTORY_SECOND_REQUEST_BLOCKED_LABEL =
        "process_directory_second_request_blocked";
    private const string CLIENT_TERMINAL_LABEL = "process_client_terminal";
    private const string SERVICE_FAULT_CLIENT_TERMINAL_LABEL =
        "process_service_fault_client_terminal";
    private const string SECOND_SERVICE_BLOCKED_LABEL = "process_second_service_blocked";
    private const string SECOND_CLIENT_COMPLETION_LABEL = "process_second_client_completion";
    private const string SECOND_SERVICE_REPLY_BLOCKED_LABEL = "process_second_service_reply_blocked";
    private const string SECOND_SERVICE_RESULT_BLOCKED_LABEL =
        "process_second_service_result_blocked";
    private const string SECOND_CLIENT_DIRECTORY_COMPLETION_LABEL =
        "process_second_client_directory_completion";
    private const string SECOND_SERVICE_DIRECTORY_REPLY_BLOCKED_LABEL =
        "process_second_service_directory_reply_blocked";
    private const string DIRECTORY_EXITED_LABEL = "process_directory_exited";
    private const string SECOND_CLIENT_TERMINAL_LABEL = "process_second_client_terminal";
    private const string COMPLETION_LABEL = "process_completion";
    private const string PT_LOOP_LABEL = "process_pt_loop";
    private const string PT_NULL_DONE_LABEL = "process_pt_null_done";
    private const string SYSCALL_SEND_LABEL = "process_syscall_send";
    private const string SYSCALL_RECEIVE_LABEL = "process_syscall_receive";
    private const string SYSCALL_EXIT_LABEL = "process_syscall_exit";
    private const string SYSCALL_GRANT_RESOURCE_LABEL = "process_syscall_grant_resource";
    private const string SYSCALL_RECEIVE_SERVICE_REQUEST_LABEL = "process_syscall_receive_service_request";
    private const string SYSCALL_CALL_SERVICE_LABEL = "process_syscall_call_service";
    private const string SYSCALL_REPLY_SERVICE_REQUEST_LABEL = "process_syscall_reply_service_request";
    private const string SYSCALL_RESUME_LABEL = "process_syscall_resume";
    private const string SYSCALL_FAILURE_LABEL = "process_syscall_failure";
    private const string EXCEPTION_KERNEL_LABEL = "process_exception_kernel";
    private const string EXCEPTION_FAILURE_LABEL = "process_exception_failure";
    private const string DISPATCHER_LABEL = "process_ready_dispatcher";
    private const string COORDINATOR_START_LABEL = "process_coordinator_start";
    private const byte CONDITION_BELOW = 0x82;
    private const byte CONDITION_EQUAL = 0x84;
    private const byte CONDITION_NOT_EQUAL = 0x85;
    private const byte CONDITION_ABOVE = 0x87;
    private const uint POLICY_INSTRUCTION_BUDGET = 16_384;
    private const uint POLICY_CALL_DEPTH_BUDGET = 3;
    private const uint FRAME_BYTES = 0xE0;
    private const uint INIT_RECORD_SLOT_OFFSET = 0xB8;
    private const uint CLIENT_RECORD_SLOT_OFFSET = 0xC0;
    private const uint DIRECTORY_RECORD_SLOT_OFFSET = 0xC8;
    private const uint DISPATCH_CURSOR_SLOT_OFFSET = 0xD0;
    private const uint CONTEXT_OFFSET = 0x40;
    private const uint IDT_DESCRIPTOR_OFFSET = 15 * Kernelˉexceptionˉcontract.IDT_GATE_BYTES;
    private const ushort IDT_LIMIT = (ushort)(IDT_DESCRIPTOR_OFFSET - 1);
    private const ushort KERNEL_CODE_SELECTOR = 0x08;
    private const ushort KERNEL_DATA_SELECTOR = 0x10;
    private const ushort TSS_SELECTOR = 0x28;

    public static Kernelˉprocessˉx64ˉartifacts Build(
        Kernelˉprocessˉimageˉartifacts image,
        Kernelˉprocessˉscenario scenario)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (scenario is not Kernelˉprocessˉscenario.Normal and
            not Kernelˉprocessˉscenario.Userˉfault and
            not Kernelˉprocessˉscenario.Serviceˉfault)
        {
            throw new ArgumentOutOfRangeException(nameof(scenario));
        }
        if (Kernelˉprocessˉcontract.INIT_RECORD_OFFSET +
                Kernelˉprocessˉcontract.RECORD_BYTES > Kernelˉprocessˉcontract.GDT_OFFSET ||
            Kernelˉprocessˉcontract.GDT_OFFSET + Kernelˉprocessˉcontract.GDT_BYTES >
                Kernelˉprocessˉcontract.GDTR_OFFSET ||
            Kernelˉprocessˉcontract.GDTR_OFFSET + Kernelˉprocessˉcontract.GDTR_BYTES >
                Kernelˉprocessˉcontract.TSS_OFFSET ||
            Kernelˉprocessˉcontract.TSS_OFFSET + Kernelˉprocessˉcontract.TSS_BYTES >
                Kernelˉprocessˉcontract.CLIENT_RECORD_OFFSET ||
            Kernelˉprocessˉcontract.CLIENT_RECORD_OFFSET +
                Kernelˉprocessˉcontract.RECORD_BYTES >
                Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET ||
            Kernelˉprocessˉcontract.DIRECTORY_ENDPOINT_RECORD_OFFSET +
                Kernelˉprocessˉcontract.ENDPOINT_RECORD_BYTES >
                Kernelˉpagingˉcontract.PAGE_BYTES)
        {
            throw new InvalidOperationException(
                "The protected-process records overlap in the kernel state page.");
        }
        if (image.Scenario != scenario ||
            image.Initˉserviceˉimageˉbytes.IsEmpty ||
            (ulong)image.Initˉserviceˉimageˉbytes.Length >
                Kernelˉprocessˉcontract.INIT_CODE_PAGES * Kernelˉpagingˉcontract.PAGE_BYTES ||
            image.Directoryˉserviceˉimageˉbytes.IsEmpty ||
            (ulong)image.Directoryˉserviceˉimageˉbytes.Length >
                Kernelˉprocessˉcontract.DIRECTORY_CODE_PAGES * Kernelˉpagingˉcontract.PAGE_BYTES ||
            !Convert.ToHexString(SHA256.HashData(image.Directoryˉserviceˉimageˉbytes.AsSpan())).Equals(
                DIRECTORY_SERVICE_IMAGE_SHA256, StringComparison.Ordinal) ||
            image.Clientˉimageˉbytes.IsEmpty ||
            (ulong)image.Clientˉimageˉbytes.Length > Kernelˉprocessˉcontract.CLIENT_CODE_BYTES ||
            image.Initˉserviceˉdigest.Length != Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES ||
            image.Directoryˉserviceˉdigest.Length != Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES ||
            image.Interpreterˉdigest.Length != Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES ||
            image.Admittedˉprogramˉdigest.Length != Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES ||
            image.Admittedˉprogramˉbytes.Length is < 12 or >
                Kernelˉprocessˉcontract.MAXIMUM_RUNTIME_INPUT_BYTES ||
            !SHA256.HashData(image.Admittedˉprogramˉbytes.AsSpan()).AsSpan()
                .SequenceEqual(image.Admittedˉprogramˉdigest.AsSpan()) ||
            image.Executionˉbudgetˉbytes.Length != Kernelˉprocessˉcontract.EXECUTION_BUDGET_BYTES ||
            BinaryPrimitives.ReadUInt32LittleEndian(image.Executionˉbudgetˉbytes.AsSpan()) !=
                Kernelˉprocessˉcontract.EXECUTION_BUDGET ||
            image.Executionˉbudgetˉdigest.Length != Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES ||
            !SHA256.HashData(image.Executionˉbudgetˉbytes.AsSpan()).AsSpan()
                .SequenceEqual(image.Executionˉbudgetˉdigest.AsSpan()) ||
            image.Resourceˉstoreˉbytes.Length is < Resourceˉstoreˉcontract.HEADER_BYTES or >
                (int)Kernelˉpagingˉcontract.PAGE_BYTES ||
            image.Resourceˉstoreˉdigest.Length != Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES ||
            !SHA256.HashData(image.Resourceˉstoreˉbytes.AsSpan()).AsSpan()
                .SequenceEqual(image.Resourceˉstoreˉdigest.AsSpan()) ||
            image.Directoryˉsnapshotˉbytes.Length is < 68 or >
                (int)Kernelˉpagingˉcontract.PAGE_BYTES ||
            image.Directoryˉsnapshotˉdigest.Length != Kernelˉprocessˉcontract.MODULE_DIGEST_BYTES ||
            !SHA256.HashData(image.Directoryˉsnapshotˉbytes.AsSpan()).AsSpan()
                .SequenceEqual(image.Directoryˉsnapshotˉdigest.AsSpan()) ||
            image.Bootˉresourceˉserviceˉoffset > (uint)image.Clientˉimageˉbytes.Length ||
            Kernelˉprocessˉcontract.BOOT_RESOURCE_SERVICE_BYTES >
                (uint)image.Clientˉimageˉbytes.Length - image.Bootˉresourceˉserviceˉoffset)
        {
            throw new InvalidOperationException("The process machine seam received an invalid user image.");
        }
        try
        {
            _ = Directoryˉsnapshotˉcodec.Verify(image.Directoryˉsnapshotˉbytes.AsSpan());
        }
        catch (Directoryˉsnapshotˉexception Exception)
        {
            throw new InvalidOperationException(
                "The process machine seam received an invalid directory snapshot.", Exception);
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
        Emitˉenter(Output, Relocations, image, scenario);
        var Enterˉbytes = Output.Position;
        Output.Align(16);
        var Exceptionˉoffset = Output.Position;
        Emitˉexceptionˉentry(Output, Relocations, scenario);
        var Exceptionˉbytes = Output.Position - Exceptionˉoffset;
        Output.Align(16);
        var Syscallˉoffset = Output.Position;
        Emitˉsyscallˉentry(Output, image);
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
                new(".rodata.dbudget", Objectˉsectionˉkind.Readˉonlyˉdata, 4,
                    (uint)image.Executionˉbudgetˉbytes.Length, image.Executionˉbudgetˉbytes),
                new(".rodata.ewvrs", Objectˉsectionˉkind.Readˉonlyˉdata, 16,
                    (uint)image.Resourceˉstoreˉbytes.Length, image.Resourceˉstoreˉbytes),
                new(".rodata.fwvds", Objectˉsectionˉkind.Readˉonlyˉdata, 16,
                    (uint)image.Directoryˉsnapshotˉbytes.Length, image.Directoryˉsnapshotˉbytes),
                new(".rodata.gdirectory", Objectˉsectionˉkind.Readˉonlyˉdata, 16,
                    (uint)image.Directoryˉserviceˉimageˉbytes.Length,
                    image.Directoryˉserviceˉimageˉbytes),
            ],
            [
                new("Windvale_init_resource_user_image", Objectˉsymbolˉbinding.Local,
                    Objectˉsymbolˉkind.Data, 1, 0, (uint)image.Initˉserviceˉimageˉbytes.Length),
                new("Windvale_process_client_image", Objectˉsymbolˉbinding.Local,
                    Objectˉsymbolˉkind.Data, 2, 0, (uint)image.Clientˉimageˉbytes.Length),
                new("Windvale_resource_init_boot", Objectˉsymbolˉbinding.Local,
                    Objectˉsymbolˉkind.Data, 3, 0, (uint)image.Admittedˉprogramˉbytes.Length),
                new("Windvale_resource_init_budget", Objectˉsymbolˉbinding.Local,
                    Objectˉsymbolˉkind.Data, 4, 0, (uint)image.Executionˉbudgetˉbytes.Length),
                new("Windvale_resource_init_directory", Objectˉsymbolˉbinding.Local,
                    Objectˉsymbolˉkind.Data, 6, 0, (uint)image.Directoryˉsnapshotˉbytes.Length),
                new("Windvale_resource_init_store", Objectˉsymbolˉbinding.Local,
                    Objectˉsymbolˉkind.Data, 5, 0, (uint)image.Resourceˉstoreˉbytes.Length),
                new("Windvale_service_directory_user_image", Objectˉsymbolˉbinding.Local,
                    Objectˉsymbolˉkind.Data, 7, 0,
                    (uint)image.Directoryˉserviceˉimageˉbytes.Length),
                new(Kernelˉprocessˉcontract.ENTER_SYMBOL, Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function, 0, 0, Enterˉbytes),
                new(Kernelˉprocessˉcontract.EXCEPTION_ENTRY_SYMBOL, Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function, 0, Exceptionˉoffset, Exceptionˉbytes),
                new(Kernelˉprocessˉcontract.SYSCALL_ENTRY_SYMBOL, Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function, 0, Syscallˉoffset, Syscallˉbytes),
                Import(Kernelˉmemoryˉcontract.ALLOCATE_PAGES_SYMBOL),
                Import(Kernelˉprocessˉcontract.POLICY_SYMBOL),
                Import(Kernelˉmemoryˉcontract.RELEASE_TAIL_PAGES_SYMBOL),
                Import(Kernelˉexceptionˉcontract.TERMINAL_SYMBOL),
                Import(Kernelˉpagingˉcontract.PAGE_TABLE_ACTIVATE_SYMBOL),
                Import(Kernelˉprocessˉcontract.EXCEPTION_13_ENTRY_SYMBOL),
                Import(Kernelˉprocessˉcontract.EXCEPTION_14_ENTRY_SYMBOL),
                Import(Kernelˉprocessˉcontract.EXCEPTION_6_ENTRY_SYMBOL),
            ],
            Frozenˉrelocations);
        var Objectˉbytes = Objectˉcodec.Write(Object).ToImmutableArray();
        Verifyˉobject(Objectˉbytes, Code, image.Initˉserviceˉimageˉbytes,
            image.Directoryˉserviceˉimageˉbytes,
            image.Clientˉimageˉbytes, image.Admittedˉprogramˉbytes,
            image.Executionˉbudgetˉbytes, image.Resourceˉstoreˉbytes,
            image.Directoryˉsnapshotˉbytes, Frozenˉrelocations);
        return new(Objectˉbytes, Code, Frozenˉrelocations);
    }

    private static void Emitˉenter(
        X64ˉcodeˉbuilder output,
        ImmutableArray<Objectˉrelocation>.Builder relocations,
        Kernelˉprocessˉimageˉartifacts image,
        Kernelˉprocessˉscenario scenario)
    {
        var Userˉfault = scenario == Kernelˉprocessˉscenario.Userˉfault;
        var Serviceˉfault = scenario == Kernelˉprocessˉscenario.Serviceˉfault;
        var Clientˉsyscallˉbudget = Serviceˉfault
            ? Kernelˉprocessˉcontract.SERVICE_FAULT_CLIENT_SYSCALL_BUDGET
            : Kernelˉprocessˉcontract.CLIENT_SYSCALL_BUDGET;
        // Preserve every nonvolatile register and retain the fixed coordinator
        // frame throughout both CPL3 round trips.
        output.Emit(0x53, 0x55, 0x56, 0x57, 0x41, 0x54, 0x41, 0x55, 0x41, 0x56, 0x41, 0x57);
        output.Emit(0x48, 0x81, 0xEC);
        output.Emitˉu32(FRAME_BYTES);
        output.Emit(0xB8);
        output.Emitˉu32(2);
        Emitˉstoreˉstackˉrax(output, DISPATCH_CURSOR_SLOT_OFFSET);
        output.Jump(COORDINATOR_START_LABEL);
        Emitˉreadyˉdispatcher(output);
        output.Mark(COORDINATOR_START_LABEL);
        output.Emit(0x49, 0x89, 0xCC, 0x49, 0x83, 0xEC, (byte)Kernelˉmemoryˉcontract.HANDOFF_COPY_OFFSET);
        Emitˉvalidateˉmemoryˉstate(output);

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
        // Keep the generated policy's native frame below the coordinator frame.
        // Recover the aligned arena base from the owned stack and revalidate it
        // instead of trusting coordinator temporaries across this Stage 0 seam.
        output.Emit(0x48, 0x83, 0xEC, 0x28);
        Emitˉexternalˉcall(output, relocations, 11);
        output.Emit(0x48, 0x83, 0xC4, 0x28);
        output.Emit(0x49, 0x89, 0xE4, 0x49, 0x81, 0xE4);
        output.Emitˉu32(unchecked((uint)~(Kernelˉmemoryˉcontract.ARENA_ALIGNMENT_BYTES - 1)));
        output.Emit(0x48, 0x83, 0xF8, (byte)Kernelˉprocessˉcontract.POLICY_TOKEN);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉvalidateˉmemoryˉstate(output);
        output.Emit(0x4D, 0x8D, 0xAC, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.INIT_RECORD_OFFSET);
        output.Emit(0x4C, 0x89, 0xAC, 0x24);
        output.Emitˉu32(INIT_RECORD_SLOT_OFFSET);
        output.Emit(0x49, 0x8D, 0x84, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.CLIENT_RECORD_OFFSET);
        output.Emit(0x48, 0x89, 0x84, 0x24);
        output.Emitˉu32(CLIENT_RECORD_SLOT_OFFSET);
        output.Emit(0x49, 0x8D, 0x84, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.DIRECTORY_PROCESS_RECORD_OFFSET);
        output.Emit(0x48, 0x89, 0x84, 0x24);
        output.Emitˉu32(DIRECTORY_RECORD_SLOT_OFFSET);

        Emitˉinitializeˉchannel(output);
        Emitˉinitializeˉendpoint(output);
        Emitˉinitializeˉchannel(output, Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET);
        Emitˉinitializeˉendpoint(
            output,
            Kernelˉprocessˉcontract.DIRECTORY_ENDPOINT_RECORD_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_CAPABILITY_REFERENCE,
            Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE);

        // Build the init/resource-owner root first. The boot WVB is visible only
        // in this root until Windvale init authorizes one immutable borrow.
        Emitˉallocateˉextent(output, relocations, Kernelˉprocessˉcontract.INIT_ALLOCATION_PAGES);
        Emitˉinitializeˉrecord(
            output,
            image.Initˉserviceˉdigest,
            image.Admittedˉprogramˉdigest,
            Kernelˉprocessˉcontract.INIT_PROCESS_ID,
            Kernelˉprocessˉcontract.INIT_THREAD_ID,
            Kernelˉprocessˉcontract.INIT_PROCESS_GENERATION,
            Kernelˉprocessˉcontract.INIT_CAPABILITY_RIGHTS,
            Kernelˉprocessˉcontract.ROLE_INIT_SERVICE,
            Kernelˉprocessˉcontract.INIT_SYSCALL_BUDGET);
        Emitˉcopyˉkernelˉtables(output);
        Emitˉpopulateˉprocessˉtable(
            output, "service", Kernelˉprocessˉcontract.INIT_CODE_PAGES,
            Kernelˉprocessˉcontract.INIT_STACK_PAGE, Kernelˉprocessˉcontract.INIT_STACK_PAGES,
            Kernelˉprocessˉcontract.INIT_DATA_PAGE,
            Kernelˉprocessˉcontract.INIT_RUNTIME_INPUT_PAGE,
            Kernelˉprocessˉcontract.INIT_RUNTIME_BUDGET_PAGE,
            Kernelˉprocessˉcontract.INIT_SERVICE_RESPONSE_PAGE,
            runtimeˉinputˉpresent: true,
            storeˉpresent: true,
            directoryˉpresent: false,
            clearˉresourceˉaliases: false);
        Emitˉcopyˉuserˉbytes(
            output, relocations, image.Initˉserviceˉimageˉbytes.Length, 0,
            Kernelˉprocessˉcontract.USER_CODE_PAGE);
        Emitˉcopyˉuserˉbytes(
            output, relocations, image.Admittedˉprogramˉbytes.Length, 2,
            Kernelˉprocessˉcontract.INIT_RUNTIME_INPUT_PAGE);
        Emitˉcopyˉuserˉbytes(
            output, relocations, image.Executionˉbudgetˉbytes.Length, 3,
            Kernelˉprocessˉcontract.INIT_RUNTIME_BUDGET_PAGE);
        Emitˉcopyˉuserˉbytes(
            output, relocations, image.Resourceˉstoreˉbytes.Length, 5,
            Kernelˉprocessˉcontract.INIT_RESOURCE_STORE_PAGE);
        Emitˉinitializeˉuserˉcontext(
            output, Kernelˉprocessˉcontract.INIT_DATA_PAGE,
            Kernelˉprocessˉcontract.INIT_INSTRUCTION_BUDGET,
            Kernelˉprocessˉcontract.INIT_CALL_DEPTH_BUDGET,
            false);
        Emitˉinitializeˉstoreˉdescriptor(output, checked((uint)image.Resourceˉstoreˉbytes.Length));

        // The immutable directory provider has its own root, snapshot mapping,
        // endpoint, and request/reply pages before any client is runnable.
        Emitˉloadˉstackˉr13(output, DIRECTORY_RECORD_SLOT_OFFSET);
        Emitˉallocateˉextent(output, relocations, Kernelˉprocessˉcontract.DIRECTORY_ALLOCATION_PAGES);
        Emitˉinitializeˉrecord(
            output,
            image.Directoryˉserviceˉdigest,
            image.Directoryˉsnapshotˉdigest,
            Kernelˉprocessˉcontract.DIRECTORY_PROCESS_ID,
            Kernelˉprocessˉcontract.DIRECTORY_THREAD_ID,
            Kernelˉprocessˉcontract.DIRECTORY_PROCESS_GENERATION,
            Kernelˉprocessˉcontract.INIT_CAPABILITY_RIGHTS,
            Kernelˉprocessˉcontract.ROLE_DIRECTORY_SERVICE,
            Kernelˉprocessˉcontract.DIRECTORY_SYSCALL_BUDGET);
        Emitˉcopyˉkernelˉtables(output);
        Emitˉpopulateˉprocessˉtable(
            output, "directory", Kernelˉprocessˉcontract.DIRECTORY_CODE_PAGES,
            Kernelˉprocessˉcontract.DIRECTORY_STACK_PAGE,
            Kernelˉprocessˉcontract.DIRECTORY_STACK_PAGES,
            Kernelˉprocessˉcontract.DIRECTORY_DATA_PAGE,
            0, 0,
            Kernelˉprocessˉcontract.DIRECTORY_SERVICE_RESPONSE_PAGE,
            runtimeˉinputˉpresent: false,
            storeˉpresent: false,
            directoryˉpresent: true,
            clearˉresourceˉaliases: false);
        Emitˉcopyˉuserˉbytes(
            output, relocations, image.Directoryˉserviceˉimageˉbytes.Length, 6,
            Kernelˉprocessˉcontract.USER_CODE_PAGE);
        Emitˉcopyˉuserˉbytes(
            output, relocations, image.Directoryˉsnapshotˉbytes.Length, 4,
            Kernelˉprocessˉcontract.DIRECTORY_SNAPSHOT_PAGE);
        Emitˉinitializeˉuserˉcontext(
            output, Kernelˉprocessˉcontract.DIRECTORY_DATA_PAGE,
            Kernelˉprocessˉcontract.DIRECTORY_INSTRUCTION_BUDGET,
            Kernelˉprocessˉcontract.DIRECTORY_CALL_DEPTH_BUDGET,
            false);
        Emitˉinitializeˉdirectoryˉdescriptor(
            output, checked((uint)image.Directoryˉsnapshotˉbytes.Length));

        // Build a distinct send-only interpreter root. Its target resource PTE
        // and ABI-17 resource tables remain zero until the init grant syscall.
        Emitˉloadˉstackˉr13(output, CLIENT_RECORD_SLOT_OFFSET);
        Emitˉallocateˉextent(output, relocations, Kernelˉprocessˉcontract.CLIENT_ALLOCATION_PAGES);
        Emitˉinitializeˉrecord(
            output,
            image.Interpreterˉdigest,
            image.Admittedˉprogramˉdigest,
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID,
            Kernelˉprocessˉcontract.CLIENT_THREAD_ID,
            Kernelˉprocessˉcontract.FIRST_CLIENT_GENERATION,
            Kernelˉprocessˉcontract.CLIENT_CAPABILITY_RIGHTS,
            Kernelˉprocessˉcontract.ROLE_BYTECODE_INTERPRETER,
            Clientˉsyscallˉbudget);
        Emitˉcopyˉkernelˉtables(output);
        Emitˉpopulateˉprocessˉtable(
            output, "client", Kernelˉprocessˉcontract.CLIENT_CODE_PAGES,
            Kernelˉprocessˉcontract.CLIENT_STACK_PAGE, Kernelˉprocessˉcontract.CLIENT_STACK_PAGES,
            Kernelˉprocessˉcontract.CLIENT_DATA_PAGE,
            Kernelˉprocessˉcontract.CLIENT_RUNTIME_INPUT_PAGE,
            Kernelˉprocessˉcontract.CLIENT_RUNTIME_BUDGET_PAGE,
            Kernelˉprocessˉcontract.CLIENT_SERVICE_RESPONSE_PAGE,
            runtimeˉinputˉpresent: false,
            storeˉpresent: false,
            directoryˉpresent: false,
            clearˉresourceˉaliases: true);
        Emitˉcopyˉuserˉbytes(
            output, relocations, image.Clientˉimageˉbytes.Length, 1,
            Kernelˉprocessˉcontract.USER_CODE_PAGE);
        Emitˉinitializeˉuserˉcontext(
            output, Kernelˉprocessˉcontract.CLIENT_DATA_PAGE,
            Kernelˉprocessˉcontract.CLIENT_INSTRUCTION_BUDGET,
            Kernelˉprocessˉcontract.CLIENT_CALL_DEPTH_BUDGET,
            true);
        Emitˉinitializeˉresourceˉrecord(output, image);
        Emitˉvalidateˉstoreˉresource(output, image, FAILURE_LABEL);
        Emitˉvalidateˉdirectoryˉresource(output, image, FAILURE_LABEL);

        Emitˉinstallˉdescriptorˉstate(output, relocations);
        Emitˉconfigureˉsyscallˉmsrs(output);

        // The ready/wait dispatcher starts the directory provider first so its
        // exact receive destination exists before either client can call it.
        Emitˉdispatchˉreadyˉrecord(
            output, Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        Emitˉenterˉinitialˉprocess(
            output,
            DIRECTORY_SERVICE_BLOCKED_LABEL,
            Kernelˉprocessˉcontract.DIRECTORY_STACK_PAGES);

        output.Mark(DIRECTORY_SERVICE_BLOCKED_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_RUNNING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_WAITING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_SERVICE_REQUEST);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉstateˉchannelˉr10(
            output, Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);

        // Windvale init grants its resource, registers its request destination,
        // then deterministically blocks on the channel.
        Emitˉdispatchˉreadyˉrecord(
            output, Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE);
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
            Kernelˉprocessˉcontract.WAIT_REASON_SERVICE_REQUEST);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉstateˉchannelˉr10(output, Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REQUEST_COUNT_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉvalidateˉborrowedˉresource(
            output, FAILURE_LABEL, checked((uint)image.Admittedˉprogramˉbytes.Length));

        // Return GS to its user value, install the client as the next syscall
        // destination, and run it under its own root.
        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉdispatchˉreadyˉrecord(
            output, Kernelˉprocessˉcontract.FIRST_CLIENT_PROCESS_REFERENCE);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        Emitˉenterˉinitialˉprocess(output, CLIENT_COMPLETION_LABEL, Kernelˉprocessˉcontract.CLIENT_STACK_PAGES);

        output.Mark(CLIENT_COMPLETION_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_RUNNING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_WAITING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_SERVICE_REPLY);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉstateˉchannelˉr10(output, Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_STATE_REQUEST_DELIVERED);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REQUEST_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_BYTE_LENGTH_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_CONFIGURATION_REQUEST_BYTES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);

        // The request is now copied. Resume only the registered service and
        // return the exact opaque byte count from its receive operation.
        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉwakeˉandˉdispatchˉreadyˉrecord(
            output,
            INIT_RECORD_SLOT_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        output.Loadˉripˉrelativeˉrdx(SERVICE_REPLY_BLOCKED_LABEL);
        output.Emit(0x49, 0x89, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_RUNNING);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_NONE);
        output.Emit(0xB8);
        output.Emitˉu32(Kernelˉprocessˉcontract.RESOURCE_CONFIGURATION_REQUEST_BYTES);
        Emitˉresumeˉsavedˉprocess(output);

        output.Mark(SERVICE_REPLY_BLOCKED_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_RUNNING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_WAITING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 3);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_CHANNEL_RECEIVE);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉstateˉchannelˉr10(output, Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REPLY_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_BYTE_LENGTH_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_CONFIGURATION_REPLY_BYTES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);

        // Complete the reply syscall, then let init register its ordinary
        // result receive before the client uses the independent directory endpoint.
        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET, 0);
        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉwakeˉandˉdispatchˉreadyˉrecord(
            output,
            INIT_RECORD_SLOT_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        output.Loadˉripˉrelativeˉrdx(SERVICE_RESULT_BLOCKED_LABEL);
        output.Emit(0x49, 0x89, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_RUNNING);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_NONE);
        output.Emit(0x31, 0xC0);
        Emitˉresumeˉsavedˉprocess(output);

        output.Mark(SERVICE_RESULT_BLOCKED_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_RUNNING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_WAITING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 4);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_CHANNEL_RECEIVE);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉstateˉchannelˉr10(output, Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REQUEST_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REPLY_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);

        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉwakeˉandˉdispatchˉreadyˉrecord(
            output,
            CLIENT_RECORD_SLOT_OFFSET,
            Kernelˉprocessˉcontract.FIRST_CLIENT_PROCESS_REFERENCE);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        output.Loadˉripˉrelativeˉrdx(CLIENT_DIRECTORY_COMPLETION_LABEL);
        output.Emit(0x49, 0x89, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_RUNNING);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_NONE);
        output.Emit(0xB8);
        output.Emitˉu32(Kernelˉprocessˉcontract.RESOURCE_CONFIGURATION_REPLY_BYTES);
        Emitˉresumeˉsavedˉprocess(output);

        output.Mark(CLIENT_DIRECTORY_COMPLETION_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_RUNNING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_WAITING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 2);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_SERVICE_REPLY);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉstateˉchannelˉr10(
            output, Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_STATE_REQUEST_DELIVERED);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REQUEST_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_BYTE_LENGTH_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_READ_REQUEST_BYTES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);

        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉwakeˉandˉdispatchˉreadyˉrecord(
            output,
            DIRECTORY_RECORD_SLOT_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        output.Loadˉripˉrelativeˉrdx(SERVICE_DIRECTORY_REPLY_BLOCKED_LABEL);
        output.Emit(0x49, 0x89, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_RUNNING);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_NONE);
        output.Emit(0xB8);
        output.Emitˉu32(Kernelˉprocessˉcontract.DIRECTORY_READ_REQUEST_BYTES);
        Emitˉresumeˉsavedˉprocess(output);

        output.Mark(SERVICE_DIRECTORY_REPLY_BLOCKED_LABEL);
        if (Serviceˉfault)
        {
            // The service rejected the structurally inconsistent WVDQ request
            // and trapped in CPL3. Only this exact fault shape is contained.
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
                Kernelˉprocessˉcontract.PROCESS_STATE_FAULTED);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
                Kernelˉprocessˉcontract.THREAD_STATE_FAULTED);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 1);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
                Kernelˉprocessˉcontract.WAIT_REASON_NONE);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.FAULT_VECTOR_OFFSET,
                Kernelˉexceptionˉcontract.GENERAL_PROTECTION_VECTOR);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.FAULT_ERROR_OFFSET, 0);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.RESULT_OFFSET, 1);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉloadˉstateˉchannelˉr10(
                output, Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET);
            Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET,
                Kernelˉprocessˉcontract.CHANNEL_STATE_REQUEST_DELIVERED);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
                Kernelˉprocessˉcontract.CLIENT_PROCESS_ID);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REQUEST_COUNT_OFFSET, 1);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REPLY_COUNT_OFFSET, 0);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_BYTE_LENGTH_OFFSET,
                Kernelˉprocessˉcontract.DIRECTORY_READ_REQUEST_BYTES);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);

            // WVCHAN04 records the failed service peer, wakes the one waiting
            // client, and clears every transient address and byte count first.
            Emitˉterminateˉchannelˉpeer(
                output,
                Kernelˉprocessˉcontract.DIRECTORY_PROCESS_ID,
                Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_FAULTED,
                wakeˉwaiter: true);
            Emitˉcloseˉendpoint(
                output,
                Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_STATUS_FAULTED,
                Kernelˉprocessˉcontract.SERVICE_FAULT_DIRECTORY_ENDPOINT_RESOLUTION_COUNT,
                Kernelˉprocessˉcontract.DIRECTORY_ENDPOINT_RECORD_OFFSET,
                Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET,
                Kernelˉprocessˉcontract.DIRECTORY_CAPABILITY_REFERENCE,
                Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE);
            Emitˉvalidateˉserviceˉfaultˉchannel(output);
            Emitˉvalidateˉclosedˉendpoint(
                output,
                Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_STATUS_FAULTED,
                Kernelˉprocessˉcontract.SERVICE_FAULT_DIRECTORY_ENDPOINT_RESOLUTION_COUNT,
                Kernelˉprocessˉcontract.DIRECTORY_ENDPOINT_RECORD_OFFSET,
                Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET,
                Kernelˉprocessˉcontract.DIRECTORY_CAPABILITY_REFERENCE,
                Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE);

            output.Emit(0x0F, 0x01, 0xF8);
            Emitˉwakeˉandˉdispatchˉreadyˉrecord(
                output,
                CLIENT_RECORD_SLOT_OFFSET,
                Kernelˉprocessˉcontract.FIRST_CLIENT_PROCESS_REFERENCE);
            Emitˉactivateˉrecordˉroot(output, relocations);
            Emitˉsetˉkernelˉgsˉbase(output);
            output.Loadˉripˉrelativeˉrdx(SERVICE_FAULT_CLIENT_TERMINAL_LABEL);
            output.Emit(0x49, 0x89, 0x95);
            output.Emitˉu32(Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);
            Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
                Kernelˉprocessˉcontract.THREAD_STATE_RUNNING);
            Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
                Kernelˉprocessˉcontract.WAIT_REASON_NONE);
            output.Emit(0xB8);
            output.Emitˉu32(Kernelˉprocessˉcontract.SERVICE_CALL_PEER_FAULTED);
            Emitˉresumeˉsavedˉprocess(output);

            output.Mark(SERVICE_FAULT_CLIENT_TERMINAL_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
                Kernelˉprocessˉcontract.PROCESS_STATE_EXITED);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
                Kernelˉprocessˉcontract.THREAD_STATE_EXITED);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET,
                Kernelˉprocessˉcontract.SERVICE_FAULT_CLIENT_SYSCALL_BUDGET);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.ROLE_OFFSET,
                Kernelˉprocessˉcontract.ROLE_BYTECODE_INTERPRETER);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_GENERATION_OFFSET,
                Kernelˉprocessˉcontract.FIRST_CLIENT_GENERATION);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.RESULT_OFFSET,
                Kernelˉprocessˉcontract.EXPECTED_RESULT);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
            Emitˉvalidateˉclientˉrecordˉarena(output);
            Emitˉloadˉstateˉchannelˉr10(
                output, Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET);
            Emitˉvalidateˉserviceˉfaultˉchannel(output);
            Emitˉrevokeˉterminalˉresource(
                output, FAILURE_LABEL, checked((uint)image.Admittedˉprogramˉbytes.Length));

            // Return under init's surviving root after proving that the client
            // aliases are revoked and the attached service resources remain exact.
            output.Emit(0x0F, 0x01, 0xF8);
            Emitˉloadˉstackˉr13(output, INIT_RECORD_SLOT_OFFSET);
            Emitˉactivateˉrecordˉroot(output, relocations);
            Emitˉvalidateˉreleasedˉresource(
                output, FAILURE_LABEL, checked((uint)image.Admittedˉprogramˉbytes.Length));
            Emitˉvalidateˉstoreˉresource(output, image, FAILURE_LABEL);
            Emitˉvalidateˉdirectoryˉresource(output, image, FAILURE_LABEL);
            Emitˉvalidateˉexhaustedˉallocator(output);
            output.Emit(0xB8);
            output.Emitˉu32(Kernelˉprocessˉcontract.EXPECTED_RESULT);
            Emitˉepilogue(output);

            output.Mark(FAILURE_LABEL);
            output.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);
            Emitˉepilogue(output);
            return;
        }
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
        Emitˉloadˉstateˉchannelˉr10(
            output, Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REPLY_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_BYTE_LENGTH_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_READ_REPLY_BYTES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);

        // Complete the provider's reply syscall and let it register the next
        // request destination before the first client consumes the reply.
        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET, 0);
        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉwakeˉandˉdispatchˉreadyˉrecord(
            output,
            DIRECTORY_RECORD_SLOT_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        output.Loadˉripˉrelativeˉrdx(DIRECTORY_SECOND_REQUEST_BLOCKED_LABEL);
        output.Emit(0x49, 0x89, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_RUNNING);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_NONE);
        output.Emit(0x31, 0xC0);
        Emitˉresumeˉsavedˉprocess(output);

        output.Mark(DIRECTORY_SECOND_REQUEST_BLOCKED_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_RUNNING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_WAITING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 3);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_SERVICE_REQUEST);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉstateˉchannelˉr10(
            output, Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REPLY_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_BYTE_LENGTH_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_READ_REPLY_BYTES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉwakeˉandˉdispatchˉreadyˉrecord(
            output,
            CLIENT_RECORD_SLOT_OFFSET,
            Kernelˉprocessˉcontract.FIRST_CLIENT_PROCESS_REFERENCE);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        output.Loadˉripˉrelativeˉrdx(CLIENT_TERMINAL_LABEL);
        output.Emit(0x49, 0x89, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_RUNNING);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_NONE);
        output.Emit(0xB8);
        output.Emitˉu32(Kernelˉprocessˉcontract.DIRECTORY_READ_REPLY_BYTES);
        Emitˉresumeˉsavedˉprocess(output);

        output.Mark(CLIENT_TERMINAL_LABEL);
        if (Userˉfault)
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
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 3);
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
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_GENERATION_OFFSET,
            Kernelˉprocessˉcontract.FIRST_CLIENT_GENERATION);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.RESULT_OFFSET,
            Kernelˉprocessˉcontract.EXPECTED_RESULT);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉvalidateˉclientˉrecordˉarena(output);
        Emitˉloadˉstateˉchannelˉr10(output, Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_MESSAGE_OFFSET,
            Kernelˉprocessˉcontract.EXPECTED_RESULT);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_SEND_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉrevokeˉterminalˉresource(
            output, FAILURE_LABEL, checked((uint)image.Admittedˉprogramˉbytes.Length));

        // The client is now terminal and its immutable aliases are gone.
        // Switch to init's root before releasing the exact 122-page allocator
        // tail, then rebuild generation 2 at the identical physical root.
        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉloadˉstackˉr13(output, INIT_RECORD_SLOT_OFFSET);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        Emitˉvalidateˉreleasedˉresource(
            output, FAILURE_LABEL, checked((uint)image.Admittedˉprogramˉbytes.Length));
        Emitˉvalidateˉstoreˉresource(output, image, FAILURE_LABEL);
        Emitˉvalidateˉdirectoryˉresource(output, image, FAILURE_LABEL);
        Emitˉreclaimˉandˉrebuildˉclient(output, relocations, image);

        // Consume generation 1's message and resume init. Its WVA seam issues
        // a second grant and receive-request, then blocks at the new coordinator
        // label.
        Emitˉwakeˉandˉdispatchˉreadyˉrecord(
            output,
            INIT_RECORD_SLOT_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE);
        Emitˉsetˉkernelˉgsˉbase(output);
        output.Loadˉripˉrelativeˉrdx(SECOND_SERVICE_BLOCKED_LABEL);
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
        Emitˉterminateˉchannelˉpeer(
            output,
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID,
            Userˉfault
                ? Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_FAULTED
                : Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_EXITED);
        Emitˉreopenˉchannel(output);
        Emitˉresumeˉsavedˉprocess(output);

        output.Mark(SECOND_SERVICE_BLOCKED_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_RUNNING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_WAITING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 6);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_SERVICE_REQUEST);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉstateˉchannelˉr10(output, Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_SEND_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_RECEIVE_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAKE_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REQUEST_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REPLY_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉvalidateˉborrowedˉresource(
            output, FAILURE_LABEL, checked((uint)image.Admittedˉprogramˉbytes.Length));

        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉdispatchˉreadyˉrecord(
            output, Kernelˉprocessˉcontract.SECOND_CLIENT_PROCESS_REFERENCE);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        Emitˉenterˉinitialˉprocess(
            output, SECOND_CLIENT_COMPLETION_LABEL, Kernelˉprocessˉcontract.CLIENT_STACK_PAGES);

        output.Mark(SECOND_CLIENT_COMPLETION_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_RUNNING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_WAITING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_SERVICE_REPLY);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉstateˉchannelˉr10(output, Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_STATE_REQUEST_DELIVERED);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REQUEST_COUNT_OFFSET, 2);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_BYTE_LENGTH_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_CONFIGURATION_REQUEST_BYTES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);

        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉwakeˉandˉdispatchˉreadyˉrecord(
            output,
            INIT_RECORD_SLOT_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        output.Loadˉripˉrelativeˉrdx(SECOND_SERVICE_REPLY_BLOCKED_LABEL);
        output.Emit(0x49, 0x89, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_RUNNING);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_NONE);
        output.Emit(0xB8);
        output.Emitˉu32(Kernelˉprocessˉcontract.RESOURCE_CONFIGURATION_REQUEST_BYTES);
        Emitˉresumeˉsavedˉprocess(output);

        output.Mark(SECOND_SERVICE_REPLY_BLOCKED_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_RUNNING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_WAITING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 7);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_CHANNEL_RECEIVE);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉstateˉchannelˉr10(output, Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REPLY_COUNT_OFFSET, 2);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_BYTE_LENGTH_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_CONFIGURATION_REPLY_BYTES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);

        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET, 0);
        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉwakeˉandˉdispatchˉreadyˉrecord(
            output,
            INIT_RECORD_SLOT_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        output.Loadˉripˉrelativeˉrdx(SECOND_SERVICE_RESULT_BLOCKED_LABEL);
        output.Emit(0x49, 0x89, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_RUNNING);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_NONE);
        output.Emit(0x31, 0xC0);
        Emitˉresumeˉsavedˉprocess(output);

        output.Mark(SECOND_SERVICE_RESULT_BLOCKED_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_RUNNING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_WAITING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 8);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_CHANNEL_RECEIVE);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉstateˉchannelˉr10(output, Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REQUEST_COUNT_OFFSET, 2);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REPLY_COUNT_OFFSET, 2);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);

        // Generation two now leaves init blocked for the terminal result while
        // the client proceeds through the independent directory endpoint.
        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉwakeˉandˉdispatchˉreadyˉrecord(
            output,
            CLIENT_RECORD_SLOT_OFFSET,
            Kernelˉprocessˉcontract.SECOND_CLIENT_PROCESS_REFERENCE);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        output.Loadˉripˉrelativeˉrdx(SECOND_CLIENT_DIRECTORY_COMPLETION_LABEL);
        output.Emit(0x49, 0x89, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_RUNNING);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_NONE);
        output.Emit(0xB8);
        output.Emitˉu32(Kernelˉprocessˉcontract.RESOURCE_CONFIGURATION_REPLY_BYTES);
        Emitˉresumeˉsavedˉprocess(output);

        output.Mark(SECOND_CLIENT_DIRECTORY_COMPLETION_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_RUNNING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_WAITING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 2);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_SERVICE_REPLY);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉstateˉchannelˉr10(
            output, Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_STATE_REQUEST_DELIVERED);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REQUEST_COUNT_OFFSET, 2);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_BYTE_LENGTH_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_READ_REQUEST_BYTES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);

        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉwakeˉandˉdispatchˉreadyˉrecord(
            output,
            DIRECTORY_RECORD_SLOT_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        output.Loadˉripˉrelativeˉrdx(SECOND_SERVICE_DIRECTORY_REPLY_BLOCKED_LABEL);
        output.Emit(0x49, 0x89, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_RUNNING);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_NONE);
        output.Emit(0xB8);
        output.Emitˉu32(Kernelˉprocessˉcontract.DIRECTORY_READ_REQUEST_BYTES);
        Emitˉresumeˉsavedˉprocess(output);

        output.Mark(SECOND_SERVICE_DIRECTORY_REPLY_BLOCKED_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_RUNNING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_WAITING);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 4);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_CHANNEL_RECEIVE);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉstateˉchannelˉr10(
            output, Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REPLY_COUNT_OFFSET, 2);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_BYTE_LENGTH_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_READ_REPLY_BYTES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);

        // The final provider reply is also a blocking handoff. Complete it so
        // the provider executes its explicit exit under its own root.
        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET, 0);
        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉwakeˉandˉdispatchˉreadyˉrecord(
            output,
            DIRECTORY_RECORD_SLOT_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        output.Loadˉripˉrelativeˉrdx(DIRECTORY_EXITED_LABEL);
        output.Emit(0x49, 0x89, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_RUNNING);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_NONE);
        output.Emit(0x31, 0xC0);
        Emitˉresumeˉsavedˉprocess(output);

        output.Mark(DIRECTORY_EXITED_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_EXITED);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_EXITED);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_SYSCALL_BUDGET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.RESULT_OFFSET,
            Kernelˉprocessˉcontract.EXPECTED_RESULT);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉstateˉchannelˉr10(
            output, Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REPLY_COUNT_OFFSET, 2);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcloseˉendpoint(
            output,
            Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_STATUS_EXITED,
            Kernelˉprocessˉcontract.NORMAL_DIRECTORY_ENDPOINT_RESOLUTION_COUNT,
            Kernelˉprocessˉcontract.DIRECTORY_ENDPOINT_RECORD_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_CAPABILITY_REFERENCE,
            Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_BYTE_LENGTH_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_READ_REPLY_BYTES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉterminateˉchannelˉpeer(
            output,
            Kernelˉprocessˉcontract.DIRECTORY_PROCESS_ID,
            Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_EXITED);

        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉwakeˉandˉdispatchˉreadyˉrecord(
            output,
            CLIENT_RECORD_SLOT_OFFSET,
            Kernelˉprocessˉcontract.SECOND_CLIENT_PROCESS_REFERENCE);
        Emitˉactivateˉrecordˉroot(output, relocations);
        Emitˉsetˉkernelˉgsˉbase(output);
        output.Loadˉripˉrelativeˉrdx(SECOND_CLIENT_TERMINAL_LABEL);
        output.Emit(0x49, 0x89, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_RUNNING);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_NONE);
        output.Emit(0xB8);
        output.Emitˉu32(Kernelˉprocessˉcontract.DIRECTORY_READ_REPLY_BYTES);
        Emitˉresumeˉsavedˉprocess(output);

        output.Mark(SECOND_CLIENT_TERMINAL_LABEL);
        if (Userˉfault)
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
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 3);
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
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_GENERATION_OFFSET,
            Kernelˉprocessˉcontract.SECOND_CLIENT_GENERATION);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.RESULT_OFFSET,
            Kernelˉprocessˉcontract.EXPECTED_RESULT);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉvalidateˉclientˉrecordˉarena(output);
        Emitˉloadˉstateˉchannelˉr10(output, Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_MESSAGE_OFFSET,
            Kernelˉprocessˉcontract.EXPECTED_RESULT);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_SEND_COUNT_OFFSET, 2);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉrevokeˉterminalˉresource(
            output, FAILURE_LABEL, checked((uint)image.Admittedˉprogramˉbytes.Length));

        // Generation 2 is terminal. Switch back to init, consume its message,
        // and let the nine-syscall resource service exit through the final label.
        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉwakeˉandˉdispatchˉreadyˉrecord(
            output,
            INIT_RECORD_SLOT_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE);
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
        Emitˉterminateˉchannelˉpeer(
            output,
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID,
            Userˉfault
                ? Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_FAULTED
                : Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_EXITED);
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
        Emitˉcloseˉendpoint(
            output,
            Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_STATUS_EXITED,
            Kernelˉprocessˉcontract.NORMAL_ENDPOINT_RESOLUTION_COUNT);
        Emitˉvalidateˉclosedˉendpoint(
            output,
            Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_STATUS_EXITED,
            Kernelˉprocessˉcontract.NORMAL_ENDPOINT_RESOLUTION_COUNT);
        Emitˉloadˉstateˉchannelˉr10(output, Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_MESSAGE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_RECEIVE_COUNT_OFFSET, 2);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAKE_COUNT_OFFSET, 2);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REQUEST_COUNT_OFFSET, 2);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REPLY_COUNT_OFFSET, 2);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(
            output,
            Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_OFFSET,
            Userˉfault
                ? Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_FAULTED
                : Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_EXITED);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_PEER_PROCESS_OFFSET,
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_CLOSE_COUNT_OFFSET, 2);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉvalidateˉclosedˉendpoint(
            output,
            Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_STATUS_EXITED,
            Kernelˉprocessˉcontract.NORMAL_DIRECTORY_ENDPOINT_RESOLUTION_COUNT,
            Kernelˉprocessˉcontract.DIRECTORY_ENDPOINT_RECORD_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_CAPABILITY_REFERENCE,
            Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE);
        Emitˉloadˉstateˉchannelˉr10(
            output, Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET);
        foreach (var Field in new (uint Offset, uint Value)[]
        {
            (Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0),
            (Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET, 0),
            (Kernelˉprocessˉcontract.CHANNEL_REQUEST_COUNT_OFFSET, 2),
            (Kernelˉprocessˉcontract.CHANNEL_REPLY_COUNT_OFFSET, 2),
            (Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_OFFSET,
                Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_EXITED),
            (Kernelˉprocessˉcontract.CHANNEL_PEER_PROCESS_OFFSET,
                Kernelˉprocessˉcontract.DIRECTORY_PROCESS_ID),
            (Kernelˉprocessˉcontract.CHANNEL_CLOSE_COUNT_OFFSET, 1),
        })
        {
            Emitˉcompareˉchannelˉu32(output, Field.Offset, Field.Value);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        }
        Emitˉvalidateˉreleasedˉresource(
            output, FAILURE_LABEL, checked((uint)image.Admittedˉprogramˉbytes.Length));
        Emitˉvalidateˉstoreˉresource(output, image, FAILURE_LABEL);
        Emitˉvalidateˉdirectoryˉresource(output, image, FAILURE_LABEL);

        // Restore the kernel GS state and independently re-check the terminal client.
        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉloadˉstackˉr13(output, CLIENT_RECORD_SLOT_OFFSET);
        Emitˉcompareˉrecordˉu32(output, Kernelˉprocessˉcontract.PROCESS_GENERATION_OFFSET,
            Kernelˉprocessˉcontract.SECOND_CLIENT_GENERATION);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉrecordˉu32(output, Kernelˉprocessˉcontract.RESULT_OFFSET,
            Kernelˉprocessˉcontract.EXPECTED_RESULT);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉvalidateˉexhaustedˉallocator(output);
        output.Emit(0xB8);
        output.Emitˉu32(Kernelˉprocessˉcontract.EXPECTED_RESULT);
        Emitˉepilogue(output);

        output.Mark(FAILURE_LABEL);
        output.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);
        Emitˉepilogue(output);
    }

    private static void Emitˉexceptionˉentry(
        X64ˉcodeˉbuilder output,
        ImmutableArray<Objectˉrelocation>.Builder relocations,
        Kernelˉprocessˉscenario scenario)
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
        if (scenario == Kernelˉprocessˉscenario.Serviceˉfault)
        {
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.ROLE_OFFSET,
                Kernelˉprocessˉcontract.ROLE_DIRECTORY_SERVICE);
            output.Jumpˉif(CONDITION_NOT_EQUAL, EXCEPTION_FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 1);
            output.Jumpˉif(CONDITION_NOT_EQUAL, EXCEPTION_FAILURE_LABEL);
            Emitˉloadˉstateˉchannelˉr10(
                output, Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET);
            Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET,
                Kernelˉprocessˉcontract.CHANNEL_STATE_REQUEST_DELIVERED);
            output.Jumpˉif(CONDITION_NOT_EQUAL, EXCEPTION_FAILURE_LABEL);
            Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
                Kernelˉprocessˉcontract.CLIENT_PROCESS_ID);
            output.Jumpˉif(CONDITION_NOT_EQUAL, EXCEPTION_FAILURE_LABEL);
            Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REQUEST_COUNT_OFFSET, 1);
            output.Jumpˉif(CONDITION_NOT_EQUAL, EXCEPTION_FAILURE_LABEL);
            Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REPLY_COUNT_OFFSET, 0);
            output.Jumpˉif(CONDITION_NOT_EQUAL, EXCEPTION_FAILURE_LABEL);
            Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_BYTE_LENGTH_OFFSET,
                Kernelˉprocessˉcontract.DIRECTORY_READ_REQUEST_BYTES);
            output.Jumpˉif(CONDITION_NOT_EQUAL, EXCEPTION_FAILURE_LABEL);
        }
        else
        {
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.ROLE_OFFSET,
                Kernelˉprocessˉcontract.ROLE_BYTECODE_INTERPRETER);
            output.Jumpˉif(CONDITION_NOT_EQUAL, EXCEPTION_FAILURE_LABEL);
            Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET, 3);
            output.Jumpˉif(CONDITION_NOT_EQUAL, EXCEPTION_FAILURE_LABEL);
            Emitˉloadˉgsˉchannelˉr10(output);
            Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 1);
            output.Jumpˉif(CONDITION_NOT_EQUAL, EXCEPTION_FAILURE_LABEL);
            Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_MESSAGE_OFFSET,
                Kernelˉprocessˉcontract.EXPECTED_RESULT);
            output.Jumpˉif(CONDITION_NOT_EQUAL, EXCEPTION_FAILURE_LABEL);
        }
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
            Kernelˉprocessˉcontract.PROCESS_STATE_FAULTED);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_FAULTED);
        Emitˉstoreˉgsˉu32(
            output,
            Kernelˉprocessˉcontract.RESULT_OFFSET,
            scenario == Kernelˉprocessˉscenario.Serviceˉfault
                ? 1U
                : checked((uint)Kernelˉprocessˉcontract.EXPECTED_RESULT));
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
        relocations.Add(Relocation(Jumpˉfield, 13));
    }

    private static void Emitˉsyscallˉentry(
        X64ˉcodeˉbuilder output,
        Kernelˉprocessˉimageˉartifacts image)
    {
        output.Mark("process_syscall_machine_entry");
        output.Emit(0x0F, 0x01, 0xF8);
        Emitˉstoreˉgsˉrsp(output, Kernelˉprocessˉcontract.USER_STACK_POINTER_OFFSET);
        Emitˉstoreˉgsˉrcx(output, Kernelˉprocessˉcontract.USER_INSTRUCTION_POINTER_OFFSET);
        Emitˉstoreˉgsˉr11(output, Kernelˉprocessˉcontract.USER_FLAGS_OFFSET);
        Emitˉloadˉgsˉrsp(output, Kernelˉprocessˉcontract.KERNEL_STACK_OFFSET);
        output.Emit(0x83, 0xFB, (byte)Kernelˉprocessˉcontract.SYSCALL_SEND);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_SEND_LABEL);
        output.Emit(0x83, 0xFB, (byte)Kernelˉprocessˉcontract.SYSCALL_RECEIVE);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_RECEIVE_LABEL);
        output.Emit(0x83, 0xFB, (byte)Kernelˉprocessˉcontract.SYSCALL_EXIT);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_EXIT_LABEL);
        output.Emit(0x83, 0xFB, (byte)Kernelˉprocessˉcontract.SYSCALL_GRANT_BOOT_RESOURCE);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_GRANT_RESOURCE_LABEL);
        output.Emit(0x83, 0xFB, (byte)Kernelˉprocessˉcontract.SYSCALL_RECEIVE_SERVICE_REQUEST);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_RECEIVE_SERVICE_REQUEST_LABEL);
        output.Emit(0x83, 0xFB, (byte)Kernelˉprocessˉcontract.SYSCALL_CALL_SERVICE);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_CALL_SERVICE_LABEL);
        output.Emit(0x83, 0xFB, (byte)Kernelˉprocessˉcontract.SYSCALL_REPLY_SERVICE_REQUEST);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_REPLY_SERVICE_REQUEST_LABEL);
        output.Jump(SYSCALL_FAILURE_LABEL);

        output.Mark(SYSCALL_GRANT_RESOURCE_LABEL);
        Emitˉvalidateˉcapability(
            output, Kernelˉprocessˉcontract.CAPABILITY_RIGHT_GRANT_BOOT_RESOURCE, provider: true);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.ROLE_OFFSET,
            Kernelˉprocessˉcontract.ROLE_INIT_SERVICE);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        output.Emit(0x3D);
        output.Emitˉu32(Kernelˉprocessˉcontract.RESOURCE_SET_TOKEN);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉrequireˉsyscallˉbudget(output);
        Emitˉgrantˉbootˉresource(output, checked((uint)image.Admittedˉprogramˉbytes.Length));
        Emitˉincrementˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET);
        output.Emit(0x31, 0xC0);
        output.Jump(SYSCALL_RESUME_LABEL);

        output.Mark(SYSCALL_SEND_LABEL);
        Emitˉvalidateˉcapability(
            output, Kernelˉprocessˉcontract.CAPABILITY_RIGHT_SEND, provider: false);
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

        output.Mark(SYSCALL_RECEIVE_SERVICE_REQUEST_LABEL);
        Emitˉvalidateˉcapability(
            output, Kernelˉprocessˉcontract.CAPABILITY_RIGHT_RECEIVE_SERVICE_REQUEST, provider: true);
        Emitˉrequireˉsyscallˉbudget(output);
        Emitˉvalidateˉdataˉrange(
            output, "receive", addressˉisˉr10: false, lengthˉisˉedi: false);
        Emitˉloadˉgsˉchannelˉr10(output);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        output.Emit(0x4D, 0x89, 0x42,
            checked((byte)Kernelˉprocessˉcontract.CHANNEL_SERVICE_DESTINATION_OFFSET));
        output.Emit(0x45, 0x89, 0x4A,
            checked((byte)Kernelˉprocessˉcontract.CHANNEL_SERVICE_CAPACITY_OFFSET));
        Emitˉloadˉgsˉeax(output, 24);
        Emitˉstoreˉchannelˉeax(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET);
        Emitˉincrementˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_WAITING);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_SERVICE_REQUEST);
        Emitˉjumpˉgs(output, Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);

        output.Mark(SYSCALL_CALL_SERVICE_LABEL);
        Emitˉvalidateˉcapability(
            output, Kernelˉprocessˉcontract.CAPABILITY_RIGHT_CALL_SERVICE, provider: false);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.ROLE_OFFSET,
            Kernelˉprocessˉcontract.ROLE_BYTECODE_INTERPRETER);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉrequireˉsyscallˉbudget(output);
        Emitˉvalidateˉcodeˉrange(output);
        Emitˉvalidateˉdataˉrange(
            output, "call", addressˉisˉr10: true, lengthˉisˉedi: true);
        // Preserve the reply destination in the architecturally clobbered R11
        // before R10 becomes the shared record. R12 remains the kernel-state base.
        output.Emit(0x4D, 0x89, 0xD3);
        Emitˉloadˉgsˉchannelˉr10(output);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉcompareˉchannelˉproviderˉwaiter(output);
        // The service registered a destination large enough for the opaque request.
        output.Emit(0x45, 0x39, 0x4A,
            checked((byte)Kernelˉprocessˉcontract.CHANNEL_SERVICE_CAPACITY_OFFSET));
        output.Jumpˉif(CONDITION_BELOW, SYSCALL_FAILURE_LABEL);
        output.Emit(0x4D, 0x89, 0x5A,
            checked((byte)Kernelˉprocessˉcontract.CHANNEL_CLIENT_DESTINATION_OFFSET));
        output.Emit(0x41, 0x89, 0x7A,
            checked((byte)Kernelˉprocessˉcontract.CHANNEL_CLIENT_CAPACITY_OFFSET));
        output.Emit(0x45, 0x89, 0x4A,
            checked((byte)Kernelˉprocessˉcontract.CHANNEL_BYTE_LENGTH_OFFSET));
        // All process physical pages remain supervisor-visible through the
        // checked identity map; only the validated user extents are copied.
        output.Emit(0x4C, 0x89, 0xC6);
        output.Emit(0x49, 0x8B, 0x7A,
            checked((byte)Kernelˉprocessˉcontract.CHANNEL_SERVICE_DESTINATION_OFFSET));
        output.Emit(0x44, 0x89, 0xC9, 0xFC, 0xF3, 0xA4);
        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_STATE_REQUEST_DELIVERED);
        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_SENDER_OFFSET,
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID);
        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID);
        Emitˉincrementˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REQUEST_COUNT_OFFSET);
        Emitˉincrementˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_WAITING);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_SERVICE_REPLY);
        Emitˉjumpˉgs(output, Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);

        output.Mark(SYSCALL_REPLY_SERVICE_REQUEST_LABEL);
        Emitˉvalidateˉcapability(
            output, Kernelˉprocessˉcontract.CAPABILITY_RIGHT_REPLY_SERVICE_REQUEST, provider: true);
        Emitˉrequireˉsyscallˉbudget(output);
        Emitˉvalidateˉdataˉrange(
            output, "reply", addressˉisˉr10: false, lengthˉisˉedi: false);
        Emitˉloadˉgsˉchannelˉr10(output);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_STATE_REQUEST_DELIVERED);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        output.Emit(0x45, 0x39, 0x4A,
            checked((byte)Kernelˉprocessˉcontract.CHANNEL_CLIENT_CAPACITY_OFFSET));
        output.Jumpˉif(CONDITION_BELOW, SYSCALL_FAILURE_LABEL);
        output.Emit(0x45, 0x89, 0x4A,
            checked((byte)Kernelˉprocessˉcontract.CHANNEL_BYTE_LENGTH_OFFSET));
        output.Emit(0x4C, 0x89, 0xC6);
        output.Emit(0x49, 0x8B, 0x7A,
            checked((byte)Kernelˉprocessˉcontract.CHANNEL_CLIENT_DESTINATION_OFFSET));
        output.Emit(0x44, 0x89, 0xC9, 0xFC, 0xF3, 0xA4);
        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET, 0);
        Emitˉloadˉgsˉeax(output, 24);
        Emitˉstoreˉchannelˉeax(output, Kernelˉprocessˉcontract.CHANNEL_SENDER_OFFSET);
        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_RECEIVER_OFFSET,
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID);
        Emitˉstoreˉchannelˉeax(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET);
        Emitˉincrementˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_REPLY_COUNT_OFFSET);
        Emitˉincrementˉgsˉu32(output, Kernelˉprocessˉcontract.SYSCALL_COUNT_OFFSET);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_WAITING);
        Emitˉstoreˉgsˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_CHANNEL_RECEIVE);
        Emitˉjumpˉgs(output, Kernelˉprocessˉcontract.KERNEL_RESUME_OFFSET);

        output.Mark(SYSCALL_RECEIVE_LABEL);
        Emitˉvalidateˉcapability(
            output, Kernelˉprocessˉcontract.CAPABILITY_RIGHT_RECEIVE, provider: true);
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
        Emitˉloadˉgsˉrdx(output, Kernelˉprocessˉcontract.USER_DATA_ADDRESS_OFFSET);
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

    private static void Emitˉinitializeˉchannel(
        X64ˉcodeˉbuilder output,
        uint channelˉrecordˉoffset = Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET)
    {
        output.Emit(0x49, 0x8D, 0xBC, 0x24);
        output.Emitˉu32(channelˉrecordˉoffset);
        output.Emit(0x31, 0xC0, 0xB9);
        output.Emitˉu32(Kernelˉprocessˉcontract.CHANNEL_RECORD_BYTES / sizeof(ulong));
        output.Emit(0xFC, 0xF3, 0x48, 0xAB, 0x48, 0xB8);
        output.Emitˉu64(Kernelˉprocessˉcontract.CHANNEL_MAGIC);
        output.Emit(0x49, 0x89, 0x84, 0x24);
        output.Emitˉu32(channelˉrecordˉoffset);
        Emitˉstoreˉstateˉu32(output, channelˉrecordˉoffset + 8,
            Kernelˉprocessˉcontract.CHANNEL_VERSION);
        Emitˉstoreˉstateˉu32(output, channelˉrecordˉoffset + 12,
            Kernelˉprocessˉcontract.CHANNEL_RECORD_BYTES);
        Emitˉstoreˉstateˉu32(
            output,
            channelˉrecordˉoffset + Kernelˉprocessˉcontract.CHANNEL_CAPACITY_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_CAPACITY);
    }

    private static void Emitˉinitializeˉendpoint(
        X64ˉcodeˉbuilder output,
        uint endpointˉrecordˉoffset = Kernelˉprocessˉcontract.ENDPOINT_RECORD_OFFSET,
        uint channelˉrecordˉoffset = Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET,
        uint capabilityˉreference = Kernelˉprocessˉcontract.RESOURCE_CAPABILITY_REFERENCE,
        uint providerˉreference = Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE)
    {
        output.Emit(0x49, 0x8D, 0xBC, 0x24);
        output.Emitˉu32(endpointˉrecordˉoffset);
        output.Emit(0x31, 0xC0, 0xB9);
        output.Emitˉu32(Kernelˉprocessˉcontract.ENDPOINT_RECORD_BYTES / sizeof(ulong));
        output.Emit(0xFC, 0xF3, 0x48, 0xAB, 0x48, 0xB8);
        output.Emitˉu64(Kernelˉprocessˉcontract.ENDPOINT_MAGIC);
        output.Emit(0x49, 0x89, 0x84, 0x24);
        output.Emitˉu32(endpointˉrecordˉoffset);
        foreach (var Field in new (uint Offset, uint Value)[]
        {
            (8, Kernelˉprocessˉcontract.ENDPOINT_VERSION),
            (12, Kernelˉprocessˉcontract.ENDPOINT_RECORD_BYTES),
            (Kernelˉprocessˉcontract.ENDPOINT_STATE_OFFSET,
                Kernelˉprocessˉcontract.ENDPOINT_STATE_OPEN),
            (Kernelˉprocessˉcontract.ENDPOINT_REFERENCE_OFFSET,
                capabilityˉreference),
            (Kernelˉprocessˉcontract.ENDPOINT_KIND_OFFSET,
                Kernelˉprocessˉcontract.ENDPOINT_KIND_SERVICE),
            (Kernelˉprocessˉcontract.ENDPOINT_CAPACITY_OFFSET,
                Kernelˉprocessˉcontract.CHANNEL_CAPACITY),
            (Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_PROCESS_OFFSET,
                providerˉreference),
            (Kernelˉprocessˉcontract.ENDPOINT_CLIENT_PROCESS_OFFSET,
                Kernelˉprocessˉcontract.FIRST_CLIENT_PROCESS_REFERENCE),
        })
        {
            Emitˉstoreˉstateˉu32(
                output,
                endpointˉrecordˉoffset + Field.Offset,
                Field.Value);
        }
        output.Emit(0x49, 0x8D, 0x84, 0x24);
        output.Emitˉu32(channelˉrecordˉoffset);
        output.Emit(0x49, 0x89, 0x84, 0x24);
        output.Emitˉu32(
            endpointˉrecordˉoffset +
            Kernelˉprocessˉcontract.ENDPOINT_CHANNEL_ADDRESS_OFFSET);
    }

    private static void Emitˉrebindˉendpointˉclient(
        X64ˉcodeˉbuilder output,
        uint endpointˉrecordˉoffset,
        uint channelˉrecordˉoffset,
        uint capabilityˉreference,
        uint providerˉreference,
        uint expectedˉresolutionˉcount)
    {
        Emitˉloadˉstateˉendpointˉr11(output, endpointˉrecordˉoffset);
        Emitˉvalidateˉendpointˉheader(
            output, capabilityˉreference, providerˉreference, channelˉrecordˉoffset);
        foreach (var Field in new (uint Offset, uint Value)[]
        {
            (Kernelˉprocessˉcontract.ENDPOINT_STATE_OFFSET,
                Kernelˉprocessˉcontract.ENDPOINT_STATE_OPEN),
            (Kernelˉprocessˉcontract.ENDPOINT_CLIENT_PROCESS_OFFSET,
                Kernelˉprocessˉcontract.FIRST_CLIENT_PROCESS_REFERENCE),
            (Kernelˉprocessˉcontract.ENDPOINT_RESOLUTION_COUNT_OFFSET,
                expectedˉresolutionˉcount),
            (Kernelˉprocessˉcontract.ENDPOINT_CLOSE_COUNT_OFFSET, 0),
            (Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_STATUS_OFFSET,
                Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_STATUS_OPEN),
        })
        {
            Emitˉcompareˉendpointˉu32(output, Field.Offset, Field.Value);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        }
        Emitˉstoreˉendpointˉu32(
            output,
            Kernelˉprocessˉcontract.ENDPOINT_CLIENT_PROCESS_OFFSET,
            Kernelˉprocessˉcontract.SECOND_CLIENT_PROCESS_REFERENCE);
    }

    private static void Emitˉcloseˉendpoint(
        X64ˉcodeˉbuilder output,
        uint providerˉstatus,
        uint expectedˉresolutionˉcount,
        uint endpointˉrecordˉoffset = Kernelˉprocessˉcontract.ENDPOINT_RECORD_OFFSET,
        uint channelˉrecordˉoffset = Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET,
        uint capabilityˉreference = Kernelˉprocessˉcontract.RESOURCE_CAPABILITY_REFERENCE,
        uint providerˉreference = Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE)
    {
        Emitˉloadˉstateˉendpointˉr11(output, endpointˉrecordˉoffset);
        Emitˉvalidateˉendpointˉheader(
            output, capabilityˉreference, providerˉreference, channelˉrecordˉoffset);
        foreach (var Field in new (uint Offset, uint Value)[]
        {
            (Kernelˉprocessˉcontract.ENDPOINT_STATE_OFFSET,
                Kernelˉprocessˉcontract.ENDPOINT_STATE_OPEN),
            (Kernelˉprocessˉcontract.ENDPOINT_RESOLUTION_COUNT_OFFSET,
                expectedˉresolutionˉcount),
            (Kernelˉprocessˉcontract.ENDPOINT_CLOSE_COUNT_OFFSET, 0),
            (Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_STATUS_OFFSET,
                Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_STATUS_OPEN),
        })
        {
            Emitˉcompareˉendpointˉu32(output, Field.Offset, Field.Value);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        }
        Emitˉstoreˉendpointˉu32(
            output,
            Kernelˉprocessˉcontract.ENDPOINT_STATE_OFFSET,
            Kernelˉprocessˉcontract.ENDPOINT_STATE_CLOSED);
        Emitˉstoreˉendpointˉu32(output, Kernelˉprocessˉcontract.ENDPOINT_CLOSE_COUNT_OFFSET, 1);
        Emitˉstoreˉendpointˉu32(
            output,
            Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_STATUS_OFFSET,
            providerˉstatus);
    }

    private static void Emitˉvalidateˉclosedˉendpoint(
        X64ˉcodeˉbuilder output,
        uint providerˉstatus,
        uint expectedˉresolutionˉcount,
        uint endpointˉrecordˉoffset = Kernelˉprocessˉcontract.ENDPOINT_RECORD_OFFSET,
        uint channelˉrecordˉoffset = Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET,
        uint capabilityˉreference = Kernelˉprocessˉcontract.RESOURCE_CAPABILITY_REFERENCE,
        uint providerˉreference = Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE)
    {
        Emitˉloadˉstateˉendpointˉr11(output, endpointˉrecordˉoffset);
        Emitˉvalidateˉendpointˉheader(
            output, capabilityˉreference, providerˉreference, channelˉrecordˉoffset);
        foreach (var Field in new (uint Offset, uint Value)[]
        {
            (Kernelˉprocessˉcontract.ENDPOINT_STATE_OFFSET,
                Kernelˉprocessˉcontract.ENDPOINT_STATE_CLOSED),
            (Kernelˉprocessˉcontract.ENDPOINT_RESOLUTION_COUNT_OFFSET,
                expectedˉresolutionˉcount),
            (Kernelˉprocessˉcontract.ENDPOINT_CLOSE_COUNT_OFFSET, 1),
            (Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_STATUS_OFFSET, providerˉstatus),
        })
        {
            Emitˉcompareˉendpointˉu32(output, Field.Offset, Field.Value);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        }
    }

    private static void Emitˉvalidateˉendpointˉheader(
        X64ˉcodeˉbuilder output,
        uint capabilityˉreference,
        uint providerˉreference,
        uint channelˉrecordˉoffset)
    {
        output.Emit(0x48, 0xBA);
        output.Emitˉu64(Kernelˉprocessˉcontract.ENDPOINT_MAGIC);
        output.Emit(0x49, 0x39, 0x13);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        foreach (var Field in new (uint Offset, uint Value)[]
        {
            (8, Kernelˉprocessˉcontract.ENDPOINT_VERSION),
            (12, Kernelˉprocessˉcontract.ENDPOINT_RECORD_BYTES),
            (Kernelˉprocessˉcontract.ENDPOINT_REFERENCE_OFFSET,
                capabilityˉreference),
            (Kernelˉprocessˉcontract.ENDPOINT_KIND_OFFSET,
                Kernelˉprocessˉcontract.ENDPOINT_KIND_SERVICE),
            (Kernelˉprocessˉcontract.ENDPOINT_CAPACITY_OFFSET,
                Kernelˉprocessˉcontract.CHANNEL_CAPACITY),
            (Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_PROCESS_OFFSET,
                providerˉreference),
            (Kernelˉprocessˉcontract.ENDPOINT_RESERVED_OFFSET, 0),
        })
        {
            Emitˉcompareˉendpointˉu32(output, Field.Offset, Field.Value);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        }
        output.Emit(0x49, 0x8B, 0x53,
            checked((byte)Kernelˉprocessˉcontract.ENDPOINT_CHANNEL_ADDRESS_OFFSET));
        output.Emit(0x49, 0x8D, 0x8C, 0x24);
        output.Emitˉu32(channelˉrecordˉoffset);
        output.Emit(0x48, 0x39, 0xCA);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
    }

    private static void Emitˉterminateˉchannelˉpeer(
        X64ˉcodeˉbuilder output,
        uint processˉid,
        uint peerˉstatus,
        bool wakeˉwaiter = false)
    {
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_OPEN);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        if (wakeˉwaiter)
        {
            Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET, 0);
            output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
            Emitˉincrementˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAKE_COUNT_OFFSET);
        }
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
            Emitˉstoreˉchannelˉu32(output, Offset, 0);
        }
        output.Emit(0x49, 0xC7, 0x42,
            checked((byte)Kernelˉprocessˉcontract.CHANNEL_SERVICE_DESTINATION_OFFSET),
            0x00, 0x00, 0x00, 0x00);
        output.Emit(0x49, 0xC7, 0x42,
            checked((byte)Kernelˉprocessˉcontract.CHANNEL_CLIENT_DESTINATION_OFFSET),
            0x00, 0x00, 0x00, 0x00);
        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_OFFSET,
            peerˉstatus);
        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_PEER_PROCESS_OFFSET,
            processˉid);
        Emitˉincrementˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_CLOSE_COUNT_OFFSET);
    }

    private static void Emitˉvalidateˉserviceˉfaultˉchannel(X64ˉcodeˉbuilder output)
    {
        foreach (var Offset in new[]
        {
            Kernelˉprocessˉcontract.CHANNEL_STATE_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_MESSAGE_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_SENDER_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_RECEIVER_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_SEND_COUNT_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_RECEIVE_COUNT_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_BYTE_LENGTH_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_SERVICE_DESTINATION_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_SERVICE_DESTINATION_OFFSET + sizeof(uint),
            Kernelˉprocessˉcontract.CHANNEL_SERVICE_CAPACITY_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_CLIENT_DESTINATION_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_CLIENT_DESTINATION_OFFSET + sizeof(uint),
            Kernelˉprocessˉcontract.CHANNEL_CLIENT_CAPACITY_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_RESERVED_OFFSET,
        })
        {
            Emitˉcompareˉchannelˉu32(output, Offset, 0);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        }
        foreach (var Field in new (uint Offset, uint Value)[]
        {
            (Kernelˉprocessˉcontract.CHANNEL_CAPACITY_OFFSET,
                Kernelˉprocessˉcontract.CHANNEL_CAPACITY),
            (Kernelˉprocessˉcontract.CHANNEL_REQUEST_COUNT_OFFSET, 1),
            (Kernelˉprocessˉcontract.CHANNEL_REPLY_COUNT_OFFSET, 0),
            (Kernelˉprocessˉcontract.CHANNEL_WAKE_COUNT_OFFSET, 1),
            (Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_OFFSET,
                Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_FAULTED),
            (Kernelˉprocessˉcontract.CHANNEL_PEER_PROCESS_OFFSET,
                Kernelˉprocessˉcontract.DIRECTORY_PROCESS_ID),
            (Kernelˉprocessˉcontract.CHANNEL_CLOSE_COUNT_OFFSET, 1),
        })
        {
            Emitˉcompareˉchannelˉu32(output, Field.Offset, Field.Value);
            output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        }
    }

    private static void Emitˉreopenˉchannel(X64ˉcodeˉbuilder output)
    {
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_OPEN);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_PEER_STATUS_OFFSET, 0);
        Emitˉstoreˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_PEER_PROCESS_OFFSET, 0);
    }

    private static void Emitˉinitializeˉresourceˉrecord(
        X64ˉcodeˉbuilder output,
        Kernelˉprocessˉimageˉartifacts image)
    {
        output.Emit(0x49, 0x8D, 0xBC, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET);
        output.Emit(0x31, 0xC0, 0xB9);
        output.Emitˉu32(Kernelˉprocessˉcontract.RESOURCE_STATE_BYTES / sizeof(ulong));
        output.Emit(0xFC, 0xF3, 0x48, 0xAB);

        Emitˉinitializeˉresourceˉrecordˉentry(
            output,
            Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET,
            Kernelˉprocessˉcontract.MODULE_RESOURCE_ID,
            Kernelˉprocessˉcontract.MODULE_RESOURCE_ATTRIBUTES,
            Kernelˉprocessˉcontract.INIT_RUNTIME_INPUT_PAGE,
            Kernelˉprocessˉcontract.CLIENT_RUNTIME_INPUT_PAGE,
            checked((uint)image.Admittedˉprogramˉbytes.Length),
            image.Admittedˉprogramˉdigest,
            image.Bootˉresourceˉserviceˉoffset);
        Emitˉinitializeˉresourceˉrecordˉentry(
            output,
            Kernelˉprocessˉcontract.SECOND_RESOURCE_RECORD_OFFSET,
            Kernelˉprocessˉcontract.BUDGET_RESOURCE_ID,
            Kernelˉprocessˉcontract.BUDGET_RESOURCE_ATTRIBUTES,
            Kernelˉprocessˉcontract.INIT_RUNTIME_BUDGET_PAGE,
            Kernelˉprocessˉcontract.CLIENT_RUNTIME_BUDGET_PAGE,
            Kernelˉprocessˉcontract.EXECUTION_BUDGET_BYTES,
            image.Executionˉbudgetˉdigest,
            image.Bootˉresourceˉserviceˉoffset);
        Emitˉinitializeˉstoreˉresourceˉrecord(output, image);
        Emitˉinitializeˉdirectoryˉresourceˉrecord(output, image);
    }

    private static void Emitˉinitializeˉstoreˉresourceˉrecord(
        X64ˉcodeˉbuilder output,
        Kernelˉprocessˉimageˉartifacts image)
    {
        Emitˉinitializeˉattachedˉresourceˉrecord(
            output,
            Kernelˉprocessˉcontract.STORE_RESOURCE_RECORD_OFFSET,
            Kernelˉprocessˉcontract.STORE_RESOURCE_ID,
            Kernelˉprocessˉcontract.STORE_RESOURCE_ATTRIBUTES,
            Kernelˉprocessˉcontract.INIT_RESOURCE_STORE_PAGE,
            checked((uint)image.Resourceˉstoreˉbytes.Length),
            image.Resourceˉstoreˉdigest,
            INIT_RECORD_SLOT_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE);
    }

    private static void Emitˉinitializeˉdirectoryˉresourceˉrecord(
        X64ˉcodeˉbuilder output,
        Kernelˉprocessˉimageˉartifacts image)
    {
        Emitˉinitializeˉattachedˉresourceˉrecord(
            output,
            Kernelˉprocessˉcontract.DIRECTORY_RESOURCE_RECORD_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_RESOURCE_ID,
            Kernelˉprocessˉcontract.DIRECTORY_RESOURCE_ATTRIBUTES,
            Kernelˉprocessˉcontract.DIRECTORY_SNAPSHOT_PAGE,
            checked((uint)image.Directoryˉsnapshotˉbytes.Length),
            image.Directoryˉsnapshotˉdigest,
            DIRECTORY_RECORD_SLOT_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE);
    }

    private static void Emitˉinitializeˉattachedˉresourceˉrecord(
        X64ˉcodeˉbuilder output,
        uint recordˉoffset,
        uint resourceˉid,
        uint resourceˉattributes,
        ulong resourceˉpage,
        uint resourceˉbytes,
        ImmutableArray<byte> resourceˉdigest,
        uint ownerˉrecordˉslot,
        uint ownerˉreference)
    {
        output.Emit(0x4D, 0x8D, 0x8C, 0x24);
        output.Emitˉu32(recordˉoffset);
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(Kernelˉprocessˉcontract.RESOURCE_MAGIC);
        output.Emit(0x49, 0x89, 0x01);
        Emitˉstoreˉresourceˉu32(output, 8, Kernelˉprocessˉcontract.RESOURCE_VERSION);
        Emitˉstoreˉresourceˉu32(output, 12, Kernelˉprocessˉcontract.RESOURCE_RECORD_BYTES);
        Emitˉstoreˉresourceˉu32(output, Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_STATE_ATTACHED);
        Emitˉstoreˉresourceˉu32(output, Kernelˉprocessˉcontract.RESOURCE_ID_OFFSET,
            resourceˉid);
        Emitˉstoreˉresourceˉu32(output, Kernelˉprocessˉcontract.RESOURCE_OWNER_OFFSET,
            ownerˉreference);
        Emitˉstoreˉresourceˉu32(output, Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET,
            ownerˉreference);
        Emitˉstoreˉresourceˉu32(output, Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET,
            resourceˉbytes);
        Emitˉstoreˉresourceˉu32(output, Kernelˉprocessˉcontract.RESOURCE_FLAGS_OFFSET,
            resourceˉattributes);
        Emitˉstoreˉresourceˉu32(output, Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET, 1);
        Emitˉstoreˉresourceˉu32(output, Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET, 1);

        Emitˉloadˉstackˉrax(output, ownerˉrecordˉslot);
        output.Emit(0x48, 0x8B, 0x50, (byte)Kernelˉprocessˉcontract.ROOT_ADDRESS_OFFSET);
        output.Emit(0x48, 0x8D, 0x82);
        output.Emitˉu32(checked((uint)(resourceˉpage * Kernelˉpagingˉcontract.PAGE_BYTES)));
        output.Emit(0x49, 0x89, 0x41, (byte)Kernelˉprocessˉcontract.RESOURCE_SOURCE_ADDRESS_OFFSET);
        output.Emit(0x49, 0x89, 0x51, (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_ROOT_OFFSET);
        Emitˉloadˉstackˉrax(output, ownerˉrecordˉslot);
        output.Emit(0x48, 0x8B, 0x40, (byte)Kernelˉprocessˉcontract.USER_DATA_ADDRESS_OFFSET);
        output.Emit(0x49, 0x89, 0x41, (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_DATA_OFFSET);
        output.Emit(0x48, 0x8D, 0x82);
        output.Emitˉu32(checked((uint)(resourceˉpage * Kernelˉpagingˉcontract.PAGE_BYTES)));
        output.Emit(0x49, 0x89, 0x41, (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_ADDRESS_OFFSET);
        for (var Offset = 0; Offset < resourceˉdigest.Length; Offset += sizeof(ulong))
        {
            output.Emit(0x48, 0xB8);
            output.Emitˉu64(BinaryPrimitives.ReadUInt64LittleEndian(
                resourceˉdigest.AsSpan().Slice(Offset)));
            output.Emit(0x49, 0x89, 0x41,
                checked((byte)(Kernelˉprocessˉcontract.RESOURCE_DIGEST_OFFSET + Offset)));
        }
        output.Emit(0x49, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_ADDRESS_OFFSET);
        output.Emit(0x48, 0x25, 0x00, 0xF0, 0x1F, 0x00, 0x48, 0xC1, 0xE8, 0x09);
        output.Emit(0x49, 0x03, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_ROOT_OFFSET);
        output.Emit(0x48, 0x05);
        output.Emitˉu32(checked((uint)(Kernelˉprocessˉcontract.USER_PT_PAGE *
            Kernelˉpagingˉcontract.PAGE_BYTES)));
        output.Emit(0x49, 0x89, 0x41, (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_PTE_OFFSET);
    }

    private static void Emitˉinitializeˉresourceˉrecordˉentry(
        X64ˉcodeˉbuilder output,
        uint recordˉoffset,
        uint resourceˉid,
        uint attributes,
        ulong sourceˉpage,
        ulong targetˉpage,
        uint resourceˉlength,
        ImmutableArray<byte> digest,
        uint serviceˉoffset)
    {
        output.Emit(0x4D, 0x8D, 0x8C, 0x24);
        output.Emitˉu32(recordˉoffset);
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
            resourceˉid);
        Emitˉstoreˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_OWNER_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE);
        Emitˉstoreˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_FLAGS_OFFSET,
            attributes);

        Emitˉloadˉstackˉrax(output, INIT_RECORD_SLOT_OFFSET);
        output.Emit(0x48, 0x8B, 0x40,
            (byte)Kernelˉprocessˉcontract.ROOT_ADDRESS_OFFSET, 0x48, 0x05);
        output.Emitˉu32(checked((uint)(sourceˉpage * Kernelˉpagingˉcontract.PAGE_BYTES)));
        output.Emit(0x49, 0x89, 0x41, (byte)Kernelˉprocessˉcontract.RESOURCE_SOURCE_ADDRESS_OFFSET);
        Emitˉstoreˉresourceˉu32(
            output, Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET,
            resourceˉlength);

        Emitˉloadˉstackˉrax(output, CLIENT_RECORD_SLOT_OFFSET);
        output.Emit(0x48, 0x8B, 0x50,
            (byte)Kernelˉprocessˉcontract.ROOT_ADDRESS_OFFSET);
        output.Emit(0x49, 0x89, 0x51, (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_ROOT_OFFSET);
        output.Emit(0x48, 0x8B, 0x40, (byte)Kernelˉprocessˉcontract.USER_DATA_ADDRESS_OFFSET);
        output.Emit(0x49, 0x89, 0x41, (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_DATA_OFFSET);
        output.Emit(0x48, 0x8D, 0x82);
        output.Emitˉu32(checked((uint)(targetˉpage * Kernelˉpagingˉcontract.PAGE_BYTES)));
        output.Emit(0x49, 0x89, 0x41, (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_ADDRESS_OFFSET);
        output.Emit(0x48, 0x8D, 0x82);
        output.Emitˉu32(checked((uint)(Kernelˉprocessˉcontract.USER_CODE_PAGE *
            Kernelˉpagingˉcontract.PAGE_BYTES + serviceˉoffset)));
        output.Emit(0x49, 0x89, 0x41, (byte)Kernelˉprocessˉcontract.RESOURCE_SERVICE_ADDRESS_OFFSET);
        for (var Offset = 0; Offset < digest.Length; Offset += sizeof(ulong))
        {
            output.Emit(0x48, 0xB8);
            output.Emitˉu64(BinaryPrimitives.ReadUInt64LittleEndian(
                digest.AsSpan().Slice(Offset)));
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

    private static void Emitˉvalidateˉstoreˉresource(
        X64ˉcodeˉbuilder output,
        Kernelˉprocessˉimageˉartifacts image,
        string failureˉlabel)
    {
        Emitˉvalidateˉattachedˉresource(
            output,
            Kernelˉprocessˉcontract.STORE_RESOURCE_RECORD_OFFSET,
            Kernelˉprocessˉcontract.STORE_RESOURCE_ID,
            Kernelˉprocessˉcontract.STORE_RESOURCE_ATTRIBUTES,
            Kernelˉprocessˉcontract.INIT_RESOURCE_STORE_PAGE,
            Kernelˉprocessˉcontract.INIT_STORE_DESCRIPTOR_OFFSET,
            checked((uint)image.Resourceˉstoreˉbytes.Length),
            image.Resourceˉstoreˉdigest,
            descriptorˉgeneration: 0,
            ownerˉrecordˉslot: INIT_RECORD_SLOT_OFFSET,
            ownerˉreference: Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE,
            failureˉlabel);
    }

    private static void Emitˉvalidateˉdirectoryˉresource(
        X64ˉcodeˉbuilder output,
        Kernelˉprocessˉimageˉartifacts image,
        string failureˉlabel)
    {
        Emitˉvalidateˉattachedˉresource(
            output,
            Kernelˉprocessˉcontract.DIRECTORY_RESOURCE_RECORD_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_RESOURCE_ID,
            Kernelˉprocessˉcontract.DIRECTORY_RESOURCE_ATTRIBUTES,
            Kernelˉprocessˉcontract.DIRECTORY_SNAPSHOT_PAGE,
            Kernelˉprocessˉcontract.DIRECTORY_DESCRIPTOR_OFFSET,
            checked((uint)image.Directoryˉsnapshotˉbytes.Length),
            image.Directoryˉsnapshotˉdigest,
            Kernelˉprocessˉcontract.DIRECTORY_DESCRIPTOR_GENERATION,
            DIRECTORY_RECORD_SLOT_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE,
            failureˉlabel);
    }

    private static void Emitˉvalidateˉattachedˉresource(
        X64ˉcodeˉbuilder output,
        uint recordˉoffset,
        uint resourceˉid,
        uint resourceˉattributes,
        ulong resourceˉpage,
        uint descriptorˉoffset,
        uint resourceˉbytes,
        ImmutableArray<byte> resourceˉdigest,
        uint descriptorˉgeneration,
        uint ownerˉrecordˉslot,
        uint ownerˉreference,
        string failureˉlabel)
    {
        Emitˉloadˉstateˉresource(output, recordˉoffset);
        Emitˉvalidateˉresourceˉheader(output, failureˉlabel);
        foreach (var Field in new (uint Offset, uint Value)[]
        {
            (Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET,
                Kernelˉprocessˉcontract.RESOURCE_STATE_ATTACHED),
            (Kernelˉprocessˉcontract.RESOURCE_ID_OFFSET,
                resourceˉid),
            (Kernelˉprocessˉcontract.RESOURCE_OWNER_OFFSET,
                ownerˉreference),
            (Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET,
                ownerˉreference),
            (Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET,
                resourceˉbytes),
            (Kernelˉprocessˉcontract.RESOURCE_FLAGS_OFFSET,
                resourceˉattributes),
            (Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET, 1),
            (Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET, 1),
        })
        {
            Emitˉcompareˉresourceˉu32(output, Field.Offset, Field.Value);
            output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        }
        for (var Offset = 0; Offset < resourceˉdigest.Length; Offset += sizeof(ulong))
        {
            output.Emit(0x48, 0xB8);
            output.Emitˉu64(BinaryPrimitives.ReadUInt64LittleEndian(
                resourceˉdigest.AsSpan().Slice(Offset)));
            output.Emit(0x49, 0x39, 0x41,
                checked((byte)(Kernelˉprocessˉcontract.RESOURCE_DIGEST_OFFSET + Offset)));
            output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        }
        Emitˉloadˉstackˉrax(output, ownerˉrecordˉslot);
        output.Emit(0x48, 0x8B, 0x50, (byte)Kernelˉprocessˉcontract.ROOT_ADDRESS_OFFSET);
        output.Emit(0x48, 0x8D, 0x82);
        output.Emitˉu32(checked((uint)(resourceˉpage * Kernelˉpagingˉcontract.PAGE_BYTES)));
        output.Emit(0x49, 0x39, 0x41, (byte)Kernelˉprocessˉcontract.RESOURCE_SOURCE_ADDRESS_OFFSET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        output.Emit(0x49, 0x39, 0x41, (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_ADDRESS_OFFSET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        output.Emit(0x49, 0x39, 0x51, (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_ROOT_OFFSET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        Emitˉloadˉstackˉrax(output, ownerˉrecordˉslot);
        output.Emit(0x48, 0x8B, 0x40, (byte)Kernelˉprocessˉcontract.USER_DATA_ADDRESS_OFFSET);
        output.Emit(0x49, 0x39, 0x41, (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_DATA_OFFSET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        output.Emit(0x49, 0x8B, 0x49,
            (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_ADDRESS_OFFSET);
        output.Emit(0x48, 0x39, 0x88);
        output.Emitˉu32(descriptorˉoffset);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        output.Emit(0x81, 0xB8);
        output.Emitˉu32(descriptorˉoffset + sizeof(ulong));
        output.Emitˉu32(resourceˉbytes);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        if (descriptorˉgeneration != 0)
        {
            output.Emit(0x81, 0xB8);
            output.Emitˉu32(descriptorˉoffset + 12);
            output.Emitˉu32(descriptorˉgeneration);
            output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        }
        output.Emit(0x49, 0x8B, 0x51,
            (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_PTE_OFFSET);
        output.Emit(0x48, 0x8B, 0x0A, 0x48, 0x0F, 0xBA, 0xF1, 0x05);
        output.Emit(0x48, 0x83, 0xE1,
            unchecked((byte)~Kernelˉpagingˉcontract.ENTRY_ACCESSED));
        output.Emit(0x48, 0x0F, 0xBA, 0xE9, 0x3F);
        output.Emit(0x49, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_SOURCE_ADDRESS_OFFSET);
        output.Emit(0x48, 0x83, 0xC8,
            (byte)(Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER));
        output.Emit(0x48, 0x0F, 0xBA, 0xE8, 0x3F);
        output.Emit(0x48, 0x39, 0xC1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
    }

    private static void Emitˉallocateˉextent(
        X64ˉcodeˉbuilder output,
        ImmutableArray<Objectˉrelocation>.Builder relocations,
        ulong allocationˉpages)
    {
        output.Emit(0x4C, 0x89, 0xE1, 0xBA);
        output.Emitˉu32(checked((uint)allocationˉpages));
        Emitˉexternalˉcall(output, relocations, 10);
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

    private static void Emitˉreclaimˉandˉrebuildˉclient(
        X64ˉcodeˉbuilder output,
        ImmutableArray<Objectˉrelocation>.Builder relocations,
        Kernelˉprocessˉimageˉartifacts image)
    {
        Emitˉloadˉstackˉr13(output, CLIENT_RECORD_SLOT_OFFSET);
        Emitˉcompareˉrecordˉu32(output, Kernelˉprocessˉcontract.PROCESS_GENERATION_OFFSET,
            Kernelˉprocessˉcontract.FIRST_CLIENT_GENERATION);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉrecordˉu32(output, Kernelˉprocessˉcontract.RESULT_OFFSET,
            Kernelˉprocessˉcontract.EXPECTED_RESULT);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);

        output.Emit(0x4C, 0x89, 0xE1, 0x49, 0x8B, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.ROOT_ADDRESS_OFFSET);
        output.Emit(0x41, 0xB8);
        output.Emitˉu32(checked((uint)Kernelˉprocessˉcontract.CLIENT_ALLOCATION_PAGES));
        Emitˉexternalˉcall(output, relocations, 12);
        output.Emit(0x49, 0x3B, 0x85);
        output.Emitˉu32(Kernelˉprocessˉcontract.ROOT_ADDRESS_OFFSET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x49, 0x83, 0x7C, 0x24, 0x20,
            checked((byte)(Kernelˉmemoryˉcontract.ARENA_PAGES -
                Kernelˉprocessˉcontract.CLIENT_ALLOCATION_PAGES)));
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x49, 0x81, 0x7C, 0x24, 0x28);
        output.Emitˉu32(checked((uint)Kernelˉprocessˉcontract.CLIENT_ALLOCATION_PAGES));
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);

        Emitˉallocateˉextent(
            output, relocations, Kernelˉprocessˉcontract.CLIENT_ALLOCATION_PAGES);
        output.Emit(0x4D, 0x3B, 0xB5);
        output.Emitˉu32(Kernelˉprocessˉcontract.ROOT_ADDRESS_OFFSET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉinitializeˉrecord(
            output,
            image.Interpreterˉdigest,
            image.Admittedˉprogramˉdigest,
            Kernelˉprocessˉcontract.CLIENT_PROCESS_ID,
            Kernelˉprocessˉcontract.CLIENT_THREAD_ID,
            Kernelˉprocessˉcontract.SECOND_CLIENT_GENERATION,
            Kernelˉprocessˉcontract.CLIENT_CAPABILITY_RIGHTS,
            Kernelˉprocessˉcontract.ROLE_BYTECODE_INTERPRETER,
            Kernelˉprocessˉcontract.CLIENT_SYSCALL_BUDGET);
        Emitˉcopyˉkernelˉtables(output);
        Emitˉpopulateˉprocessˉtable(
            output, "client_reuse", Kernelˉprocessˉcontract.CLIENT_CODE_PAGES,
            Kernelˉprocessˉcontract.CLIENT_STACK_PAGE,
            Kernelˉprocessˉcontract.CLIENT_STACK_PAGES,
            Kernelˉprocessˉcontract.CLIENT_DATA_PAGE,
            Kernelˉprocessˉcontract.CLIENT_RUNTIME_INPUT_PAGE,
            Kernelˉprocessˉcontract.CLIENT_RUNTIME_BUDGET_PAGE,
            Kernelˉprocessˉcontract.CLIENT_SERVICE_RESPONSE_PAGE,
            runtimeˉinputˉpresent: false,
            storeˉpresent: false,
            directoryˉpresent: false,
            clearˉresourceˉaliases: true);
        Emitˉcopyˉuserˉbytes(
            output, relocations, image.Clientˉimageˉbytes.Length, 1,
            Kernelˉprocessˉcontract.USER_CODE_PAGE);
        Emitˉinitializeˉuserˉcontext(
            output, Kernelˉprocessˉcontract.CLIENT_DATA_PAGE,
            Kernelˉprocessˉcontract.CLIENT_INSTRUCTION_BUDGET,
            Kernelˉprocessˉcontract.CLIENT_CALL_DEPTH_BUDGET,
            true);
        Emitˉrebindˉendpointˉclient(
            output,
            Kernelˉprocessˉcontract.ENDPOINT_RECORD_OFFSET,
            Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_CAPABILITY_REFERENCE,
            Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE,
            6);
        Emitˉrebindˉendpointˉclient(
            output,
            Kernelˉprocessˉcontract.DIRECTORY_ENDPOINT_RECORD_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_CAPABILITY_REFERENCE,
            Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE,
            4);
        Emitˉvalidateˉexhaustedˉallocator(output);
    }

    private static void Emitˉvalidateˉclientˉrecordˉarena(X64ˉcodeˉbuilder output)
    {
        output.Emit(0x65, 0x48, 0x8B, 0x04, 0x25);
        output.Emitˉu32(Kernelˉprocessˉcontract.USER_DATA_ADDRESS_OFFSET);
        output.Emit(0x48, 0x8B, 0x48,
            (byte)Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_POINTER_OFFSET);
        output.Emit(0x48, 0x8D, 0x90);
        output.Emitˉu32(Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_OFFSET);
        output.Emit(0x48, 0x39, 0xD1);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x81, 0x78,
            (byte)Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET);
        output.Emitˉu32(Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_BYTES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x81, 0x78,
            (byte)Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET);
        output.Emitˉu32(Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_USED_BYTES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
    }

    private static void Emitˉvalidateˉexhaustedˉallocator(X64ˉcodeˉbuilder output)
    {
        Emitˉvalidateˉmemoryˉstate(output);
        output.Emit(0x49, 0x81, 0x7C, 0x24, 0x20);
        output.Emitˉu32(checked((uint)Kernelˉmemoryˉcontract.ARENA_PAGES));
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x49, 0x83, 0x7C, 0x24, 0x28, 0x00);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
    }

    private static void Emitˉinitializeˉrecord(
        X64ˉcodeˉbuilder output,
        ImmutableArray<byte> digest,
        ImmutableArray<byte> programˉdigest,
        uint processˉid,
        uint threadˉid,
        uint processˉgeneration,
        uint capabilityˉrights,
        uint role,
        uint syscallˉbudget)
    {
        var Isˉinit = role == Kernelˉprocessˉcontract.ROLE_INIT_SERVICE;
        var Isˉdirectory = role == Kernelˉprocessˉcontract.ROLE_DIRECTORY_SERVICE;
        var Stackˉpage = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_STACK_PAGE
            : Isˉdirectory
                ? Kernelˉprocessˉcontract.DIRECTORY_STACK_PAGE
                : Kernelˉprocessˉcontract.CLIENT_STACK_PAGE;
        var Stackˉpages = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_STACK_PAGES
            : Isˉdirectory
                ? Kernelˉprocessˉcontract.DIRECTORY_STACK_PAGES
                : Kernelˉprocessˉcontract.CLIENT_STACK_PAGES;
        var Dataˉpage = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_DATA_PAGE
            : Isˉdirectory
                ? Kernelˉprocessˉcontract.DIRECTORY_DATA_PAGE
                : Kernelˉprocessˉcontract.CLIENT_DATA_PAGE;
        var Serviceˉresponseˉpage = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_SERVICE_RESPONSE_PAGE
            : Isˉdirectory
                ? Kernelˉprocessˉcontract.DIRECTORY_SERVICE_RESPONSE_PAGE
                : Kernelˉprocessˉcontract.CLIENT_SERVICE_RESPONSE_PAGE;
        var Codeˉpages = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_CODE_PAGES
            : Isˉdirectory
                ? Kernelˉprocessˉcontract.DIRECTORY_CODE_PAGES
                : Kernelˉprocessˉcontract.CLIENT_CODE_PAGES;
        var Memoryˉpageˉbudget = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_MEMORY_PAGE_BUDGET
            : Isˉdirectory
                ? Kernelˉprocessˉcontract.DIRECTORY_MEMORY_PAGE_BUDGET
                : Kernelˉprocessˉcontract.CLIENT_MEMORY_PAGE_BUDGET;
        var Instructionˉbudget = Isˉinit
            ? Kernelˉprocessˉcontract.INIT_INSTRUCTION_BUDGET
            : Isˉdirectory
                ? Kernelˉprocessˉcontract.DIRECTORY_INSTRUCTION_BUDGET
                : Kernelˉprocessˉcontract.CLIENT_INSTRUCTION_BUDGET;
        var Runtimeˉkind = Isˉinit || Isˉdirectory
            ? Kernelˉprocessˉcontract.RUNTIME_KIND_AOT_SERVICE
            : Kernelˉprocessˉcontract.RUNTIME_KIND_BYTECODE_INTERPRETER;
        var Runtimeˉprofile = Isˉinit
            ? Kernelˉprocessˉcontract.RUNTIME_PROFILE_RESOURCE_OWNER
            : Isˉdirectory
                ? Kernelˉprocessˉcontract.RUNTIME_PROFILE_DIRECTORY_OWNER
                : Kernelˉprocessˉcontract.RUNTIME_PROFILE_GRANTED_RESOURCE_DIRECTORY_INTERPRETER;
        var Capabilityˉreference = Isˉdirectory
            ? Kernelˉprocessˉcontract.DIRECTORY_CAPABILITY_REFERENCE
            : Kernelˉprocessˉcontract.RESOURCE_CAPABILITY_REFERENCE;
        var Endpointˉrecordˉoffset = Isˉdirectory
            ? Kernelˉprocessˉcontract.DIRECTORY_ENDPOINT_RECORD_OFFSET
            : Kernelˉprocessˉcontract.ENDPOINT_RECORD_OFFSET;
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
        Emitˉstoreˉrecordˉu32(output, 104, Isˉinit || Isˉdirectory
            ? Kernelˉprocessˉcontract.PROVIDER_HANDLE_BUDGET
            : Kernelˉprocessˉcontract.CLIENT_HANDLE_BUDGET);
        Emitˉstoreˉrecordˉu32(
            output, Kernelˉprocessˉcontract.SYSCALL_BUDGET_OFFSET, syscallˉbudget);
        Emitˉstoreˉrecordˉu32(output, 112, Capabilityˉreference & 0xFFFF);
        Emitˉstoreˉrecordˉu32(output, 116, Capabilityˉreference >> 16);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.CAPABILITY_RIGHTS_OFFSET,
            capabilityˉrights);
        Emitˉstoreˉrecordˉu32(output, 124, Kernelˉprocessˉcontract.CHANNEL_CAPACITY);
        output.Emit(0x49, 0x8D, 0x84, 0x24);
        output.Emitˉu32(Endpointˉrecordˉoffset);
        output.Emit(0x49, 0x89, 0x85);
        output.Emitˉu32(Kernelˉprocessˉcontract.ENDPOINT_ADDRESS_OFFSET);
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
        Emitˉstoreˉrecordˉu32(
            output, Kernelˉprocessˉcontract.PROCESS_GENERATION_OFFSET, processˉgeneration);
        Emitˉstoreˉrecordˉaddress(
            output,
            Kernelˉprocessˉcontract.USER_SERVICE_RESPONSE_ADDRESS_OFFSET,
            Serviceˉresponseˉpage);
        if (!Isˉinit && !Isˉdirectory)
        {
            Emitˉstoreˉrecordˉu32(
                output,
                Kernelˉprocessˉcontract.SECOND_CAPABILITY_REFERENCE_OFFSET,
                Kernelˉprocessˉcontract.DIRECTORY_CAPABILITY_REFERENCE);
            Emitˉstoreˉrecordˉu32(
                output,
                Kernelˉprocessˉcontract.SECOND_CAPABILITY_RIGHTS_OFFSET,
                Kernelˉprocessˉcontract.CLIENT_CAPABILITY_RIGHTS);
            output.Emit(0x49, 0x8D, 0x84, 0x24);
            output.Emitˉu32(Kernelˉprocessˉcontract.DIRECTORY_ENDPOINT_RECORD_OFFSET);
            output.Emit(0x49, 0x89, 0x85);
            output.Emitˉu32(Kernelˉprocessˉcontract.SECOND_ENDPOINT_ADDRESS_OFFSET);
        }
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
        ulong runtimeˉbudgetˉpage,
        ulong responseˉpage,
        bool runtimeˉinputˉpresent,
        bool storeˉpresent,
        bool directoryˉpresent,
        bool clearˉresourceˉaliases)
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
        Emitˉwriteˉuserˉpte(
            output,
            responseˉpage,
            Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉpagingˉcontract.ENTRY_WRITABLE |
            Kernelˉprocessˉcontract.ENTRY_USER | Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        if (runtimeˉinputˉpresent)
        {
            Emitˉwriteˉuserˉpte(output, runtimeˉinputˉpage,
                Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER |
                Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
            Emitˉwriteˉuserˉpte(output, runtimeˉbudgetˉpage,
                Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER |
                Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        }
        if (storeˉpresent)
        {
            Emitˉwriteˉuserˉpte(output, Kernelˉprocessˉcontract.INIT_RESOURCE_STORE_PAGE,
                Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER |
                Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        }
        if (directoryˉpresent)
        {
            Emitˉwriteˉuserˉpte(output, Kernelˉprocessˉcontract.DIRECTORY_SNAPSHOT_PAGE,
                Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER |
                Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        }
        if (clearˉresourceˉaliases)
        {
            Emitˉclearˉprocessˉpte(output, runtimeˉinputˉpage);
            Emitˉclearˉprocessˉpte(output, runtimeˉbudgetˉpage);
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
        uint callˉdepthˉbudget,
        bool includeˉrecordˉarena)
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
        if (includeˉrecordˉarena)
        {
            output.Emit(0x48, 0x8D, 0x87);
            output.Emitˉu32(Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_OFFSET);
            output.Emit(0x48, 0x89, 0x47,
                (byte)Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_POINTER_OFFSET);
            output.Emit(0xC7, 0x47,
                (byte)Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET);
            output.Emitˉu32(Kernelˉprocessˉcontract.CLIENT_RECORD_ARENA_BYTES);
        }
    }

    private static void Emitˉinitializeˉstoreˉdescriptor(
        X64ˉcodeˉbuilder output,
        uint storeˉbytes)
    {
        output.Emit(0x49, 0x8D, 0x86);
        output.Emitˉu32(checked((uint)(Kernelˉprocessˉcontract.INIT_RESOURCE_STORE_PAGE *
            Kernelˉpagingˉcontract.PAGE_BYTES)));
        output.Emit(0x49, 0x89, 0x86);
        output.Emitˉu32(checked((uint)(Kernelˉprocessˉcontract.INIT_DATA_PAGE *
            Kernelˉpagingˉcontract.PAGE_BYTES + Kernelˉprocessˉcontract.INIT_STORE_DESCRIPTOR_OFFSET)));
        output.Emit(0x41, 0xC7, 0x86);
        output.Emitˉu32(checked((uint)(Kernelˉprocessˉcontract.INIT_DATA_PAGE *
            Kernelˉpagingˉcontract.PAGE_BYTES + Kernelˉprocessˉcontract.INIT_STORE_DESCRIPTOR_OFFSET +
            sizeof(ulong))));
        output.Emitˉu32(storeˉbytes);
    }

    private static void Emitˉinitializeˉdirectoryˉdescriptor(
        X64ˉcodeˉbuilder output,
        uint snapshotˉbytes)
    {
        output.Emit(0x49, 0x8D, 0x86);
        output.Emitˉu32(checked((uint)(Kernelˉprocessˉcontract.DIRECTORY_SNAPSHOT_PAGE *
            Kernelˉpagingˉcontract.PAGE_BYTES)));
        output.Emit(0x49, 0x89, 0x86);
        output.Emitˉu32(checked((uint)(Kernelˉprocessˉcontract.DIRECTORY_DATA_PAGE *
            Kernelˉpagingˉcontract.PAGE_BYTES +
            Kernelˉprocessˉcontract.DIRECTORY_DESCRIPTOR_OFFSET)));
        output.Emit(0x41, 0xC7, 0x86);
        output.Emitˉu32(checked((uint)(Kernelˉprocessˉcontract.DIRECTORY_DATA_PAGE *
            Kernelˉpagingˉcontract.PAGE_BYTES +
            Kernelˉprocessˉcontract.DIRECTORY_DESCRIPTOR_OFFSET + sizeof(ulong))));
        output.Emitˉu32(snapshotˉbytes);
        output.Emit(0x41, 0xC7, 0x86);
        output.Emitˉu32(checked((uint)(Kernelˉprocessˉcontract.DIRECTORY_DATA_PAGE *
            Kernelˉpagingˉcontract.PAGE_BYTES +
            Kernelˉprocessˉcontract.DIRECTORY_DESCRIPTOR_OFFSET + 12)));
        output.Emitˉu32(Kernelˉprocessˉcontract.DIRECTORY_DESCRIPTOR_GENERATION);
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
        Emitˉgate(output, relocations, Kernelˉexceptionˉcontract.INVALID_OPCODE_GATE_OFFSET, 17);
        Emitˉgate(output, relocations, Kernelˉexceptionˉcontract.GENERAL_PROTECTION_GATE_OFFSET, 15);
        Emitˉgate(output, relocations, 14 * Kernelˉexceptionˉcontract.IDT_GATE_BYTES, 16);
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
        Emitˉexternalˉcall(output, relocations, 14);
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
        output.Emitˉu32(Kernelˉprocessˉcontract.USER_DATA_ADDRESS_OFFSET);
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

    private static void Emitˉvalidateˉcapability(
        X64ˉcodeˉbuilder output,
        uint right,
        bool provider)
    {
        var Resourceˉlabel = $"process_capability_resource_{output.Position}";
        var Selectedˉlabel = $"process_capability_selected_{output.Position}";
        var Directoryˉobjectˉlabel = $"process_capability_directory_object_{output.Position}";
        var Objectˉselectedˉlabel = $"process_capability_object_selected_{output.Position}";
        var Directoryˉfieldsˉlabel = $"process_capability_directory_fields_{output.Position}";
        var Fieldsˉcompleteˉlabel = $"process_capability_fields_complete_{output.Position}";

        output.Emit(0x81, 0xFE);
        output.Emitˉu32(Kernelˉprocessˉcontract.RESOURCE_CAPABILITY_REFERENCE);
        output.Jumpˉif(CONDITION_EQUAL, Resourceˉlabel);
        output.Emit(0x81, 0xFE);
        output.Emitˉu32(Kernelˉprocessˉcontract.DIRECTORY_CAPABILITY_REFERENCE);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        output.Emit(0x65, 0xF7, 0x04, 0x25);
        output.Emitˉu32(provider
            ? Kernelˉprocessˉcontract.CAPABILITY_RIGHTS_OFFSET
            : Kernelˉprocessˉcontract.SECOND_CAPABILITY_RIGHTS_OFFSET);
        output.Emitˉu32(right);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_FAILURE_LABEL);
        output.Emit(0x65, 0x4C, 0x8B, 0x1C, 0x25);
        output.Emitˉu32(provider
            ? Kernelˉprocessˉcontract.ENDPOINT_ADDRESS_OFFSET
            : Kernelˉprocessˉcontract.SECOND_ENDPOINT_ADDRESS_OFFSET);
        output.Jump(Selectedˉlabel);

        output.Mark(Resourceˉlabel);
        output.Emit(0x65, 0xF7, 0x04, 0x25);
        output.Emitˉu32(Kernelˉprocessˉcontract.CAPABILITY_RIGHTS_OFFSET);
        output.Emitˉu32(right);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_FAILURE_LABEL);

        // Resolve the process-local capability entry through the kernel-owned
        // endpoint object. R11 is architecturally clobbered by SYSCALL and its
        // user value is already retained in the process record.
        output.Emit(0x65, 0x4C, 0x8B, 0x1C, 0x25);
        output.Emitˉu32(Kernelˉprocessˉcontract.ENDPOINT_ADDRESS_OFFSET);
        output.Mark(Selectedˉlabel);

        output.Emit(0x81, 0xFE);
        output.Emitˉu32(Kernelˉprocessˉcontract.DIRECTORY_CAPABILITY_REFERENCE);
        output.Jumpˉif(CONDITION_EQUAL, Directoryˉobjectˉlabel);
        output.Emit(0x49, 0x8D, 0x94, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.ENDPOINT_RECORD_OFFSET);
        output.Jump(Objectˉselectedˉlabel);
        output.Mark(Directoryˉobjectˉlabel);
        output.Emit(0x49, 0x8D, 0x94, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.DIRECTORY_ENDPOINT_RECORD_OFFSET);
        output.Mark(Objectˉselectedˉlabel);
        output.Emit(0x49, 0x39, 0xD3);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        output.Emit(0x48, 0xBA);
        output.Emitˉu64(Kernelˉprocessˉcontract.ENDPOINT_MAGIC);
        output.Emit(0x49, 0x39, 0x13);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        foreach (var Field in new (uint Offset, uint Value)[]
        {
            (8, Kernelˉprocessˉcontract.ENDPOINT_VERSION),
            (12, Kernelˉprocessˉcontract.ENDPOINT_RECORD_BYTES),
            (Kernelˉprocessˉcontract.ENDPOINT_STATE_OFFSET,
                Kernelˉprocessˉcontract.ENDPOINT_STATE_OPEN),
            (Kernelˉprocessˉcontract.ENDPOINT_KIND_OFFSET,
                Kernelˉprocessˉcontract.ENDPOINT_KIND_SERVICE),
            (Kernelˉprocessˉcontract.ENDPOINT_CAPACITY_OFFSET,
                Kernelˉprocessˉcontract.CHANNEL_CAPACITY),
            (Kernelˉprocessˉcontract.ENDPOINT_CLOSE_COUNT_OFFSET, 0),
            (Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_STATUS_OFFSET,
                Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_STATUS_OPEN),
            (Kernelˉprocessˉcontract.ENDPOINT_RESERVED_OFFSET, 0),
        })
        {
            output.Emit(0x41, 0x81, 0x7B, checked((byte)Field.Offset));
            output.Emitˉu32(Field.Value);
            output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        }
        output.Emit(0x41, 0x39, 0x73,
            checked((byte)Kernelˉprocessˉcontract.ENDPOINT_REFERENCE_OFFSET));
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        output.Emit(0x81, 0xFE);
        output.Emitˉu32(Kernelˉprocessˉcontract.DIRECTORY_CAPABILITY_REFERENCE);
        output.Jumpˉif(CONDITION_EQUAL, Directoryˉfieldsˉlabel);
        output.Emit(0x41, 0x81, 0x7B,
            checked((byte)Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_PROCESS_OFFSET));
        output.Emitˉu32(Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        output.Emit(0x49, 0x8B, 0x53,
            checked((byte)Kernelˉprocessˉcontract.ENDPOINT_CHANNEL_ADDRESS_OFFSET));
        output.Emit(0x49, 0x8D, 0x8C, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET);
        output.Jump(Fieldsˉcompleteˉlabel);
        output.Mark(Directoryˉfieldsˉlabel);
        output.Emit(0x41, 0x81, 0x7B,
            checked((byte)Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_PROCESS_OFFSET));
        output.Emitˉu32(Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        output.Emit(0x49, 0x8B, 0x53,
            checked((byte)Kernelˉprocessˉcontract.ENDPOINT_CHANNEL_ADDRESS_OFFSET));
        output.Emit(0x49, 0x8D, 0x8C, 0x24);
        output.Emitˉu32(Kernelˉprocessˉcontract.DIRECTORY_CHANNEL_RECORD_OFFSET);
        output.Mark(Fieldsˉcompleteˉlabel);
        output.Emit(0x48, 0x39, 0xCA);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        output.Emit(0x50, 0x48, 0xB8);
        output.Emitˉu64(Kernelˉprocessˉcontract.CHANNEL_MAGIC);
        output.Emit(0x48, 0x39, 0x02, 0x58);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        foreach (var Field in new (uint Offset, uint Value)[]
        {
            (8, Kernelˉprocessˉcontract.CHANNEL_VERSION),
            (12, Kernelˉprocessˉcontract.CHANNEL_RECORD_BYTES),
            (Kernelˉprocessˉcontract.CHANNEL_CAPACITY_OFFSET,
                Kernelˉprocessˉcontract.CHANNEL_CAPACITY),
        })
        {
            output.Emit(0x81, 0x7A, checked((byte)Field.Offset));
            output.Emitˉu32(Field.Value);
            output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        }
        // Match the endpoint participant to this exact process generation.
        output.Emit(0x65, 0x8B, 0x14, 0x25);
        output.Emitˉu32(Kernelˉprocessˉcontract.PROCESS_GENERATION_OFFSET);
        output.Emit(0xC1, 0xE2, 0x10, 0x65, 0x0B, 0x14, 0x25);
        output.Emitˉu32(24);
        output.Emit(0x41, 0x39, 0x53, checked((byte)(provider
            ? Kernelˉprocessˉcontract.ENDPOINT_PROVIDER_PROCESS_OFFSET
            : Kernelˉprocessˉcontract.ENDPOINT_CLIENT_PROCESS_OFFSET)));
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        output.Emit(0x41, 0xFF, 0x43,
            checked((byte)Kernelˉprocessˉcontract.ENDPOINT_RESOLUTION_COUNT_OFFSET));
    }

    private static void Emitˉvalidateˉcodeˉrange(X64ˉcodeˉbuilder output)
    {
        // R8/R9D describe a nonempty source wholly inside this process's RX image.
        output.Emit(0x45, 0x85, 0xC9);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_FAILURE_LABEL);
        output.Emit(0x41, 0x81, 0xF9);
        output.Emitˉu32(Kernelˉprocessˉcontract.MAXIMUM_CHANNEL_MESSAGE_BYTES);
        output.Jumpˉif(CONDITION_ABOVE, SYSCALL_FAILURE_LABEL);
        output.Emit(0x65, 0x48, 0x8B, 0x04, 0x25);
        output.Emitˉu32(Kernelˉprocessˉcontract.USER_CODE_ADDRESS_OFFSET);
        output.Emit(0x49, 0x39, 0xC0);
        output.Jumpˉif(CONDITION_BELOW, SYSCALL_FAILURE_LABEL);
        output.Emit(0x4C, 0x89, 0xC2, 0x44, 0x89, 0xC9, 0x48, 0x01, 0xCA);
        output.Jumpˉif(CONDITION_BELOW, SYSCALL_FAILURE_LABEL);
        output.Emit(0x65, 0x8B, 0x0C, 0x25);
        output.Emitˉu32(Kernelˉprocessˉcontract.CODE_PAGE_COUNT_OFFSET);
        output.Emit(0x48, 0xC1, 0xE1, 0x0C, 0x48, 0x01, 0xC8, 0x48, 0x39, 0xC2);
        output.Jumpˉif(CONDITION_ABOVE, SYSCALL_FAILURE_LABEL);
    }

    private static void Emitˉvalidateˉdataˉrange(
        X64ˉcodeˉbuilder output,
        string labelˉsuffix,
        bool addressˉisˉr10,
        bool lengthˉisˉedi)
    {
        // IPC data must be wholly inside either the process context page or its
        // dedicated one-page RW/NX response extent. No intervening RO page is
        // admitted by treating the two mappings as one broad range.
        var Responseˉlabel = $"process_data_range_response_{labelˉsuffix}";
        var Acceptedˉlabel = $"process_data_range_accepted_{labelˉsuffix}";
        output.Emit(lengthˉisˉedi ? [0x85, 0xFF] : [0x45, 0x85, 0xC9]);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_FAILURE_LABEL);
        if (lengthˉisˉedi)
        {
            output.Emit(0x81, 0xFF);
        }
        else
        {
            output.Emit(0x41, 0x81, 0xF9);
        }
        output.Emitˉu32(Kernelˉprocessˉcontract.MAXIMUM_CHANNEL_MESSAGE_BYTES);
        output.Jumpˉif(CONDITION_ABOVE, SYSCALL_FAILURE_LABEL);
        output.Emit(0x65, 0x48, 0x8B, 0x04, 0x25);
        output.Emitˉu32(Kernelˉprocessˉcontract.USER_DATA_ADDRESS_OFFSET);
        output.Emit(addressˉisˉr10 ? [0x49, 0x39, 0xC2] : [0x49, 0x39, 0xC0]);
        output.Jumpˉif(CONDITION_BELOW, Responseˉlabel);
        output.Emit(addressˉisˉr10 ? [0x4C, 0x89, 0xD2] : [0x4C, 0x89, 0xC2]);
        output.Emit(lengthˉisˉedi ? [0x89, 0xF9] : [0x44, 0x89, 0xC9]);
        output.Emit(0x48, 0x01, 0xCA);
        output.Jumpˉif(CONDITION_BELOW, SYSCALL_FAILURE_LABEL);
        output.Emit(0x48, 0x05);
        output.Emitˉu32(checked((uint)Kernelˉpagingˉcontract.PAGE_BYTES));
        output.Emit(0x48, 0x39, 0xC2);
        output.Jumpˉif(CONDITION_BELOW, Acceptedˉlabel);
        output.Jumpˉif(CONDITION_EQUAL, Acceptedˉlabel);

        output.Mark(Responseˉlabel);
        output.Emit(0x65, 0x48, 0x8B, 0x04, 0x25);
        output.Emitˉu32(Kernelˉprocessˉcontract.USER_SERVICE_RESPONSE_ADDRESS_OFFSET);
        output.Emit(addressˉisˉr10 ? [0x49, 0x39, 0xC2] : [0x49, 0x39, 0xC0]);
        output.Jumpˉif(CONDITION_BELOW, SYSCALL_FAILURE_LABEL);
        output.Emit(addressˉisˉr10 ? [0x4C, 0x89, 0xD2] : [0x4C, 0x89, 0xC2]);
        output.Emit(lengthˉisˉedi ? [0x89, 0xF9] : [0x44, 0x89, 0xC9]);
        output.Emit(0x48, 0x01, 0xCA);
        output.Jumpˉif(CONDITION_BELOW, SYSCALL_FAILURE_LABEL);
        output.Emit(0x48, 0x05);
        output.Emitˉu32(checked((uint)Kernelˉpagingˉcontract.PAGE_BYTES));
        output.Emit(0x48, 0x39, 0xC2);
        output.Jumpˉif(CONDITION_ABOVE, SYSCALL_FAILURE_LABEL);
        output.Mark(Acceptedˉlabel);
    }

    private static void Emitˉgrantˉbootˉresource(
        X64ˉcodeˉbuilder output,
        uint programˉlength)
    {
        Emitˉloadˉgsˉchannelˉr10(output);
        Emitˉloadˉchannelˉresource(output, Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET);
        Emitˉvalidateˉownedˉresource(
            output,
            Kernelˉprocessˉcontract.MODULE_RESOURCE_ID,
            Kernelˉprocessˉcontract.MODULE_RESOURCE_ATTRIBUTES,
            programˉlength);
        Emitˉloadˉchannelˉresource(output, Kernelˉprocessˉcontract.SECOND_RESOURCE_RECORD_OFFSET);
        Emitˉvalidateˉownedˉresource(
            output,
            Kernelˉprocessˉcontract.BUDGET_RESOURCE_ID,
            Kernelˉprocessˉcontract.BUDGET_RESOURCE_ATTRIBUTES,
            Kernelˉprocessˉcontract.EXECUTION_BUDGET_BYTES);

        // Both records and both empty target PTEs are accepted before the
        // single resource-set operation mutates either borrower mapping.
        foreach (var Recordˉoffset in new[]
        {
            Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET,
            Kernelˉprocessˉcontract.SECOND_RESOURCE_RECORD_OFFSET,
        })
        {
            Emitˉloadˉchannelˉresource(output, Recordˉoffset);
            output.Emit(0x4D, 0x8B, 0x41,
                (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_PTE_OFFSET);
            output.Emit(0x49, 0x8B, 0x41,
                (byte)Kernelˉprocessˉcontract.RESOURCE_SOURCE_ADDRESS_OFFSET);
            output.Emit(0x48, 0x83, 0xC8,
                (byte)(Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER));
            output.Emit(0x48, 0x0F, 0xBA, 0xE8, 0x3F, 0x49, 0x89, 0x00);
        }

        Emitˉloadˉchannelˉresource(output, Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET);
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
        Emitˉstoreˉclientˉdataˉu32(
            output, Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET + 8,
            Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_BYTES);
        Emitˉstoreˉclientˉdataˉu32(
            output,
            Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET +
                Kernelˉprocessˉcontract.BOOT_RESOURCE_COUNT_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_COUNT);
        Emitˉpublishˉresourceˉentry(
            output,
            Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET,
            Kernelˉprocessˉcontract.BOOT_RESOURCE_FIRST_ENTRY_OFFSET,
            Kernelˉprocessˉcontract.MODULE_RESOURCE_ID,
            Kernelˉprocessˉcontract.RESOURCE_KIND_WVB_MODULE);
        Emitˉpublishˉresourceˉentry(
            output,
            Kernelˉprocessˉcontract.SECOND_RESOURCE_RECORD_OFFSET,
            Kernelˉprocessˉcontract.BOOT_RESOURCE_SECOND_ENTRY_OFFSET,
            Kernelˉprocessˉcontract.BUDGET_RESOURCE_ID,
            Kernelˉprocessˉcontract.RESOURCE_KIND_U32_EXECUTION_BUDGET);

        foreach (var Recordˉoffset in new[]
        {
            Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET,
            Kernelˉprocessˉcontract.SECOND_RESOURCE_RECORD_OFFSET,
        })
        {
            Emitˉloadˉchannelˉresource(output, Recordˉoffset);
            Emitˉstoreˉresourceˉu32(
                output, Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET,
                Kernelˉprocessˉcontract.RESOURCE_STATE_BORROWED);
            Emitˉstoreˉresourceˉu32(
                output, Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET, 1);
            Emitˉstoreˉresourceˉborrowerˉgeneration(output);
            output.Emit(0x41, 0xFF, 0x41,
                checked((byte)Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET));
        }
    }

    private static void Emitˉvalidateˉownedˉresource(
        X64ˉcodeˉbuilder output,
        uint resourceˉid,
        uint attributes,
        uint resourceˉlength)
    {
        Emitˉvalidateˉresourceˉheader(output, SYSCALL_FAILURE_LABEL);
        foreach (var Field in new (uint Offset, uint Value)[]
        {
            (Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET,
                Kernelˉprocessˉcontract.RESOURCE_STATE_OWNED),
            (Kernelˉprocessˉcontract.RESOURCE_ID_OFFSET, resourceˉid),
            (Kernelˉprocessˉcontract.RESOURCE_OWNER_OFFSET,
                Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE),
            (Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET, 0),
            (Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET, resourceˉlength),
            (Kernelˉprocessˉcontract.RESOURCE_FLAGS_OFFSET, attributes),
            (Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET, 0),
        })
        {
            Emitˉcompareˉresourceˉu32(output, Field.Offset, Field.Value);
            output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
        }
        Emitˉvalidateˉownedˉgrantˉgeneration(output, SYSCALL_FAILURE_LABEL);
        output.Emit(0x49, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_SOURCE_ADDRESS_OFFSET);
        output.Emit(0x48, 0x85, 0xC0);
        output.Jumpˉif(CONDITION_EQUAL, SYSCALL_FAILURE_LABEL);
        output.Emit(0x48, 0xA9, 0xFF, 0x0F, 0x00, 0x00);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
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
    }

    private static void Emitˉpublishˉresourceˉentry(
        X64ˉcodeˉbuilder output,
        uint recordˉoffset,
        uint entryˉoffset,
        uint resourceˉid,
        uint kind)
    {
        Emitˉloadˉchannelˉresource(output, recordˉoffset);
        var Entryˉbase = Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET + entryˉoffset;
        Emitˉstoreˉclientˉdataˉu32(
            output, Entryˉbase + Kernelˉprocessˉcontract.BOOT_RESOURCE_ENTRY_ID_OFFSET, resourceˉid);
        Emitˉstoreˉclientˉdataˉu32(
            output, Entryˉbase + Kernelˉprocessˉcontract.BOOT_RESOURCE_ENTRY_KIND_OFFSET, kind);
        output.Emit(0x49, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_ADDRESS_OFFSET);
        output.Emit(0x49, 0x89, 0x80);
        output.Emitˉu32(Entryˉbase + Kernelˉprocessˉcontract.BOOT_RESOURCE_DATA_POINTER_OFFSET);
        output.Emit(0x41, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET);
        output.Emit(0x41, 0x89, 0x80);
        output.Emitˉu32(Entryˉbase + Kernelˉprocessˉcontract.BOOT_RESOURCE_DATA_LENGTH_OFFSET);
        Emitˉstoreˉclientˉdataˉu32(
            output, Entryˉbase + Kernelˉprocessˉcontract.BOOT_RESOURCE_ENTRY_FLAGS_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_BASE_FLAGS);
    }

    private static void Emitˉstoreˉclientˉdataˉu32(
        X64ˉcodeˉbuilder output,
        uint offset,
        uint value)
    {
        output.Emit(0x41, 0xC7, 0x80);
        output.Emitˉu32(offset);
        output.Emitˉu32(value);
    }

    private static void Emitˉvalidateˉborrowedˉresource(
        X64ˉcodeˉbuilder output,
        string failureˉlabel,
        uint resourceˉlength,
        bool allowˉaccessedˉleaf = false)
    {
        Emitˉvalidateˉborrowedˉresourceˉentry(
            output,
            failureˉlabel,
            Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET,
            Kernelˉprocessˉcontract.MODULE_RESOURCE_ID,
            Kernelˉprocessˉcontract.MODULE_RESOURCE_ATTRIBUTES,
            resourceˉlength,
            allowˉaccessedˉleaf);
        Emitˉvalidateˉborrowedˉresourceˉentry(
            output,
            failureˉlabel,
            Kernelˉprocessˉcontract.SECOND_RESOURCE_RECORD_OFFSET,
            Kernelˉprocessˉcontract.BUDGET_RESOURCE_ID,
            Kernelˉprocessˉcontract.BUDGET_RESOURCE_ATTRIBUTES,
            Kernelˉprocessˉcontract.EXECUTION_BUDGET_BYTES,
            allowˉaccessedˉleaf);

        Emitˉloadˉstateˉresource(output, Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET);
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
        Emitˉcompareˉclientˉdataˉu32(
            output, failureˉlabel,
            Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET + 8,
            Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_BYTES);
        Emitˉcompareˉclientˉdataˉu32(
            output, failureˉlabel,
            Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET +
                Kernelˉprocessˉcontract.BOOT_RESOURCE_COUNT_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_COUNT);
        Emitˉvalidateˉresourceˉentry(
            output, failureˉlabel,
            Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET,
            Kernelˉprocessˉcontract.BOOT_RESOURCE_FIRST_ENTRY_OFFSET,
            Kernelˉprocessˉcontract.MODULE_RESOURCE_ID,
            Kernelˉprocessˉcontract.RESOURCE_KIND_WVB_MODULE);
        Emitˉvalidateˉresourceˉentry(
            output, failureˉlabel,
            Kernelˉprocessˉcontract.SECOND_RESOURCE_RECORD_OFFSET,
            Kernelˉprocessˉcontract.BOOT_RESOURCE_SECOND_ENTRY_OFFSET,
            Kernelˉprocessˉcontract.BUDGET_RESOURCE_ID,
            Kernelˉprocessˉcontract.RESOURCE_KIND_U32_EXECUTION_BUDGET);
    }

    private static void Emitˉvalidateˉborrowedˉresourceˉentry(
        X64ˉcodeˉbuilder output,
        string failureˉlabel,
        uint recordˉoffset,
        uint resourceˉid,
        uint attributes,
        uint resourceˉlength,
        bool allowˉaccessedˉleaf)
    {
        Emitˉloadˉstateˉresource(output, recordˉoffset);
        Emitˉvalidateˉresourceˉheader(output, failureˉlabel);
        foreach (var Field in new (uint Offset, uint Value)[]
        {
            (Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET,
                Kernelˉprocessˉcontract.RESOURCE_STATE_BORROWED),
            (Kernelˉprocessˉcontract.RESOURCE_ID_OFFSET, resourceˉid),
            (Kernelˉprocessˉcontract.RESOURCE_OWNER_OFFSET,
                Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE),
            (Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET, resourceˉlength),
            (Kernelˉprocessˉcontract.RESOURCE_FLAGS_OFFSET, attributes),
            (Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET, 1),
        })
        {
            Emitˉcompareˉresourceˉu32(output, Field.Offset, Field.Value);
            output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        }
        Emitˉvalidateˉborrowedˉgeneration(output, failureˉlabel);
        output.Emit(0x49, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_SOURCE_ADDRESS_OFFSET);
        output.Emit(0x4D, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_PTE_OFFSET);
        output.Emit(0x48, 0x83, 0xC8,
            (byte)(Kernelˉpagingˉcontract.ENTRY_PRESENT | Kernelˉprocessˉcontract.ENTRY_USER));
        output.Emit(0x48, 0x0F, 0xBA, 0xE8, 0x3F);
        if (allowˉaccessedˉleaf)
        {
            output.Emit(0x49, 0x8B, 0x10, 0x48, 0x83, 0xE2, 0xDF, 0x48, 0x39, 0xC2);
        }
        else
        {
            output.Emit(0x49, 0x39, 0x00);
        }
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
    }

    private static void Emitˉvalidateˉresourceˉentry(
        X64ˉcodeˉbuilder output,
        string failureˉlabel,
        uint recordˉoffset,
        uint entryˉoffset,
        uint resourceˉid,
        uint kind)
    {
        Emitˉloadˉstateˉresource(output, recordˉoffset);
        output.Emit(0x4D, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_DATA_OFFSET);
        var Entryˉbase = Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET + entryˉoffset;
        Emitˉcompareˉclientˉdataˉu32(
            output, failureˉlabel,
            Entryˉbase + Kernelˉprocessˉcontract.BOOT_RESOURCE_ENTRY_ID_OFFSET, resourceˉid);
        Emitˉcompareˉclientˉdataˉu32(
            output, failureˉlabel,
            Entryˉbase + Kernelˉprocessˉcontract.BOOT_RESOURCE_ENTRY_KIND_OFFSET, kind);
        output.Emit(0x49, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_ADDRESS_OFFSET);
        output.Emit(0x49, 0x39, 0x80);
        output.Emitˉu32(Entryˉbase + Kernelˉprocessˉcontract.BOOT_RESOURCE_DATA_POINTER_OFFSET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        output.Emit(0x41, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET);
        output.Emit(0x41, 0x39, 0x80);
        output.Emitˉu32(Entryˉbase + Kernelˉprocessˉcontract.BOOT_RESOURCE_DATA_LENGTH_OFFSET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        Emitˉcompareˉclientˉdataˉu32(
            output, failureˉlabel,
            Entryˉbase + Kernelˉprocessˉcontract.BOOT_RESOURCE_ENTRY_FLAGS_OFFSET,
            Kernelˉprocessˉcontract.RESOURCE_BASE_FLAGS);
        output.Emit(0x49, 0x83, 0xB8);
        output.Emitˉu32(Entryˉbase + Kernelˉprocessˉcontract.BOOT_RESOURCE_RESERVED_OFFSET);
        output.Emit(0x00);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        output.Emit(0x49, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_SERVICE_ADDRESS_OFFSET);
        output.Emit(0x49, 0x39, 0x80);
        output.Emitˉu32(Kernelˉprocessˉcontract.RUNTIME_SERVICE_TABLE_OFFSET +
            Nativeˉserviceˉtableˉcontract.FILE_READ_BYTES_POINTER_OFFSET);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
    }

    private static void Emitˉcompareˉclientˉdataˉu32(
        X64ˉcodeˉbuilder output,
        string failureˉlabel,
        uint offset,
        uint value)
    {
        output.Emit(0x41, 0x81, 0xB8);
        output.Emitˉu32(offset);
        output.Emitˉu32(value);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
    }

    private static void Emitˉrevokeˉterminalˉresource(
        X64ˉcodeˉbuilder output,
        string failureˉlabel,
        uint resourceˉlength)
    {
        Emitˉvalidateˉborrowedˉresource(
            output, failureˉlabel, resourceˉlength, allowˉaccessedˉleaf: true);

        foreach (var Recordˉoffset in new[]
        {
            Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET,
            Kernelˉprocessˉcontract.SECOND_RESOURCE_RECORD_OFFSET,
        })
        {
            Emitˉloadˉstateˉresource(output, Recordˉoffset);
            output.Emit(0x4D, 0x8B, 0x51,
                (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_PTE_OFFSET);
            output.Emit(0x49, 0xC7, 0x02);
            output.Emitˉu32(0);
        }

        Emitˉloadˉstateˉresource(output, Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET);
        output.Emit(0x4D, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_DATA_OFFSET);
        output.Emit(0x31, 0xC0);
        foreach (var Offset in Revokedˉclientˉdataˉqwordˉoffsets())
        {
            output.Emit(0x49, 0x89, 0x80);
            output.Emitˉu32(Offset);
        }

        foreach (var Recordˉoffset in new[]
        {
            Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET,
            Kernelˉprocessˉcontract.SECOND_RESOURCE_RECORD_OFFSET,
        })
        {
            Emitˉloadˉstateˉresource(output, Recordˉoffset);
            Emitˉstoreˉresourceˉu32(
                output, Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET,
                Kernelˉprocessˉcontract.RESOURCE_STATE_OWNED);
            Emitˉstoreˉresourceˉu32(
                output, Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET, 0);
            Emitˉstoreˉresourceˉu32(
                output, Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET, 0);
        }
    }

    private static void Emitˉvalidateˉreleasedˉresource(
        X64ˉcodeˉbuilder output,
        string failureˉlabel,
        uint resourceˉlength)
    {
        Emitˉvalidateˉreleasedˉresourceˉentry(
            output, failureˉlabel,
            Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET,
            Kernelˉprocessˉcontract.MODULE_RESOURCE_ID,
            Kernelˉprocessˉcontract.MODULE_RESOURCE_ATTRIBUTES,
            resourceˉlength);
        Emitˉvalidateˉreleasedˉresourceˉentry(
            output, failureˉlabel,
            Kernelˉprocessˉcontract.SECOND_RESOURCE_RECORD_OFFSET,
            Kernelˉprocessˉcontract.BUDGET_RESOURCE_ID,
            Kernelˉprocessˉcontract.BUDGET_RESOURCE_ATTRIBUTES,
            Kernelˉprocessˉcontract.EXECUTION_BUDGET_BYTES);

        Emitˉloadˉstateˉresource(output, Kernelˉprocessˉcontract.RESOURCE_RECORD_OFFSET);
        output.Emit(0x4D, 0x8B, 0x41,
            (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_DATA_OFFSET);
        output.Emit(0x31, 0xC0);
        foreach (var Offset in Revokedˉclientˉdataˉqwordˉoffsets())
        {
            output.Emit(0x49, 0x39, 0x80);
            output.Emitˉu32(Offset);
            output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        }
    }

    private static void Emitˉvalidateˉreleasedˉresourceˉentry(
        X64ˉcodeˉbuilder output,
        string failureˉlabel,
        uint recordˉoffset,
        uint resourceˉid,
        uint attributes,
        uint resourceˉlength)
    {
        Emitˉloadˉstateˉresource(output, recordˉoffset);
        Emitˉvalidateˉresourceˉheader(output, failureˉlabel);
        foreach (var Field in new (uint Offset, uint Value)[]
        {
            (Kernelˉprocessˉcontract.RESOURCE_STATE_OFFSET,
                Kernelˉprocessˉcontract.RESOURCE_STATE_OWNED),
            (Kernelˉprocessˉcontract.RESOURCE_ID_OFFSET, resourceˉid),
            (Kernelˉprocessˉcontract.RESOURCE_OWNER_OFFSET,
                Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE),
            (Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET, 0),
            (Kernelˉprocessˉcontract.RESOURCE_LENGTH_OFFSET, resourceˉlength),
            (Kernelˉprocessˉcontract.RESOURCE_FLAGS_OFFSET, attributes),
            (Kernelˉprocessˉcontract.RESOURCE_MAPPING_COUNT_OFFSET, 0),
        })
        {
            Emitˉcompareˉresourceˉu32(output, Field.Offset, Field.Value);
            output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        }
        Emitˉvalidateˉreleasedˉgeneration(output, failureˉlabel);
        output.Emit(0x4D, 0x8B, 0x51,
            (byte)Kernelˉprocessˉcontract.RESOURCE_TARGET_PTE_OFFSET);
        output.Emit(0x49, 0x83, 0x3A, 0x00);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
    }

    private static void Emitˉloadˉchannelˉresource(X64ˉcodeˉbuilder output, uint recordˉoffset)
    {
        output.Emit(0x4D, 0x8D, 0x8A);
        output.Emitˉu32(recordˉoffset - Kernelˉprocessˉcontract.CHANNEL_RECORD_OFFSET);
    }

    private static void Emitˉloadˉstateˉresource(X64ˉcodeˉbuilder output, uint recordˉoffset)
    {
        output.Emit(0x4D, 0x8D, 0x8C, 0x24);
        output.Emitˉu32(recordˉoffset);
    }

    private static IEnumerable<uint> Revokedˉclientˉdataˉqwordˉoffsets()
    {
        yield return Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET;
        yield return Nativeˉexecutionˉcontextˉcontract.FILE_INPUT_TABLE_POINTER_OFFSET;
        for (uint Offset = Kernelˉprocessˉcontract.RUNTIME_SERVICE_TABLE_OFFSET;
            Offset < Kernelˉprocessˉcontract.RUNTIME_SERVICE_TABLE_OFFSET +
                Nativeˉserviceˉtableˉcontract.SIZE;
            Offset += sizeof(ulong))
        {
            yield return Offset;
        }
        for (uint Offset = Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET;
            Offset < Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_OFFSET +
                Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_BYTES;
            Offset += sizeof(ulong))
        {
            yield return Offset;
        }
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

    private static void Emitˉstoreˉresourceˉborrowerˉgeneration(
        X64ˉcodeˉbuilder output)
    {
        Emitˉloadˉclientˉgenerationˉeax(output, SYSCALL_FAILURE_LABEL);
        output.Emit(0xC1, 0xE0, 0x10, 0x83, 0xC8,
            checked((byte)Kernelˉprocessˉcontract.CLIENT_PROCESS_ID));
        output.Emit(0x41, 0x89, 0x41,
            checked((byte)Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET));
    }

    private static void Emitˉvalidateˉownedˉgrantˉgeneration(
        X64ˉcodeˉbuilder output,
        string failureˉlabel)
    {
        Emitˉloadˉclientˉgenerationˉeax(output, failureˉlabel);
        output.Emit(0xFF, 0xC8, 0x41, 0x39, 0x41,
            checked((byte)Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET));
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
    }

    private static void Emitˉvalidateˉborrowedˉgeneration(
        X64ˉcodeˉbuilder output,
        string failureˉlabel)
    {
        Emitˉloadˉclientˉgenerationˉeax(output, failureˉlabel);
        output.Emit(0x41, 0x39, 0x41,
            checked((byte)Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET));
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
        output.Emit(0xC1, 0xE0, 0x10, 0x83, 0xC8,
            checked((byte)Kernelˉprocessˉcontract.CLIENT_PROCESS_ID));
        output.Emit(0x41, 0x39, 0x41,
            checked((byte)Kernelˉprocessˉcontract.RESOURCE_BORROWER_OFFSET));
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
    }

    private static void Emitˉvalidateˉreleasedˉgeneration(
        X64ˉcodeˉbuilder output,
        string failureˉlabel)
    {
        Emitˉloadˉclientˉgenerationˉeax(output, failureˉlabel);
        output.Emit(0x41, 0x39, 0x41,
            checked((byte)Kernelˉprocessˉcontract.RESOURCE_GRANT_COUNT_OFFSET));
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
    }

    private static void Emitˉloadˉclientˉgenerationˉeax(
        X64ˉcodeˉbuilder output,
        string failureˉlabel)
    {
        output.Emit(0x41, 0x8B, 0x84, 0x24);
        output.Emitˉu32(
            Kernelˉprocessˉcontract.CLIENT_RECORD_OFFSET +
            Kernelˉprocessˉcontract.PROCESS_GENERATION_OFFSET);
        output.Emit(0x83, 0xF8, (byte)Kernelˉprocessˉcontract.FIRST_CLIENT_GENERATION);
        output.Jumpˉif(CONDITION_BELOW, failureˉlabel);
        output.Emit(0x83, 0xF8, (byte)Kernelˉprocessˉcontract.SECOND_CLIENT_GENERATION);
        output.Jumpˉif(CONDITION_ABOVE, failureˉlabel);
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
        var Primaryˉlabel = $"process_channel_primary_{output.Position}";
        var Directoryˉproviderˉlabel = $"process_channel_directory_provider_{output.Position}";
        var Selectedˉlabel = $"process_channel_selected_{output.Position}";
        output.Emit(0x81, 0xFE);
        output.Emitˉu32(Kernelˉprocessˉcontract.RESOURCE_CAPABILITY_REFERENCE);
        output.Jumpˉif(CONDITION_EQUAL, Primaryˉlabel);
        Emitˉcompareˉgsˉu32(output, Kernelˉprocessˉcontract.ROLE_OFFSET,
            Kernelˉprocessˉcontract.ROLE_DIRECTORY_SERVICE);
        output.Jumpˉif(CONDITION_EQUAL, Directoryˉproviderˉlabel);
        output.Emit(0x65, 0x4C, 0x8B, 0x14, 0x25);
        output.Emitˉu32(Kernelˉprocessˉcontract.SECOND_ENDPOINT_ADDRESS_OFFSET);
        output.Jump(Selectedˉlabel);
        output.Mark(Directoryˉproviderˉlabel);
        output.Mark(Primaryˉlabel);
        output.Emit(0x65, 0x4C, 0x8B, 0x14, 0x25);
        output.Emitˉu32(Kernelˉprocessˉcontract.ENDPOINT_ADDRESS_OFFSET);
        output.Mark(Selectedˉlabel);
        output.Emit(0x4D, 0x8B, 0x52,
            checked((byte)Kernelˉprocessˉcontract.ENDPOINT_CHANNEL_ADDRESS_OFFSET));
    }

    private static void Emitˉcompareˉchannelˉproviderˉwaiter(X64ˉcodeˉbuilder output)
    {
        var Directoryˉlabel = $"process_waiter_directory_{output.Position}";
        var Completeˉlabel = $"process_waiter_complete_{output.Position}";
        output.Emit(0x81, 0xFE);
        output.Emitˉu32(Kernelˉprocessˉcontract.DIRECTORY_CAPABILITY_REFERENCE);
        output.Jumpˉif(CONDITION_EQUAL, Directoryˉlabel);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.INIT_PROCESS_ID);
        output.Jump(Completeˉlabel);
        output.Mark(Directoryˉlabel);
        Emitˉcompareˉchannelˉu32(output, Kernelˉprocessˉcontract.CHANNEL_WAITER_OFFSET,
            Kernelˉprocessˉcontract.DIRECTORY_PROCESS_ID);
        output.Mark(Completeˉlabel);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SYSCALL_FAILURE_LABEL);
    }

    private static void Emitˉloadˉrecordˉchannelˉr10(X64ˉcodeˉbuilder output)
    {
        output.Emit(0x4D, 0x8B, 0x95);
        output.Emitˉu32(Kernelˉprocessˉcontract.ENDPOINT_ADDRESS_OFFSET);
        output.Emit(0x4D, 0x8B, 0x52,
            checked((byte)Kernelˉprocessˉcontract.ENDPOINT_CHANNEL_ADDRESS_OFFSET));
    }

    private static void Emitˉloadˉstateˉchannelˉr10(
        X64ˉcodeˉbuilder output,
        uint channelˉrecordˉoffset)
    {
        output.Emit(0x4D, 0x8D, 0x94, 0x24);
        output.Emitˉu32(channelˉrecordˉoffset);
    }

    private static void Emitˉloadˉstateˉendpointˉr11(
        X64ˉcodeˉbuilder output,
        uint endpointˉrecordˉoffset)
    {
        output.Emit(0x4D, 0x8D, 0x9C, 0x24);
        output.Emitˉu32(endpointˉrecordˉoffset);
    }

    private static void Emitˉcompareˉendpointˉu32(
        X64ˉcodeˉbuilder output,
        uint offset,
        uint value)
    {
        output.Emit(0x41, 0x81, 0x7B, checked((byte)offset));
        output.Emitˉu32(value);
    }

    private static void Emitˉstoreˉendpointˉu32(
        X64ˉcodeˉbuilder output,
        uint offset,
        uint value)
    {
        output.Emit(0x41, 0xC7, 0x43, checked((byte)offset));
        output.Emitˉu32(value);
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

    private static void Emitˉreadyˉdispatcher(X64ˉcodeˉbuilder output)
    {
        const string Failureˉlabel = "process_ready_dispatcher_failure";
        const string Startˉzeroˉlabel = "process_ready_dispatcher_start_0";
        const string Startˉoneˉlabel = "process_ready_dispatcher_start_1";
        const string Startˉtwoˉlabel = "process_ready_dispatcher_start_2";

        output.Mark(DISPATCHER_LABEL);
        foreach (var Record in new (uint Slot, uint Processˉid)[]
        {
            (INIT_RECORD_SLOT_OFFSET, Kernelˉprocessˉcontract.INIT_PROCESS_ID),
            (CLIENT_RECORD_SLOT_OFFSET, Kernelˉprocessˉcontract.CLIENT_PROCESS_ID),
            (DIRECTORY_RECORD_SLOT_OFFSET, Kernelˉprocessˉcontract.DIRECTORY_PROCESS_ID),
        })
        {
            Emitˉloadˉstackˉr13(output, checked(Record.Slot + sizeof(ulong)));
            output.Emit(0x4D, 0x85, 0xED);
            output.Jumpˉif(CONDITION_EQUAL, Failureˉlabel);
            output.Emit(0x48, 0xB8);
            output.Emitˉu64(Kernelˉprocessˉcontract.RECORD_MAGIC);
            output.Emit(0x49, 0x39, 0x45, 0x00);
            output.Jumpˉif(CONDITION_NOT_EQUAL, Failureˉlabel);
            Emitˉcompareˉrecordˉu32(output, 8, Kernelˉprocessˉcontract.RECORD_VERSION);
            output.Jumpˉif(CONDITION_NOT_EQUAL, Failureˉlabel);
            Emitˉcompareˉrecordˉu32(output, 12, Kernelˉprocessˉcontract.RECORD_BYTES);
            output.Jumpˉif(CONDITION_NOT_EQUAL, Failureˉlabel);
            Emitˉcompareˉrecordˉu32(output, 24, Record.Processˉid);
            output.Jumpˉif(CONDITION_NOT_EQUAL, Failureˉlabel);
            Emitˉcompareˉrecordˉu32(
                output, Kernelˉprocessˉcontract.PROCESS_GENERATION_OFFSET, 0);
            output.Jumpˉif(CONDITION_EQUAL, Failureˉlabel);
            Emitˉcompareˉrecordˉu32(
                output, Kernelˉprocessˉcontract.PROCESS_GENERATION_OFFSET, ushort.MaxValue);
            output.Jumpˉif(CONDITION_ABOVE, Failureˉlabel);
        }

        Emitˉloadˉstackˉrax(output, DISPATCH_CURSOR_SLOT_OFFSET + sizeof(ulong));
        output.Emit(0x83, 0xF8, 0x00);
        output.Jumpˉif(CONDITION_EQUAL, Startˉzeroˉlabel);
        output.Emit(0x83, 0xF8, 0x01);
        output.Jumpˉif(CONDITION_EQUAL, Startˉoneˉlabel);
        output.Emit(0x83, 0xF8, 0x02);
        output.Jumpˉif(CONDITION_EQUAL, Startˉtwoˉlabel);
        output.Jump(Failureˉlabel);

        output.Mark(Startˉzeroˉlabel);
        Emitˉreadyˉscan(output, "0", [0, 1, 2], Failureˉlabel);
        output.Mark(Startˉoneˉlabel);
        Emitˉreadyˉscan(output, "1", [1, 2, 0], Failureˉlabel);
        output.Mark(Startˉtwoˉlabel);
        Emitˉreadyˉscan(output, "2", [2, 0, 1], Failureˉlabel);

        output.Mark(Failureˉlabel);
        output.Emit(0x45, 0x31, 0xED, 0xC3);
    }

    private static void Emitˉreadyˉscan(
        X64ˉcodeˉbuilder output,
        string prefix,
        int[] order,
        string failureˉlabel)
    {
        var Slots = new[]
        {
            INIT_RECORD_SLOT_OFFSET,
            CLIENT_RECORD_SLOT_OFFSET,
            DIRECTORY_RECORD_SLOT_OFFSET,
        };
        for (var Distance = 0; Distance < order.Length; Distance++)
        {
            var Index = order[Distance];
            var Eligibleˉlabel = $"process_ready_dispatcher_{prefix}_{Distance}_eligible";
            var Nextˉlabel = $"process_ready_dispatcher_{prefix}_{Distance}_next";
            Emitˉloadˉstackˉr13(output, checked(Slots[Index] + sizeof(ulong)));
            Emitˉcompareˉrecordˉu32(
                output,
                Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
                Kernelˉprocessˉcontract.PROCESS_STATE_READY);
            output.Jumpˉif(CONDITION_EQUAL, Eligibleˉlabel);
            Emitˉcompareˉrecordˉu32(
                output,
                Kernelˉprocessˉcontract.PROCESS_STATE_OFFSET,
                Kernelˉprocessˉcontract.PROCESS_STATE_RUNNING);
            output.Jumpˉif(CONDITION_NOT_EQUAL, Nextˉlabel);
            output.Mark(Eligibleˉlabel);
            Emitˉcompareˉrecordˉu32(
                output,
                Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
                Kernelˉprocessˉcontract.THREAD_STATE_READY);
            output.Jumpˉif(CONDITION_NOT_EQUAL, Nextˉlabel);
            output.Emit(0xB8);
            output.Emitˉu32(checked((uint)((Index + 1) % Kernelˉreadyˉdispatcher.RECORD_COUNT)));
            Emitˉstoreˉstackˉrax(
                output, DISPATCH_CURSOR_SLOT_OFFSET + sizeof(ulong));
            output.Emit(0xC3);
            output.Mark(Nextˉlabel);
        }
        output.Jump(failureˉlabel);
    }

    private static void Emitˉdispatchˉreadyˉrecord(
        X64ˉcodeˉbuilder output,
        uint expectedˉprocessˉreference)
    {
        output.Call(DISPATCHER_LABEL);
        output.Emit(0x4D, 0x85, 0xED);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉrecordˉu32(output, 24, expectedˉprocessˉreference & 0xFFFF);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉrecordˉu32(
            output,
            Kernelˉprocessˉcontract.PROCESS_GENERATION_OFFSET,
            expectedˉprocessˉreference >> 16);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
    }

    private static void Emitˉmarkˉrecordˉready(
        X64ˉcodeˉbuilder output,
        uint recordˉslotˉoffset)
    {
        Emitˉloadˉstackˉr13(output, recordˉslotˉoffset);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.THREAD_STATE_OFFSET,
            Kernelˉprocessˉcontract.THREAD_STATE_READY);
        Emitˉstoreˉrecordˉu32(output, Kernelˉprocessˉcontract.WAIT_REASON_OFFSET,
            Kernelˉprocessˉcontract.WAIT_REASON_NONE);
    }

    private static void Emitˉwakeˉandˉdispatchˉreadyˉrecord(
        X64ˉcodeˉbuilder output,
        uint recordˉslotˉoffset,
        uint expectedˉprocessˉreference)
    {
        Emitˉmarkˉrecordˉready(output, recordˉslotˉoffset);
        Emitˉdispatchˉreadyˉrecord(output, expectedˉprocessˉreference);
    }

    private static void Emitˉloadˉstackˉrax(X64ˉcodeˉbuilder output, uint offset)
    {
        output.Emit(0x48, 0x8B, 0x84, 0x24);
        output.Emitˉu32(offset);
    }

    private static void Emitˉloadˉstackˉr13(X64ˉcodeˉbuilder output, uint offset)
    {
        output.Emit(0x4C, 0x8B, 0xAC, 0x24);
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
        ImmutableArray<byte> directoryˉserviceˉimage,
        ImmutableArray<byte> clientˉimage,
        ImmutableArray<byte> runtimeˉinput,
        ImmutableArray<byte> runtimeˉbudget,
        ImmutableArray<byte> resourceˉstore,
        ImmutableArray<byte> directoryˉsnapshot,
        ImmutableArray<Objectˉrelocation> relocations)
    {
        var Object = Objectˉcodec.Readˉandˉverify(objectˉbytes.AsSpan()).Value;
        if (Object.Sections.Length != 8 ||
            Object.Sections[0].Kind != Objectˉsectionˉkind.Code ||
            Object.Sections[1].Kind != Objectˉsectionˉkind.Readˉonlyˉdata ||
            Object.Sections[2].Kind != Objectˉsectionˉkind.Readˉonlyˉdata ||
            Object.Sections[3].Kind != Objectˉsectionˉkind.Readˉonlyˉdata ||
            Object.Sections[4].Kind != Objectˉsectionˉkind.Readˉonlyˉdata ||
            Object.Sections[5].Kind != Objectˉsectionˉkind.Readˉonlyˉdata ||
            Object.Sections[6].Kind != Objectˉsectionˉkind.Readˉonlyˉdata ||
            Object.Sections[7].Kind != Objectˉsectionˉkind.Readˉonlyˉdata ||
            !Object.Sections[0].Data.AsSpan().SequenceEqual(code.AsSpan()) ||
            !Object.Sections[1].Data.AsSpan().SequenceEqual(serviceˉimage.AsSpan()) ||
            !Object.Sections[2].Data.AsSpan().SequenceEqual(clientˉimage.AsSpan()) ||
            !Object.Sections[3].Data.AsSpan().SequenceEqual(runtimeˉinput.AsSpan()) ||
            !Object.Sections[4].Data.AsSpan().SequenceEqual(runtimeˉbudget.AsSpan()) ||
            !Object.Sections[5].Data.AsSpan().SequenceEqual(resourceˉstore.AsSpan()) ||
            !Object.Sections[6].Data.AsSpan().SequenceEqual(directoryˉsnapshot.AsSpan()) ||
            !Object.Sections[7].Data.AsSpan().SequenceEqual(directoryˉserviceˉimage.AsSpan()) ||
            Object.Symbols.Length != 18 ||
            Object.Symbols[2].Name != "Windvale_resource_init_boot" ||
            Object.Symbols[3].Name != "Windvale_resource_init_budget" ||
            Object.Symbols[3].Sectionˉindex != 4 ||
            Object.Symbols[4].Name != "Windvale_resource_init_directory" ||
            Object.Symbols[4].Sectionˉindex != 6 ||
            Object.Symbols[5].Name != "Windvale_resource_init_store" ||
            Object.Symbols[5].Sectionˉindex != 5 ||
            Object.Symbols[6].Name != "Windvale_service_directory_user_image" ||
            Object.Symbols[6].Sectionˉindex != 7 ||
            Object.Symbols[7].Name != Kernelˉprocessˉcontract.ENTER_SYMBOL ||
            Object.Symbols[8].Name != Kernelˉprocessˉcontract.EXCEPTION_ENTRY_SYMBOL ||
            Object.Symbols[9].Name != Kernelˉprocessˉcontract.SYSCALL_ENTRY_SYMBOL ||
            Object.Symbols[10].Name != Kernelˉmemoryˉcontract.ALLOCATE_PAGES_SYMBOL ||
            Object.Symbols[11].Name != Kernelˉprocessˉcontract.POLICY_SYMBOL ||
            Object.Symbols[12].Name != Kernelˉmemoryˉcontract.RELEASE_TAIL_PAGES_SYMBOL ||
            Object.Symbols[13].Name != Kernelˉexceptionˉcontract.TERMINAL_SYMBOL ||
            Object.Symbols[14].Name != Kernelˉpagingˉcontract.PAGE_TABLE_ACTIVATE_SYMBOL ||
            Object.Symbols[15].Name != Kernelˉprocessˉcontract.EXCEPTION_13_ENTRY_SYMBOL ||
            Object.Symbols[16].Name != Kernelˉprocessˉcontract.EXCEPTION_14_ENTRY_SYMBOL ||
            Object.Symbols[17].Name != Kernelˉprocessˉcontract.EXCEPTION_6_ENTRY_SYMBOL ||
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
