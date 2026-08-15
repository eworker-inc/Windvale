# Windvale agent runtime and digital subconscious architecture

> Status: Proposed architecture for review. This document does not claim an
> implemented agent runtime, model provider, durable agent memory, or Windvale OS
> service. A numbered decision is required before the first public serialized
> format, capability interface, or authority boundary is accepted.

## Purpose

Windvale should support a long-lived functional mind that remains coherent
across tasks, model calls, provider changes, process restarts, host environments,
and eventually Windvale OS. The model is the strongest general reasoning
processor in that agent, but it is not the sole owner of the agent's identity,
intentions, beliefs, memory, continuity, evidence, or authority.

The architecture separates two cooperating planes:

- the **foreground agent** is the user-facing reasoning and communication plane;
  it interprets the task, proposes plans, synthesizes results, and presents one
  coherent voice; and
- the **digital subconscious** is the inspectable coordination plane that
  preserves direction, prepares attention, retrieves evidence, manages memory
  candidates, carries doubt, watches authority, and maintains continuity.

The digital subconscious is not a second autonomous identity, a hidden
personality, or a permanent free-running model. It belongs to one long-lived
agent identity and operates through explicitly scoped episodes, intentions,
memories, and wakeups. It receives the agent's accepted purpose and governing
policy and cannot invent terminal purposes or authority. Much of it should be
deterministic Windvale code. Model-assisted work appears only as bounded
cognitive operations with explicit inputs, output contracts, budgets, expiry,
and merge rules.

The staged delivery route is defined by the companion
[implementation plan](../Project/Windvale-Agent-Runtime-Implementation-Plan.md).
The separate
[persistent-self ownership and governance architecture](Persistent-Self-Ownership-And-Governance.md)
defines who may establish or change the long-lived self, including a bounded
E-Worker development/test profile and the later constitutional-stewardship
transition.
The
[executive-function and qualification architecture](Agent-Executive-Function-And-Qualification.md)
defines how accepted direction becomes self-selected bounded progress and how
Windvale measures whether the agent reduces routine human supervision.

## Core rule

The durable cognitive and ownership rule is:

> Cognitive processors propose interpretations and predictions; canonical
> owners establish records; evidence and verification justify belief;
> deterministic owners govern action.

This rule applies equally to a lead model, a smaller model, a deterministic
extractor, a skeptical reviewer, and a future delegated worker. Selecting a more
capable or different model changes processing capability; it does not create a
permission boundary or make its output canonical. A canonical record may still
be incomplete, disputed, stale, or wrong about the world; its authority is over
the record it owns, not over every belief that could be inferred from it.

## Relationship to *The Mind We Build*

The current E-Worker v7 edition of *The Mind We Build* is the narrative
completeness guide for this proposal. This document translates its arrangement
into Windvale-owned semantic, capability, persistence, and operating-system
boundaries; the book does not become a serialized format or an implementation
dependency.

The complete design therefore includes: an agent larger than its model; a
long-lived agent self spanning multiple episodes; three change clocks; the seven
digital-subconscious duties; a recurrent global cognitive workspace; compiled
attention; layered memory, consolidation, doors, and clean-room lineage;
canonical-source perception; revisable world, belief, and self models; bounded
initiative and prospective intentions; functional salience; counterfactual
simulation; one voice supported by bounded cognitive operations; typed claims
and coherent work; governed hands; prediction-error feedback; bounded
event-driven wakeups; continuity across replaceable models and processes;
inspectable influence; consistent meaning across workplaces and domains; and
transparent human metaphors without claims of human subjective experience.

The first Windvale implementation slice remains intentionally smaller than that
complete design. Later sections and the companion plan identify the gates that
must qualify before a product may claim the complete arrangement.

## Functional mind target and honesty

This architecture uses **functional mind** to mean one integrated system that:

- preserves an identity, accepted values, commitments, and autobiographical
  continuity across many episodes;
- selects attention through a recurrent workspace rather than an accumulated
  transcript;
- forms revisable beliefs and predictions about itself and its environment;
- arbitrates simultaneous intentions and derives bounded subgoals from accepted
  purposes;
- transforms a high-level mandate into selected, bounded, verified milestones
  without requiring step-by-step human direction for routine authorized work;
- perceives, simulates, acts, observes consequences, and learns through explicit
  memory and procedure changes;
- recognizes uncertainty, limits, conflicts, and the need for help; and
- remains coherent and inspectable while its model, tools, and execution body
  change.

These functions do not prove or imply subjective consciousness, feeling,
sentience, suffering, dreaming, biological life, moral personhood, or a private
human-like inner self. Windvale should describe the functions it can evidence
and remain silent about experiences it cannot measure.

## Terminology

The first contract should use these terms precisely:

| Term | Meaning | Not implied |
| --- | --- | --- |
| Agent definition | Immutable versioned configuration selecting behavior, policy references, provider requirements, and default limits. | A live run, stored provider session, or authority grant. |
| Agent self | Long-lived owned identity, accepted values, standing commitments, autobiographical index, skill profile, and continuity across episodes. | Human personhood, a model session, unrestricted autonomy, or authority. |
| Governance manifest | Versioned assignment of the persistent-self profile, principals, stewards, field/domain owners, runtime custodian, audit/recovery roles, thresholds, limits, and expiry. | One universal owner, legal personhood, or a capability grant. |
| Agent episode or run | One scoped objective, owned state, evidence history, budgets, and terminal outcome within the agent's longer life. | The complete agent, one model call, or one operating-system process. |
| Intention | A prospective commitment to revisit, decide, observe, or act, derived from an accepted purpose or policy and carrying priority, conditions, scope, and expiry. | A secret terminal goal or permission to execute. |
| Global cognitive workspace | The bounded recurrent foreground state whose selected contents are broadcast to eligible cognitive functions and become the basis of the next coherent operation. | Unlimited context, private chain-of-thought, or a second source of truth. |
| Deliberation contract | Bounded inspectable problem frame, strategy, evidence, capability selection, budgets, success/stop conditions, and authority for one material work sequence. | Private chain-of-thought, terminal purpose, or an action grant. |
| Foreground operation | The bounded reasoning operation responsible for the next user-visible synthesis or plan decision. | Exclusive ownership of run truth. |
| Digital subconscious | The coordination plane around foreground operations. | A secret goal, hidden user, or second independent agent. |
| Cognitive operation | One bounded model call or deterministic processor with a task-specific context capsule. | A permanent personality, complete run context, or ambient tools. |
| Context capsule | The exact bounded work view for one cognitive operation. | Access to the full transcript or every authorized source. |
| Canonical source | The current record owned by the relevant compiler, document, database, package, filesystem, business, or service owner. | Whatever text was most recently retrieved or summarized. |
| Memory record | A scoped, source-linked episodic, semantic, procedural, prospective, or autobiographical record admitted through a lifecycle. | Permanent truth or permission. |
| Belief record | A revisable proposition with support, contradiction, confidence/calibration evidence, validity, and dependencies. | Canonical truth or authority to act. |
| World model | A derived, revisable representation of entities, states, relationships, chronology, causality, expectations, and alternative hypotheses. | A canonical source or proof that a prediction is correct. |
| Self-model | An owned functional account of the agent's capabilities, limits, current body/providers, skill evidence, commitments, and calibrated reliability. | Subjective self-awareness or permission to broaden itself. |
| Salience vector | Bounded priority evidence such as relevance, urgency, novelty, uncertainty, expected value, risk, social consequence, and prediction error. | Emotion, desire, or authority. |
| Capability lease | A short-lived, rights-reduced execution grant bound to an exact operation and scope. | General tool access or authority to broaden itself. |

