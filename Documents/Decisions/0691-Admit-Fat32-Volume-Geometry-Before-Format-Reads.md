# Decision 0691: Admit FAT32 volume geometry before format reads

- Status: Implemented architecture-neutral native candidate; guest block integration pending
- Date: 2026-08-16
- Advances: [filesystem implementation plan](../Project/Windvale-Filesystem-Implementation-Plan.md)
- Contract: [FAT32 volume admission 1](../../Specifications/Windvale-Os-Fat32-Volume-Admission.md)

## Context

Windvale OS selected FAT32 for its first boot and interchange filesystem, but a
format service cannot safely derive offsets from an untrusted BIOS parameter
block. Native integer overflow, a too-small FAT, a mismatched device extent, or
an out-of-range root cluster could otherwise direct later reads outside the
granted block capability.

## Decision

- Admit one exact 512-byte FAT32 boot sector before any FAT, directory, or data
  access.
- Match its declared total sectors to an independently supplied block-device
  extent; boot-sector bytes do not establish storage authority.
- Perform FAT-area, data-area, capacity, cluster-count, and root-cluster
  arithmetic in `u64`.
- Reject FAT12/16 geometry, unsupported FAT32 versions, invalid metadata-sector
  locations, and any FAT that cannot address every derived data cluster.
- Freeze a deliberately strict first profile: 512-byte sectors, power-of-two
  clusters through 64 KiB, one or two FATs, extended signature `0x29`, and the
  exact `FAT32   ` profile marker, valid FAT mirroring/selection, and zero
  reserved fields.
- Return an immutable geometry record on success and zero geometry on every
  failure.

## Evidence and consequences

The 7,367-byte policy WVB has SHA-256
`d7f5e96b7d4710f8ba9d68c991239ad1a77b23943ca3d112862b3307168d93e2`.
The 13,866-byte test WVB has SHA-256
`1d500f81f31fd79a79bf9710fc4adabdc3247b911c7a08ce73945a7872c45c87`;
its paired native images cover 25 cases and the current Windows host returns
47.

This creates the trusted geometry input for a read-only FAT and directory
reader. It does not claim a block driver, a complete FAT32 parser, file reads,
guest launch, writable media, or crash recovery.

## Reconsideration triggers

Add a new explicit profile when a measured device requires other logical-sector
sizes or a compatibility field omitted here. Do not broaden admission merely
because a host operating system accepts a malformed or ambiguous volume.
