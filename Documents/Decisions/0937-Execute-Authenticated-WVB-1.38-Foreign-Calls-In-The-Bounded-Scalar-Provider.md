# Decision 0937: execute authenticated WVB 1.38 Foreign calls in the bounded scalar provider

## Status

Accepted and implemented locally on Windows on 2026-09-03. This decision opens
only the source-built bounded scalar provider to the exact registered WVB 1.38
binding. It forms no native address, resolves no symbol, loads no dynamic
library, invokes no external machine code, and does not complete Slice 8 or
claim Linux or paired-host qualification.

## Context

[Decision 0935](0935-Verify-Contained-WVB-1.38-Foreign-Calls.md) made the
complete compiler-aligned verifier consume the Foreign pointer affinely, but
kept every execution consumer closed. The next checkpoint must prove the
dynamic provider contract without weakening the static containment proof or
pretending that a logical scratch descriptor is a host pointer.

The registered paper buffer-source contract has useful success and stale-
generation outcomes. A bounded in-process provider can exercise the emitted
operand order, live-allocation checks, exact writes, signed status result, and
teardown before native ABI work introduces symbol and address authority.

## Decision

1. Admit WVB 1.38 in the source-built runner only after the existing complete
   metadata, semantic, typed-stack, control-flow, and ownership verifier passes.
2. Execute only registered binding identity `1`. Retain the exact canonical
   `Foreignˉpointer<u8, Abi>`, ABI enum, three-argument order, and `i64` result
   required by opcode `E0`.
3. Treat the pointer as a private `{allocation offset, length}` descriptor. The
   provider requires one live 64-byte allocation, 64-byte capacity, eight-byte
   alignment, and matching pointer owner. It never converts the descriptor to a
   native address.
4. Use provider generation `42`. When the expected generation is `42`, write
   the exact 24-byte buffer-source record and return status `24`. Otherwise,
   write the observed `u64` generation and return status `-3`.
5. Preserve the 64-byte allocation, execution budget, affine pointer
   consumption, and ordinary invocation teardown bounds. Malformed or
   mismatched provider state fails execution rather than widening the binding.
6. Add the inherited `i64.const`, `i64.negate`, `i64.equal`, and
   `i64.not-equal` scalar operations needed by the generated success and stale
   paths; retain their ordinary checked scan and typed execution rules.
7. Exercise both provider outcomes through the complete production sequence:
   authenticated source admission, generic-aware analysis, retained WVFB
   pairing, WVB 1.38 emission, compiler-aligned verification, and source-built
   runner execution.
8. Keep the native lowerer, launchers, browser, WebAssembly host, packages, and
   Windvale OS at their declared narrower execution boundaries.

## Implementation standing

Implementation commit `98334495cc7c501e1262a5939ebf68f473e55745` passes all 23
cases of `language-1-production-admission-ingress` in 158.741 seconds on the
local Windows host. Both real generated WVB 1.38 programs return `Result: 42`:
one observes the 24-byte success result for generation `42`, and one observes
the `-3` stale result for generation `41`.

The source-built runner WVB is 1,040,878 bytes at SHA-256
`4e50301efe5e2260608eb994f21ece89e83ad102aac28cebb705d35d06e3d86b`.
Its measured local Windows profile-5 application is 10,547,712 bytes at
SHA-256
`8942e7c0a17182ff15ed79eaf63f7aeb8a8ab7cd4cde5015cd489612c3494972`.
These are development measurements, not promoted paired-host identities.

The exact command, artifact identities, cases, and limitations are recorded in
the [authenticated Foreign scalar-execution evidence](../Evidence/2026-09-03-Authenticated-Foreign-Scalar-Execution.json).

## Consequences

- Candidate WVB 1.38 now has a complete Windows source-to-scalar-execution
  checkpoint for both defined provider outcomes.
- This checkpoint validates dynamic generation and destination rules without
  granting native symbol, library, loader, or address authority.
- The native x86-64 path remains at WVB 1.37. Native ABI lowering and the first
  real external-library boundary are the next Slice 8 implementation work.

## Reconsideration triggers

Revisit the scalar provider before registering another binding, accepting a
different buffer geometry, exposing more provider state, allowing retained
pointers, or sharing implementation with a native loader. Any replacement must
retain complete verification before dispatch, explicit identity matching,
bounded allocation checks, exact partial-result behavior, affine non-escape,
and deterministic teardown.
