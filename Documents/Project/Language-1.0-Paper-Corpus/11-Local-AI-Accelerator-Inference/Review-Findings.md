# Workload 11 review findings

## Status

First-author review completed and the project owner accepted all five recommended
clarifications on 2026-08-17 under
[Decision 0754](../../../Decisions/0754-Resolve-First-Language-1.0-Paper-Findings.md).
Its task/context surface was reconciled after workload 6 under
[Decision 0760](../../../Decisions/0760-Resolve-Language-1.0-Concurrent-Service-Findings.md).
The dependent semantic, grammar, Foundation, migration, and paper documents now
carry those normative-candidate rules. The bundle remains a draft-reviewed paper
source row, not frozen or implemented source, because ten corpus workloads and
the complete Language 1.0 signature identities are still pending.

## Required-scenario matrix

| Requirement | Paper evidence | Standing |
| --- | --- | :---: |
| Package-backed tokenizer, model, and quantized weights | Four exact package declarations/bindings, 96 bytes, lengths and SHA-256 identities. | Pass on paper |
| Versioned model parsing without reflection/path lookup | Fixed `WVTK0001` and `WVAI0001` decoders with exact endian/format/budget validation. | Pass on paper |
| Minimum capability approval and observable provider generation/mode | Four-interface split; catalog description is observable before session use and must match terminal identity, generation, and attachment mode. | Pass on paper |
| Bounded residency and rejection before partial publication | Five slots, 44 logical / 320 charged bytes, atomic residency. | Pass on paper |
| Shape, layout, range, quantization, and alias validation | Exact 4-by-2 geometry, I4 packing, scale grouping, source ranges, slot sizes, and non-aliasing kernel output. | Pass on paper |
| Asynchronous transfer and dispatch | One six-command sealed submission and typed `Wait` under a cancel-and-join task scope. | Pass on paper |
| Mixed-precision matrix/tensor work | f16 activations, signed-I4 weights, per-row f32 scales, f32 accumulation. | Pass on paper |
| One custom Windvale kernel through the shared compiler | Ordinary edition-1 `Biasˉreluˉlane` with package-bound kernel/interface identities. | Pass on paper |
| Clean, cancelled, unsupported, stale/lost, fault, and teardown outcomes | Closed task/provider failure families and reverse release walkthroughs. | Pass on paper |
| Software reference comparison | Exact `[3.0, 1.5]`, output bytes/digest, strict operation order, and absolute-plus-relative tolerance. | Pass on paper |

## Main design findings

### No general sub-byte scalar is required

The workload represents I4 as a nominal tensor storage format whose contract
includes signed two's-complement interpretation, nibble order, row-major layout,
row scaling, and f32 accumulation. An `i4` primitive would express only a small
part of that meaning and would not simplify source or validation. The original
recommendation stands.

### One language and compiler remain credible

The custom kernel body uses two Copy f32 parameters, strict arithmetic, a
conditional, and return. The target interface maps validated tensor lanes to
independent calls and collects results by lane, avoiding overlapping mutable
borrows. It needs a target interface and verified representation, not a second
lexer, parser, type system, or source language. No new grammar is justified by
this kernel.

### The four-capability split is sufficient for the first proof

Catalog, memory, portable execution, and package-bound custom-kernel authority
separate materially different actions without forcing profiling, native
extensions, display, graphics, partition, or passthrough into the grant. A later
workload may split an interface further, but this proof gives no evidence for
combining them into one broad capability.

### Package data supports model inputs without filesystem authority

The application can validate and share exact immutable model bytes, retain them
through an asynchronous command, and charge them once. No model path, runtime
package lookup, or eager full-model copy is required. Larger models can retain
the same semantics while adding separately bounded streaming/residency policy.

### Memory management remains ordinary language/runtime ownership

Package bytes are shared immutable, task/session/residency/batch/submission values
are move-owned resources, tensor views are borrows, and resource domains own
accounting/teardown. The accelerator does not need tracing garbage collection or
special source pointers.

## Source-freeze clarifications exposed

### 1. Exact generic-call resolution

The source uses `Mapˉaccelerator<T>` and `Mapˉspawn<W>`. Their type parameters are
uniquely recoverable by structural equality from one exact argument type; there
is no overload set, conversion, return-context choice, or protocol search.

Accepted resolution: Language 1.0 permits deterministic argument-derived
generic parameter resolution only when every type/constant
parameter is solved uniquely from explicit argument types. A parameter not
solved that way is a diagnostic; result context cannot guess it. Decision 0758
later admits a full-arity explicit suffix for one resolved named declaration
when argument evidence is insufficient. This workload still uses the ordinary
argument-derived form and needs no source revision.

### 2. Module-bound capability roots versus closure capture

The async closure explicitly captures lexical `Model` and the scope-derived
`Context`. It calls an imported
async function whose signature exposes four capability effects; the application
and library modules both declare those four requirements. No instance resource
or local capability value is captured implicitly.

Accepted resolution: an approved module-bound singleton capability root is not a
lexical closure capture. Its use remains part of the
function/closure effect set and module dependency closure. An instance-bearing
capability or rights-reduced provider value stored in a local must still appear
as an explicit copy/move/borrow capture. This preserves current singleton calls
without making future instance authority ambient.

