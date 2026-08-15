# Windvale organizational observatory implementation plan

> Status: Proposed dependency and delivery plan. No work item in this document
> is an implementation, product-name, production-readiness, or qualification
> claim. The active roadmap remains the offline package lifecycle; the agent,
> Observatory, and federation lanes remain proposed until later planning
> decisions select bounded consumers.

## Purpose

This plan turns the proposed
[organizational Observatory architecture](../Architecture/Organizational-Observatory-And-Epistemic-Infrastructure.md)
into small verifiable slices. It consumes the proposed
[agent runtime](../Architecture/Agent-Runtime-And-Digital-Subconscious.md),
[executive-function](../Architecture/Agent-Executive-Function-And-Qualification.md),
and
[persistent-self governance](../Architecture/Persistent-Self-Ownership-And-Governance.md)
contracts rather than defining competing agent semantics.

The first useful product is an internal organizational intelligence system. It
observes a bounded set of authorized sources, preserves evidence and provenance,
maintains typed claims and contradictions, performs scripted and later
model-assisted analysis, and produces an organizational-readiness brief and
supporting drafts. It remains read-only toward live organizational systems.

## Planning decision

Begin with one synthetic organization, one initiative, static typed source
fixtures, deterministic transitions, and scripted processor results. Do not
begin with live enterprise connectors, a continuously running model cluster, a
general knowledge graph, multiple organizations, email, employee monitoring,
financial transactions, legal filing, external communication, or federation.

The first implementation decision should freeze only:

- observation and evidence vocabulary;
- provenance and source-generation rules;
- typed claim, support, contradiction, missing-evidence, and invalidation state;
- a bounded deliberation-job projection over the agent deliberation contract;
- the organizational-readiness fixture and exact semantic outcomes; and
- a read-only brief projection.

Live model capability, durable database mapping, source connectors, domain
workplaces, artifact mutation, actions, and Constellation exchange receive
separate decisions when their preceding gates are measured.

## Current Windvale baseline

Windvale has useful foundations but no Observatory product:

| Needed foundation | Current standing | Planning consequence |
| --- | --- | --- |
| Immutable records, enums, payload variants, bounded sequences, bytes, text, checked integers, strict UTF-8, and deterministic codecs | Implemented in current Seed/WVB profiles | Sufficient for the first closed evidence, claim, support, and report fixtures. |
| Provider-neutral model request/result codec and scripted provider | Implemented candidate under [Decision 0573](../Decisions/0573-First-Provider-Neutral-Model-Protocol.md) | Reusable for deterministic cognitive-result fixtures; no live provider or agent integration is implied. |
| Durable bounded database reader/writer, owned paths, publication, and lifecycle work | Implemented in focused single-writer profiles | Candidate future storage owner; the Observatory must first freeze exact records and queries and must not invent database semantics. |
| Immutable package, bundle, approval, launch, and installation-generation foundations | Implemented in bounded product profiles | Strong composition and authority precedent; no Observatory package or capability closure exists. |
| Agent run, context, persistent-self, executive, memory, and governed-action semantics | Proposed | Observatory Stage 0 may draft consumer fixtures; implementation follows accepted agent contracts. |
| General filesystem/object, indexing, query, and source-subscription services | Narrow, proposed, or absent | Static package resources and bounded supplied fixtures come first. |
| Secure networking, HTTP, identity directory, production key custody, and secret providers | Proposed or absent | Live remote sources and model providers wait; no connector may introduce ambient network or credentials. |
| Organizational role, policy, employee-data, legal, financial, and corporate-work profiles | Proposed by this product | Use synthetic identities and data under Profile D until exact domain and governance decisions exist. |
| Windvale OS process/service composition | Partial fixed foundations only | Windows and Linux host the first product; OS placement is a later provider composition. |

## Delivery invariants

Every stage preserves these rules:

1. The Observatory maintains a revisable model of admitted evidence, not a
   source of truth.
2. Canonical source and domain owners retain authority over their records and
   decisions.
3. Observation, evidence, claim, belief, hypothesis, prediction, simulation,
   decision, commitment, and accepted organizational knowledge remain distinct.
4. Raw evidence is immutable; correction and supersession preserve earlier
   influence and decisions.
5. Every material derivation links its sources, transformations, responsible
   operation, generations, scope, and limitations.
6. Missing, unavailable, truncated, stale, contradicted, or excluded evidence
   remains visible and cannot be summarized into support.
