# Decision 0435: Digest-bound OS boot execution

- Status: Implemented current-host boundary split and normal Probe 40 repair; five-scenario promotion pending
- Date: 2026-08-09
- Advances: [Decision 0045](0045-First-Uefi-Application-And-Boot-Probe.md), [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), and [Decision 0434](0434-Expanded-Native-Wva-Positive-Matrix.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

`Verify-Os-Boot.ps1` combined two independent responsibilities. It rebuilt a
Probe 40 EFI application through `Operating-System/Windvale.Bootstrap` and the
.NET SDK, then admitted firmware, launched QEMU, and checked exact serial and
preservation evidence. This made every ordinary boot execution depend on Stage
0 even when an already identified EFI artifact was available.

Committing the generated EFI images is not an accepted shortcut: repository
policy excludes firmware images, and checked-in binaries would not replace the
missing native constructor. Boot execution and image construction therefore
need separate retirement rows.

## Decision

- Require `Verify-Os-Boot.ps1` callers to provide one EFI path and its exact
  SHA-256 identity. Reject a missing or mismatched input before QEMU execution.
- Copy the admitted image into the private FAT root and require both the
  supplied image and run-private copy to remain byte-identical through the run.
- Remove all .NET discovery, project build, and Stage 0 dispatch from the boot
  verifier. Keep QEMU, firmware, serial, timeout, scenario, and cleanup
  contracts unchanged.
- Add `Tools/Recovery/Rebuild-Os-Probe.ps1` as the explicit Stage 0 image
  reconstruction command. It writes through a private sibling and refuses to
  replace an existing destination.
- Split inventory ownership: O1 is digest-bound boot execution; O2 remains the
  managed-normal Probe 40 image constructor until a native replacement exists.

## Evidence and consequences

Both changed PowerShell files parse without errors. Static inspection finds no
`dotnet`, builder-project, or `Windvale.Bootstrap` invocation in the boot
verifier. The first focused recovery command reconstructed a 683,008-byte
normal image at SHA-256
`6eeb2a73e32b54872687186447662f917e80b973d48f670d1498e76ffd376820`.

One focused normal-scenario boot admitted those exact bytes and reached the
pinned QEMU 11/OVMF guest. It did not pass: serial evidence stops after
`boot-services=exited` with `status=fail`, and QEMU exits `3` instead of the
expected `0`. The failure was repeated once only to retain and inspect the
serial log. This is an exposed current Probe 40 image/runtime blocker, not
successful O1 promotion evidence. The other four scenarios, broad OS suite,
Development, Standard, Qualification, and grouped retirement gate were not run.

Focused stage diagnostics located the failure in the init-owned resource-store
validation. WVB 1.11 had increased the embedded module and complete WVRS image
by one byte, but the WVA init shim still required the prior 1,195-byte store and
823-byte data region. The repaired shim now admits the generated 1,196-byte
store and 824-byte data region. After removing all diagnostic instrumentation,
the recovery command produced a 683,008-byte image at SHA-256
`080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9`.
One focused normal boot admitted that exact image and passed the complete Probe
40 serial contract with QEMU exit `0`. The other four scenarios and every broad
verifier remain deferred, so this is repair evidence rather than five-scenario
promotion.

No firmware image, generated cache, or vendor-specific metadata is committed.
The normal boot process no longer invokes Stage 0, while native image
construction and the remaining promotion gate stay explicit.

## Reconsideration triggers

Promote O1 only after the repaired current image passes all five exact scenarios
through this supplied-artifact boundary. Replace O2 only when a native
constructor reproduces every admitted scenario image; do not satisfy that row
with checked-in EFI binaries or an implicit managed fallback.
