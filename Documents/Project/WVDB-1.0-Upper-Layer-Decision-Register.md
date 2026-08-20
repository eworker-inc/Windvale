# WVDB 1.0 upper-layer decision register

- Date: 2026-08-20
- Status: Accepted recommendation set and retained tradeoff record; numeric and
  format details remain specification work
- Direction: [Decision 0790](../Decisions/0790-Define-WVDB-1.0-As-A-Windvale-Owned-Database.md)
- Accepted identity: [Decision 0791](../Decisions/0791-Use-One-Explicit-Primary-Identity-Per-WVDB-Entity-Set.md)
- Accepted entity/table foundation: [Decision 0792](../Decisions/0792-Accept-The-WVDB-1.0-Entity-And-Table-Foundation.md)
- Accepted relationships/integrity: [Decision 0793](../Decisions/0793-Accept-WVDB-1.0-References-And-Typed-Relationships.md)
- Accepted index/query/transactions: [Decision 0794](../Decisions/0794-Accept-WVDB-1.0-Index-Query-And-Transaction-Direction.md)
- Accepted profiles/storage: [Decision 0795](../Decisions/0795-Accept-WVDB-1.0-Profiles-And-Storage-Organization.md)
- Program: [WVDB 1.0 specification plan](WVDB-1.0-Specification-Plan.md)
- Detailed review: [entities, tables, relationships, and indexes](Windvale-Database-Tables-And-Indexes.md)

## Purpose

This register retains the alternatives, benefits, costs, and recommended
answers reviewed for WVDB 1.0. The recommendations are accepted by Decisions
0792 through 0795. They become complete normative contracts only when the
affected specifications state exact behavior, limits, and conformance cases.

The choices are ordered by dependency. Group 1 shapes entities and tables.
Group 2 shapes relationships and constraints. Group 3 shapes indexes, queries,
transactions, and expressions. Group 4 shapes profiles and lower storage.

## Accepted choice

### A1. Entity and table identity

**Accepted:** every entity set has one explicit primary identity; a table
presents it as its primary key. There is no second hidden row identity.
Application-supplied and WVDB-generated values are both permitted once their
exact contracts exist. Identity is immutable; physical addresses are internal.

The remaining generated-identity type and allocation questions appear in
Group 1 because they affect key size, indexes, imports, and network retries.

## Accepted group 1: entities and tables

### Q1. Shared logical model

| Option | Benefits | Costs |
| --- | --- | --- |
| Relational core only | Small vocabulary; tables, keys, and joins are familiar | Documents and knowledge relationships become awkward table conventions |
| Independent table, document, and graph engines | Each model can be optimized independently | Duplicates identity, transaction, authority, migration, and operational machinery |
| Shared foundation with explicit profiles | Reuses identity, values, transactions, capabilities, and diagnostics while preserving table/document/graph behavior | Requires careful boundaries so the shared layer does not become a vague lowest common denominator |

**Accepted direction:** shared foundation with explicit profiles. Accept
`database`, `namespace`, `entity type`, `entity set`, `entity`,
`relationship`, `index`, `view`, and `transaction` as logical terms.
Make the strict table profile first; do not require every profile to ship in
WVDB 1.0.

### Q2. Meaning of collection

| Option | Benefits | Costs |
| --- | --- | --- |
| Expose `collection` as the universal public object | Matches current implementation vocabulary | Blurs tables, documents, graph sets, and physical ownership |
| Keep `collection` as a provisional engine term | Preserves current work while public profiles use exact names | Requires mapping and later lower-layer review |
| Remove the term immediately | Produces clean new vocabulary | Forces format and code changes before the replacement storage contract is settled |

**Accepted direction:** keep `collection` only as a provisional internal engine
term. Do not expose it in the WVDB 1.0 public model. Reconsider or replace it
when storage organizations are specified.

### Q3. Stable field identity

| Option | Benefits | Costs |
| --- | --- | --- |
| Field name is identity | Easy to inspect and encode | Rename changes identity; spelling and normalization become durable semantics |
| Field ordinal is identity | Compact rows and fast positional access | Reorder and insertion change identity; migrations become fragile |
| Stable field identifier plus versioned name and ordinal | Rename and reordering preserve meaning; indexes can bind durably | Adds catalog bytes, validation, and mapping work |

**Accepted direction:** give every field a stable nonzero identifier. A schema
version owns the field's current unique name and compact row ordinal. Persisted
indexes and constraints bind stable field identifiers, not names or ordinals.

