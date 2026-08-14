# Windvale database tree node

## Status

- Format: `WVTN 1`
- Portable codec, routing, and leaf split: `Libraries/Database/Tree-Node.wv`
- Durable compositions: `Libraries/Database/Single-Leaf-Upsert.wv` and
  `Libraries/Database/Root-Split-Upsert.wv`
- Physical envelope: `WVPG 1`
- Hosted reader: `Libraries/Platform/Database/Durable-Tree-Reader.wv`
- Evidence: portable native execution and focused Windows interruption/restart;
  independent Linux execution pending

## Boundary

`WVTN 1` is the first durable logical tree-node payload for Windvale Database.
It defines ordered variable-length byte keys, variable-length leaf values, and
`u64` branch-child identities inside a checksummed `WVPG 1` root, branch, or
leaf page. The node codec is portable and has no storage authority.

Keys and values are bytes so later catalog, schema, row, and index codecs can
own their typed encodings without changing tree ordering. Version 1 does not
interpret text, numbers, nulls, rows, or collation. Callers must provide one
canonical key encoding whose unsigned bytewise order is the intended index
order.

The implemented mutations are one root-leaf copy-on-write upsert and the first
full-root transition to two leaves beneath a branch root. Provider-backed
lookup traverses that depth-two shape with global range and graph proofs.
General non-root split propagation, branch rewrite, merge, and reclamation are
not yet implemented.

## Header

Every nonempty node payload begins with this exact 32-byte header:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVTN`, integer `0x4E545657` |
| 4 | 4 | version | `1` |
| 8 | 4 | header size | `32` |
| 12 | 4 | kind | leaf `1` or branch `2` |
| 16 | 4 | entry count | at most 4,096; branch is nonempty |
| 20 | 4 | used length | exact complete payload length |
| 24 | 8 | rightmost child | no-page for leaf; real page for branch |

The complete node is 32 through 65,408 bytes. There is no node-local checksum:
`WVPG 1` already hashes the exact used payload and checks every outer padding
byte. A valid checksum does not override any node length, order, kind, count,
or child failure.

The generation-1 bootstrap root remains the one admitted exception: an empty
root page has zero items and an empty payload. The durable composition proves
generation 1, sequence 0, and root page 0 before its first upsert produces a
nonempty canonical `WVTN 1` leaf. Generic decode and lookup reject empty bytes.
Later empty-tree deletion requires a separate contract.

## Leaf entries

A leaf entry is packed without alignment or padding:

| Relative offset | Bytes | Field |
| ---: | ---: | --- |
| 0 | 4 | key length |
| 4 | 4 | value length |
| 8 | key length | key bytes |
| after key | value length | value bytes |

Keys contain 1 through 4,096 bytes. Values contain zero through 61,440 bytes.
Keys are strictly increasing under unsigned lexicographic comparison; a key
that is a proper prefix sorts first. Duplicate keys are invalid on decode.
Upsert replaces an equal key without changing entry count or inserts a missing
key at its canonical position. The complete result must fit the caller's exact
`page_size - 128` payload ceiling.

## Branch entries

A branch with `n` separators has `n + 1` children. Each packed separator owns
the child to its left:

| Relative offset | Bytes | Field |
| ---: | ---: | --- |
| 0 | 4 | separator-key length |
| 4 | 4 | reserved zero |
| 8 | 8 | left child page identity |
| 16 | key length | separator key bytes |

The header owns the rightmost child. Every child is different from the no-page
sentinel, separator keys are nonempty and strictly increasing, and the complete
entry sequence consumes the declared used length exactly. The hosted durable
reader supplies global child bounds, generation visibility, acyclicity,
separator ranges, and routed reachability because those proofs depend on the
selected database and other pages.

## Portable APIs

```text
Databaseˉtreeˉkeyˉcompare(Left, Right) -> Databaseˉtreeˉkeyˉorder
Databaseˉtreeˉnodeˉdecode(Input) -> Databaseˉtreeˉnodeˉresult
Databaseˉtreeˉleafˉlookup(Input, Key) -> Databaseˉtreeˉleafˉlookupˉresult
Databaseˉtreeˉleafˉupsert(Input, Key, Value, Maximum_payload)
    -> Databaseˉtreeˉleafˉupsertˉresult
