# Windvale native changed-file verification

## Status and purpose

This contract defines the .NET-free Windows development front door for selecting
the narrowest owned native verification after a changed-file classification.
It composes existing native verification owners; it does not redefine their test
expectations or replace the final dual-host qualification gate.
Single-execution ownership for shared automatic meta-verification is recorded by
[Decision 0928](../Documents/Decisions/0928-Run-Shared-Development-Meta-Verification-Once.md).

## Planning contract

`Get-Native-Changed-Verification-Plan.ps1` accepts an explicit changed-path set,
normalizes separators and leading repository markers, removes empty and duplicate
values, and returns:

- native verification-owner names in the canonical manifest order;
- sorted, stable names for every uncovered evidence boundary;
- whether verification-plan and managed-entry inventory checks are required; and
- the OS x64 code-emission development target when every affected input belongs
  to one declared project closure, otherwise `all`;
- the library development target when every input that selects the library owner
  belongs to one declared dependency cluster, otherwise `all`;
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

Candidate `.wv` listings beneath
`Documents/Project/Language-1.0-Paper-Corpus/<workload>/Source/` and
`Documents/Project/Language-1.0-Localization-Workloads/<workload>/Source/` are
design evidence rather than inputs to the implemented Seed compiler. They
receive the same lightweight checks as their surrounding paper documents and
never select an implemented-language owner or an editor grammar check. This
exception is limited to those numbered workload trees; a `.wv` file elsewhere
must retain its normal implemented-source ownership or fail closed as an
uncovered boundary.

Exact files under a numbered localization workload's `Reference-Artifacts/`
directory are likewise paper inputs, not currently implemented package or
compiler formats. They receive lightweight integrity review until a source
freeze assigns executable parsers and focused verification owners. The exception
does not apply to the same extensions elsewhere.

Unknown input must never select every owner. The complete coordinator is
reserved for the final grouped gate, not used as changed-file fallback.

## Persistent development resume

The front door reuses a passing owner automatically when its complete version-1
result state and exact action match. Preparation measures the complete
non-ignored Git source tree plus the local host, boot, environment, and fixed
host-tool identities. The action binds the selected suite, actual command,
arguments, development scope, and cache format. A tracked-diff and
untracked-content sentinel must still match after the owner finishes before its
pass can be published.

Version 1 is intentionally conservative: one source-tree change selects a new
state and invalidates every prior owner receipt. It does not yet reuse an
unaffected owner across two different source trees. A commit or push with an
identical tree retains the state. Repeated or interrupted runs of that exact
state can therefore skip every owner that already passed, while a failed owner
runs again.

This optimization applies only to development execution through
`Verify-Changed.ps1`. Qualification and direct coordinator commands remain
fresh. `-NoResultCache` forces fresh development execution, while
`-ResultCacheRoot` provides an outside-repository test root. Any cache error
degrades to normal owner execution. The exact record validation, publication,
cleanup, and retention bounds are defined by the
[verification-owner contract](Windvale-Native-Verification-Owners.md).

The `workspace-project2` lane owns Workspace 1 and Project 2 parsing, workspace
containment, deterministic source ordering, and native publication. The `libraries`
lane owns 19 reusable or importing projects, eight conformance builds, and two
capability/profile rejection projects. Its development manifest groups those 29
projects into seven dependency clusters and derives each cluster's inputs from
the project declarations. One unambiguous cluster may run independently;
shared, multiple, owner, and unmapped owner inputs retain the complete lane.
Library, database, fixture, project, and contract paths outside that maintained
inventory use their actual focused owners and do not select `libraries` merely
because they occupy a broad library or database directory. The `packages` lane
owns the WVDB Query
application, its exact Package 1 / Lock 1 metadata, Project 2 input, locked library
parts, native package front doors, deterministic output identity, capability
inspection, negative admission, and failed-output preservation.

The `seed-native-front-door` lane is the ordinary pinned identity and admission
smoke. It binds the exact manifest and checksum-inventory identities, hashes all
18 Windows/Linux/WVB artifacts, and admits all six WVB modules through the
current-host verifier. Changes inside the checked-in front-door artifact family
and changes to the owner itself select this lane directly. Compiler source,
compiler-service project, and compiler-product-launcher changes use their mapped
behavioral and reconstruction owners without re-admitting an unchanged pinned
front door. Other historical source-transfer mappings remain explicit until
their affected-owner closures are audited. This lane does not reconstruct
products.

