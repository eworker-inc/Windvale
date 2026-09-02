# Windvale Language 1.0 Foundation specification

## Status and scope

This is the normative-candidate Foundation companion to the
[Language 1.0 semantic specification](Windvale-Language-1.0.md), authorized by
[Decision 0751](../Documents/Decisions/0751-Accept-Windvale-Language-1.0-Direction.md)
and refined by
[Decision 0754](../Documents/Decisions/0754-Resolve-First-Language-1.0-Paper-Findings.md)
and
[Decision 0755](../Documents/Decisions/0755-Resolve-Language-1.0-Command-Workload-Findings.md)
and
[Decision 0756](../Documents/Decisions/0756-Resolve-Language-1.0-File-Copy-Findings.md)
and
[Decision 0757](../Documents/Decisions/0757-Resolve-Language-1.0-Database-Transaction-Findings.md)
and
[Decision 0758](../Documents/Decisions/0758-Resolve-Language-1.0-Compiler-Front-End-Findings.md)
and
[Decision 0759](../Documents/Decisions/0759-Resolve-Language-1.0-Http-Handler-Findings.md)
and
[Decision 0760](../Documents/Decisions/0760-Resolve-Language-1.0-Concurrent-Service-Findings.md)
and
[Decision 0761](../Documents/Decisions/0761-Resolve-Language-1.0-Retained-Gui-Findings.md)
and
[Decision 0762](../Documents/Decisions/0762-Resolve-Language-1.0-Numeric-Graphics-Findings.md)
and
[Decision 0763](../Documents/Decisions/0763-Resolve-Language-1.0-Package-Parser-Findings.md)
and
[Decision 0764](../Documents/Decisions/0764-Resolve-Language-1.0-System-Ffi-Findings.md),
with complete-suite reconciliation accepted by
[Decision 0765](../Documents/Decisions/0765-Complete-Language-1.0-Source-Freeze-Candidate.md).
Localized-source reconciliation is accepted by
[Decision 0766](../Documents/Decisions/0766-Complete-Language-1.0-Localized-Source-Reconciliation.md).
It specifies the standard nominal values and protocols required for one coherent
Language 1.0 surface. It is not the currently implemented Foundation library.

The implemented Seed Foundation contracts remain separately owned by
[Foundation bytes](Foundation-Bytes.md),
[byte construction](Foundation-Byte-Construction.md),
[byte ordering](Foundation-Byte-Ordering.md), and related current specifications
until the migration plan advances them.

This document owns semantic behavior. The exact source grammar is owned by the
[Language 1.0 grammar](Windvale-Language-1.0-Grammar.md). The
[Foundation signature registry](Windvale-Language-1.0-Foundation-Registry.md)
owns the complete edition-1 public declaration spelling and reproducible
candidate signature-set identity for each required module. Those hashes remain
candidate identities until the explicit source-freeze decision accepts them.

The owner-accepted replacement-candidate
[localized-source and source-vocabulary specification](Windvale-Language-1.0-Localized-Source.md)
may bind separately shipped primary source labels, display labels, and
documentation to those exact canonical module/signature identities. Such a
catalog does not add a Foundation declaration, alternate implementation,
overload, intrinsic identity, ABI alias, or runtime dependency. A stale catalog
cannot bind after its target signature-set identity changes.

## Foundation principles

Foundation 1:

- is explicitly imported and does not create an ambient prelude;
- is ordinary typed Windvale source wherever an intrinsic is unnecessary;
- may use compiler-recognized lowering without changing public semantics;
- has no hidden capability acquisition, host path, host locale, host encoding,
  host scheduler, or catchable exception;
- bounds allocation, iteration, formatting, diagnostics, and retained state;
- distinguishes rejection, accepted partial progress, completion, and
  indeterminate external completion;
- preserves exact ownership on every failure path; and
- keeps serialization in separately versioned format modules.

The compiler may recognize `Option<T>`, `Result<T, E>`, `Localˉrelease<Self>`,
the `Task.Spawn` closure-shape relation, and task suspension because language
typing or syntax depends on their exact identities. Recognition is by canonical
module, declaration, type, major version, and signature-set identity, never by
an unqualified source name. `Task.Spawn` recognition decomposes the exact
explicit `Work` argument type; it does not search an overload or use result
context.

## Required modules

The candidate required modules are:

| Module | Minimum profile | Contract |
| --- | --- | --- |
| `Foundationˉoption` | Core | Optional presence. |
| `Foundationˉresult` | Core | Recoverable typed success/failure. |
| `Foundationˉnumeric` | Core | Explicit conversions, parsing, and strict float helpers. |
| `Foundationˉordering` | Core | Equality and deterministic total-order protocols. |
| `Foundationˉmemory` | Core | Allocation domains, leases, limits, and failures. |
| `Foundationˉcollections` | Core | Arrays, vectors, sequences, slices, maps, sets, iterators, and arenas. |
| `Foundationˉbytes` | Core | Immutable bytes, codecs, and bounded byte construction. |
| `Foundationˉtext` | Core | Unicode text, rune iteration, formatting, and bounded text construction. |
| `Foundationˉresource` | Core | Local-release protocol and owned-resource outcomes. |
| `Foundationˉoperation` | Hosted | Shared deadline and cancellation observation context. |
| `Foundationˉtask` | Hosted | Task scopes, task handles, cancellation, and join outcomes. |
| `Foundationˉunsafe` | System | Raw address and foreign boundary primitives. |

A compiler claiming a complete profile supplies or binds every module required
by that profile. Hosted includes the Core rows; System includes the Core and
Hosted rows it uses plus the System row. A package may select a compatible newer Foundation implementation
only when its major contract and exact required signature set remain compatible.

## Standard optional presence

`Foundationˉoption.Option<T>` is:

~~~text
export variant Option<T> {
    Present(Value: T);
    Absent;
}
~~~

`Option<T>` has the strictest value class required by `T`:

- Copy when `T` is Copy;
- shared immutable when `T` is shared immutable;
- owned when `T` is owned; and
- never valid when storing `T` would allow a borrow to outlive its owner.

`Present` owns or copies its payload according to `T`. `Absent` has no hidden
sentinel representation. Matching is exhaustive. There is no implicit conversion
between `Option<T>` and `T`, `bool`, a pointer, an integer, or a result.

The registry fixes `Isˉpresent`, immutable and exclusive `Borrow`, consuming
`Take`, and pure consuming `Map`. `Take` leaves `Absent`. `Map` accepts one exact
effect-free `fn(T) -> U`, evaluates it once only for `Present`, and preserves
`Absent` without calling it. An effectful transform is written as an explicit
`match`; edition 1 does not add effect-polymorphic convenience calls.

No operation traps merely because the option is absent. An explicit
`Requireˉpresent` contract may trap only when its source precondition proves
presence and names that precondition in diagnostics.

## Standard recoverable result

`Foundationˉresult.Result<T, E>` is:

~~~text
export variant Result<T, E> {
    Valid(Value: T);
    Failure(Error: E);
}
~~~

`Result<T, E>` adopts the strictest value class of `T` and `E`. It has no hidden
exception, stack trace, allocation, or ambient error conversion.

The language `try` expression recognizes this exact identity and:

- extracts `Valid.Value`;
- propagates the original `Failure` unchanged;
- requires the containing function to return `Result<U, E>` with the same exact
  `E`;
- releases ordinary owned locals before returning; and
- never invokes a protocol or adapter implicitly.

The registry fixes exact case tests, immutable and exclusive payload borrows,
and pure consuming `Mapˉvalid` and `Mapˉfailure`. Mapping the valid side accepts
one exact effect-free `fn(T) -> U`; mapping the failure side accepts one exact
effect-free `fn(E) -> F`. The selected side is evaluated once and the other
owned payload passes through unchanged. Effectful transforms use explicit
`match`; no operation is selected through overload or result inference.

`Result<unit, E>` is the standard recoverable no-data completion. A function that
cannot fail returns `unit`, not `Result<unit, never>`.

## Common bounded failures

### Limit failure

A source or value maximum is different from current memory availability:

~~~text
export variant Limitˉfailure {
    Maximumˉexceeded(
        Requested: u64,
        Maximum: u64,
    );
    Arithmeticˉoverflow;
}
~~~

`Maximumˉexceeded` is reported before allocating or mutating. Requested and
maximum use the width selected by the API; values above `u64` are rejected as
arithmetic overflow before constructing this common form.

### Allocation failure

~~~text
export enum Allocationˉreason: u8 {
    Budgetˉexhausted = 1u8;
    Targetˉunaddressable = 2u8;
    Providerˉunavailable = 3u8;
    Fragmented = 4u8;
}

export record Allocationˉfailure {
    Reason: Allocationˉreason;
    Requestedˉbytes: u64;
    Availableˉbytes: u64;
}
~~~

The values reveal the admitted accounting result, not a native allocator address
or host error code. `Availableˉbytes` is zero when the provider cannot expose a
stable amount. Allocation operations should borrow configuration inputs. When an
operation accepts an owned input, its failure payload must return that input
explicitly if the caller is to recover it; there is no implicit ownership
rollback.

### Capacity failure

~~~text
export variant Capacityˉfailure<T> {
    Rejected(
        Requestedˉitems: u64,
        Remainingˉitems: u64,
        Value: T,
    );
    Acceptedˉprefix(
        Acceptedˉitems: u64,
        Remaining: T,
    );
}
~~~

An operation chooses one exact family:

- all-or-nothing operations return `Rejected` with the original owned input and
  leave the destination unchanged; or
- prefix-admitting operations return the exact accepted count and the unaccepted
  owned remainder.

One operation never changes between these families because of target, capacity,
or provider behavior.

## Memory domains and allocation

### Memory budget

`Memoryˉbudget` is an owned accounting domain with exact maximum retained bytes,
current committed bytes, current reserved bytes, child count, and provider
generation. A root budget is supplied by a launcher, runtime, or parent domain;
Core source cannot manufacture one from ambient host memory.

A budget can create a rights-reduced child budget:

~~~text
fn Split(
    Parent: borrow mut Memoryˉbudget,
    Maximumˉbytes: u64,
    Maximumˉchildren: u32,
) -> Result<Memoryˉbudget, Allocationˉfailure>
    effects(memory.allocate);
~~~

The parent reserves the child's maximum accounting authority until the child is
released. Splitting never grants access outside the parent and cannot increase
the combined maxima. It is deterministic Core accounting under the explicit
parent budget, not provider acquisition; therefore it carries
`memory.allocate` but not `resource.acquire`.

