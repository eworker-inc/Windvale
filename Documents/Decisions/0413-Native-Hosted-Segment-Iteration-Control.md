# Decision 0413: Native hosted segment iteration control

- Status: Implemented candidate; Windows composition complete under Decision 0414
- Date: 2026-08-08
- Advances: [Decision 0403](0403-Native-Hosted-Service-Bundle-Request.md), [Decision 0404](0404-Native-Hosted-Container-Segment-Request.md), and [Decision 0412](0412-Native-Hosted-Container-Segment-Manifest.md)
- Contracts: [Native service-bundle requests](../../Specifications/Windvale-Native-Hosted-Service-Bundle-Request.md) and [native container-segment requests](../../Specifications/Windvale-Native-Hosted-Container-Segment-Request.md)

## Context and decision

Every hosted-container binary record has a Windvale-native process owner, but
the following PowerShell/Bash adapter still needed both service-bundle and
final-application segment counts. Decoding `WVPQ` or `WVCD` in those scripts
would create a second format implementation. Treating the first rejected index
as end-of-loop would make a malformed resource indistinguishable from normal
completion.

Extend each existing request producer with a read-only `count` mode. It admits
the same plan, geometry, and complete immutable resource set already required
for request production, reuses that producer's exact segment limit, writes no
file, and reports one bounded decimal count. Keep these modes in their focused
owners rather than enlarging the orchestration-control graph; the current
native compiler rejects that otherwise valid combined closure.

The resulting shell adapter can parse only fixed process-control text. It does
not interpret Windvale binary fields, guess completion from failure, construct
formats, or broaden the authority of either portable core.

## Evidence and consequences

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Service-bundle request WVB | 29,070 | `f79852fc85b87b4484596b7aa6a41efac2365edeb3f933b32fe12797f19e43e2` |
| Windows service-bundle request | 302,080 | `b3c7db2f5721beee13473462ce49313c41e2e6f08f98a37ce0fee6139c1810bc` |
| Linux service-bundle request | 303,104 | `e7e90cfc824bcd345f28edbd432d4a3826fa6a21ba7a7818904de4fc90c51371` |
| Container-segment request WVB | 44,019 | `c18f71d2a20612dd10063e88a9ebb34ff1a416da207ad685d49fc0e92ed8e206` |
| Windows container-segment request | 519,168 | `c4690d57b85b951b5af2c7eefdbd81a805114f9a246c02bbf2b593ecec34da18` |
| Linux container-segment request | 520,192 | `4207ba76d4387ec3dce54210a9278e616ddf32ae41f36d3f478f8e134147f82d` |

Both WVBs reconstruct exactly through the native Project 1 front door. After
review, the zero-warning Release build completes in 8.69 seconds. The two
selected tests pass 1/1 in 5.037 and 5.654 seconds, covering exact independent
counts, current-host native execution, existing request equality, malformed
and alias rejection, output preservation, and zero CLR/hostfxr/hostpolicy
loads. No broad verifier ran.

[Decision 0414](0414-Digest-Bound-Native-Hosted-Container-Composition.md)
closes the Windows host boundary with digest-bound tool acquisition, ordered
child execution, and private-resource cleanup rather than binary parsing or
loop discovery. Linux execution, promotion, and the grouped retirement gate
remain.

## Reconsideration triggers

Version the affected command when `WVPQ`, `WVCD`, source geometry, or either
segment ceiling changes. Keep count mode read-only and require the same
admission as request production. Do not add binary parsing to platform scripts
or use rejected requests as an iteration sentinel.
