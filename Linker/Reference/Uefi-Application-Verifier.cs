using System.Buffers.Binary;
using System.Collections.Immutable;

namespace Windvale.Linker;

public static class Uefiˉapplicationˉverifier
{
    public static Verifiedˉuefiˉapplication Verify(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length is < 1_536 or > Uefiˉapplicationˉcontract.MAX_APPLICATION_BYTES)
        {
            Fail("WVU2001", "The application length is outside the version 2 bounds.");
        }

        Requireˉu16(bytes, 0x00, 0x5A4D, "WVU2002", "The DOS signature is invalid.");
        Requireˉzero(bytes, 0x02, 0x3A, "WVU2002", "The DOS header contains noncanonical bytes.");
        Requireˉu32(bytes, 0x3C, 0x80, "WVU2002", "The PE header offset is invalid.");
        Requireˉzero(bytes, 0x40, 0x40, "WVU2002", "The DOS stub contains noncanonical bytes.");
        Requireˉu32(bytes, 0x80, 0x0000_4550, "WVU2003", "The PE signature is invalid.");

        Requireˉu16(bytes, 0x84, 0x8664, "WVU2003", "The image machine is not x86-64.");
        Requireˉu16(bytes, 0x86, 2, "WVU2003", "The image does not have exactly two sections.");
        Requireˉu32(bytes, 0x88, 0, "WVU2003", "The COFF timestamp must be zero.");
        Requireˉu32(bytes, 0x8C, 0, "WVU2003", "The COFF symbol-table pointer must be zero.");
        Requireˉu32(bytes, 0x90, 0, "WVU2003", "The COFF symbol count must be zero.");
        Requireˉu16(bytes, 0x94, 0xF0, "WVU2003", "The optional-header length is invalid.");
        Requireˉu16(bytes, 0x96, 0x0022, "WVU2003", "The COFF characteristics are noncanonical.");

        Requireˉu16(bytes, 0x98, 0x020B, "WVU2004", "The image is not PE32+.");
        Requireˉbyte(
            bytes,
            0x9A,
            Uefiˉapplicationˉcontract.FORMAT_VERSION,
            "WVU2004",
            "The Windvale writer version is invalid.");
        Requireˉbyte(bytes, 0x9B, 0, "WVU2004", "The Windvale writer minor version is invalid.");
        var Textˉrawˉbytes = Readˉu32(bytes, 0x9C);
        Require(Textˉrawˉbytes is >= 0x200 and <= Uefiˉapplicationˉcontract.MAX_CODE_BYTES,
            "WVU2004", 0x9C, "The code raw size is invalid.");
        Require(Textˉrawˉbytes % 0x200 == 0, "WVU2004", 0x9C, "The code raw size is not file-aligned.");
        Requireˉu32(bytes, 0xA0, 0x200, "WVU2004", "The initialized-data size is invalid.");
        Requireˉu32(bytes, 0xA4, 0, "WVU2004", "The uninitialized-data size must be zero.");
        var Entryˉrva = Readˉu32(bytes, 0xA8);
        Requireˉu32(bytes, 0xAC, 0x1000, "WVU2004", "The code base is invalid.");
        Requireˉu64(bytes, 0xB0, 0x0040_0000, "WVU2004", "The preferred image base is invalid.");
        Requireˉu32(bytes, 0xB8, 0x1000, "WVU2004", "The section alignment is invalid.");
        Requireˉu32(bytes, 0xBC, 0x200, "WVU2004", "The file alignment is invalid.");
        Requireˉzero(bytes, 0xC0, 16, "WVU2004", "Version fields must be zero.");
        var Imageˉmemoryˉbytes = Readˉu32(bytes, 0xD0);
        Requireˉu32(bytes, 0xD4, 0x200, "WVU2004", "The header size is invalid.");
        Requireˉu32(bytes, 0xD8, 0, "WVU2004", "The checksum must be zero.");
        Requireˉu16(bytes, 0xDC, 10, "WVU2004", "The subsystem is not EFI application.");
        Requireˉu16(bytes, 0xDE, 0x0140, "WVU2004", "The DLL characteristics are noncanonical.");
        Requireˉu64(bytes, 0xE0, 0x0010_0000, "WVU2004", "The stack reserve is invalid.");
        Requireˉu64(bytes, 0xE8, 0x0000_1000, "WVU2004", "The stack commit is invalid.");
        Requireˉu64(bytes, 0xF0, 0x0010_0000, "WVU2004", "The heap reserve is invalid.");
        Requireˉu64(bytes, 0xF8, 0x0000_1000, "WVU2004", "The heap commit is invalid.");
        Requireˉu32(bytes, 0x100, 0, "WVU2004", "Loader flags must be zero.");
        Requireˉu32(bytes, 0x104, 16, "WVU2004", "The data-directory count is invalid.");
        Requireˉzero(bytes, 0x108, 40, "WVU2004", "Data directories before base relocation must be zero.");
        var Relocationˉdirectoryˉrva = Readˉu32(bytes, 0x130);
        Requireˉu32(bytes, 0x134, 12, "WVU2004", "The base-relocation directory size is invalid.");
        Requireˉzero(bytes, 0x138, 80, "WVU2004", "Unused data directories must be zero.");

