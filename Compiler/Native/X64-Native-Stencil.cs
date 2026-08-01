using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

public enum Nativeˉstencilˉpatchˉkind : uint
{
    Executionˉcontextˉu8ˉoffset = 1,
}

public sealed record Nativeˉstencil(
    ImmutableArray<byte> Template,
    uint Patchˉoffset,
    uint Patchˉwidth,
    Nativeˉstencilˉpatchˉkind Patchˉkind);

public static class X64ˉnativeˉstencil
{
    private const string ARGUMENT_COUNT_RESOURCE =
        "Windvale.NativeCompiler.Process-Argument-Count.wvo";
    private const uint PATCH_MAGIC = 0x5053_5657;
    private const uint PATCH_VERSION = 1;
    private const int PATCH_BYTES = 20;
    private static readonly ImmutableArray<byte> ARGUMENT_COUNT_SHELL =
        [0x41, 0x8B, 0x47, 0x00, 0xC3];
    private static readonly Nativeˉstencil PROCESS_ARGUMENT_COUNT =
        Loadˉprocessˉargumentˉcount();

    public static ImmutableArray<byte> Buildˉprocessˉargumentˉcount() => Instantiateˉu8(
        PROCESS_ARGUMENT_COUNT,
        Nativeˉstencilˉpatchˉkind.Executionˉcontextˉu8ˉoffset,
        checked((byte)Nativeˉexecutionˉcontextˉcontract.ARGUMENT_COUNT_OFFSET));

    public static Nativeˉstencil Readˉprocessˉargumentˉcount(Verifiedˉobject value)
    {
        var Object = value.Value;
        if (Object.Architecture != Objectˉarchitecture.X86ˉ64 ||
            Object.Sections.Length != 1 ||
            Object.Symbols.Length != 2 ||
            Object.Relocations.Length != 0)
        {
            throw Invalid();
        }

        var Section = Object.Sections[0];
        if (Section.Name != ".rodata" ||
            Section.Kind != Objectˉsectionˉkind.Readˉonlyˉdata ||
            Section.Alignment != 1 ||
            Section.Memoryˉsize != PATCH_BYTES + ARGUMENT_COUNT_SHELL.Length ||
            Section.Data.Length != Section.Memoryˉsize)
        {
            throw Invalid();
        }

        var Patchˉsymbol = Object.Symbols[0];
        var Stencilˉsymbol = Object.Symbols[1];
        if (Patchˉsymbol != new Objectˉsymbol(
                "Argument_count_patch",
                Objectˉsymbolˉbinding.Local,
                Objectˉsymbolˉkind.Data,
                0,
                0,
                PATCH_BYTES) ||
            Stencilˉsymbol != new Objectˉsymbol(
                "Process_argument_count_stencil",
                Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Data,
                0,
                PATCH_BYTES,
                checked((uint)ARGUMENT_COUNT_SHELL.Length)))
        {
            throw Invalid();
        }

        var Patch = Section.Data.AsSpan(0, PATCH_BYTES);
        if (BinaryPrimitives.ReadUInt32LittleEndian(Patch) != PATCH_MAGIC ||
            BinaryPrimitives.ReadUInt32LittleEndian(Patch[4..]) != PATCH_VERSION)
        {
            throw Invalid();
        }

        var Patchˉoffset = BinaryPrimitives.ReadUInt32LittleEndian(Patch[8..]);
        var Patchˉwidth = BinaryPrimitives.ReadUInt32LittleEndian(Patch[12..]);
        var Rawˉkind = BinaryPrimitives.ReadUInt32LittleEndian(Patch[16..]);
        var Template = Section.Data.AsSpan()[PATCH_BYTES..].ToImmutableArray();
        if (Patchˉoffset != 3 ||
            Patchˉwidth != 1 ||
            Rawˉkind != (uint)Nativeˉstencilˉpatchˉkind.Executionˉcontextˉu8ˉoffset ||
            !Template.AsSpan().SequenceEqual(ARGUMENT_COUNT_SHELL.AsSpan()))
        {
            throw Invalid();
        }

        return new(
            Template,
            Patchˉoffset,
            Patchˉwidth,
            Nativeˉstencilˉpatchˉkind.Executionˉcontextˉu8ˉoffset);
    }

    public static ImmutableArray<byte> Instantiateˉu8(
        Nativeˉstencil stencil,
        Nativeˉstencilˉpatchˉkind kind,
        byte value)
    {
        if (stencil.Patchˉkind != kind ||
            stencil.Patchˉwidth != 1 ||
            stencil.Patchˉoffset >= (uint)stencil.Template.Length ||
            stencil.Template[checked((int)stencil.Patchˉoffset)] != 0)
        {
            throw Invalid();
        }

        var Code = stencil.Template.ToArray();
        Code[checked((int)stencil.Patchˉoffset)] = value;
        return Code.ToImmutableArray();
    }

    private static Nativeˉstencil Loadˉprocessˉargumentˉcount()
    {
        using var Stream = typeof(X64ˉnativeˉstencil).Assembly
            .GetManifestResourceStream(ARGUMENT_COUNT_RESOURCE) ?? throw Invalid();
        if (Stream.Length > Objectˉlimits.MAX_OBJECT_BYTES)
        {
            throw Invalid();
        }

        using var Buffer = new MemoryStream(checked((int)Stream.Length));
        Stream.CopyTo(Buffer);
        return Readˉprocessˉargumentˉcount(
            Objectˉcodec.Readˉandˉverify(Buffer.GetBuffer().AsSpan(0, checked((int)Buffer.Length))));
    }

    private static InvalidOperationException Invalid() => new(
        "The WVA native stencil does not match the bounded process.argument_count contract.");
}