### 3. Exact Foundation call signatures

At first-author review, the Foundation candidate owned the required semantics but
did not spell every `Bytes.Length`, bounds-checked `Bytes.At`, task-scope
`Construct`, and `Await` signature used here. Decision 0754 accepts the coherent
paper-selected names and signature shapes while the complete module identities
still wait for all eleven workloads.

Accepted resolution: retain these signatures through the full eleven-workload
review and update source coherently if another workload proves a better shape.
This is Foundation completion, not accelerator syntax.

Workload 6 supplied that later evidence. Decision 0760 replaces the
allocation-only `Construct` failure with exact `Taskˉscopeˉfailure`, requires a
borrowed parent operation context, and adds the scope-derived child context. The
source and dependency list above now use that coherent successor shape.

### 4. Application entry and root budgets

The exported application `Run` receives one owned `Memoryˉbudget`; source cannot
manufacture ambient host memory. The candidate language does not itself own an
application entry ABI or launcher parameter binding.

Accepted resolution: the package/launcher contract binds the selected
entry signature, root budget, and approved capability roots. Do not add a hidden
global allocator or special `Main` semantics merely for this workload.

Decision 0760 later completes this entry boundary: the launcher also lends one
valid parent `Operationˉcontext`; `Task.Construct` derives the child's context,
and every accelerator call borrows that one view. This adds no ambient clock,
cancel flag, or provider authority.

### 5. Accelerator target-scope identity

The kernel uses canonical paper scopes `accelerator.software.v1` and
`accelerator.spirv.v1`. The grammar can carry those names today, while the build
graph owns their meaning and alternative selection.

Accepted resolution: the Language 1.0 suite publishes these opaque canonical
target-interface keys and their relationship to structured environment,
architecture, ABI, extension, and target-interface metadata. The later kernel
contract owns their exact admission and representation. No grammar change is
required.

## Quantitative review record

| Measure | Recorded value |
| --- | --- |
| Source size | 7 modules, 1,316 lines / 48,785 UTF-8 bytes, 28 functions, 6 records, 6 enums, 2 variants, 4 package-data declarations. |
| Maximum source width | 4 function parameters; 15 record fields; 15 inference-failure cases; largest module 389 lines. |
| Explicitness | 4 capabilities, 2 copied closure captures, 4 nested provider resources, 6 tensor-view descriptors, 6 commands, 5 device slots. |
| Resources | 16,384 host bytes, 64 pinned bytes, 320 charged device bytes, 1 task/queue/submission, 8 command slots, 16 diagnostics. |
| Failure surface | Package rejection; 4 decoder families; scope/spawn; accelerator rejection; cancelled/deadline/lost/fault/trap; output validation/mismatch. |
| Compiler planning | 7 `Mapˉaccelerator<T>` specializations, 1 closure-specific spawn adapter, planned ceilings of 64 generic instances, 512 WIR blocks, 4,096 WIR operations, and 1 MiB retained compiler evidence. |
| Artifacts | 96 package-resource bytes; host and target artifact sizes unknown until implementation; a target kernel representation is likely, while no new core source syntax is shown. |
| Usability | Resource nesting is long but readable and mirrors ownership. Generic result mapping removes repeated provider adapters without hiding effects. |

The compiler-planning values are admission ceilings for the future executable
fixture, not measurements or expected exact output. Implementation must record
actual tokens, parse/bind/type time, generic instances, WIR blocks/operations,
retained evidence, WVB bytes, target artifact bytes, elapsed time, and peak memory.

## Revisions made during first-author review

1. Tokenizer metadata was changed to a 24-byte header plus contiguous f16 payload
   so the validated immutable region can be uploaded without a hidden host copy.
2. Device accounting was changed from logical 44 bytes to five explicit 64-byte
   charged ceilings, producing an exact 320-byte provider limit.
3. Model capture was made `copy`, matching its all-Copy scalar fields and avoiding
   a false ownership/lifetime problem across suspension.
4. Catalog, memory, execution, and kernel authority were separated instead of
   using one compute flag.
5. The custom kernel was reduced to a pure scalar lane function and two-lane
   target mapping, proving the shared-language boundary without overlapping
   mutable borrows or speculating about barriers and atomics.
6. The reference operation order and accelerated tolerance were separated so a
   physical provider cannot redefine strict Language 1.0 arithmetic.
7. Workload 6 replaced allocation-only scope construction with exact
   `Taskˉscopeˉfailure`, added parent/scope-derived operation contexts, and split
   task-runtime loss/restart from accelerator-provider loss.

## Owner resolution

The owner selected all five recommendations. The suite now defines:

1. unique argument-derived structural generic-call resolution with no
   result-context inference or explicit generic-call suffix;
2. module-bound singleton capability roots as dependencies rather than lexical
   captures, while local provider instances still require explicit capture;
3. the exact byte, numeric, scope-construction, spawn, and await calls used by
   this workload;
4. package/launcher ownership of entry selection, root budgets, capability-root
   binding, ordinary arguments, and terminal completion; and
5. an opaque target-scope registry over structured environment, architecture,
   ABI, extension, and target-interface metadata.

This resolves the workload's general source blockers. It does not freeze edition
1, the complete Foundation signature sets, the accelerator API, a kernel
representation, or a physical provider.