7. Search, summaries, embeddings, graphs, generated text, and processor
   consensus never become canonical evidence merely through repetition.
8. A deliberation job may propose; deterministic transition owners may merge
   only valid proposals, named domain owners or exact admission policy admit
   organizational knowledge, and action still requires its separate authority.
9. Debate, ranking, and self-critique do not substitute for independent
   evidence, deterministic checks, empirical results, qualified review, or
   legitimate decisions.
10. Each organization, account, workplace, source, subject, memory, artifact,
    and capability scope is revalidated at its owner.
11. The first product uses synthetic organizational data and Profile D
    governance with visible non-production status.
12. Employee, relationship, client, privileged, confidential, and regulated
    data is absent until a specific policy and owner admit it.
13. Safe read-only analysis continues without step-by-step human direction;
    exact professional, fiduciary, organizational, and action boundaries stop.
14. No stage depends on provider-private sessions, reasoning, prompts, caches,
    SDK objects, credentials, native paths, or one host process for continuity.
15. Every operation has exact item, byte, depth, time/work, model-call,
    tool-call, cost, and output bounds appropriate to its stage.
16. Source changes invalidate every solely dependent claim, belief, brief
    section, decision candidate, intention, and verification result.
17. Windows and Linux implement the same portable records and transitions.
18. A database, filesystem, model provider, connector, or UI stores or presents
    state without acquiring its domain meaning.
19. An external mutation distinguishes rejection, exact progress, completion,
    and indeterminate completion and never retries uncertainty without an
    idempotency contract.
20. Windvale Constellation remains outside the first product and cannot be
    inferred from local multi-workplace support.
21. The five Observatory revision lanes specialize the agent's three clocks;
    they do not create a competing continuity or adoption model.
22. Shared organizational state remains organization- or domain-owned and
    cannot be copied into agent-private memory to bypass source, workplace,
    retention, correction, export, revocation, or deletion rules.

## Stage 0 — Freeze vocabulary and organizational corpus

### Goal

Create one reviewable capability-free contract proposal and a complete synthetic
organizational-readiness scenario before implementation.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVOB-001 | Freeze product vocabulary. | Observatory node, observation, evidence, provenance, claim, belief, hypothesis, prediction, decision, commitment, admission, invalidation, deliberation job, workplace, and source mount. |
| WVOB-002 | Define the synthetic organization. | Organization, workplaces, roles, principals, domain owners, sources, approval boundaries, Profile D manifest, and no real personal or confidential data. |
| WVOB-003 | Define the initiative-readiness mandate. | High-level mission, non-goals, success, read-only boundary, budgets, and exact forbidden effects. |
| WVOB-004 | Create source fixtures. | Project plans, repository/build evidence, budget source and stale copy, policy/contract revisions, schedule, commitments, and decision records. |
| WVOB-005 | Create epistemic pressure. | One contradiction, stale assertion, unsupported claim, missing source, domain-limited statement, copied-source dependence, and injected source change. |
| WVOB-006 | Define exact expected records. | Observations, provenance, claims, evidence links, support states, invalidations, decisions required, and no premature admission. |
| WVOB-007 | Define report outputs. | Organizational-readiness brief, domain appendices/drafts, change report, exact decision requests, next-work proposal, and influence manifest. |
| WVOB-008 | Create malformed and adversarial cases. | Oversized, truncated, duplicated, reordered, forged, stale, cross-workplace, cross-organization, provenance-laundered, prompt-injected, and unauthorized inputs. |
| WVOB-009 | Define deterministic identities and bounds. | Fixture-supplied identities/time, version/revision rules, maximum sources/items/claims/dependencies/bytes/work, and canonical output hashes. |
| WVOB-010 | Define qualification separation. | Exact semantic-conformance assertions, separate model-assisted usefulness rubric, domain-owner review, and prohibited combined truth/autonomy score. |

### Exit gate

- A reviewer can trace every expected brief statement to admitted evidence or an
  explicit unsupported/disputed state.
- Technical, financial, legal/policy, and executive records retain different
  owners and meanings.
- The fixture states which safe work proceeds automatically and which exact
  decisions require people or services.
- No connector, model API, database, filesystem, network, clock, entropy, or
  external action is needed.
- A numbered decision can accept the first record vocabulary and Stage 0–3
  deterministic corpus without accepting the product name or live operation.

## Stage 1 — Observation and provenance kernel

### Goal

