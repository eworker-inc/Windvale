# Decision 0112: Metered WebAssembly control flow

- Date: 2026-08-02
- Status: Implemented with Windows engine, repository, and Chromium browser evidence; cross-host qualification pending
- Extends: [Decision 0106](0106-Bounded-Straight-I32-WebAssembly-Lowering.md), [Decision 0110](0110-Standalone-Dotnet-Free-WebAssembly-Artifact-Demo.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Profile 3 lowers genuine WVB instruction streams but rejects every branch. Its ABI-1 counter publishes a statically assigned instruction ordinal, which is exact only because each accepted instruction executes at most once. A loop requires both reconstruction of absolute WVB byte offsets into WebAssembly's structured control flow and a dynamic budget that cannot be bypassed by a backward edge.

The existing native and reference runtimes already define the relevant semantic boundary: charge before every WVB instruction; a budget of `N` permits exactly `N` charges; return `WVR3011` before instruction `N + 1`. The retained six-iteration loop is especially useful because it succeeds after exactly 157 instructions, fails at 156, and has a matching nonterminating compiler shape that can prove containment.

## Decision

- Add experimental profile 4 for one compiler-produced `while` region in one portable exported `Main() -> i32`.
- Admit `i32` and `bool` locals, the profile-3 scalar operations, signed `i32` comparisons, compiler no-op block jumps, one forward false edge, and one backward edge. Keep calls, multiple or nested regions, memory, imports, and capabilities outside the profile.
- Revalidate local and operand types, instruction widths, local indices, exact stack depth, every branch direction and target, the single structured region, empty-stack edges, and the final return before emission.
- Reconstruct the region as one WebAssembly `block` and `loop`. Lower the false edge to `br_if 1` and the back edge to `br 0`.
- Define execution ABI 2. Preserve the four export names, set `Windvale.abi` to `2`, and change `Windvale.run` to `(i32) -> i32`. A positive supplied limit is charged dynamically before every WVB instruction. Return status `3011` for `WVR3011` without an engine trap.
- Reset result and count before each run. Publish the result only on status zero, preserve checked `i32` overflow as status `3007`, and guarantee that the instruction global never exceeds the supplied limit.
- Extend the shared browser worker to ABI 2 while retaining ABI 0 and ABI 1 compatibility, the import-free and 64-KiB checks, fresh-worker lifecycle, and independent two-second wall-clock timeout.
- Replace the standalone .NET-free page's straight-line artifact with the qualified loop artifact. One button action runs fresh workers at budgets 157 and 156 and requires the exact success and exhaustion tuples.

## Consequences

The Windvale-authored backend now executes bounded source control flow directly in a browser engine. The instruction limit is a generated semantic boundary rather than only a worker timeout, so a selected nonterminating program returns deterministic Windvale status instead of relying on termination of the hosting worker.

ABI 2 is deliberately additive. Profiles 2 and 3 retain ABI 1 and their exact artifact bytes. Profile 1 retains ABI 0. The worker dispatches from the immutable ABI global and requires the same closed export set for ABI 1 and ABI 2.

Profile 4 is not a general control-flow lowerer. Its selector accepts one canonical compiler shape and rejects malformed targets, additional back edges, nonempty edge stacks, unsupported local types, multiple regions, and calls without output publication. Canonical WVB remains the portable identity, and the Stage 0 compiler and verifier remain required for artifact production.

## Initial evidence

The terminating source compiles to 341-byte WVB SHA-256 `99bf8d36c8ba8ab63c092143ebec1d79cd333c88df8b4da4ec67fd3772802af6`. Its generated 972-byte Wasm SHA-256 is `1c429ca20faa42b5018ea565ad10f148792dfbf6a8ecd438cf990cd60d664afe`. Node.js validates and instantiates it without imports; budget 157 returns status/result/count `0/42/157`, budget 156 returns `3011/0/156`, and a repeated budget-157 run resets and reproduces `0/42/157`.

The nonterminating source compiles to 292-byte WVB SHA-256 `68b7043535b9ed33db4e3bde9ec7b3e21f5ef977bd325078f61a1c631758bfab`. Its generated 663-byte Wasm SHA-256 is `325b6f8c9f8d7e2557f93c412aa85b913295dc4bfda5fbb32fb2337915109fde`; budget 50 returns `3011/0/50`.

The focused Seed conformance case independently reconstructs the ABI-2 module structure and every emitted meter, scalar operation, block, loop, and branch from verified WVB. It compares reference-runtime success and exhaustion, requires deterministic repeats, and corrupts both the backward and exit targets to prove rejection without publication. `Tools/Verify/Verify-WebAssembly.ps1` executes all eight retained ABI-1/ABI-2 artifacts under Node.js 24.18.0. `npm run verify:wasm-demo` independently reconstructs the deployed base64 bytes and executes both loop budgets.

The local Chromium-based in-app browser loads only the stylesheet, logo, analytics script, application, artifact data, shared host, and shared worker. One button action reports the exact SHA-256 and ABI-2 tuples `0/42/157` and `3011/0/156`, records zero .NET/Blazor resource requests, and produces no warning or error. This is one browser-engine family, not cross-browser qualification.

These are local Windows implementation results until the same source state passes the Linux repository gate.

## Rejected alternatives

A JavaScript-side loop counter was rejected because it would meter host calls rather than WVB instructions and could not interrupt an import-free backward edge.

Relying only on the two-second worker timeout was rejected because it supplies recovery but not deterministic `WVR3011` semantics or an exact portable count.

A general program-counter dispatcher was deferred because the first bounded compiler shape maps directly to WebAssembly structured control and can be independently verified with a much smaller contract.

Keeping ABI 1 while adding a hidden fixed budget was rejected because the caller must select and test the semantic resource boundary explicitly.

## Reconsider when

- Multiple or nested regions can be reconstructed without weakening edge-stack validation.
- Calls require a shared budget across more than one generated WebAssembly function.
- Representative programs make inline metering size or engine cost unacceptable.
- Cross-browser evidence exposes a module-worker, WebAssembly, or ABI portability problem.
- A Windvale-native verifier or browser compiler can replace another Stage 0 component.
