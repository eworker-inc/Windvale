# Workload 11 package and build plan

## Status and identity

This is the exact paper package plan for canonical package identity
`windvale.paper.language1.ai_inference` version 1. It binds source and immutable
content without host-path lookup, build-script execution, dynamic import, or
runtime package-name lookup.

The hexadecimal payloads below are the canonical package-resource bytes. They
are written inline because this is paper evidence rather than a published binary
package. A later fixture generator must reproduce these bytes and verify the
listed SHA-256 identities before publication; the hex text itself is not the
resource payload.

## Module mapping

| Canonical module | Source | Profile | Target scope |
| --- | --- | --- | --- |
| `Inferenceˉtypes` | [`Source/Inference-Types.wv`](Source/Inference-Types.wv) | Core | Windows, Linux, Windvale |
| `Inferenceˉpackage` | [`Source/Inference-Package.wv`](Source/Inference-Package.wv) | Core | Windows, Linux, Windvale |
| `Inferenceˉdecode` | [`Source/Inference-Decode.wv`](Source/Inference-Decode.wv) | Core | Windows, Linux, Windvale |
| `Inferenceˉreference` | [`Source/Inference-Reference.wv`](Source/Inference-Reference.wv) | Core | Windows, Linux, Windvale |
| `Inferenceˉaccelerated` | [`Source/Inference-Accelerated.wv`](Source/Inference-Accelerated.wv) | Hosted | Windows, Linux, Windvale |
| `Inferenceˉapplication` | [`Source/Inference-Application.wv`](Source/Inference-Application.wv) | Hosted application | Windows, Linux, Windvale |
| `Inferenceˉkernel` | [`Source/Inference-Kernel.wv`](Source/Inference-Kernel.wv) | Core kernel part | `accelerator.software.v1`, `accelerator.spirv.v1` |

The build also supplies the exact accepted normative-candidate Foundation
signatures named in the Language 1.0 Foundation paper and the paper-only `Platformˉaccelerator`
signatures in
[Accelerator-Contract.md](Accelerator-Contract.md). Those supplied modules are
dependencies, not searched source.

## Package-resource bindings

| Declaration identity | Resource identity | Type | Declared maximum | Exact length | SHA-256 |
| --- | --- | --- | ---: | ---: | --- |
| `Inferenceˉpackage.Tokenizerˉmetadata` | `windvale.paper.ai.tokenizer_metadata.v1` | `bytes` | 24 | 24 | `8afe251e891d940612b590e790cafd4af44c24761eb138ff63571d5231eadbc9` |
| `Inferenceˉpackage.Modelˉmetadata` | `windvale.paper.ai.model_metadata.v1` | `bytes` | 64 | 64 | `0ee6c80f6468f0abab3858b2753143b706773e17da842516d48cefa7e7c4d28d` |
| `Inferenceˉpackage.Quantizedˉweights` | `windvale.paper.ai.quantized_weights.v1` | `bytes` | 4 | 4 | `098177b651ae32818b7e6c9271592ab675c5662f17e7f9bbf798a9db4c10ecba` |
| `Inferenceˉpackage.Inputˉtokens` | `windvale.paper.ai.input_tokens.case1` | `bytes` | 4 | 4 | `9f64a747e1b97f131fabb6b447296c9b6f0201e79fb3c5356e6c77e89b6a806a` |

All four digests are distinct, so canonical shipment contains four content
objects totaling 96 bytes. Each declaration references its one object exactly
once. Mapping or sharing a content object never multiplies its resource-domain
charge. No declaration exposes a path, native handle, mapping address, or
accelerator grant.

## Canonical payloads

### Tokenizer metadata: 24 bytes

```text
57 56 54 4B 30 30 30 31  04 00 00 00 08 00 00 00
00 40 00 BC 00 38 00 42
```

| Offset | Field | Value |
| ---: | --- | --- |
| 0 | Magic/version | ASCII `WVTK0001` |
| 8 | Entry count, little-endian u32 | 4 |
| 12 | Activation bytes, little-endian u32 | 8 |
| 16 | Four finite f16 values | `0x4000`, `0xBC00`, `0x3800`, `0x4200` |

The four decoded feature values are `2.0`, `-1.0`, `0.5`, and `3.0`.
Zero/subnormal and infinity/NaN f16 encodings are outside this first finite-normal
tokenizer format and reject explicitly.

### Model metadata: 64 bytes

```text
57 56 41 49 30 30 30 31  04 00 00 00 02 00 00 00
04 00 00 00 01 01 01 01  00 00 00 3F 00 00 80 3E
00 00 80 3E 00 00 00 BE  00 40 00 00 00 00 00 00
40 01 00 00 00 00 00 00  08 00 00 00 10 00 00 00
```

| Offset | Field | Value |
| ---: | --- | --- |
| 0 | Magic/version | ASCII `WVAI0001` |
| 8 | Input elements, u32 | 4 |
| 12 | Output elements, u32 | 2 |
| 16 | Packed weight bytes, u32 | 4 |
| 20 | Weight format | 1 = signed I4 two's-complement |
| 21 | Activation format | 1 = finite f16 interchange bits |
| 22 | Accumulator format | 1 = strict f32 |
| 23 | Weight layout | 1 = row-major, low nibble first |
| 24 | Row scales, two f32 values | `0.5`, `0.25` |
| 32 | Row biases, two f32 values | `0.25`, `-0.125` |
| 40 | Maximum host bytes, u64 | 16,384 |
| 48 | Maximum device bytes, u64 | 320 |
| 56 | Maximum commands, u32 | 8 |
| 60 | Maximum diagnostic records, u32 | 16 |

