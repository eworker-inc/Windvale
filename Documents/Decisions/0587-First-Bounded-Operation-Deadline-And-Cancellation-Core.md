# Decision 0587: First bounded operation, deadline, and cancellation core

- Date: 2026-08-15
- Status: Implemented candidate with focused Windows execution evidence
- Implements: network slice 1 from the
  [networking foundation plan](../Project/Windvale-Networking-Foundation-Implementation-Plan.md)
- Enables: future semantic streams, secure transport, HTTP, package retrieval,
  and live external-model adapters

## Context

The provider-neutral model protocol and bound native model provider can now
publish success, revocation, stale generation, peer exit, and indeterminate
submission. They intentionally do not contact a public endpoint. Windvale's
networking plan requires a shared asynchronous operation model before any
network call so HTTPS does not introduce a model-specific event loop, elapsed
wall-clock timeout, or unsafe retry rule.

The current source and native toolchain can execute immutable nominal state,
checked `u64` arithmetic, bounded bytes, generation-aware transitions, and
deterministic cross-host native images. It still has no native monotonic timer,
resolver/connect, entropy, trust, TLS, or HTTP provider.

Current standards reinforce the planned boundary. New TLS protocols require
TLS 1.3 by default; HTTPS clients verify the service identity for the URI
origin; HTTP/1.1 recipients must reject ambiguous framing; and TLS 1.3 early data
requires explicit application-level replay policy. Windvale therefore needs
exact deadline and indeterminate-submission semantics before a secure HTTP
provider can be honest.

## Decision

- Add the portable `Windvaleˉboundedˉoperationˉcore` under
  `Libraries/Foundation/Operations/`.
- Bind every operation to opaque nonzero provider, operation, and monotonic-clock
  identities and generations.
- Use absolute monotonic ticks. At the deadline tick, timeout wins over another
  observation presented at that tick.
- Represent immediate rejection separately from accepted queued work. Permit
  immediate completion from the queued state and cumulative partial progress
  only after dispatch.
- Complete every accepted operation at most once. Treat later terminal events as
  invalid state rather than replacing the first outcome.
- Map cancellation, loss, restart, and teardown after mutating dispatch to
  `Submissionˉindeterminate` while retaining the exact terminal cause. A deadline
  after mutating dispatch likewise records cause `Deadline` with an indeterminate
  outcome. Never retry such a mutation automatically.
- Add a bounded immutable event queue with two control reservations: one for a
  cancellation and one for provider-wide teardown. Normal exhaustion cannot
  consume either reservation.
- Closing a queue appends teardown evidence, rejects later events, and keeps
  returning `Closed` after the event is drained so waiters cannot sleep on a
  stale generation.
- Keep the queue's fixed internal entry bytes private to this implementation;
  this decision creates no serialized wire format or native wait capability.
- Own the complete ten-group exit corpus with a focused native verifier that
  executes locally and builds the opposite-host image deterministically.

## Consequences

Windvale now has executable shared semantics for the first infrastructure layer
needed by HTTPS and external model adapters. Files, processes, terminals,
devices, and network providers can reuse the same operation lifecycle instead
of defining incompatible cancellation or timeout behavior.

This is not a live network claim. The next coherent network work is slice 2's
strict address, endpoint, and authority model, followed by deterministic stream
and host semantic-provider slices. TLS and HTTP remain above those boundaries.

The current library needs the reconstructed current native compiler closure;
the ordinary pinned library front door has not yet been promoted to that source
generation. The focused owner records this explicitly rather than weakening the
source contract to match an obsolete bootstrap compiler.

## Reconsideration triggers

Revisit the operation core when a concrete stream, file, or process provider
needs a terminal result that cannot be represented without ambiguity; when
measured wait workloads need more than 64 events; when several simultaneous
cancellation requests require distinct identities; or when the first native
timer/wait provider reveals a cross-host ordering rule not captured by explicit
monotonic observations. Any change that permits automatic replay after
indeterminate mutation requires a separate idempotency decision.
