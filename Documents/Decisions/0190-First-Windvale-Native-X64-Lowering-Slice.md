# Decision 0190: First Windvale-native x86-64 lowering slice

- Date: 2026-08-03
- Status: Implemented; cross-host qualification pending
- Advances: Phase 10 native host tools and the [Decision 0057 native-retirement gate](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Extends: [Decision 0180](0180-Compiler-Runtime-And-Native-Toolchain-Boundaries.md) and [Decision 0059](0059-First-Shared-Native-Wvb-Slice.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

The Windvale compiler, verifier applications, and project-aware build driver can now run as native Windows/Linux tools without loading .NET. Their source-to-WVB path is Windvale-owned, but Stage 0 still performs every WVB-to-x86-64 selection. Moving PE or ELF construction first would retain that managed blocker while making the normal pipeline look more independent than it is.

The complete ABI-22 backend is large. Its first historical constant slice has since accumulated the same execution-context, instruction-budget, call-depth, and trap contracts as every larger program. That current shape is a useful transfer boundary only if Windvale emits those exact 406 bytes and the standard WVO—not the obsolete six-byte prototype and not a descriptive plan.

## Decision

Add a portable Windvale module that independently parses and admits one canonical WVB 1.6 shape: capability-free portable `Main() -> i32` returning one `i32` constant through its compiler temporary. Reject every other profile, module shape, function shape, instruction sequence, malformed length, and extension.

Encode the current ABI-22 selector stencil as bounded signed 32-bit words around one dynamic little-endian source immediate. Include the shared prologue, four instruction charges, depth accounting, success epilogue, and canonical trap tails. Emit one exact WVO 1.0 `.text` object with exported `Main` and no relocations.

Expose both a capability-free memory adapter and a hosted file tool. The tool reads one WVB snapshot, lowers in memory, and calls `file.write_bytes` once only after success. Keep its ordinary WVB build manifests separate from any future fixed-authority PE/ELF package profile.

Extend the existing shared-native-backend conformance test instead of adding another top-level pipeline. Compare exact WVO bytes for constants 42 and 43 against the Stage 0 oracle, execute the Windvale tool through the native ABI, and require malformed-input output preservation.

## Consequences

- Windvale now owns one executable WVB-to-x86-64 machine-byte selection slice and its standard object serialization.
- The first transferred bytes preserve the current shared Windows/Linux ABI rather than introducing a second target or compatibility layer.
- A source immediate changes only its intended four-byte field; deterministic object identity remains structurally checked.
- The hosted shell can itself be lowered and executed natively, but a named standalone lowering-tool PE/ELF profile is not introduced by this decision.
- The exact core, memory-adapter, and hosted-tool WVB identities are respectively `654251d1aad3f8099bedb49193ec3a4a92ebeab99f0a7315c4fed780b4535620`, `e5c7472f9eca2a36fa7b63009fb01bdeb38c97229e5a8c7e880ea7c5800a8252`, and `a0e1894ce9ca79cb9181936f8d5f0ca0a114da3eb62a5c26c149720a1f707fe7`.
- Stage 0 remains the complete backend, fragment verifier, linker/package constructor, normal CLI integration, and recovery oracle.
- Arithmetic, control flow, calls, data, values, capabilities, relocations, and complete machine-IR ownership remain open.

## Reconsideration triggers

- the next typed operation family is small enough to transfer without embedding divergent compiler semantics;
- a Windvale-owned machine-IR serialization becomes the better handoff than direct WVB admission;
- ABI 22 changes and requires a new pinned stencil/version;
- a fixed-authority native lowering application profile is needed for normal builds; or
- dual-host evidence changes the emitted object or rejection behavior.
