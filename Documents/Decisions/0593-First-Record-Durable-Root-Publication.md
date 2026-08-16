# Decision 0593: First-record durable root publication

- Date: 2026-08-15
- Status: Implemented hosted milestone
- Defines: [hosted root writer](../../Specifications/Windvale-Database-Hosted-Root-Writer.md)
- Builds on: [Decision 0535](0535-First-Durable-Database-Commit.md), [Decision 0548](0548-First-Durable-Tree-Node-And-Upsert.md), [Decision 0588](0588-Portable-Local-Database-Service-Contract.md)

## Context

The portable local service could prepare a put, and the existing hosted writer
could update trees of depth two through eight. A freshly bootstrapped database
has depth one, so no hosted component could yet write its first application
record and prove that record across a process restart.

Adding this path to the already large multi-level writer exceeded the useful
native object boundary. The existing portable single-leaf transaction already
owned the correct record replacement and two-page commit construction.

## Decision

- Add a separate hosted root writer for exactly depth one.
- Read and own exactly one provider-backed root page before mutation.
- Reuse the portable single-leaf transaction and existing four-action storage
  publication executor.
- Require exact provider generation and length stability and preserve distinct
  active, aborted, recovery-required, and committed outcomes.
- Give the writer its own `host-root-writer` development target so ordinary
  changes do not require the larger tree-writer target.
- Prove the same committed bytes after restart at all five publication
  interruption boundaries.
- Keep memory proportional to a fixed number of admitted pages, never database
  history or total storage length.

## Consequences

A canonical empty database can now accept one real durable record, restart, and
read that exact value back. The remaining local-server work is composition: map
the portable service's prepared request to the depth-one or multi-level writer,
then reopen the session on the newly selected superblock.

The root writer rejects a full-page split rather than silently selecting a
different transaction. A later dispatcher must route that case to the existing
root-split and multi-level contracts explicitly.

## Reconsideration triggers

Revisit the separate native component only when measured object limits and
feedback time support safe unification. Revisit single-operation publication
only with a specified batching or transaction contract; never infer automatic
retry from an uncertain provider outcome.
