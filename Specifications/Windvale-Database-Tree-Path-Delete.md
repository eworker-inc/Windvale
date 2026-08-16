# Windvale database bounded tree-path delete

## Status

- Portable transaction: `Libraries/Database/Tree-Path-Delete.wv`
- Logical leaf operation: `Libraries/Database/Tree-Node.wv`
- Physical pages and publication: `WVPG 1`, `WVCR 1`, and `WVDS 1`
- Evidence: focused portable Windows native execution; independent Linux
  execution pending

## Boundary

The capability-free operation removes one exact key from an existing committed
tree whose depth is between two and eight inclusive:

```text
Databaseˉtreeˉpathˉdeleteˉbegin(Current, Path, Key)
    -> Databaseˉtreeˉpathˉdelete
```

`Path` contains exactly `Current.Rootˉdepth` complete `WVPG 1` pages in
root-to-leaf order. The transaction validates the selected root generation and
sequence, descendant visibility, page size and identity, descending child
identities, decoded item counts, page kinds, routing, and inherited key range.
It performs no I/O and retains no caller or provider buffer.

## Result

A missing key is a successful no-op with `Found = false`, `Hasˉcommit = false`,
no new or obsolete pages, and the unchanged root identity. It does not spend a
generation or sequence.

A present key produces `Found = true` and one publishable commit. For depth
`D`, it appends exactly `D` data pages: the replacement leaf and each
replacement ancestor through the root. Every output names its selected
predecessor; obsolete identities are packed little-endian `u64` values in
leaf-first order. Branch separators and tree depth remain unchanged.

Deleting the final entry leaves a canonical zero-entry `WVTN 1` leaf inside a
zero-item `WVPG 1` leaf. Merge, reuse, and reclamation are separate policies.
Equal admitted inputs produce byte-identical pages, log, obsolete evidence,
and superblock.

Typed rejection distinguishes invalid current state, unsupported depth, wrong
path length, malformed pages, snapshot or route mismatch, invalid node or
range, leaf or branch update failure, exhausted identities, output-page
failure, and commit rejection. A rejected result contains no publishable
transaction.

## Verification

The portable fixture deletes through depth three, validates the replacement
leaf, branch, root, predecessor identities, routing, and deterministic bytes.
It proves a missing-key no-op and deletion of the final key into a valid empty
leaf. The hosted fixture independently exercises the same transaction against
real provider storage.

## Exclusions

This contract does not discover pages, publish storage actions, merge sparse
nodes, reclaim pages, pin snapshots, coordinate concurrent writers, or retry
an uncertain mutation.
