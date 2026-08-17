# Decision 0753: Require Language 1.0 AI accelerator evidence

## Status

Accepted by the project owner on 2026-08-17. This decision refines
[Decision 0171](0171-Future-Virtualization-And-Accelerator-Architecture.md),
[Decision 0751](0751-Accept-Windvale-Language-1.0-Direction.md), and
[Decision 0752](0752-Complete-Language-1.0-Collection-And-Package-Data-Boundaries.md).
It does not freeze source edition 1, change Windvale Seed, select an accelerator
API or backend, change WIR or WVB, or claim accelerator implementation or
performance on any target.

## Context

Windvale is intended to implement AI and other data-parallel workloads locally,
not only call an external model provider. The Language 1.0 candidate already has
the general host-language facilities needed to describe model metadata, bounded
collections, package-backed weights, ownership, capabilities, tasks, resources,
unsafe adapters, and explicit failure. That is necessary but not sufficient
evidence that one source language can express a practical accelerator workload.

Current hardware also makes a simple scalar-width answer misleading. A value
described as one, two, four, six, or eight bits may be an integer, float,
codebook entry, packed lane, or quantized value with a scale, zero point, group,
tile, layout, and wider accumulation rule. Turning every storage format into a
general source primitive would put vendor and model-format policy into portable
language semantics without proving ordinary scalar use.

Decision 0171 already requires explicit accelerator attachment modes, separate
portable and native-extension capabilities, bounded queues and memory, provider
loss, fault containment, and a software oracle. Language 1.0 needs paper evidence
that its source, ownership, capability, and concurrency rules can express those
boundaries before source freeze.

## Decision

Add an eleventh mandatory Language 1.0 paper workload: bounded local AI
inference through an explicitly bound accelerator provider. The workload must
exercise package-backed model data, tensor shapes and layouts, quantized values,
mixed-precision accumulation, bounded device memory, asynchronous transfer and
dispatch, cancellation, provider loss, deterministic result collection, a CPU
or software reference comparison, and one small target-scoped custom kernel.

Do not add general `i1`, `i2`, `i4`, `u1`, `u2`, or `u4` primitives to Language
1.0 merely to represent packed AI storage. The accelerator design instead owns
nominal packed and quantized formats with exact signedness, encoding, packing,
scale, zero-point, grouping, layout, and accumulation semantics. Exact `f16`,
`bf16`, FP8, FP6, and FP4 formats begin as accelerator or library contracts.
Promotion of a format to a portable core scalar requires independent ordinary
source-language evidence and a named decision.

Use the same Windvale Language 1.0 source contract and shared compiler pipeline
for host code and target-scoped custom kernels. Do not create a second shader or
kernel language. Kernel admission may add explicit target requirements, address
spaces, dispatch geometry, barriers, atomics, and operations behind a separately
versioned accelerator source and verification contract; those details do not
become accepted Language 1.0 syntax through this decision.

Keep four layers distinct:

1. portable host and framework code owns model structure, bounds, scheduling,
   fallback, and result interpretation;
2. a portable accelerator-operation contract owns exact common tensor, transfer,
   dispatch, numeric, completion, and failure semantics;
3. a target-scoped kernel contract owns only operations that cannot be expressed
   through the common operation set; and
4. software, shared-device, hardware-partition, passthrough, host, and vendor
   providers implement declared subsets without defining Windvale semantics.

The accelerator contract must not use one broad authority flag. Discovery,
memory admission, portable execution, target-scoped kernel loading, native-device
extensions, profiling, and physical-device assignment remain independently
approvable where their authority differs. Exact canonical capability names and
signatures remain open until the paper workload proves the smallest coherent
split.

Every numeric mode is explicit. A strict software/reference path supplies the
correctness oracle. Relaxed rounding, contraction, denormal handling, approximate
operations, tensor-core use, or nondeterministic reduction is selected by an
exact operation or mode and never by ambient fast-math policy.

## Consequences

The Language 1.0 source-freeze gate now requires eleven complete paper source
bundles. The AI workload may cause revisions to existing general language rules,
but accelerator-only operations remain library, capability, target-extension,
WIR, or provider work unless the evidence proves a core-language gap.

Windvale retains one compiler architecture. A CPU/software implementation can
qualify semantics before a physical accelerator backend exists, while physical
performance and vendor-extension claims require separate target evidence.

Package data can carry model weights and metadata without granting filesystem
authority, but loading, mapping, decompression, upload, device storage, and
retained provider state each receive separate bounds and accounting. A package
resource declaration is not accelerator authority and does not imply that its
complete content must be copied into host or device memory at once.

External-model gateway capabilities remain separate from local accelerator
compute. Authority to send a prompt to a remote provider neither grants device
access nor satisfies this local inference evidence.

Training, automatic differentiation, distributed execution, multi-device
collectives, and general scientific tensor notation remain later design work.
Their absence does not weaken the local bounded-inference proof and they do not
justify speculative Language 1.0 syntax.

## Reconsideration triggers

Reconsider the absence of a sub-byte core scalar only when multiple ordinary,
non-accelerator programs need value—not storage—semantics that nominal packed
formats cannot express clearly and efficiently.

Reconsider the shared source-language boundary if a complete custom-kernel paper
program cannot state ownership, address spaces, synchronization, target
requirements, bounds, and diagnostics without a contradictory language rule.
The revision must first attempt an explicit target-scoped contract over the same
parser, type system, and compiler pipeline rather than introducing a parallel
language.

Reconsider the portable operation set when at least two independent providers
cannot implement an accepted operation with the specified numeric, resource, and
failure behavior. Narrow or split that operation rather than silently weakening
its semantics.
