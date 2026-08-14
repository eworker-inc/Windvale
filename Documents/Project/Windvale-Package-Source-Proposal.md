# Windvale hybrid official package source proposal

- Date: 2026-08-14
- Status: Proposed implementation contract; promote to a numbered decision only after the active baseline and concurrent decision sequence settle
- Builds on: [Package 1 and Lock 1](../../Specifications/Windvale-Package.md), [package and recovery architecture](../Architecture/Packages-Releases-And-Recovery.md), [bundle and installation architecture](../Architecture/Windvale-Package-Bundle-And-Installation.md), and [Decision 0530](../Decisions/0530-First-Locked-Source-Package-And-Wvdb-Application.md)
- Architecture: [release discovery 1](../Architecture/Windvale-Release-Discovery.md)

## Context

Decision 0530 proves one exact offline source package but intentionally leaves
bundles, stores, signatures, installation, and network sources open. The first
installed `wv` bootstrap needs a durable official address without making DNS,
TLS, a mutable website, or one hosting vendor the semantic identity of releases.

Serving every binary directly from `windvale.ca` would add artifact storage,
bandwidth, retention, and recovery operations before the package contract is
qualified. Using GitHub URLs directly as the permanent client configuration would
couple the product namespace and discovery policy to one provider and would tempt
clients to treat a mutable `latest` URL as release identity.

GitHub is already Windvale's accepted public release archive. GitHub immutable
releases can bind a tag and attached assets against later modification and provide
independent attestations. Those facilities are useful origin evidence without
replacing Windvale's own identities and signatures.

## Proposed direction

Use `windvale.official` as the canonical source identifier and
`https://packages.windvale.ca/v1` as its default endpoint. Keep the root website
human-facing. The package subdomain serves small signed Root, Channel, Release,
and Signature objects through stable paths.

Retain exact package bundles, host bootstrap applications, source archives,
license inventories, provenance, qualification reports, and recovery artifacts
as immutable GitHub Release assets. The package endpoint may redirect a
content-addressed object request to an exact GitHub asset. Every client verifies
the signed inventory, declared byte count, and SHA-256 identity after retrieval,
so changing an origin or mirror does not change an artifact identity.

Do not use the GitHub `latest` release, a branch, a tag name alone, an HTML release
page, DNS, TLS, or a redirect target as the selected release. The signed Channel
selects one exact Release digest and monotonically increasing sequence. The signed
Release inventories every exact content object.

Begin with an embedded offline Root authorizing replaceable Ed25519 release keys.
Require old- and new-root threshold agreement for sequential Root rotation. Keep
capability approval independent: a release signature proves publisher intent and
does not grant an installed application filesystem, network, process, device, or
administrative authority.

## Bootstrap shape

The only manually installed product is a small native launcher plus one verified
`wv` client generation, initial Root, and official source configuration. The
compiler, runtime, assembler, linker, libraries, tools, and applications are
ordinary packages acquired afterward.

Use per-user installation first. Machine-wide installation, background updates,
third-party sources, public search, and arbitrary installer scripts remain outside
the initial boundary. The launcher activates immutable client generations so a
Windows process never overwrites its own running executable and the previous
client remains recoverable.

## Implementation order

1. Require a green dual-host qualification result for the merged native
   compiler/database baseline before package implementation changes begin.
2. Generalize Package 1 and Lock 1 admission with a second real package rather
   than copying the WVDB Query digest-pinned shell.
3. Specify and implement one deterministic package bundle and independently
   verify it before extraction or execution.
4. Implement Release discovery parsing and offline signature verification before
   enabling network retrieval.
5. Implement the immutable content store, private staging, atomic object
   publication, and dry-run reachability inventory.
6. Implement complete installation generations, activation, rollback, and
   separate capability approvals.
7. Package the `wv` launcher/client for Windows and Linux, then deploy the signed
   metadata endpoint and immutable GitHub assets from one qualified source state.
8. Add network channel update and client self-update only after failure,
   freshness, rotation, and recovery evidence passes on both hosts.

## Consequences and non-claims

- Windvale owns its stable source name, release selection, trust, identity, and
  rollback semantics while GitHub remains the initial durable byte archive.
- The initial server is small and replaceable; clients need no GitHub API and can
  use an offline directory with the same logical object layout.
- Release discovery adds a security-sensitive parser and signature verifier that
  require hostile-input, key-custody, rotation, and cross-host evidence.
- This proposal does not claim TUF conformance or equivalent freshness and mirror
  protection. Complete TUF adoption remains a later decision if automatic
  background updates or multiple public sources require it.
- No DNS record, endpoint, signing key, release metadata, package bundle, content
  store, installer, or updater is created by this decision.

## Reconsideration triggers

Reconsider the hybrid source if immutable GitHub assets cannot meet artifact size,
availability, legal, or retention needs; if an independent mirror becomes
operationally necessary; if a complete TUF deployment is justified; if qualified
civil time is unavailable to the updater; or if signing-key custody cannot meet
the Root and release-role separation.
