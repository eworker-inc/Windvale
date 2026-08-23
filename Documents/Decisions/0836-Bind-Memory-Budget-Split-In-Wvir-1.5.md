# Decision 0836: Bind memory-budget Split in WVIR 1.5

## Status

Accepted on 2026-08-23. Executable provider accounting and candidate toolset
qualification remain pending.

## Context

Decision 0834 transfers one opaque `Memoryˉbudget` owner from the launcher into
WVB `Main` and releases it at invocation teardown. Slice 5 next needs a child
budget with explicit byte and child limits. Treating Split as an ordinary
function would erase the mutable parent identity before WVIR, while copying the
opaque value into an operand would contradict its affine ownership contract.

The executable provider has not yet gained allocation accounting, leases, or a
fallible allocation surface. Publishing a placeholder WVB opcode now would make
an unimplemented effect appear executable. The compiler still needs a precise,
independently verifiable boundary from which that runtime work can proceed.

Adding the ownership proof inline to the already large general WVIR validator
also exceeded the current native emitter's record-storage analysis boundary.
That is a measured self-hosting blocker, not evidence that the validator's
limits should be raised or that the transitional compiler should receive a
broad optimization pass.

## Decision

1. The canonical call is `Foundationˉmemory.Split(borrow mut Parent,
   Maximumˉbytes, Maximumˉchildren)`. It resolves only in edition 1 through the
   exact `Foundationˉmemory` module and has arity three.
2. `Parent` must be one directly named mutable local of exact compiler-private
   `Memoryˉbudget` shape `805306368`. Immutable, derived, temporary, or
   non-budget borrow origins are rejected.
3. `Maximumˉbytes` is exactly `u64`; `Maximumˉchildren` is exactly `u32`. They
   are evaluated left to right and become the only two serialized operands.
4. The exact result is the materialized canonical
   `Foundationˉresult.Result<Memoryˉbudget,
   Foundationˉmemory.Allocationˉfailure>`. The failure record must contain, in
   declaration order, `Reason` of the canonical same-module
   `Allocationˉreason` enum, `Requestedˉbytes: u64`, and
   `Availableˉbytes: u64`. Matching names with a different layout do not
   acquire this identity.
5. WVIR operation `171` represents Split. Its result is the exact private
   Result instance, its operands are the `u64` and `u32` limit temporaries,
   `Target` is the parent local slot, and `Auxiliary` is the canonical
   Foundation memory module index.
6. WVIR 1.5 is the non-specialized 1.3 layout with operation 171. WVIR 1.6 is
   the specialized 1.4 envelope with operation 171. A directory must select
   1.5/1.6 exactly when Split is present; programs without it retain byte-for-
   byte 1.3/1.4 headers.
7. Independent validation reconstructs the exact generic Result and failure
   layout and runs a conservative affine proof only for functions containing
   Split. This first proof accepts one basic block, tracks live budget slots and
   moved budget temporaries, rejects duplicate ownership and use after move,
   and requires every temporary budget owner to be consumed.
8. The WVB writer rejects operation 171 as `Unsupportedˉshape` and returns no
   output. Executable Split, allocation effects, leases, provider accounting,
   control-flow ownership joins, and fallible Vector construction are later
   connected Slice 5 checkpoints.
9. The focused ownership proof remains a separate function so the compiler can
   rebuild through the current native emitter without widening fixed analysis
   bounds. Broad optimization of the transitional compiler remains deferred
   until Language 1.0 becomes the active seed, except for measured migration or
   verification blockers.

## Consequences

- The positive three-module fixture publishes a 568-byte WVIR 1.5 directory
  with five function entries, two blocks, six operations, five temporaries,
  three operands, and exactly one operation 171. The current WVB writer rejects
  it at function `2`, operation `6`, and leaves no output.
- Seven bounded mutations reject an older minor, unknown operation, primitive
  result, missing operand, consumed parent slot, wrong module, and swapped
  numeric limits. Four source cases reject immutable borrow, wrong limit width,
  wrong result, and the same-name wrong failure layout.
- The current analyzer is 1,144,757 bytes at SHA-256
  `384cb966d9b8718fda0c2e7bf3863ae168ce7d9fcb911d076b87d5e33400b0e3`
  with 549 functions and 938,146 code bytes.
- The current emitter is 1,078,300 bytes at SHA-256
  `215034c1149ee898ae4a9980bbe82326cb0d2a82fe7939e6191af64972a9af50`
  with 561 functions and 897,085 code bytes. Both source-build and native
  packaging paths complete under their retained bounds.
- The Language 1.0 owner adds 13 cases and declares 462 total. The 108-owner
  registry declares 5,246 cases at SHA-256
  `824e1b4fb800916b3f149c235e16d366c3213f25faa7f71fc35bce498b52fd18`.
- The compiler-generic-WIR regression deterministically advances to 1,315,395
  bytes at SHA-256
  `1da34176e4e17f395fadccfff9fe4f7f5e346ec2c919658744915ca86b7d6c19`;
  the current native verifier accepts it. The final Windows owner passes all 13
  phases and 462 cases in 725,200 ms, and its coordinator completes in 725,980
  ms.
- `Source-Wir-Core.wv` remains large, but the new proof is a cohesive named
  boundary rather than another inline block. A future file extraction should
  follow a real model/API ownership split and must not create numbered parts.

## Reconsideration triggers

Advance operation 171 into WVB only with exact provider state, debit/credit,
failure atomicity, child-count, teardown, and malformed-input contracts. Replace
the single-block proof with explicit control-flow ownership joins before Split
is admitted across branches or loops. Reprofile the compiler after Language 1.0
becomes the active seed, or earlier only when a reproducible blocker prevents a
migration slice from completing.
