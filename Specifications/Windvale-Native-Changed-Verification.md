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

Unknown input must never select every suite. The complete 3,206-case coordinator
is reserved for the final grouped gate, not used as changed-file fallback.

The `seed-native-front-door` lane owns the exact native Project 1 build,
publication, verification, inspection, execution, assembly, object, and linker
report audit formerly reachable only after a broad managed gate. Every path
previously classified with the `seed-native-front-door` evidence gap now selects
this focused native lane, including changes to the paired host audit scripts.
No `seed-native-front-door` gap remains in the native planner.

The `seed-native-console-aot` lane owns the standalone canonical `Sum-Data`
source-to-WVB, WVB-to-WVO, WVO admission, flat-link, paired version-1 console
packaging, and current-host execution chain. It constructs its exact WVB through
the qualified native Project 1 front door before invoking the paired host audit.
All three paths formerly classified with the `seed-native-console-aot` gap now
select this lane; no gap with that name remains in the native planner.

The complete WebAssembly generation-and-verification owner remains a distinct
long-running command rather than a fixed retirement-coordinator suite.
WebAssembly backend sources, tools, fixtures, project manifests, exact native
packages, verifier scripts, and their specification therefore select the
stable `webassembly-native-verification` gap. That gap directs a maintainer to
run `Verify-WebAssembly.ps1` on Windows and prevents a managed or unfiltered
fallback. It remains a gap until the same command contract has an independently
executed Linux owner that the changed-file front door can dispatch.

The `console-verifier-reconstruction` lane owns its exact candidate,
constructor, test command, project, and Windvale source closure. Its direct
lowering, linking, assembly, hosted-verifier toolsets, profile-7 sources,
inspector startups, and required service leaves also select the lane. Generic
console changes and unused file-output leaves do not select it merely by name.

The `wvb-runner-reconstruction` lane owns its four-artifact candidate,
source-building constructor, focused owner, project and runner sources,
profile-5 WVHV closure, inspector startups, build/lower/link dependencies,
launcher, and nine service leaves. Changes to any member of the Project 1
closure therefore select the exact reconstruction owner.

The `console-publisher-reconstruction` lane owns the exact console-application
publisher candidate, constructor, test command, project, source, and contract.
Its source closure and direct native build, lowering, linking, profile-2 hosted
container, publisher-overlay, and publication-object dependencies also select
the lane. Only `Package-Console` and `Publish-Console` are console consumers of
this publisher; other tools do not select the lane merely because their names
contain `Console`.

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
