# Decision 0598: First supervised host resolver and stream provider

- Date: 2026-08-15
- Status: Implemented bootstrap candidate with isolated Windows execution evidence
- Advances: network slice 4 in the
  [networking-foundation plan](../Project/Windvale-Networking-Foundation-Implementation-Plan.md)
- Contract: [host network provider](../../Specifications/Host-Network-Provider.md)
- Builds on: [Decision 0596](0596-First-Resolve-Connect-And-Reliable-Stream-Core.md)

## Context

Windvale has exact portable network authority, operation, resolve/connect, and
reliable-stream state machines, but its native executable container does not
yet bind Windows Winsock, Linux host resolution, or a native timer provider.
The build runtime does contain pinned Node on both required hosts. Node's
`dns.lookup` delegates to host `getaddrinfo`, and its TCP sockets expose the
local write, read, half-close, error, and teardown mechanisms needed to exercise
the accepted semantics against real host networking.

Putting DNS, sockets, or URLs directly into the external-model tool would
duplicate the networking boundary. Extending the current executable container
with ambient socket imports before there is a strict request/result and
supervision contract would expose mechanism without an authority boundary.

## Decision

- Add one separately supervised hosted network provider under
  `Runtime/Hosted/Network/` as an explicit bootstrap dependency.
- Bind each provider process at launch to one exact canonical service name,
  one TCP port, one nonzero provider generation, and finite connection, queued
  byte, transfer, operation-span, and lifetime limits.
- Give the child an empty environment and only framed standard input/output. It
  receives no credential, path, URL, proxy setting, arbitrary header, listener
  grant, datagram grant, or numeric-address grant.
- Use canonical `WVNR 1` requests and `WVNS 1` responses for connect, write,
  read, write-half shutdown, and close. Requests repeat provider and connection
  generations and an absolute host-monotonic deadline.
- Resolve the exact admitted name with the operating-system resolver, admit at
  most 32 canonical IPv4/IPv6 results, and connect only to those returned
  addresses on the bound port. The provider performs bounded staggered racing;
  it does not publish resolution output as reusable numeric authority.
- Permit at most one read and one write per connection. Report a successful
  write only after the complete bounded buffer has left Node's user-space write
  queue for the underlying system. A failure after dispatch is
  `Submission_indeterminate` and destroys the usable connection.
- Preserve TCP half-close with `allowHalfOpen` and explicit shutdown. Read
  delivery, peer close, reset, provider loss, stale generations, deadlines, and
  limits remain distinct.
- Treat an OS resolver request that outlives its caller deadline as retained
  provider debt until the underlying `getaddrinfo` work finishes. Reject new
  connects rather than accumulating unbounded uncancellable resolver work.
- Keep verification on isolated loopback peers. Public-network access and
  credentials are absent from the owner.

## Consequences

Windows and Linux now have one executable hosted resolver/TCP mechanism and a
supervisor protocol that can be connected to the Windvale provider table. The
current source runs on both host Node runtimes; the accepted local evidence is
Windows only until the Linux owner executes independently.

This is not completion of production network slice 4. Node is a temporary
bootstrap host adapter, not the semantic definition or final native runtime
leaf. The next work must bind the protocol through the Windvale capability
table and timer contract, obtain independent Linux execution, and replace or
qualify the Node mechanism with thin Winsock and Linux native leaves in the
hosted executable container. TLS, trust, entropy, HTTP, credential custody, and
the model gateway remain above that boundary.

The provider does not claim complete RFC 8305 Happy Eyeballs v2 behavior. Its
bounded 100 ms stagger follows the host resolver's admitted order and is a
finite bootstrap selection policy. A later production selection profile must
freeze address-family interleaving, attempt delays, history, cancellation, and
diagnostic evidence before making the broader claim.

## Reconsideration triggers

Revisit this decision when native Winsock/Linux leaves are admitted, when a
cancellable resolver replaces `getaddrinfo` debt containment, when measured
consumers need more than one simultaneous read or write, when selection policy
requires full Happy Eyeballs v2, or when the supervised service manager owns
process restart and provider-generation publication.
