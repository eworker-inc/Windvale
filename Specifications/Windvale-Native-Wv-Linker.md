# Windvale native linker application

## Status and scope

`WVHL 1` packages the canonical Windvale-written `Wvˉlinkerˉcore` as paired Windows x64 and Linux x64 command-line applications. Digest-bound candidate launchers exist for both hosts, but they are not yet the ordinary front door: the C# CLI remains the normal and recovery linker until the exact candidate passes the independent Windows/Linux gate and is promoted.

The product logic remains in `Linker/Windvale/Wv-Linker-Core.wv`. It owns WVO admission, resolution, layout, relocation, independent image reconstruction, canonical-map construction, SHA-256 identities, and publish-after-success behavior. The package adds no second linker implementation.

## Construction contract

`Projects/Linker/Windvale-Wv-Linker.wvproj` is the exact source-to-WVB project. Its canonical module identity is `Wvˉlinkerˉcore`, its profile is `hosted`, and it exports exactly one `Main() -> i32`.

Decision 0501 reconstructs the exact candidate without asking either target
Wv-Linker application to link itself. The retained raw lowerer first produces
the exact WVO oracle. A distinct segmented staging, image-linking, and canonical
transport path then derives one raw fragment directly from the WVB, and the
native hosted-container toolset packages that fragment for both targets under
profile 4. The route consumes retained same-release native seeds; it is not a
clean bootstrap or previous-release renewal.

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

The Stage 0 `compile` and `aot` recovery commands independently verify the WVB, native fragment, bundle, metadata, runtime data, startup, and complete PE/ELF container before atomic executable publication. The Decision 0501 reconstruction instead writes exact products into a separate caller-owned directory; it is construction evidence, not an atomic installer or promotion transaction. The raw application accepts:

```text
wvlink-core <base-address> <entry> <output.bin> <input.wvo>...
```

One through 64 input objects are accepted. Success writes one canonical flat image, emits the complete canonical map with one final LF, and returns 0. A deterministic WVL rejection returns 2 and does not create or modify the output. Wrong argument count returns 64. Host input/output failure remains a runtime-boundary failure rather than a linker diagnostic. The retained one-argument object-inspection form and no-argument self-test are also part of the source module contract.

## Candidate identities and evidence

The current candidate identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Linker WVB | 135,740 | `02f727a8ce2d6826c8414cada0933c7d5a54893ea061621d08147984c3d6f874` |
| Raw-lowerer WVO oracle | 1,786,271 | `0141219773241e8780e2520f30ab8377914bf89a72f57da091871ac40d68a287` |
| Canonical linked fragment, `Main` at 884,630 | 1,777,781 | `d30e0c4dce7159bf98c546a0200e8b541797612ab67d6f21e3d8ee876af27480` |
| Windows linker | 1,796,608 | `08744f3cacf71280ea757dcdf6509ee3770d5536b08e5b3984a438cb6123fb78` |
| Linux linker | 1,798,144 | `8a220bfd6c7ef684897583e728419ecd6d383c8e8cf40094edbcfb695e3d6d7a` |

The focused reconstruction owner requires all five identities, reconstructs the
WVB, WVO oracle, fragment, and paired containers through the route above, and
runs the reconstructed current-host application on the fixed canonical
two-object input. That input must reproduce the complete frozen image and map
byte for byte. The separate linker rejection, hostile-input, map-limit, and
managed recovery suites retain their existing behavioral and differential
ownership. Independent Linux execution and grouped qualification remain.

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
129,387-byte canonical map. Decision 0441 isolates the old Windows failure as
exhaustion of the native runtime's 134,217,728-byte text/dynamic arena: 498
relocations caused the production and verifier paths to retain complete image
generations. The scale-safe candidate validates the established traversal
orders, emits patches in strictly ascending canonical placement order, and
reproduces the exact image at SHA-256
`76aa64cc03c8b86dfe96f83d761be40e8128b988a182fd971004a287a5990af0`
in 4.3 seconds on the current Windows host. This is current-host candidate
evidence, not Windows/Linux qualification or ordinary-path promotion.

## Qualification gate

Promotion to the ordinary linker front door requires one exact source commit to pass on Windows and Linux with:

- byte-identical linker WVB, image, and canonical map;
- independently reconstructed and verified format-7 packages;
- current-host self-test, valid-link, and rejected-link raw execution;
- no CLR/.NET module or mapping in the linker process; and
- no regression in WVO, Windvale Linking 1, native ABI, capability, or hosted-service contracts.

Decision 0302 already pins both platform applications behind digest-bound native launchers. Only an exact descendant containing those launchers, Decision 0325's expanded rejection matrix, and Decision 0327's map-limit boundary that passes both hosts moves ordinary linking to them. `windvale link` then remains the explicit Stage 0 recovery/differential command until Decision 0057's complete archive gate permits deletion.

Decision 0521 supplies independent Windows/Linux execution evidence for the
native-equivalent linker block through those digest-bound applications. Both
hosts pass the no-argument self-test with no output, accept the canonical WVO,
reject a non-WVO scanner input, consume the exact provider WVO, and publish the
identical 24-byte image plus 1,721-byte path-free canonical map. Undefined
imports return 2 without creating a new destination and preserve an existing
destination byte for byte. Hosted capability denial, missing-output-parent
resource failure, and live Stage 0 differential/oracle behavior remain outside
this transferred block.
