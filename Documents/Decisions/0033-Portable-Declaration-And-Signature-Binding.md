# Decision 0033: Portable declaration and signature binding

- Date: 2026-07-30
- Status: Accepted and implemented; exact cross-host qualification pending

## Context

The qualified source graph gives portable Windvale code a complete, acyclic WVSS closure, but later semantic phases still need declaration namespaces, nominal identities, import visibility, and validated record, enum, and function signatures. Repeating graph walks and reparsing every candidate declaration for every lookup appeared attractive because the sources are bounded and immutable.

That first implementation was not practical. Validating the real compiler closure exhausted a 4,000,000,000-instruction execution limit after 248.1 seconds. The failure was algorithmic evidence: bounded work can still be too expensive when a semantic phase multiplies source and graph rescans.

## Decision

Introduce `Compilerˉsourceˉsymbols` as the portable declaration and signature phase above `Compilerˉsourceˉgraph`. It validates the graph first, applies global declaration bounds, enforces the Stage 0 namespace and capability rules, binds named signature types through transitive import visibility, and returns stable failure evidence.

The phase constructs two compiler-owned immutable byte values:

- `WVSD 1`, a packed declaration directory containing every non-import declaration in canonical WVSS module/source order.
- A row-major `Modules * Modules` visibility matrix containing the reflexive transitive import closure.

`WVSD 1` has a 16-byte header followed by fixed 24-byte entries. Each entry records module index, declaration kind, declaration offset, name offset, name length, and item count as little-endian `u32` values. The directory excludes imports and owns no source copies, paths, host handles, or semantic objects. Before semantic lookup, an independent streaming validator compares the complete directory back to the accepted immutable sources and rejects any byte, count, order, or trailing-data mismatch.

Visibility is built once from direct imports and closed deterministically in WVSS index order. Type lookup then scans the validated directory and performs a constant-time matrix visibility check instead of traversing the graph for each signature occurrence. Nominal bytecode identity is canonical: record indices are ordinal by name, followed by enum indices ordinal by name.

The semantic rules in this slice are:

- Capability, data, nominal type, and function names each have their defined global namespace; record constructors also conflict with function names.
- Foundation intrinsic names are reserved from record constructors and functions.
- Capabilities must be known and may not appear in a portable-profile module.
- Records and enums must be nonempty. Record fields, enum member names, enum values, and function parameter names must be unique within their owner.
- Named signature types must resolve to a nominal declaration in the same module or a transitive import.
- Record fields may use primitive types or enums, but Seed continues to reject nested record fields.

Body names, locals, calls, expressions, control-flow semantics, typed WIR, and WVB production remain later phases.

## Consequences

The implementation replaces the demonstrated repeated-rescan path; it does not retain a slow fallback. The real eight-module, 283,765-byte compiler closure now completes within the qualified runtime envelope and returns 24 records, 14 enums, 131 functions, 289 fields, 181 enum members, and 582 parameters.

The directory and visibility matrix are deliberately narrow compiler data structures, not a general collection library. WVSD is an internal development contract and may be revised with the compiler while public compatibility is not yet promised. Any replacement must preserve independent validation, deterministic identities, stable failure evidence, and measured closure performance.

## Verification gate

The exact candidate must pass the complete conformance and native CLI verifiers on Windows and Debian. Portable coverage includes valid cross-module signatures, transitive visibility, namespace conflicts, capability policy, reserved names, empty and duplicate record/enum members, unknown and inaccessible types, nested-record refusal, duplicate parameters, graph failure propagation, canonical nominal indices, and independent rejection of malformed WVSD values.

Both hosts must produce identical symbol core, demo, and tool WVB files. The hosted tool must report the exact real compiler closure counts, normalized conformance reports must match, and all directly compared upstream and downstream artifacts must retain their identities.
