# Workload 11 semantic review

## Review standing

The owner review is complete under
[Decision 0754](../../../Decisions/0754-Resolve-First-Language-1.0-Paper-Findings.md).
This record describes the source as written and the accepted general
clarifications; it does not convert paper dependencies into implemented or
frozen behavior.

## Part metadata

| Module | Profile | Platform | Authority | Direct capability requirements |
| --- | --- | --- | --- | --- |
| `Inferenceˉtypes` | Core | Windows, Linux, Windvale | Library | None |
| `Inferenceˉpackage` | Core | Windows, Linux, Windvale | Library | None |
| `Inferenceˉdecode` | Core | Windows, Linux, Windvale | Library | None |
| `Inferenceˉreference` | Core | Windows, Linux, Windvale | Library | None |
| `Inferenceˉaccelerated` | Hosted | Windows, Linux, Windvale | Library | `accelerator.catalog`, `.execute`, `.kernel`, `.memory` v1 |
| `Inferenceˉapplication` | Hosted | Windows, Linux, Windvale | Application | Same exact transitive four-capability set |
| `Inferenceˉkernel` | Core | software accelerator v1, SPIR-V accelerator v1 | Library/kernel part | None; target mapping is package/build evidence, not host authority |

No module is System profile. The host source contains no unsafe block, FFI,
native handle, raw address, DMA value, platform driver API, dynamic import, or
filesystem operation. The kernel target scopes are canonical target-interface
registry keys over the structured build descriptor, not host platform aliases or
capability grants.

## Value and ownership inventory

| Value | Class | Owner and transfer |
| --- | --- | --- |
| Four package-data values | Shared immutable | Application resource domain; copying into command records shares the same admitted values and does not duplicate payload charge. |
| `Inputˉvector`, `Modelˉplan`, `Outputˉvector`, limits, enums, and scalar failures | Copy aggregates | Ordinary lexical source; the async closure explicitly copies its model plan. |
| Provider identity text and completed output bytes | Shared immutable | Returned terminal record, then final result / decoder; no backing identity or mutation is visible. |
| Provider `Selection` | Copy witness | Produced by catalog selection; it names a generation but grants no allocation or execution authority by itself. |
| Root `Memoryˉbudget` | Move-owned resource | Supplied by the launcher to `Inferenceˉapplication.Run`, then consumed by task-scope construction. |
| `Taskˉscope` | Move-owned resource | Bound by `task scope`; cancel-and-join policy owns the child and outcome retention. |
| Async work closure | Owned function value | Explicitly copies `Model`; accepted exactly once by spawn or returned inside spawn failure and then released. |
| Task handle | Move-owned scoped handle | Returned by spawn, consumed by `Await`, and unable to detach or outlive the scope. |
| Accelerator session | Move-owned resource | Outermost accelerator `using`; owns provider-generation and session ceilings. |
| Residency | Move-owned resource | Nested inside session; owns five device slots and their 320-byte charge. |
| Command batch | Move-owned resource | Nested inside residency; owns six sealed command records and shared upload-source references. |
| Submission | Move-owned resource | Innermost `using`; retains exact batch/residency/session generations until terminal completion. |
| Tensor-view descriptors | Copy geometry | Source constructs slot/format/shape/stride/range/rights values; each use requires a live borrowed residency and materializes only a provider-validated generation-bound command view. |
| Kernel lane inputs/results | Copy f32 values | The target interface invokes two independent pure scalar calls and collects each return at its matching lane. |

No stored user record or variant contains a borrow. No borrow crosses the task
boundary: the closure owns its copied model, and the accelerator function borrows
that owner only within the child. Provider resource borrows remain inside the
lexically nested `using` scopes and across `Wait` only under the live owners whose
generations the submission retains.

## Capability and effect closure

The exact external authority closure is:

```text
accelerator.catalog version 1
accelerator.execute version 1
accelerator.kernel version 1
accelerator.memory version 1
```

The root application redeclares the complete set. No dependency adds an optional
or hidden capability. Exact function effects additionally expose
`memory.allocate`, `resource.acquire`, `resource.release`, `task.spawn`, and
`task.suspend` where applicable.

