# Decision 0584: Identity-gated database create or open

- Date: 2026-08-15
- Status: Implemented candidate with focused Windows execution evidence
- Requires: [Decision 0581](0581-Deterministic-Durable-Database-Bootstrap.md), [Decision 0575](0575-Single-Writer-Database-Engine-Lifecycle.md)
- Advances: [database engine lifecycle](../../Specifications/Windvale-Database-Engine-Lifecycle.md)
- Retains: rights-limited storage, exact initial-image admission, provider
  fencing, bounded tail recovery, and no uncertain mutation replay

## Context

Bootstrap could initialize an empty storage object and the engine could open or
recover an existing database, but callers still had to compose the two state
machines correctly. A naïve composition could open a valid database with the
wrong identity or, worse, repair its unpublished tail before discovering that
it was not the database the caller intended to access.

The first hosted bootstrap also retained two provider read results at once.
The native provider deliberately reuses bounded response scratch, so the
second read could invalidate the first borrowed byte view before exact resume
admission.

## Decision

- Add `Durableˉdatabaseˉlifecycle` as the hosted create-or-open composition over
  bootstrap and engine lifecycle.
- Require the expected nonzero database identity and exact durable page size on
  every call.
- Return typed created, resumed, opened, recovered, active, reopen-required,
  invalid-request, not-database, identity-mismatch, page-size-mismatch, storage,
  creation, and open outcomes while retaining both lower-layer results.
- Do not attempt engine open after invalid, active, rejected, uncertain, or
  failed creation.
- Observe an existing engine with a zero recovery-action budget, validate its
  selected identity and page size, and only then authorize bounded tail
  recovery. Revalidate after recovery.
- Distinguish fresh `Created` bootstrap completion from byte-exact nonempty
  `Resumed` completion.
- Copy the first hosted bootstrap read into owned bytes before issuing the
  second provider call.
- Keep tree lookup in the dedicated reader target. Combining lifecycle,
  bootstrap, engine, and reader in one ordinary object crosses the deliberate
  object-size boundary without adding a semantic guarantee.

## Evidence

The hosted lifecycle and revised engine fixture compile through the native
front door. Provider-backed Windows execution proves invalid requests do not
attempt engine open, zero-action creation remains visible, an empty object is
created and opened, the canonical initial image resumes without byte changes,
an evolved database opens, wrong identity and page size fail before mutation,
tail recovery remains bounded, and a truncated header is not admitted as a
database. The response-scratch regression is exercised by the stable initial
reopen that requires both exact header and root reads.

## Consequences

- Server code now has one engine-ready create-or-open entry point without
  duplicating bootstrap or recovery policy.
- Expected identity is an authorization precondition for recovery, not merely
  metadata checked after mutation.
- Lower-layer bootstrap and engine evidence remains available for diagnostics.
- Database-storage remains 24 retirement cases and fifteen development targets;
  the existing engine target owns the deeper lifecycle scenarios. The complete
  inventory remains 69 suites and 3,553 cases.
- Collection operations, session ownership, configurable storage-object
  lifecycle, authentication, networking, concurrent clients, and server
  supervision remain later milestones.

## Reconsideration triggers

Split create and open into separately granted capabilities if deployment policy
must forbid creation on an empty binding. Add a database-format identity above
the current record versions before compatible migrations exist. Introduce a
session resource only when source-level close, cancellation, and concurrent
request ownership are defined.
