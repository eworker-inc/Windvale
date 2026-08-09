# Windvale native WVB-to-WVO application

## Status and scope

`WVHN 1` packages the canonical Windvale-written `Compilerˉnativeˉx64ˉloweringˉtool` as paired Windows x64 and Linux x64 command-line applications. It lowers only the exact metered scalar/control/direct-call and bounded static-data subset defined by the [Windvale-native x86-64 lowering contract](Windvale-Native-X64-Lowering.md). Decision 0304 pins exact candidate artifacts, fixed vectors, and digest-bound launchers; Decision 0308 composes those launchers with native atomic WVO publication. This remains neither the complete native backend nor an ordinary front door until native host-container construction, the grouped Windows/Linux gate, and promotion succeed.

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

Both targets reuse the compiler-authority process entry, argument capture, bounded file adapters, runtime state, service leaves, and platform containers. No new WVA or platform assembly is added. Stage 0 independently verifies the WVB, native fragment, bundle, metadata, runtime data, startup, and complete PE/ELF container before atomic candidate publication.

## Candidate identities and fixed vector

The current candidate identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVB-to-WVO tool WVB | 372,514 | `f2283d33fdcae404a6dd15f6a888c3d1efa359328110fca6d54be1aa67cc1d5c` |
| Windows WVB-to-WVO tool | 5,348,864 | `0e0d0c87f82f6576b11f888cfa26469f86f157064ea605a4bb188bcee5e3b280` |
| Linux WVB-to-WVO tool | 5,349,376 | `c6ba202ffcb32a261bfd9c997e4bab754ab5a636e2d0b95e5de5f55e598c6358` |

These pinned applications predate Decision 0419. The current Windvale source
contract additionally admits parameterless `Main() -> bytes`. Decision 0420's
segmented native construction accepts the current 409-function, 399,691-byte
lowerer WVB at SHA-256
`92655af0632b4dd3525c2b2de98353b095fa1df94b524a94aa47f16014f1e508`
and reproduces the independently pinned 5,792,768-byte Windows application at
SHA-256
`e096dc7fec20e3318364da1f3b5289f772b53c16cc370f29622dfac35780e2bf`.
That native application reproduces both the descriptor-entry and baseline-JIT
bridge WVOs byte for byte. The paired 5,791,744-byte Linux identity at SHA-256
`a9d4ae08d449aa2b1238120efb6bab9720e97f2e2a99354abf15bf086be4cb1e`
remains an exact expected reconstruction pending genuine Linux execution.
Ordinary consumers continue to use digest-bound retained WVOs until paired
promotion replaces the older candidate applications.

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

Decision 0225 composes this source candidate with the qualified native source builder and the linker/packager candidates into one exact current-host source-to-executable proof. That proof does not promote this application. Decision 0304 pins both platform applications, a native-produced fixed vector, and digest-bound candidate launchers while retaining Stage 0 host-container construction. Decision 0308 makes those launchers construct privately and publish through the shared portable WVO verifier plus native transaction. Decision 0317 fixes malformed-WVB and valid-but-unsupported-function rejection through the public launcher, with exact diagnostics, output preservation, and isolated-work cleanup. Only an exact descendant that supplies native construction and then passes both hosts moves accepted-subset WVB-to-WVO lowering to the ordinary native launcher. C# remains the complete recovery/differential backend and the normal route for every unsupported module until later Windvale-owned backend slices close those gaps. Decision 0057's complete gate still controls final deletion.