### Allocation lease

`Allocationˉlease` is a move-only child of one budget. It states maximum retained
bytes, current retained bytes, alignment ceiling, and generation. Collections
and arenas own leases rather than reaching an ambient allocator.

Creating a lease can fail recoverably. A lease maximum is an accounting bound,
not a promise that every later physical growth succeeds. An implementation may
offer a separately named `Reserveˉcommitted` operation that guarantees later
growth within the committed amount; its initial result must prove the reserved
capacity.

An edition-1 budget's maximum authority never expands implicitly. A hosted
provider may later expose a separately named, capability-bound operation for
requesting additional authority, with explicit provider-unavailable,
revocation, and retry behavior. Collection growth in Core consumes only the
authority already present in the supplied budget and never reaches ambient OS
memory.

Releasing a collection releases its lease and credits retained accounting
locally. Provider loss cannot make local handle invalidation fallible.

## Equality and ordering protocols

### Equality

~~~text
export protocol Equality<T> {
    fn Equal(
        Left: borrow T,
        Right: borrow T,
    ) -> bool effects();
}
~~~

Equality is reflexive, symmetric, and transitive for admitted values. Floating
IEEE equality is deliberately not reflexive for NaN and therefore does not
implement this protocol directly. A named canonical or bitwise floating wrapper
may do so.

### Ordering

~~~text
export enum Orderingˉresult: i8 {
    Less = -1i8;
    Equal = 0i8;
    Greater = 1i8;
}

export protocol Ordering<T> {
    fn Compare(
        Left: borrow T,
        Right: borrow T,
    ) -> Orderingˉresult effects();
}
~~~

Ordering is a deterministic total order compatible with Equality. It cannot use
locale, pointer identity, process-random seeds, provider state, or host object
layout. A protocol implementation declares a finite comparison-work bound in
terms of the admitted value maxima.

### Copy and clone

Copy is a language value class, not a user-overloadable operation. A nominal
aggregate may derive Copy only when every field is Copy and the compiler proves a
finite representation bound.

Owned values use explicitly named `Clone` operations. Clone:

- takes an immutable borrow of the source;
- takes an allocation lease or destination budget;
- states a maximum work and output bound;
- returns a typed allocation or limit failure; and
- never silently converts an owned type into Copy.

## Numeric conversions

Numeric conversion names encode the policy. Foundation supplies a generated,
reviewable matrix rather than overloaded `cast`.

### Integer widening

`WidenˉSˉtoˉD(Value: S) -> D` is available only when every value of source type
`S` is representable in destination `D`. It is total and preserves mathematical
value.

Examples include `Widenˉu8ˉtoˉu16` and `Widenˉi32ˉtoˉi64`. Unsigned to signed is
widening only when the destination has strictly more value bits.

### Checked integer conversion

`ConvertˉSˉtoˉD(Value: S) -> Result<D, Numericˉconversionˉfailure>` admits every
integer pair not covered by identity or widening:

~~~text
export variant Numericˉconversionˉfailure {
    Belowˉminimum;
    Aboveˉmaximum;
}
~~~

It returns one failure without truncating or mutating input.

### Explicit lossy integer conversion

Edition 1 does not require lossy integer conversion. A later Foundation
signature set may add these separate names when a complete workload requires
them:

- `WrapˉSˉtoˉD` uses modulo 2 to the destination width;
- `SaturateˉSˉtoˉD` clamps to the destination mathematical range; and
- `TruncateˉSˉtoˉD` preserves the low destination-width bits.

Signed interpretation is defined through exact two's-complement bits. These
operations are never used implicitly by assignment, calls, literals, operators,
serialization, or protocol selection.

### Floating conversions

Integer-to-float conversion names include their rounding mode when not exact.
`ConvertˉSˉtoˉf32ˉnearest` and `ConvertˉSˉtoˉf64ˉnearest` use
roundTiesToEven. An `Exact` form returns failure when the integer cannot be
represented exactly.

Float-to-integer operations name truncation or rounding and return failure for
NaN, infinity, or an out-of-range result:

~~~text
export variant Floatingˉconversionˉfailure {
    Notˉaˉnumber;
    Infinite;
    Belowˉminimum;
    Aboveˉmaximum;
    Inexact;
}
~~~

`Widenˉf32ˉtoˉf64` is total after canonicalizing NaN. Narrowing to `f32` names
the rounding mode and returns either the rounded value or an exactness failure,
depending on the selected operation.

### Bit reinterpretation and byte encoding

`Bitsˉf32ˉtoˉu32`, `Bitsˉu32ˉtoˉf32`, `Bitsˉf64ˉtoˉu64`, and
`Bitsˉu64ˉtoˉf64` preserve exact bits, including NaN sign and payload. A later
arithmetic operation canonicalizes a NaN result under the language floating
profile; reversing the bit reinterpretation before arithmetic reproduces the
original bits. Equal-width integer signed/unsigned bit operations use separately
named functions.

Bit reinterpretation never emits bytes. Byte codecs name `little` or `big` byte
order and exact width.

### First accepted exact numeric signatures

The first reviewed paper workload fixed these version-1 names, parameter names,
types, results, and empty effect sets:

~~~text
export fn Widenˉu8ˉtoˉu16(Value: u8) -> u16 effects();
export fn Widenˉu8ˉtoˉu32(Value: u8) -> u32 effects();
export fn Widenˉu8ˉtoˉu64(Value: u8) -> u64 effects();
export fn Widenˉu16ˉtoˉu32(Value: u16) -> u32 effects();
export fn Widenˉu32ˉtoˉu64(Value: u32) -> u64 effects();
export fn Bitsˉu32ˉtoˉf32(Value: u32) -> f32 effects();
~~~

The widening calls preserve mathematical value. `Bitsˉu32ˉtoˉf32` preserves
all 32 input bits and performs no arithmetic. The signature registry now closes
the complete integer, bit-reinterpretation, and integer/floating conversion
families deterministically; these calls remain the workload-proven subset.

### Strict floating operation and conversion surface

The numeric/graphics workload fixes these additional version-1 names:

~~~text
export enum Floatingˉclass: u8 {
    Negativeˉinfinity = 1u8;
    Negativeˉnormal = 2u8;
    Negativeˉsubnormal = 3u8;
    Negativeˉzero = 4u8;
    Positiveˉzero = 5u8;
    Positiveˉsubnormal = 6u8;
    Positiveˉnormal = 7u8;
    Positiveˉinfinity = 8u8;
    Notˉaˉnumber = 9u8;
}

export fn Bitsˉf32ˉtoˉu32(Value: f32) -> u32 effects();
export fn Bitsˉf64ˉtoˉu64(Value: f64) -> u64 effects();
export fn Bitsˉu64ˉtoˉf64(Value: u64) -> f64 effects();
export fn Classifyˉf32(Value: f32) -> Floatingˉclass effects();
export fn Bitwiseˉequalˉf32(Left: f32, Right: f32) -> bool effects();
export fn Totalˉcompareˉf32(
    Left: f32,
    Right: f32,
) -> Orderingˉresult effects();
export fn Fusedˉmultiplyˉaddˉf32(
    Left: f32,
    Right: f32,
    Addend: f32,
) -> f32 effects();
export fn Convertˉu32ˉtoˉf32ˉnearest(Value: u32) -> f32 effects();
export fn Convertˉu32ˉtoˉf32ˉexact(
    Value: u32,
) -> Result<f32, Floatingˉconversionˉfailure> effects();
export fn Convertˉf32ˉtoˉi32ˉtruncate(
    Value: f32,
) -> Result<i32, Floatingˉconversionˉfailure> effects();
export fn Widenˉf32ˉtoˉf64(Value: f32) -> f64 effects();
export fn Narrowˉf64ˉtoˉf32ˉnearest(Value: f64) -> f32 effects();
export fn Narrowˉf64ˉtoˉf32ˉexact(
    Value: f64,
) -> Result<f32, Floatingˉconversionˉfailure> effects();
~~~

The bit calls preserve every bit without arithmetic. `Classifyˉf32` observes
the input bits and returns the exact sign/category; all NaN encodings use the one
`Notˉaˉnumber` case. `Bitwiseˉequalˉf32` compares all 32 bits.
`Totalˉcompareˉf32` implements IEEE 754 `totalOrder`, including `-0 < +0` and
the specified ordering of NaN signs, signaling/quiet state, and payloads; it is
a named observation and does not make `f32` implement `Ordering<f32>`.

`Fusedˉmultiplyˉaddˉf32` computes the infinitely precise product and sum with
one final roundTiesToEven operation. It preserves subnormals, follows IEEE
signed-zero/infinity rules, and canonicalizes every NaN result to `0x7fc00000`.
Ordinary `Left * Right + Addend` remains two separately rounded operations and
must not contract to this call.

The nearest conversions use roundTiesToEven. The exact forms return `Inexact`
when the mathematical source is in range but not exactly representable.
Float-to-integer conversion reports NaN, infinity, and range before inexactness;
truncate means toward zero and never wraps. Widening preserves every finite
value and signed zero and canonicalizes NaN. Nearest narrowing may produce
infinity on finite overflow and preserves signed zero; exact narrowing reports
`Inexact` for finite overflow, underflow, or any other rounded result. These
accepted calls are the exact workload subset. The registry's finite generated
families apply the same naming, ordering, and failure rules to every required
pair and add the corresponding f64 classify, bitwise-equality, total-order, and
fused-operation observations.

### Parsing

Numeric parsing receives:

- exact destination type;
- radix or admitted prefix policy;
- sign policy;
- separator policy;
- whitespace policy;
- whole-input or prefix mode; and
- maximum input bytes.

It returns:

~~~text
export variant Numericˉparseˉfailure {
    Empty;
    Invalidˉdigit(Offset: u64);
    Invalidˉsign(Offset: u64);
    Invalidˉseparator(Offset: u64);
    Trailingˉinput(Offset: u64);
    Belowˉminimum;
    Aboveˉmaximum;
    Limitˉexceeded;
}
~~~

Floating parsing additionally reports invalid exponent and unsupported special
value. Parsing is locale-independent and never accepts a host-specific spelling.

The command workload fixes the first exact type- and policy-specific parser:

~~~text
export fn Parseˉu64ˉdecimalˉwhole(
    Value: borrow text,
    Maximumˉinputˉbytes: u64,
) -> Result<u64, Numericˉparseˉfailure> effects();
~~~

