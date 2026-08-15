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

The target is one durable user-facing agent with two cooperating execution
planes:

- a foreground reasoning plane that presents one coherent voice; and
- a digital-subconscious coordination plane that owns direction, attention,
  evidence selection, memory proposals, doubt, authority preparation, and
  continuity.

The digital subconscious remains a support plane of the same agent run. The
first product does not create a second autonomous agent identity.

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
| Durable database | Focused single-writer storage/tree/recovery candidates now update an existing depth-three generation and can grow one bounded path to depth four | Suitable future owner for run state and memory, but not yet a general run ledger or indexed memory service. |
| Dynamic process launch, supervision, and general resource domains | Designed and partially proven in fixed OS slices | Not required for the first Windows/Linux semantic kernel; required before a Windvale OS agent-host claim. |
| Monotonic clocks, structured tasks, cancellation, channels, and synchronization | Proposed or absent as general source/runtime contracts | Host-driven one-transition-at-a-time execution is the first route. |
| Networking, secure streams, identity, and production key custody | Future architecture | Not required by the deterministic kernel; required for a remote production model provider. |

## Delivery invariants

Every stage preserves these rules:

1. One run has one durable objective and one current revision.
2. Model output, tool output, retrieved text, and imported memory are untrusted
   until the owning deterministic boundary validates them.
3. Canonical sources and state owners remain authoritative over summaries and
   model confidence.
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

## Stage 0 — Freeze vocabulary and deterministic corpus

### Goal

Create one reviewable contract proposal and a provider-independent corpus before
source implementation begins.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVAG-001 | Define the first run-state vocabulary. | Closed status, plan, fact, decision, blocker, verification, budget, and artifact kinds. |
| WVAG-002 | Define the first event vocabulary. | Run-created, plan-updated, source-observed, context-compiled, challenge-proposed, challenge-merged, checkpoint-created, and terminal events. |
| WVAG-003 | Define identity and revision rules. | Supplied deterministic identities for fixtures, nonzero monotonic run revision, idempotency identity, and no civil-time ordering dependency. |
| WVAG-004 | Define first context and challenge records. | Bounded candidates, manifest items, omissions, context diff, skeptic result, and merge outcome. |
| WVAG-005 | Create accepted and rejected corpus cases. | Valid, boundary, truncated, oversized, duplicate, reordered, stale-revision, replay, invalid-merge, and protected-state cases. |
| WVAG-006 | Define deterministic output identities. | Exact canonical bytes and SHA-256 for final state, manifest, checkpoint, and handoff fixtures. |

### Exit gate

- Every record has exact bounds and a malformed-input matrix.
- The corpus requires no model API, database, filesystem, clock, entropy, or
  operating-system process service.
- A reviewer can distinguish run truth, context projection, cognitive proposal,
  and canonical evidence without relying on product-specific DTOs.
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
| WVAG-103 | Fold run events into immutable state. | Deterministic plan, facts, decisions, blockers, verification, budget, and terminal status. |
| WVAG-104 | Enforce protected state. | Events cannot remove the objective, current user refinement, policy, failed verification, or open unknown-effect blocker through an ordinary context command. |
| WVAG-105 | Enforce idempotent replay. | Exact repeated identity returns the recorded transition; changed payload returns conflict. |
| WVAG-106 | Publish canonical inspection output. | Bounded human-readable report plus exact machine record. |

### Exit gate

- The same fixture produces byte-identical state and report bytes on Windows and
  Linux.
- Every rejected event leaves the preceding state unchanged.
- Event replay cannot increment a budget, revision, model-call count, or other
  effect twice.
- No hidden mutable global state or capability call participates.

## Stage 2 — Context and digital-subconscious kernel

### Goal

Compile bounded attention, accept one scripted skeptical proposal, and merge it
through deterministic policy.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVAG-201 | Define bounded context candidates. | Source identity/revision, trust, epistemic kind, layer, priority, estimated size, protection, and compact content/reference. |
| WVAG-202 | Implement deterministic eligibility. | Permission, mount, lineage, freshness, protection, expiry, and source-generation checks. |
| WVAG-203 | Implement token/size-budget planning v1. | Conservative abstract units or bounded byte/character accounting; exact model tokenizer is not required. |
| WVAG-204 | Compile one context manifest. | Stable ordering, output reserve, selected items, omissions, rejection codes, and exact fingerprints. |
| WVAG-205 | Produce one context diff. | Added, removed, replaced, reactivated, protected, and stale transitions with reasons. |
| WVAG-206 | Admit a skeptic capsule and result. | One objective, non-goals, exact sources, constraints, no tools, output kind, budget, and expiry evidence. |
| WVAG-207 | Merge a challenge. | Unsupported or contradictory claim becomes challenged without rewriting source evidence or granting authority. |
| WVAG-208 | Reject unsafe context and merge requests. | Protected removal, stale revision, forged source, cross-run identity, over-budget output, and hidden-permission proposals fail closed. |

