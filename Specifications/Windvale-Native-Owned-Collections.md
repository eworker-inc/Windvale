# Windvale native owned collections

## Status

Candidate x64 compiler/runtime integration for bounded WVB 1.24, 1.25, 1.27,
1.41 and scalar-collection 1.42/1.43 subsets.
The current source lowerer emits **ABI 24**, with **execution context 10**,
for that subset. Frozen source signatures are unchanged; WVB 1.43 versions
the new scalar mutation operations. ABI 22/23,
their consumers and pinned bootstrap products retain their recorded behavior.
This path has a dedicated test caller; normal installed launchers do not yet
construct context 10. It is not installed qualification or complete Libraries 1.0.

Reserved scalar Vectors now own physical storage and a canonical allocation
lease. Append, indexed scalar reads and replacement use that backing directly. Explicit
growth reserves replacement storage before releasing the old allocation.
Explicit scope release and automatic helper-return cleanup return storage and
parent budget credit. Main return and terminal traps tear down the enclosing
execution domain.
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
(`D0`), reserved growth (`D1`), parameter length (`E2`), indexed borrowed
scalar reads (`E3`) and scalar replacement (`E4`). Unit
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

Ordinary helper returns automatically release remaining budgets, scalar Vectors
and canonical allocation Results. Allocation Results are tracked by exact type
even when received from a call or parameter; tracking does not depend on a
constructor appearing in the same function. Successful Results release their
owned payload; failure Results have no such payload. Returned owners remain
live in the caller, including an allocation whose released parent budget must
retain its reservation until the final descendant releases.

At most 64 owned local slots are tracked per function. Functions needing implicit
cleanup use a stack ledger with a count, capacity and ordered local ranks: at
most 264 bytes, included in the existing 240-cell frame limit. Owned parameters
enter in parameter order; stores append acquisitions and moves remove them.
Cleanup walks remaining acquisitions in reverse order. Borrowed parameters do
not enter the ledger. Immutable code metadata maps ranks to local offsets and
the four admitted owner forms; no additional heap allocation is needed.
The [frame helper](../Runtime/Native/X64-Owned-Frame-Cleanup.wva) calls the
existing generation-checked domain release operation and preserves instruction
fuel and call depth. Invalid internal ledger state or release traps; Main's
wrapper reclaims the enclosing domain. This does not add general block-exit
cleanup, arbitrary owner-bearing aggregates or shared immutable last-share
release. Existing join and backedge restrictions remain.

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

Scalar replacement through an exclusive parameter updates the caller's retained
backing and returns the old value. WVB 1.43 also permits append through that
parameter; earlier minors retain their non-parameter append rule. Parameter
length observes the handle without moving its owner. Mutation accepts neither
an immutable parameter nor a projected borrowed payload. The complete verifier
rejects mutation while an indexed element loan remains live.

Growth through a borrowed helper remains rejected: replacement must update
the caller's owning slot, while this calling convention passes a handle value.
Mutable element views are not implemented. The receiver-retaining length
operation (`CA`) retains its existing loan behavior.
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

The candidate [fresh-domain constructor](Windvale-Native-Owned-Domain.md)
now checks and initializes these states from one bounded private request,
then publishes context 10. It refuses nonzero metadata and restores the
supplied bytes if initialization fails. Normal launchers still need an explicit
mapped layout and startup integration; a constructor alone does not close that gate.

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
Replacement applies the same full-width index check before either reading or
writing. It returns the prior scalar and writes one cell without changing the
handle, length, capacity, lease or charge. The private access selector is `3`;
RAX carries the handle, RDX the index and R8 the replacement, with the previous
cell returned in RDX. This does not change ABI 24/context 10.

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
Vector through recursive calls, appends in its owning caller and exclusive
helpers, replaces through nested exclusive helpers, observes parameter length,
forwards an immutable borrow, reads it and releases it in the caller. A bytecode
variation reads through exclusive mutable parameters.
Its one, 1,000 and 32,768 iteration cases must fit the same 64-byte arena and
48-byte peak charge, with zero live charge at completion. Main is not first in the function directory; the
Vector travels through a stack argument as well as a return. Checks cover the
64-function boundary, allocation refusal, nested fuel/depth failure, entry
recursion rejection and copied owners.
Borrowed-read and replacement bounds traps must reclaim the same domain. Malformed helper
cases reject mutation through an immutable parameter, ownership escape and
release of a borrowed Vector.

The [automatic-cleanup workload](../Tests/Fixtures/Native-X64/Owned-Helper-Cleanup.wv)
uses ordinary source helpers that leave budgets, Vectors and both canonical
allocation Results at return. A constructor returns an allocation while its
parent budget releases; later caller cleanup must still release that allocation
and credit its ancestors. One, 1,000 and 32,768 iteration cases enforce the same
64-byte arena and 48-byte peak physical charge, with zero live charge at exit.
Budget-only cases allocate no physical backing. Repeated physical refusal uses
a 16-byte arena; constructor traps must close the whole domain. Odd budget and
even lease generation histories remain distinct.
The same owner rebuilds the frame template, checks its private entry offsets,
and executes bounded ledger tests for acquisition order, transfer/reacquisition,
all four owner forms, malformed state and the 64-rank boundary. Its setup and
boundary checks use bounded runtime loops rather than unrolled assembly.

The [six-scalar workload](../Tests/Fixtures/Native-X64/Owned-Vector-Scalar-Mutation.wv)
checks the old and updated values for every admitted scalar kind, including
signed limits and the high half of `u32`/`u64`. Its six sequential allocations
must peak at 32 charged bytes and finish with zero live storage. These are
bounded native storage checks, separate from interpreter guest accounting or
process peak-memory measurements.

Preparation includes eight products: lowerer, borrow probe, budget oracle,
scope, growth, append-refusal, helper and six-scalar fixtures. The behavior phase reuses them. Explicit
digest-checked products may be supplied with `--lowerer`, `--borrow-probe`,
`--budget-oracle`, `--owned-vector`, `--owned-growth`, `--owned-append`, `--owned-helpers`
and `--owned-scalar-helpers`
to the existing native owner.
Its `--owned-helper-memory` selection checks only helper mutation, transfers,
refusal, bounds and cleanup with a digest-checked lowerer. It also builds the
small automatic-cleanup fixture through the prepared current compiler; the
eight supplied-product options above do not supply that fixture. The existing
memory-budget execution owner adds `--vector-mutation-products` to check the
same bytecode on another host without rebuilding a compiler. Neither selection
is independent compiler reconstruction or full cross-host qualification.
