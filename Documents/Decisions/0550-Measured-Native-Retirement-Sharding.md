# Decision 0550: Measured native retirement sharding

- Date: 2026-08-14
- Status: Implemented candidate pending dual-host workflow evidence
- Refines: [native retirement test suite](../../Specifications/Windvale-Native-Retirement-Test-Suite.md)
- Preserves: cold owner execution, exact case inventory, fail-fast owner behavior,
  focused filters, and the sequential complete-plan oracle

## Context

Database development exposed a verification critical path much larger than its
runtime fixtures. GitHub run `31806725202` passed the complete native
retirement suite in 2,374 seconds on Windows and 2,186 seconds on Linux. The
database-storage owner alone used 651 and 626 seconds respectively, while the
seed native front door and compiler reconstruction were the next two largest
owners. The full plan remained sequential even though each owner allocates its
own private state and validates immutable repository artifacts.

The workflow already cancels superseded runs by workflow and ref. Replacing or
duplicating that policy would not reduce one surviving run. The existing native
tool checkpoint is intentionally local-development evidence and explicitly
excluded from clean qualification, so using it to accelerate the full gate
would weaken rather than reorganize evidence.

The version-1 prose also reported 3,282 cases even though the manifest and the
green dual-host run contained 3,287. A format revision is the right point to
make that total machine-checked.

## Decision

- Advance the digest-bound retirement manifest to version 2 and add one exact
  shard field to every suite.
- Define four shards. Every suite appears once, every shard is nonempty, and
  the complete inventory remains 52 suites and 3,287 cases.
- Preserve manifest order within each selected shard. No-argument execution
  still runs the entire plan sequentially, and `--filter` still runs one exact
  owner. `--shard 1` through `--shard 4` adds disjoint selection only.
- Balance using the slower observed Windows/Linux interval for each owner from
  run `31806725202`. The largest owner remains alone because it establishes the
  current lower bound.
- Emit owner and total elapsed milliseconds as operational telemetry. Timing is
  excluded from child success summaries and is never a pass/fail threshold.
- Run all four shards on each GitHub host with matrix fail-fast disabled. The
  unchanged final Verification gate consumes each host matrix result, so every
  shard must succeed.
- Keep every owner cold and unchanged. This decision does not import the local
  checkpoint into qualification, skip a suite, cache a test result, or replace
  independent bootstrap and WebAssembly evidence.

## Measured allocation

| Shard | Suites | Cases | Slower-host seconds |
| ---: | ---: | ---: | ---: |
| 1 | 1 | 13 | 651.2 |
| 2 | 14 | 1,279 | 592.2 |
| 3 | 18 | 1,138 | 592.1 |
| 4 | 19 | 857 | 591.8 |

The measured owner total is 2,427.3 seconds and the largest shard is 651.2
seconds. Ignoring runner setup and queueing, the expected retirement critical
path falls by about 73%, from 36–40 minutes to roughly 11 minutes. The first
dual-host matrix run, not this projection, determines the accepted result.

## Consequences

- A failed shard does not cancel independent evidence from its peers.
- Total compute remains approximately constant and may rise modestly because
  checkout, Node setup, and Debian preparation repeat per shard. The intended
  improvement is wall-clock feedback, not lower hosted-runner consumption.
- GitHub runner availability may limit realized parallelism. The sequential
  command remains the deterministic fallback and release oracle.
- Per-owner telemetry makes later compiler-product promotion and safe toolset
  reuse measurable. Qualification cache reuse remains a separate decision.
- Manifest additions require an explicit shard assignment, updated digest, and
  exact suite/case validation before they can enter the gate.

## Reconsideration triggers

Rebalance only when multiple dual-host runs show material drift, one shard
repeatedly exceeds the others, or runner queueing erases the wall-clock gain.
Revisit the shard count when the largest indivisible owner changes materially.
Do not replace cold execution with cache reuse until an independently qualified,
content-addressed compiler-product contract proves equivalent evidence.
