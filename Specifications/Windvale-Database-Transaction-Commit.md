# Windvale database transaction commit publication

## Status

- Version: transaction commit publication 1
- Profile: portable
- Input root depths: 2 through 8
- Maximum tree pages: 792
- Maximum append pages: 793, including one commit-log page
- Evidence: focused segmented Windows native execution; independent Linux
  execution pending

## Purpose

`Databaseˉtransactionˉcommitˉplan` is the first complete portable commit
coordinator for the general transaction tree. It accepts one freshly selected
committed snapshot, one canonical mutation set, and the exact durable path for
every mutation. It composes the existing contracts in this order:

1. `WVTC 1` rebuilds every changed leaf and ancestor and names one completed
   root.
2. The commit batch validates every new `WVPG 1` page and its obsolete-page
   ownership.
3. One following commit-log page carries the compact `WVCR 1` record.
4. The inactive `WVDS 1` superblock names the new root and log head.
5. The existing publication state machine returns the exact ordered storage
   actions required to make that generation current.

The coordinator is capability-free and performs no I/O. The returned
publication state begins at `Write_pages`; a hosted owner must use the
rights-limited random-access storage executor and obey every partial,
rejected, and indeterminate result rule.

## Result and no-op behavior

`Databaseˉtransactionˉcommitˉresult` carries the complete `WVTC 1` result
and the existing commit-batch result. `Publication_ready` is true only when
the tree changed and the batch, compact log, target superblock, and publication
state all validate.

A logical no-op is a successful result with `Changed = false` and
`Publication_ready = false`. It retains the selected root, depth, generation,
and sequence, allocates no pages, creates no log record, and must not dispatch
storage I/O.

`Databaseˉtransactionˉcommitˉvalidate` reruns the complete plan from the
selected snapshot, mutations, and paths. It requires byte-identical tree
manifests, tree pages, append pages, and target superblock plus identical
publication coordinates. This is deliberate deterministic replay, not a
substitute for reopening and recovery after an uncertain mutation.

## Bounds and memory

The commit batch now accepts 1 through 792 consecutive data pages, matching the
hard `WVTC 1` ceiling. One following log page makes the maximum append 793
pages. At the largest admitted 64 KiB page size this is 51,970,048 bytes. All
page-count, byte-length, page-identifier, generation, sequence, and storage
length arithmetic is checked before publication is returned.

Every replacement page names either one older page that it makes obsolete or
the no-page sentinel. The batch rejects an obsolete page outside the selected
snapshot and rejects duplicate ownership inside the same commit.

Earlier code detected duplicate ownership by fully decoding and hashing every
earlier page for every replacement. The current check fully validates each page
once, then reads the fixed `WVPG 1` previous-page field from already validated
earlier pages. The worst case remains a bounded 313,236 scalar comparisons for
792 pages, but it no longer implies more than 20 GB of repeated page hashing at
the 64 KiB ceiling. A future pre-sized page sink and compact ownership set may
reduce both immutable concatenation and quadratic scalar work before large
persistent-server transactions are enabled by default.

## Publication and recovery

A changed result requires exactly the existing ordered protocol:

```text
Write(tree pages + commit-log page)
  -> Flush(Content_and_length)
  -> Write(inactive superblock)
  -> Flush(Content)
  -> Committed
```

A rejected, partial, or indeterminate mutation is never retried blindly.
The caller closes the uncertain path, reopens the storage object, selects the
newest fully valid superblock, and truncates any unpublished tail through the
bounded recovery plan before admitting another writer.

## Verification and next step

The capacity fixture proves that 64 data pages cross the former 63-page limit,
793 pages fail before page-byte validation, deterministic append bytes remain
stable, and duplicate obsolete ownership is rejected. The composed depth-four
fixture turns seven tree pages into an eight-page append, verifies the compact
record, target superblock, exact publication coordinates, deterministic replay,
and logical no-op.

The next milestone is the persistent hosted writer loop: freshly select the
snapshot, gather bounded paths, call this coordinator, execute the four durable
actions, reopen after uncertain results, and expose stable request timing and
memory counters. The fair EWDB, PostgreSQL, and SQLite comparison follows that
persistent path rather than measuring compiler or process startup.
