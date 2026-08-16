# Decision 0611: Capacity-one filesystem provider and host open plans

- Status: Implemented current-host native candidate; independent Linux execution pending
- Date: 2026-08-15
- Advances: [Decision 0610](0610-First-Bounded-Filesystem-Service-Request.md)
- Contract: [filesystem service protocol](../../Specifications/Windvale-Os-Filesystem-Service.md)

## Context

Validated bytes alone cannot safely own a native file. The first provider needs
an explicit, bounded lifecycle and Windows/Linux translation policy before
native syscall leaves can be connected. Using a native handle as the public
reference would make stale reuse and client teardown unsafe.

## Decision

- Begin with one active handle. Keep its native token private and publish a
  generation-stamped `u64` reference owned by exactly one caller.
- Enforce the open profile again at operation authorization. Read-only permits
  read and close; write-capable profiles permit the complete bounded set.
- Advance the generation after completed close. Reject references from prior
  generations even when the sole slot is reused.
- Move indeterminate close and peer exit into `Stopping`; retain the native
  token until explicit provider release completes reclamation.
- Translate all four profiles into exact Windows and Linux open plans. Require
  no-link flags and a post-open regular-file check on both hosts. Keep native
  status capture and syscall execution outside this portable policy.

## Evidence and consequences

The exact composed 33,871-byte self-test WVB has SHA-256
`e2b9279e18676c1a6e3ede3a92d6dee21305c70b14e2f37826ad70b4f2637133`.
The focused owner returns 43 on Windows and constructs deterministic Windows and
Linux images across nineteen wire, host-plan, ownership, rights, generation,
and teardown cases. The Windows and Linux native syscall leaves, multi-handle
inventory, queue/backpressure, resource-domain charge, and guest launch remain
active work.

## Reconsideration triggers

Increase capacity only with bounded allocation, accounting, and teardown tests.
Change the public reference encoding only with a versioned wire change. Never
weaken no-link or regular-file enforcement to approximate cross-host behavior.
