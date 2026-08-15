# Decision 0568: Existing depth-three upsert and bounded cascade

- Date: 2026-08-15
- Status: Implemented candidate with focused Windows execution evidence
- Requires: [Decision 0556](0556-Depth-Three-Root-Growth-And-Internal-Branch-Split.md)
- Defines: [existing depth-three upsert](../../Specifications/Windvale-Database-Depth-Three-Upsert.md)
- Retains: `WVTN 1`, `WVPG 1`, `WVCR 1`, `WVDS 1`, append-only descending
  child identities, unique predecessor ownership, and four-action publication

## Context

Decision 0556 creates the first depth-three tree but deliberately stops before
updating one. The next storage-kernel step is not a listener, SQL parser, or
catalog. It is the ability to mutate an already deeper generation while
preserving deterministic copy-on-write publication and crash recovery.

The established node operations already rewrite a selected child and split a
full branch. The missing composition must validate two routing levels, allocate
new pages bottom-up, give every obsolete page one replacement owner, and handle
the case where the split reaches the root. A general recursive path structure
is not yet justified by the language's bounded collection surface.

## Decision

- Add one capability-free transaction accepting an exact selected depth-three
  root, internal branch, leaf, key, and value.
- Validate physical selection, both routes, inherited ranges, item counts,
  generation/sequence visibility, and descending child identities before
  constructing output.
- Support ordinary leaf rewrite, leaf split, internal branch split, and one
  final root split. The last case creates a depth-four root.
- Allocate pages bottom-up. Exactly the left/replacement leaf, branch, and root
  own the three supplied predecessor pages; new right siblings and a new root
  own none.
- Reuse the existing leaf and branch byte-balance policy, 63-data-page commit
  batch, compact log, dual superblock, and four publication actions without a
  format revision.
- Add a focused native target covering all four propagation shapes, typed
  rejection, deterministic bytes, and publication uncertainty boundaries.
- Put the new target in both database-storage owner modes and its declared
  development dependency closure.

## Evidence

The focused source set compiler-aligns 125 functions and executes to result
`0` through the Windows native lower, link, and hosted application path. Its
model validates three-, four-, five-, and seven-data-page transactions, exact
predecessor ownership, depth-four root creation, routed lookup, repeated byte
identity, and recover-state transitions for uncertain publication.

The change-aware Windows development verifier passes the nine-case database
owner in 97.390 seconds with all checkpoints hit, then passes the eight-case
workspace/project and 26-case library owners. Independent Linux execution and
the cold paired-host retirement gate remain qualification evidence.

## Consequences

- Existing depth-three generations can now accept an update without flattening
  or rebuilding the tree.
- One leaf split can propagate through its internal branch and root while
  retaining exact allocation and recovery bounds.
- The tree can grow to depth four, but updating that result is intentionally a
  later contract.
- The database-storage retirement owner grows from 17 to 18 cases and its
  development subset from eight to nine portable/hosted targets.
- The complete retirement manifest remains 62 suites and grows from 3,441 to
  3,442 cases.
- Catalogs, row schemas, query execution, networking, authentication,
  concurrent readers/writers, deletion, and reclamation remain outside this
  engine milestone.

## Reconsideration triggers

Extend or replace the bounded composition before updating depth four, before a
caller needs an arbitrary-depth owned path, or before page reuse invalidates
descending child identities. Revisit the transaction surface when Windvale has
a sufficiently precise bounded path collection and consuming ownership model;
do not introduce hidden recursion or ambient storage authority meanwhile.
