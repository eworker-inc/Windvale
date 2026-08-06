# Decision 0310: Fixed native WVO test cases

- Date: 2026-08-06
- Status: Implemented current-host candidate; grouped Windows/Linux qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0218](0218-First-Native-Test-Orchestration.md), [Decision 0230](0230-Native-Typed-And-Control-Malformed-Fixtures.md), [Decision 0301](0301-Digest-Bound-Native-Wvo-Candidate-Launchers.md), and [Decision 0308](0308-Native-Wvo-Publication.md)
- Contract: [Windvale native test plan](../../Specifications/Windvale-Native-Test-Plan.md)

## Context

The fixed native plan already transfers successful WVB execution, exact runtime
failures, and representative malformed-WVB rejection without consulting .NET.
WVO verification remained in the managed test suite even after Decisions 0301
and 0308 pinned the native object verifier and shared its complete portable
admission logic with publication.

A bounded transfer should exercise the existing digest-bound verifier without
inventing a dynamic test language, host-specific parser, live C# oracle, or broad
randomized corpus.

## Decision

- Advance the fixed plan to `WVNT 5` and retain every existing WVB case.
- Add one canonical return-42 WVO plus fixed bad-magic, one-byte-truncated, and
  one-byte-trailing corruptions as exact base64 fixtures.
- Pin each complete decoded input identity before execution.
- Send WVO cases only to the digest-bound native WVO verifier. Require exit `0`
  with an empty error channel for the accepted object, or exit `2` with an empty
  output channel for each rejected object.
- Pin the SHA-256 of the complete success or diagnostic report. This preserves
  exact behavior across the host adapters without decoding Windvale report text
  in shell code or generating expectations from C# during the run.
- Keep the plan fixed and digest-bound. Broader malformed, randomized, unsafe,
  serialization, linker, and execution coverage remains outside this slice.

## Evidence and consequences

- `Tests/Native/Plan.txt` is 4,742 UTF-8/LF bytes at SHA-256
  `6ad262319aad1b9df3c9e211fd1e01ed509d8e00beff0de8004642e2928457de`.
  The Windows and Linux adapters now own 26 fixed cases.
- The reviewed focused selection
  `native test orchestration runs the pinned WVB and WVO plan` passes 1/1 in
  7.260 test seconds after a 12.17-second zero-warning Release build; the
  complete command takes 24 seconds. All 26 inner native cases pass.
- The run invokes no .NET command and does not compare WVO output with a live C#
  implementation. Input and report identities are repository-owned constants.
- No WebAssembly implementation changed. Direct Linux execution, grouped
  Windows/Linux qualification, promotion, and broader WVO corpus transfer remain.

## Reconsideration triggers

Introduce a separately specified bounded dynamic plan only when the fixed
inventory becomes an actual maintenance constraint. Add randomized or generated
malformed WVO data only with deterministic seeds, explicit limits, and a native
oracle boundary that does not restore .NET to the normal test path.
