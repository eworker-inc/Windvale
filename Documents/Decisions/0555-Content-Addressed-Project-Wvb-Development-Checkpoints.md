# Decision 0555: Content-addressed project-WVB development checkpoints

Status: Accepted

Date: 2026-08-14

Extends: [Windvale native tool checkpoint 1](../../Specifications/Windvale-Native-Tool-Checkpoint.md)

## Context

Decision 0554 reduced the measured two-case database development owner to
125.757 seconds, but its preparation-only path still took 70.704 seconds. The
packaged compiler build-driver application was already a cache hit. The owner
nevertheless rebuilt that application's 1.1 MiB input WVB from the complete
compiler build-driver Project 2 source closure before it could derive the
existing application key.

That WVB is a deterministic source product. Reusing it must still invalidate on
workspace, project, source, native front-door, inventory, host build-driver, or
publisher changes and must not turn local cache state into qualification
evidence.

## Decision

Add a host-local, content-addressed project-WVB checkpoint for development
verification:

- reuse the existing length-framed project key over the exact workspace,
  project manifest, declared root/source closure, `Build-Wvb` launcher,
  native-front-door inventory, and current-host build-driver and publisher;
- publish one verified WVB plus its complete size/digest record into an
  immutable host-scoped entry;
- on every hit, reject linked or malformed state, rehash the WVB, compare the
  exact manifest, materialize and compare a fresh copy, and run the current
  native WVB verifier;
- fail closed on an invalid existing entry without repairing or replacing it;
  and
- use the checkpoint only to prepare the compiler build driver for the bounded
  two-case database development owner.

The no-argument database owner, ordinary `Build-Wvb` front door, compiler
owners, GitHub shards, and qualification remain cache-independent. A compiler,
front-door, build-driver project, or source change keeps its broader planner
ownership even though a future eligible run derives a different cache key.

## Consequences

- Creating the first entry and accepting the existing packaged application
  takes 78.894 seconds.
- A warm preparation hit takes 9.417 seconds instead of 70.704 seconds: 86.7
  percent less time, or 7.51 times faster.
- The complete warm database development owner takes 71.048 seconds instead of
  125.757 seconds: a further 43.5 percent reduction or 1.77 times speedup.
- Complete `Verify-Changed.ps1` takes 73.531 seconds instead of 141.120
  seconds: 47.9 percent less time, or 1.92 times faster including planning.
- Relative to the clean 1,111.135-second fourteen-case owner, the bounded warm
  feedback is 93.6 percent shorter and 15.64 times faster.
- Reordering the exact producer inputs selects a different key. Appending one
  byte to an isolated cached WVB is rejected before the packaged build driver
  is consulted, and the entry is not repaired.
- Fresh linking and repeated database-process execution now account for about
  61.6 seconds after the 9.4-second preparation phase. They require separate
  timing before another cache or batching boundary is chosen.

## Reconsideration triggers

Reconsider this decision if a declared source or producer can change without a
new key, a cache hit differs from a clean WVB, current native verification does
not run, independent Linux behavior differs, or native incremental compilation
provides a smaller auditable checkpoint.
