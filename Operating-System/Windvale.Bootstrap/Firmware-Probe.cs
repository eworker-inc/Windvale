using System.Collections.Immutable;
using System.Text;
using Windvale.Compiler;
using Windvale.Linker;
using Windvale.ObjectModel;

namespace Windvale.Bootstrap;

public enum Firmwareˉprobeˉscenario
{
    Normal,
    Invalidˉopcode,
    Generalˉprotection,
    Userˉfault,
    Serviceˉfault,
}

public static class Firmwareˉprobe
{
    public const int FORMAT_VERSION = 37;
    public const string ENTRY_SYMBOL = "Windvale_boot_probe";
    public const string KERNEL_ENTRY_SYMBOL = X64ˉkernelˉcontract.KERNEL_ENTRY_SYMBOL;
    public const string WRITE_BYTE_SYMBOL = X64ˉkernelˉcontract.WRITE_BYTE_SYMBOL;
    public const string X64_WRITE_BYTE_SYMBOL = Kernelˉassemblyˉcontract.X64_WRITE_BYTE_SYMBOL;
    public const string ENTRY_MARKER = "windvale-os-boot 37\nentry=pass\n";
    public const string SYSTEM_TABLE_MARKER = "system-table=pass\n";
    public const string MEMORY_MAP_MARKER = "memory-map=pass\n";
    public const string BOOT_SERVICES_MARKER = "boot-services=exited\n";
    public const string MEMORY_OWNED_MARKER = "memory-owned=pass\n";
    public const string ALLOCATOR_MARKER = "allocator=pass\n";
    public const string KERNEL_STACK_MARKER = "kernel-stack=pass\n";
    public const string PAGING_OWNED_MARKER = "paging=owned\n";
    public const string WVB_ADMISSION_MARKER = "wvb-admission=pass\n";
    public const string PROCESS_MARKER = "processes=isolated\n";
    public const string ENDPOINT_MARKER = "service-endpoint=bound\n";
    public const string WVB_RUNTIME_MARKER = "wvb-runtime=interpreted\n";
    public const string INIT_SERVICE_MARKER = "init-service=pass\n";
    public const string DIRECTORY_SERVICE_MARKER = "directory-service=pass\n";
    public const string RESOURCE_GRANT_MARKER = "resource-grant=pass\n";
    public const string TYPED_RESOURCES_MARKER = "typed-resources=pass\n";
    public const string RESOURCE_REVOKED_MARKER = "resource-revoked=pass\n";
    public const string PROCESS_REUSE_MARKER = "process-reuse=pass\n";
    public const string IPC_MARKER = "ipc=resource-and-directory\n";
    public const string HELLO_WORLD_MARKER = "Hello from Windvale\n";
    public const string CPU_EXCEPTIONS_MARKER = "cpu-exceptions=armed\n";
    public const string INVALID_OPCODE_PANIC_MARKER = Kernelˉexceptionˉcontract.INVALID_OPCODE_PANIC_MARKER;
    public const string GENERAL_PROTECTION_PANIC_MARKER =
        Kernelˉexceptionˉcontract.GENERAL_PROTECTION_PANIC_MARKER;
    public const string NATIVE_CONTEXT_MARKER = "native-context=pass\n";
    public const string NATIVE_WVB_MARKER = "native-wvb=pass\n";
    public const string WINDVALE_SOURCE_MARKER = "windvale-source=pass\n";
    public const string USER_FAULT_CONTAINED_MARKER = Kernelˉprocessˉcontract.USER_FAULT_CONTAINED_MARKER;
    public const string SERVICE_FAULT_CONTAINED_MARKER =
        Kernelˉprocessˉcontract.SERVICE_FAULT_CONTAINED_MARKER;
    public const string SERVICE_PEER_LOSS_MARKER = "ipc=service-peer-loss\n";
    public const string SUCCESS_MARKER = "status=pass\n";
    public const string SHUTDOWN_MARKER = "shutdown=poweroff\n";
    public const string SERIAL_MARKER =
        ENTRY_MARKER + SYSTEM_TABLE_MARKER + MEMORY_MAP_MARKER + BOOT_SERVICES_MARKER +
        MEMORY_OWNED_MARKER + ALLOCATOR_MARKER + KERNEL_STACK_MARKER + PAGING_OWNED_MARKER +
        WVB_ADMISSION_MARKER + PROCESS_MARKER + ENDPOINT_MARKER +
        RESOURCE_GRANT_MARKER + TYPED_RESOURCES_MARKER +
        RESOURCE_REVOKED_MARKER + PROCESS_REUSE_MARKER +
        WVB_RUNTIME_MARKER + INIT_SERVICE_MARKER + DIRECTORY_SERVICE_MARKER +
        IPC_MARKER + HELLO_WORLD_MARKER +
        CPU_EXCEPTIONS_MARKER + NATIVE_CONTEXT_MARKER + NATIVE_WVB_MARKER +
        WINDVALE_SOURCE_MARKER + SUCCESS_MARKER + SHUTDOWN_MARKER;
    public const string USER_FAULT_SERIAL_MARKER =
        ENTRY_MARKER + SYSTEM_TABLE_MARKER + MEMORY_MAP_MARKER + BOOT_SERVICES_MARKER +
        MEMORY_OWNED_MARKER + ALLOCATOR_MARKER + KERNEL_STACK_MARKER + PAGING_OWNED_MARKER +
        WVB_ADMISSION_MARKER + PROCESS_MARKER + ENDPOINT_MARKER +
        RESOURCE_GRANT_MARKER + TYPED_RESOURCES_MARKER +
        RESOURCE_REVOKED_MARKER + PROCESS_REUSE_MARKER +
        WVB_RUNTIME_MARKER + INIT_SERVICE_MARKER + DIRECTORY_SERVICE_MARKER +
        IPC_MARKER + HELLO_WORLD_MARKER +
        CPU_EXCEPTIONS_MARKER + NATIVE_CONTEXT_MARKER + NATIVE_WVB_MARKER +
        WINDVALE_SOURCE_MARKER + USER_FAULT_CONTAINED_MARKER + SUCCESS_MARKER + SHUTDOWN_MARKER;
    public const string SERVICE_FAULT_SERIAL_MARKER =
        ENTRY_MARKER + SYSTEM_TABLE_MARKER + MEMORY_MAP_MARKER + BOOT_SERVICES_MARKER +
        MEMORY_OWNED_MARKER + ALLOCATOR_MARKER + KERNEL_STACK_MARKER + PAGING_OWNED_MARKER +
        WVB_ADMISSION_MARKER + PROCESS_MARKER + ENDPOINT_MARKER +
        RESOURCE_GRANT_MARKER + TYPED_RESOURCES_MARKER +
        RESOURCE_REVOKED_MARKER + WVB_RUNTIME_MARKER + SERVICE_FAULT_CONTAINED_MARKER +
        SERVICE_PEER_LOSS_MARKER + HELLO_WORLD_MARKER +
        CPU_EXCEPTIONS_MARKER + NATIVE_CONTEXT_MARKER + NATIVE_WVB_MARKER +
        WINDVALE_SOURCE_MARKER + SUCCESS_MARKER + SHUTDOWN_MARKER;

