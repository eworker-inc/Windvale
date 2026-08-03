# Decision 0192: Capability-oriented user-space network stack

- Date: 2026-08-03
- Status: Accepted future architecture; no NIC, packet, IP, resolver, TCP, secure-transport, or application-network mechanism is implemented
- Refines: [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md), [Decision 0140](0140-Per-Module-Platform-Scope-And-Filesystem-Capabilities.md), [Decision 0173](0173-Windvale-Process-Service-And-Driver-Architecture.md), and [Decision 0183](0183-Product-Packaging-Trust-And-Evolution.md)
- Coordinates with: [Decision 0171](0171-Future-Virtualization-And-Accelerator-Architecture.md) for VM network attachments, [Decision 0191](0191-Windvale-Console-Shell-And-Cli-Architecture.md) for terminal sessions, and [Decision 0193](0193-Simple-Windvale-Remote-Terminal-Protocol.md) for the first remote-session carrier
- Architecture: [Network stack](../Architecture/Network-Stack.md)

## Context

Windvale OS deliberately has no current network device or stack. Its accepted kernel direction already keeps networking policy outside the kernel, its process architecture separates endpoint control planes from measured shared-memory data planes, and its capability model separates name resolution, connection, listening, packet access, and network configuration. The missing decision is how those principles become a usable, performant host network without importing POSIX sockets as Windvale semantics or making every packet cross many isolated protocol processes.

Networking also intersects future remote terminals and VM hosting. A remote terminal should not define an early private transport before a secure network foundation exists. A future VM must not gain host-network authority merely because it has a virtual NIC. These consumers need one common device, packet, protocol, capability, and lifecycle boundary.

## Decision

### Use one user-space network service and an isolated NIC driver

The kernel owns interrupt delivery, scheduling, monotonic timer primitives, endpoints, shared memory, page and DMA ownership, IOMMU enforcement, resource accounting, revocation, and teardown. It parses no packet or application network protocol.

An isolated NIC or virtual-link driver owns device initialization, exact registers and interrupts, bounded RX/TX queues, approved DMA buffers, link state, reset, and completion reporting. It receives no routing, name-resolution, application-policy, certificate, filesystem, package, or terminal authority.

One network service initially owns Ethernet, ARP, IPv4, ICMPv4, IPv6, ICMPv6, Neighbor Discovery, routing, UDP, TCP, fragments, timers, and connection state. Keeping the tightly coupled protocol path in one process avoids per-layer IPC and copying. Resolver, address configuration, secure transport, filtering, or virtual-network policy may later become separate providers when authority, key custody, restart, update, or measured containment justifies the split.

### Implement standards, not a Windvale wire-level Internet

Windvale implements established Ethernet and Internet protocols. The public model is dual-stack from its first contract even if the first deterministic device proof uses static IPv4. IPv6 link-local addressing, ICMPv6, Neighbor Discovery, Duplicate Address Detection, and SLAAC precede an accepted general host profile. TCP follows the current consolidated base specification and includes bounded retransmission and congestion-control behavior. UDP exposes datagram semantics without inventing reliability or delivery claims.

The initial stack is a host, not a router. Forwarding, bridging, NAT, multicast services, VPNs, tunnels, dynamic routing, and service discovery require later explicit capabilities and evidence. A first experimental profile may reject fragmentation explicitly; a general host profile requires bounded reassembly and cleanup.

Windvale writes and owns the stack. A mature external implementation may act as a test oracle or an explicitly temporary bootstrap, but it is not the semantic definition or an undeclared permanent dependency. Cryptographic algorithms and secure transports use qualified standard implementations and test vectors rather than newly invented cryptography.

### Expose semantic network capabilities rather than ambient sockets

Applications use typed capabilities for name resolution, outbound stream connection, datagram channels, listening and acceptance, secure authenticated connection, and separately privileged administration or raw packet access. Exact source names and encodings remain open until measured consumers exist.

A declaration is not authority. Admission approves exact transitive requirements, and launch binds independently rights-reduced instances. Grants may constrain peer names, address prefixes, ports, transport, interface, direction, multicast or broadcast, connection count, bandwidth, packet rate, queued bytes, deadlines, and lifetime. Outbound connection does not grant listening. Ordinary networking does not grant raw packets, capture, interface configuration, routing, forwarding, promiscuous mode, or a VM attachment.

Name authorization and connection selection are bound together so an approved name cannot become an unrestricted numeric-address capability through rebinding or a time-of-check/time-of-use gap. Provider restart, link change, address expiry, route loss, peer closure, cancellation, and stale generation remain explicit outcomes.

Stream write completion means exact local-provider acceptance, not remote receipt or application commit. Datagram send completion means local acceptance only; delivery is unknown without a higher-level acknowledgement. Partial progress and uncertain application-level mutation remain visible, and reconnection never silently retries an operation.

Windows and Linux may bind these semantic contracts to native sockets or supervised host providers. Windvale OS binds them to protected services. A later POSIX compatibility library remains an adapter and does not make native handles, ambient descriptors, process-global resolver state, or host option numbers the shared definition.

### Begin with copied bounded packets and preserve an optimized path

The first data path uses fixed pools and bounded copies across trust boundaries. Every device completion, descriptor, header, length, offset, option, fragment, checksum, DNS record, and derived allocation is validated as untrusted input with checked arithmetic and bounded iteration.

A later measured data path may use versioned shared-memory rings with buffer identity, generation, offset, length, ownership, and completion evidence. It exposes no raw physical pointer. Queue and pool exhaustion have exact backpressure or drop behavior, and teardown reclaims every buffer before a device or provider generation is reused.

