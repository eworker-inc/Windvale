# Windvale native owned-domain initialization

## Status

Candidate runtime-private x64 constructor for the existing ABI 24/context 10
[owned collection path](Windvale-Native-Owned-Collections.md). It constructs a
fresh physical and accounting domain using the existing runtime leaves. It
does not establish normal launcher integration, installed qualification,
general memory management or a new source allocation rule. See
[Decision 0974](../Documents/Decisions/0974-Construct-Fresh-Native-Owned-Storage-Domains.md).

The candidate version-two constructor and ABI 25 entry are selected by
[Decision 0980](../Documents/Decisions/0980-Bind-Native-Shared-Values-To-Budgets-And-Tool-Entries.md).
They add charged module/input mappings and separate supplied runtime/application
budgets within this same domain. Exact version-one dispatch and pinned
qualification/artifact identities remain unchanged. Normal ABI 25 host startup,
current generated-body execution and Debian/full qualification are not
established by the focused native fixtures described here.

## Caller and request

`Windvale_owned_domain_initialize`, implemented in
[`X64-Owned-Domain.wva`](../Runtime/Native/X64-Owned-Domain.wva), takes R8 as a
pointer to a complete readable version-one 112-byte or version-two 192-byte
request. The request remains unchanged. The following table describes version one.
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

## Version-one construction and publication

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

## Candidate version-two construction

Version two requires an exact 192-byte request and 192-byte context, a
1,216-byte [budgeted adapter](Windvale-Native-Budgeted-Storage.md) and a
2,176-byte [shared adapter](Windvale-Native-Shared-Storage.md). Physical and
accounting sizes remain 2,112 and 2,616. The first 104 request bytes retain their
field meanings, except that context/adapter extents use these selected sizes and
the root maximum is an unrestricted supplied u64 rather than an arena-capacity
limit. The version-two tail is:

| Offset | Width | Meaning |
| --- | --- | --- |
| 104 | 8 | Writable shared-state pointer, aligned sixteen |
| 112 | 8 | Immutable native data-directory pointer, aligned four |
| 120 | 8 | Exact directory byte length, a multiple of sixteen, at most 8192 |
| 128 | 8 | Immutable module-payload pointer |
| 136 | 8 | Module byte length, at most 4194304 |
| 144 | 8 | Immutable host-input pointer |
| 152 | 8 | Input byte length, at most 4194304 |
| 160 | 8 | Runtime bootstrap-intrinsic budget maximum |
| 168 | 8 | Application budget maximum |
| 176 | 16 | Reserved zero |

Empty directory/module/input extents require zero pointers; nonempty extents
require mapped nonzero pointers and checked ends. All ten supplied extents,
including the request, are pairwise disjoint: 45 comparisons. Every extent is
excluded from entry RSP minus 8,192 through plus eight before saved-register
writes. The five writable metadata regions are initially all zero: 8,312 bytes,
including context, or 8,120 without context. The constructor's own frame is
648 bytes; its conservative deepest selected call bound is 1,744 bytes.

At most 512 directory rows are examined. Each 16-byte row contains u32 module
offset, physical byte length, admitted maximum in physical bytes and kind.
Kinds are text 3, u32-array 4 or bytes 5. Length is at most maximum, and checked
offset plus maximum is at most module length. Kind 4 requires offset and length
to be multiples of four; descriptor formation uses length divided by four for
the logical element count. Maximum remains a physical byte bound.

The checked u64 sum of module length, input length, runtime maximum and
application maximum must fit the root maximum. The root must permit at least
four children, within the existing maximum of 64. After initializing physical,
accounting, budgeted and shared state, reserve four actual root children.
Consume the first two into exact-length mapped leases and immutable anchors
65/66. Keep the runtime and application children as distinct owned budgets;
source Main receives only the application owner. No payload copying, fabricated
lease or ambient allocator is used. Even an empty mapping owns a real zero-charge
lease. All owners share the supplied nonzero epoch.

Context 11 retains the earlier instruction/depth fields at 8/16 and zero
capability fields 24 through 111. Its selected tail is:

