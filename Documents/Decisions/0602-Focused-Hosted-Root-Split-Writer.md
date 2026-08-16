# Decision 0602: focused hosted root-split writer

## Status

Accepted on 2026-08-16.

## Context

Windvale had a hosted writer for a non-full depth-one root and another for
depths two through eight. The portable root-split transaction existed, but no
hosted component performed that first height increase. Combining all writer
paths would exceed the current native object limit.

## Decision

Add one focused hosted root-split writer. It reads and owns the selected root,
requires the portable transaction to prove a split is necessary, and publishes
the two leaves, branch root, and commit log with the existing four-action
durability protocol. Add a logical projection using the shared write-only
record codec.

Verify the transition with separate create, root-fill, root-split, and restart
read processes. Keep the chain under the existing root-writer development owner
and report each content-addressed cache state.

## Consequences

- Every admitted tree write transition now has a hosted component: ordinary
  depth one, depth-one root split, and depths two through eight.
- The root-split fixture is 3,673,316 object bytes, leaving 520,988 bytes below
  the 4 MiB limit.
- The split operation performs one provider read and a fixed four-page
  publication; memory does not grow with database history.
- A small supervisor-level dispatcher is still required to choose the process
  safely and expose one application-facing put operation.

## Reconsideration triggers

Reconsider the process split only when measured dead-code elimination or a new
segmented execution contract can combine the writers with clear memory
headroom and without slowing focused verification.
