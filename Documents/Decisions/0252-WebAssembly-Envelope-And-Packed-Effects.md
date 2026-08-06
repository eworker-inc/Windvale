# Decision 0252: WebAssembly interpreter envelope and packed effects

- Date: 2026-08-05
- Status: Implemented as a candidate; dual-host qualification pending
- Advances: [Decision 0250](0250-Bounded-Root-First-WebAssembly-Graph.md)
- Contract: [Windvale WebAssembly](../../Specifications/Windvale-WebAssembly.md)

## Context

The first bounded root-first graph moved static opcode stack-effect classification into a private Windvale function. That reduced the root enough for Node.js's ordinary optimizing tier, but it also added one descriptor-bearing private call to every interpreted guest instruction. The large portable compiler reached its guest budget correctly, yet the additional hot-call cost moved complete compiler execution farther from an interactive browser budget.

The request envelope, WVB section discovery, capability rejection, and unique `Main` export scan are cold preflight work with a cohesive boundary. They can use the graph without adding a call to the instruction loop. Static opcode effects are only four bits each, but the bounded WebAssembly backend rejects any nonempty WVB data section.

## Decision

Replace the hot stack-effect helper with `Wvb-Scalar-Interpreter-Envelope.wv`. It accepts the exact `WVXI 1` or `WVXI 2` request, validates framing and bounded module metadata, and returns one fixed 60-byte summary containing request limits, module and guest-input slices, section offsets, type and function counts, and the unique `Main` index. Empty output denotes rejection. The root remains responsible for complete executable verification and execution.

Encode eight four-bit static opcode effects in each `u32` word. A balanced group selection chooses the word and a variable shift extracts consumed-cell and produced-descriptor flags. Module-dependent record, field, call, and return effects continue to override the static value explicitly. This preserves an empty data section and the bounded root-first backend contract.

## Consequences

- The three capability-free `bytes -> bytes` functions remain root `Main`, Windvale SHA-256, and the envelope reader. All calls still target greater ordinals and the generated Wasm remains import-free.
- The root has 5,364 nonparameter locals, down from 5,551 with the hot helper and 5,740 before graph extraction. The envelope reader owns 414 locals and SHA-256 remains at 903.
- Static classification performs bounded branches and integer operations instead of allocating an argument/result packet and crossing a private descriptor call per guest instruction.
- A nonempty WVB data section remains an explicit backend limitation. This decision records it rather than silently widening profile 17.
- The pinned Windvale-native build driver still rejects this source-binding shape. Stage 0 remains necessary to produce the experimental composed WVB; no .NET-free artifact-production claim is made.

## Focused evidence

Stage 0 compiles the composition to 110,319 WVB bytes with SHA-256 `ae4700a082f2188d1d40d5281322ba43224e26fe2a5708ed7265bc85febea5eb`. The retained Windvale WebAssembly backend lowers it in 267,391,678 instructions to 791,182 import-free Wasm bytes with SHA-256 `cd70d42128137dd7df9c4cb17fc43bbbc4132739c6d70a5accabd70d6b6fcf53`.

Ordinary Node.js validates, compiles, instantiates, and executes the focused same-instance probe. The 15,627-instruction ownership workload cumulatively constructs 143,364 guest-heap bytes and 1,136 record field cells, returns `69`, and consumes 62,743,806 outer instructions. Text/bytes, formatting/quoting, and SHA return `42`; guest budget 350 returns exact `WVR3011`; budget 351 then returns `42`.

The exact 919,577-byte portable compiler with SHA-256 `2bf84dc2a8cbb80c52ec7fb6cb2e29eef27def1707f398a276c61063d73df06e` returns exact guest `WVR3011` at budget 1,000 after 78,492,135 outer instructions and at budget 100,000 after 192,935,833 outer instructions. These are calibration points, not a complete source-to-WVB result.

## Reconsideration triggers

Admit immutable data only through a separately bounded layout, validation, and deterministic-publication contract. Replace packed selection if scalar private helper signatures become available without descriptor packets or if a measured direct dispatch representation is both smaller and faster. Revisit the interpreter route when compiler execution demonstrates that direct verified lowering is the more coherent browser path.
