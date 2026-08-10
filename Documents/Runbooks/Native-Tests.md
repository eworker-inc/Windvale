# Native test runbook

This runbook owns the first .NET-free repository test slice accepted by
[Decision 0218](../Decisions/0218-First-Native-Test-Orchestration.md). Its exact
inventory and non-claims are defined by the
[native test-plan contract](../../Specifications/Windvale-Native-Test-Plan.md).

## Retirement-suite coordinator

The digest-bound coordinator composes every transferred fixed native lane
without entering the managed Seed harness. On Windows x64, run one focused lane
with:

```bat
Tools\Native\Test-Retirement-Suite.cmd --filter unsafe-wvb
```

On Linux x64:

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter unsafe-wvb
```

The exact filter names and case counts are:

| Filter | Cases |
| --- | ---: |
| `seed` | 26 |
| `compiler-reconstruction` | 3 |
| `segmented-compiler-toolset-reconstruction` | 3 |
| `wvb-to-wvo-reconstruction` | 3 |
| `wvb-runner-reconstruction` | 3 |
| `wv-linker-reconstruction` | 3 |
| `wvo-inspector-reconstruction` | 3 |
| `console-verifier-reconstruction` | 3 |
| `console-publisher-reconstruction` | 3 |
| `wvo-publisher-reconstruction` | 2 |
| `baseline-jit` | 6 |
| `unsafe-wvb` | 20 |
| `wvb-containment` | 1,000 |
| `wvo-read-only` | 13 |
| `wvo-differential` | 256 |
| `wvo-containment` | 500 |
| `wvo-hostile-size` | 4 |
| `assembler-rejections` | 11 |
| `assembler-golden` | 4 |
| `wva-differential` | 269 |
| `source-containment` | 500 |
| `lowerer-rejections` | 2 |
| `linker-rejections` | 10 |
| `linker-hostile` | 200 |
| `linker-map-limit` | 1 |
| `console-packager-rejections` | 3 |
| `console-container-hostile` | 256 |
| `console-container-mutations` | 19 |
| `hosted-console-container-mutations` | 15 |
| `console-segmented-size` | 2 |
| `console-segmented-construction` | 2 |
| `console-packager-source-reconstruction` | 2 |
| `console-packager-container-reconstruction` | 4 |
| `publisher-rejections` | 4 |
| `hosted-verifier-publisher-files` | 15 |
| `uefi-packager` | 3 |
| `wvo-export-renamer` | 4 |
| `os-probe-object` | 11 |
| `os-kernel-target` | 7 |
| `os-process-policy` | 2 |
| `os-process-object` | 2 |
| `os-probe` | 4 |
| `aot-chain` | 1 |

Omitting `--filter` selects all 43 suites and 3,204 cases in manifest order. Its
terminal success line is:

```text
Suites: 43, Passed: 43, Failed: 0, Cases: 3204
```

Do not use the unfiltered command as another inner-loop level. It is reserved
for the final grouped retirement candidate unless the coordinator boundary
itself changes. The plan identity, child summaries, exit/channel behavior, and
failure rules are defined by the
[native retirement test-suite contract](../../Specifications/Windvale-Native-Retirement-Test-Suite.md).

The bounded baseline-JIT owner can be selected directly:

```cmd
Tools\Native\Test-Retirement-Suite.cmd --filter baseline-jit
```

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter baseline-jit
```

It runs the aggregate `WVJP 1` producer/verifier self-test and the five named
`WVLT 1` W^X publication behaviors. A current-host pass is not paired-host
qualification and does not claim the general JIT/backend is complete.

The segmented compiler toolset reconstruction owner can be selected directly:

```cmd
Tools\Native\Test-Retirement-Suite.cmd --filter segmented-compiler-toolset-reconstruction
```

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter segmented-compiler-toolset-reconstruction
```

It calls the durable constructor once, then treats each WVB and its paired exact
Windows/Linux applications as one case. The three cases cover the WVO staging
producer, compiler-image staging tool, and canonical image transport tool.

The current WVB-to-WVO reconstruction owner can be selected directly:

```cmd
Tools\Native\Test-Retirement-Suite.cmd --filter wvb-to-wvo-reconstruction
```

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter wvb-to-wvo-reconstruction
```

