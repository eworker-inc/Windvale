# Windvale native hosted-container metadata constructor

## Status and scope

This contract packages the portable hosted-tool metadata constructor as one
standalone native Windows/Linux command. It consumes the existing exact
`WVHM 1` request, admits the complete `WVHD 1` response, and writes the raw
1,024-byte canonical `WVH* 1` metadata record consumed by the standalone
runtime-header producer.

The command does not construct or trust resource evidence implicitly. The
paired [native metadata-request producer](Windvale-Native-Hosted-Metadata-Request.md)
now acquires immutable bundle chunks, recomputes the fragment and ten service
digests, and constructs the 576-byte request in its own process. This command
owns request validation, canonical metadata policy, response admission, and raw
metadata output.

## Command contract

```text
wvhostmetadata <request.wvhq> <metadata.wvhm>
```

The request must be the exact 576-byte `WVHM 1` envelope defined by
[hosted-tool metadata construction](Windvale-Native-Hosted-Tool-Metadata-Construction.md).
The tool derives the target and profile only after the exact request size is
known, calls the existing portable constructor, and accepts only the exact
successful 1,056-byte `WVHD 1` response. It then independently runs the shared
metadata-admission module over the returned 1,024-byte payload before writing.

Identical input/output names are rejected with usage status 64. A malformed or
rejected request, malformed response, or metadata-admission failure returns
status 2, writes one diagnostic, and leaves an existing output unchanged.
Success returns zero and reports exactly 1,024 bytes.

The module declares exactly `console.write_line`, `diagnostic.write_line`,
`file.read_bytes`, `file.write_bytes`, `process.argument`, and
`process.argument_count`. Its native fragment requires the existing nine host
services used by the other hosted-container transition tools.

## Targets and exact identities

- `windows-x64-hosted-container-metadata-v1`, producing `.exe`;
- `linux-x64-hosted-container-metadata-v1`, producing `.elf`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Metadata constructor WVB | 26,748 | `196c233ec549872204c5fcfa1c8fc275dba7ff339264de428be7ce72621a2333` |
| Windows metadata constructor | 252,928 | `f4cb8689757f1c93c8da77fa109bcb7d0e0bfd9148de54ee7c88aa03f456955e` |
| Linux metadata constructor | 253,952 | `d95e843f862f20c2b027cd7b335b8ccb683a2589b14818027cb6eabd689a782a` |

The WVB reconstructs through the native Project 1 front door. Focused
current-host evidence builds the public CLI target, reproduces the frozen
Stage 0 metadata oracle exactly, observes no CLR load, preserves an existing
output after request corruption, and rejects an output alias.

## Retirement boundary

`WVHM` validation, canonical metadata construction, `WVHD` admission, final
metadata admission, and raw runtime-input production now have a native process
boundary on both hosts. The new C# target writer is deletion-bound package
layout and identity wiring.

The standalone
[service-bundle producer](Windvale-Native-Hosted-Service-Bundle.md) now emits
one exact immutable segment response, and the native metadata-request producer
binds admitted bundle resources to `WVHM`. Ordered bundle-request/resource
orchestration, segment-request orchestration, complete pipeline composition,
Linux execution, promotion, and the grouped dual-host retirement gate remain
pending.
