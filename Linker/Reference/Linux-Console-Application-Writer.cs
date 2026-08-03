using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Linuxˉconsoleˉapplicationˉwriter
{
    private const int ELF_HEADER_BYTES = 64;
    private const int PROGRAM_HEADER_BYTES = 56;
    private const int PROGRAM_HEADER_COUNT = 5;
    private const int NOTE_OFFSET = 0x180;
    private const int NOTE_BYTES = 28;
    private const uint PAGE_BYTES = 0x1000;
    private const uint CONTEXT_FILE_BYTES = Nativeˉexecutionˉcontextˉcontract.SIZE;

    public static Linuxˉconsoleˉapplicationˉresult Write(Nativeˉfragment fragment)
    {
        var Prepared = Nativeˉconsoleˉapplicationˉpreparer.Prepare(fragment);
        if (!Prepared.Success)
        {
            var Code = Prepared.Failure switch
            {
                Nativeˉconsoleˉapplicationˉinputˉfailure.Unverifiedˉfragment => "WVL1001",
                Nativeˉconsoleˉapplicationˉinputˉfailure.Unsupportedˉentry => "WVL1002",
                Nativeˉconsoleˉapplicationˉinputˉfailure.Linkˉfailure => "WVL1003",
                _ => "WVL1003",
            };
            return Linuxˉconsoleˉapplicationˉresult.Failed(Code, Prepared.Message);
        }

        var Input = Prepared.Input!;
        var Image = Buildˉimage(Input.Imageˉbytes.AsSpan(), Input.Entryˉoffset);
        try
        {
            var Verified = Linuxˉconsoleˉapplicationˉverifier.Verify(Image.AsSpan());
            if (Verified.Nativeˉentryˉoffset != Input.Entryˉoffset ||
                !Verified.Nativeˉimageˉbytes.AsSpan().SequenceEqual(Input.Imageˉbytes.AsSpan()))
            {
                return Linuxˉconsoleˉapplicationˉresult.Failed(
                    "WVL1004",
                    "The independently verified Linux application did not reproduce the native image and entry.");
            }
        }
        catch (Linuxˉconsoleˉapplicationˉexception Exception)
        {
            return Linuxˉconsoleˉapplicationˉresult.Failed(
                "WVL1004",
                $"Independent Linux application verification failed: {Exception.Message}");
        }

        return Linuxˉconsoleˉapplicationˉresult.Succeeded(Image.ToImmutableArray());
    }

    private static byte[] Buildˉimage(ReadOnlySpan<byte> nativeˉimage, uint nativeˉentryˉoffset)
    {
        var Textˉbytes = checked(
            (uint)Linuxˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET +
            (uint)nativeˉimage.Length);
        var Dataˉoffset = Alignˉup(
            Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS + Textˉbytes,
            PAGE_BYTES);
        var Fileˉbytes = checked(Dataˉoffset + CONTEXT_FILE_BYTES);
        var Result = new byte[checked((int)Fileˉbytes)];

        ReadOnlySpan<byte> Identification =
        [
            0x7F, 0x45, 0x4C, 0x46,
            0x02,
            0x01,
            0x01,
            0x00,
            0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        ];
        Identification.CopyTo(Result);
        Writeˉu16(Result, 16, 3);
        Writeˉu16(Result, 18, 62);
        Writeˉu32(Result, 20, 1);
        Writeˉu64(Result, 24, Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS);
        Writeˉu64(Result, 32, ELF_HEADER_BYTES);
        Writeˉu16(Result, 52, ELF_HEADER_BYTES);
        Writeˉu16(Result, 54, PROGRAM_HEADER_BYTES);
        Writeˉu16(Result, 56, PROGRAM_HEADER_COUNT);

        var Headerˉload = ELF_HEADER_BYTES;
        Writeˉprogramˉheader(
            Result,
            Headerˉload,
            type: 1,
            flags: 4,
            fileˉoffset: 0,
            virtualˉaddress: 0,
            fileˉbytes: Linuxˉconsoleˉapplicationˉcontract.HEADER_BYTES,
            memoryˉbytes: Linuxˉconsoleˉapplicationˉcontract.HEADER_BYTES,
            alignment: PAGE_BYTES);

        var Textˉload = Headerˉload + PROGRAM_HEADER_BYTES;
        Writeˉprogramˉheader(
            Result,
            Textˉload,
            type: 1,
            flags: 5,
            fileˉoffset: Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS,
            virtualˉaddress: Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS,
            fileˉbytes: Textˉbytes,
            memoryˉbytes: Textˉbytes,
            alignment: PAGE_BYTES);

        var Dataˉload = Textˉload + PROGRAM_HEADER_BYTES;
        Writeˉprogramˉheader(
            Result,
            Dataˉload,
            type: 1,
            flags: 6,
            fileˉoffset: Dataˉoffset,
            virtualˉaddress: Dataˉoffset,
            fileˉbytes: CONTEXT_FILE_BYTES,
            memoryˉbytes: Linuxˉconsoleˉapplicationˉcontract.DATA_VIRTUAL_BYTES,
            alignment: PAGE_BYTES);

        var Note = Dataˉload + PROGRAM_HEADER_BYTES;
        Writeˉprogramˉheader(
            Result,
            Note,
            type: 4,
            flags: 4,
            fileˉoffset: NOTE_OFFSET,
            virtualˉaddress: NOTE_OFFSET,
            fileˉbytes: NOTE_BYTES,
            memoryˉbytes: NOTE_BYTES,
            alignment: 4);

        var Stack = Note + PROGRAM_HEADER_BYTES;
        Writeˉprogramˉheader(
            Result,
            Stack,
            type: 0x6474_E551,
            flags: 6,
            fileˉoffset: 0,
            virtualˉaddress: 0,
            fileˉbytes: 0,
            memoryˉbytes: Linuxˉconsoleˉapplicationˉcontract.STACK_BYTES,
            alignment: 16);

        Writeˉu32(Result, NOTE_OFFSET + 0, 9);
        Writeˉu32(Result, NOTE_OFFSET + 4, sizeof(uint));
        Writeˉu32(Result, NOTE_OFFSET + 8, 1);
        ReadOnlySpan<byte> Noteˉname = [0x57, 0x69, 0x6E, 0x64, 0x76, 0x61, 0x6C, 0x65, 0x00];
        Noteˉname.CopyTo(Result.AsSpan(NOTE_OFFSET + 12));
        Writeˉu32(
            Result,
            NOTE_OFFSET + 24,
            Linuxˉconsoleˉapplicationˉcontract.FORMAT_VERSION);

        Writeˉstartup(
            Result.AsSpan(
                checked((int)Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS),
                Linuxˉconsoleˉapplicationˉcontract.STARTUP_BYTES),
            Dataˉoffset,
            nativeˉentryˉoffset);
        nativeˉimage.CopyTo(Result.AsSpan(
            checked(
                (int)Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS +
                Linuxˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET)));

        var Contextˉoffset = checked((int)Dataˉoffset);
        Writeˉu32(Result, Contextˉoffset + 0, Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION);
        Writeˉu32(Result, Contextˉoffset + 4, Nativeˉexecutionˉcontextˉcontract.SIZE);
        Writeˉu64(Result, Contextˉoffset + 8, checked((ulong)Nativeˉcontract.DEFAULT_MAXIMUM_INSTRUCTIONS));
        Writeˉu64(Result, Contextˉoffset + 16, checked((ulong)Nativeˉcontract.DEFAULT_MAXIMUM_CALL_DEPTH));
        Writeˉu32(
            Result,
            Contextˉoffset + Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET,
            Linuxˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES);
        Writeˉu32(
            Result,
            Contextˉoffset + Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
            Linuxˉconsoleˉapplicationˉcontract.TEXT_ARENA_BYTES);
        return Result;
    }

    private static void Writeˉstartup(
        Span<byte> output,
        uint dataˉaddress,
        uint nativeˉentryˉoffset)
    {
        ReadOnlySpan<byte> Stackˉprefix =
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
            0x73, 0x41,
            0x48, 0x8D, 0xA0, 0x00, 0x00, 0x00, 0x04,
            0x48, 0x8D, 0x15,
        ];
        Stackˉprefix.CopyTo(output);
        Writeˉi32(
            output,
            53,
            Relativeˉi32(
                Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS + 57,
                dataˉaddress));

        ReadOnlySpan<byte> Recordˉprefix = [0x48, 0x89, 0xD6, 0x48, 0x8D, 0x05];
        Recordˉprefix.CopyTo(output[57..]);
        Writeˉi32(
            output,
            63,
            Relativeˉi32(
                Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS + 67,
                dataˉaddress + Nativeˉexecutionˉcontextˉcontract.SIZE));

        ReadOnlySpan<byte> Recordˉstoreˉandˉtextˉprefix =
            [0x48, 0x89, 0x42, 0x20, 0x48, 0x8D, 0x05];
        Recordˉstoreˉandˉtextˉprefix.CopyTo(output[67..]);
        Writeˉi32(
            output,
            74,
            Relativeˉi32(
                Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS + 78,
                dataˉaddress + Nativeˉexecutionˉcontextˉcontract.SIZE +
                    Linuxˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES));

        ReadOnlySpan<byte> Callˉprefix =
        [
            0x48, 0x89, 0x42, 0x30,
            0x31, 0xFF,
            0x31, 0xC9,
            0x45, 0x31, 0xC0,
            0x45, 0x31, 0xC9,
            0xE8,
        ];
        Callˉprefix.CopyTo(output[78..]);
        Writeˉi32(
            output,
            93,
            Relativeˉi32(
                Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS + 97,
                Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS +
                    Linuxˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET +
                    nativeˉentryˉoffset));

        ReadOnlySpan<byte> Exitˉsuffix =
        [
            0x48, 0x89, 0xC2,
            0x48, 0xC1, 0xEA, 0x20,
            0x85, 0xD2,
            0x74, 0x05,
            0xB8, 0x01, 0x00, 0x00, 0x00,
            0x89, 0xC7,
            0xB8, 0x3C, 0x00, 0x00, 0x00,
            0x0F, 0x05,
            0x0F, 0x0B,
        ];
        Exitˉsuffix.CopyTo(output[97..]);
    }

    private static int Relativeˉi32(uint sourceˉend, uint target) =>
        checked((int)((long)target - sourceˉend));

    private static uint Alignˉup(uint value, uint alignment) =>
        checked((value + alignment - 1) & ~(alignment - 1));

    private static void Writeˉprogramˉheader(
        byte[] output,
        int offset,
        uint type,
        uint flags,
        ulong fileˉoffset,
        ulong virtualˉaddress,
        ulong fileˉbytes,
        ulong memoryˉbytes,
        ulong alignment)
    {
        Writeˉu32(output, offset + 0, type);
        Writeˉu32(output, offset + 4, flags);
        Writeˉu64(output, offset + 8, fileˉoffset);
        Writeˉu64(output, offset + 16, virtualˉaddress);
        Writeˉu64(output, offset + 32, fileˉbytes);
        Writeˉu64(output, offset + 40, memoryˉbytes);
        Writeˉu64(output, offset + 48, alignment);
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
