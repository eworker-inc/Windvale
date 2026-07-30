# Decision 0021: Shared ordinal byte-span ordering

- Date: 2026-07-30
- Status: Accepted and implemented; cross-host qualification pending

## Context

The Windvale WVO object core, WVA assembler, and linker each implemented the same unsigned lexicographic comparison over two validated spans of one immutable byte value. The comparison determines canonical section and symbol order, duplicate detection, and semantic lookup. A Windvale-written compiler will need the same primitive for source-token equality and deterministic name tables.

A first whole-value prototype required every caller to create two byte slices. The additional slice and call work pushed the already accepted maximum-symbol linker case beyond its fixed 200,000,000-instruction ceiling. Raising the ceiling would hide a Foundation abstraction cost at a security-relevant boundary.

## Decision

Create portable module `Foundationˉbyteˉordering` with exactly one exported function:

```text
Foundationˉbyteˉspansˉcompare(
    Input: bytes,
    Leftˉoffset: u32,
    Leftˉlength: u32,
    Rightˉoffset: u32,
    Rightˉlength: u32
) -> i32
```

Both spans must be inside the same immutable input value. The function compares bytes as unsigned values in index order, returns `-1` or `1` at the first difference, otherwise orders a proper prefix before the longer span, and returns `0` only for equal spans. Callers own span validation before invoking it; parser and object boundaries already establish those ranges.

The object core, assembler, and same-object linker paths call the shared function directly so their hot paths retain one comparison call. The linker's cross-object comparison remains local because it is a different two-value contract, and its complete-image equality check remains local because equality does not require ordering. No general collection, Unicode collation, locale behavior, case folding, allocation policy, or host service is added.

## Consequences

Canonical byte ordering now has one owner for three real tools and a future compiler use. The API is deliberately span-oriented: it avoids copying immutable data and expresses the representation the parsers already own.

The validity precondition is explicit. A future safe arbitrary-span API would need a structured result or a separately justified failure contract; it must not overload the ordering values with an invalid-range sentinel.

## Verification gate

The standalone module must expose only the declared function. Its portable demo covers empty/equal spans, first-byte differences, proper prefixes, subranges, and unsigned `127` versus `128` ordering. The object core, assembler, and linker must preserve their exact produced WVO, hosted reports, linked image, canonical map, failure publication rules, and the existing 200,000,000-instruction maximum cases.

Cross-host qualification requires the exact committed archive to pass all 37 tests and the complete native CLI verifier on Windows and Debian, equal normalized contracts, and direct byte comparison of the Foundation module, its demo, all three composed consumer WVB files, the canonical object, image, and map.
