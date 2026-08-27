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

## Sequential reference execution

The portable WVB 1.32 runner is the correctness oracle, not a promise of one
thread. It executes accepted work sequentially and records each child beneath
its lexical scope. One child work unit is one verified WVB instruction
dispatched after the spawn baseline. If no unit remains before the next
dispatch, the child completes as `Taskˉoutcome.Trapped(3011)`; synthetic trap
unwind and outcome construction do not consume another child unit.

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

`Maximumˉtimers` and `Maximumˉdiagnostics` are validated with the other six
limits. WVB 1.32's first six task instructions do not create a task-owned timer
or diagnostic, so both counters remain zero in this profile. Later operations
must define and charge creation before mutation; this specification does not
reserve an ambient timer or logging service.

A parallel-capable hosted scheduler may replace eager execution only after it
reproduces the same accepted/rejected transfers, exact outcomes, creation-order
joins, resource accounting, cancellation observations, and deterministic
data-race-free result bytes on Windows and Linux. Scheduler selection remains a
runtime policy and never changes source, WVIR, or WVB.
