using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Text;

namespace Windvale.Linker;

internal static class Windowsˉhostedˉverifierˉimports
{
    internal const int PAGE_BYTES = 4096;
    internal const int DIRECTORY_OFFSET = 0;
    internal const int DIRECTORY_BYTES = 60;
    internal const int KERNEL_LOOKUP_OFFSET = 64;
    internal const int SHELL_LOOKUP_OFFSET = 176;
    internal const int KERNEL_IAT_OFFSET = 192;
    internal const int SHELL_IAT_OFFSET = 304;
    internal const int IAT_BYTES = 128;
    internal const int KERNEL_LIBRARY_NAME_OFFSET = 536;
    internal const int SHELL_LIBRARY_NAME_OFFSET = 549;

    internal const int CLOSE_HANDLE_IAT_OFFSET = KERNEL_IAT_OFFSET + 0;
    internal const int CREATE_FILE_IAT_OFFSET = KERNEL_IAT_OFFSET + 8;
    internal const int EXIT_PROCESS_IAT_OFFSET = KERNEL_IAT_OFFSET + 88;
    internal const int GET_COMMAND_LINE_IAT_OFFSET = KERNEL_IAT_OFFSET + 16;
    internal const int GET_FILE_SIZE_IAT_OFFSET = KERNEL_IAT_OFFSET + 24;
    internal const int GET_LAST_ERROR_IAT_OFFSET = KERNEL_IAT_OFFSET + 32;
    internal const int GET_STD_HANDLE_IAT_OFFSET = KERNEL_IAT_OFFSET + 40;
    internal const int LOCAL_FREE_IAT_OFFSET = KERNEL_IAT_OFFSET + 48;
    internal const int MULTI_BYTE_TO_WIDE_CHAR_IAT_OFFSET = KERNEL_IAT_OFFSET + 56;
    internal const int READ_FILE_IAT_OFFSET = KERNEL_IAT_OFFSET + 64;
    internal const int WIDE_CHAR_TO_MULTI_BYTE_IAT_OFFSET = KERNEL_IAT_OFFSET + 72;
    internal const int WRITE_FILE_IAT_OFFSET = KERNEL_IAT_OFFSET + 80;
    internal const int COMMAND_LINE_TO_ARGV_IAT_OFFSET = SHELL_IAT_OFFSET;

    private static readonly ImmutableArray<Importˉname> KERNEL_IMPORTS =
    [
        new("CloseHandle", 320),
        new("CreateFileW", 334),
        new("GetCommandLineW", 368),
        new("GetFileSizeEx", 386),
        new("GetLastError", 402),
        new("GetStdHandle", 418),
        new("LocalFree", 434),
        new("MultiByteToWideChar", 446),
        new("ReadFile", 468),
        new("WideCharToMultiByte", 480),
        new("WriteFile", 502),
        new("ExitProcess", 562),
    ];
    private static readonly Importˉname SHELL_IMPORT =
        new("CommandLineToArgvW", 514);

    internal static ImmutableArray<byte> Build(uint importˉaddress)
    {
        if (importˉaddress % PAGE_BYTES != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(importˉaddress),
                "The Windows hosted-verifier import page must be page aligned.");
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
            checked(importˉaddress + SHELL_LOOKUP_OFFSET),
            checked(importˉaddress + SHELL_LIBRARY_NAME_OFFSET),
            checked(importˉaddress + SHELL_IAT_OFFSET));