It verifies the five-file candidate inventory, calls its durable constructor
once, compares the rebuilt WVB and paired applications byte for byte, and
requires the current-host rebuilt lowerer to reproduce the fixed Return-42 WVO.

The retained-WVB runner reconstruction owner can be selected directly:

```cmd
Tools\Native\Test-Retirement-Suite.cmd --filter wvb-runner-reconstruction
```

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter wvb-runner-reconstruction
```

Its three cases verify the four-file candidate inventory, reconstruct the exact
WVO and paired profile-5 applications from the retained WVB, and exercise the
current-host runner with exact result and rejected-input reports. It does not
claim source-to-WVB closure or independent Linux execution.

The standard Wv-Linker reconstruction owner can be selected directly:

```cmd
Tools\Native\Test-Retirement-Suite.cmd --filter wv-linker-reconstruction
```

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter wv-linker-reconstruction
```

Its three cases verify the exact five-file linker candidate inventory, rebuild
the canonical WVB plus intermediate WVO and fragment through the retained
segmented stage, link, and transport path, reconstruct the paired profile-4
applications, and exercise the rebuilt current-host linker over a fixed link
vector. The segmented construction path avoids using the standard linker to
link its own successor. This remains retained same-release current-Windows-host
evidence, not independent Linux reconstruction, clean bootstrap, promotion, or
grouped qualification.

The WVO inspector reconstruction owner can be selected directly:

```bat
Tools\Native\Test-Retirement-Suite.cmd --filter wvo-inspector-reconstruction
```

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter wvo-inspector-reconstruction
```

Its three cases verify the exact candidate inventory, reconstruct the WVO
inspector WVB and paired Windows/Linux applications through the retained native
cross-target toolsets, and execute the current-host compatibility and profile
isolation matrix. This is current-Windows-host evidence; it is not independent
Linux execution or a clean previous-seed renewal. The accepted focused run
passes all three cases in 28.1 seconds.

The console-application verifier reconstruction owner can be selected directly:

```cmd
Tools\Native\Test-Retirement-Suite.cmd --filter console-verifier-reconstruction
```

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter console-verifier-reconstruction
```

Its three cases verify the exact candidate inventory, run the durable
constructor once and compare the rebuilt WVB, WVO, and paired applications,
then exercise the rebuilt current-host verifier over the fixed valid and
rejected console-container vectors. This focused lane does not claim an
independent Linux-host execution or the final grouped retirement gate.

The console-application publisher reconstruction owner can be selected directly:

```cmd
Tools\Native\Test-Retirement-Suite.cmd --filter console-publisher-reconstruction
```

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter console-publisher-reconstruction
```

Its three cases verify the exact four-file candidate inventory, rebuild the WVB
and WVO and construct both target applications, and exercise current-host
publication success plus rejected-input output preservation. The lane runs its
durable constructor for each target and retains publisher-family regression in
the existing focused owner. It does not claim independent Linux-host execution
or the final grouped retirement gate.

The role-3 WVO publisher reconstruction owner can be selected directly:

```cmd
Tools\Native\Test-Retirement-Suite.cmd --filter wvo-publisher-reconstruction
```

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter wvo-publisher-reconstruction
```

It verifies the three-file publisher candidate inventory, natively rebuilds
the publisher WVB with an exact completion transcript, constructs the paired
Windows and Linux applications through the role-3 pipeline, and compares all
three rebuilt products byte for byte. The two cases retain WVO behavioral and
publisher-rejection coverage in their existing focused lanes.

The ordinary and segmented console-packager container candidates have one
separate focused reconstruction owner:

```cmd
Tools\Native\Test-Retirement-Suite.cmd --filter console-packager-container-reconstruction
```

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter console-packager-container-reconstruction
```

It checks each three-file candidate inventory, rejects a missing constructor
destination, calls the durable constructor exactly once, requires its exact
channels, and compares both rebuilt WVB-and-paired-application families byte
for byte. The four cases keep source-to-WVO reconstruction in the separate
`console-packager-source-reconstruction` lane.

## Changed-file front door

On Windows, the ordinary local entry point is:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Changed.ps1
```

