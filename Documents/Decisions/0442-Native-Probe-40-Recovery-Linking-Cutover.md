# Decision 0442: Native Probe 40 recovery linking cutover

- Status: Implemented current-host normal-scenario recovery cutover; Linux execution pending; top-level WVA assembly advanced by [Decision 0443](0443-Native-Probe-40-Top-Level-Wva-Assembly.md)
- Date: 2026-08-09
- Advances: [Decision 0439](0439-Native-Uefi-Recovery-Packaging-Cutover.md), [Decision 0441](0441-Scale-Safe-Native-Wv-Linker-Relocation-Emission.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)
- Runbook: [Native tests](../Runbooks/Native-Tests.md#windvale-os-boot-execution)

## Context

The normal Probe 40 recovery command already used the retained native UEFI
packager, but it asked `Windvale.Bootstrap` for a complete managed-linked flat
image. Decision 0440 exposed the same build as fourteen ordered WVOs, and
Decision 0441 made the digest-bound native linker reproduce the exact real
image without widening its runtime arena. The managed link was therefore the
next removable executed step.

## Decision

- Make `Rebuild-Os-Probe.ps1` ask Stage 0 for the reviewed Probe 40 object
  inventory rather than `--linked-output`.
- Admit exactly inventory format 40, one machine entry symbol, fourteen ordered
  unique `.wvo` basenames, and fourteen existing files before native linking.
- Select the digest-bound Windows or Linux `Link-Wvo` launcher for the current
  host, link at base address zero, and parse exactly one matching decimal entry
  address from the canonical native map.
- Pass the native flat image and parsed entry offset to the already retained
  native UEFI packager. Preserve exclusive final publication and clean the
  private object, flat-image, and EFI candidates.
- Retain `Buildˉlinkedˉimage` and the `--linked-output` mode only as frozen
  Stage 0 recovery/differential evidence. They are no longer executed by the
  normal recovery script.
- Keep `.NET` explicit for the remaining Probe 40 object/scenario production.
  This slice does not claim native object production, all-scenario
  reconstruction, or final .NET retirement.

## Evidence and consequences

The focused current-host normal recovery command completes in 15.6 seconds.
It generates fourteen objects through Stage 0, links them through the
digest-bound 1,796,608-byte Windows native linker, packages through the retained
native UEFI application, and publishes the established 683,008-byte EFI at
SHA-256
`080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9`.
The private object directory, linked image, and candidate EFI are absent after
completion.

No broad Seed, OS, QEMU, Qualification, or Linux recovery command ran for this
local cutover. Independent Linux execution remains required before the
cross-host slice is qualified. The next upstream retirement boundary is
Probe 40 object production, not another linker or packager implementation.

## Reconsideration triggers

Reconsider the strict fourteen-object admission only through a reviewed Probe
format change. Reconsider the native cutover if the Linux launcher cannot
reproduce the same image/map contract or if failure cleanup can affect a path
outside the unique recovery candidate directory.
