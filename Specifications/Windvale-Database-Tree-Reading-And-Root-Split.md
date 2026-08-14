# Windvale database tree reading and root split

## Status

- Logical node format: `WVTN 1`
- Physical and publication formats: `WVPG 1`, `WVCR 1`, and `WVDS 1`
- Portable owners: `Libraries/Database/Tree-Node.wv`,
  `Commit-Batch.wv`, and `Root-Split-Upsert.wv`
- Hosted owner: `Libraries/Platform/Database/Durable-Tree-Reader.wv`
- Storage authority: one pre-bound `storage.random_access_v1` instance
- Evidence: portable native execution and focused Windows provider/restart
  execution; independent Linux execution pending

## Boundary

This contract adds the first provider-backed lookup through a durable
multi-page tree and the first copy-on-write transition from a full root leaf to
a branch root with two leaf children. It does not introduce another on-disk
format. Every logical node remains `WVTN 1`, every physical page remains
`WVPG 1`, the commit log remains `WVCR 1`, and publication still selects one
alternate `WVDS 1` superblock.

The reader owns storage access and global graph validation. The portable split
and commit modules own deterministic bytes but have no I/O authority. The
current mutation is deliberately one root split, not general split propagation,
merge, delete, reclamation, or concurrent transaction processing.

## Bounded durable lookup

```text
Databaseˉdurableˉtreeˉlookup(
    Current: Databaseˉsuperblockˉselection,
    Key: bytes
) -> Databaseˉdurableˉtreeˉlookupˉresult
```

The input selection must be valid, the key must contain 1 through 4,096 bytes,
and root depth must be 1 through 32. A rejected preflight performs no provider
call. The selected root may be the generation-1 empty bootstrap root; that one
case returns `Missing` after one validated page visit.

Every ordinary visit performs one exact page read through
`Randomˉaccessˉdatabaseˉpage`, then proves:

- the requested page identity is below the selected page count;
- provider generation and storage length remain equal across the complete
  lookup, and storage length is not shorter than committed length;
- `WVPG 1` checksum, padding, page size, and page identity are valid;
- page generation and sequence do not exceed the selected snapshot;
- the root page exactly matches the selected generation and sequence;
- outer root, branch, and leaf kinds agree with the remaining depth;
- the `WVTN 1` entry count agrees with the physical page item count; and
- every leaf key is inside the inherited lower-inclusive, upper-exclusive
  range.

A branch separator is the first key admitted by its right child. Equality
therefore routes right. Each route returns owned copies of its next lower and
upper boundaries before another provider call can invalidate the borrowed page
response. Every child identity must be strictly less than its parent identity
and below the selected page count. Under append-only bottom-up allocation this
single rule makes a routed path acyclic and rejects forward, self, and
out-of-range references.

The result distinguishes `Found`, `Missing`, invalid selection/key/depth,
provider/page failure, invalid physical page, invalid logical node, invalid
graph, and provider snapshot change. It reports the exact page visits and final
page identity. A found value is borrowed from the final provider response and
remains valid only until the next call on the same capability instance.

## Portable routing and split planning

The `WVTN 1` portable owner adds:

```text
Databaseˉtreeˉbranchˉtwoˉchildren(Separator, Left, Right, Maximum)
Databaseˉtreeˉbranchˉroute(Node, Key, Has_lower, Lower, Has_upper, Upper)
Databaseˉtreeˉleafˉrangeˉvalidate(Node, Has_lower, Lower, Has_upper, Upper)
Databaseˉtreeˉleafˉsplitˉupsert(Node, Key, Value, Maximum)
```

Split-upsert first constructs the complete ordered replacement or insertion.
It examines every contiguous split boundary whose two encoded leaves fit the
exact payload ceiling, chooses the smallest encoded byte imbalance, and breaks
ties by the earliest boundary. Both leaves are nonempty and separately
revalidated. The separator is an owned copy of the first right-leaf key. A
single entry that cannot fit a page remains `Full`; a valid multi-entry input
with no legal division returns `Cannotˉsplit`.

## Bounded commit batch

```text
Databaseˉcommitˉbatchˉbegin(
    Current,
    Root_page,
    Root_depth,
    Data_page_count,
    Data_pages
) -> Databaseˉcommitˉbatch
```

The portable batch accepts 1 through 63 exact, contiguous data pages and adds
one commit-log page. It requires a tail-free valid current selection, root
depth 1 through 128, consecutive page identities beginning at the current page
count, one root page inside that range, the next generation and sequence on
every data page, and no second root or embedded commit-log page. Counter,
identity, and length growth are checked before byte construction.

Success constructs one `WVCR 1` record, one `WVPG 1` log page, the inactive
`WVDS 1` superblock, and the existing publication state. All data pages plus
the log page are returned as one contiguous append. The four-action durability
protocol is unchanged regardless of data-page count:

```text
Write(all data pages + log page)
  -> Flush(Content_and_length)
  -> Write(inactive superblock)
  -> Flush(Content)
  -> Committed
```

## First root split

`Databaseˉrootˉsplitˉupsertˉbegin` accepts only a tail-free depth-one selected
generation and its exactly matching nonempty root leaf. It calls ordinary leaf
upsert first and proceeds only when that operation returns `Full`.

One successful transaction appends, in order:

1. the left leaf;
2. the right leaf;
3. a branch root whose one separator names the left child and whose header
   names the right child; and
4. the commit-log page produced by the batch.

All three data pages link `Previous_page` to the old root. The new superblock
has depth two and names the branch root. The old generation remains immutable,
and equal inputs produce equal split, page, log, and superblock bytes.

## Verification

Portable fixtures cover unsigned and prefix ordering, inherited ranges,
separator equality, owned route boundaries, valid/invalid branch children,
all legal split positions, deterministic byte balancing, replacement,
oversized entries, exact capacity, batch page identities/kinds, root inclusion,
commit linkage, superblock depth, and repeat-byte equality.

The hosted fixture publishes three data pages plus one log page into the real
Windows provider, reopens a 20,992-byte depth-two generation, and performs
two-visit lookups in both leaves plus one missing lookup. It rejects invalid
keys and a forged page-count graph before I/O. Separate processes interrupt
after zero through four publication actions; every restart first removes an
unpublished tail when required, then reaches the exact committed generation
and a stable byte-identical reopen. The paired Linux image is constructed but
requires independent execution before cross-host conformance is claimed.

## Exclusions and next contracts

The reader has no page cache and performs at most one read per tree level.
There is no general non-root split propagation, branch rewrite, delete, merge,
overflow value, sibling scan, range cursor, page reclamation, snapshot pin,
concurrent writer, group commit, row/catalog codec, SQL layer, network listener,
or server lifecycle. The next storage-kernel milestone should make depth-two
upsert and split propagation general while defining obsolete-page ownership;
catalog and SQL work should consume that engine contract rather than bypass it.
