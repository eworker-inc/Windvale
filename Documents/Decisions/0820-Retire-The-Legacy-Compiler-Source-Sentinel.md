# Decision 0820: Retire the legacy compiler-source sentinel

- Status: Accepted
- Date: 2026-08-21

## Context

The five-case `compiler-source-sentinel` reconstructed a partial monolithic
source compiler through `Compile-Compiler-Source-Set`. That recovery-oriented
source list predates the general generic binding, layout, materialization, and
WVB components. It is no longer the Windvale 1.0 compiler closure.

Decision 0810 made the current analyzer and emitter the supported
compiler-scale development boundary. The Language 1.0 front-door owner already
reconstructs those exact products, compiles deterministic fixtures twice,
verifies the emitted WVB, and executes it. Keeping the older sentinel repeated
those checks through a second compiler path and made recovery capacity an
ordinary development gate again.

Slice 4 exposed the mismatch directly. The partial monolithic product compiled
and independently verified its `Function-Only.wv` output, but execution returned
`Sourceˉwir` at source bindings, function zero, operation zero. The current
split owner passed all 350 registered cases, including compiler reconstruction,
determinism, verification, execution, and the six Foundation generic cases.

## Decision

1. Remove `compiler-source-sentinel` from the native verification-owner
   registry and delete its Windows and Linux owner scripts.
2. Route source-compiler implementation, compiler project, source-language
   contract, and `Function-Only.wv` development changes to
   `language-1-front-door` wherever the retired owner previously supplied the
   active-compiler check.
3. Retain a planner tombstone for the deleted owner-script names so this
   retirement diff maps to the current Language owner without creating a
   coverage gap.
4. Keep `Compile-Compiler-Source-Set.cmd` and `.sh`. They remain explicit
   bootstrap, recovery, and historical convergence tools; retirement of their
   ordinary verification owner does not delete recovery provenance.
5. Keep source containment and the focused generic binding, layout,
   materialization, and WVLB carrier owners. They test distinct security and
   phase boundaries and are not replaced by the Language owner.

## Evidence

The current Windows Language owner passes all 350 cases and reproduces the
1,055,866-byte analyzer, 974,837-byte emitter, and 1,210,665-byte Generic-WIR
sentinel at their pinned SHA-256 identities. The old sentinel's failed cached
attempt consumed 94,290 milliseconds after duplicating compiler packaging and
fixture compilation.

Removing its five cases leaves 108 registered owners and 5,093 cases across all
four qualification shards. The registry SHA-256 is
`30f7a2130f41b18e5b4ca38e46775bb0ca4cbaef8add0cdf77e06589f4c660de`.
The changed-file plan passes 31 general and 192 native routing cases and selects
11 focused owners with no coverage gap for the complete retirement diff.

## Consequences

Ordinary compiler development has one evolving compiler owner instead of a
split compiler plus a partial legacy compiler. This removes redundant work and
prevents recovery semantics from silently constraining Language 1.0.

The immutable Seed and monolithic recovery tools remain available for named
bootstrap, recovery, security, or historical differential work. Their presence
does not imply that they accept every current Language 1.0 compiler source.

## Reconsideration triggers

Reintroduce an independent current-compiler sentinel only if it owns a distinct
failure boundary not exercised by the Language owner, uses the complete current
compiler closure, and has measured value greater than its duplicated cost.
