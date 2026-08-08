# Windvale native streaming SHA-256 evidence

## Status and scope

`WVHS 1` and `WVHE 1` define a bounded, capability-free hashing plan and its
raw SHA-256 evidence. The standalone hosted command reads a sequence of
immutable resources, hashes ordered logical regions across resource boundaries,
and writes one manifest-bound evidence file.

This contract exists because one Windvale `bytes` value is intentionally capped
at 4 MiB while a complete hosted compiler native image is about 26 MiB. The
command does not weaken that value limit, add a cryptography capability, depend
on a host SHA implementation, or accept hardcoded product digests as proof.

## Command contract

```text
wvsha256evidence <manifest.wvhs> <chunk-prefix> <evidence.wvhe>
```

Chunk resource `N` is named `<chunk-prefix>.chunk-N`. The prefix contains one
through 16 chunks. Each resource remains at most 4,194,304 bytes, but their
logical concatenation may contain up to 67,108,864 bytes. The command reads
only the manifest and the named chunks, and publishes the evidence only after
the complete plan and every used resource extent have been admitted.

The module declares exactly `console.write_line`, `diagnostic.write_line`,
`file.read_bytes`, `file.write_bytes`, `process.argument`, and
`process.argument_count`. Success returns zero and reports the evidence byte
count. A malformed manifest, aliased resource, changed resource length,
incomplete region, or digest failure returns status 2 and leaves an existing
evidence output unchanged. Wrong argument count returns 64.

## Sequence manifest: `WVHS 1`

The 32-byte header is followed by one 20-byte record per chunk and one 12-byte
record per region.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVHS`, `0x53485657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | Exact header plus records |
| 12 | 4 | chunk count | `1` through `16` |
| 16 | 4 | region count | `1` through `16` |
| 20 | 4 | logical bytes | `1` through 64 MiB |
| 24 | 8 | reserved | Zero |

Each chunk record contains its zero-based ordinal, logical offset, resource
offset, logical byte count, and complete resource byte count. Chunk ordinals
and logical extents are contiguous and cover the declared logical sequence
exactly. The selected resource extent must fit within a nonempty resource no
larger than 4 MiB. A resource offset permits hashing `WVSI` payloads without
copying or trusting their 40-byte response envelope as image bytes.

Each region record contains its zero-based ordinal, logical offset, and
nonzero byte count. Regions are ordered, nonoverlapping, and fully contained in
the logical sequence. Gaps are permitted because aligned bundle fill is not a
metadata identity leaf.

## Evidence: `WVHE 1`

The 64-byte header is followed by one 44-byte record per region.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVHE`, `0x45485657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `64 + region-count * 44` |
| 12 | 4 | logical bytes | Exact admitted manifest value |
| 16 | 4 | region count | Exact admitted manifest value |
| 20 | 4 | manifest bytes | Exact `WVHS` length |
| 24 | 32 | manifest SHA-256 | Raw digest of the complete `WVHS` bytes |
| 56 | 8 | reserved | Zero |

Each evidence record repeats the region ordinal, logical offset, and byte count,
then carries the raw 32-byte SHA-256 digest of exactly that logical region. A
consumer must match the header digest to the exact manifest it admits and match
every region descriptor before using a digest in another format.

## Windvale owner and bounded implementation

`Foundation/Sha256-Compression.wv` owns one exact 64-byte SHA-256 compression
step and uses checked 16-bit halves for wrapping addition; every intermediate
sum is at most 131,071. `Foundation/Sha256-Streaming.wv` owns tail retention,
multi-value update, padding, and raw digest encoding. A compression call builds
only its 256-byte schedule and returns before the next block, so dynamic state
does not grow with the logical input.

`Streaming-Sha256-Evidence-Core.wv` owns manifest/evidence admission, while
`Streaming-Sha256-Resource-Evidence.wv` owns portable region-to-chunk state and
envelope construction. The small hosted shell owns only resource acquisition,
path alias rejection, and no-write failure behavior. The same portable state is
reused by the native metadata-request producer.

## Targets and exact identities

- `windows-x64-streaming-sha256-evidence-v1`, producing `.exe`;
- `linux-x64-streaming-sha256-evidence-v1`, producing `.elf`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Streaming evidence WVB | 39,483 | `9ea136c44f76d9a53474d81e989f38cf15ffa8dc267638e2224e20f733205e6b` |
| Windows streaming evidence tool | 646,656 | `e69a9a4c0a33d4bc05d98e5c977ba537715081d08d8c8493da87990989547af3` |
| Linux streaming evidence tool | 647,168 | `eb1ba149a94741801732e53773f566aee752a19510d22f8e2768def7614d396c` |

The WVB reconstructs byte-for-byte through the native Project 1 front door.
The package writers are deletion-bound Stage 0 target and identity wiring; the
product process and all digest decisions are Windvale-owned.

## Retirement boundary

Large immutable resource sequences and their ordered identity regions can now
produce manifest-bound raw SHA-256 evidence without a managed runtime process.
The [native metadata-request producer](Windvale-Native-Hosted-Metadata-Request.md)
reuses this state to compute the eleven actual bundle leaves and construct
`WVHM` without trusting a loose evidence file. Ordered `WVSI` orchestration,
complete composition, Linux execution, normal-path promotion, and the final
grouped retirement gate remain pending.
