using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Reflection;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.ObjectModel;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal sealed record Immutableˉsnapshotˉpublisherˉapplicationˉinput(
    Verifiedˉmodule Module,
    Nativeˉfragment Fragment,
    uint Nativeˉentry,
    ImmutableArray<byte> Moduleˉbytes);

internal sealed record Immutableˉsnapshotˉpublisherˉapplicationˉprofile(
    string Description,
    string Moduleˉname,
    int Moduleˉbytes,
    string Moduleˉsha256,
    ImmutableArray<string> Moduleˉcapabilities,
    ImmutableArray<Nativeˉservice> Moduleˉservices,
    string Linuxˉstartupˉresource,
    string Linuxˉadapterˉresource,
    string Linuxˉstartupˉexport,
    string Linuxˉadapterˉexport,
    string Windowsˉstartupˉresource,
    string Windowsˉadapterˉresource,
    string Windowsˉstartupˉexport,
    string Windowsˉadapterˉexport,
    string Snapshotˉresource,
    uint Metadataˉmagic);

internal static class Immutableˉsnapshotˉpublisherˉapplicationˉbuilder
{
    internal const string LINUX_STARTUP_RESOURCE =
        "Windvale.Linker.Linux-X64-Wvo-Staging-Publisher.wvo";
    internal const string LINUX_ADAPTER_RESOURCE =
        "Windvale.Linker.Linux-X64-Wvo-Staging-Publication-Adapter.wvo";
    internal const string LINUX_HOSTED_STARTUP_RESOURCE =
        "Windvale.Linker.Linux-X64-Hosted-Container-Publisher.wvo";
    internal const string LINUX_HOSTED_ADAPTER_RESOURCE =
        "Windvale.Linker.Linux-X64-Hosted-Container-Publication-Adapter.wvo";
    internal const string LINUX_SHELL_RESOURCE =
        "Windvale.Linker.Linux-X64-Immutable-Snapshot-Publisher.wvo";
    internal const string LINUX_TRANSACTION_RESOURCE =
        "Windvale.Linker.Linux-X64-Durable-Multi-Chunk-Publication.wvo";
    internal const string WINDOWS_STARTUP_RESOURCE =
        "Windvale.Linker.Windows-X64-Wvo-Staging-Publisher.wvo";
    internal const string WINDOWS_ADAPTER_RESOURCE =
        "Windvale.Linker.Windows-X64-Wvo-Staging-Publication-Adapter.wvo";
    internal const string WINDOWS_HOSTED_STARTUP_RESOURCE =
        "Windvale.Linker.Windows-X64-Hosted-Container-Publisher.wvo";
    internal const string WINDOWS_HOSTED_ADAPTER_RESOURCE =
        "Windvale.Linker.Windows-X64-Hosted-Container-Publication-Adapter.wvo";
    internal const string WINDOWS_SHELL_RESOURCE =
        "Windvale.Linker.Windows-X64-Immutable-Snapshot-Publisher.wvo";
    internal const string WINDOWS_TRANSACTION_RESOURCE =
        "Windvale.Linker.Windows-X64-Durable-Multi-Chunk-Publication.wvo";
    internal const string SNAPSHOT_TABLE_RESOURCE =
        "Windvale.Linker.X64-Wvo-Staging-Snapshot-Table.wvo";
    internal const string IMMUTABLE_SNAPSHOT_SEQUENCE_RESOURCE =
        "Windvale.Linker.X64-Immutable-Snapshot-Sequence.wvo";
    internal const string HOSTED_CONTAINER_SNAPSHOT_RESOURCE =
        "Windvale.Linker.X64-Hosted-Container-Snapshot-Table.wvo";
    internal const string PUBLICATION_STATE_RESOURCE =
        "Windvale.Linker.X64-Publication-Transaction-State.wvo";