| Context offset | Width | Meaning |
| --- | --- | --- |
| 112 | 8 | Budgeted-state pointer |
| 120 | 8 | Linked budgeted adapter entry |
| 128 | 8 | Shared-state pointer |
| 136 | 8 | Linked shared adapter entry |
| 144 | 8 | Immutable native data-directory pointer |
| 152 | 8 | Exact directory byte length |
| 160 | 8 | Complete module anchor handle, slot 65 |
| 168 | 8 | Complete input anchor handle, slot 66 |
| 176 | 8 | Runtime-owned bootstrap budget token |
| 184 | 8 | Application budget token, transferred once to Main |

Publish size 192, fields and context version 11 last. Unexpected provider
refusal restores all five originally zero metadata regions and the original
sixteen-byte arena header. Directory/module/input bytes and request stay
unchanged. The selected initializer/adoption leaves acquire no physical payload,
so rollback has a fixed bound independent of mapped length.

## Candidate ABI 25 entry and host closure

[`X64-Owned-Entry.wva`](../Runtime/Native/X64-Owned-Entry.wva) retains the exact
ABI 24 prefix and adds `Windvale_shared_scalar_entry`,
`Windvale_shared_bytes_entry` and `Windvale_shared_result_close`. These are
ordinary native entry calls, not the leaf R8/R9 convention: RDX points at
context 11. Byte entry also receives RCX pointing at two contiguous 16-byte
cells, initially zero result followed by the complete immutable input
descriptor. That input is exactly anchor 66, including its generation, length
and admitted maximum. Scalar Main receives its transferred budget in R8; byte
Main receives the borrowed input-cell pointer in R8 and budget token in R9.
Generated code initializes its fuel/depth counters from the admitted context;
ordinary host-volatile registers are not incoming counter authority.

One independent admission path serves both entries and finalization. Before
pushes, check every complete control, arena, directory, mapped and byte-bridge
extent against the 8-KiB scratch exclusion. Authenticate exact versions/sizes,
the shared-to-budgeted association and the linked provider addresses, run
complete leaf validation, and require current whole immutable anchors 65/66.
Check all 36 pairs among nine domain extents, or all 45 pairs when the byte
bridge is included, and validate every directory row again. The runtime budget
is a live nonroot child of the retained root; fresh entry also requires a
distinct live application child. Call depth is nonzero and at most 64. The
entry's own frame uses 408 bytes and its deepest admission/teardown path uses
1,504; admitted generated frames have separate selected bounds.

Only after successful admission does entry clear application-token field 184
and pass its owner to Main. Another invocation refuses without transferring it
again. Scalar return and any byte-body trap invoke shared terminal teardown;
an earlier packed language trap survives a cleanup refusal, while cleanup
failure after scalar success returns packed status nine. Byte success leaves
the domain and returned semantic share live for independent host admission and
copy. The host then calls the finalizer on that same context. Finalization
requires consumed application field 184, repeats complete admission and releases
all shared/physical/accounting owners. It returns zero on closure or one on
refusal. A forged association with another otherwise-valid domain cannot close
that domain. Shared teardown owns closure of the lower adapter; calling lower
teardown first is forbidden while shared bindings remain.

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

On success, ownership of the domain belongs to its caller. Version-one Main's
existing entry wrapper tears down that domain after normal return or a packed
trap. A caller that never enters Main must invoke the selected adapter teardown itself.
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

The same [case helper](../Tools/Native/Native-Owned-Domain-Cases.mjs) adds nine
version-two constructor fixtures: real anchors/budgets, empty charged mappings,
checked sums and limits, directory rows, all extent pairs, complete null/alignment/
wrap/stack guards, initially dirty metadata and both provider rollback paths.
Refusals snapshot the complete 9,008-byte caller layout. Nine additional entry
fixtures compose that constructor with the production entry object and private
body stubs. They cover alias survival until host close, scalar/trap teardown,
earlier-trap preservation, charged empty input/results, complete domain/bridge
refusal snapshots, pre-push stack preservation and another valid domain's
survival under a forged finalizer context. The focused owner therefore has
25 cases: seven version-one constructors, nine version-two constructors and
nine ABI 25 entries. Narrow Windows native development runs passed all 25.
These stub bodies do not establish current compiler-generated Main execution,
normal ABI 25 startup, Debian/full qualification or installed delivery.