It maps the changed paths to focused suite filters in the manifest's canonical
order. Planner, inventory, website, and editor changes retain their own focused
checks. A maintained boundary without native evidence returns a stable named
gap and invokes neither the managed Seed verifier nor the unfiltered native
coordinator. Use `-PlanOnly` to inspect the selection without executing it.

The mapping and fail-closed rules are defined by the
[native changed-file verification contract](../../Specifications/Windvale-Native-Changed-Verification.md).
Passing this front door is development feedback, not cross-host qualification.

## Individual candidate commands

On Windows x64:

```bat
Tools\Native\Test-Seed.cmd
```

On Linux x64:

```sh
./Tools/Native/Test-Seed.sh
```

The command verifies the fixed test plan, builds project cases through the pinned
native source-to-WVB tools, decodes fixed WVB and WVO fixtures, compares every
complete input identity, and dispatches only to the pinned native runner, WVB
verifier, or WVO verifier.
Success is:

```text
PASS  calls-control
PASS  scalar-core
PASS  function-only
PASS  data-text
PASS  nominal-types
PASS  invalid-utf8
PASS  range-failure
PASS  u16-failure
PASS  malformed-bad-magic
PASS  malformed-bad-version
PASS  malformed-bad-utf8
PASS  malformed-truncated
PASS  malformed-trailing
PASS  malformed-typed-operator-stack-kind
PASS  malformed-typed-local-store-kind
PASS  malformed-typed-call-argument-identity
PASS  malformed-typed-record-receiver-identity
PASS  malformed-typed-enum-operand-identity
PASS  malformed-typed-branch-condition-kind
PASS  malformed-typed-declared-maximum-stack
PASS  malformed-typed-capability-argument-kind
PASS  malformed-control-unreachable-instruction
PASS  wvo-return-42
PASS  wvo-bad-magic
PASS  wvo-truncated
PASS  wvo-trailing
Tests: 26, Passed: 26, Failed: 0
```

The focused unsafe-WVB command reuses neither project builds nor successful
execution. On Windows x64 run:

```bat
Tools\Native\Test-Wvb-Unsafe-Rejections.cmd
```

On Linux x64 run:

```sh
./Tools/Native/Test-Wvb-Unsafe-Rejections.sh
```

Its exact success report is:

```text
PASS  unknown-opcode
PASS  truncated-operand
PASS  local-index
PASS  jump-target
PASS  after-return
PASS  record-parameter-type
PASS  record-field-index
PASS  duplicate-record-field
PASS  mismatched-enum-comparison
PASS  duplicate-nominal-name
PASS  mismatched-merge
PASS  bytes-length-on-i32
PASS  record-create-wrong-field-type
PASS  invalid-enum-member
PASS  enum-const-on-record
PASS  duplicate-enum-value
PASS  stack-capacity
PASS  record-field-on-primitive
PASS  enum-name-on-primitive
PASS  wrong-nominal-kind
Tests: 20, Passed: 20, Failed: 0
```

Both digest-bound WVB read-only launchers must reject every case with the exact
phase report and preserve the complete input. The command decodes fixed compact
fixtures; it does not mutate WVB or start .NET.

The positive assembler-golden lane admits four repository source identities,
assembles each twice, requires exact success reports and WVO identities, verifies
the first object independently, and compares both generated objects byte for
byte. Run it on Windows with:

```bat
Tools\Native\Test-Retirement-Suite.cmd --filter assembler-golden
```

On Linux x64:

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter assembler-golden
```

Success is:

```text
PASS  suite assembler-golden cases=4
Suites: 1, Passed: 1, Failed: 0, Cases: 4
```

The WVO differential lane freezes the Stage 0 acceptance decision for 128
single-byte mutations of the canonical sample plus 128 arbitrary values. It
includes 32 valid mutations, so this is not a rejection-only corpus. Run only
this lane on Windows with:

```bat
Tools\Native\Test-Retirement-Suite.cmd --filter wvo-differential
```

or on Linux:

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter wvo-differential
```

The native verifier must agree on all 32 accepted and 224 rejected rows and
preserve every input. Accepted rows require their exact digest-bearing success
report; rejected rows stay within one object-status diagnostic while the
separate thirteen-case WVO matrix owns exact status-family reports. The exact
terminal summary is `Tests: 256, Passed: 256, Failed: 0`.

