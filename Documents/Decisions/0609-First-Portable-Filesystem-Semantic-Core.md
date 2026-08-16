# Decision 0609: First portable filesystem semantic core

- Status: Implemented current-host native candidate; independent Linux execution pending
- Date: 2026-08-15
- Advances: [Decision 0181](0181-Next-Windvale-Os-Mechanism-Contracts.md)
- Contract: [filesystem semantic core](../../Specifications/Windvale-Filesystem-Semantics.md)

## Context

Windvale already has a qualified immutable single-file directory-read
capability and an implemented-candidate pre-opened random-access storage
capability with Windows and Linux native leaves. Neither is a filesystem:
directory-read cannot mutate or retain file references, while random-access
storage has no directory-relative open or path semantics.

The shared filesystem boundary must be defined before host and guest providers
translate it. Inheriting POSIX or Win32 calls would leak different link, path,
sharing, partial-write, durability, and failure behavior into portable code.

## Decision

- Add a capability-free portable semantic core for `Open`, `Read_at`,
  `Write_at`, `Set_length`, `Close`, and `Flush`.
- Retain the qualified one-to-255-byte ASCII single-segment rule and reject
  empty, `.`, `..`, separators, native roots, drive syntax, and non-admitted
  bytes before provider invocation.
- Define four initial no-link open profiles: read existing, read/write existing,
  read/write create-new, and read/write open-or-create.
- Use nonzero generation-safe `u64` file references, `u64` positions and
  lengths, and bounded transfers of at most 65,536 bytes.
- Separate operation status from mutation completion. Writes report exact
  completion, exact nonzero partial progress, or indeterminate completion with
  no replayable progress or authoritative length. Rejection reports no
  completion, progress, or payload.
- Name content flush and content-plus-length flush separately. Neither implies
  directory-entry durability, atomic replacement, or transaction commit.
- Reuse the existing Windows/Linux random-access leaves for eventual file data
  operations, but add directory/open/handle-table policy around them rather
  than relabeling the pre-opened storage capability as a filesystem.

## Evidence and consequences

The focused self-test is a 9,555-byte WVB at SHA-256
`f540ca6a7dbaa6ec1e5e8b48dea081288cdb2f6090ce9432bd226a98bf8d4a9d`.
The shared native backend lowers it to a 62,650-byte WVO and a 61,992-byte flat
image. Deterministic 64,000-byte Windows and 69,744-byte Linux console images
have SHA-256 `f350d86b442a221f5135bd090680f28f976274c702fb75b9ef2e000a5d927194`
and `d1badd6ebdf1a9f28051465ac197474641815ff210b912373f6779ba11a8c705`.
The current Windows image returns 42 across 18 cases, including positions above
4 GiB, `u64` exclusive-end overflow, EOF reads, exact complete and partial
writes, indeterminate mutation, length change, traversal rejection, and
rejection cleanliness.

The core does not claim a provider protocol, host binding, OS service, handle
table, dynamic capability transfer, metadata, enumeration, replacement,
deadline/cancellation, FAT32, block device, or durability qualification. Those
remain explicit later filesystem slices.

## Reconsideration triggers

Broaden segment policy only with a versioned provider capability that reports
normalization, comparison, collision, and traversal behavior. Add stronger
flush, replacement, or retry behavior only with exact crash and indeterminate-
completion semantics. Do not narrow the shared position or length below `u64`.