    private static readonly (string Resource, int Bytes, string Sha256)[] OBJECTS =
    [
        (LINUX_STARTUP_RESOURCE, 180,
            "8cb479d958881b8fa74b67dc3de6bc5b669adfd38d699735a2ab62aee610ccba"),
        (LINUX_HOSTED_STARTUP_RESOURCE, 190,
            "88d45c0936a81d1727a36a6013353e4b01da2ac3c3e121baa7cf21ee17234965"),
        (LINUX_ADAPTER_RESOURCE, 287,
            "c4f51956b86d477a93232091da491b0c8a4a117150125dbf24e817234d0a1da1"),
        (LINUX_HOSTED_ADAPTER_RESOURCE, 300,
            "7cdbf94f43d53252db293fda79c69773c5d40b03a1acbd120256614487583929"),
        (LINUX_SHELL_RESOURCE, 3_503,
            "595398fc2fd80e4b29bc88e9de13731374c6783a6dc2ac0f86eecf7734eb41fc"),
        (LINUX_TRANSACTION_RESOURCE, 2_468,
            "01eb64695ef008da75b282079fde1cb93766bdc37a518695b2377eecee7b3f85"),
        (WINDOWS_STARTUP_RESOURCE, 184,
            "7e4ef5d1565aed7dddb325faa74f800f5d006567d0de84a84e8bc9b898f420ab"),
        (WINDOWS_HOSTED_STARTUP_RESOURCE, 194,
            "84475183f21b69abde8d73cc9748cca7b7c8377335d4a8ddabe8a9dfc88ea57b"),
        (WINDOWS_ADAPTER_RESOURCE, 285,
            "86dd44e921418a82c69aa155b671662fa2961041d0ead661a0328f3371f7f045"),
        (WINDOWS_HOSTED_ADAPTER_RESOURCE, 298,
            "bfb42ca6a679a25c7a45660bf0743ee3f4e64febceeede6b079126e1df0aab75"),
        (WINDOWS_SHELL_RESOURCE, 6_116,
            "d5233eb678b1c96eb6c8c4108ff10d7bcc263678defb81915ab1c67a6b398110"),
        (WINDOWS_TRANSACTION_RESOURCE, 4_001,
            "3795ab62b6dc5008748ba7c4332b885419a14479c9c11369bcc13885cad8974b"),
        (SNAPSHOT_TABLE_RESOURCE, 224,
            "03ff27e8a8fce7b3eddfb0191b6626c20971df32790f8f7274cd9091a4b69628"),
        (IMMUTABLE_SNAPSHOT_SEQUENCE_RESOURCE, 1_282,
            "7c6ea6b16ac8cfcfed9e0983b7e6aedc3ead4aab3a54cb207b75d22a228db676"),
        (HOSTED_CONTAINER_SNAPSHOT_RESOURCE, 256,
            "390ee99e24e02cfa904f64d1ab772d76f5de358783c3f75e0310e37750cc5e86"),
        (PUBLICATION_STATE_RESOURCE, 433,
            "54f18e6221bd40ee9c32a5ad32a747706de9857b5e605c6658a31aaf9c13a0ec"),
    ];

    internal static readonly Immutableˉsnapshotˉpublisherˉapplicationˉprofile
        WVO_STAGING_PROFILE = new(
            "staged-WVO",
            Wvoˉstagingˉpublisherˉapplicationˉcontract.MODULE_NAME,
            Wvoˉstagingˉpublisherˉapplicationˉcontract.MODULE_BYTES,
            Wvoˉstagingˉpublisherˉapplicationˉcontract.MODULE_SHA256,
            [
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
                Capabilityˉcatalog.FILE_READ_BYTES,
                Capabilityˉcatalog.PROCESS_ARGUMENT,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
            ],
            [
                Nativeˉservice.Processˉargumentˉcount,
                Nativeˉservice.Processˉargument,
                Nativeˉservice.Fileˉreadˉbytes,
                Nativeˉservice.Textˉutf8ˉisˉvalid,
                Nativeˉservice.Diagnosticˉwriteˉline,
                Nativeˉservice.Enumˉname,
                Nativeˉservice.Textˉconcat,
                Nativeˉservice.U32ˉformat,
            ],
            LINUX_STARTUP_RESOURCE,
            LINUX_ADAPTER_RESOURCE,
            "Linux_wvo_staging_publisher_startup",
            "Linux_wvo_staging_publisher_run",
            WINDOWS_STARTUP_RESOURCE,
            WINDOWS_ADAPTER_RESOURCE,
            "Windows_wvo_staging_publisher_startup",
            "Windows_wvo_staging_publisher_run",
            SNAPSHOT_TABLE_RESOURCE,
            0x5053_5657);

