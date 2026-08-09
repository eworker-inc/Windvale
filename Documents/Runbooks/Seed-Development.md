# Windvale Seed development runbook

This runbook is the practical entry point for building, testing, and exploring the current Windvale Seed implementation. [AGENTS.md](../../AGENTS.md) and [CONTRIBUTING.md](../../CONTRIBUTING.md) remain authoritative for contribution, provenance, verification, and Git workflow requirements.

## Prerequisites

- Windows x64 with the inbox command processor, or Linux x64 with Bash and
  `sha256sum`, for the ordinary native project source-to-WVB path
- .NET SDK 10.0.302 or a compatible later patch in the same feature band for
  Stage 0 development, verification, execution, packaging, and recovery
- PowerShell 7 on Windows, or a POSIX shell on Linux, for repository automation
- Node.js 24 when running the optional direct-WebAssembly engine verifier

The repository pins the SDK in `global.json` and uses no external NuGet packages.
The ordinary project source-to-WVB command itself does not invoke .NET.

## Source organization

Prefer focused source files with one clear owner or capability. When a source file
becomes difficult to navigate or review, split it along an existing responsibility
boundary when that produces clearer names and dependencies. This is maintainability
guidance, not a mandatory size limit: do not create arbitrary numbered fragments,
duplicate shared state, or force a split where the code is more coherent together.

## Development verification

Choose one verification level for a source state. The levels are nested alternatives, not a checklist: a passing broader level subsumes the narrower levels, and a commit or push does not invalidate that result. Rerun only after relevant inputs change. After fixing a failure, rerun the narrowest affected selection and use at most one broader final gate if warranted.

For the normal Windows inner loop, let changed paths select the relevant test areas:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Changed.ps1
```

Use the fast tier directly to select one or more areas and optionally narrow them by displayed test name:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1 `
  -Level Fast `
  -TestArea compiler,runtime `
  -TestFilter 'declaration namespaces' `
  -FailFast `
  -TimingReportPath artifacts/seed-timing-fast.json
```

Available Seed areas are `assembler`, `bytecode`, `compiler`, `database`, `foundation`, `golden`, `linker`, `object-model`, and `runtime`. `Verify-Changed.ps1` fails closed to all areas for broad or unrecognized implementation changes.

Fast runs regular tests by default. Tests that execute broad compiler closures, exact-compiler AOT transport, full-stage reproduction, or the golden contract are explicitly extended because a small group dominates suite time. To run one as a focused check, retain the area/filter selection and add `-IncludeExtended`; on Linux set `INCLUDE_EXTENDED=1`. Standard and Qualification always include every extended test.

`Development`, `Standard`, and `Qualification` have fixed suites, so omit `-TestArea`, `-TestFilter`, `-FailFast`, and `-IncludeExtended` at those levels. For the broad regular suite without the extended integration contracts, use:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1 -Level Development
```

Choose the gate that protects the changed boundary:

| Change or purpose | Usual gate |
| --- | --- |
| One implementation area or focused fix | `Verify-Changed.ps1` or a filtered `Fast` run |
| Coherent cross-area development batch | `Development` |
| Complete regular and extended in-process candidate | `Standard` |
| Release, qualification, or changed portable artifact identity | Cross-host `Qualification` |
| Compiler inventory or project change | Native `Verify-Bootstrap.cmd` or `.sh` once for the final candidate |
| OS boot, image, firmware, or kernel-seam change | Focused OS tests and the relevant live boot gate |

