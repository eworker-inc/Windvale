# Decision 0863: Preserve running development verification and failure checkpoints

- Date: 2026-08-27
- Status: Implemented; pushed workflow evidence complete
- Refines: [verification throughput](../Architecture/Seed-Verification-Throughput.md)
- Preserves: exact changed-file classification, immutable checkpoint validation,
  cold qualification, and the stable verification gate

## Context

Several independent development agents may push coherent changes to `main`
while another affected-owner run is reconstructing a compiler-scale product.
The prior workflow cancelled the running verification on every newer push.
GitHub retained no new development cache after a failed or cancelled job, so a
late assertion could discard more than twenty-five minutes of valid immutable
compiler checkpoints and force the next run to recreate them.

GitHub's concurrency contract already supports the required bounded queue. With
one concurrency group and cancellation disabled, one run remains active and
only one pending run is retained; a newer push replaces the older pending run.
The official cache action also supplies separate restore and save entry points,
allowing a final `always()` step to publish completed checkpoints after a late
behavior failure.

## Decision

- Keep the workflow-and-ref concurrency group, set `cancel-in-progress` to
  false, and declare the single pending-run queue. Never run two Verify
  workflows for the same ref concurrently. Let the current run finish while
  coalescing pending pushes to the newest source state.
- Replace the combined development cache action with its pinned restore and save
  actions. Restore the latest host-specific version-1 prefix before affected
  owners run. Save the run-attempt-specific primary key in an `always()` step
  whenever that exact key was not restored.
- Cache only the existing content-addressed development root. A checkpoint hit
  must still validate its key, manifest, sizes, digests, and structural
  admission. Never cache a passing test result or infer behavior from a product
  hit.
- Keep qualification jobs cold. They bind no development cache root and contain
  no restore or save action.
- Verify the exact concurrency, queue, action pins, keys, failure-save condition,
  and qualification exclusion in the repository workflow verifier.

## Evidence

GitHub run `33096583366` reconstructed the exact current runner on both hosts,
then both jobs failed only on the same stale 53-byte diagnostic oracle after
1,577 seconds on Linux and 1,765 seconds on Windows. The next run could restore
only older cache keys because the failed jobs did not publish their completed
checkpoint roots. This is the concrete repeated-work case addressed here.

GitHub run `33099588334` remained active and completed successfully on both
hosts after the policy was pushed. Run `33100571963` for the newer source state
was created while that work was still active, waited rather than cancelling
it, and started its focused jobs only after the predecessor completed. The
queued run then passed on both hosts and reached the stable verification gate.
Its affected-owner step completed in 34 seconds on Linux and 30 seconds on
Windows after restoring the preserved development checkpoints; both final save
steps also completed successfully. Qualification, bootstrap, WebAssembly, and
website jobs remained unselected, preserving the cold qualification boundary.

A deliberate late-failure probe is unnecessary: the pinned workflow verifier
checks the `always()` save boundary, while every checkpoint family retains its
own corruption and atomic-publication coverage.

## Consequences

The newest commit may wait for the current run, but the repository no longer
throws away almost-complete compiler work merely to start the newest comparison
immediately. Intermediate pending commits still do not form an unbounded queue.
Late failures preserve reusable products while rerunning all affected behavior
on the next source state. Cache storage may grow more quickly during a sequence
of failing runs; immutable run-attempt keys remain subject to GitHub's normal
cache retention and eviction policy.

This changes development scheduling and reuse only. It does not make a failed
run pass, weaken a test, reuse qualification evidence, or change Windvale
language/runtime semantics.

## Reconsideration triggers

Reconsider the single pending queue if measured wait time becomes worse than
discarded-work time, if repository pushes become infrequent enough that
cancellation no longer wastes material work, or if a trusted shared checkpoint
service can atomically preserve partial run progress independently of workflow
completion.
