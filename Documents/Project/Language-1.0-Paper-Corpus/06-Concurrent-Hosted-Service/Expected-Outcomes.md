# Workload 6 expected semantic outcomes

## Canonical all-success transcript

Four children are accepted in order 0 through 3. The scheduler completes them
3, 1, 0, 2. Await still returns reports in fields `First`, `Second`, `Third`,
`Fourth`; `Cancellationˉrequested=false`, `Recovery=Absent`, and
`Refreshedˉgeneration=Absent`. All five reserved budgets and the scope/root
accounting are released by return.

## Policy-trigger transcript

Children 0 through 3 are accepted. Child 0 succeeds, child 1 produces a typed
non-restart handler failure, and children 2 and 3 are still live. The parent:

1. records child 0;
2. records child 1;
3. requests cancellation exactly once with `Cancellationˉafterˉindex=1`;
4. awaits children 2 and 3 in that order; and
5. reports their actual terminal values, commonly `Cancelled`.

No child outlives return. A late child success remains success rather than being
rewritten to cancellation.

## Contained trap transcript

A trap in child 0 produces `Trapped(Identity)` with bounded identity evidence.
The parent requests cancellation for remaining children and joins them. No
source exception is caught, no arbitrary stack trace is retained, and the
moved child budget is not returned to application logic.

## Single provider restart transcript

Initial endpoint generation 41 is copied to all four children. They report a
mixture of normal completions and `Providerˉrestarted(Expected=41,
Observed=42)`. Any indeterminate write remains indeterminate in its child
outcome. After all four joins, refresh returns the same endpoint rights at
generation 42. One recovery child accepts a fresh request and succeeds.

The final report records `Refreshedˉgeneration=Present(42)` and one recovery
outcome. It never claims that any failed old request completed.

## Conflicting or repeated restart

- Observed generations 42 and 43 in the initial wave produce
  `Conflictingˉrestartˉevidence`; scope teardown still joins all children.
- A restart of the recovery child produces `Secondˉproviderˉrestart`; the
  bounded profile does not loop or raise its restart limit.
- Provider loss without a successor generation is retained as a handler failure
  and triggers cancellation; it cannot justify refresh.

## Spawn rejection

Scope-closing, child-count, completion-queue, and memory rejection all happen
before execution or capture acceptance. The returned closure contains the exact
moved handler budget. Mapping the rejection releases that closure locally;
previously accepted children are cancelled/joined by scope exit.

## Deadline and parent cancellation

A parent deadline/cancellation is narrowed into the scope context. Already
running provider awaits observe it cooperatively. At the exact deadline,
deadline wins. Pre-dispatch observation proves zero provider progress;
post-dispatch response mutation may remain indeterminate. Await and teardown are
still bounded by the scope's reserved recovery resources.
