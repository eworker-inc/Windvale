# Decision 0399: Standalone native hosted-container metadata

- Status: Implemented candidate; native request evidence and pipeline integration pending
- Date: 2026-08-08
- Advances: [Decision 0398](0398-Standalone-Native-Hosted-Container-Runtime.md), [Decision 0383](0383-Windvale-Owned-Hosted-Tool-Metadata-Construction.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native hosted-container metadata constructor](../../Specifications/Windvale-Native-Hosted-Container-Metadata.md)

## Context

Windvale already owned validation and every canonical byte of hosted metadata.
Normal packaging still used C# to invoke the retained fragment, admit `WVHD 1`,
and extract the raw `WVH* 1` record. Decision 0398's standalone runtime-header
producer could consume that raw record, but no native process produced it.

The current `WVHM 1` request contains actual fragment and service extents plus
their SHA-256 values. Moving constructor invocation must not be confused with
retiring the separate trust boundary that acquires and verifies those resources.

## Decision

Add `Native-Hosted-Container-Metadata-Tool.wv` as a focused hosted shell over
the existing metadata-construction and metadata-admission modules. It reads one
exact `WVHM 1` request, admits only a complete successful `WVHD 1` response,
revalidates the returned metadata, and writes the raw 1,024-byte record expected
by `wvhostruntime`.

Expose exact Windows/Linux targets through `windvale compile` and
`windvale aot`. Reuse the shared deletion-bound package builder and existing
compiler-authority host envelope. Keep native request construction from actual
fragment/service evidence as the next explicit boundary.

## Exact local evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Metadata constructor WVB | 26,748 | `196c233ec549872204c5fcfa1c8fc275dba7ff339264de428be7ce72621a2333` |
| Windows metadata constructor | 252,928 | `f4cb8689757f1c93c8da77fa109bcb7d0e0bfd9148de54ee7c88aa03f456955e` |
| Linux metadata constructor | 253,952 | `d95e843f862f20c2b027cd7b335b8ccb683a2589b14818027cb6eabd689a782a` |

The reviewed metadata test passes 1/1 in 5.189 test seconds after a 9.18-second
zero-warning build. It pins both packages, exercises the public CLI target,
matches the frozen Stage 0 oracle exactly, observes no CLR load, preserves an
existing output after request corruption, rejects an alias, and rebuilds the
WVB through the native front door. No broader verifier was run.

## Consequences

- A canonical metadata request can now flow through a native process directly
  into Decision 0398's raw metadata input.
- Metadata policy remains in the existing focused portable modules; the hosted
  shell is 106 lines.
- Decision 0400 now produces one immutable service-bundle segment response;
  native resource acquisition, ordered bundle requests, metadata-request
  construction from that evidence, segment requests, complete composition,
  Linux execution, promotion, and the grouped gate remain.

## Reconsideration triggers

Version the command if `WVHM`, `WVHD`, `WVH*`, profile mapping, metadata extent,
or raw runtime input changes. Do not accept projected digests as proof of the
underlying resource bytes; the next boundary must bind them independently.
