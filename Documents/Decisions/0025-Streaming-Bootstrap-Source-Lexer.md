# Decision 0025: Streaming bootstrap source lexer

- Date: 2026-07-30
- Status: Accepted, implemented, and cross-host qualified at `d91dbfb`

## Context

Phase 8 needs the first Windvale-written compiler component. Copying the C# lexer into a host wrapper would not advance self-hosting, while designing a general token collection, string builder, or diagnostic framework before the parser needs one would add another bootstrap loop.

The Stage 0 lexer already reveals a smaller boundary: a parser consumes one current token and advances. Windvale has immutable UTF-8 bytes, nominal records and enums, bounded module composition, and shared unsigned decimal parsing. Those facilities are sufficient for a portable scanner without adding language or runtime features.

## Decision

Create portable module `Compilerˉsourceˉlexer` and import only `Foundationˉdecimalˉparsing`.

The lexer returns one flat `Compilerˉsourceˉtoken` containing status, token kind, source byte span, next cursor, one-based line/column positions, numeric classification/value, and failure position. `Compilerˉlexˉnext` performs strict whole-input UTF-8 validation; a parser or bounded scanner validates once and then advances with `Compilerˉlexˉnextˉvalidated`. The latter rejects a cursor beyond the input, a zero line or column, and a cursor on a UTF-8 continuation byte.

`Compilerˉtokenˉkind` preserves the Stage 0 numeric identities for end, bad, identifier, integer, string, all 28 Seed keywords, and all 22 punctuation/operator forms. Source offsets and lengths are UTF-8 byte counts. Line and column values reproduce the current Stage 0 observable convention: LF starts a new line; other whitespace advances the column; non-BMP scalars occupy two UTF-16-compatible columns.

Identifiers remain deliberately narrow: ASCII letters or underscore start a name; ASCII letters, digits, underscore, and U+02C9 continue it. Integer tokens classify unsuffixed `i32`, `u8`, and `u32`, enforce suffix boundaries, and use the shared decimal parser before applying the type-specific maximum.

String tokens validate termination, the accepted simple escapes, four-hex-digit Unicode escapes, and surrogate pairing. This slice retains the original source span and does not construct a decoded string value. Decoding belongs at the parser/semantic consumer that proves it needs the value.

`Compilerˉlexˉsourceˉbounded` validates and counts a whole source without retaining tokens. It accepts at most 262,144 tokens and the existing immutable-byte ceiling bounds source input at 4 MiB. `Compilerˉlexˉtokenˉat` is a verification convenience that rescans; the future parser must keep and advance one token instead of using indexed rescans.

## Consequences

Windvale now implements its own complete Seed lexical contract as verified portable bytecode. The next parser slice can operate with constant token storage and does not need a general list merely to begin parsing.

The public bootstrap surface is larger than an eventual library surface because statically composed dependency functions currently cross an explicit export boundary. A later module/linkage improvement may narrow helper visibility without changing token behavior.

This decision does not implement a parser, decoded string values, token collection, source-file loading, recoverable multi-error diagnostics, syntax trees, semantics, WIR, or WVB emission. Collection and diagnostic facilities remain evidence-driven decisions for the first parser pressure.

## Verification gate

The fixed demo must cover all keywords and punctuation, U+02C9 identifiers, all integer classifications and range failures, strict UTF-8 rejection, Unicode whitespace and comment position tracking, valid and invalid escapes, surrogate pairing, invalid cursors, unexpected characters, token limits, and exact portable result `0`.

Qualification requires the exact committed candidate to pass the complete 40-test suite and native CLI verifier on Windows and Debian, equal normalized reports, and direct byte equality for `Source-Lexer-Core.wvb` and `Source-Lexer-Demo.wvb`. The candidate identities are respectively `0a9d5ff05afbe8598491ca636029fdfc7577dda754a048b93b0529d549019b04` and `32429c56b1b027fc440de14487ac0b5c628cec3c9bded1a98c1c21e6cbeed05a`.

Candidate `d91dbfb` satisfied this gate on Windows and Debian GNU/Linux 12 x64 with zero build warnings/errors. Both normalized reports matched, the two lexer artifacts were directly byte-identical, and the previously qualified assembler, linker, object, image, and map identities remained unchanged.
