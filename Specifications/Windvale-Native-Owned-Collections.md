# Windvale native owned collections

## Status

Candidate x64 compiler/runtime integration for bounded WVB 1.24, 1.25, 1.27,
1.41 and scalar-collection 1.42 subsets.
The current source lowerer emits **ABI 24**, with **execution context 10**,
for that subset. Source signatures and WVB bytes are unchanged. ABI 22/23,
their consumers and pinned bootstrap products retain their recorded behavior.
This path has a dedicated test caller; normal installed launchers do not yet
construct context 10. It is not installed qualification or complete Libraries 1.0.

Reserved scalar Vectors now own physical storage and a canonical allocation
lease. Append and indexed scalar reads use that backing directly. Explicit
growth reserves replacement storage before releasing the old allocation.
An explicit scope release returns the storage and parent budget credit.
Normal return and terminal traps tear down the enclosing execution domain.
The interpreter's working storage and shared immutable backing still need
integration. No reduction in ordinary interpreter process memory is claimed.

## Admitted source and bytecode

The module is capability-free Core, with no static data, at most 64 functions,
and exactly one exported `Main(Budget: Memoryˉbudget) -> i32`. WVB input is at
most 1 MiB. Each function has at most 64 parameters, 4,096 code bytes, 128 total
locals including parameters, and a projected native frame at
most 240 cells (3,840 bytes), below one Windows stack page. The shared complete WVB verifier runs before
native admission; the native ownership analysis is an additional restriction.

This first path admits scalar control flow, flat scalar records, enums,
resource-free variants, canonical allocation Results, budget splitting (`CE`),
reserved Vector construction (`CF`), affine local moves (`CD`), and explicit
owner release (`CD` immediately followed by `50`). Vector elements are `i32`,
`bool`, `u8`, `u32`, `i64` or `u64`; elements use eight-byte physical cells.
Vectors containing records or resource-owning values are rejected.

The additional operations are unit constants (`C3`), length (`CA`), append
(`D0`), reserved growth (`D1`) and indexed borrowed scalar reads (`E3`). Unit
uses its exact shape 20 identity; it is not interchangeable with an integer.
Append uses the canonical `Result<unit, Vectorˉappendˉfailure<T>>`; growth uses
`Result<unit, Allocationˉfailure>`. Native admission checks their exact layouts.

Opaque owners cannot be loaded as ordinary values, forged from integers,
copied, used after a move or consumed twice. Control-flow joins and backedges
must agree on their live owners: this target does not silently discard a
branch-specific owner. Explicit release accepts a budget or a Vector local;
general aggregate drops are not yet admitted. The entry wrapper owns remaining
owners until Main returns or traps. The current source borrow validator freezes
a Vector after indexed access for the remaining function; moving it into a
later `using` binding is rejected. Such programs use terminal domain cleanup.
Direct helper calls (`40`) transfer owned budgets, scalar Vectors and canonical
allocation Results through the existing native argument and return convention.
Recursive helpers share the entry's instruction and call-depth limits. The
entry template is emitted once at Main, at any function-directory position;
helper allocation and release operations call that same template. Calling Main
from bytecode is rejected because its wrapper owns initialization and teardown.

Every ordinary helper return must have consumed, explicitly released or
returned all its tracked owners. The native target rejects otherwise valid WVB
that needs implicit helper-local cleanup. Allocation Results are tracked by
their exact type even when received from a call or parameter; tracking does not
depend on a constructor appearing in the same function. At most 64 owned local
slots are tracked per function. Terminal helper traps unwind to Main's wrapper
and reclaim the execution domain. Returned owners remain live in the caller.

Borrowed scalar Vector helper parameters use the existing WVB shapes 26
(immutable) and 27 (exclusive mutable) in admitted minor versions at least 1.26.
Both pass the owner's generation-checked handle without moving its allocation
lease. Indexed scalar reads accept either parameter. The complete WVB verifier
proves the call's lifetime
and alias exclusions before native admission. The native owner-load restriction
permits only a direct call preceded by at most 64 remaining local loads.
Borrowed helpers do not release the caller's owner on ordinary return; the
caller can explicitly release it after the call completes. Returning a borrowed
parameter as an owner, taking it or releasing it remains invalid.

Growth through a borrowed helper remains rejected: replacement must update
the caller's owning slot, while this calling convention passes a handle value.
Append through a borrowed parameter remains outside the source, complete WVB
verifier and native execution subset.
Parameter length (`E2`) is not yet integrated. The receiver-retaining length
operation (`CA`) does not gain a temporary-loan drop through this change.
Other borrowed helper parameters, bytes/text values, aggregate drops,
arbitrary owner-bearing records, freeze, sharing, hosted capabilities and WVB
1.29 remain unsupported. WVB 1.42 Copy-record collection elements also reject;
admission of scalar helpers does not admit that version's entire vocabulary.

## Context and ownership

Context 10 is exactly 136 bytes, little-endian. The caller supplies a complete,
mapped, writable context and upholds the existing x64 calling convention.
The entry pointer is `Main`; RDX points to its context. A context version or
size mismatch returns packed status 9 before reading the extended fields.

| Offset | Width | Meaning |
| --- | --- | --- |
| 0 | 4 | Version 10 |
| 4 | 4 | Size 136 |
| 8 | 8 | WVB instruction budget |
| 16 | 8 | Main call-depth budget |
| 24 | 88 | Earlier context slots; zero in this capability-free target |
| 112 | 8 | Aligned pointer to the exclusive initialized budgeted adapter |
| 120 | 8 | Exact `Windvale_budgeted_storage` function entry |
| 128 | 8 | Reserved zero |