The no-argument verifier defaults to `Development`. Request complete Qualification explicitly on Windows:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1 -Level Qualification
```

On Linux:

```sh
VERIFY_LEVEL=qualification ./Tools/Verify/Verify-Seed.sh
```

Linux exposes the same tiers through `VERIFY_LEVEL`, comma-separated `TEST_AREAS`, `TEST_FILTER`, `FAIL_FAST`, `INCLUDE_EXTENDED`, and `TIMING_REPORT_PATH`; its default is also `development`.

### Standard and Qualification

`Standard` is the complete in-process candidate gate. It runs every regular and extended Seed test, including the golden contract, then runs the bounded Windvale OS in-process suite and writes the conformance report. It stops before the published command-line, process, filesystem, and native-artifact checks.

`Qualification` begins with that same complete in-process suite, then exercises the published CLI as an external tool. It verifies real file publication and preservation behavior, native Windows and Linux application artifacts, assembler/object/linker routes, malformed and rejected inputs, exact artifact identities, and Windvale-written versus Stage 0 agreement.

The recommended candidate workflow is:

1. Use `Verify-Changed.ps1`, a focused `Fast` selection, or `Development` while editing.
2. Run `Standard` once after a coherent higher-risk candidate has settled and before presenting it as complete in-process work.
3. Commit and push the unchanged candidate.
4. Let GitHub run the independent Windows and pinned-Debian `Qualification` jobs.
5. Run a local `Qualification` only when diagnosing that gate, preparing a critical release, or changing a boundary whose external process behavior cannot be established by `Standard`.

Do not run each tier sequentially against the same source state. A successful `Standard` already subsumes the local Fast and Development suites. A successful single-host `Qualification` is useful diagnostic evidence, but it is not a cross-host qualification claim; that claim requires the paired Windows and Debian results.

Fast and changed-file runs are development feedback, not qualification evidence. GitHub runs the independent dual-host Qualification gate for implementation and specification changes. Do not duplicate that gate locally merely because a commit or push follows. Record which broader checks were not run and why.

### Qualification follow-ups

After an exact implementation commit has passed the complete Windows/Linux Qualification gate, a follow-up that only records that result must not repeat the same long gate. Keep the promotion in the decision status, qualification-evidence ledger, progress/roadmap, changelog, and other ordinary documentation; run changed-document link/path inspection and `git diff --check`. Specifications should define the contract and link to its decision/evidence rather than require a second commit merely to replace “candidate” with “qualified.”

This shortcut applies only when code, tests, contract semantics, serialized bytes, artifact identities, and verifier expectations are unchanged. Any follow-up that changes those boundaries—or makes a new claim not established by the completed run—still requires the proportional focused checks and, when applicable, a fresh cross-host Qualification.

## Direct WebAssembly verification

On Windows, rebuild the Windvale-authored backend, lower thirteen retained fixtures spanning checked arithmetic, bounded straight-line `i32`, metered loops, sequential conditionals, and bounded direct calls through the `.wv` hosted tool, verify exact artifact sizes and hashes, and execute all thirteen modules in Node.js with:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-WebAssembly.ps1
```

The verifier requires execution ABI 1 to reset its exported evidence, return status `0` or `3007`, and publish the same result and attempted-instruction count as the retained arithmetic contracts. For execution ABI 2 it requires exact single-loop success at budget 157, `3011` exhaustion at 156, nonterminating-loop containment at 50, mixed sequential-control success/exhaustion at 184/183 and 331/330 across both conditional routes, two-`if` success/exhaustion at 41/40, shared-budget direct-call success/exhaustion at 66/65, and callee-overflow propagation as `3007/0/14`. Every retained repeat resets exactly. A successful local run is engine evidence, not Windows/Linux cross-host qualification or browser-worker evidence.

## Native compiler bootstrap verification

The digest-bound native bootstrap is separate from the normal development suite. Run it once for a final compiler candidate:

```bat
Tools\Verify\Verify-Bootstrap.cmd
```

```sh
./Tools/Verify/Verify-Bootstrap.sh
```

The verifier admits the versioned native compiler seed and publisher, rebuilds
Stage 1 from the exact project inventory, packages and executes that newly built
compiler, independently verifies Stage 2, and requires complete Stage 1/Stage 2
byte equality. The older managed Stage 0 → Stage 1 → Stage 2 proof remains
available only as `Tools/Recovery/Verify-Managed-Bootstrap.ps1` or `.sh`.

## Compile and run a portable program

Build the project through the ordinary native front door on Windows:

```bat
Tools\Native\Build-Wvb.cmd Examples\Seed\Sum-Data.wvproj Artifacts\Sum-Data.wvb
```

Or on Linux:

```sh
./Tools/Native/Build-Wvb.sh Examples/Seed/Sum-Data.wvproj Artifacts/Sum-Data.wvb
```

The launcher verifies its pinned native tools, builds a caller-owned candidate,
and invokes the exact native publisher for verifier-admitted atomic replacement.
Verify and inspect that WVB through the ordinary native route on Windows:

```bat
Tools\Native\Verify-Wvb.cmd Artifacts\Sum-Data.wvb
Tools\Native\Inspect-Wvb.cmd Artifacts\Sum-Data.wvb
```

Or on Linux:

```sh
./Tools/Native/Verify-Wvb.sh Artifacts/Sum-Data.wvb
./Tools/Native/Inspect-Wvb.sh Artifacts/Sum-Data.wvb
```

The retained Stage 0 runtime still owns execution:

```powershell
dotnet run --project Tools/Windvale.Tool -- run Artifacts/Sum-Data.wvb
```

Direct single-source compilation remains a Stage 0 development/recovery command:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Seed/Sum-Data.wv -o Artifacts/Sum-Data.wvb
```

On Windows, build and run the first narrow import-free native application target with:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Seed/Sum-Data.wv --target windows-x64-console-v1 -o artifacts/Sum-Data.exe
& ./artifacts/Sum-Data.exe
$LASTEXITCODE # 29
```

On Linux, build and run the matching sectionless static-PIE target with:

