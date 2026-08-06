using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Reflection;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.ObjectModel;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal sealed record Wvbˉpublisherˉapplicationˉinput(
    Verifiedˉmodule Module,
    Nativeˉfragment Fragment,
    uint Nativeˉentry,
    int Beginˉindex,
    int Applyˉindex,
    ImmutableArray<byte> Moduleˉbytes,
    Nativeˉpublisherˉapplicationˉcontract Contract);

internal sealed record Nativeˉpublisherˉapplicationˉcontract(
    string Moduleˉname,
    int Moduleˉbytes,
    string Moduleˉsha256,
    uint Metadataˉmagic,
    string Beginˉfunction,
    string Applyˉfunction,
    string Description);

internal static class Wvbˉpublisherˉapplicationˉbuilder
{
    internal static readonly Nativeˉpublisherˉapplicationˉcontract WVB_CONTRACT = new(
        Wvbˉpublisherˉapplicationˉcontract.MODULE_NAME,
        Wvbˉpublisherˉapplicationˉcontract.MODULE_BYTES,
        Wvbˉpublisherˉapplicationˉcontract.MODULE_SHA256,
        0x4250_5657,
        "Wvbˉpublicationˉpublisherˉbegin",
        "Wvbˉpublicationˉpublisherˉapply",
        "WVB publisher");

    internal const string LINUX_STARTUP_RESOURCE =
        "Windvale.Linker.Linux-X64-Wvb-Publisher.wvo";
    internal const string LINUX_ADAPTER_RESOURCE =
        "Windvale.Linker.Linux-X64-Wvb-Publication-Adapter.wvo";
    internal const string WINDOWS_STARTUP_RESOURCE =
        "Windvale.Linker.Windows-X64-Wvb-Publisher.wvo";
    internal const string WINDOWS_ADAPTER_RESOURCE =
        "Windvale.Linker.Windows-X64-Wvb-Publication-Adapter.wvo";
    internal const string SHA256_RESOURCE =
        "Windvale.Linker.X64-Wvb-Publication-Sha256.wvo";

    private static readonly (string Resource, int Bytes, string Sha256)[] OBJECTS =
    [
        (LINUX_STARTUP_RESOURCE, 164,
            "eee997412ced0d7edacaf39dae9c4a3c51e859dce4537045f3972be990b115a4"),
        (LINUX_ADAPTER_RESOURCE, 5_507,
            "9272c17b0d7234218a6cd7c31131e9d25e62b6c1ccd976d94975e9b436b2ca5a"),
        (WINDOWS_STARTUP_RESOURCE, 168,
            "bb136af0382b2f72efc8a07f58fb2368319fce7c119bc7bbfa1b94da6ded9367"),
        (WINDOWS_ADAPTER_RESOURCE, 9_544,
            "ef795dabbced735e0808fca04d0205b87d3735b26dd53ca23ed57a7e74453e93"),
        (SHA256_RESOURCE, 2_176,
            "380af02cf29f85be1f63a4ea1f02ca3cc027e63091659e214a023b03730f6608"),
    ];

    internal static uint Alignˉup(uint value, uint alignment) => checked(
        (value + alignment - 1) & ~(alignment - 1));

    internal static Wvbˉpublisherˉapplicationˉinput Validateˉinput(
        Verifiedˉmodule module,
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes) =>
        Validateˉinput(module, fragment, moduleˉbytes, WVB_CONTRACT);

    internal static Wvbˉpublisherˉapplicationˉinput Validateˉinput(
        Verifiedˉmodule module,
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes,
        Nativeˉpublisherˉapplicationˉcontract contract)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(fragment);
        ArgumentNullException.ThrowIfNull(contract);
        if (moduleˉbytes.Length != contract.Moduleˉbytes ||
            !StringComparer.Ordinal.Equals(
                Calculateˉsha256(moduleˉbytes),
                contract.Moduleˉsha256))
        {
            throw new ArgumentException(
                $"The {contract.Description} module does not have the canonical byte identity.",
                nameof(moduleˉbytes));
        }
        if (!StringComparer.Ordinal.Equals(
                module.Module.Name,
                contract.Moduleˉname) ||
            module.Module.Profile != Moduleˉprofile.Hosted)
        {
            throw new ArgumentException(
                $"The {contract.Description} module identity or profile is invalid.",
                nameof(module));
        }
        string[] Expectedˉcapabilities =
        [
            Capabilityˉcatalog.CONSOLE_WRITE_LINE,
            Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
            Capabilityˉcatalog.FILE_READ_BYTES,
            Capabilityˉcatalog.PROCESS_ARGUMENT,
            Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
        ];
        if (!module.Module.Capabilities.Select(Item => Item.Name)
                .SequenceEqual(Expectedˉcapabilities))
        {
            throw new ArgumentException(
                $"The {contract.Description} capability profile is invalid.",
                nameof(module));
        }

