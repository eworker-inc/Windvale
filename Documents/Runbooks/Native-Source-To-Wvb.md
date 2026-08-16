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

Both launchers use the qualified checked-in inventory under
[`Artifacts/Native-Front-Door/`](../../Artifacts/Native-Front-Door/Manifest.json).
They verify both pinned host tools before execution, ask the native build driver to
write a private caller-owned candidate, and invoke the native publisher only after
successful compiler and verifier admission. The publisher repeats admission over
the exact candidate snapshot and atomically replaces the destination. A rejected
project, source set, compiler result, verifier result, or pre-replacement publication
attempt preserves an existing destination.

The project must use the current Project 2 format and may identify at most 63 source
modules. The launchers do not discover source files, install packages, infer imports,
create output directories, package PE/ELF applications, or execute the result.

## Forward-language candidate build

Source that uses post-freeze language features must opt into the unqualified
current compiler candidate:

```bat
Tools\Native\Build-Current-Wvb.cmd <project.wvproj> [output.wvb]
```

```sh
./Tools/Native/Build-Current-Wvb.sh <project.wvproj> [output.wvb]
```

This route binds the exact build driver under
[`Artifacts/Native-Compiler-Reconstruction-Candidate/`](../../Artifacts/Native-Compiler-Reconstruction-Candidate/Manifest.json)
and uses its self-verified raw output contract. It is non-atomic development
evidence, not a promotion or cross-host qualification claim; a failed write can
leave an indeterminate destination. The WVDB Query package and current library
verification use this route because the read-only directory facade consumes typed
singleton capability references.

Place a project manifest beside the component source it owns. Use a repository-root
manifest only when one artifact genuinely spans components and therefore needs their
common ancestor under Project 1's contained path rules. Do not use `..` or move every
manifest to the root for convenience; a future workspace/reference layer will own
cross-component organization without changing Project 1 containment.

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

## Ordinary accepted-subset execution

The bounded native WVB runner is the ordinary execution route for its documented
accepted subset. On Windows x64:

```bat
Tools\Native\Run-Wvb.cmd <module.wvb>
```

On Linux x64:

```sh
./Tools/Native/Run-Wvb.sh <module.wvb>
```

Each launcher verifies the exact pinned runner digest before starting it. The
runner admits and executes only its specified profiles and budgets, uses explicit
host authority, and does not load .NET. Unsupported capability or execution
surface fails explicitly.

## Stage 0 recovery

Managed Stage 0 source and commands are absent from `main`. The immutable
`stage0-recovery-e5a1a7473c57` release reconstructs the exact feature-freeze
state on Windows and Linux. Follow
[`Bootstrap/Stage0/README.md`](../../Bootstrap/Stage0/README.md) and restore it in
a separate workspace for a named recovery, security, or historical differential
investigation. New source semantics remain solely in `Compiler/Windvale`.

## Verification boundary

Use `Tools/Verify/Verify-Changed.ps1` once after a coherent change. It selects the
focused native build, read-only tool, runner, publisher, or planner owner required
by the changed paths. Do not run progressively broader local levels against the
same source state; complete Windows/Linux qualification is an explicit selected
state rather than a per-commit gate.
