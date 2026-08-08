# Decision 0396: Standalone native hosted-container platform bytes

- Status: Implemented candidate; advanced by [Decision 0397](0397-Standalone-Native-Hosted-Container-Startup.md)
- Date: 2026-08-08
- Advances: [Decision 0395](0395-Standalone-Native-Hosted-Container-Planner.md), [Decision 0385](0385-Windvale-Owned-Hosted-Container-Construction.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native hosted-container platform bytes](../../Specifications/Windvale-Native-Hosted-Container-Platform-Bytes.md)

## Context

Decision 0395 introduced a native process that produces the exact hosted
container plan. The next materialization inputs—PE/ELF header bytes, Windows
imports, and Windows relocation bytes—were already Windvale-owned, but normal
Stage 0 construction could obtain them only by dispatching retained
service-free fragments in process.

Copying those format rules into a new command would create a second semantic
implementation. Leaving them embedded would prevent the planner, segmenter,
and publisher from forming a complete no-.NET process pipeline.

## Decision

Add `Native-Hosted-Container-Platform-Bytes-Tool.wv` as a focused hosted shell
over the existing portable Windows and Linux constructors. It reads one
successful `WVCD 1` plan, selects the constructor by the plan's admitted target,
validates the exact `WVWB 1` or `WVLB 1` response envelope, and writes the
complete target-owned response without loading .NET.

Expose exact Windows/Linux targets through `windvale compile` and
`windvale aot`. Reuse the shared deletion-bound hosted-tool package builder and
centralize its exact application-identity check so planner and platform-byte
targets do not carry duplicate C# validation code. Keep response validation as
explicit checks accepted identically by the frozen reference compiler and the
ordinary native compiler.

## Exact local evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Platform-byte WVB | 29,793 | `3cce3e2d548be4f9304a6e6ae62355d42b2879c4fe837283fb8415ea4d715732` |
| Windows platform-byte producer | 309,760 | `46db452f1356dadb93bf80d4a81a34cf73e02d0f45342309700b5892ea571f7b` |
| Linux platform-byte producer | 311,296 | `cf09c62056d4960e914504779973a7227bcb2d9879c4328496adb859f83c526d` |

The reviewed platform-byte test passes 1/1 in 5.726 test seconds after a
15.11-second zero-warning build. It pins both packages, exercises the public
CLI target, matches the current host's retained fragment exactly, observes no
CLR load, preserves an existing output on rejection, rejects an alias, and
rebuilds the WVB through the native front door. The reviewed planner regression
passes 1/1 in 5.495 test seconds after a 10.74-second zero-warning build. No
broader verifier was run.

## Consequences

- Outer PE/ELF header, import, and relocation production now has a native
  process boundary on both permanent hosts.
- Platform construction semantics remain in the existing portable Windvale
  modules; the new source owns only process policy and response admission.
- Shared deletion-bound C# package wiring now owns one identity validator rather
  than repeating it for every transition tool.
- Decision 0397 now adds exact startup instantiation; remaining runtime/resource
  production, full process composition, Linux execution, promotion, and the
  grouped gate remain open.

## Reconsideration triggers

Version the command if `WVCD`, `WVWB`, `WVLB`, target numbering, platform-owned
payload extents, or failure behavior changes. Do not combine startup or final
publication into this tool unless the resulting resource and mutation boundary
remains independently reviewable.
