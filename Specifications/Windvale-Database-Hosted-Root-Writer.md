# Windvale database hosted root writer

## Status and purpose

`Durableˉrootˉwriter` performs one real durable upsert while the database tree
fits in its single root page. In practical terms, it can take a fresh database,
write its first record, restart, and prove that the record survived.

The writer owns `storage.random_access_v1`. It is separate from the existing
depth-two-through-eight writer so each native application remains within the
bounded object and verification envelope.

## Operation

```text
Databaseˉdurableˉrootˉupsert(Current, Key, Value, Maximumˉactions)
    -> Databaseˉdurableˉrootˉupsertˉresult
```

`Current` must be a valid, tail-free depth-one selection. The key must contain
one through 4,096 bytes. The observed provider generation must be nonzero and
the storage length must exactly equal the selected committed length.

The writer reads exactly one root page, verifies that the provider did not
change between description and read, decodes and owns that page, and delegates
the in-page update to the portable single-leaf transaction. A successful
transaction produces exactly two appended pages: the replacement root and its
commit-log page.

Publication has at most four actions:

1. append the replacement root and commit-log page;
2. flush content and length;
3. write the inactive superblock; and
4. flush content.

Committed, still active, provider-rejected, and recovery-required outcomes stay
distinct. An uncertain result is never permission to replay the put.

## Performance and memory bounds

The operation visits one provider page regardless of total historical storage
length. It retains a bounded owned copy of that page and a fixed two-page
publication batch. The accepted page size is at most 65,536 bytes, and no loop
scans database history or allocates in proportion to database length.

The native development application currently compiles to 186,700 WVB bytes and
lowers to a 3,729,870-byte WVO. These sizes are evidence for this source state,
not permanent format promises.

## Restart and interruption evidence

The `host-root-writer` target starts from the canonical 4,608-byte empty image,
writes `first-record` to `survives-restart`, and reaches a 12,800-byte generation
2 image with root page 1 and commit-log page 2. A second process invocation
decodes the root and reads the exact value without changing any byte.

Five interruption scenarios stop before publication and after each of the four
actions. Restart either removes the uncommitted tail or admits the published
superblock, then converges byte-for-byte on the same committed image.

## Exclusions

This writer does not split a full root, dispatch to the multi-level writer,
bind the portable local-service session, allocate collections, delete records,
reclaim pages, arbitrate concurrent writers, authenticate clients, or listen on
a network endpoint.
