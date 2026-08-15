# Windvale networking-foundation implementation plan

## Status

- Date: 2026-08-15
- Status: Active implementation; slice 1 is implemented under
  [Decision 0587](../Decisions/0587-First-Bounded-Operation-Deadline-And-Cancellation-Core.md),
  and no network capability is yet claimed
- Accepted architecture: [network stack](../Architecture/Network-Stack.md)
- Required trust services: [identity, time, entropy, and trust](../Architecture/Identity-Time-Entropy-And-Trust.md)
- First package consumer: [package-system implementation plan](Windvale-Package-System-Implementation-Plan.md)

This plan makes networking a permanent Windvale platform facility rather than a
private downloader hidden inside `wv`. The offline package system does not wait
for it. The online source consumes it only after signed offline release admission
and the reusable secure retrieval path are qualified.

## Meaning of “implement networking once”

Windvale freezes one semantic application boundary and one set of portable
protocol libraries. It does not pretend that Windows sockets, Linux sockets, and
Windvale OS devices are the same mechanism.

The permanent shape is:

1. portable value, operation, error, limit, and capability contracts;
2. deterministic capability-free parsers, serializers, and state machines;
3. rights-limited providers for Windows and Linux using their native network and
   trust facilities behind the Windvale contracts;
4. a Windvale OS user-space packet and transport service behind the same contracts;
5. shared secure-stream consumers, HTTP policy, package retrieval, remote
   terminal, and later application libraries above those providers.

Host adapters may differ internally. Applications never observe a native socket,
descriptor, handle, `sockaddr`, resolver structure, certificate-store handle, or
platform error number. Replacing a host provider with the Windvale OS stack must
not change the application contract.

“Once” therefore means one public model and one reusable consumer stack, not one
unsafe lowest-common-denominator socket wrapper. It also does not mean writing
new cryptography: secure transport uses qualified standard algorithms and
independent vectors or a constrained platform provider until the portable
implementation is qualified.

## What the current stack can support

The merged compiler and runtime are sufficient now for:

- bounded nominal address, endpoint, policy, operation, and result records;
- strict byte parsers and serializers with checked arithmetic;
- deterministic DNS, IP, UDP, TCP, TLS-record, and HTTP framing fixtures;
- state machines driven by explicit input events, virtual monotonic time, and
  deterministic test entropy;
- bounded queues encoded as immutable bytes or fixed record directories; and
- focused provider-table experiments using the existing explicit native provider
  call and generation model.

The current product runtime is not sufficient for a real network client without
new work. It now has the capability-free operation/wait/cancellation semantics,
but lacks a native wait and monotonic timer provider,
secure entropy provider, resolver/connect provider, secure-stream provider,
streaming HTTP body service, host adapters, civil-time/trust policy, and dynamic
launcher binding. Those are implementation slices below, not reasons to redesign
the compiler or wait for the unrelated broad verification backlog.

## Permanent contract layers

### Layer 1: common asynchronous operation model

Define this before any network call:

- opaque provider and operation identities with generations;
- one terminal completion for every accepted operation;
- explicit immediate rejection, queued acceptance, partial progress, completion,
  cancellation, timeout, provider loss, stale generation, and indeterminate
  mutation outcomes;
- monotonic deadlines rather than elapsed wall-clock guesses;
- bounded wait sets or event batches with no reentrant application callback;
- bounded queue and byte accounting plus reserved control capacity for close and
  cancellation; and
- deterministic teardown that wakes every waiter and invalidates stale handles.

This layer is shared by files, processes, terminals, networking, devices, and
future package-store operations. Networking must not invent a private event loop.

### Layer 2: public network values and grants

Freeze IP-version-neutral nominal IPv4/IPv6 addresses, prefixes, transport ports,
scoped IPv6 endpoints, service names, peer policies, connection limits, and
failure classes. Separate grants cover resolution/connect, datagram, listen,
secure connect, raw link access, capture, and administration.

Name resolution and connection authorization remain one provider decision. An
approved name cannot be converted into ambient numeric-address authority. A
grant can restrict names, address ranges, ports, direction, interface, connection
count, queued bytes, transfer budget, deadlines, and lifetime.

### Layer 3: byte stream, datagram, and secure stream

A reliable stream reports exact bytes accepted by the local provider, orderly
peer close, reset, timeout, cancellation, provider loss, and generation change.
It never reports remote application commit. A datagram reports local acceptance,
not delivery. Listening is absent unless an exact listener grant is bound.

Secure stream is a separate contract over peer identity, trust generation,
protocol policy, entropy, and civil-time or pinned-key policy. Plain transport
authority never implies secure peer authentication, and package code never
receives the underlying raw connection automatically.

### Layer 4: shared retrieval protocols

Implement URI authority parsing, HTTP request construction, response framing,
redirect policy, bounded headers, content length, chunking where selected,
streaming bodies, cancellation, and exact excess/truncation rejection once above
the secure-stream contract. Package retrieval additionally pins expected length
and digest from already authenticated release metadata.

