# Decision 0149: Windvale-native WebAssembly WVB executable verifier

- Date: 2026-08-03
- Status: Implemented with local Windows and Node.js evidence; cross-host and cross-browser qualification pending
- Extends: [Decision 0146](0146-Expanded-Descriptor-Bearing-WebAssembly-Call-Graph.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The modular Windvale verifier already consumed every WVB 1.6 section and checked canonical metadata, declarations, identities, indices, data kinds, branch boundaries, exports, and nominal references. It did not prove that executable instructions agree with local, operand-stack, call, capability, record, enum, return, reachability, or declared-stack contracts. The browser therefore still needed the Stage 0 verifier before it could execute newly supplied WVB.

General WVB locals have deterministic type-specific defaults. Executable verification must type-check every local access, but a load is not invalid merely because no earlier store appears. The source compiler also emits an empty operand stack at every WVIR operation and basic-block boundary. That invariant permits a smaller first browser verifier than the general Stage 0 graph worklist while preserving all compiler-produced control flow admitted by this decision.

## Decision

- Add `Wvb-Executable-Verify-Phase.wv` as two composable portable `bytes -> bytes` functions. `Hˉexecutable` verifies typed executable value flow; `Iˉcontrol` verifies compiler-aligned control reachability and target boundaries. Success returns the unchanged input descriptor and rejection returns an empty descriptor.
- Compose both functions after the seven existing semantic phases and before the final success result. The complete verifier remains one import-free profile-13 call graph under execution ABI 3.
- Bound candidate input to 256 functions, 131,072 aggregate code bytes, 16,000 aggregate decoded instructions, and declared operand-stack depth sixteen. The earlier structural and semantic phases remain responsible for canonical section layout, field ranges, instruction widths, indices, names, identities, and UTF-8.
- Prove exact local-load and local-store shapes, typed primitive and bytes operations, checked scalar families, function arguments and results, capability arguments and results, record construction field order, record-field receiver identity, enum identity, return shape, operand-stack depth, and exact declared maximum stack.
- Preserve canonical default-local semantics. Do not add a store-before-load rule to general WVB.
- For this first browser-executable subset, require an empty stack at every `jump` and after the condition consumed by `branch.false`; require every control target to begin at zero or immediately after a jump, branch, or return; reject an instruction region after a jump or return unless an earlier forward predecessor targets it; and require every function to end in a control terminator. This is the source-compiler control contract, not a claim to accept every valid hand-authored WVB control-flow graph.
- Keep capability authorization outside the WVB byte stream. The verifier proves declared call signatures, while the worker or host must separately compare requested capabilities with the explicitly granted set before execution.
- Raise the Stage 0 playground lowering ceiling from 200,000,000 to 225,000,000 instructions. The larger complete verifier requires 213,655,515 instructions to construct; the generated Wasm and execution ABI limits remain unchanged.

## Consequences

Portable source-compiler output now has a Windvale-authored executable admission proof that runs as WebAssembly without .NET. The complete artifact can be shipped as a static browser asset and can reject executable type and control corruption before an interpreter consumes the candidate WVB.

This does not yet switch the editable playground. Source-to-WVB compilation, WVB execution after verification, capability authorization, disposable-worker orchestration, cross-host construction, and Chromium/Firefox/WebKit qualification remain separate gates. General WVB graphs with nonempty stack joins also remain on the Stage 0 verifier until a bounded graph-worklist representation is implemented and qualified.

The two new phase functions fit the retained profile-13 per-function limits without raising them. `Hˉexecutable` uses 2,019 nonparameter locals, 32,766 code bytes, and maximum stack three. `Iˉcontrol` uses 564 locals, 9,793 code bytes, and maximum stack three.

## Local evidence

The standalone two-function phase WVB has SHA-256:

```text
93fa740c433f291af082ce3e8d9fbd304d7e9547a30b6a59ac73b752eb68a02d
```

The complete ten-function verifier is 115,483 WVB bytes with 108,331 aggregate code bytes and SHA-256:

```text
6a26b09c0f96e3fa9edf8c180ee8f4b2551f1b1007f0faabcec39be1106285b4
```

The Windvale backend lowers it in exactly 213,655,515 instructions to a deterministic 722,837-byte import-free Wasm module with SHA-256:

```text
6060b8198405b5f8763890ef5b53482398e1e0c7716f91ab279d9307db8d077b
```

The reference runtime and Node.js accept the compiler-produced data/text, nominal record/enum, and hosted-capability fixtures. Node.js reports exact verifier budgets of 4,181,579, 3,250,582, and 241,607 instructions, and the first fixture returns `WVR3011` one instruction below its successful budget.

Nine structurally and canonically valid mutations pass the earlier semantic-only Windvale verifier but are rejected by both the Stage 0 oracle and the new complete verifier. Node.js pins the rejection counts for operator stack kind, local store kind, call argument identity, record receiver identity, enum operand identity, branch condition kind, unreachable instruction region, declared maximum stack, and capability argument kind at 1,489,699; 962,942; 972,877; 1,007,979; 1,018,861; 1,024,723; 1,586,834; 1,501,929; and 172,552 instructions respectively.

`Tools/Verify/Verify-WebAssembly.ps1` rebuilds the phase composition, checks the exact WVB and Wasm identities, validates the ABI, instantiates the module without imports, exercises accepted and hostile inputs, and passes under Node.js 24.18.0 on Windows.

## Rejected alternatives

Treating default-valued locals as uninitialized was rejected because it would contradict canonical WVB semantics and the Stage 0 verifier.

Raising the 32,768-byte or 2,047-local per-function limits was rejected because the verifier separates cleanly into typed-flow and control-flow ownership under the already qualified profile-13 call graph.

Claiming full general-WVB verification was rejected because the first control phase deliberately requires source-compiler-aligned empty-stack boundaries. The narrower claim is explicit and differentially testable.

Embedding capability grants in WVB was rejected because declarations describe requirements, while authorization is a host decision that must remain explicit and revocable.

## Reconsider when

- The Wasm-hosted interpreter needs a valid general-WVB graph with nonempty stack joins.
- A bounded mutable worklist or stack-shape table can replace allocation-free control rescans with a simpler measured proof.
- Portable-only playground execution is ready to become the default and hosted capability grants need worker integration.
- The exact artifact has matching Windows/Linux construction and Chromium, Firefox, and WebKit execution evidence.