The separate focused linker-rejection command does not rebuild source or repeat
the successful AOT chain. On Windows x64 run:

```bat
Tools\Native\Test-Linker-Rejections.cmd
```

On Linux x64 run:

```sh
./Tools/Native/Test-Linker-Rejections.sh
```

Its exact success report is:

```text
PASS  invalid-base
PASS  malformed-object
PASS  aggregate-limit
PASS  duplicate-export
PASS  undefined-import
PASS  kind-mismatch
PASS  missing-entry
PASS  layout-overflow
PASS  absolute-overflow
PASS  relative-overflow
Tests: 10, Passed: 10, Failed: 0
```

All ten cases require deterministic linker rejection, empty standard output, an
exact report identity, and byte-for-byte preservation of an existing output.
The internal `WVL1011` reconstruction trap retains separate internal evidence.

The fixed hostile-linker corpus is intentionally separate from those diagnostic
families. It expands one 63,224-byte digest-bound archive into 200 manifest-owned
zero-through-511-byte inputs, then requires exact `WVL1002` plus complete input
and destination preservation for each public native-linker invocation. Run its
focused lane with:

```bat
Tools\Native\Test-Retirement-Suite.cmd --filter linker-hostile
```

or on Linux:

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter linker-hostile
```

The command generates no random value while running. Its exact success summary
is `Tests: 200, Passed: 200, Failed: 0`.

The canonical-map size boundary has its own compact generated-fixture command.
It expands two digest-bound WVOs from a 21,046-byte archive, combines them with
the existing entry object into exactly 16,384 definitions, and invokes only the
native linker. On Windows x64 run:

```bat
Tools\Native\Test-Linker-Map-Limit.cmd
```

On Linux x64 run:

```sh
./Tools/Native/Test-Linker-Map-Limit.sh
```

Its exact success report is:

```text
PASS  canonical-map-limit
Tests: 1, Passed: 1, Failed: 0
```

The command requires exact `WVL1012`, empty standard output, and preservation
of the entry, both generated WVOs, and the existing output. It does not build
the WVA sources, start .NET, or repeat the successful AOT chain.

The focused console-packager rejection command likewise avoids source building,
linking, and successful AOT execution. On Windows x64 run:

```bat
Tools\Native\Test-Console-Packager-Rejections.cmd
```

On Linux x64 run:

```sh
./Tools/Native/Test-Console-Packager-Rejections.sh
```

Its exact success report is:

```text
PASS  entry-at-end
PASS  invalid-entry
PASS  empty-image
Tests: 3, Passed: 3, Failed: 0
```

All cases require the current-host native packager to reject before publication,
write no standard output, emit the exact host-target report identity, and leave
the complete pre-existing destination unchanged.

The console-container hostile lane expands one digest-bound archive into 128 PE
and 128 ELF candidates, then drives both suffix-selected portable verifier paths
through the current-host native console publisher. Run only this lane with:

```bat
Tools\Native\Test-Retirement-Suite.cmd --filter console-container-hostile
```

or on Linux:

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter console-container-hostile
```

Every manifest-owned case requires exact rejection, empty standard output,
unchanged input and destination, and zero `.wvpublish-*` scratch. Inputs include
explicit zero and 4,096-/9,000-byte boundaries and are never generated during
the run. The exact terminal summary is
`Tests: 256, Passed: 256, Failed: 0`.

The hosted console-container lane uses two fixed valid format-2 applications
and thirteen exact managed mutations. It does not rebuild either container or
generate mutations during the run. On Windows use:

```bat
Tools\Native\Test-Retirement-Suite.cmd --filter hosted-console-container-mutations
```

On Linux use:

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter hosted-console-container-mutations
```

Both valid candidates must publish byte-identically. Every rejected candidate
must emit the exact console-application report, preserve the destination
sentinel, and leave no scratch. All inputs remain unchanged. The exact terminal
summary is `Tests: 15, Passed: 15, Failed: 0`.

The segmented console-size lane uses a separate read-only verifier because each
first chunk is exactly 4 MiB and cannot be joined with its second chunk into one
ordinary Windvale byte value. On Windows use:

```bat
Tools\Native\Test-Retirement-Suite.cmd --filter console-segmented-size
```

On Linux use:

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter console-segmented-size
```

