# Decision 0832: Bind the canonical Foundation memory-budget identity

## Status

Accepted on 2026-08-22.

## Context

Public fallible collection construction must receive explicit finite allocation
authority. The frozen Foundation contract names that authority
`Foundationˉmemory.Memoryˉbudget`, but the source compiler previously had no
non-forgeable type identity for it. Treating a user record with the same member
spelling as a budget would expose representation, permit ambient construction,
and make later allocation effects only documentary.

The first implementation checkpoint needs to establish exact identity and
ownership before it introduces a runtime representation, launcher transfer,
`Split`, allocation leases, or fallible Vector construction. It must also avoid
pretending that source-set name matching authenticates a package; package locks
and admission remain responsible for selecting the trusted Foundation source.

## Decision

1. Edition-1 source recognizes only the qualified member
   `Foundationˉmemory.Memoryˉbudget`, reached through an admitted import whose
   target module header is exactly `Foundationˉmemory`. An unqualified spelling
   and the same member spelling in another module are unknown types.
2. The type is compiler supplied and representation hidden. The
   `Foundationˉmemory` source module does not publish a record or constructor
   that can manufacture budget authority.
3. The compiler uses private fixed shape `0x30000000` (`805306368`) and an
   internal named/end sentinel to carry this identity through binding and WIR.
   Neither value is a Language 1.0 ABI, a public declaration kind, or a WVB
   encoding.
4. WIR classifies the shape as owned. An explicit immutable borrow may observe
   it, but a borrowed budget cannot satisfy a consuming by-value parameter.
   Record fields reject the shape in this checkpoint so the opaque authority
   cannot be embedded through an unsupported storage representation.
5. WVB emission rejects the shape as `Unsupportedˉshape` without publishing a
   partial module. Runtime representation, moves, destruction, and launcher or
   parent-domain transfer must be specified and implemented together before
   this gate can advance.
6. `Memoryˉbudget.Split`, allocation effects, `Allocationˉreason`,
   `Allocationˉfailure`, leases, provider accounting, and public fallible
   collection operations remain later connected checkpoints. In particular,
   the frozen exact enum backing syntax and allocation-effect syntax are not
   approximated with current Seed declarations.

## Consequences

- Five cross-host front-door cases cover exact qualified analysis, immutable
  observation, rejection of borrowed consumption, unqualified and lookalike
  rejection, and explicit WVB non-publication.
- The maintained split products are a 1,114,218-byte analyzer at SHA-256
  `640ba1a9714979927433fa4936c73fa164b83f33ad22c794e86092ee8e17faa8`
  and a 1,029,551-byte emitter at SHA-256
  `a3bdffe028b2d4268358324a9b9a13aba2841730dd8f7334c4512a4f312827eb`.
- The Language 1.0 owner advances from 412 to 417 cases. The 108-owner registry
  advances to 5,201 declared cases at SHA-256
  `bdff820b2e13034763962928b1c162e22f9852102ccb60dd5bb04f525c4c173d`.
- Exact module-header matching prevents accidental lookalikes but is not a
  cryptographic package identity. Admission and the project source lock remain
  the trust boundary for the canonical Foundation dependency.
- `Source-Wir-Core.wv` reaches 12,612 lines, but this checkpoint adds only the
  fixed-shape validity and ownership classifications there. A later refactor
  should extract a cohesive type/ownership or collection-lowering phase with an
  explicit contract, not create numbered fragments.

## Reconsideration triggers

Revisit the private shape when the first serialized/runtime representation is
designed, or if package admission gains a canonical declaration-identity token
that can replace exact module-header matching without weakening deterministic
source analysis.
