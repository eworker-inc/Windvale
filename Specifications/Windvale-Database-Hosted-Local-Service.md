# Windvale hosted local database service

## Status and scope

The first hosted local service binds the portable local database contract to
`storage.random_access_v1`. Application code supplies a collection identity,
record identity, schema identity, and payload. It does not select pages,
superblocks, database identities, generations, or commit locations.

The initial writer supports the canonical depth-one database. The hosted get
adapter uses the bounded durable tree reader and therefore reads every admitted
tree depth. A full depth-one root is rejected for later routing to the existing
root-split and multi-level writer contracts; it is never silently retried.

## Open, put, restart, and get

Open describes one provider snapshot, reads exactly the 512-byte dual
superblock header, rejects a changed snapshot, selects the current tail-free
superblock, and constructs a ready portable session from its stored identity
and page size.

Put prepares the canonical logical key and record envelope before dispatching
the depth-one writer. A confirmed four-action publication returns
`Put_committed` and requires reopen. Any outcome after provider action, any
active/aborted/recovery outcome, and any changed snapshot returns
`Reopen_required`; the adapter never replays the mutation. A depth mismatch is
rejected without pretending it was committed.

After process restart, get opens the selected durable snapshot, prepares the
same canonical logical key, performs one bounded tree lookup, and passes only a
found value through the logical record decoder. Missing remains distinct from
failure. A changed reader snapshot requires reopen.

## Performance and memory

Put and get are separate native applications because the current single-object
format is bounded to 4 MiB. The focused write object is 4,096,322 bytes and the
focused read object is 2,337,300 bytes with the measured 2026-08-15 toolchain.
The split avoids loading reader code into a writer or writer code into a reader,
keeps verification focused, and preserves bounded memory independent of total
database history.

The `host-local-service` verifier reports tool preparation, storage bootstrap,
and adapter time separately. It reuses content-addressed project-object and
hosted-application checkpoints, so an unchanged rerun measures cache reuse
rather than recompiling the same source.

## Verification

Windows and Linux scripts own the same two projects. Each creates or reuses the
canonical 4,608-byte initial database, runs logical put to a 12,800-byte
committed image, starts a distinct get process, verifies schema and payload,
checks a missing logical identity, and proves the get did not change any byte.
The root-writer owner separately retains all five publication-interruption
boundaries used by this adapter.