The network service is event-driven, batches work, uses bounded connection and timer state, and does not create one thread per connection. Multiqueue, RSS, checksum or segmentation offload, larger MTUs, zero-copy, and service sharding wait for a correct unaccelerated baseline plus evidence that each optimization preserves validation, ownership, and failure semantics.

### Use modern virtio-net for the first device proof

The first QEMU device is modern `virtio-net` with one RX queue, one TX queue, a standard Ethernet MTU, and the smallest required feature set. Multiqueue, RSS, jumbo frames, checksum and segmentation offloads, and optional control features are disabled initially. Bounded polling may prove first packets, but interrupt-driven completion, reset, link loss, DMA revocation, and full buffer reclamation are required before the driver is usable.

The network service consumes one versioned link-port capability. Loopback and deterministic simulated links arrive before hardware; later providers may include physical NICs, Hyper-V synthetic adapters, guest virtual NICs, and virtual switch ports without changing the protocol or application contract.

VM network access is a separate attachment capability. A guest remains disconnected until an exact virtual link, bridge, route, NAT, filter, or physical device is authorized. A future user-space network-fabric service owns that policy; the kernel continues to enforce memory, DMA, interrupt, capability, accounting, and teardown mechanisms only.

### Stage remote access after secure networking

Local serial and graphical terminals do not wait for networking. Authenticated remote sessions wait for the network service, semantic application capabilities, secure entropy, trust and peer identity, key protection, civil-time policy where required, TLS 1.3, authorization, audit, revocation, and session-replacement behavior. The terminal adapter then binds to the existing terminal/session capability model; neither networking nor authentication is embedded in the shell.

### Qualify deterministic slices

Implement in this order:

1. timer/preemption, independently lived memory, resource domains, dynamic launch, service supervision, PCI discovery, interrupts, shared memory, DMA/IOMMU ownership, and teardown;
2. pure packet parsing and serialization, virtual time, loopback, and a deterministic simulated link on Windows and Linux;
3. one isolated single-queue modern `virtio-net` driver;
4. Ethernet, ARP, static IPv4, ICMPv4, and UDP with an isolated deterministic peer;
5. dual-stack routing, IPv6 link-local addressing, ICMPv6, Neighbor Discovery, Duplicate Address Detection, and SLAAC;
6. configuration, DHCPv4, DNS, address and route lifecycle;
7. bounded TCP with retransmission, congestion control, close, reset, and teardown;
8. semantic resolve, connect, datagram, listen, and accept capabilities;
9. qualified TLS 1.3 secure connections; and
10. remote terminal transport, package and browser consumers, VM networks, physical NICs, QUIC, and performance features one measured need at a time.

Qualification uses deterministic isolated peers rather than the public Internet. It injects malformed and oversized packets, loss, duplication, reordering, delay, corruption, fragmentation pressure, MTU and route changes, link removal, interrupt storms, queue exhaustion, service exit, driver fault, reset, and provider replacement. Tests record virtual time and random seeds, use exact bytes where wire encoding is the contract, and may compare against an independent mature stack.

## Consequences

- The kernel remains protocol-blind and has a smaller remotely reachable attack surface.
- NIC-driver compromise does not automatically acquire application or routing policy; network-service compromise does not automatically acquire MMIO or unrestricted DMA.
- One service keeps the first protocol data path simple and performant while retaining later measured split points.
- Applications receive least-authority network objects with cross-host semantics rather than ambient socket state.
- The first implementation can be small static IPv4 without freezing an IPv4-only public contract.
- Correctness, ownership, backpressure, and teardown precede zero-copy, offloads, multiqueue, physical hardware breadth, and public-Internet use.
- Loopback and deterministic peers make most protocol work testable before a NIC and reproducible on Windows, Linux, and Windvale OS.
- VM networking and remote terminals reuse the same capability and lifecycle foundations but remain independently authorized later gates.

No NIC driver, link-port protocol, packet ring, IP stack, route table, resolver, TCP implementation, network capability, TLS provider, remote transport, firewall, virtual switch, or VM network attachment is implemented by this decision.

## Rejected alternatives

- **Put TCP/IP in the kernel:** enlarges the most trusted and remotely reachable parser/state-machine surface and couples protocol failure to kernel survival.
- **Use one process for every network layer immediately:** makes each packet cross excessive IPC boundaries and distributes tightly coupled connection state without measured benefit.
- **Make POSIX sockets the native contract:** imports ambient descriptors, numeric host constants, broad authority, process-global resolver behavior, and host-specific options.
- **Invent Windvale replacements for Internet protocols:** creates an interoperability and security burden without advancing the distinctive capability and isolation goals.
- **Adopt an external stack permanently as the semantic definition:** weakens Windvale ownership and makes another runtime or language part of the OS contract; external code remains useful as an oracle or named bootstrap.
- **Start with zero-copy and every hardware offload:** makes ownership, validation, teardown, and debugging harder before performance evidence exists.
- **Build remote terminal transport before networking:** forces a private transport and security boundary before the network, entropy, identity, trust, and lifecycle foundations exist.

## Reconsideration triggers

Reconsider a boundary when:

- measured service transitions or copying prevent a named workload from meeting a recorded performance target;
- the single network-service failure domain gives one component materially excessive authority that a measured split can reduce;
- an essential NIC cannot be reset, isolated, or revoked safely from a user-space driver;
- a required compatibility product cannot be expressed through a bounded adapter above semantic capabilities;
- dual-stack implementation cost blocks a useful bounded profile without a safe explicit temporary limitation;
- a production cryptographic or protocol implementation cannot be qualified within the accepted bootstrap and dependency policy; or
- VM networking, routing, high-rate packet processing, or multi-core scaling provides evidence for a different service or queue topology.

Any revision must preserve explicit authority, bounded untrusted parsing, deterministic tests, exact lifecycle and completion evidence, standards interoperability, DMA containment, and an independent recovery path.
