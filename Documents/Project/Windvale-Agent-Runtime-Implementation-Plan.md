# Windvale agent runtime implementation plan

> Status: Proposed dependency and delivery plan. No work item in this document is
> an implementation or qualification claim. The active project milestone remains
> the offline package lifecycle, and OS-1 remains the independent launch and
> service track until a later planning decision selects this lane.

## Purpose

This plan turns the proposed
[agent runtime and digital subconscious architecture](../Architecture/Agent-Runtime-And-Digital-Subconscious.md)
into small, verifiable Windvale slices. It also routes enabling work to the
compiler, database, storage, library, runtime, package, and operating-system
owners without making every dependency part of one foundation mega-change.

The target is one long-lived user-facing functional mind whose durable self
spans many episodes and whose cognition uses two cooperating execution planes:

- a foreground reasoning plane that presents one coherent voice; and
- a digital-subconscious coordination plane that owns direction, attention,
  evidence selection, memory proposals, doubt, authority preparation, and
  continuity.

The digital subconscious remains a support plane of the same persistent agent
identity. Individual runs are episodes inside that longer continuity. The first
product does not create a second autonomous agent identity.

The current E-Worker v7 edition of *The Mind We Build* supplies the narrative
completeness checklist. Windvale owns its own record encodings, limits,
capability contracts, storage mappings, and host qualification; neither the
book nor E-Worker DTOs become runtime dependencies.

## Planning decision

Begin with a capability-free portable semantic kernel and deterministic scripted
processors. Do not begin with a network model provider, multi-agent delegation,
durable shared memory, a general scheduler, or a Windvale OS service.

This order allows Windvale to prove the distinctive part of the design—owned
state, bounded attention, explicit doubt, deterministic merge, and continuity—
using current language semantics. Host scheduling and E-Worker's existing model
transport may later adapt to that kernel without becoming the definition of the
portable contract.

The first accepted implementation decision should freeze only the records and
state transitions needed for the deterministic qualification target. Public
provider and storage capabilities receive separate decisions when a real hosted
consumer reaches them.

Stages 0 through 7 establish the governed single-episode foundation. Later
functional-mind stages add the persistent self, multi-episode intentions,
recurrent workspace, world/belief/self models, salience, simulation,
consolidation, and event-driven wakeups. These later stages must reuse the
qualified evidence and authority boundaries; they do not justify skipping the
single-episode kernel.

## Current Windvale baseline

Windvale is not starting from zero, but the available pieces have narrower
contracts than a complete agent host.

| Needed foundation | Current standing | Planning consequence |
| --- | --- | --- |
| Immutable nominal records, enums, payload variants, and exhaustive matching | Implemented in current Seed/WVB | Sufficient for closed run, event, context, challenge, and result contracts. |
| Bounded immutable sequences and affine builders | Implemented | Sufficient for finite fixtures and small state projections; no general map should be assumed. |
| Checked `u32`/`u64`, bytes, text, SHA-256, and deterministic codecs | Implemented in selected profiles | Sufficient for bounds, revisions, fingerprints, and canonical record experiments. |
| Capability declarations and native provider table | Implemented in bounded form | Requirements and scalar/descriptor provider calls exist; typed capability values and nominal provider signatures do not. |
| Immutable package/lock/bundle identity and approval/launch evidence | Implemented for the first bounded application | Strong authority precedent; current approval grammar is not a general agent policy. |
| Random-access mutable storage and durable publication planning | Implemented in focused bounded profiles | A future run store can reuse semantics, but current fixed providers are not a general agent-state service. |
| Durable database | Focused single-writer storage/tree/recovery candidates accept bounded owned paths for input depths two through eight and propagate full splits | Suitable future owner for run state and memory, but not yet a general run ledger or indexed memory service. |
| Dynamic process launch, supervision, and general resource domains | Designed and partially proven in fixed OS slices | Not required for the first Windows/Linux semantic kernel; required before a Windvale OS agent-host claim. |
| Monotonic clocks, structured tasks, cancellation, channels, and synchronization | Proposed or absent as general source/runtime contracts | Host-driven one-transition-at-a-time execution is the first route. |
| Networking, secure streams, identity, and production key custody | Future architecture | Not required by the deterministic kernel; required for a remote production model provider. |

## Delivery invariants

Every stage preserves these rules:

1. One persistent agent self may own many episodes and intentions; each run has
   one primary durable objective and one current revision.
2. Model output, tool output, retrieved text, and imported memory are untrusted
   until the owning deterministic boundary validates them.
3. Canonical owners remain authoritative over their records; evidence and
   verification justify revisable beliefs; deterministic owners govern action.
4. Every accepted command has an expected revision and idempotency identity.
5. Every serialized record is versioned, bounded, canonical, and rejected on
   trailing, truncated, reordered, duplicate, inconsistent, or oversized input.
6. A requirement, root approval, concrete grant, provider binding, and
   operation-specific lease remain separate evidence.
7. Large evidence is referenced rather than copied through every event and
   context.
8. A failed verifier, stale source, declined approval, or unknown effect cannot
   be hidden by later optimistic prose.
9. Windows and Linux implement the same portable semantics; Windvale OS later
   supplies another provider composition rather than redefining the agent.