```sh
dotnet run --project Tools/Windvale.Tool -- compile Examples/Seed/Sum-Data.wv --target linux-x64-console-v1 -o artifacts/Sum-Data.elf
./artifacts/Sum-Data.elf
echo $? # 29
```

Both version-1 targets accept only capability-free `Main() -> i32` and use the Stage 0 compiler/tool host to construct the container; the generated process does not load .NET. Successful results `0` through `255` are observable unchanged on both hosts, while any other successful `i32` or native failure becomes process result `1`. The CLI stages the complete executable and Linux mode `0755` under a unique sibling name before one atomic replacement, so prepublication failure leaves an existing requested output unchanged. See the [Windows](../../Specifications/Windvale-Windows-Console-Application.md) and [Linux](../../Specifications/Windvale-Linux-Console-Application.md) console application specifications for their fixed ABI-22 context, arenas, verification, and deliberate limits.

To build the first standalone hosted console application, use the version-2 target with the existing hosted example:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Seed/Hello-Windvale.wv --target windows-x64-console-v2 -o artifacts/Hello-Windvale.exe
& ./artifacts/Hello-Windvale.exe
```

```sh
dotnet run --project Tools/Windvale.Tool -- compile Examples/Seed/Hello-Windvale.wv --target linux-x64-console-v2 -o artifacts/Hello-Windvale.elf
./artifacts/Hello-Windvale.elf
```

Version 2 accepts exactly `console.write_line`, serializes and independently verifies its [`WVHC 1`](../../Specifications/Windvale-Hosted-Console-Application.md) capability/service metadata, and retains Stage 0 only as a build-time container adapter. The application writes `Hello from Windvale` and exits with process result `0`.

## Run a hosted program

Hosted capabilities are denied unless granted explicitly:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Seed/Hello-Windvale.wv -o artifacts/Hello-Windvale.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Hello-Windvale.wvb --allow console.write_line
```

The runtime refuses this module without `--allow console.write_line`.

## Build a project manifest

Use the native front door for an ordinary project build:

```bat
Tools\Native\Build-Wvb.cmd Examples\Foundation\Module-Composition-Demo.wvproj Artifacts\Module-Composition-Demo.wvb
```

```sh
./Tools/Native/Build-Wvb.sh Examples/Foundation/Module-Composition-Demo.wvproj Artifacts/Module-Composition-Demo.wvb
```

The equivalent Stage 0 command remains available for recovery, differential
evidence, and development of tool boundaries that are not native yet:

```powershell
dotnet run --project Tools/Windvale.Tool -- build `
  Examples/Foundation/Module-Composition-Demo.wvproj `
  -o artifacts/Module-Composition-Demo.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Module-Composition-Demo.wvb
```

The manifest identifies one root and the explicit source files available to its imports. [`Windvale-Compiler.wvproj`](../../Windvale-Compiler.wvproj) is the complete compiler consumer used by bootstrap verification.

## Assemble and link

Assemble the canonical WVA example and verify its WVO object:

```powershell
dotnet run --project Tools/Windvale.Tool -- assemble Examples/Assembler/Hello-Object.wva -o artifacts/Hello-Object.wvo
dotnet run --project Tools/Windvale.Tool -- object-verify artifacts/Hello-Object.wvo
dotnet run --project Tools/Windvale.Tool -- object-inspect artifacts/Hello-Object.wvo
```

Link it with the example provider:

```powershell
dotnet run --project Tools/Windvale.Tool -- assemble Examples/Linker/Console-Provider.wva -o artifacts/Console-Provider.wvo
dotnet run --project Tools/Windvale.Tool -- link `
  --base-address 1048576 `
  --entry Main `
  -o artifacts/Hello-Linked.bin `
  artifacts/Hello-Object.wvo `
  artifacts/Console-Provider.wvo
```

The raw linked image is a memory-layout experiment, not itself a Windows, Linux, UEFI, or Windvale OS executable.

## Explore component examples

The maintained examples are grouped by the contract they exercise:

- [`Examples/Seed/`](../../Examples/Seed/) — portable and hosted source programs
- [`Examples/Foundation/`](../../Examples/Foundation/) — shared modules, project composition, bytecode inspection, and object production
- [`Examples/Compiler/`](../../Examples/Compiler/) — Windvale-written compiler phase demonstrations
- [`Examples/Assembler/`](../../Examples/Assembler/) — canonical WVA inputs
- [`Examples/Linker/`](../../Examples/Linker/) — multi-object link inputs

Use the [Seed implementation architecture](../Architecture/Seed-Implementation.md) for ownership and implemented boundaries, the [CLI specification](../../Specifications/Seed-CLI.md) for command contracts, and the [qualification evidence](../Project/Seed-Verification-Evidence.md) for reproducible artifact identities.
