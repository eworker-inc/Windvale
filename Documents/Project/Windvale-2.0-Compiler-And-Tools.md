# Windvale 2.0 compiler and tooling review

> Status: Proposed; code-backed findings and improvement candidates
> Authority: Informative; not an accepted implementation or qualification plan
> Last reviewed: 2026-09-08

## Intended outcome and review boundary

Make the compiler easier to change, faster to give relevant feedback, and better
at explaining its behavior. Evolve the existing analysis/emission pipeline,
typed Windvale IR (WIR), verification boundaries, deterministic output, caches,
and resumable checkpoints rather than replacing them with a second compiler.

This document records the follow-up AI review requested by the project owner
on 2026-09-08. The review inspected compiler, build, verification, editor, and
containment-test code at revision `73257851`. It was read-only and did not run
benchmarks or qualification suites. References below identify inspected owners;
they are not claims that the entire compiler was audited or that a speedup has
been measured. Recheck each observation against the selected implementation
state before acting on it.

The [2.0 release plan](Windvale-2.0-Release-Plan.md) owns proposed scope and
compatibility selection. The [ideas register](Windvale-2.0-Ideas.md) owns the
broader language and product proposals. All recommendations here remain
Proposed; recording them does not authorize implementation or change the
active 1.0 delivery gate.

The later [stack and application review](Windvale-2.0-Stack-And-Applications.md)
extends the native-tool boundary with WVO admission indexing and distinct
retained assembly/link plans, while preserving independent reconstruction and
the existing object-writer migration direction.

## 1. Compute temporary lifetimes together

Observation: the [temporary-slot allocator](../../Compiler/Windvale/Source-Wvb-Temporary-Slots.wv)
calls `Findˉlifetime` separately for each temporary. That function scans
operations, checks block terminators, and searches backwards through operands.
Allocation then searches existing slots for reuse. The repeated work is visible
in the code; its share of total compilation time is not established.

Proposal: compute definition and last-use information for all temporaries in
shared passes, then organize reusable slots by exact type. A temporary is an
intermediate value; a slot is storage that compatible, non-overlapping values
can reuse. Preserve deterministic selection and the special treatment of
control-flow merge values rather than substituting an ordinary linear lifetime
rule where it is unsound.

Tradeoff and evidence: indexing retains additional bounded state. Compare large
functions with increasing operation, operand, block, and temporary counts;
include branches, loops, merge values, malformed directories, and exhaustion.
Keep the existing allocator as a correctness oracle during development. Check
slot validity, deterministic output, elapsed time, and peak memory before
claiming an improvement. This is the strongest initial performance candidate,
not a measured bottleneck or an instruction to change source semantics.

## 2. Preserve diagnostics from the original analysis

