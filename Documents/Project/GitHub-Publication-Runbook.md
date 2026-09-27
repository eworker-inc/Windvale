# GitHub publication runbook

> Status: Public visibility complete; initial publication-baseline follow-up open
> Authority: Operational guidance; contribution and qualification policies govern
> Last reviewed: 2026-09-26

## Purpose

The official repository is public at [eworker-inc/Windvale](https://github.com/eworker-inc/Windvale).
The completed private inspection and visibility procedure is retained in
[Git history](../Git-History.md#retired-migration-notes-and-diagnostic-probes).
Do not repeat the initial import, identity normalization or visibility change.

## Current repository workflow

Keep the shared development remote named origin and the separate GitHub remote
named github. Use main and inspect the configured URLs before pushing. Publish
only intended branches and tags; do not mirror or overwrite shared history.
The [agent handbook](../../AGENTS.md#git-workflow) owns the normal Git workflow.

[CONTRIBUTING](../../CONTRIBUTING.md) owns author and committer identities,
DCO sign-off and external contributor CLA requirements. Preserve the
pre-normalization evidence tag and the [bootstrap attribution mapping](Bootstrap-Attribution-Migration.md).
Any future identity migration needs its own governance decision.

## 6. Record the initial publication baseline

This follow-up remains open. The initial publication baseline identifies one exact commit. From clean source archives of that commit:

1. Run the Windows verifier.
2. Run the Debian verifier from the same committed source archive.
3. Compare the normalized reports and required artifact digests under the existing qualification procedure.
4. Record what passed, what was not run, and any known limitations without presenting planned layers as implemented.
5. Record whether GitHub's `main` still points at the verified baseline commit; later development may legitimately have advanced beyond it.

This is a publication snapshot, not a final project verification. Windvale remains under active research and development after the repository becomes public.

## Continue normal development

Public visibility does not freeze Windvale. GitHub automation supplies review
gates; each change follows the focused verification policy in the
[agent handbook](../../AGENTS.md#testing-and-verification). New cross-host
qualification claims require the applicable Windows and Debian evidence.
Releases, compatibility promises and signed publication retain their separate
authorization and qualification requirements.