    private const string FAILURE_MARKER = "status=fail\n";
    private const string FAILURE_LABEL = "failure";
    private const string CLEANUP_FAILURE_LABEL = "cleanup_failure";
    private const string TERMINAL_FAILURE_LABEL = "terminal_failure";
    private const string TERMINAL_HALT_LABEL = "terminal_halt";
    private const string MAP_VALIDATE_LABEL = "map_validate";
    private const string DESCRIPTOR_LOOP_LABEL = "descriptor_loop";
    private const string EXIT_SUCCESS_LABEL = "exit_success";
    private const string WRITE_BYTE_WAIT_LABEL = "write_byte_wait";
    private const string HELLO_WORLD_RESOURCE = "Windvale.Os.Kernel.Hello-World.wv";
    private const string SERVICE_FAULT_HELLO_RESOURCE = "Windvale.Os.Kernel.Hello-Service-Fault.wv";

    private const byte CONDITION_BELOW = 0x82;
    private const byte CONDITION_EQUAL = 0x84;
    private const byte CONDITION_NOT_EQUAL = 0x85;
    private const byte CONDITION_ABOVE = 0x87;

    private const uint FRAME_BYTES = 0x88;
    private const byte FIFTH_ARGUMENT_OFFSET = 0x20;
    private const byte MAP_SIZE_OFFSET = 0x28;
    private const byte MAP_KEY_OFFSET = 0x30;
    private const byte DESCRIPTOR_SIZE_OFFSET = 0x38;
    private const byte DESCRIPTOR_VERSION_OFFSET = 0x40;
    private const byte MAP_BUFFER_OFFSET = 0x48;
    private const byte SYSTEM_TABLE_OFFSET = 0x50;
    private const byte BOOT_SERVICES_OFFSET = 0x58;
    private const byte MAP_ALLOCATION_SIZE_OFFSET = 0x60;
    private const byte IMAGE_HANDLE_OFFSET = 0x68;
    private const byte EXIT_ATTEMPTS_OFFSET = 0x70;
    private const byte EXIT_ATTEMPTED_OFFSET = 0x74;

    private const byte HANDOFF_OFFSET = 0x20;
    private const byte HANDOFF_VERSION_OFFSET = 0x28;
    private const byte HANDOFF_SIZE_OFFSET = 0x2C;
    private const byte HANDOFF_MAP_BUFFER_OFFSET = 0x30;
    private const byte HANDOFF_MAP_BYTES_OFFSET = 0x38;
    private const byte HANDOFF_DESCRIPTOR_BYTES_OFFSET = 0x40;
    private const byte HANDOFF_DESCRIPTOR_VERSION_OFFSET = 0x48;
    private const byte HANDOFF_RESERVED_OFFSET = 0x4C;

