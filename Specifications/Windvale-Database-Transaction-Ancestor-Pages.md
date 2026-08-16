# Windvale database transaction ancestor pages

## Status

- Version: `WVAP 1`
- Profile: portable
- Maximum parent groups: 32
- Maximum durable ancestor pages: 95
- Maximum manifest bytes: 589,824
- Evidence: focused Windows native execution; independent Linux execution pending

## Purpose

`Databaseˉtransactionˉancestorˉpagesˉplan` turns one complete logical
`WVAG 1` ancestor level into checksummed `WVPG 1` pages. It also emits one
generic `WVCR 1` replacement plan for the next ancestor level. The operation
does not write storage or publish a superblock.

The caller supplies the page identity at which this transaction began and the
first still-unallocated page identity. Keeping those values separate allows
earlier leaf and branch rounds to share one append-only allocation range.

## Allocation rules

Every `WVAG 1` output receives one consecutive durable page identity beginning
at `Firstˉpage`. The first output for a logical parent records that parent's
committed page identity as its previous page. Later split outputs use the
no-page sentinel. Every page uses the caller's next generation and commit
sequence.

When child level one has exactly one parent and one output, that output is the
completed durable root and `Rootˉready` is true. All other outputs are ordinary
branch pages. This includes multiple outputs replacing the old root: a later
root-growth step must point a new root at those branches.

The operation accepts committed root depths three through eight, matching the
current complete-path contract. It requires the first transaction allocation
page to equal the committed page count. The enclosing transaction must prove
that `Firstˉpage` immediately follows all earlier allocations before it
publishes the combined result.

## `WVAP 1` encoding

The manifest uses a 160-byte little-endian header followed by one complete
`WVCR 1` payload:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u32` | magic `WVAP` |
| 4 | `u32` | version `1` |
| 8 | `u32` | header length `160` |
| 12 | `u32` | flags: bit 0 changed, bit 1 root ready |
| 16 | `u32` | durable page size |
| 20 | `u32` | committed root depth |
| 24 | `u32` | input child level |
| 28 | `u32` | parent-group count |
| 32 | `u32` | durable ancestor-page count |
| 36 | `u32` | replacement-plan byte length |
| 40 | `u64` | target generation |
| 48 | `u64` | target commit sequence |
| 56 | `u64` | first transaction allocation page |
| 64 | `u64` | first page allocated by this round |
| 72 | `u64` | completed root page, or no-page sentinel |
| 80 | `u32` | companion `WVAG 1` byte length |
| 84 | `u32` | companion durable-page byte length |
| 88 | `u32` | total manifest length |
| 92 | `u32` | reserved, zero |
| 96 | 32 bytes | SHA-256 of the complete `WVAG 1` companion |
| 128 | 32 bytes | SHA-256 of the complete page companion |

The manifest binds, but does not duplicate, its potentially multi-megabyte
logical-group and page companions. Decoding validates the manifest and its
embedded replacement plan. `validate` additionally requires exact companion
lengths and hashes and revalidates every group, partition, replacement child,
durable page, logical payload, separator, identity, generation, sequence,
kind, predecessor, item count, and aggregate count.

## Failure and bounds

Invalid current state, malformed groups, inconsistent allocation or generation,
exhausted page identities, malformed partitions or replacements, failed page
encoding, invalid manifests, digest mismatch, and invalid durable pages fail
atomically with no partial result.

At most 32 parent groups produce at most 95 pages. The manifest is capped at
576 KiB, its `WVAG 1` companion at 3 MiB, and its replacement payload at
512 KiB. Page bytes are exactly page count times the validated 4 through
64 KiB page size. Page-identity arithmetic is checked before allocation.

The immutable byte builder can copy while accumulating page output. Persistent
server benchmarks must measure this cost and peak memory before selecting a
pre-sized arena or streaming writer.

## Verification and next step

Focused native executables cover a completed depth-three root, two independent
depth-four intermediate branches, deterministic output, exact predecessor and
replacement identities, manifest validation, invalid allocation, truncation,
oversize input, bad magic, companion digest mismatch, and a digest-bound but
invalid durable page.

[`WVRG 1`](Windvale-Database-Transaction-Root-Growth.md) constructs a new root
when replacement of the old root produces multiple branches.
[`WVTC 1`](Windvale-Database-Transaction-Tree-Completion.md) now alternates
`WVAG 1` and `WVAP 1`, accounts for the complete allocation and memory budget,
and invokes root growth only when required.
