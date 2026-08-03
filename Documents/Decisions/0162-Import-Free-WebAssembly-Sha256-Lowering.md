# Decision 0162: Import-free WebAssembly SHA-256 lowering

- Date: 2026-08-03
- Status: Implemented with local Windows and Node.js evidence; cross-host and cross-browser qualification pending
- Extends: [Decision 0158](0158-Wasm-Hosted-Wvb-Formatting-And-Quoting.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Decision 0158 left `bytes.sha256_hex` as the last pure text/bytes operation needed by the Windvale compiler but absent from the Wasm-hosted WVB interpreter. Delegating it to Web Crypto would add a host import and make asynchronous JavaScript behavior part of a deterministic Windvale operation. Adding general source-language bitwise operations solely to reach SHA-256 would broaden the language before those operations had an independently motivated contract.

The Windvale-authored WebAssembly backend can instead recognize the already specified WVB opcode and emit its fixed target implementation directly. This preserves WVB as the semantic contract, keeps the browser artifact import-free, and leaves any future general bitwise-language design separate.

## Decision

- Advance the bounded selector to profile 16 while retaining execution ABI 3, `WVXI 1`, `WVXO 1`, fixed memory, all profile-15 resource limits, and the separate complete-verifier-first pipeline.
- Admit text descriptor locals, treat `text.to_utf8` as descriptor identity after verified Windvale text construction, and lower `bytes.sha256_hex` to deterministic Wasm integer, memory, and control instructions with no host import.
- Reserve linear-memory bytes 0 through 335 as private SHA-256 scratch: 64 schedule words, eight hash-state words, eight working words, padded length, block cursor, and two temporaries. The ABI input begins at 65,536, so scratch cannot overlap input, output, or the allocation arena.
- Implement SHA-256 padding and block processing over the bounded input descriptor. The ABI input ceiling keeps the bit length within the low 32 bits; the high four bytes of the eight-byte encoded bit length are therefore zero.
- Publish exactly 64 lowercase ASCII hexadecimal bytes through the existing charged arena. Insufficient aggregate space returns `WVR3018` before publication.
- Extend the retained Wasm-hosted interpreter to execute WVB opcode 125 by hashing its guest-heap slice through the new import-free lowering and appending the resulting text to its existing 64 KiB guest heap. Guest instruction metering remains one charge for the semantic WVB operation.

## Consequences

The verify-then-interpret path now covers the Windvale compiler's complete pure scalar, text, and bytes operation set. SHA-256 semantics are controlled by Windvale code and generated Wasm, not browser cryptography, locale, or JavaScript strings. Every previously generated Wasm module remains byte-identical.

Profile 16 is deliberately named because the selector's accepted outer WVB types and operations changed. Execution ABI 3 and its browser-facing memory contract did not change.

This does not complete runtime-value coverage. Records and enums remain the next part of the active goal, followed by executing the compiler WVB, packaging the complete static worker, and cross-host/cross-browser qualification.

## Local evidence

The expanded backend core compiles to 284,271 WVB bytes with SHA-256 `47e7689516b4c121fd4a05a6a98a90b8e5845f44fecb01c8703efb4334c86044`; the composed tool has SHA-256 `7ea8e83ab798a78def3349022aff38daa1d5ef830d409bcedd868b42a05e0e2d`.

`Wvb-Scalar-Interpreter-Main.wv` compiles to 53,761 WVB bytes with SHA-256 `5db84237d88f2204d8330e14d2964d3f6bfe08d36dd01f818c4760c68b3f0b7b`. Its one function has 3,248 nonparameter locals, 50,340 code bytes, 11,004 instructions, and maximum stack three. The profile-16 backend lowers it in exactly 246,994,217 Windvale instructions to a deterministic 334,209-byte import-free Wasm module with SHA-256 `2b932f153be8d428f35ef22a3504a0895cad9e8d1b83d0e2d8e4e3d480489cbe`.

The 1,869-byte SHA fixture has SHA-256 `91913b1276521e61cd3577e2f7f95d7116be69147fb42c8b6a365cfeca3ce054`. It covers empty input, `abc`, 55 and 56 repeated `a` bytes across the padding boundary, and 65 repeated `a` bytes across a second block. The complete verifier admits it in exactly 1,914,004 instructions. The reference runtime and Wasm-hosted interpreter both return `42` after 3,996 guest instructions; the interpreter consumes 2,747,726 outer instructions.

The 1,168-byte aggregate-heap fixture has SHA-256 `7d20e7e4c3209a18cd5e021898d37fb3e2296a64ee0ecfefadbc11eabe43ef9e`. Its preceding allocations leave 65,535 guest-heap bytes charged, so the 64-byte SHA result is the exact failing allocation. The complete verifier admits it in 603,492 instructions; the unrestricted reference runtime succeeds, while the bounded interpreter returns guest `WVR3018` after 388 guest and 344,902 outer instructions.

The independent emitted-Wasm decoder requires real block/loop, memory-read/write, rotate-right, shift, bitwise, and select evidence in the generated implementation. On ancestor `a797e31`, change-aware verification completes a zero-warning Release build, passes the editor contract, and passes all 87 selected Seed tests in 396.972 suite seconds; the WebAssembly and golden cases take 74.454 and 211.613 seconds. The final SHA-specific heap fixture then passes the focused Seed case in 73.594 seconds and the complete twenty-nine-artifact gate in 94.6 seconds. That gate rebuilds every module, proves the previous twenty-eight generated Wasm identities unchanged, rejects imports, validates and instantiates each module, and passes under Node.js 24.18.0 on Windows. After the final rebase to upstream `5d72a41`, the focused WebAssembly case again passes with a zero-warning build in 74.039 seconds. This is local development evidence, not cross-host or browser qualification.

## Rejected alternatives

JavaScript Web Crypto was rejected because it requires a host boundary for a pure synchronous Windvale operation and would make the current import-free artifact dependent on browser API behavior.

General Windvale bitwise syntax was rejected for this slice because the WVB SHA-256 operation already has defined semantics and no separate language requirement yet justifies exposing a broader primitive set.

Charging the internal SHA-256 rounds as guest WVB instructions was rejected because the guest contract meters semantic WVB operations, not target implementation instructions. The enclosing Wasm-hosted interpreter still has its independent outer budget.

## Reconsider when

- General bitwise operations receive a source, WIR, WVB, verifier, runtime, and cross-backend contract.
- A shared Foundation implementation can replace target-specific expansion without adding imports or changing observable resource behavior.
- Larger ABI inputs require a full 64-bit encoded bit-length path.
- Reentrant or concurrent execution within one Wasm instance requires per-invocation scratch rather than the current disposable-instance region.
