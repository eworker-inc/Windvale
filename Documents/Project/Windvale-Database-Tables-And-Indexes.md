# WVDB 1.0 entities, tables, relationships, and indexes

- Date: 2026-08-20
- Status: Active WVDB 1.0 upper-layer design candidate; non-normative until
  its choices are accepted in decisions and specifications
- Windvale source state reviewed: `97a18d90`
- Scope: logical entities, tables, typed rows, identity, relationships,
  constraints, ordered indexes, and the minimum query-planning seam
- Direction: [Decision 0790](../Decisions/0790-Define-WVDB-1.0-As-A-Windvale-Owned-Database.md)
- Accepted identity: [Decision 0791](../Decisions/0791-Use-One-Explicit-Primary-Identity-Per-WVDB-Entity-Set.md)
- Program: [WVDB 1.0 specification plan](WVDB-1.0-Specification-Plan.md)
- Open choices: [upper-layer decision register](WVDB-1.0-Upper-Layer-Decision-Register.md)
- Current candidate inputs: [typed rows and schemas](../../Specifications/Windvale-Database-Typed-Rows-And-Schemas.md),
  [collection catalog](../../Specifications/Windvale-Database-Collection-Catalog.md),
  and [secondary indexes](../../Specifications/Windvale-Database-Secondary-Indexes.md)

## Outcome

Windvale already has useful table and index ingredients, but it does not yet
have one first-class table object or an executable indexed-query path. The
current storage model is:

```text
collection identity and descriptor
  -> one primary schema identity
  -> typed logical records with opaque record identities
  -> optional bounded secondary-index definitions
  -> pure primary-row and index mutation planning
```

The recommended shared logical model is:

```text
database
  -> named entity sets
  -> versioned entity types
  -> stable entity identities
  -> typed relationships
  -> constraints and derived access paths
```

The first concrete profile over that model should be a strict table:

```text
table
  = named entity set
  + immutable schema versions and one active write schema
  + one explicit primary-key definition
  + zero or more constraints and ordered secondary indexes
  + one atomic mutation and integrity boundary
```

`Collection` may remain an engine term if it survives the lower-layer review.
`Entity set` is the shared logical term. `Table` is the typed row-and-column
profile presented to applications and administrators. A document collection
or graph node set can use another profile without pretending to be a table.

The first useful table profile should retain deterministic binary types,
explicit resource limits, one ordered index family, and an exact writer policy.
It should be small because its own workloads are small enough, not because it
copies or deliberately opposes another product.

## Review basis

The WVDB review covers the current collection catalog, logical records, typed
schemas and rows, secondary-index formats and mutation planner, transaction
writer, query IR, and SQL lowerer. The comparative survey uses official
PostgreSQL, SQLite, MySQL/InnoDB, SQL Server, MongoDB, Neo4j, and DuckDB
documentation available on the review date. These are representative systems
chosen for different design lessons. The list is neither a compatibility set
nor an exhaustive ranking.

## Why the foundation should not be only tables or objects

Classes make common programming structures easier for humans, but a runtime
does not have to store a class exactly as written. Tables play a similar role:
they give people a compact way to declare repeated shapes, identities,
constraints, and set operations. Their benefits are real even if WVDB stores
pages, trees, columns, logs, or adjacency records underneath.

Renaming a table to an object does not remove the database problems. Durable
objects still need stable identity, admitted shape, uniqueness, relationships,
queryable fields, concurrent mutation rules, schema evolution, and deletion
behavior. Conversely, forcing every document or relationship into a table can
make nested values and graph traversal unnatural.

The candidate compromise is therefore:

- use entities, entity sets, identities, and typed relationships as shared
  logical concepts;
- make tables a first-class profile with rows, columns, keys, constraints, and
  set queries;
- permit later document and graph profiles to keep their distinct semantics;
  and
- allow multiple physical storage and index organizations behind explicit
  profile contracts rather than promising one universal representation.

This separation keeps familiar tools where they improve human work while
leaving room for a future Windvale knowledge system.

## Candidate upper-layer surface

