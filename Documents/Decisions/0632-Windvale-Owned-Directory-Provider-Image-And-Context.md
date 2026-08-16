# Decision 0632: Windvale-owned directory-provider image and context

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-15
- Advances: [Decision 0631](0631-Windvale-Owned-Directory-Provider-Paging.md)
- Contract: [directory-provider image emission](../../Specifications/Windvale-Os-X64-Process-Directory-Image-Emission.md)

## Context

The directory provider now has a private record and W^X address space but no
service bytes, immutable snapshot, execution context, or snapshot descriptor.
Those inputs must remain bounded and relocation-typed before readiness can be
considered.

## Decision

- Emit the exact 116-byte private provider image/context slice at fixture
  offsets 4,225 through 4,340.
- Reject empty or over-mapped service and snapshot inputs.
- Preserve separate typed relocations for measured service and snapshot data.
- Initialize exact native context budgets and a generation-one snapshot
  descriptor inside private memory.
- Keep endpoint/process readiness publication, rollback, and QEMU execution as
  later mandatory composition steps.

## Evidence and consequences

The exact slice has SHA-256
`2cd7e484a2eb928cdc3862d2660a7a05f5b39a92efee153b1aa992eaf3dd30b2`.
The self-test WVB is 15,098 bytes at
`589034ed2ae906ba8c96ebedb3e583decb9d9181527b70b389d64296f66a4171`.
Its Windows executable is 204,288 bytes at
`b20d649b83c3b3ca54550118f77c7775a4937d789f0c08832c03444861c68fbd`;
the paired Linux image is 209,008 bytes at
`4c66120f10ba53e10cf1e7e31ca600eef51d47874b5f629aec0f8c46091bef98`.
The focused owner passes 78 cases across thirteen projects with local results
50/51/52/53/54/55/56/57/58/59/60/61/62. The retirement inventory is 70
suites and 3,642 cases.

Windvale source now reconstructs the first 4,341 process-machine bytes and all
33 relocation fields in that interval. The next boundary is complete recyclable
client construction, not provider publication.

## Reconsideration triggers

Replace fixed destinations and budgets when verified provider metadata drives
general layout. Preserve exact bounds, measured relocations, immutable snapshot
mapping, generation-tagged descriptors, readiness-only publication, and rollback.
