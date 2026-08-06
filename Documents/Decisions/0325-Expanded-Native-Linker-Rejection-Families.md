# Decision 0325: Expanded native linker rejection families

- Date: 2026-08-06
- Status: Implemented current-host evidence; Linux execution pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0311](0311-Fixed-Native-Linker-Rejections.md), and [Decision 0322](0322-Fixed-Native-Wvo-Read-Only-Rejection-Families.md)
- Contract: [Native Windvale linker](../../Specifications/Windvale-Native-Wv-Linker.md#fixed-native-rejection-contract)

## Context

Decision 0311 fixed the native linker's invalid-request, malformed-object, and
missing-entry outcomes. The retained managed suite still owned duplicate export,
aggregate limit, import resolution, layout, and relocation overflow evidence
even though the digest-bound native linker already implements those boundaries.

The linker's `WVL1011` status is an internal independent-reconstruction trap,
not an externally selectable malformed-link family. `WVL1012` is externally
reachable only through the bounded canonical-map size limit and needs a large
definition/map construction with many symbols or long names. Treating either as
an ordinary compact fixture would hide its distinct evidence needs or add an
unnecessarily large source file.

## Decision

- Expand `Test-Linker-Rejections.cmd` / `.sh` from three to ten ordered cases,
  covering every externally driven `WVL1001` through `WVL1010` family through
  the existing digest-bound launcher.
- Add five compact reviewable WVA sources and their exact WVO fixtures. Three
  WVOs are exact assembler outputs; the absolute and relative overflow WVOs
  change only the declared four-byte relocation addend to `2147483647`.
- Trigger aggregate overflow by passing one 64-section 1,560-byte object five
  times. Do not commit a multiplied aggregate object or a large generated source.
- Require exit `2`, empty standard output, exact complete reports, and
  byte-for-byte preservation of the existing 479-byte destination sentinel.
- Keep `WVL1011` owned by the linker's internal self-test and independent image
  corruption evidence. Design `WVL1012` as a separate bounded map-limit slice.

## Evidence and consequences

- The five new WVO fixtures are 77 through 1,560 bytes; their source, decoded
  sizes, identities, and the ten complete report identities are normative in
  the linked contract.
- Direct Windows execution passes 10/10 in 2.9 seconds. After reviewing the
  merged wrapper and exact report, the focused selection
  `native linker rejections preserve existing output without .NET` passes 1/1
  in 2.533 test seconds after a 9.19-second zero-warning Release build; the
  complete command takes 15.1 seconds.
- The permanent command invokes no .NET process, rebuilds no linker or fixture,
  and does not repeat a successful link or the AOT chain. No product
  implementation, candidate artifact, WebAssembly implementation, linking
  semantic, or WVO format byte changed.
- Linux execution of this exact matrix, the separate `WVL1012` boundary, and the
  grouped end-of-goal gate remain. This proof does not promote the linker or
  remove its Stage 0 recovery route.

## Reconsideration triggers

Add a fixed case only when it represents a distinct observable or security
boundary. Keep reconstruction mismatch injection and map-size construction in
their focused owners rather than weakening the public matrix or committing a
large numbered fixture.