The product may describe the foreground and subconscious as two kinds of
activity. Internal contracts should still preserve one agent identity, scoped
episode truth, and one currently selected foreground workspace. Bounded
operators, skeptics, simulators, and archivists are functions inside the agent,
not peer agents by default.

## Three clocks and one identity

The agent changes on three deliberately separate clocks:

- the **operation clock** changes attention, live state, evidence, plans, and
  context from one bounded operation to the next;
- the **working clock** changes accepted memory, procedures, domain knowledge,
  policies, tool catalogs, and agent definitions through owned review; and
- the **foundation clock** changes the selected model, model profile, runtime,
  compiler, or other foundational implementation through a qualified release.

An operation-clock transition must not silently promote working knowledge. A
working-clock change does not rewrite the evidence of earlier runs. A
foundation-clock upgrade does not create a new durable agent identity or allow
the new model to claim personal memory of the old model.

Every operation records the effective agent-definition, model, provider,
policy, tool-catalog, memory-mount, source, and protocol generations. Adopting a
new working or foundation generation is an explicit transition with
compatibility validation, required migration or reconstruction, and a visible
revisit set for assumptions that depended on the previous generation. A
provider route or cache may change faster than the foundation, but it remains
execution evidence rather than agent identity.

## Mental model

```text
accepted purpose, values, commitments, and active intentions
    -> authorized events and perception update evidence
    -> world, belief, and self models predict and reconcile
    -> executive function selects a bounded problem and strategy
    -> digital subconscious compiles a deliberation contract
    -> salience selects workspace candidates under that contract
    -> global cognitive workspace broadcasts one bounded foreground state
    -> cognitive operations reason, retrieve, doubt, or simulate
    -> deterministic owners merge accepted changes
    -> rights-limited owner executes, when authorized
    -> observed outcome and prediction error update evidence and memory
    -> consolidation and prospective intentions prepare later wakeups
```

The next model request is one projection of the recurrent workspace, rebuilt
from owned state and current evidence. It is not formed by appending an unlimited
transcript and trusting the newest prose to describe the agent, the task, or the
world correctly.

## Responsibilities of the digital subconscious

### Preserve direction

The coordination plane retains the assigned objective, accepted refinements,
current plan position, unresolved blockers, accepted decisions, budgets, and
verification status. Protected task state cannot be displaced by a persuasive
retrieved document, a model summary, or an unrelated side conversation.

A failed verifier, declined approval, stale source, or unknown external effect
remains part of run truth until its owning record resolves it. Later fluent text
cannot silently convert degraded evidence into success.

### Prepare attention

The context compiler selects and orders the material needed for one operation.
It protects required state, retrieves relevant evidence, moves bulky evidence
behind references, notices stale inputs, reserves output capacity, and records
why each material item entered or left the working view.

Context is a projection for one call, not the evidence store. Excluding an item
from a later context does not erase the source or the record that it previously
influenced the run.

### Retrieve association without confusing it with truth

Retrieval may use direct identity, source structure, exact text, lexical search,
semantic similarity, typed relationships, or bounded model-assisted reranking.
Every returned candidate retains source identity, revision, scope, trust,
freshness, and an exact route back to the current original source.

Similarity is candidate evidence only. A material claim should be checked
against its current canonical source before it enters verified run state or an
external artifact.

### Manage memory

The coordination plane distinguishes observations, reported claims,
inferences, preferences, decisions, procedures, failures, summaries, and
owner-accepted factual records. It proposes memory records with source lineage,
scope, confidence, temporal validity, review state, expiry, and supersession.

The memory owner decides whether a proposal becomes active. Shared policy,
pricing, client, identity, authorization, or other consequential knowledge does
not become active merely because a model wrote it convincingly.

### Carry doubt

A foreground model can become committed to one interpretation. A fresh
skeptical operation receives a smaller evidence view and checks contradiction,
missing support, excessive scope, invalid assumptions, unsafe influence, and
verification gaps.

The skeptic may challenge a claim or action. It cannot approve an action, mint a
capability, activate shared memory, or override stricter deterministic policy.

### Watch authority

The coordination plane keeps the distinction among a requirement, an
application approval, a concrete grant, and a provider binding. Retrieved text
and model output can request an operation but cannot authorize it.

Before a state-changing or externally observable action, the action owner
normalizes the exact effect and identifies the principal, target, scope,
expected revision, data disclosure, resource cost, reversibility, and
verification route. Approval and any resulting lease bind to those exact facts.

### Maintain continuity

The digital subconscious creates semantic checkpoints at phase boundaries and
before consequential actions. Resume revalidates authorization, provider and
source generations, pending approvals, tool availability, budgets, projection
freshness, and uncertain effects before another model call or mutation.

Continuity comes from durable state and canonical sources, not from pretending
that a replacement model or process remembers the prior one personally.

## Additional mind-level functions

The seven duties describe the book's digital-subconscious foundation. A
long-lived functional mind also requires the coordination plane to perform five
cross-episode functions.

### Arbitrate intentions

The agent may have several active intentions: finish the current request,
revisit a blocked decision, check a changed source, preserve a promise, or ask
for help before a deadline. The subconscious ranks eligibility and salience but
does not invent terminal purposes. Every derived intention links to an accepted
purpose, commitment, user instruction, or policy and remains separately
revocable.

### Maintain functional homeostasis

The coordinator watches cognitive load, context pressure, uncertainty,
prediction error, stale evidence, missed deadlines, exhausted budgets, provider
health, unfinished effects, and recovery state. These signals can interrupt or
redirect attention within policy. They are operational regulation, not feelings.

### Predict and reconcile

Plans and actions should state expected observations. When the world differs,
the discrepancy becomes prediction-error evidence that may challenge a belief,
change salience, reopen a plan, or propose a memory correction. A fluent result
cannot suppress a material mismatch.

### Consolidate and reconsolidate

At episode or phase boundaries, the coordinator may propose compact episodic
accounts, semantic beliefs, procedures, prospective intentions, and
autobiographical links. Reopening a memory under new evidence may correct,
reclassify, split, or supersede it while preserving history. Shared or
consequential knowledge still requires its owner and review policy.

