# Verification throughput redesign plan

> Status: Active implementation plan
> Authority: Informative
> Last reviewed: 2026-09-06

## Goal

Windvale verification must make the common correct action inexpensive. A
developer should be able to change one compiler, runtime, library, or database
contract and receive relevant behavioral feedback in seconds where practical
and in a few minutes otherwise. Adding tests must add mostly execution work,
not another complete compile, lower, link, and cross-package pipeline.

Complete qualification must continue to bind one exact source state, rebuild
the evidence that changed, execute every required behavior on its required
host, and fail closed when coverage or provenance is incomplete. Faster does
not mean sampling, trusting an unvalidated cache, or silently weakening the
meaning of `Qualified`.

The working performance targets are:

| Feedback boundary | Warm target | Clean target | Maximum local development bound |
| --- | ---: | ---: | ---: |
| Planner and coverage validation | 1 second | 3 seconds | 10 seconds |
| One affected behavior or small closure | 5 seconds | 30 seconds | 60 seconds |
| Ordinary changed-file development gate | 30 seconds | 3 minutes | 10 minutes |
| Complete database qualification on one host | Not applicable | 5 minutes | Explicit qualification only |
| Complete paired-host repository qualification | Not applicable | 10 minutes wall clock | Explicit qualification only |

These are redesign targets, not current claims or pass thresholds. Measure each
phase on Windows and Linux before making a target enforceable.

## Current checkpoint

Hosted cache/session edits now select their focused owner instead of a
2,540-second database plan. The existing session test is part of that owner,
which passed twelve cases in 4,968 ms on Windows and 10,041 ms on Debian; the implementation changed-file
gate, including routing checks, passed in 53,824 ms. Producer contexts retain 3,560
bytes of fingerprints instead of about 24.4 MB of file contents. Four bounded
reads preserve every producer check and reduced observed Debian preparation
from 2,733 to 850 ms. The paired packaging workload changed from 31,628 to
23,962 ms on Debian and from 23,684 to 23,390 ms on Windows. The
[producer-context evidence](../Evidence/2026-09-06-Hosted-Producer-Fingerprints.json)
records the host measurements and memory limits. Wider qualification and
compiler-product reuse remain unfinished.

Segmented hosted packaging now shares the staged, linked, and transported
native image across application profiles, while retaining a separate container
and producer check for each profile. On the 82,115-byte enum-request workload,
the two-profile cached run changed from 27,260 to 23,684 ms on Windows and from
34,749 to 31,628 ms on Debian. Both profiles matched uncached package bytes and
passed native behavior checks. The
[shared-image evidence](../Evidence/2026-09-06-Segmented-Image-Profile-Reuse.json)
also records cases where cache overhead exceeds saved construction. Large
compiler packaging and complete qualification still need measurements; this
change does not establish the overall feedback targets.

The routing guard now reads executable modes once for all 126 owners, shares
immutable planner initialization across the eleven development dependency
closures, and checks filesystem existence only for relevant retirement inputs.
All 31 general and 280 native routing cases still pass. The complete Windows
guard took 38,607 ms against the preceding 47,216 ms observation; it remains
above the planner/coverage target. The
[setup-reuse evidence](../Evidence/2026-09-06-Verification-Guard-Setup-Reuse.json)
records component costs, the fixed quoted-command detection gap, and the lack
of Linux PowerShell measurements. Repeated process startup and remaining
construction work still need reduction.

Hosted database qualification now constructs each ordinary product once and
packages only the image its host executes. Six hosted construction sites no
longer repeat opposite-host assembly, linking, or packaging. Dedicated packager
owners retain that construction coverage; paired Windows/Linux database
behavior remains required for complete qualification. Recovery, reopen,
interruption, and object-admission checks remain in place. The focused Windows
storage-plus-root-writer run fell from 234,129 to 137,140 ms in these observations.
The storage step passed in 78,060 ms on Windows and 80,000 ms on Debian. These
runs used retained tools and fresh test products; they are not clean-machine or
complete qualification measurements. The
[current-host packaging evidence](../Evidence/2026-09-06-Hosted-Database-Current-Host-Packaging.json)
records the scope and limits. Repeated construction across different hosted
products and the broader qualification graph remain optimization work.

