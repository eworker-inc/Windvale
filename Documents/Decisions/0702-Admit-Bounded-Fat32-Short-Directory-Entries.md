# Decision 0702: Admit bounded FAT32 short directory entries

- Status: Implemented architecture-neutral native candidate; long names and file reads pending
- Date: 2026-08-16
- Advances: [filesystem implementation plan](../Project/Windvale-Filesystem-Implementation-Plan.md)
- Contract: [FAT32 directory admission 1](../../Specifications/Windvale-Os-Fat32-Directory-Admission.md)

## Context

Volume geometry, cluster chains, and block exchanges can retrieve directory
bytes but cannot safely identify a file. Directory records are untrusted media
input with deletion markers, long-name slots, attribute combinations, cluster
fields, duplicate names, and chain-boundary ambiguity.

## Decision

- Admit only whole 32-byte entries under a ceiling of 4,096 entries.
- Begin with exact canonical 11-byte short-name lookup; do not silently invent
  long-file-name or Unicode mapping semantics.
- Skip deleted, long-name, and valid volume-label entries while rejecting
  reserved attributes, reserved/bad clusters, and inconsistent cluster/size
  fields.
- Distinguish a zero end marker from exhaustion of a confirmed complete chain
  and reject an incomplete trace as truncated.
- Continue scanning after a match and reject duplicate target entries.

The 6,340-byte module has SHA-256
`14548e1da399a95bb8c25be9c9224b4d524c729457992c2dc26ef153561b7733`.
Its 19 focused cases return 47 and pin paired Windows/Linux images.

## Consequences

The FAT32 service can now admit a short-name file or directory record after a
bounded chain read. It still cannot publish general Windvale path lookup until
long-name/Unicode policy or an explicit short-name-only mounted profile is
selected. File-data cluster reads, timestamps, media change, and live driver
composition remain pending.

## Reconsideration triggers

Add VFAT long-name assembly only with exact slot ordering, checksum, UTF-16,
surrogate, normalization, collision, malformed-input, and work-limit rules.
