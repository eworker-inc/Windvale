# Windvale OS FAT32 volume admission 1

## Status and scope

FAT32 volume admission 1 is the first implemented read-only format boundary for
the isolated Windvale OS filesystem provider. It converts one exact 512-byte
boot sector plus an independently supplied block-device sector count into an
immutable checked geometry record. No FAT, directory, or file-data read may use
unvalidated boot-sector arithmetic.

This profile is intentionally narrower than every FAT32 variant accepted by
general-purpose operating systems. It supports 512-byte logical sectors, one or
two FATs, and power-of-two clusters from one through 128 sectors. A provider
that needs another sector size or compatibility variation must add a named
profile rather than silently weaken these checks.

## Admission order

[`Fat32-Volume-Admission.wv`](../Operating-System/Services/Fat32-Volume-Admission.wv)
requires and derives the following in order:

1. exactly 512 input bytes, the trailing `0x55AA` signature, extended signature
   `0x29`, and exact profile marker `FAT32   `;
2. 512 bytes per sector, a supported sectors-per-cluster value, a nonempty
   reserved area, and one or two FATs;
3. zero FAT12/16 root-entry count, 16-bit total-sector count, and 16-bit FAT
   size, plus FAT32 version zero;
4. a nonzero 32-bit total-sector count exactly matching the separately admitted
   block-device extent;
5. nonzero FAT32 size, a data-area start strictly before the end of the volume,
   and FAT capacity for every derived data cluster plus the two reserved FAT
   entries;
6. a FAT32 data-cluster count from 65,525 through 268,435,438 and a root cluster
   inside that derived range;
7. a nonzero FSInfo sector strictly inside the reserved area and a backup-boot
   sector that is either absent (`0`) or also strictly inside that area; and
8. zero FAT32 reserved fields plus valid mirroring/active-FAT flags.

All products, sums, and derived sector counts use `u64`. The accepted record
contains bytes/sector, sectors/cluster, reserved sectors, FAT count, selected
active FAT and mirroring mode, FAT size, total and data sectors, first data
sector, cluster count, and root cluster.
Every rejection returns zero geometry.

The upper cluster-count limit is deliberately stricter than the 28-bit storage
ceiling. It keeps every allocatable cluster at or below `0x0FFFFFEF`, so the
reserved `0x0FFFFFF0` through `0x0FFFFFF6` entry values can never be confused
with ordinary data-cluster links.

## Evidence and limits

The admission module builds as a 7,654-byte WVB at SHA-256
`564793e2af919a9adf7623f28775f653ac89cc642c5bb0cd22624cde896645e8`.
Its shared volume-and-chain owner lowers deterministic Windows/Linux images and
passes locally with result 47. The volume cases cover one valid
70,000-sector/68,890-cluster geometry, the legal no-backup and selected-second-
FAT forms, and truncated, malformed, legacy, unsupported, mismatched,
undersized, or out-of-range variants.

This slice does not validate FSInfo contents or the backup sector, cross-check
mirrored FAT copies, parse short or long directory entries, read file data,
handle media removal, or enable mutation. FAT entry and bounded chain admission
are specified separately; the remaining items are mandatory read-only-provider
increments before FAT32 is connected to a guest application.
