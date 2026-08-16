# Decision 0623: Windvale-owned process channel and endpoint initialization

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-15
- Advances: [Decision 0622](0622-First-Windvale-Owned-Process-Coordinator-Initialization.md)
- Contract: [process channel and endpoint emission](../../Specifications/Windvale-Os-X64-Process-Endpoint-Emission.md)

## Context

The source-owned process entry, dispatcher, and coordinator initialization end
at fixture offset 1,428. The next cohesive region constructs both IPC record
pairs used by the existing three-process baseline. It has no branches or WVO
imports, so leaving it opaque would preserve fixture dependence without any
relocation or source-capacity reason.

## Decision

- Emit both exact capacity-one `WVCHAN04` records and both exact open `WVEND01`
  service endpoints as one 444-byte Windvale module.
- Zero each complete record before publishing its magic and typed fields.
- Preserve distinct resource/directory capability references, provider process
  generations, the shared first client generation, and exact channel pointers.
- Keep the fixed record addresses and identities private to fixture migration;
  later dynamic provider launch must allocate rather than standardize them.
- Do not claim filesystem/network provider publication from these retained
  resource/directory endpoints.

## Evidence and consequences

The exact 444-byte payload has SHA-256
`92a53755d236709268d69b7b157ef7d2c8af345931e0dc06d2e2a77663b2104e`.
The self-test WVB is 14,386 bytes at
`2d9bdb6b1705bdc0e2e2f3a9b5e5e98224545abc1730ced3c5f55ec0a5cd1391`.
Its Windows executable is 213,504 bytes at
`bb53be86bb8351e805fd0919c6b0836efb483894c36568a6b38dde039a369b20`;
the Linux image is 217,200 bytes at
`b649ba1abe8db582942085afc90b14ad8d9cd44b542d232df3b7ea19f8a7eb2f`.
The x64 emission owner now passes 25 cases across four projects with local
results 50/51/52/53. The retirement inventory is 70 suites and 3,589 cases.

Windvale source now reconstructs the first 1,872 process-machine bytes and all
eight relocation fields encountered there. The boot object remains on the
reviewed fixture until the rest of the process machine is source-composed.

[Decision 0624](0624-First-Windvale-Owned-Init-Extent-Allocation.md)
subsequently owns the first kernel memory-object call and advances through byte
1,970 with 13 explicit relocation fields in the combined interval.

## Reconsideration triggers

Replace these fixed record constructors when live dynamic provider allocation
owns their addresses and generations. Preserve semantic capability identity and
generation validation, not the migration-era offsets.
