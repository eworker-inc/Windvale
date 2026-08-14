# Decision 0546: First native verification tool checkpoint

- Date: 2026-08-14
- Status: Implemented with focused Windows cache and execution evidence
- Requires: [Decision 0545](0545-Refresh-Native-Compiler-Products-For-Host-Storage.md)
- Defines: [Native tool checkpoint 1](../../Specifications/Windvale-Native-Tool-Checkpoint.md)
- Accelerates: [Decision 0544](0544-First-Native-Durable-Storage-Provider.md)

## Context

The first coherent nine-case database-storage owner passed in 658.777 seconds,
although cached source compilation and lowering required only 3.270 and 3.501
seconds and each durability process ran in less than one second. More than half
of the owner elapsed time remained in native tool reconstruction and packaging
rather than the changed database behavior. The current compiler build driver
does not change during an ordinary database edit. A server restart also showed
that the monolithic temporary construction had no reusable tool checkpoint.

## Decision

- Keep the no-argument database-storage owner as the clean nine-case path. It
  ignores local cache state, reconstructs both compiler tools, checks duplicate
  WVB/WVO output, and constructs both target applications.
- Add `--prepare-development-tools` and `--development` as explicitly
  non-qualification modes.
- Build the current build-driver WVB on every lookup so its complete source
  closure remains the primary input identity. Cache only its expensive native
  profile-2 package.
- Key the target-specific immutable entry by SHA-256 of the WVB, every producing
  launcher, hosted-toolset inventory, target, profile, and checkpoint format.
- Bound and compare the exact five-line checkpoint plus application size and
  SHA-256 on every hit. Reject invalid existing entries instead of repairing or
  executing them.
- In the development lane, run the composed host-storage case once, assemble
  and verify both platform leaves, and execute Windows create, injected-tail
  recovery, and stable reopen. Skip historical standalone cases, duplicate
  generation, and Linux container packaging until the coherent clean owner.

## Evidence

| Path | Elapsed | Result |
| --- | ---: | --- |
| Clean nine-case owner | 658.777 s | Passed |
| Initial checkpoint construction and development case | 365.652 s | Passed, pre-final manifest format |
| Final checkpoint hit preparation | 55.241 s | Passed |
| Final warm development lifecycle | 100.179 s | Passed |
| Forged output SHA-256 | 54.519 s | Rejected before execution |
| Final changed-file gate | 624.934 s | Passed 9 database-storage and 26 library cases |

The final warm lifecycle is approximately 6.6 times faster than the clean owner
and reduces elapsed time by approximately 84.8 percent. These lanes provide
different evidence and the timing comparison is not a claim of equivalent
coverage.

## Consequences

- An ordinary provider or database-fixture edit now receives real durability
  feedback in roughly one to two minutes on the measured host.
- Compiler-source changes still rebuild the current WVB; packaging inputs select
  a new cache key automatically.
- Cache entries are local acceleration, never trusted bootstrap provenance.
- Version 1 leaves compiler incrementality, per-case WVB/WVO checkpoints,
  eviction, stale partial cleanup, bounded concurrent publication, and Linux
  execution to later work.

## Reconsideration triggers

Replace the owner-specific checkpoint with a shared native cache when a second
owner needs it. Revise the format before sharing entries across hosts, changing
the digest suite, allowing concurrent writers, or using cache evidence in a
release or qualification decision.
