# Decision 0819: Retire the Foundation generic side path

- Status: Accepted
- Date: 2026-08-21

## Context

Foundation `Option<T>` and `Result<T, E>` first reached Language 1.0 through a
bounded temporary encoding. The symbol binder packed their arguments into
reserved `u32` shape ranges, WIR carried dedicated construction, matching, and
`try` branches, and Source WVB collected a second first-use plan serialized as
private `__WvZ000` through `__WvZ255` variants.

The general generic pipeline now owns template binding, `WVGT` instance
identity, substituted layout, WIR construction and field selection, ordinary
WVB Types materialization, and execution. Keeping both paths would duplicate
validation, canonical ordering, size limits, and every future language change.

## Decision

1. Bind Foundation `Option<T>` and `Result<T, E>` applications through the same
   `WVGT` catalog as every other generic nominal type. No packed Foundation
   shape is a valid compiler model.
2. Require explicit complete type arguments on Foundation variant
   construction, such as `Result.Result.Valid<i32, u32> { Value: 42 }`. This is
   the existing Language 1.0 rule for generic nominal construction; Foundation
   does not retain a context-inference exception.
3. Lower construction, matching, case tests, and field reads through the general
   generic variant operations and materialized layouts.
4. Keep `try` as Foundation-specific language semantics, but recognize the exact
   edition-1 `Foundationˉresult.Result<T, E>` declaration from its ordinary
   materialized layout. The operation requires matching error shapes, extracts
   the exact `Valid.Value` or `Failure.Error` field, and does not depend on a
   private packed representation.
5. Serialize all retained generic instances in canonical `WVGT` order as
   ordinary private `__WvY0000` through `__WvY1023` WVB Types entries. Remove
   the Foundation first-use plan, its 256-entry side limit, the `__WvZ` suffix,
   and the associated corruption statuses and tests.
6. Preserve the frozen Language 1.0 grammar. This migration changes semantic
   routing and removes the Foundation construction exception; it introduces no
   new token or ambiguous production.

This decision supersedes the Foundation-only representation in Decisions 0780
and 0811. Their general Option/Result and ordinary-WVB conclusions remain
accepted.

## Evidence

The complete Foundation fixture carries Option and Result construction and
matching, same-error/different-success `try`, statement `try`, nested calls, and
16 concrete specializations. The clean analyzer publishes a 3,233-byte WVSS,
104-byte WVCA, 1,796-byte WVLB, and 5,144-byte WVIR. The clean emitter publishes
a 3,143-byte WVB 1.16 with SHA-256
`fb3d07717252b60dcbcd6da1a95dbf6bccb8b85ba79d3a08c5e0e6306b722a81`.
It contains `__WvY` identities, contains no `__WvZ` identity, and returns `42`
in 360 scalar-runner instructions.

Removing the side path reduces the optimized emission compiler from 542 to 530
functions and from 987,682 to 974,837 WVB bytes. Its packaged Windows
development executable falls from 21,718,016 to 21,490,688 bytes. The refactored
compiler self-analyzes, emits, stages, links, transports, and packages within the
unchanged 128-nominal-type native profile.

The current analyzer is 1,055,866 bytes at SHA-256
`2edf577a8b549fff0f351264e814e25783011a94942c904780a57be6ec1194b7`;
the current emitter is 974,837 bytes at SHA-256
`25e9d5b491627b083ceb288f285901ca958bf3d1b12c427ba850a36a539328c9`.
The compiler-scale Generic-WIR sentinel builds twice to identical 1,210,665-byte
WVBs at SHA-256
`1b79838079339a397d05d4b03f8e42f94b978d7c583cf9ace4cb1e6abaedf696`.

The focused materialization fixture retains 28 general-plan cases after deleting
two grouped Foundation-plan-only cases. A new source rejection proves that an
omitted generic construction argument list publishes no WVB; the mismatched
`try` fixture now uses explicit constructors so it continues to isolate error
shape rejection.

## Consequences

There is one generic nominal implementation to optimize and verify. Foundation
values use the same catalog identity, target remapping, WVB metadata, runtime
layout, and limits as user-defined generic variants. Compiler size and future
change surface both decrease.

Source written for the temporary inferred Foundation constructor spelling must
add explicit type arguments. This is a pre-1.0 migration with no compatibility
alias. Existing type annotations such as `Result.Result<i32, u32>` are unchanged.

The frozen grammar remains open to a later evidence-backed revision, but this
implementation found no grammar defect requiring one.

## Reconsideration triggers

Revisit this decision if Language 1.0 adopts a general, deterministic nominal
construction-inference rule, if WVB gains reified generics for an independent
reason, or if a future `try` design intentionally applies to a protocol broader
than the exact Foundation Result family.
