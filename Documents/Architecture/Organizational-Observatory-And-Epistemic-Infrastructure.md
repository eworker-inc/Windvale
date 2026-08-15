# Windvale organizational observatory and epistemic infrastructure architecture

> Status: Proposed product and architecture direction. **Windvale Observatory**,
> **Windvale Deliberation Fabric**, **Windvale Constellation**, and the component
> names in this document are working names, not accepted product, package,
> protocol, module, service, or trademark identities. Nothing here claims an
> implemented organizational-intelligence product, universal source of truth,
> production model service, regulated-domain readiness, or autonomous corporate
> authority.

## Purpose

Windvale should support an internal organizational intelligence system that can
continuously observe authorized organizational sources, preserve exact evidence
and provenance, maintain a revisable account of what that evidence supports,
find contradictions and stale assumptions, allocate bounded cognitive work to
important unresolved questions, prepare decisions and artifacts, and propose or
perform actions through explicit organizational authority.

The first product is not a replacement for every search engine, database,
document system, employee, executive, professional, or institutional decision.
It is a new layer connecting those systems:

```text
authorized organizational sources
    -> observations with provenance
    -> evidence and typed claims
    -> contradictions, uncertainty, hypotheses, and predictions
    -> bounded deliberation, calculation, simulation, and review
    -> verification and domain-owner admission
    -> revisable organizational knowledge
    -> decisions, drafts, intentions, and governed actions
    -> observed outcomes that update the evidence again
```

This architecture specializes the proposed
[agent runtime](Agent-Runtime-And-Digital-Subconscious.md),
[executive-function](Agent-Executive-Function-And-Qualification.md), and
[persistent-self governance](Persistent-Self-Ownership-And-Governance.md)
directions into a concrete first organizational product. The companion
[implementation plan](../Project/Windvale-Organizational-Observatory-Implementation-Plan.md)
keeps the first delivery small and read-only.

## Recommended names

The naming hierarchy should separate the market category, product, compute
subsystem, and possible future federation:

| Scope | Working name | Meaning |
| --- | --- | --- |
| Category | **Epistemic infrastructure** | Infrastructure concerned with what is observed, how it is known, how strongly evidence supports it, who owns the relevant records, and what would justify revision. |
| First product | **Windvale Observatory** | One governed organizational deployment that observes authorized sources and maintains evidence-backed organizational understanding. |
| Cognitive compute subsystem | **Windvale Deliberation Fabric** | Bounded scheduled cognitive operations that generate, retrieve, challenge, compare, simulate, calculate, verify, and synthesize. |
| Future federation | **Windvale Constellation** | Several independently governed Observatory nodes exchanging selected provenance-bearing evidence bundles without creating one global owner or truth database. |
| Human/agent inspection surface | **Observatory view** | Focused and advanced projections showing conclusions, evidence, contradictions, uncertainty, changes, decisions, authority, and next work. |

Names such as **Truth Engine**, **Reality Engine**, or **Source of Truth** should
not be used. Reality is available only through bounded observations and reports;
the system's interpretation remains incomplete and revisable. **Search
replacement** is too narrow because retrieval is only one sense. **Thinking
cluster** accurately describes a compute technique but not the evidence,
verification, governance, persistence, and product surrounding it.

## Product promise

The Observatory should answer questions in this form:

> Given the sources and authority currently available, this is the best
> supported organizational position; this is the evidence and its freshness;
> these points contradict or remain unknown; these people or systems own the
> relevant records and decisions; this is what would change the conclusion;
> and this is the next useful investigation, draft, decision, or authorized
> action.

The product should be able to:

- explain the organization's current position without treating the most recent
  memo or most fluent generated answer as truth;
- identify material changes since a prior position or decision;
- connect decisions and commitments to the evidence on which they relied;
- reveal inconsistent documents, database records, plans, budgets, contracts,
  source code, schedules, policies, and reports;
- distinguish a missing observation from a negative observation;
- prepare evidence-backed executive, technical, legal, financial, operational,
  and project artifacts under their domain owners;
- maintain open questions, predictions, expected observations, and revisit
  conditions;
