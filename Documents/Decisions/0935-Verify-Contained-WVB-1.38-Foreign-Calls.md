# Decision 0935: verify contained WVB 1.38 Foreign calls

## Status

Accepted and implemented locally on Windows on 2026-09-03. This decision admits
the exact candidate WVB 1.38 registered Foreign call through the complete
compiler-aligned verifier. It does not bind a provider, resolve a native symbol,
form a host address, invoke Foreign code, migrate a real boundary, complete
Slice 8, or claim Linux or paired-host qualification.

## Context

[Decision 0934](0934-Represent-Paired-Foreign-Calls-In-Candidate-Wvb-1.38.md)
made the authenticated production path emit candidate opcode `E0`, but retained
only an independent structural reader and deliberately kept the complete
verifier and all execution consumers closed.

The next boundary is bytecode containment. Before any provider can interpret a
registered binding, the verifier must independently reconstruct the exact
pointer/ABI relationship, prove typed operand order, consume the pointer as an
affine value, and prevent it from escaping or being reused. Static verification
can establish that the capacity and expected-generation operands are exact
`u64` values; only a later runtime provider can compare the generation value
with live provider state.

## Decision

1. Admit WVB minor `1.38` through the compiler-aligned metadata, structural,
   semantic, typed-stack, control-flow, and ownership passes while preserving
   every inherited WVB 1.11-through-1.37 rule.
2. Require System profile metadata, one through 4,096 exact `E0` instructions,
   and registered binding identity `1`. Reject `E0` under every earlier minor.
3. Require each instruction to name an exact canonical
   `Foreignˉpointer<u8, Abi>` record and a kind-`2` or kind-`7` ABI enum. Retain
   at most 256 distinct pointer/ABI relations and require agreement with any
   inherited `DF` relation for the same pointer.
4. Consume verifier-internal affine pointer kind `38`, `u64` capacity, and
   `u64` expected generation in that order and produce exact `i64`.
5. Permit a pointer-local load only as the first of exactly three consecutive
   local loads immediately before a matching `E0`. Track every pointer local in
   the existing bounded ownership proof so reuse, overwrite, return, ordinary
   call escape, forward-join disagreement, and backedge disagreement reject.
6. Retain the published native front door and every scalar, native, launcher,
   browser, WebAssembly, package, and operating-system execution consumer at
   its narrower version boundary.
7. Extend the production-ingress owner so the current source-built verifier
   accepts the canonical authenticated publication and rejects the seven
   structural mutations plus pointer, capacity, and generation stack-kind
   substitutions.
8. Preserve the native lowerer's fewer-than-2,048-local function bound. Extract
   System-operation selection from the main executable-verifier pass instead of
   widening that implementation limit.

## Implementation standing

Implementation commit `1e12b2f24f42a121a0d10c6e908592d15bbd4b9a`
passes the 21-case `language-1-production-admission-ingress` owner in 162.788
seconds on the local Windows host. The source-built verifier is 502,386 bytes at
SHA-256
`742cb07b7351473c188d9247eb11be5ef39b2a522c09e89b9f97b5e2886651b4`.
Its packaged Windows application is 4,063,232 bytes at SHA-256
`8076bc5c0a289e87bf883cf36b25c2481a40cc17a58c07ec32a3ee5bac6fe86d`.

The exact commands, product identities, mutation classes, and limitations are
recorded in the
[compiler-aligned WVB 1.38 evidence](../Evidence/2026-09-03-Compiler-Aligned-WVB-1-38-Foreign-Call-Containment.json).

## Consequences

- Candidate WVB 1.38 is no longer rejected merely because its version is newer;
  admission now proves the exact registered call shape and affine non-escape.
- Verification does not authenticate how a WVB was produced and does not grant
  library, symbol, address, or call authority.
- Expected generation remains an explicit dynamic input rather than a value the
  static verifier pretends to know.
- The next checkpoint is a bounded runtime/provider implementation for binding
  identity `1`, followed by native symbol resolution and ABI invocation. One
  migrated real boundary and final paired-host qualification remain later gates.

## Reconsideration triggers

Revisit this containment profile before execution if a provider needs a
different operand order, a second registered binding needs additional immutable
metadata, legitimate compiler output needs nonconsecutive operand loads, or a
real call requires multiple simultaneously live pointers. Any replacement must
retain bounded relation construction, affine ownership, exact ABI identity,
malformed-input rejection, and fail-closed execution consumers.
