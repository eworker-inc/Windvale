# Decision 0575: Single-writer database engine lifecycle

- Date: 2026-08-15
- Status: Implemented candidate with focused Windows execution evidence
- Requires: [Decision 0572](0572-Provider-Driven-Durable-Tree-Writer.md)
- Defines: [database engine lifecycle](../../Specifications/Windvale-Database-Engine-Lifecycle.md)
- Retains: `WVDS 1`, exact provider generation/length checks, explicit recovery
  stages, no uncertain replay, and separate static read/write projections

## Context

The hosted reader and writer both accepted a durable superblock selection, but
their callers still duplicated storage description, header admission, current
selection, tail recovery, and reopen validation. A future server needs one
canonical lifecycle state before it can safely associate operations with a
session or expose records and queries.

Directly combining the complete reader and writer closures in one current
native object exceeds the ordinary 4 MiB object bound. Raising that safety
limit or introducing runtime module linking merely to make the facade appear
monolithic would broaden this milestone without improving its semantics.

## Decision

- Add one hosted engine open operation over `storage.random_access_v1`.
- Return provider generation/length, exact current selection, recovery stage
  and error, and exact executed-action count as immutable evidence.
- Require the header read to match the initial provider observation.
- Distinguish clean ready, completed recovery, active recovery, required reopen,
  storage/header/current failure, changed storage, and recovery failure.
- After tail repair, reopen and prove every committed selection field remains
  identical except that tail bytes become zero.
- Permit reader and writer operations only as projections of a ready or
  recovered `Engine.Current`; reopen after a committed mutation.
- Keep reader and writer in separate native targets under their existing common
  engine-state contract until a qualified linker/runtime boundary supports
  composition within the object limit.
- Add focused ready, partial-recovery, completed-recovery, invalid-header, and
  engine-backed lookup evidence.

## Evidence

The engine target compiler-aligns 146 functions and lowers to a 3,439,032-byte
ordinary native object. Focused Windows execution opens the exact depth-two
generation without mutation, reads through the selected engine snapshot,
retains explicit resize and flush states after zero and one actions, converges
byte-identically after two recovery actions, and rejects a 511-byte header.

The database development owner passes twelve targets with the new engine
project and hosted application checkpoints created. Independent Linux execution
and the cold paired-host retirement gate remain qualification evidence.

## Consequences

- The server can depend on one exact open/recovery state instead of duplicating
  lifecycle logic across commands.
- Read and write share the same admitted current selection even while their
  native images remain physically segmented.
- Database-storage grows from 20 to 21 retirement cases and from eleven to
  twelve development targets.
- The complete retirement manifest remains 65 suites and grows from 3,491 to
  3,492 cases.
- Database creation, logical records, catalogs, sessions, networking,
  authentication, concurrent writers, and reclamation remain later milestones.

## Reconsideration triggers

Revisit physical segmentation after qualified runtime linking can preserve the
same lifecycle and capability contracts below explicit size limits. Add
snapshot pinning before concurrent readers can outlive a provider generation.
Any automatic retry requires a separately specified idempotency and uncertain-
completion protocol.
