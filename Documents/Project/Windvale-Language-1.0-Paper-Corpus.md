# Windvale Language 1.0 paper design corpus

## Status

This document defines the mandatory pre-freeze usability and semantic evidence
for the Language 1.0 candidate accepted under
[Decision 0751](../Decisions/0751-Accept-Windvale-Language-1.0-Direction.md)
and refined by
[Decision 0752](../Decisions/0752-Complete-Language-1.0-Collection-And-Package-Data-Boundaries.md)
and
[Decision 0753](../Decisions/0753-Require-Language-1.0-AI-Accelerator-Evidence.md),
with the first reviewed bundle's findings resolved by
[Decision 0754](../Decisions/0754-Resolve-First-Language-1.0-Paper-Findings.md)
and the command workload findings resolved by
[Decision 0755](../Decisions/0755-Resolve-Language-1.0-Command-Workload-Findings.md)
and the file-copy workload findings resolved by
[Decision 0756](../Decisions/0756-Resolve-Language-1.0-File-Copy-Findings.md)
and the database-transaction findings resolved by
[Decision 0757](../Decisions/0757-Resolve-Language-1.0-Database-Transaction-Findings.md)
and the compiler-front-end findings resolved by
[Decision 0758](../Decisions/0758-Resolve-Language-1.0-Compiler-Front-End-Findings.md)
and the HTTP-handler findings resolved by
[Decision 0759](../Decisions/0759-Resolve-Language-1.0-Http-Handler-Findings.md),
and the concurrent-service findings resolved by
[Decision 0760](../Decisions/0760-Resolve-Language-1.0-Concurrent-Service-Findings.md),
and the retained-GUI findings resolved by
[Decision 0761](../Decisions/0761-Resolve-Language-1.0-Retained-Gui-Findings.md),
and the numeric/graphics findings resolved by
[Decision 0762](../Decisions/0762-Resolve-Language-1.0-Numeric-Graphics-Findings.md),
the package-parser findings by
[Decision 0763](../Decisions/0763-Resolve-Language-1.0-Package-Parser-Findings.md),
the System/FFI findings by
[Decision 0764](../Decisions/0764-Resolve-Language-1.0-System-Ffi-Findings.md),
and complete-suite reconciliation by
[Decision 0765](../Decisions/0765-Complete-Language-1.0-Source-Freeze-Candidate.md).
The complementary five localization workload findings are accepted by
[Decision 0766](../Decisions/0766-Complete-Language-1.0-Localized-Source-Reconciliation.md)
and are inventoried by the
[localization workload plan](Windvale-Language-1.0-Localization-Workloads.md).
All eleven source bundles are owner reviewed against the
[semantic specification](../../Specifications/Windvale-Language-1.0.md),
[grammar](../../Specifications/Windvale-Language-1.0-Grammar.md), its
[machine projection](../../Specifications/Windvale-Language-1.0.ebnf), the
[Foundation contract](../../Specifications/Windvale-Language-1.0-Foundation.md),
and its
[signature registry](../../Specifications/Windvale-Language-1.0-Foundation-Registry.md).
The explicit source-freeze decision remains pending.

Paper source is design evidence, not an implementation claim. Current tools
continue to accept Windvale Seed.

## Purpose

Small grammar examples can prove parsing while hiding whether a complete program
is usable. The corpus tests whether Language 1.0 can express real work without:

- accidental verbosity replacing missing abstractions;
- hidden allocation, authority, failure, capture, or cleanup;
- unbounded collections, recursion, tasks, queues, diagnostics, or generated
  code;
- packed bytes or giant flat records standing in for typed state;
- ownership rules that ordinary diagnostics cannot explain;
- two competing error, object, collection, or concurrency models; or
- target-specific behavior entering portable semantics.

The corpus may change candidate syntax or semantics. It cannot waive a bound or
invent an implementation-only exception.

## Required bundle contents

Each workload produces one reviewable bundle containing:

1. complete candidate edition-1 source modules;
2. a package/build-plan sketch with exact module mapping;
3. platform, profile, authority, and required/optional capability metadata;
4. all input, collection, memory, recursion, task, queue, output, and diagnostic
   maxima;
