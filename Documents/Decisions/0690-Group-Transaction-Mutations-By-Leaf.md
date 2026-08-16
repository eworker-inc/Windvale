# Decision 0690: Group transaction mutations by leaf

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Decision 0687](0687-Bounded-Transaction-Path-Set.md)
- Defines: [transaction leaf groups](../../Specifications/Windvale-Database-Transaction-Leaf-Groups.md)

## Context

The transaction path set proves that all mutations belong to one committed
snapshot, but applying the full transaction separately for every mutation
would repeatedly decode and rebuild shared leaves. Atomic multi-record planning
needs one bounded unit of work per affected leaf and must keep leaf overflow
separate from hard failure.

## Decision

- Group consecutive sorted mutations by the durable identity of their routed
  leaf and apply each group exactly once.
- Revalidate paths at the planner boundary instead of trusting a caller's
  counts or previously decoded records.
- Emit versioned `WVLG 1` bytes with explicit unchanged, rewritten, and
  split-required outcomes. Store payload bytes only for rewritten leaves.
- Treat leaf overflow as planned successor work. Treat every other rewrite or
  validation failure as atomic plan failure with no returned bytes or counts.
- Keep aggregate work counts exact only for groups that completed. A later
  split planner reapplies an overflowing group from its canonical mutations.

## Evidence

The scale-qualified project build produces a deterministic 142,495-byte WVB
with SHA-256
`783d111d8461863d90522f094b42aa1ee60d01d5e435a7151d9dcb9e397d8de6`.
It lowers to a deterministic 2,190,043-byte WVO with SHA-256
`01f0d06a835ef5f1387f7592cdffb7ca7b038e9c889322e26c9f660f98f11ab9`
and packages as a 2,207,232-byte Windows application with SHA-256
`6ddfc986370e4f1e5cb7b58c5f5f429bd201d8bf5e648e42f88ac939898375d0`.
The application returns zero.

Twenty fresh whole-process runs measured 86.580 ms minimum, 89.534 ms median,
90.909 ms mean, and 97.099 ms maximum. Peak sampled working set was 9,003,008
bytes. This includes process startup, durable-page hashing, every positive and
malformed case, and deliberate 4 KiB overflow construction. It is correctness
test cost, not transaction throughput.

Changed-file planning passes 24 general and 133 native routing cases. Native
development dependency closure passes for 3 owners and 34 declarations; WVB
verification, deterministic WVB/WVO comparison, Bash syntax, and diff checks
pass. The general changed-file gate selects all database targets because the
verification harness itself changed and then reaches the current native build
tool's known `Unsupported_module` capacity boundary. The focused project uses
the scale-qualified current project build driver; lowering, packaging, and
execution use repository-native tools.

## Consequences

The transaction planner now performs one leaf rewrite per affected leaf and
has an explicit bounded handoff for split work. It still does not claim a
durable atomic multi-record transaction. Split handling, shared ancestor
replacement, page allocation, and one commit publication remain required.

The repeated validated path copy can transiently consume close to 32 MiB when
caller and owned bytes coexist. This remains bounded and visible. The
persistent-server benchmark will determine whether path deduplication should
precede or follow the first correct durable batch.

## Reconsideration triggers

Replace the repeated path representation if server memory or transaction
latency evidence shows material cost. Change the plan encoding only through a
new explicit version. Do not hide split work inside partial leaf output.
