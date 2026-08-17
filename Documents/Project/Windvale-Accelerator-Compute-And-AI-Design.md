# Windvale accelerator compute and AI design

> Status: Pre-freeze design evidence required by
> [Decision 0753](../Decisions/0753-Require-Language-1.0-AI-Accelerator-Evidence.md).
> This document refines the accepted architecture in
> [Decision 0171](../Decisions/0171-Future-Virtualization-And-Accelerator-Architecture.md)
> for the Language 1.0 paper corpus. It is not a normative accelerator
> specification, accepted kernel syntax, implementation plan, provider support
> claim, or performance claim. Windvale Seed remains the implemented language.

## Purpose

Windvale needs to run useful local AI and data-parallel work without making one
GPU vendor, driver API, model container, numeric shortcut, or device-memory model
part of the language. Before Language 1.0 freezes, one complete paper program
must prove that the candidate language can describe the host side of that work
and can contain the few target-specific operations that remain.

This design answers five pre-freeze questions:

1. which work belongs to the ordinary language and Foundation libraries;
2. how tensors, layouts, quantized data, and device resources remain explicit;
3. how one compiler admits target-scoped custom kernels without a second
   language;
4. which authority, bounds, numeric behavior, failure, and teardown must be
   observable; and
5. what the eleventh paper workload must prove before implementation begins.

It does not design a neural-network framework, choose a model architecture, or
promise that every provider implements every operation.

## Existing boundaries retained

This design keeps the following repository contracts unchanged:

- platform scope, authority level, required capabilities, and optional
  capabilities are independent metadata dimensions;
- an accelerator is attached as a visible software, paravirtual-shared,
  hardware/vendor-partitioned, or exclusive-passthrough provider;
- portable compute, native-device extensions, physical assignment, graphics,
  and display are separate interfaces;
- memory objects, mappings, pinned or DMA memory, device memory, and
  resource-domain charges are different contracts;
- untrusted kernels, command records, shapes, lengths, offsets, model data, and
  provider results are validated before expensive work;
- provider loss, cancellation, timeout, device reset, contained fault, and clean
  completion are different outcomes; and
- a software implementation is a semantic oracle, not a physical-accelerator
  performance claim.

The existing external-model gateway sends bounded requests to a separately
authorized remote or hosted provider. Local accelerator compute consumes
package/model data and device resources on the selected machine. Neither
capability implies the other.

## Four contract layers

### Portable host and framework layer

Ordinary Language 1.0 source owns model graphs, tokenization, preprocessing,
shape validation, memory planning, provider selection, fallback, scheduling,
result interpretation, and application policy. It uses existing language
facilities: nominal records and variants, generics and protocols, bounded
collections, package data, ownership and borrowing, resource scope, explicit
effects, structured tasks, and typed recoverable failure.

This layer is portable when all imported parts and selected operations are
portable. It may run entirely through the software provider.

### Portable accelerator-operation layer

A separately versioned library and capability contract exposes a deliberately
small set of exact operations needed by more than one provider. Candidate
families include:

- admitted device/session discovery with observable attachment mode and feature
  limits;
- bounded buffer construction, immutable upload, readback, and release;
- tensor views with checked shape, stride, layout, element-format, byte-range,
  and alias rules;
- exact elementwise, reduction, matrix/tensor multiplication, conversion, and
  quantization operations;
- explicit command submission, dependencies, completion, cancellation request,
  deadline, and terminal result; and
- bounded diagnostics and profiling through separate authority.

An operation belongs here only when its numeric, layout, memory, alias,
completion, and failure behavior can be specified independently of one provider.
An implementation may fuse or batch operations only when the observable contract
is preserved.

### Target-scoped kernel layer

One small custom kernel tests whether Language 1.0 can reach operations not yet
in the portable set. It remains Windvale source processed by the same lexer,
parser, type system, ownership analysis, WIR orchestration, diagnostics, and
build graph. The module declares exact target or extension requirements and is
unavailable when the selected build cannot satisfy them.

