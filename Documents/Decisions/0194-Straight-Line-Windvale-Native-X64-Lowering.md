# Decision 0194: Straight-line Windvale-native x86-64 lowering

- Date: 2026-08-03
- Status: Implemented; cross-host qualification pending
- Advances: Phase 10 native host tools and the [Decision 0057 native-retirement gate](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Extends: [Decision 0190](0190-First-Windvale-Native-X64-Lowering-Slice.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0190 proved that Windvale could admit one canonical WVB shape and emit the current ABI-22 object exactly, but its machine code was a pinned whole-program constant stencil. Adding more expression stencils would increase examples without transferring a reusable selector mechanism. The next meaningful boundary is the complete one-function, straight-line `i32` path emitted by the canonical source compiler.

That path already contains the core mechanics needed by wider lowering: WVB instruction decoding, typed operand-stack evidence, local-index and default-value semantics, deterministic native value numbering, frame projection, instruction charging, checked arithmetic, relative trap branches, and dynamic WVO section/symbol sizes. It remains bounded enough to verify independently without inventing a second machine IR or changing ABI 22.

## Decision

Replace the constant whole-program stencil with an algorithmic Windvale selector for one capability-free portable `Main() -> i32`. Admit only straight-line WVB 1.6 instructions for `i32` constants, local loads/stores, checked add/subtract/multiply/negate, and one final return.

Verify section and function envelopes, all-`i32` local metadata, instruction boundaries, local indices, stack effects, final return shape, declared maximum stack, instruction/code limits, and the 2,048-cell ABI frame ceiling before emitting bytes. Preserve WVB's zero-initialized locals through the ABI-22 frame prologue. Assign native value cells in canonical block order, matching Stage 0's single-block typed allocator.

Construct the ABI-22 prologue, zeroed frame, instruction charges, scalar operations, success return, status traps, and relative displacements from explicit bounded emitters. Construct WVO section and symbol sizes from the resulting code length. Preserve the existing memory and hosted adapters and their publish-only-after-success behavior.

Extend the existing shared-backend conformance case rather than adding parallel top-level tests. Compile the Windvale modules once, compare exact Stage 0 WVO bytes across a scalar corpus, execute the combined arithmetic object through the native hosted tool, and reject malformed stack and invalid-local inputs.

## Consequences

- Windvale now owns a reusable straight-line scalar decoder, verifier, frame projector, and ABI-22 machine-byte emitter rather than one constant template.
- Exact agreement includes the 406-byte constant fragment and the 1,871-byte combined checked-arithmetic fragment, including metering and trap displacements.
- The accepted limits are 1,024 locals, 1,024 instructions, 8,192 WVB code bytes, and 2,048 combined local/value cells; smaller structural limits fail closed before emission.
- The core, memory-adapter, and hosted-tool WVB identities are respectively `59c78a1eba86bd93084d815b3667f04b9297304dc29eac59db2b306c750a047d`, `eae679ce43b0c421de4768871cef83f32b482399f0a276d3952f10da6f63f914`, and `87f3b0c51fb4a2778539f4bb8e0533e96eab8a5a5d0378fa0ff76b609b4f5139`.
- The hosted tool currently lowers through Stage 0 to 492,477 code bytes and a 493,963-byte WVO. These are current implementation measurements, not optimization promises.
- Stage 0 remains the complete multi-function/control-flow backend, fragment verifier, linker/package constructor, normal CLI integration, and recovery oracle.

## Reconsideration triggers

- canonical conditional control flow can be admitted with explicit basic-block and stack-entry evidence;
- function calls require a shared Windvale-owned call graph, ABI argument, or relocation model;
- a Windvale-native machine IR becomes a clearer verified handoff than direct WVB decoding;
- ABI 22 changes; or
- independent Windows/Linux evidence changes any accepted bytes or rejection result.
