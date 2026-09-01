# Decision 0369: Windvale-owned native output leaves

- Status: Accepted current-host normal-path C# emitter removal; Linux execution and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0364](0364-Direct-Fixed-Native-Service-Leaf-Consumption.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native execution context](../../Specifications/Windvale-Native-Execution-Context.md#runtime-private-output-table)
- Advanced by: [Decision 0374](0374-Windvale-Owned-Native-Output-Table.md)

## Context

The console and diagnostic output leaves were the smallest remaining
platform-specific service family still generated in live C# code. One
307-line source emitted and patched the complete Windows and Linux x86-64
instruction streams each time a service bundle was assembled, even though all
four leaf identities were already stable and independently verified.

These leaves are necessarily platform machine adapters, but their source
ownership does not need to remain in .NET. The existing focused Windvale
service-code builder already owns byte emission and relative-branch patching
for the capability-free runtime leaves.

## Decision

- Add one focused portable Windvale module for the 258-byte Windows leaf and
  one for the 213-byte Linux leaf. Each accepts only the console or diagnostic
  output-table field displacement and constructs the established machine
  contract through the shared Windvale service-code builder.
- Add one small bridge that returns Windows console, Windows diagnostic, Linux
  console, and Linux diagnostic leaves in that exact order. Retain its WVB for
  exact source reconstruction, differential execution, and Stage 0 recovery.
- Require the ordinary native source front door to reproduce the same bridge
  WVB. Its project sources use canonical module-name order.
- Embed only the four generated `.bin` leaves in `Runtime/Windvale.Native`.
  Select each through a thread-safe exact-length and SHA-256 check; do not
  embed or execute the generator WVB in the normal runtime.
- Remove the C# x86-64 emitter, label dictionary, relative-branch patcher, and
  platform instruction-construction methods. Preserve all output-table slots,
  ABI registers, partial-write loops, `EINTR` behavior, failure detail, leaf
  bytes, service-bundle placement, and descendant container identities.

## Exact evidence identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows source core WVB | 9,435 | `a072c3dc92b9675d00ac833860c0c7ef7b44cf98d15a3fead38955921d321983` |
| Linux source core WVB | 8,908 | `d3d8c8b660694af7aed52b3f78a650fc6030bfe4ad6d8adc25396ee64ed608ad` |
| Four-leaf bridge WVB | 14,930 | `209b3fad1d03c6f9d08a20e4cfce2511c3af3ed894e1e70e3b32f05ad067ceed` |
| Windows console leaf | 258 | `10f3a500aca7f0236cdf9f6c20658591df88bc612e677264cdaa0bcef59a0a48` |
| Windows diagnostic leaf | 258 | `1b4068c01b2050c3055c78eb82303c71b8488e8766f7b628fab10ffb23e5ffe2` |
| Linux console leaf | 213 | `c5ea073a24c46dd634b1a67a7e7041d476dbce856d058aa8adc2c4e680d3d226` |
| Linux diagnostic leaf | 213 | `1c81018143fa9b708373eaceda62722ca40fb1e11b20808f765fe5ece33406fe` |

## Evidence and consequences

The affected tests were reviewed before execution. The new focused contract
compiles and pins both source cores and the bridge, compares the retained WVB,
requires the reference interpreter and verified x86-64 backend to generate all
four exact leaves, proves the runtime embeds those leaves but not the bridge,
and reconstructs the bridge through the ordinary native source front door. It
passes 1/1 in 1.334 seconds.

The existing runtime-service owner still covers authorization, unsupported
channels, real current-host output, write failure, deterministic identities,
and native error mapping; it passes 1/1 in 0.740 seconds. The final focused
Release build succeeds with zero warnings and errors in 7.36 seconds. Both
qualification scripts pin the source, bridge, and leaf identities; only their
syntax is checked in this slice. No Development, Standard, Qualification, or
grouped cross-host gate was run.

The normal runtime no longer constructs output-service instructions in C#.
Its remaining managed responsibilities include exact artifact loading,
platform output-table binding, file-input and file-output leaf construction,
service-bundle assembly, executable-memory ownership, contexts, arenas, and
invocation. Linux leaf bytes are reproduced and verified here, but real Linux
execution remains part of the deferred grouped gate.

## Reconsideration triggers

Change a leaf identity only through an explicit platform-adapter contract and
complete descendant qualification. Keep the bridge WVB as recovery evidence
until the final digest-bound Stage 0 archive exists. Replace the managed
artifact selector only when a native host owner enforces the same exact
identity and service-selection boundary.
