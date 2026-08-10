# Windvale native WVB-to-WVO application

## Status and scope

`WVHN 1` packages the canonical Windvale-written `Compilerˉnativeˉx64ˉloweringˉtool` as paired Windows x64 and Linux x64 command-line applications. It lowers only the exact metered scalar/control/direct-call and bounded static-data subset defined by the [Windvale-native x86-64 lowering contract](Windvale-Native-X64-Lowering.md). Decision 0304 pins exact candidate artifacts, fixed vectors, and digest-bound launchers; Decision 0308 composes those launchers with native atomic WVO publication; and Decision 0497 reconstructs the current WVB and both target applications through the retained segmented native toolset. This remains neither the complete native backend nor an ordinary front door until independent Linux reconstruction and execution, the grouped Windows/Linux gate, and promotion succeed.

The portable core owns WVB admission, control and type verification, ABI-22 selection, branch/call measurement and patching, and canonical WVO 1.0 construction. The application adds no second selector, object writer, or target-specific lowering logic.

## Construction contract

`Windvale-Native-X64-Lowering-Tool.wvproj` is the exact source-to-WVB project. Its root is `Compiler/Windvale/Native-X64-Lowering-Tool.wv`, its canonical module identity is `Compilerˉnativeˉx64ˉloweringˉtool`, its profile is `hosted`, and it exports exactly one `Main() -> i32`.

The native writer accepts only that identity and entry. Its fragment requires these services in this exact order:

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

The fragment and application bundle now share the established ten-service compiler-authority layout; the lowerer's bounded data reader directly requires strict UTF-8 validation. The six declared capabilities are `console.write_line`, `diagnostic.write_line`, `file.read_bytes`, `file.write_bytes`, `process.argument`, and `process.argument_count`.

A different module identity, capability set, fragment or bundle service set, entry shape, runtime profile, metadata, or outer target is rejected before publication.

## Command behavior

The raw application accepts:

```text
wvnative <input.wvb> <output.wvo>
```

It reads the input once, invokes `Compilerˉlowerˉwvbˉnativeˉx64`, and makes exactly one `file.write_bytes` call only after valid complete WVO construction. Success prints the ABI, native code bytes, and object bytes with one final LF and returns 0. Invalid or unsupported WVB prints `native x64 status=<status>` to diagnostics, returns 1, and does not call output. Wrong argument count prints usage and returns 2. Host read or write failure remains a runtime-boundary failure. Atomic durable replacement is a separate launcher/publication responsibility.

## Container and targets

The metadata magic is `WVHN`, format version is 1, profile number is 6 in the shared hosted-compiler family, profile flags are 7, and the outer container format is 9. The public targets are:

- `windows-x64-wvb-to-wvo-v1`, producing `.exe`;
- `linux-x64-wvb-to-wvo-v1`, producing `.elf` and exact executable mode on Linux.

Both targets reuse the compiler-authority process entry, argument capture, bounded file adapters, runtime state, service leaves, and platform containers. No new WVA or platform assembly is added. The retained segmented native staging, linking, transport, and hosted-packaging path constructs the current paired containers; Stage 0 remains the independent complete-backend, recovery, and differential oracle.

## Candidate identities and fixed vector

The current candidate identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVB-to-WVO tool WVB | 416,464 | `8ce81d1c657f558f66b5b503d0edabad371b1f601297892f0d139e06176bdc27` |
| Windows WVB-to-WVO tool | 5,997,056 | `884bc29e8ae1ddc5e892f01729d1eab7443cc32d92cbdc37a1587b642c1cdc14` |
| Linux WVB-to-WVO tool | 5,996,544 | `55df0cd2a679491bc1a3da50b6bc67b5f512f2a4698721faea6becb556a2a46f` |

