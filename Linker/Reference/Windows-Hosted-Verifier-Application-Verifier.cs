using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Windowsˉhostedˉverifierˉapplicationˉverifier
{
    private const int PE_OFFSET = 0x80;
    private const int OPTIONAL = 0x98;
    private const int OPTIONAL_BYTES = 0xF0;
    private const int SECTIONS = 0x188;
    private const uint FILE_ALIGNMENT = 0x200;
    private const uint SECTION_ALIGNMENT = 0x1000;

    internal static Verifiedˉwindowsˉhostedˉverifierˉapplication Verify(
        ReadOnlySpan<byte> bytes,
        Nativeˉserviceˉbundle expectedˉbundle,
        Hostedˉverifierˉapplicationˉprofile expectedˉprofile =
            Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier)
    {
        Windowsˉhostedˉverifierˉapplicationˉcontract.Validateˉbundle(
            expectedˉbundle,
            expectedˉprofile);
        var Startupˉbytes =
            Windowsˉhostedˉverifierˉapplicationˉcontract.Startupˉbytes(expectedˉprofile);
        var Textˉvirtual = checked((uint)(
            Windowsˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
            expectedˉbundle.Imageˉbytes.Length));
        var Textˉfileˉbytes = Alignˉup(Textˉvirtual, FILE_ALIGNMENT);
        var Dataˉfile = checked((uint)
            Windowsˉhostedˉverifierˉapplicationˉcontract.HEADER_BYTES + Textˉfileˉbytes);
        var Dataˉaddress = Alignˉup(checked(
            Windowsˉhostedˉverifierˉapplicationˉcontract.TEXT_ADDRESS + Textˉvirtual),
            SECTION_ALIGNMENT);
        var Runtimeˉlayout = Hostedˉverifierˉruntimeˉdata.Plan(
            Consoleˉapplicationˉtarget.Windowsˉx64,
            expectedˉprofile);
        var Dataˉvirtual = checked(
            Windowsˉhostedˉverifierˉapplicationˉcontract.IMPORT_FILE_BYTES +
            Runtimeˉlayout.Virtualˉbytes);
        var Relocationˉfile = checked(Dataˉfile +
            Windowsˉhostedˉverifierˉapplicationˉcontract.DATA_FILE_BYTES);
        var Relocationˉaddress = Alignˉup(checked(Dataˉaddress + Dataˉvirtual),
            SECTION_ALIGNMENT);
        var Expectedˉbytes = checked((int)(Relocationˉfile +
            Windowsˉhostedˉverifierˉapplicationˉcontract.RELOCATION_FILE_BYTES));
        if (bytes.Length != Expectedˉbytes)
        {
            throw Invalid("The Windows hosted-verifier application has trailing or missing bytes.");
        }

        Requireˉu16(bytes, 0, 0x5A4D, "DOS signature");
        Requireˉzero(bytes, 2, 0x3A, "DOS header");
        Requireˉu32(bytes, 0x3C, PE_OFFSET, "PE header offset");
        Requireˉzero(bytes, 0x40, 0x40, "DOS stub");
        Requireˉu32(bytes, PE_OFFSET, 0x0000_4550, "PE signature");
        var Coff = PE_OFFSET + sizeof(uint);
        Requireˉu16(bytes, Coff + 0, 0x8664, "COFF machine");
        Requireˉu16(bytes, Coff + 2, 3, "section count");
        Requireˉu32(bytes, Coff + 4, 0, "timestamp");
        Requireˉzero(bytes, Coff + 8, 8, "COFF symbol metadata");
        Requireˉu16(bytes, Coff + 16, OPTIONAL_BYTES, "optional-header size");
        Requireˉu16(bytes, Coff + 18, 0x0022, "COFF characteristics");

        Requireˉu16(bytes, OPTIONAL + 0, 0x020B, "PE32+ magic");
        Requireˉbyte(bytes, OPTIONAL + 2,
            Windowsˉhostedˉverifierˉapplicationˉcontract.FORMAT_VERSION,
            "writer version");
        Requireˉbyte(bytes, OPTIONAL + 3, 0, "writer minor version");
        Requireˉu32(bytes, OPTIONAL + 4, Textˉfileˉbytes, "initialized code size");
        Requireˉu32(bytes, OPTIONAL + 8,
            Windowsˉhostedˉverifierˉapplicationˉcontract.DATA_FILE_BYTES +
                Windowsˉhostedˉverifierˉapplicationˉcontract.RELOCATION_FILE_BYTES,
            "initialized data size");
        Requireˉu32(bytes, OPTIONAL + 12,
            Dataˉvirtual - Windowsˉhostedˉverifierˉapplicationˉcontract.DATA_FILE_BYTES,
            "uninitialized data size");
        Requireˉu32(bytes, OPTIONAL + 16,
            Windowsˉhostedˉverifierˉapplicationˉcontract.TEXT_ADDRESS, "entry RVA");
        Requireˉu32(bytes, OPTIONAL + 20,
            Windowsˉhostedˉverifierˉapplicationˉcontract.TEXT_ADDRESS, "code RVA");
        Requireˉu64(bytes, OPTIONAL + 24,
            Windowsˉhostedˉverifierˉapplicationˉcontract.IMAGE_BASE, "image base");
        Requireˉu32(bytes, OPTIONAL + 32, SECTION_ALIGNMENT, "section alignment");
        Requireˉu32(bytes, OPTIONAL + 36, FILE_ALIGNMENT, "file alignment");
        Requireˉu16(bytes, OPTIONAL + 40, 6, "minimum operating-system major version");
        Requireˉzero(bytes, OPTIONAL + 42, 6, "remaining operating-system and image versions");
        Requireˉu16(bytes, OPTIONAL + 48, 6, "minimum subsystem major version");
        Requireˉzero(bytes, OPTIONAL + 50, 6, "minor subsystem and Win32 version");
        var Imageˉbytes = Alignˉup(checked(Relocationˉaddress +
            Windowsˉhostedˉverifierˉapplicationˉcontract.RELOCATION_BYTES),
            SECTION_ALIGNMENT);
        Requireˉu32(bytes, OPTIONAL + 56, Imageˉbytes, "virtual image size");
        Requireˉu32(bytes, OPTIONAL + 60,
            Windowsˉhostedˉverifierˉapplicationˉcontract.HEADER_BYTES, "header size");
        Requireˉu32(bytes, OPTIONAL + 64, 0, "checksum");
        Requireˉu16(bytes, OPTIONAL + 68, 3, "console subsystem");
        Requireˉu16(bytes, OPTIONAL + 70, 0x0160, "DLL characteristics");
        Requireˉu64(bytes, OPTIONAL + 72, Nativeˉconsoleˉapplicationˉcontract.STACK_BYTES,
            "stack reserve");
        Requireˉu64(bytes, OPTIONAL + 80, Nativeˉconsoleˉapplicationˉcontract.STACK_BYTES,
            "stack commit");
        Requireˉu64(bytes, OPTIONAL + 88, 0x0010_0000, "heap reserve");
        Requireˉu64(bytes, OPTIONAL + 96, 0x0000_1000, "heap commit");
        Requireˉu32(bytes, OPTIONAL + 104, 0, "loader flags");
        Requireˉu32(bytes, OPTIONAL + 108, 16, "data-directory count");
        Verifyˉdirectories(bytes, Dataˉaddress, Relocationˉaddress);

        Verifyˉsection(bytes, SECTIONS, ".text", Textˉvirtual,
            Windowsˉhostedˉverifierˉapplicationˉcontract.TEXT_ADDRESS,
            Textˉfileˉbytes,
            Windowsˉhostedˉverifierˉapplicationˉcontract.HEADER_BYTES,
            0x6000_0020);
        Verifyˉsection(bytes, SECTIONS + 40, ".data", Dataˉvirtual,
            Dataˉaddress,
            Windowsˉhostedˉverifierˉapplicationˉcontract.DATA_FILE_BYTES,
            Dataˉfile,
            0xC000_0040);
        Verifyˉsection(bytes, SECTIONS + 80, ".reloc",
            Windowsˉhostedˉverifierˉapplicationˉcontract.RELOCATION_BYTES,
            Relocationˉaddress,
            Windowsˉhostedˉverifierˉapplicationˉcontract.RELOCATION_FILE_BYTES,
            Relocationˉfile,
            0x4200_0040);

        var Textˉfile = Windowsˉhostedˉverifierˉapplicationˉcontract.HEADER_BYTES;
        var Bundleˉfile = Textˉfile +
            Windowsˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET;
        Requireˉzero(bytes,
            Textˉfile + Startupˉbytes,
            Windowsˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET -
                Startupˉbytes,
            "startup-to-bundle padding");
        if (!bytes.Slice(Bundleˉfile, expectedˉbundle.Imageˉbytes.Length)
                .SequenceEqual(expectedˉbundle.Imageˉbytes.AsSpan()))
        {
            throw Invalid("The Windows hosted-verifier service bundle is noncanonical.");
        }
        Requireˉzero(bytes,
            Bundleˉfile + expectedˉbundle.Imageˉbytes.Length,
            checked((int)Textˉfileˉbytes -
                Windowsˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET -
                expectedˉbundle.Imageˉbytes.Length),
            "text raw padding");

        Windowsˉhostedˉverifierˉimports.Verify(
            bytes.Slice(checked((int)Dataˉfile),
                Windowsˉhostedˉverifierˉimports.PAGE_BYTES),
            Dataˉaddress);
        var Runtimeˉfile = checked(Dataˉfile +
            Windowsˉhostedˉverifierˉapplicationˉcontract.IMPORT_FILE_BYTES);
        var Runtimeˉaddress = checked(Dataˉaddress +
            Windowsˉhostedˉverifierˉapplicationˉcontract.IMPORT_FILE_BYTES);
        var Runtime = Hostedˉverifierˉruntimeˉdata.Verify(
            bytes.Slice(checked((int)Runtimeˉfile),
                checked((int)Windowsˉhostedˉverifierˉapplicationˉcontract.RUNTIME_FILE_BYTES)),
            Consoleˉapplicationˉtarget.Windowsˉx64,
            expectedˉbundle,
            bytes.Slice(Bundleˉfile, expectedˉbundle.Imageˉbytes.Length),
            expectedˉprofile);
        if (Runtime.Metadata.Bundleˉoffset !=
                Windowsˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET ||
            Runtime.Metadata.Bundleˉbytes != expectedˉbundle.Imageˉbytes.Length)
        {
            throw Invalid("The Windows hosted-verifier bundle metadata is inconsistent.");
        }
        if (expectedˉprofile ==
            Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier)
        {
            Windowsˉhostedˉverifierˉstartup.Verify(
                bytes.Slice(Textˉfile, Startupˉbytes),
                Windowsˉhostedˉverifierˉapplicationˉcontract.TEXT_ADDRESS,
                Dataˉaddress,
                Runtimeˉaddress,
                Runtime.Layout,
                expectedˉbundle,
                Runtime.Metadata.Nativeˉentryˉoffset);
        }
        else
        {
            Windowsˉhostedˉinspectorˉstartup.Verify(
                bytes.Slice(Textˉfile, Startupˉbytes),
                Windowsˉhostedˉverifierˉapplicationˉcontract.TEXT_ADDRESS,
                Dataˉaddress,
                Runtimeˉaddress,
                Runtime.Layout,
                expectedˉbundle,
                Runtime.Metadata.Nativeˉentryˉoffset,
                expectedˉprofile);
        }

        Requireˉu32(bytes, checked((int)Relocationˉfile),
            Windowsˉhostedˉverifierˉapplicationˉcontract.TEXT_ADDRESS,
            "relocation page RVA");
        Requireˉu32(bytes, checked((int)Relocationˉfile + 4),
            Windowsˉhostedˉverifierˉapplicationˉcontract.RELOCATION_BYTES,
            "relocation block size");
        Requireˉu32(bytes, checked((int)Relocationˉfile + 8), 0,
            "absolute relocation padding");
        Requireˉzero(bytes,
            checked((int)(Relocationˉfile +
                Windowsˉhostedˉverifierˉapplicationˉcontract.RELOCATION_BYTES)),
            checked((int)(
                Windowsˉhostedˉverifierˉapplicationˉcontract.RELOCATION_FILE_BYTES -
                Windowsˉhostedˉverifierˉapplicationˉcontract.RELOCATION_BYTES)),
            "relocation raw padding");

        var Layout = Windowsˉhostedˉverifierˉapplicationˉcontract.Plan(
            expectedˉbundle,
            Runtime.Metadata.Nativeˉentryˉoffset,
            expectedˉprofile);
        return new(
            Layout,
            Runtime.Metadata.Nativeˉentryˉoffset,
            bytes.Slice(Bundleˉfile, expectedˉbundle.Imageˉbytes.Length)
                .ToArray().ToImmutableArray(),
            Runtime);
    }

    private static void Verifyˉdirectories(
        ReadOnlySpan<byte> bytes,
        uint dataˉaddress,
        uint relocationˉaddress)
    {
        for (var Index = 0; Index < 16; Index++)
        {
            var Offset = OPTIONAL + 112 + Index * 8;
            if (Index == 1)
            {
                Requireˉu32(bytes, Offset,
                    dataˉaddress + Windowsˉhostedˉverifierˉimports.DIRECTORY_OFFSET,
                    "import directory RVA");
                Requireˉu32(bytes, Offset + 4,
                    Windowsˉhostedˉverifierˉimports.DIRECTORY_BYTES,
                    "import directory size");
            }
            else if (Index == 5)
            {
                Requireˉu32(bytes, Offset, relocationˉaddress, "relocation directory RVA");
                Requireˉu32(bytes, Offset + 4,
                    Windowsˉhostedˉverifierˉapplicationˉcontract.RELOCATION_BYTES,
                    "relocation directory size");
            }
            else if (Index == 12)
            {
                Requireˉu32(bytes, Offset,
                    dataˉaddress + Windowsˉhostedˉverifierˉimports.KERNEL_IAT_OFFSET,
                    "IAT directory RVA");
                Requireˉu32(bytes, Offset + 4,
                    Windowsˉhostedˉverifierˉimports.IAT_BYTES,
                    "IAT directory size");
            }
            else
            {
                Requireˉzero(bytes, Offset, 8, "unused data directory");
            }
        }
    }

    private static void Verifyˉsection(
        ReadOnlySpan<byte> bytes,
        int offset,
        string name,
        uint virtualˉbytes,
        uint address,
        uint fileˉbytes,
        uint fileˉoffset,
        uint characteristics)
    {
        Requireˉname(bytes, offset, name);
        Requireˉu32(bytes, offset + 8, virtualˉbytes, name + " virtual size");
        Requireˉu32(bytes, offset + 12, address, name + " RVA");
        Requireˉu32(bytes, offset + 16, fileˉbytes, name + " raw size");
        Requireˉu32(bytes, offset + 20, fileˉoffset, name + " file offset");
        Requireˉzero(bytes, offset + 24, 12, name + " object metadata");
        Requireˉu32(bytes, offset + 36, characteristics, name + " characteristics");
    }

    private static void Requireˉname(ReadOnlySpan<byte> bytes, int offset, string expected)
    {
        Span<byte> Name = stackalloc byte[8];
        for (var Index = 0; Index < expected.Length; Index++)
        {
            Name[Index] = checked((byte)expected[Index]);
        }
        if (!bytes.Slice(offset, Name.Length).SequenceEqual(Name))
        {
            throw Invalid($"The Windows hosted-verifier section '{expected}' is invalid.");
        }
    }

    private static uint Alignˉup(uint value, uint alignment) => checked(
        (value + alignment - 1) & ~(alignment - 1));

    private static void Requireˉbyte(
        ReadOnlySpan<byte> bytes,
        int offset,
        byte expected,
        string field)
    {
        if (bytes[offset] != expected)
        {
            throw Invalid($"The Windows hosted-verifier {field} is invalid.");
        }
    }

    private static void Requireˉu16(
        ReadOnlySpan<byte> bytes,
        int offset,
        ushort expected,
        string field)
    {
        if (BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(offset, sizeof(ushort))) != expected)
        {
            throw Invalid($"The Windows hosted-verifier {field} is invalid.");
        }
    }

    private static void Requireˉu32(
        ReadOnlySpan<byte> bytes,
        int offset,
        uint expected,
        string field)
    {
        if (BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(offset, sizeof(uint))) != expected)
        {
            throw Invalid($"The Windows hosted-verifier {field} is invalid.");
        }
    }

    private static void Requireˉu64(
        ReadOnlySpan<byte> bytes,
        int offset,
        ulong expected,
        string field)
    {
        if (BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(offset, sizeof(ulong))) != expected)
        {
            throw Invalid($"The Windows hosted-verifier {field} is invalid.");
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
            throw Invalid($"The Windows hosted-verifier {field} is invalid.");
        }
    }

    private static InvalidDataException Invalid(string message) => new(message);
}