A later normative kernel contract may need typed address spaces, workgroup and
subgroup identities, dispatch geometry, barriers, atomics, vector/tile
operations, and provider feature requirements. These should be named types,
intrinsics, protocols, or target metadata when possible. This design does not
accept a new grammar production merely because another ecosystem spells an
operation as a keyword.

The compiler must reject recursion, unsupported calls, hidden allocation,
unbounded loops, illegal aliasing, cross-address-space references, divergent
barrier use, invalid atomic orders, and unavailable features before publication
when the target contract forbids them. Exact rules remain future specification
work informed by the paper kernel.

### Provider layer

Software, Windows, Linux, Windvale OS, vendor, shared-device, partition, and
passthrough adapters translate an admitted operation or kernel to the selected
environment. A provider reports its identity, generation, attachment mode,
memory and queue limits, supported format/operation set, numeric modes, and
failure containment. It cannot redefine a common operation silently.

CUDA, SPIR-V/Vulkan, DirectML, vendor libraries, or future Windvale-native device
interfaces may inform and implement adapters; none is Windvale semantics. Native
extension calls are explicitly target-scoped and do not make the calling part
portable.

## Values, formats, and tensors

### Scalar value versus stored representation

A bit width alone is not a complete AI value contract. Four stored bits could
mean a signed integer, unsigned integer, one of several floating formats, or a
codebook index. Its interpretation may depend on a scale, zero point, group,
tile, lane order, bit order, and wider accumulator.

Language 1.0 therefore keeps ordinary portable fixed-width scalar values and
does not add general `i1`, `i2`, `i4`, `u1`, `u2`, or `u4` primitives for packed
storage. Accelerator libraries use nominal descriptors such as:

- packed signed or unsigned lanes with an exact lane width and packing order;
- binary or bit-plane tensors with an exact logical operation contract;
- affine or symmetric quantized tensors with exact scale and zero-point types;
- block-scaled formats with exact group and scale layout; and
- named FP4, FP6, FP8, `f16`, or `bf16` encodings with exact conversion and
  accumulation behavior.

These are semantic categories, not accepted source declarations. The final
contracts must use canonical names and versioned identities rather than a loose
record whose fields can form unsupported combinations.

### Tensor contract

A tensor value or view needs at least:

- element or quantized-format identity;
- rank and per-dimension extents;
- logical layout and exact byte strides where applicable;
- backing buffer identity, generation, permitted byte range, and address space;
- mutability and alias/overlap rules;
- maximum logical elements, backing bytes, and validation work; and
- for quantized values, scale, zero-point, group/tile, packing, and accumulation
  metadata.

Shapes and strides use checked fixed-width values selected by the owning
contract; there is no portable pointer-sized integer. Validation proves every
reachable element lies inside the admitted buffer before dispatch. A zero-copy
view borrows storage and cannot outlive or move independently of its backing
resource.

The first proof needs dense tensors, checked slices/views, and one explicit
quantized layout. Sparse tensors, ragged tensors, symbolic unbounded dimensions,
and semantically unbounded collections are outside the first contract.

## Ownership and memory

Package-backed weights and tokenizer/model metadata are shared immutable source
values. Their package binding proves content identity and maximum bytes but does
not require eager materialization. An implementation may map, stream, page, or
decompress bounded regions when those mechanisms preserve the same value and
accounting contract.

Host buffers and device buffers are different move-owned resources. A device
buffer carries provider and generation identity, admitted byte count, rights,
format constraints, and terminal release behavior; it does not expose a host
pointer. Tensor views borrow buffers. Submission records retain the exact
resource generations required until terminal completion or teardown, preventing
the application from releasing or repurposing storage still visible to a device.

Admission reserves the complete host, pinned, device, queue, command, retained
evidence, and diagnostic charges required by the operation before publication.
Rejected admission leaves usage unchanged. Teardown blocks new work, resolves or
fails outstanding submissions, revokes mappings and DMA, invalidates generations,
releases provider resources, and returns every charge in bounded order.

The design permits chunked model loading and bounded residency. It does not
assume the entire package, model, or activation graph fits simultaneously in
host RAM or device memory.

## Authority and capability split