Mixed changes now preserve each owner's focused development selection. The
planner excludes paths already routed to documentation-only checks from
Foundation borrow, publisher, and front-end narrowing. Previously, adding a
README change to the borrow-plan source changed its estimate from 30 to 900
seconds and requested the complete execution owner. A generic-declaration
project plus README similarly selected all six front-end products at 330
expected seconds instead of one at 20. Those mixed plans now retain the 30-
and 20-second estimates. These are planning estimates, not measured execution
speedups. Other selected owners and documentation checks still execute; shared
dependencies, implementation companions, and unknown coverage still fail
conservatively. Owner routing is not yet a complete construction dependency
inventory, so it cannot justify ignoring other implementation changes. Complete
qualification remains hours long and requires further construction reuse.

The front-door development checkpoint now has one six-product inventory shared
by Windows and Linux. It reuses exact project and native-package products while
executing every selected behavior again. Descriptor reproducibility still uses
two independent constructions. Changed-file planning can select exact project
closures instead of all six products; unknown dependencies conservatively keep
the full checkpoint. The 329 development claims and separate 492-case
qualification contract are retained. Current measurements and limits are recorded
in the [focused development-product evidence](../Evidence/2026-09-04-Front-End-Development-Product-Reuse.json); clean-machine and paired-host
performance qualification remain separate work.

The work planner also keeps historical timing case counts separate from the
current registry. Adding a test reports a timing-coverage mismatch instead of
breaking planning or rewriting old evidence. Timings remain advisory and never
grant passing evidence for the added cases.

Phase 1 is complete for the database owner, is now in progress across complete
qualification, and Phase 2 has started. One versioned database
qualification-step inventory drives both host wrappers, distinguishes
57 logical database cases from three portable runtime prerequisites, and emits
exact rows, counts, source-closure duplication, and per-step elapsed time without
running the owner. A focused step can be selected for diagnostics without making
a complete qualification claim.

Six compatible case pairs now share products, reducing the inventory from 60 to
54 execution steps while preserving all 57 logical cases. The current graph has
58 project references across 57 unique manifests and 673 root/source references
across 146 unique source paths, a 4.61-fold declaration overlap. Pairing every
construction would visit those source references 1,346 times per host. The 42
ordinary portable steps now consume one admitted construction, delegating 385
duplicate source visits to focused reproducibility owners. Portable steps also
package and execute only the current-host image; generic opposite-host packaging
is delegated to its focused owners. Only the previously rejected three-case
branch-pages closure remains as a static bundling candidate. Hosted migration,
build-once graph execution, capacity-aware bundling, and paired-host
qualification remain pending.

The planner now also ranks the twelve strongest non-identical portable pair
candidates by shared declarations and bytes, union size, and potential source-
visit reduction. These measurements are discovery evidence, not permission to
merge: every candidate must still compile within the existing limits and run
both behaviors. The first ranked bounded trial outside the known branch-pages
capacity risk combined transaction leaf groups and leaf pages, which share 10
of 23 declarations. It passed in 56,140 ms without increasing a limit and is the
fifth retained bundle. A second ranked trial combined root split and depth two;
it passed in 59,990 ms without increasing a limit and is the sixth retained
bundle.

The six retained bundle projects and their root fixtures now route to their
exact two-case development selectors. The version-3 development inventory binds
those cases back to the qualification bundle membership and distinguishes 53
logical development cases from 47 physical executions. A complete pair now
plans one 65-second execution with a 210-second bound and dispatches the bundle
project once; selecting only one member still dispatches its one-case project.
The publication/recovery bundle took 44,850 ms while creating its development
checkpoints and 2,130 ms with project, link, and application cache hits. Full-
owner changes report focused development estimates as `not-applicable` instead
of the misleading value zero.

