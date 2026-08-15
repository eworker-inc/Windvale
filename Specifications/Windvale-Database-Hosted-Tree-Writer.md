# Windvale database hosted tree writer

## Status

- Hosted coordinator: `Libraries/Platform/Database/Durable-Tree-Writer.wv`
- Capability: `storage.random_access_v1`
- Portable transaction: `Libraries/Database/Tree-Path-Upsert.wv`
- Evidence: focused Windows native execution; independent Linux execution pending

## Boundary

The hosted operation discovers and publishes one upsert against an exact
committed tree snapshot:

```text
Databaseˉdurableˉtreeˉupsert(Current, Key, Value, Maximumˉactions)
    -> Databaseˉdurableˉtreeˉupsertˉresult
```

`Current` must be a valid, tail-free `WVDS 1` selection with input depth two
through eight. `Key` is nonempty and at most 4 KiB. The selected committed
length must equal the provider's described length before traversal. The
operation never repairs a tail or silently retries an uncertain mutation; its
caller must enter the existing recovery contract first.

## Traversal and ownership

The coordinator performs exactly one provider-backed visit per tree level.
Every visit requires the provider generation and storage length to remain
equal to the initial description. It validates physical page identity, size,
visibility, root generation and sequence, expected root/branch/leaf kind,
decoded item count, descending child identity, committed page bounds, selected
route, and inherited leaf range.

Provider-returned bytes are borrowed until the next provider call. Each page
is copied before traversal continues. After concatenating the exact root-to-
leaf sequence, the coordinator creates one additional non-tail-reusable owned
view. This final ownership barrier prevents the portable transaction's
builders from aliasing a slice of their input path.

## Transaction and publication

The owned path is passed to `Databaseˉtreeˉpathˉupsertˉbegin`. On success, the
coordinator begins the existing four-action storage publication against the
same provider generation and committed length and executes at most
`Maximumˉactions`: append pages and log, flush, write the inactive superblock,
then flush again.

The result reports visited path pages, actions executed, replacement and split
metadata, resulting root and depth, data-page count, and the complete
publication state. `Committed`, `Active`, `Aborted`, and `Recover` remain
distinct. An active or uncertain result is not permission to repeat the
application mutation.

Typed failure distinguishes invalid current state or key, unsupported depth,
storage failure, changed snapshot, provider page failure, malformed durable
page, invalid node or graph, portable transaction rejection, and publication
failure while preserving the relevant lower-layer errors.

## Verification

The hosted writer fixture starts from the committed depth-two generation
created by the independent hosted reader target. It inserts key `4` with value
`40`, checks two provider visits, a nonsplitting two-page replacement, root 6,
log 7, generation 3, sequence 2, and committed length 33,280. It decodes the
new leaf and root, checks predecessor ownership, key/value lookup, and routing,
then proves a stable reopen does not change bytes.

For each publication boundary zero through four, the fixture reopens a marked
tail, runs recovery, republishes with the selected action bound, restarts, and
converges on the same committed generation. The Windows database development
owner passes eleven targets. Paired-host qualification remains pending.

## Exclusions

This milestone does not own server listening, sessions, authentication,
catalogs, row codecs, query planning, concurrent writers, delete/merge,
reclamation, range cursors, snapshot pinning, or automatic retry.