`Mapˉaccelerator<T>` has an empty effect set because it consumes and translates
an already returned nominal result; generic use does not hide the effect of the
capability call passed into it. The async closure repeats the four capability
effects in its function type. Its direct lexical value capture is only
`copy Model`; module-bound singleton capability calls remain visible through the
called function's effect set and the importing module's requirement closure.

Authority intentionally absent from the closure includes filesystem, network,
external-model inference, native-device extension, profiling, display, graphics,
physical assignment, partition, passthrough, and arbitrary kernel loading.
Provider selection therefore admits only software or paravirtual shared mode;
hardware partition and exclusive passthrough are explicitly false requirements.

## Evaluation and deterministic ordering

- Package bytes validate before provider discovery or allocation.
- Function arguments, record fields, arithmetic operands, and command additions
  evaluate left to right once.
- Signed-I4 weights unpack row-major and low nibble first.
- The strict oracle accumulates columns zero through three with separate f32
  operations and no contraction.
- The submitted graph has one dependency order and one readback.
- Spawn accepts one child; task results collect in creation order, which is
  trivially the single child.
- Provider scheduling and wall-clock completion order are not source evidence.
- Output decoding is index zero then index one; comparison and class selection
  use the same order.

## Bounds

| Dimension | Maximum |
| --- | ---: |
| Package resource objects / retained bytes | 4 / 96 |
| Input / output elements | 4 / 2 |
| Packed weights | 8 logical I4 values / 4 bytes |
| Host-domain bytes | 16,384 |
| Pinned host bytes | 64 |
| Device logical / charged bytes | 44 / 320 |
| Device buffers | 5 |
| Provider queues / submissions | 1 / 1 |
| Batch commands / admitted slots | 6 / 8 |
| Accelerator / enclosing task work units | 64 / 256 |
| Task children / runnable / completed | 1 / 1 / 1 |
| Timers | 1 |
| Runtime call depth | 16 |
| Diagnostic records / bytes | 16 / 2,048 |
| Kernel lanes / loop iterations / barriers / atomics | 2 / 0 / 0 / 0 |

Every shape and byte product is checked before allocation or command mutation.
No collection, queue, recursion path, diagnostic list, task graph, provider
operation, or kernel loop is semantically unbounded.

## Recoverable failure families

| Family | Earliest owner | Observable result |
| --- | --- | --- |
| Missing, duplicate, oversized, wrong-type, or digest-mismatched package binding | Package constructor | Reject before application publication. |
| Invalid tokenizer length, magic, shape, tokens, or f16 class | Core decoder | `Invalidˉtokenizer` with bounded offset/rule. |
| Invalid model length, magic, shape, format, finite values, scale, or budgets | Core decoder | `Invalidˉmodel` with bounded offset/rule. |
| Invalid weight length or index | Core decoder | `Invalidˉweights` with bounded offset/rule. |
| Root/task budget unavailable | Foundation memory/task | `Taskˉscopeˉrejected` with normalized reason and bytes. |
| Scope closing, child, queue, or spawn-memory limit | Foundation task | `Taskˉspawnˉrejected` with one closed reason. |
| Unsupported formats/mode/provider, insufficient device budget, stale generation, invalid command/range/shape/alias/kernel, or provider-side limit | Accelerator capability | `Acceleratorˉrejected` with kind, stage, generation, requested, and limit. |
| Cooperative cancellation | Task scope / provider wait | `Cancelled` only after terminal containment. |
| Deadline | Task scope / provider wait | `Deadlineˉreached`. |
| Provider loss or reset | Provider | `Providerˉlost` with the last proved generation where available. |
| Terminal identity/generation/mode differs from selected description | Application/provider evidence validator | `Providerˉevidenceˉmismatch`; completed output is not accepted. |
| Contained task trap | Foundation task | `Taskˉtrapped` with bounded identity. |
| Mis-sized or non-finite completed output | Output decoder | `Invalidˉproviderˉoutput`. |
| Differential mismatch | Application comparator | `Outputˉmismatch` with index, values, exact error, and allowed error. |

There is no catchable general exception and no implicit error conversion.
`Mapˉaccelerator<T>`, scope construction, and spawn mapping are named adapters.

## Normal completion walkthrough

1. The launcher transfers one admitted root memory budget into `Run`.
2. Package-bound input, tokenizer, metadata, and weights are validated; source
   constructs only Copy/shared values.
