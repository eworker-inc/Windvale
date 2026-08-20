# WVDB 1.0 types, sizes, documents, graphs, and backup

- Date: 2026-08-20
- Status: Accepted design direction; normative encodings, algorithms, and
  conformance fixtures remain specification work
- Accepted decisions: [entity/table foundation](../Decisions/0792-Accept-The-WVDB-1.0-Entity-And-Table-Foundation.md),
  [relationships/integrity](../Decisions/0793-Accept-WVDB-1.0-References-And-Typed-Relationships.md),
  [indexes/queries/transactions](../Decisions/0794-Accept-WVDB-1.0-Index-Query-And-Transaction-Direction.md),
  and [profiles/storage](../Decisions/0795-Accept-WVDB-1.0-Profiles-And-Storage-Organization.md)
- Accepted types/sizes: [Decision 0796](../Decisions/0796-Accept-WVDB-1.0-Field-Types-And-Size-Ceilings.md)
- Accepted documents/graphs: [Decision 0797](../Decisions/0797-Keep-Documents-In-WVDB-And-Support-Basic-Typed-Graphs.md)
- Accepted backup/restore: [Decision 0798](../Decisions/0798-Require-Full-Backup-And-Restore-Before-WVDB-1.0-Production.md)
- Program: [WVDB 1.0 specification plan](WVDB-1.0-Specification-Plan.md)
- Current implementation review: [entities, tables, relationships, and indexes](Windvale-Database-Tables-And-Indexes.md)

## Outcome

WVDB should support ordinary business and knowledge applications directly,
without pretending that one row, document, or database can grow without bound.
The accepted 1.0 type family covers exact business values, scientific
numbers, time, identifiers, text, and binary data. Specialized spatial, vector,
full-text, and arbitrary-precision types remain later profiles.

The accepted size model distinguishes:

- the mathematical address range of a format;
- the WVDB 1.0 format ceiling;
- the smaller size actually qualified by a server profile;
- a database's configured quota;
- aggregate table/entity-set size; and
- one row, relationship, document, key, or external object.

Documents are a WVDB-owned profile in the same database service, not a
separate microservice. The accepted initial 1.0 conformance scope does not yet
require that profile. Large media and opaque files remain in a separate
object-storage capability and are referenced from WVDB.

Basic typed graphs are part of the accepted 1.0 relationship profile. General
graph algorithms and an unrestricted graph query language are not.

Backup and restore must be designed now because they constrain snapshots,
reclamation, format identity, checksums, encryption boundaries, and derived
indexes. Complete implementation can follow the core storage work, but a tested
full backup and restore path is required before WVDB 1.0 is declared
production-ready.

## Current implemented-candidate limits

The present formats are implementation evidence, not final 1.0 limits:

| Boundary | Current candidate |
| --- | --- |
| Field kinds | Boolean, I64, U64, UTF-8 text, bytes |
| Fields per schema | 1 through 64 |
| Complete typed row | At most 61,408 bytes |
| Logical record value | At most 61,440 bytes |
| Record or tree key | At most 4,096 bytes |
| Strict JSON value | At most 65,536 bytes |
| Page size | One of 4, 8, 16, 32, or 64 KiB |
| Page identity, page count, and storage length | Unsigned 64-bit |
| Mutation set | At most 32 mutations and 256 KiB |
| Implemented transaction input depth | At most eight |

The unsigned 64-bit storage arithmetic has a mathematical byte ceiling just
below 16 EiB. That is not a supported database-size claim. No production
database size has yet been qualified, and reclamation, backup, restore,
concurrent readers, and long-running service behavior are incomplete.

## Accepted table field types

Nullability is a field constraint, not a separate type. Every admitted value
also needs an exact canonical encoding and comparison contract.

### Required scalar and bounded-value families

