# Windvale agent runtime and digital subconscious architecture

> Status: Proposed architecture for review. This document does not claim an
> implemented agent runtime, model provider, durable agent memory, or Windvale OS
> service. A numbered decision is required before the first public serialized
> format, capability interface, or authority boundary is accepted.

## Purpose

Windvale should support a long-running agent that remains coherent across model
calls, provider changes, process restarts, host environments, and eventually
Windvale OS. The model is the strongest general reasoning processor in that
agent, but it is not the sole owner of the task, its evidence, or its authority.

The architecture separates two cooperating planes:

- the **foreground agent** is the user-facing reasoning and communication plane;
  it interprets the task, proposes plans, synthesizes results, and presents one
  coherent voice; and
- the **digital subconscious** is the inspectable coordination plane that
  preserves direction, prepares attention, retrieves evidence, manages memory
  candidates, carries doubt, watches authority, and maintains continuity.

The digital subconscious is not a second autonomous identity, a hidden
personality, or a permanent free-running model. It belongs to one agent run,
receives the same user objective and governing policy, and cannot invent goals
or authority. Much of it should be deterministic Windvale code. Model-assisted
work appears only as bounded cognitive operations with explicit inputs, output
contracts, budgets, expiry, and merge rules.

The staged delivery route is defined by the companion
[implementation plan](../Project/Windvale-Agent-Runtime-Implementation-Plan.md).

## Core rule

The durable ownership rule is:

> Cognitive processors propose meaning; deterministic owners enforce authority;
> canonical sources establish truth.

This rule applies equally to a lead model, a smaller model, a deterministic
extractor, a skeptical reviewer, and a future delegated worker. Selecting a more
capable or different model changes processing capability; it does not create a
permission boundary or make its output canonical.

## Relationship to *The Mind We Build*

The current E-Worker v7 edition of *The Mind We Build* is the narrative
completeness guide for this proposal. This document translates its arrangement
into Windvale-owned semantic, capability, persistence, and operating-system
boundaries; the book does not become a serialized format or an implementation
dependency.

The complete design therefore includes: an agent larger than its model; three
change clocks; the seven digital-subconscious duties; compiled attention; memory
meaning, doors, and clean-room lineage; canonical-source perception; one voice
supported by bounded cognitive operations; typed claims and coherent work;
governed hands; continuity across replaceable models and processes; inspectable
influence; consistent meaning across workplaces and domains; and transparent
human metaphors without claims of human subjective experience.

The first Windvale implementation slice remains intentionally smaller than that
complete design. Later sections and the companion plan identify the gates that
must qualify before a product may claim the complete arrangement.

## Terminology

The first contract should use these terms precisely:

| Term | Meaning | Not implied |
| --- | --- | --- |
| Agent definition | Immutable versioned configuration selecting behavior, policy references, provider requirements, and default limits. | A live run, stored provider session, or authority grant. |
| Agent run | One durable objective, owned state, evidence history, budgets, and terminal outcome. | One model call or one operating-system process. |
| Foreground operation | The bounded reasoning operation responsible for the next user-visible synthesis or plan decision. | Exclusive ownership of run truth. |
| Digital subconscious | The coordination plane around foreground operations. | A secret goal, hidden user, or second independent agent. |
| Cognitive operation | One bounded model call or deterministic processor with a task-specific context capsule. | A permanent personality, complete run context, or ambient tools. |
| Context capsule | The exact bounded work view for one cognitive operation. | Access to the full transcript or every authorized source. |
| Canonical source | The current record owned by the relevant compiler, document, database, package, filesystem, business, or service owner. | Whatever text was most recently retrieved or summarized. |
| Memory record | A scoped, source-linked working claim admitted through a lifecycle. | Permanent truth or permission. |
| Capability lease | A short-lived, rights-reduced execution grant bound to an exact operation and scope. | General tool access or authority to broaden itself. |

The product may describe the foreground and subconscious as two kinds of
activity. Internal contracts should still preserve one agent identity and one
run truth. Bounded operators, skeptics, and archivists are roles inside the run,
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
user objective
    -> owned run state
    -> digital subconscious compiles bounded attention
    -> foreground model reasons and proposes
    -> deterministic policy validates
    -> rights-limited owner executes, when authorized
    -> canonical result and verification enter the run ledger
    -> digital subconscious reconciles the next state
