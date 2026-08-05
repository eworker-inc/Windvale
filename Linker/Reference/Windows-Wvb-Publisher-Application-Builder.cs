using System.Buffers.Binary;
using Windvale.Compiler.Native;
using Windvale.ObjectModel;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Windowsˉwvbˉpublisherˉapplicationˉbuilder
{
    internal static byte[] Build(
        Windvale.Bytecode.Verifiedˉmodule module,
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes)
    {
        var Input = Wvbˉpublisherˉapplicationˉbuilder.Validateˉinput(
            module,
            fragment,
            moduleˉbytes);
        var (_, Startup) = Wvbˉpublisherˉapplicationˉbuilder.Readˉobject(
            Wvbˉpublisherˉapplicationˉbuilder.WINDOWS_STARTUP_RESOURCE);
        var (_, Adapter) = Wvbˉpublisherˉapplicationˉbuilder.Readˉobject(
            Wvbˉpublisherˉapplicationˉbuilder.WINDOWS_ADAPTER_RESOURCE);
        var (_, Sha256) = Wvbˉpublisherˉapplicationˉbuilder.Readˉobject(
            Wvbˉpublisherˉapplicationˉbuilder.SHA256_RESOURCE);

        var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉverifier(
            Input.Fragment,
            Nativeˉserviceˉplatform.Windows);
        var Baseˉapplication = Windowsˉhostedˉverifierˉapplicationˉbuilder.Build(
            Input.Module.Module.Capabilities,
            Bundle,
            Input.Nativeˉentry);
        var Baseˉverified = Windowsˉhostedˉverifierˉapplicationˉverifier.Verify(
            Baseˉapplication.AsSpan(),
            Bundle);

        var Adapterˉsection = Adapter.Sections.Single(Item =>
            Item.Kind == Objectˉsectionˉkind.Code);
        var Adapterˉoffset = Wvbˉpublisherˉapplicationˉbuilder.Alignˉup(
            checked((uint)(
                Windowsˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
                Bundle.Imageˉbytes.Length)),
            16);
        var Sha256ˉoffset = Wvbˉpublisherˉapplicationˉbuilder.Alignˉup(checked(
            Adapterˉoffset + Adapterˉsection.Memoryˉsize), 16);
        var (Sha256ˉbytes, Sha256ˉexports) =
            Wvbˉpublisherˉapplicationˉbuilder.Instantiateˉobject(
                Sha256,
                checked(Baseˉverified.Layout.Textˉaddress + Sha256ˉoffset),
                new Dictionary<string, uint>(StringComparer.Ordinal));
        var Textˉvirtual = checked(
            Sha256ˉoffset + (uint)Sha256ˉbytes.Length);
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
        var Targets = Wvbˉpublisherˉapplicationˉbuilder.Windowsˉtargets(
            Baseˉverified.Runtime.Layout,
            Baseˉverified.Layout.Textˉaddress,
            Importˉaddress,
            Runtimeˉaddress,
            Bundle,
            Input);
        foreach (var Export in Sha256ˉexports)
        {
            Targets.Add(Export.Key, Export.Value);
        }
        var Adapterˉexport = Adapter.Symbols.Single(Item =>
            Item.Binding == Objectˉsymbolˉbinding.Export &&
            Item.Name == "Windows_wvb_publisher_run");
        Targets["Windows_wvb_publisher_run"] = checked(
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
            Startupˉexports["Windows_wvb_publisher_startup"] -
            Baseˉverified.Layout.Textˉaddress);
        if (Startupˉbytes.Length >
            Windowsˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET)
        {
            throw new InvalidDataException(
                "The Windows publisher startup exceeds its fixed pre-bundle extent.");
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
        Sha256ˉbytes.CopyTo(Application.AsSpan(checked(
            Baseˉverified.Layout.Textˉfileˉoffset + (int)Sha256ˉoffset)));
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
        Wvbˉpublisherˉapplicationˉbuilder.Writeˉmetadata(
            Application.AsSpan(checked(
                (int)Runtimeˉfile +
                (int)Hostedˉverifierˉruntimeˉdata.METADATA_OFFSET +
                Hostedˉverifierˉapplicationˉmetadata.SIZE),
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
