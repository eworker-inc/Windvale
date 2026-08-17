# Decision 0740: Reuse hosted-application producer trust

- Date: 2026-08-17
- Status: Implemented with Windows development evidence; independent Linux execution pending
- Advances: [Decision 0554](0554-Content-Addressed-Hosted-Application-Development-Checkpoints.md)
- Contract: [Windvale native tool checkpoint 1](../../Specifications/Windvale-Native-Tool-Checkpoint.md)

## Context

After segmented-project checkpoints, the all-hit 50-case database owner took
323,820 ms. Its portable cases still took 115,980 ms even though every project,
linked image, and application was an immutable checkpoint hit.

One representative unchanged 5.65 MiB hosted application took 1,573 through
2,393 ms per hit. Key construction alone took 447 through 930 ms because every
invocation started Node, reopened the 72-entry inventory, rehashed the same
21.7 MiB toolset and remaining shared producer closure, then discarded that
trust state. The owner invoked this boundary roughly fifty times.

The exact WVB, fragments, profile, entry, target, application checkpoint, and
materialized copy still require per-request hashing and validation. Only the
producer closure is common to one already bounded owner invocation.

## Decision

Extract the existing version-1 key algorithm into
`Native-Hosted-Application-Cache-Core.mjs`. Keep the standalone key command as
a thin adapter and prove that it emits the existing key for unchanged inputs.

For database development verification, start one current-host Node session
after tool preparation. At startup, read and validate the exact packager,
inventory, all 72 inventory-verified artifacts, enum-request family, nine
target service leaves, and target startup object. Retain their exact buffers in
the private process. For every request, hash the exact WVB and ordered native
fragments, replay the retained producer fields in their original order, and
derive the byte-identical `hosted-application-v1` key.

Keep checkpoint consumption independent per request: reject unexpected entries
or links, rehash the application, reconstruct and compare the exact record,
copy to a private owner path, rehash the copy, and preserve Linux executable
mode. Serialize requests through a loopback-only service authenticated by a
random 256-bit token whose bounded readiness record lives in the owner's
private temporary directory.

Make the session read-only for publication. A missing key returns exit 75 with
no output mutation; the database helper then invokes the unchanged standalone
checkpoint driver. That path repeats full producer validation and owns cache
creation, immutable publication, and miss diagnostics. A corrupt existing
entry is an error and never becomes a miss. Qualification and no-argument
verification do not start or consult the session.

## Evidence

The refactored standalone command and session derive the existing representative
key `a58f9ab62c3d5f83d9d27fc777c2e3a0f78515ffe1d41bcafdb620ce0fb7088f`.
The same application hit falls from 1,573 through 2,393 ms to 129 through
165 ms. An isolated cold request returns 75 unchanged; the standalone driver
then creates the exact key in 40,962 ms, and the still-running session consumes
the resulting byte-identical product as a hit.

The bounded session regression passes four concurrent serialized hits, exact
standalone/session key equivalence, corruption rejection with its sentinel
output unchanged, miss preservation, executable-mode preservation on Linux,
and clean readiness/process teardown when exercised on the respective host.
The current Windows run covers the shared behavior and teardown; independent
Linux execution remains pending. The final change-aware Windows 50-case owner
passes with every application reporting `Hit` in 281,240 ms, down from
323,820 ms. This saves 42,580 ms or 13.15 percent; its portable section falls
from 115,980 ms to 81,940 ms, a 29.35 percent reduction. Relative to the earlier
500,610 ms project-object-v2 result, the segmented and session changes together
save 219,370 ms or 43.82 percent and are 1.78 times faster. These are host
diagnostic measurements, not portable timing claims.

## Consequences

Ordinary warm verification no longer treats immutable producer files as if
they were newly discovered fifty times in one owner process. The session adds
one bounded Node process and small authenticated loopback requests; it does not
share compiler/runtime state or skip any application execution.

The service retains roughly the shared producer closure size in memory for the
owner lifetime. Cache population is deliberately not accelerated: misses pay
the original complete validation and publication cost. Standalone callers and
other native owners keep their existing behavior.

## Reconsideration triggers

Reconsider this decision if session and standalone keys differ; a per-request
input, producer, target, or profile change can select an old entry; a miss or
corruption mutates an output; session failure can leave a live process or
readiness record; Linux mode or lifecycle behavior differs; qualification
starts the session; or a native multi-request packager can preserve the same
immutable admission contract with less fallback cost.