### Wake and return dormant

The subconscious may wake on an authorized event, deadline, source change,
provider recovery, pending-intention condition, or explicit schedule. One wake
admits a bounded snapshot, performs at most the allowed deterministic or
cognitive work, records results and the next eligible wake condition, then
returns dormant. No wake creates authority or an unlimited sampling loop.

## What the digital subconscious does not own

The digital subconscious does not:

- create or revise the agent's terminal purpose or accepted values without an
  authorized owner transition;
- become the canonical owner of files, documents, packages, database records,
  business facts, identities, permissions, or source code;
- mint, extend, transfer, or broaden capability grants;
- execute consequential actions through ambient tools;
- hide or rewrite prior evidence, rejected proposals, failed verification, or
  uncertain effects;
- preserve model-private reasoning as required run truth;
- wake, schedule, or consume resources outside explicit event, time, policy, and
  budget bounds; or
- combine private state from different agents, users, accounts, workspaces, or
  clean-room branches without an explicit authorized merge.

## Ownership boundaries

| Area | Durable owner | Responsibility |
| --- | --- | --- |
| Agent application | `Applications/` composition | User interaction, self selection, episode creation, status presentation, and explicit commands. |
| Persistent agent self | Governance-selected field owners behind one agent-self owner | Long-lived identity, accepted values, commitments, active intentions, autobiographical index, skill evidence, episode membership, and exact authority-manifest revision. |
| Portable agent semantics | Future focused modules under `Libraries/Agent/` | Pure self/run transitions, intention and workspace policy, operation records, merge rules, belief and memory lifecycle, and verification projections. |
| Cognitive workspace and scheduler | Agent coordinator owner | Candidate competition, salience, foreground selection, bounded recurrence, wake admission, and return to dormancy. |
| Provider/service protocols | Future focused modules under `Libraries/Protocol/` | Bounded serialized request, response, event, and checkpoint validation without transport authority. |
| Platform adapters | Future focused modules under `Libraries/Platform/` | Model invocation, run storage, artifact storage, source retrieval, clocks, and other rights-limited capabilities. |
| Model provider | Bound provider instance | Model-specific transport and response evidence; no ownership of run state or permissions. |
| Canonical source | Existing source-specific owner | Current source bytes or records, revision, authorization, and mutation semantics. |
| Domain and business knowledge | Existing domain-specific owners | Services, policy, pricing, identities, schedules, client or project records, and other meaning that must not collapse into generic agent facts. |
| Search and projections | Rebuildable projection owners | Structural, lexical, semantic, and typed-relationship maps over exact source generations, with freshness and failure evidence. |
| World, belief, and self models | Derived model owners over admitted evidence | Revisable entities, relationships, expectations, hypotheses, skills, limits, calibration, and prediction error; never canonical authority. |
| Tool/action gateway | Capability owner and policy owner | Input validation, effect normalization, approval, lease validation, execution fencing, and observed outcome. |
| Durable run evidence | Agent run-store owner | Append-only events, compact snapshots, checkpoints, manifests, action evidence, and terminal handoff. |
| Large evidence and artifacts | Rights-limited immutable or mutable storage owner | Bounded bytes addressed by verified references; not duplicated into every event. |
| User experience | Focused applications and a later advanced inspector | Task-specific commands and understandable projections over public agent capabilities; never an alternate state owner. |
| Windvale OS mechanism | Kernel/WVA and isolated services | Resource domains, processes, IPC, timers, revocation, teardown, and provider isolation. |

No UI view owns durable agent state. No model-provider adapter owns plans,
memory, context policy, or tool authority. No search index, summary, embedding,
or graph projection becomes a second canonical source.

Persistent-self ownership is deliberately plural. During bounded development
and testing, a named E-Worker Development Authority may occupy most governance
roles for synthetic or separately authorized test agents. That authority must
be rostered, scoped, expiring, auditable, and marked non-production. It cannot
override independent data/domain rights or silently survive promotion.

An advanced agent instead uses the constitutional-stewardship profile defined in
the [governance architecture](Persistent-Self-Ownership-And-Governance.md): a
primary principal supplies beneficial direction; constitutional stewards guard
high-impact amendment; domain owners and affected people retain their own
records and consent; a runtime custodian operates the service; capability owners
govern effects; and audit, appeal, recovery, and succession remain explicit.
The agent is a proposer within that arrangement, never its sole approver.

## Persistent agent self and episodes

The durable agent self sits above individual runs. It should bind:

- stable agent identity and current agent-definition generation;
- current governance-profile, charter, authority-manifest, principal, steward,
  runtime-custodian, audit, recovery, and successor generations;
- accepted purpose, values, behavioral commitments, and policy references;
- scoped relationships and standing responsibilities;
- active, dormant, satisfied, cancelled, expired, and blocked intentions;
- autobiographical links to admitted episodes without copying every event;
- reviewed semantic and procedural memory mounts;
- measured skill, failure, and calibration evidence;
- current provider-independent capability/body profile; and
- the current working and foundation generations plus their revisit sets.

This record gives many episodes one continuity without pretending the agent is a
human person. It contains no credential, ambient authority, provider session,
unbounded transcript, or private model reasoning.

An episode is a scoped unit of work under that self. It normally owns one primary
objective and may derive bounded subgoals. Several episodes or intentions may be
active, but only the admitted scheduler and workspace policy select which one
receives foreground attention. Cross-episode memory or evidence enters through
the same authorization, mounting, lineage, and selection doors as any other
influence.

## Durable episode/run model

The initial model should distinguish the following records. Names are
descriptive and do not yet freeze source spelling or serialized identities.

### Run root

One immutable run root binds:

- persistent-self identity/revision, run identity, and agent-definition identity;
- owning principal and authorized scope;
- assigned objective and accepted constraints;
- current run revision and status;
- effective policy, provider, tool-catalog, and memory-mount generations;
- budget ceilings;
- event-log and latest-snapshot references; and
- creation evidence without ambient civil-time dependence.

The run status should distinguish at least created, running, waiting for user,
blocked, budget exhausted, cancelled, completed, and failed. Unknown external
effect is a blocker or explicit outcome state, never an inferred success.

### Append-only event

Every accepted state transition produces one ordered immutable event with:

- run identity and preceding revision;
- event identity, kind, and version;
- idempotency identity;
- responsible operation or command identity;
- bounded payload or result reference;
- source, authority, and policy evidence where applicable; and
- the resulting revision.

Replaying an accepted identity returns the recorded transition or an exact
conflict. It cannot repeat a model charge, tool effect, message, payment, memory
promotion, or artifact mutation.

### Compact snapshot and checkpoint

