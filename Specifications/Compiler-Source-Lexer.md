# Windvale bootstrap source lexer

## Status and purpose

`Compilerˉsourceˉlexer` is the first Windvale-written self-hosted compiler slice. It tokenizes the complete implemented Seed lexical surface over immutable UTF-8 bytes. The module is portable, capability-free, and depends only on `Foundationˉdecimalˉparsing`.

The original streaming implementation is cross-host qualified at `d91dbfb` under Decision 0025. Decision 0042's bounded-dispatch implementation is the current candidate. It does not replace the Stage 0 compiler yet.

## Limits and coordinates

- Input is one strict UTF-8 byte value, bounded by the 4,194,304-byte value limit.
- A whole-source scan accepts at most 262,144 non-end tokens.
- `Offset`, `Length`, `Nextˉoffset`, and `Failureˉoffset` are UTF-8 byte counts.
- Lines and columns are one-based.
- LF increments the line and resets the column to one. CR and other accepted whitespace advance the column.
- One-, two-, and three-byte Unicode scalars occupy one column. Four-byte scalars occupy two columns, matching the current Stage 0 UTF-16 position convention.

## Token contract

```text
record Compilerˉsourceˉtoken {
    Status: Compilerˉlexˉstatus;
    Kind: Compilerˉtokenˉkind;
    Offset: u32;
    Length: u32;
    Nextˉoffset: u32;
    Line: u32;
    Column: u32;
    Nextˉline: u32;
    Nextˉcolumn: u32;
    Numericˉkind: Compilerˉnumericˉkind;
    Numericˉvalue: u32;
    Failureˉoffset: u32;
    Failureˉline: u32;
    Failureˉcolumn: u32;
}
```

`Offset` begins after leading whitespace and `//` comments. `Length` covers only the token. `Nextˉ*` identifies the cursor immediately after it; trivia before the following token is skipped by the next call. Failure coordinates identify the accepted deterministic error location.

`Compilerˉlexˉstatus` contains `Valid`, `Sourceˉtooˉlarge`, `Invalidˉutf8`, `Invalidˉcursor`, `Invalidˉlimit`, `Unexpectedˉcharacter`, `Integerˉoutˉofˉrange`, `Unsupportedˉescape`, `Unterminatedˉstring`, `Shortˉunicodeˉescape`, `Invalidˉunicodeˉescape`, `Unpairedˉsurrogate`, and `Tooˉmanyˉtokens`.

## Token kinds

Token-kind values are frozen to the Stage 0 ordering:

- `End`, `Bad`, `Identifier`, `Integer`, and `String` are values 0 through 4.
- `Module` through `Length` are values 5 through 32 and cover `module profile portable hosted system import capability data record enum export fn let var if else while return true false i32 u8 u32 bool text bytes void length`.
- `Leftˉparenthesis` through `Greaterˉequals` are values 33 through 54 and cover `(`, `)`, `{`, `}`, `[`, `]`, `;`, `:`, `,`, `.`, `->`, `+`, `-`, `*`, `!`, `=`, `==`, `!=`, `<`, `<=`, `>`, and `>=`.

An identifier begins with an ASCII letter or underscore. Later characters may also be ASCII digits or U+02C9. No other non-ASCII identifier character is accepted.

Keyword classification uses exact byte length and first ASCII byte to select only plausible candidates before full ordinal comparison. Ordinary identifier bytes classify ASCII start characters directly. The complete whitespace routine runs only for byte values that can begin an accepted ASCII or Unicode whitespace scalar. These are bounded dispatch choices, not lexical-contract changes.

## Numeric and string rules

`Compilerˉnumericˉkind` distinguishes `None`, `I32`, `U8`, and `U32`. Unsuffixed decimal digits are `I32` and cannot exceed 2,147,483,647. The exact suffixes `u8` and `u32` require a non-identifier boundary and enforce 255 and 4,294,967,295 respectively. Digits are parsed by `Foundationˉu32ˉdecimalˉparse`.

Strings accept the simple escapes `\"`, `\\`, `\n`, `\r`, and `\t`, plus `\u` followed by exactly four hexadecimal digits. Escaped UTF-16 high and low surrogates must be paired. Raw LF or CR terminates scanning with `Unterminatedˉstring`.

The token preserves the original quoted source span. Decoded string construction is intentionally outside this contract.

## Entry points

```text
Compilerˉlexˉnext(Input, Start, Startˉline, Startˉcolumn) -> Compilerˉsourceˉtoken
Compilerˉlexˉnextˉvalidated(Input, Start, Startˉline, Startˉcolumn) -> Compilerˉsourceˉtoken
Compilerˉlexˉsourceˉbounded(Input, Maximumˉtokens) -> Compilerˉsourceˉscan
Compilerˉlexˉsource(Input) -> Compilerˉsourceˉscan
Compilerˉlexˉtokenˉat(Input, Wanted) -> Compilerˉsourceˉtoken
```

`Compilerˉlexˉnext` is the safe standalone entry and validates strict UTF-8. `Compilerˉlexˉnextˉvalidated` is for a caller that already validated the complete byte value; it still validates cursor shape. A parser should call the first entry once and advance with the second.

`Compilerˉsourceˉscan` reports final status, accepted token count, failure coordinates, and end cursor. It never stores a token sequence. `Compilerˉlexˉtokenˉat` exists for tests and inspection and is not the parser iteration contract.

## Current candidate

`Compiler/Bootstrap/Source-Lexer-Core.wv` composes to a 36,741-byte WVB with SHA-256 `4d48af0c208e88d9e84d48c80324f35bed1985a799bd275b65b6a07f70111706`. `Examples/Compiler/Source-Lexer-Demo.wv` composes to a 43,250-byte WVB with SHA-256 `5422673a70ecf92f99f9a2db144f9b7a691d6281a98284dde6c6bc796ada60a4`, returns `0`, and executes 1,438,364 instructions under the 10,000,000-instruction ceiling. Exact Windows/Debian qualification is pending under Decision 0042.