The HTTP library is useful to package management and ordinary applications. It
does not select a package release, trust redirects, retry uncertain mutations, or
grant broader network authority.

### Layer 5: provider families

Windows and Linux initially provide constrained resolve/connect, secure stream,
monotonic time, civil time, secure entropy, and trust snapshots through native
facilities. Their adapters normalize behavior into the contracts and retain all
native state privately.

Windvale OS later provides `LinkPort 1`, routing, UDP, TCP, resolver, and secure
transport from isolated services. The kernel retains interrupt, timer, memory,
DMA/IOMMU, accounting, capability, and teardown mechanisms but contains no DNS,
TCP, TLS, HTTP, or package protocol parser.

## Ordered implementation slices

### Network slice 1: operation, deadline, and cancellation core

Status: implemented candidate under [Decision 0587](../Decisions/0587-First-Bounded-Operation-Deadline-And-Cancellation-Core.md).

Specify the common operation identity, generation, completion, monotonic deadline,
cancellation, wait-batch, and teardown records. Implement a capability-free model
with a virtual clock and bounded event queue.

Exit gate: deterministic tests cover immediate completion, queued completion,
partial progress, cancellation races, deadline races, stale generations, provider
restart, queue exhaustion, reserved close capacity, and complete teardown.

The focused native owner now covers all ten groups, including immediate
rejection and persistent closed-wait evidence. Native timer and blocking-wait
providers remain slice 4 work; this slice is the capability-free semantic core.

### Network slice 2: address, endpoint, and authority model

Implement strict IPv4 and IPv6 text/binary conversion, prefixes, scoped endpoints,
ports, service names, peer rules, and rights reduction. Include canonical display
only where its exact form is specified.

Exit gate: published protocol vectors plus boundary, malformed, noncanonical,
scope, prefix, overflow, and grant-narrowing cases agree on Windows and Linux.

### Network slice 3: deterministic link and packet core

Implement checksums and bounded Ethernet, ARP, IPv4, ICMPv4, UDP, IPv6, ICMPv6,
and Neighbor Discovery parsers/serializers behind a deterministic copied link.
Every derived length, option, header chain, fragment decision, and queue operation
is checked.

Exit gate: simulated peers cover loss, duplication, reordering, delay, corruption,
MTU change, exhaustion, reset, and provider loss without using the public Internet.

### Network slice 4: host semantic providers

Bind constrained Windows and Linux resolver/connect, stream, datagram, monotonic
timer, secure entropy, civil-time, trust, and secure-stream providers. Begin with
one operation in flight per bound instance if necessary, but keep the versioned
contract able to express bounded concurrency.

Exit gate: both hosts produce the same semantic reports for authorized and denied
peers, partial I/O, cancellation, timeout, close, reset, stale providers, malformed
responses, trust failure, and entropy/time unavailability. Live Internet access is
smoke evidence only.

### Network slice 5: shared secure HTTP retrieval

Implement the portable HTTP client and bounded streaming response body above the
secure-stream provider. Do not expose cookies, credential discovery, proxy
inheritance, ambient redirects, decompression, caching, or automatic mutation
retry in version 1.

Exit gate: an isolated deterministic server covers response fragmentation, header
and body limits, content-length agreement, selected chunking behavior, redirects,
downgrade refusal, truncation, excess bytes, cancellation, and connection loss.

### Network slice 6: package-source integration

Bind `wv` to a grant limited to the official discovery names and declared object
locations. Authenticate signed Root, Channel, and Release metadata before object
selection; stream exactly the declared object length and digest into private
package-store publication.

Exit gate: online and offline installation yield identical admitted objects and
generation records, and no network response can select authority or identity by
itself.

### Network slice 7: Windvale OS link and transport service

After the required kernel and driver mechanisms exist, bind the same semantic
contracts to simulated links, loopback, `virtio-net`, IP routing, UDP, TCP, DNS,
and secure transport in isolated services. Add IPv6 before claiming a general
host profile.

Exit gate: the pinned QEMU device and deterministic peer prove interrupt-driven
completion, bounded pools, reset, DMA revocation, service restart, every stale
generation, and zero retained resources after teardown.

## Verification policy

Each slice has one focused native owner and deterministic fixtures. Development
may proceed while an unrelated broad repository verifier is red. Integration runs
the changed-file planner and named adjacent owners. Only an exact-source,
independent Windows/Linux Qualification result can promote an application-network,
secure-network, installer-network, or Windvale OS network claim.

Public-network success is never the oracle. It is inherently variable and cannot
replace isolated peers, virtual time, deterministic entropy, malformed inputs,
failure injection, structural traces, exact wire bytes, or independent protocol
and cryptographic vectors.

## Immediate recommendation

The `v0.1.0` offline installer and release subset is complete. Do not make
networking the next milestone automatically. If a selected product requires
online retrieval, start network slice 1 and then slice 2 as shared infrastructure,
not as a competing package-client implementation. Do not add a synchronous
`download(url)` host call or package-specific HTTPS capability. The first real
online package request should arrive only after the common operation model,
secure-stream boundary, and shared HTTP framing exist.
