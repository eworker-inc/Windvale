# Decision 0452: Native Probe 40 memory-object producer

- Status: Implemented current-host native-build candidate; Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0451](0451-Native-Probe-40-Paging-Object-Producer.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contracts: [OS memory-object producer](../../Specifications/Windvale-Os-Memory-Object-Producer.md), [OS Probe object producer](../../Specifications/Windvale-Os-Probe-Object-Producer.md), and [WVO object construction](../../Specifications/Windvale-Wvo-Object-Construction.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The ordinary Probe 40 build still consumed the 1,529-byte frozen Stage 0
`08-memory.wvo`. Its one 1,089-byte x64 code section owns normal-scenario memory
entry and page-allocation mechanics, exports two entry points, imports four, and
carries four relative relocations. The historical C# owner also contains other
scenario-specific construction, so retaining it as recovery evidence remains
useful until the final retirement gate.

Appending this larger recipe to the existing 317-line compact-object producer
would weaken reviewability and create a catch-all owner. A separate focused
Windvale source can reuse the same verified WVO construction contract while the
public launcher remains one stable command.

## Decision

- Add a 158-line hosted Windvale memory-object producer with the exact normal
  code, symbol, relocation, object-length, and digest identities.
- Keep the existing compact producer unchanged. Dispatch only the closed
  `memory` selector to the dedicated digest-bound package behind the unified
  `Produce-Os-Probe-Object` launchers.
- Construct and independently admit `08-memory.wvo` inside the ordinary Probe
  40 private work directory, then remove it from the frozen seed.
- Expand the producer lane from seven to eight cases. Preserve the existing
  destination, selector, extension, and normal-image contracts.
- Retain the C# emitter as frozen recovery/differential evidence; do not claim
  the non-normal recovery scenarios from this normal-image slice.

## Evidence and consequences

The dedicated producer identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Producer WVB | 37,769 | `2ae5f3a2f108b74a86150854c78f7f2dc0335cff2cb1e071be7718fce40e17e7` |
| Windows x64 application | 399,872 | `79461480b72cc1865278ea6f06170b8f4e9f4e849898d7b3c06aa3d36ff70032` |
| Linux x64 application | 401,408 | `02280b115ead806f8b6e2f1dd066d7d06a85ae571d790c66d05daecf2acc6554` |

Current-host execution reproduces the former seed object byte for byte at
1,529 bytes and SHA-256
`2668e17c3181e168415fb7bdee530873e2ddc8fa2d100af94bcc7b74909df3ed`.
After affected-test review, the producer lane passes 8/8 in 1.7 seconds and the
normal `os-probe` lane passes 2/2 in 11.4 seconds. The final EFI remains 683,008
bytes at SHA-256
`080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9`.

The frozen seed now contains four WVOs totaling 660,758 bytes. Seven ordinary
objects come from Windvale-native producers totaling 31,892 bytes, three more
come from native WVA, and the fourteen-object link order remains unchanged. The
retirement plan remains 2,331 bytes at SHA-256
`b9fddc319c4185393e255917c9b22d426101336cb3e2901d6f8043c6a043faab`
and contains 28 suites with 3,135 fixed cases.

Linux execution and every broad Seed, OS, QEMU, Standard, Qualification, and
complete retirement gate remain pending. No maintained Stage 0 artifact was
produced in this slice.

## Reconsideration triggers

Move the recipe to WVA or a shared native backend when its instructions and ABI
forms have an independent general consumer. Split the focused source only when
a real sub-contract exists; do not create numbered fragments solely to reduce
line count.