10. Each stage has a finite deterministic exit gate before broader capability is
    added.
11. Operation, working-knowledge, and foundation/model changes remain separate
    clocks with recorded generations and explicit adoption.
12. Material prose claims remain typed, source-linked, and visibly complete,
    partial, disputed, stale, absent, or intentionally unverified.
13. Projection and memory derivations remain rebuildable and ineligible when
    their permitted source lineage disappears.
14. Account, workspace, principal, source, memory, artifact, and capability
    identities are revalidated by their owning services.
15. Operational pause, quota, backup, restore, redaction, and alerting controls
    are product gates, not optional post-launch hardening.
16. Every derived intention links to an accepted purpose, commitment, owner, or
    policy and remains separate from action authority.
17. Workspace recurrence, simulation, consolidation, and subconscious wakeups
    are bounded, attributable, interruptible, and incapable of widening scope.
18. World, belief, and self models remain derived and revisable; prediction error
    cannot be hidden by a fluent result.
19. Functional salience guides selection but does not become emotion, truth,
    permission, or an unreviewed reward objective.
20. Persistent identity and autobiographical continuity never depend on one
    model, provider session, process, host, or execution body.

## Stage 0 — Freeze vocabulary and deterministic corpus

### Goal

Create one reviewable contract proposal and a provider-independent corpus before
source implementation begins.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVAG-001 | Define the first run-state vocabulary. | Closed status, plan, typed claim, decision, blocker, verification, budget, and artifact kinds. |
| WVAG-002 | Define the first event vocabulary. | Run-created, plan-updated, source-observed, context-compiled, challenge-proposed, challenge-merged, checkpoint-created, and terminal events. |
| WVAG-003 | Define identity and revision rules. | Supplied deterministic identities for fixtures, nonzero monotonic run revision, idempotency identity, and no civil-time ordering dependency. |
| WVAG-004 | Define first context and challenge records. | Bounded candidates, manifest items, omissions, context diff, skeptic result, and merge outcome. |
| WVAG-005 | Create accepted and rejected corpus cases. | Valid, boundary, truncated, oversized, duplicate, reordered, stale-revision, replay, invalid-merge, and protected-state cases. |
| WVAG-006 | Define deterministic output identities. | Exact canonical bytes and SHA-256 for final state, manifest, checkpoint, and handoff fixtures. |
| WVAG-007 | Define the three clocks. | Operation, working-knowledge/configuration, and foundation/model generation records plus explicit adoption and revisit rules. |
| WVAG-008 | Freeze attention lifecycle terms. | Pinned, active, referenced, dormant, summarized, excluded, superseded, and expired states with valid transitions. |
| WVAG-009 | Define the first claim and evidence vocabulary. | Epistemic kind, support state, source requirements, supporting/contradicting/missing evidence, dependent artifacts, and owner. |
| WVAG-010 | Define human-facing cognitive terms. | Exact observable meanings for remembered, noticed, forgot, doubted, learned, and changed its mind without subjective claims. |

### Exit gate

- Every record has exact bounds and a malformed-input matrix.
- The corpus requires no model API, database, filesystem, clock, entropy, or
  operating-system process service.
- A reviewer can distinguish run truth, context projection, cognitive proposal,
  and canonical evidence without relying on product-specific DTOs.
- A reviewer can distinguish model knowledge, run evidence, live state, durable
  memory, active context, and each of the three change clocks.
- Persistent-self, multi-episode intention, world/self-model, salience,
  simulation, consolidation, and wake terms remain architecture vocabulary; the
  first decision does not freeze their serialized forms.
- A numbered decision is ready to accept only this first bounded contract.

## Stage 1 — Portable run-state kernel

### Goal

Implement a capability-free Windvale library that creates one run projection and
folds a bounded ordered event sequence under exact revision and replay rules.

### Candidate ownership

Create `Libraries/Agent/` only with the first accepted implementation. Initial
modules should remain focused—for example run records, event admission, and run
projection—rather than one broad `Agent-Utils` file. Exact names wait for the
contract decision.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVAG-101 | Admit one run root. | Exact version, objective, constraints, policy generation, budgets, and initial revision. |
| WVAG-102 | Admit one ordered event. | Checked identity, kind, expected revision, resulting revision, bounded payload, and trailing-data rejection. |
| WVAG-103 | Fold run events into immutable state. | Deterministic plan, typed claim ledger, decisions, blockers, verification, budget, and terminal status. |
| WVAG-104 | Enforce protected state. | Events cannot remove the objective, current user refinement, policy, failed verification, or open unknown-effect blocker through an ordinary context command. |
| WVAG-105 | Enforce idempotent replay. | Exact repeated identity returns the recorded transition; changed payload returns conflict. |
| WVAG-106 | Publish canonical inspection output. | Bounded human-readable report plus exact machine record. |
| WVAG-107 | Bind effective generations. | Agent-definition, policy, model profile, provider, tool catalog, memory mount, source, and protocol generations plus an explicit revisit set after adoption. |

### Exit gate

- The same fixture produces byte-identical state and report bytes on Windows and
  Linux.
- Every rejected event leaves the preceding state unchanged.
- Event replay cannot increment a budget, revision, model-call count, or other
  effect twice.
