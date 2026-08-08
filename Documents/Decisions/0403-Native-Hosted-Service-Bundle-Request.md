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
| Request-producer WVB | 27,843 | `2cd2311b9053abbe92f64d533d0681b6a5438c89a0548cad5ddc5a114c1b1917` |
| Windows request producer | 294,912 | `e7fe0939f62ce2403e3e24d1f4523dbb2e63c8fe469ee6930a039b1b66cc8576` |
| Linux request producer | 294,912 | `256304761afaa42da2df66a2f0e89303a4a00a282b95a235148a2633959d8e2c` |

After Decision 0404's shared-state extraction, the focused current-host test
passes 1/1 in 4.676 seconds. It
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
- Decision 0404 now performs that reuse and emits exact `WVHT` requests.
- Decision 0405 now supplies the exact upstream `WVPQ` from admitted `WVSG`
  geometry without managed request construction.
- Stage 0 adds only deletion-bound package identities and the independent
  differential oracle.
- Ordered request/response orchestration, final segment requests, Linux
  execution, promotion, and grouped qualification remain.

## Reconsideration triggers

Version `WVSG` if source/image limits, resource naming, region ordering, or
zero-length-region semantics change. Version the command if `WVPQ`, `WVSQ`,
canonical hosted services, or segmentation changes. Add identity evidence at
the acquiring boundary rather than treating geometry as authorization.
