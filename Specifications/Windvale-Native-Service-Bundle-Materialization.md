# Windvale native service-bundle materialization

## Status and scope

`WVSQ 2` and `WVSI 2` are versioned internal contracts for constructing one
verified native fragment plus its already verified runtime-service leaves in
bounded segments of the exact executable image selected by the Windvale
publication plan. They transfer fragment/service copying and canonical
alignment fill from Stage 0 to portable Windvale without widening the ordinary
4 MiB `bytes` value.

The segmented session covers every accepted publication image through the
existing 34 MiB limit. It replaces the bounded version-1 whole-image contract;
version 1 remains historical evidence and is not accepted by the current
bridge. Operating-system allocation, pointers, service tables, W^X authority,
invocation, and teardown remain outside this contract.

All integers are unsigned 32-bit little-endian values. Unknown versions,
nonzero reserved fields, truncation, trailing bytes, inconsistent extents, and
malformed embedded publication plans are rejected.

## Canonical segmentation

One segment contains at most `4,194,104` image bytes. That value reserves the
32-byte `WVSQ 2` header and the maximum 168-byte `WVPQ 1` request inside one
4 MiB request even when the complete segment is source data.

Segments are canonical and independently constructible:

- the first starts at image offset zero;
- every later start is an exact multiple of `4,194,104`;
- every nonfinal segment has exactly `4,194,104` image bytes;
- the final segment has the exact remaining positive extent; and
- ordered segments cover the `WVPL` image exactly once without a gap or overlap.

## Request envelope: `WVSQ 2`

The fixed request header is 32 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVSQ`, encoded as `0x51535657` |
| 4 | 4 | version | `2` |
| 8 | 4 | total bytes | Exact complete request length, at most 4 MiB |
| 12 | 4 | plan bytes | Exact embedded `WVPQ 1` length, 24 through 168 |
| 16 | 4 | segment offset | Canonical image-segment start |
| 20 | 4 | segment bytes | Canonical positive image-segment extent |
| 24 | 4 | payload bytes | Exact remaining source payload length |
| 28 | 4 | reserved | Zero |

The header is followed by one complete canonical
[`WVPQ 1`](Windvale-Native-Publication-Plan.md#request-envelope-wvpq-1)
request. The remaining payload contains only source bytes that intersect the
requested image segment, in this order:

1. the intersecting verified native-fragment bytes, if any;
2. each intersecting verified service-leaf range in `WVPQ` order.

Alignment fill is omitted from the payload. The Windvale constructor derives
all source intersections and fill regions from the independently validated
publication plan. Service identity and platform selection occur before request
construction; materialization does not authorize a service or reinterpret its
machine code.

## Response envelope: `WVSI 2`

A successful response begins with this 40-byte header:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVSI`, encoded as `0x49535657` |
| 4 | 4 | version | `2` |
| 8 | 4 | total bytes | Exact complete response length, at most 4 MiB |
| 12 | 4 | status | Zero (`Valid`) |
| 16 | 4 | failure offset | Exact accepted `WVSQ` length |
| 20 | 4 | plan bytes | Exact embedded request-plan length |
| 24 | 4 | image bytes | Exact `WVPL` final image extent |
| 28 | 4 | segment offset | Exact accepted segment start |
| 32 | 4 | segment bytes | Exact accepted segment extent |
| 36 | 4 | service count | Exact `WVPL` service count |

The header is followed by exactly `segment bytes` constructed image bytes. The
complete response length is therefore `40 + segment_bytes`.

Across the ordered session, the image preserves these established bytes:

- the verified fragment begins at offset zero unchanged;
- the gap before the first service is zero-filled;
- every later service-alignment gap is filled with x86 NOP byte `0x90`;
- every service leaf is copied unchanged at its `WVPL` offset;
- a service-free fragment is zero-filled through its final aligned extent; and
- no alignment follows the final service.

The managed session accepts only canonical offsets in ascending order,
concatenates the returned segments, and independently checks each source and
fill region plus the complete final extent. It does not write an image source
byte or choose an alignment-fill byte.

## Failure response

A rejected request returns exactly the 40-byte `WVSI 2` header. Plan, image,
segment, and service-count fields are zero. `failure_offset` identifies the
first relevant `WVSQ` byte; an embedded publication-plan failure is translated
to `32 + WVPQ failure_offset`.

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Valid` | The exact constructed segment follows |
| 1 | `Invalid_size` | Truncation, declared-size mismatch, or inconsistent payload extent |
| 2 | `Invalid_magic` | Request magic differs |
| 3 | `Invalid_version` | Request version differs |
| 4 | `Invalid_reserved` | Reserved field is nonzero |
| 5 | `Invalid_plan` | Embedded `WVPQ 1` is rejected |
| 6 | `Invalid_segment` | Segment start or extent is not canonical for the image |
| 7 | `Invalid_payload` | Source payload does not exactly cover the segment intersections |
| 8 | `Value_limit` | Segment construction cannot remain within the bounded value contract |

## Windvale owner and bootstrap

`Runtime/Windvale/Native-Service-Bundle-Materialization-Core.wv` owns request
validation, publication-plan consumption, source/fill intersection, exact
segment construction, and the response. Its bridge is capability-free
`Main(bytes) -> bytes`.

The retained bridge WVB is 17,150 bytes with SHA-256
`327b753062d46755b934cfe6e6bc16550ec711c8b7d2aff46eac4bf0d8d9d902`.
The normal runtime embeds only its 179,452-byte WVNF 1 artifact with SHA-256
`d0b12e426e891f6ee78209ab817dde7c547c0f68541750d39dd665607434e7a9`.
The separately compiled core is 17,185 bytes with SHA-256
`97063c0c3d264d9b9ede73cc316c68798c66d61732c5b115f71a33e486ee7008`.

The narrow service-free bootstrap may publish only internally selected,
digest-bound, independently verified fragments with no runtime services. It
does not accept an ambient WVB, bind capabilities, or replace the ordinary
publication planner. Layout and lifetime for the materializer itself remain
the independently checked fixed service-free bootstrap case, avoiding bundle
construction recursion.

Any format, limit, fill, ordering, payload, or bootstrap change requires a new
accepted contract version, regenerated WVB/WVNF identities, malformed-input
coverage, and Windows/Linux qualification.
