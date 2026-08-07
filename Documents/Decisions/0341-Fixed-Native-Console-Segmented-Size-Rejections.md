# Decision 0341: Fixed native console segmented-size rejections

- Date: 2026-08-06
- Status: Implemented current-host evidence; Linux execution pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0223](0223-First-Native-Console-Application-Packager.md), [Decision 0307](0307-Native-Console-Application-Publication.md), [Decision 0330](0330-Manifest-Driven-Native-Retirement-Test-Suite.md), and [Decision 0340](0340-Windvale-Native-Hosted-Console-Admission.md)
- Contract: [Native console-application segmented-size tests](../../Specifications/Windvale-Native-Console-Application-Segmented-Size-Tests.md)

## Context

The remaining managed console verifier cases construct one application byte
beyond each version-1 target maximum. Both inputs exceed one ordinary Windvale
`bytes` value and therefore cannot enter the established one-snapshot verifier
or publisher. Replacing them with small malformed applications would discard
the boundary being retired.

The managed verifiers are target-specific, while the portable Windvale
verifier identifies Windows or Linux from input bytes. A permanent test must
therefore preserve the exact total/chunk boundaries and target markers without
claiming byte-for-byte identity with the managed zero arrays.

## Decision

- Add one focused Windvale read-only `wvappverify` source tool over the shared
  portable console-application admission modules.
- Add one hosted profile with two immutable file snapshots. Keep all existing
  verifier, inspector, and runner profile layouts unchanged.
- Give the source tool exactly the five standard read-only process/file
  capabilities and a focused service-bundle constructor. It has no file-write
  authority.
- Expose explicit Windows and Linux AOT target names through Stage 0 so recovery
  can reconstruct the paired host containers without an ad-hoc program.
- Pin the WVB and both host applications in one candidate manifest. Application
  construction remains explicitly `stage0-recovery`; ordinary execution of the
  pinned tools does not load .NET.
- Freeze two target-marked, highly compressible two-chunk inputs. Retain
  `WVW2001` and `WVL2001` as managed recovery provenance, while requiring the
  exact portable status ordering actually reached by those segmented inputs.
- Add one fixed two-case native lane with exact report and input-preservation
  checks. Do not rerun already-passing children through the changed coordinator.

## Evidence and consequences

- The new WVB compiles through Stage 0 in 12.6 seconds to 103,424 bytes at
  SHA-256 `5894bb7180597945f4e4d49e87ae954fb3c2bba84cde4b9cb549a2f168006a91`.
  The native front door first reports its known `Sourceˉbindings` subset limit;
  this construction does not redefine the normal build path.
- The Windows application is 1,041,408 bytes at SHA-256
  `ebc6f54884e3d93ee1fb1f3658a9062167294f3d0e936554cadc499b83bd8111`.
  The Linux application is 1,040,384 bytes at SHA-256
  `5dbd78b3f67cc179e9848eacca6627a03f5f44ddecc6480d2e9ab98d073f792e`.
- The Stage 0 construction tool builds with zero warnings and zero errors. Both
  writers independently reverify the two-snapshot profile, entry, and complete
  native service bundle before accepting an artifact.
- The fixed 8,909-byte archive has SHA-256
  `d0e9aa4f6e31d3bd28fb0468606f43b275c320adb470e4d3b78034d440573200`.
  It expands to the two exact 4,194,304-byte first chunks plus 2,049- and
  8,305-byte second chunks recorded by the contract.
- The reviewed direct Windows command passes 2/2 in 1.5 seconds. Windows returns
  `Invalidˉsize` after target selection; Linux-shaped input returns
  `Invalidˉchunk` at the shared second-chunk bound before target selection. Both
  input pairs remain byte-identical.
- The retirement plan becomes 1,751 LF-only bytes at SHA-256
  `aa8b8680fe87c18186815f6dd7f4f924df86c22ed416b6f69168a1f59a287566`;
  it fixes 21 suites and 3,026 declared cases.

The focused child is not rerun through the changed coordinator. Linux
execution, maximum-size valid application construction, large-native
segmented-object transfer, broader unsafe/WVA evidence, candidate promotion,
Development, Standard, Qualification, and the grouped end-of-goal gate remain
deferred. The managed target-specific tests remain frozen recovery and final
independent evidence.

## Reconsideration triggers

Revise this decision if the 4-MiB first-chunk limit, 8,304-byte second-chunk
limit, target application maxima, two-snapshot runtime profile, portable target
selection, or native diagnostic newline changes. Do not widen one ordinary
Windvale byte value merely to imitate a target-specific managed API. Final
managed-source removal still requires the digest-bound Stage 0 recovery release
and complete Decision 0057 gate.
