# Decision 0796: Accept WVDB 1.0 field types and size ceilings

- Date: 2026-08-20
- Status: Accepted direction; exact encodings, algorithms, and conformance
  fixtures pending
- Builds on: [Decision 0792](0792-Accept-The-WVDB-1.0-Entity-And-Table-Foundation.md)
- Review: [types and sizes](../Project/WVDB-1.0-Types-Sizes-Documents-Graphs-And-Backup.md)

## Context

WVDB needs enough exact field types for ordinary business, system, scientific,
and knowledge applications. It also needs honest, layered size promises rather
than treating a 64-bit address domain as a qualified database capacity.

The current candidate supports Boolean, I64, U64, UTF-8 text, and bytes; at
most 64 fields; approximately 60 KiB rows; 4 KiB keys; and approximately
64 KiB strict JSON values. Those limits are useful implementation evidence but
are too narrow to define the complete WVDB 1.0 product.

## Decision

The required WVDB 1.0 table field families are:

- Boolean;
- signed I8, I16, I32, and I64;
- unsigned U8, U16, U32, and U64;
- exact Decimal with a signed 128-bit coefficient, declared precision and
  scale, and maximum precision 38;
- binary floating-point F32 and F64;
- opaque Id128;
- bounded strict UTF-8 Text;
- bounded Bytes;
- Date;
- Time of day without a date or zone;
- Instant with exact UTC meaning;
- Duration; and
- typed Enum with stable member identities.

Nullability is a field constraint, not a type. A reference is a constraint over
fields whose canonical types match the target primary identity.

Every type specification must define canonical bytes, equality, total ordering
where order is admitted, conversions, overflow, arithmetic where applicable,
text formatting/parsing, size bounds, and schema evolution. Floating-point
specification includes canonical NaN and signed-zero behavior. Date and time
types do not inherit host calendar, locale, clock, or time-zone behavior.

Document/List/Map/nested Record, arbitrary-precision numeric, named-zone
date-time, full-text, geospatial, vector, and large-object types remain
separately versioned profiles or extensions.

Accept these size boundaries:

| Boundary | Accepted WVDB 1.0 direction |
| --- | ---: |
| Mathematical format arithmetic | Unsigned 64-bit; less than 16 EiB and not a support claim |
| WVDB 1.0 format ceiling | 1 PiB per database |
| First hosted server supported ceiling | 16 TiB per database |
| Default new-database quota | 1 TiB, explicitly configurable |
| One table/entity/relationship set | Up to its database or owner-selected quota |
| Encoded primary or secondary key | 4 KiB |
| Fields per table | Portable support at least 256; hard format maximum 1,024 |
| Indexes per table | Portable support at least 64; hard maximum 256 |
| Key components per index | Portable support at least 16; hard maximum 32 |
| One table row | 1 MiB canonical encoded |
| One relationship record | 1 MiB canonical encoded |
| One document | 16 MiB canonical encoded |

Database, namespace, entity-set, relationship-set, cursor, transaction, cache,
and retained-snapshot quotas remain explicit advertised values within their
normative maxima. Nothing is described as unlimited.

Rows and documents larger than one durable page require a versioned overflow
or chunk organization. Logical mutation remains atomic, while APIs and tools
stream large values and keep memory, I/O, diagnostics, and cancellation bounded.

## Consequences

- Business and accounting systems receive exact decimal and temporal values.
- Scientific and telemetry applications receive floating point but not yet a
  columnar or high-ingest execution claim.
- Current row/schema formats require successors before they can satisfy 1.0.
- A 1 PiB format ceiling leaves long-term address room without claiming that
  the first server has qualified that operating scale.
- The 16 TiB hosted ceiling requires boundary, sparse/synthetic, recovery,
  inspection, backup-stream, and resource-accounting evidence. It does not
  imply full-capacity performance without a measured workload.
- Large media remains outside rows and documents.

## Reconsideration triggers

Revisit a type or size only when conformance cannot define it portably, the
Windvale Language 1.0 implementation cannot express it safely, or measured
representative workloads show that the ceiling causes unacceptable page
fanout, write amplification, memory, backup time, or application exclusion.
