# Windvale native WVO inspector application

## Status and scope

The shared hosted-verifier container packages the canonical Windvale-written `Wvoˉobjectˉcore` as paired Windows x64 and Linux x64 read-only command-line applications. Digest-bound native launchers own repository WVO checking, verification, and inspection. The frozen Stage 0 commands remain recovery/differential paths rather than normal dependencies.

The product logic is split across two cohesive object-model modules. `Wvo-Object-Verification.wv` owns complete portable WVO admission and malformed-input status; `Wvo-Object-Core.wv` owns deterministic verification and inspection reports, SHA-256 identity, the hosted shell, and self-test. The native WVO publisher reuses the first module. The package adds no second object parser or platform-specific WVO logic.

Decision 0520 makes the digest-bound applications own successful verification
and inspection of the canonical native-assembled WVO in both broad Seed
scripts. Decision 0522 repairs their incomplete enum-service bundle, refreshes
the candidate identities, and transfers no-argument self-test. Empty and
missing resource exits still do not match the typed reference-runtime reports,
so those calls remain explicit qualification gaps.

## Construction contract

`Projects/Object-Model/Windvale-Wvo-Object.wvproj` is the exact source-to-WVB project. Its canonical module identity is `Wvoˉobjectˉcore`, its profile is `hosted`, and it exports exactly one `Main() -> i32`.

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

The additional `check` form performs the same complete WVO admission without SHA-256 calculation or success output:

```text
wvo-object-core check <object.wvo>
```

Success returns 0. Structurally invalid WVO returns 2 after a deterministic diagnostic. Wrong command or argument count returns 64. Host input failure remains a runtime-boundary failure rather than a WVO diagnostic. No arguments run the internal deterministic self-test. `verify` and `inspect` retain digest reporting over their single admitted in-memory snapshot; `check` is used only when a caller does not consume that report.

## Candidate identities and evidence

The current candidate identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVO inspector WVB | 73,322 | `40f7b7efcff5b6e5bbc3c878cf5f0147ee92af208d43d54ab8a04f87ec1e9070` |
| WVO inspector WVO | 1,022,822 | `bab6b73e5edd6b0b2726380ba2ff10859fbbcc37481572457b508bbd0d67c2ae` |
| Linked inspector fragment | 1,017,780 | `1410b92ebc614f17cbf6e8a1147cb2cd448ae687a3b776e8d4ec3eb96a434854` |
| Windows WVO inspector | 1,037,312 | `5362372e826958470eee7d90eb01938de5b91dcb3e1b0f952722e00578a82d03` |
| Linux WVO inspector | 1,036,288 | `fcfd134222b05482a6ac432fc4acbfb72f3dfce92c3c646fc17595ddb078b840` |

[Decision 0500](../Documents/Decisions/0500-Native-Wvo-Inspector-Reconstruction.md)
adds the exact native source-to-WVB-to-WVO-to-paired-application route. Its
focused current-Windows-host owner passes all three inventory, reconstruction,
and compatibility/profile-isolation cases in 28.1 seconds. The Linux result is
cross-target construction evidence; independent Linux execution remains part
of the grouped gate. Decision 0522 advances that historical candidate to the
enum-complete candidate-3 identity. Native `wvhostenumrequest` produces a
945-byte request, `wvhostenumservice` produces the complete 1,244-byte
leaf-plus-metadata service, and both target reconstructions equal the current
Stage 0 writer byte for byte. Focused reconstruction and no-argument execution
pass independently on Windows and Linux.

Decision 0552 advances that candidate to the bounded-reporting candidate-4
identity. The report path composes the existing streaming Foundation SHA-256
modules, while the new structural-only command avoids hashing when callers
discarded the digest. The WVO format, native ABI 22, profile 6, startup, service
order, entry address, and validation rules remain unchanged.

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
