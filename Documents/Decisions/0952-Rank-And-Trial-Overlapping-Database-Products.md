# Decision 0952: rank and trial overlapping database products

- Date: 2026-09-04
- Status: Implemented candidate with focused Windows execution evidence; Linux
  syntax verification passed and Linux execution remains pending
- Extends: [Decision 0950](0950-Bundle-Compatible-Database-Ancestor-Cases.md)
  and [Decision 0951](0951-Coalesce-Complete-Database-Bundles-In-Development.md)
- Extended by: [Decision 0953](0953-Bundle-The-Root-Split-And-Depth-Two-Database-Cases.md)
- Preserves: all database behavior labels, existing compiler and lowerer
  capacity limits, exact changed-case selection, and fresh qualification

## Context

The first four database bundles were found by identical source-closure groups.
That criterion is safe but incomplete: two products can repeat most of the same
compiler input while owning one distinct source each. Guessing at such merges is
unsafe because source overlap alone does not prove output capacity, isolation,
or successful combined execution. The previously rejected branch-pages triple
already demonstrates that a plausible source merge can cross an existing native
lowerer limit.

## Decision

1. Advance the read-only database qualification plan to version 3 and rank the
   twelve strongest
   non-identical portable one-case pairs. Report shared source count and bytes,
   union source count and bytes, and the declaration-visit reduction in basis
   points.
2. Exclude identical closures from that ranking because they already have a
   separate exact-closure report. Ranking is discovery evidence only and never
   authorizes an automatic merge.
3. Trial one candidate at a time through the existing compiler, lowerer,
   admission, linker, packager, and current-host execution path. Keep all
   existing bounds. Retain a bundle only when both original `Main` functions run
   and distinct return codes preserve member attribution.
4. Retain the transaction leaf-groups/pages pair as the first non-identical
   overlap bundle. Its Project 2 manifest uses canonical dependency-module order
   and contains the union of both original source closures plus one bounded
   dispatch root.
5. Bind the new bundle to development exactly as in Decision 0951. A complete
   pair is one execution, while a partial selection stays on its original
   project.

## Evidence

The selected pair shares 10 of its 23 separate declared source visits, a
43.47-percent visit reduction before adding the small bundle root. The union has
13 existing source paths and 249,380 source bytes. The first trial intentionally
failed closed on noncanonical dependency-module order; reordering dependencies
by their declared module names satisfied the unchanged source-set contract.

The corrected focused Windows qualification node passed both
`TransactionLeafGroups` and `TransactionLeafPages` in 56,140 ms. The development
path created its project, link, and hosted-application checkpoints in 59,540 ms
and completed an unchanged warm run with all three hits in 2,990 ms.

The qualification graph now contains 55 execution steps and 57 cases, with 59
project references across 58 unique manifests and 681 root/source references
across 145 unique paths. The development graph contains 53 behaviors in 48
executions across five bundles. No capacity limit was raised. Complete database
or paired-host qualification was not run. The owner-plan verifier passed all 31
general and 278 native routing cases, including the new exact bundle route.

## Consequences

- One more full compiler/lowerer/link/package execution is removed from both
  complete database qualification and complete database development.
- Candidate choice is reproducible and reviewable instead of depending on file
  names or intuition.
- A high overlap score remains only a prioritization signal. Actual bounded
  construction and behavior execution remain mandatory.
- The remaining branch-pages identical-closure candidate stays rejected; this
  decision does not weaken its prior capacity evidence.

## Reconsideration triggers

Change the ranking only when measured compilation data provides a stronger
capacity predictor. Stop adding pair bundles when the marginal critical-path
reduction is smaller than their manifest cost; at that point replace manual
pair dispatch with a general product/case graph or compiler phase reuse.
