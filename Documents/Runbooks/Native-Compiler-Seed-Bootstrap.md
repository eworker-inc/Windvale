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

The seed first emits the transitional 947,975-byte Stage 1 WVB at SHA-256
`c929d5123078272e33a3c32288c770d6c20c2abc8f8800a3e0a32b8bda5c2fcb`.
The launcher packages that private compiler and uses it once to emit the
fixed-point Stage 2 candidate. Native publication must complete at 923,818
bytes with SHA-256
`49b5cbf040de4bcb22c071a5da9a4fbad47f4f0658ef910957a67b52c07607c2`.
Its exact compiler status summary is
`source wvb status=Valid functions=414 code-bytes=761807 module-bytes=923818`.
Decision 0494 reconstructs the downstream paired applications without executing
them; Decision 0492 owns the hosted-container toolset.

To reconstruct the current unqualified WVB and both target applications into an
existing directory without changing the qualified seed, use:

```bat
Tools\Native\Construct-Compiler-Reconstruction.cmd artifacts\Current-Compiler
```

```sh
./Tools/Native/Construct-Compiler-Reconstruction.sh artifacts/Current-Compiler
```

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