It admits one or more ASCII decimal digits only, checks the canonical UTF-8 byte
maximum before digit work, and consumes the whole input. It accepts no sign,
prefix, separator, whitespace, locale digit, special value, or trailing input.
It reports `Limitˉexceeded`, `Empty`, the exact first `Invalidˉdigit` byte
offset, or `Aboveˉmaximum` as applicable and never wraps or truncates. The name
fixes destination, radix, and policy without result-context inference.

## Fixed arrays

`Array<T, N>` contains exactly compile-time constant `N` elements. `N` uses an
exact unsigned type selected by the Foundation signature and is bounded by
compiler and target representation limits.

An array:

- stores elements in index order;
- reports length `N` without allocation;
- checks every index before access;
- adopts the strictest value class of `T`;
- iterates from zero through `N - 1`; and
- has no hidden capacity or growth.

Array construction names or supplies every element exactly once. Repetition
syntax, if later admitted, may repeat only a Copy value and evaluates that value
once.

The numeric/graphics workload fixes array literals as the first construction
form. A literal has an exact expected `Array<T, N>` type, contains exactly `N`
elements of exact type `T`, evaluates them left to right once, and allocates no
dynamic backing. No element conversion, inferred common type, omitted element,
or repetition form is implied.

## Owned vectors and immutable sequences

### Vector

`Vector<T>` is a move-owned contiguous collection. Construction receives:

- maximum items;
- maximum retained bytes;
- one owned allocation lease; and
- optional initial capacity not exceeding either maximum.

The vector records length, current capacity, maximum items, maximum retained
bytes, and lease generation. It never grows past either maximum.

The edition-1 registry fixes this deliberately small operation family:

- immutable and exclusive mutable indexed borrow;
- all-or-nothing append of one item;
- explicit all-or-nothing replacement growth under a separately borrowed
  budget;
- remove or replace with exact ownership return;
- immutable slice creation;
- exclusive mutable slice creation; and
- consuming freeze.

Construction reserves the complete initially admitted item maximum. Append
never grows implicitly. The separately named growth operation replaces the
complete backing under an explicit budget; edition 1 still has no
prefix-admitting vector append. A later bulk API may be added under a different
signature-set identity after its exact ownership and partial-progress contract
is selected.

An all-or-nothing rejected append returns the original owned item and leaves
length, contents, capacity, and iteration unchanged. A successful append accepts
ownership exactly once.

The compiler-front-end workload fixes the first reserved empty-vector surface:

~~~text
export record Vectorˉappendˉfailure<T> {
    Error: Collectionˉfailure;
    Value: T;
}

export fn Vectorˉconstructˉreserved<T>(
    Budget: Memoryˉbudget,
    Maximumˉitems: u64,
) -> Result<Vector<T>, Allocationˉfailure>
    effects(memory.allocate);

export fn Vectorˉappend<T>(
    Vector: borrow mut Vector<T>,
    Value: T,
) -> Result<unit, Vectorˉappendˉfailure<T>> effects();

export fn Vectorˉgrowˉreserved<T>(
    Vector: borrow mut Vector<T>,
    Budget: borrow mut Memoryˉbudget,
    Newˉmaximumˉitems: u64,
) -> Result<unit, Allocationˉfailure>
    effects(memory.allocate);

export fn Vectorˉlength<T>(
    Vector: borrow Vector<T>,
) -> u64 effects();

export fn Vectorˉborrowˉat<T>(
    Vector: borrow Vector<T>,
    Index: u64,
) -> borrow T effects();

export fn Vectorˉborrowˉatˉmut<T>(
    Vector: borrow mut Vector<T>,
    Index: u64,
) -> borrow mut T effects();

export fn Vectorˉreplace<T>(
    Vector: borrow mut Vector<T>,
    Index: u64,
    Replacement: T,
) -> T effects();

export fn Vectorˉremove<T>(
    Vector: borrow mut Vector<T>,
    Index: u64,
) -> T effects();

export fn Vectorˉfreeze<T>(
    Vector: Vector<T>,
) -> Sequence<T> effects();
~~~

Construction requires a positive maximum, consumes one budget, and reserves the
complete representation/capacity for that item maximum. The empty generic call
uses edition 1's explicit `::<T>` syntax. Later append cannot fail for physical
growth; capacity rejection returns the unchanged original value and vector.
Zero is a violated constructor precondition and traps before allocation.
If a positive maximum cannot be represented by the selected target, construction
returns `Allocationˉfailure` with reason `Targetˉunaddressable`; it does not
silently narrow the maximum. Requested-byte evidence is saturated at `u64`
rather than wrapped. A target's smaller executable allocation profile is not a
portable Language maximum.

Growth requires `Newˉmaximumˉitems` to be greater than the Vector's current
maximum; violation traps before allocation. It reserves the complete
replacement representation against the supplied budget while the old backing
remains live, then copies the initialized prefix and commits one atomic backing
and lease swap. Success preserves length and element order. Any target, budget,
provider, fragmentation, or allocation refusal before that swap returns exact
`Allocationˉfailure` and leaves the Vector's owner, length, contents,
capacity, maximum, and the supplied budget's accounting/generation unchanged.
The temporary peak can therefore include both old and replacement allocations.
This explicit strong-transaction cost is preferable to hidden partial growth;
an implementation must not silently fall back to in-place prefix progress.

Length reports accepted items. Both borrow calls, replacement, and removal
require `Index < Vectorˉlength` and trap before access on a violated proved
precondition. Replacement accepts the new value once and returns the prior
owned value. Removal returns the owned element and shifts later elements left
without changing their relative order. Freeze consumes the vector, publishes
exactly its items in order, transfers retained accounting, and performs no
fallible compaction.

### Sequence

`Sequence<T>` is shared immutable publication produced by consuming a vector or
builder. It records current length and admitted maximum. Copying a sequence may
share backing; backing identity, capacity slack, and reference count are
unobservable.

Freeze transfers the owner's allocation accounting into the immutable backing.
Copying a sequence does not multiply that charge. The accounting remains retained
until the final semantic share is released, regardless of whether the
implementation uses reference counts, an arena, copy-on-write storage, or
another unobservable bounded strategy.

Freeze:

- consumes the mutable owner;
- publishes exactly its current elements in order;
- invalidates every mutable borrow before publication;
- may compact storage only if source cannot observe it;
- reports any fallible compaction before consuming ownership, or returns the
  original owner with the failure; and
- cannot leave both mutable and immutable aliases to the same storage.

The command workload fixes these version-1 immutable observations:

~~~text
export fn Sequenceˉlength<T>(
    Value: borrow Sequence<T>,
) -> u64 effects();

export fn Sequenceˉat<T>(
    Value: borrow Sequence<T>,
    Index: u64,
) -> borrow T effects();
~~~

`T` is solved uniquely from the explicit `Value` argument. `Sequenceˉlength`
returns current element count. `Sequenceˉat` checks
`Index < Sequenceˉlength(Value)` before access and traps terminally on
violation. Its result is tied to the one borrowed sequence owner and cannot
escape it. No unchecked Core or Hosted counterpart is implied.

## Slices

`Slice<T>` is an immutable borrow of one contiguous range.
`Mutableˉslice<T>` is an exclusive mutable borrow. A slice records exact length;
its start and byte geometry are not serializable source identity.

Creating a slice validates start plus length with checked arithmetic before
creating any view. An empty slice may point only to its owner's admitted
one-past-end boundary and never permits element access.

Slices cannot outlive, resize, move, freeze, release, or close their owner.
Mutable slices cannot overlap any live read or write access. Splitting a mutable
slice is permitted only by an operation that proves disjoint ranges.

The HTTP-handler workload fixes these first exact immutable-slice observations:

~~~text
export fn Sliceˉlength<T>(Value: Slice<T>) -> u64 effects();

export fn Sliceˉat<T>(
    Value: Slice<T>,
    Index: u64,
) -> borrow T effects();
~~~

`Sliceˉlength` returns exact elements. `Sliceˉat` checks
`Index < Sliceˉlength(Value)` before access and traps terminally on a violated
proved precondition. Its result inherits the ephemeral slice's one underlying
owner and cannot escape it. Decision 0758's Copy/shared read-through applies to
the result; it does not expose an address or add unchecked indexing.

The numeric/graphics workload fixes checked range creation and exclusive
replacement:

~~~text
export variant Sliceˉfailure {
    Rangeˉoverflow(Start: u64, Length: u64);
    Outˉofˉrange(Start: u64, Length: u64, Ownerˉlength: u64);
}

export fn Arrayˉslice<T, const N: u64>(
    Value: borrow Array<T, N>,
    Start: u64,
    Length: u64,
) -> Result<Slice<T>, Sliceˉfailure> effects();

export fn Vectorˉslice<T>(
    Value: borrow Vector<T>,
    Start: u64,
    Length: u64,
) -> Result<Slice<T>, Sliceˉfailure> effects();

export fn Vectorˉsliceˉmut<T>(
    Value: borrow mut Vector<T>,
    Start: u64,
    Length: u64,
) -> Result<Mutableˉslice<T>, Sliceˉfailure> effects();

export fn Mutableˉsliceˉlength<T>(
    Value: Mutableˉslice<T>,
) -> u64 effects();

export fn Mutableˉsliceˉreplace<T>(
    Value: Mutableˉslice<T>,
    Index: u64,
    Replacement: T,
) -> T effects();
~~~

Every range call checks `Start + Length` for overflow and against the exact
owner length before publishing a view. Failure publishes no borrow. Mutable
slice creation exclusively borrows the vector for the view's lifetime, so the
vector cannot resize, freeze, move, or be observed concurrently. Replacement
checks `Index < Mutableˉsliceˉlength` before mutation, returns the previous owned
element, and accepts the replacement exactly once. The check is a proved
precondition trap, matching `Sliceˉat`; there is no partial mutation or
unchecked Core counterpart.

## Deterministic maps

`Map<K, V>` is a move-owned finite associative collection. Language 1.0's
standard map requires `Ordering<K>` and uses that total order as its canonical
iteration and publication order.

The semantic contract is an ordered map:

- keys are unique under `Ordering<K>.Compare == Equal`;
- lookup and mutation have bounded worst-case work proportional to a published
  logarithmic function of maximum items;
- iteration is ascending canonical key order;
- insertion either rejects unchanged with the original owned key/value or
  completes exactly once;
- replacement returns the previous owned value;
- removal returns the owned key and value;
- maximum items, retained bytes, comparison work, and diagnostic work are
  finite; and
- implementation layout, balancing, and node addresses are unobservable.

