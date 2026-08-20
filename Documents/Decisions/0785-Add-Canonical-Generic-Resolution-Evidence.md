# Decision 0785: Add canonical generic resolution evidence

- Status: Accepted
- Date: 2026-08-20

## Context

The Language 1.0 parser now admits bounded generic declarations and
`::`-disambiguated explicit generic calls, while the existing WIR and WVB
backend remains deliberately monomorphic. Connecting that syntax directly to
code emission would leave argument inference, specialization identity,
deduplication, diagnostics, and code-growth limits implicit across compiler
phases.

The compiler needs an immutable semantic product between source binding and WIR
lowering. It must retain exact conflict origins, make explicit and inferred
calls converge on the same identity, reject excessive work before publication,
and produce deterministic results without depending on host hash-table order.

## Decision

1. Add one explicit generic-resolution and specialization-admission phase
   before ordinary monomorphic WIR generation.
2. Encode a solved generic argument list as bounded WVGS 1.0 evidence. Preserve
   the first contribution's origin, accept later structurally equal
   contributions, and diagnose conflicting or unsolved parameters exactly.
3. Encode admitted concrete specializations as a bounded WVGC 1.0 catalog. A
   specialization identity is the declaration identity plus its ordered
   structural arguments; origin, depth, and code estimate are not identity.
4. Search for reuse before enforcing growth bounds. Limit each declaration's
   solution to 32 parameters and each catalog to 256 instances, depth 32,
   1 MiB of retained evidence, and 16 MiB of estimated specialized code.
5. Use deterministic linear admission and bounded pairwise duplicate
   validation. Hash layout, iteration order, and collisions are not semantic
   inputs.
6. Derive counts and aggregate measurements from canonical evidence instead of
   retaining independently mutable copies. Keep failure evidence in fixed-size
   diagnostic byte records.
7. Keep WIR and WVB monomorphic. Later integration substitutes concrete
   arguments into the existing backend; it does not add runtime generics or a
   parallel compiler.

## Consequences

Generic inference conflicts, specialization reuse, and growth limits are now
independently testable before source-symbol or code-generation integration. The
evidence is internal to the compiler and does not change WVB or package format.

General generic source declarations still do not compile end to end. The next
checkpoint must connect declaration and call symbols to WVGS/WVGC, then lower
each admitted concrete specialization through WIR and WVB.

The focused semantic self-test uses the hosted native package path because the
pinned scalar runner does not expose this ownership-heavy compiler-service
closure. This checkpoint does not require storage, OS, paired-host, or broad
qualification verification.

While this phase remains an independent compiler module, changed-file
verification selects compiler-only source containment and the Language 1.0
front-door owner. The full compiler source sentinel returns when source-symbol
integration makes the phase part of the real compiler closure; selecting it
before then would only repackage an unrelated source set.

## Reconsideration triggers

Reconsider the limits only with representative source and measured compile
time, memory, and generated-code evidence. Reconsider the linear catalog only
if the bounded 256-entry workload is a measured hotspot and a replacement keeps
canonical order and collision-independent behavior. Reconsider the evidence
shape if Language 1.0 adds another generic-argument category or the monomorphic
backend cannot consume concrete substitution without losing exact diagnostics.
