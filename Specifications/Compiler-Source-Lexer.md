# Windvale bootstrap source lexer

## Status and purpose

`Compilerˉsourceˉlexer` is the first Windvale-written self-hosted compiler slice. It tokenizes the complete implemented Seed lexical surface over immutable UTF-8 bytes. The module is portable, capability-free, and depends only on `Foundationˉdecimalˉparsing`.

The original streaming implementation is cross-host qualified at `d91dbfb` under Decision 0025. Decision 0042's bounded dispatch and Decision 0055's validated-scan reuse are cross-host qualified, with the latter qualified at `1a4fca7`. It does not replace the Stage 0 compiler yet.

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
    Numericˉhigh: u32;
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
- `Const` is appended as value 55 and covers the exact keyword `const` without renumbering any retained token kind.
- `Break`, `Continue`, `Andˉand`, `Orˉor`, `Plusˉequals`, `Minusˉequals`, and `Starˉequals` are appended as values 56 through 62 and cover `break`, `continue`, `&&`, `||`, `+=`, `-=`, and `*=`.
- `As`, `Platform`, `Authority`, `Requires`, `Optional`, and `Version` are values 63 through 68.
- `Match`, `Case`, `Variant`, `Sequence`, `Builder`, `Freeze`, `Push`, `For`, and `In` are values 69 through 77.
- `Slash`, `Percent`, `Ampersand`, `Pipe`, `Caret`, `Tilde`, `Shiftˉleft`, and `Shiftˉright` are values 78 through 85 and cover `/`, `%`, `&`, `|`, `^`, `~`, `<<`, and `>>`.
- `I64` and `U64` are values 86 and 87 and cover the exact type keywords `i64` and `u64`.

An identifier begins with an ASCII letter or underscore. Later characters may also be ASCII digits or U+02C9. No other non-ASCII identifier character is accepted.

Keyword classification uses exact byte length and first ASCII byte to select only plausible candidates before full ordinal comparison. Ordinary identifier bytes classify ASCII start characters directly. The complete whitespace routine runs only for byte values that can begin an accepted ASCII or Unicode whitespace scalar. These are bounded dispatch choices, not lexical-contract changes.

`&&` and `||` are recognized before their valid single-character `&` and `|` prefixes. `<<`, `>>`, `<=`, and `>=` are likewise recognized before `<` and `>`. A `/` that begins `//` trivia remains a comment; otherwise it is the division token.

## Numeric and string rules

`Compilerˉnumericˉkind` distinguishes `None`, `I32`, `U8`, `U32`, `I64`, and `U64`. Unsuffixed decimal digits are `I32` and cannot exceed 2,147,483,647. The exact suffixes `u8`, `u32`, `i64`, and `u64` require a non-identifier boundary and enforce 255, 4,294,967,295, 9,223,372,036,854,775,807, and 18,446,744,073,709,551,615 respectively. Narrow values use `Numericˉvalue` with `Numericˉhigh` zero; wide values use the two fields as a little-endian low/high `u32` pair. Narrow digits are parsed by `Foundationˉu32ˉdecimalˉparse`; the lexer owns a bounded two-limb decimal accumulator for wide literals.

Strings accept the simple escapes `\"`, `\\`, `\n`, `\r`, and `\t`, plus `\u` followed by exactly four hexadecimal digits. Escaped UTF-16 high and low surrogates must be paired. Raw LF or CR terminates scanning with `Unterminatedˉstring`.

The token preserves the original quoted source span. Decoded string construction is intentionally outside this contract.

## Entry points

```text
Compilerˉlexˉnext(Input, Start, Startˉline, Startˉcolumn) -> Compilerˉsourceˉtoken
Compilerˉlexˉnextˉvalidated(Input, Start, Startˉline, Startˉcolumn) -> Compilerˉsourceˉtoken
Compilerˉlexˉnextˉafterˉscan(Input, Start, Startˉline, Startˉcolumn) -> Compilerˉsourceˉtoken
Compilerˉlexˉsourceˉbounded(Input, Maximumˉtokens) -> Compilerˉsourceˉscan
Compilerˉlexˉsource(Input) -> Compilerˉsourceˉscan
Compilerˉlexˉtokenˉat(Input, Wanted) -> Compilerˉsourceˉtoken
```

`Compilerˉlexˉnext` is the safe standalone entry and validates strict UTF-8. `Compilerˉlexˉnextˉvalidated` is for a caller that already validated the complete byte value; it still validates cursor shape. `Compilerˉlexˉnextˉafterˉscan` is the narrower compiler-internal boundary for a cursor returned by an accepted token or validated parser/symbol record. It does not repeat cursor-shape checks, so callers must hold both complete-scan and cursor-provenance evidence. The checked whitespace and identifier-part helpers similarly retain their original signatures while internal `afterˉscan` variants accept the already known total byte length.

`Compilerˉsourceˉscan` reports final status, accepted token count, failure coordinates, and end cursor. It never stores a token sequence. `Compilerˉlexˉtokenˉat` exists for tests and inspection and is not the parser iteration contract.

## Current candidate implementation

`Compiler/Windvale/Source-Lexer-Core.wv` composes to a 49,470-byte WVB 1.11 module with SHA-256 `411c7d9679fc53a600c15d2d132b4ac62aa410e45a67f63f76e08efb89da6b3e`. `Examples/Compiler/Source-Lexer-Demo.wv` composes to a 56,674-byte module with SHA-256 `f83ff53dd2ffa1808bbf5c9ca2056f8dbb386308d52142f720ddf26420a6c2db` and returns `0` under the 10,000,000-instruction ceiling. These wide-scalar identities are local deterministic evidence. The Decision 0042 implementation passed exact Windows/Debian qualification at `5d67463`, the role-based path passed at `4fdc6bf`, and Decision 0055 was cross-host qualified at `1a4fca7`; those retained runs predate the new tokens.

Decision 0516 makes the native Project 1 front door the ordinary constructor
for both current WVBs and binds the core's exact portable type/export surface
through native inspection. The demo execution remains in the managed
differential lane because the current native runner does not produce its
required result; this is local Windows transfer evidence, not a new cross-host
qualification claim.
