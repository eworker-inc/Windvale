# Decision 0849: Define AI-led research and review evidence

- Date: 2026-08-24
- Status: Accepted and implemented in project policy and public framing
- Amends: the human-direction-and-review wording in
  [Decision 0031](0031-AI-Authorship-And-Vendor-Neutrality.md) and the
  contribution-inspection wording in
  [Decision 0032](0032-Public-Contribution-And-Governance-Foundation.md)

## Context

Windvale began with AI-authored source and documentation under human direction.
The project's development rate and scope now reflect a broader AI-led research
and engineering process: AI systems perform most research, architecture work,
implementation, documentation, test construction, diagnosis, and technical
review while a human steward directs the project and remains responsible for
acceptance and publication.

The earlier phrase “under human direction and review” can imply that a human
reads and validates every generated line before it enters the repository. That
is not the project's operating model and must not be presented as if it were.
Human responsibility, human review, AI review, deterministic verification,
reproducibility, and independent evaluation are different forms of evidence.

Windvale still intends to be understandable and fully available for review.
Reviewability is a property of explicit contracts, bounded components,
reproducible artifacts, focused changes, hostile-input tests, and traceable
decisions. It is not proof that every artifact has already received exhaustive
human review.

## Decision

Describe Windvale as an **AI-led research and development project under human
direction and accountable stewardship**.

AI systems may perform:

- research and source review;
- architecture, specification, and implementation work;
- documentation and test construction;
- diagnosis, comparison, adversarial challenge, and technical review; and
- preparation of reproducibility, conformance, and qualification evidence.

Humans or responsible organizations:

- establish the project's purpose, values, constraints, and priorities;
- direct research and select among consequential alternatives;
- review risk-sensitive, representative, disputed, or otherwise selected work;
- decide which claims, decisions, changes, and releases the project accepts;
- confirm legal authority to submit and publish material; and
- retain institutional and legal responsibility for stewardship and
  publication.

Do not state or imply that every accepted line or artifact has been reviewed by
a human unless exact review evidence supports that claim. Prefer **designed for
independent review**, **reviewable**, or an exact named review claim over
**human-reviewed**.

Keep these evidence classes distinct:

| Evidence | Meaning |
| --- | --- |
| AI-produced | One or more AI systems produced the source, prose, analysis, or test. |
| AI-reviewed | A separately scoped AI operation challenged or reviewed the result under a recorded task; this is not human or independent external review. |
| Machine-verified | A named deterministic checker, test, comparison, or qualification gate passed for an exact state. |
| Human-inspected | A named person or responsible organization inspected the stated scope; do not infer inspection outside that scope. |
| Independently reproduced | A separately operated environment or implementation reproduced an exact result under a stated contract. |
| Externally audited or certified | A named independent party or recognized scheme evaluated an exact scope; no such status follows from repository publication alone. |

An accepted change may rely on several of these evidence classes. Passing a
machine verifier does not convert the source into human-reviewed work. Human
acceptance does not convert a selective inspection into line-by-line review.
AI review does not make a result independent merely because a different model
or operation performed it.

Continue the vendor-neutral attribution policy in Decision 0031. **Frontier
model** is a dated comparative description rather than a durable authorship
class. Use it only when the exact model or a defined capability comparison is
material and recorded. Normal project language remains **AI systems** or
**AI-led research and development**.

Every contribution still requires proportionate verification and an accountable
submitter. The submitter states the intent, affected boundaries, evidence run,
known omissions, incorporated material, and exact human review performed, if
any. The DCO, CLA, stewardship, licensing, security, and release responsibilities
remain unchanged.

## Consequences

- The public description matches the actual division of work without weakening
  human responsibility for publication.
- Windvale may increase development speed through AI-led research and development
  while preserving exact evidence about what was and was not reviewed.
- Reviewability remains a project requirement even when exhaustive human review
  is infeasible.
- Security-sensitive or high-consequence boundaries can require named human or
  external review without making that requirement universal by implication.
- Later release and assurance formats may record these evidence classes without
  treating one class as a substitute for another.

## Reconsideration triggers

Revisit this decision if Windvale establishes mandatory human review for a
defined release class, delegates acceptance to multiple maintainers, adopts an
external audit or certification scheme, records model identity for every change,
or automates acceptance in a way that changes accountable publication authority.
