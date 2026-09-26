# Verification throughput redesign plan

> Status: Active implementation plan
> Authority: Informative
> Last reviewed: 2026-09-26

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
narratives are in the [dated history](Verification-Throughput-History-2026-09-26.md).

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
| 2. CI preparation and reuse | One preparation per exact input closure; completed products survive a later failure; focused Windows/Linux jobs complete with cache-hit and cache-miss behavior measured. | Open |
| 3. Targeted refactoring | Extract cohesive responsibilities from a changed large function; measure slot headroom and build/runtime cost; preserve behavior and output contracts. | Open |
| 4. Verification consolidation | Audit overlapping wrappers and construction; merge/remove duplicated execution where no unique coverage is lost; retain named cases and required host boundaries. | Open |
| 5. Active documentation | Remove completed chronology and duplicate current status from active guidance; preserve indexed history and passing links/catalogs. | In progress |
| 6. Obsolete-code audit | Identify consumers, generators and recovery dependencies before deletion; remove proven-unused entries or record why candidates must remain; retain useful caches. | In progress |

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

The original Package-Lock rejection is an exhausted ownership-analysis bound,
not an unmapped nominal target. Repeated constructors trigger conservative
rejection even for acyclic Copy-only records. Diagnostics identify that rule;
they do not fix the underlying scan or invent an expression location. The
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
and valid development caches. This scoped audit does not establish that every
old wrapper is still needed; the wrapper/construction audit remains open.

CI already requests cache saving after failure. Its remaining preparation work
must finish or checkpoint before the enclosing 15-minute job is cancelled.
The next CI batch will separate exact tool preparation from behavior execution
and measure cache-miss and cache-hit runs; adding a second cache wrapper alone
would not resolve this boundary.

The wrapper audit must also distinguish old rejection checkpoints from current
behavior. The full legacy front-door script expects
`Memory-Budget-Type-Identity.wv` emission to fail, while the current supplied
compiler emits it successfully. Do not repair that mismatch by changing only
the expected diagnostic text. Audit the owning cases and current selectors
before retiring or replacing the old checkpoint.

## Earlier baseline

The [dated review](Verification-Throughput-History-2026-09-26.md#earlier-qualification-baseline-and-migration-review) preserves qualification timings, owner rankings and the database construction inventory. Those measurements describe their recorded source states. Use the active six-item goal for the current work order.

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
