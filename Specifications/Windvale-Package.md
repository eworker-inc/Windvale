# Windvale Package 1 and Lock 1

## Status and purpose

Package 1 (`.wvpack`) and Lock 1 (`.wvlock`) define the first native-owned local
source-package contract. The implemented instance packages the WVDB Query
application and its reusable portable and hosted libraries. It builds the same
canonical WVB from exact checked-in inputs on Windows and Linux without .NET.

This contract is separate from Workspace 1 and Project 2. A project selects source
inputs for one compilation. A package names parts, dependency edges, target scope,
license, and required capabilities. A lock records one fully selected source graph
and every byte identity used to produce the output.

No package bundle, content store, registry, signature envelope, installation
generation, runtime WVB linker, or updater is implemented by this version.

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

Resolution is complete before compilation. Once the checked-in resources exist,
the package build performs no network lookup and must not choose an alternate
source, compiler, version, target, or capability set.

## Implemented native front door

`Libraries/Package/Canonical-Package-Text.wv` and
`Libraries/Package/Package-Manifest.wv` provide the portable native-owned strict
text and general Package 1 manifest readers. The manifest reader validates the
fixed records, ordering, collection limits, identifiers, scopes, paths, unique
ordered keys, root reference, and dependency endpoints before returning a valid
view. Returned string fields are bounded offset-and-length references into the
caller's immutable input; the directory stores only checked fixed-width numeric
records.

The paired `Tools/Native/Build-Wvdb-Query-Package` commands accept an explicit
manifest, lock, and output path. That publication front door still intentionally
admits only the exact checked-in WVDB Query manifest and lock identity; replacing
its specialized admission path with the general readers and adding a general
Lock 1 reader are subsequent implementation slices.

The command verifies the manifest, lock, workspace, compiler WVB, Project 2 file,
and each source part before compiling to a private candidate. It then verifies the
candidate's exact locked size and digest and uses the pinned native publisher to
replace the requested output. Rejection must not create a new output or modify an
existing one.

Invalid invocation exits `64`. A lock or locked-resource mismatch exits `1` and
reports `package status=Lock_rejected`. Successful publication reports the package
name, target, byte count, and SHA-256 identity.

## Qualification boundary

`Test-Wvdb-Query-Package` performs two independent builds, compares their bytes,
inspects the exact five-capability closure, rejects a modified lock, rejects a
missing lock, rejects a copied manifest at an alternate identity, and proves that
failed admission preserves an existing output.

This qualifies deterministic local build, inspection, locked-input rejection, and
publication behavior. It does not qualify runtime provider binding or application
execution. The current native runner cannot yet bind
`filesystem.directory_read_v1`; cross-host execution and capability-denial
evidence remain the next package-application slice.
