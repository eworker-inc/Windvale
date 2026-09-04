# Decision 0946: delegate portable database reproducibility to toolchain owners

- Date: 2026-09-04
- Status: Implemented candidate for ordinary portable database steps; hosted and
  storage-lowering evidence remain unchanged
- Extends: [Decision 0945](0945-Separate-Database-Behavior-From-Cross-Target-Packaging.md)

## Context

Each ordinary portable database qualification step historically compiled its
Project 2 source set twice, compared the WVB bytes, lowered both products,
compared the WVO bytes, admitted one object, linked it, packaged it, and finally
executed the database behavior. That made every database case an independent
compiler and lowerer reproducibility test.

Complete native qualification is fail-closed across every selected owner and
host. It already contains focused compiler reconstruction, Language 1.0
deterministic-product, WVB-to-WVO reconstruction, object-admission, linker, and
packager owners. Repeating the generic A/B construction around each database
behavior adds cost proportional to the number of cases without creating a
distinct database failure signal.

## Decision

1. An ordinary portable database step constructs one WVB and one WVO from its
   exact declared project, admits the WVO, links it, packages the current-host
   application, and executes every logical behavior named by that step.
2. The database result binds the exact source project, retained tool identities,
   admitted product, current host, and behavioral outcome. It does not claim
   independent construction reproducibility.
3. Complete repository qualification composes the database behavior result with
   the focused compiler, Language 1.0 deterministic-product, lowerer
   reconstruction, object-admission, linker, and packager results. Failure or
   absence of any required result prevents a complete qualification claim.
4. A database-only owner run, one qualification shard, or focused
   `--qualification-step` run is partial evidence. None may promote itself to a
   complete qualification result.
5. The database summary exposes `portable-reproducibility=Delegated` so the
   removed A/B work is visible rather than implied.
6. The `storage-lowering` step retains its paired source, lowerer-report, WVO,
   and bridge-object comparisons because lowering and ABI-23 composition are the
   behavior that step owns.
7. Hosted database construction retains its current paired evidence until its
   common-object, platform-object, and segmented-product dependencies are made
   explicit in the work graph.

## Evidence

The qualification planner identifies 46 portable ordinary or segmented steps
that can consume one construction. Their declared project closures contain 423
source references. Delegating the redundant second visit removes those 423
compiler source visits from the previous all-paired upper bound of 1,422 per
host while preserving the 57 logical database cases.

The publication/recovery bundle passed the focused Windows path after the change
in 38,310 ms. It constructed and admitted one product, linked and packaged the
current-host application, executed both logical cases, and emitted the explicit
delegation summary. After focused prerequisite expansion was made explicit, the
same node passed again in 39,860 ms with zero support steps. Planner, shell,
routing, documentation, and dependency contracts pass after the change. Linux,
hosted migration, and complete paired-host qualification remain pending; this
decision does not claim their completion.

## Consequences

- Ordinary portable database qualification performs approximately one half of
  its previous compiler and lowerer process work per execution step.
- Database behavior still executes from a freshly constructed and admitted
  current-host product.
- Reproducibility failures have one focused owner and diagnostic rather than
  dozens of database-shaped rediscoveries.
- A complete qualification report must prove the composition explicitly; a
  database pass alone is intentionally insufficient.

## Reconsideration triggers

Restore an independent A/B construction for a database step if its exact input
exposes a database-specific generator, ordering, or serialization nondeterminism
that is not covered by a focused toolchain owner. Reconsider delegation if the
aggregate gate can publish complete qualification without every required
toolchain result, or if database result records stop binding the exact admitted
product used for execution.
