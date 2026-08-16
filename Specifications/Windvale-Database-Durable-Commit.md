# Windvale Database durable pages and commit publication

## Status

This document specifies the implemented candidate `WVPG 1` physical-page
envelope, `WVCR 1` compact commit record, and the capability-free
single-writer publication planner. Together with the `WVDS 1` dual
superblock, these contracts define the first complete durable-before-publish
byte transition for Windvale Database.

The portable implementations are:

- `Libraries/Database/Durable-Page.wv`;
- `Libraries/Database/Durable-Commit-Record.wv`; and
- `Libraries/Database/Commit-Publication.wv`.

They perform no I/O and hold no storage capability. A later platform owner
must map each planned write and flush to one pre-authorized
`storage.random_access_v1` instance without weakening completion semantics.

## Common rules

All integers use little-endian encoding. Page identity, generation, commit
sequence, storage length, and allocation counts use their complete `u64`
range except where an invariant reserves `u64::MAX` as the no-page sentinel.
Page sizes are restricted to 4,096, 8,192, 16,384, 32,768, or 65,536 bytes.

SHA-256 detects corruption; it does not authenticate the storage object. No
valid checksum can override a failed size, range, identity, generation, or
publication invariant.

## `WVPG 1` physical page

Every database page is exactly the page size recorded by `WVDS 1`. Its first
128 bytes form this envelope:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVPG`, integer `0x47505657` |
| 4 | 4 | version | `1` |
| 8 | 4 | header size | `128` |
| 12 | 4 | flags | zero |
| 16 | 4 | physical page size | one closed supported size and equal to input length |
| 20 | 4 | kind | root `1`, branch `2`, leaf `3`, or commit log `4` |
| 24 | 8 | page identity | not the no-page sentinel |
| 32 | 8 | generation | positive |
| 40 | 8 | commit sequence | zero only for the initial root rule below |
| 48 | 8 | previous page | earlier page identity or no-page sentinel |
| 56 | 4 | payload length | at most `page_size - 128` |
| 60 | 4 | item count | zero when payload is empty; a leaf may also carry one validated nonempty zero-item tree payload |
| 64 | 32 | payload checksum | raw SHA-256 of the used payload bytes only |
| 96 | 32 | header checksum | raw SHA-256 of bytes 0 through 95 |

Payload begins at offset 128. Every byte after the used payload through the
end of the physical page is zero and is checked during decode. Hashing only
the bounded used payload avoids feeding unused multi-kilobyte padding through
SHA-256 while the zero-padding rule still detects noncanonical tail bytes.

Page identity is an allocation identity, not an offset. A non-sentinel
previous page must be less than the current page identity. Sequence zero is
reserved for generation 1, root page 0, root kind, and no previous page. A
commit-log page has a positive sequence, exactly one item, and an exact
256-byte payload. Complete commit-log admission then decodes that payload as
`WVCR 1` and checks its identity, generation, sequence, and page linkage
against the page and selected superblock; page-envelope admission alone does
not promote arbitrary 256-byte payload bytes into a commit.

The envelope does not interpret root, branch, or leaf payload bytes. The
separate [`WVTN 1`](Windvale-Database-Tree-Node.md) contract now defines the
first variable-key leaf and branch payload shape, while `WVPG 1` continues to
own physical identity, generation, checksums, and zero padding.

A leaf alone may have item count zero with a nonempty payload. This admits the
canonical 32-byte zero-entry `WVTN 1` leaf produced by physical deletion.
The tree reader still decodes that inner payload and requires its entry count
to equal the outer zero, so this envelope exception does not admit an invalid
logical node. Root, branch, and commit-log pages retain the ordinary nonempty
payload and positive item-count rule.

## `WVCR 1` compact commit record

One commit record occupies exactly 256 bytes and is stored as the payload of
the last newly allocated commit-log page.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVCR`, integer `0x52435657` |
| 4 | 4 | version | `1` |
| 8 | 4 | declared size | `256` |
| 12 | 4 | flags | zero |
| 16 | 8 | database identity high | combined identity is not all zero |
| 24 | 8 | database identity low | combined identity is not all zero |
| 32 | 8 | generation | exactly previous generation plus one |
| 40 | 8 | committed sequence | greater than previous sequence |
| 48 | 8 | previous generation | positive and not `u64::MAX` |
| 56 | 8 | previous sequence | selected superblock sequence |
| 64 | 8 | new root page | inside the new allocation |
| 72 | 8 | previous log page | selected log head or no-page sentinel |
| 80 | 8 | new log page | final page in the new allocation |
| 88 | 8 | first allocated page | previous committed page count |
| 96 | 8 | allocated page count | at least two: a new root and log page |
| 104 | 8 | resulting page count | exact checked allocation end |
| 112 | 8 | resulting committed length | `512 + page_count * page_size` |
| 120 | 4 | page size | same closed size as the selected database |
| 124 | 4 | root depth | 1 through 128 |
| 128 | 96 | reserved | all zero |
| 224 | 32 | checksum | raw SHA-256 of bytes 0 through 223 |

