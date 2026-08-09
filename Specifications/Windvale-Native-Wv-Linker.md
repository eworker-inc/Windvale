# Windvale native linker application

## Status and scope

`WVHL 1` packages the canonical Windvale-written `Wvˉlinkerˉcore` as paired Windows x64 and Linux x64 command-line applications. Digest-bound candidate launchers exist for both hosts, but they are not yet the ordinary front door: the C# CLI remains the normal and recovery linker until the exact candidate passes the independent Windows/Linux gate and is promoted.

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
source or fixtures, repeat the successful AOT chain, invoke .NET, or consult a
live Stage 0 oracle. The decoded canonical input is 479 bytes at SHA-256
`0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5`;
the malformed input and output sentinel are 479 bytes at SHA-256
`0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288`.

Five compact WVA sources preserve fixture provenance. Their fixed WVO values
are:

| Fixture | Bytes | SHA-256 |
| --- | ---: | --- |
| `Many-Sections.wvo` | 1,560 | `09cad03b9bf0543db2dec815f3f20deff044f5226e9347314b8c4d9a9e1020f8` |
| `Unresolved-Import.wvo` | 126 | `569926307b578cd1bf90dfb2b3c70eeb4b5ec7eff8e638e83613e89463717617` |
| `Wrong-Kind-Provider.wvo` | 77 | `1276a484c52d48996a7d781121f85cab93ecde729cb6ce18dd7c77b4bdb98ce6` |
| `Absolute-Overflow.wvo` | 150 | `994bc31ed39548dbd9339e7b0d2ac9b58936250b3603f90e84bda51f74b8bb11` |
| `Relative-Overflow.wvo` | 125 | `4d6dcc8211e02399e8ba38fbbec94dcd11c15842efe09fd8af615e25b57d7a48` |

The first three are exact assembler outputs. The last two differ from their
source-built WVO only in the final relocation addend, which is fixed to signed
maximum `2147483647` to reach checked arithmetic rejection.

Each case must return `2`, write no standard output, preserve the complete
sentinel, and emit one LF-terminated diagnostic whose complete SHA-256 is:

| Case | Diagnostic | Report SHA-256 |
| --- | --- | --- |
| `invalid-base` | `WVL1001`, invalid unsigned base address | `b5a687af92c9eca7eb5ba850bddf6dec932c94a6be304af35357655a915056b8` |
| `malformed-object` | `WVL1002`, bad-magic input zero | `18eeeeb5d84e82c54cf14480bc5c54e593f5cd429d68686ad64110d9780a5353` |
| `aggregate-limit` | `WVL1003`, fifth 64-section input | `33ecb82d77ff1f307b60a18993edf46807a39bf66ab7091054fc9ee7ad04ef61` |
| `duplicate-export` | `WVL1004`, second `Main` export | `cd8c0a1c80784f3d6db68984fe07f9bcbc0657c12e548bd923efad7f2666c324` |
| `undefined-import` | `WVL1005`, unresolved `Missing` function | `448d3e4eb8053d1aca41ebcdcf61af3d8519f3fea033859f82eb95d63ac275e0` |
| `kind-mismatch` | `WVL1006`, function import resolved to data | `047bea593cba87e948ea03c3cee09c5b04879683a1eb5856b9d0d30f7f774441` |
| `missing-entry` | `WVL1007`, missing `Missing` entry | `883ad60b71d4c010d4a2ddf168199dfaae04d1e076313ee1cf4dac8bee67a517` |
| `layout-overflow` | `WVL1008`, maximum base with nonempty image | `9c393cdbef3dc4a6dbe28ae5ba0c77fc56166a84b30c845bee78475f2679912d` |
| `absolute-overflow` | `WVL1009`, target plus signed-maximum addend | `1867b048e4c725d2ea76f0ed0dd28b80f360fe07395d17ff62b743d5bc974b74` |
| `relative-overflow` | `WVL1010`, displacement plus signed-maximum addend | `d8a7ac5340b29066470b5656c840654221b508702cbc62ebfcecf7f36aa66e67` |

