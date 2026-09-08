# Windvale 2.0 release planning

> Status: Proposed; initial future-release planning, not an accepted release gate
> Authority: Informative; existing decisions and specifications remain controlling
> Last reviewed: 2026-09-08

## Intended outcome

Windvale 2.0 should make safe programs easier to express and maintain without
weakening explicit authority, typed failure, resource bounds, or verified
execution. The working direction is the same strong guarantees with less code
devoted to expressing them.

The project owner requested these planning documents after an architectural
review of the 1.0 specifications, sampled code, and future ideas. That request
authorizes recording and discussing proposals, not accepting every
recommendation, implementing new semantics, or publishing a release.

The companion [2.0 ideas register](Windvale-2.0-Ideas.md) owns the individual
language/product proposals, tradeoffs, current-contract links, and evidence
needed for selection. The [compiler and tooling review](Windvale-2.0-Compiler-And-Tools.md)
owns the follow-up code-backed findings and implementation/tooling candidates.
The [qualification and testing review](Windvale-2.0-Qualification-And-Testing.md)
owns deeper findings on test cost, selection, shared construction, and resumption.
The [stack and application review](Windvale-2.0-Stack-And-Applications.md)
owns the remaining sampled runtime, library, database, OS, distribution, and
browser findings, including current-contract concerns to investigate before 2.0.
This document owns proposed release scope, sequencing, and compatibility review.

## What works now and what remains open

The [Language 1.0 compiler qualification decision](../Decisions/0943-Complete-Windvale-Language-1.0-Slice-8-Qualification.md)
records completion of the frozen compiler track within its declared scope and
target subsets. It does not qualify unfinished libraries or the whole product.
The [Progress dashboard](Progress.md) owns current implementation standing;
this planning set does not create a second status dashboard.

The [Windvale 1.0 product plan](Windvale-1.0-Product-Plan.md) and
[Roadmap](Roadmap.md) remain the active delivery direction. Required library,
database, service, support, and integrated release work must not be described as
completed or moved out of 1.0 merely because a 2.0 proposal mentions it.

No 2.0 source edition, grammar, bytecode version, component matrix, release date,
support lifetime, tag, or qualification gate is selected by these documents.
The next intended product release remains 1.0 under the current
[release naming policy](Release-Names-And-Tags.md).

## Candidate scope

The initial review recommends three distinct kinds of work:

| Track | Candidate outcome | Compatibility treatment |
| --- | --- | --- |
| Source language | More expressive borrowing, callback-effect composition, and simpler explicit syntax | Assess additive possibilities first; incompatible source rules need an explicitly selected newer edition. |
| Compiler, tools, and libraries | Diagnostic reuse, temporary-slot analysis, shared typed contracts, inspectable build dependencies, semantic tooling, and measured optimization | Prefer compatible 1.x delivery when contracts permit; internal/tool contracts may need versioning without a source break. |
| Product and authoring | Independent component release review, better portable-target declarations, optional localization frontend evaluation, and focused service workloads | Require separate product, source, package, or tooling decisions where each actual boundary changes. |

The highest-priority language candidates are borrowing and effect composition.
The follow-up compiler review recommends diagnostic reuse and measurement of
temporary-slot allocation as the first focused implementation candidates.
Migrating compiler/runtime working state onto typed abstractions remains a
structural priority after its required Foundation support is ready. These are
review priorities, not an implementation schedule or measured speedup claim.