One `gpu` or `accelerator.compute` flag would combine materially different
authority. The paper workload must test a minimal split among these candidate
responsibilities:

| Responsibility | Why it may require separate approval |
| --- | --- |
| Catalog and selection | Reveals available providers, modes, features, and limits but need not authorize execution. |
| Device-memory admission | Reserves scarce or pinned resources without authorizing arbitrary kernels. |
| Portable operation execution | Runs only the versioned common operation set within admitted bounds. |
| Target-scoped kernel loading | Admits caller-supplied device code and therefore enlarges validation and fault risk. |
| Native-device extension | Exposes provider/vendor behavior that is not portable. |
| Profiling and diagnostics | May expose timing, topology, co-tenant, model, or data-sensitive information. |
| Partition or passthrough assignment | Changes physical ownership, DMA, reset, and isolation boundaries. |

These are responsibilities, not frozen capability names or signatures. The
application approves the exact transitive requirement set; the launcher binds
rights-limited instances separately. A library requirement is never a grant.

## Execution and completion

The host submits a bounded command graph or ordered batch against one provider
generation. Every command names exact input/output views, operation identity,
numeric mode, work bound, dependencies, and output limit. Unknown required
features reject before submission.

Submission returns a move-owned typed handle integrated with Language 1.0 task
scope. Awaiting observes one terminal outcome. Requesting cancellation does not
mean that hardware rolled back work already accepted. The result distinguishes:

- rejected before device acceptance;
- clean completion;
- cancellation observed with a stated completion boundary;
- deadline exceeded;
- contained kernel or data fault;
- unsupported operation, format, shape, or numeric mode;
- provider loss, reset, removal, or stale generation; and
- indeterminate mutation of externally visible device state when a provider
  cannot prove a stronger result.

Read-only or privately produced inference work can discard an indeterminate
private output during teardown. A mutating external-device operation cannot be
retried automatically unless its contract is idempotent.

Independent submissions may execute concurrently, but result publication and
diagnostics have an explicit deterministic order. Provider scheduling and
wall-clock timing are not portable semantic evidence.

## Numeric contract

Every operation states its input formats, output format, accumulation type,
rounding, overflow/saturation, NaN, infinity, signed-zero, subnormal, conversion,
reduction-order, and reproducibility behavior. No ambient compiler option may
silently enable fast math.

The first workload has two modes:

1. a strict software/reference mode that provides the semantic oracle; and
2. an explicitly selected accelerated mode whose allowed error and exceptional
   behavior are recorded per operation.

Comparison uses an exact tolerance contract appropriate to the operation and
format, not a statement that results are “close enough.” The record includes
absolute/relative or ULP limits, exceptional-value handling, permitted reduction
variation, input domain, and the provider features used.

Mixed-precision matrix multiplication names its input formats and accumulator
explicitly. A provider cannot substitute a narrower accumulator, approximate
operation, stochastic rounding, or nondeterministic reduction unless the chosen
contract permits that exact behavior.

## Eleventh paper workload

The paper bundle implements one bounded local inference pipeline with enough
real pressure to expose design gaps without attempting a complete framework.

### Required scenario

1. Bind tokenizer metadata, model metadata, and quantized weights through
   immutable `package data` declarations with exact maxima and content identities.
2. Parse and validate one versioned model description without reflection or an
   ambient filesystem path.
3. Approve and bind the minimum accelerator capabilities; report the selected
   provider generation, attachment mode, limits, operation set, and numeric modes.
4. Construct a bounded residency plan and reject a model that cannot fit the host,
   pinned, device, queue, or diagnostic budgets before partial publication.
5. Validate dense tensor shapes, strides, views, ranges, quantization metadata,
   and alias rules.
6. Upload or stream weights and inputs through explicit asynchronous operations.
7. Execute at least one mixed-precision matrix/tensor multiplication with wider
   accumulation plus ordinary elementwise work.
8. Execute one small target-scoped custom Windvale kernel through the same
   compiler architecture, with a software/reference equivalent.
9. Observe clean completion, cancellation, unsupported format/feature, stale
   generation, provider loss, and complete teardown.