| Type family | Accepted members or parameters | Primary uses | Important semantic work |
| --- | --- | --- | --- |
| Boolean | Bool | Flags and exact two-state values | Only true or false; nullable remains separate |
| Signed integer | I8, I16, I32, I64 | Counts, signed measurements, balances stored in minor units | Checked conversion and specified overflow |
| Unsigned integer | U8, U16, U32, U64 | Sizes, counters, masks, nonnegative quantities | Checked conversion and specified overflow |
| Exact decimal | Decimal(precision, scale), 128-bit coefficient, maximum precision 38 | Money, tax, rates, measurements requiring decimal accuracy | Rounding mode, overflow, division, comparison, and scale conversion |
| Binary floating point | F32, F64 | Scientific, statistical, sensor, and graphics values | Canonical NaN, signed zero, ordering, and deterministic encoding |
| Primary identity | Id128 | WVDB-generated opaque entity identities | Allocation, collision, formatting, privacy, import, and retry |
| Text | Text(max_utf8_bytes) | Names, descriptions, codes, messages | Strict UTF-8; binary ordering first; named collations later |
| Binary | Bytes(max_bytes) | Hashes, encrypted values, compact payloads | Bytewise equality/order; large media remains external |
| Date | Date | Calendar dates, accounting periods, birthdays | Exact calendar and range |
| Time of day | Time_of_day | Wall-clock schedule values without a zone | Precision, range, and no implied date or zone |
| Instant | Instant | Creation, expiry, event, and audit timestamps | Exact epoch, precision, range, and UTC meaning |
| Duration | Duration | Timeouts, elapsed time, retention, and intervals | Unit, range, arithmetic, and overflow |
| Enumeration | Enum(type, stable_member_id) | Status, category, workflow state | Stable member identity across rename and schema versions |

The accepted first core does not inherit host date, locale, floating-point
formatting, Unicode normalization, time-zone database, or decimal behavior.
Those semantics must be Windvale-owned and versioned.

### Profile or later-extension types

| Type | Why it is separate |
| --- | --- |
| Document, List, Map, nested Record | Require a document/composite profile with path, depth, node, schema, and update semantics |
| Arbitrary-precision integer or decimal | Unbounded values conflict with predictable allocation and work; a separately bounded family may follow |
| Zoned date-time | Named civil zones require a versioned time-zone rules source and update policy |
| Full-text value/index | Tokenization, language, scoring, normalization, and freshness are not ordinary text ordering |
| Geospatial geometry/index | Coordinate reference systems, validity, precision, and spatial predicates need their own profile |
| Vector/embedding | Dimension, element type, distance function, approximate search, and freshness need a semantic-index profile |
| Large blob/object | Streaming, chunking, content identity, range reads, retention, and independent lifecycle need an object-storage contract |

A reference is not a new scalar kind. It is a constraint over fields whose
types and canonical encoding match a target primary identity.

## Application suitability

This table describes the accepted type and profile direction. It is not a
performance claim for the unfinished implementation.

| Application family | Expected fit | Why |
| --- | --- | --- |
| CRM, ERP, inventory, order management | Strong | Exact identities, decimals, dates, constraints, references, indexes, and transactions cover the central model |
| Accounting, invoicing, billing | Strong after decimal qualification | Exact decimal, immutable identity, audit instants, and transactional constraints avoid floating money |
| Workflow, issue tracking, project management | Strong | Enums, relationships, text, dates, indexes, and bounded transactions fit naturally |
| Identity directory and authorization metadata | Strong for metadata | IDs, bytes, relationships, expiry instants, and constraints fit; secrets still require application security and key-management boundaries |
| Package, compiler, and system catalogs | Strong | Strict schemas, byte identities, exact versions, relationships, and deterministic queries match Windvale tooling |
| Knowledge base and dependency/lineage graph | Strong for basic connected data | Typed entities and relationships, properties, adjacency, and bounded traversal are accepted 1.0 direction |
| Ecommerce catalog and content metadata | Strong | Tables and relationships fit; bounded documents can later hold variable product attributes |
| CMS and document-centric applications | Moderate initially | Text and metadata work now; nested document updates wait for the document profile and large media stays external |
| Messaging, jobs, and event workflow | Moderate | Ordering, transactions, and instants fit; very high write concurrency, retention compaction, and streaming delivery need later work |
| Logs, metrics, and time-series | Moderate at bounded scale | Numeric and instant types fit; partitioning, compression, columnar scans, and high-ingest concurrency are not initial strengths |
| Scientific or telemetry data | Moderate | Floating point and instants fit after qualification; large arrays and analytical execution remain later |
| Data warehouse and large OLAP | Difficult initially | No required columnar profile, distributed scan, broad aggregates, or materialized analytical engine |
| Full-text search engine | Difficult initially | Binary text ordering is not tokenization, ranking, stemming, or language-aware search |
| AI vector retrieval | Difficult initially | No required vector type, distance contract, approximate index, or freshness model |
| GIS and routing | Difficult initially | No coordinate-system, geometry, spatial-index, or route algorithm profile |
| Large media/file repository | Poor fit for inline storage | Database records should store metadata, content hash, rights-limited object reference, and lifecycle state; media bytes belong in object storage |
| Globally distributed multi-writer service | Not a 1.0 target | The accepted first profile uses one serialized writer and does not claim replication, sharding, or consensus |

