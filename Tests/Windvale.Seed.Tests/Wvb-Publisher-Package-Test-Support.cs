using System.Buffers.Binary;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static Dictionary<string, uint> Wvbˉpublisherˉlinuxˉstartupˉtargets(
        Verifiedˉlinuxˉhostedˉverifierˉapplication application,
        Nativeˉserviceˉbundle bundle,
        Nativeˉfragment fragment,
        int beginˉindex,
        int applyˉindex)
    {
        uint Service(Nativeˉservice service) => checked(
            application.Layout.Textˉaddress +
            (uint)Linuxˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
            (uint)bundle.Placements.Single(Item => Item.Service == service).Imageˉoffset);
        uint Native(string name) => checked(
            application.Layout.Textˉaddress +
            (uint)Linuxˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
            fragment.Symbols.Single(Item => Item.Name == name).Offset);
        var Runtime = application.Runtime.Layout;
        var Data = application.Layout.Dataˉaddress;
        return new(StringComparer.Ordinal)
        {
            ["Argument_bytes"] = checked(Data + Runtime.Argumentˉbytesˉoffset),
            ["Argument_table"] = checked(Data + Runtime.Argumentˉtableˉoffset),
            ["Data_arena"] = checked(Data + Runtime.Dataˉarenaˉoffset),
            ["Execution_context"] = Data,
            ["File_input_scratch"] = checked(Data + Runtime.Fileˉinputˉscratchˉoffset),
            ["File_input_table"] = checked(
                Data + Hostedˉverifierˉruntimeˉdata.FILE_INPUT_TABLE_OFFSET),
            ["Name_arena"] = checked(Data + Runtime.Nameˉarenaˉoffset),
            ["Native_main"] = Native("Main"),
            ["Native_publication_apply"] = Native($"$function_{applyˉindex:D4}"),
            ["Native_publication_begin"] = Native($"$function_{beginˉindex:D4}"),
            ["Output_table"] = checked(
                Data + Hostedˉverifierˉruntimeˉdata.OUTPUT_TABLE_OFFSET),
            ["Record_arena"] = checked(Data + Runtime.Recordˉarenaˉoffset),
            ["Service_console_write"] = Service(Nativeˉservice.Consoleˉwriteˉline),
            ["Service_diagnostic_write"] = Service(Nativeˉservice.Diagnosticˉwriteˉline),
            ["Service_file_read"] = Service(Nativeˉservice.Fileˉreadˉbytes),
            ["Service_process_argument"] = Service(Nativeˉservice.Processˉargument),
            ["Service_process_argument_count"] =
                Service(Nativeˉservice.Processˉargumentˉcount),
            ["Service_table"] = checked(
                Data + Hostedˉverifierˉruntimeˉdata.SERVICE_TABLE_OFFSET),
            ["Service_utf8"] = Service(Nativeˉservice.Textˉutf8ˉisˉvalid),
            ["Snapshot_table"] = checked(Data + Runtime.Snapshotˉtableˉoffset),
            ["Text_arena"] = checked(Data + Runtime.Textˉarenaˉoffset),
        };
    }

    private static (byte[] Bytes, Dictionary<string, uint> Exports)
        Instantiateˉwvbˉpublisherˉobject(
        Objectˉfile value,
        uint address,
        IReadOnlyDictionary<string, uint> targets)
    {
        static uint Alignˉup(uint input, uint alignment) => checked(
            (input + alignment - 1) & ~(alignment - 1));

        var Sectionˉoffsets = new uint[value.Sections.Length];
        uint Cursor = 0;
        foreach (var Kind in Enum.GetValues<Objectˉsectionˉkind>())
        {
            for (var Index = 0; Index < value.Sections.Length; Index++)
            {
                var Section = value.Sections[Index];
                if (Section.Kind != Kind)
                {
                    continue;
                }
                True(
                    Section.Kind != Objectˉsectionˉkind.Zeroˉfill,
                    "Publisher adapter objects must not require zero-fill storage.");
                var Alignedˉaddress = Alignˉup(
                    checked(address + Cursor),
                    Section.Alignment);
                var Offset = checked(Alignedˉaddress - address);
                Sectionˉoffsets[Index] = Offset;
                Cursor = checked(Offset + Section.Memoryˉsize);
            }
        }

        var Bytes = new byte[checked((int)Cursor)];
        for (var Index = 0; Index < value.Sections.Length; Index++)
        {
            value.Sections[Index].Data.AsSpan().CopyTo(
                Bytes.AsSpan(checked((int)Sectionˉoffsets[Index])));
        }
        foreach (var Relocation in value.Relocations)
        {
            Equal(Objectˉrelocationˉkind.Relativeˉi32, Relocation.Kind);
            Equal(-4, Relocation.Addend);
            var Symbol = value.Symbols[checked((int)Relocation.Symbolˉindex)];
            var Target = Symbol.Binding == Objectˉsymbolˉbinding.Import
                ? targets[Symbol.Name]
                : checked(address +
                    Sectionˉoffsets[checked((int)Symbol.Sectionˉindex)] +
                    Symbol.Offset);
            var Patchˉoffset = checked(
                Sectionˉoffsets[checked((int)Relocation.Sectionˉindex)] +
                Relocation.Offset);
            var Field = checked(address + Patchˉoffset);
            BinaryPrimitives.WriteInt32LittleEndian(
                Bytes.AsSpan(checked((int)Patchˉoffset), sizeof(int)),
                checked((int)((long)Target - Field + Relocation.Addend)));
        }
        var Exports = value.Symbols
            .Where(Item => Item.Binding == Objectˉsymbolˉbinding.Export)
            .ToDictionary(
                Item => Item.Name,
                Item => checked(address +
                    Sectionˉoffsets[checked((int)Item.Sectionˉindex)] +
                    Item.Offset),
                StringComparer.Ordinal);
        return (Bytes, Exports);
    }

    private static void Writeˉlinuxˉwvbˉpublisherˉsegment(
        Span<byte> header,
        uint fileˉoffset,
        uint address,
        uint bytes)
    {
        header.Clear();
        BinaryPrimitives.WriteUInt32LittleEndian(header[0..], 1);
        BinaryPrimitives.WriteUInt32LittleEndian(header[4..], 5);
        BinaryPrimitives.WriteUInt64LittleEndian(header[8..], fileˉoffset);
        BinaryPrimitives.WriteUInt64LittleEndian(header[16..], address);
        BinaryPrimitives.WriteUInt64LittleEndian(header[32..], bytes);
        BinaryPrimitives.WriteUInt64LittleEndian(header[40..], bytes);
        BinaryPrimitives.WriteUInt64LittleEndian(header[48..], 0x1000);
    }

    private static void Createˉtestˉhardˉlink(string path, string target)
    {
        var Result = OperatingSystem.IsWindows()
            ? Createˉhardˉlinkˉwindows(path, target, 0) ? 0 :
                System.Runtime.InteropServices.Marshal.GetLastPInvokeError()
            : Createˉhardˉlinkˉlinux(target, path) == 0 ? 0 :
                System.Runtime.InteropServices.Marshal.GetLastPInvokeError();
        if (Result != 0)
        {
            throw new System.ComponentModel.Win32Exception(
                Result,
                "The publisher test could not create its hard-link alias.");
        }
    }

    [System.Runtime.InteropServices.DllImport(
        "kernel32.dll",
        EntryPoint = "CreateHardLinkW",
        CharSet = System.Runtime.InteropServices.CharSet.Unicode,
        SetLastError = true)]
    [return: System.Runtime.InteropServices.MarshalAs(
        System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool Createˉhardˉlinkˉwindows(
        string path,
        string target,
        nint securityˉattributes);

    [System.Runtime.InteropServices.DllImport(
        "libc",
        EntryPoint = "link",
        SetLastError = true)]
    private static extern int Createˉhardˉlinkˉlinux(
        string target,
        string path);

    private static Dictionary<string, uint> Wvbˉpublisherˉwindowsˉstartupˉtargets(
        Hostedˉverifierˉruntimeˉlayout runtime,
        uint textˉaddress,
        uint importˉaddress,
        uint runtimeˉaddress,
        Nativeˉserviceˉbundle bundle,
        Nativeˉfragment fragment,
        int beginˉindex,
        int applyˉindex)
    {
        uint Service(Nativeˉservice service) => checked(
            textˉaddress +
            (uint)Windowsˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
            (uint)bundle.Placements.Single(Item => Item.Service == service).Imageˉoffset);
        uint Native(string name) => checked(
            textˉaddress +
            (uint)Windowsˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
            fragment.Symbols.Single(Item => Item.Name == name).Offset);
        var Result = new Dictionary<string, uint>(StringComparer.Ordinal)
        {
            ["Argument_bytes"] = checked(runtimeˉaddress + runtime.Argumentˉbytesˉoffset),
            ["Argument_table"] = checked(runtimeˉaddress + runtime.Argumentˉtableˉoffset),
            ["Data_arena"] = checked(runtimeˉaddress + runtime.Dataˉarenaˉoffset),
            ["Execution_context"] = runtimeˉaddress,
            ["File_input_scratch"] = checked(runtimeˉaddress + runtime.Fileˉinputˉscratchˉoffset),
            ["File_input_table"] = checked(
                runtimeˉaddress + Hostedˉverifierˉruntimeˉdata.FILE_INPUT_TABLE_OFFSET),
            ["Name_arena"] = checked(runtimeˉaddress + runtime.Nameˉarenaˉoffset),
            ["Native_main"] = Native("Main"),
            ["Native_publication_apply"] = Native($"$function_{applyˉindex:D4}"),
            ["Native_publication_begin"] = Native($"$function_{beginˉindex:D4}"),
            ["Output_table"] = checked(
                runtimeˉaddress + Hostedˉverifierˉruntimeˉdata.OUTPUT_TABLE_OFFSET),
            ["Record_arena"] = checked(runtimeˉaddress + runtime.Recordˉarenaˉoffset),
            ["Service_console_write"] = Service(Nativeˉservice.Consoleˉwriteˉline),
            ["Service_diagnostic_write"] = Service(Nativeˉservice.Diagnosticˉwriteˉline),
            ["Service_file_read"] = Service(Nativeˉservice.Fileˉreadˉbytes),
            ["Service_process_argument"] = Service(Nativeˉservice.Processˉargument),
            ["Service_process_argument_count"] =
                Service(Nativeˉservice.Processˉargumentˉcount),
            ["Service_table"] = checked(
                runtimeˉaddress + Hostedˉverifierˉruntimeˉdata.SERVICE_TABLE_OFFSET),
            ["Service_utf8"] = Service(Nativeˉservice.Textˉutf8ˉisˉvalid),
            ["Snapshot_table"] = checked(runtimeˉaddress + runtime.Snapshotˉtableˉoffset),
            ["Text_arena"] = checked(runtimeˉaddress + runtime.Textˉarenaˉoffset),
        };
        void Iat(string name, int offset) =>
            Result[name] = checked(importˉaddress + (uint)offset);
        Iat("Windows_close_handle_iat", Windowsˉwvbˉpublisherˉimports.CLOSE_HANDLE_IAT_OFFSET);
        Iat("Windows_command_line_to_argv_iat", Windowsˉwvbˉpublisherˉimports.COMMAND_LINE_TO_ARGV_IAT_OFFSET);
        Iat("Windows_create_file_iat", Windowsˉwvbˉpublisherˉimports.CREATE_FILE_IAT_OFFSET);
        Iat("Windows_flush_file_buffers_iat", Windowsˉwvbˉpublisherˉimports.FLUSH_FILE_BUFFERS_IAT_OFFSET);
        Iat("Windows_get_command_line_iat", Windowsˉwvbˉpublisherˉimports.GET_COMMAND_LINE_IAT_OFFSET);
        Iat("Windows_get_file_information_iat", Windowsˉwvbˉpublisherˉimports.GET_FILE_INFORMATION_IAT_OFFSET);
        Iat("Windows_get_file_size_iat", Windowsˉwvbˉpublisherˉimports.GET_FILE_SIZE_IAT_OFFSET);
        Iat("Windows_get_last_error_iat", Windowsˉwvbˉpublisherˉimports.GET_LAST_ERROR_IAT_OFFSET);
        Iat("Windows_get_std_handle_iat", Windowsˉwvbˉpublisherˉimports.GET_STD_HANDLE_IAT_OFFSET);
        Iat("Windows_local_free_iat", Windowsˉwvbˉpublisherˉimports.LOCAL_FREE_IAT_OFFSET);
        Iat("Windows_multi_byte_to_wide_char_iat", Windowsˉwvbˉpublisherˉimports.MULTI_BYTE_TO_WIDE_CHAR_IAT_OFFSET);
        Iat("Windows_nt_set_file_information_iat", Windowsˉwvbˉpublisherˉimports.NT_SET_FILE_INFORMATION_IAT_OFFSET);
        Iat("Windows_read_file_iat", Windowsˉwvbˉpublisherˉimports.READ_FILE_IAT_OFFSET);
        Iat("Windows_set_file_information_iat", Windowsˉwvbˉpublisherˉimports.SET_FILE_INFORMATION_IAT_OFFSET);
        Iat("Windows_set_file_pointer_iat", Windowsˉwvbˉpublisherˉimports.SET_FILE_POINTER_IAT_OFFSET);
        Iat("Windows_wide_char_to_multi_byte_iat", Windowsˉwvbˉpublisherˉimports.WIDE_CHAR_TO_MULTI_BYTE_IAT_OFFSET);
        Iat("Windows_write_file_iat", Windowsˉwvbˉpublisherˉimports.WRITE_FILE_IAT_OFFSET);
        return Result;
    }

    private static void Writeˉwvbˉpublisherˉmetadata(
        Span<byte> metadata,
        Consoleˉapplicationˉtarget target,
        ReadOnlySpan<byte> startup,
        uint startupˉentry,
        uint nativeˉentry,
        uint nativeˉbegin,
        uint nativeˉapply,
        ReadOnlySpan<byte> module)
    {
        metadata.Clear();
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[0..], 0x4250_5657);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[4..], 1);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[8..], 128);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[12..], (uint)target);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[16..], 5);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[20..], checked((uint)startup.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[24..], startupˉentry);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[28..], nativeˉentry);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[32..], nativeˉbegin);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[36..], nativeˉapply);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[40..], 4 * 1024 * 1024);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[44..], 1);
        System.Security.Cryptography.SHA256.HashData(startup, metadata[48..80]);
        System.Security.Cryptography.SHA256.HashData(module, metadata[80..112]);
    }
}