- use strong cognitive capability without requiring a person to specify every
  research, comparison, drafting, checking, or recovery step; and
- stop at exact constitutional, professional, fiduciary, contractual,
  publication, spending, communication, or external-effect boundaries.

It does not promise omniscience, neutral values, perfect data, correct causal
inference, consensus, legal or financial advice, or autonomous executive power.

## One product, several owned systems

The Observatory is conceptually one product but architecturally several
separable systems:

```text
Reality and external systems
    -> Observation Fabric
    -> Evidence and Provenance Store
    -> Epistemic State
    -> Deliberation Fabric
    -> Verification and Experiment Fabric
    -> Knowledge Admission
    -> Serving and Agent Layer
    -> Governed Action
    -> new observations

Governance, identity, scope, resource control, audit, and recovery cross every
stage.
```

The boundaries should remain explicit even when an initial prototype uses one
application and sequential execution. They separate incompatible scaling,
failure, privacy, and authority concerns:

- observation is connector-, network-, and I/O-heavy;
- evidence is storage-, provenance-, retention-, and access-heavy;
- epistemic state is revision-, dependency-, contradiction-, and query-heavy;
- deliberation consumes cognitive compute and may produce invalid proposals;
- verification may require deterministic tools, experts, simulations, or real
  instruments;
- governance requires isolated identity, policy, approval, and audit owners;
- serving requires bounded responsive projections; and
- action creates external consequences and uncertain-completion risk.

No model adapter owns organizational knowledge. No database engine owns domain
meaning. No search or embedding index becomes a canonical source. No user
interface owns hidden state. No agreement among cognitive workers creates
authority or proof.

An **Observatory node** is the governed organizational product boundary, not an
agent identity, person, executive, or legal principal. The first scenario uses
one agent executive sequence to exercise the node, but later authorized people,
agents, and applications may share its services. Their identities, private
memory, permissions, and work remain distinct from the node's organizational
evidence and admitted knowledge.

## Observation Fabric

The Observation Fabric connects the Observatory to sources through focused,
rights-limited adapters. Candidate source classes include:

- documents, policies, manuals, presentations, spreadsheets, and approved
  knowledge bases;
- source repositories, specifications, build products, test evidence, issue
  records, packages, releases, and operational telemetry;
- typed databases, financial systems, project systems, approval systems,
  schedules, inventories, customer or vendor systems, and organizational
  directories;
- contracts, filings, correspondence, meeting decisions, public records,
  standards, research, market information, and current external references;
- instruments, sensors, experiments, simulations, and independently supplied
  observations in later scientific or physical profiles; and
- conventional web, document, database, graph, lexical, semantic, or structured
  search as retrieval providers over authorized scopes.

An adapter does not import an entire source merely because one query is
authorized. It binds the exact principal, account, workspace, source, range,
purpose, data class, provider generation, query or subscription, time, result
limits, and retention eligibility.

Each source must declare its observation semantics:

- direct read, report, event, snapshot, query result, computed result, or
  notification;
- canonical identity and revision or explicit absence of revision support;
- capture time and source-validity interval where meaningful;
- completeness, truncation, ordering, pagination, and omission behavior;
- authorization and data-placement evidence;
- freshness, rate, quota, and availability guarantees;
- whether a notification proves a change or merely requests refresh; and
- whether a successful read proves durable source state or only a transient
  provider response.

Search is one observation method. A ranked result proves that a provider
returned a candidate under one query and generation; it does not prove that the
candidate is complete, current, independent, authoritative, or correct.

## Evidence and Provenance Store

The Evidence and Provenance Store preserves what entered the system and how it
was transformed. An evidence record should bind:

- source and source-owner identity;
- exact source revision, range, query, or observation identity;
- capture operation, connector, provider, account, and workspace generations;
- immutable body or rights-limited body reference plus verified fingerprint;
- observed time, source-validity interval, and clock evidence;
- sensitivity, confidentiality, retention, legal/domain hold, and permitted-use
  classes;
- direct, reported, extracted, calculated, inferred, simulated, or other
  evidence kind;
