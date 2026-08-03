# Decision 0117: Nominal native record-storage plan

- Date: 2026-08-02
- Status: Qualified at exact implementation commit `57416d0f93c803ef4218b6c0206798f2fd4f362c`
- Retains: Native ABI 20, execution-context version 7, target `x86-64-wvb-baseline-v20`, the 2,048-cell physical frame ceiling, and the 2 MiB host record arena
- Refines: [Decision 0105](0105-Typed-Block-Scoped-Native-Value-Slots.md), [Decision 0112](0112-Bounded-Exact-Compiler-Record-Arena.md), and [Decision 0115](0115-Exact-Compiler-Record-Lifetime-Pressure.md)

## Context

Decision 0115 showed that monotonic immutable-record allocation is not a viable full-bootstrap contract. The successful reference execution constructs at least 77,821,091 record fields, which would require more than 1.24 GB under ABI 20 even though most values are short-lived compiler intermediates.

The native machine IR could not safely plan reuse because it retained only broad record and enum kinds. It did not preserve nominal identity for every parameter, return, local, semantic value, and call edge, so a later phase could neither derive the correct field width nor prove that a copy preserved the same record type.

The exact compiler also needed to answer a more useful question than total allocation: how much record backing must exist at the same time in one native invocation? Its bytecode operand stack is empty at every basic-block edge, its records are immutable, and its local/value identities are explicit. Those properties permit bounded static storage analysis without introducing a general collector.

## Decision

- Preserve the nominal declaration index alongside the native physical kind for every function return, parameter/local binding, and semantic value. Non-nominal values use `-1`.
- Independently validate those identities before selection. Record identities must name record declarations, enum identities must name enum declarations, and constants, loads, stores, constructors, field reads, calls, comparisons, and returns must preserve the required identity.
- Keep nominal metadata in native machine IR only. It does not alter WVB, WVO, native fragments, selected x86-64 bytes, ABI 20, or runtime behavior.
- Add a deterministic Stage 0 record-storage planner. It measures an implementation candidate but does not change code generation:
  - unchanged record parameters borrow caller-owned immutable storage; a parameter that is assigned requires local backing;
  - record locals use control-flow liveness and a deterministic width-first, local-index-tied interference allocation for reusable persistent backing;
  - record semantic results use definition-to-last-use liveness within each basic block, allocating a result before releasing operands consumed by the same operation;
  - record-returning functions reserve one hidden destination-pointer cell, while the caller owns the returned field storage;
  - existing 16-byte local/value handle cells remain present, and backing uses one 16-byte cell per direct field.
- Define projected frame cells as existing ABI-20 frame cells plus persistent record-local backing, peak live record-result backing, and one hidden cell for a record return.
- Retain the coarse physical-slot record bound as comparison evidence. The liveness result, not the sum of every record-shaped physical slot, is the candidate reusable scratch requirement.
- Detect record-valued fields explicitly. Direct field counts are sufficient for the exact compiler because none of its 49 records contains another record; this does not admit nested records to a reclaiming ABI.
- Use these results as the entry evidence for ABI 21. ABI 21 should implement frame-owned storage only after the planner also publishes exact deterministic offsets and the selector/independent decoder can prove copies, calls, early returns, traps, and bounds. This decision does not implement or qualify ABI 21.

## Exact-compiler evidence

The canonical 328-function compiler produces the following deterministic machine-IR inventory:

| Measure | Exact result |
| --- | ---: |
| Functions involving records | 253 |
| Record parameters | 308 |
| Assigned record parameters | 0 |
| Record local bindings | 8,097 |
| Declared record-local field cells | 137,512 |
| Liveness-allocated persistent field cells, summed across functions | 9,291 |
| Record semantic value identifiers | 14,948 |
| Existing record-shaped physical value slots | 4,484 |
| Coarse physical-slot field cells, summed across functions | 88,669 |
| Peak-live semantic-result field cells, summed across functions | 7,463 |
| Record-returning functions | 206 |
| Record-return field widths, summed across functions | 3,222 |
| Widest record | 34 fields |
| Functions containing nested record fields | 0 |

The largest projected frame is `Compilerˉsourceˉwirˉcompileˉblock` at 1,489 cells, or 23,824 bytes. Its existing frame is 1,178 cells; liveness reduces 13,512 declared local-field cells to 196 persistent cells, peak live semantic results require 114 cells, and its record return adds one hidden destination cell. The result remains below the unchanged 2,048-cell/32 KiB physical frame ceiling.

These sums describe the complete static inventory, not simultaneous process memory. The per-function maximum is the relevant invocation-frame bound; the existing native depth and stack contracts continue to bound simultaneously active calls.

The exact selected fragment remains 4,556,121 bytes with SHA-256 `8e74707df03a535e3ef68cfcfc8da6fa68fda29ccf4344e272fc50c8a5845bab`. The focused nominal test independently compares every retained shape with verified WVB and pins a four-function storage plan. The exact-compiler test pins every aggregate above, the largest function, the frame equation, deterministic native bytes, and independent fragment verification.

Windows Development verification completes a zero-warning Release build, all 69 regular Seed tests, and all 25 bounded OS tests. The Seed suite takes 61.331 seconds; the complete command takes 79.7 seconds.

Exact implementation commit `57416d0f93c803ef4218b6c0206798f2fd4f362c` passes GitHub [Verify run 30773327094](https://github.com/eworker-inc/Windvale/actions/runs/30773327094). Windows and digest-pinned Debian 12 each complete a zero-warning Release build, all 70 Seed tests, all 25 OS tests, and the complete native CLI gate. The focused nominal/storage case takes 52 ms on Windows and 32 ms on Linux; the exact native compiler-plan case takes 1.933 and 2.014 seconds; the retained full-bootstrap boundary takes 737 and 669 ms. Windows Seed takes 225.610 seconds with a 167.252-second golden contract; Linux Seed takes 199.654 seconds with a 146.882-second golden contract. The complete host jobs finish in 8m32s and 7m25s. QEMU is not rerun because ABI 20, every generated machine byte, and all OS source/artifact inputs remain unchanged.

## Consequences

The full native-bootstrap failure now has a bounded replacement direction that fits the existing frame envelope. The evidence rejects another monotonic-arena increase and also shows that a general garbage collector is not required for this exact direct-record workload.

The planner remains Stage 0 evidence. It neither changes the current `WVR3017` boundary nor makes native Stage 1 to Stage 2 reproduction pass. ABI 20 continues to allocate records monotonically in the execution-owned arena.

A reclaiming implementation must retain record handles while separating their storage classes: borrowed parameter storage, liveness-reused persistent local backing, block-local result scratch, and caller-owned result destinations. A record local load must preserve value semantics even if the local is overwritten while the loaded value remains live.

[Decision 0118](0118-Deterministic-Native-Record-Storage-Offsets.md) implements the next required planning seam: exact absolute frame-cell maps plus independent lifetime/overlap reconstruction. It retains ABI 20; selector and decoder adoption remain the next boundary.

Nested records remain outside the proposed first ABI-21 admission because copying one direct field cell would retain a pointer into storage whose lifetime may end. They require a recursive layout/copy contract or a separately owned representation.

## Reconsider when

- Exact offset planning cannot retain deterministic selected bytes or the 2,048-cell frame ceiling.
- Independent decoding cannot distinguish compiler-derived frame pointers from forged or escaping record handles.
- Assigned record parameters, recursion, or a new compiler shape invalidates the measured bounds.
- Nested records become necessary for the native compiler or another required tool.
- Measured text, bytes, WVO, linker, publication, or native-stack pressure becomes the next full-bootstrap boundary.
