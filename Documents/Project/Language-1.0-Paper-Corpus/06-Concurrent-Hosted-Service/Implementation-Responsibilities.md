# Workload 6 implementation responsibilities

## Responsibility map

| Boundary | Future owner | Required work | Special compiler feature? |
| --- | --- | --- | --- |
| Task construction/failure | Foundation task/runtime | Validate limits/context and reserve bounded child, queue, continuation, timer, work, and diagnostic state. | Ordinary result/record lowering. |
| Scope-derived context | Foundation operation/task | Create a non-forgeable child cancellation generation with no later deadline. | Lifetime/escape checking, no new expression. |
| Spawn captures | Compiler ownership analysis/runtime | Check explicit copy/move/borrow modes; return exact closure on pre-accept rejection. | Closure environment plus affine-state analysis. |
| Await/outcomes | Runtime scheduler | Consume handle once; retain exact typed outcome and runtime generations. | Async continuation lowering. |
| Cancellation | Task runtime/provider adapters | Close scope to spawn, mark views, observe cooperatively, and always join. | Named call/effect, not async exception. |
| Async stream operations | Network provider adapters | Preserve workload-5 exact progress while suspending explicitly. | Ordinary async capability call. |
| Endpoint refresh | Launcher/network provider | Prove same approved service/rights/limits at exact successor generation. | Canonical capability import only. |
| HTTP child | Workload-5 application/library | Accept endpoint parameter and await accept/read/write. | No HTTP special case. |
| Verification | Focused Language 1.0 task/network owners | Execute capture, ordering, queue, cancellation, deadline, trap, restart, no-replay, teardown, and cross-host cases. | No broad qualification substitute. |

## Required diagnostics

The compiler must diagnose an implicit capture, illegal copy of affine state,
use after move, mutable capture that can outlive its borrow, scope-context
escape, await outside async scope, missing task/capability effect, unconsumed
task handle, detached child attempt, and provider call made without `await`.
Diagnostics are bounded and identify the capture, owner, suspension, and scope
where applicable.

## Implementation sequence after source freeze

1. Freeze operation-context and task signatures together.
2. Implement a deterministic single-thread scheduler oracle with all limits.
3. Add async continuation and capture ownership checking to the shared compiler.
4. Convert workload-5 provider calls/signature to the frozen async endpoint
   form while preserving its exact transcript oracle.
5. Implement Windows and Linux adapters over the same semantic provider API.
6. Add alternative sequential/interleaved/parallel schedules and compare
   creation-ordered reports byte for byte.
7. Measure source/WIR/WVB/native sizes, compile/runtime time, peak memory,
   continuation bytes, queue maxima, cancellation latency in work units, and
   teardown bounds.

No implementation step adds threads to source semantics. Host threads are one
possible scheduler implementation beneath the same task contract.