The completed paired-host qualification now supplies one exact elapsed value for
every owner on Windows and Linux. It showed that the equal 4,890-second profile
assignment still projected a 6,547,869 ms critical shard because broad duration
classes are timeout policy, not accurate weights. Reassigning six independent
owners produces projected shard maxima of 4,655,707 ms on Windows and 4,521,081
ms on Linux, 28.90 percent below the preceding measured projection and only
4.01 percent above the arithmetic lower bound. This is a scheduling-only
projection pending a run of the new assignment; all 126 owners and 5,981 baseline
cases remain selected. The later 12 payload-borrow cases have no timing claim
from that historical run.

Compatible development-result reuse is also implemented as a bounded candidate.
One focused six-case owner first executed in 15,682 ms; after four unrelated
documentation paths changed, its next development run reused the earlier result
through current-planner delta proof instead of executing the owner again. This
does not alter cold qualification and does not permit reuse across a changed
owner dependency, planner gap, repository, host identity, or global cache-proof
input. A subsequent result-cache implementation change exercised that last
barrier and reran all six cases in 16,188 ms rather than reusing the receipt.

The first compiler-owner boundary is now implemented. The 492-case
`language-1-front-door` owner exposes a 329-case development checkpoint covering
frozen source evidence, descriptor construction, value-front-end behavior, and
the first generic products. The exact committed checkpoint took 200,727 ms on
GitHub-hosted Windows and 172,336 ms on Linux. The historical complete owner took
2,761,285 ms on Windows and 2,362,071 ms on Linux, so the selected feedback path
removes 92.73 and 92.70 percent of the respective observed wait while leaving
the no-argument qualification command and summary unchanged.

The next compiler product merge is implemented as a development candidate.
When at least two of generic nominal type binding, layout, and materialization
are selected, their wrappers use one 108-case product but retain three distinct
owner receipts. A focused Windows miss built, packaged, and executed the
971,313-byte WVB in 292,203 ms; all three wrappers then reused the immutable
product and completed in 8,017 ms total. A four-way merge including the WVLB
carrier produced 604 functions and a 1,171,385-byte unpublished module, so it
was rejected without raising a product limit. Qualification still owns four
independent fresh products; Linux execution of the development bundle remains
pending.

Ten hosted dependency edges are now explicit in the same database inventory.
A focused hosted request expands its transitive prerequisites in topological
order; for example, `HostEngine` selects `HostStorage`, `HostTreeReader`, then
`HostEngine`. Focused diagnostics no longer depend on ambient artifacts from a
previous invisible step.

The scope is the entire Windvale verification system, not only the database
example. The read-only [qualification work planner](../../Tools/Verify/Plan-Qualification-Work.mjs)
now inventories the canonical owner registry, duration profiles, both host
wrappers, same-named JavaScript orchestration modules, repeated project
references, nested owner calls, and common build/lower/link/package entry
points without executing qualification.

## Complete qualification baseline

The current registry contains 126 owners and 5,993 declared cases. Its coarse
duration profiles sum to 19,560 expected seconds per host (5 hours 26 minutes)
and 79,200 maximum seconds. The measured assignment intentionally has an uneven
declared distribution: 2,535, 4,950, 4,815, and 7,260 expected seconds. Those
values remain conservative timeout-policy categories and are not used as claims
about current wall time.

The exact completed qualification baseline contains 17,904,835 ms of Windows
owner work and 16,467,401 ms of Linux owner work. Its owner ranking is very
different from the profile ranking: `language-1-front-door` took 2,761,285 ms on
Windows, `wvb-runner-reconstruction` took 2,592,004 ms on Linux,
`language-1-memory-budget-split-execution` took 2,569,776 ms on Linux, and
`database-storage` took 2,460,171 ms on Windows. The timing manifest remains a
single qualified observation, not an enforced regression threshold.

The planner exposes one analysis row for every owner. The current scan covers
277 wrapper/orchestration files, 44,525 source lines, and 215 distinct
owner-to-project references, so the long-owner ranking can be drilled into
without starting any of the owners.

Static orchestration inspection also finds 221 `Build-Wvb` call sites across 48
owners, 124 `Link-Wvo` call sites across 39 owners, and 111
`Package-Hosted-Wvb` call sites across 19 owners. These counts are candidate
construction nodes, not proof of duplication: each must be classified by exact
inputs and failure signal before it is merged or removed. The planner currently
finds no top-level owner directly invoking another registered owner, so the
largest waste is inside owner pipelines and shared project construction rather
than obvious owner nesting.

