# Decision 0828: Lower exact Foundation Sequence reads

## Status

Accepted on 2026-08-22.

## Context

WVB 1.19 already defines verified, executable `sequence.length` and
`sequence.element` operations over the WVB 1.18 kind-6 Sequence identity.
Source signatures can already carry the canonical
`Foundationˉcollections.Sequence<T>` type, but the compiler did not yet bind
the public Foundation read functions or select those operations. Programs
could therefore describe Sequence values without reading them through the
Language 1.0 library surface.

The Foundation contract returns `borrow T` from `Sequenceˉat`. The current WVB
runtime subset admits only resource-free Copy scalars, for which reading the
borrow and copying its value is observationally equivalent and requires no
owned element lifetime. Extending that erasure to aggregates or resource-owning
elements would hide the provenance work still required by Slice 5.

## Decision

1. Bind `Sequenceˉlength` and `Sequenceˉat` as compiler-supplied Foundation
   operations only when a qualified source alias resolves to the exact module
   identity `Foundationˉcollections`. An unqualified spelling, missing alias,
   or lookalike module cannot acquire the intrinsic.
2. Infer the element from the first argument's validated private WVGT kind-12
   `Sequence<T>` identity. `Sequenceˉlength` requires one immutable-borrow
   argument and returns `u64`; `Sequenceˉat` additionally requires an exact
   `u64` index.
3. Admit `Sequenceˉat` only for the WVB 1.19 resource-free Copy scalar subset:
   integers, Boolean, rune, floating point, and exact enums. Lower its borrowed
   result as the equivalent copied scalar value. Text, bytes, records, variants,
   arrays, nested collections, and other resource-bearing elements remain
   rejected.
4. Add WVIR operations 167 and 168 for the two exact Foundation reads. Their
   target is the validated private Sequence shape, their auxiliary value is
   zero, and independent WVIR validation reconstructs the element from WVGT
   rather than trusting the result shape.
5. Lower those operations to WVB 1.19 `CB sequence.length` and
   `CC sequence.element` with the exact planned kind-6 Types index. Both WVB
   operations preserve the shared Sequence owner, so generated code stores the
   scalar result and emits one explicit `pop` to release that preserved
   temporary owner.
6. Keep public fallible Vector construction, `Memoryˉbudget`, recoverable
   append, freeze selection, general borrowed element provenance, native
   lowering, and WebAssembly execution outside this checkpoint.

## Consequences

- `Sequence-Read-Main-Pipeline.wv` compiles to a 472-byte WVB 1.19 module at
  SHA-256
  `8f8cb926df946bff3b254b37304ac7cf8ffa744ccea963703cfcfebfdf7e1831`.
  Its `Readˉat` function contains one `CB`, one `CC`, matching type index zero,
  and one result-store/release sequence after each operation. The current
  compiler-aligned verifier accepts it and the independent `Main` returns 42.
- Four malformed WVB mutations reject an old minor, either out-of-range Types
  immediate, and Sequence/Vector kind confusion. Four source cases reject a
  non-Sequence owner, a non-`u64` index, a resource-bearing element, and a
  lookalike library even when that lookalike receives a real Foundation
  Sequence.
- The implementation adds focused catalog queries and exact intrinsic
  selection. It does not add a runtime representation, allocation, capability,
  or new WVB version.
- The current split analyzer is 1,104,336 WVB bytes at SHA-256
  `55c08703e4b4a93904e21ec82a9305adcf895290f6540c55262b115c69565b97`;
  the current target-aware emitter is 1,019,952 WVB bytes at SHA-256
  `9d53ba13e68c186a0092a2f77c6fc22071b128dc6c629d5f010a7a7b8ab1bdc3`.
  Both package through the unchanged profile-7 segmented development path.
- The 108-owner verification registry advances to 5,177 declared cases at
  SHA-256
  `2c4b82a7381d33509a64d1bd0ff057c7871408d2478ae5ff7326e7cb78602ea5`.
- `Source-Wir-Core.wv` reaches 12,343 lines. This remains workable for the
  checkpoint, but a later refactor should extract the cohesive collection
  signature/lowering/validation owner rather than create numbered fragments or
  move isolated helpers without their invariants.

## Reconsideration triggers

Revisit copied scalar erasure when Slice 5 can represent and validate borrowed
result provenance for non-Copy elements. Revisit the intrinsic boundary if the
Foundation registry gains a general compiler-supplied declaration mechanism
that preserves the same exact module identity and cannot be forged by a
lookalike source module.
