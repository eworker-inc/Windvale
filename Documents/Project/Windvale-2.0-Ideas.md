# Windvale 2.0 ideas register

> Status: Proposed; initial architectural recommendations for discussion
> Authority: Informative; no idea is an accepted semantic or release change
> Last reviewed: 2026-09-08

## Purpose and review boundary

This register preserves the initial 2.0 recommendations requested by the project
owner on 2026-09-08. They came from an AI architectural review of the 1.0
specifications, sampled compiler/runtime/library source, and future-work
proposals. That review was not an exhaustive code audit, benchmark run, or new
qualification result.

A follow-up review of the actual compiler and its tools is recorded in the
[compiler and tooling review](Windvale-2.0-Compiler-And-Tools.md). It owns nine
focused findings covering temporary-slot analysis, diagnostic reuse, modular
working state, artifact-reader extraction, operation definitions, build/cache
actions, verification dependencies, semantic tooling, and optimization/testing.
Those findings distinguish inspected behavior from unmeasured expectations.

The [qualification and testing review](Windvale-2.0-Qualification-And-Testing.md)
extends those findings with shared test construction, narrower behavior
selection, bounded scheduling, and safe qualification resumption. It builds on
the existing throughput plan without reducing independent safety claims.

The [stack and application review](Windvale-2.0-Stack-And-Applications.md)
covers sampled areas beyond the compiler and tests: runtime/native tools,
libraries, WVDB, OS/distribution, and browser/application boundaries. It records
current-contract concerns separately from optional future enhancements.

The [2.0 release plan](Windvale-2.0-Release-Plan.md) owns scope selection and
compatibility planning. All entries below remain Proposed. Their local labels
are discussion references, not decision numbers. Priorities express an initial
opinion and may change after real workload evidence.

## A. More expressive borrowing

Priority: First language-design candidate.

