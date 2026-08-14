# Decision 0539: Bounded execution-context version 9 construction

- Date: 2026-08-13
- Status: Implemented candidate with focused Windows execution and Linux image evidence
- Requires: [Decision 0538](0538-First-Native-Capability-Provider-Call-Emission.md)
- Defines: [`WVXQ 2`, `WVXR 2`, and context 9](../../Specifications/Windvale-Native-Execution-Context-9-Construction.md)
- Retains: Native ABI 22, context 7, service-table 5, WVB 1.11, WVO 1.0, and all qualified ABI-22 artifacts
- Plans: Native ABI 23

## Context

The provider-call emitter loads `WVPT 1` from context offset 128. Decision 0151
already reserves offsets 112 and 120 for allocator state and its leaf. The
qualified context-7 constructor cannot append the provider pointer without a new
request version, exact size, pointer-presence rule, and post-call immutability
contract.

Changing the current constructor in place would mislabel old request bytes and
force every ABI-22 host/container consumer to advance before any fragment can
use provider dispatch. A parallel candidate constructor lets the next lowerer
and host integration be proven while retaining the qualified recovery path.

## Decision

- Add `WVXQ 2`, exactly 144 bytes, and `WVXR 2`, 32 bytes on failure or
  168 bytes on success.
- Retain all context-7 fields and relationships. Append allocator state at 112,
  allocator leaf at 120, and `WVPT 1` pointer at 128 in context version 9, size
  136.
- Require both allocator fields to remain zero until their independent execution
  contract is published. Reject rather than silently ignore nonzero reservations.
- Add request flag bit 5 for provider-table presence and require exact agreement
  with the opaque pointer at request offset 136.
- Keep the provider pointer immutable for the entire call. Portable construction
  never dereferences it; the host separately proves table bytes, WVB identity
  agreement, allocation lifetime, and teardown.
- Preserve the existing three mutable post-call fields and every current arena,
  budget, argument, and fixed-table rule.
- Keep `WVXQ/WVXR 1`, context 7, and ABI 22 unchanged. This decision does not
  select ABI 23 in the main lowerer or host executor.

## Evidence

The core and bridge build through the native Project 2 front door to exact WVBs
of 5,986 and 5,979 bytes. The focused test constructs the valid provider-bearing
request twice and proves byte identity, then rejects physical and declared size,
magic, version, flags, reserved state, zero budgets, service mismatch, arena
bounds, argument mismatch, fixed-table mismatch, both allocator reservations,
and both provider-presence directions.

The test WVB is 13,833 bytes at SHA-256
`2da0ea6deb6a00d722300d05b6a10a46d2ae91b01a029807a3279fee71d69b17`.
Its verified ABI-22 test object is 140,298 bytes at SHA-256
`2244b1a5fd398d933690187639a06e694ee8fef89ada5dfbc244bf45855b5ac7`.
The Windows native package returns zero and the same fragment constructs a Linux
package. The fixed packager still requires an exercised nominal type; the test's
one-member enum records that existing limitation without changing the context
contract.

## Consequences

- The successor provider call now has a concrete, validated context owner rather
  than an unconstructed pointer offset.
- ABI-23 integration can distinguish missing provider state from malformed
  context and can keep allocator publication independent.
- No current ABI-22 artifact, execution path, or recovery constructor changes.
- Main-lowerer capability analysis/emission, fragment verification, host
  execution, and storage leaves remain open.

## Reconsideration triggers

Revisit if allocator integration must advance before provider dispatch, a host
cannot preserve the append-only layout, multiple provider tables are required
before typed capability values, or post-call verification needs a mutable field
not represented here.