The long-owner shape is not uniform. `language-1-front-door` contains 237
references to the selected common pipeline helpers across its two host scripts,
while database storage contains 86 across 66 statically visible projects. Those
are immediate graph-extraction candidates. In contrast,
`language-1-authenticated-foreign-binding` exposes one segmented construction
helper and reports 27 isolated executions. Its next step is phase timing and an
isolation-boundary audit; merging those executions before proving that their
security and failure isolation is redundant would be unsafe.

## Prioritized owner migration

| Priority | Owner or group | Current signal | Next bounded change |
| ---: | --- | --- | --- |
| 1 | Compiler front door and generic nominal products | 329 front-door cases now take 3.35 minutes Windows and 2.87 minutes Linux; the 108-case three-owner bundle takes 4.87 minutes on a Windows miss and 8.02 seconds warm | Measure the bundle on both CI hosts, then decide whether qualification can adopt the same product while leaving the over-limit carrier separate. |
| 2 | `wvb-runner-reconstruction` | 28.49 minutes Windows and 43.20 minutes Linux for three cases | Separate immutable construction products from the three reconstruction claims and find the Linux-specific critical phase. |
| 3 | `language-1-memory-budget-split-execution` | 41.13 minutes Windows and 42.83 minutes Linux for 172 cases | Inventory product acquisition versus selector execution and share only immutable packages across the retained behavior cases. |
| 4 | `database-storage` | 41.00 minutes Windows, 20.94 minutes Linux, 57 cases, 86 helper references across 66 visible projects | Finish hosted construction ownership and execute the inventory as a shared dependency graph; keep fresh state for recovery and mutation. |
| 5 | `language-1-authenticated-foreign-binding` | 28.01 minutes Windows and 24.56 minutes Linux, with 27 isolated processes | Record build/package/execute phase time and peak memory; preserve process isolation, then use only measured capacity-safe concurrency or shared immutable packages. |

This order is based on expected critical-path contribution and visible work
shape, not on test count alone. A high case count can be cheap when cases share
one product, while one compiler-scale construction can dominate many small
executions.

## Current bottleneck

The database owner shows the scaling failure clearly. Its 54 current execution
steps reference 58 Project 2 inputs across 57 unique manifests. Those uses
contain 673 root/source entries but only 146 unique source paths, a 4.61-fold
declaration overlap. The most common Foundation and durable-storage sources
appear in 39 or 40 project uses. Qualification then compiles and lowers every
ordinary project twice, so a common source can still pass through the compiler
about 80 times per host. The 54 steps represent 57 database cases and three
explicit portable runtime prerequisites; six products each exercise two logical
cases.

Each case currently mixes several independent claims:

- source compilation and deterministic recompilation;
- WVB-to-WVO lowering and deterministic relowering;
- structural WVO admission;
- native linking;
- Windows and Linux packaging; and
- the database behavior that the case actually exists to test.

The database behavior is often the cheapest part. Repeating the complete
toolchain pipeline makes elapsed time grow approximately with `cases × pipeline`
instead of `unique construction + behaviors`. The same pattern will become
more expensive as Language 1.0, Libraries 1.0, WVDB, packages, and OS coverage
grow.

## Target evidence model

Replace case-owned pipelines with one declared evidence graph. A graph node is
an immutable operation identified by all of its inputs, tool identities,
options, target, profile, and node-format version. The initial node kinds are:

| Node kind | Owns |
| --- | --- |
| Construction | Source set to WVB, WVB to WVO, assembly, link, or package |
| Admission | One structural or semantic validation of one exact digest |
| Behavior | One fresh execution, rejection, mutation, recovery, or lifecycle claim |
| Platform | Behavior that must execute on Windows, Linux, or both |
| Reproducibility | Independent construction A/B and exact output comparison |
| Coverage | Proof that every required claim resolves to an executed evidence node |

The runner materializes a construction or admission node once per qualification
graph and fans its immutable output out to every dependent behavior. When exact
reproducibility is required, it constructs graph A and graph B in separate clean
temporary roots and compares the declared outputs. It does not reconstruct the
same graph separately for every behavioral case.