The [edition-1 borrowing rules](../../Specifications/Windvale-Language-1.0.md#borrows)
allow a returned borrow only when the signature has exactly one borrowed
parameter. User-declared records cannot contain borrows; particular Foundation
borrowed results receive explicit treatment.

Explore explicit lifetime relationships for ambiguous public boundaries,
user-defined borrowed views, and compiler-proven disjoint mutable access.
A parser should be able to return a view tied to its input while also borrowing
a diagnostic sink. A buffer algorithm should be able to mutate two proven
non-overlapping regions without exposing unrestricted aliasing.

Keep ordinary cases annotation-free. No view may outlive its owner, and no
extension may silently permit task/suspension escape, use after move, or mutable
aliasing. More expressive rules need bounded checking and diagnostics that name
the owner, conflicting use, and required lifetime.

Evidence: compare a real parser, a database view, and a split-buffer operation;
include multiple possible owners, overlap, escape, cleanup, and suspension
rejections. Measure copies, allocations, diagnostic clarity, and compiler cost.
This is worth API migration where justified, but additive rules should be
considered before requiring an incompatible edition.

## B. Generic composition of effects

Priority: First language-design candidate alongside borrowing.

An effect is a declared kind of work, such as allocation, suspension, or a
capability call. Edition 1 gives functions and function values exact
[effect sets](../../Specifications/Windvale-Language-1.0.md#capabilities-and-effects).
Explore effect polymorphism: a generic helper declares the effects of its
callback plus the helper's own effects, rather than duplicating the helper or
claiming an unnecessarily broad set.

A bounded traversal could use a pure callback or an allocating callback while
preserving the exact distinction at each concrete call. This must not introduce
ambient authority, implicit suspension, effect erasure, or inferred overload
selection. Capability approval still resolves the exact required interfaces;
an effect parameter is not a capability grant.

Evidence: compare pure and allocating traversals and a hosted composition case.
Check protocol and closure boundaries, transitive approval, denied effects,
recursive constraints, generic identity, code growth, and bounded inference.
Choose the precise rules and syntax only after those examples are reviewed.

## C. Less repetitive source without hidden behavior

Priority: High; compatible library/tooling opportunities first.

The paper [compiler binder](Language-1.0-Paper-Corpus/04-Compiler-Front-End/Source/Front-End-Binder.wv)
illustrates repeated unwrapping and rewrapping of errors. It is design-workload
evidence, not proof that every illustrated API is implemented. Explore:

- Concise error adaptation through an explicitly named mapper; do not infer an
  error conversion from return context or hide partial/indeterminate progress.
- Field-name shorthand when a field and lexical variable have the same name,
  preserving exact evaluation order and ownership transfer.
- Explicit single-symbol imports to avoid repeated forms such as
  `Result.Result`, without wildcard lookup or an ambient prelude.
- Formatting/interpolation into a supplied bounded builder, using the existing
  [Formatting contract](../../Specifications/Windvale-Language-1.0-Foundation.md#formatting)
  as the starting point rather than inventing hidden allocation.
- Narrow expected-type inference for an already-resolved generic declaration,
  such as an empty collection constructor with an explicitly typed destination.
  It must not add conversions, overload searches, or ambiguous substitutions.

The last item deliberately reconsiders edition 1's
[argument-only generic inference](../../Specifications/Windvale-Language-1.0.md#generics-and-protocols).
Not every item requires an edition change. Existing planned Result mapping and
library functionality should be completed before attributing all repetition to
the language itself.

Evidence: compare real error-heavy parser/service functions and bounded text
construction. Test owned temporaries, mapper failure/effects, ambiguous names,
conflicting type evidence, capacity refusal, and left-to-right evaluation.
Record readability as well as line count; fewer tokens are not a safety proof.

## D. Typed compiler and runtime working state

Priority: Structural implementation recommendation; not inherently edition 2.

Sampled [generic resolution](../../Compiler/Windvale/Source-Generic-Resolution-Core.wv)
uses byte-encoded evidence and status records. The
[runtime borrow-view component](../../Runtime/Windvale/Foundation-Borrow-View-Core.wv)
also maintains a bounded packed internal retention table. These are concrete
examples, not a claim that all internal byte representations are inappropriate.

After the required Foundation APIs are usable, migrate selected working state
to typed records/variants, typed arenas and handles, and bounded mutable builders.
Keep serialization at genuine persistence, interchange, and checkpoint
boundaries. Preserve compact layouts where a real contract or measurement
justifies them; do not replace every byte table with separately allocated
objects or discard immutable checkpoint evidence.

The hypothesis is fewer manually maintained offsets, sentinel states, copies,
and repeated checks. It is not an established performance result.

Evidence: choose one compiler table and one runtime working table. Compare
deterministic outputs, malformed-input behavior, cold/warm build latency,
runtime cost, retained memory, and cleanup. Reuse unchanged qualification
evidence only when its complete declared inputs still permit it. Prefer
compatible 1.x delivery rather than an unnecessary source break.

The [focused compiler review](Windvale-2.0-Compiler-And-Tools.md#3-separate-responsibilities-and-typed-working-state)
extends this recommendation with concrete WIR ownership boundaries. Its first
recommended focused changes are diagnostic reuse and temporary-slot analysis;
the broader typed-state migration is not a prerequisite for every improvement.

## E. Semantic portability without an operating-system list

Priority: Language/target-contract review candidate.

Edition 1 requires an explicit
[platform declaration](../../Specifications/Windvale-Language-1.0.md#source-descriptor-and-module-header).
Pure Foundation modules commonly list Windows, Linux, and Windvale OS.
Explore an explicit declaration meaning any target implementing the named
semantic contract, so a pure parser need not change for each new conforming host.

Do not confuse eligibility with qualification. A new target still needs its
own support evidence, and unmet semantic features must reject admission.
Platform restrictions, unsafe ABIs, extensions, capabilities, and artifact
requirements remain explicit; no wildcard may bypass their checks.

Evidence: admit one capability-free library under multiple exact target
descriptors, reject a descriptor lacking required semantics, and prove that a
platform-specific dependency narrows the resulting artifact correctly. Compare
an additive target-registry mechanism with a new source form before choosing
an incompatible contract.

## F. Optional localization frontend versus stored localized source

Priority: Contested design alternative; do not implement without user review.

The existing [localized-source invariant](../../Specifications/Windvale-Language-1.0-Localized-Source.md#product-invariant)
already resolves localized labels to one canonical declaration identity. It
does not create separate runtime languages, and canonical identity is not
inherently English.

Keep Unicode identifiers, multilingual diagnostics, and localized API
documentation. Compare the existing exact stored keyword/API vocabularies with
localized authoring and review views over canonical source, or an optional
deterministic source frontend feeding the semantic compiler. The initial
recommendation favors a small independently usable semantic compiler, not
removal of multilingual access.

The counterargument is important: a view-only approach can disadvantage people
using ordinary editors, command-line tools, or recovery environments. The
existing [semantic source views exploration](Windvale-Semantic-Source-Views-And-Localization.md)
records relevant rationale and earlier alternatives. Treat them as context,
not authorization to undo the accepted localization direction.

Evidence: real multilingual editing, review, merge, exact source maps,
offline/recovery use, conversion round trips, Unicode/confusable rejection,
pack admission, and front-door time/memory comparisons. Do not depend on AI
translation for canonical compilation. No change to the official macron naming
convention is proposed merely for cosmetic familiarity.

## G. Independently useful component releases

Priority: Product-policy discussion, not a language requirement.

The [1.0 product plan](Windvale-1.0-Product-Plan.md#required-workstreams)
deliberately couples the toolchain, required libraries, WVDB, and operational
release gates. For future releases, consider independently versioned delivery
of the language/toolchain/runtime, libraries/service platform, WVDB, and
Windvale OS, backed by tested compatibility combinations.

One architecture need not require one release clock. The intended benefit is
that useful components can ship without waiting for unrelated work. The cost is
a larger installation, support, integration, and compatibility matrix. An
integrated distribution may still provide one tested combination.

Evidence: define component ownership, supported version combinations,
capability/ABI admission, upgrade and rollback behavior, security maintenance,
offline installation, and data-format migration. Keep the current 1.0 gate
unchanged unless a separate explicit decision revises it. This idea does not
authorize tags, branches, releases, or a support promise.

## H. Focus the future product on bounded services and effects

Priority: Strategic workload selection, not mandatory new syntax.

Favor useful bounded services and verified AI tool execution as early
integration consumers rather than simultaneously making a general scientific
platform, inference engine, and broad desktop OS release requirements.
The [verified AI workloads proposal](Verified-AI-Workloads-And-Agent-Aware-Inference-Proposal.md)
is a relevant starting point: verifying code, authority, limits, and execution
evidence does not prove model reasoning or an opaque provider's computation.

Scientific modeling, accelerators, and Windvale OS remain legitimate distinct
workstreams, not removed ambitions. Keep their providers and specialized
contracts outside the mandatory language nucleus unless a reviewed workload
demonstrates a shared need.

Evidence: select a finite service workflow with rights-limited data/tools,
deadlines, memory bounds, provider loss, durable state, and exact external
mutation outcomes. It must not replay uncertain mutations or treat model
output as authority. Compare usefulness, operational effort, and measurable
end-to-end cost before expanding the release scope.

## I. Reuse admitted models and make operation outcomes precise

Priority: Investigate current-contract concerns first, then measure focused
cross-stack improvements.

The [remaining-stack findings](Windvale-2.0-Stack-And-Applications.md) recommend
bounded indexed working models for WVO admission, assembly/linking, database
transactions, and FAT32 reads; explicit aggregate reservations for concurrent
operations; and coherent ownership, cancellation, and publication in browser,
package, and credential code. Repeated validation can be avoided only inside
an exact immutable identity/lifetime boundary, not across a new trust boundary.

Investigate the recorded stream-accounting, activation-concurrency, and
secret-buffer cleanup concerns as existing-contract work, not future features.
The review did not reproduce those failures or establish exploitability.
Consider segmented I/O and bounded group commit only after measuring their
workloads and selecting exact ownership, progress, and durability contracts.

Evidence: preserve independent oracles, byte identities where required,
cross-host behavior, stale-state rejection, failure cleanup, and recovery.
Measure retained memory as well as elapsed time; a reusable index or shared
buffer is not automatically cheaper. Most candidates can be compatible 1.x
work, while new provider APIs or serialized evidence need explicit versioning.
Existing 1.0 library, service, package, and OS plans retain their scope.

## Principles to keep and additions to defer

Preserve immutable defaults, typed recoverable failures, checked numerics,
explicit capabilities, deterministic local release, structured concurrency,
bounded resource use, and verified WVB as the distribution contract. Windows
and Linux remain permanent hosts, with Windvale OS as a separate integration
target over shared contracts.

The initial recommendation is not to add general exceptions, inheritance,
ambient reflection, detached tasks, unrestricted macros, or a second execution
model. Explicit runtime protocol values and constrained mathematical operators
may be reconsidered only after real programs show a need and their dispatch,
effects, numeric behavior, bounds, and target costs are fully specified.

Prefer evidence-driven simplification over cosmetic syntax churn. Before
promoting any entry, record the selected alternative, rejected alternatives,
exact changed contract, owned implementation path, migration rule, resource
bounds, and focused verification plan. The aim is stronger practical usability,
not the number of features or incompatible changes in a release.
