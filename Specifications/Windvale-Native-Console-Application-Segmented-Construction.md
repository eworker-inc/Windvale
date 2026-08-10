# Windvale native console-application segmented construction

## Status and scope

This contract transfers construction of the maximum valid version-1 Windows
and Linux console applications to one bounded Windvale-native staging tool. It
keeps the ordinary 4 MiB byte-value limit: the completed application never
exists as one Windvale value. The tool instead emits one exact 4 MiB first
chunk, one bounded second chunk, and a fixed staging manifest written last.

The current applications reconstruct through the retained native hosted-
container toolset on the current Windows host. Normal candidate execution does
not load .NET, and the project reconstructs its exact WVB and WVO through the
digest-bound native front doors. Independent Linux reconstruction and
execution, promotion, and grouped qualification remain.

## Portable construction

`Linker/Windvale/Console-Application-Segmented-Recipe.wv` validates the complete
`WVCC 1` sparse recipe and streams its ordered literal, native-image, and zero
regions across the 4 MiB split. It accepts only applications larger than 4 MiB
and within the existing target limits. No recipe segment may overlap, escape
the application, consume the wrong literal payload, or provide a second native
image.

`Linker/Windvale/Console-Application-Segmented-Construction.wv` creates that
recipe from the canonical request and then passes both completed chunks through
the shared portable console-application verifier. The recovered target,
application size, native image, and entry offset must all agree before either
chunk becomes publishable.

## Hosted staging tool

`Windvale-Console-Application-Segmented-Packager.wvproj` builds module
`Consoleˉapplicationˉsegmentedˉpackager`. Its command is:

```text
wvpack-segmented <windows-x64-console-v1|linux-x64-console-v1> <native-image.bin> <entry-offset> <chunk-prefix> <manifest.wvcs>
```

The two chunk resources are `<chunk-prefix>.chunk-0` and
`<chunk-prefix>.chunk-1`. Input, manifest, and generated chunk paths must be
distinct. The first and second chunks are written before the manifest; callers
therefore treat a missing manifest as incomplete staging. This is not yet the
public durable destination transaction.

The exact candidate manifest pins:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Console-Segmented-Packager.wvb` | 70,033 | `c4941f396f76467cb6455472f7f4711c21a6f65c12c09a9b5f4135987628f20e` |
| `Console-Segmented-Packager.exe` | 805,376 | `a6a6fd40a6becf0f65bbf995006e8e5410832da6f5ebc906f216f9e435032ef0` |
| `Console-Segmented-Packager.elf` | 806,912 | `8916fb509f81e29dabca7ed0202c0ad250f129e78b70b701630dbfcd55a1d30d` |

Decision 0343 corrects the project inventory order and reconstructs the exact
WVB through the native front door; Decision 0344 continues through exact WVO
lowering. Decision 0498 links that WVO once and uses profile 5 of the retained
hosted-container toolset to reconstruct both exact target applications without
invoking this segmented packager or a managed application writer. This is a
current-Windows-host cross-target construction result that consumes retained
native candidates, not a clean bootstrap or Linux execution result.

## `WVCS 1.0` staging manifest

The 60-byte little-endian manifest has this fixed layout:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | ASCII `WVCS`. |
| 4 | 2 | Major version `1`. |
| 6 | 2 | Minor version `0`. |
| 8 | 4 | Total bytes, exactly `60`. |
| 12 | 4 | Target, `1` for Windows or `2` for Linux. |
| 16 | 4 | Complete application bytes. |
| 20 | 4 | Native-image bytes. |
| 24 | 4 | Native-entry offset. |
| 28 | 4 | Chunk count, exactly `2`. |
| 32 | 4 | Maximum chunk bytes, `4,194,304`. |
| 36 | 12 | Chunk zero: index `0`, position `0`, length `4,194,304`. |
| 48 | 12 | Chunk one: index `1`, position `4,194,304`, bounded remaining length. |

The portable builder validates its own result. The target, application limit,
native-image limit, entry, chunk indices, positions, lengths, and complete
coverage are all closed values.

## Fixed evidence

`Tests/Native/Console-Application-Segmented-Construction/Corpus.tar.gz.b64`
decodes to a 4,949-byte gzip archive with SHA-256
`3363b3edc5c05f6665566f236793761cf9f7dd03aacfb29334f1535bcfcba7c9`.
Its 1,087-byte LF-only manifest has SHA-256
`27cd7d83d6c44a5b53c26c6b732523a46036a76e1be78f6b0ae590d6f873b005`.
The only data input is a 4,194,304-byte native image whose first byte is `0x31`,
last byte is `0xC3`, and SHA-256 is
`25711ae262e606e61654606b563aa7cdc93bb5288558bba0b3e533ab6eab238c`.
Its entry offset is `4,194,303`.

| Case | Application bytes | Stage 0 application SHA-256 | Second bytes |
| --- | ---: | --- | ---: |
| `windows-maximum` | 4,196,352 | `9cf6ab6650778969c97fad9e149a58d19de8334b806a6375ccc7150c3ad7091c` | 2,048 |
| `linux-maximum` | 4,202,608 | `7b5eb125ce971b53071be80c3424a34436d082b806918fd06690b32e86e87d3a` | 8,304 |

The runner verifies the archive, manifest, input, both staged chunks, `WVCS`
manifest, exact reports, and complete joined Stage 0 application identity. It
then invokes the independent digest-bound segmented verifier and rechecks input
preservation. A complete focused run ends with:

```text
Tests: 2, Passed: 2, Failed: 0
```

## Boundary

This contract does not publish the chunks atomically to a public application
path, independently reconstruct or execute the packager on Linux, promote
either candidate, transfer large-native WVO construction, prove a clean
bootstrap, or complete the Decision 0057 retirement gate.
