# Windvale Language 1.0 Foundation specification

## Status and scope

This is the normative-candidate Foundation companion to the
[Language 1.0 semantic specification](Windvale-Language-1.0.md), authorized by
[Decision 0751](../Documents/Decisions/0751-Accept-Windvale-Language-1.0-Direction.md).
It specifies the standard nominal values and protocols required for one coherent
Language 1.0 surface. It is not the currently implemented Foundation library.

The implemented Seed Foundation contracts remain separately owned by
[Foundation bytes](Foundation-Bytes.md),
[byte construction](Foundation-Byte-Construction.md),
[byte ordering](Foundation-Byte-Ordering.md), and related current specifications
until the migration plan advances them.

This document owns semantic identities and behavior. The exact source grammar is
owned by the [Language 1.0 grammar](Windvale-Language-1.0-Grammar.md). A source
freeze must publish an exact signature-set identity for each required Foundation
module; no hash is assigned while these signatures remain candidate.

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
and task suspension because language syntax depends on their exact identities.
Recognition is by canonical module, type, major version, and signature-set
identity, never by an unqualified source name.

## Required modules

The candidate required modules are:

| Module | Contract |
| --- | --- |
| `Foundationˉoption` | Optional presence. |
| `Foundationˉresult` | Recoverable typed success/failure. |
| `Foundationˉnumeric` | Explicit conversions, parsing, and strict float helpers. |
| `Foundationˉordering` | Equality and deterministic total-order protocols. |
| `Foundationˉmemory` | Allocation domains, leases, limits, and failures. |
| `Foundationˉcollections` | Arrays, vectors, sequences, slices, maps, iterators, and arenas. |
| `Foundationˉbytes` | Immutable bytes, codecs, and bounded byte construction. |
| `Foundationˉtext` | Unicode text, rune iteration, formatting, and bounded text construction. |
| `Foundationˉresource` | Local-release protocol and owned-resource outcomes. |
| `Foundationˉtask` | Hosted task scopes, task handles, cancellation, and join outcomes. |
| `Foundationˉunsafe` | System-only raw address and foreign boundary primitives. |

A compiler claiming a complete profile supplies or binds every module required
by that profile. A package may select a compatible newer Foundation implementation
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

Required operations are:

- `Isˉpresent(borrow Option<T>) -> bool`;
- `Borrow(borrow Option<T>) -> Option<borrow T>`;
- `Borrowˉmut(borrow mut Option<T>) -> Option<borrow mut T>`;
- `Take(borrow mut Option<T>) -> Option<T>` for owned-capable `T`, leaving
  `Absent`; and
- explicitly named map or combine operations whose closure effects and output
  bounds remain visible.

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

Required operations include exact case tests, immutable and mutable payload
borrows, and explicitly named map operations. Mapping the failure side requires
one function `fn(E) -> F` and produces `Result<T, F>`; it is never selected
through overload inference.

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
    effects(memory.allocate, resource.acquire);
~~~

The parent reserves the child's maximum accounting authority until the child is
released. Splitting never grants access outside the parent and cannot increase
the combined maxima.

### Allocation lease

`Allocationˉlease` is a move-only child of one budget. It states maximum retained
bytes, current retained bytes, alignment ceiling, and generation. Collections
and arenas own leases rather than reaching an ambient allocator.

Creating a lease can fail recoverably. A lease maximum is an accounting bound,
not a promise that every later physical growth succeeds. An implementation may
offer a separately named `Reserveˉcommitted` operation that guarantees later
growth within the committed amount; its initial result must prove the reserved
capacity.

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

When a workload requires it, these separate names may exist:

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

## Owned vectors and immutable sequences

### Vector

`Vector<T>` is a move-owned contiguous collection. Construction receives:

- maximum items;
- maximum retained bytes;
- one owned allocation lease; and
- optional initial capacity not exceeding either maximum.

The vector records length, current capacity, maximum items, maximum retained
bytes, and lease generation. It never grows past either maximum.

Required operation families are:

- immutable and exclusive mutable indexed borrow;
- all-or-nothing append of one item;
- explicit prefix append of a slice;
- remove or replace with exact ownership return;
- reserve with typed allocation failure;
- immutable slice creation;
- exclusive mutable slice creation; and
- consuming freeze.

An all-or-nothing rejected append returns the original owned item and leaves
length, contents, capacity, and iteration unchanged. A successful append accepts
ownership exactly once.

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

## Typed arenas and handles

`Arena<T, N>` is a move-owned store of at most compile-time constant `N` nodes.
It owns one allocation lease and every live node.

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

## Bounded iterators

The `Iterator<T>` compile-time protocol exposes:

- exact or maximum remaining items;
- one `Next` operation returning `Option<T>` or a borrowed item;
- finite per-item work and retained-state bounds; and
- deterministic iteration order.

An iterator cannot represent an infinite source in Language 1.0. A filter or
flat-map operation derives an admitted maximum from its input and expansion
bound. A generator that cannot state a finite maximum is rejected.

`for` accepts only compiler-known arrays, sequences, slices, maps, or an exact
Iterator implementation.

## Bytes and byte builder

