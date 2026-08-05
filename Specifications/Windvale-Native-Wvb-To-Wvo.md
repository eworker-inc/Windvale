# Windvale native WVB-to-WVO application

## Status and scope

`WVHN 1` packages the canonical Windvale-written `Compilerˉnativeˉx64ˉloweringˉtool` as paired Windows x64 and Linux x64 command-line applications. It lowers only the exact metered scalar/control/direct-call subset defined by the [Windvale-native x86-64 lowering contract](Windvale-Native-X64-Lowering.md). This source candidate is not the complete native backend or an ordinary front door until the grouped Windows/Linux gate and later pinned-artifact promotion succeed.

The portable core owns WVB admission, control and type verification, ABI-22 selection, branch/call measurement and patching, and canonical WVO 1.0 construction. The application adds no second selector, object writer, or target-specific lowering logic.

## Construction contract

`Windvale-Native-X64-Lowering-Tool.wvproj` is the exact source-to-WVB project. Its root is `Compiler/Windvale/Native-X64-Lowering-Tool.wv`, its canonical module identity is `Compilerˉnativeˉx64ˉloweringˉtool`, its profile is `hosted`, and it exports exactly one `Main() -> i32`.

The native writer accepts only that identity and entry. Its fragment requires these services in this exact order:

1. `console.write_line`;
2. `process.argument_count`;
3. `process.argument`;
4. `file.read_bytes`;
5. `diagnostic.write_line`;
6. `enum.name`;
7. `text.concat`;
8. `u32.format`;
9. `file.write_bytes`.

The application bundle inserts startup-internal `text.utf8_is_valid` after `file.read_bytes`, producing the established ten-service compiler-authority layout. The six declared capabilities are `console.write_line`, `diagnostic.write_line`, `file.read_bytes`, `file.write_bytes`, `process.argument`, and `process.argument_count`.

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
| WVB-to-WVO tool WVB | 86,741 | `e1a795dd07be21ccb150823bd8790a8766af28d4361b8151cdf224a48f1c4389` |
| Windows WVB-to-WVO tool | 1,127,936 | `74fc450f042d4ef48e77c89ff7ad5f8fbf88dd19b3a9b4bae53106b536957061` |
| Linux WVB-to-WVO tool | 1,126,400 | `7bd6c4e0cf5e7cfeb416f3a36386722b9317204c828cc40794da2e87071e4538` |

`Windvale-Native-Test-Wvb-To-Wvo-Return-42.wvproj` produces the fixed accepted-subset input:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Return-42 WVB | 174 | `7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31` |
| Return-42 WVO | 479 | `0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5` |

The WVO contains 406 code bytes, one exported `Main`, no data, imports, or relocations, and is admitted by the independent WVO parser. The current-host native application must reproduce it byte for byte, repeat deterministically, reject a truncated WVB without changing a sentinel output, and load no CLR/.NET runtime.

## Qualification and retirement boundary

Promotion requires one exact source commit to pass on Windows and Linux with:

- byte-identical tool WVB and deterministic platform packages;
- independently reconstructed and verified format-9 containers with profile 6;
- exact service and capability agreement;
- direct current-host success, deterministic repetition, usage, and malformed-input execution;
- exact fixed-vector WVO plus the retained differential corpus;
- no CLR/.NET module or mapping in the lowerer process; and
- no regression in WVB 1.11, WVO 1.0, ABI 22, metering, control-flow, or call contracts.

After that source gate, pin both platform applications and add digest-bound native launchers in a separate provenance commit. Only the exact pinned-artifact commit passing both hosts moves accepted-subset WVB-to-WVO lowering to the native launcher. C# remains the complete recovery/differential backend and the normal route for every unsupported module until later Windvale-owned backend slices close those gaps. Decision 0057's complete gate still controls final deletion.
