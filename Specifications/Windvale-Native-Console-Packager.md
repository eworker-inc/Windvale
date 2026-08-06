# Windvale native console-application packager

## Status and scope

`WVHP 1` packages the canonical Windvale-written `Consoleˉapplicationˉpackager` as paired Windows x64 and Linux x64 command-line applications. Decision 0303 pins exact candidate artifacts and digest-bound launchers, but this is not yet an ordinary front door: Stage 0 remains the construction and recovery target packager until native construction, safe publication, the grouped Windows/Linux gate, and artifact promotion succeed.

The application composes the existing [console-application plan](Windvale-Console-Application-Plan.md), [construction](Windvale-Console-Application-Construction.md), and [verification](Windvale-Console-Application-Verification.md) contracts. It adds no second PE or ELF layout implementation.

## Construction contract

`Windvale-Console-Application-Packager.wvproj` is the exact source-to-WVB project. Its canonical module identity is `Consoleˉapplicationˉpackager`, its profile is `hosted`, and it exports exactly one `Main() -> i32`.

The native writer accepts only that identity and one exported `Main`. Its fragment requires these services in this exact order:

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
wvpack <windows-x64-console-v1|linux-x64-console-v1> <native-image.bin> <entry-offset> <output>
```

The native image is 1 through 4,194,304 bytes. The entry offset is canonical unsigned decimal with no sign or surrounding text and must be within the image. The application constructs the exact 32-byte `WVCQ 1` request, invokes the existing sparse constructor, independently validates the complete `WVCC 1` recipe, and materializes every literal, native, and canonical-zero span.

The completed application must fit in one immutable Windvale byte value, so this profile currently rejects results larger than 4,194,304 bytes. It then invokes the portable verifier and requires exact target, application length, native bytes, and entry recovery before making exactly one `file.write_bytes` call. Rejection therefore leaves a requested output missing or unchanged at the application boundary. Atomic durable replacement is a separate launcher/publication responsibility.

Success prints one LF-terminated report and returns 0:

```text
package status=Valid target=<target> native-image-bytes=<n> entry-offset=<n> application-bytes=<n>
```

A deterministic request, recipe, limit, or verification rejection prints the same bounded shape to diagnostics with its typed status and returns 2. Wrong argument count prints usage and returns 64. A host input or output failure remains a runtime-boundary failure. No arguments run the internal deterministic self-test for both targets.

## Container and targets

The metadata magic is `WVHP`, format version is 1, profile number is 5 in the shared hosted-compiler family, profile flags are 6, and the outer container format is 8. The public targets are:

- `windows-x64-console-packager-v1`, producing `.exe`;
- `linux-x64-console-packager-v1`, producing `.elf` and exact executable mode on Linux.

Both targets reuse the compiler-authority process entry, argument capture, bounded file adapters, runtime state, service leaves, and platform containers. No new WVA or platform assembly is added. The Stage 0 `compile` and `aot` commands independently verify the WVB, native fragment, service bundle, metadata, runtime data, startup, and complete PE/ELF container before atomic candidate publication.

## Candidate identities and evidence

The current candidate identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Console packager WVB | 58,127 | `7b055d4e6a456680a79eb28eaafa577e0019ea0ff1e34d9e713e9178428acc29` |
| Windows console packager | 667,648 | `a9cd6e222b869d838f563ffc46ae3acbde74ff8beb10c28373b6d5985c8f680f` |
| Linux console packager | 667,648 | `10b1d752ab6c9c7217f833add9ef77ca0d61b6bcc02d7023b1877f42bab2a683` |

The focused test reconstructs both containers, checks exact capabilities and services, exercises the public current-host AOT target, and runs the current-host raw application. One six-byte return-42 image must package deterministically into both target formats. The independent platform verifiers recover the exact image and zero entry, the current-host result returns 42, malformed target or entry input preserves an existing output, and process inspection finds no CLR/.NET runtime.

## Qualification gate

Promotion requires one exact source commit to pass on Windows and Linux with:

- byte-identical WVB and deterministic platform packages;
- independently reconstructed and verified format-8 containers with profile 5;
- current-host self-test, both output formats, deterministic repetition, rejection, and direct application execution;
- exact raw-image and entry recovery under the portable and independent platform verifiers;
- no CLR/.NET module or mapping in the packager process; and
- no regression in version-1 PE/ELF, native ABI, capability, construction, or verification contracts.

Decision 0303 pins both platform applications and adds digest-bound candidate launchers while explicitly recording Stage 0 construction. Only an exact descendant that supplies native construction and safe publication and then passes both hosts moves ordinary version-1 materialization to the native packager. Stage 0 remains a named recovery/differential path until Decision 0057's complete archive gate permits deletion.
