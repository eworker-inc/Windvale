# Windvale database committed-generation range scan

## Status and boundary

This contract composes bounded ascending leaf scans across a durable tree
without leaf sibling pointers and without accumulating a multi-leaf result in
memory. The hosted owner is
`Libraries/Platform/Database/Durable-Tree-Scan.wv` and uses one pre-bound
`storage.random_access_v1` capability.

```text
Databaseˉdurableˉtreeˉscanˉpage(
    Current,
    Has_start, Start, Start_inclusive,
    Has_end, End, End_inclusive,
    Limit,
    Snapshot
) -> Databaseˉdurableˉtreeˉscanˉresult
```

The limit is 1 through 500 rows. Present keys are 1 through 4,096 bytes.
Absent bounds have empty bytes and a false inclusive flag. Start must not sort
after end. Invalid current state, bounds, limits, and inactive snapshot shapes
are rejected before provider I/O.

## Cursor pages and memory

One call visits at most one root-to-leaf path, so page reads are bounded by the
selected root depth, currently at most 32. It returns entries from at most one
physical leaf as the existing packed leaf-entry encoding. `Entries` and
`Last_key` borrow the final provider response and remain valid only until the
next call on the same capability instance.

This page shape is deliberate. It avoids copying each row, avoids a
quadratic growing concatenation across leaves, and bounds returned database
bytes by one physical page even when a logical scan covers millions of rows.
The caller consumes or streams the packed page before resuming.

`Has_resume` and `Has_more` are equal. `Resume` is an owned key:

- when more selected entries remain in the same leaf, it is the last returned
  key and `Resume_inclusive = false`;
- when the leaf is exhausted but another leaf may match, it is the inherited
  upper separator and `Resume_inclusive = true`; and
- at the global end, both flags are false and the status is `Complete`.

An empty or nonmatching leaf may therefore return zero rows with an owned
inclusive resume separator. No arithmetic successor key or sibling link is
required. An absent initial start routes with the minimum valid one-byte zero
key, then scans the selected leaf without a start bound.

## Snapshot binding

`Databaseˉdurableˉtreeˉscanˉbegin()` returns the only valid inactive snapshot.
The first successful page captures:

- committed database generation and sequence;
- committed root identity;
- provider generation; and
- exact provider storage length.

Every later page requires the database fields to equal `Current` before I/O
and every provider response to equal the captured provider generation and
length. A committed database change is `Invalid_snapshot`; a provider change
during or between pages is `Changed_snapshot`. The scanner never silently
restarts against a newer generation, so a caller cannot combine rows from two
commits under one cursor.

## Validation

Every call repeats the durable reader's physical and graph checks: page
identity and size, generation and sequence visibility, exact current root,
expected root/branch/leaf kinds, outer/inner item-count agreement, descending
and committed child identities, complete branch routing, inherited leaf
ranges, checksums, and padding. The generation-1 empty root is a valid complete
zero-row scan.

The final leaf is passed to the portable bounded leaf scanner. End-bound
comparison against the inherited upper separator decides whether another leaf
can still match. Equality continues only for an inclusive end because the
separator is the first key in the right child.

## Verification

The focused hosted fixture consumes exact committed generation-two and
generation-three files produced by the existing storage and tree-reader
prerequisites. It checks unbounded and bounded cross-leaf traversal, a zero-row
boundary page, one-row continuation inside a leaf, inclusive end behavior,
pre-I/O rejection, database-snapshot mismatch, provider-snapshot mismatch, and
byte-for-byte read-only storage. The scan has its own native project so adding
cursor behavior does not push the established publication/reader image beyond
the ordinary 4 MiB object limit.

## Exclusions

This contract is ascending only. Reverse scans, opaque serialized cursor
tokens, server session ownership, timeout policy, snapshot retention/pinning,
page caching, read-ahead, aggregation, and query-row decoding remain separate
layers. A long-lived cursor must eventually integrate with retention so page
reclamation cannot retire its committed generation.
