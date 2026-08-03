# Decision 0195: Acyclic control flow in Windvale-native x86-64 lowering

- Date: 2026-08-03
- Status: Implemented; cross-host qualification pending
- Advances: Phase 10 native host tools and the [Decision 0057 native-retirement gate](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Extends: [Decision 0194](0194-Straight-Line-Windvale-Native-X64-Lowering.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0194 transferred reusable straight-line scalar verification and selection into Windvale, but ordinary compiler-produced `if` expressions remained dependent on the C# backend. The existing ABI-22 Stage 0 backend already defines the required portable handoff: dense basic blocks, empty operand stacks at block boundaries, typed physical value-slot reuse, explicit forward terminators, exact WVB instruction charging, and fixed x86-64 encodings.

Moving only branch byte templates would not transfer that ownership. The Windvale lowerer must independently discover and validate the control-flow graph, establish typed stack evidence for every block, project the same deterministic frame, and resolve target displacements from computed block offsets before it can publish a WVO.

Backward edges remain a separate boundary. Admitting them requires the native execution-budget or safe-point contract used by the complete backend; accepting loops without that contract would make resource enforcement depend on the caller.

## Decision

Extend the one-function portable subset with `bool` locals and values, `bool.const`, all six signed `i32` comparisons, boolean equality/inequality/negation, `jump`, `branch.false`, and multiple returns. Admit only forward instruction-aligned targets and require an empty operand stack on every inter-block edge.

Perform a bounded instruction-boundary and leader pass before block analysis. Reject unknown or truncated instructions, invalid boolean constants, out-of-range or backward targets, targets that are not instruction boundaries, unreachable blocks, type mismatches, nonempty edge stacks, incomplete exits, inconsistent declared maximum stack, and frames exceeding 2,048 ABI cells.

Allocate physical value cells independently by type. Reuse the maximum `i32` and `bool` block-local counts across blocks in canonical order, matching Stage 0 without introducing a second machine IR. Compute every block's native offset from the verified instruction sizes, then emit the existing ABI-22 comparison, unconditional-jump, and conditional-branch encodings with exact relative displacements.

Extend the existing shared-backend conformance case instead of adding another top-level test. Require exact WVO equality for the retained nested-control oracle through the memory adapter, hosted tool under the reference runtime, and the same hosted tool compiled to native x86-64. Retain malformed-input checks and add a backward-target rejection case.

## Consequences

- Windvale now independently owns the bounded acyclic `Main() -> i32` control-flow path from WVB verification through canonical ABI-22 WVO emission.
- The retained control oracle covers both scalar types, all supported comparisons, boolean negation, jumps, branches, nested early returns, and typed value-cell reuse. It remains exactly 4,835 code bytes and a 4,908-byte WVO, with the pre-existing Stage 0 hashes unchanged.
- Constant and arithmetic artifacts remain byte-identical to their prior Stage 0 oracles.
- The core, memory-adapter, and hosted-tool WVB identities are respectively `3ba83ebe5d848273b42f9a4fed3aa110b7396ba54796b03bf43324c5077c584b`, `8c01614988c24d17070b9bbb55276fd9d35a34a917d4fcfb70bc77dc16532a4f`, and `f6d44af0a3ea8d4dd5c8c3367358d970fb9bd0b2252305d8058bec791a041a57`.
- The hosted tool currently lowers through Stage 0 to 893,997 code bytes and an 896,179-byte WVO. These are current implementation measurements, not optimization promises.
- Stage 0 remains the complete backend and recovery oracle for loops, calls, multi-function modules, data and capability operations, relocations, fragment verification, linker/package integration, and native publication.

## Reconsideration triggers

- the shared execution-budget contract can be reproduced for backward edges and loops;
- direct calls require a Windvale-owned call graph, ABI argument, symbol, or relocation model;
- a Windvale-native machine IR becomes a clearer verified handoff than direct WVB decoding;
- ABI 22 changes; or
- independent Windows/Linux evidence changes any accepted bytes or rejection result.
