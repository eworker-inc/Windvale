# Windvale native shared storage

## Status

Candidate runtime-private x64 profile, version 1, selected by
[Decision 0977](../Documents/Decisions/0977-Add-Native-Reserved-Byte-Builders-And-Shared-Storage.md).
[The shared-storage leaf](../Runtime/Native/X64-Shared-Storage.wva) supplies
reserved byte builders and immutable backing over the current
[budgeted storage](Windvale-Native-Budgeted-Storage.md) adapter.
The existing native owned-storage verifier owns its behavior checks.

This contract controls private runtime metadata and calls. It does not expose
source representations or change WVB, native ABI 24, context 10, pinned
bootstrap artifacts or the frozen Language 1.0 source contract. Source byte
owners, automatic shared lifetime cleanup, text, mutable buffers, interpreter
working storage and installed delivery remain unfinished.

## Ownership and limits

Calls are serialized. All supplied control and backing extents are complete,
mapped, writable and pairwise disjoint; borrowed input spans must be mapped and
readable for their complete length. These are trusted runtime preconditions,
not authority to dereference arbitrary source-provided host pointers. The leaf
has no host calls or allocation outside the supplied domain.

The caller prepays 2,112 bytes of shared metadata in addition to the lower
domain's 5,816 metadata bytes, for 7,928 bytes before payload. Requests and
bounded call scratch are separate. There are 64 shared slots, scoped to one
nonzero, never-reused lower-domain epoch. Physical handle generations retain
the lower allocator's stale-handle checks. Reference counts are unsigned 32-bit
and may not wrap. There is no concurrency or atomic-reference-count guarantee.

Every shared entry must match one current lower binding. Lower bindings may
also belong to other owners, including scalar Vectors, without a shared entry.
While a shared entry exists, its lower handle and lease may only be released
through this adapter. Whole-domain teardown releases both kinds of owner.

A builder's admitted maximum is 0 through 4,194,304 bytes. Reserve commits
`max(1, Maximum)` physical bytes, with exact retained charge
`align_up(max(1, Maximum) + 16, 16)`. The hidden one-byte allocation for a zero
maximum does not permit appending a byte. Its charge is 32 bytes. Logical length
starts at zero and never exceeds the admitted maximum. Appends within the
maximum acquire no additional backing or lease. Bytes outside logical length
are not exposed by immutable views.

Freeze keeps the same physical allocation and full committed charge, including
unused capacity. Retain changes only its reference count; it creates no lease
or new physical allocation. Release makes storage reusable when the final
reference disappears. An ancestor budget may remain reserved while surviving
children exist, following the lower canonical accounting contract.

## Layouts and calling convention

All integer fields are unsigned and little-endian. The exported symbol is
`Windvale_shared_storage`. R8 points to 16-byte-aligned shared state; R9 points
to an 8-byte-aligned request. EAX returns status. Nonvolatile registers and
R10/R11 survive. The ordinary native x64 stack alignment and shadow-space
requirements apply. The leaf uses 456 bytes below its entry stack pointer;
the current deepest nested path uses 1,024 bytes. Pointer preflight excludes
the conservative interval from entry stack pointer minus 8,192 through plus 8.

| Shared state offset | Width | Meaning |
| --- | --- | --- |
| 0 | 4 | Magic 1397970519 after initialization; initially zero |
| 4 | 4 | Version 1 |
| 8 | 4 | State size 2112 |
| 12 | 4 | Slot count 64 |
| 16 | 8 | Initialized 1088-byte budgeted adapter address |
| 24 | 8 | Nonzero epoch, equal to the lower adapter's epoch |
| 32 | 4 | Closed flag, initially zero |
| 36 | 28 | Reserved zero |
| 64 | 2048 | 64 entries of 32 bytes |

Each live entry contains its physical handle at +0, reference count at +8,
kind at +12 (1 mutable builder, 2 immutable bytes), logical length at +16,
admitted maximum at +20, and reserved zero at +24. A builder has exactly one
reference. An immutable entry has at least one. Inactive entries are all zero.
Metadata contains no duplicate payload header or separate budget-token format.

| Request offset | Width | Meaning |
| --- | --- | --- |
| 0 | 4 | Version 1 |
| 4 | 4 | Request size 128 |
| 8 | 4 | Operation, 0 through 13 |
| 12 | 4 | Reserved zero |
| 16 | 8 | Domain epoch |
| 24 | 8 | Input owner handle; zero for initialize/reserve/teardown/validate |
| 32 | 8 | Consumed budget token for reserve; otherwise zero |
| 40 | 8 | Argument 0 |
| 48 | 8 | Argument 1 |
| 56 | 8 | Input span address, or operation-specific argument 2 |
| 64 | 4 | Status output |
| 68 | 4 | Exact charge output |
| 72 | 8 | Borrowed pointer output |
| 80 | 8 | Logical/range length output |
| 88 | 8 | Admitted maximum output |
| 96 | 4 | Reference count output |
| 100 | 4 | Reserved zero |
| 104 | 8 | Requested charge output |
| 112 | 8 | Pre-call available authority output |
| 120 | 8 | Reserved zero |

