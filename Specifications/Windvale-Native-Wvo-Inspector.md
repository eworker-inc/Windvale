# Windvale native WVO inspector application

## Status and scope

The shared hosted-verifier container packages the canonical Windvale-written `Wvoˉobjectˉcore` as paired Windows x64 and Linux x64 read-only command-line applications. This source candidate is not yet the ordinary front door: `windvale object-verify` and `windvale object-inspect` remain Stage 0 commands until the grouped final Windows/Linux retirement gate and a later pinned-artifact promotion succeed.

The product logic is split across two cohesive object-model modules. `Wvo-Object-Verification.wv` owns complete portable WVO admission and malformed-input status; `Wvo-Object-Core.wv` owns deterministic verification and inspection reports, SHA-256 identity, the hosted shell, and self-test. The native WVO publisher reuses the first module. The package adds no second object parser or platform-specific WVO logic.

## Construction contract

`Windvale-Wvo-Object.wvproj` is the exact source-to-WVB project. Its canonical module identity is `Wvoˉobjectˉcore`, its profile is `hosted`, and it exports exactly one `Main() -> i32`.

The native writer accepts only that identity and one exported `Main`. Its fragment and application bundle require these services in this exact order:

1. `console.write_line`;
2. `process.argument_count`;
3. `process.argument`;
4. `file.read_bytes`;
5. `text.utf8_is_valid`;
6. `diagnostic.write_line`;
7. `enum.name`;
8. `text.concat`;
9. `text.quote`;
10. `i32.format`;
11. `u32.format`.

The five declared capabilities are `console.write_line`, `diagnostic.write_line`, `file.read_bytes`, `process.argument`, and `process.argument_count`. There is no file-output service, output scratch region, or write capability. A different module identity, capability set, service set, entry shape, runtime profile, bundle, or outer target is rejected before publication.

## Container and targets

The application uses profile number 6 in the shared hosted-verifier metadata and reuses outer container format 4. The public targets are:

- `windows-x64-wvo-inspector-v1`, producing `.exe`;
- `linux-x64-wvo-inspector-v1`, producing `.elf` and exact executable mode on Linux.

Both targets reuse the existing read-only inspector startup, argument capture, bounded file snapshot, runtime state, and service leaves. No new platform startup assembly is added. Assembly remains limited to the unavoidable process/ABI/syscall boundary; all WVO meaning stays in Windvale source.

The Stage 0 `compile` and `aot` commands independently verify the WVB, native fragment, bundle, metadata, runtime data, startup, and complete PE/ELF container before atomic executable publication. The raw application accepts:

```text
wvo-object-core verify <object.wvo>
wvo-object-core inspect <object.wvo>
```

Success returns 0. Structurally invalid WVO returns 2 after a deterministic diagnostic. Wrong command or argument count returns 64. Host input failure remains a runtime-boundary failure rather than a WVO diagnostic. No arguments run the internal deterministic self-test.

## Candidate identities and evidence

The current candidate identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVO inspector WVB | 60,974 | `b0d0568cb6861c84ea9cad0b77f9722a9141b30c94952e5662aaa3afc47eae0f` |
| Windows WVO inspector | 606,720 | `2a8f6f8ca8fc6054fff23441f7971c0b90900383d5bed0fecc54f9cac102a300` |
| Linux WVO inspector | 606,208 | `bdc4817c252ecf2592299a6646161b396bfb251acabc68d3f5d75ff40891541e` |

The focused candidate test reconstructs and independently parses both containers, checks exact capabilities and services, exercises the public current-host AOT target, and runs the current-host raw application. It compares complete successful `verify` and `inspect` output with the Stage 0 oracle during candidate qualification, checks malformed and usage outcomes, and inspects loaded modules or mappings for CLR/.NET.

## Qualification gate

Promotion to the ordinary WVO read-only front door requires the final grouped retirement commit to pass on Windows and Linux with:

- byte-identical WVB and deterministic platform packages;
- independently reconstructed and verified format-4 containers with profile 6;
- current-host self-test, verify, inspect, malformed-input, and usage execution;
- stable WVO vectors and structural assertions agreeing with the frozen oracle;
- no CLR/.NET module or mapping in the WVO process; and
- no regression in WVO 1.0, native ABI, capability, or hosted-service contracts.

After that source gate, pin both platform applications and add digest-bound native launchers in a separate provenance commit. Only the exact pinned-artifact commit passing both hosts moves ordinary WVO verification and inspection to those launchers. The C# commands remain named recovery/differential paths until Decision 0057's complete archive gate permits deletion.
