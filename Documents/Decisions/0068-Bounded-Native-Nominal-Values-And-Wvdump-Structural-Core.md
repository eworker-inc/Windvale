# Decision 0068: Bounded native nominal values and the wvdump structural core

- Date: 2026-08-01
- Status: Implemented candidate; cross-host and pinned-QEMU qualification pending
- Refines: [Decision 0067](0067-Borrowed-Hosted-Input-And-First-Native-Wvb-Inspector.md)'s ABI-8 hosted input boundary
- Advances: The existing Windvale-written `wvdump` structural parser through JIT and WVO/AOT

## Context

ABI 8 could read a real WVB but could only run the small header inspector natively. The existing `Wv-Dump-Core.wv` parser represents status with enums and returns immutable inspection records throughout its envelope, section, instruction, and payload scanners. Rejecting all nominal metadata therefore kept the useful structural parser on the reference interpreter even though its byte operations, control flow, and hosted input were otherwise native-eligible.

Native records need an explicit lifetime and failure boundary. Reusing host or managed objects would make generated code depend on .NET and would weaken independent fragment verification. A general heap, tracing collector, descriptor-return ABI, or compatibility reader is not justified by immutable parser results.

## Decision

- Advance the experimental target to `x86-64-wvb-baseline-v9` and native ABI version 9. ABI-8 artifacts remain historical evidence and are not accepted through a compatibility branch.
- Admit bounded enum metadata when its first member has value zero. Enums use their canonical signed 32-bit member value in the low dword of the existing 16-byte value cell. Equality and inequality preserve nominal identity during verified WVB lowering.
- Admit non-empty immutable records whose fields are native scalars, enums, or records. A record value is a 32-bit offset into one execution-owned arena. Every field occupies one complete 16-byte value cell, so construction and field access copy the existing typed cell without introducing a second representation.
- Give each host execution one fixed 1 MiB record arena. Construction bump-allocates only after checked addition and capacity comparison. Exhaustion returns packed status 7 and becomes `WVR3017`; the arena and all record values expire together after `Main` returns. No individual free, mutation, host object, or garbage collector is introduced.
- Advance the execution context to version 2 and 48 bytes by adding record-arena pointer, length, and used fields. The execution owner initializes all three. A service-free OS bridge supplies a zero-length arena because its current module creates no records.
- Advance the runtime-service table to version 3 and 48 bytes with one pure `Textˉutf8ˉisˉvalid` entry. The operation accepts only an independently proven borrowed-byte descriptor and requires no capability authorization. Stage 0 supplies exact Windows/System V thunks and strict UTF-8 validation; a future native runtime may implement the same closed service without changing generated code.
- Extend native machine IR, selection, and independent decoding over enum constants/comparisons, record creation, record field access, arena overflow/capacity edges, full-cell copies, the new context fields, and the pure UTF-8 service load. Corrupt allocation sizes, field offsets, service loads, or branch targets fail before WVO serialization or W^X publication.
- Refine `Scanˉshape` in `Wv-Dump-Core.wv` from five parameters to four by replacing two booleans with one bounded policy value. This preserves parser semantics and fits the existing four-register internal call convention rather than expanding the ABI for one function.
- Differentially execute the existing structural portion of `Wv-Dump-Core.wv` through the reference interpreter, real W^X JIT, and linked WVO/AOT image. This is the actual envelope and payload parser through `Inspectˉwvbˉpayloads`, not a separately reimplemented demonstration.
- Advance the kernel native bridge to version 4 and firmware probe identity to version 11 because the service-free OS AOT consumer is rebuilt through ABI 9 and context version 2. The guest still receives no runtime-service table and performs no record allocation.
- Retain C#/.NET as Stage 0 selector, independent decoder, Windows/Linux adapter, semantic oracle, recovery implementation, and host image builder. The qualified retirement gates in Decision 0057 remain unchanged.

## Candidate evidence

On Windows x64, the focused enum/record differential test passes under the reference interpreter, W^X JIT, and linked WVO/AOT. It covers enum equality/inequality, record construction and field access, record parameters and returns, deterministic code, verifier corruption rejection, the required zero enum default, and deterministic 1 MiB arena exhaustion as `WVR3017`.

The native structural-wvdump test derives the existing parser portion directly from checked-in `Wv-Dump-Core.wv`. It validates the complete canonical fixture envelope and all seven payload sections, including strict UTF-8, and returns 42 identically under interpreter, JIT, and linked AOT. The full 54-test Seed suite passes in 196.743 seconds, including compiler self-reproduction and all golden closure phases. All 15 focused OS tests pass with the version-11 image. Exact Windows/Debian qualification and pinned-QEMU evidence will replace this candidate paragraph before the decision is marked cross-host qualified.

## Consequences

Windvale enums and immutable records now cross native calls without a managed object model, and the substantive WVB parser inside `wvdump` runs as native code. The remaining full-tool gap is narrower and explicit: dynamic text construction/formatting, descriptor returns, void-return calls, and diagnostic output.

The record arena is deliberately execution-scoped and monotonic. It is suitable for bounded parser and compiler-phase values, not long-lived mutable application graphs. A later general allocator may coexist with or replace it only through a new versioned ABI and measured ownership requirements.

The pure UTF-8 service is runtime support rather than ambient authority. Its table entry is versioned and independently decoded, but the current callback is still implemented by the C# Stage 0 host. Native-runtime replacement remains required before .NET retirement.

## Reconsider when

- Records must outlive one run, mutate in place, or participate in cyclic graphs.
- Text/bytes/record descriptors need to return from native functions through one general value ABI.
- A native runtime implements UTF-8 validation inline or through a stable non-managed runtime library.
- More than four internal parameters are common enough to justify a specified stack-argument convention.