    internal static readonly Immutableˉsnapshotˉpublisherˉapplicationˉprofile
        HOSTED_CONTAINER_PROFILE = new(
            "hosted-container",
            Hostedˉcontainerˉpublisherˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcontainerˉpublisherˉapplicationˉcontract.MODULE_BYTES,
            Hostedˉcontainerˉpublisherˉapplicationˉcontract.MODULE_SHA256,
            [
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
                Capabilityˉcatalog.FILE_READ_BYTES,
                Capabilityˉcatalog.PROCESS_ARGUMENT,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
            ],
            [
                Nativeˉservice.Processˉargumentˉcount,
                Nativeˉservice.Processˉargument,
                Nativeˉservice.Fileˉreadˉbytes,
                Nativeˉservice.Diagnosticˉwriteˉline,
                Nativeˉservice.Enumˉname,
                Nativeˉservice.Textˉconcat,
                Nativeˉservice.U32ˉformat,
            ],
            LINUX_HOSTED_STARTUP_RESOURCE,
            LINUX_HOSTED_ADAPTER_RESOURCE,
            "Linux_hosted_container_publisher_startup",
            "Linux_hosted_container_publisher_run",
            WINDOWS_HOSTED_STARTUP_RESOURCE,
            WINDOWS_HOSTED_ADAPTER_RESOURCE,
            "Windows_hosted_container_publisher_startup",
            "Windows_hosted_container_publisher_run",
            HOSTED_CONTAINER_SNAPSHOT_RESOURCE,
            0x5048_5657);

    internal static Immutableˉsnapshotˉpublisherˉapplicationˉinput Validateˉinput(
        Verifiedˉmodule module,
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes,
        Immutableˉsnapshotˉpublisherˉapplicationˉprofile profile)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(fragment);
        if (moduleˉbytes.Length != profile.Moduleˉbytes ||
            !StringComparer.Ordinal.Equals(
                Calculateˉsha256(moduleˉbytes),
                profile.Moduleˉsha256))
        {
            throw new ArgumentException(
                $"The {profile.Description} publisher module does not have the canonical byte identity.",
                nameof(moduleˉbytes));
        }
        if (!StringComparer.Ordinal.Equals(
                module.Module.Name,
                profile.Moduleˉname) ||
            module.Module.Profile != Moduleˉprofile.Hosted)
        {
            throw new ArgumentException(
                $"The {profile.Description} publisher module identity or profile is invalid.",
                nameof(module));
        }
        if (!module.Module.Capabilities.Select(Item => Item.Name)
                .SequenceEqual(profile.Moduleˉcapabilities))
        {
            throw new ArgumentException(
                $"The {profile.Description} publisher capability profile is invalid.",
                nameof(module));
        }

