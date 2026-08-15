# Decision 0579: Persistent collection catalog

- Date: 2026-08-15
- Status: Implemented candidate with focused Windows execution evidence
- Requires: [Decision 0577](0577-Canonical-Logical-Database-Records.md)
- Defines: [database collection catalog](../../Specifications/Windvale-Database-Collection-Catalog.md)
- Retains: exact engine projections, single-writer reopen, no uncertain replay,
  and explicit separation between upsert and atomic creation

## Context

`WVKR 1` reserves one collection key for each stable numeric identity, but the
server still needs a deterministic persisted value that names the collection
and selects its primary record schema. Without it, clients would have to carry
out-of-band conventions and future schema/catalog work could not validate that
a key and descriptor refer to the same collection.

The current writer is an upsert. Calling a prepared mutation “create” would
incorrectly promise absence testing and atomic name uniqueness that the engine
does not yet implement.

## Decision

- Add a version-1 `WVCL` descriptor stored at a `WVKR 1` collection key.
- Persist nonzero collection and primary-schema identities.
- Persist one through 256 exact valid UTF-8 name bytes without host locale or
  implicit Unicode normalization.
- Require exact little-endian header fields, zero flags/reserved fields, exact
  lengths, and owned admitted bytes.
- Add typed read and put preparation with no operation bytes on failure.
- Decode key and value independently and reject collection identity mismatch.
- Name the mutation put because it may replace an existing descriptor.
- Add deterministic, maximum-boundary, invalid-UTF-8, truncated, trailing,
  corrupted-field, and identity-mismatch native coverage.

## Evidence

The catalog project compiles through the native front door as 36 aligned
functions and a 25,629-byte WVB. The focused database development owner runs
the portable catalog target with the existing record, engine, reader, writer,
and recovery targets. Independent Linux execution and the cold paired-host
retirement gate remain qualification evidence.

## Consequences

- The future server can persist a stable collection name and primary schema at
  the already reserved collection anchor.
- Catalog admission cannot silently accept a descriptor under the wrong key.
- Database-storage grows from 22 to 23 retirement cases and from thirteen to
  fourteen development targets.
- With the independently added offline lifecycle owner, the rebased retirement
  manifest remains 67 suites and grows from 3,526 to 3,527 cases.
- Atomic create-if-absent, name lookup/uniqueness, schema bodies, migration,
  deletes, sessions, queries, and database bootstrap remain later milestones.

## Reconsideration triggers

Define an explicit normalized-name index before server APIs promise lookup or
uniqueness by human name. Add conditional mutation or a transaction predicate
before exposing create-if-absent. Use a new version for flags or fields whose
meaning cannot be added without changing version-1 admission.
