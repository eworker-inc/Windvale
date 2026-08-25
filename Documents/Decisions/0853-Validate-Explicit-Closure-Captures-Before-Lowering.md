# Decision 0853: Validate explicit closure captures before lowering

- Status: Accepted
- Date: 2026-08-25

## Context

Language 1.0 function values must not silently retain lexical state, mutable
owners, provider instances, capabilities, or borrows. The existing WVLB phase
describes top-level function locals, but a closure body needs a separate scope:
only its declared captures and parameters are visible as lexical bindings.
Treating captures as ordinary immutable parameters also loses the one mode that
must permit mutation, `borrow mut`.

## Decision

1. Analyze every closure against an isolated binding phase containing exactly
   its explicit captures and parameters.
2. Require each capture to name a visible outer local and reject duplicate
   captures or capture/parameter conflicts.
3. Preserve four exact modes: copy, move, immutable borrow, and mutable borrow.
   Only mutable borrow creates a mutable binding in the closure scope.
4. Admit copy only for a conservative Copy or shared-immutable classification;
   reject an unproven aggregate instead of assuming bytewise copyability.
5. Reject borrowed captures in async closures until the structured-task slice
   proves their owner, suspension, and join lifetime.
6. Keep required module capability roots outside the lexical capture list while
   retaining every capability call in the closure's exact effect set. Local
   provider or capability instances remain ordinary explicit captures.
7. Retain one bounded 24-byte evidence entry per capture, with at most 64
   captures and 1,536 evidence bytes.
8. Validate WVSD, WVLB, function-range, and source-span evidence before reading
   it. Invalid upstream evidence fails closed.
9. Keep this compiler evidence private. Move invalidation, escape analysis,
   closure environment representation, indirect calls, WVB, verifier, runtime,
   and native integration remain later Slice 6 checkpoints.

## Evidence

The focused project builds through the current native source compiler. Its
compiler-scale WVB uses the segmented native application path because the
ordinary single-image lowerer reaches its fixed output ceiling. One published
application is reused for nine selectors covering every accepted mode, the
principal semantic rejection boundaries, and forged valid-status/empty-directory
evidence; all selectors return `42`.

The exact current Windows development WVB is 941,148 bytes at SHA-256
`733fd5313d8de51c79574b577affc46aef901572cfc2ab8a94805015622020b4`.
The focused owner packages it through the segmented native path and reuses one
application for all nine selectors. This is focused current-host evidence;
paired-host conformance remains a later integration gate.

The first test revision intentionally used the legacy inline module header while
marking its packed source as edition 1. The source graph rejected it before
capture analysis. Rewriting the embedded programs to the canonical edition-1
module, profile, platform, and authority declarations made that version boundary
explicit. A separate mutable-borrow case then exposed and corrected the initial
immutable-parameter modeling error.

## Consequences

Closure bodies now have a deterministic lexical boundary that cannot acquire an
outer local by omission. Mutable access is visible in the capture syntax and in
the isolated binding kind. Capability roots remain auditable as module
dependencies and effects without redundant pseudo-captures.

This decision does not make closures executable. A later decision must select
and version the public callable value, environment ownership, indirect-call,
and runtime contracts before any compiler-private shape or capture evidence can
enter WVB.

## Reconsideration triggers

Reconsider the conservative Copy classification when aggregate derivation has a
complete recursive ownership proof. Reconsider async borrowing only when the
structured-task verifier can prove owner immobility, suspension safety,
cancellation, join, and teardown for the exact capture.
