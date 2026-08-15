# Windvale open questions

> Status: Active unresolved choices after completed Milestones 1 through 3 and
> the signed `v0.1.0` preview.
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
- Managed Stage 0 source is archived outside `main` under
  [Decision 0558](../Decisions/0558-Archive-Managed-Stage0-Outside-Main.md); its
  exact pre-removal state remains `stage0-recovery-e5a1a7473c57`.
- Windows and Linux remain permanent hosts, canonical WVB remains the shared
  distribution contract, and Windvale OS remains the vertical integration
  target.
- Milestone 1 established affected-owner feedback within the two-minute local
  and five-minute pull-request targets while keeping complete qualification an
  explicit promotion gate.
- Milestone 2 established the first admitted Bundle 1, bounded immutable store,
  package-backed WVDB Query application, and rights-reduced success/denial path.
- Milestone 3 established the official 0.1 root/release delegation, signed
  Release Envelope 1, offline verifier, Windows/Linux installers, and exact-state
  qualification published as `v0.1.0`.

Historical documents that predate those decisions must not present them as
unfinished work.

## Immediate roadmap questions

### Milestone 4 selection

- Is the primary next user an installer/tool user, a Windvale OS developer, or
  an application/database user?
- Should Milestone 4 close the offline generation-and-rollback lifecycle,
  complete OS-1 composition, or prove one bounded durable application workload?
- Does the selected gate produce `v0.2.0`, a development checkpoint, or evidence
  deliberately combined with a later product track?
- Which dependencies are explicitly excluded so networking, new language
  semantics, or broader host authority cannot silently expand the gate?

### Development verification maintenance

- Which remaining expensive owner spends most of its time reconstructing
  immutable products rather than exercising changed behavior?
- Can the 1,600-line changed-path planner be replaced by declarative per-owner
  dependency manifests while retaining fail-closed coverage and a verified
  generated plan?
- Which exact source/product boundaries justify the next development checkpoint,
  and which must remain cold because identity reconstruction is the evidence?
- Which artifact families can move to deliberate promotion commits so ordinary
  semantic work does not repin unrelated native applications and documentation?

### Durable application and database

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

- When a second preview is prepared, is one replacement release key beneath a
  new root-signed policy sufficient, or does operational experience justify a
  threshold/root-rotation successor contract?
- Which two real packages are sufficient to prove a general offline resolver,
  generation inventory, activation recovery, and rollback without adding a
  registry?
- Which exact launcher boundary binds a selected generation and its approvals
  without giving the package client ambient process or filesystem authority?

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

The next decision should define the selected Milestone 4 outcome and its finite
gate. If OS-1 is selected, limit the following semantic decision to the first
post-Probe-40 resource-domain and atomic-launch contract. If the offline package
lifecycle is selected, decide Generation 1 and Activation 1 transaction semantics
before adding commands or network retrieval.
