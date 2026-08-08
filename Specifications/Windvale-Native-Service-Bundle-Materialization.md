# Windvale native service-bundle materialization

## Status and scope

`WVSQ 1` and `WVSI 1` are versioned internal contracts for constructing one
bounded native fragment plus its already verified runtime-service leaves into
the exact executable image selected by the Windvale publication plan. They
transfer fragment/service copying and canonical alignment fill from Stage 0 to
portable Windvale without moving operating-system allocation, pointers,
service tables, W^X authority, invocation, or teardown.

This first materializer is deliberately limited by the ordinary Windvale
4 MiB `bytes` value. Both the complete request and complete response must fit
that limit. Larger compiler-family bundles remain on the explicit Stage 0
large-bundle materializer until a segmented Windvale contract replaces it.
They must not be silently split or truncated by this format.

All integers are unsigned 32-bit little-endian values. Unknown versions,
nonzero reserved fields, truncation, trailing bytes, inconsistent extents, and
malformed embedded publication plans are rejected.

## Request envelope: `WVSQ 1`

The fixed request header is 24 bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVSQ`, encoded as `0x51535657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | Exact complete request length, at most 4 MiB |
| 12 | 4 | plan bytes | Exact embedded `WVPQ 1` length, at least 24 |
| 16 | 4 | payload bytes | Exact remaining payload length |
| 20 | 4 | reserved | Zero |

The header is followed by one complete canonical
[`WVPQ 1`](Windvale-Native-Publication-Plan.md#request-envelope-wvpq-1)
request. Its fragment length, ordered service IDs, and service lengths define
the payload that follows it:

1. exact verified native-fragment code bytes;
2. exact verified service-leaf bytes in `WVPQ` record order.

The payload has no alignment, names, hashes, pointers, or trailing bytes.
Service identity verification and platform selection happen before request
construction; materialization does not authorize a service or reinterpret its
machine code.

## Response envelope: `WVSI 1`

A successful response begins with this 36-byte header:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVSI`, encoded as `0x49535657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | Exact complete response length, at most 4 MiB |
| 12 | 4 | status | Zero (`Valid`) |
| 16 | 4 | failure offset | Exact accepted `WVSQ` length |
| 20 | 4 | plan bytes | Exact embedded request-plan length |
| 24 | 4 | fragment bytes | Exact accepted fragment length |
| 28 | 4 | image bytes | Exact `WVPL` final image extent |
| 32 | 4 | service count | Exact `WVPL` service count |

The header is followed by the exact `WVPL 1` placement records, twelve bytes
per service, and then the complete materialized image. Placement records keep
their existing service ID, image offset, and leaf-length fields. The final
response length is therefore:

```text
36 + service_count * 12 + image_bytes
```

The image preserves these established bytes exactly:

- the verified fragment begins at offset zero unchanged;
- the gap before the first service is zero-filled;
- every later service-alignment gap is filled with x86 NOP byte `0x90`;
- every service leaf is copied unchanged at its `WVPL` offset;
- a service-free fragment is zero-filled through its final aligned extent;
- no alignment follows the final service.

The managed transport independently checks the response envelope, all plan
evidence, every placement, fragment and leaf byte, every zero/NOP gap, and the
complete final extent before accepting the image.

## Failure response

A rejected request returns exactly the 36-byte `WVSI 1` header. Plan,
fragment, image, and service-count fields are zero. `failure_offset` identifies
the first relevant `WVSQ` byte; an embedded publication-plan failure is
translated to `24 + WVPQ failure_offset`.

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Valid` | Complete placement records and image follow |
| 1 | `Invalid_size` | Truncation, declared-size mismatch, or inconsistent plan/payload extent |
| 2 | `Invalid_magic` | Request magic differs |
| 3 | `Invalid_version` | Request version differs |
| 4 | `Invalid_reserved` | Reserved field is nonzero |
| 5 | `Invalid_plan` | Embedded `WVPQ 1` is rejected |
| 6 | `Invalid_payload` | Fragment/service payload does not exactly match the accepted plan |
| 7 | `Value_limit` | The complete response would exceed 4 MiB |

## Windvale owner and bootstrap

`Runtime/Windvale/Native-Service-Bundle-Materialization-Core.wv` owns request
validation, publication-plan consumption, exact image construction, and the
response. Its bridge is capability-free `Main(bytes) -> bytes`.

The retained bridge WVB is 15,253 bytes with SHA-256
`25512a7c3e6eae0dd060426d5a51a93abfc7a7127f59538fd2a315242ed2b660`.
The normal runtime embeds only its 157,174-byte WVNF 1 artifact with SHA-256
`8bb1f06bd8b25d9a5ff78971ad4af36b609c618b080ed0fa9b17fe4b51669629`.
The separately compiled core is 15,245 bytes with SHA-256
`54a0cb83cba3c9c9118cfc209aaef43938f9f9a9f4212ccb9d4657ce6a139ba1`.

The existing narrow bootstrap is generalized from publication-planner naming
to service-free bootstrap naming. It may publish only internally selected,
digest-bound, independently verified fragments with no runtime services; it
does not accept an ambient WVB, bind capabilities, or replace the ordinary
publication planner. Layout and lifetime for the materializer itself remain
the independently checked fixed service-free bootstrap case, avoiding bundle
construction recursion.

Any format, limit, fill, ordering, payload, or bootstrap change requires a new
accepted contract version, regenerated WVB/WVNF identities, malformed-input
coverage, and Windows/Linux qualification.
