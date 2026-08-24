# Decision 0843: Execute recoverable Vector append as WVB 1.25

## Status

Accepted on 2026-08-23 with current-Windows development evidence. Paired Linux
execution remains pending before a cross-host conformance claim. General owned
calls and joins, semantic `using`, reverse-order release, and a hosted resource
consumer remain later Slice 5 work.

This decision supersedes only the serialized-order clauses of Decisions 0811,
0814, and 0842. Their generic materialization, template erasure, and fallible
construction decisions remain accepted.

## Context

The frozen Language 1.0 Foundation contract makes `Vectorˉappend<T>` an
all-or-nothing exclusive mutable operation. Success consumes one item. Ordinary
capacity refusal must preserve the Vector and return that same item through
`Vectorˉappendˉfailure<T>`; trapping, prefix progress, implicit growth, or
discarding an owned item would violate the source contract.

Typed WVIR already represented fallible construction and affine budget/result
owners, but append still lacked a direct executable boundary. The first exact
fixture also combined generic records and variants. Dependency-ordered generic
materialization placed those entries between declared nominal categories,
contradicting the WVB rule that all Types are grouped by semantic category and
then ordered by ordinal name. Weakening the verifier would make canonical bytes
depend on compiler discovery order.

## Decision

1. WVIR 1.7/1.8 operation `173` represents the exact canonical Foundation
   `Vectorˉappend::<T>` call. The result is
   `Result<unit, Vectorˉappendˉfailure<T>>`; its sole operand is exact `T`, its
   target is one direct mutable Vector local, and its auxiliary is exact
   private `Vector<T>` identity.
2. The first executable profile admits a directly named mutable non-parameter
   Vector local and resource-free scalar `T`. Borrow evidence is compile-time
   only. General borrowed parameters and resource-bearing element destruction
   require later profiles without changing the public source signature.
3. WVB 1.25 is the lowest minor containing recoverable append. Opcode `D0`
   (`208`) is nine bytes: `D0 u32 vector-local, u32 result-type`. Every WVB
   1.25 module contains at least one exact `D0` instruction.
4. `D0` consumes one exact `T` from the operand stack and produces exact
   `Result<unit, Vectorˉappendˉfailure<T>>`. Its local must be a non-parameter
   kind-5 `Vector<T>` and its Result type must use the same `T`.
5. Success appends atomically, consumes the item exactly once, retains the same
   unique Vector owner, and returns `Valid(unit)`. It acquires no new budget or
   allocation lease.
6. At reserved capacity, the Vector's owner, length, contents, capacity, and
   iteration remain unchanged. The instruction returns
   `Failure(Vectorˉappendˉfailure(Collectionˉfailure.Capacityˉexhausted(
   Maximumˉitems), Value))`, returning ownership of the original item exactly
   once. Capacity refusal is not a trap.
7. The verifier derives the Vector element and exact nested Result, append-
   failure record, Collection-failure variant, and unit payload from Types. It
   admits at most 64 append instructions per function and rejects mismatched
   local, element, result, field, or nominal identities.
8. Generic materialization order remains bounded dependency evidence, not a
   serialized ordering rule. The emitter constructs a separate immutable WVB
   output map and serializes records, enums, variants, fixed arrays, Vectors,
   then Sequences, strictly by ordinal name within each category. Every nominal
   reference is remapped to that output order; forward references are valid.
9. The generic serializer records separate generic-record and generic-variant
   payload boundaries so the main Types writer can place both in their canonical
   categories. No template, private shape, borrow handle, lease, pointer, or
   alternate runtime generic representation enters WVB.

## Consequences

- `Vector-Append-Executable.wv` emits byte-identically twice as a 3,096-byte WVB
  1.25 module at SHA-256
  `6478cc8b302e91caa54ff3aea835ef3ea1c1722161cd4f12aa587aa432b6918f`.
- The source-built scalar runner appends `7`, refuses the attempted `9` at
  capacity, returns that `9` with exact maximum `1`, and completes with result
  `42`.
- The combined focused owner passes 47 cases: six valid modules, 31 malformed
  mutations, deterministic Split/construction/append publication, typed success
  and refusal paths, and the construction precondition trap.
- The native verification registry remains 112 owners and advances to 5,376
  cases at SHA-256
  `cf78e39ec42551a9fc1715e4582a1a0971aeb35ad2e547a1f7587c0d72da267d`.
- The runner envelope now admits ordinary variant shape `11` in record fields,
  matching the decoder and compiler-aligned verifier. This fixes a latent
  contradiction exposed by `Vectorˉappendˉfailure<T>`.
- This checkpoint fixes durable semantics, canonical bytes, ownership, and
  refusal behavior. It deliberately does not micro-optimize the transitional
  Seed compiler or freeze the scalar runner's heap layout.

## Reconsideration triggers

Broaden the executable element set when exact destruction, tracing, and failure
cleanup exist. Admit Vector parameters when general owned-call and borrow
provenance are represented. Change the private backing only with equivalent
atomic success/refusal, item return, lease credit, and deterministic output.
Do not make dependency discovery a serialized order, turn capacity refusal into
a trap, copy the item or Vector owner, introduce hidden reallocation, or expose
runtime representation in WVIR or WVB.
