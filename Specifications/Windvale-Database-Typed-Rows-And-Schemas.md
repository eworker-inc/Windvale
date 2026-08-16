# Windvale database typed rows and schemas

## Status and scope

`Windvaleˉdatabaseˉschemaˉdefinition` and
`Windvaleˉdatabaseˉtypedˉrow` are portable, capability-free format and
validation modules. They replace opaque application payload assumptions with
collection-scoped schema definitions and exact typed row admission. They do
not perform storage I/O or grant authority.

This contract defines `WVSC 1` schemas and `WVTR 1` rows. All multi-byte
scalars are little-endian. Encoders are deterministic and decoders require an
exact bounded input with no trailing bytes.

## Schema key

A schema is stored at a `WVKR 1` key with kind `3`, its nonzero collection
identity, and exactly eight identity bytes containing the nonzero schema
identity. This separates schema metadata from both the collection anchor and
application record identities.

## Field descriptor

Each schema field is encoded in declaration order:

| Relative offset | Width | Field | Requirement |
| ---: | ---: | --- | --- |
| 0 | 4 | Kind | Boolean `1`, I64 `2`, U64 `3`, text `4`, or bytes `5` |
| 4 | 4 | Flags | zero, or bit zero alone for nullable |
| 8 | 4 | Maximum value bytes | `1` for Boolean, `8` for I64/U64, otherwise 1 through 61,408 |
| 12 | 4 | Name bytes | exact trailing count, 1 through 128 |
| 16 | variable | Name | exact valid UTF-8 |

Field names are bytewise unique within one schema. Their exact UTF-8 bytes are
the durable spelling; no host locale or Unicode normalization is inherited.
Schemas contain 1 through 64 fields. This makes validation work and memory
bounded even for malicious input.

## `WVSC 1` schema definition

Every schema begins with this exact 48-byte header:

| Offset | Width | Field | Requirement |
| ---: | ---: | --- | --- |
| 0 | 4 | Magic | `WVSC` (`0x43535657`) |
| 4 | 4 | Version | `1` |
| 8 | 4 | Header bytes | `48` |
| 12 | 4 | Flags | zero |
| 16 | 8 | Collection identity | nonzero `u64` |
| 24 | 8 | Schema identity | nonzero `u64` |
| 32 | 4 | Field count | 1 through 64 |
| 36 | 4 | Field bytes | exact trailing byte count, at most 9,216 |
| 40 | 8 | Reserved | zero |

The ordered field descriptors follow immediately. The complete schema is at
most 9,264 bytes. Decoding independently recounts fields, validates every
descriptor, rejects duplicate names, and requires the declared count and byte
length to match.

`Databaseˉschemaˉreadˉprepare` and `Databaseˉschemaˉputˉprepare` produce the
canonical tree key and value. `Databaseˉschemaˉadmit` decodes both independently
and rejects collection or schema identity mismatch.

## Typed value

Each row value uses this exact 16-byte header followed by its data:

| Relative offset | Width | Field | Requirement |
| ---: | ---: | --- | --- |
| 0 | 4 | Kind | exact declared field kind |
| 4 | 4 | Flags | zero, or bit zero alone for null |
| 8 | 4 | Data bytes | exact trailing count |
| 12 | 4 | Reserved | zero |

A null value has no data and is accepted only for a nullable field. Non-null
Boolean data is exactly one byte, zero or one. I64 and U64 data is exactly
eight bytes. Text is exact valid UTF-8. Text and bytes must not exceed the
field maximum. I64 bytes preserve the signed two's-complement bit pattern;
U64 bytes preserve the unsigned little-endian value.

## `WVTR 1` typed row

Every row begins with this exact 32-byte header:

| Offset | Width | Field | Requirement |
| ---: | ---: | --- | --- |
| 0 | 4 | Magic | `WVTR` (`0x52545657`) |
| 4 | 4 | Version | `1` |
| 8 | 4 | Header bytes | `32` |
| 12 | 4 | Flags | zero |
| 16 | 4 | Field count | exactly the schema field count |
| 20 | 4 | Value bytes | exact trailing byte count, at most 61,376 |
| 24 | 8 | Reserved | zero |

Values follow in schema declaration order. Row validation scans schema and
values together once without copying field names or descriptors, rejects
missing or extra values, and returns no admitted bytes after any mismatch. The
complete row is at most 61,408 bytes so it fits exactly inside the implemented
`WVRD 1` payload bound.

`Databaseˉtypedˉputˉprepare` validates a collection-scoped schema and row before
wrapping the row in `WVRD 1`. `Databaseˉtypedˉadmit` independently validates
the logical envelope, proves its schema identity matches `WVSC 1`, and then
validates the complete row. Each operation admits its schema once rather than
decoding and copying it again during row validation. Equal inputs produce
byte-identical output.

## Verification

The focused portable fixture proves exact schema, field, value, row, schema
key, and logical record bytes; deterministic round trips; 64-field and
61,408-byte row boundaries; nullable and required fields; schema/key identity
admission; and malformed magic, version, sizes, flags, counts, reserved bytes,
UTF-8, Boolean, kind, nullability, truncation, trailing bytes, duplicates, and
oversized values.

## Exclusions

This version does not define defaults, computed fields, decimal or floating
point values, nested rows, arrays, schema migration, compatibility coercions,
name indexes, query execution, deletion, transactions, concurrency, networking,
or authorization. Ordered secondary-index definitions and keys are now a
separate bounded contract; index maintenance and execution remain later work.
Those layers must lower to this typed value model rather than bypass it.
