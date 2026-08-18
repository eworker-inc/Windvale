# Decision 0764: Resolve the Language 1.0 System/FFI findings

## Status

Accepted by the project owner on 2026-08-17 under the instruction to integrate
all recommended correctness/completeness findings needed for a correct Language
1.0. This decision refines
[Decision 0751](0751-Accept-Windvale-Language-1.0-Direction.md) and the
normative-candidate language, grammar, and Foundation companions.

It accepts all seven findings from workload 10. It does not freeze edition 1,
implement System source or an ABI/backend, make the paper ABI a product promise,
or authorize arbitrary foreign libraries.

## Context

The tenth mandatory workload calls one exact Linux x86-64 SysV AMD64 C symbol
through a small System adapter, lends one bounded aligned caller-owned region,
translates an i64 outcome, validates returned bytes in Core, copies the payload,
and publishes no unsafe value. Its rejected cases distinguish invalid target,
ABI, pointer, range, alignment, lifetime, alias, generation, status, length,
enum/Boolean, unwind, and containment behavior.

The candidate already selected System-only unsafe blocks, opaque foreign
pointers, exact foreign declarations, checked address arithmetic, no general
exceptions, and no implicit null/pointer-sized integer. Complete source exposed
that concrete ABI target predicates, ABI-registry meaning, nullable pointers,
caller-owned scratch/regions, and the recoverable-data versus terminal-memory-
contract boundary still needed exact closure.

## Decision

### Add the first concrete System ABI target scope

Add `linux.x86_64.sysv_amd64_c_v1` to the target-scope registry. It matches only
Linux, x86-64, ABI identity `sysv_amd64_c_v1`, and no-unwind C scalar/pointer
interface major 1. A target-specific System adapter is a separate module over
portable Core logic. Do not add conditional compilation or infer the current
host.

### Make the foreign ABI literal a complete registered contract

The first foreign-declaration text literal resolves to one immutable registered
ABI contract fixing architecture, address width, calling convention,
scalar/pointer representation, byte order, alignment, ownership/retention,
symbol scope, error interpretation boundary, unwind policy, and required target
predicate. Unknown, unsupported, mismatched, or duplicate binding rejects before
artifact publication.

The existing foreign-declaration grammar remains sufficient; no attribute bag,
header import, or host-default shortcut is added.

### Distinguish nullable and non-null foreign pointers

`Foreignˉpointer<T,Abi>` is non-null but not safe to dereference.
`Nullableˉforeignˉpointer<T,Abi>` is the distinct ABI form when null is admitted.
`Requireˉnonˉnull` is named and unsafe. Neither pointer kind is an integer,
serializable value, safe reference, implicit Option, or authority token.

Non-null validation alone does not prove range, alignment, initialization,
lifetime, aliasing, ownership, or access.

### Fix caller-owned aligned scratch and write-region calls

Accept `Foreignˉscratch<Abi>`, `Foreignˉwriteˉregion<Abi>`, memory/pointer failure
families, scratch construction/length, checked exclusive region borrowing,
borrow-tied pointer extraction, region length, and post-region ordinary slice
borrowing.

Scratch is positive-length, power-of-two aligned, address-witness checked,
zero-initialized, budget-owned, and lexically released. Region creation validates
relative and native address arithmetic, owner bounds, alignment, live lifetime,
and exclusivity before pointer publication. Pointer/region lifetime ends before
ordinary byte observation.

### Separate recoverable untrusted data from terminal ABI violations

Returned status, lengths, bytes, enums, Booleans, generations, and format ranges
are untrusted data. Adapters validate them completely and may return typed safe
failures.

A write outside the supplied range, forbidden retention/use-after-return,
calling-convention or callee-state corruption, or forbidden unwind may destroy
process integrity before control returns. It follows the exact ABI's terminal
containment policy and is tested in isolation; it is not converted into a safe
`Result`.

### Keep status/error/unwind translation explicit

Accept the workload's one-call, no-retry i64 contract. Named negative outcomes
translate explicitly, nonnegative lengths convert exactly and are range-checked,
and stale generation retains expected/observed values. Recoverable foreign
failure is a returned value. Foreign unwinding never crosses a safe Windvale
frame.

### Publish only independently safe values and no new authority

The System adapter returns a Core record only after complete portable decoding
and copies payload bytes out of scratch. No pointer, region, scratch, raw address,
layout witness, unsafe handle, foreign status, or ABI-specific type escapes.

System profile and unsafe context enable the audited operation but grant no
filesystem, network, device, process, clock, entropy, allocator, or other
authority. A real foreign adapter declares and receives every such grant
separately.

## Consequences

The System/FFI bundle becomes draft reviewed. All eleven mandatory Language 1.0
paper workloads now have project-owner-reviewed findings. The next step is the
complete-suite reconciliation and source-freeze identity proposal; this decision
does not itself freeze source edition 1.

The semantic candidate gains one concrete target key and exact registered-ABI,
pointer-kind, recoverable-data, and terminal-containment rules. Foundation gains
two opaque scratch/region owners, one nullable pointer kind, two typed failure
families, and seven exact pointer/scratch/region operations. Grammar gains no new
production.

The reference uses one call, one 64-byte 8-byte-aligned scratch, one 24-byte
record, one four-byte copied payload, and one 62-byte report with SHA-256
`c0a915258a1d23e50599c51f208465768368683158b8d9a17af2b981999961cd`.

## Reconsideration triggers

Reconsider ABI-declaration metadata only if two implemented incompatible ABIs
cannot be represented unambiguously by registered identity plus exact signature.
Do not replace the registry with host inference.

Reconsider scratch/region operations only with a smaller surface that proves the
same bounds, alignment, address width, ownership, lifetime, aliasing, and safe
publication outcomes.

Reconsider terminal containment only if a named isolation mechanism proves that
the fault cannot corrupt the caller's safety domain. A returned status is not
such proof.
