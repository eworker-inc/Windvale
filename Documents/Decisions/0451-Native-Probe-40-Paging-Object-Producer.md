# Decision 0451: Native Probe 40 paging object producer

- Status: Implemented current-host native-build candidate; Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0450](0450-Native-Probe-40-Native-Bridge-And-Support-Producer.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contracts: [OS Probe object producer](../../Specifications/Windvale-Os-Probe-Object-Producer.md) and [WVO object construction](../../Specifications/Windvale-Wvo-Object-Construction.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The ordinary Probe 40 build still consumed the 1,292-byte frozen Stage 0
`10-paging.wvo`. Its one 899-byte x64 code section installs the page-table root,
depends on four imported entry points, and carries four relative relocations.
The exact historical code uses instructions and 64-bit immediate forms not yet
represented by the accepted WVA surface.

Broadening WVA solely to spell this frozen code would add a semantic assembly
contract without a general consumer. Creating a second hosted object tool would
duplicate launcher, package, and test infrastructure. The existing bounded OS
Probe producer and shared verified WVO constructor remain the smallest coherent
owner for this exact architecture recipe.

## Decision

- Add `paging` as a fourth closed selector in the OS Probe object producer. Fix
  its complete code, symbol, relocation, object-length, and digest identities.
- Construct the WVO through the shared portable constructor and independently
  admit it before host publication.
- Generate `10-paging.wvo` inside the ordinary Probe 40 private work directory
  and remove it from the frozen seed. Keep the C# emitter as frozen
  recovery/differential evidence.
- Expand the producer lane from six to seven cases with the exact paging output.
  Retain the existing preservation and closed-input cases and the unchanged
  two-case normal-image lane.
- Keep the resulting 317-line source together because all four recipes share one
  closed selector, constructor, launcher, and package. Reassess the larger memory
  object separately; do not append it merely because this path exists.

## Evidence and consequences

The retained producer identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Producer WVB | 42,835 | `ab26d2cd8820887fc15475a4ee29aaf884af9b5a0d8bd3313a847d00cc03e042` |
| Windows x64 application | 461,312 | `fcd22c975ed04534d30733c5ddabb7811a9b9578effd0d27839d171bdac76d0c` |
| Linux x64 application | 462,848 | `c4e22a9f67d5bdb4f186ddfbb63aa93032712ea7bdc260ed28076b12f0217e80` |

Current-host execution reproduces all four former seed objects exactly. The
paging object is 1,292 bytes at SHA-256
`a6bcad24e4752acc1fbab75d6667e965f2ab4d5613edd2c8e6cda244616fba2d`.
After affected-test review, the producer lane passes 7/7 and the normal
`os-probe` lane passes 2/2. The final EFI remains 683,008 bytes at SHA-256
`080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9`.

The frozen seed now contains five WVOs totaling 662,287 bytes. Six ordinary
objects come from Windvale-native producers, three more come from native WVA,
and the fourteen-object link order remains unchanged. The retirement plan
contains 28 suites and 3,134 fixed cases.

Linux execution and every broad Seed, OS, QEMU, Standard, Qualification, and
complete retirement gate remain pending. No maintained Stage 0 artifact was
produced in this slice.

## Reconsideration triggers

Move this recipe to WVA when its generally useful missing instructions and
immediate forms are accepted for independent reasons. Split the hosted recipe
module when native cross-module bindings support a cohesive boundary or its
reviewability materially degrades.