5. a value and ownership inventory;
6. a capability/effect closure;
7. every recoverable failure family and terminal-trap boundary;
8. a step-by-step cleanup and cancellation walkthrough;
9. at least five rejected or boundary source cases;
10. expected semantic outputs independent of one backend;
11. an implementation responsibility map;
12. every package-data binding, maximum, digest, type, retained-byte charge, and
    distinct shipped content object where applicable; and
13. reviewer findings, revisions, and acceptance status.

Source bundles live under one future owned corpus directory only when their
content is written; this plan does not add empty scaffolding.

## Common review questions

Every reviewer answers:

- Can a reader identify every mutation and unique owner?
- Can a reader tell when ownership moves and when access is borrowed?
- Is every allocation bounded before growth?
- Does every capability requirement remain visible in module and function
  contracts?
- Can any return, `try`, break, cancellation, or provider failure bypass local
  release?
- Can a failed operation leave ownership or external mutation indeterminate
  without saying so?
- Is evaluation and iteration order exact?
- Can the same source retain semantics on Windows, Linux, Windvale OS, and an
  admitted WebAssembly target?
- Are error adapters explicit but not painfully repetitive?
- Are named records, variants, and arguments readable at realistic sizes?
- Does a compiler diagnostic have enough local evidence to explain each rejected
  ownership, effect, bound, or profile case?
- Would a simpler language/library rule express the same program more clearly?

## Workload 1: command-line application

### Scenario

Parse bounded arguments, select an operation, read input through an approved
capability, produce diagnostics, write exact output, and return a process status.

### Required language pressure

- edition/module/profile metadata;
- named arguments and configuration records;
- strict numeric parsing;
- Option, Result, `try`, and domain variants;
- text and byte builders;
- one bounded immutable package-data usage template with no filesystem grant;
- explicit output capability;
- no ambient environment or locale; and
- deterministic exit-status mapping.

### Boundary cases

Include no arguments, unknown option, repeated option, numeric overflow, maximum
argument bytes, malformed UTF-8 at a provider boundary, output rejection, and
indeterminate output progress.

### Acceptance

The successful path must remain easy to read without hiding parse or output
failure. Adding one option must not change unrelated call meaning through
overload resolution.

The complete first-author source and evidence are in the
[workload 1 paper bundle](Language-1.0-Paper-Corpus/01-Command-Line-Application/README.md).
It expresses one bounded strict UTF-8 byte/rune inspector. Its owner review
accepts the sequence, numeric parsing, reserved-builder, stream-authority, and
command-status recommendations. The general Foundation calls and command profile
are normative-candidate contracts. The paper stream interface remains a fixture
contract; its production capability signature-set identity belongs to the later
library/provider implementation gate and does not block source-language freeze.

## Workload 2: bounded file copy

### Scenario

Acquire source and destination instances, copy a bounded number of bytes in
chunks, explicitly finish durable output, and release both handles on every
ordinary path.

### Required language pressure

- required filesystem capability and rights-reduced instances;
- move-only resources and `using`;
- borrowed mutable buffer slices;
- exact partial read/write progress;
- explicit maximum input, output, chunk, and work;
- typed completion versus local release; and
- cancellation and provider loss as distinct outcomes.

### Boundary cases

Include empty file, exact maximum, source growth, destination full, zero-progress
provider defect, partial write, indeterminate mutation, finish failure, early
`try` propagation, and provider restart.

### Acceptance

No path may leak a resource or retry an indeterminate write. A body failure and
finish failure cannot be silently collapsed or discarded.

The complete source and evidence are in the
[workload 2 paper bundle](Language-1.0-Paper-Corpus/02-Bounded-File-Copy/README.md).
Its owner review accepts the fixed byte-buffer, release/completion,
known-partial-progress, independent-authority, and synchronous-cancellation
findings. The general Foundation and language clarifications are
normative-candidate contracts. The paper filesystem interface remains a fixture
contract; its production capability signature-set identity belongs to the later
library/provider implementation gate and does not block source-language freeze.

## Workload 3: database transaction

### Scenario

