# Decision 0003: Source naming and local mutation

- Date: 2026-07-29
- Status: Accepted

## Context

Windvale Seed began with ASCII identifiers, lowercase `main`, and mutable `let` locals. Windvale's bootstrap implementation already follows the [E-Worker](https://eworker.ca) convention of capitalized semantic names joined by U+02C9. Foundation work will introduce buffers, aggregate data, and larger libraries, so source naming and mutation should become explicit before those contracts expand.

## Decision

- Official Windvale source uses capitalized identifiers with U+02C9 between semantic words.
- Seed admits ASCII identifier segments joined by exactly U+02C9; visually similar separators are not accepted as aliases.
- Source casing is a project style rule rather than a grammar restriction on third-party code.
- Keywords and primitive type names remain lowercase.
- `let` declares an immutable initialized local, `var` declares a mutable initialized local, and parameters remain immutable.
- The executable source entry point is exported `Main() -> i32`.
- Capability IDs and other protocol namespaces remain ASCII-safe and independent of source naming.
- Bytecode declaration names are UTF-8 source metadata, not native ABI names. Native symbol mangling will be specified with the object model.

## Consequences

- Official Windvale examples visually align with the established E-Worker naming convention.
- Mutation is visible at the declaration site before the language gains mutable aggregates.
- The lexer, verifier, examples, golden modules, and cross-host conformance reports change together.
- Editors must make U+02C9 convenient to enter for official project development.
- Future C, assembly, object, and linker work cannot treat source spelling as an external ABI contract.

## Reconsider when

- U+02C9 input is impractical in supported editors even with project tooling.
- A broader Unicode identifier proposal provides precise normalization and confusable-character rules.
- Native symbol mangling cannot preserve deterministic, collision-free linkage for valid Windvale names.
