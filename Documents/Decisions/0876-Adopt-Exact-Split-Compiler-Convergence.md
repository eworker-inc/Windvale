# Decision 0876: adopt exact split compiler convergence

## Status

Accepted implementation direction on 2026-08-29. Current-host fixed-point and
verifier evidence is complete; final paired Windows/Linux Slice 7 Qualification
remains pending.

## Context

The final Slice 7 Qualification gate still invoked a bootstrap path pinned to a
649-byte monolithic compiler manifest. The current manifest is 1,712 bytes and
the compiler-scale source closure now exceeds the old route's ownership and
resource model. Repinning the manifest did not repair the design: the complete
2.82 MiB monolithic source set failed before producing semantic outputs, while
the accepted analyzer and emitter closures already completed within explicit
bounds.

The current compiler had therefore advanced, but qualification was repeatedly
reconstructing an obsolete compiler shape and large native containers. The
checked-in compiler-aligned verifier was also source-behind: it rejected current
WVB products that the current verifier source accepts.

## Decision

1. Define current compiler convergence as two exact fixed points: the analyzer
   and emitter. Their split is a phase/resource boundary inside one compiler.
2. Keep the promoted Seed and managed recovery release immutable. Use the three
   target-aware bootstrap WVB products to construct the current pair; do not
   migrate Seed internals merely to advance ordinary source work.
3. Build Stage 1 analyzer and emitter products, package them for the current
   host, rebuild both products with that pair, and require exact Stage 1/Stage 2
   byte equality.
4. Build the compiler-aligned verifier from the same current pair, package it
   independently, and use it to admit both Stage 2 products. A stale retained
   verifier cannot define current acceptance.
5. Run qualification with a private empty cache, bounded products and
   diagnostics, per-child timeouts, visible phase progress, and guarded cleanup.
6. Retire the monolithic bootstrap, source-set, and reconstruction launchers
   from `main`. Retain the old six-artifact candidate only as historical,
   differential, and fixed WebAssembly stress evidence.
7. Retire the Seed source-compiler `tool` selector. Keep only its still-exact
   `core` and `demo` products; build the current compiler through the split
   Project 2 route.
8. Keep WebAssembly verification focused by consuming its exact retained
   compiler workload directly instead of rebuilding a current compiler first.

## Evidence

The current analyzer converges at 1,515,372 bytes and SHA-256
`9876f178f4ac06872a44f44085de5d72f17777abf462985300f6e453e4b625d9`.
The current emitter converges at 1,523,605 bytes and SHA-256
`a0beb624dcc225b0ccdac848d808af1faef63cdb66eb650faf0bb9216e0815c9`.
Both second-generation products reproduced those exact identities in a cold
Windows run.

The retained 1,255,936-byte Windows verifier rejected both products. Rebuilding
the verifier from current source produced a 399,387-byte WVB at SHA-256
`7da624b070b69c3a720a00df12b753ed28276b7909c48ec5e6c349bd15ed9800`;
its independently packaged Windows application accepted both exact products.
The complete paired-host qualification remains required before this evidence is
called cross-host or closes Slice 7.

## Consequences

- Qualification follows the compiler architecture that current development
  actually uses instead of maintaining a stale monolith.
- Seed remains one stable oracle rather than another evolving compiler codebase.
- A source-current verifier catches format/semantic drift without making old
  release tools define current behavior.
- WebAssembly and reconstruction owners stop repeating unrelated compiler and
  large-container work.
- The cold bootstrap remains intentionally expensive; later work may reuse
  independently admitted fixed container segments without weakening exact
  current-product generation and comparison.

## Reconsideration triggers

Reconsider the two-product boundary if analysis and emission later share a
smaller admitted representation with equal or better ownership and memory
evidence. Reconsider cold container construction when a content-addressed
qualification cache can prove exact tool, runtime, service, host, and source
identity without accepting ambient development state. Do not restore a
monolithic compiler solely to preserve historical artifact shape.
