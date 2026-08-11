# Windvale native WVO inspector application

## Status and scope

The shared hosted-verifier container packages the canonical Windvale-written `Wvoˉobjectˉcore` as paired Windows x64 and Linux x64 read-only command-line applications. Digest-bound candidate launchers exist for both hosts, but they are not yet the ordinary front door: `windvale object-verify` and `windvale object-inspect` remain Stage 0 commands until the grouped final Windows/Linux retirement gate and pinned-artifact promotion succeed.

The product logic is split across two cohesive object-model modules. `Wvo-Object-Verification.wv` owns complete portable WVO admission and malformed-input status; `Wvo-Object-Core.wv` owns deterministic verification and inspection reports, SHA-256 identity, the hosted shell, and self-test. The native WVO publisher reuses the first module. The package adds no second object parser or platform-specific WVO logic.

Decision 0520 makes the digest-bound applications own successful verification
and inspection of the canonical native-assembled WVO in both broad Seed
scripts. The current candidate's no-argument native self-test returns 1 and its
empty/missing resource exits do not yet match the reference-runtime reports, so
those calls remain explicit qualification gaps rather than being counted as
transferred.

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
| WVO inspector WVB | 61,008 | `a630d49f0549c865644d8052fbff7e8bf2b6a6dcd013e1187d4356d49cd188db` |
| WVO inspector WVO | 591,723 | `f45b14c33a7615209a2a16f6caf0bee041bdb5e2f46fd868792222e774fdb30c` |
| Linked inspector fragment | 587,529 | `f318ee573b149aac169b67369e90dbacc6451fc129022bfb4e62b2ceff9cfba4` |
| Windows WVO inspector | 606,208 | `bb39e58d51e7b6c3eab2690995ee52fc958557ab03cfcbcb9b5ef0f3070157d2` |
| Linux WVO inspector | 606,208 | `bf94145cee63a4d7014bd7a31a40832017f025b7d8086a4ae3875385ba8345c1` |

[Decision 0500](../Documents/Decisions/0500-Native-Wvo-Inspector-Reconstruction.md)
adds the exact native source-to-WVB-to-WVO-to-paired-application route. Its
focused current-Windows-host owner passes all three inventory, reconstruction,
and compatibility/profile-isolation cases in 28.1 seconds. The Linux result is
cross-target construction evidence; independent Linux execution remains part
of the grouped gate.

The focused candidate test reconstructs and independently parses both containers, checks exact capabilities and services, exercises the public current-host AOT target, and runs the current-host raw application. It compares complete successful `verify` and `inspect` output with the Stage 0 oracle during candidate qualification, checks malformed and usage outcomes, and inspects loaded modules or mappings for CLR/.NET.

Decision 0322 adds a separate fixed rejection-family matrix over the digest-bound
launchers. All thirteen stable WVO 1.0 status families require exit `2`, empty
standard output, identical exact reports from `Verify-Wvo` and `Inspect-Wvo`,
and byte-for-byte preservation of the input. These fixed identities replace a
live managed oracle for that permanent boundary; randomized and hostile-size
coverage remains independent recovery evidence until the final retirement gate.

## Qualification gate

Promotion to the ordinary WVO read-only front door requires the final grouped retirement commit to pass on Windows and Linux with:

- byte-identical WVB and deterministic platform packages;
- independently reconstructed and verified format-4 containers with profile 6;
- current-host self-test, verify, inspect, malformed-input, and usage execution;
- exact dual-launcher agreement for every stable WVO rejection family;
- stable WVO vectors and structural assertions agreeing with the frozen oracle;
- no CLR/.NET module or mapping in the WVO process; and
- no regression in WVO 1.0, native ABI, capability, or hosted-service contracts.

Decision 0301 already pins both platform applications behind digest-bound native launchers. Only an exact descendant containing those launchers and Decision 0322's fixed rejection matrix that passes both hosts moves ordinary WVO verification and inspection to them. The C# commands remain named recovery/differential paths until Decision 0057's complete archive gate permits deletion.
