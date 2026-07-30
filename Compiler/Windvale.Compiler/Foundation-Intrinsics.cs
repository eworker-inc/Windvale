using System.Collections.Immutable;
using Windvale.Bytecode;

namespace Windvale.Compiler;

internal sealed record Foundationˉintrinsicˉdeclaration(
    string Name,
    ImmutableArray<Valueˉtype> Parameterˉtypes,
    Valueˉtype Returnˉtype,
    Wirˉoperation Operation);

internal static class Foundationˉintrinsics
{
    public const string BYTES_LENGTH = "Bytesˉlength";
    public const string BYTES_SLICE = "Bytesˉslice";
    public const string BYTES_READ_U8 = "Bytesˉreadˉu8";
    public const string BYTES_READ_U16_LITTLE = "Bytesˉreadˉu16ˉlittle";
    public const string BYTES_READ_U32_LITTLE = "Bytesˉreadˉu32ˉlittle";

    private static readonly ImmutableDictionary<string, Foundationˉintrinsicˉdeclaration> DECLARATIONS =
        new Foundationˉintrinsicˉdeclaration[]
        {
            new(BYTES_LENGTH, [Valueˉtype.Bytes], Valueˉtype.U32, Wirˉoperation.Bytesˉlength),
            new(
                BYTES_SLICE,
                [Valueˉtype.Bytes, Valueˉtype.U32, Valueˉtype.U32],
                Valueˉtype.Bytes,
                Wirˉoperation.Bytesˉslice),
            new(
                BYTES_READ_U8,
                [Valueˉtype.Bytes, Valueˉtype.U32],
                Valueˉtype.U8,
                Wirˉoperation.Bytesˉreadˉu8),
            new(
                BYTES_READ_U16_LITTLE,
                [Valueˉtype.Bytes, Valueˉtype.U32],
                Valueˉtype.U32,
                Wirˉoperation.Bytesˉreadˉu16ˉlittle),
            new(
                BYTES_READ_U32_LITTLE,
                [Valueˉtype.Bytes, Valueˉtype.U32],
                Valueˉtype.U32,
                Wirˉoperation.Bytesˉreadˉu32ˉlittle),
        }.ToImmutableDictionary(Declaration => Declaration.Name, StringComparer.Ordinal);

    public static bool Tryˉget(string name, out Foundationˉintrinsicˉdeclaration declaration)
    {
        return DECLARATIONS.TryGetValue(name, out declaration!);
    }
}
