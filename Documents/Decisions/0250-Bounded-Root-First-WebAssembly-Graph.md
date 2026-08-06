# Decision 0250: Bounded root-first WebAssembly descriptor graph

- Date: 2026-08-05
- Status: Implemented as a candidate; dual-host qualification pending
- Advances: [Decision 0219](0219-Root-First-WebAssembly-Leaf-Composition.md)
- Contract: [Windvale WebAssembly](../../Specifications/Windvale-WebAssembly.md)

## Context

The retained WebAssembly interpreter statically composed root `Main` with one Windvale SHA-256 leaf. Its exact two-function exception preserved Windvale-owned hashing, but prevented cohesive interpreter extraction. Completing guest heap ownership raised the root to 5,740 nonparameter locals. Node.js's baseline tier executed the result, while the ordinary optimizing tier exhausted the available process memory during compilation.

The existing descriptor-call encoder already emitted any bounded function count and shifted every guest call index around the allocator/reference helpers. The limiting contract was the validator's exact two-function root-first exception, not the generated ABI or private-body representation.

## Decision

Generalize the root-first alternative into a bounded dependency DAG. Exported `Main` remains ordinal zero. Every call must target a strictly greater ordinal, every non-root function must be reachable from `Main`, and the existing two-through-sixteen function, 131,072 aggregate-code, 400,000 aggregate-instruction, signature, stack, and generated-output limits remain. Strict ordinal increase rejects backward calls, self-calls, recursion, and cycles without a graph traversal; the reachability mask rejects unused injected functions.

Extract opcode stack-effect classification into `Wvb-Scalar-Interpreter-Stack-Effect.wv`. It receives the opcode as four little-endian bytes and returns an eight-byte consumed-count/flags value. Operation-specific dynamic effects remain in the interpreter root. The project now composes that helper with the existing Windvale SHA module, and all three functions remain capability-free `bytes -> bytes` functions.

## Consequences

- The root falls from 5,740 to 5,551 nonparameter locals; the focused stack-effect function owns 239 locals and the unchanged SHA function owns 903.
- The ordinary Node.js optimizing tier compiles and executes the retained same-instance probe instead of requiring `--liftoff-only`.
- The pressure workload gains a descriptor call per interpreted instruction and therefore more outer work. Exact guest meters, results, failures, heap ownership, and reset behavior remain unchanged.
- Execution ABI 3, fixed 129-page memory, public exports, helper indices, and absence of imports remain unchanged.
- The pinned Windvale-native build driver predates this source-binding shape and rejects the three-module composition. Stage 0 therefore remains necessary to produce this experimental WVB until a refreshed native front door is qualified; this decision does not claim .NET-free artifact production.

## Focused evidence

The current backend is 321,867 WVB bytes with SHA-256 `2e5fa504aa16c17c567f0e35161f2c5024336cc9f70617313270d7ae72d824fa`. Stage 0 compiles the interpreter composition to 111,316 WVB bytes with SHA-256 `82036943267eb21704916fdc2d48b9466964570092f283876e7245a86db9d6e5`. Its three functions contain 104,381 aggregate code bytes and 22,732 decoded instructions. The backend lowers it in 261,291,275 instructions to 770,608 import-free Wasm bytes with SHA-256 `7312b0b4bef49e58b354705b5474e278e000f867903a8484eee7eea777f04ccc`.

Default Node.js validates, compiles, instantiates, and executes the complete focused probe in one instance. The 15,627-instruction ownership workload cumulatively constructs 143,364 guest-heap bytes and 1,136 record field cells and returns `69`. The retained text/bytes, formatting/quoting, and SHA cases return `42`; budget 350 returns exact guest `WVR3011`; budget 351 then returns `42`, proving reset.

## Reconsideration triggers

Reconsider the forward-only ordering if canonical composition requires shared cycles or recursion. Such an expansion requires an explicit call-depth and recursion contract rather than weakening this DAG check. Reconsider byte-packed helper arguments when source-level private scalar helper calls can retain one Windvale implementation and the same bounded WebAssembly graph.
