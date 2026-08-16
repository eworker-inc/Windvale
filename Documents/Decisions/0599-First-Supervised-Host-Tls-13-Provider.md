# Decision 0599: First supervised host TLS 1.3 provider

- Date: 2026-08-15
- Status: Implemented bootstrap candidate with isolated Windows evidence
- Advances: secure-transport portion of network slice 4
- Contract: [host TLS provider](../../Specifications/Host-Tls-Provider.md)
- Builds on: [Decision 0598](0598-First-Supervised-Host-Resolver-And-Stream-Provider.md)

## Context

The supervised host provider now performs real resolver and TCP stream work on
Windows and Linux and has independent execution evidence on both hosts. HTTPS
still requires peer-authenticated TLS before HTTP parsing or credentials can be
introduced. Windvale has no admitted native cryptographic provider, immutable
platform trust-snapshot interface, or TLS implementation today, while its
pinned Node runtime supplies TLS 1.3 and a bundled root set on both hosts.

Adding TLS inside a model-specific tool would duplicate service identity,
trust, deadline, and teardown policy. Accepting arbitrary URLs, trust roots,
verification callbacks, or client keys from requests would also turn transport
mechanism into ambient authority.

## Decision

- Add a separately launched `Host-Tls-Provider` that reuses the supervised
  `WVNR/WVNS 1` stream mechanism only after TLS authentication succeeds.
- Bind the child to one DNS service, TCP port, provider generation, trust
  generation, exact trust-snapshot SHA-256, ALPN value, and finite stream limits.
- Require TLS exactly 1.3, normal certificate-chain and service-name
  verification for the original DNS service, the exact ALPN result, and a
  nonempty peer certificate before publishing the connection.
- Admit either the pinned Node runtime's ordered bundled roots or one explicitly
  pinned certificate. Independently recompute the canonical snapshot digest in
  the supervisor and child. Do not inherit ambient host trust changes.
- Supply no session and expose no application write before `secureConnect`, so
  version 1 sends no early data.
- Give the child no private key, bearer credential, URL, proxy, redirect,
  arbitrary header, filesystem path, listener, datagram, or raw-packet grant.
- Generate all isolated test keys and certificates only in memory and commit no
  private-key fixture.

## Consequences

Windvale now has an executable supervised TLS 1.3 stream mechanism suitable for
building and testing bounded HTTP/1.1 on an isolated peer. It preserves exact
origin authority, monotonic deadlines, byte budgets, half-close, indeterminate
write, and provider-loss behavior from the underlying host provider.

This is a bootstrap candidate rather than final production secure transport.
The current evidence is Windows until the shared owner executes independently
on Linux. Authentication, protocol, and connection failures currently collapse
to one non-sensitive unavailable status. A native capability/timer bridge,
typed secure peer evidence, explicit trust/protocol failure results, and later
Node-free platform leaves remain production-promotion work. HTTP, credentials,
and the model gateway are still absent.

## Reconsideration triggers

Revisit this decision when bounded HTTP needs richer peer evidence; when public
PKI policy needs civil-time, revocation, or platform roots beyond the pinned
Node snapshot; when client authentication introduces protected key operations;
when session resumption is considered; or when native Windows/Linux TLS leaves
can replace the bootstrap mechanism without changing the secure-stream contract.
