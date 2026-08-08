# Decision 0366: Direct native argument-service leaf consumption

- Status: Accepted current-host normal-path loader reduction; Linux execution and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0080](0080-Native-Byte-Result-And-Live-Stencil-Consumption.md), [Decision 0364](0364-Direct-Fixed-Native-Service-Leaf-Consumption.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [WVA native stencil](../../Specifications/Wva-Native-Stencil.md)

## Context

The process-argument-count and process-argument service leaves were already
fixed 5-byte and 70-byte machine contracts. Their normal runtime path still
embedded a 20,800-byte portable generator WVB, then decoded, lowered,
published, executed, copied, split, and cached its immutable 75-byte result.

That live generator retained valuable provenance: Windvale validates both
typed WVO stencil records and derives every ABI patch value. It did not need to
remain an ordinary runtime dependency once its exact outputs were fixed.

## Decision

- Keep the WVA sources, retained WVOs, Windvale consumer and bridge sources,
  and exact bridge WVB as qualification, differential, and recovery evidence.
- Remove `Native-Stencil-Bridge.wvb` from the normal runtime assembly. Do not
  decode or lower it when an application requires either process-input service.
- Retain and embed the two generated machine leaves separately. Read each
  through a thread-safe path that requires its exact length and SHA-256 before
  it can enter a service bundle.
- Preserve the existing ABI slots, context offsets, failure detail, leaf bytes,
  WVO contracts, and source-reproduction tests. This changes normal-path supply
  and loading only; it does not change process-input semantics.

## Exact generated artifacts

| Leaf | Bytes | SHA-256 |
| --- | ---: | --- |
| Process argument count | 5 | `2358e7e2c72d6476cfe05134db4f0eb5e6987fcca1b10894a8588a28d3929829` |
| Process argument | 70 | `2253e1435f141df5b68f9f7e9e9aa0de448410c42dcf33ad76dcf131afea65d1` |

The retained generator WVB remains 20,800 bytes with SHA-256
`0a4387f12674f08d91682898a27bf84494cbdf886c34542beeb52fd9c4a538da`.

## Evidence and consequences

The affected test was reviewed before execution. It still compiles the exact
Windvale bridge, compares the repository-retained WVB, interprets and lowers
the bridge, independently verifies its x86-64 fragment, compares W^X and linked
WVO execution, and requires both generated leaves to equal the embedded
artifacts. It additionally proves that the normal runtime assembly does not
embed the generator WVB.

The focused Release build succeeds with zero warnings and errors in 32.88
seconds. The single affected contract passes 1/1 in 2.629 seconds. The Windows
and Linux qualification scripts pin both leaf identities; their PowerShell and
Bash syntax checks pass. No Development, Standard, Qualification, or grouped
cross-host gate was run.

Normal process-input service assembly no longer performs managed WVB decoding,
x86-64 lowering, temporary generator publication, invocation, result copying,
or splitting. Together with Decisions 0363 and 0364, the runtime now directly
consumes eight fixed Windvale-generated service leaves. The three
variable-input enum-metadata and publication-planner WVBs, managed
service-bundle assembly, final W^X ownership, Linux evidence, and the grouped
retirement gate remain open.

## Reconsideration triggers

Regenerate either leaf only from the named Windvale stencil contract and record
any identity change explicitly. Do not return the generator WVB to the normal
runtime; preserve it as independent qualification and recovery evidence until
the final Stage 0 archive is complete.
