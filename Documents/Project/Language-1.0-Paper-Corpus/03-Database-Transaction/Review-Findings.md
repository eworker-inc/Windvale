# Workload 3 review findings

## Status

All five first-author findings are resolved by the project owner through
[Decision 0757](../../../Decisions/0757-Resolve-Language-1.0-Database-Transaction-Findings.md)
and the later lifetime/generic-construction refinement in
[Decision 0758](../../../Decisions/0758-Resolve-Language-1.0-Compiler-Front-End-Findings.md).
The workload is draft reviewed. Capability/schema signature-set digests remain
source-freeze inputs, not current implementation identities.

## Finding 1: arena capacity should not be a type argument

The candidate Foundation described `Arena<T, N>`. The paper parser knows its
three-node maximum at launch/build planning, but edition 1 cannot infer constant
`N` from a plain integer argument and never uses result-context inference.
Decision 0758 now permits a full-arity explicit suffix, but spelling each runtime
capacity as a type argument would still multiply generic instances without
changing handle semantics.

Resolution: use `Arena<T>` with an immutable positive `Maximumˉnodes` stored in
the move-owned arena. Capacity remains explicit, validated before allocation,
and included in admission/resource evidence. Handles retain arena identity,
slot, and generation. This is an accepted Foundation refinement, not an
unbounded runtime collection.

## Finding 2: nonempty generic construction needs a standard pattern

An empty `Map<K,V>` or `Arena<T>` has no explicit typed argument from which
Decision 0754 can infer parameters. This workload always rejects zero fields and
therefore has a real first value, key, and handle.

Resolution: accept ownership-preserving `Arenaˉconstructˉwithˉfirst` and
`Mapˉconstructˉwithˉfirst`. Their generic parameters derive only from explicit
typed arguments and the first element is inserted atomically. Failure returns
the original owned input(s). No grammar or inference change is made. General
empty generic construction remains deliberately absent; workload 4 must supply
the second complete pressure case before Decision 0754's reconsideration trigger
can fire. Workload 4 supplied that second complete case, so Decision 0758 now
also admits full-arity `Qualifiedˉfunction::<...>(...)` calls and exact empty
map/arena/vector construction. The first-item constructors remain useful
ownership-preserving conveniences.

## Finding 3: recoverable borrowed lookup needs a nonescaping shape

`Option<borrow V>` and `Result<borrow T, E>` would place a borrowed value inside
a variant payload, which edition 1 deliberately forbids without named lifetime
grammar. Yet missing keys and stale handles must remain recoverable.

Resolution: accept a two-step observation pattern. Decision 0758 refines the
provisional key-borrow operation because both the map and key were competing
lifetime sources: `Mapˉfindˉrank` returns an owned optional canonical rank, then
`Mapˉborrowˉat` borrows through the map alone. `Arenaˉvalidate` precedes
`Arenaˉborrowˉvalidated`; the borrow operation copies its `Handle<T>` value, so
its result is likewise tied only to the arena. The borrow checker prevents
intervening mutation. Skipping either proof is a terminal precondition trap, not
unchecked memory access. `Mapˉkeyˉat` shares the canonical rank contract without
a stored borrowed iterator.

## Finding 4: typed rows need a schema adapter, not reflection

Letting a generic database API discover record fields would add ambient
reflection and make wire identity depend on compiler layout. Passing packed
bytes throughout the application would defeat the workload's typed-state goal.

Resolution: bind one application-specific typed capability adapter to an exact
schema identity/version and explicit codec. Safe source passes `Customerˉrow`;
the adapter validates/encodes the three named fields under the existing typed-row
contracts. Schema digests and field maxima are build inputs. No source record
layout, host object layout, or reflection registry is observable.

## Finding 5: commit, release, and recovery are separate

The file-copy decision established that `using` is only local release. A
transaction sharpens the rule because implicit commit, implicit rollback result,
or exception-style precedence would hide the most important mutation.

Resolution: stage changes transaction-local state; source calls `Commit`
explicitly once. Commit returns completed, conflict, rejected, or indeterminate.
`using` then releases the session without changing that outcome. Leaving the
body before commit discards staged state locally and is never reported as
success. An indeterminate commit returns a stable identity and generation
evidence, forbids replay, and permits only local release. A separately launched
fresh recovery session reopens, validates, selects exactly one durable state,
and reports whether the old or new row exists. The same independent reopen also
qualifies a reported successful commit.

## Review conclusion

The source satisfies workload 3 without classes, exceptions, reflection,
garbage collection, SQL syntax, automatic retry, pointer exposure, database WIR
operations, or a second compiler. Ownership and uncertainty are readable from
the application module alone. Later workloads may broaden shared capabilities,
collections, and cancellation but may not weaken the accepted capacity,
ownership-return, explicit-commit, or fresh-recovery rules silently.
