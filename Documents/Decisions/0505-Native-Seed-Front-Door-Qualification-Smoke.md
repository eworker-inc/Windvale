# Decision 0505: Native Seed front-door qualification smoke

- Status: Current-Windows focused evidence complete; independent Linux and broad qualification integration pending
- Date: 2026-08-10
- Scope: source/project-to-WVB build, WVB verification, and WVB inspection inside the broad Seed qualification commands
- Extends: Decisions 0075, 0213, 0215, 0457, 0458, and 0504

## Context

`Tools/Verify/Verify-Seed.ps1` and `.sh` remain broad managed qualification
entry points. Even after the ordinary Project 1 builder, WVB verifier, and WVB
inspector became native-qualified, their qualification smoke still invoked the
managed tool for four representative builds, two verifications, two
inspections, and one malformed-project rejection.

Those calls obscured the actual retirement boundary and repeatedly exercised
Stage 0 for behavior already owned by digest-bound native products.

## Decision

Windvale adds paired `Verify-Seed-Native-Front-Door.ps1` and `.sh` helpers with
five fixed cases:

1. build, verify, and inspect `Sum-Data`;
2. build `Hello-Windvale`;
3. build, verify, and inspect `Read-Wvb-Header`;
4. build the three-source `Module-Composition-Demo` project; and
5. reject a project missing `emit wvb` while preserving its existing output.

The helper requires exact build/publication reports and these products:

| Product | Bytes | SHA-256 |
| --- | ---: | --- |
| `Sum-Data.wvb` | 494 | `76b4fa3c4c0cc37e6f1350e8191ccd78c6272224f146ef9816b5f987114c15df` |
| `Hello-Windvale.wvb` | 253 | `0a9230e700a10d14e718340e49562e5b0184a3c3a71b5cd29915126a6b28c28f` |
| `Read-Wvb-Header.wvb` | 1,701 | `c13efd14485afa1bf7fa418b54cea2fdd234fe34fdc824ae52346ce062be7793` |
| `Module-Composition-Demo-Project.wvb` | 660 | `030ce3f627e7bdeb8ff8a3432f01e94920c93551fd58d982bdafe9f9a5d24607` |

Both broad Seed scripts invoke that helper once at the start of their
qualification-only manual checks. This replaces nine managed invocations per
host script. The later managed execution, target packaging, explicit-source
composition, and broad harness work remain unchanged.

The two newly required manifests live beside their owning examples:
`Examples/Seed/Hello-Windvale.wvproj` and
`Examples/Foundation/Read-Wvb-Header.wvproj`. Project 1 paths are relative to
their manifest, so ordinary component projects should be colocated with their
source or component directory. This decision does not require all manifests at
repository root and does not prescribe a future multi-project workspace
layout.

## Evidence boundary

The current Windows helper passed all five cases in 2.8 seconds and reproduced
all four exact products. Its invalid project returned `WVP1004` at line 3,
column 1 and preserved the three-byte existing destination.

The Linux helper is structurally paired but was not executed because a Bash
host is unavailable in this environment. Changed-file planning therefore
reports the explicit `seed-native-front-door` gap. The broad Qualification
command was not rerun: it still contains managed work outside this slice and
would not be narrower evidence for the transferred boundary.

## Consequences

- B1, V1, and I1 now own this qualification smoke through their ordinary native
  front doors.
- T2 remains `managed-normal`; both broad Seed scripts and the GitHub workflow
  remain in the direct managed-entry inventory.
- The direct file count and the 42-suite/3,201-case fixed retirement plan do
  not change.
- Independent Linux execution, remaining broad Seed transfers, GitHub
  orchestration cutover, grouped qualification, promotion, and recovery
  retirement remain open.

## Reconsider when

Reconsider this decision if Project 1 path resolution, any of the four source
closures or exact products, native reports, malformed-project diagnostics,
qualification orchestration, or the future workspace/project layout changes.
