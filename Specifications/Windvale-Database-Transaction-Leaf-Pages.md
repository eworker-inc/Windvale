# Windvale database transaction leaf pages

## Status

- Version: `WVLD 1`
- Profile: portable
- Maximum changed groups: 32
- Maximum data pages: 64
- Maximum encoded plan: 5,242,880 bytes
- Evidence: focused Windows native execution; independent Linux execution pending

## Purpose

`Databaseˉtransactionˉleafˉpagesˉplan` is the durable handoff between final
leaf mutation planning and shared-ancestor rewriting. It accepts one selected
committed snapshot, canonical `WVTM 1` mutations, and their validated paths.
It consumes `WVLG 2` once, assigns consecutive new page identities, and emits
complete checksummed `WVPG 1` pages plus a compact replacement map.

Unchanged groups allocate no page. If every group is unchanged, the result is
a valid 72-byte no-change plan that retains the current generation and commit
sequence. A changed result advances both values exactly once. Any validation,
capacity, arithmetic, or page-encoding failure returns no plan bytes.

## Page allocation

The first new page identity is the current committed page count. All data
pages are consecutive in changed-group and partition order. A changed group
may replace one old leaf with one through 33 pages, while the transaction-wide
limit is 64 pages.

The first replacement page records the old leaf as `previous_page`. Additional
pages created by the same split use `NO_PAGE`; they have no distinct obsolete
predecessor. A single replacement of a depth-one root remains a durable `Root`
page. Every other emitted data page is a durable `Leaf`; root growth and parent
pages belong to the following ancestor planner.

## `WVLD 1` encoding

The 72-byte little-endian header is:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u32` | magic `WVLD` |
| 4 | `u32` | version `1` |
| 8 | `u32` | header length `72` |
| 12 | `u32` | flags: bit 0 means changed |
| 16 | `u32` | durable page size |
| 20 | `u32` | input root depth |
| 24 | `u32` | changed replacement-group count |
| 28 | `u32` | emitted data-page count |
| 32 | `u32` | replacement-map byte length |
| 36 | `u32` | reserved, zero |
| 40 | `u64` | target generation |
| 48 | `u64` | target commit sequence |
| 56 | `u64` | first emitted page, or `NO_PAGE` |
| 64 | `u32` | total encoded length |
| 68 | `u32` | reserved, zero |

The replacement map follows the header. Each changed group begins with:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u64` | original leaf page |
| 8 | `u64` | first replacement page |
| 16 | `u32` | replacement-leaf count |
| 20 | `u32` | aggregate final entry count |
| 24 | `u32` | following leaf-record bytes |
| 28 | `u32` | reserved, zero |

Each leaf record is a 16-byte header followed by its separator bytes:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u64` | replacement page identity |
| 8 | `u32` | separator length |
| 12 | `u32` | leaf entry count |

The first leaf in a group has an empty separator. Every later leaf has its
non-empty first key as the separator. The map is followed by exactly
`data_page_count * page_size` bytes of complete `WVPG 1` pages.

## Validation boundary

The decoder rejects bad magic, versions, flags, sizes, limits, arithmetic,
reserved fields, total lengths, map framing, nonconsecutive pages, duplicate
or non-old source identities, invalid separators, inconsistent aggregate
counts, and trailing bytes. It independently decodes every durable page and
tree node, verifies the exact page identity, generation, sequence, kind,
predecessor, item count, leaf kind, and later-leaf separator, and exposes
replacement readers only after the complete envelope passes.

## Performance and memory

Planning is linear in the bounded group plan, emitted node bytes, and emitted
page bytes. It does not reapply mutations. At most 64 fixed-size pages and one
compact map are emitted; the 5 MiB envelope limit is explicit. The current
portable byte builder copies while concatenating pages, so this is bounded but
not yet the final persistent-server allocation path. Persistent-server
profiles will decide whether a pre-sized page arena is required.

## Verification

The focused native test covers deterministic two-group allocation, exact page
IDs with source identities deliberately out of numeric key order, predecessor
links, decoded values, a no-change plan, a depth-one root
replacement, a two-page leaf partition and separator, malformed paths, page-ID
exhaustion, trailing bytes, checksum corruption, and invalid replacement
indexes.

## Exclusions and next step

`WVLD 1` does not rewrite branch pages, grow a root after leaf partitioning,
append a commit record, publish a superblock, or write storage. The next
planner consumes its replacement map and the already validated transaction
paths to rewrite each shared ancestor once, bottom-up.