The caller supplies the adapter's disjoint accounting state, physical state,
binding metadata and arena, and a fresh non-reused domain epoch. All these
extents remain mapped for the complete invocation. The context and generated
stack frames must also be disjoint from those extents. Provider pointers and
complete extents are trusted caller preconditions, never source authority.

At entry the wrapper validates the current root budget token (identity 1,
generation 1) through the adapter. Main receives that canonical opaque token,
not the historical packed remaining-byte/child counters. Budget and Vector
locals occupy the low eight bytes of native cells; Vector locals contain only
generation-checked physical handles. The private binding table retains the
matching full lease. No public pointer or lease constructor is added.

All runtime calls preserve nonvolatile registers and the instruction/depth
registers. The common WVB instruction charge still applies to each emitted
operation. The bounded adapter work does not acquire additional host memory.
The compiler embeds the linked entry/helper template from
[`X64-Owned-Entry.wva`](../Runtime/Native/X64-Owned-Entry.wva); the existing
native owner rebuilds that template and checks its bytes and helper offsets.

## Charging, refusal and release

The caller prepays the adapter's fixed 5,816 metadata bytes separately from
payload. Vector capacity `N` requires `8 + 8*N` payload bytes: a zero length,
a 32-bit capacity, and the committed scalar cells. The physical lease charge
is `align_up(24 + 8*N, 16)`, including the physical allocation header. This is
the named target's physical charge, not the reference interpreter's separate
serialized-heap representation. Maximum capacity is 2,047 cells. Successful
construction zeroes the committed backing and owns it until release.

Zero capacity is an invalid-limit terminal failure (packed status 5). A positive
capacity above the target limit returns `Targetˉunaddressable`; an unrepresentable
requested charge saturates to `u64` maximum. Insufficient budget returns
`Budgetˉexhausted`, exhausted generation/slot authority returns
`Providerˉunavailable`, and physical reserve refusal returns `Fragmented`.
Failures report the requested physical charge and the budget's pre-call
available bytes. Every typed construction outcome consumes the supplied budget;
refusal releases it locally. Split refusal preserves its parent exactly.

Append within capacity writes one scalar cell and increments length without
allocating. A full Vector returns `Capacityˉexhausted` with its maximum and the
original input value, preserving length and contents. Indexed reads require
`index < length`; an out-of-range index is a terminal invalid-limit failure.

Growth requires a strictly larger positive capacity. It borrows both the Vector
and funding budget. The complete replacement charge must fit the funding
budget while the old allocation is still live; this deliberately exposes peak
memory rather than charging only the difference. Adapter operation 11 reserves
zeroed backing, copies the header and live cells, transfers the owning local
to the new handle, and releases the old backing and lease. The helper updates
the capacity header after the adapter succeeds. Typed refusal leaves the
Vector, funding budget, accounting generations and both storage domains
unchanged. The funding budget remains owned after success or refusal. Invalid
limits are terminal; the other allocation reasons match construction above.

Explicit Vector release uses the validated handle-to-lease binding, releases
the committed allocation, and credits its accounting tree exactly once.
Releasing a budget with live descendants defers its parent's credit until the
last descendant disappears. No provider call can prevent local release.

After every returned Main success or packed trap, the wrapper tears down both
domains, clearing outstanding owners while preserving generation histories.
An existing trap remains the reported result if teardown detects corruption;
otherwise a teardown failure changes success to packed status 9. Invalid initial
contexts/domains refuse before Main and are not claimed to have been reclaimed.
The caller retains responsibility for the outer arena and metadata extents.

## Development evidence

The existing `native-x64-lowering-development` owner builds the
[source workload](../Tests/Fixtures/Native-X64/Owned-Vector-Scope.wv), lowers it
with the current compiler, and executes the linked native code on Windows and
Debian. Its 1,000 successful scope iterations must fit a 64-byte arena, peak at
32 charged bytes, preserve all retired generations and finish with zero live
charge. Tests also cover typed refusal, invalid limits, fuel/depth cleanup,
old/short contexts, deterministic output and rejected ownership violations.
This is a bounded allocation-reuse result, not a general working-set benchmark.

The growth workload appends 7, grows capacity from one to two, appends 35 and
reads back 42. Its arena is 80 bytes: old charge 32 plus replacement charge 48,
with zero live charge after return. A second workload checks that a full append
returns the original item. Refusal and index-boundary mutations also require
terminal cleanup; the adapter tests separately compare complete pre/post
snapshots for ordinary refusals.

The [helper workload](../Tests/Fixtures/Native-X64/Owned-Vector-Helpers.wv)
constructs a Vector in a helper, returns its allocation Result, transfers the
Vector through recursive calls, appends in its owning caller, forwards an
immutable borrow through two helpers, reads it and releases it in
the caller. A bytecode variation reads through exclusive mutable parameters.
Its one, 1,000 and 32,768 iteration cases must fit the same 64-byte arena and
32-byte peak charge, with zero live charge at completion. Main is last in the function directory; the
Vector travels through a stack argument as well as a return. Checks cover the
64-function boundary, allocation refusal, nested fuel/depth failure, entry
recursion rejection, copied owners and a helper retaining an owner at return.
The borrowed-read bounds trap must reclaim the same domain. Malformed helper
cases reject mutation through an immutable parameter, ownership escape and
release of a borrowed Vector.

Preparation includes seven products: lowerer, borrow probe, budget oracle,
scope, growth, append-refusal and helper source fixtures. The behavior phase reuses them. Explicit
digest-checked products may be supplied with `--lowerer`, `--borrow-probe`,
`--budget-oracle`, `--owned-vector`, `--owned-growth`, `--owned-append` and `--owned-helpers`
to the existing native owner.
