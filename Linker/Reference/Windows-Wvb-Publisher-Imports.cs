using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Text;

namespace Windvale.Linker;

// Fixed publisher-only Win32 import page. Keeping this separate prevents the
// narrow mutation adapter from expanding the ordinary hosted-verifier profile.
internal static class Windowsˉwvbˉpublisherˉimports
{
    internal const int PAGE_BYTES = 4096;
    internal const int DIRECTORY_OFFSET = 0;
    internal const int DIRECTORY_BYTES = 80;
    internal const int KERNEL_LOOKUP_OFFSET = 80;
    internal const int NTDLL_LOOKUP_OFFSET = 208;
    internal const int SHELL_LOOKUP_OFFSET = 224;
    internal const int KERNEL_IAT_OFFSET = 240;
    internal const int NTDLL_IAT_OFFSET = 368;
    internal const int SHELL_IAT_OFFSET = 384;
    internal const int IAT_BYTES = 160;
    internal const int KERNEL_LIBRARY_NAME_OFFSET = 724;
    internal const int NTDLL_LIBRARY_NAME_OFFSET = 738;
    internal const int SHELL_LIBRARY_NAME_OFFSET = 748;

    internal const int CLOSE_HANDLE_IAT_OFFSET = KERNEL_IAT_OFFSET + 0;
    internal const int CREATE_FILE_IAT_OFFSET = KERNEL_IAT_OFFSET + 8;
    internal const int FLUSH_FILE_BUFFERS_IAT_OFFSET = KERNEL_IAT_OFFSET + 16;
    internal const int GET_COMMAND_LINE_IAT_OFFSET = KERNEL_IAT_OFFSET + 24;
    internal const int GET_FILE_INFORMATION_IAT_OFFSET = KERNEL_IAT_OFFSET + 32;
    internal const int GET_FILE_SIZE_IAT_OFFSET = KERNEL_IAT_OFFSET + 40;
    internal const int GET_LAST_ERROR_IAT_OFFSET = KERNEL_IAT_OFFSET + 48;
    internal const int GET_STD_HANDLE_IAT_OFFSET = KERNEL_IAT_OFFSET + 56;
    internal const int LOCAL_FREE_IAT_OFFSET = KERNEL_IAT_OFFSET + 64;
    internal const int MULTI_BYTE_TO_WIDE_CHAR_IAT_OFFSET = KERNEL_IAT_OFFSET + 72;
    internal const int READ_FILE_IAT_OFFSET = KERNEL_IAT_OFFSET + 80;
    internal const int SET_FILE_INFORMATION_IAT_OFFSET = KERNEL_IAT_OFFSET + 88;
    internal const int SET_FILE_POINTER_IAT_OFFSET = KERNEL_IAT_OFFSET + 96;
    internal const int WIDE_CHAR_TO_MULTI_BYTE_IAT_OFFSET = KERNEL_IAT_OFFSET + 104;
    internal const int WRITE_FILE_IAT_OFFSET = KERNEL_IAT_OFFSET + 112;
    internal const int NT_SET_FILE_INFORMATION_IAT_OFFSET = NTDLL_IAT_OFFSET;
    internal const int COMMAND_LINE_TO_ARGV_IAT_OFFSET = SHELL_IAT_OFFSET;

    private static readonly ImmutableArray<Importˉname> KERNEL_IMPORTS =
    [
        new("CloseHandle", 400),
        new("CreateFileW", 414),
        new("FlushFileBuffers", 428),
        new("GetCommandLineW", 448),
        new("GetFileInformationByHandle", 466),
        new("GetFileSizeEx", 496),
        new("GetLastError", 512),
        new("GetStdHandle", 528),
        new("LocalFree", 544),
        new("MultiByteToWideChar", 556),
        new("ReadFile", 578),
        new("SetFileInformationByHandle", 590),
        new("SetFilePointerEx", 620),
        new("WideCharToMultiByte", 640),
        new("WriteFile", 662),
    ];
    private static readonly Importˉname NTDLL_IMPORT =
        new("NtSetInformationFile", 674);
    private static readonly Importˉname SHELL_IMPORT =
        new("CommandLineToArgvW", 702);

