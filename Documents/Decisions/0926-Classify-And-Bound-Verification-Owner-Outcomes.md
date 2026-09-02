# Decision 0926: classify and bound verification-owner outcomes

- Date: 2026-09-02
- Status: Accepted and implemented
- Extends: [Decision 0924: use one PowerShell verification-owner runner](0924-Use-One-PowerShell-Verification-Owner-Runner.md)
- Current contract: [native verification owners](../../Specifications/Windvale-Native-Verification-Owners.md)

## Context

A nonzero verification process previously collapsed assertion failures,
timeouts, missing tools, stream failures, malformed summaries, and orchestration
defects into the same red result. That made a broken verifier look like evidence
that product code was wrong. Owners also had no declared duration policy, so a
small changed-file selection could begin work without exposing that its expected
cost exceeded the ten-minute local development budget.

Automatic development feedback and release qualification have different safety
needs. Development should not claim that code failed when infrastructure could
not complete the check. Qualification must fail closed whenever required
evidence is absent.

## Decision

- Version the owner registry to include one duration-profile name per owner.
  Duration profiles declare conservative expected seconds, enforced maximum
  seconds, and at most one retry for retryable infrastructure failures.
- Make `-PlanOnly` report per-owner and aggregate expected and maximum duration.
  Refuse execution when aggregate expected duration exceeds ten minutes unless
  `-AllowLongRun` explicitly names an approved longer or qualification run.
- Terminate an owner process tree at its maximum total duration, including any
  infrastructure retry, and bound the termination settle time. A timeout is
  never a passing result and is not retried.
- Classify runner outcomes as `passed`, `test-failed`, `timed-out`, or
  `framework-error`. Retry once only when the stream boundary identifies a
  retryable process-launch, stream-I/O, or process-status publication failure;
  never retry an assertion, nonzero owner exit, output-limit violation, summary
  mismatch, or timeout.
- Emit a bounded structured JSON result when `-ResultPath` is supplied. Preserve
  the existing human-readable live output and exact terminal-summary oracle.
- In automatic GitHub development jobs, a classified infrastructure error or
  timeout produces `verification-incomplete` timing evidence and a warning but
  does not assert that product code failed. Test failures remain blocking.
- Keep explicit qualification fail-closed. Its shards pass `-AllowLongRun`, and
  every non-passing outcome fails the job. Development and qualification retain
  their structured reports as short-lived workflow artifacts.

## Consequences

Ordinary local runs reveal their estimated cost before work begins, and an
accidental complete run cannot silently consume hours. GitHub can distinguish a
product failure from missing verification evidence. A nonblocking development
infrastructure result is not a pass and cannot be reused or promoted as release
evidence; explicit qualification still requires every selected owner to pass.

The initial duration profiles are conservative planning policy rather than
performance claims. Structured run artifacts provide the measurements needed to
move owners between profiles without guessing or copying volatile timing tables
into documentation.

## Reconsideration triggers

Revisit the profiles after representative Windows and Linux timing histories are
available, when a Windvale-native process supervisor can replace the Node stream
boundary, or if GitHub supports a first-class required `incomplete` conclusion
that preserves the same fail-closed qualification semantics.
