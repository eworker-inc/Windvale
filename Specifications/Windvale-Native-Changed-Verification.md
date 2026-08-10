# Windvale native changed-file verification

## Status and purpose

This contract defines the .NET-free Windows development front door for selecting
the narrowest owned native verification after a changed-file classification.
It composes existing native retirement suites; it does not redefine their test
expectations or replace the final dual-host qualification gate.

## Planning contract

`Get-Native-Changed-Verification-Plan.ps1` accepts an explicit changed-path set,
normalizes separators and leading repository markers, removes empty and duplicate
values, and returns:

- native retirement-suite names in the canonical manifest order;
- sorted, stable names for every uncovered evidence boundary;
- whether verification-plan and managed-entry inventory checks are required; and
- the normalized changed-path count.

Maintained Windvale compiler, bytecode, Foundation, object, assembler, linker,
OS, project, example, and native-tool paths select their existing focused owners.
Frozen managed implementation or test source selects a named recovery-source
gap rather than pretending its native replacement was exercised. Database,
GitHub qualification, unknown verification tools, unknown native tools,
unmapped specifications, and empty input likewise fail closed with explicit gap
names.

Unknown input must never select every suite. The complete 3,180-case coordinator
is reserved for the final grouped gate, not used as changed-file fallback.

## Dispatch contract

For lightweight and website changes, `Verify-Changed.ps1` retains the existing
whitespace, editor, and website behavior. For qualification-scoped changes it:

1. computes the native plan before mutation or test execution;
2. refuses any nonempty gap set without invoking .NET;
3. runs the planner/inventory verifier when selected;
4. invokes each selected suite through `Test-Retirement-Suite.cmd --filter` on
   Windows or the paired `.sh` coordinator on non-Windows hosts;
5. stops at the first failure unless `-NoFailFast` is explicit; and
6. optionally writes `windvale-native-changed-verification-timing-1` JSON.

The command is development feedback. Passing it is not Standard, Qualification,
cross-host, or complete-retirement evidence. A pre-existing output owned by a
child suite retains that suite's preservation contract.

## Verification

`Verify-Verification-Plan.ps1` owns general classification plus native selection
cases. It must cover deterministic ordering, exact suite ownership, combined
boundaries, frozen managed-source gaps, known missing native coverage, unknown
paths, planner self-verification, and empty input. The actual no-argument
working-tree route must also select the planner for changes to its own files.
