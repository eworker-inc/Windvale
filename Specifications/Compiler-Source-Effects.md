# Windvale source effect analysis

## Status and boundary

`Compilerˉsourceˉeffects` implements exact declared and inferred effect analysis
over validated Language 1.0 source evidence and typed WVIR. It resolves source
identities to canonical language-effect bits or capability symbols, computes
transitive effects through direct function calls, and compares explicit clauses
with the exact result.

This is compiler-private analysis. WVEF does not enter WVB, grant authority, or
prove that an effect remains available at runtime. A capability requirement is
still separate from the application's grant and the launcher's rights-limited
provider binding.

## Exact identities

Language 1.0 currently assigns these canonical language-effect bits:

| Bit | Identity |
| ---: | --- |
| 0 | `memory.allocate` |
| 1 | `resource.acquire` |
| 2 | `resource.complete` |
| 3 | `resource.release` |
| 4 | `task.cancel` |
| 5 | `task.spawn` |
| 6 | `task.suspend` |
| 7 | `unsafe.address` |

Every other identity must resolve to an exact capability symbol declared by the
source set. Capability bits are assigned by canonical source-name ordering, not
symbol insertion order. A module may declare at most 32 distinct capabilities
in this checkpoint. Unknown and duplicate identities reject.

WVIR operations 171, 172, and 175 contribute `memory.allocate`; operation 174
contributes `resource.release`. Operation 62 propagates the exact effect set of
its direct function target. Operation 63 contributes the canonical capability
target. Other current operations are effect-free unless a later version adds an
explicit mapping.

An exported function must have an explicit `effects(...)` clause. An explicit
clause is valid only when its language and capability masks exactly equal the
direct and transitive result: missing and extra effects both reject. Private
functions may omit a clause and receive the inferred exact set.

## WVEF 1.0 directory

Successful analysis publishes one bounded WVEF directory:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | ASCII `WVEF` |
| 4 | 2 | major version 1 |
| 6 | 2 | minor version 0 |
| 8 | 4 | function-entry count |
| 12 | 4 | function-entry size, exactly 12 |
| 16 | 4 | capability-entry count |
| 20 | 4 | capability-entry size, exactly 4 |
| 24 | 4 | function-entry offset, exactly 32 |
| 28 | 4 | capability-entry offset |

Each function entry contains 4-byte flags, a 4-byte language-effect mask, and a
4-byte capability-effect mask. Flag bit 0 marks a function and bit 1 marks an
explicit clause; all other flag bits reject. Each capability entry is the WVSD
symbol index at its canonical rank.

The analyzer admits at most 87,380 function entries and 32 capability entries,
and retains at most 4 MiB of typed WVIR input. All lengths, offsets, counts,
entry widths, target indices, masks, and directory aggregates are checked before
dependent reads. Public WVEF validation checks the same count limits before
performing size arithmetic.

Transitive propagation has an exact ceiling of 256 complete passes. A graph
that still changes after that ceiling reports `Evidenceˉlimit`; the compiler
does not continue an input-sized fixed point indefinitely. This deliberately
bounds development and verification time. A future work-list or strongly
connected-component implementation may lift the depth limit only while
preserving exact results and bounded resource use.

## Focused evidence

The maintained self-test covers exact empty and nonempty clauses, real binding
evidence, direct language and capability effects, allocation operations,
missing and extra effects, local and transitive inference, exported-clause
requirements, duplicate and unknown identities, recursive fixed points, and
valid versus oversized WVEF directory counts. It is part of the focused
callable-semantics owner; paired-host execution remains a separate qualification
claim.

Closure declarations currently resolve their declared effect identities with
this registry. Comparing those declarations with an executable lowered closure
body remains part of the later callable WVIR/WVB integration checkpoint.
