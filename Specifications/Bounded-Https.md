# Windvale bounded HTTPS version 1

## Status and purpose

The hosted bootstrap client selected by
[Decision 0600](../Documents/Decisions/0600-First-Bounded-Https-Client.md)
implements one bounded HTTP/1.1 request over one authenticated
[host TLS 1.3 stream](Host-Tls-Provider.md). It is shared infrastructure for
package retrieval and the external-model gateway rather than a model-specific
`fetch`, unrestricted URL client, or portable HTTP semantic definition.

The implementation follows [HTTP semantics](https://www.rfc-editor.org/rfc/rfc9110)
and [HTTP/1.1 message framing](https://www.rfc-editor.org/rfc/rfc9112) while
selecting a deliberately stricter first profile. The current local execution
evidence is Windows. The same source and owner target Linux, but independent
Linux execution remains required.

## Immutable client binding

One client instance binds all of:

- one exact lowercase DNS service and TCP port;
- one TLS provider generation, trust generation, trust snapshot, and exact
  `http/1.1` ALPN;
- one nonempty set of exact origin-form request targets;
- one allow-list of non-authority request header names;
- maximum complete request, response-header, decoded-body, wire, operation-span,
  and provider-lifetime limits; and
- one request per newly authenticated connection.

The caller cannot supply a scheme, authority, absolute URL, proxy, redirect
target, trust root, TLS option, connection-reuse choice, `Host`, `Connection`,
`Content-Length`, `Transfer-Encoding`, `Expect`, `Upgrade`, or `Cookie` field.
Changing a service, port, trust snapshot, target set, or allowed-header set is a
new binding rather than data inside an admitted request.

## Request profile

Version 1 accepts `GET` with no body and `POST` with a body. A target is 1
through 2,048 printable ASCII bytes, begins with `/`, contains no fragment or
backslash, and must equal one bound target. The client owns:

```text
METHOD target HTTP/1.1\r\n
Host: exact-service[:non-443-port]\r\n
Connection: close\r\n
selected admitted fields
[Content-Length: exact-decimal]\r\n
\r\n
body
```

Request fields are unique lowercase token names with trimmed printable-ASCII
values. At most 16 are admitted. A nonempty body requires an admitted
`content-type`. Chunked requests, request compression, cookies, upgrades,
expect/continue, and trailers are absent. The complete headers and body are at
most 65,536 bytes and are submitted in one semantic stream write.

A successful write must report exact acceptance of every request byte. Partial
acceptance or any uncertain post-dispatch failure becomes
`submission_indeterminate`; the client tears down and never reconnects or
replays automatically.

## Response profile

The status line must be exact HTTP/1.1 with a status from 200 through 599 and a
bounded printable reason. Informational responses and protocol switching are
unsupported. Response fields have token names, printable ASCII values, at most
64 entries, at most 4,096 value bytes each, no obsolete folding, and no repeated
name. The complete header block is bounded independently, initially to 16 KiB.

Body framing is exactly one of:

- one canonical nonnegative `Content-Length`; or
- `Transfer-Encoding: chunked` as the sole coding, with canonical hexadecimal
  sizes, no extensions, and an empty trailer section.

`Content-Length` plus `Transfer-Encoding`, repeated lengths, comma-joined or
noncanonical lengths, unsupported transfer codings, close-delimited bodies,
truncation, invalid chunk terminators, trailers, and bytes after the selected
body are rejected. Status 204 and 304 permit no transfer coding and only an
absent or zero content length.

After the framed body, the client requires orderly peer closure before
publishing completion. A later TLS record containing any byte is excess; a peer
that ignores `Connection: close` expires at the same overall deadline. This
finite first profile proves that no unseen second response or trailing data is
silently discarded.

The decoded body is bounded independently, initially to 1 MiB. The wire limit
also bounds chunk overhead and is at least the header plus decoded-body bounds.
`Content-Encoding` is absent or exact `identity`; decompression is not performed.
`Set-Cookie` and `Upgrade` are rejected, so the client acquires no cookie or
protocol-switch state.

Statuses 300 through 399 are returned with `redirect=true` and their bounded
headers. They are never followed. A higher policy may construct a separately
authorized request only after validating its own redirect rules; this client
does not convert `Location` into authority.

## Deadlines, completion, and evidence

One absolute host-monotonic deadline covers provider launch, resolution, TLS
handshake, complete request acceptance, response framing, and body delivery.
Every child operation receives only the remaining span. Deadline, denial, stale
generation, provider loss, ordinary transport failure, framing failure,
truncation, limit, unsupported behavior, and indeterminate submission remain
distinct fixed failure kinds. Raw socket, TLS, child-process, and exception text
does not cross the HTTP boundary.

Completion returns the HTTP status, normalized ordered headers, caller-owned
body bytes, redirect flag, provider/trust generations, selected endpoint
evidence, and exact locally accepted request byte count. It proves receipt of a
complete framed response, not application truth or trust in JSON contents.

## Executable evidence

`Test-Bounded-Https` owns 29 cases. Pure framing cases cover exact GET/POST,
target and header authority, request limits, fragmented content length and
chunking, redirects, ambiguity, duplicates, invalid lengths, unsupported
coding, missing framing, truncation, excess bytes, header/body/wire limits,
cookies, upgrades, compression, chunk extensions, trailers, bodyless statuses,
informational statuses, obsolete folding, controls, and bare line feeds.

Real ephemeral TLS peers cover fragmented GET, exact POST, redirect without a
second connection, later-record excess bytes, deadline without retry, and indeterminate partial local
submission. Certificates and keys exist only in memory. The accepted summary
includes `public-network=0`, `credentials=0`, and `redirects-followed=0`.

## Deferred production boundary

Independent Linux execution, a serialized Windvale HTTP capability/facade, and
the native capability/timer bridge remain open. Authorization or API-key header
injection belongs to protected credential custody above this client and must not
broaden its public header interface. Provider JSON admission and the supervised
model gateway are the next consumers. HTTP/2, HTTP/3, connection pooling,
cookies, caching, decompression, proxies, automatic redirects, and automatic
retry are not version-1 features.
