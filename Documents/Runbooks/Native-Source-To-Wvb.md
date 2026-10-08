# Native source-to-WVB runbook

> Status: Current mixed pinned/bootstrap and Project 4 development workflow
> Authority: Informative; linked contracts own admission and publication rules
> Last reviewed: 2026-10-08

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

The Project 2/3 route uses the qualified checked-in inventory under
[`Artifacts/Native-Front-Door/`](../../Artifacts/Native-Front-Door/Manifest.json).
They verify both pinned host tools before execution, ask the native build driver to
write a private caller-owned candidate, and invoke the native publisher only after
successful compiler and verifier admission. The publisher repeats admission over
the exact candidate snapshot and atomically replaces the destination. A rejected
project, source set, compiler result, verifier result, or pre-replacement publication
attempt preserves an existing destination.

Project 2 and Project 3 manifests use that pinned front-door route. A Project 4
manifest is dispatched to the current development Project 4 helper, which reads
the manifest reader, source admitter, source authenticator, foreign binder,
analyzer, and emitter through the current split-compiler cache before invoking
authenticated project compilation. Compiler construction is disabled: prepare
the current checkpoint separately using the
[native-test preparation procedure](Native-Tests.md#separate-current-compiler-preparation).
A missing checkpoint gives an actionable refusal. The current-source publisher
admits the candidate again and transactionally replaces an unaliased ordinary
destination; publisher preparation may still be required. This is development
evidence, not installed promotion or complete cross-host qualification.

The project may identify at most 63 source modules. The launchers do not discover
source files, install packages, infer imports, create output directories, package
PE/ELF applications for the build result, or execute the result.

## Pinned candidate build for older project manifests

These wrappers select a historical pinned candidate for Project 2/3 manifests.
Project 4 dispatches to the prepared current-source helper described above:

```bat
Tools\Native\Build-Current-Wvb.cmd <project.wvproj> [output.wvb]
```

```sh
./Tools/Native/Build-Current-Wvb.sh <project.wvproj> [output.wvb]
```

This route binds the exact build driver under
[`Artifacts/Native-Compiler-Reconstruction-Candidate/`](../../Artifacts/Native-Compiler-Reconstruction-Candidate/Manifest.json)
and uses its self-verified raw output contract. It is non-atomic development
evidence for the older manifest route, not a promotion or cross-host qualification
claim; a failed write can leave an indeterminate destination. Do not use this
pinned artifact's name as proof that it is the current Language 1.0 compiler.
The [supported-toolchain map](../Project/Compiler-Tools-And-Libraries-Completion-Plan.md#supported-toolchain-and-retirement-map)
owns its replacement conditions.

Place a project manifest beside the component source it owns. Use a repository-root
manifest only when one artifact genuinely spans components and therefore needs their
common ancestor under Project 1's contained path rules. Do not use `..` or move every
manifest to the root for convenience; a future workspace/reference layer will own
cross-component organization without changing Project 1 containment.

## Current-source verification

Use the current Windvale verifier for forward Language 1.0 modules on Windows
and Linux x64:

```sh
node Tools/Native/Verify-Wvb.mjs --current <module.wvb>
```

The verifier is built from
[`Windvale-Compiler-Wvb-Verifier.wvproj`](../../Projects/Tools/Windvale-Compiler-Wvb-Verifier.wvproj).
Node coordinates validated cache products and bounded native execution; Windvale
owns module admission. Ordinary verification requires prepared source and native
verifier products. A missing product fails with an explicit preparation instruction
instead of starting a compiler or verifier rebuild.

First prepare the current compiler checkpoint using the
[separate preparation procedure](Native-Tests.md#separate-current-compiler-preparation).
Then prepare verifier products with a finite absolute deadline. In PowerShell:

```powershell
$VerifierDeadline = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds() + 600000
node Tools/Native/Verify-Wvb.mjs --prepare --deadline-ms $VerifierDeadline
```

In Bash:

```sh
VerifierDeadline=$(node -p 'Date.now() + 600000')
node Tools/Native/Verify-Wvb.mjs --prepare --deadline-ms "$VerifierDeadline"
```

Verifier preparation permits at most ten minutes and keeps compiler construction
disabled. It builds or reuses the current verifier's WVB and profile-2 native
application. It refuses inherited `WINDVALE_PREPARED_PRODUCTS_ONLY=1` rather than
overriding the caller's prohibition on construction.

Ordinary verification accepts an ordinary, non-linked input of 1 through
16,777,216 bytes, verifies a private snapshot, checks that the source products
and input remain unchanged, and cleans up its private directory. Its complete
operation has a two-minute deadline. Success reports
`wvb status=Valid profile=compiler-aligned`; bounded `INFO` activity lines may
precede the result during a slow cache check. Malformed modules report an invalid
phase and optional step. Admission does not establish an execution target,
grant capabilities, or promote these development products to installed delivery.

Repository owners that admit several modules can use
`Withˉcurrentˉverification({ Prepare: false, Deadline }, Use)` from
[`Current-Wvb-Verification-Batch-Core.mjs`](../../Tools/Native/Current-Wvb-Verification-Batch-Core.mjs).
It materializes one prepared current verifier and passes a temporary `Verify`
operation to `Use`. Each call admits a fresh private input snapshot and checks
the exact verifier executable before and after execution. The batch repeats
source and compiler identity checks before returning and releases its private
directory on success or failure. One batch may be active per process; overlapping
starts refuse before changing preparation mode. Calls are sequential, limited to 1,024 inputs,
and retain the two-minute per-input execution limit within the caller's finite
deadline of at most two hours. A closed batch refuses reuse; a callback that
returns with unfinished verification fails after that operation has been drained.
This API does not reuse admission results or enable compiler reconstruction.

## Pinned bootstrap verification and inspection

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

These commands retain the historical pinned contract for their named bootstrap
and qualification callers. They can reject a newer valid Language 1.0 module;
use the current-source command above for forward verification. Both routes verify
the pinned native application before use. Inspection first asks
the semantic verifier to admit the exact input, then runs the read-only structural
inspector. Neither route requires .NET or grants file-write authority. The retained
Stage 0 CLI requires the separate recovery workspace described below.

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

## Current owned-memory console development

A program whose `Main` takes a canonical memory budget needs the owned-console
launcher to supply that budget. The ordinary WVB runner's `portable-main-i32`
entry profile does not supply it. Use the existing current native lowerer and
owned launcher for the candidate scalar-Vector profile:

```powershell
$Work = 'Artifacts/Work/Owned-Vector-Example'
New-Item -ItemType Directory -Force -Path $Work | Out-Null
Tools/Native/Build-Wvb.cmd Projects/Tests/Windvale-Native-Test-Owned-Vector-Scope.wvproj "$Work/Example.wvb"
Tools/Native/Verify-Wvb.cmd "$Work/Example.wvb"
node Tools/Native/Lower-Wvb-To-Wvo.mjs --current "$Work/Example.wvb" "$Work/Example.wvo"
node Tools/Native/Package-Console.mjs --maximum-seconds 120 --owned windows "$Work/Example.wvo" "$Work/Example.exe"
& "$Work/Example.exe"
```

The example returns `42` after 1,000 reserve/release cycles; that exit code is its
expected result. On Linux, use `Build-Wvb.sh` and `Verify-Wvb.sh`, package with
`--owned linux`, and execute the resulting `.elf`. Prepare the current compiler
and native tools separately using the
[owned-console preparation procedure](Native-Tests.md#separate-current-compiler-preparation).
The 120-second packaging limit above assumes those tools are prepared; it is
not a cold preparation budget.

Owned-console preparation also assembles and checks the six shared runtime
objects. Packaging reuses them from the `native-assembly-objects-v1` cache only
when the source bytes, pinned assembler and object checker, cache implementation,
Node executable and host identity match. A source edit selects a new checkpoint;
the linker still validates every materialized object. Source input is bounded to
1 MiB and each object to 4 MiB. Missing objects may be built during ordinary
packaging; prepared-product mode refuses them before assembly. Corrupt
checkpoints fail without reconstruction or publication.

`WINDVALE_PREPARED_PRODUCTS_ONLY=1` is for execution with already prepared
products. It refuses a missing transactional publisher as well as other missing
products, including these runtime objects, without construction. An invalid
setting or corrupt checkpoint is an error. This mode can also refuse a new
application product; ordinary source-edit
builds use the prepared compiler while allowing the requested product to compile.
It is not a substitute for an explicitly bounded preparation phase.

These commands exercise ABI 24/context 10 scalar ownership and ordinary Core
startup. They do not establish general owner aggregates, shared immutable
last-share release, hosted memory startup, installed delivery or complete 1.0
qualification.

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
