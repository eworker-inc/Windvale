# Decision 0944: select exact database development target sets

- Date: 2026-09-04
- Status: Implemented candidate with focused Windows execution evidence
- Extends: [Decision 0586](0586-Target-Aware-Database-Development-Verification.md)
- Extended by: [Decision 0951](0951-Coalesce-Complete-Database-Bundles-In-Development.md)
- Preserves: cold qualification, immutable content-addressed checkpoints, and
  behavioral execution for every selected case and prerequisite

## Context

Decision 0586 made the database development owner target-aware, but selected a
focused path only when every changed input resolved to one target. A change that
affected two independent target closures fell back to every development case.
This was deliberately conservative, and its reconsideration trigger called for
a deterministic multi-target plan when that fallback again dominated feedback.

That trigger has fired. Of the previously measured 115 maintained database
inputs, 25 selected `all`. For example, `Local-Database-Put.wv` affected the
portable and hosted local-service closures but ran all 50 then-current cases;
`Durable-Tree-Reader.wv` affected four closures but also ran all cases. Three
qualification fixtures for publication, recovery, and single-writer commit were
not represented in the development case list at all.

## Decision

1. Make `Tests/Native/Database-Storage-Development-Cases.txt` the versioned
   inventory of the 53 development cases. Each row declares its stable case
   label, portable or hosted cost class, and every selector that requires it.
2. Accept a sorted `+`-separated target set. The shared planner validates every
   selector, rejects duplicates and unknown names, unions matching cases in
   stable inventory order, and returns the exact case count. The `+` separator
   is intentionally safe through Windows batch and POSIX shell argument parsing.
3. Have both host owners execute the returned case labels through one generic
   membership check. Remove their separate target-count tables and branching
   selector implementations.
4. Preserve behavioral prerequisites in the inventory. A hosted local-service
   selection, for example, still runs host storage first. Checkpoints may reuse
   immutable construction products; they do not replace selected execution.
5. Add publication, recovery, and single-writer commit to development coverage.
   The complete qualification owner remains 57 cases and does not consume
   development checkpoints.
6. Return `all` only when every development case is selected or an input is not
   safe for the focused development path. A multi-target edit otherwise receives
   the exact deterministic union.
7. Report the selected total, portable, and hosted case counts. Estimate cold
   development work as a 20-second setup plus 45 seconds per portable case and
   90 seconds per hosted case. Bound it with a 120-second setup plus 90 and 180
   seconds per respective case, capped at one hour. These are planning values,
   not conformance thresholds.
8. Refuse a focused database development plan whose cold estimate exceeds the
   600-second local budget unless `-AllowLongRun` names an explicitly approved
   longer run. Preserve exact result-cache reuse before that refusal.
9. Keep execution sequential. This change removes unrelated cases; it does not
   introduce CPU or memory contention through speculative concurrency.
10. Route a combined project and its bundle root through an explicit selector
    containing only the cases constructed by that product. Do not let a
    bundle-only edit fall through to generic project routing or to every
    database case.

## Evidence

The planner contract passes all 31 general and 277 native routing cases. It now
proves exact unions across portable, hosted, transaction-commit, and JSON
boundaries, proves all four combined-project routes, verifies the versioned case
inventory and shared planner, and rejects a duplicate selector.

On the measured Windows host, the new three-case publication, recovery, and
single-writer target set passed in 77,030 ms. Publication was a checkpoint hit;
the other two cases constructed fresh products. The three-case local-service
and hosted-local-service closure passed in 193,520 ms with fresh portable and
hosted applications. Its inventory-derived planning estimate is 245 seconds
with a 570-second bound. These measurements are diagnostic local evidence, not
portable thresholds or qualification evidence.

## Consequences

- Independent database changes no longer pay for every unrelated behavior.
- The planner, Windows owner, and Linux owner share one case-selection contract,
  so adding a case no longer requires three copied selection tables.
- Plans expose their real development size and a cost-class-aware cold estimate
  instead of inheriting the 2,700-second qualification profile.
- Truly shared inputs can still select all 53 development cases, and broad
  compiler, runtime, cache, or owner changes can still require the complete
  57-case cold qualification route.
- The focused Windows results do not claim Linux execution or replace final
  paired-host qualification.

## Reconsideration triggers

Split the inventory further only when measurements identify a smaller causal
case boundary that preserves the same behavior and prerequisite evidence. Add
bounded parallel execution only after independent state, CPU and memory limits,
deterministic log collation, and a sequential equivalence oracle exist. Refit
the planning coefficients from bounded timing history when at least three cold
observations for each cost class show the current estimates are materially
wrong.
