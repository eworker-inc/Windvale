# Verification throughput history through September 26, 2026

> Status: Historical implementation and measurement notes
> Authority: Informative; linked evidence owns exact claims
> Last reviewed: 2026-09-26

This snapshot preserves the earlier chronological checkpoint notes. Statements
about pending work describe those checkpoints, not current standing. Use the
[current throughput plan](Verification-Throughput-Plan.md) for active work.

## Historical checkpoints

The current workflow trial separates compiler preparation from the Foundation
integration loop. During local implementation, use the existing owner with:

```powershell
node Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.mjs --vector-borrow-integration --maximum-seconds 600 --prepared-compiler-only
```

This mode requires an exact, validated current compiler checkpoint. A missing
checkpoint stops before any build command, with instructions to prepare the
compiler explicitly or use the existing supplied-product selections. It never
starts bootstrap reconstruction as hidden test setup. Missing target products
may still need compilation and packaging within the same selected deadline.
The existing construction-enabled invocation remains available to automation
and deliberately selected cold runs; its progress now states whether compiler
preparation is allowed. Neither mode omits integration assertions.

For an individual runtime or compiler diagnostic, prefer the existing focused
selection with explicitly identified products. Rebuild only products whose
declared inputs changed. Passing a check with an older product does not verify
new source changes. Run the combined integration selection at a coherent feature
boundary and independent qualification at the existing promotion/release gates.

Before native packaging, the existing `--inspect-function-limits <module.wvb>`
diagnostic now checks every function and separately identifies the largest code
body and largest local-slot requirement. Previously it reported only the
largest code body as valid, allowing an oversized different function to reach
packaging before rejection. This is an early diagnostic, not complete WVB
verification or proof that native lowering supports every operation.

Cold source analysis and emission remain the dominant measured cost of the
active runner edit. This trial does not establish a faster cold compiler or
completion of the collection/budget feature. Its measurements and exact scope
are in the [workflow evidence](../Evidence/2026-09-26-Prepared-Compiler-Feedback.json).

The follow-up stabilization repairs fresh-checkout cache tests by creating their
ignored work directory before resolving it. Product-acquisition-only edits now
select the existing cache owner; language test, compiler, runtime, and fixture
edits retain their execution coverage. The repair passes the Windows and Linux
GitHub development jobs and aggregate gate. Four cases in the existing cache
owner protect the function-limit diagnostic, including an oversized function
that is not the largest code body.

In the pending collection/budget work, extracting return ownership classification
reduces the runner's largest function from 2,057 to 2,028 total slots without
changing the 2,048 limit. Rebuilding changed runner source took 276 seconds;
Windows and Debian packaging took 120 and 127 seconds in parallel, for about
6 minutes 44 seconds to both executable products. This reused identified
compiler tools and does not measure cold compiler reconstruction. Focused
borrowing, ownership, runtime, and 40 record/helper cases pass on both hosts.
The record/helper rerun took 60 seconds on Windows and 129 seconds on Debian
after correcting a hand-built fixture's exact stack declaration. Unaffected
passing selections were retained. The refactor and fixture correction remain
with the pending feature batch; full integration and release qualification are
still separate gates. Exact inputs, results, and limits are in the
[stabilization evidence](../Evidence/2026-09-26-Development-Stabilization.json).

SHA-256 compression now expresses its fixed rotations directly, removing 576
variable-distance helper calls per block. Repeated benchmark medians fell from
2.192 to 2.107 seconds on Windows and 2.271 to 2.130 seconds on Debian. All 20
existing streaming owner cases pass on both hosts with identical bytecode. The
[rotation evidence](../Evidence/2026-09-06-Sha256-Constant-Rotations.json) records
the modest gain, 1.5% benchmark bytecode growth and unadopted two-round trial.
Packaged hashing tools remain unchanged. Investigate the existing native
SHA-256 intrinsic for regions contained in one buffer while preserving the
bounded streaming path for larger regions; that larger optimization is pending.

A bounded source-set read-reuse trial showed no useful performance gain and
was discarded. Reducing thirteen response reads to three preserved output
bytes on both hosts, but whole packaging changed by less than 2%; isolated
Windows execution was slightly slower with essentially unchanged memory use.
The [trial evidence](../Evidence/2026-09-06-Source-Set-Read-Reuse-Trial.json)
records the measurements and unchanged production tools. The source-set phase
still takes about 20 seconds on the 10.6 MB workload. Investigate native hashing
cost and sharing independently valid digest products before optimizing more
file reads; full cold compiler preparation remains unmeasured after the earlier
hashing-tool promotion.

