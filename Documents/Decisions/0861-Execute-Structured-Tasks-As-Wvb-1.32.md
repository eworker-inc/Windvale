# Decision 0861: Execute structured tasks as WVB 1.32

## Status

Accepted implementation contract for Language 1.0 Slice 7. The decision becomes
an implementation claim only when the source compiler, verifier, portable
runtime, sequential scheduler, and parallel-capable host evidence named below
all pass together.

This decision implements the source semantics frozen by
[Decision 0760](0760-Resolve-Language-1.0-Concurrent-Service-Findings.md). It
does not add detached work, threads to the source language, implicit
cancellation, completion-order observation, or a second task API.

## Context

WVB 1.31 can execute frame-owned callables with immutable plain captures. It
cannot describe an async callable's exact effects, transfer accepted work to a
task scope, represent an affine typed task handle, consume that handle at an
await point, or prove that all accepted children are joined before scope
release. Encoding those behaviors as ordinary library calls would hide the
ownership and suspension boundaries from the bytecode verifier.

Structured concurrency does not require one scheduling algorithm. A legal
single-threaded scheduler may run a child to completion during `Spawn`; a
parallel scheduler may retain accepted work and start it on another worker.
Both must preserve the same source-visible outcomes, handle consumption,
cancellation observations, creation-order join behavior, bounds, and teardown.

Implementation also exposed one contradiction in the frozen Foundation
identity. The accepted HTTP, concurrent-service, GUI, and accelerator workload
sources import the shared context from `Foundationˉoperation`, while the
machine-extractable registry accidentally declared that type inside
`Foundationˉtask` and omitted the operation module. Keeping the registry form
would make every hosted provider API depend on the task library and would make
the accepted source corpus impossible to bind to its registry.

## Decision

### Add one verified task extension

The canonical shared context is
`Foundationˉoperation.Operationˉcontext`. `Foundationˉtask` consumes and
produces that exact type but does not own or redeclare it. The Foundation
registry gains the missing Hosted module and its exact signature-set identity;
the task signature-set identity changes because its two context references are
now qualified. This is a correction of the frozen cross-document contradiction,
not a second context type or a compatibility alias.

WVB 1.32 extends the callable descriptor with its async flag, exact finite
language-effect mask, and exact capability-effect bitmap. Older callable
descriptors retain their 1.30/1.31 encoding and meaning.

WVB 1.32 adds compiler-recognized opaque `Taskˉscope`, `Operationˉcontext`, and
`Task<T, E>` identities plus the following instructions:

| Opcode | Instruction | Ownership result |
|---|---|---|
| `D6` | `task.scope.construct` | consumes one budget and returns the exact typed construction result |
| `D7` | `task.operation_context` | observes one live scope and returns its lifetime-bound Copy context |
| `D8` | `task.spawn` | accepts and owns the work exactly once or returns the exact work in the typed rejection |
| `D9` | `task.await` | consumes one exact typed handle and returns its `Taskˉoutcome<T, E>` |
| `DA` | `task.request_cancel` | closes the scope on the first request and reports the current live-child count |
| `DB` | `task.scope.exit` | applies `join`, `cancel_join`, or `fail_join`, joins every child, and consumes the scope |

The instruction operands name exact result and handle descriptors where their
generic shapes cannot be reconstructed from the operand stack alone. The
verifier rejects mismatched `T`/`E`, non-async work, a work result other than
`Result<T, E>`, missing effects, a second await, spawn after closing, scope or
context escape, scope release with an unconsumed child, a forged task value,
and every path that bypasses `task.scope.exit`.

`task.scope.exit` is emitted on fallthrough, `return`, `try` propagation,
`break`, and `continue` before outer resource release. The policy is part of the
instruction and cannot be supplied or changed by the runtime.

### Keep scheduling below the verified contract

The portable reference scheduler is deterministic and sequential. It may run
accepted work eagerly during `task.spawn`; completed work is still represented
by one affine handle, is observed only by consuming `task.await`, and remains
owned by its lexical scope until observation or teardown.

The parallel-capable hosted scheduler may retain accepted work. It must enforce
the same finite limits, preserve child creation identities, publish explicit
runtime generation loss or restart, and return joined outcomes in creation
order regardless of completion order. Parallel execution cannot change the
result bytes of a data-race-free program.

Scheduler choice is target/runtime policy. It is absent from source and does
not alter WVB verification.

### Bound the first executable profile

Each scope validates all eight `Taskˉlimits` fields before accepting work. The
portable profile additionally caps one scope at 64 accepted children, 64
runnable children, 64 retained completions, 1 MiB of retained task state,
1,000,000 work units, 64 child call frames, 64 timers, and 256 bounded
diagnostics. A requested lower application limit wins. Zero, internally
inconsistent, unaddressable, or larger limits fail before scheduling.

