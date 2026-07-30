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
    public const string BYTES_READ_I32_LITTLE = "Bytesˉreadˉi32ˉlittle";
    public const string I32_FORMAT = "I32ˉformat";
    public const string U8_FORMAT = "U8ˉformat";
    public const string U32_FORMAT = "U32ˉformat";
    public const string U32_FROM_U8 = "U32ˉfromˉu8";
    public const string TEXT_CONCAT = "Textˉconcat";
    public const string TEXT_UTF8_IS_VALID = "Textˉutf8ˉisˉvalid";
    public const string TEXT_FROM_UTF8 = "Textˉfromˉutf8";
    public const string TEXT_QUOTE = "Textˉquote";
    public const string ENUM_NAME = "Enumˉname";

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
            new(
                BYTES_READ_I32_LITTLE,
                [Valueˉtype.Bytes, Valueˉtype.U32],
                Valueˉtype.I32,
                Wirˉoperation.Bytesˉreadˉi32ˉlittle),
            new(I32_FORMAT, [Valueˉtype.I32], Valueˉtype.Text, Wirˉoperation.I32ˉformat),
            new(U8_FORMAT, [Valueˉtype.U8], Valueˉtype.Text, Wirˉoperation.U8ˉformat),
            new(U32_FORMAT, [Valueˉtype.U32], Valueˉtype.Text, Wirˉoperation.U32ˉformat),
            new(U32_FROM_U8, [Valueˉtype.U8], Valueˉtype.U32, Wirˉoperation.U32ˉfromˉu8),
            new(
                TEXT_CONCAT,
                [Valueˉtype.Text, Valueˉtype.Text],
                Valueˉtype.Text,
                Wirˉoperation.Textˉconcat),
            new(
                TEXT_UTF8_IS_VALID,
                [Valueˉtype.Bytes],
                Valueˉtype.Bool,
                Wirˉoperation.Textˉutf8ˉisˉvalid),
            new(
                TEXT_FROM_UTF8,
                [Valueˉtype.Bytes],
                Valueˉtype.Text,
                Wirˉoperation.Textˉfromˉutf8),
            new(
                TEXT_QUOTE,
                [Valueˉtype.Text],
                Valueˉtype.Text,
                Wirˉoperation.Textˉquote),
        }.ToImmutableDictionary(Declaration => Declaration.Name, StringComparer.Ordinal);

    public static bool Tryˉget(string name, out Foundationˉintrinsicˉdeclaration declaration)
    {
        return DECLARATIONS.TryGetValue(name, out declaration!);
    }
}
