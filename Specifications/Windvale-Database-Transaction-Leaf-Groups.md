# Windvale database transaction leaf groups

## Status

- Version: `WVLG 2`
- Profile: portable
- Maximum groups: 32
- Maximum encoded plan: 4,194,304 bytes
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
- `Rewritten`: the group stores one changed final leaf in a complete `WVLP 1`
  partition result.
- `Partitioned`: the group stores two through 33 changed final leaves in a
  complete `WVLP 1` partition result.

The result reports exact group, changed-group, partitioned-group,
replacement-leaf, applied, put, and delete counts. All groups are evaluated as
complete final states. An individually oversized entry is a hard atomic
failure with no plan bytes or counts.

## `WVLG 2` encoding

The 24-byte little-endian plan header is:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u32` | magic `WVLG` |
| 4 | `u32` | version `2` |
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
| 16 | `u32` | status: unchanged `1`, rewritten `2`, partitioned `3` |
| 20 | `u32` | final entry count |
| 24 | `u32` | following payload length |
| 28 | `u32` | reserved, must be zero |

Every changed group has one complete following `WVLP 1` payload. An unchanged
group has no payload. The public group reader validates magic, version, lengths,
count, status, reserved bytes, contiguous mutation ranges, the complete
partition plan, changed flag, one-versus-many leaf status, final entry count,
payload rules, and the absence of trailing bytes before returning a group.

## Performance and memory

Grouping is linear in mutation count and supplied path bytes plus the bulk
merge/partition work for each distinct leaf. It decodes the canonical mutation
set once, scans entries sequentially, and evaluates each affected leaf exactly
once as a final state. The transaction limit bounds the plan to 32 groups and
4 MiB. The practical tighter bound comes from at most 32 old leaves plus the
global 256 KiB mutation set.

Path validation currently owns a copy of at most 16 MiB. The caller still owns
its input, so peak transient path storage can approach twice that limit before
page and plan allocations. This is explicit bounded behavior, not the final
persistent-server admission target. Server measurements will decide whether a
deduplicated path collector is worth the added complexity.

## Verification

The focused native test covers two leaves with three mutations, two mutations
coalesced into one leaf rewrite, exact final leaf values, deterministic bytes,
an unchanged missing delete, one group partitioned into two leaves with exact
replacement counts and separator, malformed path input, an invalid group
index, inconsistent total length, and a corrupt partition plan.

## Exclusions and next step

This plan does not assign new page identities, merge shared ancestors, write
storage, or publish a transaction. The next planner assigns consecutive durable
page identities to every changed group's replacement leaves and constructs one
bottom-up shared replacement tree before one atomic commit batch.