The fixed corpus contains the target-marked Windows and Linux maximum-plus-one
inputs. The runner requires exact portable rejection ordering, empty standard
output, and unchanged hashes for both chunks. It does not invoke the managed
target-specific oracle. The exact terminal summary is
`Tests: 2, Passed: 2, Failed: 0`.

The focused publisher command tests pre-replacement admission and cleanup without
running a successful package or lower operation. On Windows x64 run:

```bat
Tools\Native\Test-Publisher-Rejections.cmd
```

On Linux x64 run:

```sh
./Tools/Native/Test-Publisher-Rejections.sh
```

Its exact success report is:

```text
PASS  console-application
PASS  hosted-verifier-application
PASS  hosted-verifier-publisher
PASS  wvo
Tests: 4, Passed: 4, Failed: 0
```

All four cases require exact phase diagnostics, empty standard output, complete
candidate and destination preservation, and zero native publication scratch files.

The focused WVB-to-WVO lowerer command tests malformed admission and one valid
module outside the accepted native subset without rebuilding the lowerer or
running the successful AOT chain. On Windows x64 run:

```bat
Tools\Native\Test-Lowerer-Rejections.cmd
```

On Linux x64 run:

```sh
./Tools/Native/Test-Lowerer-Rejections.sh
```

Its exact success report is:

```text
PASS  malformed
PASS  unsupported-function
Tests: 2, Passed: 2, Failed: 0
```

Both cases require exact native status diagnostics, empty standard output,
complete destination preservation, and no residual private lowerer work.

The focused WVA assembler rejection command covers every stable diagnostic
family without rebuilding the already-qualified assembler. On Windows x64 run:

```bat
Tools\Native\Test-Assembler-Rejections.cmd
```

On Linux x64 run:

```sh
./Tools/Native/Test-Assembler-Rejections.sh
```

Its exact success report contains `PASS  wva1001` through `PASS  wva1011` in
order, followed by:

```text
Tests: 11, Passed: 11, Failed: 0
```

Each family requires an exact input and report identity, exit `2`, empty standard
output, and complete destination preservation. The source-limit case generates a
temporary one-byte-over-limit zero-filled input rather than retaining a very
large fixture.

The WVA differential lane freezes the exact 200-case seeded mutation sequence
and 69 managed positive register/control/relocation vectors. Run the complete lane on
Windows with:

```bat
Tools\Native\Test-Retirement-Suite.cmd --filter wva-differential
```

