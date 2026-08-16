# Decision 0673: Committed-generation database range cursor

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Defines: [durable range scan](../../Specifications/Windvale-Database-Durable-Range-Scan.md)
- Extends: [physical leaf scan](../../Specifications/Windvale-Database-Tree-Leaf-Operations.md)
- Advances: [Windvale Database proposal](../Project/Windvale-Database-Proposal.md)

## Context

The portable leaf scanner returned one bounded range from one decoded leaf,
but a database caller could not continue through the next durable leaf. A
single accumulated result would copy rows repeatedly, grow with the logical
result, and work against the database memory contract. Adding sibling pointers
would instead revise the durable tree format and every mutation invariant.

The established hosted tree-reader test image is also close to the ordinary
4 MiB native object limit. Combining publication, lookup, and the new scan
closure crossed that limit even though each owned component remained bounded.

## Decision

- Add one hosted cursor-page operation that visits at most one root-to-leaf
  path and returns entries from at most one physical leaf.
- Resume within a leaf with the last returned key exclusive. Resume across a
  leaf boundary with the inherited upper separator inclusive.
- Pin database generation, committed sequence, root identity, provider
  generation, and exact provider length across calls. Never silently restart
  a cursor against a newer commit.
- Borrow the packed entries and last key from the final provider response, but
  copy the small resume key so callers can consume one page and continue.
- Retain limits of depth 32, key length 4,096 bytes, and 500 rows per call.
- Keep the hosted scan fixture in its own native target. Its prerequisite chain
  reuses host storage and the hosted tree reader to produce exact committed
  generation-two and generation-three files.
- Add the focused `host-tree-scan` target and the composite `tree-scan` target
  so shared portable leaf changes execute both the portable and hosted owners
  without selecting the complete database suite.

## Evidence

Two source builds produced identical 134,335-byte library WVB modules at
SHA-256 `222da652cf03901b8463d800defff8cda6cc3012d5fca26165de12c60fb4365b`.
Two hosted test builds produced identical 145,759-byte WVB modules at SHA-256
`9cb6dc5463a58262806c8e933e91a5d0c58a309c78487ee6bc25b740eceef1ff`.
Independent lowering produced identical 2,326,174-byte WVO objects at SHA-256
`7590c0b42ab715a35066755adb609ee9fa8623cd82af4c3534a5161205287ac3`;
the WVO checker accepted the result. The packaged 2,349,056-byte Windows
application returned zero against both generations without changing either
database file.

The generation-two file is 20,992 bytes at SHA-256
`9a72e1a495b68196b0f94ca2c90b65b1ae077a95ab64a4c512e992852945cd47`.
The generation-three file is 33,280 bytes at SHA-256
`7d12696ee44e6a23ea8eca9308616753d854000654f6f0845d514e9af97c7a26`.
Execution covers unbounded and bounded cross-leaf traversal, zero-row boundary
pages, one-row local continuation, exact inclusive end handling, invalid
pre-I/O requests, database-snapshot mismatch, and provider-snapshot mismatch.

Twenty-one cold-process executions of the complete generation-three fixture
had 46.702 ms median, 47.321 ms mean, 44.716 ms minimum, and 51.641 ms maximum
time. One-millisecond working-set sampling observed at most 8,458,240 bytes.
These figures include process startup and several cursor scenarios; they are
not server throughput. Changed-file planning passes 24 general and 128 native
routing cases, and both development scripts pass shell syntax checks.

The normal Windows database gate reaches its compiler-tool preparation but the
current compiler WVB then reports `native x64 staging status=Unsupported_module`.
The independently built scan WVB, WVO, packaged application, and durable-file
executions above remain the focused evidence; cross-host qualification is not
claimed.

## Consequences

Windvale Database can now stream a stable ascending range across any admitted
committed tree depth with memory bounded by one path, one physical leaf
response, and one resume key. The durable format and mutation paths do not gain
sibling-link maintenance.

Server-owned cursor lifetime, retention pins, timeout and cancellation,
reverse traversal, read-ahead, query-row decoding, and performance comparison
against SQLite and PostgreSQL remain later layers.

## Reconsideration triggers

Reconsider sibling links only if measured repeated root routing dominates real
server scans and mutation cost remains bounded. Reconsider borrowed page output
when a server transport requires ownership beyond the next provider call.
