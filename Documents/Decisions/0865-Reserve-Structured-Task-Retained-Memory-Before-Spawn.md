# Decision 0865: Reserve structured-task retained memory before spawn

- Date: 2026-08-27
- Status: Implemented locally; paired-host reconstruction and integration evidence pending
- Requires: [Decision 0861](0861-Execute-Structured-Tasks-As-Wvb-1.32.md)
- Follows: [Decision 0864](0864-Reserve-Structured-Task-Completion-Slots-Before-Spawn.md)

## Context

Language 1.0 gives every lexical task scope an explicit
`Maximumˉretainedˉbytes` limit. The first sequential WVB 1.32 runtime validated
that limit but did not charge accepted children against it. A scope could
therefore retain continuation frames and terminal outcomes beyond its declared
bound even though child-count, runnable, and completion capacity remained
bounded.

The ownership boundary matters as much as the byte count. Refusal after capture
acceptance would strand moved captures inside a task that the caller never
received. The runtime must either reserve enough state before accepting the
closure or return the exact unchanged closure with a typed reason.

## Decision

- Before accepting `Spawn`, reserve the exact maximum scheduler state retained
  by that child in the sequential WVB 1.32 profile:
  - 40 bytes for its pending continuation;
  - 8 bytes for its terminal-result cell;
  - the complete child local frame; and
  - the newly suspended portion of the parent frame, including resume and
    function words.
- Compare the checked 64-bit reservation with the scope's remaining
  `Maximumˉretainedˉbytes` before task state or work ownership changes.
- On refusal, return
  `Spawnˉfailure.Memoryˉfailure(Allocationˉfailure)` with reason
  `Budgetˉexhausted`, exact requested and available byte counts, and the
  original owned work value.
- Hold an accepted reservation through child completion. Release it only when
  `Await` consumes the affine handle or bounded scope teardown removes the
  child.
- Validate the sum of all active task reservations independently against the
  scope limit.
- Charge heap allocations reachable through captures or outcomes only to their
  explicit memory budgets. The scope keeps those values live but does not
  double-charge their allocation bytes as scheduler state.
- Advance the private fixed task-state encoding from version 1 to version 2,
  store one exact 64-bit reservation in each active task record, and grow the
  bounded state from 8,976 to 10,000 bytes. Keep source syntax and WVB at 1.32.
- Parse every nominal value shape, including callable shape kind 35, through one
  shared width routine. Nested result and spawn-failure variants must not
  misalign their field cursor when an earlier case contains a callable.

The accepted path reuses the preflight call layout rather than rebuilding it.
The rejected path may construct that one bounded layout, but does not allocate
or retain child state.

## Evidence

The runtime-core self-test adds an exact retained-memory boundary. With a
24-byte scope limit, one 16-byte reservation succeeds, a second 16-byte
reservation returns requested `16` and available `8` without changing state,
completion retains the first reservation, and consuming its handle releases
capacity for the next spawn. The task-state core now passes 38 cases.

`Structured-Task-Memory-Limit-Executable.wv` exercises the source-visible nested
failure shape. A one-byte retained limit rejects spawn, matches
`Budgetˉexhausted`, proves requested bytes exceed one and available bytes equal
one, recovers the original work value, and returns `42`. Its exact 4,755-byte
WVB has SHA-256
`92c1c521d4bd1a3198ff01dd54a97fb5153170afe009b6c0111ce06aba51fb64`.

The focused `language-1-memory-budget-split-execution` owner passes 134 cases:
21 valid modules, 69 malformed modules, 19 structured-task cases, the 38-case
runtime core, and the existing ownership, collection, resource, and callable
compatibility evidence. The current local reconstruction candidate is:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Wvb-Runner.wvb` | 446,532 | `56b208d1f892f4bdd1d9c309bb6d4d46257d533a76d79d22efc8f83f27896fbe` |
| `windows-x64-wvrun.exe` | 5,366,784 | `063de8f1fadcf9c37e9cef6526d628b410fa0cd21067fe6f3c795b97623cb519` |
| `linux-x64-wvrun.elf` | 5,365,760 | `6e18c9c9480df40814b81244b3dcd039c8851ded646a240134d4e2969b9c2e71` |

The WVB contains 213 functions and 399,144 code bytes. Segmented staging emits
5,357,511 object bytes in 12 chunks; linking emits a 5,348,533-byte image in
eight chunks at entry offset 105,270; canonical transport uses two chunks.
The focused three-case reconstruction owner rebuilds all three artifacts from
source, proves byte equality, and passes current-host execution, reporting,
usage-rejection, and malformed-module rejection.
The repinned distribution surface passes eight installer cases and twelve
selective installer-repository cases. This is local development evidence;
independent Windows and Linux reconstruction remains required before the
checkpoint becomes a paired-host claim.

## Consequences

Every accepted child now has a bounded owner for its continuation and terminal
outcome before captures move. Backpressure is observable as a typed, exact,
recoverable failure at spawn rather than as a late scheduler trap.

The scheduler performs checked reservation arithmetic and one retained-byte
comparison at spawn. Await and teardown subtract the recorded value. Dispatch,
completion, source syntax, portable bytecode, and explicit heap-budget behavior
otherwise remain unchanged.

The internal task-state version changes deliberately because old state records
lack reservation evidence. No compatibility reader is retained for that private
in-process representation.

## Reconsideration triggers

Reconsider the formula when a later scheduler changes continuation ownership,
introduces separately allocated task records, or admits parallel workers with a
different retained-state layout. Any replacement must remain checked, exact,
published by profile, side-effect-free on rejection, and able to return the
original work and captures.
