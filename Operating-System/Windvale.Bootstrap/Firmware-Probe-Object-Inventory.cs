using System.Collections.Immutable;
using Windvale.Compiler;
using Windvale.Linker;
using Windvale.ObjectModel;

namespace Windvale.Bootstrap;

public sealed record Firmwareˉprobeˉobject(
    string Fileˉname,
    ImmutableArray<byte> Bytes);

public sealed record Firmwareˉprobeˉobjectˉinventory(
    string Entryˉsymbol,
    ImmutableArray<Firmwareˉprobeˉobject> Objects);

public enum Firmwareˉprobeˉobjectˉinventoryˉscope
{
    Complete = 0,
    Nativeˉwvaˉexternal = 1,
}

public static partial class Firmwareˉprobe
{
    private static ImmutableArray<byte> Packˉnativeˉbridgeˉandˉsupport(
        ImmutableArray<byte> nativeˉbridgeˉbytes,
        ImmutableArray<byte> supportˉbytes)
    {
        var Nativeˉbridge = Objectˉcodec.Readˉandˉverify(nativeˉbridgeˉbytes.AsSpan()).Value;
        var Support = Objectˉcodec.Readˉandˉverify(supportˉbytes.AsSpan()).Value;
        if (Nativeˉbridge.Architecture != Objectˉarchitecture.X86ˉ64 ||
            Support.Architecture != Objectˉarchitecture.X86ˉ64 ||
            Nativeˉbridge.Sections.Length != 1 ||
            Support.Sections.Length != 1 ||
            Nativeˉbridge.Symbols.Length != 3 ||
            Support.Symbols.Length != 1 ||
            Nativeˉbridge.Relocations.Length != 2 ||
            !Support.Relocations.IsEmpty ||
            Nativeˉbridge.Symbols[0] is not
                {
                    Name: Kernelˉnativeˉprobeˉcontract.BRIDGE_SYMBOL,
                    Binding: Objectˉsymbolˉbinding.Export,
                    Sectionˉindex: 0,
                } ||
            Nativeˉbridge.Symbols[1] is not
                {
                    Name: Kernelˉnativeˉprobeˉcontract.NATIVE_MAIN_SYMBOL,
                    Binding: Objectˉsymbolˉbinding.Import,
                } ||
            Nativeˉbridge.Symbols[2] is not
                {
                    Name: X64ˉkernelˉcontract.KERNEL_MAIN_SYMBOL,
                    Binding: Objectˉsymbolˉbinding.Import,
                } ||
            Support.Symbols[0] is not
                {
                    Name: X64_WRITE_BYTE_SYMBOL,
                    Binding: Objectˉsymbolˉbinding.Export,
                    Sectionˉindex: 0,
                } ||
            Nativeˉbridge.Relocations[0].Symbolˉindex != 1 ||
            Nativeˉbridge.Relocations[1].Symbolˉindex != 2)
        {
            throw new InvalidOperationException(
                "The Probe 40 native bridge/support packing contract changed.");
        }

        var Packed = new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [
                Nativeˉbridge.Sections[0] with { Name = ".text.native" },
                Support.Sections[0] with { Name = ".text.support" },
            ],
            [
                Nativeˉbridge.Symbols[0],
                Support.Symbols[0] with { Sectionˉindex = 1 },
                Nativeˉbridge.Symbols[1],
                Nativeˉbridge.Symbols[2],
            ],
            [.. Nativeˉbridge.Relocations.Select(
                Relocation => Relocation with { Symbolˉindex = Relocation.Symbolˉindex + 1 })]);
        return Objectˉcodec.Write(Packed).ToImmutableArray();
    }

    public static Firmwareˉprobeˉobjectˉinventory Buildˉobjectˉinventory(
        Firmwareˉprobeˉscenario scenario = Firmwareˉprobeˉscenario.Normal,
        Firmwareˉprobeˉobjectˉinventoryˉscope scope =
            Firmwareˉprobeˉobjectˉinventoryˉscope.Complete)
    {
        if (scenario is not Firmwareˉprobeˉscenario.Normal and
            not Firmwareˉprobeˉscenario.Invalidˉopcode and
            not Firmwareˉprobeˉscenario.Generalˉprotection and
            not Firmwareˉprobeˉscenario.Userˉfault and
            not Firmwareˉprobeˉscenario.Serviceˉfault)
        {
            throw new ArgumentOutOfRangeException(nameof(scenario));
        }
        if (scope is not Firmwareˉprobeˉobjectˉinventoryˉscope.Complete and
            not Firmwareˉprobeˉobjectˉinventoryˉscope.Nativeˉwvaˉexternal)
        {
            throw new ArgumentOutOfRangeException(nameof(scope));
        }

        var Kernelˉsourceˉname = scenario == Firmwareˉprobeˉscenario.Serviceˉfault
            ? "Hello-Service-Fault.wv"
            : "Hello-World.wv";
        var Kernel = X64ˉkernelˉcompiler.Compile(
            Loadˉhelloˉworldˉsource(scenario), Kernelˉsourceˉname);
        if (!Kernel.Success)
        {
            throw new InvalidOperationException(
                $"The Windvale kernel source did not compile: {Kernel.Diagnostics[0]}");
        }
        var Admission = Kernelˉwvbˉadmission.Build();
        var Processˉscenario = scenario switch
        {
            Firmwareˉprobeˉscenario.Userˉfault => Kernelˉprocessˉscenario.Userˉfault,
            Firmwareˉprobeˉscenario.Serviceˉfault => Kernelˉprocessˉscenario.Serviceˉfault,
            _ => Kernelˉprocessˉscenario.Normal,
        };
        var Processˉimage = Kernelˉprocessˉimage.Build(Admission, Processˉscenario);
        var Process = Kernelˉprocessˉx64.Build(Processˉimage, Processˉscenario);
        var Nativeˉprobe = Kernelˉnativeˉprobe.Build();
        var Exceptions = Kernelˉexceptionˉx64.Build();
        var Paging = Kernelˉpagingˉx64.Build();

        var Loader = Buildˉloaderˉmachineˉcode(scenario);
        var Loaderˉobject = new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 16, (uint)Loader.Bytes.Length, Loader.Bytes)],
            [
                new(
                    ENTRY_SYMBOL,
                    Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function,
                    0,
                    0,
                    (uint)Loader.Bytes.Length),
                new(
                    KERNEL_ENTRY_SYMBOL,
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
                new(
                    Kernelˉassemblyˉcontract.Q35_SHUTDOWN_SYMBOL,
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
            ],
            [
                new(Objectˉrelocationˉkind.Relativeˉi32, 0, Loader.Kernelˉcallˉoffset, 1, -4),
                new(Objectˉrelocationˉkind.Relativeˉi32, 0, Loader.Shutdownˉcallˉoffset, 2, -4),
            ]);
        var Supportˉcode = Buildˉwriteˉbyteˉmachineˉcode();
        var Supportˉobject = new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 16, (uint)Supportˉcode.Length, Supportˉcode)],
            [new(
                X64_WRITE_BYTE_SYMBOL,
                Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Function,
                0,
                0,
                (uint)Supportˉcode.Length)],
            []);
        var Memory = Kernelˉmemoryˉx64.Build(scenario);
        var Memoryˉobject = new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 16, (uint)Memory.Bytes.Length, Memory.Bytes)],
            [
                new(
                    Kernelˉmemoryˉcontract.ALLOCATE_PAGES_SYMBOL,
                    Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function,
                    0,
                    Memory.Allocatorˉoffset,
                    checked((uint)Memory.Bytes.Length - Memory.Allocatorˉoffset)),
                new(
                    Kernelˉmemoryˉcontract.MEMORY_ENTER_SYMBOL,
                    Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function,
                    0,
                    0,
                    Memory.Enterˉbytes),
                new(
                    Kernelˉmemoryˉcontract.ALLOCATE_MEMORY_OBJECT_SYMBOL,
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
                new(
                    Kernelˉassemblyˉcontract.MAIN_SHIM_SYMBOL,
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
                new(
                    Kernelˉexceptionˉcontract.INSTALL_SYMBOL,
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
                new(
                    Kernelˉpagingˉcontract.INSTALL_SYMBOL,
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
            ],
            [.. Memory.Relocations.Select(Relocation => new Objectˉrelocation(
                Objectˉrelocationˉkind.Relativeˉi32,
                0,
                Relocation.Offset,
                Relocation.Symbolˉindex,
                -4))]);
        var Loaderˉobjectˉbytes = Objectˉcodec.Write(Loaderˉobject).ToImmutableArray();
        var Memoryˉobjectˉbytes = Objectˉcodec.Write(Memoryˉobject).ToImmutableArray();
        var Supportˉobjectˉbytes = Objectˉcodec.Write(Supportˉobject).ToImmutableArray();
        var Objects = ImmutableArray.CreateBuilder<Firmwareˉprobeˉobject>(14);
        Objects.Add(new("00-loader.wvo", Loaderˉobjectˉbytes));
        Objects.Add(new("01-kernel.wvo", Kernel.Objectˉbytes));
        Objects.Add(new("02-wvb-admission-native.wvo", Admission.Admissionˉnativeˉobjectˉbytes));
        Objects.Add(new("03-native-wvb-probe.wvo", Nativeˉprobe.Nativeˉobjectˉbytes));
        Objects.Add(new("04-process-policy.wvo", Processˉimage.Policyˉnativeˉobjectˉbytes));
        Objects.Add(new("05-process.wvo", Process.Objectˉbytes));
        if (scope == Firmwareˉprobeˉobjectˉinventoryˉscope.Complete)
        {
            Objects.Add(new(
                "06-memory-object-shims.wvo",
                Kernelˉmemoryˉassemblyˉshim.Buildˉobject()));
            Objects.Add(new("07-timer-shims.wvo", Kernelˉtimerˉassemblyˉshim.Buildˉobject()));
        }
        Objects.Add(new("08-memory.wvo", Memoryˉobjectˉbytes));
        Objects.Add(new("09-exceptions.wvo", Exceptions.Objectˉbytes));
        Objects.Add(new("10-paging.wvo", Paging.Objectˉbytes));
        if (scope == Firmwareˉprobeˉobjectˉinventoryˉscope.Complete)
        {
            Objects.Add(new("11-kernel-shims.wvo", Kernelˉassemblyˉshim.Buildˉobject()));
        }
        Objects.Add(new("12-wvb-admission-bridge.wvo", Admission.Bridgeˉobjectˉbytes));
        Objects.Add(new(
            "13-native-bridge-and-support.wvo",
            Packˉnativeˉbridgeˉandˉsupport(
                Nativeˉprobe.Bridgeˉobjectˉbytes,
                Supportˉobjectˉbytes)));
        return new(ENTRY_SYMBOL, Objects.ToImmutable());
    }

    public static Linkˉresult Buildˉlinkedˉimage(
        Firmwareˉprobeˉscenario scenario = Firmwareˉprobeˉscenario.Normal)
    {
        var Inventory = Buildˉobjectˉinventory(scenario);
        var Link = Linkˉcompiler.Link(
            [.. Inventory.Objects.Select(Object => new Linkˉinput(Object.Bytes))],
            new(
                Uefiˉapplicationˉcontract.REQUIRED_LINK_BASE_ADDRESS,
                Inventory.Entryˉsymbol));
        if (!Link.Success)
        {
            throw new InvalidOperationException(
                $"The firmware probe did not link: {Link.Diagnostics[0].Code}: " +
                Link.Diagnostics[0].Message);
        }
        if (Link.Entryˉaddress != 0 ||
            Link.Imageˉbytes.Length > (int)Kernelˉpagingˉcontract.EXECUTABLE_BYTES)
        {
            throw new InvalidOperationException(
                $"The linked firmware payload is {Link.Imageˉbytes.Length} bytes and does not fit the fixed " +
                $"{Kernelˉpagingˉcontract.EXECUTABLE_BYTES}-byte executable window.");
        }

        return Link;
    }
}
