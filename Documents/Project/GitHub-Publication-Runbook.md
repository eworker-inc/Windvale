# GitHub publication runbook

> Status: Initial public visibility is complete. This document retains the completed private-inspection procedure and the still-open publication-baseline follow-up; it is not a request to repeat the import or visibility change.

## Purpose

This runbook records how the active Windvale project was prepared for initial public visibility and what publication-baseline work remains. It does not declare the project complete, create a stable compatibility promise, or replace the qualification rules for later changes.

The official repository is public at [`eworker-inc/Windvale`](https://github.com/eworker-inc/Windvale). It was created and inspected privately, then made public through a separate [E-Worker Inc](https://eworker.ca) approval.

## Accepted publication choices

- The one-time pre-public identity normalization is complete. Do not repeat it; preserve its evidence while continuing ordinary development from normalized `main`.
- Preserve the pre-normalization tip under the evidence tag `evidence/pre-eworkerai-linkage` and publish a [complete old-to-new mapping](Bootstrap-Attribution-Migration.md) with tree-equivalence checks.
- Keep the currently configured shared development remote named `origin` during preparation.
- Add GitHub as a separate remote named `github`; do not replace or mirror over `origin`.
- The existing GitHub repository remained private during normalization and inspection; visibility was not changed implicitly as part of a push.
- Use `main` as the initial default branch.
- Use [info@eworker.ca](mailto:info@eworker.ca) as the public business, fallback security, conduct, and project-identity contact.
- Use `E-Worker AI <246088022+EWorkerAI@users.noreply.github.com>` as the descriptive author for new E-Worker project-generated commits.
- Use `E-Worker Inc <info@eworker.ca>` as the responsible committer and DCO signer.
- Apply the Developer Certificate of Origin to new contributions prospectively. Require Contributor License Agreement 1.0 acceptance for external contributions from its adoption onward. Neither requirement is retrofitted onto bootstrap commits.

## 1. Preserve the completed identity normalization

The migration was completed while the repository was private and is recorded in [Bootstrap attribution migration](Bootstrap-Attribution-Migration.md). Its exact transformation values, commit mapping, and tree-equivalence results belong in that evidence record rather than current project narrative.

1. Keep `evidence/pre-eworkerai-linkage` and the mapping document available for qualification traceability.
2. Do not rerun, extend, or reinterpret the migration as part of ordinary contribution work.
3. Confirm that new E-Worker project-generated commits use the configured descriptive author, responsible committer, and DCO sign-off.
4. Treat any future identity migration as a new governance decision with its own safety and evidence review.

The mapping and evidence tag preserve the identity of previously qualified source snapshots. The completed metadata migration did not claim that cross-host tests were rerun or that old commits gained DCO sign-offs.

## 2. Prepare the exact source state

Wait for active implementation work to reach a clean committed point, then:

1. Fetch and fast-forward from `origin`.
2. Confirm that the working tree is clean and that `main` is synchronized.
3. Record the candidate's full commit identifier.
4. Confirm that all intended files are tracked and generated artifacts remain ignored.
5. Recheck documentation links, repository paths, licenses, copyright notices, contribution policies, secrets, large files, and the complete reachable Git history.
6. Review branches and tags explicitly. Push only the branches and tags intended for publication; do not use a mirror push.

The candidate was allowed to advance during private inspection. The still-open initial publication-baseline record must identify one exact commit rather than treating a moving branch as evidence.

## 3. Confirm GitHub identity and recovery

Before public visibility:

- Confirm the authenticated `EWorkerAI` account is an owner of the existing `eworker-inc` organization.
- Keep at least two human-controlled organization owners so account loss does not strand the project.
- Confirm two-factor authentication and recovery methods for every owner before enforcing an organization-wide requirement.
- Confirm `info@eworker.ca` remains verified and the GitHub-provided ID-based author address remains associated with `EWorkerAI`.
- Confirm each active checkout uses repository-local author and committer configuration. The account login remains administrative identity, not source authorship.

## 4. Confirm the repository and remotes

Authenticate GitHub CLI, confirm the active account and organization membership, and inspect the existing `eworker-inc/Windvale` repository. Confirm that the separate remote remains:

```powershell
git remote get-url github
git ls-remote github refs/heads/main
```

Inspect the existing `github` remote rather than replacing it blindly. Do not push credentials in a remote URL, use `--mirror`, delete the local shared remote, or force-push outside the one authorized identity-normalization step.

## 5. Private inspection completed before visibility

The pre-public inspection covered:

- Confirm `main` is the default branch and its tip matches the selected local commit.
- Inspect the root README, license, policy files, links, issue forms, pull-request template, workflow, and Dependabot configuration as GitHub renders them.
- Confirm the complete intended history is visible and historical qualification commit links resolve.
- Run the GitHub Actions workflow and inspect its permissions and logs.
- Enable private vulnerability reporting.
- Enable secret scanning and push protection when available for the organization and repository.
- Keep the default workflow token read-only unless a reviewed workflow has a narrower documented need for write access.
- Configure the DCO check and preserve a reviewable CLA acceptance record for new external contributions.
- Protect `main` after the initial checks have run: require the applicable checks, block force pushes and branch deletion, and avoid rules that make a single-maintainer project impossible to operate safely.
- Decide whether Issues and Discussions are enabled and verify that their public guidance matches `SUPPORT.md` and `SECURITY.md`.

## 6. Record the initial publication baseline

This follow-up remains open. The initial publication baseline identifies one exact commit. From clean source archives of that commit:

1. Run the Windows verifier.
2. Run the Debian verifier from the same committed source archive.
3. Compare the normalized reports and required artifact digests under the existing qualification procedure.
4. Record what passed, what was not run, and any known limitations without presenting planned layers as implemented.
5. Record whether GitHub's `main` still points at the verified baseline commit; later development may legitimately have advanced beyond it.

This is a publication snapshot, not a final project verification. Windvale remains experimental and development continues after the repository becomes public.

## 7. Public visibility completed

Changing repository visibility required separate explicit approval from E-Worker Inc. Immediately before that change, the publication owner was required to:

- Review GitHub's visibility-change warning and the repository's effective organization policies.
- Confirm there are no secrets, private issues, private discussions, unintended collaborators, actions artifacts, caches, or environment values that would be exposed.
- Confirm the security, conduct, support, license, governance, and project-identity contacts are monitored.
- Confirm the displayed repository owner, description, website, default branch, and initial baseline commit.

After approval, the repository was changed to public and verified from a signed-out view. Ongoing checks cover cloning, rendered links, Actions visibility, issue forms, private vulnerability reporting, and commit-history evidence links.

## 8. Continue normal development

Public visibility does not freeze Windvale. Each later change follows the ordinary contribution and verification policy:

- GitHub automation provides a review gate for every proposed change.
- Documentation-only changes normally require repository hygiene and link checks.
- Changes to portable semantics or qualified artifacts require the applicable Windows and Debian evidence before making a new cross-host qualification claim.
- Releases, compatibility promises, and signed artifacts remain separate future decisions.