10. Compare the accelerated output with the software reference under one exact
    tolerance and exceptional-value contract.

The example model may be deliberately small. Reducing its dimensions must not
remove quantized packing, layout, memory residency, asynchronous completion,
custom-kernel, provider-loss, or numeric-comparison pressure.

### Required evidence

The bundle records source modules, package/build mapping, target requirements,
capability/effect closure, every resource maximum, ownership moves and borrows,
operation/format identities, provider feature negotiation, command graph,
cleanup sequence, expected reference output, tolerance contract, compiler/WIR
expectations, and at least ten rejected or boundary cases.

Required rejected cases include malformed model metadata, digest mismatch,
oversized weights, shape product overflow, out-of-range stride/view, incompatible
quantization grouping, unsupported format, insufficient device memory, stale
provider generation, illegal custom kernel, divergent or invalid barrier use if
the kernel uses a barrier, cancellation, provider loss, and diagnostic-limit
exhaustion.

### Acceptance

The paper workload passes only when:

- ordinary host/framework logic uses the candidate Language 1.0 contract without
  accelerator-specific grammar;
- the custom kernel remains one explicitly target-scoped module compiled through
  the shared compiler architecture;
- sub-byte storage is exact without adding general sub-byte scalar primitives;
- no path, host pointer, native handle, hidden allocation, hidden authority,
  ambient fast math, or unbounded queue crosses the portable boundary;
- every resource remains owned until terminal completion and releases under
  clean, rejected, cancelled, faulted, lost-provider, and teardown paths;
- the software path supplies a deterministic correctness oracle; and
- any remaining source-language gap is recorded separately from a missing
  library, accelerator, WIR, verifier, provider, or backend contract.

## What remains outside this design set

The following work is intentionally not accepted by this document:

- exact public accelerator capability names and wire signatures;
- final tensor, buffer, format, command, event, or kernel APIs;
- kernel grammar, address-space syntax, atomics, memory model, barrier rules, or
  accelerator WIR/WVB encoding;
- automatic differentiation, training, optimizers, distributed execution,
  collectives, multi-device partitioning, or checkpoint formats;
- general equation or tensor-index notation;
- model-container, tokenizer, neural-network graph, or framework standards;
- a CUDA, Vulkan, DirectML, vendor-library, Windows, Linux, or Windvale OS
  backend; and
- a claim about supported devices, throughput, latency, energy, or model size.

Those items become implementation or later design slices only after the paper
workload proves which contracts are actually required.

## External format evidence

Current accelerator ecosystems reinforce the format/operation boundary used
here. The [CUDA Math API](https://docs.nvidia.com/cuda/cuda-math-api/index.html)
documents several distinct low-precision families rather than one generic
sub-byte integer. The
[SPIR-V specification](https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html)
models capabilities, execution environments, memory, and packed operations as
explicit contracts. The
[OCP Microscaling Formats specification](https://www.opencompute.org/documents/ocp-microscaling-formats-mx-v1-0-spec-final-pdf)
couples element encodings to block scaling and layout rules. These are design
inputs only; none defines Windvale semantics.

## Freeze questions

The eleventh paper review must resolve:

1. Does host/framework source need any general Language 1.0 change?
2. Which exact tensor and quantized-format descriptors prevent invalid states?
3. What is the smallest useful portable operation set shared by software and at
   least two plausible hardware providers?
4. Which capability responsibilities need separate approval and binding?
5. Can custom kernels use existing Language 1.0 grammar with target-scoped types,
   intrinsics, and metadata, or is one narrowly justified production required?
6. Which address-space, atomic, barrier, numeric, and validation rules belong to
   the first separately versioned kernel contract?
7. Which parts lower through existing WIR/WVB, and which require new verified
   representations without changing source semantics?
8. What reference inputs, outputs, tolerance rules, budgets, and failure cases
   become permanent conformance evidence?

Language 1.0 may freeze when these questions either have exact answers or are
shown to be accelerator-extension work that does not change the general source
contract. Physical accelerator implementation may follow later.
