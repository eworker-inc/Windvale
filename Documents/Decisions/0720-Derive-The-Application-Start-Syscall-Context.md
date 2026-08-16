# Decision 0720: Derive the application-start syscall context

- Status: Implemented internal context leaf; retained dispatcher cutover pending
- Date: 2026-08-16
- Advances: [Decision 0718](0718-Snapshot-Checked-Application-Start-Requests.md)

## Context

The first native application-start leaf accepts an admitted page and an
independently derived caller reference. Those are correct trust boundaries, but
passing a precombined reference directly from a syscall handler would make it
easy to confuse caller-controlled request state with machine-owned process
state during the retained dispatcher cutover.

## Decision

- Add one internal x86-64 context leaf between the retained syscall machine and
  the bounded copy leaf.
- Accept process id, process generation, and current-process request-page start
  as separate machine-supplied inputs.
- Admit only init process 1 generation 1 for `WVSR 1`, derive reference 65537
  internally, and reject zero or unaligned page context.
- Construct the exclusive one-page end with checked addition and pass only the
  derived values to the existing copy/validation/erasure leaf.
- Preserve the copy leaf's stable size, window, range, caller, and payload
  statuses.
- Keep `GS` offsets, page-presence/stability proof, fault containment, syscall
  numbering, budget accounting, construction, and publication in the retained-
  machine adapter that follows.

## Consequences

The eventual privileged handler no longer needs to assemble a caller reference
in caller-visible memory or duplicate request validation. The context leaf is a
344-byte WVO at SHA-256
`d639056eb9831f89ef3baa33b06b522437d2da4444f74e2db1d58229656dc04b`.
Its nine-case self-test links both leaves into a 4,288-byte image at SHA-256
`3b5b95a0ceb544ca9beac65c3da9fb62ce4cce48dfb0a23e858a76827fd82b6f`,
returns 48, and packages deterministically for Windows and Linux.

This is not a public syscall and does not start an application. The next
machine slice must load these values from retained `GS` state, prove the fixed
RW/NX page remains present, contain access faults, enforce the caller's syscall
budget, and then connect successful admission to construction/publication.

## Reconsideration triggers

Broaden the admitted caller or page selection only when a successor request
version defines its authority and resource profile. Preserve separate machine-
derived identity fields and never accept a caller-supplied precombined identity
as current-process evidence.