`bytes` is the shared immutable byte sequence. Each value retains current byte
length and an admitted maximum byte length; sharing preserves both and never
exposes backing capacity. `Bytesˉbuilder` is an owned specialized buffer with
maximum output bytes, current length, one allocation lease, and no implicit text
encoding.

Required builder operations include:

- append one byte;
- append an immutable byte slice;
- append an exact-width integer with named byte order;
- append a bounded sequence;
- reserve;
- truncate to an earlier valid length;
- consume into `bytes`; and
- inspect an immutable or exclusive mutable slice.

Each append is either all-or-nothing or explicitly prefix-admitting. A failed
all-or-nothing append leaves content and length unchanged.

Byte codecs validate complete input ranges before reading and use checked offset
arithmetic. No codec inherits native alignment or endianness.

## Text and text builder

`text` is a shared immutable Unicode scalar sequence with valid canonical UTF-8
interchange. Each value retains current UTF-8 byte length, rune count, and
admitted maximum UTF-8 byte length; sharing preserves those values and never
exposes backing capacity. `Textˉbuilder` is an owned bounded UTF-8 construction
buffer that never exposes malformed text as `text`.

Required operations include:

- append rune;
- append text or text slice;
- append validated UTF-8 bytes with typed failure;
- append one bounded formatted value;
- inspect rune count and byte count separately;
- consume into `text`; and
- clear or truncate only at a validated scalar boundary.

Text builder maximum output is measured in bytes. Rune count is separately
bounded by byte maximum. Failure leaves the builder unchanged unless an operation
is explicitly named as prefix-admitting.

## Formatting and interpolation

`Formatting<T>` is a compile-time protocol with:

- a function deriving or validating maximum UTF-8 output bytes from the value
  type, value maximums, and selected format;
- an append operation targeting a mutable text builder;
- invariant default numeric spelling;
- no allocation outside the supplied builder and lease;
- no capability, locale, clock, entropy, or provider access; and
- exact escaping behavior when a named format requires it.

Interpolation:

- evaluates fields from left to right once;
- computes or validates a complete maximum before output mutation;
- rejects before mutation when that maximum exceeds the builder or surrounding
  budget;
- appends literal and formatted fields in source order; and
- returns typed formatting or allocation failure rather than truncating.

User-visible locale, collation, pluralization, time zone, and cultural formatting
require explicit library data and are not the default Formatting protocol.

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
rights-reduced memory budget and optional deadline/cancellation providers. It
records one scope-exit policy selected by grammar: join, cancel then join, or fail
then join.

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
    Providerˉlost;
    Trapped(Identity: u32);
}
~~~

Trap identity is bounded diagnostic evidence, not a catchable source exception or
arbitrary stack trace.

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

For one async closure type `W` returning `Result<T, E>`, the semantic signature
is `Spawn(...) -> Result<Task<T, E>, Spawnˉfailure<W>>` with effects
`task.spawn` and any allocation effects named by the scope. A rejected spawn
returns the exact closure, including every moved capture, so the caller again
owns it. Once spawn returns a task handle, the child owns the captures and a
later task failure never rolls them back.

Join ordering for join-all is child creation order, not scheduler completion
order. An explicitly named completion-order operation may exist only with a
bounded completion queue and stable child identities.

Cancellation is cooperative and observable only at explicit checks, provider
operations, and `await` suspension points. Cancelling a scope requests
cancellation for all live children and still joins them before scope release.

No task can become detached, retain a borrowed value beyond the proven owner
lifetime, capture an optional-only capability, or silently replay an
indeterminate external mutation.

The scope-exit policies mean:

- `join` waits for every live child and retains their individually typed
  outcomes for explicit collection;
- `cancel_join` requests cancellation for every live child on every block exit
  and then waits for teardown; and
- `fail_join` joins normally until the block or one observed child fails or
  traps, then requests cancellation for remaining children and joins them before
  propagating the initiating outcome.

If retaining all outcomes would exceed the completion bound, spawn is rejected
before accepting the child that could exceed it. Scope exit does not allocate an
unbounded outcome list. A block `return`, `try` propagation, `break`, or
`continue` first applies the selected scope policy and local release, then
continues the transfer.

The paper corpus must confirm these signatures and the source spelling before
the task module receives a frozen signature identity.

## Unsafe Foundation

`Foundationˉunsafe` exists only in System modules. It provides opaque:

- `Rawˉaddress<Scope>`;
- `Foreignˉpointer<T, Abi>`;
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

Before source freeze:

1. every required module has a canonical major version and signature-set
   identity;
2. all generic and protocol signatures parse under the frozen grammar;
3. ownership on every success and failure path is explicit;
4. collection algorithms have stable worst-case bounds;
5. map ordering and arena generation behavior pass adversarial paper cases;
6. numeric conversion and parsing matrices are complete;
7. builder and interpolation maxima are computable in every admitted case;
8. resource release cannot discard a completion or body result;
9. task scope, capture, cancellation, join, and teardown pass the paper corpus;
10. unsafe values cannot enter Core or Hosted source; and
11. a responsibility matrix identifies ordinary source, compiler intrinsic,
    runtime, provider, and target-specific ownership for each operation.
