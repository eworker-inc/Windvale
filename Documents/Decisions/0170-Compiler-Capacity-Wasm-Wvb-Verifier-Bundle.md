# Decision 0170: Compiler-capacity Wasm WVB verifier bundle

- Date: 2026-08-03
- Status: Implemented with local Windows and Node.js evidence; cross-host and cross-browser qualification pending
- Extends: [Decision 0166](0166-Wasm-Hosted-Record-And-Enum-Values.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Decision 0166 completed the scalar, text, bytes, record, enum, and bounded-memory value families needed before attempting the Windvale compiler. The exact `Compilerˉsourceˉwvbˉtool` WVB is 599,868 bytes and declares 328 functions, 481,356 code bytes, 100,194 instructions, 31,251 nonparameter locals, maximum function-local count 1,049, maximum operand stack 34, a recursive call graph, and six hosted capabilities.

The retained complete verifier deliberately admitted only 256 functions, 131,072 code bytes, 16,000 instructions, and operand depth 16. It therefore returned empty rejection for the compiler after 36,507,729 outer instructions. Raising only those limits was insufficient: the canonical metadata/reference phase needs 1,381,753,055 Windvale instructions, typed execution proof needs 2,434,833,692, and control/reachability proof needs 1,952,101,000. Their 5,768,687,747 combined instructions cannot fit one 32-bit execution-ABI meter even though every phase fits independently.

This is a metering/composition boundary, not evidence that any semantic check may be skipped.

## Decision

- Preserve the retained 722,837-byte complete verifier and every earlier generated Wasm artifact byte for byte.
- Derive a compiler-capacity verifier bundle from the same canonical Windvale semantic and executable-verifier sources. Do not introduce a second verifier implementation or a host-language semantic oracle.
- Expand only candidate capacity: at most 4,096 functions, 4 MiB aggregate code, 400,000 decoded instructions, 4,096 operand cells, and the existing WVB-format parameter/local/type bounds.
- Split verification into three import-free execution-ABI-3 artifacts: canonical metadata/references, typed execution, and control/reachability. The host supplies the identical candidate bytes to fresh fixed-memory instances and admits the candidate only when all three return the exact one-byte success value.
- Keep independent instruction meters for the three phases. Normalize exported Wasm `i32` instruction globals to unsigned values in the JavaScript verifier so exact counts above `2^31 - 1` are preserved.
- Reconstruct the exact compiler WVB from `Projects/Examples/Windvale-Compiler.wvproj` in the repository gate and require its existing 599,868-byte identity before running the bundle.

## Consequences

The exact Windvale compiler now passes complete structural, canonical, typed, nominal, call, return, control-flow, reachability, and declared-stack verification under Node.js without .NET. This closes compiler admission, not compiler execution.

Three phases make the existing 32-bit meter honest and independently exhaustible. A worker can run or terminate each phase separately and never feeds the candidate to the interpreter until every phase succeeds. The split does not change WVB semantics, acceptance rules, execution ABI 3, fixed memory, or the compiler artifact.

The verifier remains intentionally rescan-based and expensive at compiler scale. Splitting makes the proof executable but does not claim interactive latency. Metadata indices or another bounded Windvale-owned representation should reduce the current multi-billion-instruction verification cost before the default playground switch.

The retained interpreter still rejects the compiler during preflight after 128 outer instructions. The next implementation boundary is a portable in-memory compiler entry contract plus compiler-scale function, frame, recursion, guest-budget, record, and dynamic-value capacity. The hosted CLI's process/file/console capabilities must not be smuggled into the browser; editable source and resulting WVB need an explicit memory ABI or authorized worker protocol.

## Local evidence

The semantic verifier is 70,016 WVB bytes with SHA-256 `16193b7cd5a16b8e9f1cf3bdb2c72fed1b4abd464e165b42a94b0b593636a1c6`. It lowers in 135,299,773 Windvale instructions to 440,093 import-free Wasm bytes with SHA-256 `3a760d39b9ce0bac50b3eea0b0000a986a60363a7d222d7de1587f6b19fadd1e`.

The typed verifier is 45,546 WVB bytes with SHA-256 `bc282d6163b97e70766fb0882395ce594a00ecafb60bea12e4b401e731f3f0be`. It lowers in 85,826,365 instructions to 282,718 import-free Wasm bytes with SHA-256 `6fc1e02498de6345b441d0f34e52fe9d0c014642fc637f9066623b07b4329240`.

The control verifier is 45,548 WVB bytes with SHA-256 `7848041e406b0f92ab3219e9a7052e9cca337014fa9698978bf2c894c689532b`. It lowers in 85,882,186 instructions to 282,718 import-free Wasm bytes with SHA-256 `597c0f8313aac9fbcb1ba50fbcd4e25937f0cc464d090d491570f8eeb559253d`.

Under Node.js 24.18.0, the three fresh Wasm instances accept the exact compiler in 1,381,753,055, 2,434,833,692, and 1,952,101,000 instructions respectively. The full 34-artifact repository WebAssembly gate rebuilds the backend, retained artifacts, bundle, compiler, and fixtures; validates every Wasm module; rejects imports; runs all previous cases; verifies the compiler bundle; and passes locally on Windows in 366.3 seconds. The focused Seed WebAssembly case passes a zero-warning Release build and exact source/WVB/Wasm identities in 95.546 seconds after the final implementation. After a clean rebase onto the packaged-compiler decisions, the same focused case passes again in 95.078 seconds.

Change-aware Windows verification then completes in 427.7 seconds with a zero-warning Release build and all 87 selected Seed tests passing in 419.662 suite seconds. The WebAssembly and golden compiler cases take 93.581 and 215.794 seconds. This remains development feedback rather than cross-host or browser qualification.

The reference compiler executes the hosted tool over `Function-Only.wv` in 5,030,688 guest instructions and emits the canonical 815-byte WVB with SHA-256 `9ccfed0509e84bfc63979c6dc13170c14762efbdaa448b4c5894325f31aa7761`. This is measurement for the next execution slice, not Wasm-hosted compiler execution evidence.

## Rejected alternatives

Increasing one combined verifier budget beyond 32 bits was rejected because execution ABI 3 deliberately exposes a 32-bit exact counter and the browser needs independently terminable work.

Skipping typed or control proof for the compiler was rejected because exact artifact identity is test evidence, not a replacement for verification of untrusted editable-pipeline input.

Using JavaScript to parse or semantically approve the compiler was rejected because host code must not define WVB acceptance.

Treating the hosted compiler's six ambient capabilities as the browser API was rejected because browser source input and compiler output need explicit memory-owned contracts.

## Reconsider when

- Windvale-owned metadata indices can reduce phase work while preserving identical acceptance.
- An execution ABI with a wider meter is justified independently of this compiler workload.
- Compiler execution selects a portable in-memory adapter or supplies evidence that direct AOT is the smaller coherent route.
- Cross-host or browser engines expose a phase-budget, memory, or high-bit-counter difference.
