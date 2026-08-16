# Decision 0728: Boot-link the filesystem machine

- Status: Accepted; live provider launch pending
- Date: 2026-08-16
- Advances: [Decision 0727](0727-Separate-The-Filesystem-Transfer-And-Native-Stack.md)
- Contracts: [process-object build](../../Specifications/Windvale-Os-Process-Object.md), [filesystem-machine emission](../../Specifications/Windvale-Os-X64-Process-Filesystem-Machine-Emission.md), and [kernel paging](../../Specifications/Windvale-Kernel-Paging.md)

## Context

The generation-three filesystem record, page tables, image copy, and native
context already had focused source-owned emitters. The boot object still
contained only the provider image, so no ordinary Probe 40 link could resolve
or inspect those constructors. Adding them grows the linked executable code
through byte 791,447. The previous 772 KiB supervisor executable window ended
at byte 790,528 and therefore could not safely admit the new code tail.

Paging version 6 also mapped 772 KiB while its ownership record retained the
older 768 KiB literal. That mismatch did not change the active page tables, but
it made the published evidence inaccurate and had to be corrected rather than
carried into the next image.

## Decision

Build the three filesystem constructors into `05-process.wvo` as separate,
canonically ordered code sections. Export the image, paging, and record symbols
and bind the image constructor to the exact embedded 195,657-byte filesystem
image with one typed `relative-i32` relocation. Do not call any constructor in
this slice.

Advance kernel paging to version 7. Map exactly 194 consecutive supervisor
read-only/executable pages, a 776 KiB span, and record the same 794,624-byte
value in `WVKPAG07`. Preserve the seven-page hierarchy, null guard, NX, write
protection, and timer-MMIO mappings.

## Consequences

The process object is 956,230 bytes at SHA-256
`6c54a37dbe4e08d43068fed9bfb98edea536ae097666fa2c793a1c1bea9f9ac3`.
Its filesystem image, paging, and record constructors link at addresses
780,192, 780,256, and 783,600. The WVO contains 14 sections, 33 symbols, and 60
relocations.

Paging version 7 remains 1,292 bytes at SHA-256
`a76c4a199d46f6d91c0d3cd76aec7439a5e3fa72403cfc753fa6c36cd5b9b871`.
The three current 1,696,768-byte EFI identities are
`e9a113b0b108a9da0bf31a0802d1fa7ae58f4c1888a1e30a0eb7d090732d40d9`
for normal, `af1cacbc0d139958e6f8d083d68493b35e4987a8a843939506e27f9595a133e2`
for invalid opcode, and
`eea4961a1a4b2287737ccd238f088b53ae4274714bf2c88f5a2ef0f7c4bdb384`
for general protection. Eleven object-producer cases and all four focused
Probe 40 build/QEMU cases pass on Windows. Independent Windows/Linux
qualification remains pending.

This decision does not allocate the 85-page provider extent, invoke the
constructors, replace the empty configuration digest, advance or bind endpoint
`131072`, publish the generation-three record, enter the provider, or perform a
file read. Those actions remain the next boot-integration transaction and must
roll back without visible process state or committed resource charge on any
pre-publication failure.

## Reconsideration triggers

Regenerate this boundary when constructor bytes, provider image identity,
configuration/media identity, process-object layout, or executable code tail
changes. Never widen executable mappings implicitly; each growth requires an
exact link-bound check, ownership-record update, and boot evidence.
