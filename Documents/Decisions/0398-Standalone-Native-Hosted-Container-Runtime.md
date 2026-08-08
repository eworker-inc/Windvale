# Decision 0398: Standalone native hosted-container runtime header

- Status: Implemented candidate; advanced by [Decision 0399](0399-Standalone-Native-Hosted-Container-Metadata.md)
- Date: 2026-08-08
- Advances: [Decision 0397](0397-Standalone-Native-Hosted-Container-Startup.md), [Decision 0382](0382-Windvale-Owned-Hosted-Tool-Runtime-Header.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native hosted-container runtime-header producer](../../Specifications/Windvale-Native-Hosted-Container-Runtime.md)

## Context

Windvale already owned canonical hosted metadata admission and every byte of the
initial runtime header. Normal packaging still used C# to derive the target and
profile, construct `WVHR 1`, dispatch the retained fragment, admit `WVHS 1`, and
extract the 4,096-byte header later consumed by the container planner.

A command that emitted only `WVHS 1` would leave another adapter between it and
the existing planner. Reimplementing runtime tables in the hosted shell would
duplicate the portable core.

## Decision

Add `Native-Hosted-Container-Runtime-Tool.wv` as a focused hosted shell over the
existing metadata admission and runtime-header construction modules. It reads
one raw canonical metadata record, derives its target and profile, constructs
the exact existing request, admits the exact response and embedded metadata,
and writes the raw 4,096-byte header expected by `wvhostplan`.

Expose exact Windows/Linux targets through `windvale compile` and
`windvale aot`. Reuse the shared deletion-bound package builder and existing
compiler-authority host envelope. Do not fold metadata construction or service
bundle materialization into this source.

## Exact local evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Runtime-header producer WVB | 22,956 | `be7db77c3171c042ab2a740eb9b3e7492d5624d50e35625b9ad07015f5c013e3` |
| Windows runtime-header producer | 244,736 | `b1a653d4fa00bdfd4964e8a2911317b25801484f06462a2d11572d481c3cb198` |
| Linux runtime-header producer | 245,760 | `ca0e23a717b9252b40847e7c976d64178252678db47f0472b6e958186a8466cc` |

The reviewed runtime-header test passes 1/1 in 5.125 test seconds after an
11.66-second zero-warning build. It pins both packages, exercises the public CLI
target, matches a real retained-fragment header exactly, observes no CLR load,
preserves an existing output after metadata corruption, rejects an alias, and
rebuilds the WVB through the native front door. No broader verifier was run.

## Consequences

- A Windvale-produced metadata record can now flow through a native process
  directly into the standalone planner's raw runtime-header input.
- Runtime table and metadata placement semantics remain in the existing focused
  portable modules; the hosted shell is 135 lines.
- Decision 0399 now supplies raw canonical metadata from `WVHM 1`; native
  construction of that request from immutable fragment/service evidence,
  service-bundle production, segment-request orchestration, full composition,
  Linux execution, and the grouped gate remain.

## Reconsideration triggers

Version the command if `WVH*`, `WVHR`, `WVHS`, profile/container mapping, runtime
header extent, or raw planner input changes. Keep metadata construction separate
while its immutable fragment/service evidence remains a distinct trust boundary.
