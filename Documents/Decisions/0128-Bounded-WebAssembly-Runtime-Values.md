# Decision 0128: Bounded WebAssembly runtime values

- Date: 2026-08-02
- Status: Implemented with local Windows and Node.js evidence; cross-host and cross-browser qualification pending
- Extends: [Decision 0123](0123-Versioned-WebAssembly-Linear-Memory-And-Utf8-Buffers.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Execution ABI 3 establishes fixed input and output windows, strict UTF-8 transport, and exact text and byte identity functions. A Windvale-native WVB verifier and compiler need more than transport: they must inspect bounded byte sequences, construct encoded fields, retain typed locals, and fail with the same range and resource statuses as the reference runtime.

Adding one exact compiler fixture for every operation would not establish a reusable runtime boundary. The next slice instead needs one statically checked straight-line profile whose operations can compose while retaining deterministic target bytes, exact WVB instruction charging, and explicit allocation limits.

## Decision

- Add experimental profile 9 for one canonical `Main(Input: bytes) -> bytes` function with zero through 255 nonparameter locals of type `i32`, `bool`, `u8`, `u32`, or `bytes`.
- Admit compiler-produced constants and local flow plus `Bytesˉlength`, `Bytesˉslice`, little-endian `u8`, `u16`, `u32`, and `i32` reads, `U32ˉfromˉu8`, `Bytesˉconcat`, little-endian one-, two-, and four-byte construction, `pop`, and one final `return`.
- Reconstruct the operand types, local indices, definite local initialization, instruction count, code extent, exact declared maximum stack, and final bytes result before emitting any target bytes. Bound code to 16,384 bytes, 4,096 instructions, and operand-stack depth four.
- Represent an internal bytes value as an `i64` descriptor whose low 32 bits are a pointer and high 32 bits are a length. This representation is a generated-module detail and does not change canonical WVB.
- Borrow the host-owned input window for the initial value and slices. Constructed values use a checked monotonic arena spanning the existing 4 MiB output window. Concatenation copies both operands to a fresh arena extent; fixed-width builders store little-endian bytes in a fresh extent. A successful return normalizes the selected descriptor to the published output base with `memory.copy`.
- Preserve execution ABI 3 and its import-free, fixed 129-page memory. Charge each WVB instruction before attempting it, reset output length and instruction count before validation, and publish output length only after status zero.
- Return `WVR3008` for an invalid slice or read range, `WVR3011` for budget exhaustion, `WVR3015` when one bytes value would exceed 4 MiB, `WVR3016` when `Bytesˉfromˉu16ˉlittle` receives a value above 65,535, and `WVR3018` when otherwise valid constructed values exhaust the aggregate 4 MiB arena. Failures publish no output descriptor.

## Consequences

The direct backend can now execute a general straight-line compiler-produced bytes program rather than only an identity stencil. The retained program composes primitive locals, reads, slicing, widening, construction, and repeated concatenation; separate cases distinguish value size from aggregate arena exhaustion. These are the byte primitives needed to begin a Windvale-native WVB verifier.

The monotonic arena deliberately favors a small auditable ownership model over reclamation. A final value can fit within 4 MiB while its construction history exhausts the arena and returns `WVR3018`. A later allocator may revise the internal representation only behind a new explicitly qualified profile or ABI contract when observable behavior changes.

Profile 9 does not yet compose profile-3 arithmetic or profile-7 control and calls with descriptors. It does not add general text operations, records, enums, collection, capabilities, a complete WVB verifier, or a compiler/interpreter running in WebAssembly. The normal editable playground still uses .NET.

## Local evidence

The main runtime fixture compiles to 914-byte WVB SHA-256 `6436f97c0e9abf131cc3a503c4449104706aa66eb0292a282a978fb7a5c5e100` and deterministic 4,878-byte Wasm SHA-256 `7bd5d2b0bc256503cd07dc300e528da38f8a09bcfec4c2b1007c1994db1b88f4`. The reference interpreter and Node.js agree on the 19-byte result and 155 charged instructions; budget 154 returns `WVR3011` with no output, and empty input reaches `WVR3008` after 26 charges.

Three focused artifacts retain exact failure boundaries. The 718-byte concatenation module SHA-256 `94533e9d01bdfcc606a3225ac28c774ecadd3cc0e0eccb02a7dba4f3fdb4ccb2` succeeds with an exact 4 MiB result and returns `WVR3015` one input byte beyond its 2 MiB value boundary. The 797-byte u16 guard SHA-256 `f312812fedae4c8dd45ffcb022301c1e85d7bdad4c71906a771cfc95333cde41` distinguishes successful little-endian construction, `WVR3016`, and `WVR3008`. The 921-byte aggregate-arena module SHA-256 `0e37802a606ee67abd467ddc5da84f0d18807bb86b8bf497c4bdf0a41fa5a089` distinguishes `WVR3018` from `WVR3015` at the top of the input capacity.

The C# conformance oracle independently verifies the source profile and decodes the generated Wasm sections, fixed memory, globals, ordered exports, local representations, complete opcode stream, meter count, output publication, and bulk-memory operations. It also corrupts a load to reference a not-yet-initialized local and requires rejection without publication. The Node.js gate rebuilds and executes all four artifacts, including success, one-instruction-short budgets, range failures, exact value and arena boundaries, memory non-growth, deterministic digests, and invalid over-capacity input lengths.

## Rejected alternatives

Host imports for slicing, concatenation, or allocation were rejected because they would move Windvale value semantics and resource failures into JavaScript.

Publishing arbitrary internal descriptors was rejected because the host contract should expose only the fixed output window and a successful bounded length. Normalizing the returned value keeps temporary arena layout private.

Treating aggregate arena exhaustion as value overflow was rejected because a valid final value and an overlarge individual value are different resource failures in the Windvale runtime contract.

## Reconsider when

- A representative verifier program requires scalar arithmetic, comparisons, structured control, or calls to compose with these byte values.
- Text construction or validation beyond the identity boundary requires a typed descriptor policy.
- Records or enums require a versioned memory representation.
- Measured verifier/compiler workloads demonstrate that monotonic 4 MiB temporary storage is insufficient.
