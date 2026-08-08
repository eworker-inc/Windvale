# Decision 0403: Native hosted service-bundle request construction

- Status: Implemented candidate; ordered process orchestration pending
- Date: 2026-08-08
- Advances: [Decision 0400](0400-Standalone-Native-Hosted-Service-Bundle.md), [Decision 0402](0402-Native-Hosted-Metadata-Request.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contracts: [Immutable source geometry](../../Specifications/Windvale-Immutable-Source-Geometry.md) and [native service-bundle request producer](../../Specifications/Windvale-Native-Hosted-Service-Bundle-Request.md)

## Context

Decision 0400 transferred exact `WVSQ 2` consumption and `WVSI 2` production
to a standalone Windvale process. A retained managed session still chose each
source intersection and constructed every request. The existing `WVHS` format
could not honestly describe this input: it binds digest regions at their final
image offsets, while request construction needs a gap-free logical sequence of
raw fragment and service sources mapped into those output offsets.

The repository already owns an unrelated `WVRS` package resource store. A new
format must not reuse that name or silently reinterpret its authority.

## Decision

Add `WVSG 1` as a small capability-free source-geometry contract. It maps up
to 31 bounded resources and 16 semantic source regions into an output image,
keeps gaps under the consumer's fill policy, and explicitly does not claim
content identity or authority.

Add `wvhostbundlerequest` as a paired native Windows/Linux command. It admits
the exact ten-service publication plan, requires eleven canonical fragment and
service regions, validates every declared resource, accepts one canonical
segment index, and emits the exact existing `WVSQ 2` bytes. Produce one request
per invocation so failure cannot publish a partially committed request set.
Keep the loop and temporary-resource lifecycle for the next process-composition
slice.

## Exact local evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Request-producer WVB | 26,615 | `7eb367894051b89acee497c906c3c3282621f9d0d2a7274d79931af0ec7926e2` |
| Windows request producer | 271,360 | `0101389e7fca09905e5aa64902df6b61d07debe4735e091cf57d01af7b217c3b` |
| Linux request producer | 270,336 | `216dc362944945ba3259d6ffb0aeed094eb8ba2d475678641335d892e2c316ec` |

After review, the focused current-host test passes 1/1 in 4.672 seconds. It
crosses one source region over two immutable resources, matches the frozen C#
request oracle byte-for-byte, exercises public target
routing and the real native process without loading the CLR, preserves output
on malformed geometry and an invalid segment, rejects a source/output alias,
and reconstructs the WVB through the native front door. The Release build is
zero-warning. No broader verifier ran under the end-of-goal gate policy.

## Consequences

- Managed code no longer owns service-bundle segment arithmetic or `WVSQ`
  request bytes in the candidate process pipeline.
- `WVSG` can be reused for the six hosted-container source regions without
  conflating geometry, digests, or package resources.
- Stage 0 adds only deletion-bound package identities and the independent
  differential oracle.
- Ordered request/response orchestration, final segment requests, Linux
  execution, promotion, and grouped qualification remain.

## Reconsideration triggers

Version `WVSG` if source/image limits, resource naming, region ordering, or
zero-length-region semantics change. Version the command if `WVPQ`, `WVSQ`,
canonical hosted services, or segmentation changes. Add identity evidence at
the acquiring boundary rather than treating geometry as authorization.