A `Hashˉmap`, insertion-ordered map, or process-randomized map is a different
future type. Serialization uses a named format and cannot assume internal nodes.

Consuming publication produces `Immutableˉmap<K, V>` with the same canonical
iteration order and no mutation.

The database-transaction and compiler-front-end workloads fix this first exact
ordered-map construction and observation surface:

~~~text
export variant Collectionˉfailure {
    Invalidˉlimit(Field: u32, Observed: u64, Minimum: u64, Maximum: u64);
    Allocation(Error: Allocationˉfailure);
    Capacityˉexhausted(Maximum: u64);
    Duplicate;
    Comparisonˉlimit(Maximum: u64);
    Wrongˉarena;
    Slotˉoutˉofˉrange(Slot: u64, Maximum: u64);
    Vacant(Slot: u64);
    Staleˉgeneration(Expected: u64, Observed: u64);
    Retired(Slot: u64);
}

export record Mapˉinsertˉfailure<K, V> {
    Error: Collectionˉfailure;
    Key: K;
    Value: V;
}

export record Mapˉentry<K, V> {
    Key: K;
    Value: V;
}

export variant Mapˉreplaceˉoutcome<V> {
    Replaced(Previous: V);
    Absent(Replacement: V);
    Rejected(Error: Collectionˉfailure, Replacement: V);
}

export fn Mapˉconstruct<K, V>(
    Budget: Memoryˉbudget,
    Maximumˉitems: u64,
) -> Result<Map<K, V>, Collectionˉfailure>
    effects(memory.allocate)
    where K: Ordering<K>;

export fn Mapˉconstructˉwithˉfirst<K, V>(
    Budget: Memoryˉbudget,
    Maximumˉitems: u64,
    Key: K,
    Value: V,
) -> Result<Map<K, V>, Mapˉinsertˉfailure<K, V>>
    effects(memory.allocate)
    where K: Ordering<K>;

export fn Mapˉinsert<K, V>(
    Map: borrow mut Map<K, V>,
    Key: K,
    Value: V,
) -> Result<unit, Mapˉinsertˉfailure<K, V>> effects()
    where K: Ordering<K>;

export fn Mapˉlength<K, V>(
    Map: borrow Map<K, V>,
) -> u64 effects();

export fn Mapˉcontains<K, V>(
    Map: borrow Map<K, V>,
    Key: borrow K,
) -> bool effects()
    where K: Ordering<K>;

export fn Mapˉfindˉrank<K, V>(
    Map: borrow Map<K, V>,
    Key: borrow K,
) -> Option<u64> effects()
    where K: Ordering<K>;

export fn Mapˉborrowˉat<K, V>(
    Map: borrow Map<K, V>,
    Index: u64,
) -> borrow V effects();

export fn Mapˉkeyˉat<K, V>(
    Map: borrow Map<K, V>,
    Index: u64,
) -> borrow K effects();

export fn Mapˉreplace<K, V>(
    Map: borrow mut Map<K, V>,
    Key: borrow K,
    Replacement: V,
) -> Mapˉreplaceˉoutcome<V> effects()
    where K: Ordering<K>;

export fn Mapˉremove<K, V>(
    Map: borrow mut Map<K, V>,
    Key: borrow K,
) -> Result<Option<Mapˉentry<K, V>>, Collectionˉfailure> effects()
    where K: Ordering<K>;

export fn Mapˉfreeze<K, V>(
    Map: Map<K, V>,
) -> Immutableˉmap<K, V> effects();

export fn Immutableˉmapˉlength<K, V>(
    Map: borrow Immutableˉmap<K, V>,
) -> u64 effects();

export fn Immutableˉmapˉcontains<K, V>(
    Map: borrow Immutableˉmap<K, V>,
    Key: borrow K,
) -> bool effects()
    where K: Ordering<K>;

export fn Immutableˉmapˉfindˉrank<K, V>(
    Map: borrow Immutableˉmap<K, V>,
    Key: borrow K,
) -> Option<u64> effects()
    where K: Ordering<K>;

export fn Immutableˉmapˉborrowˉat<K, V>(
    Map: borrow Immutableˉmap<K, V>,
    Index: u64,
) -> borrow V effects();

export fn Immutableˉmapˉkeyˉat<K, V>(
    Map: borrow Immutableˉmap<K, V>,
    Index: u64,
) -> borrow K effects();
~~~

Construction requires a positive maximum. Empty construction uses explicit
`::<K,V>` syntax and creates no dummy node. First-item construction derives `K`
and `V` from its explicit key/value and atomically inserts that pair. First-item
construction or insertion failure returns the original owned key/value and
leaves no partial observable change. The budget supplies the retained-byte bound
and success transfers its accounting into the map.

`Mapˉfindˉrank` returns absence or the owned ascending canonical rank.
`Mapˉkeyˉat` and `Mapˉborrowˉat` require `Index < Mapˉlength` and observe key and
value at that same rank. A violated precondition traps before access. Each direct
borrow is tied to the map's one borrowed owner; the key used to find a rank is no
longer a competing lifetime source. The borrow checker prevents intervening
exclusive mutation while a rank-derived borrow is live. `Mapˉcontains` remains a
Boolean convenience, not a borrow-lifetime proof.

Replacement first resolves the borrowed key without accepting the owned
replacement. Success accepts it exactly once and returns the previous owned
value. Absence or bounded comparison/allocation rejection returns the unchanged
owned replacement. Removal returns the stored owned key and value, absence
returns `None`, and rejection leaves the map unchanged. Consuming freeze cannot
fail or allocate and transfers the existing accounting and canonical order.
The immutable observation calls have the same rank, precondition, comparison,
and borrow rules as their mutable-owner counterparts.

## Deterministic sets

`Set<T>` is a move-owned finite membership collection. It requires
`Ordering<T>` and uses the same total-order, budget, and worst-case-work contract
as `Map<K, V>`.

The semantic contract is an ordered set:

- values are unique under `Ordering<T>.Compare == Equal`;
- membership, insertion, and removal have bounded worst-case work proportional
  to the map contract's published logarithmic function of maximum items;
- iteration is ascending canonical value order;
- successful insertion accepts ownership exactly once;
- already-present, capacity, comparison, or allocation rejection returns the
  original owned input and leaves the set unchanged;
- removal returns the owned stored value, while absence leaves the set unchanged;
- maximum items, retained bytes, comparison work, and diagnostic work are
  finite; and
- implementation layout, balancing, and reuse of a map-like representation are
  unobservable.

Consuming publication produces `Immutableˉset<T>` with the same canonical
iteration order and no mutation. A host hash set, process-randomized set, bit set,
or insertion-ordered set is a distinct future type rather than an implementation
of this contract.

The package-parser workload fixes the complete version-1 construction,
mutation, publication, and observation surface:

~~~text
export record Setˉinsertˉfailure<T> {
    Error: Collectionˉfailure;
    Value: T;
}

export fn Setˉconstruct<T>(
    Budget: Memoryˉbudget,
    Maximumˉitems: u64,
) -> Result<Set<T>, Collectionˉfailure>
    effects(memory.allocate)
    where T: Ordering<T>;

export fn Setˉconstructˉwithˉfirst<T>(
    Budget: Memoryˉbudget,
    Maximumˉitems: u64,
    Value: T,
) -> Result<Set<T>, Setˉinsertˉfailure<T>>
    effects(memory.allocate)
    where T: Ordering<T>;

export fn Setˉinsert<T>(
    Set: borrow mut Set<T>,
    Value: T,
) -> Result<unit, Setˉinsertˉfailure<T>> effects()
    where T: Ordering<T>;

export fn Setˉlength<T>(Set: borrow Set<T>) -> u64 effects();

export fn Setˉcontains<T>(
    Set: borrow Set<T>,
    Value: borrow T,
) -> bool effects()
    where T: Ordering<T>;

export fn Setˉfindˉrank<T>(
    Set: borrow Set<T>,
    Value: borrow T,
) -> Option<u64> effects()
    where T: Ordering<T>;

export fn Setˉborrowˉat<T>(
    Set: borrow Set<T>,
    Index: u64,
) -> borrow T effects();

export fn Setˉremove<T>(
    Set: borrow mut Set<T>,
    Value: borrow T,
) -> Result<Option<T>, Collectionˉfailure> effects()
    where T: Ordering<T>;

export fn Setˉfreeze<T>(Set: Set<T>) -> Immutableˉset<T> effects();

export fn Immutableˉsetˉlength<T>(
    Set: borrow Immutableˉset<T>,
) -> u64 effects();

export fn Immutableˉsetˉcontains<T>(
    Set: borrow Immutableˉset<T>,
    Value: borrow T,
) -> bool effects()
    where T: Ordering<T>;

export fn Immutableˉsetˉfindˉrank<T>(
    Set: borrow Immutableˉset<T>,
    Value: borrow T,
) -> Option<u64> effects()
    where T: Ordering<T>;

export fn Immutableˉsetˉborrowˉat<T>(
    Set: borrow Immutableˉset<T>,
    Index: u64,
) -> borrow T effects();
~~~

Set construction, insertion, rank, borrow, removal, and freeze have the same
positive-maximum, ownership-preservation, checked-rank, comparison-bound, and
accounting rules as the corresponding map operations. Duplicate insertion
returns `Collectionˉfailure.Duplicate` plus the original owned value. Removal
returns the stored owned value, not the borrowed search value. Freeze is
consuming, infallible, and allocation-free. Immutable rank observation is
ascending under the one resolved `Ordering<T>` implementation.

## Typed arenas and handles

`Arena<T>` is a move-owned store with one immutable positive runtime
`Maximumˉnodes`. It owns one allocation lease and every live node. Construction
validates its maximum before allocation, records it for admission and
diagnostics, and never grows past it. `Immutableˉarena<T>` is its consuming
read-only publication form: node ranks and handles remain stable, mutation is
unavailable, and the publication retains the arena's allocation lease.

`Handle<T>` is a Copy, non-owning pair of arena identity, slot index, and
generation. Its representation is opaque and not generally serializable,
orderable, or convertible to an integer.

Exact handle equality is available and compares arena identity, slot, and
generation; it does not prove that either handle is currently live. Liveness
requires lookup through the owning arena.

Insertion returns one handle. Lookup validates:

- arena identity;
- slot range;
- live occupancy;
- exact generation; and
- requested immutable or exclusive mutable access.

Removing a node returns its owned value and increments the slot generation. If a
generation would wrap, the slot is permanently retired until the arena is
destroyed. A stale handle never aliases a newer node. Destroying the arena
destroys every live node and invalidates every handle, including cyclic graphs.