The proposed compiler/tooling order is diagnostic reuse, lifetime/slot analysis,
shared contracts and artifact-reader extraction, build/cache/verification
dependencies, semantic editor tooling, and measured optimization with
differential tests. The [focused review](Windvale-2.0-Compiler-And-Tools.md#recommended-order-and-compatibility)
owns its rationale and evidence requirements. Compatible selected work can
support 1.0 delivery; completing this list is not an added 1.0 release gate.

For testing, prioritize visible cold costs, shared immutable test products, and
precise behavior selection before refactoring qualification resumption and
scheduling. Extend the active [throughput plan](Verification-Throughput-Plan.md);
retain independent reconstruction and required Windows/Linux evidence. The
[testing review](Windvale-2.0-Qualification-And-Testing.md) separates implemented
mechanisms, historical measurements, and proposed changes.

For the remaining stack, first investigate stream reservation accounting,
activation writer exclusion, and explicit secret-buffer cleanup. Then prioritize
bounded admitted-state reuse in native tools, transactions, and FAT32 reads,
alongside coherent cancellable browser operations and bounded release tooling.
Segmented I/O and group commit need separate API/durability review. The
[stack review's ordering](Windvale-2.0-Stack-And-Applications.md#recommended-order-and-compatibility)
is a proposed investigation sequence, not evidence of reproduced failures or
approval to implement. Existing-contract corrections need not wait for 2.0.

## Proposed compatibility policy

Breaking old code is an available design choice, not a success criterion. Each
candidate should explain the lasting simplicity, safety, expressiveness, or
measured performance benefit that cannot reasonably be obtained compatibly.

The proposed rules for selecting work are:

- Never silently reinterpret accepted edition-1 source. Incompatible source
  changes require an explicit newer edition and deterministic rejection when
  that edition is unsupported.
- Keep product versions, source editions, Foundation/API versions, WVB versions,
  package contracts, native ABIs, and stored-data formats separate. A product
  named 2.0 does not automatically require a format named 2.0.
- Prefer mechanical migration with reviewable diffs for syntax and naming.
  Ownership, effects, authority, and error-behavior changes need explicit review
  wherever equivalent behavior cannot be proven by the migration tool.
- Reuse the owned compiler and verified execution architecture. Do not introduce
  a parallel compiler or silently restore retired bootstrap implementations.
- Decide edition-1 tool availability, mixed-edition package admission, supported
  component combinations, and the support window before promising migration.
  These questions are still open; perpetual compatibility is not assumed.
- Treat database and persisted-data migration separately from source migration.
  A source-breaking release never implies permission to discard user data.
  Any selected format transition needs backup, restore, rollback, and failure
  behavior defined before release.

## Proposed delivery sequence

1. Complete the useful 1.0 library and product path under its existing owners.
   Compatible improvements need not wait for a future major release.
2. Migrate representative real compiler, parser, and service code onto the
   implemented 1.0 APIs. Record remaining repetition, copies, lifetime
   restrictions, effect limitations, and diagnostic quality.
3. Compare small candidate designs against those programs. Preserve simple
   correctness oracles and include malformed-input, resource-exhaustion,
   ownership, and capability-denial cases where applicable.
4. Select a finite 2.0 scope through named decisions. Write exact semantics and
   migration rules before freezing any new source contract.
5. Implement selected changes through shared compiler/runtime owners and
   qualify only the declared component, host, and target combinations.
6. Publish only after accepting a concrete release gate, compatibility matrix,
   support policy, recovery path, and signed distribution requirements.

These steps are proposed planning checkpoints, not approval for long verifier
runs or permission to interrupt 1.0 delivery.

## Evidence needed before scope selection

Each idea needs an old/new program comparison, the changed contract, an
implementation owner, migration impact, and a narrow failure signal that tests
can detect. Ergonomic review should measure useful logic versus repetitive
plumbing and assess whether people can explain the resulting ownership and
authority boundaries; fewer lines alone do not prove improvement.

Performance claims need named input sizes, host/tool/profile identity, elapsed
time, and peak or working-set memory where practical. Track warm and cold
compiler feedback, code size, and runtime behavior separately. Do not assume
typed structures or fewer source lines will be faster without measurements.

Ordinary local verification keeps the repository's ten-minute budget and
change-aware plan review. Full cross-host qualification is a later explicitly
selected gate, not a prerequisite for recording or reviewing these proposals.

## Next planning checkpoint

Review the [ideas register](Windvale-2.0-Ideas.md), especially the borrowing,
effect-composition, localization, and independent-release alternatives, and the
[compiler/tooling findings](Windvale-2.0-Compiler-And-Tools.md). Select the first
real programs and focused measurements, using the
[testing review](Windvale-2.0-Qualification-And-Testing.md) and
[remaining-stack findings](Windvale-2.0-Stack-And-Applications.md) to check
cross-component costs and current-contract concerns, before creating normative 2.0
specifications or accepting an implementation plan. No implementation is needed
merely to make this plan look complete.