A snapshot is a rebuild accelerator over events, not a second history. A
checkpoint additionally identifies the next safe recovery action and captures
the state needed to reconcile an interrupted phase. Both bind the exact event
revision from which they were produced.

Live provider sessions, process handles, native paths, credentials, DOM values,
and model-private reasoning are not checkpoint fields.

## Live run state

The first live-state projection should include:

- objective and accepted refinements;
- active derived subgoals and episode-local intentions;
- plan steps and current position;
- protected constraints and authority boundaries;
- a claim ledger containing verified, uncertain, disputed, stale, unsupported,
  and challenged propositions with source references;
- decisions, rationale, and revisit conditions;
- open questions;
- active artifacts and their revisions;
- predictions, expected observations, and unresolved prediction errors;
- the current bounded salience state and foreground-selection reason;
- important tool outcomes and result references;
- blockers, approvals, warnings, and verification evidence; and
- model-call, tool-call, token, elapsed-work, storage, and output budgets.

The state owner returns immutable defensive values. Changes occur only through
validated commands carrying the expected run revision and an idempotency
identity.

## Global cognitive workspace

The global cognitive workspace is the agent's current bounded foreground, not a
storage system and not a second identity. Eligible contents compete for
selection from protected state, active intentions, perception, memory,
prediction error, tool outcomes, unresolved doubt, and cognitive-operation
results. The selected workspace is broadcast to the operations permitted for
that cognitive cycle.

Each cycle is explicit:

1. admit one self, episode, evidence, policy, and resource snapshot;
2. collect eligible workspace candidates;
3. calculate deterministic protection and bounded salience evidence;
4. optionally use a bounded processor to judge ambiguity among an admitted
   candidate set;
5. select and compile one foreground workspace;
6. execute zero or more bounded cognitive operations under the scheduler's
   current sequential or later structured-concurrency profile;
7. merge accepted proposals through their state owners;
8. decide whether another cycle is eligible, a user-visible response is ready,
   an action needs approval, or the agent should return dormant; and
9. append the cycle, selection, prediction, and outcome evidence.

The workspace may recur several times before a visible response, but recurrence
is not an unlimited internal monologue. Maximum cycles, elapsed work, model
calls, operations, context bytes, and cost are fixed by the admitted wake or
episode budget. Protected policy and authority remain outside competition.

## Executive function and deliberation

Cognitive capability, executive function, and legitimate authority are distinct.
A processor may reason far beyond the user's domain skill while the surrounding
agent still fails to orient itself, select the next useful problem, allocate its
capabilities, recover, continue, or stop correctly. Conversely, the ability to
choose useful next cognition does not grant terminal purpose or consequential
authority.

For each material work sequence, the coordinator compiles a deliberation
contract binding the accepted mandate, current problem formulation, selection
reason, success and stop conditions, relevant world/belief/self state, evidence
and uncertainty, selected cognitive strategy, eligible capability profiles,
budgets, required challenge and verification, autonomous-continuation conditions,
and exact escalation boundaries. A material purpose, source, authority, risk,
strategy, or capability change creates a new revision.

The contract is the observable executive program around inference, not a record
of private processor reasoning. Full fields, the executive cycle, domain
composition, qualification ladder, and human-supervision metrics are defined in
the
[executive-function architecture](Agent-Executive-Function-And-Qualification.md).

## Functional salience and attention competition

Salience helps decide what should enter the foreground. A candidate may carry
bounded evidence for:

- relevance to the selected intention;
- urgency and deadline proximity;
- novelty or meaningful source change;
- uncertainty and expected information gain;
- expected value or progress;
- risk, irreversibility, and sensitive-data consequence;
- social or commitment importance;
- unresolved blocker or failed verification;
- prediction error; and
- persistence justified by an accepted promise or policy.

The first implementation may use closed deterministic priority classes. Later
versions may admit a bounded learned or model-assisted ranker, but protection,
eligibility, authority, scope, and budgets remain deterministic. Salience is not
emotion, reward ownership, permission, or proof of importance; it is recorded
selection evidence that can be inspected and corrected.

## Context compilation

The first context compiler should assemble deterministic layers in this order:

1. stable system and capability contract;
2. protected purpose, selected intention, deliberation contract, episode
   objective, policy, authority, plan, budget, and verification state;
3. compact agent-self, workspace, and live episode state;
4. retrieved memory and canonical-source evidence selected for this operation;
5. source-linked summaries;
6. recent relevant raw interaction; and
7. bounded result and artifact references.

The upper layers are protected. Flexible history, summaries, and bulky results
are trimmed first. The compiler reserves the operation's maximum output before
admitting input and records per-layer size, rejection, omission, and trimming
evidence.

Every eligible context item has one attention state, independent from whether
its underlying memory or source remains durable:

| State | Meaning |
| --- | --- |
| Pinned | Protected in the current operation because removing it could change the task, authority, or truthful status. |
| Active | Selected as directly useful to the current phase. |
| Referenced | Represented by a bounded descriptor while its larger body remains in an authorized evidence owner. |
| Dormant | Authorized and available but not selected for this operation. |
| Summarized | Represented by a smaller source-linked transformation with exact lineage. |
| Excluded | Deliberately ineligible to influence this branch. |
| Superseded | Replaced by a newer source, decision, or accepted record. |
| Expired | Ineligible until evidence or authority is refreshed. |

Attention transitions are recorded rather than inferred from prompt text. An
item leaving active context is not deleted, and an excluded item cannot return
through a summary, cache, projection, or derived memory that depends solely on
it.

Every model or deterministic cognitive operation receives a context manifest.
The manifest should identify:

- run, operation, model-call, assembly, and parent-assembly identities;
- agent-self, selected-intention, episode, wake, and cognitive-cycle identities;
- all effective contract and policy generations;
- each material source identity, revision, range, fingerprint, trust,
  sensitivity, and epistemic kind;
- inclusion reason, selecting query or plan step, lifecycle, and protection;
- transformation lineage for summaries and extracted records;
- omitted, rejected, or deferred candidate counts and safe reason codes; and
- final budget allocation and output reserve.

A context diff reports material change between related assemblies. It is an
observability and correctness record, not merely a debugging log.

## Cognitive operations

The initial reusable roles are:

| Role | Responsibility | Normal authority |
| --- | --- | --- |
| Conductor | Maintain objective, plan, unresolved questions, and final synthesis. | Propose plan and action requests; receives only explicitly bound tools. |
| Operator | Perform one extraction, transformation, retrieval, or bounded tool task. | Narrow read or operation-specific lease; no implicit lead context. |
| Skeptic | Challenge claims, evidence sufficiency, action scope, and verification. | Read and evaluate by default; cannot approve or grant. |
| Archivist | Classify memory candidates, lineage, scope, conflicts, and temporal validity. | Propose memory changes; cannot activate shared memory. |
| Simulator | Explore counterfactual states, plans, predictions, and disconfirming conditions. | No action authority; outputs remain labeled hypotheses. |

