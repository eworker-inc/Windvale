# Windvale networking-foundation implementation plan

## Status

- Date: 2026-08-16
- Status: Active implementation; slices 1, 2, and 5, the Layer 3 contract core,
  and the first supervised resolver/TCP and TLS 1.3 bootstrap providers are implemented candidates under
  [Decision 0587](../Decisions/0587-First-Bounded-Operation-Deadline-And-Cancellation-Core.md),
  [Decision 0594](../Decisions/0594-First-Network-Address-Endpoint-And-Authority-Model.md),
  [Decision 0596](../Decisions/0596-First-Resolve-Connect-And-Reliable-Stream-Core.md),
  [Decision 0598](../Decisions/0598-First-Supervised-Host-Resolver-And-Stream-Provider.md),
  [Decision 0599](../Decisions/0599-First-Supervised-Host-Tls-13-Provider.md),
  [Decision 0603](../Decisions/0603-First-Bounded-Https-Client.md),
  [Decision 0604](../Decisions/0604-First-Protected-Provider-Credential-Custody.md),
  [Decision 0605](../Decisions/0605-First-Supervised-External-Model-Gateway.md),
  and [Decision 0646](../Decisions/0646-First-Native-External-Model-Gateway-Bridge.md).
  Resolver/TCP, TLS, bounded HTTPS, and protected credential custody have
  independent Windows/Linux execution evidence, as does the supervised
  external-model gateway. The first model-only native capability/timer bridge
  executes on Windows and constructs the Linux image; independent Linux evidence
  remains pending, so no complete production network promotion is yet claimed
- Accepted architecture: [network stack](../Architecture/Network-Stack.md)
- Required trust services: [identity, time, entropy, and trust](../Architecture/Identity-Time-Entropy-And-Trust.md)
- First package consumer: [package-system implementation plan](Windvale-Package-System-Implementation-Plan.md)
- Selected release consumers: the official package source and external-model
  gateway in the [Windvale 0.2.0 connected-services release plan](Windvale-0.2.0-Connected-Services-Release-Plan.md)

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

### Network slice 1: operation, deadline, and cancellation core — first candidate implemented

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

Status: implemented candidate under [Decision 0594](../Decisions/0594-First-Network-Address-Endpoint-And-Authority-Model.md).

Implement strict IPv4 and IPv6 text/binary conversion, prefixes, scoped endpoints,
ports, service names, peer rules, and rights reduction. Include canonical display
only where its exact form is specified.

Exit gate: published protocol vectors plus boundary, malformed, noncanonical,
scope, prefix, overflow, and grant-narrowing cases agree on Windows and Linux.

The portable implementation and twelve-group focused owner now cover that
surface, execute on Windows, and construct the exact Linux image. Independent
Linux execution is still required before the complete exit gate is claimed.

### Network Layer 3 contract core: resolve/connect and reliable stream

Status: implemented candidate under
[Decision 0596](../Decisions/0596-First-Resolve-Connect-And-Reliable-Stream-Core.md).

Implement the capability-free provider target for one authorized resolve/connect
operation and one bounded reliable stream. It keeps name resolution and address
selection in one decision, composes common operation deadlines/cancellation,
reports exact local write acceptance and read delivery, preserves half-close,
and terminates on indeterminate writes or stale provider generations.

This does not replace ordered slice 3 packet work and does not claim a host
network capability. It removes semantic invention from slice 4: Windows, Linux,
and later Windvale OS providers now have one exact contract to implement.

Exit gate: thirteen deterministic groups cover authorization, selection,
deadlines, partial I/O, half-close, limits, generation changes, and teardown.
The focused Windows path executes and constructs the Linux image; independent
Linux execution remains required for cross-host qualification.

### Network slice 3: deterministic link and packet core

Implement checksums and bounded Ethernet, ARP, IPv4, ICMPv4, UDP, IPv6, ICMPv6,
and Neighbor Discovery parsers/serializers behind a deterministic copied link.
Every derived length, option, header chain, fragment decision, and queue operation
is checked.

Exit gate: simulated peers cover loss, duplication, reordering, delay, corruption,
MTU change, exhaustion, reset, and provider loss without using the public Internet.

### Network slice 4: host semantic providers

Status: bootstrap mechanism implemented under
[Decision 0598](../Decisions/0598-First-Supervised-Host-Resolver-And-Stream-Provider.md)
and [Decision 0599](../Decisions/0599-First-Supervised-Host-Tls-13-Provider.md);
production bridge and promotion remain pending.

Bind constrained Windows and Linux resolver/connect, stream, datagram, monotonic
timer, secure entropy, civil-time, trust, and secure-stream providers. Begin with
one operation in flight per bound instance if necessary, but keep the versioned
contract able to express bounded concurrency.

