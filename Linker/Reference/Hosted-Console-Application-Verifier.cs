using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Hostedˉconsoleˉapplicationˉverifier
{
    private const int WINDOWS_OPTIONAL = 0x98;
    private const int WINDOWS_SECTIONS = 0x188;
    private const int WINDOWS_HEADERS = 0x200;
    private const uint TEXT_ADDRESS = 0x1000;
    private const uint WINDOWS_IMPORT_DIRECTORY_OFFSET = 464;
    private const uint WINDOWS_IMPORT_LOOKUP_OFFSET = 504;
    private const uint WINDOWS_IMPORT_ADDRESS_OFFSET = 528;
    private const uint WINDOWS_GET_STD_HANDLE_NAME_OFFSET = 552;
    private const uint WINDOWS_WRITE_FILE_NAME_OFFSET = 568;
    private const uint WINDOWS_LIBRARY_NAME_OFFSET = 580;
    private const int LINUX_NOTE_OFFSET = 0x180;

    private static readonly int[] WINDOWS_STARTUP_PATCHES =
        [10, 17, 32, 47, 62, 77, 84, 104, 121, 136, 151, 166, 180];
    private static readonly uint[] WINDOWS_STARTUP_TARGET_KINDS =
        [0, 112, 1024, 2_098_176, 216, 112, uint.MaxValue, 528, 216, 536, 216, 0, uint.MaxValue];
    private static readonly int[] LINUX_STARTUP_PATCHES =
        [64, 74, 89, 104, 119, 134, 141, 156, 171];
    private static readonly uint[] LINUX_STARTUP_TARGET_KINDS =
        [0, 112, 1024, 2_098_176, 216, 112, uint.MaxValue, 0, uint.MaxValue];
    private const string WINDOWS_STARTUP_SHA256 =
        "4e9b792e2517431efe58b9d00168a3f1b085852ba37d6bf9f9484f18e0c50a7e";
    private const string LINUX_STARTUP_SHA256 =
        "ff296788d5d7221c8e5ec06753957a678b3d249f37ac291c03829832e6b609c7";

    internal static Verifiedˉwindowsˉconsoleˉapplication Verifyˉwindows(
        ReadOnlySpan<byte> bytes)
    {
        Require(bytes.Length is >= 2_048 and <=
            Windowsˉconsoleˉapplicationˉcontract.HOSTED_MAX_APPLICATION_BYTES,
            "The Windows hosted application length is outside the v2 bounds.");
        Requireˉu16(bytes, 0, 0x5A4D, "The DOS signature is invalid.");
        Requireˉzero(bytes, 2, 0x3A, "The DOS header contains noncanonical bytes.");
        Requireˉu32(bytes, 0x3C, 0x80, "The PE header offset is invalid.");
        Requireˉzero(bytes, 0x40, 0x40, "The DOS stub contains noncanonical bytes.");
        Requireˉu32(bytes, 0x80, 0x0000_4550, "The PE signature is invalid.");
        Requireˉu16(bytes, 0x84, 0x8664, "The PE machine is not x86-64.");
        Requireˉu16(bytes, 0x86, 3, "The PE section count is invalid.");
        Requireˉu32(bytes, 0x88, 0, "The PE timestamp must be zero.");
        Requireˉzero(bytes, 0x8C, 8, "The COFF symbol metadata must be zero.");
        Requireˉu16(bytes, 0x94, 0xF0, "The PE optional-header size is invalid.");
        Requireˉu16(bytes, WINDOWS_OPTIONAL, 0x020B, "The image is not PE32+.");
        Requireˉbyte(bytes, WINDOWS_OPTIONAL + 2,
            Windowsˉconsoleˉapplicationˉcontract.HOSTED_FORMAT_VERSION,
            "The Windows hosted writer version is invalid.");
        Requireˉbyte(bytes, WINDOWS_OPTIONAL + 3, 0, "The writer minor version is invalid.");
        Requireˉu32(bytes, WINDOWS_OPTIONAL + 16, TEXT_ADDRESS, "The entry RVA is invalid.");
        Requireˉu32(bytes, WINDOWS_OPTIONAL + 20, TEXT_ADDRESS, "The code RVA is invalid.");
        Requireˉu64(bytes, WINDOWS_OPTIONAL + 24, 0x0000_0001_4000_0000,
            "The image base is invalid.");
        Requireˉu32(bytes, WINDOWS_OPTIONAL + 32, 0x1000, "The section alignment is invalid.");
        Requireˉu32(bytes, WINDOWS_OPTIONAL + 36, 0x200, "The file alignment is invalid.");
        Requireˉu32(bytes, WINDOWS_OPTIONAL + 60, WINDOWS_HEADERS,
            "The header size is invalid.");
        Requireˉu16(bytes, WINDOWS_OPTIONAL + 68, 3, "The image is not a console executable.");
        Requireˉu64(bytes, WINDOWS_OPTIONAL + 72,
            Windowsˉconsoleˉapplicationˉcontract.STACK_BYTES,
            "The stack reserve is invalid.");
        Requireˉu32(bytes, WINDOWS_OPTIONAL + 108, 16,
            "The data-directory count is invalid.");
        Requireˉzero(bytes, WINDOWS_OPTIONAL + 112, 8,
            "The export directory must be zero.");
        Requireˉzero(bytes, WINDOWS_OPTIONAL + 128, 24,
            "Unused directories before base relocation must be zero.");
        Requireˉzero(bytes, WINDOWS_OPTIONAL + 160, 48,
            "Unused directories before the import-address table must be zero.");
        Requireˉzero(bytes, WINDOWS_OPTIONAL + 216, 24,
            "Trailing data directories must be zero.");

        Requireˉname(bytes, WINDOWS_SECTIONS, ".text", "The first section is not .text.");
        var Textˉbytes = Readˉu32(bytes, WINDOWS_SECTIONS + 8);
        var Textˉraw = Readˉu32(bytes, WINDOWS_SECTIONS + 16);
        Require(Textˉbytes > Windowsˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET,
            "The hosted text section has no native image.");
        Requireˉu32(bytes, WINDOWS_SECTIONS + 12, TEXT_ADDRESS, "The text RVA is invalid.");
        Require(Textˉraw == Alignˉup(Textˉbytes, 0x200),
            "The text raw size is not canonical.");
        Requireˉu32(bytes, WINDOWS_SECTIONS + 20, WINDOWS_HEADERS,
            "The text file offset is invalid.");
        Requireˉu32(bytes, WINDOWS_SECTIONS + 36, 0x6000_0020,
            "The text permissions are invalid.");
        Requireˉzero(bytes, WINDOWS_SECTIONS + 24, 12,
            "The text section contains object metadata.");

        var Nativeˉbytes = checked((int)Textˉbytes -
            Windowsˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET);
        var Dataˉfile = checked((uint)WINDOWS_HEADERS + Textˉraw);
        var Dataˉaddress = Alignˉup(TEXT_ADDRESS + Textˉbytes, 0x1000);
        var Relocationˉfile = Dataˉfile +
            Windowsˉconsoleˉapplicationˉcontract.HOSTED_DATA_FILE_BYTES;
        var Relocationˉaddress = Alignˉup(Dataˉaddress +
            Windowsˉconsoleˉapplicationˉcontract.HOSTED_DATA_VIRTUAL_BYTES, 0x1000);

        Requireˉname(bytes, WINDOWS_SECTIONS + 40, ".data", "The second section is not .data.");
        Requireˉu32(bytes, WINDOWS_SECTIONS + 48,
            Windowsˉconsoleˉapplicationˉcontract.HOSTED_DATA_VIRTUAL_BYTES,
            "The hosted data virtual size is invalid.");
        Requireˉu32(bytes, WINDOWS_SECTIONS + 52, Dataˉaddress, "The data RVA is invalid.");
        Requireˉu32(bytes, WINDOWS_SECTIONS + 56,
            Windowsˉconsoleˉapplicationˉcontract.HOSTED_DATA_FILE_BYTES,
            "The data raw size is invalid.");
        Requireˉu32(bytes, WINDOWS_SECTIONS + 60, Dataˉfile, "The data file offset is invalid.");
        Requireˉu32(bytes, WINDOWS_SECTIONS + 76, 0xC000_0040,
            "The data permissions are invalid.");
        Requireˉzero(bytes, WINDOWS_SECTIONS + 64, 12,
            "The data section contains object metadata.");
        Requireˉname(bytes, WINDOWS_SECTIONS + 80, ".reloc", "The third section is not .reloc.");
        Requireˉu32(bytes, WINDOWS_SECTIONS + 88, 12, "The relocation size is invalid.");
        Requireˉu32(bytes, WINDOWS_SECTIONS + 92, Relocationˉaddress,
            "The relocation RVA is invalid.");
        Requireˉu32(bytes, WINDOWS_SECTIONS + 96, 0x200,
            "The relocation raw size is invalid.");
        Requireˉu32(bytes, WINDOWS_SECTIONS + 100, Relocationˉfile,
            "The relocation file offset is invalid.");
        Requireˉu32(bytes, WINDOWS_SECTIONS + 116, 0x4200_0040,
            "The relocation permissions are invalid.");
        Requireˉzero(bytes, WINDOWS_SECTIONS + 104, 12,
            "The relocation section contains object metadata.");
        Require(bytes.Length == Relocationˉfile + 0x200,
            "The Windows hosted application has trailing or missing bytes.");

        Requireˉu32(bytes, WINDOWS_OPTIONAL + 4, Textˉraw, "The code size is inconsistent.");
        Requireˉu32(bytes, WINDOWS_OPTIONAL + 8, 0x600, "The initialized-data size is invalid.");
        Requireˉu32(bytes, WINDOWS_OPTIONAL + 12,
            Windowsˉconsoleˉapplicationˉcontract.HOSTED_DATA_VIRTUAL_BYTES - 0x400,
            "The uninitialized-data size is invalid.");
        Requireˉu32(bytes, WINDOWS_OPTIONAL + 120,
            Dataˉaddress + WINDOWS_IMPORT_DIRECTORY_OFFSET,
            "The import directory RVA is invalid.");
        Requireˉu32(bytes, WINDOWS_OPTIONAL + 124, 40,
            "The import directory size is invalid.");
        Requireˉu32(bytes, WINDOWS_OPTIONAL + 152, Relocationˉaddress,
            "The relocation directory RVA is invalid.");
        Requireˉu32(bytes, WINDOWS_OPTIONAL + 156, 12,
            "The relocation directory size is invalid.");
        Requireˉu32(bytes, WINDOWS_OPTIONAL + 208,
            Dataˉaddress + WINDOWS_IMPORT_ADDRESS_OFFSET,
            "The import-address table RVA is invalid.");
        Requireˉu32(bytes, WINDOWS_OPTIONAL + 212, 24,
            "The import-address table size is invalid.");

        var Native = bytes.Slice(
            WINDOWS_HEADERS + Windowsˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET,
            Nativeˉbytes);
        var Output = bytes.Slice(
            WINDOWS_HEADERS + Windowsˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_SERVICE_OFFSET,
            X64ˉnativeˉoutputˉservices.WINDOWS_CANONICAL_SIZE);
        var Entry = Verifyˉstartup(
            bytes.Slice(WINDOWS_HEADERS,
                Windowsˉconsoleˉapplicationˉcontract.HOSTED_STARTUP_BYTES),
            WINDOWS_STARTUP_PATCHES,
            WINDOWS_STARTUP_TARGET_KINDS,
            WINDOWS_STARTUP_SHA256,
            Dataˉaddress,
            Windowsˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_SERVICE_OFFSET,
            Windowsˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET,
            Nativeˉbytes);
        Verifyˉhostedˉdata(bytes, checked((int)Dataˉfile),
            Consoleˉapplicationˉtarget.Windowsˉx64, Native, Entry, Output);
        Verifyˉwindowsˉimports(bytes, checked((int)Dataˉfile), Dataˉaddress);
        Requireˉzero(bytes,
            WINDOWS_HEADERS + Windowsˉconsoleˉapplicationˉcontract.HOSTED_STARTUP_BYTES,
            Windowsˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_SERVICE_OFFSET -
                Windowsˉconsoleˉapplicationˉcontract.HOSTED_STARTUP_BYTES,
            "The startup alignment padding is not zero.");
        Requireˉzero(bytes,
            WINDOWS_HEADERS + Windowsˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_SERVICE_OFFSET +
                X64ˉnativeˉoutputˉservices.WINDOWS_CANONICAL_SIZE,
            Windowsˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET -
                Windowsˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_SERVICE_OFFSET -
                X64ˉnativeˉoutputˉservices.WINDOWS_CANONICAL_SIZE,
            "The output-service alignment padding is not zero.");
        Requireˉzero(bytes, WINDOWS_HEADERS + checked((int)Textˉbytes),
            checked((int)(Textˉraw - Textˉbytes)), "The text padding is not zero.");
        Requireˉu32(bytes, checked((int)Relocationˉfile), TEXT_ADDRESS,
            "The relocation page is invalid.");
        Requireˉu32(bytes, checked((int)Relocationˉfile + 4), 12,
            "The relocation block size is invalid.");
        Requireˉu32(bytes, checked((int)Relocationˉfile + 8), 0,
            "Only absolute relocation padding is permitted.");
        Requireˉzero(bytes, checked((int)Relocationˉfile + 12), 0x200 - 12,
            "The relocation padding is not zero.");

        return new(Native.ToArray().ToImmutableArray(), Entry,
            Windowsˉconsoleˉapplicationˉcontract.HOSTED_FORMAT_VERSION,
            [Nativeˉservice.Consoleˉwriteˉline]);
    }

    internal static Verifiedˉlinuxˉconsoleˉapplication Verifyˉlinux(
        ReadOnlySpan<byte> bytes)
    {
        Require(bytes.Length is >= 5_120 and <=
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_MAX_APPLICATION_BYTES,
            "The Linux hosted application length is outside the v2 bounds.");
        Requireˉbytes(bytes, 0, [0x7F, 0x45, 0x4C, 0x46, 2, 1, 1],
            "The ELF identification is invalid.");
        Requireˉzero(bytes, 7, 9, "The ELF identification padding is noncanonical.");
        Requireˉu16(bytes, 16, 3, "The ELF type is not position-independent executable.");
        Requireˉu16(bytes, 18, 62, "The ELF machine is not x86-64.");
        Requireˉu32(bytes, 20, 1, "The ELF version is invalid.");
        Requireˉu64(bytes, 24, TEXT_ADDRESS, "The ELF entry is invalid.");
        Requireˉu64(bytes, 32, 64, "The program-header offset is invalid.");
        Requireˉu16(bytes, 52, 64, "The ELF header size is invalid.");
        Requireˉu16(bytes, 54, 56, "The program-header size is invalid.");
        Requireˉu16(bytes, 56, 5, "The program-header count is invalid.");
        Requireˉu64(bytes, 40, 0, "The section-header offset must be zero.");
        Requireˉu32(bytes, 48, 0, "The ELF flags must be zero.");
        Requireˉzero(bytes, 58, 6, "The section-header metadata must be zero.");

        Verifyˉprogramˉheader(bytes, 64, 1, 4, 0, 0, 0x1000, 0x1000, 0x1000);
        var Text = 120;
        Requireˉu32(bytes, Text, 1, "The text segment type is invalid.");
        Requireˉu32(bytes, Text + 4, 5, "The text segment permissions are invalid.");
        Requireˉu64(bytes, Text + 8, 0x1000, "The text file offset is invalid.");
        Requireˉu64(bytes, Text + 16, TEXT_ADDRESS, "The text address is invalid.");
        var Textˉbytes64 = Readˉu64(bytes, Text + 32);
        Require(Textˉbytes64 > (ulong)Linuxˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET &&
            Textˉbytes64 <= (ulong)Linuxˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET +
                Consoleˉapplicationˉlayout.MAXIMUM_NATIVE_IMAGE_BYTES,
            "The Linux hosted text size is invalid.");
        Requireˉu64(bytes, Text + 40, Textˉbytes64, "The text memory size is inconsistent.");
        Requireˉu64(bytes, Text + 48, 0x1000, "The text alignment is invalid.");
        var Textˉbytes = checked((uint)Textˉbytes64);
        var Nativeˉbytes = checked((int)Textˉbytes -
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET);
        var Dataˉfile = Alignˉup(0x1000 + Textˉbytes, 0x1000);
        Verifyˉprogramˉheader(bytes, 176, 1, 6,
            Dataˉfile, Dataˉfile,
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_DATA_FILE_BYTES,
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_DATA_VIRTUAL_BYTES, 0x1000);
        Verifyˉprogramˉheader(bytes, 232, 4, 4,
            LINUX_NOTE_OFFSET, LINUX_NOTE_OFFSET, 28, 28, 4);
        Verifyˉprogramˉheader(bytes, 288, 0x6474_E551, 6, 0, 0, 0,
            Linuxˉconsoleˉapplicationˉcontract.STACK_BYTES, 16);
        Require(bytes.Length == Dataˉfile +
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_DATA_FILE_BYTES,
            "The Linux hosted application has trailing or missing bytes.");
        Requireˉu32(bytes, LINUX_NOTE_OFFSET, 9, "The Windvale note name size is invalid.");
        Requireˉu32(bytes, LINUX_NOTE_OFFSET + 4, 4, "The Windvale note value size is invalid.");
        Requireˉu32(bytes, LINUX_NOTE_OFFSET + 8, 1, "The Windvale note type is invalid.");
        Requireˉbytes(bytes, LINUX_NOTE_OFFSET + 12,
            [0x57,0x69,0x6E,0x64,0x76,0x61,0x6C,0x65,0x00,0x00,0x00,0x00],
            "The Windvale note name is invalid.");
        Requireˉu32(bytes, LINUX_NOTE_OFFSET + 24,
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_FORMAT_VERSION,
            "The Linux hosted format version is invalid.");
        Requireˉzero(bytes, 344, LINUX_NOTE_OFFSET - 344,
            "The pre-note ELF header padding is not zero.");
        Requireˉzero(bytes, LINUX_NOTE_OFFSET + 28, 0x1000 - LINUX_NOTE_OFFSET - 28,
            "The post-note ELF header padding is not zero.");

        var Native = bytes.Slice(
            0x1000 + Linuxˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET,
            Nativeˉbytes);
        var Output = bytes.Slice(
            0x1000 + Linuxˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_SERVICE_OFFSET,
            X64ˉnativeˉoutputˉservices.LINUX_CANONICAL_SIZE);
        var Entry = Verifyˉstartup(
            bytes.Slice(0x1000, Linuxˉconsoleˉapplicationˉcontract.HOSTED_STARTUP_BYTES),
            LINUX_STARTUP_PATCHES,
            LINUX_STARTUP_TARGET_KINDS,
            LINUX_STARTUP_SHA256,
            Dataˉfile,
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_SERVICE_OFFSET,
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET,
            Nativeˉbytes);
        Verifyˉhostedˉdata(bytes, checked((int)Dataˉfile),
            Consoleˉapplicationˉtarget.Linuxˉx64, Native, Entry, Output);
        Requireˉzero(bytes,
            0x1000 + Linuxˉconsoleˉapplicationˉcontract.HOSTED_STARTUP_BYTES,
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_SERVICE_OFFSET -
                Linuxˉconsoleˉapplicationˉcontract.HOSTED_STARTUP_BYTES,
            "The Linux startup alignment padding is not zero.");
        Requireˉzero(bytes,
            0x1000 + Linuxˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_SERVICE_OFFSET +
                X64ˉnativeˉoutputˉservices.LINUX_CANONICAL_SIZE,
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET -
                Linuxˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_SERVICE_OFFSET -
                X64ˉnativeˉoutputˉservices.LINUX_CANONICAL_SIZE,
            "The Linux output-service alignment padding is not zero.");
        Requireˉzero(bytes, 0x1000 + checked((int)Textˉbytes),
            checked((int)Dataˉfile - 0x1000 - (int)Textˉbytes),
            "The Linux segment padding is not zero.");

        return new(Native.ToArray().ToImmutableArray(), Entry,
            Linuxˉconsoleˉapplicationˉcontract.HOSTED_FORMAT_VERSION,
            [Nativeˉservice.Consoleˉwriteˉline]);
    }

    private static uint Verifyˉstartup(
        ReadOnlySpan<byte> startup,
        int[] patches,
        uint[] targets,
        string expectedˉnormalizedˉsha256,
        uint dataˉaddress,
        int outputˉserviceˉoffset,
        int nativeˉimageˉoffset,
        int nativeˉimageˉbytes)
    {
        var Normalized = startup.ToArray();
        uint Entry = uint.MaxValue;
        for (var Index = 0; Index < patches.Length; Index++)
        {
            var Patch = patches[Index];
            var Target = checked((long)TEXT_ADDRESS + Patch + sizeof(int) +
                Readˉi32(startup, Patch));
            var Expected = targets[Index] switch
            {
                uint.MaxValue when Index == patches.Length - 1 =>
                    checked((long)TEXT_ADDRESS + nativeˉimageˉoffset),
                uint.MaxValue => checked((long)TEXT_ADDRESS + outputˉserviceˉoffset),
                var Offset => checked((long)dataˉaddress + Offset),
            };
            if (Index == patches.Length - 1)
            {
                Require(Target >= Expected && Target < Expected + nativeˉimageˉbytes,
                    "The hosted startup entry target is outside the native image.");
                Entry = checked((uint)(Target - Expected));
            }
            else
            {
                Require(Target == Expected, "A hosted startup relocation target is invalid.");
            }
            Normalized.AsSpan(Patch, sizeof(int)).Clear();
        }
        Require(StringComparer.Ordinal.Equals(
            Convert.ToHexString(SHA256.HashData(Normalized)).ToLowerInvariant(),
            expectedˉnormalizedˉsha256),
            "The hosted startup machine code is not the canonical WVA image.");
        return Entry;
    }

    private static void Verifyˉhostedˉdata(
        ReadOnlySpan<byte> bytes,
        int data,
        Consoleˉapplicationˉtarget target,
        ReadOnlySpan<byte> native,
        uint entry,
        ReadOnlySpan<byte> outputˉservice)
    {
        Requireˉu32(bytes, data, Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION,
            "The execution-context version is invalid.");
        Requireˉu32(bytes, data + 4, Nativeˉexecutionˉcontextˉcontract.SIZE,
            "The execution-context size is invalid.");
        Requireˉu64(bytes, data + 8, checked((ulong)Nativeˉcontract.DEFAULT_MAXIMUM_INSTRUCTIONS),
            "The instruction budget is invalid.");
        Requireˉu64(bytes, data + 16, checked((ulong)Nativeˉcontract.DEFAULT_MAXIMUM_CALL_DEPTH),
            "The call-depth budget is invalid.");
        Requireˉzero(bytes, data + 24, 16, "Initial service and record pointers must be zero.");
        Requireˉu32(bytes, data + 40, Nativeˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES,
            "The record-arena size is invalid.");
        Requireˉzero(bytes, data + 44, 12, "Initial record cursor and text pointer must be zero.");
        Requireˉu32(bytes, data + 56,
            Nativeˉconsoleˉapplicationˉcontract.HOSTED_TEXT_ARENA_BYTES,
            "The text-arena size is invalid.");
        Requireˉzero(bytes, data + 60, 52, "The remaining execution context must be zero.");

        var Service = data + checked((int)Nativeˉconsoleˉapplicationˉcontract.HOSTED_SERVICE_TABLE_OFFSET);
        Requireˉu32(bytes, Service, Nativeˉserviceˉtableˉcontract.FORMAT_VERSION,
            "The service-table version is invalid.");
        Requireˉu32(bytes, Service + 4, Nativeˉserviceˉtableˉcontract.SIZE,
            "The service-table size is invalid.");
        Requireˉzero(bytes, Service + 8, checked((int)Nativeˉserviceˉtableˉcontract.SIZE - 8),
            "The initial service-table pointers must be zero.");

        var Output = data + checked((int)Nativeˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_TABLE_OFFSET);
        Requireˉu32(bytes, Output, Nativeˉoutputˉtableˉcontract.MAGIC,
            "The output-table magic is invalid.");
        Requireˉu32(bytes, Output + 4, Nativeˉoutputˉtableˉcontract.FORMAT_VERSION,
            "The output-table version is invalid.");
        Requireˉu32(bytes, Output + 8, Nativeˉoutputˉtableˉcontract.SIZE,
            "The output-table size is invalid.");
        Requireˉu32(bytes, Output + 12,
            target == Consoleˉapplicationˉtarget.Windowsˉx64
                ? (uint)Nativeˉoutputˉplatform.Windows
                : (uint)Nativeˉoutputˉplatform.Linux,
            "The output-table platform is invalid.");
        Requireˉu32(bytes, Output + 16, Nativeˉoutputˉtableˉcontract.CONSOLE_PRESENT,
            "The output-table flags are invalid.");
        Requireˉu32(bytes, Output + 20, 0, "The output-table reserved field is nonzero.");
        Requireˉu64(bytes, Output + 24,
            target == Consoleˉapplicationˉtarget.Windowsˉx64 ? 0UL : 1UL,
            "The initial console target is invalid.");
        Requireˉzero(bytes, Output + 32, 16, "Unused output-table fields must be zero.");
        Requireˉzero(bytes, data + 264, 8, "The metadata alignment padding is not zero.");

        var Metadata = Hostedˉconsoleˉapplicationˉmetadata.Verify(
            bytes.Slice(
                data + checked((int)Nativeˉconsoleˉapplicationˉcontract.HOSTED_METADATA_OFFSET),
                Hostedˉconsoleˉapplicationˉmetadata.SIZE),
            target,
            native,
            outputˉservice);
        try
        {
            X64ˉnativeˉoutputˉservices.Verify(
                Nativeˉservice.Consoleˉwriteˉline,
                target == Consoleˉapplicationˉtarget.Windowsˉx64
                    ? Nativeˉoutputˉplatform.Windows
                    : Nativeˉoutputˉplatform.Linux,
                outputˉservice);
        }
        catch (InvalidOperationException Exception)
        {
            throw new InvalidDataException(
                "The hosted console output leaf is not canonical.",
                Exception);
        }
        Require(Metadata.Nativeˉentryˉoffset == entry,
            "The hosted metadata entry does not match the startup entry.");
        Require(Metadata.Nativeˉimageˉoffset ==
            (target == Consoleˉapplicationˉtarget.Windowsˉx64
                ? Windowsˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET
                : Linuxˉconsoleˉapplicationˉcontract.HOSTED_NATIVE_IMAGE_OFFSET),
            "The hosted metadata native-image offset is invalid.");
        Require(Metadata.Outputˉserviceˉoffset ==
            (target == Consoleˉapplicationˉtarget.Windowsˉx64
                ? Windowsˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_SERVICE_OFFSET
                : Linuxˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_SERVICE_OFFSET),
            "The hosted metadata output-service offset is invalid.");
    }

    private static void Verifyˉwindowsˉimports(
        ReadOnlySpan<byte> bytes,
        int data,
        uint dataˉaddress)
    {
        var Descriptor = data + checked((int)WINDOWS_IMPORT_DIRECTORY_OFFSET);
        Requireˉu32(bytes, Descriptor, dataˉaddress + WINDOWS_IMPORT_LOOKUP_OFFSET,
            "The import lookup table RVA is invalid.");
        Requireˉzero(bytes, Descriptor + 4, 8, "The import timestamp or forwarder is nonzero.");
        Requireˉu32(bytes, Descriptor + 12, dataˉaddress + WINDOWS_LIBRARY_NAME_OFFSET,
            "The import library-name RVA is invalid.");
        Requireˉu32(bytes, Descriptor + 16, dataˉaddress + WINDOWS_IMPORT_ADDRESS_OFFSET,
            "The import-address table RVA is invalid.");
        Requireˉzero(bytes, Descriptor + 20, 20, "The terminating import descriptor is nonzero.");
        foreach (var Table in new[] { WINDOWS_IMPORT_LOOKUP_OFFSET, WINDOWS_IMPORT_ADDRESS_OFFSET })
        {
            var Offset = data + checked((int)Table);
            Requireˉu64(bytes, Offset, dataˉaddress + WINDOWS_GET_STD_HANDLE_NAME_OFFSET,
                "The GetStdHandle thunk is invalid.");
            Requireˉu64(bytes, Offset + 8, dataˉaddress + WINDOWS_WRITE_FILE_NAME_OFFSET,
                "The WriteFile thunk is invalid.");
            Requireˉu64(bytes, Offset + 16, 0, "The import thunk terminator is invalid.");
        }
        Requireˉu16(bytes, data + checked((int)WINDOWS_GET_STD_HANDLE_NAME_OFFSET), 0,
            "The GetStdHandle hint is nonzero.");
        Requireˉascii(bytes, data + checked((int)WINDOWS_GET_STD_HANDLE_NAME_OFFSET) + 2,
            "GetStdHandle");
        Requireˉu16(bytes, data + checked((int)WINDOWS_WRITE_FILE_NAME_OFFSET), 0,
            "The WriteFile hint is nonzero.");
        Requireˉascii(bytes, data + checked((int)WINDOWS_WRITE_FILE_NAME_OFFSET) + 2,
            "WriteFile");
        Requireˉascii(bytes, data + checked((int)WINDOWS_LIBRARY_NAME_OFFSET), "KERNEL32.dll");
        Requireˉzero(bytes, data + 593,
            checked((int)Nativeˉconsoleˉapplicationˉcontract.HOSTED_DATA_HEADER_BYTES - 593),
            "The unused hosted data header is not zero.");
    }

    private static void Verifyˉprogramˉheader(
        ReadOnlySpan<byte> bytes,
        int offset,
        uint type,
        uint flags,
        ulong fileˉoffset,
        ulong address,
        ulong fileˉbytes,
        ulong memoryˉbytes,
        ulong alignment)
    {
        Requireˉu32(bytes, offset, type, "A program-header type is invalid.");
        Requireˉu32(bytes, offset + 4, flags, "A program-header permission set is invalid.");
        Requireˉu64(bytes, offset + 8, fileˉoffset, "A program-header file offset is invalid.");
        Requireˉu64(bytes, offset + 16, address, "A program-header address is invalid.");
        Requireˉu64(bytes, offset + 24, 0, "A program-header physical address is invalid.");
        Requireˉu64(bytes, offset + 32, fileˉbytes, "A program-header file size is invalid.");
        Requireˉu64(bytes, offset + 40, memoryˉbytes, "A program-header memory size is invalid.");
        Requireˉu64(bytes, offset + 48, alignment, "A program-header alignment is invalid.");
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
        ReadOnlySpan<byte> bytes, int offset, byte expected, string message) =>
        Require(bytes[offset] == expected, message);
    private static void Requireˉu16(
        ReadOnlySpan<byte> bytes, int offset, ushort expected, string message) =>
        Require(Readˉu16(bytes, offset) == expected, message);
    private static void Requireˉu32(
        ReadOnlySpan<byte> bytes, int offset, uint expected, string message) =>
        Require(Readˉu32(bytes, offset) == expected, message);
    private static void Requireˉu64(
        ReadOnlySpan<byte> bytes, int offset, ulong expected, string message) =>
        Require(Readˉu64(bytes, offset) == expected, message);

    private static void Requireˉbytes(
        ReadOnlySpan<byte> bytes,
        int offset,
        ReadOnlySpan<byte> expected,
        string message) =>
        Require(bytes.Slice(offset, expected.Length).SequenceEqual(expected), message);

    private static void Requireˉascii(ReadOnlySpan<byte> bytes, int offset, string expected)
    {
        for (var Index = 0; Index < expected.Length; Index++)
        {
            Require(bytes[offset + Index] == expected[Index],
                "A Windows import name is invalid.");
        }
        Require(bytes[offset + expected.Length] == 0,
            "A Windows import name is not terminated.");
    }

    private static void Requireˉname(
        ReadOnlySpan<byte> bytes, int offset, string expected, string message)
    {
        for (var Index = 0; Index < 8; Index++)
        {
            Require(bytes[offset + Index] ==
                (Index < expected.Length ? expected[Index] : 0), message);
        }
    }

    private static void Requireˉzero(
        ReadOnlySpan<byte> bytes, int offset, int length, string message)
    {
        for (var Index = 0; Index < length; Index++)
        {
            Require(bytes[offset + Index] == 0, message);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidDataException(message);
        }
    }
}
