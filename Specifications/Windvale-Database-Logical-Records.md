# Windvale database logical records

## Status and scope

`Windvaleˉdatabaseˉlogicalˉrecord` is the portable, capability-free boundary
between application record identities and `WVTN 1` tree bytes. It defines one
canonical collection/record key format, one typed record envelope, and bounded
get/put preparation. It performs no storage I/O and grants no authority.

The current format version is `1`. Every multi-byte scalar is little-endian.
Decoders require exact length and reject malformed, oversized, truncated, or
trailing input before returning any admitted bytes.

## Logical key format

Every key begins with this exact 32-byte header:

| Offset | Width | Field | Requirement |
| ---: | ---: | --- | --- |
| 0 | 4 | Magic | `WVKR` (`0x524b5657` as a little-endian `u32`) |
| 4 | 4 | Version | `1` |
| 8 | 4 | Header bytes | `32` |
| 12 | 4 | Kind | collection `1` or record `2` |
| 16 | 8 | Collection identity | nonzero `u64` |
| 24 | 4 | Identity bytes | exact trailing byte count |
| 28 | 4 | Reserved | zero |

A collection key has kind `1`, zero identity bytes, and an exact total length
of 32 bytes. A record key has kind `2`, one through 4,064 opaque identity
bytes, and an exact total length of 33 through 4,096 bytes. The limits match
the implemented `WVTN 1` maximum key size.

The common header and collection field make record keys for one collection a
contiguous bytewise prefix group. The `u64` collection field is intentionally
little-endian like the rest of the durable stack; this version does not promise
numeric collection ordering across different collection identities. Identity
bytes are opaque and have no text normalization or host-locale behavior.

Collection keys reserve a deterministic metadata anchor without defining the
metadata value. Catalog naming, schema definitions, and migration policy are a
later contract.

## Record envelope

Every record value begins with this exact 32-byte header:

| Offset | Width | Field | Requirement |
| ---: | ---: | --- | --- |
| 0 | 4 | Magic | `WVRD` (`0x44525657` as a little-endian `u32`) |
| 4 | 4 | Version | `1` |
| 8 | 4 | Header bytes | `32` |
| 12 | 4 | Flags | zero |
| 16 | 8 | Schema identity | nonzero `u64` |
| 24 | 4 | Payload bytes | exact trailing byte count |
| 28 | 4 | Reserved | zero |

The payload is zero through 61,408 opaque bytes. The complete value is at most
61,440 bytes, matching the implemented `WVTN 1` value bound. Schema identity
selects an external logical schema contract; it is not executable code and the
codec does not validate application fields.

## Typed operations

`Databaseˉlogicalˉgetˉprepare(Collection, Identity)` returns either a typed
error and no key, or one canonical record key. A ready engine read projection
passes that key unchanged to `Databaseˉdurableˉtreeˉlookup`, then admits a
found value only through `Databaseˉlogicalˉrecordˉdecode`.

`Databaseˉlogicalˉputˉprepare(Collection, Identity, Schema, Payload)` returns
either a typed error and no key/value bytes, or one canonical key and record
envelope. A ready engine write projection passes those bytes unchanged to
`Databaseˉdurableˉtreeˉupsert`, then reopens the engine after a committed
mutation as required by the lifecycle contract.

Preparation is deterministic: equal arguments produce byte-identical output.
It does not make an operation durable, retry an uncertain mutation, allocate a
collection, authorize a schema, or establish transaction/session identity.

## Failure vocabulary

Typed errors distinguish invalid total length, magic, version, header size,
kind, collection, identity length, reserved fields, flags, schema, payload
length, truncation, and trailing bytes. Failure results expose empty operation
bytes, so a caller cannot accidentally submit a partially prepared put.

## Verification

The portable native fixture proves exact headers, deterministic encode/decode,
collection anchors, get/put key agreement, empty payload admission, maximum
4,096-byte keys, maximum 61,440-byte values, and typed malformed-input
rejections. It is an owned target of both database-storage modes on Windows
and Linux.

## Exclusions

This contract does not define collection names or descriptors, schema bodies,
indexes, queries, deletes, compare-and-swap, multi-record transactions,
sessions, networking, authentication, concurrent readers, writer arbitration,
reclamation, or migration between format versions.
