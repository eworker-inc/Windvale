# Windvale development installer 1

## Status

Implemented candidate for the first bounded Milestone 3 installer slice under
[Decision 0562](../Documents/Decisions/0562-First-Deterministic-Development-Installers.md).
This contract does not define or announce the Windvale `v0.1.0` release.

## Purpose and scope

Development installer 1 turns the checked-in native Windows and Linux x64 tool
artifacts into two deterministic, inspectable per-user installation archives.
It provides an unsigned local flow before the signed release envelope, release
key policy, native recovery launcher, installed-generation rollback, updater,
and final release qualification are complete.

The exact label is `0.1.0-dev.1`. It is a prerelease artifact label, not a Git
release tag and not a compatibility promise.

## Declared inputs

`Distribution/Installers/Windvale-Development-Installer.json` is the bounded
input envelope. It declares exactly two targets and seven native tools per
target:

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
builder canonicalizes repository-maintained text to LF before admitting the
declared license bytes, so checkout line-ending policy cannot change an archive
identity. Undeclared build caches, managed binaries, SDKs, WVDB, application
data, credentials, and host configuration cannot enter the payload.

The payload also contains the source license, a version record, a bounded
development notice, a small `wv` inspection client, and an offline payload
verifier. `wv` exposes only `version`, `tools`, `doctor`, and `help`; it resolves
only sibling installation files. The native tools remain separate commands so
their complete argument lists are not reinterpreted by a shell dispatcher.

## Deterministic artifacts

The builder command is:

```text
node Tools/Release/Build-Development-Installers.mjs build <existing-empty-output-directory>
```

It constructs both targets on either host and refuses changed declared inputs,
unsafe paths, an unknown target set, duplicate payload paths, a nonempty output
directory, or an existing output artifact.

| Target | Artifact | Bytes | SHA-256 | Payload manifest SHA-256 |
| --- | --- | ---: | --- | --- |
| Windows x64 | `windvale-0.1.0-dev.1-windows-x64.zip` | 38,351,998 | `2c2112bef12e89b0594e2510b5ea71318b4c9ff8979b35c7fa7c20ca8703a186` | `6147bfdb0c4b91d34b157ecb3bce8a857157f5d0c885d2579ec7ae9cd7fcf7c1` |
| Linux x64 | `windvale-0.1.0-dev.1-linux-x64.tar.gz` | 38,362,500 | `dc65a1091e918b8d73106cc6c4bb9bd1a3a905b42601eacd32453e0a073e5937` | `f6f96c6df5fcdf12a70bdffd7a7ccc622a77840f5c78762c8f520b3f2c93ce06` |

The ZIP uses stored entries, UTF-8 names, fixed DOS epoch metadata, no extra
fields, ordered files, and explicit Unix-compatible modes. The Linux artifact
uses ordered USTAR files with zero uid, gid, and mtime followed by a fixed gzip
header and deterministic uncompressed DEFLATE blocks. Neither artifact uses a
host archive utility or admits variable timestamps, owners, random identifiers,
comments, or compression-library output.

The archive contains one canonical `Payload-Manifest.txt`. Its digest selects
the immutable installation-generation directory. Every listed file record owns
the relative path, exact length, mode, and SHA-256. The archive digest remains a
separate transport identity; a later release envelope will sign that identity.

## Per-user installation

Windows installs below the current user's local application-data directory by
default. Linux installs below `${XDG_DATA_HOME}/windvale`, falling back to
`${HOME}/.local/share/windvale`; command links default to
`${XDG_BIN_HOME}` or `${HOME}/.local/bin`. Both installers accept explicit roots
for testing and local policy. Neither requires elevation.

The installer:

1. verifies the complete extracted payload before creating the install root;
2. copies only manifest-listed files into one private candidate generation;
3. verifies the candidate again;
4. renames it to its payload-derived immutable generation;
5. publishes fixed command shims that resolve only that generation; and
6. writes one exact installation record used to bound uninstall.

An existing matching generation is verified and reused. A mismatching existing
generation is corruption and is not replaced. Windows changes the per-user PATH
only when `-AddToPath` is explicit. Linux refuses to replace an unrelated command
or link. The installed stable `wv doctor` shim pins the selected payload-manifest
identity, so rewriting a payload and its manifest together does not create an
admitted installation. Installation does not grant capabilities or create
application mutable storage.

Uninstall requires the exact installation record, removes only matching command
links and the selected Windvale development installation root, and leaves
separately owned files and application state unchanged. This first slice owns
clean first install and complete uninstall; multi-generation activation,
self-update, rollback, reachability collection, and interrupted activation remain
later contracts.

## Required owner

`development-installers` runs the same eight-case owner on Windows and Linux:

1. construct both target artifacts twice;
2. prove byte-for-byte reproducibility and the pinned identities;
3. admit both exact archives and reject a corrupt transport;
4. extract the current-host archive through the exact verifier;
5. reject a tampered payload before installation;
6. install twice, run the client doctor, and invoke the native WVB verifier;
7. detect tampering inside the installed immutable generation; and
8. uninstall while preserving an external sentinel.

The owner reports bounded phase progress before its final summary. A passing
current-host report is development evidence; cross-host installer conformance is
claimed only after the exact owner passes on both permanent hosts.

## Explicit non-claims

Development installer 1 is not signed, notarized, code-signed, compressed for
download efficiency, automatically updated, machine-wide, registered with an OS
package manager, or published as `v0.1.0`. It does not implement the final native
`wv` recovery launcher, release trust roots, application installation,
capability approval records, general activation, rollback, repair, or garbage
collection. Those remain explicit Milestone 3 work rather than hidden installer
behavior.
