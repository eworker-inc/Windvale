# Decision 0798: Require full backup and restore before WVDB 1.0 production

- Date: 2026-08-20
- Status: Accepted direction; manifest and operation specifications pending
- Builds on: [Decision 0795](0795-Accept-WVDB-1.0-Profiles-And-Storage-Organization.md)
- Review: [backup and restore](../Project/WVDB-1.0-Types-Sizes-Documents-Graphs-And-Backup.md)

## Context

Backup and restore affect snapshot retention, page reclamation, database
identity, format upgrades, integrity, encryption boundaries, derived indexes,
and operational recovery. Postponing their design until after the storage
format is frozen risks incompatible ownership and lifecycle assumptions.

Scheduling interfaces, cloud-provider placement, continuous archival, and
advanced incremental policy need not be implemented with the first storage
slice.

## Decision

Specify backup and restore during WVDB 1.0 storage design. Require a qualified
full backup and restore path before a WVDB 1.0 service is production-ready.

The required contract is:

- backup reads one exact committed snapshot;
- output is streamed in bounded chunks;
- one versioned manifest identifies the database, format, committed
  generation/sequence, logical profiles, lengths, checksums, and chunk order;
- canonical entities, relationships, schemas, constraints, and required
  transaction metadata are mandatory;
- reconstructible indexes and asynchronous projections may be omitted only
  when the manifest requires their rebuild;
- completion means every required chunk and manifest reached the backup
  provider's specified durable state;
- rejection, exact partial progress, completion, and indeterminate completion
  remain distinct;
- restore writes new empty storage, validates all bytes and invariants, reopens
  the database, and completes integrity checks before activation;
- restore never overwrites a live database in place;
- disaster recovery explicitly preserves database identity, while clone
  explicitly creates a new identity and records lineage;
- logical export/import is a separate migration and inspection contract;
- compression, encryption, and signatures use explicit capabilities and
  established providers rather than custom cryptography; and
- qualification includes automated restoration and verification of canonical
  data, relationships, constraints, and required indexes.

The first backup manifest reserves lineage and base-generation fields.
Incremental backup, point-in-time recovery, remote replication, and continuous
archival remain later optional contracts.

## Consequences

- Snapshot and reclamation specifications must account for backup readers now.
- A backup claim requires restoration evidence, not only successful byte output.
- Derived state can be rebuilt without being mistaken for canonical truth.
- Restore and clone have different identity semantics.
- The production gate grows, while early storage implementation may still
  proceed before the complete operational tooling exists.

## Reconsideration triggers

Revisit the required physical backup shape if the selected canonical storage
cannot be streamed consistently, or promote incremental and point-in-time
recovery when accepted recovery-time and recovery-point objectives require
them.
