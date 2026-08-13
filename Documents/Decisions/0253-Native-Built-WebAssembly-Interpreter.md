# Decision 0253: Native-built WebAssembly interpreter

- Date: 2026-08-05
- Status: Implemented as a candidate; dual-host qualification pending
- Advances: [Decision 0252](0252-WebAssembly-Envelope-And-Packed-Effects.md)
- Uses: [Decision 0213](0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md)
- Contract: [Windvale WebAssembly](../../Specifications/Windvale-WebAssembly.md)

## Context

The composed WebAssembly interpreter project listed its envelope dependency before Foundation SHA-256. Stage 0 canonicalized the dependency set and built the project, but the pinned Windvale-native build driver passed that order into the native compiler and reached `Sourceˉbindings` rejection. Decision 0252 therefore still required Stage 0 to produce the experimental interpreter WVB even though the repository already had a qualified ordinary no-.NET source-to-WVB front door.

Project 1 declares dependency directives order-independent. Reordering the explicit inventory is therefore allowed and must not change a successful artifact. The canonical module-name order is Foundation SHA-256 followed by the WebAssembly envelope reader.

## Decision

List the two dependency sources in canonical module-name order in `Projects/Tests/Windvale-Wvb-Scalar-Interpreter.wvproj`. Keep root identity, imports, source bytes, compiler semantics, WVB format, and WebAssembly profile unchanged.

Use the pinned native front door as the preferred production measurement for this interpreter WVB. Retain Stage 0 output as independent recovery and differential evidence. Do not claim that the later WVB-to-Wasm lowering or website publication path is .NET-free until a native packaged backend owns those steps.

## Consequences

- `Tools/Native/Build-Wvb.cmd` builds, compiler-aligned verifies, and atomically publishes the three-function interpreter WVB without loading .NET.
- The order-only manifest change leaves Stage 0 output byte-identical with Decision 0252.
- The native compiler reuses temporary slots and emits 981 root locals instead of Stage 0's 5,364. The SHA and envelope helpers remain at 903 and 414 locals.
- Native and Stage 0 WVB bytes are different verified compiler artifacts. Behavioral evidence, not byte identity, relates them.
- The pinned native build driver's order sensitivity remains a narrower conformance defect because Project 1 promises order-independent successful bytes. Canonical inventory is a bounded project correction, not a new manifest requirement.

## Focused evidence

The ordinary native front door publishes 105,936 WVB bytes with SHA-256 `26ba1a6a1bf992ca5bb1682fd1342fa3ec3b7d4694035a82e58e4f9cbaad1c53`. It contains three functions, 103,396 aggregate code bytes, and 22,523 decoded instructions. The same source state still produces the Decision 0252 Stage 0 identity `ae4700a082f2188d1d40d5281322ba43224e26fe2a5708ed7265bc85febea5eb`.

The retained WebAssembly backend lowers the native WVB in 260,829,445 instructions to 782,416 import-free Wasm bytes with SHA-256 `35f79e8a819f395c3f0d57cd2d59812c341e0340d337e63ac56ae0b4fa705916`. Ordinary Node.js passes the exact ownership, text/bytes, formatting, SHA-256, one-short, reset, and seven malformed-envelope cases. Guest and outer meters match the Stage 0-derived Wasm exactly. On the measured Windows host, the 15,627-instruction ownership case falls from about 34 seconds to 18 seconds and the exact portable compiler's 100,000-instruction calibration falls from about 53 seconds to 36 seconds; wall time is informative local evidence, not a portable contract.

## Reconsideration triggers

Fix the native build driver's order independence when that pinned application is next reconstructed and qualified. Replace the Stage 0-hosted lowering evidence with a packaged Windvale-native backend on both permanent hosts. Promote this WVB identity into website publication only with deterministic asset generation, worker containment, and browser verification evidence.
