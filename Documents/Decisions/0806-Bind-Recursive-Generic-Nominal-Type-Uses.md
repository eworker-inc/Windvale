# Decision 0806: Bind recursive generic nominal type uses

- Status: Accepted
- Date: 2026-08-20

## Context

Decision 0804 defined bounded canonical WVGT identities, and Decision 0805
made generic record and variant declarations own validated parameter
namespaces. The remaining gap between those checkpoints was a source use such
as `Choice<Box<i32>, text>`: the ordinary nominal binder correctly rejected the
templates as non-concrete, but no compiler phase parsed their ordered
arguments, resolved nested types, or admitted the resulting identities.

Putting substitution, WIR carriage, and WVB materialization into that first
connection would make failures difficult to attribute. It would also risk
publishing an inner WVGT admission when a later outer argument proved invalid.

## Decision

1. Add a focused generic nominal type-binding phase over validated Source Set,
   Source Symbols, and WVGT evidence. Keep it separate from the current WIR/WVB
   path until its identity and failure contracts execute independently.
2. Give the ordinary type binder first refusal. Primitive, collection,
   non-generic nominal, and existing Foundation-specialized `Option` and
   `Result` shapes retain their established identities and do not enter WVGT.
3. For a general record or variant template, require the exact declared
   argument count and preserve declaration order. A type parameter accepts one
   recursively bound concrete type. A constant parameter accepts only an exact
   fixed-width integer literal matching its declared shape. Retain the frozen
   optional trailing comma.
4. Admit nested generic instances before their parents. Reuse equal WVGT
   identities, retain phantom arguments, and return the private WVGT shape only
   with the replacement catalog that defines it.
5. Treat one outer bind as a transaction. If any nested argument, later
   separator, arity check, contribution, or outer admission fails, return the
   caller's original catalog status and bytes. A rejected outer use therefore
   cannot publish an otherwise-valid inner instance.
6. Enforce the existing depth-32, parameter-32, instance-256, retained-byte,
   and estimated-output bounds. Malformed input catalog evidence rejects before
   an ordinary or generic result is published.
7. Defer field and case substitution, WIR carriage, reachable WVB
   materialization, and deliberate Foundation migration to subsequent
   connected checkpoints.
8. Give this boundary an independent 18-case cross-host owner using the
   content-keyed project and hosted-application caches. Do not require storage,
   OS, complete Language 1, or Qualification gates for this checkpoint.

## Evidence

The focused fixture exercises direct and repeated `Box<i32>`, nested
`Choice<Box<i32>, text>`, type-plus-constant `Buffer<u8, 8u32>`, two distinct
phantom identities, a trailing comma, and an ordinary nominal regression. It
rejects wrong arity, type/constant kind mismatch, constant-width mismatch, a
bare template, arguments on a non-generic nominal, malformed catalog evidence,
and a failed outer use whose successfully parsed inner type must roll back.

The fixture builds to a 649,494-byte WVB with SHA-256
`94a7b1672a846d329c9056f01539ca1d30499ddab0dc460862fd76a5855dfa9b`.
Its 15,842,304-byte four-fragment hosted Windows executable has SHA-256
`9071f7a16051ff46f422cb692d63a10103d3b36e57d0242198203548dc9c0e07`,
returns `42`, and writes no output. The maintained owner reports visible build,
package, and execute phases; its final run passed all 18 declared cases.

## Consequences

The compiler now has one bounded and executable answer for the identity of a
recursive concrete generic nominal use. Nested dependencies are canonical and
failure is atomic with respect to the supplied catalog. This is not yet a claim
that an application containing `Box<i32>` reaches WVB: the main source WIR path
does not consume this binding or substitute template fields and cases.

Shared lexer, declaration, generic, Source Set, Source Symbols, and WVGT changes
select the focused owner in development planning. The first final-shape hosted
package earns its content-addressed checkpoint; repeat verification reuses it
without selecting unrelated storage or OS workloads.

## Reconsideration triggers

Fold this module into the ordinary type binder only when every consumer can
carry the exact replacement WVGT catalog without hidden mutation. Replace the
literal-only constant-argument source parser when migration reaches Language
1.0's already-frozen bounded constant-expression evaluation contract.
