# Windvale native console-application segmented construction

## Status and scope

This contract transfers construction of the maximum valid version-1 Windows
and Linux console applications to one bounded Windvale-native staging tool. It
keeps the ordinary 4 MiB byte-value limit: the completed application never
exists as one Windvale value. The tool instead emits one exact 4 MiB first
chunk, one bounded second chunk, and a fixed staging manifest written last.

The current applications are Stage 0-constructed candidates. Normal candidate
execution does not load .NET, but native source and host-container
reconstruction, Linux execution, promotion, and grouped qualification remain.

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
| `Console-Segmented-Packager.wvb` | 68,451 | `33d7619c6115295a9eb612fd559031ab99c85196e3133a9405f880a19ac9ded2` |
| `Console-Segmented-Packager.exe` | 782,336 | `b9c02553966a758001a7ea03428565cf306fc8e5203e58056aebc1fed6b4a253` |
| `Console-Segmented-Packager.elf` | 782,336 | `779a87a9246e5d13eab08bf47ab53d329e627c5e64e6cfe86082cc6600450089` |

Construction is recorded as `stage0-recovery`. The project currently reaches
the known native source-binding ceiling shared by the ordinary console
packager, so these artifacts remain candidates rather than rebuildable native
front-door products.

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
path, reconstruct the packager through the native source front door, qualify
Linux execution, promote either candidate, transfer large-native WVO
construction, or complete the Decision 0057 retirement gate.
