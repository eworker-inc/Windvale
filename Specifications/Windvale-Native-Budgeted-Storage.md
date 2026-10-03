# Windvale native budgeted storage

## Status

Current runtime-private x64 implementation, version 1. The
[`Windvale_budgeted_storage` adapter](../Runtime/Native/X64-Budgeted-Storage.wva)
connects the existing generation-safe budget accounting state to
[owned physical storage](Windvale-Native-Owned-Storage.md). Successful reserve
consumes a budget into a lease and owns real committed backing; release returns
that backing and credits the accounting tree. Refusal changes neither resource.

The candidate [native owned-collections path](Windvale-Native-Owned-Collections.md)
uses this adapter for scalar Vector reservation, replacement growth and release. Wider collection
code and interpreter working storage still need integration. Source signatures,
WVB, native ABI 22/23 and pinned bootstrap identities are unchanged. No ordinary
interpreter process-memory improvement or installed qualification is claimed.

## Ownership and authority

The adapter has no host calls or ambient allocation. Its caller supplies an
initialized owned-storage domain, an existing canonical 2,616-byte budget state,
1,088 bytes of adapter metadata and one request. The backing arena and all four
other extents are exclusive, complete, mapped, writable and pairwise disjoint.
The physical domain and adapter share one nonzero, never-reused epoch. The
caller must uphold the same mapped-pointer and borrow-lifetime preconditions as
the physical leaf. This is not an interface for accepting arbitrary host
pointers from untrusted source code.

After initialization the adapter exclusively owns every physical slot. Direct
physical reserve/release calls would break that ownership boundary. Budget
splitting and release of **budget** owners may use the existing canonical
accounting operations while serialized with adapter calls. They must preserve
generation history and outstanding leases. Creation, release, root collection
or teardown of **lease** owners must go through the adapter while it owns their
physical storage. The implementation rejects a mismatched binding, duplicate
lease, unbacked live lease, or changed physical generation before mutation.

The caller prepays 5,816 bytes of fixed metadata: 1,088 adapter bytes, 2,112
physical-state bytes and 2,616 accounting bytes. Payload and its allocation
header are charged separately. This profile retains the physical leaf's 64-slot,
4 MiB per-allocation and 16 MiB arena limits. These are provider limits, not
universal Language 1.0 limits.

## Canonical accounting relationship

The adapter uses the exact private state and token layouts implemented by
[`Wvb-Scalar-Interpreter-Memory-Budget-Core.wv`](../Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Memory-Budget-Core.wv).
It does not introduce another budget-token encoding. The native
[read-only validator](../Runtime/Native/X64-Memory-Budget-Validation.wva) checks
the header, all 65 entries, parent indices, active/owner flags, nonzero
generations, acyclic ancestor chains, byte limits, child counts and checked
64-bit sums of child maxima before the adapter can write either state.

An accounting token contains a 32-bit identity followed by a 32-bit generation.
Odd generations own budgets; even generations own leases. A lease adds its
64-bit maximum retained bytes, 64-bit current retained bytes and 32-bit alignment
ceiling, for exactly 28 bytes. The epoch scopes these tokens to this adapter
domain. Tokens remain hidden from source programs.

The committed allocation's maximum and current retained bytes both equal the
physical charge `align_up(capacity + 16, 16)`. The charge must fit the consumed
budget's maximum minus its outstanding child reservations. It does not change
the reservation already held by that budget's parent. Logical shrinking keeps
the full committed charge; subsequent logical growth within capacity remains
allocation-free. Replacement growth uses a separate full-charge reservation
while the old allocation remains live. In-place capacity growth, sharing, transfer to another domain
and a different collection's public accounting formula are outside this leaf.
Compiler integration must preserve each collection's accepted charging contract.

Reserve advances the selected odd budget generation once, exactly as
`Constructˉallocationˉlease` does. The last odd generation cannot advance and
refuses without mutation. Release clears the lease's owner flag. A node becomes
inactive only after its owner and final child disappear; its full maximum is
then subtracted from its parent's reservation and the parent's child count is
decremented. This continues through unowned ancestors, at most 65 nodes, while
preserving every retired generation. Releasing physical storage is immediate
even if surviving child budgets defer release of the parent reservation.

## Layouts and calling convention

