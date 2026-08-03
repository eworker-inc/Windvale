using System.Buffers.Binary;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal sealed record Hostedˉconsoleˉapplicationˉlayout(
    Consoleˉapplicationˉtarget Target,
    int Applicationˉbytes,
    int Headerˉbytes,
    int Textˉfileˉoffset,
    uint Textˉaddress,
    int Startupˉbytes,
    int Outputˉserviceˉoffset,
    int Nativeˉimageˉoffset,
    int Nativeˉimageˉbytes,
    uint Nativeˉentryˉoffset,
    uint Textˉvirtualˉbytes,
    uint Textˉfileˉbytes,
    uint Dataˉfileˉoffset,
    uint Dataˉaddress,
    uint Dataˉfileˉbytes,
    uint Dataˉvirtualˉbytes,
    uint Metadataˉfileˉoffset,
    uint Metadataˉaddress,
    uint Imageˉvirtualˉbytes);

internal static class Hostedˉconsoleˉapplicationˉbuilder
{
    private const int PE_OFFSET = 0x80;
    private const int OPTIONAL_HEADER_OFFSET = 0x98;
    private const int OPTIONAL_HEADER_BYTES = 0xF0;
    private const int SECTION_TABLE_OFFSET = 0x188;
    private const int WINDOWS_HEADER_BYTES = 0x200;
    private const uint WINDOWS_FILE_ALIGNMENT = 0x200;
    private const uint PAGE_BYTES = 0x1000;
    private const uint TEXT_ADDRESS = 0x1000;
    private const ulong WINDOWS_IMAGE_BASE = 0x0000_0001_4000_0000;
    private const uint WINDOWS_RELOCATION_BYTES = 12;
    private const uint WINDOWS_RELOCATION_RAW_BYTES = 0x200;
    private const int ELF_HEADER_BYTES = 64;
    private const int ELF_PROGRAM_HEADER_BYTES = 56;
    private const int ELF_PROGRAM_HEADER_COUNT = 5;
    private const int LINUX_NOTE_OFFSET = 0x180;
    private const int LINUX_NOTE_BYTES = 28;

    private const uint WINDOWS_IMPORT_DIRECTORY_OFFSET = 464;
    private const uint WINDOWS_IMPORT_LOOKUP_OFFSET = 504;
    private const uint WINDOWS_IMPORT_ADDRESS_OFFSET = 528;
    private const uint WINDOWS_GET_STD_HANDLE_NAME_OFFSET = 552;
    private const uint WINDOWS_WRITE_FILE_NAME_OFFSET = 568;
    private const uint WINDOWS_LIBRARY_NAME_OFFSET = 580;