- A working- or foundation-clock transition cannot rewrite prior run evidence or
  silently carry assumptions forward without a revisit result.
- No hidden mutable global state or capability call participates.

## Stage 2 — Context and digital-subconscious kernel

### Goal

Compile bounded attention, accept one scripted skeptical proposal, and merge it
through deterministic policy.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVAG-201 | Define bounded context candidates. | Source identity/revision, trust, epistemic kind, layer, attention state, priority, estimated size, protection, and compact content/reference. |
| WVAG-202 | Implement deterministic eligibility. | Permission, mount, lineage, freshness, protection, expiry, and source-generation checks. |
| WVAG-203 | Implement token/size-budget planning v1. | Conservative abstract units or bounded byte/character accounting; exact model tokenizer is not required. |
| WVAG-204 | Compile one context manifest. | Stable ordering, output reserve, selected items, omissions, rejection codes, and exact fingerprints. |
| WVAG-205 | Produce one context diff. | Added, removed, replaced, reactivated, protected, and stale transitions with reasons. |
| WVAG-206 | Admit a skeptic capsule and result. | One objective, non-goals, exact sources, constraints, no tools, output kind, budget, and expiry evidence. |
| WVAG-207 | Merge a challenge. | Unsupported or contradictory claim becomes challenged without rewriting source evidence or granting authority. |
| WVAG-208 | Reject unsafe context and merge requests. | Protected removal, stale revision, forged source, cross-run identity, over-budget output, and hidden-permission proposals fail closed. |
| WVAG-209 | Bind challenge evidence to claims. | Supporting, contradicting, and missing-evidence references update support state and dependent verification without converting an allegation or inference into fact. |

### Exit gate

- A bounded fixture omits low-value evidence while retaining every protected
  objective, constraint, blocker, and failed verification.
- The manifest explains every material inclusion and omission.
- Context diffs explain every transition among pinned, active, referenced,
  dormant, summarized, excluded, superseded, and expired.
- A valid skeptic result challenges one claim; an invalid result changes nothing.
- The next compiled context reflects the accepted challenge deterministically.

## Stage 3 — Scripted hosted processor bridge

### Goal

Prove the transition between Windvale portable semantics and a disposable host
processor without introducing a production model dependency.

The first bridge may be a repository test host or an E-Worker adapter. The host
owns scheduling and supplies one exact scripted response. Windvale owns request
admission, context/capsule construction, response validation, merge, and final
state.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVAG-301 | Define a bounded processor request/response protocol. | Strict versioned bytes with role, capsule digest, output kind, maximum bytes, result status, and response fingerprint. |
| WVAG-302 | Add a deterministic scripted host. | Fixed success, invalid output, timeout, unavailable, cancellation, and provider-loss scenarios. |
| WVAG-303 | Validate placement evidence. | Selected processor kind/profile and permitted source sensitivity are recorded before dispatch. |
| WVAG-304 | Externalize processor transcripts. | The run retains bounded result evidence and references, not provider-private reasoning or unlimited raw traces. |
| WVAG-305 | Qualify resume without provider session state. | Replaying canonical response evidence after host restart reaches the same state without a second processor invocation. |
| WVAG-306 | Record deterministic routing evidence. | Processor/model version, capability profile, provider generation, placement, permitted data class, limits, usage, and capability/privacy/risk/quality/latency/cost reasons. |
| WVAG-307 | Expose fallback and degradation. | Failure fallback, truncation, repair, route change, or reduced verifier status emits a bounded warning and cannot impersonate the requested route. |

### Exit gate

- Scripted processor loss cannot corrupt or advance run state.
- Invalid, mismatched, oversized, late, or cross-run responses are rejected.
- Exact response replay does not repeat a charge or merge.
- A provider substitution changes execution evidence without changing run or
  durable agent identity.
- Windows and Linux pass the same request/response corpus.

This stage does not accept a public `ai.*` capability. It proves the semantic
seam that a later capability must implement.

## Stage 4 — Durable run continuity

### Goal

Move accepted run events, snapshots, manifests, and checkpoints into a durable
single-writer owner and recover after interruption.

### Minimum database/store contract

The first durable owner needs only:

- create one run root if absent;
- append one bounded event when the current run revision equals the expected
  revision;
- return the already-recorded result for an exact idempotency identity;
- reject a reused identity with changed normalized content;
- read ordered events by run identity and sequence range;
- publish a compact snapshot bound to one exact event revision;
- retain immutable context manifests and checkpoint references;
- recover an unpublished tail without guessing; and
- keep large processor/tool evidence in a separately bounded byte owner.

It does not initially require SQL, arbitrary queries, multiple simultaneous
writers per run, distributed consensus, a network database server, embeddings,
or a graph store.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVAG-401 | Specify the durable run-store interface. | Exact operations, widths, bounds, failure results, expected-revision behavior, and durability class. |
| WVAG-402 | Map the interface onto the selected database/storage owner. | One single-writer run profile with no ambient paths or database-internal bytes in application code. |
| WVAG-403 | Persist event and idempotency evidence atomically. | Accepted event and replay identity cannot disagree after restart. |
| WVAG-404 | Publish compact snapshots and checkpoints. | Snapshot generation and source event revision are inseparable. |
| WVAG-405 | Inject interruption at every publication boundary. | Recovery selects either the complete prior or complete next revision. |
| WVAG-406 | Reconcile resume. | Source/provider generations, budgets, policy, and pending effects are rechecked before progress. |
| WVAG-407 | Prove backup and restore of the first durable profile. | Run events, snapshots, manifests, checkpoints, idempotency evidence, and large-body references restore without changing identity or replaying work. |

