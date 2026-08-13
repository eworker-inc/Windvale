# Decision 0353: Native compiler-image staging source build

- Status: Accepted current-host source-build evidence; Linux execution, native container reconstruction, and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0352](0352-Digest-Bound-Compiler-Image-Staging-Applications.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale linking](../../Specifications/Windvale-Linking.md#hosted-immutable-snapshot-staging-boundary)
- Advanced by: [Decision 0354](0354-Native-Compiler-Image-Staging-Reconstruction.md)

## Context

Decision 0352 pinned the compiler-image staging WVB and its Windows/Linux
application candidates, but its focused package writer still obtained the WVB
through the feature-frozen Stage 0 compiler. The staging implementation was
already Windvale source; without an ordinary native project closure, changing
or reconstructing that source would retain an unnecessary managed build step
before the separate host-container construction debt.

The native source front door currently consumes dependency entries in
canonical ordinal module-name order. The project contract promises canonical
source composition, so the checked-in closure must state that order explicitly
until the build driver derives it itself.

## Decision

- Add `Projects/Linker/Windvale-Compiler-Image-Staging.wvproj` as the complete source closure
  for the hosted staging root, segmented linker, independent verifier, strict
  `WVOP` and `WVLI` contracts, and staging resource policy.
- Keep every non-root source in canonical ordinal module-name order. Do not
  introduce an alternate generated source list or another compiler-specific
  orchestration file.
- Require `Tools/Native/Build-Wvb.cmd` and its Linux sibling to remain the
  ordinary source-to-WVB front doors for this project.
- Compare the complete native output byte for byte with the frozen Stage 0
  oracle and independently verify its module name, hosted profile, and six
  declared capabilities.
- Keep Stage 0 only as differential/recovery evidence for this source-build
  boundary. This slice does not replace the separate C# PE/ELF package writer,
  promote either application, or claim Linux execution.

## Exact identity

The native source front door publishes the existing 75,337-byte WVB at
SHA-256
`855983284c088cd795c119fe0c392308824066b10a9173dceb7cdc2daa219101`.
It contains 73 functions and 61,712 code bytes and is byte-identical to the
Stage 0 recovery oracle used by the digest-bound application contract.

## Evidence and consequences

The first focused attempt failed closed at `Sourceˉbindings`, function zero,
operation zero, without publishing an output. The project sources were not in
canonical module-name order. Reordering only those entries made the direct
native build publish the exact pinned identity in 3.0 seconds.

The already-reviewed named test then passed 1/1 in 2.803 test seconds using the
existing zero-warning Release build. It starts with a nonempty destination,
requires successful native replacement, compares every output byte with the
Stage 0 oracle, and verifies the hosted module metadata. No broader test level
was run because the relevant Release build and product sources were unchanged
after the earlier focused failure.

This removes Stage 0 from construction of the staging tool WVB. Stage 0 still
constructs the 849,920-byte Windows and 851,968-byte Linux host containers.
Linux source-build/execution evidence, publisher-scale transfer, durable public
publication, canonical map output, native host-container construction,
promotion, Development, Standard, Qualification, and the grouped retirement
gate remain deferred.

## Reconsideration triggers

Remove the explicit dependency-order requirement when the native project
front door canonically sorts and binds an arbitrary valid project source list.
Revisit this project when the staging root, imported modules, capability set,
WVB identity, or hosted profile changes. Do not refresh the pinned WVB without
independent verification and an explicit replacement decision.
