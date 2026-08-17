# Windvale open questions

> Status: Active unresolved choices after completed Milestones 1 through 4, the
> signed `v0.1.0` preview, and selection of Milestone 5 / `v0.2.0`.
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
- [Decision 0590](../Decisions/0590-Offline-Package-Lifecycle-And-Generation-Activation-1.md)
  selected the offline package lifecycle as Milestone 4 without assigning a
  `v0.2.0` release. Decisions 0578 and 0580 close its paired activation,
  rollback, and safe-uninstall evidence in GitHub run `31906316540`; OS-1 and
  database work remain independent.
- [Decision 0595](../Decisions/0595-Select-Windvale-0.2.0-Connected-Services-Preview.md)
  selects the native database service, Windows/Debian service lifecycle,
  official connected installer/repository, and one real external-model gateway
  as the required `v0.2.0` product gate. OS work continues independently and is
  included only to its ready, accurately qualified boundary at freeze.

Historical documents that predate those decisions must not present them as
unfinished work.

## Immediate roadmap questions

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

- Which exact EWorker Data Service repository/tag and licensing record define
  the database parity baseline?
- Which legacy file/protocol behaviors require direct compatibility, which need
  migration only, and which are intentionally replaced?
- Which finite client concurrency, query/index, transaction, backup/restore,
  repair, and migration rows belong to the required `0.2.0` parity profile?

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
- Should exact `try` propagation ever admit a named adapter or an owned failure
  payload, and what transfer and cleanup ordering would make that widening safe?
- Which two measured consumers justify the first bounded deterministic keyed
  collection and its capacity, collision, and iteration contract?
- Which first unsupported package, database, or OS operation should broaden the
  shared native backend, and what interpreter/AOT/JIT agreement proves it?

## Package and release questions

The durable direction is in
[Packages-Releases-And-Recovery.md](../Architecture/Packages-Releases-And-Recovery.md).
The active questions are:

- Is one replacement release key beneath a
  new root-signed policy sufficient, or does operational experience justify a
  threshold/root-rotation successor contract?
- What exact freshness clock, high-water state, redirect policy, and revocation
  record are sufficient for the first official online repository?
- Which stable bootstrap URL and release-asset hosts fit the first rights-limited
  network grant without making transport location an authority source?
- Which service upgrade health result permits activation to remain current, and
  which database-format minimum version must block package rollback?

## Agent runtime and digital subconscious questions

The proposed [agent runtime architecture](../Architecture/Agent-Runtime-And-Digital-Subconscious.md)
and [implementation plan](Windvale-Agent-Runtime-Implementation-Plan.md), together
with the proposed
[persistent-self governance architecture](../Architecture/Persistent-Self-Ownership-And-Governance.md)
and
[executive-function and qualification architecture](../Architecture/Agent-Executive-Function-And-Qualification.md),
establish direction but intentionally leave the following choices open until a
fixed corpus can measure them:

- What are the exact first wire-independent agent, run, event, typed-claim,
  context-manifest, context-diff, checkpoint, and cognitive-operation records,
  and what byte, item, depth, diagnostic, and work limits bound each one?
- Which agent-definition, policy, model, provider, tool-catalog, memory-mount,
  source, and protocol generations belong to the operation, working-knowledge,
  and foundation clocks, and what enters the revisit set when one changes?
- Which exact epistemic and support kinds are sufficient for the first claim
  ledger, and how do later domain owners add meaning without turning every
  allegation, preference, compiler result, or observation into a generic fact?
- Which exact small language/runtime/operating-system project snapshot,
  candidate milestones, injected source change, failed verification, safe local
  action, real authority gate, and bounds should instantiate the first
  Mandate-to-Milestone corpus?
- Which minimum deliberation strategies, capability profiles, success/stop
  reasons, autonomous-continuation conditions, and escalation fields belong in
  the first contract without encoding a provider prompt or private reasoning?
- Which independent outcome rubric and initial baselines make unplanned human
  intervention, autonomy horizon, direction amplification, recovery,
  capability realization, correction, and handoff reports meaningful without
  collapsing them into one score?
- Which document, legal, financial, or corporate-work variant should follow the
  software reference, and what domain evidence and authority owners must differ?
- Which identities, sequence numbers, deadlines, and timestamps remain supplied
  inputs in the first portable kernel, and which later semantic clock or entropy
  capabilities should create them?
- What is the smallest evidence-bundle and projection-lifecycle contract that
  proves freshness, contradiction, omission, truncation, sufficiency,
  invalidation, and rebuild without requiring embeddings or a general graph?
- Once the database proposal's durable writer contract is finalized, what exact
  mapping provides expected-revision event append, idempotency replay/conflict,
  ordered replay, checkpoints, and large-body references without inventing a
  second persistence protocol?
- How should the first agent capsule map into the accepted provider-neutral
  model request/result contract, and which agent-specific role, context,
  placement, cancellation, usage, truncation, malformed-output, and response-
  fingerprint evidence must remain outside the general model protocol?
- Which first action capability demonstrates lease-bound governed effect without
  making model output executable authority or retrying an indeterminate mutation?
- Which retention, deletion, legal-hold, backup/restore, quota, telemetry,
  provider-placement, and emergency-pause responsibilities belong to portable
  agent records, host product policy, or existing platform capabilities?
