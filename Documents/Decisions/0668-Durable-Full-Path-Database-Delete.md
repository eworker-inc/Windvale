# Decision 0668: Durable full-path database delete

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Windvale Database proposal](../Project/Windvale-Database-Proposal.md)
- Extends: [tree-path mutation](../../Specifications/Windvale-Database-Tree-Path-Upsert.md)
- Narrows: [`WVPG 1` zero-item payload admission](../../Specifications/Windvale-Database-Durable-Commit.md)

## Context

The portable leaf layer could physically remove one key, but the database
could not yet turn that replacement into one crash-safe full-depth tree
commit. The existing path-upsert transaction already owned exact path
validation, bottom-up copy-on-write allocation, predecessor evidence, and
publication planning. Delete should reuse those rules without introducing a
parallel tree or durability model.

Deleting the final key also exposed a mismatch between layers. `WVTN 1`
correctly represents an empty leaf as a canonical 32-byte node, while the
outer `WVPG 1` validator previously rejected every nonempty payload whose item
count was zero.

## Decision

- Add `Databaseˉtreeˉpathˉdeleteˉbegin(Current, Path, Key)` beside the
  existing path upsert so both mutations share complete route, generation,
  page, kind, count, visibility, and inherited-range validation.
- Treat a missing key as a successful no-op: no allocated pages, no obsolete
  pages, no generation or sequence consumption, and no publishable commit.
- For a present key at depth `D`, append exactly `D` data pages: the physical
  replacement leaf followed by every replacement ancestor through the root.
- Retain branch separators and tree depth. Sparse and empty leaves are valid;
  merge and reclamation remain separate policies.
- Permit a nonempty zero-item payload only on a physical leaf page. The tree
  reader must still decode the inner node and prove that its entry count is
  zero. Root, branch, and commit-log item-count rules do not change.
- Allow ordinary path upsert and hosted path discovery to admit a validated
  zero-entry `WVTN 1` leaf so deletion does not create an unwriteable leaf.

## Evidence

The focused path fixture deletes through depth three and checks the replacement
leaf, branch, root, predecessors, obsolete identities, unchanged separators,
and routing. It proves deterministic commit bytes, a missing-key no-op,
deletion of the final entry, and refill of the resulting empty durable leaf.

Two independent builds produced identical 186,153-byte WVB modules at
SHA-256 `9ac76bfc6032157dadb26280f95d56a90543b03f329be8c2de4e0e9b3cf7be04`.
Independent lowering produced identical 3,606,297-byte WVO objects at SHA-256
`b7387182dbbbdc98325371258515e4f1e47d3d4cffb1e4c850fbb13d84b76a5a`.
The 3,624,960-byte Windows executable returned zero.

Ten whole-process runs had 699.031 ms median, 700.175 ms mean, 681.089 ms
minimum, and 729.300 ms maximum time. Ten sampled runs observed at most
11,730,944 bytes of client working set. These figures execute the complete
portable path fixture and are not server throughput.

The durable-page owner reproduced identical WVB and WVO artifacts, ran all
twelve Windows boundary and malformed cases with result 42, and constructed
the exact Windows and Linux images. Its official focused verifier passed.
Changed-file planning passes 24 general and 127 native routing cases.

## Consequences

Windvale Database now has a deterministic crash-safe delete transaction for
every currently admitted path depth. The old generation stays immutable and
the new generation becomes visible only through the existing superblock
publication protocol. Missing deletes cause no write amplification.

An empty leaf consumes one page until a later bounded merge or compactor
reclaims it. Provider-backed delete orchestration, cross-leaf scans, reverse
scans, snapshot cursors, and reclamation remain explicit next layers.

## Reconsideration triggers

Revisit stable separators and empty-leaf retention when measured sparse-tree
cost justifies bounded merge, when reclamation can prove snapshot safety, or
when reverse traversal requires additional navigation evidence.
