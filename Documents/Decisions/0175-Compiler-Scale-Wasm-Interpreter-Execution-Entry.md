# Decision 0175: Compiler-scale Wasm interpreter execution entry

- Date: 2026-08-03
- Status: Implemented with local Windows and Node.js evidence; cross-host and cross-browser qualification pending
- Extends: [Decision 0174](0174-Portable-Compiler-Memory-Contract-And-Wasm-Bytes-Entry.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Decision 0174 established a capability-free compiler artifact and byte-array guest transport, but the retained interpreter rejected that compiler at its sixteen-function preflight ceiling. The exact portable compiler has 326 functions, 99,839 aggregate instructions, maximum 1,049 declared locals, maximum stack depth 34, recursion, and a `Main(bytes) -> bytes` browser entry. Compiler execution could not begin until the interpreter admitted that measured metadata under explicit bounds.

Raising every guest to compiler-sized frames would regress the retained small workloads and consume the outer monotonic value arena before useful work. The frame model therefore also needs to preserve the existing compact path.

## Decision

- Raise the retained interpreter selector to at most 4,096 functions, 64 parameters, 2,048 combined parameter/local cells per function, stack depth 64, and 400,000 aggregate decoded instructions. These bounds admit the measured compiler and remain no wider than the compiler-capacity verifier where the contracts overlap.
- Raise the versioned request contract to at most 20,000,000 guest instructions and call depth 64. The host still chooses a smaller positive value per request, and the enclosing execution ABI keeps its independent outer meter.
- Size local frames per candidate. Preserve a minimum 128-cell frame for existing workloads, then double to the smallest power of two that contains the candidate's maximum combined parameter/local count, capped at 2,048 cells. Calls in one candidate share that deterministic frame width.
- Build the zero frame by bounded doubling instead of repeated linear append. Keep the existing immutable byte-backed local and call-frame representation for this slice so the next allocation boundary is measured rather than hidden.
- Require the exact portable compiler to pass complete verification and the enlarged interpreter preflight. Prove entry into guest execution with a one-instruction request that returns normal `WVXO 2` guest-budget exhaustion.
- Run a compiler-capable request separately and pin the first enclosing Wasm runtime failure. Do not report an outer allocation failure as a guest compiler diagnostic.

## Consequences

The exact portable compiler is no longer merely verified or admitted by shape. It enters the Wasm-hosted Windvale interpreter and executes its first WVB instruction without .NET. This closes function-table, parameter, local-frame, stack, aggregate-instruction, guest-budget, and call-depth preflight capacity for the measured artifact.

The next blocker is now execution storage. Compiler-scale frames round to 2,048 eight-byte cells. The current `local.store` implementation reconstructs the immutable byte-backed frame through slices and concatenation, and the profile-16 Wasm runtime uses one 4 MiB monotonic value arena. A full-budget compiler attempt therefore returns outer `WVR3018` before publishing `WVXO 2`. Increasing the arena alone would postpone rather than solve allocation proportional to repeated whole-frame replacement.

The next implementation must give interpreter locals and saved frames bounded reclaiming or reusable storage while preserving Windvale value semantics and exact reset. A direct-AOT compiler artifact remains an allowed alternative if a measured implementation plan is smaller than adding safe interpreter-owned mutable frame storage; this decision does not make that claim yet.

## Local evidence

The expanded interpreter is 67,251 WVB bytes with SHA-256 `cb6c3c8528a6da45c3f2cfd0c7faf63663b9015f9d58bd28996b789602dadd19`. Its one outer function has 4,094 locals, 62,984 code bytes, 13,781 instructions, and maximum stack three. It lowers in 286,009,307 Windvale instructions to 412,367 import-free Wasm bytes with SHA-256 `f1ed2586544c92bce64cd45ae00ae44a34086f80fa0d08efc8ae8f6b5b3e7e47`.

The exact 597,545-byte portable compiler receives canonical WVSS for `Function-Only.wv`. With guest budget one and call depth 64, it completes preflight, executes one instruction, and returns `WVXO 2` status `3011`, guest count one, and empty result after exactly 44,163,574 outer instructions in both the reference runtime and Node.js. With guest budget 20,000,000, the generated Wasm returns outer status `3018`, no output, and 44,978,597 outer instructions.

Every retained scalar, text, bytes, formatting, SHA-256, record, enum, resource-failure, and version-2 byte-entry case continues to agree with the reference runtime. The focused Seed WebAssembly case passes a zero-warning Release build and exact identities in 104.588 test seconds. The complete 34-generated-Wasm-artifact gate rebuilds all artifacts, runs both compiler adapters through all three verifier phases, executes the expanded interpreter evidence, and passes under Node.js 24.18.0 on Windows in 424.7 seconds. Change-aware Windows verification then passes a zero-warning Release build and all 87 selected Seed tests in 495.977 suite seconds; the WebAssembly and golden cases take 104.832 and 255.095 seconds, and the complete command takes 507.5 seconds. This is local development evidence rather than cross-host or browser qualification.

## Rejected alternatives

Keeping the sixteen-function selector was rejected because verification without execution no longer advances the editable compiler path.

Giving every candidate a fixed 2,048-cell frame was rejected because it exhausted the outer arena in previously supported formatting, SHA-256, and record workloads. Candidate-adaptive sizing preserves their bounded path.

Only raising the 4 MiB outer arena was rejected because immutable whole-frame replacement scales with local stores, not merely live frame bytes.

Claiming compiler execution from preflight alone was rejected. The one-instruction `WVXO 2` result is required evidence that dispatch began.

## Reconsider when

- A bounded reusable-frame representation can be expressed through existing Windvale semantics and lowered without unsafe aliasing.
- A new explicit mutable-storage contract has verifier, reset, bounds, and malformed-input rules.
- Direct AOT of the portable compiler has a measured smaller implementation and qualification path.
- Cross-host or browser engines disagree on the expanded artifact, high instruction counts, or outer allocation failure.