Output and reserved fields are zero on entry. Unused arguments are zero.
Reserve publishes handle +24 and all observation/charge fields. Append, freeze
and retain publish charge, full-backing pointer, logical length, maximum and
count. View publishes those observations with the exact range pointer/length.
Release, initialize, teardown and validate publish only status. Published
pointers grant a borrow, never additional ownership or mutable authority.

## Operations

0. **Initialize:** require zero magic, open state, empty shared entries and a
   valid supplied lower domain. Publish magic only after complete validation.
   Do not reset physical or accounting generations.
1. **Reserve builder:** argument 0 is the admitted maximum, and +32 is a live
   canonical budget owner. Consume the budget into one committed lease and
   publish a builder handle. Complete construction refusal releases the valid
   input budget as required by the source constructor contract.
2. **Append u8:** argument 0 must fit u8. Append one byte.
3. **Append u32 little-endian:** argument 0 must fit u32. Append four bytes.
4. **Append u64 little-endian:** append argument 0 as eight bytes.
5. **Append u64 decimal:** append the shortest ASCII decimal representation,
   including one zero digit for zero. Conversion uses at most 20 digits and
   bounded 64-bit software-division work per digit.
6. **Append mapped immutable span:** +56 addresses argument-1 bytes, at most
   4,194,304; argument 0 is zero. The span must not overlap control extents, the lower arena or call
   scratch. An empty span requires a zero pointer. Check the whole append before
   copying any byte. Use operation 13 for immutable backing in this domain.
7. **Freeze:** consume the unique builder into one immutable reference without
   copying, compaction, allocation or charge change. Subsequent appends refuse.
8. **Retain immutable backing:** increment a live immutable entry's count after
   checking overflow. Copies and owning slices require one balanced retain.
9. **Release:** release a unique builder or one immutable share. At the final
   reference, release the bound lower handle and lease and clear the entry.
10. **Borrow immutable range:** argument 0 is byte start and argument 1 is byte
    length. Check addition and the complete range against logical length. Return
    the range pointer and length without changing references or accounting.
11. **Teardown:** validate the whole domain, release the lower resource domain,
    clear shared entries and close shared state. Outstanding counts do not
    prevent terminal domain reclamation. All later operations refuse.
12. **Validate:** read-only complete preflight of shared state, lower state and
    their binding relationship. It creates no owner and acquires no storage.
13. **Append same-domain immutable range:** +24 is the destination builder,
    argument 0 is the source immutable handle, argument 1 is its byte start,
    and +56 is its byte length. Validate both owners, checked source range and
    complete destination capacity before copying. The borrow changes no source
    reference count or charge.

Borrowed ranges are valid only while a semantic owner keeps their backing live
and before terminal teardown. Source compilation must enforce this lifetime.
The trusted caller must balance shares exactly; generation checks do not
detect two releases of the same semantic share while another alias survives.

## Validation and refusal

Preflight validates control alignment, checked extents, mutual and stack
disjointness, versions, exact sizes, epoch, open state, reserved fields, all
shared entries and canonical lower bindings before mutation. Lower operation
12 validates the complete accounting/physical relationship once per preflight.
The shared pass then visits at most 64 entries. It rejects duplicate, stale or
mismatched bindings, invalid kinds/counts, and lengths or maxima inconsistent
with committed backing.

| Status | Meaning |
| --- | --- |
| 0 | Success |
| 1 | Malformed request, extent or overlap |
| 2 | Insufficient budget authority |
| 3 | Physical exhaustion or fragmentation |
| 4 | Stale owner, epoch or incompatible owner kind |
| 5 | Corrupt state or binding relationship |
| 6 | Exhausted slots, generations or reference-count range |
| 7 | Append or borrowed range exceeds its admitted bound |
| 8 | Unsupported target capacity |
| 9 | Closed domain |

An append, retain, range or release refusal changes neither payload, logical
length, shared metadata nor lower accounting/storage. Only request status may
change. Complete constructor refusals 2, 3, 6 and 8 additionally consume and
release the previously validated input budget and publish requested/available
charge. If the private requested-charge calculation exceeds u64, its refusal
observation saturates at the u64 maximum; no allocation occurs. Malformed or
corrupt requests do not consume it. Invalid control or raw-span extents,
including oversized spans, refuse without writing request status. Error priority is unspecified
when several inputs are invalid.

## Verification and remaining integration

The shared cases extend the existing
[native owned-storage cases](../Tools/Native/Native-Owned-Storage-Cases.mjs)
and its focused native lowerer owner. They cover exact charges, freeze slack,
aliases, final release, parent credit, non-tail reuse, decimal boundaries,
same-domain append, malformed and corrupt state, unchanged-on-refusal snapshots,
and terminal teardown. Fixed-live-state workloads run 1, 1,000 and 32,768
iterations with an explicit arena and retained-charge ceiling.

Leaf checks establish only this private storage profile. Closing the 1.0 gate
still requires canonical source byte owners and operations, verifier lifetime
proofs, versioned native descriptor/context integration, cleanup through all
exits and aggregates, a maintained compiler consumer, and exact Windows/Debian
qualification. No interpreter working-set or installed-product claim follows
from a native assembly fixture.
