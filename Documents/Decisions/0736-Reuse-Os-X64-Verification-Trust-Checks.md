# Decision 0736: Reuse OS x64 verification trust checks

- Date: 2026-08-16
- Status: Implemented with complete Windows execution evidence; independent Linux execution pending
- Extends: [Decision 0735](0735-Manifest-Driven-Os-X64-Code-Emission-Verification.md)
- Advanced by: [Decision 0737](0737-Batch-Os-X64-Project-Wvb-Development-Checkpoints.md)
- Preserves: per-target native processes, immutable publication, exact hashes, and the 336-case complete owner

## Context

The manifest-driven owner initially invoked the ordinary `Build-Wvb`,
`Lower-Wvb-To-Wvo`, `Link-Wvo`, `Package-Console`, `Publish-Wvo`, and
`Publish-Console` wrappers for each target. Those wrappers correctly establish
trust when invoked independently, but a complete 56-target owner repeated the
same work inside one already bounded process.

On Windows each target performed nine hashes across seven distinct pinned tools:
two for build and WVB publication, two for lower and WVO publication, one for
linking, and four for two package/publication operations. It also recursively
scanned the complete workspace for reparse points during every build. The full
owner therefore performed 504 tool hashes and 56 equivalent workspace scans.

The native build driver exposes one project request per process. Introducing a
persistent compiler service would change a larger runtime and lifecycle boundary
than is required to remove this measured overhead.

## Decision

- Treat one OS x64 owner invocation as a bounded verified tool session.
- Before processing target rows, validate the seven pinned build, publication,
  lowerer, linker, and packager identities once. Preserve the existing exact
  host-specific digests and the Linux native-front-door inventory check.
- Copy those seven tools into the private session directory and validate the
  staged bytes before use. Later changes to repository artifacts cannot change
  the executable identity of an active session.
- Validate the workspace marker and the absence of reparse points or symbolic
  links once before the first build.
- Invoke the already verified native tools directly for each selected row.
- Retain a distinct native process for every build, lower, link, package, and
  publication request. This is trust-check reuse, not a daemon or shared mutable
  compiler instance.
- Give every WVB, WVO, Windows container, and Linux container a separate
  candidate path and publish it through the same immutable publisher used by
  the standalone wrappers. Continue checking every final byte size and SHA-256.
- Keep the standalone wrappers unchanged for callers that do not already own a
  bounded verified session.
- Make planner verification require every pinned identity, workspace-containment
  check, direct tool/publication boundary, and the absence of standalone wrapper
  calls in the paired owners.

## Evidence

Planner verification passes 24 general and 163 native cases, and Git Bash accepts
the Linux owner syntax. First, middle, and final Windows focused targets retain
their six checks and pass through private tool snapshots.

The complete Windows owner passed all 56 projects and 336 checks in 82,557 ms,
compared with 129,638 ms immediately before session reuse. This saves 47,081 ms,
reduces measured wall time by 36.32 percent, and is a 1.57-fold speedup. These are
diagnostic measurements on one host, not portable thresholds. Linux syntax does
not substitute for independent Linux behavior execution.

## Consequences

The complete Windows owner now hashes each of its seven private tool snapshots
once and scans the workspace once. Exact output verification remains per target,
so a changed compiler, malformed artifact, publication failure, or byte mismatch
still fails the owner.

Focused execution also avoids redundant publisher and packager validation, but
still pays one bounded session setup. Further material improvement requires a
versioned multi-request build-driver protocol or dependency-aware compiler cache;
neither is implied by this decision.

## Reconsideration triggers

Reconsider this decision if private tool staging cannot preserve identity during
an owner invocation, publication no longer provides an immutable
candidate-to-destination boundary, workspace containment can change concurrently
without detection, Linux session execution differs from standalone-wrapper
execution, or a native multi-request driver can preserve isolation and
per-project diagnostics.
