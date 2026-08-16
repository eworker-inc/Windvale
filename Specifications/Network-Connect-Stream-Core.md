# Network Resolve/Connect and Reliable Stream Core

## Status

Implemented candidate under
[Decision 0595](../Documents/Decisions/0595-First-Resolve-Connect-And-Reliable-Stream-Core.md).

## Purpose and boundary

`Windvaleˉnetworkˉconnectˉstreamˉcore` is the capability-free semantic model
between network grants and later Windows, Linux, or Windvale OS providers. It
owns one bounded outbound TCP connect operation and one reliable full-duplex
byte stream with at most one read and one write operation in flight.

It performs no DNS query, route selection, socket call, packet exchange, TLS,
certificate validation, HTTP framing, proxy discovery, credential access, or
automatic reconnect. Native providers remain responsible for those mechanisms
and must translate their results into this contract.

## Resolve and connect

A connect begins from exactly one admitted network grant:

- a service connect requires the exact service name and port, outbound TCP,
  an unexpired grant, and a deadline no later than both grant expiry and its
  maximum deadline span;
- a numeric connect requires an endpoint already contained by an authorized
  prefix grant under the same limits; and
- provider, operation, and later connection identities all carry nonzero
  generations.

A service connect enters `Resolving`. The same provider selects one canonical
endpoint and supplies a nonzero resolution generation before the operation can
become `Connecting`. The selected endpoint must retain the requested port. It is
evidence inside the connect result, not a new prefix grant or reusable numeric
authority. This deliberately keeps name resolution and connection selection in
one authorization decision.

The common bounded-operation core owns dispatch, completion, rejection,
cancellation, deadline, provider-loss, and provider-restart races. Only a
completed operation with a new nonzero connection identity/generation enters
`Connected`. Every other accepted terminal event enters `Terminal` with the
exact common operation outcome.

## Reliable stream

`Networkˉstreamˉopen` derives limits and provider/connection generations from a
successful connect. It rejects expired or structurally invalid connection results. The first
profile permits one active read and one active write; the sum of their remaining
reservations cannot exceed the grant's queued-byte limit.

Each read or write has its own operation identity, generation, monotonic clock
generation, deadline, and requested-byte limit. A request is rejected before
dispatch when it is empty, expired, over the deadline span, over the queue
budget, or over the remaining aggregate transfer budget.

Write progress and completion mean exact bytes accepted by the local provider.
They do not mean remote receipt or application commit. A completion may report
less than the requested limit and the remainder is not replayed. Cancellation,
timeout, provider loss, provider restart, reset, or teardown after write dispatch
can produce `Submissionˉindeterminate`; that closes the usable stream as
`Writeˉindeterminate`. No reconnect or retry is automatic.

Read progress and completion mean exact bytes delivered locally. A completion
can also report orderly peer closure, including a zero-byte end-of-stream. Peer
closure preserves the write half; local shutdown preserves the read half; both
halves closed produces `Closed`. Reset, provider loss, generation change, and
teardown remain distinct terminal phases.

Every provider-originated stream event repeats the exact provider and connection
identity/generation. Stale or cross-connection events are rejected without
changing state.

## Limits and arithmetic

Queued bytes are reservations for unfinished read and write operations. Transfer
bytes are the sum of exact accepted writes and delivered reads. All admission
checks occur before addition, and forged totals that cannot fit the configured
budget are rejected. Grant expiry prevents new stream operations; a later live
provider must also schedule deterministic teardown at expiry.

## Executable evidence

`Connect-Stream-Core-Self-Test.wv` covers thirteen groups: authorized service
connect, denied service authority, direct prefix connect, deadline/expiry,
resolution generation and exact-port binding, successful completion, connect
interruption, stream opening, partial write acceptance, indeterminate write,
read progress and half-close, queue/deadline limits, and provider lifecycle.

The focused native owner builds the library and test twice, requires identical
WVB and WVO output, executes the current-host image, and constructs the
opposite-host image from the same linked input. Independent execution on both
hosts remains required for cross-host qualification.

## HTTPS consequence

This core is still not HTTPS. It supplies the exact resolver/connect and raw
reliable-stream semantics that a rights-limited host provider must implement.
The remaining application path is native resolver/connect, monotonic timer and
stream bindings; secure entropy, civil time, trust, and TLS 1.3; then bounded
HTTP request/response framing and the external-model adapter.

## Standards references

- [RFC 9293: Transmission Control Protocol](https://www.rfc-editor.org/rfc/rfc9293)
- [RFC 8305: Happy Eyeballs Version 2](https://www.rfc-editor.org/rfc/rfc8305)
- [RFC 9846: TLS 1.3](https://www.rfc-editor.org/rfc/rfc9846)
