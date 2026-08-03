using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

public static class Windowsˉconsoleˉapplicationˉwriter
{
    private const int PE_OFFSET = 0x80;
    private const int OPTIONAL_HEADER_OFFSET = 0x98;
    private const int OPTIONAL_HEADER_BYTES = 0xF0;
    private const int SECTION_TABLE_OFFSET = OPTIONAL_HEADER_OFFSET + OPTIONAL_HEADER_BYTES;
    private const uint FILE_ALIGNMENT = 0x200;
    private const uint SECTION_ALIGNMENT = 0x1000;
    private const uint TEXT_RVA = 0x1000;
    private const ulong IMAGE_BASE = 0x0000_0001_4000_0000;
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
            var Portable = Consoleˉapplicationˉverification.Verify(Image.AsSpan());
            if (Portable.Target != Consoleˉapplicationˉtarget.Windowsˉx64 ||
                Portable.Nativeˉentryˉoffset != Input.Entryˉoffset ||
                !Portable.Nativeˉimageˉbytes.AsSpan().SequenceEqual(Input.Imageˉbytes.AsSpan()))
            {
                return Windowsˉconsoleˉapplicationˉresult.Failed(
                    "WVW1004",
                    "The Windvale-owned verifier did not reproduce the Windows native image and entry.");
            }

