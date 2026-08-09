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
inventory, totaling 14,484,204 bytes. Its `SHA256SUMS` has 57 entries, is 5,426
bytes, has SHA-256
`6237a4131ab079ed03992e969375d8569f3c546bb415a50c25b19c982f516522`,
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

The paired focused smoke scripts keep success and rejection evidence outside
managed test code. The Windows script passes 2/2 in 13.1 seconds: exact package
equality, then invalid-WVB rejection with input/destination preservation and no
private scratch. Git Bash accepts both Linux scripts syntactically; this host
has no Linux execution environment, so genuine ELF evidence remains pending.

The first focused Linux execution also exposed two host-specific assumptions:
three retained ELF dependencies had lost executable mode, and empty Linux
import and relocation regions inherited the plan's zero absent-section offset.
The candidate now preserves executable mode and anchors each empty region at
the preceding region's image end. The updated source-set contract passes its
reviewed focused test 1/1 in 8.241 seconds, and the changed candidate inventory
passes the Windows packaging smoke 2/2 in 12.3 seconds.

Focused run `31286018209` then passed Windows and carried Linux through the
corrected source set before the segment-request producer rejected that same
canonical empty-region geometry. An audit of every downstream `WVSG` consumer
found this as the only remaining stale equality rule. The segment-request
producer now applies the same empty-region anchors and reconstructs through the
native front door at 44,543 bytes with SHA-256
`09802f31927bc3120476001ff3733c15dcf3072537c109f3b044b170cee8b27f`.
Its reviewed focused test passes 1/1 in 7.650 seconds after a zero-warning
build, and the updated Windows packaging smoke passes 2/2 in 13.1 seconds.

The remaining chain was also exercised locally through the Windows process
forms over the preserved Linux-target resources: segment request, segment
construction, segment manifest, and publication all accept the source set and
produce the exact 237,568-byte ELF with SHA-256
`f7b40ac03478d54bdf8fed468fdfbe52a9449159a9fb45c05da6603935e24c67`.
A fresh genuine Linux run from this exact inventory remains the promotion
evidence.

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
