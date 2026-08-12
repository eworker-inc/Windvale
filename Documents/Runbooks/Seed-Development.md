# Windvale Seed development runbook

This runbook is the practical entry point for building, testing, and exploring the current Windvale Seed implementation. [AGENTS.md](../../AGENTS.md) and [CONTRIBUTING.md](../../CONTRIBUTING.md) remain authoritative for contribution, provenance, verification, and Git workflow requirements.

## Prerequisites

- Windows x64 with the inbox command processor, or Linux x64 with Bash and
  `sha256sum`, for the ordinary native project source-to-WVB path
- .NET SDK 10.0.302 or a compatible later patch in the same feature band for
  explicit Stage 0 recovery and managed differential verification
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

For the normal Windows inner loop, let changed paths select focused native suites:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Changed.ps1
```

The planner refuses maintained boundaries whose native evidence has not moved
yet; it never falls back to the complete native gate or .NET. Use `-PlanOnly` to
inspect its selected suites and named gaps. When a managed differential is
explicitly required, use the recovery Fast tier directly and narrow it by area
and displayed test name:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1 `
  -Level Fast `
  -TestArea compiler,runtime `
  -TestFilter 'declaration namespaces' `
  -FailFast `
  -TimingReportPath artifacts/seed-timing-fast.json
```

Available managed Seed areas are `assembler`, `bytecode`, `compiler`, `database`, `foundation`, `golden`, `linker`, `object-model`, and `runtime`. An unrecognized changed path now produces a named native gap rather than selecting all managed areas.

The remaining managed verifier is frozen recovery and differential tooling. Its
Fast tier runs regular tests by default. Tests that execute broad compiler
closures, exact-compiler AOT transport, full-stage reproduction, or the golden
contract are explicitly extended because a small group dominates suite time. To
request one for a focused recovery diagnosis, retain the area/filter selection
and add `-IncludeExtended`; on Linux set `INCLUDE_EXTENDED=1`. Managed Standard
and Qualification always include every extended test.

The managed `Development`, `Standard`, and `Qualification` tiers have fixed
suites, so omit `-TestArea`, `-TestFilter`, `-FailFast`, and `-IncludeExtended`
when explicit recovery or final comparison evidence requires one. For example,
the broad regular managed recovery suite is:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1 -Level Development
```

Choose the gate that protects the changed boundary:

| Change or purpose | Usual gate |
| --- | --- |
| One implementation area or focused fix | Native `Verify-Changed.ps1` |
| Coherent cross-area development batch | Native `Verify-Changed.ps1` after the batch settles |
| Explicit Stage 0 differential diagnosis | Filtered managed `Fast` run |
| Final retirement candidate | One fetched, settled cross-host retirement/Qualification gate |
| Compiler inventory or project change | Native `Verify-Bootstrap.cmd` or `.sh` once for the final candidate |
| OS boot, image, firmware, or kernel-seam change | Focused OS tests and the relevant live boot gate |

The no-argument managed recovery verifier defaults to `Development`. Request its
complete Qualification tier explicitly on Windows only when the final retirement
gate or a named diagnosis requires it:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1 -Level Qualification
```

On Linux:

```sh
VERIFY_LEVEL=qualification ./Tools/Verify/Verify-Seed.sh
```

Linux exposes the same managed recovery tiers through `VERIFY_LEVEL`,
comma-separated `TEST_AREAS`, `TEST_FILTER`, `FAIL_FAST`, `INCLUDE_EXTENDED`, and
`TIMING_REPORT_PATH`; its default is also `development`.

### Standard and Qualification

Managed `Standard` is the complete in-process recovery/differential candidate
gate. It runs every regular and extended Seed test, including the golden
contract, then runs the bounded Windvale OS in-process suite and writes the
conformance report. It stops before the published command-line, process,
filesystem, and native-artifact checks.

Managed `Qualification` begins with that same complete in-process suite, then
exercises the published CLI as an external tool. Until the final cutover it
remains comparison evidence for real file publication, native Windows and Linux
artifacts, assembler/object/linker routes, malformed inputs, exact identities,
and Windvale-written versus Stage 0 agreement; it is not the normal inner loop.

The retirement candidate workflow is:

1. Use native `Verify-Changed.ps1` once after each coherent slice settles.
2. Commit and push that unchanged slice without rerunning a passing check.
3. Close named native gaps one at a time; use managed Fast only for an explicit
   differential question.
4. After all slices settle, fetch and reconcile latest once, then run the complete
   Windows/Linux retirement and Qualification gate once on the exact candidate.
5. Use managed Standard or Qualification outside that end gate only to diagnose a
   failure that focused native evidence cannot explain.

Do not run each tier sequentially against the same source state. A successful
managed `Standard` already subsumes its Fast and Development suites. A successful
single-host `Qualification` is useful diagnostic evidence, but it is not a
cross-host qualification claim; that claim requires the paired Windows and
Debian results.

Fast and changed-file runs are development feedback, not qualification evidence. GitHub runs the independent dual-host Qualification gate for implementation and specification changes. Do not duplicate that gate locally merely because a commit or push follows. Record which broader checks were not run and why.

### Qualification follow-ups

After an exact implementation commit has passed the complete Windows/Linux Qualification gate, a follow-up that only records that result must not repeat the same long gate. Keep the promotion in the decision status, qualification-evidence ledger, progress/roadmap, changelog, and other ordinary documentation; run changed-document link/path inspection and `git diff --check`. Specifications should define the contract and link to its decision/evidence rather than require a second commit merely to replace “candidate” with “qualified.”

This shortcut applies only when code, tests, contract semantics, serialized bytes, artifact identities, and verifier expectations are unchanged. Any follow-up that changes those boundaries—or makes a new claim not established by the completed run—still requires the proportional focused checks and, when applicable, a fresh cross-host Qualification.

## Direct WebAssembly verification

On Windows or Linux, build the retained source/WVB corpus and both exact compiler adapters
through the native front doors, lower every admitted input through the
manifest-bound native WebAssembly compiler, verify the exact artifacts, and run
the strict Node.js engine plus record-arena and compiler probes with:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-WebAssembly.ps1
```

