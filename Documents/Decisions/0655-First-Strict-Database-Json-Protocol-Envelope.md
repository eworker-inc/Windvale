# Decision 0655: First strict database JSON protocol envelope

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Windvale Database proposal](../Project/Windvale-Database-Proposal.md)
- Defines: [database JSON protocol envelope](../../Specifications/Windvale-Database-Json-Protocol.md)
- Builds on: [strict database JSON value](../../Specifications/Windvale-Database-Json-Value.md)
- Informed by: EWDB request contexts, response headers, strict unmapped-member rejection, and mutation ambiguity separation

## Context

Windvale has a strict bounded JSON value but no stable way to identify a
database request, select an operation, carry a caller deadline or freshness
condition, or correlate a response. EWDB provides useful lessons: version the
wire contract independently, reject unknown members, keep request identity and
database identity explicit, report the observed commit sequence, and do not
hide mutation ambiguity behind a generic success value.

EWDB's multi-megabyte request and response ceilings and large operation family
do not fit the current Windvale record and server maturity. Copying them would
create an unmeasured concurrency and memory promise.

## Decision

- Define one 64 KiB version-1 request envelope and one response envelope.
- Require bounded request and database identifiers with ASCII-safe exact
  grammars.
- Admit only the named `ping`, `get`, `put`, `delete`, `scan`, `transaction`,
  and `query` operation families.
- Require a bounded deadline, permit one optional minimum committed sequence,
  and require object bodies.
- Require response status and observed committed sequence while leaving exact
  mutation outcome and ambiguity to strict operation bodies.
- Reject every unknown envelope member and every unsupported scalar spelling.
- Retain one owned admitted envelope and body spans rather than copying the
  body.
- Emit deterministic compact member order from bounded inputs.
- Do not claim that operation bodies, transport, authentication, authorization,
  or server execution exist yet.

## Evidence

The focused native fixture covers valid arbitrary member order, escaped known
member names, maximum `u64`, nested bodies, deterministic request and response
encoding, decode/encode round trips, an optional freshness sequence, and the
near-64 KiB envelope boundary. It rejects malformed JSON, non-object envelopes,
unknown and missing members, wrong versions, invalid request/database IDs,
unknown operations and statuses, fractional or excessive deadlines, overflowing
sequences, and non-object bodies.

Two independent builds produced identical 70,217-byte WVB modules with SHA-256
`4c0b3ec8c9e7e46a42ea382e53526012a3016b938261bf48fd56f6c6f32b8fc1`.
Independent lowering produced identical 722,849-byte WVO objects with SHA-256
`fbd20dd3370e90775108a7f4060779e4241dc50b2450f19029ddd8bf6a7c1768`.
The 736,768-byte Windows hosted application returned zero for the complete
fixture.

A ten-run local whole-process sample had 31.696 ms median time, 40.344 ms mean
time, and 3,997,696 bytes maximum observed client working set. The first sample
was 112.374 ms while the remaining samples were 24.924 through 46.112 ms; this
is test-harness evidence, not server throughput or parser-only latency.

## Consequences

Windvale clients and the future server can share one deterministic correlation,
database-selection, deadline, freshness, operation, and response-generation
boundary. SQL and direct JSON requests can later lower into the same query and
transaction models without treating arbitrary JSON objects as authority.

The operation body remains intentionally unclaimed. Serving an operation before
its strict body schema, limits, error body, and capability checks exist would
violate this decision.

## Reconsideration triggers

Revisit the envelope when persistent-server concurrency measurements justify a
different total byte ceiling, when connection-scoped database capabilities make
the explicit database ID redundant, or when E-Worker migration evidence needs a
new versioned correlation or freshness field.
