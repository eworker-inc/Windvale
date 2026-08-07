# Windvale open questions

This list records unresolved choices only. Accepted product-wide direction is recorded by [Decision 0178](../Decisions/0178-Project-Stewardship-Archives-And-Recovery.md), [Decision 0179](../Decisions/0179-Language-Application-And-Capability-Metadata-Direction.md), [Decision 0180](../Decisions/0180-Compiler-Runtime-And-Native-Toolchain-Boundaries.md), [Decision 0181](../Decisions/0181-Next-Windvale-Os-Mechanism-Contracts.md), [Decision 0182](../Decisions/0182-Browser-And-WebAssembly-Product-Direction.md), [Decision 0183](../Decisions/0183-Product-Packaging-Trust-And-Evolution.md), [Decision 0184](../Decisions/0184-Language-Syntax-And-Operator-Evolution.md), [Decision 0191](../Decisions/0191-Windvale-Console-Shell-And-Cli-Architecture.md), [Decision 0192](../Decisions/0192-Capability-Oriented-User-Space-Network-Stack.md), [Decision 0193](../Decisions/0193-Simple-Windvale-Remote-Terminal-Protocol.md), and cross-host-qualified [Decision 0196](../Decisions/0196-First-Generation-Safe-Non-Tail-Memory-Object-Reclamation.md). Proposed [Decision 0198](../Decisions/0198-Next-Integrated-Architecture-Defaults.md) selects recommended successor defaults for the next integrated review but is not accepted or implemented. Implementation details remain open when those records deliberately require a measured consumer, hardware inventory, or qualification gate.

## Proposed defaults awaiting review

Decision 0198 and its linked architecture guides recommend:

- Probe 40's qualified bitmap, ownership, page-vector, generation, and zero-before-reuse invariants as the fixed baseline, followed by separate mappings and one flat resource domain before measured generalization of the object inventory or page selection;
- a two-level immutable launch plan committed through one atomic clean-spawn transaction with bounded supervision;
- directional byte streams, typed terminal events, versioned small shell grammar, and all-stage pipeline success;
- copied `LinkPort 1`, a minimal modern single-queue `virtio-net` profile, and honest virtual-IOMMU qualification scope;
- separate monotonic/civil time, secure/test entropy, key, identity, trust, and authorization interfaces;
- content-addressed package parts, canonical lockfiles, immutable installation generations, signed releases, and a .NET-free Windows/Linux Windvale 0.1 gate; and
- nominal payload variants, explicit result matching, move-only bounded builders, immutable sequences, and structured platform/capability metadata.

The questions below now ask for exact encodings, limits, consumers, and evidence or call out a remaining product choice from that proposed direction.

## Language and application model

- Which WVB version, case-table encoding, construction and match operations, maximum value-pressure proof, and native ABI evidence qualify the proposed nominal payload-variant contract, and which later result annotation could make visible propagation unambiguous?
- Which exact source spelling and canonical source/package/WVB table encodings carry the proposed environment, architecture, ABI, extension, authority, required-capability, and optional-capability fields, and which source edition performs the one-way migration from current profile bytes?
- Which first application selects the exact `sequence<T, N>` and move-only `builder<T, N>` constructors and operations, and which allocation, exhaustion, partial-mutation, freeze, stale-builder, and representation-independent evidence qualifies them?
- Which first bounded consumer should add text/bytes content equality, and which explicit derived-equality syntax should later admit immutable records or variants without giving capabilities, builders, functions, or resources general equality?
- Which first real scientific, graphics, media, or ML workload justifies floating-point semantics, and which exact IEEE, NaN, conversion, comparison, and formatting rules does it require? After those foundations mature, which bounded model should revisit the [mathematical and scientific modeling exploration](Mathematical-And-Scientific-Modeling-Exploration.md), and does evidence favor an ordinary library, a restricted source profile, or a notation front end lowering through the shared Windvale execution stack?
- Which ownership, scheduler, cancellation, and failure-propagation evidence is required before structured concurrency enters the source language?
- After initial self-hosting and the first stable language, is there enough normalization, confusable-character, editor, formatter, and security evidence to propose an optional broader-Unicode identifier revision?

## Compiler and runtime

