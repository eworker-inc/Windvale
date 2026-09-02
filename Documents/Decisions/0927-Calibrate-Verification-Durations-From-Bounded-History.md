# Decision 0927: calibrate verification durations from bounded history

- Date: 2026-09-02
- Status: Accepted and implemented
- Extends: [Decision 0926: classify and bound verification-owner outcomes](0926-Classify-And-Bound-Verification-Owner-Outcomes.md)
- Current contract: [native verification owners](../../Specifications/Windvale-Native-Verification-Owners.md)

## Context

Initial owner-duration profiles are deliberately conservative. One observation
cannot distinguish a stable fast owner from a warm-cache accident, and Linux
timing alone cannot justify reducing a Windows allowance. Raw workflow artifacts
also expire, so a later calibration review needs a small durable history without
turning timing collection into another required test or repository data stream.

## Decision

- Reuse each development job's existing per-host cache for a bounded timing
  history. Keep at most 20 observations per owner and host, and deduplicate an
  input report by its SHA-256 identity, owner, and host.
- Accept only the structured development timing and runner-result formats.
  Retain executed owner outcomes; ignore cached entries and non-owner timings.
- Treat timing analysis as optional development infrastructure. Failure to read,
  update, cache, or upload timing history cannot convert good product code into
  a test failure and cannot supply missing verification evidence.
- Require at least five passing samples on both Windows and Linux before
  recommending a smaller profile. Allow only a one-profile reduction per review,
  with 50 percent headroom over the observed 95th percentile and 25 percent over
  the observed maximum.
- Report timeouts for human review. Never use a timed-out run to recommend a
  smaller profile, and never edit the owner registry automatically.
- Bound report count, individual and aggregate input bytes, retained history,
  analysis output, diagnostics, and accepted elapsed values. Publish history and
  analysis with same-directory atomic replacement.

## Consequences

Normal development jobs add a small bounded analysis step rather than another
test job. Their existing cache carries recent host-local measurements forward, and
the existing artifact includes the current analysis. A lost or evicted cache
only loses optimization history; it cannot change verification correctness.

Profile changes remain explicit reviewed edits. Reviewers can see whether an
owner lacks cross-host evidence, fits its current allowance, qualifies for one
smaller profile, exceeds its expected allowance, or has timed out.

## Reconsideration triggers

Revisit the sample counts and margins after enough owners have representative
dual-host data, if GitHub cache behavior prevents useful history retention, or
when a Windvale-native metrics store can replace the transitional JSON cache.
