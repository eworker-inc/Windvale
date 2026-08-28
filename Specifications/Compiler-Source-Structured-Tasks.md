# Windvale source structured-task lowering

## Status and scope

This specification defines the Language 1.0 Slice 7 compiler boundary selected
by [Decision 0861](../Documents/Decisions/0861-Execute-Structured-Tasks-As-Wvb-1.32.md).
It refines the frozen source and Foundation contracts; it does not replace
them. Until all required evidence in that decision passes, this document is an
implementation contract rather than a completion claim.

## Canonical identities

Only edition-1 source may use the task surface. The canonical module identity is
`Foundationˉtask`. Imported aliases are resolved before intrinsic recognition;
lookalike modules or declarations never acquire task semantics.

The compiler supplies the representation-hidden `Foundationˉtask.Taskˉscope`,
`Foundationˉoperation.Operationˉcontext`, and
`Foundationˉtask.Task<T, E>` types, and the semantic operations `Construct`,
`Operationˉcontext`, `Spawn`, `Await`, and `Requestˉcancel`. The task module
owns the ordinary public layouts `Taskˉlimits`, `Taskˉscopeˉfailure`,
`Spawnˉfailure<W>`, `Taskˉoutcome<T, E>`, and `Cancelˉrequestˉoutcome`.

Source cannot invoke a constructor, inspect a representation field, serialize,
or forge any compiler-supplied task identity.

## Source admission

The exact task-scope statement is:

~~~text
task scope Name = Expression policy join Block
task scope Name = Expression policy cancel_join Block
task scope Name = Expression policy fail_join Block
~~~

`Expression` must have exact type
`Result<Taskˉscope, Taskˉscopeˉfailure>` or a function-local exact error mapping
whose successful value is `Taskˉscope`. The binding is mutable only through the
named task operations, cannot be reassigned, cannot escape its block, and is
consumed by implicit scope-exit lowering.

`Spawn` requires an explicit mutable borrow of that live scope and one exact
zero-argument async callable `W`. If `W` returns `Result<T, E>`, its result is
exactly `Result<Task<T, E>, Spawnˉfailure<W>>`. Neither `T`, `E`, nor its finite
effect set is inferred from result context.

`Await` requires one owned `Task<T, E>` local. The `await` operator is valid only
in an async Hosted function, requires `task.suspend`, consumes the local exactly
once, and returns exactly `Taskˉoutcome<T, E>`.

`Requestˉcancel` requires an explicit mutable scope borrow, requires
`task.cancel`, and returns `Cancelˉrequestˉoutcome`. `Operationˉcontext` requires
an immutable scope borrow and produces a Copy value whose origin scope must
remain live.

## WVIR 1.21 operations

WVIR 1.21 appends these operations without changing earlier identities:

| Operation | Identity | Result |
|---:|---|---|
| 180 | `task.scope.construct` | exact construction `Result` |
| 181 | `task.operation_context` | `Operationˉcontext` |
| 182 | `task.spawn` | exact typed spawn `Result` |
| 183 | `task.await` | exact `Taskˉoutcome<T, E>` |
| 184 | `task.request_cancel` | `Cancelˉrequestˉoutcome` |
| 185 | `task.scope.exit` | `unit` |

Every operation carries its source span. Local-slot immediates identify affine
budget, scope, and handle operands; ordinary temporaries carry immutable limits,
context observations, and work values. The WVIR validator reconstructs every
canonical Foundation layout and rejects a shape or mode mismatch before WVB
encoding.

Task bindings use one distinct binding kind. Generic local release walks task
scopes and ordinary `using` resources in reverse declaration order. A transfer
out of nested blocks emits `task.scope.exit` for each exited task scope, then
releases ordinary resources that were declared outside that scope.

## WVB 1.32 mapping

WVIR operations 180 through 185 map to WVB opcodes `D6` through `DB` in order.
The WVB type section retains exact generic type indices for task handles,
outcomes, and rejection values. Extended callable descriptors retain the async
flag and exact effect evidence used by `task.spawn` verification.

The WVB verifier treats a task scope, accepted work, and task handle as affine.
Control-flow merge requires identical task ownership state. Return is legal only
after every live lexical scope has executed `task.scope.exit`; a handle is legal
only in its origin scope and is invalid after `task.await` consumes it.

The bytecode does not encode a thread, worker count, scheduling order, or host
API. Those are runtime choices below the verified contract.

## Bounds

- lexical task-scope nesting: 32;
- task scopes per function: 256;
- task operations per module: 65,536;
- callable parameters: zero for version-1 spawned work;
- captures per work value: 64;
- type/effect/capture evidence: 128 KiB per catalog;
- verifier ownership states: bounded by existing block and local limits;
- runtime limits: the lower of validated `Taskˉlimits` and the selected profile
  ceilings in Decision 0861.

Every limit failure is deterministic and occurs before an ownership transfer
that the failure result does not explicitly return.

## Sequential queued reference execution

