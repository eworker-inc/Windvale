# Contributing to Windvale

Windvale welcomes careful contributions that strengthen its specifications, implementation, tests, documentation, reproducibility, or security. The project is experimental: acceptance depends on evidence and architectural coherence, not on the size or speed of a change.

## Development model

Windvale's code and project documentation are authored by AI systems under human direction and review. Substantive contributions are expected to follow that model. The person or organization submitting a contribution defines the intent, reviews the generated result, and accepts responsibility for proposing it.

Any AI system may contribute. Do not add a model or provider label to every file or commit. Record the system, version, prompt, or generation procedure only when it is technically material to reproducibility, qualification, diagnosis, attribution, or a third-party obligation. See [Decision 0031](Documents/Decisions/0031-AI-Authorship-And-Vendor-Neutrality.md).

Small mechanical actions such as resolving merge conflicts, applying formatting, or correcting metadata do not alter the repository-wide attribution. They must not be used to conceal substantive human-authored or third-party content.

## Before proposing a change

1. Read [AGENTS.md](AGENTS.md), the relevant specification, and any related accepted decisions.
2. Search existing issues and pull requests before opening a duplicate.
3. Discuss architectural, semantic, serialized-format, security-boundary, governance, or large-scope changes before implementation.
4. Keep one coherent path. Do not add parallel formats, runtimes, compatibility layers, or speculative scaffolding without an accepted need.
5. Preserve unrelated work and never include secrets, credentials, machine-local configuration, generated build output, firmware images, or private data.

## Required evidence

Every contribution must state:

- The problem and the intended result.
- The affected contracts, profiles, formats, or security boundaries.
- The focused checks performed and their results.
- Broader checks not performed and why.
- Documentation or decisions changed with the implementation.
- Any incorporated third-party source, data, generated asset, or license obligation.
- Specific AI provenance only when required by the project-wide policy.

Documentation-only changes normally require `git diff --check`, Markdown link inspection, and direct review of the rendered text. Windvale Seed code changes normally require:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1
```

On Linux:

```sh
./Tools/Verify/Verify-Seed.sh
```

During iteration, select the narrowest relevant test by displayed-name substring:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1 -Level Fast -TestFilter '<substring>' -FailFast
```

Use `-Level Standard` for the complete in-process conformance suite without the native CLI qualification pass. Fast and Standard results are development feedback, not milestone qualification. The default `Qualification` level remains the complete Windows or Linux gate.

Changes to portable semantics, bytecode, serialization, runtime behavior, or golden hashes require evidence from Windows and real Debian before cross-host qualification is claimed. GitHub-hosted CI is a review gate, not a substitute for the exact cross-host qualification procedure.

## Pull requests

Keep pull requests focused and reviewable. Update tests and specifications in the same change when a contract changes. Do not describe proposals, incomplete work, or unverified behavior as implemented or qualified.

A maintainer may request a smaller change, additional hostile-input coverage, an independent verifier, deterministic-byte comparison, a decision record, or clearer ownership boundaries before acceptance. E-Worker Inc retains final responsibility for accepting and releasing changes under [GOVERNANCE.md](GOVERNANCE.md).

## Developer Certificate of Origin

Windvale uses the [Developer Certificate of Origin 1.1](https://developercertificate.org/) instead of a separate contributor license agreement. Every contributed commit must include a sign-off from the responsible human or organization:

```text
Signed-off-by: Responsible Name <verified-address@example.org>
```

Add it with `git commit --signoff`. By signing, the submitter certifies that the contribution may be submitted under the project's license and that the sign-off can remain in the public record. The sign-off identifies the responsible submitter; it does not claim that the submitter personally wrote AI-authored content.

Use a stable verified email or a GitHub-provided ID-based `noreply` email if privacy is required. GitHub usernames, Git author names, and Git emails are separate identifiers. Never publish an address accidentally or invent an address that appears deliverable.

Windvale completed a one-time identity normalization before public visibility. Bootstrap commits retain the descriptive `Codex` author and committer names, but their non-routable machine-local email is replaced by `246088022+EWorkerAI@users.noreply.github.com` so GitHub can associate them with the steward's project account. That account association does not replace the recorded AI-system name, imply that the account holder personally produced the content, add a retroactive DCO sign-off, or change the repository-wide provenance policy.

New E-Worker project-generated commits use `E-Worker AI <246088022+EWorkerAI@users.noreply.github.com>` as the descriptive author and `E-Worker Inc <info@eworker.ca>` as the responsible committer and DCO signer. Repository-local Git configuration keeps these roles separate and persists across host restarts. Other contributors use their own verified identities and responsible sign-offs.

## Licensing

Contributions accepted into Windvale are distributed under the root [MIT License](LICENSE). A contribution must not include material that the submitter cannot distribute under those terms. AI generation does not erase copyright, license, patent, privacy, export, or attribution obligations attached to source material.
