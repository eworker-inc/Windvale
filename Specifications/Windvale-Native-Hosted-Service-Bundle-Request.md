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
| Service-bundle request WVB | 27,843 | `2cd2311b9053abbe92f64d533d0681b6a5438c89a0548cad5ddc5a114c1b1917` |
| Windows application | 294,912 | `e7fe0939f62ce2403e3e24d1f4523dbb2e63c8fe469ee6930a039b1b66cc8576` |
| Linux application | 294,912 | `256304761afaa42da2df66a2f0e89303a4a00a282b95a235148a2633959d8e2c` |

The WVB reconstructs byte-for-byte through the native Project 1 front door.
The package writers are deletion-bound Stage 0 target and identity wiring.

## Retirement boundary

The focused current-host evidence compares the produced request byte-for-byte
with the frozen C# recovery oracle, executes the public current-host package
without loading the CLR, rejects malformed geometry and an out-of-range
segment without changing output, and rejects an output/source alias.

The retained managed `Build_request` is now differential/recovery evidence.
Decision 0405 supplies the preceding `WVPQ` from the same admitted source
geometry. Ordered invocation of this producer and the standalone `WVSQ` to
`WVSI` constructor, construction of the resulting `WVHS` evidence manifest,
final hosted-container segment requests, Linux execution, promotion, and the
grouped retirement gate remain.
