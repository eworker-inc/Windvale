# Decision 0757: Resolve the Language 1.0 database-transaction findings

## Status

Accepted by the project owner on 2026-08-17. This decision refines
[Decision 0751](0751-Accept-Windvale-Language-1.0-Direction.md),
[Decision 0752](0752-Complete-Language-1.0-Collection-And-Package-Data-Boundaries.md),
[Decision 0754](0754-Resolve-First-Language-1.0-Paper-Findings.md), and
[Decision 0756](0756-Resolve-Language-1.0-File-Copy-Findings.md). It accepts the
five owner resolutions from the database-transaction paper bundle.

It does not freeze source edition 1, change Windvale Seed, publish a general
database catalog interface, select an on-disk format, or claim a provider
implementation on any target.

## Context

The third mandatory paper workload parses three named fields, stores typed
values in one arena, relates schema keys to generation-checked handles through
one ordered map, performs one typed database lookup/update, commits explicitly,
and reports missing data, invalid schema, conflict, rejection, provider failure,
or uncertainty. A separate fresh session supplies reopen/recovery evidence.

The complete source needs no class, reflection, exception, garbage collector,
SQL syntax, native pointer, automatic retry, database-specific WIR operation, or
parallel compiler. Review exposed five shared decisions:

1. an arena capacity encoded as generic constant `N` cannot be inferred from a
   plain runtime maximum and needlessly multiplies otherwise identical types;
2. nonempty generic collections need a callable ownership-safe construction
   pattern under argument-only generic resolution;
3. optional/recoverable collection lookup cannot hide borrows inside `Option` or
   `Result`, because edition 1 forbids borrowed aggregate fields;
4. typed database source needs an explicit stable schema adapter rather than
   packed application bytes or ambient reflection; and
5. commit completion, local release, and uncertainty recovery are three
   different operations and ownership domains.

## Decision

### Runtime-bounded typed arenas

Replace the Foundation candidate `Arena<T, N>` with `Arena<T>`. Construction
receives an immutable positive `Maximumˉnodes: u64`, validates it before
allocation, stores it in the move-owned arena, and charges all retained state to
the supplied memory budget. Admission may impose a smaller target or launcher
maximum, but no implementation may grow beyond the accepted owner value.

This changes where the bound is represented, not whether a bound exists.
`Handle<T>` remains Copy, non-owning, opaque, arena-identity/index/generation
checked, nonserializable by default, and nonconvertible to an integer. Removal
increments generation; generation wrap retires the slot. A stale handle never
aliases a later node.

Runtime capacity avoids one native/WIR specialization per arbitrary node count
while leaving compilers and launchers enough exact resource evidence for
admission.

### First-item construction under argument-only generic resolution

Accept these version-1 construction shapes:

```text
export record Arenaˉseed<T> {
    Owner: Arena<T>;
    First: Handle<T>;
}

export record Arenaˉinsertˉfailure<T> {
    Error: Collectionˉfailure;
    Value: T;
}

export record Mapˉinsertˉfailure<K, V> {
    Error: Collectionˉfailure;
    Key: K;
    Value: V;
}

export fn Arenaˉconstructˉwithˉfirst<T>(
    Budget: Memoryˉbudget,
    Maximumˉnodes: u64,
    First: T,
) -> Result<Arenaˉseed<T>, Arenaˉinsertˉfailure<T>>
    effects(memory.allocate);

export fn Mapˉconstructˉwithˉfirst<K, V>(
    Budget: Memoryˉbudget,
    Maximumˉitems: u64,
    Key: K,
    Value: V,
) -> Result<Map<K, V>, Mapˉinsertˉfailure<K, V>>
    effects(memory.allocate)
    where K: Ordering<K>;
```

All generic parameters derive structurally from explicit typed value arguments.
Each successful call transfers its budget and inserts the first item atomically.
Failure releases any partial allocation and returns the original owned item, or
key and value, unchanged. Maxima must be positive; zero is `Invalidˉlimit`.

Accept ordinary `Arenaˉinsert` and `Mapˉinsert` with the same ownership-return
failure rule. Insertion performs the capacity check before node allocation or
ordering work beyond the published comparison limit. Duplicate map insertion
leaves the map unchanged and returns the input key/value.

This decision does not add result-context inference or an explicit generic-call
suffix. It does not yet accept a general empty generic constructor. The compiler
front-end workload is the next complete pressure case under Decision 0754's
reconsideration trigger.

### Checked collection observation without borrowed aggregates

Accept these exact observation shapes for the workload:

```text
export fn Mapˉlength<K, V>(
    Map: borrow Map<K, V>,
) -> u64 effects();

export fn Mapˉcontains<K, V>(
    Map: borrow Map<K, V>,
    Key: borrow K,
) -> bool effects()
    where K: Ordering<K>;

export fn Mapˉborrowˉexisting<K, V>(
    Map: borrow Map<K, V>,
    Key: borrow K,
) -> borrow V effects()
    where K: Ordering<K>;

export fn Mapˉkeyˉat<K, V>(
    Map: borrow Map<K, V>,
    Index: u64,
) -> borrow K effects();

export fn Arenaˉvalidate<T>(
    Arena: borrow Arena<T>,
    Handle: borrow Handle<T>,
) -> Result<unit, Collectionˉfailure> effects();

export fn Arenaˉborrowˉvalidated<T>(
    Arena: borrow Arena<T>,
    Handle: borrow Handle<T>,
) -> borrow T effects();
```

