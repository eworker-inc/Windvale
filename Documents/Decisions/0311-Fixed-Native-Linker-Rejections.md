# Decision 0311: Fixed native linker rejections

- Date: 2026-08-06
- Status: Implemented current-host candidate; grouped Windows/Linux qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0218](0218-First-Native-Test-Orchestration.md), [Decision 0302](0302-Digest-Bound-Native-Wvo-Linker-Candidate.md), [Decision 0305](0305-Digest-Bound-Native-Aot-Chain-Test.md), and [Decision 0310](0310-Fixed-Native-Wvo-Test-Cases.md)
- Contract: [Windvale native linker](../../Specifications/Windvale-Native-Wv-Linker.md)

## Context

The fixed native AOT chain already proves one successful link, but repeating the
whole source-to-executable sequence for linker rejection coverage would waste
time and blur ownership. Invalid base, missing entry, and malformed-object
behavior remained available only through managed orchestration even though the
digest-bound native linker already exposes deterministic failure reports.

The repository now has exact accepted and bad-magic WVO fixtures. A focused
coordinator can reuse those immutable inputs, call only the pinned linker, and
check both diagnostics and publication safety without a C# oracle.

## Decision

- Add no-argument `Test-Linker-Rejections.cmd` and `.sh` coordinators. Keep them
  separate from `Test-Aot-Chain` so rejection checks do not rebuild source,
  lower, package, or execute an application.
- Decode and identity-check the existing canonical return-42 and bad-magic WVO
  fixtures before any case runs.
- Fix three cases: invalid base (`WVL1001`), missing entry (`WVL1007`), and
  malformed object (`WVL1002`). Require exit `2`, empty standard output, and the
  SHA-256 of the complete LF-terminated diagnostic.
- Before each case, copy the bad-magic object to the requested output path as a
  sentinel. Require its complete identity to remain unchanged after rejection.
- Keep complete resolution, layout, relocation, hostile-input, randomized, and
  concurrency coverage in the managed evidence lane until separately moved.

## Evidence and consequences

- Direct Windows execution passes all three cases and reports exactly
  `Tests: 3, Passed: 3, Failed: 0` in about 1.2 seconds.
- The reviewed focused selection
  `native linker rejections preserve existing output without .NET` passes 1/1
  in 0.815 test seconds after a 15.25-second zero-warning Release build; the
  complete command takes 20.7 seconds.
- The expectations come from the native linker's exact reports and pinned
  repository fixtures. No result is generated or compared through .NET while
  the permanent command runs.
- No linker semantics, WebAssembly implementation, compiler source, or candidate
  artifact changed. Linux execution, grouped qualification, promotion, and the
  broader linker-corpus transfer remain.

## Reconsideration triggers

Add another fixed rejection only when it closes a distinct durable linker
boundary. Use a separately specified bounded generated corpus if representative
fixed cases stop being adequate; do not turn this command into a general test
language or make the end-to-end AOT chain the rejection-test setup.
