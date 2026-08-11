# Windvale native hosted-tool metadata construction

## Status and scope

`WVHM 1` and `WVHD 1` are runtime-private contracts for constructing the
canonical 1,024-byte `WVH* 1` metadata record shared by hosted Windows and
Linux compiler-family tools. Portable Windvale owns the profile directory,
capability directory, service identities, service-to-capability mapping,
service-table slots, target adapters, flags, fixed limits, layout, and zero
reserved bytes.

The host projects already verified bundle extents and raw SHA-256 evidence.
It independently verifies the returned metadata against the actual service
bundle before outer-container construction. This version accepts the compiler,
build-driver, WVA assembler, WV linker, console packager, WVB-to-WVO, and
hosted-container segmenter profiles.

## Request envelope: `WVHM 1`

The request is exactly 576 bytes: one 96-byte header followed by ten 48-byte
service-placement records.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVHM`, `0x4D485657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `576` |
| 12 | 4 | target | `1` Windows x64 or `2` Linux x64 |
| 16 | 4 | hosted profile | `1` through `7` |
| 20 | 4 | bundle offset | Exactly `4,096` |
| 24 | 4 | bundle bytes | `1` through 34 MiB |
| 28 | 4 | native-image bytes | `1` through 32 MiB and no larger than the bundle |
| 32 | 4 | native entry | Within the native image |
| 36 | 4 | service count | Exactly `10` |
| 40 | 32 | native-image SHA-256 | Raw nonzero digest of the native image extent |
| 72 | 24 | reserved | Zero |

Each placement record has this shape:

| Relative offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | image offset | At or after the preceding native/service extent and within the bundle |
| 4 | 4 | code bytes | Nonzero and contained in the bundle |
| 8 | 32 | service SHA-256 | Raw nonzero digest of the service extent |
| 40 | 8 | reserved | Zero |

The request deliberately omits service identities, capability identities,
service-table slots, adapters, and flags. Those values are canonical policy
owned by the Windvale constructor, not choices supplied by a host bridge.

## Response envelope: `WVHD 1`

The response starts with a 32-byte header:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVHD`, `0x44485657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `32` on failure or `1,056` on success |
| 12 | 4 | status | Status below |
| 16 | 4 | failure offset | Relevant request byte; `576` on success |
| 20 | 4 | metadata bytes | Zero on failure; `1,024` on success |
| 24 | 8 | reserved | Zero |

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Valid` | Exact metadata follows |
| 1 | `Invalid_size` | Actual or declared request size is invalid |
| 2 | `Invalid_magic` | Request magic differs |
| 3 | `Invalid_version` | Request version differs |
| 4 | `Invalid_request` | Header policy, extent, digest, count, or reserved bytes are invalid |
| 5 | `Invalid_placement` | A service placement, digest, or reserved field is invalid |
| 6 | `Invalid_result` | Constructed metadata fails the independent Windvale admission contract |

## Constructed metadata

The successful payload is the exact `WVH* 1` metadata shape admitted by
[the hosted-tool runtime-header contract](Windvale-Native-Hosted-Tool-Runtime-Header.md).
The constructor derives the profile magic, container version, profile flags,
six canonical capability records, ten canonical service records, target
adapters, table slots, fixed ABI and profile-owned arena values, and all reserved zeros. It
copies only the admitted placement extents and raw digests from the request.
Profile 2 (the compiler build driver) receives a 234,881,024-byte dynamic
text/byte arena. Profiles 1 and 3 through 7 retain 134,217,728 bytes.

## Windvale owner and retained artifact

Metadata construction and metadata admission are separate focused modules.
The constructor calls the admission module before returning a successful
payload, while the host performs a second verification against the actual
bundle. The capability-free bridge exposes `Main(bytes) -> bytes`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Metadata-construction core WVB | 24,360 | `5808f778eb21c1214b581f0ce03958a74173a801b886aec7ed32124d7446abcd` |
| Retained bridge WVB | 24,252 | `b5e9397326d3106b22ce735369ef8202ff6bb4c8e14f6069a0c467b4266c8208` |
| Retained bridge WVNF | 216,203 | `3bcb475b7be2760ad514d656d6ad5bffaaca7f74dce0439eff1e277ac7b2d5cb` |

The normal managed packaging seam still embeds only the digest-bound WVNF. The
former C# constructor is retained under the explicit `Buildˉstage0` name for
differential and recovery evidence and is not called by normal packaging. The
standalone
[metadata constructor](Windvale-Native-Hosted-Container-Metadata.md) now owns
request dispatch, response admission, and raw metadata output. The
[native metadata-request producer](Windvale-Native-Hosted-Metadata-Request.md)
now combines the publication plan with actual immutable chunk bytes, recomputes
all eleven manifest-bound SHA-256 leaves, and constructs the exact `WVHM`
request without managed projection. The Stage 0 oracle leaves the product tree
only after the final recovery archive.

[Decision 0515](../Documents/Decisions/0515-Native-Hosted-Construction-Build-And-Inspection-Transfer.md)
makes the paired native Project 1 helper the ordinary build and inspection
owner for metadata admission plus the construction core/bridge. The broad
scripts retain exact bridge-WVB and WVNF comparisons; this transfer does not
remove the independent Stage 0 behavioral oracle or recovery archive.
