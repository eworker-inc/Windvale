# Decision 0631: Windvale-owned directory-provider paging

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-15
- Advances: [Decision 0630](0630-Windvale-Owned-Directory-Provider-Record.md)
- Contract: [directory-provider paging emission](../../Specifications/Windvale-Os-X64-Process-Directory-Paging-Emission.md)

## Context

The directory provider has a measured private record but no address space. It
must preserve required kernel mappings while exposing only its code, stack,
data, response, and immutable snapshot pages with exact W^X permissions.

## Decision

- Emit the exact 440-byte directory paging slice at fixture offsets 3,785
  through 4,224.
- Copy the three retained kernel table pages into the isolated extent.
- Fill exactly one bounded identity PTE page and preserve null-page denial.
- Map code executable/non-writable, mutable pages writable/NX, and the immutable
  snapshot read-only/NX; expose no unlisted user page.
- Preserve both local branch fields explicitly and add no external import.
- Keep provider image/context construction, readiness publication, rollback,
  and QEMU execution as mandatory later steps.

## Evidence and consequences

The exact slice has SHA-256
`6ec4a6d510027b8871346b888e0e6c0479a17696f6e0372bfbec45aa5c993bf4`.
The self-test WVB is 14,228 bytes at
`caba027a75434fc07c2f44cafead16f595e7ce4fc13a84864041204d24cd5c17`.
Its Windows executable is 203,776 bytes at
`0308cf1a5d01eeb2d463f43bc4ea3b3993f4922b5732cee7e8b23964e2d001c0`;
the paired Linux image is 209,008 bytes at
`303eada707e4868fba8406ccc304e5764ce069d156808d6f44245e98629fb0d9`.
The focused owner passes 72 cases across twelve projects with local results
50/51/52/53/54/55/56/57/58/59/60/61. The retirement inventory is 70 suites
and 3,636 cases.

Windvale source now reconstructs the first 4,225 process-machine bytes and all
31 relocation fields in that interval. The next boundary is measured provider
service/snapshot copy and private context/descriptor initialization.

## Reconsideration triggers

Replace fixed page geometry when verified provider metadata drives general
layout construction. Preserve bounded table work, null-page denial, W^X,
minimal user visibility, readiness-only publication, and rollback.