Observation: [source analysis](../../Compiler/Windvale/Source-Analysis-Core.wv)
returns phase statuses but does not retain all detailed failure locations.
The [analysis driver's failure renderer](../../Tools/Windvale.Build/Compiler-Analysis-Driver.wv)
reruns symbol analysis and, for WIR failures, WIR construction to recover those
details. The proposed improvement concerns diagnostic reconstruction, not
independent admission of artifacts crossing a trust boundary.

Proposal: return one bounded diagnostic record from the failing pass, containing
a stable identity, phase, primary source span, related locations, expected and
observed values, and the relevant resource or target requirement. Command-line
and editor clients render that record without repeating compilation.

Tradeoff and evidence: retained diagnostics consume memory and must obey an
explicit count/byte ceiling. Extend the existing
[source-analysis diagnostic checks](../../Tools/Native/Verify-Source-Analysis-Diagnostic.mjs)
to prove stable locations, bounded related information, and rejection of invalid
diagnostic records. Instrument failing compilations to show that reporting does
not rerun the failed phase. Version any changed machine-facing diagnostic
format deliberately. This is the recommended first focused implementation
candidate because it does not depend on a new source edition.

## 3. Separate responsibilities and typed working state

Observation: [Source-Wir-Core](../../Compiler/Windvale/Source-Wir-Core.wv)
was approximately 17,500 lines at review. It combines type-shape handling,
generic and closure machinery, construction state, expression/control-flow
compilation, and serialization. Its build-state record is repeatedly
reconstructed through long positional argument lists. Size alone does not prove
a defect, but these distinct responsibilities justify reviewing their ownership.

Proposal: extract cohesive owners for type identities/classification,
function-local construction, expression/control-flow lowering, generic/closure
integration, and WIR serialization. Use typed records, variants, arenas, and
bounded mutable builders once their required Foundation paths are available.
The broader rationale remains in
[typed working state](Windvale-2.0-Ideas.md#d-typed-compiler-and-runtime-working-state).

Tradeoff and evidence: do not split into numbered fragments, create circular
dependencies, replace every compact table with a heap object, or serialize
between every internal helper. Preserve explicit immutable phase evidence and
useful checkpoints. Migrate one boundary at a time; compare deterministic
products, diagnostics, rejection behavior, compiler time, and retained memory.
Code readability and local reasoning are outcomes alongside performance.

## 4. Replace text-based artifact-reader extraction

Observation: the [artifact-reader generator](../../Tools/Native/Generate-Compiler-Artifact-Readers.mjs)
discovers functions with regular expressions and brace counting, follows
textual call patterns, and transforms a closure-validation body using string
markers. It deliberately produces compact target-specific readers; this review
does not demonstrate an incorrect generated artifact.

Proposal: first move genuinely shared readers and contracts into explicit
modules imported by both analysis and emission. Where selective extraction
remains necessary, use compiler-produced declaration/dependency information
instead of treating formatting and source spellings as the extraction API.

Tradeoff and evidence: preserve the memory and bootstrap role that motivated
compact readers. Do not discard independent artifact validation or reintroduce
a monolithic bootstrap product merely to remove a script. Check generated
product equivalence, dependency completeness, missing/unsupported declarations,
source-format variations, memory bounds, and deterministic reconstruction.
Retire the textual route only after the replacement preserves its required
contracts and has an explicit bootstrap transition where needed.

## 5. Centralize mechanical operation facts

Observation: the [WIR builder](../../Compiler/Windvale/Source-Wir-Core.wv)
and [WIR validator](../../Compiler/Windvale/Source-Wir-Validation-Core.wv)
both contain long operation-identifier lists. Repetition makes consistency an
ongoing maintenance responsibility; it is not evidence that the lists disagree.

Proposal: define a small versioned operation description for mechanical facts:
identity, encoding, operand/result categories, required version or feature,
effect classification, and inspection name. Generate repetitive declarations,
decoder scaffolding, and coverage checks from that owner where appropriate.

Tradeoff and evidence: shared definitions can create common-mode errors.
Independently check control flow, ownership, effects, malformed encodings, and
runtime behavior; do not generate the implementation and its correctness oracle
from the same semantic algorithm. Keep WIR, WVB, and machine operations distinct
contracts, not one universal instruction table. Require deterministic generation
and explicit failures for duplicate, missing, or incompatible entries.

## 6. Unify build actions and cache explanations

Observation: the [current compiler cache](../../Tools/Native/Current-Split-Compiler-Cache-Core.mjs),
[split-project build](../../Tools/Native/Build-Cached-Split-Project-Wvb.mjs),
and [verification-result cache](../../Tools/Native/Verification-Owner-Result-Cache.mjs)
already preserve valuable construction and test evidence. Their different
responsibilities should not be collapsed into one undifferentiated cache.
Project parsing and process orchestration also occur in several tool paths,
including the [Project 2 compilation helper](../../Tools/Native/Compile-Project-2-With-Compiler.mjs).

Proposal: organize those mechanisms around one inspectable action graph: a
declared set of build steps and their dependencies. Each step records exact
inputs, producer identity, target/options, outputs, admission requirements,
resource ceilings, and cache identity. Expose why a step rebuilt, what a command
will execute, and which missing product makes a cold run expensive.

Consolidate repeated manifest parsing and bounded process-launch policy where
contracts match. Gradually make Windows/Linux wrappers thin host adapters, with
ordinary operations discoverable through the existing `wv` interface. No new
command syntax is frozen here.

Tradeoff and evidence: reuse the existing cache families rather than adding a
competing cache layer. Keep product reuse distinct from passing behavioral test
evidence. Test transitive invalidation, input changes during construction,
corruption, concurrent publication, cancellation, output preservation, and
recovery. Retain content-based identity and required admission; timestamps or
ambient file discovery cannot replace declared immutable inputs. Compare warm
and cold phase costs before claiming orchestration savings.

## 7. Make verification dependencies declarative

Observation: the [native changed-file planner](../../Tools/Verify/Get-Native-Changed-Verification-Plan.ps1)
was approximately 5,300 lines at review, with detailed path matching and
specialized selection logic. Existing owner, duration, and development-product
registries already provide part of the needed structure.

Proposal: extend validated manifests to declare source/specification
dependencies, shared products, behavioral groups, required hosts, and expected
and maximum durations. Retain a small explicit rule layer for exceptional
cases. Every selected check should explain the chain from changed contract to
affected product to behavior tested.

Tradeoff and evidence: a small router is not sufficient if manifests omit
dependencies. Unknown or incomplete ownership must fail closed. Differentially
compare selections with the current planner, test intentional dependency
mutations and malformed manifests, and preserve exclusions that avoid unrelated
work. Build on the existing
[verification-throughput plan](Verification-Throughput-Plan.md), not another
coordinator or competing registry. Preserve the local budget and distinct
cross-host qualification boundary.

The follow-up [qualification and testing review](Windvale-2.0-Qualification-And-Testing.md)
examines expensive owners, existing product reuse, precise behavior selection,
durable qualification resumption, and resource-aware scheduling. It records
historical timing limits and the evidence needed before removing repeated work.

## 8. Add a reusable semantic tooling interface

Observation: the [editor package](../../Tools/Editors/Windvale/package.json)
provides syntax highlighting and language configuration. Those are useful
presentation features, not a compiler-backed semantic service.

Proposal: expose a read-only compiler analysis interface for definitions,
references, inferred types/effects, borrow-conflict explanations, capability
requirements, and source-to-WIR/WVB inspection. Add semantic rename as a
reviewable edit proposal, then place an editor language-server protocol over
the same interface. It should serve human tools and AI agents without a second
approximate parser or resolver.

Tradeoff and evidence: incremental requests need exact source-snapshot identity,
bounded retention, cancellation, and safe invalidation. Test incomplete source,
stale requests, edits across modules, localized source maps, ambiguous names,
and ownership/effect errors. A rename must not silently change declaration
identity or edit a newer source snapshot. Measure interactive latency and
long-session memory. This interface does not grant filesystem or execution
authority to arbitrary compiler requests.

## 9. Develop optimization and valid-program testing together

Observation: the [WVB emitter](../../Compiler/Windvale/Source-Wvb-Core.wv)
already performs reachability-based pruning. The
[containment tooling](../../Tools/Native/Test-Random-Containment.mjs) exercises
source and binary rejection boundaries. Neither should be described as absent.

Proposal: grow a clearly owned optimization pipeline with bounded passes for
selected constant propagation, copy elimination, inlining, range analysis, and
proved bounds-check elimination. Each pass should explain its transformations,
legality conditions, resource cost, and invalidated analysis evidence.

Extend existing suitable test owners with generated valid programs,
differential execution across declared supported paths, and automatic reduction
of a failing program to a smaller reproducer. Keep deterministic seeds and
bounded generation, execution, and reduction. Compare values, traps, observable
effects, and required ordering, not only successful return codes.

Tradeoff and evidence: optimizer defects often involve valid source, while
malformed-input testing protects a different boundary. Preserve both without
creating duplicate verifier entry points. Never erase observable traps,
authority, resource failures, or strict numeric semantics as an optimization.
Use named workloads and code-size/time/memory limits. Do not begin with a
speculative universal machine IR, a parallel backend, or relaxed numerics.

## Recommended order and compatibility

1. Preserve diagnostics rather than recomputing failed analysis.
2. Measure and improve temporary lifetime/slot allocation.
3. Extract shared compiler contracts and replace text-based artifact extraction.
4. Unify action dependencies, cache explanations, and verification routing.
5. Add compiler-backed semantic tooling.
6. Expand measured optimization alongside differential valid-program testing.

These are proposed priorities, not implementation authorization. Typed working
state and mechanical operation definitions support the relevant steps; they
need not become an all-or-nothing rewrite before useful work can proceed.

Most recommendations can be compatible 1.x implementation or tooling work.
Internal checkpoint, diagnostic, tool, and generation contracts may still need
explicit version changes and cache invalidation. Do not force a source-edition
break merely because the work is discussed in 2.0 planning. Preserve accepted
source behavior, independent verification, bootstrap provenance, and the active
1.0 delivery plan throughout any selected transition.
