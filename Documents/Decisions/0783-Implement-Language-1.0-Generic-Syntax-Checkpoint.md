# Decision 0783: Implement the Language 1.0 generic syntax checkpoint

- Status: Accepted
- Date: 2026-08-20

## Context

Slice 3 admitted only the exact Foundation `Option<T>` and `Result<T, E>` type
families. Slice 4 requires general generic declarations, argument-derived calls,
full-arity explicit calls, bounded specialization, retained solution evidence,
and collections. The declaration parser previously allowed a narrow list of
variant type-parameter names, while record and function headers had no generic
parameter position. The body parser had no `::`-disambiguated explicit call.

Adding semantic specialization before these spans were exact would force the
symbol and WIR phases to rediscover delimiter structure and would make a bare
relational `<` vulnerable to accidental reinterpretation.

## Decision

1. Admit optional generic-parameter lists on records, variants, and functions.
   Each of at most 32 parameters is either one type identifier or
   `const Name: Integerˉtype` using an exact fixed-width signed or unsigned
   integer type.
2. Keep a declaration's `Items` field assigned to fields, cases, or ordinary
   value parameters. Generic parameters remain in their exact source span and
   are reparsed by the future bounded semantic planner; no second declaration
   collection is allocated.
3. Append `Explicitˉgenericˉcall = 18` to the expression-kind mapping. Retain
   the complete qualified name, argument-list spans, generic-argument count,
   ordinary argument count, node count, depth, and next cursor in the existing
   flat expression record.
4. Require `::` before explicit generic arguments. `Name<T>(...)` is not a
   compatibility spelling and remains an ordinary relational parse candidate.
5. Accept exact fixed-integer tokens as constant arguments in addition to type
   arguments at this syntax boundary. Declaration-ordered classification,
   full constant-expression evaluation, type substitution, structural
   inference, protocol checks, canonical identity, and specialization limits
   belong to the next semantic checkpoint.
6. Keep parser verification bounded. The declaration-parameter and
   explicit-call fixtures are separate WVBs so the pinned depth-eight scalar
   runner does not add test-wrapper frames or retain both large parser closures
   in one executable.

## Evidence

The generic-declaration fixture covers two type parameters, one mixed
type/`u64` constant parameter list, and rejection of a Boolean constant
parameter type. Its 195,184-byte WVB has SHA-256
`79addd47b9e43f15819a00af8614a83ae82d48003e5b27917541bb8a95d05579`
and returns `42` in 25,431 instructions.

The explicit-call fixture combines a qualified name, a type argument, a `u32`
constant argument, and one value argument. Its 320,654-byte WVB has SHA-256
`791adc9f33af3ca7e4c40eccc7760ea5832bb0b02b6eb8c7f83034fda536ff24`
and returns `42` in 26,493 instructions. The focused Language 1.0 owner now
builds and executes both fixtures next to the existing 25-case value front end.

These are current-host parser results. They are not general generic semantic,
collection, native-code, or paired-host qualification evidence.

## Consequences

The semantic planner can consume one unambiguous source representation without
changing the optimizer, verifier, runtime, or WVB format merely to recognize
syntax. Malformed parameter kinds, excessive arity, missing delimiters, and
ambiguous bare angle brackets fail before specialization work begins.

The compiler does not yet accept arbitrary generic declarations as executable
programs. This checkpoint is an incremental Slice 4 boundary, not completion of
the slice or Language 1.0.

## Reconsideration triggers

Reconsider retained spans only if constant-expression parsing requires a
separate immutable syntax product or diagnostic recovery cannot remain bounded.
Reconsider the 32-parameter ceiling only with representative source, measured
compiler memory/time, and a specialization format that rejects excess work
before allocation or code emission.
