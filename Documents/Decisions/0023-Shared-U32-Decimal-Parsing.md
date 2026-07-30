# Decision 0023: Shared bounded `u32` decimal parsing

- Date: 2026-07-30
- Status: Accepted and implemented; cross-host qualification pending

## Context

The qualified Windvale WVA assembler and linker both parse unsigned decimal fields from immutable byte spans. Each implementation performed the same ASCII digit validation and checked `u32` accumulation and returned an equivalent validity/value record. The future self-hosted lexer will also need deterministic integer parsing.

The duplicate logic is a measured three-consumer contract, but integer syntax policy remains owned by each language or format. A broad numeric library would prematurely mix signs, widths, bases, whitespace, suffixes, and diagnostics.

## Decision

Create portable module `Foundationˉdecimalˉparsing` with one nominal result and one exported function:

```text
record Foundationˉu32ˉparse {
    Valid: bool;
    Value: u32;
}

Foundationˉu32ˉdecimalˉparse(
    Input: bytes,
    Offset: u32,
    Length: u32
) -> Foundationˉu32ˉparse
```

The accepted span contains one through ten ASCII bytes `0` through `9`. Leading zeroes are accepted. The value must fit `u32`. The function returns `{ Valid: false, Value: 0 }` for an invalid offset, an out-of-range or empty span, more than ten digits, a nondigit, or overflow. It checks `Offset <= total` and `Length <= total - Offset`, avoiding overflowing range arithmetic.

The function is total for arbitrary immutable input and `u32` span values; malformed input is data, not a runtime trap. It performs no text decoding, allocation, host call, diagnostic emission, whitespace trimming, sign handling, suffix recognition, or base detection.

The WVA assembler and linker retain small local adapters because their token ownership and higher-level grammar differ. Signed `i32`, field-width checks, status selection, and source diagnostics remain with those consumers.

## Consequences

Unsigned decimal accumulation and its structured result now have one portable owner. The assembler and linker lose duplicate records and arithmetic, and the future compiler can reuse the same exact byte-span primitive.

The name is intentionally explicit rather than a generic `Parse`. New integer widths or syntaxes require demonstrated consumers and must not silently widen this contract.

## Verification gate

The standalone module and composed demo must cover zero, `u32` maximum, overflow, empty and over-ten-digit spans, nondigits, non-zero-offset subspans, leading zeroes, invalid offsets, and invalid ranges. Both real tools must compile with the module and preserve exact canonical WVO, image, map, reports, rejection behavior, and fixed instruction ceilings.

Qualification requires the exact committed archive to pass all 38 tests and the complete CLI verifier on Windows and Debian, equal normalized reports, and direct byte equality for the parser, demo, nominal composition fixture, assembler, linker, canonical object, linked image, and map.