| Concept | Why users need it | WVDB 1.0 candidate | Main cost or risk |
| --- | --- | --- | --- |
| Database and namespace | Own names, authority, transactions, and lifecycle | Required | Catalog and cross-namespace rules must stay bounded |
| Entity type/schema | State the admitted shape and meaning of data | Required, immutable and versioned | Evolution and old-version reads need exact rules |
| Entity set/table | Name and query a population of similarly governed values | Required table profile; other profiles may follow | One model must not be stretched over every workload |
| Stable identity/primary key | Address values, deduplicate, and connect relationships | Required | Key width and mutability affect every access path |
| Unique and check constraints | Reject invalid state at the owning boundary | Required in stages | Expression and validation work can become unbounded |
| Reference/foreign key | Preserve a valid typed reference between entity sets | Required relationship form | Cross-set reads, deletion, and cycles complicate commits |
| First-class relationship | Store direction, type, properties, and graph identity | Required basic typed relationship profile | Adjacency storage and cardinality require separate design |
| Index | Accelerate a declared access pattern without changing truth | Required ordered form; specialized forms measured later | Write amplification, space, rebuild, and stale derived state |
| Query and view | Select, join/traverse, project, order, and reuse derivations | Required bounded typed query; views staged | Planner behavior and result resources need visibility |
| Transaction and snapshot | Make multi-entity observations and changes coherent | Required | Isolation, conflicts, and uncertain completion need exact semantics |

## Terminology mapping

| Product concept | Current Windvale representation | Standing |
| --- | --- | --- |
| Database table | Collection descriptor plus primary schema and records | Composition exists; one atomic table contract does not |
| Table identity | Nonzero `u64` collection identity | Implemented portable format |
| Table name | Exact UTF-8 collection descriptor name | Persisted, but not unique or indexed by name |
| Column | Ordered `WVSC 1` field descriptor | Implemented with no stable field identity |
| Schema version | Nonzero schema identity stored in `WVSC 1` and each `WVRD 1` record | Multiple identities are representable; migration is not |
| Row | `WVTR 1` values inside a `WVRD 1` logical record | Implemented portable format |
| Primary key | Opaque record-identity bytes supplied outside the typed row | Efficient storage identity exists; typed primary-key semantics do not |
| Secondary index | `WVSI 1` definition and `WVIX 1` entries in the shared durable tree | Portable definition and key construction implemented |
| Index catalog | `WVIB 1` bundle for one collection/schema pair | Portable bundle implemented; durable discovery is not |
| Unique constraint | `WVUC 1` owner checks plus unique index mutations | Planned portably; hosted execution is not implemented |
| Query plan | `WVQI 1` typed predicates, projection, order, and limit | Validation exists; planning and execution do not |
| Reference constraint | No current cross-collection integrity contract | Missing |
| First-class relationship | No current typed relationship record or adjacency contract | Missing |

## What Windvale has today

### Table and row foundation

The implemented candidate contracts provide:

- one nonzero `u64` collection identity, an exact UTF-8 name of 1 through 256
  bytes, and one nonzero primary schema identity;
- one through 64 schema fields in declaration order;
- Boolean, I64, U64, UTF-8 text, and bytes field kinds;
- exact nullability and text/bytes maximum-length checks;
- exact typed rows of at most 61,408 bytes;
- an opaque record identity of 1 through 4,064 bytes;
- a row envelope that retains the schema identity used to encode it;
- deterministic, capability-free validation and encoding; and
- atomic publication of a bounded canonical mutation set through the
  persistent single-writer transaction path.

The current shape is strictly typed by default, does not inherit host locale or
Unicode normalization, rejects malformed rows before mutation planning, and can
retain old schema identities in stored row envelopes.

It does not yet provide:

- atomic create-if-absent for a table identity and name;
- a durable unique name-to-table lookup;
- an explicit typed primary key;
- stable field identities independent from ordinal position and spelling;
- literal defaults, generated fields, or field expressions;
- schema compatibility rules or row projection between schema versions;
- primary-key, check, or foreign-key constraint contracts;
- table rename, drop, truncate, or migration lifecycle;
- a public table DDL contract; or
- a server operation that composes the complete catalog, schema, row, index,
  and authority boundary.

### Ordered index foundation

The implemented index candidates provide:

- zero indexes by absence of a bundle, or one through eight definitions in one
  admitted `WVIB 1` bundle;
- one through eight distinct fields in one index;
- Boolean, I64, U64, and UTF-8 text index components;
- ascending or descending order per component;
- explicit `Exclude`, `First`, or `Last` null placement;
- unique and non-unique entry shapes;
- stable record-identity suffixes for non-unique entries;
- owner values and read-before-write checks for unique entries;
- deterministic old-entry deletion, new-entry insertion, and primary-row put
  planning as one sorted `WVTM 1` transaction; and
- a worst-case eight-index upsert of 17 mutations, within the current
  32-mutation transaction ceiling.

Every current secondary index is an ordered tree index. Definitions and index
entries share the database's durable B+tree with catalog, schema, and primary
record entries. This permits one atomic publication without adding a second
storage engine or separate index files.

The missing executable boundary is substantial:

- the hosted writer does not discover the persisted index bundle;
- the hosted writer does not execute `WVUC 1` unique-owner checks;
- indexed delete composition is absent;
- index-backed exact and range query execution is absent;
- no planner selects an index or proves that it satisfies requested ordering;
- no index create, build, validate, rebuild, or drop lifecycle exists;
- no statistics or `EXPLAIN` surface exists; and
- no reclamation policy bounds obsolete primary or index pages.

Unique-null behavior is explicit but differs from common SQL defaults.
`Exclude` makes rows with a null component absent from the index and therefore
outside its uniqueness rule. `First` or `Last` encodes null as a real component,
so equal null-bearing unique keys conflict. PostgreSQL and SQLite normally
treat null values as distinct in a unique index, while PostgreSQL can request
`NULLS NOT DISTINCT`. Windvale should preserve an explicit policy rather than
silently selecting a host convention.

## Representative systems and lessons

This comparison asks what each system makes easy and what that choice costs. It
does not ask WVDB to reproduce their syntax or feature lists.

| System | Characteristic design | Strength exposed by that design | Tradeoff or lesson for WVDB |
| --- | --- | --- | --- |
| PostgreSQL | Rich relational server with extensible types, constraints, MVCC, and several index methods | Broad transactional table semantics and specialized access paths | Extensibility, concurrency, maintenance, and planner breadth form a large coupled system; adopt requirements individually |
| SQLite | Embedded library with a compact file model, B-tree tables/indexes, optional strict tables, and serialized writes | Small deployment and a remarkably complete table/query surface | Dynamic typing defaults and file/journal behavior are product choices; small packaging does not eliminate concurrency or schema tradeoffs |
| MySQL/InnoDB | Clustered primary index; secondary entries carry the primary-key value | Primary-key lookup locality and a clear illustration of physical organization | Wide primary keys enlarge every secondary index; logical identity and physical clustering should be separate decisions |
| SQL Server | Heaps, clustered and nonclustered row indexes, filtered/covering forms, and columnstore indexes | One database can select different layouts for transactional and analytical access | More physical choices increase tuning, metadata, maintenance, and planner obligations |
| MongoDB | Nested documents with embedding or references plus multikey, wildcard, partial, text, and other indexes | Natural ownership of aggregates and query-shaped nested data | Embedding duplicates shared data; references require application or query coordination; document boundaries must be explicit |
| Neo4j | Typed property relationships are stored and queried as first-class graph elements | Direct adjacency traversal and relationship properties fit connected knowledge | Graph identity, direction, cardinality, constraints, and traversal need real semantics, not a table alias |
| DuckDB | Embedded analytical engine with column-oriented execution, automatic zonemaps, and ART indexes for selective queries | Fast scans and aggregates without treating every access path as a row B-tree | Transactional point updates and analytical scans favor different organizations; derived projections may be preferable to one universal layout |

The strongest shared lesson is that the logical contract and physical access
path should be related but not fused. A primary key establishes identity; it
does not automatically have to select heap, clustered-tree, rowid, or another
physical layout. Likewise, a relationship contract should not dictate whether
the first implementation uses an index, adjacency records, or both.

## Relationships: references and first-class connections

