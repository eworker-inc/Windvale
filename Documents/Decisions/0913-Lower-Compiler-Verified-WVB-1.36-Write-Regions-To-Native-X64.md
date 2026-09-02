# Decision 0913: lower compiler-verified WVB 1.36 write regions to native x64

## Status

Accepted on 2026-09-01 and implemented locally on Windows by 2026-09-02.

This decision admits the exact compiler-verified WVB 1.36 write-region
operation to the Windvale-native x86-64 backend and promotes the corresponding
verifier-gated WVB runner candidate. It does not expose a native address or
pointer, lower a Foreign call, migrate a runtime or operating-system boundary,
open browser, WebAssembly, or Windvale OS consumers, or claim Linux execution
or paired-host qualification.

## Context

[Decision 0912](0912-Execute-WVB-1.36-Write-Regions-In-A-Bounded-Scalar-Provider.md)
made write-region validation executable in the bounded scalar provider while
keeping native lowering and the promoted package launcher closed. The scalar
provider established the outcome order and exact Foundation Result layouts,
but it intentionally stored only a scalar-heap subrange descriptor.

The native backend already constructs bounded scratch owners and observes
their lengths without publishing host addresses. The smallest coherent native
checkpoint is therefore the same compiler-verified validation semantics over
a private native descriptor. Pointer derivation remains a later, separately
authenticated authority boundary.

## Decision

1. Admit WVB minor `36` and opcode `DE` (`222`) to the Windvale-native x86-64
   lowerer only under the exact capability-free System profile and inherited
   module, function, instruction, frame, record, affine, and output bounds.
2. Independently validate the canonical scratch, region Result,
   `Foreignˉpointerˉfailure`, and ABI identities. Require an available exact
   scratch owner or authenticated mutable borrowed scratch parameter, then
   make it unavailable through every successor and function exit.
3. Under minor 36, retain the bounded scratch length in the low 32 bits of its
   private field-zero word and its construction alignment in the high 32 bits.
   `Scratchˉlength` reads only the low word. This word is not an address,
   pointer, host handle, or public representation.
4. Emit one fixed 340-byte metered instruction sequence for `DE`: the existing
   ten-byte instruction-budget charge followed by 330 operation-specific
   bytes. Preserve `R11` as the shared instruction budget while using ordinary
   temporary registers for validation and Result construction.
5. Check zero length first and construct exact `Outˉofˉrange`. Next check
   unsigned `start + length` carry and construct exact `Addressˉoverflow` with
   width 64. Then reject an exclusive end beyond the bounded owner length as
   `Outˉofˉrange`.
6. Require a nonzero power-of-two alignment no greater than the retained
   construction alignment and require the logical start to satisfy it.
   Construct exact `Misaligned` on any disagreement.
7. On success, store only a private packed logical descriptor whose low word
   is the checked start and whose high word is the exact length. Publish the
   canonical Valid Result around that opaque region record. Do not allocate,
   copy backing bytes, form a machine address, or grant dereference authority.
8. Preserve exact Failure payload observation while keeping the Valid region
   payload unavailable to accepted ordinary operations. Preserve all WVB
   1.33-through-1.35 construction, transfer, and observation behavior.
9. Require the focused native oracle to pass aligned success, zero length,
   address overflow, owner-range rejection, and alignment rejection after the
   compiler-aligned verifier. Reject a same-geometry missing-operation
   mutation before WVO publication and execute the inherited native scratch
   matrix unchanged.
10. Promote the source-built verifier-gated WVB runner candidate and its
    deterministic Windows and Linux packages. Current-host runner execution
    must retain ordinary reporting and malformed-input rejection. A produced
    Linux artifact is reconstruction evidence, not Linux execution evidence.
11. Preserve segmented native-object publication when the relocation region is
    empty. After the final nonempty chunk, the staging producer may consume
    only canonical zero-length publication steps to reach the exact Complete
    cursor and object extent. It must not publish an empty chunk or accept any
    other zero-length terminal state.

## Implementation standing

The focused native oracle passes all five exact write-region outcomes with
result `42`, rejects the missing-operation mutation, and preserves all nine
inherited WVB 1.33-through-1.35 native scratch executions. The segmented
compiler toolset, WVB-to-WVO lowerer, and WVB runner independently reconstruct
their promoted paired-host artifacts byte for byte in focused owners.

Downstream database verification exposed that the staging producer previously
reported `Outputˉlimit` after writing a complete object whose relocation region
was empty. The corrected producer publishes the 479-byte, relocation-free
Return-42 object as exactly three nonempty chunks and a 60-byte manifest. The
same reconstruction owner retains exact SHA-helper staging and the 50,761,605-
byte compiler-scale object, so the terminal correction does not weaken chunk,
manifest, or large-object bounds.

The native implementation uses only relative logical geometry. Unlike a later
pointer-producing operation, it has no base address whose addition could
overflow. The compiler-aligned verifier remains the public semantic gate
before the focused native execution path.

## Consequences

- Compiler-verified WVB 1.36 write-region validation now executes in both the
  bounded scalar oracle and generated native x86-64 code.
- The promoted source-built runner candidate executes the verified scalar
  semantics, while the promoted lowerer candidate can generate the native
  validation sequence.
- Success still creates no usable pointer. Pointer derivation is the next
  unsafe operation and must preserve region lifetime and non-retention proof.
- Authenticated no-retain Foreign calls, one migrated real boundary, Linux
  execution, and paired-host qualification remain required for Slice 8.

## Reconsideration triggers

Replace the packed descriptor only if a later accepted native provider needs a
larger logical owner extent. Do not reinterpret either word as an ambient
address. Shorten scratch unavailability only after an explicit region-release
or lexical-consumption proof exists.
