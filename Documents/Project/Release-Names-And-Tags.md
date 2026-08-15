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

Public product releases use annotated `v0.y.z` tags while contracts remain
experimental:

- increment `z` for a compatible fix or packaging correction within one preview
  line;
- increment `y` for a new preview milestone or an intentional experimental
  contract break; and
- reserve `v1.0.0` for an accepted stability/support policy.

The planned first product tag is `v0.1.0`, after the package-backed application,
release envelope, offline verification, and explicit dual-host release gate are
complete. In the dependency-based roadmap these are Milestone 2 followed by
Milestone 3. Parallel OS-1 launch/service composition is valuable but is not a
prerequisite for this host-product preview. Later previews would normally be
`v0.2.0`, `v0.3.0`, and so on—not “release 2” without a compatibility meaning.

`v0.1.0` must publish installable per-user Windows and Linux artifacts for
Windvale and its core tools. The base installation includes the launcher/client,
offline verifier, compiler, assembler, linker, runtime, and inspectors. WVDB
Query can be a separate downloadable application package; a database server and
other applications use their own packages or projects and versions rather than
becoming implicit base-install dependencies.

## Milestone checkpoint

After the exact Milestone 2 commit passes its paired Windows/Linux owners, it may
receive an annotated checkpoint tag of the form
`milestone-2-package-app-<commit12>`. This records a completed stage without
claiming that installers, signing, support, or the `v0.1.0` product are ready.
Do not use an unqualified `stage-2` tag: “Stage 2” is already used for compiler
self-reproduction and would be ambiguous.

## Tag mechanics

Tags are immutable names for exact Git commits. Create release tags as annotated
tags, push the specific tag, and publish assets/notes against that same commit.
Never move or reuse a published tag; correct a release with a new version or
checkpoint tag. Where signing is part of the release gate, sign the annotated
tag and the release manifest according to the accepted key policy.
