using System.Buffers.Binary;
using System.Collections.Immutable;

namespace Windvale.Linker;

public static class Uefiˉapplicationˉwriter
{
    private const int PE_OFFSET = 0x80;
    private const int OPTIONAL_HEADER_OFFSET = 0x98;
    private const int OPTIONAL_HEADER_BYTES = 0xF0;
    private const int SECTION_TABLE_OFFSET = OPTIONAL_HEADER_OFFSET + OPTIONAL_HEADER_BYTES;
    private const int HEADERS_BYTES = 0x200;
    private const uint FILE_ALIGNMENT = 0x200;
    private const uint SECTION_ALIGNMENT = 0x1000;
    private const uint TEXT_RVA = 0x1000;
    private const uint IMAGE_BASE = 0x0040_0000;
    private const uint RELOCATION_DATA_BYTES = 12;
    private const uint RELOCATION_RAW_BYTES = 0x200;

    public static Uefiˉapplicationˉresult Write(Linkˉresult linkˉresult)
    {
        if (linkˉresult is null || !linkˉresult.Success)
        {
            return Uefiˉapplicationˉresult.Failed(
                "WVU1001",
                "A UEFI application requires a successful verified flat-image link result.");
        }
        if (
            linkˉresult.Baseˉaddress != Uefiˉapplicationˉcontract.REQUIRED_LINK_BASE_ADDRESS ||
            linkˉresult.Sectionˉcount != 1 ||
            linkˉresult.Importˉcount != 0 ||
            linkˉresult.Relocationˉcount != 0 ||
            linkˉresult.Imageˉbytes.IsDefaultOrEmpty ||
            linkˉresult.Entryˉaddress >= (uint)linkˉresult.Imageˉbytes.Length)
        {
            return Uefiˉapplicationˉresult.Failed(
                "WVU1002",
                "Version 1 requires one non-empty base-zero code section, no imports or relocations, and an entry inside the section.");
        }
        if (linkˉresult.Imageˉbytes.Length > Uefiˉapplicationˉcontract.MAX_CODE_BYTES)
        {
            return Uefiˉapplicationˉresult.Failed(
                "WVU1003",
                $"The code exceeds the {Uefiˉapplicationˉcontract.MAX_CODE_BYTES}-byte UEFI application limit.");
        }

        var Image = Buildˉimage(linkˉresult.Imageˉbytes.AsSpan(), linkˉresult.Entryˉaddress);
        try
        {
            var Verified = Uefiˉapplicationˉverifier.Verify(Image.AsSpan());
            if (
                Verified.Entryˉcodeˉoffset != linkˉresult.Entryˉaddress ||
                !Verified.Codeˉbytes.AsSpan().SequenceEqual(linkˉresult.Imageˉbytes.AsSpan()))
            {
                return Uefiˉapplicationˉresult.Failed(
                    "WVU1004",
                    "The independently verified UEFI application does not reproduce the linked code and entry.");
            }
        }
        catch (Uefiˉapplicationˉexception Exception)
        {
            return Uefiˉapplicationˉresult.Failed(
                "WVU1004",
                $"Independent UEFI application verification failed: {Exception.Message}");
        }

        return Uefiˉapplicationˉresult.Succeeded(Image.ToImmutableArray());
    }

