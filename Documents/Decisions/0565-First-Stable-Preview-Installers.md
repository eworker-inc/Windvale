# Decision 0565: First stable preview installers

- Status: Implemented; paired qualification pending
- Date: 2026-08-15
- Advances: Milestone 3 and Decisions 0562 and 0563
- Contract: [Windvale installer 1](../../Specifications/Windvale-Installer.md)

## Context

The signed release-envelope implementation and selected-state qualification
were ready to consume deterministic installers, but the only pinned archives
still identified themselves as `0.1.0-dev.1`. The release naming policy
correctly prohibits publishing those prerelease artifacts as `v0.1.0`.
Renaming the files without changing their internal channel, version, generation,
notice, and installation record would create misleading product bytes.

The development and stable payloads select the same exact native tools. They
need one construction and verification implementation, while retaining distinct
metadata and archive identities.

## Decision

Generalize Installer 1 around a checked-in `windvale-installer-input-1`
envelope with an explicit `development` or `stable` channel. Keep the default
builder input at `0.1.0-dev.1`; require release construction to name
`Distribution/Installers/Windvale-Release-Installer.json` explicitly.

Preserve the two qualified development archives byte-for-byte. Pin separate
stable artifacts:

| Target | Artifact | Bytes | SHA-256 | Payload manifest SHA-256 |
| --- | --- | ---: | --- | --- |
| Windows x64 | `windvale-0.1.0-windows-x64.zip` | 38,351,745 | `8e6e5dcd16ae437933e0eab739e84f5c48bf1d4045089495dccdef7f2de7deee` | `8a09172a7a8c8fec62ef2218a3f23f3bdbf443337d92149b544ef73845aa5732` |
| Linux x64 | `windvale-0.1.0-linux-x64.tar.gz` | 38,363,012 | `4c99bda1b98156493df77b5e7b337265517c573e9ea3554fad2979315e88c11a` | `83472405b2b4255f43158a673dbc098caa58ec442f100ea963ed19d1c78e0d59` |

Give the stable payload an exact `0.1.0` version and `stable` channel record,
release-specific notice, stable generation identity, and
`windvale-installation 1` uninstall record. Keep the development record and
notices unchanged in the development bytes.

Rename the shared builder and permanent owner to their actual channel-neutral
scope. The eight-case `installers` owner constructs both channels twice, proves
all four exact archive identities, and exercises stable corruption rejection,
installation, execution, tamper detection, and bounded uninstall on each host.

Do not treat the stable label as a signature. Release Envelope 1 must select the
archive digest, the owner-controlled root/release ceremony must sign the exact
envelope, and the selected commit must pass the deliberate dual-host release
gate before the archives or `v0.1.0` tag are official.

## Consequences

- The release no longer needs to mislabel or mutate qualified development
  artifacts during a signing ceremony.
- One builder and one payload/install contract own both channels without copied
  scripts or divergent format readers.
- Development users retain the existing exact archive identities.
- Stable archive authenticity still depends on the signed release envelope;
  platform code signing, notarization, automatic update, and rollback remain
  later product work.

## Reconsideration triggers

Reconsider this decision when a second stable version requires compatibility
policy, when an updater needs multiple active generations and rollback, when
native launch recovery replaces shell bootstrap, or when platform distribution
requires code signing or package-manager integration.
