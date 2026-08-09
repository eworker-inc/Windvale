# Decision 0449: Native Probe 40 admission bridge producer

- Status: Implemented current-host native-build candidate; package identities superseded by [Decision 0450](0450-Native-Probe-40-Native-Bridge-And-Support-Producer.md); Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0448](0448-Native-Probe-40-Exception-Object-Producer.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contracts: [OS Probe object producer](../../Specifications/Windvale-Os-Probe-Object-Producer.md) and [WVO object construction](../../Specifications/Windvale-Wvo-Object-Construction.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The ordinary Probe 40 build still consumed the 484-byte frozen Stage 0
`12-wvb-admission-bridge.wvo`. Its code uses ordinary x64 operations but requires
an exact 64-bit execution-context immediate not presently in WVA. Expanding WVA
for one historical bridge would widen the assembler contract and change the
retained bytes unless both reference and native encoders gained a new canonical
instruction.

Decision 0448's focused exception producer already owns verified construction
for a similarly small architecture recipe. A first generic dispatcher failed
because the native compiler does not yet accept the cross-module function
binding shape. Keeping both bounded recipes in one focused hosted module compiles
natively, avoids a second pair of host packages, and remains a reviewable
155-line source file.

## Decision

- Replace the exception-only hosted application with one selector-bound OS Probe
  object producer. Keep both recipes in one module until native cross-module
  source bindings support cohesive extraction; do not grow it into an arbitrary
  object generator.
- Retain `exceptions` and `wvb-admission-bridge` as the only accepted selectors.
  Each selector fixes the complete WVO identity and independently verifies the
  constructed object before the launcher publishes it.
- Generate both `09-exceptions.wvo` and `12-wvb-admission-bridge.wvo` inside the
  ordinary Probe 40 private work directory. Remove the admission bridge from the
  frozen seed while keeping the C# recipe as frozen recovery/differential
  evidence.
- Replace the three-case exception-only lane with five cases covering both exact
  objects, independent admission, existing-output preservation, unknown-kind
  rejection, and invalid-extension rejection. Keep the unchanged two-case
  normal image lane as end-to-end link/package evidence.

## Evidence and consequences

The retained combined producer identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Producer WVB | 38,229 | `41696bba17570dda638abf9c0f58938950d8363b1f5044cb6dcf619b25d54cce` |
| Windows x64 application | 413,696 | `895237d4a651b4fb0a8a458a7bfa55f952c0364304d6e2af3f30fdc945ba5889` |
| Linux x64 application | 413,696 | `4c651c82379d3dc7f83781504182f33e3931b1b9e50a2574c23eb08faf3066bf` |

Current-host execution reproduces the 483-byte exception object and 484-byte
admission bridge exactly. After affected-test review, the combined producer
filter passes 5/5 and the normal `os-probe` filter passes 2/2. The final EFI
remains 683,008 bytes at SHA-256
`080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9`.

The frozen seed now contains seven WVOs totaling 664,040 bytes. Four ordinary
objects come from Windvale-native producers, three more come from native WVA,
and the fourteen-object link order remains unchanged. The retirement plan still
contains 28 suites and now owns 3,132 fixed cases.

Linux execution and every broad Seed, OS, QEMU, Standard, Qualification, and
complete retirement gate remain pending. No maintained Stage 0 artifact was
produced in this slice.

## Reconsideration triggers

Split the hosted recipe module when native cross-module bindings accept the
focused recipe boundary, or move this bridge to WVA if a generally useful and
independently specified 64-bit immediate instruction is accepted. Reconsider the
recipe itself if cross-host execution changes either retained WVO or EFI identity.