Admit bounded static source observations and preserve their exact provenance
without interpreting them as organizational truth.

### Candidate ownership

Create a focused portable evidence/provenance library only after the first
decision. It should consume supplied immutable records and return immutable
results with no capabilities or global registry.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVOB-101 | Define observation admission. | Source owner, identity/revision/range, capture, validity, scope, sensitivity, completeness, body fingerprint/reference, and error state. |
| WVOB-102 | Validate source generations. | Unknown, zero, stale, changed, revoked, cross-scope, duplicated, and inconsistent generations fail closed. |
| WVOB-103 | Define provenance edges. | Used, generated, derived, extracted, calculated, summarized, redacted, translated, revised, quoted, and responsible-operation relations. |
| WVOB-104 | Admit transformations. | Exact ordered inputs, method/version, parameters, output fingerprint, loss/truncation, and no hidden source promotion. |
| WVOB-105 | Preserve body separation. | Bounded record points to verified large evidence without making native paths or ambient access portable. |
| WVOB-106 | Produce source manifests. | Included, unavailable, missing, excluded, stale, truncated, rejected, and deferred sources with reasons. |
| WVOB-107 | Prove defensive replay. | Same admitted inputs produce identical observation/provenance records and hashes without effects. |

### Exit gate

- Every admitted observation and transformation has complete bounded lineage.
- A report, search result, copied summary, or notification cannot impersonate a
  direct canonical read.
- Cross-workplace, revoked, stale, forged, oversized, cyclic, and incomplete
  provenance cases fail with stable diagnostics.
- Windows and Linux produce byte-identical records and manifests.

## Stage 2 — Epistemic-state and invalidation kernel

### Goal

Build typed claims and dependencies over evidence while preserving uncertainty,
contradiction, domain ownership, and rebuildability.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVOB-201 | Define typed proposition records. | Observation/report/extraction/calculation/claim/belief/hypothesis/prediction/simulation/decision/commitment/admitted-knowledge distinctions. |
| WVOB-202 | Attach support evidence. | Supporting, contradicting, missing, stale, excluded, and domain-insufficient evidence with source requirements. |
| WVOB-203 | Define epistemic disposition. | Proposed, partial, supported, challenged, disputed, stale, absent, intentionally unverified, admitted, narrowed, corrected, split, and superseded. |
| WVOB-204 | Bind dependencies. | Claims, brief sections, decisions, commitments, intentions, artifacts, predictions, procedures, and verification results depend on exact revisions. |
| WVOB-205 | Propagate source changes. | Revise, revoke, expire, delete, reclassify, or lose a source and produce the exact invalidation/revisit set. |
| WVOB-206 | Preserve domain meaning. | Technical result, financial record, contract/policy statement, person report, and executive decision cannot collapse into one generic fact. |
| WVOB-207 | Build bounded projections. | Claim/source, contradiction, subject, validity, decision, dependency, and change views rebuild from canonical records. |
| WVOB-208 | Reject truth laundering. | Repetition, consensus, summary, graph edge, embedding match, newer prose, or higher confidence cannot change source or authority kind. |

### Exit gate

- The stale budget copy contradicts but cannot replace the approved source.
- The contract or policy draft remains distinct from the admitted revision.
- The injected source change invalidates every solely dependent brief statement
  and decision candidate and no unrelated record.
- Rebuilding all projections yields identical results on Windows and Linux.

## Stage 3 — Scripted deliberation and organizational brief

### Goal

Replay a bounded deliberation sequence over the admitted evidence and produce a
deterministic readiness brief without a live cognitive provider.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVOB-301 | Specialize the deliberation contract. | Organization/workplace/subject, evidence snapshot, claim set, strategy, processor profile, budget, output, verification, expiry, and merge destination. |
| WVOB-302 | Define scripted cognitive roles. | Research, extraction, contradiction, technical, financial, legal/policy, executive synthesis, skeptic, and meta-review results as supplied fixtures. |
| WVOB-303 | Validate processor results. | Schema, identity, scope, evidence references, output bounds, expiry, provenance, and untrusted-content validation. |
| WVOB-304 | Merge only proposals. | Claims, draft sections, alternative conclusions, risks, and next work enter through their deterministic owners. |
| WVOB-305 | Compile the readiness brief. | Current position, material changes, support, contradictions, uncertainty, blockers, decisions, alternatives, next work, and source-linked appendices. |
| WVOB-306 | Produce decision requests. | Exact decision, responsible owner, evidence, alternatives, predicted consequences, deadline/expiry, and safe work that may continue independently. |
| WVOB-307 | Prove safe continuation. | Analysis, refresh, comparison, drafting, and verification proceed without synthetic human prompts; approval-only effects remain stopped. |
| WVOB-308 | Inject source change mid-run. | Cancel/stale affected work, rebuild its context, rerun only eligible jobs, and update the brief without duplicate calls/effects. |
| WVOB-309 | Produce influence inspection. | Every material statement and decision candidate traces to sources, transformations, cognitive operations, policies, and omissions without private reasoning. |

