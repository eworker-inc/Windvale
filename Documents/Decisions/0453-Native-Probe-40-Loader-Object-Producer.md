# Decision 0453: Native Probe 40 loader-object producer

- Status: Implemented current-host native-build candidate; Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0452](0452-Native-Probe-40-Memory-Object-Producer.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contracts: [OS loader-object producer](../../Specifications/Windvale-Os-Loader-Object-Producer.md), [OS Probe object producer](../../Specifications/Windvale-Os-Probe-Object-Producer.md), and [WVO object construction](../../Specifications/Windvale-Wvo-Object-Construction.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The ordinary Probe 40 build still consumed the 6,336-byte frozen Stage 0
`00-loader.wvo`. Its 6,115-byte x64 code section validates UEFI tables, obtains
the memory map, exits boot services, constructs the kernel handoff, emits serial
evidence, and calls two relocated imports.

Today’s WVA surface can express much of that behavior but cannot reproduce the
exact established encoding: the loader uses a 32-bit debug-port output and
short stack-memory forms outside the current canonical WVA encodings. Expanding
WVA and changing the final EFI belongs to a separate semantic and boot-evidence
decision. Embedding all code as decimal literals would create an unnecessarily
large, hard-to-review source file.

## Decision

- Retain the exact normal loader code as one digest-pinned 6,115-byte
  architecture fixture. This is reviewed machine input, not portable source.
- Add a 75-line hosted Windvale producer that reads only that explicit fixture,
  constructs the exact section, symbols, and relocations through the shared
  verified WVO constructor, and rejects any unexpected size.
- Dispatch the closed `loader` selector to its dedicated package behind the
  unified `Produce-Os-Probe-Object` launchers. Bind both the package and code
  fixture identities before execution.
- Generate and independently admit `00-loader.wvo` inside the ordinary Probe 40
  private work directory, then remove it from the frozen seed.
- Expand the producer lane from eight to nine cases. Keep the scenario-aware C#
  generator only as frozen recovery/differential evidence.

## Evidence and consequences

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Loader code fixture | 6,115 | `19008f698db52c206dae920cf57ca4461eb009d47d8ecba258d6b021b05a2eed` |
| Producer WVB | 36,009 | `427ffcdaf7e9656f7bc17584de06b7954fddd38266663b295151d5a054f020d5` |
| Windows x64 application | 387,072 | `1ce2a2e3dd84d5af9a614b06382226c105e6051ba07d205a66c6d47e8d0e373c` |
| Linux x64 application | 389,120 | `616cc30cdd6c46dba15ead2dc7881f4ce53df187e485939337cfd0c5a540dc42` |

Current-host execution reproduces the former seed object byte for byte at
6,336 bytes and SHA-256
`b310bc0e9aebc7b14c0892bb3dd4b833d42539c2194427a8f333b511d6af3804`.
After affected-test review, the producer lane passes 9/9 in 2.0 seconds and the
normal `os-probe` lane passes 2/2 in 11.9 seconds. The final EFI remains 683,008
bytes at SHA-256
`080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9`.

The frozen seed now contains three WVOs totaling 654,422 bytes. Eight ordinary
objects come from Windvale-native producers totaling 38,228 bytes, three more
come from native WVA, and the fourteen-object link order remains unchanged. The
retirement plan remains 2,331 bytes at SHA-256
`9e07c88506f1ad6c97c12c5a016b456eab6fba179b245d1d2f207aeb975eb39d`
and contains 28 suites with 3,136 fixed cases.

Linux execution and every broad Seed, OS, QEMU, Standard, Qualification, and
complete retirement gate remain pending. No maintained Stage 0 artifact was
produced in this slice.

## Reconsideration triggers

Translate the fixture into WVA when the missing exact instructions and encoding
policy have independent consumers and the resulting EFI can receive focused
boot evidence. Do not grow WVA merely to preserve this historical byte stream.
