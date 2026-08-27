# Windvale installer 1

## Status

Qualified and implemented for the deterministic `stable` channel under
[Decisions 0562](../Documents/Decisions/0562-First-Deterministic-Development-Installers.md)
and [0565](../Documents/Decisions/0565-First-Stable-Preview-Installers.md).
Decision 0562's exact development artifacts pass on both hosts in
[Verify run 31881681424](https://github.com/eworker-inc/Windvale/actions/runs/31881681424).
The channel-neutral owner, including the stable artifacts, passes on both hosts
in [Verify run 31885759856](https://github.com/eworker-inc/Windvale/actions/runs/31885759856).
The default development input exercises the exact compressed `0.2.0-dev.1`
candidate under [Decision 0749](../Documents/Decisions/0749-Compress-Successor-Installer-Archives.md).
[Decision 0800](../Documents/Decisions/0800-Target-Windvale-1.0-Directly.md)
leaves that already-created identity intact as historical
development evidence; it does not select a `v0.2.0` product release.
Its local identities below remain candidates until the same owner reports them
from both permanent hosts.

## Purpose and scope

Installer 1 turns the checked-in native Windows and Linux x64 tools into two
deterministic, inspectable per-user installation archives per selected channel.
The `development` channel currently selects the exact historical compressed
`0.2.0-dev.1` candidate.
The `stable` channel preserves the release-labeled `0.1.0` artifacts consumed
by Release Envelope 1 byte for byte.

The two target archives are also the first release-repository selection
boundary: a future bootstrap installer selects only the archive matching the
machine target. The repository may retain both targets and older immutable
releases without placing them in one machine's download or installation.

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
bounded notice, small `wv` client, and offline payload verifier. `wv` exposes
`version`, `tools`, `doctor`, `run`, and `help`; it resolves only sibling
installation files. `wv run` composes the installed compiler, verifier, and
bounded runner according to [Windvale scripting 1](Windvale-Scripting.md). The
native tools remain separate commands so their complete argument lists are not
reinterpreted by a shell dispatcher.

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
| development | Windows x64 | `windvale-0.2.0-dev.1-windows-x64.zip` | 5,425,920 | `f4c3d15cde199230688682df6413cc6564b0cbc5979cdfc78b92af3498a62849` | `e0b5e51960a555af38aae1ebf992a1e8f4fb89d114e1dbd4f19da4f4c4062bb4` |
| development | Linux x64 | `windvale-0.2.0-dev.1-linux-x64.tar.gz` | 5,420,897 | `043176b3499e9a7a882324b463da3a5f1a8916d02e68a0031e8abb0b73b00642` | `a7847d68db5af77dcc7abbac808a029ae4c959db4559a28a73ac8ffd3018eb39` |
| stable | Windows x64 | `windvale-0.1.0-windows-x64.zip` | 42,941,959 | `4c05155fc41a3821afeffe0645614f3e80d6a77bf7685c6147ff93000bfa44c4` | `af1f41a495fc1067f2bb7f002dc93d31a462a6e2cda7c86e4d22bf9e9cf2d178` |
| stable | Linux x64 | `windvale-0.1.0-linux-x64.tar.gz` | 42,951,906 | `af40e4d1c58ecb40aa9c75678081625f4171abf1116e7d5d7362e97ea8c0c58e` | `5843921a7c6677fde0bf25b08841099ec8b3a601ea4244af82cb29011ae8bcad` |

The stable ZIP uses stored entries. Its Linux peer uses deterministic
uncompressed DEFLATE blocks. The development successor instead uses raw zlib
DEFLATE level 6, memory level 8, the default strategy, and a 32 KiB window for
each ZIP file and for the complete Linux tar stream. The builder supplies the
ZIP records and the gzip header, CRC-32, and input-size trailer itself, so zlib
does not supply variable container metadata. Exact archive identities remain
pinned and the permanent owner rejects any producer-version drift.

Both generations use UTF-8 names, fixed epoch metadata, ordered files,
explicit Unix-compatible modes, USTAR with zero uid/gid/mtime on Linux, and no
extra fields, owners, random identifiers, or comments.
Each target has one exact format and canonical filename, and its complete
expanded ordinary-file payload may not exceed 64 MiB. A consumer must admit the
archive's exact transport length and SHA-256 before extraction; compression
does not weaken the existing manifest verification after extraction.

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
6. install twice, run `wv version`, `wv doctor`, `wvverify`, and one script
   through the installed `wv run` route;
7. detect tampering inside the installed immutable generation; and
8. uninstall while preserving an external sentinel.

The owner reports bounded phase progress before its final summary. Cross-host
installer conformance is claimed only after the same exact owner passes on both
permanent hosts.

## Explicit non-claims

Installer 1 is not code-signed, notarized, automatically updated, machine-wide,
or registered with an OS package manager. The development successor is
compressed for delivery; the stable `0.1.0` archives remain stored and retain
their published identities.
The stable archives rely on Release Envelope 1 for distribution authenticity;
an archive copied outside that envelope has no independent signature. Installer
1 does not implement a general package resolver, native recovery launcher,
multi-generation activation, rollback, repair, or garbage collection.

Decision 0590's portable Generation 1 / Activation 1 parser and transition
planner begin the successor lifecycle contract without changing these published
Installer 1 bytes or claiming that host activation and rollback are implemented.