### Q4. Schema evolution

| Option | Benefits | Costs |
| --- | --- | --- |
| Mutate one schema in place | Simple catalog and current-shape lookup | Old rows and indexes become ambiguous; rollback and concurrent readers are hard |
| Immutable schema versions with bounded projection and copy migration | Reproducible reads; explicit compatibility; safe rollback and index binding | Requires version retention, projection rules, and migration tooling |
| No schema evolution in 1.0 | Smallest implementation | Makes a production database impractical and pushes unsafe conversion to applications |

**Accepted direction:** immutable versions. Permit a small compatible set such as
adding a nullable field, adding a deterministic literal default, renaming while
retaining field identity, and increasing a bounded text or byte limit.
Everything else uses an explicit copy migration.

### Q5. Type and value boundary

| Option | Benefits | Costs |
| --- | --- | --- |
| Reproduce a broad SQL-style type catalog immediately | Familiar and expressive | Large conversion, comparison, collation, arithmetic, encoding, and migration surface |
| Freeze the current Boolean/I64/U64/text/bytes set as all of 1.0 | Small and already implemented | Missing decimal, time, identifiers, and other common production meanings |
| Define a small exact scalar core with separately versioned type families | Portable semantics can grow deliberately; profiles need not admit every type | Requires capability/admission metadata and conversion rules |

**Accepted direction:** define a strict portable scalar core, then add only types
with exact equality, ordering, encoding, arithmetic, limits, and evolution
contracts. Decimal, time/instant, and generated identity need deliberate 1.0
decisions. Nested values belong to an explicit document or composite-value
profile rather than appearing accidentally in table fields.

### Q6. Generated primary identity

| Option | Benefits | Costs |
| --- | --- | --- |
| Monotonic `u64` | Compact, ordered, and efficient in B+trees | Database-local; predictable; coordination and import collisions need handling |
| Opaque random 128-bit value | Can be allocated without a central sequence; suitable for imports and distributed creation | Larger keys and secondary indexes; random insertion locality |
| Opaque sortable 128-bit value | Large namespace with better index locality | More complex generation; accidental time/order inference must be prevented |
| No built-in generator | Small core and fully explicit applications | Every application reinvents identity, retry, collision, and import behavior |

**Accepted direction:** specify one opaque WVDB-generated 128-bit identity with no
application-visible chronological meaning, while still permitting explicit
application keys. Compare random and sortable encodings with B+tree, import,
privacy, and uncertain-retry workloads before selecting the exact algorithm.

### Q7. Atomic table lifecycle

| Option | Benefits | Costs |
| --- | --- | --- |
| Clients compose catalog, schema, and index writes | Small server API | Partial creation and cleanup become client-visible failure states |
| Server owns atomic create, rename, truncate, and drop operations | One authority and recovery boundary; deterministic retries | More server operations and lifecycle states |
| Treat DDL as unrestricted ordinary transactions | Flexible and composable | Hard to validate global invariants and bound destructive work |

**Accepted direction:** named server-owned lifecycle operations that publish their
catalog, schema, constraint, and index state atomically. Destructive operations
must expose exact ownership, progress, cancellation, and uncertain-completion
behavior.

## Accepted group 2: relationships and constraints

### Q8. References versus first-class relationships

| Option | Benefits | Costs |
| --- | --- | --- |
| Use only table foreign keys | Familiar and small | Relationship properties, identity, direction, and graph traversal become join conventions |
| Represent every reference as a graph relationship | One relationship abstraction | Ordinary table references pay graph complexity and may lose concise field semantics |
| Keep distinct public contracts that share mechanisms | Tables retain foreign keys; graphs and knowledge use typed relationship records | Requires two surfaces and exact rules for crossing between them |

**Accepted direction:** distinct contracts with shared identity lookup,
transactions, constraints, and index machinery. A foreign key constrains a
field value; a first-class relationship is durable typed data.

### Q9. First foreign-key behavior

| Option | Benefits | Costs |
| --- | --- | --- |
| Defer foreign keys beyond 1.0 | Faster table implementation | Applications can create dangling references; integrity is not centrally owned |
| Immediate validation with `RESTRICT` on target deletion | Strong basic integrity with bounded, understandable mutation behavior | Cross-set reads and conflict rules are still required |
| Add cascades, deferred checks, and mutable referenced keys immediately | Rich relational behavior | Cycles, work bounds, ordering, rollback, and diagnostics become much larger |

