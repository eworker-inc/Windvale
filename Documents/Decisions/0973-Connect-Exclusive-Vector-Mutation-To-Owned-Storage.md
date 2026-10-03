# Decision 0973: connect exclusive Vector mutation to owned storage

## Status

Proposed implementation contract under the accepted Language 1.0 memory
direction, with candidate compiler/runtime integration. This decision does not
establish complete ownership-to-storage implementation,
qualification, installed promotion or a released 1.0 product.
The [native collection contract](../../Specifications/Windvale-Native-Owned-Collections.md)
owns exact admission and bounds. Ordinary host checks belong in the commit
summary; the maintained native and source-to-interpreter owners cover this slice.

## Direction

Implement the existing frozen Foundation `Vectorˉreplace` operation and
reserved append through an exclusive Vector helper parameter. Both operate on
the caller's generation-checked backing and preserve its allocation lease.
Replacement returns the previous scalar exactly once; append preserves the
Vector and original item on capacity refusal. Neither operation allocates,
replaces a backing handle or changes the Vector's storage charge.

Keep the source contract and frozen registry bytes unchanged. Introduce
candidate WVIR 1.37/1.38 and WVB 1.43 because earlier bytecode contracts restrict
fallible append to non-parameter locals and have no replacement instruction.
Older bytecode retains its exact admission rules.

## Candidate intermediate and bytecode contracts

WVIR operation `193` names one exact scalar Vector slot in `Target`, its
Vector shape in `Auxiliary`, two temporaries in index/replacement order, and
the exact scalar element result shape. Operation `194` has the existing
fallible append layout but requires an exclusive Vector parameter. Plain
locals retain operation `173`. The new WVIR minor is selected only when one
of the new operations occurs; the even minor carries the existing generic
catalog. Independent validation must reconstruct the exact parameter mode,
element, call-loan and live-owner evidence.

WVB 1.43 inherits candidate 1.42's metadata and ownership vocabulary and adds
the nine-byte instruction `E4 vector.replace`: little-endian `u32` owner-slot
index followed by the exact `u32` Vector type index. It consumes a `u64` index
then one exact scalar replacement and produces the previous scalar. The named
slot must be a live unique Vector or an exclusive Vector parameter, never an
immutable parameter or borrowed payload projection. Both operands, scalar
shape, version and stack are checked before execution. Indexes require
`index < length` and trap before reading or writing on violation.

Only minor 43 permits `D0` through a shape-27 exclusive parameter. The existing
parameter-length instruction `E2` observes the retained handle without moving
or retaining an owner. Lifetime and alias exclusions still come from complete
WVB verification; native admission remains an additional bounded restriction.
The distinguishing minor-43 feature is `E4` or borrowed-parameter `D0`.
Canonical emission examines emitted reachable functions; an unused mutation
helper alone does not raise the required WVB version.

## Native connection and remaining gates

Reuse ABI 24/context 10 and its existing physical storage/budgeted adapter.
The generated private access helper gains scalar replacement; its embedded
template and relative offsets must be rebuilt and checked together. No public
provider ABI, context layout, source pointer or lease constructor changes.
The first target admits `i32`, `bool`, `u8`, `u32`, `i64` and `u64` elements.

Extend the existing compiler, complete verifier, interpreter and native owner.
Require source-level helper mutation, caller-visible observations, reservation
refusal, bounds/failure cleanup, alias rejection and fixed-live-state repeated
release on Windows and real Debian. Preserve unaffected earlier evidence and
pinned bootstrap identities. Candidate behavior must fail closed where an old
bootstrap tool cannot consume the new bytecode; current-source tools are built
and selected explicitly before wider delivery is claimed.

Mutable element views, borrowed replacement growth, arbitrary owner aggregates,
shared immutable backing, normal launcher context construction and interpreter
working-storage migration remain separate required integration work. A passing
scalar mutation slice does not close those gates.

Direct indexed read-through retains conservative source borrowing until the
function returns. The scalar workload ends that loan in a read helper before
the caller releases its Vector. This is not general last-use borrow analysis.
Selected Debian execution uses exact Windows-built WVB products with Linux
containers; independent Debian compiler reconstruction remains open.
