# Decision 0445: Digest-bound native Probe 40 object seed

- Status: Implemented current-host native-build candidate; Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0444](0444-Native-Probe-40-Inner-Process-Wva-Handoff.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)
- Native-test contract: [Windvale native retirement test suite](../../Specifications/Windvale-Native-Retirement-Test-Suite.md)

## Context

Decision 0444 removes managed WVA assembly from the normal recovery command,
but Stage 0 still compiles, lowers, adapts, internally links, and constructs
eleven top-level Probe 40 WVOs. Replacing those producers one at a time remains
necessary for source reconstruction. It need not keep `.NET` in the ordinary
native image-construction path: WVO is already a versioned verified handoff,
and Decision 0057 explicitly requires a digest-bound native bootstrap seed.

A frozen object seed is a distribution and bootstrap shortcut, not a claim
that the underlying source producers have already moved to Windvale.

## Decision

- Freeze the exact eleven-object, normal-scenario inventory produced from
  committed Decision 0444 state `957ed051ed226ce1b1694d19ad13b2c7e64a968a`.
  Record every filename, byte length, SHA-256, order, entry symbol, scenario,
  construction method, and provenance in one candidate manifest.
- Add ordinary Windows and Linux native launchers. They admit the frozen seed,
  assemble top-level WVA objects `06`, `07`, and `11` through the digest-bound
  native assembler, link the exact fourteen-object order through the native
  linker, and package the exact UEFI image through the native packager.
- Do not invoke `dotnet`, PowerShell, a CLR host, or the Stage 0 builder from
  either ordinary launcher.
- Refuse an existing destination, construct under one private sibling path,
  verify the final exact EFI identity before publication, and remove private
  files on success or failure.
- Keep `Rebuild-Os-Probe.ps1` as the explicit Stage 0 regeneration and
  differential path. The frozen seed does not replace its source provenance.
- Add one two-case `os-probe` lane to the .NET-free retirement coordinator:
  exact construction plus repeated-output rejection and preservation.

## Evidence and consequences

The candidate contains eleven WVOs totaling 692,650 bytes. The manifest pins
their individual identities and source commit. With the three exact native WVA
products, the current-host launcher emits the established 681,913-byte flat
image at SHA-256
`76aa64cc03c8b86dfe96f83d761be40e8128b988a182fd971004a287a5990af0`
and the established 683,008-byte EFI at SHA-256
`080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9`.

The first successful direct Windows run takes 9.1 seconds and leaves zero
private sibling paths. Repeating against its destination returns `1`, reports
that the output exists, preserves the exact bytes, and creates no private path.
The focused retirement-coordinator lane passes both cases in 9.5 seconds:

```text
PASS  suite os-probe cases=2
Suites: 1, Passed: 1, Failed: 0, Cases: 2
```

The Linux launchers pass Bash syntax validation but have not executed on Linux.
No broad retirement, Seed, OS, QEMU, Qualification, or non-normal scenario ran.
The complete coordinator now owns 26 suites and 3,123 fixed cases.

This creates a `.NET`-free ordinary normal-image build candidate without
pretending the eleven frozen objects are natively reconstructed. Source changes
still require either the explicit Stage 0 regeneration path or later native
producer slices. Other Probe 40 scenarios remain outside this candidate.

## Reconsideration triggers

Replace an individual frozen WVO when its source producer is reconstructed and
qualified natively. Replace or version the complete seed when Probe format,
scenario behavior, object order, entry symbol, or any exact object identity
changes. Do not promote this candidate or delete Stage 0 until Linux execution,
all scenarios required by the retirement gate, and final provenance qualify.
