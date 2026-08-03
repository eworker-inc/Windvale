# Decision 0174: Portable compiler memory contract and Wasm bytes entry

- Date: 2026-08-03
- Status: Implemented with local Windows and Node.js evidence; cross-host and cross-browser qualification pending
- Extends: [Decision 0170](0170-Compiler-Capacity-Wasm-Wvb-Verifier-Bundle.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Decision 0170 admitted the exact hosted Windvale compiler under three compiler-capacity WebAssembly verifier phases, but the artifact still exposed a no-argument CLI entry and declared six process, file, console, and diagnostic capabilities. Those are bootstrap-shell concerns, not authority the editable browser compiler should inherit.

The retained WebAssembly-hosted WVB interpreter also returned only one `i32`. Even a capability-free compiler could not receive a WVSS source set or publish canonical WVB through that interface. The next slice therefore needs a portable compiler adapter and byte-array guest entry before enlarging interpreter execution capacity.

## Decision

- Add `Compilerˉsourceˉwvbˉmemoryˉadapter`, a portable, capability-free Windvale module whose exported `Main(bytes) -> bytes` passes one complete WVSS 1 value to `Compilerˉcompileˉsourceˉwvb`.
- Publish the result through versioned `WVCO 1`: a sixteen-byte little-endian header followed by either canonical WVB bytes or one strict UTF-8 diagnostic. The adapter performs no path resolution, process argument access, file I/O, console output, or ambient capability call.
- Build that closure from `Windvale-Compiler-Memory.wvproj`. Keep the hosted `Windvale-Compiler.wvproj` as the CLI and bootstrap artifact; the two adapters share the same compiler core instead of introducing parallel compilers.
- Retain `WVXI 1` / `WVXO 1` scalar execution byte for byte at the protocol level. Add `WVXI 2` with explicit module and guest-input lengths and `WVXO 2` with an explicit byte-result length. Version 2 admits only verified portable, capability-free `Main(bytes) -> bytes` candidates under the existing interpreter resource profile.
- Continue to require complete verifier success and capability policy before building either interpreter request. Empty interpreter output means malformed input or a verifier-valid candidate outside the bounded execution profile; it is not a compiler diagnostic.
- Qualify the protocol first with a small Windvale guest that appends byte `42`, then feed the exact portable compiler to the interpreter and pin its first unsupported boundary.

## Consequences

The browser-facing compiler contract is now entirely Windvale-owned and capability-free. A host can provide immutable WVSS bytes and receive a self-describing WVB-or-diagnostic envelope without adapting the compiler to JavaScript, C#, files, command-line arguments, or console streams.

The new byte-array execution route is real: the exact 209-byte `Bytes-Entry-Guest.wvb` accepts `[1, 2, 3]` and returns `[1, 2, 3, 42]` in 13 guest instructions. The import-free Wasm interpreter returns that result through `WVXO 2` under Node.js.

This does not yet execute the compiler. The exact portable compiler contains 326 functions, so the retained sixteen-function interpreter rejects it during preflight after 96,927 outer instructions. That rejection occurs before guest source processing and precisely selects the next implementation boundary: compiler-scale function metadata, frames, recursion, instruction budgets, record lifetime, and dynamic-value storage.

The hosted compiler remains useful for bootstrap and independent recovery evidence. `WVCO 1`, `WVXI 2`, and `WVXO 2` are experimental browser-pipeline contracts, not replacements for canonical WVB or the execution ABI.

## Local evidence

The portable compiler is 597,545 WVB bytes with SHA-256 `5b819f86ffa05feaae1e27feb0b6fe6eda5034f1b229d4d9917ac7fa8041a0d4`. It contains 326 functions, 479,733 aggregate code bytes, 99,839 instructions, maximum 1,049 locals, maximum stack depth 34, recursion, and zero capabilities. The three compiler-capacity Wasm phases admit this exact artifact independently in 1,380,573,747 metadata/reference, 2,430,056,746 typed-execution, and 1,951,031,795 control/reachability instructions.

Under the reference runtime, the adapter compiles `Function-Only.wv` from canonical WVSS in 5,030,333 guest instructions and returns an exact 831-byte `WVCO 1` response containing the canonical 815-byte WVB. Empty input returns an exact 96-byte `WVCO 1` response containing the stable 80-byte diagnostic `source-wvb status=Sourceˉwir wir-status=Sourceˉbindings function=0 operation=0` in 1,046 guest instructions.

The expanded interpreter is 66,945 WVB bytes with SHA-256 `b44580122734d682d4a351a5a9e272f369d536d4d0cb689e6a7fd65df4be3455`. It lowers in 247,197,892 Windvale instructions to 410,698 import-free Wasm bytes with SHA-256 `7ea0058b7f4f2b4c6886f5f968fb239bd995f263a44f9e56f1bf5a91bf2c2c23`. Its single outer function has 4,076 locals, 62,696 code bytes, maximum stack three, and 13,717 instructions. The small byte-array guest completes in 22,065 outer instructions; a truncated `WVXI 2` request returns empty output in 255; the portable compiler reaches its sixteen-function preflight ceiling and returns empty output in 96,927.

The focused Seed WebAssembly case passes a zero-warning Release build and every exact reference-runtime assertion in 99.888 test seconds. The repository WebAssembly gate reconstructs both compiler adapters, verifies the portable artifact through all three compiler-capacity phases, admits the byte-array fixture through the complete verifier, preserves the earlier scalar/text/bytes/SHA/record/enum differentials, and runs the version-2 protocol under Node.js 24.18.0. It passes locally on Windows in 422.7 seconds. Change-aware verification then passes the editor contract, a zero-warning Release build, and all 87 selected Seed tests in 461.150 suite seconds; the complete command takes 471.6 seconds. This is local development evidence rather than cross-host or browser qualification.

## Rejected alternatives

Passing browser source through the hosted compiler's file and process capabilities was rejected because a library requirement is not browser authority and those capabilities are unnecessary for immutable in-memory input.

Returning raw WVB with empty bytes for every failure was rejected because success and diagnostic output need an unambiguous, length-checked transport contract.

Replacing `WVXI 1` in place was rejected because existing scalar evidence remains useful and versioned coexistence makes the byte-result shape explicit.

Raising all interpreter limits before defining the input/output contract was rejected because it would spend compiler-scale resources without a publishable compiler result path.

## Reconsider when

- Compiler-scale execution evidence justifies a wider guest instruction meter or a multi-phase execution protocol.
- Dynamic-value ownership requires a different bounded heap or result-publication scheme.
- A worker pipeline needs streaming source input or output beyond the current fixed execution-ABI windows.
- Cross-browser evidence exposes an engine-specific limit in the expanded interpreter artifact.