The arena provides bounded iteration over live nodes in ascending slot order.
Graph traversal order is owned by the algorithm, not inferred from handle values.

The database-transaction and compiler-front-end workloads fix these first exact
arena operations:

~~~text
export record Arenaˉseed<T> {
    Owner: Arena<T>;
    First: Handle<T>;
}

export record Arenaˉinsertˉfailure<T> {
    Error: Collectionˉfailure;
    Value: T;
}

export record Arenaˉreplaceˉfailure<T> {
    Error: Collectionˉfailure;
    Value: T;
}

export fn Arenaˉconstruct<T>(
    Budget: Memoryˉbudget,
    Maximumˉnodes: u64,
) -> Result<Arena<T>, Collectionˉfailure>
    effects(memory.allocate);

export fn Arenaˉconstructˉwithˉfirst<T>(
    Budget: Memoryˉbudget,
    Maximumˉnodes: u64,
    First: T,
) -> Result<Arenaˉseed<T>, Arenaˉinsertˉfailure<T>>
    effects(memory.allocate);

export fn Arenaˉinsert<T>(
    Arena: borrow mut Arena<T>,
    Value: T,
) -> Result<Handle<T>, Arenaˉinsertˉfailure<T>> effects();

export fn Arenaˉreplace<T>(
    Arena: borrow mut Arena<T>,
    Handle: borrow Handle<T>,
    Value: T,
) -> Result<T, Arenaˉreplaceˉfailure<T>> effects();

export fn Arenaˉremove<T>(
    Arena: borrow mut Arena<T>,
    Handle: borrow Handle<T>,
) -> Result<T, Collectionˉfailure> effects();

export fn Arenaˉvalidate<T>(
    Arena: borrow Arena<T>,
    Handle: borrow Handle<T>,
) -> Result<unit, Collectionˉfailure> effects();

export fn Arenaˉborrowˉvalidated<T>(
    Arena: borrow Arena<T>,
    Handle: Handle<T>,
) -> borrow T effects();

export fn Arenaˉlength<T>(Arena: borrow Arena<T>) -> u64 effects();

export fn Arenaˉfreeze<T>(Arena: Arena<T>) -> Immutableˉarena<T> effects();

export fn Immutableˉarenaˉvalidate<T>(
    Arena: borrow Immutableˉarena<T>,
    Handle: borrow Handle<T>,
) -> Result<unit, Collectionˉfailure> effects();

export fn Immutableˉarenaˉborrowˉvalidated<T>(
    Arena: borrow Immutableˉarena<T>,
    Handle: Handle<T>,
) -> borrow T effects();

export fn Immutableˉarenaˉlength<T>(
    Arena: borrow Immutableˉarena<T>,
) -> u64 effects();
~~~

Empty construction uses explicit `::<T>` syntax and creates no dummy node. The
first-item constructor solves `T` from `First`, atomically constructs the arena
and inserts the value, and returns both owner and first handle. Failure releases
partial allocation and returns the original value unchanged. Ordinary insertion
checks capacity before acceptance and has the same ownership-return rule.

`Arenaˉreplace` validates the exact arena/slot/generation before mutation.
Success installs `Value` without changing the slot generation and returns the
previous owned node. Failure leaves the arena unchanged and returns the proposed
owned `Value` inside `Arenaˉreplaceˉfailure`. `Arenaˉremove` validates before
mutation; success vacates the slot, advances its generation or retires it before
wrap, and returns the removed owned node. Failure leaves the arena unchanged.
Both operations are bounded by the arena's admitted maximum and comparison-free
slot validation. A successful prior validation under the same uninterrupted
exclusive arena borrow proves that the corresponding mutation cannot fail.

`Arenaˉvalidate` returns a nominal failure for wrong arena, range, vacancy, stale
generation, or retired slot. `Arenaˉborrowˉvalidated` requires a successful
validation for the same arena/handle state and returns a direct borrow tied only
to the arena owner; the borrow operation copies `Handle<T>` by value, so it is
not a second lifetime source. Validation may borrow the handle because it
returns only owned unit/failure. Violation traps before access. No exclusive
arena mutation can occur between proof and use while that immutable owner borrow
is live. `Arenaˉfreeze` consumes the mutable owner without copying nodes, and
the corresponding `Immutableˉarena` operations preserve the same validation and
borrow rules. This avoids a borrowed value inside `Result` without weakening
recoverable validation or memory safety.

## Bounded iterators

The exact edition-1 compile-time protocol is:

~~~text
export protocol Iterator<Self, T> {
    fn Maximumˉremaining(Value: borrow Self) -> u64 effects();
    fn Next(Value: borrow mut Self) -> Option<T> effects();
}
~~~

It exposes:

- exact or maximum remaining items;
- one `Next` operation returning an owned-or-Copy `Option<T>`;
- finite per-item work and retained-state bounds; and
- deterministic iteration order.

An iterator cannot represent an infinite source in Language 1.0. A filter or
flat-map operation derives an admitted maximum from its input and expansion
bound. A generator that cannot state a finite maximum is rejected.

`for` accepts only compiler-known arrays, sequences, slices, maps, or an exact
`Iterator<Self, T>` implementation. A borrowing iterator needs named lifetime
representation and is not part of edition 1; compiler-known borrowed collection
iteration retains its direct owner provenance without pretending that
`Option<borrow T>` is an unrestricted stored value.

## Bytes and byte builder

`bytes` is the shared immutable byte sequence. Each value retains current byte
length and an admitted maximum byte length; sharing preserves both and never
exposes backing capacity. `Bytesˉbuilder` is an owned specialized construction
buffer with maximum output bytes, current length, one allocation lease, and no
implicit text encoding. `Byteˉbuffer` is a move-owned fixed-length initialized
buffer for bounded mutable byte I/O.

The first reviewed paper workload fixes these version-1 immutable-byte
signatures:

~~~text
export fn Length(Value: borrow bytes) -> u64 effects();
export fn At(Value: borrow bytes, Index: u64) -> u8 effects();

export fn Borrowˉrange(
    Value: borrow bytes,
    Start: u64,
    Length: u64,
) -> Slice<u8> effects();
~~~

`Length` returns the current byte length, not the admitted maximum or hidden
capacity. `At` requires `Index < Length(Value)`. It checks that precondition with
`u64` arithmetic before reading and produces a terminal bounds trap on
violation; it can never read outside the value or return a partial byte. Code
parsing untrusted offsets first proves the complete range or uses a separately
named recoverable codec. No unchecked Core or Hosted counterpart is implied.

`Borrowˉrange` checks `Start + Length` with checked arithmetic against the
exact byte length before constructing a view. An empty range may use only the
admitted one-past-end boundary. The slice is tied to `Value` as its one borrowed
owner and cannot outlive, move, or release that owner.

The file-copy workload fixes these version-1 byte-buffer signatures:

~~~text
export fn Constructˉbuffer(
    Budget: Memoryˉbudget,
    Length: u64,
) -> Result<Byteˉbuffer, Allocationˉfailure>
    effects(memory.allocate);

export fn Bufferˉlength(
    Buffer: borrow Byteˉbuffer,
) -> u64 effects();

export fn Borrowˉslice(
    Buffer: borrow Byteˉbuffer,
    Start: u64,
    Length: u64,
) -> Slice<u8> effects();

export fn Borrowˉsliceˉmut(
    Buffer: borrow mut Byteˉbuffer,
    Start: u64,
    Length: u64,
) -> Mutableˉslice<u8> effects();
~~~

Construction consumes one rights-reduced budget. Success transfers its
accounting into exactly `Length` zero-initialized bytes; failure consumes and
locally releases the child budget without exposing a partial buffer.
`Bufferˉlength` reports the fixed current length. Both slice calls check
`Start + Length` and the complete buffer range before forming a borrow. Their
results are tied to the one buffer owner and obey ordinary immutable/exclusive
borrow rules. No safe uninitialized byte, backing capacity, native address, or
unchecked Core/Hosted slice is exposed.

The exact edition-1 builder operations are the registry calls below:

- append one byte;
- append immutable bytes or canonical UTF-8 text;
- append an exact-width integer with named byte order;
- append invariant unsigned decimal; and
- consume into `bytes`.

Each append is either all-or-nothing or explicitly prefix-admitting. A failed
all-or-nothing append leaves content and length unchanged.

Edition 1's constructor commits the complete maximum, so there is no later
reserve call. Truncation and direct builder-slice observation are omitted from
the minimum surface: callers build a new bounded result when rollback is needed,
which avoids publishing aliases into mutable construction state.

The command workload fixes this version-1 reserved construction family:

~~~text
export fn Constructˉreserved(
    Budget: Memoryˉbudget,
    Maximumˉoutputˉbytes: u64,
) -> Result<Bytesˉbuilder, Allocationˉfailure>
    effects(memory.allocate);

export fn Appendˉbytes(
    Builder: borrow mut Bytesˉbuilder,
    Value: borrow bytes,
) -> Result<unit, Limitˉfailure> effects();

export fn Appendˉutf8(
    Builder: borrow mut Bytesˉbuilder,
    Value: borrow text,
) -> Result<unit, Limitˉfailure> effects();

export fn Appendˉu8(
    Builder: borrow mut Bytesˉbuilder,
    Value: u8,
) -> Result<unit, Limitˉfailure> effects();

export fn Appendˉu32ˉlittle(
    Builder: borrow mut Bytesˉbuilder,
    Value: u32,
) -> Result<unit, Limitˉfailure> effects();

export fn Appendˉu64ˉlittle(
    Builder: borrow mut Bytesˉbuilder,
    Value: u64,
) -> Result<unit, Limitˉfailure> effects();

export fn Appendˉu64ˉdecimal(
    Builder: borrow mut Bytesˉbuilder,
    Value: u64,
) -> Result<unit, Limitˉfailure> effects();

export fn Freeze(Builder: Bytesˉbuilder) -> bytes effects();
~~~

`Constructˉreserved` consumes the budget and commits the complete output maximum
before returning. Constructor failure consumes and locally releases that child
budget. Later appends cannot fail for physical growth, but reject before
mutation with `Limitˉfailure` when the complete result would exceed the maximum.
All appends are all-or-nothing. `Appendˉutf8` emits canonical UTF-8 without a
host encoding. The integer operations prove their complete resulting length
with checked arithmetic before mutation and append exactly 1, 4, or 8 bytes;
the multi-byte forms use little endian regardless of host alignment or byte
order. `Appendˉu64ˉdecimal` emits shortest unsigned ASCII decimal, with zero
as `0` and no sign, locale, grouping, padding, or radix prefix. It too proves the
complete resulting length before mutation. `Freeze` consumes the builder and
transfers retained accounting to the exact immutable result without fallible
compaction.

