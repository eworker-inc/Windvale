# Windvale database hosted tree delete

## Status

- Hosted coordinator: `Libraries/Platform/Database/Durable-Tree-Delete.wv`
- Shared path reader: `Libraries/Platform/Database/Durable-Tree-Path.wv`
- Portable transaction: `Libraries/Database/Tree-Path-Delete.wv`
- Capability: `storage.random_access_v1`
- Evidence: focused Windows native execution; independent Linux execution
  pending

## Boundary

```text
Databaseˉdurableˉtreeˉdelete(Current, Key, Maximumˉactions)
    -> Databaseˉdurableˉtreeˉdeleteˉresult
```

`Current` must be a valid tail-free committed selection at depth two through
eight. `Key` is nonempty and at most 4 KiB. The provider identity and length
must remain equal to the initial description during traversal. Recovery of an
existing tail remains the caller's responsibility.

The shared path reader performs one provider read per level and copies each
borrowed response before the next provider call. It validates page identity,
visibility, kind, decoded count, child bounds, route, and inherited key range,
then passes one owned root-to-leaf path to the portable delete transaction.

## Publication

A missing key returns `Missing`, `Actions = 0`, and `Found = false` without
calling the publication executor or changing provider bytes.

A present key begins the existing four-action publication against the same
provider generation and committed length: append data pages plus log, flush,
write the inactive superblock, and flush again. `Maximumˉactions` bounds one
call. `Committed`, `Active`, `Aborted`, `Recover`, and typed failure remain
distinct. An active or uncertain result never authorizes replay.

The result preserves storage, page-read, durable-page, node, transaction, and
publication errors. It also reports path visits, executed actions, whether the
key was found, resulting root and depth, data-page count, and full publication
state.

## Size and verification

Delete is a focused component instead of being added to the already tight
hosted upsert image. The Windows fixture compiles to a 4,050,276-byte WVO,
leaving 144,028 bytes below the fixed 4 MiB object ceiling.

The fixture starts from generation 2 at 20,992 bytes, deletes key `2`, and
commits generation 3 at 33,280 bytes with root 6 and log 7. It decodes the new
leaf and root, proves key `2` is absent, key `3` and value `30` remain, routing
selects the replacement leaf, and a repeated delete is a byte-preserving
no-op. For action boundaries zero through four, recovery plus restart converges
on the same valid committed generation.

## Exclusions

This milestone does not merge or reclaim pages, update logical collection
records, maintain secondary indexes, batch multiple mutations, coordinate
concurrent writers, listen on a network, or own server lifecycle.