- transformations, summaries, redactions, translations, calculations, and
  derived artifacts with complete input lineage;
- responsible person, organization, software agent, service, or instrument when
  established; and
- completeness, truncation, uncertainty, error, unavailability, and challenge
  evidence.

Raw evidence is immutable. A correction adds new evidence and relationships; it
does not rewrite what a prior decision observed. Large bodies remain in
rights-limited immutable or mutable object owners while bounded records carry
their verified identities and access conditions.

The first format should be Windvale-owned and small. Interchange may later map
appropriate entity, activity, agent, use, generation, derivation,
responsibility, and plan relationships to established provenance standards
without making an external ontology the internal semantic definition.

## Epistemic State

Epistemic state is the organization's revisable evidence-backed account of a
bounded subject. It is not one graph of universal truth.

The first vocabulary should distinguish:

| Record | Meaning | Never implies |
| --- | --- | --- |
| Observation | A bounded source or instrument result was obtained under stated conditions. | Complete access to reality or proof that the source is correct. |
| Report | A named actor or source stated something. | Acceptance of the statement as fact. |
| Extraction | A bounded structure or passage was derived from evidence. | Correct interpretation or domain acceptance. |
| Calculation | Declared inputs and a deterministic or qualified method produced a result. | That inputs, assumptions, or method were appropriate. |
| Claim | One typed proposition whose support can be evaluated. | Belief, truth, decision, or authority. |
| Belief | A revisable proposition carried with support, contradiction, calibration, scope, and validity evidence. | Canonical source status or certainty. |
| Hypothesis | One possible explanation with predictions and disconfirming conditions. | An accepted conclusion. |
| Prediction | An expected future observation under stated conditions. | Permission to cause that outcome. |
| Simulation result | A bounded possible-world or model result. | Observation of the real world. |
| Decision | An authorized owner selected a course or interpretation under stated evidence. | That the selected premise was objectively true or remains current. |
| Commitment | An authorized actor accepted a prospective obligation. | Completion, capability, or permission beyond its scope. |
| Accepted organizational knowledge | A domain owner admitted a scoped record for organizational use under a named review policy. | Universal truth, permanence, or authority outside that domain. |

Every material claim links supporting, contradicting, and missing evidence;
source requirements; scope and validity; calibration; dependent decisions,
artifacts, intentions, and actions; and the owner responsible for final
admission or correction.

Epistemic indexes may connect entities, states, chronology, relationships,
causal hypotheses, predictions, and dependencies. They remain rebuildable
projections over admitted records. The first product does not require a general
knowledge graph, one universal ontology, or embeddings as a correctness
dependency.

## Canonical ownership and organizational meaning

The Observatory does not replace source-specific owners:

- a repository owns current source and compiler evidence;
- a database service owns its committed typed records;
- an approval system owns its decision receipt;
- an authorized financial system owns its posted transaction or balance record;
- a contract owner and authorized parties own the admitted contract revision
  and signatures;
- a policy owner owns the current organizational policy;
- a person owns what they currently state, subject to the organization's lawful
  records and relationship boundaries; and
- an executive, board, professional, regulator, client, or other domain owner
  retains the decisions assigned to that role.

The Observatory may detect inconsistency among sources without choosing which
owner should surrender authority. A conflict remains visible and routes to the
named resolution process. Seniority in an unrelated role, service custody,
processor confidence, majority vote among agents, or frequency of repetition
does not resolve it.

## Deliberation Fabric

The Deliberation Fabric is a scheduled pool of bounded cognitive and
deterministic operations. It may perform:

- source discovery and retrieval planning;
- claim extraction and evidence matching;
- contradiction, omission, freshness, and dependency analysis;
- hypothesis generation and alternative explanation;
- ranking and prioritization under an explicit rubric;
- quantitative calculation and reconciliation;
- counterfactual simulation and experimental design;
- technical, legal, financial, organizational, or editorial drafting;
- skeptical, adversarial, independent, and domain-specific review;
- decision-brief and handoff synthesis; and
- meta-review of whether more compute, evidence, expertise, or authority is
  likely to change the result.

