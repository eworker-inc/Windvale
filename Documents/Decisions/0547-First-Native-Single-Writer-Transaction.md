# Decision 0547: First native single-writer transaction

- Date: 2026-08-14
- Status: Implemented candidate with focused Windows interruption and restart evidence
- Requires: [Decision 0535](0535-First-Durable-Database-Commit.md),
  [Decision 0536](0536-Nested-Records-And-Database-Storage-Recovery.md), and
  [Decision 0544](0544-First-Native-Durable-Storage-Provider.md)
- Defines: [Single-writer transaction 1](../../Specifications/Windvale-Database-Single-Writer-Transaction.md)

## Context

Windvale could encode durable pages and commit records, plan publication, bind
one fenced storage object, and recover an unpublished tail. Those pieces had
not yet produced and executed one coherent transaction. The hosted fixture
also duplicated provider-result mapping instead of placing authority at a
reusable platform boundary.

A real interruption test exposed a lifetime rule that the storage contract had
not stated precisely: consecutive capability calls reuse one provider response
scratch buffer. Retaining a first read payload while issuing a second read can
therefore observe overwritten bytes even though the copied scalar result fields
remain valid.

## Decision

- Add one portable builder that accepts a freshly selected tail-free `WVDS 1`
  generation and constructs exactly one new root page, one compact commit-log
  page, one inactive superblock, and their validated publication state.
- Keep the first transaction append-only and root-depth-one. Allocate exactly
  two pages, increment generation and sequence once, and reject all arithmetic
  exhaustion before byte construction.
- Add one hosted executor that is the only new layer declaring
  `storage.random_access_v1`. It maps typed write, resize, and flush actions to
  provider calls and preserves partial, stale, rejected, and indeterminate
  outcomes without retries.
- Give publication and recovery execution explicit action budgets so a caller
  can stop at any durable transition without introducing a second state model.
- State that a read payload is borrowed only until the next call to the same
  storage capability. Decode, validate, or copy it before another call.
- Model unclean stop in the focused native test shell by returning dedicated
  results after zero through four actions and letting process teardown close the
  handle. Do not add test interruption behavior to the source capability or
  product ABI.
- On restart, accept only a fully valid old or new generation. Never infer a
  commit from an unvalidated tail and never replay an uncertain mutation.

## Evidence

The standalone portable fixture compiled in 2.440 seconds, lowered to a valid
1,873,039-byte WVO in 2.548 seconds, and executed all format, determinism,
boundary, and rejection assertions in 1.077 seconds. The composed cached
Windows lifecycle passed in 107.296 seconds. The final changed-file gate passed
10 database-storage, 8 workspace/project, and 26 library cases in 687.317
seconds.

Five interruption scenarios stop after zero, one, two, three, or four
completed publication actions. Each restarts through fresh superblock
selection and recovery, accepts only the old 4,608-byte or new 12,800-byte
committed generation, and then passes a stable reopen. The equivalent Linux
application is constructed but has not yet supplied independent execution
evidence.

## Consequences

- Stage 3 now has a real Windvale-native single-writer commit path rather than
  disconnected format and provider probes.
- Portable commit construction remains independently testable and has no
  storage authority.
- Hosted provider mapping has one reusable owner for publication and recovery.
- The borrowed-response rule prevents hidden aliasing bugs in future page and
  catalog readers.
- This is not yet a key/value engine or server. Tree-node formats, insertion,
  page ownership, catalog, concurrency, protocol, and SQL remain later slices.

## Reconsideration triggers

Version the transaction contract before allocating a variable number of pages,
publishing root depth above one, adding reclamation, allowing multiple writers,
retaining read responses across provider calls, introducing durable mutation
identities, or changing the recovery ambiguity accepted at the inactive-slot
write boundary.
