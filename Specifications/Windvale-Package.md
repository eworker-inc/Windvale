# Windvale Package 1 and Lock 1

## Status and purpose

Package 1 (`.wvpack`) and Lock 1 (`.wvlock`) define the first native-owned local
source-package contract. The implemented instances package the WVDB Query
application with its reusable libraries and the independent WVB inspector. Each
builds the same canonical WVB from exact checked-in inputs on Windows and Linux.

This contract is separate from Workspace 1 and Project 2. A project selects source
inputs for one compilation. A package names parts, dependency edges, target scope,
license, and required capabilities. A lock records one fully selected source graph
and every byte identity used to produce the output.

Decision 0561 separately implements the bounded Bundle 1 and immutable-publication
subset used by WVDB Query. Decisions 0563 through 0566 implement the first signed
offline release and installer custody path. Generation 1 / Activation 1 now define
and implement the portable installed-state parser/planner under Decision 0590.
A general resolver, registry, streaming store service, runtime WVB linker, and
online updater are not implemented by Package 1. Decision 0590's separate host
adapter now publishes and recovers exact caller-validated Activation 1 bytes.

## Text and naming rules

Both formats are strict UTF-8 without a byte-order mark, use LF line endings, and
contain one record per nonempty line. Comments, blank lines, leading or trailing
whitespace, repeated separators, escapes, and trailing tokens are invalid. The
current files are bounded to 65,536 bytes.

Package, part, target, resolver, and capability identifiers are ASCII-safe.
Package and part identifiers use lowercase ASCII letters, digits, `-`, and `.`,
begin and end with a letter or digit, and contain no empty dotted component. Paths
are canonical workspace-relative paths using the Project 2 path rules. SHA-256
identities are 64 lowercase hexadecimal digits. Byte counts are canonical unsigned
decimal values without leading zeros.

## Package 1 records

The first line is exactly:

```text
windvale-package 1
```

It is followed, in order, by exactly one `name`, `version`, `license`, `root`,
`target`, and `project` record; one or more `part` records; zero or more
`dependency` records; and zero or more `capability` records:

```text
name <package-id>
version <version>
license <license-id>
root <part-id>
target <target-id>
project <workspace-relative-project-path>
part <part-id> <portable|hosted|system> <workspace-relative-source-path>
dependency <importing-part-id> <required-part-id>
capability <capability-id>
```

The root names one declared part. Every dependency endpoint names one declared
part. Part identifiers and source paths are unique. Part records are ordered by
part identifier, dependency records by importing part then required part, and
capability records by capability identifier. The capability list is the complete
transitive requirement approved by the application root; it does not authorize or
bind a provider.

A Package 1 manifest contains at most 64 parts, 256 dependency edges, and 64
capabilities. A reader must reject a collection before appending an entry beyond
its limit and must not partially accept an over-limit manifest.

Package 1 currently selects one Project 2 source build and the target
`hosted-wvb-v1`. It does not select binary dependencies, optional capabilities,
resources, architecture-specific derivatives, feature variants, or build actions.

## Lock 1 records

The first line is exactly:

```text
windvale-lock 1
```

The implemented `local-source-1` lock contains, in order:

```text
resolver local-source-1
root <package-id> <version> <root-part-id>
target <target-id>
license <license-id>
origin workspace <sha256> <bytes> <workspace-path>
package <sha256> <bytes> <manifest-path>
compiler <sha256> <bytes> <compiler-wvb-path>
project <sha256> <bytes> <project-path>
part <part-id> <portable|hosted|system> <sha256> <bytes> <source-path>
dependency <importing-part-id> <required-part-id>
capability <capability-id>
output <sha256> <bytes>
```

Part, dependency, and capability records retain Package 1 canonical ordering.
Every Package 1 part, edge, and capability appears exactly once and matches the
manifest. The origin, manifest, compiler, project, and source part records pin the
complete build input by both byte count and SHA-256. `output` pins the sole
published WVB identity.

A Lock 1 file contains at most 64 parts, 256 dependency edges, and 64
capabilities. Part identifiers and source paths are unique, every dependency
endpoint names a locked part, and `output` is the final record. Readers reject a
collection before appending beyond its bound and reject any record after
`output`.

Resolution is complete before compilation. Once the checked-in resources exist,
the package build performs no network lookup and must not choose an alternate
source, compiler, version, target, or capability set.

## Implemented native front door

`Libraries/Package/Canonical-Package-Text.wv` and
`Libraries/Package/Package-Manifest.wv` provide the portable native-owned strict
text and general Package 1 manifest readers. `Libraries/Package/Package-Lock.wv`
provides the matching general `local-source-1` Lock 1 reader. The readers validate
fixed records, ordering, collection limits, identifiers, scopes, paths, digests,
canonical byte counts, unique ordered keys, root references, dependency
endpoints, and the final output record before returning valid views. Returned
string fields are bounded offset-and-length references into the caller's
immutable input; directories store only checked fixed-width numeric records.

`Libraries/Package/Package-Consistency.wv` composes both readers and the portable
SHA-256 implementation. It requires matching package name, version, root, target,
license, project, ordered parts, dependency edges, and capability closure, and it
requires the lock's manifest byte count and digest to identify the supplied
manifest exactly. It returns a bounded status and collection index without file
access.

`Libraries/Package/Package-Resource-Admission.wv` verifies one selected lock
resource at a time. Origin, manifest, compiler, project, and part resources must
match their exact locked path, byte count, and SHA-256; the output has no source
path but must match its locked byte count and SHA-256. A part index or nonzero
fixed-resource index outside the lock is rejected before path or content checks.
Host shells retain responsibility for acquiring the named bytes without path
aliasing and for withholding publication until every required resource passes.

The paired `Tools/Native/Build-Wvdb-Query-Package` and
`Build-Wvb-Inspector-Package` commands accept an explicit manifest, lock, and
output path. Each publication front door still intentionally admits only its
exact checked-in manifest and lock identity; replacing those specialized shells
with one general consistency/resource-admission consumer remains a subsequent
implementation slice.

The command verifies the manifest, lock, workspace, compiler WVB, Project 2 file,
and each source part before compiling to a private candidate. It then verifies the
candidate's exact locked size and digest and uses the pinned native publisher to
replace the requested output. Rejection must not create a new output or modify an
existing one.

Invalid invocation exits `64`. A lock or locked-resource mismatch exits `1` and
reports `package status=Lock_rejected`. Successful publication reports the package
name, target, byte count, and SHA-256 identity.

## Qualification boundary

`Test-Wvdb-Query-Package` performs two independent builds of each real package,
compares both output pairs, and inspects their distinct exact five-capability
closures. It also rejects a modified lock, a missing lock, and a copied manifest
at an alternate identity while proving failed admission preserves existing
output. The package-format owner admits both real manifest/lock pairs and every
locked input resource through the portable cores.

This qualifies deterministic local build, inspection, locked-input rejection, and
publication behavior. Decision 0561's separate Bundle 1 and WVDB Query owners now
add immutable content-addressed publication, fixed
`filesystem.directory_read_v1` binding, application execution, and explicit
missing, denied, and unavailable evidence. Exact paired-host promotion remains
the boundary for claiming the complete package-backed application milestone.
