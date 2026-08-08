# Windvale native hosted service-bundle request producer

## Status and scope

This contract constructs one exact `WVSQ 2` request from an admitted ten-
service `WVPQ 1` publication request and immutable source resources described
by `WVSG 1`. It moves source-region selection, canonical segment arithmetic,
payload intersection, and request-byte construction out of the managed
service-bundle session.

One invocation produces one segment request. Process-level iteration and
temporary-resource lifecycle remain the following orchestration boundary; the
tool does not publish a partially complete multi-file request set.

## Command contract

```text
wvhostbundlerequest <plan.wvpq> <sources.wvsg> <chunk-prefix> <segment-index> <request.wvsq>
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
| Service-bundle request WVB | 26,615 | `7eb367894051b89acee497c906c3c3282621f9d0d2a7274d79931af0ec7926e2` |
| Windows application | 271,360 | `0101389e7fca09905e5aa64902df6b61d07debe4735e091cf57d01af7b217c3b` |
| Linux application | 270,336 | `216dc362944945ba3259d6ffb0aeed094eb8ba2d475678641335d892e2c316ec` |

The WVB reconstructs byte-for-byte through the native Project 1 front door.
The package writers are deletion-bound Stage 0 target and identity wiring.

## Retirement boundary

The focused current-host evidence compares the produced request byte-for-byte
with the frozen C# recovery oracle, executes the public current-host package
without loading the CLR, rejects malformed geometry and an out-of-range
segment without changing output, and rejects an output/source alias.

The retained managed `Build_request` is now differential/recovery evidence.
Ordered invocation of this producer and the standalone `WVSQ` to `WVSI`
constructor, construction of the resulting `WVHS` evidence manifest, final
hosted-container segment requests, Linux execution, promotion, and the grouped
retirement gate remain.
