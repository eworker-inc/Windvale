# Windvale database bounded tree-path upsert

## Status

- Portable transaction: `Libraries/Database/Tree-Path-Upsert.wv`
- Logical operations: `Libraries/Database/Tree-Node.wv` and
  `Libraries/Database/Tree-Branch-Split.wv`
- Physical pages and publication: `WVPG 1`, `WVCR 1`, and `WVDS 1`
- Evidence: focused portable Windows native execution; independent Linux
  execution pending

## Boundary

This contract updates one routed leaf in an existing committed `WVTN 1` tree
whose input depth is between two and eight inclusive:

```text
Databaseˉtreeˉpathˉupsertˉbegin(Current, Path, Key, Value)
    -> Databaseˉtreeˉpathˉupsert
```

`Path` is one owned `bytes` value containing exactly `Current.Rootˉdepth`
complete `WVPG 1` pages in root-to-leaf order. Its exact length is therefore
`Current.Rootˉdepth * Current.Pageˉsize`. The transaction is capability-free
and performs no I/O. A hosted caller must copy every borrowed provider response
into owned storage before a later provider call can invalidate that response.

`Current` must be valid and tail-free. The first page must be the exact current
root generation and sequence. Descendants may come from any generation and
sequence not newer than `Current`. Every page must use the selected page size,
match the child selected by routing `Key`, remain below its parent and the
committed page count, agree with its decoded item count, and use the expected
root, branch, or leaf physical kind. The nonempty leaf and `Key` must remain
inside the lower-inclusive and upper-exclusive bounds inherited from all
branches.

The exact-length owned path is the transaction's input-consumption boundary.
It does not borrow provider buffers, retain caller storage, discover pages, or
grant storage authority.

## Validation and propagation

The transaction first validates the complete path top-down. It then allocates
the replacement leaf or leaf pair and revisits each parent from the selected
path while rebuilding bottom-up. Each revisit performs the same bounded
top-down routing validation; with maximum input depth eight, this requires at
most 36 decoded input-page visits. That fixed rescan bound avoids hidden
recursion and a new general collection contract.

At each parent, a nonsplit child is replaced by its new identity. A split child
inserts its separator and two new identities. If that insertion fills the
parent, the existing deterministic branch split promotes one separator to the
next level. Separator equality routes right. Split selection retains the
established minimum encoded-byte imbalance and earliest legal-boundary tie
break.

If the selected root does not split, its replacement remains a root at the
same depth. If it splits, both replacements become branches and one new root
is appended, increasing the depth by one. An admitted depth-eight input can
therefore produce a depth-nine result, but a later mutation must still use an
input depth supported by the then-current transaction contract.

## Allocation and ownership

For current page count `P`, allocation proceeds in identity order:

1. replacement/left leaf, then an optional right leaf;
2. replacement/left parent and optional right parent at every level toward the
   root; and
3. an optional new root, followed by the compact commit log.

A nonsplitting depth-`D` update emits exactly `D` data pages. A split at every
level emits `2D + 1` data pages; the maximum admitted depth therefore emits 17
data pages, below the existing 63-data-page commit limit.

Exactly one left or replacement output page owns each selected input page as
its `Previousˉpage`, producing exactly `D` distinct committed predecessors.
Right siblings and a newly created root have no predecessor because they do
not replace distinct committed pages. The result publishes the obsolete page
identities as packed little-endian `u64` values in bottom-up order: leaf first,
then each ancestor through the old root.

All new child identities are below their parent identities. Page-identifier,
generation, and sequence exhaustion are rejected before unsafe arithmetic or
publication.

## Publication and determinism

`Databaseˉcommitˉbatchˉbegin` validates the ordered encoded data pages,
appends one `WVCR 1` log page, builds the inactive `WVDS 1` superblock slot, and
returns the existing four actions:

1. append data pages and log;
2. flush content and length;
3. write the inactive 256-byte superblock slot; and
4. flush content.

An uncertain mutation still enters recovery and is never silently retried.
No durable format or provider contract changes. Equal admitted inputs produce
byte-identical nodes, pages, log, obsolete-page evidence, and superblock.

## Result and rejection

Success reports replacement, the number of levels that split, whether the root
split, input and result depths, data-page count, first and root identities, the
packed obsolete identities, and the complete commit batch.

Typed rejection distinguishes invalid current state, unsupported depth, wrong
path length, malformed physical pages, path/visibility mismatch, invalid node
or route, inherited-range failure, leaf or branch update failure,
generation/sequence/page exhaustion, output-page failure, new-root failure,
and commit rejection. Failure returns no publishable transaction.

## Verification

The focused native fixture covers ordinary updates at depths three and four,
exact bottom-up predecessor evidence, and a full depth-four leaf/branch/root
cascade that emits nine data pages and creates a depth-five root. It decodes
the emitted pages, checks exact identities and target length, and compares two
complete constructions byte for byte. It rejects wrong length, unsupported
depth, a mismatched routed child, and a malformed page.

The project is an owned target of both database-storage modes on Windows and
Linux. Current focused Windows execution returns `0`; paired-host
qualification remains pending.

## Exclusions

This milestone does not discover the path through I/O, update input trees
deeper than eight, delete or merge entries, reclaim or reuse pages, pin
snapshots, add concurrent writers, provide range cursors, define catalogs or
row codecs, parse SQL, listen on a network, authenticate clients, or own server
lifecycle.
