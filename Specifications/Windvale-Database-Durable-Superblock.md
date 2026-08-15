# Windvale Database durable superblock

## Status

This document specifies the implemented candidate `WVDS 1` dual-superblock
record. It is the first accepted durable byte contract for Windvale Database,
but it is not a complete database file format. The connected `WVPG 1`, `WVCR
1`, and publication planner are specified by [durable pages and commit
publication](Windvale-Database-Durable-Commit.md). Tree nodes, transactions, a
capability-bearing writer, crash injection, and the server protocol remain
separate contracts.

The portable implementation is
`Libraries/Database/Durable-Superblock.wv`. It has no capability and performs
no I/O. A storage owner supplies two exact candidate records and the observed
storage length; the implementation validates and selects a committed state.

## Storage region

The first 512 bytes of a database storage object are reserved for two
independent 256-byte superblock slots:

| Slot | Offset | Length |
| --- | ---: | ---: |
| first | 0 | 256 |
| second | 256 | 256 |

Database page zero begins at byte offset 512. A writer alternates slots and
never requires whole-file replacement to publish a generation.

All integers use little-endian encoding. Every `u64` field retains its full
width through source, canonical WVB 1.11, native ABI 22, and x64 lowering.

## Record layout

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVDS`, integer `0x53445657` |
| 4 | 4 | version | `1` |
| 8 | 4 | declared size | `256` |
| 12 | 4 | flags | zero |
| 16 | 8 | database identity high | combined identity must not be all zero |
| 24 | 8 | database identity low | combined identity must not be all zero |
| 32 | 8 | generation | positive; compared as unsigned `u64` |
| 40 | 8 | committed sequence | zero only for an initial state |
| 48 | 8 | root page | less than page count |
| 56 | 8 | commit-log head page | page identity or `u64::MAX` sentinel |
| 64 | 8 | earliest retained sequence | bounded by committed sequence |
| 72 | 8 | page count | positive |
| 80 | 8 | committed storage length | exact formula below |
| 88 | 4 | page size | 4,096, 8,192, 16,384, 32,768, or 65,536 |
| 92 | 4 | root depth | 1 through 128 |
| 96 | 128 | reserved | all zero |
| 224 | 32 | checksum | raw SHA-256 of bytes 0 through 223 |

The exact committed length is:

```text
512 + page_count * page_size
```

The multiplication and addition are checked in `u64`. Overflow is invalid.
The committed length is a recovery fence: bytes after it are an unpublished
tail and do not become reachable merely because the provider reports them.

For committed sequence zero, the commit-log head is the `u64::MAX` sentinel
and earliest retained sequence is zero. For a positive committed sequence,
the log head is a page identity below page count and earliest retained
sequence is in the inclusive range `1..committed_sequence`.

## Validation

Decode requires exactly 256 bytes. Validation proceeds through the observable
failure classes in this order:

1. exact length;
2. magic, version, declared size, flags, and reserved bytes;
3. raw SHA-256 checksum;
4. database identity and positive generation;
5. page size, positive page count, root page, and root depth;
6. commit-log and retained-sequence relationships; and
7. checked committed-length arithmetic and equality.

The portable API returns a closed `Databaseˉsuperblockˉerror` value. Expected
malformed input is not an exception and never yields partially trusted fields.

## Recovery selection

Selection decodes each slot independently and then rejects an otherwise valid
slot when its committed length exceeds the provider's observed storage length.
Recovery follows these rules:

- no valid slot returns `Noˉvalidˉsuperblock` with both slot errors;
- exactly one valid slot selects it;
- two valid slots with different database identities return
  `Conflictingˉdatabase`;
- two valid slots with different generations select the greater generation;
- two valid slots with the same generation must contain identical 256-byte
  records, otherwise selection returns `Conflictingˉgeneration`; and
- identical same-generation records select the first slot deterministically.

The selection reports the exact unpublished tail as
`observed_storage_length - committed_length`. It does not inspect, truncate,
or authorize that tail.

## Publication protocol

The codec does not perform I/O. The implemented capability-free publication
planner validates a target superblock and compact commit record, then requires
the random-access storage contract in this order:

1. write all new immutable pages and commit-log records;
2. flush their content and the extended object length;
3. encode generation `N + 1` into the inactive superblock slot;
4. write that complete slot without modifying the active slot;
5. flush the modified superblock content; and
6. only after successful flush, report the generation as committed.

A rejected write is not committed. An exact partial write or indeterminate
mutation forces reopen and selection; it must not be blindly replayed. A torn
record fails its checksum. Storage providers must not claim this protocol
unless their flush behavior preserves the stated ordering. The exact planner
states and progress rules are defined in
[the durable commit contract](Windvale-Database-Durable-Commit.md).

## Limits and exclusions

- The database identity is opaque; this contract does not define its generator.
- SHA-256 provides corruption detection here, not authentication.
- Page and commit-log checksums use their separately versioned `WVPG 1` and
  `WVCR 1` contracts.
- This version has no compatibility rule for unknown flags or nonzero reserved
  bytes; they fail closed.
- Multi-writer fencing, replication, backup replacement, directory durability,
  encryption, key rotation, and remote storage are not implied.

## Verification

`Projects/Tests/Windvale-Native-Test-Database-Durable-Superblock.wvproj`
builds one hosted selector with 13 bounded cases. The focused
`database-superblock` verification owner proves deterministic WVB and WVO bytes,
independent WVO verification, exact flat-image and hosted-application
identities, local execution of all cases, and construction of the other host's
application. Independent Linux execution remains part of the dual-host GitHub
qualification boundary.
