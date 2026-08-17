# Decision 0758: Resolve the Language 1.0 compiler-front-end findings

## Status

Accepted by the project owner on 2026-08-17. This decision refines
[Decision 0751](0751-Accept-Windvale-Language-1.0-Direction.md),
[Decision 0754](0754-Resolve-First-Language-1.0-Paper-Findings.md), and
[Decision 0757](0757-Resolve-Language-1.0-Database-Transaction-Findings.md).
It accepts all six owner findings from the compiler-front-end paper bundle.

It does not freeze edition 1, change the current Seed compiler, assign final
Foundation signatures, select WVB changes, or claim implementation.

## Context

The fourth mandatory paper workload decodes strict UTF-8, lexes positioned
tokens, parses recursive expressions into a typed arena, publishes phase models
immutably, binds names through an ordered map, emits bounded diagnostics, and
constructs deterministic bytes. It uses empty vectors, an empty arena, and an
empty map before the first source item exists.

Workload 3 could bootstrap collections from its required first input. Making a
compiler carry `Option<Vector<T>>`, `Option<Arena<T>>`, and `Option<Map<K,V>>`
through every phase merely to satisfy generic inference would obscure real
invariants and mishandle valid zero-declaration state. This is the second
complete workload that meets Decision 0754's explicit reconsideration trigger.

Review also found that workload 3's provisional map value borrow has two
borrowed parameters without syntax identifying which owns the result. The
compiler workload needs exact lifetime provenance, immutable arena publication,
source scalar positions, deterministic diagnostic saturation, and complete
reserved vector/builder operations.

## Decision

### Explicit generic calls for named declarations

Accept this edition-1 expression form:

```text
Explicitˉgenericˉcall ::= Qualifiedˉsourceˉname "::"
                          Typeˉarguments
                          "(" [ Callˉarguments ] ")"
```

Examples:

```text
Collections.Arenaˉconstruct::<Types.Node>(...)
Collections.Mapˉconstruct::<text, Types.Binding>(...)
```

The qualified name must resolve to one generic function declaration before
argument checking. The suffix supplies every type and compile-time constant
parameter exactly once in declaration order. Type arguments must occupy type
parameters and exact constant expressions must occupy constant parameters.
Partial lists, defaults, placeholders, parameter names, result-context repair,
protocol search, conversions, and overload selection are absent.

After substitution, ordinary parameter, effect, ownership, protocol, and
admission checks run. An explicit and an argument-derived call that produce the
same canonical substitution have the same generic instance identity.

`::` is required. A bare `Name<T>(...)` remains invalid, so relational `<`/`>`
parsing is unchanged. The explicit form applies only to a qualified named
declaration, not an arbitrary function-valued expression or dynamic dispatch.
Ordinary generic calls continue using Decision 0754's argument-derived rule.

This deliberately narrow syntax resolves real empty-owner construction without
introducing general result inference.

### Reserved vectors and empty bounded owners

Accept these Foundation version-1 shapes:

```text
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

export fn Vectorˉlength<T>(
    Vector: borrow Vector<T>,
) -> u64 effects();

export fn Vectorˉfreeze<T>(
    Vector: Vector<T>,
) -> Sequence<T> effects();

export fn Mapˉconstruct<K, V>(
    Budget: Memoryˉbudget,
    Maximumˉitems: u64,
) -> Result<Map<K, V>, Collectionˉfailure>
    effects(memory.allocate)
    where K: Ordering<K>;

export fn Arenaˉconstruct<T>(
    Budget: Memoryˉbudget,
    Maximumˉnodes: u64,
) -> Result<Arena<T>, Collectionˉfailure>
    effects(memory.allocate);
```

These calls require explicit generic syntax because their value arguments supply
no generic evidence. Positive maxima and budget bounds are validated before
allocation. Reserved vector construction commits representation/capacity for
the complete item maximum; later append cannot fail for physical growth, but
capacity rejection returns the original value unchanged. Freeze consumes the
vector and performs no fallible compaction.

Map/arena construction creates an empty move-owned owner. It never allocates a
node/item merely to establish type. Workload 3's atomic first-item constructors
remain valid convenience calls.

### Unambiguous rank-based map borrows

Supersede Decision 0757's `Mapˉborrowˉexisting(Map borrow, Key borrow)` with:

```text
export fn Mapˉfindˉrank<K, V>(
    Map: borrow Map<K, V>,
    Key: borrow K,
) -> Option<u64> effects()
    where K: Ordering<K>;

export fn Mapˉborrowˉat<K, V>(
    Map: borrow Map<K, V>,
    Index: u64,
) -> borrow V effects();
```

The owned optional rank is absent or the key's exact ascending canonical rank.
`Mapˉkeyˉat` and `Mapˉborrowˉat` require the same
`Index < Mapˉlength(Map)` precondition. Each borrowed result has exactly one
borrowed parameter, the map owner, so lifetime provenance is visible without
named lifetime syntax. No exclusive mutation may intervene while a rank-derived
borrow remains live.

`Mapˉcontains` may remain a Boolean convenience but is not a lifetime proof.
The workload 3 source is revised to rank/borrow in the same change.

Arena validation may still borrow a handle because it returns owned unit/failure.
`Arenaˉborrowˉvalidated` and its immutable counterpart take `Handle<T>` by value;
handles are Copy. Their direct borrow is therefore tied only to the arena.