Development may restore content-addressed products after validating their
complete keys and records. Qualification initially remains independent of
cross-run development caches; sharing is limited to immutable nodes created
inside that qualification run. Reusing signed qualification evidence across
runs is a later decision and is not required to obtain the first large speedup.

Development result reuse now has a separate compatible-state proof. After an
exact receipt miss, it compares a retained passing Git tree with the current
tree and asks the current changed-file planner whether any changed path owns the
selected owner. Reuse is allowed only on the same repository and host identity,
with no planner gap and no selected-owner dependency; global planner, registry,
coordinator, dispatcher, stream, and cache changes remain exact-state-only.
The current source sentinel must still match before the result is promoted into
the new state. Qualification remains fresh and does not consume these receipts.

## Ownership corrections

Domain owners must test domain behavior. Generic toolchain guarantees belong to
their focused owners:

- compiler determinism owns source-to-WVB reproducibility;
- lowerer and object owners own WVB-to-WVO determinism and admission;
- linker owners own relocation and image determinism;
- packager owners own PE/ELF construction and cross-target packaging; and
- database owners own database encoding, mutation, recovery, and hosted
  lifecycle behavior.

A database qualification still builds its exact database test products and
executes every required database behavior. It does not need every database case
to independently re-prove the complete generic packager contract. Any removed
overlap must first be mapped to an existing focused owner or a new evidence node;
no assertion disappears merely because it is slow.

Windows executes Windows-host behavior and Linux executes Linux-host behavior.
Portable WVB and WVO identities are compared across hosts where portability is
the claim. Producing both target packages on both hosts remains only where that
cross-construction property is itself the owned contract.

## Delivery plan

### Phase 1: expose the work graph

Status: complete for the database subgraph and in progress for all 126 native
qualification owners on 2026-09-04.

Add a machine-readable qualification-case inventory and a read-only planner
that reports cases, unique source closures, construction nodes, admission
nodes, behavior nodes, duplicated work, and estimated critical path. Add
structured start/end timing for every node and retain the active node when a
timeout occurs.

The current planner reports exact inventory rows, counts, duplicate source use,
and identical dependency-closure candidates. Construction-node identity,
estimated critical path, and timeout-state retention move forward with the
bounded graph runner rather than being guessed from shell call sites.

Exit condition: every qualification area explains its predicted cost and unique
failure signals without running an hours-long gate, and paired host wrappers
consume shared inventories rather than duplicated enumeration.

### Phase 2: remove unrelated repeated evidence

Status: in progress. Portable database steps delegate unused opposite-host
packaging and private A/B compiler/lowerer repetition. Storage-lowering retains
its paired evidence. Hosted ownership and explicit aggregate fail-closed
composition remain.

Assign every determinism, admission, linking, packaging, cross-host, and
database assertion to one explicit owner. Stop cross-packaging every portable
database case when the package bytes are not part of that case's contract.

Exit condition: coverage validation proves that the 57 database cases and all
previously owned generic claims remain represented, while a database case no
longer launches unrelated target packaging.

### Phase 3: build once inside qualification

Introduce a bounded graph runner with two clean construction roots. It hashes
and reads common inputs once, runs independent A/B construction only for nodes
that own reproducibility, admits each exact digest once, and shares immutable
outputs with dependent executions. Limit workers by declared CPU and memory
cost and collate diagnostics deterministically.

Exit condition: the cold database qualification performs work proportional to
unique graph nodes rather than shell call sites, and its sequential reference
mode produces the same outputs and behavior results.

### Phase 4: test many behaviors per product

Group compatible portable fixtures into a small number of database test
applications and expose each case as a named callable test. Group only cases
with compatible profile, authority, resource limits, and failure isolation.
Hosted recovery cases may share immutable application bytes but must retain a
fresh private state directory and process where crash or restart behavior is
the contract.

Exit condition: adding a pure behavioral case normally adds a function and a
manifest row, not a new compiler/lowerer/linker/packager pipeline.