- Which dual-host, browser, and native evidence should promote the implemented-candidate checked `i64`/`u64` path into the accepted default format and execution surface?
- Which first application graph requires cyclic ownership, and does measured evidence favor tracing, reference counting with cycle handling, regions, or another physical reclamation mechanism above the shared semantic ownership model?
- Which measured branch, call, data-reference, or wider-patch consumer should define the first stencil after `WVSP 1` and `WVSP 2`?
- Which measured compiler, runtime, OS, cryptographic, or performance consumer first requires division, variable-count shifts, conditional moves, or a shared production encoder?
- Which measured source consumer first requires `/`, `%`, or unsigned bitwise operators, and which WIR/WVB version should carry Decision 0184's accepted zero, overflow, remainder, count, and fixed-width shift behavior?
- Which deterministic hotness thresholds and native-cache limits are appropriate after representative interpreter, JIT, cached, and AOT workloads exist?
- What is the exact versioned normalized execution-transcript format for cross-engine differential evidence?
- Which binary-size, startup-memory, cold-start, and trusted-surface budgets should constrain the first permanent Windvale-native runtime?
- After the Windvale assembler retires the C# assembler from the normal path, which ergonomic source mode and source-map contract should expand expressions, constants, declaration ordering, or macros into canonical WVA?
- Which canonical debug-sidecar records are needed by the first debugger consumer, and which records should adapters translate to CodeView or DWARF?

## Operating system

- Which invariants from qualified `WVPROC17` and Probe 39 must survive the first general timer/scheduler slice without freezing the private timer/context offsets, fixed three-slot order, or one-thread shape?
- Which physical-machine measurements qualify HPET/local APIC beyond the pinned Q35 candidate, and what calibrated evidence would justify selecting invariant TSC instead of HPET on a later machine profile?
- Which dynamic-process, fragmentation, or noncontiguous-allocation consumer should generalize Probe 40's fixed bitmap, owner bytes, and three `WVMEMO01` records, and what page-set selection, zeroing, stale-generation, rollback, and scan-cost evidence qualifies that successor?
- Which exact COM1 configuration, batching limit, provider protocol, revocation sequence, and diagnostic separation qualify the first isolated serial-output service?
- Which interface identity and record shape publish the immutable directory provider to two clients, and which queue limit and backpressure result qualify the first multi-client endpoint?
- Which exact reduced-right copy, cancellation/deadline, provider-replacement, and shared-memory queue consumers should implement the accepted capability sequence one slice at a time?
- After checked `u64` is qualified on every intended target, which first versioned Windvale filesystem interfaces and provider protocols should implement `Open`, `Readˉat`, `Writeˉat`, `Setˉlength`, and `Close`?
- Which physical or root Windows and Linux machines own direct Hyper-V Generation 2, optional WHPX, and KVM qualification, and which nested topologies merit separate qualification?
- Does the first suitable physical Windvale machine select VMX or SVM, and what exact private-memory, reset-state, exit, budget, and teardown records qualify the minimal profile?
- Which secondary non-display GPU or accelerator can prove isolated IOMMU ownership, interrupt remapping, reset, DMA revocation, teardown, and rebind before exclusive passthrough is accepted?
- Which pinned workloads and per-machine noise measurements establish the first VM, memory, storage, network, graphics, and compute regression thresholds?

## Console, shell, and CLI

The [console architecture guide](../Architecture/Console-Shell-And-Cli.md) fixes the device/terminal/shell/application split and now recommends directional stream results, typed terminal events, a two-level launch plan, staged Shell 1–3 grammar, a first command catalog, and all-stage pipeline success. The remaining questions are focused encodings and limits:

- Which bounded serial-input adapter and exact UTF-8/control-event profile should qualify the first terminal session without making ANSI escape bytes the semantic interface?
- Which exact event ordinals, key/modifier set, editing behavior, batch limits, and scrollback policy qualify the proposed terminal event family for the first serial shell?
- Which interface identities, binary records, queue limits, control reserve, and provider-loss tests qualify the proposed directional read/write/close results without requiring indeterminate outcomes for local streams?
- Which exact serialization and digest bind the proposed semantic and kernel launch plans, and which rollback corpus proves that no malformed or rejected plan publishes a partial child or moved capability?
- Which exact Shell 1 quote escapes and byte/word limits qualify the first parser, and which later measured stream/file consumer activates each reserved Shell 2 operator?
- Which subset of the proposed external command catalog is required for the first recovery session, and what optional alias format preserves canonical inspectable command identity?
- How should a directory capability expose a stable user-facing current-location identity and redirection target without making a native path the shared contract?
- Which structured completion and pipeline-record encoding preserves every stage while implementing the proposed all-stage-success and first-failure display rule?
- What bounded history/configuration format, sensitive-input suppression, and storage grant are safe before startup customization is accepted?
- Which measured consumer first justifies schema-versioned typed pipelines above the universal byte-stream base?
- Which multi-user login, identity-directory, administrative-elevation, session-ownership, and session-replacement evidence is required beyond Decision 0193's provisioned first remote profile?

## Network stack

