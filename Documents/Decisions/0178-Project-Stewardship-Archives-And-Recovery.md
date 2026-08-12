# Decision 0178: Project stewardship, archives, and recovery

- Date: 2026-08-03
- Status: Implemented; the final retirement archive is published and independently retained under Decision 0526
- Refines: [Decision 0032](0032-Public-Contribution-And-Governance-Foundation.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Retains: E-Worker Inc stewardship, the public GitHub repository, and C#/.NET as recoverable Stage 0 rather than a permanent product dependency

## Context

Windvale now has enough public source, generated artifacts, cross-host evidence, and bootstrap history that community authority and long-term recovery need deliberate policies. Those policies should remain small enough for the current maintainer group. They should not require several external community or archive services merely to preserve the project.

The final .NET Stage 0 recovery release will contain substantial evidence. Attempting to reconstruct all of it only at the native-retirement boundary would create avoidable cost and a serious risk of discovering missing inputs too late.

## Decision

### Use least-privilege GitHub roles

- Keep at least two organization owners for continuity, but keep the owner group small.
- Grant no organization-wide write authority merely for participation. Use Read for ordinary access, Triage for community moderation, Write for active repository contributors, and Maintain for release or repository managers who do not need organization administration.
- Grant Admin only for an exact administrative need and remove it when that need ends.
- Prefer a narrowly permissioned GitHub App or workflow identity over a broadly privileged human or shared service account for automation.
- Review role assignments periodically and whenever a maintainer, release manager, or automation owner changes. Exact account assignments are operational records, not architecture decisions.

### Keep public conversation with the repository

- Enable GitHub Discussions for open-ended design, usage, question-and-answer, and idea conversations.
- Keep GitHub Issues for accepted, scoped, actionable work.
- Treat Discussions as conversation rather than specification. An accepted architectural outcome moves into `Documents/Decisions/`, and current contracts move into architecture or specification documents.
- Make no promise of immediate support response merely because a public discussion channel exists.

### Use GitHub as the only external archive service

- Keep the public GitHub repository and its release facility as Windvale's external source and release archive.
- Use immutable tags, release manifests, content hashes, signatures or attestations when available, dependency and license inventories, and exact build evidence for official releases.
- Retain an E-Worker-controlled local copy independently of GitHub. The local copy is recovery redundancy, not another public source of truth.
- Do not add Zenodo, Software Heritage, or another external archive service without a later operational or legal need.
- Do not commit private keys, credentials, local SDK installations, or machine-specific state as archive material.

### Accumulate Stage 0 recovery evidence gradually

Maintain recovery as a growing evidence stream rather than one final archival project:

1. Keep the recovery runbook, exact source inventory, dependency versions, build commands, golden identities, and cross-host evidence current during ordinary bootstrap work.
2. At significant bootstrap milestones, retain an exact commit or tag, source bundle, dependency and license inventory, selected seed artifacts, checksums, and verification report.
3. Before removing .NET from normal automation, build one final signed or digest-bound recovery release from a clean checkout on Windows and Linux and verify that a separate checkout can use it to recover the accepted native path.
4. Preserve that final release in GitHub and the E-Worker local copy.
5. A later smaller from-zero seed may supplement this record after the native toolchain is mature. It does not replace the final .NET Stage 0 recovery release unless separately qualified.

### Replace the binary “from scratch” claim with evidence levels

Windvale reports the strongest level actually proved and names all remaining external dependencies:

1. reproducible from pinned external toolchains;
2. self-reproducing compiler on Windows and Linux;
3. .NET-free normal build, test, package, and execution path;
4. Windvale-native hosted compiler and toolchain;
5. Windvale OS self-hosting of the selected application and toolchain path; and
6. an independently qualified minimal recovery seed.

Firmware, CPU architecture, host operating systems, SDKs, signing tools, and hardware remain explicit even at higher levels. Windvale does not claim dependency-free construction.

## Consequences

Community growth receives a clear least-authority path without prematurely creating a large governance system. Design discussion can become public without turning issues into an unbounded forum or treating conversational text as accepted architecture.

GitHub remains an operational concentration risk, but the independently held E-Worker copy preserves recovery without adding another public service. Release integrity depends on exact manifests and evidence rather than the availability of an additional archive provider.

Stage 0 recovery work becomes incremental and reviewable. The final native-retirement gate still requires one complete clean reconstruction, but most inputs will already have been exercised rather than assembled for the first time at retirement.

This decision did not itself grant a GitHub role or enable a Discussion category. [Decision 0526](0526-Dotnet-Retirement-Qualification-And-Stage0-Archive.md) later published the digest-bound final recovery release, verified its exact selected bundle on Windows and Linux, retained the independent E-Worker copy, and retired .NET from the normal accepted workflow.

## Reconsider when

- the maintainer group outgrows the standard GitHub role model;
- a legal, citation, institutional-preservation, or disaster-recovery requirement needs another public archive;
- GitHub can no longer serve as the public source of truth;
- the local recovery copy cannot be independently restored and verified; or
- a smaller from-zero seed becomes capable of reproducing the same accepted native toolchain with stronger trust evidence.