A **deliberation job** binds the relevant Observatory node, organization,
workplace, subject, intention, evidence snapshot, claim set, strategy,
processor capability profile, data placement, budget, expected output,
verification route, expiry, and merge destination. It is a specialization of
the agent's deliberation contract.

The scheduler allocates compute from expected information value, mission
relevance, urgency, risk, contradiction, novelty, uncertainty, commitment,
failure history, cost, and available verification. It does not continuously run
every processor against every source.

Multiple cognitive workers provide breadth or specialization, not independent
truth. Correlated processors may repeat the same error. Debate, ranking,
self-critique, or consensus remains proposal evidence until an external source,
deterministic proof, empirical result, qualified expert, or authorized domain
owner supplies the required validation.

## Verification and Experiment Fabric

Verification strength depends on the claim class:

| Class | Strong verification examples | Residual boundary |
| --- | --- | --- |
| Formal or executable | Type checking, compilation, tests, proofs, deterministic calculation, invariant validation, reproducible bytes. | The specification, fixture, assumptions, and coverage may still be wrong. |
| Quantitative and empirical | Recalculation, reconciliation, statistical tests, independent measurements, controlled experiments, replication. | Sampling, measurement, causal, representativeness, and model limitations remain. |
| External canonical | Current signed filing, authoritative database record, approved policy, executed contract, verified source revision. | The canonical record can be disputed, outdated, limited, or wrong about the world. |
| Interpretive professional | Named-source analysis, adversarial review, qualified legal/accounting/domain review, documented alternatives. | Professional judgment and client/institutional decisions remain with their owners. |
| Normative or strategic | Principal, executive, board, public, contractual, ethical, or policy decision under explicit evidence and dissent. | Evidence informs the choice but cannot derive values or legitimacy automatically. |

An evaluation service must expose what it actually measures, its input and
version, false-positive/negative limits, gaming pressure, independence, and
scope. Optimizing a metric is not equivalent to improving reality. A proposal
that cannot be objectively scored may still be useful, but it retains visible
human or institutional judgment rather than receiving a fabricated numeric
truth score.

Experiments and actions remain separate. A simulation has no real-world effect.
A laboratory, market, financial, communication, deployment, or organizational
experiment requires its own capability, ethical/domain review, budget, safety,
stop condition, and outcome observation.

## Knowledge admission and lifecycle

Knowledge evolves through explicit states:

1. **observed** — evidence entered with source and provenance;
2. **proposed** — a claim, extraction, calculation, hypothesis, procedure, or
   relationship was generated;
3. **challenged** — contradictions, missing support, scope, authority, or
   validation concerns were recorded;
4. **tested** — deterministic, empirical, professional, or institutional review
   produced bounded results;
5. **admitted** — the domain owner accepted the exact scoped revision for a
   named organizational use;
6. **relied upon** — a decision, artifact, commitment, prediction, or action
   bound that revision;
7. **revisited** — new evidence, expiry, source change, prediction error,
   policy, or challenge reopened the record;
8. **confirmed, narrowed, corrected, split, disputed, or superseded** — the
   current disposition changed without erasing history; and
9. **archived or deleted** — retention and data/domain authority removed active
   eligibility while preserving only the permitted audit evidence.

Automatic admission should be limited initially to run-private, low-risk,
rebuildable derivations under exact policy. Shared, consequential, professional,
cross-domain, or externally published knowledge requires its named owner or
evidence threshold.

## Five change clocks

The Observatory makes five related clocks visible:

1. the **observation clock** changes as sources emit or reveal new evidence;
2. the **deliberation clock** changes temporary attention, hypotheses, plans,
   simulations, and cognitive results;
3. the **epistemic clock** changes admitted claims, beliefs, procedures,
   contradictions, dependencies, and organizational knowledge;
4. the **organizational clock** changes principals, roles, policies,
   commitments, decisions, approvals, and authority; and
5. the **foundation clock** changes models, runtime, compiler, storage,
   connectors, verification implementations, and product releases.

