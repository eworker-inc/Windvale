# Decision 0440: Probe 40 object-inventory boundary

- Status: Implemented current-host recovery seam; native linker scale transfer pending
- Date: 2026-08-09
- Advances: [Decision 0439](0439-Native-Uefi-Recovery-Packaging-Cutover.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)
- Linker contract: [Windvale native linker](../../Specifications/Windvale-Native-Wv-Linker.md)

## Context

The Probe 40 Stage 0 coordinator constructed fifteen ordered WVO inputs and
immediately passed them to the managed linker. That hid the next recovery
boundary and made a native-link cutover inseparable from object production.

The retained linker source admits up to 64 WVO inputs, but its current Windows
container accepts only seventeen application arguments. Fifteen WVOs require
eighteen arguments after base, entry, and output. Reducing the physical input
count exposes a second measured boundary: the complete Probe 40 link enters
the native application but exits through its narrow v1 runtime's generic
resource mapping before publishing output. The available evidence does not yet
identify whether analysis, image reconstruction, verification, or canonical-map
construction owns the peak.

## Decision

- Give Probe 40 an explicit ordered object-inventory record and a recovery CLI
  mode that writes its WVOs only into an existing empty directory.
- Keep fifteen logical components, but pack the adjacent native-probe bridge
  and firmware-support sections into one verified WVO. The resulting fourteen
  physical inputs preserve section order, symbol meaning, relocation targets,
  and final linked bytes.
- Keep the packing invariant and the managed-link wrapper in the focused
  `Firmware-Probe-Object-Inventory.cs` source instead of expanding the already
  large firmware-probe source.
- Keep the real recovery command on its current managed-link/native-package
  boundary until the one Windvale linker handles this measured input. Do not
  add a fallback or claim a native-link cutover from an unpublished candidate.
- Add no source-language or OS behavior. This is frozen Stage 0 recovery
  plumbing and a measured handoff boundary.

## Evidence and consequences

The object-only command reports fourteen exact WVO containers. The managed
link oracle consumes them in order and produces the established 681,913-byte
payload at SHA-256
`76aa64cc03c8b86dfe96f83d761be40e8128b988a182fd971004a287a5990af0`.
Its canonical map has 663 lines and 129,387 UTF-8 bytes. The retained native
UEFI packager then reproduces the exact 683,008-byte normal EFI at SHA-256
`080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9`.

The retained native linker handles fourteen-input rejection paths, but the
complete resolved Probe 40 link exits `1` without publication. Reconstructing
its 1,655,296-byte Windows container through the current managed and native
host-container routes reproduces the existing SHA-256
`ca88735061d7e36e79813346621a867a9293d04d3c01ffb0336f4ee32cbe316d`;
stale packaging is not the cause. No QEMU, OS suite, Seed suite, or broad gate
ran for this boundary measurement.

## Reconsideration triggers

Improve the existing Windvale linker rather than adding a Probe-specific
linker. Preserve the complete canonical map contract while reducing peak
native resource use, or introduce a separately specified compact report mode
whose selection is explicit and whose default map remains unchanged. After a
retained Windows/Linux linker passes this exact fourteen-input case, switch the
recovery command and remove the managed link from its executed path.
