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

Documentation-only changes normally require `git diff --check`, Markdown link inspection, and direct review of the rendered text. Windvale Seed code changes normally require one proportional local verifier. For focused work, use:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Changed.ps1
```

For a coherent cross-area batch, the no-argument Seed verifier runs the `Development` suite:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1
```

On Linux, use `VERIFY_LEVEL=development ./Tools/Verify/Verify-Seed.sh`.

Verification levels are alternatives, not a required sequence. Do not run changed-file, Fast, Development, Standard, and Qualification one after another for the same source state. A passing broader tier subsumes narrower tiers, and remains valid until relevant inputs change; committing or pushing is not a reason to repeat it. After a failed check, rerun the narrowest affected selection and use at most one broader final gate if the risk calls for it.

When selecting explicitly, use one or more test areas and optionally narrow them by displayed-name substring:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1 -Level Fast -TestArea compiler,runtime -TestFilter '<substring>' -FailFast
```

The available Seed areas are `assembler`, `bytecode`, `compiler`, `database`, `foundation`, `golden`, `linker`, `object-model`, and `runtime`. Area selections form a union; an accompanying filter intersects with that union. Fast and Development run regular tests and omit explicitly extended compiler-closure, AOT-transport, full-stage, and golden contracts. Add `-IncludeExtended` to an explicit Fast selection when one of those contracts is the narrowest relevant check. Fast selection remains Seed-area-only; run `Tests/Windvale.Os.Tests` directly for focused OS work. Development adds the bounded OS in-process suite. Standard runs every regular and extended Seed in-process contract plus the OS suite without the native CLI qualification pass. Qualification must be requested explicitly and adds the complete native CLI gate. Changed-file, Fast, Development, and Standard results are development feedback, not milestone qualification.

Testing is proportional to risk. Ordinary edits should not pay for unrelated multi-minute gates: choose focused or change-aware checks, or Development for a coherent batch, and reserve Standard/Qualification for final candidates or changed portable contracts. GitHub provides the independent dual-host Qualification gate for implementation and specification changes; do not duplicate it locally without a qualification reason. Run the separate compiler-bootstrap or OS-boot gates only when the compiler inventory/bootstrap boundary or boot/image/kernel boundary changed, or when making the corresponding qualification claim. Always state which broader checks were not run and why.

Changes to portable semantics, bytecode, serialization, runtime behavior, or golden hashes require evidence from Windows and real Debian before cross-host qualification is claimed. GitHub-hosted CI is a review gate, not a substitute for the exact cross-host qualification procedure.

## Pull requests

Keep pull requests focused and reviewable. Update tests and specifications in the same change when a contract changes. Do not describe proposals, incomplete work, or unverified behavior as implemented or qualified.

A maintainer may request a smaller change, additional hostile-input coverage, an independent verifier, deterministic-byte comparison, a decision record, or clearer ownership boundaries before acceptance. [E-Worker Inc](https://eworker.ca) retains final responsibility for accepting and releasing changes under [GOVERNANCE.md](GOVERNANCE.md).

## Contributor License Agreement and Developer Certificate of Origin

Before E-Worker Inc can accept an external contribution, the responsible individual or authorized entity representative must accept the [Windvale Contributor License Agreement 1.0](CONTRIBUTOR-LICENSE-AGREEMENT.md). Complete the CLA acceptance section in the pull-request description with the legal name or entity, GitHub account, agreement version, and acceptance date. The public acceptance record applies to later contributions under the same identity and agreement version unless E-Worker requests renewed acceptance. Contributions submitted directly by E-Worker Inc do not require a separate external acceptance record.

The CLA does not transfer a contributor's copyright. It gives E-Worker the rights needed to distribute Windvale under the community license and offer separate commercial terms. CLA acceptance does not replace source and provenance review.

Every contributed commit must also include a sign-off under the [Developer Certificate of Origin 1.1](https://developercertificate.org/) from the responsible human or organization:

```text
Signed-off-by: Responsible Name <verified-address@example.org>
```

Add it with `git commit --signoff`. By signing, the submitter certifies that the contribution may be submitted under the project's contribution terms and that the sign-off can remain in the public record. The sign-off identifies the responsible submitter; it does not claim that the submitter personally wrote AI-authored content and does not by itself constitute CLA acceptance.

Use a stable verified email or a GitHub-provided ID-based `noreply` email if privacy is required. GitHub usernames, Git author names, and Git emails are separate identifiers. Never publish an address accidentally or invent an address that appears deliverable.

Historical identity normalization is documented separately in the [bootstrap attribution migration record](Documents/Project/Bootstrap-Attribution-Migration.md). It does not change the identity, responsibility, or sign-off requirements for new contributions.

New E-Worker project-generated commits use `E-Worker AI <246088022+EWorkerAI@users.noreply.github.com>` as the descriptive author and `E-Worker Inc <info@eworker.ca>` as the responsible committer and DCO signer. Repository-local Git configuration keeps these roles separate and persists across host restarts. Other contributors use their own verified identities and responsible sign-offs.

## Licensing

Windvale-owned work, including accepted contributions, is distributed under the root [Windvale Community Source License 1.0](LICENSE). Under the CLA, E-Worker may also offer the contribution under separate commercial or other terms. Contributors retain any copyright they hold in their contributions. A contribution must not include material that the submitter cannot provide under the CLA and project license. AI generation does not erase copyright, license, patent, privacy, export, or attribution obligations attached to source material. Third-party material remains under its own terms and must be identified as described in the [third-party notice policy](THIRD-PARTY-NOTICES.md).
