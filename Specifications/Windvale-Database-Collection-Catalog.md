# Windvale database collection catalog

## Status and scope

`Windvaleˉdatabaseˉcollectionˉcatalog` is the portable, capability-free format
for values stored at `WVKR 1` collection keys. It defines `WVCL 1` collection
descriptors, typed read/put preparation, and admission that proves the key and
value name the same collection.

The module performs no I/O, grants no storage authority, and does not claim
atomic collection creation. The current tree writer provides upsert semantics;
proving that a definition was absent requires a later transactional condition.

## Descriptor format

Every descriptor begins with this exact 40-byte little-endian header:

| Offset | Width | Field | Requirement |
| ---: | ---: | --- | --- |
| 0 | 4 | Magic | `WVCL` (`0x4c435657` as a little-endian `u32`) |
| 4 | 4 | Version | `1` |
| 8 | 4 | Header bytes | `40` |
| 12 | 4 | Flags | zero |
| 16 | 8 | Collection identity | nonzero `u64` |
| 24 | 8 | Primary schema identity | nonzero `u64` |
| 32 | 4 | Name bytes | exact trailing byte count, 1 through 256 |
| 36 | 4 | Reserved | zero |

The trailing name is exact valid UTF-8. Its bytes, not host locale or Unicode
normalization, are the persisted spelling. The descriptor is therefore 41
through 296 bytes. Decoders reject invalid UTF-8, malformed fields, oversized,
truncated, or trailing input and return owned admitted bytes.

Collection identity is repeated in the value deliberately. Admission decodes
the `WVKR 1` collection key and `WVCL 1` value independently, then rejects an
identity mismatch. Primary schema identity supplies the default schema for
record writes; [`WVSC 1`](Windvale-Database-Typed-Rows-And-Schemas.md) defines
the collection-scoped schema body, while migration remains a separate contract.

## Typed operations

`Databaseˉcollectionˉreadˉprepare(Collection)` produces the canonical 32-byte
collection key or an empty typed failure. A ready engine read projection passes
the key unchanged to the durable tree lookup and admits a found value through
`Databaseˉcollectionˉadmit`.

`Databaseˉcollectionˉputˉprepare(Collection, Primary_schema, Name)` produces a
matching collection key and descriptor or no operation bytes. A ready engine
write projection may pass those bytes to durable tree upsert and must reopen
after commit. Equal arguments produce byte-identical output.

Put may replace an existing descriptor. It is not an atomic create, does not
check name uniqueness, and must not be retried after uncertain mutation without
a separately specified mutation-identity protocol.

## Failure vocabulary

Typed errors distinguish invalid total length, magic, version, header size,
flags, collection, primary schema, name length, UTF-8 encoding, reserved field,
truncation, trailing bytes, invalid collection key, and key/value identity
mismatch.

## Verification

The portable native fixture proves exact bytes, deterministic round trips,
read/put key agreement, key/value identity admission and mismatch rejection,
empty/oversized/invalid-UTF-8 names, maximum 296-byte descriptors, and corrupted,
truncated, or trailing input. It is an owned database-storage target on both
hosts.

## Exclusions

This contract does not itself define schema bodies; `WVSC 1` layers them on
schema keys. It does not define a name-to-identity index, atomic
create-if-absent, deletion, migration, indexes, queries, sessions, networking,
authentication, concurrent writers, reclamation, or database bootstrap.
