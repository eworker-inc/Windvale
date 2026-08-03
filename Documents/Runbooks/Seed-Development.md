# Windvale Seed development runbook

This runbook is the practical entry point for building, testing, and exploring the current Windvale Seed implementation. [AGENTS.md](../../AGENTS.md) and [CONTRIBUTING.md](../../CONTRIBUTING.md) remain authoritative for contribution, provenance, verification, and Git workflow requirements.

## Prerequisites

- .NET SDK 10.0.302 or a compatible later patch in the same feature band
- PowerShell 7 on Windows, or a POSIX shell on Linux
- Node.js 24 when running the optional direct-WebAssembly engine verifier

The repository pins the SDK in `global.json` and uses no external NuGet packages.

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

Available Seed areas are `assembler`, `bytecode`, `compiler`, `foundation`, `golden`, `linker`, `object-model`, and `runtime`. `Verify-Changed.ps1` fails closed to all areas for broad or unrecognized implementation changes.

`Development`, `Standard`, and `Qualification` have fixed suites, so omit `-TestArea`, `-TestFilter`, and `-FailFast` at those levels. For the broad regular suite without the very long golden contract, use:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1 -Level Development
```

Choose the gate that protects the changed boundary:

| Change or purpose | Usual gate |
| --- | --- |
| One implementation area or focused fix | `Verify-Changed.ps1` or a filtered `Fast` run |
| Coherent cross-area development batch | `Development` |
| Complete in-process conformance candidate | `Standard` |
| Release, qualification, or changed portable artifact identity | Cross-host `Qualification` |
| Compiler inventory, project, or convergence change | `Verify-Bootstrap.ps1` or `.sh` once for the final candidate |
| OS boot, image, firmware, or kernel-seam change | Focused OS tests and the relevant live boot gate |

The no-argument verifier defaults to `Development`. Request complete Qualification explicitly on Windows:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1 -Level Qualification
```

On Linux:

```sh
VERIFY_LEVEL=qualification ./Tools/Verify/Verify-Seed.sh
```

Linux exposes the same tiers through `VERIFY_LEVEL`, comma-separated `TEST_AREAS`, `TEST_FILTER`, `FAIL_FAST`, and `TIMING_REPORT_PATH`; its default is also `development`.

Fast and changed-file runs are development feedback, not qualification evidence. GitHub runs the independent dual-host Qualification gate for implementation and specification changes. Do not duplicate that gate locally merely because a commit or push follows. Record which broader checks were not run and why.

## Direct WebAssembly verification

On Windows, rebuild the Windvale-authored backend, lower thirteen retained fixtures spanning checked arithmetic, bounded straight-line `i32`, metered loops, sequential conditionals, and bounded direct calls through the `.wv` hosted tool, verify exact artifact sizes and hashes, and execute all thirteen modules in Node.js with:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-WebAssembly.ps1
```

The verifier requires execution ABI 1 to reset its exported evidence, return status `0` or `3007`, and publish the same result and attempted-instruction count as the retained arithmetic contracts. For execution ABI 2 it requires exact single-loop success at budget 157, `3011` exhaustion at 156, nonterminating-loop containment at 50, mixed sequential-control success/exhaustion at 184/183 and 331/330 across both conditional routes, two-`if` success/exhaustion at 41/40, shared-budget direct-call success/exhaustion at 66/65, and callee-overflow propagation as `3007/0/14`. Every retained repeat resets exactly. A successful local run is engine evidence, not Windows/Linux cross-host qualification or browser-worker evidence.

## Compiler bootstrap convergence

Bootstrap convergence is separate from the normal development suite because it executes billions of verified VM instructions. Run it once for a final compiler candidate:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Bootstrap.ps1
```

```sh
./Tools/Verify/Verify-Bootstrap.sh
```

The verifier builds Stage 1 with the C# recovery compiler, asks that Windvale bytecode compiler to build Stage 2 from the canonical source inventory, independently verifies both modules, and requires complete byte equality.

## Compile and run a portable program

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Seed/Sum-Data.wv -o artifacts/Sum-Data.wvb
dotnet run --project Tools/Windvale.Tool -- verify artifacts/Sum-Data.wvb
dotnet run --project Tools/Windvale.Tool -- inspect artifacts/Sum-Data.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Sum-Data.wvb
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

Version 2 accepts exactly `console.write_line`, serializes and independently verifies its [`WVHC 1`](../../Specifications/Windvale-Hosted-Console-Application.md) capability/service metadata, and retains Stage 0 only as a build-time container adapter.

The result is `Result: 29`.

## Run a hosted program

Hosted capabilities are denied unless granted explicitly:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Seed/Hello-Windvale.wv -o artifacts/Hello-Windvale.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Hello-Windvale.wvb --allow console.write_line
```

The runtime refuses this module without `--allow console.write_line`.

## Build a project manifest

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