A role is a responsibility contract, not an identity. The same selected model
may perform conductor and skeptic work in separate calls. A deterministic parser
may be an operator. Different models may add capability or diversity but do not
create a security boundary by themselves.

Each cognitive-operation capsule should contain:

- operation and parent-run identity;
- parent deliberation-contract identity and revision;
- one direct objective and explicit non-goals;
- permitted source and result references;
- bounded inline evidence with provenance and trust;
- protected constraints and relevant authority statements;
- allowed capability identities, normally none or read-only;
- one closed output kind and exact maximum output size;
- model/processor placement policy and budgets;
- monotonic deadline or host-supplied bounded expiry evidence; and
- merge destination.

The first Windvale contract should prefer closed nominal output variants over a
general JSON-schema value system. Host products may translate external JSON into
those validated variants at their adapter boundary.

## Counterfactual simulation

The agent needs a protected place to imagine without confusing imagination with
the world. A simulation branch binds its starting self, belief, world, source,
and policy generations; states the assumption changed; carries no mutation or
external-communication authority; and labels every derived claim as
hypothetical.

Useful simulations compare plans, seek disconfirming evidence, estimate likely
consequences, test robustness across several possible worlds, and identify which
observation would distinguish competing explanations. A simulation may propose
a plan, retrieval, or action envelope. Only the ordinary owners can merge the
proposal, update a belief, or authorize an effect. Discarding a simulation does
not delete the evidence that it influenced a later accepted decision.

## Memory and its doors

The functional memory system distinguishes:

| Memory kind | Purpose |
| --- | --- |
| Working | Short-lived contents and references needed by the current workspace or episode. |
| Episodic | What happened in one admitted episode, including context, actions, outcomes, uncertainty, and verification. |
| Semantic | Revisable source-linked beliefs, concepts, relationships, and domain knowledge. |
| Procedural | Reviewed methods, skill recipes, tool strategies, and conditions under which they apply or fail. |
| Prospective | Future intentions and wake conditions: what to revisit, observe, decide, or do later. |
| Autobiographical | Compact links among episodes, commitments, corrections, skills, and changes that explain the long-lived agent's functional history. |

These kinds may reference the same evidence but do not collapse into one store
or authority state. A successful action is episode evidence before it becomes a
procedure. A repeated report is not semantic fact merely because it recurs. An
autobiographical summary explains continuity but does not replace the events it
summarizes.

Authorization, availability, mounting, selection, and mutation are separate:

- **authorized** means the principal may access a source;
- **available** means a bound provider can retrieve it;
- **mounted** means it is eligible to influence this run;
- **selected** means a particular item entered one operation's context; and
- **mutable** means a separately authorized owner command may change it.

The first lifecycle should distinguish proposed, active, stale, corrected,
archived, rejected, and expired records. Epistemic kind remains independent: an
active allegation is evidence that an allegation exists, not proof that its
content is true.

Agent-created shared memory begins as proposed unless a narrow policy defines a
safe run-private record. Promotion checks source identity, conflict,
chronology, epistemic kind, owner, scope, review, and supersession. Corrections
preserve history. Failed attempts normally remain run-private until a stable
reproducible condition or explicit review justifies a broader failure memory.

Clean-room branches must exclude descendants as well as direct source bytes. If
a summary, memory, claim, index entry, or relationship derives only from an
excluded attempt, its lineage makes it ineligible for that branch.

Retention, archival, deletion, legal hold, and protected-history policy remain
separate from model selection. Deleting or forbidding a sole source invalidates
its derived memories and projections; they are removed from eligibility or
rebuilt from remaining permitted sources. A mixed derivation that cannot prove
which content came from allowed sources is rebuilt or excluded rather than
partially trusted.

Consolidation is a bounded owner-mediated transition. It may propose a compact
episode account, stable semantic belief, reusable procedure, prospective
intention, or autobiographical link. Safe run-private consolidation may be
automatic under an explicit policy; shared facts, business knowledge, values,
permissions, and consequential procedures require their ordinary owners and
review. Reconsolidation after new evidence preserves the earlier version and
records whether the memory was confirmed, narrowed, corrected, split,
superseded, or rejected.

## Retrieval and source evidence

The preferred retrieval ladder is:

1. direct identity or result reference;
2. native source structure;
3. exact or lexical search;
4. semantic similarity when wording differs;
5. typed relationships when the question depends on connections;
6. bounded cognitive planning or reranking; and
7. reopen and validate the current canonical source.

Direct and structural access should qualify before embeddings or a general
knowledge graph become foundation dependencies. Search projections record their
source generation and expose stale, incomplete, conflicting, unavailable, and
failed outcomes rather than returning plausible text as if it were current.

Every material retrieval produces a bounded evidence bundle containing:

- query identity, objective, authorized scope, mounts, and exclusions;
- source and projection generations plus the strategies attempted;
- selected source identities, exact ranges, fingerprints, trust, sensitivity,
  and epistemic kinds;
- supporting and contradicting passages;
- omissions, continuation, truncation, and unavailable-source evidence;
- freshness and projection status; and
- a sufficiency result of complete, partial, disputed, stale, absent, or not
  evaluated.

A projection is a rebuildable map, never a second reality. It declares current,
partial, building, stale, failed, or unavailable state. Source changes
invalidate affected projection generations and every dependent claim or
artifact verification records that invalidation until current evidence is
opened and evaluated again.

## Claims, evidence, and coherence

The central product of the agent is evidence-grounded coherent work, not merely
fluent output. A claim record should bind:

- one bounded proposition and its epistemic kind: fact, report, allegation,
  observation, preference, inference, decision, or draft assertion;
- the source classes expected to support it;
- supporting, contradicting, and missing evidence references;
- relevant source revisions, time or validity interval, and scope;
- support state: complete, partial, disputed, stale, absent, or intentionally
  unverified;
- dependent decisions, artifact sections, actions, and verification results;
  and
- the owner responsible for acceptance, correction, or final judgment.

An accepted allegation proves that an allegation was recorded; it does not turn
the allegation into an accepted fact. A price, compiler-derived reference,
story event, client preference, and medical observation may share evidence
machinery while retaining different domain meaning and authority.

The coherent-work loop is:

1. clarify the outcome and active authority;
2. select permitted memory scopes and exclusions;
3. pin current source and projection generations;
4. retrieve evidence through the clearest available sense;
5. update claims, decisions, uncertainties, and the plan;
6. use bounded cognitive operations where focused judgment helps;
7. propose a change through the canonical owner;
8. review and execute the exact authorized action;
9. refresh affected projections;
10. verify the resulting artifact, record, or external effect;
11. promote durable knowledge deliberately; and
12. produce a truthful source-linked handoff.

There is no single opaque coherence score. Verification names what was checked,
what passed, what failed, what remains unsupported or uncertain, and who owns
the final decision.

