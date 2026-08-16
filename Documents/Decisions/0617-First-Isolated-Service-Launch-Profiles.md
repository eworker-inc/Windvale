# Decision 0617: First isolated service-launch profiles

- Status: Implemented current-host policy candidate; live provider launch pending
- Date: 2026-08-15
- Contract: [service-launch policy 1](../../Specifications/Windvale-Os-Service-Launch-Policy.md)

## Decision

Admit filesystem and network providers through one `WVPR 1` serialization and
one lifecycle model, but distinct exact roles, domains, page budgets, bindings,
rights masks, and transfer ceilings. Each service owns one endpoint, reserves
one of four queue slots for control, publishes only after initialization, uses
`Never` restart initially, rejects stale teardown, and cannot stop with active
work. The kernel receives only checked identities and budgets; provider meaning
remains outside it.

## Evidence and consequences

The policy WVB is 10,150 bytes at
`b81513e5ac366389b09fd5bce075d6bd480c970ef910250f2d2281e64bb57eed`;
the behavior WVB is 13,333 bytes at
`6692d65d3c428138d157e81d4fde967df181e16337085866e7a253c1b2e8c2ab`.
The focused launch owner passes 32 cases. This establishes the checked service
plan and lifecycle but does not claim that either provider process is running.

## Reconsideration triggers

Change the fixed budgets only from measured provider images and queue evidence.
Add restart only with reserved recovery resources, explicit generation rebind,
and proof that no uncertain mutation is replayed.
