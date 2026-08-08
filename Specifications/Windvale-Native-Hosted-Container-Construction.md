# Windvale native hosted-container construction

## Status and scope

This contract transfers normal hosted compiler-family layout, startup-target
resolution, PE/ELF header construction, Windows imports, and Windows relocation
construction to portable Windvale. It composes three independently
reconstructible native fragments with the existing hosted-startup constructor.
The complete 27 MiB application is never represented as one Windvale `bytes`
value: a deletion-bound managed relay copies already verified large bundle and
runtime inputs into Windvale-declared positions.

The contract covers both x64 targets and all six hosted profiles. It does not
change their public container formats, startup WVOs, runtime metadata, service
bundle, file bytes, virtual-memory policy, or independent PE/ELF verification.

## Planner request: `WVCR 1`

The request is exactly 4,128 bytes. All integers are little-endian `u32`.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVCR`, `0x52435657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `4,128` |
| 12 | 4 | target | `1` Windows or `2` Linux |
| 16 | 4 | hosted profile | `1` through `6` |
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
| Hosted-container planner WVB | 33,591 | `c62b671c06212fb7450bd4d1335284988bd825402713565f94e45f5592330483` |
| Hosted-container planner WVNF | 536,691 | `58f4e2553ee423c2fcf492f69dcb494f7bd618c47a0ecd54939e04c75e87279b` |
| Windows container-byte WVB | 17,409 | `f3e47f2d447ac968c2b56df2fc4656ceaffbf7f3a2c84cd16c7347e29fb3b70b` |
| Windows container-byte WVNF | 183,406 | `49240c2aacfe806ab786fccdc5ef248775e09dae88fb5c8cb6ee703a9bde7e7c` |
| Linux container-byte WVB | 12,086 | `a502fe7e6d5aaa29bf7b629e96b14bb26346c3296256b89d2aacd069a9eede5e` |
| Linux container-byte WVNF | 124,463 | `0c57b13158570163b113ba8f0faf608a804725316435ecc5485aa172666bec40` |
| Hosted-container segmentation WVB | 21,806 | `c1c446d22e578eac330a0bead108d4d759b7c346c48c335601df62e19538bca4` |
| Hosted-container segmentation WVNF | 278,243 | `f80570a216cbf99e04b83f8e5c8f576f0f8f9d179fdc907715b7f80a57e43c3a` |

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
and fill byte, and concatenates successful segments in order. It does not
allocate the complete application in advance or place any source/fill region
itself. Ordered managed concatenation remains deletion-bound transport until
the same segment contract feeds the native durable publisher directly.

## Normal path and retirement boundary

Normal construction now follows one ordered path:

1. Windvale constructs and managed code independently verifies metadata and
   the initial runtime header against the actual bundle.
2. The planner derives all file/virtual layout and startup targets.
3. The startup constructor applies that target table to the canonical WVO.
4. The selected platform constructor emits every outer-container-owned byte.
5. The bounded segment constructor consumes only intersecting source bytes,
   constructs every complete segment including padding in Windvale, and returns
   exact ordered pieces below the ordinary byte-value limit.
6. The deletion-bound managed session validates and concatenates those pieces;
   it no longer allocates and populates a complete image from source regions.
7. The existing independent PE or ELF verifier validates the complete result.

The former C# application builders are named `Buildˉstage0` and are called only
by focused differential evidence. Managed native dispatch, ordered segment
concatenation, and final publication remain deletion-bound work. Linux-host
execution and grouped dual-host qualification remain deferred to the final
retirement gate.
