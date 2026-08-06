# Decision 0303: Digest-bound native console-packager candidate

- Date: 2026-08-06
- Status: Implemented candidate; native construction, atomic publication, grouped dual-host qualification, and promotion pending
- Advances: [Decision 0223](0223-First-Native-Console-Application-Packager.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native console packager](../../Specifications/Windvale-Native-Console-Packager.md)

## Context

Decision 0223 moves bounded version-1 PE/ELF materialization and verification
into Windvale source. Its exact WVB, Windows application, and Linux application
existed only as ignored loose artifacts, so a clean checkout could not invoke
the candidate without reconstructing it through Stage 0.

The qualified native source front door does not yet accept this complete
project. A direct regeneration attempt fails closed at `Sourceˉbindings`; the
accepted native compiler subset must not be silently widened or treated as the
complete backend. Stage 0 therefore remains the explicit construction lane
while the already-implemented raw-image-to-application operation can be pinned
as a runnable native candidate.

## Decision

- Regenerate the canonical WVB and paired hosted applications through the
  named Stage 0 recovery lane into a clean candidate directory. Record that
  construction owner explicitly in the manifest.
- Pin all three sizes, digests, targets, source relationships, Decision 0223
  provenance, and pending qualification status.
- Add digest-bound `Package-Console.cmd` and `Package-Console.sh` launchers.
  They accept exactly the raw tool's target, native-image path, entry offset,
  and output path; verify the complete current-host packager digest; and then
  execute the Windvale materializer.
- Keep this as a candidate front door. Its current output capability is durable
  create-or-replace but not an atomic publication contract. Normal promotion
  requires a native construction path, a safe publication boundary, and the
  grouped Windows/Linux gate.
- Test only the added boundary: manifest and cross-host digest pins, one
  current-host return-42 package, exact structural recovery of the native bytes
  and entry offset, and deterministic launcher usage rejection. Retain the
  existing package test as the owner of paired reconstruction, cross-target
  materialization, repetition, malformed-input preservation, direct execution,
  and no-CLR process evidence.

## Evidence and consequences

- The canonical WVB is 58,127 bytes at SHA-256
  `7b055d4e6a456680a79eb28eaafa577e0019ea0ff1e34d9e713e9178428acc29`.
- The Windows application is 667,648 bytes at SHA-256
  `a9cd6e222b869d838f563ffc46ae3acbde74ff8beb10c28373b6d5985c8f680f`.
- The Linux application is 667,648 bytes at SHA-256
  `10b1d752ab6c9c7217f833add9ef77ca0d61b6bcc02d7023b1877f42bab2a683`.
- Stage 0 regeneration of the three exact candidate artifacts took 5.1
  seconds. The preceding native-source attempt rejected without publishing an
  output, preserving the accepted compiler boundary.
- The reviewed focused linker selection passes 1/1 in 1.044 test seconds after
  an 11.31-second zero-warning Release build; the complete command takes 17.0
  seconds.
- The current-host native process materializes a structurally valid version-1
  application whose verifier recovers the exact six-byte image and entry zero.
- No PE/ELF semantics, native backend, WebAssembly implementation, or source
  language behavior changed. Development, Standard, Qualification, native
  construction/publication, general hosted packaging, release integration, and
  ordinary-path cutover remain deferred.

## Reconsideration triggers

Regenerate all identities if the packager source, portable construction or
verification contract, native backend, hosted profile, startup, service bundle,
or Stage 0 writer changes. Replace the one-value output boundary if completed
applications above 4 MiB become required; do not raise the global Windvale
`bytes` limit merely to make this candidate broader.
