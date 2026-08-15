# Windvale database existing depth-three upsert

## Status

- Portable transaction: `Libraries/Database/Depth-Three-Upsert.wv`
- Logical operations: `Libraries/Database/Tree-Node.wv` and
  `Libraries/Database/Tree-Branch-Split.wv`
- Physical pages and publication: `WVPG 1`, `WVCR 1`, and `WVDS 1`
- Evidence: focused portable Windows native execution; independent Linux
  execution pending

## Boundary

This contract updates one routed leaf inside an existing committed depth-three
`WVTN 1` tree. The caller supplies the selected root, selected internal branch,
and selected leaf as owned decoded `WVPG 1` results:

```text
Databaseˉdepthˉthreeˉupsertˉbegin(
    Current, Root, Branch, Leaf, Key, Value
) -> Databaseˉdepthˉthreeˉupsert
```

The transaction is capability-free and performs no I/O. `Current` must be a
valid tail-free depth-three selection. `Root` must be the exact selected root;
routing `Key` through it must select `Branch`; routing through `Branch` under
the inherited root bounds must select `Leaf`. Every physical page must agree
with the selected page size, generation, committed sequence, item count, and
descending child-identity rule. The selected leaf must be nonempty and all of
its keys, plus `Key`, must be within the inherited lower-inclusive and
upper-exclusive range.

## Bounded propagation

The operation handles four deterministic outcomes:

1. rewrite a leaf that still fits, then rewrite its branch and root;
2. split a full leaf, insert its separator into a branch that still fits, then
   rewrite the root;
3. split the leaf and full branch, insert the promoted branch separator into a
   root that still fits; or
4. split the leaf, branch, and full root, then create a new depth-four root.

Each rewrite or split replaces exactly the routed child. Untouched separators
and child identities remain byte-for-byte canonical. Leaf and branch split
boundaries retain the established minimum encoded-byte imbalance and earliest
legal-boundary tie break. Separator equality routes right.

Propagation is deliberately bounded to the supplied three levels. It is not a
general recursive tree mutator and does not accept a dynamic path. A future
depth-four update contract must supply and validate the additional selected
level rather than weakening this operation's explicit input or allocation
bounds.

## Allocation and predecessor ownership

For current page count `P`, pages are appended bottom-up and the compact commit
log follows the last data page.

| Outcome | Data pages in identity order | Log identity | Result depth |
| --- | --- | ---: | ---: |
| leaf rewrite | leaf `P`, branch `P+1`, root `P+2` | `P+3` | 3 |
| leaf split | leaves `P..P+1`, branch `P+2`, root `P+3` | `P+4` | 3 |
| branch split | leaves `P..P+1`, branches `P+2..P+3`, root `P+4` | `P+5` | 3 |
| root split | leaves `P..P+1`, branches `P+2..P+3`, root children `P+4..P+5`, new root `P+6` | `P+7` | 4 |

Exactly three data pages own committed predecessors in every outcome:

- the left or replacement leaf owns the supplied leaf;
- the left or replacement branch owns the supplied branch; and
- the left or replacement root owns the supplied root.

Right siblings and a newly created root have no predecessor because they do
not replace a distinct committed page. This preserves the one-owner rule used
by future reclamation without claiming that reclamation itself exists.

All child identities are below their parents. The implementation checks page
identifier arithmetic before constructing any page and passes between three
and seven data pages to the existing 63-page commit batch.

## Publication and crash boundary

`Databaseˉcommitˉbatchˉbegin` validates the ordered encoded pages, appends one
compact log, constructs the inactive superblock slot, and returns the unchanged
four-action publication:

1. append all data pages and the log;
2. flush content and length;
3. write the inactive 256-byte superblock slot; and
4. flush content.

The focused model executes the ordinary update through all four stages and
requires an indeterminate page append or a rejected final flush to enter
`Recover`. It never retries an uncertain mutation. Hosted reopen continues to
use the shared storage recovery contract: select valid superblock evidence,
remove any unpublished tail, flush content and length, and only then begin a
new transaction.

No serialized format, publication action, or provider contract changes in this
milestone. Equal admitted inputs produce byte-identical nodes, pages, log, and
superblock.

## Result and rejection

Success reports whether the key was replaced and which of leaf, branch, and
root split. It reports the resulting root depth, every allocated identity, the
three obsolete identities, and the complete commit batch.

Typed rejection distinguishes invalid current selection, malformed or
mismatched root/branch/leaf pages, invalid routing, inherited-range failure,
leaf or branch update failure, generation/sequence/page exhaustion, page
encoding failure, and commit-batch rejection. Failure returns no publishable
transaction.

## Verification

The focused portable fixture covers all four outcomes. It decodes the emitted
pages, checks exact identities and predecessor ownership, routes through the
rewritten levels, finds the inserted value, checks the compact record and
target lengths, and compares repeated ordinary construction byte for byte. It
also exercises publication completion and uncertainty boundaries and rejects
wrong depth, wrong routed branch, wrong routed leaf, invalid inherited range,
and malformed root input.

The project is an owned target of the native database-storage development and
complete suites on Windows and Linux. Current focused Windows execution returns
`0`; paired-host qualification remains pending.

## Exclusions

This milestone does not update an existing depth-four tree, accept a dynamic
path, delete or merge entries, reclaim or reuse pages, pin snapshots, add
concurrent writers, provide range cursors, define catalogs or row codecs, parse
SQL, listen on a network, authenticate clients, or own server lifecycle.
