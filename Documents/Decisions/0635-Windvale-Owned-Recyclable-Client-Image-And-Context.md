# Decision 0635: Windvale-owned recyclable-client image and context

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-15
- Advances: [Decision 0634](0634-Windvale-Owned-Recyclable-Client-Paging.md)
- Contract: [recyclable-client image emission](../../Specifications/Windvale-Os-X64-Process-Client-Image-Emission.md)

## Context

The recyclable client now has an exact private W^X address space, but its
interpreter copy and execution context remained fixture-authored. Both must be
bounded and relocation-typed before resource construction or publication.

## Decision

- Emit the exact 76-byte interpreter-copy/context slice at fixture offsets
  9,607 through 9,682.
- Reject empty or larger-than-110-page interpreter inputs.
- Preserve the typed interpreter relocation and fixed private context budgets.
- Keep resource/program binding, readiness publication, rollback, and QEMU
  execution as later mandatory composition steps.

## Evidence and consequences

The exact slice has SHA-256
`54432a2880a44c20e9c9246eeab45a488a9f9aa7746d2eff9aaef0671faac633`.
The self-test WVB is 13,798 bytes at
`e45446f9c0aa6d8806c3427d2aa3900266067112ff90c29b8d0dea2ea4f4aafd`.
Its Windows executable is 187,904 bytes at
`741049bdb17717f89fc617322a5aa07fe94a4e2c2e3e1286a5a83d62b285067f`;
the paired Linux image is 192,624 bytes at
`a2b3880da1d0bdefaf491717d180bb638118d9b706f550f2100b7e596382c1fe`.
The focused owner passes 96 cases across sixteen projects with local results
50 through 65. The retirement inventory is 70 suites and 3,660 cases.

Windvale source now reconstructs the first 9,683 process-machine bytes and all
34 relocation fields in that interval. Client resource construction is next.

## Reconsideration triggers

Replace the fixed interpreter limit and budgets when admitted metadata drives
general layout. Preserve exact bounds, typed relocations, private W^X input
copy, bounded context state, readiness-only publication, and rollback.
