using System.Buffers.Binary;
using Windvale.Compiler.Native;
using Windvale.ObjectModel;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Windowsˉwvoˉstagingˉpublisherˉapplicationˉbuilder
{
    internal static byte[] Build(
        Windvale.Bytecode.Verifiedˉmodule module,
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes)
    {
        var Input = Wvoˉstagingˉpublisherˉapplicationˉbuilder.Validateˉinput(
            module,
            fragment,
            moduleˉbytes);
        var Startup = Wvoˉstagingˉpublisherˉapplicationˉbuilder.Readˉobject(
            Wvoˉstagingˉpublisherˉapplicationˉbuilder.WINDOWS_STARTUP_RESOURCE);
        var Adapter = Wvoˉstagingˉpublisherˉapplicationˉbuilder.Readˉobject(
            Wvoˉstagingˉpublisherˉapplicationˉbuilder.WINDOWS_ADAPTER_RESOURCE);
        var Shell = Wvoˉstagingˉpublisherˉapplicationˉbuilder.Readˉobject(
            Wvoˉstagingˉpublisherˉapplicationˉbuilder.WINDOWS_SHELL_RESOURCE);
        var Transaction = Wvoˉstagingˉpublisherˉapplicationˉbuilder.Readˉobject(
            Wvoˉstagingˉpublisherˉapplicationˉbuilder.WINDOWS_TRANSACTION_RESOURCE);
        var Snapshot = Wvoˉstagingˉpublisherˉapplicationˉbuilder.Readˉobject(
            Wvoˉstagingˉpublisherˉapplicationˉbuilder.SNAPSHOT_TABLE_RESOURCE);
        var Sequence = Wvoˉstagingˉpublisherˉapplicationˉbuilder.Readˉobject(
            Wvoˉstagingˉpublisherˉapplicationˉbuilder
                .IMMUTABLE_SNAPSHOT_SEQUENCE_RESOURCE);

        var Bundle = Wvoˉstagingˉpublisherˉapplicationˉbuilder.Buildˉcontainerˉbundle(
            Input.Fragment,
            Nativeˉserviceˉplatform.Windows);
        var Baseˉapplication = Windowsˉhostedˉcompilerˉapplicationˉbuilder.Build(
            Wvoˉstagingˉpublisherˉapplicationˉbuilder.Containerˉcapabilities(),
            Bundle,
            Input.Nativeˉentry,
            Hostedˉcompilerˉapplicationˉprofile.Compiler);
        var Baseˉverified = Windowsˉhostedˉcompilerˉapplicationˉverifier.Verify(
            Baseˉapplication.AsSpan(),
            Bundle,
            Hostedˉcompilerˉapplicationˉprofile.Compiler);

        var Adapterˉsection = Adapter.Sections.Single(Item =>
            Item.Kind == Objectˉsectionˉkind.Code);
        var Adapterˉoffset = Wvbˉpublisherˉapplicationˉbuilder.Alignˉup(
            checked((uint)(
                Windowsˉhostedˉcompilerˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
                Bundle.Imageˉbytes.Length)),
            16);
        var Shellˉoffset = Wvbˉpublisherˉapplicationˉbuilder.Alignˉup(checked(
            Adapterˉoffset + Adapterˉsection.Memoryˉsize), 16);
        var Transactionˉoffset = Wvbˉpublisherˉapplicationˉbuilder.Alignˉup(checked(
            Shellˉoffset + Shell.Sections.Single(Item =>
                Item.Kind == Objectˉsectionˉkind.Code).Memoryˉsize), 16);
        var Snapshotˉoffset = Wvbˉpublisherˉapplicationˉbuilder.Alignˉup(checked(
            Transactionˉoffset + Transaction.Sections.Single(Item =>
                Item.Kind == Objectˉsectionˉkind.Code).Memoryˉsize), 16);
        var Sequenceˉoffset = Wvbˉpublisherˉapplicationˉbuilder.Alignˉup(checked(
            Snapshotˉoffset + Snapshot.Sections.Single(Item =>
                Item.Kind == Objectˉsectionˉkind.Code).Memoryˉsize), 16);
        var Textˉvirtual = checked(
            Sequenceˉoffset + Sequence.Sections.Single(Item =>
                Item.Kind == Objectˉsectionˉkind.Code).Memoryˉsize);
        var Textˉfile = Wvbˉpublisherˉapplicationˉbuilder.Alignˉup(
            Textˉvirtual,
            0x200);
        var Fileˉdelta = checked(
            Textˉfile - Baseˉverified.Layout.Textˉfileˉbytes);
        var Dataˉaddress = Wvbˉpublisherˉapplicationˉbuilder.Alignˉup(checked(
            Baseˉverified.Layout.Textˉaddress + Textˉvirtual), 0x1000);
        var Virtualˉdelta = checked(
            Dataˉaddress - Baseˉverified.Layout.Dataˉsectionˉaddress);
        var Importˉaddress = Dataˉaddress;
        var Runtimeˉaddress = checked(
            Dataˉaddress + Windowsˉwvbˉpublisherˉimports.PAGE_BYTES);
        var Relocationˉaddress = checked(
            Baseˉverified.Layout.Relocationˉaddress + Virtualˉdelta);
        var Dataˉfile = checked(
            Baseˉverified.Layout.Dataˉfileˉoffset + Fileˉdelta);
        var Importˉfile = checked(
            Baseˉverified.Layout.Importˉfileˉoffset + Fileˉdelta);
        var Runtimeˉfile = checked(
            Baseˉverified.Layout.Runtimeˉfileˉoffset + Fileˉdelta);
        var Relocationˉfile = checked(
            Baseˉverified.Layout.Relocationˉfileˉoffset + Fileˉdelta);
        var Targets = Wvoˉstagingˉpublisherˉapplicationˉbuilder.Windowsˉtargets(
            Baseˉverified.Runtime.Layout,
            Baseˉverified.Layout.Textˉaddress,
            Importˉaddress,
            Runtimeˉaddress,
            Bundle,
            Input);
        var (Sequenceˉbytes, Sequenceˉexports) =
            Wvbˉpublisherˉapplicationˉbuilder.Instantiateˉobject(
                Sequence,
                checked(Baseˉverified.Layout.Textˉaddress + Sequenceˉoffset),
                Targets);
        foreach (var Export in Sequenceˉexports)
        {
            Targets.Add(Export.Key, Export.Value);
        }
        var (Snapshotˉbytes, Snapshotˉexports) =
            Wvbˉpublisherˉapplicationˉbuilder.Instantiateˉobject(
                Snapshot,
                checked(Baseˉverified.Layout.Textˉaddress + Snapshotˉoffset),
                Targets);
        foreach (var Export in Snapshotˉexports)
        {
            Targets.Add(Export.Key, Export.Value);
        }
        var (Transactionˉbytes, Transactionˉexports) =
            Wvbˉpublisherˉapplicationˉbuilder.Instantiateˉobject(
                Transaction,
                checked(Baseˉverified.Layout.Textˉaddress + Transactionˉoffset),
                Targets);
        foreach (var Export in Transactionˉexports)
        {
            Targets.Add(Export.Key, Export.Value);
        }
        var (Shellˉbytes, Shellˉexports) =
            Wvbˉpublisherˉapplicationˉbuilder.Instantiateˉobject(
                Shell,
                checked(Baseˉverified.Layout.Textˉaddress + Shellˉoffset),
                Targets);
        foreach (var Export in Shellˉexports)
        {
            Targets.Add(Export.Key, Export.Value);
        }
        var Adapterˉexport = Adapter.Symbols.Single(Item =>
            Item.Binding == Objectˉsymbolˉbinding.Export &&
            Item.Name == "Windows_wvo_staging_publisher_run");
        Targets["Windows_wvo_staging_publisher_run"] = checked(
            Baseˉverified.Layout.Textˉaddress +
            Adapterˉoffset +
            Adapterˉexport.Offset);
        var (Adapterˉbytes, _) =
            Wvbˉpublisherˉapplicationˉbuilder.Instantiateˉobject(
                Adapter,
                checked(Baseˉverified.Layout.Textˉaddress + Adapterˉoffset),
                Targets);
        var (Startupˉbytes, Startupˉexports) =
            Wvbˉpublisherˉapplicationˉbuilder.Instantiateˉobject(
                Startup,
                Baseˉverified.Layout.Textˉaddress,
                Targets);
        var Startupˉentry = checked(
            Startupˉexports["Windows_wvo_staging_publisher_startup"] -
            Baseˉverified.Layout.Textˉaddress);
        if (Startupˉbytes.Length >
            Windowsˉhostedˉcompilerˉapplicationˉcontract.BUNDLE_TEXT_OFFSET)
        {
            throw new InvalidDataException(
                "The Windows staged-WVO publisher startup exceeds its fixed extent.");
        }

        var Application = new byte[checked(
            Baseˉapplication.Length + (int)Fileˉdelta)];
        Baseˉapplication.AsSpan(
            0,
            checked((int)Baseˉverified.Layout.Dataˉfileˉoffset)).CopyTo(Application);
        Baseˉapplication.AsSpan()[
            checked((int)Baseˉverified.Layout.Dataˉfileˉoffset)..].CopyTo(
                Application.AsSpan(checked((int)Dataˉfile)));
        Application.AsSpan(
            Baseˉverified.Layout.Textˉfileˉoffset,
            Windowsˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET).Clear();
        Startupˉbytes.CopyTo(Application.AsSpan(
            Baseˉverified.Layout.Textˉfileˉoffset));
        Adapterˉbytes.CopyTo(Application.AsSpan(checked(
            Baseˉverified.Layout.Textˉfileˉoffset + (int)Adapterˉoffset)));
        Shellˉbytes.CopyTo(Application.AsSpan(checked(
            Baseˉverified.Layout.Textˉfileˉoffset + (int)Shellˉoffset)));
        Transactionˉbytes.CopyTo(Application.AsSpan(checked(
            Baseˉverified.Layout.Textˉfileˉoffset + (int)Transactionˉoffset)));
        Snapshotˉbytes.CopyTo(Application.AsSpan(checked(
            Baseˉverified.Layout.Textˉfileˉoffset + (int)Snapshotˉoffset)));
        Sequenceˉbytes.CopyTo(Application.AsSpan(checked(
            Baseˉverified.Layout.Textˉfileˉoffset + (int)Sequenceˉoffset)));
        var Imports = Windowsˉwvbˉpublisherˉimports.Build(Importˉaddress);
        Imports.AsSpan().CopyTo(Application.AsSpan(checked((int)Importˉfile)));
        Windowsˉwvbˉpublisherˉimports.Verify(
            Application.AsSpan(
                checked((int)Importˉfile),
                Windowsˉwvbˉpublisherˉimports.PAGE_BYTES),
            Importˉaddress);

        BinaryPrimitives.WriteUInt32LittleEndian(
            Application.AsSpan(0x98 + 4, sizeof(uint)),
            Textˉfile);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Application.AsSpan(0x98 + 16, sizeof(uint)),
            checked(Baseˉverified.Layout.Textˉaddress + Startupˉentry));
        Application[0x98 + 2] = 5;
        BinaryPrimitives.WriteUInt32LittleEndian(
            Application.AsSpan(0x98 + 56, sizeof(uint)),
            checked(Baseˉverified.Layout.Imageˉvirtualˉbytes + Virtualˉdelta));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Application.AsSpan(0x98 + 120, sizeof(uint)),
            checked(Importˉaddress +
                Windowsˉwvbˉpublisherˉimports.DIRECTORY_OFFSET));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Application.AsSpan(0x98 + 152, sizeof(uint)),
            Relocationˉaddress);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Application.AsSpan(0x98 + 208, sizeof(uint)),
            checked(Importˉaddress +
                Windowsˉwvbˉpublisherˉimports.KERNEL_IAT_OFFSET));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Application.AsSpan(0x98 + 212, sizeof(uint)),
            Windowsˉwvbˉpublisherˉimports.IAT_BYTES);
        Wvoˉstagingˉpublisherˉapplicationˉbuilder.Writeˉmetadata(
            Application.AsSpan(checked(
                (int)Runtimeˉfile +
                (int)Hostedˉcompilerˉruntimeˉdata.METADATA_OFFSET +
                Hostedˉcompilerˉapplicationˉmetadata.SIZE),
            128),
            Consoleˉapplicationˉtarget.Windowsˉx64,
            Startupˉbytes,
            Startupˉentry,
            Input);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Application.AsSpan(0x188 + 8, sizeof(uint)),
            Textˉvirtual);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Application.AsSpan(0x188 + 16, sizeof(uint)),
            Textˉfile);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Application.AsSpan(0x188 + 40 + 12, sizeof(uint)),
            Dataˉaddress);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Application.AsSpan(0x188 + 40 + 20, sizeof(uint)),
            Dataˉfile);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Application.AsSpan(0x188 + 80 + 12, sizeof(uint)),
            Relocationˉaddress);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Application.AsSpan(0x188 + 80 + 20, sizeof(uint)),
            Relocationˉfile);
        return Application;
    }
}
