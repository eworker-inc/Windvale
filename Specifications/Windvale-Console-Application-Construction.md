# Windvale console-application construction

## Status and purpose

The console-application construction recipe is the portable Windvale-owned byte-construction boundary for `windows-x64-console-v1` and `linux-x64-console-v1`. It converts the accepted 32-byte console-application plan request into a complete sparse description of the final executable: exact literal spans, one opaque native-image span, and implicit canonical zero gaps.

The sparse form preserves the existing 4 MiB Windvale byte-value limit. A maximum native fragment remains one opaque 4 MiB input, while the recipe itself stays below 5 KiB even though the completed PE or ELF is slightly larger than 4 MiB.

This is an internal deterministic build contract, not a public executable, object, or package format. It does not authorize capabilities or redefine PE, ELF, native ABI, startup, or process-result semantics.

## Encoding

All integer fields are unsigned 32-bit little-endian values.

- Magic: bytes `WVCC`, integer `0x43435657`.
- Format version: `1`.
- Fixed header: 40 bytes.
- Segment descriptor: 12 bytes.
- Segment kind 1: literal bytes follow the descriptor table.
- Segment kind 2: copy the supplied native image; it contributes no recipe payload.

| Offset | Field |
| ---: | --- |
| 0 | Magic |
| 4 | Version |
| 8 | Total recipe bytes |
| 12 | Status |
| 16 | Failure offset |
| 20 | Target |
| 24 | Complete application bytes |
| 28 | Native-image bytes |
| 32 | Native-entry offset |
| 36 | Segment count |

Each descriptor contains kind, final-file offset, and byte length at offsets 0, 4, and 8 relative to the descriptor. Descriptors are strictly ordered by final-file offset and may not overlap. File bytes outside the described segments are canonical zero. Literal payloads follow the complete descriptor table in descriptor order with no padding or trailing bytes.

An invalid plan request produces an exact 40-byte response carrying the plan status and failure offset; all remaining header fields are zero. Valid status is 0 with failure offset 32. The status values and request-validation order are defined by the [console-application plan](Windvale-Console-Application-Plan.md).

## Canonical Windows recipe

The Windows recipe is exactly 834 bytes with five segments:

| Segment | Kind | Placement |
| --- | --- | --- |
| PE headers | Literal | Offset 0, 512 bytes. |
| Startup | Literal | Planned text file offset, 98 bytes. |
| Native image | Native copy | Planned text file offset plus native-image offset, exact input length. |
| Execution context | Literal | Planned data file offset, 112 bytes. |
| Relocation metadata | Literal | Planned metadata file offset, 12 bytes. |

The PE header literal contains every accepted DOS, COFF, optional-header, directory, and section-table byte. The startup literal includes all four final relative displacements. The context and relocation literals contain their exact initialized bytes; the remaining file-aligned data and relocation bytes are implicit zero.

## Canonical Linux recipe

The Linux recipe is exactly 4,454 bytes with four segments:

| Segment | Kind | Placement |
| --- | --- | --- |
| ELF header page | Literal | Offset 0, 4,096 bytes. |
| Startup | Literal | Planned text file offset, 158 bytes. |
| Native image | Native copy | Planned text file offset plus native-image offset, exact input length. |
| Execution context | Literal | Planned data file offset, 112 bytes. |

The header-page literal contains the complete ELF header, five program headers, version note, and canonical zero padding. The startup literal includes all four final relative displacements. Every alignment gap is implicit zero.

## Materialization and verification

Every materializer treats the recipe as untrusted. It checks exact identity, version, total size, status, request evidence, target-specific segment count, every descriptor, ordering, non-overlap, application bounds, literal extent, native extent, and complete payload consumption before copying anything into the zero-initialized result.

The Stage 0 C# target adapters separately construct recovery images from the independently checked Windvale layout and compare every completed byte. The [native console packager](Windvale-Native-Console-Packager.md) instead validates and materializes the recipe in Windvale source, then requires the portable [console-application verifier](Windvale-Console-Application-Verification.md) to recover the original target, native image, and entry before output. The detailed C# PE and ELF verifiers remain independent differential oracles during the transition.

`Linker/Windvale/Console-Application-Construction-Core.wv` is portable. Its minimal hosted bridge reads only the named request and returns the recipe. The Stage 0 linker embeds the exact verified bridge WVB, checks its size and SHA-256, authorizes only `file.read_bytes`, and applies a five-million-instruction limit.

Sparse materialization remains available as a Stage 0 recovery operation. The ordinary native candidate performs single-value materialization through 4 MiB. The separate [segmented construction contract](Windvale-Native-Console-Application-Segmented-Construction.md) streams larger accepted recipes into one exact 4 MiB value plus one bounded remainder and revalidates both without joining them. Arbitrary completed PE/ELF input uses the same portable segmented verifier. C# remains required for independent target parsing, recovery provenance, current candidate construction, and broader host evidence until the retirement gate.
