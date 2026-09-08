# Windvale 2.0 qualification and testing review

> Status: Proposed; code-backed findings and improvement candidates
> Authority: Informative; not an accepted qualification-model change
> Last reviewed: 2026-09-08

## Intended outcome and review boundary

Give developers relevant feedback in seconds or minutes, while preserving the
stronger evidence needed to release Windvale. My recommendation is to remove
repeated construction and overly broad selection before reducing test depth.
Finish and extend the existing throughput work rather than rewrite the test
system or create another coordinator.

This AI review inspected qualification orchestration, CI, duration registries,
development selectors, cache boundaries, and sampled expensive test owners at
revision `d1df6eb3`. It ran the read-only qualification-work planner, not tests,
benchmarks, or a qualification suite. Historical measurements below identify
useful priorities; they are not measurements of this checkout or a full audit.

The [verification-throughput plan](Verification-Throughput-Plan.md) remains the
active implementation owner, and the
[verification architecture](../Architecture/Seed-Verification-Throughput.md)
owns current boundaries. This document adds review findings to the
[2.0 release plan](Windvale-2.0-Release-Plan.md) and
[compiler/tooling review](Windvale-2.0-Compiler-And-Tools.md), not a competing
roadmap. Compatible selected improvements need not wait for 2.0. No test removal,
implementation, longer local run, or weakened release gate is authorized here.

