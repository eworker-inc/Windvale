# Decision 0594: First network address, endpoint, and authority model

- Date: 2026-08-15
- Status: Implemented candidate with focused Windows execution evidence
- Implements: network slice 2 from the
  [networking foundation plan](../Project/Windvale-Networking-Foundation-Implementation-Plan.md)
- Enables: rights-limited resolver/connect, secure stream, HTTPS, package
  retrieval, and live external-model adapters

## Context

Decision 0587 supplied shared deadlines, cancellation, provider generations,
and indeterminate-mutation outcomes. HTTPS still could not state exactly which
peer a provider may contact. Native socket addresses, resolver results, URI
strings, and host interface indices are unsuitable application authority: they
have different host representations and make it easy for an approved name to
turn into unrestricted numeric-address access.

The current portable compiler/runtime can execute strict bounded byte parsing,
nominal records and enums, checked prefixes, canonical formatting, and immutable
policy reduction. Current Internet standards specify canonical IPv6 display,
URI host distinctions, DNS label bounds, and the host-local nature of IPv6 zone
identifiers. Those rules can be frozen without waiting for native sockets.

## Decision

- Add `Windvaleˉnetworkˉaddressˉauthority` under `Libraries/Network/` as
  the first implemented owner of portable network values.
- Store IPv4 and IPv6 as exact nominal network-order bytes and admit only
  canonical text at this strict application boundary.
- Use RFC 5952 lowercase, leading-zero suppression, and longest/first zero-run
  compression for IPv6 display. Keep dotted-decimal IPv6 tails outside version
  1.
- Require prefix base host bits to be zero rather than silently normalizing.
- Keep IPv6 link-local interface identity as a separate opaque endpoint field.
  Never encode it into address text, a service name, or an HTTPS authority.
- Admit ports `1..65535`; reject unspecified and multicast connect endpoints.
- Admit canonical lowercase ASCII service A-labels with DNS label bounds and
  reject canonical IPv4 literals as ambiguous service names.
- Keep exact service selectors and numeric-prefix selectors distinct. Resolver
  output cannot be used to transform the former into the latter.
- Put transport, direction, port interval, interface, connection, queue,
  transfer, deadline-span, and lifetime bounds in every grant.
- Provide a fail-closed rights-reduction proof: selector identity/containment is
  preserved and every other dimension can only stay equal or narrow.
- Own twelve deterministic conformance groups with a focused native verifier.

## Consequences

Windvale can now express a future live model binding as exact outbound TCP
authority for `api.openai.com`, `api.anthropic.com`, or
`generativelanguage.googleapis.com` on port 443 without exposing a socket,
native address, API key, or ambient DNS authority to portable code.

This is still not a live network or HTTPS claim. No resolver, connect, native
timer, secure entropy, trust snapshot, TLS stream, or HTTP client is added by
this decision. The next infrastructure milestone is the host semantic-provider
slice, preceded or accompanied by deterministic stream contracts; TLS and HTTP
remain shared layers above it.

The strict parser rejects noncanonical but otherwise valid external spellings.
Boundary adapters that must accept general Internet text can parse and
canonicalize under a separate contract before constructing this value. This
keeps policy comparison byte-stable and avoids inheriting host parser quirks.

The current library uses the reconstructed current compiler closure. Its owner
builds deterministic Windows and Linux images, while independent execution on
both hosts remains required for qualification.

## Reconsideration triggers

Revisit this decision when a concrete consumer requires an RFC-defined IPv6
dotted-decimal tail, qualified IDNA mapping, a connectable multicast contract,
per-interface global routing authority, wildcard service policy, Unix-domain or
another non-IP endpoint family, or resource accounting that cannot be expressed
by the current bounded grant. Any rule that permits service authority to become
independent numeric authority requires a new security decision.