    internal static Hostedˉconsoleˉapplicationˉlayout Plan(
        Consoleˉapplicationˉtarget target,
        int nativeˉimageˉbytes,
        uint nativeˉentryˉoffset)
    {
        if (!Enum.IsDefined(target) ||
            nativeˉimageˉbytes <= 0 ||
            nativeˉimageˉbytes > Consoleˉapplicationˉlayout.MAXIMUM_NATIVE_IMAGE_BYTES ||
            nativeˉentryˉoffset >= nativeˉimageˉbytes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nativeˉimageˉbytes),
                "The hosted console native image or entry exceeds the bounded v2 layout.");
        }

        if (target == Consoleˉapplicationˉtarget.Windowsˉx64)
        {
            var Textˉvirtual = checked((uint)(
                Windowsˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET +
                nativeˉimageˉbytes));
            var Textˉraw = Alignˉup(Textˉvirtual, WINDOWS_FILE_ALIGNMENT);
            var Dataˉfile = checked((uint)WINDOWS_HEADER_BYTES + Textˉraw);
            var Dataˉaddress = Alignˉup(TEXT_ADDRESS + Textˉvirtual, PAGE_BYTES);
            var Relocationˉfile = checked(Dataˉfile +
                Windowsˉconsoleˉapplicationˉcontract.HOSTED_DATA_FILE_BYTES);
            var Relocationˉaddress = Alignˉup(
                Dataˉaddress + Windowsˉconsoleˉapplicationˉcontract.HOSTED_DATA_VIRTUAL_BYTES,
                PAGE_BYTES);
            return new(
                target,
                checked((int)(Relocationˉfile + WINDOWS_RELOCATION_RAW_BYTES)),
                WINDOWS_HEADER_BYTES,
                WINDOWS_HEADER_BYTES,
                TEXT_ADDRESS,
                Windowsˉconsoleˉapplicationˉcontract.HOSTED_STARTUP_BYTES,
                Windowsˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_SERVICE_OFFSET,
                Windowsˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET,
                nativeˉimageˉbytes,
                nativeˉentryˉoffset,
                Textˉvirtual,
                Textˉraw,
                Dataˉfile,
                Dataˉaddress,
                Windowsˉconsoleˉapplicationˉcontract.HOSTED_DATA_FILE_BYTES,
                Windowsˉconsoleˉapplicationˉcontract.HOSTED_DATA_VIRTUAL_BYTES,
                Relocationˉfile,
                Relocationˉaddress,
                Alignˉup(Relocationˉaddress + WINDOWS_RELOCATION_BYTES, PAGE_BYTES));
        }

        var Linuxˉtextˉbytes = checked((uint)(
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET +
            nativeˉimageˉbytes));
        var Linuxˉdataˉfile = Alignˉup(
            Linuxˉconsoleˉapplicationˉcontract.HEADER_BYTES + Linuxˉtextˉbytes,
            PAGE_BYTES);
        return new(
            target,
            checked((int)(Linuxˉdataˉfile +
                Linuxˉconsoleˉapplicationˉcontract.HOSTED_DATA_FILE_BYTES)),
            checked((int)Linuxˉconsoleˉapplicationˉcontract.HEADER_BYTES),
            checked((int)Linuxˉconsoleˉapplicationˉcontract.HEADER_BYTES),
            TEXT_ADDRESS,
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_STARTUP_BYTES,
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_SERVICE_OFFSET,
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET,
            nativeˉimageˉbytes,
            nativeˉentryˉoffset,
            Linuxˉtextˉbytes,
            Linuxˉtextˉbytes,
            Linuxˉdataˉfile,
            Linuxˉdataˉfile,
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_DATA_FILE_BYTES,
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_DATA_VIRTUAL_BYTES,
            LINUX_NOTE_OFFSET,
            LINUX_NOTE_OFFSET,
            checked(Linuxˉdataˉfile + Linuxˉconsoleˉapplicationˉcontract.HOSTED_DATA_VIRTUAL_BYTES));
    }

    internal static byte[] Buildˉwindows(
        ReadOnlySpan<byte> nativeˉimage,
        uint nativeˉentryˉoffset)
    {
        var Layout = Plan(
            Consoleˉapplicationˉtarget.Windowsˉx64,
            nativeˉimage.Length,
            nativeˉentryˉoffset);
        var Outputˉservice = X64ˉnativeˉoutputˉservices.Build(
            Nativeˉservice.Consoleˉwriteˉline,
            Nativeˉoutputˉplatform.Windows);
        var Result = new byte[Layout.Applicationˉbytes];

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
        Result[Optional + 2] = Windowsˉconsoleˉapplicationˉcontract.HOSTED_FORMAT_VERSION;
        Writeˉu32(Result, Optional + 4, Layout.Textˉfileˉbytes);
        Writeˉu32(Result, Optional + 8, Layout.Dataˉfileˉbytes + WINDOWS_RELOCATION_RAW_BYTES);
        Writeˉu32(Result, Optional + 12, Layout.Dataˉvirtualˉbytes - Layout.Dataˉfileˉbytes);
        Writeˉu32(Result, Optional + 16, TEXT_ADDRESS);
        Writeˉu32(Result, Optional + 20, TEXT_ADDRESS);
        Writeˉu64(Result, Optional + 24, WINDOWS_IMAGE_BASE);
        Writeˉu32(Result, Optional + 32, PAGE_BYTES);
        Writeˉu32(Result, Optional + 36, WINDOWS_FILE_ALIGNMENT);
        Writeˉu16(Result, Optional + 40, 6);
        Writeˉu16(Result, Optional + 48, 6);
        Writeˉu32(Result, Optional + 56, Layout.Imageˉvirtualˉbytes);
        Writeˉu32(Result, Optional + 60, WINDOWS_HEADER_BYTES);
        Writeˉu16(Result, Optional + 68, 3);
        Writeˉu16(Result, Optional + 70, 0x0160);
        Writeˉu64(Result, Optional + 72, Windowsˉconsoleˉapplicationˉcontract.STACK_BYTES);
        Writeˉu64(Result, Optional + 80, 0x0001_0000);
        Writeˉu64(Result, Optional + 88, 0x0010_0000);
        Writeˉu64(Result, Optional + 96, 0x0000_1000);
        Writeˉu32(Result, Optional + 108, 16);
        Writeˉu32(Result, Optional + 120, Layout.Dataˉaddress + WINDOWS_IMPORT_DIRECTORY_OFFSET);
        Writeˉu32(Result, Optional + 124, 40);
        Writeˉu32(Result, Optional + 152, Layout.Metadataˉaddress);
        Writeˉu32(Result, Optional + 156, WINDOWS_RELOCATION_BYTES);
        Writeˉu32(Result, Optional + 208, Layout.Dataˉaddress + WINDOWS_IMPORT_ADDRESS_OFFSET);
        Writeˉu32(Result, Optional + 212, 24);

        Writeˉsection(Result, SECTION_TABLE_OFFSET, ".text", Layout.Textˉvirtualˉbytes,
            TEXT_ADDRESS, Layout.Textˉfileˉbytes, WINDOWS_HEADER_BYTES, 0x6000_0020);
        Writeˉsection(Result, SECTION_TABLE_OFFSET + 40, ".data", Layout.Dataˉvirtualˉbytes,
            Layout.Dataˉaddress, Layout.Dataˉfileˉbytes, Layout.Dataˉfileˉoffset, 0xC000_0040);
        Writeˉsection(Result, SECTION_TABLE_OFFSET + 80, ".reloc", WINDOWS_RELOCATION_BYTES,
            Layout.Metadataˉaddress, WINDOWS_RELOCATION_RAW_BYTES, Layout.Metadataˉfileˉoffset,
            0x4200_0040);

        Writeˉwindowsˉstartup(
            Result.AsSpan(Layout.Textˉfileˉoffset, Layout.Startupˉbytes),
            Layout,
            nativeˉentryˉoffset);
        Outputˉservice.AsSpan().CopyTo(Result.AsSpan(
            Layout.Textˉfileˉoffset + Layout.Outputˉserviceˉoffset));
        nativeˉimage.CopyTo(Result.AsSpan(
            Layout.Textˉfileˉoffset + Layout.Nativeˉimageˉoffset));
        Writeˉhostedˉdata(Result, Layout, nativeˉimage, Outputˉservice.AsSpan());
        Writeˉwindowsˉimports(Result, Layout);
        Writeˉu32(Result, checked((int)Layout.Metadataˉfileˉoffset), TEXT_ADDRESS);
        Writeˉu32(Result, checked((int)Layout.Metadataˉfileˉoffset + 4), WINDOWS_RELOCATION_BYTES);
        return Result;
    }

    internal static byte[] Buildˉlinux(
        ReadOnlySpan<byte> nativeˉimage,
        uint nativeˉentryˉoffset)
    {
        var Layout = Plan(
            Consoleˉapplicationˉtarget.Linuxˉx64,
            nativeˉimage.Length,
            nativeˉentryˉoffset);
        var Outputˉservice = X64ˉnativeˉoutputˉservices.Build(
            Nativeˉservice.Consoleˉwriteˉline,
            Nativeˉoutputˉplatform.Linux);
        var Result = new byte[Layout.Applicationˉbytes];
        ReadOnlySpan<byte> Identification =
        [
            0x7F, 0x45, 0x4C, 0x46, 0x02, 0x01, 0x01, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        ];
        Identification.CopyTo(Result);
        Writeˉu16(Result, 16, 3);
        Writeˉu16(Result, 18, 62);
        Writeˉu32(Result, 20, 1);
        Writeˉu64(Result, 24, TEXT_ADDRESS);
        Writeˉu64(Result, 32, ELF_HEADER_BYTES);
        Writeˉu16(Result, 52, ELF_HEADER_BYTES);
        Writeˉu16(Result, 54, ELF_PROGRAM_HEADER_BYTES);
        Writeˉu16(Result, 56, ELF_PROGRAM_HEADER_COUNT);

        var Headerˉload = ELF_HEADER_BYTES;
        Writeˉprogramˉheader(Result, Headerˉload, 1, 4, 0, 0,
            checked((ulong)Layout.Headerˉbytes), checked((ulong)Layout.Headerˉbytes), PAGE_BYTES);
        var Textˉload = Headerˉload + ELF_PROGRAM_HEADER_BYTES;
        Writeˉprogramˉheader(Result, Textˉload, 1, 5,
            checked((ulong)Layout.Textˉfileˉoffset), TEXT_ADDRESS,
            Layout.Textˉfileˉbytes, Layout.Textˉvirtualˉbytes, PAGE_BYTES);
        var Dataˉload = Textˉload + ELF_PROGRAM_HEADER_BYTES;
        Writeˉprogramˉheader(Result, Dataˉload, 1, 6,
            Layout.Dataˉfileˉoffset, Layout.Dataˉaddress,
            Layout.Dataˉfileˉbytes, Layout.Dataˉvirtualˉbytes, PAGE_BYTES);
        var Note = Dataˉload + ELF_PROGRAM_HEADER_BYTES;
        Writeˉprogramˉheader(Result, Note, 4, 4, LINUX_NOTE_OFFSET, LINUX_NOTE_OFFSET,
            LINUX_NOTE_BYTES, LINUX_NOTE_BYTES, 4);
        var Stack = Note + ELF_PROGRAM_HEADER_BYTES;
        Writeˉprogramˉheader(Result, Stack, 0x6474_E551, 6, 0, 0, 0,
            Linuxˉconsoleˉapplicationˉcontract.STACK_BYTES, 16);

        Writeˉu32(Result, LINUX_NOTE_OFFSET + 0, 9);
        Writeˉu32(Result, LINUX_NOTE_OFFSET + 4, sizeof(uint));
        Writeˉu32(Result, LINUX_NOTE_OFFSET + 8, 1);
        ReadOnlySpan<byte> Noteˉname =
            [0x57, 0x69, 0x6E, 0x64, 0x76, 0x61, 0x6C, 0x65, 0x00];
        Noteˉname.CopyTo(Result.AsSpan(LINUX_NOTE_OFFSET + 12));
        Writeˉu32(Result, LINUX_NOTE_OFFSET + 24,
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_FORMAT_VERSION);

        Writeˉlinuxˉstartup(
            Result.AsSpan(Layout.Textˉfileˉoffset, Layout.Startupˉbytes),
            Layout,
            nativeˉentryˉoffset);
        Outputˉservice.AsSpan().CopyTo(Result.AsSpan(
            Layout.Textˉfileˉoffset + Layout.Outputˉserviceˉoffset));
        nativeˉimage.CopyTo(Result.AsSpan(
            Layout.Textˉfileˉoffset + Layout.Nativeˉimageˉoffset));
        Writeˉhostedˉdata(Result, Layout, nativeˉimage, Outputˉservice.AsSpan());
        return Result;
    }

    internal static void Writeˉwindowsˉstartup(
        Span<byte> output,
        Hostedˉconsoleˉapplicationˉlayout layout,
        uint nativeˉentryˉoffset)
    {
        ReadOnlySpan<byte> Template =
        [
            0x48,0x81,0xEC,0x28,0,0,0,0x48,0x8D,0x15,0,0,0,0,0x48,0x8D,
            0x05,0,0,0,0,0x48,0x89,0x84,0x22,0x18,0,0,0,0x48,0x8D,0x05,
            0,0,0,0,0x48,0x89,0x84,0x22,0x20,0,0,0,0x48,0x8D,0x05,0,
            0,0,0,0x48,0x89,0x84,0x22,0x30,0,0,0,0x48,0x8D,0x05,0,0,
            0,0,0x48,0x89,0x84,0x22,0x58,0,0,0,0x48,0x8D,0x15,0,0,0,
            0,0x48,0x8D,0x05,0,0,0,0,0x48,0x89,0x84,0x22,0x08,0,0,0,
            0xB9,0xF5,0xFF,0xFF,0xFF,0x48,0x8D,0x05,0,0,0,0,0x48,0x8B,0x84,0x20,
            0,0,0,0,0xFF,0xD0,0x48,0x8D,0x15,0,0,0,0,0x48,0x89,0x84,
            0x22,0x18,0,0,0,0x48,0x8D,0x05,0,0,0,0,0x48,0x8B,0x84,0x20,
            0,0,0,0,0x48,0x8D,0x15,0,0,0,0,0x48,0x89,0x84,0x22,0x28,
            0,0,0,0x48,0x8D,0x15,0,0,0,0,0x48,0x31,0xC9,0x49,0x89,0xD0,
            0x4D,0x31,0xC9,0xE8,0,0,0,0,0x48,0x89,0xC2,0x48,0xC1,0xEA,0x20,0x85,
            0xD2,0x0F,0x85,0x0C,0,0,0,0x81,0xF8,0xFF,0,0,0,0x0F,0x86,0x05,
            0,0,0,0xB8,0x01,0,0,0,0x48,0x81,0xC4,0x28,0,0,0,0xC3,
        ];
        Template.CopyTo(output);
        Patchˉrelative(output, 10, TEXT_ADDRESS, layout.Dataˉaddress);
        Patchˉrelative(output, 17, TEXT_ADDRESS, layout.Dataˉaddress +
            Nativeˉconsoleˉapplicationˉcontract.HOSTED_SERVICE_TABLE_OFFSET);
        Patchˉrelative(output, 32, TEXT_ADDRESS, layout.Dataˉaddress +
            Nativeˉconsoleˉapplicationˉcontract.HOSTED_RECORD_ARENA_OFFSET);
        Patchˉrelative(output, 47, TEXT_ADDRESS, layout.Dataˉaddress +
            Nativeˉconsoleˉapplicationˉcontract.HOSTED_TEXT_ARENA_OFFSET);
        Patchˉrelative(output, 62, TEXT_ADDRESS, layout.Dataˉaddress +
            Nativeˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_TABLE_OFFSET);
        Patchˉrelative(output, 77, TEXT_ADDRESS, layout.Dataˉaddress +
            Nativeˉconsoleˉapplicationˉcontract.HOSTED_SERVICE_TABLE_OFFSET);
        Patchˉrelative(output, 84, TEXT_ADDRESS, TEXT_ADDRESS +
            Windowsˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_SERVICE_OFFSET);
        Patchˉrelative(output, 104, TEXT_ADDRESS, layout.Dataˉaddress + WINDOWS_IMPORT_ADDRESS_OFFSET);
        Patchˉrelative(output, 121, TEXT_ADDRESS, layout.Dataˉaddress +
            Nativeˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_TABLE_OFFSET);
        Patchˉrelative(output, 136, TEXT_ADDRESS, layout.Dataˉaddress + WINDOWS_IMPORT_ADDRESS_OFFSET + 8);
        Patchˉrelative(output, 151, TEXT_ADDRESS, layout.Dataˉaddress +
            Nativeˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_TABLE_OFFSET);
        Patchˉrelative(output, 166, TEXT_ADDRESS, layout.Dataˉaddress);
        Patchˉrelative(output, 180, TEXT_ADDRESS, TEXT_ADDRESS +
            Windowsˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET + nativeˉentryˉoffset);
    }

    internal static void Writeˉlinuxˉstartup(
        Span<byte> output,
        Hostedˉconsoleˉapplicationˉlayout layout,
        uint nativeˉentryˉoffset)
    {
        ReadOnlySpan<byte> Template =
        [
            0x31,0xFF,0xBE,0,0,0,0x04,0xBA,0x03,0,0,0,0x41,0xBA,0x22,0,
            0x02,0,0x4D,0x31,0xC0,0x49,0x81,0xE8,0x01,0,0,0,0x45,0x31,0xC9,0xB8,
            0x09,0,0,0,0x0F,0x05,0x48,0x81,0xF8,0x01,0xF0,0xFF,0xFF,0x0F,0x83,0x97,
            0,0,0,0x48,0x89,0xC4,0x48,0x81,0xC4,0,0,0,0x04,0x48,0x8D,0x15,
            0,0,0,0,0x48,0x89,0xD6,0x48,0x8D,0x05,0,0,0,0,0x48,0x89,
            0x84,0x22,0x18,0,0,0,0x48,0x8D,0x05,0,0,0,0,0x48,0x89,0x84,
            0x22,0x20,0,0,0,0x48,0x8D,0x05,0,0,0,0,0x48,0x89,0x84,0x22,
            0x30,0,0,0,0x48,0x8D,0x05,0,0,0,0,0x48,0x89,0x84,0x22,0x58,
            0,0,0,0x48,0x8D,0x15,0,0,0,0,0x48,0x8D,0x05,0,0,0,
            0,0x48,0x89,0x84,0x22,0x08,0,0,0,0x48,0x8D,0x15,0,0,0,0,
            0x31,0xFF,0x31,0xC9,0x45,0x31,0xC0,0x45,0x31,0xC9,0xE8,0,0,0,0,0x48,
            0x89,0xC2,0x48,0xC1,0xEA,0x20,0x85,0xD2,0x0F,0x85,0x0C,0,0,0,0x81,0xF8,
            0xFF,0,0,0,0x0F,0x86,0x05,0,0,0,0xB8,0x01,0,0,0,0x89,
            0xC7,0xB8,0x3C,0,0,0,0x0F,0x05,0xCC,
        ];
        Template.CopyTo(output);
        Patchˉrelative(output, 64, TEXT_ADDRESS, layout.Dataˉaddress);
        Patchˉrelative(output, 74, TEXT_ADDRESS, layout.Dataˉaddress +
            Nativeˉconsoleˉapplicationˉcontract.HOSTED_SERVICE_TABLE_OFFSET);
        Patchˉrelative(output, 89, TEXT_ADDRESS, layout.Dataˉaddress +
            Nativeˉconsoleˉapplicationˉcontract.HOSTED_RECORD_ARENA_OFFSET);
        Patchˉrelative(output, 104, TEXT_ADDRESS, layout.Dataˉaddress +
            Nativeˉconsoleˉapplicationˉcontract.HOSTED_TEXT_ARENA_OFFSET);
        Patchˉrelative(output, 119, TEXT_ADDRESS, layout.Dataˉaddress +
            Nativeˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_TABLE_OFFSET);
        Patchˉrelative(output, 134, TEXT_ADDRESS, layout.Dataˉaddress +
            Nativeˉconsoleˉapplicationˉcontract.HOSTED_SERVICE_TABLE_OFFSET);
        Patchˉrelative(output, 141, TEXT_ADDRESS, TEXT_ADDRESS +
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_SERVICE_OFFSET);
        Patchˉrelative(output, 156, TEXT_ADDRESS, layout.Dataˉaddress);
        Patchˉrelative(output, 171, TEXT_ADDRESS, TEXT_ADDRESS +
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET + nativeˉentryˉoffset);
    }

    private static void Writeˉhostedˉdata(
        byte[] result,
        Hostedˉconsoleˉapplicationˉlayout layout,
        ReadOnlySpan<byte> nativeˉimage,
        ReadOnlySpan<byte> outputˉservice)
    {
        var Data = checked((int)layout.Dataˉfileˉoffset);
        Writeˉu32(result, Data + Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION);
        Writeˉu32(result, Data + Nativeˉexecutionˉcontextˉcontract.SIZE_OFFSET,
            Nativeˉexecutionˉcontextˉcontract.SIZE);
        Writeˉu64(result, Data + Nativeˉexecutionˉcontextˉcontract.INSTRUCTION_BUDGET_OFFSET,
            checked((ulong)Nativeˉcontract.DEFAULT_MAXIMUM_INSTRUCTIONS));
        Writeˉu64(result, Data + Nativeˉexecutionˉcontextˉcontract.CALL_DEPTH_BUDGET_OFFSET,
            checked((ulong)Nativeˉcontract.DEFAULT_MAXIMUM_CALL_DEPTH));
        Writeˉu32(result, Data + Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET,
            Nativeˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES);
        Writeˉu32(result, Data + Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
            Nativeˉconsoleˉapplicationˉcontract.HOSTED_TEXT_ARENA_BYTES);

        var Service = Data + checked((int)Nativeˉconsoleˉapplicationˉcontract.HOSTED_SERVICE_TABLE_OFFSET);
        Writeˉu32(result, Service + Nativeˉserviceˉtableˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉserviceˉtableˉcontract.FORMAT_VERSION);
        Writeˉu32(result, Service + Nativeˉserviceˉtableˉcontract.SIZE_OFFSET,
            Nativeˉserviceˉtableˉcontract.SIZE);

        var Output = Data + checked((int)Nativeˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_TABLE_OFFSET);
        Writeˉu32(result, Output + Nativeˉoutputˉtableˉcontract.MAGIC_OFFSET,
            Nativeˉoutputˉtableˉcontract.MAGIC);
        Writeˉu32(result, Output + Nativeˉoutputˉtableˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉoutputˉtableˉcontract.FORMAT_VERSION);
        Writeˉu32(result, Output + Nativeˉoutputˉtableˉcontract.SIZE_OFFSET,
            Nativeˉoutputˉtableˉcontract.SIZE);
        var Platform = layout.Target == Consoleˉapplicationˉtarget.Windowsˉx64
            ? Nativeˉoutputˉplatform.Windows
            : Nativeˉoutputˉplatform.Linux;
        Writeˉu32(result, Output + Nativeˉoutputˉtableˉcontract.PLATFORM_OFFSET, (uint)Platform);
        Writeˉu32(result, Output + Nativeˉoutputˉtableˉcontract.FLAGS_OFFSET,
            Nativeˉoutputˉtableˉcontract.CONSOLE_PRESENT);
        if (Platform == Nativeˉoutputˉplatform.Linux)
        {
            Writeˉu64(result, Output + Nativeˉoutputˉtableˉcontract.CONSOLE_TARGET_OFFSET, 1);
        }

        var Metadata = Hostedˉconsoleˉapplicationˉmetadata.Build(
            layout.Target,
            checked((uint)layout.Nativeˉimageˉoffset),
            nativeˉimage,
            layout.Nativeˉentryˉoffset,
            checked((uint)layout.Outputˉserviceˉoffset),
            outputˉservice);
        Metadata.AsSpan().CopyTo(result.AsSpan(
            Data + checked((int)Nativeˉconsoleˉapplicationˉcontract.HOSTED_METADATA_OFFSET)));
    }

    private static void Writeˉwindowsˉimports(
        byte[] result,
        Hostedˉconsoleˉapplicationˉlayout layout)
    {
        var Data = checked((int)layout.Dataˉfileˉoffset);
        var Rva = layout.Dataˉaddress;
        var Descriptor = Data + checked((int)WINDOWS_IMPORT_DIRECTORY_OFFSET);
        Writeˉu32(result, Descriptor + 0, Rva + WINDOWS_IMPORT_LOOKUP_OFFSET);
        Writeˉu32(result, Descriptor + 12, Rva + WINDOWS_LIBRARY_NAME_OFFSET);
        Writeˉu32(result, Descriptor + 16, Rva + WINDOWS_IMPORT_ADDRESS_OFFSET);
        for (var Table = WINDOWS_IMPORT_LOOKUP_OFFSET;
            Table <= WINDOWS_IMPORT_ADDRESS_OFFSET;
            Table += WINDOWS_IMPORT_ADDRESS_OFFSET - WINDOWS_IMPORT_LOOKUP_OFFSET)
        {
            Writeˉu64(result, Data + checked((int)Table), Rva + WINDOWS_GET_STD_HANDLE_NAME_OFFSET);
            Writeˉu64(result, Data + checked((int)Table + 8), Rva + WINDOWS_WRITE_FILE_NAME_OFFSET);
        }
        Writeˉascii(result, Data + checked((int)WINDOWS_GET_STD_HANDLE_NAME_OFFSET) + 2,
            "GetStdHandle");
        Writeˉascii(result, Data + checked((int)WINDOWS_WRITE_FILE_NAME_OFFSET) + 2,
            "WriteFile");
        Writeˉascii(result, Data + checked((int)WINDOWS_LIBRARY_NAME_OFFSET), "KERNEL32.dll");
    }

    private static void Patchˉrelative(
        Span<byte> output,
        int offset,
        uint textˉaddress,
        uint target)
    {
        var Sourceˉend = checked(textˉaddress + (uint)offset + sizeof(int));
        BinaryPrimitives.WriteInt32LittleEndian(
            output.Slice(offset, sizeof(int)),
            checked((int)((long)target - Sourceˉend)));
    }

    private static void Writeˉsection(
        byte[] result,
        int offset,
        string name,
        uint virtualˉbytes,
        uint address,
        uint fileˉbytes,
        uint fileˉoffset,
        uint characteristics)
    {
        Writeˉascii(result, offset, name, terminate: false);
        Writeˉu32(result, offset + 8, virtualˉbytes);
        Writeˉu32(result, offset + 12, address);
        Writeˉu32(result, offset + 16, fileˉbytes);
        Writeˉu32(result, offset + 20, fileˉoffset);
        Writeˉu32(result, offset + 36, characteristics);
    }

    private static void Writeˉprogramˉheader(
        byte[] result,
        int offset,
        uint type,
        uint flags,
        ulong fileˉoffset,
        ulong virtualˉaddress,
        ulong fileˉbytes,
        ulong memoryˉbytes,
        ulong alignment)
    {
        Writeˉu32(result, offset, type);
        Writeˉu32(result, offset + 4, flags);
        Writeˉu64(result, offset + 8, fileˉoffset);
        Writeˉu64(result, offset + 16, virtualˉaddress);
        Writeˉu64(result, offset + 32, fileˉbytes);
        Writeˉu64(result, offset + 40, memoryˉbytes);
        Writeˉu64(result, offset + 48, alignment);
    }

    private static uint Alignˉup(uint value, uint alignment) =>
        checked((value + alignment - 1) & ~(alignment - 1));

    private static void Writeˉascii(
        byte[] result,
        int offset,
        string value,
        bool terminate = true)
    {
        for (var Index = 0; Index < value.Length; Index++)
        {
            result[offset + Index] = checked((byte)value[Index]);
        }
        if (terminate)
        {
            result[offset + value.Length] = 0;
        }
    }

    private static void Writeˉu16(byte[] result, int offset, ushort value) =>
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(offset, sizeof(ushort)), value);

    private static void Writeˉu32(byte[] result, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(offset, sizeof(uint)), value);

    private static void Writeˉu64(byte[] result, int offset, ulong value) =>
        BinaryPrimitives.WriteUInt64LittleEndian(result.AsSpan(offset, sizeof(ulong)), value);
}
