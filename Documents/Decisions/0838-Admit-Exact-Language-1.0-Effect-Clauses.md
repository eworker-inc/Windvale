# Decision 0838: Admit exact Language 1.0 effect clauses

## Status

Accepted on 2026-08-23. Exact effect resolution, call-graph enforcement,
allocation leases, and paired-host conformance remain pending.

## Context

The frozen Language 1.0 grammar requires every function value to carry one
exact effect set. Exported functions and protocol signatures must spell even an
empty set with `effects()`; local functions may omit the clause only when the
compiler derives the exact set. Allocation leases and public fallible Vector
construction therefore cannot be implemented honestly while `effects` is
still an ordinary identifier and function declarations discard the clause.

A first token-by-token effect parser was semantically straightforward but
retained one large lexer record per identity component. Boundary tests with 32
identities, 16 components, or a 128-byte identity placed unnecessary pressure
on the current bounded aggregate arena. Raising runtime limits or broadly
optimizing the transitional compiler would hide the local ownership problem.

## Decision

1. Append token identity 102 for the exact lowercase keyword `effects`.
   Edition 1 admits it and descriptorless Seed rejects it.
2. Parse an optional effect clause immediately after a function return type.
   Admit an empty set, comma-separated canonical identities, and one trailing
   comma. An identity contains lowercase ASCII segments beginning with
   `[a-z]` and continuing with `[a-z0-9_]`, joined by dots.
3. Bound one clause to 32 identities, 16 segments per identity, 128 canonical
   identity bytes, and 16,384 source bytes including trivia. Reuse the lexer's
   exact whitespace, comment, UTF-8-width, and identifier-component primitives.
4. Retain whether the clause was present, its exact source offset and length,
   and its identity count in the function declaration record. Preserve an
   absent clause for later local inference.
5. Do not yet claim effect semantics. This checkpoint does not resolve
   identities against the canonical registry, require `effects()` on exports,
   derive local sets, compare calls and captures, or emit effects into WVIR,
   WVB, or package evidence.
6. Scan canonical identity bytes directly after the two ordinary opening
   tokens. This keeps exact syntax and diagnostics bounded without retaining a
   large lexer record for every segment. It is a measured blocker fix that
   survives self-hosting, not broad compiler tuning.
7. Build one deterministic hosted test WVB and execute each of its 20 cases in
   a fresh bounded scripting invocation. This isolates runtime state while
   avoiding 20 compiler builds.

## Consequences

- The compiler now distinguishes `effects` in Edition 1 and retains exact
  declaration-clause evidence without changing WVB or WVIR versions.
- Twenty focused cases cover the keyword, empty and populated sets, contextual
  keyword segments, trailing comma, malformed casing and separators, missing
  closure, exact and first-rejected identity/count/segment bounds, retained
  declaration spans, inferred local absence, and edition separation.
- The focused project rebuilds byte-identically as a 373,281-byte WVB at
  SHA-256
  `0a6e703cbb9b0536addaad8211c82d6d99ffddb9cf999d61ae6d45910b53153c`.
  Every isolated Windows execution returns 42.
- The verification registry advances to 111 owners and 5,298 cases at SHA-256
  `eb61fe17b976553df0e53564b625aa2b37f9ec802cc5772d7740b06eeb2eb7ed`.
  Paired Linux execution remains pending.
- Allocation work can now name `memory.allocate` in source, but it must wait
  for exact semantic resolution and call-site enforcement before the compiler
  may claim the effect.

## Reconsideration triggers

Change these finite limits only with representative source and verifier
evidence. Move clause parsing into a separate module when another real consumer
needs that ownership boundary. Do not serialize effects until their canonical
identity order, resolution, inference, and compatibility rules have independent
verification.
