# Windvale native argument-table construction

## Status and scope

`WVAQ 1` and `WVAR 1` are runtime-private contracts for constructing the
contiguous native argument descriptor table from one already validated,
execution-owned argument snapshot. Portable Windvale owns the exact 16-byte
descriptor layout and bounded entry/payload coverage. The host retains strict
UTF-8 validation, immutable payload packing, real-address projection, table
allocation/copy, complete reread, and teardown.

The constructor receives pointers as opaque little-endian integers and never
dereferences them. The payload is included so offsets and lengths must cover
the exact prevalidated bytes, not so the constructor can acquire host memory or
ambient process arguments. No pointer or argument byte enters WVB, WVNF, WVO,
or a native cache.

## Request envelope: `WVAQ 1`

The request is 40 through 66,632 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVAQ`, `0x51415657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | Exact request length |
| 12 | 4 | argument count | 1 through 67 |
| 16 | 4 | payload offset | Exactly `24 + count * 16` |
| 20 | 4 | payload bytes | At most 64 KiB; request ends at `payload offset + payload bytes` |
| 24 | variable | entry directory | `count` exact 16-byte entries |
| payload offset | variable | packed arguments | Exact prevalidated strict-UTF-8 byte concatenation |

Each request-directory entry is:

| Relative offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 8 | descriptor pointer | Nonzero opaque address in the host-owned packed allocation |
| 8 | 4 | byte length | At most 4 KiB |
| 12 | 4 | payload offset | Canonical running offset; all entries cover the payload exactly |

Zero-length arguments are valid. They retain a nonzero pointer and may share
the same running payload offset with the next entry. The host independently
proves that every pointer equals its packed-allocation base plus the recorded
offset and that each payload slice equals the previously validated argument.

## Response envelope: `WVAR 1`

The response header is 32 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVAR`, `0x52415657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `32` on failure or `32 + count * 16` on success |
| 12 | 4 | status | Status below |
| 16 | 4 | failure offset | First relevant request byte; request length on success |
| 20 | 4 | table bytes | Zero on failure; exactly `count * 16` on success |
| 24 | 8 | reserved | Zero |

A successful header is followed by one 16-byte borrowed-text descriptor per
argument. Each descriptor copies the admitted opaque pointer and byte length
and appends a zero reserved word. No envelope header is present in the table
published to native code.

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Valid` | Exact descriptor table follows |
| 1 | `Invalid_size` | Request or declared length is outside the exact envelope |
| 2 | `Invalid_magic` | Request magic differs |
| 3 | `Invalid_version` | Request version differs |
| 4 | `Invalid_count` | Count is zero or greater than 67 |
| 5 | `Invalid_payload` | Payload offset, limit, or final request extent is invalid |
| 6 | `Invalid_pointer` | An entry pointer is zero |
| 7 | `Invalid_entry` | A length, running offset, bound, or final coverage rule fails |

## Windvale owner and retained artifact

`Runtime/Windvale/Native-Argument-Table-Core.wv` owns request validation and
exact descriptor construction. Its capability-free bridge exposes
`Main(bytes) -> bytes`.

The core WVB is 4,362 bytes with SHA-256
`08df8569d091fc0c860988dceff1320d7a8e407b54ce571515af601c10120d75`.
The retained bridge WVB is 4,374 bytes with SHA-256
`080be2dea127948697222c23efe4be828410450b602dee5cf2a63abc11627788`.
The normal runtime embeds only its 44,775-byte WVNF 1 artifact with SHA-256
`4a4cc1d6171126a821c1f96de11c4ffcb78ea83e98d06d5e0802e5921e9062d8`.

Any descriptor representation, count or byte limit, snapshot lifetime,
encoding rule, artifact, or request projection change requires a new accepted
contract version and Windows/Linux qualification.
