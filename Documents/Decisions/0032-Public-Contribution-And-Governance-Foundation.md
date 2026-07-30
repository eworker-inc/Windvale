# Decision 0032: Public contribution and governance foundation

- Date: 2026-07-30
- Status: Accepted; repository files implemented, hosting settings pending

## Context

An MIT license permits use and redistribution but does not explain how Windvale accepts changes, handles security reports, governs technical direction, supports users, or distinguishes source-code rights from project identity. Public hosting also introduces untrusted contributions and repository automation as new security boundaries.

Windvale's AI-authored development model creates an additional distinction between the system producing content, the person or organization submitting it, the Git identity recorded in history, and the steward deciding whether to publish it. These roles must remain understandable without treating an AI system as a legal person or making one vendor the project owner.

## Decision

Publish repository-level contribution, security, governance, support, conduct, and trademark policies. E-Worker Inc remains the project steward and final release authority. Technical direction continues through specifications, qualification evidence, and accepted decision records rather than informal compatibility promises.

Substantive contributed implementation and project documentation follow Windvale's AI-authored model: an AI system produces the content under human direction and review. The human or organization submitting the contribution is responsible for inspecting it, confirming the right to distribute it, identifying incorporated third-party material, and satisfying the verification gate. A model or provider is recorded only when technically material under Decision 0031.

Accept contributions under the repository's MIT terms without a separate contributor license agreement. Require a Developer Certificate of Origin 1.1 sign-off from the responsible submitter. The sign-off records responsibility and permission to submit; it is distinct from descriptive AI authorship.

Use GitHub private vulnerability reporting for confidential security reports and keep ordinary support in public issues or discussions. Protect the default branch with required Windows and Linux verification checks once public hosting exists. Repository automation receives read-only contents permission unless a narrower workflow explicitly requires more.

Keep GitHub ownership and Git commit identity separate. The public repository should be owned by an E-Worker Inc organization containing Windvale and other E-Worker projects, not by an account named after one project or AI model. Existing Codex-authored history remains factual and is not rewritten merely to attach it to a GitHub profile. Future E-Worker project-generated commits may use the reusable descriptive author name `E-Worker AI`; the associated email must be a dedicated verified address or GitHub-provided ID-based `noreply` address, never an invented deliverable address or a private personal address published accidentally.

The MIT License does not grant rights to imply official status, sponsorship, or endorsement through the Windvale or E-Worker names and visual identity. The trademark policy permits truthful referential use and clearly distinguished forks while reserving official presentation to the steward.

## Consequences

Contributors receive one documented path from proposal through review and qualification. Repository ownership can outlive one maintainer account, while individual access can use least-privilege organization roles. Security and conduct reports have separate private routes from ordinary support.

The DCO adds a sign-off requirement to commits contributed for inclusion. It does not guarantee that AI-produced material is copyrightable, eliminate third-party review, or convert an AI system into a legal author or rightsholder.

GitHub organization creation, the final account name, a verified commit email, branch rules, private vulnerability reporting, and repository security settings remain publication-time operations. This decision authorizes their configuration but does not claim they exist before they are actually enabled.

## Reconsider when

- The project adds multiple maintainers who need delegated release or security authority.
- The DCO materially prevents appropriate contributions or no longer supplies adequate rights assurance.
- A foundation or other legal entity assumes stewardship.
- A stable compatibility release requires a formal support lifecycle.
- Repository automation needs write authority or begins publishing signed release artifacts.
