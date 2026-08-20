# Decision 0797: Keep documents in WVDB and support basic typed graphs

- Date: 2026-08-20
- Status: Accepted direction; the document profile remains optional for the
  initial 1.0 conformance claim
- Builds on: [Decision 0795](0795-Accept-WVDB-1.0-Profiles-And-Storage-Organization.md)
- Review: [documents and graphs](../Project/WVDB-1.0-Types-Sizes-Documents-Graphs-And-Backup.md)

## Context

Documents need identity, nested-value semantics, transactions, constraints,
indexes, authority, backup, and recovery. Making them a separate service would
duplicate these mechanisms and make atomic operations with tables and
relationships difficult.

Large opaque files have different needs: streaming, range reads, content
identity, retention, and potentially independent placement. They should not
force ordinary WVDB rows and documents to become unbounded blob containers.

Decision 0795 already requires a basic typed relationship profile. The graph
boundary needs to state what that support means without implying a complete
graph-analytics product.

## Decision

The document profile belongs inside WVDB and shares its database identity,
catalog, primary identities, transactions, authority, backup, recovery, and
service boundary. It is not a separate document microservice.

A document profile defines canonical nested values, document types or admitted
open shapes, depth/node/size limits, path mutation, path queries, path indexes,
constraints, and schema evolution. JSON may be an input/output language but
does not define durable document semantics.

The document profile remains optional for the initial WVDB 1.0 conformance
claim. Its extension boundary is specified during 1.0 design and implemented
after strict tables and typed relationships unless a named first product
promotes it through a later decision.

Large media, archives, model files, and opaque objects use a separate
rights-limited streaming object-storage capability. WVDB stores their stable
identity, content hash, size, media type, ownership, authorization metadata,
and lifecycle state. Cross-service mutations use explicit idempotent workflows;
WVDB does not report an external object mutation as transactionally complete
merely because its metadata committed.

WVDB 1.0 supports a basic typed graph:

- entities act as nodes without becoming duplicate graph copies;
- each relationship has primary identity, type, source, target, direction, and
  optional admitted properties;
- relationship types may constrain endpoint types, uniqueness, and cardinality;
- source and target adjacency indexes update synchronously; and
- queries admit exact relationship lookup, incoming/outgoing adjacency, type
  filters, and traversal with explicit depth, result, work, memory, deadline,
  and cancellation bounds.

The required profile does not include unrestricted recursion, arbitrary graph
pattern syntax, shortest-path optimization, centrality/community algorithms,
distributed graph partitioning, or graph-scale analytical execution.

## Consequences

- Tables, documents, and relationships can participate in one transaction and
  backup domain when the document profile is present.
- The initial 1.0 implementation remains bounded to core, tables, and basic
  typed relationships.
- Knowledge, dependency, lineage, permission, workflow, and moderate connected
  applications have a direct graph model.
- Media storage and database metadata remain separately authorized and require
  explicit lifecycle coordination.

## Reconsideration triggers

Revisit the optional document timing when a named initial product requires
nested documents, or revisit the graph boundary when a measured workload needs
a specialized authoritative graph store or bounded algorithm profile.