The portable WVB 1.32 runner is the correctness oracle, not a promise of one
thread. `Task.Spawn` records accepted work beneath its lexical scope and returns
the typed handle before entering the child. The reference scheduler executes
one queued child at a time when an await requires progress. Within each
consecutive group of four task slots it selects lanes `3, 1, 0, 2`, while
source-level awaits still consume handles and construct reports in creation
order. This policy makes completion order independently observable to the
runtime without changing portable source or bytecode semantics.

One child work unit is one verified WVB instruction dispatched after the spawn
baseline. If no unit remains before the next dispatch, the child completes as
`Taskˉoutcome.Trapped(3011)`; synthetic trap unwind and outcome construction do
not consume another child unit.

The child root frame has relative call depth one. Entering a nested call that
would exceed `Maximumˉcallˉdepth` completes that child as
`Taskˉoutcome.Trapped(3004)`. A child arithmetic trap retains the ordinary
stable runtime identity; signed division by zero is `3007`. The parent can
observe these identities only by consuming the exact handle or by scope exit.

The runtime retains completed scalar or aggregate results by task identity.
Every retained aggregate value is included in garbage-collection roots until
`Await` consumes its handle or scope teardown destroys the completion. Task
state, completion count, and retained bytes remain bounded independently of the
ordinary aggregate arena.

Before accepting a spawn, the queued WVB 1.32 profile reserves the exact
maximum scheduler state that the child can retain in this implementation:

- the 56-byte queued-child descriptor, which covers the later 40-byte active
  continuation and reserved 8-byte terminal-result cell;
- the complete child local frame; and
- the newly suspended parent frame, including its resume and function words.

The reservation is checked before work ownership moves. If it exceeds the
scope's remaining `Maximumˉretainedˉbytes`, spawn leaves task state unchanged
and returns `Memoryˉfailure` with `Allocationˉreason.Budgetˉexhausted`, the
exact requested and available byte counts, and the original work value. An
accepted reservation remains charged across completion and is released only
when `Await` consumes the handle or bounded scope teardown removes it.

This limit accounts task-runtime continuation and outcome state. Heap storage
reachable through a capture or outcome remains charged to its explicit memory
budget and is kept live by the task root; it is not charged a second time to
the scope's scheduler-state limit. The internal fixed task-state encoding is
version 3. Its bounded 56-byte header stores the root operation-context
identity and generation, clock generation, absolute deadline, and expected and
observed task-runtime generations. Every active task record continues to store
one exact 64-bit retained-state reservation.

Scope construction accepts either the exact live root context or a context
derived from a still-live scope. A generation mismatch returns
`Taskˉscopeˉfailure.Staleˉcontext(Expected, Observed)`; an absent or replaced
task runtime returns `Runtimeˉunavailable(Expected, Observed)`; exhausted fixed
scope storage returns the exact allocation failure. Each refusal occurs before
budget ownership or runtime state changes.

## Task environment and terminal observation

A runnable child observes its environment only at a named cooperative runtime
point. The observation validates the task identity and exact clock generation,
then applies one deterministic priority order:

1. `Tick >= Deadline` produces `Taskˉoutcome.Deadlineˉreached`;
2. before the deadline, observed task-runtime generation zero produces
   `Runtimeˉlost(Expected)`;
3. a different nonzero task-runtime generation produces
   `Runtimeˉrestarted(Expected, Observed)`;
4. otherwise a cancellation request on the origin scope produces `Cancelled`;
5. otherwise the child remains runnable and task state is unchanged.

Deadline and cancellation carry no generation evidence. Runtime loss carries
only the nonzero expected generation. Runtime restart carries nonzero, distinct
expected and observed generations. Invalid task identity, completed state, or
clock generation leaves the state byte-identical and cannot manufacture an
outcome. A terminal observation uses the ordinary reserved completion slot and
retained-memory charge until consuming `Await` or scope teardown releases it.

The sequential command-line runner constructs a non-expiring default
environment with context, clock, and task-runtime generation 1. Its explicit
`--task-environment` mode injects exact context, clock, deadline, admitted and
observed task-runtime generations, and observation tick through
execution-request major `6`, minor `1`; malformed values are rejected before
module execution. A permanent four-child cancellation fixture proves that four
accepted sibling handles coexist before child execution and that cancellation
can terminate every queued child. Child-provider generations, an externally
observable completion-order report, and parallel worker state remain later
Slice 7 checkpoints and do not change source, WVIR 1.21, or WVB 1.32.

`Maximumˉtimers` and `Maximumˉdiagnostics` are validated with the other six
limits. WVB 1.32's first six task instructions do not create a task-owned timer
or diagnostic, so both counters remain zero in this profile. Later operations
must define and charge creation before mutation; this specification does not
reserve an ambient timer or logging service.

A parallel-capable hosted scheduler may replace this sequential queue policy
only after it
reproduces the same accepted/rejected transfers, exact outcomes, creation-order
joins, resource accounting, cancellation observations, and deterministic
data-race-free result bytes on Windows and Linux. Scheduler selection remains a
runtime policy and never changes source, WVIR, or WVB.
