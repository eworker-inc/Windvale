# Windvale native console-application packager

## Status and scope

`WVHP 1` packages the canonical Windvale-written `Consoleˉapplicationˉpackager` as paired Windows x64 and Linux x64 command-line applications. Decision 0303 pins exact candidate artifacts, and Decision 0307 composes their digest-bound launchers with the native atomic console-application publisher. Decision 0343 makes the WVB natively reconstructible. This is not yet an ordinary front door: Stage 0 remains the host-container construction and recovery target until native PE/ELF construction, the grouped Windows/Linux gate, and artifact promotion succeed.

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

The completed application must fit in one immutable Windvale byte value, so this profile currently rejects results larger than 4,194,304 bytes. It then invokes the portable verifier and requires exact target, application length, native bytes, and entry recovery before making exactly one `file.write_bytes` call. Rejection therefore leaves a requested output missing or unchanged at the raw application boundary. The digest-bound `Package-Console.cmd` and `.sh` launchers write that raw result to a private candidate and invoke the [native console-application publisher](Windvale-Native-Console-Application-Publisher.md) for atomic durable replacement.

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

`Tools/Native/Test-Console-Packager-Source-Reconstruction.cmd` and `.sh`
build the ordinary and segmented projects through the digest-bound native
Project 1 front door. The ordinary reconstruction must reproduce the 58,127-byte
identity above; the segmented sibling must reproduce 68,451 bytes at SHA-256
`33d7619c6115295a9eb612fd559031ab99c85196e3133a9405f880a19ac9ded2`.
Both builds include compiler-aligned WVB verification and the separate native
atomic publisher. This proves source-WVB reconstruction, not construction of the
checked-in PE/ELF tool containers.

The focused test reconstructs both containers, checks exact capabilities and services, exercises the public current-host AOT target, and runs the current-host raw application. One six-byte return-42 image must package deterministically into both target formats. The independent platform verifiers recover the exact image and zero entry, the current-host result returns 42, malformed target or entry input preserves an existing output, and process inspection finds no CLR/.NET runtime.

## Fixed native rejection contract

`Tools/Native/Test-Console-Packager-Rejections.cmd` and `.sh` exercise only the
digest-bound current-host packager launcher. Every case rejects before the
launcher can invoke its publisher. The commands do not rebuild source, link an
image, repeat the successful AOT chain, invoke .NET, or consult a live Stage 0
oracle.

The fixed six-byte image is stored as
`Tests/Native/Images/Return-42.bin.b64` and has SHA-256
`11db5348e275fb704be582e8005ee7d604f7f17b154d6cc644d240eef29d456a`.
The empty-image case has zero bytes. The existing 479-byte bad-magic WVO at
SHA-256
`0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288`
is copied to the destination as a sentinel before every case.

Each case must return `2`, write no standard output, preserve the complete
sentinel, and emit one LF-terminated diagnostic. Because the target identity is
part of the report, the complete report hashes are host-specific:

| Case | Windows report SHA-256 | Linux report SHA-256 |
| --- | --- | --- |
| `entry-at-end` | `a48244ecee195c2171cd3bdcf93261deed94b5d3522623f81557d146ec0f4071` | `a35789de908a6275c48a6cd25f1969732cec08fe1b39cdea615c35da1e79124e` |
| `invalid-entry` | `52264e728059fe229b20c14ad9e1febecc97da454ed2de58f34b85fdd99d4349` | `7ed94e6029a369b7ca0e967dae679e85be37c85017088c1e72b94c2123626c48` |
| `empty-image` | `52264e728059fe229b20c14ad9e1febecc97da454ed2de58f34b85fdd99d4349` | `7ed94e6029a369b7ca0e967dae679e85be37c85017088c1e72b94c2123626c48` |

Success prints the three ordered `PASS` lines followed by
`Tests: 3, Passed: 3, Failed: 0` plus LF. Unsupported target names remain a
launcher usage error; they are not mislabeled as native packager execution.
This fixed set does not replace the complete construction, verification, limit,
publication-fault, concurrency, or hostile-input corpus.

## Qualification gate

Promotion requires one exact source commit to pass on Windows and Linux with:

- byte-identical WVB and deterministic platform packages;
- independently reconstructed and verified format-8 containers with profile 5;
- current-host self-test, both output formats, deterministic repetition, rejection, and direct application execution;
- exact raw-image and entry recovery under the portable and independent platform verifiers;
- no CLR/.NET module or mapping in the packager process; and
- no regression in version-1 PE/ELF, native ABI, capability, construction, or verification contracts.

Decision 0303 pins both platform applications and explicitly records Stage 0 construction. Decision 0307 adds native safe publication to their digest-bound launchers, and Decision 0343 closes exact WVB source reconstruction. Only an exact descendant that supplies native host-container construction and passes both hosts moves ordinary version-1 materialization to the native packager. Stage 0 remains a named recovery/differential path until Decision 0057's complete archive gate permits deletion.