The later [stack and application review](Windvale-2.0-Stack-And-Applications.md#bound-native-bootstrap-teardown-and-retain-useful-failed-evidence)
adds a concrete convergence-coordinator finding: bounded descendant teardown
and failure-artifact retention need attention without weakening the independent
bootstrap proof. Its other proposals name focused validation needs, not new
blanket test suites.

## What the evidence says

The [qualification-work planner](../../Tools/Verify/Plan-Qualification-Work.mjs)
reports 126 owners and 6,378 declared cases at review. Its historical timing
baseline covers 5,981 cases, with five owner case-count mismatches. Do not treat
the baseline as a current duration estimate without rechecking those workloads.
Case counts are registry units, not equal amounts of work or a quality score.

| Inspected evidence | What it supports | Important limit |
| --- | --- | --- |
| [Current compiler-pair reuse](../Evidence/2026-09-06-Current-Compiler-Pair-Reuse.json): one Windows cold foreign-binding run took 3,568,735 ms, while its 27 behavior cases summed to 1,735 ms. | Construction/setup dominated that workload; deleting assertions would miss its main cost. | One historical, non-isolated cold run; sampled process-tree working set reached about 905 MiB. Not a current or cross-host benchmark. |
| The same record compares equally warm lower caches: 7,324 ms before versus 4,492 ms with pair reuse. | Shared compiler products already improve warm feedback. | Cold versus warm is not the speedup of this change; genuinely cold construction did not become a seconds-long operation. |
| [Foundation borrow component bundle](../Evidence/2026-09-06-Foundation-Borrow-Component-Bundle.json): Windows fresh-product construction fell from 96,949 to 85,295 ms for the recorded selection. | Combining compatible test products can reduce construction without deleting their cases. | Bootstrap products were retained; this is not an empty-machine measurement or a result for the whole current owner. |
| [Current-host database packaging](../Evidence/2026-09-06-Hosted-Database-Current-Host-Packaging.json): six sites stopped building an opposite-host package they did not execute. | Packaging ownership can remove work without dropping either host's behavior contract. | Generic cross-target construction has separate owners. Timing samples had different overlap conditions, so they are not a controlled benchmark. |

The older [qualification timing baseline](../../Tests/Native/Qualification-Owner-Timing-Baseline.txt)
also identifies front-door compilation, runner reconstruction, memory-budget
execution, database storage, and foreign binding as expensive owners. Use these
as profiling candidates, not a claim that their costs remain unchanged.

## What already exists, and what still needs work

The repository already has changed-file planning, development selectors,
content-keyed product caches, validated development-result reuse, four
qualification shards per host, and explicit partial-resume reporting. Database
qualification already overlaps its portable and hosted branches. These are
assets to extend, not missing features to reinvent.

The remaining opportunity is to connect those mechanisms through explicit
dependencies and evidence claims. An evidence claim says what a run proves:
for example, source-built behavior, rejection of a malformed module, or exact
reconstruction. Sharing the same executable does not make those claims equal.

## 1. Measure phases and expose cold costs before execution

Observation: the [coordinator](../../Tools/Verify/Invoke-WindvaleTests.ps1)
records owner elapsed time, while some leaf owners already report finer build,
package, and execution timings. The
[duration profiles](../../Tests/Native/Verification-Duration-Profiles.txt)
are coarse expected/maximum categories. Static planner call-site counts do not
describe the complete executed dependency chain: runner reconstruction has no
recognized literal pipeline calls in its scanned wrapper, yet invokes an
expensive constructor.

Proposal: extend existing reporting with bounded events for input hashing,
compiler construction, fixture compilation, lowering/linking, packaging,
admission, behavior, and cleanup. Record cache state, input size, host/tool
identity, elapsed time, and sampled process-tree memory where practical. Explain
each miss and the missing product that makes a selected command expensive.

Separate fully cold construction, fixture-cold with retained bootstrap, warm
products with fresh behavior, and reused development results. Planning should
inspect availability without warming caches or launching constructors. Unknown
or stale costs must be visible; do not quietly start a cold hour-long owner
under a ten-minute development command.

Evidence before adoption: compare instrumentation overhead, bound event bytes
and retained history, test nested work attribution, and refresh named workload
measurements before setting regression thresholds. A shorter timeout is a
resource guard, not a performance optimization.

## 2. Build shared immutable products once per required construction

Observation: compiler-pair caching and database/component bundles already
remove repeated preparation. The throughput plan proposes extending shared
construction to qualification; this is not yet a reason to assume every owner
participates in a complete shared graph.

Proposal: declare product-producing steps separately from their consumers.
Within a qualification construction root, build each equivalent compiler,
fixture WVB, native object, or host package once and give consumers admitted,
immutable products. Equality requires the exact source/dependency closure,
producer and validator identities, target, options, and format version; a common
project filename is not enough.

Keep explicitly independent reconstruction roots separate. A check comparing
two independent builds must not obtain both sides from the same cached result.
Qualification must not inherit a development pass receipt as fresh evidence;
cross-run qualification reuse needs its own accepted trust and freshness rules.
Keep required validation when artifacts cross trust boundaries.

Evidence before adoption: prove shared and unshared products/behavior agree,
test invalidation and concurrent publication, and deliberately corrupt one
construction root to prove the other does not share its result. Extend existing
cache and owner checks rather than add a parallel cache framework.

## 3. Select behaviors, not whole historical scripts

Observation: the [front-door development owner](../../Tools/Native/Test-Language-1.0-Front-Door-Development.mjs)
already selects combinations of six products and checks the bounded subset
space. Foundation borrow and database owners also have focused selections.
Their existence argues for extending precise routing rather than running the
complete historical owner for every related edit.

Proposal: map changed contracts to products and logical behavior groups through
validated declarations. Each plan should show the reason for selection, the
current-source product it exercises, required hosts, and excluded qualification
claims. Share unchanged prerequisite checks within the plan where exact
identity permits; do not simply delete the front-door frozen-input checks.

A pinned-binary smoke check is not evidence that modified source works. The
[runner reconstruction wrapper](../../Tools/Native/Test-Wvb-Runner-Reconstruction.sh)
returns early in development mode after candidate inventory and current-host
behavior; qualification constructs and compares the source-built products.
If runner source changes, select a current-source behavior path or explicitly
report that the relevant reconstruction remains unverified.

Evidence before adoption: mutate direct and transitive dependencies and compare
old/new selections. Require missing ownership to be reported explicitly, retain
complete-qualification coverage, and ensure excluded cases are not counted as
executed. Routing should never convert a requested full gate into a sample.

## 4. Bundle compatible fixtures, retain process and failure isolation

Observation: existing component bundles show a useful tradeoff: fewer compiled
products can preserve many logical cases. The throughput plan also records a
rejected larger bundle that exceeded implemented limits. One giant test binary
is not the general solution.

Proposal: group fixtures only where source closure, compiler/profile, and
runtime requirements align. Retain named case selection and precise failure
reporting. Keep bundles bounded by code size, compilation memory, and execution
time; split at real capability or dependency boundaries.

Reuse immutable packages while starting fresh processes and mutable fixtures
for crash, revocation, host-state, corruption, and recovery tests. Do not batch
those cases into shared mutable state merely to save process startup. Preserve
all crash points, malformed-input classes, resource boundaries, and host adapter
behaviors. Count logical claims separately from builds and process launches.

Evidence before adoption: compare case identities and outcomes, run each group
alone and after its neighbors, seed failures to check attribution, and measure
product size and peak memory. Reject a faster bundle that hides state leakage
or requires raising safety limits without a separately justified contract.

## 5. Replace duplicated routing facts with a validated claim manifest

Observation: registries, path routing, script summaries, product selectors, and
static pipeline scanning describe overlapping parts of the test system. The
planner cannot infer all transitive actions from textual helper names.

Proposal: extend the existing manifests to own stable case/group identities,
dependencies, product inputs, execution mode, host requirements, and claims.
Let the existing coordinator consume those declarations; make shell wrappers
thin host adapters where their behavior really matches. Keep a small explicit
rule layer for exceptions instead of building another general scripting system.

Remove an entry point only after its unique failure signals are assigned to a
retained owner. Remove redundant packaging or reconstruction only after proving
action equivalence and preserving independent-construction requirements. Keep
generic package-format tests in their owners and domain behavior in its owner.

Evidence before adoption: independently validate coverage, duplicate and missing
identities, malformed/oversized manifests, and changed-dependency selection.
Generate mechanical inventories where useful, not both semantic implementation
and correctness oracle from the same algorithm.

## 6. Checkpoint qualification evidence, not just a shard position

Observation: the coordinator accumulates owner results and writes its result
file at the end. `StartAtOwner` selects a shard suffix. The
[CI workflow](../../.github/workflows/verify.yml) correctly labels resumed/subset
runs as partial qualification rather than a complete pass. This is useful
recovery, but it is not yet arbitrary evidence-graph resumption.

Proposal: publish bounded, atomic completion records for finished owners and,
where worth the complexity, expensive product/behavior nodes. Bind records to
the exact run lineage, source and dependency identity, host/tool identity,
options, and claim. Resume only missing or invalidated nodes while preserving
completed independent constructions and immutable admitted outputs.

A killed, timed-out, or partly written node is incomplete. Missing, duplicated,
stale, or incompatible receipts must not produce a full pass. Keep a coverage
aggregator separate from execution success, and retain the current partial gate
until a new qualification model is explicitly accepted. Preserve distinct
test-failed, timed-out, and framework-error outcomes; do not retry real test
failures until they happen to pass.

Evidence before adoption: interrupt at publication boundaries, modify a
dependency between attempts, mix incompatible records, and omit a required host
or case. Complete qualification must fail closed in each invalid combination.

## 7. Schedule by measured cost and resource limits

Observation: CI already runs four shards per host; the coordinator executes
owners sequentially inside a shard. The
[database qualification runner](../../Tools/Native/Run-Database-Storage-Qualification.mjs)
already runs two branches concurrently, and other helpers overlap construction
or packaging. Additional outer parallelism can compete with nested work.

Proposal: schedule independent declared steps with shared CPU, memory, and I/O
budgets. Account for child processes rather than counting only top-level jobs.
Use host-specific, cache-state-aware timing evidence to balance shards and
reduce the longest dependency chain. Preserve deterministic output and canonical
case reporting regardless of completion order.

Evidence before adoption: measure wall time, total work, peak tree memory, and
host contention. Verify cancellation terminates descendants and preserves
completed evidence. Do not raise parallelism or shard count as an assumed fix;
first remove redundant work and measure the remaining bottleneck.

## Recommended order and what not to cut

1. Extend phase/cold-cost visibility and refresh the highest-cost named workload
   evidence with explicitly approved budgets. Keep historical results intact.
2. Finish shared-product and focused-selection work in existing owners. Pilot
   one costly path such as foreign binding or foundation borrowing, compare
   like-for-like cache states, and retain every selected behavior.
3. Generalize proven dependencies into validated manifests and qualification
   construction nodes; preserve independent reconstruction and both-host claims.
4. Add durable qualification resumption, then rebalance bounded scheduling using
   the resulting measurements. Avoid an all-at-once framework rewrite.

Development selection, complete qualification, and named long-running workloads
are different evidence scopes, not a ladder to run consecutively after every
edit. Keep the existing ten-minute local budget and throughput-plan targets;
this review does not establish new measured thresholds or promise seconds-long
cold qualification.

Retain independent reconstruction, deterministic byte comparisons, negative and
malicious-input tests, Windows/Linux host behavior, process cleanup, crash and
recovery points, and required stress duration. Long soak or generated-program
work can use reproducible bounded chunks when the contract permits, but a short
smoke run cannot claim the unexecuted duration or coverage. The first things to
remove are redundant preparation, unrelated selection, duplicate wrappers with
no unique claim, and replay of still-valid work within an authorized evidence
scope. The tests that establish different safety claims are not redundant.
