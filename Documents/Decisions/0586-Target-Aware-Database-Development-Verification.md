# Decision 0586: Target-aware database development verification

- Date: 2026-08-15
- Status: Implemented candidate with focused Windows execution evidence
- Extends: [Decision 0559](0559-Checkpoint-Portable-Database-Development-Targets.md)
- Preserves: cold qualification, immutable content-addressed checkpoints, and
  behavioral execution on every selected development run

## Context

The database development owner grew from eight to fifteen targets as logical
records, catalogs, bootstrap, engine lifecycle, and hosted writing became real.
Its content-addressed project, object, linked-image, hosted-application, and tool
checkpoints avoided reconstructing unchanged products, but the changed-file
front door still executed all fifteen behaviors for every database edit.

The hosted targets are not independent. Host storage produces the canonical
initial image, the hosted tree reader produces the committed depth-two image,
and the engine and writer consume that image. Skipping those prerequisites or
caching a previously passing behavioral result would weaken the owner.

## Decision

- Derive database development targets from the root and source closure of each
  maintained database test project.
- Select one target only when every changed database input resolves to that
  single target. Select `all` for shared inputs, database tooling, ambiguous
  inputs, or more than one affected target.
- Give hosted targets an explicit prerequisite closure:
  `host-tree-reader` runs host storage first; `engine` and `host-tree-writer`
  run host storage and the hosted tree reader first.
- Keep existing content-addressed checkpoint validation. A checkpoint hit may
  reuse an immutable generated product, but the selected behavior and every
  required prerequisite behavior execute again.
- Report stable `step`, `item`, `target`, elapsed-millisecond, and cache-status
  fields before and after bounded work. Preserve one final structured status
  record.
- Retain `--development` as the full fifteen-target route. Add
  `--development-target <target>` for planner-selected development feedback on
  both Windows and Linux.
- Do not add background execution yet. The current hosted scenarios exchange
  files through one temporary dependency chain; safe concurrency first needs
  isolated immutable scenario inputs and a paired-host contract.

## Evidence

Planner verification passes 24 general and 103 native cases. It proves a
durable lifecycle edit selects `engine`, a logical-record fixture selects
`logical-record`, shared inputs select `all`, and database tooling fails closed
to `all`.

On the measured Windows host, an all-hit logical-record run passed in 11,860 ms:
9,530 ms for tool checkpoint validation and 2,200 ms for target construction,
admission, and execution. An all-hit engine run passed its three-case hosted
closure in 85,390 ms: 9,320 ms for tools, 31,300 ms for host storage, 25,190 ms
for the hosted tree reader, and 19,400 ms for the engine. These timings are
diagnostic observations, not portable pass thresholds.

## Consequences

- Ordinary lifecycle work returns focused evidence in about a minute and a
  half on the measured warm host instead of executing eleven unrelated portable
  targets and the hosted writer.
- Small independent portable edits can return evidence in seconds when their
  immutable checkpoints hit.
- Shared format, runtime, or tooling edits retain the complete database
  development owner.
- Qualification remains cold, complete, dual-host, and unaffected by target
  selection or development cache state.

## Reconsideration triggers

Add bounded parallel execution only after each hosted target can consume an
independently validated immutable input without sharing mutable scenario state.
Replace the single-target fallback with a deterministic multi-target plan if
real edits frequently affect two independent targets and measurement shows that
running `all` is again the dominant feedback cost.
