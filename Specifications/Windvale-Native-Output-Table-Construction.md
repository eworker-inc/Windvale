# Windvale native output-table construction

## Status and scope

`WVIQ 1` and `WVIR 1` are runtime-private contracts for constructing the exact
48-byte [`WVIO 1`](Windvale-Native-Execution-Context.md#runtime-private-output-table)
binding table from already acquired host targets. Portable Windvale owns the
table format and validates platform, presence flags, opaque targets, and the
Windows writer boundary. The host remains responsible for acquiring and
pinning handles, resolving `WriteFile`, allocating native memory, copying the
accepted table, rereading it, and releasing every resource.

All integers are little-endian. Unknown versions, nonzero reserved fields,
truncation, trailing bytes, inconsistent flags/targets, and invalid
platform-writer combinations are rejected.

## Request envelope: `WVIQ 1`

The request is exactly 48 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVIQ`, `0x51495657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `48` |
| 12 | 4 | platform | `1` Windows or `2` Linux |
| 16 | 4 | flags | `1` console, `2` diagnostic, or `3` both |
| 20 | 4 | reserved | Zero |
| 24 | 8 | console target | Opaque nonzero target iff console flag is present |
| 32 | 8 | diagnostic target | Opaque nonzero target iff diagnostic flag is present |
| 40 | 8 | Windows writer | Nonzero on Windows; zero on Linux |

Linux targets are zero-extended nonnegative signed-32-bit file descriptors.
Windows targets and the writer are opaque 64-bit host values. No pointer enters
portable source as an integer; Windvale copies their exact eight-byte request
ranges after validating the required zero/nonzero shape.

## Response envelope: `WVIR 1`

The response header is 32 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVIR`, `0x52495657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `32` on failure or `80` on success |
| 12 | 4 | status | Status below |
| 16 | 4 | failure offset | First relevant request byte; `48` on success |
| 20 | 4 | table bytes | Zero on failure; `48` on success |
| 24 | 8 | reserved | Zero |

A successful header is followed by the exact `WVIO 1` table. The managed
adapter independently checks every header and table field against its pinned
host inputs before publication.

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Valid` | Exact table follows |
| 1 | `Invalid_size` | Request length or declared length is not 48 |
| 2 | `Invalid_magic` | Request magic differs |
| 3 | `Invalid_version` | Request version differs |
| 4 | `Invalid_reserved` | Reserved field is nonzero |
| 5 | `Invalid_platform` | Platform is not Windows or Linux |
| 6 | `Invalid_flags` | Presence flags are zero or contain unknown bits |
| 7 | `Invalid_target` | A target disagrees with its flag or Linux range |
| 8 | `Invalid_writer` | Windows lacks a writer or Linux supplies one |

## Windvale owner and retained artifact

`Runtime/Windvale/Native-Output-Table-Core.wv` owns request validation and
exact `WVIO` construction. Its capability-free bridge exposes
`Main(bytes) -> bytes`.

The core WVB is 4,710 bytes with SHA-256
`ab51993aea2370d84b8fe116634e3da71882756bfa87822f1bce180bb01b04a8`.
The retained bridge WVB is 4,714 bytes with SHA-256
`b5b20dc0213e55790e4f39e8a512a17e2a0304b0202d488a9342905ee35e80a8`.
The normal runtime embeds only its 50,493-byte WVNF 1 artifact with SHA-256
`f444e80b2afbaaee251892ab7a7a6a879b3e5cffcbf029b0fc382b64bef97afb`.

Any format, target rule, platform identity, artifact, or bootstrap change
requires a new accepted contract version and Windows/Linux qualification.
