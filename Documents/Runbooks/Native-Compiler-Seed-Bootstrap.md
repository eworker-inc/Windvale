# Native compiler seed bootstrap

This runbook rebuilds the accepted compiler WVB from the versioned native seed
defined by the [bootstrap specification](../../Specifications/Windvale-Native-Compiler-Seed-Bootstrap.md).
It does not invoke .NET on the ordinary bootstrap path.

## Bootstrap the compiler

The ordinary verification wrappers select the repository's checked-in artifact
and source roots automatically. They build Stage 1, package and execute that
newly built compiler, and require exact Stage 2 equality:

```bat
Tools\Verify\Verify-Bootstrap.cmd
```

```sh
./Tools/Verify/Verify-Bootstrap.sh
```

Use the lower-level one-stage launchers below when proving a copied release seed
or selecting an explicit destination. They publish Stage 1 but do not run the
self-convergence coordinator.

On Windows x64:

```bat
Tools\Native\Bootstrap-Compiler.cmd Artifacts . artifacts\Bootstrap\Windvale-Compiler.wvb
```

On Linux x64:

```sh
./Tools/Native/Bootstrap-Compiler.sh Artifacts . artifacts/Bootstrap/Windvale-Compiler.wvb
```

Create the output directory first. The first argument is an artifact root that
contains both `Native-Compiler-Seed` and `Native-Front-Door`. For clean-bootstrap
evidence, copy those two directories from the promoted seed release into a fresh
location and pass that copied location rather than the checkout's `Artifacts`
directory.

The repinned candidate command must complete native publication of 921,640 bytes
with SHA-256
`18a657f8d4192f01a5822274a7348c02fc30b9bb3a4a9283e4ba302590c3f754`.
Its exact compiler status summary and downstream native compiler-closure package
identities remain pending their focused reconstruction and final gate; do not
reuse the preceding 921,900-byte candidate's 419-function summary as current
evidence. Decision 0492 separately reconstructs the hosted-container toolset.

## Reconstruct the seed through Stage 0

Seed reconstruction is recovery work, not the ordinary bootstrap. It requires the
SDK pinned by the exact reconstruction commit and writes outside the canonical
distribution directory.

On Windows:

```powershell
pwsh -NoProfile -File Tools/Recovery/Rebuild-Native-Compiler-Seed.ps1 artifacts/Recovered-Native-Compiler-Seed
```

On Linux:

```sh
./Tools/Recovery/Rebuild-Native-Compiler-Seed.sh artifacts/Recovered-Native-Compiler-Seed
```

Both commands archive the exact reconstruction and semantic-freeze commits,
rebuild the seed WVB and paired applications, and require every byte identity in
`Artifacts/Native-Compiler-Seed/SHA256SUMS`.

## Current boundary

This candidate closes the missing distribution and host-launcher part of the clean
native-seed bootstrap. It remains short of the complete retirement gate in three
ways: paired-host promotion is pending, the seed does not rebuild its own PE/ELF
without the still-candidate native packaging chain, and one later accepted release
must consume this promoted seed as a previous release.