Byte codecs validate complete input ranges before reading and use checked offset
arithmetic. No codec inherits native alignment or endianness.

## Text and text builder

`text` is a shared immutable Unicode scalar sequence with valid canonical UTF-8
interchange. Each value retains current UTF-8 byte length, rune count, and
admitted maximum UTF-8 byte length; sharing preserves those values and never
exposes backing capacity. `Textˉbuilder` is an owned bounded UTF-8 construction
buffer that never exposes malformed text as `text`.

The exact edition-1 operations include:

- append rune;
- append text;
- append the accepted invariant numeric forms;
- inspect rune count and byte count separately;
- strictly decode complete UTF-8 bytes or a byte slice; and
- consume into `text`.

Text builder maximum output is measured in bytes. Rune count is separately
bounded by byte maximum. Failure leaves the builder unchanged unless an operation
is explicitly named as prefix-admitting.

The command and compiler-front-end workloads fix these version-1 observations
and reserved operations:

~~~text
export variant Decodeˉfailure {
    Inputˉlimit(Byteˉoffset: u64, Observed: u64, Maximum: u64);
    Runeˉlimit(Byteˉoffset: u64, Observed: u64, Maximum: u64);
    Invalidˉlead(Byteˉoffset: u64);
    Invalidˉcontinuation(Byteˉoffset: u64);
    Truncated(Byteˉoffset: u64);
    Overlong(Byteˉoffset: u64);
    Surrogate(Byteˉoffset: u64);
    Outˉofˉrange(Byteˉoffset: u64);
}

export variant Decodeˉutf8ˉfailure {
    Allocation(Error: Allocationˉfailure);
    Source(Error: Decodeˉfailure);
}

export fn Byteˉlength(Value: borrow text) -> u64 effects();
export fn Runeˉcount(Value: borrow text) -> u64 effects();

export fn Decodeˉutf8ˉreserved(
    Budget: Memoryˉbudget,
    Value: borrow bytes,
    Maximumˉbytes: u64,
    Maximumˉrunes: u64,
) -> Result<text, Decodeˉutf8ˉfailure> effects(memory.allocate);

export fn Decodeˉutf8ˉsliceˉreserved(
    Budget: Memoryˉbudget,
    Value: Slice<u8>,
    Maximumˉbytes: u64,
    Maximumˉrunes: u64,
) -> Result<text, Decodeˉutf8ˉfailure> effects(memory.allocate);

export fn Decodeˉfailureˉbyteˉoffset(
    Error: borrow Decodeˉfailure,
) -> u64 effects();

export fn Runeˉat(Value: borrow text, Index: u64) -> rune effects();
export fn Runeˉutf8ˉwidth(Value: rune) -> u64 effects();

export fn Shareˉrange(
    Value: borrow text,
    Startˉrune: u64,
    Runeˉcount: u64,
) -> text effects();

export fn Constructˉreserved(
    Budget: Memoryˉbudget,
    Maximumˉoutputˉbytes: u64,
) -> Result<Textˉbuilder, Allocationˉfailure>
    effects(memory.allocate);

export fn Appendˉtext(
    Builder: borrow mut Textˉbuilder,
    Value: borrow text,
) -> Result<unit, Limitˉfailure> effects();

export fn Appendˉrune(
    Builder: borrow mut Textˉbuilder,
    Value: rune,
) -> Result<unit, Limitˉfailure> effects();

export fn Appendˉu64ˉdecimal(
    Builder: borrow mut Textˉbuilder,
    Value: u64,
) -> Result<unit, Limitˉfailure> effects();

export fn Appendˉu32ˉhexˉfixed(
    Builder: borrow mut Textˉbuilder,
    Value: u32,
) -> Result<unit, Limitˉfailure> effects();

export fn Appendˉf32ˉcanonical(
    Builder: borrow mut Textˉbuilder,
    Value: f32,
) -> Result<unit, Limitˉfailure> effects();

export fn Appendˉf64ˉcanonical(
    Builder: borrow mut Textˉbuilder,
    Value: f64,
) -> Result<unit, Limitˉfailure> effects();

export fn Freeze(Builder: Textˉbuilder) -> text effects();
~~~

`Byteˉlength` reports canonical UTF-8 bytes. `Runeˉcount` reports Unicode scalar
values, not UTF-16 code units, grapheme clusters, display cells, or locale
characters. Strict reserved decode rejects byte/rune limits before excess
allocation, accepts only shortest canonical UTF-8 scalar encodings, distinguishes
physical allocation from source failure, and releases the child budget without
a partial `text` on either failure. `Runeˉat` requires an
in-range scalar index and traps before access otherwise. `Runeˉutf8ˉwidth`
returns 1 through 4. `Shareˉrange` validates checked rune geometry and returns a
shared immutable range whose descriptor exposes exactly its byte/rune bounds;
it may retain the same backing and charge and never creates a mutable alias.
Reserved construction has the same committed-capacity, local failure-release,
atomic append, and accounting-transfer rules as the byte builder.
`Appendˉrune` emits the rune's canonical one-through-four-byte UTF-8 encoding.
`Appendˉu64ˉdecimal` emits invariant shortest unsigned decimal.
`Appendˉu32ˉhexˉfixed` emits exactly eight lowercase hexadecimal digits with no
prefix. `Appendˉf32ˉcanonical` and `Appendˉf64ˉcanonical` emit `nan`, `inf`,
`-inf`, `0`, or `-0` for special values. A finite nonzero value uses the shortest
ASCII decimal numeral that round-trips through the corresponding canonical
parser under roundTiesToEven. It has
no plus sign, grouping, redundant leading/trailing zero, exponent plus, or
exponent leading zero; `e` is lowercase. Among equal-byte-length round-tripping
candidates, choose least mathematical distance to the exact value, then an even
final coefficient digit, then ordinal ASCII order. No finite f32 result exceeds
24 bytes and no finite f64 result exceeds 32 bytes. Both calls prove their whole
append before mutation and use no locale, host formatting library, allocation,
or hidden fast-math mode.

`Decodeˉutf8ˉsliceˉreserved` has the same validation, limits, budget,
publication, and failure contract over one ephemeral immutable byte slice. The
slice remains borrowed only for the call. Success publishes independent shared
immutable text and does not retain the slice's mutable-buffer owner or allocate
an intermediate immutable byte value.

Canonical compiler positions over `text` use zero-based byte/rune offsets and
one-based scalar line/column. LF advances the line and resets the column; CR and
tab each advance one scalar column. Canonical positions do not normalize text,
translate host newlines, or count UTF-16 units, grapheme clusters, display
cells, or locale characters.

## Formatting

`Formatting<T>` is a compile-time protocol with:

- a function deriving or validating maximum UTF-8 output bytes from the value
  type, value maximums, and selected format;
- an append operation targeting a mutable text builder;
- invariant default numeric spelling;
- no allocation outside the supplied builder and lease;
- no capability, locale, clock, entropy, or provider access; and
- exact escaping behavior when a named format requires it.

User-visible locale, collation, pluralization, time zone, and cultural formatting
require explicit library data and are not the default Formatting protocol.

The exact default protocol is:

~~~text
export protocol Formatting<T> {
    fn Maximumˉutf8ˉbytes(
        Value: borrow T,
    ) -> Result<u64, Limitˉfailure> effects();

    fn Append(
        Builder: borrow mut Textˉbuilder,
        Value: borrow T,
    ) -> Result<unit, Limitˉfailure> effects();
}
~~~

The compiler resolves one exact implementation from the field type. A named
format is a distinct nominal wrapper with its own implementation, not an
overload string interpreted at runtime. A caller evaluates
`Maximumˉutf8ˉbytes` before builder mutation; `Append` cannot exceed that
admitted value. Edition 1 uses this protocol through explicit bounded-builder
calls and has no standalone interpolation syntax with hidden allocation.

## Local resource release

`Foundationˉresource.Localˉrelease<Self>` is a compiler-recognized protocol:

~~~text
export protocol Localˉrelease<Self> {
    fn Release(Value: Self) -> unit effects(resource.release);
}
~~~

Only a move-owned resource may implement it. `Release`:

- consumes exactly one live local handle;
- invalidates local use even when the provider has failed;
- returns locally retained provider capacity where possible;
- performs no fallible semantic flush, commit, finish, or graceful shutdown;
- is idempotent only through consuming ownership, not by accepting stale copies;
  and
- cannot allocate unbounded state or throw.

`using` invokes this exact protocol on ordinary scope exit. A resource exposes
separate typed operations for fallible completion.

The file-copy workload fixes the first exact completion sequencing rule. A
resource body that fails returns that body failure and does not implicitly
finish. A successful body calls its named completion operation explicitly and
returns that exact completion failure or uncertainty when completion does not
succeed. Local release then consumes the handle on every ordinary path without
replacing either result. A protocol that must complete after body failure needs
an explicit named result capable of retaining both outcomes; it cannot assign
hidden precedence to `using`.

### Completion outcomes

Mutating external I/O distinguishes:

~~~text
export variant Mutationˉoutcome<E> {
    Rejected(Error: E);
    Acceptedˉpartial(Completed: u64, Error: E);
    Completed(Completed: u64);
    Indeterminate(Error: E);
}
~~~

Rejected proves zero external progress. Accepted partial reports exact progress.
Completed reports exact accepted progress under the interface's completion
meaning. Indeterminate means progress cannot be proved and must not be retried
without a specified idempotency key or recovery protocol.

## Hosted operation context

`Foundationˉoperation.Operationˉcontext` is a shared immutable opaque Hosted
value. A launcher-created
root binds a nonzero monotonic clock identity and generation, one absolute
deadline, a nonzero cancellation-view identity and generation, and the already
admitted provider deadline span. Source cannot construct it, inspect civil time
through it, change either generation, extend its deadline, or request
cancellation through it.

The value is Copy only within its compiler-proved origin lifetime. Copying does
not duplicate a timer, grant authority, or multiply accounting. Provider calls
borrow it as an explicit cancellation/deadline observation point. At the exact
deadline tick, deadline wins. Pre-dispatch cancellation proves no operation
progress; a dispatched mutation may remain indeterminate under its interface.