### Exit gate

- A 50-operation scripted run resumes after every injected interruption.
- Reconstructed state and next context equal a clean execution byte for byte.
- Concurrent or stale append attempts cannot both advance one run revision.
- An uncertain durable mutation is reopened and classified rather than replayed
  blindly.
- Backup and restore preserve exact run reconstruction and every referenced body
  required by the next context.

## Stage 5 — Retrieval and memory doors

### Goal

Add source-linked perception and reviewed cross-run memory without treating
retrieved or remembered text as canonical truth.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVAG-501 | Define canonical source references. | Owner, identity, revision, range, fingerprint, trust, sensitivity, and freshness. |
| WVAG-502 | Implement direct and structural retrieval first. | Known identities and native owner structure precede broad semantic search. |
| WVAG-503 | Add exact/lexical candidate retrieval. | Bounded results, ordering, continuation, projection generation, and stale reporting. |
| WVAG-504 | Define memory records and lifecycle. | Epistemic kind, source lineage, scope, confidence, validity, review, expiry, supersession, and revision. |
| WVAG-505 | Implement propose/correct/archive/reject/mark-stale. | Optimistic concurrency and audit evidence; no silent rewrite. |
| WVAG-506 | Implement memory mounts. | Inherit, clean-room, selective, and compare profiles with derived-lineage exclusion. |
| WVAG-507 | Add archivist classification. | Structured proposal only; the memory owner and review policy decide activation. |
| WVAG-508 | Define bounded evidence bundles. | Query/scope, mounts/exclusions, source and projection generations, strategies, anchors, contradiction, omission, truncation, freshness, and sufficiency. |
| WVAG-509 | Define projection lifecycle and invalidation. | Current, partial, building, stale, failed, and unavailable states plus source-change invalidation and deterministic rebuild evidence. |
| WVAG-510 | Connect claims to evidence and artifacts. | Source deletion or revision invalidates dependent claims, decisions, artifact sections, and verification until refreshed. |
| WVAG-511 | Define retention and deletion interaction. | Archive, expiry, deletion, legal hold, protected history, sole-source invalidation, and inseparable mixed-derivation rebuild/exclusion. |

### Exit gate

- Known source identities bypass broad search.
- Stale projections never identify themselves as current.
- Agent-created shared memories begin as proposed.
- A clean-room branch excludes selected attempts and every projection derived
  solely from them.
- Correcting a memory retains the prior record and source lineage.
- Every material retrieval reports freshness, contradiction, truncation, and a
  closed sufficiency state.
- Removing or forbidding a source cannot leave its sole-source claims, memories,
  or projections eligible.

Embeddings and typed relationship indexes remain later, benchmark-selected
extensions.

## Stage 6 — Governed hands

### Goal

Allow one reversible, bounded mutation through its canonical owner without
giving the foreground or subconscious plane ambient authority.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVAG-601 | Define the action envelope. | Normalized effect, objective evidence, exact target/revision, disclosure, cost, reversibility, risk, and verification. |
| WVAG-602 | Define permission receipt and capability lease. | Exact decision binding, principal, operation, rights, limits, expiry, revocation, and single-use behavior. |
| WVAG-603 | Register one capability owner. | Preview, checkpoint, expected-revision mutation, observed result, and read-back verification. |
| WVAG-604 | Add isolated skeptical review. | Read-only challenge that cannot approve or mint authority. |
| WVAG-605 | Fence execution and replay. | Exact completed retry returns recorded outcome; unknown effect is not dispatched again. |
| WVAG-606 | Connect artifact and verification ledgers. | Failed or unknown verification prevents a completed claim. |
| WVAG-607 | Persist the append-only action evidence chain. | Envelope, review decision, receipt, lease issue/revocation/expiry, execution-start fence, observed outcome, and verification reconstruct effective status. |
| WVAG-608 | Classify consequence treatment. | Read/calculation, scoped reversible mutation, exact-approval consequential action, and deny classes with deterministic data/scope/cost rules. |

### Exit gate

- The model can propose but cannot construct a valid lease.
- Approval applies only to the exact normalized previewed action.
- Stale source revision, expired lease, changed target, or forged receipt stops
  before mutation.
- An interrupted effect becomes a truthful known result or unknown blocker,
  never an automatic retry.
- A material envelope change creates a new proposal and cannot reuse the prior
  approval or lease.

External communication, payment, permission changes, live broad deletion, and
other consequential actions remain denied or separately approved future
profiles.

## Stage 7 — Product, operational controls, and Windvale OS placement

### Goal