The verifier contains no normal .NET invocation and selects the paired native
host scripts. It requires execution ABI 1 to reset its exported evidence,
return status `0` or `3007`, and publish the same result and attempted-instruction
count as the retained arithmetic contracts. For execution ABI 2 it requires
exact single-loop success at budget 157, `3011` exhaustion at 156,
nonterminating-loop containment at 50, mixed sequential-control
success/exhaustion at 184/183 and 331/330 across both conditional routes,
two-`if` success/exhaustion at 41/40, shared-budget direct-call
success/exhaustion at 66/65, and callee-overflow propagation as `3007/0/14`.
Every retained repeat resets exactly. Measured complete routes take roughly
23–27 minutes across the current Windows and independent Linux hosts.
`Verify-Changed.ps1` dispatches this owner once for
WebAssembly-owned changes, after any cheaper fixed suites pass. A successful
local run is engine evidence; paired independent host reports remain necessary
for Windows/Linux qualification, and neither local report is browser-worker
evidence.

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

Run the fixed 156-case native front-door qualification smoke directly with:

```powershell
$output = New-Item -ItemType Directory -Force artifacts/seed-front-door
pwsh -NoProfile -File Tools/Verify/Verify-Seed-Native-Front-Door.ps1 `
  -OutputDirectory $output.FullName
