# Network Address and Authority

## Status

Implemented candidate under
[Decision 0594](../Documents/Decisions/0594-First-Network-Address-Endpoint-And-Authority-Model.md).

## Purpose and boundary

`Windvaleˉnetworkˉaddressˉauthority` is the capability-free value and
policy core for network slice 2. It owns canonical IPv4 and IPv6 values,
prefixes, connectable endpoints, ASCII service names, peer selectors, bounded
grants, and provable rights reduction.

It performs no DNS lookup, connection, listening, I/O, interface discovery,
URI parsing, TLS, HTTP, credential access, or native-handle conversion. A later
resolver/connect provider must admit a service name and its resolved addresses
as one decision; this library deliberately provides no conversion from service
authority to numeric-address authority.

## Address values and text

`Networkˉaddress` contains a nominal kind and exactly 4 or 16 network-order
bytes. Public construction rejects every other kind/length combination.

The text parser accepts only canonical input:

- IPv4 has exactly four decimal octets, no sign or whitespace, values `0..255`,
  and no leading zero in a multi-digit octet.
- IPv6 uses lowercase hexadecimal, suppresses leading zeroes, compresses the
  longest run of at least two zero groups, and uses the first such run on a tie.
- IPv6 dotted-decimal tails are outside version 1. Callers can construct the
  equivalent 16-byte address explicitly and receive canonical hexadecimal
  display.
- Interface zone identifiers are not address text. They are host-local endpoint
  state and are never emitted as a service name or URI authority.

IPv6 parsing first admits the hexadecimal structure, reconstructs exactly 16
bytes, renders the RFC 5952 canonical form, and requires byte-for-byte equality
with the input. `Networkˉaddressˉdisplay` is therefore deterministic on every
host.

## Prefixes and scopes

`Networkˉprefixˉcreate` accepts lengths `0..32` for IPv4 and `0..128` for
IPv6. Every host bit in the base address must already be zero; the constructor
does not silently normalize an overbroad value. Containment compares the exact
prefix bits and requires the same address family.

The implemented scope classifier distinguishes global, unspecified, loopback,
link-local, and multicast values. It recognizes IPv4 `0/8`, `127/8`,
`169.254/16`, and `224/4`, plus IPv6 unspecified, loopback, `fe80::/10`, and
`ff00::/8`.

`Networkˉendpointˉcreate` accepts ports `1..65535`. Unspecified and multicast
addresses are not connectable endpoints. An IPv6 link-local endpoint requires a
nonzero opaque interface identity; other endpoints require interface zero in
version 1. The identity has meaning only within its bound host/provider
generation. It is not a portable interface number and must not be transmitted.

## Service names

Version 1 service names are canonical lowercase ASCII DNS A-label form:

- total length is `1..253` bytes;
- each nonempty label is at most 63 bytes;
- labels contain only `a..z`, `0..9`, and interior `-`;
- a trailing root dot, uppercase input, underscores, whitespace, and non-ASCII
  U-labels are rejected; and
- a canonical dotted-decimal IPv4 literal is rejected as ambiguous.

Applications that begin with Unicode names must use a separately qualified IDNA
mapper and pass its canonical A-label result. This library does not inherit host
resolver search suffixes or comparison behavior.

## Grants and rights reduction

A grant has exactly one selector kind:

- an exact service name; or
- one IPv4/IPv6 prefix and an optional opaque interface restriction.

It also fixes a transport (`Tcp`, `Udp`, or `Any`), direction (`Outbound`,
`Listen`, or `Any`), inclusive nonzero port interval, maximum connections,
maximum queued bytes, maximum transfer bytes, maximum deadline span, and a
nonzero absolute monotonic expiry tick. Every resource limit must be nonzero.

`Networkˉgrantˉnarrows(Child, Parent)` succeeds only when:

- selector kinds remain the same;
- service names are identical, or the child prefix is contained by and at least
  as long as the parent prefix;
- transport and direction stay equal or specialize parent `Any`;
- the child's port interval is contained in the parent's;
- a nonzero parent interface remains identical;
- every child resource bound is no greater; and
- child expiry is no later.

A service grant can never narrow into a prefix grant. DNS output therefore does
not become ambient address authority. Service and endpoint match operations also
require a concrete transport/direction and `Now < Expiresˉat`.

The stored resource bounds are policy input for a later provider. The pure match
functions do not maintain consumption counters. Binding a grant must pair it
with provider-generation state and exact accounting before a live operation is
accepted.

## Executable evidence

`Address-Authority-Self-Test.wv` covers twelve groups: canonical IPv4,
malformed/overflow IPv4, RFC-style IPv6 compression, noncanonical IPv6,
prefix containment and host-bit rejection, scoped endpoints, port boundaries,
service names for the first three model providers, service matching and expiry,
service-grant reduction, prefix-grant reduction/matching, and strict separation
of service and numeric selectors.

The focused native owner builds library and test WVB twice, requires identical
bytes, lowers twice to identical WVO, executes the current-host result, and
constructs the opposite-host application from the same linked bytes. Independent
Linux execution remains required before a cross-host semantic qualification
claim.

## HTTPS consequence

This contract still does not implement HTTPS. It gives the later secure-connect
provider an exact, auditable statement such as “outbound TCP to
`api.openai.com` on port 443 with these limits and this expiry.” The remaining
path is host resolver/connect and timer providers, secure entropy and trust,
TLS 1.3 peer authentication, then bounded HTTP framing and model adapters.

## Standards references

- [RFC 3986: URI generic syntax](https://www.rfc-editor.org/rfc/rfc3986)
- [RFC 4291: IPv6 addressing architecture](https://www.rfc-editor.org/rfc/rfc4291)
- [RFC 5952: canonical IPv6 text representation](https://www.rfc-editor.org/rfc/rfc5952)
- [RFC 1123: Internet host requirements](https://www.rfc-editor.org/rfc/rfc1123)
- [RFC 9844: IPv6 zone identifiers are host-local](https://www.rfc-editor.org/rfc/rfc9844)
