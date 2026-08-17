# Windvale release discovery 1 architecture

## Status and purpose

This architecture defines the proposed signed metadata and transport boundary
between one installed Windvale bootstrap and official release objects. Root 1,
Channel 1, Release 1, Signature 1, network acquisition, and freshness handling
are not implemented. Package 1 and Lock 1 remain the implemented package
distribution metadata, while Decision 0750 adds the narrower local Installer
Repository 1 index, verifier, and target/profile selector. That index is ready
to become one signed release subject but is not itself public discovery,
freshness, or network trust. The exact WVDB Query front door remains local and
source-locked. The hosting choice and rollout order are recorded in the
[hybrid official-source proposal](../Project/Windvale-Package-Source-Proposal.md).

The official logical source is `windvale.official`. Its default network endpoint
is `https://packages.windvale.ca/v1`. GitHub immutable releases retain the exact
public objects and recovery evidence behind that endpoint. A source endpoint,
redirect, GitHub repository, tag, release page, or TLS certificate is an origin;
it is not an artifact identity or a Windvale trust root.

Release discovery keeps five objects distinct:

1. Root 1 authorizes replaceable release-signing keys.
2. Channel 1 selects one current signed release for a named channel.
3. Release 1 inventories exact immutable release objects.
4. Signature 1 carries detached threshold signatures for metadata.
5. Package bundles, source archives, tools, evidence, and other artifacts remain
   content objects named by Release 1 rather than embedded in discovery metadata.

The proposed Bundle 1, immutable store, Generation 1, activation, rollback, and
launch transaction are defined separately in the
[package bundle and installation architecture](Windvale-Package-Bundle-And-Installation.md).

The proposed contract is deliberately smaller than The Update Framework. It does not
claim TUF protection, delegated targets, consistent-snapshot behavior, or online
timestamp-role protection. Reconsider adopting complete TUF roles before an
automatic background updater, third-party package source, or public mirror
network is enabled.

## Common canonical text rules

Root 1, Channel 1, Release 1, and Signature 1 are strict UTF-8 without a byte-order
mark and use LF line endings. Each contains one record per nonempty line. Blank
lines, comments, leading or trailing whitespace, repeated separators, escapes,
control characters, and trailing tokens are invalid. Every file ends with one LF.

Identifiers use lowercase ASCII letters, digits, `-`, and `.`, begin and end with
a letter or digit, and contain no empty dotted component. Versions additionally
permit `+`. SHA-256 identities are 64 lowercase hexadecimal digits. Ed25519 public
keys are 64 lowercase hexadecimal digits and signatures are 128 lowercase
hexadecimal digits. Counts and Unix seconds are canonical unsigned decimal values
without leading zeros.

Release target identifiers use the same syntax but are independent of package
manifest versions. The initial values are `any`, `hosted-wvb-v1`,
`windows-x64-hosted`, and `linux-x64-hosted`. `any` is valid only for bytes whose
meaning and admission are platform-independent; it does not make a host executable
portable.

Key identifiers are the SHA-256 digest of the exact 32 raw Ed25519 public-key
bytes. Unknown required record names, algorithms, roles, or versions are rejected.

The first bounds are:

- Root, Channel, and Signature files: at most 65,536 bytes each.
- Release files: at most 1,048,576 bytes.
- Root or release keys: at most 16 per role.
- Signatures: at most 32 per Signature file.
- Release artifacts: at most 4,096.
- One content object: at most 2,147,483,648 bytes.

All parsers reject before publication when a bound, ordering rule, digest, size,
threshold, time, or relationship is invalid.

## Root 1

Root 1 begins with:

```text
windvale-root 1
```

The remaining records are exactly:

```text
source <source-id>
generation <generation>
expires <unix-seconds>
root-threshold <count>
release-threshold <count>
root-key <key-id> ed25519 <public-key-hex>
release-key <key-id> ed25519 <public-key-hex>
```

There is one `source`, `generation`, `expires`, `root-threshold`, and
`release-threshold` record. One or more `root-key` and `release-key` records
follow, ordered by role and then key identifier. Identifiers and public keys are
unique across the complete file. Each threshold is nonzero and no greater than
the number of keys in its role.

The bootstrap embeds the exact digest and bytes of its initial Root 1 plus the
root public keys needed to authenticate it. A replacement Root must:

- name the same source;
- have generation exactly one greater than the locally trusted Root;
- be unexpired according to qualified civil time;
- satisfy the old Root's root threshold; and
- satisfy its own root threshold.

The same new Root bytes are signed by both sets during rotation. A client publishes
the replacement only after all signatures and relationships pass, and retains the
previous Root as recovery evidence. Skipping generations and silent trust reset
are invalid. An expired or unrecoverable Root requires an explicit offline recovery
or a new bootstrap installation; network metadata cannot repair its own trust
anchor.

## Channel 1

Channel 1 begins with:

```text
windvale-channel 1
```

The remaining records are exactly:

```text
source <source-id>
channel <channel-id>
sequence <sequence>
expires <unix-seconds>
release <sha256> <bytes>
```

The initial official channel identifiers are `stable`, `preview`, and
`development`. A Channel is accepted only when its Signature 1 satisfies the
currently trusted Root's release threshold, it names the configured source and
channel, it has not expired, and its sequence is no lower than the highest
accepted sequence stored for that source and channel.

