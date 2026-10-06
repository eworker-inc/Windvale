# Decision 0980: Bind native shared values to budgets and tool entries

## Status

Accepted candidate implementation direction, 3 October 2026, under the
maintainer's request to complete one compiler, runtime and maintained-consumer
path before final verification. The contracts below version that path; they do
not establish passing execution, cross-host qualification or released delivery.

## Problem and resulting behavior

Reserved builders alone cannot replace the compiler's historical storage.
Literals, input, owning slices, shared aggregate fields and temporary values
must preserve both a backing identity and its charge. A byte-result tool must
also keep its result alive until the caller has validated and copied it.

Extend the existing budgeted and shared adapters, native lowerer and tool-entry
boundary. Keep one physical allocator. Ordinary copies allocate no metadata;
last-share release makes dynamic storage reusable and returns its lease charge.
Mapped literals and input hold real leases until their enclosing runtime anchor
is released. Move a maintained compiler serializer and its actual callers to
the reserved builder through a supplied application budget.

## Mapped immutable backing

Budgeted adapter version two has 1,216 bytes: the existing 1,088-byte prefix and
two 64-byte mapped entries at 1,088 and 1,152. Physical indices 1 through 64 keep
their meanings; mapped handles use indices 65 and 66 with the same complete
32-bit generation and 32-bit index handle convention.

| Entry offset | Width | Meaning |
| --- | --- | --- |
| 0 | 4 | Preserved generation |
| 4 | 4 | Active flag, zero or one |
| 8 | 8 | Complete immutable mapped extent address |
| 16 | 4 | Capacity |
| 20 | 4 | Logical length, equal to capacity |
| 24 | 8 | Exact retained charge |
| 32 | 8 | Canonical lease token |
| 40 | 8 | Lease maximum |
| 48 | 8 | Lease current retained bytes |
| 56 | 4 | Alignment ceiling, one |
| 60 | 4 | Reserved zero |

The version-two budgeted request is 112 bytes. Its first 96 bytes retain their
field meanings; the tail is mapped pointer at 96, length at 104 and reserved
zero at 108. New selector 13 consumes a valid supplied child budget into a real
canonical lease for an immutable mapped span. Its exact charge and lease
maximum/current are the span's length. The 128 bytes of mapped-entry metadata
are prepaid separately; there is no fabricated payload header or hidden copy.
Lengths are at most 4 MiB. A zero length requires a zero pointer and still
creates a real zero-charge lease. The caller must map and retain the complete
nonempty extent and prevent mutation while any share survives.

Inspect, release, full validation and teardown cover mapped entries and every
owned live lease. Mapped resize, replacement and mutation refuse. Preflight
rejects duplicate leases, mismatched generation, overlapping controlled extents,
unbacked owned leases and corrupt accounting before mutation. Teardown releases
mapped leases as well as physical leases and preserves retired generations.
Version-one states and requests retain their exact selected behavior; pinned
artifacts are not reinterpreted as version two.

Shared adapter version two has 2,176 bytes, comprising its existing 64-byte
header and 66 entries of 32 bytes. Its request remains 128 bytes with version
two. New selector 14 adopts a mapped immutable span: budget token at 32,
length at 40, pointer at 56, and zero unused fields. It publishes the full
generation-checked handle, pointer, length, maximum, charge and initial share.
Constructor refusal releases a valid consumed budget as required by the
accepted constructor contract; malformed requests preserve state. The existing
shared append, borrow, retain, release and teardown operations cover mapped
immutable backing without granting mutation to it.

A runtime holds one anchor for the module's contiguous immutable data blob and
one for the immutable input blob. Literal and owning-input values acquire
shares of those same backings; there is no separately copied literal pool.
The runtime anchors keep their real charges until invocation closure. Copies
and ranges do not multiply that charge. The canonical accounting tree still
has 65 entries; mapped leases and runtime/application budget owners consume
entries in that same bound. The 64 physical slots do not imply 64 additional
simultaneously available leases when those owners are live.

## Native ABI 25 value descriptors

WVB 1.44 selects native ABI 25 for this path. Bytes and text remain 16-byte
descriptor values passed through the existing descriptor calling convention:
packed identity at zero, logical length at eight and admitted maximum at
twelve. This does not preserve the historical raw-pointer descriptor meaning.

For a nonempty backed value, the identity's high word is the complete backing
generation. The low word is `(Start << 7) | Slot`, where Slot is 1 through 66,
Start is an absolute byte offset below 4 MiB and the top three low-word bits are
zero. Decoding recovers the complete leaf handle before provider access. Checked
start/length/maximum validation establishes containment in that backing before
reading. An empty owning range normalizes Start to zero while preserving its
nonzero backing identity. This represents a one-past-end empty range without
losing its lease. The all-zero identity denotes only an unbacked canonical
empty value, never a charged frozen builder.

