# Decision 0718: Snapshot checked application-start requests

- Status: Implemented internal x86-64 leaf; public syscall pending
- Date: 2026-08-16
- Advances: [application-start user-copy policy 1](../../Specifications/Windvale-Os-Application-Start-User-Copy.md)

## Context

Windvale already had an architecture-neutral immutable copy policy and exact
`WVSR 1` decoder, but no native machine boundary performed the bounded request
snapshot. Connecting the portable decoder directly to caller memory would let
the caller change bytes between checks and would leave caller identity under
caller control.

## Decision

- Add one internal x86-64 leaf for an already admitted exact 4,096-byte current-
  process window.
- Require an exact 64-byte source range and a nonzero, aligned, kernel-owned
  destination that does not overlap the caller window.
- Clear the destination before copying, copy exactly eight qwords, validate
  every `WVSR 1` field, and compare the copied caller reference with an
  independently supplied current-caller reference.
- Clear all copied qwords again when caller or payload validation fails.
- Return stable internal statuses for size, window, range, caller, and payload
  rejection.
- Keep page ownership, accessibility, stabilization, fault containment, caller
  derivation, syscall numbering, process construction, and publication outside
  this leaf.

## Consequences

The architecture now has executable native evidence for the small copy-and-
validate core without claiming a public start operation. Work and memory are
constant: one 64-byte kernel snapshot and fixed eight-iteration clear/copy
loops. The 799-byte WVO has SHA-256
`74978b1f6124517b44205cba52aaf6c161cf5d00e39ff9ab3ad883d527c87ddb`;
the ten-case linked image is 4,288 bytes at SHA-256
`19411b99859049d7453bd17c3d473e0141122213b39d9c9f4be5356c6b495cc1`
and packages deterministically for Windows and Linux.

This leaf can fault if a caller violates its precondition. Therefore the next
slice must derive and stabilize the current process's page at the privileged
entry boundary and convert access failure to a defined rejection before the
leaf becomes public.

## Reconsideration triggers

Replace this exact-page interface only if the public ABI requires a larger
request, a multi-page immutable transport, or a different architecture fault-
containment mechanism. Preserve the independent caller check and rejection
erasure in any successor.
