# Workload 6 rejected and boundary cases

## Implicit capture

```text
let Work = async fn []() { return Endpoint.Accept(); };
```

Reject because `Endpoint` is referenced but absent from the explicit capture
list. A compiler may suggest `copy Endpoint`; it may not infer authority.

## Capturing a mutable outer borrow

```text
let Work = async fn [borrow mut Sharedˉbuffer]() { await Fill(Sharedˉbuffer); };
Task.Spawn(Scope: borrow mut Scope, Work: Work);
```

Reject because a spawned child may retain the exclusive borrow until scope
teardown. Move an owned child buffer instead.

## Using a moved capture after accepted spawn

```text
let Work = async fn [move Childˉbudget]() { ... };
let Handle = try Task.Spawn(Scope: borrow mut Scope, Work: Work);
Use(Childˉbudget);
```

Reject use after move. If spawn rejects, the typed `Spawnˉfailure<W>` returns
the exact closure and capture; accepted spawn never does.

## Dropping a task handle to detach

```text
let _ = try Task.Spawn(Scope: borrow mut Scope, Work: Work);
return Result.Valid { Value: () };
```

Reject an unconsumed affine handle in ordinary source. Even recovery cleanup
cannot detach the child: the lexical scope still cancels/joins it.

## Escaping a scope-derived context

```text
task scope Scope = ... policy join {
    return Task.Operationˉcontext(Scope: borrow Scope);
}
```

Reject because the returned view could outlive the scope-owned cancellation
generation. Provider use of serialized or forged stale values also fails
closed.

## Spawning after cancellation

After the first `Task.Requestˉcancel`, every later spawn returns
`Scopeˉclosing(Work)` before accepting captures. Cancellation does not create a
race in which a child sometimes owns the closure.

## Full completion queue

With `Maximumˉcompleted=4`, the fifth possible child is rejected before capture
acceptance even if the first four are still running. The runtime cannot assume
an await will free a slot before all four complete.

## Scheduler completion order

If children complete 3, 1, 2, 0, the report remains 0, 1, 2, 3. A scheduler may
not substitute completion order for source observation order. A future named
completion-order API would require stable child identities and a separately
bounded queue.

## Cancellation/deadline race

At the absolute deadline tick, `Deadlineˉreached` wins. Before the deadline, a
previously requested cancellation yields `Cancelled` at the next observation
point. Neither is translated to a handler failure or exception.

## Refresh without restart evidence

Reject refresh unless the stale endpoint and an exact observed successor
generation came from a provider-restart result. The operation cannot act as
service discovery or authority acquisition.

## Replay after provider restart

Do not rerun an old handler after restart, including one whose write result is
indeterminate. The recovery child accepts a new request only. Replaying could
duplicate externally visible response bytes or application mutations.

## Provider/runtime ambiguity

Reject a task API that returns payload-free `Providerˉlost`. The caller could
not tell whether the scheduler provider failed or the child's service provider
returned an application error, and it would lose exact generations.
