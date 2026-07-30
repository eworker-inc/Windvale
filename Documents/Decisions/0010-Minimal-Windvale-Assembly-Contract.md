# Decision 0010: Minimal Windvale assembly contract

- Date: 2026-07-29
- Status: Accepted; Stage 0 implementation cross-host qualified at `3bfc6bb`

## Context

WVO 1.0 can preserve canonical sections, symbols, and relocations, but the project has no textual way to construct a variable object. Jumping directly to Intel or AT&T syntax would import a much larger grammar and implicit ABI expectations. Building the first parser only in Windvale would make it difficult to distinguish grammar defects from current language-library limitations.

The next boundary must reveal the real needs of a Windvale-written assembler while preserving one object model and a reproducible replacement path for temporary C# code.

## Decision

- Define the versioned line-oriented WVA 1 contract in `Specifications/Windvale-Assembly.md`.
- Use explicit canonical symbol declarations and named definition blocks so offsets and sizes are inferred without requiring a general collection library or hiding object metadata from the source.
- Begin with `nop`, `return`, `trap`, relative `call`/`jump`, and 32-bit immediate register moves plus bounded numeric/data directives, zero-fill, and `address_u32`.
- Keep calling convention and ABI semantics out of WVA 1; instruction bytes and relocation meanings are exact, but a syntactically valid call is not yet a portable foreign-call promise.
- Implement a dependency-free C# Stage 0 parser/encoder as the oracle, with stable diagnostics and mandatory WVO verification.
- Qualify deterministic Stage 0 output on Windows and Debian before treating the grammar as the target for a Windvale-written implementation.
- Complete Phase 6 only after a verified Windvale module consumes WVA 1 and emits the same objects, and a separate linker owns resolution, layout, relocation, and final output.

## Consequences

- The first useful assembler grammar stays small enough to implement and fuzz while exercising both WVO relocation kinds and all section representations.
- Canonical declarations place some responsibility on source producers. This avoids premature dynamic collections and sorting in the bootstrap assembler; a future ergonomic source layer may remove that requirement.
- The C# assembler is an explicit recovery/oracle dependency, not a second permanent assembly language or object model.
- Named definitions provide function/data sizes but do not yet provide internal labels. Conditional control flow will require a deliberate label and relocation extension.

## Reconsider when

- The Windvale implementation demonstrates that canonical declarations cost more complexity than a bounded symbol collection.
- The linker or native backend needs an internal-label relocation, 64-bit address materialization, RIP-relative data access, or an ABI-specific instruction sequence.
- Real assembly sources exceed the line, statement, or immutable-construction bounds.
