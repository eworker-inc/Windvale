# Windvale database transaction parent groups

## Status

- Version: `WVPP 1`
- Profile: portable
- Maximum changed parent groups: 32
- Maximum output branches: 95
- Maximum encoded bytes: 3,145,728
- Evidence: focused Windows native execution; independent Linux execution pending

## Purpose

`Databaseˉtransactionˉparentˉgroupsˉplan` joins the durable replacement leaves
from `WVLD 1` to their actual parent branches from the already validated
transaction paths. It groups every changed leaf owned by the same parent,
builds one `WVCR 1` replacement plan for that parent, and applies `WVBP 1`
once to the parent's complete final state.

The result keeps the `WVLD 1` leaf-page plan separate from the parent-group
bytes. It is a pure, deterministic planning boundary: it allocates no durable
branch page identities, performs no storage I/O, and publishes no commit.

## Planning rules

The root depth must be at least two. The planner invokes the leaf-page planner
once, so mutations and paths are decoded and validated through that boundary
before parent lookup. An unchanged leaf plan produces a valid unchanged
40-byte `WVPP 1` value and allocates no parent output.

For every changed old leaf, the planner finds the leaf in the proven path set
and reads the branch immediately above it. Consecutive leaf groups with the
same parent identity are combined. This is complete because ordered B+tree
children owned by one parent form one contiguous key range. Each parent is
rebuilt exactly once, even when several of its children change.

Parent output remains logical `WVBP 1` payload. `WVBD 1` assigns consecutive
durable branch-page identities, encodes checksummed `WVPG 1` pages, and
converts the promoted separators into the same generic `WVCR 1` shape for the
next ancestor level.

## `WVPP 1` encoding

The 40-byte little-endian header is:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u32` | magic `WVPP` |
| 4 | `u32` | version `1` |
| 8 | `u32` | header length `40` |
| 12 | `u32` | flags: bit 0 is changed; no other bits |
| 16 | `u32` | parent-group count, 0 through 32 |
| 20 | `u32` | aggregate output-branch count, 0 through 95 |
| 24 | `u32` | durable leaf data-page count, 0 through 64 |
| 28 | `u32` | input root depth, 2 through 128 |
| 32 | `u32` | total encoded length |
| 36 | `u32` | reserved, zero |

An unchanged value has flags zero and all three counts zero. A changed value
has bit 0 set and all three counts nonzero.

Each changed parent has a 32-byte record followed by one complete `WVBP 1`
payload:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u64` | old parent page identity |
| 8 | `u32` | applied replacement-group count, 1 through 32 |
| 12 | `u32` | replacement-child count, group count through 64 |
| 16 | `u32` | output branch count, 1 through 64 |
| 20 | `u32` | final logical separator count, 1 through 4,159 |
| 24 | `u32` | `WVBP 1` payload length |
| 28 | `u32` | reserved, zero |

Parent identities are unique across the complete value, not merely adjacent
records. The decoder revalidates every embedded partition, its exact counts,
aggregate output count, framing, reserved fields, total length, and absence of
trailing bytes before returning owned bytes.

## Failure and bounds

Invalid depth, leaf planning failure, inconsistent internal leaf output,
missing or malformed parent pages, child-replacement failure, branch
partition failure, oversized output, duplicate parents, and malformed bytes
fail atomically with no partial parent output.

The encoded result is capped at 3 MiB. This remains above the bounded worst
case of 32 maximum-size parent pages plus 64 replacement-child separators and
all framing. Work is bounded by 32 mutations, at
most 32 parent groups, and the existing `WVLD 1`, `WVCR 1`, and `WVBP 1`
limits. Duplicate-parent validation is deliberately quadratic in at most 32
records, avoiding an unbounded set. The current immutable byte builder copies
during concatenation; persistent-server measurement will decide whether a
pre-sized arena is justified without changing the format.

## Verification and next step

The focused native test covers two leaves sharing one parent, two changed
parents below a depth-three root, deterministic output, routing to allocated
leaf identities, no-change output, invalid depth, malformed paths, trailing
bytes, truncated and oversized envelopes, bad magic and version, inconsistent
counts, invalid indexes, and a non-adjacent duplicate-parent attack.

`WVBD 1` now assigns durable identities to every output branch and emits one
replacement plan for the next ancestor level. The next milestone repeats
grouping and allocation until it reaches or grows the root; only then can one
compact log and one inactive superblock publish the complete transaction.
