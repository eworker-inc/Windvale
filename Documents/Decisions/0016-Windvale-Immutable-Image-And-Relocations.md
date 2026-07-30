# Decision 0016: Windvale immutable image construction and relocations

- Date: 2026-07-30
- Status: Accepted and implemented; cross-host qualification pending

## Context

The qualified resolution/layout passes determine every contribution and defined-symbol address but publish no bytes. Windvale Linking 1 next requires exact materialized padding and zero-fill plus checked `absolute-u32` and `relative-i32` relocation arithmetic. Seed has immutable bytes and balanced persistent slice/concatenation but no public mutable buffer, `i64`, or general bitwise arithmetic.

Moving either image construction or signed relocation arithmetic into C# would make the Windvale linker a coordinator around a host linker rather than an implementation of the accepted contract.

## Decision

- Construct the unrelocated image in final section order using immutable concatenation: append explicit zero padding, exact section-data slices, and bounded zero-fill values.
- Build zero values from one immutable 4 KiB page and append pages plus a final slice, keeping construction bounded and compatible with persistent byte trees.
- Recompute source and target placements from deterministic object snapshots for each relocation.
- Resolve a relocation targeting a local or export within its object; resolve an import to the previously validated unique same-kind export.
- Express signed addends as explicit sign and `u32` magnitude derived from their exact little-endian bits. Check absolute results against `u32` and relative results against the complete `i32` range before encoding.
- Patch four bytes immutably as prefix slice plus encoded value plus suffix slice. Persistent byte balancing keeps each patch structural rather than copying the whole image.
- Emit the exact SHA-256 of the candidate image as development evidence, but do not invoke `file.write_bytes` until a separately structured complete-image verifier accepts the candidate.

## Consequences

- The Windvale module now constructs the same canonical candidate image as Stage 0 for aligned, unaligned, reordered, BSS, padding, local/export/import, absolute, and relative cases.
- `WVL1009` and `WVL1010` are decided in Windvale without integer truncation, wraparound, host arithmetic, or a new general `i64` language type.
- A digest is useful deterministic cross-host evidence but is not treated as proof of byte equality or as independent image verification. Publication remains intentionally unavailable.
- The current source repeats placement and symbol-address work. Independent verification and map generation may justify a bounded resolved-record collection, but only after measurements identify the exact shared shape.

## Reconsider when

- Native lowering requires relocation values outside the accepted 32-bit kinds.
- Measured valid relocation counts make repeated placement/resolution passes impractical.
- A language-level owned mutable buffer can be introduced with stronger aliasing and verification rules than a linker-specific escape hatch.
