# Decision 0202: Four-phase compiler-capacity WebAssembly verification

- Status: Accepted; implemented locally
- Date: 2026-08-04
- Scope: experimental WebAssembly compiler admission
- Retains: canonical verifier sources, execution ABI 3, profile 16, exact metering, import-free artifacts, and the active-development no-compatibility policy

## Context

The language-evolution candidate grows the hosted compiler to 859,555 WVB bytes with 397 functions, 707,044 aggregate code bytes, 146,737 decoded instructions, maximum 1,396 locals, and maximum operand stack 34. The capability-free memory adapter is 857,232 bytes with 395 functions, 705,421 aggregate code bytes, and 146,382 decoded instructions under the same local and stack maxima.

The retained compiler-capacity metadata/reference and control/reachability phases still admit both artifacts below execution ABI 3's 32-bit instruction ceiling. The former single typed-execution phase does not: it exhausts a 4,000,000,000-instruction run before completing either current compiler. Raising the external budget cannot solve that boundary because the ABI exposes its meter as an unsigned 32-bit value.

## Decision

- Keep the canonical complete semantic and executable verifier sources. Do not introduce a second verifier implementation or a host-language semantic oracle.
- Retain metadata/reference and control/reachability as independent phases.
- Derive two complementary typed-execution phases from the canonical `Hˉexecutable` source. The first validates function indices below 200; the second validates indices 200 and above. Both still parse the complete function table and enforce its declaration bounds. Together they validate every function exactly once through the expensive typed instruction walk.
- Run all four phases in fresh import-free execution-ABI-3 instances over identical candidate bytes. Admission succeeds only when every phase returns the one-byte success value.
- Pin each generated WVB and Wasm identity, its lowering cost, and the exact successful high-bit execution meter for both current compiler adapters.
- Keep the split an internal compiler-capacity specialization. It does not change WVB, the canonical complete verifier, WebAssembly profile 16, execution ABI 3, or the ordinary bounded verifier limits.
- During the current no-compatibility development window, replace the former three-phase local bundle directly. No legacy bundle mode, phase alias, or dual acceptance path is required.

## Exact local evidence

The semantic phase is 70,092 WVB bytes with SHA-256 `523e596426040a8870dfffc1fe8c1ba46779b2238a89bcdc58148d5da5c7d72b`; it lowers in 135,768,828 instructions to 440,583 Wasm bytes with SHA-256 `d61fe8f1091429d64ba425670ad4d85608f1efcc8bc71ad8904f3a7691ded677`. The first typed phase is 45,659 WVB bytes with SHA-256 `1abd13fcc2ca3695c5c6ba16949158e7ae7b79c586047f04aeaf6fa6d39ea79a`; it lowers in 86,249,017 instructions to 283,430 Wasm bytes with SHA-256 `a64084987d6890f4c2384ce2aaaa9e724f931cd61d39f3b460fbf9e35714e0ba`. The second typed phase is 45,667 WVB bytes with SHA-256 `3eb8fb93a44d8c2c1c60fb04e84948a1daea356de40b2915e920e49111b57c19`; it lowers in 86,249,017 instructions to 283,430 Wasm bytes with SHA-256 `225c489f3db95414480889f8fc725c87b71fed45d504c539c65a418466dd56f0`. The control phase remains 45,548 WVB bytes and lowers in 86,090,732 instructions to 282,718 Wasm bytes.

Under Node.js 24.18.0, the hosted compiler completes metadata/reference, typed first, typed second, and control/reachability in exactly 2,538,949,903, 1,498,394,475, 3,666,795,913, and 3,884,234,392 instructions. The portable compiler completes the same phases in exactly 2,537,643,002, 1,493,928,972, 3,651,136,554, and 3,883,165,187 instructions. All values are preserved as unsigned host observations and remain below `u32` maximum.

The focused Release Seed WebAssembly case independently reconstructs and validates all four artifacts and passes with a zero-warning build in 143.243 test seconds. The integrated standalone gate emits 34 Wasm modules, including the second typed partition and the independent reclamation workload, and passes every exact Node.js 24.18.0 engine case in 749 command seconds. The final change-aware gate passes the editor contract, a zero-warning Release build, and all 92 affected Seed tests in 873.920 suite seconds and 883.8 command seconds. This remains local development evidence and makes no new cross-host or browser claim.

## Consequences

- Compiler admission remains complete without widening a serialized format or execution ABI.
- The split point is deterministic and tested as part of artifact identity; moving it deliberately changes both typed artifacts and their exact meters.
- Each typed phase repeats bounded declaration-table parsing, trading a modest amount of work and bytes for a verifiable meter ceiling.
- Historical three-phase evidence remains valid for the exact older compiler artifacts named by Decisions 0170 and 0174. It is not a compatibility obligation for the current candidate.
- The native hosted verifier retains one monolithic typed walk under its `u64` meter; [Decision 0203](0203-Evolved-Compiler-Hosted-Tool-Capacity.md) owns that separate capacity contract.

## Reconsideration triggers

Revisit this decision if:

- either typed phase approaches the unsigned 32-bit instruction ceiling again;
- a verified shared meter wider than `u32` is introduced for execution ABI 3 or its successor;
- compiler partitioning becomes semantic rather than a verifier-only specialization;
- the canonical verifier changes so the fixed function-index partition no longer covers each expensive typed check exactly once; or
- a named release policy creates a compatibility obligation for compiler-admission bundles.
