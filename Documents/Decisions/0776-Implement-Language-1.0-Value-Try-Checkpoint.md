# Decision 0776: Implement the first Language 1.0 value-producing try checkpoint

- Status: Accepted
- Date: 2026-08-19

## Context

Decision 0767 freezes `try Expression` as a value-producing unary expression
over the exact Foundation `Result<T, E>` identity. The operand must execute
once. `Valid(Value)` continues with `Value`; `Failure(Error)` returns the
original failure from the containing function without an implicit conversion.

The active compiler already retained an older statement-only Seed `try` over a
two-case variant, but it did not produce a value and it recognized structural
case spelling rather than the frozen Foundation identity. General generic
specialization, Foundation package publication, and explicit error adapters are
later boundaries in the ordered migration. The first executable checkpoint must
therefore be closed to false positives while remaining a strict subset of the
final generic rule.

The source compiler was also close to the fixed 32 MiB native construction-plan
bound. The checkpoint could not widen that limit or create a second compiler or
runtime path.

## Decision

1. `try` is admitted at the existing unary precedence. Its operand is compiled
   and evaluated exactly once.
2. Edition-1 value `try` accepts only a nominal variant declaration named
   `Result` in an edition-1 module named `Foundationˉresult`. The declaration
   must contain exactly `Valid(Value: T)` at case index zero and
   `Failure(Error: E)` at case index one, with no third case. A structurally
   identical declaration in another module is rejected.
3. The lowerer tests the existing result value once. The failure edge returns
   that original value unchanged. The success edge extracts the named `Value`
   field and resumes as an ordinary typed expression.
4. Until bounded generic specialization supplies distinct `Result<T, E>` and
   `Result<U, E>` instances, the operand and containing return shapes must be
   the same concrete nominal shape. This deliberately rejects some valid final
   Language 1.0 programs instead of admitting an implicit error conversion or
   an unsound structural approximation.
5. Edition-1 statement `try Expression;` uses the same value lowerer and
   discards the success value. Descriptorless Seed retains its older
   statement-only shape and does not gain edition-1 value syntax.
6. No WVIR operation, WVB opcode, serialized format version, verifier rule, or
   scalar runtime representation is added. Existing variant test, field access,
   branch, and return operations own the complete runtime behavior.
7. Unary-expression lowering is extracted into one focused function. Repeated
   inaccessible-or-missing symbol guards are consolidated without changing
   their diagnostics. These source refactors keep the rebuilt compiler inside
   the unchanged native output bound.

## Evidence

`Result-Try.wv` covers success extraction, unchanged failure propagation, and
statement-form reuse. It compiles deterministically to a 1,430-byte WVB with
SHA-256
`afa5b9ca60eacb30042a8c6621f12969d06a105510a3379b26b49e883fb9b0cf`.
The current compiler-aligned verifier accepts it and the source-built scalar
runner returns exactly `42`.

Four separate source fixtures reject a lookalike module, a wrong success-field
name, an extra case, and a scalar operand without publishing output. The Windows
Language 1.0 owner passed all 141 cases across 10 visible phases. Its Linux
counterpart contains the same deterministic, rejection, verifier, and runtime
checks; paired-host evidence remains reserved for the final integrated gate.

The rebuilt compiler contains 501 functions, 964,105 code bytes, and 1,164,935
module bytes. Native planning reports 33,452,623 machine-code bytes and 2,472
relocation bytes. The unchanged staging path publishes a 33,482,742-byte object
across 42 bounded chunks with a 528-byte manifest; no size limit was widened.

## Non-decision

This checkpoint does not claim completion of Migration Slice 3. It does not yet
publish the hash-pinned generic Foundation `Option<T>` and `Result<T, E>`
packages, propagate from `Result<T, E>` to a distinct `Result<U, E>` return
shape, add explicit error adapters, or migrate a manual status family. It also
does not implement general generics, ownership, borrowing, effects, localized
tokens, or localized public-library lookup.

## Consequences

The compiler now has one executable, identity-checked control path for
value-producing typed failure without a new bytecode or runtime facility. The
accepted subset cannot silently treat a user lookalike as Foundation Result.
The next Slice 3 checkpoint must publish the exact Foundation identities and
their bounded specializations before relaxing the same-concrete-shape rule.

## Reconsideration triggers

Replace the concrete-shape restriction only when generic specialization can
prove exact shared `E`, preserve the original `Failure(Error)` value, and emit
deterministic verifier-accepted WVB. Reconsider compiler recognition of the
Foundation identity only through a versioned registry-binding decision; case
spelling alone must never become sufficient.
