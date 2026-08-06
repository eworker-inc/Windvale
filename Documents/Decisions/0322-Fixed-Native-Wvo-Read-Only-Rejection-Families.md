# Decision 0322: Fixed native WVO read-only rejection families

- Date: 2026-08-06
- Status: Implemented current-host evidence; Linux execution pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0222](0222-First-Native-Wvo-Read-Only-Front-Door.md), [Decision 0301](0301-Digest-Bound-Native-Wvo-Candidate-Launchers.md), and [Decision 0321](0321-Fixed-Native-Wva-Assembler-Rejection-Families.md)
- Contract: [Native WVO read-only rejection tests](../../Specifications/Windvale-Native-Wvo-Read-Only-Rejection-Tests.md)

## Context

Windvale already owns complete WVO 1.0 admission and exposes it through paired
digest-bound native verifier and inspector launchers. The fixed native Seed plan
covers bad magic, truncation, and trailing bytes, but the managed hostile-object
suite still owns the other stable structural status families.

Copying the complete managed mutation corpus would create a second broad object
harness. One exact representative for each public status family instead fixes
the portable rejection contract while retaining randomized, hostile-size, and
differential coverage as independent recovery evidence.

## Decision

- Add ten compact base64 fixtures and reuse the existing bad-magic, truncated,
  and trailing-byte fixtures to cover all thirteen stable WVO 1.0 rejection
  families.
- Add one no-argument `Test-Wvo-Read-Only-Rejections.cmd` / `.sh` coordinator
  that invokes both `Verify-Wvo` and `Inspect-Wvo` independently for every case.
- Require exit `2`, empty standard output, the same exact complete diagnostic
  report from both launchers, and byte-for-byte preservation of each input.
- Pin every decoded input and complete LF-terminated report by SHA-256. Derive no
  expectation from the managed implementation at test time.
- Retain the managed randomized, hostile-size, and differential object coverage
  until the complete Decision 0057 retirement gate qualifies its native
  replacements and the final recovery archive.

## Evidence and consequences

- The linked contract records all thirteen input sizes and identities plus all
  thirteen complete report identities. The permanent matrix performs twenty-six
  independent digest-bound native launcher calls.
- Direct Windows execution passes 13/13 in 5.724 seconds. After reviewing the
  merged wrapper and exact report, the focused selection
  `native WVO read-only rejection families agree without .NET` passes 1/1 in
  5.620 test seconds after an 11.84-second zero-warning Release build; the
  complete command takes 22.5 seconds.
- The coordinator invokes no .NET process, reconstructs no WVO application, and
  writes no object. No product implementation, candidate artifact, WebAssembly
  implementation, source semantic, or WVO format byte changed.
- Linux execution of this exact matrix and the grouped end-of-goal gate remain.
  This current-host proof does not promote the candidate launchers or remove the
  Stage 0 recovery commands.

## Reconsideration triggers

Add another fixed fixture only for a distinct observable or security boundary
not represented by the family matrix. Keep randomized mutation and large-input
coverage in focused independent tests rather than growing this coordinator into
a duplicate object conformance suite.
