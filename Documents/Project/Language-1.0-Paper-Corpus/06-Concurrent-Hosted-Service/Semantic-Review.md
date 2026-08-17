# Workload 6 semantic review

## Ownership inventory

| Value | Class | Owner and lifetime |
| --- | --- | --- |
| parent operation context | shared opaque borrow | launcher owns; used only to construct scope |
| child operation context | Copy scope-derived view | invalidated after scope teardown |
| service endpoint | Copy shared-call reference | exact approved rights and provider generation |
| root and child budgets | move-owned accounting | coordinator, then task scope or one child |
| task scope | move-owned resource | lexical `task scope` statement |
| async closures | move-owned until spawn accepts | rejection returns exact captures |
| task handles | move-only | consumed once in creation order by `Await` |
| request streams | move-only resources | workload-5 child `using` block |
| reports, limits, generation evidence | ordinary values | coordinator/result state |

No stream, task handle, memory budget, closure, mutable borrow, or capability
authority is stored globally or detached.

## Capture walkthrough

Each child closure says `copy Endpoint`, `copy Context`, `borrow Limits`, and
`move BudgetN`. Spawn first checks scope, child, runnable, completion, work, and
memory capacity. Before acceptance, the caller remains owner of the complete
closure. After acceptance, the child owns the moved budget even if it later
fails or traps. Task failure never rolls captures back.

The Copy endpoint is legal only because this interface declares concurrent
shared accepts. The Copy context is observation state, not cancellation or
provider authority. The immutable limits borrow is safe because the owner
outlives the lexical task scope and cannot be mutated until every child joins.

## Suspension and borrows

The child owns its budget, stream, and byte buffer in its continuation frame.
An awaited provider call may temporarily hold exclusive borrows of that owned
state, because no alias or move is available until the call completes. By
contrast, a closure capture `[borrow mut Outer]` is rejected: the child could
remain live until scope teardown while the outer mutable borrower expects to
resume, and concurrent siblings could not receive the same exclusive value.

## Failure-domain separation

| Domain | Source representation | Consequence |
| --- | --- | --- |
| HTTP/application | `Taskˉoutcome.Failure(Handlerˉfailure)` | retain typed handler evidence |
| service provider loss/restart | nested `Handlerˉfailure` with exact generations/progress | no replay; optional one refresh for future accept |
| cooperative cancellation | `Taskˉoutcome.Cancelled` | request remaining cancellation and join |
| absolute deadline | `Taskˉoutcome.Deadlineˉreached` | distinct from cancellation/provider failure |
| task runtime loss/restart | exact `Runtimeˉlost` / `Runtimeˉrestarted` generations | cancel/join; never mislabel service provider |
| contained child trap | bounded `Trapped(Identity)` | cancel/join; no catchable exception/stack trace |

## Ordering and cleanup

Four handles are awaited explicitly as 0, 1, 2, 3. This proves the required
ordering without prematurely adding a general task collection or completion
stream. Early `try` propagation remains inside `cancel_join`; it cannot bypass
child teardown. Normal exit sees every accepted handle consumed, so the exit
policy has no live child to cancel.

## Provider restart walkthrough

1. Old endpoint generation `G` is copied into all four initial closures.
2. One or more calls report `Providerˉrestarted(Expected=G, Observed=G+1)`.
3. Every old child is awaited and its exact outcome retained.
4. Observed successor generations must agree.
5. Refresh proves the same capability/service/rights/limits and returns `G+1`.
6. One new child accepts fresh work at `G+1`.
7. No old request, read suffix, or write suffix is replayed.
8. Another restart exceeds the profile and returns a typed failure.

This is survival, not transparent retry.
