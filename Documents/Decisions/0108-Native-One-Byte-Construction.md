# Decision 0108: Native one-byte construction

- Date: 2026-08-02
- Status: Implemented with Windows Standard and pinned-QEMU evidence; cross-host qualification pending
- Advances: Native ABI 19
- Refines: [Decision 0066](0066-Borrowed-Bytes-And-Unsigned-Native-Values.md) and [Decision 0105](0105-Typed-Block-Scoped-Native-Value-Slots.md)

## Context

Qualified ABI 18 removed the exact Windvale compiler's slot-2,049 blocker without raising the 2,048-cell physical frame ceiling. Repeating exact preflight then reached the first unsupported operation in `Compilerˉcompileˉsourceˉwvb`: `Bytesˉfromˉu8`.

The source language, WVB 1.6 verifier, and reference runtime already define this operation as construction of one immutable byte from the complete unsigned `u8` range. The shared native backend already owns bounded byte descriptors, the execution-scoped text/byte arena, byte reads, slicing, concatenation, and four-byte little-endian construction. The missing boundary was therefore one native machine-IR operation and one independently recognized machine-code shape, not a new source or runtime semantic.

## Decision

- Add typed `Nativeˉbytesˉfromˉu8(Result, Value)` machine IR. Lower verified `Bytesˉfromˉu8` only from `U8` to `Borrowedˉbytes`; independently reconstruct those exact operand/result types before selection.
- Allocate exactly one byte from the existing execution-owned 16 MiB text/byte arena. Check cursor addition and capacity before publication, write a pointer/length descriptor with length one, then store only the source's low byte.
- Reuse the established `WVR3018` text-arena-exhaustion boundary. The operation adds no service-table slot, host callback, allocator, or escaping lifetime.
- Extend the byte-level fragment verifier with the exact checked-allocation, descriptor, scalar-load, one-byte-store, failure, and control-flow shape. Require the scalar source cell to differ from the result descriptor cell.
- Apply the same source/result non-alias rule to the retained four-byte little-endian encoder. Its emitter stores the result descriptor before reloading the scalar, so an independently supplied aliased shape must fail closed even though the canonical typed slot map cannot produce it.
- Advance the experimental target to `x86-64-wvb-baseline-v19` and ABI 19. Retain WVB 1.6, WVO 1.0, execution context 7, service table 5, all twelve service leaves, the 64-parameter convention, ABI 18's typed block-scoped physical map, the 2,048-cell frame ceiling, and all arena/value limits.
- Cover both `0u8` and `255u8` through the reference interpreter, W^X JIT, and linked WVO/AOT; require deterministic machine fragments; and reject corrupt allocation widths, store widths, and scalar/descriptor aliases.
- Repeat exact compiler preflight after the operation is admitted. Record the next observed contract rather than expanding this decision speculatively.

## Initial evidence

The focused borrowed-bytes case passes interpreter/JIT/WVO differential execution to result `42`, exercises both ends of the `u8` range, and rejects all targeted corruptions. Exact compiler preflight clears `Bytesˉfromˉu8` and now stops in the same `Compilerˉcompileˉsourceˉwvb` function at unsupported `Bytesˉfromˉu16ˉlittle`.

The Windows Development gate passes a zero-warning Release build, all 67 regular Seed tests, and all 25 OS tests. The complete local Standard gate passes all 68 Seed tests, including the 171.677-second golden contract, and all 25 OS tests; the Seed suite takes 228.705 seconds and the complete command takes 243.5 seconds.

All four pinned Windows QEMU 11.0/Q35/TCG Probe-32 scenarios retain their exact ABI-18-qualified image identities and pass: normal `b8f0e656066b1e4f28edc4124eca6eea18130a0d6c0f4a9018e8ae817a0fa985` (531,456 bytes, exit 0), invalid opcode `0322ce3d3a9fecfa5c84809d8594f4f3ea643aaff2776f8d25668f1d723b9b54` (531,456 bytes, exit 3), general protection `1a0bd9f37c595d4170bd05fe83cc05dc344d2223674a01812f253ceb77893e40` (531,456 bytes, exit 3), and contained user fault `68319856b2913b3c857012d3fd38f147cf2a2307afacc9ffc8c8a33c005d0cf9` (531,968 bytes, exit 0). This confirms that the new backend shape does not perturb the retained OS workload.

Cross-host Windows/Debian qualification and an exact implementation commit are still required before ABI 19 replaces ABI 18 as the latest qualified baseline.

## Consequences

The exact Windvale-written compiler advances through a real byte-construction dependency using the same verified native backend as host tools and Windvale OS. No parallel compiler, C backend, managed callback, or new lifetime model is introduced.

The accepted native fragment language grows by one exact encoder shape, so the ABI version advances even though the call convention and serialized WVO format do not. ABI 18's compact frame work remains intact and Probe 32 stays byte-for-byte unchanged.

This is not native compiler execution, support for `Bytesˉfromˉu16ˉlittle`, a general byte builder, a standalone PE/ELF host container, a stable public ABI, or .NET retirement. C# remains the Stage 0 loader, native compiler/verifier, W^X owner, and recovery implementation.

## Reconsider when

- A later fixed-width encoder needs materially different allocation, byte-order, or verification machinery instead of the same bounded pattern.
- Representative compiler execution shows that per-value arena allocation creates unacceptable space pressure and justifies a measured builder or region contract.
- A future WVO revision serializes the ABI and service requirements needed for independent fragment loading.
- Native compiler execution reaches a lifetime or ownership boundary that the execution-scoped byte arena cannot safely express.