    private const ulong EFI_SYSTEM_TABLE_SIGNATURE = 0x5453_5953_2049_4249;
    private const ulong EFI_BOOT_SERVICES_SIGNATURE = 0x5652_4553_544F_4F42;
    private const ulong EFI_BUFFER_TOO_SMALL = 0x8000_0000_0000_0005;
    private const ulong EFI_DEVICE_ERROR = 0x8000_0000_0000_0007;
    private const ulong EFI_INVALID_PARAMETER = 0x8000_0000_0000_0002;
    private const uint EFI_1_02_REVISION = 0x0001_0002;
    private const uint EFI_SYSTEM_TABLE_MINIMUM_BYTES = 120;
    private const uint EFI_BOOT_SERVICES_MINIMUM_BYTES = 240;
    private const uint EFI_MEMORY_DESCRIPTOR_VERSION = 1;
    private const uint EFI_MEMORY_DESCRIPTOR_MINIMUM_BYTES = 40;
    private const uint EFI_MEMORY_DESCRIPTOR_MAXIMUM_BYTES = 256;
    private const uint MAX_MEMORY_MAP_BYTES = 1024 * 1024;
    private const uint EFI_LOADER_DATA = 2;
    private const uint EXIT_BOOT_SERVICES_MAX_ATTEMPTS = 3;
    private const uint GET_MEMORY_MAP_OFFSET = 0x38;
    private const uint ALLOCATE_POOL_OFFSET = 0x40;
    private const uint FREE_POOL_OFFSET = 0x48;
    private const uint EXIT_BOOT_SERVICES_OFFSET = 0xE8;