Parse a typed row, perform bounded lookup and update, commit one transaction,
and report rejection, conflict, provider failure, or indeterminate commit.

### Required language pressure

- records, multi-field variants, maps, arenas, and generation-checked handles;
- explicit schemas and no reflection serialization;
- owned transaction resource;
- named error-domain adapters;
- fallible commit separated from release; and
- deterministic map and query iteration.

### Boundary cases

Include missing row, duplicate key, invalid schema value, maximum row bytes,
arena exhaustion, stale handle, conflict, commit rejection, commit uncertainty,
and reopen/recovery evidence.

### Acceptance

Typed state must be clearer than packed bytes. Transaction ownership and uncertain
commit behavior must be evident without knowing provider internals.

The complete source and evidence are in the
[workload 3 paper bundle](Language-1.0-Paper-Corpus/03-Database-Transaction/README.md).
Its owner review accepts runtime-bounded typed arenas, ownership-preserving
first-item collection construction, two-step checked borrowed observation,
explicit typed schema adapters, and explicit commit plus fresh-session recovery.
Decision 0758 later makes the map half of that observation explicitly
rank-based so the direct borrow has only the map as lifetime owner. The general
Foundation clarifications are normative-candidate contracts. The paper database
and schema interfaces remain fixture contracts; production signature-set
identities belong to the later library/provider implementation gate and do not
block source-language freeze.

## Workload 4: compiler front end

### Scenario

Decode strict source, lex tokens, parse recursive syntax, bind names, emit bounded
diagnostics, and construct deterministic byte output.

### Required language pressure

- text, runes, byte/rune/source positions;
- vectors, sequences, slices, builders, maps, and typed arenas;
- recursive variants and handles;
- generics and protocols;
- value-producing match and destructuring;
- explicit recursion/work/diagnostic limits; and
- immutable publication between phases.

### Boundary cases

Include invalid UTF-8, macron lookalikes, truncated literals, maximum nesting,
generic-instantiation limit, duplicate symbol, stale arena handle, diagnostic
cascade limit, and output-capacity rejection.

### Acceptance

The design must materially improve on packed offsets, wide flat records, manual
status propagation, and repeated concatenation without producing unreviewable
ownership annotations.

The complete source and evidence are in the
[workload 4 paper bundle](Language-1.0-Paper-Corpus/04-Compiler-Front-End/README.md).
Its owner review accepts full-arity named explicit generic calls, empty bounded
collection construction, rank-based one-owner map borrows, immutable typed-arena
publication, exact UTF-8/scalar source positions, deterministic diagnostic
saturation, and exact integer byte-builder appends. These are
normative-candidate contracts and do not claim that Windvale Seed implements
them.

## Workload 5: HTTP request handler

### Scenario

Accept one bounded request from a bound stream provider, parse method, target,
headers, and body, route it, and write a bounded response.

### Required language pressure

- hosted capabilities and owned stream instances;
- deterministic ordered maps;
- slices into input buffers;
- explicit text/bytes decoding;
- exact partial and indeterminate network writes;
- deadlines and cancellation;
- builders and formatting; and
- no ambient socket, TLS, clock, or entropy access.

### Boundary cases

Include oversized start line, header count/bytes limit, duplicate header policy,
invalid UTF-8 where text is required, early peer close, timeout, cancellation,
partial response acceptance, and provider restart.

### Acceptance

Transport details may be hidden behind semantic capabilities, but authority,
limits, mutation progress, and response ownership must remain visible.

The complete source and evidence are in the
[workload 5 paper bundle](Language-1.0-Paper-Corpus/05-Http-Request-Handler/README.md).
Its owner review accepts checked slice observation and immutable byte-range
borrowing, strict UTF-8 decode directly from a byte slice, invariant decimal
byte-builder append, one opaque launcher-supplied operation context, and exact
reliable-stream progress/no-replay meanings. These are normative-candidate
contracts. Decision 0760 completes their async provider spelling,
generation-bound endpoint, and task/cancellation integration.

## Workload 6: concurrent hosted service

### Scenario

Serve multiple bounded requests inside one lexical task scope, collect results in
creation order, cancel remaining work after a policy trigger, and survive one
provider restart.