All integer fields are unsigned and little-endian. R8 points to a
16-byte-aligned 1,088-byte adapter state. R9 points to an 8-byte-aligned 96-byte
request. EAX returns status; nonvolatile registers and R10/R11 survive. The
existing native x64 shadow-space and stack-alignment requirements apply.
The adapter uses 392 bytes below its entry stack pointer, plus the bounded
nested validator or physical-leaf call. Its temporary request and extent table
are stack-owned and do not outlive the call.

| Adapter offset | Width | Meaning |
| --- | --- | --- |
| 0 | 4 | Magic 1396856663 after initialization; initially zero |
| 4 | 4 | Version 1 |
| 8 | 4 | State size 1088 |
| 12 | 4 | Reserved zero |
| 16 | 8 | Address of the initialized 2112-byte physical state |
| 24 | 8 | Address of the canonical 2616-byte accounting state |
| 32 | 8 | Nonzero epoch, equal to the physical domain's epoch |
| 40 | 4 | Closed flag, initially zero |
| 44 | 20 | Reserved zero |
| 64 | 1024 | 64 bindings, each a physical handle followed by an accounting token |

Each binding is 16 bytes and corresponds to the same physical slot index.
Both words are zero when inactive. A live binding's physical generation must
match its slot, and its accounting token must identify a distinct current owned
lease. An active accounting lease whose owner has been released may remain
without backing while children survive; it is not an owned live lease.

The first 64 request bytes use the physical-request fields with these changes:
size at +4 is **96**, operation at +8 is 0 through 12, and the physical maximum
field at +56 is always zero. The adapter computes authority from the budget,
rather than trusting a caller-supplied byte maximum. The tail is:

| Request offset | Width | Meaning |
| --- | --- | --- |
| 64 | 8 | Input budget token for reserve; exact lease token prefix for existing-owner operations |
| 72 | 8 | Lease maximum, output on reserve; input for resize, release and inspect |
| 80 | 8 | Lease current charge, output on reserve; input for resize, release and inspect |
| 88 | 4 | Lease alignment ceiling, output on reserve; input for resize, release and inspect |
| 92 | 4 | Reserved zero |

Reserve requires zero handle, lease metadata and result fields. Success publishes
the physical handle at +24, charge at +44, borrowed pointer at +48 and complete
lease at +64. Operations 2 through 4 require both the matching physical
handle and the exact 28-byte lease. Initialize, teardown and validation require all
operation-specific fields to be zero. All other unused fields remain zero as
in the physical contract.

## Operations, refusal and teardown

0. **Initialize:** require empty physical slots and bindings and no live owned
   accounting leases; existing valid budget owners and their reservations are
   allowed. Validate the supplied physical domain without changing it, validate
   the complete accounting tree, then publish adapter magic. Initialization does
   not construct budgets, acquire arena memory or reset generations.
1. **Reserve committed:** validate the input budget and available authority,
   then call physical reserve using a private request. On physical success,
   advance the accounting generation, bind the lease and publish both results.
   No fallible allocation or provider operation remains after physical success.
2. **Resize logical length:** validate the bound lease and delegate to the
   existing committed capacity. Growing re-exposed bytes are zeroed. Accounting
   remains charged for the same complete capacity.
3. **Release:** prevalidate both domains and their relationship, release the
   physical allocation, remove the binding and locally finalize the accounting
   owner and any now-unowned ancestors. No provider acquisition is involved.
4. **Inspect:** validate the exact owner pair and return the physical borrowed
   pointer and charge. It creates no additional owner.
5. **Teardown:** prevalidate everything, release all physical allocations, clear
   bindings and all active accounting entries while retaining their generations,
   and close both physical and adapter domains. All later adapter operations
   refuse; destroying and recreating a domain requires a new epoch.
6. **Split budget:** +64 is the parent budget token, +72 the child maximum and
   +88 its maximum child count (0 through 64). Other operation fields are zero.
   Success publishes the child token at +64 and pre-call parent availability at
   +80. Reserve the child's full maximum and advance an inactive slot to its
   next odd generation. Never reuse a retired generation or the root slot.
7. **Release budget:** +64 is an owned odd-generation budget token; all other
   operation fields are zero. Release that owner and finalize unowned ancestors
   only after their last child disappears. This operation cannot release leases.
8. **Query budget:** same input as release budget. Publish available authority
   at +80 without changing any domain state.
9. **Release bound handle:** +24 is the physical owner; all other operation
   fields are zero. Full binding validation supplies the hidden lease authority,
   then performs the same release as operation 3. Source values cannot forge
   handles; the caller keeps them inside the exclusive domain.
