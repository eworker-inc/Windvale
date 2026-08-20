# Release names and Git tags

> Status: Current naming policy for recovery archives, qualified baselines, and
> public product releases.

Windvale uses different tags for different promises. A source-custody checkpoint
is not automatically a product version, and removing an implementation dependency
does not by itself justify a major release number.

## Existing managed recovery release

The final pre-archive managed state is already preserved by the immutable tag and
GitHub release `stage0-recovery-e5a1a7473c57`. It identifies exact commit
`e5a1a7473c57935c5dfcf09b78b18c3c099e70ef`, its complete recovery bundle, and
the paired-host evidence recorded by Decision 0526.

Do not rename that state to `v0.1.0` or `v1.0.0`:

- `v0.1.0` is reserved for the first inspectable Windvale product preview;
- `v1.0.0` requires a later explicit compatibility, stability, and support
  decision; and
- adding a second version tag would not improve the existing commit, checksum,
  and release identity.

## Post-archive native baseline

After the managed-source removal commit passes one explicit complete Windows and
Linux qualification dispatch, it may receive an annotated checkpoint tag of the
form `native-only-baseline-<commit12>`. This tag means the repository boundary is
qualified; it does not promise a supported product release or stable public
contracts.

Do not create that tag before the exact committed state has passed the gate. The
tag annotation and release notes should name the workflow run, commit, tree,
changed custody boundary, and the unchanged Stage 0 recovery release.

## Product versions

Public product releases use annotated semantic-version tags. The published 0.x
line remains experimental history:

- increment `z` for a compatible fix or packaging correction within one preview
  line;
- increment `y` for a new preview milestone or an intentional experimental
  contract break; and
- reserve `v1.0.0` for an accepted stability/support policy.

The first product tag is the signed `v0.1.0` preview published on 15 August 2026
from exact commit `c1d350949207c7ee6f82ed2c399b748e188bf949` after the
package-backed application, release envelope, offline verification, and explicit
dual-host release gate completed. Parallel OS-1 launch/service composition was
valuable but was not a prerequisite for this host-product preview.

[Decision 0800](../Decisions/0800-Target-Windvale-1.0-Directly.md) supersedes
the future `v0.2.0` selection in Decision 0595. The next intended product tag is
`v1.0.0`; no `v0.2.0` product release is planned. `v1.0.0` is created only after
the complete [Windvale 1.0 product gate](Windvale-1.0-Product-Plan.md) passes and
the stability, support, deprecation, format, package, and migration promises are
accepted. If prerelease publication becomes necessary, use immutable
`v1.0.0-alpha.<n>`, `v1.0.0-beta.<n>`, or `v1.0.0-rc.<n>` annotated tags through
a named release decision; do not infer them from implementation slice numbers.

The checked-in `0.2.0-dev.1` candidates retain their exact historical
development names and hashes. They are neither Git product tags nor evidence of
an active 0.2 release line, and they must not be renamed in place.

`v0.1.0` must publish installable per-user Windows and Linux artifacts for
Windvale and its core tools. The base installation includes the launcher/client,
offline verifier, compiler, assembler, linker, runtime, and inspectors. WVDB
Query can be a separate downloadable application package; a database server and
other applications use their own packages or projects and versions rather than
becoming implicit base-install dependencies.

`0.1.0-dev.1` remains the first deterministic development artifact label under
Decision 0562. It is not a Git product tag and must not be published as
`v0.1.0`. Decision 0565 separately pins internally consistent `0.1.0` stable
Windows/Linux artifacts for the release envelope; the stable filename alone is
not a release or authenticity claim. Decisions 0563, 0564, and 0566 implement the
trust policy, envelope, threat-model boundary, protected-key custody, and exact
approval records. The official artifacts are authenticated by the signed tag and
published Release Envelope 1; exact-state Qualification run 31889107326 passed
before publication.

## Milestone checkpoint

The exact completed Milestone 2 implementation/evidence commit is
`204e8082fdaabbc7333ac40ed6ca7ff8564de123`. Its annotated checkpoint tag is
`milestone-2-package-app-204e8082fdaa`, backed by paired Bundle 1/store run
31872089188 and capability run 31872429140. This records a completed stage
without claiming that installers, signing, support, or the `v0.1.0` product are
ready. Do not use an unqualified `stage-2` tag: “Stage 2” is already used for
compiler self-reproduction and would be ambiguous.

## Tag mechanics

Tags are immutable names for exact Git commits. Create release tags as annotated
tags, push the specific tag, and publish assets/notes against that same commit.
Never move or reuse a published tag; correct a release with a new version or
checkpoint tag. Where signing is part of the release gate, sign the annotated
tag and the release manifest according to the accepted key policy.
