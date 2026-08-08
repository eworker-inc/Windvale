# Decision 0390: Reusable Linux durable multi-chunk publication

- Status: Implemented candidate; Linux execution pending
- Date: 2026-08-08
- Advances: [Decision 0389](0389-Shared-Immutable-Snapshot-Sequence.md), [Decision 0351](0351-Immutable-Snapshot-Compiler-Image-Staging.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native x64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md#hosted-immutable-snapshot-staging-boundary)
- Advanced by: [Decision 0391](0391-Reusable-Windows-Durable-Multi-Chunk-Publication.md)

## Context

The Linux staged-WVO adapter mixed WVO acquisition, manifest-independent host
identity checks, destination alias rejection, sibling naming, partial writes,
flush, reread comparison, rename, directory durability, cleanup, and transaction
state in one 784-line source. The hosted-container publisher needs exactly the
same mutation protocol but selects alternating response snapshots and skips
their 40-byte envelopes.

Copying the transaction would create a second security-sensitive implementation
and another large source file. Moving WVO admission into the transaction would
instead make the shared component format-aware.

## Decision

Add `Linux-X64-Durable-Multi-Chunk-Publication.wva` as a format-neutral platform
transaction. It accepts an already opened directory descriptor, a validated
NUL-terminated destination basename, first and one-past snapshot ordinals,
stride one or two, and a fixed per-snapshot header skip.

The transaction alone owns:

- exclusive `.wvpub-<hex>` sibling creation below the anchored directory;
- exact partial-progress writes directly from immutable snapshot payloads;
- sibling `fsync`, rewind, bounded reread/byte comparison, and exact EOF;
- the Windvale-owned begin/apply state transitions;
- same-directory `renameat` replacement and directory `fsync`;
- pre-replacement sibling cleanup; and
- distinct `0` complete, `1` rejected/unchanged, and `2` indeterminate results.

Keep argument handling, Windvale admission, snapshot-name reopening, host byte
comparison, source/destination identity checks, and directory ownership in the
format adapter. The staged-WVO adapter calls the shared transaction with first
ordinal 2, stride 1, and skip 0. The hosted adapter will call the same function
with first ordinal 3, stride 2, and skip 40.

The extraction reduces the WVO-specific Linux adapter from 784 to 467 lines.
The 412-line transaction is cohesive platform mutation logic, not an arbitrary
numbered fragment.

## Exact local evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Linux staged-WVO adapter WVO | 3,499 | `2ca0989221f55c1b4a4e8de1bf2bf4437f758e10c1211944b36333f0d029c15d` |
| Linux durable multi-chunk WVO | 2,432 | `47a22cd108702d6427fe5be9fca00c3c05f38cb26dd69e51c8648544b3f98e76` |
| Linux staged-WVO publisher | 6,455,773 | `71a70e3bf3c98a7f8a8b951a090a7f83681d25cf064046f7a9d76cd50dabb601` |

The reviewed focused test assembles and pins both new objects, reconstructs the
Windows and Linux applications, and passes its current-host publication and
failure-preservation matrix 1/1 in 6.624 test seconds after a 10.28-second
zero-warning build. On the current Windows host, this is package/link evidence
for the changed Linux transaction rather than Linux process execution.

Actual Linux success, corruption rejection, alias preservation, cleanup, and
durability evidence remains part of the final paired-host gate as requested.
No broader verifier was run.

## Consequences

- Linux durable publication is no longer owned by a WVO-specific source.
- Hosted-container publication can reuse the exact transaction with a different
  admitted snapshot selection and no managed concatenation.
- The C# changes only link and pin canonical WVO objects during the deletion-bound
  Stage 0 packaging phase; no mutation semantics moved into C#.
- The equivalent Windows handle-relative transaction extraction remains next.

## Reconsideration triggers

Revisit this interface if Linux loses same-directory `renameat` semantics, the
directory durability contract changes, mutation gains an idempotency token, or
snapshot payloads no longer remain immutable for the process lifetime. Do not
hide an indeterminate post-replacement failure behind retry or unchanged status.
