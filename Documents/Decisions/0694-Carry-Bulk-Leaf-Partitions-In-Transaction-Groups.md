# Decision 0694: Carry bulk leaf partitions in transaction groups

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Decision 0692](0692-Bulk-Partition-Transaction-Leaves.md)
- Replaces format: `WVLG 1` from [Decision 0690](0690-Group-Transaction-Mutations-By-Leaf.md)
- Defines: [`WVLG 2`](../../Specifications/Windvale-Database-Transaction-Leaf-Groups.md)

## Context

`WVLG 1` used the sequential leaf rewriter and stopped at the first temporary
overflow. That was safe as an intermediate signal but not the correct final
transaction state: later sorted deletes can make the leaf fit again. It also
discarded overflow work and required the successor to repeat the mutation
group.

## Decision

- Evaluate each distinct affected leaf exactly once with the bulk final-state
  partitioner.
- Replace `split_required` with `partitioned`. A changed group stores its full
  validated `WVLP 1` result whether it contains one or many leaves.
- Report exact output-leaf and operation counts across every completed group.
- Treat an individually oversized entry or invalid partition as a hard atomic
  group-plan failure with no bytes or counts.
- Advance the early plan encoding directly to `WVLG 2`; no compatibility case
  requires retaining `WVLG 1` parsing.

## Evidence

The scale-qualified project build produces a deterministic 160,313-byte WVB
with SHA-256
`2ded6e5b7a471bd42f545bbfc4a5b689cccc57a4042176d589f7ac98a5f6627e`.
It lowers to a deterministic 2,435,529-byte WVO with SHA-256
`c231f05748a9b514ce7e3add60305d132714d0703ee2b1b98aee54d16591c577`
and packages as a 2,452,992-byte Windows application with SHA-256
`173d235415943e448815106851888fdec141f768a7c4b5797d383acf0fbd486c`.
The application returns zero.

Twenty fresh whole-process runs measured 104.740 ms minimum, 110.784 ms
median, 110.623 ms mean, and 118.758 ms maximum. Peak sampled working set was
9,388,032 bytes. This includes startup, durable-page hashing, two-leaf grouping,
bulk two-leaf partitioning, deterministic comparison, and malformed cases. It
is correctness-test cost, not persistent-server transaction throughput.

Changed-file planning passes 24 general and 135 native routing cases. Native
development dependency closure passes for 3 owners and 34 declarations. WVB
verification, deterministic WVB/WVO comparison, Bash syntax, and diff checks
pass. The focused project uses the scale-qualified current project build
driver; lowering, packaging, and execution use repository-native tools.

## Consequences

The leaf-group plan is now the complete bounded logical leaf replacement plan.
The durable-page allocator can consume it without repeating mutations or
guessing whether overflow is final. Shared ancestor rewrite and publication
remain incomplete.

Changed groups store full `WVLP 1` envelopes, adding small format overhead in
exchange for an independently validated handoff. The 4 MiB group-plan ceiling
is explicit and above the tighter bound imposed by 32 old leaves plus the
global 256 KiB mutation limit.

## Reconsideration triggers

Flatten nested partition envelopes only if persistent-server profiles show a
material decode or copy cost. Any flattening must preserve one validation
boundary, exact final-state semantics, deterministic separators, and bounded
memory.
