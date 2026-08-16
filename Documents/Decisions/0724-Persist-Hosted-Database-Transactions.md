# Decision 0724: Persist hosted database transactions

- Status: Implemented candidate; cross-host qualification pending
- Date: 2026-08-16
- Advances: [Decision 0723](0723-Compose-Segmented-Hosted-Storage.md) and [Decision 0721](0721-Publish-One-Completed-Transaction-Tree.md)
- Contract: [Persistent transaction writer](../../Specifications/Windvale-Database-Persistent-Transaction-Writer.md)

## Context

The portable transaction coordinator could already turn one bounded canonical
mutation set and its complete paths into one crash-safe publication. The hosted
database still had no component that gathered those paths from the provider,
executed the publication, required a fresh reopen, or retained useful timing,
I/O, and memory evidence across requests.

Composing that real path produced 77 record declarations. The native x64
backend retained a historical 64-record internal-tag limit even though WVB and
the surrounding nominal directory already admit 128 total declarations.

## Decision

Add two focused hosted layers:

1. a durable transaction writer that validates the selected snapshot and
   mutation set, gathers one provider-backed path per mutation, invokes the
   canonical transaction coordinator, and executes the existing publication;
2. a persistent session that sequences requests, requires reopen after a
   commit or uncertain mutation, and retains saturating request, recovery, I/O,
   byte, action, memory-bound, and caller-supplied monotonic-tick counters.

Keep the writer bounded to 32 mutations, depth eight, and 4 MiB of counted live
logical byte values. The memory counter is deterministic payload evidence, not
an allocator or RSS measurement. Keep clock ownership outside the component so
tests remain deterministic and a future server can name its actual clock unit.

Retain the 128-total-nominal limit while assigning previously unused internal
type tags 12 through 63 to record declaration indices 64 through 115. Scalar
tags remain 1 through 11, variants remain 64 through 127, enums remain 128
through 191, and the earlier record range remains 192 through 255. Canonical
nominal ordering puts records first, so the new mapping is reversible. Variants
retain their global declaration-index-below-64 rule.

Refresh the segmented staging producer and hosted enum-request producer from
current Windvale source so ordinary packaging admits and preserves the expanded
record range on both host targets.

## Evidence and consequences

The focused Windows database target passes all four checkpoints. It starts
from a recovered depth-two database, commits two mutations routed to different
leaves as one four-action publication, reopens and reads both values, and proves
that repeating the same mutation set performs no write. It checks session
sequence, reopen, provider-call, path/read-byte, append-byte, retained-bound,
and explicit-tick counters.

The first run recreated compiler and hosted-application caches after candidate
identity changes. The final rerun reported a 2,590 ms tools cache hit,
33,190 ms hosted-storage prerequisite, 26,020 ms hosted tree-reader prerequisite,
and 107,580 ms changed writer build and execution. These are development
pipeline timings, not database throughput results.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Segmented staging-producer WVB | 532,490 | `72d738268580584b967deca648bb12bc80bf3243d10600921dfc8ddf670be623` |
| Windows segmented staging producer | 7,756,800 | `6cc939dc3f3e319f036d633626e867078c490564db83814add90b31936bc2bfd` |
| Linux segmented staging producer | 7,757,824 | `7b9d1b1124b0d7cb09bc9b3d9bfd7c916e7272a40d3e029a39b444c788e1b758` |
| Hosted enum-request WVB | 42,088 | `ede2310a39e4963d517834ef6bc800b27dcaf91dca7fe9602627dc7567650f85` |
| Windows hosted enum-request producer | 461,824 | `1089a5c07290d1b7707e0b16ca30692c68435defcda51b7b0ace4de280ffddbc` |
| Linux hosted enum-request producer | 462,848 | `01d0d02ce041fd85e7aea537d21ad466600f32da050f41d20bb24c957ae04e21` |

The component remains a serialized library boundary, not a concurrent server.
Independent Linux execution, full crash injection for multi-leaf transactions,
process RSS measurement, a writer queue, group commit, secondary indexes,
reclamation, and external performance comparison remain pending.

## Reconsideration triggers

Version the internal type encoding if additional scalar, record, enum, or
variant identities exhaust the disjoint byte ranges. Replace logical retained-
byte evidence with allocator-owned accounting only when Windvale has an
allocator contract that can report it exactly. Do not add concurrency by
allowing old immutable session values to race; introduce an explicit owned
queue and cancellation boundary first.
