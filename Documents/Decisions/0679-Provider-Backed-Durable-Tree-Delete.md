# Decision 0679: Provider-backed durable tree delete

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Windvale Database proposal](../Project/Windvale-Database-Proposal.md)
- Extends: [durable full-path delete](0668-Durable-Full-Path-Database-Delete.md)
- Defines: [hosted tree delete](../../Specifications/Windvale-Database-Hosted-Tree-Delete.md)

## Context

Windvale could construct a capability-free full-path delete commit but could
not discover that path from durable provider storage or publish the result.
The hosted upsert image was already close to the fixed 4 MiB native-object
ceiling, so adding delete to it would weaken a measured memory boundary.

The original `Tree-Path-Upsert.wv` also owned both upsert and delete. That made
every hosted delete carry branch-split and upsert code that it never executes.

## Decision

- Give portable delete its own `Tree-Path-Delete.wv` transaction and focused
  test owner. `Tree-Path-Upsert.wv` remains the single upsert transaction.
- Extract provider-backed root-to-leaf discovery into
  `Durable-Tree-Path.wv`. It performs one bounded read per level and copies
  borrowed provider bytes before the next call.
- Add `Durable-Tree-Delete.wv` as a separate hosted component. It validates a
  stable committed snapshot, calls the portable delete, and publishes a
  present deletion through the existing four-action executor.
- Treat a missing key as `Missing` with zero actions and no provider mutation.
- Preserve typed storage, page, node, transaction, and publication errors.
  Recovery remains explicit; an uncertain mutation is never replayed.
- Keep the ordinary 4 MiB object ceiling unchanged and route the portable and
  hosted delete through one focused `host-tree-delete` development target.

## Evidence

The portable upsert and delete test WVOs are 3,153,072 and 2,744,131 bytes and
their packaged Windows applications return zero. The hosted delete fixture
produces a 213,286-byte WVB with SHA-256
`eef44baab3a1a97c326975ce32baca098d883220a712817b315442ef50fdbf06`
and a 4,050,276-byte WVO with SHA-256
`79eb31869c4a4a4e2e870d5b54a4e6eb0decf61c566c3a0b5dec13233a7cc938`.
The object retains 144,028 bytes below the fixed limit.

The 4,076,032-byte hosted Windows application deletes key `2` from the
20,992-byte generation-2 database and commits a 33,280-byte generation 3. It
proves the key is absent, key `3` still maps to value `30`, the replacement
root routes to the replacement leaf, and a repeated delete leaves the file
byte-identical. All five action-bound interruptions return their expected
markers, restart with result zero, and converge on 33,280 bytes.

Ten fresh-file runs measured 59.625 ms minimum, 61.648 ms median, 62.103 ms
mean, and 67.163 ms maximum whole-process latency. Peak sampled client working
set was 10,715,136 bytes. These are development measurements of one cold
whole-process delete, not server throughput.

Changed-file plan verification passes 24 general and 129 native routing
cases. The normal focused wrapper still fails closed before database execution
at the existing native staging `Unsupportedˉmodule` bootstrap boundary; the
recorded Windows execution used the qualified direct cached build driver,
native lowerer, linker, and hosted packager path.

## Consequences

Windvale Database can now delete a present key through provider-backed durable
storage at every admitted path depth and can prove a missing delete causes no
write. Upsert and delete load only their owned mutation code, which improves
native object headroom and keeps memory cost visible.

The next database layer can compose provider-backed mutations into atomic
multi-record transactions. Page merge, safe reclamation, secondary-index
maintenance, concurrent-writer coordination, and a persistent request server
remain separate milestones.

## Reconsideration triggers

Revisit component separation only when native dead-code elimination or linked
library distribution can prove an equal or smaller bounded image. Revisit
stable separators when measured sparse-tree cost and snapshot-safe reclamation
justify bounded merge.
