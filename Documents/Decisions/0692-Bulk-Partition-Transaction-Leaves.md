# Decision 0692: Bulk partition transaction leaves

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Decision 0690](0690-Group-Transaction-Mutations-By-Leaf.md)
- Defines: [transaction leaf partition](../../Specifications/Windvale-Database-Transaction-Leaf-Partition.md)

## Context

The leaf-group plan marks overflow, but one overflow is not necessarily one
two-way split. Up to 32 puts may require several leaves. Applying operations
one at a time also gives the wrong answer when an early put temporarily fills
the leaf and later deletes make the final state fit.

## Decision

- Merge the original leaf and all sorted mutations as one final logical state.
- Copy contiguous old-leaf runs between mutation keys so work remains linear
  without a per-entry collection or repeated whole-leaf rebuild.
- Partition the merged body deterministically into as many leaves as required,
  bounded to the original leaf plus one possible additional leaf per put.
- Encode the result as versioned `WVLP 1` bytes with exact operation counters,
  total entries, ordered separators, and full final leaf payloads.
- Reject an entry that cannot fit one leaf and return no partial bytes or
  counts. Keep an empty final key set as one valid empty leaf.

## Evidence

The scale-qualified project build produces a deterministic 85,411-byte WVB
with SHA-256
`6237a73fb7ed5d44f1bbe69b851a7cddc5dd5ec00b89218bb7cf792353c39a99`.
It lowers to a deterministic 1,042,433-byte WVO with SHA-256
`97522867bc471bce00ca2cecc12d39e3d49b9e450b50792c02ea32f67277a6d7`
and packages as a 1,059,840-byte Windows application with SHA-256
`ec8bde1fc8505a4fab9e9c9ec5871dc03c12d8f1f9cba69d4f1864edaa6c0981`.
The application returns zero.

Twenty fresh whole-process runs measured 33.869 ms minimum, 36.597 ms median,
38.050 ms mean, and 55.283 ms maximum. Peak sampled working set was 7,856,128
bytes. The process includes startup, all positive and malformed cases, the
temporary-overflow/final-fit case, and the exact 33-leaf ceiling. It is
correctness-test cost, not persistent-server transaction throughput.

Changed-file planning passes 24 general and 135 native routing cases. Native
development dependency closure passes for 3 owners and 34 declarations. WVB
verification, deterministic WVB/WVO comparison, Bash syntax, and diff checks
pass. The focused project uses the scale-qualified current project build
driver; lowering, packaging, and execution use repository-native tools.

## Consequences

Transaction overflow is now a bounded bulk operation rather than a one-split
assumption. The next shared tree planner can treat every leaf group as zero
replacement pages when unchanged or one-to-33 replacement leaves when changed.

Greedy partitions are deterministic and bounded but do not promise minimum
occupancy. Rebalancing, sibling borrowing, merging, reclamation, page identity
allocation, parent rewrite, and durable publication remain separate work.

## Reconsideration triggers

Change partition balancing only with workload and storage-growth evidence.
Replace immutable per-mutation concatenation if persistent-server measurements
show material CPU or memory cost. Change the format only through a new explicit
version.
