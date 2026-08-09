# Decision 0450: Native Probe 40 native bridge and support producer

- Status: Implemented current-host native-build candidate; Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0449](0449-Native-Probe-40-Admission-Bridge-Producer.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contracts: [OS Probe object producer](../../Specifications/Windvale-Os-Probe-Object-Producer.md) and [WVO object construction](../../Specifications/Windvale-Wvo-Object-Construction.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The ordinary Probe 40 build still consumed the 461-byte frozen Stage 0
`13-native-bridge-and-support.wvo`. It combines a 143-byte native-probe bridge
with a 23-byte byte-write support routine. The bridge calls two imported entry
points through relative relocations, while the support routine is a second code
section. This exact historical shape does not justify broadening WVA or creating
a separate host package.

Decision 0449's selector-bound producer already owns two similarly bounded x64
recipes over the shared verified WVO constructor. Adding the related bridge and
support recipe keeps one explicit Probe 40 owner. The resulting source is 211
lines, remains reviewable, and is not an open object-generation surface.

## Decision

- Add `native-bridge-and-support` as the third and final currently accepted
  selector in the focused OS Probe object producer.
- Fix its two code sections, four symbols, two relative-i32 relocations, complete
  WVO length, and digest. Independently admit the constructed WVO before the
  launcher publishes it.
- Generate `13-native-bridge-and-support.wvo` inside the ordinary Probe 40
  private work directory and remove it from the frozen seed. Keep the C# recipe
  frozen as recovery/differential evidence.
- Expand the producer lane from five to six cases by adding the third exact
  positive output. Retain the existing preservation and closed-input cases, and
  retain the unchanged two-case normal-image lane as end-to-end evidence.
- Treat 211 lines as a prompt to reassess the next recipe's ownership. Extract a
  cohesive module once native cross-module bindings support it; do not split the
  source into numbered fragments merely to reduce its size.

## Evidence and consequences

The retained producer identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Producer WVB | 40,025 | `889da93916e6387e35d11502c432ee5a661b5eefdc9772a0692235e846bfccfe` |
| Windows x64 application | 434,688 | `8e4c93a79584873b4cbf3884837f33f94e2909d0a8b99745739fb17dfed283cf` |
| Linux x64 application | 434,176 | `4ae292f582b309b0ce0fad9ccf90a5f115cf46f5f3ff489f587d313cb97231c5` |

Current-host execution reproduces all three former seed objects exactly. The
new object is 461 bytes at SHA-256
`472a0fbe6497525e634a4785e92aa9ee62c3c7d70fff7510e45acbea644eea0b`.
After affected-test review, the producer lane passes 6/6 and the normal
`os-probe` lane passes 2/2. The final EFI remains 683,008 bytes at SHA-256
`080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9`.

The frozen seed now contains six WVOs totaling 663,579 bytes. Five ordinary
objects come from Windvale-native producers, three more come from native WVA,
and the fourteen-object link order remains unchanged. The retirement plan
contains 28 suites and 3,133 fixed cases.

Linux execution and every broad Seed, OS, QEMU, Standard, Qualification, and
complete retirement gate remain pending. No maintained Stage 0 artifact was
produced in this slice.

## Reconsideration triggers

Split the hosted recipe module when native cross-module bindings accept a real
cohesive boundary. Reconsider this recipe if a generally useful WVA form replaces
the historical bytes or cross-host execution changes either retained WVO or EFI
identity.
