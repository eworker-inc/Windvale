# Decision 0972: pass native Vector borrows to helpers

## Status

Candidate implementation of the accepted Language 1.0 memory direction.
The [native collection contract](../../Specifications/Windvale-Native-Owned-Collections.md)
owns admission, limits and failure behavior. Completed host checks belong in
the commit summary; installed promotion and interpreter migration remain open.

## Decision

Extend the existing ABI 24/context 10 helper path to immutable and exclusive
mutable scalar Vector parameters using existing WVB shapes 26 and 27. Pass the
generation-checked handle while retaining its allocation lease with the caller.
Reuse the complete WVB verifier's lifetime and alias proof; keep native
admission conservative and bounded. No source, WVB or native ABI version changes.

Permit indexed scalar reads through either borrow. These operations preserve
the backing handle.
Helpers return scalar observations; after the borrow ends, the caller can
release its owner rather than retaining it until terminal domain teardown.
Terminal failures still reclaim the enclosing domain.

Current-source analyzer construction and ordinary project analysis use the
existing hosted profile 8 consistently. The unchanged native lowerer compiles
under profile 7, but this added source exceeds that profile's arena ceiling.
Profile 8 bounds the analyzer arena at 435,945,472 bytes and instruction fuel at
64 times 2^32. This is a bounded bootstrap mitigation; it does not complete the
compiler's memory-management path or change pinned bootstrap products.

## Boundaries and next work

Reject borrowed-helper growth until the calling convention can update the
caller's owning slot. Replacing only the helper's copied handle would leave the
caller with a retired handle and lose the new allocation. Parameter length,
temporary stack-loan drops and other borrowed parameter families remain outside
this slice.
Borrowed parameters cannot become returned owners or be explicitly released.
Append through a borrowed parameter also remains outside the source, complete
WVB verifier, and native execution subset.

Add caller-visible replacement and mutable indexed operations before moving
interpreter working buffers to owned collections. Normal launchers, arbitrary
owner aggregates and shared immutable backing remain separate integration gates.