A table foreign key answers an integrity question: may this source key refer to
that target key, and what happens when either changes? It is valuable because
the database—not every application—can prevent dangling references. It also
adds cross-entity reads, lock/conflict choices, cycle handling, validation of
existing data, and deletion/update consequences.

A first-class relationship answers a wider modeling question: is the connection
itself durable data with a type, direction, identity, properties, cardinality,
and queryable lifecycle? This fits graphs and knowledge systems better than a
hidden join convention, but it requires adjacency access, traversal limits,
duplicate-edge rules, and relationship-specific constraints.

WVDB should specify both concepts without claiming they are identical:

- a **reference constraint** belongs to a field or key and validates target
  existence at a stated transaction time;
- a **relationship record** is a typed entity connecting stable source and
  target identities and may carry its own properties; and
- both may reuse identity lookup, uniqueness, transaction, and index machinery
  while preserving distinct public behavior.

For the first table profile, primary keys and unique constraints should precede
foreign keys. The first foreign-key contract should use immediate validation
and reject deletion of a referenced target. Cascades, deferred checks, and
cross-database references should wait until their transaction and recovery
costs are specified. This is a design candidate, not yet a normative choice.

## Index patterns and the WVDB starting point

| Index question | Established patterns | WVDB candidate |
| --- | --- | --- |
| Ordered lookup | B-tree-family indexes are common across relational and document systems | Complete the existing ordered tree path first |
| Primary organization | Heap plus index, rowid tree, clustered primary index, and separate document identity all exist | Define logical primary identity independently; qualify one physical organization per profile |
| Compound keys | Multiple ordered components are widely supported | Retain the bounded one-through-eight candidate, then validate with WVDB workloads |
| Uniqueness and null | Systems differ on whether nulls conflict and whether policy is selectable | Keep the policy explicit; never inherit a host default |
| Partial/expression indexes | PostgreSQL, SQLite, SQL Server, and MongoDB expose predicate or expression forms with different restrictions | Wait for one bounded, immutable expression and predicate contract |
| Covering data | Included or appended values can avoid base-record reads | Add only when measured primary lookup cost justifies duplicated data |
| Nested/multivalue paths | Document systems index paths, arrays, or broad wildcard fields | Reserve a document-path index contract; do not overload table-field indexes |
| Graph adjacency | Graph systems optimize traversal from node to relationship to node | Treat adjacency as a graph access path with bounded traversal, not an ordered-index spelling trick |
| Analytical skipping | Columnar systems use segment statistics or zonemaps to avoid scans | Permit derived analytical projections and automatic metadata indexes |
| Search and similarity | Full-text, spatial, and vector indexes have scoring and freshness semantics | Specify each as a distinct derived index family after a real workload exists |
| Lifecycle | Mature systems expose build, validation, rebuild, statistics, and plan inspection | Require states, progress, cancellation, integrity checks, and deterministic rebuild before broadening methods |

## Current WVDB strengths and risks

Current advantages are exact cross-host byte semantics, strict typing, explicit
resource bounds, deterministic ordered keys, atomic copy-on-write publication,
and separation between pure planning and rights-limited I/O.

Current risks are equally important: formats exist without a complete product
surface; field ordinals make evolution costly; logical structures share one
tree and append-only history; reclamation and locality are unqualified; and no
mature planner, statistics, relationship engine, index lifecycle, or operational
tooling exists. Those gaps are reasons to specify the vertical behavior before
calling the engine WVDB 1.0.

## What to build before a useful server

### 1. Accept one table profile

Define a table as one catalog-owned state transition rather than a convention
assembled by clients. Creation must atomically establish one stable nonzero
identity, one unique name in an explicit database namespace, one initial
immutable schema, one primary-key definition, and zero or more secondary-index
definitions.

Create must be conditional on identity and name absence. A retry after an
uncertain mutation must use the future durable mutation-identity contract, not
an unchecked upsert.

### 2. Change schema identity before expanding indexes

`WVSC 1` fields have names and ordinals but no stable identities. An index binds
to one schema identity and stores field ordinals. That works for immutable
fixtures but makes rename, reordering, projection, and selective rebuild hard.

Before table DDL or more index features, decide whether a successor schema
format adds a nonzero stable field identity. The recommended model is:

- stable field identity owns durable meaning;
- field name is unique presentation and query spelling within a schema;
- ordinal remains the compact row encoding position for that schema version;
- index definitions name stable field identities and cache validated ordinals
  only inside a schema-bound admitted plan; and
- each schema version is immutable once published.

The first compatible schema transitions should be narrow: add a nullable field,
add a field with a deterministic literal default, rename a field while retaining
its identity, or increase a text/bytes maximum without changing kind. Kind
changes, primary-key changes, removal, required-without-default additions, and
collation changes should use an explicit copy migration.

### 3. Implement the accepted explicit primary identity

Decision 0791 requires every entity set to declare one explicit primary
identity. The table profile presents that identity as its primary key. It
contains one or more non-null fields of admitted key types; the exact component
and encoded-size maxima remain specification questions rather than inheriting
the current eight-component index bound.

Primary-key fields are immutable. A key change is a delete plus insert in one
explicit transaction and does not silently retarget relationships. Tables do
not expose a second hidden row identifier. Physical addresses remain internal,
and the current opaque record-identity bytes may become the canonical primary
key encoding only if the format specification accepts that mapping.

Applications may supply the primary value or request a WVDB-generated value.
The generated kind, allocation, collision, import, rollback, and uncertain-retry
contracts must be accepted before that path is implemented.

### 4. Complete end-to-end index correctness

The next index implementation should:

1. read and admit the table's exact schema and persisted index bundle at one
   committed generation;
2. read the existing primary row when required;
3. derive old and new index entries;
4. execute every unique-owner check against that same committed snapshot;
5. compose primary and secondary puts/deletes into one canonical transaction;
6. publish once through the persistent writer; and
7. require reopen after commit or uncertainty without blind replay.

Delete must remove every old index entry and the primary row atomically. No
server or SQL adapter may bypass this coordinator with an ordinary tree put.

### 5. Add the smallest index planner and executor

The first deterministic planner may choose exact primary-key lookup, exact
unique-index lookup, left-prefix equality plus one range, an ordered index scan,
or a bounded primary-table scan. Every plan retains table, schema, index-bundle,
and committed-generation identity.

Execution returns at most the existing 500 rows and reports rows visited, index
entries visited, primary lookups, bytes read, and whether an explicit bounded
sort was required. A later cost model must come from measured WVDB workloads,
not copied constants from another engine.

### 6. Add an index lifecycle

The first create/rebuild path may use explicit maintenance mode: reject new
writes, pin one committed snapshot, build and validate the index with bounded
progress, then publish its `Ready` catalog state once. Interruption leaves the
old ready index set authoritative and makes incomplete work reclaimable.

The lifecycle needs explicit `Building`, `Ready`, `Failed`, and `Dropping`
states, progress, cancellation before publication, integrity checking, and a
deterministic rebuild. Online construction can follow only after the writer
queue, snapshot pins, change capture, and reclamation policies exist.

## What can wait

The first useful table/index profile does not require:

- another database's SQL, wire, file, API, or extension compatibility;
- expression indexes or arbitrary partial-index predicates;
- covering payload columns before primary-lookup measurements justify them;
- hash, full-text, JSON-path, geospatial, vector, or learned index methods;
- locale-dependent collations or user-defined operator classes;
- cascades, deferrable constraints, triggers, or general check expressions
  beyond the accepted immediate-reference and `RESTRICT` profile;
- stored or virtual generated fields;
- partitioning, inheritance, temporary tables, materialized views, or table
  spaces;
- online index creation; or
- unrestricted `ALTER TABLE` and destructive in-place conversion.

These are deferrals, not permanent exclusions. A WVDB workload, knowledge-system
requirement, conformance obligation, or measured performance limit may promote
a feature through a later decision and specification.

## Suggested implementation order

1. Apply the accepted shared entity, entity-set, relationship, and profile
   boundaries from Decisions 0792 through 0795.
2. Apply Decision 0791 and specify primary-key limits, generated identity,
   stable field identities, schema compatibility classes, and explicit null
   uniqueness.
3. Implement atomic table catalog creation and exact name lookup.
4. Integrate persisted bundle discovery, unique checks, indexed upsert, and
   indexed delete into the hosted persistent writer.