- What exact software and proposal fixtures jointly prove the complete design
  while keeping business-domain semantics outside the portable kernel?
- What exact roster identity, approval threshold, expiry, environment/data
  scope, audit route, and test-only bypass representation should instantiate the
  first E-Worker Development Authority fixture?
- Which personal, organizational, collaborative, or assisted arrangement should
  supply the first Profile C corpus, and what principal/steward/domain/custody/
  audit/recovery separation is realistic for it?
- Which exact self fields and deployment obligations require stricter or
  narrower authority than the proposed change-class and field matrix defaults?
- What cooling-off, dissent, lost-steward, compromised-operator, appeal, and
  emergency-resumption rules permit recovery without recreating a sole owner?
- Should a test self ever retain identity across the development-to-advanced
  gate, or should the first profile always create a successor and admit only
  separately authorized memories and commitments?
- How should several eligible intentions express parent purpose, conflict,
  priority, fairness, preemption, satisfaction, cancellation, and expiry without
  creating secret goals or implicit action authority?
- What smallest nominal world/belief model proves entity, state, chronology,
  hypothesis, expected observation, contradiction, calibration, and revision
  without committing Windvale to a general knowledge graph?
- Which first deterministic salience classes are adequate for relevance,
  urgency, novelty, uncertainty, progress, risk, commitment, blocker, and
  prediction error, and which dimensions must never become a reward objective?
- Which working, episodic, semantic, procedural, prospective, and
  autobiographical memory transitions may occur automatically in run-private
  scope, and which require user, domain, policy, or business-owner review?
- What exact wake sources, coalescing identity, rate limits, fairness, missed-wake
  behavior, cancellation, and dormancy result let the subconscious continue
  across time without becoming an unlimited background model?
- Which evidence can calibrate the functional self-model's skills and failures,
  and how must the agent refuse, degrade, or ask for help when that evidence is
  missing or stale?
- What smallest social-model profile distinguishes observed statements, accepted
  preferences/commitments/consent/authority, knowledge evidence, and inferred
  motives or reactions without enabling cross-relationship leakage or hidden
  behavioral profiling?

## Organizational Observatory questions

The proposed
[organizational Observatory architecture](../Architecture/Organizational-Observatory-And-Epistemic-Infrastructure.md)
and
[implementation plan](Windvale-Organizational-Observatory-Implementation-Plan.md)
define a product direction, not final answers to these choices:

- Should **Windvale Observatory**, **Deliberation Fabric**, and **Windvale
  Constellation** become the durable product, subsystem, and federation names?
- Which exact synthetic organization, mandate, departments, reporting period,
  contradictions, stale facts, missing evidence, and decision deadline should
  define the first organizational-readiness corpus?
- Which observation, report, extraction, calculation, claim, belief,
  hypothesis, prediction, simulation, decision, commitment, and accepted-
  knowledge records belong in the first exact contract?
- How should source identity, source revision, observation time, effective time,
  reported time, ingestion time, and review time relate without treating a
  mutable report as an immutable fact?
- Which minimum provenance relations prove quotation, extraction, derivation,
  calculation, contradiction, supersession, invalidation, and dependency?
- Which epistemic fields remain domain-neutral, and which evidence-sufficiency,
  materiality, legal, financial, technical, or operational meanings must belong
  to separately qualified workplaces?
- What rubric proves that the first readiness brief is useful, complete,
  uncertainty-aware, and decision-relevant without rewarding confident prose?
- Which live source should enter first after the synthetic corpus, and which
  identity, authorization, rate, retention, deletion, legal-hold, and placement
  controls must precede it?
- Which exact expected-revision appends, idempotency outcomes, snapshots,
  replay queries, and measured indexes should the database provide for the first
  durable Observatory profile?
- Which exact revision records map the Observatory's observation, deliberation,
  epistemic, organizational, and foundation lanes onto the agent's operation,
  working, and foundation clocks without coupling independent owners?
- Which procedure, skill, calibration, and continuity evidence may an agent
  retain in its persistent self, and which organizational evidence, knowledge,
  policy, decision, commitment, or workplace state must remain only in the
  organization-owned node?
- Which employee, customer, health, legal, financial, credential, secret,
  message, behavioral, or other sensitive data must the first product exclude
  even when technically observable?
- Which first safe, reversible organizational action can prove governed effect
  without turning a recommendation or model output into executable authority?
- At what measured workload does distributed deliberation outperform one
  bounded executive sequence enough to justify independent jobs, evaluators,
  budgets, and merge policy?
- How independent must an evaluator be from the proposer, source connector,
  model provider, and decision owner before its challenge counts as useful
  verification evidence?
- Which review, dissent, appeal, correction, and notification mechanisms keep
  admitted organizational knowledge contestable after publication?
- What real cross-organization problem would justify Windvale Constellation
  research, and which disclosure, trust, revocation, provenance, and consensus
  boundaries must be frozen before federation begins?

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

Generation 1 and Activation 1 semantics and the first host publication mechanics
are accepted under Decision 0590. The next package decision should be limited to
command dispatch or package ownership only if implementation reveals a cross-host
semantic choice not already fixed by that contract. OS work should independently
limit its next semantic decision to the first post-Probe-40 resource-domain and
atomic-launch contract.
