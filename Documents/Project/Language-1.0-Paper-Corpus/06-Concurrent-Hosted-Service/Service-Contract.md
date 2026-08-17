# Workload 6 task, cancellation, and restart contract

## One task domain

`Taskˉscope` owns every accepted child, its continuation storage, and its one
retained terminal outcome. `Task<T,E>` is a move-only observation handle, not a
second owner of the child. Consuming a handle through `Await` cannot detach the
child. Leaving the lexical block applies `cancel_join` before any enclosing
return or error propagation continues.

The reference service accepts at most five children: four initial requests and
one fresh post-refresh request. `Maximumˉcompleted` is also at least five, so a
provider must reject a spawn before capture acceptance if it cannot retain the
eventual outcome. It may not drop an early completion to make space.

## Context derivation

The launcher supplies one opaque parent `Operationˉcontext`. Task construction
derives one child view with:

- the same or an earlier absolute monotonic deadline;
- a fresh nonzero cancellation identity and generation owned by the scope;
- the admitted parent clock and provider span; and
- a lifetime no longer than the task scope.

`Task.Operationˉcontext(Scope)` returns a Copy shared view, not cancellation
authority. A child may copy it because the scope joins that child before the
view becomes stale. Application source still cannot construct, inspect, extend,
or request cancellation through the context itself.

`Task.Requestˉcancel(Scope)` is the only source-visible request in this bundle.
The first call closes the scope to new spawns and marks all live child contexts;
later calls are idempotent. It does not interrupt code asynchronously. A child
observes cancellation at `await`, a provider call, or a named explicit check.
Scope exit still joins children that have not yet observed it.

## Deterministic observation policy

The coordinator spawns indices 0 through 3, then consumes their handles in that
order. A completion queue may receive index 3 first, but source does not observe
it until indices 0 through 2 have been collected. The first creation-ordered
outcome that is a non-restart handler failure, cancellation, deadline,
task-runtime loss/restart, or trap requests cancellation. The report retains
that exact index.

Provider-restart failures are collected without immediately cancelling their
siblings because all old endpoint copies are already generation-bound. The
scope still joins them before it considers refresh. Conflicting observed
successor generations fail closed.

## Provider generations

`Serviceˉendpoint` is a Copy, shared-call endpoint only because the capability
interface explicitly admits concurrent accepts. It binds exact rights, service
identity, provider identity, and nonzero provider generation. Copying it does
not widen authority.

`network.service.accept.Refresh` accepts the stale endpoint, the scope context,
and the one consistently observed successor generation. It may return only the
same approved interface, service identity, rights, and limits at that exact
generation. It cannot discover another service or increase authority.

Refresh does not replay work. Old children retain their exact result, including
an indeterminate write. The optional fifth child accepts a new connection and a
new request. A second restart is a terminal typed service failure in this
bounded profile.

## Async provider meaning

`Acceptˉone`, `Read`, `Write`, and `Refresh` are asynchronous semantic calls in
this workload and require `await` plus `task.suspend`. Their workload-5 progress
meaning is unchanged: read publishes one exact initialized prefix; write
publishes one exact positive locally accepted prefix; rejection proves zero
current-call progress; and an indeterminate mutation has no safe replay point.

The temporary exclusive stream/buffer arguments of one awaited provider call
remain valid because their owners live in the same child task frame and have no
alias during suspension. This does not permit storing a borrow in a task or
capturing an outer mutable borrow into a spawned child.
