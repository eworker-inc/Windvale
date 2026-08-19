# Decision 0772: Implement Language 1.0 named variant fields

- Status: Accepted
- Date: 2026-08-19

## Context

Decision 0767 freezes variant cases with zero through 64 named fields, named
construction, and named destructuring. Descriptorless Seed and WVB 1.11 model a
case as either no payload or one payload. Repeating that one-payload mechanism
would hide field identity, impose source-visible nesting, and make construction
and matching depend on an accidental runtime representation.

The compiler therefore needs one bounded product inside each nominal sum case,
while retaining the exact zero-field and one-field bytes already published by
unaffected programs. Independent verification must recover each field's exact
type without dynamic name lookup or trusting compiler output.

## Decision

Implement the compiler/verifier checkpoint as follows.

1. An edition-1 variant case has zero through 64 uniquely named fields. An empty
   parenthesized field list is invalid; a no-data case omits parentheses.
2. Construction uses `Type.Case { Field: Value, ... }`, including `{}` for a
   no-data case. Every field appears exactly once. Values evaluate left to right
   in source order, and only their temporary identities are reordered to
   declaration order for construction.
3. A named match pattern lists every field exactly once in any order. `_`
   discards without a binding; every other immutable binding is arm-local.
4. WVIR `Variantˉcreate = 65` consumes exactly the selected case's zero through
   64 fields. New operation `Variantˉfield = 164` consumes one exact nominal
   variant, carries the nominal index in `Target`, packs `case * 64 + field` in
   `Auxiliary`, and produces that field's exact shape. The retained
   `Variantˉpayload = 67` is valid only for exactly one field.
5. WVB 1.16 adds variant field-list marker `2`, a `u32` field count from 2
   through 64, and repeated names and shapes. Markers `0` and `1` retain their
   exact earlier encodings. Opcode `C4` carries the nominal index and packed
   case/field selector. Marker `2` or opcode `C4` selects version 1.16.
6. The compiler-aligned verifier validates canonical markers, counts, names,
   shapes, constructor arity and operand types, packed indices, exact nominal
   operands, field result types, version admission, instruction width, control
   boundaries, and truncation before execution.
7. Descriptorless Seed retains positional zero/one-payload syntax and prior WVB
   bytes. Edition 1 does not expose a layout, tuple position, pointer, or dynamic
   field lookup.
8. Execution consumers that do not implement WVB 1.16 remain explicit narrower
   subsets. Compiler/verifier admission does not silently claim scalar, native,
   WebAssembly, or Windvale OS execution.

## Evidence

`Tests/Fixtures/Language-1.0/Multi-Field-Variant.wv` covers a three-field case,
a no-data case, trailing commas, source order different from declaration order,
named destructuring in another order, and zero-field named construction. It
also retains empty `if` and `else` blocks as a parser-disambiguation regression.
It compiles twice to the same 918-byte WVB 1.16 module with SHA-256
`f3ceb596f1bcedda877ceea5aeb99aff1d5bcfa3b984fdae0e16eb21570562d1`.

`Named-Variant-Field.wv` isolates a legacy marker-`1` case with opcode `C4` and
therefore proves instruction-driven WVB 1.16 selection. It compiles to 428 bytes
with SHA-256
`2dea4aa515633e85863e51279f320d53f09c2bf4628b72d93fdc79559479209f`.
Both modules pass the compiler-aligned verifier.

Nine negative source fixtures cover duplicate declaration fields, an empty
payload declaration, missing/duplicate/unknown construction fields, type
mismatch, and missing/duplicate/unknown pattern fields. Nine byte-level
mutations cover version downgrade, unknown marker, too-small and too-large field
counts, out-of-range field and case indices, wrong field type, wrong nominal
identity, and truncation. This is focused Windows development evidence; paired
host CI owns cross-host conformance, and no heavy storage or complete
qualification gate is claimed.

## Non-decision

This checkpoint does not define a public variant memory layout, native ABI,
scalar runtime representation, value-producing `if` or `match`, guarded or
record patterns, localized token execution, generalized tuples, reflection, or
complete Language 1.0.

## Consequences

The frozen source spelling now reaches one compiler architecture and a versioned,
independently verified bytecode contract without duplicating compiler pipelines
or changing unaffected zero/one-field bytes. WVB readers that stop at 1.15 fail
explicitly. The next Slice 2 checkpoint can implement one bounded scalar
representation for WVB 1.16, then reuse the same typed branch evidence for
value-producing control flow.

## Reconsideration triggers

Reconsider the packed selector only if a future accepted case-field limit exceeds
64. Reconsider the compatibility markers only through an explicit WVB version
decision with replacement fixtures and independent malformed-input evidence.
