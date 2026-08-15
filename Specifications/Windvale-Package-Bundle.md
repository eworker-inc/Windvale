# Windvale package bundle version 1

## Status

Bundle 1 is the implemented deterministic in-memory package transport contract.
`Package-Bundle-Writer.wv` constructs canonical bytes and the separately owned
`Package-Bundle-Verifier.wv` admits untrusted bytes before publication or
execution. The initial implementation policy accepts at most 4 MiB in one
`bytes` value; the format limit remains 2,147,483,648 bytes and requires the
separate streaming admission slice before objects above the in-memory policy
can be accepted.

The focused owner now rebuilds and independently admits the exact WVDB Query
and WVB Inspector bundles twice, then publishes both into one immutable store.
Their canonical LF license is one shared object, so the two bundles occupy nine
unique objects and repeat publication creates nothing. Bundle 1 carries selected immutable bytes. It does not resolve dependencies,
execute installation scripts, grant capabilities, select host paths, or confer
official-release trust.

## Header

All integers are unsigned little-endian. The header is exactly 128 bytes.

| Offset | Bytes | Field | Required value |
| ---: | ---: | --- | --- |
| 0 | 4 | Magic | ASCII `WVPB` |
| 4 | 2 | Major | `1` |
| 6 | 2 | Minor | `0` |
| 8 | 4 | Header bytes | `128` |
| 12 | 4 | Flags | `0` |
| 16 | 8 | Total bytes | Exact file length |
| 24 | 8 | Index offset | `128` |
| 32 | 8 | Index bytes | `1..1,048,576` |
| 40 | 8 | Content offset | `128 + index_bytes` |
| 48 | 8 | Content bytes | Exact remaining length |
| 56 | 4 | Blob count | `1..4,096` |
| 60 | 4 | Item count | `1..4,096` |
| 64 | 32 | Index SHA-256 | Raw digest bytes |
| 96 | 32 | Reserved | All zero |

There is no padding between the header, index, and content. Every sum and range
is checked before use. Total bundle length cannot exceed 2,147,483,648 bytes.

## Canonical index

The index is strict UTF-8 without a byte-order mark. It uses LF, ends in exactly
one LF, has no empty lines, and contains ASCII-space-separated records with no
leading, trailing, or repeated spaces.

```text
windvale-bundle-index 1
package <package-id> <version> <target-id>
manifest <sha256> <bytes>
lock <sha256> <bytes>
item <role> <item-id> <target-id> <sha256>
blob <sha256> <bytes> <content-offset>
```

The header, package, manifest, and lock records occur exactly once in that
order. Exactly `item_count` item records follow, then exactly `blob_count` blob
records. Item records are ordered by role, item identifier, target, and digest.
Blob records are ordered by digest. Duplicates are invalid.

Bundle 1 roles are `executable`, `license`, and `provenance`. The current writer
emits one of each. Manifest and lock digests identify exact blob records. Every
item digest identifies one blob, and every blob is referenced by the manifest,
lock, or an item.

Blob geometry is complete and contiguous: the first offset is zero, each later
offset equals the preceding offset plus length, and the final end equals the
declared content length. Zero-length blobs are permitted by the format only
when their owning semantic record permits them; the current writer requires
non-empty bounded license and provenance values.

## Admission

An admitting reader validates, in order:

1. size, header, reserved bytes, counts, and checked geometry;
2. the exact SHA-256 of the canonical index;
3. index text, record count, ordering, and duplicates;
4. complete contiguous blob geometry and every blob SHA-256;
5. complete reference reachability;
6. Package 1 and Lock 1 syntax, identity, and cross-file agreement;
7. package, version, and target agreement with the index;
8. item roles and target agreement; and
9. target-specific executable admission and the locked output identity.

The verifier never extracts to a native path. Rejection publishes nothing and
does not replace an existing destination. The entire admitted bundle SHA-256 is
reported as its immutable bundle identity.

The writer and verifier use distinct implementations. Conformance covers
determinism, valid boundaries, truncation, collection limits, overlap, gaps,
duplicates, ordering, corrupt index and blob bytes, wrong targets, and hostile
executables. Windows and Linux must report byte-identical successful bundles
and matching rejection statuses before cross-host qualification is claimed.
