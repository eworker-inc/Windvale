# Decision 0335: Fixed native WVO differential corpus

- Date: 2026-08-06
- Status: Implemented current-host evidence; Linux execution pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0301](0301-Digest-Bound-Native-Wvo-Candidate-Launchers.md), [Decision 0322](0322-Fixed-Native-Wvo-Read-Only-Rejection-Families.md), and [Decision 0330](0330-Manifest-Driven-Native-Retirement-Test-Suite.md)
- Contract: [Native WVO differential tests](../../Specifications/Windvale-Native-Wvo-Differential-Tests.md)

## Context

The managed WVO differential test mutates one canonical valid object 128 times,
then presents 128 arbitrary byte values. It asks the Stage 0 object codec and
the Windvale object scanner only whether each value is accepted. The thirteen
fixed native rejection cases already pin every stable status family, but they do
not prove agreement for valid-shaped mutations that remain accepted.

A permanent replacement should preserve the independent oracle decision without
running the reference framework on every future test. It also should not double
the process count by repeating the same scanner through both read-only modes.

## Decision

- Run the exact managed seed, canonical sample, mutation rule, and continued
  arbitrary-byte sequence once at commit `c183e9a`; freeze all input bytes and
  reference acceptance decisions in a digest-bound manifest.
- Retain the Stage 0 error code/offset for rejected cases as provenance and an
  exact digest-bearing success report for every accepted case.
- Add one Windows/Linux `Test-Wvo-Differential` command over only the public
  native verifier. Require exact agreement on 32 accepted and 224 rejected
  cases plus complete input preservation.
- Keep exact rejection-report mapping in Decision 0322's thirteen-case matrix.
  Do not invoke the inspector or linker again merely to multiply equivalent
  calls.
- Remove the one-time managed generator and build output. Generate nothing and
  start no .NET process during the permanent run.

## Evidence and consequences

- The 32,682-byte oracle manifest covers 256 unique names, 39,872 input bytes,
  91 distinct mutation offsets, 104 arbitrary lengths, 19 Stage 0 rejection
  codes, 32 accepted mutations, and 224 rejections. Every recorded size, input
  digest, and accepted report digest was independently rechecked before native
  execution.
- The reviewed Windows command passes all 256 cases in 53.553 seconds. Every
  accepted mutation emits its exact digest-bearing report; every rejected value
  stays within one native object-status diagnostic; every input is unchanged.
- The retirement plan is now 1,049 LF-only bytes at SHA-256
  `a904dd6063f97363fcef214a498e8046d8d9796c82ab6cf63c5bc073ee7bd4a2`;
  it fixes 13 suites and 786 cases.
- The already-passing child is not rerun through the unchanged manifest wrapper.
  Linux execution, hostile-size WVO coverage, WVA/source differential transfer,
  promotion, and the grouped end-of-goal gate remain.

This slice changes no WVO format, object implementation, candidate artifact,
linker, WebAssembly implementation, or managed reference source.

## Reconsideration triggers

Revise the corpus version and identities if the canonical sample, mutation
count/rule, arbitrary-input bound, reference acceptance contract, native
success report, or public verifier changes. Keep detailed rejection-family
expectations in their focused matrix instead of copying them into every
differential row.