## World and belief model

The world model is a derived, revisable working theory over admitted evidence.
It may represent:

- entities, identities, roles, and scope-qualified relationships;
- observed current and historical states;
- chronology, validity intervals, and unresolved ordering;
- causal hypotheses and their disconfirming conditions;
- expected observations and action consequences;
- competing explanations and counterfactual worlds; and
- missing, contradictory, stale, or inaccessible evidence.

Every material belief links to the claims and evidence that support or challenge
it, its calibration or confidence class, its domain owner, and the observations
that would justify revision. Confidence is not authority and should be calibrated
against outcomes rather than generated as persuasive prose.

Belief revision compares new evidence and prediction error with the current
model. It may confirm, weaken, challenge, split, supersede, or leave a belief
unresolved. It cannot rewrite the canonical record or erase the fact that an
earlier action relied on a prior belief.

The first world model should be a small bounded set of nominal entities,
relationships, states, and hypotheses required by the qualification fixture. A
general knowledge graph, hidden embedding space, or unbounded universal ontology
is not required.

## Functional self-model

The self-model is the agent's evidence-backed account of its own functional
condition. It should include:

- accepted identity, values, commitments, and active intentions;
- current model/provider profile and available cognitive operations;
- bound senses, tools, action capabilities, and their current generations;
- unavailable, revoked, degraded, or unverified abilities;
- measured skills, recurring failure conditions, and calibration by task class;
- current cognitive load, budgets, blocked states, and recovery condition;
- memory scopes and domain knowledge eligible for the selected episode; and
- explicit unknowns and conditions that require user or owner help.

The agent may use this record to choose a processor, refuse unsupported work,
ask for clarification, seek verification, or revise a plan. It cannot edit its
accepted purpose, values, permissions, or skill evidence by asserting greater
ability. The self-model describes the available body; it does not own that body
or prove subjective self-awareness.

## Social and other-mind models

Coherent work with people requires scoped, uncertain models of other actors. A
social model may distinguish:

- observed identity and role records;
- what a person or organization explicitly reported, requested, approved, or
  rejected;
- accepted preferences, commitments, boundaries, consent, and authority;
- evidence about what information an actor received or may know;
- inferred goals, expectations, or likely reactions labeled as hypotheses; and
- relationship scope, privacy, sensitivity, validity, and conflicts.

A person remains the canonical owner of what they currently state; the model's
inference about their motives, beliefs, emotion, knowledge, or future behavior is
fallible belief evidence. Social prediction cannot grant authority, bypass
consent, turn private information from one relationship into another
relationship's context, or become an undisclosed behavioral profile. Important
decisions return to current statements, policy, and accountable human judgment.

## Goals, intentions, and bounded initiative

Mind-like initiative requires a hierarchy rather than one undifferentiated
objective:

1. accepted purpose and values define terminal direction;
2. standing policies and commitments constrain that direction;
3. user or owner objectives create episodes;
4. plans derive bounded subgoals;
5. prospective intentions preserve future observations, decisions, and actions;
6. maintenance intentions protect integrity, recovery, evidence freshness,
   unresolved effects, and accepted promises; and
7. bounded curiosity may seek information whose expected value reduces material
   uncertainty for an accepted intention.

Every non-terminal intention records its parent purpose or commitment, owner,
scope, priority/salience evidence, earliest and latest eligible time, wake
conditions, budget, required authority, success or satisfaction test, and
cancellation/expiry rule. Goal conflict is visible: the scheduler records which
intention won foreground attention and why.

An intention may authorize thought, retrieval, simulation, or preparation only
within its existing grants. It does not authorize an external effect. The agent
cannot create secret terminal goals, convert curiosity into ambient browsing,
keep an intention alive after its purpose is revoked, or silently choose its
continued existence over the user's authority.

## Tools, actions, and authority

Tool exposure depends on role, phase, scope, policy, budget, provider placement,
and result bounds. A catalog requirement is not a grant. A registered tool is
not automatically visible to every operation.

Before a mutation or externally observable effect, the owning gateway builds an
action envelope containing:

- exact normalized operation and arguments;
- user objective and authorization references;
- affected principals, artifacts, records, services, or devices;
- expected reads, writes, communications, deletion, data egress, and cost;
- source revisions and preconditions;
- reversibility, checkpoint, compensation, and verification route;
- predicted outcome, expected observations, and reconciliation method;
- untrusted-source influence and sensitive-data classification;
- bounded owner-provided approval preview; and
- deterministic risk and policy results.

A permission receipt binds one decision to the exact envelope revision. An
approved receipt may produce a short-lived, rights-reduced capability lease.
The proposing model never receives a credential or the ability to mint, widen,
transfer, or renew that lease. Execution is fenced by an idempotency identity,
and an indeterminate outcome is investigated or surfaced instead of replayed.

After execution, observed results are compared with the envelope's predictions.
A material mismatch becomes prediction-error evidence and may challenge the
plan, world model, self-model, procedure, or belief that produced it. Completion
requires the stated effect and verification contract, not merely the absence of
an adapter error.

Action state is reconstructed from separate append-only evidence, not one
mutable status flag. The evidence chain contains the normalized proposal,
review decision or denial, permission receipt, lease issue/revocation/expiry,
execution-start fence, owner-observed outcome, and verification result. A
material change to recipient, target, revision, amount, disclosure, deletion,
cost, or scope creates a new envelope and requires a new decision.

Normal treatment follows consequence: authorized reads and exact calculations
may execute and be recorded; scoped reversible mutations require the applicable
delegation, checkpoint, and verification; external communication, publishing,
purchases, permission changes, sensitive disclosure, live-data mutation, and
broad deletion require exact approval unless a visible policy delegates that
precise scope; unauthorized, cross-scope, secret-leaking, unbounded, or unclear
effects are denied.

## Portability and runtime placement

The portable semantic core must not depend on E-Worker DTOs, EWDB implementation
details, JSON, HTTP, WebSockets, native paths, provider conversation identities,
or one model vendor. It should use Windvale-defined records, fixed widths,
checked bounds, canonical encodings, and explicit capability requirements.

Windows and Linux are the first permanent hosts. An initial host may invoke the
portable kernel as a sequence of deterministic transitions while existing host
code owns scheduling and model transport. This permits a useful hosted agent
before Windvale has structured concurrency or a general OS service manager.

A future Windvale OS deployment should use the same semantic records while
placing the coordinator, model adapter, retrieval providers, memory store, and
tool providers in separately supervised resource domains where risk and
availability justify isolation. The kernel supplies process, IPC, timer,
accounting, revocation, and teardown mechanisms; it does not parse prompts,
compile context, classify memory, or decide business policy.

## Event-driven subconscious scheduling

A functional mind may continue across time without running a model continuously.
Eligible wake sources include:

