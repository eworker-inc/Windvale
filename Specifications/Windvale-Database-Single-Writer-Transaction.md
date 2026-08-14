# Windvale database single-writer transaction

## Status

- Version: single-writer transaction 1
- Portable builder: `Libraries/Database/Single-Writer-Commit.wv`
- Hosted executor: `Libraries/Platform/Database/Durable-Storage-Executor.wv`
- Durable formats: `WVDS 1`, `WVPG 1`, and `WVCR 1`
- Storage capability: `storage.random_access_v1`
- Evidence: focused native Windows execution and restart recovery; independent
  Linux execution pending

## Boundary

This contract is the first complete bounded write through Windvale-owned
database code. It turns one freshly selected committed generation and one
opaque root payload into immutable root and commit-log pages, a target
superblock, and an exact publication plan. A separate hosted executor maps the
plan and the existing tail-recovery plan to one pre-authorized random-access
storage object.

The builder is portable and has no I/O authority. The executor is hosted and
declares exactly `storage.random_access_v1`. Neither layer receives a native
path or host handle. This slice is one transaction over one storage object,
not a database server, SQL engine, general B+tree, page cache, or concurrent
transaction manager.

## Builder contract

```text
Databaseˉsingleˉwriterˉcommitˉbegin(
    Current: Databaseˉsuperblockˉselection,
    Rootˉitemˉcount: u32,
    Rootˉpayload: bytes
) -> Databaseˉsingleˉwriterˉcommit
```

`Current` must be the valid result of selecting the two current `WVDS 1`
slots against the freshly described storage length. It must name slot one or
two, have no unpublished tail, and have root depth one. The builder rejects
generation, sequence, page-identity, or storage-length exhaustion before
constructing bytes.

One successful call allocates exactly two append-only pages:

1. page `Current.Page_count` is a `WVPG 1` root containing the supplied item
   count and payload;
2. the following page is a `WVPG 1` commit-log page containing one `WVCR 1`
   record;
3. generation and committed sequence each increase by one;
4. the commit record links the old generation, sequence, root, and log head to
   the two allocated pages;
5. the inactive `WVDS 1` slot names the new root and log head; and
6. committed length increases by exactly two page sizes.

The result contains the complete two-page append, the exact 256-byte target
superblock, allocated page identities, target generation, target sequence,
target length, and the validated commit-publication state. Every expected
failure is a typed result. No partial result is publishable.

The transaction builder continues to treat its root payload as opaque and
requires only that it fit the selected `WVPG 1` envelope and agree with its
item count. The separate [`WVTN 1`](Windvale-Database-Tree-Node.md) composition
now supplies the first root-depth-one variable-key leaf upsert. Branch splits,
rows, and catalogs remain separate contracts.

## Hosted execution

The hosted executor translates only the existing typed action vocabulary:

- `Write` calls `Storageˉwriteˉat`;
- `Resize` calls `Storageˉresize`;
- `Flush(Content)` and `Flush(Content_and_length)` call the corresponding
  storage flush class; and
- every provider result becomes one exact publication observation, including
  rejection, partial progress, stale generation, and indeterminate completion.

Both publication and recovery executors take an explicit maximum action count.
They stop when that budget is exhausted or the state becomes terminal and
return the exact number of dispatched actions. This makes interruption points
observable without adding a hidden retry loop.

For the current two-page commit, four successful actions are required:

```text
Write(root page + commit-log page)
  -> Flush(Content_and_length)
  -> Write(inactive superblock)
  -> Flush(Content)
  -> Committed
```

A partial or indeterminate mutation is never retried automatically. The caller
must close the failed execution path, reopen the storage object, freshly
describe it, reread both superblocks, select a valid generation, and run the
bounded recovery policy. Recovery accepts only a completely valid old or new
generation; bytes beyond the selected committed length are an unpublished tail
and are truncated and length-flushed before another transaction begins.

## Borrowed response lifetime

`storage.random_access_v1` returns a provider-owned response buffer. The typed
scalar fields copied out of that response remain ordinary values, but a
successful read's `Storage_result.Value` is a borrowed view into the shared
provider response scratch. It remains valid only until the next call to the
same bound storage capability, regardless of that next operation's kind.

A caller must decode, validate, or copy the read payload before issuing another
storage call. It must not retain two read payload views across calls and treat
them as independent snapshots. The hosted transaction fixture deliberately
decodes and validates the root page before reading the commit-log page.

## Verification

The portable self-test compiles and lowers through the current Windvale-native
compiler and validates:

- exact allocated page identities, generation, sequence, and committed length;
- the root page, commit-log page, compact commit record, and inactive
  superblock fields;
- deterministic page and superblock bytes across equal inputs; and
- invalid selection, unpublished tail, unsupported depth, counter and length
  exhaustion, oversized payload, and inconsistent item-count rejection.

The hosted native fixture executes a real Windows provider and injects process
termination after zero through four completed publication actions. Restart
must select and validate only the old 4,608-byte generation or the new
12,800-byte generation. Interruptions before superblock publication recover
the old generation, completion recovers the new generation, and the boundary
after the superblock write accepts either fully valid generation. Every path is
followed by a stable reopen. The equivalent Linux application is constructed;
independent Linux execution is still required before cross-host conformance is
claimed.

## Exclusions and next contracts

This version has no delete, update-in-place, page reclamation, branch split,
multi-page tree traversal, row encoding, catalog, schema, transaction
isolation, concurrent reader lifetime, group commit, authentication, network
listener, client protocol, SQL parser, query planner, or operator execution.
It does not claim hardware power-loss qualification. The next storage-kernel
step is bounded provider-backed tree reading followed by leaf split and branch
root creation over this publication boundary.
