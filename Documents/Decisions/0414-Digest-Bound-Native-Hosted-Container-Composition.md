# Decision 0414: Digest-bound native hosted-container composition

- Status: Implemented candidate on Windows; Linux execution and promotion pending
- Date: 2026-08-08
- Advances: [Decision 0413](0413-Native-Hosted-Segment-Iteration-Control.md)
- Contract: [Native hosted-container packaging](../../Specifications/Windvale-Native-Hosted-Container-Packaging.md)

## Context and decision

Every hosted-container binary format and both remaining loop counts have a
Windvale-native owner. The last normal-path boundary is therefore platform
process composition: acquire the exact native tools and retained leaves,
execute them in order, keep intermediate resources private, and invoke the
already native atomic publisher.

Commit one candidate toolset containing 19 WVBs and their paired Windows/Linux
applications. Bind all 57 binaries through one exact `SHA256SUMS` inventory.
Add one focused launcher per permanent host. These scripts may parse only fixed
process-control text; they must not decode or construct Windvale binary formats.
Keep all intermediate state in one fresh private temporary directory and limit
cleanup to that resolved directory.

The first composition exposed a contract mismatch hidden by isolated fixtures:
the metadata-request producer compared the logical source byte count with the
aligned image extent and expected image-space offsets in the hashing manifest.
Clarify and enforce the intended model instead. `WVHS` hashes the contiguous
logical concatenation of the fragment and ten service resources; `WVPQ`
separately owns aligned image placements. Padding is neither a source resource
nor an identity leaf.

## Evidence and consequences

The candidate toolset contains 59 files including its manifest and checksum
inventory, totaling 14,458,202 bytes. Its `SHA256SUMS` has 57 entries, is 5,426
bytes, has SHA-256
`5f6fd8be149729285590ba649c3a034ffce117d7a8db1c06b3fc689f9f2d8804`,
and verifies every listed artifact.

The changed metadata-request WVB reconstructs through the native front door at
54,397 bytes with SHA-256
`683538840f21325469324a62e3296582c43f4df9f396908263dd9f074c5b19b9`.
The updated focused test uses eleven raw source resources, requires a real
alignment gap, and passes 1/1 in 7.201 seconds after a zero-warning build.

The reviewed Windows launcher completes the entire path in 10.3 seconds. It
packages the existing orchestration-control WVB into a 236,032-byte PE with
SHA-256 `eeec7c229b20ac006ed366849c91e2f03e035a9e3ee29da2e9aeb408c76b2709`,
exactly equal to the separately constructed candidate. No CLR, hostfxr, or
hostpolicy process participates in this ordinary launcher. No broad verifier
ran, in accordance with the grouped end-of-goal verification policy.

This is a candidate rather than an ordinary-path promotion. Linux process
execution must pass from the same inventory, both launchers need focused
failure/preservation checks, and the complete containing commit must pass the
grouped Windows/Linux gate before managed hosted-container construction moves
to recovery-only status.

## Reconsideration triggers

Version the candidate when any contained target, fixed service leaf, startup
WVO, status line, binary format, or segment bound changes. Extend the launcher
through a native fragment-geometry owner before accepting fragments larger than
4 MiB. Do not add binary decoding, digest calculation, or format construction
to the platform scripts.