When an expression has type `borrow T` or `borrow mut T` and an exact by-value
`T` is required, edition 1 may read through that borrow only when `T` is Copy or
shared immutable. The result is the ordinary semantic copy; a shared result may
retain its backing and admitted charge. This does not move from the owner, clone
an owned value, change the borrow's lifetime, or create a general implicit
conversion. It makes a borrowed map value containing a Copy handle usable as the
by-value validation token without a dereference/pointer surface.

The same observation rule applies when a pattern matches a borrowed aggregate:
Copy and shared-immutable fields bind as their ordinary semantic copies; an
owned field can bind only as a borrow tied to the aggregate. Pattern matching
never moves out of a borrowed value.

### Immutable typed arena publication

Accept:

```text
export fn Arenaˉlength<T>(
    Arena: borrow Arena<T>,
) -> u64 effects();

export fn Arenaˉfreeze<T>(
    Arena: Arena<T>,
) -> Immutableˉarena<T> effects();

export fn Immutableˉarenaˉlength<T>(
    Arena: borrow Immutableˉarena<T>,
) -> u64 effects();

export fn Immutableˉarenaˉvalidate<T>(
    Arena: borrow Immutableˉarena<T>,
    Handle: borrow Handle<T>,
) -> Result<unit, Collectionˉfailure> effects();

export fn Immutableˉarenaˉborrowˉvalidated<T>(
    Arena: borrow Immutableˉarena<T>,
    Handle: Handle<T>,
) -> borrow T effects();
```

Freeze consumes the mutable arena, requires no allocation or compaction,
invalidates every mutable borrow, and preserves arena identity, positive maximum,
live slot indices, exact generations, handle equality/liveness, slot order, and
retained charge. The immutable arena has no insert/remove/mutable-borrow surface.
Destroying it invalidates its handles as ordinary owner destruction.

### Exact source-text primitives and positions

Accept strict reserved decode and scalar observations:

```text
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

export fn Decodeˉutf8ˉreserved(
    Budget: Memoryˉbudget,
    Value: borrow bytes,
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
```

Decode rejects byte/rune limits before excess allocation, validates shortest
UTF-8 and Unicode scalar range, and transfers its reserved budget into shared
immutable text on success. Physical allocation failure remains distinct from a
source decode failure. Either failure consumes/releases the child budget and
constructs no partial text. `Runeˉat` is checked and traps on a violated proved
index. Width returns 1–4. Shared range validates checked rune geometry and may
share the same backing/retained charge; its new descriptor has the exact range
byte/rune maximum and no mutable alias.

Compiler positions use zero-based byte/rune offsets and one-based scalar
line/column. LF alone advances line/reset column; CR and tab each advance one
scalar column. No normalization, UTF-16 indexing, grapheme/display width, locale,
or host newline conversion occurs.

### Deterministic diagnostic saturation and exact integer appends

For bounded compiler diagnostic sinks, accept the paper policy: reserve one last
slot, retain at most maximum-minus-one ordinary diagnostics, place exactly one
`Diagnosticˉlimit` at the next issue, then retain no later issue. The maximum is
positive and at least two for this workload. Diagnostic order is phase encounter
order; any diagnostic suppresses artifact publication.

Accept byte-builder all-or-nothing operations:

```text
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
```

Each proves complete resulting length with checked arithmetic before mutation,
then appends the exact byte width/order. Limit failure leaves builder bytes and
length unchanged. No host alignment or endianness enters output.

## Consequences

The compiler-front-end bundle becomes draft reviewed. Five of eleven workloads
are now draft reviewed and six remain.

Edition 1 gains one narrowly delimited grammar form because two complete
workloads proved empty generic construction cannot remain clear under argument
inference alone. The new syntax does not add overloads, partial inference,
default arguments, or result guessing.

Rank-based map observation corrects the workload 3 lifetime ambiguity. Immutable
arena freeze gives compilers, GUIs, and retained models a phase-publication shape
without pointer exposure or mutation. Exact UTF-8/source position and builder
operations are reusable compiler/library contracts.

These paper calls should lower through ordinary generics, collections, variants,
borrows, and bytes. This decision adds no compiler-specific WIR opcode, second
compiler, capability, macro, exception, tracing GC, unsafe escape, or current
Seed implementation.

## Reconsideration triggers

Reconsider explicit generic calls only if later workloads prove the qualified
named form cannot express an exact generic declaration/constant instance. Keep
full arity, deterministic identity, `::` disambiguation, and no result-context or
overload search.

Reconsider rank-based map borrows if measured general workloads require a
compiler-known borrowed iterator. Any replacement must identify one lifetime
owner without storable ambiguous borrows.

Reconsider immutable arena freeze only if a target cannot preserve handle
identity without fallible compaction. Return the original owner on any proposed
fallible freeze; never silently invalidate handles.

Reconsider scalar columns only for a separately named user-display diagnostic
layer. Canonical compiler identity and machine spans remain byte/rune based.

Reconsider saturation when a later compiler workload proves multiple severities
or related-diagnostic groups require a different bounded policy. Preserve stable
ordering, an explicit truncation marker, and a hard retained-state/work limit.
