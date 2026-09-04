# Decision 0949: balance qualification shards by declared cost

- Date: 2026-09-04
- Status: Implemented candidate with plan-only runner evidence; paired execution
  remains pending
- Extends: [Decision 0550](0550-Measured-Native-Retirement-Sharding.md)
- Supports: [Decision 0947](0947-Treat-Complete-Qualification-As-One-Evidence-Graph.md)
- Extended by: [Decision 0954](0954-Balance-Qualification-Shards-By-Paired-Host-Timings.md)

## Context

Complete native qualification already executes four shards on four independent
Windows runners and four independent Linux runners. The registry nevertheless
assigned 7,545 expected seconds to shard 2 and only 3,225 to shard 4. Because the
gate waits for every shard, the 19,560 owner-seconds had only 64.81 percent
declared parallel efficiency and a 7,545-second expected critical path.

Adding parallel owner execution inside one runner would introduce shared CPU,
memory, temporary-path, and diagnostic-order risks. Reassigning existing owners
among the already isolated runner jobs changes none of those boundaries.

## Decision

1. Preserve all 126 owners, 5,981 cases, commands, duration profiles, summaries,
   host requirements, and failure behavior. Change only 16 shard fields.
2. Assign exactly 4,890 expected seconds to each of four shards, the arithmetic
   ideal for 19,560 declared seconds. Shard owner and case counts may differ;
   expected execution cost is the scheduling objective.
3. Reduce the maximum-profile critical path as a secondary constraint. The
   resulting declared maxima are 13,200, 24,600, 24,600, and 16,800 seconds.
4. Keep execution sequential inside each shard. GitHub's existing four-job
   matrix remains the parallelism boundary on each host.
5. Have the read-only qualification planner report the ideal, expected spread,
   and parallel-efficiency basis points. Static verification binds the exact
   four shard summaries and asks the real runner for each plan without launching
   an owner.
6. A single-shard resume uses the new assignment. A previously recorded owner
   name must be paired with its current shard; stale shard/name combinations
   continue to fail closed.
7. Declared durations remain planning weights, not measured performance claims.
   Rebalance again only from bounded current-host observations or when registry
   growth creates a material expected spread.

## Evidence

The qualification planner reports these current plans:

| Shard | Owners | Cases | Expected seconds | Maximum seconds |
| --- | ---: | ---: | ---: | ---: |
| 1 | 10 | 780 | 4,890 | 13,200 |
| 2 | 39 | 2,140 | 4,890 | 24,600 |
| 3 | 39 | 1,809 | 4,890 | 24,600 |
| 4 | 38 | 1,252 | 4,890 | 16,800 |

The expected spread is zero and declared parallel efficiency is 10,000 basis
points, or 100 percent. The expected critical path falls from 7,545 to 4,890
seconds, a 35.19 percent reduction; the maximum-profile critical path falls from
27,300 to 24,600 seconds, a 9.89 percent reduction. These are schedule
predictions. No full or paired-host qualification was launched for this change.

## Consequences

- Existing CI capacity is used evenly before new same-machine concurrency is
  considered.
- No test, case, owner, or diagnostic contract is removed or weakened.
- Shard numbers remain scheduling metadata, so their semantic grouping is not
  preserved when it conflicts with critical-path balance.
- Actual elapsed time can still differ because current profiles are coarse; the
  structured shard results remain the source for future calibration.

## Reconsideration triggers

Rebalance when three stable measurements on either host show a material shard
spread, when an owner changes duration class, or when owner additions raise the
declared expected critical path above the arithmetic ideal by more than five
percent. Do not introduce within-shard parallelism until owners declare resource
classes and prove isolated temporary and mutable state.