## Accepted size model

### Database and entity-set sizes

| Boundary | Accepted target | Meaning |
| --- | ---: | --- |
| Mathematical address range | Less than 16 EiB | Existing unsigned 64-bit arithmetic ceiling; not a support claim |
| WVDB 1.0 format ceiling | 1 PiB per database | Hard rejection boundary for the first complete format |
| First hosted server supported ceiling | 16 TiB per database | Largest database shape the first server profile must admit, recover, inspect, and stream through backup tools; full-capacity performance still requires measured evidence |
| Default database quota | 1 TiB | Explicit configurable deployment policy, not a format limit |
| One entity set/table/relationship set | Up to its database quota | No smaller artificial total-size ceiling; an owner may set a per-set quota |
| Entity count | Advertised bounded count within storage quota | Never marketed as unlimited; exact count and catalog limits remain specification work |

A table can therefore be the largest object in one database. Quotas, page
counts, reclamation pressure, backup time, and index growth provide the real
operational bounds.

### Individual values and records

| Boundary | Accepted WVDB 1.0 maximum | Reason |
| --- | ---: | --- |
| Encoded primary or secondary index key | 4 KiB | Retains bounded comparison and the current ordered-tree evidence |
| Table fields | Portable profile supports at least 256; hard format maximum 1,024 | Supports ordinary wide schemas without unbounded validation |
| Indexes per table | Portable profile supports at least 64; hard maximum 256 | Replaces the prototype eight-index limit while bounding write amplification |
| Key components per index | Portable profile supports at least 16; hard maximum 32 | Covers compound business keys without unbounded comparison |
| One table row | 1 MiB canonical encoded | Keeps transactional rows bounded; requires overflow/chunk storage above one page |
| One relationship record | 1 MiB canonical encoded | Allows properties while discouraging relationships as blob containers |
| One document | 16 MiB canonical encoded | Supports substantial nested application documents with bounded decode and update work |
| One inline text or byte field | Limited by its containing row/document | Avoids a second contradictory inline limit |
| One query result batch | Advertised bounded batch, streamed by cursor | Complete result sets are not retained as one unbounded value |
| Large media or opaque object | Outside the WVDB record limit | Stored through a separate object-storage capability and referenced by identity/hash |

Rows and documents larger than one durable page require a versioned chunk or
overflow-value organization. The logical mutation remains atomic, but APIs
must stream large values and bound retained memory. No page, cache, diagnostic,
backup, or query operation may allocate the complete database or table.

Before normative format freezing, synthetic boundary tests and representative
CRM, accounting, catalog, knowledge, document, and time-series schemas must
measure page fanout, index amplification, transaction memory, backup
throughput, and restore time.

## Document storage

The accepted architecture keeps the table, typed relationship, and optional
document profiles inside one WVDB service and transaction domain. Large media
and opaque file bytes use a separate object-storage capability.

A WVDB document has primary identity, a versioned document type or admitted
open-shape policy, canonical nested values, constraints, path queries, path
indexes, transactions, authorization, backup, and lifecycle. JSON may be one
input/output language, but JSON text spelling is not the durable semantic
definition.

Keeping documents inside WVDB provides atomic updates between tables,
relationships, and documents and gives them one backup and authority boundary.
A separate document microservice would duplicate transactions, identity,
schema, authorization, and recovery before a real deployment requires that
separation.

