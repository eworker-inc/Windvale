# Decision 0918: verify WVB 1.37 write-pointer lifetime containment

## Status

Accepted and implemented locally on Windows on 2026-09-02. This decision admits
the exact candidate WVB 1.37 write-pointer derivation to the complete
compiler-aligned verifier. It does not execute pointer derivation, form or
expose a native address, authenticate a Foreign call, complete Slice 8, or claim
Linux or paired-host qualification.

## Context

[Decision 0916](0916-Represent-Contained-Write-Pointer-Derivation-In-Candidate-Wvb-1.37.md)
gave typed WVIR operation `189` an exact candidate representation as WVB 1.37
opcode `DF`, but deliberately kept the complete verifier and every execution
consumer closed. The representation preserved the direct region, pointer, and
ABI identities; it did not prove that the opaque pointer stayed within the
region borrow.

The next smallest boundary is a complete bytecode lifetime proof. Address
formation and a Foreign call must remain later decisions so an admitted WVB
cannot gain native authority merely because its type and ownership evidence is
valid.

## Decision

1. Admit WVB minor `1.37` through the compiler-aligned metadata, structural,
   semantic, typed-stack, control-flow, and ownership verifier while preserving
   every inherited WVB 1.11-through-1.36 rule.
2. Require one through 4,096 exact `DF` instructions in a minor-37 module. Each
   instruction names an exact immutable shape-`28`
   `Foreignˉwriteˉregion<Abi>` parameter, a distinct canonical
   `Foreignˉpointer<u8, Abi>` record, and a kind-`2` or kind-`7` ABI enum.
3. Build an explicit region/pointer/ABI relation directory. Limit it to 256
   entries and 3,072 bytes, require every repeated nominal role to agree, and
   require consistency with inherited scratch/region/ABI evidence.
4. Represent the pointer only inside the verifier as affine stack kind `38`.
   Ordinary record operations do not construct or expose it.
5. Admit direct discard or the exact compiler-generated sequence that stores
   the produced pointer, immediately loads that same available local, and stores
   it into a distinct exact pointer local. Treat the load as a consuming move.
6. Reject `local.take`, a load from an unavailable pointer local, a mismatched
   pointer nominal, construction, embedding, call or return escape, and any
   forward-join or backedge ownership disagreement. The immutable source region
   remains available because `DF` observes rather than consumes its borrow.
7. Keep the scalar provider, native lowerer, launcher, published front door,
   browser, WebAssembly host, package consumers, and Windvale OS closed to minor
   `37` until a separate execution decision defines private address state and
   complete teardown.
8. Retain the independent byte reader and add semantic mutations for
   `local.take`, call escape, and loading an unavailable pointer local. A focused
   run must also retain the inherited WVB 1.36 write-region containment cases.

## Implementation standing

Implementation commit
`e54087aa48d22dd369fb03a1f0c748d42f10f929` produces a 494,934-byte
source-built verifier at SHA-256
`3108026889d28c5088e14c2fa73fe8b24e190ad94695030cee3959d697642c2d`.
Its packaged Windows executable is 3,996,160 bytes at SHA-256
`5ee4f626be8c8dc4f638d146fc99e506318f230160d641b209a5a4b3bfee907b`.
An immediate second source build produces byte-identical WVB.

The focused matrix covers 22 source cases, seven malformed write-region WVIR
cases, seven malformed pointer WVIR cases, five malformed WVB 1.36 cases, ten
malformed WVB 1.37 cases, and 25 compiler-verifier decisions. The exact commands,
artifact identities, and limitations are recorded in the
[compiler-aligned WVB 1.37 evidence](../Evidence/2026-09-02-Compiler-Aligned-WVB-1-37-Write-Pointer-Containment.json).

## Consequences

- Candidate WVB 1.37 is no longer rejected merely because its version is newer;
  admission now proves exact pointer identity and conservative non-escape.
- The pointer remains opaque and affine. Verification does not expose its
  representation, form a host address, or grant dereference or call authority.
- The compiler-generated pointer-local move has an explicit ownership meaning
  instead of inheriting copying behavior from ordinary record loads.
- The next implementation boundary is private provider/native address formation
  followed by one authenticated no-retain Foreign call. Linux reproduction and
  final paired-host qualification remain later gates.

## Reconsideration triggers

Reconsider this containment profile before execution if a real Foreign-call
lowering requires a different affine transfer sequence, multiple live derived
pointers, a region local rather than the direct borrowed parameter, or stronger
control-flow precision. Any replacement must preserve explicit region/pointer/
ABI identity, bounded verification, malformed-input rejection, and fail-closed
execution consumers.
