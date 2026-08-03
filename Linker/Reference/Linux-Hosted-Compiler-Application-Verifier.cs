using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Linuxˉhostedˉcompilerˉapplicationˉverifier
{
    private const int ELF_HEADER_BYTES = 64;
    private const int PROGRAM_HEADER_BYTES = 56;
    private const int PROGRAM_HEADER_COUNT = 5;
    private const int PROGRAM_HEADER_TABLE_END =
        ELF_HEADER_BYTES + PROGRAM_HEADER_BYTES * PROGRAM_HEADER_COUNT;
    private const int NOTE_OFFSET = 0x180;
    private const int NOTE_BYTES = 28;
    private const uint PAGE_BYTES = 0x1000;

    internal static Verifiedˉlinuxˉhostedˉcompilerˉapplication Verify(
        ReadOnlySpan<byte> bytes,
        Nativeˉserviceˉbundle expectedˉbundle,
        Hostedˉcompilerˉapplicationˉprofile expectedˉprofile =
            Hostedˉcompilerˉapplicationˉprofile.Compiler)
    {
        Linuxˉhostedˉcompilerˉapplicationˉcontract.Validateˉbundle(expectedˉbundle);
        var Textˉbytes = checked((uint)(
            Linuxˉhostedˉcompilerˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
            expectedˉbundle.Imageˉbytes.Length));
        var Dataˉoffset = Alignˉup(checked(
            Linuxˉhostedˉcompilerˉapplicationˉcontract.TEXT_ADDRESS + Textˉbytes),
            PAGE_BYTES);
        var Runtimeˉlayout = Hostedˉcompilerˉruntimeˉdata.Plan(
            Consoleˉapplicationˉtarget.Linuxˉx64);
        var Expectedˉbytes = checked((int)(Dataˉoffset +
            Linuxˉhostedˉcompilerˉapplicationˉcontract.DATA_FILE_BYTES));
        if (bytes.Length != Expectedˉbytes)
        {
            throw Invalid("The Linux hosted-compiler application has trailing or missing bytes.");
        }

        Requireˉbytes(bytes, 0,
        [
            0x7F, 0x45, 0x4C, 0x46, 0x02, 0x01, 0x01, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        ], "ELF identification");
        Requireˉu16(bytes, 16, 3, "ELF type");
        Requireˉu16(bytes, 18, 62, "ELF machine");
        Requireˉu32(bytes, 20, 1, "ELF version");
        Requireˉu64(bytes, 24,
            Linuxˉhostedˉcompilerˉapplicationˉcontract.TEXT_ADDRESS, "entry address");
        Requireˉu64(bytes, 32, ELF_HEADER_BYTES, "program-header offset");
        Requireˉu64(bytes, 40, 0, "section-header offset");
        Requireˉu32(bytes, 48, 0, "ELF flags");
        Requireˉu16(bytes, 52, ELF_HEADER_BYTES, "ELF header size");
        Requireˉu16(bytes, 54, PROGRAM_HEADER_BYTES, "program-header size");
        Requireˉu16(bytes, 56, PROGRAM_HEADER_COUNT, "program-header count");
        Requireˉzero(bytes, 58, 6, "section-header metadata");

        var Headerˉload = ELF_HEADER_BYTES;
        Verifyˉprogramˉheader(bytes, Headerˉload, 1, 4, 0, 0,
            Linuxˉhostedˉcompilerˉapplicationˉcontract.HEADER_BYTES,
            Linuxˉhostedˉcompilerˉapplicationˉcontract.HEADER_BYTES,
            PAGE_BYTES, "header segment");
        var Textˉload = Headerˉload + PROGRAM_HEADER_BYTES;
        Verifyˉprogramˉheader(bytes, Textˉload, 1, 5,
            Linuxˉhostedˉcompilerˉapplicationˉcontract.HEADER_BYTES,
            Linuxˉhostedˉcompilerˉapplicationˉcontract.TEXT_ADDRESS,
            Textˉbytes, Textˉbytes, PAGE_BYTES, "text segment");
        var Dataˉload = Textˉload + PROGRAM_HEADER_BYTES;
        Verifyˉprogramˉheader(bytes, Dataˉload, 1, 6,
            Dataˉoffset, Dataˉoffset,
            Linuxˉhostedˉcompilerˉapplicationˉcontract.DATA_FILE_BYTES,
            Runtimeˉlayout.Virtualˉbytes, PAGE_BYTES, "data segment");
        var Note = Dataˉload + PROGRAM_HEADER_BYTES;
        Verifyˉprogramˉheader(bytes, Note, 4, 4, NOTE_OFFSET, NOTE_OFFSET,
            NOTE_BYTES, NOTE_BYTES, 4, "Windvale note segment");
        var Stack = Note + PROGRAM_HEADER_BYTES;
        Verifyˉprogramˉheader(bytes, Stack, 0x6474_E551, 6, 0, 0, 0,
            Nativeˉconsoleˉapplicationˉcontract.STACK_BYTES, 16,
            "non-executable stack declaration");

        Requireˉzero(bytes, PROGRAM_HEADER_TABLE_END,
            NOTE_OFFSET - PROGRAM_HEADER_TABLE_END, "program-header padding");
        Requireˉu32(bytes, NOTE_OFFSET + 0, 9, "note name length");
        Requireˉu32(bytes, NOTE_OFFSET + 4, sizeof(uint), "note value length");
        Requireˉu32(bytes, NOTE_OFFSET + 8, 1, "note type");
        Requireˉbytes(bytes, NOTE_OFFSET + 12,
            [0x57, 0x69, 0x6E, 0x64, 0x76, 0x61, 0x6C, 0x65, 0x00],
            "note owner");
        Requireˉzero(bytes, NOTE_OFFSET + 21, 3, "note owner padding");
        Requireˉu32(bytes, NOTE_OFFSET + 24,
            Hostedˉcompilerˉapplicationˉmetadata.Containerˉformat(expectedˉprofile),
            "container format version");
        Requireˉzero(bytes, NOTE_OFFSET + NOTE_BYTES,
            Linuxˉhostedˉcompilerˉapplicationˉcontract.HEADER_BYTES -
                NOTE_OFFSET - NOTE_BYTES,
            "header-page tail");

        var Textˉfile = Linuxˉhostedˉcompilerˉapplicationˉcontract.HEADER_BYTES;
        var Bundleˉfile = Textˉfile +
            Linuxˉhostedˉcompilerˉapplicationˉcontract.BUNDLE_TEXT_OFFSET;
        Requireˉzero(bytes,
            Textˉfile + Linuxˉhostedˉcompilerˉapplicationˉcontract.STARTUP_BYTES,
            Linuxˉhostedˉcompilerˉapplicationˉcontract.BUNDLE_TEXT_OFFSET -
                Linuxˉhostedˉcompilerˉapplicationˉcontract.STARTUP_BYTES,
            "startup-to-bundle padding");
        if (!bytes.Slice(Bundleˉfile, expectedˉbundle.Imageˉbytes.Length)
                .SequenceEqual(expectedˉbundle.Imageˉbytes.AsSpan()))
        {
            throw Invalid("The Linux hosted-compiler service bundle is noncanonical.");
        }
        Requireˉzero(bytes,
            checked(Bundleˉfile + expectedˉbundle.Imageˉbytes.Length),
            checked((int)Dataˉoffset - Bundleˉfile - expectedˉbundle.Imageˉbytes.Length),
            "bundle-to-data padding");

        var Runtime = Hostedˉcompilerˉruntimeˉdata.Verify(
            bytes.Slice(checked((int)Dataˉoffset),
                checked((int)Linuxˉhostedˉcompilerˉapplicationˉcontract.DATA_FILE_BYTES)),
            Consoleˉapplicationˉtarget.Linuxˉx64,
            expectedˉbundle,
            bytes.Slice(Bundleˉfile, expectedˉbundle.Imageˉbytes.Length),
            expectedˉprofile);
        if (Runtime.Metadata.Bundleˉoffset !=
                Linuxˉhostedˉcompilerˉapplicationˉcontract.BUNDLE_TEXT_OFFSET ||
            Runtime.Metadata.Bundleˉbytes != expectedˉbundle.Imageˉbytes.Length)
        {
            throw Invalid("The Linux hosted-compiler bundle metadata is inconsistent.");
        }

        Linuxˉhostedˉcompilerˉstartup.Verify(
            bytes.Slice(Textˉfile,
                Linuxˉhostedˉcompilerˉapplicationˉcontract.STARTUP_BYTES),
            Linuxˉhostedˉcompilerˉapplicationˉcontract.TEXT_ADDRESS,
            Dataˉoffset,
            Runtime.Layout,
            expectedˉbundle,
            Runtime.Metadata.Nativeˉentryˉoffset);

        var Layout = Linuxˉhostedˉcompilerˉapplicationˉcontract.Plan(
            expectedˉbundle,
            Runtime.Metadata.Nativeˉentryˉoffset);
        return new(
            Layout,
            Runtime.Metadata.Nativeˉentryˉoffset,
            bytes.Slice(Bundleˉfile, expectedˉbundle.Imageˉbytes.Length)
                .ToArray().ToImmutableArray(),
            Runtime);
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
        string field)
    {
        Requireˉu32(bytes, offset + 0, type, field + " type");
        Requireˉu32(bytes, offset + 4, flags, field + " flags");
        Requireˉu64(bytes, offset + 8, fileˉoffset, field + " file offset");
        Requireˉu64(bytes, offset + 16, virtualˉaddress, field + " virtual address");
        Requireˉu64(bytes, offset + 24, 0, field + " physical address");
        Requireˉu64(bytes, offset + 32, fileˉbytes, field + " file size");
        Requireˉu64(bytes, offset + 40, memoryˉbytes, field + " memory size");
        Requireˉu64(bytes, offset + 48, alignment, field + " alignment");
    }

    private static uint Alignˉup(uint value, uint alignment) => checked(
        (value + alignment - 1) & ~(alignment - 1));

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
        string field)
    {
        if (!bytes.Slice(offset, expected.Length).SequenceEqual(expected))
        {
            throw Invalid($"The Linux hosted-compiler {field} is invalid.");
        }
    }

    private static void Requireˉu16(
        ReadOnlySpan<byte> bytes,
        int offset,
        ushort expected,
        string field)
    {
        if (Readˉu16(bytes, offset) != expected)
        {
            throw Invalid($"The Linux hosted-compiler {field} is invalid.");
        }
    }

    private static void Requireˉu32(
        ReadOnlySpan<byte> bytes,
        int offset,
        uint expected,
        string field)
    {
        if (Readˉu32(bytes, offset) != expected)
        {
            throw Invalid($"The Linux hosted-compiler {field} is invalid.");
        }
    }

    private static void Requireˉu64(
        ReadOnlySpan<byte> bytes,
        int offset,
        ulong expected,
        string field)
    {
        if (Readˉu64(bytes, offset) != expected)
        {
            throw Invalid($"The Linux hosted-compiler {field} is invalid.");
        }
    }

    private static void Requireˉzero(
        ReadOnlySpan<byte> bytes,
        int offset,
        int length,
        string field)
    {
        if (!bytes.Slice(offset, length).SequenceEqual(new byte[length]))
        {
            throw Invalid($"The Linux hosted-compiler {field} is invalid.");
        }
    }

    private static InvalidDataException Invalid(string message) => new(message);
}
