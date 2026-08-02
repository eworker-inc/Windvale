# Decision 0116: Sequential WebAssembly control regions

- Date: 2026-08-02
- Status: Cross-host qualified; local Chromium browser evidence retained; cross-browser qualification pending
- Extends: [Decision 0113](0113-Metered-WebAssembly-Control-Flow.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Profile 4 proves one compiler-produced `while`, structured WebAssembly reconstruction, and exact dynamic instruction metering. It cannot accept a second loop or an ordinary conditional. The smallest useful next program shape needs reusable control regions without yet accepting arbitrary control-flow graphs, nesting, calls, or a dispatcher whose behavior would be harder to verify independently.

The current source compiler emits a stable canonical shape for `while`, `if`, and `if/else`: empty-stack block transitions, absolute instruction-boundary targets, a forward false edge, an optional then-to-join edge, and for loops one final back edge. This supplies a narrower contract than general reducible control flow and can be checked before any output is emitted.

## Decision

- Add experimental profile 5 for two or more sequential, nonnested compiler-produced regions in one portable exported `Main() -> i32`. Require at least one conditional region.
- Retain profile 4's `i32` and `bool` local types, scalar operations, checked arithmetic, comparisons, maximum stack depth two, size bounds, ABI 2, and per-WVB-instruction dynamic meter.
- Classify every `branch.false` as one canonical loop, `if`, or `if/else`. Revalidate exact instruction boundaries, empty edge stacks, loop entry and back edge, conditional false and join targets, no-op block transitions, and the final return.
- Reject nested, overlapping, crossing, self-directed, noncanonical, or malformed regions without publishing output.
- Emit each loop as `block` plus `loop`, a one-route conditional as `if`, and a two-route conditional as `if` plus `else`. Preserve profile-4 `br_if 1` and `br 0` loop lowering.
- Keep execution ABI 2 unchanged. Exact instruction counts are Windvale counts; target control instructions and validation work are not additional semantic charges.
- Advance the .NET-free static page to the retained two-loop/one-`if/else` artifact and require exact success at budget 184 and `WVR3011` exhaustion at 183.

## Consequences

The backend can now lower multiple useful control statements in one function without a JavaScript-side scheduler or WebAssembly program-counter dispatcher. Sequential loops and conditionals compose while retaining deterministic bytes, checked arithmetic, exact reset behavior, and a single caller-supplied resource boundary.

Profile 5 does not accept nested control flow. It also does not add calls, parameters, recursion, arrays, memory, text, capabilities, `break`, or `continue`. The Stage 0 compiler, mandatory WVB verifier, and hosted execution of the `.wv` lowerer remain part of artifact production.

## Qualification evidence

The retained mixed-control source compiles to 566-byte WVB SHA-256 `28eeed9d8f77f87f2c69399be05a1e6f3cb53b813ed949d7d2fde65a83dac50f`. Its deterministic 1,923-byte Wasm SHA-256 is `454e8af4f739ede63e0b2d55b8907f6075fec1495a4123df53ef5ebcf3ea2c4b`. The reference runtime and Node.js agree on `0/42/184` at budget 184 and `3011/0/183` at budget 183; a repeated success run resets and reproduces the same tuple.

The false-route fixture produces 544-byte WVB SHA-256 `37dcab42a4bdff5c4f89a2252b79880a1da65bf66d3251c1edfd2398f714ae49` and 1,770-byte Wasm SHA-256 `242116d69f8c28acf4886b1210ffd2b75e622ce92b44586a8a1668188930a84b`. Its actual `else` route agrees at exact budgets 331 and 330. The two-`if` fixture exercises a skipped first condition and taken second condition; it produces 399-byte WVB SHA-256 `061e1db0f14dd36d32235a44502b0b3accdd5c3cad529c3926a381a293884148` and 1,164-byte Wasm SHA-256 `d4fd2bf65a6b4aebf55aaf033e86984a4e882761a4c9a59d85bd7ca8353a21ba`, with exact budgets 41 and 40.

The independent C# decoder reconstructs every inline meter and selected WebAssembly block, loop, `if`, `else`, branch, scalar operation, and join from verified WVB. It also corrupts the `else` join and creates overlapping, crossing, and nested regions to require `Unsupportedˉcode` with zero output publication. The Node.js engine verifier retains every prior artifact identity and executes all three profile-5 modules. The standalone-page verifier reconstructs and executes the embedded mixed-control module without a .NET asset.

The local Chromium-based in-app browser loads the updated .NET-free page and executes fresh workers at both retained budgets. It reports the exact profile-5 SHA-256, ABI `2`, tuples `0/42/184` and `3011/0/183`, and zero .NET/Blazor resource requests. This is local evidence from one browser-engine family, not cross-browser qualification.

Exact implementation commit `87cb0a3c83441d34c8307243df5dee4ffb220417` passes GitHub [Verify run 30772366223](https://github.com/eworker-inc/Windvale/actions/runs/30772366223). Windows and digest-pinned Debian 12 each pass zero-warning Release builds, all 70 Seed tests, all 25 OS tests, and the complete native CLI qualification gate. The WebAssembly case takes 1.849 seconds on Windows and 1.305 seconds on Linux; the complete Seed suites take 226.744 and 199.612 seconds respectively. This qualifies the deterministic profile-5 backend, fixtures, and exact execution contracts across both hosts. GitHub [Deploy homepage run 30772366229](https://github.com/eworker-inc/Windvale/actions/runs/30772366229) independently reconstructs and executes the embedded artifact before publishing the standalone route successfully.

## Rejected alternatives

A general program-counter dispatcher remains deferred because it would add target-level dispatch work to every dynamic source instruction, expand modules, and replace directly structured control with a harder independent proof obligation.

Accepting nested regions in the same slice was rejected because their branch-depth calculation and edge-stack validation deserve dedicated malformed-input and differential evidence.

Treating compiler output as trusted was rejected because canonical WVB remains an untrusted serialized input at the backend boundary.

## Reconsider when

- Nested `if` and `while` regions can be reconstructed with explicit label-depth evidence.
- `break` or `continue` needs a named source and WVB contract.
- Function calls require one shared ABI-2 budget across generated target functions.
- Repeated region scanning becomes material for the bounded lowering workload.
- A Windvale-native verifier or browser compiler can replace another Stage 0 component.