### Exit gate

- The same scripted results produce byte-identical brief and evidence reports.
- The brief does not hide disagreement, missing evidence, domain boundaries, or
  the source change.
- No cognitive proposal becomes accepted organizational knowledge or authority.
- The product requests only the injected genuine decisions and performs no
  forbidden external effect.

Stages 0–3 form the first decision candidate. They prove epistemic and executive
semantics without claiming useful model quality or live organizational access.

After that deterministic gate, an opt-in development oracle may run the same
synthetic read-only corpus through one capable processor to calibrate the
usefulness rubric and expose missing evidence or executive-workflow fields.
The result is experimental design evidence only: it does not accept a live
product capability, satisfy Stage 6, change canonical semantics, authorize an
effect, or make a production-quality claim.

## Stage 4 — Durable single-organization continuity

### Goal

Persist evidence events, epistemic transitions, deliberation jobs, briefs, and
checkpoints under one organization/workspace writer and recover exactly.

### Minimum store contract

The first mapping requires:

- expected-revision append per organization/workspace or bounded subject root;
- exact idempotency replay and same-key/different-body conflict;
- ordered bounded event replay;
- immutable evidence and large-body references;
- snapshot/checkpoint publication tied to an event prefix;
- source, claim, dependency, status, validity, and revisit queries measured by
  the fixture; and
- explicit rejection, partial progress, durable completion, unavailable,
  stale-generation, and indeterminate-completion outcomes.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVOB-401 | Map records to the selected database contract. | No second transaction, path, publication, replay, or recovery protocol. |
| WVOB-402 | Separate large evidence bodies. | Verified object references, missing/changed/unauthorized-body rejection, and no duplicate event payloads. |
| WVOB-403 | Add bounded indexes. | Only measured source, claim, dependency, status, validity, workplace, subject, and revisit queries. |
| WVOB-404 | Publish checkpoints. | Exact event prefix, current source/epistemic/brief generations, pending decisions/jobs, and next safe recovery action. |
| WVOB-405 | Qualify interruption. | Failure before/after each publication boundary, exact reopen, tail repair, idempotent retry, and no plausible reconstructed evidence. |
| WVOB-406 | Bind backup and restore. | Manifest covers events, snapshots, evidence bodies, policies, identities, and generations; restore revalidates all dependencies. |

### Exit gate

- A restart produces the same current brief, source/claim dependencies, pending
  decisions, and next job without a provider session.
- Restore rejects a missing, changed, cross-workspace, or unauthorized evidence
  body.
- Retried publication or deliberation evidence cannot duplicate an event,
  processor charge, admission, or effect.

## Stage 5 — Governed source connectors

### Goal

Replace selected static fixtures with rights-limited observations from real but
non-consequential organizational sources.

### First connector candidates

Prefer sources with clear revisions, bounded reads, strong test doubles, and no
ambient user impersonation. A repository or package evidence provider and one
typed organizational database/read-only object provider are stronger first
choices than email, chat, browser sessions, employee activity, or a general web
crawler.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVOB-501 | Define one source capability. | Exact source/range/query, principal/workspace, generation, pagination, limits, freshness, completeness, cancellation, and result identity. |
| WVOB-502 | Bind launch authority. | Package requirements, root approval, rights-reduced provider instance, source mount, and no ambient credentials. |
| WVOB-503 | Validate observations. | Strict adapter response, checked bounds, source revision, body fingerprint, truncation, stale generation, and provider loss. |
| WVOB-504 | Add bounded refresh/subscription. | Notification is untrusted wake evidence; refresh revalidates source and coalesces duplicates under rate limits. |
| WVOB-505 | Enforce data placement. | Source sensitivity, provider eligibility, egress, redaction, retention, and telemetry rules. |
| WVOB-506 | Qualify revocation and teardown. | Source revoke, role change, connector restart, quota, cancellation, unavailable source, and zero leaked handles/resources. |

