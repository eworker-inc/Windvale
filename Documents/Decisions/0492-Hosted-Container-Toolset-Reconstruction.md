# Decision 0492: Hosted-container toolset reconstruction

- Status: Accepted current-host candidate
- Date: 2026-08-10
- Scope: Decision 0491 hosted-container toolset reconstruction, exact candidate inventory, and focused package evidence
- Extends: [Decision 0485](0485-Native-WVHV-Publisher-Admission-Applications.md) and [Decision 0491](0491-Build-Driver-Profile-Capacity.md)
- Retains: 24 commands, 72 candidate artifacts, Stage 0 recovery provenance for outer host containers, and the pending grouped dual-host retirement gate

## Context

Decision 0491 changed the shared hosted-compiler startup, both file-input leaves,
profile-2 metadata and runtime geometry, and several hosted-container source
closures. The retained candidate inventory therefore failed closed and could no
longer support the ordinary digest-bound `Package-Hosted-Wvb` path.

During reconstruction, the source-set tool exposed one stale boundary. It still
admitted startup responses carrying the prior request lengths even though the
startup objects and relocation target tables had grown. Retrying the complete
pipeline without correcting that contract would have hidden the inconsistency.

## Decision

- Rebuild every one of the 24 canonical tool WVBs through the digest-bound native
  Project 1 front door.
- Retain explicit Stage 0 recovery construction for the 20 outer applications
  that have named recovery targets. Use a staged copy of the native packager for
  the four verifier platform, bundle, composition, and startup wrappers that do
  not have recovery targets.
- Require the source-set tool to admit the complete current startup request:
  4,674 bytes on Windows and 2,622 bytes on Linux.
- Replace the candidate only after all 72 artifacts match one canonical inventory.
  Do not overwrite the live candidate during measurement.
- Repin current application contracts, active specifications, focused tests, and
  digest-bound launchers. Retain historical decisions and dated evidence without
  rewriting their identities.

## Current-host evidence

The candidate retains 24 WVBs, 24 Windows applications, and 24 Linux
applications. `SHA256SUMS` is 6,927 bytes with SHA-256
`bca5cead0b3698f060c4cc5a165eb75dc52aaad5e81202ef95c54f16976d0ded`;
all 72 unique paths independently match their listed digests.

The source-set WVB is 82,068 bytes with SHA-256
`7f110c0e7fe9a4a50627e9c600f19c61850e12a265cc44c26ad704353f4b2a74`.
Its Windows application is 1,284,096 bytes with SHA-256
`c4626edcc40c2b0c8aff4f4eec8af494034d9bf42fb04959dca393945f7eadfb`;
its Linux application is 1,286,144 bytes with SHA-256
`a2a4687804e063d6f2d9b9c965b07893f749d34da53f271373bc0b41ae671e63`.

The focused Windows owner completed once in 41.1 seconds. Its five cases
reproduced the exact Windows and cross-target Linux orchestration-control
applications, reproduced both verifier-request applications, rejected an invalid
WVB, preserved the input and pre-existing destination, and left no private
package scratch. The repinned recovery tool project also built with zero warnings
and zero errors. No broad Seed, bootstrap, or grouped qualification gate ran.

## Consequences

The ordinary Windows package launcher again runs from a complete current
digest-bound native toolset and cross-constructs the paired Linux candidate
without executing .NET. The shell launcher carries the same identities, but
independent Linux execution remains pending. The candidate is not promoted by
this decision, and its 20 recovery-constructed outer applications remain explicit
bootstrap provenance rather than native outer-package construction evidence.

Publisher-specific base and final-application identities affected by the new
file-input leaves remain a later reconstruction slice. Their launchers now admit
this toolset and the current leaves, but must continue to reject stale downstream
identities until that slice is measured and repinned.

## Reconsideration

Reconsider this decision if either host cannot reproduce the same 72-entry
inventory, if a wrapper requires an unnamed recovery target, if the native
packager cannot reconstruct one of the 20 recovery-built outer applications, or
if the grouped retirement gate finds a contract not owned by the focused package
lane.
