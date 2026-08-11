# Windvale native WVA assembler application

## Status and scope

`WVHS 1` packages the canonical Windvale-written `Wvaˉassemblerˉcore` as paired Windows x64 and Linux x64 command-line applications. Exact source commit `3aa5ba27f0ae4f96bc80d8bd521363015e884ab3` passes the independent Windows/Linux gate in GitHub [Verify run 31004212797](https://github.com/eworker-inc/Windvale/actions/runs/31004212797). Its pinned native-front-door artifacts and digest-bound launchers move ordinary WVA-to-WVO behavior to a .NET-free process once their exact containing commit passes the artifact promotion gate, while preserving the frozen C# assembler as a named Stage 0 recovery and differential oracle until the complete retirement gate is qualified.

The product logic remains in `Assembler/Windvale/Wva-Assembler-Core.wv`. It owns strict UTF-8 admission, WVA scanning and semantics, exact diagnostics, object measurement, x86-64 instruction encoding, WVO construction, and the single final output call. The package adds no second assembler implementation.

## Construction contract

`Windvale-Wva-Assembler.wvproj` is the exact source-to-WVB project. Its canonical module identity is `Wvaˉassemblerˉcore`, its profile is `hosted`, and it exports exactly one `Main() -> i32`.

The native writer accepts only that identity and one exported `Main`. Its fragment must require these services in this exact order:

1. `console.write_line`;
2. `process.argument_count`;
3. `process.argument`;
4. `file.read_bytes`;
5. `text.utf8_is_valid`;
6. `diagnostic.write_line`;
7. `text.concat`;
8. `u32.format`;
9. `file.write_bytes`.

The application bundle retains `enum.name` between diagnostics and concatenation as an internal tenth slot. This preserves the already-bounded compiler-authority runtime and startup layout without giving the Windvale fragment another dependency or capability. The slot is not part of the fragment's accepted service set.

The six declared capabilities are `console.write_line`, `diagnostic.write_line`, `file.read_bytes`, `file.write_bytes`, `process.argument`, and `process.argument_count`. A different module identity, capability set, fragment service set, entry shape, runtime profile, bundle, or outer target is rejected before publication.

## Container and targets

The metadata magic is `WVHS`, format version is 1, profile number is 3 in the shared hosted-compiler family, profile flags are 4, and the outer container format is 6. The public targets are:

- `windows-x64-wva-assembler-v1`, producing `.exe`;
- `linux-x64-wva-assembler-v1`, producing `.elf` and exact executable mode on Linux.

Both targets reuse the existing compiler-authority process entry, argument capture, bounded file adapters, runtime state, and service leaves. No new platform startup assembly is added by this profile. Assembly remains limited to the unavoidable process/ABI/syscall boundary; all WVA and WVO meaning stays in Windvale source.

The Stage 0 `compile` and `aot` commands independently verify the WVB, native fragment, bundle, metadata, runtime data, startup, and complete PE/ELF container before atomic executable publication. The raw application then accepts exactly:

```text
wvasm <source.wva> <output.wvo>
```

Successful input writes one canonical WVO, emits the stable two-line `wvasm 1` report, and returns 0. Rejected WVA returns 2 and does not create or modify the requested output. Wrong argument count returns 64. Host input/output failure remains a runtime-boundary failure rather than an assembler diagnostic.

## Candidate identities and evidence

The current candidate identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Assembler WVB | 180,071 | `a50e261fb690b1b2836b7b05da2d94ec7f023ef531ddd2432fc6a9001ae7049c` |
| Windows assembler | 2,895,360 | `e03a1f22317fef36213d14a0a669b262f81143a54cbe334da075901987268ed4` |
| Linux assembler | 2,895,872 | `ebe18959f2a057db5181f4e2bbf7979fac9359d50542581b63da6dc48c4163a0` |

These are immutable qualified front-door identities, not expected output from
every later Stage 0 backend revision. Decision 0520 keeps them under the
manifest, launcher, and front-door owner while current recovery-writer tests
prove repeatability, independent application verification, CLI equality,
accepted/rejected execution, and absence of CLR loading. The same digest-bound
applications now construct the canonical WVO inspection fixture for both broad
Seed scripts.

The focused candidate test reconstructs both containers, checks the exact authority and service bundle, exercises the public AOT target, and runs the current-host raw application. Canonical input must produce the independently verified 218-byte WVO with SHA-256 `992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85`; malformed input must leave output absent. Current-host module or mapping inspection must find no CLR/.NET runtime.

Decision 0321 adds a separate fixed rejection-family command over the ordinary
digest-bound launcher. It covers `WVA1001` through `WVA1011` with exact input and
report identities plus preservation of an existing destination; the oversized
case is generated temporarily rather than retained as a very large source file.
Decision 0336 adds a separate fixed 200-case seeded-mutation command with exact
Stage 0 acceptance, rejection-code, and successful WVO-byte agreement. These
focused contracts do not replace remaining representative valid-source vectors,
arbitrary-source containment, or final dual-host qualification.

The C# Stage 0 oracle supplies differential evidence during this candidate stage. After the candidate is pinned and passes the same exact commit on Windows and Linux, normal tests use fixed WVA/WVO vectors, structural WVO assertions, malformed-input outcomes, and deterministic artifact identities. The transferred seeded-mutation lane already requires no live C# result generator. Stage 0 then remains only in named recovery, differential, and final archive qualification lanes.

## Qualification gate

Promotion to the ordinary assembler front door requires one exact commit to pass on Windows and Linux with:

- byte-identical assembler WVB and canonical WVO;
- independently reconstructed and verified format-6 packages;
- current-host valid and malformed raw execution;
- no CLR/.NET module or mapping in the assembler process;
- digest-bound launchers and manifest entries for both platform applications; and
- no regression in the accepted WVA, WVO, native ABI, or hosted service contracts.

`Tools/Native/Assemble-Wva.cmd` and `Tools/Native/Assemble-Wva.sh` are the ordinary front-door commands after their exact artifact commit passes that gate. `windvale assemble` remains the explicit Stage 0 recovery/differential command; it is not deleted until Decision 0057's complete archive gate passes.
