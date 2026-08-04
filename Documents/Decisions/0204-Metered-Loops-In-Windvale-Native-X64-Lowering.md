# Decision 0204: Metered loops in Windvale-native x86-64 lowering

- Date: 2026-08-04
- Status: Implemented locally; independent cross-host qualification pending
- Advances: Phase 10 native host tools and the [Decision 0057 native-retirement gate](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Extends: [Decision 0195](0195-Acyclic-Control-Flow-Windvale-Native-X64-Lowering.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0195 transferred typed basic blocks, comparisons, forward branches, and exact ABI-22 displacement construction into Windvale. Ordinary compiler-produced `while` control remained rejected only because the independent graph pass assumed source-order reachability and the emitter computed relative targets with forward-only unsigned subtraction.

ABI 22 already supplies the required resource contract. Every selected WVB instruction begins with an instruction-budget charge and branches to the instruction-limit status before executing when the budget is exhausted. The jump that closes a loop is charged on every iteration. A second loop meter or changed runtime ABI would duplicate semantics rather than transfer ownership.

## Decision

Admit instruction-aligned backward targets for the existing `jump` and `branch.false` operations. Preserve empty operand stacks at every control-flow edge, exact typed block analysis, the 1,024-instruction/code limits, and complete rejection of unreachable blocks.

Replace source-order reachability with a bounded fixed-point proof. Analyze every discovered block independently, then propagate entry reachability across verified fallthrough, jump, and branch edges for at most the number of blocks. Reject any leader not reached after that bound, including unreachable cycles.

Encode relative x86-64 displacements as explicit 32-bit bit patterns. Forward targets use checked subtraction. Backward targets compute the checked magnitude and its two's-complement representation without relying on host casts or unchecked arithmetic. Preserve the existing machine offset projection and ABI-22 instruction charges.

Extend the existing shared-backend differential case rather than adding a top-level test. Feed the already-retained native loop oracle through the Windvale memory adapter, hosted tool under the reference runtime, and the same tool compiled to native x86-64. Require exact Stage 0 WVO bytes, the retained 157-instruction success and 156-instruction exhaustion boundary, invalid-target rejection, unreachable-cycle rejection, and publish-only-after-success behavior.

## Consequences

- The Windvale-written selector now owns bounded single-function scalar loops as well as acyclic conditionals.
- The canonical loop remains exactly 1,665 code bytes and a 1,738-byte WVO with SHA-256 `470542f262ebb288c72b306cf73807f1922c9c1cf089ecfc8dbba6c810435fe8` and `1771bcb36ce897dab2184b28a93a93d3d1116e948997ee551920c94c2a52e9e6`.
- The core, memory-adapter, and hosted-tool WVB identities are respectively `d4df72b19fa1222cfffa32e87de798b5073c24b7b2037c3ed2711799e006303d`, `6b06e9c9ceb10ebecee11d1d6533d9586e99cac04d407c602bbc29821770f8ab`, and `05c379c6d09b4eadb3b7db68212db42c51e2762b36d5fc453b81998398c92e0d`.
- The hosted tool currently lowers through Stage 0 to 900,877 code bytes and a 903,093-byte WVO. These are current implementation measurements, not optimization promises.
- No ABI, WVB, WVO, runtime-status, or application-container format changes.
- Direct calls and multiple functions are now the next compiler-backend ownership boundary on the path from the Windvale-native source compiler to general executable production.

## Reconsideration triggers

- a future control-flow operation carries a nonempty typed edge stack and requires explicit merge evidence;
- direct calls require a Windvale-owned call graph, ABI argument, symbol, or relocation model;
- measured graph sizes justify storing verified block edges instead of bounded repeated analysis;
- ABI 22 changes its instruction-budget or branch contract; or
- independent Windows/Linux evidence changes any accepted bytes, status, or rejection result.
