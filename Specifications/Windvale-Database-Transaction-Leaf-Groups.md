# Windvale database transaction leaf groups

## Status

- Version: `WVLG 1`
- Profile: portable
- Maximum groups: 32
- Evidence: focused Windows native execution; independent Linux execution pending

## Purpose

`Databaseˉtransactionˉleafˉgroupsˉplan` turns one validated transaction path
set into deterministic per-leaf work. Canonical sorted mutation keys make every
leaf's mutations consecutive. The planner therefore applies each affected leaf
once instead of rebuilding it once per key.

The planner accepts the selected committed snapshot, one `WVTM 1` mutation set,
and its repeated transaction paths. It calls the transaction-path validator at
the boundary, then scans mutations and paths in order. A hard failure returns
no plan bytes and zero counts.

## Group outcomes

Each group has one of three outcomes:

- `Unchanged`: all mutations were valid no-ops; no payload is stored.
- `Rewritten`: the group stores the complete final `WVTN 1` leaf payload.
- `Split_required`: the mutations do not fit one leaf; no partial payload or
  partial mutation counts are exposed.

The result reports exact group, changed-group, split-group, applied, put, and
delete counts. Applied counts cover only complete unchanged or rewritten
groups. Split-required groups are deliberately reapplied by the later split
planner.

## `WVLG 1` encoding

The 24-byte little-endian plan header is:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u32` | magic `WVLG` |
| 4 | `u32` | version `1` |
| 8 | `u32` | header length `24` |
| 12 | `u32` | flags, currently zero |
| 16 | `u32` | group count, 1 through 32 |
| 20 | `u32` | total byte length |

Each group begins with a 32-byte header:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u64` | original leaf page identity |
| 8 | `u32` | first mutation index |
| 12 | `u32` | mutation count |
| 16 | `u32` | status: unchanged `1`, rewritten `2`, split required `3` |
| 20 | `u32` | final entry count, or zero when split is required |
| 24 | `u32` | following payload length |
| 28 | `u32` | reserved, must be zero |

Only a rewritten group has a non-empty following payload. The public group
reader validates magic, version, lengths, count, status, reserved bytes,
contiguous mutation ranges, rewritten `WVTN 1` leaf structure and entry count,
payload rules, and the absence of trailing bytes before returning a group.

## Performance and memory

Grouping is linear in mutation count and supplied path bytes. It decodes the
canonical mutation set once, scans entries sequentially, and never searches a
leaf more than once per group. The transaction limit bounds the plan to 32
groups and the rewritten payload bytes to about 2 MiB at the largest admitted
page size.

Path validation currently owns a copy of at most 16 MiB. The caller still owns
its input, so peak transient path storage can approach twice that limit before
page and plan allocations. This is explicit bounded behavior, not the final
persistent-server admission target. Server measurements will decide whether a
deduplicated path collector is worth the added complexity.

## Verification

The focused native test covers two leaves with three mutations, two mutations
coalesced into one leaf rewrite, exact final leaf values, deterministic bytes,
an unchanged missing delete, a split-required large put, malformed path input,
an invalid group index, inconsistent total length, and a corrupt rewritten leaf.

## Exclusions and next step

This plan does not split a leaf, assign new page identities, merge shared
ancestors, write storage, or publish a transaction. The next planner handles
split-required groups and constructs one bottom-up replacement tree shared by
all groups. One commit batch must then publish every replacement atomically.
