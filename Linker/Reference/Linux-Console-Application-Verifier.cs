using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Linuxˉconsoleˉapplicationˉverifier
{
    private const int ELF_HEADER_BYTES = 64;
    private const int PROGRAM_HEADER_BYTES = 56;
    private const int PROGRAM_HEADER_COUNT = 5;
    private const int PROGRAM_HEADER_TABLE_END =
        ELF_HEADER_BYTES + (PROGRAM_HEADER_BYTES * PROGRAM_HEADER_COUNT);
    private const int NOTE_OFFSET = 0x180;
    private const int NOTE_BYTES = 28;
    private const uint PAGE_BYTES = 0x1000;
    private const uint CONTEXT_FILE_BYTES = Nativeˉexecutionˉcontextˉcontract.SIZE;

    public static Verifiedˉlinuxˉconsoleˉapplication Verify(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length is < 8_304 or > Linuxˉconsoleˉapplicationˉcontract.MAX_APPLICATION_BYTES)
        {
            Fail("WVL2001", "The application length is outside the version 1 bounds.");
        }

        Requireˉbytes(
            bytes,
            0,
            [
                0x7F, 0x45, 0x4C, 0x46,
                0x02,
                0x01,
                0x01,
                0x00,
                0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            ],
            "WVL2002",
            "The ELF identification is invalid.");
        Requireˉu16(bytes, 16, 3, "WVL2002", "The application is not a static position-independent ELF.");
        Requireˉu16(bytes, 18, 62, "WVL2002", "The ELF machine is not x86-64.");
        Requireˉu32(bytes, 20, 1, "WVL2002", "The ELF header version is invalid.");
        Requireˉu64(
            bytes,
            24,
            Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS,
            "WVL2002",
            "The ELF entry address is invalid.");
        Requireˉu64(bytes, 32, ELF_HEADER_BYTES, "WVL2002", "The program-header offset is invalid.");
        Requireˉu64(bytes, 40, 0, "WVL2002", "Section headers are not permitted in version 1.");
        Requireˉu32(bytes, 48, 0, "WVL2002", "The x86-64 ELF flags must be zero.");
        Requireˉu16(bytes, 52, ELF_HEADER_BYTES, "WVL2002", "The ELF header size is invalid.");
        Requireˉu16(bytes, 54, PROGRAM_HEADER_BYTES, "WVL2002", "The program-header size is invalid.");
        Requireˉu16(bytes, 56, PROGRAM_HEADER_COUNT, "WVL2002", "The program-header count is invalid.");
        Requireˉzero(bytes, 58, 6, "WVL2002", "Section-header metadata must be zero.");

        var Headerˉload = ELF_HEADER_BYTES;
        Verifyˉprogramˉheader(
            bytes,
            Headerˉload,
            type: 1,
            flags: 4,
            fileˉoffset: 0,
            virtualˉaddress: 0,
            fileˉbytes: Linuxˉconsoleˉapplicationˉcontract.HEADER_BYTES,
            memoryˉbytes: Linuxˉconsoleˉapplicationˉcontract.HEADER_BYTES,
            alignment: PAGE_BYTES,
            "The read-only header segment is invalid.");

        var Textˉload = Headerˉload + PROGRAM_HEADER_BYTES;
        Requireˉu32(bytes, Textˉload + 0, 1, "WVL2003", "The code program header is not PT_LOAD.");
        Requireˉu32(bytes, Textˉload + 4, 5, "WVL2003", "The code segment is not read/execute.");
        Requireˉu64(
            bytes,
            Textˉload + 8,
            Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS,
            "WVL2003",
            "The code file offset is invalid.");
        Requireˉu64(
            bytes,
            Textˉload + 16,
            Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS,
            "WVL2003",
            "The code virtual address is invalid.");
        Requireˉu64(bytes, Textˉload + 24, 0, "WVL2003", "The code physical address must be zero.");
        var Textˉbytes = Readˉu64(bytes, Textˉload + 32);
        Require(
            Textˉbytes > (ulong)Linuxˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET &&
                Textˉbytes <= (ulong)Linkˉlimits.MAX_IMAGE_BYTES +
                    (ulong)Linuxˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET,
            "WVL2004",
            Textˉload + 32,
            "The code size is outside the linked-image bounds.");
        Requireˉu64(bytes, Textˉload + 40, Textˉbytes, "WVL2004", "The code memory size is inconsistent.");
        Requireˉu64(bytes, Textˉload + 48, PAGE_BYTES, "WVL2003", "The code alignment is invalid.");

        var Dataˉoffset = Alignˉup(checked(
            Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS + (uint)Textˉbytes), PAGE_BYTES);
        var Dataˉload = Textˉload + PROGRAM_HEADER_BYTES;
        Verifyˉprogramˉheader(
            bytes,
            Dataˉload,
            type: 1,
            flags: 6,
            fileˉoffset: Dataˉoffset,
            virtualˉaddress: Dataˉoffset,
            fileˉbytes: CONTEXT_FILE_BYTES,
            memoryˉbytes: Linuxˉconsoleˉapplicationˉcontract.DATA_VIRTUAL_BYTES,
            alignment: PAGE_BYTES,
            "The writable context and arena segment is invalid.");

        var Note = Dataˉload + PROGRAM_HEADER_BYTES;
        Verifyˉprogramˉheader(
            bytes,
            Note,
            type: 4,
            flags: 4,
            fileˉoffset: NOTE_OFFSET,
            virtualˉaddress: NOTE_OFFSET,
            fileˉbytes: NOTE_BYTES,
            memoryˉbytes: NOTE_BYTES,
            alignment: 4,
            "The Windvale note program header is invalid.");

        var Stack = Note + PROGRAM_HEADER_BYTES;
        Verifyˉprogramˉheader(
            bytes,
            Stack,
            type: 0x6474_E551,
            flags: 6,
            fileˉoffset: 0,
            virtualˉaddress: 0,
            fileˉbytes: 0,
            memoryˉbytes: Linuxˉconsoleˉapplicationˉcontract.STACK_BYTES,
            alignment: 16,
            "The non-executable stack declaration is invalid.");

        Requireˉzero(
            bytes,
            PROGRAM_HEADER_TABLE_END,
            NOTE_OFFSET - PROGRAM_HEADER_TABLE_END,
            "WVL2008",
            "The program-header padding is not zero.");
        Verifyˉnote(bytes);
        Requireˉzero(
            bytes,
            NOTE_OFFSET + NOTE_BYTES,
            checked((int)Linuxˉconsoleˉapplicationˉcontract.HEADER_BYTES - NOTE_OFFSET - NOTE_BYTES),
            "WVL2008",
            "The remaining read-only header page is not zero.");

        var Expectedˉfileˉbytes = checked((int)Dataˉoffset + (int)CONTEXT_FILE_BYTES);
        Require(
            bytes.Length == Expectedˉfileˉbytes,
            "WVL2001",
            null,
            "The application has trailing or missing bytes.");

        Verifyˉstartup(bytes, Dataˉoffset, checked((uint)Textˉbytes), out var Nativeˉentryˉoffset);
        var Textˉfileˉoffset = checked((int)Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS);
        Requireˉzero(
            bytes,
            Textˉfileˉoffset + Linuxˉconsoleˉapplicationˉcontract.STARTUP_BYTES,
            Linuxˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET -
                Linuxˉconsoleˉapplicationˉcontract.STARTUP_BYTES,
            "WVL2008",
            "The startup alignment padding is not zero.");
        Requireˉzero(
            bytes,
            checked(Textˉfileˉoffset + (int)Textˉbytes),
            checked((int)Dataˉoffset - Textˉfileˉoffset - (int)Textˉbytes),
            "WVL2008",
            "The code-to-data file padding is not zero.");

        Verifyˉcontext(bytes, checked((int)Dataˉoffset));
        var Nativeˉbytes = checked(
            (int)Textˉbytes - Linuxˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET);
        return new(
            bytes.Slice(
                Textˉfileˉoffset + Linuxˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET,
                Nativeˉbytes).ToArray().ToImmutableArray(),
            Nativeˉentryˉoffset);
    }

    private static void Verifyˉnote(ReadOnlySpan<byte> bytes)
    {
        Requireˉu32(bytes, NOTE_OFFSET + 0, 9, "WVL2005", "The note name length is invalid.");
        Requireˉu32(bytes, NOTE_OFFSET + 4, sizeof(uint), "WVL2005", "The note value length is invalid.");
        Requireˉu32(bytes, NOTE_OFFSET + 8, 1, "WVL2005", "The note type is invalid.");
        Requireˉbytes(
            bytes,
            NOTE_OFFSET + 12,
            [0x57, 0x69, 0x6E, 0x64, 0x76, 0x61, 0x6C, 0x65, 0x00],
            "WVL2005",
            "The note owner is invalid.");
        Requireˉzero(bytes, NOTE_OFFSET + 21, 3, "WVL2005", "The note owner padding is not zero.");
        Requireˉu32(
            bytes,
            NOTE_OFFSET + 24,
            Linuxˉconsoleˉapplicationˉcontract.FORMAT_VERSION,
            "WVL2005",
            "The Windvale Linux application version is invalid.");
    }

    private static void Verifyˉstartup(
        ReadOnlySpan<byte> bytes,
        uint dataˉaddress,
        uint textˉbytes,
        out uint nativeˉentryˉoffset)
    {
        var Startup = checked((int)Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS);
        Requireˉbytes(
            bytes,
            Startup,
            [
                0x31, 0xFF,
                0xBE, 0x00, 0x00, 0x00, 0x04,
                0xBA, 0x03, 0x00, 0x00, 0x00,
                0x41, 0xBA, 0x22, 0x00, 0x02, 0x00,
                0x49, 0xC7, 0xC0, 0xFF, 0xFF, 0xFF, 0xFF,
                0x45, 0x31, 0xC9,
                0xB8, 0x09, 0x00, 0x00, 0x00,
                0x0F, 0x05,
                0x48, 0x3D, 0x01, 0xF0, 0xFF, 0xFF,
                0x73, 0x48,
                0x48, 0x8D, 0xA0, 0x00, 0x00, 0x00, 0x04,
                0x48, 0x8D, 0x15,
            ],
            "WVL2006",
            "The mmap-owned stack or context-load prefix is invalid.");
        Requireˉrelativeˉtarget(
            bytes,
            Startup + 53,
            Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS + 57,
            dataˉaddress,
            "The startup context target is invalid.");
        Requireˉbytes(
            bytes,
            Startup + 57,
            [0x48, 0x89, 0xD6, 0x48, 0x8D, 0x05],
            "WVL2006",
            "The System V context duplicate or record-arena load is invalid.");
        Requireˉrelativeˉtarget(
            bytes,
            Startup + 63,
            Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS + 67,
            dataˉaddress + Nativeˉexecutionˉcontextˉcontract.SIZE,
            "The startup record-arena target is invalid.");
        Requireˉbytes(
            bytes,
            Startup + 67,
            [0x48, 0x89, 0x42, 0x20, 0x48, 0x8D, 0x05],
            "WVL2006",
            "The record-arena store or text-arena load is invalid.");
        Requireˉrelativeˉtarget(
            bytes,
            Startup + 74,
            Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS + 78,
            dataˉaddress + Nativeˉexecutionˉcontextˉcontract.SIZE +
                Linuxˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES,
            "The startup text-arena target is invalid.");
        Requireˉbytes(
            bytes,
            Startup + 78,
            [
                0x48, 0x89, 0x42, 0x30,
                0x31, 0xFF,
                0x31, 0xC9,
                0x45, 0x31, 0xC0,
                0x45, 0x31, 0xC9,
                0xE8,
            ],
            "WVL2006",
            "The text-arena store or native-entry call prefix is invalid.");
        Requireˉbytes(
            bytes,
            Startup + 97,
            [
                0x48, 0x89, 0xC2,
                0x48, 0xC1, 0xEA, 0x20,
                0x85, 0xD2,
                0x75, 0x07,
                0x3D, 0xFF, 0x00, 0x00, 0x00,
                0x76, 0x05,
                0xB8, 0x01, 0x00, 0x00, 0x00,
                0x89, 0xC7,
                0xB8, 0x3C, 0x00, 0x00, 0x00,
                0x0F, 0x05,
                0x0F, 0x0B,
            ],
            "WVL2006",
            "The native status, portable process-result, or Linux exit boundary is invalid.");

        var Callˉtarget = checked(
            (long)Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS +
            97 + Readˉi32(bytes, Startup + 93));
        var Nativeˉstart = checked(
            Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS +
            (uint)Linuxˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET);
        Require(
            Callˉtarget >= Nativeˉstart &&
                Callˉtarget < Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS + textˉbytes,
            "WVL2006",
            Startup + 93,
            "The startup call target is outside the native image.");
        nativeˉentryˉoffset = checked((uint)(Callˉtarget - Nativeˉstart));
    }

    private static void Verifyˉcontext(ReadOnlySpan<byte> bytes, int offset)
    {
        Requireˉu32(
            bytes,
            offset + Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION,
            "WVL2007",
            "The execution-context version is invalid.");
        Requireˉu32(
            bytes,
            offset + Nativeˉexecutionˉcontextˉcontract.SIZE_OFFSET,
            Nativeˉexecutionˉcontextˉcontract.SIZE,
            "WVL2007",
            "The execution-context size is invalid.");
        Requireˉu64(
            bytes,
            offset + Nativeˉexecutionˉcontextˉcontract.INSTRUCTION_BUDGET_OFFSET,
            checked((ulong)Nativeˉcontract.DEFAULT_MAXIMUM_INSTRUCTIONS),
            "WVL2007",
            "The instruction budget is invalid.");
        Requireˉu64(
            bytes,
            offset + Nativeˉexecutionˉcontextˉcontract.CALL_DEPTH_BUDGET_OFFSET,
            checked((ulong)Nativeˉcontract.DEFAULT_MAXIMUM_CALL_DEPTH),
            "WVL2007",
            "The call-depth budget is invalid.");
        Requireˉzero(bytes, offset + 24, 16, "WVL2007", "The initial service and record pointers must be zero.");
        Requireˉu32(
            bytes,
            offset + Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET,
            Linuxˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES,
            "WVL2007",
            "The record-arena length is invalid.");
        Requireˉzero(bytes, offset + 44, 12, "WVL2007", "The initial record cursor and text pointer must be zero.");
        Requireˉu32(
            bytes,
            offset + Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
            Linuxˉconsoleˉapplicationˉcontract.TEXT_ARENA_BYTES,
            "WVL2007",
            "The text-arena length is invalid.");
        Requireˉzero(
            bytes,
            offset + Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
            checked((int)CONTEXT_FILE_BYTES - Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET),
            "WVL2007",
            "The remaining initial execution context must be zero.");
    }

    private static void Verifyˉprogramˉheader(
        ReadOnlySpan<byte> bytes,
        int offset,
        uint type,
        uint flags,
        ulong fileˉoffset,
        ulong virtualˉaddress,
        ulong fileˉbytes,
        ulong memoryˉbytes,
        ulong alignment,
        string message)
    {
        Requireˉu32(bytes, offset + 0, type, "WVL2003", message);
        Requireˉu32(bytes, offset + 4, flags, "WVL2003", message);
        Requireˉu64(bytes, offset + 8, fileˉoffset, "WVL2003", message);
        Requireˉu64(bytes, offset + 16, virtualˉaddress, "WVL2003", message);
        Requireˉu64(bytes, offset + 24, 0, "WVL2003", message);
        Requireˉu64(bytes, offset + 32, fileˉbytes, "WVL2003", message);
        Requireˉu64(bytes, offset + 40, memoryˉbytes, "WVL2003", message);
        Requireˉu64(bytes, offset + 48, alignment, "WVL2003", message);
    }

    private static void Requireˉrelativeˉtarget(
        ReadOnlySpan<byte> bytes,
        int fileˉoffset,
        uint sourceˉend,
        uint expectedˉtarget,
        string message)
    {
        var Actual = checked((long)sourceˉend + Readˉi32(bytes, fileˉoffset));
        Require(Actual == expectedˉtarget, "WVL2006", fileˉoffset, message);
    }

    private static uint Alignˉup(uint value, uint alignment) =>
        checked((value + alignment - 1) & ~(alignment - 1));

    private static int Readˉi32(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(offset, sizeof(int)));

    private static ushort Readˉu16(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(offset, sizeof(ushort)));

    private static uint Readˉu32(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(offset, sizeof(uint)));

    private static ulong Readˉu64(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(offset, sizeof(ulong)));

    private static void Requireˉbytes(
        ReadOnlySpan<byte> bytes,
        int offset,
        ReadOnlySpan<byte> expected,
        string code,
        string message)
    {
        for (var Index = 0; Index < expected.Length; Index++)
        {
            Require(bytes[offset + Index] == expected[Index], code, offset + Index, message);
        }
    }

    private static void Requireˉu16(
        ReadOnlySpan<byte> bytes,
        int offset,
        ushort expected,
        string code,
        string message) =>
        Require(Readˉu16(bytes, offset) == expected, code, offset, message);

    private static void Requireˉu32(
        ReadOnlySpan<byte> bytes,
        int offset,
        uint expected,
        string code,
        string message) =>
        Require(Readˉu32(bytes, offset) == expected, code, offset, message);

    private static void Requireˉu64(
        ReadOnlySpan<byte> bytes,
        int offset,
        ulong expected,
        string code,
        string message) =>
        Require(Readˉu64(bytes, offset) == expected, code, offset, message);

    private static void Requireˉzero(
        ReadOnlySpan<byte> bytes,
        int offset,
        int length,
        string code,
        string message)
    {
        for (var Index = 0; Index < length; Index++)
        {
            Require(bytes[offset + Index] == 0, code, offset + Index, message);
        }
    }

    private static void Require(bool condition, string code, int? offset, string message)
    {
        if (!condition)
        {
            Fail(code, message, offset);
        }
    }

    private static void Fail(string code, string message, int? offset = null) =>
        throw new Linuxˉconsoleˉapplicationˉexception(code, message, offset);
}
