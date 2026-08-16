# Windvale database transaction paths

## Status

- Version: transaction paths 1
- Profile: portable
- Maximum mutations: 32
- Maximum root depth: 8
- Maximum path bytes: 16,777,216
- Evidence: focused Windows native execution; independent Linux execution pending

## Purpose

The transaction planner needs every affected root-to-leaf path from one stable
committed snapshot before it can rewrite shared pages. This contract validates
that bounded input without performing storage I/O or publishing a generation.

`Databaseˉtransactionˉpathsˉvalidate` accepts one valid `WVTM 1` set and
one complete path for each mutation, in mutation order. Every path contains
exactly `Root_depth` consecutive durable pages of the selected page size. The
complete byte length is therefore:

```text
mutation count * root depth * page size
```

The admitted page sizes, 32-mutation limit, and depth-eight limit make this at
most 16 MiB without arithmetic overflow.

## Validation

For every path the validator proves:

- the selected snapshot is valid, has no unpublished tail, uses an admitted
  page size, and names a root inside its page count;
- the first page is the exact current root generation and sequence;
- every later page is no newer than the selected snapshot;
- each durable page has the expected identity and page size;
- root, branch, and leaf durable kinds agree with decoded `WVTN 1` kinds and
  item counts;
- every branch routes the mutation key to the next supplied page;
- child identities are older than their parent and inside the committed page
  count; and
- leaf contents remain inside all inherited separator bounds.

When adjacent paths name the same page at the same level, their complete page
bytes must be identical. Sorted unique mutation keys make a shared B-tree
subtree contiguous, so adjacent consistency is sufficient to reject conflicting
views of every shared page. The result reports the exact path count and number
of distinct consecutive leaf groups and retains one owned copy of the paths.

## Performance and memory

Validation is linear in the supplied paths, plus one page comparison at each
shared adjacent level. It decodes the mutation set once and scans mutation
entries sequentially. It does not retain decoded page trees or allocate one
object graph per key. The owned input is explicitly capped at 16 MiB; the
provider-side collector may use a lower service limit before calling this
portable boundary.

## Verification

The focused native test covers root depth one, two mutations routed to two
leaves at depth two, two mutations sharing one leaf, exact unique-leaf counts,
incorrect length, unsupported depth, a corrupt durable page, a key paired with
the wrong leaf, conflicting bytes for a shared root identity, invalid
mutations, and a forged invalid page-size selection.

## Exclusions and next step

This contract does not read provider storage, remove duplicate input paths,
rewrite leaves, split nodes, assign new page identities, rewrite shared
ancestors, or publish a commit. The
[leaf-group planner](Windvale-Database-Transaction-Leaf-Groups.md) now groups
consecutive paths by leaf identity and applies each group once. Split handling
and the bottom-up shared replacement map remain next before one commit batch.
