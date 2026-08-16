# Windvale database secondary indexes

## Status and scope

`Windvaleˉdatabaseˉsecondaryˉindex` is the first portable secondary-index
contract. It defines bounded ordered-index metadata and deterministically derives
tree keys from an admitted `WVSC 1` schema, `WVTR 1` row, and record identity. It
does not perform storage I/O, publish mutations, or grant authority.

This slice carries forward the useful EWDB mechanisms: compound ordered keys,
explicit ascending or descending fields, explicit null placement, row-identity
suffixes for non-unique entries, and owner values for unique-entry validation.
It replaces EWDB's JSON descriptors, host dictionaries, and separate index files
with typed Windvale binary formats in the same durable tree.

## Limits and supported values

One index has one through eight distinct schema-field ordinals and a UTF-8 name
of one through 128 bytes. Boolean, I64, U64, and text values are indexable. Raw
bytes are rejected because this first query/order contract has no declared byte
collation. A definition may be unique or non-unique.

Each field declares ascending or descending order and one null policy:

- `Exclude` omits a row when that component is null;
- `First` encodes null before concrete values; or
- `Last` encodes null after concrete values.

All work is bounded by the schema's 64-field limit, the index's 8-field limit,
the 4,064-byte record-identity limit, and the durable tree's 4,096-byte key limit.
An otherwise valid value whose escaped compound key exceeds 4,096 bytes is
rejected rather than truncated.

## `WVSI 1` definition

Every definition begins with this exact 64-byte little-endian header:

| Offset | Width | Field | Requirement |
| ---: | ---: | --- | --- |
| 0 | 4 | Magic | `WVSI` (`0x49535657`) |
| 4 | 4 | Version | `1` |
| 8 | 4 | Header bytes | `64` |
| 12 | 4 | Flags | zero, or bit zero alone for unique |
| 16 | 8 | Collection identity | nonzero |
| 24 | 8 | Schema identity | nonzero |
| 32 | 8 | Index identity | nonzero |
| 40 | 4 | Field count | 1 through 8 |
| 44 | 4 | Field bytes | exactly field count times 12 |
| 48 | 4 | Name bytes | 1 through 128 |
| 52 | 4 | Reserved | zero |
| 56 | 8 | Reserved | zero |

Each 12-byte field descriptor contains a zero-based schema-field ordinal, a
direction (`1` ascending or `2` descending), and a null policy (`1` exclude,
`2` first, or `3` last). Descriptors follow the header in compound-key order,
then the exact UTF-8 name. Field ordinals must be unique. Decoding rejects all
truncation, extension, unknown flag, reserved, count, direction, and null-policy
states.

The catalog key is exactly 24 bytes: `WVIK`, version `1`, the collection identity
in big-endian order, and the index identity in big-endian order. The definition
is its tree value. Big-endian identities keep catalog keys in numeric order.

## `WVIX 1` entry key

An entry key begins with `WVIX`, version `1`, and the big-endian collection and
index identities. Ordered components follow without host-dependent collation:

- null-first is marker `0x10` and null-last is marker `0xF0`;
- every concrete component begins with marker `0x40`;
- Boolean is its canonical zero or one byte;
- U64 reverses canonical little-endian data into big-endian order;
- I64 does the same and flips the high sign bit, producing signed numeric order;
- text preserves exact UTF-8 bytes, escapes zero as `00 FF`, and ends in `00 00`;
- descending order inverts every encoded concrete payload byte while leaving its
  `0x40` marker unchanged.

A non-unique key then appends marker `0x80` plus the zero-escaped, `00 00`
terminated record identity. This makes equal indexed values stable and unique per
record. A unique key omits that suffix, so all owners of the same indexed value
address the same tree key.

The `WVIV 1` entry value has a 24-byte little-endian header containing magic,
version `1`, header size `24`, zero flags, identity byte count, and zero reserved
field, followed by the exact record identity. Both unique and non-unique entries
retain this owner value.

## Atomicity and uniqueness boundary

Key construction is not publication. The next mutation-composition boundary must
delete stale index entries, put new index entries, and mutate the primary row in
one canonical `WVTM 1` transaction. With at most eight indexes, the worst update
is 17 mutations, below `WVTM 1`'s limit of 32.

Before a unique-entry put is published, the writer must read the candidate key.
No existing value is acceptable; the same owner identity is an idempotent update;
a different owner is a unique conflict. A caller must never use an ordinary put
to overwrite another owner. This contract exposes the unique shape but does not
claim that the ownership probe or atomic composer is implemented yet.

## Verification

The focused portable fixture proves deterministic definition round trips,
compound ascending I64 and descending text bytes, big-endian catalog ordering,
non-unique identity suffixes, equal unique keys with distinct owner values, null
exclusion and placement, the eight-field bound, duplicate fields, unsupported
byte fields, the exact key-size rejection, and malformed definition boundaries.

Range-scan execution, index-set discovery, primary-plus-index mutation
composition, unique ownership probes, query planning, online build, rebuild,
schema migration, full-text search, JSON paths, locale collation, and vector
search remain separate milestones.