The former `seed-native-front-door-reconstruction` lane is retired. It combined
immutable Seed evidence with exact hashes for mutable current source and
duplicated focused owners. Deleted launcher names remain planner tombstones so
the retirement diff selects policy verification without opening a coverage gap.
The immutable front door, canonical Seed AOT chain, current split compiler
fixed points, assembler, object, linker, and runtime boundaries retain their
separate owners.

The `seed-native-console-aot` lane owns the standalone canonical `Sum-Data`
source-to-WVB, WVB-to-WVO, WVO admission, flat-link, paired version-1 console
packaging, and current-host execution chain. It constructs its exact WVB through
the qualified native Project 1 front door before invoking the paired host audit.
All three paths formerly classified with the `seed-native-console-aot` gap now
select this lane; no gap with that name remains in the native planner.

The `compiler-reconstruction` lane admits the exact retained candidate family
and runs a deterministic retained-to-current Project 2 differential smoke. The
independent Windows and Linux bootstrap jobs own cold current analyzer/emitter
fixed points plus current-verifier admission. Retained historical containers
are not reconstructed merely because current compiler source changed.

The `native-x64-lowering-development` lane is the ordinary current-source
WVB-to-WVO feedback boundary. It compiles the complete 648-function native
lowerer once, materializes its current-host executable through the exact
development package cache, compares inherited Return-42 and metadata WVO bytes,
executes one contained WVB 1.37 write-pointer image with result `42`, and rejects
ten malformed version, operand, nominal, and affine-ownership cases. Normal
lowerer implementation files and the tool root select this owner. Staging-only
sources retain segmented toolset reconstruction, while the complete lowerer
project manifest retains its downstream reconstruction closure. A warm cache
changes repeated feedback from hours of unrelated owner work to one roughly
32-second current-source check on the measured Windows host; this is development
evidence, not paired-host qualification.

The `source-containment` lane likewise separates affected compiler feedback
from complete dual-oracle evidence. Compiler and direct-compiler-artifact
changes execute all 500 fixed compiler containment cases through
`--compiler-only`; they do not launch the unchanged native assembler 500 times.
Changes to the containment corpus, implementation, runner, owner commands, or
specification retain the complete compiler-plus-assembler lane. No-argument and
qualification execution also remain complete.

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
unfiltered fallback is permitted. This is an explicit WebAssembly promotion
boundary, not part of every unrelated qualification. Its current migration to
the split compiler remains required before a new full current-source claim.

The independent Windows and Linux WebAssembly jobs in blanket qualification run
`Verify-WebAssembly-Engine.ps1`. They validate the digest-pinned package and
execute real compiler-core behavior without regenerating products. This is a
cross-host retained-package claim, not a substitute claim that current
WebAssembly sources were rebuilt.

The shared scalar-interpreter source retains several historical paths under
`Tests/Fixtures/WebAssembly`, but its non-browser envelope, main driver, and
fixed-integer, floating, and rune cores are inputs only to maintained native
runner/runtime projects. Those exact paths select the Language 1.0 front-door,
callable, and memory/resource owners and do not select complete WebAssembly
construction merely because of their directory name. Browser-prefixed inputs,
actual WebAssembly projects/backends/tools, and broad WebAssembly fixtures keep
their WebAssembly owners.

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

Ordinary source-compiler and source-language contract changes select the
`language-1-front-door` instead of using database storage or a legacy monolithic
compiler as a downstream proxy. The owner reconstructs the current analyzer and
emitter through the shared content-addressed split pipeline, compiles focused
programs twice, compares exact WVB bytes, verifies the results independently,
and executes them. Database storage remains selected for changes to its own
sources or contracts, shared cache tooling, and deliberate final milestone or
qualification evidence. A native-lowerer source edit does not select database
storage merely because a retained database image was once produced by a lowerer.

The canonical WVFC format and producer retain separate focused owners. A change
to their shared foreign-catalog contract selects both; producer source, project,
fixture, owner, or Decision 0888 selects
`language-1-foreign-catalog-producer`. That owner builds its candidate twice,
through the current reconstructed compiler, complete-verifies the immutable
WVB, acquires one content-addressed segmented profile-7 application, and
executes all 25 selectors directly. A change to `Build-Current-Wvb` selects
both this producer and the source-admission coordinator because both consume
that forward-language build boundary. The producer does not use the scalar
interpreter or a generic bounded-case runner.

The authenticated source-admission coordinator has its own focused
`language-1-source-admission-coordinator` owner. Direct coordinator or target-
admission source, project, fixture, and owner changes select it; changes to its
profile, descriptor, target, catalog, admission-evidence, SHA-256, or segmented-
hosted dependencies select it alongside the narrower owner of that dependency.
The owner uses the current reconstructed compiler to build the candidate twice,
requires byte identity and one pinned WVB identity, acquires a bounded native
profile-7 application, and executes all 28 live selector branches independently.
Those branches prove deterministic `WVSS`, `WVTD`, `WVFC`, and `WVAE` output on
success and empty publication of all four values on failure. This owner does not
claim a filesystem `wvadmit` or `wvauth` product.

