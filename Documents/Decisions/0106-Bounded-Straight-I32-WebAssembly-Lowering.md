# Decision 0106: Bounded straight-line i32 WebAssembly lowering

- Date: 2026-08-02
- Status: Cross-host qualified with independent Node.js evidence; locally integrated into the playground worker by [Decision 0107](0107-Playground-Disposable-WebAssembly-Worker.md)
- Extends: [Decision 0104](0104-WebAssembly-Checked-Addition-And-Execution-Contract.md)

## Context

The first two Windvale-authored WebAssembly profiles proved deterministic binary construction, a direct constant result, checked addition, and a host-visible status/result/instruction contract. Profile selection still depended on two exact compiler output templates, so it could not lower an ordinary sequence of locals and arithmetic or show that the same execution contract scales across multiple checked operations.

A general WVB backend would combine structured control-flow reconstruction, calls, other value families, memory, capabilities, and broader resource accounting. The next evidence step only needs to replace exact arithmetic templates with a bounded, statically validated instruction stream while keeping those larger contracts outside the experiment.

## Decision

- Add experimental profile 3 for one portable exported `Main() -> i32` with zero through 256 `i32` locals.
- Accept only straight-line `i32.const`, `local.load`, `local.store`, `i32.add`, `i32.subtract`, `i32.multiply`, `i32.negate`, `pop`, and one final `return`.
- Bound selected code to 16,384 bytes, 4,096 instructions, and declared maximum operand-stack depth 256. Retain the existing 65,536-byte output limit.
- Validate instruction widths, local indices, operand-stack transitions, the exact declared maximum stack, the final return position, and the final single `i32` before emission.
- Retain execution ABI 1. Charge each WVB instruction before attempting its generated operation, publish the result only on success, and return status `3007` for checked arithmetic overflow without an engine trap.
- Detect signed subtraction overflow from the wrapped result, multiplication overflow through a signed `i64` result comparison, and negation overflow by rejecting `i32` minimum.
- Preserve profiles 1 and 2 byte-for-byte. Exact template selection remains ahead of profile 3 so established artifact identities and the direct profile-1 `Main` export do not change.
- Keep branches, calls, other value types, linear memory, imports, and browser capabilities outside this revision.

## Consequences

The `.wv` backend now translates a genuine bounded WVB stack-machine sequence instead of recognizing only two complete code templates. Source programs can use compiler-synthesized locals, discarded expressions, and checked addition, subtraction, multiplication, and negation while producing standalone import-free WebAssembly.

The profile uses the selected WVB locals plus three `i32` scratch locals and one `i64` scratch local. This is target-private storage and does not alter the WVB function contract. Straight-line lowering avoids unresolved reconstruction rules for WVB branch targets and WebAssembly blocks.

Execution ABI 1 remains sufficient because the profile's 4,096-instruction limit is well within its `i32` counter. Output may expand substantially because instruction charging and checked arithmetic are emitted inline; the existing 65,536-byte limit therefore remains a meaningful publication boundary and is tested independently of the input limits.

This is still not a general backend or a replacement for the playground's .NET compiler and runtime. The encoder is Windvale-authored, but its current hosted shell runs as canonical WVB through the Stage 0 runtime.

## Evidence

The profile-3 success fixture WVB has SHA-256 `f7d360cf4d717d2cce93eda4f2c814960c39f1dd04bd0f74c44f55066730d655`. Its 432-byte WebAssembly output has SHA-256 `15f2d58746ff2b0ae33a0de05e2781949c9d908fab46dd4072bfe3b2fa42b0bb` and reports status `0`, result `42`, and 30 attempted instructions.

The checked subtraction, multiplication, and negation overflow fixtures produce 268-, 224-, and 307-byte modules with SHA-256 values `757d26c2cf404cabcf5b78d2c998bc7ddc78ec4531e4571630ae2c1b5c8d7925`, `e924c7507a363a7b019935622abfbd4bf4ac8445cd37a0412130ce8e5c83d51a`, and `3f098efd63c68d8c62a4f6b373507e12c21808ff01120d165c9dc85a047e99e2`. They report status `3007` after 10, 7, and 13 attempted instructions, respectively.

Node.js 24.18.0 validates, instantiates, and executes those four modules plus the two retained profile-2 addition modules. The engine results, statuses, and counts agree with the Stage 0 WVB reference runtime. Focused tests independently parse the emitted sections and instructions, simulate their checked arithmetic, compare deterministic repeats and exact digests, exercise non-overflow and both signed-extrema paths, lower `pop`, and reject malformed, unsupported, and output-limit cases without publication.

At this checkpoint, the Windvale core WVB has SHA-256 `18d8f2a32c7ee6ff0a89ac705663595dc611bf7ffd545f76662e1227085bbc34`; the hosted tool WVB has SHA-256 `b47a6f5b89ac0d58dc6cafd6489b1fb12f1a0b9b161c09e8d2ca5a438993076a`; and the portable encoder demo WVB has SHA-256 `cb6b5fbf378a4b13387704dda87beb75d6023112afeabfbaa558cf8fa32f5fe1`.

Exact implementation commit `a2285f5a0c09598ec701691bdbf0af9080e8cf0c` passes both host jobs in GitHub [Verify run 30762541741](https://github.com/eworker-inc/Windvale/actions/runs/30762541741). Windows and digest-pinned Debian 12 each pass a zero-warning Release build, all 68 Seed tests, all 25 OS tests, and the complete native CLI qualification gate. The WebAssembly conformance case recompiles the portable backend and all selected WVB inputs, requires the exact core/tool/demo and generated-Wasm digests, independently parses the emitted modules, and compares the execution tuples with the reference runtime on both hosts.

The run-level conclusion is `cancelled` because a later `main` documentation push activated the workflow's `cancel-in-progress` policy after the Debian job completed successfully at 19:09:53 UTC and the Windows job completed successfully at 19:11:07 UTC. Both exact-host job conclusions are `success`; no qualification step was cancelled or failed. The separate Node.js 24.18.0 engine run remains Windows evidence. Decision 0107 subsequently integrates the retained straight-line artifact into a disposable playground worker; cross-browser qualification remains pending.

## Rejected alternatives

Adding source branches in the same slice was rejected because WVB byte offsets must be reconstructed into valid structured WebAssembly control flow with explicit branch and instruction-accounting evidence.

Using wrapping WebAssembly arithmetic was rejected because it contradicts Windvale's checked `i32` semantics.

Pre-evaluating constant arithmetic in the selector was rejected because it would not preserve runtime instruction accounting or test generated checked execution.

Using JavaScript or C# as the profile-3 emitter was rejected because the experiment is specifically testing whether the backend can be owned in portable Windvale source. Both remain useful independent oracles.

## Reconsider when

- Structured branch reconstruction requires a different validated input model than canonical WVB offsets.
- Calls or multiple functions require execution ABI changes or a different code-layout strategy.
- Inline accounting and overflow checks create unacceptable artifact growth under representative programs.
- Cross-host or browser-worker evidence exposes an engine portability or containment issue.
