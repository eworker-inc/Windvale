# Decision 0580: Recoverable offline package uninstall

- Status: Implemented; paired-host evidence passed in GitHub run `31906316540`
- Date: 2026-08-15
- Advances: Milestone 4 and Decision 0568
- Depends on: Decisions 0561 and 0578
- Contract: [Package bundle and installation](../Architecture/Windvale-Package-Bundle-And-Installation.md)

## Context

The offline lifecycle can authenticate two real packages, publish immutable
generations, activate, execute, recover, and roll back. Completion also requires
uninstall that cannot delete separately owned application data or an unrelated
user-selected directory. Removing several mutable and immutable directories is
not one atomic filesystem operation, so interruption must have an explicit
recoverable meaning.

The bounded installation layout gives this adapter exactly three owned roots:
`state`, `generations`, and `store`. The installation root, application data, and
all other sibling entries have separate ownership.

## Decision

Add a narrow Windows/Linux host adapter for the bounded offline package state.
Before mutation it rejects links, nonordinary files, excessive inventory, and
names outside the exact Activation 1, Generation 1, and SHA-256 store layouts.
An interrupted activation or generation publication must first pass its existing
recovery adapter; uninstall does not reinterpret an ambiguous candidate.

Uninstall creates `.windvale-uninstall-1/Uninstall-1.txt`, flushes the exact
canonical transaction record, and then moves owned roots into that quarantine in
this order:

1. `state`, which prevents new command resolution;
2. `generations`; and
3. `store`.

Each move remains inside the installation root and is followed by directory
durability where the host can provide it. Only after every existing owned root is
quarantined does the adapter revalidate and remove those quarantined trees. It
then removes the transaction record and directory. It never removes the
installation root.

Recovery treats a canonical transaction record as a committed uninstall: it
validates both source and quarantined inventory, finishes missing moves, and
finishes removal. An empty transaction directory has no committed record and is
discarded without changing owned state. A malformed or ambiguous transaction is
preserved and rejected.

## Consequences

- New launches fail after `state` is quarantined; a process that already holds a
  private verified executable copy may finish.
- Repeating a completed uninstall is an idempotent success.
- Application data and unrelated root entries remain byte-identical and the
  caller decides whether to remove them separately.
- This is a bounded host adapter using the pinned Node.js development runtime. It
  is not retroactively added to the immutable `v0.1.0` installers.
- Garbage collection, signed revocation/minimum-version policy, and the future
  installed package-client UI remain separate work.

The focused owner covers 13 complete, interrupted, empty, linked, malformed,
unknown-inventory, preservation, and repeat cases. The composed lifecycle adds
uninstall and preservation after serial-3 rollback, for 27 total planner and
lifecycle cases. GitHub run `31906316540` passed both reports on Windows and
Linux at exact implementation commit `df2d15dad0434182b74ad7ae357b4596d4aef82d`.

## Reconsideration triggers

Reconsider the three-root inventory when an installed client owns additional
state, package-store garbage collection becomes concurrent, application data
moves beneath a package-owned root, or native anchored-directory handles replace
the current bounded host adapter.