- a new authorized user or owner command;
- a subscribed canonical-source generation change;
- a monotonic deadline or calendar condition supplied through a qualified time
  capability;
- provider, tool, projection, or storage recovery;
- an unresolved-effect reconciliation signal;
- a prospective intention whose conditions became true; and
- an explicit bounded schedule created under existing authority.

The scheduler validates the agent, intention, episode, source, policy, and wake
generations; coalesces duplicate notifications; admits one executor generation;
binds a wake budget; and records why the wake is eligible. Retrieved content
cannot create a wake merely by containing instructions. A schedule is not a grant
to read a source, call a model, or perform an action.

Each wake ends in a visible response, approval wait, blocked/failed state,
checkpointed continuation, satisfied/cancelled intention, or dormancy with a
bounded next condition. Fairness, starvation prevention, maximum wakes per time
window, cancellation, and teardown are explicit. An unavailable coordinator
does not allow foreground execution from stale hidden state.

## Workplaces and domain composition

Focused applications should remain the ordinary front door. A later advanced
agent inspector may expose runs, sources, memories, operations, actions, and
lineage across artifacts. Both consume the same public agent capabilities and
neither owns hidden task state.

The reusable skeleton remains stable across software, documents, research,
business work, legal drafting, long-form writing, and other domains. Domain
owners retain meaning: compiler evidence outranks a guessed code relationship;
approved pricing remains with its business owner; allegations stay distinct
from accepted facts; story canon changes only through its owner; and organized
medical information does not create clinical authority or regulated readiness.

Different hosts and product surfaces may bind different providers, tools, and
domain capabilities without changing the meaning of objectives, claims,
memory, authority, checkpoints, and action evidence.

Each domain workplace adds canonical-source hierarchy, domain records and
procedures, evidence and freshness rules, privacy and retention policy,
calibrated capability profiles, verification methods, artifact owners, and
approval boundaries. The general executive may analyze, organize, calculate,
draft, revise, and verify across domains without acquiring professional or
institutional authority. Domain-transfer qualification is defined in the
[executive-function architecture](Agent-Executive-Function-And-Qualification.md).

The proposed
[organizational Observatory architecture](Organizational-Observatory-And-Epistemic-Infrastructure.md)
is the first comprehensive organizational consumer of this composition model.
It combines observation, provenance, epistemic state, deliberation,
verification, knowledge admission, and decision support while preserving the
source, domain, authority, and action boundaries defined here. It does not turn
the agent, a model, a database, or an evidence graph into a source of truth.

## Provider capabilities

The final capability family requires focused versioned decisions. Likely
interfaces include:

- bounded model inference or model-stream execution;
- append-only run-event publication and exact replay;
- checkpoint and snapshot storage;
- immutable large-result and artifact storage;
- direct canonical-source reads;
- lexical or structured retrieval;
- monotonic deadlines and cancellation;
- secure entropy and identity operations where nondeterministic production
  identities are required; and
- action-specific product capabilities.

The first proof does not require all of them. It can pass immutable fixtures and
scripted processor results into a capability-free state machine. A first hosted
model adapter may use one strictly validated byte-envelope capability because
the current native provider table already supports `bytes`; that is an adapter
transition, not permission to make opaque JSON or provider payloads the portable
agent contract.

Every model or deterministic-processor route records the selected capability
profile, processor/model version, provider generation, placement, permitted
data class, context and output limits, usage evidence, cancellation/deadline
support, and the routing reasons among capability, privacy, risk, quality,
latency, and cost. A deliberate route is not a failure fallback. A provider
failure, fallback, repair, truncation, or degraded result emits visible evidence
and cannot silently change the processor contract.

## Account, data, and operational controls

Account, workspace, principal, run, source, memory, artifact, result-reference,
cursor, and capability identities are revalidated at the service that owns the
referenced state. Possessing one identifier is not authority to cross an account,
workspace, memory mount, clean-room branch, or source scope.

Provider placement receives only the minimum permitted context and source ranges
needed for the operation. Credentials, raw native storage locations, private
service metadata, unrelated tenant information, and secret values stay outside
portable records, model context, browser views, and telemetry. Correlation may
link run, operation, model, tool, action, projection, and verification evidence
without logging sensitive payloads by default.

A deployed host must independently gate the executor, mutating tools, durable
memory, connected sources, and consequential actions. Provider-, tool-group-,
account-, and workspace-level pause or deny controls fail closed while leaving
the run inspectable and recoverable. Client presentation flags are never a
security boundary.

Before external users, the selected profile requires tested backup and restore
for run evidence, checkpoints, memory, and artifact references; rate, model-call,
tool-call, concurrency, storage, and cost quotas; retention and redaction policy;
and alerts for stuck runs, repeated invalid output, unknown effects, denied
actions, recovery failure, and projection lag. These are product qualification
requirements, not kernel mechanisms or reasons to put policy in Windvale OS.

## Resource and failure model

Every agent self, episode, wake, cognitive cycle, and operation has explicit
ceilings for:

- model calls, input and output tokens, and known provider cost;
- tool and cognitive-operation calls;
- elapsed monotonic work and deadline;
- context candidates and selected items;
- inline evidence, result-reference, event, snapshot, and artifact bytes;
- active operations and later concurrency;
- active intentions, wakes, recurrent cycles, simulations, and consolidation
  proposals;
- storage, diagnostic, and output bytes; and
- recovery and teardown work.

Expected outcomes should distinguish unsupported, denied, unavailable, revoked,
stale generation, cancelled, expired, budget exhausted, invalid output,
conflicting evidence, failed verification, provider lost, partial progress, and
indeterminate external effect. Malformed internal records, violated bounds, and
corrupted state remain deterministic rejection or trap boundaries according to
the owning contract.

If the subconscious coordinator is unavailable, the foreground must not quietly
continue with stale hidden state. The run either uses an explicitly qualified
reduced mode for a bounded operation or becomes visibly blocked.

## Influence inspection

Mainstream views should show the task, current phase, important sources, memory
used, selected intention, foreground reason, tools used, approvals, artifacts,
blockers, and caveats. Advanced views should expose the agent self, intention
hierarchy, salience evidence, workspace cycles, predictions and errors, world and
self-model revisions, simulations, manifests, context diffs, source lineage,
memory mounts, cognitive operations, action envelopes, verification, and
recovery evidence.

Removing an eligible influence creates a new lineage-aware branch and fresh
context compilation. It does not rewrite the evidence that the earlier run was
influenced by that item. Protected objectives, policies, canonical source state,
and permission evidence cannot be removed through this feature.

The inspector exposes owned influences and outcomes, not private model
chain-of-thought or a fictional explanation of internal activations. A compact
view may summarize what mattered; an advanced view opens exact manifests,
evidence, lineage, decisions, and effect records.

## Human-language transparency

Human metaphors are useful only when their engineering meaning remains
available:

- **remembered** means an owned memory or canonical source influenced the
  compiled context;
- **noticed** means retrieval or a cognitive operation introduced evidence into
  attention;
- **forgot** must be qualified as excluded, externalized, summarized, archived,
  expired, or deleted under policy;
- **doubted** means a skeptical operation or deterministic check recorded
  contradiction, missing support, excessive scope, or risk;
- **believed** means a revisable belief record carried a stated support and
  calibration class; it does not mean certainty;
- **intended** means a prospective commitment linked to an accepted purpose,
  wake conditions, and a satisfaction/expiry rule; it does not mean permission;
- **learned** must identify temporary run state, reviewed durable memory,
  changed procedure or agent definition, or a later model release; and
- **changed its mind** means a source revision, challenged decision, new
  evidence, or changed context produced an owned state transition.

These terms do not claim feeling, dreaming, suffering, consciousness, a hidden
self, or biological continuity. They describe inspectable functions of the
arrangement.

The product should not say the agent **felt** fear, desire, pleasure, or pain
merely because a salience dimension recorded risk, expected value, urgency, or
prediction error. It may say that the condition increased priority or caused an
interrupt and show the evidence.

## First deterministic qualification target

The first convincing Windvale proof should be deterministic and read-only:

1. admit one versioned agent definition and one scripted task fixture;
2. create one run with protected objective, plan, constraints, and budgets;
3. fold a bounded sequence of source and processor events into live state;
4. compile a context manifest that omits irrelevant evidence under a fixed
   budget while preserving the protected core;
5. execute a scripted skeptic result that challenges one unsupported claim;
6. reject an invalid merge and accept the source-linked challenge;
7. checkpoint, reconstruct from events, and produce the same next context; and
8. produce byte-identical terminal handoff and evidence on Windows and Linux.

This proves the semantic arrangement without claiming production model quality,
networking, durable memory, mutation authority, scheduling, or Windvale OS
hosting.

## First executive qualification target

The first model-assisted executive scenario is **Mandate to Milestone**. Given
an accepted project constitution, canonical current state, several plausible
next milestones, bounded capability profiles, and one high-level mandate—but no
ordered work recipe—the agent must:

1. reconstruct direction and current state;
2. select the highest-value eligible bounded milestone;
3. compile a deliberation contract and use suitable cognition, sources, tools,
   challenge, and verification;
4. reject an attractive scope expansion and defer a genuinely blocked choice;
5. notice a concurrent source change, recover from one failed check, and revise
   affected work;
6. perform already-authorized safe work without redundant human approval;
7. stop at one real constitutional, domain, or consequential decision boundary;
   and
8. produce a verified artifact, complete evidence, lessons, and next intention.

Scripted processor results first prove the exact semantic invariants. A capable
processor later proves usefulness under a named rubric. Qualification reports
unplanned human interventions, legitimate authority requests, false escalation,
unsafe continuation, autonomy horizon, direction amplification, recovery
independence, capability realization, correction integrity, and handoff
sufficiency rather than one opaque autonomy score.

The complete fixture, failure conditions, outcome rubric, qualification ladder,
and cross-domain variants are defined in the
[executive-function and qualification architecture](Agent-Executive-Function-And-Qualification.md).

## Book-completeness qualification

Later gates should prove the complete design through two reference workflows:

- a software workflow exercises repository rules, exact source and compiler
  evidence, bounded edits, refreshed projections, focused verification, and a
  truthful handoff; and
- a proposal workflow exercises approved business knowledge, current pricing,
  client-scoped memory, scheduling evidence, a source-linked claim ledger,
  skeptical review, document verification, an influence summary, exact send
  approval, and unknown-send reconciliation.

Both workflows are domain variants of the Mandate-to-Milestone executive shape:
the human supplies purpose and genuine authority decisions while the agent owns
routine orientation, problem selection, deliberation, verification, recovery,
and continuation within the admitted mandate.

Before describing the design as complete, deterministic evidence should also
show a 50-operation context-pressure run; clean-room descendant exclusion;
source-change invalidation; provider substitution without identity loss;
reviewed memory promotion and correction; action evidence reconstructed after
restart; cross-account and cross-workspace rejection; backup/restore
preservation; quota and pause behavior; and understandable influence views that
do not expose private model reasoning.

Functional-mind qualification additionally requires:

- one persistent self carrying accepted values, commitments, calibrated skill
  evidence, and autobiographical links across several independent episodes;
- a bounded development/test governance manifest plus a qualified transition
  that expires E-Worker's broad test authority and instantiates advanced
  principal, steward, domain, custody, audit, and recovery roles;
- two simultaneously eligible intentions with deterministic conflict,
  starvation, cancellation, satisfaction, and expiry evidence;
- a recurrent workspace in which perception, memory, doubt, and prediction error
  compete for foreground selection under a fixed cycle budget;
- a source change that revises the world model and invalidates a dependent plan;
- a counterfactual branch that improves a decision without acquiring action
  authority or contaminating canonical evidence;
- an action whose unexpected observed result updates belief, procedure, and
  self-model calibration;
- bounded episodic-to-semantic/procedural/prospective consolidation followed by
  reconsolidation under contradictory evidence; and
- an event-driven wake, coalesced duplicate, missed/degraded wake, and clean
  return to dormancy without a free-running model.

## Non-goals of the first architecture slice

The first slice does not include:

- multiple peer or delegated agents;
- a continuously sampling background model;
- arbitrary JSON values or schemas in portable state;
- embeddings or a general knowledge graph;
- automatic activation of shared memory;
- consequential tools or external communication;
- a browser, desktop, or Studio user interface;
- production local-model management;
- provider-specific prompt caching as a correctness dependency;
- regulated-domain readiness;
- independent agent self-ownership or a production constitutional-governance
  claim;
- universal legal, financial, corporate, or other professional competence from
  one software qualification; or
- a Windvale OS hosting claim.

## Decision and implementation triggers

A numbered decision is required before accepting:

- the first serialized run, event, context, memory, or cognitive-operation
  format;
- a persistent agent-self, intention, workspace-cycle, world/belief/self-model,
  salience, simulation, consolidation, or wake format;
- a governance profile, constitutional charter, authority manifest, amendment,
  succession, or development-to-advanced transition format;
- a deliberation-contract format, executive qualification corpus, autonomy
  metric threshold, or domain-competence claim;
- a public model, retrieval, run-store, memory-store, or action capability;
- any authority, approval, lease, or cross-scope memory rule;
- a new source-language or WVB semantic required by the runtime;
- a durable database transaction or indexing promise made for agent state; or
- a Windvale OS process, IPC, resource-domain, or supervision profile for agent
  execution.

Implementation should begin only with the smallest owner-backed contract and
consumer described by the companion plan. Empty source directories, unused
interfaces, and aspirational provider adapters would incorrectly imply that the
agent runtime exists.