### Required language pressure

- explicit function effects and closure captures;
- copy, move, immutable borrow, and mutable borrow rejection;
- task count, queue, memory, work, timer, and diagnostic limits;
- typed task handles and await;
- deterministic join policy;
- cancellation observation points; and
- trap containment distinct from recoverable failure.

### Boundary cases

Include spawn rejection before capture acceptance, task failure after capture,
scope cancellation, deadline, provider loss, one trapped child, full completion
queue, mutable borrow across await, and attempted detach.

### Acceptance

No child may outlive the scope or retain invalid authority. Scheduler choice may
change timing but not ownership, result order, or cleanup semantics.

The draft-reviewed source and evidence are in the
[workload 6 paper bundle](Language-1.0-Paper-Corpus/06-Concurrent-Hosted-Service/README.md).
Its seven accepted findings reconcile task construction with the operation
context, add an explicit cooperative cancellation request, distinguish task
runtime generations from child provider failures, make blocking provider calls
explicitly asynchronous, bind shared accepts to a typed endpoint generation,
clarify valid awaited borrows versus invalid captures, and avoid premature task
collection/detach syntax. The project owner accepted them under Decision 0760;
they remain normative-candidate inputs rather than implementation or final
source-freeze claims.

## Workload 7: GUI or retained application state

### Scenario

Maintain bounded retained state, process events, publish immutable render state,
and run background work through a lexical task scope.

### Required language pressure

- variants for events;
- bounded package data for one immutable icon, theme, or layout input;
- owned mutable state and immutable publication;
- maps or arenas for retained nodes;
- generation-checked handles;
- closures with explicit capture;
- task results applied on the owning event path; and
- explicit display, timer, and input capabilities.

### Boundary cases

Include stale widget handle, event-queue maximum, cancelled background result,
provider restart, render-state publication failure, duplicate identity, and
attempted mutable alias.

### Acceptance

The program must not require classes, tracing-GC cycles, implicit UI-thread
globals, or detached tasks to remain readable.

The complete draft-reviewed source and evidence are in the
[workload 7 paper bundle](Language-1.0-Paper-Corpus/07-Gui-Retained-State/README.md).
Its seven accepted findings complete typed-arena replacement/removal, use closed
semantic event variants, preserve a Core/Hosted and three-capability boundary,
apply copied background results only on the owning path, accept explicit stable
identity tombstones, require exact immutable-frame publication outcomes, and
retain exact package bytes for the first small theme instead of inventing schema
or reflection syntax.

## Workload 8: numeric or graphics processing

### Scenario

Transform a bounded numeric buffer with strict `f32`/`f64` operations, explicit
integer conversions, and deterministic output formatting.

### Required language pressure

- fixed arrays, vectors, slices, and generic algorithms;
- strict rounding and explicit fused operations;
- NaN, infinity, signed zero, and subnormal behavior;
- explicit widening, narrowing, and rounded conversion;
- no operator overloading; and
- bounded parallelism only if the sequential result remains exact.

### Boundary cases

Include canonical NaN, positive/negative zero, smallest subnormal, overflow to
infinity, inexact integer conversion, mis-sized buffer, capacity failure, and
cross-target bit comparison.

### Acceptance

The source must be practical for numeric work without silently enabling
fast-math or target-dependent results. Named domain operations must remain
readable without unrestricted operator overloading.

The complete draft-reviewed source and evidence are in the
[workload 8 paper bundle](Language-1.0-Paper-Corpus/08-Numeric-Graphics-Processing/README.md).
Its six accepted findings add contextual fixed-array literals, complete checked
array/vector slice creation and mutable replacement, fix the strict f32
operation/observation subset, retain policy-bearing conversions and the complete
generated-matrix freeze gate, require canonical bounded numeric formatting, and
keep parallel execution bit-identical to the sequential oracle without adding
parallel syntax or implicit fast math.

## Workload 9: package parser and deterministic map

### Scenario

Parse a bounded package manifest and lock, validate identities, build a
deterministic dependency map, and serialize one canonical report.

### Required language pressure

