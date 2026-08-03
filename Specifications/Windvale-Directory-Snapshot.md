# Windvale immutable directory snapshot

## Status and purpose

`WVDS 1` is the implemented one-page immutable directory snapshot owned by [Decision 0155](../Documents/Decisions/0155-First-Immutable-Windvale-Directory-Snapshot.md) and adopted by the Probe-35 guest under [Decision 0159](../Documents/Decisions/0159-First-Guest-Directory-Service.md). It gives a Windvale directory service a deterministic, measured source for the existing [`filesystem.directory_read_v1`](Read-Only-Directory-Capability.md) operation and [`WVDQ 1` / `WVDR 1`](Windvale-Directory-Service-Ipc.md) exchange.

The snapshot is a service-private data format. It is not a package-resource store, filesystem image, path namespace, kernel message, open-file table, or application-visible handle. A loader verifies the complete value before publishing an immutable mapping. Consumers must treat every byte as untrusted even when the page came from a measured boot input.

## Complete extent and byte order

Every integer is unsigned little-endian. The complete canonical value is 68 through 4,096 bytes and contains one through 64 entries. Version 1 requires at least one entry and every name contains at least one byte, making 68 bytes the smallest possible aligned value. The exact complete extent is stored in the header; no trailing bytes are allowed.

The 32-byte header is:

| Offset | Bytes | Meaning |
| ---: | ---: | --- |
| `0` | 4 | Magic `WVDS` |
| `4` | 4 | Version `1` |
| `8` | 4 | Exact complete snapshot bytes |
| `12` | 4 | Entry count, `1…64` |
| `16` | 4 | Entry region offset, exactly `32` |
| `20` | 4 | Bytes per entry, exactly `32` |
| `24` | 4 | Name region offset, exactly `32 + entry_count * 32` |
| `28` | 4 | Four-byte-aligned data region offset |

The entry table is followed by one canonical name region, zero through three alignment bytes, and one canonical data region. The data region ends exactly at the complete snapshot extent.

## Entry record

Each 32-byte entry is:

| Relative offset | Bytes | Meaning |
| ---: | ---: | --- |
| `0` | 4 | Kind: `1` file, `2` other |
| `4` | 4 | Absolute name offset |
| `8` | 4 | Name bytes |
| `12` | 4 | Absolute file-data offset, or zero for `other` |
| `16` | 4 | File-data bytes, or zero for `other` |
| `20` | 12 | Reserved zero |

Entries are strictly ordered by their encoded name using unsigned ordinal byte comparison. Duplicate names are invalid. Each name is a one-through-255-byte ASCII segment under the read-only directory contract: letters, digits, `.`, `_`, and `-` are admitted, while empty, dot, dot-dot, separators, colon, NUL, non-ASCII, and every other byte are rejected.

Name extents are packed without gaps in entry order, beginning exactly at the declared name region. The only bytes between the final name and the data region are the zero bytes required for four-byte alignment.

File extents are packed without gaps in entry order, beginning exactly at the data region. Empty files are valid. An `other` entry has zero data offset and length and does not consume data-region bytes. The final file extent ends exactly at the complete snapshot extent. Overlap, aliasing, gaps, nonzero padding, and trailing data are invalid.

## Verification and lookup

A consumer verifies the complete structure before lookup and revalidates all bounds with checked arithmetic. Identity, version, exact extent, count, fixed region locations, entry kinds, reserved bytes, name grammar and order, alignment padding, and exact name/data coverage are mandatory. A structural failure invalidates the whole snapshot; partial lookup results must not escape.

After verification, ordinal lookup has these results:

- an absent name produces `WVDR Not_found`;
- an `other` entry produces `WVDR Not_file`;
- a file offset greater than its length produces `WVDR Invalid_offset` with the exact file length;
- an offset at the file length is a successful zero-byte read;
- otherwise the result is the exact `min(maximum, file_length - offset)` bytes.

The `WVDQ 1` service boundary limits `maximum` to 3,072 bytes and validates the resulting exact `WVDR 1` envelope before publication. A malformed snapshot produces no reply; it is a service integrity failure, not a filesystem status.

## Deterministic construction

A canonical writer sorts entries by ordinal ASCII name, packs exact name and file extents, emits only required zero alignment, writes every reserved byte as zero, and verifies its own result. Identical logical entries therefore produce identical bytes regardless of input order.

The Probe-35 guest fixture contains `folder` as kind `other` and `kernel.wv` as a 3,072-byte file whose byte at index `i` is `i mod 251`. Its exact layout is:

- 32-byte header;
- two 32-byte entries;
- names at offsets 96 through 110;
- one zero alignment byte at offset 111;
- file data at offsets 112 through 3,183;
- exact total 3,184 bytes;
- SHA-256 `0f793a41a701240b9cf41179dafa252384b43cd23214646ff021d245657c235a`.

This fixture is a reproducibility and boundary-pressure value, not a general root filesystem. Probe 35 maps it RO/NX only into init, binds its complete identity as attached resource 5, and proves that two independently rebuilt clients receive and validate its complete file over the format-blind service channel without mapping the snapshot itself.

## Ownership and current limits

Portable [`Directory-Snapshot.wv`](../Operating-System/Services/Directory-Snapshot.wv) owns the Windvale verifier, lookup, and `WVDR 1` construction. [`Directory-Snapshot-Service.wv`](../Operating-System/Services/Directory-Snapshot-Service.wv) composes that storage policy with the independently qualified directory protocol. The hosted bridge supplies only opaque snapshot and request bytes for differential testing. [`Directory-Snapshot.cs`](../Operating-System/Windvale.Bootstrap/Directory-Snapshot.cs) is the independent Stage 0 writer, verifier, and provider oracle.

Version 1 has no nested paths, enumeration, timestamps, permissions, ownership, links, mutation, persistence, compression, sparse extents, block allocation, caching, signatures, open handles, or concurrent update protocol. Those concerns require separate contracts rather than unused fields in this page.