**Accepted direction:** require immediate validation and `RESTRICT` initially.
References target the primary identity. Add cascades, deferred constraints, and
references to named unique keys only through later exact contracts.

### Q10. Relationship identity and duplicate edges

| Option | Benefits | Costs |
| --- | --- | --- |
| Source, type, and target uniquely identify an edge | Simple set semantics and compact adjacency | Cannot represent repeated events or parallel relationships with different properties |
| Every relationship has an independent generated identity | Supports parallel edges and relationship lifecycle | Duplicate prevention requires additional constraints |
| Relationship type declares its identity/cardinality rule | Supports set-like links and event-like edges accurately | More type metadata and validation paths |

**Accepted direction:** every relationship has explicit primary identity, while its
relationship type can additionally declare source/type/target uniqueness and
cardinality constraints. This follows the one-identity rule without forbidding
parallel relationships.

### Q11. Unique constraints containing null

| Option | Benefits | Costs |
| --- | --- | --- |
| Null values are always distinct | Familiar in many SQL systems; permits many unknown values | Cannot express one shared unknown value without another constraint |
| Null values always conflict | Strong uniqueness including missing values | Often surprises applications and prevents multiple incomplete rows |
| Constraint selects `Exclude`, `Distinct`, or `Not_distinct` | Exact intent and supports sparse indexes | More surface and conformance cases |

**Accepted direction:** require an explicit policy in the durable constraint:
`Exclude`, `Distinct`, or `Not_distinct`. A convenience language may have a
documented default, but serialized WVDB metadata must never depend on an
unstated default.

## Accepted group 3: indexes, queries, transactions, and expressions

### Q12. Index limits and capabilities

| Option | Benefits | Costs |
| --- | --- | --- |
| Fix the current eight indexes and eight fields permanently | Simple bounds and current transaction planning fits | Likely too restrictive for some production schemas |
| Allow unbounded indexes and key width | Maximum apparent flexibility | Unbounded memory, mutation, diagnostics, and recovery are unacceptable |
| Database advertises exact limits within WVDB 1.0 maxima | Bounded execution with room for profiles and future implementations | Applications must admit requirements against database capabilities |

**Accepted direction:** use advertised immutable database limits bounded by WVDB
1.0 maxima. Keep the current eight/eight values as implementation-candidate
limits, not public 1.0 limits, until representative workloads measure key size,
write amplification, and transaction cost.

### Q13. Index build lifecycle

| Option | Benefits | Costs |
| --- | --- | --- |
| Maintenance-mode build only | Small, deterministic, and easiest to recover | Blocks writes and may be unacceptable for large live databases |
| Online build only | Best availability | Requires snapshot pins, change capture, catch-up, cancellation, and more failure states immediately |
| Offline build required; online build optional capability | Gives every implementation one correct path and allows stronger servers | Applications must inspect capability and plan downtime when absent |

**Accepted direction:** require bounded maintenance-mode build, validate, rebuild,
and drop in the base profile. Define online build later as an optional explicit
capability with the same final index semantics.

### Q14. Canonical query interface

| Option | Benefits | Costs |
| --- | --- | --- |
| SQL text defines WVDB semantics | Familiar ecosystem and concise ad hoc use | Grammar quirks and compatibility expectations can become the semantic authority |
| Typed structured query only | Exact validation and natural Windvale integration | Less convenient for humans and existing tools |
| Typed query model is canonical; SQL is an optional lowering surface | One exact semantic model with both programmatic and human access | Requires mapping diagnostics and rejection of SQL outside the typed model |

**Accepted direction:** make the bounded typed query model canonical. Provide WVDB
SQL as a separately versioned language that lowers into that model; it is not a
compatibility promise.

### Q15. Transaction and isolation profile

| Option | Benefits | Costs |
| --- | --- | --- |
| Autocommit single-record operations only | Smallest service and recovery surface | Cannot preserve multi-entity or relationship invariants |
| Immutable snapshot readers with bounded transactions through one serialized writer | Deterministic conflicts and publication; matches current copy-on-write evidence | Write throughput is serialized and long readers require snapshot/reclamation bounds |
| Multi-writer serializable execution in 1.0 | Greater write concurrency and strong isolation | Requires substantially more locking or validation, deadlock/conflict policy, testing, and recovery work |