Expose understandable run/influence views on Windows and Linux, then qualify the
same semantic core through Windvale OS mechanisms when OS-1 foundations are
ready.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVAG-701 | Package one hosted agent application. | Exact package, lock, bundle, agent-definition, approval, launch, provider, and capability closure. |
| WVAG-702 | Add compact run and influence views. | Task, phase, sources, memory, tools, approvals, artifacts, blockers, caveats, and advanced lineage. |
| WVAG-703 | Add remove-influence-and-regenerate branching. | New branch, lineage exclusion, fresh retrieval, and retained prior history. |
| WVAG-704 | Bind explicit resource ceilings. | Model, tool, context, storage, output, process, memory, and teardown budgets. |
| WVAG-705 | Define Windvale OS service composition. | Coordinator, model adapter, store, retrieval, and tools placed in explicit resource domains with bounded IPC. |
| WVAG-706 | Qualify provider loss and supervision. | Peer loss, cancellation, restart generation, rebind, checkpoint resume, and terminal teardown. |
| WVAG-707 | Add focused and advanced product projections. | One task-specific application and one advanced inspector consume the same public contracts without owning hidden state. |
| WVAG-708 | Enforce account, workspace, and data placement. | Owner-bound identifier revalidation, minimum provider disclosure, secret exclusion, and safe telemetry correlation. |
| WVAG-709 | Add independent emergency controls. | Executor, mutation, durable-memory, connected-source, and consequential-action gates plus provider/tool-group/account/workspace pause. |
| WVAG-710 | Bind quotas, retention, and alerts. | Rate, calls, concurrency, storage, cost, redaction, and retention ceilings with stuck-run, invalid-output, unknown-effect, recovery, denial, and projection-lag alerts. |
| WVAG-711 | Qualify transparent human vocabulary. | Product text maps remembered, noticed, forgot, doubted, learned, and changed its mind to owned evidence and never claims a hidden human self. |
| WVAG-712 | Run the two book-completeness workflows. | Verified software change and proposal-draft scenarios cover canonical evidence, claims, memory, influence, action approval, and uncertain-effect reconciliation. |

### Exit gate

- Mainstream users can understand what the agent is doing without reading raw
  events.
- Advanced inspection can trace a material claim to current source evidence.
- Focused and advanced views agree because both project the same owned state.
- Cross-account/workspace identifiers, forbidden provider placement, paused
  capability groups, exceeded quotas, and incomplete restore fail closed while
  preserving an inspectable run.
- Windows and Linux hosted products use the same canonical agent semantics.
- A Windvale OS claim is made only after dynamic launch, supervision, clocks,
  resource domains, provider isolation, and durable storage pass their own
  qualification gates.

Stage 7 qualifies the first governed product and host composition. It does not
yet qualify persistent functional-mind behavior across many episodes.

## Stage 8 — Persistent agent self and intention hierarchy

### Goal

Place a long-lived owned self above individual runs and preserve bounded
intentions across episode completion, interruption, model replacement, and host
restart.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVAG-801 | Define the persistent agent-self record. | Identity, accepted purpose/values, commitments, policy references, current generations, skill/calibration evidence, autobiographical root, and no credentials or ambient authority. |
| WVAG-802 | Bind episodes to the self. | Exact episode membership, primary objective, scope, terminal result, and no cross-episode influence without memory/source doors. |
| WVAG-803 | Define prospective intentions. | Parent purpose/commitment, owner, scope, priority evidence, eligible interval, wake conditions, budget, authority requirements, satisfaction, cancellation, and expiry. |
| WVAG-804 | Arbitrate multiple intentions. | Deterministic eligibility, selection reason, conflict, fairness, starvation, preemption, satisfaction, and cancellation evidence. |
| WVAG-805 | Build the autobiographical index. | Compact episode, commitment, correction, skill, and change links that never replace canonical episode events. |
| WVAG-806 | Define the first functional self-model. | Current body/providers, senses, tools, capabilities, failures, calibration, load, blocks, unknowns, and help conditions. |
| WVAG-807 | Qualify identity continuity. | Provider/model/process/host replacement preserves self and intention identity while revalidating every generation and grant. |

### Exit gate

- One self owns at least three separately completed or interrupted episodes
  without copying their transcripts into the self record.
- Two simultaneously eligible intentions produce one explainable foreground
  selection and retain deterministic fairness/cancellation evidence.
- Revoking a parent purpose or commitment cancels every solely derived intention.
- The self-model cannot grant a capability or improve its skill rating without
  measured evidence and owner admission.
- Replacing the model and process preserves identity and autobiographical links
  without claiming personal memory by the replacement processor.

## Stage 9 — Recurrent workspace, world model, salience, and simulation

### Goal