    private static byte[] Buildˉimage(ReadOnlySpan<byte> code, uint entryˉoffset)
    {
        var Textˉrawˉbytes = Alignˉup((uint)code.Length, FILE_ALIGNMENT);
        var Relocationˉrva = Alignˉup(TEXT_RVA + (uint)code.Length, SECTION_ALIGNMENT);
        var Relocationˉrawˉoffset = (uint)HEADERS_BYTES + Textˉrawˉbytes;
        var Imageˉbytes = Relocationˉrawˉoffset + RELOCATION_RAW_BYTES;
        var Imageˉmemoryˉbytes = Alignˉup(Relocationˉrva + RELOCATION_DATA_BYTES, SECTION_ALIGNMENT);
        var Result = new byte[Imageˉbytes];

        Writeˉu16(Result, 0x00, 0x5A4D);
        Writeˉu32(Result, 0x3C, PE_OFFSET);
        Writeˉu32(Result, PE_OFFSET, 0x0000_4550);

        var Coff = PE_OFFSET + sizeof(uint);
        Writeˉu16(Result, Coff + 0, 0x8664);
        Writeˉu16(Result, Coff + 2, 2);
        Writeˉu16(Result, Coff + 16, OPTIONAL_HEADER_BYTES);
        Writeˉu16(Result, Coff + 18, 0x0022);

        var Optional = OPTIONAL_HEADER_OFFSET;
        Writeˉu16(Result, Optional + 0, 0x020B);
        Result[Optional + 2] = Uefiˉapplicationˉcontract.FORMAT_VERSION;
        Writeˉu32(Result, Optional + 4, Textˉrawˉbytes);
        Writeˉu32(Result, Optional + 8, RELOCATION_RAW_BYTES);
        Writeˉu32(Result, Optional + 16, TEXT_RVA + entryˉoffset);
        Writeˉu32(Result, Optional + 20, TEXT_RVA);
        Writeˉu64(Result, Optional + 24, IMAGE_BASE);
        Writeˉu32(Result, Optional + 32, SECTION_ALIGNMENT);
        Writeˉu32(Result, Optional + 36, FILE_ALIGNMENT);
        Writeˉu32(Result, Optional + 56, Imageˉmemoryˉbytes);
        Writeˉu32(Result, Optional + 60, HEADERS_BYTES);
        Writeˉu16(Result, Optional + 68, 10);
        Writeˉu16(Result, Optional + 70, 0x0140);
        Writeˉu64(Result, Optional + 72, 0x0010_0000);
        Writeˉu64(Result, Optional + 80, 0x0000_1000);
        Writeˉu64(Result, Optional + 88, 0x0010_0000);
        Writeˉu64(Result, Optional + 96, 0x0000_1000);
        Writeˉu32(Result, Optional + 108, 16);
        Writeˉu32(Result, Optional + 112 + (5 * 8), Relocationˉrva);
        Writeˉu32(Result, Optional + 112 + (5 * 8) + 4, RELOCATION_DATA_BYTES);

        Writeˉsectionˉname(Result, SECTION_TABLE_OFFSET, ".text");
        Writeˉu32(Result, SECTION_TABLE_OFFSET + 8, (uint)code.Length);
        Writeˉu32(Result, SECTION_TABLE_OFFSET + 12, TEXT_RVA);
        Writeˉu32(Result, SECTION_TABLE_OFFSET + 16, Textˉrawˉbytes);
        Writeˉu32(Result, SECTION_TABLE_OFFSET + 20, HEADERS_BYTES);
        Writeˉu32(Result, SECTION_TABLE_OFFSET + 36, 0x6000_0020);

        var Relocationˉsection = SECTION_TABLE_OFFSET + 40;
        Writeˉsectionˉname(Result, Relocationˉsection, ".reloc");
        Writeˉu32(Result, Relocationˉsection + 8, RELOCATION_DATA_BYTES);
        Writeˉu32(Result, Relocationˉsection + 12, Relocationˉrva);
        Writeˉu32(Result, Relocationˉsection + 16, RELOCATION_RAW_BYTES);
        Writeˉu32(Result, Relocationˉsection + 20, Relocationˉrawˉoffset);
        Writeˉu32(Result, Relocationˉsection + 36, 0x4200_0040);

        code.CopyTo(Result.AsSpan(HEADERS_BYTES));
        Writeˉu32(Result, (int)Relocationˉrawˉoffset, TEXT_RVA);
        Writeˉu32(Result, (int)Relocationˉrawˉoffset + 4, RELOCATION_DATA_BYTES);
        return Result;
    }

    private static uint Alignˉup(uint value, uint alignment) =>
        checked((value + alignment - 1) & ~(alignment - 1));

    private static void Writeˉsectionˉname(byte[] output, int offset, string value)
    {
        for (var Index = 0; Index < value.Length; Index++)
        {
            output[offset + Index] = (byte)value[Index];
        }
    }

    private static void Writeˉu16(byte[] output, int offset, ushort value) =>
        BinaryPrimitives.WriteUInt16LittleEndian(output.AsSpan(offset, sizeof(ushort)), value);

    private static void Writeˉu32(byte[] output, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(offset, sizeof(uint)), value);

    private static void Writeˉu64(byte[] output, int offset, ulong value) =>
        BinaryPrimitives.WriteUInt64LittleEndian(output.AsSpan(offset, sizeof(ulong)), value);
}
