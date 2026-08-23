# Decision 0840: Bind fallible Vector construction in WVIR

## Status

Accepted on 2026-08-23. Allocation leases, provider debit/refusal, executable
WVB lowering, recoverable append, semantic effect checking, paired-host
conformance, and broader collection migration remain pending within Slice 5.

## Context

The frozen Language 1.0 Foundation contract makes bounded dynamic collection
construction explicit and fallible:

```text
Vectorˉconstructˉreserved::<T>(Budget, Maximumˉitems)
    -> Result<Vector<T>, Allocationˉfailure>
```

The call must consume one `Memoryˉbudget`, reject an unavailable owner, preserve
the exact requested limit, and name the canonical allocation-failure domain.
Freezing a concrete heap pointer, capacity layout, or lease table in source
semantics would couple the language to a transitional runtime. Conversely,
waiting for the self-hosted compiler before representing the call would leave
the most important ownership and failure behavior untested.

Decision 0837 already supplies an executable, versioned memory-budget Split and
a bounded forward-control ownership proof. That proof is the right independent
oracle for consuming the constructor's budget before executable allocation is
introduced.

## Decision

1. The exact canonical `Foundationˉcollections` declaration
   `Vectorˉconstructˉreserved` is intrinsic identity 10 in the Foundation
   collection symbol family and typed WVIR operation `172`.
2. Calls require exactly one explicit type argument and two arguments. Result-
   context inference, overload search, and a bare generic call are rejected.
3. `T` must be in the currently executable resource-free scalar collection
   subset. The expected result must be the canonical materialized
   `Foundationˉresult.Result<Foundationˉcollections.Vector<T>,
   Foundationˉmemory.Allocationˉfailure>`.
4. The first argument must be one directly named available local of exact
   private `Memoryˉbudget` shape. The second is evaluated as exact `u64`.
5. Operation 172 stores the result shape in `Shape`, the maximum-items
   temporary as its sole operand, the consumed budget slot in `Target`, and the
   canonical Foundation memory module in `Auxiliary`. The failure module is
   derived from the exact Result error identity rather than from the callable's
   Collections module.
6. WVIR 1.5 is selected when operation 171 or 172 occurs without function
   specialization; WVIR 1.6 combines either operation with specialization.
7. Independent WVIR validation reconstructs the Result, Vector, element, and
   allocation-failure identities and consumes the target budget in the existing
   bounded ownership dataflow. A second use fails before WVB publication.
8. Explicit generic calls receive the same exact expected-shape propagation as
   ordinary calls. This is contextual checking after a declaration is already
   selected; it does not use a result to select an overload or solve a generic.
9. The WVB emitter recognizes valid operation 172 but returns exact
   `Unsupportedˉoperation` and publishes no output. Executable lowering waits
   for versioned allocation-lease, failure-atomicity, representation, and
   teardown contracts.
10. Broad tuning of the transitional compiler remains deferred until Language
    1.0 becomes the seed. This checkpoint freezes durable semantics and verifier
    evidence, not a compiler-internal allocation strategy.

## Consequences

- The positive four-module fixture publishes 2,085 source bytes, 104 WVCA
  bytes, 284 WVLB bytes, and exact 456-byte WVIR 1.5 containing one operation
  172. Its current WVB boundary rejects at function 0, operation 1 without
  output.
- Eight byte-level mutations reject version downgrade, operation 173, a bare
  Vector result, missing or wrong-typed maximum evidence, a non-budget target,
  a Collections auxiliary module, and a result temporary reused as the limit.
- Five source fixtures reject inferred generic arguments, a non-`u64` maximum,
  the wrong Result, the wrong budget, and a lookalike allocation-failure record.
  A sixth publishes analyzable source and is independently rejected as invalid
  WVIR when it consumes the same budget twice.
- The maintained split build produces an 83,055-byte admitter at SHA-256
  `aefe1711155aa74bd6f1ac188e778aaf94d5e9f603434d0ce737858f9543cd04`,
  a 1,165,611-byte analyzer at SHA-256
  `351368e34169c8f4c92992f924df0d39bab13168b012e92e943130cd93b80010`,
  and a 1,101,122-byte emitter at SHA-256
  `b0b4f7cd12e7ef90abf61b125c53a05dd13af26eba6b93b13313b599aca35046`.
- The Language 1 front-door contract grows from 462 to 478 cases. The registry
  remains 112 owners and advances from 5,316 to 5,332 cases at SHA-256
  `ae29842cedcd3eda416b8008cf77e03b9346faba5bf0779d4ebacc7468be51f0`.
- The one-shot immutable Seed still fails at its pre-existing compiler-scale
  Source Bindings capacity. Development continues through the maintained split
  compiler rather than widening recovery limits or tuning obsolete internals.

## Reconsideration triggers

Change operation 172 only if the frozen public contract changes or executable
lease evidence proves the typed fields insufficient. Do not add a hidden
ambient allocator, infer a budget, collapse allocation refusal into a trap, or
expose runtime pointer/capacity layout. Advance WVB only with exact success,
refusal, use-after-move, stale-lease, teardown, malformed-input, and bounded-
resource evidence.
