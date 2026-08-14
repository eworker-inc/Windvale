# Decision 0551: General depth-two upsert and obsolete ownership

- Date: 2026-08-14
- Status: Implemented candidate with focused Windows execution evidence
- Requires: [Decision 0549](0549-Bounded-Durable-Tree-Reader-And-Root-Split.md)
- Defines: [depth-two upsert](../../Specifications/Windvale-Database-Depth-Two-Upsert.md)
- Retains: `WVTN 1`, `WVPG 1`, `WVCR 1`, `WVDS 1`, append-only descending
  child identities, and four-action durable publication

## Context

Decision 0549 could create and read the first two-leaf generation, but every
later write stopped at that boundary. A useful database engine must repeatedly
replace one routed leaf and propagate a leaf split without rewriting unchanged
siblings. It must also identify which old pages became obsolete before any
reclamation policy can be designed.

The hosted storage capability deliberately lends one response buffer until the
next call. The first integration attempt retained decoded root and child views
simultaneously. Portable tests passed, but hosted mutation correctly exposed
that lifetime violation. The near-4 MiB hosted object also exposed a separate
native inspector reporting limit, reinforcing the need for a compiler/tooling
performance milestone before broader database features.

## Decision

- Add exact branch-child replacement and split propagation for an arbitrary
  entry child or the rightmost child. Reject missing, duplicate, colliding, or
  noncanonical child updates.
- Add one portable depth-two transaction over a freshly selected root and
  routed leaf. Preserve untouched children and separators.
- Allocate replacement leaf, replacement root, and log for an ordinary update;
  allocate left leaf, new right leaf, replacement root, and log for a split.
- Make the replacement leaf own the old leaf and the replacement root own the
  old root through `Previous_page`. A new right leaf owns no predecessor.
- Reject predecessor identities outside the old committed page set and reject
  duplicate obsolete ownership inside one commit batch.
- Add `Databaseˉdurableˉpageˉownedˉcopy` so a multi-page hosted transaction can
  materialize one borrowed response before issuing the next storage call.
- Preserve explicit `Branch_full` failure. Creating a depth-three root remains
  a later engine contract.
- Extend the database-storage owner with one portable transaction case and ten
  hosted interruption paths covering both initial and repeated publication.
- Pause database feature breadth after this batch and prioritize the native
  compiler, lowerer, assembler, verifier, linker, packager, and cache loop.

## Evidence

The current portable depth-two application compiles and executes natively. It
covers ordinary left-child update, a later right-child update while sharing an
unchanged sibling, left and rightmost splits, deterministic bytes, routed-child
mismatch, invalid leaf range, replacement collision, branch-full rejection,
and owned-page copy validation.

The focused hosted Windows application links as a 4,076,890-byte image. Normal
execution advances 4,608 bytes to the 20,992-byte first depth-two generation,
then to a 33,280-byte repeated-update generation, and reopens without changing
bytes. A cached build/lower/link/package and three-run probe took 60.1 seconds;
the ten already-built interruption/restart scenarios took 2.1 seconds. Each
initial interruption returns `100` through `104`, each repeated-update
interruption returns `110` through `114`, and every restart converges to the
same 33,280-byte committed length.

The generated hosted WVO remains below the canonical 4,194,304-byte limit and
the native linker fully validates it, computes its input identity, and links it.
The separate WVO inspector validates the object but its one-shot SHA reporting
path exits while handling this near-limit value. That reporter and the large
generated x64 code are compiler/tooling follow-up work, not a reason to widen
the format or weaken the linker boundary.

Final change-aware Windows verification passed all 60 selected cases in
1,408.8 seconds: durable commit 12 cases in 144.68 seconds, database storage 14
cases in 1,226.89 seconds, Project 2 workspace 8 cases in 1.76 seconds, and
libraries 26 cases in 27.52 seconds. Database storage reconstruction consumed
87 percent of the combined interval even though the cached interruption probes
take about two seconds. Independent Linux execution remains the qualification
boundary for this implemented candidate.

## Consequences

- A depth-two tree can now accept repeated copy-on-write updates and routed leaf
  splits without rewriting unchanged siblings.
- Obsolete pages have unique accountable owners, but no page is reclaimed or
  reused yet.
- Borrowed provider lifetime is explicit at the reusable durable-page boundary;
  one-page traversal stays allocation-free while multi-page mutation opts into
  an owned copy.
- The retirement manifest grows from 3,287 to 3,288 cases while retaining the
  existing four-shard, dual-host qualification structure.
- SQL, networking, catalogs, concurrency, and deeper trees are intentionally
  paused while native tool latency and generated-code size are improved.

## Reconsideration triggers

Replace or extend this contract before page reuse invalidates descending child
identities, before one commit needs more than 63 data pages, or before branch
growth needs a depth-three root. Revisit owned-copy placement if the capability
contract gains independently retained response values. Do not raise WVO or
arena limits merely to hide inefficient lowering or reporting.
