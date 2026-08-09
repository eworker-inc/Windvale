# Decision 0440: Probe 40 object-inventory boundary

- Status: Implemented; scale blocker resolved by [Decision 0441](0441-Scale-Safe-Native-Wv-Linker-Relocation-Emission.md)
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
count exposed a second measured boundary: the complete Probe 40 link entered
the native application but exited through its narrow v1 runtime before
publishing output. Decision 0441 later isolated that failure to the
134,217,728-byte native text/dynamic arena and the linker's repeated
complete-image relocation generations.

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

The original retained native linker handled fourteen-input rejection paths,
but the complete resolved Probe 40 link exited `1` without publication.
Reconstructing its 1,655,296-byte Windows container through the available
managed and native host-container routes reproduced SHA-256
`ca88735061d7e36e79813346621a867a9293d04d3c01ffb0336f4ee32cbe316d`,
excluding stale packaging. Decision 0441 records the succeeding scale-safe
candidate and exact current-host image evidence. No QEMU, OS suite, or broad
gate ran for this boundary measurement.

## Reconsideration triggers

After the retained Windows/Linux linker passes this exact fourteen-input case,
switch the recovery command and remove the managed link from its executed path.
Do not add a Probe-specific linker or weaken the complete canonical map.
