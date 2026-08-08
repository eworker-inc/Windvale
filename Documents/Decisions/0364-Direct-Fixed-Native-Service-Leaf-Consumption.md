# Decision 0364: Direct fixed native service leaf consumption

- Status: Accepted current-host normal-path loader reduction; Linux execution and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0355](0355-Windvale-Owned-Native-Utf8-Service-Construction.md), [Decision 0356](0356-Windvale-Owned-Native-Integer-Format-Construction.md), [Decision 0357](0357-Windvale-Owned-Native-Text-Concatenation-Construction.md), [Decision 0358](0358-Windvale-Owned-Native-Text-Quote-Leaf.md), [Decision 0363](0363-Direct-Native-Enum-Name-Leaf-Consumption.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native execution context](../../Specifications/Windvale-Native-Execution-Context.md#dynamic-text-and-byte-arena)

## Context

Decision 0363 proved a smaller normal path for the fixed enum-name leaf: keep
Windvale source and its generator WVB as reproducible qualification/recovery
evidence, but embed and digest-check the exact generated machine leaf instead
of decoding, lowering, publishing, invoking, copying, and tearing down the WVB
inside the ordinary managed runtime.

The UTF-8, text-concatenation, text-quote, signed-format, and unsigned-format
leaves have the same fixed-identity property. Their managed wrappers still ran
four generator WVBs on first use even though the resulting bytes were already
qualified contracts and inputs to pinned service bundles.

## Decision

- Apply the direct generated-artifact boundary to every remaining fixed
  Windvale-owned runtime service leaf.
- Keep all Windvale sources, project closures, and retained generator WVBs in
  the repository for exact native-front-door reproduction, interpreter/backend
  comparison, recovery provenance, and the final grouped gate.
- Embed only the generated fixed leaves in the normal runtime assembly. Remove
  the UTF-8, integer-format, concatenation, quote, and enum-name generator WVBs
  from that assembly.
- Read every fixed artifact through one thread-safe exact-identity path. Reject
  a missing, wrong-length, or wrong-digest artifact before it can enter a
  service bundle or final W^X publication.
- Preserve every existing machine byte, ABI slot, arena rule, failure detail,
  service layout, and descendant identity. This is a supply and loading change,
  not a service semantic or encoding change.
- Keep variable-input constructors, including segmented `WVEN`, on their
  separate verified WVB path until their loading and execution boundary is
  replaced by a native runtime owner.

## Exact generated artifacts

| Leaf | Bytes | SHA-256 |
| --- | ---: | --- |
| Strict UTF-8 validation | 800 | `4c3d2e370d62c8d2f54a3c453f39b94cf46ddabd6db3c2f3d6b65f0713b68aaf` |
| Text concatenation | 249 | `75c5588117e1f5f58a593a23aae6156a3a68a6302df5f50153b977bccbaaa3a0` |
| Text quoting | 1,165 | `4f334af9b6349437d36fd703edb6b5882416f033fae47906a40a4bafdc083bb7` |
| Signed integer formatting | 225 | `c33758106e8d7cd31bbed8ef1e789a8e355c52736c119c75493154a4184fa41e` |
| Unsigned integer formatting | 191 | `b98f2d55e30bb7369e233f94e4ade5f3e8917a7730114446f1ebc81f353e1e43` |

The 323-byte enum-name artifact remains pinned by Decision 0363. Together the
runtime embeds six fixed leaves derived from five retained generator WVBs.

## Evidence and consequences

The four reviewed focused tests still compile every core and bridge through
Stage 0, compare the retained provenance WVBs, reproduce them through the
ordinary native source front door, and require reference-interpreter and
verified-x64 execution to equal the embedded leaves byte for byte. Existing
identity and corruption checks remain. The three-case UTF-8, integer-format,
and concatenation selection passed 3/3 in 3.871 seconds; quote passed 1/1 in
0.768 seconds. The focused Release project built with zero warnings and errors
in 11.68 seconds.

A direct manifest-resource check proves the runtime assembly contains the six
fixed `.bin` leaves and none of the five generator WVBs. The Windows and Linux
qualification scripts pin every generated artifact's exact size and digest;
only script syntax is checked in this slice.

No fixed Windvale-owned pure runtime service now performs managed WVB decoding,
x64 lowering, temporary W^X generator execution, result copying, or generator
teardown in the normal path. Managed code still verifies and assembles service
bundles, executes the variable-input enum-metadata constructor, lays out final
images and service tables, publishes W^X memory, and invokes applications.
Linux execution and the grouped broad gate remain deferred.

## Reconsideration triggers

Regenerate a fixed artifact only from its named Windvale source and record any
identity change explicitly. Replace embedded leaves when native packaging can
derive and bind the same exact artifacts without a managed assembly. Do not
return generator execution to the normal runtime; retain it as independent
qualification and recovery evidence.