The client records the highest accepted sequence through private-write,
reread/verification, and atomic publication. It does not advance that value until
the complete Channel and selected Release are admitted. An explicitly requested
local rollback activates an already installed generation; it does not lower the
network channel sequence or reinterpret older network metadata as current.

When qualified civil time is unavailable, `wv` may install an explicitly named
release digest from an offline source, but it must not claim that a network channel
is current.

## Release 1

Release 1 begins with:

```text
windvale-release 1
```

The fixed records are:

```text
source <source-id>
name <release-id>
version <version>
sequence <sequence>
created <unix-seconds>
source-revision <git-sha1|git-sha256> <revision-hex>
artifact <role> <artifact-id> <target-id> <sha256> <bytes>
```

There is one of every fixed non-artifact record and one or more artifacts. A
`git-sha1` revision has exactly 40 lowercase hexadecimal digits and `git-sha256`
has exactly 64. The Channel sequence equals the Release sequence. Artifact records
are ordered by role, artifact identifier, and target identifier. The tuple and
digest are unique.

The initial roles are:

- `bootstrap` for the independently installable Windows or Linux `wv` entry;
- `package` for a canonical Windvale package bundle;
- `source` for the exact source archive;
- `license` for license and dependency inventory;
- `provenance` for build provenance or attestations;
- `qualification` for Windows/Linux qualification evidence; and
- `recovery` for retained recovery inputs and instructions.

Release metadata does not contain an artifact URL. A source resolves an admitted
artifact digest through the object path defined below. Consequently changing a
mirror or redirect does not change Release bytes. Every executable remains subject
to its format-specific admission after its object digest and size pass.

Release versions are display and compatibility evidence. The Release digest,
sequence, exact artifact identities, and selected installation generation control
admission and activation.

## Signature 1

Signature 1 begins with:

```text
windvale-signature 1
```

It then contains:

```text
subject <root|channel|release> <sha256> <bytes>
signature <key-id> ed25519 <signature-hex>
```

There is exactly one `subject` and one or more signatures ordered by key
identifier. Duplicate key identifiers are invalid and never count twice toward a
threshold. Before signature verification, the client checks the subject's exact
byte count and SHA-256 identity.

Each Ed25519 signature is over the following byte sequence:

```text
Windvale <root|channel|release> 1 signature\0<exact-subject-bytes>
```

The prefix is ASCII, the separator is one zero byte, and the subject is the exact
canonical metadata file including its final LF. Ed25519 behavior follows
[RFC 8032](https://www.rfc-editor.org/rfc/rfc8032). Signature verification uses
the key role and threshold selected by the currently trusted Root; a cryptographic
signature does not authorize capabilities, installation scope, or execution.

## Official source transport

The official endpoint exposes these stable paths:

```text
/v1/root.wvroot
/v1/root.wvroot.sig
/v1/channels/<channel>.wvchannel
/v1/channels/<channel>.wvchannel.sig
/v1/releases/sha256/<digest>.wvrelease
/v1/releases/sha256/<digest>.wvrelease.sig
/v1/objects/sha256/<digest>
```

`packages.windvale.ca` serves the small metadata objects directly. A content-object
request may return the exact bytes or redirect to the corresponding immutable
GitHub Release asset. The client:

- uses HTTPS for every network hop and forbids protocol downgrade;
- sends no package authority, local path, approval, or secret to the source;
- follows at most five redirects;
- applies the declared and global size bounds while streaming to a private file;
- verifies exact size and SHA-256 before publication;
- treats timeouts, truncation, excess bytes, inconsistent range behavior, and
  indeterminate retrieval as failure; and
- never executes a downloaded object as part of retrieval or verification.

The client does not query a mutable GitHub `latest` endpoint to decide identity.
GitHub release tags, assets, and attestations are useful independent evidence, but
the signed Channel and Release select the exact Windvale objects.

Local directories and removable media may implement the same logical paths without
network access. An alternate source is an explicit user or administrator policy
input; package metadata cannot add a source or trust key.

## Bootstrap and capability boundary

The first host installer places one small native launcher, the current verified
`wv` client generation, initial Root 1, and the official source configuration. It
does not install the compiler or grant general application capabilities.

The bootstrap's network, civil-time, content-store mutation, generation activation,
and self-update authority is internal package-manager authority. It is never
inherited by an installed tool or application. Package capability requirements,
owner approval, provider availability, and rights-limited launch binding remain
separate evidence.

Self-update downloads a `bootstrap` artifact into a new immutable client generation,
admits its host container and embedded identities, and changes the active client
pointer only after the running client exits. The previous client remains available
for bounded recovery. A running Windows executable is not overwritten in place.

## Required verification

Before network release discovery becomes ordinary, Windows and Linux evidence must
cover:

- canonical parsing, every limit, malformed UTF-8, truncation, extra data, and
  noncanonical order;
- digest, size, key identifier, signature, threshold, expiry, and sequence failures;
- initial trust, valid rotation, missing-old-signature, missing-new-signature,
  skipped-generation, and expired-root cases;
- redirect bounds, HTTPS downgrade, timeout, partial retrieval, excess bytes,
  corrupt content, and destination preservation;
- offline retrieval of the same exact Release and objects;
- no activation before complete release and target admission; and
- deterministic metadata bytes and matching reports on Windows and Linux.

This evidence qualifies release discovery only. Package-bundle admission, content
store publication, installed generations, capability launch, and bootstrap
self-replacement retain their own focused contracts and tests.
