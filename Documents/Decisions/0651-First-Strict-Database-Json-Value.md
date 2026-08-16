# Decision 0651: First strict database JSON value

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Windvale Database proposal](../Project/Windvale-Database-Proposal.md)
- Defines: [strict database JSON value](../../Specifications/Windvale-Database-Json-Value.md)
- Informed by: EWDB protocol JSON inspection and limits

## Context

EWDB showed that deserializing a contract type is not a sufficient JSON safety
boundary. Its protocol first limits bytes, depth, nodes, object properties,
array items, strings, and numbers; rejects comments, trailing commas, duplicate
or unsafe property names, and unknown contract members; then applies semantic
validation. Windvale needs the same separation before JSON requests, JSON
columns, query values, and SQL parameters can share one database model.

The EWDB request and response ceilings are larger than Windvale's current
61 KiB logical-record payload. Copying those limits would create a contract the
current database could not store or test honestly.

## Decision

- Add one portable strict JSON admission module with a 64 KiB document ceiling.
- Support all JSON structural kinds while retaining exact admitted number and
  document spelling.
- Bound depth, nodes, properties, array items, decoded strings, number tokens,
  property names, and escape work explicitly.
- Decode object names during admission, reject semantic duplicates, and retain
  EWDB's `__proto__`, `prototype`, and `constructor` rejection.
- Validate UTF-16 escape pairs and expose exact decoded UTF-8 string bytes.
- Return no partial tree. Success owns one exact document byte copy plus root
  kind and node count; future protocol and typed-row layers inspect it through
  bounded APIs.
- Keep request/response envelope policy separate so unknown members and
  operation-specific semantics can be strict without changing JSON itself.

## Evidence

The focused native fixture covers every kind, exact spelling retention, strict
string decoding, semantic duplicate names, unsafe names, invalid UTF-8,
malformed literals/numbers/escapes/surrogates, comments, trailing commas,
truncation, and missing separators. It executes the accepted maximums and their
one-over-limit counterparts for depth, nodes, properties, array items, string
bytes, number bytes, escapes, and total document bytes.

Two independent focused builds produced identical 47,611-byte WVB modules with
SHA-256
`da7a4a84ad209df7c135fea48679929c630e8b170239b0f21e7d1735d91c185e`.
Independent lowering produced identical 501,066-byte WVO objects with SHA-256
`f1242341f0c4da83a24ae363acc80618d9e58eb5fd08c340603f7822ceab0a80`.
The packaged 515,072-byte Windows application had SHA-256
`c63d3258dcf1c457bb0c8048c4f75465135079e413484a93b09421eb26d6517a`
and returned zero for the complete fixture.

A ten-run local development sample of the complete maximum-and-malformed
fixture had 56.512 ms median process time, 59.367 ms mean process time, and
8,060,928 bytes maximum observed client working set. That is a whole-process
test-harness measurement, not parser-only throughput.

## Consequences

Windvale now has a concrete JSON value boundary that can be reused by a strict
database protocol and a future JSON field kind without depending on .NET JSON
objects. The accepted limits fit the current logical row boundary and make
malicious work predictable.

The implementation deliberately does not claim a request protocol, canonical
JSON writer, numeric coercion, JSON query semantics, or PostgreSQL-compatible
JSON behavior. Those remain separate milestones.

## Reconsideration triggers

Revisit the limits when server concurrency measurements provide a real total
memory budget, when E-Worker migration data demonstrates a larger required
value, or when measured property-name or escape-heavy workloads justify a
different bounded representation.