        Nativeˉfragmentˉverifier.Verify(fragment);
        if (!fragment.Requiredˉservices.SequenceEqual(profile.Moduleˉservices))
        {
            throw new ArgumentException(
                $"The {profile.Description} publisher native service profile is invalid.",
                nameof(fragment));
        }
        var Nativeˉentry = fragment.Symbols.Single(Item =>
            Item.Binding == Nativeˉsymbolˉbinding.Export &&
            Item.Kind == Nativeˉsymbolˉkind.Function &&
            Item.Name == "Main").Offset;
        return new(
            module,
            fragment,
            Nativeˉentry,
            moduleˉbytes.ToArray().ToImmutableArray());
    }

    internal static Objectˉfile Readˉobject(string resource)
    {
        var Contract = OBJECTS.Single(Item => Item.Resource == resource);
        using var Stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(resource) ??
            throw new InvalidDataException(
                $"The embedded immutable-snapshot publisher object '{resource}' is missing.");
        using var Buffer = new MemoryStream();
        Stream.CopyTo(Buffer);
        var Bytes = Buffer.ToArray();
        if (Bytes.Length != Contract.Bytes ||
            !StringComparer.Ordinal.Equals(
                Objectˉdigest.Calculateˉsha256(Bytes),
                Contract.Sha256))
        {
            throw new InvalidDataException(
                $"The embedded immutable-snapshot publisher object '{resource}' has an invalid identity.");
        }
        return Objectˉcodec.Readˉandˉverify(Bytes).Value;
    }

    internal static Nativeˉserviceˉbundle Buildˉcontainerˉbundle(
        Nativeˉfragment fragment,
        Nativeˉserviceˉplatform platform)
    {
        ImmutableArray<Nativeˉservice> Services =
        [
            Nativeˉservice.Consoleˉwriteˉline,
            Nativeˉservice.Processˉargumentˉcount,
            Nativeˉservice.Processˉargument,
            Nativeˉservice.Fileˉreadˉbytes,
            Nativeˉservice.Textˉutf8ˉisˉvalid,
            Nativeˉservice.Diagnosticˉwriteˉline,
            Nativeˉservice.Enumˉname,
            Nativeˉservice.Textˉconcat,
            Nativeˉservice.U32ˉformat,
            Nativeˉservice.Fileˉwriteˉbytes,
        ];
        return X64ˉnativeˉserviceˉbundle.Build(
            fragment with { Requiredˉservices = Services },
            platform);
    }

    internal static ImmutableArray<Capabilityˉdeclaration>
        Containerˉcapabilities()
    {
        string[] Names =
        [
            Capabilityˉcatalog.CONSOLE_WRITE_LINE,
            Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
            Capabilityˉcatalog.FILE_READ_BYTES,
            Capabilityˉcatalog.FILE_WRITE_BYTES,
            Capabilityˉcatalog.PROCESS_ARGUMENT,
            Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
        ];
        return [.. Names.Select(Name =>
            Capabilityˉcatalog.Tryˉget(Name, out var Declaration)
                ? Declaration
                : throw new InvalidOperationException(
                    $"Capability '{Name}' left the canonical catalog."))];
    }

    internal static Dictionary<string, uint> Linuxˉtargets(
        Verifiedˉlinuxˉhostedˉcompilerˉapplication application,
        Nativeˉserviceˉbundle bundle,
        Immutableˉsnapshotˉpublisherˉapplicationˉinput input)
    {
        uint Service(Nativeˉservice service) => checked(
            application.Layout.Textˉaddress +
            (uint)Linuxˉhostedˉcompilerˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
            (uint)bundle.Placements.Single(Item => Item.Service == service).Imageˉoffset);
        uint Native(string name) => checked(
            application.Layout.Textˉaddress +
            (uint)Linuxˉhostedˉcompilerˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
            input.Fragment.Symbols.Single(Item => Item.Name == name).Offset);
        return Runtimeˉtargets(
            application.Layout.Dataˉaddress,
            application.Runtime.Layout,
            Native("Main"),
            Service);
    }

    internal static Dictionary<string, uint> Windowsˉtargets(
        Hostedˉcompilerˉruntimeˉlayout runtime,
        uint textˉaddress,
        uint importˉaddress,
        uint runtimeˉaddress,
        Nativeˉserviceˉbundle bundle,
        Immutableˉsnapshotˉpublisherˉapplicationˉinput input)
    {
        uint Service(Nativeˉservice service) => checked(
            textˉaddress +
            (uint)Windowsˉhostedˉcompilerˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
            (uint)bundle.Placements.Single(Item => Item.Service == service).Imageˉoffset);
        uint Native(string name) => checked(
            textˉaddress +
            (uint)Windowsˉhostedˉcompilerˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
            input.Fragment.Symbols.Single(Item => Item.Name == name).Offset);
        var Result = Runtimeˉtargets(
            runtimeˉaddress,
            runtime,
            Native("Main"),
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
        uint publicationˉbeginˉoffset,
        uint publicationˉapplyˉoffset,
        Immutableˉsnapshotˉpublisherˉapplicationˉinput input,
        Immutableˉsnapshotˉpublisherˉapplicationˉprofile profile)
    {
        metadata.Clear();
        BinaryPrimitives.WriteUInt32LittleEndian(
            metadata[0..], profile.Metadataˉmagic);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[4..], 1);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[8..], 128);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[12..], (uint)target);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[16..], 4);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[20..], checked((uint)startup.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[24..], startupˉentry);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[28..], input.Nativeˉentry);
        BinaryPrimitives.WriteUInt32LittleEndian(
            metadata[32..], publicationˉbeginˉoffset);
        BinaryPrimitives.WriteUInt32LittleEndian(
            metadata[36..], publicationˉapplyˉoffset);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[40..], 4 * 1024 * 1024);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata[44..], 64);
        SHA256.HashData(startup, metadata[48..80]);
        SHA256.HashData(input.Moduleˉbytes.AsSpan(), metadata[80..112]);
    }

    internal static void Requireˉapplicationˉidentity(
        ReadOnlySpan<byte> bytes,
        int expectedˉbytes,
        string expectedˉsha256,
        string platform,
        string description)
    {
        if (bytes.Length != expectedˉbytes ||
            !StringComparer.Ordinal.Equals(
                Calculateˉsha256(bytes),
                expectedˉsha256))
        {
            throw new InvalidDataException(
                $"The {platform} {description} publisher application identity is invalid " +
                $"(bytes={bytes.Length}, sha256={Calculateˉsha256(bytes)}).");
        }
    }

    private static Dictionary<string, uint> Runtimeˉtargets(
        uint data,
        Hostedˉcompilerˉruntimeˉlayout runtime,
        uint nativeˉmain,
        Func<Nativeˉservice, uint> service) =>
        new(StringComparer.Ordinal)
        {
            ["Argument_bytes"] = checked(data + runtime.Argumentˉbytesˉoffset),
            ["Argument_table"] = checked(data + runtime.Argumentˉtableˉoffset),
            ["Data_arena"] = checked(data + runtime.Dataˉarenaˉoffset),
            ["Execution_context"] = data,
            ["File_input_scratch"] = checked(data + runtime.Fileˉinputˉscratchˉoffset),
            ["File_input_table"] = checked(
                data + Hostedˉcompilerˉruntimeˉdata.FILE_INPUT_TABLE_OFFSET),
            ["Name_arena"] = checked(data + runtime.Nameˉarenaˉoffset),
            ["Native_main"] = nativeˉmain,
            ["Output_table"] = checked(
                data + Hostedˉcompilerˉruntimeˉdata.OUTPUT_TABLE_OFFSET),
            ["Record_arena"] = checked(data + runtime.Recordˉarenaˉoffset),
            ["Service_diagnostic_write"] = service(Nativeˉservice.Diagnosticˉwriteˉline),
            ["Service_enum_name"] = service(Nativeˉservice.Enumˉname),
            ["Service_file_read"] = service(Nativeˉservice.Fileˉreadˉbytes),
            ["Service_process_argument"] = service(Nativeˉservice.Processˉargument),
            ["Service_process_argument_count"] =
                service(Nativeˉservice.Processˉargumentˉcount),
            ["Service_table"] = checked(
                data + Hostedˉcompilerˉruntimeˉdata.SERVICE_TABLE_OFFSET),
            ["Service_text_concat"] = service(Nativeˉservice.Textˉconcat),
            ["Service_u32_format"] = service(Nativeˉservice.U32ˉformat),
            ["Service_utf8"] = service(Nativeˉservice.Textˉutf8ˉisˉvalid),
            ["Snapshot_table"] = checked(data + runtime.Snapshotˉtableˉoffset),
            ["Text_arena"] = checked(data + runtime.Textˉarenaˉoffset),
        };

    private static string Calculateˉsha256(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
