# Decision 0629: Windvale-owned directory-provider allocation

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-15
- Advances: [Decision 0628](0628-Windvale-Owned-Recyclable-Client-Reservation.md)
- Contract: [directory-provider allocation emission](../../Specifications/Windvale-Os-X64-Process-Directory-Allocation-Emission.md)

## Context

The recyclable client extent is now reserved below the retained directory
provider. The next source boundary must allocate the provider's isolated extent
with the same checked geometry and failure-atomic non-publication rules before
its immutable image or endpoint can become available.

## Decision

- Emit the exact normalized 107-byte directory allocation slice at fixture
  offsets 3,216 through 3,322.
- Allocate exactly ten pages against memory-object record `0xec0` and directory
  reference `0x00010003`.
- Reject null, unaligned, out-of-range, or cross-2-MiB-window results.
- Preserve allocator symbol 13, addend -4, and all four failure edges.
- Do not initialize or publish the provider until its record, page tables,
  immutable service/snapshot inputs, context, descriptor, endpoint state, and
  rollback are composed.

## Evidence and consequences

The normalized slice has SHA-256
`4b1c706de37503a89df9eecc9245f9e38f36a5281e0fcd1d370b921912b4be88`.
The self-test WVB is 14,733 bytes at
`c75790ba9823172830b6da72f83a77ce9de2014e0ac9ce4730283a21e261d76f`.
Its Windows executable is 207,872 bytes at
`45d79cbb35032809d41adb4711803772dad0f07a8696674614e832c651748d75`;
the paired Linux image is 213,104 bytes at
`551df680881fb91b911caa77f92cb60e02e5f68c11544ea24ffe9b3b634486a3`.
The focused owner passes 60 cases across ten projects with local results
50/51/52/53/54/55/56/57/58/59. The retirement inventory is 70 suites and
3,624 cases.

Windvale source now reconstructs the first 3,323 process-machine bytes and all
29 relocation fields in that interval. The next boundary is complete private
directory-provider construction, not endpoint publication.

## Reconsideration triggers

Replace the fixed ten-page extent when a verified provider image supplies
general layout metadata. Preserve bounded geometry, W^X, isolated ownership,
readiness-only publication, stale-generation rejection, and complete rollback.
