# Decision 0914: lower canonical unsafe write pointer to WVIR

## Status

Accepted and implemented locally on Windows on 2026-09-02.

This decision adds the typed compiler boundary for deriving the canonical
`Foreignˉpointer<u8, Abi>` identity from an already verified write region. It
does not serialize the operation to WVB, form or expose a native address,
execute a provider or native operation, authenticate a Foreign call, migrate a
runtime or operating-system boundary, or claim Linux or paired-host
qualification.

## Context

[Decision 0913](0913-Lower-Compiler-Verified-WVB-1.36-Write-Regions-To-Native-X64.md)
made exact write-region validation executable in compiler-verified native
x86-64 code while deliberately retaining only private logical geometry. The
next smallest compiler boundary is the exact source and typed-WVIR operation
that requests a pointer from that contained region. Opening WVB or execution at
the same time would combine type identity, borrow containment, address
formation, runtime representation, and Foreign authority in one review step.

The existing write-region checkpoint uses an explicitly generic qualified call
and a contextual result to keep generic inference out of the unsafe boundary.
Pointer derivation follows that rule until a general specified call-result
inference design replaces it.

## Decision

1. Bind only the exact qualified call
   `Foundationˉunsafe.Writeˉpointer::<Abi>`. Require the canonical edition-1
   System module, one explicit declared ABI enum, and a lexical unsafe context.
2. Require the named argument `Region` to be written as an immutable borrow of
   one directly named parameter or local with exact type
   `Foreignˉwriteˉregion<Abi>`. Reject by-value, mutable-borrow, wrong-label,
   wrong-ABI, and non-name arguments.
3. Require the contextual result to be the exact canonical
   `Foreignˉpointer<u8, Abi>`. Independently check the module, profile, record,
   generic arity, `u8`, ABI, and one-field opaque layout before admitting it.
4. Add typed WVIR operation `189`. It has zero operands. `Target` is the
   region parameter or local slot, `Auxiliary` is the ABI-enum shape, and the
   result is the canonical private generic pointer shape. The operation
   contributes `unsafe.address`.
5. Reserve WVIR 1.29 for operation `189` without generic specializations and
   WVIR 1.30 for the same feature with the specialization envelope. Either may
   contain earlier scratch, write-region, task, callable, collection, or scalar
   operations. Lower versions reject operation `189`; 1.29/1.30 require it.
6. Retain the function-type length/version header in the 1.29/1.30 family even
   when its catalog length is zero, matching the existing task and unsafe
   families. Do not relax the four-mebibyte WVIR bound or the 256-entry native
   static-data bound.
7. Keep WVB publication and execution closed. The source-WVB backend must
   report valid analysis followed by `Unsupportedˉoperation` for operation
   `189`. A later decision must specify the WVB instruction, affine region and
   pointer lifetime evidence, runtime/provider representation, native address
   checks, and authenticated no-retain call relation.

## Implementation standing

The focused Windows matrix passes seven valid cases and fifteen exact source
rejections across the inherited write-region and new pointer paths. The pointer
cases cover the canonical call plus missing unsafe context, by-value and mutable
borrows, wrong region ABI, wrong result ABI or element, wrong explicit ABI, and
a wrong argument label. Seven malformed pointer WVIR mutations reject through
the independent emitter validator; the inherited seven WVIR and five WVB
write-region mutations remain passing.

The current analyzer reconstructs to 1,647,988 WVB bytes at SHA-256
`904a11ba14d70239a09f63b483464ddb4a623c42978462dd73818a1d5fa18dde`.
The current emitter reconstructs to 1,509,519 WVB bytes at SHA-256
`deb65933e8ed643ea74df64980e4f1c60de34219f2f2df861b779a52efd17bbc`.
Each product is reproduced byte for byte when the reconstructed pair builds its
own project. The analyzer remains exactly at the native staging limit of 256
static-data entries; the new callable name therefore uses bounded direct UTF-8
comparison instead of adding a redundant static literal.

## Consequences

- Source analysis and independent WVIR validation now have one exact boundary
  for canonical write-pointer derivation.
- The region remains an immutable borrow target and is neither consumed nor
  exposed through ordinary construction or field access.
- No executable pointer exists yet. WVB, verifier, scalar provider, native
  lowering, authenticated Foreign calls, and real boundary migration remain
  separate work.
- Ordinary generic inference remains incomplete for this call; the current
  checkpoint requires `::<Abi>` and an exact contextual result.
- Linux reconstruction and execution remain required before paired-host
  qualification.

## Reconsideration triggers

Replace the contextual-result restriction only through a general specified
generic-call inference design. Change operation `189` or the WVIR 1.29/1.30
envelope only if executable lifetime evidence cannot preserve the same exact
region, element, ABI, opacity, effect, and no-retain relations.
