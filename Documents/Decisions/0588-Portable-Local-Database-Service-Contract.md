# Decision 0588: Portable local database service contract

- Date: 2026-08-15
- Status: Implemented portable contract
- Defines: [local database service](../../Specifications/Windvale-Database-Local-Service.md)
- Builds on: [Decision 0575](0575-Single-Writer-Database-Engine-Lifecycle.md), [Decision 0578](0578-Canonical-Logical-Database-Records.md)

## Context

The durable engine, tree reader/writer, logical records, catalog, and bootstrap
were individually useful but did not yet define an application-facing session.
Direct callers could accidentally overlap work, reuse request identities, keep
using a pre-write snapshot, or retry an uncertain mutation.

The hosted lifecycle, reader, and writer together also exceed one ordinary
native object. The service contract therefore needs to be capability-free and
segmentable before host adapters are composed.

## Decision

- Add one portable session state machine with explicit closed, ready, busy-get,
  busy-put, reopen-required, and failed states.
- Admit exactly one sequential nonzero request identity at a time.
- Reuse canonical logical-record preparation and decoding rather than define a
  second key/value format.
- Require reopen after committed or uncertain puts, reject generation or
  committed-sequence rollback, and require both values to advance after a
  confirmed commit.
- Make cancellation explicit and never turn uncertain mutation into replay.
- Keep lifecycle, reader, writer, storage binding, networking, and arbitration
  outside this module so each hosted adapter can remain independently bounded.
- Add a dedicated `local-service` target to both database development owners
  and the changed-file planner.

## Consequences

Windvale now has a stable portable seam that a local database server and E-Worker
7 adapter can consume without inheriting host paths or storage capabilities.
The next hosted milestone can be split into lifecycle, reader, depth-one writer,
and depth-two-through-eight writer components while preserving one session
protocol.

This decision does not claim that a hosted database server exists. In
particular, fresh bootstrap databases still need a separately bounded
depth-one writer publication adapter before the first service put can execute.

## Reconsideration triggers

Revisit request ordering when asynchronous multiplexing is implemented, and
revisit reopen rules only with a specified snapshot/transaction model. Do not
add automatic retry for any indeterminate mutation outcome.
