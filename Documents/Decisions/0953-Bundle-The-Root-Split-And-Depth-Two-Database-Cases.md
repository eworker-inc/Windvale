# Decision 0953: bundle the root-split and depth-two database cases

- Date: 2026-09-04
- Status: Implemented candidate with focused Windows execution evidence; Linux
  syntax verification passed and Linux execution remains pending
- Extends: [Decision 0952](0952-Rank-And-Trial-Overlapping-Database-Products.md)
- Preserves: both logical case labels, existing capacity limits, exact partial
  development selection, and fresh qualification

## Context

The overlap ranking identified root split and depth-two upsert as a second
non-identical pair with a smaller union than the accepted transaction leaf-
groups/pages bundle. The projects repeat nine declared inputs and differ in one
case root plus one library implementation each. Their state is process-local,
their profiles and authority are equal, and neither case requires host storage.

## Decision

1. Add one portable Project 2 bundle whose root invokes the existing root-split
   and depth-two self-test modules with distinct failure return codes.
2. Order dependency modules by their declared UTF-8 module identities and keep
   the exact union of the two original source closures.
3. Replace the two qualification construction steps with the bundle while
   retaining `RootSplit` and `DepthTwo` as separate logical cases.
4. Bind both development cases to the same bundle. Coalesce only when both are
   selected; preserve their original one-case projects for partial selection.
5. Keep every compiler, lowerer, WVO, linker, package, and execution bound
   unchanged.

## Evidence

The original projects contain 22 declared source visits. They share nine; their
union contains 13 existing source paths and 208,926 bytes, for a 40.90-percent
declaration-visit reduction before the small dispatch root is added.

Focused Windows qualification compiled, lowered, admitted, linked, packaged,
and ran both cases in 59,990 ms. Development checkpoint creation took 58,470 ms,
and an unchanged run completed in 2,860 ms with project, link, and hosted-
application hits.

After merging the qualified compiler Slice 8 state from `origin/main`, the
changed-file verification contract passed all 31 general and 279 native static
cases, including exact bundle routing, full and partial development plans, and
the 54-step qualification graph.

The database qualification graph now contains 54 executions and 57 logical
cases. It has 58 project references across 57 unique manifests and 673 declared
root/source references across 146 unique paths. Full database development has
53 behaviors in 47 executions across six bundles. Complete database and paired-
host qualification were not run.

## Consequences

- One additional complete portable construction is removed from qualification
  and from a full development selection.
- Both the cold focused run and unchanged warm run remain below the current
  60-second and 5-second small-closure development targets on the measured host.
- The next overlap candidate still requires an independent bounded trial; this
  success does not establish a source-byte-only capacity rule.

## Reconsideration triggers

Split the bundle if the cases acquire different profiles, capabilities, state,
limits, or failure-isolation needs. Stop pair-by-pair consolidation when the
remaining ranked candidates approach known output limits or no longer remove a
material amount of repeated construction.
