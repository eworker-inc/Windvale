# Decision 0418: Segmented compiler hosted-package composition

- Status: Implemented Windows composition; Linux execution, complete compiler run, native process-container construction, and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0417](0417-Canonical-Compiler-Image-Transport.md), [Decision 0414](0414-Digest-Bound-Native-Hosted-Container-Composition.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale linking](../../Specifications/Windvale-Linking.md#segmented-compiler-hosted-package-composition)

## Context

The native process front door and canonical image transport produced every
input required by the qualified hosted-container toolset, but no ordinary
launcher connected them. Copying the complete packaging pipeline would create
two large scripts with the same digest pins, services, metadata, layout,
segment, and publication rules.

## Decision

- Extend `Package-Hosted-Wvb.cmd` and `.sh` with one explicit image-input mode.
  It accepts an exact source WVB, canonical fragment prefix, one-through-eight
  count, validated decimal entry, profile, and output. The existing ordinary
  three-argument mode remains unchanged.
- In image mode, reuse the same digest-bound fixed services, enum service,
  source geometry, evidence, metadata, runtime, layout, service-bundle,
  source-set, segment, manifest, and publication processes. Do not lower or
  link the WVB a second time.
- Add `Package-Segmented-Compiler-Wvb` launchers that privately run
  staging producer, segmented image linker, canonical transport, and hosted
  packaging. They parse only the transport process's bounded decimal result;
  no platform script decodes WVB, WVOP, WVO, WVLI, WVSG, or container formats.
- Add paired focused smoke scripts. They package the exact compiler-image
  staging WVB under profile 6 and compare every output byte with the pinned
  recovery candidate.
- Keep the complete 27.5 MiB compiler run for the grouped end gate. This slice
  qualifies composition mechanics on the small exact staging application and
  does not make a complete compiler performance or cross-host claim.

## Evidence and consequences

The reviewed Windows composition completes in 13.1 seconds and reproduces the
exact 851,968-byte application at SHA-256
`967827e4592c23f30e2a70b9a60a43837c1dfec6112584596c09d382058e2752`.
The checked-in focused smoke repeats that exact comparison 1/1 in 11.4
seconds.
The existing ordinary hosted-packaging smoke then passes its exact-output and
invalid-WVB preservation cases 2/2 in 12.4 seconds, proving that the added mode
did not change the normal single-fragment route. Git Bash accepts all five
affected Linux scripts syntactically; genuine Linux execution remains grouped
with the final dual-host gate.

This closes the missing process-level path from a large accepted WVB to a
canonical hosted-package input. The generic console-v3 managed constructor
must remain available until the complete compiler crosses this path and the
end gate passes. The three checked-in process containers also remain
recovery-built candidates until native container reconstruction replaces them.

## Reconsideration triggers

Version the composition when any process result, candidate digest, 4 MiB
fragment geometry, hosted profile, service order, `WVLI`, `WVSG`, segment
limit, or publication transaction changes. Keep image-input validation in the
shared packager and keep binary decoding out of shell scripts.
