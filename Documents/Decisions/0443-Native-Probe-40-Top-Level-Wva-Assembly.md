# Decision 0443: Native Probe 40 top-level WVA assembly

- Status: Implemented current-host normal-scenario cutover; Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0442](0442-Native-Probe-40-Recovery-Linking-Cutover.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)
- Runbook: [Native tests](../Runbooks/Native-Tests.md#windvale-os-boot-execution)

## Context

After native linking and UEFI packaging, the normal Probe 40 recovery command
still asked Stage 0 to assemble three top-level WVA sources even though WVA
assembly is already a qualified native surface. Those sources own the memory
object shims, timer shims, and kernel/trap/shutdown shims. Their exact WVOs are
ordinary inputs `06`, `07`, and `11` in the reviewed fourteen-object link.

The object-inventory implementation also lived in the already large
`Firmware-Probe.cs` source despite having a focused
`Firmware-Probe-Object-Inventory.cs` owner.

## Decision

- Add an explicit Stage 0 inventory scope that omits only top-level WVA objects
  `06`, `07`, and `11`. Preserve the complete default inventory and its existing
  C# assembler checks as frozen recovery/differential evidence.
- Give the recovery CLI a distinct `--object-directory-native-wva` mode whose
  format-40 report contains the exact remaining eleven ordered objects.
- Make `Rebuild-Os-Probe.ps1` assemble the three repository-owned WVA sources
  through the current host's digest-bound `Assemble-Wva` launcher. Admit each
  resulting WVO by its exact reviewed SHA-256 before linking.
- Reconstruct the original fourteen-name order explicitly, then reuse the
  native linker and native UEFI packager selected by Decision 0442.
- Move `Buildˉobjectˉinventory` from the broad firmware-probe source into the
  focused object-inventory source. This is a cohesive ownership extraction,
  not numbered fragmentation or a change to OS behavior.
- Do not claim that all Probe 40 WVA assembly is retired: process-image
  composition still contains inner WVA assembly owned by Stage 0.

## Evidence and consequences

The native assembler produces these exact objects:

| Object | Bytes | SHA-256 |
| --- | ---: | --- |
| `06-memory-object-shims.wvo` | 2,538 | `fe0a94461b743be58319d2e2f8b737840ec1216e61a98ee7e210f96f97f85bee` |
| `07-timer-shims.wvo` | 1,202 | `e331a1db404b8b8359d35d410792496683a63acee621ff64f128a6eae128c344` |
| `11-kernel-shims.wvo` | 1,894 | `845d45d6787ec819ca300ffc81a9ffe3e86c7b3998f3dd2a50a017a353d86193` |

The focused Bootstrap project build succeeds with zero warnings. The updated
current-host recovery command completes in 16.0 seconds and reproduces the
exact 683,008-byte normal EFI at SHA-256
`080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9`.
Private objects, linked image, and EFI candidate are removed after completion.

No broad Seed, OS, QEMU, Qualification, or Linux recovery command ran for this
local cutover. Stage 0 now produces eleven top-level link objects. Its remaining
executed assembly, source compilation, native lowering, machine-code/object
construction, and object composition stay explicit for later slices.

## Reconsideration triggers

Reconsider the exact three-object split when another top-level object acquires
a stable repository-owned WVA source or when process-image WVA components are
exposed as independently linkable objects. Any object name, source, or digest
change requires review before the recovery script accepts it.
