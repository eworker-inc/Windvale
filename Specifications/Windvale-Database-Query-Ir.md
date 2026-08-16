# Windvale database typed query IR

## Status and scope

`WVQI 1` is the first portable, capability-free typed query representation for
Windvale Database. Direct JSON query bodies and the parameterized SQL front end
must lower into this one form. The IR validates structure and schema binding; it
does not read rows, choose an index, execute a scan, authorize a collection, or
define a network request body.

The first form is deliberately a bounded single-collection query with a flat
conjunction of predicates. It is useful for exact and ordered-index work without
committing Windvale to EWDB's larger expression and graph-query surface.

## Binary format

All integers are unsigned little-endian unless a typed parameter says otherwise.
The 72-byte header is:

| Offset | Bytes | Field |
| ---: | ---: | --- |
| 0 | 4 | Magic `WVQI` |
| 4 | 4 | Version, exactly 1 |
| 8 | 4 | Header bytes, exactly 72 |
| 12 | 4 | Flags, exactly zero |
| 16 | 8 | Nonzero collection identity |
| 24 | 8 | Nonzero schema identity |
| 32 | 4 | Projection count |
| 36 | 4 | Predicate count |
| 40 | 4 | Order count |
| 44 | 4 | Parameter count |
| 48 | 4 | Result row limit |
| 52 | 4 | Projection-section bytes |
| 56 | 4 | Predicate-section bytes |
| 60 | 4 | Order-section bytes |
| 64 | 4 | Parameter-section bytes |
| 68 | 4 | Reserved, exactly zero |

The variable sections occur in that exact order with no padding or trailing
bytes. A projection is one 4-byte zero-based schema field index. Zero
projections means all fields; otherwise every projected field is unique.

A predicate is 16 bytes: field index, operator, parameter index, and a zero
reserved word. Operators are equal, not equal, less, less or equal, greater,
greater or equal, is null, and is not null. All predicates are joined by `AND`.
Null operators use parameter index `0xffffffff`; other operators require one
valid parameter index.

An order item is 8 bytes: field index and ascending-or-descending direction.
Each ordered field is unique. Boolean and byte-array fields have no first-version
ordering; I64, U64, and UTF-8 text use their future executor's exact typed order.

A parameter is kind, null flag, byte length, zero reserved word, and data using
the same Boolean, I64, U64, UTF-8 text, and bytes representation as a typed row
value. Query parameters are independently framed so the query IR remains a
small semantic boundary rather than embedding a complete row.

## Limits and validation

| Resource | Maximum |
| --- | ---: |
| Complete query | 61,408 bytes |
| Projections | 64 |
| Predicates | 32 |
| Order fields | 8 |
| Parameters | 32 |
| Result rows | 500 |

The row limit is required and nonzero. Admission decodes the referenced
`WVSC 1` schema and requires exact collection and schema identities. Every field
index must exist. Projection and order duplicates reject. Every parameter must
be referenced by at least one predicate; reuse is allowed. Parameter kind must
equal the field kind. Ordinary comparisons reject null parameters, requiring
explicit `IS NULL` or `IS NOT NULL`. Ordered comparisons and order clauses reject
Boolean and byte-array fields.

Checked section arithmetic occurs before slicing. Malformed parameter framing,
invalid UTF-8 text, invalid Boolean bytes, invalid numeric lengths, unknown
operators or directions, nonzero reserved words, unused parameters, and schema
mismatches reject before execution. Equal inputs and schemas produce equal
admitted bytes on every host.

## Exclusions

`WVQI 1` does not yet define `OR`, `NOT`, joins, aggregates, expressions,
functions, grouping, offsets, text search, JSON paths, collation, null ordering,
cursor serialization, mutations, plan selection, or execution. SQL syntax may
expose only the subset it can lower exactly. New syntax must not bypass or
silently widen this IR.
