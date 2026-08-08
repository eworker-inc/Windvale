# Decision 0382: Windvale-owned hosted-tool runtime header

- Status: Accepted current-host normal-path construction transfer; advanced by [Decision 0398](0398-Standalone-Native-Hosted-Container-Runtime.md)
- Date: 2026-08-08
- Advances: [Decision 0381](0381-Windvale-Owned-Native-Byte-Result-Admission.md), [Decision 0163](0163-Bounded-Hosted-Compiler-Runtime-Data.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native hosted-tool runtime header](../../Specifications/Windvale-Native-Hosted-Tool-Runtime-Header.md)

## Context

Native source reconstruction, lowering, linking, service leaves, and simple
console packaging already compose, but every hosted compiler-family PE and ELF
still receives its initial 4 KiB runtime header from C#. That shared writer
owned five ABI tables, target-specific initial values, metadata placement, and
reserved bytes. Reconstructing a complete hosted tool without Stage 0 requires
removing this common constructor before duplicating work in each outer
container.

## Decision

- Define exact `WVHR 1` and `WVHS 1` envelopes for one target, one of the six
  implemented hosted profiles, and one already verified 1,024-byte metadata
  record.
- Keep metadata admission separate from runtime-header construction. Windvale
  validates the complete fixed directory shape, then constructs the exact
  context, service table, output table, file tables, metadata extent, and zero
  tail.
- Make the normal hosted application builders consume one digest-bound,
  service-free WVNF and independently verify the returned header before any
  PE/ELF construction.
- Move the former C# byte writer behind the explicit
  `Hostedˉcompilerˉruntimeˉheaderˉstage0ˉoracle` name. It exists only for
  recovery and differential evidence and is never called by normal packaging.
- Treat the small managed request/response bridge and current C# test harness
  as temporary migration code. They must move out of the normal tree when the
  native host-container constructor and native qualification runner consume
  this contract directly.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Metadata-admission WVB | 10,550 | `e43c712431e386eba159cd17f87b279cc4a4b5b99084d3a738a3718633099c78` |
| Runtime-header core WVB | 18,911 | `700efbbad9619b58d06561be3e805e18b5498f1e13881646e6e121c2b8ab7564` |
| Retained bridge WVB | 18,864 | `0bbf1c0e5c67c14b3e90bef5243d9c5aea64b3343ad11cfd3f7f93067648fe3d` |
| Retained bridge WVNF | 190,709 | `31e7b98c738972b4f9b23075d48bb1724aac229e5f77d8e517877b5b5733dfe4` |

## Evidence and consequences

The reviewed focused owner case reproduces all source/WVB/WVNF identities,
confirms no constructor WVB is embedded, compares interpreter and native
execution, rejects ten malformed envelopes, covers all six profiles for both
targets, and compares every successful byte against the frozen Stage 0 oracle
and the normal hosted-runtime verifier. It passes 1/1 in 3.423 seconds. The
existing real console-packager PE/ELF materialization case also passes 1/1 in
6.061 seconds, preserving the pinned 708,608-byte Windows and Linux identities.
The Release test application builds with zero warnings and errors.

The exact compiler, Development, Standard, Qualification, Linux-host
execution, and broader hosted gates were not run under the goal's
deferred-broad-verification rule.

Normal hosted-tool runtime-header bytes are now Windvale-owned. C# still
projects the existing `WVH* 1` metadata, invokes and verifies the retained
fragment, constructs the outer PE/ELF, and builds the service bundle. The next
host-container slice should transfer metadata construction and then compose
the Windows/Linux outer container around the already native WVO and service
bundle. No new C# introduced here is a permanent product dependency.

## Reconsideration triggers

Version the request if the hosted profile directory, metadata size, ABI table
versions, initial target state, arena bounds, or runtime-header extent changes.
Do not add live pointers or host handles to retained artifacts.