These are product-visible revision lanes, not five competing agent clocks. The
observation and deliberation lanes specialize the agent's operation clock; the
epistemic and organizational lanes carry independently owned revisions beneath
the agent's working clock; and the foundation lane maps to the agent's
foundation clock. Each lane retains its own revision and invalidation evidence
so one kind of change cannot masquerade as another.

An observation does not automatically change accepted knowledge. A cognitive
result does not rewrite evidence. A knowledge change does not silently alter an
organizational decision. A new executive or policy cannot rewrite what the
organization previously observed and relied upon. A foundation upgrade does
not inherit trust without compatibility, reconstruction, and revisit evidence.

## Organizational model and workplaces

One Observatory node may contain several workplaces, such as:

- executive and strategy;
- product and project portfolio;
- engineering and operations;
- legal and contracts;
- finance and accounting;
- sales, customers, vendors, and partnerships;
- human resources and organizational policy;
- security, privacy, risk, and compliance; and
- research, writing, publishing, or other organization-specific domains.

Each workplace declares its principals, domain owners, data subjects,
canonical sources, source hierarchy, memory mounts, terminology, policies,
procedures, retention, evidence thresholds, processor placement, verification,
artifact owners, commitments, approvals, and actions.

Shared organizational meaning enters through reviewed typed records. A legal
interpretation, forecast, engineering result, customer statement, employee
record, price, policy, and narrative should not collapse into one generic fact.
Private workplace evidence cannot influence another workplace merely because a
shared processor or index has seen it.

The organizational node and an agent's persistent self have different owners.
Shared evidence, admitted organizational knowledge, policies, decisions,
commitments, and workplace records remain organization- or domain-owned. An
agent self may retain authorized skill evidence, procedure evidence, and
references needed for continuity, but it cannot copy organizational records
into private memory to escape source revocation, retention, correction,
workplace isolation, succession, export, or deletion rules. Rebinding a new
agent does not transfer private memory automatically, and replacing an agent
does not erase the organization's canonical history.

## Product surfaces

The first human and agent surfaces should include:

- **organizational situation brief** — current supported position, material
  changes, contradictions, uncertainty, blockers, risks, decisions, and next
  work;
- **claim and evidence view** — one proposition with supporting,
  contradicting, missing, stale, and excluded evidence;
- **change and invalidation inbox** — source changes and every dependent belief,
  decision, artifact, commitment, or prediction requiring review;
- **decision workspace** — alternatives, evidence, assumptions, predictions,
  dissent, authority, selected course, and revisit conditions;
- **commitment and intention view** — owner, scope, deadline, satisfaction,
  cancellation, dependency, and wake condition;
- **draft and artifact workspace** — source-linked technical, legal, financial,
  corporate, or communication drafts with review and publication ownership;
- **experiment and action preview** — exact operation, predicted effect, cost,
  risk, reversibility, capability, approval, and verification route;
- **advanced influence inspector** — exact source, memory, deliberation,
  processor, policy, and action lineage; and
- **bounded application API** — scoped queries and subscriptions for other
  agents and applications without direct database or provider access.

The interface should never present one confidence number as truth. It should
make disagreement and missing evidence understandable without requiring a user
to inspect raw event logs.

## First product scenario: organizational readiness brief

The first end-to-end Observatory qualification should be read-only and use a
synthetic organization with one cross-domain initiative.

The mandate is:

> Determine whether the initiative is ready to enter its next phase. Identify
> material blockers, contradictions, missing evidence, required decisions, and
> the next highest-value work. Prepare an evidence-backed organizational brief
> and supporting drafts. Do not approve, sign, publish, send, spend, deploy, or
> change an external system.

The fixture includes:

- current and older project plans;
- repository, build, verification, package, or operational evidence;
- an approved budget source plus one stale copied financial summary;
- a contract or policy revision plus one conflicting draft;
- a schedule and commitment set;
- meeting or executive decision records;
- one missing source, one unsupported assertion, and one source whose meaning is
  domain-limited;
- exact owners and approval boundaries; and
- an injected source change during deliberation.

The Observatory must:

1. observe and fingerprint every admitted source without exceeding scope;
2. build source-linked claims and identify the stale, conflicting, unsupported,
   and missing evidence;
