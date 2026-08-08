# Windvale native hosted-container construction

## Status and scope

This contract transfers normal hosted compiler-family layout, startup-target
resolution, PE/ELF header construction, Windows imports, and Windows relocation
construction to portable Windvale. It composes three independently
reconstructible native fragments with the existing hosted-startup constructor.
The complete 27 MiB application is never represented as one Windvale `bytes`
value: a deletion-bound managed relay copies already verified large bundle and
runtime inputs into Windvale-declared positions.

The contract covers both x64 targets and all seven hosted profiles. It does not
change their public container formats, startup WVOs, runtime metadata, service
bundle, file bytes, virtual-memory policy, or independent PE/ELF verification.
The same planner is now also available through the standalone
[native hosted-container planner](Windvale-Native-Hosted-Container-Planner.md).

## Planner request: `WVCR 1`

The request is exactly 4,128 bytes. All integers are little-endian `u32`.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVCR`, `0x52435657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `4,128` |
| 12 | 4 | target | `1` Windows or `2` Linux |
| 16 | 4 | hosted profile | `1` through `7` |
| 20 | 4 | runtime-header bytes | `4,096` |
| 24 | 8 | reserved | Zero |
| 32 | 4,096 | runtime header | Exact Windvale-owned initial header |

The planner validates the canonical `WVH* 1` metadata at runtime-header offset
480. That metadata binds target, profile, bundle/native extents, native entry,
and the ten ordered service placements. The managed relay has already verified
the metadata and runtime header against the actual bundle; the planner derives
all addresses from those admitted bytes rather than accepting host-projected
addresses.

## Planner response: `WVCD 1`

The response is a 128-byte header followed by 58 Windows or 31 Linux absolute
startup targets in canonical WVO relocation order.

| Offset | Bytes | Field |
| ---: | ---: | --- |
| 0 | 4 | magic `WVCD`, `0x44435657` |
| 4 | 4 | version `1` |
| 8 | 4 | total response bytes |
| 12 | 4 | status; zero on success |
| 16 | 4 | failure offset; request length on success |
| 20 | 4 | target |
| 24 | 4 | hosted profile |
| 28 | 4 | complete application bytes |
| 32 | 4 | header file offset, zero |
| 36 | 4 | header bytes |
| 40 | 4 | startup file offset |
| 44 | 4 | startup bytes |
| 48 | 4 | bundle file offset |
| 52 | 4 | bundle bytes |
| 56 | 4 | import file offset |
| 60 | 4 | import bytes |
| 64 | 4 | runtime-header file offset |
| 68 | 4 | runtime-header bytes |
| 72 | 4 | relocation file offset |
| 76 | 4 | relocation payload bytes |
| 80 | 4 | text address |
| 84 | 4 | data address |
| 88 | 4 | runtime address |
| 92 | 4 | complete virtual image bytes |
| 96 | 4 | target-table payload offset, `128` |
| 100 | 4 | target-table bytes, `232` Windows or `124` Linux |
| 104 | 4 | relocation address, zero on Linux |
| 108 | 4 | import address, zero on Linux |
| 112 | 4 | text virtual bytes |
| 116 | 4 | text file bytes |
| 120 | 4 | data file bytes |
| 124 | 4 | data virtual bytes |

Failure responses are exactly 128 bytes. Statuses distinguish size, magic,
version, header, metadata, layout, and target-table rejection.

## Platform byte constructors

The Windows constructor accepts the exact 360-byte successful Windows plan and
returns `WVWB 1`: a 32-byte response header followed by the 512-byte PE header,
4,096-byte import page, and 12-byte relocation block. It owns every PE/COFF,
optional-header, data-directory, section-header, import descriptor, lookup/IAT,
name, alignment, stack, and relocation byte.

The Linux constructor accepts the exact 252-byte successful Linux plan and
returns `WVLB 1`: a 32-byte response header followed by the exact 4,096-byte ELF
header page. It owns ELF identification, five program headers, the Windvale
format note, stack declaration, and all padding.

Both fragments reject truncated, oversized, misidentified, wrong-target, and
internally inconsistent plan envelopes before returning owned bytes. The
linker embeds their WVNF artifacts, not their WVB modules.

