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
- the database development target when exactly one declared test-project closure
  owns all affected database inputs, otherwise `all`; and
- the normalized changed-path count.

Maintained Windvale compiler, bytecode, Foundation, object, assembler, linker,
OS, project, example, and native-tool paths select their existing focused owners.
Frozen managed implementation or test source selects a named recovery-source
gap rather than pretending its native replacement was exercised. Database,
GitHub qualification, unknown verification tools, unknown native tools,
unmapped specifications, and empty input likewise fail closed with explicit gap
names.

Unknown input must never select every suite. The complete 3,351-case coordinator
is reserved for the final grouped gate, not used as changed-file fallback.

The `workspace-project2` lane owns Workspace 1 and Project 2 parsing, workspace
containment, deterministic source ordering, and native publication. The `libraries`
lane owns the seven reusable library projects plus capability-bearing composition
and database conformance consumers. The `packages` lane owns the WVDB Query
application, its exact Package 1 / Lock 1 metadata, Project 2 input, locked library
parts, native package front doors, deterministic output identity, capability
inspection, negative admission, and failed-output preservation.

The `seed-native-front-door` lane is the ordinary pinned identity and admission
smoke. It binds the exact manifest and checksum-inventory identities, hashes all
18 Windows/Linux/WVB artifacts, and admits all six WVB modules through the
current-host verifier. Every maintained source path previously classified with
the `seed-native-front-door` evidence gap selects this fast lane. It does not
reconstruct products.

The separate `seed-native-front-door-reconstruction` lane retains the complete
105-artifact, 185-assertion Project 1 build, publication, verification,
inspection, execution, assembly, object, and linker audit. Changes to that owner
select it directly, and explicit complete qualification includes it through the
digest-bound retirement plan. Ordinary source changes do not run it merely to
reprove unchanged pinned artifacts.

The `seed-native-console-aot` lane owns the standalone canonical `Sum-Data`
source-to-WVB, WVB-to-WVO, WVO admission, flat-link, paired version-1 console
packaging, and current-host execution chain. It constructs its exact WVB through
the qualified native Project 1 front door before invoking the paired host audit.
All three paths formerly classified with the `seed-native-console-aot` gap now
select this lane; no gap with that name remains in the native planner.

WebAssembly has separate development-engine and complete-construction owners.
The `RunWebAssemblyEngineVerification` owner validates the checked-in browser
package and every referenced package identity, instantiates the pinned direct
compiler and interpreter Wasm, and exercises portable compilation, verification,
execution, capability denial/grant, and bounded-output behavior. It never
regenerates WVB or Wasm. Changes to that owner or the pinned playground package
select `Verify-WebAssembly-Engine.ps1`.

WebAssembly backend sources, generation tools, broad fixtures, project
manifests, exact native compiler/backend packages, the complete engine matrix,
and their specification select `RunWebAssemblyVerification`. The changed-file
front door dispatches `Verify-WebAssembly.ps1`, which reconstructs the complete
product set through the paired native host front doors and then executes the
broad engine and probe matrix. Complete construction subsumes the engine
checkpoint, so the planner never runs both for one source state. No managed or
unfiltered fallback is permitted.

The main GitHub workflow and its focused static verifier select one explicit
`RunGitHubQualificationVerification` owner. That owner requires the exact six
native qualification jobs, their pinned host/action dependencies, their native
commands, and the fail-closed required-gate join. It also runs the direct
managed-entry audit, which must report zero normal entries. Workflow changes no
longer report `github-native-qualification` or fall back to a broad local gate;
the committed workflow's independent jobs provide execution evidence.

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

The `database-storage` development lane derives target ownership from the root
and source entries of its fifteen maintained test projects. One exact target is
returned only when the changed database inputs resolve to one closure. Shared
sources, multiple targets, cache or database-owner tooling, and otherwise
ambiguous maintained database inputs select `all`. Hosted targets retain their
behavioral prerequisites: the tree reader consumes host-storage output, and the
engine and tree writer consume the reader's committed depth-two output.

## Dispatch contract

For lightweight and website changes, `Verify-Changed.ps1` retains the existing
whitespace, editor, and website behavior. For development or qualification-scoped
changes it:

1. computes the native plan before mutation or test execution;
2. refuses any nonempty gap set without invoking .NET;
3. runs the planner/inventory verifier when selected;
4. runs the focused GitHub workflow verifier when selected;
5. invokes each selected suite through `Test-Retirement-Suite.cmd --filter` on
   Windows or the paired `.sh` coordinator on non-Windows hosts, except that an
   eligible `database-storage` suite receives its exact development target;
6. invokes either the pinned WebAssembly engine checkpoint or the complete
   construction-and-engine owner when selected, never both;
7. stops at the first failure unless `-NoFailFast` is explicit; and
8. optionally writes `windvale-native-changed-verification-timing-1` JSON.

The command is development feedback. Passing it is not Standard, Qualification,
cross-host, or complete-retirement evidence. A pre-existing output owned by a
child suite retains that suite's preservation contract.

## Verification

`Verify-Verification-Plan.ps1` owns general classification plus native selection
cases. It must cover deterministic ordering, exact suite ownership, combined
boundaries, frozen managed-source gaps, known missing native coverage, unknown
paths, planner self-verification, and empty input. The actual no-argument
working-tree route must also select the planner for changes to its own files.
