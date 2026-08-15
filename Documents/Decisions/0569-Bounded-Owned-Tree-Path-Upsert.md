# Decision 0569: Bounded owned tree-path upsert

- Date: 2026-08-15
- Status: Implemented candidate with focused Windows execution evidence
- Requires: [Decision 0568](0568-Existing-Depth-Three-Upsert-And-Bounded-Cascade.md)
- Defines: [bounded tree-path upsert](../../Specifications/Windvale-Database-Tree-Path-Upsert.md)
- Retains: `WVTN 1`, `WVPG 1`, `WVCR 1`, `WVDS 1`, append-only descending
  child identities, unique predecessor ownership, and four-action publication

## Context

The fixed depth-two and depth-three transactions proved the copy-on-write
invariants and split shapes, but each additional tree height would otherwise
require another nearly identical transaction. The database needs to update the
depth-four trees it already creates and to exercise a cascade across more than
one internal branch before server, catalog, or query work can rely on it.

Windvale has bounded collections, but introducing a new collection-backed page
path would broaden both the language/runtime dependency and ownership surface.
The durable page format already supplies a natural exact-size packed boundary.

## Decision

- Add one capability-free transaction for input depths two through eight.
- Accept one owned byte path containing exact `WVPG 1` pages from root to leaf.
  Hosted callers must copy borrowed responses before assembling the path.
- Validate the entire path top-down, including physical kind, current-root
  identity, descendant visibility, selected-child identity, descending graph
  order, node counts, and inherited key ranges.
- Rebuild bottom-up with a bounded loop, propagating deterministic leaf and
  branch splits through every supplied ancestor and creating one new root when
  required.
- Permit bounded rescans of the owned path instead of adding recursion or a
  general path collection. Depth eight limits validation to 36 input-page
  decodes and worst-case output to 17 data pages.
- Give exactly one left/replacement output page ownership of every selected
  predecessor and publish packed obsolete identities in leaf-to-root order.
- Reuse the existing commit batch, log, dual superblock, recovery boundary,
  and publication actions without a format revision.
- Add focused depth-three, depth-four, full-cascade, deterministic-byte, and
  malformed/mismatched-path execution evidence on the database owner.

## Evidence

The focused source set compiler-aligns 124 functions and executes to result
`0` through the Windows native lower, link, and hosted application path. Its
full depth-four cascade splits the leaf, two internal branches, and old root;
allocates pages 22 through 30 plus log 31; creates a depth-five root; records
obsolete pages 7, 10, 15, and 20; and reproduces the complete pages and
superblock byte for byte.

The Windows database development owner passes ten cases with all portable and
hosted checkpoints hit. Independent Linux execution and the cold paired-host
retirement gate remain qualification evidence.

## Consequences

- Existing depth-four trees can now be updated, and one transaction shape
  covers input depths two through eight.
- A single leaf split can propagate through every supplied ancestor while
  retaining exact deterministic allocation and recovery bounds.
- The packed path makes ownership and length validation explicit, but callers
  remain responsible for reading and copying its pages before mutation.
- Bounded top-down rescans trade at most 36 page decodes for a smaller current
  runtime surface and can be replaced when a precise consuming path collection
  is justified.
- The database-storage retirement owner grows from 18 to 19 cases and its
  development subset from nine to ten portable/hosted targets.
- The complete retirement manifest remains 62 suites and grows from 3,451 to
  3,452 cases.
- Catalogs, row schemas, query execution, networking, authentication,
  concurrency, deletion, and reclamation remain outside this engine milestone.

## Reconsideration triggers

Replace the packed path when a caller needs provider-driven mutation in one
operation, when a consuming bounded path collection has an explicit ownership
contract, or before input depth nine must be updated. Revisit the fixed maximum
against stack, work, memory, and commit limits rather than silently raising it.
Page reuse must first replace the descending-identity and predecessor rules
with equally explicit generation-safe evidence.