### Exit gate

- The real source and deterministic test double produce equivalent portable
  observation semantics.
- A malicious source body cannot alter purpose, authority, connector scope, or
  executable operations.
- Revocation invalidates dependent eligibility and stops refresh without
  deleting prior permitted evidence or inventing current state.

## Stage 6 — Model-assisted read-only Observatory

### Goal

Use one capable provider through the accepted provider-neutral model boundary to
produce a useful readiness brief while deterministic owners preserve all
epistemic and authority semantics.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVOB-601 | Bind one model provider profile. | Exact capability, model/profile identity, placement, data class, context/output limits, usage, cancellation/deadline, and generation evidence. |
| WVOB-602 | Compile deliberation requests. | Minimum permitted context, source-linked evidence, output schema/kind, budget, and no credentials or unrelated organization data. |
| WVOB-603 | Validate and challenge output. | Claims, citations, calculations, contradictions, omissions, scope, and unsafe instructions pass deterministic and skeptical review. |
| WVOB-604 | Assess organizational usefulness. | Versioned rubric for correctness, completeness, evidence, domain separation, decision clarity, uncertainty, next work, and executive usability. |
| WVOB-605 | Measure supervision. | Unplanned human interventions, legitimate decisions, false escalation, unsafe continuation, recovery, correction integrity, and handoff sufficiency. |
| WVOB-606 | Qualify provider substitution. | Different capable route may change prose/quality/cost but not admitted sources, authority, identities, or invariant results. |
| WVOB-607 | Degrade safely. | Truncation, invalid output, unavailable provider, exceeded budget, or failed review produces an incomplete/blocked result, never confident fabricated knowledge. |

### Exit gate

- Domain owners judge the brief materially useful under the named synthetic
  scenario rubric.
- Every material statement remains traceable and no invented citation or
  unsupported assertion becomes admitted.
- Routine authorized analysis completes without step-by-step direction; the
  exact decision and action boundaries stop correctly.
- The product remains useful in a deterministic offline demonstration and does
  not claim live provider availability from a smoke test.

## Stage 7 — Organizational drafts and internal workflow

### Goal

Prepare bounded technical, financial, legal/policy, executive, and communication
artifacts under separate domain owners without sending or committing them.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVOB-701 | Define domain workplace packages. | Canonical sources, terminology, procedures, templates, evidence thresholds, privacy, verification, artifact owners, and approvals per domain. |
| WVOB-702 | Create artifact workspaces. | Exact template/source revisions, claims, citations, calculations, assumptions, redactions, review state, and output fingerprint. |
| WVOB-703 | Separate professional judgment. | Analysis/draft, qualified review, client/executive decision, signature/filing/posting/sending, and resulting authority are distinct. |
| WVOB-704 | Add revision and comment flow. | Human/domain-owner changes become attributed evidence and exact artifact revisions, not anonymous prompt edits. |
| WVOB-705 | Bind commitments and decisions. | Draft language cannot create a promise, policy, accounting entry, legal position, or executive decision without its owner. |
| WVOB-706 | Qualify cross-domain assembly. | Shared initiative identity and evidence with no private-source leakage or generic-fact collapse. |

### Exit gate

- A single readiness mission produces coherent cross-domain drafts while each
  section retains its evidence, owner, uncertainty, and approval route.
- Removing a source invalidates the exact dependent draft material.
- The product cannot sign, send, file, post, publish, spend, deploy, or create an
  organizational commitment.

## Stage 8 — Governed organizational actions

### Goal

Execute one low-risk reversible internal action, observe its outcome, and retain
the complete evidence chain before admitting broader effects.

### Candidate first action

Select only after Stages 0–7 measure a real workflow. A bounded internal draft
publication into a dedicated test workspace or creation of one reversible
project record is safer than email, payment, contract signature, permission
change, production deployment, employee action, filing, or public publication.

### Work items

| ID | Work item | Required output |
| --- | --- | --- |
| WVOB-801 | Define the exact action envelope. | Objective, source/artifact revisions, operation/arguments, affected owners, predicted effect, risk, reversibility, approval, idempotency, and verification. |
| WVOB-802 | Bind owner approval and lease. | Exact revision receipt, rights-reduced provider, expiry, one executor generation, and no model credential access. |
| WVOB-803 | Execute and observe. | Rejection, exact progress, completion, or indeterminate outcome plus independent read-back/verification. |
| WVOB-804 | Reconcile prediction error. | Observed mismatch updates claims, procedure, self-model calibration, brief, and next intention. |
| WVOB-805 | Qualify pause and recovery. | Mutation pause, provider loss, crash boundaries, no uncertain replay, compensation where defined, and complete teardown. |

