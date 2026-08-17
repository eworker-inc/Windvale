# Decision 0751: Accept the Windvale Language 1.0 direction

## Status

Accepted by the project owner on 2026-08-17. This decision accepts the complete
Language 1.0 product direction and authorizes normative-candidate specification
work. It does not freeze source edition 1, change the implemented Seed language,
change WVB, or claim implementation on any target.

## Context

Windvale now has enough real source to review the language as a whole rather than
allow implementation order to become the permanent design. The audited corpus
contains 866 `.wv` files and about 223,000 lines, with production pressure from
wide records, positional construction, manual result propagation, packed-byte
state, repeated immutable concatenation, and resource paths that depend on
explicit close calls.

The project-owner review in
[Windvale Language 1.0 design](../Project/Windvale-Language-1.0-Design.md)
considered fourteen directions and their alternatives, costs, and freeze
conditions. The owner accepts the recommended direction for all fourteen.

The accepted direction is intentionally broader than the currently implemented
[Windvale Seed language](../../Specifications/Seed-Language.md). A design
direction is not an implementation claim. Exact grammar and semantics must be
written, exercised through representative paper programs, and frozen by a later
named decision before edition 1 becomes a compatibility promise.

## Decision

Accept these Language 1.0 directions:

1. retain capitalized, macron-separated semantic source names and lower private
   declarations to deterministic short machine identities;
2. retain deterministic evaluation, checked values, explicit capabilities,
   bounded resources, typed recoverable failures, and no general exceptions;
3. replace `void` with first-class `unit` and add `never`;
4. complete the fixed-width integer family and define strict `f32`, `f64`, and
   `rune` values;
5. use named record construction and add named update, multi-field variants,
   destructuring, and value-producing `if` and `match`;
6. add bounded static generics and compile-time protocols without class
   inheritance or implicit dynamic dispatch;
7. replace the isolated exact-capacity collection model with fixed arrays,
   runtime-budgeted owned collections, immutable publication, slices, maps,
   builders, and typed arenas;
8. define Copy, shared immutable, owned, and borrowed value classes without
   requiring tracing garbage collection;
9. standardize `Option<T>` and `Result<T, E>` and make `try` an exact typed
   propagation expression;
10. add named call arguments, first-class function values, and explicit closure
    capture;
11. add move-only resource instances and lexical `using`, separating fallible
    semantic completion from infallible local release;
12. add bounded text and byte builders plus bounded interpolation;
13. define structured concurrency in the hosted profile; and
14. define visible unsafe blocks and exact FFI boundaries in the system profile.

Create one specification suite with unambiguous rule ownership:

- the [Language 1.0 semantic specification](../../Specifications/Windvale-Language-1.0.md)
  owns static and dynamic source semantics, conformance, profiles, effects, and
  resource behavior;
- the [Language 1.0 grammar](../../Specifications/Windvale-Language-1.0-Grammar.md)
  owns lexical tokens and parsing;
- the [Language 1.0 Foundation contract](../../Specifications/Windvale-Language-1.0-Foundation.md)
  owns the required standard nominal types, protocols, collections, builders,
  budgets, and failure values;
- the [Language 1.0 paper corpus](../Project/Windvale-Language-1.0-Paper-Corpus.md)
  supplies usability and boundary evidence; and
- the [Seed-to-1.0 migration plan](../Project/Windvale-Language-1.0-Migration.md)
  owns repository transition order without a permanent compatibility
  implementation.

Treat the specification suite as a normative candidate until a later source
freeze. Candidate rules must be exact enough to implement and review, but may be
revised when the paper corpus exposes ambiguity or unacceptable ergonomics. A
revision updates all owning documents together rather than creating aliases or
parallel language modes.

Require deeper evidence before freezing:

- collection maxima and resource-domain budgets must remain usable in complete
  programs;
- moves, borrows, immutable sharing, and arena handles must have understandable
  diagnostics and no unowned escape;
- resource completion and local release must never discard a result; and
- structured tasks must have exact capture, join, cancellation, failure,
  teardown, and bound behavior independent of one host scheduler.

Retain one compiler architecture. Language 1.0 is implemented by evolving the
existing Windvale compiler front end, typed IR, WVB lowering, runtime, native
backend, editor tools, and libraries in coherent vertical slices. This decision
does not authorize a parallel compiler, a textual-assembly compiler path, or a
second object model.

## Consequences

The design review is complete enough for specification work to begin. The
accepted direction gives the grammar, type system, ownership model, Foundation
surface, hosted concurrency model, and system boundary one shared target.

Seed remains the only implemented source contract until implementation advances.
Tools must continue to report their actually supported source edition and target
profile. A candidate specification example is not accepted input merely because
it appears in `Specifications/`.

Many Language 1.0 features are front-end or library contracts and may lower to
existing WIR or WVB. No new opcode or binary version is introduced without an
independent lowering and verification need. The assembler remains a sibling
textual input to shared native encoding and object construction rather than an
intermediate source-compiler stage.

The source-freeze decision requires:

- complete lexical, grammar, type, evaluation, ownership, failure, profile, and
  limit rules;
- complete accepted, boundary, and rejected examples;
- the required paper corpus with owner-reviewed usability findings;
- a feature-to-compiler/library/WVB/runtime/tooling responsibility matrix;
- a bounded Seed migration plan; and
- named cross-host and target evidence requirements for implementation.

## Reconsideration triggers

Reconsider an accepted direction when a complete paper-corpus program cannot
express a required workload clearly, when a rule prevents bounded implementation
on a permanent target, when measured compiler or runtime cost violates an
explicit budget, or when two accepted rules produce an unresolved semantic
conflict.

Do not reconsider solely because another language uses a familiar construct.
Any replacement must state its safety, determinism, authority, allocation,
failure, compatibility, and implementation consequences with equal precision.
