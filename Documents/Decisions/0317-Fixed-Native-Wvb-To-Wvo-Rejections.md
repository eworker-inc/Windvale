# Decision 0317: Fixed native WVB-to-WVO rejections

- Date: 2026-08-06
- Status: Implemented current-host candidate; grouped Windows/Linux qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0224](0224-First-Native-Wvb-To-Wvo-Front-Door.md), [Decision 0304](0304-Digest-Bound-Native-Wvb-To-Wvo-Candidate.md), and [Decision 0308](0308-Native-Wvo-Publication.md)
- Contract: [Native WVB-to-WVO rejection tests](../../Specifications/Windvale-Native-Wvb-To-Wvo-Rejection-Tests.md)

## Context

The digest-bound lowerer already reproduces the fixed accepted-subset object and
publishes it through complete portable WVO admission plus native atomic
replacement. Its managed evidence also proves malformed-input rejection and
destination preservation, but no permanent focused command owned that failure.

Malformed WVB and valid-but-unsupported WVB are different boundaries. The first
must fail complete WVB admission; the second must remain a valid module while
the bounded backend fails closed. Neither requires rebuilding the lowerer or
rerunning the successful source-to-AOT chain.

## Decision

- Add one no-argument `Test-Lowerer-Rejections.cmd` / `.sh` coordinator with a
  malformed case and a valid unsupported-function case.
- Reuse the existing 174-byte bad-magic WVB fixture and the committed 1,698-byte
  decimal-parsing WVB. Pin each complete identity before invoking the launcher.
- Reuse the canonical 479-byte return-42 WVO as the destination sentinel.
- Require exit `1`, empty standard output, the exact LF-terminated native status,
  complete destination preservation, and no residual private lowerer work.
- Keep successful lowering in the existing fixed front door and AOT composition.
  Keep the broader malformed and unsupported matrix in independent managed
  evidence until separately transferred or covered by the final grouped gate.

## Evidence and consequences

- The bad-magic input has SHA-256
  `20618498d9df059d52fc0d660bf52f32df291c88b94d4b5ded224078f936108e`;
  the valid unsupported input has
  `bb120d1098855b8b4adced6bcd1b1ab695f115e76bebdacb19a2b07b798cad37`;
  and the destination sentinel has
  `0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5`.
- The complete malformed report has SHA-256
  `6dc739ce9e8c752efe41fbede32d6c373ea33e1c22159faf86772a4cc94ff323`;
  the unsupported-function report has
  `fc854d5370fe6da10243d8e28663f932baa4d7c30402488f5193d0a3dad77ded`.
- Direct Windows execution passes both cases in 0.788 seconds. The reviewed
  focused selection
  `native WVB-to-WVO rejections preserve existing output without .NET` passes
  1/1 in 0.867 test seconds after an 11.33-second zero-warning Release build; the
  complete command takes 17.0 seconds.
- The permanent command invokes no .NET process and reconstructs no lowerer,
  object, linker, or application. No implementation, artifact, WebAssembly, or
  format bytes changed in this slice.
- Linux execution, grouped qualification, native host-container construction,
  promotion, and broader lowerer-rejection transfer remain.

## Reconsideration triggers

Add another case only when it covers a distinct WVB admission or native-subset
status. Do not multiply fixtures that produce the same status without adding a
separate security, preservation, or bounded-resource contract.
