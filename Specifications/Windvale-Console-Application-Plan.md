# Windvale console-application plan

## Status and purpose

The console-application plan is a versioned internal boundary between Windvale-owned portable layout logic and the Stage 0 Windows PE32+ and Linux ELF64 constructors. It accepts only the bounded native-image facts shared by both targets and returns every file extent, virtual extent, and placement consumed by either constructor.

This format is deterministic build evidence, not a public executable or package format. Version 1 does not carry native bytes, construct a container, verify an executable, authorize a hosted service, or redefine either platform target.

## Encoding and limits

All fields are unsigned 32-bit little-endian integers. Requests and responses have exact sizes, include their version and total size, and reserve zero fields for compatible detection rather than implicit extension.

- Request magic: bytes `WVCQ`, integer `0x51435657`.
- Response magic: bytes `WVCP`, integer `0x50435657`.
- Format version: `1`.
- Request size: 32 bytes.
- Response size: 108 bytes.
- Native image: 1 through 4,194,304 bytes.
- Native entry offset: strictly less than the native-image size.
- Targets: `1` for `windows-x64-console-v1`; `2` for `linux-x64-console-v1`.

## Request

| Offset | Field | Rule |
| ---: | --- | --- |
| 0 | Magic | Exact request magic. |
| 4 | Version | Exact format version. |
| 8 | Total bytes | Must equal both the supplied byte length and 32. |
| 12 | Target | Exact target value 1 or 2. |
| 16 | Native-image bytes | Bounded as above. |
| 20 | Native-entry offset | Within the native image. |
| 24 | Reserved | Zero. |
| 28 | Reserved | Zero. |

Validation is ordered by safe availability: a truncated request fails before any fixed read, followed by magic, version, encoded/actual size, target, reserved fields, native-image size, and entry offset.

## Response

| Offset | Field |
| ---: | --- |
| 0 | Magic |
| 4 | Version |
| 8 | Total bytes |
| 12 | Status |
| 16 | Failure offset |
| 20 | Target |
| 24 | Complete application bytes |
| 28 | Header bytes |
| 32 | Text file offset |
| 36 | Text virtual address |
| 40 | Startup bytes |
| 44 | Native-image offset within text |
| 48 | Native-image bytes |
| 52 | Native-entry offset |
| 56 | Native-entry virtual address |
| 60 | Text virtual bytes |
| 64 | Text file bytes |
| 68 | Data file offset |
| 72 | Data virtual address |
| 76 | Data file bytes |
| 80 | Data virtual bytes |
| 84 | Metadata file offset |
| 88 | Metadata file bytes |
| 92 | Metadata virtual address |
| 96 | Metadata virtual bytes |
| 100 | Complete image virtual bytes |
| 104 | Reserved; zero |

A valid response has status 0 and failure offset 32. A rejected request returns the same exact 108-byte envelope, the applicable status and failure offset, and zero for every field from offset 20 onward.

| Status | Name | Meaning |
| ---: | --- | --- |
| 0 | Valid | Complete canonical plan. |
| 1 | Invalid size | Truncated, extended, or inconsistent request extent. |
| 2 | Invalid magic | Unknown request identity. |
| 3 | Invalid version | Unsupported request version. |
| 4 | Invalid target | Target is not one of the paired console targets. |
| 5 | Invalid native image | Native image is empty or exceeds 4 MiB. |
| 6 | Invalid entry | Entry is outside the native image. |
| 7 | Invalid reserved | A reserved field is nonzero. |
| 8 | Layout limit | A defensively checked target file limit would be exceeded. |

## Target formulas

The Windows plan fixes a 512-byte header, text file offset 512, text virtual address 4,096, 98-byte startup, native-image offset 112, 512-byte file alignment, 4 KiB virtual alignment, one 512-byte initialized data block, an 18,874,480-byte data mapping, one 512-byte relocation block containing 12 virtual bytes, and a maximum complete file of 4,196,352 bytes.

The Linux plan fixes a 4 KiB header and text placement, 158-byte startup, native-image offset 160, 4 KiB load alignment, a 112-byte initialized data context, an 18,874,480-byte data mapping, the 28-byte version note at file and virtual offset 384, and a maximum complete file of 4,202,608 bytes.

These values restate the accepted version-1 target contracts. Changing one requires changing the relevant target version or an explicit compatible contract decision, the portable planner, the independent oracle, and the malformed-input evidence together.

## Ownership and verification

`Linker/Windvale/Console-Application-Plan-Core.wv` owns request validation, alignment, checked target calculations, and response serialization as a portable module. A minimal hosted bridge reads one explicitly named immutable request and returns the response bytes; the bridge has only `file.read_bytes` capability.

The Stage 0 linker embeds the exact verified bridge WVB and checks its size and SHA-256 before evaluation. It authorizes only the one input capability and applies a two-million-instruction limit. C# then independently reconstructs every expected field with checked arithmetic and rejects unknown status, truncation, extension, envelope changes, nonzero reserved data, and any field disagreement. Only the verified plan reaches a PE or ELF writer.

The current C# constructors and independent untrusted-executable verifiers remain the recovery oracles. Moving byte construction and PE/ELF verification into portable Windvale is a later gate and must preserve exact output bytes and rejection behavior.
