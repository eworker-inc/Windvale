# Decision 0744: Add exact result propagation

- Date: 2026-08-16
- Status: Implemented with local Windows compiler and native-lowering evidence; independent Linux execution pending
- Advances: [post-.NET-retirement language and library stage](../Project/Post-Dotnet-Retirement-Language-And-Libraries.md)
- Contracts: [Seed language](../../Specifications/Seed-Language.md), [typed source IR](../../Specifications/Compiler-Source-Wir.md), and [source-to-WVB backend](../../Specifications/Compiler-Source-Wvb.md)

## Context

Windvale already models expected failures as nominal variants and requires
exhaustive `match`. Forwarding one unchanged failure through a validation chain is
therefore explicit but verbose. `Wvdbˉreaderˉvalidate` measured the pattern:
it retained a result and Boolean flag, matched both cases, reconstructed the
failure, and guarded a success path solely to continue a page loop.

General exceptions, inferred result conversions, and hidden cleanup would weaken
Windvale's visible failure and capability contracts. No current source value has
an ordinary caller-owned close operation, so propagation must not invent a scoped
ownership promise.

## Decision

- Add the statement `try Expression;`. It evaluates `Expression` exactly once.
- Require the expression's exact nominal type to equal the containing function's
  non-void return type.
- Require that variant to declare exactly two cases in order: `Valid;` with no
  payload, then `Failure(Value)` with one non-void payload.
- Continue execution for `Valid`. For `Failure`, return the original expression
  value unchanged; do not extract or reconstruct its payload.
- Reject every other shape with `Invalidˉtry`, including void functions,
  mismatched nominal return types, additional or reordered cases, a payload on
  `Valid`, or no payload on `Failure`.
- Lower the statement to the existing WVIR variant-case test, branch, and return.
  Keep WVB 1.11 and every runtime/backend serialized contract unchanged.
- Perform no inferred conversion, adapter call, capability call, trap
  interception, implicit retry, or implicit cleanup.

## Evidence

The lexer, body parser, binding pass, WVIR demo, WVB demo, and dedicated source
fixture cover acceptance and rejection. A compiler built by the pinned previous
Windvale compiler accepts the new fixture; its WVB independently validates and
lowers through the existing native x64 object and Windows container path. The
same native path exhibits its pre-existing limitation for returning nominal
variant values, including an equivalent source without `try`, so it is not used
as execution evidence for this source-only change. The compiler/WVIR oracle and
the database consumer remain the behavioral gates until that backend boundary is
widened deliberately.

## Consequences

The WVDB page-validation loop now expresses exact failure forwarding with one
statement while retaining its nominal failure type and evaluation order. Other
result shapes keep exhaustive `match`; case-name similarity alone is insufficient.

This decision does not add exceptions, a generic result type, adapters, scoped
ownership, or cleanup. A later owned resource must define transfer, close,
early-return cleanup order, cleanup failure, revocation, and provider loss before
propagation may interact with it implicitly.

## Reconsideration triggers

Reconsider the exact two-case contract when two measured consumers require a
different success payload or explicit failure adapter and can preserve deterministic
typing and evaluation. Reconsider propagation of owned values only after a
caller-controlled close contract and precise early-return cleanup semantics exist.
