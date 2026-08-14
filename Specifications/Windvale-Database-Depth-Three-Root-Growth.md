# Windvale database depth-three root growth

## Status

- Portable transaction: `Libraries/Database/Depth-Three-Root-Growth.wv`
- Internal split operation: `Libraries/Database/Tree-Branch-Split.wv`
- Logical node codec: `Libraries/Database/Tree-Node.wv`
- Physical pages and publication: `WVPG 1`, `WVCR 1`, and `WVDS 1`
- Evidence: focused portable Windows native execution; independent Linux
  execution pending

## Boundary

This contract performs the first height increase from a committed depth-two
`WVTN 1` tree to depth three. It applies only when an insertion overflows both
the routed leaf and its current branch root. Fits and leaf-only splits remain
owned by the [depth-two upsert](Windvale-Database-Depth-Two-Upsert.md).

The operation has no I/O authority:

```text
Databaseˉdepthˉthreeˉrootˉgrowthˉbegin(Current, Root, Child, Key, Value)
    -> Databaseˉdepthˉthreeˉrootˉgrowth
```

`Current` must be a valid tail-free depth-two selection. `Root` must be the
exact current branch-root page, and routing `Key` through it must select
`Child`. The selected child must be a visible, nonempty leaf whose physical
identity, generation, sequence, item count, and inherited key range are valid.

The transaction rejects an ordinary leaf update with
`Leaf_split_not_required`. After a valid leaf split, it first proves that the
updated root cannot hold the additional separator; otherwise it returns
`Root_split_not_required`. These outcomes prevent this specialized path from
silently replacing the cheaper depth-two transaction.

## Internal branch split

```text
Databaseˉtreeˉbranchˉsplitˉpropagate(
    Input, Previous_child, Separator, Left_child, Right_child, Maximum_payload
) -> Databaseˉtreeˉnodeˉsplitˉresult
```

The operation replaces exactly one selected child with the two children and
separator produced by a lower-level split. It rejects missing or duplicate
selected children, new-child collisions, invalid ranges, malformed branches,
and invalid payload ceilings. The combined branch must overflow and contain at
least three separators so that both resulting branches remain nonempty.

Every interior separator is considered as the promotion candidate. A legal
candidate must leave both encoded branch bodies within the exact payload
ceiling. Selection minimizes the absolute encoded-byte imbalance and uses the
earliest candidate as the deterministic tie break. The promoted separator is
removed from both children: its left child becomes the left branch's rightmost
child, while the original combined rightmost child remains on the right. Both
results are decoded again before success is returned.

## Page allocation and ownership

For a selection with current page count `P`, one successful growth allocates:

| Page | Identity | Previous page | Purpose |
| --- | ---: | ---: | --- |
| left leaf | `P` | routed child | lower half of the leaf split |
| right leaf | `P + 1` | no-page | upper half of the leaf split |
| left branch | `P + 2` | current root | lower half of the root split |
| right branch | `P + 3` | no-page | upper half of the root split |
| new root | `P + 4` | no-page | promoted separator and two branch children |
| commit log | `P + 5` | current log head | publication linkage |

The left replacement leaf uniquely owns the obsolete routed leaf. The left
replacement branch uniquely owns the obsolete root. The new right siblings and
new root have no predecessor because they do not replace distinct committed
pages. The resulting superblock names root `P + 4`, depth `3`, and a page count
advanced by six including the compact log.

Every child identity remains lower than its parent identity. This preserves the
append-only descending graph invariant used by bounded traversal and recovery.

## Publication and recovery

The five data pages are passed to `Databaseˉcommitˉbatchˉbegin` in identity
order. The existing publication contract adds one log page and returns the
unchanged four actions:

1. append all six pages;
2. flush content and length;
3. write the inactive superblock slot; and
4. flush content.

No uncertain mutation is retried. A hosted executor must reopen, select a valid
superblock, and repair any unpublished tail before starting another operation.
Equal admitted inputs produce byte-identical nodes, pages, log, and superblock.

## Verification

The portable fixture grows a full three-separator depth-two root after a routed
leaf split, validates all six allocated identities and predecessor edges,
routes through both levels to every retained and inserted key, decodes the
commit record, and compares a repeated construction byte for byte. It also
rejects leaf-split-not-needed, root-split-not-needed, wrong-child, invalid-range,
wrong-depth, malformed-root, collision, and invalid-ceiling cases.

The focused Windows application compiles 116 functions to a 157,653-byte WVB,
lowers to a 2,889,859-byte WVO, links, packages, and returns `0`. The eight-case
development owner covers tree node, single-leaf, branch split, root split,
depth-two, depth-three, host storage, and host tree-reader recovery. It passes
in 402.638 seconds with compiler and hosted-application caches hit. Cross-host
qualification remains pending.

## Exclusions

This milestone does not update an already depth-three tree, cascade a split
from a non-root internal branch, delete or merge entries, reclaim or reuse
pages, pin snapshots, add concurrent writers, provide range cursors, define
catalog or row codecs, parse SQL, listen on a network, authenticate clients, or
own server lifecycle.
