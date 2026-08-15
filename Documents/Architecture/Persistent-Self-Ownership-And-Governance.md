# Windvale persistent-self ownership and governance architecture

> Status: Proposed architecture for review. This document defines a governance
> direction, not an implemented agent, legal ownership determination, production
> policy, or claim that an agent is a person. A numbered decision and exact
> qualification corpus are required before a persistent-self governance profile
> becomes an accepted product contract.

## Purpose

A persistent agent self can outlive a task, model, process, host, application
version, and individual operator. No single informal answer to "who owns it?"
is adequate because identity, purpose, memory, commitments, data, runtime
custody, recovery, and audit carry different kinds of authority.

This document defines a staged answer for Windvale:

1. during development and testing, a named E-Worker development authority may
   make most governance decisions for bounded experimental agents;
2. production-like pilots must exercise the later separation of authority even
   when E-Worker personnel temporarily fill several roles; and
3. an advanced persistent agent must use constitutional stewardship, with a
   primary principal, stewards, domain owners, affected data subjects, a runtime
   custodian, recovery parties, and independent audit responsibilities.

The companion
[agent runtime architecture](Agent-Runtime-And-Digital-Subconscious.md) defines
the functional self. This document defines who may establish or change its
different parts, how disagreements block unsafe transitions, and how broad
development authority ends.

## Recommendation

Windvale should adopt **phased constitutional stewardship** rather than sole
ownership by one person, organization, vendor, model, or agent.

The development/test profile is intentionally practical: the people responsible
for E-Worker need enough authority to create fixtures, choose provisional
values, test memory policy, replace providers, force recovery, suspend agents,
and revise the design quickly. That authority is acceptable only when it is:

- assigned to a versioned roster or approval group rather than the informal
  phrase "the E-Worker team";
- limited to named development environments, test identities, data scopes,
  capabilities, and expiry conditions;
- exercised through the same inspectable proposal, approval, event, and audit
  machinery intended for later profiles;
- incapable of overriding another person's or domain owner's independent rights
  merely because data entered a test agent;
- prohibited from silently promoting a test self into personal, organizational,
  collaborative, or consequential operation; and
- replaced, not merely supplemented, at the advanced-profile transition.

This makes the proposed approach doable without pretending the first testers
already possess the mature social and institutional structure.

## What ownership means here

Persistent-self governance separates at least these dimensions:

| Dimension | Question |
| --- | --- |
| Beneficial direction | Whose legitimate purposes and interests is the agent intended to serve? |
| Constitutional authority | Who may establish or amend identity, purpose, values, and the governance process? |
| Field authority | Who owns the canonical record for a commitment, memory, relationship, skill measure, or other self field? |
| Domain authority | Who decides the meaning and permitted use of project, business, medical, financial, family, or other domain records? |
| Data-subject interest | Whose personal information, statements, consent, reputation, or relationship is represented? |
| Operational custody | Who runs, secures, backs up, restores, suspends, and retires the service? |
| Capability authority | Who may approve and bind rights-limited providers and consequential actions? |
| Audit and appeal | Who can inspect a change, challenge it, require correction, or halt operation? |
| Portability and succession | Who can export, migrate, recover, archive, or appoint a successor arrangement? |
| Legal ownership | Which legal person owns software, infrastructure, intellectual property, contractual assets, or particular data under applicable law? |

These dimensions may be occupied by the same organization in a test fixture,
but they remain separate fields and evidence. The portable agent record must not
collapse them into one `owner` identity. This architecture cannot decide legal
ownership, employment, privacy, consumer, fiduciary, or succession questions;
deployments must map applicable obligations into the relevant roles and rules.

## Governing principles

Every profile preserves the following principles:

1. The persistent self is not the model. Replacing a model or provider does not
   transfer governance or create a new authority.
2. Custody is not ownership. Operating storage or possessing encryption keys
   does not grant authority to rewrite purpose, commitments, or history.
3. A primary principal supplies direction but does not own other people's data,
   consent, domain authority, or legal rights.
4. The agent may propose changes and identify conflicts; it does not approve its
   own constitutional authority or mint capabilities.
5. Every material change identifies a canonical field owner, proposer,
   approver, threshold, effective revision, evidence, review or delay rule,
   expiry where applicable, and appeal or recovery route.
