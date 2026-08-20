# WVDB 1.0 specification plan

- Date: 2026-08-20
- Status: Active specification program; this plan is non-normative
- Product target: [Windvale 1.0](Windvale-1.0-Product-Plan.md)
- Direction: [Decision 0790](../Decisions/0790-Define-WVDB-1.0-As-A-Windvale-Owned-Database.md)
- Accepted identity: [Decision 0791](../Decisions/0791-Use-One-Explicit-Primary-Identity-Per-WVDB-Entity-Set.md)
- Accepted upper-layer choices: [decision register](WVDB-1.0-Upper-Layer-Decision-Register.md)
- Accepted decision groups: [entities/tables](../Decisions/0792-Accept-The-WVDB-1.0-Entity-And-Table-Foundation.md),
  [relationships/integrity](../Decisions/0793-Accept-WVDB-1.0-References-And-Typed-Relationships.md),
  [indexes/queries/transactions](../Decisions/0794-Accept-WVDB-1.0-Index-Query-And-Transaction-Direction.md),
  and [profiles/storage](../Decisions/0795-Accept-WVDB-1.0-Profiles-And-Storage-Organization.md)
- First upper-layer review: [entities, tables, relationships, and indexes](Windvale-Database-Tables-And-Indexes.md)
- Types, sizes, profiles, and recovery review:
  [types, sizes, documents, graphs, and backup](WVDB-1.0-Types-Sizes-Documents-Graphs-And-Backup.md)
- Accepted type/size decision:
  [Decision 0796](../Decisions/0796-Accept-WVDB-1.0-Field-Types-And-Size-Ceilings.md)
- Accepted document/graph decision:
  [Decision 0797](../Decisions/0797-Keep-Documents-In-WVDB-And-Support-Basic-Typed-Graphs.md)
- Accepted backup/restore decision:
  [Decision 0798](../Decisions/0798-Require-Full-Backup-And-Restore-Before-WVDB-1.0-Production.md)
- Implementation language: Windvale Language 1.0

## Purpose

This program turns the developing Windvale database implementation into a
coherent WVDB 1.0 specification set before a long implementation phase begins.
The goal is a proper database system with useful user-visible models, exact
durability and authority boundaries, reproducible formats, and conformance
evidence. Current code and formats are candidates and evidence; they do not
silently define 1.0.

The design proceeds from upper layers to lower layers for product meaning, then
checks the complete vertical path before accepting each contract:

```text
user models and operations
  -> logical database contracts
  -> transactions, queries, and access paths
  -> storage organizations and durable formats
  -> rights-limited platform providers
```

Starting with the upper layer answers what people and applications can express.
Lower-layer work then selects storage organizations that preserve those
semantics with bounded time, memory, I/O, and recovery behavior.

## Public positioning

WVDB is a Windvale-owned database system. It is not a rewrite, successor,
compatibility layer, or substitute claim for another named database. Active
WVDB 1.0 documents should be understandable without access to external source
code or product history.

Comparisons with established databases are comparative research. They should
use official current documentation, state the review date, select systems for
distinct architectural lessons, and describe both benefits and costs. Avoid
marketing language, feature-count contests, unsupported performance claims,
and wording that implies WVDB inherits another product's semantics.

Historical project documents may retain their original provenance and context
when clearly marked as historical or superseded. They are not part of the
active WVDB 1.0 specification set.

## Candidate logical foundation

A class, table, document, or graph node is a convenient user model, not
necessarily the universal database primitive. Regardless of spelling, durable
data usually needs identity, admitted shape, constraints, access paths,
relationships, migration, authority, and lifecycle.

The candidate shared vocabulary is:

- **database**: one independently identified catalog, transaction, durability,
  and authority domain;
- **namespace**: a bounded naming domain inside one database;
- **entity type**: one immutable versioned shape and constraint definition;
- **entity set**: one named population admitted by an entity type and identity
  rule;
- **entity**: one durable typed value with stable identity;
- **relationship type**: one versioned source, target, direction, cardinality,
  property, and integrity contract;
- **relationship**: one durable typed connection between entity identities;
- **index**: one derived access path with explicit consistency, ordering,
  lifecycle, and resource contracts;
- **view**: one named derived query contract; and
- **transaction**: one atomic mutation and observation boundary over admitted
  logical models.

Decision 0792 accepts this vocabulary as the logical foundation, but not as
final source syntax. The table profile projects an entity set as a table, an
entity type as a table schema, an entity as a row, and the identity rule as a
primary key. The relationship profile makes connections first-class and
property-bearing. A later document profile may admit nested values without
pretending that every nested value is a separate table. Model-specific behavior
remains explicit.

## Candidate model profiles

### Table profile

Typed rows, columns/fields, primary keys, unique constraints, reference/foreign
key constraints, check constraints, ordered indexes, set queries, joins, views,
and schema evolution. This is the first upper-layer design focus because the
current implementation already has the strongest evidence here.

### Document profile

Typed or schema-admitted nested objects, arrays, optional fields, path queries,
document identity, document-local atomicity, and explicit reference behavior.
Embedding and cross-document references must remain deliberate choices.

### Graph and relationship profile