In the sequential WVB 1.32 profile, one work unit is one verified WVB
instruction dispatched for a spawned child. The spawn instruction establishes
the child's baseline; synthetic trap unwind and outcome construction consume no
additional child work unit. Exhaustion before dispatching the next instruction
becomes `Taskˉoutcome.Trapped(3011)`. The child root frame counts as call depth
one, and a call that would exceed the child-relative limit becomes
`Taskˉoutcome.Trapped(3004)`. These identities are task outcomes, not failures of
the parent invocation.

A completed aggregate result remains a garbage-collection root until its exact
handle is awaited or its origin scope tears down. The first task opcode family
does not create task-owned timers or diagnostic records, so the validated timer
and diagnostic limits begin at zero use. A future creation operation must
charge the relevant limit before publishing any state.

The first WVB 1.32 callable transport admits zero-argument async work with exact
finite effects and compiler-proved captures. Copy captures must be recursively
Copy. Move captures may be recursively owned and are transferred exactly once.
An immutable outer borrow is admitted only when verifier lifetime evidence ties
its owner to the same lexical scope; mutable outer borrows remain rejected.
Temporary exclusive provider borrows inside the child continuation remain
governed by the source suspension proof.

### Keep failures and cancellation exact

Construction and spawn retain the exact frozen `Result` and failure variants.
A rejected spawn returns the original work value and all of its captures.
Cancellation is an idempotent request, not an exception. Deadline wins at the
exact deadline tick. Task-runtime loss/restart remains separate from the
child's typed provider error `E`.

No scheduler may retry a child, replay an indeterminate mutation, synthesize a
capability, or convert an unobserved outcome into silent success.

### Current implementation checkpoint

The source compiler now admits the exact edition-1 task statement and semantic
operations, emits WVIR 1.21 and WVB 1.32, and preserves async/effect evidence in
the extended callable descriptor. The compiler-aligned verifier enforces the
canonical Foundation layouts and affine scope/handle state. The source-built
portable runner implements deterministic sequential execution, typed success,
stable child traps, exact work and child-call-depth exhaustion, retained
aggregate completion roots, and lexical scope teardown.

The runner transports this exact entry through execution-request major `6`.
The request contains only the fixed instruction/depth header and hosted WVB
module; the envelope requires WVB 1.32, hosted profile `2`, no declared
capabilities, and the exact two-parameter task entry before it creates the root
budget and operation context. Request major `5` remains the independent
source-file snapshot contract.

The 3,885-byte source-amendment manifest
`Documents/Project/Windvale-Language-1.0-Source-Amendment-0861-Candidate.txt`
at SHA-256
`edb0840333d34bbedc8e47bce5bad279ee9de832e4ce119b93b273a981521436`
binds the corrected Foundation operation/task registry and this migration
checkpoint while retaining the prior 251-input identity shape.

This checkpoint does not yet satisfy the parallel-capable Windows/Linux host
item below. It is therefore a completed sequential implementation checkpoint,
not the final cross-host Slice 7 qualification claim.

## Evidence required before completion

- a real edition-1 source fixture lowers `task scope`, semantic `Spawn`, and
  consuming `await` through WVIR and WVB 1.32;
- the verifier accepts the canonical module and rejects malformed descriptors,
  forged handles, double await, missing exit, mismatched outcomes, invalid
  policies, oversized limits, and illegal capture lifetimes;
- fallthrough, `return`, `try`, `break`, and `continue` all apply scope policy
  before outer resource release;
- the sequential scheduler proves typed success, typed child failure,
  cancellation, deadline, runtime generation loss/restart, trap identity,
  spawn rejection with work recovery, and deterministic creation-order join;
- one parallel-capable Windows host and one parallel-capable Linux host produce
  the same canonical observations while children finish in a different order;
- source, WIR, WVB, verifier, runtime, Foundation, editor, fixture registry, and
  migration evidence remain synchronized.

## Consequences

Structured concurrency becomes a bytecode-verifiable ownership boundary rather
than a naming convention. The source language stays independent of threads,
worker pools, and host scheduling APIs. A simple reference scheduler remains a
correct oracle, while faster hosts can execute in parallel without inventing a
second compiler or source surface.

WVB 1.32 is required only when a task instruction or extended async callable
descriptor is present. WVB 1.11 through 1.31 remain byte-for-byte unchanged.
Native backends that have not implemented the 1.32 task ABI reject the module
explicitly rather than silently lowering it as synchronous ordinary calls.

## Reconsideration triggers

Reconsider the transport representation if retained aggregate captures cannot
be reclaimed exactly once without a second heap, if paired-host evidence cannot
preserve deterministic observations, or if a real workload requires more than
the accepted fixed-handle surface. Preserve affine handles, lexical teardown,
exact effects, explicit suspension, bounded state, and the no-detach rule under
any replacement.
