# Decision 0412: Native hosted-container segment manifest

- Status: Implemented candidate; ordered host-process composition pending
- Date: 2026-08-08
- Advances: [Decision 0411](0411-Native-Hosted-Container-Source-Set.md), [Decision 0388](0388-Immutable-Hosted-Container-Segment-Set.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native hosted-container segment manifest](../../Specifications/Windvale-Native-Hosted-Container-Segment-Manifest.md)
- Advanced by: [Decision 0413](0413-Native-Hosted-Segment-Iteration-Control.md)

## Context and decision

Decision 0411 completed the immutable six-region source set consumed by final
segment-request production. The resulting `WVHT` requests and `WVHU` responses
still required managed code to count segments and serialize the `WVHM 1`
manifest consumed by the existing native admission/publisher.

Add one paired native command that admits the complete plan, derives every
canonical resource name, binds exact request/response envelopes and lengths,
constructs the manifest, and runs the shared segment-set admission core before
publication. Keep full response-payload reconstruction in the existing
downstream admission process; it remains the independent content gate and
avoids duplicating segmentation semantics in the producer.

The native composer currently rejects an otherwise valid extracted constructor
result across the additional module binding. Retain the small 32-byte header
constructor in the focused hosted root that already imports segment-set
admission. This is a documented bootstrap closure constraint, not permission
to duplicate the manifest or segment algorithm.

## Evidence and consequences

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Segment-manifest WVB | 34,853 | `28299931809e61bb80848e28e0621b670df2d13330f284dc77dac843b0138049` |
| Windows application | 406,016 | `ff8028aebdaeda1c305225f2d6c3883d22af3ab5bd440e71b50837e4400c334f` |
| Linux application | 405,504 | `48a60917b0693457441c15c121bf42e489067b8e69356f355dba5ec184ad533e` |

The native front door and Stage 0 recovery compiler produce identical WVB
bytes. After test review, the focused Release build passes with zero warnings
and the single selected test passes 1/1 in 6.182 seconds. It compares the
output with the independent retained manifest oracle, verifies public target
and package identities, executes without CLR loading, rejects a changed
response while preserving output, and reconstructs through the native front
door. No broad local verifier ran.

All currently identified hosted-container binary records now have native
process owners. Decision 0413 additionally removes host-side binary decoding
from both bounded segment loops. The next slice is ordered digest-bound
composition and private resource lifecycle, followed by Linux execution,
promotion, and the grouped retirement gate.

## Reconsideration triggers

Version the command if `WVCD`, `WVHT`, `WVHU`, `WVHM`, the segment ceiling,
resource naming, or the downstream segment-set admission contract changes.
Do not move response-payload reconstruction out of the independent admission
boundary merely to make the producer appear more complete.