Bundling must remain capacity-aware. A trial that combined the three
branch-page cases exceeded the native lowerer's declared output limit and was
discarded without increasing that limit. Compatible cases therefore need a
bounded segmented product when one ordinary product would cross a compiler,
lowerer, execution, or diagnostic resource limit.

### Phase 5: make compiler construction incremental

Move the batch path into the Windvale-native build driver and split compiler.
Keep the compiler process alive for a bounded request batch, reuse source bytes
by digest, and reuse immutable symbol, analysis, WIR, and emission checkpoints
only when their complete dependency keys match. Reject undeclared dependencies
and preserve the simple clean compiler as the correctness oracle.

Exit condition: changing one leaf module does not reanalyze unaffected modules,
and clean versus incremental output bytes compare exactly.

### Phase 6: qualify and enforce the budgets

Run the unchanged 57-case database contract through the new path on Windows and
Linux, compare it with the sequential oracle, then run the deliberately selected
paired-host repository qualification. Record elapsed time, critical path,
process count, bytes read, peak working set, graph-node counts, and exact source
state.

After at least three stable observations per host, replace advisory targets with
enforced regression bounds. A new case or owner must declare its unique failure
signal, evidence dependencies, expected incremental cost, and resource class.

## Growth rules

The redesigned verifier follows these rules:

1. A behavior case requests artifacts from the graph; it does not privately
   rebuild the toolchain.
2. A new case joins an existing owner and product when its profile and isolation
   requirements match.
3. A new top-level owner requires a distinct contract, host boundary, authority
   profile, or failure domain.
4. Every reusable result declares its complete input closure and producer
   identities. A missing dependency invalidates reuse.
5. Mutable behavior always reruns. Only immutable construction and admission
   evidence may be shared.
6. Qualification coverage is checked from declared claims, not inferred from a
   successful process exit or a case count.
7. The planner reports added critical-path time in review so test growth cannot
   silently turn seconds back into hours.
8. A test without a unique failure signal is merged into its causal owner or
   removed. Case count alone never justifies retaining a separate pipeline.
9. A merged test keeps distinct case names and diagnostics even when it shares
   construction, so qualification coverage remains auditable.

## Honest qualification boundary

Rechecking the envelope of an unchanged, already-qualified source state can
take seconds because it verifies identities and retained evidence. Freshly
qualifying changed compiler source cannot honestly be reduced to only that
check: independent construction, affected execution, and paired-host evidence
must still occur. The achievable near-term target for new qualification is a
few minutes by eliminating duplicate work, not by relabeling cached development
results as qualification.

## Measurements

On the measured Windows host, the separate publication and recovery products
used 71,830 ms for clean compile, lower, link, package, and execution, while one
combined product used 40,608 ms, a 43.47 percent reduction. After portable
opposite-host packaging was delegated, that combined focused qualification step
reported 46,150 ms including its wrapper and verification overhead. Delegating
its private second compiler and lowerer constructions then reduced the same
checkpoint to 38,310 ms, another 16.99 percent. The transaction root-growth pair
also passed as one product before those delegations, in 75,010 ms. These are
diagnostic measurements, not qualification thresholds. The same
publication/recovery node reran in 39,860 ms after explicit prerequisite
expansion was added and selected zero support steps. Its development selector
now coalesces the same two logical cases into one execution: the first
checkpoint creation took 44,850 ms and the unchanged warm path took 2,130 ms.
Independent warm `publication` and `recovery` selections took 2,100 and 1,980
ms respectively, proving that a partial selection does not consume the bundle.
The non-identical transaction leaf-groups/pages trial passed its focused cold
qualification node in 56,140 ms, then created development checkpoints in 59,540
ms and completed the unchanged warm development path in 2,990 ms. The root-
split/depth-two bundle passed its qualification node in 59,990 ms, created
development checkpoints in 58,470 ms, and completed unchanged in 2,860 ms.

The qualified paired-host owner receipts contain 17,904,835 ms of Windows work
and 16,467,401 ms of Linux work. The measured six-owner shard assignment projects
that historical work onto a 4,655,707 ms critical path, versus 6,547,869 ms under
the immediately preceding mapping. Execution of the new shard assignment and
complete paired-host measurement of the database graph remain pending.
