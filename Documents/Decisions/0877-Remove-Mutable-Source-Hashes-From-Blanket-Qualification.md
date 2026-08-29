# Decision 0877: remove mutable-source hashes from blanket qualification

## Status

Accepted implementation correction on 2026-08-29. Final paired-host evidence
remains pending.

## Context

The first post-convergence Qualification run exposed three orchestration
problems rather than a split-compiler defect. Bootstrap jobs invoked their new
Node coordinator without installing Node. The WebAssembly jobs launched a cold
historical reconstruction that compiled current Language 1.0 fixtures through
frozen Seed and attempted to regenerate an untracked compiler workload. The
Seed front-door reconstruction owner encoded 185 exact assertions across 105
products built from mutable current source; its first hash was already stale.

The run was cancelled after those failures. A database owner that ended at 45
seconds was terminated by that workflow cancellation; it did not exceed an
owner timeout and its bounds are unchanged.

## Decision

1. Install pinned Node.js 24 in both independent bootstrap jobs and require the
   workflow policy verifier to enforce the Node pin for every qualification
   job.
2. Keep the two WebAssembly qualification jobs independent and cross-host, but
   run the digest-pinned playground package and executable compiler-core
   checkpoint. Do not rebuild mutable current compiler source as an incidental
   WebAssembly fixture.
3. Leave full current-source WebAssembly reconstruction as an explicit
   promotion boundary. Before it can make a current claim, migrate it from
   frozen `Build-Wvb` Seed compilation to the current split compiler and bind
   any promoted workloads as retained artifacts.
4. Retire `seed-native-front-door-reconstruction`. Do not repin its mutable
   source hashes. Preserve immutable front-door admission, the canonical Seed
   console AOT chain, focused assembler/object/linker/runtime owners, and exact
   current split convergence as distinct evidence.
5. Keep deletion tombstones in the changed-file planner for the retired
   launchers and advance the digest-bound registry from 115 owners / 5,617
   cases to 114 owners / 5,616 cases.

## Consequences

- Seed remains immutable and no longer masquerades as a compiler for current
  Language 1.0 fixtures.
- Complete qualification stops spending roughly twelve minutes per host on a
  duplicate Seed hash farm and avoids a roughly 27-minute WebAssembly rebuild
  unrelated to the promoted package contract.
- The WebAssembly jobs still execute real compiler behavior on Windows and
  Linux; they make a pinned-package claim, not a current-source rebuild claim.
- Current compiler convergence remains cold and exact in its own two host jobs.
- A future WebAssembly promotion must deliberately construct and retain its
  current split-compiler inputs instead of inheriting mutable repository state.

## Local evidence

On the settled Windows tree, the replacement WebAssembly checkpoint passed in
24.688 seconds: package identity took 9.592 seconds and executable compiler-core
behavior took 14.193 seconds. It compiled and executed the exact scalar and
standard-output programs, returning `42` and `Hello from Windvale`.

The one change-aware Development gate passed the 31 general and 205 native
planner cases, the pinned six-job workflow policy, four verification-owner
stream cases, and all 482 Language 1.0 front-door cases. The front-door owner
completed in 816.520 seconds and reproduced the accepted analyzer and emitter
identities. This is local development evidence; the corrected paired-host
Qualification run remains required.

## Reconsideration triggers

Add a full WebAssembly reconstruction job back to blanket qualification only
when it consumes retained inputs or constructs current inputs through the split
compiler, has bounded dual-host measurements, and proves evidence not already
owned by changed-file conformance or the pinned package checkpoint. Restore a
Seed reconstruction owner only for a named immutable release artifact family,
never for hashes derived from mutable current source.
