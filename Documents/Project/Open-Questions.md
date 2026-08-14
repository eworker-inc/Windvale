# Windvale open questions

> Status: Active unresolved choices after the completed .NET retirement gate.
> Detailed recommendations live in architecture documents; this file lists only
> questions that can change the active roadmap or its next bounded slices.

## Resolved boundaries

The following are no longer open project questions:

- .NET is retired from the accepted normal Windows/Linux workflow under
  [Decision 0526](../Decisions/0526-Dotnet-Retirement-Qualification-And-Stage0-Archive.md).
- Forward source semantics belong only in `Compiler/Windvale` under
  [Decision 0213](../Decisions/0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md).
- Complete qualification is an explicit selected-state gate rather than a
  per-commit workflow under
  [Decision 0557](../Decisions/0557-Separate-Development-Verification-From-Qualification.md).
- Windows and Linux remain permanent hosts, canonical WVB remains the shared
  distribution contract, and Windvale OS remains the vertical integration
  target.

Historical documents that predate those decisions must not present them as
unfinished work.

## Immediate roadmap questions

### Development feedback

- After representative Windows and Linux runs, can ordinary pull-request
  verification meet the five-minute target with affected owners on both hosts,
  or should one host run during iteration and the second run at merge readiness?
- Which remaining expensive owner spends most of its time reconstructing
  immutable products rather than exercising changed behavior?
- Can the 1,600-line changed-path planner be replaced by declarative per-owner
  dependency manifests while retaining fail-closed coverage and a verified
  generated plan?
- Which exact source/product boundaries justify the next development checkpoint,
  and which must remain cold because identity reconstruction is the evidence?
- Which artifact families can move to deliberate promotion commits so ordinary
  semantic work does not repin unrelated native applications and documentation?

### Stage 0 live-source archival

- Should the frozen managed implementation be removed from `main` now that the
  exact recovery release and independent copy exist, or quarantined beneath one
  explicit `Bootstrap/Stage0` boundary?
- What minimal in-tree restoration record proves the release identity, checksum,
  source inventory, supported recovery hosts, and reconstruction command without
  retaining live managed projects?
- Which independent oracle cases remain valuable enough to restore on demand,
  and which duplicate evidence already owned by native fixtures?
- What one-time native audit and dual-host qualification demonstrate that source
  removal exposed no hidden build, package, website, test, or OS dependency?

### Useful package-backed application

- What is the smallest deterministic bundle that carries the exact Package 1,
  Lock 1, source, resource, license, and capability evidence required by WVDB
  Query?
- Which canonical digest and publication transaction identify an object in the
  first local content-addressed store?
- What exact approval record binds the transitive application capability closure
  to rights-reduced Windows and Linux provider instances?
- Which success, denial, unsupported-provider, stale-generation, partial-progress,
  and indeterminate-mutation cases are required before hosted execution is useful?
- How much repeated depth-three update, reclamation, and restart recovery does
  the application actually need before broader database work should wait?

### Windvale OS launch and service slice

- Which exact resource-domain record and limits are sufficient for the first
  cleanly launched application/provider composition after Probe 40?
- Which immutable semantic and kernel admission records make reserve, construct,
  publish, and rollback independently verifiable?
- Should the first isolated normal provider be console output or durable storage,
  and which existing host application supplies the clearer cross-environment
  comparison?
- What bounded queue, cancellation, peer-loss, provider-restart, and teardown
  behavior qualifies two clients without beginning general service discovery?
- Which structured completion record is sufficient for one supervised restart or
  deliberate terminal failure?

## Language, compiler, and runtime questions

- Which direct consumer first requires caller-visible liveness for relocating
  descriptor-bearing aggregate returns?
- Which useful program justifies integrating the complete allocator-emission
  schedule rather than retaining the qualified bounded lifetime profile?
- Which capability-reference, cleanup-order, and cleanup-failure contracts must
  exist before scoped ownership syntax is accepted?
- Which bounded result-propagation or collection feature removes enough measured
  application code to justify a new source-language contract?
- Which first unsupported package, database, or OS operation should broaden the
  shared native backend, and what interpreter/AOT/JIT agreement proves it?

## Package and release questions

The durable direction is in
[Packages-Releases-And-Recovery.md](../Architecture/Packages-Releases-And-Recovery.md).
The active questions are:

- Which bundle encoding and maximum sizes can be independently admitted before
  extraction without requiring compression or a registry?
- Which first signing algorithms, offline-root ceremony, release-key rotation,
  and revocation rules are small enough for the 0.1 preview?
- Which threat-model assets and attackers cover the actual shipped parsers,
  verifiers, providers, package inputs, and recovery process?
- What exact offline third-party command verifies every source, package, tool,
  license, provenance, and qualification artifact in the preview?
- Is the qualified Stage 0 release retained as a referenced immutable dependency,
  or copied into each future release envelope by identity?

## Later architecture questions

The following lanes remain important but do not select the immediate milestone
unless a current product produces direct pressure:

- [Console, shell, and CLI](../Architecture/Console-Shell-And-Cli.md): terminal
  events, stream contracts, launch plans, bounded grammar, and recovery commands.
- [Network stack](../Architecture/Network-Stack.md): asynchronous operations,
  `LinkPort 1`, dual-stack semantics, rights-limited grants, and isolated drivers.
- [Remote terminal](../Architecture/Remote-Terminal-Protocol.md): authenticated
  secure-stream framing, identity/authorization, bounded sessions, and teardown.
- [Identity, time, entropy, and trust](../Architecture/Identity-Time-Entropy-And-Trust.md):
  provider generations, secure entropy, civil time, key custody, and release trust.
- [Memory objects and resource domains](../Architecture/Memory-Objects-And-Resource-Domains.md):
  noncontiguous allocation, accounting, revocation, and domain teardown.
- [Virtualization](../Architecture/Windvale-Os-Architecture.md): physical/root
  qualification, guest memory, vCPU, IOMMU, reset, DMA, and recovery evidence.
- [WebAssembly playground](WebAssembly-Playground-Exploration.md): browser support,
  package size, bounded storage, and whether WebAssembly becomes a permanent host
  or target.

## Decision discipline

A numbered decision should answer a durable semantic, format, capability, ABI,
security, bootstrap, recovery, or qualification-model question. Ordinary
implementation checkpoints, fixture additions, artifact refreshes, and timing
measurements should update code, specifications, the changelog, or the progress
dashboard without creating another decision record.

The next decisions should therefore be limited to:

1. the live Stage 0 archival policy, if source removal is selected;
2. a package bundle/content-store contract when the WVDB Query consumer is ready;
3. an exact capability approval/binding record for that application; and
4. the first post-Probe-40 resource-domain and atomic-launch contract.