10. **Inspect bound handle:** same input as operation 9; return its borrowed
    pointer and charge without acquiring another owner. No lease metadata is
    published. The same lifetime and serialization rules as operation 4 apply.
11. **Replace bound backing:** +24 is the old physical handle, +64 a borrowed
    owned budget token, +12 the new capacity, +32 alignment, +36 new logical
    length and +88 the prefix byte count to preserve. Lease fields +72/+80 and
    result fields must be zero. Capacity must increase; the prefix must fit
    both old and new logical lengths. Preflight the complete replacement charge,
    one additional budget child and an available accounting generation. Reserve
    new zeroed storage while the old backing is live, copy the prefix, commit a
    new child lease and binding, then release and credit the old pair. Success
    publishes the replacement handle, charge and pointer at +24/+44/+48. The
    borrowed budget token remains unchanged; no lease metadata is published.
12. **Validate complete domain:** require zero operation-specific fields and
    run the full existing physical, accounting and binding preflight without
    changing either domain. This additive runtime-private operation is selected
    by [Decision 0977](../Documents/Decisions/0977-Add-Native-Reserved-Byte-Builders-And-Shared-Storage.md)
    for the candidate [shared-storage adapter](Windvale-Native-Shared-Storage.md).
    It works when no budget or backing owner remains. Previously pinned
    artifacts retain their recorded operation set; no public ABI changes here.

Replacement advances an inactive nonroot accounting slot exactly as a split
followed by lease construction would, skipping slots whose next even generation
would overflow. No generation or reservation is consumed on refusal. The copy
is bounded by the physical 4 MiB capacity ceiling. Serialized preflight makes
old-pair release infallible after commit; a detected internal invariant failure
there is terminal, never reported as an ordinary atomic refusal.

Status codes retain the physical vocabulary: 1 invalid request, 2 insufficient
budget authority, 3 physical exhaustion/fragmentation, 4 stale or mismatched
owner, 5 corrupt state, 6 exhausted generations/slots, 7 length beyond capacity,
8 unsupported target capacity/alignment and 9 closed domain. These are private
statuses, not a new public `Allocationˉreason` encoding.

Every refusal leaves the accounting state, bindings, physical state, arena and
request input fields unchanged; only status at +40 may be written. Null or
misaligned supplied control pointers, overflowing extents or any overlap return
1 without writing request status. Complete mapped extents remain a caller
precondition. No error priority is promised for multiply-invalid requests.

All calls are serialized. Full preflight precedes mutation; intermediate stores
are not concurrently observable. The budget state uses at most 65 bounded
ancestor walks and 65-by-64 child scans. Binding checks visit 64 slots and at
most 2016 earlier bindings. The physical validator retains its existing bounds.
Release and teardown are bounded local operations independent of provider loss.

## Verification and remaining integration

The existing native lowering development owner includes eighteen adapter cases
alongside the ten physical-storage cases. It builds the
[accounting oracle](../Tests/Fixtures/Native-X64/Budgeted-Storage-Accounting-Oracle.wv)
from the existing Windvale budget core and compares all 2,616 bytes at fourteen
matching checkpoints, including consumption, deferred parent release, ancestor
credit and teardown. The native implementation is not the oracle's generator.
Corruption tests cover cycles, overflowing child sums, mismatched bindings,
stale tokens and request overlap. Rejected operations compare snapshots of all
state, backing and request inputs. The repeated-credit workload performs 32,768
Split/reserve/release cycles in a 64-byte arena, returns the parent's reservation to zero each cycle,
and enforces peak physical charge 48 with fixed metadata 5,816 bytes.
Replacement adds exact content/padding checks, accounting-oracle comparison,
full-state refusal snapshots, insufficient full-charge authority, fragmented
backing, generation exhaustion and 1,000 replacement cycles in a 112-byte
arena with peak charge 112 and zero final live charge. The owner runs at most
two independent native fixtures concurrently and drains them before cleanup.

Use `Test-Native-Unsafe-Write-Pointer-Lowering.mjs <host> <repo> --owned-storage`
with a prepared compiler, or append `--budget-oracle <wvb> <sha256>` to supply
an explicitly checked oracle product. The ordinary `--prepare-only` phase now
prepares six products: lowerer, borrow probe, accounting oracle and the scope,
growth and append-refusal Vector fixtures. The
prepared behavior phase must reuse them and may not reconstruct the compiler.
Owned helper transfer, general aggregate cleanup, shared immutable
backing, in-place capacity growth and interpreter migration remain integrations
after the bounded native Vector path.
