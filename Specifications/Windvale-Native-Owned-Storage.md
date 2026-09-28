# Windvale native owned storage

## Status

Current runtime-private x64 implementation, version 1. This is physical backing
for the accepted Language 1.0 memory direction, implemented by
[`X64-Owned-Storage.wva`](../Runtime/Native/X64-Owned-Storage.wva) over the existing
descriptor allocator leaf. The [budgeted adapter](Windvale-Native-Budgeted-Storage.md)
now connects it to canonical budget accounting. Compiler-generated collection
operations and interpreter working storage still need integration. It neither
changes native ABI 22 nor replaces its arena/context fields.

The native lowering development owner verifies this leaf on Windows and Linux.
Its focused selection is `--owned-storage`; the complete owner includes the same
cases. This is development evidence, not installed-toolchain qualification.

The leaf provides committed capacity, zeroed mutable storage, exact physical
charges, deterministic reuse and domain teardown. A generation in each handle
prevents a released handle from accessing a later allocation at the same address.
The adapter binds physical charges to canonical budget leases; the compiler
must still carry ownership and cleanup through emitted operations. No process-memory
improvement for existing consumers follows from this isolated implementation.

## Authority and lifetime

`Windvale_owned_storage` is an internal system operation with no host calls,
ambient allocation, filesystem or network authority. Its caller supplies an
exclusive, mapped, readable/writable state, request and arena. The three complete
extents must be disjoint and remain mapped throughout the call. Invalid arbitrary
machine pointers and truncated mapped extents cannot be safely probed by this
leaf; the trusted runtime must validate their provenance before calling it.

Calls are synchronous and serialized per domain. The caller owns the domain and
must ensure that borrowed pointers expire before release or teardown, and that
mutation has exclusive access. A raw pointer is never a lifetime proof. Handles
are runtime-private ownership identities; this leaf does not enforce source-level
move or borrow checking. There is no retain/share operation. Each live allocation
has exactly one physical owner. Shared immutable backing needs a later adapter.

The caller prepays the fixed 2,112-byte metadata state separately from payload
charges and provides a nonzero 64-bit epoch that is never reused for another
domain in the same authority scope, including after teardown. Retained slot
generations prevent stale handles within that epoch. Domain identity reuse is a
caller error, not something an isolated state buffer can detect.

## Calling convention and layouts

All fields are little-endian unsigned integers. Pointers are 64-bit addresses.
R8 points to the 16-byte-aligned state; R9 points to the 8-byte-aligned request.
EAX returns status. All nonvolatile registers and the R10/R11 execution-budget
registers survive. Other volatile registers and flags may change. The stack must
follow the existing x64 native call convention, including 32 bytes of shadow
space. The leaf uses 152 bytes below its entry stack pointer, plus the nested
descriptor allocator's bounded stack.

State version 1 is exactly 2,112 bytes:

| Offset | Width | Meaning |
| --- | --- | --- |
| 0 | 4 | Magic 1397700183 after initialization; initially zero |
| 4 | 4 | Version 1 |
| 8 | 4 | State size 2112 |
| 12 | 4 | Slot count 64 |
| 16 | 8 | Nonzero domain epoch |
| 24 | 4 | Peak simultaneous physical charge; initially zero |
| 28 | 4 | Closed flag, initially zero; teardown sets one |
| 32 | 8 | Arena address, aligned to 16 bytes |
| 40 | 4 | Arena length, multiple of 16, from 16 through 16,777,216 |
| 44 | 4 | Embedded allocator magic 1396790871; initially zero |
| 48 | 4 | Address-ordered free-list head token; initially zero |
| 52 | 4 | Live allocation count; initially zero |
| 56 | 4 | Current total physical charge; initially zero |
| 60 | 4 | Reserved zero |
| 64 | 2048 | 64 slots of 32 bytes, initially zero |

Slot fields are generation at +0, physical owner token at +4, capacity at +8,
logical length at +12, physical charge at +16, alignment at +20, and eight
reserved zero bytes at +24. All except generation are zero when inactive.
The physical token is the allocator header's arena offset plus one. It is never
exposed as the owned handle.

Request version 1 is exactly 64 bytes:

| Offset | Width | Meaning |
| --- | --- | --- |
| 0 | 4 | Version 1 |
| 4 | 4 | Request size 64 |
| 8 | 4 | Operation 0 through 6 |
| 12 | 4 | Requested capacity, reserve only |
| 16 | 8 | Expected domain epoch |
| 24 | 8 | Handle input; zero for reserve, initialization and teardown |
| 32 | 4 | Requested alignment, reserve only |
| 36 | 4 | New logical length, reserve or resize only |
| 40 | 4 | Status output; zero on entry |
| 44 | 4 | Physical charge output; zero on entry |
| 48 | 8 | Borrowed data pointer output; zero on entry |
| 56 | 4 | Maximum authorized physical charge, reserve only |
| 60 | 4 | Reserved zero |

