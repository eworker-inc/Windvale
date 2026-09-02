# Decision 0916: represent contained write-pointer derivation in candidate WVB 1.37

## Status

Accepted and implemented as a source-publication candidate on 2026-09-02.
This decision does not admit WVB 1.37 to the complete verifier, execute pointer
derivation, form or expose a native address, authenticate a Foreign call,
complete Slice 8, or claim Linux or paired-host qualification.

## Context

[Decision 0914](0914-Lower-Canonical-Unsafe-Write-Pointer-To-Wvir.md)
stopped exact `Writeˉpointer::<Abi>` at typed WVIR operation `189`. That
preserved the immutable region borrow, canonical opaque pointer result, and ABI
identity while executable lifetime containment and address formation remained
unresolved.

The next smallest boundary is deterministic serialization. Reusing an ordinary
record load or constructor would erase the relation between the borrowed region
and the opaque pointer. Opening verification or execution at the same time would
combine byte representation, affine lifetime proof, provider address state,
native range checks, and Foreign-call authentication in one review boundary.

## Decision

1. Reserve candidate WVB minor `1.37` for contained write-pointer derivation. A
   source module containing WVIR operation `189` selects minor `37`.
2. Assign opcode byte `DF` (`223`) to `unsafe.write-pointer.borrow`.
3. Encode exactly three little-endian `u32` immediates: the directly borrowed
   region parameter or local index, canonical `Foreignˉpointer<u8, Abi>`
   record-type index, and explicit ABI-enum type index. The instruction is 13
   bytes, consumes no operand-stack values, and produces the exact opaque
   pointer value for later typed verification.
4. Encode the exact immutable `Foreignˉwriteˉregion<Abi>` parameter through
   nominal borrowed-record shape `28`. The writer admits that parameter only
   when the same function directly targets it with operation `189`; ordinary
   region parameters and other uses remain unsupported.
5. Require the region nominal and pointer nominal to be distinct kind-`1`
   records and the ABI immediate to identify a kind-`2` or kind-`7` enum. The
   source and WVIR phases continue to own the complete canonical Foundation,
   `u8`, generic-arity, opacity, borrow-mode, effect, and ABI relationship.
6. Preserve the seven-section envelope and every inherited WVB 1.11-through-1.36
   encoding. A minor-37 module contains at least one `DF`; an earlier minor may
   not contain it.
7. Keep the complete compiler verifier, scalar provider, native lowerer,
   launcher, published front door, browser, WebAssembly host, and Windvale OS
   consumers closed to minor `37` until separate decisions prove and implement
   pointer lifetime containment.
8. Test publication with the bounded source/WVIR oracle plus an independent WVB
   reader. Reject an old minor, unknown opcode, invalid region local, invalid
   pointer or ABI type, non-borrowed region shape, and aliased region/pointer
   nominal.
9. Treat provider address representation, checked base-plus-offset formation,
   pointer escape, region release, no-retain call authentication, and callee
   behavior as pending execution semantics rather than implicit properties of
   opcode `DF`.

## Implementation standing

Implementation commit
`bb9a8b456019a0f90f7afdc6b2ad9974e9cbb503` publishes one exact pointer WVB in
the 22-case focused matrix. Seven malformed pointer WVIR mutations and seven
malformed pointer WVB mutations reject independently. The complete source-built
compiler verifier is deliberately supplied to the same run: it accepts the
inherited valid WVB 1.36 cases and rejects canonical and malformed WVB 1.37.
The older execution front door also remains closed.

The analyzer reconstructs to 1,647,988 WVB bytes at SHA-256
`904a11ba14d70239a09f63b483464ddb4a623c42978462dd73818a1d5fa18dde`.
The emitter reconstructs to 1,514,261 WVB bytes at SHA-256
`22e3c08884df6eaf78ad1d1f940b40c9a1c8b3dc7feafbb2aa439d964e8192f0`.
The closed compiler verifier reconstructs to 473,783 WVB bytes at SHA-256
`76c36d5a341a37e14a162427bef2870ed498cdbf4366abcebb703f8f79f32c7d`.
The exact commands and limitations are recorded in the
[candidate WVB 1.37 evidence](../Evidence/2026-09-02-Candidate-WVB-1-37-Contained-Write-Pointer.json).

## Consequences

- The compiler no longer loses a valid typed write-pointer derivation at WVB
  publication.
- Region, pointer, and ABI identities remain direct bytecode operands rather
  than host conventions or inferred layout.
- A WVB 1.37 candidate is non-executable. Independent byte reading proves the
  proposed representation and malformed-input boundary, not lifetime safety or
  address validity.
- The next checkpoint is compiler-aligned verification with a bounded
  region-to-pointer non-escape proof. Provider and native address formation and
  authenticated no-retain Foreign calls follow only after that proof exists.

## Reconsideration triggers

Reconsider the three-immediate encoding only before a promoted verifier or
runtime accepts WVB 1.37, and only if the replacement preserves explicit
region, pointer, ABI, borrow, opacity, deterministic-byte, and malformed-input
evidence. Do not renumber the accepted opcode after external WVB 1.37 artifacts
are published.
