# Decision 0139: Descriptor-bearing WebAssembly call graph

- Date: 2026-08-03
- Status: Implemented with local Windows and Node.js evidence; cross-host and cross-browser qualification pending
- Extends: [Decision 0134](0134-Windvale-Native-WebAssembly-Wvb-Structural-Verifier.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Profile 11 can execute one large Windvale-written structural WVB verifier as import-free WebAssembly. The remaining semantic verifier must track canonical declarations, catalog identities, nominal shapes, typed locals and operand stacks, control-flow targets, and reachability. Keeping that implementation in one generated Wasm function would force unrelated semantic phases into one source function and continue increasing its already large local and code inventory.

Profiles 6 and 7 already lower acyclic scalar calls to real private Wasm functions under one instruction budget. Execution ABI 3 already carries bounded bytes values as private `i64` descriptors, but its selector previously accepted only one `bytes -> bytes` function. The missing seam was real descriptor-bearing calls composed with the profile-11 runtime and control operations.

## Decision

- Add experimental profile 12 over unchanged execution ABI 3. It accepts two through eight functions, each with exactly one `bytes` parameter and a `bytes` result. `Main` remains the only export and the final canonical function.
- Admit the profile-11 primitive, bytes, checked-unsigned, comparison, local, return, and terminator-aligned control operations in every function. Require at least one direct call in the module.
- Require every call target to have a lower canonical function ordinal than its caller. This rejects forward calls, self-calls, mutual cycles, and recursion and bounds dynamic call depth to eight.
- Retain at most 2,047 nonparameter locals, 32,768 code bytes, 100,000 decoded instructions, and operand-stack depth four per function. Bound the aggregate to 65,536 code bytes and 200,000 decoded instructions. Generated output remains bounded to 524,288 bytes.
- Emit one public ABI-3 wrapper plus one private Wasm function per WVB function. Private functions have target type `(i64) -> i64`; the descriptor representation remains an implementation detail and does not cross the host ABI.
- Add private mutable status, arena, and instruction-limit globals while retaining the exact ten public ABI-3 exports and their indices. Before a call, the caller publishes its current arena. After a successful call, it reloads the callee's advanced arena and continues with the returned descriptor. A callee failure records the Windvale status and returns an empty descriptor; every active caller propagates that status without publishing output.
- Reset output length, instruction count, shared status, shared arena, and instruction limit in the public wrapper before every run. All caller and callee instructions charge the same budget.

## Consequences

Large Windvale-authored Wasm consumers can now be decomposed into bounded functions without JavaScript orchestration, host imports, duplicate arenas, or per-function budgets. This is the enabling call boundary for splitting the semantic WVB verifier and, later, a WVB interpreter or compiler driver into understandable Windvale modules.

This milestone strengthens item 1 of the active .NET-removal goal and removes a scaling obstacle from items 4 and 5. It does not itself implement canonical semantic WVB verification, execute records or enums, compile editable source, contain the complete pipeline in one worker, or qualify a default switch away from .NET.

General mixed signatures, text parameters, record and enum values, capabilities, recursion, indirect calls, and more than eight functions remain unsupported. The private `i64` descriptor is generated-code policy, not a new portable WVB or public browser ABI.

## Local evidence

`Runtime-Calls-Main.wv` compiles to a 764-byte WVB module with SHA-256:

```text
a44c8bdbf9983a7929a769d5ca2e0b60323d72cf96b04e31450d9757bb15729a
```

Its three functions exercise a nested call, a call inside conditional control, empty and nonempty byte paths, slicing, concatenation, and cross-callee arena movement. The selector emits a deterministic 4,086-byte import-free Wasm module with SHA-256:

```text
5ee04d5b3b33399dce61709135709f0d0ebb7d6374e14759d83986859806eadd
```

The reference runtime and Node.js both map `[9, 8]` to `[9, 9, 8]` in exactly 127 WVB instructions. Budget 126 returns `WVR3011`, reports 126 instructions, and publishes no output. Empty input returns `[0, 0]` in 63 instructions. A repeated successful run proves wrapper reset and arena reuse. Changing a private call to target its caller is rejected by the selector with no output resource.

The independent C# decoder checks the source signatures and decreasing call ordinals and consumes both Wasm types, all function type indices, fixed memory, eleven globals, the ordered ten-export ABI, wrapper locals, and every private local layout. The focused Seed WebAssembly case passes locally. The Node.js verifier rebuilds, validates, instantiates, and executes 25 generated Wasm artifacts plus the retained verifier inputs on Windows. The changed-scope Windows verifier completes a zero-warning Release build and passes all 77 Seed tests, including the golden compiler contract; this is development evidence rather than cross-host qualification.

The updated standalone core WVB is SHA-256 `9d29c443e0642ecce87e5194ec9dc077be8d6b2fec97d47ba667404d0d09e2f9`. The composed hosted backend WVB is SHA-256 `8bef4da0e80aa5d6876800b7ed519e9ee79db9ee963f9d830da455354c58bd24`. Existing profile output artifacts retain their exact bytes.

## Rejected alternatives

Inlining the semantic verifier into one larger function was rejected because it couples semantic phases, worsens selector construction cost, and obscures bounded ownership without adding useful language evidence.

Routing calls through JavaScript was rejected because it would move Windvale value lifetime, instruction metering, and failure propagation into the host at the boundary intended to become self-contained.

Adding recursion with a dynamic depth counter was rejected because the semantic verifier does not require it and decreasing ordinals already provide a simple static depth proof.

Exposing descriptors in execution ABI 3 was rejected because the public fixed input/output windows and normalized successful output remain sufficient and safer for browser hosts.

## Reconsider when

- The semantic verifier needs more than eight functions or more than 65,536 aggregate code bytes.
- A measured compiler or interpreter requires mixed value signatures, nominal values, or bounded recursion.
- Direct structured-control reconstruction can replace per-function dispatch loops without weakening target validation.
- Profile 12 has matching Windows/Linux construction plus Chromium, Firefox, and WebKit execution evidence.
