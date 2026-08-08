# Decision 0409: Native fixed-service acquisition

- Status: Implemented candidate; ordered process orchestration pending
- Date: 2026-08-08
- Advances: [Decision 0408](0408-Native-Enum-Service-Fragment-Reconstruction.md), [Decision 0402](0402-Native-Hosted-Metadata-Request.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native hosted fixed-service acquisition](../../Specifications/Windvale-Native-Hosted-Fixed-Services.md)

## Context and decision

Decision 0408 made the variable enum-service fragment reconstructible through
the native lowerer and linker. The candidate hosted-container path still used
managed service-bundle code to select nine checked-in fixed leaves, choose the
platform variants, and place them around service 7 in source geometry.

Add one focused paired Windvale command that accepts those nine immutable
artifacts, validates their target-specific lengths and complete path set, reads
each exactly once, and stages the exact snapshots in canonical order. Keep the
variable enum service separate and leave process ordering and private-resource
lifecycle to the next slice.

Do not embed a second SHA-256 implementation in this small acquisition owner.
The already implemented native metadata-request process recomputes the exact
fragment and ten-service leaves from the staged resources before construction
continues. This preserves one cryptographic decision point while the
acquisition command owns only platform selection, size admission, alias
rejection, snapshotting, and placement.

## Evidence and consequences

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Fixed-service acquisition WVB | 7,491 | `048deb0818f11c61c2dd16b6bbcde8f7f58eb351c59149332d12bac6256797c0` |
| Native WVO | 58,340 | `674b063490c33477655233f508b337b826a448928913185cdb78e2ec1c1b78b1` |
| Windows application | 75,264 | `7f923dc636da591ac719f07a5f3c4f1f2ce24ae5866ba2176ce8dacf615583b0` |
| Linux application | 77,824 | `707144072747186ee2fd77e0a27c920a96fac03fe76b1bcaa90b7b4cb1db2dde` |

The native source front door and Stage 0 recovery compiler produce identical
WVB bytes. The digest-bound native lowerer accepts the result without a limit
change and produces the exact Stage 0 WVO. The reviewed focused contract checks
all nine current-host placements, the untouched enum slot, public target
routing, native execution without CLR loading, alias/duplicate/size rejection,
output preservation, native source reconstruction, and native WVO equality.
It passes 1/1 in 5.361 seconds after a zero-warning 10.44-second Release build.
No broader local verifier ran.

Managed code no longer needs to select or stage fixed hosted-service resources
in the candidate path. Ordered invocation, private temporary-resource cleanup,
service 7 integration, complete composition, Linux execution, promotion, and
the grouped dual-host retirement gate remain.

## Reconsideration triggers

Version this command when the hosted service order, fixed leaf sizes, source
geometry, or platform set changes. Keep leaf generation and digest evidence in
their existing owners. Do not turn this acquisition tool into a second service
generator, hash oracle, or broad child-process coordinator.
