# Windvale database transaction leaf partition

## Status

- Version: `WVLP 1`
- Profile: portable
- Maximum mutations: 32
- Maximum output leaves: 33
- Maximum final entries: 4,128
- Maximum encoded result: 524,288 bytes
- Evidence: focused Windows native execution; independent Linux execution pending

## Purpose

One transaction leaf group can overflow into more than two leaves. A sequence
of large puts can require several replacements, while later deletes can make a
group fit again even if an earlier intermediate state did not fit.

`Databaseˉtransactionˉleafˉpartitionˉapply` merges one valid `WVTN 1` leaf
with one canonical `WVTM 1` mutation set as a single final state. It then emits
one through 33 ordered final leaves. It never exposes an intermediate leaf and
returns no bytes or counts when any single final entry cannot fit.

## Merge and partition rules

The old leaf and sorted mutations are merged in key order:

- a put inserts or replaces exactly one key;
- a delete removes a present key and is a no-op when the key is absent;
- each put counts as applied, including a byte-identical replacement;
- only a present delete counts as applied; and
- changed is based on the final bytes, not the number of attempted operations.

The merge appends contiguous runs from the old leaf between mutation keys. It
therefore performs at most two body concatenations per mutation instead of one
concatenation per existing entry. After the merge, a second linear scan makes
deterministic greedy partitions. Every output payload is at most the caller's
maximum payload. The first partition has no separator; each later separator is
the exact first key of that partition.

An empty final key set is represented by one valid zero-entry leaf. No leaf
merge or minimum-fill policy is claimed yet.

## `WVLP 1` encoding

The 40-byte little-endian header is:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u32` | magic `WVLP` |
| 4 | `u32` | version `1` |
| 8 | `u32` | header length `40` |
| 12 | `u32` | flags: bit zero means final bytes changed |
| 16 | `u32` | output leaf count, 1 through 33 |
| 20 | `u32` | total final entry count, at most 4,128 |
| 24 | `u32` | applied mutation count |
| 28 | `u32` | put count |
| 32 | `u32` | present-delete count |
| 36 | `u32` | total encoded length |

Each leaf record begins with 16 bytes:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u32` | following separator length |
| 4 | `u32` | leaf entry count |
| 8 | `u32` | following leaf payload length |
| 12 | `u32` | reserved, must be zero |

The separator bytes and complete `WVTN 1` payload follow. Decoding validates
all bounds, counters, leaf structure, exact entry totals, separators, adjacent
key order, trailing bytes, and the rule that a multi-leaf result is changed.

## Limits, performance, and memory

The input leaf is at most 65,408 bytes and `WVTM 1` is at most 256 KiB. One put
can add at most one required leaf because a single entry must fit one admitted
payload. The initial leaf plus 32 puts therefore bounds output at 33 leaves.

Merge work is linear in old entries plus mutations. Partition work is linear in
the merged body. The implementation retains the caller inputs, one merged body,
and one encoded result. All are explicitly bounded; no per-entry object graph,
map, or unbounded queue is allocated. Immutable concatenation occurs per
mutation and per output leaf, not per old entry. Persistent-server benchmarks
will measure whether a future affine byte builder is justified.

## Verification

The focused native test covers replacement, insertion, present and missing
deletes, byte-deterministic output, a true no-change result, three output
leaves, exact separators and lookups, deleting every entry, an early expansion
followed by later deletes that fits one final leaf, an individually oversized
entry, the exact 33-leaf ceiling, malformed mutations, malformed leaves,
invalid payload limits, corrupt
format bytes, trailing bytes, and invalid leaf indices.

## Exclusions and next step

This function partitions one already-grouped logical leaf. It does not validate
transaction paths, assign durable page identities, rewrite parents, or publish
a commit. The transaction tree planner next applies this partitioner to every
leaf group, allocates the complete replacement page set, and rebuilds shared
ancestors once from the bottom up.