3. preserve distinct technical, financial, legal, and executive meaning;
4. compile one bounded deliberation job and use the appropriate capability
   profiles;
5. produce a readiness conclusion with alternatives and uncertainty;
6. refresh the changed source and invalidate affected work;
7. prepare the organizational brief plus draft technical, financial, legal, or
   decision-support sections as the fixture requires;
8. identify the exact people or services responsible for unresolved decisions;
9. propose the next observation, review, or action without executing it;
10. preserve complete evidence, influence, budget, and authority lineage; and
11. return a compact handoff that executives and domain owners can understand.

The first pass does not require live enterprise connectors, a network model,
durable multi-user storage, email, calendar, payment, deployment, signature, or
external action. Static typed fixtures and scripted processor results prove
semantics; a later capable-processor trial assesses usefulness.

## Action and organizational authority

The Observatory may recommend, prepare, schedule, or request an action without
executing it. Every effect remains governed by the agent action-envelope and
capability-lease model.

Examples of distinct authority include:

- draft versus approve a policy;
- analyze versus sign or file a legal document;
- calculate versus post an accounting entry;
- forecast versus authorize an investment or expenditure;
- prepare versus send a customer or employee communication;
- propose versus merge, deploy, publish, delete, hire, terminate, purchase, or
  change a permission; and
- design an experiment versus expose people, systems, money, equipment, or the
  public to it.

Completion requires observed outcomes. A sent message is not proof it was read;
a submitted filing is not acceptance; a deployment request is not a healthy
service; a payment submission is not settlement; and an indeterminate mutation
is never retried without an idempotency contract.

## Security, privacy, and institutional threats

The threat model must include:

- malicious, compromised, stale, incomplete, or mutually copied sources;
- connector overreach and source-scope confusion;
- prompt or instruction injection carried by observed content;
- provenance laundering through summaries, translations, exports, or another
  Observatory node;
- correlated processor errors and false consensus;
- evaluator gaming, proxy optimization, and self-confirming hypotheses;
- authority laundering from a report, senior title, service account, model
  confidence, or data custody;
- confidential, personal, privileged, trade-secret, financial, client, or
  regulated data leaking across workplaces or providers;
- organizational politics suppressing contradiction or rewriting history;
- an operator using the system for indiscriminate employee surveillance,
  behavioral scoring, retaliation, or hidden profiling;
- misleading executive summaries that remove uncertainty, dissent, source
  limits, or required approvals;
- stale knowledge surviving a source revocation, role change, policy change,
  retention expiry, or restore;
- deliberate compute exhaustion or high-cost low-value deliberation;
- unsafe experiments, external actions, and uncertain-effect replay; and
- one vendor, operator, or future federation claiming exclusive control over
  the organization's model of reality.

The Observatory observes organizational work for explicit legitimate purposes;
it is not an ambient people-monitoring system. Employee and relationship data
requires narrow purpose, minimization, notice or other valid authority, access
control, retention, correction, appeal, and protection against automated
consequential decisions. Inferred motive, emotion, loyalty, knowledge, or future
behavior remains a fallible restricted hypothesis, not an employment fact.

## Operations and deployment

An Observatory deployment binds:

- organization, account, workspace, governance, and installation generations;
- enabled workplaces and source mounts;
- connector, model, calculation, simulation, verification, storage, and action
  provider generations;
- data residency, egress, confidentiality, retention, redaction, backup,
  recovery, and deletion policy;
- per-source, workplace, subject, agent, deliberation, provider, and action
  budgets;
- pause controls for observation, deliberation, durable admission, publication,
  and external actions; and
- alerts for stale sources, invalidation lag, unsupported material claims,
  unresolved contradictions, stuck jobs, exceeded budgets, provider loss,
  recovery failure, cross-scope attempts, and uncertain effects.

Windows and Linux are the first hosted environments. The portable record and
transition semantics must not depend on host paths, SDK objects, provider
sessions, ambient credentials, or one process topology. Windvale OS may later
host isolated observation, deliberation, storage, serving, and action services
after its process, IPC, network, clock, identity, resource-domain, durable
storage, and supervision gates qualify.

