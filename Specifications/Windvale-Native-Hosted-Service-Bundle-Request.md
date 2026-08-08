# Windvale native hosted service-bundle request producer

## Status and scope

This contract constructs one exact `WVSQ 2` request from an admitted ten-
service `WVPQ 1` publication request and immutable source resources described
by `WVSG 1`. It moves source-region selection, canonical segment arithmetic,
payload intersection, and request-byte construction out of the managed
service-bundle session.

One invocation produces one segment request. A read-only count mode admits the
same plan and resources and reports the exact bounded iteration count without
requiring the host adapter to decode `WVPQ`. Temporary-resource lifecycle
remains the following orchestration boundary; the tool does not publish a
partially complete multi-file request set.

## Command contract

```text
wvhostbundlerequest <plan.wvpq> <sources.wvsg> <chunk-prefix> <segment-index> <request.wvsq>
wvhostbundlerequest <plan.wvpq> <sources.wvsg> <chunk-prefix> count
```

The segment index is a decimal `u32` and must identify a canonical segment of
the planned image. Each segment contains at most 4,194,104 image bytes, as
defined by the existing service-bundle materialization contract. The command
derives every source intersection and emits the exact 32-byte request header,
complete publication request, and ordered source payload.

The publication request must select exactly the canonical hosted-tool service
order `1` through `8`, then `11` and `12`. The `WVSG` image extent, eleven
source regions, fragment extent, service placements, source lengths, and
logical coverage must agree with the successful publication plan. Every
declared chunk resource is length-validated before output, including resources
that do not intersect the selected segment.

Count mode performs the same plan, geometry, and resource admission, derives
the ceiling division by 4,194,104 in Windvale, writes no file, and reports
`segments=N`. This text is process control only and does not introduce another
serialized Windvale format.

Control, derived chunk, and output names must not alias textually. Rejection
returns status 2, reports one diagnostic line, and leaves an existing output
unchanged. Wrong argument count returns 64. Success returns zero and reports
the segment index and exact request byte count.

The application declares exactly `console.write_line`,
`diagnostic.write_line`, `file.read_bytes`, `file.write_bytes`,
`process.argument`, and `process.argument_count`.

## Targets and exact identities

- `windows-x64-hosted-service-bundle-request-v1`, producing `.exe`;
- `linux-x64-hosted-service-bundle-request-v1`, producing `.elf`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Service-bundle request WVB | 29,070 | `f79852fc85b87b4484596b7aa6a41efac2365edeb3f933b32fe12797f19e43e2` |
| Windows application | 302,080 | `b3c7db2f5721beee13473462ce49313c41e2e6f08f98a37ce0fee6139c1810bc` |
| Linux application | 303,104 | `e7e90cfc824bcd345f28edbd432d4a3826fa6a21ba7a7818904de4fc90c51371` |

The WVB reconstructs byte-for-byte through the native Project 1 front door.
The package writers are deletion-bound Stage 0 target and identity wiring.

## Retirement boundary

The focused current-host evidence compares the produced request byte-for-byte
with the frozen C# recovery oracle, executes the public current-host package
without loading the CLR, rejects malformed geometry and an out-of-range
segment without changing output, and rejects an output/source alias.
The same native process now supplies the admitted iteration count used by the
following host adapter.

The retained managed `Build_request` is now differential/recovery evidence.
Decision 0405 supplies the preceding `WVPQ` from the same admitted source
geometry. Ordered invocation of this producer and the standalone `WVSQ` to
`WVSI` constructor, private cleanup, Linux execution, promotion, and the
grouped retirement gate remain.
