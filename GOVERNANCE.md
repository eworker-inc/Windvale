# Windvale governance

## Stewardship

[E-Worker Inc](https://eworker.ca) initiated Windvale and is its project steward. The steward controls official repositories and releases, appoints maintainers, accepts or rejects contributions, administers security reports, and decides when a contract or milestone is qualified.

Windvale-owned work is source-available under the [Windvale Community Source License 1.0](LICENSE.md). The public license permits defined free uses while reserving large-organization production use and Windvale-as-a-product use for separate commercial terms. Independent applications remain the property of their creators, and third-party components remain under their own licenses. Source availability does not require the official project to accept every compatible change or transfer stewardship of the Windvale identity.

## Roles

- **Steward:** E-Worker Inc holds final governance and release authority.
- **Maintainers:** people or organizations delegated authority over named areas, repositories, reviews, or security work.
- **Contributors:** people or organizations submitting changes under the contribution policy.
- **AI systems:** implementation and documentation authors in the descriptive project sense. AI systems do not hold governance roles, repository credentials, legal responsibility, or licensing authority.

One participant may direct several AI systems, but acceptance still depends on the same specifications, review, tests, and evidence.

## Decisions

Routine implementation choices are resolved through review against existing contracts. Durable architecture, semantics, formats, safety boundaries, bootstrap direction, governance, and compatibility policy require a dated record under `Documents/Decisions/`.

Maintainers seek technically grounded consensus. When consensus is absent or release responsibility is implicated, the steward makes and records the decision. Earlier decisions are marked amended or superseded rather than silently erased.

## Contribution and release authority

Pull requests are proposals until accepted. Passing tests does not create a right to merge, and merging does not by itself qualify a cross-host milestone. The steward may require narrower scope, additional evidence, licensing clarification, or security review.

Official releases require an identified source commit, release notes, applicable qualification evidence, and authorization by the steward. No stable support or compatibility promise exists until a release explicitly defines one.

## Identity and attribution

Repository ownership, GitHub login, Git author metadata, CLA acceptance, DCO sign-off, AI attribution, and legal stewardship serve different purposes and need not use the same name.

The [completed one-time pre-public identity normalization](Documents/Project/Bootstrap-Attribution-Migration.md) associates bootstrap commits with the project account while retaining their existing descriptive author names, source trees, messages, timestamps, and evidence mapping. New E-Worker project-generated commits use `E-Worker AI <246088022+EWorkerAI@users.noreply.github.com>` as the vendor-neutral descriptive author shared across E-Worker projects. `E-Worker Inc <info@eworker.ca>` is the responsible committer and DCO signer. The repository context identifies Windvale.

The official public GitHub repository is [`eworker-inc/Windvale`](https://github.com/eworker-inc/Windvale) under the existing [E-Worker Inc organization](https://github.com/eworker-inc), which can contain Windvale and other E-Worker projects. The administrative service account is [`EWorkerAI`](https://github.com/EWorkerAI); its login identity remains separate from source authorship. The repository completed private inspection before public visibility. Individual or service accounts administer it through least-privilege roles; the organization is not named after one project or AI model.

## Changes to governance

Governance changes require review appropriate to the repository's current visibility and an accepted decision record. After public visibility, ordinary lasting governance changes require public review. E-Worker Inc may make an immediate protective change for legal, security, or abuse reasons, but must document the lasting policy once disclosure is safe.
