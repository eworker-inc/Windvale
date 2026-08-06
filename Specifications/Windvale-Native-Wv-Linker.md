# Windvale native linker application

## Status and scope

`WVHL 1` packages the canonical Windvale-written `Wvˉlinkerˉcore` as paired Windows x64 and Linux x64 command-line applications. This source candidate is not yet the ordinary front door: the C# CLI remains the normal and recovery linker until the exact candidate and a later pinned-artifact promotion both pass the independent Windows/Linux gate.

The product logic remains in `Linker/Windvale/Wv-Linker-Core.wv`. It owns WVO admission, resolution, layout, relocation, independent image reconstruction, canonical-map construction, SHA-256 identities, and publish-after-success behavior. The package adds no second linker implementation.

## Construction contract

`Windvale-Wv-Linker.wvproj` is the exact source-to-WVB project. Its canonical module identity is `Wvˉlinkerˉcore`, its profile is `hosted`, and it exports exactly one `Main() -> i32`.

The native writer accepts only that identity and one exported `Main`. Its fragment and application bundle must require these services in this exact order:

1. `console.write_line`;
2. `process.argument_count`;
3. `process.argument`;
4. `file.read_bytes`;
5. `text.utf8_is_valid`;
6. `diagnostic.write_line`;
7. `enum.name`;
8. `text.concat`;
9. `u32.format`;
10. `file.write_bytes`.

The six declared capabilities are `console.write_line`, `diagnostic.write_line`, `file.read_bytes`, `file.write_bytes`, `process.argument`, and `process.argument_count`. A different module identity, capability set, fragment service set, entry shape, runtime profile, bundle, or outer target is rejected before publication.

Signed map values are formatted in Windvale from their raw two's-complement `u32` bits. Values through `2147483647` use `U32ˉformat`; larger values emit `-` plus the checked magnitude. This covers `-2147483648` without adding `I32ˉformat` or changing the shared ten-service startup.

## Container and targets

The metadata magic is `WVHL`, format version is 1, profile number is 4 in the shared hosted-compiler family, profile flags are 5, and the outer container format is 7. The public targets are:

- `windows-x64-wv-linker-v1`, producing `.exe`;
- `linux-x64-wv-linker-v1`, producing `.elf` and exact executable mode on Linux.

Both targets reuse the existing compiler-authority process entry, argument capture, bounded file adapters, runtime state, and service leaves. No new platform startup assembly is added by this profile. Assembly remains limited to the unavoidable process/ABI/syscall boundary; all WVO and linking meaning stays in Windvale source.

The Stage 0 `compile` and `aot` commands independently verify the WVB, native fragment, bundle, metadata, runtime data, startup, and complete PE/ELF container before atomic executable publication. The raw application accepts:

```text
wvlink-core <base-address> <entry> <output.bin> <input.wvo>...
```

One through 64 input objects are accepted. Success writes one canonical flat image, emits the complete canonical map with one final LF, and returns 0. A deterministic WVL rejection returns 2 and does not create or modify the output. Wrong argument count returns 64. Host input/output failure remains a runtime-boundary failure rather than a linker diagnostic. The retained one-argument object-inspection form and no-argument self-test are also part of the source module contract.

## Candidate identities and evidence

The current candidate identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Linker WVB | 127,482 | `592467003974dab240e1f90b5a647d360cfd4cc6d7186bfdedbcc3ba8788f386` |
| Windows linker | 1,655,296 | `ca88735061d7e36e79813346621a867a9293d04d3c01ffb0336f4ee32cbe316d` |
| Linux linker | 1,654,784 | `994f27f5a2449990b767c0ed8c8c367e2676d41d652ee9a61eab1de36de82dc2` |

The focused candidate test reconstructs both containers, checks exact capabilities and services, exercises the public AOT target, and runs the current-host raw application. Canonical two-object input must reproduce the complete Stage 0 image and map byte for byte, including signed addend output and all Windvale-computed SHA-256 values. Invalid WVO must preserve existing output. Current-host module or mapping inspection must find no CLR/.NET runtime.

## Fixed native rejection contract

`Tools/Native/Test-Linker-Rejections.cmd` and `.sh` exercise only the pinned
linker launcher and repository-owned WVO fixtures. They do not rebuild the
source, repeat the successful AOT chain, invoke .NET, or consult a live Stage 0
oracle. The decoded canonical input is 479 bytes at SHA-256
`0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5`;
the malformed input and output sentinel are 479 bytes at SHA-256
`0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288`.

Each case must return `2`, write no standard output, preserve the complete
sentinel, and emit one LF-terminated diagnostic whose complete SHA-256 is:

| Case | Diagnostic | Report SHA-256 |
| --- | --- | --- |
| `invalid-base` | `WVL1001`, invalid unsigned base address | `b5a687af92c9eca7eb5ba850bddf6dec932c94a6be304af35357655a915056b8` |
| `missing-entry` | `WVL1007`, missing `Missing` entry | `883ad60b71d4c010d4a2ddf168199dfaae04d1e076313ee1cf4dac8bee67a517` |
| `malformed-object` | `WVL1002`, bad-magic input zero | `18eeeeb5d84e82c54cf14480bc5c54e593f5cd429d68686ad64110d9780a5353` |

Success prints the three ordered `PASS` lines followed by
`Tests: 3, Passed: 3, Failed: 0` plus LF. This is a bounded permanent fixture
set, not a replacement for the complete resolution, layout, relocation,
hostile-input, randomized, or concurrency corpus.

## Qualification gate

Promotion to the ordinary linker front door requires one exact source commit to pass on Windows and Linux with:

- byte-identical linker WVB, image, and canonical map;
- independently reconstructed and verified format-7 packages;
- current-host self-test, valid-link, and rejected-link raw execution;
- no CLR/.NET module or mapping in the linker process; and
- no regression in WVO, Windvale Linking 1, native ABI, capability, or hosted-service contracts.

After that source gate, pin both platform applications and add digest-bound native launchers in a separate provenance commit. Only the exact pinned-artifact commit passing both hosts moves ordinary linking to the launchers. `windvale link` then remains the explicit Stage 0 recovery/differential command until Decision 0057's complete archive gate permits deletion.
