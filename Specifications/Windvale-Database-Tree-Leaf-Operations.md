# Windvale database leaf delete and bounded range scan

## Status and boundary

This contract adds physical key deletion and bounded ascending range scans to
the portable `WVTN 1` leaf layer. Both operations are capability-free. They do
not publish pages, navigate branches, merge sparse nodes, reclaim storage, or
authorize logical records.

Deletion is a real removal, not a tombstone. A durable caller uses the returned
canonical leaf bytes as a copy-on-write replacement and retains the old leaf
for older committed generations.

## Physical leaf delete

```text
Databaseˉtreeˉleafˉdelete(Input, Key, Maximum_payload)
    -> Databaseˉtreeˉleafˉdeleteˉresult
```

The key is 1 through 4,096 bytes and the payload ceiling is 32 through 65,408
bytes. A nonempty input must be one valid `WVTN 1` leaf within that ceiling.
The generation-1 empty root payload is also accepted as an absent key.

When the key exists, the operation removes exactly its packed key/value entry,
decrements the count, rebuilds the canonical leaf header, and decodes the
result again before returning it. Removing the final entry produces one valid
32-byte zero-entry leaf. When the key is absent, the operation reports
`Found = false` and returns the unchanged immutable input without copying it.

Deletion does not rewrite a branch separator. A separator remains a valid
lower-inclusive partition even when the key that originally supplied it is
removed or its child becomes empty. Merge and separator compaction can improve
space later without changing lookup correctness.

## Ascending leaf scan

```text
Databaseˉtreeˉleafˉscan(
    Input,
    Has_start, Start, Start_inclusive,
    Has_end, End, End_inclusive,
    Limit
) -> Databaseˉtreeˉleafˉscanˉresult
```

The limit is 1 through 500. Present bounds are valid `WVTN 1` keys. An absent
bound has empty bytes and a false inclusive flag. Start must not sort after
end; equal bounds are valid and select the equal key only when both ends are
inclusive.

The operation validates the complete leaf before scanning. It returns up to
the limit in ascending byte-key order. `Entries` is one contiguous borrowed
slice using the existing packed leaf-entry encoding. `Last_key` is also a
borrowed slice and is empty when no row matched. `Has_more` means another
matching entry remains in this leaf; resumption uses `Last_key` as an exclusive
start. `Examined` reports exact leaf entries inspected, including one lookahead
entry used to establish `Has_more` or an end boundary.

Because selected entries are contiguous, scanning allocates no per-row result
objects and copies no key or value bytes. A caller that performs another
provider read must consume or explicitly copy borrowed slices first.

## Durable continuation

[`Databaseˉtreeˉpathˉdeleteˉbegin`](Windvale-Database-Tree-Path-Upsert.md)
now composes physical deletion through an owned root-to-leaf path and emits
one atomic copy-on-write commit. Provider-backed path discovery and
publication remain separate from that portable transaction.

The durable reader first routes the start key to one leaf with inherited
lower-inclusive and upper-exclusive bounds. When the leaf is exhausted, that
upper separator is the inclusive start key for routing the next leaf from the
same committed root. This permits bounded traversal without sibling pointers
or arithmetic successor keys. The
[durable range scanner](Windvale-Database-Durable-Range-Scan.md) now owns this
cross-leaf orchestration and binds every page to one committed database and
provider snapshot. Reverse scans remain the next traversal layer.

## Verification

The focused native fixture covers removal before, between, and after retained
keys; absent-key stability; deterministic bytes; removal of the final entry;
the empty bootstrap root; invalid keys, payload ceilings, node kinds, and
malformed nodes. Scan coverage includes unbounded, inclusive, exclusive,
equal, empty, limited, resumed, malformed, reversed, and invalid-bound cases,
plus exact result bytes, examined counts, and lookahead behavior.
