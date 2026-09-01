# Decision 0909: lower mutable unsafe write-region borrowing to WVIR

## Status

Accepted and implemented locally on Windows on 2026-09-01.

This decision adds the typed compiler boundary for constructing a checked
mutable write region. It does not add a WVB opcode, expose an address, execute
a provider operation, derive a pointer, authenticate a Foreign call, or claim
cross-host qualification.

## Context

[Decision 0904](0904-Execute-Wvb-1.33-Unsafe-Scratch-In-A-Bounded-Scalar-Provider.md)
made bounded scratch ownership executable, and
[Decision 0907](0907-Observe-Immutable-Borrowed-Unsafe-Scratch-In-Wvb-1.35.md)
added a non-consuming length observation. The frozen Foundation surface also
defines `Borrowˉwriteˉregion`, but source binding and typed WVIR could not
represent that call. The compiler therefore had no explicit boundary at which
to prove its unsafe context, mutable borrow, range inputs, ABI relation, and
canonical typed failure before later executable lowering.

The result must remain contextual. Existing typed Foundation intrinsics use the
declared receiving type to reject a wrong generic Result before publication;
this operation follows the same rule. An unsafe block containing the declaration
preserves that context and also gives the future region borrow a lexical owner.

## Decision

1. Bind only the exact qualified generic call
   `Foundationˉunsafe.Borrowˉwriteˉregion::<Abi>`. Require the canonical
   edition-1 System module, one explicit declared ABI enum, four named
   arguments in the frozen order, and a lexical unsafe context.
2. Require `Scratch` to be written `borrow mut` over one directly named
   parameter or local with exact type `Foreignˉscratch<Abi>`. Reject immutable
   and by-value origins. Require `Start`, `Length`, and `Requiredˉalignment` to
   produce exact `u64` values.
3. Require the contextual result to be the exact canonical
   `Result<Foreignˉwriteˉregion<Abi>, Foreignˉpointerˉfailure>`. Check the
   edition, profile, module, generic arity, nominal identities, ABI arguments,
   opaque region layout, pointer-failure identity, and `unsafe.address` effect
   before publishing the operation.
4. Add typed WVIR operation `188`. Its three ordered operands are start, length,
   and required alignment. `Target` is the mutable-borrow scratch slot;
   `Auxiliary` is the ABI-enum shape; the result shape is the contextual generic
   Result.
5. Reserve WVIR 1.27 for operation `188` without generic specializations and
   WVIR 1.28 for the same operation with them. Either version may also contain
   the earlier scratch construction and observation operations. Lower versions
   reject operation `188`, and 1.27/1.28 require it.
6. Keep execution closed at this checkpoint. A later WVB decision must specify
   the instruction, mutable-borrow and region-lifetime proof, failure encoding,
   runtime/provider state, native representation, bounds, malformed-input
   cases, and relationship to pointer derivation before any address can exist.

## Implementation standing

The current source-built analyzer publishes operation `188` and validates the
new 1.27/1.28 directory family. Its rebuilt WVB is 1,639,438 bytes at SHA-256
`5b4f3b65bf16bf4e349b9e807b82237493e0a7fc2a24057c438d999452771672`.
The current emitter/independent-validator WVB is 1,502,152 bytes at SHA-256
`ecf4663d65e9cab7fae56fdfdac743fa369131bbe7067f52f44741555a56fcab`.

The focused local Windows matrix passes three valid cases and seven exact
rejections. It covers the canonical type surface, parameter and mutable-local
calls, missing unsafe context, immutable and by-value scratch origins,
mismatched result ABI, mismatched failure type, mismatched explicit ABI, and a
wrong alignment label.
The valid WVIR contains one operation `188`, three prior `u64` temporaries, the
mutable scratch slot, the declared ABI shape, and the private generic Result.
Seven malformed version, operation, operand-count, result, operand-range,
scratch-slot, and ABI mutations reject through independent WVIR validation.
The valid operation reaches source WVB as `Unsupportedˉoperation`, preserving
the deliberately closed executable boundary.

## Consequences

- The source compiler now has one explicit, reviewable boundary for checked
  mutable scratch-region formation.
- The result retains typed failure and ABI evidence without publishing a raw
  address or choosing a runtime representation.
- Mutable borrowing is admitted only for this exact intrinsic; it is not
  generalized to arbitrary records, calls, fields, collections, or escapes.
- WVB execution, affine region containment, provider/native implementation,
  `Writeˉpointer`, `Regionˉlength`, authenticated Foreign calls, a migrated real
  boundary, Linux reproduction, and paired-host qualification remain pending.

## Reconsideration triggers

Reconsider the contextual-result rule only as part of a general, specified
call-result inference design. Change the operation or version envelope only if
the executable representation cannot preserve exact failure, borrow, ABI,
lifetime, and alias semantics without weakening this compiler boundary.
