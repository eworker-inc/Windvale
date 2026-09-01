# Decision 0903: verify candidate WVB 1.33 without opening execution

## Status

Accepted and implemented locally on Windows on 2026-09-01.

This decision admits candidate WVB 1.33 to the compiler-aligned verifier. It
does not admit WVB 1.33 to a scalar runtime, native lowerer, launcher, package
execution path, browser, WebAssembly host, or Windvale OS consumer, and it does
not complete Slice 8 or claim paired-host qualification.

## Context

[Decision 0902](0902-Represent-Unsafe-Scratch-Construction-In-Candidate-Wvb-1.33.md)
selected the exact `DC` (`unsafe.scratch.construct`) byte representation but
kept the complete verifier and all execution consumers closed. A format-only
reader could prove section geometry and immediate categories, but it did not
prove typed stack behavior, canonical Foundation layouts, affine budget and
result ownership, or control-flow agreement.

Opening a runtime before those proofs would let execution infer foreign-memory
authority from partially checked bytes. Keeping the complete verifier closed
would instead prevent the runtime and native checkpoints from sharing one
trusted input contract. The next coherent boundary is therefore complete
verification without execution.

## Decision

1. Extend the compiler-aligned verifier's structural, semantic, typed-stack,
   and ownership phases through WVB minor `33`. Preserve every inherited WVB
   1.11-through-1.32 rule.
2. Require WVB 1.33 module metadata to select system profile `3`. The source
   authority and platform declarations remain separately validated metadata;
   profile selection is not an authority grant.
3. Recognize opcode `DC` only under minor `33`, with exact width `13`, exactly
   two ordered `u64` stack operands, and three valid `u32` immediates.
4. Require the budget immediate to name an available shape-`25` parameter or
   affine local in the same function. Consume that owner in the control-flow
   ownership proof.
5. Require the result immediate to name the exact materialized
   `Result<Foreignˉscratch<Abi>, Foreignˉmemoryˉfailure>` layout. Verify the
   synthesized generic identities, scratch token field, Result cases and
   fields, foreign-memory failures, nested allocation failure, reason enum,
   field types, and member values.
6. Require the ABI immediate to name a kind-`2` or kind-`7` enum. The opcode is
   the serialized binding between its opaque scratch nominal and that ABI;
   reject a module that binds the same scratch nominal to two ABI enums.
7. Bound one module to at most 4,096 `DC` instructions and 256 distinct
   scratch-nominal/ABI bindings. Keep only the bounded binding directory during
   verification.
8. Treat the constructed Result, its Valid scratch payload, and direct scratch
   records as affine owners in calls, returns, variant operations, local
   stores, loads, and takes. Ordinary record construction and ordinary record
   field observation cannot manufacture or expose the opaque scratch token.
9. Preserve the inherited exact
   `Result<Memoryˉbudget, Allocationˉfailure>` payload exception. The WVB 1.33
   shape-`25` widening does not otherwise permit budget tokens in nominal
   payloads, collections, function results, or Types entries.
10. Require at least one `DC` in every WVB 1.33 module. Continue rejecting `DC`
    under every earlier minor and rejecting minor `33` at every current
    execution consumer.
11. Extend the focused unsafe-scratch oracle so the packaged compiler verifier
    must accept the canonical candidate and reject malformed versions,
    opcodes, indices, kinds, budget shapes, synthesized identities, and
    canonical failure-layout names.

## Implementation standing

The source-built compiler-aligned verifier builds as a 120-function WVB and
packages as a hosted Windows executable. The focused oracle accepts the
deterministic 1,123-byte candidate and rejects six structurally malformed WVB
mutations plus three semantic forgeries. Retained WVB 1.11, 1.30, and 1.31
samples remain accepted.

The current native front-door runner reports the WVB 1.33 candidate as
unsupported, and the previously published front-door verifier remains closed
to it. That fail-closed result is intentional until a separately reviewed
execution checkpoint replaces it.

## Consequences

- Runtime and native work can consume one completely verified WVB 1.33 shape
  instead of repeating source-level inference.
- Accepting bytes in the compiler verifier does not allocate memory and does
  not make the candidate executable.
- The serialized ABI immediate defines the ABI associated with an opaque
  scratch nominal inside WVB. The verifier checks consistency across all uses;
  it does not reconstruct erased source generic arguments from generated type
  names.
- The verifier's scratch/ABI relation work is explicitly bounded and cannot
  grow with an unbounded number of instructions or retained relations.
- Existing WVB 1.11-through-1.32 artifacts retain their prior semantics and
  version gates.
- The next Slice 8 checkpoint is bounded runtime/provider allocation, exact
  failure and zeroing behavior, and affine teardown. Native lowering and
  containment follow that runtime oracle.

## Reconsideration triggers

Reconsider this boundary if WVB later preserves generic arguments directly in
its Types directory, if more than 256 distinct scratch/ABI bindings are needed
by a real bounded workload, or if runtime work shows that the verified affine
result cannot be torn down without another explicit bytecode operation. Do not
open execution by weakening the verifier or by treating profile selection as
ambient authority.
