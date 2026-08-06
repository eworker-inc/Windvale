# Decision 0329: Fixed native WVB unsafe rejections

- Date: 2026-08-06
- Status: Implemented current-host evidence; Linux execution pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0229](0229-Native-Malformed-Wvb-Test-Fixtures.md), and [Decision 0230](0230-Native-Typed-And-Control-Malformed-Fixtures.md)
- Contract: [Native WVB unsafe rejection tests](../../Specifications/Windvale-Native-Wvb-Unsafe-Rejection-Tests.md)

## Context

The fixed native plan already contains malformed envelopes, representative
typed-execution corruptions, and one control-reachability corruption. Core
unsafe instruction-stream cases still run only through the managed verifier
test, even though both digest-bound native WVB read-only launchers reject them.

Generating mutations inside each host script would duplicate binary-layout
logic and let a candidate construct its own test inputs. Running the complete
native plan would also repeat unrelated builds and successful execution.

## Decision

- Add `Test-Wvb-Unsafe-Rejections.cmd` and `.sh` as a separate five-case matrix
  over the digest-bound verifier and inspector launchers.
- Derive five fixed 174-byte WVB values from the canonical
  `Wvbˉtoˉwvoˉfixture` module: unknown opcode, truncated immediate, out-of-range
  local index, out-of-range jump target, and instructions after return.
- Store the exact mutations as compact base64 fixtures. The permanent commands
  decode and digest-check them; they do not parse or mutate WVB.
- Require both launchers to return `1`, write no standard output, emit the exact
  semantic or typed-execution phase report, and preserve every input byte.
- Keep broader nominal/limit unsafe cases and deterministic randomized
  containment in their existing differential lane until separately transferred.

## Evidence and consequences

- Each fixture is 174 decoded bytes and 233 LF base64 bytes. Their exact
  mutations and identities are normative in the linked contract.
- The semantic report SHA-256 is
  `4938c6906dfb318e36b11a2795699638f4cdef1bef12c477b5b726137cd3d9b5`;
  the typed-execution report SHA-256 is
  `c083d8e4a7dbe48f3c72248285d6f4ace645202ca8f2013df1d51ab328db7930`.
- Direct Windows execution passes 5/5 in 2.411 seconds. After reviewing the
  merged wrapper, the exact Seed selection
  `native WVB unsafe rejections agree without .NET` passes 1/1 in 2.420 seconds
  after a 9.64-second zero-warning Release build; the complete command takes
  16.4 seconds.
- The permanent command starts no .NET process, rebuilds no artifact, and
  repeats no project compilation or successful native execution. No WVB
  format, verifier implementation, WebAssembly implementation, or candidate
  artifact changes.
- Linux execution and the grouped end-of-goal Windows/Linux Qualification gate
  remain deferred. This local evidence does not promote or delete Stage 0.

## Reconsideration triggers

Regenerate the fixture identities and reports if WVB 1.11, opcode encoding,
verification phases, or either read-only launcher changes. Add a case only for
a distinct unsafe boundary; do not turn this fixed matrix into a duplicate
randomized corpus or a host-side WVB mutator.