        Nativeˉfragmentˉverifier.Verify(fragment);
        Nativeˉservice[] Expectedˉservices =
        [
            Nativeˉservice.Consoleˉwriteˉline,
            Nativeˉservice.Processˉargumentˉcount,
            Nativeˉservice.Processˉargument,
            Nativeˉservice.Fileˉreadˉbytes,
            Nativeˉservice.Diagnosticˉwriteˉline,
        ];
        if (!fragment.Requiredˉservices.SequenceEqual(Expectedˉservices))
        {
            throw new ArgumentException(
                $"The {contract.Description} native service profile is invalid.",
                nameof(fragment));
        }
        var Nativeˉentry = fragment.Symbols.Single(Item =>
            Item.Binding == Nativeˉsymbolˉbinding.Export &&
            Item.Kind == Nativeˉsymbolˉkind.Function &&
            Item.Name == "Main").Offset;
        var Beginˉindex = Functionˉindex(
            module,
            contract.Beginˉfunction);
        var Applyˉindex = Functionˉindex(
            module,
            contract.Applyˉfunction);
        Requireˉprivateˉfunction(fragment, Beginˉindex, "transaction begin");
        Requireˉprivateˉfunction(fragment, Applyˉindex, "transaction apply");
        return new(
            module,
            fragment,
            Nativeˉentry,
            Beginˉindex,
            Applyˉindex,
            moduleˉbytes.ToArray().ToImmutableArray(),
            contract);
    }

    internal static (ImmutableArray<byte> Bytes, Objectˉfile Object)
        Readˉobject(string resource)
    {
        var Contract = OBJECTS.Single(Item => Item.Resource == resource);
        using var Stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(resource) ??
            throw new InvalidDataException(
                $"The embedded publisher object '{resource}' is missing.");
        using var Buffer = new MemoryStream();
        Stream.CopyTo(Buffer);
        var Bytes = Buffer.ToArray().ToImmutableArray();
        if (Bytes.Length != Contract.Bytes ||
            !StringComparer.Ordinal.Equals(
                Objectˉdigest.Calculateˉsha256(Bytes.AsSpan()),
                Contract.Sha256))
        {
            throw new InvalidDataException(
                $"The embedded publisher object '{resource}' has an invalid identity.");
        }
        return (Bytes, Objectˉcodec.Readˉandˉverify(Bytes.AsSpan()).Value);
    }

    internal static (byte[] Bytes, Dictionary<string, uint> Exports)
        Instantiateˉobject(
        Objectˉfile value,
        uint address,
        IReadOnlyDictionary<string, uint> targets)
    {
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
                if (Section.Kind == Objectˉsectionˉkind.Zeroˉfill)
                {
                    throw new InvalidDataException(
                        "Publisher adapter objects must not require zero-fill storage.");
                }
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
            if (Relocation.Kind != Objectˉrelocationˉkind.Relativeˉi32 ||
                Relocation.Addend != -4)
            {
                throw new InvalidDataException(
                    "A publisher object contains an unsupported relocation.");
            }
            var Symbol = value.Symbols[checked((int)Relocation.Symbolˉindex)];
            var Target = Symbol.Binding == Objectˉsymbolˉbinding.Import
                ? targets.TryGetValue(Symbol.Name, out var Imported)
                    ? Imported
                    : throw new InvalidDataException(
                        $"Publisher import '{Symbol.Name}' is unresolved.")
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

    internal static Dictionary<string, uint> Linuxˉtargets(
        Verifiedˉlinuxˉhostedˉverifierˉapplication application,
        Nativeˉserviceˉbundle bundle,
        Wvbˉpublisherˉapplicationˉinput input)
    {
        uint Service(Nativeˉservice service) => checked(
            application.Layout.Textˉaddress +
            (uint)Linuxˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
            (uint)bundle.Placements.Single(Item => Item.Service == service).Imageˉoffset);
        uint Native(string name) => checked(
            application.Layout.Textˉaddress +
            (uint)Linuxˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
            input.Fragment.Symbols.Single(Item => Item.Name == name).Offset);
        var Runtime = application.Runtime.Layout;
        var Data = application.Layout.Dataˉaddress;
        return Runtimeˉtargets(
            Data,
            Runtime,
            Native("Main"),
            Native($"$function_{input.Beginˉindex:D4}"),
            Native($"$function_{input.Applyˉindex:D4}"),
            Service);
    }

    internal static Dictionary<string, uint> Windowsˉtargets(
        Hostedˉverifierˉruntimeˉlayout runtime,
        uint textˉaddress,
        uint importˉaddress,
        uint runtimeˉaddress,
        Nativeˉserviceˉbundle bundle,
        Wvbˉpublisherˉapplicationˉinput input)
    {
        uint Service(Nativeˉservice service) => checked(
            textˉaddress +
            (uint)Windowsˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
            (uint)bundle.Placements.Single(Item => Item.Service == service).Imageˉoffset);
        uint Native(string name) => checked(
            textˉaddress +
            (uint)Windowsˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
            input.Fragment.Symbols.Single(Item => Item.Name == name).Offset);
        var Result = Runtimeˉtargets(
            runtimeˉaddress,
            runtime,
            Native("Main"),
            Native($"$function_{input.Beginˉindex:D4}"),
            Native($"$function_{input.Applyˉindex:D4}"),
            Service);
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

    internal static void Writeˉmetadata(
        Span<byte> metadata,
        Consoleˉapplicationˉtarget target,
        ReadOnlySpan<byte> startup,
        uint startupˉentry,
        Wvbˉpublisherˉapplicationˉinput input)
    {
        metadata.Clear();
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[0..], input.Contract.Metadataˉmagic);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[4..], 1);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[8..], 128);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[12..], (uint)target);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[16..], 5);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[20..], checked((uint)startup.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[24..], startupˉentry);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[28..], input.Nativeˉentry);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[32..],
            input.Fragment.Symbols.Single(Item =>
                Item.Name == $"$function_{input.Beginˉindex:D4}").Offset);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[36..],
            input.Fragment.Symbols.Single(Item =>
                Item.Name == $"$function_{input.Applyˉindex:D4}").Offset);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[40..], 4 * 1024 * 1024);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[44..], 1);
        SHA256.HashData(startup, metadata[48..80]);
        SHA256.HashData(input.Moduleˉbytes.AsSpan(), metadata[80..112]);
    }

    internal static void Requireˉapplicationˉidentity(
        ReadOnlySpan<byte> bytes,
        int expectedˉbytes,
        string expectedˉsha256,
        string platform)
    {
        if (bytes.Length != expectedˉbytes ||
            !StringComparer.Ordinal.Equals(
                Calculateˉsha256(bytes),
                expectedˉsha256))
        {
            throw new InvalidDataException(
                $"The {platform} WVB publisher application identity is invalid.");
        }
    }

    private static Dictionary<string, uint> Runtimeˉtargets(
        uint data,
        Hostedˉverifierˉruntimeˉlayout runtime,
        uint nativeˉmain,
        uint nativeˉbegin,
        uint nativeˉapply,
        Func<Nativeˉservice, uint> service) =>
        new(StringComparer.Ordinal)
        {
            ["Argument_bytes"] = checked(data + runtime.Argumentˉbytesˉoffset),
            ["Argument_table"] = checked(data + runtime.Argumentˉtableˉoffset),
            ["Data_arena"] = checked(data + runtime.Dataˉarenaˉoffset),
            ["Execution_context"] = data,
            ["File_input_scratch"] = checked(data + runtime.Fileˉinputˉscratchˉoffset),
            ["File_input_table"] = checked(
                data + Hostedˉverifierˉruntimeˉdata.FILE_INPUT_TABLE_OFFSET),
            ["Name_arena"] = checked(data + runtime.Nameˉarenaˉoffset),
            ["Native_main"] = nativeˉmain,
            ["Native_publication_apply"] = nativeˉapply,
            ["Native_publication_begin"] = nativeˉbegin,
            ["Output_table"] = checked(
                data + Hostedˉverifierˉruntimeˉdata.OUTPUT_TABLE_OFFSET),
            ["Record_arena"] = checked(data + runtime.Recordˉarenaˉoffset),
            ["Service_console_write"] = service(Nativeˉservice.Consoleˉwriteˉline),
            ["Service_diagnostic_write"] = service(Nativeˉservice.Diagnosticˉwriteˉline),
            ["Service_file_read"] = service(Nativeˉservice.Fileˉreadˉbytes),
            ["Service_process_argument"] = service(Nativeˉservice.Processˉargument),
            ["Service_process_argument_count"] =
                service(Nativeˉservice.Processˉargumentˉcount),
            ["Service_table"] = checked(
                data + Hostedˉverifierˉruntimeˉdata.SERVICE_TABLE_OFFSET),
            ["Service_utf8"] = service(Nativeˉservice.Textˉutf8ˉisˉvalid),
            ["Snapshot_table"] = checked(data + runtime.Snapshotˉtableˉoffset),
            ["Text_arena"] = checked(data + runtime.Textˉarenaˉoffset),
        };

    private static int Functionˉindex(Verifiedˉmodule module, string name) =>
        module.Functions
            .Select((Item, Index) => (Item, Index))
            .Single(Item => Item.Item.Declaration.Name == name).Index;

    private static void Requireˉprivateˉfunction(
        Nativeˉfragment fragment,
        int index,
        string field)
    {
        if (!fragment.Symbols.Any(Item => Item.Name == $"$function_{index:D4}"))
        {
            throw new ArgumentException(
                $"The publisher native fragment omitted its {field} bridge.",
                nameof(fragment));
        }
    }

    private static string Calculateˉsha256(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
