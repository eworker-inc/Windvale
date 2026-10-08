# Verification throughput redesign plan

> Status: Active implementation plan
> Authority: Informative
> Last reviewed: 2026-10-08

## Goal

The active six-item simplification goal below supports the
[compiler, tools and libraries milestones](Compiler-Tools-And-Libraries-Completion-Plan.md).
Implement measured improvements in coherent batches, using focused diagnostics
and one causal final verification plan per batch. Reuse valid construction and
unaffected evidence. The longer-term performance targets are not prerequisites
for resuming product implementation.

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

Changing the native compiler still exceeds the ordinary development target.
The broad prepared lowering owner retains an 80-minute planning estimate;
compiler reconstruction is a separate cost. Its complete duration has not been
remeasured. The
[compiler-only selection](../Runbooks/Native-Tests.md#separate-current-compiler-preparation)
retains generated-code and consumer checks while excluding unchanged
assembly-only runtime fixtures. Keep the default full selection's runtime
coverage and measure each selected path before reducing its estimate.

The candidate compiler now reads an adjacent scalar record field without
temporarily retaining and releasing all of that record's shared fields.
It preserves scalar copies, instruction charges, ordinary transfers and
block boundaries. The existing data-limit fixture covers recognition and
refusal boundaries; the existing source cases cover integer widths, Boolean
and enum values, owning and borrowed records, aliases, refusal and cleanup.
These checks pass on Windows and real Debian. The
[lowering specification](../../Specifications/Windvale-Native-X64-Lowering.md)
owns the exact supported shapes.

Both compiler hosts have been reconstructed and admitted through the accepted
[temporary projection](../Decisions/0981-Bootstrap-The-Current-Native-Compiler-With-A-Temporary-Serializer-Projection.md).
Their 50 compiler object chunks match byte for byte, as do the ten source-case
inputs and native objects. The compiler object is 54,811,652 bytes, about 4.7%
smaller than before this scalar-read change. This is an object-size result;
it does not establish a whole-compiler speedup or a memory reduction.

The 8 October development measurements separate construction from behavior:

| Work | Windows | Real Debian |
| --- | --- | --- |
| Build and admit compiler/staging WVB | 4 minutes 27 seconds | 5 minutes 54 seconds |
| Existing data-limit self-test | 1 minute 26 seconds | 2 minutes 20 seconds |
| Reconstruct and admit compiler host | 14 minutes 59 seconds | 37 minutes 1 second |
| Prepare ten native source objects | 1 minute 49 seconds | 7 minutes 59 seconds |
| Execute prepared source behavior | 1 minute 7 seconds | 7 minutes 20 seconds |
| Construct and execute Plan workloads | 9 minutes 21 seconds | 16 minutes 3 seconds |

Prepared compiler, frontend and assembly products were reused where valid.
The source and Plan behavior runs overlapped on Debian. These are individual
development measurements, including their required identity checks, not stable
thresholds, complete cold costs or measurements of the broad lowering owner.
Both measurements used a Windows Hyper-V guest; local Debian ran in WSL2
inside that guest. They are not bare-metal timings.
The staging consumer uses 4,178,300 bytes of intermediate output, leaving 16,004
bytes under the unchanged 4 MiB profile. Native self-compilation, independent
reconstruction without the projection, promotion and installed delivery remain
separate obligations. The changed Windows compiler again reached its
fifteen-minute self-compilation limit without producing output; that result is
incomplete and requires diagnosis before another attempt.

The Plan consumer packages its 0-, 1- and 1,000-iteration workloads once and
runs them in one process. Every workload initializes and closes its own memory
domain. Eleven staging assertions, reservation refusals, cleanup checks and
the equal high-water requirement remain. The current workloads took 5.0 seconds
to execute on Windows and 15.4 seconds on Debian. Both hosts measured 608 bytes
of physical high-water at both 1 and 1,000 iterations and zero final physical
and budget charges. Construction, packaging and admission remain the main
costs of this focused check.

Shared-source preparation retains its ten checked native objects under exact
input, compiler, host and validation identities. Prepared behavior repeats WVB
admission and object validation while reusing all ten objects; a missing or
corrupt product refuses before compilation. Both hosts pass sixteen cache
lifecycle cases, 21 provider cases and four publication cases. Publication
retains five separate compiler executions. Complete native-host construction
provenance is checked at entry and close, with exact execution and configuration
checks inside the batch. Cache-only edits have a causal source-only route with
eight minutes expected and ten minutes maximum for prepared behavior; mixed
compiler, runtime and consumer changes retain their affected coverage.

The earlier
[budget-validation optimization](https://github.com/eworker-inc/Windvale/commit/c184f80f4334397edd88bc6826336290e607a8ed)
retains complete structure, ancestor, overflow and physical-binding checks with
592 bytes of bounded stack scratch. It improved the two preserved compiler
workloads before this scalar-read change. Current Windows timings of 58.2 and
50.0 seconds are roughly unchanged from 57.6 and 53.2 seconds; both ordinary
ABI-22 objects remain byte-identical and pass the existing object checker on
both hosts. Debian measured 104.7 and 79.8 seconds during overlapping Plan
packaging. An isolated repeat passed in 61.0 and 50.6 seconds, with identical
objects and successful admission and closure. The overlapping timings do not
establish a compiler regression. These fixed-input probes do not complete
self-compilation or establish a stable regression bound.

A separate Debian 12 guest running directly under Hyper-V reports the same
Ryzen processor and twelve virtual CPUs. Two repetitions per input on its
native ext4 test disk produced identical objects and passed the retained object
checker:

| Preserved compiler input | Local WSL2, isolated | Direct Debian Hyper-V guest |
| --- | ---: | ---: |
| Staging relocations | 61.0 seconds | 55.1 and 59.3 seconds |
| Staging symbols | 50.6 seconds | 48.3 and 46.9 seconds |

Mean native execution time was about 6% lower on the direct Debian guest.
Observed child CPU time was close to elapsed time there, so substantial
compiler computation remains. Existing QA services stayed running; physical
host load was not controlled. This small comparison does not measure complete
builds, qualification or a stable performance threshold.

The preceding
[focused GitHub run](https://github.com/eworker-inc/Windvale/actions/runs/37798569299)
passed: its native-owner step took 9 minutes 22 seconds, plus a separate
17-minute 40-second preparation job. This does not measure the broad compiler
owner. The
[current compiler run](https://github.com/eworker-inc/Windvale/actions/runs/37813201657)
passed preparation, shared-source and Plan behavior, then failed the broader
`implicit-vector-single` helper lowering with `Outputˉlimit`. Diagnose that
specific failure before retrying; the broader gate is not passing.

Warm access to the same 54.9 MB compiler image took 0.739-0.785 seconds through
the local Windows-mounted path, versus 0.005-0.012 seconds on the direct Debian
guest's native ext4 test disk. One hundred metadata pairs took 0.792 seconds
versus less than one millisecond. This measures warm filesystem access, not
physical SSD throughput or the complete verification owner. Use the QA guest's
native Linux storage for Debian build and verification preparation, retain
Windows work here and keep local WSL2 for narrow diagnostics. Existing caches
and QA services remain intact. Current-workload profiling and reuse of unchanged
compiler components remain necessary; moving execution does not close the
self-compilation gate.

The development build path now separates compiler preparation from project
compilation. Ordinary split-project and Project 4 builds reject a missing
compiler checkpoint instead of reconstructing the toolchain. An explicit
checkpoint selection lets application/library work keep using a validated
compiler while compiler source evolves; it does not verify those newer sources.
Existing construction owners opt in with an explicit deadline.

A Windows source-edit measurement using a retained compiler and existing WVB
runner took 2.8 seconds initially and 2.6 seconds after changing one function.
A Foundation Option/Result workload took 3.2 and 3.1 seconds. Every build missed
both analysis and WVB caches, and execution observed the expected 42-to-43
change. This avoids compiler reconstruction and native application packaging;
it does not accelerate either cold operation. The cache/lifecycle owner passes
147 Windows and 150 Debian cases, alongside its 24 analysis-cache cases per
host. Both Foundation outputs also execute on Debian. See the
[compiler-selection evidence](../Evidence/2026-09-26-Explicit-Compiler-Selection.json)
and [development commands](../Runbooks/Native-Tests.md#separate-current-compiler-preparation).

The accepted next design checkpoints are to separate publisher preparation,
rebuild only changed compiler components where their interfaces permit it,
and measure a representative larger edit through a reused execution host.
They are not implemented by checkpoint selection. Module-level incremental
compilation and broader qualification remain separate work.

Compact frame initialization is implemented in the current native backend.
A source-built analyzer processes the same 1.7 MB admitted source set in
60.4 seconds instead of 100.3 seconds on Windows, and 63.7 instead of 106.3
seconds on Debian: about 40% less analysis time, with all four outputs
byte-identical. The existing lowering owner passes 53 cases on each host,
including frame reentry, arguments, hidden returns and fuel/depth failures.
Prepared execution takes 21.5/40.9 seconds; tool construction still takes
minutes. The specification now routes to that current-source owner instead
of unrelated frozen-product and database checks. See the
[source-built measurements](../Evidence/2026-09-26-Compact-Native-Frame-Initialization.json)
and the earlier [diagnosis](../Evidence/2026-09-26-Compiler-Preparation-Diagnosis.json).

Normal staging now uses the refreshed compact backend on both hosts. Its
Profile-8 executables reproduce their own native output exactly and pass the
existing toolset owner's six focused reconstruction checks in 134.6 seconds
on Windows and 142.9 seconds on Debian. Source compilation independently
reproduces the retained staging WVB on both hosts.

Authenticated Project 4 compilation now retains analysis across later emission
changes or failures. Admission and authentication still execute before reuse.
For the 1.4 MB staging-tool source set, cold compilation took 86.9/96.0 seconds
on Windows/Debian; repeating with retained analysis took 47.5/54.8 seconds and
produced identical WVB. The new cache passes 24 cases per host, the existing
cache suite passes 130/133 cases, and the admission failure checks pass on both
hosts. See the [staging and analysis-reuse evidence](../Evidence/2026-09-26-Staging-And-Analysis-Reuse.json).

These are focused development results, not complete cold-preparation or release
qualification claims. Existing caches are preserved and invalidate when their
declared producers change. The original Debian preparation stopped after
59 minutes 30 seconds; the whole preparation graph has not been rerun for this
batch. Native packaging and broader exact-product reconstruction remain the
next checkpoints. Do not substitute longer timeouts for reducing their work.

Focused runtime execution takes seconds with prepared tools, while changed tool
construction still takes minutes. The latest runner rebuild took 276 seconds,
followed by Windows and Debian packaging in parallel at 120 and 127 seconds:
about 6 minutes 44 seconds in total. Focused runtime, ownership, borrowing and
record/helper selections pass on both hosts. These are development measurements
using retained compiler products, not independent reconstruction or release
qualification. See the [stabilization evidence](../Evidence/2026-09-26-Development-Stabilization.json).

The helper implementation and runner refactor are committed. The
[consumer completion plan](Compiler-Tools-And-Libraries-Completion-Plan.md)
owns current Package-Lock standing; this page does not maintain another copy.
One subsequent CI run timed out during development preparation. The next run
restored a compiler checkpoint and completed its job, but admission and callable
owners were classified as incomplete because their cold profiles exceeded the
development budget. A green infrastructure job is not evidence that those
owners passed. Reliable automatic preparation and complete affected execution
remain open.

The Project-WVB cache now has its own specification and focused lifecycle
owner mapping. Its cache-only edits no longer inherit the database behavior
plan; the shared cache-root and database-consumer contract retains that routing.

Prepared-product mode now stops compiler, split-WVB and segmented hosted-product
misses before construction. Both hosts pass the focused cache checks. A real
binder application took 161/163 seconds to package on Windows/Debian, then
0.29/2.69 seconds to validate and reuse. A later profile miss preserved that
completed product. The [cache enforcement evidence](../Evidence/2026-09-26-Prepared-Product-Cache-Enforcement.json)
records the bounds and failed preliminary probes. CI now has separate preparation
and prepared-execution phases for the authenticated foreign-binding owner, with
its 27 behavior cases retained. Local phase rejection and process-cleanup probes
pass on Windows and Debian; complete CI execution is still awaiting measurement.
Other owners and the older console-packager reconstruction expectations after
its Project 4 migration remain part of the open CI work.

Use prepared-only mode during ordinary Foundation implementation:

```powershell
node Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.mjs --vector-borrow-integration --maximum-seconds 600 --prepared-compiler-only
```

A missing exact compiler checkpoint stops with preparation instructions.
Explicit supplied-product selections remain useful for diagnosis. Rebuild only
invalidated products, run one causal final plan per coherent change, and retain
unaffected passing evidence. Preparation time belongs inside the declared
budget. The all-function limit inspection rejects oversized functions before
native packaging; it does not replace bytecode verification.

Earlier optimization trials, per-owner measurements and completed checkpoint
narratives are in the [dated history](https://github.com/eworker-inc/Windvale/blob/928f772e9e65c1840f263cf6ac099ab1bae0df27/Documents/Project/Verification-Throughput-History-2026-09-26.md).

## Six-item simplification goal

The maintainer authorized this program on September 26, 2026. Its purpose is to
shorten the path from a code change to a useful result while preserving accepted
contracts, host coverage and qualification requirements. This table owns its
completion status; the implementation order is diagnostics and documentation,
then CI reuse, targeted refactoring, verifier consolidation and obsolete-code
cleanup. Product milestones remain in the existing completion matrix.

| Item | Completion evidence | Status |
| --- | --- | --- |
| 1. Emission diagnostics | Maintained source reproducer and original Package-Lock snapshots pass seven cases each on Windows/Debian; exact rule, canonical type name, function location, malformed evidence and unchanged successful output covered. | Complete |
| 2. CI preparation and reuse | One preparation per exact input closure; completed products survive a later failure; focused Windows/Linux jobs complete with cache-hit and cache-miss behavior measured. | In progress |
| 3. Targeted refactoring | Emission analysis delegates vector/resource validation to private helpers; its slots fall from 1,486 to 645. Windows/Debian each pass 32 byte/diagnostic comparisons and seven diagnostic cases; construction and emission costs recorded. | Complete |
| 4. Verification consolidation | Project-WVB host wrappers share one implementation and retain old checkpoints; 125 Windows/128 Debian cache cases and 335 routing cases pass. Distinct publication, reconstruction and legacy coverage remain retained by audit. | Complete |
| 5. Active documentation | Throughput and library checkpoint history archived and indexed; Progress and the matrix point to the completion plan's current blockers; active links/catalogs verified. | Complete |
| 6. Obsolete-code audit | Named cleanup candidates audited against consumers, generators and recovery dependencies; retained candidates and reasons recorded; no unsupported deletion or cache purge. | Complete |

Do not mark an item complete from recommendations alone. Record each implemented
batch, its exact evidence and remaining limitations here. A justified audit may
conclude that a candidate should be retained. Do not force deletions to meet a
count. Generated artifact readers and immutable bootstrap recovery provenance
are not obsolete merely because they resemble other source files.

### First cleanup batch

The split emitter reports the containing source function, declaration line,
module index, WIR operation, nominal type and the rule from the actual rejection
branch. The maintained reproducer intentionally separates canonical type order
from declaration order. It revealed a wrong type name in the first diagnostic;
the emitter now uses the validated reverse symbol lookup.

The original Package-Lock rejection was an exhausted ownership-analysis bound,
not an unmapped nominal target. Repeated constructors triggered conservative
rejection even for acyclic Copy-only records. Diagnostics identified that rule;
the later [ownership scan fix](../Evidence/2026-09-26-Ownership-Scan.json) removes
duplicate child types from the recursive work while retaining its bounds. The
[corrected evidence](../Evidence/2026-09-26-Emission-Diagnostic-Correction.json)
supersedes the initial inferred cause while retaining the original run history.
The maintained source preparation and seven cases take 2.5 seconds on Windows
and 7.3 seconds on Debian. The original snapshots also pass seven cases per
host. Final emitter compilation and parallel packaging took about 8 minutes
29 seconds, with 1,504 of 2,048 function slots used. Peak memory was not measured.

Completed checkpoint chronology and the earlier qualification review now live
in the dated history. The completion plan owns the current consumer blockers;
the matrix and Progress link to that owner instead of repeating stale helper
limitations. This reduces the throughput plan from about 700 lines to roughly
300 while retaining its design and verification requirements.

The first dependency audit retains the generated compiler artifact readers:
`Projects/Tools/Windvale-Compiler-Emission-Driver.wvproj` consumes them and
`Tools/Native/Generate-Compiler-Artifact-Readers.mjs` owns their generation.
They are a bounded emission closure, not unused copies. No tracked C# projects
or source remain. Preserve the [Stage 0 recovery provenance](../../Bootstrap/Stage0/README.md)
and valid development caches. The [scoped audit](../Evidence/2026-09-26-Simplification-Documentation-And-Retention.json)
also identifies consumers for the historical source predecessor, paired host
launchers, build wrappers and full front-door owner. They cannot be removed as
unused code. Item 4 owns consolidation of their duplicated implementation;
the audit does not claim whole-repository reachability or current qualification.

The [library checkpoint history](https://github.com/eworker-inc/Windvale/blob/928f772e9e65c1840f263cf6ac099ab1bae0df27/Documents/Project/Library-Development-History-2026-09-26.md)
preserves the old borrowing and delivered-consumer detail. Progress no longer
lists completed record-vector work as a next step or repeats the obsolete
pre-delivery consumer status. Its active text is below 2,000 words.

### Ordinary owned-console baseline

On 3 October 2026, the maintained `Owned-Vector-Scope.wv` workload was copied
to a private development project and its final result changed from `42` to `43`,
retaining its 1,000 scalar-Vector reserve/release cycles. The source builds used
the prepared compiler at revision `77757eb8cdb0a42c3eff5cebedae6d88bebbbb47`.
Both hosts produced identical edited WVB and native WVO; an invalid source edit
preserved the last output, and restoring the valid source reproduced its bytes.

| Measured step | Windows x64 | Real Debian x64 |
| --- | ---: | ---: |
| Changed source to WVB | 8.4 s | 75.3 s |
| Current native lowering | 4.7 s | 60.5 s |
| Repeated packaging with cached runtime objects | 8.3 / 7.5 s | 64.7 / 55.9 s |

Owned-console packaging now retains the six assembled runtime objects under
their exact source and producer identities. Preparing those objects took
46.7 seconds on Windows and 51.5 seconds on Debian. The repeated packaging
measurements above forbid compiler, publisher and runtime-object construction;
both applications execute with the expected `43`. Windows application bytes
also match the prior uncached packaging of this edited workload. Earlier warm
Windows packaging took 50.9 seconds at revision `8c19c1f2`; Debian's first
uncached run reached its packaging deadline, so it supplies no completed
before/after timing. The
[source-to-WVB runbook](../Runbooks/Native-Source-To-Wvb.md#current-owned-memory-console-development)
owns the commands and preparation boundary.

These are focused source-edit and packaging measurements using prepared tools.
Peak process memory was not measured. They do not close the ordinary-development
phase or the memory completion gate. Measure an affected compiler component and
reduce the remaining Debian wrapper overhead before closing that phase.

CI already requests cache saving after failure. Its remaining preparation work
must finish or checkpoint before the enclosing 15-minute job is cancelled.
The next CI batch will separate exact tool preparation from behavior execution
and measure cache-miss and cache-hit runs; adding a second cache wrapper alone
would not resolve this boundary.

The existing builder now accepts `--prepare-only` with an explicit deadline
and `--prepared-compiler-only` for later product builds. The latter refuses a
missing exact checkpoint before construction and preserves existing output.
The existing split-cache owner covers phase selection, stale keys, corrupt
products, inherited prepared-only mode and reuse after a later failure; the
[boundary evidence](../Evidence/2026-09-26-Compiler-Preparation-Boundary.json)
records the paired-host checks. See the
[preparation procedure](../Runbooks/Native-Tests.md#separate-current-compiler-preparation).
The [native preparation measurement](../Evidence/2026-09-26-Native-Compiler-Preparation-Measurement.json)
created the Windows compiler checkpoint in 19 minutes 26 seconds and reused it
in 1.24 seconds. Debian lacked the historical predecessor and was stopped
cleanly after 9 minutes 7 seconds when the original estimate proved wrong;
completed caches were retained. These are different cache conditions, not a
paired cold-build comparison. CI integration and completed Debian preparation
remain open. Do not fit either cold graph inside a 15-minute behavior job.

The wrapper audit must also distinguish old rejection checkpoints from current
behavior. The full legacy front-door script expects
`Memory-Budget-Type-Identity.wv` emission to fail, while the current supplied
compiler emits it successfully. Do not repair that mismatch by changing only
the expected diagnostic text. Audit the owning cases and current selectors
before retiring or replacing the old checkpoint.

### Shared project cache wrapper

The project-WVB development cache now has one implementation and two four-line
host launchers. This replaces 266 lines of paired shell logic with 211 lines
of shared code and eight launcher lines. Cache identities, host record endings
and successful output bytes remain compatible. The existing split-cache owner
covers construction, corruption, cleanup, publication races, changing inputs,
linked paths and prepared-only Project 4 delegation; no new verification owner
or compiler pipeline was added.

The [consolidation evidence](../Evidence/2026-09-26-Shared-Project-Wvb-Cache.json)
records 125 passing cache cases on Windows in 25 seconds and 128 on Debian in
129 seconds, plus 335 routing cases. The small retained-cache workload improves
from a 0.58-second warm median to 0.23 seconds on Windows; Debian remains about
1.08 seconds. These are local measurements with concurrent work, not controlled
compiler-throughput claims. Peak process memory was not measured.

Wrapper-only changes now select that focused owner instead of database and OS
behavior suites. The shared checkpoint specification remains a conservative
filename-level route; inspect the changed section before running its proposed
domain owner. Other wrappers retain the distinct responsibilities documented
in the dependency audit. This consolidation does not retire legacy front-door
coverage or complete CI preparation.

### Emission validation refactor

The emission analysis function now owns bytecode sizing and stack accounting,
with private helpers for vector and task/foreign-resource validation. Its slot
use falls from 1,486 to 645; the helpers use 354 and 581. The separate WIR
validator still owns the emitter's overall maximum of 1,504 slots. This creates
headroom in the changed function without claiming a lower global maximum.

The [refactor evidence](../Evidence/2026-09-26-Emission-Validation-Refactor.json)
records 32 old/new comparisons on each host, covering all 20 moved operation
kinds, and seven diagnostic cases on each host. Successful bytes, rejection
diagnostics and preserved output match. Emission command time remains close:
6.56/6.54 seconds for reference/candidate on Windows and 17.10/17.24 seconds on
Debian. These include process startup and are not a throughput-speedup claim.
Candidate construction and parallel packaging took about 8 minutes 13 seconds.
The emitter WVB grows by 1,012 bytes; process peak memory was not measured.
The later ownership scan fix closes the repeated-constructor rejection; consumer
integration remains in the completion plan. CI preparation and reuse remain open
under item 2.

## Earlier baseline

The [dated review](https://github.com/eworker-inc/Windvale/blob/928f772e9e65c1840f263cf6ac099ab1bae0df27/Documents/Project/Verification-Throughput-History-2026-09-26.md#earlier-qualification-baseline-and-migration-review) preserves qualification timings, owner rankings and the database construction inventory. Those measurements describe their recorded source states. Use the active six-item goal for the current work order.

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

## Longer-term verification roadmap

These broader redesign phases remain proposals. They do not expand the six-item simplification goal or block ongoing product implementation.

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

Bundling must remain capacity-aware. The three branch-page cases exceed the
ordinary native lowerer's output limit when combined. Their retained bundle
therefore uses the existing bounded segmented path: two image fragments, with
no increased compiler, lowerer, execution, or diagnostic limit. Other candidate
bundles still require the same capacity and behavior evidence.

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
