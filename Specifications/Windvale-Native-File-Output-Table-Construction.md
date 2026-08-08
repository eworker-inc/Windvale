# Windvale native file-output-table construction

## Status and scope

`WVFQ 1` and `WVFR 1` are runtime-private contracts for constructing the exact
80-byte [`WVFO 1`](Windvale-Native-Execution-Context.md#runtime-private-file-output-table)
binding table from an already allocated path scratch range and already
resolved platform function targets. Portable Windvale owns the table format
and validates platform-specific capacities and function presence. The host
retains scratch allocation, Windows export resolution, native table
allocation/copy, independent reread, and teardown.

All integers are little-endian. Unknown versions, nonzero reserved fields,
truncation, trailing bytes, incorrect capacities, and invalid platform-function
combinations are rejected.

## Request envelope: `WVFQ 1`

The request is exactly 80 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVFQ`, `0x51465657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `80` |
| 12 | 4 | platform | `1` Windows or `2` Linux |
| 16 | 8 | scratch pointer | Opaque nonzero execution-owned target |
| 24 | 4 | scratch bytes | Windows `2,097,154`; Linux `1,048,577` |
| 28 | 4 | reserved | Zero |
| 32 | 48 | Windows functions | Six nonzero opaque targets on Windows; all zero on Linux |

The six function ranges retain their `WVFO` order: UTF-8 conversion,
create/replace, write, durable flush, close, and last-error. Windvale validates
only the required zero/nonzero shape and copies each opaque eight-byte range;
portable source does not acquire or dereference a host pointer.

## Response envelope: `WVFR 1`

The response header is 32 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVFR`, `0x52465657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `32` on failure or `112` on success |
| 12 | 4 | status | Status below |
| 16 | 4 | failure offset | First relevant request byte; `80` on success |
| 20 | 4 | table bytes | Zero on failure; `80` on success |
| 24 | 8 | reserved | Zero |

A successful header is followed by the exact `WVFO 1` table. The managed
adapter independently checks every field against its allocated scratch range
and resolved function list before copying and again after native execution.

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Valid` | Exact table follows |
| 1 | `Invalid_size` | Request length or declared length is not 80 |
| 2 | `Invalid_magic` | Request magic differs |
| 3 | `Invalid_version` | Request version differs |
| 4 | `Invalid_reserved` | Reserved field is nonzero |
| 5 | `Invalid_platform` | Platform is not Windows or Linux |
| 6 | `Invalid_scratch` | Scratch pointer or platform capacity is invalid |
| 7 | `Invalid_functions` | A Windows function is absent or a Linux function is present |

## Windvale owner and retained artifact

`Runtime/Windvale/Native-File-Output-Table-Core.wv` owns request validation and
exact `WVFO` construction. Its capability-free bridge exposes
`Main(bytes) -> bytes`.

The core WVB is 3,926 bytes with SHA-256
`fb6fd67339561f517967b326cc4299132699dc6f098a38595bbb3aabbf1fbc7f`.
The retained bridge WVB is 3,930 bytes with SHA-256
`94cc057b655c58be3ccd2db333cff4e7a755482c52983c4031196ab060a89e06`.
The normal runtime embeds only its 42,302-byte WVNF 1 artifact with SHA-256
`9333d4573b87b829e6e577d8a27c937bf2fb433a93d4a4b11b783b372d31d08a`.

Any format, capacity, function ordering, platform identity, artifact, or
bootstrap change requires a new accepted contract version and Windows/Linux
qualification.