Success prints the ten ordered `PASS` lines followed by
`Tests: 10, Passed: 10, Failed: 0` plus LF. `WVL1011` remains an internal
independent-reconstruction trap rather than an externally driven family. This
bounded permanent set does not replace concurrency coverage. The separate
[native linker hostile-input contract](Windvale-Native-Linker-Hostile-Input-Tests.md)
now owns the fixed 200-value raw-byte containment corpus.

The [native WVO hostile-size contract](Windvale-Native-Wvo-Hostile-Size-Tests.md)
separately requires the first input beyond the 4-MiB snapshot limit to fail at
the host boundary while preserving both the input and existing link output.

## Fixed native map-limit contract

`Tools/Native/Test-Linker-Map-Limit.cmd` and `.sh` provide the separate
`WVL1012` boundary without rebuilding the linker, invoking .NET, or retaining a
large generated WVA source. The existing 479-byte canonical `Main` WVO is
followed by the same 4,096-local object three times and one 4,095-local object.
The five valid inputs therefore carry exactly 16,384 definitions.

The generated WVOs each contain one empty code section named `.text`, aligned
to one, followed by zero-sized local function definitions named in ascending
order from `L0000`. Their fixed identities are:

| Fixture | Symbols | Bytes | SHA-256 |
| --- | ---: | ---: | --- |
| `Map-Locals-4096.wvo` | 4,096 | 102,449 | `a05c4f51be960c7fc900d8cc9fc39dbc525ccd0b2b1a4c55b12ca8396107ee75` |
| `Map-Locals-4095.wvo` | 4,095 | 102,424 | `398737cfd465fb976e6319ce7ddc4dbefb9e082d39432d09474cf75f8aafffdc` |

They are stored in one 21,046-byte gzip tar archive at SHA-256
`1c6227931496f54c93677b4dfecfbfa256214a5da72ecfd05d441e49c809e27d`.
The command verifies the archive and both extracted identities before linking.
It must return `2`, write no standard output, preserve every input and the
existing output, and emit exactly this report plus LF:

```text
link status=WVL1012 inputs=5 sections=5 symbols=16384 relocations=0 image-bytes=0 entry-address=0 input=4294967295
```

The complete report SHA-256 is
`097ad88fa0e4fd48504da8d69516e47ff7f6b5979fccf186e0307b814b5af86e`.
Success prints `PASS  canonical-map-limit` followed by
`Tests: 1, Passed: 1, Failed: 0`, each with LF.

## Measured Probe 40 scale boundary

Decision 0440 supplies fourteen ordered standard-profile WVO inputs whose
managed differential result is a 681,913-byte image and a 663-line,
129,387-byte canonical map. The current retained v1 Windows container admits
the argument and object envelopes but exits through its generic native resource
mapping before publication; the failing internal phase is not yet isolated.
This is a candidate implementation-capacity gap,
not a change to WVO, layout, relocation, map, or diagnostic semantics. The
ordinary native linker is not qualified for this case until the same source
preserves the complete canonical map within its bounded runtime on Windows and
Linux.

## Qualification gate

Promotion to the ordinary linker front door requires one exact source commit to pass on Windows and Linux with:

- byte-identical linker WVB, image, and canonical map;
- independently reconstructed and verified format-7 packages;
- current-host self-test, valid-link, and rejected-link raw execution;
- no CLR/.NET module or mapping in the linker process; and
- no regression in WVO, Windvale Linking 1, native ABI, capability, or hosted-service contracts.

Decision 0302 already pins both platform applications behind digest-bound native launchers. Only an exact descendant containing those launchers, Decision 0325's expanded rejection matrix, and Decision 0327's map-limit boundary that passes both hosts moves ordinary linking to them. `windvale link` then remains the explicit Stage 0 recovery/differential command until Decision 0057's complete archive gate permits deletion.