Task construction below derives a child context with a deadline no later than
its parent and a fresh scope-owned cancellation identity/generation. That value
may be copied into joined children but cannot escape its lexical task scope.
Scope teardown invalidates the derived generation. A later use of forged,
serialized, cross-scope, or stale context evidence fails closed.

## Structured task Foundation

Structured tasks are Hosted-only.

### Task scope limits

~~~text
export record Taskˉlimits {
    Maximumˉchildren: u32;
    Maximumˉrunnable: u32;
    Maximumˉcompleted: u32;
    Maximumˉretainedˉbytes: u64;
    Maximumˉworkˉunits: u64;
    Maximumˉcallˉdepth: u32;
    Maximumˉtimers: u32;
    Maximumˉdiagnostics: u32;
}
~~~

`Taskˉscope` is move-owned and implements Local release. Construction consumes a
rights-reduced memory budget, borrows one valid parent operation context, and
reserves bounded task-runtime state. The surrounding grammar records one
scope-exit policy: join, cancel then join, or fail then join.

~~~text
export variant Taskˉscopeˉfailure {
    Invalidˉlimits(
        Field: u32,
        Observed: u64,
        Minimum: u64,
        Maximum: u64,
    );
    Allocation(Error: Allocationˉfailure);
    Parentˉcontextˉstale(
        Expectedˉgeneration: u64,
        Observedˉgeneration: u64,
    );
    Runtimeˉunavailable(
        Expectedˉgeneration: u64,
        Observedˉgeneration: u64,
    );
}
~~~

The first accepted construction signature is:

~~~text
export fn Construct(
    Budget: Memoryˉbudget,
    Limits: Taskˉlimits,
    Parentˉcontext: borrow Operationˉcontext,
) -> Result<Taskˉscope, Taskˉscopeˉfailure>
    effects(memory.allocate, resource.acquire);
~~~

`Construct` consumes `Budget`. On rejection it releases any consumed local
accounting before returning the exact typed failure; it does not recover the
budget implicitly. Invalid child/runnable/completion/retained-byte/work/depth/
timer/diagnostic relationships reject before scheduling. A stale parent or
unavailable task runtime retains exact expected/observed generation evidence.
The surrounding `task scope` statement supplies the one explicit exit policy,
so `Construct` has no default or hidden policy.

The accepted scope context observation is:

~~~text
export fn Operationˉcontext(
    Scope: borrow Taskˉscope,
) -> Operationˉcontext effects();
~~~

It returns the scope-derived Copy view described above. The immutable borrow of
`Scope` ends with the call; the returned value carries the scope lifetime
provenance rather than retaining a source borrow that would prevent later
spawns or cancellation requests.

### Task handle

`Task<T, E>` is a move-owned handle to one child in its lexical scope. It cannot
outlive that scope or detach. Await consumes or exclusively borrows the handle
according to the operation and yields one:

~~~text
export variant Taskˉoutcome<T, E> {
    Valid(Value: T);
    Failure(Error: E);
    Cancelled;
    Deadlineˉreached;
    Runtimeˉlost(
        Expectedˉgeneration: u64,
        Observedˉgeneration: u64,
    );
    Runtimeˉrestarted(
        Expectedˉgeneration: u64,
        Observedˉgeneration: u64,
    );
    Trapped(Identity: u32);
}
~~~

Trap identity is bounded diagnostic evidence, not a catchable source exception or
arbitrary stack trace. Runtime loss/restart describes only the task runtime.
Loss or restart of a capability used by the child remains inside `E`; the two
domains cannot be collapsed into one payload-free provider result.

The accepted version-1 operation for this handle consumes it exactly once:

~~~text
export async fn Await<T, E>(Handle: Task<T, E>)
    -> Taskˉoutcome<T, E> effects(task.suspend);
~~~

Both generic parameters are solved structurally from the explicit `Handle`
argument. Await never detaches the child or returns while retaining a second
source-visible owner for the handle.

### Spawn and join

`Spawn(Scope: borrow mut Taskˉscope, Work: async fn ...)` receives one
explicit async closure and its capture modes. Spawn either:

- rejects before starting the child and returns the owned closure and captures
  in its typed rejection result; or
- accepts them exactly once and returns a task handle.

The rejection shape is:

~~~text
export variant Spawnˉfailure<W> {
    Scopeˉclosing(Work: W);
    Taskˉlimit(Work: W);
    Queueˉlimit(Work: W);
    Memoryˉfailure(
        Error: Allocationˉfailure,
        Work: W,
    );
}
~~~

For one exact async closure type `W` whose explicit no-argument call signature is
`async fn() -> Result<T, E> effects(F)`, the accepted version-1 semantic call
family is:

~~~text
Spawn(
    Scope: borrow mut Taskˉscope,
    Work: W,
) -> Result<Task<T, E>, Spawnˉfailure<W>>
    effects(memory.allocate, task.spawn)
~~~

`W` is solved from the explicit `Work` argument. `T`, `E`, and the exact finite
effect set `F` are structural components of that argument's function type; none
is selected from the result context. This is one semantic generic family, not an
overload set or a request for explicit generic-call syntax. The caller and
declaring module must admit `F`; Spawn's own immediate effects are
`memory.allocate` and `task.spawn`.

A rejected spawn returns the exact closure, including every moved capture, so
the caller again owns it. Once spawn returns a task handle, the child owns the
captures and a later task failure never rolls them back.

`Maximumˉretainedˉbytes` bounds scope-owned scheduler, continuation, and
terminal-outcome state. Spawn computes the selected runtime profile's exact
reservation before accepting `Work`. If the reservation exceeds the remaining
scope limit, spawn returns `Memoryˉfailure` with
`Allocationˉreason.Budgetˉexhausted`, exact requested and available byte
counts, and the unchanged owned `Work`. Completion does not release the
reservation; consuming the handle or bounded scope teardown does.

Owned allocations reachable through captures or task outcomes remain charged
to their explicit `Memoryˉbudget` or `Allocationˉlease`. The scope keeps those
values live while required but does not double-charge their allocation bytes as
scheduler state. A runtime profile must publish its exact scheduler-state
accounting formula and preserve the same accept-or-return ownership boundary.

Join ordering for join-all is child creation order, not scheduler completion
order. An explicitly named completion-order operation may exist only with a
bounded completion queue and stable child identities.

Cancellation is cooperative and observable only at explicit checks, provider
operations, and `await` suspension points. Its accepted source operation is:

~~~text
export variant Cancelˉrequestˉoutcome {
    Requested(Liveˉchildren: u32);
    Alreadyˉrequested(Liveˉchildren: u32);
}

export fn Requestˉcancel(
    Scope: borrow mut Taskˉscope,
) -> Cancelˉrequestˉoutcome effects(task.cancel);
~~~

The first request closes the scope to new spawn acceptance, marks the one
scope-owned cancellation generation, and reports the then-live child count.
Later requests are idempotent and report the then-live count. Cancelling a scope
still joins every accepted child before scope release.

No task can become detached, retain a borrowed value beyond the proven owner
lifetime, capture an optional-only capability, or silently replay an
indeterminate external mutation.

A temporary exclusive borrow passed directly into one awaited provider call is
valid when its owner lives in the same child continuation and no alias can
execute until the await completes. It cannot be stored in task state, returned,
or captured from an outer mutable owner into a spawned child. This is a
lifetime/aliasing proof, not a provider exception to ownership.

The scope-exit policies mean:

- `join` waits for every live child and retains their individually typed
  outcomes for explicit collection;
- `cancel_join` requests cancellation for every live child on every block exit
  and then waits for teardown; and
- `fail_join` joins normally until the block or one observed child fails or
  traps, then requests cancellation for remaining children and joins them before
  propagating the initiating outcome.

`Maximumˉcompleted` reserves one eventual terminal-outcome slot for every
accepted live child. Spawn rejects before capture acceptance when no slot can be
reserved. Child completion retains that reservation; only consuming its handle
with `await`, or bounded scope teardown, releases it. Scope exit does not
allocate an unbounded outcome list. A block `return`, `try` propagation,
`break`, or `continue` first applies the selected scope policy and local release,
then continues the transfer.

The concurrent hosted-service paper workload accepts these signatures and the
source spelling as normative-candidate inputs. The registry now records their
canonical candidate signature-set identity; accepting that identity remains an
explicit source-freeze decision and not an implementation claim.

## Unsafe Foundation

`Foundationˉunsafe` exists only in System modules. It provides opaque:

- `Rawˉaddress<Scope>`;
- `Foreignˉpointer<T, Abi>`;
- `Nullableˉforeignˉpointer<T, Abi>`;
- `Volatileˉpointer<T, Scope>`;
- `Dmaˉregion<Device, Generation>`; and
- exact ABI layout witnesses.

None is an ordinary integer, serializable value, ambient capability, or source of
authority. Construction and use require an unsafe block plus the platform,
authority, and capability contract named by the operation.

Address addition and range construction use checked arithmetic over an exact
address-width witness. Dereference states size, alignment, initialization,
aliasing, lifetime, and concurrency. No operation infers host C layout from a
Windvale record.

The module may be implemented intrinsically, but its public contracts remain
versioned and independently tested.

The type-identity checkpoint publishes the canonical edition-1
`Foundationˉunsafe` module with the four foreign pointer/scratch/region type
identities below and the exact two failure variants. Their current one-word
physical record carrier is compiler-private: typed WVIR requires the exact
canonical module, profile, name, arity, and layout, then rejects ordinary
construction and field observation. Same-named records in another module do
not acquire opacity.

The first producer checkpoint additionally publishes exactly
`Constructˉscratch` through compiler-owned source binding and typed WVIR
operation `186`. It requires an explicit `Memoryˉbudget`, exact `u64` length
and alignment, one declared ABI enum, and the canonical affine result. The
source backend serializes that exact operation as WVB 1.33 opcode `DC`,
preserving the budget-local, construction-Result, and ABI-enum indexes. The
complete verifier admits that version, and the first source-built scalar
provider executes bounded 1-through-64-byte, at-most-8-aligned construction
with exact zeroing, failure, private lease, opaque-carrier, and invocation
teardown behavior.

The next observation checkpoint publishes exact `Scratchˉlength` as typed
WVIR operation `187` and WVB 1.35 opcode `DD`. Its argument is an immutable
borrow of the exact ABI-matched scratch, its result is `u64`, and it has an
empty effect set. The scalar provider and native x86-64 backend read the
provider-private retained length in constant time without consuming the owner,
copying the backing, or exposing its address.

