# Windvale database engine lifecycle

## Status

- Hosted lifecycle: `Libraries/Platform/Database/Durable-Database-Engine.wv`
- Read projection: `Libraries/Platform/Database/Durable-Tree-Reader.wv`
- Write projection: `Libraries/Platform/Database/Durable-Tree-Writer.wv`
- Capability: `storage.random_access_v1`
- Evidence: focused Windows native execution; independent Linux execution pending

## Engine state

The lifecycle operation opens one prebound storage resource and returns an
immutable engine snapshot:

```text
Databaseˉengineˉopen(Maximumˉrecoveryˉactions) -> Databaseˉengine
```

The snapshot reports lifecycle status, provider status, provider generation
and length, recovery stage and error, exact recovery actions, and one complete
`Databaseˉsuperblockˉselection` named `Current`.

`Ready` means the selected committed length exactly equals the provider length
and no mutation was performed. `Recovered` means a tail was resized and
flushed, the resource was described and read again, and the reopened selection
is exactly the same committed database identity, generation, sequence, root,
log, page count, page size, depth, and committed length with zero tail.

`Recoveryˉactive` preserves a definite resize or flush action still to perform.
`Reopenˉrequired` preserves a stale or indeterminate observation and does not
claim that the committed state remains current. Neither state authorizes
application-mutation replay.

## Open and recovery

Open performs these bounded steps:

1. describe the storage resource and require a nonzero provider generation;
2. read the exact 512-byte durable header at that generation;
3. require header and description generation/length agreement;
4. select the newest valid `WVDS 1` slot;
5. return `Ready` when the selection has no tail; otherwise begin recovery;
6. execute at most `Maximumˉrecoveryˉactions`; and
7. after completed recovery, re-describe, reread, reselect, and prove the exact
   committed selection did not change.

Zero actions never hides pending recovery. One completed resize still reports
the required flush. A provider rejection becomes `Recoveryˉfailure`; stale or
indeterminate completion becomes `Reopenˉrequired`; and a changed reopened
selection becomes `Changedˉstorage`.

## Read and write projections

The engine snapshot is the common lifecycle contract for the existing hosted
operations. A read projection passes `Engine.Current` to
`Databaseˉdurableˉtreeˉlookup`; a single-writer projection passes the same
selection to `Databaseˉdurableˉtreeˉupsert`. Only `Ready` or `Recovered`
snapshots may be projected. A committed write is followed by a fresh engine
open before another operation uses the resource.

The projections remain separate static native targets. The current writer
closure is already close to the ordinary 4 MiB complete-object ceiling, and a
combined reader/writer object exceeds it. This physical segmentation does not
change the common engine state or database semantics and avoids treating an
object-limit increase or premature dynamic linker as part of the database API.

## Failure vocabulary

Typed status distinguishes storage failure, header read failure, no valid or
conflicting current selection, changed provider observation, stopped recovery,
and reopen-required uncertainty. Lower-layer provider and recovery errors are
preserved. Failure does not fabricate a valid current snapshot.

## Verification

The focused engine fixture opens the committed depth-two generation without
changing bytes and performs a two-page lookup through `Engine.Current`. It
then covers zero-action recovery at resize, one-action recovery at the required
flush, completed two-action tail recovery with byte-identical convergence, and
truncated-header rejection. The dedicated target compiler-aligns 146 functions
and lowers to a 3,439,032-byte ordinary object.

The Windows database development owner passes twelve targets. The Linux image
is constructed by the complete owner, but independent execution and the cold
paired-host retirement gate remain qualification evidence.

## Exclusions

This contract does not create a database, grant storage authority, pin a
snapshot, coordinate multiple writers, assign client sessions, define records
or catalogs, parse queries, listen on a network, authenticate peers, reclaim
pages, or retry uncertain mutations.
