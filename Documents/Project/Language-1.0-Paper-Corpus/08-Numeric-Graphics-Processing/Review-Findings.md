# Workload 8 review findings

## Status

First-author review is complete. The project owner authorized direct acceptance
of all recommended correctness/completeness findings on 2026-08-17; all six
findings are accepted under
[Decision 0762](../../../Decisions/0762-Resolve-Language-1.0-Numeric-Graphics-Findings.md).
They are normative-candidate/source-freeze inputs, not implementation or final
freeze claims.

## Finding 1: fixed arrays need a construction expression

The candidate specified `Array<T,N>` behavior but grammar had no array literal
or complete constructor. Workload source otherwise had to disguise fixed values
as package bytes or dynamically allocate a vector.

Accept contextual `[E0, ...]` only under exact expected `Array<T,N>`. This keeps
type/count diagnostics local, avoids common-type inference, and adds no dynamic
collection or repetition semantics.

## Finding 2: generic slices need exact creation/mutation calls

The candidate described vector/array slices and mutable views but fixed only
immutable observation calls. Accept checked array/vector range creation,
exclusive vector slicing, mutable length, and replacing one element with exact
ownership return. This completes the safe algorithm surface without pointers or
unchecked indexing.

## Finding 3: strict floating behavior needs callable names

The semantic profile described FMA, classification, bit equality, and total
order but exposed only one bit-construction call. Accept the exact f32 workload
subset. Lane zero proves contraction is observable; special lanes prove NaN,
infinity, signed-zero, and subnormal rules.

## Finding 4: conversions remain a generated named matrix

Accept the exact nearest/exact/truncate/widen/narrow calls used by the workload.
Do not add `cast`, implicit conversion, or overload selection. The complete
required matrix remains a Foundation source-freeze gate and must use the same
policy/failure naming.

## Finding 5: numeric reports need canonical builder operations

Accept fixed lowercase u32 hex and bounded shortest-round-trip f32 appends with
exact special spellings/tie rules. Interpolation or host formatting would hide
capacity and portability. The exact 328-byte report is a cross-host oracle.

## Finding 6: parallelism must not alter scalar semantics

The independent lanes need no task syntax to prove Language 1.0. Accept the
sequential loop as the correctness oracle. Future bounded parallel libraries may
split disjoint ranges only with identical ordered result bits. Reductions need a
separate named contract; no implicit reassociation or fast math is allowed.

## Quantitative record

| Measure | Recorded value |
| --- | --- |
| Source | 6 files; 851 lines / 26,518 UTF-8 bytes; 33 top-level declarations; largest 268 lines. |
| Fixed data | 4 arrays / 32 u32 words / 128 semantic bytes. |
| Execution | 8 FMA lanes / 8 replacements / 1 immutable sequence. |
| Audit | 8 output comparisons plus 18 boundary/conversion checks. |
| Memory | 8,192-byte root split into two 4,096-byte children. |
| Report | 328 UTF-8 bytes / 12 LF-terminated lines / one SHA-256. |
| Failure surface | 66 named compile/slice/numeric/format/target cases. |
| New general surface | 1 grammar form, 2 variants/enums, 20 Foundation functions; no capability/unsafe form. |

## Owner resolution

The owner accepted all six recommendations. Workload 8 is draft reviewed. The
grammar, semantic, and Foundation candidates carry the corresponding rules.
No current compiler/runtime support, performance result, graphics subsystem, or
source-freeze claim follows.