Every multi-byte value is little-endian. Reserved, unknown, non-finite, invalid
scale, contradictory-shape, and excessive-limit values reject before provider
selection.

### Quantized weights: 4 bytes

```text
E1 03 2F 4D
```

Low-nibble-first signed I4 unpacking produces this row-major matrix:

```text
[  1, -2,  3, 0 ]
[ -1,  2, -3, 4 ]
```

### Input tokens: 4 bytes

```text
01 02 03 04
```

The fixed tokenizer format maps the ordered token identities to the four f16
feature entries. Duplicate, missing, reordered, or unknown token identities are
invalid for this exact fixture.

## Target parts and kernel binding

The package plan selects exported `Inferenceˉapplication.Run` by canonical
identity and exact monomorphic signature whose parameters are owned
`Memoryˉbudget` followed by borrowed parent `Operationˉcontext` for its hosted
launcher profile. The
launcher creates and transfers the owned 16,384-byte root `Memoryˉbudget`,
borrows one launcher-created `Operationˉcontext`, approves the exact
four-capability transitive closure, binds four rights-limited module roots, and
starts no source until every binding is admitted. `Run` is an ordinary source
name, not a special language entry. Task construction derives the child context;
the async child copies that scope-bound view and passes it to every accelerator
operation, so cancellation and deadline observation do not form a second system.

The host build selects one `Platformˉaccelerator` implementation compatible with
Windows, Linux, or Windvale. Provider selection remains runtime-visible and may
select the software implementation. The package graph separately selects one
kernel target part through the canonical `accelerator.software.v1` or
`accelerator.spirv.v1` target-interface registry key:

| Kernel property | Exact paper value |
| --- | --- |
| Source module/export | `Inferenceˉkernel.Biasˉreluˉlane` |
| Kernel identity | `windvale.paper.bias_relu.v1` |
| Interface identity | `windvale.kernel.lane_f32x2.v1` |
| Dispatch | X = 2, Y = 1, Z = 1 |
| Scalar lane inputs | one f32 accumulator value; one f32 bias value |
| Scalar lane result | one f32 value, collected at the matching lane index |
| Addressability | provider-validated views only; no source pointer or address |
| Targets | software reference; future SPIR-V provider |

The package/build plan binds the kernel identity to the compiled target part and
its eventual target-artifact digest. Source cannot substitute a runtime string or
load arbitrary device code merely because it knows the identity text.

## Resource admission plan

### Host-domain ceiling: 16,384 bytes

| Charge | Maximum bytes |
| --- | ---: |
| Four immutable package objects | 96 |
| Parser and ordinary source values | 512 |
| Task scope, continuation, and one retained outcome | 4,096 |
| Session, residency, batch, and submission host state | 4,096 |
| Pinned staging | 64 |
| Readback result | 8 |
| Sixteen bounded diagnostic records | 2,048 |
| Reserved cancellation/teardown capacity | 2,048 |
| Unassigned admitted headroom | 3,416 |
| **Total** | **16,384** |

The package/build plan admits the complete ceiling before launch. The unassigned
headroom is still charged authority, not permission for unbounded growth.

### Device-domain ceiling: 320 bytes

| Slot | Logical bytes | Charged ceiling |
| --- | ---: | ---: |
| Input f16 tensor | 8 | 64 |
| Packed I4 weights | 4 | 64 |
| f32 scales and biases | 16 | 64 |
| f32 accumulators | 8 | 64 |
| f32 output | 8 | 64 |
| **Total** | **44** | **320** |

The 64-byte per-slot charged ceiling permits provider alignment without exposing
layout. A provider requiring more rejects admission before allocating any slot.
No partial residency becomes visible.

| View | Format/shape | Logical element strides | Byte range | Rights |
| --- | --- | --- | --- | --- |
| Input | f16 `[1,4]` | `[4,1]` | input 0..8 | Read |
| Weights | signed-I4 `[2,4]` | `[4,1]` | weights 0..4 | Read |
| Scales | f32 `[2]` | `[1]` | parameters 0..8 | Read |
| Bias | f32 `[2]` | `[1]` | parameters 8..16 | Read |
| Accumulator | f32 `[2]` | `[1]` | accumulator 0..8 | Read/write |
| Output | f32 `[2]` | `[1]` | output 0..8 | Read/write |

Rank-one descriptors canonically set the unused second extent to one and second
stride to zero. Signed-I4 strides count logical elements; the format/layout
contract converts the checked logical range to low-nibble-first bytes.

### Work and queue ceilings

- one task child, runnable entry, completion entry, provider queue, submission,
  and timer;
- six commands: three uploads, quantized linear, custom kernel, and readback;
- eight admitted command slots, leaving two sealed headroom slots;
- 64 accelerator work units and 256 enclosing task work units;
- 16 call-depth units; and
- 16 diagnostic records / 2,048 diagnostic bytes.

## Construction and publication order

1. Verify module and resource identities, exact lengths, declared maxima, types,
   and digests.
2. Charge all 96 immutable package bytes once to the application domain.
3. Validate metadata and token/weight geometry without provider work.
4. Admit the complete task and host ceiling.
5. Select a provider and reserve the complete session ceiling.
6. Atomically admit all five device slots and their 320-byte charged ceiling.
7. Build and seal the six-command batch.
8. Submit once; no later source mutation changes the admitted commands.

Any rejection before a step publishes nothing from that step and releases prior
owned state in reverse order.