or on Linux:

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter wva-differential
```

The native assembler must match all 199 Stage 0 rejection codes, preserve each
source and rejected destination, and reproduce all 70 accepted Stage 0 WVOs.
Every WVO then passes the native verifier with its exact digest report. Two
compact archives retain all 217 exact inputs without adding loose or very large
source files. The exact terminal summary is
`Tests: 269, Passed: 269, Failed: 0`.

For the narrow positive-matrix inner loop, use:

```bat
Tools\Native\Test-Wva-Differential.cmd --positive-only
```

```sh
./Tools/Native/Test-Wva-Differential.sh --positive-only
```

That selection ends with `Tests: 69, Passed: 69, Failed: 0` and does not rerun
the unchanged 200-case mutation corpus.

No .NET process is required by these commands. The host dependencies are
`cmd.exe`, `certutil`, `fsutil`, and `tar` on Windows, or Bash, `sha256sum`,
`base64`, `tar`, `cmp`, `truncate`, and core utilities on Linux.

## Hosted-container composition

After reviewing the launcher, candidate inventory, and the two focused cases,
run exactly one host-specific command:

```powershell
Tools\Native\Test-Hosted-Wvb-Packaging.cmd
```

```sh
./Tools/Native/Test-Hosted-Wvb-Packaging.sh
```

The first case must reproduce the pinned orchestration-control application
byte for byte through the complete native hosted-container path. The second
must reject a fixed invalid WVB, preserve that input and a pre-existing
destination exactly, and leave no private package directory. The terminal
summary is `Tests: 2, Passed: 2, Failed: 0`.

These commands are separate from the fixed retirement-suite coordinator until
the hosted-container candidate is promoted. Run the Windows and Linux halves
from the same fetched commit during the final grouped gate; do not run one host
script repeatedly or use a passing Windows result as Linux execution evidence.

## Windvale OS boot execution

The ordinary native build candidate constructs every Probe 40 object for the
normal, invalid-opcode, and general-protection scenarios and does not invoke
`.NET`:

```bat
Tools\Native\Build-Os-Probe.cmd C:\path\to\BOOTX64.EFI
Tools\Native\Build-Os-Probe.cmd C:\path\to\INVALID.EFI invalid-opcode
Tools\Native\Build-Os-Probe.cmd C:\path\to\GENERAL.EFI general-protection
```

```sh
./Tools/Native/Build-Os-Probe.sh /path/to/BOOTX64.EFI
./Tools/Native/Build-Os-Probe.sh /path/to/invalid.efi invalid-opcode
./Tools/Native/Build-Os-Probe.sh /path/to/general.efi general-protection
```

It compiles, lowers, and export-renames the canonical admission source;
compiles and lowers the canonical native-probe
source; constructs the focused x64 exception, paging, WVB admission-bridge, and
native bridge/support objects through one digest-bound Windvale-native producer;
constructs the selected architecture-fault memory object through a separate
focused producer;
constructs the normal UEFI loader object from a separately pinned architecture
fixture and focused producer; compiles the canonical system-kernel source to WVB
and lowers it through the bounded Windvale-native kernel target; compiles,
lowers, and export-renames the portable process-policy source through the
general native tools; rebuilds the process object from its canonical Windvale
sources, WVA shims, versioned records, and one reviewed architecture fixture;
assembles three top-level WVA objects natively; links fourteen inputs; and
packages the exact EFI. Use the focused retirement lane to check all three exact
constructions plus existing-output preservation:

```bat
Tools\Native\Test-Retirement-Suite.cmd --filter os-probe
```

The normal object seed is now empty. All eleven formerly frozen objects have
moved to native producers. Use the recovery command below only to regenerate
and compare Stage 0 provenance.

To exercise the kernel target directly:

```bat
Tools\Native\Lower-Os-Kernel-Wvb.cmd input.wvb output.wvo
Tools\Native\Test-Retirement-Suite.cmd --filter os-kernel-target
```

```sh
./Tools/Native/Lower-Os-Kernel-Wvb.sh input.wvb output.wvo
./Tools/Native/Test-Retirement-Suite.sh --filter os-kernel-target
```

To construct the process-policy object directly:

```bat
Tools\Native\Build-Os-Process-Policy-Object.cmd output.wvo
Tools\Native\Test-Retirement-Suite.cmd --filter os-process-policy
```

```sh
./Tools/Native/Build-Os-Process-Policy-Object.sh output.wvo
./Tools/Native/Test-Retirement-Suite.sh --filter os-process-policy
```

To construct the normal process object directly:

```bat
Tools\Native\Build-Os-Process-Object.cmd output.wvo
Tools\Native\Test-Retirement-Suite.cmd --filter os-process-object
```

```sh
./Tools/Native/Build-Os-Process-Object.sh output.wvo
./Tools/Native/Test-Retirement-Suite.sh --filter os-process-object
```

This path regenerates the three embedded images, canonical program, resource
store, and directory snapshot. Only the 46,678-byte process machine-code section
is a reviewed architecture fixture. The focused test checks exact final identity,
independent WVO admission, output preservation, and private-work cleanup.

To construct and independently admit any focused object, use:

```bat
Tools\Native\Produce-Os-Probe-Object.cmd exceptions output.wvo
Tools\Native\Produce-Os-Probe-Object.cmd wvb-admission-bridge output.wvo
Tools\Native\Produce-Os-Probe-Object.cmd native-bridge-and-support output.wvo
Tools\Native\Produce-Os-Probe-Object.cmd paging output.wvo
Tools\Native\Produce-Os-Probe-Object.cmd memory output.wvo
Tools\Native\Produce-Os-Probe-Object.cmd memory-invalid-opcode output.wvo
Tools\Native\Produce-Os-Probe-Object.cmd memory-general-protection output.wvo
Tools\Native\Produce-Os-Probe-Object.cmd loader output.wvo
```

```sh
./Tools/Native/Produce-Os-Probe-Object.sh exceptions output.wvo
./Tools/Native/Produce-Os-Probe-Object.sh wvb-admission-bridge output.wvo
./Tools/Native/Produce-Os-Probe-Object.sh native-bridge-and-support output.wvo
./Tools/Native/Produce-Os-Probe-Object.sh paging output.wvo
./Tools/Native/Produce-Os-Probe-Object.sh memory output.wvo
./Tools/Native/Produce-Os-Probe-Object.sh memory-invalid-opcode output.wvo
./Tools/Native/Produce-Os-Probe-Object.sh memory-general-protection output.wvo
./Tools/Native/Produce-Os-Probe-Object.sh loader output.wvo
```

Run `--filter os-probe-object` when any recipe, its shared WVO constructor, or
its launcher changes. The fixed digests and structural admission remain usable
after the managed recovery generators are archived or removed.

To rename one admitted WVO export through the digest-bound native tool, use:

```bat
Tools\Native\Rename-Wvo-Export.cmd input.wvo Main Link_name output.wvo
```

```sh
./Tools/Native/Rename-Wvo-Export.sh input.wvo Main Link_name output.wvo
```

The exact transformation and rejection rules are defined by the
[WVO export-renamer contract](../../Specifications/Windvale-Wvo-Export-Renamer.md).

The ordinary boot verifier consumes an already constructed EFI application; it
does not build one or invoke `dotnet`. Bind the supplied bytes explicitly:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 `
    -EfiPath <probe.efi> `
    -ExpectedEfiSha256 <64-lowercase-hex-digest> `
    -Scenario normal
