# Decision 0705: Bound FAT32 file-read mapping

- Status: Implemented architecture-neutral native candidate; live I/O pending
- Date: 2026-08-16
- Advances: [filesystem implementation plan](../Project/Windvale-Filesystem-Implementation-Plan.md)
- Contract: [FAT32 file-read plan 1](../../Specifications/Windvale-Os-Fat32-File-Read-Plan.md)

## Context

Volume, cluster-chain, directory, and block-exchange boundaries can admit all
inputs needed to read file data, but composing them without an exact mapping
contract risks crossing file, cluster, device, or transfer bounds. FAT32 file
sizes are 32-bit while shared Windvale file positions are 64-bit, so narrowing
and end-of-file behavior must also be explicit.

## Decision

- Map only an already-admitted file offset and an already-resolved chain
  cluster; do not duplicate chain traversal inside the mapper.
- Validate the caller-supplied cluster ordinal against the file offset and
  require the cluster to remain within admitted volume geometry.
- Keep file positions and device-sector calculations in `u64` while retaining
  the exact FAT32 `u32` file-size field.
- Limit one ready result by the request, file tail, cluster tail, and a
  4,096-byte block window, producing no more than eight sectors.
- Distinguish exact end-of-file from invalid offset, geometry, request limit,
  ordinal, or cluster.

The 4,543-byte module has SHA-256
`71868dc89be5f640ca137b50ba09ccddab3a940706756a209570079f3e2e2b1d`.
Its 16 focused cases return 47 and pin paired Windows/Linux images.

## Consequences

The FAT32 service now has a deterministic plan for each bounded file-data block
after chain resolution. This is not yet an application-visible read: a later
composition must bind the admitted directory record and chain result to a media
generation, dispatch the block exchange, extract the requested partial-sector
bytes, and form the filesystem response.

## Reconsideration triggers

Change the plan only if a selected block provider uses a different sector or
transfer contract, or if measured scatter/gather requirements justify a
versioned multi-extent result without weakening the current bounds.