5. Implement primary, unique, prefix/range, ordered, and bounded fallback scan
   plans over `WVQI 1`.
6. Add maintenance-mode index build, validation, rebuild, and drop with progress
   and interruption evidence.
7. Measure representative WVDB schemas and workloads before changing index,
   mutation, row,
   page, cache, or result limits.
8. Add wider constraints, types, methods, and online maintenance only from an
   accepted requirement or measured performance pressure.

Reclamation and snapshot ownership remain parallel prerequisites. Table and
index writes cannot become production-safe while every changed entry grows the
file forever or while a future recycler cannot prove that no reader retains an
older page generation.

## Accepted direction and details still required

Decisions 0791 through 0798 accept the upper-layer recommendations and the
initial type, size, document, graph, and backup boundaries. The
[upper-layer decision register](WVDB-1.0-Upper-Layer-Decision-Register.md)
retains the considered alternatives and tradeoffs.

Normative specifications must still encode the accepted types and limits and
settle serialized forms, generated-identity allocation, schema projections,
relationship traversal operations and work bounds, query operations,
overflow-value storage, backup manifests and restore transitions, and
conformance evidence. An accepted direction is not an implemented feature.

## External references

The comparison was reviewed against these official sources on 2026-08-20:

- [PostgreSQL 18 `CREATE TABLE`](https://www.postgresql.org/docs/current/sql-createtable.html)
- [PostgreSQL constraints](https://www.postgresql.org/docs/current/ddl-constraints.html)
- [PostgreSQL modifying tables](https://www.postgresql.org/docs/current/ddl-alter.html)
- [PostgreSQL indexes](https://www.postgresql.org/docs/current/indexes.html)
- [PostgreSQL `CREATE INDEX`](https://www.postgresql.org/docs/current/sql-createindex.html)
- [SQLite `CREATE TABLE`](https://www.sqlite.org/lang_createtable.html)
- [SQLite strict tables](https://www.sqlite.org/stricttables.html)
- [SQLite `WITHOUT ROWID`](https://www.sqlite.org/withoutrowid.html)
- [SQLite `ALTER TABLE`](https://www.sqlite.org/lang_altertable.html)
- [SQLite `CREATE INDEX`](https://www.sqlite.org/lang_createindex.html)
- [SQLite partial indexes](https://www.sqlite.org/partialindex.html)
- [SQLite indexes on expressions](https://www.sqlite.org/expridx.html)
- [SQLite query planner](https://www.sqlite.org/queryplanner.html)
- [SQLite isolation](https://www.sqlite.org/isolation.html)
- [SQLite foreign keys](https://www.sqlite.org/foreignkeys.html)
- [MySQL 8.4 InnoDB clustered and secondary indexes](https://dev.mysql.com/doc/refman/8.4/en/innodb-index-types.html)
- [MySQL 8.4 `CREATE INDEX`](https://dev.mysql.com/doc/refman/8.4/en/create-index.html)
- [MySQL 8.4 generated columns](https://dev.mysql.com/doc/refman/8.4/en/create-table-generated-columns.html)
- [SQL Server indexes](https://learn.microsoft.com/en-us/sql/relational-databases/indexes/indexes?view=sql-server-ver17)
- [SQL Server clustered and nonclustered indexes](https://learn.microsoft.com/en-us/sql/relational-databases/indexes/clustered-and-nonclustered-indexes-described?view=sql-server-ver17)
- [MongoDB index types](https://www.mongodb.com/docs/manual/core/indexes/index-types/)
- [MongoDB partial indexes](https://www.mongodb.com/docs/manual/core/index-partial/)
- [MongoDB relationship modeling](https://www.mongodb.com/docs/manual/applications/data-models-relationships/)
- [Neo4j graph database concepts](https://neo4j.com/docs/getting-started/graph-database/)
- [Neo4j indexes](https://neo4j.com/docs/cypher-manual/current/indexes/)
- [Neo4j constraints](https://neo4j.com/docs/cypher-manual/current/schema/constraints/)
- [DuckDB indexes](https://duckdb.org/docs/current/sql/indexes)
- [DuckDB indexing performance](https://duckdb.org/docs/current/guides/performance/indexing)
