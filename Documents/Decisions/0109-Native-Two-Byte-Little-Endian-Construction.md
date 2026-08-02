# Decision 0109: Native two-byte little-endian construction

- Date: 2026-08-02
- Status: Accepted
- Advances: Native ABI 20
- Refines: [Decision 0066](0066-Borrowed-Bytes-And-Unsigned-Native-Values.md) and [Decision 0108](0108-Native-One-Byte-Construction.md)

## Context

Qualified ABI 19 admitted exact one-byte construction without changing ABI 18's typed physical-slot map or the existing execution-owned dynamic-value arena. Repeating exact compiler preflight then reached the next unsupported operation in `Compilerˉcompileˉsourceˉwvb`: `Bytesˉfromˉu16ˉlittle`.

The source language, WVB 1.6 verifier, and reference runtime already define this operation. It accepts `u32` values from zero through 65,535, produces exactly two little-endian bytes, and traps as `WVR3016` above that range. The missing boundary was a typed native operation, its checked machine shape, and native mapping of the existing trap—not a new source semantic, allocator, service, or byte order.

## Decision

- Add typed `Nativeˉbytesˉfromˉu16ˉlittle(Result, Value)` machine IR. Lower verified `Bytesˉfromˉu16ˉlittle` only from `U32` to `Borrowedˉbytes`, and independently reconstruct those exact operand/result types before selection.
- Load and compare the complete unsigned source against 65,535 before mutating arena state. A larger value writes service-failure detail 12, returns the retained packed status 5, and becomes the existing `WVR3016` trap. This extends the generated-failure vocabulary already used by bounded byte concatenation; it adds no runtime-service call.
- Allocate exactly two bytes from the existing execution-owned 16 MiB text/byte arena only after the range proof. Check cursor addition and capacity before publication, write a pointer/length descriptor with length two, then use the target's exact two-byte store. The x86-64 target is little-endian, matching the operation's specified byte order.
- Extend the byte-level fragment verifier with the exact source load, unsigned upper-bound branch, checked allocation, descriptor, repeated source load, two-byte store, range/arena failures, and control-flow shape. Require the scalar source cell to differ from the result descriptor cell.
- Advance the experimental target to `x86-64-wvb-baseline-v20` and ABI 20. Retain WVB 1.6, WVO 1.0, execution context 7, service table 5, all twelve service leaves, the 64-parameter convention, ABI 18's typed block-scoped physical map, the 2,048-cell frame ceiling, and all arena/value limits.
- Cover valid boundaries zero and 65,535 plus invalid 65,536 through the reference interpreter and native executor. Require interpreter, W^X JIT, and linked WVO/AOT agreement, deterministic fragments/objects, and fail-closed rejection of corrupt range constants, allocation widths, store widths, failure details, and scalar/descriptor aliases.
- Repeat exact compiler preflight after admission and record the next measured boundary without raising another limit in this decision.

## Evidence

The focused borrowed-bytes case passes on Windows. Interpreter, W^X JIT, and linked WVO/AOT retain result `42` while exercising both valid `u16` boundaries; the first invalid value becomes `WVR3016` under both the interpreter and native execution. Deterministic fragments and WVOs agree, and all targeted machine-shape corruptions fail before publication.

Exact compiler preflight clears `Bytesˉfromˉu16ˉlittle` and completes lowering and x86-64 selection. The resulting deterministic fragment is 4,556,121 bytes, so it reaches the existing `WVN2902` admission boundary against the 1,048,576-byte code limit. The diagnostic now records both exact sizes. This decision does not change that limit because no execution evidence yet distinguishes safe limit growth from needed baseline-code compaction or function-granular publication.

The Windows Development gate passes a zero-warning Release build, all 67 regular Seed tests, and all 25 OS tests. The complete Standard gate passes all 68 Seed tests, including the golden compiler-reproduction contract, and all 25 OS tests in 215.085 seconds.

All four pinned Windows QEMU 11.0/Q35/TCG Probe-32 scenarios retain their exact ABI-19-qualified identities and pass: normal `b8f0e656066b1e4f28edc4124eca6eea18130a0d6c0f4a9018e8ae817a0fa985` (531,456 bytes, exit 0), invalid opcode `0322ce3d3a9fecfa5c84809d8594f4f3ea643aaff2776f8d25668f1d723b9b54` (531,456 bytes, exit 3), general protection `1a0bd9f37c595d4170bd05fe83cc05dc344d2223674a01812f253ceb77893e40` (531,456 bytes, exit 3), and contained user fault `68319856b2913b3c857012d3fd38f147cf2a2307afacc9ffc8c8a33c005d0cf9` (531,968 bytes, exit 0). Debian and independent GitHub evidence remain pending before this decision becomes qualified.

## Consequences

The exact Windvale-written compiler now passes every operation-coverage boundary observed by native preflight. Its next blocker is whole-fragment size rather than unsupported WVB semantics, oversized frames, or call width.

The accepted native fragment language and generated-failure vocabulary both grow, so the ABI version advances even though the call convention, context layout, service table, WVB, and WVO format do not. The failed range path performs no arena allocation and cannot publish a partial descriptor.

This is not native compiler execution, permission to publish a 4.35 MiB executable fragment, a general byte builder, a standalone PE/ELF host container, a stable public ABI, or .NET retirement. C# remains the Stage 0 loader, native compiler/verifier, W^X owner, and recovery implementation.

## Reconsider when

- Exact compiler size analysis identifies dominant selector expansion and supports a bounded compaction plan.
- Function-granular JIT/AOT publication can retain complete verification while avoiding one monolithic code allocation.
- A measured host/OS image contract justifies a larger code ceiling with corresponding address, allocation, and hostile-input evidence.
- Another fixed-width encoder needs materially different allocation, byte-order, or trap machinery.
