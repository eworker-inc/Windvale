# Decision 0795: Accept WVDB 1.0 profiles and storage organization

- Date: 2026-08-20
- Status: Accepted direction; normative storage formats and profile details
  pending
- Accepts: recommendations Q17 through Q20 in the
  [upper-layer decision register](../Project/WVDB-1.0-Upper-Layer-Decision-Register.md)
- Builds on: [Decision 0793](0793-Accept-WVDB-1.0-References-And-Typed-Relationships.md)

## Context

WVDB may become a knowledge-system foundation, but requiring every possible
document, analytical, search, and graph feature in the first release would
prevent a coherent 1.0. One physical layout is also unlikely to serve point
updates, adjacency, scans, and semantic search equally well.

## Decision

The required WVDB 1.0 product scope is:

- the shared database, entity, identity, transaction, authority, versioning,
  diagnostics, and operations core;
- the strict table profile; and
- a basic typed relationship profile with synchronous endpoint adjacency and
  integrity.

Document, analytical, full-text, geospatial, and vector/semantic profiles
remain WVDB-owned extension boundaries but are not required for the initial
WVDB 1.0 conformance claim.

Each admitted entity or relationship has one canonical transactional truth.
Indexes and projections are declared derived organizations. The canonical
organization does not have to remain the current ordered tree forever, and a
later profile may admit another authoritative organization through an explicit
format and transaction contract.

Logical primary identity is independent from physical organization. A format
records and validates its selected organization. Qualify one simple
organization first; add heap, clustered, columnar, adjacency, or other layouts
only from measured profile requirements.

Ordered indexes used for uniqueness, references, relationships, or normal
table access remain synchronous with canonical mutation. An analytical,
full-text, or semantic projection may be asynchronous only when its profile
exposes exact freshness, checkpoint, query, rebuild, failure, and resource
semantics.

## Consequences

- WVDB 1.0 can be useful for tables and connected knowledge without claiming
  every future profile.
- Basic graph-shaped applications can use typed relationships and bounded
  adjacency queries.
- Specialized projections can evolve without becoming competing sources of
  truth.
- Backup, restore, reclamation, and migration must distinguish canonical state
  from reconstructible derived state.
- Full document-database, graph-algorithm, analytical warehouse, and semantic
  search claims require later profile specifications and conformance evidence.

## Reconsideration triggers

Revisit the required 1.0 profile set if the first accepted product cannot be
useful without a document profile, or if a measured workload requires a
specialized authoritative organization rather than canonical records plus
derived access paths.
