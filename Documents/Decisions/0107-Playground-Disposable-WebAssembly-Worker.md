# Decision 0107: Playground disposable WebAssembly worker

- Status: Cross-host qualified engine integration with local Chromium worker execution; cross-browser qualification pending
- Date: 2026-08-02
- Extends: [Decision 0106](0106-Bounded-Straight-I32-WebAssembly-Lowering.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Decision 0106 qualified deterministic Windows/Linux construction of a bounded, portable, Windvale-authored WVB-to-WebAssembly backend. The generated ABI-1 modules already agreed with the reference runtime under an independent Node.js engine, but the public playground still executed only its .NET-hosted reference path. Browser execution and a wall-clock containment boundary therefore remained unproven.

The backend is deliberately partial. Treating every unsupported portable program as a failure would regress the usable playground, while replacing the reference interpreter before browser differential evidence exists would discard the semantic oracle.

## Decision

The playground embeds `Compiler/Windvale/WebAssembly-Core.wv` and `Examples/Compiler/WebAssembly-Tool.wv` as source resources. Its reusable engine compiles that composition once with Stage 0, verifies the result as canonical WVB, and requires SHA-256 `b47a6f5b89ac0d58dc6cafd6489b1fb12f1a0b9b161c09e8d2ca5a438993076a` before use. The backend itself remains the emitter; C# supplies bootstrap compilation, the verified reference runtime, bounded in-memory file resources, and result packaging.

After a capability-free portable program completes through the reference interpreter, the engine runs the Windvale backend with a 100,000,000-instruction ceiling and a 65,536-byte Wasm output ceiling. Selector rejection is recorded as `Unsupported` with no output and leaves the reference result intact. Invalid verified input, backend drift, unexpected publication, or runtime failure is an integration failure.

Every successfully generated module is transferred to a newly constructed module Web Worker. The worker:

- rejects empty or greater-than-64-KiB input;
- requires `WebAssembly.validate` and successful compilation;
- rejects every import;
- requires the exact execution ABI 0 or ABI 1 export set;
- executes once and returns only ABI, status, result, and instruction-count evidence; and
- is terminated on completion, worker failure, or a two-second wall-clock timeout.

The UI compares ABI-0 results or ABI-1 status/result/instruction counts with the reference execution and displays the canonical WVB, generated Wasm, and backend WVB identities. A new `WebAssembly worker` example is identical to the retained straight-line fixture. It compiles to 301-byte WVB SHA-256 `f7d360cf4d717d2cce93eda4f2c814960c39f1dd04bd0f74c44f55066730d655`, lowers to 432-byte Wasm SHA-256 `15f2d58746ff2b0ae33a0de05e2781949c9d908fab46dd4072bfe3b2fa42b0bb`, and reports status `0`, result `42`, and 30 attempted instructions on both paths.

## Consequences

Windvale source now owns the code emission used by a real browser execution path. Canonical WVB remains the handoff and distribution identity, and the .NET reference interpreter remains both the general fallback and the differential oracle.

This is a worker boundary for generated Wasm only. Stage 0 source compilation, bytecode verification, the Windvale backend running as WVB, and reference execution remain on the browser UI thread. The accepted direct profile has no imports or linear memory, so this decision defines no browser capability ABI. One successful Chromium-based in-app browser run on 2026-08-02 establishes local integration evidence; it does not establish cross-browser compatibility or production isolation.

## Evidence

Exact implementation commit `f3b96052b964832fb5fc60ed3d076b42e8b78e9d` passes GitHub [Verify run 30763774038](https://github.com/eworker-inc/Windvale/actions/runs/30763774038). Windows and digest-pinned Debian 12 each complete a zero-warning Release build, all 68 Seed tests, all 25 OS tests, and the complete native CLI qualification gate. The expanded playground case recompiles and digest-checks the embedded `.wv` backend, requires unsupported fallback without Wasm publication, reproduces the exact straight-line WVB and Wasm identities, and proves deterministic repeats on both hosts.

The Chromium-based local run executes the new example twice in fresh workers with equal ABI `1`, status `0`, result `42`, and 30-instruction evidence and no browser warning or error. GitHub [Deploy homepage run 30763774003](https://github.com/eworker-inc/Windvale/actions/runs/30763774003) also publishes the exact implementation commit successfully. Deployment success establishes static packaging, not browser-engine execution or cross-browser compatibility.

## Rejected alternatives

Replacing the reference result with direct Wasm was rejected because the bounded selector is intentionally incomplete and differential evidence is still valuable.

Moving the C# compiler and interpreter into the same worker in this change was rejected because it is a separate packaging and message-boundary hardening task, not a prerequisite for proving that Windvale-generated Wasm runs in a worker.

Writing the emitter in JavaScript or C# was rejected because the goal is to lower canonical WVB through the qualified portable `.wv` backend.

Keeping one long-lived execution worker was rejected because termination after every run gives a simple recovery boundary and avoids retaining generated module state across user programs.

## Reconsider when

- Structured control flow or multiple functions extend the accepted direct profile.
- Generated modules require imports, linear memory, asynchronous services, or browser capabilities.
- Backend compilation or lowering latency makes UI-thread execution unacceptable before the broader worker move.
- Cross-browser evidence exposes ABI, module-worker, transfer, or timeout differences.
- A Windvale-native verifier or interpreter can replace another Stage 0 component.
