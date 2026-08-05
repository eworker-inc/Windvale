using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Linuxˉhostedˉverifierˉapplicationˉbuilder
{
    private const int ELF_HEADER_BYTES = 64;
    private const int PROGRAM_HEADER_BYTES = 56;
    private const int PROGRAM_HEADER_COUNT = 5;
    private const int NOTE_OFFSET = 0x180;
    private const int NOTE_BYTES = 28;
    private const uint PAGE_BYTES = 0x1000;

    internal static ImmutableArray<byte> Build(
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset,
        Hostedˉverifierˉapplicationˉprofile profile =
            Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier)
    {
        var Layout = Linuxˉhostedˉverifierˉapplicationˉcontract.Plan(
            bundle,
            nativeˉentryˉoffset,
            profile);
        var Runtime = Hostedˉverifierˉruntimeˉdata.Build(
            Consoleˉapplicationˉtarget.Linuxˉx64,
            capabilities,
            bundle,
            nativeˉentryˉoffset,
            profile);
        var Startup = profile switch
        {
            Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier =>
                Linuxˉhostedˉverifierˉstartup.Build(
                    Layout.Textˉaddress,
                    Layout.Dataˉaddress,
                    Hostedˉverifierˉruntimeˉdata.Plan(
                        Consoleˉapplicationˉtarget.Linuxˉx64),
                    bundle,
                    nativeˉentryˉoffset),
            Hostedˉverifierˉapplicationˉprofile.Wvbˉinspector =>
                Linuxˉhostedˉinspectorˉstartup.Build(
                    Layout.Textˉaddress,
                    Layout.Dataˉaddress,
                    Hostedˉverifierˉruntimeˉdata.Plan(
                        Consoleˉapplicationˉtarget.Linuxˉx64),
                    bundle,
                    nativeˉentryˉoffset),
            _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null),
        };
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
        Writeˉu64(Result, 24, Layout.Textˉaddress);
        Writeˉu64(Result, 32, ELF_HEADER_BYTES);
        Writeˉu16(Result, 52, ELF_HEADER_BYTES);
        Writeˉu16(Result, 54, PROGRAM_HEADER_BYTES);
        Writeˉu16(Result, 56, PROGRAM_HEADER_COUNT);

        var Headerˉload = ELF_HEADER_BYTES;
        Writeˉprogramˉheader(Result, Headerˉload, 1, 4, 0, 0,
            checked((ulong)Layout.Headerˉbytes), checked((ulong)Layout.Headerˉbytes), PAGE_BYTES);
        var Textˉload = Headerˉload + PROGRAM_HEADER_BYTES;
        Writeˉprogramˉheader(Result, Textˉload, 1, 5,
            checked((ulong)Layout.Textˉfileˉoffset), Layout.Textˉaddress,
            Layout.Textˉbytes, Layout.Textˉbytes, PAGE_BYTES);
        var Dataˉload = Textˉload + PROGRAM_HEADER_BYTES;
        Writeˉprogramˉheader(Result, Dataˉload, 1, 6,
            Layout.Dataˉfileˉoffset, Layout.Dataˉaddress,
            Layout.Dataˉfileˉbytes, Layout.Dataˉvirtualˉbytes, PAGE_BYTES);
        var Note = Dataˉload + PROGRAM_HEADER_BYTES;
        Writeˉprogramˉheader(Result, Note, 4, 4, NOTE_OFFSET, NOTE_OFFSET,
            NOTE_BYTES, NOTE_BYTES, 4);
        var Stack = Note + PROGRAM_HEADER_BYTES;
        Writeˉprogramˉheader(Result, Stack, 0x6474_E551, 6, 0, 0, 0,
            Nativeˉconsoleˉapplicationˉcontract.STACK_BYTES, 16);

        Writeˉu32(Result, NOTE_OFFSET + 0, 9);
        Writeˉu32(Result, NOTE_OFFSET + 4, sizeof(uint));
        Writeˉu32(Result, NOTE_OFFSET + 8, 1);
        ReadOnlySpan<byte> Noteˉname =
            [0x57, 0x69, 0x6E, 0x64, 0x76, 0x61, 0x6C, 0x65, 0x00];
        Noteˉname.CopyTo(Result.AsSpan(NOTE_OFFSET + 12));
        Writeˉu32(Result, NOTE_OFFSET + 24,
            Linuxˉhostedˉverifierˉapplicationˉcontract.FORMAT_VERSION);

        Startup.AsSpan().CopyTo(Result.AsSpan(Layout.Textˉfileˉoffset));
        bundle.Imageˉbytes.AsSpan().CopyTo(Result.AsSpan(
            Layout.Textˉfileˉoffset + Layout.Bundleˉoffset));
        Runtime.AsSpan().CopyTo(Result.AsSpan(checked((int)Layout.Dataˉfileˉoffset)));
        return Result.ToImmutableArray();
    }

    private static void Writeˉprogramˉheader(
        byte[] bytes,
        int offset,
        uint type,
        uint flags,
        ulong fileˉoffset,
        ulong virtualˉaddress,
        ulong fileˉbytes,
        ulong memoryˉbytes,
        ulong alignment)
    {
        Writeˉu32(bytes, offset + 0, type);
        Writeˉu32(bytes, offset + 4, flags);
        Writeˉu64(bytes, offset + 8, fileˉoffset);
        Writeˉu64(bytes, offset + 16, virtualˉaddress);
        Writeˉu64(bytes, offset + 24, 0);
        Writeˉu64(bytes, offset + 32, fileˉbytes);
        Writeˉu64(bytes, offset + 40, memoryˉbytes);
        Writeˉu64(bytes, offset + 48, alignment);
    }

    private static void Writeˉu16(byte[] bytes, int offset, ushort value) =>
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset, sizeof(ushort)), value);

    private static void Writeˉu32(byte[] bytes, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset, sizeof(uint)), value);

    private static void Writeˉu64(byte[] bytes, int offset, ulong value) =>
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(offset, sizeof(ulong)), value);
}
