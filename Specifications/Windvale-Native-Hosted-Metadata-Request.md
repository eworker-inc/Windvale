# Windvale native hosted metadata-request producer

## Status and scope

This contract constructs the exact 576-byte `WVHM 1` request consumed by the
standalone hosted-container metadata constructor. The native command admits one
canonical publication plan, reads the immutable service-bundle resources named
by `WVHS 1`, recomputes all eleven SHA-256 identity leaves inside its own
process, and writes the request only after the plan, manifest, evidence, and
canonical service placements agree.

The command does not consume a loose `WVHE` file or accept projected digest
strings. The actual chunk bytes, their manifest, the publication layout, and
the resulting raw digests therefore remain in one native trust boundary.

## Command contract

```text
wvhostrequest <inputs.wvmi> <plan.wvpq> <manifest.wvhs> <chunk-prefix> <request.wvhq>
```

Chunk `N` is read from `<chunk-prefix>.chunk-N` under the bounded resource
rules in the [streaming evidence contract](Windvale-Native-Streaming-Sha256-Evidence.md).
Input, plan, manifest, output, and every derived chunk resource must be distinct
names. Wrong argument count returns 64. Rejection returns 2, writes one
diagnostic, and preserves any existing output. Success returns zero and reports
exactly 576 bytes.

The module declares exactly `console.write_line`, `diagnostic.write_line`,
`file.read_bytes`, `file.write_bytes`, `process.argument`, and
`process.argument_count`.

## Fixed inputs: `WVMI 1`

`WVMI 1` is exactly 32 little-endian bytes.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVMI`, `0x494D5657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `32` |
| 12 | 4 | target | `1` Windows x64 or `2` Linux x64 |
| 16 | 4 | hosted profile | `1` through `7` |
| 20 | 4 | native entry | Within the planned native fragment |
| 24 | 8 | reserved | Zero |

The bundle file offset is canonical policy and is written as 4,096 rather than
accepted from this input.

## Plan and evidence binding

The publication input is the exact 144-byte `WVPQ 1` request for ten services.
The command executes the shared Windvale publication planner and requires the
canonical ordered service identities `1` through `8`, then `11` and `12`.
Its successful layout is exactly 152 bytes.

The admitted `WVHS 1` manifest must describe the logical concatenation of the
actual source resources and exactly eleven ordered regions: region zero is the
complete native fragment; regions one through ten are the exact service bytes
in canonical publication order. These regions are contiguous in logical-source
space. Their independently admitted publication placements may contain
alignment gaps in image space; those padding bytes are not source resources,
do not enter the logical byte count, and never become identity leaves.

The process reads each named chunk and drives the shared portable streaming
SHA-256 state. It constructs and validates the corresponding 548-byte `WVHE 1`
value in memory. It separately requires the manifest logical byte count to
equal the fragment byte count plus all ten planned service byte counts. The
native-image digest and ten service digests are copied into `WVHM 1` only after
that evidence is bound to the exact manifest and the image placements remain
bound to the publication plan.

## Targets and exact identities

- `windows-x64-hosted-metadata-request-v1`, producing `.exe`;
- `linux-x64-hosted-metadata-request-v1`, producing `.elf`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Metadata-request WVB | 63,278 | `55edb3633ee13f4ed7b02781e469c2d0325d8a0a8e274658a3bb06cc580bac04` |
| Windows metadata-request tool | 1,052,672 | `4d1d5c114f9b022e594dd7d4abef2408143f9de60e4fa4bb00810316b5557366` |
| Linux metadata-request tool | 1,052,672 | `8a4fb176439e2b71f98c244a98c04deec7985453038f3b2813de6fd6e179d4dd` |

The WVB reconstructs byte-for-byte through the native Project 1 front door.
The package writers are deletion-bound Stage 0 target and identity wiring; no
C# code selects or calculates product evidence.

## Source organization and retirement boundary

Manifest admission, SHA-256 compression/streaming, and per-resource region
state remain focused portable modules. The exact request-format constructor
currently shares the hosted CLI root because the current native source composer
rejects that otherwise valid extracted branch at its binding boundary. This is
a documented bootstrap constraint, not permission to duplicate the algorithm
or grow numbered source fragments; re-extract it when the native compiler can
compile the same graph.

Immutable bundle resources now flow through a native process into the existing
native metadata constructor without managed hashing or request projection.
[Decision 0414](../Documents/Decisions/0414-Digest-Bound-Native-Hosted-Container-Composition.md)
composes that producer into the complete Windows candidate and verifies a real
alignment gap. Linux-host execution, promotion, managed-entry-point cutover,
and the grouped dual-host retirement gate remain pending.
