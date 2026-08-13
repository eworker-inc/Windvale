# Decision 0536: Nested records and database storage recovery

- Status: Implemented candidate
- Date: 2026-08-13
- Contracts: Seed immutable records, native x64 lowering, database storage
  planner 1, ABI 22 unchanged

## Context

The database storage executor needs to retain the complete commit-publication
value while it chunks writes and processes provider observations. Flattening
that value duplicated generation, slot, position, length, and stage fields and
made the language limitation dictate the library API. Reopen also needed an
exact rule for removing bytes after the selected committed length.

## Decision

- Admit immutable record fields whose types are other record identities and
  lower dotted local field paths one segment at a time.
- Reject cyclic native record containment and native flattened widths above
  64 cells. Cache each validated width in the nominal type directory.
- Use deterministic inline native backing: direct cells first, then nested
  child backing depth-first. Nested field reads produce bounded views; copies,
  calls, construction, and returns copy complete flattened backing.
- Embed the complete commit-publication record in the portable storage state.
  Split page writes at 65,536 bytes without advancing commit stage until the
  full append completes.
- After fresh dual-superblock selection, truncate only a positive unpublished
  tail to selected committed length, then require a content-and-length flush.
  Any uncertain resize or flush requires another reopen and must not be
  retried in place.
- Keep ABI 22 unchanged. The planner remains capability-free until a native
  `storage.random_access_v1` service and writer fence have their own contract.

## Consequences

Natural structured compiler and database APIs no longer require flattened
duplicate fields. Cached record widths also remove repeated recursive layout
work: the 97 KB storage-publication fixture lowers locally in about 3.5 seconds
instead of reaching the standalone lowerer's operation ceiling after roughly
52 seconds.

The implementation still does not perform real storage I/O, inject process or
power failure, provide multi-writer fencing, or qualify Linux execution. Those
claims remain behind the native service ABI and dual-host qualification gate.

## Reconsideration triggers

Reconsider the 64-cell native bound when measured applications require wider
immutable values and a bounded allocation/copy strategy exists. Reconsider
inline backing when mutable aggregates, sharing, or independent lifetimes are
specified. Version the recovery contract before adding mutation identities,
idempotent replay, remote storage, reclamation, or directory publication.