These pinned candidate applications include Decision 0419's parameterless
`Main() -> bytes` contract and Decision 0423's compiler-scale admission work.
Decision 0497 uses the current Windows native source front door plus the
retained segmented staging, linking, transport, and hosted-packaging toolset to
reconstruct this exact 414,298-byte WVB and both exact target applications in a
separate output directory. The constructed Windows application then reproduces
the fixed WVO below. This removes Stage 0 as the only constructor of the current
candidate generation, but it consumes an already retained native toolset and
therefore is not a non-circular bootstrap. Decisions 0420 and 0422 preserve
independent Windows and Debian reconstruction and execution evidence for an
earlier lowerer generation; the identities above still require independent
Linux reconstruction and execution before promotion. Ordinary consumers
continue to use digest-bound retained WVOs until paired promotion qualifies the
current candidate applications.

`Windvale-Native-Test-Wvb-To-Wvo-Return-42.wvproj` produces the fixed accepted-subset input:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Return-42 WVB | 174 | `7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31` |
| Return-42 WVO | 479 | `0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5` |

The WVO contains 406 code bytes, one exported `Main`, no data, imports, or relocations, and is admitted by the independent WVO parser. The current-host native application must reproduce it byte for byte, repeat deterministically, reject a truncated WVB without changing a sentinel output, and load no CLR/.NET runtime. Decision 0228 adds exact Stage 0 WVO agreement for the existing three-function `Add -> Build -> Main` fixture. Decision 0231 adds the canonical 493-byte `Sum-Data.wv` differential vector with exact `.text`, `.rodata`, data symbol, and relative relocation. Decision 0232 adds arbitrary exported-Main order plus bounded forward, recursive, and cyclic scalar calls under the existing instruction and call-depth budgets. Decision 0233 adds the compiler-produced `Function-Only.wv` vector with `u8`/`u32` locals and operations plus a Boolean helper return; the Windvale memory adapter, hosted tool, and generated native tool reproduce Stage 0's exact 6,041-byte `.text` and 6,216-byte WVO. Decision 0234 expands the separate `u8`/`u32` helper-return vector across all eight bounded comparisons and reproduces its exact 5,263-byte `.text` and 5,404-byte WVO through the hosted lowerer. Decisions 0235 through 0240 add exact descriptor, dynamic text/bytes, enum, direct-record, and record-call agreement. Decision 0241 adds the complete compiler-produced `Nominal-Types.wv` vector with multi-block record liveness and a scalar-returning record consumer; its 22,404-byte WVO is reproduced by the Windvale adapters and the direct current-host native package. None of those extensions changes the fixed return-42 vector.

## Qualification and retirement boundary

Promotion requires one exact source commit to pass on Windows and Linux with:

- byte-identical tool WVB and deterministic platform packages;
- independently reconstructed and verified format-9 containers with profile 6;
- exact service and capability agreement;
- direct current-host success, deterministic repetition, usage, and malformed-input execution;
- exact fixed-vector WVO plus the retained differential corpus;
- no CLR/.NET module or mapping in the lowerer process; and
- no regression in WVB 1.11, WVO 1.0, ABI 22, metering, control-flow, or call contracts.

Decision 0225 composes this source candidate with the qualified native source builder and the linker/packager candidates into one exact current-host source-to-executable proof. That proof does not promote this application. Decision 0304 pins both platform applications, a native-produced fixed vector, and digest-bound candidate launchers. Decision 0308 makes those launchers construct privately and publish through the shared portable WVO verifier plus native transaction. Decision 0317 fixes malformed-WVB and valid-but-unsupported-function rejection through the public launcher, with exact diagnostics, output preservation, and isolated-work cleanup. Decision 0497 closes native construction for this exact current candidate on the current Windows host without qualifying the Linux application, proving a clean bootstrap, or promoting either launcher. Only an exact descendant that passes independent reconstruction and execution on both hosts moves accepted-subset WVB-to-WVO lowering to the ordinary native launcher. C# remains the complete recovery/differential backend and the normal route for every unsupported module until later Windvale-owned backend slices close those gaps. Decision 0057's complete gate still controls final deletion.
