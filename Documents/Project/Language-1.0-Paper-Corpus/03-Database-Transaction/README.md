# Language 1.0 paper workload 3: database transaction

## Status

Complete first-author bundle, draft reviewed by the project owner on
2026-08-17. [Decision 0757](../../../Decisions/0757-Resolve-Language-1.0-Database-Transaction-Findings.md)
accepts the general collection, schema, transaction-completion, and recovery
findings. The paper capability name and its application-specific typed schema
remain provisional until the later service and package workloads reconcile the
catalog.

This is Language 1.0 design evidence. It is not accepted by the current Seed
compiler, does not implement a database provider, and does not freeze edition 1.

## Result

The candidate language expresses one bounded update without packed application
state or hidden transaction behavior:

1. three named input fields become a `Customerˉrow` through strict parsers;
2. a typed arena owns the parsed values and an ordered map relates field keys to
   generation-checked handles;
3. canonical map iteration validates schema order;
4. one move-owned transaction performs lookup and stages one typed update;
5. explicit commit reports completion, conflict, rejection, or uncertainty; and
6. lexical release invalidates the local session without committing or retrying.

An indeterminate commit returns its stable commit identity and requires a fresh
provider/session reopen. Neither the same handle nor the source application may
blindly replay the update.

## Source modules

| Module | Profile | Responsibility |
| --- | --- | --- |
| `Transactionˉtypes` | Core | Input, row, error-domain, and report types. |
| `Transactionˉordering` | Core | The one total order for the three schema keys. |
| `Transactionˉparser` | Core | Strict parsing, arena ownership, checked handles, ordered map, and typed row construction. |
| `Transactionˉapplication` | Hosted | Budget split, transaction acquisition, lookup, stage, commit, and result adaptation. |

The source is under [Source](Source). It imports one application-specific
`Platformˉcustomerˉdatabase` typed adapter. That adapter has an explicit schema;
it is not a reflective serializer and does not expose storage pages or provider
internals.

## Evidence index

- [Package plan](Package-Plan.md) fixes module mapping, target/profile metadata,
  bounds, and the provisional launch contract.
- [Database contract](Database-Contract.md) fixes the explicit row schema,
  typed transaction surface, state machine, commit outcomes, and recovery rule.
- [Semantic review](Semantic-Review.md) inventories values, ownership, effects,
  cleanup, compiler planning, resources, and artifacts.
- [Rejected cases](Rejected-Cases.md) covers every required malformed, boundary,
  ownership, and uncertainty case.
- [Expected outcomes](Expected-Outcomes.md) gives backend-independent result and
  recovery oracles.
- [Implementation responsibilities](Implementation-Responsibilities.md) keeps
  language, Foundation, provider, launcher, storage, and target work separate.
- [Review findings](Review-Findings.md) records the five owner resolutions and
  the remaining provisional identities.

## Acceptance answer

Typed state is visibly clearer than packed bytes: schema fields, parsed values,
database failures, lookup outcomes, and commit outcomes are nominal records or
variants. The only transaction owner is the `using Transaction` binding. Commit
is explicit and fallible; release is automatic, local, and noncommitting.
Provider uncertainty is visible in the application result without requiring a
reader to know WAL, superblock, or flush mechanics.

## Nonclaims

The bundle does not select SQL, a query language, reflection, object-relational
mapping, a general database capability, multi-writer scheduling, distributed
transactions, automatic retry, or a permanent on-disk format. The existing
Windvale typed-row and persistent single-writer specifications remain concrete
implementation evidence, not the semantic definition of this source API.
