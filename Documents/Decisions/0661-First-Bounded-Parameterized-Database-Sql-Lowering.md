# Decision 0661: First bounded parameterized database SQL lowering

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Windvale Database proposal](../Project/Windvale-Database-Proposal.md)
- Defines: [parameterized database SQL subset](../../Specifications/Windvale-Database-Sql.md)
- Lowers to: [typed database query IR](../../Specifications/Windvale-Database-Query-Ir.md)
- Informed by: EWDB's parameterized query boundary and bounded normalization

## Context

Windvale Database had a schema-bound typed query IR but no text query front
end. Importing a mature SQL parser or allowing host PostgreSQL/SQLite behavior
to define semantics would create a second, effectively unbounded query model.
The first SQL step instead needs to prove that familiar text can reach the
existing typed boundary without weakening its limits, types, or deterministic
bytes.

## Decision

- Accept one UTF-8 `SELECT` statement of at most 16,384 bytes over one exactly
  bound collection and `WVSC 1` schema.
- Admit `*` or up to 64 named projections, up to 32 `AND` predicates, up to 8
  order fields, and a mandatory 1-through-500 row limit.
- Admit the seven ordinary comparison spellings plus `IS NULL` and
  `IS NOT NULL`; require ordinary values to be `$1` through `$32` parameters.
- Use case-insensitive ASCII keywords and exact case-sensitive bounded ASCII
  collection and field names.
- Lower every admitted statement into `WVQI 1` and retain its typed rejection
  when parameter, null, order, duplicate-field, schema, or framing checks fail.
- Exclude literals, comments, multiple statements, quoted identifiers,
  mutations, joins, `OR`, expressions, aggregates, and PostgreSQL/SQLite
  compatibility from the first grammar.
- Give SQL, query IR, and shared schema edits focused one-, two-, and three-case
  development targets so verification cost grows only with affected behavior.

## Evidence

The focused fixture covers mixed keyword case and whitespace, exact
projection/predicate/order lowering, all supported operators, parameter reuse,
`SELECT *`, and the 500-row ceiling. Semantically equivalent `ASC` omission and
`!=`/`<>` spellings produce byte-identical query IR. It rejects empty,
oversized, malformed UTF-8, truncated, unknown-name, unsupported grammar,
literal, parameter-number, limit, trailing-statement, duplicate-field,
kind-mismatch, null-comparison, unsupported-ordering, unused-parameter, and
all count-bound cases.

Two independent builds produced identical 89,580-byte WVB modules with SHA-256
`b1672dde914564622314b2363367b636cc124cc27f2ac4c0b177b275c8e7eba8`.
Independent lowering produced identical 1,030,586-byte WVO objects with
SHA-256
`8c538f55b9bcabccc2a22fbf08532e327000333f435b79f627285a2407c5fcd1`.
The 1,045,504-byte Windows hosted application returned zero for the complete
fixture.

A ten-run local whole-process sample had 34.567 ms median time and 35.703 ms
mean time. A separate sampled run set observed at most 3,440,640 bytes of
client working set. Replacing a deliberately oversized-input fixture's linear
byte-by-byte construction with bounded doubling reduced its earlier 62.171 ms
median to the reported 34.567 ms. These are parser/test-harness measurements,
not database query throughput.

## Consequences

Windvale now has one familiar read-query syntax without creating a second
semantic engine. The next query work can implement bounded execution and index
selection against `WVQI 1`; JSON query bodies can lower to the same form.

PostgreSQL and SQLite remain comparison peers, not compatibility definitions.
A useful performance comparison waits for a persistent Windvale server and
batch transactions so process startup does not dominate database work.

## Reconsideration triggers

Revisit the grammar when an E-Worker migration workload proves a bounded need
for another construct, when query execution needs a cursor form, or when a
measured parser hot path justifies a different scanning representation without
changing accepted SQL meaning.
