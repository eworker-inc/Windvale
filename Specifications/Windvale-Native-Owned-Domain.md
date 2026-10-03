# Windvale native owned-domain initialization

## Status

Candidate runtime-private x64 constructor for the existing ABI 24/context 10
[owned collection path](Windvale-Native-Owned-Collections.md). It constructs a
fresh physical and accounting domain using the existing runtime leaves. It
does not establish normal launcher integration, installed qualification,
general memory management or a new source allocation rule. See
[Decision 0974](../Documents/Decisions/0974-Construct-Fresh-Native-Owned-Storage-Domains.md).

## Caller and request

`Windvale_owned_domain_initialize`, implemented in
[`X64-Owned-Domain.wva`](../Runtime/Native/X64-Owned-Domain.wva), takes R8 as a
pointer to a complete readable 112-byte request. The request remains unchanged.
The result is an unsigned status in EAX. All nonvolatile registers and the
instruction/depth registers R10/R11 are preserved. Other volatile registers
are unspecified. Windows and Linux use the same private x64 convention.

This is a native caller boundary, not a reader of arbitrary serialized
pointers. The caller maps every complete extent, supplies exclusive write
access to the four metadata regions and arena, keeps them mapped for the
whole invocation, and prevents concurrent access. Validation checks address
arithmetic and relationships; it cannot prove host mappings or authority.
The request is aligned to eight bytes. Context, physical state, adapter and
arena are aligned to sixteen bytes; accounting state is aligned to eight.
All six regions are pairwise disjoint and disjoint from live stack frames.

All fields use little-endian representation:

| Offset | Width | Meaning |
| --- | --- | --- |
| 0 | 4 | Private request version 1 |
| 4 | 4 | Exact request size 112 |
| 8 | 8 | Reserved zero |
| 16 | 8 | Writable 136-byte context address |
| 24 | 8 | Writable 2,112-byte physical state address |
| 32 | 8 | Writable 2,616-byte canonical accounting state address |
| 40 | 8 | Writable 1,088-byte budgeted adapter address |
| 48 | 8 | Writable physical arena address |
| 56 | 8 | Arena capacity, 16 through 16,777,216, a multiple of sixteen |
| 64 | 8 | Nonzero, never-reused domain epoch |
| 72 | 8 | Root maximum charge, zero through arena capacity |
| 80 | 4 | Root maximum child count, zero through 64 |
| 84 | 4 | Reserved zero |
| 88 | 8 | Nonzero WVB instruction budget |
| 96 | 8 | Nonzero call-depth budget |
| 104 | 8 | Reserved zero |

Every pointer is nonzero and every extent end must fit in u64 without wrapping.
The four metadata regions must initially contain only zero bytes, including
the entire context. This requirement rejects an initialized or retired domain;
the constructor never resets its generations. A new epoch remains a caller
requirement even when a previously used address has been zeroed externally.
Arena contents need not be zero. The physical provider zeroes committed backing
before exposing it to an owner.

## Construction and publication

Before saved-register writes, the constructor rejects any input extent that
overlaps its prospective stack window, from entry RSP minus 8,192 through entry
RSP plus eight. The request end is checked before reading its fields; each
region end is checked before comparing its extent. The window bounds this
constructor and its selected callees;
disjointness from the caller's other live frames remains a caller precondition.
The constructor then checks exactly fifteen pairs among its six input extents,
and scans exactly 5,952 metadata bytes (744 eight-byte cells). Arena capacity
does not increase the initialization scan or scratch requirement.

The physical state selects the existing 64-slot owned-storage profile and the
supplied arena/epoch. Canonical accounting has exactly 65 entries. Entry one
is the root budget, identity one and generation one, with no parent, the
requested maximum and child count, and no reserved charge. Other entries are
inactive and zero. The adapter refers to those same physical/accounting states
and epoch. No payload allocation or lease is created during initialization.

The constructor invokes physical initialize and budgeted-adapter initialize
through the existing leaves. Only after both succeed does it publish context
10: size 136, the supplied instruction/depth budgets, adapter pointer at 112,
the linked `Windvale_budgeted_storage` entry at 120, and zero elsewhere. The
version word is stored last. No caller-supplied provider pointer is accepted.

## Failure and lifetime

| EAX | Meaning |
| --- | --- |
| 0 | Complete initialized domain and published context |
| 1 | Invalid request address/alignment, version, size, reserved field or limit |
| 2 | Null/misaligned region, wrapping extent or overlapping regions/stack |
| 3 | At least one metadata byte was nonzero |
| 4 | A selected provider unexpectedly refused initialization |

Every refusal preserves all supplied region bytes and the request. For status
four, the constructor restores the initially zero metadata and the arena's
original sixteen-byte header. The selected initializers do not write other
arena bytes or allocate payload; that boundary makes rollback finite and exact.
This is a contract over the linked runtime leaves, not protection against
arbitrary native code violating their write boundaries.

On success, ownership of the domain belongs to its caller. Generated Main's
existing entry wrapper tears down that domain after normal return or a packed
trap. A caller that never enters Main must invoke adapter teardown itself.
Teardown preserves generations, closes both storage domains and invalidates
the context's use for another Main invocation. Storage mapping destruction is
separate from logical teardown.

## Verification boundary

The existing native storage owner includes seven generated fixtures for this
constructor: initialization with allocation/release and teardown; zero root
authority; minimum arena; invalid request fields; every extent pair plus null,
alignment, wrap and stack conflicts; nonzero metadata; and forced provider
failure with exact rollback. Refusals compare the full 6,144-byte caller layout,
including request, metadata, arena and padding. Calls also check preserved
registers. Both initialized live-owner and retired-domain reuse must refuse.

The existing owner remains the entry point. Its `--owned-domain` selection
runs this boundary without rebuilding the source compiler or replaying unrelated
lowering suites. Paired execution of these native fixtures does not qualify a
normal packaged Language 1.0 application; that integration is still required.
