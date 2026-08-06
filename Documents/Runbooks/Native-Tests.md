# Native test runbook

This runbook owns the first .NET-free repository test slice accepted by
[Decision 0218](../Decisions/0218-First-Native-Test-Orchestration.md). Its exact
inventory and non-claims are defined by the
[native test-plan contract](../../Specifications/Windvale-Native-Test-Plan.md).

## Candidate commands

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
PASS  missing-entry
PASS  malformed-object
Tests: 3, Passed: 3, Failed: 0
```

All three cases require deterministic linker rejection, empty standard output,
an exact report identity, and byte-for-byte preservation of an existing output.

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

No .NET process is required by these commands. The host dependencies are `cmd.exe`
and `certutil` on Windows, or Bash, `sha256sum`, `base64`, `cmp`, and core utilities
on Linux.

## Current boundary

This is a candidate portable result/runtime-failure/fixed-malformed-WVB-and-WVO
gate, not the complete normal repository verifier. Its WVB cases reach semantic,
typed-execution, and control-reachability rejection, while its small WVO matrix
covers one accepted object plus bad magic, truncation, and trailing bytes. They do
not replace the complete unsafe and randomized malformed corpora. Continue to
select one appropriate Stage 0 verifier for changes outside these transferred
fixtures. Do not run this candidate and progressively broader local levels
merely as a checklist; use the narrowest gate that owns the changed behavior.

For changes to the native plan, launchers, projects, or fixed fixtures, review the
managed wrapper's exact report and run `Tools\Native\Test-Seed.cmd` or
`./Tools/Native/Test-Seed.sh` directly once. For the focused linker-rejection
boundary, review its wrapper and run only `Test-Linker-Rejections.cmd` or `.sh`.
For the packager-rejection boundary, do the same with
`Test-Console-Packager-Rejections.cmd` or `.sh`; for publisher admission, use
only `Test-Publisher-Rejections.cmd` or `.sh`. GitHub owns the independent Windows
and pinned-Debian Qualification run for a final committed candidate.
