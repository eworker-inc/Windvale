# Decision 0636: Windvale-owned client program resource

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-15
- Advances: [Decision 0635](0635-Windvale-Owned-Recyclable-Client-Image-And-Context.md)
- Contract: [client program-resource emission](../../Specifications/Windvale-Os-X64-Process-Client-Program-Resource-Emission.md)

## Context

The recyclable client has a private interpreter and execution context, but its
program identity still needs a generation-tagged resource descriptor before
program input or readiness can be bound safely.

## Decision

- Emit the exact 248-byte table-clear and first resource slice at fixture
  offsets 9,683 through 9,930.
- Require the exact admitted 32-byte program digest.
- Preserve fixed generation-one rights, private pointers, response bounds, and
  the derived page-table reference.
- Keep remaining resources, program binding, publication, rollback, and QEMU
  execution as later mandatory steps.

## Evidence and consequences

The exact slice has SHA-256
`a8e2b2f3be9588c6b3b044aa6bf67a75f06a38b19b902bd4f3c665640e7fad20`.
The self-test WVB is 12,763 bytes at
`d0c7e8f7890e6cbc0168dfe122564b48f03a2c4d5bfb658e4e20a9c4ec4e85a1`.
Its Windows executable is 168,960 bytes at
`ac00e3dc1267d2c1c5ce11e389ea93711297930a7b99c1fb061d148b3c001f49`;
the paired Linux image is 172,144 bytes at
`d8b7bf66d482a976a7ecec2b3c0d408c52d942e0b0c75360883cf117aab3d72f`.
The focused owner passes 102 cases across seventeen projects with local results
50 through 66. The retirement inventory is 70 suites and 3,666 cases.

Windvale source now reconstructs the first 9,931 process-machine bytes and all
34 relocation fields in that interval. Remaining client resources are next.

## Reconsideration triggers

Replace fixed records when admitted executable metadata drives general resource
construction. Preserve exact identity admission, clearing before population,
rights and generations, private pointers, readiness-only publication, and
failure-atomic rollback.
