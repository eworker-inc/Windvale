# Windvale database transaction branch partition

## Status

- Version: `WVBP 1`
- Profile: portable
- Maximum output branches: 64
- Maximum logical separators: 4,159
- Maximum encoded bytes: 5,242,880
- Evidence: focused Windows native execution; independent Linux execution pending

## Purpose

`Databaseˉtransactionˉbranchˉpartitionˉapply` rebuilds one `WVTN 1` branch
from one validated `WVCR 1` child-replacement plan. It applies every matching
group once, preserves untouched children and separators, inserts all split
children in order, and emits one or more bounded replacement branches.

This is the shared-parent rewrite core. It does not repeatedly call the
single-child branch API and does not stop at temporary overflow. The complete
final child sequence is built once and partitioned once.

`Databaseˉtransactionˉbranchˉpartitionˉrootˉchildren` uses the same packing
core for one complete `WVCR 1` group without an existing parent payload. This
is the new-root boundary: it converts already durable split-root children into
one or more logical parent branches without changing `WVBP 1`.

## Replacement semantics

For one old child, the first replacement inherits the old child's lower
bound. Each later replacement separator must be strictly above that bound and
strictly below the old child's upper bound. The final replacement inherits the
old upper bound. A replacement plan must match every named old child exactly
once; an absent or duplicate match is an atomic failure.

All committed child identities must be below `WVCR.first_page`, while all new
identities are consecutive at or above it. This keeps new children disjoint
from untouched committed children without an unbounded duplicate set.

## Partitioning

If the final branch fits the caller's exact payload ceiling, `WVBP 1` contains
one branch. Otherwise the planner greedily retains the largest fitting prefix
and promotes the next separator between output branches. Every output branch
has at least one local separator and two children.

If only the final separator overflows, the planner backtracks one entry: that
previous separator is promoted and the final entry starts the right branch.
This avoids producing an invalid one-child branch. A final sequence with too
few children for two nonempty branches fails explicitly as `cannot_partition`.
An individual entry that cannot fit fails as `full`.

## `WVBP 1` encoding

The 40-byte little-endian header is:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u32` | magic `WVBP` |
| 4 | `u32` | version `1` |
| 8 | `u32` | header length `40` |
| 12 | `u32` | flags, changed bit exactly `1` |
| 16 | `u32` | output branch count, 1 through 64 |
| 20 | `u32` | final logical separator count |
| 24 | `u32` | applied replacement-group count |
| 28 | `u32` | total encoded length |
| 32 | `u32` | exact maximum node payload |
| 36 | `u32` | reserved, zero |

Each output begins with a 16-byte record followed by its promoted separator
and complete `WVTN 1` branch payload:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u32` | promoted-separator length |
| 4 | `u32` | local branch entry count |
| 8 | `u32` | branch payload length |
| 12 | `u32` | reserved, zero |

The first output separator is empty. Every later separator is nonempty and
lies strictly between the previous branch's last local separator and the next
branch's first local separator. The sum of local separators plus promoted
separators equals the final logical count.

## Validation, performance, and memory

The input branch, replacement plan, child bounds, exact counts, and output are
validated before bytes are returned. The decoder revalidates every branch
node, promoted range, payload ceiling, aggregate count, framing, and absence
of trailing bytes.

Work is linear in the original branch, at most 64 new children, and emitted
branch bytes, plus at most 32 bounded group lookups per original child. Memory
is explicitly bounded by the 512 KiB replacement plan and 5 MiB result. The
portable byte builder still copies during concatenation; persistent-server
profiles will determine whether to replace construction with a pre-sized
arena without changing the format.

## Verification and next step

The focused native test covers two changed children in one shared parent,
unchanged middle routing, a four-child replacement split into two branches,
the final-separator backtrack, deterministic output, invalid bounds, a missing
child, malformed replacement bytes, trailing output, and invalid indexes.

Root-growth tests additionally cover the root-children entry point with one
fitting output and a two-round large-separator partition.

`WVPP 1` now groups `WVCR 1` replacements by parent from the validated
transaction paths and invokes this partitioner once per affected parent.
[`WVRG 1`](Windvale-Database-Transaction-Root-Growth.md) now uses the same core
to allocate as many bounded new levels as required after an old-root split.
