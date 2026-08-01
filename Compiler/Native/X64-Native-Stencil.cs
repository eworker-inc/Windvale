using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

public enum Nativeˉstencilˉpatchˉkind : uint
{
    Executionˉcontextˉu8ˉoffset = 1,
    Executionˉcontextˉserviceˉfailureˉdetailˉu8ˉoffset = 2,
    Executionˉcontextˉargumentˉcountˉu8ˉoffset = 3,
    Executionˉcontextˉargumentˉtableˉpointerˉu8ˉoffset = 4,
    Borrowedˉtextˉlengthˉu8ˉoffset = 5,
    Borrowedˉtextˉreservedˉu8ˉoffset = 6,
    Argumentˉindexˉoutˉofˉrangeˉu8ˉdetail = 7,
}

public sealed record Nativeˉstencilˉpatch(
    uint Offset,
    uint Width,
    Nativeˉstencilˉpatchˉkind Kind);

public sealed record Nativeˉstencil(
    uint Formatˉversion,
    ImmutableArray<byte> Template,
    ImmutableArray<Nativeˉstencilˉpatch> Patches);

public static class X64ˉnativeˉstencil
{
    private const string ARGUMENT_COUNT_RESOURCE =
        "Windvale.NativeCompiler.Process-Argument-Count.wvo";
    private const string ARGUMENT_RESOURCE =
        "Windvale.NativeCompiler.Process-Argument.wvo";
    private const uint PATCH_MAGIC = 0x5053_5657;
    private const uint SINGLE_PATCH_VERSION = 1;
    private const uint ORDERED_PATCH_VERSION = 2;
    private const int PATCH_RECORD_BYTES = 12;
    private const int VERSION_1_HEADER_BYTES = 8;
    private const int VERSION_2_HEADER_BYTES = 16;
    private const int MAXIMUM_PATCHES = 32;

    private static readonly ImmutableArray<byte> ARGUMENT_COUNT_SHELL =
        [0x41, 0x8B, 0x47, 0x00, 0xC3];
    private static readonly ImmutableArray<Nativeˉstencilˉpatch> ARGUMENT_COUNT_PATCHES =
    [
        new(3, 1, Nativeˉstencilˉpatchˉkind.Executionˉcontextˉu8ˉoffset),
    ];
    private static readonly ImmutableArray<byte> ARGUMENT_SHELL =
    [
        0x41, 0xC7, 0x47, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x45, 0x3B, 0x47, 0x00, 0x0F, 0x83, 0x26, 0x00, 0x00, 0x00,
        0x49, 0x8B, 0x47, 0x00, 0x44, 0x89, 0xC1,
        0x48, 0xC1, 0xE1, 0x04, 0x48, 0x01, 0xC8,
        0x48, 0x8B, 0x08, 0x49, 0x89, 0x09,
        0x8B, 0x48, 0x00, 0x41, 0x89, 0x49, 0x00,
        0x41, 0xC7, 0x41, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x31, 0xC0, 0xC3,
        0x41, 0xC7, 0x47, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xB8, 0x01, 0x00, 0x00, 0x00, 0xC3,
    ];
    private static readonly ImmutableArray<Nativeˉstencilˉpatch> ARGUMENT_PATCHES =
    [
        new(3, 1, Nativeˉstencilˉpatchˉkind.Executionˉcontextˉserviceˉfailureˉdetailˉu8ˉoffset),
        new(11, 1, Nativeˉstencilˉpatchˉkind.Executionˉcontextˉargumentˉcountˉu8ˉoffset),
        new(21, 1, Nativeˉstencilˉpatchˉkind.Executionˉcontextˉargumentˉtableˉpointerˉu8ˉoffset),
        new(40, 1, Nativeˉstencilˉpatchˉkind.Borrowedˉtextˉlengthˉu8ˉoffset),
        new(44, 1, Nativeˉstencilˉpatchˉkind.Borrowedˉtextˉlengthˉu8ˉoffset),
        new(48, 1, Nativeˉstencilˉpatchˉkind.Borrowedˉtextˉreservedˉu8ˉoffset),
        new(59, 1, Nativeˉstencilˉpatchˉkind.Executionˉcontextˉserviceˉfailureˉdetailˉu8ˉoffset),
        new(60, 1, Nativeˉstencilˉpatchˉkind.Argumentˉindexˉoutˉofˉrangeˉu8ˉdetail),
    ];

    private static readonly Nativeˉstencilˉcontract ARGUMENT_COUNT_CONTRACT = new(
        ARGUMENT_COUNT_RESOURCE,
        SINGLE_PATCH_VERSION,
        "Argument_count_patch",
        "Process_argument_count_stencil",
        ARGUMENT_COUNT_SHELL,
        ARGUMENT_COUNT_PATCHES);
    private static readonly Nativeˉstencilˉcontract ARGUMENT_CONTRACT = new(
        ARGUMENT_RESOURCE,
        ORDERED_PATCH_VERSION,
        "Process_argument_patches",
        "Process_argument_stencil",
        ARGUMENT_SHELL,
        ARGUMENT_PATCHES);
    private static readonly Nativeˉstencil PROCESS_ARGUMENT_COUNT = Load(ARGUMENT_COUNT_CONTRACT);
    private static readonly Nativeˉstencil PROCESS_ARGUMENT = Load(ARGUMENT_CONTRACT);

