# Decision 0314: Fixed native publisher rejections

- Date: 2026-08-06
- Status: Implemented current-host candidate; grouped Windows/Linux qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0214](0214-Exact-Native-Wvb-Publication-Step.md), [Decision 0307](0307-Native-Console-Application-Publication.md), [Decision 0308](0308-Native-Wvo-Publication.md), and [Decision 0313](0313-Fixed-Native-Console-Packager-Rejections.md)
- Contract: [Native publisher rejection tests](../../Specifications/Windvale-Native-Publisher-Rejection-Tests.md)

## Context

The native console-application and WVO publishers already share complete
portable admission followed by the qualified atomic publication transaction.
Their managed tests prove invalid-candidate destination preservation and scratch
cleanup, but no permanent fixed command owned those two pre-replacement outcomes.

Packaging and lowering rejection are separate boundaries: they prove that a
candidate is not produced. Publisher rejection must instead pass invalid bytes
through each digest-bound publisher launcher and prove the shared transaction
never mutates the destination or leaves scratch.

## Decision

- Add one no-argument `Test-Publisher-Rejections.cmd` / `.sh` coordinator with
  distinct console-application and WVO cases.
- Reuse the existing bad-magic WVO bytes as the invalid candidate and the
  canonical return-42 WVO bytes as the destination sentinel. Pin both complete
  decoded identities before invoking a publisher.
- Give the bytes host-appropriate `.exe`/`.elf` names for console admission and
  `.wvo` names for object admission; use only the public digest-bound launchers.
- Require exit `1`, empty standard output, the exact LF-terminated phase report,
  complete destination preservation, and no `.wvpublish-*` scratch file.
- Keep successful replacement in the existing fixed compositions. Keep shared
  hard-link, concurrency, injected-fault, directory-durability, and indeterminate
  completion coverage in the qualified transaction evidence rather than
  duplicating it here.

## Evidence and consequences

- The invalid candidate is 479 bytes at SHA-256
  `0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288`;
  the 479-byte sentinel is
  `0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5`.
- Direct Windows execution passes both cases in about 1.1 seconds. The complete
  diagnostic hashes are
  `39db034713225109f62c272db447d75cfe93ff0c259c8d9e5211f0df5c007e1f`
  for console-application admission and
  `e7a127a800310d9fbaf8b511b20c7b8184159521dec1be56b641793939a5c69f`
  for WVO admission.
- The reviewed focused selection
  `native publishers reject invalid candidates without changing destinations`
  passes 1/1 in 0.678 test seconds after a 15.33-second zero-warning Release
  build; the complete command takes 20.5 seconds.
- The permanent command invokes no .NET process and reconstructs no publisher.
  No implementation, artifact, WebAssembly, compiler, or format bytes changed.
- Linux execution, grouped qualification, native host-container construction,
  promotion, and the broader publication-fault transfer remain.

## Reconsideration triggers

Add another publisher case only when it covers a distinct admission or
transaction phase. Generated transaction faults require a separately bounded
adapter and must preserve explicit completed, rejected, and indeterminate
outcomes rather than being represented as more input fixtures.