Foundation borrow component verification now offers one 225-group development
product over the existing plan, typed-directory and owner-flow tests. Fresh
test-product construction and execution took 85.3 seconds on Windows and 92.1
seconds on Debian, compared with 96.9 and 103.6 seconds for three separate
products. Bootstrap tools were retained in both measurements. Warm runs execute
all assertions in 2.0 and 4.3 seconds including process startup; the representative Windows mixed-file
dispatcher passes in 3.3 seconds with result caching disabled. The
[component evidence](../Evidence/2026-09-06-Foundation-Borrow-Component-Bundle.json)
records exact inputs, identical cross-host WVB and remaining limits.

Mixed owner-flow fixture edits with borrow-plan or directory fixture edits now
select that product. Standalone diagnostics remain separate, and two small
components alone do not acquire the larger owner-flow bundle. Compiler
integration, verifier implementation and format changes retain conservative
routing. The no-argument full owner and publication selection remain unchanged;
this development bundle does not qualify candidate WVB 1.39 execution.

The owner coordinator now reads Linux executable modes with one Git index
request instead of one request per owner. A controlled Windows-hosted run of
the production Linux registry reader fell from 5,514 to 168 ms and returned
identical rows for all 126 owners. The existing routing guard now checks missing,
non-executable, linked-mode, conflicted, malformed and failed index responses.
The selected Windows changed-file plan passed in 59.3 seconds, including all
six stream-owner cases. This is not a Linux host timing or a whole-plan speedup:
Debian currently has no PowerShell executable. The
[registry evidence](../Evidence/2026-09-06-Verification-Registry-Index-Batching.json)
records that remaining measurement and the unchanged broad performance targets.

Canonical packaging now uses the rebuilt current-source hashing tools. In one
paired-host 10.6 MB image comparison, packaging fell from 67.8 to 52.9 seconds
on Windows and from 68.9 to 50.8 seconds on Debian, with identical output bytes.
Both hosts pass the existing five packaging checks and six direct CLI rejection
and output-preservation cases. The [promotion evidence](../Evidence/2026-09-06-Hosted-Hashing-Tool-Promotion.json)
records the exact identities, stale control-fixture correction and explicit
adoption of the current Profile 8 instruction allowance. Memory geometry is
unchanged. Full compiler-scale timing and narrower toolset-change routing remain
open; dependent caches must rebuild under the new producer identities.

Streaming SHA-256 now has a focused selection in the existing native SHA owner.
All 20 cases pass in 14.8 seconds on Windows and 23.9 seconds on Debian for
fresh test products using existing compiler/tool caches; warm runs take 3.2 and
5.0 seconds. The source incorporates the arithmetic change measured about 27%
faster in the [preceding trial](../Evidence/2026-09-06-Streaming-Sha256-Sum-Trial.json).
The [owner evidence](../Evidence/2026-09-06-Streaming-Sha256-Focused-Owner.json)
records identical bytecode, malformed-state coverage and timing limits.

Changes confined to the streaming hash sources and fixture select the existing
owner's `--streaming` mode, capped at 60 seconds including preparation. Native
backend and owner changes retain the full eight-group selection; its existing
KAT application also executes the streaming cases. No new verifier entry point
was added. The full owner now reuses the existing project and segmented package
products for its lowerer and publication stager. All eight groups pass warm in
17.4 seconds on Windows and 20.8 seconds on Debian, executing every assertion.
First application-cache misses took 4 minutes 49 seconds and 5 minutes 2 seconds;
that remains above the cold target. The [construction evidence](../Evidence/2026-09-06-Native-Sha256-Construction-Reuse.json)
records exact product identities and the limits of the comparison with earlier
270-second incomplete runs. Planning now budgets five minutes expected and ten
minutes maximum for the full owner, with all five projects visible in inventory.
The hashing-tool promotion above improves real packaging, but does not yet
establish a full cold compiler improvement. Complete repository qualification
remains open.

Current compiler preparation now overlaps two independent branches after
constructing the shared native image: Profile 7 Analyzer packaging and Emitter
construction. Both profiles and all twelve operations remain. The existing
cache sentinel now covers branch ordering, joined failures and publication on
Windows and Debian. All 27 foreign-binding cases passed in 8,643 ms on Windows
while republishing the pair from preserved lower-level products; compiler and
fixture bytes are unchanged. This is not a new cold timing. The
[construction evidence](../Evidence/2026-09-06-Current-Compiler-Construction-Overlap.json)
records the distinction and the pending cold time/memory measurement.

