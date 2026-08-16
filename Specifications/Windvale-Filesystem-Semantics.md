# Windvale filesystem semantic core

## Status and scope

Filesystem semantics 1 is the portable operation and result-validation core for
the first shared filesystem capability. It defines directory-relative opening,
explicit-offset reads and writes, length changes, close, and two flush classes
without inheriting Win32, POSIX, NTFS, or ext4 behavior. It is implemented
candidate behavior; provider IPC, dynamic handle tables, host binding, and guest
service integration remain successor slices.

The core deliberately reuses the proven single-segment policy from
`filesystem.directory_read_v1`: one to 255 ASCII bytes drawn from letters,
digits, `-`, `.`, and `_`, excluding `.` and `..`. Native separators, absolute
paths, drive names, device names, file descriptors, handles, and traversal do
not cross the contract.

## Operations

The operation identities are `Open = 1`, `Read_at = 2`, `Write_at = 3`,
`Set_length = 4`, `Close = 5`, and `Flush = 6`.

Open has four exact initial profiles:

1. read an existing regular file without following links;
2. read and write an existing regular file without following links;
3. create a new read/write regular file, failing on collision, without links;
4. open or create a read/write regular file without following links.

An accepted open returns a nonzero generation-safe file reference. Every later
operation supplies that reference. Reads and writes use `u64` positions and
bounded chunks of at most 65,536 bytes. `Set_length` uses a `u64` length. Close
is explicit. Flush distinguishes content from content-and-length; neither name
promises directory-entry durability or atomic replacement.

## Mutation outcomes

Statuses distinguish success, missing/colliding/wrong-kind objects,
authorization failure, provider unavailability, revocation, stale references,
peer exit, unsupported behavior, invalid requests, and invalid responses.

Mutation completion is separately one of:

- `Completed`: the exact requested write progress or length change occurred;
- `Partial`: a write reports nonzero exact progress below the request;
- `Indeterminate`: the provider cannot prove whether mutation occurred and
  therefore reports zero replayable progress and no authoritative length; or
- `None`: no mutation completion applies.

An indeterminate mutation must not be retried automatically. Rejection requires
zero progress, `None`, and no payload. Successful reads return exactly
`min(maximum, file_length - position)` bytes. Successful completed and partial
writes prove the resulting length covers the exact reported extent. Checked
addition rejects a write whose exclusive end would exceed `u64`.

## Evidence and limits

[`Filesystem-Semantics.wv`](../Libraries/Platform/Filesystem/Filesystem-Semantics.wv)
is capability-free portable policy. The focused 9,555-byte self-test WVB has
SHA-256 `f540ca6a7dbaa6ec1e5e8b48dea081288cdb2f6090ce9432bd226a98bf8d4a9d`.
The shared native lowerer produces the exact 62,650-byte WVO and deterministic
Windows and Linux console images; the current host executes to result 42.
Eighteen cases cover names, traversal, open profiles, above-4-GiB positions,
`u64` overflow, EOF reads, complete/partial/indeterminate writes, length change,
and rejection invariants.

This core does not itself open a host file, grant a capability, define the IPC
envelope, enumerate directories, follow links, rename, remove, atomically
replace, append, map, watch, lock, expose permissions, parse FAT32/NTFS/ext4, or
provide durability beyond the named flush class. Windows and Linux providers
must translate this contract through their native kernels; they do not expose
native filesystem semantics as Windvale semantics.
