# Decision 0405: Native hosted publication-request construction

- Status: Implemented candidate; ordered process orchestration pending
- Date: 2026-08-08
- Advances: [Decision 0404](0404-Native-Hosted-Container-Segment-Request.md), [Decision 0403](0403-Native-Hosted-Service-Bundle-Request.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native hosted publication-request producer](../../Specifications/Windvale-Native-Hosted-Publication-Request.md)

## Context

The native hosted pipeline can plan publication and can construct each exact
service-bundle request, but retained C# still constructed the 144-byte `WVPQ`
that joined those boundaries. Hiding that construction in an orchestration
script would retain the semantic dependency and make the apparent process
composition misleading.

`WVSG 1` already records the exact fragment and service source extents and
their final image placements. Publication planning needs only those extents;
actual source acquisition remains owned by the later request producer.

## Decision

Add `wvhostpublicationrequest` as a paired Windows/Linux command. Require the
eleven canonical hosted-tool regions, derive the fragment and ten service
sizes, construct the exact `WVPQ 1`, and run the Windvale publication planner
inside the same process. Publish only when the planner's final image and every
placement reproduce the supplied geometry.

Keep the root focused: it imports the existing portable publication core and
immutable-geometry owner, performs no chunk acquisition, and leaves ordered
process lifecycle for the following composition slice.

## Exact local evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Publication-request WVB | 22,067 | `7d525451a92d2f0969e5c9006b43f16cd5485fe7791526e4769a920ec01ad430` |
| Windows publication-request producer | 240,640 | `6d382e6d3a1442fdbf0cf46ff6cc52aabfd1bd6fed86171775d8acc1fdeef0b1` |
| Linux publication-request producer | 241,664 | `7a3c97a9e8abc36accc54e94a7abe968486ac679dd5a34b5f18b86a68ab2dd15` |

After reviewing the test boundary, the final focused current-host check passes
1/1 in 4.123 seconds with a zero-warning Release build. It matches the frozen
C# request oracle byte-for-byte, exercises public target routing and the real
native process without loading the CLR, rejects malformed and inconsistent
geometry while preserving output, rejects an input/output alias, and
reconstructs through the native front door. No broader verifier ran under the
end-of-goal gate policy.

## Consequences

- C# no longer owns `WVPQ` construction in the candidate hosted process path.
- Decision 0406 supplies its `WVSG` directly from real bounded resources.
- The publication planner independently checks the geometry before output;
  `WVSG` still does not become content identity or authority.
- The new root is 134 lines and reuses focused portable owners rather than
  growing an orchestration source.
- Ordered request/response execution, manifest lifecycle, Linux execution,
  promotion, and grouped qualification remain.

## Reconsideration triggers

Version the command if `WVPQ`, canonical service order, publication alignment,
or `WVSG` region meaning changes. Keep actual resource acquisition and identity
checks at the consuming boundaries rather than expanding this size-only seam.