If previous sequence is zero, previous log page is the no-page sentinel. For
a positive previous sequence it is a real page below the first new page. The
new root and new log are distinct, both lie in the contiguous new extent, and
the log is exactly `resulting_page_count - 1`. All addition and length
arithmetic is checked before fields become trusted.

## Publication agreement

Publication begins from three independently validated values:

1. the currently selected `WVDS 1` superblock;
2. the encoded target `WVDS 1` superblock; and
3. the encoded `WVCR 1` record and validated `WVPG 1` log page that contains
   its exact bytes.

The planner rejects the transition unless all three agree exactly on database
identity, previous and target generations, previous and target sequences,
page size, previous log, first allocation, new root, new log, resulting page
count, committed length, and root depth. The log-page envelope must also agree
with the record's page size, page identity, generation, sequence, previous
log, item count, and all 256 payload bytes. The first transition fixes the
retention floor at sequence 1; this version otherwise holds the selected
retention floor unchanged. The target slot is always the inactive superblock
slot.

The required state sequence is:

```text
Write_pages(current_committed_length, allocated_pages * page_size)
  -> Flush_pages(Content_and_length)
  -> Write_superblock(inactive_slot_offset, 256)
  -> Flush_superblock(Content)
  -> Committed
```

The first flush makes both appended content and the extended object length
durable. The superblock lies inside the existing 512-byte header region, so
the second flush needs content durability. A provider may use a stronger
flush, but it may not reorder or omit either boundary.

## Mutation observations

Every planned action consumes one closed observation and exact progress:

- `Completed` requires the complete planned write length, or zero progress for
  a flush;
- `Rejected` requires zero progress and normally enters `Aborted`; rejection
  of the final superblock flush enters `Recover` because the already-written
  slot may later reach durable storage;
- `Partial` is valid only for a write with progress strictly between zero and
  its full length, and enters `Recover`; and
- `Indeterminate` requires zero reported progress and enters `Recover`.

`Recover` means reopen the storage object and rerun dual-superblock selection.
It is never permission to retry an uncertain mutation. Invalid progress,
partial flushes, or attempts to advance a terminal state fail closed.

The publication planner does not truncate an aborted or uncertain tail. The
connected [storage recovery contract](Windvale-Database-Storage-Recovery.md)
consumes a fresh dual-superblock selection, resizes only its reported
unpublished tail, and flushes content-and-length before another publication
may begin. An indeterminate resize or flush requires another reopen and is
never retried in place.

## Limits and exclusions

- The implemented planner is an executable crash-boundary oracle, not yet a
  capability-bearing writer.
- Root, branch, and leaf payload formats, B+tree operations, transactions,
  reclamation, and checkpointing remain future work.
- This contract does not provide multi-writer fencing, replication,
  encryption, backup replacement, or directory durability.
- SQL is a human-facing layer above typed query and transaction contracts; it
  is neither a page payload nor the machine service protocol.

## Verification

`Projects/Tests/Windvale-Native-Test-Database-Durable-Commit.wvproj` builds one
hosted selector with 12 independently budgeted cases. The focused
`database-durable-commit` owner covers exact encode/decode behavior, truncated
and malformed headers, checksum and padding corruption, semantic field
failures, alternating-slot publication, every planned write and flush stage,
agreement failures, partial progress, rejection, and indeterminate recovery.

The owner compares two WVB builds and two WVO lowerings byte-for-byte,
verifies the WVO, pins linked and Windows/Linux hosted artifact identities,
executes all cases on the local host, and constructs the other host image.
Independent Windows and Linux execution remains a GitHub qualification claim.
