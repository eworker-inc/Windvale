# Windvale Language 1.0 paper design corpus

## Status

This document defines the mandatory pre-freeze usability and semantic evidence
for the Language 1.0 candidate accepted under
[Decision 0751](../Decisions/0751-Accept-Windvale-Language-1.0-Direction.md).
The corpus is not complete yet. Its ten source bundles must be written and
reviewed against the
[semantic specification](../../Specifications/Windvale-Language-1.0.md),
[grammar](../../Specifications/Windvale-Language-1.0-Grammar.md), and
[Foundation contract](../../Specifications/Windvale-Language-1.0-Foundation.md)
before the source-freeze decision.

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
11. an implementation responsibility map; and
12. reviewer findings, revisions, and acceptance status.

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

## Workload 7: GUI or retained application state

### Scenario

Maintain bounded retained state, process events, publish immutable render state,
and run background work through a lexical task scope.

### Required language pressure

- variants for events;
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

## Workload 9: package parser and deterministic map

### Scenario

Parse a bounded package manifest and lock, validate identities, build a
deterministic dependency map, and serialize one canonical report.

### Required language pressure

- raw, multiline, byte, and text literals;
- numeric and text parsing;
- ordered maps, sequences, and builders;
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

Pass when workloads 2, 3, 4, 6, 7, and 10 have exact moves, borrows, publication,
handles, and diagnostics; no valid program requires an unsafe escape merely to
express ordinary ownership.

### Resource completion

Pass when workloads 2, 3, 5, and 6 prove every body, completion, cancellation,
provider-loss, and release path without discarding a result or double-releasing.

### Structured concurrency

Pass when workloads 5, 6, and 7 prove lexical scope, capture acceptance, join
order, cancellation observation, provider restart, trap containment, and finite
queues on both sequential and parallel-capable scheduler models.

## Corpus status

| Workload | Status |
| --- | --- |
| Command-line application | Pending source bundle |
| Bounded file copy | Pending source bundle |
| Database transaction | Pending source bundle |
| Compiler front end | Pending source bundle |
| HTTP request handler | Pending source bundle |
| Concurrent hosted service | Pending source bundle |
| GUI retained state | Pending source bundle |
| Numeric or graphics processing | Pending source bundle |
| Package parser | Pending source bundle |
| System and FFI boundary | Pending source bundle |

The source-freeze decision cannot mark a pending row accepted. When all rows pass,
their source becomes the first Language 1.0 conformance and migration input rather
than disposable prose.
