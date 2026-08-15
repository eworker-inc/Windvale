# Windvale installer 1

## Status

Qualified and implemented for deterministic `development` and `stable` channels under
[Decisions 0562](../Documents/Decisions/0562-First-Deterministic-Development-Installers.md)
and [0565](../Documents/Decisions/0565-First-Stable-Preview-Installers.md).
Decision 0562's exact development artifacts pass on both hosts in
[Verify run 31881681424](https://github.com/eworker-inc/Windvale/actions/runs/31881681424).
The channel-neutral owner, including the stable artifacts, passes on both hosts
in [Verify run 31885759856](https://github.com/eworker-inc/Windvale/actions/runs/31885759856).

## Purpose and scope

Installer 1 turns the checked-in native Windows and Linux x64 tools into two
deterministic, inspectable per-user installation archives per selected channel.
The `development` channel retains the qualified `0.1.0-dev.1` input. The
`stable` channel selects the release-labeled `0.1.0` artifacts consumed by
Release Envelope 1.

A stable label does not make an archive official by itself. The project owner
must still sign its exact transport identity through the release envelope,
accept the selected-state qualification, and publish the immutable product tag.

## Declared inputs

`Distribution/Installers/Windvale-Development-Installer.json` and
`Distribution/Installers/Windvale-Release-Installer.json` are bounded
`windvale-installer-input-1` envelopes. Each declares exactly two targets and
seven native tools per target:

| Installed command | Selected implementation |
| --- | --- |
| `wvbuild` | Native source/project build driver |
| `wvasm` | Native WVA assembler |
| `wvlink` | Native Wv-Linker |
| `wvrun` | Bounded native WVB runner |
| `wvdump` | Read-only WVB inspector |
| `wvverify` | Semantic WVB verifier |
| `wvpublish` | Verified WVB publisher |

Every selected source path has an exact byte length and SHA-256 identity. The
builder canonicalizes declared text to LF before checking its identity, so
checkout line endings cannot change an archive. Undeclared build caches,
managed binaries, SDKs, WVDB, application data, credentials, and host
configuration cannot enter the payload.

The payload also contains the source license, channel and version record,
bounded notice, small `wv` inspection client, and offline payload verifier.
`wv` exposes only `version`, `tools`, `doctor`, and `help`; it resolves only
sibling installation files. The native tools remain separate commands so their
complete argument lists are not reinterpreted by a shell dispatcher.

## Deterministic artifacts

The default command constructs the development channel:

```text
node Tools/Release/Build-Installers.mjs build <empty-output-directory>
```

The release command names its checked-in stable input explicitly:

```text
node Tools/Release/Build-Installers.mjs build <empty-output-directory> Distribution/Installers/Windvale-Release-Installer.json
```

The builder works on either host and refuses changed declared inputs, unsafe
paths, an unknown target set, duplicate payload paths, a nonempty output
directory, an input outside `Distribution/Installers/`, or an existing output.

| Channel | Target | Artifact | Bytes | SHA-256 | Payload manifest SHA-256 |
| --- | --- | --- | ---: | --- | --- |
| development | Windows x64 | `windvale-0.1.0-dev.1-windows-x64.zip` | 38,351,998 | `2c2112bef12e89b0594e2510b5ea71318b4c9ff8979b35c7fa7c20ca8703a186` | `6147bfdb0c4b91d34b157ecb3bce8a857157f5d0c885d2579ec7ae9cd7fcf7c1` |
| development | Linux x64 | `windvale-0.1.0-dev.1-linux-x64.tar.gz` | 38,363,012 | `cbeddb17e258307b6005f5746925c5a4c3d68affca6495308abc6578d9294850` | `f6f96c6df5fcdf12a70bdffd7a7ccc622a77840f5c78762c8f520b3f2c93ce06` |
| stable | Windows x64 | `windvale-0.1.0-windows-x64.zip` | 38,351,745 | `8e6e5dcd16ae437933e0eab739e84f5c48bf1d4045089495dccdef7f2de7deee` | `8a09172a7a8c8fec62ef2218a3f23f3bdbf443337d92149b544ef73845aa5732` |
| stable | Linux x64 | `windvale-0.1.0-linux-x64.tar.gz` | 38,363,012 | `4c99bda1b98156493df77b5e7b337265517c573e9ea3554fad2979315e88c11a` | `83472405b2b4255f43158a673dbc098caa58ec442f100ea963ed19d1c78e0d59` |

The ZIP uses stored entries, UTF-8 names, fixed DOS epoch metadata, no extra
fields, ordered files, and explicit Unix-compatible modes. The Linux artifact
uses ordered USTAR files with zero uid, gid, and mtime followed by a fixed gzip
header and deterministic uncompressed DEFLATE blocks. Neither admits variable
timestamps, owners, random identifiers, comments, or compression-library
output.

The archive contains one canonical `Payload-Manifest.txt`. Its digest selects
the immutable installation generation. Every listed file record owns its path,
length, mode, and SHA-256. The archive digest is a separate transport identity
selected by the signed release envelope.

## Per-user installation

Windows installs below the current user's local application-data directory by
default. Linux installs below `${XDG_DATA_HOME}/windvale`, falling back to
`${HOME}/.local/share/windvale`; command links default to `${XDG_BIN_HOME}` or
`${HOME}/.local/bin`. Both accept explicit roots for testing and local policy.
Neither requires elevation.

The installer verifies the complete extracted payload, copies only
manifest-listed files into a private candidate, verifies it again, renames it
to the payload-derived immutable generation, publishes fixed command shims, and
writes one exact channel-specific installation record. An existing matching
generation is reused; a differing one is corruption. Windows PATH mutation is
explicit. Linux refuses to replace an unrelated command or link.

Uninstall requires the exact installation record, removes only matching command
links and the selected Windvale installation root, and leaves separately owned
files and application state unchanged. Multi-generation activation,
self-update, rollback, repair, reachability collection, and interrupted
activation remain later contracts.

## Required owner

`installers` runs the same eight-case owner on Windows and Linux:

1. construct both channels and both targets twice;
2. prove byte-for-byte reproducibility and all four pinned identities;
3. admit the stable archives and reject corrupt transport;
4. extract the current-host stable archive through its exact format reader;
5. reject a tampered payload before installation;
6. install twice, run `wv version` and `wv doctor`, and invoke `wvverify`;
7. detect tampering inside the installed immutable generation; and
8. uninstall while preserving an external sentinel.

The owner reports bounded phase progress before its final summary. Cross-host
installer conformance is claimed only after the same exact owner passes on both
permanent hosts.

## Explicit non-claims

Installer 1 is not code-signed, notarized, compressed for download efficiency,
automatically updated, machine-wide, or registered with an OS package manager.
The stable archives rely on Release Envelope 1 for distribution authenticity;
an archive copied outside that envelope has no independent signature. Installer
1 does not implement a general package resolver, native recovery launcher,
multi-generation activation, rollback, repair, or garbage collection.

Decision 0590's portable Generation 1 / Activation 1 parser and transition
planner begin the successor lifecycle contract without changing these published
Installer 1 bytes or claiming that host activation and rollback are implemented.
