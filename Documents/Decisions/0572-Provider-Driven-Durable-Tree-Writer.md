# Decision 0572: Provider-driven durable tree writer

- Date: 2026-08-15
- Status: Implemented candidate with focused Windows execution evidence
- Requires: [Decision 0569](0569-Bounded-Owned-Tree-Path-Upsert.md)
- Defines: [hosted tree writer](../../Specifications/Windvale-Database-Hosted-Tree-Writer.md)
- Retains: `WVTN 1`, `WVPG 1`, `WVCR 1`, `WVDS 1`, exact snapshot
  validation, append-only copy-on-write, and four-action publication

## Context

The portable tree-path transaction could update depths two through eight, but
its caller still had to discover, validate, copy, concatenate, transact, and
publish the path correctly. That seam is safety-sensitive because each random-
access provider response is borrowed only until the next provider call, and a
concatenated arena value may itself remain tail-reusable by later builders.

The database needs one hosted mutation boundary before catalog, record, query,
or server work can rely on an executable storage engine.

## Decision

- Add a hosted `storage.random_access_v1` coordinator for one routed upsert.
- Admit only a tail-free exact committed snapshot of depth two through eight.
- Describe storage once and reject any generation or length change during traversal.
- Validate the physical graph, visibility, kinds, routes, counts, and inherited range.
- Copy every borrowed page before the next provider call and create a final
  non-tail-reusable owned path before invoking the portable transaction.
- Reuse the existing transaction, commit batch, publication state machine,
  hosted executor, and recovery contract without changing a durable format.
- Return exact active, committed, aborted, or recovery-required state and never
  silently retry an uncertain mutation.
- Give the writer its own native target because combining reader and writer
  closures exceeds the ordinary complete-object bound while each target fits.

## Evidence

The dedicated target compiler-aligns 169 functions and lowers below the 4 MiB
complete-object ceiling. Focused Windows execution inserts key 4/value 40 into
the prior depth-two generation, publishes generation 3 at length 33,280,
validates predecessor links and routing, proves byte-stable reopen, and
converges after interruption at actions zero through four.

The Windows database development owner passes eleven targets: eight portable
transactions plus hosted storage, reader, and writer lifecycles. Independent
Linux execution and the cold paired-host retirement gate remain qualification
evidence.

## Consequences

- Windvale now has a provider-driven, durable, bounded tree mutation operation.
- Borrowed provider lifetimes and arena-tail aliasing are both closed before mutation.
- Database-storage grows from 19 to 20 retirement cases and from ten to eleven
  development targets.
- The complete retirement manifest remains 63 suites and grows from 3,471 to 3,472 cases.
- Server lifecycle, client protocol, catalogs, records, queries, concurrency,
  deletion, and reclamation remain separate milestones.

## Reconsideration triggers

Revisit the coordinator when snapshot pinning or concurrent writers introduce
a stronger provider generation contract, when a consuming bounded path
collection replaces packed bytes, or before depth nine must be admitted.
Automatic retry requires an explicit idempotency and uncertain-completion
contract; it must not be inferred from the current publication result.
