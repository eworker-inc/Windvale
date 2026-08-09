# Windvale OS loader-object producer

## Status and scope

This hosted Windvale tool constructs the exact normal-scenario Probe 40
`00-loader.wvo` from one digest-pinned x86-64 code fixture. It replaces normal
Stage 0 object construction without claiming that the UEFI loader machine code
has been translated into WVA or portable Windvale source.

The public entry points remain the unified OS Probe object launchers:

```text
Tools/Native/Produce-Os-Probe-Object.cmd loader <output.wvo>
Tools/Native/Produce-Os-Probe-Object.sh loader <output.wvo>
```

They bind the exact native application and `normal-x64-loader.bin`, require a
new `.wvo` destination in an existing directory, remove a newly created invalid
result, and require the complete output identity.

## Object contract

The code fixture is 6,115 bytes at SHA-256
`19008f698db52c206dae920cf57ca4461eb009d47d8ecba258d6b021b05a2eed`.
It contains the normal Probe 40 UEFI memory-map acquisition,
`ExitBootServices`, serial diagnostics, handoff construction, kernel entry, and
terminal paths.

The resulting object is 6,336 bytes at SHA-256
`b310bc0e9aebc7b14c0892bb3dd4b833d42539c2194427a8f333b511d6af3804`.
It contains one 6,115-byte `.text` section aligned to 16 bytes, exports
`Windvale_boot_probe`, and imports `Windvale_kernel_entry` and
`Windvale_kernel_x64_q35_shutdown`. Relative-i32 relocations at offsets 2,782
and 5,526 target those imports with addend `-4`.

The producer constructs the WVO through the shared portable constructor, which
independently admits the complete result before publication. The launcher also
pins the code fixture and complete object digest, so a changed resource cannot
silently become a new loader contract.

## Retained package identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `normal-x64-loader.bin` | 6,115 | `19008f698db52c206dae920cf57ca4461eb009d47d8ecba258d6b021b05a2eed` |
| `Os-Probe-Loader-Object-Producer.wvb` | 36,009 | `427ffcdaf7e9656f7bc17584de06b7954fddd38266663b295151d5a054f020d5` |
| Windows x64 application | 387,072 | `1ce2a2e3dd84d5af9a614b06382226c105e6051ba07d205a66c6d47e8d0e373c` |
| Linux x64 application | 389,120 | `616cc30cdd6c46dba15ead2dc7881f4ce53df187e485939337cfd0c5a540dc42` |

The 75-line producer keeps object structure explicit without duplicating 6,115
decimal byte literals in source. The scenario-aware C# generator remains frozen
recovery/differential evidence until the final retirement gate; the user-fault
marker variant is not claimed by this normal-image candidate.
