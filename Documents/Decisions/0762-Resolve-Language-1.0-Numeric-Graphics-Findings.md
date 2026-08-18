# Decision 0762: Resolve the Language 1.0 numeric/graphics findings

## Status

Accepted by the project owner on 2026-08-17 under the instruction to integrate
all recommended correctness/completeness findings needed for a correct Language
1.0. This decision refines
[Decision 0751](0751-Accept-Windvale-Language-1.0-Direction.md) and the
normative-candidate language, grammar, and Foundation companions.

It accepts all six findings from workload 8. It does not freeze edition 1,
implement floating point, promise GPU reproducibility outside a named profile,
or introduce a graphics API.

## Context

The eighth mandatory workload processes eight independent f32 lanes from fixed
bit fixtures, publishes a bounded immutable vector, validates special values and
conversions, and creates one canonical text report. Its first fused lane differs
from ordinary multiply-then-add, so contraction mistakes are observable. Other
lanes cover subnormal preservation, finite overflow, infinity, canonical NaN,
and both signed zeros.

The candidate already chose strict IEEE values, explicit conversions, arrays,
vectors, slices, generics, and bounded builders. Complete source exposed that
array construction, generic range borrowing, mutable slice replacement, exact
strict-float calls, and canonical f32 formatting were still described but not
spelled as usable signatures/grammar.

## Decision

### Admit contextual fixed-array literals

Add `[E0, E1, ...]` as a primary expression only under one exact expected
`Array<T, N>` type. It supplies exactly `N` exact-`T` values, evaluates left to
right once, and performs no common-type selection, conversion, dynamic
allocation, omission, or repetition. Empty `[]` requires expected `N = 0`.

This is construction syntax for fixed values, not a general collection literal.

### Complete checked slice creation and exclusive replacement

Accept `Sliceˉfailure`, `Arrayˉslice`, `Vectorˉslice`,
`Vectorˉsliceˉmut`, `Mutableˉsliceˉlength`, and
`Mutableˉsliceˉreplace`. Every range checks addition and owner length before
publishing a borrow. A mutable view exclusively borrows its vector, and checked
replacement returns the old owned element while accepting the new element once.

Do not add pointer arithmetic, unchecked indexing, overlapping mutable views,
or hidden vector resizing.

### Fix the strict f32 operation/observation subset

Accept exact bit conversion, classification, bitwise equality, IEEE total
ordering, and explicitly fused multiply-add names. Ordinary `A * B + C` remains
two rounded operations. Arithmetic NaN results canonicalize to `0x7fc00000`;
bit reinterpretation alone preserves arbitrary payloads. Subnormals are not
flushed and signed zero remains observable through named bit/total-order calls.

This surface is general numeric Foundation, not graphics or accelerator syntax.

### Keep conversions policy-bearing and complete the generated matrix

Accept the workload's exact u32→f32 nearest/exact, f32→i32 truncate,
f32→f64 widen, and f64→f32 nearest/exact calls. Names state direction and
rounding/exactness. Failures distinguish NaN, infinity, range, and inexactness.

There is no general `cast`, implicit widening, result-context conversion, or
host conversion. Before source freeze, Foundation must publish the complete
required generated matrix using the same rules; the workload subset is not a
license to leave other required pairs ambiguous.

### Require canonical invariant f32 reporting

Accept fixed lowercase u32 hex and canonical f32 text-builder appends. The f32
form uses `nan`, `inf`, `-inf`, `0`, `-0`, or the bounded shortest decimal that
round-trips under nearest-even with exact tie rules. Every append is
all-or-nothing, locale-free, allocation-free after builder construction, and
bounded by 24 bytes for one f32.

### Keep parallelism outside scalar semantics

The sequential lane loop is the correctness oracle. A library may execute
proved-disjoint lanes in parallel only when it preserves each lane's operation
order and publishes identical ordered bits. Reassociation-sensitive reductions
need separately named order/accuracy contracts.

Do not add a `parallel` keyword, implicit task creation, automatic fast math, or
target-selected numeric semantics for this workload.

## Consequences

The numeric/graphics bundle becomes draft reviewed. Nine of eleven workloads
are now draft reviewed; package/deterministic-map and System/FFI remain.

The grammar gains one contextual fixed-array literal. Foundation gains one
slice-failure variant, five range/mutable-slice operations, one floating-class
enum, thirteen strict numeric calls, and two formatting calls. These are candidate
contracts; current Seed tools do not implement them.

The exact reference has eight lanes, 32 fixed input/expected bit values, two
4,096-byte child budgets, 18 boundary/conversion checks, and one 328-byte report
with SHA-256
`25f308384b0a6ad088039cb3a65f5cf6eb928148b0f2cc9b18b2e2ca7c6ead2a`.

## Reconsideration triggers

Reconsider array literal spelling only if parser/type implementation cannot keep
the expected type exact and diagnostics bounded. Do not replace it with inferred
heterogeneous or dynamically allocated literal semantics.

Reconsider formatting only with a complete alternative algorithm, exhaustive
cross-host golden evidence, and the same round-trip/size guarantees.

Reconsider parallel policy only for a named deterministic algorithm whose
grouping, precision, and publication rules are explicit. Performance alone does
not authorize source-result drift.
