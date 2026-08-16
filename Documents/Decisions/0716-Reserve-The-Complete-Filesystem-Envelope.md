# Decision 0716: Reserve the complete filesystem envelope

- Status: Superseded by [Decision 0727](0727-Separate-The-Filesystem-Transfer-And-Native-Stack.md)
- Date: 2026-08-16
- Corrects: [Decision 0620](0620-First-Checked-Os-Provider-Launch-Transaction.md)
- Advances: [filesystem implementation plan](../Project/Windvale-Filesystem-Implementation-Plan.md)

## Context

The filesystem process image occupies 48 RX pages. Its service shim places the
response buffer 1,024 bytes into private memory and gives syscall 5 an exact
65,600-byte capacity. The existing launch profile reserved only 16 private
pages, or 65,536 bytes. A maximum response would therefore extend 1,088 bytes
beyond the admitted private region.

## Decision

- Increase filesystem profile 2 from 64 to 65 total pages.
- Partition those pages as the unchanged 48-page RX image plus 17 RW/NX private
  pages.
- Reject the prior 64-page `WVPR 1` request rather than accepting a process that
  cannot safely expose its maximum protocol response.
- Carry the exact 65-page charge through reservation, commit, readiness
  publication, drain, release, and terminal zero-charge evidence.
- Leave the network profile unchanged at 96 pages.

## Consequences

The admitted filesystem process can now contain its complete response window,
including the 1,024-byte private prefix, without crossing its resource-domain
allocation. This is required before a multi-page syscall copy can be connected.

The correction does not itself map those pages, launch the process, or change
the qualified syscall handler. Those remain the active boot-integration slice.

The corrected service policy is a 10,150-byte WVB at SHA-256
`b31b0004a698fa3d4101241d3d0d4e87fc50384fef30f2be494183cdee99b8b7`.
The composed provider transaction remains 28,419 bytes at SHA-256
`7db47678e01b52473084fe65fc5430bb7b6e8c4e960ae6f6dd032aeab50f04f4`.
The application-launch owner now covers 42 cases, including explicit rejection
of the 64-page request; the provider transaction owner retains 15 cases.

## Reconsideration triggers

Change this partition only when a different exact image, buffer placement,
queue model, stack/heap budget, or measured recovery requirement changes the
maximum live mapping. Never infer private capacity from payload bytes alone;
include every header, prefix, alignment, and simultaneously live buffer.
