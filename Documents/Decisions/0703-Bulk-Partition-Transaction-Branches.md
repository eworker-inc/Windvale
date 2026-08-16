# Decision 0703: Bulk partition transaction branches

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Decision 0701](0701-Allocate-Durable-Transaction-Leaf-Pages.md)
- Defines: [`WVCR 1`](../../Specifications/Windvale-Database-Transaction-Child-Replacements.md)
- Defines: [`WVBP 1`](../../Specifications/Windvale-Database-Transaction-Branch-Partition.md)

## Context

Durable replacement leaves now have page identities, but several changed
children can share one parent. Applying the existing one-child branch rewrite
repeatedly would rebuild and copy that parent for every child, and its first
temporary overflow would lose the complete final transaction state.

The same problem repeats at every branch level. The parent core therefore
needs one generic child-replacement contract and one bulk final-state branch
partitioner before the full bottom-up planner is assembled.

## Decision

- Represent one tree level's old-to-new child mappings as bounded, versioned
  `WVCR 1` with consecutive new page identities and explicit split separators.
- Rebuild each affected branch once from all of its replacement groups.
- Validate every inserted separator against the old child's inherited lower
  and upper bounds.
- Partition the complete final branch into one through 64 nonempty branches,
  promoting separators between them.
- Backtrack one entry when the final separator causes overflow, so no output
  branch contains only one child.
- Fail atomically on a missing child, invalid range, individually oversized
  separator, impossible partition, malformed input, or invalid output.

## Evidence

The scale-qualified project build produces a deterministic 84,717-byte WVB
with SHA-256
`da05b00df0f23f4a23f54914619add54f3965fabd902fec96f6777b47ad8c803`.
It lowers to a deterministic 1,030,957-byte WVO with SHA-256
`2c5cf4b0192ddce6acac1d4a7610aeaa96a4bbe3b2e2882fbf50d4ebf9efd496`
and packages as a 1,048,064-byte Windows application with SHA-256
`7bad2c98078fe780f44c48f4de20ad49cfd68eb4f7f9a69cc6095f803c7565bb`.
The application returns zero.

Twenty fresh whole-process runs measured 25.089 ms minimum, 31.438 ms median,
35.793 ms mean, and 119.877 ms maximum. Peak sampled working set was 7,749,632
bytes. This includes startup, child-plan validation, shared-parent rebuild,
branch partitioning, deterministic comparison, routing checks, and malformed
cases. It is correctness-test cost, not persistent-server throughput.

The normal Windows development target passes its one focused case in 22.960
seconds including cached 1.560-second tool setup. Changed-file planning passes
24 general and 139 native routing cases. Native development dependency closure
passes for 3 owners and 34 declarations. WVB verification, deterministic
WVB/WVO comparison, Windows target execution, Bash syntax, and diff checks
pass. Independent Linux execution and broad qualification remain pending.

## Consequences

The remaining full-depth planner can use one stable operation at every parent
level. Shared parents will be evaluated once, and branch splits can propagate
as the same child-replacement shape instead of special one-off cases.

`WVCR 1` deliberately does not contain payload bytes or durability metadata.
It is a compact routing handoff; durable page construction remains owned by
the level planner. `WVBP 1` similarly contains logical branch payloads and
promoted separators, not storage writes.

The current implementation builds one bounded final branch body before
partitioning. This is simpler evidence and makes final-state semantics clear,
but persistent-server measurement may justify a pre-sized streaming arena.

## Reconsideration triggers

Replace the final-body construction only if measured memory-copy cost is
material. Any alternative must preserve one-pass final-state semantics,
deterministic promoted separators, exact range checks, explicit limits, and
complete validation before use.
