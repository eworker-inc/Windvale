# Decision 0614: First portable network address and authority core

- Status: Implemented current-host native candidate; independent Linux execution pending
- Date: 2026-08-15
- Advances: [networking foundation plan](../Project/Windvale-Networking-Foundation-Implementation-Plan.md)
- Contract: [network address and authority model](../../Specifications/Windvale-Network-Authority.md)

## Context

The shared bounded-operation lifecycle supplies queue, deadline, cancellation,
progress, and provider-loss semantics, but a network provider also needs values
that do not inherit `sockaddr`, host byte order, native interface identifiers, or
ambient socket authority. Name resolution must not manufacture unrestricted
numeric-address access.

The current native source-binding path also makes deeply nested record operations
an unnecessary dependency for this first slice. A flat model keeps each current
compiler and provider boundary explicit while later composed grants remain
possible without changing address meaning.

## Decision

- Represent IPv4 and IPv6 with an explicit family, four network-order `u32`
  words, and a numeric scope; IPv4 requires unused words and scope to be zero.
- Require a nonzero scope for IPv6 link-local addresses and reject scope on
  non-link-local addresses in this first profile.
- Keep prefix records flat. The first executable matcher admits 0 through 4
  complete prefix bytes within one word; arbitrary-bit multiword containment is
  an explicit remainder rather than an accidental host-mask dependency.
- Bound ports to 1 through 65,535, direction rights to connect/listen/datagram
  send/datagram receive, connections to 1,024, and queued bytes to 16 MiB.
- Make `u32`, `u64`, and deadline narrowing non-restoring. Keep service-name and
  resolver authority out of this numeric core.

## Evidence and consequences

The 7,813-byte WVB has SHA-256
`1d3be8e490b5a7927156a57b019ce7fef2956d8793c8085f77d01afa395bf8e4`.
Its focused owner passes 18 cases with result 45 on Windows and constructs exact
Windows and Linux images at SHA-256
`bcbeaf820e970c7369a942ffb2cf407a92c3f399002c2fb478b96588986449a3`
and `95c342a6a027baec2f41aa2959cc78e855463619dd04eb4eaca9aeaa4ac73b9e`.

This starts network slice 2; it does not bind a socket or claim a network
capability. Complete prefix containment, text codecs, composed peer and resolver
grants, published vectors, independent Linux execution, wait batches, provider
IPC, packets, streams, secure transport, and the guest stack remain open.

## Reconsideration triggers

Compose the flat values only when a real provider-binding request selects the
wire layout and response bounds. Do not expose native socket structures or merge
resolution and numeric-address authority to shorten that step.
