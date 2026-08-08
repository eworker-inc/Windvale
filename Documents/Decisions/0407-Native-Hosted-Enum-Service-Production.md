# Decision 0407: Native hosted enum-service production

- Status: Implemented candidate; ordered resource orchestration pending
- Date: 2026-08-08
- Advances: [Decision 0406](0406-Native-Hosted-Source-Geometry-Production.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native hosted enum processes](../../Specifications/Windvale-Native-Hosted-Enum-Processes.md)

## Context and decision

The hosted source-geometry producer requires ten raw service resources. Nine
are fixed Windvale-owned leaves. Service 7, `Enumˉname`, is variable because an
exact `WVEN 1` metadata block derived from the selected WVB follows the fixed
323-byte x86-64 leaf. Stage 0 still owned the normal construction seam.

Transfer the bounded normal case as two focused native processes over the
existing `WVEQ 2` handoff:

1. after upstream full WVB verification, `wvhostenumrequest` revalidates the
   WVB 1.11 envelope and nominal-type section and writes the single complete-
   group request; and
2. `wvhostenumservice` passes that request through the existing Windvale-owned
   enum-metadata core and appends its admitted `WVEN 1` to the existing exact
   Windvale-owned leaf.

The split respects the pinned native compiler's deterministic source inventory
boundary, reuses the qualified metadata engine, and keeps WVB decoding,
metadata construction, and machine-leaf ownership separate. Multi-group
metadata remains an explicit later extension rather than hidden managed
fallback.

## Evidence and consequences

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Enum-request WVB | 25,098 | `cd3332893277fbdc5c64e90e62900458bad506ec10be5d8b381ea9ca61a14b97` |
| Windows enum-request producer | 279,040 | `64b6cad08646204af01dc6b6d06b581f54cfc2993ddb8f3d28b22b6f3f9cf032` |
| Linux enum-request producer | 278,528 | `e601e3e9a9259f48c0f8d7e59f9212422d4f520ce4d4b5bbe30f6381e4970a9f` |
| Enum-service WVB | 17,511 | `2aaa45372322f39c751e6abb3062c72c14d949eb29c6edd7ca756d4378955255` |
| Windows enum-service producer | 162,304 | `c4f2a7190ee68e39bc76f5870577be6db15e3763b18656ad40ec4ccd591cd1a8` |
| Linux enum-service producer | 163,840 | `1c118fc24c2948a64cd9f6c1a49163cfc62333330b86b30f54998307fa6a99dc` |

After reviewing the affected test, the final focused case passes 1/1 in 7.114
seconds after a zero-warning incremental build. It checks native-front-door
reproduction, exact Windows/Linux package identities, byte-for-byte agreement
with the frozen Stage 0 `WVEQ` and complete service oracles, public targets,
failure preservation, alias rejection, and current-host execution without CLR
loading. No broader verifier ran.

The bounded normal service-7 resource no longer requires managed construction
in this candidate path. Native fragment resource production, fixed-leaf
resource acquisition, ordered process/manifest lifecycle, multi-group enum
metadata, Linux execution, promotion, and the grouped retirement gate remain.

## Reconsideration triggers

Version the request producer when WVB nominal encoding changes. Add an explicit
segmented coordinator before accepting a `WVEN` larger than one Windvale byte
value. Do not merge fixed machine-leaf generation, metadata policy, or final
publication authority into either process.
