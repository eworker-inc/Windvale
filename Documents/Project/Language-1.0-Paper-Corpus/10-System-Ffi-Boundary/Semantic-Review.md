# Workload 10 semantic review

## Profile, target, and authority

Only the adapter and application are System. Their platform key selects one
complete environment/architecture/ABI predicate. The types, decoder, and report
remain Core and contain no conditional layout. A nonmatching target rejects the
System modules before artifact publication; dead-code elimination cannot make
an unsupported ABI acceptable.

System authority admits the declared machine boundary but creates no capability.
The one paper symbol owns no external authority. A file/network/device foreign
binding would still require the exact corresponding grant.

## Unsafe visibility

The foreign declaration begins with `unsafe foreign`. Invocation occurs inside
one `unsafe` value block. Constructing scratch, splitting budgets, translating
status, borrowing ordinary initialized bytes after the exclusive region ends,
decoding fields, copying payload, and formatting output remain ordinary typed
code.

The unsafe value block returns only `i64`. It cannot return the pointer, region,
scratch, raw address, layout witness, or a borrow. The public result contains
only Core types.

## Address, range, alignment, lifetime, and aliasing

Scratch construction uses the exact ABI/address witness and an 8-byte power-of-
two alignment. Region construction checks relative `Start + Length`, native
base plus start/end, 64-bit representability, owner bounds, actual alignment,
live allocation generation, and exclusive borrow state before returning.

The region exclusively borrows scratch. No safe slice or second write region can
coexist. The derived foreign pointer is non-null, borrow-tied, exact-ABI, and
no-retain. It becomes unusable when the call/region ends. The later safe slice
therefore cannot alias an executing foreign writer.

No pointer-sized source integer exists. Source cannot compare pointer addresses,
add integers to them, serialize them, store them in safe data, or forge null.
Nullable ABI parameters use a distinct opaque type and named validation.

## Foreign outcome translation

The exact i64 status is interpreted once. `-1`, `-2`, and `-3` are named.
Other negatives are invalid. Nonnegative i64 converts through a named exact
operation and must not exceed capacity. Stale generation reads only the exact
eight-byte prefix authorized by that status and retains expected/observed values.

There is no retry. Foreign rejection/failure/invalid completion discards local
scratch and publishes no record. This interface has no indeterminate external
mutation because its only semantic product is caller-owned memory and exact
status; a future external mutation needs the ordinary completion/uncertainty
contract.

## Data validation and safe publication

The foreign pointer contract makes in-range memory access safe only if the
callee obeys it. It does not make returned bytes trustworthy. Core validation
checks exact length, magic/version, enum, Boolean, reserved byte, generation,
checked payload geometry, and no trailing bytes.

The payload is copied into a new bounded bytes builder. The scratch owner is not
retained. The report consumes only the safe record and has no System import.
This is the boundary proof: unsafe memory becomes safe data only after complete
validation and independent ownership.

## Unwind and terminal containment

The registered ABI forbids unwind. Recoverable failures are return values.
Out-of-range writes, retention, use-after-return, ABI corruption, and forbidden
unwind can invalidate process integrity before adapter code resumes. They are
terminal containment events, not catchable exceptions or `Boundaryˉfailure`.

An implementation tests these cases in an isolated process with exact expected
termination. It must not execute a deliberately corrupting shim inside the
verification coordinator or reinterpret survival as safety.

## Acceptance matrix

| Pressure | Evidence | Standing |
| --- | --- | :---: |
| System/exact platform/ABI | one concrete target key + registered ABI identity | Pass with accepted registry entry |
| unsafe declaration/invocation | one declaration + one value block | Pass |
| opaque foreign pointer | one non-null borrow-tied `Foreignˉpointer<u8,Abi>` | Pass |
| alignment/range/lifetime/alias | aligned scratch + checked exclusive write region | Pass with accepted Foundation surface |
| checked address arithmetic | relative and native base/end checks before region publication | Pass |
| foreign failure/unwind | typed status translation + terminal no-unwind containment | Pass |
| no capability escalation | no capability; System/unsafe adds none | Pass |
| safe publication | Core decoder + copied bytes + Core-only report | Pass |
