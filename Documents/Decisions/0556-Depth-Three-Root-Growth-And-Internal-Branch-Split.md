# Decision 0556: Depth-three root growth and internal branch split

- Date: 2026-08-14
- Status: Implemented candidate with focused Windows execution evidence
- Requires: [Decision 0551](0551-General-Depth-Two-Upsert-And-Obsolete-Ownership.md)
- Defines: [depth-three root growth](../../Specifications/Windvale-Database-Depth-Three-Root-Growth.md)
- Retains: `WVTN 1`, `WVPG 1`, `WVCR 1`, `WVDS 1`, append-only descending
  child identities, unique predecessor ownership, and four-action publication

## Context

Decision 0551 can repeatedly update a depth-two tree and propagate a routed
leaf split while its root has space. It deliberately returns `Branch_full`
when the extra separator overflows that root. Continuing the storage kernel
requires a deterministic internal-node split and a new root without adding I/O
authority, changing durable formats, or weakening recovery behavior.

The database development loop has also become a compiler workload in its own
right. The new fixture closes over 116 functions and produces a multi-megabyte
native object, so it must remain a focused cached owner rather than forcing the
complete retirement suite into ordinary development.

## Decision

- Generalize the leaf split result into one node split result shared by leaves
  and branches.
- Add one portable branch split operation that first applies a lower-level
  child split, requires the combined branch to overflow, promotes the legal
  interior separator with the best encoded-byte balance, and deterministically
  returns two canonical nonempty branches.
- Add one specialized depth-two-to-depth-three transaction. It requires both a
  leaf overflow and a root overflow; cheaper updates remain owned by the
  existing depth-two transaction.
- Allocate two leaves, two branches, one new root, and one compact log. The
  left leaf owns the old leaf and the left branch owns the old root; the new
  right siblings and new root own no predecessor.
- Publish root depth three through the unchanged bounded commit batch and
  four-action storage protocol.
- Split the former broad tree-node fixture into focused tree-node and
  single-leaf projects, then add dedicated branch-split and depth-three projects.
- Add branch split and depth-three execution to the development database owner;
  retain all four new focused projects in the complete owner.

## Evidence

The branch-split application compiles to a 52,551-byte WVB and returns `0`
through native lower, link, and hosted execution. It covers entry and rightmost
child behavior, canonical routing, byte-balanced deterministic promotion,
not-required, malformed, range, collision, empty-separator, and invalid-ceiling
rejection.

The depth-three application compiles 116 functions to a 157,653-byte WVB and a
2,889,859-byte WVO. Its linked Windows application returns `0` after validating
the five new data pages, compact log, depth-three superblock, unique predecessor
ownership, two-level routing, retained values, inserted value, typed failures,
and repeated byte-identical construction.

The focused eight-case Windows development owner passes with compiler tool,
project-WVB, and hosted-application cache hits in 402.638 seconds. The six
portable tree targets perform fresh lower, link, package, and native execution;
those steps account for nearly all of the interval. Independent Linux execution
and the digest-bound GitHub retirement shard remain qualification evidence.

## Consequences

- A full depth-two root can now grow to a valid depth-three tree without format
  or publication changes.
- Internal split policy is explicit and deterministic, but recursive cascading
  from a non-root branch is not yet implemented.
- The retirement manifest remains 53 suites and grows from 3,326 to 3,329 cases.
- The optimized inner loop is still minutes rather than seconds for this large
  compiler closure. The next tooling work should cache or avoid repeated
  lower/link/package construction for unchanged focused portable targets.
- SQL, networking, catalogs, concurrency, deletion, and reclamation remain
  outside this engine milestone.

## Reconsideration triggers

Extend this contract before updating an existing depth-three tree, before a
split must cascade through more than one internal level, or before page reuse
invalidates descending child identities. Revisit the branch encoder boundary
when the language can express a smaller reusable internal node-construction
model without increasing every tree consumer's compiler closure.