The routing guard now shares immutable inventory initialization across its
focused-selection checks and the full native-routing table. The final Windows
measurement passed in 27.8 seconds, versus about 38.4 seconds before, preserving
all 31 general and 284 native cases. An uncached plan remains the comparison oracle and mixed
selections still check for retained state from prior requests. This guard runs
for verification-infrastructure changes; it is not a universal source-edit cost.
The [routing evidence](../Evidence/2026-09-06-Routing-Guard-Shared-Initialization.json)
records the measurement limits. The planner performance targets remain open.

The complete 27-case foreign-binding owner now passes warm in 4,492 ms on
Windows, versus 7,324 ms for the prior coordinator with equally warm lower-level
caches. The current compiler pair is a separate validated product, so a hit
skips all twelve pinned/intermediate preparation steps and builds only the two
requested projects. Its 28-case cache sentinel passes on Windows and Debian.
The [compiler-pair evidence](../Evidence/2026-09-06-Current-Compiler-Pair-Reuse.json)
records unchanged WVB identities and the limits of this comparison. The approved
fully cold baseline still took 59.5 minutes; only 1,735 ms was behavior execution.
Cold compiler construction and full paired-host qualification remain open.

Three branch-page cases now share one segmented product. Fresh Windows
construction and execution passed in 52,110 ms; the separate-product baseline
was still incomplete at its 210-second bound after two passes. Development
executes all three cases in 3,353 ms warm on Windows and 8,108 ms warm on Debian.
Both hosts passed fresh development construction and produced identical WVB
bytes. The [branch-page evidence](../Evidence/2026-09-06-Segmented-Branch-Page-Bundle.json)
records those observations and the incomplete baseline. This seventh bundle
retains all 57 qualification cases in 52 steps and all 53 development cases in
45 executions. Full qualification and the broader timing targets remain open.

The split compiler now checks its finished WVB before acquiring intermediate
analysis. A valid final checkpoint survives independent analysis/symbol
eviction without restarting those producers. Invalid WVB bytes or analysis-key
records fail before construction. The existing cache sentinel covers this
dependency boundary, and cache/verifier edits now select the focused split
owner instead of runner reconstruction and production admission. The owner
passed in 5,107 ms on Windows and 11,155 ms on Debian; the complete Windows
changed-file plan passed in 57,312 ms. The
[final-product reuse evidence](../Evidence/2026-09-06-Split-Final-Product-Reuse.json)
records the reproduced rebuild and coverage limits. This removes an avoidable
rebuild path; the current-pair checkpoint above also removes earlier coordinator
setup on a hit. Cold qualification remains outside the target.

Hosted cache/session edits now select their focused owner instead of a
2,540-second database plan. The existing session test is part of that owner,
which passed twelve cases in 4,968 ms on Windows and 10,041 ms on Debian; the implementation changed-file
gate, including routing checks, passed in 53,824 ms. Producer contexts retain 3,560
bytes of fingerprints instead of about 24.4 MB of file contents. Four bounded
reads preserve every producer check and reduced observed Debian preparation
from 2,733 to 850 ms. The paired packaging workload changed from 31,628 to
23,962 ms on Debian and from 23,684 to 23,390 ms on Windows. The
[producer-context evidence](../Evidence/2026-09-06-Hosted-Producer-Fingerprints.json)
records the host measurements and memory limits. Cold compiler construction and
wider qualification remain unfinished.

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

Six compatible pairs and one segmented three-case bundle now share products,
reducing the inventory from 60 to 52 execution steps while preserving all 57
logical cases. The current graph has 56 project references across 55 unique
manifests and 644 root/source references across 147 unique source paths, a
4.38-fold declaration overlap. Pairing every construction would visit those
source references 1,288 times per host. The 40 portable construction steps now
consume one admitted construction, delegating 356
duplicate source visits to focused reproducibility owners. Portable steps also
package and execute only the current-host image; generic opposite-host packaging
is delegated to its focused owners. The branch-page bundle uses the existing
segmented image path because its native image exceeds the ordinary lowerer
limit. Hosted product sharing, build-once graph execution, further capacity-aware
bundling, and complete paired-host qualification remain pending.

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

The seven retained bundle projects and their root fixtures now route to their
exact two- or three-case development selectors. The version-3 development inventory binds
those cases back to the qualification bundle membership and distinguishes 53
logical development cases from 45 physical executions. A complete bundle now
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

## Earlier qualification baseline and migration review

These measurements and priorities belong to the earlier redesign review. The active six-item table supersedes that work order.

### Complete qualification baseline

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

### Prioritized owner migration

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

### Current bottleneck

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
