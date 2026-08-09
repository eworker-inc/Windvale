# Decision 0456: Native Probe 40 process object

- Status: Implemented current-host native-build candidate; Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0455](0455-Native-Probe-40-Process-Policy-Source-Path.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [OS process-object build](../../Specifications/Windvale-Os-Process-Object.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The final frozen input in the ordinary Probe 40 build was the 512,978-byte
normal process WVO. Only 46,678 bytes were architecture-specific process code.
The remaining 463,531 payload bytes were derived from canonical Windvale
service and interpreter sources, one canonical test program, WVA shims, and
versioned resource/directory encodings. Freezing the entire object therefore
hid a large generated payload behind Stage 0 even though the native compiler,
lowerer, assembler, linker, encoders, WVO verifier, and publisher already owned
the required boundaries.

A direct line-for-line port of the managed constructor would also create one
large catch-all source. The Windvale implementation instead needs explicit
owners for layout, relocations, resource-store encoding, directory snapshots,
boot-resource transformation, and hosted orchestration.

## Decision

- Retain only the reviewed `.text.process` bytes as a digest-bound architecture
  fixture. Preserve their derivation with a bounded Windvale extractor that
  validates the predecessor WVO before copying the section.
- Rebuild the init/resource, directory, and bytecode-interpreter modules from
  canonical Windvale sources, lower and rename their exports, assemble the four
  exact WVA shims, and link the three embedded images natively.
- Rebuild the admitted program, execution budget, resource store, and directory
  snapshot from canonical inputs.
- Construct the final eight-section, 25-symbol, 55-relocation WVO through
  focused Windvale layout and relocation modules, independently verify it, and
  publish it without replacing an existing destination.
- Bind the four hosted tools and architecture fixture in a paired Windows/Linux
  candidate toolset. Keep Linux execution for the final grouped gate.
- Remove `05-process.wvo` from the Probe object seed. The normal seed now owns
  zero frozen objects; Stage 0 remains only in the explicit recovery and
  differential lane.
- Add two focused cases for exact construction and destination preservation,
  then reuse the existing two-case Probe image lane for integration.

## Evidence and consequences

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process architecture fixture | 46,678 | `05938e22e02abac6d396fa5a64342d94609900a6401b112f18de0fb5421a41b5` |
| Init service WVB | 526 | `7cefa7dcf82ed05d6b6e133aa79b7da90372e2d8f8f993abe7449513398ede83` |
| Directory service WVB | 474 | `f7410595f9824e510da9399f52a463013ff41240b67308cdf28b4f5b7484ab2b` |
| Interpreter WVB | 56,307 | `e2024702919e9acd37c119a7afb9991a73904d97ef3bdb1defe8c5ea13e91a3d` |
| Linked init image | 5,159 | `e9624ebe3b857b77d8b1024a4edfdaf23e040ee61f9dfc484e590ce1e5aa18f0` |
| Linked directory image | 3,911 | `f4d047c6f311b1561a5621b98f3db2868a969c54bb81dac2f75d599b7207f3fb` |
| Linked client image | 449,261 | `be4f88ad2460a17e5902670a9ca2bf70021d8b5ce46e2414f00f940a8f4d32b6` |
| Final process WVO | 512,978 | `dff07c3f6a52dedf6bcd96181221cba50c831359502ec763ee77f6aaaaafdfaa` |

After affected-test review, `os-process-object` passes 2/2 in 17.8 seconds and
the integrated `os-probe` lane passes 2/2 in 29.9 seconds. The final EFI remains
683,008 bytes at SHA-256
`080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9`.

The Probe inventory now records eleven native-produced objects totaling 692,650
bytes and no frozen WVOs. The retirement plan is 2,558 LF-only bytes at SHA-256
`f8ab968d081a7ad053ab08d4414057f53b27a7bc653b2ee975ef15c2f6c2eae4`
and contains 31 suites with 3,147 fixed cases.

This closes the ordinary Probe object-seed transfer, not the complete .NET
retirement gate. A normal-path audit, independent Linux execution, the final
dual-host Decision 0057 qualification, and a digest-bound Stage 0 recovery
release remain. Broad Seed, OS, QEMU, Standard, Qualification, and complete
retirement gates did not run in this slice.

## Reconsideration triggers

Replace the architecture fixture when an accepted Windvale/WVA backend owns
that exact machine-code surface. Change the composed toolset only when one of
its versioned formats or canonical sources changes; do not merge the focused
modules merely to reduce the number of files.