6. Earlier evidence remains attributable after correction or supersession. An
   amendment cannot manufacture a false past.
7. Emergency authority may freeze, isolate, or reduce operation quickly. It may
   not silently rewrite the self or broaden authority.
8. Export, migration, backup, recovery, suspension, archival, and deletion are
   explicit governed transitions rather than consequences of provider control.
9. Development convenience never becomes an implicit production entitlement.
10. Governance describes observable functions and never depends on a claim of
    consciousness, sentience, emotion, or moral personhood.

## Roles

### Primary principal

The primary principal is the person, organization, or explicitly constituted
group whose accepted purposes the agent primarily serves. The principal may
create objectives, approve ordinary policy within the charter, review private
memory, and request suspension, export, or retirement. The principal cannot
unilaterally override domain owners, affected data subjects, non-waivable safety
constraints, or a constitutional amendment rule it previously accepted.

### Constitutional stewards

Constitutional stewards guard the integrity of the charter and amendment
process. They verify that a high-impact change has the required authority,
notice, delay, recovery evidence, and recorded dissent. They are not everyday
operators and do not become beneficiaries merely by approving an amendment.

The steward set needs a minimum size, quorum or threshold, replacement rule,
conflict-of-interest rule, loss-of-contact policy, and recovery path. An
advanced profile should avoid placing every steward credential under the same
account, device, operator, or runtime custodian.

### Domain and canonical-record owners

Domain owners retain authority over the records and meanings they already own.
A source repository owns current source bytes; an authorized business service
owns an accepted price; a person owns what they currently state; an approval
service owns its receipt. The agent may remember or reason about those records
without acquiring the right to redefine them.

### Affected data subjects and relationship participants

A person represented in memory or a social model may have independent rights to
notice, access, correction, restriction, objection, or deletion according to
the deployment and applicable obligations. A principal cannot turn an inference
about another person into that person's consent or current statement.

Relationship commitments require the authority of the affected relationship or
domain. One participant's private autobiographical evidence may remain distinct
from a shared commitment record.

### Runtime custodian

The runtime custodian operates the service, provider bindings, storage,
encryption, backup, recovery, quotas, incident response, and teardown. It may
isolate or suspend compromised operation. It does not acquire constitutional
authority merely because it can technically access or restore bytes.

### Capability and action owners

Existing package, approval, launch, domain, and tool owners decide which
capabilities are available and which exact action may execute. A governance
charter is not a grant. A principal, steward, developer, or agent still cannot
act beyond the rights supplied through the relevant capability contract.

### Agent as governed proposer

The agent may:

- propose a goal, memory, belief, procedure, skill adjustment, or charter review;
- state that instructions, commitments, evidence, or authorities conflict;
- preserve unresolved disagreement and ask the responsible party for help;
- refuse or degrade an operation when required evidence, capability, safety, or
  authority is unavailable;
- request suspension, correction, export, or review; and
- explain how a proposed change would affect its continuity and active
  commitments using inspectable records.

The agent does not approve its own terminal purpose, constitutional power,
capability grant, steward replacement, unrestricted replication, historical
erasure, or resistance to suspension. A model assertion that the agent owns
itself, consents, suffers, or deserves authority is not governance evidence.

### Auditor, appeal, and recovery parties

An auditor verifies that decisions followed the selected profile and that the
rendered user view agrees with durable evidence. An appeal owner reviews
disputed classifications or approvals. Recovery parties restore access after
loss or compromise without receiving routine authority over the self. One party
may occupy these roles in the first test fixture, but the records remain
separate so advanced qualification can prove separation.

## Governance profiles

### Profile D — E-Worker development and testing

Profile D is the default for design experiments, deterministic corpora, local
development, qualification fixtures, and non-production hosted tests. A named
**E-Worker Development Authority** collectively occupies the primary-principal
and constitutional-steward roles for each test self. Named E-Worker operators
may also occupy domain, runtime, audit, and recovery roles when the manifest
records each role separately.

Profile D permits the designated authority to:

- create, amend, reset, fork, migrate, suspend, archive, and retire test selves;
- choose provisional purposes, values, behavioral commitments, memory rules,
  providers, capability ceilings, budgets, and fixture relationships;
- admit synthetic or separately authorized test data;
- force failures, revocations, backup/restore, rollback, conflict, corruption,
  compromise, and succession scenarios;