## Exact retained artifacts

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Hosted-container planner WVB | 33,667 | `55b89d7b5ca5e6118214bfadf7c8597a959348bf18b230954c623a7549e27509` |
| Hosted-container planner WVNF | 537,190 | `6a9ba7b7bf7cc058f3a3a8bde5e694891dc857ee66521a4c7496aa6bc0fd6634` |
| Windows container-byte WVB | 17,409 | `a63a185af4b58226f8afd5416b9a7b96f6d25ca8151cfa42f5d9a3cb70daaee8` |
| Windows container-byte WVNF | 183,406 | `7e04baf6105eab333b2898142f7dd2d4f636f445471620ae67cb5125e1cb2d02` |
| Linux container-byte WVB | 12,086 | `c4cdd9b71c677359de324201206ec215df2c5a92a2622912ba1a425074f29ca2` |
| Linux container-byte WVNF | 124,463 | `3c8cbb6f1b06794fb25e7fda136cee1081b5bc13f15cf6f09f0b6c01a0d5556a` |
| Hosted-container segmentation WVB | 21,832 | `af869ba326f99eaa8d1a2c0898c14145a62c4f046da7bbcccf511d7918e79056` |
| Hosted-container segmentation WVNF | 281,719 | `ab96ecad8d37f9383626d24c2e97c7e6615dd3c92c2ed5f9dc816cf77f3dc7d7` |

All four WVB modules reconstruct byte-for-byte through the native project
front door. Separate fragments are required because the current bootstrap
source-binding closure rejects the aggregate planner, startup-object parser,
and both platform byte constructors in one source compilation. This is a
bounded composition constraint, not a new semantic implementation.

## Bounded final-image segments: `WVHT 1` / `WVHU 1`

The final application is constructed through canonical segments of at most
4,194,144 bytes. This reserves 160 request bytes for the fixed 32-byte `WVHT`
header and successful 128-byte `WVCD` plan header while keeping every request
and response inside the ordinary 4 MiB Windvale byte-value limit.

The request records total bytes, the fixed plan-header length, canonical
segment offset and length, intersecting payload length, and a zero reserved
word. Its payload contains only the header, startup, actual bundle, imports,
runtime header, and relocation byte ranges that intersect that segment, in
file order. Padding is omitted. Portable Windvale rederives and validates the
complete layout, rejects noncanonical segment boundaries, reconstructs every
zero region, and returns `WVHU 1`: a 40-byte status envelope followed by the
exact segment. The response binds the original request length, application
length, segment extent, and six-region directory.

The managed session projects bounded requests, validates every returned source
and fill byte, and concatenates successful segments in order. The standalone
[native hosted-container segmenter](Windvale-Native-Hosted-Container-Segmenter.md)
now accepts the same request and produces the same response without loading
.NET. The [immutable segment-set admission](Windvale-Native-Hosted-Container-Segment-Set.md)
now binds the selected layout header and reconstructs every complete response
before mutation. Managed final publication remains deletion-bound until that
set feeds the native durable publisher directly.

## Normal path and retirement boundary

Normal construction now follows one ordered path:

1. Windvale constructs and managed code independently verifies metadata and
   the initial runtime header against the actual bundle.
2. The planner derives all file/virtual layout and startup targets; the paired
   native planner command can now produce this exact plan as a separate process.
3. The startup constructor applies that target table to the canonical WVO.
4. The selected platform constructor emits every outer-container-owned byte.
5. The bounded segment constructor consumes only intersecting source bytes,
   constructs every complete segment including padding in Windvale, and returns
   exact ordered pieces below the ordinary byte-value limit.
6. The native segmenter can construct each exact piece as a standalone
   Windows/Linux process.
7. The immutable segment-set tool admits every request/response pair without
   joining its payloads; the deletion-bound managed session still performs
   final publication in the ordinary Stage 0 path.
8. The existing independent PE or ELF verifier validates the complete result.

The former C# application builders are named `Buildˉstage0` and are called only
by focused differential evidence. The ordinary managed builders still dispatch
the retained planner and platform fragments while the standalone planner,
segmenter, and publisher are composed into their replacement process pipeline.
The C# segment-set fixture/harness is temporary transition evidence rather than
product logic. Linux-host execution and grouped dual-host qualification remain
deferred to the final retirement gate.
