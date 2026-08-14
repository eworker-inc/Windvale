# Decision 0493: Native WVHV publisher-overlay reconstruction

- Status: Accepted current-host candidate
- Date: 2026-08-10
- Scope: hosted-verifier application publisher, publisher construction inventory, profile-8 admitter, durable promoter, and role-2 WVB publisher
- Extends: [Decision 0490](0490-Indexed-Compiler-WVB-Verification.md) and [Decision 0492](0492-Hosted-Container-Toolset-Reconstruction.md)
- Retains: roles 0/1/2, profiles 2/8, publication transaction semantics, frozen Stage 0 recovery evidence, and the pending grouped dual-host retirement gate

## Context

Decision 0492 replaced both target file-input leaves without changing their
lengths or service contract. Every publisher-overlay base embeds one of those
leaves, so all three role-specific base identities and all four completed
application pairs became stale even though their byte lengths and layouts were
unchanged.

The role-1 promoter and profile-8 admitter import exact admission of the
role-0 publisher. Advancing the publisher identities therefore also advances
the admission-tool and promoter WVB/WVO identities. Reconstructing the outer
applications without first advancing that non-circular trust chain would leave
the promoter unable to admit the publisher it was meant to install.

## Decision

- Reconstruct the role-0 publisher and role-2 WVB publisher first through the
  Decision 0492 hosted-container toolset and the existing native publisher
  overlay.
- Put the measured role-0 identities into the portable Windvale publisher
  admission module. Rebuild and natively lower the admission tool and promoter.
- Advance exact promoter metadata and final-application identities in the
  shared role-aware construction digest table, then rebuild only the five
  construction modules that consume that table.
- Repackage all eleven construction commands for both hosts because their
  outer hosted applications embed the new shared file-input leaves.
- Replace the construction candidate only after its 48 artifacts match one
  canonical checksum inventory. Preserve semantic and recovery provenance in
  every candidate manifest.
- Reconstruct and pin both promoter and profile-8 admitter applications from
  the settled construction inventory. Keep Stage 0 writers as frozen recovery
  and differential evidence rather than normal construction owners.

## Current-host evidence

The version-15 construction candidate contains 26 WVB/WVO artifacts and 22
paired host applications. Its 48-entry `SHA256SUMS` is 4,980 LF-only bytes at
SHA-256
`4989e21858705df8fb1776b36a26350144b6bf02fab5bd8d910e1711f2a7691d`;
every listed artifact independently matches its length and digest.

The reconstructed completed applications are:

| Role | Windows x64 | Linux x64 |
| --- | --- | --- |
| Hosted-verifier application publisher | 256,000 / `17cb5c4228e8448693b17f1b73695fd0ecfd03d7ada922794a5bf3bd7594fc96` | 254,917 / `babe721a573e29f89ec095c35677880077ff465d4e2129063f6742cd47591a97` |
| Publisher promoter | 681,472 / `598bd2de8247abd19d931efa1edcc8323adef7f56da51da1d41256933667eb23` | 680,901 / `422332fb4f2824ae558bf93adadb6470597399d07810f5428f71aa4d971a4f58` |
| WVB publisher | 1,340,928 / `71794a6a254ccfd652ffe3bad556c32f86e2d9210a5a3099bad576f97476a8f3` | 1,340,357 / `7f2dbfaecf2734c5afdbd6e2e54263a5a74038b8a498eeb1e155ee71788b630c` |
| Profile-8 publisher admitter | 570,368 / `4742ee299759728be1b72fed3d3b42620c21b10f77aed12cf150c1549b177b53` | 569,344 / `b03788fad58ce071788b2f30945ed1dc0992559bb04b6cad04e719ff1114dc0a` |

The focused Windows owner passed its first twelve construction, admission, and
preservation cases. Its next case correctly exposed one stale success-report
digest in the test after the installed publisher identity advanced. After that
test contract was reviewed and repinned, the exact promoter installation,
installed-publisher verifier publication, and WVB-publisher execution remainder
passed 3/3 in 4.8 seconds. The already-passing twelve cases were not restarted.
No broad Seed, hosted-container packaging, bootstrap, or grouped qualification
gate reran.

## Consequences

The normal current-host candidate path again constructs all three publisher
overlay roles and the separate read-only admitter without invoking a managed
publisher writer. The promoter admits the current publisher, the publisher
admits the current verifier, and the role-2 application durably publishes a
canonical WVB. The formats, layouts, capability authority, transaction state
machine, and fixed byte counts are unchanged.

This decision does not promote any candidate. Independent Linux execution,
cross-host reproducibility, grouped qualification, release integration, final
Stage 0 recovery publication, and managed-source deletion remain open.

## Reconsideration

Reconsider this decision if either host cannot reproduce the 48-entry
construction inventory, if the publisher admission update creates a digest
self-reference, if the promoter installs bytes other than its admitted immutable
snapshot, or if the grouped retirement gate finds a publisher-overlay behavior
not owned by the focused lane.
