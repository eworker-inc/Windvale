# Decision 0912: execute WVB 1.36 write regions in a bounded scalar provider

## Status

Accepted and implemented locally on Windows on 2026-09-01.

This decision admits compiler-verified WVB 1.36 write-region borrowing to the
source-built scalar provider. It does not open the published front door, native
lowerer, package launcher, browser or WebAssembly host, or Windvale OS. It does
not expose a host address or pointer, authenticate a Foreign call, complete
Slice 8, or claim paired-host qualification.

## Context

[Decision 0911](0911-Verify-WVB-1.36-Write-Region-Lifetime-Containment.md)
proved the exact WVB shape, canonical Foundation layouts, typed stack, affine
Result, and conservative scratch lifetime while every execution consumer
remained closed. The existing scalar scratch provider already owns a bounded
64-byte private heap, an allocation descriptor, an exact budget lease, and
bounded aggregate storage. That provider can execute region validation without
inventing a second address model or publishing a native address.

The scalar provider is a semantics oracle, not an FFI substitute. Its useful
next boundary is exact subrange validation and failure construction while the
backing descriptor remains interpreter-private.

## Decision

1. Admit WVB minor `36` and opcode `DE` (`222`) to the source-built scalar
   interpreter only under the inherited capability-free System profile and
   existing instruction, aggregate, heap, lease, and diagnostic bounds.
   Every runner entry mode executes the embedded compiler-aligned verifier
   first and rejects an invalid minor-36 module before constructing an
   interpreter request.
2. Under minor 36, store the existing eight-byte heap-allocation descriptor in
   the opaque scratch record. The descriptor contains a bounded scalar-heap
   offset and length; it is not a native address, pointer-sized integer, host
   handle, or serializable public value. WVB 1.33-through-1.35 retain their
   previous private length representation.
3. Preserve `Scratchˉlength` under minor 36 by reading the descriptor length in
   constant time. Do not scan or copy the backing allocation.
4. Before executing `DE`, require the named scratch local to contain the exact
   live record, descriptor, heap allocation, generation-safe lease, length,
   and power-of-two construction alignment. Require three `u64` operands, the
   canonical region Result shape, its one-word region record, and a declared
   kind-`2` or kind-`7` ABI enum.
5. Apply the frozen region checks in order: zero length; relative, base-plus-
   start, and exclusive-end overflow; owner extent; then requested and actual
   alignment. Construct exact `Outˉofˉrange`, `Addressˉoverflow`, or
   `Misaligned` payloads without publishing a region on failure.
6. On success, store only the checked scalar-heap subrange descriptor in the
   opaque region record. The descriptor contains the subrange offset and exact
   length and remains inaccessible through accepted source or WVB operations.
7. Continue to mark the scratch unavailable through function exit. Static
   verifier containment therefore makes dynamic aliasing, stale-owner, and
   concurrent-region states unreachable in this first executable profile.
   Normal function teardown releases the original scratch allocation and
   lease; the region descriptor owns no second allocation lease.
8. Permit extraction and ordinary matching of the Result's Failure payload so
   exact `Foreignˉpointerˉfailure` data can be observed. Continue to reject
   payload and field extraction from the Valid region arm, Result construction,
   call or signature transfer, and return escape.
9. Extend the focused oracle with one success, zero length, address overflow,
   owner-range rejection, and alignment rejection. Require each module to pass
   source analysis, WVB 1.36 emission, compiler-aligned verification, and
   scalar execution with result `42`. Reject a same-geometry missing-`DE`
   mutation and preserve the inherited WVB 1.33/1.35 scratch matrix.

## Implementation standing

The source-built compiler verifier is 473,783 WVB bytes at SHA-256
`76c36d5a341a37e14a162427bef2870ed498cdbf4366abcebb703f8f79f32c7d`.
Its packaged Windows executable is 3,817,984 bytes at SHA-256
`5bad08c1e6c4bfdbd3195a4a0de6460a6de8c4b710f8a699e6fb1d469358c6d1`.

The verifier-gated source-built scalar runner is 993,328 WVB bytes at SHA-256
`2e7f5390c95e74be2abb06c2b2cbb84d789c3d449a7577c40f9de45157a874a6`.
Its three-fragment Windows executable is 10,127,360 bytes at SHA-256
`c7e7a917622698a511ebb8b478c8075d943feaf987d0aae56c9b7c8cab21c5e4`.

The focused write-region oracle passes 13 source cases, seven malformed WVIR
mutations, five malformed WVB mutations, five semantic WVB forgeries, 19
compiler-verifier decisions, five scalar executions, and one direct runtime
mutation. The inherited scratch oracle passes all 12 source cases, 31 malformed
WVIR/WVB cases, 20 compiler-verifier decisions, nine scalar executions, and
bounded teardown.

## Consequences

- WVB 1.36 now has one executable, bounded semantics oracle for region
  construction and exact validation failures.
- No public carrier contains a host address. Native pointer derivation remains
  a separate unsafe operation and cannot be inferred from this descriptor.
- Failure observation is useful source behavior; successful region opacity is
  the authority boundary and remains enforced by the compiler verifier.
- The 64-byte and 8-byte-alignment limits belong to this scalar oracle, not the
  portable language contract or future native provider.
- The next checkpoint is native x86-64 lowering of the same verified semantics.
  Pointer derivation, authenticated no-retain Foreign calls, a migrated real
  boundary, Linux reproduction, and paired-host qualification follow.

## Reconsideration triggers

Replace the scalar descriptor only if a later bounded interpreter memory model
requires more than a 32-bit offset or length. Do not expose it as a language
integer or reuse it as a native pointer. Shorten the conservative function-exit
borrow only after an explicit region-consumption or lexical-release proof exists.