3. The strict reference output is computed before any provider work.
4. Task-scope construction consumes the budget and publishes one cancel-and-join
   scope.
5. Spawn either returns the closure unchanged on rejection or accepts its copied
   model and publishes one task handle.
6. The child selects and describes a provider generation before use, opens one
   session, atomically admits five residency slots, and creates one batch.
7. Six all-or-nothing commands are added and the batch is submitted once.
8. `Wait` suspends while the live submission retains every required generation.
9. Completed output bytes are validated and converted into an ordinary result.
10. Submission, batch, residency, and session release in reverse order. Device
    and provider charges return before the child result is published.
11. `Await` consumes the task handle; the parent compares both scores and returns
    one final result.
12. The scope joins the already terminal child, releases its retained outcome,
    and returns the complete task budget.

## Failure and cancellation walkthroughs

### Rejection before submit

Decoder, selection, session, residency, or command-add failure propagates through
typed `Result`. Every successfully constructed inner resource releases before its
outer owner. A failed residency admits no slot; a failed command addition leaves
the batch unchanged. No device command has executed.

### Submit rejection

Submit proves zero device acceptance. Submission is never published. Batch,
residency, and session release in that order. Source may report the failure but
does not retry automatically.

### Cancellation after submit

The `cancel_join` policy requests child cancellation. `Wait` is an explicit
observation point and forwards the request to the provider. The child remains
owned by the scope until the provider proves completed, cancelled, lost, or
faulted. `Cancelled` means no later private output can publish. Resources then
release in reverse order and the scope joins before its block can exit.

### Provider loss or stale generation

Wait reports loss/fault rather than completed output. Local release still
invalidates submission, batch, residency, and session values, returns locally
retained capacity, and prevents the old generation from rebinding after restart.
Uncertain private device output is discarded. The application performs no
automatic replay.

### Output mismatch

The provider has completed and all resources have already reached terminal-safe
state. The application returns `Outputˉmismatch`; it does not publish the
accelerated score as accepted inference. The strict reference remains diagnostic
evidence only.

### Terminal trap

Language traps are not caught. User cleanup is not promised after corruption,
but the enclosing runtime/provider resource domain retains pre-reserved teardown
capacity and must reclaim task/provider/device state. A task trap that the runtime
can contain before process corruption appears as bounded `Taskˉtrapped`.

## Common corpus questions

| Question | Finding |
| --- | --- |
| Is every mutation visible? | Yes. Host source mutates only owned provider resources through `borrow mut`; the pure kernel returns a value and target collection writes it under the validated output command. |
| Are ownership moves and borrows visible? | Yes. `using`, task handle consumption, explicit closure capture, and every resource/view parameter show them. |
| Is allocation bounded before growth? | Yes. Host, pinned, task, device, queue, work, command, and diagnostic ceilings precede publication. |
| Are capabilities/effects visible? | Yes. Four independently required interfaces and all exported effect sets are explicit. |
| Can early transfer bypass release? | No ordinary `return`, `try`, cancellation, deadline, or provider-loss path bypasses the lexical task/resource scopes. |
| Is failure typed without exceptions? | Yes. Package rejection, decoder failures, task outcomes, provider terminal outcomes, and mismatches remain distinct. |
| Is target behavior leaking into portable semantics? | No. Provider identity/mode is observable, but source numeric/ownership/failure rules do not inherit CUDA, SPIR-V, or a host ABI. |
| Is any work unbounded? | No. The fixed fixture and every resource/work/diagnostic limit are explicit. |
| Does the kernel need a second language? | No. It is an ordinary edition-1 Core function; only its package target/interface binding is accelerator-specific. |

## What already passes on paper

- general sub-byte core integers are unnecessary for this workload;
- exact nominal format/layout metadata describes I4/f16/f32 semantics;
- package data can carry weights and metadata without filesystem authority;
- ordinary ownership and `using` express session/residency/batch/submission life;
- ordinary async/task syntax expresses one bounded provider operation;
- a normal pure Windvale function expresses the first custom kernel body without
  overlapping lane borrows; and
- the software oracle and tolerance contract are independent of a physical
  backend.

The remaining source-freeze findings are recorded in
[Review-Findings.md](Review-Findings.md), not hidden here as implementation
assumptions.
