# Decision 0485: Native WVHV publisher admission applications

- Status: Implemented current-host candidate; dual-host qualification pending
- Date: 2026-08-09
- Advances: [Decision 0484](0484-Native-WVHV-Publisher-Admission-Lowering.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contracts: [hosted read-only binary applications](../../Specifications/Windvale-Hosted-Verifier-Application.md) and [publisher application admission](../../Specifications/Windvale-Native-Hosted-Verifier-Publisher-Application-Admission.md)

## Context

The publisher admitter already had exact non-circular Windvale admission and a
native WVO, but no truthful hosted application identity. Reusing compiler or
container profile `7` would conflate unrelated roles, and following a separate
read-only process with host-side copying would introduce a snapshot race.

## Decision

- Allocate `WVHV 1` profile `8` to exact hosted-verifier publisher admission.
- Retain exactly five read-only capabilities and six services by using a fixed
  success line rather than importing text concatenation and integer formatting.
- Pass the expected profile explicitly through metadata, runtime, layout,
  platform, startup, and container admission. Existing APIs and command forms
  remain strict profile `2`; the constructor tools require the literal
  `publisher-admission` mode for profile `8`.
- Reproduce paired PE/ELF applications through the native linker and existing
  hosted-container construction tools, then pin their exact identities in a
  separate two-artifact candidate.
- Keep construction and read-only admission in the existing focused native
  publisher lane. Durable mutation remains a separate future transaction.

## Evidence and consequences

The native source front door produces a 30,778-byte WVB with SHA-256
`c6ba933fa0ea1068f02235f75ed251655b10b43d64f8984d22b548f01608af0d`.
Native lowering produces a 555,690-byte WVO with SHA-256
`722d819152d8415487c1cf111474fd11dd0ab89a863e33ab84c865a2e3e13771`;
linking places `Main` at zero in a 554,354-byte fragment with SHA-256
`356a9bbf2c3ce3d7c959cbf5276a7840bad109e1a563b66bc20b4d5d98ea76fe`.

The exact applications are 570,368 Windows bytes with SHA-256
`7f58a5e321d1b4baa16ba673b3e0e1c21c9acd040cba92dae0f180d629c63e6b`
and 569,344 Linux bytes with SHA-256
`9bfe16fa751e21a32847f5534eff7de18ba74cfe5b714c63fb6a6589d30d7cad`.
Current-host execution admits both exact publisher subjects and rejects target
swaps, same-length wrong digests, and unsupported targets while preserving every
subject. The reviewed `hosted-verifier-publisher-files` filter passes all nine
cases locally (`Suites: 1, Passed: 1, Failed: 0, Cases: 9`) in 55.5 seconds.
The changed-file planner contract also passes all 27 general and 14 native
cases. No C# source or Stage 0 CLI target was added.

This is current-host evidence only. The Linux executable has not yet executed
on Linux in this decision, and read-only admission does not install a file.

## Reconsideration triggers

Replace the launcher-pinned candidate only after paired Windows/Linux evidence
is recorded. Durable promotion requires one immutable snapshot shared by exact
admission and atomic replacement; never implement it as this command followed
by a host copy or rename.
