# Decision 0928: run shared development meta-verification once

- Date: 2026-09-02
- Status: Accepted and implemented
- Extends: [Decision 0926: classify and bound verification-owner outcomes](0926-Classify-And-Bound-Verification-Owner-Outcomes.md)
- Current contract: [native changed-file verification](../../Specifications/Windvale-Native-Changed-Verification.md)

## Context

Automatic development verification always runs on Linux and adds Windows only
when a changed path requires that host. The routing-plan and GitHub-workflow
verifiers inspect repository contracts, including both host command paths; they
do not execute a host-specific owner. Running them again inside the optional
Windows job duplicated evidence while delaying the overall gate.

The first structured dual-host run, GitHub Actions run `33688191414`, measured
the shared routing verifier at 34.4 seconds on Linux and 40.1 seconds on Windows.
The affected native owner itself took 8.4 seconds on Linux and 2.9 seconds on
Windows. Removing the duplicate Windows meta-verification therefore has a larger
effect than reducing the already-focused owner.

## Decision

- The Linux automatic development job owns shared routing-plan and
  GitHub-workflow verification for the exact source state.
- The conditional Windows development job delegates only those shared checks to
  its required Linux peer. It continues running every selected Windows owner,
  preserving host-specific failure evidence and timing.
- Restrict `-SharedVerificationOnLinux` to a GitHub Actions Windows development
  process. Local runs, Linux jobs, lightweight/website scopes, and any other use
  reject the switch.
- Keep both development jobs as required inputs to the aggregate verification
  gate. A failed Linux shared verifier therefore remains blocking even when the
  Windows owner passes.
- Do not apply this delegation to qualification evidence or to a verifier that
  begins executing host-specific behavior.

## Consequences

A verification-framework change still receives one complete shared contract
check and both selected host-owner checks, while the Windows critical path avoids
about 40 seconds of repeated work on the measured run. Timing artifacts remain
per host and record only work actually executed by that job.

## Reconsideration triggers

Revisit this boundary if a shared verifier gains host-specific behavior, Linux
is no longer mandatory for every development scope, or measurements show a
better single-owner placement.