        for (var Index = 0; Index < KERNEL_IMPORTS.Length; Index++)
        {
            var Nameˉaddress = checked(importˉaddress + (uint)KERNEL_IMPORTS[Index].Offset);
            Writeˉu64(Bytes, KERNEL_LOOKUP_OFFSET + Index * sizeof(ulong), Nameˉaddress);
            Writeˉu64(Bytes, KERNEL_IAT_OFFSET + Index * sizeof(ulong), Nameˉaddress);
            Writeˉhintˉname(Bytes, KERNEL_IMPORTS[Index]);
        }
        var Shellˉnameˉaddress = checked(importˉaddress + (uint)SHELL_IMPORT.Offset);
        Writeˉu64(Bytes, SHELL_LOOKUP_OFFSET, Shellˉnameˉaddress);
        Writeˉu64(Bytes, SHELL_IAT_OFFSET, Shellˉnameˉaddress);
        Writeˉhintˉname(Bytes, SHELL_IMPORT);
        Writeˉascii(Bytes, KERNEL_LIBRARY_NAME_OFFSET, "KERNEL32.dll");
        Writeˉascii(Bytes, SHELL_LIBRARY_NAME_OFFSET, "SHELL32.dll");
        return Bytes.ToImmutableArray();
    }

    internal static void Verify(ReadOnlySpan<byte> bytes, uint importˉaddress)
    {
        if (bytes.Length != PAGE_BYTES || importˉaddress % PAGE_BYTES != 0)
        {
            throw Invalid("The Windows hosted-verifier import page has an invalid extent.");
        }
        Verifyˉdescriptor(
            bytes,
            DIRECTORY_OFFSET,
            checked(importˉaddress + KERNEL_LOOKUP_OFFSET),
            checked(importˉaddress + KERNEL_LIBRARY_NAME_OFFSET),
            checked(importˉaddress + KERNEL_IAT_OFFSET));
        Verifyˉdescriptor(
            bytes,
            DIRECTORY_OFFSET + 20,
            checked(importˉaddress + SHELL_LOOKUP_OFFSET),
            checked(importˉaddress + SHELL_LIBRARY_NAME_OFFSET),
            checked(importˉaddress + SHELL_IAT_OFFSET));
        Requireˉzero(bytes, DIRECTORY_OFFSET + 40, 20, "terminating import descriptor");
        Requireˉzero(bytes, DIRECTORY_OFFSET + 60, 4, "descriptor alignment");

        for (var Index = 0; Index < KERNEL_IMPORTS.Length; Index++)
        {
            var Expected = checked(importˉaddress + (uint)KERNEL_IMPORTS[Index].Offset);
            Requireˉu64(bytes, KERNEL_LOOKUP_OFFSET + Index * sizeof(ulong), Expected,
                "kernel lookup entry");
            Requireˉu64(bytes, KERNEL_IAT_OFFSET + Index * sizeof(ulong), Expected,
                "kernel import-address entry");
            Verifyˉhintˉname(bytes, KERNEL_IMPORTS[Index]);
        }
        Requireˉzero(bytes,
            KERNEL_LOOKUP_OFFSET + KERNEL_IMPORTS.Length * sizeof(ulong),
            sizeof(ulong), "kernel lookup terminator");
        Requireˉzero(bytes,
            KERNEL_IAT_OFFSET + KERNEL_IMPORTS.Length * sizeof(ulong),
            sizeof(ulong), "kernel IAT terminator");
        var Shellˉexpected = checked(importˉaddress + (uint)SHELL_IMPORT.Offset);
        Requireˉu64(bytes, SHELL_LOOKUP_OFFSET, Shellˉexpected, "shell lookup entry");
        Requireˉzero(bytes, SHELL_LOOKUP_OFFSET + sizeof(ulong), sizeof(ulong),
            "shell lookup terminator");
        Requireˉu64(bytes, SHELL_IAT_OFFSET, Shellˉexpected, "shell IAT entry");
        Requireˉzero(bytes, SHELL_IAT_OFFSET + sizeof(ulong), sizeof(ulong),
            "shell IAT terminator");
        Verifyˉhintˉname(bytes, SHELL_IMPORT);
        Requireˉascii(bytes, KERNEL_LIBRARY_NAME_OFFSET, "KERNEL32.dll");
        Requireˉascii(bytes, SHELL_LIBRARY_NAME_OFFSET, "SHELL32.dll");

        var Expectedˉbytes = Build(importˉaddress);
        if (!bytes.SequenceEqual(Expectedˉbytes.AsSpan()))
        {
            throw Invalid("The Windows hosted-verifier import page has noncanonical padding.");
        }
    }

    private static void Writeˉdescriptor(
        byte[] bytes,
        int offset,
        uint lookup,
        uint name,
        uint iat)
    {
        Writeˉu32(bytes, offset + 0, lookup);
        Writeˉu32(bytes, offset + 12, name);
        Writeˉu32(bytes, offset + 16, iat);
    }

    private static void Verifyˉdescriptor(
        ReadOnlySpan<byte> bytes,
        int offset,
        uint lookup,
        uint name,
        uint iat)
    {
        Requireˉu32(bytes, offset + 0, lookup, "import lookup address");
        Requireˉzero(bytes, offset + 4, 8, "import descriptor timestamp and forwarder");
        Requireˉu32(bytes, offset + 12, name, "import library name address");
        Requireˉu32(bytes, offset + 16, iat, "import-address table address");
    }

    private static void Writeˉhintˉname(byte[] bytes, Importˉname import)
    {
        Writeˉu16(bytes, import.Offset, 0);
        Writeˉascii(bytes, import.Offset + sizeof(ushort), import.Name);
    }

    private static void Verifyˉhintˉname(ReadOnlySpan<byte> bytes, Importˉname import)
    {
        Requireˉu16(bytes, import.Offset, 0, "import ordinal hint");
        Requireˉascii(bytes, import.Offset + sizeof(ushort), import.Name);
    }

    private static void Writeˉascii(byte[] bytes, int offset, string value)
    {
        Encoding.ASCII.GetBytes(value).CopyTo(bytes.AsSpan(offset));
        bytes[offset + value.Length] = 0;
    }

    private static void Requireˉascii(ReadOnlySpan<byte> bytes, int offset, string value)
    {
        var Expected = Encoding.ASCII.GetBytes(value);
        if (!bytes.Slice(offset, Expected.Length).SequenceEqual(Expected) ||
            bytes[offset + Expected.Length] != 0)
        {
            throw Invalid($"The Windows hosted-verifier import name '{value}' is invalid.");
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

    private static void Writeˉu16(byte[] bytes, int offset, ushort value) =>
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset, sizeof(ushort)), value);

    private static void Writeˉu32(byte[] bytes, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset, sizeof(uint)), value);

    private static void Writeˉu64(byte[] bytes, int offset, ulong value) =>
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(offset, sizeof(ulong)), value);

    private static InvalidDataException Invalid(string message) => new(message);

    private sealed record Importˉname(string Name, int Offset);
}
