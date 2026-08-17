# Decision 0760: Resolve the Language 1.0 concurrent-service findings

## Status

Accepted by the project owner on 2026-08-17. This decision refines
[Decision 0751](0751-Accept-Windvale-Language-1.0-Direction.md),
[Decision 0754](0754-Resolve-First-Language-1.0-Paper-Findings.md), and
[Decision 0759](0759-Resolve-Language-1.0-Http-Handler-Findings.md).
It accepts all seven general findings from the concurrent hosted-service paper
bundle.

It does not freeze edition 1, implement task scheduling or networking, select a
final network capability identity, require host threads, or authorize detached
work.

## Context

The sixth mandatory workload starts four bounded HTTP children in one lexical
task scope, collects typed outcomes in creation order, requests cooperative
cancellation after a policy trigger, joins every child, and may refresh one
generation-bound endpoint for one fresh request after a provider restart.

The earlier candidates had the right ownership direction but left the launcher
operation context disconnected from task construction, had no explicit source
cancellation request, conflated task-runtime loss with child-provider loss, and
allowed provider suspension to hide behind synchronous calls. Shared accepts
also needed an explicit generation-bound value, while the borrow rules needed to
distinguish a safe child-owned mutable borrow across `await` from an escaping
outer mutable capture.

## Decision

### Connect task scopes to one operation context

`Task.Construct` consumes its memory budget, borrows a valid parent
`Operationˉcontext`, validates limits, and returns `Taskˉscopeˉfailure` with exact
invalid-limit, allocation, stale-parent, or unavailable-runtime evidence.

`Task.Operationˉcontext(Scope)` returns a shared immutable Copy view derived from
that scope. It carries a deadline no later than the parent and the scope's one
cancellation identity/generation. It grants no capability, creates no timer, and
cannot outlive scope teardown.

### Add one explicit cooperative cancellation request

Accept:

```text
Task.Requestˉcancel(
    Scope: borrow mut Taskˉscope,
) -> Cancelˉrequestˉoutcome effects(task.cancel)
```

The first request closes the scope to new spawns, marks the scope-owned
cancellation generation, and reports live children. Later requests are
idempotent. Cancellation is observed cooperatively; it is not an asynchronous
exception and never replaces deterministic join.

### Separate task-runtime and child-provider failures

Replace the ambiguous payload-free task outcome with:

```text
Runtimeˉlost(Expectedˉgeneration: u64, Observedˉgeneration: u64)
Runtimeˉrestarted(Expectedˉgeneration: u64, Observedˉgeneration: u64)
```

These outcomes describe only the task runtime. Network, accelerator, or other
capability-provider loss remains in the child's typed error `E`, with its own
provider identity and generation evidence.

### Make potentially suspending provider operations explicit

Any provider operation that may suspend is `async`, carries `task.suspend`, and
must be called with `await`. Workload 5's accept, read, write, and refresh calls
therefore become explicitly asynchronous without changing their exact progress,
deadline, generation, partial-completion, indeterminate-mutation, or no-replay
semantics.

Bounded provider operations that are contractually non-suspending may remain
synchronous. An implementation may not hide a continuation under that spelling.

### Bind shared accepts to one generation-bound endpoint

Accept a Copy `Serviceˉendpoint` only for a capability interface that explicitly
permits concurrent shared accepts. It binds the approved service, exact
rights/limits, provider identity, and generation. Copying it duplicates neither
authority nor accounting.

Refresh receives that stale endpoint and the exact observed successor
generation. It may return only the same approved authority at that generation;
it is not discovery, replay, reconnection, or a new grant.

### Permit only child-owned exclusive borrows across suspension

A temporary exclusive provider argument may remain live across `await` when its
owner is in the same child continuation and no alias can execute. Storing or
returning that borrow, or capturing an outer mutable borrow into the spawned
child, remains invalid. Diagnostics identify the owner, borrow, suspension, and
required lifetime.

### Keep the version-1 task surface small

Five fixed named handles and creation-order `Await` calls are sufficient for the
workload. Language 1.0 adds no detach, task vector, completion stream,
`select`/race syntax, channel, actor, or thread primitive. A later measured
consumer may motivate a bounded library abstraction over the same scope and
handle contracts.

## Consequences

The concurrent hosted-service bundle becomes draft reviewed. Seven of eleven
workloads are now draft reviewed; workloads 7 through 10 remain.

The HTTP workload now uses explicit async provider calls and a typed endpoint.
The AI workload now borrows a launcher context, derives and copies one child
context, passes it to accelerator operations, preserves exact task-scope
construction failures, and keeps task-runtime generations separate from
accelerator-provider loss.

This decision adds no second cancellation system, ambient clock, catchable
exception, automatic retry, hidden reconnect, detached task, thread syntax,
HTTP-specific compiler behavior, accelerator-specific task behavior, or new
WIR/WVB opcode by itself.

## Reconsideration triggers

Reconsider context representation only if implementation evidence cannot enforce
its origin lifetime without retaining the task scope. Preserve one cancellation
generation, monotonic absolute deadlines, and non-forgeability.

Reconsider fixed handles only after a measured workload proves named handles
unusable. Any extension must remain bounded, scoped, deterministic where the
program promises ordering, and unable to detach implicitly.

Reconsider provider async spelling only for an operation contractually proven
not to suspend. Never hide continuation ownership, cancellation observation, or
an exclusive borrow across an unmarked suspension.
