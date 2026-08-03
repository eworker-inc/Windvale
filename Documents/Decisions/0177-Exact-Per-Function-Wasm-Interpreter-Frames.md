# Decision 0177: Exact per-function Wasm interpreter frames

- Date: 2026-08-03
- Status: Implemented with local Windows and Node.js evidence; cross-host and cross-browser qualification pending
- Extends: [Decision 0175](0175-Compiler-Scale-Wasm-Interpreter-Execution-Entry.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Decision 0175 admitted the exact portable compiler and entered guest execution, but assigned every function in one candidate the candidate's largest power-of-two frame. The compiler therefore used a 16,384-byte local frame even when the active function declared far fewer values. Every immutable `local.store`, call, and return copied that oversized representation into the enclosing 4 MiB monotonic Wasm value arena.

The existing direct WebAssembly selector is not a smaller compiler route at this boundary. It rejects nonempty static data and nominal declarations, supports only the primitive direct-value families, and requires a decreasing acyclic call graph. The exact portable compiler contains static data, records, enums, recursion, and calls that do not meet that ordering. Interpreter storage remains the coherent path until those direct-AOT gaps have a separately measured implementation.

## Decision

- Record two unsigned values per guest function during preflight: parameter count and exact frame length `(parameters + locals) * 8` bytes. Retain the existing 2,048-cell declaration ceiling.
- Build one bounded zero backing large enough for the candidate's largest declared function, but slice each active local frame to that function's exact declared length. Zero-local functions therefore own an empty local byte sequence rather than the former 128-cell minimum.
- Save only the caller return PC, exact caller locals, and caller function index. Recover the caller's frame length, code offset, and code length from verified preflight metadata on return instead of duplicating them in every saved frame.
- Keep locals, operand values, saved frames, heap values, and records immutable in this slice. The change reduces copied bytes but does not claim reclamation, aliasing, or a mutable Windvale storage primitive.
- Pin the generated-Wasm compiler boundary at the largest successful guest budget and the first failing budget. Require reference-runtime agreement through the largest successful budget; keep the Wasm arena failure target-specific rather than imposing that smaller resource ceiling on the reference runtime.

## Consequences

Small retained guests now pay for their declared frames rather than a 128-cell floor, and compiler functions pay for their own declarations rather than the compiler's 2,048-cell maximum. The exact compiler advances from one successfully dispatched guest instruction to 1,511 before the same enclosing `WVR3018` boundary. Budget 1,512 is the exact first outer allocation failure for the canonical `Function-Only.wv` source set.

This is progress toward reusable interpreter storage, not its completion. `local.store` still reconstructs an immutable frame, and the generated profile-16 runtime still allocates constructed byte values monotonically. Complete compilation therefore cannot yet publish `WVCO 1`. The next implementation boundary remains bounded reusable or reclaiming interpreter-owned storage with explicit reset and stale-reference rules.

The compact saved-frame representation is internal to the experimental interpreter. It does not change WVB 1.6, execution ABI 3, `WVXI`, `WVXO`, guest instruction charging, failure codes, or complete-verifier acceptance.

## Local evidence

The retained interpreter is 67,209 WVB bytes with SHA-256 `b27b36959f0c8ac045d3fa653e728a1aa261a858dc0e4d7af4ff2aec527bd8d9`. Its single outer function has 4,088 nonparameter locals, 62,948 code bytes, 13,769 instructions, and maximum stack three. The unchanged profile-16 backend lowers it in exactly 269,459,085 Windvale instructions to 412,498 deterministic import-free Wasm bytes with SHA-256 `c35e3b5144cb8883667bb22462d54318561857691d6af26c7fc6e2bce959d752`.

The exact 597,545-byte portable compiler receives canonical WVSS for `Function-Only.wv` at call depth 64. Budget one returns `WVXO 2` status `3011`, guest count one, and empty output after 44,171,762 outer instructions. The reference runtime and generated Wasm agree at budget 1,511 on the same versioned guest-budget status, count 1,511, empty output, and 49,727,640 outer instructions. In generated Wasm, budget 1,512 is the exact first enclosing `WVR3018`, consumes 49,727,825 outer instructions, and publishes no `WVXO`; the 20,000,000 budget reaches the same exact outer boundary. The reference runtime deliberately does not inherit the target's smaller 4 MiB allocation ceiling.

All retained scalar, text, bytes, UTF-8, formatting, SHA-256, record, enum, typed-default, bounded-failure, and byte-array entry cases preserve their guest status, guest instruction count, and result. Repeated function, mutated-default, and record-arena requests reproduce their exact first-run counts in fresh executions. The complete verifier still accepts the mutated default-local fixture in 523,386 instructions before interpretation.

The focused Seed WebAssembly test passes a zero-warning Release build and all exact reference-runtime, artifact, lowerer, and boundary assertions in 105.842 test seconds. The complete repository WebAssembly gate rebuilds all 34 retained generated Wasm artifacts, verifies all imports and ABIs, runs the three compiler-capacity phases, and passes the expanded interpreter cases under Node.js 24.18.0 on Windows in 446.1 seconds. Change-aware Windows verification then passes a zero-warning Release build and all 87 selected Seed tests in 472.695 suite seconds; its WebAssembly and golden cases take 106.091 and 230.761 seconds, and the complete command takes 491.5 seconds. This is local development evidence, not cross-host or cross-browser qualification.

## Rejected alternatives

Keeping one candidate-wide frame was rejected because a single large compiler function imposed its 16,384-byte width on all 326 functions.

Adding an unversioned mutable byte operation was rejected because WVB currently exposes immutable byte values. A new mutation or ownership capability needs explicit verifier, aliasing, bounds, lifetime, reset, and lowering contracts rather than an interpreter-only semantic shortcut.

Switching immediately to the existing direct-AOT selector was rejected because its current static-data, nominal-value, and call-graph limits exclude the measured compiler before code generation.

Increasing the outer arena was rejected again because it changes only the distance to failure while immutable whole-frame stores continue allocating in proportion to execution history.

## Reconsider when

- The interpreter can update local cells through a bounded reusable representation without changing observable Windvale value semantics.
- A versioned mutable-storage primitive has complete verifier, alias, lifetime, reset, malformed-input, and WebAssembly-lowering evidence.
- The direct selector gains measured support for compiler static data, nominal values, recursion, and general verified calls with a smaller complete path.
- Cross-host or browser engines disagree on the exact 1,511/1,512 boundary, reset behavior, or import-free artifact identity.
