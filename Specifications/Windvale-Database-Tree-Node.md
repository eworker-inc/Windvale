# Windvale database tree node

## Status

- Format: `WVTN 1`
- Portable codec and leaf operations: `Libraries/Database/Tree-Node.wv`
- Durable composition: `Libraries/Database/Single-Leaf-Upsert.wv`
- Physical envelope: `WVPG 1`
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

The implemented mutation is one root-leaf copy-on-write upsert. Branch nodes
are decoded and locally validated so their durable shape is reserved, but
branch lookup, split propagation, merge, and reclamation are not yet
implemented.

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
entry sequence consumes the declared used length exactly. Global child bounds,
generation visibility, acyclicity, separator ranges, and reachability require
the future tree reader because they depend on the selected database and other
pages.

## Portable APIs

```text
Databaseˉtreeˉkeyˉcompare(Left, Right) -> Databaseˉtreeˉkeyˉorder
Databaseˉtreeˉnodeˉdecode(Input) -> Databaseˉtreeˉnodeˉresult
Databaseˉtreeˉleafˉlookup(Input, Key) -> Databaseˉtreeˉleafˉlookupˉresult
Databaseˉtreeˉleafˉupsert(Input, Key, Value, Maximum_payload)
    -> Databaseˉtreeˉleafˉupsertˉresult
```

All expected failures are typed. Decode rejects malformed headers, lengths,
counts, children, reservations, entry truncation, oversized keys or values,
noncanonical ordering, and trailing bytes. Upsert additionally distinguishes
an invalid payload ceiling from a full page. It never silently drops an entry
or changes the requested key.

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

## Verification

The native portable fixture covers prefix and unsigned-byte ordering, insertion
before/between/after existing keys, empty values, replacement, missing and
found lookup, deterministic output, exact payload capacity, key/value/count
limits, every header field, truncated entries, duplicate order, trailing bytes,
branch reserved/child rules, root-selection mismatch, invalid root payload,
and two consecutive durable generations.

The focused Windows host uses the same composition to publish key `u32(7)` and
value `i32(42)`. It terminates after zero through four storage actions. Every
restart selects only a fully valid old or new generation; a new generation must
decode `WVTN 1` and return the exact value before the commit log is read. The
equivalent Linux image is constructed pending independent execution.

## Next contracts

The next tree milestone is a bounded reader over provider-loaded pages followed
by leaf split and branch-root creation. It must add global child/range proofs,
bounded depth and page visits, immutable page ownership, and crash tests for a
multi-page allocation. Delete, merge, reclamation, snapshots, concurrent
readers, catalog typing, SQL, and network service behavior remain later layers.
