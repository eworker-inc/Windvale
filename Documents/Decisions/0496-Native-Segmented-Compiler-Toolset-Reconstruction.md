# Decision 0496: Native segmented compiler toolset reconstruction

- Status: Accepted current-Windows-host construction evidence
- Date: 2026-08-10
- Scope: the three segmented compiler process WVBs and their paired Windows/Linux hosted applications
- Extends: [Decision 0416](0416-Digest-Bound-Segmented-Compiler-Process-Front-Door.md), [Decision 0417](0417-Canonical-Compiler-Image-Transport.md), [Decision 0418](0418-Segmented-Compiler-Hosted-Package-Composition.md), and [Decision 0492](0492-Hosted-Container-Toolset-Reconstruction.md)
- Retains: the retained candidate as a bootstrap seed, explicit Stage 0 recovery/differential provenance, and the final grouped retirement gate

## Context

The segmented compiler path separates WVB-to-WVO staging, compiler-image
linking, and canonical image transport so the complete compiler need not fit in
one hosted process value. Each process WVB already builds through the native
source front door, and the retained applications execute without loading .NET.
Their paired PE/ELF application identities nevertheless remained attributed to
the Stage 0 application writers.

The complete hosted-container toolset can now package an admitted canonical
image for either permanent host from a Windows invocation. That allows the
three process families to reconstruct themselves, but the reconstruction uses
the retained segmented applications to produce their replacements. The result
is useful construction evidence, not a clean bootstrap rooted only in an older
independent seed.

## Decision

- Build the staging-producer, compiler-image-staging, and canonical-transport
  WVBs through the digest-bound native Project 1 front door.
- For each WVB, use the retained staging, linking, transport, and hosted-
  container candidates to construct both its Windows and Linux applications.
- Require all nine outputs to match the exact identities below before reporting
  completion.
- Require a caller-supplied existing output directory that is distinct from the
  checked-in candidate, and keep all intermediate chunks and manifests in one
  private temporary directory.
- Preserve the Stage 0 writers as explicit recovery and differential owners.
  Do not treat this self-reconstruction as seed independence, Stage 2,
  promotion, Linux execution, or dual-host qualification.

## Current-host identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Wvo-Staging-Producer.wvb` | 439,000 | `5b0c18b73921c90ff4b168b49999ac8b39b322964e1204c47d3ff588efba0b07` |
| `windows-x64-wvstage.exe` | 6,400,512 | `4185b17364b524bb897cf9f8e5917546ad0abb2b15695393879be11c6630a7eb` |
| `linux-x64-wvstage.elf` | 6,402,048 | `cc46996c074a94dfd92a9c42f1403ad377f7dd850c8533387b2857742821f944` |
| `Compiler-Image-Staging.wvb` | 75,553 | `14521acae6052d08add386833a35dd22c36e0dd07a1fad494961ee8064119d1c` |
| `windows-x64-wvlinkstage.exe` | 852,480 | `7f4be5d6b1236b5f5171e52f3861540432c4781140d154e28d52f804aa8cbcde` |
| `linux-x64-wvlinkstage.elf` | 851,968 | `845402fb71bbf7a76524fd90b771b7c6e2d88b92ff9fe7440efe5839304a6ab3` |
| `Compiler-Image-Canonical-Transport.wvb` | 23,836 | `dc5f460ce89bcce2678092030376c8ddc928e682b263af2a73ba2a57034b6d4d` |
| `windows-x64-wvimagetransport.exe` | 269,312 | `51801aaf70ba265212edd4bcbf6277cc395bb6412a6f38f07954e65a6978f9dc` |
| `linux-x64-wvimagetransport.elf` | 270,336 | `56c9fd42da56f00f04d4bacf7689bad56693a36b4e9ce7f88dcfcae16db75fe7` |

These identities were measured from the current Windows-host native cross-
target reconstruction. No Stage 2 run, Linux-host construction or execution,
promotion transaction, or grouped qualification is claimed by this decision.
The fixed `segmented-compiler-toolset-reconstruction` owner then rebuilt all
nine products through the checked-in constructor and passed 3/3 cases in 167.9
seconds on the current Windows host.

## Consequences

The C1/L1 construction ledger no longer needs a managed application writer to
reproduce this exact nine-artifact segmented candidate. The normal native path
can renew all three process families before later compiler reconstruction.

The dependency is circular at the candidate-toolset boundary: the retained
applications construct their own replacements. A later release must consume a
qualified previous release or another explicitly documented native seed before
this evidence can support a non-circular clean-bootstrap claim. Independent
Linux reconstruction and execution, current full Stage 2, paired promotion,
and the grouped dual-host gate remain open.

## Reconsideration

Reconsider this decision if any of the three source closures, segmented object
or image formats, hosted-container profile, target startup/service leaves, or
exact application identities change, or when a previous qualified release can
replace the retained same-candidate seed.