The accepted Decision 0795 scope does not require the document profile for the
initial WVDB 1.0 conformance claim. Decision 0797 requires its extension
boundary to be specified now and permits implementation after strict tables and
typed relationships are correct. If the first real product requires nested
documents, Decision 0795 must be revisited explicitly rather than hiding
documents in a bytes or JSON field.

Large media, archives, model files, and other opaque objects use a
separate streaming object-storage capability. WVDB stores their stable object
identity, content hash, size, media type, ownership, authorization metadata,
and lifecycle state. Cross-service creation and deletion require an explicit
idempotent workflow; a database commit must not pretend that an external object
mutation was atomic.

## Backup and restore

Backup and restore must be specified now and implemented before the WVDB 1.0
production gate.

Designing them later could invalidate snapshot ownership, page reclamation,
format upgrade, encryption, database identity, and derived-index decisions.
Scheduling UI, cloud placement, retention policy, and advanced incremental
operation can wait.

### Accepted 1.0 contract

- A full backup reads one exact committed snapshot without blocking bounded
  ordinary reads.
- The backup is streamed in bounded chunks and carries a versioned manifest,
  database identity, format version, committed generation/sequence, logical
  profile inventory, chunk sizes, checksums, and total lengths.
- Canonical entities, relationships, schemas, constraints, and required
  transaction metadata are mandatory.
- Reconstructible indexes and asynchronous projections may be omitted when the
  manifest says they must be rebuilt. A backup must never omit canonical truth.
- Completion means every chunk and the manifest reached the backup provider's
  specified durable state. Rejection, partial progress, completion, and
  indeterminate completion remain distinct.
- Restore writes new empty storage, validates every byte and invariant, reopens
  the restored database, and runs integrity checks before activation. It never
  overwrites a live database in place.
- Disaster recovery may preserve database identity. A clone operation creates a
  new identity and records lineage. The caller selects the mode explicitly.
- A logical export/import contract is separate from physical backup and is used
  for migration, inspection, and selected interoperability.
- Compression, encryption, and signatures use explicit versioned capabilities
  and established cryptographic providers; checksums alone are not
  authentication.
- A backup feature is not qualified until automated restore tests prove the
  resulting database, relationships, and required indexes.

The first production profile requires full backup and restore. Incremental
backup, point-in-time recovery, remote replication, and continuous archival can
follow, but the initial manifest reserves lineage and base-generation
fields so they do not require an incompatible redesign.

## Graph support

WVDB 1.0 supports a **basic typed graph** through entities and first-class
relationships:

- an entity is a node with one primary identity and one entity type;
- a relationship has its own primary identity, relationship type, source,
  target, direction, and optional properties;
- relationship types may enforce endpoint types, uniqueness, and cardinality;
- source and target adjacency indexes update synchronously with the
  relationship mutation; and
- queries can perform exact relationship lookup, incoming/outgoing adjacency,
  type filtering, and explicitly depth/result/work-bounded traversal.

This supports knowledge graphs, dependency graphs, lineage, organizational
connections, permission relationships, workflows, and moderate social/product
graphs.

The initial profile does not claim unrestricted recursive queries, arbitrary
pattern language, shortest-path optimization, centrality/community algorithms,
RDF or another graph-language compatibility, distributed graph partitioning,
or graph-scale analytical execution. Those features need a separate graph
query and analytical profile with exact work, memory, cancellation, and
freshness behavior.

Tables and graph relationships may connect the same canonical entities. WVDB
does not duplicate a table row into a second authoritative graph node merely to
support traversal.

## Accepted decisions and specification follow-up

Decisions 0796 through 0798 accept:

1. the required table field type families;
2. the 1 PiB format ceiling and 16 TiB first hosted supported ceiling;
3. 1 MiB rows/relationships, 16 MiB documents, and 4 KiB index keys;
4. documents as an in-database optional profile, with large opaque objects in a
   separate storage capability;
5. full backup and restore as a required WVDB 1.0 production gate; and
6. basic typed relationships and bounded traversal as the WVDB 1.0 graph
   contract.

The next work is normative: exact type bytes and operations, overflow-value
storage, database-limit admission, document encoding, traversal limits, backup
manifest bytes, restore state transitions, and conformance fixtures.
