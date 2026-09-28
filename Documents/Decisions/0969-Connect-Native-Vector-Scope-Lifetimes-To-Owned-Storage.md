# Decision 0969: connect native Vector scope lifetimes to owned storage

## Status

Candidate implementation of the maintainer's accepted Language 1.0 memory
direction. The source lowerer and runtime connect a bounded reserved-Vector
subset to physical storage and canonical budget accounting. The
[native owned-collections contract](../../Specifications/Windvale-Native-Owned-Collections.md)
owns exact admission, limits and failure behavior. Installed promotion and
independent whole-product qualification remain separate gates.

## Context

The runtime-private adapter could reserve and release physical storage with
correct accounting, but generated collection code could not call it. The old
native budget representation was a pair of packed counters, which could not
identify a canonical budget generation or return released child credit safely.
Source collections therefore still ran through the interpreter's byte-backed
working state. A passing allocator leaf was not an end-to-end memory result.

## Decision

Extend the existing source lowerer with a deliberately bounded WVB 1.24 target:
one capability-free Main, scalar Vectors, budget Split, reserved construction,
explicit owner release and complete execution-domain teardown. Reuse the
canonical budget tokens and physical handle-to-lease bindings. Do not add a
second accounting tree, compiler, runtime or source format.

Use ABI 24 and context 10 so these owners cannot be confused with ABI 22's
packed budget counters. Old contexts fail before their missing extended fields
are read. The new entry wrapper binds one domain for Main and cleans it up on
normal return and every returned terminal trap. A source owner never exposes
the physical pointer or hidden lease metadata.

Keep the first frame and code bounds small. Reject unbalanced ownership at a
control-flow join instead of treating an intersection of live-owner sets as a
release. General aggregate cleanup and helper transfers require their own
lowering before admission can widen. The frozen source API is unchanged.

## Evidence and next boundary

The existing native development owner covers generated scope reuse, refusal,
trap cleanup, incompatible contexts and malformed ownership. Its repeat workload
requires 1,000 Vector lifetimes in a 64-byte arena with a 32-byte peak allocation
charge. The runtime adapter's separate 32,768-cycle workload now uses real
native Split operations instead of seeding the accounting transition in tests.
Routine run results belong in the commit summary.

The next boundary is useful Vector access/mutation and owned helper/aggregate
transfer, then migration of interpreter working state. Shared immutable backing,
general capacity replacement and installed launcher integration are still open.
This checkpoint does not claim that ordinary interpreted programs now have a
smaller process working set or that Windvale 1.0 memory management is complete.
