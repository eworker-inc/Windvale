# Decision 0612: First shared bounded operation core

- Status: Implemented current-host native candidate; independent Linux execution pending
- Date: 2026-08-15
- Advances: [networking foundation plan](../Project/Windvale-Networking-Foundation-Implementation-Plan.md)
- Contract: [bounded operation model](../../Specifications/Windvale-Bounded-Operation.md)

## Context

Networking needs deadlines, cancellation, partial progress, provider loss, and
bounded queues, but these semantics are not network-specific. A private socket
event loop would duplicate future filesystem, process, terminal, and device
requirements and would make uncertain mutations easy to replay incorrectly.

## Decision

- Add one capability-free operation model with generation-safe provider and
  operation identities, exact byte progress, virtual monotonic deadlines, and
  terminal evidence.
- Reserve configured queue capacity for cancellation and close/control work.
- Never turn timeout, cancellation, or provider loss of an active mutation into
  a clean rejection. Report `Indeterminate` without provider confirmation.
- Release queue charge exactly once after terminal evidence exists.

## Evidence and consequences

The exact 12,769-byte self-test WVB has SHA-256
`dac9582ae8ea2202fc16e5e15020136b63a668c722dbdab6863a98e07d7ff477`.
The focused native owner returns 44 on Windows and constructs deterministic
Windows/Linux images across twelve lifecycle and failure cases. This starts
network slice 1; wait batches, host wait/timer providers, address values, packet
code, sockets, and guest networking remain open.

## Reconsideration triggers

Advance the contract when measured multi-operation wait batches select their
encoding and capacity. Do not add callbacks or weaken indeterminate mutation
evidence to imitate a host API.
