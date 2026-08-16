# Windvale OS FAT32 file-read plan 1

## Status and scope

File-read plan 1 is an implemented architecture-neutral mapping boundary for
read-only FAT32 file data. It accepts admitted volume geometry, an admitted
directory file size, one shared `u64` file offset, a bounded request, and the
cluster already resolved for the exact file-relative ordinal. It returns one
exact block-read plan or an explicit rejection.

[`Fat32-File-Read-Plan.wv`](../Operating-System/Services/Fat32-File-Read-Plan.wv)
does not read the FAT, prove cluster-chain membership, issue provider IPC, or
copy bytes into an application grant. The caller must obtain the cluster from
the separately admitted chain and preserve that association when constructing
this request.

## Mapping and bounds

The input geometry must use 512-byte sectors, a power-of-two cluster size from
one through 128 sectors, and the strict compatible FAT32 cluster-count range.
The declared cluster area must fit inside the independently admitted device
extent. File size remains the FAT32 `u32` directory field while positions and
derived device sectors use `u64` arithmetic.

A nonempty request is limited to 65,536 bytes. One ready result is further
bounded by the remaining file bytes, remaining cluster bytes, and a 4,096-byte
block window beginning at the sector offset. Therefore one plan names no more
than eight sectors and never crosses a cluster, the file end, or the block
exchange ceiling. Exact end-of-file is distinct from an offset beyond the file.

The caller supplies the cluster ordinal. It must equal
`file-offset / cluster-bytes`; the cluster must be within the admitted data
cluster range. A ready result carries the exact sector, sector count, leading
sector offset, data-byte count, and next file offset.

## Evidence and limits

The module builds as a 4,543-byte WVB at SHA-256
`71868dc89be5f640ca137b50ba09ccddab3a940706756a209570079f3e2e2b1d`.
Its 16-case test WVB is 10,627 bytes at SHA-256
`7d4397fa3bc9a338ff88af5bebd8008b2ef8497ad902e6e9bafcc0c19ac4fff1`,
returns 47 on Windows, and pins deterministic Windows and Linux console images.

This boundary does not traverse a chain, bind a file reference or media
generation, compare mirrored FATs, perform live block-provider dispatch, copy
partial-sector data, or publish a filesystem response. Those operations remain
separate composition and service contracts.
