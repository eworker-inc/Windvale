using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int WVB_PUBLISHER_TOOL_BYTES = 136_698;
    private const string WVB_PUBLISHER_TOOL_SHA256 =
        "d8fcbebe7915542b0206900bcce5459957cee768470bf64a2999e6ee688af05d";
    private const string LINUX_WVB_PUBLISHER_STARTUP_WVO_SHA256 =
        "eee997412ced0d7edacaf39dae9c4a3c51e859dce4537045f3972be990b115a4";
    private const string LINUX_WVB_PUBLICATION_ADAPTER_WVO_SHA256 =
        "9272c17b0d7234218a6cd7c31131e9d25e62b6c1ccd976d94975e9b436b2ca5a";
    private const string WINDOWS_WVB_PUBLISHER_STARTUP_WVO_SHA256 =
        "bb136af0382b2f72efc8a07f58fb2368319fce7c119bc7bbfa1b94da6ded9367";
    private const string WINDOWS_WVB_PUBLICATION_ADAPTER_WVO_SHA256 =
        "ef795dabbced735e0808fca04d0205b87d3735b26dd53ca23ed57a7e74453e93";
    private const string X64_WVB_PUBLICATION_SHA256_WVO_SHA256 =
        "380af02cf29f85be1f63a4ea1f02ca3cc027e63091659e214a023b03730f6608";
    private const string LINUX_WVB_PUBLISHER_PACKAGE_SHA256 =
        "71dccc29333b05cff71e4b36e5e41617e0df4f8d747747479e8a27f4a90ed3b0";
    private const string WINDOWS_WVB_PUBLISHER_PACKAGE_SHA256 =
        "f2502ecf9143cfa1343c5f5cb1de066bdf1f82f0e4782afae178f11c41afd735";

    private static readonly string WVB_PUBLISHER_TOOL_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Wvb-Publisher-Tool.wv");

    private static readonly string LINUX_WVB_PUBLISHER_STARTUP_SOURCE =
        Readˉembeddedˉsource("Windvale.Seed.Tests.Linux-X64-Wvb-Publisher.wva");

    private static readonly string LINUX_WVB_PUBLICATION_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Linux-X64-Wvb-Publication-Adapter.wva");

    private static readonly string WINDOWS_WVB_PUBLISHER_STARTUP_SOURCE =
        Readˉembeddedˉsource("Windvale.Seed.Tests.Windows-X64-Wvb-Publisher.wva");

    private static readonly string WINDOWS_WVB_PUBLICATION_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Windows-X64-Wvb-Publication-Adapter.wva");

    private static readonly string X64_WVB_PUBLICATION_SHA256_SOURCE =
        Readˉembeddedˉsource("Windvale.Seed.Tests.X64-Wvb-Publication-Sha256.wva");

    private static void Wvbˉpublisherˉfrontˉdoorˉruns()
    {
        var Compilation = Seedˉcompiler.Compileˉmodules(
            new("Wvb-Publisher-Tool.wv", WVB_PUBLISHER_TOOL_SOURCE),
            [
                new Sourceˉmoduleˉinput(
                    "Compiler-Wvb-Verifier-Semantic-Core.wv",
                    COMPILER_WVB_VERIFIER_SEMANTIC_SOURCE),
                new Sourceˉmoduleˉinput(
                    "Compiler-Wvb-Verifier-Executable-Core.wv",
                    COMPILER_WVB_VERIFIER_EXECUTABLE_SOURCE),
                new Sourceˉmoduleˉinput(
                    "Wvb-Publication-Transaction.wv",
                    WVB_PUBLICATION_TRANSACTION_SOURCE),
                new Sourceˉmoduleˉinput(
                    "Wvb-Publication-Native-Bridge.wv",
                    WVB_PUBLICATION_NATIVE_BRIDGE_SOURCE),
            ]);
        True(
            Compilation.Success,
            "The WVB publisher front door did not compile: " +
                string.Join(" | ", Compilation.Diagnostics));
        Equal(
            WVB_PUBLISHER_TOOL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compilation.Moduleˉbytes.AsSpan()));
        Equal(WVB_PUBLISHER_TOOL_BYTES, Compilation.Moduleˉbytes.Length);

        var Verified = Moduleˉcodec.Readˉandˉverify(Compilation.Moduleˉbytes.AsSpan());
        Equal("Windvaleˉwvbˉpublisherˉtool", Verified.Module.Name);
        Equal(Moduleˉprofile.Hosted, Verified.Module.Profile);
        Sequenceˉequal(
            [
                Capabilityˉcatalog.CONSOLE_WRITE_LINE,
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
                Capabilityˉcatalog.FILE_READ_BYTES,
                Capabilityˉcatalog.PROCESS_ARGUMENT,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
            ],
            Verified.Module.Capabilities.Select(Item => Item.Name));
        Sequenceˉequal(
            ["Main"],
            Verified.Module.Exports.Select(Item => Item.Name));

        var Native = X64ˉnativeˉbackend.Compile(Verified);
        Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Sequenceˉequal(
            [
                Nativeˉservice.Consoleˉwriteˉline,
                Nativeˉservice.Processˉargumentˉcount,
                Nativeˉservice.Processˉargument,
                Nativeˉservice.Fileˉreadˉbytes,
                Nativeˉservice.Diagnosticˉwriteˉline,
            ],
            Native.Fragment.Requiredˉservices);
        Equal(
            "Main",
            Native.Fragment.Symbols.Single(
                Item => Item.Binding == Nativeˉsymbolˉbinding.Export).Name);
        var Beginˉindex = Verified.Functions
            .Select((Item, Index) => (Item, Index))
            .Single(Item => Item.Item.Declaration.Name ==
                "Wvbˉpublicationˉpublisherˉbegin").Index;
        var Applyˉindex = Verified.Functions
            .Select((Item, Index) => (Item, Index))
            .Single(Item => Item.Item.Declaration.Name ==
                "Wvbˉpublicationˉpublisherˉapply").Index;
        True(
            Native.Fragment.Symbols.Any(
                Item => Item.Name == $"$function_{Beginˉindex:D4}"),
            "The publisher native fragment omitted its transaction-begin bridge.");
        True(
            Native.Fragment.Symbols.Any(
                Item => Item.Name == $"$function_{Applyˉindex:D4}"),
            "The publisher native fragment omitted its transaction-apply bridge.");

        var Linuxˉstartupˉwvo =
            Assembleˉsuccess(LINUX_WVB_PUBLISHER_STARTUP_SOURCE);
        var Linuxˉstartup = Objectˉcodec.Readˉandˉverify(
            Linuxˉstartupˉwvo.AsSpan()).Value;
        Equal(Objectˉarchitecture.X86ˉ64, Linuxˉstartup.Architecture);
        Equal(
            "Linux_wvb_publisher_startup",
            Linuxˉstartup.Symbols.Single(Item =>
                Item.Binding == Objectˉsymbolˉbinding.Export).Name);
        Equal(0, Linuxˉstartup.Symbols.Count(Item =>
            Item.Binding == Objectˉsymbolˉbinding.Local));
        True(
            Linuxˉstartup.Symbols.Any(Item =>
                Item.Binding == Objectˉsymbolˉbinding.Import &&
                Item.Name == "Linux_wvb_publisher_run"),
            "The Linux publisher startup omitted its adapter jump.");

        var Linuxˉadapterˉwvo =
            Assembleˉsuccess(LINUX_WVB_PUBLICATION_ADAPTER_SOURCE);
        var Linuxˉadapter = Objectˉcodec.Readˉandˉverify(
            Linuxˉadapterˉwvo.AsSpan()).Value;
        Equal(
            "Linux_wvb_publisher_run",
            Linuxˉadapter.Symbols.Single(Item =>
                Item.Binding == Objectˉsymbolˉbinding.Export).Name);
        Sequenceˉequal(
            ["Linux_wvb_publisher_hex8"],
            Linuxˉadapter.Symbols
                .Where(Item => Item.Binding == Objectˉsymbolˉbinding.Local)
                .Select(Item => Item.Name));
        True(
            Linuxˉadapter.Symbols.Any(Item =>
                Item.Binding == Objectˉsymbolˉbinding.Import &&
                Item.Name == "Native_publication_begin"),
            "The Linux publisher adapter omitted the transaction-begin bridge.");
        True(
            Linuxˉadapter.Symbols.Any(Item =>
                Item.Binding == Objectˉsymbolˉbinding.Import &&
                Item.Name == "Native_publication_apply"),
            "The Linux publisher adapter omitted the transaction-apply bridge.");

        var Windowsˉstartupˉwvo =
            Assembleˉsuccess(WINDOWS_WVB_PUBLISHER_STARTUP_SOURCE);
        var Windowsˉstartup = Objectˉcodec.Readˉandˉverify(
            Windowsˉstartupˉwvo.AsSpan()).Value;
        Equal(Objectˉarchitecture.X86ˉ64, Windowsˉstartup.Architecture);
        Equal(
            "Windows_wvb_publisher_startup",
            Windowsˉstartup.Symbols.Single(Item =>
                Item.Binding == Objectˉsymbolˉbinding.Export).Name);
        Equal(0, Windowsˉstartup.Symbols.Count(Item =>
            Item.Binding == Objectˉsymbolˉbinding.Local));
        True(
            Windowsˉstartup.Symbols.Any(Item =>
                Item.Binding == Objectˉsymbolˉbinding.Import &&
                Item.Name == "Windows_wvb_publisher_run"),
            "The Windows publisher startup omitted its adapter jump.");

        var Windowsˉadapterˉwvo =
            Assembleˉsuccess(WINDOWS_WVB_PUBLICATION_ADAPTER_SOURCE);
        var Windowsˉadapter = Objectˉcodec.Readˉandˉverify(
            Windowsˉadapterˉwvo.AsSpan()).Value;
        Equal(
            "Windows_wvb_publisher_run",
            Windowsˉadapter.Symbols.Single(Item =>
                Item.Binding == Objectˉsymbolˉbinding.Export).Name);
        Sequenceˉequal(
            ["Windows_commit_runtime_pages", "Windows_wvb_publisher_hex8_utf16"],
            Windowsˉadapter.Symbols
                .Where(Item => Item.Binding == Objectˉsymbolˉbinding.Local)
                .Select(Item => Item.Name));
        True(
            Windowsˉadapter.Symbols.Any(Item =>
                Item.Binding == Objectˉsymbolˉbinding.Import &&
                Item.Name == "Windows_set_file_information_iat"),
            "The Windows publisher adapter omitted handle-relative replacement.");
        True(
            Windowsˉadapter.Symbols.Any(Item =>
                Item.Binding == Objectˉsymbolˉbinding.Import &&
                Item.Name == "Windows_flush_file_buffers_iat"),
            "The Windows publisher adapter omitted its durability function.");

        var Sha256ˉadapterˉwvo =
            Assembleˉsuccess(X64_WVB_PUBLICATION_SHA256_SOURCE);
        var Sha256ˉadapter = Objectˉcodec.Readˉandˉverify(
            Sha256ˉadapterˉwvo.AsSpan()).Value;
        Sequenceˉequal(
            [
                "X64_wvb_publication_report_newline",
                "X64_wvb_publication_report_prefix",
                "X64_wvb_publication_report_separator",
                "X64_wvb_publication_sha256_hex",
                "X64_wvb_publication_u32_hex8",
            ],
            Sha256ˉadapter.Symbols
                .Where(Item => Item.Binding == Objectˉsymbolˉbinding.Export)
                .Select(Item => Item.Name));
        Equal(2, Sha256ˉadapter.Sections.Length);
        Equal(0, Sha256ˉadapter.Symbols.Count(Item =>
            Item.Binding == Objectˉsymbolˉbinding.Import));

        var Linuxˉbundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉverifier(
            Native.Fragment,
            Nativeˉserviceˉplatform.Linux);
        var Nativeˉentry = Native.Fragment.Symbols.Single(Item =>
            Item.Binding == Nativeˉsymbolˉbinding.Export &&
            Item.Name == "Main").Offset;
        var Baseˉlinuxˉapplication =
            Linuxˉhostedˉverifierˉapplicationˉbuilder.Build(
                Verified.Module.Capabilities,
                Linuxˉbundle,
                Nativeˉentry);
        var Baseˉlinuxˉverified =
            Linuxˉhostedˉverifierˉapplicationˉverifier.Verify(
                Baseˉlinuxˉapplication.AsSpan(),
                Linuxˉbundle);
        static uint Alignˉup(uint value, uint alignment) => checked(
            (value + alignment - 1) & ~(alignment - 1));
        var Linuxˉadapterˉfileˉoffset = Alignˉup(
            checked((uint)Baseˉlinuxˉapplication.Length),
            0x1000);
        var Linuxˉadapterˉaddress = Alignˉup(
            Baseˉlinuxˉverified.Layout.Imageˉvirtualˉbytes,
            0x1000);
        var Linuxˉadapterˉsection = Linuxˉadapter.Sections.Single(Item =>
            Item.Kind == Objectˉsectionˉkind.Code);
        var Linuxˉsha256ˉoffset = Alignˉup(
            Linuxˉadapterˉsection.Memoryˉsize,
            16);
        var (Linuxˉsha256ˉbytes, Linuxˉsha256ˉexports) =
            Instantiateˉwvbˉpublisherˉobject(
                Sha256ˉadapter,
                checked(Linuxˉadapterˉaddress + Linuxˉsha256ˉoffset),
                new Dictionary<string, uint>(StringComparer.Ordinal));
        var Linuxˉtargets = Wvbˉpublisherˉlinuxˉstartupˉtargets(
            Baseˉlinuxˉverified,
            Linuxˉbundle,
            Native.Fragment,
            Beginˉindex,
            Applyˉindex);
        foreach (var Export in Linuxˉsha256ˉexports)
        {
            Linuxˉtargets.Add(Export.Key, Export.Value);
        }
        var Linuxˉadapterˉexport = Linuxˉadapter.Symbols.Single(Item =>
            Item.Binding == Objectˉsymbolˉbinding.Export &&
            Item.Name == "Linux_wvb_publisher_run");
        Linuxˉtargets["Linux_wvb_publisher_run"] = checked(
            Linuxˉadapterˉaddress + Linuxˉadapterˉexport.Offset);
        var (Linuxˉadapterˉbytes, Linuxˉadapterˉexports) =
            Instantiateˉwvbˉpublisherˉobject(
                Linuxˉadapter,
                Linuxˉadapterˉaddress,
                Linuxˉtargets);
        Equal(
            checked(Linuxˉadapterˉaddress + Linuxˉadapterˉexport.Offset),
            Linuxˉadapterˉexports["Linux_wvb_publisher_run"]);
        var Linuxˉadapterˉsegment = new byte[checked(
            (int)Linuxˉsha256ˉoffset + Linuxˉsha256ˉbytes.Length)];
        Linuxˉadapterˉbytes.CopyTo(Linuxˉadapterˉsegment);
        Linuxˉsha256ˉbytes.CopyTo(Linuxˉadapterˉsegment.AsSpan(
            checked((int)Linuxˉsha256ˉoffset)));
        var (Linuxˉstartupˉbytes, Linuxˉstartupˉexports) =
            Instantiateˉwvbˉpublisherˉobject(
                Linuxˉstartup,
                Baseˉlinuxˉverified.Layout.Textˉaddress,
                Linuxˉtargets);
        var Linuxˉstartupˉentry = checked(
            Linuxˉstartupˉexports["Linux_wvb_publisher_startup"] -
            Baseˉlinuxˉverified.Layout.Textˉaddress);
        True(
            Linuxˉstartupˉbytes.Length <=
                Linuxˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET,
            "The Linux publisher startup exceeded its fixed pre-bundle extent.");
        var Linuxˉapplication = new byte[checked(
            (int)Linuxˉadapterˉfileˉoffset + Linuxˉadapterˉsegment.Length)];
        Baseˉlinuxˉapplication.AsSpan().CopyTo(Linuxˉapplication);
        Linuxˉadapterˉsegment.CopyTo(Linuxˉapplication.AsSpan(
            checked((int)Linuxˉadapterˉfileˉoffset)));
        var Linuxˉnote = Linuxˉapplication.AsSpan(0x180, 28).ToArray();
        Linuxˉapplication.AsSpan(0x180, 28).Clear();
        Linuxˉnote.CopyTo(Linuxˉapplication.AsSpan(0x200));
        BinaryPrimitives.WriteUInt16LittleEndian(
            Linuxˉapplication.AsSpan(56, sizeof(ushort)),
            6);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Linuxˉapplication.AsSpan(0xE8 + 8, sizeof(ulong)),
            0x200);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Linuxˉapplication.AsSpan(0xE8 + 16, sizeof(ulong)),
            0x200);
        Writeˉlinuxˉwvbˉpublisherˉsegment(
            Linuxˉapplication.AsSpan(0x158, 56),
            Linuxˉadapterˉfileˉoffset,
            Linuxˉadapterˉaddress,
            checked((uint)Linuxˉadapterˉsegment.Length));
        Linuxˉapplication.AsSpan(
            Baseˉlinuxˉverified.Layout.Textˉfileˉoffset,
            Linuxˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET).Clear();
        Linuxˉstartupˉbytes.CopyTo(Linuxˉapplication.AsSpan(
            Baseˉlinuxˉverified.Layout.Textˉfileˉoffset));
        BinaryPrimitives.WriteUInt64LittleEndian(
            Linuxˉapplication.AsSpan(24, sizeof(ulong)),
            checked((ulong)(Baseˉlinuxˉverified.Layout.Textˉaddress +
                Linuxˉstartupˉentry)));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Linuxˉapplication.AsSpan(0x200 + 24, sizeof(uint)),
            5);
        Writeˉwvbˉpublisherˉmetadata(
            Linuxˉapplication.AsSpan(checked(
                (int)Baseˉlinuxˉverified.Layout.Dataˉfileˉoffset +
                (int)Hostedˉverifierˉruntimeˉdata.METADATA_OFFSET +
                Hostedˉverifierˉapplicationˉmetadata.SIZE),
            128),
            Consoleˉapplicationˉtarget.Linuxˉx64,
            Linuxˉstartupˉbytes,
            Linuxˉstartupˉentry,
            Nativeˉentry,
            Native.Fragment.Symbols.Single(Item =>
                Item.Name == $"$function_{Beginˉindex:D4}").Offset,
            Native.Fragment.Symbols.Single(Item =>
                Item.Name == $"$function_{Applyˉindex:D4}").Offset,
            Compilation.Moduleˉbytes.AsSpan());
        var Linuxˉapplicationˉsha256 =
            Objectˉdigest.Calculateˉsha256(Linuxˉapplication);
        Equal(64, Linuxˉapplicationˉsha256.Length);

        var Windowsˉbundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉverifier(
            Native.Fragment,
            Nativeˉserviceˉplatform.Windows);
        var Baseˉwindowsˉapplication =
            Windowsˉhostedˉverifierˉapplicationˉbuilder.Build(
                Verified.Module.Capabilities,
                Windowsˉbundle,
                Nativeˉentry);
        var Baseˉwindowsˉverified =
            Windowsˉhostedˉverifierˉapplicationˉverifier.Verify(
                Baseˉwindowsˉapplication.AsSpan(),
                Windowsˉbundle);

        var Windowsˉadapterˉsection = Windowsˉadapter.Sections.Single(Item =>
            Item.Kind == Objectˉsectionˉkind.Code);
        var Windowsˉadapterˉoffset = Alignˉup(
            checked((uint)(
                Windowsˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
                Windowsˉbundle.Imageˉbytes.Length)),
            16);
        var Windowsˉsha256ˉoffset = Alignˉup(checked(
            Windowsˉadapterˉoffset + Windowsˉadapterˉsection.Memoryˉsize), 16);
        var (Windowsˉsha256ˉbytes, Windowsˉsha256ˉexports) =
            Instantiateˉwvbˉpublisherˉobject(
                Sha256ˉadapter,
                checked(Baseˉwindowsˉverified.Layout.Textˉaddress +
                    Windowsˉsha256ˉoffset),
                new Dictionary<string, uint>(StringComparer.Ordinal));
        var Windowsˉtextˉvirtual = checked(
            Windowsˉsha256ˉoffset + (uint)Windowsˉsha256ˉbytes.Length);
        var Windowsˉtextˉfile = Alignˉup(Windowsˉtextˉvirtual, 0x200);
        var Windowsˉfileˉdelta = checked(
            Windowsˉtextˉfile - Baseˉwindowsˉverified.Layout.Textˉfileˉbytes);
        var Windowsˉdataˉaddress = Alignˉup(checked(
            Baseˉwindowsˉverified.Layout.Textˉaddress + Windowsˉtextˉvirtual), 0x1000);
        var Windowsˉvirtualˉdelta = checked(
            Windowsˉdataˉaddress - Baseˉwindowsˉverified.Layout.Dataˉsectionˉaddress);
        var Windowsˉimportˉaddress = Windowsˉdataˉaddress;
        var Windowsˉruntimeˉaddress = checked(
            Windowsˉdataˉaddress + Windowsˉwvbˉpublisherˉimports.PAGE_BYTES);
        var Windowsˉrelocationˉaddress = checked(
            Baseˉwindowsˉverified.Layout.Relocationˉaddress + Windowsˉvirtualˉdelta);
        var Windowsˉdataˉfile = checked(
            Baseˉwindowsˉverified.Layout.Dataˉfileˉoffset + Windowsˉfileˉdelta);
        var Windowsˉimportˉfile = checked(
            Baseˉwindowsˉverified.Layout.Importˉfileˉoffset + Windowsˉfileˉdelta);
        var Windowsˉruntimeˉfile = checked(
            Baseˉwindowsˉverified.Layout.Runtimeˉfileˉoffset + Windowsˉfileˉdelta);
        var Windowsˉrelocationˉfile = checked(
            Baseˉwindowsˉverified.Layout.Relocationˉfileˉoffset + Windowsˉfileˉdelta);
        var Windowsˉtargets = Wvbˉpublisherˉwindowsˉstartupˉtargets(
            Baseˉwindowsˉverified.Runtime.Layout,
            Baseˉwindowsˉverified.Layout.Textˉaddress,
            Windowsˉimportˉaddress,
            Windowsˉruntimeˉaddress,
            Windowsˉbundle,
            Native.Fragment,
            Beginˉindex,
            Applyˉindex);
        foreach (var Export in Windowsˉsha256ˉexports)
        {
            Windowsˉtargets.Add(Export.Key, Export.Value);
        }
        var Windowsˉadapterˉexport = Windowsˉadapter.Symbols.Single(Item =>
            Item.Binding == Objectˉsymbolˉbinding.Export &&
            Item.Name == "Windows_wvb_publisher_run");
        Windowsˉtargets["Windows_wvb_publisher_run"] = checked(
            Baseˉwindowsˉverified.Layout.Textˉaddress +
            Windowsˉadapterˉoffset +
            Windowsˉadapterˉexport.Offset);
        var (Windowsˉadapterˉbytes, Windowsˉadapterˉexports) =
            Instantiateˉwvbˉpublisherˉobject(
                Windowsˉadapter,
                checked(Baseˉwindowsˉverified.Layout.Textˉaddress +
                    Windowsˉadapterˉoffset),
                Windowsˉtargets);
        var Windowsˉadapterˉentry = checked(
            Windowsˉadapterˉexports["Windows_wvb_publisher_run"] -
            Baseˉwindowsˉverified.Layout.Textˉaddress -
            Windowsˉadapterˉoffset);
        Equal(Windowsˉadapterˉexport.Offset, Windowsˉadapterˉentry);
        var (Windowsˉstartupˉbytes, Windowsˉstartupˉexports) =
            Instantiateˉwvbˉpublisherˉobject(
                Windowsˉstartup,
                Baseˉwindowsˉverified.Layout.Textˉaddress,
                Windowsˉtargets);
        var Windowsˉstartupˉentry = checked(
            Windowsˉstartupˉexports["Windows_wvb_publisher_startup"] -
            Baseˉwindowsˉverified.Layout.Textˉaddress);
        True(
            Windowsˉstartupˉbytes.Length <=
                Windowsˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET,
            $"The Windows publisher startup is {Windowsˉstartupˉbytes.Length} bytes and " +
                "exceeded its fixed pre-bundle extent.");
        var Windowsˉapplication = new byte[checked(
            Baseˉwindowsˉapplication.Length + (int)Windowsˉfileˉdelta)];
        Baseˉwindowsˉapplication.AsSpan(
            0,
            checked((int)Baseˉwindowsˉverified.Layout.Dataˉfileˉoffset)).CopyTo(
                Windowsˉapplication);
        Baseˉwindowsˉapplication.AsSpan()[
            checked((int)Baseˉwindowsˉverified.Layout.Dataˉfileˉoffset)..].CopyTo(
                Windowsˉapplication.AsSpan(checked((int)Windowsˉdataˉfile)));
        Windowsˉapplication.AsSpan(
            Baseˉwindowsˉverified.Layout.Textˉfileˉoffset,
            Windowsˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET).Clear();
        Windowsˉstartupˉbytes.CopyTo(Windowsˉapplication.AsSpan(
            Baseˉwindowsˉverified.Layout.Textˉfileˉoffset));
        Windowsˉadapterˉbytes.CopyTo(Windowsˉapplication.AsSpan(checked(
            Baseˉwindowsˉverified.Layout.Textˉfileˉoffset +
            (int)Windowsˉadapterˉoffset)));
        Windowsˉsha256ˉbytes.CopyTo(Windowsˉapplication.AsSpan(checked(
            Baseˉwindowsˉverified.Layout.Textˉfileˉoffset +
            (int)Windowsˉsha256ˉoffset)));
        var Windowsˉimports = Windowsˉwvbˉpublisherˉimports.Build(
            Windowsˉimportˉaddress);
        Windowsˉimports.AsSpan().CopyTo(Windowsˉapplication.AsSpan(
            checked((int)Windowsˉimportˉfile)));
        Windowsˉwvbˉpublisherˉimports.Verify(
            Windowsˉapplication.AsSpan(
                checked((int)Windowsˉimportˉfile),
                Windowsˉwvbˉpublisherˉimports.PAGE_BYTES),
            Windowsˉimportˉaddress);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Windowsˉapplication.AsSpan(0x98 + 4, sizeof(uint)),
            Windowsˉtextˉfile);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Windowsˉapplication.AsSpan(0x98 + 16, sizeof(uint)),
            checked(Baseˉwindowsˉverified.Layout.Textˉaddress +
                Windowsˉstartupˉentry));
        Windowsˉapplication[0x98 + 2] = 5;
        BinaryPrimitives.WriteUInt32LittleEndian(
            Windowsˉapplication.AsSpan(0x98 + 56, sizeof(uint)),
            checked(Baseˉwindowsˉverified.Layout.Imageˉvirtualˉbytes +
                Windowsˉvirtualˉdelta));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Windowsˉapplication.AsSpan(0x98 + 120, sizeof(uint)),
            checked(Windowsˉimportˉaddress +
                Windowsˉwvbˉpublisherˉimports.DIRECTORY_OFFSET));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Windowsˉapplication.AsSpan(0x98 + 152, sizeof(uint)),
            Windowsˉrelocationˉaddress);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Windowsˉapplication.AsSpan(0x98 + 208, sizeof(uint)),
            checked(Windowsˉimportˉaddress +
                Windowsˉwvbˉpublisherˉimports.KERNEL_IAT_OFFSET));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Windowsˉapplication.AsSpan(0x98 + 212, sizeof(uint)),
            Windowsˉwvbˉpublisherˉimports.IAT_BYTES);
        Writeˉwvbˉpublisherˉmetadata(
            Windowsˉapplication.AsSpan(checked(
                (int)Windowsˉruntimeˉfile +
                (int)Hostedˉverifierˉruntimeˉdata.METADATA_OFFSET +
                Hostedˉverifierˉapplicationˉmetadata.SIZE),
            128),
            Consoleˉapplicationˉtarget.Windowsˉx64,
            Windowsˉstartupˉbytes,
            Windowsˉstartupˉentry,
            Nativeˉentry,
            Native.Fragment.Symbols.Single(Item =>
                Item.Name == $"$function_{Beginˉindex:D4}").Offset,
            Native.Fragment.Symbols.Single(Item =>
                Item.Name == $"$function_{Applyˉindex:D4}").Offset,
            Compilation.Moduleˉbytes.AsSpan());

        BinaryPrimitives.WriteUInt32LittleEndian(
            Windowsˉapplication.AsSpan(0x188 + 8, sizeof(uint)),
            Windowsˉtextˉvirtual);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Windowsˉapplication.AsSpan(0x188 + 16, sizeof(uint)),
            Windowsˉtextˉfile);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Windowsˉapplication.AsSpan(0x188 + 40 + 12, sizeof(uint)),
            Windowsˉdataˉaddress);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Windowsˉapplication.AsSpan(0x188 + 40 + 20, sizeof(uint)),
            Windowsˉdataˉfile);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Windowsˉapplication.AsSpan(0x188 + 80 + 12, sizeof(uint)),
            Windowsˉrelocationˉaddress);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Windowsˉapplication.AsSpan(0x188 + 80 + 20, sizeof(uint)),
            Windowsˉrelocationˉfile);
        var Windowsˉapplicationˉsha256 =
            Objectˉdigest.Calculateˉsha256(Windowsˉapplication);
        Equal(64, Windowsˉapplicationˉsha256.Length);
        Equal(164, Linuxˉstartupˉwvo.Length);
        Equal(
            LINUX_WVB_PUBLISHER_STARTUP_WVO_SHA256,
            Objectˉdigest.Calculateˉsha256(Linuxˉstartupˉwvo.AsSpan()));
        Equal(5_507, Linuxˉadapterˉwvo.Length);
        Equal(
            LINUX_WVB_PUBLICATION_ADAPTER_WVO_SHA256,
            Objectˉdigest.Calculateˉsha256(Linuxˉadapterˉwvo.AsSpan()));
        Equal(168, Windowsˉstartupˉwvo.Length);
        Equal(
            WINDOWS_WVB_PUBLISHER_STARTUP_WVO_SHA256,
            Objectˉdigest.Calculateˉsha256(Windowsˉstartupˉwvo.AsSpan()));
        Equal(9_544, Windowsˉadapterˉwvo.Length);
        Equal(
            WINDOWS_WVB_PUBLICATION_ADAPTER_WVO_SHA256,
            Objectˉdigest.Calculateˉsha256(Windowsˉadapterˉwvo.AsSpan()));
        Equal(2_176, Sha256ˉadapterˉwvo.Length);
        Equal(
            X64_WVB_PUBLICATION_SHA256_WVO_SHA256,
            Objectˉdigest.Calculateˉsha256(Sha256ˉadapterˉwvo.AsSpan()));
        Equal(1_119_173, Linuxˉapplication.Length);
        Equal(LINUX_WVB_PUBLISHER_PACKAGE_SHA256, Linuxˉapplicationˉsha256);
        Equal(1_121_792, Windowsˉapplication.Length);
        Equal(WINDOWS_WVB_PUBLISHER_PACKAGE_SHA256, Windowsˉapplicationˉsha256);
        var Authorized = Verified.Module.Capabilities
            .Select(Item => Item.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);

        (Runtimeˉresult Result, string Output, string Diagnostics) Run(
            ImmutableArray<string> arguments,
            ImmutableArray<byte> candidate)
        {
            using var Output = new StringWriter();
            using var Diagnostics = new StringWriter();
            var Host = new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                arguments,
                Output,
                Diagnostics,
                new Testˉfileˉreader((_, _) => candidate)));
            var Result = new Referenceˉruntime(
                Verified,
                Host,
                new Runtimeˉoptions(Authorized, Maximumˉinstructions: 20_000_000_000))
                .Runˉmain();
            return (Result, Output.ToString(), Diagnostics.ToString());
        }

        var Candidate = Seedˉcompiler.Compile(
            "module Publisherˉcandidate profile portable; export fn Main() -> i32 { return 0; }",
            "Publisher-Candidate.wv");
        True(
            Candidate.Success,
            "The publisher candidate fixture did not compile: " +
                string.Join(" | ", Candidate.Diagnostics));
        var Candidateˉreport =
            $"publication status=Complete bytes=0x{Candidate.Moduleˉbytes.Length:x8} " +
            $"sha256={Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                Candidate.Moduleˉbytes.AsSpan())).ToLowerInvariant()}\n";
        var Accepted = Run(
            ["candidate.wvb", "destination.wvb"],
            Candidate.Moduleˉbytes);
        Equal(0, Accepted.Result.Exitˉcode);
        Equal(string.Empty, Accepted.Output);
        Equal(string.Empty, Accepted.Diagnostics);

        var Sameˉpath = Run(
            ["candidate.wvb", "candidate.wvb"],
            Candidate.Moduleˉbytes);
        Equal(64, Sameˉpath.Result.Exitˉcode);
        Equal("Usage: wvpublish <candidate.wvb> <destination.wvb>\n", Sameˉpath.Diagnostics);

        var Rejected = Run(
            ["candidate.wvb", "destination.wvb"],
            [0]);
        Equal(1, Rejected.Result.Exitˉcode);
        Equal("publication status=Rejected phase=semantic\n", Rejected.Diagnostics);

        if (OperatingSystem.IsLinux())
        {
            var Directoryˉpath = Path.Combine(
                Path.GetTempPath(),
                $"windvale-wvb-publisher-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Directoryˉpath);
            try
            {
                var Candidateˉpath = Path.Combine(Directoryˉpath, "Candidate.wvb");
                var Destinationˉpath = Path.Combine(Directoryˉpath, "Destination.wvb");
                File.WriteAllBytes(Candidateˉpath, Candidate.Moduleˉbytes.AsSpan());
                File.WriteAllBytes(Destinationˉpath, [1, 2, 3, 4]);
                var Loadedˉmappings = new HashSet<string>(StringComparer.Ordinal);
                Equal(
                    0,
                    Executeˉlinuxˉapplication(
                        Linuxˉapplication.ToImmutableArray(),
                        Candidateˉreport,
                        [Candidateˉpath, Destinationˉpath],
                        timeoutˉmilliseconds: 60_000,
                        loadedˉmappings: Loadedˉmappings));
                Sequenceˉequal(
                    Candidate.Moduleˉbytes,
                    File.ReadAllBytes(Destinationˉpath));
                Equal(
                    0,
                    Directory.EnumerateFiles(Directoryˉpath, ".wvpublish-*").Count());
                Equal(
                    0,
                    Loadedˉmappings.Count(Name =>
                        Name.Contains("coreclr", StringComparison.OrdinalIgnoreCase) ||
                        Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                        Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));

                var Preserved = new byte[] { 9, 8, 7, 6 };
                File.WriteAllBytes(Candidateˉpath, [0]);
                File.WriteAllBytes(Destinationˉpath, Preserved);
                Equal(
                    1,
                    Executeˉlinuxˉapplication(
                        Linuxˉapplication.ToImmutableArray(),
                        string.Empty,
                        [Candidateˉpath, Destinationˉpath],
                        timeoutˉmilliseconds: 60_000,
                        expectedˉerror:
                            "publication status=Rejected phase=semantic\n"));
                Sequenceˉequal(Preserved, File.ReadAllBytes(Destinationˉpath));
                Equal(
                    0,
                    Directory.EnumerateFiles(Directoryˉpath, ".wvpublish-*").Count());

                File.WriteAllBytes(Candidateˉpath, Candidate.Moduleˉbytes.AsSpan());
                File.Delete(Destinationˉpath);
                Createˉtestˉhardˉlink(Destinationˉpath, Candidateˉpath);
                Equal(
                    1,
                    Executeˉlinuxˉapplication(
                        Linuxˉapplication.ToImmutableArray(),
                        string.Empty,
                        [Candidateˉpath, Destinationˉpath],
                        timeoutˉmilliseconds: 60_000));
                Sequenceˉequal(
                    Candidate.Moduleˉbytes,
                    File.ReadAllBytes(Candidateˉpath));
                Sequenceˉequal(
                    Candidate.Moduleˉbytes,
                    File.ReadAllBytes(Destinationˉpath));
                Equal(
                    0,
                    Directory.EnumerateFiles(Directoryˉpath, ".wvpublish-*").Count());
            }
            finally
            {
                Directory.Delete(Directoryˉpath, recursive: true);
            }
        }
        if (OperatingSystem.IsWindows())
        {
            var Directoryˉpath = Path.Combine(
                Path.GetTempPath(),
                $"windvale-wvb-publisher-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Directoryˉpath);
            try
            {
                var Candidateˉpath = Path.Combine(Directoryˉpath, "Candidate.wvb");
                var Destinationˉpath = Path.Combine(Directoryˉpath, "Destination.wvb");
                File.WriteAllBytes(Candidateˉpath, Candidate.Moduleˉbytes.AsSpan());
                File.WriteAllBytes(Destinationˉpath, [1, 2, 3, 4]);
                var Loadedˉmodules = new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);
                Equal(
                    0,
                    Executeˉwindowsˉapplication(
                        Windowsˉapplication.ToImmutableArray(),
                        Candidateˉreport,
                        [Candidateˉpath, Destinationˉpath],
                        timeoutˉmilliseconds: 60_000,
                        loadedˉmodules: Loadedˉmodules));
                Sequenceˉequal(
                    Candidate.Moduleˉbytes,
                    File.ReadAllBytes(Destinationˉpath));
                Equal(
                    0,
                    Directory.EnumerateFiles(Directoryˉpath, ".wvpublish-*").Count());
                Equal(
                    0,
                    Loadedˉmodules.Count(Name =>
                        Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                        Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                        Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));

                var Preserved = new byte[] { 9, 8, 7, 6 };
                File.WriteAllBytes(Candidateˉpath, [0]);
                File.WriteAllBytes(Destinationˉpath, Preserved);
                Equal(
                    1,
                    Executeˉwindowsˉapplication(
                        Windowsˉapplication.ToImmutableArray(),
                        string.Empty,
                        [Candidateˉpath, Destinationˉpath],
                        timeoutˉmilliseconds: 60_000,
                        expectedˉerror:
                            "publication status=Rejected phase=semantic\n"));
                Sequenceˉequal(Preserved, File.ReadAllBytes(Destinationˉpath));
                Equal(
                    0,
                    Directory.EnumerateFiles(Directoryˉpath, ".wvpublish-*").Count());

                File.WriteAllBytes(Candidateˉpath, Candidate.Moduleˉbytes.AsSpan());
                File.Delete(Destinationˉpath);
                Createˉtestˉhardˉlink(Destinationˉpath, Candidateˉpath);
                Equal(
                    1,
                    Executeˉwindowsˉapplication(
                        Windowsˉapplication.ToImmutableArray(),
                        string.Empty,
                        [Candidateˉpath, Destinationˉpath],
                        timeoutˉmilliseconds: 60_000));
                Sequenceˉequal(
                    Candidate.Moduleˉbytes,
                    File.ReadAllBytes(Candidateˉpath));
                Sequenceˉequal(
                    Candidate.Moduleˉbytes,
                    File.ReadAllBytes(Destinationˉpath));
                Equal(
                    0,
                    Directory.EnumerateFiles(Directoryˉpath, ".wvpublish-*").Count());
            }
            finally
            {
                Directory.Delete(Directoryˉpath, recursive: true);
            }
        }
    }

}
