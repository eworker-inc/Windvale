# Decision 0031: AI authorship and vendor neutrality

- Date: 2026-07-30
- Status: Accepted and implemented

## Context

Windvale's code and documentation are produced by AI systems under human direction and review. OpenAI Codex produced most of the initial implementation, and future work may involve Codex or other AI systems. The project needs to describe that history truthfully without turning a model or provider into the project's identity, implying provider involvement, or confusing creative attribution with legal copyright status.

Per-change model labels would add noise without establishing quality, responsibility, or reproducibility. Windvale instead needs a durable repository-wide default and a narrower rule for recording a particular system when its identity is technically material.

## Decision

Describe Windvale as implemented and documented entirely by AI systems under human direction and review. E-Worker Inc initiated and stewards the project.

Use “author” and “authored” as descriptive project attribution. These terms do not assert that an AI system is a legal person, a copyright holder, or able to grant a license. The project does not make a universal determination about the copyrightability of AI-produced material; that can depend on the material, human contribution, and applicable jurisdiction.

Windvale is model- and vendor-neutral. Any AI system may contribute. A reference to OpenAI Codex or another system records development history or technically relevant provenance and does not imply sponsorship, affiliation, endorsement, ownership, or special status for its provider.

AI authorship is the repository-wide default and does not require a model name on every commit or file. Record a specific model, provider, tool version, prompt, or generation procedure only when it is technically material to reproducing or qualifying an artifact, diagnosing behavior, or satisfying a third-party attribution or license obligation.

A Git hosting account association is administrative metadata, not descriptive authorship. A verified project-account email may associate a commit with the steward's GitHub account while the Git author name continues to identify `Codex`, `E-Worker AI`, or another applicable descriptive source. Account linkage must not erase a materially recorded system name or be presented as proof that the account holder personally produced the content.

The person or organization submitting or accepting a contribution remains responsible for review, publication, and confirming that it may be distributed. The MIT License grants permissions from each applicable rightsholder for rights that subsist; it does not depend on treating an AI system as a rightsholder. The root notice is:

```text
Copyright (c) 2026 E-Worker Inc and Windvale contributors
```

This broadens the notice used by [Decision 0028](0028-MIT-License-And-E-Worker-Stewardship.md) without changing the MIT license or E-Worker Inc's stewardship.

## Consequences

Public documents can normally say “AI systems” without maintaining a provider roster. Codex receives factual historical credit, and future systems can participate on equal terms. Vendor competition, provider branding, and model churn do not become project-governance concerns.

Quality and acceptance continue to depend on specifications, review, tests, deterministic artifacts, and qualification evidence rather than the identity of the generating system. Third-party code, data, and other incorporated material still require their own provenance and license review; describing a contribution as AI-authored does not erase those obligations.
