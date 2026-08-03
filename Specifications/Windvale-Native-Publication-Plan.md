# Windvale native publication plan

## Status and scope

`WVPQ 1` and `WVPL 1` are versioned internal contracts between the Stage 0 native executor and the retained Windvale publication planner. They describe the bounded layout of one independently verified native fragment and its canonical runtime-service leaves before writable memory is allocated and before the image becomes executable.

[Decision 0082](../Documents/Decisions/0082-Windvale-Owned-Native-Publication-Layout.md) first cross-host qualifies both contracts and their live use at exact commit `ba2cf69cd4a97876f5e953b3938d032fc75a8ff7`. [Decision 0087](../Documents/Decisions/0087-Native-Windows-And-Linux-File-Output.md) cross-host qualifies the closed planner domain's extension from 11 to 12 service IDs at exact commit `12e9e2e` without changing either serialized contract version. [Decision 0111](../Documents/Decisions/0111-Bounded-Exact-Compiler-Fragment-Publication.md) expands the bounded fragment extent to 8 MiB. Cross-host-qualified [Decision 0133](../Documents/Decisions/0133-Frame-Owned-Direct-Native-Records.md) advances the current limit to 32 MiB for ABI 21's measured direct-record code while retaining both format versions and the 34 MiB final-image ceiling. Cross-host-qualified [Decision 0150](../Documents/Decisions/0150-Bounded-Native-Dynamic-Value-Lifetimes.md) retains those publication bounds under ABI 22 at exact descendant `2591cd5`. The current portable core is 7,189 bytes with SHA-256 `f2c315c4c52099b8682396358563eef2eb9dceecf1feb84ce5bef5f8465bdeba`; the current retained hosted bridge is 7,105 bytes with SHA-256 `b21e1136fc9087f530391127a1e1400e7248fa1831a51f00d86d467cf5133cb0`.

This contract is not a public application format, a native object format, a code cache, or a general linker input. It does not contain machine bytes, absolute addresses, relocations, operating-system handles, or executable-memory policy. WVB remains the portable program identity and WVO remains the serialized native object format.

All integers are unsigned 32-bit little-endian values. Every reserved field is zero. Unknown versions, trailing bytes, missing bytes, noncanonical service order, and arithmetic outside the stated limits are rejected.

## Request envelope: `WVPQ 1`

The request is exactly `24 + service_count * 12` bytes.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVPQ`, encoded as `0x51505657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | Exact request length |
| 12 | 4 | fragment bytes | `1` through `33,554,432` |
| 16 | 4 | service count | `0` through `12` |
| 20 | 4 | reserved | Zero |

Each service record is exactly 12 bytes:

| Relative offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | service ID | `1` through `12`, matching the closed ABI-16 service table retained from ABI 15 |
| 4 | 4 | leaf bytes | Positive |
| 8 | 4 | reserved | Zero |

Records are strictly increasing by service ID. Duplicate, descending, unknown, or zero IDs are invalid. The request describes only service-leaf sizes because the executor has already reconstructed and independently verified every exact leaf before planning.

## Response envelope: `WVPL 1`

A successful response is exactly `32 + service_count * 12` bytes.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVPL`, encoded as `0x4c505657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | Exact response length |
| 12 | 4 | status | Zero (`Valid`) |
| 16 | 4 | failure offset | Exact accepted request length |
| 20 | 4 | fragment bytes | Exact request value |
| 24 | 4 | image bytes | Final bounded image extent |
| 28 | 4 | service count | Exact request value |

Each successful placement record is exactly 12 bytes:

| Relative offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | service ID | Exact corresponding request ID |
| 4 | 4 | image offset | Canonical 16-byte-aligned placement |
| 8 | 4 | leaf bytes | Exact corresponding request size |

The first cursor is the fragment length rounded upward to a 16-byte boundary. Each service starts at the preceding cursor rounded upward to a 16-byte boundary, and advances the cursor by its exact leaf size. A service-free image ends at the aligned fragment cursor. No trailing alignment follows the last service. The final image extent must not exceed `35,651,584` bytes (34 MiB).

The current executor preserves the already-qualified image-fill policy: fragment bytes are copied unchanged; the gap from the fragment to the first service remains zero; alignment gaps between later service leaves contain x86 NOP byte `0x90`. Fill bytes are executor construction policy and are not repeated in `WVPL`.

## Failure response

A rejected request produces an exact 32-byte `WVPL 1` envelope with zero fragment extent, image extent, and service count. `failure_offset` identifies the first relevant request field or boundary. The status values are:

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Valid` | The complete canonical plan follows |
| 1 | `Invalidˉsize` | Truncation, declared-size mismatch, trailing bytes, or wrong record extent |
| 2 | `Invalidˉmagic` | Request magic differs |
| 3 | `Invalidˉversion` | Request version differs |
| 4 | `Invalidˉreserved` | A reserved field is nonzero |
| 5 | `Invalidˉfragment` | Fragment extent is zero or above 32 MiB |
| 6 | `Invalidˉservice` | Service count or ID is outside the closed table |
| 7 | `Invalidˉorder` | Service IDs are not strictly increasing |
| 8 | `Invalidˉrange` | A service leaf has zero bytes |
| 9 | `Imageˉlimit` | Alignment or a leaf would exceed 34 MiB |

The live host validates its own inputs before serialization. A nonzero planner status becomes `WVN4013`. A successful response is independently reconstructed in C#: envelope, extents, count, every ID, every size, every aligned offset, and the final image extent must agree exactly before allocation; malformed successful output becomes `WVN4014`.

## Windvale owner and hosted seam

`Compiler/Windvale/Native-Publication-Core.wv` is portable and owns validation, checked layout, status selection, and canonical response construction. `Compiler/Windvale/Native-Publication-Bridge.wv` is a narrow hosted wrapper that reads one exact in-memory resource named `native-publication-request.bin` and returns the core response as `bytes`.

The retained bridge is executed by the Stage 0 reference interpreter, not by the image it is planning. Its only authorized capability is `file.read_bytes`, backed by the executor's immutable in-memory request reader. This avoids a publication cycle while making the layout decision executable Windvale code. The retained WVB has an exact size and digest gate and must reproduce from source on every qualifying host.

## Publication boundary

Before planning, the native fragment verifier proves that every relative patch field already contains its exact base-independent displacement. Publication therefore copies verified fragment bytes unchanged; there is no second C# relocation rewrite. After accepting the Windvale plan, the platform adapter alone:

1. allocates writable, non-executable memory;
2. constructs the planned image and service table;
3. copies the image;
4. changes the image to read/execute and flushes the instruction cache where required;
5. invokes the verified entry through the qualified ABI; and
6. releases every execution-owned allocation.

Windows `VirtualAlloc`/`VirtualProtect` and Linux `mmap`/`mprotect` remain narrow platform adapters. [`WVLQ 1` and `WVLT 1`](Windvale-Native-Publication-Lifetime.md) separately make Windvale the authority for the allowed executable-image lifetime transitions and isolate those calls inside one internal host owner; `WVPQ 1` and `WVPL 1` themselves do not claim that transfer.

Any change to either serialized layout, its limits, service ordering, alignment, or response semantics requires a new accepted contract version, regenerated retained WVB identity, hostile-input coverage, and exact Windows/Debian qualification.
