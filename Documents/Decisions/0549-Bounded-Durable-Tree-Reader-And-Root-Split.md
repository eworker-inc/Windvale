# Decision 0549: Bounded durable tree reader and root split

- Date: 2026-08-14
- Status: Implemented candidate with change-aware Windows execution evidence
- Requires: [Decision 0548](0548-First-Durable-Tree-Node-And-Upsert.md)
- Defines: [durable tree reading and root split](../../Specifications/Windvale-Database-Tree-Reading-And-Root-Split.md)
- Retains: `WVTN 1`, `WVPG 1`, `WVCR 1`, `WVDS 1`, and the four-action
  durable publication protocol

## Context

The first durable node milestone could read and update only one already-loaded
root leaf. A full page returned `Full`, branch bytes had only local validation,
and no hosted operation could follow a durable child reference. Advancing
directly to catalogs, SQL, or a network server would therefore have placed
product surfaces over a storage engine that could neither grow nor read its
first multi-page generation.

The existing single-writer builder also accepted exactly one data page plus one
log page. A root split needs three immutable data pages, but changing the
durability protocol for that special case would duplicate commit logic and
make later split propagation harder to reason about.

## Decision

- Add portable lower-inclusive, upper-exclusive range validation and branch
  routing. Equality routes right because a separator is the first key of the
  right child.
- Copy inherited route boundaries before the next provider call, preserving
  `storage.random_access_v1`'s borrowed-response lifetime.
- Limit one lookup to 32 levels and one provider read per level. Require stable
  provider generation and length throughout the operation.
- Validate physical page identity, selected generation/sequence visibility,
  logical node kind/count, and the complete inherited key range at every
  visited node.
- Require each child page identity to be lower than its parent and the selected
  page count. This makes append-only bottom-up graphs acyclic without an
  allocation-heavy visited set.
- Add deterministic leaf split-upsert. Consider every fitting contiguous split,
  minimize encoded byte imbalance, and use the earliest split as the tie break.
- Generalize commit construction to 1 through 63 data pages plus one log page,
  while retaining one contiguous page write and the existing four durable
  actions.
- Implement the first split transaction only for a full depth-one root leaf.
  Append left leaf, right leaf, branch root, then log; publish a depth-two
  superblock without modifying the old generation.
- Route every new durable-composition source and Project 2 manifest to the
  database-storage owner, which reconstructs the current compiler closure
  before building it. Keep the generic library owner on the ordinary pinned
  front door until that compiler product is separately promoted.

## Evidence

The portable tree fixture lowered in 3.325 seconds and executed in 1.243
seconds after adding routing, inherited ranges, and split planning. The focused
root-split fixture lowered in 3.661 seconds and executed in 1.182 seconds. It
validates the exact three data pages, generated log, depth-two superblock,
separator routing, all values, deterministic bytes, and typed rejections.

The cached Windows database-development owner passed both hosted lifecycles in
185.282 seconds. The new case publishes a 20,992-byte generation, performs
exact two-page lookups through both child routes, proves stable reopen, and
recovers each of five independently injected publication interruptions. The
first interruption run exposed a real orchestration defect: a marker tail had
to be recovered before retrying publication. The fixture now performs that
recovery explicitly.

The preceding GitHub run exposed a separate stale lowerer-rejection golden
contract on both hosts. Decision 0540 had intentionally added `plan-status`,
`function`, and `detail`, but the fixed report hashes still described the old
one-field diagnostic. The native two-case owner now pins the structured reports
and passes locally in 0.610 seconds; compiler rejection behavior and output
preservation are unchanged.

The settled-tree changed-file verifier passed in 959.5 seconds. Its selected
native owners covered lowerer rejections (2 cases), database storage (13),
Project 2 workspace policy (8), and libraries (26), for 49 passing cases with
no native coverage gaps. Changed-file planning also passed all 27 general and
73 native planner cases. This is Windows development feedback; independent
dual-host conformance remains GitHub-owned.

The first dual-host run passed the repaired lowerer owner and every Windows
retirement case, then exposed a later Linux-only recovery failure. The shared
storage host retained a resize request in `rdi` across the platform call;
Windows preserves that register, while the System V ABI permits the Linux leaf
to reuse it for the file descriptor. The file was resized correctly but the
completed response reported the clobbered value as its storage length. The
host now reloads the immutable validated request before publishing the
response. The focused 13-case database-storage owner passed in 962.4 seconds,
including deterministic Windows and Linux image construction; Linux execution
confirmation remains with the next GitHub run.

## Consequences

- Windvale Database can now publish and read its first durable multi-page tree
  without a .NET or C# normal-path dependency.
- Commit batching is reusable by later branch rewrites and split propagation;
  the storage action count does not grow with the number of contiguous pages.
- Strictly descending child identities intentionally depend on immutable
  append-only, bottom-up construction. A future reclamation or page-reuse
  contract must replace or re-prove this acyclicity rule.
- Root split is real but intentionally narrow. Updating an existing depth-two
  tree, propagating a child split, reclaiming obsolete pages, and pinning reader
  snapshots remain separate milestones.
- The ordinary pinned `Build-Wvb` product predates the nested-record/storage
  closure required by these newer library roots. Their native source and
  execution evidence is current, but standalone ordinary-front-door promotion
  remains compiler-product work rather than a database semantic dependency.

## Reconsideration triggers

Revisit the 32-level lookup bound or 63-page batch only with measured workloads
and exact memory/I/O evidence. Version or replace the routing contract before
adding a different separator convention. Revisit descending child identities
before any allocator can reuse pages or create a parent before its children.
