# Decision 0603: First bounded HTTPS client

- Date: 2026-08-15
- Status: Implemented candidate with independent Windows/Linux evidence
- Advances: network slice 5
- Contract: [bounded HTTPS](../../Specifications/Bounded-Https.md)
- Builds on: [Decision 0599](0599-First-Supervised-Host-Tls-13-Provider.md)

## Context

Windvale now has dual-host resolver/TCP and TLS 1.3 bootstrap providers, but a
secure byte stream is not yet HTTPS. Package and model consumers need one shared
HTTP boundary that rejects request-controlled authority, ambiguous response
framing, unbounded bodies, automatic redirects, and uncertain mutation replay.

Using Node `fetch` inside each consumer would inherit URL, redirect, proxy,
decompression, pooling, and implementation-specific framing behavior at the
wrong boundary. Implementing a full general-purpose browser client would add
state and protocol families not required by the first consumers.

## Decision

- Add a shared hosted HTTP/1.1 codec and HTTPS client under
  `Runtime/Hosted/Http/` above the supervised TLS provider.
- Bind each client to one service/port/trust/provider identity, an exact target
  set, selected non-authority header names, and finite request/header/body/wire/
  deadline limits.
- Support one GET or POST on one new authenticated connection. Own `Host`,
  `Connection: close`, and exact POST `Content-Length`; prohibit caller control
  of framing, authority, cookies, upgrades, and expect/continue.
- Accept exactly one canonical content length or the sole transfer coding
  `chunked`. Reject ambiguous/repeated lengths, close-delimited bodies, chunk
  extensions, trailers, truncation, and excess bytes.
- Reject compression, cookies, upgrades, informational responses, obsolete
  folding, controls, and unsupported status/framing behavior.
- Surface 3xx responses without following them. Never convert `Location` into
  authority.
- Require exact complete local acceptance of the request. Partial acceptance or
  uncertain post-dispatch failure is indeterminate and is never retried.
- Use one overall monotonic deadline and normalize provider exceptions to fixed
  HTTP failure kinds without leaking host diagnostics.

## Consequences

Windvale has the first executable shared HTTPS request/response path rather than
only encrypted bytes. It can now exercise exact provider endpoints on isolated
peers with strict limits and without credentials. The external-model gateway
can reuse this framing after protected credential custody supplies an internal
authorization field; portable callers still receive no URL or header map.

This remains a hosted bootstrap candidate. The Windvale capability/timer bridge,
provider JSON validation,
and an end-to-end supervised model gateway remain required. The first profile
does not follow redirects or implement HTTP/2, HTTP/3, pooling, compression,
cookies, caching, proxies, or retries.

## Reconsideration triggers

Revisit this decision when a qualified consumer requires carefully bounded
same-origin redirects, streamed artifact publication, connection reuse,
interim responses, selected trailers, compression, HTTP/2, or a response status
that the current fixed failure/result model cannot preserve exactly.
