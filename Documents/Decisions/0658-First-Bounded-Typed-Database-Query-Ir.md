# Decision 0658: First bounded typed database query IR

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Windvale Database proposal](../Project/Windvale-Database-Proposal.md)
- Defines: [typed database query IR](../../Specifications/Windvale-Database-Query-Ir.md)
- Builds on: [typed rows and schemas](../../Specifications/Windvale-Database-Typed-Rows-And-Schemas.md)
- Informed by: EWDB bounded projections, typed index bounds, paging limits, and query-normalization limits

## Context

Windvale needs SQL and JSON without creating two query engines. Both surfaces
need one typed, bounded, schema-bound representation before execution or index
selection. EWDB demonstrates the value of explicit projection, predicate,
parameter, order, and row limits, but its mature expression, text, graph, and
membership surface is too large for Windvale's first native database path.

## Decision

- Add one deterministic `WVQI 1` binary IR shared by future JSON and SQL
  lowering.
- Bind each query to one nonzero collection and one exact `WVSC 1` schema.
- Support zero-or-more unique projections, up to 32 flat `AND` predicates, up
  to 8 unique order fields, up to 32 typed parameters, and a required 1-through-
  500 row limit.
- Support equal, not equal, four ordered comparisons, is null, and is not null.
- Require every parameter to be typed, non-null for ordinary comparisons, and
  referenced at least once; permit intentional reuse.
- Reject Boolean and bytes ordering until their semantics are separately
  specified.
- Validate all sizes, reserved fields, indices, kinds, parameter framing, and
  schema identities before any future executor sees the query.
- Keep execution, plans, indexes, cursors, JSON bodies, and SQL grammar separate.

## Evidence

The focused native fixture covers deterministic encoding and decoding, exact
header and section sizes, mixed typed parameters, reuse-independent parameter
framing, projection and order sections, three predicate forms, bounded count
headers, and a 500-row ceiling. It rejects malformed headers and
section lengths, excessive counts, zero limits, invalid fields, duplicate
projections and orders, kind mismatches, null comparisons, unused parameters,
and Boolean ordering.

Two independent builds produced identical 62,193-byte WVB modules with SHA-256
`31a98cab4ce1d45e8183644a9d0dd8bd160060b8da532de71ccacdfc53bc1fc6`.
Independent lowering produced identical 805,745-byte WVO objects with SHA-256
`a70adb67db310fcd070249cab819cb4e7bb212d8ff98b898e498fcabd29f4fd6`.
The 823,296-byte Windows hosted application returned zero for the complete
fixture.

A ten-run local whole-process sample had 31.498 ms median time, 39.283 ms mean
time, and 6,242,304 bytes maximum observed client working set. This is
test-harness evidence, not query execution throughput.

## Consequences

The next SQL and JSON milestones have a single target and cannot invent
different null, type, ordering, or limit behavior. A future executor can plan
against stable field indices and typed parameters without parsing text or JSON.

The flat conjunction is intentional. `OR`, joins, aggregates, expressions, and
text/JSON operators require measured use cases and explicit bounded contracts
rather than an open-ended expression tree.

## Reconsideration triggers

Revisit the first IR when SQL lowering demonstrates a necessary expression that
cannot be represented exactly, when secondary-index execution needs explicit
index or cursor evidence, or when E-Worker migration workloads demonstrate a
bounded need for a larger projection, predicate, or page limit.
