# Decision 0563: First release envelope and key policy

- Status: Implemented candidate
- Date: 2026-08-15
- Advances: Milestone 3 and Decisions 0183, 0198, and 0562
- Contract: [Windvale release envelope version 1](../../Specifications/Windvale-Release-Envelope.md)

## Context

The deterministic Windows and Linux development installers have exact archive
and payload identities, but a digest on a web page does not authenticate who
selected a release. Windvale 0.1 also requires offline verification without
turning signing into runtime authority or pretending that a partial update
framework provides protections it does not implement.

The first preview does not have trustworthy civil-time, network freshness,
automatic update, threshold-key, or durable rollback-state requirements. It
does need a small replaceable release key beneath a root that is never placed in
CI or an ordinary development checkout.

## Decision

Adopt Release Envelope 1: one pinned Ed25519 root signs one bounded root policy,
which delegates one Ed25519 release key for one `0.y.` line and inclusive
release-sequence range. Use SHA-256 over canonical SPKI DER as the key identity.
Sign exact domain-separated canonical root-policy and release-manifest bytes.

Do not use civil time or online discovery in the first verifier. Let an offline
caller require a minimum sequence. A later release-key replacement uses a new
root-signed policy generation. Root rotation, threshold roots, emergency
revocation, and network freshness require a successor decision with explicit
transition and recovery evidence; Release Envelope 1 does not claim TUF.

Require the preview manifest to select source, both platform installers, WVDB
Query Bundle 1, capability/launch approval, license, provenance, Stage 0
recovery reference, paired qualification reports, and offline verifier. Verify
every byte and reject undeclared files, links, unsafe paths, oversize input,
wrong keys, malformed signatures, rollback below caller policy, and incomplete
profiles before reporting success.

Keep creator and verifier parsers independent. Generate test keys ephemerally.
Never check in, upload, log, or place an official private key in CI. The
repository may implement the ceremony and qualify the format without claiming
that an official trust root exists. Creating that root is a separate explicit
custody action by the project owner.

## Consequences

- Release provenance becomes an exact offline-verifiable contract without
  granting application capabilities.
- Identical inputs and keys produce identical envelope bytes because Ed25519
  signing is deterministic and the manifest is canonical.
- A verifier can reject a valid old sequence only when the caller supplies a
  trusted minimum; there is no hidden freshness claim.
- The first root ceremony, final signed envelope, selected-state qualification,
  tag signature, and public release remain open until the project owner accepts
  custody and performs the documented ceremony.

## Reconsideration triggers

Reconsider when a second release key, root rotation, threshold custody, online
updates, revocation, civil-time freshness, hardware-backed signing, a release
above the 512 MiB profile, or a non-Node independent verifier becomes required.
