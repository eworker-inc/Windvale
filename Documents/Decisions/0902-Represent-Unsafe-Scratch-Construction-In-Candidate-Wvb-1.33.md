# Decision 0902: represent unsafe scratch construction in candidate WVB 1.33

## Status

Accepted and implemented as a source-publication candidate on 2026-08-31.
This decision does not admit WVB 1.33 to the complete verifier, execute foreign
memory, complete Slice 8, or claim paired-host qualification.

## Context

[Decision 0899: lower canonical unsafe scratch construction to WVIR](0899-Lower-Canonical-Unsafe-Scratch-Construction-To-Wvir.md)
stopped exact operation `186` at the source-WVB boundary. That kept the ABI,
memory authority, affine result, and containment questions visible while the
typed source representation settled.

The next smallest boundary is deterministic serialization. Reusing an ordinary
allocation opcode would erase the foreign ABI and let a verifier infer
authority from a result shape. Encoding only the result type would also force a
runtime to recover the selected ABI indirectly from a materialized generic
name. Both choices weaken an identity that later provider binding must check
directly.

Complete execution is still too large for one checkpoint. It needs canonical
type verification, provider allocation and zeroing, bounded failure behavior,
affine teardown, native lowering, and hostile containment tests. Selecting the
byte representation first allows those consumers to converge on one input
without claiming that the input is executable today.

## Decision

1. Reserve candidate WVB minor `1.33` for the first unsafe-scratch
   serialization boundary. A source module containing WVIR operation `186`
   selects minor `33`, including when it also contains inherited task evidence.
2. Assign opcode byte `DC` (`220`) to `unsafe.scratch.construct`. Do not reuse
   the managed-memory or Vector allocation instructions.
3. Consume exactly two ordered stack operands, `Length: u64` followed by
   `Alignment: u64`.
4. Encode exactly three little-endian `u32` immediates: the consumed
   `Memoryˉbudget` local index, the canonical construction-Result type index,
   and the explicit ABI-enum type index.
5. Require the result to remain the exact affine
   `Result<Foreignˉscratch<Abi>, Foreignˉmemoryˉfailure>` selected by the source
   call, and require the ABI immediate to be a declared enum materialized in
   the same compilation.
6. Require every candidate WVB 1.33 module to contain at least one `DC`, and
   reject `DC` under every earlier minor.
7. Preserve the seven-section WVB envelope and every inherited 1.32 encoding.
   An inherited extended callable descriptor keeps its existing trailer.
8. Admit opaque shape `25` as an exact by-value parameter or affine
   non-parameter local in any candidate-1.33 function. It remains invalid as a
   result, borrowed parameter, nominal payload, collection element, or Types
   entry. `DC` consumes one available shape-`25` owner in its own function.
9. Keep current complete verifiers, launchers, interpreters, native lowerers,
   and OS consumers closed to minor 33 until their own exact checkpoint lands.
10. Test the writer with the already bounded source/WVIR oracle and an
   independent byte reader that validates the header, sections, result and ABI
   type categories, budget index, and sole opcode occurrence.
11. Treat runtime length, alignment, capacity, provider allocation, zeroing,
    addressability, teardown, and containment as pending execution semantics,
    not compile-time literal restrictions or implicit traps.

## Implementation standing

The source writer and focused bounded source/WVIR/WVB oracle implement this
candidate serialization locally on Windows. The durable run record is added
only after the implementation commit supplies its immutable source identity.
Linux reconstruction and the complete verifier/runtime/native chain remain
pending.

## Consequences

- The compiler no longer loses an otherwise valid unsafe-scratch operation at
  source-WVB publication.
- The ABI is a first-class bytecode operand rather than a convention recovered
  from source spelling or a generic type name.
- Existing WVB 1.11 through 1.32 artifacts and consumers remain unchanged.
- A well-formed WVB 1.33 candidate is still non-executable and must fail closed
  at every current execution front door.
- The next Slice 8 checkpoint is complete compiler-aligned WVB 1.33
  verification, followed by bounded runtime/provider execution and native
  lowering.

## Reconsideration triggers

Reconsider the three-immediate encoding only before a promoted verifier or
runtime accepts WVB 1.33, and only if the replacement preserves explicit ABI
identity, memory authority, affine ownership, deterministic bytes, and bounded
malformed-input validation. Do not renumber an accepted opcode after external
WVB 1.33 artifacts are published.
