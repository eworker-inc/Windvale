# Decision 0198: Next integrated architecture defaults

- Date: 2026-08-03
- Status: Proposed for product review; no implementation or public ABI is accepted by this record
- Language-design status: Superseded by [Decision 0751](0751-Accept-Windvale-Language-1.0-Direction.md); the remaining cross-stack proposal is not accepted by that supersession
- Builds from: cross-host-qualified [Decision 0196](0196-First-Generation-Safe-Non-Tail-Memory-Object-Reclamation.md)
- Refines: [Decision 0173](0173-Windvale-Process-Service-And-Driver-Architecture.md), [Decision 0179](0179-Language-Application-And-Capability-Metadata-Direction.md), [Decision 0181](0181-Next-Windvale-Os-Mechanism-Contracts.md), [Decision 0183](0183-Product-Packaging-Trust-And-Evolution.md), [Decision 0184](0184-Language-Syntax-And-Operator-Evolution.md), [Decision 0191](0191-Windvale-Console-Shell-And-Cli-Architecture.md), [Decision 0192](0192-Capability-Oriented-User-Space-Network-Stack.md), and [Decision 0193](0193-Simple-Windvale-Remote-Terminal-Protocol.md)
- Architecture: [memory objects and resource domains](../Architecture/Memory-Objects-And-Resource-Domains.md), [process launch and supervision](../Architecture/Process-Launch-And-Supervision.md), [console and CLI](../Architecture/Console-Shell-And-Cli.md), [network stack](../Architecture/Network-Stack.md), [identity and trust](../Architecture/Identity-Time-Entropy-And-Trust.md), [packages and releases](../Architecture/Packages-Releases-And-Recovery.md), and [language design](../Architecture/Language-Design.md)

## Context

Windvale's accepted architecture establishes the major trust boundaries, and cross-host-qualified Probe 40 now proves fixed generation-safe non-tail memory-object reclamation. Resource domains, general object inventory, dynamic launch, terminal streams, link transport, identity, package lifecycle, and language values still depend on one another. Choosing each independently could create conflicting ownership, generation, cancellation, and failure models.

This proposal selects coherent defaults for discussion. It does not freeze record bytes, syscall numbers, queue sizes, algorithms, package encodings, source tokens, or qualification thresholds. If accepted after review, each implementation slice still needs its own focused decision and evidence.

## Proposed direction

### Use one lifecycle pattern across kernel objects and providers

Resources reserve complete capacity, construct privately, validate fully, publish atomically, become generation-visible, stop accepting work before teardown, revoke external access, release in dependency order, and only then permit identity reuse. Rejection leaves no visible partial state. Provider replacement creates a new generation rather than impersonating continuity.

### Build memory before dynamic launch

Retain Probe 40's deterministic bitmap, ownership, page-vector, generation, zeroing, and non-tail-reuse evidence as the fixed baseline. Add one flat aggregate resource domain before dynamic launch. Let that first launch select the smallest general object inventory and retain contiguous first-fit unless a measured object requires deterministic noncontiguous page sets. Keep mappings distinct from backing, preserve W^X, reserve recovery capacity outside ordinary domains, and defer overcommit, paging, copy-on-write, hierarchy, and SMP invalidation.

### Launch through an immutable transaction

Separate a semantic user-space launch plan from a mechanism-only kernel admission plan. Resolve exact content, authorize the complete capability closure, reserve resources, construct a non-running process, bind reduced capabilities and streams, then publish the process atomically. Observation, cancellation, termination, inspection, and capability transfer are separate rights. Service restart is bounded, creates a new generation, and never silently replays an indeterminate mutation.

### Make byte streams and typed terminal events the console foundation

Use explicit bounded input, output, and diagnostic byte streams with exact end, cancellation, peer-loss, partial-progress, and indeterminate-mutation results. The terminal service translates devices or remote messages into typed text, key, resize, interrupt, end-input, and disconnect events. The first shell stays small, launches exact identities, retains all pipeline stage results, and treats a pipeline as successful only when every stage succeeds.

### Put a copied `LinkPort 1` before packet-ring optimization

