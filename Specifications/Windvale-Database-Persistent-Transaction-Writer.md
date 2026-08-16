# Windvale database persistent transaction writer

## Status and purpose

The persistent transaction writer is the first hosted Windvale component that
keeps one database writer session across requests and publishes a complete
multi-record transaction. It composes the canonical `WVTM 1` mutation set,
provider-backed root-to-leaf path discovery, transaction-tree completion, one
commit batch, and the random-access storage publication executor.

This is a bounded single-writer component, not yet a network server or a
concurrent queue. One owner must serialize access to a session and must not
reuse an older immutable session value after accepting a later result.

## Durable write

`Databaseˉdurableˉtransactionˉwrite` accepts a valid committed superblock
selection, one canonical mutation value, and a maximum publication-action
budget. It:

1. decodes and validates the complete mutation set before I/O;
2. describes the storage object and requires its length to equal the selected
   committed length;
3. reads one complete path for each mutation against that provider generation;
4. validates the combined paths and their shared-page agreement;
5. constructs the complete transaction tree and commit publication;
6. returns without provider mutation for a logical no-op; or
7. executes the existing ordered publication state machine.

The result preserves the lower-layer storage, path, page, node, transaction,
and publication errors. It also reports mutation count, unique changed leaves,
path visits and bytes, publication actions, generated pages, appended bytes,
and target generation, sequence, and length. The persistent session derives
the complete logical provider-call count from describes, path reads, and
publication actions.

A completed changed transaction uses the existing four durable actions:

```text
append tree and log pages
  -> flush content and length
  -> write the inactive superblock
  -> flush content
```

Rejected, partial, indeterminate, or changed-snapshot results are never retried
blindly. The owner must reopen the storage object and run bounded recovery when
required before admitting another mutation.

## Persistent session

`Databaseˉpersistentˉwriterˉopen` creates a session at request sequence one
only from a valid committed selection with depth one through eight and no
unpublished tail. `execute` accepts exactly the next nonzero request sequence
while the session is ready. `finish` consumes that result once, advances the
request sequence, updates counters, and returns one of these useful states:

- `Ready` after a no-op or a safely rejected request;
- `Reopen_required` after a commit, uncertainty, storage failure after possible
  mutation, or any result that performed publication actions;
- `Failed` for invalid session/result sequencing; or
- `Closed` after explicit close.

A successful commit requires the reopened selection to advance both generation
and committed sequence while preserving database identity and page size. The
caller tells `reopen` whether recovery occurred and how many recovery actions
were used. Reopen and recovery I/O stay owned by the existing lifecycle
component rather than being hidden inside this session.

## Bounds and measurements

One request admits the existing `WVTM 1` maximum of 32 mutations and a selected
tree depth of at most eight. The writer rejects more than 4 MiB of gathered
path bytes before provider reads and rejects a planned result whose counted
live logical byte values exceed 4 MiB. The reported retained-byte high-water
mark is this deterministic logical-payload bound; it is not allocator-resident
memory or process RSS. Benchmark tooling must measure process memory separately.

The session accumulates requests, commits, no-ops, rejections, reopen demands,
reopens, recoveries, recovery actions, provider calls, path visits, read bytes,
publication actions, append bytes, and retained-byte high water. Timing uses
caller-supplied monotonic ticks. The session records only differences between
start and finish, so the eventual server or benchmark wrapper must name the
clock and tick unit.

Saturating counters never wrap. Request-sequence exhaustion is rejected. The
current byte construction remains immutable and can create transient copies;
the logical retained bound does not claim an optimized allocation strategy.

## Verification and exclusions

The focused hosted fixture starts from a recovered depth-two generation,
updates one record in each leaf as one atomic transaction, verifies the exact
two-leaf path/read/action counters, reopens and reads both committed values,
then repeats the same transaction and proves that it performs no publication.
It also proves request sequencing, generation advancement, tick accounting,
provider-call accounting, append-byte accounting, retained-byte high water,
and close.

The contract does not yet provide concurrent readers, a writer queue, group
commit, cancellation, secondary-index bundle discovery or unique-check execution,
query execution, page reclamation, process supervision, transport,
authentication, or authorization.
Those remain separate milestones over this durable single-writer boundary.
