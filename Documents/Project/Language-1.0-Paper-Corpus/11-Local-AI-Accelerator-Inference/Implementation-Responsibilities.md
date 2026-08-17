# Workload 11 implementation responsibilities

## Rule

This matrix assigns every paper dependency to one repository owner. It prevents a
missing accelerator operation from becoming speculative core syntax and prevents
a real general-language gap from being hidden inside a provider. No row is an
implementation authorization before Language 1.0 source freeze.

## Responsibility matrix

| Boundary | Existing owner | Work proven necessary by this bundle | Core source change? |
| --- | --- | --- | --- |
| Edition/module/profile/platform/authority/capability parsing | Language grammar and compiler front end | Parse the seven modules and retain separate host/kernel target scopes and exact four-capability closure. | No new syntax. Decision 0754 defines the canonical target-scope registry and structured target descriptor. |
| Nominal records/enums/variants, named construction, value `if`/`match` | Language semantic owner and compiler | Type the model/result/failure values and exhaustive provider/task outcomes. | No. |
| Strict f32 and unsigned bit operations | Language semantics, Foundation numeric, runtimes/backends | Decode f16 bits, I4 nibbles, metadata, reference math, tolerance, and finite output identically. | No. Decision 0754 accepts the exact numeric helpers used here; the complete numeric matrix remains a freeze dependency. |
| Generic result adapters | Type checker and specialization planner | Resolve seven exact `Mapˉaccelerator<T>` instances and one exact `Mapˉspawn<W>` closure instance without overload search. | No new syntax. Decision 0754 requires unique structural resolution from explicit argument types and rejects result-context inference. |
| Ownership, borrows, closure capture, `using` | Ownership analysis and cleanup lowering | Prove resource nesting, copied model capture, scoped task handle, retained provider generations, and exclusive kernel output. | No new form. Decision 0754 separates module-bound capability roots from explicitly captured local instances. |
| Async/task scope/cancel-and-join | Language effects, Foundation task, runtime scheduler | Construct one bounded scope, spawn one child, await one typed outcome, forward cancellation, join before exit, and retain one result deterministically. | Existing syntax suffices; Decision 0754 accepts the exact `Construct`, semantic `Spawn`, and `Await` calls used here. |
| `package data` source semantics | Parser, source graph, package/build plan | Bind four exact resource identities, maxima, types, lengths, and digests with one 96-byte charge. | No. |
| Package/WVB representation | Package formats, WVB owner, loader, publisher | Store typed content references without duplicating payload bytes; charge mapped/shared bytes; reject malformed bindings before publication. | No source change; likely a versioned typed content-reference table. |
| Capability catalog and singleton call lowering | Capability catalog, source binding, WIR, runtime binding | Resolve four identity/version pairs to exact signature-set/limit profiles and lower qualified calls without ambient grants. | No new syntax. Required module-bound roots are accepted dependencies; local provider instances retain ordinary capture rules. |
| Accelerator public types and operation signatures | Future accelerator library/specification | Publish `Platformˉaccelerator` records/enums/opaque resources, exact failures, six-command graph, and `Boundedˉf32ˉv1`. | Library/capability contract, not core syntax. |
| Resource and device accounting | Runtime/provider resource domains | Atomically reserve/release host 16,384, pinned 64, device 320, queue/submission/command/work/diagnostic ceilings. | No. |
| Software provider | Accelerator provider and test owners | Implement all four candidate capabilities, exact I4/f16/f32 operation, kernel lane ABI, failures, generations, cancellation, and exact reference bytes. | No. This is the first semantic oracle. |
| Kernel source admission | Compiler target analysis and kernel verifier | Admit the ordinary pure scalar `Biasˉreluˉlane` function under exact no-recursion/allocation/task/capability/barrier/atomic rules and the package-bound two-lane mapping. | No grammar change shown; separately versioned kernel restrictions are required. |
| Kernel representation and backend | WIR orchestration, target WIR/verifier, native/accelerator backends | Lower the admitted function and interface for software and one later physical-provider format while preserving source semantics. | No; a target representation may be new. |
| Physical provider adapters | Windows, Linux, Windvale OS, or vendor provider owners | Map the common contract to a measured provider and report generation, attachment mode, limits, numeric mode, faults, and reset. | No. Provider APIs do not define semantics. |
| Application launch | Package/launcher/runtime contract | Select exported `Run`, supply the owned 16,384-byte root memory budget, bind four approved singleton roots, and translate terminal completion. | No language syntax. Decision 0754 assigns exact entry and root binding to named launcher-profile metadata. |
| Diagnostics | Compiler, package, runtime, provider, and kernel verifier | Preserve phase, stable identity, source span or command/stage, expected/observed state, applicable bound, and at most 16 runtime records. | No. |
| Editor tooling | Windvale editor/formatter | Classify edition-1 source, macron names, package data, async closure, task scope, effects, and target scopes; keep WVA/kernel artifacts distinct. | Uses accepted candidate grammar plus eventual kernel target metadata. |
| Verification | Focused Language 1.0/accelerator owners | Turn all valid, boundary, rejected, cleanup, differential, deterministic, and malformed cases into bounded fixtures. | No. |

