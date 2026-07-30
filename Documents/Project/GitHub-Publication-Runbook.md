# GitHub publication runbook

## Purpose

This runbook prepares the active Windvale project for initial public visibility. It does not declare the project complete, create a stable compatibility promise, or replace the qualification rules for later changes.

The planned official repository is `eworker-inc/Windvale`. It is created privately, inspected at an identified commit, and made public only through a separate E-Worker Inc approval.

## Accepted publication choices

- Preserve the complete existing `main` history, including factual Codex author metadata and every commit identifier referenced by qualification evidence.
- Keep the currently configured shared development remote named `origin` during preparation.
- Add GitHub as a separate remote named `github`; do not replace or mirror over `origin`.
- Create the GitHub repository as private first and do not change its visibility implicitly as part of a push.
- Use `main` as the initial default branch.
- Use [info@eworker.ca](mailto:info@eworker.ca) as the public business, fallback security, conduct, and project-identity contact.
- Treat `E-Worker AI` as a possible vendor-neutral author name for future E-Worker project-generated commits. Select and verify its Git email separately before use.
- Apply the Developer Certificate of Origin to new contributions prospectively. Do not rewrite bootstrap history to retrofit sign-offs.

## 1. Prepare the exact source state

Wait for active implementation work to reach a clean committed point, then:

1. Fetch and fast-forward from `origin`.
2. Confirm that the working tree is clean and that `main` is synchronized.
3. Record the candidate's full commit identifier.
4. Confirm that all intended files are tracked and generated artifacts remain ignored.
5. Recheck documentation links, repository paths, licenses, copyright notices, contribution policies, secrets, large files, and the complete reachable Git history.
6. Review branches and tags explicitly. Push only the branches and tags intended for publication; do not use a mirror push.

The candidate may advance while the repository remains private. Freeze one exact commit only when performing the initial publication baseline.

## 2. Confirm GitHub identity and recovery

Before creating the repository:

- Confirm the authenticated account is an owner of the existing `eworker-inc` organization.
- Keep at least two human-controlled organization owners so account loss does not strand the project.
- Confirm two-factor authentication and recovery methods for every owner before enforcing an organization-wide requirement.
- Decide, with explicit approval and a final availability check, whether to rename the administrative service account to `EWorkerAI` before public visibility. The account name is administrative identity, not source authorship.
- Verify any email selected for future Git author, committer, or DCO metadata. A public contact address is not automatically a commit address.

## 3. Create and populate the private repository

Authenticate GitHub CLI, confirm the active account and organization membership, and create `eworker-inc/Windvale` with private visibility. Then add and push the separate remote:

```powershell
git remote add github https://github.com/eworker-inc/Windvale.git
git push github main
```

If the `github` remote already exists, inspect it rather than replacing it blindly. Do not push credentials in a remote URL, use `--mirror`, force-push, or delete the local shared remote.

## 4. Inspect privately

At the private GitHub repository:

- Confirm `main` is the default branch and its tip matches the selected local commit.
- Inspect the root README, license, policy files, links, issue forms, pull-request template, workflow, and Dependabot configuration as GitHub renders them.
- Confirm the complete intended history is visible and historical qualification commit links resolve.
- Run the GitHub Actions workflow and inspect its permissions and logs.
- Enable private vulnerability reporting.
- Enable secret scanning and push protection when available for the organization and repository.
- Keep the default workflow token read-only unless a reviewed workflow has a narrower documented need for write access.
- Configure the DCO check for new contributions.
- Protect `main` after the initial checks have run: require the applicable checks, block force pushes and branch deletion, and avoid rules that make a single-maintainer project impossible to operate safely.
- Decide whether Issues and Discussions are enabled and verify that their public guidance matches `SUPPORT.md` and `SECURITY.md`.

## 5. Record the initial publication baseline

The initial publication baseline identifies one exact commit. From clean source archives of that commit:

1. Run the Windows verifier.
2. Run the Debian verifier from the same committed source archive.
3. Compare the normalized reports and required artifact digests under the existing qualification procedure.
4. Record what passed, what was not run, and any known limitations without presenting planned layers as implemented.
5. Confirm that GitHub's `main` still points at the verified commit before changing visibility.

This is a publication snapshot, not a final project verification. Windvale remains experimental and development continues after the repository becomes public.

## 6. Approve public visibility

Changing repository visibility requires a separate explicit approval from E-Worker Inc. Immediately before that change:

- Review GitHub's visibility-change warning and the repository's effective organization policies.
- Confirm there are no secrets, private issues, private discussions, unintended collaborators, actions artifacts, caches, or environment values that would be exposed.
- Confirm the security, conduct, support, license, governance, and project-identity contacts are monitored.
- Confirm the displayed repository owner, description, website, default branch, and initial baseline commit.

After approval, change visibility to public and verify the repository from a signed-out view. Check cloning, rendered links, Actions visibility, issue forms, private vulnerability reporting, and the commit-history evidence links.

## 7. Continue normal development

Public visibility does not freeze Windvale. Each later change follows the ordinary contribution and verification policy:

- GitHub automation provides a review gate for every proposed change.
- Documentation-only changes normally require repository hygiene and link checks.
- Changes to portable semantics or qualified artifacts require the applicable Windows and Debian evidence before making a new cross-host qualification claim.
- Releases, compatibility promises, and signed artifacts remain separate future decisions.
