# Decision 0831: Propagate exact collection result contexts

## Status

Accepted on 2026-08-22.

## Context

Decision 0830 initially resolved `Vectorˉfreeze` from the enclosing function's
declared return shape. That proved exact Vector-to-Sequence lowering but left
the same operation unavailable in a typed local initializer, assignment, or
function argument. Public fallible Vector construction and append will produce
nested typed results, so keeping result selection tied to the enclosing return
would force special cases into every later collection operation.

The compiler must propagate an already-declared expected shape without adding
result-context inference, overload search, or a budget-free Vector constructor.
The expected shape is evidence from the selected program, not a candidate used
to choose among declarations.

## Decision

1. An ordinary call may receive one exact expected shape from a declared local,
   an assignment target, the enclosing function return, or a parameter of an
   already selected non-generic fixed-signature function.
2. `Vectorˉfreeze` accepts that context only when it is the exact canonical
   `Foundationˉcollections.Sequence<T>` whose element shape equals the consumed
   canonical `Vector<T>` element. A missing, inferred, non-Sequence, or
   different-element context rejects as `Invalidˉcollection` before WVIR
   publication.
3. The expected shape does not participate in callable lookup, overload
   selection, generic argument solving, or inferred local typing. Generic calls
   retain their frozen argument-derived or explicit full-arity rules.
4. A fixed-signature call compiles an argument with its selected parameter
   shape. This permits a nested `Consume(Vectorˉfreeze(Values))` only because
   `Consume` was independently selected and declares one exact
   `Sequence<i32>` parameter.
5. WVIR operation 170 and WVB 1.20 remain unchanged. Independent WVIR
   validation continues to reconstruct the Vector and Sequence element shapes;
   contextual source checking does not replace serialized-evidence validation.
6. Broader contextual propagation through value-producing control flow,
   generic calls, and future overload-like features is not implied by this
   checkpoint.

## Consequences

- The source fixture now covers direct return, declared-local initialization,
  assignment, and a nested fixed-signature argument. It publishes a
  deterministic 1,199-byte WVB 1.20 module at SHA-256
  `c73f2e77aa4208a74385046a27beba7dea42e4cece730bfd9ac0ac61ca7a77bc`;
  the verifier accepts it and the unchanged runner returns 42.
- Eleven harness cases and eight source rejection cases cover this phase. The
  new source cases reject an inferred result, a mismatched declared result, and
  a mismatched fixed parameter before any product is published.
- The exact split analyzer is 1,112,436 WVB bytes at SHA-256
  `d294003a2cb37c33475c384de71f34d95a18254d34116b8883c576ac416bbbcb`.
  Its cached Windows package is 35,059,200 bytes at SHA-256
  `f12e299b1c9787538b3c6bc41a2a1d0538c2eacebcd3b770849c630867c5b879`.
- The 108-owner verification registry advances to 5,196 declared cases at
  SHA-256
  `b3db60ae871d308c36cf17cc02639efd3a8a7b6d4a107bafb646af1abe6e690c`.
- `Source-Wir-Core.wv` reaches 12,611 lines. It remains below the fixed WVIR
  evidence limit, but contextual call compilation and collection lowering now
  form a clearer extraction boundary. Any later split should own a real phase
  contract rather than create numbered fragments.
- The next public Vector checkpoint still requires the canonical owned
  `Memoryˉbudget` identity, allocation effect, transfer contract, and typed
  recoverable failure. This decision does not authorize an implicit or
  unbounded constructor.

## Reconsideration triggers

Revisit this boundary if generic result contextualization becomes necessary for
an accepted Language 1.0 construct, or when value-producing branch/match
checking can propagate one exact joined context without overload search.