## Foundation signature dependencies

Decision 0754 accepts the exact operation names and signature shapes used by this
paper source. Complete Foundation module signature-set identities remain pending
until all eleven workloads are reviewed:

- `Foundationˉbytes.Length` and bounds-checked `At`;
- integer widening and `Bitsˉu32ˉtoˉf32`;
- `Result<T,E>` construction, matching, and `try`;
- `Memoryˉbudget` and `Allocationˉfailure`;
- `Taskˉlimits`, `Taskˉscope`, `Task<T,E>`, `Taskˉoutcome<T,E>`,
  `Spawnˉfailure<W>`, `Construct`, `Spawn`, and `Await`; and
- local release for every task/provider resource.

The paper-selected signature shapes are:

```text
Bytes.Length(Value: borrow bytes) -> u64 effects()
Bytes.At(Value: borrow bytes, Index: u64) -> u8 effects()

Numeric.Widenˉu8ˉtoˉu16(Value: u8) -> u16 effects()
Numeric.Widenˉu8ˉtoˉu32(Value: u8) -> u32 effects()
Numeric.Widenˉu8ˉtoˉu64(Value: u8) -> u64 effects()
Numeric.Widenˉu16ˉtoˉu32(Value: u16) -> u32 effects()
Numeric.Widenˉu32ˉtoˉu64(Value: u32) -> u64 effects()
Numeric.Bitsˉu32ˉtoˉf32(Value: u32) -> f32 effects()

Task.Construct(
    Budget: Memoryˉbudget,
    Limits: Taskˉlimits,
) -> Result<Taskˉscope, Allocationˉfailure>
    effects(memory.allocate, resource.acquire)

Task.Spawn(Scope: borrow mut Taskˉscope, Work: W)
    -> Result<Task<T, E>, Spawnˉfailure<W>>
    effects(memory.allocate, task.spawn)
// W has the one exact type async fn() -> Result<T, E> effects(...).

Task.Await(Handle: Task<T, E>) -> Taskˉoutcome<T, E>
    effects(task.suspend)
```

`Bytes.At` is bounds checked. This bundle proves every index in advance; a future
unchecked counterpart would be System-only and is unnecessary here. `Await`
consumes the handle exactly once. Spawn rejection returns the exact owned closure
inside `Spawnˉfailure<W>`.

A later mandatory workload may revise one accepted call only through a named
reconsideration that updates the Foundation candidate and all paper source
coherently. It may not substitute hidden host allocation, exceptions, detached
tasks, unchecked indexing, or implicit numeric conversion.

## Representation planning

### Host path

After the general Language 1.0 features exist, host/framework source should lower
through ordinary typed WIR: records, variants, calls, control flow, borrows,
cleanup edges, strict numeric operations, generic specialization, task operations,
and capability calls. Package-data references and instance-bearing provider
resources may require versioned WIR/WVB evidence, but no host construct justifies
accelerator-specific grammar or a second compiler.

### Accelerator operation path

The portable quantized-linear command is a versioned capability operation. Its
host WIR call retains exact signature-set, format, numeric-mode, resource, and
failure identities. The provider validates command records before translating
them. WVB need not expose a CUDA, SPIR-V, DirectML, or vendor opcode merely to
carry that capability call.

### Custom-kernel path

The compiler front end and ordinary semantic/ownership analysis remain shared.
After admission, the kernel may enter a separate verified target representation
that owns lane mapping, address spaces behind the provider boundary, target
operations, and target artifact identity. The software provider is the
correctness oracle for that representation.
The compiler must not print WVA text and reparse it, and the kernel target format
must not become the definition of Language 1.0.

## Planned verification owners

| Owner | Initial cases |
| --- | --- |
| `language-1-paper-ai` | Candidate parse/name/type/effect/ownership cases and stable paper identities. |
| `package-data` | Four bindings, lengths, digests, type mismatch, missing/duplicate/oversized content, and retained-byte accounting. |
| `accelerator-software` | Valid six-command graph, all admission failures, numeric oracle, cancellation, provider generation/loss, release, and deterministic output. |
| `accelerator-kernel` | Valid lane function, interface binding, target absence, illegal calls/loops/recursion/views, and deterministic software lowering. |
| `accelerator-differential` | Software versus each admitted physical provider under the exact tolerance contract. |

These names are planning labels, not additions to the current verification
registry. Implementation must add focused owners only when executable boundaries
exist; it must not route ordinary paper-document changes to qualification.

## Implementation order after source freeze

1. Freeze the general Language 1.0 and Foundation clarifications exposed by this
   and the other ten paper bundles.
2. Implement the edition-1 host vertical slices in the shared compiler.
3. Implement typed package-data references and the software accelerator contract.
4. Compile and execute the host path entirely through the software provider.
5. Add the separately verified kernel target representation and run the same
   bias/ReLU body through the software provider.
6. Add one measured physical-provider adapter and differential lane without
   changing source semantics.

Training, autodiff, distributed execution, larger operation sets, barriers,
atomics, native extensions, profiling, and passthrough require later measured
consumers and do not enter this order speculatively.