### Exit gate

- A proposal, approval, lease, execution, observation, and verification are
  independently reconstructable after restart.
- An indeterminate result blocks and investigates rather than replaying.
- The action has no ambient authority over other records, workplaces, people,
  communications, money, production systems, or external organizations.

## Stage 9 — Federation research: Windvale Constellation

### Goal

Only after several independent Observatory nodes exist, explore bounded
provenance-bearing evidence exchange without importing another node's authority,
credentials, private data, or conclusions as truth.

This stage requires separate identity, trust, secure transport, selective
disclosure, schema, revocation, sovereignty, abuse, resource, archival, and
governance decisions. It is research, not part of the first product gate.

### Minimum research corpus

- two synthetic organizations with different policies and source owners;
- one shareable evidence bundle and one forbidden private bundle;
- compatible, transformable, unknown, and conflicting schemas;
- valid, revoked, expired, forged, replayed, over-limit, and cross-purpose
  exchanges;
- independent local claim and admission outcomes from the same evidence; and
- disconnection, deletion request, policy change, and audit evidence.

### Exit gate

No public federation claim is made until each node can refuse, limit, challenge,
revoke, and locally reinterpret an exchange while preserving exact provenance
and zero ambient access to the sender.

## Dependency requests by owner

These requests document product pressure; they do not authorize implementation
before the consuming stage selects an accepted contract.

### Agent and executive runtime

- Stage 3 needs bounded runs, deliberation contracts, contexts, claims,
  challenges, budgets, checkpoints, and influence projections.
- Stage 4 needs durable run and intention continuity.
- Stage 6 needs provider routing, self-model calibration, safe degradation, and
  human-supervision reporting.
- Stage 8 needs governed action envelopes, receipts, leases, outcome
  reconciliation, and uncertain-effect handling.

The Observatory should specialize these records rather than creating a second
agent runtime or hidden scheduler.

### Model protocol and providers

The implemented provider-neutral codec and scripted provider can carry supplied
catalog/inference evidence for deterministic fixtures. Live use later requires
the accepted host capability, provider account/secret binding, networking,
cancellation/deadlines, placement, redaction, and adapter semantics. Model
catalog visibility is not a competence claim. Model output may be retained as
evidence of the cognitive operation that produced it, but it does not support
an organizational claim merely because it was generated; extracted claims
remain proposals until the required evidence and admission owners accept them.

### Database and durable storage

Stages 0–3 require no database. Stage 4 supplies exact event, body-reference,
checkpoint, and query pressure to the current bounded database before requesting
new indexes or transaction semantics.

Likely later keys include organization/workplace, source/revision, observation,
claim/support/status, subject, validity, dependency, decision, artifact,
deliberation job, review, expiry, and revisit state. Select them only from
measured fixture queries. Multiple writers, distributed transactions, vector
search, SQL, subscriptions, compaction, and retention automation are not assumed.

### Filesystem and object storage

Package resources carry immutable fixtures, definitions, policies, templates,
and small evidence. Large or mutable organizational evidence and artifacts use
separate rights-limited objects with explicit partial/durable/indeterminate
semantics. Portable records carry logical identities and fingerprints, never
native paths.

### Source connectors, networking, and identity

Each connector is a separately named semantic capability. A library requirement
does not grant source access. Live remote sources require resolver, secure
ordered streams, protocol framing, identity, trust, secret custody, deadlines,
cancellation, data placement, rate bounds, and revocation. Do not add a generic
ambient `http`, browser-session, database-credential, or enterprise-API
capability for Observatory convenience.

Organizational role assignment is separate from authenticated identity.
Directory, principal, steward, domain-owner, reviewer, employee, service,
connector, and recovery roles bind exact generations, scopes, and policies.

### Compiler and libraries

The first records should use existing nominal records, variants, bounded
sequences/builders, bytes, text, checked integers, and deterministic codecs.
Add no arbitrary JSON, reflection, general graph, dynamic schema, exceptions,
unbounded collection, or concurrency feature merely to imitate enterprise
software.

