using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Windowsˉconsoleˉapplicationˉwriter
{
    private const int PE_OFFSET = 0x80;
    private const int OPTIONAL_HEADER_OFFSET = 0x98;
    private const int OPTIONAL_HEADER_BYTES = 0xF0;
    private const int SECTION_TABLE_OFFSET = OPTIONAL_HEADER_OFFSET + OPTIONAL_HEADER_BYTES;
    private const int HEADERS_BYTES = 0x200;
    private const uint FILE_ALIGNMENT = 0x200;
    private const uint SECTION_ALIGNMENT = 0x1000;
    private const uint TEXT_RVA = 0x1000;
    private const ulong IMAGE_BASE = 0x0000_0001_4000_0000;
    private const uint DATA_RAW_BYTES = 0x200;
    private const uint RELOCATION_DATA_BYTES = 12;
    private const uint RELOCATION_RAW_BYTES = 0x200;

    public static Windowsˉconsoleˉapplicationˉresult Write(Nativeˉfragment fragment)
    {
        var Prepared = Nativeˉconsoleˉapplicationˉpreparer.Prepare(fragment);
        if (!Prepared.Success)
        {
            var Code = Prepared.Failure switch
            {
                Nativeˉconsoleˉapplicationˉinputˉfailure.Unverifiedˉfragment => "WVW1001",
                Nativeˉconsoleˉapplicationˉinputˉfailure.Unsupportedˉentry => "WVW1002",
                Nativeˉconsoleˉapplicationˉinputˉfailure.Linkˉfailure => "WVW1003",
                _ => "WVW1003",
            };
            return Windowsˉconsoleˉapplicationˉresult.Failed(
                Code,
                Prepared.Message);
        }

        var Input = Prepared.Input!;
        var Image = Buildˉimage(Input.Imageˉbytes.AsSpan(), Input.Entryˉoffset);
        try
        {
            var Verified = Windowsˉconsoleˉapplicationˉverifier.Verify(Image.AsSpan());
            if (Verified.Nativeˉentryˉoffset != Input.Entryˉoffset ||
                !Verified.Nativeˉimageˉbytes.AsSpan().SequenceEqual(Input.Imageˉbytes.AsSpan()))
            {
                return Windowsˉconsoleˉapplicationˉresult.Failed(
                    "WVW1004",
                    "The independently verified Windows application did not reproduce the native image and entry.");
            }
        }
        catch (Windowsˉconsoleˉapplicationˉexception Exception)
        {
            return Windowsˉconsoleˉapplicationˉresult.Failed(
                "WVW1004",
                $"Independent Windows application verification failed: {Exception.Message}");
        }

        return Windowsˉconsoleˉapplicationˉresult.Succeeded(Image.ToImmutableArray());
    }

    private static byte[] Buildˉimage(ReadOnlySpan<byte> nativeˉimage, uint nativeˉentryˉoffset)
    {
        var Textˉbytes = checked(
            (uint)Windowsˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET +
            (uint)nativeˉimage.Length);
        var Textˉrawˉbytes = Alignˉup(Textˉbytes, FILE_ALIGNMENT);
        var Dataˉrva = Alignˉup(TEXT_RVA + Textˉbytes, SECTION_ALIGNMENT);
        var Relocationˉrva = Alignˉup(
            Dataˉrva + Windowsˉconsoleˉapplicationˉcontract.DATA_VIRTUAL_BYTES,
            SECTION_ALIGNMENT);
        var Dataˉrawˉoffset = (uint)HEADERS_BYTES + Textˉrawˉbytes;
        var Relocationˉrawˉoffset = Dataˉrawˉoffset + DATA_RAW_BYTES;
        var Imageˉbytes = Relocationˉrawˉoffset + RELOCATION_RAW_BYTES;
        var Imageˉmemoryˉbytes = Alignˉup(
            Relocationˉrva + RELOCATION_DATA_BYTES,
            SECTION_ALIGNMENT);
        var Result = new byte[Imageˉbytes];

        Writeˉu16(Result, 0x00, 0x5A4D);
        Writeˉu32(Result, 0x3C, PE_OFFSET);
        Writeˉu32(Result, PE_OFFSET, 0x0000_4550);

        var Coff = PE_OFFSET + sizeof(uint);
        Writeˉu16(Result, Coff + 0, 0x8664);
        Writeˉu16(Result, Coff + 2, 3);
        Writeˉu16(Result, Coff + 16, OPTIONAL_HEADER_BYTES);
        Writeˉu16(Result, Coff + 18, 0x0022);

        var Optional = OPTIONAL_HEADER_OFFSET;
        Writeˉu16(Result, Optional + 0, 0x020B);
        Result[Optional + 2] = Windowsˉconsoleˉapplicationˉcontract.FORMAT_VERSION;
        Writeˉu32(Result, Optional + 4, Textˉrawˉbytes);
        Writeˉu32(Result, Optional + 8, DATA_RAW_BYTES + RELOCATION_RAW_BYTES);
        Writeˉu32(
            Result,
            Optional + 12,
            Windowsˉconsoleˉapplicationˉcontract.DATA_VIRTUAL_BYTES - DATA_RAW_BYTES);
        Writeˉu32(Result, Optional + 16, TEXT_RVA);
        Writeˉu32(Result, Optional + 20, TEXT_RVA);
        Writeˉu64(Result, Optional + 24, IMAGE_BASE);
        Writeˉu32(Result, Optional + 32, SECTION_ALIGNMENT);
        Writeˉu32(Result, Optional + 36, FILE_ALIGNMENT);
        Writeˉu16(Result, Optional + 40, 6);
        Writeˉu16(Result, Optional + 48, 6);
        Writeˉu32(Result, Optional + 56, Imageˉmemoryˉbytes);
        Writeˉu32(Result, Optional + 60, HEADERS_BYTES);
        Writeˉu16(Result, Optional + 68, 3);
        Writeˉu16(Result, Optional + 70, 0x0160);
        Writeˉu64(Result, Optional + 72, Windowsˉconsoleˉapplicationˉcontract.STACK_BYTES);
        Writeˉu64(Result, Optional + 80, 0x0001_0000);
        Writeˉu64(Result, Optional + 88, 0x0010_0000);
        Writeˉu64(Result, Optional + 96, 0x0000_1000);
        Writeˉu32(Result, Optional + 108, 16);
        Writeˉu32(Result, Optional + 112 + (5 * 8), Relocationˉrva);
        Writeˉu32(Result, Optional + 112 + (5 * 8) + 4, RELOCATION_DATA_BYTES);

        Writeˉsectionˉname(Result, SECTION_TABLE_OFFSET, ".text");
        Writeˉu32(Result, SECTION_TABLE_OFFSET + 8, Textˉbytes);
        Writeˉu32(Result, SECTION_TABLE_OFFSET + 12, TEXT_RVA);
        Writeˉu32(Result, SECTION_TABLE_OFFSET + 16, Textˉrawˉbytes);
        Writeˉu32(Result, SECTION_TABLE_OFFSET + 20, HEADERS_BYTES);
        Writeˉu32(Result, SECTION_TABLE_OFFSET + 36, 0x6000_0020);

        var Dataˉsection = SECTION_TABLE_OFFSET + 40;
        Writeˉsectionˉname(Result, Dataˉsection, ".data");
        Writeˉu32(
            Result,
            Dataˉsection + 8,
            Windowsˉconsoleˉapplicationˉcontract.DATA_VIRTUAL_BYTES);
        Writeˉu32(Result, Dataˉsection + 12, Dataˉrva);
        Writeˉu32(Result, Dataˉsection + 16, DATA_RAW_BYTES);
        Writeˉu32(Result, Dataˉsection + 20, Dataˉrawˉoffset);
        Writeˉu32(Result, Dataˉsection + 36, 0xC000_0040);

        var Relocationˉsection = SECTION_TABLE_OFFSET + 80;
        Writeˉsectionˉname(Result, Relocationˉsection, ".reloc");
        Writeˉu32(Result, Relocationˉsection + 8, RELOCATION_DATA_BYTES);
        Writeˉu32(Result, Relocationˉsection + 12, Relocationˉrva);
        Writeˉu32(Result, Relocationˉsection + 16, RELOCATION_RAW_BYTES);
        Writeˉu32(Result, Relocationˉsection + 20, Relocationˉrawˉoffset);
        Writeˉu32(Result, Relocationˉsection + 36, 0x4200_0040);

        Writeˉstartup(
            Result.AsSpan(HEADERS_BYTES, Windowsˉconsoleˉapplicationˉcontract.STARTUP_BYTES),
            Dataˉrva,
            nativeˉentryˉoffset);
        nativeˉimage.CopyTo(Result.AsSpan(
            HEADERS_BYTES + Windowsˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET));

        var Contextˉoffset = checked((int)Dataˉrawˉoffset);
        Writeˉu32(Result, Contextˉoffset + 0, Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION);
        Writeˉu32(Result, Contextˉoffset + 4, Nativeˉexecutionˉcontextˉcontract.SIZE);
        Writeˉu64(Result, Contextˉoffset + 8, checked((ulong)Nativeˉcontract.DEFAULT_MAXIMUM_INSTRUCTIONS));
        Writeˉu64(Result, Contextˉoffset + 16, checked((ulong)Nativeˉcontract.DEFAULT_MAXIMUM_CALL_DEPTH));
        Writeˉu32(
            Result,
            Contextˉoffset + Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET,
            Windowsˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES);
        Writeˉu32(
            Result,
            Contextˉoffset + Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
            Windowsˉconsoleˉapplicationˉcontract.TEXT_ARENA_BYTES);

        Writeˉu32(Result, (int)Relocationˉrawˉoffset, TEXT_RVA);
        Writeˉu32(Result, (int)Relocationˉrawˉoffset + 4, RELOCATION_DATA_BYTES);
        return Result;
    }

    private static void Writeˉstartup(
        Span<byte> output,
        uint dataˉrva,
        uint nativeˉentryˉoffset)
    {
        ReadOnlySpan<byte> Template =
        [
            0x48, 0x81, 0xEC, 0x28, 0x00, 0x00, 0x00,
            0x48, 0x8D, 0x15, 0x00, 0x00, 0x00, 0x00,
            0x48, 0x8D, 0x05, 0x00, 0x00, 0x00, 0x00,
            0x48, 0x89, 0x84, 0x22, 0x20, 0x00, 0x00, 0x00,
            0x48, 0x8D, 0x05, 0x00, 0x00, 0x00, 0x00,
            0x48, 0x89, 0x84, 0x22, 0x30, 0x00, 0x00, 0x00,
            0x48, 0x31, 0xC9,
            0x49, 0x89, 0xD0,
            0x4D, 0x31, 0xC9,
            0xE8, 0x00, 0x00, 0x00, 0x00,
            0x48, 0x89, 0xC2,
            0x48, 0xC1, 0xEA, 0x20,
            0x85, 0xD2,
            0x0F, 0x85, 0x0C, 0x00, 0x00, 0x00,
            0x81, 0xF8, 0xFF, 0x00, 0x00, 0x00,
            0x0F, 0x86, 0x05, 0x00, 0x00, 0x00,
            0xB8, 0x01, 0x00, 0x00, 0x00,
            0x48, 0x81, 0xC4, 0x28, 0x00, 0x00, 0x00,
            0xC3,
        ];
        Template.CopyTo(output);
        Writeˉi32(
            output,
            10,
            Relativeˉi32(TEXT_RVA + 14, dataˉrva));
        Writeˉi32(
            output,
            17,
            Relativeˉi32(
                TEXT_RVA + 21,
                dataˉrva + Nativeˉexecutionˉcontextˉcontract.SIZE));
        Writeˉi32(
            output,
            32,
            Relativeˉi32(
                TEXT_RVA + 36,
                dataˉrva + Nativeˉexecutionˉcontextˉcontract.SIZE +
                    Windowsˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES));
        Writeˉi32(
            output,
            54,
            Relativeˉi32(
                TEXT_RVA + 58,
                TEXT_RVA + Windowsˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET +
                    nativeˉentryˉoffset));
    }

    private static int Relativeˉi32(uint sourceˉend, uint target) =>
        checked((int)((long)target - sourceˉend));

    private static uint Alignˉup(uint value, uint alignment) =>
        checked((value + alignment - 1) & ~(alignment - 1));

    private static void Writeˉsectionˉname(byte[] output, int offset, string value)
    {
        for (var Index = 0; Index < value.Length; Index++)
        {
            output[offset + Index] = (byte)value[Index];
        }
    }

    private static void Writeˉi32(Span<byte> output, int offset, int value) =>
        BinaryPrimitives.WriteInt32LittleEndian(output.Slice(offset, sizeof(int)), value);

    private static void Writeˉu16(byte[] output, int offset, ushort value) =>
        BinaryPrimitives.WriteUInt16LittleEndian(output.AsSpan(offset, sizeof(ushort)), value);

    private static void Writeˉu32(byte[] output, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(offset, sizeof(uint)), value);

    private static void Writeˉu64(byte[] output, int offset, ulong value) =>
        BinaryPrimitives.WriteUInt64LittleEndian(output.AsSpan(offset, sizeof(ulong)), value);
}
