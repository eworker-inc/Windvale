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
6. a FAT32 data-cluster count from 65,525 through 268,435,445 and a root cluster
   inside that derived range;
7. a nonzero FSInfo sector strictly inside the reserved area and a backup-boot
   sector that is either absent (`0`) or also strictly inside that area; and
8. zero FAT32 reserved fields plus valid mirroring/active-FAT flags.

All products, sums, and derived sector counts use `u64`. The accepted record
contains bytes/sector, sectors/cluster, reserved sectors, FAT count and size,
total and data sectors, first data sector, cluster count, and root cluster.
Every rejection returns zero geometry.

## Evidence and limits

The admission module builds as a 7,367-byte WVB at SHA-256
`d7f5e96b7d4710f8ba9d68c991239ad1a77b23943ca3d112862b3307168d93e2`.
The focused 13,866-byte self-test WVB at SHA-256
`1d500f81f31fd79a79bf9710fc4adabdc3247b911c7a08ce73945a7872c45c87`
lowers to deterministic Windows/Linux images and passes locally with result 47.
Twenty-five cases cover one valid 70,000-sector/68,890-cluster geometry, the
legal no-backup form, and truncated, malformed, legacy, unsupported,
mismatched, undersized, or out-of-range variants.

This slice does not validate FSInfo contents or the backup sector, select an
active FAT, mask or follow FAT entries, detect cluster-chain cycles, parse
short or long directory entries, read file data, handle media removal, or
enable mutation. Those remain mandatory read-only-provider increments before
FAT32 is connected to a guest application.
