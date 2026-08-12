# Final Stage 0 recovery archive

## Purpose

The final Stage 0 recovery release preserves the frozen C#/.NET implementation,
its complete reachable Git history, repository-owned seed artifacts, exact
dependencies, licenses, recovery commands, and qualification evidence after
.NET leaves normal Windvale automation. It is historical recovery evidence,
not a normal compiler, build, test, package, or execution dependency.

The archive proves recovery level 3 from Decision 0178: the normal workflow is
.NET-free, while an explicit external Stage 0 toolchain can reconstruct and
hand off to the accepted native path on Windows and Linux.

## Required external tools

- Git with bundle, fetch, archive, and SHA-1 object support;
- .NET SDK 10.0.302, as pinned by `global.json`;
- PowerShell 7 on Windows or a POSIX shell on Linux;
- ordinary host SHA-256 and filesystem facilities; and
- enough temporary storage for a separate checkout and reconstructed native
  products.

`Documents/Project/Stage0-Recovery-Dependencies.json` is the machine-readable
dependency and license inventory. The release deliberately excludes SDK
installations, NuGet caches, credentials, private keys, machine configuration,
and private QA information.

## Construct the archive

Start from the exact committed source state with a clean working tree. Write to
a new empty directory outside the repository:

```powershell
pwsh -NoProfile -File Tools/Recovery/New-Stage0-Recovery-Archive.ps1 `
  -OutputDirectory C:\Recovery\Windvale-Stage0
```

The command creates:

- one complete Git bundle whose `HEAD` is the archived commit and whose history
  contains the historical commits consumed by the seed reconstructor;
- exact Git source, SHA-256 artifact, recovery-entry, dependency, and license
  inventories;
- a copy of this runbook;
- one versioned release manifest; and
- `SHA256SUMS` covering every other release asset.

The command refuses a dirty source tree, an existing nonempty destination, an
unverifiable bundle, or an inventory inconsistency. Run it twice into separate
directories, once from a clean Windows checkout and once from a clean Linux
checkout, for final qualification. The two base asset sets must agree in every
byte, because the manifest derives its timestamp from the archived commit rather
than the wall clock.

## Verify structure only

From an extracted release set, or from the archived checkout itself, run:

```powershell
pwsh -NoProfile -File Tools/Recovery/Test-Stage0-Recovery-Archive.ps1 `
  -ReleaseDirectory C:\Recovery\Windvale-Stage0
```

This verifies every release digest, bundle head and tree, exact source
inventory, every repository artifact byte listed by the archive, all recovery
entry classifications, dependency/license inventories, and a fresh detached
checkout constructed only from the bundle.

## Verify recovery and native handoff

Run the complete recovery check on a clean Windows x64 host:

```powershell
pwsh -NoProfile -File Tools/Recovery/Test-Stage0-Recovery-Archive.ps1 `
  -ReleaseDirectory C:\Recovery\Windvale-Stage0 `
  -RunRecovery
```

On Linux x64, use the same PowerShell command when PowerShell 7 is available.
The verifier selects the paired `.ps1` or `.sh` recovery owners for the current
host. It creates a separate checkout from the release bundle and then:

1. proves frozen managed Stage 1/Stage 2 compiler convergence;
2. reconstructs the exact paired native compiler seed from the historical
   source commits contained in the bundle;
3. reconstructs the exact current native source-build, publisher, verifier,
   inspector, and runner front door; and
4. runs native compiler self-convergence through `Verify-Bootstrap.cmd` or
   `.sh` without using the recovered managed tool as the normal verifier.

All reconstruction occurs below a verifier-owned temporary directory and is
removed after success or failure. The release directory and bundle are
read-only inputs.

## Qualification evidence and publication

The final GitHub release must attach every generated asset without renaming it,
record the release commit and tree, include the Windows and Linux recovery
reports, link the exact six-job native `Verification gate`, and publish a final
supplemental checksum file that covers the generated `SHA256SUMS` file and the
two reports as well as every manifest asset. The generated checksum remains the
self-contained base-archive inventory; the supplemental checksum is release
evidence and may coexist beside it. The immutable tag must resolve to the same
commit recorded by the archive manifest.

Keep one independently held E-Worker copy of the exact release assets. Do not
record its filesystem location, credentials, host address, or transfer details
in the public repository.

The managed recovery commands stay feature-frozen. Run them only for archive
reconstruction, a named differential diagnosis, a security correction, or an
explicit recovery drill. New language semantics belong in `Compiler/Windvale`.
