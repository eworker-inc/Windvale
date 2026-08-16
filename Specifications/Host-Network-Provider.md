# Windvale supervised host network provider version 1

## Status and purpose

The hosted bootstrap provider selected by
[Decision 0598](../Documents/Decisions/0598-First-Supervised-Host-Resolver-And-Stream-Provider.md)
performs real operating-system name resolution and TCP stream I/O on Windows
and Linux behind a bounded child-process protocol. It is the first executable
mechanism targeting the portable
[resolve/connect and reliable-stream contract](Network-Connect-Stream-Core.md).

The provider uses the pinned Node host runtime. Official Node documentation
states that [`dns.lookup`](https://nodejs.org/docs/latest-v24.x/api/dns.html#dnslookuphostname-options-callback)
uses operating-system name-resolution facilities and `getaddrinfo`, while
[`node:net`](https://nodejs.org/docs/latest-v24.x/api/net.html) supplies TCP
client sockets, write completion, paused reads, errors, and half-close. These
mechanisms are host implementation details, not Windvale semantics.

The same owner and source passed independently on Windows and Ubuntu in
verification run 31920690046. The provider is a bootstrap mechanism pending a
provider-table bridge, native timer
binding, and Node-free Winsock/Linux leaves; it is not yet a production network
slice completion claim.

## Binding and authority

One child process is launched with exactly:

- one canonical lowercase ASCII service name under the version-1 network-name
  rules;
- one outbound TCP port;
- one nonzero provider generation;
- maximum live plus resolving/connecting operations from 1 through 64;
- maximum queued bytes from 1 through 65,536;
- a nonzero aggregate transfer limit per connection;
- maximum operation span from 1 through 300,000 milliseconds; and
- maximum provider lifetime from 1 through 86,400,000 milliseconds.

The supervisor supplies an empty environment. The child receives framed binary
requests on standard input and publishes framed responses on standard output.
It has no request-controlled base URL, port, proxy, redirect, listener,
credential, filesystem path, arbitrary header, datagram, or raw-packet access.
Changing the service, port, limits, or process replaces the provider binding and
requires a new generation.

## Request record: `WVNR 1`

All integers are unsigned little-endian. The fixed 72-byte header is:

| Offset | Field |
| ---: | --- |
| 0 | magic `WVNR` |
| 4 | version `1` |
| 8 | exact total bytes, at most 131,072 |
| 12 | operation |
| 16 | nonzero caller request identity `u64` |
| 24 | expected provider generation `u64` |
| 32 | connection identity `u64` |
| 40 | connection generation `u64` |
| 48 | absolute host-monotonic deadline in nanoseconds `u64` |
| 56 | port or maximum read bytes `u32` |
| 60 | service-name byte length `u32` |
| 64 | payload byte length `u32` |
| 68 | reserved zero |

Service and payload bytes follow. Operations are `Connect=1`, `Write=2`,
`Read=3`, `Shutdown_write=4`, and `Close=5`.

A connect has zero connection identities, the exact bound service and port, and
no payload. A write has exact nonzero connection identities, 1 through 65,536
payload bytes, and zero control. A read has exact connection identities, a
maximum from 1 through 65,536, and no payload. Shutdown and close carry only
the identities and deadline.

## Response record: `WVNS 1`

The fixed 80-byte header is:

| Offset | Field |
| ---: | --- |
| 0 | magic `WVNS` |
| 4 | version `1` |
| 8 | exact total bytes, at most 131,072 |
| 12 | status |
| 16 | echoed caller request identity `u64` |
| 24 | current provider generation `u64` |
| 32 | connection identity `u64` |
| 40 | connection generation `u64` |
| 48 | exact write acceptance or read delivery `u64` |
| 56 | selected endpoint port `u32` |
| 60 | endpoint family: absent `0`, IPv4 `4`, IPv6 `6` |
| 64 | flags: peer closed bit `1`, local write closed bit `2` |
| 68 | address-text byte length `u32` |
| 72 | payload byte length `u32` |
| 76 | diagnostic byte length `u32` |

Address, payload, and diagnostic bytes follow. Status values are `Valid=0`,
`Invalid_request=1`, `Unauthorized=2`, `Stale=3`, `Expired=4`,
`Unavailable=5`, `Limit=6`, `Reset=7`, `Peer_closed=8`,
`Submission_indeterminate=9`, `Provider_lost=10`, and `Cancelled=11`.

A valid response has no diagnostic. A failure has no endpoint, progress, or
payload and uses one fixed non-sensitive diagnostic. Host error numbers,
resolver text, paths, internal addresses not selected for the connection, and
raw exception messages never cross the boundary.

## Resolution, connection, and stream rules

The provider checks authority and deadline before invoking resolution. It calls
the host resolver once for the exact admitted name, accepts at most 32 unique
canonical IPv4/IPv6 results, and attempts only those addresses on the already
bound port. Attempts are staggered by 100 milliseconds in resolver order; the
first success wins and every loser is destroyed. Selected endpoint evidence is
returned only with the connection and is never a reusable numeric grant.

At most one read and one write are active for a connection. The complete
unsettled reservations may not exceed the queued-byte bound, and exact delivered
plus accepted bytes may not exceed the transfer bound. The Node write callback
is interpreted as complete local-provider acceptance into the underlying
system, not remote receipt or application commit. Error or deadline after write
dispatch produces `Submission_indeterminate`, destroys the socket, and permits
no retry.

Reads use paused/manual stream consumption and publish no more than the caller's
maximum. End-of-stream preserves the local write half because clients are
created with `allowHalfOpen: true`. Explicit shutdown sends the write FIN while
retaining reads. Reset, close, provider teardown, stale generation, deadline,
and peer FIN remain different outcomes.

Node's host `getaddrinfo` work is not cancellable through this interface. A
timed-out lookup therefore remains charged against the connection limit until
the underlying promise settles. New connects are rejected while that debt
would exceed the binding. Provider-process teardown is the final containment
boundary.

## Supervision and verification

`Host-Network-Supervisor.mjs` starts the provider with no inherited environment,
correlates out-of-order responses by request identity, bounds frame buffering,
rejects unknown/duplicate responses, and terminates the child after a missed
response deadline. Malformed framing terminates the child rather than attempting
resynchronization.

`Test-Host-Network-Provider` owns 25 cases. It covers canonical and malformed
records, authority, stale generations, deadline span and expiry, resolver
failure and retained debt, invalid resolved addresses, real loopback connect,
endpoint evidence, exact writes and reads, connection and transfer limits,
read timeout, concurrent-read rejection, half-close, peer close, teardown,
supervised process round-trip, and malformed-child containment. It binds only
an ephemeral loopback listener; `public-network=0` and `credentials=0` are part
of its accepted summary.

## Deferred production boundary

The next bridge must map these records and provider generations into the
Windvale native capability table and common operation state without exposing
stdio or Node objects. A native timer supplies the absolute monotonic deadline.
Independent Linux execution and thin Node-free Winsock/Linux leaves must then
preserve the same reports. TLS, trust snapshots, secure entropy, civil-time
policy, bounded HTTP, credentials, package retrieval, and external-model JSON
remain separate higher layers.