    public static ImmutableArray<byte> Buildˉprocessˉargumentˉcount() => Instantiateˉu8(
        PROCESS_ARGUMENT_COUNT,
        Nativeˉstencilˉpatchˉkind.Executionˉcontextˉu8ˉoffset,
        checked((byte)Nativeˉexecutionˉcontextˉcontract.ARGUMENT_COUNT_OFFSET));

    public static ImmutableArray<byte> Buildˉprocessˉargument() =>
        Instantiateˉprocessˉargument(PROCESS_ARGUMENT);

    public static Nativeˉstencil Readˉprocessˉargumentˉcount(Verifiedˉobject value) =>
        Read(value, ARGUMENT_COUNT_CONTRACT);

    public static Nativeˉstencil Readˉprocessˉargument(Verifiedˉobject value) =>
        Read(value, ARGUMENT_CONTRACT);

    public static ImmutableArray<byte> Instantiateˉu8(
        Nativeˉstencil stencil,
        Nativeˉstencilˉpatchˉkind kind,
        byte value) => Instantiate(
            stencil,
            ImmutableDictionary<Nativeˉstencilˉpatchˉkind, byte>.Empty.Add(kind, value));

    public static ImmutableArray<byte> Instantiateˉprocessˉargument(Nativeˉstencil stencil) =>
        Instantiate(
            stencil,
            ImmutableDictionary<Nativeˉstencilˉpatchˉkind, byte>.Empty
                .Add(
                    Nativeˉstencilˉpatchˉkind.Executionˉcontextˉserviceˉfailureˉdetailˉu8ˉoffset,
                    checked((byte)Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET))
                .Add(
                    Nativeˉstencilˉpatchˉkind.Executionˉcontextˉargumentˉcountˉu8ˉoffset,
                    checked((byte)Nativeˉexecutionˉcontextˉcontract.ARGUMENT_COUNT_OFFSET))
                .Add(
                    Nativeˉstencilˉpatchˉkind.Executionˉcontextˉargumentˉtableˉpointerˉu8ˉoffset,
                    checked((byte)Nativeˉexecutionˉcontextˉcontract.ARGUMENT_TABLE_POINTER_OFFSET))
                .Add(
                    Nativeˉstencilˉpatchˉkind.Borrowedˉtextˉlengthˉu8ˉoffset,
                    checked((byte)Nativeˉcontract.BORROWED_TEXT_LENGTH_OFFSET))
                .Add(
                    Nativeˉstencilˉpatchˉkind.Borrowedˉtextˉreservedˉu8ˉoffset,
                    checked((byte)Nativeˉcontract.BORROWED_TEXT_RESERVED_OFFSET))
                .Add(
                    Nativeˉstencilˉpatchˉkind.Argumentˉindexˉoutˉofˉrangeˉu8ˉdetail,
                    checked((byte)Nativeˉserviceˉfailureˉdetail.Argumentˉindexˉoutˉofˉrange)));

    private static Nativeˉstencil Read(
        Verifiedˉobject value,
        Nativeˉstencilˉcontract contract)
    {
        var Object = value.Value;
        var Metadataˉbytes = contract.Formatˉversion switch
        {
            SINGLE_PATCH_VERSION => checked(VERSION_1_HEADER_BYTES + PATCH_RECORD_BYTES),
            ORDERED_PATCH_VERSION => checked(
                VERSION_2_HEADER_BYTES + (contract.Patches.Length * PATCH_RECORD_BYTES)),
            _ => throw Invalid(),
        };
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
            Section.Memoryˉsize != Metadataˉbytes + contract.Shell.Length ||
            Section.Data.Length != Section.Memoryˉsize)
        {
            throw Invalid();
        }

        if (Object.Symbols[0] != new Objectˉsymbol(
                contract.Patchˉsymbol,
                Objectˉsymbolˉbinding.Local,
                Objectˉsymbolˉkind.Data,
                0,
                0,
                checked((uint)Metadataˉbytes)) ||
            Object.Symbols[1] != new Objectˉsymbol(
                contract.Stencilˉsymbol,
                Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Data,
                0,
                checked((uint)Metadataˉbytes),
                checked((uint)contract.Shell.Length)))
        {
            throw Invalid();
        }

        var Metadata = Section.Data.AsSpan(0, Metadataˉbytes);
        if (BinaryPrimitives.ReadUInt32LittleEndian(Metadata) != PATCH_MAGIC ||
            BinaryPrimitives.ReadUInt32LittleEndian(Metadata[4..]) != contract.Formatˉversion)
        {
            throw Invalid();
        }

