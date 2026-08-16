# Windvale supervised host TLS provider version 1

## Status and purpose

The hosted TLS bootstrap provider selected by
[Decision 0599](../Documents/Decisions/0599-First-Supervised-Host-Tls-13-Provider.md)
turns the exact outbound service binding from the
[host network provider](Host-Network-Provider.md) into an authenticated TLS 1.3
byte stream. It runs in a separately supervised Node child on Windows and
Linux. The shared owner passed independently on Windows and Ubuntu in
verification run 31921483457.

The provider is enough to begin bounded HTTP framing against an isolated HTTPS
peer. It is not yet the final native secure-stream capability, public-network
qualification, production credential store, or external-model gateway.

The mechanism uses Node's documented
[`tls.connect`](https://nodejs.org/docs/latest-v24.x/api/tls.html#tlsconnectoptions-callback)
client and follows the current [TLS 1.3 specification](https://www.rfc-editor.org/rfc/rfc9846)
and [service-identity guidance](https://www.rfc-editor.org/rfc/rfc9525).

## Exact binding

One provider process is immutable for its lifetime and binds:

- one canonical lowercase ASCII DNS service and one TCP port;
- one nonzero provider generation;
- one nonzero trust-snapshot generation and exact SHA-256 snapshot digest;
- one exact ASCII ALPN identifier, initially `http/1.1` for HTTPS;
- TLS version exactly 1.3; and
- the connection, queued-byte, transfer, operation-span, and lifetime limits
  defined by `WVNR/WVNS 1`.

Changing any bound value replaces the provider and its generation. A request
cannot supply another service, port, trust root, protocol version, ALPN value,
proxy, redirect target, client certificate, private key, session, cipher list,
or verification callback.

## Trust snapshot profiles

The first implementation accepts either:

1. the pinned Node runtime's bundled root-certificate list, in exact order; or
2. one explicitly pinned certificate for isolated verification and narrowly
   provisioned peers.

Every certificate is parsed as X.509, the list is limited to 256 certificates
and 1 MiB of DER, and the canonical digest covers the ordered count plus every
length-delimited DER certificate. The supervisor and child independently
recompute and require the same digest. An update creates a new trust generation;
ambient operating-system trust-store mutation is not silently inherited.

Pinned-certificate bytes are public trust material and may be passed to the
child through a canonical base64 launch argument bounded to 16,384 characters.
No private key or bearer credential enters this provider. The default
profile uses the pinned Node distribution's bundled roots without placing the
whole snapshot on the command line.

## Handshake and identity

The provider first applies the exact DNS/port authority and bounded resolver
rules from `Host-Network-Provider`. It connects only to admitted resolver
results while passing the original DNS service as TLS `servername`. The TLS
client fixes both minimum and maximum versions to `TLSv1.3`, supplies only the
bound ALPN value, enables normal certificate and service-name verification, and
requires all of the following before publishing a connection:

- the socket is encrypted and authorized;
- the negotiated protocol is exactly TLS 1.3;
- the negotiated ALPN is the exact bound value; and
- a nonempty peer certificate is present.

The provider supplies no resumable session and exposes no application write
until `secureConnect`; version 1 therefore sends no TLS early data. Certificate,
hostname, protocol, ALPN, plaintext-peer, and handshake failures publish the
fixed non-sensitive unavailable result. A later production promotion must add
separate typed trust and protocol failure evidence if a consumer requires that
distinction.

## Stream, supervision, and closure

After authentication, `WVNR/WVNS 1` connect, write, read, write-half shutdown,
and close records carry the encrypted ordered stream. Their generation,
deadline, queue, transfer, half-close, indeterminate-write, and provider-loss
rules are unchanged. The protocol is reused as a supervised mechanism; the
launcher must bind a TLS provider capability separately from a raw TCP provider
and cannot treat one as the other.

The child inherits an empty environment and owns no URL, credential, arbitrary
header, filesystem path, listener, datagram, or raw-packet authority. Malformed
framing, duplicate active request identities, excessive output, provider exit,
and missed response deadlines trigger bounded teardown.

## Executable evidence

`Test-Host-Tls-Provider` owns 15 isolated cases. It generates an ephemeral CA,
server certificate, and private key only in memory, then covers canonical trust
digests, invalid binding, authority-before-resolution, authenticated TLS 1.3
read/write and half-close, wrong identity, wrong trust, TLS 1.2, ALPN mismatch,
plaintext peers, stalled handshake deadlines, stale generations, transfer
limits, provider teardown, and a complete supervised child round-trip.

The accepted summary includes `public-network=0`, `credentials=0`, and
`tls=1.3`. No test key or certificate is stored in the repository.

## Deferred production boundary

Independent Linux execution, the Windvale capability-table and monotonic-timer
bridge, explicit secure-stream peer-evidence records, and eventual Node-free
platform leaves remain open. Bounded HTTP/1.1 is implemented separately under
[Decision 0603](../Documents/Decisions/0603-First-Bounded-Https-Client.md). Public
model access additionally requires protected credential custody, provider JSON
admission, supervision, quotas, and opt-in live smoke evidence.