    public static ImmutableArray<byte> Buildˉapplication(
        Firmwareˉprobeˉscenario scenario = Firmwareˉprobeˉscenario.Normal)
    {
        if (scenario is not Firmwareˉprobeˉscenario.Normal and
            not Firmwareˉprobeˉscenario.Invalidˉopcode and
            not Firmwareˉprobeˉscenario.Generalˉprotection and
            not Firmwareˉprobeˉscenario.Userˉfault and
            not Firmwareˉprobeˉscenario.Serviceˉfault)
        {
            throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        var Kernelˉsourceˉname = scenario == Firmwareˉprobeˉscenario.Serviceˉfault
            ? "Hello-Service-Fault.wv"
            : "Hello-World.wv";
        var Kernel = X64ˉkernelˉcompiler.Compile(
            Loadˉhelloˉworldˉsource(scenario), Kernelˉsourceˉname);
        if (!Kernel.Success)
        {
            throw new InvalidOperationException(
                $"The Windvale kernel source did not compile: {Kernel.Diagnostics[0]}");
        }
        var Admission = Kernelˉwvbˉadmission.Build();
        var Processˉscenario = scenario switch
        {
            Firmwareˉprobeˉscenario.Userˉfault => Kernelˉprocessˉscenario.Userˉfault,
            Firmwareˉprobeˉscenario.Serviceˉfault => Kernelˉprocessˉscenario.Serviceˉfault,
            _ => Kernelˉprocessˉscenario.Normal,
        };
        var Processˉimage = Kernelˉprocessˉimage.Build(Admission, Processˉscenario);
        var Process = Kernelˉprocessˉx64.Build(Processˉimage, Processˉscenario);
        var Nativeˉprobe = Kernelˉnativeˉprobe.Build();
        var Exceptions = Kernelˉexceptionˉx64.Build();
        var Paging = Kernelˉpagingˉx64.Build();

        var Loader = Buildˉloaderˉmachineˉcode(scenario);
        var Loaderˉobject = new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 16, (uint)Loader.Bytes.Length, Loader.Bytes)],
            [
                new(
                    ENTRY_SYMBOL,
                    Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function,
                    0,
                    0,
                    (uint)Loader.Bytes.Length),
                new(
                    KERNEL_ENTRY_SYMBOL,
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
                new(
                    Kernelˉassemblyˉcontract.Q35_SHUTDOWN_SYMBOL,
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
            ],
            [
                new(Objectˉrelocationˉkind.Relativeˉi32, 0, Loader.Kernelˉcallˉoffset, 1, -4),
                new(Objectˉrelocationˉkind.Relativeˉi32, 0, Loader.Shutdownˉcallˉoffset, 2, -4),
            ]);
        var Supportˉcode = Buildˉwriteˉbyteˉmachineˉcode();
        var Supportˉobject = new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 16, (uint)Supportˉcode.Length, Supportˉcode)],
            [new(
                X64_WRITE_BYTE_SYMBOL,
                Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Function,
                0,
                0,
                (uint)Supportˉcode.Length)],
            []);
        var Memory = Kernelˉmemoryˉx64.Build(scenario);
        var Memoryˉobject = new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 16, (uint)Memory.Bytes.Length, Memory.Bytes)],
            [
                new(
                    Kernelˉmemoryˉcontract.ALLOCATE_PAGES_SYMBOL,
                    Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function,
                    0,
                    Memory.Allocatorˉoffset,
                    Memory.Releaserˉoffset - Memory.Allocatorˉoffset),
                new(
                    Kernelˉmemoryˉcontract.MEMORY_ENTER_SYMBOL,
                    Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function,
                    0,
                    0,
                    Memory.Enterˉbytes),
                new(
                    Kernelˉmemoryˉcontract.RELEASE_TAIL_PAGES_SYMBOL,
                    Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function,
                    0,
                    Memory.Releaserˉoffset,
                    checked((uint)Memory.Bytes.Length - Memory.Releaserˉoffset)),
                new(
                    Kernelˉassemblyˉcontract.MAIN_SHIM_SYMBOL,
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
                new(
                    Kernelˉexceptionˉcontract.INSTALL_SYMBOL,
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
                new(
                    Kernelˉpagingˉcontract.INSTALL_SYMBOL,
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
            ],
            [.. Memory.Relocations.Select(Relocation => new Objectˉrelocation(
                Objectˉrelocationˉkind.Relativeˉi32,
                0,
                Relocation.Offset,
                Relocation.Symbolˉindex,
                -4))]);
        var Loaderˉobjectˉbytes = Objectˉcodec.Write(Loaderˉobject).ToImmutableArray();
        var Memoryˉobjectˉbytes = Objectˉcodec.Write(Memoryˉobject).ToImmutableArray();
        var Assemblyˉshimˉobjectˉbytes = Kernelˉassemblyˉshim.Buildˉobject();
        var Supportˉobjectˉbytes = Objectˉcodec.Write(Supportˉobject).ToImmutableArray();
        var Link = Linkˉcompiler.Link(
            [
                new(Loaderˉobjectˉbytes),
                new(Kernel.Objectˉbytes),
                new(Admission.Admissionˉnativeˉobjectˉbytes),
                new(Nativeˉprobe.Nativeˉobjectˉbytes),
                new(Processˉimage.Policyˉnativeˉobjectˉbytes),
                new(Process.Objectˉbytes),
                new(Memoryˉobjectˉbytes),
                new(Exceptions.Objectˉbytes),
                new(Paging.Objectˉbytes),
                new(Assemblyˉshimˉobjectˉbytes),
                new(Admission.Bridgeˉobjectˉbytes),
                new(Nativeˉprobe.Bridgeˉobjectˉbytes),
                new(Supportˉobjectˉbytes),
            ],
            new(Uefiˉapplicationˉcontract.REQUIRED_LINK_BASE_ADDRESS, ENTRY_SYMBOL));
        if (!Link.Success)
        {
            throw new InvalidOperationException(
                $"The firmware probe did not link: {Link.Diagnostics[0].Code}: {Link.Diagnostics[0].Message}");
        }
        if (Link.Entryˉaddress != 0 || Link.Imageˉbytes.Length > (int)Kernelˉpagingˉcontract.EXECUTABLE_BYTES)
        {
            throw new InvalidOperationException(
                $"The linked firmware payload is {Link.Imageˉbytes.Length} bytes and does not fit the fixed " +
                $"{Kernelˉpagingˉcontract.EXECUTABLE_BYTES}-byte executable window.");
        }

        var Application = Uefiˉapplicationˉwriter.Write(Link);
        if (!Application.Success)
        {
            throw new InvalidOperationException(
                $"The firmware probe did not encode: {Application.Diagnostics[0].Code}: {Application.Diagnostics[0].Message}");
        }
        return Application.Imageˉbytes;
    }

    private static Bootstrapˉcode Buildˉloaderˉmachineˉcode(Firmwareˉprobeˉscenario scenario)
    {
        var Output = new X64ˉcodeˉbuilder();
        uint Kernelˉcallˉoffset;
        uint Shutdownˉcallˉoffset;

        Output.Emit(0x48, 0x81, 0xEC);
        Output.Emitˉu32(FRAME_BYTES);
        Emitˉstoreˉstackˉrcx(Output, IMAGE_HANDLE_OFFSET);
        Emitˉstoreˉstackˉrdx(Output, SYSTEM_TABLE_OFFSET);
        Emitˉserialˉinitialization(Output);
        Emitˉserialˉtext(Output, ENTRY_MARKER);

        Emitˉloadˉstackˉrax(Output, SYSTEM_TABLE_OFFSET);
        Output.Emit(0x48, 0x85, 0xC0);
        Output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        Output.Emit(0x48, 0xB9);
        Output.Emitˉu64(EFI_SYSTEM_TABLE_SIGNATURE);
        Output.Emit(0x48, 0x39, 0x08);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Output.Emit(0x81, 0x78, 0x08);
        Output.Emitˉu32(EFI_1_02_REVISION);
        Output.Jumpˉif(CONDITION_BELOW, FAILURE_LABEL);
        Output.Emit(0x83, 0x78, 0x0C, (byte)EFI_SYSTEM_TABLE_MINIMUM_BYTES);
        Output.Jumpˉif(CONDITION_BELOW, FAILURE_LABEL);
        Output.Emit(0x83, 0x78, 0x14, 0x00);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Output.Emit(0x48, 0x8B, 0x48, 0x60);
        Output.Emit(0x48, 0x85, 0xC9);
        Output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        Emitˉstoreˉstackˉrcx(Output, BOOT_SERVICES_OFFSET);

        Output.Emit(0x48, 0xBA);
        Output.Emitˉu64(EFI_BOOT_SERVICES_SIGNATURE);
        Output.Emit(0x48, 0x39, 0x11);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Output.Emit(0x81, 0x79, 0x08);
        Output.Emitˉu32(EFI_1_02_REVISION);
        Output.Jumpˉif(CONDITION_BELOW, FAILURE_LABEL);
        Output.Emit(0x81, 0x79, 0x0C);
        Output.Emitˉu32(EFI_BOOT_SERVICES_MINIMUM_BYTES);
        Output.Jumpˉif(CONDITION_BELOW, FAILURE_LABEL);
        Output.Emit(0x83, 0x79, 0x14, 0x00);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉrequireˉpointer(Output, GET_MEMORY_MAP_OFFSET, FAILURE_LABEL);
        Emitˉrequireˉpointer(Output, ALLOCATE_POOL_OFFSET, FAILURE_LABEL);
        Emitˉrequireˉpointer(Output, FREE_POOL_OFFSET, FAILURE_LABEL);
        Emitˉrequireˉpointer(Output, EXIT_BOOT_SERVICES_OFFSET, FAILURE_LABEL);
        Emitˉserialˉtext(Output, SYSTEM_TABLE_MARKER);

        Output.Emit(0x31, 0xC0);
        Emitˉstoreˉstackˉrax(Output, MAP_SIZE_OFFSET);
        Emitˉstoreˉstackˉrax(Output, MAP_KEY_OFFSET);
        Emitˉstoreˉstackˉrax(Output, DESCRIPTOR_SIZE_OFFSET);
        Emitˉstoreˉstackˉrax(Output, DESCRIPTOR_VERSION_OFFSET);
        Emitˉstoreˉstackˉrax(Output, MAP_BUFFER_OFFSET);
        Output.Emit(0x89, 0x44, 0x24, EXIT_ATTEMPTED_OFFSET);

        Emitˉaddressˉstackˉrcx(Output, MAP_SIZE_OFFSET);
        Output.Emit(0x31, 0xD2);
        Emitˉaddressˉstackˉr8(Output, MAP_KEY_OFFSET);
        Emitˉaddressˉstackˉr9(Output, DESCRIPTOR_SIZE_OFFSET);
        Emitˉaddressˉstackˉrax(Output, DESCRIPTOR_VERSION_OFFSET);
        Emitˉstoreˉstackˉrax(Output, FIFTH_ARGUMENT_OFFSET);
        Emitˉcallˉbootˉservice(Output, GET_MEMORY_MAP_OFFSET);
        Output.Emit(0x48, 0xB9);
        Output.Emitˉu64(EFI_BUFFER_TOO_SMALL);
        Output.Emit(0x48, 0x39, 0xC8);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉvalidateˉdescriptorˉmetadata(Output, FAILURE_LABEL);

        Emitˉloadˉstackˉrcx(Output, MAP_SIZE_OFFSET);
        Output.Emit(0x48, 0x85, 0xC9);
        Output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        Output.Emit(0x48, 0x81, 0xF9);
        Output.Emitˉu32(MAX_MEMORY_MAP_BYTES);
        Output.Jumpˉif(CONDITION_ABOVE, FAILURE_LABEL);
        Emitˉloadˉstackˉrax(Output, DESCRIPTOR_SIZE_OFFSET);
        Output.Emit(0x48, 0x01, 0xC0);
        Output.Emit(0x48, 0x01, 0xC8);
        Output.Emit(0x48, 0x3D);
        Output.Emitˉu32(MAX_MEMORY_MAP_BYTES);
        Output.Jumpˉif(CONDITION_ABOVE, FAILURE_LABEL);
        Emitˉstoreˉstackˉrax(Output, MAP_ALLOCATION_SIZE_OFFSET);

        Output.Emit(0xB9);
        Output.Emitˉu32(EFI_LOADER_DATA);
        Emitˉloadˉstackˉrdx(Output, MAP_ALLOCATION_SIZE_OFFSET);
        Emitˉaddressˉstackˉr8(Output, MAP_BUFFER_OFFSET);
        Emitˉcallˉbootˉservice(Output, ALLOCATE_POOL_OFFSET);
        Output.Emit(0x48, 0x85, 0xC0);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Emitˉloadˉstackˉrax(Output, MAP_BUFFER_OFFSET);
        Output.Emit(0x48, 0x85, 0xC0);
        Output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);

        Emitˉloadˉstackˉrax(Output, MAP_ALLOCATION_SIZE_OFFSET);
        Emitˉstoreˉstackˉrax(Output, MAP_SIZE_OFFSET);
        Emitˉaddressˉstackˉrcx(Output, MAP_SIZE_OFFSET);
        Emitˉloadˉstackˉrdx(Output, MAP_BUFFER_OFFSET);
        Emitˉaddressˉstackˉr8(Output, MAP_KEY_OFFSET);
        Emitˉaddressˉstackˉr9(Output, DESCRIPTOR_SIZE_OFFSET);
        Emitˉaddressˉstackˉrax(Output, DESCRIPTOR_VERSION_OFFSET);
        Emitˉstoreˉstackˉrax(Output, FIFTH_ARGUMENT_OFFSET);
        Emitˉcallˉbootˉservice(Output, GET_MEMORY_MAP_OFFSET);
        Output.Emit(0x48, 0x85, 0xC0);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, CLEANUP_FAILURE_LABEL);
        Output.Emit(0xC7, 0x44, 0x24, EXIT_ATTEMPTS_OFFSET);
        Output.Emitˉu32(EXIT_BOOT_SERVICES_MAX_ATTEMPTS);

        Output.Mark(MAP_VALIDATE_LABEL);
        Emitˉvalidateˉdescriptorˉmetadata(Output, CLEANUP_FAILURE_LABEL);

        Emitˉloadˉstackˉrax(Output, MAP_SIZE_OFFSET);
        Output.Emit(0x48, 0x85, 0xC0);
        Output.Jumpˉif(CONDITION_EQUAL, CLEANUP_FAILURE_LABEL);
        Output.Emit(0x48, 0x3B, 0x44, 0x24, MAP_ALLOCATION_SIZE_OFFSET);
        Output.Jumpˉif(CONDITION_ABOVE, CLEANUP_FAILURE_LABEL);
        Output.Emit(0x31, 0xD2);
        Output.Emit(0x48, 0xF7, 0x74, 0x24, DESCRIPTOR_SIZE_OFFSET);
        Output.Emit(0x48, 0x85, 0xD2);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, CLEANUP_FAILURE_LABEL);
        Output.Emit(0x48, 0x85, 0xC0);
        Output.Jumpˉif(CONDITION_EQUAL, CLEANUP_FAILURE_LABEL);

        Emitˉloadˉstackˉr10(Output, MAP_BUFFER_OFFSET);
        Output.Emit(0x49, 0x89, 0xC3);
        Emitˉloadˉstackˉr8(Output, DESCRIPTOR_SIZE_OFFSET);
        Output.Mark(DESCRIPTOR_LOOP_LABEL);
        Output.Emit(0x49, 0xF7, 0x42, 0x08);
        Output.Emitˉu32(0x0FFF);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, CLEANUP_FAILURE_LABEL);
        Output.Emit(0x49, 0xF7, 0x42, 0x10);
        Output.Emitˉu32(0x0FFF);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, CLEANUP_FAILURE_LABEL);
        Output.Emit(0x49, 0x83, 0x7A, 0x18, 0x00);
        Output.Jumpˉif(CONDITION_EQUAL, CLEANUP_FAILURE_LABEL);
        Output.Emit(0x49, 0x8B, 0x4A, 0x18);
        Output.Emit(0x48, 0xB8);
        Output.Emitˉu64(0x0010_0000_0000_0000);
        Output.Emit(0x48, 0x39, 0xC1);
        Output.Jumpˉif(CONDITION_ABOVE, CLEANUP_FAILURE_LABEL);
        Output.Emit(0x48, 0xFF, 0xC9);
        Output.Emit(0x48, 0xC1, 0xE1, 0x0C);
        Output.Emit(0x49, 0x03, 0x4A, 0x08);
        Output.Jumpˉif(CONDITION_BELOW, CLEANUP_FAILURE_LABEL);
        Output.Emit(0x4D, 0x01, 0xC2);
        Output.Emit(0x49, 0xFF, 0xCB);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, DESCRIPTOR_LOOP_LABEL);

        Output.Emit(0xC7, 0x44, 0x24, EXIT_ATTEMPTED_OFFSET, 0x01, 0x00, 0x00, 0x00);
        Emitˉloadˉstackˉrcx(Output, IMAGE_HANDLE_OFFSET);
        Emitˉloadˉstackˉrdx(Output, MAP_KEY_OFFSET);
        Emitˉcallˉbootˉservice(Output, EXIT_BOOT_SERVICES_OFFSET);
        Output.Emit(0x48, 0x85, 0xC0);
        Output.Jumpˉif(CONDITION_EQUAL, EXIT_SUCCESS_LABEL);
        Output.Emit(0x48, 0xB9);
        Output.Emitˉu64(EFI_INVALID_PARAMETER);
        Output.Emit(0x48, 0x39, 0xC8);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, CLEANUP_FAILURE_LABEL);
        Output.Emit(0xFF, 0x4C, 0x24, EXIT_ATTEMPTS_OFFSET);
        Output.Jumpˉif(CONDITION_EQUAL, CLEANUP_FAILURE_LABEL);

        Emitˉloadˉstackˉrax(Output, MAP_ALLOCATION_SIZE_OFFSET);
        Emitˉstoreˉstackˉrax(Output, MAP_SIZE_OFFSET);
        Emitˉaddressˉstackˉrcx(Output, MAP_SIZE_OFFSET);
        Emitˉloadˉstackˉrdx(Output, MAP_BUFFER_OFFSET);
        Emitˉaddressˉstackˉr8(Output, MAP_KEY_OFFSET);
        Emitˉaddressˉstackˉr9(Output, DESCRIPTOR_SIZE_OFFSET);
        Emitˉaddressˉstackˉrax(Output, DESCRIPTOR_VERSION_OFFSET);
        Emitˉstoreˉstackˉrax(Output, FIFTH_ARGUMENT_OFFSET);
        Emitˉcallˉbootˉservice(Output, GET_MEMORY_MAP_OFFSET);
        Output.Emit(0x48, 0x85, 0xC0);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, CLEANUP_FAILURE_LABEL);
        Output.Jump(MAP_VALIDATE_LABEL);

        Output.Mark(EXIT_SUCCESS_LABEL);
        Output.Emit(0xFA);
        Emitˉserialˉtext(Output, MEMORY_MAP_MARKER);
        Emitˉserialˉtext(Output, BOOT_SERVICES_MARKER);

        Emitˉloadˉstackˉr10(Output, MAP_BUFFER_OFFSET);
        Emitˉloadˉstackˉr11(Output, MAP_SIZE_OFFSET);
        Emitˉloadˉstackˉr8(Output, DESCRIPTOR_SIZE_OFFSET);
        Emitˉloadˉstackˉr9(Output, DESCRIPTOR_VERSION_OFFSET);
        Output.Emit(0x48, 0xB8);
        Output.Emitˉu64(X64ˉkernelˉcontract.HANDOFF_MAGIC);
        Emitˉstoreˉstackˉrax(Output, HANDOFF_OFFSET);
        Output.Emit(0xC7, 0x44, 0x24, HANDOFF_VERSION_OFFSET);
        Output.Emitˉu32(X64ˉkernelˉcontract.HANDOFF_VERSION);
        Output.Emit(0xC7, 0x44, 0x24, HANDOFF_SIZE_OFFSET);
        Output.Emitˉu32(X64ˉkernelˉcontract.HANDOFF_BYTES);
        Emitˉstoreˉstackˉr10(Output, HANDOFF_MAP_BUFFER_OFFSET);
        Emitˉstoreˉstackˉr11(Output, HANDOFF_MAP_BYTES_OFFSET);
        Emitˉstoreˉstackˉr8(Output, HANDOFF_DESCRIPTOR_BYTES_OFFSET);
        Output.Emit(0x44, 0x89, 0x4C, 0x24, HANDOFF_DESCRIPTOR_VERSION_OFFSET);
        Output.Emit(0xC7, 0x44, 0x24, HANDOFF_RESERVED_OFFSET, 0x00, 0x00, 0x00, 0x00);
        Emitˉaddressˉstackˉrcx(Output, HANDOFF_OFFSET);
        Kernelˉcallˉoffset = Output.Emitˉcallˉplaceholder();
        Output.Emit(0x48, 0x85, 0xC0);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, TERMINAL_FAILURE_LABEL);
        Emitˉserialˉtext(Output, CPU_EXCEPTIONS_MARKER);
        Emitˉserialˉtext(Output, NATIVE_CONTEXT_MARKER);
        Emitˉserialˉtext(Output, NATIVE_WVB_MARKER);
        Emitˉserialˉtext(Output, WINDVALE_SOURCE_MARKER);
        if (scenario == Firmwareˉprobeˉscenario.Userˉfault)
        {
            Emitˉserialˉtext(Output, USER_FAULT_CONTAINED_MARKER);
        }
        Emitˉserialˉtext(Output, SUCCESS_MARKER);
        Emitˉserialˉtext(Output, SHUTDOWN_MARKER);
        Shutdownˉcallˉoffset = Output.Emitˉcallˉplaceholder();
        Output.Jump(TERMINAL_FAILURE_LABEL);

        Output.Mark(CLEANUP_FAILURE_LABEL);
        Emitˉloadˉstackˉrcx(Output, MAP_BUFFER_OFFSET);
        Emitˉcallˉbootˉservice(Output, FREE_POOL_OFFSET);
        Output.Emit(0x83, 0x7C, 0x24, EXIT_ATTEMPTED_OFFSET, 0x00);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, TERMINAL_FAILURE_LABEL);
        Output.Jump(FAILURE_LABEL);

        Output.Mark(TERMINAL_FAILURE_LABEL);
        Emitˉserialˉtext(Output, FAILURE_MARKER);
        Emitˉdebugˉexit(Output, 1);
        Emitˉhaltˉloop(Output, TERMINAL_HALT_LABEL);

        Output.Mark(FAILURE_LABEL);
        Emitˉserialˉtext(Output, FAILURE_MARKER);
        Emitˉdebugˉexit(Output, 1);
        Output.Emit(0x48, 0xB8);
        Output.Emitˉu64(EFI_DEVICE_ERROR);
        Emitˉrestoreˉstackˉandˉreturn(Output);
        return new(Output.Build(), Kernelˉcallˉoffset, Shutdownˉcallˉoffset);
    }

    private static ImmutableArray<byte> Buildˉwriteˉbyteˉmachineˉcode()
    {
        var Output = new X64ˉcodeˉbuilder();
        Emitˉmoveˉedx(Output, 0x03FD);
        Output.Mark(WRITE_BYTE_WAIT_LABEL);
        Output.Emit(0xEC, 0xA8, 0x20);
        Output.Jumpˉif(CONDITION_EQUAL, WRITE_BYTE_WAIT_LABEL);
        Emitˉmoveˉedx(Output, 0x03F8);
        Output.Emit(0x8B, 0xC1, 0xEE, 0xC3);
        return Output.Build();
    }

    private static string Loadˉhelloˉworldˉsource(Firmwareˉprobeˉscenario scenario)
    {
        var Resource = scenario == Firmwareˉprobeˉscenario.Serviceˉfault
            ? SERVICE_FAULT_HELLO_RESOURCE
            : HELLO_WORLD_RESOURCE;
        using var Stream = typeof(Firmwareˉprobe).Assembly.GetManifestResourceStream(Resource) ??
            throw new InvalidOperationException($"Embedded Windvale source '{Resource}' is missing.");
        using var Reader = new StreamReader(Stream, new UTF8Encoding(false, true), false);
        return Reader.ReadToEnd();
    }

    private static void Emitˉvalidateˉdescriptorˉmetadata(
        X64ˉcodeˉbuilder output,
        string failureˉlabel)
    {
        Emitˉloadˉstackˉrax(output, DESCRIPTOR_SIZE_OFFSET);
        output.Emit(0x48, 0x83, 0xF8, (byte)EFI_MEMORY_DESCRIPTOR_MINIMUM_BYTES);
        output.Jumpˉif(CONDITION_BELOW, failureˉlabel);
        output.Emit(0x48, 0x3D);
        output.Emitˉu32(EFI_MEMORY_DESCRIPTOR_MAXIMUM_BYTES);
        output.Jumpˉif(CONDITION_ABOVE, failureˉlabel);
        output.Emit(0x83, 0x7C, 0x24, DESCRIPTOR_VERSION_OFFSET, (byte)EFI_MEMORY_DESCRIPTOR_VERSION);
        output.Jumpˉif(CONDITION_NOT_EQUAL, failureˉlabel);
    }

    private static void Emitˉrequireˉpointer(
        X64ˉcodeˉbuilder output,
        uint offset,
        string failureˉlabel)
    {
        if (offset <= sbyte.MaxValue)
        {
            output.Emit(0x48, 0x83, 0x79, (byte)offset, 0x00);
        }
        else
        {
            output.Emit(0x48, 0x83, 0xB9);
            output.Emitˉu32(offset);
            output.Emit(0x00);
        }
        output.Jumpˉif(CONDITION_EQUAL, failureˉlabel);
    }

    private static void Emitˉcallˉbootˉservice(X64ˉcodeˉbuilder output, uint offset)
    {
        Emitˉloadˉstackˉrax(output, BOOT_SERVICES_OFFSET);
        if (offset <= sbyte.MaxValue)
        {
            output.Emit(0xFF, 0x50, (byte)offset);
        }
        else
        {
            output.Emit(0xFF, 0x90);
            output.Emitˉu32(offset);
        }
    }

    private static void Emitˉserialˉinitialization(X64ˉcodeˉbuilder output)
    {
        Emitˉoutˉbyte(output, 0x03F9, 0x00);
        Emitˉoutˉbyte(output, 0x03FB, 0x80);
        Emitˉoutˉbyte(output, 0x03F8, 0x03);
        Emitˉoutˉbyte(output, 0x03F9, 0x00);
        Emitˉoutˉbyte(output, 0x03FB, 0x03);
        Emitˉoutˉbyte(output, 0x03FA, 0xC7);
        Emitˉoutˉbyte(output, 0x03FC, 0x0B);
    }

    private static void Emitˉserialˉtext(X64ˉcodeˉbuilder output, string value)
    {
        foreach (var Value in Encoding.ASCII.GetBytes(value))
        {
            Emitˉmoveˉedx(output, 0x03FD);
            output.Emit(0xEC, 0xA8, 0x20, 0x74, 0xFB);
            Emitˉmoveˉedx(output, 0x03F8);
            Emitˉmoveˉeax(output, Value);
            output.Emit(0xEE);
        }
    }

    private static void Emitˉdebugˉexit(X64ˉcodeˉbuilder output, uint value)
    {
        Emitˉmoveˉedx(output, 0x00F4);
        Emitˉmoveˉeax(output, value);
        output.Emit(0xEF);
    }

    private static void Emitˉhaltˉloop(X64ˉcodeˉbuilder output, string label)
    {
        output.Emit(0xFA);
        output.Mark(label);
        output.Emit(0xF4);
        output.Jump(label);
    }

    private static void Emitˉoutˉbyte(X64ˉcodeˉbuilder output, uint port, byte value)
    {
        Emitˉmoveˉedx(output, port);
        Emitˉmoveˉeax(output, value);
        output.Emit(0xEE);
    }

    private static void Emitˉmoveˉedx(X64ˉcodeˉbuilder output, uint value)
    {
        output.Emit(0xBA);
        output.Emitˉu32(value);
    }

    private static void Emitˉmoveˉeax(X64ˉcodeˉbuilder output, uint value)
    {
        output.Emit(0xB8);
        output.Emitˉu32(value);
    }

    private static void Emitˉloadˉstackˉrax(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x48, 0x8B, 0x44, 0x24, offset);

    private static void Emitˉloadˉstackˉrcx(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x48, 0x8B, 0x4C, 0x24, offset);

    private static void Emitˉloadˉstackˉrdx(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x48, 0x8B, 0x54, 0x24, offset);

    private static void Emitˉloadˉstackˉr8(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x4C, 0x8B, 0x44, 0x24, offset);

    private static void Emitˉloadˉstackˉr9(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x4C, 0x8B, 0x4C, 0x24, offset);

    private static void Emitˉloadˉstackˉr10(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x4C, 0x8B, 0x54, 0x24, offset);

    private static void Emitˉloadˉstackˉr11(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x4C, 0x8B, 0x5C, 0x24, offset);

    private static void Emitˉstoreˉstackˉrax(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x48, 0x89, 0x44, 0x24, offset);

    private static void Emitˉstoreˉstackˉrcx(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x48, 0x89, 0x4C, 0x24, offset);

    private static void Emitˉstoreˉstackˉrdx(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x48, 0x89, 0x54, 0x24, offset);

    private static void Emitˉstoreˉstackˉr8(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x4C, 0x89, 0x44, 0x24, offset);

    private static void Emitˉstoreˉstackˉr10(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x4C, 0x89, 0x54, 0x24, offset);

    private static void Emitˉstoreˉstackˉr11(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x4C, 0x89, 0x5C, 0x24, offset);

    private static void Emitˉaddressˉstackˉrax(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x48, 0x8D, 0x44, 0x24, offset);

    private static void Emitˉaddressˉstackˉrcx(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x48, 0x8D, 0x4C, 0x24, offset);

    private static void Emitˉaddressˉstackˉr8(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x4C, 0x8D, 0x44, 0x24, offset);

    private static void Emitˉaddressˉstackˉr9(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x4C, 0x8D, 0x4C, 0x24, offset);

    private static void Emitˉrestoreˉstackˉandˉreturn(X64ˉcodeˉbuilder output)
    {
        output.Emit(0x48, 0x81, 0xC4);
        output.Emitˉu32(FRAME_BYTES);
        output.Emit(0xC3);
    }

    private sealed record Bootstrapˉcode(
        ImmutableArray<byte> Bytes,
        uint Kernelˉcallˉoffset,
        uint Shutdownˉcallˉoffset);
}