The [network-stack architecture](../Architecture/Network-Stack.md) fixes the protocol-blind kernel and now recommends copied `LinkPort 1`, explicit transmit completions, link generations, and a minimal modern single-queue `virtio-net` profile. The remaining questions are measured encodings, sizes, and protocol policies:

- Which exact `LinkPort 1` interface identity, record encoding, batch/queue limits, control reserve, correlation rollover, and reset corpus qualify the deterministic simulated link and first isolated NIC driver?
- Which exact allowed feature bits, descriptor and buffer counts, interrupt/coalescing behavior, Q35 virtual-IOMMU topology, DMA mapping, and reset sequence qualify the proposed modern single-queue `virtio-net` profile?
- Which address, prefix, port, interface, route, connection, listener, datagram, resolver, configuration, and provider-evidence records form the smallest dual-stack semantic API without exposing native socket types?
- Which grant constraints bind names, address prefixes, transports, ports, interfaces, directions, rates, bytes, connection counts, deadlines, and lifetimes, and how does resolve-and-connect preserve one authorization decision across DNS changes?
- Which bounded IPv4 and IPv6 header, option, extension, fragmentation, reassembly, ICMP, Neighbor Discovery, Duplicate Address Detection, and address-lifetime policies qualify the first general host profile?
- Which monotonic-timer resolution, retransmission, congestion-control, ephemeral-port, initial-sequence, receive-window, close, reset, and half-open-connection policies qualify the first TCP implementation?
- Which DHCP, DNS cache, negative-result, search-name, address-selection, route-selection, provider-restart, and configuration rollback rules are sufficient for the first configured network?
- Which entropy, trust-store, certificate, peer-name, key-custody, civil-time, protocol-version, revocation, and test-vector evidence qualifies the first TLS 1.3 secure-connection provider?
- Which measured copy volume first justifies a versioned shared packet or stream ring, and which ownership, generation, notification, batching, zero-copy, multiqueue, RSS, or offload rules preserve exact validation and teardown?
- Which virtual-port, bridge, filter, NAT, routing, audit, capture, and attachment contracts safely connect a future Windvale-hosted guest without making VM management grant host-network authority?
- Which physical NIC and Hyper-V synthetic adapter provide the first non-QEMU evidence after the modern `virtio-net` path is qualified?

## Remote terminal protocol

The [remote-terminal architecture](../Architecture/Remote-Terminal-Protocol.md) fixes the secure-stream carrier, one-connection/one-session first profile, separate identity and authorization, typed terminal control, bounded framing, disabled TLS early data, connection-owned teardown, and later compatibility-adapter direction. The remaining questions require measured implementation evidence:

- Which published protocol name, ALPN identifier, port, discovery rule, frame type numbers, flag assignments, maximum payload, and version-negotiation encoding qualify the first `WVTS/1` specification?
- Which bounded small-certificate profile, signature suite, subject-public-key digest, provisioning artifact, key store, rotation, revocation, recovery, and audit records qualify the first mutually authenticated client and server identities, and what later evidence would justify an RFC 7250 raw-public-key profile?
- Which exact rights-limited remote-session profiles, listener bindings, connection limits, source constraints, authorization results, and optional elevation ceremony are useful without creating ambient remote-root authority?
- Which canonical key/modifier set, strict-UTF-8 text limits, resize bounds, normal/diagnostic ordering evidence, completion records, and terminal echo behavior are sufficient for the first line-oriented client?
- Which input, output, diagnostic, parser, control-reserve, authentication-attempt, session, rate, timeout, cancellation, drain, and forced-teardown limits preserve recovery under a hostile or stalled peer?
- Which exact deterministic and fuzz corpus covers split/coalesced reads, truncation, oversize, invalid UTF-8, invalid enums, unsupported versions/features, illegal state transitions, replay attempts, backpressure, provider loss, and disconnect cleanup?
- Which real workload first justifies multiple sessions, detach/resume, roaming, keepalive, richer terminal operations, SSH interoperability, WebSocket/browser carriage, or a QUIC stream?

## Identity, time, entropy, and trust

The [identity, time, entropy, and trust guide](../Architecture/Identity-Time-Entropy-And-Trust.md) recommends distinct providers and immutable generations. Remaining questions require cryptographic and operational evidence:

- Which exact monotonic/civil instant records, uncertainty classes, provider-generation rules, and clock synchronization source qualify the first production civil-time capability?
- Which host APIs, Windvale OS entropy sources, conditioning and DRBG profile, startup/reseed thresholds, health tests, VM snapshot behavior, and failure corpus qualify `entropy.secure` without allowing the deterministic test provider to satisfy it?
- Which exact small-certificate and pinned-subject-public-key profile, algorithm suite, encoding, parser bounds, provisioning artifact, and Windows/Linux interoperability evidence qualify the proposed first mutual-TLS path?
- Which key-operation, trust-snapshot, identity, authorization, rotation-overlap, revocation, recovery, and audit encodings qualify the first production remote-session and release-signing providers?
- Which key-custody mechanism is sufficient for the first production Windvale OS listener, and which separately labeled QEMU provider remains test evidence only?

