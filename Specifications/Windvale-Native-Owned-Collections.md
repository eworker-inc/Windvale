# Windvale native owned collections

## Status

Candidate x64 compiler/runtime integration for a bounded WVB 1.24 subset.
The current source lowerer emits **ABI 24**, with **execution context 10**,
for that subset. Source signatures and WVB bytes are unchanged. ABI 22/23,
their consumers and pinned bootstrap products retain their recorded behavior.
This path has a dedicated test caller; normal installed launchers do not yet
construct context 10. It is not installed qualification or complete Libraries 1.0.

Reserved scalar Vectors now own physical storage and a canonical allocation
lease. An explicit scope release returns the storage and parent budget credit.
Normal return and terminal traps tear down the enclosing execution domain.
The interpreter's working storage and shared immutable backing still need
integration. No reduction in ordinary interpreter process memory is claimed.

## Admitted source and bytecode

The module is capability-free Core, with no static data and exactly one exported
`Main(Budget: Memoryˉbudget) -> i32`. WVB input is at most 1 MiB, Main code at
most 4,096 bytes, total locals at most 64, and the projected native frame at
most 128 cells (2,048 bytes). The shared complete WVB verifier runs before
native admission; the native ownership analysis is an additional restriction.

This first path admits scalar control flow, flat scalar records, enums,
resource-free variants, canonical allocation Results, budget splitting (`CE`),
reserved Vector construction (`CF`), affine local moves (`CD`), and explicit
owner release (`CD` immediately followed by `50`). Vector elements are `i32`,
`bool`, `u8`, `u32`, `i64` or `u64`; elements use eight-byte physical cells.
Vectors containing records or resource-owning values are rejected.

Opaque owners cannot be loaded as ordinary values, forged from integers,
copied, used after a move or consumed twice. Control-flow joins and backedges
must agree on their live owners: this target does not silently discard a
branch-specific owner. Explicit release accepts a budget or a Vector local;
general aggregate drops are not yet admitted. The entry wrapper owns remaining
owners until Main returns or traps. Helper calls, owned returns, borrowed
Vector access, append, growth, freeze, sharing, hosted capabilities and WVB
1.29/1.42 remain unsupported by this native subset.

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

Preparation now includes four products: lowerer, borrow probe, budget oracle
and owned Vector source fixture. The behavior phase reuses them. Explicit
digest-checked products may be supplied with `--lowerer`, `--borrow-probe`,
`--budget-oracle` and `--owned-vector` to the existing native owner.