```

The next model request is rebuilt from owned state and current evidence. It is
not formed by appending an unlimited transcript and trusting the newest prose to
describe the task correctly.

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
canonical facts. It proposes memory records with source lineage, scope,
confidence, temporal validity, review state, expiry, and supersession.

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

## What the digital subconscious does not own

The digital subconscious does not:

- create or revise the user's objective without an accepted user refinement;
- become the canonical owner of files, documents, packages, database records,
  business facts, identities, permissions, or source code;
- mint, extend, transfer, or broaden capability grants;
- execute consequential actions through ambient tools;
- hide or rewrite prior evidence, rejected proposals, failed verification, or
  uncertain effects;
- preserve model-private reasoning as required run truth;
- run an unlimited background loop; or
- combine private state from different agents, users, accounts, workspaces, or
  clean-room branches without an explicit authorized merge.

## Ownership boundaries

| Area | Durable owner | Responsibility |
| --- | --- | --- |
| Agent application | `Applications/` composition | User interaction, run creation, status presentation, and explicit commands. |
| Portable agent semantics | Future focused modules under `Libraries/Agent/` | Pure run-state transitions, context policy, operation records, merge rules, memory lifecycle, and verification projections. |
| Provider/service protocols | Future focused modules under `Libraries/Protocol/` | Bounded serialized request, response, event, and checkpoint validation without transport authority. |
| Platform adapters | Future focused modules under `Libraries/Platform/` | Model invocation, run storage, artifact storage, source retrieval, clocks, and other rights-limited capabilities. |
| Model provider | Bound provider instance | Model-specific transport and response evidence; no ownership of run state or permissions. |
| Canonical source | Existing source-specific owner | Current source bytes or records, revision, authorization, and mutation semantics. |
| Domain and business knowledge | Existing domain-specific owners | Services, policy, pricing, identities, schedules, client or project records, and other meaning that must not collapse into generic agent facts. |
| Search and projections | Rebuildable projection owners | Structural, lexical, semantic, and typed-relationship maps over exact source generations, with freshness and failure evidence. |
| Tool/action gateway | Capability owner and policy owner | Input validation, effect normalization, approval, lease validation, execution fencing, and observed outcome. |
| Durable run evidence | Agent run-store owner | Append-only events, compact snapshots, checkpoints, manifests, action evidence, and terminal handoff. |
| Large evidence and artifacts | Rights-limited immutable or mutable storage owner | Bounded bytes addressed by verified references; not duplicated into every event. |
| User experience | Focused applications and a later advanced inspector | Task-specific commands and understandable projections over public agent capabilities; never an alternate state owner. |
| Windvale OS mechanism | Kernel/WVA and isolated services | Resource domains, processes, IPC, timers, revocation, teardown, and provider isolation. |

No UI view owns durable agent state. No model-provider adapter owns plans,
memory, context policy, or tool authority. No search index, summary, embedding,
or graph projection becomes a second canonical source.

## Durable run model

The initial model should distinguish the following records. Names are
descriptive and do not yet freeze source spelling or serialized identities.

### Run root

One immutable run root binds:

- run and agent-definition identity;
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
- plan steps and current position;
- protected constraints and authority boundaries;
- a claim ledger containing verified, uncertain, disputed, stale, unsupported,
  and challenged propositions with source references;
- decisions, rationale, and revisit conditions;
- open questions;
- active artifacts and their revisions;
- important tool outcomes and result references;
- blockers, approvals, warnings, and verification evidence; and
- model-call, tool-call, token, elapsed-work, storage, and output budgets.

The state owner returns immutable defensive values. Changes occur only through
validated commands carrying the expected run revision and an idempotency
identity.

## Context compilation

The first context compiler should assemble deterministic layers in this order:

1. stable system and capability contract;
2. protected objective, policy, authority, plan, budget, and verification state;
3. compact live run state;
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

A role is a responsibility contract, not an identity. The same selected model
may perform conductor and skeptic work in separate calls. A deterministic parser
may be an operator. Different models may add capability or diversity but do not
create a security boundary by themselves.

Each cognitive-operation capsule should contain:

- operation and parent-run identity;
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

## Memory and its doors

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
- untrusted-source influence and sensitive-data classification;
- bounded owner-provided approval preview; and
- deterministic risk and policy results.

A permission receipt binds one decision to the exact envelope revision. An
approved receipt may produce a short-lived, rights-reduced capability lease.
The proposing model never receives a credential or the ability to mint, widen,
transfer, or renew that lease. Execution is fenced by an idempotency identity,
and an indeterminate outcome is investigated or surfaced instead of replayed.

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

Every run and operation has explicit ceilings for:

- model calls, input and output tokens, and known provider cost;
- tool and cognitive-operation calls;
- elapsed monotonic work and deadline;
- context candidates and selected items;
- inline evidence, result-reference, event, snapshot, and artifact bytes;
- active operations and later concurrency;
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
used, tools used, approvals, artifacts, blockers, and caveats. Advanced views
should expose manifests, context diffs, source lineage, memory mounts, cognitive
operations, action envelopes, verification, and recovery evidence.

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
- **learned** must identify temporary run state, reviewed durable memory,
  changed procedure or agent definition, or a later model release; and
- **changed its mind** means a source revision, challenged decision, new
  evidence, or changed context produced an owned state transition.

These terms do not claim feeling, dreaming, suffering, consciousness, a hidden
self, or biological continuity. They describe inspectable functions of the
arrangement.

## First qualification target

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

## Book-completeness qualification

Later gates should prove the complete design through two reference workflows:

- a software workflow exercises repository rules, exact source and compiler
  evidence, bounded edits, refreshed projections, focused verification, and a
  truthful handoff; and
- a proposal workflow exercises approved business knowledge, current pricing,
  client-scoped memory, scheduling evidence, a source-linked claim ledger,
  skeptical review, document verification, an influence summary, exact send
  approval, and unknown-send reconciliation.

Before describing the design as complete, deterministic evidence should also
show a 50-operation context-pressure run; clean-room descendant exclusion;
source-change invalidation; provider substitution without identity loss;
reviewed memory promotion and correction; action evidence reconstructed after
restart; cross-account and cross-workspace rejection; backup/restore
preservation; quota and pause behavior; and understandable influence views that
do not expose private model reasoning.

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
- regulated-domain readiness; or
- a Windvale OS hosting claim.

## Decision and implementation triggers

A numbered decision is required before accepting:

- the first serialized run, event, context, memory, or cognitive-operation
  format;
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