### Exit gate

- A bounded fixture omits low-value evidence while retaining every protected
  objective, constraint, blocker, and failed verification.
- The manifest explains every material inclusion and omission.
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

### Exit gate

- Scripted processor loss cannot corrupt or advance run state.
- Invalid, mismatched, oversized, late, or cross-run responses are rejected.
- Exact response replay does not repeat a charge or merge.
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

### Exit gate

- A 50-operation scripted run resumes after every injected interruption.
- Reconstructed state and next context equal a clean execution byte for byte.
- Concurrent or stale append attempts cannot both advance one run revision.
- An uncertain durable mutation is reopened and classified rather than replayed
  blindly.

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

### Exit gate

- Known source identities bypass broad search.
- Stale projections never identify themselves as current.
- Agent-created shared memories begin as proposed.
- A clean-room branch excludes selected attempts and every projection derived
  solely from them.
- Correcting a memory retains the prior record and source lineage.

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

### Exit gate

- The model can propose but cannot construct a valid lease.
- Approval applies only to the exact normalized previewed action.
- Stale source revision, expired lease, changed target, or forged receipt stops
  before mutation.
- An interrupted effect becomes a truthful known result or unknown blocker,
  never an automatic retry.

External communication, payment, permission changes, live broad deletion, and
other consequential actions remain denied or separately approved future
profiles.

## Stage 7 — Product and Windvale OS placement

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

### Exit gate

- Mainstream users can understand what the agent is doing without reading raw
  events.
- Advanced inspection can trace a material claim to current source evidence.
- Windows and Linux hosted products use the same canonical agent semantics.
- A Windvale OS claim is made only after dynamic launch, supervision, clocks,
  resource domains, provider isolation, and durable storage pass their own
  qualification gates.

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

### Database and durable storage

The first consumer profile is an append-only single-writer run ledger with
expected-revision append, exact idempotency lookup, ordered replay, and compact
snapshot/checkpoint publication. Large evidence stays outside event rows.

Later memory and retrieval pressure may require bounded indexes over run,
sequence, operation, idempotency identity, source identity/revision, scope,
status, review state, and expiry. These are selected only by measured queries.
Multiple writers, group commit, retention, compaction, reclamation, and backup
enter when the workload reaches them; they are not hidden assumptions of the
first semantic kernel.

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

### Windvale OS

The first portable and hosted stages do not wait for Windvale OS. The OS profile
later requires:

- one dynamic clean launch transaction;
- explicit resource-domain membership and budgets;
- bounded IPC and provider-generation loss;
- monotonic timers, cancellation, and teardown;
- durable storage and checkpoint access;
- secure model transport or a separately qualified local-model provider; and
- supervision that does not replay an uncertain mutation after restart.

The kernel supplies mechanisms. Agent objectives, prompts, context, memory,
retrieval, model routing, and approval policy remain in user-space libraries and
isolated services.

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
- identifying a strict byte-envelope seam in the current provider table; and
- designing package capability closure for a future read-only hosted proof.

Memory activation, consequential actions, peer agents, scheduling, and OS
hosting wait for their earlier ownership and recovery gates.

## Deferred breadth

The following remain deferred until the single-agent core qualifies:

- lead-worker delegation and peer communication;
- recurring, scheduled, or unattended execution;
- embedding retrieval and vector indexes;
- a general knowledge graph;
- automatic shared-memory activation;
- unrestricted browsing, terminal, filesystem, or network tools;
- local-model installation and lifecycle management;
- high-risk or regulated domain profiles; and
- provider-specific hidden state as a continuity mechanism.

## Immediate next documentation and decision work

1. Review the proposed architecture terminology, especially one agent identity
   with foreground and subconscious planes.
2. Select the Stage 0 corpus scenario and exact maximum sizes.
3. Draft the first closed run/event/context/challenge record specification.
4. Check the proposed fields against current Seed record/variant/sequence limits.
5. Map the Stage 4 store operations to the database team's completed contract
   after that result lands; do not pre-empt its current completion claim.
6. Prepare one numbered decision accepting only Stages 0 through 2 as the first
   implementation boundary.

Implementation begins after those records are reviewable. The first code change
should create only the source owner and tests needed by the accepted portable
kernel; it should not add empty provider, memory, UI, or OS scaffolding.
