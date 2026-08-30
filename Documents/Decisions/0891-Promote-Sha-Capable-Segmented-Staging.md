# Decision 0891: promote SHA-capable segmented staging

## Status

Accepted implementation candidate with focused Windows reconstruction evidence
on 2026-08-30. Local Linux reconstruction and paired-host CI qualification
remain pending.

## Context

[Decision 0890](0890-Lower-Bytes-Sha256-Hex-In-The-Native-X64-Backend.md)
implements bounded native x64 lowering for `bytes.sha256_hex` and explicitly
keeps candidate-pin promotion separate from the opcode proof. The registered
SHA owner reconstructed temporary current-source tools, so it proved the
lowering without silently replacing retained compiler products.

The first real admission-validator consumer then exposed both retained-product
boundaries. Its SHA-capable WVB could be staged by a temporary current-source
producer, but the pinned pre-SHA WVO producer rejected opcode `0x7D`. After
using the current producer, the pinned compiler-image stager rejected the
otherwise valid private `$native_sha256_hex` symbol. Refreshing only the first
application would therefore move the failure one process later rather than
make the retained segmented path SHA-capable.

The canonical transport does not interpret WVB operations or WVO symbols. Its
three artifacts remain valid byte-preserving transport products and do not
need regeneration.

## Decision

1. Promote the two source-changed segmented families together: the WVO staging
   producer WVB plus its Windows/Linux applications, and the compiler-image
   staging WVB plus its Windows/Linux applications.
2. Pin these exact identities:

   | Artifact | Bytes | SHA-256 |
   | --- | ---: | --- |
   | `Wvo-Staging-Producer.wvb` | 587,383 | `2a3d3a1c088ff0a99eab3ec5f1723fe0bf2886eddbe0fc0858e4f0250bb419aa` |
   | `windows-x64-wvstage.exe` | 8,630,784 | `a20d6b9465540bc72a28b61e128b31254ce76db1d840b558a73944ebe22bf7d7` |
   | `linux-x64-wvstage.elf` | 8,630,272 | `139025c23fb52e02afd27b0e7514f3617502230bf99130fd47da262e14640c93` |
   | `Compiler-Image-Staging.wvb` | 81,530 | `825445b022cfd8a6b75fc6e0a63df548707bf5251f840d7cf0c33e2cf2ac15c9` |
   | `windows-x64-wvlinkstage.exe` | 931,840 | `969bc653c765e3d2e24f62afaa50717268df51fcb805f66e927f0f16ab47838f` |
   | `linux-x64-wvlinkstage.elf` | 933,888 | `d5909f461c10c6529f881350e86d288cdb40a6ed0b600b75ada86037265af4b0` |

3. Preserve all three canonical-transport artifacts byte for byte:

   | Artifact | Bytes | SHA-256 |
   | --- | ---: | --- |
   | `Compiler-Image-Canonical-Transport.wvb` | 23,836 | `d4bdfa7588e4431432a300e0da257507d73846931f5dd1296855b03714d218c8` |
   | `windows-x64-wvimagetransport.exe` | 269,312 | `e724a5efbffc233fda76f55bfb5cc01c044e221882b5de5f247b0ab236726f81` |
   | `linux-x64-wvimagetransport.elf` | 270,336 | `9ff5401eca1ffd93a49077dd6ebc56c446c59939379a481f22662465fc3cf6db` |

4. Update every normal segmented launcher and cache consumer to verify the
   promoted application for its current host before execution. Do not retain a
   compatibility fallback to either pre-SHA application.
5. Extend the reconstruction owner from four to five cases. The new bounded
   case builds the exact 237-byte SHA fixture, stages it with the locally
   reconstructed WVO producer, requires the exact 2,860-byte segmented WVO,
   and links it with the locally reconstructed image stager into the exact
   2,672-byte image. This proves both boundaries in sequence and specifically
   proves acceptance of the valid private helper symbol.
6. Keep the native SHA owner separate. This checkpoint consumes its established
   opcode and helper contract; it does not rerun or redefine the eight-case
   lowering oracle.
7. Treat this as retained candidate promotion, not as admission-validator,
   compiler-coordinator, Slice 8, or paired-host qualification completion.

## Initial evidence

Current source deterministically builds the 587,383-byte producer WVB and
81,530-byte image-staging WVB at the identities above. Focused generation
stages them to 8,634,149 and 914,898 WVO bytes, links 8,611,649-byte and
911,258-byte images, and packages the exact six promoted artifacts. The three
transport artifacts retain their prior sizes and SHA-256 identities.

The SHA smoke WVB has SHA-256
`d7962514021a6771efef7894472efabf339014b03051b54d97165cca030dafdf`.
Direct execution of the promoted Windows applications reports
`object-bytes=2860 chunks=6 manifest-bytes=96` followed by
`image-bytes=2672 entry-offset=0 chunks=2 manifest-bytes=52`. The settled
Windows reconstruction owner rebuilds all three artifact families, reproduces
every exact candidate byte, passes the SHA stage-through-link case, retains
compiler-scale staging, and reports `Tests: 5, Passed: 5, Failed: 0`.

The registered inventory has 122 owners and 5,850 cases in 21,412 LF-only bytes
at SHA-256
`b549ef509cf6eac5d53679ea79ceae66f4b2d17306e2daa66b8d1e6bb7032208`.
Its four shards contain 1/57, 42/2,762, 38/1,782, and 41/1,249 owners/cases.
This is current-Windows development evidence, not local Linux execution or
paired-host qualification.

## Consequences

Any compiler product whose WVB uses `bytes.sha256_hex` can now traverse the
ordinary retained segmented WVO and compiler-image staging path. A valid
`$native_sha256_hex` helper no longer fails as an unknown symbol, while the
stager continues to reject duplicate, misplaced, out-of-range, or malformed
symbols under its existing bounds.

Six tracked binary artifacts change and three do not. The larger application
containers are an explicit cost of embedding the current source behavior;
they do not change WVB, WVO, WVOP, WVLI, hosted-container, capability, or
transport format versions. Existing SHA-free output identity remains owned by
Decision 0890's independent oracle.

## Reconsideration triggers

Reconsider the paired promotion if Windows and Linux reconstruction yields
different WVB, WVO, image, or failure behavior; if the private-helper contract
changes; if a smaller shared staging product can retain the same exact safety
checks; if the 62-resource staging or 64 MiB image bounds change; or if the
promoted applications materially regress measured compiler feedback time or
retained storage. Any replacement must retain a bounded stage-through-link
SHA smoke and exact transport preservation evidence.