The separate `language-1-production-admission-ingress` owner claims that hosted
boundary. Its default development mode builds target-aware `wvadmit`, complete
independent `wvauth`, private `wvbind`, the successor Analyzer, and the matching
current emitter once each through the validated shared development cache. It
requires each WVB to match its exact recorded size and SHA-256 identity, packages
the real products, and executes the private runner through 21 bounded acceptance,
tamper, bypass, sequencing, publication, cleanup, timeout, output-limit, and
progress cases. This reduces the owner from ten cold product compilations to at
most five on an empty development cache, while normal development runs can reuse
valid cached checkpoints.

Changes limited to the hosted `wvbind` driver source or its tool project select
only `language-1-production-admission-ingress`. That owner builds, packages, and
executes the real private product. The portable
`language-1-authenticated-foreign-binding` owner exercises the shared binding
core fixture instead, while `language-1-front-door` and
`compiler-split-development` do not currently build or execute `wvbind`.
Shared binding core, coordinator, Analyzer, specification, and fixture changes
retain their broader owner mappings because those paths cross more than the
hosted driver boundary.

Set
`WINDVALE_PRODUCTION_ADMISSION_INGRESS_COLD_DOUBLE_BUILD=1` for the explicit
qualification mode. That mode gives each build pass its own isolated empty cache,
builds all five products twice, requires the two WVB values for each product to
be byte-identical, and still requires the recorded size and SHA-256 identity
before packaging or execution. A shared-cache hit is evidence for the recorded
candidate identity, not a same-run cold double-build proof. The owner emits its
selected build mode and per-product build progress. Only the mode-specific
`cold-double-build=Verified` result claims same-run cold reproducibility. Direct
changes to the five product entry-point drivers, their project manifests, the
runner, the cached split-project builder, the focused owner, or Decision 0893
select this owner. The development-only admission helper and target-descriptor
writer select the front-
door owner that executes them.
Shared parser, coordinator, format, producer, and compiler sources retain their
narrow semantic owners; they do not force five cold double builds merely because
they occur in a pinned product closure. Exact product identities are refreshed
at an explicit production checkpoint. The production owner does not replace the
shared semantic owners.

The separate `language-1-authenticated-foreign-binding` owner claims the
compiler-owned semantic adapter rather than the hosted authentication boundary.
It reconstructs one current split-compiler pair, uses that identity to build two
bounded fixtures, packages two profile-7 native applications, and executes 24
isolated selectors. Those cases cover canonical binding; accepted declaration
layouts; foreign-free and foreign-bearing WVSD/WVSI version selection; complete
serialized-symbol malformations; callable-namespace collisions and import
visibility; exact positional and named calls; the full zero-through-four arity
matrix; first-class value, indirect, and generic-call rejection; catalog order,
correspondence, and normalized-fact tampering; unsupported targets; and the
early Foreign-count semantic bound. The callable-semantics owner separately
proves at the owning closure-capture phase that a Foreign global cannot enter an
explicit capture. The ordinary Analyzer's fail-closed foreign-input guard
belongs to the production-ingress owner, not these 24 selectors. Direct adapter,
either fixture/project, owner, or Decision 0895 changes select this owner.
Changes to the adapter implementation, Decision 0895, `wvbind`, or its runner
integration also select the production-ingress owner because digest evidence,
phase order, and retained-snapshot checks are hosted coordinator contracts.
The shared target, foreign-catalog, and foreign-catalog-authentication cores
select this semantic owner because the adapter directly consumes them; broader
producer-only dependencies do not acquire it transitively.

The `os-x64-code-emission` development lane reads the canonical version-2,
56-target manifest. Each row owns its project closure, artifact stem, expected
local result, and exact WVB, WVO, linked-image, Windows-container, and
Linux-container identities. One exact target is returned only when every
affected emission input belongs to that same project closure. Shared inputs,
multiple targets, changes to the paired owner scripts, malformed target data,
and otherwise ambiguous inputs select `all`. The paired owners execute one
generic host pipeline over the selected rows. An exact target retains the six
declared checks. Development `all` retains all 56 projects and 336 cases while
allowing dependency-keyed project-WVB checkpoints; qualification and
no-argument owner execution retain the same 336 cases with cold compilation.
A change to the manifest itself selects planner verification plus development
`all`.