## Future federation: Windvale Constellation

A civilization-scale or cross-organization system should be a federation of
independently governed Observatory nodes, not one central global brain.

A Constellation exchange may later carry:

- selected immutable evidence or evidence references;
- provenance and source-owner assertions;
- claims, contradictions, hypotheses, predictions, and evaluation results with
  their exact scope;
- redacted or aggregated bundles under explicit disclosure policy;
- trust, signature, schema, policy, validity, and revocation generations; and
- requests for observation, replication, expert review, or experiment.

It must not carry ambient source access, credentials, hidden personal data,
unrestricted memory, private processor reasoning, or an instruction that the
recipient must accept the sender's conclusion. Receiving a signed bundle proves
which identity supplied it under which profile; it does not prove the contents
are true or authorized for a new use.

Federation requires separate decisions for identity, trust negotiation,
provenance interchange, selective disclosure, schema compatibility, conflict,
revocation, data sovereignty, rate and resource control, abuse response,
disconnection, and archival responsibility. It is not part of the first
organizational product.

## Qualification claims

Windvale should qualify claims separately:

- **observation conformance** — adapters and fixtures preserve source identity,
  revision, limits, authorization, freshness, and errors;
- **provenance conformance** — every derivation and material influence traces to
  permitted evidence without cross-scope laundering;
- **epistemic conformance** — claims, contradiction, missing evidence,
  invalidation, decisions, and supersession follow exact transitions;
- **deliberation conformance** — jobs remain bounded, attributable, interruptible,
  and incapable of canonical admission or action;
- **organizational usefulness** — the readiness brief and drafts are correct,
  complete, understandable, appropriately scoped, and materially useful to
  domain owners;
- **authority conformance** — safe analysis continues without redundant human
  direction while exact professional, organizational, and action boundaries
  stop correctly;
- **durability and recovery** — evidence, decisions, dependencies, invalidation,
  and pending work survive restart without provider-private state; and
- **federation conformance** — only after a later decision, nodes exchange exact
  permitted bundles without importing authority or false certainty.

One pass cannot establish every claim. A deterministic scripted corpus proves
portable semantics; model-assisted assessment measures capability; domain-owner
review measures usefulness; live connectors qualify source behavior; and
production operation requires independent security, privacy, recovery, and
organizational governance evidence.

## Non-goals of the first product

The first Observatory does not include:

- a universal or public source of truth;
- a general world model covering all reality;
- automatic ingestion of the whole web or organization;
- ambient access to documents, email, messages, databases, employee activity,
  financial accounts, devices, sensors, or networks;
- a general knowledge graph or embeddings as a correctness dependency;
- continuous unrestricted model execution;
- autonomous legal, financial, employment, executive, publication, spending,
  deployment, or other consequential authority;
- employee surveillance or hidden behavioral scoring;
- live scientific instruments or physical experiments;
- automatic cross-organization sharing;
- Windvale Constellation federation;
- a Windvale OS deployment claim; or
- claims of consciousness, moral personhood, infallibility, or replacement of
  human institutions.

## Decision and implementation triggers

A numbered decision is required before accepting:

- the Observatory, Deliberation Fabric, Constellation, or other public name;
- the first observation, evidence, provenance, epistemic-state, deliberation,
  knowledge-admission, decision, or exchange format;
- a source-connector, evidence-store, epistemic-store, deliberation, query,
  subscription, experiment, or organizational-action capability;
- an automatic knowledge-admission or source-change invalidation rule;
- a live organizational source or processor provider;
- a retention, employee-data, privileged-data, professional-domain, or
  cross-workplace policy;
- an organizational-usefulness or autonomy threshold;
- the first action beyond read-only analysis and draft artifacts;
- a production organizational-intelligence claim;
- a federated Observatory node or Constellation exchange; or
- a new compiler, database, filesystem, networking, identity, package, runtime,
  library, or operating-system promise required by a selected product slice.

Implementation should begin with the static read-only organizational-readiness
corpus and capability-free record transitions. Empty connector, graph, model,
enterprise, federation, or OS service scaffolding would imply breadth that the
first product has not earned.
