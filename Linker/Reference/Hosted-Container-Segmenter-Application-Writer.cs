using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉcontainerˉsegmenterˉapplicationˉwriter
{
    private const string MODULE_NAME = "Nativeˉhostedˉcontainerˉsegmenterˉtool";

    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname) =>
        Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉwindows(
            fragment,
            capabilities,
            moduleˉname,
            MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Hostedˉcontainerˉsegmenter,
            "container-segmenter",
            "WVW2201");

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname) =>
        Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉlinux(
            fragment,
            capabilities,
            moduleˉname,
            MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Hostedˉcontainerˉsegmenter,
            "container-segmenter",
            "WVL2201");
}
