# Decision 0318: Fixed native WVA assembler rejection families

- Date: 2026-08-06
- Status: Implemented current-host evidence; Linux execution pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0220](0220-First-Native-Wva-Assembler-Front-Door.md), and [Decision 0317](0317-Fixed-Native-Wvb-To-Wvo-Rejections.md)
- Contract: [Native WVA assembler rejection tests](../../Specifications/Windvale-Native-Wva-Assembler-Rejection-Tests.md)

## Context

The ordinary digest-bound WVA assembler is already cross-host qualified and its
native application proves one malformed-header rejection. The managed assembler
suite still owns a wider rejection matrix, including every stable diagnostic
family and many instruction-, register-, number-, label-, and section-specific
spellings.

Moving every managed assertion line for line would duplicate the recovery oracle
and make the native test boundary difficult to review. One fixed representative
per public diagnostic family gives permanent deterministic status, location,
counter, and destination-preservation evidence while leaving the broader corpus
as independent evidence.

## Decision

- Add ten compact LF-terminated WVA fixtures representing `WVA1001` through
  `WVA1010`, with complete byte length and SHA-256 identities.
- Generate the `WVA1011` one-byte-over-source-limit input as a temporary
  1,048,577-byte zero-filled file. Pin its complete identity rather than adding a
  very large source fixture to the repository.
- Add one no-argument `Test-Assembler-Rejections.cmd` / `.sh` coordinator that
  invokes only the ordinary digest-bound assembler launcher.
- For each family require exit `2`, empty standard output, the exact complete
  diagnostic report, and byte-for-byte preservation of the canonical return-42
  WVO destination sentinel.
- Retain the managed malformed corpus for recovery, differential, hostile-input,
  and complete instruction-surface evidence until the final retirement gate.

## Evidence and consequences

- All ten committed input identities and eleven report identities are normative
  in the linked test contract. The generated oversized input has SHA-256
  `2cb74edba754a81d121c9db6833704a8e7d417e5b13d1a19f4a52f007d644264`.
- Direct Windows execution passes 11/11 in 2.929 seconds. The reviewed focused
  selection
  `native WVA assembler rejection families preserve existing output without .NET`
  passes 1/1 in 2.865 test seconds after a 17.20-second zero-warning Release
  build; the complete command takes 25.1 seconds.
- The permanent coordinator invokes no .NET process, rebuilds no assembler, and
  constructs no successful WVO. No product implementation, artifact, WebAssembly,
  WVA semantic, or WVO format bytes changed.
- Linux execution of this exact matrix and the grouped end-of-goal gate remain.
  This decision does not reopen the already-qualified ordinary assembler route.

## Reconsideration triggers

Add another fixed source only when it covers a distinct security boundary or
observable contract not represented by the family matrix. Keep dense
instruction-spelling and mutation coverage in the independent differential suite
instead of growing this command into a second assembler conformance harness.