The isolated driver owns device discovery, queue mechanics, DMA, interrupts, reset, and link state. The network service owns Ethernet and higher protocols. `LinkPort 1` uses copied receive batches, copied transmit submission plus explicit completions, immutable link snapshots, generations, and bounded control reserve. The first usable `virtio-net` profile is modern, single-queue, interrupt-driven, and minimally featured; QEMU evidence without an IOMMU is not misreported as containment against a malicious DMA-capable driver.

### Separate time, entropy, keys, identity, authorization, and trust

Monotonic time, civil time, secure entropy, deterministic test entropy, private-key operations, identities, trust snapshots, and authorization decisions are independent versioned interfaces. Secure entropy has no weak fallback. Private keys are non-exportable capabilities by default. Authentication proves an identity; authorization binds exact rights. The first remote profile uses provisioned mutually authenticated small certificates with pinned subject-public-key digests, while production listening waits for qualified key custody and revocation. Raw public keys remain a later profile option rather than a parallel first path.

### Make packages immutable and releases recoverable

Keep project manifests, package manifests, lockfiles, bundles, release envelopes, and update plans separate. Resolve and lock before build or launch. Use per-part platform and capability metadata, content-addressed objects, immutable installed generations, offline verification, and signatures that prove release provenance without granting runtime authority. Recommend Windvale 0.1 only after the .NET-free normal Windows/Linux gate, exact Stage 0 recovery, one packaged useful application/library, explicit capabilities, signed reproducible artifacts, and a public threat model.

### Add variants, bounded collections, and metadata without a second type system

Payload variants are nominal, immutable, exhaustively matched, and verifier-bounded. Recoverable results initially use ordinary two-case variants and explicit `match`, not exceptions or generic propagation magic. `sequence<T, N>` is immutable and bounded; `builder<T, N>` is uniquely owned, reports exhaustion, and is consumed by `freeze`. Platform, authority, required capability, and optional capability metadata receive independent canonical fields and migrate only through an explicit source/WVB version boundary.

## Proposed sequence

The main OS dependency line is:

1. retain qualified Probe 40 as the fixed timer and memory-object baseline;
2. add one flat resource domain over the three existing processes and objects;
3. add one atomic dynamic launch, generalizing the object inventory and page selection only as that process requires;
4. add minimal supervision and the isolated serial service;
5. qualify streams, terminal events, and the first shell;
6. add shared memory, DMA/IOMMU, `LinkPort 1`, and `virtio-net`; and
7. add network protocols, secure identity, TLS, and finally `WVTS/1`.

Language metadata, variants/results, bounded collections, package manifests, lockfiles, recovery evidence, and release tooling may progress in parallel when their direct consumers exist. They must reuse the same ownership, generation, capability, result, and deterministic-publication principles.

## Consequences

- Console, networking, packages, and remote access share one launch, stream, identity, and teardown model.
- Memory accounting becomes a prerequisite rather than a cleanup feature added after processes exist.
- The first device paths favor containment and deterministic evidence over speculative zero-copy performance.
- A TLS or release signature cannot become ambient administrative authority.
- Windvale 0.1 has a clear useful product meaning without depending on Windvale OS completion.
- The proposal adds documentation only and does not change Probe 40's qualified status. Every new format, interface, language feature, driver, service, release, and qualification gate introduced by this proposal remains unimplemented.

## Review questions

Before accepting this proposal, review:

- whether fixed-length committed memory objects are sufficient for the first dynamic process and stream workloads;
- whether all-stage pipeline success is preferable to last-stage status;
- whether the interoperability benefit of pinned small certificates justifies their bounded parser and provisioning cost over a future raw-public-key profile;
- whether qualified virtual-IOMMU evidence is required for the first usable isolated `virtio-net` claim;
- whether Windvale 0.1 should require the complete .NET retirement gate or allow a separately named earlier preview; and
- whether the proposed metadata, variant/result, and bounded-collection ordering gives the package and application paths the right prerequisites.

## Reconsideration triggers

Reconsider a default when a measured consumer cannot express correct ownership, bounded progress, recovery, or performance through it. Any replacement must preserve explicit authority, deterministic failure, generation safety, independent validation, and honest qualification scope. Convenience alone is not sufficient reason to introduce ambient inheritance, silent fallback, mutable package identity, weak entropy, unbounded queues, or an unreported DMA boundary.
