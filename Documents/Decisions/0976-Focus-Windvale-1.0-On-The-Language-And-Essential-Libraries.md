# Decision 0976: Focus Windvale 1.0 on the language and essential libraries

## Status

Accepted maintainer direction, 3 October 2026. Implementation, toolchain
promotion, qualification and release remain incomplete.

Amends the required product bundle in
[Target Windvale 1.0 directly](0800-Target-Windvale-1.0-Directly.md).
The intended `v1.0.0` target and the accepted Language 1.0 source contract remain.

## Problem and result

The maintainer wants a usable programming language, compiler and libraries
before further database or application work. The previous product bundle made
production WVDB, a complete Backend profile and database service operations
prerequisites for the next release. That dependency obscured the language's
delivery path and encouraged preserving older implementations for secondary
consumers.

The next delivery is one coherent Windvale toolchain and essential library set
on Windows and Debian/Linux. Windvale's compiler and development tools are its
primary maintained consumers. WVDB, secondary applications, broader web/service
profiles, Windvale OS and 2.0 proposals are outside this delivery's critical path.

## Required delivery

1. Keep the accepted source semantics and versioned execution contracts. Finish
   ownership, release, allocation accounting and runtime working storage through
   the existing compiler and shared runtime; a larger arena does not close this
   requirement.
2. Select one ordinary build, verify, run, inspect, assemble, link and package
   path. Explicitly prepare the necessary tools, reuse products by complete
   input identity, and report cold cost separately from ordinary feedback.
3. Complete essential value, numeric/ordering, memory, collection, bytes/text
   and resource APIs needed for ordinary programs and the toolchain. Supply the
   explicit host file, directory and publication boundaries needed to build
   programs. The existing completion matrix owns accepted declarations and
   remaining API choices; draft catalogs do not become accepted through this
   scope decision.
4. Migrate compiler parsing, symbol/state collections and output construction
   onto those maintained libraries as their native support becomes available.
   Demonstrate bounded fixed-live-state memory, alias survival, allocation
   refusal, failure cleanup and reusable storage on both hosts.
5. Demonstrate deterministic self-host reconstruction and clean-install use of
   the selected generation. Qualify its exact declared API and target matrix
   before promotion or a released 1.0 claim. Include packaging, compatibility,
   recovery, provenance and signed distribution for the shipped toolchain.
6. Retire superseded active implementations and compatibility paths after
   proving that the supported toolchain no longer needs them. Preserve immutable
   published, qualified and recovery evidence without treating it as current
   implementation guidance.

The frozen Foundation registry remains authoritative. Selecting and completing
the essential delivery profile does not establish that every proposed library
family, hosted profile or target is complete. The exact release matrix must name
every included and excluded promise; no passing subset silently weakens an
accepted source rule.

## Consumer and bootstrap policy

Secondary applications may break when their obsolete dependency or API is
removed. Their later migration is separate work, rather than a requirement to
keep a second compiler or memory model. Package functionality required to build,
authenticate or distribute the toolchain remains maintained; the typed
Package-Lock refactor is no longer the mandatory memory-management consumer.

Required pinned tools and the recorded source-edition predecessor remain
construction inputs until a successor reconstructs the supported toolchain
without them. Their names or age alone are not evidence that they can be
deleted. Managed Stage 0 remains recovery-only under its existing policy.

Host scripts and Node-based orchestration may remain as explicit development
dependencies while needed. Source semantics, compilation, verification and
runtime behavior stay with their Windvale owners. Replacing orchestration must
follow demonstrated Windvale library support and a measured delivery benefit.

## Deferred product work

Production WVDB, full Data/Backend catalogs, network/TLS/HTTP services, privileged
service installation, optional applications and the 2027/2.0 proposals retain
their own contracts and evidence. They no longer block this toolchain delivery.
No new implementation in those lanes is required by this decision.

The [product plan](../Project/Windvale-1.0-Product-Plan.md),
[roadmap](../Project/Roadmap.md) and
[completion plan](../Project/Compiler-Tools-And-Libraries-Completion-Plan.md)
own the active gates and retirement map. This decision does not publish a
release, rename immutable artifacts or claim that an implemented candidate is
qualified.
