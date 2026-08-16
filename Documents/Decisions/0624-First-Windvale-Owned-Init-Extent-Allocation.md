# Decision 0624: First Windvale-owned init extent allocation

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-15
- Advances: [Decision 0623](0623-Windvale-Owned-Process-Channel-And-Endpoint-Initialization.md)
- Contract: [process memory-allocation emission](../../Specifications/Windvale-Os-X64-Process-Memory-Allocation-Emission.md)

## Context

Windvale source now constructs the fixed process entry, dispatcher, coordinator,
and retained IPC records through fixture byte 1,871. The next boundary is the
first real kernel memory-object call. It must preserve the WVO import and every
failure edge rather than merely copying the allocation instructions.

## Decision

- Emit the exact 99-byte init extent allocation and validation slice.
- Pass the retained init object offset/reference and 12-page extent to the
  kernel allocator import without treating those values as portable semantics.
- Require nonzero, 4 KiB-aligned output, a bounded exclusive end, and one 2 MiB
  identity window before later record/page-table publication.
- Publish the allocation import's field, symbol index 11, addend -4, and four
  exact failure fields/displacements to the common target at offset 33,826.
- Keep this constructor disconnected from the live provider transaction until
  complete process-machine composition and QEMU evidence exist.

## Evidence and consequences

The normalized slice is SHA-256
`971392d74447dd464c33d6df5379891d324afe29f4bad21384c35942f9612723`.
The self-test WVB is 14,586 bytes at
`1baa66d77b35db8c2629c0cc2478e29b716739b5ad2c3a2a9096ad9439011112`.
Its Windows executable is 205,312 bytes at
`fe1aa700ae411cc3f02277bc13cc8980721fe62aa03f08b0862d81f5bf9e6270`;
the Linux image is 209,008 bytes at
`197947667b10fc4bb9a4df15117a0f34f9ff1237a950408679cf9c729fb008c8`.
The owner passes 30 cases across five projects with local results
50/51/52/53/54. The retirement inventory is 70 suites and 3,594 cases.

Windvale source now reconstructs the first 1,971 process-machine bytes and all
13 relocation fields in that interval. The next source boundary is complete
init record construction, not another allocation policy model.

## Reconsideration triggers

Replace the fixed init allocation arguments when dynamic process launch owns
general memory-object records. Preserve the checked alignment, address-range,
generation, rollback, and publication invariants under the new layout.
