# Windvale installer repository version 1

## Status and purpose

Installer Repository 1 is the implemented development candidate for selecting
one host target and one named component profile without acquiring the complete
Windvale tool distribution. Its producer, independent verifier, selector, and
twelve-case Windows owner are implemented. Independent Linux execution remains
required before a paired-host qualification claim.

The checked-in `0.2.0-dev.1` input pins the current repository identity. It is a
historical development input, not a signed or published repository and not a
selected `v0.2.0` release under
[Decision 0800](../Documents/Decisions/0800-Target-Windvale-1.0-Directly.md). A future release
envelope can carry the exact index as one `repository|all` artifact. The release
signature then binds the index bytes, and the admitted index binds every
content-addressed blob. Signing each blob separately is unnecessary.

## Canonical index

`Repository-Index.txt` is strict UTF-8 without a byte-order mark, uses LF, ends
in one LF, and contains exactly:

```text
windvale-installer-repository 1
version <version>
channel <development|stable>
expanded-limit <bytes>
target-count 2
target linux-x64
target windows-x64
component-count <count>
component <component-id>
profile-count <count>
profile <profile-id> <component-count> <component-id>...
object-count <count>
blob-count <count>
object <component-id> <all|linux-x64|windows-x64> <path> <mode> <sha256> <bytes> gzip-1 <blob-sha256> <blob-bytes>
```

Components, profiles, and objects are bytewise ordered and unique. Component
and profile identifiers use lowercase ASCII letters, digits, `-`, and `.` and
are at most 64 bytes. Paths are relative ASCII-safe installation paths with at
most 1,024 bytes; empty, `.`, `..`, absolute, repeated-separator, linked, and
special-file paths are rejected. Modes are four-digit octal text.

The first bounds are:

- index: at most 1,048,576 bytes;
- objects: 1..256;
- components: 1..64;
- profiles: 1..16;
- one expanded object: at most 67,108,864 bytes; and
- one selected target/profile expansion: at most 67,108,864 bytes.

Every component must occur in at least one profile and every profile component
must have an object for each supported target, either directly or through an
`all` object. A selected profile cannot repeat an installation path.

## Objects and compression

Blobs live at `Objects/sha256/<blob-sha256>`. The index binds both the compressed
blob identity and the expanded file identity. `gzip-1` is the canonical gzip
container over the shared `deflate-1` producer used by successor installers:
raw zlib DEFLATE, level 6, memory level 8, default strategy, and a 32 KiB
window, with a deterministic gzip header and trailer.

Complete verification rejects missing, extra, linked, special, oversized, or
identity-changing entries before accepting the repository. Decompression is
bounded by the declared expanded size. Construction publishes the index only
after every blob, making index presence the local publication marker. Selection
reads and admits only the index, so a future client can decide which blob
identities to acquire without requiring unselected blobs to be present locally.

## Initial profiles

The development input defines:

| Profile | Components |
| --- | --- |
| `runtime` | `base`, `runner`, `verifier` |
| `developer` | `assembler`, `base`, `compiler`, `linker`, `runner`, `verifier` |
| `publisher` | `base`, `inspector`, `publisher`, `verifier` |
| `full` | all eight components |

The current deterministic repository has 15 objects and 15 unique blobs. The
index is 3,546 bytes with SHA-256
`c44bec65ab7b235a22e1e4d24d98f3eb2249f6f21031ddefbe2bbe5c4a6b4ef3`;
the blobs total 9,502,931 bytes.

| Target | Profile | Objects | Download bytes | Expanded bytes |
| --- | --- | ---: | ---: | ---: |
| Windows x64 | `runtime` | 3 | 656,576 | 3,091,393 |
| Linux x64 | `runtime` | 3 | 655,076 | 3,093,441 |
| Windows x64 | `developer` | 6 | 4,307,743 | 37,212,609 |
| Linux x64 | `developer` | 6 | 4,303,735 | 37,217,217 |
| Windows x64 | `publisher` | 4 | 742,573 | 3,435,457 |
| Linux x64 | `publisher` | 4 | 739,890 | 3,434,422 |
| Windows x64 | `full` | 8 | 4,757,006 | 39,378,881 |
| Linux x64 | `full` | 8 | 4,751,050 | 39,380,918 |

These sizes are evidence for the pinned development input, not permanent
budgets for future releases.

## Commands

Create a repository in an existing empty directory:

```text
node Tools/Release/Build-Installer-Repository.mjs build <output-directory> [input]
```

Verify a complete local repository:

```text
node Tools/Release/Verify-Installer-Repository.mjs verify <repository>
```

Select the exact objects for one target and profile:

```text
node Tools/Release/Verify-Installer-Repository.mjs select <repository> <target> <profile>
```

Selection emits canonical `windvale-installer-selection 1` text containing the
index version, target, profile, object and blob counts, download and expanded
byte totals, and every selected object's two identities.

## Boundary and nonclaims

Installer Repository 1 owns deterministic construction, bounded admission,
content identity, target selection, and component-profile selection. It does
not perform network discovery, signature verification, downloading, retry or
resume, installation, generation publication, activation, rollback, uninstall,
or repository garbage collection. Those operations remain separate contracts.
