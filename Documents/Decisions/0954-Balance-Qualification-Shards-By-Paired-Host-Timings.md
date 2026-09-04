# Decision 0954: balance qualification shards by paired-host timings

- Date: 2026-09-04
- Status: Implemented scheduling candidate from one qualified paired-host run;
  execution of the new assignment remains pending
- Extends: [Decision 0949](0949-Balance-Qualification-Shards-By-Declared-Cost.md)
- Preserves: all 126 owners, all 5,981 cases, owner commands, duration profiles,
  timeout bounds, host isolation, and fresh qualification

## Context

The declared duration profiles were designed as conservative owner timeouts and
planning allowances. They are too coarse to balance qualification: before this
decision they assigned exactly 4,890 expected seconds to each shard, while the
qualified owner receipts project 6,547,869 Windows milliseconds onto shard 1
and only 2,502,130 onto shard 4. Equal profile totals therefore did not mean
equal work.

The completed Language 1.0 Slice 8 qualification produced a structured result
for every owner in every Windows and Debian shard. Those 252 measurements are a
better scheduling input than the profile labels, provided their single-run and
source-state limits remain explicit.

## Decision

1. Retain a versioned timing baseline containing exactly one Windows and one
   Linux elapsed duration for every registered owner at the qualified source
   commit and GitHub Actions run.
2. Make the read-only qualification planner validate the baseline against every
   current owner name and case count. Missing, duplicate, malformed, or stale-
   shape rows fail planning.
3. Report declared profile totals and observed paired-host totals separately.
   Declared values remain timeout policy; observed values guide shard placement.
4. Reassign only six independent owners: database storage moves from shard 1 to
   4; WVB-to-WVO reconstruction moves from 3 to 2; native SHA-256 lowering,
   hosted verifier publication, echo-command launch, and installation-command
   dispatch move to shard 1.
5. Do not change an owner command, case, duration profile, timeout, or evidence
   requirement as part of this scheduling change.

## Evidence

GitHub Actions run 33894696448 passed 126 owners and 5,981 cases on each host.
The retained manifest contains 126 owner rows and 252 elapsed measurements. The
planner validates the exact source commit, run, date, owner set, and case counts.

Under the immediately preceding shard assignment, those same owner durations
project a 6,547,869 ms critical shard. The six moves produce these projected
loads:

| Shard | Windows ms | Linux ms |
| ---: | ---: | ---: |
| 1 | 4,610,402 | 4,077,157 |
| 2 | 4,041,125 | 4,521,081 |
| 3 | 4,597,601 | 4,504,598 |
| 4 | 4,655,707 | 3,364,565 |

The projected critical path is 4,655,707 ms, 28.90 percent below the preceding
mapping and 4.01 percent above the 4,476,209 ms arithmetic lower bound. Combined
paired-host parallel efficiency rises to 92.28 percent. These are historical
projections, not a claim that the new source state completed qualification at
that speed.

## Consequences

- Qualification scheduling now follows measured host work instead of equal
  categorical profile sums.
- The current declared profile distribution becomes intentionally uneven. That
  is visible rather than hidden, and individual timeout enforcement is unchanged.
- The next paired qualification can compare predicted and actual shard times,
  while owner-level optimization continues against the measured compiler,
  runtime, and database bottlenecks.

## Reconsideration triggers

Rebalance again after at least one complete result set shows a material shift,
or when owners are added, removed, split, or merged. Do not infer an enforceable
regression threshold until at least three stable measurements exist per host.
