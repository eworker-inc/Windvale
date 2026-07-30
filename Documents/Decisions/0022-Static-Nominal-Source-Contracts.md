# Decision 0022: Static nominal source contracts

- Date: 2026-07-30
- Status: Accepted, implemented, and cross-host qualified at `6d2a351`

## Context

Decision 0019 qualified bounded static composition for portable dependency functions. That function-only surface was enough for predicate and comparison modules, but it cannot express a reusable structured result. The assembler and linker each consequently owned an equivalent decimal-parse record as well as equivalent unsigned parsing logic. A Windvale-written lexer will need the same result shape repeatedly.

Adding binary module linkage, type export tables, aliases, package discovery, or runtime identity would create unrelated bootstrap work. Seed already has nominal records and enums and already combines dependency functions into one verified WVB.

## Decision

Permit an imported portable source module to declare records and enums in addition to imports and exported functions. Static composition internalizes those declarations into the same ordinary WVB as the dependency functions.

- A dependency still cannot declare capabilities or data.
- Every dependency function must still be declared `export`; composition makes it internal in the resulting root WVB.
- All dependency record and enum declarations form part of that source module's visible static contract. They are not independently exported from the resulting WVB.
- Each dependency is semantically checked with only its own declarations and transitive imports. A type in a root or sibling module cannot satisfy an undeclared dependency.
- Included record, enum, and function declarations are ordered by ordinal module name before root declarations. Normal category-specific canonicalization and duplicate diagnostics then apply.
- All included nominal type names share the existing combined type namespace. There are no aliases or qualified type names in this slice.
- WVB remains version 1.6. There is no runtime module loader, binary type import, or cross-WVB nominal identity.

## Consequences

Foundation APIs can return explicit immutable results without sentinel values, traps, or duplicate caller-local record declarations. This is enough for bounded parsers and future lexer helpers while retaining the smallest compile-time model.

Making every nominal declaration visible means a dependency cannot yet keep private implementation types. That is accepted for the small Foundation surface. Private dependency functions, private nominal types, imported data, aliases, and separately compiled libraries require measured pressure and a separate decision.

## Verification gate

The transitive composition fixture must pass a dependency-declared record containing a dependency-declared enum through two imported function layers, retain root-only WVB exports, return `42`, and be byte-identical when explicit dependency argument order changes. A dependency that refers to a nominal type in a sibling it did not import must fail during its isolated semantic check.

The exact committed candidate must pass the complete verifier on Windows and Debian. Cross-host reports and the composed fixture must match exactly. The shared decimal parser in Decision 0023 must prove the feature through real assembler and linker consumers rather than leaving it as unused language surface.

Candidate `6d2a351` satisfied this gate on Windows and Debian GNU/Linux 12 x64 with zero build warnings/errors and all 38 tests. Both hosts produced the same 714-byte nominal composition fixture at SHA-256 `0980b7178943be516cd9b6924f179d5977ca147e11bf105c5063ea078c645b60`; it returned `42`, retained only root `Main` as a WVB export, and was byte-identical under reversed explicit dependency arguments. The isolated sibling-type leak failed as required, and Decision 0023 supplied the real nominal consumer contract.
