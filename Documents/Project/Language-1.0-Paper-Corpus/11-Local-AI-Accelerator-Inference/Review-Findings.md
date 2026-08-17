# Workload 11 review findings

## Status

First-author review complete on 2026-08-17. The bundle covers every required
workload-11 scenario and is ready for project-owner review. It remains a draft
paper source bundle, not an accepted corpus row, because the complete Language
1.0 suite and several exact dependency signatures are not frozen or implemented.

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

Recommended resolution: Language 1.0 should permit deterministic
argument-derived generic parameter resolution only when every type/constant
parameter is solved uniquely from explicit argument types. A parameter not
solved that way is a diagnostic; result context cannot guess it. If the project
instead requires explicit generic arguments, the grammar must add one
unambiguous call spelling and update every paper source. The current documents
should not leave generic functions declared but uncallable.

### 2. Module-bound capability roots versus closure capture

The async closure explicitly captures only lexical `Model`. It calls an imported
async function whose signature exposes four capability effects; the application
and library modules both declare those four requirements. No instance resource
or local capability value is captured implicitly.

Recommended resolution: clarify that an approved module-bound singleton
capability root is not a lexical closure capture. Its use remains part of the
function/closure effect set and module dependency closure. An instance-bearing
capability or rights-reduced provider value stored in a local must still appear
as an explicit copy/move/borrow capture. This preserves current singleton calls
without making future instance authority ambient.

### 3. Exact Foundation call signatures

The Foundation candidate owns the required semantics but does not yet spell
every `Bytes.Length`, bounds-checked `Bytes.At`, task-scope `Construct`, and
`Await` signature used here. The paper source selects coherent candidate names.

Recommended resolution: freeze these signatures with the full eleven-workload
review and update source coherently if another workload proves a better shape.
This is Foundation completion, not accelerator syntax.

### 4. Application entry and root budgets

The exported application `Run` receives one owned `Memoryˉbudget`; source cannot
manufacture ambient host memory. The candidate language does not itself own an
application entry ABI or launcher parameter binding.

Recommended resolution: the package/launcher contract should bind the selected
entry signature, root budget, and approved capability roots. Do not add a hidden
global allocator or special `Main` semantics merely for this workload.

### 5. Accelerator target-scope identity

The kernel uses canonical paper scopes `accelerator.software.v1` and
`accelerator.spirv.v1`. The grammar can carry those names today, while the build
graph owns their meaning and alternative selection.

Recommended resolution: publish their canonical target/extension registry and
structured relationship to environment/architecture/ABI metadata in the later
kernel contract. No grammar change is demonstrated.

## Quantitative review record

| Measure | Recorded value |
| --- | --- |
| Source size | 7 modules, 1,203 lines, 28 functions, 6 records, 6 enums, 1 variant, 4 package-data declarations. |
| Maximum source width | 4 function parameters; 15 record fields; 13 failure cases; largest module 370 lines. |
| Explicitness | 4 capabilities, 1 copied closure capture, 4 nested provider resources, 6 tensor-view descriptors, 6 commands, 5 device slots. |
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

## Reviewer decision requested

The owner can now either:

1. accept the five recommended clarifications as the direction for the later
   coherent Language 1.0 suite update; or
2. request a source/boundary revision while keeping this corpus row in draft.

Even after owner acceptance, the corpus row should remain “draft reviewed” until
the dependent Foundation and Language 1.0 documents are updated and all eleven
paper workloads pass. This bundle alone does not freeze edition 1.
