# Workload 5 HTTP and reliable-stream contract

## Status

This is the smallest paper-only provider and protocol contract required to type
and review workload 5. Its general Language 1.0 and Foundation findings are
accepted by
[Decision 0759](../../../Decisions/0759-Resolve-Language-1.0-Http-Handler-Findings.md).
The async/context/endpoint completion is accepted by
[Decision 0760](../../../Decisions/0760-Resolve-Language-1.0-Concurrent-Service-Findings.md).
This document does not claim a native provider.

## Bound service authority

`Httpˉapplication` declares exactly:

```text
requires capability network.service.accept version 1;
```

The launcher approves one service binding and supplies a rights-limited provider
instance. The binding, not application source, selects:

- one listener or service endpoint;
- whether the byte stream is authenticated secure transport or an explicitly
  admitted non-production plain transport;
- one provider identity and nonzero generation;
- maximum accepted connections, queued bytes, transfer bytes, operations, and
  deadline span; and
- platform adapter and teardown policy.

The application receives no address enumeration, raw socket, listener handle,
TLS session, trust store, certificate, entropy, clock, environment, or native
handle authority. A library requirement is transitive but is not a grant; the
application approval and provider binding remain separate.

## Operation context

`Foundationˉoperation.Operationˉcontext` is a shared immutable opaque value
constructed by the launcher/provider boundary. It carries:

- one nonzero monotonic-clock identity and generation;
- one absolute deadline tick in that generation;
- one nonzero cancellation-view identity and generation; and
- the maximum provider deadline span already proved at binding.

Application source can borrow and pass the context but cannot forge its fields,
read civil time, extend the deadline, or request cancellation. Every accept,
read, and write is an explicit observation point. At an observation tick equal
to the deadline, timeout wins. Cancellation requested before dispatch proves no
operation progress. A cancellation, timeout, loss, restart, or teardown after a
write dispatch may be indeterminate and can never be mapped to known-zero
acceptance.

Workload 6 connects this provider-facing context to lexical task scopes. Task
construction derives a narrower child view; `Task.Requestˉcancel` marks that
view, and every awaited provider call observes it. The context remains neither
a keyword nor a second cancellation model.

## Paper stream surface

The imported `Platformˉstream` types have these shapes:

```text
export record Streamˉlimits {
    Maximumˉreadˉbytes: u64;
    Maximumˉwriteˉbytes: u64;
    Maximumˉtransferˉbytes: u64;
    Maximumˉoperations: u64;
}

export opaque Serviceˉendpoint Copy;
export opaque resource Requestˉstream;

export enum Streamˉfailureˉkind: u8 {
    Rejected;
    Timedˉout;
    Cancelled;
    Peerˉreset;
    Revoked;
    Providerˉlost;
    Providerˉrestarted;
    Invalidˉresponse;
}

export record Streamˉfailure {
    Kind: Streamˉfailureˉkind;
    Expectedˉgeneration: u64;
    Observedˉgeneration: u64;
}

export record Readˉcompletion {
    Delivered: u64;
    Peerˉclosed: bool;
}

export variant Readˉoutcome {
    Completed(Delivered: u64, Peerˉclosed: bool);
    Rejected(Error: Streamˉfailure);
}

export variant Writeˉoutcome {
    Completed(Accepted: u64);
    Rejected(Error: Streamˉfailure);
    Indeterminate(Error: Streamˉfailure);
}
```

`Serviceˉendpoint` binds the approved service identity, exact rights and limits,
provider identity, and one nonzero generation. It is Copy only because this
interface explicitly admits concurrent shared accepts. Copying cannot discover
another service or widen authority.

The capability root supplies asynchronous semantic operations:

```text
async Acceptˉone(
    Endpoint: Serviceˉendpoint,
    Context: borrow Operationˉcontext,
    Limits: Streamˉlimits,
) -> Result<Requestˉstream, Streamˉfailure>
    effects(network.service.accept, resource.acquire, task.suspend)

async Read(
    Stream: borrow mut Requestˉstream,
    Target: Mutableˉslice<u8>,
    Context: borrow Operationˉcontext,
) -> Readˉoutcome effects(network.service.accept, task.suspend)

async Write(
    Stream: borrow mut Requestˉstream,
    Value: Slice<u8>,
    Context: borrow Operationˉcontext,
) -> Writeˉoutcome effects(network.service.accept, task.suspend)

async Refresh(
    Endpoint: Serviceˉendpoint,
    Context: borrow Operationˉcontext,
    Observedˉgeneration: u64,
) -> Result<Serviceˉendpoint, Streamˉfailure>
    effects(network.service.accept, task.suspend)
```

Every call requires explicit `await`. `Refresh` requires exact restart evidence
for `Endpoint` and may return only the same approved service, rights, and limits
at `Observedˉgeneration`. It is not discovery or replay. The handler itself does
not refresh; workload 6 joins old children and uses a refreshed endpoint only
for a fresh request.

`Requestˉstream` is move-only and implements local release. Release always
invalidates the local handle and schedules bounded transport teardown. It does
not claim graceful peer receipt or replace the body result.

### Read meaning

`Completed` reports the exact initialized prefix written into the target. A
positive short delivery is normal. Zero delivery is valid only with
`Peerˉclosed = true`. The provider never returns a hidden partial prefix with a
rejection: if bytes are locally delivered, it completes that call and observes
the later failure at the next call. A count above the supplied slice or zero
without peer closure is a provider defect.

Peer closure after the complete declared body is accepted. Closure before the
header/body boundary is `Earlyˉpeerˉclose`. The read half has no indeterminate
uninitialized-memory state because the caller's fixed buffer begins initialized
and only the reported prefix becomes request input.

### Write meaning

`Completed(Accepted)` proves that exact positive prefix was accepted by the
local provider. It does not prove remote receipt, peer processing, or
application commit. The next call receives only the unsent suffix.

`Rejected` proves zero bytes from that call were accepted. It retains the exact
previously accepted total. `Indeterminate` cannot prove the current call's
accepted count; the handler returns immediately, releases the stream, and never
reconnects or replays. Counts of zero or above the supplied suffix are provider
defects.

## Strict request profile

The workload admits one request and one response per stream:

```text
request-line = method SP origin-target SP "HTTP/1.1" CRLF
header-line  = token ":" SP visible-ascii CRLF
request      = request-line *header-line CRLF exact-body
```

Rules are intentionally narrower than general HTTP/1.1:

- CRLF is exact; bare CR/LF, folding, control bytes, tabs, and trailing space
  around the field name are rejected;
- the target is nonempty visible ASCII origin form; only `/health` and `/echo`
  route successfully;
- `Host` is required once and nonempty;
- `Content-Length`, `Content-Type`, and `Connection` are singleton recognized
  fields and duplicates are rejected case-insensitively;
- `Transfer-Encoding` is always rejected, so no request has two framing rules;
- unknown valid headers are counted and ignored; they may repeat because they
  cannot affect framing, routing, decoding, or authority;
- `Content-Length` is canonical unsigned ASCII decimal with no sign,
  whitespace, or multi-digit leading zero;
- explicit `Connection`, if present, must be `close` case-insensitively;
- `GET /health` has zero body;
- `POST /echo` requires `Content-Length` and exact case-insensitive
  `text/plain; charset=utf-8`, then strict UTF-8 decode; and
- bytes after the declared body are rejected as pipelining/trailing input.

The handler responds once with `Connection: close`. It does not parse or emit
chunked encoding, keep-alive reuse, upgrade, informational responses, trailers,
compression, or ambiguous whitespace.

## Deterministic headers

Recognized headers use
`Map<Headerˉname, Headerˉvalue>` with the order `Host`, `Contentˉlength`,
`Contentˉtype`, `Connection`. Values are Copy byte ranges into the one input
buffer. The map has maximum four items and stores no borrow. Rank lookup and
one-owner borrowed observation follow Decision 0758. Unknown fields are never
retained.

## No hidden retry or completion

The handler performs no accept retry, read retry after a typed failure, write
retry after uncertainty, automatic deadline extension, connection replacement,
or graceful-close claim. Positive known-short reads/writes are progress, not
retries: each next call addresses only bytes not already delivered/accepted.
