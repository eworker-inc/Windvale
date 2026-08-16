# Windvale local database service

## Status and scope

The local database service is split into small portable modules so an
application includes only the code it uses:

- `Windvaleˉdatabaseˉlocalˉcontracts` owns the shared session, request, result,
  status, and completion types;
- `Windvaleˉdatabaseˉlocalˉsession` owns open, reopen, and close;
- `Windvaleˉdatabaseˉlocalˉput` owns put preparation and completion;
- `Windvaleˉdatabaseˉlocalˉget` owns get preparation and completion; and
- `Windvaleˉdatabaseˉlocalˉcontrol` owns cancellation before provider
  acceptance.

These modules form one portable contract for one locally bound database
instance. They own no capability and perform no storage I/O. Hosted lifecycle,
reader, and writer adapters supply admitted superblock selections and explicit
completion outcomes. The split is part of the performance and memory contract:
unused get, put, or lifecycle code must not be forced into a focused native
application.

The first profile is deliberately sequential: one session, one request in
flight, and monotonically increasing nonzero `u64` request identities. It does
not define networking, concurrent writers, ambient database discovery, or
automatic mutation replay.

## Session lifecycle

Opening requires a nonzero database identity, nonzero page size, a valid
tail-free superblock selection, and exact identity/page-size agreement. A
successful session begins ready with request identity `1`.

Ready sessions can prepare one get or put. Preparation delegates canonical key
and record-envelope construction to the logical-record contract. Invalid
logical input leaves the session ready and consumes no request identity.
Successful preparation makes the session busy until exactly one completion or
cancellation is supplied for the active request.

A completed or uncertain put moves the session to `Reopen_required`; it cannot
issue another request. Reopen requires the same database identity and page
size, no unpublished tail, and nondecreasing generation and committed sequence.
After a confirmed commit, both generation and committed sequence must strictly
advance. An uncertain completion may reopen the same admitted snapshot because
the publication may not have occurred, but the mutation is never replayed.

Closing a ready, failed, or reopen-required session is explicit. Closing a busy
or already closed session is rejected.

## Completion semantics

A found get admits its value only through the logical record decoder and
returns the schema identity plus payload. Missing requires an empty provider
value. Malformed found values fail the session.

Put completion distinguishes committed, rejected, reopen-required, and failed.
Committed means the hosted writer reported durable publication; the portable
session still requires reopen before later work. Reopen-required represents an
uncertain or changed storage state and must never be converted into retry.
Cancellation is valid only before the provider accepts a prepared request. Once
a put has been dispatched, its adapter must produce a completion outcome rather
than cancel it as if no mutation could have occurred.

## Verification and exclusions

The `local-service` database owner target proves invalid open, exact request
ordering, logical encoding rejection, one-in-flight enforcement, cancellation,
put-to-reopen, identity and monotonic reopen checks, found/missing decoding,
strict advancement after confirmed commit, same-snapshot admission after an
uncertain completion, malformed-value failure, and close behavior. Both host
scripts own the same portable project.

The `host-local-service` owner starts with the canonical empty database, calls
the hosted logical put adapter, starts a separate process, calls the hosted
logical get adapter, and proves that the read leaves the committed file byte
for byte unchanged. The focused put and get objects are reported separately so
object growth remains visible.

Recovery orchestration, collection allocation, delete, queries, transactions,
authentication, networking, and multi-client arbitration remain separate
contracts.
