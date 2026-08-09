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
| `unsafe-wvb` | 20 |
| `wvb-containment` | 1,000 |
| `wvo-read-only` | 13 |
| `wvo-differential` | 256 |
| `wvo-containment` | 500 |
| `wvo-hostile-size` | 4 |
| `assembler-rejections` | 11 |
| `assembler-golden` | 3 |
| `wva-differential` | 200 |
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
| `publisher-rejections` | 2 |
| `aot-chain` | 1 |

Omitting `--filter` selects all 24 suites and 3,048 cases in manifest order. Its
terminal success line is:

```text
Suites: 24, Passed: 24, Failed: 0, Cases: 3048
```

Do not use the unfiltered command as another inner-loop level. It is reserved
for the final grouped retirement candidate unless the coordinator boundary
itself changes. The plan identity, child summaries, exit/channel behavior, and
failure rules are defined by the
[native retirement test-suite contract](../../Specifications/Windvale-Native-Retirement-Test-Suite.md).

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

The positive assembler-golden lane admits three repository source identities,
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
PASS  suite assembler-golden cases=3
Suites: 1, Passed: 1, Failed: 0, Cases: 3
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
PASS  wvo
Tests: 2, Passed: 2, Failed: 0
```

Both cases require exact phase diagnostics, empty standard output, complete
destination preservation, and zero native publication scratch files.

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
from the managed Stage 0 test. Run only this lane on Windows with:

```bat
Tools\Native\Test-Retirement-Suite.cmd --filter wva-differential
```

or on Linux:

```sh
./Tools/Native/Test-Retirement-Suite.sh --filter wva-differential
```

The native assembler must match all 199 Stage 0 rejection codes, preserve each
source and rejected destination, and reproduce the sole accepted 243-byte WVO.
That WVO then passes the native verifier with its exact digest report. The
compact archive retains all 200 exact 432-byte sources without adding loose or
very large source files. The exact terminal summary is
`Tests: 200, Passed: 200, Failed: 0`.

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

## Current boundary

The 986-case coordinator is a candidate fixed native gate, not the complete normal
repository verifier. It covers the transferred result, runtime-failure,
malformed-WVB/WVO, WVO and WVA differential, assembler, lowerer, linker,
packager, publisher, and AOT-chain contracts. It does not replace the remaining
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
