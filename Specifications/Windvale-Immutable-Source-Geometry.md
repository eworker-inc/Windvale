# Windvale immutable source geometry

## Status and scope

`WVSG 1` maps a bounded logical sequence of immutable source bytes to ordered
regions in a larger output image. It allows native tools to acquire source
bytes from multiple sub-4-MiB resources without joining the complete source or
output into one Windvale `bytes` value.

This is geometry and acquisition evidence, not content identity or authority.
A consuming tool must still validate the semantic plan that selects the
regions and must pass the acquired bytes through the format-specific
constructor or verifier. `WVSG` is deliberately distinct from the existing
package-oriented `WVRS` resource store and the `WVHS` streaming-digest plan.

## `WVSG 1` header

All integers are little-endian `u32`. The 32-byte header is followed by one
20-byte chunk record and one 16-byte region record per declared item.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVSG`, `0x47535657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | Exact header and record extent |
| 12 | 4 | chunk count | `1` through `31` |
| 16 | 4 | region count | `1` through `16` |
| 20 | 4 | logical source bytes | `1` through 64 MiB |
| 24 | 4 | output image bytes | `1` through 128 MiB |
| 28 | 4 | reserved | Zero |

## Chunk records

Each chunk record contains its zero-based ordinal, logical source offset,
resource offset, selected logical byte count, and complete resource byte
count. Chunk ordinals and logical extents are contiguous and cover the entire
logical source sequence exactly once. A complete resource is nonempty and at
most 4 MiB; the selected nonempty extent must fit within it.

The resource for chunk `N` is named `<prefix>.chunk-N`. Prefix UTF-8 length is
one through 4,076 bytes so the derived name remains bounded. Consumers must
reject aliases between control, source, and output resource names before
mutation.

## Region records

Each region record contains its zero-based ordinal, logical source offset,
output-image offset, and byte count. Logical offsets are contiguous and cover
the source sequence exactly. Output ranges are ordered, nonoverlapping, and
contained in the declared image. A zero-length region is permitted so a fixed
semantic region table can represent target-specific absent data without
renumbering later regions.

Gaps in the output image are intentional: a format-specific constructor owns
their fill policy. The manifest never supplies implicit gap bytes.

## Windvale owner and limits

`Foundation/Immutable-Source-Regions.wv` owns complete admission, canonical
resource naming, exact resource-length checks, bounded extraction of a logical
range, and region-append state across chunk resources. It performs no I/O and
declares no capability; command roots retain explicit acquisition authority.
Thirty-one chunks leave room for control and output resources
inside the current 64-snapshot hosted-resource ceiling.

Changing the limits, record meaning, naming rule, ordering, or zero-region
policy requires a new format version and malformed-input evidence.