- raw, multiline, byte, and text literals;
- numeric and text parsing;
- ordered maps, ordered sets, sequences, and builders;
- package-data manifest entries with duplicate-content deduplication;
- explicit format versions and no reflection serialization;
- deterministic generic/protocol selection; and
- bounded cycle diagnostics.

### Boundary cases

Include duplicate key, unknown field, invalid identity, dependency cycle,
maximum package count, malicious length, ordering equality conflict, and output
limit.

### Acceptance

Input order and map implementation cannot change canonical output. Rejection must
occur before expensive allocation for oversized declared values.

The complete draft-reviewed source and evidence are in the
[workload 9 paper bundle](Language-1.0-Paper-Corpus/09-Package-Parser-Deterministic-Map/README.md).
Its six accepted findings complete map mutation/publication and immutable
observation, fix the complete ordered-set API, make Ordering equality/laws
testable, retain explicit bounded parsing instead of reflection, define
unobservable per-domain package-content deduplication/accounting, and use
ordered-rank topology plus sorted cycle evidence as the deterministic oracle.

## Workload 10: system and FFI boundary

### Scenario

Call one named foreign or machine interface through a small audited System
module, validate an untrusted buffer, and publish a safe Core or Hosted value.

### Required language pressure

- System profile and exact platform/ABI scope;
- unsafe declaration and unsafe invocation block;
- opaque raw address or foreign pointer;
- alignment, range, lifetime, and alias validation;
- checked address arithmetic;
- foreign error and unwind translation; and
- no capability escalation through unsafe code.

### Boundary cases

Include null or invalid foreign pointer where the ABI permits representation,
misalignment, truncated range, arithmetic overflow, invalid enum/Boolean,
foreign failure, forbidden unwind, stale generation, and unsupported target.

### Acceptance

Unsafe code must remain small, visible, target-scoped, and unable to leak an
unchecked pointer or host layout into safe portable source.

The complete draft-reviewed source and evidence are in the
[workload 10 paper bundle](Language-1.0-Paper-Corpus/10-System-Ffi-Boundary/README.md).
Its seven accepted findings add the first concrete System ABI target predicate,
make the foreign ABI literal a registered complete contract, distinguish
nullable/non-null opaque pointers, fix caller-owned aligned scratch/write-region
operations, separate recoverable untrusted data from terminal ABI violations,
retain explicit status/unwind translation, and publish only independently safe
Core values without granting authority.

## Workload 11: local AI accelerator inference

### Scenario

Load a small versioned model and quantized weights from immutable package data,
run one bounded inference through an explicitly bound accelerator provider, and
compare its result with a strict software reference. The complete design pressure
and review questions are owned by the
[accelerator compute and AI design](Windvale-Accelerator-Compute-And-AI-Design.md).
The complete first-author source and evidence are in the
[workload 11 paper bundle](Language-1.0-Paper-Corpus/11-Local-AI-Accelerator-Inference/README.md).
Its project-owner review accepts five general Language/Foundation clarifications
without freezing its paper-only accelerator API or claiming implementation.

### Required language pressure

- package-backed tokenizer/model metadata and quantized weights;
- explicit accelerator capabilities, provider generation, attachment mode,
  supported feature set, and device-memory budget;
- bounded tensors with exact shape, stride, layout, view, alias, and
  quantization rules;
- nominal packed sub-byte formats without general sub-byte core integers;
- mixed-precision matrix/tensor multiplication with named accumulation and
  tolerance behavior;
- asynchronous upload, dispatch, completion, cancellation, and provider loss;
- move-owned host/device resources and borrowed tensor views; and
- one small target-scoped custom Windvale kernel through the shared compiler
  architecture, paired with a software/reference equivalent.

### Boundary cases

Include malformed metadata, digest mismatch, oversized weights, shape-product
overflow, invalid stride or view range, incompatible quantization grouping,
unsupported format or numeric mode, insufficient device memory, stale provider
generation, invalid kernel, cancellation, provider loss, and diagnostic-budget
exhaustion.

### Acceptance