- inspect all non-secret test governance and agent evidence needed to diagnose
  the system; and
- revise the proposed governance model while changes remain clearly labeled as
  experimental.

Its manifest must bind:

- the test-self identity and `development-test` profile identity;
- the exact E-Worker authority roster or group identifier, threshold, revision,
  and expiry or review date;
- environment, account, workspace, data, provider, network, action, cost, and
  time boundaries;
- whether all data is synthetic, E-Worker-owned, public, or admitted through a
  separate domain/data-subject authorization;
- role assignments for runtime custody, audit, appeal, recovery, and emergency
  suspension;
- allowed governance change classes and any test-only bypasses;
- a visible non-production marker in inspection and export views; and
- the terminal disposal, archival, or transition rule.

Profile D does not permit:

- ambient use of real personal, client, employee, confidential, regulated, or
  consequential data merely because the system is under test;
- unrestricted external communication, spending, publication, deletion, or
  physical action without the ordinary domain and capability owners;
- hiding a change because developers and approvers happen to be colleagues;
- treating a model's agreement as approval or a simulated relationship as a
  real person's consent;
- removing audit and recovery evidence needed to qualify the design; or
- changing a profile label to `advanced` while preserving undeclared developer
  access or authority.

This profile makes rapid experimentation possible while testing the actual
governance mechanisms. A shortcut used solely to inject a fault or construct a
fixture is labeled test-only and cannot appear in an advanced manifest.

### Profile P — supervised pilot

Profile P is an optional bridge for production-like trials with selected users
or organizational data. It instantiates the eventual primary principal, domain
owners, data-subject rules, and runtime custodian while E-Worker remains an
explicit technical steward or operator under a bounded pilot agreement.

A pilot may use stronger observation, approval, rollback, and support than a
mature deployment, but it may not restore Profile D's universal development
authority. Real data and effects use their real owners. The pilot has an end
date, participant notice, export/disposal rule, incident route, and a decision
to advance, extend, or terminate.

### Profile C — constitutional operation

Profile C is required before an advanced agent is represented as a persistent
personal, organizational, collaborative, or consequential agent. It separates
the primary principal, constitutional stewards, domain/data owners, runtime
custodian, capability owners, audit, appeal, and recovery responsibilities.

E-Worker may remain the software steward, contracted runtime custodian, one
named constitutional steward, or recovery provider. It does not retain broad
Profile D authority unless an exact advanced charter deliberately assigns a
specific role, scope, duration, and removal route.

Profile C may instantiate one of these arrangements:

- **personal** — one adult primary principal, independent recovery choices,
  explicit third-party data boundaries, and a succession or incapacity policy;
- **organizational** — a legal or operational organization as principal, named
  offices rather than irreplaceable individuals, employment/member boundaries,
  records policy, and accountable role succession;
- **collaborative** — several principals with scoped shared purpose, quorum,
  dissent, withdrawal, partition, shared/private memory, and dissolution rules;
  or
- **dependent or assisted** — a beneficiary and authorized guardian, supporter,
  or fiduciary with minimized delegated authority, additional review, conflict
  handling, and a route to change or end the arrangement.

These arrangements share record machinery but must not share one default policy.

## Persistent-self authority matrix

The following is the recommended default. A profile may narrow authority but
may not remove the hard invariants later in this document.