`Mapˉkeyˉat` uses ascending canonical order and requires
`Index < Mapˉlength`. `Mapˉborrowˉexisting` requires a key proven present in the
same immutable map state. `Arenaˉborrowˉvalidated` requires a successful
`Arenaˉvalidate` for the same arena/handle state. Violating either precondition
is a terminal programming trap; untrusted keys and handles use the recoverable
check first. No intervening exclusive mutation can occur while the immutable
owner borrow remains live, so the proof cannot become stale.

This two-step design keeps failure recoverable while returning direct borrowed
values tied to one owner. It does not introduce `Option<borrow V>`,
`Result<borrow T,E>`, named lifetime syntax, a storable reference record, or an
unchecked memory access. Later workloads may propose a compiler-known borrowed
iterator only if its lifetime and nonstorage rules are equally exact.

`Collectionˉfailure` is a nominal bounded failure record whose kind distinguishes
invalid limit, allocation, capacity, duplicate, comparison limit, wrong arena,
slot range, vacancy, stale generation, and retired slot. It carries only the
relevant maximum/observed/generation evidence and never exposes layout or an
address.

### Explicit typed schema adapters

A database capability may expose an application/domain-specific typed record
only when its build binding names an exact collection identity, schema identity
and version, field names/order/types/maxima, codec identity, maximum encoded
bytes, and canonical schema digest. The adapter validates untrusted encoded data
before constructing the safe record and encodes named fields explicitly.

Source record layout, compiler field offsets, host object metadata, reflection,
provider pages, and storage-engine types are not schema. A generic database
library may remain byte-oriented internally, but application source need not
pack or unpack its domain state at every call.

For this paper workload, `database.customer.transaction` version 1 exposes one
typed `Customerˉrow` under schema `windvale.paper.customer.row.v1`. The semantic
shape and authority split are accepted; final capability and schema signature
identities remain provisional until later service/package workloads and source
freeze.

### Explicit commit and fresh-session recovery

A transaction is move-only and locally releasable. Staging changes only
transaction-local/provider-staged state. Source invokes `Commit` explicitly at
most once. Commit returns one of:

- `Committed`, with stable commit identity and generation/sequence evidence;
- `Conflict`, with expected and observed revision and proof this transaction did
  not commit;
- `Rejected`, with proof this transaction did not commit; or
- `Indeterminate`, with stable commit identity and generation evidence but no
  replayable completion claim.

`using` then performs local release exactly once. It never commits, retries,
flushes, recovers, or replaces the body/commit result. Leaving the body before
commit discards the staged value and cannot produce committed success.

After `Indeterminate`, the same transaction admits release only. Source must not
retry, query, switch provider, or assume either outcome. A separately bound fresh
session reopens and recovers the database, validates its durable state, and
selects exactly the old or new generation. The same independent reopen qualifies
a reported `Committed` result. Corruption or ambiguous selection is provider
qualification failure.

## Consequences

The database transaction becomes a draft-reviewed corpus row. Four of eleven
workloads are now draft reviewed and seven remain.

Runtime-bounded arenas reduce generic/code-size pressure without weakening
resource bounds. First-item construction keeps Decision 0754 intact for this
workload. Checked two-step observation avoids adding lifetime syntax and gives
untrusted handle/key code a recoverable path. The explicit schema adapter keeps
typed application state and stable serialization compatible without reflection.

The commit rule extends Decision 0756's release/completion separation to
transactions and adds the fresh recovery-session requirement. Existing Windvale
database storage contracts are the first implementation oracle but do not define
Language 1.0 semantics or force their physical format into source.

No grammar production, implicit conversion, catchable exception, pointer,
unbounded collection, database-specific WIR instruction, or current
implementation changes through this decision.

## Reconsideration triggers

Reconsider runtime arena capacity only if a mandatory workload proves a
compile-time capacity is required for sound layout or a target that cannot
support a runtime maximum. Preserve explicit positive capacity, admission, and
generation safety.

Reconsider argument-only construction after the compiler-front-end workload if
it supplies the second complete case that cannot express safe empty generic
state with a first item or clear non-generic constructor. Any replacement must
remain unique, bounded, reproducible, and independent of overload/protocol/result
guessing.

Reconsider two-step observation if mutation can occur between proof and borrow
without the borrow checker rejecting it, or if two complete workloads prove the
pattern unacceptably repetitive. Do not solve it with storable borrowed variant
payloads lacking exact lifetimes.

Reconsider the typed capability adapter if package/schema evolution needs a
shared generic form. Preserve explicit schema identity, validation, maxima,
codec determinism, and the absence of ambient reflection.

Reconsider transaction outcomes only if a provider can prove a stronger
idempotency or recovery contract. Never collapse uncertainty into rejection,
retry an uncertain mutation blindly, or let local release imply semantic commit.