Exit gate: both hosts produce the same semantic reports for authorized and denied
peers, partial I/O, cancellation, timeout, close, reset, stale providers, malformed
responses, trust failure, and entropy/time unavailability. Live Internet access is
smoke evidence only.

The first supervised provider now binds one exact service/port and finite
connection, byte, deadline, and lifetime limits. It performs host `getaddrinfo`
and real TCP connect/read/write/half-close through pinned Node in a child with an
empty environment and strict `WVNR/WVNS 1` framing. Twenty-five isolated
loopback cases pass independently on Windows and Linux. The Windvale
capability-table/timer bridge and thin Node-free Winsock/Linux leaves remain
required by this slice's exit gate.

The supervised TLS provider fixes TLS 1.3, exact service identity, ALPN, trust
generation, and trust-snapshot digest above the same bounded stream. Fifteen
isolated cases pass independently on Windows and Linux with only in-memory
ephemeral key material. Typed peer evidence, capability/timer binding, and
native secure-transport leaves remain required before production promotion.

### Network slice 5: shared secure HTTP retrieval

Status: hosted bootstrap candidate implemented under
[Decision 0603](../Decisions/0603-First-Bounded-Https-Client.md); independent
Windows/Linux execution is complete and native binding remains pending.

Implement the portable HTTP client and bounded streaming response body above the
secure-stream provider. Do not expose cookies, credential discovery, proxy
inheritance, ambient redirects, decompression, caching, or automatic mutation
retry in version 1.

Exit gate: an isolated deterministic server covers response fragmentation, header
and body limits, content-length agreement, selected chunking behavior, redirects,
downgrade refusal, truncation, excess bytes, cancellation, and connection loss.

The first client binds one exact service and target set, performs one GET or
POST per TLS connection, owns authority and length headers, and accepts only one
canonical content length or selected chunked framing. Twenty-nine cases cover
strict parsing plus real isolated TLS peers. It follows no redirect, performs no
decompression or retry, and receives credentials only through the separately
owned protected-custody injection path.

### External-model credential custody

Status: hosted bootstrap candidate implemented under
[Decision 0604](../Decisions/0604-First-Protected-Provider-Credential-Custody.md);
independent Windows/Linux execution is complete and the supervised gateway owns
the live lease.

The bounded `WVSC 1` wrapper encrypts one provider credential at rest and
authenticates its provider, generation, exact DNS service, implied HTTPS port
443, and cryptographic profile. A private revocable lease injects the OpenAI,
Anthropic, or Google authorization field only inside bounded HTTPS after an exact
generation check. Sixteen isolated cases use fake keys and no public network.

### Supervised external-model gateway

Status: hosted bootstrap candidate implemented under
[Decision 0605](../Decisions/0605-First-Supervised-External-Model-Gateway.md);
independent Windows/Linux execution is complete and native capability binding
remains pending.

One empty-environment child owns the encrypted startup/unlock exchange, exact
provider and credential generations, three fixed provider mappings, bounded
JSON admission, shared HTTPS, and canonical model responses. Thirty isolated
cases cover the core and child boundary, including differential output against
the reference oracle and real credential-to-HTTPS composition with fake data.
No deterministic case reads a real key or contacts the public Internet.

### Native external-model gateway bridge

Status: implemented candidate under
[Decision 0646](../Decisions/0646-First-Native-External-Model-Gateway-Bridge.md);
Windows execution is complete and independent Linux execution remains pending.

The bridge binds the existing ABI-23 catalog/inference entries to the protected
gateway through a model-only native worker with dedicated pipes. Platform WVA
leaves perform exact bounded reads/writes while the launcher owns readiness,
operation/lifetime timers, diagnostics, and joint teardown. Fourteen isolated
cases assemble and admit the shared host and both leaves, construct both hosted
images, and execute one canonical stale request without public networking.

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

The first fixed boot-composition candidate now requires the selected IPv4/IPv6,
port, direction-right, and shared operation-queue envelope before Probe 40
returns token 97. It does not start a network process, grant a link, or process a
packet; checked provider launch, IPC/resource binding, and the slice-7 device and
transport evidence remain open.

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

The `v0.1.0` offline installer and release subset is complete, and Decisions
0598, 0599, and 0603 through 0605 now supply shared retrieval and the selected
external-model gateway candidate. Preserve the implemented slice 1 and slice 2
candidates, then bind the supervised providers and gateway through the native
capability table and timer
contract without weakening the
separately owned deterministic packet work. Do not add a synchronous
`download(url)` host call or package-specific HTTPS capability. Host networking
does not claim completion of the independent Windvale OS packet, driver, or TCP
workstream.

The OS path already admits network profile 3 as an isolated 96-page,
one-process, one-endpoint resource domain and boot-embeds its deterministic user
image. The privileged machine still has to instantiate and publish that provider;
the guest packet, device-driver, and transport path remains independent work.
