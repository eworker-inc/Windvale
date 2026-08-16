# Windvale database transaction root growth

## Status

- Version: `WVRG 1`
- Profile: portable
- Maximum input children: 64
- Maximum packing rounds: 6
- Maximum new pages: 63
- Maximum output root depth: 8
- Evidence: focused Windows native execution; independent Linux execution pending

## Purpose

`Databaseˉtransactionˉrootˉgrowthˉplan` consumes one complete `WVCR 1`
replacement group produced when the old root split into multiple durable
branches. It packs those ordered children into new branch levels until exactly
one durable root remains. It does not write storage or publish a superblock.

This is a bounded tree builder, not a two-child special case. Large separators
may require more than one new level. Every intermediate page and the final root
uses the same target generation and commit sequence.

## Packing and allocation

The input plan contains exactly one group with 2 through 64 children and names
the committed root as its original child. Its consecutive replacement pages
must be the transaction's immediately preceding allocations.

`Databaseˉtransactionˉbranchˉpartitionˉrootˉchildren` converts that ordered
child sequence into the same `WVBP 1` partitions used for existing-parent
rewrites. One output is encoded directly as the new root. Multiple outputs are
encoded as ordinary branch pages and become the child sequence for the next
round. Synthetic pages have no previous-page identity because they do not
replace one corresponding committed page.

Every branch contains at least two children, so each nonfinal round reduces the
child count by at least half. Starting with 64 children therefore needs at most
six rounds and at most 63 new pages. Growth fails before output depth would
exceed the current complete-path ceiling of eight.

## `WVRG 1` encoding

The fixed 168-byte little-endian manifest binds, but does not duplicate, its
input replacement plan and page companion:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u32` | magic `WVRG` |
| 4 | `u32` | version `1` |
| 8 | `u32` | header length `168` |
| 12 | `u32` | flags: changed and root-ready bits exactly `3` |
| 16 | `u32` | durable page size |
| 20 | `u32` | committed input root depth |
| 24 | `u32` | completed output root depth |
| 28 | `u32` | packing-round count |
| 32 | `u32` | aggregate new-page count |
| 36 | `u32` | input replacement-child count |
| 40 | `u32` | input `WVCR 1` byte length |
| 44 | `u32` | page-companion byte length |
| 48 | `u64` | target generation |
| 56 | `u64` | target commit sequence |
| 64 | `u64` | first transaction allocation page |
| 72 | `u64` | first page allocated by root growth |
| 80 | `u64` | next unallocated page |
| 88 | `u64` | completed root page |
| 96 | `u32` | total manifest length, exactly `168` |
| 100 | `u32` | reserved, zero |
| 104 | 32 bytes | SHA-256 of the complete input `WVCR 1` companion |
| 136 | 32 bytes | SHA-256 of the complete page companion |

Decoding validates the fixed manifest. `validate` additionally checks exact
companion lengths and hashes, replays every partition round, and revalidates
each durable page's identity, kind, generation, sequence, predecessor, item
count, and logical payload.

## Failure and bounds

Invalid current state, malformed or inconsistent replacements, depth or page
identity exhaustion, partition failure, excess pages, replacement construction
failure, page encoding failure, invalid manifests, digest mismatch, and invalid
durable pages fail atomically with no partial result.

At 64 KiB pages, the fixed 63-page ceiling bounds the page companion below
4 MiB. Input replacements remain bounded by 512 KiB. Packing work shrinks
geometrically by round. Immutable concatenation remains measurable debt for the
persistent-server benchmark rather than an unbounded behavior.

## Verification and next step

Focused native tests cover direct two-child root growth and the exact boundary
of 64 children, six rounds, 63 pages, and depth eight using 2,000-byte
separators. They also cover exact routing and page metadata, deterministic
output, invalid allocation, exhausted depth, malformed manifests, digest
mismatch, and a digest-bound invalid page. Existing branch-partition tests
protect the shared packing refactor.

The next milestone composes `WVAG 1` and `WVAP 1` in a bounded descending loop,
then invokes `WVRG 1` only when the final old-root replacement has multiple
children.