**Accepted direction:** begin with immutable committed snapshots and bounded
multi-record transactions through one serialized writer queue. Transactions
provide read-your-writes and publish atomically; readers keep their admitted
snapshot. Specify queue, snapshot, transaction, retry, and retained-page limits
before considering multiple concurrent writers.

### Q16. Defaults, checks, and generated expressions

| Option | Benefits | Costs |
| --- | --- | --- |
| Permit host callbacks or arbitrary functions | Highly expressive | Non-portable, non-deterministic, authority-bearing, and unsafe for recovery or replication |
| Support literals only | Very small and deterministic | Check constraints and useful generated values remain unavailable |
| Use a versioned pure bounded expression model | Supports defaults, checks, indexes, and generated fields with one semantic core | Requires an evaluator, type rules, work bounds, and evolution policy |

**Accepted direction:** accept deterministic literal defaults first, then define
one versioned, pure, capability-free, bounded expression model shared by check
constraints, expression indexes, and generated fields. No host callbacks or
ambient time, randomness, locale, network, or filesystem access.

## Accepted group 4: profile scope and lower-layer consequences

### Q17. WVDB 1.0 profile scope

| Option | Benefits | Costs |
| --- | --- | --- |
| Ship tables only | Smallest coherent database product | Knowledge relationships remain application conventions |
| Ship every planned profile | Broad vision in one release | Risks years of coupled work and shallow, unqualified features |
| Ship shared core, strict tables, and basic typed relationships | Useful transactional system with a knowledge-ready seam | More work than tables alone; documents, analytics, and semantic search remain later |

**Accepted direction:** WVDB 1.0 should include the shared core, strict table
profile, and basic typed relationship profile. Specify extension boundaries for
documents, analytical projections, full-text, geospatial, and vector search,
but do not claim those profiles until individually qualified.

### Q18. Canonical and derived storage

| Option | Benefits | Costs |
| --- | --- | --- |
| Put every logical structure in one ordered tree | One publication and recovery mechanism | Poor fit for scans, adjacency, large documents, and specialized search |
| Give every profile an independent authoritative store | Workload-specific layouts | Cross-profile transactions, backup, and consistency become difficult |
| One canonical transactional truth plus declared derived organizations | Clear durability and recovery authority; specialized access can evolve | Derived freshness, rebuild, space, and write amplification need exact contracts |

**Accepted direction:** one canonical transactional truth for each admitted entity
or relationship, plus reconstructible indexes and projections. The canonical
store need not always be the current tree, and a later specification may admit
another authoritative organization for a distinct database profile.

### Q19. Physical primary organization

| Option | Benefits | Costs |
| --- | --- | --- |
| Cluster by declared primary identity | Direct lookup and ordered locality | Wide or random identities affect layout and every rewrite |
| Heap or generated physical address plus primary index | Stable physical placement independent of key order | Adds indirection and a second physical lookup |
| Select an organization per database profile and record it in the format | Matches workload and preserves explicitness | More format variants, qualification, and tooling |

**Accepted direction:** keep logical primary identity independent from physical
organization. Qualify one simple organization first, then permit explicit
profile-scoped organizations only when measured workloads justify them.

### Q20. Derived-index consistency

| Option | Benefits | Costs |
| --- | --- | --- |
| Every index is transactionally current | Simple query truth and recovery | High write amplification; unsuitable for some analytical or semantic indexes |
| Every derived index is eventually consistent | Fast canonical writes | Reads need freshness semantics; constraints cannot rely on stale indexes |
| Classify indexes as synchronous constraint/access indexes or asynchronous projections | Preserves exact table integrity while allowing expensive derived search | Two lifecycle and query-freshness contracts |

**Accepted direction:** ordered indexes used for uniqueness, references, or normal
table access are synchronous with the canonical mutation. Analytical, full-text,
or semantic projections may be asynchronous only when their type exposes exact
freshness, checkpoint, rebuild, and query requirements.

## Accepted sequence and specification follow-up

The choices were accepted in these dependency groups:

1. Q1 through Q7: logical vocabulary, types, fields, schema evolution,
   generated identity, and table lifecycle;
2. Q8 through Q11: references, relationships, and integrity;
3. Q12 through Q16: index limits, lifecycle, query, transactions, and
   expressions; and
4. Q17 through Q20: 1.0 scope and lower storage consequences.

Decisions 0792 through 0795 record those groups. Normative specifications still
must define the exact operations, serialized forms, numeric limits, failure
behavior, and conformance evidence before implementation treats them as 1.0
contracts.