An owning slice acquires a share and carries its range within the same backing.
Replacing a destination retains its incoming share before releasing its old
share. Consuming stores and returns transfer ownership. Locals, evaluated
temporaries, outgoing arguments and shared fields in admitted records and active
variants participate in cleanup. Fixed-array aggregates remain outside the
initial native subset; source support does not admit them into this backend.
Reclaim dead temporaries during loops;
retaining every historical temporary until function return is insufficient.
Retain/release traversal is bounded and uses the same independently admitted
shape evidence as ordinary native aggregate handling.

Borrowed Slice<u8> uses a separate 16-byte cell: full leaf handle, absolute
start and length. It acquires no share, has no mutation authority and cannot
escape its independently proved owner lifetime. Builders remain unique owners.
The source opaque builder, borrowed Slice and exclusive borrowed-builder shapes
are the WVB tags selected by Decision 0979, not user-forgeable records.

The private native stack types are Slice 7, builder 8 and the existing exact
Budget type 10/nominal identity 25. Builder and Budget cells contain their opaque
handle or token in the first word and an ownership marker in the second word:
one for an owned value, zero for a borrowed or moved cell. Moves transfer and
clear the source marker; aliases acquire no release obligation. Malformed
markers refuse. Budget validation uses the canonical budget domain rather than
interpreting tokens as builder handles. This ABI-25 rule does not change ABI-24
padding or source types.

ABI-25 WVOs keep the existing object format. They export `Main`,
`Windvale_shared_data_blob`, `Windvale_shared_data_directory` and
`Windvale_shared_result_close` in canonical name order. The blob covers only
the exact immutable payload, including checked zero alignment between u32
arrays. The separately aligned directory contains at most 512 sixteen-byte
rows; its bytes are not included in the blob charge. Empty spans retain valid
zero-size data symbols and the host normalizes their pointers to zero. The
close symbol aliases the independently checked entry template inside Main.
Provider relocations identify only `Windvale_budgeted_storage` and
`Windvale_shared_storage`; object admission and the existing segmented linker
must validate and resolve these explicitly.
The initial writer requires the blob, zero directory-alignment padding and
directory together to fit its 4 MiB source-bytes region. Reject before
concatenation if that publication bound is exceeded. This is an explicit
candidate writer limit, distinct from the mapped blob's storage limit.

## Context 11 and supplied budgets

Context 11 is exactly 192 bytes. Its existing leading instruction/depth fields
retain their meanings. Capability-free entries require zero provider fields.
The additive fields are:

| Offset | Width | Meaning |
| --- | --- | --- |
| 112 | 8 | Existing budgeted adapter state |
| 120 | 8 | Linked budgeted adapter entry |
| 128 | 8 | Shared adapter state |
| 136 | 8 | Linked shared adapter entry |
| 144 | 8 | Immutable native data directory |
| 152 | 8 | Exact directory byte length |
| 160 | 8 | Module-data anchor handle |
| 168 | 8 | Input anchor handle |
| 176 | 8 | Runtime-owned bootstrap-intrinsic budget token |
| 184 | 8 | Application budget token transferred to source Main |

Directory rows are 16 bytes: absolute offset within the module-data backing,
length, admitted maximum and ordinary native data kind, each u32. There are at
most 512 rows and no pointers in these rows. Data and input spans are each at
most 4 MiB. Directory containment, supported kind, maxima and integer arithmetic
are checked before a literal descriptor is formed.

The runtime owns the root accounting domain. It creates exact module/input
leases and distinct rights-reduced runtime and application budgets. Core source
cannot manufacture authority. Historical pure byte/text construction still
used by identified compiler bootstrap callers receives storage only through
the explicitly supplied runtime budget; its shared backing is released by the
same adapter. This is required bootstrap migration work, not a second allocator
or permanent Seed compatibility promise. Reserved source construction consumes
the application's supplied child budgets and retains its typed refusal rules.
Report those two allocation paths separately during consumer migration.

The existing fresh-domain constructor's version-two request is 192 bytes. The
first 104 bytes preserve their field meanings; context/adapter extent sizes
select their version-two sizes. Its tail contains shared state at 104,
directory pointer/bytes at 112/120, module pointer/bytes at 128/136, input
pointer/bytes at 144/152, runtime/application budget maxima at 160/168 and
reserved zero words at 176/184. Root authority must fund all four exact child
reservations. Requests, metadata, writable arena, directory and immutable spans
obey checked complete-extent and stack-disjointness rules. Initialization and
rollback remain bounded; publish the context version last.

Fixed metadata is 8,120 bytes: 2,112 physical, 2,616 accounting, 1,216 budgeted
and 2,176 shared. Request, context and directory metadata are separately prepaid.
The physical arena remains bounded at 16 MiB. A launcher declares finite root,
runtime and application maxima, instruction count, call depth, stack and scratch
bounds. Those enforced bounds include in-flight aggregate and argument copies
when proving that a u32 share count cannot overflow during valid execution.