```

```sh
mkdir -p artifacts/seed-front-door
./Tools/Verify/Verify-Seed-Native-Front-Door.sh artifacts/seed-front-door
```

The helper builds 97 exact WVBs. It retains the original Project 1,
verification, inspection, execution, instruction-count, and malformed-project
cases; it additionally builds and inspects Machine Contracts, Byte Ordering,
Decimal Parsing, and Byte Construction, builds all four demos, and executes the
first three demos to exact result `0`. It also builds the native-stencil,
UTF-8, integer-format, and service-code products and natively inspects the
seven products with public ownership surfaces. Byte Construction's 4 MiB
execution and profiling and the Stencil demo's 20-million-step execution remain
in the frozen managed differential lane. Example and component `.wvproj` files
normally live beside their owning source, with paths resolved relative to the
manifest. Only genuine cross-component aggregates currently remain at the
repository common ancestor. A later workspace/package design may organize
those aggregates without changing Project 1 or permitting directory escape.

The helper also builds the eleven output, file-output, and file-input service
products and natively inspects their three public bridges. Their embedded-WVB
and exact platform-leaf comparisons remain in the broad scripts.

It additionally builds the text-concatenation, text-quote, enum-name,
enum-metadata, native-publication, and service-bundle-materialization core and
bridge pairs, and natively inspects eleven ownership surfaces. Ten of these
manifests are component-local. The service-bundle core and bridge remain root
aggregates because they span Compiler, Foundation, and Runtime sources.

The helper also builds the output, file-output, file-input, service,
execution-context, argument, entry, and byte-result-admission core/bridge
pairs, and natively inspects all eight public bridges. All sixteen manifests
are component-local. Their retained bridge-WVB and exact fragment comparisons
remain in the broad scripts.

The helper then builds hosted-tool metadata admission/construction, startup
instantiation, all four hosted-container products, runtime-header construction,
and publication lifetime, and natively inspects nine public or core surfaces.
The broad scripts retain exact bridge-WVB, hosted-startup WVO, and linked
fragment comparisons. Projects stay component-local only when their entire
closure has one owner; cross-component Foundation/Runtime/Linker aggregates
remain at repository root.

The helper then builds the source-lexer core/demo,
declaration-parser core/demo/tool, and body-parser core/demo/tool and natively
inspects the three core type/export surfaces. These manifests remain root
aggregates because their complete closures span Compiler or Examples plus
Foundation. The broad scripts still run the three demos and five
capability-bearing tools through Stage 0; the current native runner does not
complete the demos, and the tools require explicit console, diagnostic, file,
and process capabilities.

The helper finally builds the source-set, source-graph, and source-symbol
core/demo/tool products and natively inspects the three core type/export
surfaces. These nine manifests are also root aggregates because their complete
closures span Compiler or Examples plus Foundation. Keep their dependencies in
canonical module-name order while the pinned native Project driver's documented
order-sensitivity defect remains; this is a compatibility workaround, not a
Project 1 ordering requirement. The broad scripts still run the three demos and
three capability-bearing tools through Stage 0. Native demo probes stop with
runtime code `3004`, and the scalar profile does not bind the tools' console,
diagnostic, file, and process capabilities.

It then builds the source-bindings, typed-WVIR, and source-WVB core/demo/tool
products and natively inspects the three core type/export surfaces. The generic
native Project front door owns the bindings and WVIR families. Use the bounded
source-compiler-product launcher for the WVB family while the pinned generic
driver cannot compile the current closure:

```bat
Tools\Native\Build-Source-Compiler-Product.cmd tool Artifacts\Source-Wvb-Tool.wvb
```

```sh
./Tools/Native/Build-Source-Compiler-Product.sh tool Artifacts/Source-Wvb-Tool.wvb
```

The launcher accepts `core`, `demo`, or `tool`, binds the exact selected
manifest and source inventory, compiles privately through the native compiler
seed, and publishes through the qualified native publisher. The broad scripts
retain the three demo runs, bindings/WVIR hosted tools, and complete source-WVB
fixture/differential/oracle sequence.

Run the exact native capability-free console-AOT qualification composition in
the same output directory after `Sum-Data.wvb` exists:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed-Native-Console-Aot.ps1 `
  -OutputDirectory $output.FullName
```

```sh
./Tools/Verify/Verify-Seed-Native-Console-Aot.sh artifacts/seed-front-door
```

This lowers and verifies the WVO, requires the complete flat-link map, packages
the exact Windows and Linux version-1 products from one image, and executes the
current-host product to result `29`. The paired broad Seed scripts invoke it
immediately after the native front-door helper.

The retained Stage 0 runtime still owns the remaining general and parser
execution lane:

```powershell
dotnet run --project Tools/Windvale.Tool -- run Artifacts/Sum-Data.wvb
```

Direct single-source compilation remains a Stage 0 development/recovery command:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Seed/Sum-Data.wv -o Artifacts/Sum-Data.wvb
```

For an independent Stage 0 recovery/differential comparison of the Windows
version-1 writer, use:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Seed/Sum-Data.wv --target windows-x64-console-v1 -o artifacts/Sum-Data.exe
& ./artifacts/Sum-Data.exe
$LASTEXITCODE # 29
```

For the corresponding Stage 0 Linux recovery/differential comparison, use:

```sh
dotnet run --project Tools/Windvale.Tool -- compile Examples/Seed/Sum-Data.wv --target linux-x64-console-v1 -o artifacts/Sum-Data.elf
./artifacts/Sum-Data.elf
echo $? # 29
```

Both version-1 contracts accept only capability-free `Main() -> i32`. The
ordinary canonical qualification smoke composes the Windvale-native build,
lower, verify, link, and package front doors; the commands above retain the
frozen Stage 0 compiler/tool host as an independent writer and recovery oracle.
The generated process itself does not load .NET. Successful results `0`
through `255` are observable unchanged on both hosts, while any other
successful `i32` or native failure becomes process result `1`. Publication
stages the complete executable and Linux mode `0755` under a unique sibling
name before one atomic replacement, so prepublication failure leaves an
existing requested output unchanged. See the
[Windows](../../Specifications/Windvale-Windows-Console-Application.md) and
[Linux](../../Specifications/Windvale-Linux-Console-Application.md) console
application specifications for their fixed ABI-22 context, arenas,
verification, and deliberate limits.

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
