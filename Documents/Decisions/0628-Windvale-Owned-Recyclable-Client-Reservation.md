# Decision 0628: Windvale-owned recyclable-client reservation

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-15
- Advances: [Decision 0627](0627-Windvale-Owned-Init-Image-And-Context-Construction.md)
- Contract: [recyclable-client reservation emission](../../Specifications/Windvale-Os-X64-Process-Client-Reservation-Emission.md)

## Context

The init process's private image and context are now source-owned. The retained
machine next reserves the recyclable client extent before allocating the
directory provider so that client generation replacement cannot disturb the
provider above it. That ordering and its complete failure surface must remain
explicit during fixture retirement.

## Decision

- Emit the exact normalized 118-byte recyclable-client reservation slice at
  fixture offsets 3,098 through 3,215.
- Reserve exactly 122 pages against memory-object record `0xd90` and first-
  generation client reference `0x00010002`.
- Reject null, unaligned, out-of-range, or cross-2-MiB-window extents before
  retaining the root address.
- Preserve allocator import symbol 13, addend -4, and all four failure edges.
- Retain the root privately in the coordinator stack without initializing or
  publishing a client process record.
- Keep directory/provider construction and application publication as later
  failure-atomic composition steps with live QEMU evidence.

## Evidence and consequences

The normalized slice has SHA-256
`e5aeaef67c50076c8b46c1da56dd788420020a1fe1c41ca88f4f3a41cd27c0ab`.
The self-test WVB is 14,957 bytes at
`bd9bd8bb378642e707e5a328a783dd42df20457aa04c967fcbf63cf8845678b4`.
Its Windows executable is 211,968 bytes at
`b98c4e3351ea369e6eb70fb8476b03d61300065ae0b57e0d860de458a955196f`;
the paired Linux image is 217,200 bytes at
`547f5351c84530e41436b51f03b25680f1added815d6238998dc5fe7915e0684`.
The focused owner passes 54 cases across nine projects with local results
50/51/52/53/54/55/56/57/58. The retirement inventory is 70 suites and 3,618
cases.

Windvale source now reconstructs the first 3,216 process-machine bytes and all
24 relocation fields in that interval. The next source boundary is the isolated
directory-provider allocation, record, paging, image, and context construction.

## Reconsideration triggers

Replace the fixed reservation ordering when a general allocator can prove
generation-safe non-tail reuse independently of provider placement. Preserve
checked geometry, exact charging, non-publication before complete construction,
stale-generation rejection, and rollback.