        var Patchˉstart = VERSION_1_HEADER_BYTES;
        if (contract.Formatˉversion == ORDERED_PATCH_VERSION)
        {
            if (BinaryPrimitives.ReadUInt32LittleEndian(Metadata[8..]) !=
                    checked((uint)contract.Patches.Length) ||
                BinaryPrimitives.ReadUInt32LittleEndian(Metadata[12..]) !=
                    checked((uint)contract.Shell.Length))
            {
                throw Invalid();
            }

            Patchˉstart = VERSION_2_HEADER_BYTES;
        }

        for (var Index = 0; Index < contract.Patches.Length; Index++)
        {
            var Recordˉoffset = checked(Patchˉstart + (Index * PATCH_RECORD_BYTES));
            var Expected = contract.Patches[Index];
            if (BinaryPrimitives.ReadUInt32LittleEndian(Metadata[Recordˉoffset..]) != Expected.Offset ||
                BinaryPrimitives.ReadUInt32LittleEndian(Metadata[(Recordˉoffset + 4)..]) != Expected.Width ||
                BinaryPrimitives.ReadUInt32LittleEndian(Metadata[(Recordˉoffset + 8)..]) !=
                    (uint)Expected.Kind)
            {
                throw Invalid();
            }
        }

        var Template = Section.Data.AsSpan()[Metadataˉbytes..].ToImmutableArray();
        if (!Template.AsSpan().SequenceEqual(contract.Shell.AsSpan()) ||
            contract.Patches.Any(Patch => Template[checked((int)Patch.Offset)] != 0))
        {
            throw Invalid();
        }

        return new(contract.Formatˉversion, Template, contract.Patches);
    }

    private static ImmutableArray<byte> Instantiate(
        Nativeˉstencil stencil,
        IReadOnlyDictionary<Nativeˉstencilˉpatchˉkind, byte> values)
    {
        if (stencil.Template.IsDefaultOrEmpty ||
            stencil.Template.Length > Nativeˉcontract.MAXIMUM_CODE_BYTES ||
            stencil.Patches.IsDefaultOrEmpty ||
            stencil.Patches.Length > MAXIMUM_PATCHES ||
            (stencil.Formatˉversion == SINGLE_PATCH_VERSION && stencil.Patches.Length != 1) ||
            (stencil.Formatˉversion == ORDERED_PATCH_VERSION && stencil.Patches.Length < 2) ||
            (stencil.Formatˉversion != SINGLE_PATCH_VERSION &&
                stencil.Formatˉversion != ORDERED_PATCH_VERSION))
        {
            throw Invalid();
        }

        var Requiredˉkinds = ImmutableHashSet.CreateBuilder<Nativeˉstencilˉpatchˉkind>();
        uint? Previousˉoffset = null;
        foreach (var Patch in stencil.Patches)
        {
            if (!Enum.IsDefined(Patch.Kind) ||
                Patch.Width != 1 ||
                Patch.Offset >= checked((uint)stencil.Template.Length) ||
                (Previousˉoffset.HasValue && Patch.Offset <= Previousˉoffset.Value) ||
                stencil.Template[checked((int)Patch.Offset)] != 0)
            {
                throw Invalid();
            }

            Requiredˉkinds.Add(Patch.Kind);
            Previousˉoffset = Patch.Offset;
        }

        if (values.Count != Requiredˉkinds.Count ||
            values.Keys.Any(Kind => !Enum.IsDefined(Kind) || !Requiredˉkinds.Contains(Kind)) ||
            Requiredˉkinds.Any(Kind => !values.ContainsKey(Kind)))
        {
            throw Invalid();
        }

        var Code = stencil.Template.ToArray();
        foreach (var Patch in stencil.Patches)
        {
            Code[checked((int)Patch.Offset)] = values[Patch.Kind];
        }

        return Code.ToImmutableArray();
    }

    private static Nativeˉstencil Load(Nativeˉstencilˉcontract contract)
    {
        using var Stream = typeof(X64ˉnativeˉstencil).Assembly
            .GetManifestResourceStream(contract.Resource) ?? throw Invalid();
        if (Stream.Length > Objectˉlimits.MAX_OBJECT_BYTES)
        {
            throw Invalid();
        }

        using var Buffer = new MemoryStream(checked((int)Stream.Length));
        Stream.CopyTo(Buffer);
        return Read(
            Objectˉcodec.Readˉandˉverify(
                Buffer.GetBuffer().AsSpan(0, checked((int)Buffer.Length))),
            contract);
    }

    private static InvalidOperationException Invalid() => new(
        "The WVA native stencil does not match the bounded native-service contract.");

    private sealed record Nativeˉstencilˉcontract(
        string Resource,
        uint Formatˉversion,
        string Patchˉsymbol,
        string Stencilˉsymbol,
        ImmutableArray<byte> Shell,
        ImmutableArray<Nativeˉstencilˉpatch> Patches);
}
