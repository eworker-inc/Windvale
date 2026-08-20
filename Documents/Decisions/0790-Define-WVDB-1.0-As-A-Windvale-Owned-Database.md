# Decision 0790: Define WVDB 1.0 as a Windvale-owned database

- Date: 2026-08-20
- Status: Accepted direction; normative specifications and implementation pending
- Supersedes in part: the external database rewrite and parity direction in
  [Decision 0595](0595-Select-Windvale-0.2.0-Connected-Services-Preview.md)
- Program: [WVDB 1.0 specification plan](../Project/WVDB-1.0-Specification-Plan.md)

## Context

Windvale now has executable database storage, tree, transaction, typed-row,
query-IR, SQL-lowering, and index-planning candidates. Earlier project framing
treated an existing external database implementation as the product baseline
and parity authority. That framing was useful while Windvale and its source
language were incomplete, but it would make the public WVDB design appear to be
a translation or compatibility project.

Windvale Language 1.0 implementation is now advancing directly in this
repository. WVDB needs a coherent long-lived semantic identity of its own. It
may also become a storage and retrieval foundation for a future Windvale
knowledge system, where table, document, graph, analytical, and semantic access
patterns should be evaluated deliberately rather than forced through one
historical product model.

Established databases remain valuable evidence. Their designs demonstrate
different answers for typed tables, documents, relationships, indexes,
transactions, query planning, storage organization, concurrency, and
operations. They do not need to become compatibility targets or normative
dependencies for WVDB to learn from them.

## Decision

Define **WVDB 1.0** as a Windvale-owned database system specified by Windvale
contracts and implemented in Windvale Language 1.0.

WVDB 1.0 has no required source, runtime, API, wire, file, SQL, migration, or
behavioral compatibility with an external database, language runtime,
application framework, or service. No external system is a parity authority
for WVDB.

The active WVDB design may compare representative established systems when the
comparison expands design knowledge. Each comparison must:

- identify the design axis and official source reviewed;
- use neutral language and state material tradeoffs;
- distinguish a system's product choice from a universal database rule;
- avoid compatibility, superiority, or exhaustive-feature claims; and
- turn lessons into explicit Windvale requirements only through a later WVDB
  decision or normative specification.

Separate user-visible logical models from physical persistence. The shared
WVDB foundation may own database identity, catalogs, typed values, transactions,
snapshots, capabilities, resource bounds, diagnostics, and versioning. A table
profile, document profile, graph/relationship profile, analytical projection,
or semantic index may then define its own exact behavior over one or more
qualified storage organizations.

This decision does not accept a universal object model or require WVDB 1.0 to
ship every profile. The first complete upper-layer specification work begins
with entities, entity sets/tables, identities, keys, relationships, constraints,
and indexes. It must preserve room for a knowledge-oriented relationship model
without weakening the exact table profile.

Keep current implementation contracts as evidence and candidate mechanisms.
They become WVDB 1.0 contracts only when reconciled with the new specification
set. Do not retain a candidate byte format merely to avoid changing early
fixtures.

Historical decisions and proposals remain provenance records. This decision
supersedes their external rewrite and parity requirements but does not rewrite
their original context. It does not otherwise cancel the connected-services
milestone or select a WVDB release date.

## Consequences

- Active WVDB documents no longer present an external database as the product
  source or completion definition.
- Windvale Language 1.0 is the implementation language and semantic host for
  the database above explicit platform capability boundaries.
- PostgreSQL, SQLite, MySQL/InnoDB, SQL Server, MongoDB, Neo4j, DuckDB, and
  later systems may appear as representative comparisons, never dependencies.
- The current ordered copy-on-write tree remains a strong storage candidate but
  does not preclude a columnar, graph-adjacency, document, search, vector, or
  other measured storage/index organization.
- Normative `Specifications/` documents are written only after the corresponding
  design questions settle; project reviews remain visibly non-normative.
- Implementation follows accepted specifications and conformance cases rather
  than driving public semantics through whatever the current fixture supports.
- Importing data from another database is an optional versioned tool contract,
  not native compatibility or an authority over WVDB design.

## Reconsideration triggers

Revisit this decision if Windvale Language 1.0 cannot express a required WVDB
contract without weakening safety or bounded execution, if multiple logical
profiles cannot share a coherent transaction and authority foundation, or if a
named product requirement justifies one explicit compatibility profile. Any
compatibility profile must remain separate from core WVDB semantics and must
name its exact source version, limits, and qualification evidence.