    internal static ImmutableArray<byte> Build(uint importˉaddress)
    {
        if (importˉaddress % PAGE_BYTES != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(importˉaddress),
                "The Windows WVB publisher import page must be page aligned.");
        }

        var Bytes = new byte[PAGE_BYTES];
        Writeˉdescriptor(
            Bytes,
            DIRECTORY_OFFSET,
            checked(importˉaddress + KERNEL_LOOKUP_OFFSET),
            checked(importˉaddress + KERNEL_LIBRARY_NAME_OFFSET),
            checked(importˉaddress + KERNEL_IAT_OFFSET));
        Writeˉdescriptor(
            Bytes,
            DIRECTORY_OFFSET + 20,
            checked(importˉaddress + NTDLL_LOOKUP_OFFSET),
            checked(importˉaddress + NTDLL_LIBRARY_NAME_OFFSET),
            checked(importˉaddress + NTDLL_IAT_OFFSET));
        Writeˉdescriptor(
            Bytes,
            DIRECTORY_OFFSET + 40,
            checked(importˉaddress + SHELL_LOOKUP_OFFSET),
            checked(importˉaddress + SHELL_LIBRARY_NAME_OFFSET),
            checked(importˉaddress + SHELL_IAT_OFFSET));

        for (var Index = 0; Index < KERNEL_IMPORTS.Length; Index++)
        {
            var Address = checked(importˉaddress + (uint)KERNEL_IMPORTS[Index].Offset);
            Writeˉu64(Bytes, KERNEL_LOOKUP_OFFSET + Index * sizeof(ulong), Address);
            Writeˉu64(Bytes, KERNEL_IAT_OFFSET + Index * sizeof(ulong), Address);
            Writeˉhintˉname(Bytes, KERNEL_IMPORTS[Index]);
        }
        var Ntdllˉaddress = checked(importˉaddress + (uint)NTDLL_IMPORT.Offset);
        Writeˉu64(Bytes, NTDLL_LOOKUP_OFFSET, Ntdllˉaddress);
        Writeˉu64(Bytes, NTDLL_IAT_OFFSET, Ntdllˉaddress);
        Writeˉhintˉname(Bytes, NTDLL_IMPORT);
        var Shellˉaddress = checked(importˉaddress + (uint)SHELL_IMPORT.Offset);
        Writeˉu64(Bytes, SHELL_LOOKUP_OFFSET, Shellˉaddress);
        Writeˉu64(Bytes, SHELL_IAT_OFFSET, Shellˉaddress);
        Writeˉhintˉname(Bytes, SHELL_IMPORT);
        Writeˉascii(Bytes, KERNEL_LIBRARY_NAME_OFFSET, "KERNEL32.dll");
        Writeˉascii(Bytes, NTDLL_LIBRARY_NAME_OFFSET, "ntdll.dll");
        Writeˉascii(Bytes, SHELL_LIBRARY_NAME_OFFSET, "SHELL32.dll");
        return Bytes.ToImmutableArray();
    }

    internal static void Verify(ReadOnlySpan<byte> bytes, uint importˉaddress)
    {
        if (bytes.Length != PAGE_BYTES ||
            !bytes.SequenceEqual(Build(importˉaddress).AsSpan()))
        {
            throw new InvalidDataException(
                "The Windows WVB publisher import page is not canonical.");
        }
    }

    private static void Writeˉdescriptor(
        byte[] bytes,
        int offset,
        uint lookup,
        uint name,
        uint iat)
    {
        Writeˉu32(bytes, offset, lookup);
        Writeˉu32(bytes, offset + 12, name);
        Writeˉu32(bytes, offset + 16, iat);
    }

    private static void Writeˉhintˉname(byte[] bytes, Importˉname import)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(import.Offset), 0);
        Writeˉascii(bytes, import.Offset + sizeof(ushort), import.Name);
    }

    private static void Writeˉascii(byte[] bytes, int offset, string value)
    {
        Encoding.ASCII.GetBytes(value).CopyTo(bytes.AsSpan(offset));
        bytes[offset + value.Length] = 0;
    }

    private static void Writeˉu32(byte[] bytes, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset), value);

    private static void Writeˉu64(byte[] bytes, int offset, ulong value) =>
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(offset), value);

    private sealed record Importˉname(string Name, int Offset);
}
