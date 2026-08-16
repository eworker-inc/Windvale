# Decision 0596: First resolve/connect and reliable-stream core

- Date: 2026-08-15
- Status: Implemented candidate with focused Windows execution evidence
- Refines: the Layer 3 semantic boundary required before network slice 4 in the
  [networking foundation plan](../Project/Windvale-Networking-Foundation-Implementation-Plan.md)
- Enables: host resolver/connect and stream providers, TLS secure streams,
  HTTPS, package retrieval, and live external-model adapters

## Context

Decisions 0587 and 0594 define bounded asynchronous operations and exact
network authority. A host adapter still needs one portable result model for the
security-sensitive step from an authorized service name to a selected address,
and for the partial, half-closed, failed, or generation-stale behavior of a
reliable byte stream.

Exposing resolver output as a numeric grant would create a rebinding and
time-of-check/time-of-use gap. Treating a successful host write as remote
receipt would overstate TCP semantics. Silently retrying after an uncertain
write could duplicate a paid model request or another application mutation.

The existing bounded-operation core can represent exact progress and
indeterminate dispatched mutations, and the current compiler can execute the
necessary immutable state machines without a native network capability.

## Decision

- Add `Windvaleˉnetworkˉconnectˉstreamˉcore` under `Libraries/Network/`.
- Keep service resolution and connection selection in one provider-owned
  operation. Retain the selected canonical endpoint and resolution generation
  only as connection evidence; create no numeric authority from it.
- Require the resolved endpoint to retain the exact requested port and pass the
  canonical endpoint validator.
- Allow direct numeric connection only when the endpoint is already covered by
  a prefix grant.
- Compose connect, read, and write with the common bounded-operation core rather
  than introducing a network-specific event loop.
- Bind every stream event to provider and connection identities/generations.
- Define write progress as exact local-provider acceptance and read progress as
  exact local delivery. Neither implies a remote application commitment.
- Permit exact partial completion without replaying the remainder.
- Make a dispatched write whose final acceptance is unknown terminate the
  stream as `Writeˉindeterminate`; do not reconnect or retry automatically.
- Model peer close and local write shutdown as independent halves, with reset,
  provider loss, restart, and teardown as distinct terminal states.
- Enforce grant queue, aggregate transfer, deadline-span, and lifetime limits
  before operations are accepted.
- Own thirteen deterministic conformance groups with one focused native owner.

## Consequences

Windows, Linux, and later Windvale OS providers can now target the same precise
resolve/connect and byte-stream behavior without exposing sockets, handles,
resolver structures, platform error numbers, or ambient DNS state. Secure
streams and HTTP can be written once above this boundary.

This decision does not add a native provider or make a network connection. It
does not implement DNS, TCP packets, TLS, certificates, HTTP, secrets, or model
vendor JSON. Network slice 3 remains the ordered Windvale OS packet-core work;
this capability-free contract is an explicit prerequisite for the host semantic
providers in slice 4.

The initial concurrency profile is deliberately bounded to one read and one
write operation per stream. A later provider can raise concurrency only through
a new version that preserves ordering, accounting, cancellation, and teardown.

## Reconsideration triggers

Revisit this decision when a measured consumer requires multiple concurrent
operations per direction, datagrams, listening, multipath or QUIC semantics,
provider-independent address racing policy, a longer-lived connection beyond
grant expiry, or a documented idempotency key that safely permits retry after a
specific indeterminate mutation.
