# Windvale database transaction ancestor groups

## Status

- Version: `WVAG 1`
- Profile: portable
- Maximum validated root depth: 8
- Maximum changed parent groups: 32
- Maximum input children and output branches: 95
- Maximum encoded bytes: 3,145,728
- Evidence: focused Windows native execution; independent Linux execution pending

## Purpose

`Databaseˉtransactionˉancestorˉgroupsˉplan` is the reusable logical step for
moving a transaction upward after durable child pages exist. It accepts one
complete `WVCR 1` replacement plan, finds each old child at an explicit level
of the already validated mutation paths, groups children owned by the same
parent, and applies `WVBP 1` exactly once to every parent.

The result allocates no pages and performs no I/O. A later ancestor-page
allocator assigns durable identities to the logical output branches and emits
the next `WVCR 1` level. Repeating those two operations reaches the root.

## Planning rules

The selected snapshot and canonical mutations are revalidated through the
`WVTP` path boundary. Root depth is 3 through 8, matching the current 16 MiB
bounded path envelope. `Childˉlevel` is 1 through `root_depth - 2`, where zero
is the committed root. Thus level 1 rebuilds the root, while larger values
rebuild intermediate branches.

Every input replacement group must name an old child present at the requested
level. Consecutive groups owned by one parent are combined into one bounded
replacement plan and applied to the parent's complete committed payload.
When the parent changes, the previous parent is finalized before the next is
started. Non-adjacent duplicate parent output is rejected by the decoder,
which proves that each parent appears exactly once.

## `WVAG 1` encoding

The 64-byte little-endian header is:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u32` | magic `WVAG` |
| 4 | `u32` | version `1` |
| 8 | `u32` | header length `64` |
| 12 | `u32` | flags: bit 0 changed; no other bits |
| 16 | `u32` | durable page size |
| 20 | `u32` | committed root depth |
| 24 | `u32` | input child level |
| 28 | `u32` | output parent-group count |
| 32 | `u32` | aggregate logical output-branch count |
| 36 | `u32` | consumed replacement-group count |
| 40 | `u32` | consumed replacement-child count |
| 44 | `u32` | total encoded length |
| 48 | `u64` | first input replacement page |
| 56 | `u64` | reserved, zero |

Each output parent has a 32-byte record followed by one complete `WVBP 1`
payload:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u64` | committed parent page identity |
| 8 | `u32` | applied replacement-group count |
| 12 | `u32` | applied replacement-child count |
| 16 | `u32` | logical output-branch count |
| 20 | `u32` | aggregate final entry count |
| 24 | `u32` | `WVBP 1` payload length |
| 28 | `u32` | reserved, zero |

The decoder revalidates every embedded partition, exact aggregate counts,
unique parent identities, old-versus-new page ordering, framing, reserved
fields, total length, and absence of trailing bytes before exposing a group.

## Failure and bounds

Invalid depth, mutations, paths, replacements, missing children, malformed
parents, partition failure, aggregate overflow, oversized output, malformed
encoding, duplicate parents, and inconsistent counts fail atomically with no
partial output.

Work is linear in at most 32 paths, 32 replacement groups, 95 replacement
children, and the emitted bounded node bytes. Duplicate-parent validation is
quadratic only in the fixed maximum of 32 output records. The 3 MiB envelope
matches the previous parent-group boundary. Immutable byte concatenation is
bounded but remains subject to persistent-server allocation measurement.

## Verification and next step

Focused native tests cover two changed children sharing a depth-three root,
two changed branches under separate depth-four grandparents, deterministic
output, route preservation, malformed paths, invalid levels, missing child
identities, truncated and trailing encodings, bad magic, and the 3 MiB size
boundary.

[`WVAP 1`](Windvale-Database-Transaction-Ancestor-Pages.md) assigns durable
page identities to every `WVAG 1` output and emits the next `WVCR 1` plan.
[`WVTC 1`](Windvale-Database-Transaction-Tree-Completion.md) now repeats both
steps until one root is complete or a new root level is required.
