# Windvale database depth-two upsert

## Status

- Portable transaction: `Libraries/Database/Depth-Two-Upsert.wv`
- Logical node operations: `Libraries/Database/Tree-Node.wv`
- Physical pages and ownership copy: `Libraries/Database/Durable-Page.wv`
- Publication builder: `Libraries/Database/Commit-Batch.wv`
- Evidence: portable native execution plus focused Windows publication,
  interruption, recovery, and stable reopen; independent Linux execution pending

## Boundary

This contract updates one routed leaf in an already committed depth-two
`WVTN 1` tree. It preserves copy-on-write generations and the existing
four-action `WVPG 1` / `WVCR 1` / `WVDS 1` publication protocol. It has no I/O
authority; a hosted caller supplies freshly decoded pages and executes the
returned publication plan through a separately bound storage capability.

```text
Databaseˉdepthˉtwoˉupsertˉbegin(Current, Root, Child, Key, Value)
    -> Databaseˉdepthˉtwoˉupsert
```

`Current` must be a valid, tail-free depth-two selection. `Root` must be its
exact current branch-root page. Routing the requested key must select `Child`,
whose page identity is below both the root identity and selected page count.
The child must be a visible leaf with the selected page size, an exact physical
identity, generation and sequence no newer than the selection, a nonempty
canonical node, and keys inside the inherited lower-inclusive,
upper-exclusive route.

## Ordinary replacement

If the leaf upsert fits, a selection with current page count `P` allocates:

| Page | Identity | Previous page | Purpose |
| --- | ---: | ---: | --- |
| replacement leaf | `P` | routed child | new leaf contents |
| replacement root | `P + 1` | current root | branch with one child rewritten |
| commit log | `P + 2` | current log head | publication linkage |

The replacement root preserves every separator and untouched child identity.
Exactly one selected entry child or the rightmost child must equal the routed
child, and the replacement identity must not already occur in the branch.

## Child split propagation

If ordinary upsert returns `Full`, deterministic leaf split-upsert supplies a
left payload, right payload, and the first right-leaf key as separator. The
transaction allocates:

| Page | Identity | Previous page | Purpose |
| --- | ---: | ---: | --- |
| left replacement | `P` | routed child | lower split half |
| new right leaf | `P + 1` | no-page | upper split half |
| replacement root | `P + 2` | current root | inserted separator and two children |
| commit log | `P + 3` | current log head | publication linkage |

The branch operation supports any selected entry child and the rightmost
child. It preserves canonical separator order, inherited ranges, and all
unselected children. A root that cannot hold the additional separator returns
  `Branch_full`; the separate [depth-three root-growth
  contract](Windvale-Database-Depth-Three-Root-Growth.md) consumes that exact
  boundary only when both the leaf and root overflow.

## Obsolete-page ownership

`WVPG 1`'s `Previous_page` field is the implemented ownership edge for pages
made obsolete by one commit. The replacement leaf owns the old routed leaf,
and the replacement root owns the old root. A newly created right split leaf
has no predecessor because it replaces no committed page.

`Databaseˉcommitˉbatchˉbegin` requires every non-no-page predecessor to name a
page below the old committed page count and rejects duplicate predecessor
identities inside the same batch. The transaction result reports the same two
obsolete identities explicitly. This establishes accountable ownership but
does not reclaim or reuse pages; append-only descending child identities remain
unchanged.

## Borrowed storage values

`storage.random_access_v1` returns one provider-owned response buffer. A
multi-page caller must own the root before reading the child. The portable
`Databaseˉdurableˉpageˉownedˉcopy` operation materializes exact owned page
bytes and re-decodes every checksum, header, payload, and padding invariant.
Ordinary one-page-at-a-time traversal remains borrowed and zero-copy.

Lookup results are borrowed for the same reason. A caller must inspect or copy
one value before the next storage operation; retaining several lookup results
does not retain several provider snapshots.

## Publication and recovery

Both paths return one contiguous data-page batch, one generated log page, the
inactive superblock bytes, and the existing four-action plan:

1. append all new pages;
2. flush content and length;
3. write the inactive superblock slot;
4. flush content.

Stopping after zero through four actions is recoverable by reopening, selecting
the newest complete superblock, removing any unpublished tail, and only then
starting another transaction. An uncertain mutation is never replayed without
that recovery sequence.

## Exclusions

This milestone does not itself implement root growth. Its successor implements
the first depth-two-to-depth-three transition, but neither contract updates an
existing depth-three tree or cascades a split from a non-root internal branch.
Delete, merge, page reclamation or reuse, snapshot pins, concurrent writers,
group commit, range cursors, catalogs, row codecs, SQL, networking,
authentication, and server lifecycle remain excluded.
