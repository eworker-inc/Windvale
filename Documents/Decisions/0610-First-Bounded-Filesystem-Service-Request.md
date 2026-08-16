# Decision 0610: First bounded filesystem service wire validation

- Status: Implemented current-host native candidate; independent Linux execution pending
- Date: 2026-08-15
- Advances: [Decision 0609](0609-First-Portable-Filesystem-Semantic-Core.md)
- Contract: [filesystem service protocol](../../Specifications/Windvale-Os-Filesystem-Service.md)

## Context

The portable semantic core defines operations and outcomes, but a guest service
and hosted provider still need one bounded untrusted-input boundary. Passing a
Windvale record or native host structure would freeze compiler layout, expose
addresses, and make Windows/Linux representation part of the shared contract.

## Decision

- Add little-endian `WVFQ 1`, a 64-byte header plus at most 65,536 payload
  bytes, with exact total length, operation, correlation, directory/file
  references, `u64` position, control, deadline, and reserved fields.
- Make open the sole path-bearing request. It carries one validated segment and
  a directory reference. Every later operation carries only a generation-safe
  file reference.
- Reserve the deadline field as zero in profile 1 rather than assigning partial
  cancellation semantics prematurely.
- Reject malformed size, identity, version, header, operation, reference
  ownership, segment, transfer, and arithmetic before provider invocation.
- Return only compact admission status from portable policy. Decode operation
  fields after success; do not depend on a compiler/native mixed-width record
  layout at the IPC boundary.
- Add little-endian `WVFP 1` responses that echo operation and correlation and
  carry only semantic status, generation-safe references, exact geometry,
  mutation progress/completion, and a bounded read payload. Validate every
  response against its admitted request and the portable semantic core.

## Evidence and consequences

The exact 20,312-byte self-test WVB has SHA-256
`94b2b72f72a9672b33912adf67261332bd852ae1c0dab2a77f855f40dc18a8c3`.
The focused native owner returns 43 on Windows and constructs deterministic
Windows/Linux images across eleven admitted and rejected request/response
cases. This proves wire validation only. Provider calls, handle inventory,
queue/backpressure, peer loss, and boot integration remain required.

## Reconsideration triggers

Advance the version for any incompatible field or semantic change. Activate a
deadline only with defined observation, cancellation, and indeterminate-
mutation behavior. Never expose a native handle, file descriptor, path, or
compiler record layout in this envelope.