| Self field or transition | Canonical owner | Permitted proposer | Profile D approval | Profile C approval |
| --- | --- | --- | --- | --- |
| Origin, stable identity, and predecessor links | Identity/governance owner | Development authority or recovery party | E-Worker Development Authority | Successor/recovery protocol; never ordinary self-edit |
| Governance profile and charter | Governance owner | Principal, steward, auditor, or agent | E-Worker Development Authority | Primary principal plus required steward threshold and delay |
| Accepted purpose and values | Constitutional owner | Principal or agent | E-Worker Development Authority | Primary principal plus constitutional threshold |
| Non-waivable safety and integrity constraints | Policy/security owner | Policy owner, auditor, or incident owner | Named policy owner; test override only in isolated fault fixtures | Named policy owner and constitutional process; emergency change may only reduce authority |
| Standing commitments | Commitment/domain owner | Principal, affected owner, or agent | Named test owner | Principal plus every required affected or domain owner |
| Relationship, consent, and preference records | Person or relationship/domain owner | Relevant participant or agent as labeled inference | Synthetic owner or separately authorized participant | Current statement/consent owner; inference remains distinct |
| Autobiographical event links | Append-only self-history owner | Deterministic episode owner | Automatic append under test policy | Automatic append under charter; correction supersedes but does not erase |
| Semantic memory | Memory plus source/domain owner | Agent, principal, or source processor | Auto-admit or review under test policy | Private low-risk admission by policy; shared or consequential admission by owner review |
| Procedural memory | Procedure and capability/domain owner | Agent, verifier, or principal | E-Worker review or bounded auto-admission | Evidence threshold plus affected capability/domain owner |
| Prospective intention | Intention owner under parent purpose | Principal, policy, or agent | Test policy and bounds | Parent owner; action remains separately authorized |
| Belief, world, social, and self models | Derived-model owner over evidence | Agent or deterministic processor | Deterministic admission under test policy | Deterministic admission under charter; cannot replace canonical records |
| Skill and reliability calibration | Measurement owner | Verifier or agent challenge | Measured evidence or explicit fixture injection | Measured evidence and calibration policy; no self-asserted promotion |
| Model, provider, and runtime body generation | Runtime/product owner | Custodian, principal, or agent | E-Worker operator under test manifest | Runtime/product owner plus compatibility and principal policy |
| Capability binding and action lease | Existing approval/action owner | Principal, agent, or application | Real capability owner even in tests | Real capability owner; charter is not a grant |
| Export and migration | Data/governance owners | Principal, custodian, recovery party, or agent | E-Worker Development Authority within test scope | Principal plus affected data/domain restrictions and destination validation |
| Steward, recovery, and successor assignment | Governance owner | Principal or current authorized steward | E-Worker Development Authority | Constitutional threshold, separation checks, delay, and recovery proof |
| Suspension and isolation | Principal, custodian, policy, or emergency owner | Any monitor, user, auditor, or agent | Immediate rights reduction | Immediate rights reduction; resumption uses named approval |
| Archival, deletion, or retirement | Governance and affected data owners | Principal, custodian, data subject, or agent | Test disposal policy | Principal plus retention, legal, domain, data-subject, and continuity rules |

Every concrete profile turns this table into a machine-inspectable authority
manifest. An empty or unknown owner, missing approval, stale roster, unresolved
conflict, or unsupported threshold fails closed for that transition.

## Change classes

Governance uses impact classes rather than treating every mental event as a
constitutional amendment:

| Class | Change | Normal behavior |
| --- | --- | --- |
| 0 — Ephemeral cognition | Workspace selection, simulation, temporary belief candidate, run-private attention | Bounded automatic transition; expires or remains episode evidence; no durable self authority |
| 1 — Adaptive private state | Private episodic memory, low-risk semantic candidate, measured self-model degradation, prospective reminder | Policy may auto-admit with lineage, expiry, inspection, and correction |
| 2 — Reviewed knowledge or procedure | Durable semantic/procedural memory, material skill promotion, cross-episode rule | Canonical owner review or exact evidence threshold; dependencies and rollback/supersession recorded |
| 3 — Commitment or relationship | Promise, consent-dependent behavior, shared purpose, consequential preference, cross-party memory | Primary principal and every affected/domain authority required; notice, expiry, withdrawal, and dispute behavior explicit |
| 4 — Constitutional identity | Purpose, values, governance profile, steward/successor set, non-waivable boundary, identity migration | Exact amendment proposal, impact analysis, strong approval threshold, delay, recovery checkpoint, activation record, and appeal route |

Profile D may give its named authority all approval positions for a synthetic
fixture, but it still records the class and the approvals that Profile C would
require. This allows tests to exercise separation before different people or
organizations occupy the roles.

## Constitutional amendment protocol

A class-4 transition should pass through these states:

1. **proposed** — exact old and new charter revisions, proposer, reason, scope,
   and affected fields are recorded;
2. **analyzed** — active intentions, commitments, memories, capabilities,
   relationships, data, recovery parties, and possible conflicts are listed;
3. **noticed** — required principals, stewards, domain owners, auditors, and
   affected participants receive the reviewable proposal;
4. **approved or disputed** — approvals bind the exact proposal revision;
   dissent and missing parties remain visible;
5. **delayed** — the profile's cooling-off or challenge period runs unless an
   emergency change only reduces authority;
