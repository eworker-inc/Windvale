# Windvale native service-table construction

## Status and scope

`WVTQ 1` and `WVTR 1` are runtime-private contracts for constructing the exact
104-byte [runtime-service table](Windvale-Native-Execution-Context.md#runtime-service-table)
from already published native service targets. Portable Windvale owns version,
size, slot order, and required/absent presence. The host retains executable
image allocation, service placement, target-address projection, table
allocation/copy, invocation, and teardown.

All integers are little-endian. Unknown versions, truncation, trailing bytes,
an empty or oversized required mask, a zero required target, or a nonzero
absent target are rejected.

## Request envelope: `WVTQ 1`

The request is exactly 112 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVTQ`, `0x51545657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `112` |
| 12 | 4 | required-service mask | Nonzero; only low twelve bits may be set |
| 16 | 96 | service targets | Twelve opaque little-endian targets in canonical service order |

Mask bit zero and the first target correspond to `console.write_line`; bit
eleven and the final target correspond to `file.write_bytes`. Every set bit
requires a nonzero target and every clear bit requires zero. The managed
adapter derives the mask from the fragment's already verified canonical
required-service list and computes each target from the published image base
plus its accepted placement. Windvale copies targets as opaque bytes and does
not dereference native pointers.

## Response envelope: `WVTR 1`

The response header is 32 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVTR`, `0x52545657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `32` on failure or `136` on success |
| 12 | 4 | status | Status below |
| 16 | 4 | failure offset | First relevant request byte; `112` on success |
| 20 | 4 | table bytes | Zero on failure; `104` on success |
| 24 | 8 | reserved | Zero |

A successful header is followed by the exact service-table version 5 bytes.
The adapter independently checks version, size, every target, and the complete
required/absent presence relation before allocating and copying the table.

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Valid` | Exact service table follows |
| 1 | `Invalid_size` | Request length or declared length is not 112 |
| 2 | `Invalid_magic` | Request magic differs |
| 3 | `Invalid_version` | Request version differs |
| 4 | `Invalid_mask` | Mask is empty or uses a bit above the twelve closed services |
| 5 | `Invalid_target` | Target presence differs from its mask bit |

## Windvale owner and retained artifact

`Runtime/Windvale/Native-Service-Table-Core.wv` owns request validation and
exact service-table construction. Its capability-free bridge exposes
`Main(bytes) -> bytes`.

The core WVB is 3,065 bytes with SHA-256
`ca7388bf816e7d23d5a4cd3cb7cff488ba2cb3d96c0c1a0f511ced54b4296c26`.
The retained bridge WVB is 3,079 bytes with SHA-256
`04c87116f12097c6efaeddc471c06ce831f6146c94b4cae0205a635f31bcd50b`.
The normal runtime embeds only its 34,830-byte WVNF 1 artifact with SHA-256
`e1b838652150999d13b84cd6f1c527b4e82923190530f707ef8d163d39a1f58e`.

Any table version, service count/order, presence rule, artifact, or bootstrap
change requires a new accepted contract version and Windows/Linux
qualification.
