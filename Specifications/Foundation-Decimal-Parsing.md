# Foundation bounded decimal parsing

## Status and purpose

`Foundationˉdecimalˉparsing` is the portable owner of bounded ASCII-to-`u32` parsing for immutable byte spans. It supplies the shared structured result used by the Windvale-written assembler and linker and is intended to be reusable by the future self-hosted compiler lexer. The current implementation is cross-host qualified at `6d2a351`.

## Contract

The module declares:

```text
record Foundationˉu32ˉparse {
    Valid: bool;
    Value: u32;
}

export fn Foundationˉu32ˉdecimalˉparse(
    Input: bytes,
    Offset: u32,
    Length: u32
) -> Foundationˉu32ˉparse;
```

An accepted span:

- is wholly inside `Input`;
- has length 1 through 10;
- contains only ASCII bytes `0` through `9`; and
- represents a value from 0 through 4,294,967,295 inclusive.

Leading zeroes are accepted. Success returns `Valid = true` and the exact value. Every rejected case returns `Valid = false` and `Value = 0`.

The function is total over arbitrary `bytes`, `Offset`, and `Length` values. It validates the range before reading and uses a pre-multiply overflow check. It does not decode Unicode, accept signs or whitespace, recognize prefixes or suffixes, allocate a slice, emit diagnostics, or call a capability.

## Consumer boundary

`Assembler/Windvale/Wva-Assembler-Core.wv` parses each unsigned token through this contract. `Linker/Windvale/Wv-Linker-Core.wv` parses the base-address argument through the same contract. Each consumer retains grammar-specific signed parsing, token/range ownership, field-width validation, status codes, and diagnostics.

The record and function reach consumers through bounded static source composition. They are internalized into one ordinary WVB 1.11 module; this is not runtime library linkage.

## Qualification

`Examples/Foundation/Decimal-Parsing-Demo.wv` is the fixed portable boundary artifact. The complete assembler and linker suites are the consumer conformance tests. Qualification must preserve the 218-byte canonical assembled WVO, 24-byte linked image, 1,721-byte canonical map, no-write failures, and existing maximum image/map instruction ceilings.

The qualified standalone WVB is 1,697 bytes with SHA-256 `39f6c1c3d5a2233d5296e777e798450571c5f4ba837120a25a6487bf8014ee1f`. The qualified composed demo is 3,778 bytes with SHA-256 `16a20ee595eb708095f6e8c38c809a24774989110780dbefbacbc36ee468e695` and returns `0`. The exact `6d2a351` archive passed the complete Windows and Debian verifier; both artifacts and both composed tool consumers were byte-identical across hosts while all prior object, image, map, ceiling, and no-write contracts remained intact.