```

The verifier copies the admitted image into a unique FAT root, runs the pinned
QEMU/firmware contract, checks the complete scenario marker, and proves that
the supplied image, private copy, firmware code, and variable-store template
did not change. Run only the affected scenario during the inner loop. User-fault
and service-fault still need native image construction; all five scenarios
belong to the final OS qualification gate.

To reconstruct an image through the explicit recovery boundary, use:

```powershell
pwsh -NoProfile -File Tools/Recovery/Rebuild-Os-Probe.ps1 `
    -OutputPath <new-probe.efi> `
    -Scenario normal
```

Stage 0 produces eleven reviewed Probe 40 WVOs. Before invoking it, the recovery
command assembles the init-service, directory-service, boot-resource, and
scenario-selected client WVA objects through the current host's digest-bound
native assembler. It passes that exact four-object directory into Stage 0 for
checked process-image composition. The command then assembles the top-level
memory-object, timer, and kernel shims natively, admits all seven exact hashes,
restores the reviewed fourteen-object order, invokes the digest-bound native
linker, parses its canonical entry address, and invokes the digest-bound native
UEFI packager. It refuses an existing destination and removes its private WVA,
object, linked-payload, and EFI-candidate paths. It is provenance and
differential infrastructure, not the normal boot path; remaining source
compilation, native lowering, three inner links, and other object/scenario
production still require .NET. Managed WVA assembly, top-level linking, and
UEFI packaging are retained only as recovery/differential implementations.

## Current boundary

The 3,204-case coordinator is a candidate fixed native gate, not the complete normal
repository verifier. It covers the transferred result, runtime-failure,
malformed-WVB/WVO, WVO and WVA differential, assembler, lowerer, linker,
console/UEFI packager, publisher, and AOT-chain contracts. It does not replace the remaining
complete unsafe, arbitrary-source/WVB randomized, representative WVA, golden,
OS, or bootstrap suites. Continue to select one appropriate Stage 0 verifier
for changes outside these transferred fixtures.

Before running a filter, review its child command, fixtures, and expected
summary against the changed behavior; update them first if the contract changed.
Then run only `Test-Retirement-Suite.cmd --filter <suite-name>` or its `.sh`
counterpart once. Reuse that result while relevant inputs remain unchanged. Do
not also run the child directly or progress through broader local levels for the
same source state. Immediately before the final grouped candidate, update from
the shared branch and run the unfiltered Windows/Linux suite as part of the one
broad qualification gate.
