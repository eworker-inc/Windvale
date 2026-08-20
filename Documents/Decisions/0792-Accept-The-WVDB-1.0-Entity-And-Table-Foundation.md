# Decision 0792: Accept the WVDB 1.0 entity and table foundation

- Date: 2026-08-20
- Status: Accepted direction; normative type and format details pending
- Accepts: recommendations Q1 through Q7 in the
  [upper-layer decision register](../Project/WVDB-1.0-Upper-Layer-Decision-Register.md)
- Builds on: [Decision 0791](0791-Use-One-Explicit-Primary-Identity-Per-WVDB-Entity-Set.md)

## Context

WVDB needs one coherent logical foundation without forcing tables, documents,
and graphs to behave identically. Its first table specification also needs
durable field identity, schema evolution, a bounded type system, generated
identity, and atomic lifecycle behavior.

## Decision

Use a shared WVDB foundation for database identity, namespaces, entity types,
entity sets, entities, relationships, indexes, views, transactions,
capabilities, diagnostics, and versioning. Define tables, documents, graphs,
analytics, and search as explicit profiles over that foundation.

The strict table profile is the first complete profile. Not every profile must
ship merely because the shared vocabulary reserves its boundary.

Keep `collection` only as a provisional lower-engine term while current
candidate formats use it. It is not a WVDB 1.0 public model. The storage
specification will retain, rename, or remove it after physical organizations
are selected.

Give every field a stable nonzero identifier. A schema version owns the
field's current unique name and compact row ordinal. Persisted indexes,
constraints, defaults, and migrations bind stable field identities rather than
names or ordinals.

Schema versions are immutable. The initial compatible projection set may add a
nullable field, add a deterministic literal default, rename a field without
changing its identity, or increase an admitted text or byte limit. Other
changes require an explicit copy migration unless a later specification proves
another transition safe and bounded.

Define a small strict portable scalar core and separately versioned type
families. A type enters WVDB only with exact equality, ordering, encoding,
conversion, arithmetic where applicable, resource bounds, and evolution
semantics. Nested values belong to an explicit document or composite profile.

Provide one opaque WVDB-generated 128-bit primary-identity kind with no
application-visible chronological meaning. Applications may instead supply
their primary values. The exact random or sortable encoding, allocation,
collision, import, rollback, and uncertain-retry behavior remains normative
specification work.

Table create, rename, truncate, migration, and drop are named server-owned
operations. They atomically govern catalog, schema, constraint, and index
state and expose exact authority, progress, cancellation, recovery, and
uncertain-completion behavior.

## Consequences

- Tables remain familiar without becoming WVDB's universal storage shape.
- Field rename and row-layout changes do not silently break durable indexes.
- Old schema versions remain readable and diagnosable.
- Host types and callbacks cannot silently define database behavior.
- Generated identity supports applications without a stable natural key but
  adds a required 128-bit type and allocation contract.
- Atomic lifecycle operations enlarge the server surface while preventing
  partially created or partially removed tables.

## Reconsideration triggers

Revisit this direction if a qualified profile cannot share the foundation
without losing essential semantics, if stable field identifiers impose
disproportionate measured cost, or if the selected generated identity cannot
meet locality, privacy, import, and retry requirements together.