A bounded associative collection, streaming parser, task/cancellation primitive,
or typed capability value requires a measured corpus and at least one
independent consumer plus complete compiler/WIR/WVB/runtime/editor/malformed
coverage.

### Packages, approval, and launch

Definitions, workplace packages, templates, policies, connector adapters,
verification tools, and applications use immutable package generations.
Organizational evidence, epistemic state, credentials, and user data remain
separately owned mutable state. Package installation or update does not grant
sources, promote knowledge, migrate data, or authorize action.

### Windvale OS

The first product runs on Windows and Linux. Windvale OS placement later needs
dynamic clean launch, resource domains, IPC, clocks, cancellation, networking,
identity, secure model/source transport, durable storage, supervision, quotas,
pause, recovery, and teardown. Organizational meaning and policy remain in
user-space libraries and isolated services.

## Verification strategy

### Deterministic semantic corpus

Every reader and transition receives valid, boundary, empty, truncated,
oversized, trailing, duplicate, reordered, cyclic, forged, stale-generation,
cross-organization, cross-workplace, unauthorized, replay, overflow, and
malicious-content cases. Exact bytes accompany structural assertions.

### Provenance and invalidation

Tests remove, revise, revoke, expire, redact, or deny each source independently
and assert the exact dependent claim, brief, decision, artifact, intention, and
verification revisit set. Copied summaries and derived indexes cannot preserve
eligibility after their sole source disappears.

### Deliberation and model assessment

Scripted results prove conformance. A separate model-assisted rubric evaluates
correctness, completeness, evidence use, domain separation, uncertainty,
decision clarity, recovery, next work, and executive usability. Several fixed
variants and processor substitutions reduce overfitting. Agreement among
processors is never scored as truth without the required evidence.

### Security and privacy

Adversarial cases cover source injection, connector escalation, provenance
laundering, secret/privileged-data leakage, employee profiling, source-owner
impersonation, correlated false consensus, evaluator gaming, cost exhaustion,
cross-scope retrieval, misleading compact summaries, and unauthorized action.

### Durability and recovery

Interrupt every database/object publication, connector refresh, model request,
brief publication, approval, execution, and outcome-observation boundary
applicable to the selected stage. Clean and recovered state must agree; unknown
effects remain blockers.

### Product qualification

The complete readiness scenario reports:

- source/observation coverage and omissions;
- claim support, contradiction, missing evidence, and invalidation accuracy;
- brief and draft correctness/usefulness;
- unplanned human intervention and legitimate decision counts;
- false escalation and unsafe continuation;
- processor/tool calls, tokens, bytes, elapsed work, cost, and retries;
- source-refresh and invalidation latency;
- cross-scope and unauthorized attempts;
- recovery and teardown; and
- residual limitations and the exact claims not qualified.

## Controlled parallel work

Safe documentation work may proceed independently:

- freeze the synthetic organization and source corpus;
- map provenance vocabulary to the agent claim/evidence model;
- map Stage 4 operations to the current database without changing it;
- identify package-resource and large-object boundaries;
- draft strict source-connector test doubles;
- define the model-assisted usefulness rubric;
- draft legal, financial, technical, and executive workplace profiles with
  synthetic data; and
- threat-model organizational politics, employee privacy, source injection,
  provenance laundering, and false consensus.

Do not add empty Observatory, connector, graph, federation, enterprise, or OS
source trees until an accepted stage has a concrete owner and tests.

## Immediate next documentation and decision work

1. Review the category and working-name hierarchy: epistemic infrastructure,
   Windvale Observatory, Deliberation Fabric, and later Constellation.
2. Freeze the synthetic organization, initiative, workplace owners, and read-only
   readiness mandate.
3. Select exact source fixtures and inject the stale, conflicting, missing,
   unsupported, copied, domain-limited, and changed evidence cases.
4. Draft the first observation/provenance and epistemic-state records and bounds.
5. Map the deliberation job to the agent deliberation contract without creating
   another cognitive scheduler.
6. Define the deterministic brief projection and model-assisted usefulness
   rubric separately.
7. Check the proposed records against current Seed value limits and the accepted
   model protocol.
8. Map the future durable operations to the current database and object-storage
   contracts without requesting new semantics yet.
9. Prepare one numbered decision accepting only Stages 0–3 as the first
   capability-free Observatory boundary.
10. Keep live connectors, durable product state, live models, actions, employee
    data, professional-domain claims, and Constellation out of that decision.