Within one owner invocation, the paired OS x64 executors may stage private
copies of their pinned tools, validate those staged identities and workspace
containment once, and then process the selected rows. Every row still receives
independent native tool processes, candidate paths, immutable publication,
local execution, and exact final-byte checks. Session-scoped trust reuse must
not become a persistent compiler service. Explicit development execution may
derive all selected Project 2 keys and validate/materialize their immutable WVB
checkpoints in one Node process. Every miss still receives an independent
native compiler process. Qualification remains cache-independent.

## Dispatch contract

For lightweight and website changes, `Verify-Changed.ps1` retains the existing
whitespace, editor, and website behavior. For development or qualification-scoped
changes it:

1. computes the native plan before mutation or test execution;
2. refuses any nonempty gap set without invoking .NET;
3. runs the planner/inventory verifier when selected, except in the automatic
   Windows job that explicitly delegates shared verification to its required
   Linux peer;
4. runs the focused GitHub workflow verifier under the same shared delegation;
5. invokes each selected owner through
   `Invoke-WindvaleTests.ps1 -Owner <owner-name>` on both hosts, except that
   development-scoped `compiler-reconstruction` receives `--development`, and
   an eligible `database-storage`, `libraries`, or `os-x64-code-emission` owner
   receives its development target, including explicit checkpointed `all` when
   more than one OS x64 project is affected;
6. invokes either the pinned WebAssembly engine checkpoint or the complete
   construction-and-engine owner when selected, never both;
7. stops at the first test failure unless `-NoFailFast` is explicit;
8. classifies runner exit `1` as a product-test failure and runner exit `2` or
   `124` as verification-incomplete framework or timeout evidence;
9. permits `-AllowIncompleteInfrastructure` to keep only those incomplete
   outcomes nonblocking for automatic development feedback, without caching or
   treating them as a pass; and
10. optionally writes `windvale-native-changed-verification-timing-2` JSON with
    the normalized host, start time, overall outcome, incomplete owners, and
    per-owner outcomes and timing.

The command is development feedback. Passing it is not Standard, Qualification,
cross-host, or qualification evidence. A pre-existing output owned by a child
owner retains that owner's preservation contract.

## GitHub automatic verification

Push and pull-request development verification runs the affected plan on a clean
Linux host. It adds a clean Windows host only when a changed path names a Windows
or Win32 path token, a Windows command or batch file, a PowerShell script, or an
`.exe`, `.dll`, or `.pdb` artifact. Both automatic development jobs have a
15-minute wall-clock limit including checkout, cache restore, tool setup, owner
execution, and cache publication. A classified test failure remains blocking.
A framework error or owner timeout is retained as `verification-incomplete` and
warns without asserting that the product code is wrong. It is not a pass,
cannot populate the result cache, and cannot satisfy qualification.
Development-only Node setup, checkpoint restore/save, timing-history analysis,
and timing-artifact upload steps are nonblocking infrastructure conveniences.
The runner validates
Node.js 24 before owner execution, so a failed setup cannot silently run tests
with an unsupported runtime. Checkout, classification, routing, and classified
product-test failures remain blocking.

Linux owns the platform-neutral routing-plan and GitHub-workflow verifiers once
for an automatic development source state. The conditional Windows peer passes
`-SharedVerificationOnLinux` and omits only those shared checks; it still runs
the selected Windows owners. The switch rejects outside a GitHub Actions Windows
development process. The aggregate gate requires both jobs, so a shared-verifier
failure on Linux remains blocking. This delegation never applies to explicit
qualification or to host-specific owners.

Automatic runs for the same workflow and ref cancel when a newer push or pull
request supersedes them. An explicit `workflow_dispatch` qualification run is
not cancelled by later automatic work. Documentation verification runs once on
Linux; it is not repeated on Windows. Explicit qualification retains the four
fresh native shards, WebAssembly engine verification, and compiler convergence
on both hosts. Qualification explicitly authorizes the reported longer shard
plan, fails on every non-passing runner outcome, and retains each structured
shard result. Development jobs retain their structured timing reports and
analysis. Before cache publication, each host appends executed owner outcomes to
a bounded host-local timing history. History loss or analysis failure cannot
fail product verification, and the analyzer cannot modify duration policy.
Profile reduction requires the dual-host sample and margin rules in the native
owner contract.

## Verification

`Verify-Verification-Plan.ps1` owns general classification plus native selection
cases. It must cover deterministic ordering, exact suite ownership, combined
boundaries, frozen managed-source gaps, known missing native coverage, unknown
paths, planner self-verification, and empty input. The actual no-argument
working-tree route must also select the planner for changes to its own files.
