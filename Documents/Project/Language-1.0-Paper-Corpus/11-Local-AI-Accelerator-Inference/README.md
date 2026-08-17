# Language 1.0 workload 11: local AI accelerator inference

## Status

Complete first-author paper bundle for workload 11 under
[Decision 0753](../../../Decisions/0753-Require-Language-1.0-AI-Accelerator-Evidence.md)
and the
[Language 1.0 paper corpus](../../Windvale-Language-1.0-Paper-Corpus.md).
It is ready for semantic review but is not accepted source, an implemented
program, a frozen accelerator contract, or evidence of physical accelerator
support. Current compilers continue to accept Windvale Seed.

## Result first

The bundle expresses one bounded local inference pipeline without adding a core
sub-byte scalar, a second kernel language, a GPU pointer, ambient fast math, or a
broad GPU capability. It uses:

- four immutable package resources with 96 distinct retained payload bytes;
- token features stored as four exact finite f16 values;
- one 2-by-4 signed-I4 row-major weight tensor, low nibble first;
- per-output f32 scales and biases;
- f32 accumulation through one portable quantized-linear operation;
- one ordinary target-scoped Windvale two-lane bias/ReLU function;
- one strict software reference; and
- one asynchronous six-command submission with typed cancellation, provider
  loss, fault, output-validation, and teardown behavior.

The exact strict output is `[3.0f32, 1.5f32]`, whose little-endian bytes are
`00 00 40 40 00 00 C0 3F`. Deterministic class selection returns class `0`.

## Bundle contents

| Item | Owner |
| --- | --- |
| [`Source/`](Source/) | Seven complete candidate edition-1 modules for types, package data, decoding, reference execution, accelerator execution, the custom kernel, and application orchestration. |
| [Package plan](Package-Plan.md) | Module graph, four exact resource bindings, payload bytes, SHA-256 identities, target parts, budgets, and non-duplication. |
| [Accelerator contract](Accelerator-Contract.md) | Paper-only capability split, nominal types, operations, numeric mode, kernel ABI, completion, cancellation, and release rules used by the source. |
| [Reference oracle](Reference-Oracle.md) | Byte decoding, signed-I4 unpacking, strict f32 evaluation, expected bits, tolerance, and class selection. |
| [Semantic review](Semantic-Review.md) | Metadata, value/ownership inventory, effects, failures, cleanup, cancellation, limits, and common-corpus review answers. |
| [Rejected cases](Rejected-Cases.md) | Compile, build, admission, runtime, provider, kernel, and diagnostic boundary cases. |
| [Implementation responsibilities](Implementation-Responsibilities.md) | Compiler, Foundation, package, WIR/WVB, runtime, provider, backend, verifier, editor, and evidence ownership. |
| [Review findings](Review-Findings.md) | Acceptance matrix, source-freeze findings, recommended resolutions, quantitative planning record, and review status. |

## Source graph

```text
Inferenceˉapplication
  -> Inferenceˉaccelerated
       -> Inferenceˉdecode
       -> Inferenceˉpackage
       -> Inferenceˉtypes
       -> Platformˉaccelerator
  -> Inferenceˉreference
       -> Inferenceˉdecode
       -> Inferenceˉtypes
  -> Foundationˉmemory/result/task

Inferenceˉkernel
  -> no imported module; package metadata supplies the scalar-lane target mapping
```

The host and kernel parts share Language 1.0 syntax and the compiler front end.
They are separate target parts in the build graph: the host part supports
Windows, Linux, and Windvale; the kernel part supports the paper software target
and a future SPIR-V target. No source import searches a path or selects a provider.

## Scenario boundary

This is intentionally a tiny inference model, not a toy omission of the hard
contracts. Reducing the matrix to eight weights preserves all required pressure:
package binding, packed I4 decoding, f16 representation, shape/layout validation,
mixed precision, exact budgets, asynchronous transfer and dispatch, custom
kernel admission, deterministic collection, provider generation, cancellation,
loss, teardown, and differential comparison.

Training, automatic differentiation, distributed execution, model-container
standards, general tensor syntax, native device extensions, profiling, and
physical passthrough are outside this bundle.

## Review rule

Review the source and evidence together. A reviewer must not make the source
appear valid by treating a missing language, Foundation, package, accelerator,
kernel, or launcher contract as an implementation detail. Conversely, a missing
accelerator representation is not a reason to add general source syntax when an
owned target contract suffices.