The mutable-borrow checkpoint publishes exact `Borrowˉwriteˉregion` through
source binding and typed WVIR operation `188`. Its arguments are one direct
mutable borrow of the ABI-matched scratch plus exact `u64` start, length, and
required-alignment values. Its contextual result is the canonical
`Result<Foreignˉwriteˉregion<Abi>, Foreignˉpointerˉfailure>`, and it contributes
`unsafe.address`. WVIR 1.27/1.28 retain the scratch slot, three scalar operands,
and ABI identity while independent validation rejects wrong borrow modes,
types, labels, effects, or result relationships.

The WVB checkpoint serializes that exact WVIR operation as WVB 1.36 opcode
`DE`. It consumes the three `u64` scalar values and carries direct scratch-local,
canonical Result-type, and ABI-enum indexes. The compiler-aligned verifier
contains the Result and scratch affinely, permits observation of exact Failure
data, and keeps the Valid region payload inaccessible. The bounded scalar
provider and compiler-verified native x86-64 backend execute zero-length,
address-overflow, owner-range, alignment, and success outcomes through private
subrange descriptors with no exposed native address. Native success retains
only checked logical start and length; it does not form a pointer or grant
dereference authority.

The next typed compiler checkpoint publishes exact
`Writeˉpointer::<Abi>(Region: borrow Region)` as WVIR operation `189`. The
argument is an immutable borrow of one directly named canonical
`Foreignˉwriteˉregion<Abi>` parameter or local, and the contextual result is
the exact opaque `Foreignˉpointer<u8, Abi>`. WVIR 1.29/1.30 preserve the region
slot and ABI identity with zero operands; independent validation rejects wrong
context, borrow mode, label, element, ABI, result, version, slot, or operation
relationships. Candidate WVB 1.37 serializes that operation as 13-byte opcode
`DF` with direct region-local, canonical pointer-record, and ABI-enum indexes.
The exact immutable region parameter uses borrowed-record shape `28` only when
the same function directly targets it with `DF`.

The complete compiler-aligned verifier admits the exact direct-parameter
derivation as an affine pointer value. It allows direct discard or the
compiler-generated consuming move between two exact pointer locals and rejects
`local.take`, copying, unavailable-local use, call or return escape, and record
embedding. The verifier bounds a module to 4,096 pointer derivations and 256
explicit region/pointer/ABI relations. Every execution consumer still rejects
minor 37. The candidate therefore forms no native address and grants no
dereference or call authority. Provider and native address formation,
authenticated Foreign calls, host ABI publication, Linux execution, and
paired-host containment remain pending.
The remaining operations below may appear only with their compiler-owned
intrinsic semantics and containment evidence.

`Foreignˉpointer<T, Abi>` is non-null but remains unsafe and opaque. Non-null
does not prove alignment, accessible range, initialization, lifetime, aliasing,
ownership, or permission to dereference. When a foreign ABI admits null, its
signature uses the distinct `Nullableˉforeignˉpointer<T, Abi>`; there is no
implicit `null`, zero conversion, or optional-pointer spelling. An unsafe named
`Requireˉnonˉnull` call distinguishes null and produces a non-null foreign
pointer, but dereference still needs the complete region/layout/lifetime proof.

The System/FFI paper workload fixes this first exact caller-owned scratch and
foreign-write surface:

~~~text
export variant Foreignˉmemoryˉfailure {
    Invalidˉlength(Observed: u64, Maximum: u64);
    Invalidˉalignment(Observed: u64);
    Allocation(Error: Allocationˉfailure);
    Unsupportedˉabi;
}

export variant Foreignˉpointerˉfailure {
    Null;
    Addressˉoverflow(Start: u64, Length: u64, Addressˉbits: u32);
    Outˉofˉrange(Start: u64, Length: u64, Ownerˉlength: u64);
    Misaligned(Start: u64, Requiredˉalignment: u64);
    Aliasing;
    Lifetimeˉended;
    Unsupportedˉabi;
}

export unsafe fn Requireˉnonˉnull<T, Abi>(
    Value: Nullableˉforeignˉpointer<T, Abi>,
) -> Result<Foreignˉpointer<T, Abi>, Foreignˉpointerˉfailure>
    effects(unsafe.address);

export fn Constructˉscratch<Abi>(
    Budget: Memoryˉbudget,
    Length: u64,
    Alignment: u64,
) -> Result<Foreignˉscratch<Abi>, Foreignˉmemoryˉfailure>
    effects(memory.allocate);

export fn Scratchˉlength<Abi>(
    Scratch: borrow Foreignˉscratch<Abi>,
) -> u64 effects();

export unsafe fn Borrowˉwriteˉregion<Abi>(
    Scratch: borrow mut Foreignˉscratch<Abi>,
    Start: u64,
    Length: u64,
    Requiredˉalignment: u64,
) -> Result<Foreignˉwriteˉregion<Abi>, Foreignˉpointerˉfailure>
    effects(unsafe.address);

export unsafe fn Writeˉpointer<Abi>(
    Region: borrow Foreignˉwriteˉregion<Abi>,
) -> Foreignˉpointer<u8, Abi> effects(unsafe.address);

export fn Regionˉlength<Abi>(
    Region: borrow Foreignˉwriteˉregion<Abi>,
) -> u64 effects();

export fn Borrowˉscratchˉslice<Abi>(
    Scratch: borrow Foreignˉscratch<Abi>,
    Start: u64,
    Length: u64,
) -> Slice<u8> effects();
~~~

Scratch construction requires positive length and power-of-two alignment, checks
both against the selected ABI/address-width witness, allocates exactly the
admitted extent, and zero-initializes every byte. Failure consumes and locally
releases the supplied budget. Success owns one allocation lease; lexical drop
releases it. Scratch cannot be shared, serialized, stored in Core/Hosted data,
or reinterpreted as a native allocator handle.

`Borrowˉwriteˉregion` checks `Start + Length`, base-address plus start/end, ABI
address width, owner bounds, required power-of-two alignment, live lifetime, and
exclusive alias state before publishing a region. Failure publishes no pointer
and leaves the scratch unchanged. The region exclusively borrows the scratch;
its pointer cannot escape that lifetime or coexist with a Windvale observation
of the bytes. `Writeˉpointer` exposes only the region start and exact remaining
extent to a compatible no-retain foreign signature.

After the region and every derived pointer are dead, `Borrowˉscratchˉslice`
performs ordinary checked range construction over initialized bytes. Foreign
bytes are untrusted values and must be decoded before safe publication. The
slice cannot expose an address or retain the scratch beyond its borrow.

Calling a foreign function assumes only its declared memory-safety preconditions:
it may write within the supplied region and no farther, obey alignment/aliasing,
and retain no pointer when the ABI contract says no-retain. Returned bytes,
lengths, enums, Booleans, generations, and status are still untrusted and
recoverably validated. A foreign write outside the supplied region, use after
the call, forbidden unwind, or calling-convention violation may already have
destroyed process integrity and follows the ABI's terminal containment policy;
it is not reported as an ordinary safe `Result`.

## Intrinsics and ordinary implementations

A compiler intrinsic is permitted only when:

- this document or a child format contract names the exact Foundation operation;
- a simple ordinary reference implementation or oracle exists;
- both paths have identical typing, ownership, failure, bounds, and effects;
- malformed or unsupported use is rejected before privileged lowering; and
- target absence produces an unsupported-target diagnostic rather than a
  semantic substitute.

Builders, numeric codecs, strict float helpers, collection bulk operations, and
task suspension may receive intrinsic lowering. Intrinsic status never makes an
API ambient or removes its resource accounting.

## Foundation freeze requirements

This is the Foundation source-contract gate, not an implementation qualification
claim. A paper case passes when the exact signature, ownership, effect, bound,
failure precedence, and expected result are coherent. Migration must make the
same cases executable against ordinary reference implementations and any
intrinsic lowering before Foundation conformance is reported.

Before source freeze, the candidate registry supplies the exact module blocks
and hashes while the owner decision must still accept them. The freeze review
must confirm that:

1. every required module has a canonical major version and signature-set
   identity;
2. all ordinary generic and protocol signatures parse under the frozen grammar,
   and each compiler-recognized semantic family has one canonical structural
   relation and signature-set encoding;
3. ownership on every success and failure path is explicit;
4. collection algorithms have stable worst-case bounds;
5. map ordering and arena generation behavior pass adversarial paper cases;
6. numeric conversion and parsing matrices are complete;
7. builder and formatting maxima are computable in every admitted case;
8. resource release cannot discard a completion or body result;
9. ordered set insertion, duplicate, removal, publication, capacity, and
   ownership outcomes pass the paper corpus;
10. task scope, capture, cancellation, join, and teardown pass the paper corpus;
11. unsafe values cannot enter Core or Hosted source; and
12. fixed byte-buffer initialization, slicing, ownership, and release pass the
    paper corpus;
13. known partial progress never permits replay of an uncertain mutation;
14. runtime-bounded arena capacity, generation validation, and two-step borrowed
    observation pass the paper corpus;
15. ordered-map first construction, ownership-return failure, presence proof,
    and canonical rank access pass the paper corpus;
16. explicit empty collection construction, immutable arena publication, and
    one-owner rank/handle borrows pass the compiler paper corpus;
17. strict UTF-8 decode, scalar source positions, diagnostic saturation, and
    exact integer byte appends pass boundary and deterministic-output cases;
18. all explicit generic Foundation calls name the complete canonical instance;
19. checked slice observation, immutable byte-range borrowing, strict slice
    decode, and invariant decimal byte append pass malformed-range, ownership,
    allocation, and capacity cases;
20. deadline/cancellation context and exact stream progress remain compatible
    with the structured-service workload without permitting indeterminate
    mutation replay; and
21. task construction, derived context, cancellation, result collection, and
    runtime/provider failure separation pass the concurrent-service cases;
22. typed arena replacement/removal, generation-safe tombstones, and immutable
    frame publication pass the retained-GUI cases;
23. contextual fixed arrays, checked immutable/exclusive slices, strict float
    operations/conversions, canonical numeric formatting, and bit-identical
    parallel equivalence pass the numeric/graphics cases; and
24. complete mutable/immutable map and ordered-set ownership, canonical rank,
    comparison-law, package-content dedup/accounting, and graph-order cases pass
    the package-parser corpus;
25. registered ABI identity, nullable/non-null pointer, aligned scratch, checked
    address/range/lifetime/alias, foreign outcome, isolated containment, and safe
    publication pass the System/FFI corpus; and
26. a responsibility matrix identifies ordinary source, compiler intrinsic,
    runtime, provider, and target-specific ownership for each operation.
