# Decision 0577: Canonical logical database records

- Date: 2026-08-15
- Status: Implemented candidate with focused Windows execution evidence
- Requires: [Decision 0575](0575-Single-Writer-Database-Engine-Lifecycle.md)
- Defines: [logical database records](../../Specifications/Windvale-Database-Logical-Records.md)
- Retains: `WVTN 1` byte ordering and limits, explicit engine projections,
  single-writer reopen, and no uncertain mutation replay

## Context

The durable engine can now open, recover, read, and commit byte keys and byte
values, but a server still needs one deterministic boundary between client
record identities and physical tree bytes. Letting each caller invent its own
prefixes, schema markers, or length handling would make catalogs, indexes, and
future E-Worker integration mutually incompatible.

The next layer must remain portable. It must not acquire the storage provider,
hide the segmented read/write projections, or claim that collection metadata
and schemas are implemented merely because keys can reserve their namespace.

## Decision

- Add version-1 `WVKR` keys with exact collection and record kinds.
- Use nonzero `u64` collection identities and opaque bounded record identities.
- Reserve an exact collection key as the future catalog metadata anchor.
- Add version-1 `WVRD` values with nonzero schema identity and opaque payload.
- Match the current 4,096-byte `WVTN 1` key and 61,440-byte value bounds.
- Require exact little-endian headers, zero flags/reserved fields, exact length,
  and owned admitted output.
- Add typed get and put preparation that returns no operation bytes on failure.
- Keep I/O in the existing ready-engine read and write projections.
- Add deterministic, boundary, truncated, trailing, and corrupted-input native
  coverage as one portable database-storage target.

## Evidence

The portable module and fixture compile through the native project front door.
The focused database development owner covers the logical target alongside the
existing engine, reader, writer, and recovery targets. Independent Linux
execution and the cold paired-host retirement gate remain qualification
evidence.

## Consequences

- Server and future E-Worker adapters can share exact record key/value bytes.
- Collection anchors can later store catalog descriptors without changing the
  record-key namespace.
- Schema identity is explicit without treating payload bytes as executable or
  claiming schema validation.
- Database-storage grows from 21 to 22 retirement cases and from twelve to
  thirteen development targets.
- With the independently added installation-dispatch owner, the rebased
  retirement manifest contains 66 suites and grows from 3,501 to 3,502 cases.
- Catalog descriptors, database creation, delete semantics, sessions, query
  planning, networking, authentication, and reclamation remain later work.

## Reconsideration triggers

Use a new version or explicit secondary encoding if cross-collection numeric
range ordering becomes required; do not reinterpret the little-endian version-1
bytes. Define catalog and schema migration before stored metadata can evolve.
Add mutation identity before any automatic retry or externally replayable put.