Turn one-shot context compilation into a bounded recurrent cognitive cycle that
integrates perception, intention, memory, prediction, doubt, simulation, and
foreground selection.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVAG-901 | Define workspace-cycle records. | Self, intention, episode, wake, prior-cycle, candidate set, selected foreground, broadcast operations, merge results, disposition, and exact budgets. |
| WVAG-902 | Collect workspace candidates. | Protected state, intention, perception, memory, prediction error, tool result, doubt, and operation candidates with bounded provenance. |
| WVAG-903 | Define functional salience v1. | Closed relevance, urgency, novelty, uncertainty/information gain, progress, risk, social/commitment, blocker, and prediction-error evidence. |
| WVAG-904 | Define bounded world and belief models. | Nominal entities, states, relationships, chronology, hypotheses, expected observations, support/calibration, and revision outcomes. |
| WVAG-905 | Reconcile prediction error. | Expected versus observed evidence can confirm, challenge, split, supersede, or leave beliefs and plans unresolved. |
| WVAG-906 | Calibrate the self-model. | Task-class success/failure and degraded-capability evidence update measured reliability without model self-assertion. |
| WVAG-907 | Add counterfactual simulation branches. | Starting generations, changed assumption, possible worlds, disconfirming observations, no action authority, and explicit merge proposal. |
| WVAG-908 | Run bounded recurrence. | Maximum cycles, calls, operations, elapsed work, bytes, cost, stop reasons, and no hidden internal monologue. |
| WVAG-909 | Define a scoped social-model profile. | Observed roles/statements, accepted preferences/commitments/consent/authority, knowledge evidence, inferred goals or reactions as hypotheses, relationship scope, and privacy. |

### Exit gate

- Perception, memory, doubt, and prediction error compete for a foreground under
  fixed bounds while protected purpose, policy, and authority remain pinned.
- A changed source revises one belief, invalidates a dependent plan, and changes
  the next workspace with exact lineage.
- A simulation improves or rejects a plan without mutating canonical state or
  acquiring a lease.
- An unexpected action result changes prediction evidence and self-model
  calibration rather than being summarized away.
- An inferred motive, preference, emotion, or knowledge state remains a scoped
  hypothesis and cannot replace a person's current statement or authority record.
- Replaying the same admitted candidates and processor results produces the same
  workspace cycles and terminal disposition on Windows and Linux.

## Stage 10 — Consolidation and event-driven subconscious continuity

### Goal

Let the persistent mind learn across episodes and resume bounded cognition on
authorized events without a continuously running model or secret goals.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVAG-1001 | Separate functional memory kinds. | Working, episodic, semantic, procedural, prospective, and autobiographical records with independent ownership and lifecycle. |
| WVAG-1002 | Add bounded consolidation. | Episode-to-semantic/procedural/prospective/autobiographical proposals with source lineage, confidence, review class, and exact budget. |
| WVAG-1003 | Add reconsolidation. | Reopened memories can be confirmed, narrowed, corrected, split, superseded, or rejected while preserving history. |
| WVAG-1004 | Define wake records and sources. | User command, source change, monotonic/calendar condition, provider recovery, unresolved effect, intention condition, and approved schedule. |
| WVAG-1005 | Admit one wake safely. | Generation validation, duplicate coalescing, one executor generation, wake budget, cancellation, and reason evidence. |
| WVAG-1006 | Enforce fairness and rate bounds. | Maximum wake rate, intention fairness, starvation prevention, backpressure, missed/degraded wake, and exhaustion behavior. |
| WVAG-1007 | Return to dormancy. | Visible response, approval wait, block/failure, checkpoint, satisfied/cancelled intention, or bounded next condition with complete teardown. |
| WVAG-1008 | Qualify the functional-mind corpus. | Multi-episode self, intention conflict, recurrent workspace, prediction revision, simulation, consolidation, wake, and dormancy evidence. |

### Exit gate

- Safe private consolidation reduces repeated work while shared or consequential
  knowledge remains proposed until its owner admits it.
- Contradictory evidence reconsolidates a memory without erasing the prior
  episode or action that relied on it.
- Duplicate, stale, unauthorized, over-rate, and cross-workspace wake events
  cannot create extra model calls, operations, intentions, or actions.
- One prospective intention wakes, performs bounded cognition, records its
  result, and returns dormant with zero leaked resources.
- The same persistent self survives the complete functional-mind corpus without
  depending on provider-private state or claiming consciousness.

## Dependency requests by owner

These requests identify likely pressure. They are not permission to add breadth
before the consuming stage reaches it.

### Compiler and language

Stages 0 through 3 should use current source semantics: records, enums, payload
variants, exhaustive match, bounded sequences/builders, checked arithmetic,
bytes, text, and explicit capabilities. Do not add arbitrary JSON, reflection,
classes, inheritance, exceptions, unbounded collections, or general generics for
the first agent kernel.

Later measured requests are:

1. typed rights-limited capability references so a source value can identify one
   approved provider instance without exposing a host handle;
2. scoped ownership only for values whose caller owns an explicit close
   operation;
3. one bounded associative collection when context, idempotency, or memory
   indexing demonstrates that ordered sequences are insufficient; and
4. structured tasks, cancellation, channels, and synchronization only when a
   hosted or OS coordinator must own concurrent operations directly.

Each addition requires the Windvale-owned compiler, source specification, WIR
and WVB where affected, independent verifier, runtime, native targets, editor,
malformed corpus, and explicit support matrix. A host-driven sequential adapter
must not be used as evidence that structured concurrency exists in the language.

Stages 8 through 10 can begin with bounded sequences, explicit priority classes,
and one host-driven cognitive cycle at a time. A general map, priority queue,
async syntax, or task runtime is justified only when the measured self,
intention, workspace, world-model, or wake corpus cannot remain clear and bounded
without it. Functional-mind vocabulary does not create a separate source
dialect.

### Database and durable storage

The first consumer profile is an append-only single-writer run ledger with
expected-revision append, exact idempotency lookup, ordered replay, and compact
snapshot/checkpoint publication. Large evidence stays outside event rows.

