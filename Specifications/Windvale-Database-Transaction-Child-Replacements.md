# Windvale database transaction child replacements

## Status

- Version: `WVCR 1`
- Profile: portable
- Maximum replacement groups: 32
- Maximum new children per group: 64
- Maximum new children per level: 95
- Maximum encoded bytes: 524,288
- Evidence: focused Windows native execution; independent Linux execution pending

## Purpose

`WVCR 1` is the small generic handoff between one rewritten tree level and the
parent level above it. Each group says that one old child page is replaced by
one through 64 consecutive new child pages. The first new child inherits the
old lower bound. Every later child carries the separator that divides it from
the previous replacement.

The format is independent of leaf or branch payloads. The durable leaf-page
planner and each future durable branch level can therefore lower into the same
bounded representation before the parent is rebuilt.

## Encoding

The 40-byte little-endian header is:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u32` | magic `WVCR` |
| 4 | `u32` | version `1` |
| 8 | `u32` | header length `40` |
| 12 | `u32` | flags, zero |
| 16 | `u32` | replacement-group count, 1 through 32 |
| 20 | `u32` | total new-child count, 1 through 95 |
| 24 | `u32` | following map length |
| 28 | `u32` | total encoded length |
| 32 | `u64` | first new page identity |

Each group has a 24-byte header:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u64` | original child page |
| 8 | `u64` | first replacement page |
| 16 | `u32` | replacement count, 1 through 64 |
| 20 | `u32` | following child-record length |

Each child record has a 16-byte header followed by separator bytes:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u64` | new page identity |
| 8 | `u32` | separator length |
| 12 | `u32` | reserved, zero |

The first record in every group has an empty separator. Later records have
nonempty strictly increasing separators. New page identities are consecutive
within and across groups. Old child identities are unique and lower than the
first new page. Arithmetic may not reach the no-page sentinel.

## Validation and limits

The decoder validates the complete envelope before returning readers. It
rejects malformed framing, unsupported fields, duplicate old children,
nonconsecutive new pages, bad separator order, arithmetic exhaustion, count
mismatch, and trailing bytes. Component builders validate their own bounded
child or group bytes; the final encoder revalidates all groups together.

The map is linear in at most 95 new children. Public group and child readers
return owned slices only after complete validation.

## Next use

The branch partitioner consumes `WVCR 1` directly. `WVPP 1` adapts `WVLD 1`
leaf replacements into this form and groups them by their parent at the first
branch level. Durable branch allocation will emit the same contract again for
each next ancestor level.