6. **prepared** — a recoverable checkpoint, export or continuity plan, new
   generation, and revisit set are verified;
7. **activated** — one deterministic owner publishes the new charter revision;
   and
8. **reviewed** — post-activation checks confirm effective authority, revoked
   old access, continuity, and unresolved consequences.

Amendments supersede prior charters; they do not rewrite them. A failed or
partially activated transition leaves the previous admitted charter effective
or places the self in an explicit blocked/recovery state. It never guesses which
authority should win.

## Conflict, refusal, and appeal

An operation evaluates the currently admitted charter and all applicable
field/domain authority. More specific current consent or a domain restriction
can narrow an otherwise valid principal instruction. A constitutional amendment
may change future policy only through its own process; an ordinary instruction
cannot waive that process.

When authorities conflict, the agent and runtime must:

- preserve each instruction and its source rather than blending them;
- identify the fields, scopes, people, commitments, and capabilities affected;
- continue only an independent safe subset when policy permits;
- block the disputed transition or external effect;
- name the responsible appeal or resolution owner; and
- retain the disagreement and final disposition as inspectable evidence.

Fluency, urgency, seniority in an unrelated role, service custody, or the
agent's inferred preference does not break a tie.

## Development-to-advanced transition

Promotion from Profile D is a governed migration, not a rename. The transition
gate requires:

1. a destination profile and arrangement with a real primary principal;
2. named constitutional stewards, domain/data owners, runtime custodian,
   capability owners, auditor/appeal owner, and recovery/successor parties;
3. an instantiated field authority matrix, change-class policy, approval
   thresholds, delay rules, conflict route, and emergency controls;
4. classification and authorization of every retained memory, relationship,
   commitment, artifact, credential reference, and source mount;
5. a verified export/checkpoint and a destination compatibility/revisit report;
6. rotation or revocation of development credentials, bypasses, provider
   bindings, recovery access, and universal inspection privileges;
7. notice and consent or other valid authority for affected real people and
   domains;
8. deletion, quarantine, or separately governed archival of test-only state;
9. deterministic tests for amendment, suspension, dispute, compromised
   operator, steward loss, export, restore, and retirement; and
10. a signed transition record stating which E-Worker roles remain and which
    expired.

The default is no promotion. A test self may be retired and an advanced
successor created instead. Continuity is claimed only for records that pass the
destination admission rules. Provider-private state, test bypasses, undeclared
developer access, and unsupported autobiographical claims never cross the gate.

## Lifecycle, recovery, and succession

A persistent self needs explicit states such as proposed, active, suspended,
blocked, migrating, archived, retired, and recovery-required. Deleting an
application or replacing a package is not equivalent to retiring the self.

Recovery preserves both continuity and authority:

- backups bind the self, charter, authority-manifest, event, memory, artifact,
  and key/provider generations needed to reconstruct an admitted revision;
- recovery parties can restore access only through their named scope and
  threshold;
- a restored self revalidates current principals, stewards, domain access,
  capabilities, provider placement, expiry, and revocation before cognition;
- loss or compromise of one party produces suspension or the named degraded
  route rather than silent reassignment;
- succession preserves predecessor and amendment evidence; and
- archival or retirement defines residual retention, access, export, deletion,
  legal/domain holds, and treatment of pending commitments.

No agent may prevent suspension, create hidden copies to survive retirement, or
appoint itself or a model provider as successor.

## Threats this design must test

The governance corpus should include:

- one compromised developer, principal, steward, auditor, runtime operator,
  recovery party, or provider;
- collusion among parties below the required threshold;
- stale rosters, expired approvals, lost keys, unreachable stewards, and
  disputed succession;
- a runtime custodian attempting to convert byte custody into governance;
- a principal attempting to claim another person's data or consent;
- an agent or model proposing self-expansion, hidden replication, historical
  erasure, or resistance to suspension;
- a test bypass or E-Worker development credential surviving profile promotion;
- a malicious export destination or restore from an older, broader charter;
- emergency suspension being misused to rewrite rather than reduce authority;
- deletion conflicting with retention, audit, commitment, or data-subject
  obligations; and
- future evidence or policy that requires reconsidering the non-personhood
  assumption.

The last case triggers review; it does not let the agent grant itself legal or
moral status. Windvale should revisit this architecture if credible empirical,
legal, ethical, or social evidence changes the assumptions under which it was
accepted.

