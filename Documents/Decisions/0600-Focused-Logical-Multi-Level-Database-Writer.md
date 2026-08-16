# Decision 0600: focused logical multi-level database writer

## Status

Accepted on 2026-08-15.

## Context

The provider-driven durable tree writer already commits raw key/value bytes at
tree depths two through eight. Applications need the canonical logical record
boundary instead: collection identity, record identity, schema identity, and
payload. Combining the complete local session, reader, logical codec, and tree
writer in one native object exceeded the current 4 MiB object limit.

Performance, memory use, verifier time, and developer feedback time are product
requirements for Windvale. A database composition that only fits by loading
unneeded read or session code into each write process would weaken those
requirements and leave too little space for future validation.

## Decision

Add a focused, capability-free logical write codec that owns the exact `WVKR 1`
key and `WVRD 1` value construction. The complete logical record codec delegates
put preparation to it, preserving one durable format implementation.

Add a hosted logical tree-writer projection that validates logical arguments
and sends the canonical bytes unchanged to the existing bounded durable tree
writer. Keep open/session dispatch and reading in separate projections. Extend
the focused host-tree-writer owner to commit a logical record and verify it
through an independent restart reader without modifying the file.

## Consequences

- Logical records can now be committed through the full admitted tree depth,
  not only through the depth-one local-service writer.
- The measured Windows writer object is 4,166,878 bytes, 27,426 bytes below the
  current 4 MiB limit; the restart reader object is 2,374,656 bytes.
- Write and read processes load only the code they need, and database history
  remains outside their fixed code-memory cost.
- The service still needs one explicit bounded dispatcher for depth-one,
  root-split, and existing multi-level snapshots.
- This decision does not add SQL, JSON, schemas, indexes, transactions, delete,
  range scans, networking, or concurrent-writer arbitration.

## Reconsideration triggers

Reconsider the projection split when the native module/object limit changes,
dead-code elimination makes the combined service comfortably bounded, or a
measured in-process server shows that an explicit dispatcher is slower or more
memory-intensive than another equally clear capability boundary.
