# Decision 0141: Exact-compiler dynamic-value lifetime

- Date: 2026-08-03
- Status: Implemented with local Windows evidence; cross-host qualification pending
- Retains: Native ABI 21, execution-context version 7, target `x86-64-wvb-baseline-v21`, the 16 MiB dynamic-value arena, and immutable `text`/`bytes` semantics
- Refines: [Decision 0136](0136-Exact-Compiler-Dynamic-Value-Pressure.md) and [Decision 0137](0137-Bounded-Owned-Values-Before-Dynamic-Collections.md)

## Context

Decision 0136 measures 902,262,268 bytes of flat allocation-bearing results during the successful canonical compiler bootstrap. That cumulative value proves the existing monotonic arena cannot work, but it does not state how many backings must coexist. Choosing a larger arena, a collector, a free list, or a chunked representation from cumulative allocation alone would confuse construction traffic with retained storage.

Windvale values are immutable and may share backing through copies, slices, UTF-8 views, calls, and direct-record fields. The exact native compiler also returns descriptor-bearing records whose payloads replace earlier caller values. A useful lifetime model therefore has to preserve backing identity through all of those paths and count unique live backings rather than descriptor copies.

## Decision

- Add an opt-in dynamic-backing lifetime tracker to the Stage 0 reference runtime. `Collectˉdynamicˉvalueˉlifetime` allocates profiler-only backing identities; ordinary runs allocate no tracker or backing metadata.
- Treat each allocation-bearing format, concatenation, quote, enum-name, and byte-construction result as one conceptual contiguous backing with its exact UTF-8 or byte length. The WVB 1.7 `i64` and `u64` format operations are tracked for completeness, although ABI 21 and the canonical compiler remain on WVB 1.6.
- Preserve identity through `bytes.slice`, `text.from_utf8`, `text.to_utf8`, operand/local copies, calls, returns, and direct or nested record fields. Static constants, process arguments, and host snapshots remain external borrowed storage and do not count against the native dynamic arena.
- Count roots in verified operand stacks and local frames. Argument transfer moves existing roots into the callee; local replacement releases the previous root; function return releases the callee frame before the caller installs the returned value. A bytecode local remains a root until it is overwritten or its frame returns, even when source-level use has ended.
- Count each backing once while any typed root reaches it. Record roots contain the unique union of their fields' backing identities, so aliases do not multiply live bytes.
- Report both peak live backing storage and peak allocation-operation storage. The latter includes live backings, input backings that were popped immediately before an allocating operation, and the new flat result, thereby retaining the required input/output copy overlap.
- Require every successful or failed run to balance all tracked roots. A nonzero retained count is reported, and starting another run with retained roots fails the profiler invariant rather than resetting away an ownership error.
- Add CLI option `--report-dynamic-lifetime`. It emits one deterministic summary after success or failure. The normal CLI and the existing per-function aggregate profiler remain unchanged unless their options are selected.
- Keep the 16 MiB native arena and ABI 21 unchanged in this evidence slice. Ideal peak liveness proves a capacity lower bound, not that an unimplemented allocator avoids fragmentation or that generated retain/release operations are correct.
- Evaluate a concrete bounded reclaiming policy next. It must replay the exact allocation/release trace, account for metadata, alignment, coalescing and fragmentation, preserve checked per-value limits, and be independently verified. Ownership-aware reuse or a chunked builder remains a fallback if a simple policy cannot fit the retained arena safely.

## Local evidence

The canonical 12-module Stage 1 compiler again executes exactly 6,700,562,174 verified instructions, returns zero, emits the established valid-source report, and produces the byte-identical 599,868-byte Stage 2 compiler with SHA-256 `9673bf3331763181f443ec67b7a513bc66daa718969f7f6b0d197a4186071066`.

The lifetime tracker sees the same 1,852,773 constructed values and 902,262,268 constructed bytes as Decision 0136, but only 17 unique backings and 9,030,829 bytes at the live peak. The allocation-operation peak is the same 17 backings and 9,030,829 bytes and occurs in `Compilerˉsourceˉwirˉdirectory` during `bytes.concat` (function index 211). All roots balance to zero at completion.

The measured ideal peak occupies approximately 53.83% of the retained 16,777,216-byte arena, leaving 7,746,387 bytes, approximately 7.39 MiB, before allocator metadata or fragmentation. This rules out a capacity increase as the first repair and makes bounded reclamation the leading candidate. It does not yet prove a concrete allocator or native execution.

Two local Windows Release runs reproduce the exact result and artifact identity in 512.450 and 517.510 seconds; the second captures the deterministic lifetime report. The bounded Foundation byte-construction CLI sample constructs 8,388,672 bytes but peaks at 6,291,475 bytes across five backings and retains zero. The focused dynamic profiler and lifetime tests pass after a zero-warning build; the warm lifetime case completes in 13 milliseconds.

The candidate then fast-forwards over Decisions 0137 through 0139. Their WVB 1.7 and WebAssembly additions do not execute in the canonical WVB 1.6 bootstrap profile. After integration, change-aware Windows verification completes a zero-warning Release build and passes all 80 selected Seed tests in 314.943 suite seconds, including the 219.181-second golden compiler contract and the three-millisecond warm lifetime case. The multi-billion-instruction lifetime profile is not repeated after that additive integration.

The focused test covers disabled behavior, slices sharing backing, record-held aliases, caller/callee transfer, frame-local retention, allocation copy overlap, zero retained roots, and reset rather than accumulation across repeated runs. The Windows and Linux CLI verifiers require the exact bounded Foundation report. No WVB/WVO bytes, source semantics, native ABI, generated machine bytes, OS source, or guest artifact changes in this measurement slice. QEMU is not rerun because no OS input changed.

## Consequences

Windvale does not need to accept a 902 MiB arena or select a general garbage collector to clear this compiler blocker. The evidence supports an explicit bounded ownership/reclamation mechanism consistent with Decision 0137, while keeping immutable values and shared backing observable only through value semantics.

The 16 MiB capacity has meaningful measured headroom, but fragmentation and tracking costs are still open. The next native slice must prove an allocator policy from the exact trace before it changes the execution context, service table, generated ownership operations, or failure codes. The implementation must be shared runtime machinery rather than a compiler-name or function-name special case.

This decision does not qualify native Stage 1-to-Stage 2 reproduction, select permanent collection semantics, add dynamic collection syntax, advance WVB 1.7 into the native backend, retire .NET, or advance the OS guest ABI.

## Reconsider when

- A concrete allocator trace exceeds 16 MiB after metadata, alignment, and fragmentation.
- Generated ownership operations cannot preserve aliases through direct records and capability boundaries without observable mutation or leaks.
- Builders or chunked values materially reduce both copied bytes and required native complexity while preserving bounded reads, slices, publication, and failure behavior.
- Closures, concurrency, globals, cyclic values, or long-lived resource graphs require tracing rather than acyclic root accounting.
