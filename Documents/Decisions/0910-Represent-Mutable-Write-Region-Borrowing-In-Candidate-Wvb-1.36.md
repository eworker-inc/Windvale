# Decision 0910: represent mutable write-region borrowing in candidate WVB 1.36

## Status

Accepted and implemented as a source-publication candidate on 2026-09-01.
This decision does not admit WVB 1.36 to the complete verifier, execute a
write-region borrow, expose a pointer, complete Slice 8, or claim paired-host
qualification.

## Context

[Decision 0909](0909-Lower-Mutable-Unsafe-Write-Region-Borrowing-To-Wvir.md)
stopped exact `Borrowˉwriteˉregion::<Abi>` at typed WVIR operation `188`.
That preserved the mutable scratch origin, three checked scalar arguments,
canonical Result, and ABI identity while verifier lifetime containment was
still unresolved.

The next smallest boundary is deterministic serialization. Reusing ordinary
record, allocation, or call instructions would erase the exclusive borrow and
let later consumers infer authority from an opaque nominal shape. Opening
execution at the same time would instead require one change to define bytes,
prove non-escape, implement alias state, construct exact failures, and manage
teardown. Those are separate review boundaries.

## Decision

1. Reserve candidate WVB minor `1.36` for mutable unsafe write-region
   serialization. A source module containing WVIR operation `188` selects
   minor `36`.
2. Assign opcode byte `DE` (`222`) to `unsafe.write-region.borrow`.
3. Consume exactly three ordered `u64` stack operands: `Start`, `Length`, then
   `Requiredˉalignment`.
4. Encode exactly three little-endian `u32` immediates: the direct scratch
   local index, canonical
   `Result<Foreignˉwriteˉregion<Abi>, Foreignˉpointerˉfailure>` type index,
   and explicit ABI-enum type index. The instruction is 13 bytes.
5. Admit an exact `Foreignˉscratch<Abi>` parameter borrowed mutably through
   the existing nominal borrowed-record shape `28`. Ordinary mutable owned
   parameters remain invalid. The `DE` instruction, not shape `28` alone,
   records that this use requires exclusive mutable borrowing.
6. Classify the exact Result produced by operation `188` as affine in the
   writer so compiler-generated stores and inspections use move/take behavior
   instead of ordinary copying.
7. Preserve the seven-section WVB envelope and every inherited WVB
   1.11-through-1.35 encoding.
8. Keep the complete compiler verifier, front-door verifier, scalar runner,
   native lowerer, launcher, browser, WebAssembly host, and Windvale OS
   consumers closed to minor `36` until separate decisions prove and implement
   lifetime containment.
9. Test publication with the bounded source/WVIR oracle plus an independent
   WVB reader that checks the exact header, sections, opcode, local, result,
   and ABI categories. Reject old-minor, unknown-opcode, invalid-local,
   invalid-result, and invalid-ABI mutations.
10. Treat range, address-width, alignment, lifetime, alias, region release,
    pointer derivation, and provider behavior as pending execution semantics,
    not compile-time literal restrictions or implicit traps.

## Implementation standing

The current split compiler publishes two valid candidate WVB 1.36 modules and
the focused oracle rejects one unused mutable-scratch writer case, seven
malformed WVIR cases, and five malformed WVB cases.
The canonical borrowed-parameter fixture is 1,300 bytes with SHA-256
`c116d5d4a8ec84afe8321da4403e5071cf981f5bb6a9121d0660b7fa33d64eec`.
The current front-door verifier rejects it in the semantic phase, which is the
intended fail-closed result.

## Consequences

- The compiler no longer loses a valid mutable write-region operation at WVB
  publication.
- The scratch origin, result identity, and ABI remain direct bytecode operands
  rather than reconstructed conventions.
- A WVB 1.36 candidate is non-executable. Acceptance by the independent byte
  reader proves deterministic representation, not lifetime safety.
- The next checkpoint is compiler-aligned verification with a bounded affine
  non-escape and scratch-exclusivity proof. Scalar/provider and native
  execution follow only after that proof exists.

## Reconsideration triggers

Reconsider the three-immediate encoding only before a promoted verifier or
runtime accepts WVB 1.36, and only if the replacement preserves explicit
scratch, Result, ABI, ownership, deterministic-byte, and malformed-input
evidence. Do not renumber the accepted opcode after external WVB 1.36
artifacts are published.
