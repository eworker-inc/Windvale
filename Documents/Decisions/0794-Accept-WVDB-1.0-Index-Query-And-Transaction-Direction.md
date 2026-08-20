# Decision 0794: Accept WVDB 1.0 index, query, and transaction direction

- Date: 2026-08-20
- Status: Accepted direction; numeric limits and normative operations pending
- Accepts: recommendations Q12 through Q16 in the
  [upper-layer decision register](../Project/WVDB-1.0-Upper-Layer-Decision-Register.md)
- Builds on: [Decision 0792](0792-Accept-The-WVDB-1.0-Entity-And-Table-Foundation.md)

## Context

WVDB must remain bounded while supporting useful schemas, predictable index
maintenance, human queries, programmatic queries, multi-entity transactions,
and deterministic expressions.

## Decision

Each database advertises immutable admitted limits within WVDB 1.0 normative
maxima. Applications and schemas are admitted against those limits. The
current eight-index and eight-key-component implementation bounds remain
candidate evidence, not permanent public maxima.

Require bounded maintenance-mode index build, validate, rebuild, and drop in
the base profile. Online construction is an optional explicit capability with
the same final index semantics and additional snapshot, change-capture,
progress, cancellation, and recovery contracts.

The canonical query contract is one versioned bounded typed query model. WVDB
SQL is a separately versioned human language that lowers into that model. SQL
does not define storage, service messages, or compatibility with another
dialect.

The first transaction profile provides immutable committed snapshots and
bounded multi-record transactions through one serialized writer queue.
Transactions read their own changes and publish atomically. Readers retain
their admitted snapshot subject to explicit lifetime and retained-page bounds.
Multiple simultaneous physical writers are not required by WVDB 1.0.

Accept deterministic literal defaults first. Then use one versioned, pure,
capability-free, bounded expression model for check constraints, expression
indexes, and generated fields. Expressions cannot read ambient time,
randomness, locale, files, network, process state, or another unbound authority.

## Consequences

- Limits remain explicit without freezing today's prototype values forever.
- Every implementation has one recoverable index-build path.
- Applications and SQL tools share one semantic query model.
- Serialized publication simplifies correctness and recovery but limits write
  throughput until a later multi-writer contract is justified.
- One pure expression model avoids incompatible default, check, and index
  evaluators.

## Reconsideration triggers

Revisit this direction if measured workloads cannot meet their write budget
through one serialized writer, if maintenance-mode index construction prevents
a required operational profile, or if the typed query model cannot represent a
required bounded user query without SQL becoming a separate semantic engine.
