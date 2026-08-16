# Decision 0627: Windvale-owned init image and context construction

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-15
- Advances: [Decision 0626](0626-Windvale-Owned-Init-Page-Table-Construction.md)
- Contract: [init image and context emission](../../Specifications/Windvale-Os-X64-Process-Image-Emission.md)

## Context

The source-owned init page tables expose only the intended private pages, but
those pages remain empty. The next machine boundary must copy each verified
input with its exact bound, retain typed data relocations, and initialize the
execution/store metadata before any process publication is possible.

## Decision

- Emit the exact 149-byte init content and context slice at fixture offsets
  2,949 through 3,097.
- Reject an empty input or an image that exceeds its one- or two-page mapping.
- Keep service, admitted-program, execution-budget, and resource-store copies
  separate and preserve their four relative-i32 WVO data relocations.
- Initialize exact native context format/size and bounded instruction/call-depth
  budgets in private data memory.
- Publish the resource-store descriptor only inside the unpublished extent.
- Do not activate or dispatch the process until later construction composes
  record publication, root activation, rollback, and live QEMU evidence.

## Evidence and consequences

The exact slice has SHA-256
`6f8d6a7c0fcdf3c1b76955b43057c7b3e1e52d9eeedc069b51c6fbd718316b8e`.
The self-test WVB is 16,434 bytes at
`3207175a3928407f8b0fb1976e8f55c3643ffa5f0555a46fa9379354d90c0ae1`.
Its Windows executable is 212,480 bytes at
`722e4d867408a750d534ddd2ca55b43512ef934d68fd66aaf8e8ba1411d6c8e7`;
the paired Linux image is 217,200 bytes at
`58b42db3daa211c10f79426dae970fb635233ec19e7f135a1e54ed963e526a87`.
The focused owner passes 48 cases across eight projects with local results
50/51/52/53/54/55/56/57. The retirement inventory is 70 suites and 3,612 cases.

Windvale source now reconstructs the first 3,098 process-machine bytes and all
19 relocation fields in that interval. The next source boundary reserves the
recyclable client extent before directory-provider and client construction.

## Reconsideration triggers

Replace fixed page destinations and budgets when general process construction
derives layouts from verified executable metadata. Preserve exact input bounds,
typed relocations, W^X-compatible copies, private context initialization,
generation-safe publication, and failure-atomic rollback.
