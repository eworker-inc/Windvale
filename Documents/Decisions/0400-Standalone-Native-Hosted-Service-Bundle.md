# Decision 0400: Standalone native hosted service bundle

- Status: Implemented candidate; resource acquisition and request orchestration pending
- Date: 2026-08-08
- Advances: [Decision 0399](0399-Standalone-Native-Hosted-Container-Metadata.md), [Decision 0373](0373-Windvale-Owned-Segmented-Service-Bundle-Materialization.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native hosted service-bundle producer](../../Specifications/Windvale-Native-Hosted-Service-Bundle.md)

## Context

Windvale already owned exact segmented service-bundle materialization through
`WVSQ 2` and `WVSI 2`. Normal runtime construction still invoked a retained
fragment from C# and kept every response inside a managed session. Decision
0399's next metadata-request boundary needs immutable bundle evidence rather
than another in-memory managed projection.

The exact request already separates resource selection/acquisition from bounded
byte construction. Exposing the constructor as a process must retain that trust
boundary rather than hiding resource choice inside a broad new tool.

## Decision

Add `Native-Hosted-Service-Bundle-Tool.wv` as a focused hosted shell over the
existing publication and segmented materialization cores. It reads one exact
`WVSQ 2` request, admits only a complete successful `WVSI 2` response, and
writes the response envelope plus immutable segment payload unchanged.

Expose exact Windows/Linux targets through `windvale compile` and
`windvale aot`. Reuse the shared deletion-bound package builder and existing
compiler-authority host envelope. Keep fragment/service acquisition, request
construction, and ordered multi-segment composition as the next explicit
native-resource boundary.

## Exact local evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Service-bundle producer WVB | 20,144 | `2284d3896b013bd81ad75ff9de658a07fa4ae0f7ad6d7522e4cdf2abf36917ec` |
| Windows service-bundle producer | 220,672 | `2f2d012829fda83a2a6109b95c0f59f96605a5a33434f50ae8d6ba0bea2a0e86` |
| Linux service-bundle producer | 221,184 | `e474714f8b33dd8fbc8a4ace8a2440ac9fdbcbda3cc23d736101c80cf3da8878` |

The reviewed process test passes 1/1 in 4.128 test seconds after an 8.37-second
zero-warning incremental build. It pins both packages, exercises the public CLI
target, materializes and independently admits a canonical hosted-tool fixture,
observes no CLR load, preserves an existing output after request corruption,
rejects an alias, and rebuilds the WVB through the native front door. No broader
verifier was run.

## Consequences

- One canonical service-bundle request can now produce immutable native segment
  evidence without a managed runtime process.
- Materialization policy remains in the existing focused portable modules; the
  hosted shell is 108 lines.
- Decision 0401 supplies bounded native SHA-256 evidence across ordered
  multi-resource regions, and Decision 0402 recomputes the actual planned
  leaves while constructing the exact metadata request. Ordered resource and
  service-bundle request orchestration, complete composition, Linux execution,
  promotion, and the grouped gate remain.

## Reconsideration triggers

Version the command if `WVSQ`, `WVSI`, `WVPQ`, segmentation, alignment fill,
service ordering, or the byte-value limit changes. Do not treat request-carried
source bytes as independently verified resource identity; acquisition must bind
the immutable inputs before request construction.
