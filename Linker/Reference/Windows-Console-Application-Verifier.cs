using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Windowsˉconsoleˉapplicationˉverifier
{
    private const int OPTIONAL_HEADER_OFFSET = 0x98;
    private const int SECTION_TABLE_OFFSET = 0x188;
    private const int HEADERS_BYTES = 0x200;
    private const uint FILE_ALIGNMENT = 0x200;
    private const uint SECTION_ALIGNMENT = 0x1000;
    private const uint TEXT_RVA = 0x1000;
    private const uint DATA_RAW_BYTES = 0x200;
    private const uint RELOCATION_DATA_BYTES = 12;
    private const uint RELOCATION_RAW_BYTES = 0x200;

    public static Verifiedˉwindowsˉconsoleˉapplication Verify(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length is < 2_048 or
            > Windowsˉconsoleˉapplicationˉcontract.MAX_APPLICATION_BYTES)
        {
            Fail("WVW2001", "The application length is outside the version 1 bounds.");
        }

        Requireˉu16(bytes, 0x00, 0x5A4D, "WVW2002", "The DOS signature is invalid.");
        Requireˉzero(bytes, 0x02, 0x3A, "WVW2002", "The DOS header contains noncanonical bytes.");
        Requireˉu32(bytes, 0x3C, 0x80, "WVW2002", "The PE header offset is invalid.");
        Requireˉzero(bytes, 0x40, 0x40, "WVW2002", "The DOS stub contains noncanonical bytes.");
        Requireˉu32(bytes, 0x80, 0x0000_4550, "WVW2003", "The PE signature is invalid.");

        Requireˉu16(bytes, 0x84, 0x8664, "WVW2003", "The image machine is not x86-64.");
        Requireˉu16(bytes, 0x86, 3, "WVW2003", "The image does not have exactly three sections.");
        Requireˉu32(bytes, 0x88, 0, "WVW2003", "The COFF timestamp must be zero.");
        Requireˉu32(bytes, 0x8C, 0, "WVW2003", "The COFF symbol-table pointer must be zero.");
        Requireˉu32(bytes, 0x90, 0, "WVW2003", "The COFF symbol count must be zero.");
        Requireˉu16(bytes, 0x94, 0xF0, "WVW2003", "The optional-header length is invalid.");
        Requireˉu16(bytes, 0x96, 0x0022, "WVW2003", "The COFF characteristics are noncanonical.");

        var Optional = OPTIONAL_HEADER_OFFSET;
        Requireˉu16(bytes, Optional + 0, 0x020B, "WVW2004", "The image is not PE32+.");
        Requireˉbyte(
            bytes,
            Optional + 2,
            Windowsˉconsoleˉapplicationˉcontract.FORMAT_VERSION,
            "WVW2004",
            "The Windvale writer version is invalid.");
        Requireˉbyte(bytes, Optional + 3, 0, "WVW2004", "The writer minor version is invalid.");
        var Textˉrawˉbytes = Readˉu32(bytes, Optional + 4);
        Require(Textˉrawˉbytes >= FILE_ALIGNMENT &&
            Textˉrawˉbytes <= Linkˉlimits.MAX_IMAGE_BYTES + FILE_ALIGNMENT,
            "WVW2004", Optional + 4, "The code raw size is invalid.");
        Require(Textˉrawˉbytes % FILE_ALIGNMENT == 0,
            "WVW2004", Optional + 4, "The code raw size is not file-aligned.");
        Requireˉu32(bytes, Optional + 8, DATA_RAW_BYTES + RELOCATION_RAW_BYTES,
            "WVW2004", "The initialized-data size is invalid.");
        Requireˉu32(
            bytes,
            Optional + 12,
            Windowsˉconsoleˉapplicationˉcontract.DATA_VIRTUAL_BYTES - DATA_RAW_BYTES,
            "WVW2004",
            "The uninitialized-data size is invalid.");
        Requireˉu32(bytes, Optional + 16, TEXT_RVA, "WVW2004", "The entry RVA is invalid.");
        Requireˉu32(bytes, Optional + 20, TEXT_RVA, "WVW2004", "The code base is invalid.");
        Requireˉu64(bytes, Optional + 24, 0x0000_0001_4000_0000,
            "WVW2004", "The preferred image base is invalid.");
        Requireˉu32(bytes, Optional + 32, SECTION_ALIGNMENT,
            "WVW2004", "The section alignment is invalid.");
        Requireˉu32(bytes, Optional + 36, FILE_ALIGNMENT,
            "WVW2004", "The file alignment is invalid.");
        Requireˉu16(bytes, Optional + 40, 6, "WVW2004", "The operating-system version is invalid.");
        Requireˉu16(bytes, Optional + 42, 0, "WVW2004", "The operating-system minor version is invalid.");
        Requireˉzero(bytes, Optional + 44, 4, "WVW2004", "The image version must be zero.");
        Requireˉu16(bytes, Optional + 48, 6, "WVW2004", "The subsystem version is invalid.");
        Requireˉu16(bytes, Optional + 50, 0, "WVW2004", "The subsystem minor version is invalid.");
        Requireˉu32(bytes, Optional + 52, 0, "WVW2004", "The Win32 version must be zero.");
        var Imageˉmemoryˉbytes = Readˉu32(bytes, Optional + 56);
        Requireˉu32(bytes, Optional + 60, HEADERS_BYTES,
            "WVW2004", "The header size is invalid.");
        Requireˉu32(bytes, Optional + 64, 0, "WVW2004", "The checksum must be zero.");
        Requireˉu16(bytes, Optional + 68, 3, "WVW2004", "The subsystem is not Windows console.");
        Requireˉu16(bytes, Optional + 70, 0x0160,
            "WVW2004", "The DLL characteristics are noncanonical.");
        Requireˉu64(bytes, Optional + 72, 0x0400_0000,
            "WVW2004", "The stack reserve is invalid.");
        Requireˉu64(bytes, Optional + 80, 0x0001_0000,
            "WVW2004", "The stack commit is invalid.");
        Requireˉu64(bytes, Optional + 88, 0x0010_0000,
            "WVW2004", "The heap reserve is invalid.");
        Requireˉu64(bytes, Optional + 96, 0x0000_1000,
            "WVW2004", "The heap commit is invalid.");
        Requireˉu32(bytes, Optional + 104, 0, "WVW2004", "Loader flags must be zero.");
        Requireˉu32(bytes, Optional + 108, 16, "WVW2004", "The data-directory count is invalid.");
        Requireˉzero(bytes, Optional + 112, 40,
            "WVW2004", "Data directories before base relocation must be zero.");
        var Relocationˉdirectoryˉrva = Readˉu32(bytes, Optional + 152);
        Requireˉu32(bytes, Optional + 156, RELOCATION_DATA_BYTES,
            "WVW2004", "The base-relocation directory size is invalid.");
        Requireˉzero(bytes, Optional + 160, 80,
            "WVW2004", "Unused data directories must be zero.");

        Requireˉname(bytes, SECTION_TABLE_OFFSET, ".text",
            "WVW2005", "The first section is not canonical .text.");
        var Textˉbytes = Readˉu32(bytes, SECTION_TABLE_OFFSET + 8);
        Require(
            Textˉbytes > Windowsˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET &&
                Textˉbytes <= Linkˉlimits.MAX_IMAGE_BYTES +
                    Windowsˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET,
            "WVW2005",
            SECTION_TABLE_OFFSET + 8,
            "The code virtual size is invalid.");
        Requireˉu32(bytes, SECTION_TABLE_OFFSET + 12, TEXT_RVA,
            "WVW2005", "The code RVA is invalid.");
        Requireˉu32(bytes, SECTION_TABLE_OFFSET + 16, Textˉrawˉbytes,
            "WVW2005", "The code raw size is inconsistent.");
        Requireˉu32(bytes, SECTION_TABLE_OFFSET + 20, HEADERS_BYTES,
            "WVW2005", "The code raw offset is invalid.");
        Requireˉzero(bytes, SECTION_TABLE_OFFSET + 24, 12,
            "WVW2005", "The code section contains object metadata.");
        Requireˉu32(bytes, SECTION_TABLE_OFFSET + 36, 0x6000_0020,
            "WVW2005", "The code permissions are invalid.");
        Require(Textˉrawˉbytes == Alignˉup(Textˉbytes, FILE_ALIGNMENT),
            "WVW2005", SECTION_TABLE_OFFSET + 16,
            "The code raw size is not canonical for its virtual size.");

        var Dataˉsection = SECTION_TABLE_OFFSET + 40;
        Requireˉname(bytes, Dataˉsection, ".data",
            "WVW2005", "The second section is not canonical .data.");
        Requireˉu32(
            bytes,
            Dataˉsection + 8,
            Windowsˉconsoleˉapplicationˉcontract.DATA_VIRTUAL_BYTES,
            "WVW2005",
            "The data virtual size is invalid.");
        var Expectedˉdataˉrva = Alignˉup(TEXT_RVA + Textˉbytes, SECTION_ALIGNMENT);
        Requireˉu32(bytes, Dataˉsection + 12, Expectedˉdataˉrva,
            "WVW2005", "The data RVA is invalid.");
        Requireˉu32(bytes, Dataˉsection + 16, DATA_RAW_BYTES,
            "WVW2005", "The data raw size is invalid.");
        var Expectedˉdataˉraw = (uint)HEADERS_BYTES + Textˉrawˉbytes;
        Requireˉu32(bytes, Dataˉsection + 20, Expectedˉdataˉraw,
            "WVW2005", "The data raw offset is invalid.");
        Requireˉzero(bytes, Dataˉsection + 24, 12,
            "WVW2005", "The data section contains object metadata.");
        Requireˉu32(bytes, Dataˉsection + 36, 0xC000_0040,
            "WVW2005", "The data permissions are invalid.");

        var Relocationˉsection = SECTION_TABLE_OFFSET + 80;
        Requireˉname(bytes, Relocationˉsection, ".reloc",
            "WVW2005", "The third section is not canonical .reloc.");
        Requireˉu32(bytes, Relocationˉsection + 8, RELOCATION_DATA_BYTES,
            "WVW2005", "The relocation virtual size is invalid.");
        var Expectedˉrelocationˉrva = Alignˉup(
            Expectedˉdataˉrva + Windowsˉconsoleˉapplicationˉcontract.DATA_VIRTUAL_BYTES,
            SECTION_ALIGNMENT);
        Requireˉu32(bytes, Relocationˉsection + 12, Expectedˉrelocationˉrva,
            "WVW2005", "The relocation RVA is invalid.");
        Require(Relocationˉdirectoryˉrva == Expectedˉrelocationˉrva,
            "WVW2005", Optional + 152,
            "The relocation directory does not identify .reloc.");
        Requireˉu32(bytes, Relocationˉsection + 16, RELOCATION_RAW_BYTES,
            "WVW2005", "The relocation raw size is invalid.");
        var Expectedˉrelocationˉraw = Expectedˉdataˉraw + DATA_RAW_BYTES;
        Requireˉu32(bytes, Relocationˉsection + 20, Expectedˉrelocationˉraw,
            "WVW2005", "The relocation raw offset is invalid.");
        Requireˉzero(bytes, Relocationˉsection + 24, 12,
            "WVW2005", "The relocation section contains object metadata.");
        Requireˉu32(bytes, Relocationˉsection + 36, 0x4200_0040,
            "WVW2005", "The relocation permissions are invalid.");

        var Expectedˉimageˉmemory = Alignˉup(
            Expectedˉrelocationˉrva + RELOCATION_DATA_BYTES,
            SECTION_ALIGNMENT);
        Require(Imageˉmemoryˉbytes == Expectedˉimageˉmemory,
            "WVW2005", Optional + 56, "The in-memory image size is invalid.");
        var Expectedˉfileˉbytes = checked((int)Expectedˉrelocationˉraw + (int)RELOCATION_RAW_BYTES);
        Require(bytes.Length == Expectedˉfileˉbytes,
            "WVW2001", null, "The application has trailing or missing bytes.");

        Verifyˉstartup(bytes, Expectedˉdataˉrva, Textˉbytes, out var Nativeˉentryˉoffset);
        Requireˉzero(
            bytes,
            HEADERS_BYTES + Windowsˉconsoleˉapplicationˉcontract.STARTUP_BYTES,
            Windowsˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET -
                Windowsˉconsoleˉapplicationˉcontract.STARTUP_BYTES,
            "WVW2007",
            "The startup alignment padding is not zero.");
        Requireˉzero(
            bytes,
            checked(HEADERS_BYTES + (int)Textˉbytes),
            checked((int)Textˉrawˉbytes - (int)Textˉbytes),
            "WVW2007",
            "The code padding is not zero.");

        Verifyˉcontext(bytes, checked((int)Expectedˉdataˉraw));
        Requireˉu32(bytes, (int)Expectedˉrelocationˉraw, TEXT_RVA,
            "WVW2006", "The relocation block page is invalid.");
        Requireˉu32(bytes, (int)Expectedˉrelocationˉraw + 4, RELOCATION_DATA_BYTES,
            "WVW2006", "The relocation block size is invalid.");
        Requireˉu32(bytes, (int)Expectedˉrelocationˉraw + 8, 0,
            "WVW2006", "Only absolute relocation padding is permitted in version 1.");
        Requireˉzero(
            bytes,
            (int)Expectedˉrelocationˉraw + (int)RELOCATION_DATA_BYTES,
            checked((int)RELOCATION_RAW_BYTES - (int)RELOCATION_DATA_BYTES),
            "WVW2007",
            "The relocation padding is not zero.");

        var Nativeˉbytes = checked(
            (int)Textˉbytes - Windowsˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET);
        return new(
            bytes.Slice(
                HEADERS_BYTES + Windowsˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET,
                Nativeˉbytes).ToArray().ToImmutableArray(),
            Nativeˉentryˉoffset);
    }

    private static void Verifyˉstartup(
        ReadOnlySpan<byte> bytes,
        uint dataˉrva,
        uint textˉbytes,
        out uint nativeˉentryˉoffset)
    {
        const int Startup = HEADERS_BYTES;
        Requireˉbytes(bytes, Startup + 0,
            [0x48, 0x83, 0xEC, 0x28, 0x48, 0x8D, 0x15],
            "WVW2008", "The startup prologue or context load is invalid.");
        Requireˉrelativeˉtarget(bytes, Startup + 7, TEXT_RVA + 11, dataˉrva,
            "The startup context target is invalid.");
        Requireˉbytes(bytes, Startup + 11, [0x48, 0x8D, 0x05],
            "WVW2008", "The startup record-arena load is invalid.");
        Requireˉrelativeˉtarget(
            bytes,
            Startup + 14,
            TEXT_RVA + 18,
            dataˉrva + Nativeˉexecutionˉcontextˉcontract.SIZE,
            "The startup record-arena target is invalid.");
        Requireˉbytes(
            bytes,
            Startup + 18,
            [0x48, 0x89, 0x42, 0x20, 0x48, 0x8D, 0x05],
            "WVW2008",
            "The startup record store or text-arena load is invalid.");
        Requireˉrelativeˉtarget(
            bytes,
            Startup + 25,
            TEXT_RVA + 29,
            dataˉrva + Nativeˉexecutionˉcontextˉcontract.SIZE +
                Windowsˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES,
            "The startup text-arena target is invalid.");
        Requireˉbytes(
            bytes,
            Startup + 29,
            [0x48, 0x89, 0x42, 0x30, 0x31, 0xC9, 0x49, 0x89, 0xD0, 0x45, 0x31, 0xC9, 0xE8],
            "WVW2008",
            "The startup context store or native-entry call is invalid.");
        Requireˉbytes(
            bytes,
            Startup + 46,
            [
                0x48, 0x89, 0xC2,
                0x48, 0xC1, 0xEA, 0x20,
                0x85, 0xD2,
                0x74, 0x05,
                0xB8, 0x01, 0x00, 0x00, 0x00,
                0x48, 0x83, 0xC4, 0x28,
                0xC3,
            ],
            "WVW2008",
            "The startup status-to-exit epilogue is invalid.");

        var Callˉtarget = checked((long)TEXT_RVA + 46 + Readˉi32(bytes, Startup + 42));
        var Nativeˉstart = checked(
            TEXT_RVA + (uint)Windowsˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET);
        Require(
            Callˉtarget >= Nativeˉstart && Callˉtarget < TEXT_RVA + textˉbytes,
            "WVW2008",
            Startup + 42,
            "The startup call target is outside the native image.");
        nativeˉentryˉoffset = checked((uint)(Callˉtarget - Nativeˉstart));
    }

    private static void Verifyˉcontext(ReadOnlySpan<byte> bytes, int offset)
    {
        Requireˉu32(
            bytes,
            offset + Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION,
            "WVW2009",
            "The execution-context version is invalid.");
        Requireˉu32(
            bytes,
            offset + Nativeˉexecutionˉcontextˉcontract.SIZE_OFFSET,
            Nativeˉexecutionˉcontextˉcontract.SIZE,
            "WVW2009",
            "The execution-context size is invalid.");
        Requireˉu64(
            bytes,
            offset + Nativeˉexecutionˉcontextˉcontract.INSTRUCTION_BUDGET_OFFSET,
            checked((ulong)Nativeˉcontract.DEFAULT_MAXIMUM_INSTRUCTIONS),
            "WVW2009",
            "The instruction budget is invalid.");
        Requireˉu64(
            bytes,
            offset + Nativeˉexecutionˉcontextˉcontract.CALL_DEPTH_BUDGET_OFFSET,
            checked((ulong)Nativeˉcontract.DEFAULT_MAXIMUM_CALL_DEPTH),
            "WVW2009",
            "The call-depth budget is invalid.");
        Requireˉzero(bytes, offset + 24, 16,
            "WVW2009", "The initial service and record pointers must be zero.");
        Requireˉu32(
            bytes,
            offset + Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET,
            Windowsˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES,
            "WVW2009",
            "The record-arena length is invalid.");
        Requireˉzero(bytes, offset + 44, 12,
            "WVW2009", "The initial record cursor and text pointer must be zero.");
        Requireˉu32(
            bytes,
            offset + Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
            Windowsˉconsoleˉapplicationˉcontract.TEXT_ARENA_BYTES,
            "WVW2009",
            "The text-arena length is invalid.");
        Requireˉzero(
            bytes,
            offset + Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
            checked(0x200 - Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET),
            "WVW2009",
            "The remaining initial execution context and data padding must be zero.");
    }

    private static void Requireˉrelativeˉtarget(
        ReadOnlySpan<byte> bytes,
        int fileˉoffset,
        uint sourceˉend,
        uint expectedˉtarget,
        string message)
    {
        var Actual = checked((long)sourceˉend + Readˉi32(bytes, fileˉoffset));
        Require(Actual == expectedˉtarget, "WVW2008", fileˉoffset, message);
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

    private static void Requireˉbyte(
        ReadOnlySpan<byte> bytes,
        int offset,
        byte expected,
        string code,
        string message) =>
        Require(bytes[offset] == expected, code, offset, message);

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

    private static void Requireˉname(
        ReadOnlySpan<byte> bytes,
        int offset,
        string expected,
        string code,
        string message)
    {
        for (var Index = 0; Index < 8; Index++)
        {
            var Expectedˉbyte = Index < expected.Length ? (byte)expected[Index] : (byte)0;
            Require(bytes[offset + Index] == Expectedˉbyte, code, offset + Index, message);
        }
    }

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
        throw new Windowsˉconsoleˉapplicationˉexception(code, message, offset);
}
