# Decision 0868: queue structured-task children before await

## Status

Accepted on 2026-08-28.

## Context

The first WVB 1.32 runner entered every accepted child directly from
`Task.Spawn`. The call did not return its typed handle until the child had
completed. That execution order was deterministic, but it could never represent
two simultaneously live siblings. Cancellation requested after four source
spawns was therefore too late: all four children had already returned.

Workload 6 requires accepted children 0 through 3 to coexist, permits completion
order `3, 1, 0, 2`, and still requires consuming awaits and reports in creation
order. This is a runtime scheduling boundary. It does not justify source syntax,
WVIR, or WVB changes.

The runner's large execution function was also already close to the native
backend's 2,048-physical-local limit. Adding scheduler state initially raised it
to 2,060 locals, so the change required a cohesive extraction rather than a
higher backend limit.

## Decision

The portable WVB 1.32 runner remains the deterministic single-thread
correctness oracle, but `Task.Spawn` no longer executes accepted work eagerly.

For every accepted child, the runner:

1. validates the callable target and materializes its captured child locals;
2. reserves the exact retained scheduler maximum before ownership moves;
3. publishes the typed task handle and successful spawn result immediately;
4. appends one bounded 56-byte queue descriptor plus the child locals; and
5. advances the parent beyond the spawn instruction without entering the child.

The descriptor retains the exact task and origin-scope tokens, function and code
identity, local length, remaining work, call-depth maximum, parent depth, and
deterministic task-slot lane. The complete queue is limited to 1 MiB, each child
local frame is limited to 4,096 bytes, and the existing lower scope limits still
cap accepted, runnable, completed, and retained children.

When `Await` observes a still-runnable handle, it selects queued work from that
handle's exact origin scope. Four-lane groups use reference priority
`3, 1, 0, 2`; unrelated nested scopes cannot steal each other's children. The
runner records the current parent at the await instruction, installs the child
continuation, and executes it. Child completion or a typed terminal observation
returns to the same await instruction without placing an intermediate value on
the parent stack. The repeated await then consumes the completed task and
constructs its exact typed outcome.

Cancellation, deadline, and task-runtime generation are observed before the
first instruction of each selected child and at every later cooperative child
dispatch. A cancellation requested after the source has accepted four children
can therefore terminate all four queued children without executing their work.

The retained reservation is now the 56-byte queued-child descriptor, the
complete child locals, and the newly suspended parent frame. It covers the
smaller 40-byte active-child continuation and the reserved terminal cell after
dispatch, so no later scheduling transition can require more scheduler state
than spawn admitted.

Success-response serialization is extracted from the runner loop into one
focused function. The resulting source-built runner `Main` uses 1,981 physical
locals, preserving the native backend's existing 2,048-cell limit.

## Evidence

`Structured-Task-Four-Child-Cancellation-Executable.wv` compiles to a
deterministic 6,049-byte WVB at SHA-256
`b4d9c67cee803da4fb53ef21a57ccbdf9ecc410c54c369262f3c2187599df88c`.
It returned `0` through the eager runner and returns `42` through the queued
runner. The same queued runner also returns `42` for the prior scalar success,
retained aggregate result, work-limit, call-depth-limit, trap, and retained-
memory fixtures.

The source-built runner used for the focused check contains 227 functions,
424,375 code bytes, and 476,206 module bytes. Native staging, linking,
transport, and Windows hosted packaging completed without raising any format,
function, local, or plan limit.

The complete focused owner passes 160 cases, including 28 structured-task, 46
task-runtime, 17 task-environment, and 69 malformed-input cases. The complete
registry remains 114 owners and advances to 5,557 cases at 18,556 LF-only bytes
and SHA-256
`ee5bbbb30567148d9f1352e68f6bea56bce9162a3fec0a08ae86048e8ccc3d8d`.

## Consequences

- Multiple accepted sibling handles now exist before any child must complete.
- Await remains consuming and source reports remain creation ordered.
- Cancellation can affect accepted queued work before its first instruction.
- Nested task scopes select only their own queue entries.
- Source syntax, Foundation signatures, WVIR 1.21, and WVB 1.32 remain unchanged.
- The portable oracle still runs one child at a time. This decision is not
  parallel-capable Windows/Linux qualification.
- Provider-generation operations and an externally observable completion-order
  workload remain required before Slice 7 closes.

## Reconsideration triggers

Reconsider the representation if a qualified scheduler needs resumable child
continuations beyond the current instruction boundary, if a task local frame
can exceed 4,096 bytes, if task-slot reuse must carry a monotonic scheduling
identity, or if a parallel adapter cannot reproduce the same typed outcomes,
bounds, and creation-ordered result bytes.
