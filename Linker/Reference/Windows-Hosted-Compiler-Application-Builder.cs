using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Windowsˉhostedˉcompilerˉapplicationˉbuilder
{
    private const int PE_OFFSET = 0x80;
    private const int OPTIONAL_HEADER_OFFSET = 0x98;
    private const int OPTIONAL_HEADER_BYTES = 0xF0;
    private const int SECTION_TABLE_OFFSET = 0x188;
    private const uint FILE_ALIGNMENT = 0x200;
    private const uint SECTION_ALIGNMENT = 0x1000;

    internal static ImmutableArray<byte> Build(
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset,
        Hostedˉcompilerˉapplicationˉprofile profile =
            Hostedˉcompilerˉapplicationˉprofile.Compiler)
    {
        var Layout = Windowsˉhostedˉcompilerˉapplicationˉcontract.Plan(
            bundle,
            nativeˉentryˉoffset);
        var Runtimeˉlayout = Hostedˉcompilerˉruntimeˉdata.Plan(
            Consoleˉapplicationˉtarget.Windowsˉx64);
        var Runtime = Hostedˉcompilerˉruntimeˉdata.Build(
            Consoleˉapplicationˉtarget.Windowsˉx64,
            capabilities,
            bundle,
            nativeˉentryˉoffset,
            profile);
        var Imports = Windowsˉhostedˉcompilerˉimports.Build(Layout.Importˉaddress);
        var Startup = Windowsˉhostedˉcompilerˉstartup.Build(
            Layout.Textˉaddress,
            Layout.Importˉaddress,
            Layout.Runtimeˉaddress,
            Runtimeˉlayout,
            bundle,
            nativeˉentryˉoffset);
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
        Result[Optional + 2] = checked((byte)
            Hostedˉcompilerˉapplicationˉmetadata.Containerˉformat(profile));
        Writeˉu32(Result, Optional + 4, Layout.Textˉfileˉbytes);
        Writeˉu32(Result, Optional + 8,
            Layout.Dataˉfileˉbytes +
                Windowsˉhostedˉcompilerˉapplicationˉcontract.RELOCATION_FILE_BYTES);
        Writeˉu32(Result, Optional + 12,
            Layout.Dataˉvirtualˉbytes - Layout.Dataˉfileˉbytes);
        Writeˉu32(Result, Optional + 16, Layout.Textˉaddress);
        Writeˉu32(Result, Optional + 20, Layout.Textˉaddress);
        Writeˉu64(Result, Optional + 24,
            Windowsˉhostedˉcompilerˉapplicationˉcontract.IMAGE_BASE);
        Writeˉu32(Result, Optional + 32, SECTION_ALIGNMENT);
        Writeˉu32(Result, Optional + 36, FILE_ALIGNMENT);
        Writeˉu16(Result, Optional + 40, 6);
        Writeˉu16(Result, Optional + 48, 6);
        Writeˉu32(Result, Optional + 56, Layout.Imageˉvirtualˉbytes);
        Writeˉu32(Result, Optional + 60,
            checked((uint)Windowsˉhostedˉcompilerˉapplicationˉcontract.HEADER_BYTES));
        Writeˉu16(Result, Optional + 68, 3);
        Writeˉu16(Result, Optional + 70, 0x0160);
        Writeˉu64(Result, Optional + 72, Nativeˉconsoleˉapplicationˉcontract.STACK_BYTES);
        Writeˉu64(Result, Optional + 80, Nativeˉconsoleˉapplicationˉcontract.STACK_BYTES);
        Writeˉu64(Result, Optional + 88, 0x0010_0000);
        Writeˉu64(Result, Optional + 96, 0x0000_1000);
        Writeˉu32(Result, Optional + 108, 16);
        Writeˉu32(Result, Optional + 120,
            Layout.Importˉaddress + Windowsˉhostedˉcompilerˉimports.DIRECTORY_OFFSET);
        Writeˉu32(Result, Optional + 124,
            Windowsˉhostedˉcompilerˉimports.DIRECTORY_BYTES);
        Writeˉu32(Result, Optional + 152, Layout.Relocationˉaddress);
        Writeˉu32(Result, Optional + 156,
            Windowsˉhostedˉcompilerˉapplicationˉcontract.RELOCATION_BYTES);
        Writeˉu32(Result, Optional + 208,
            Layout.Importˉaddress + Windowsˉhostedˉcompilerˉimports.KERNEL_IAT_OFFSET);
        Writeˉu32(Result, Optional + 212, Windowsˉhostedˉcompilerˉimports.IAT_BYTES);

        Writeˉsection(Result, SECTION_TABLE_OFFSET, ".text",
            Layout.Textˉvirtualˉbytes, Layout.Textˉaddress,
            Layout.Textˉfileˉbytes, checked((uint)Layout.Textˉfileˉoffset), 0x6000_0020);
        Writeˉsection(Result, SECTION_TABLE_OFFSET + 40, ".data",
            Layout.Dataˉvirtualˉbytes, Layout.Dataˉsectionˉaddress,
            Layout.Dataˉfileˉbytes, Layout.Dataˉfileˉoffset, 0xC000_0040);
        Writeˉsection(Result, SECTION_TABLE_OFFSET + 80, ".reloc",
            Windowsˉhostedˉcompilerˉapplicationˉcontract.RELOCATION_BYTES,
            Layout.Relocationˉaddress,
            Windowsˉhostedˉcompilerˉapplicationˉcontract.RELOCATION_FILE_BYTES,
            Layout.Relocationˉfileˉoffset, 0x4200_0040);

        Startup.AsSpan().CopyTo(Result.AsSpan(Layout.Textˉfileˉoffset));
        bundle.Imageˉbytes.AsSpan().CopyTo(Result.AsSpan(
            Layout.Textˉfileˉoffset + Layout.Bundleˉoffset));
        Imports.AsSpan().CopyTo(Result.AsSpan(checked((int)Layout.Importˉfileˉoffset)));
        Runtime.AsSpan().CopyTo(Result.AsSpan(checked((int)Layout.Runtimeˉfileˉoffset)));
        Writeˉu32(Result, checked((int)Layout.Relocationˉfileˉoffset), Layout.Textˉaddress);
        Writeˉu32(Result, checked((int)Layout.Relocationˉfileˉoffset + 4),
            Windowsˉhostedˉcompilerˉapplicationˉcontract.RELOCATION_BYTES);
        return Result.ToImmutableArray();
    }

    private static void Writeˉsection(
        byte[] bytes,
        int offset,
        string name,
        uint virtualˉbytes,
        uint address,
        uint fileˉbytes,
        uint fileˉoffset,
        uint characteristics)
    {
        Writeˉascii(bytes, offset, name, terminate: false);
        Writeˉu32(bytes, offset + 8, virtualˉbytes);
        Writeˉu32(bytes, offset + 12, address);
        Writeˉu32(bytes, offset + 16, fileˉbytes);
        Writeˉu32(bytes, offset + 20, fileˉoffset);
        Writeˉu32(bytes, offset + 36, characteristics);
    }

    private static void Writeˉascii(
        byte[] bytes,
        int offset,
        string value,
        bool terminate = true)
    {
        for (var Index = 0; Index < value.Length; Index++)
        {
            bytes[offset + Index] = checked((byte)value[Index]);
        }
        if (terminate)
        {
            bytes[offset + value.Length] = 0;
        }
    }

    private static void Writeˉu16(byte[] bytes, int offset, ushort value) =>
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset, sizeof(ushort)), value);

    private static void Writeˉu32(byte[] bytes, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset, sizeof(uint)), value);

    private static void Writeˉu64(byte[] bytes, int offset, ulong value) =>
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(offset, sizeof(ulong)), value);
}
