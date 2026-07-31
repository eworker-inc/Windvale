using System.Collections.Immutable;
using System.Text;
using Windvale.Linker;
using Windvale.ObjectModel;

namespace Windvale.Bootstrap;

public static class Firmwareˉprobe
{
    public const int FORMAT_VERSION = 3;
    public const string ENTRY_SYMBOL = "Windvale_boot_probe";
    public const string ENTRY_MARKER = "windvale-os-boot 3\nentry=pass\n";
    public const string SYSTEM_TABLE_MARKER = "system-table=pass\n";
    public const string MEMORY_MAP_MARKER = "memory-map=pass\n";
    public const string BOOT_SERVICES_MARKER = "boot-services=exited\n";
    public const string SUCCESS_MARKER = "status=pass\n";
    public const string SERIAL_MARKER =
        ENTRY_MARKER + SYSTEM_TABLE_MARKER + MEMORY_MAP_MARKER + BOOT_SERVICES_MARKER + SUCCESS_MARKER;

    private const string FAILURE_MARKER = "status=fail\n";
    private const string FAILURE_LABEL = "failure";
    private const string CLEANUP_FAILURE_LABEL = "cleanup_failure";
    private const string TERMINAL_FAILURE_LABEL = "terminal_failure";
    private const string TERMINAL_HALT_LABEL = "terminal_halt";
    private const string MAP_VALIDATE_LABEL = "map_validate";
    private const string DESCRIPTOR_LOOP_LABEL = "descriptor_loop";
    private const string EXIT_SUCCESS_LABEL = "exit_success";
    private const string SUCCESS_HALT_LABEL = "success_halt";

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

    public static ImmutableArray<byte> Buildˉapplication()
    {
        var Code = Buildˉmachineˉcode();
        var Object = new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 16, (uint)Code.Length, Code)],
            [new(
                ENTRY_SYMBOL,
                Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Function,
                0,
                0,
                (uint)Code.Length)],
            []);
        var Objectˉbytes = Objectˉcodec.Write(Object).ToImmutableArray();
        var Link = Linkˉcompiler.Link(
            [new(Objectˉbytes)],
            new(Uefiˉapplicationˉcontract.REQUIRED_LINK_BASE_ADDRESS, ENTRY_SYMBOL));
        if (!Link.Success)
        {
            throw new InvalidOperationException(
                $"The firmware probe did not link: {Link.Diagnostics[0].Code}: {Link.Diagnostics[0].Message}");
        }

        var Application = Uefiˉapplicationˉwriter.Write(Link);
        if (!Application.Success)
        {
            throw new InvalidOperationException(
                $"The firmware probe did not encode: {Application.Diagnostics[0].Code}: {Application.Diagnostics[0].Message}");
        }
        return Application.Imageˉbytes;
    }

    private static ImmutableArray<byte> Buildˉmachineˉcode()
    {
        var Output = new X64ˉcodeˉbuilder();

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
        Emitˉserialˉtext(Output, MEMORY_MAP_MARKER);
        Emitˉserialˉtext(Output, BOOT_SERVICES_MARKER);
        Emitˉserialˉtext(Output, SUCCESS_MARKER);
        Emitˉdebugˉexit(Output, 0);
        Emitˉhaltˉloop(Output, SUCCESS_HALT_LABEL);

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
        return Output.Build();
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

    private static void Emitˉloadˉstackˉr10(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x4C, 0x8B, 0x54, 0x24, offset);

    private static void Emitˉstoreˉstackˉrax(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x48, 0x89, 0x44, 0x24, offset);

    private static void Emitˉstoreˉstackˉrcx(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x48, 0x89, 0x4C, 0x24, offset);

    private static void Emitˉstoreˉstackˉrdx(X64ˉcodeˉbuilder output, byte offset) =>
        output.Emit(0x48, 0x89, 0x54, 0x24, offset);

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
}