Databaseˉtreeˉleafˉsplitˉupsert(Input, Key, Value, Maximum_payload)
    -> Databaseˉtreeˉleafˉsplitˉresult
Databaseˉtreeˉbranchˉtwoˉchildren(Separator, Left, Right, Maximum_payload)
    -> Databaseˉtreeˉnodeˉresult
Databaseˉtreeˉbranchˉroute(Input, Key, Has_lower, Lower, Has_upper, Upper)
    -> Databaseˉtreeˉbranchˉrouteˉresult
Databaseˉtreeˉleafˉrangeˉvalidate(
    Input, Has_lower, Lower, Has_upper, Upper
) -> Databaseˉtreeˉnodeˉerror
```

All expected failures are typed. Decode rejects malformed headers, lengths,
counts, children, reservations, entry truncation, oversized keys or values,
noncanonical ordering, and trailing bytes. Upsert additionally distinguishes
an invalid payload ceiling from a full page. It never silently drops an entry
or changes the requested key.

Split-upsert evaluates every contiguous boundary whose two nonempty encoded
leaves fit the selected payload ceiling. It minimizes encoded byte imbalance
and uses the earliest boundary as the deterministic tie break. The separator
is an owned copy of the first right-leaf key. Branch routing treats inherited
bounds as lower-inclusive and upper-exclusive, copies returned bounds, and
routes separator equality to the right child.

A successful lookup value is a borrowed slice of the caller-supplied immutable
node bytes. Its lifetime is therefore the input's lifetime. When the input came
from `storage.random_access_v1`, the caller must finish lookup or copy the value
before the next storage call under that capability's borrowed-response rule.

## Durable copy-on-write upsert

```text
Databaseˉsingleˉleafˉupsertˉbegin(Current, Root, Key, Value)
    -> Databaseˉsingleˉleafˉupsert
```

The composition layer accepts a freshly selected tail-free depth-one
generation and a separately decoded current `WVPG 1` root. It requires exact
agreement on page size, root identity, generation, and committed sequence. A
nonempty root must contain a valid leaf node whose entry count equals the page
envelope's item count.

After upsert, the existing single-writer builder allocates a new root and one
commit-log page, links the old root through `Previous_page`, constructs the
target superblock, and returns the existing four-action durable publication.
The old root is never modified. Equal inputs produce equal node, page, log, and
superblock bytes.

When ordinary root-leaf upsert returns `Full`, the separate root-split
composition can append a left leaf, right leaf, and branch root through the
bounded commit batch. The new root has depth two, every new data page links the
old root as its predecessor, and the old generation remains immutable. The
complete provider-backed traversal and publication contract is defined by
[tree reading and root split](Windvale-Database-Tree-Reading-And-Root-Split.md).

## Verification

The native portable fixture covers prefix and unsigned-byte ordering, insertion
before/between/after existing keys, empty values, replacement, missing and
found lookup, deterministic output, exact payload capacity, key/value/count
limits, every header field, truncated entries, duplicate order, trailing bytes,
branch reserved/child rules, inherited ranges, separator equality, every legal
split boundary, deterministic byte balance, root-selection mismatch, invalid
root payload, and two consecutive durable generations.

The focused Windows hosts publish both a depth-one key `u32(7)` / value
`i32(42)` generation and a depth-two three-key generation. They terminate
after zero through four storage actions. Every restart selects only a fully
valid old or new generation; the depth-two reader reaches either child in two
page visits and returns the exact value. Equivalent Linux images are
constructed pending independent execution.

## Next contracts

The next tree milestone is general depth-two upsert and child split propagation
with obsolete-page ownership. Delete, merge, reclamation, pinned concurrent
snapshots, catalog typing, SQL, and network service behavior remain later
layers.
