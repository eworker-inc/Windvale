# Decision 0904: execute WVB 1.33 unsafe scratch in a bounded scalar provider

## Status

Accepted and implemented locally on Windows on 2026-09-01.

This decision opens WVB 1.33 only in the source-built scalar runner's first
bounded scratch-memory provider. It does not add native lowering, expose a
native address, admit a host capability, implement write-region borrowing or
Foreign calls, update a published runner artifact, or claim paired-host
qualification or completion of Slice 8.

## Context

[Decision 0903](0903-Verify-Candidate-Wvb-1.33-Without-Opening-Execution.md)
completed the structural, semantic, typed-stack, canonical-layout, and affine
ownership verification contract for opcode `DC`
(`unsafe.scratch.construct`) while every execution consumer remained closed.
The next useful oracle must execute the verified value without confusing an
opaque source value with a native pointer or granting System code ambient host
authority.

Scratch construction consumes a move-owned `Memoryˉbudget`, so rejection,
allocation, zeroing, and teardown must agree with the existing budget and heap
oracles. A scalar interpreter also receives untrusted WVB directly; it cannot
rely on a version byte alone to prove that a WVB 1.33 module actually contains
the extension that selected that version.

## Decision

1. Extend the source-built scalar interpreter through WVB minor `33` while
   preserving every inherited 1.11-through-1.32 encoding and execution rule.
2. Admit ordinary execution of a WVB 1.33 module only when it selects System
   profile `3`, declares no host capabilities, and contains 1 through 4,096
   exact 13-byte `DC` instructions. Profile selection remains metadata, not an
   authority grant. Earlier minors retain their existing profile boundaries.
3. Require `DC` to consume two ordered `u64` values and one available
   shape-`25` budget parameter or local. Require bounded result and ABI type
   indexes, a kind-`2` or kind-`7` ABI enum, a one-field scratch record, and the
   structural Result and failure shapes before execution. The complete
   compiler-aligned verifier remains the canonical exact-name and
   canonical-layout gate.
4. Define the first provider profile as a positive length no greater than 64
   bytes and a positive power-of-two alignment no greater than 8. Return exact
   `Invalidˉlength` and `Invalidˉalignment` failures, including the observed
   value and the 64-byte maximum where required.
5. Construct an allocation lease through the existing `Memoryˉbudget` oracle.
   A budget refusal becomes the canonical nested `Allocationˉfailure`; a
   bounded heap refusal uses `Fragmented`. A refused lease or provider
   allocation releases all state consumed by that attempt.
6. Create and verify an exact-length zero-filled backing allocation before
   publishing success. Keep the backing allocation and its lease in private
   interpreter state.
7. Publish the source-visible `Foreignˉscratch<Abi>` with a non-address-like
   opaque carrier value of `1u64`. The carrier is not a heap descriptor, host
   address, offset, or handle. Even structurally similar unverified WVB cannot
   turn it into memory authority.
8. Retain a successful backing lease until invocation teardown. The existing
   root budget teardown removes active ownership and finalizes the complete
   bounded child chain before a successful response is returned. Invalid
   length and alignment paths release the consumed budget immediately.
9. Keep zeroing and validation loops bounded by 64 bytes, heap allocation by
   the existing 65,536-byte scalar heap, scratch instructions by 4,096, type
   and local lookups by their existing directories, and diagnostics by the
   runner's existing limits.
10. Extend the focused unsafe-scratch oracle with three executable programs:
    successful zeroed construction, exact invalid-length failure, and exact
    invalid-alignment failure. Also replace the sole `DC` with a same-width
    opcode and require the runtime to reject the forged WVB 1.33 module before
    bytecode execution.

## Implementation standing

The scalar runner builds as a deterministic 498,459-byte WVB at SHA-256
`4488721450f10b7d13c0120961787201e11f5232f9d9e376b6673def26ce061a`.
The focused local Windows oracle completes in about five seconds after the
tools and runner are available. It passes nine source cases, nine malformed
WVIR cases, seven malformed WVB cases, three executable cases, one additional
runtime malformed case, and eleven compiler-verifier cases.

The successful executable returns `42` only after creating and checking the
zeroed backing allocation and completing invocation teardown. The two failure
executables destructure the canonical failure variants and check their exact
values before returning `42`.

Source currently reaches this provider directly from a budget-bearing entry.
Moving a budget through an ordinary source call before scratch construction
remains a separate compiler-emission gap and is not claimed by this checkpoint.

## Consequences

- WVB 1.33 now has one executable reference oracle for its first unsafe-memory
  producer; it is no longer a serialization-only candidate.
- The scalar oracle models allocation and lifetime behavior without exposing a
  native pointer or pretending to implement a foreign ABI.
- A capability-free System module can use this explicit provider. Declaring a
  host capability still closes the ordinary runner boundary.
- Native x64 lowering and containment can compare against this simple bounded
  oracle rather than inventing separate semantics.
- Published front-door artifacts remain at their recorded identities until a
  separate promotion or reconstruction checkpoint replaces them.
- Linux reproduction, paired-host conformance, and broad Qualification remain
  pending; this is local Windows development evidence.

## Reconsideration triggers

Reconsider the 64-byte or 8-byte bounds when a real ABI workload supplies a
measured need and a still-bounded provider contract. Reconsider the opaque
carrier only when pointer or write-region operations have an authenticated,
generation-safe private lookup that cannot expose host addresses. Do not widen
System-profile admission to ambient capabilities, and do not preserve the
scalar provider as a second memory model once native containment replaces it.
