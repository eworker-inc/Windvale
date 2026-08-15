# Decision 0571: Signed two-package offline stage

- Status: Implemented locally; paired-host evidence pending
- Date: 2026-08-15
- Advances: Milestone 4 and Decision 0568
- Contract: [Release Envelope 1](../../Specifications/Windvale-Release-Envelope.md)

## Context

Milestone 4 requires one offline directory to authenticate at least two real
packages and their exact policy records. The published `v0.1.0` preview
envelope has a broader product-release inventory and must remain immutable.
Publishing or tagging `v0.2.0` merely to test package composition would confuse
development evidence with a product promotion.

The existing Release Manifest 1 grammar already carries a channel and generic
role/target artifact records. It can bind the required package closure without
a new serialized format or access to the retained operational release keys.

## Decision

Admit a `stage` channel in the existing release-envelope creator and independent
verifier. A stage contains at least two named package targets, the shared
license and offline verifier, exact Windows/Linux Generation 1 records, and
exactly one package, approval, provenance, Windows launch, and Linux launch
artifact for every package target. Reject an `all` package target, missing
policy or generation closure, extra roles, duplicate targets, or mixed preview
package forms.

Construct the first stage from the exact admitted WVDB Query and WVB Inspector
bundles and their checked-in policy records. Its generation records explicitly
bind `wvdump` to the inspector root and `wvquery` to the WVDB application root;
their launch identities differ by target. Sign permanent tests with fresh
ephemeral root and release keys, build the directory twice, compare every byte,
verify the complete inventory and both generation records through the portable
Windvale parser, publish the current-host generation immutably, and reject a
tampered real bundle.

Keep the published single-package preview profile backward compatible. The
stage uses version text `0.1.0` only as the current artifact compatibility line;
its `stage` channel is not a stable release, tag, update instruction, or public
trust-root transition.

## Consequences

- Milestone 4 gains one deterministic, signed, network-free input directory
  carrying exact generation construction for both permanent hosts.
- Operational release private keys are neither needed nor exercised by normal
  development tests.
- Passing the stage owner proves authenticated package and policy inventory; it
  does not yet prove activation, command dispatch, rollback, or uninstall.
- A future public release may reuse the artifacts only after an explicit
  promotion decision and release qualification.

## Reconsideration triggers

Reconsider when package metadata needs independent update cadence, delegation
per package publisher, package dependency resolution, freshness beyond a local
stage sequence, or a public channel other than the existing preview policy.
