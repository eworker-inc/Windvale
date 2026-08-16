# Decision 0664: Physical database leaf delete and bounded scan

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Windvale Database proposal](../Project/Windvale-Database-Proposal.md)
- Defines: [tree leaf operations](../../Specifications/Windvale-Database-Tree-Leaf-Operations.md)
- Extends: [`WVTN 1`](../../Specifications/Windvale-Database-Tree-Node.md)
- Informed by: EWDB's soft-delete/query needs, without adopting its managed in-memory row representation

## Context

The durable tree could get and upsert records through bounded paths, but had no
removal primitive or ordered range output. Secondary indexes and query
execution both need those operations. Adding tombstones to `WVRD 1` would make
deletion depend on logical-row filtering and postpone real key removal; adding
leaf sibling pointers would make copy-on-write neighbor maintenance part of
every mutation. Neither dependency is necessary for the first correct layer.

## Decision

- Physically remove an exact key/value entry from one validated immutable leaf
  and return canonical replacement bytes.
- Admit the empty generation-1 root as an absent delete and retain a valid
  32-byte zero-entry leaf when the final real key is removed.
- Treat absent deletion as a deterministic no-op that returns the original
  immutable bytes without copying.
- Keep existing branch separators after deletion. They remain correct
  lower-inclusive partitions even if the original separator key disappears or
  a child becomes empty.
- Scan one validated leaf in ascending key order with optional inclusive or
  exclusive start/end bounds and a required 1-through-500 row limit.
- Return selected packed entries as one borrowed contiguous slice, plus exact
  examined count, local `Has_more`, and the last key for exclusive resumption.
- Keep cross-leaf traversal, committed-generation cursors, reverse scans,
  durable path replacement, merges, and reclamation as explicit next layers.
- Extend the existing tree-node verifier rather than add another process and
  build target; leaf behavior changes therefore keep one focused owner.

## Evidence

The focused fixture removes missing, first, middle, last, and sole entries;
proves retained values and deterministic bytes; admits empty bootstrap and
zero-entry leaves; and rejects invalid keys, ceilings, kinds, and nodes. It
scans unbounded, inclusive, exclusive, equal, empty, limited, and resumed
ranges, verifies exact packed result bytes and lookahead accounting, and
rejects invalid limits, bounds, ordering, and nodes.

Two independent builds produced identical 76,085-byte WVB modules with SHA-256
`64080df6e0731ae035eecc5b7c27f4680f96bb94d7ca94f1744a2ebea828a651`.
Independent lowering produced identical 869,418-byte WVO objects with SHA-256
`a7a7ca6f2dbdffa80342e5dea3ae1f22ac6608dea7af7ba3aa96a85421fceed3`.
The 886,272-byte Windows hosted executable returned zero for the complete tree
fixture.

A ten-run local whole-process sample had 35.442 ms median time and 38.447 ms
mean time. A separate sampled set observed at most 7,880,704 bytes of client
working set. These measurements include the complete tree-node fixture and are
not durable server throughput.

Changed-file planning passes 24 general and 127 native routing cases. The new
scan library, specification, and library project map to the existing focused
`tree-node` database target; no extra qualification process was added.

## Consequences

The database now owns exact physical leaf semantics needed by durable delete
and ordered query execution. Delete does not create invisible records, and a
scan page does not allocate or copy each result.

Sparse and empty non-root leaves remain until a later merge/reclamation policy.
That trades space efficiency for a small, crash-safe copy-on-write mutation
path. A future compactor may collapse those leaves without changing visible
key semantics.

## Reconsideration triggers

Revisit the result shape when cross-leaf cursor evidence requires an encoded
cursor, when reverse index scans are implemented, or when measured sparse-tree
cost justifies bounded merge and separator compaction.
