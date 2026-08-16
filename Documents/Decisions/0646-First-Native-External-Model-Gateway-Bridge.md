# Decision 0646: First native external-model gateway bridge

- Date: 2026-08-16
- Status: Implemented candidate; Windows execution complete, independent Linux execution pending
- Advances: external-model gateway production path
- Contract: [native external-model gateway bridge](../../Specifications/Native-External-Model-Gateway-Bridge.md)
- Builds on: [Decision 0605](0605-First-Supervised-External-Model-Gateway.md)

## Context

The supervised gateway already owns fixed provider mappings, protected
credential custody, bounded HTTPS, TLS, resolver/TCP, and canonical model
records. Windvale source already lowers catalog and inference to exact ABI-23
provider calls. Those two implemented halves were not connected: only a
JavaScript caller could invoke the gateway, while native Windvale execution was
bound to a deterministic scripted provider.

The first connection must preserve the existing model protocol and capability
table rather than expose sockets or credentials to generated code. It also must
give blocking native I/O a supervising timer and teardown owner on both hosts.

## Decision

- Add one platform-neutral WVA host that binds the existing two model provider
  identities and synchronously exchanges canonical records through two
  supervisor-dedicated pipes.
- Keep the worker model-only: standard input/output are private transport in
  this profile and cannot simultaneously be portable console or ambient input.
- Add thin Linux syscall and Windows admitted-handle leaves with exact partial
  read/write loops. Keep every native handle and function pointer outside the
  provider table and portable application.
- Bound request and response sizes, independently revalidate operation, framing,
  total length, response kind, and request identity, and erase response scratch
  at its borrowed-lifetime transitions.
- Add a launcher that authenticates gateway readiness before worker launch,
  supplies an empty worker environment, permits one request in flight, owns
  operation/lifetime timers, bounds diagnostics, and tears down both processes
  on any protocol or lifetime failure.
- Never retry. If the gateway becomes unavailable after generation submission,
  return canonical submission-indeterminate bytes when the launcher can still
  answer the native peer; return catalog unavailable for the read-only case.
- Qualify composition with an independent ABI-23 WVA probe and retain the
  existing source-lowering owner as separate compiler evidence.

## Consequences

There is now a concrete native product path from the Windvale provider-call ABI
to the same supervised gateway used by hosted callers. Native code receives only
borrowed canonical response bytes; network, trust, timer, credential, and
provider authority stay in supervised host processes. The deterministic Windows
run proves the complete process/pipe/credential/gateway/native path without a
public request, and the owner constructs both host images from identical shared
objects.

Independent Linux execution remains required before dual-host qualification.
The dedicated-standard-channel worker is intentionally narrower than a general
application host. A later service manager can bind separate inherited channels,
and operational secret input can move to an OS keyring, HSM, or protected
interactive unlock path. Streaming, concurrent inference, routing, and a live
provider smoke remain separate promotion work.

## Reconsideration triggers

Revisit this decision when a general launcher can bind named rights-limited
process channels, a native secure transport replaces the hosted bootstrap, the
model API gains streaming/concurrency, or operating-system credential custody
can remove passphrase material from the launcher caller.
