# Decision 0775: Implement Language 1.0 value-producing match

- Status: Accepted
- Date: 2026-08-19

## Context

Decision 0767 freezes a value-producing `match` whose selector is evaluated
once, whose selected arm alone executes, and whose reachable arms produce one
exact type. The compiler already had exhaustive statement `match`, edition-1
named variant-field patterns, and the `Valueˉphi = 64` join generalized by
Decision 0774. A separate expression matcher, WVB selector opcode, or runtime
dispatch path would duplicate those established contracts.

The compiler was also close to its fixed 32 MiB native construction-plan bound.
The checkpoint therefore had to fit without widening that bound or weakening
validation.

## Decision

1. The body parser appends expression kind `Match = 17` without renumbering any
   retained kind. It represents the value form through the same bounded match
   and case views used by statement `match`.
2. A match selector is parsed as an ordinary expression. Its following
   left brace begins the arm list, so calls, indexing, parentheses, and
   brace-form nominal construction can be selectors rather than requiring a
   temporary local.
3. Each value arm uses a braced value block: zero or more ordinary statements
   followed by exactly one final expression without a semicolon. The existing
   enum-member and edition-1 named variant-field patterns are accepted.
4. Value `match` is edition-1-only. Descriptorless Seed retains statement
   `match` and rejects the value form before WIR or WVB publication.
5. Typed WIR lowers the selector once, preserves exhaustive exact-nominal case
   validation and duplicate rejection, and emits only the selected arm's
   runtime path. Variant bindings remain immutable and arm-local.
6. Reachable arm results must have the same exact WIR shape. Pairwise joins use
   `Valueˉphi = 64`; a `never` arm contributes no value and does not invent a
   conversion or temporary.
7. Statement and value forms share one match lowerer. The statement form
   retains its existing fallthrough join, while the value form returns its
   typed joined value.
8. No WVIR operation, WVB opcode, serialized format version, verifier rule, or
   scalar runtime representation is added. Existing branch, jump,
   variant-test/field, and value-phi lowering owns the complete behavior.
9. To preserve the fixed native bound, repeated literal and binary-operator
   shape predicates in the source compiler are consolidated without changing
   their accepted types or operator semantics. The match compiler validates and
   lowers each case in one streaming pass rather than reparsing the complete
   case list twice.

## Evidence

The 25-assertion value-front-end self-test fixes the appended `Match = 17`
identity. `Value-Match.wv` uses a call selector and three enum arms, selects the
middle arm, exercises two pairwise value joins, compiles deterministically to a
588-byte WVB with SHA-256
`320ae22a8f38aea54884cacb7c07be841bf6500f2527aadf58e2f24083c86226`,
and executes with result `42`. `Value-Match-Lazy.wv` places an unbounded
recursive call in the unselected arm; its deterministic 431-byte WVB has
SHA-256
`358a533f491d7901c21ca66f653db33c1654b1730b4d2d4bf8e66cc6fd263a74`
and also returns `42`. `Value-Match-Never.wv` admits a `never`-typed arm without
inventing a value, compiles deterministically to a 422-byte WVB with SHA-256
`cfc597be4a1dc57ef6d52bb3ff61962680508d5f62b18c8057c0d231ffd1db73`,
and returns `42` through the source-built WVB 1.15 scalar runner.

`Value-Match-Variant.wv` selects a brace-form two-field variant construction,
binds both fields by name in a different source order, and compiles
deterministically to a 634-byte WVB 1.16 with SHA-256
`db8b1cab5c672dfccc337fb7874a9b07f9b5e2fb6b4243cd8c1f0a35e70af2f6`.
The source-built WVB 1.16 scalar runner executes it with result `42`. Separate
fixtures reject a missing case, a trailing semicolon where an arm value is
required, mismatched arm types, and descriptorless Seed use without publishing
WVB output.

The final source compiler candidate contains 498 functions, 961,629 code bytes,
and 1,161,873 module bytes. Native planning reports 33,408,489 machine-code
bytes and 2,440 relocation bytes. The unchanged bounded staging path publishes
a 33,438,278-byte object package across 42 chunks with a 528-byte manifest.
The fixed aggregate and per-chunk limits were not widened.

The focused language owner rebuilds the compiler once and reuses that image for
all deterministic, execution, malformed-source, and retained regression cases.
It passed all 136 cases on Windows in 371.69 seconds, including the complete
retained fixed-integer, rune, floating, unit/never, and WVB 1.16 variant phases.
Heavy storage, broad OS, paired-host, and complete Qualification gates remain
deferred to the final seven-slice integration gate.

## Non-decision

This checkpoint does not implement guarded arms, record patterns, ownership and
borrow checking not yet present in the active compiler, localized token
execution, or a new bytecode/runtime match facility. Those boundaries remain
owned by their corresponding migration slices and frozen Language 1.0 rules.

## Consequences

Edition-1 `if` and `match` now share one typed value-block and value-phi control
architecture. Existing statement match behavior remains on the same lowerer,
and selector expressions no longer depend on a hand-written delimiter scan.
Slice 2's planned value-and-control compiler surface is implemented; the next
major migration checkpoint is Slice 3 typed failure and value-producing `try`.

## Reconsideration triggers

Reconsider pairwise phi construction only through a versioned WVIR decision
that preserves exact predecessor validation and deterministic WVB bytes.
Reconsider the current pattern subset when the guarded and record-pattern
checkpoint supplies ownership, exhaustiveness, and malformed-input evidence.