The later functional-mind profile adds one persistent self root, episode
membership, prospective intentions and wake conditions, autobiographical links,
workspace cycles, predictions/errors, and working, episodic, semantic,
procedural, and prospective memory records. World, belief, self-model, salience,
and simulation indexes remain derived from canonical admitted evidence and must
be rebuildable. One self may own many episodes, but each self or episode
transition still has one explicit writer/revision owner unless a later measured
concurrency contract says otherwise.

Later memory and retrieval pressure may require bounded indexes over run,
sequence, operation, idempotency identity, source identity/revision, scope,
claim support, dependent artifact, projection generation, action identity,
status, review state, and expiry. These are selected only by measured queries.
The first durable product profile requires backup/restore of its owned events,
checkpoints, idempotency evidence, and large-body references. Multiple writers,
group commit, retention automation, compaction, and reclamation enter when the
workload reaches them; they are not hidden assumptions of the first semantic
kernel.

### Filesystem and object storage

Agent semantics must not depend on native paths. Immutable definitions,
fixtures, prompts, policies, artifacts, and result bodies should use package
resources or rights-limited object capabilities. Mutable run state should use a
database or pre-opened durable object with exact partial/indeterminate completion
semantics.

The first hosted profile may bind separate objects for run state and large
evidence. It must not infer atomic replacement, directory durability, watching,
or append semantics from the host filesystem. A new operation receives a
separate interface only when its exact guarantee is required.

Backup manifests must bind every large-body or artifact reference needed to
reconstruct the admitted run revision. Restore must reject missing, changed,
cross-workspace, or unauthorized bodies rather than rebuilding a plausible but
different context.

### Runtime and native providers

The current native capability-provider table accepts scalar and descriptor
signatures including `bytes`. A first host bridge may therefore carry one
strictly validated protocol envelope as bytes. The portable protocol library,
not the provider implementation, owns its meaning and limits.

Nominal or collection-shaped provider signatures, typed capability values,
streaming model output, cancellation, and concurrent calls require later
versions. Provider state remains a nonzero rights-limited execution-owned object
with a lifetime containing every call. Provider identity, target addresses,
credentials, and conversation handles never enter WVB or durable run evidence.

The public provider seam eventually reports stable model/profile identity,
context and output limits, data-placement eligibility, usage or an explicit
estimate class, cancellation/deadline support, route choice, fallback, repair,
truncation, and provider-generation loss. Adapter-native SDK objects remain
outside portable state and checkpoints.

The functional-mind host later needs a monotonic wake source, optional qualified
calendar mapping, bounded source/provider change subscriptions, one-generation
executor admission, cancellation, and a durable wake/result handoff. A first
test host may supply scripted events sequentially; event-driven behavior does not
require a continuously resident model.

### Package, approval, and launch

An agent definition is immutable configuration, not a package manifest and not
an authority grant. The selected application/package graph declares the complete
capability requirements. Approval accepts that closure. A launch record binds
exact rights-reduced providers. An operation-specific lease later authorizes one
effect inside that launch.

The first hosted agent package should use the active installation generation and
command-dispatch contracts rather than inventing another update or activation
system. Agent memory and user data remain separately owned from immutable package
content and survive package removal according to explicit policy.

The persistent self, autobiographical index, intentions, memories, and episode
history belong to user/workspace data ownership rather than the immutable agent
package. Updating or rolling back the package changes the available agent
definition/body generation and triggers compatibility/revisit checks; it does
not silently fork or erase the self.

The hosting composition must gate executor start, mutation, durable-memory
activation, connected sources, and consequential actions independently. Package
selection and launch approval do not replace provider-, tool-group-, account-,
or workspace-level emergency pause controls.

### Windvale OS

The first portable and hosted stages do not wait for Windvale OS. The OS profile
later requires:

- one dynamic clean launch transaction;
- explicit resource-domain membership and budgets;
- bounded IPC and provider-generation loss;
- monotonic timers, cancellation, and teardown;
- bounded event subscription, wake admission, coalescing, and fairness;
- durable storage and checkpoint access;
- secure model transport or a separately qualified local-model provider; and
- supervision that does not replay an uncertain mutation after restart.

The kernel supplies mechanisms. Agent identity, values, intentions, workspace,
salience, world/belief/self models, prompts, context, memory, consolidation,
retrieval, model routing, simulation, and approval policy remain in user-space
libraries and isolated services.

## Verification strategy

### Contract tests

Every reader and state transition receives valid, boundary, empty, truncated,
oversized, trailing, duplicate, reordered, cross-run, stale-revision, replay,
overflow, and malicious-input cases. Structural assertions accompany exact
golden bytes so failures remain diagnosable.

### Deterministic processor qualification

A scripted provider supplies named scenarios and expected outcomes. The same
corpus covers success, invalid schema/output kind, over-budget response, late
response, cancellation, provider loss, conflicting evidence, unsupported claim,
unsafe action request, and recovery. Production model quality is never required
for deterministic conformance.

### Source, claim, and coherence qualification

