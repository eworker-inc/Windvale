# Windvale database hosted root-split writer

## Status and purpose

`Durableˉrootˉsplitˉwriter` performs the one missing tree-height transition: a
full depth-one root becomes two leaves under a depth-two branch root. It owns
`storage.random_access_v1` and accepts either canonical key/value bytes or the
focused logical collection/identity/schema/payload projection.

The component is separate from the ordinary depth-one and multi-level writers
so all three remain below the 4 MiB native object limit.

## Operation

The writer requires a valid tail-free depth-one selection, a nonempty key of at
most 4,096 bytes, and an unchanged provider snapshot. It describes storage,
reads and owns exactly one root page, and delegates the split transaction to
`Databaseˉrootˉsplitˉupsertˉbegin`.

The transaction must prove that an ordinary leaf upsert is full before it
creates two replacement leaves, a branch root, and one commit-log page. The
writer then executes the same bounded four-action publication contract as the
other durable writers. It never treats a partially executed or uncertain
mutation as safe to replay.

The logical entry point prepares the exact `WVKR 1` key and `WVRD 1` envelope
through `Windvaleˉdatabaseˉlogicalˉrecordˉwrite`. Invalid logical input reaches
no provider call.

## Performance and memory

The operation visits one provider page and retains only one owned input page
plus a fixed four-page publication batch. Its work and memory are independent
of database history.

The 2026-08-16 Windows fixture lowers to 3,673,316 object bytes, leaving 520,988
bytes below the 4 MiB limit. The independent root-fill fixture is 3,565,957
object bytes. These are source-state measurements, not format promises.

## Verification

The focused chain starts from the canonical 4,608-byte database. A separate
setup process writes one 3,850-byte value and reaches the 12,800-byte full
depth-one generation. The root-split process writes logical collection `7`,
identity `customer-1`, schema `3`, and payload
`stored-through-logical-tree-writer`, reaching a 29,184-byte depth-two
generation. An independent reader process then checks the schema and payload
and proves the database file remained byte-identical.

Windows and Linux scripts own the same projects and cache each focused object
and hosted application separately.

## Exclusions

This component does not choose between writer processes, open a network
listener, own sessions, delete or merge pages, reclaim history, define schemas,
create indexes, execute queries, or arbitrate concurrent writers.
