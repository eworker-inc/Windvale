# Decision 0620: First checked OS provider launch transaction

- Status: Implemented portable policy candidate; privileged boot binding pending
- Date: 2026-08-15
- Advances: [Decision 0619](0619-Boot-Embedded-Os-Provider-Process-Images.md)
- Contract: [provider launch transaction](../../Specifications/Windvale-Os-Provider-Launch-Transaction.md)

## Context

Decision 0582 made the exact filesystem and network images available inside the
boot object but deliberately did not equate embedding with launch. The next
boundary needs one failure-atomic policy joining service admission, resource
accounting, image geometry, W^X construction evidence, endpoint binding,
readiness publication, stale-reference rejection, active-work drain, and full
teardown.

## Decision

- Admit the filesystem as one process and one endpoint in domain `65538`, with
  48 RX image pages and 16 private RW/NX pages under its 64-page ceiling.
- Admit the network provider in domain `65539`, with 60 RX image pages and 36
  private RW/NX pages under its 96-page ceiling.
- Require the exact embedded byte length and identity before reservation.
- Commit only after complete private construction; publish only after readiness.
- On construction failure, discard the unpublished reservation and stop the
  empty domain. On readiness failure, release the complete committed charge
  before stopping it.
- Reject stale teardown and refuse final teardown while work remains.
- Keep the construction and lifecycle source graphs linear because the current
  compiler correctly rejects the direct-plus-transitive import diamond.

## Evidence and consequences

The lifecycle policy is 28,419 bytes at
`1de860ff0dcf591d834ae508af341a07413b828c7459a0615ed2b5cef94eff5c`.
`os-provider-launch-transaction` passes 15 cases: three project builds, two
executions, and ten behavior cases. Construction returns 48 and lifecycle
returns 49.

This closes portable provider construction and teardown policy. It does not
change the current three-process architecture fixture and therefore does not
claim a running filesystem or network provider. The privileged process-memory,
page-table, endpoint-table, dispatcher, and syscall changes remain the next
integration boundary.

A measured direct composition with the existing `Process-Foundation` source
was rejected at the compiler's bounded source-binding evidence table before
lowering. The experiment was removed and no boot identity changed. The next
integration must split or replace that nearly saturated fixed policy/fixture
boundary; it must not substitute duplicated constants for this transaction.
[Decision 0621](0621-First-Windvale-Owned-Process-Machine-Code-Emission.md)
begins that replacement with the checked x86-64 emission primitive recovered
from the archived process constructor's smallest cohesive seam.

## Reconsideration triggers

Change the page partition only when a new exact linked image or measured stack,
heap, queue, or recovery requirement changes. Replace the fixed references only
with a generation-safe dynamic object-table contract. Never weaken rollback or
publish a provider before all mappings, bindings, charges, and readiness agree.