Typed nodes/entities, first-class directed relationships, relationship
properties, cardinality and existence constraints, adjacency traversal, path
queries, and graph-specific indexes. A table foreign key and a stored graph
relationship may share integrity mechanisms without being declared identical.

### Analytical profile

Columnar or segmented projections, scans, aggregates, statistics, and
materialized derived state. An analytical projection may be maintained from
canonical transactional data rather than becoming a second source of truth.

### Search and semantic profile

Full-text, token, geospatial, vector, or other similarity indexes. These are
derived access structures with explicit freshness and scoring semantics, not
ordinary ordered indexes hidden behind the same operation names.

Decision 0795 requires the shared core, strict table profile, and basic typed
relationship profile for WVDB 1.0. Document, analytical, search, and semantic
profiles remain explicit WVDB-owned extension boundaries rather than required
1.0 conformance claims.

## Planned normative specification set

Create a normative file only when its questions have an accepted answer and an
owner. The candidate set is:

1. `Specifications/WVDB-1.0-System.md` — identity, profiles, versions, limits,
   portability, authority, and failure vocabulary.
2. `Specifications/WVDB-1.0-Types-And-Values.md` — scalar, text, binary, nested,
   null, comparison, ordering, collation, encoding, and evolution rules.
3. `Specifications/WVDB-1.0-Entities-And-Tables.md` — entity sets, tables,
   schemas, rows, primary identity, defaults, and table lifecycle.
4. `Specifications/WVDB-1.0-Relationships-And-Constraints.md` — references,
   foreign keys, first-class relationships, cardinality, check timing, and
   mutation consequences.
5. `Specifications/WVDB-1.0-Indexes.md` — ordered and specialized access paths,
   uniqueness, null behavior, lifecycle, consistency, and observability.
6. `Specifications/WVDB-1.0-Transactions-And-Snapshots.md` — isolation,
   conflicts, writer policy, readers, cancellation, idempotency, and recovery.
7. `Specifications/WVDB-1.0-Queries-And-Results.md` — typed query model,
   planning contract, cursors, limits, diagnostics, and result encoding.
8. `Specifications/WVDB-1.0-Catalog-And-Migration.md` — namespaces, stable
   identities, immutable versions, compatibility, online/offline transitions,
   and import/export boundaries.
9. `Specifications/WVDB-1.0-Storage-And-Reclamation.md` — page/segment/object
   organizations, allocation, caches, pins, reclamation, and resource charging.
10. `Specifications/WVDB-1.0-Durability-Integrity-And-Backup.md` — publication,
    power-loss model, verification, backup, restore, repair, and format upgrade.
11. `Specifications/WVDB-1.0-Service.md` — sessions, transport-independent
    operations, authentication, authorization, deadlines, and mutation status.
12. `Specifications/WVDB-1.0-Operations.md` — configuration, health, metrics,
    diagnostics, audit, quotas, upgrade, shutdown, and service supervision.
13. `Specifications/WVDB-1.0-Conformance.md` — valid, malformed, crash,
    cross-host, determinism, performance, memory, soak, and security evidence.

This list is an architecture map, not empty scaffolding authorization. Existing
database specifications remain implemented-candidate inputs until reconciled,
superseded, or incorporated by an accepted 1.0 contract.

## Specification sequence

Decisions 0791 through 0798 settle the first upper-layer direction. Normative
work should now proceed in this dependency order:

1. exact scalar types, time, decimal, identity generation, size ceilings, and
   configured limits;
2. entity sets, tables, stable fields, immutable schemas, projections,
   defaults, and lifecycle;
3. references, typed relationships, cardinality, adjacency, and bounded graph
   traversal;
4. ordered indexes, uniqueness/null policies, synchronous maintenance,
   capabilities, statistics, and lifecycle;
5. typed queries, WVDB SQL lowering, cursors, result limits, and diagnostics;
6. snapshots, serialized-writer transactions, conflicts, cancellation, and
   uncertain network mutations;
7. canonical storage, overflow values, derived projections, reclamation,
   caches, and resource quotas;
8. durability, full backup/restore, repair, security, service, and operations;
   and
9. exact WVDB 1.0 conformance and implementation plan.

Each discussion updates a project design candidate. Once its remaining details
are coherent, record the decision and create or update the corresponding
normative specification. Implementation can proceed in independently
executable vertical slices after the relevant contract is accepted.

## Specification gate before implementation

A WVDB 1.0 feature is ready for implementation only when its specification
states:

- user-visible behavior and exact terminology;
- platform scope, authority, required and optional capabilities;
- types, sizes, limits, ordering, null, and concurrency semantics;
- mutation completion, uncertainty, idempotency, and cancellation behavior;
- validation, malformed input, recovery, and lifecycle rules;
- time, memory, I/O, retained-state, and diagnostic bounds;
- versioning, compatibility, migration, and rejection behavior;
- conformance fixtures and the narrow verification owner; and
- deliberate exclusions and reconsideration triggers.

The gate prevents a format or implementation convenience from becoming public
semantics accidentally. It does not require every 1.0 specification to finish
before a well-bounded foundational slice can be implemented and verified.