Host/framework source must use ordinary candidate Language 1.0 without hidden
allocation, authority, device handles, native pointers, ambient fast math, or
unbounded queues. Sub-byte storage must have exact format semantics without new
general scalar primitives. The custom kernel remains an explicitly target-scoped
Windvale module compiled through the shared compiler, every resource has a
bounded terminal release path, and the software path supplies a deterministic
correctness oracle under one exact comparison contract.

## Quantitative review record

Each workload records:

| Measure | Required record |
| --- | --- |
| Source size | Modules, lines, declarations, and maximum function/record width. |
| Explicitness | Moves, borrows, adapters, limits, capabilities, and unsafe blocks. |
| Resources | Maximum retained bytes, collections, tasks, queues, recursion, and diagnostics. |
| Failure surface | Result families, adapters, partial/indeterminate outcomes, and traps. |
| Compiler planning | Expected generic instances, WIR blocks/operations, and retained evidence. |
| Artifacts | Expected WVB/native changes or proof that existing lowering suffices. |
| Usability | Reviewer confusion points, repetitive forms, and proposed simplifications. |

No universal line-count target decides usability. Repetition is evidence only
when it represents missing semantics rather than intentionally visible authority,
failure, ownership, or bounds.

## Freeze gates by high-risk decision

### Collections and budgets

Pass when workloads 2, 3, 4, 5, 7, 8, and 9 express growth through understandable
maxima or surrounding budgets without one-off collection types or hidden
allocation.

### Ownership and borrowing

Pass when workloads 2, 3, 4, 6, 7, 10, and 11 have exact moves, borrows,
publication, handles, and diagnostics; no valid program requires an unsafe
escape merely to express ordinary ownership.

### Resource completion

Pass when workloads 2, 3, 5, 6, and 11 prove every body, completion,
cancellation, provider-loss, and release path without discarding a result or
double-releasing.

### Structured concurrency

Pass when workloads 5, 6, 7, and 11 prove lexical scope, capture acceptance,
join order, cancellation observation, provider restart or loss, trap
containment, and finite queues on both sequential and parallel-capable scheduler
models.

### Package data

Pass when workloads 1, 7, 9, and 11 prove exact binding, maximum-size admission,
strict text validation, imported access, resource-domain charging, missing and
malformed rejection, no filesystem authority, and one shipped payload per
distinct content identity.

### Accelerator and AI boundary

Pass when workload 11 separates ordinary host-language needs from accelerator
library, capability, target-extension, verifier, WIR, and provider needs; proves
exact packed/quantized and mixed-precision behavior; retains one compiler
architecture; and has bounded memory, queue, cancellation, provider-loss,
numeric-comparison, and teardown evidence. Every unresolved accelerator-only
detail must have a named future owner without leaving the general Language 1.0
source contract ambiguous.

## Corpus status

| Workload | Status |
| --- | --- |
| Command-line application | Draft reviewed; five findings accepted and resolved |
| Bounded file copy | Draft reviewed; five findings accepted and resolved |
| Database transaction | Draft reviewed; five findings accepted and resolved |
| Compiler front end | Draft reviewed; six findings accepted and resolved |
| HTTP request handler | Draft reviewed; five findings accepted and resolved |
| Concurrent hosted service | Draft reviewed; seven findings accepted and resolved |
| GUI retained state | Draft reviewed; seven findings accepted and resolved |
| Numeric or graphics processing | Draft reviewed; six findings accepted and resolved |
| Package parser | Draft reviewed; six findings accepted and resolved |
| System and FFI boundary | Draft reviewed; seven findings accepted and resolved |
| Local AI accelerator inference | Draft reviewed; five general findings accepted and resolved |

All eleven application/system workloads and all five localization workloads
have owner-reviewed paper findings. Complete-suite reconciliation is
recorded in the
[source-freeze review packet](Windvale-Language-1.0-Source-Freeze-Review.md),
including the candidate grammar, Foundation, localization, document, and corpus
identities. Edition 1 is not frozen until the explicit owner decision.

The source-freeze decision cannot accept a workload row with a pending finding.
All current rows pass owner paper review; when the explicit freeze decision
accepts their candidate identities, their source becomes the first Language 1.0
conformance and migration input rather than disposable prose.
