# Windvale native hosted-tool runtime header

## Status and scope

`WVHR 1` and `WVHS 1` are runtime-private contracts for constructing the
initial 4,096-byte runtime header embedded in hosted Windows and Linux tools.
Portable Windvale owns admission of the fixed `WVH* 1` metadata shape and the
exact execution-context, service-table, output-table, file-table, metadata,
and zero-tail byte layout. The host still projects already verified metadata,
executes the retained constructor, independently verifies the response, and
owns the outer PE/ELF container.

This version accepts the six implemented hosted profiles: compiler,
build-driver, WVA assembler, WV linker, console packager, and WVB-to-WVO.

## Request envelope: `WVHR 1`

The request is exactly 1,048 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVHR`, `0x52485657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `1,048` |
| 12 | 4 | target | `1` Windows x64 or `2` Linux x64 |
| 16 | 4 | hosted profile | `1` through `6` |
| 20 | 4 | reserved | Zero |
| 24 | 1,024 | hosted metadata | Exact canonical `WVH* 1` record |

Metadata admission requires the profile-specific magic, container version and
flags; ABI 22, execution-context 7 and service-table 5; exact six-capability
and ten-service directories; 4,096-byte bundle placement; arena and
48-billion-instruction bounds; target-correct adapters; ordered, contained
nonempty service leaves; nonzero digests; and a zero reserved tail.

## Response envelope: `WVHS 1`

The response starts with a 32-byte header:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVHS`, `0x53485657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `32` on failure or `4,128` on success |
| 12 | 4 | status | Status below |
| 16 | 4 | failure offset | Relevant request byte; `1,048` on success |
| 20 | 4 | runtime-header bytes | Zero on failure; `4,096` on success |
| 24 | 8 | reserved | Zero |

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Valid` | Exact runtime header follows |
| 1 | `Invalid_size` | Actual or declared request size is invalid |
| 2 | `Invalid_magic` | Request magic differs |
| 3 | `Invalid_version` | Request version differs |
| 4 | `Invalid_request` | Target, profile, or reserved field is invalid |
| 5 | `Invalid_metadata` | The projected hosted metadata is noncanonical |

## Constructed runtime header

The successful payload is exactly 4,096 bytes:

| Offset | Bytes | Content |
| ---: | ---: | --- |
| 0 | 112 | Initial native execution context 7 |
| 112 | 104 | Initial native service table 5 |
| 216 | 48 | Target-specific initial output table 1 |
| 264 | 136 | Target-specific initial file-input table 1 |
| 400 | 80 | Target-specific initial file-output table 1 |
| 480 | 1,024 | Exact admitted hosted metadata |
| 1,504 | 2,592 | Zero reserved tail |

Every live pointer is initially zero. Linux output targets are file
descriptors 1 and 2; Windows startup binds its output writer later. File path
scratch is 2,097,154 bytes on Windows and 1,048,577 bytes on Linux.

## Windvale owner and retained artifact

Metadata admission and header construction remain separate focused source
modules so either contract can evolve without creating one oversized owner.
The capability-free bridge exposes `Main(bytes) -> bytes`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Metadata-admission WVB | 10,550 | `e43c712431e386eba159cd17f87b279cc4a4b5b99084d3a738a3718633099c78` |
| Runtime-header core WVB | 18,911 | `700efbbad9619b58d06561be3e805e18b5498f1e13881646e6e121c2b8ab7564` |
| Retained bridge WVB | 18,864 | `0bbf1c0e5c67c14b3e90bef5243d9c5aea64b3343ad11cfd3f7f93067648fe3d` |
| Retained bridge WVNF | 190,709 | `31e7b98c738972b4f9b23075d48bb1724aac229e5f77d8e517877b5b5733dfe4` |

The normal managed packaging seam embeds only the digest-bound WVNF. The
former C# byte writer is retained under an explicit Stage 0 oracle name for
differential and recovery evidence; it is not called by normal packaging and
is removed from the product tree after the final recovery archive. The
temporary managed request/response bridge also retires when native hosted-tool
container construction invokes this Windvale contract directly.