Unused request fields must be zero. A handle consists of the nonzero slot
generation in the high 32 bits and slot index plus one in the low 32 bits.
It is meaningful only with its domain epoch. Release preserves the generation;
the next reserve increments it before publishing. Generation 0xffffffff is the
last usable generation. Releasing it retires that slot permanently until the
entire domain is destroyed; generations never wrap.

## Operations

0. **Initialize:** validate the seeded state and zero metadata, then establish
   one free block spanning the supplied arena. Arena payload need not start zero.
   Initialization of an already initialized state is rejected. A closed domain
   cannot be reopened. The caller must create a new state with a new epoch.
1. **Reserve committed:** capacity is 1 through 4,194,304 bytes. Alignment is a
   power of two, syntactically at most 4096; this target supports at most 16.
   Logical length must not exceed capacity. The entire capacity is physically
   acquired and zeroed before publishing the handle, pointer and charge.
2. **Resize:** change logical length within the existing committed capacity.
   Growth zeroes every newly exposed byte, including bytes previously hidden by
   shrinking. Neither charge nor address changes. Growth beyond capacity fails
   unchanged. Resizing capacity or relocating storage is not supported here.
3. **Release:** validate the epoch and live generation, return the block to the
   free list, coalesce adjacent free blocks, and invalidate the handle. Charge
   and pointer outputs remain zero. Bytes need not be erased on release; a later
   reserve zeroes them before exposure. Double release is a stale-handle error.
4. **Inspect:** validate the handle and return a borrowed pointer and charge.
5. **Teardown:** validate the entire domain before mutation, release all live
   slots in index order, and close the domain. Current charge and live count
   become zero; the peak remains available. Later operations report closed.
6. **Validate:** check the complete initialized, open domain without mutation.
   All operation-specific fields must be zero. The budgeted adapter uses this
   operation to establish physical validity before coordinating accounting.

The exact charge is `align_up(capacity + 16, 16)`, including the allocation
header and alignment padding. It must fit the request's maximum authorized
charge. That input is an already-authorized limit supplied by the caller, not a
public `Memoryˉbudget` object or a deduction from its parent. The sum of live
physical charges cannot exceed the arena. Free space is reusable immediately,
including blocks freed before later live blocks. First-fit allocation can still
fail from fragmentation when total free bytes would otherwise suffice.

## Refusal and validation

| Status | Meaning |
| --- | --- |
| 0 | Success |
| 1 | Invalid request, pointer alignment, or overlapping request extent |
| 2 | Required physical charge exceeds the authorized maximum |
| 3 | Physical arena exhausted or fragmented |
| 4 | Wrong epoch, stale generation, inactive slot or invalid handle |
| 5 | Corrupt state, extent, allocation header, free list or accounting |
| 6 | No reusable slot; live or permanently retired slots exhaust the table |
| 7 | Logical length exceeds committed capacity |
| 8 | Capacity or alignment exceeds this target's supported limit |
| 9 | Domain closed |

Every rejected request leaves state, allocation bytes and input fields unchanged.
Only request status is written. Null/misaligned request or state pointers,
overflowing state/request/arena extents, or a request overlapping state/arena
return status 1 in EAX without publishing any request field. Other validation
requires the mapped-extent precondition above. When several fields are invalid,
validation order is implementation detail; no particular error takes precedence.

Before mutation, validation checks slot bounds and metadata, each live header,
pairwise live overlap, free-list ordering, alignment, headers, bounds, cycles,
overlap with live blocks, complete arena coverage, current accounting and peak.
No operation can follow an unchecked free-list link indefinitely. With 64 live
slots there are at most 65 coalesced free blocks; the validator rejects more.
The historical allocator's lazy-initialization path is not used. Initialization
and teardown therefore do not acquire resources while deciding whether to fail.

## Resource bounds and verification

These are explicit limits of this runtime-private provider, not universal
language collection limits. Validation visits 64 slots, at most 2016 live/live
pairs, and at most 65 free nodes with 64 live-overlap comparisons each. Reserve
zeros at most 4 MiB and resize at most the same committed capacity. Teardown
releases at most 64 blocks. All address and size arithmetic is checked or bounded
before use. No operation grows the metadata or calls an external allocator.

The existing native lowering development owner assembles the runtime leaves
twice and compares object bytes, validates their objects, and executes native
cases for reuse, coalescing in both directions, fragmentation, slot exhaustion,
generation retirement, epoch mismatch, zeroing, request/state corruption,
unchanged refusal, register preservation, and multi-allocation teardown.
The repeated-reuse workload performs 32,768 reserve/release pairs with capacity
17 inside a 64-byte arena and asserts peak physical charge 48 and final charge
zero. Its fixed metadata is 2,112 bytes; process working set is a different
measurement and is not inferred from those bounds.
