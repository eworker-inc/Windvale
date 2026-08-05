# Native source-to-WVB runbook

This runbook owns the ordinary project source-to-verified-WVB workflow introduced
by [Decision 0213](../Decisions/0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md).
Its exact contract and non-claims are defined by the
[native front-door specification](../../Specifications/Windvale-Native-Source-To-Wvb-Front-Door.md).
Native verification and inspection are defined by the
[read-only WVB front-door specification](../../Specifications/Windvale-Native-Wvb-Read-Only-Front-Door.md).

## Ordinary build

On Windows x64, use the inbox command processor route:

```bat
Tools\Native\Build-Wvb.cmd <project.wvproj> [output.wvb]
```

On Linux x64, use Bash:

```sh
./Tools/Native/Build-Wvb.sh <project.wvproj> [output.wvb]
```

If the output is omitted, it defaults beside the project with the same basename
and a `.wvb` extension. The output directory must already exist.

Both launchers use the checked-in inventory under
[`Artifacts/Native-Front-Door/`](../../Artifacts/Native-Front-Door/Manifest.json).
They verify the pinned host tools before execution, ask the native build driver to
write a private caller-owned candidate, and invoke the native publisher only after
successful compiler and verifier admission. The publisher repeats admission over
the exact candidate snapshot and atomically replaces the destination. A rejected
project, source set, compiler result, verifier result, or pre-replacement publication
attempt preserves an existing destination.

The project must use the current Project 1 format and may identify at most 63 source
modules. The launchers do not discover source files, install packages, infer imports,
create output directories, package PE/ELF applications, or execute the result.

## Ordinary verification and inspection

On Windows x64:

```bat
Tools\Native\Verify-Wvb.cmd <module.wvb>
Tools\Native\Inspect-Wvb.cmd <module.wvb>
```

On Linux x64:

```sh
./Tools/Native/Verify-Wvb.sh <module.wvb>
./Tools/Native/Inspect-Wvb.sh <module.wvb>
```

Both routes verify the pinned native application before use. Inspection first asks
the semantic verifier to admit the exact input, then runs the read-only structural
inspector. Neither route requires .NET or grants file-write authority. The retained
Stage 0 CLI remains available for explicit differential and recovery work.

## Candidate execution

The bounded native WVB runner is pinned for exact dual-host qualification but
is not yet an accepted ordinary front door. On Windows x64, invoke the candidate
with:

```bat
Tools\Native\Run-Wvb.cmd <module.wvb>
```

On Linux x64:

```sh
./Tools/Native/Run-Wvb.sh <module.wvb>
```

Each launcher verifies the exact pinned runner digest before starting it. The
runner admits and executes only the currently specified portable `Main() -> i32`
scalar subset, uses read-only host authority, and does not load .NET. It becomes
the ordinary execution route only after the manifest records a passing exact-commit
Windows/Linux Qualification run.

## Stage 0 recovery and differential route

The .NET CLI remains available for recovery, independent comparison, tests, and
tool surfaces not yet transferred. It is not the ordinary project source-to-WVB
entry point:

```powershell
dotnet run --project Tools/Windvale.Tool -- build project.wvproj -o output.wvb
```

To reconstruct all fifteen pinned WVB and native application artifacts into a separate
directory on Windows:

```powershell
pwsh -NoProfile -File Tools/Recovery/Rebuild-Native-Front-Door.ps1 <output-directory>
```

Or on Linux:

```sh
./Tools/Recovery/Rebuild-Native-Front-Door.sh <output-directory>
```

Reconstruction requires the pinned .NET SDK. It builds the retained Stage 0 CLI,
reconstructs the established native tools, uses the rebuilt native build driver for
the runner WVB, and reconstructs all five canonical WVB modules and all ten native
applications. It then requires every length and SHA-256 identity to match the
committed inventory. The recovery scripts refuse to target the canonical distribution
directory.

## Verification boundary

For a change limited to the source build launchers, inventory, or native-front-door
integration, run the focused Seed test named `ordinary source-to-WVB builds use
pinned native tools`. For native WVB verification or inspection, run the focused
test named `native WVB verifier and inspector applications own the read-only front
door`. For this candidate execution slice, run the focused tests named `ordinary
source-to-WVB builds use pinned native tools` and `Windvale SHA-256 and the native
WVB runner execute without a .NET host`. Run the publisher-focused test when its
application construction or adapter inputs change. Do not run progressively broader
local levels against the same source state; GitHub owns the independent Windows and
pinned-Debian Qualification gate for the final committed candidate.
