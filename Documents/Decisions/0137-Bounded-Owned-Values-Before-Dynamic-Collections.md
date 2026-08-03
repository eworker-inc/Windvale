# Decision 0137: Bounded owned values before dynamic collections

- Date: 2026-08-03
- Status: Accepted direction; allocation syntax and runtime representation are not implemented
- Applies to: Future dynamic sequences, maps, builders, database pages, caches, and resource-backed values
- Retains: Immutable Windvale values, explicit capability boundaries, checked size arithmetic, current text/byte limits, and Stage 0 recovery implementations

## Context

The next useful language growth is being driven by database and systems code rather than by a general-purpose feature checklist. EWDB will need page buffers, typed sequences, indexes, transient builders, caches, and resources whose lifetime crosses ordinary helper calls. Adding collection syntax before defining who owns their storage would make host garbage collection, reference identity, accidental copying, or unbounded growth part of Windvale semantics.

Current `text` and `bytes` values already prove useful immutable bounded values, and ABI 21 proves frame-owned records with caller-owned return destinations. They do not yet form a complete contract for dynamically allocated values. The reference runtime may use managed host objects during Stage 0, but that implementation detail cannot define eventual native, Windvale OS, or portable behavior.

## Decision

- Define allocation and lifetime before adding a general dynamic collection surface.
- Classify runtime values as scalars, immutable borrowed views, uniquely owned mutable builders, published immutable owned values, or capability-backed resources. A representation may optimize these classes, but it must not erase their semantic distinction.
- Require every allocating operation to establish an exact charge or checked maximum charge before allocation. Enforce both a per-value limit and an execution or owner budget; failure traps before a partially published value becomes visible.
- Keep ordinary collection values immutable. Construction and bulk mutation occur through a uniquely owned bounded builder with an explicit capacity or maximum. Freezing a builder publishes one immutable value and invalidates further mutation through that builder.
- Do not make aliasing mutable. Copies of an immutable value may share backing, but observable behavior cannot depend on reference identity, host garbage collection, or copy-on-write heuristics.
- Make escaping ownership explicit in compiler IR and native ABI evidence. Frame-local temporaries may use frame or block storage; returned or longer-lived values require caller-owned destinations, transfer into a longer-lived owner, or another independently verified lifetime mechanism.
- Keep slices and iterators non-owning views whose lifetime cannot exceed their owner. Source syntax for borrows is deferred until a concrete API requires it; implementations must not silently allow an escaping view meanwhile.
- Give capability-backed resources deterministic close/release behavior and make use-after-close fail explicitly. Resource handles are not ordinary integers and are not serialized as ambient host handles.
- Use checked `u32` sizes for present bounded in-memory values. `u64` may identify persistent offsets, generations, counters, or file positions, but does not silently raise the existing 4 MiB byte or 1 MiB text limits.
- Require collection and allocation formats to state element layout, alignment, maximum count, maximum bytes, failure behavior, and malformed-input rules before implementation.
- Permit C# Stage 0 to use managed storage internally only when differential tests prove the same bounds, publication point, traps, and lifetime-visible behavior intended for native and Windvale OS implementations.

## Consequences

The first dynamic sequence should be a small typed bounded sequence plus builder, not an unbounded growable array. Deterministic maps and sets follow only after their ordering, hashing, capacity, and collision bounds are specified. Database work can then build pages, rows, and indexes on visible ownership rules rather than reverse-engineering lifetime semantics after the fact.

This direction does not select reference counting, tracing garbage collection, regions, or a single arena as the permanent physical mechanism. It selects semantic obligations that any mechanism must meet. A later implementation decision must choose the smallest mechanism that satisfies measured consumers on the reference runtime, native backend, and Windvale OS.

## Reconsider when

- A concrete builder or database cache cannot express its required lifetime under these owner classes.
- Concurrency introduces shared mutable state or cross-thread ownership transfer.
- Closures, async work, globals, or FFI allow references to escape lexical owners.
- Measured workloads justify a tracing collector or another reclamation mechanism without weakening deterministic bounds.
