# Language 1.0 paper workload 6: concurrent hosted service

## Status

Draft reviewed after the project owner accepted all seven findings on
2026-08-17 under
[Decision 0760](../../../Decisions/0760-Resolve-Language-1.0-Concurrent-Service-Findings.md).
This is paper Language 1.0 source. Current Seed tools do not accept it, and it
neither implements a service nor freezes edition 1.

## Result

Three modules express one bounded service coordinator that:

1. derives one child operation context from a launcher-supplied parent context;
2. starts four HTTP request handlers in one lexical task scope;
3. immutably borrows common limits and moves a separate 128 KiB memory budget
   into each child;
4. awaits and records the four typed outcomes in creation order;
5. requests cooperative cancellation after the first observed non-restart
   failure, deadline, task-runtime failure, or contained trap;
6. joins every child even after cancellation;
7. reconciles consistent evidence for one service-provider restart;
8. refreshes only the same approved, rights-limited endpoint generation; and
9. accepts one fresh recovery request without replaying any prior request or
   uncertain response mutation.

Scheduler completion order may vary. The report field order, cancellation
trigger, restart evidence, and cleanup do not.

## Source modules

| Module | Responsibility |
| --- | --- |
| `Concurrentˉserviceˉtypes` | Limits, child outcomes, restart evidence, report, and service failures. |
| `Concurrentˉserviceˉpolicy` | Pure task-outcome classification and deterministic restart reconciliation. |
| `Concurrentˉserviceˉapplication` | Budget split, scope construction, explicit captures, spawn/await/cancel, endpoint refresh, and cleanup. |

The policy module is Hosted because it names Hosted task and HTTP outcome types,
but it performs no capability operation. The application requires only
`network.service.accept` version 1.

## Evidence index

- [task, cancellation, and restart contract](Service-Contract.md)
- [package and resource plan](Package-Plan.md)
- [semantic review](Semantic-Review.md)
- [rejected and boundary cases](Rejected-Cases.md)
- [expected outcomes](Expected-Outcomes.md)
- [implementation responsibilities](Implementation-Responsibilities.md)
- [review findings](Review-Findings.md)

## Acceptance answer

The accepted candidate direction can express the workload without a second concurrency
syntax, detached tasks, exceptions, shared mutable state, implicit retry, or a
completion-order race. Decision 0760 accepts the workload's exact task-scope
construction failure, scope-derived operation context, explicit cancellation
request, unambiguous task-runtime generation outcomes, asynchronous provider
calls, generation-bound endpoint/refresh, awaited-borrow rule, and intentionally
small fixed-handle surface as normative-candidate inputs.

## Nonclaims

This is not an event loop, thread API, listener, load balancer, supervisor,
automatic provider restart, request replay system, HTTP implementation, or
parallel-execution requirement. A conforming scheduler may run the children
sequentially while preserving the same semantics.