## Browser and WebAssembly

The [WebAssembly playground exploration](WebAssembly-Playground-Exploration.md) remains the implementation and evidence inventory. Decision 0182 accepts a product direction without yet accepting WebAssembly as a permanent host or compiler target.

- Which bounded Windvale-native verifier, interpreter, compiler, or Module Inspector slice is the first useful experimental browser route, and which limitations must its UI expose?
- Which bounded reusable or reclaiming interpreter-owned storage model should follow Decision 0177's exact 1,511/1,512 compiler boundary, and what reset, stale-reference, allocation, and cross-engine evidence qualifies it?
- Which exact Chromium, Firefox, WebKit, and real-Safari versions, desktop/mobile environments, memory ceilings, and timeout evidence define the first supported browser profile?
- What are the exact source signatures, ordering, batching, cancellation, deadline, and closed-source behavior of the first bounded wait-set or event-stream interface?
- Which canonical output schema and bounded exported function make the Windvale Module Inspector a reproducible Windows/Linux/browser sample?
- When the Windvale-native route is complete, which retained .NET browser projects remain bootstrap/recovery evidence and which leave normal build and publication automation?
- Which real application should justify accepting direct WIR-to-WebAssembly compilation as a permanent target after WebAssembly is accepted separately as a host?

## Product and release lifecycle

- Does product review accept the proposed Windvale 0.1 gate—complete .NET-free normal Windows/Linux operation, exact Stage 0 recovery, one packaged useful application/library, explicit grants, signed reproducible artifacts, threat model, and offline third-party verification—or should an earlier artifact be named a development preview instead?
- Which exact canonical package-manifest, lockfile, bundle, content-store, and installed-generation encodings implement the proposed separation while recording per-part target selection, capability closure, dependency origin, license, integrity, and provenance without a registry?
- Which independent version fields and compatibility rules belong in the first source edition, package manifest, and capability-binding implementation?
- Which offline-root and release-key threshold, rotation overlap, revocation/freshness mechanism, verification envelope, and optional SLSA translation qualify the first official signed release while retaining unsigned local development?
- Which threat-model assets, attackers, boundaries, and residual risks should be documented first, and which normative validators or runbooks own each mitigation?
- Which structured diagnostic envelope and redaction rules should become the first shared compiler/runtime/service/OS observability contract?
- Which measured application first needs monotonic time, civil time, secure entropy, deterministic test entropy, name resolution, connection, or listening capabilities?
- After the x86-64 shared backend and initial OS process, scheduler, memory-object, and isolated-driver paths stabilize, which hardware and product value should trigger an ARM64 proposal?

## First decision sequence

Decisions 0058 through qualified 0103, 0105, 0108, 0109, 0111, 0112, 0133, and 0150 establish reproducible bytecode compiler convergence, the bounded shared native path through frame-owned direct records and generation-owned dynamic values, all current service leaves and calls through 64 parameters, typed block-scoped physical storage under the 2,048-cell bound, bounded exact-compiler publication and complete native reproduction, live Windvale-produced service leaves, Windvale-owned executable-image layout and lifetime, WVA-owned Q35 poweroff, normalized trap entries, the first kernel-owned W^X root, fixed in-guest WVB admission, protected processes, the first init/resource service, a user-space Windvale bytecode interpreter, section-derived validation, typed WVB/execution-budget publication, automatic terminal cleanup, generation-safe reclaim/reuse, and two exact compiler-produced WVB programs across hosts and Windvale OS. Decisions 0104 through 0177 also retain a separate WebAssembly interoperability track with a capability-free in-memory compiler contract and the exact 1,511/1,512 compiler execution boundary. The recommended next implementation decisions remain:

1. Add caller-visible descriptor liveness before relocating descriptor-bearing aggregate returns; do not infer aggregate safety from the direct-descriptor proof.
2. Starting from cross-host-qualified Probe 40, add one flat resource domain before dynamic launch, supervision, or driver isolation. Keep each mechanism a separate evidence claim.
3. Satisfy the remaining Decision 0057 native-retirement conditions while accumulating Decision 0178 recovery evidence gradually; remove .NET from normal automation only from one fully qualified source state.

The early experimental Windvale-native browser route from Decision 0182 may advance independently when its bounded profile is honest. It does not replace these native and OS priorities or make WebAssembly permanent.
