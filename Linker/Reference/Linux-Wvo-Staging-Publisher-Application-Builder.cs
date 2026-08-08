using System.Buffers.Binary;
using Windvale.Compiler.Native;
using Windvale.ObjectModel;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Linuxˉwvoˉstagingˉpublisherˉapplicationˉbuilder
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
            Wvoˉstagingˉpublisherˉapplicationˉbuilder.LINUX_STARTUP_RESOURCE);
        var Adapter = Wvoˉstagingˉpublisherˉapplicationˉbuilder.Readˉobject(
            Wvoˉstagingˉpublisherˉapplicationˉbuilder.LINUX_ADAPTER_RESOURCE);
        var Transaction = Wvoˉstagingˉpublisherˉapplicationˉbuilder.Readˉobject(
            Wvoˉstagingˉpublisherˉapplicationˉbuilder.LINUX_TRANSACTION_RESOURCE);
        var Snapshot = Wvoˉstagingˉpublisherˉapplicationˉbuilder.Readˉobject(
            Wvoˉstagingˉpublisherˉapplicationˉbuilder.SNAPSHOT_TABLE_RESOURCE);
        var Sequence = Wvoˉstagingˉpublisherˉapplicationˉbuilder.Readˉobject(
            Wvoˉstagingˉpublisherˉapplicationˉbuilder
                .IMMUTABLE_SNAPSHOT_SEQUENCE_RESOURCE);

        var Bundle = Wvoˉstagingˉpublisherˉapplicationˉbuilder.Buildˉcontainerˉbundle(
            Input.Fragment,
            Nativeˉserviceˉplatform.Linux);
        var Baseˉapplication = Linuxˉhostedˉcompilerˉapplicationˉbuilder.Build(
            Wvoˉstagingˉpublisherˉapplicationˉbuilder.Containerˉcapabilities(),
            Bundle,
            Input.Nativeˉentry,
            Hostedˉcompilerˉapplicationˉprofile.Compiler);
        var Baseˉverified = Linuxˉhostedˉcompilerˉapplicationˉverifier.Verify(
            Baseˉapplication.AsSpan(),
            Bundle,
            Hostedˉcompilerˉapplicationˉprofile.Compiler);

        var Adapterˉfileˉoffset = Wvbˉpublisherˉapplicationˉbuilder.Alignˉup(
            checked((uint)Baseˉapplication.Length),
            0x1000);
        var Adapterˉaddress = Wvbˉpublisherˉapplicationˉbuilder.Alignˉup(
            Baseˉverified.Layout.Imageˉvirtualˉbytes,
            0x1000);
        var Adapterˉsection = Adapter.Sections.Single(Item =>
            Item.Kind == Objectˉsectionˉkind.Code);
        var Transactionˉoffset = Wvbˉpublisherˉapplicationˉbuilder.Alignˉup(
            Adapterˉsection.Memoryˉsize,
            16);
        var Snapshotˉoffset = Wvbˉpublisherˉapplicationˉbuilder.Alignˉup(
            checked(Transactionˉoffset + Transaction.Sections.Single(Item =>
                Item.Kind == Objectˉsectionˉkind.Code).Memoryˉsize),
            16);
        var Sequenceˉoffset = Wvbˉpublisherˉapplicationˉbuilder.Alignˉup(
            checked(Snapshotˉoffset + Snapshot.Sections.Single(Item =>
                Item.Kind == Objectˉsectionˉkind.Code).Memoryˉsize),
            16);
        var Targets = Wvoˉstagingˉpublisherˉapplicationˉbuilder.Linuxˉtargets(
            Baseˉverified,
            Bundle,
            Input);
        var (Sequenceˉbytes, Sequenceˉexports) =
            Wvbˉpublisherˉapplicationˉbuilder.Instantiateˉobject(
                Sequence,
                checked(Adapterˉaddress + Sequenceˉoffset),
                Targets);
        foreach (var Export in Sequenceˉexports)
        {
            Targets.Add(Export.Key, Export.Value);
        }
        var (Snapshotˉbytes, Snapshotˉexports) =
            Wvbˉpublisherˉapplicationˉbuilder.Instantiateˉobject(
                Snapshot,
                checked(Adapterˉaddress + Snapshotˉoffset),
                Targets);
        foreach (var Export in Snapshotˉexports)
        {
            Targets.Add(Export.Key, Export.Value);
        }
        var (Transactionˉbytes, Transactionˉexports) =
            Wvbˉpublisherˉapplicationˉbuilder.Instantiateˉobject(
                Transaction,
                checked(Adapterˉaddress + Transactionˉoffset),
                Targets);
        foreach (var Export in Transactionˉexports)
        {
            Targets.Add(Export.Key, Export.Value);
        }
        var Adapterˉexport = Adapter.Symbols.Single(Item =>
            Item.Binding == Objectˉsymbolˉbinding.Export &&
            Item.Name == "Linux_wvo_staging_publisher_run");
        Targets["Linux_wvo_staging_publisher_run"] = checked(
            Adapterˉaddress + Adapterˉexport.Offset);
        var (Adapterˉbytes, _) =
            Wvbˉpublisherˉapplicationˉbuilder.Instantiateˉobject(
                Adapter,
                Adapterˉaddress,
                Targets);
        var Adapterˉsegment = new byte[checked(
            (int)Sequenceˉoffset + Sequenceˉbytes.Length)];
        Adapterˉbytes.CopyTo(Adapterˉsegment);
        Transactionˉbytes.CopyTo(Adapterˉsegment.AsSpan(
            checked((int)Transactionˉoffset)));
        Snapshotˉbytes.CopyTo(Adapterˉsegment.AsSpan(
            checked((int)Snapshotˉoffset)));
        Sequenceˉbytes.CopyTo(Adapterˉsegment.AsSpan(
            checked((int)Sequenceˉoffset)));

        var (Startupˉbytes, Startupˉexports) =
            Wvbˉpublisherˉapplicationˉbuilder.Instantiateˉobject(
                Startup,
                Baseˉverified.Layout.Textˉaddress,
                Targets);
        var Startupˉentry = checked(
            Startupˉexports["Linux_wvo_staging_publisher_startup"] -
            Baseˉverified.Layout.Textˉaddress);
        if (Startupˉbytes.Length >
            Linuxˉhostedˉcompilerˉapplicationˉcontract.BUNDLE_TEXT_OFFSET)
        {
            throw new InvalidDataException(
                "The Linux staged-WVO publisher startup exceeds its fixed extent.");
        }

        var Application = new byte[checked(
            (int)Adapterˉfileˉoffset + Adapterˉsegment.Length)];
        Baseˉapplication.AsSpan().CopyTo(Application);
        Adapterˉsegment.CopyTo(Application.AsSpan(
            checked((int)Adapterˉfileˉoffset)));
        var Note = Application.AsSpan(0x180, 28).ToArray();
        Application.AsSpan(0x180, 28).Clear();
        Note.CopyTo(Application.AsSpan(0x200));
        BinaryPrimitives.WriteUInt16LittleEndian(
            Application.AsSpan(56, sizeof(ushort)),
            6);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Application.AsSpan(0xE8 + 8, sizeof(ulong)),
            0x200);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Application.AsSpan(0xE8 + 16, sizeof(ulong)),
            0x200);
        Writeˉsegment(
            Application.AsSpan(0x158, 56),
            Adapterˉfileˉoffset,
            Adapterˉaddress,
            checked((uint)Adapterˉsegment.Length));
        Application.AsSpan(
            Baseˉverified.Layout.Textˉfileˉoffset,
            Linuxˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET).Clear();
        Startupˉbytes.CopyTo(Application.AsSpan(
            Baseˉverified.Layout.Textˉfileˉoffset));
        BinaryPrimitives.WriteUInt64LittleEndian(
            Application.AsSpan(24, sizeof(ulong)),
            checked((ulong)(Baseˉverified.Layout.Textˉaddress + Startupˉentry)));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Application.AsSpan(0x200 + 24, sizeof(uint)),
            5);
        Wvoˉstagingˉpublisherˉapplicationˉbuilder.Writeˉmetadata(
            Application.AsSpan(checked(
                (int)Baseˉverified.Layout.Dataˉfileˉoffset +
                (int)Hostedˉcompilerˉruntimeˉdata.METADATA_OFFSET +
                Hostedˉcompilerˉapplicationˉmetadata.SIZE),
            128),
            Consoleˉapplicationˉtarget.Linuxˉx64,
            Startupˉbytes,
            Startupˉentry,
            Input);
        return Application;
    }

    private static void Writeˉsegment(
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
}