The initial ABI-25 profile bounds each complete native frame to 65,536 bytes,
including locals, evaluated values, aggregate backing, scratch and outgoing
arguments. Call depth is at most 64, functions at most 2,048 and data-directory
rows at most 512. At most 262,146 sixteen-byte owning cells, including the two
runtime anchors, can therefore be live across admitted frames. Leaf helper
scratch acquires no additional semantic shares. The launcher supplies a stack
budget covering those complete frames and the separately bounded 8,192-byte
runtime scratch exclusion. These limits are candidate execution-profile limits,
not source-language limits.

The 2,048-function capacity applies to the current-source candidate and its
explicitly reconstructed staging and linker pair. Retained tools keep their
1,024-function bound. Function-record cursors require at most 8,192 bytes;
signature and machine directories each contain at most 155,648 payload bytes.
Each reserved directory's maximum physical charge is 155,664 bytes, so the pair
requires 311,328 bytes while both backings are live. This capacity change does
not widen per-function analysis, frames, call depth, live owning cells or the
4 MiB byte-result publication bound. WVB, WVO and ABI encodings remain unchanged.

## Tool entry and closure

The existing native entry owner supports capability-free
`Main(Budget: Memoryˉbudget) -> i32` and
`Main(Input: borrow bytes, Budget: Memoryˉbudget) -> bytes` under ABI 25.
Source Main receives the supplied application owner exactly once. The native
context and hidden result-pointer convention remain private.

An i32 application closes its complete shared domain on normal or trapped exit.
Ordinary returns and typed failure propagation release each frame's remaining
owners. A terminal trap cannot resume source execution: helper frames propagate
the packed trap to the authenticated entry, which reclaims the complete shared
and budget domain. This fatal path does not promise per-frame user cleanup or
need a second ledger for partially evaluated temporary values. The earlier
trap remains authoritative if terminal teardown also reports failure.
A byte-result tool transfers its result into the caller-owned descriptor while
leaving the domain alive. The caller independently validates the backing
generation, kind, range and admitted maximum, copies at most 4 MiB while that
owner is live, and then releases the result and closes the complete domain.
The old pointer-only WVRQ 1 envelope cannot admit ABI-25 identities. WVRQ 2 is
`48 + 32 * BackingCount` bytes, with at most 66 backing rows. Its header retains
magic/version/total/count at zero through fifteen, contains the exact result
descriptor at sixteen, a nonzero domain epoch at thirty-two and eight reserved
zero bytes at forty. A row contains full handle at zero, opaque mapped pointer
at eight, admitted backing maximum at sixteen, logical length at twenty, immutable kind two at
twenty-four and reserved zero at twenty-eight. Handles are distinct, nonzero,
generation-checked indices 1 through 66; lengths and capacities are at most
4 MiB. A positive maximum requires a nonzero pointer. A zero admitted maximum
may still have a nonzero pointer for a charged frozen empty builder; physical
committed slack does not increase that admitted maximum. The caller constructs
rows from complete shared-domain validation and
inspection while the domain is live. This envelope does not itself prove host
mappings or grant pointer authority.

WVRR 2 retains the 32-byte response header. Rejection has no descriptor payload.
Success has 32 payload bytes: unchanged result descriptor, independently
resolved pointer and unchanged epoch. Admission checks the descriptor's packed
generation/index, reserved high bits, length/maximum, logical range and capacity
range against its exact row, including charged empty values. Pointer addition
must not wrap. The host rechecks descriptor and epoch equality before copying.
An unbacked empty has all-zero identity, length and maximum and resolves to
zero. A backed empty still requires its live immutable row. Reject an unsupported
bridge before executing rather than invoking an old runner with a new descriptor.

Shared teardown owns closure of the lower adapter. Calling lower teardown first
would invalidate live shared bindings and is forbidden. Closure checks reusable
physical storage, zero live shared owners, canonical accounting and credited
parent reservations. Preserve an earlier language trap if cleanup also fails.

## Implementation and evidence boundary

Complete the source, WVB, independently verified native path and one actual
maintained serializer/entry/caller migration before final verification. Reuse
existing focused owners for mapped charges, last-share and non-tail reuse,
charged empties, owning/borrowed ranges, aggregate/call transfers, fixed-live
iteration growth, typed refusal, failure cleanup, exact serializer bytes and
tool-result closure. Qualify the declared path on Windows and real Debian.

Interpreter guest identities and its hosting working state remain separate
implementation obligations. General mutable collections, broader hosted
launchers, installed delivery and full memory qualification remain open. The
candidate ABI and context above do not establish any of those results.
