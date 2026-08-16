# Decision 0634: Windvale-owned recyclable-client paging

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-15
- Advances: [Decision 0633](0633-Windvale-Owned-Recyclable-Client-Record.md)
- Contract: [recyclable-client paging emission](../../Specifications/Windvale-Os-X64-Process-Client-Paging-Emission.md)

## Context

The recyclable-client record now binds admitted identities, budgets, addresses,
and rights inside its private extent, but the retained fixture still creates the
address space that enforces those promises. Client paging must preserve W^X,
the null hole, exact bounds, and guard entries before any input is copied.

## Decision

- Emit the exact 4,748-byte client paging slice at fixture offsets 4,859
  through 9,606.
- Copy only the three retained kernel table pages and construct one bounded
  private user PTE page.
- Map 110 code pages executable and non-writable; map six stack pages, one data
  page, and one response page writable and NX.
- Explicitly clear the two post-extent guard PTEs.
- Keep interpreter/program copies, context/resource completion, readiness
  publication, rollback, and QEMU execution as later mandatory steps.

## Evidence and consequences

The exact slice has SHA-256
`824ec2c944b5bebe479bf785eb2e30eeb05d06e04e95245e90c83cea27585a62`.
The self-test WVB is 14,563 bytes at
`b848688f23ff1e1750044eaec3b4f1837454f7a0c73938699435ce56f81b8fe9`.
Its Windows executable is 206,336 bytes at
`5e67969e9047f8b5d71ec79d0de6c86bfdaa77905fac314d12a6ab9d8e7cced7`;
the paired Linux image is 209,008 bytes at
`bd58157bc0b8023ea2a413c50a5b275bf958b256d08fcbb310a8abb96cca740e`.
The focused owner passes 90 cases across fifteen projects with local results
50/51/52/53/54/55/56/57/58/59/60/61/62/63/64. The retirement inventory is 70
suites and 3,654 cases.

Windvale source now reconstructs the first 9,607 process-machine bytes and all
33 relocation fields in that interval. The next boundary is private client
image/context construction, not live publication.

## Reconsideration triggers

Replace fixed page counts when admitted executable metadata drives general
layout. Preserve checked table copies, the null hole, W^X, explicit user bounds,
post-extent guards, private construction, and failure-atomic publication.
