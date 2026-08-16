# Decision 0731: Publish a durable filesystem domain ledger

- Status: Accepted; consumer binding, provider entry, and first read pending
- Date: 2026-08-16
- Advances: [Decision 0730](0730-Publish-Provider-Side-Filesystem-State.md)
- Contracts: [resource-domain record](../../Specifications/Windvale-Os-Resource-Domain-Record.md), [resource-domain policy](../../Specifications/Windvale-Os-Resource-Domain-Policy.md), and [filesystem-machine emission](../../Specifications/Windvale-Os-X64-Process-Filesystem-Machine-Emission.md)

## Context

The filesystem process, thread, and provider-only endpoint were ready, and the
portable launch policy proved a one-process/81-user-page/one-endpoint charge.
That charge was not retained in live kernel state. The state page had no blank
record range, while widening it would disturb the fixed paging and process
layout before a real need had been measured.

The terminal directory endpoint occupied a 64-byte slot immediately before the
thread table. Its channel remains separately retained, but the endpoint itself
was closed after six resolutions, one close, and successful provider exit. It
held no surviving authority and could be reused only after validating every
identity and terminal field.

## Decision

Define fixed record `WVDOM001` and reuse state bytes `0x860..0x89F` only after
an exact terminal-directory-endpoint preflight. Clear the slot, construct domain
reference `65538` privately, and bind it to filesystem process `196610` with
limits and committed use of exactly one process, 81 user pages, and one
endpoint. Reservations are zero and lifecycle is alive.

Publish the record by writing its two magic words after all other fields. The
second magic word is the record publication point in the current single-CPU
boot transcript. Commit domain, provider endpoint, thread, and process in that
order. Keep endpoint client reference 0; publishing accounting does not grant a
consumer capability and does not authorize traffic or provider entry.

Do not reinterpret the retained directory channel. Do not reuse the ledger for
the later network generation until filesystem stop has closed its endpoint,
released its process and memory, reduced committed use to zero, and validated
the exact terminal record.

## Consequences

The process object remains 956,321 bytes at SHA-256
`ea07c502f0b3f45e650284426c136c601c9fdacf8addfa9f99fd890cc2a535a1`.
The filesystem construction object becomes 2,654 bytes at SHA-256
`51a7302bfe8f5565cb9e17522a4d042b618df2903944f2c567b46c9193d002d8`.
Linked normal executable code ends at byte 793,575, below the fixed
794,624-byte supervisor RX boundary.

The normal EFI becomes 1,698,816 bytes at SHA-256
`0796a5d70d865d35bcf0833a6d6d1168bba2fe35c5968b8db6e73767ca763cc2`.
The invalid-opcode EFI has the same size and SHA-256
`14cd177057858acd35023abd558e54670ca0e4c80122f43a7f7671f5a767ae6a`.
The general-protection EFI has the same size and SHA-256
`cda36c9cdea101c81199ccd16422d1285b157e6f1dc8b69819280ccb6755d351`.
Pinned Windows QEMU 11.0/Q35/TCG passes the normal shutdown and both terminal
exception transcripts.

This is a durable fixed accounting ledger, not a general mutable domain
object. It does not solve the remaining post-allocation rollback gap, create a
surviving application, bind FAT32 media identity, dispatch the filesystem
thread, or execute an I/O request.

## Reconsideration triggers

Replace the fixed record when dynamic membership, concurrent accounting, peak
evidence, stop reasons, or a public kernel interface is required. Enlarge the
state layout rather than reusing another record if exact terminal ownership
cannot be proven. Never erase a live or authority-bearing endpoint to make
space for accounting.