Fixed cases cover direct identity, structure, lexical search, stale and partial
projections, contradictory passages, missing sources, truncation, and all closed
sufficiency states. Removing or revising a source must invalidate every
sole-source memory, claim, decision, artifact dependency, and verification
result. Domain cases prove that an allegation, observation, preference,
inference, compiler-derived relationship, and approved fact do not collapse into
one authority state.

### Differential evidence

Where E-Worker already implements an equivalent context, operation, or run
contract, an adapter may use common scenario intent as a differential oracle.
Windvale keeps its own canonical record encoding and authority semantics; it does
not copy product DTOs or make E-Worker execution the language definition.

### Cross-host evidence

Changes to serialized agent records, context selection, run-state folding,
provider protocols, or durable storage require byte-level Windows/Linux
comparison before cross-host conformance is claimed. Windvale OS evidence is a
separate later provider result over the same portable fixtures.

### Long-run and recovery evidence

The durability gate should include at least one 50-operation run with context
pressure, repeated processor results, challenged facts, checkpoint/resume, and
interruption at every publication boundary. The clean and recovered terminal
state, manifest, and handoff must match exactly.

Backup/restore, provider substitution, clean-room regeneration, action recovery,
quota exhaustion, and each emergency pause are injected independently. Restore
must preserve exact evidence identities; a pause or exhausted quota must fail
closed without making the run disappear or reporting completion.

### Book-completeness workflows

The software workflow uses repository rules, exact source and compiler evidence,
one bounded artifact change, projection refresh, verification, and truthful
handoff. The proposal workflow uses approved service and price records,
client-scoped memory, scheduling evidence, a claim ledger, skeptical review,
document verification, an influence summary, exact send approval, and unknown-
send reconciliation. The first product proof may stop before sending; a later
governed-action proof must show that proposal and authority remain separate.

### Functional-mind qualification

One deterministic corpus spans several episodes under one self and covers:

- simultaneous intention eligibility, selection, preemption, fairness,
  cancellation, satisfaction, expiry, and revoked-parent propagation;
- recurrent workspace competition among perception, memory, doubt, prediction
  error, and protected state;
- world/belief revision after source change and unexpected action outcome;
- self-model calibration, degraded capability, refusal, and help-seeking;
- social-model separation of observed statements, accepted authority/consent,
  and inferred motives or knowledge;
- counterfactual branches with zero mutation authority;
- episodic, semantic, procedural, prospective, and autobiographical
  consolidation plus contradictory reconsolidation; and
- authorized, duplicate, stale, missed, over-rate, cross-workspace, recovered,
  and cancelled wakes followed by complete dormancy teardown.

Tests assert exact state, lineage, budgets, selection reasons, prediction error,
model-call/tool-call counts, and zero duplicate effects. User-facing reports say
what the agent remembered, believed, intended, learned, or changed only through
the precise functional vocabulary; they never make a consciousness claim.

## Controlled scope and parallel work

Compiler, database, package, storage, and OS agents may advance their existing
work independently. An agent-runtime work item may consume an accepted contract;
it must not create a second owner, weaken a current gate, or describe a proposed
dependency as implemented.

Safe early parallel work includes:

- drafting Stage 0 record shapes and corpus cases;
- measuring whether current bounded sequences can express the first projection;
- mapping the Stage 4 ledger profile onto current database invariants without
  changing them;
- identifying a strict byte-envelope seam in the current provider table;
- designing package capability closure for a future read-only hosted proof;
- mapping both book-completeness workflows to existing canonical owners without
  adding domain facts to the portable agent core; and
- drafting persistent-self, intention, workspace-cycle, belief, simulation,
  consolidation, and wake corpora without adding empty implementation owners.

Memory activation, consequential actions, peer agents, scheduling, and OS
hosting wait for their earlier ownership and recovery gates.

## Deferred breadth

The following remain deferred until the single-agent core qualifies:

- lead-worker delegation and peer communication;
- recurring, scheduled, or unattended external action;
- embedding retrieval and vector indexes;
- a general knowledge graph;
- automatic shared-memory activation;
- unrestricted browsing, terminal, filesystem, or network tools;
- local-model installation and lifecycle management;
- high-risk or regulated domain profiles; and
- provider-specific hidden state as a continuity mechanism.

## Immediate next documentation and decision work

1. Review the proposed architecture terminology, especially one agent identity,
   two cooperating planes, and three clocks.
2. Select the Stage 0 software corpus scenario and exact maximum sizes.
3. Draft the first closed run/event/context/claim/challenge record
   specification, including attention and support states.
4. Define which generations a foundation or working-knowledge change records
   and which assumptions enter its revisit set.
5. Check the proposed fields against current Seed record/variant/sequence limits.
6. Map the Stage 4 store operations to the database team's completed contract
   without pre-empting its current completion claim.
7. Record the later proposal workflow as a differential book-completeness
   fixture without adding business semantics to the portable kernel.
8. Prepare one numbered decision accepting only Stages 0 through 2 as the first
   implementation boundary.
9. Draft a separate later decision map for Stages 8 through 10; do not place
   persistent-self, world-model, scheduler, or wake formats into the first
   single-episode decision merely to make the document appear complete.

Implementation begins after those records are reviewable. The first code change
should create only the source owner and tests needed by the accepted portable
kernel; it should not add empty provider, memory, UI, or OS scaffolding.