            var Verified = Windowsˉconsoleˉapplicationˉverifier.Verify(Image.AsSpan());
            if (Verified.Nativeˉentryˉoffset != Input.Entryˉoffset ||
                !Verified.Nativeˉimageˉbytes.AsSpan().SequenceEqual(Input.Imageˉbytes.AsSpan()))
            {
                return Windowsˉconsoleˉapplicationˉresult.Failed(
                    "WVW1004",
                    "The independently verified Windows application did not reproduce the native image and entry.");
            }
        }
        catch (Consoleˉapplicationˉverificationˉexception Exception)
        {
            return Windowsˉconsoleˉapplicationˉresult.Failed(
                "WVW1004",
                $"Windvale-owned Windows application verification failed: {Exception.Message}");
        }
        catch (Windowsˉconsoleˉapplicationˉexception Exception)
        {
            return Windowsˉconsoleˉapplicationˉresult.Failed(
                "WVW1004",
                $"Independent Windows application verification failed: {Exception.Message}");
        }

        return Windowsˉconsoleˉapplicationˉresult.Succeeded(Image.ToImmutableArray());
    }

    public static Windowsˉconsoleˉapplicationˉresult Writeˉhostedˉconsole(
        Nativeˉfragment fragment)
    {
        var Prepared = Nativeˉconsoleˉapplicationˉpreparer.Prepareˉhostedˉconsole(fragment);
        if (!Prepared.Success)
        {
            var Code = Prepared.Failure switch
            {
                Nativeˉconsoleˉapplicationˉinputˉfailure.Unverifiedˉfragment => "WVW1101",
                Nativeˉconsoleˉapplicationˉinputˉfailure.Unsupportedˉentry => "WVW1102",
                Nativeˉconsoleˉapplicationˉinputˉfailure.Linkˉfailure => "WVW1103",
                _ => "WVW1103",
            };
            return Windowsˉconsoleˉapplicationˉresult.Failed(Code, Prepared.Message);
        }

        var Input = Prepared.Input!;
        try
        {
            var Image = Hostedˉconsoleˉapplicationˉbuilder.Buildˉwindows(
                Input.Imageˉbytes.AsSpan(),
                Input.Entryˉoffset);
            var Verified = Windowsˉconsoleˉapplicationˉverifier.Verify(Image);
            if (Verified.Formatˉversion !=
                    Windowsˉconsoleˉapplicationˉcontract.HOSTED_FORMAT_VERSION ||
                !Verified.Requiredˉservices.SequenceEqual(Input.Requiredˉservices) ||
                Verified.Nativeˉentryˉoffset != Input.Entryˉoffset ||
                !Verified.Nativeˉimageˉbytes.AsSpan().SequenceEqual(Input.Imageˉbytes.AsSpan()))
            {
                return Windowsˉconsoleˉapplicationˉresult.Failed(
                    "WVW1104",
                    "The independently verified hosted Windows application did not reproduce its native image, entry, and service manifest.");
            }
            return Windowsˉconsoleˉapplicationˉresult.Succeeded(Image.ToImmutableArray());
        }
        catch (Exception Exception) when (
            Exception is Windowsˉconsoleˉapplicationˉexception or
                InvalidDataException or
                OverflowException or
                ArgumentException)
        {
            return Windowsˉconsoleˉapplicationˉresult.Failed(
                "WVW1104",
                $"Hosted Windows application verification failed: {Exception.Message}");
        }
    }

    public static Windowsˉconsoleˉapplicationˉresult Writeˉhostedˉcompiler(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities)
    {
        try
        {
            Nativeˉfragmentˉverifier.Verify(fragment);
            var Entries = fragment.Symbols
                .Where(Symbol => Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
                    Symbol.Kind == Nativeˉsymbolˉkind.Function &&
                    Symbol.Name == "Main")
                .ToArray();
            if (Entries.Length != 1)
            {
                return Windowsˉconsoleˉapplicationˉresult.Failed(
                    "WVW1201",
                    "The hosted compiler requires exactly one exported Main function.");
            }

            var Bundle = X64ˉnativeˉserviceˉbundle.Build(
                fragment,
                Nativeˉserviceˉplatform.Windows);
            var Image = Windowsˉhostedˉcompilerˉapplicationˉbuilder.Build(
                capabilities,
                Bundle,
                Entries[0].Offset);
            var Verified = Windowsˉhostedˉcompilerˉapplicationˉverifier.Verify(
                Image.AsSpan(),
                Bundle);
            if (Verified.Nativeˉentryˉoffset != Entries[0].Offset ||
                !Verified.Bundleˉimage.AsSpan().SequenceEqual(Bundle.Imageˉbytes.AsSpan()))
            {
                return Windowsˉconsoleˉapplicationˉresult.Failed(
                    "WVW1202",
                    "The independently verified Windows compiler application did not reproduce its entry and service bundle.");
            }
            return Windowsˉconsoleˉapplicationˉresult.Succeeded(Image);
        }
        catch (Exception Exception) when (
            Exception is Nativeˉbackendˉexception or
                InvalidDataException or
                OverflowException or
                ArgumentException)
        {
            return Windowsˉconsoleˉapplicationˉresult.Failed(
                "WVW1202",
                $"Hosted Windows compiler application verification failed: {Exception.Message}");
        }
    }

    public static Windowsˉconsoleˉapplicationˉresult Writeˉhostedˉverifier(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities)
    {
        try
        {
            Nativeˉfragmentˉverifier.Verify(fragment);
            var Entries = fragment.Symbols
                .Where(Symbol => Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
                    Symbol.Kind == Nativeˉsymbolˉkind.Function &&
                    Symbol.Name == "Main")
                .ToArray();
            if (Entries.Length != 1)
            {
                return Windowsˉconsoleˉapplicationˉresult.Failed(
                    "WVW1301",
                    "The hosted verifier requires exactly one exported Main function.");
            }

            var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉverifier(
                fragment,
                Nativeˉserviceˉplatform.Windows);
            var Image = Windowsˉhostedˉverifierˉapplicationˉbuilder.Build(
                capabilities,
                Bundle,
                Entries[0].Offset);
            var Verified = Windowsˉhostedˉverifierˉapplicationˉverifier.Verify(
                Image.AsSpan(),
                Bundle);
            if (Verified.Nativeˉentryˉoffset != Entries[0].Offset ||
                !Verified.Bundleˉimage.AsSpan().SequenceEqual(Bundle.Imageˉbytes.AsSpan()))
            {
                return Windowsˉconsoleˉapplicationˉresult.Failed(
                    "WVW1302",
                    "The independently verified Windows verifier application did not reproduce its entry and service bundle.");
            }
            return Windowsˉconsoleˉapplicationˉresult.Succeeded(Image);
        }
        catch (Exception Exception) when (
            Exception is Nativeˉbackendˉexception or
                InvalidDataException or
                OverflowException or
                ArgumentException)
        {
            return Windowsˉconsoleˉapplicationˉresult.Failed(
                "WVW1302",
                $"Hosted Windows verifier application verification failed: {Exception.Message}");
        }
    }

    private static byte[] Buildˉimage(ReadOnlySpan<byte> nativeˉimage, uint nativeˉentryˉoffset)
    {
        var Layout = Consoleˉapplicationˉlayout.Plan(
            Consoleˉapplicationˉtarget.Windowsˉx64,
            nativeˉimage.Length,
            nativeˉentryˉoffset);
        var Recoveryˉimage = Buildˉrecoveryˉimage(
            nativeˉimage,
            nativeˉentryˉoffset,
            Layout);
        return Consoleˉapplicationˉconstruction.Construct(
            Consoleˉapplicationˉtarget.Windowsˉx64,
            nativeˉimage,
            nativeˉentryˉoffset,
            Layout,
            Recoveryˉimage);
    }

    internal static byte[] Buildˉrecoveryˉimage(
        ReadOnlySpan<byte> nativeˉimage,
        uint nativeˉentryˉoffset,
        Consoleˉapplicationˉplan layout)
    {
        var Result = new byte[layout.Applicationˉbytes];

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
        Writeˉu32(Result, Optional + 4, layout.Textˉfileˉbytes);
        Writeˉu32(
            Result,
            Optional + 8,
            checked(layout.Dataˉfileˉbytes + layout.Metadataˉfileˉbytes));
        Writeˉu32(
            Result,
            Optional + 12,
            checked(layout.Dataˉvirtualˉbytes - layout.Dataˉfileˉbytes));
        Writeˉu32(Result, Optional + 16, TEXT_RVA);
        Writeˉu32(Result, Optional + 20, TEXT_RVA);
        Writeˉu64(Result, Optional + 24, IMAGE_BASE);
        Writeˉu32(Result, Optional + 32, SECTION_ALIGNMENT);
        Writeˉu32(Result, Optional + 36, FILE_ALIGNMENT);
        Writeˉu16(Result, Optional + 40, 6);
        Writeˉu16(Result, Optional + 48, 6);
        Writeˉu32(Result, Optional + 56, layout.Imageˉvirtualˉbytes);
        Writeˉu32(Result, Optional + 60, checked((uint)layout.Headerˉbytes));
        Writeˉu16(Result, Optional + 68, 3);
        Writeˉu16(Result, Optional + 70, 0x0160);
        Writeˉu64(Result, Optional + 72, Windowsˉconsoleˉapplicationˉcontract.STACK_BYTES);
        Writeˉu64(Result, Optional + 80, 0x0001_0000);
        Writeˉu64(Result, Optional + 88, 0x0010_0000);
        Writeˉu64(Result, Optional + 96, 0x0000_1000);
        Writeˉu32(Result, Optional + 108, 16);
        Writeˉu32(Result, Optional + 112 + (5 * 8), layout.Metadataˉvirtualˉaddress);
        Writeˉu32(Result, Optional + 112 + (5 * 8) + 4, layout.Metadataˉvirtualˉbytes);

        Writeˉsectionˉname(Result, SECTION_TABLE_OFFSET, ".text");
        Writeˉu32(Result, SECTION_TABLE_OFFSET + 8, layout.Textˉvirtualˉbytes);
        Writeˉu32(Result, SECTION_TABLE_OFFSET + 12, layout.Textˉvirtualˉaddress);
        Writeˉu32(Result, SECTION_TABLE_OFFSET + 16, layout.Textˉfileˉbytes);
        Writeˉu32(Result, SECTION_TABLE_OFFSET + 20, checked((uint)layout.Textˉfileˉoffset));
        Writeˉu32(Result, SECTION_TABLE_OFFSET + 36, 0x6000_0020);

        var Dataˉsection = SECTION_TABLE_OFFSET + 40;
        Writeˉsectionˉname(Result, Dataˉsection, ".data");
        Writeˉu32(
            Result,
            Dataˉsection + 8,
            layout.Dataˉvirtualˉbytes);
        Writeˉu32(Result, Dataˉsection + 12, layout.Dataˉvirtualˉaddress);
        Writeˉu32(Result, Dataˉsection + 16, layout.Dataˉfileˉbytes);
        Writeˉu32(Result, Dataˉsection + 20, layout.Dataˉfileˉoffset);
        Writeˉu32(Result, Dataˉsection + 36, 0xC000_0040);

        var Relocationˉsection = SECTION_TABLE_OFFSET + 80;
        Writeˉsectionˉname(Result, Relocationˉsection, ".reloc");
        Writeˉu32(Result, Relocationˉsection + 8, layout.Metadataˉvirtualˉbytes);
        Writeˉu32(Result, Relocationˉsection + 12, layout.Metadataˉvirtualˉaddress);
        Writeˉu32(Result, Relocationˉsection + 16, layout.Metadataˉfileˉbytes);
        Writeˉu32(Result, Relocationˉsection + 20, layout.Metadataˉfileˉoffset);
        Writeˉu32(Result, Relocationˉsection + 36, 0x4200_0040);

        Writeˉstartup(
            Result.AsSpan(layout.Textˉfileˉoffset, layout.Startupˉbytes),
            layout.Dataˉvirtualˉaddress,
            nativeˉentryˉoffset);
        nativeˉimage.CopyTo(Result.AsSpan(
            layout.Textˉfileˉoffset + layout.Nativeˉimageˉoffset));

        var Contextˉoffset = checked((int)layout.Dataˉfileˉoffset);
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

        Writeˉu32(Result, (int)layout.Metadataˉfileˉoffset, TEXT_RVA);
        Writeˉu32(Result, (int)layout.Metadataˉfileˉoffset + 4, layout.Metadataˉvirtualˉbytes);
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