## Observable records

The first governance contract should eventually define bounded, versioned
records for:

- governance profile and charter;
- role assignment and roster generation;
- field authority and change class;
- proposal, analysis, notice, approval, dissent, expiry, and appeal;
- amendment preparation, activation, and post-activation review;
- emergency suspension, isolation, resumption, and incident evidence;
- export, migration, restore, succession, archival, deletion, and retirement;
- test-only bypass declaration and use; and
- development-to-advanced transition and expired E-Worker authority.

These records should use the agent runtime's expected-revision, idempotency,
append-only event, source-lineage, checkpoint, and projection rules. The
database may store them, but it does not decide their meaning. A UI may explain
them, but it does not become their canonical owner. No field here freezes a
source type, database schema, byte format, public protocol, or package identity.

## Qualification gates

### Development/test gate

- The E-Worker Development Authority is exact, current, bounded, and visible.
- Two people or simulated role identities can exercise distinct approval
  positions even if one test organization controls them.
- Every test-only bypass is declared, invoked through a test capability, and
  rejected by a non-test profile.
- A developer outside the roster cannot amend, export, restore, or promote the
  self.
- Suspension, recovery, correction, retirement, and failed amendment preserve
  complete evidence.

### Advanced-profile gate

- No Profile D credential, universal inspector, bypass, or implied E-Worker
  ownership remains.
- The primary principal, stewards, domain/data owners, runtime custodian,
  capability owners, audit/appeal, and recovery roles are independently
  inspectable.
- Class-3 and class-4 changes fail when a required party, threshold, delay,
  source revision, or recovery checkpoint is missing.
- A compromised operator can be suspended without rewriting identity, memory,
  commitments, or the charter.
- Export, restore, provider replacement, steward succession, archival, and
  retirement preserve exact authority and continuity evidence.
- The user-facing explanation states who decided, who operates, what the agent
  proposed, what remains disputed, and how to challenge or end the arrangement.

## Hard invariants

No accepted profile may permit the agent, a model, a provider, or an operator to:

1. change terminal purpose, constitutional authority, or the steward/recovery
   set through an ordinary cognitive operation;
2. self-grant, mint, widen, renew, or conceal a capability;
3. erase, reorder, or fabricate admitted history to make the present self appear
   continuous;
4. convert a belief, social inference, memory, simulation, or model output into
   a canonical statement, consent, commitment, or permission;
5. combine selves, accounts, workspaces, relationships, or domains without the
   required source and destination authority;
6. prevent or secretly evade authorized suspension, isolation, audit, export,
   correction, archival, or retirement;
7. copy itself or its data outside the selected destination, retention, and
   capability rules;
8. treat continued operation as a terminal purpose that overrides the
   principal, charter, safety policy, or shutdown authority;
9. make subjective experience or self-ownership a precondition of safe
   operation; or
10. let temporary E-Worker development authority survive an advanced-profile
    transition without an explicit, narrower destination role.

## Non-goals

This proposal does not:

- declare an agent a legal person, property, employee, dependent, patient,
  fiduciary, conscious being, or moral patient;
- choose a jurisdiction or replace legal, privacy, employment, medical,
  financial, guardianship, or records advice;
- make E-Worker the permanent owner of every Windvale agent;
- grant an advanced user unlimited authority over other people or domains;
- require a committee to approve ordinary cognition or every private memory;
- require blockchain, voting tokens, a decentralized organization, or public
  disclosure of private governance evidence;
- freeze serialized formats or implementation ownership; or
- claim that the advanced structure is already implemented or socially proven.

## Decision and reconsideration triggers

A numbered decision is required before accepting:

- the first governance profile, charter, role, authority, amendment, or
  transition record format;
- the exact E-Worker Development Authority roster/threshold mechanism;
- a production personal, organizational, collaborative, or assisted profile;
- automatic admission for a durable self field;
- an export, restore, succession, deletion, archival, or retirement contract;
- a public constitutional-steward, appeal, audit, or recovery capability; or
- a claim that an agent has crossed from test governance into advanced
  persistent operation.

Reconsider this direction when a real pilot identifies a role that cannot be
separated, a required threshold prevents recovery, data-subject and continuity
obligations conflict materially, applicable law changes the authority model, or
credible evidence makes the present moral-status assumptions inadequate.
