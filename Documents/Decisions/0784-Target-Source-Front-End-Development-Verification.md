# Decision 0784: Target source-front-end development verification

- Status: Accepted
- Date: 2026-08-20

## Context

The changed-file planner treated every Windvale source-compiler component as a
backend change. Editing the lexer, declaration parser, body parser, source
profile, source set, graph, symbols, or bindings therefore selected Seed,
unsafe-WVB, source-containment, lowerer-rejection, console-packager,
compiler-sentinel, and sometimes Language 1.0 owners.

That mapping made one source grammar or typing slice repeatedly verify native
lowering and packaging boundaries it did not change. The Language 1.0 front-door
owner already reconstructs the changed compiler, runs its exact parser and
semantic fixtures, checks deterministic WVBs, verifies them, and executes the
focused results. The compiler source sentinel independently checks the real
compiler closure, while source containment owns hostile source bounds.

## Decision

1. Map the eight source-front-end cores—lexer, declaration parser, body parser,
   source profile, source set, graph, symbols, and bindings—to exactly:
   `source-containment`, `language-1-front-door`, and
   `compiler-source-sentinel` for ordinary development verification.
2. Keep WVIR, source-to-WVB, native lowering, bytecode, runtime, packaging, and
   executable-format changes on their existing broader owners. A change set
   touching both front-end and backend files receives the union, so this rule
   cannot suppress evidence selected by the changed backend boundary.
3. Keep Qualification as an explicit release, promotion, security, or final
   cross-host gate. The narrower development mapping does not make a paired-host
   or complete-stack claim.
4. Retain exact planner self-tests for both declaration and body parser paths.
   The plan verifier reports bounded progress across its general and native
   routing cases so a long audit is visibly active.

## Consequences

Language slices can iterate against the compiler and source-security boundaries
they actually modify. Native lowerer, unsafe-WVB, and console-packager work is
not repeated for each parser or symbol edit; it returns automatically when a
changed file belongs to those boundaries or at the final integration gate.

This reduces redundant verification, not verification depth inside the selected
owners. A newly discovered cross-boundary dependency must gain an explicit
mapping and regression case rather than restoring one undifferentiated compiler
bucket.

## Reconsideration triggers

Reconsider the mapping if a front-end-only change can alter native lowering or
packaging without changing WVIR/WVB evidence, if one of the three selected
owners stops rebuilding the current compiler, or if measured failures show a
missing boundary-specific oracle.