        Requireˉname(bytes, 0x188, ".text", "WVU2005", "The first section is not canonical .text.");
        var Codeˉbytes = Readˉu32(bytes, 0x190);
        Require(Codeˉbytes is >= 1 and <= Uefiˉapplicationˉcontract.MAX_CODE_BYTES,
            "WVU2005", 0x190, "The code virtual size is invalid.");
        Requireˉu32(bytes, 0x194, 0x1000, "WVU2005", "The code RVA is invalid.");
        Requireˉu32(bytes, 0x198, Textˉrawˉbytes, "WVU2005", "The code raw size is inconsistent.");
        Requireˉu32(bytes, 0x19C, 0x200, "WVU2005", "The code raw offset is invalid.");
        Requireˉzero(bytes, 0x1A0, 12, "WVU2005", "The code section contains object metadata.");
        Requireˉu32(bytes, 0x1AC, 0x6000_0020, "WVU2005", "The code permissions are invalid.");
        var Expectedˉtextˉraw = Alignˉup(Codeˉbytes, 0x200);
        Require(Textˉrawˉbytes == Expectedˉtextˉraw,
            "WVU2005", 0x198, "The code raw size is not canonical for its virtual size.");
        Require(Entryˉrva >= 0x1000 && Entryˉrva - 0x1000 < Codeˉbytes,
            "WVU2005", 0xA8, "The entry point is outside code.");

        Requireˉname(bytes, 0x1B0, ".reloc", "WVU2005", "The second section is not canonical .reloc.");
        Requireˉu32(bytes, 0x1B8, 12, "WVU2005", "The relocation virtual size is invalid.");
        var Expectedˉrelocationˉrva = Alignˉup(0x1000 + Codeˉbytes, 0x1000);
        Requireˉu32(bytes, 0x1BC, Expectedˉrelocationˉrva, "WVU2005", "The relocation RVA is invalid.");
        Require(Relocationˉdirectoryˉrva == Expectedˉrelocationˉrva,
            "WVU2005", 0x130, "The relocation directory does not identify .reloc.");
        Requireˉu32(bytes, 0x1C0, 0x200, "WVU2005", "The relocation raw size is invalid.");
        var Expectedˉrelocationˉraw = 0x200 + Textˉrawˉbytes;
        Requireˉu32(bytes, 0x1C4, Expectedˉrelocationˉraw, "WVU2005", "The relocation raw offset is invalid.");
        Requireˉzero(bytes, 0x1C8, 12, "WVU2005", "The relocation section contains object metadata.");
        Requireˉu32(bytes, 0x1D4, 0x4200_0040, "WVU2005", "The relocation permissions are invalid.");
        Requireˉzero(bytes, 0x1D8, 40, "WVU2007", "The header padding is not zero.");

        var Expectedˉimageˉmemory = Alignˉup(Expectedˉrelocationˉrva + 12, 0x1000);
        Require(Imageˉmemoryˉbytes == Expectedˉimageˉmemory,
            "WVU2005", 0xD0, "The in-memory image size is invalid.");
        var Expectedˉfileˉbytes = checked((int)Expectedˉrelocationˉraw + 0x200);
        Require(bytes.Length == Expectedˉfileˉbytes,
            "WVU2001", null, "The application has trailing or missing bytes.");
        Requireˉzero(
            bytes,
            checked(0x200 + (int)Codeˉbytes),
            checked((int)Textˉrawˉbytes - (int)Codeˉbytes),
            "WVU2007",
            "The code padding is not zero.");
        Requireˉu32(bytes, (int)Expectedˉrelocationˉraw, 0x1000,
            "WVU2006", "The relocation block page is invalid.");
        Requireˉu32(bytes, (int)Expectedˉrelocationˉraw + 4, 12,
            "WVU2006", "The relocation block size is invalid.");
        Requireˉu32(bytes, (int)Expectedˉrelocationˉraw + 8, 0,
            "WVU2006", "Only absolute relocation padding is permitted in version 2.");
        Requireˉzero(
            bytes,
            (int)Expectedˉrelocationˉraw + 12,
            0x200 - 12,
            "WVU2007",
            "The relocation padding is not zero.");

        return new(
            bytes.Slice(0x200, (int)Codeˉbytes).ToArray().ToImmutableArray(),
            Entryˉrva - 0x1000);
    }

    private static uint Alignˉup(uint value, uint alignment) =>
        checked((value + alignment - 1) & ~(alignment - 1));

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
        throw new Uefiˉapplicationˉexception(code, message, offset);
}
