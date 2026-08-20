# Decision 0793: Accept WVDB 1.0 references and typed relationships

- Date: 2026-08-20
- Status: Accepted direction; normative relationship formats and query behavior
  pending
- Accepts: recommendations Q8 through Q11 in the
  [upper-layer decision register](../Project/WVDB-1.0-Upper-Layer-Decision-Register.md)
- Builds on: [Decision 0791](0791-Use-One-Explicit-Primary-Identity-Per-WVDB-Entity-Set.md)

## Context

Table references and graph relationships both connect durable identities, but
they answer different user questions. WVDB also needs exact initial deletion
and null-uniqueness behavior.

## Decision

Define reference constraints and first-class relationships as distinct public
contracts that share primary-identity lookup, transactions, constraints,
indexes, authority, and diagnostics.

A table reference constraint binds one or more fields to a target entity set's
primary identity. The first profile validates references immediately and
rejects deletion of a referenced target with `RESTRICT`. It does not initially
support cascades, deferred checks, mutable referenced identities,
cross-database references, or references to non-primary unique keys.

A first-class relationship is durable typed data with one explicit primary
identity, source identity, target identity, direction, relationship type, and
optional admitted properties. Its relationship type may additionally declare
source/type/target uniqueness, cardinality, and endpoint-type constraints. This
supports both set-like links and parallel event-like relationships.

Every durable unique constraint declares one null policy:

- `Exclude`: a row with any null key component is absent from the constraint
  index;
- `Distinct`: null-bearing keys do not conflict solely because null appears
  in the same position; or
- `Not_distinct`: null participates as an equal key component.

Persisted metadata never relies on an unstated null-uniqueness default.

## Consequences

- Tables receive centrally enforced reference integrity without paying the
  complete graph surface.
- Knowledge and graph applications can attach properties and lifecycle to a
  relationship itself.
- Immediate `RESTRICT` keeps initial mutation work and failure behavior
  understandable but requires applications to order explicit deletions.
- Parallel relationships are supported when their type permits them.
- Relationship adjacency and constraint indexes become synchronous
  transaction participants.

## Reconsideration triggers

Revisit this direction when an accepted workload requires cascades, deferred
constraints, references to candidate keys, cross-database relationships, or a
relationship representation that cannot share the selected transaction and
index machinery.
