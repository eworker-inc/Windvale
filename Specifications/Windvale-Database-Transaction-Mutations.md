# Windvale database transaction mutations

## Status

- Version: `WVTM 1`
- Profile: portable
- Implementation: `Libraries/Database/Transaction-Mutations.wv`
- Maximum mutations: 32
- Maximum encoded bytes: 262,144
- Evidence: focused Windows native execution; independent Linux execution pending

## Purpose

`WVTM 1` is the canonical input to Windvale Database's multi-record tree
planner. It describes one bounded, deterministic set of key mutations before
any storage page is read or written.

This format does not commit data by itself. The next layer must read all
affected paths, prepare one shared copy-on-write tree replacement, and publish
all resulting pages through one `Databaseˉcommitˉbatchˉbegin` operation. A
successful `WVTM 1` decode therefore proves request framing and ordering, not
transaction durability.

## Encoding

All integers are unsigned little-endian `u32` values. The 24-byte header is:

| Offset | Field | Required value |
| ---: | --- | --- |
| 0 | magic | `WVTM` (`1297372759`) |
| 4 | version | `1` |
| 8 | header bytes | `24` |
| 12 | flags | `0` |
| 16 | mutation count | `1..32` |
| 20 | total bytes | exact encoded length, at most `262144` |

Each mutation immediately follows the previous mutation and has a 16-byte
header:

| Offset | Field | Rule |
| ---: | --- | --- |
| 0 | kind | `1` put or `2` delete |
| 4 | flags | `0` |
| 8 | key bytes | `1..4096` |
| 12 | value bytes | `0..61440`; must be zero for delete |

The key bytes and value bytes follow the mutation header without padding. A
put may contain an empty value. A delete contains no value bytes.

## Canonical order

Mutations are strictly increasing by unsigned bytewise key order. A shorter
key sorts first when it is an exact prefix of a longer key. Duplicate keys and
out-of-order keys are rejected.

This rule gives one deterministic meaning to a transaction and removes
ambiguous sequences such as putting and deleting the same key in one request.
Callers that accept operations in another order must normalize them before
encoding `WVTM 1` and must reject duplicate final keys rather than silently
choosing a winner.

## Validation and ownership

Decoding checks the size ceiling before reading fields, uses subtraction-based
bounds checks for every variable-length mutation, rejects reserved values,
requires the declared count to consume the complete input, and copies the
accepted encoding into owned bytes. Indexed reads validate the complete set
before returning a copied key and value.

The fixed operation and byte limits bound validation work and retained request
memory. Validation is linear in encoded bytes. Indexed reads are linear in the
requested index; the limit of 32 keeps that simple reference path bounded. The
future tree planner should scan once rather than repeatedly call indexed read.

## Relationship to EWDB

The useful EWDB transaction ideas retained here are:

- validate the complete request before mutation;
- impose explicit operation and request-size limits;
- establish one canonical operation order; and
- prepare copy-on-write changes before durable publication.

Windvale does not copy EWDB's JSON evidence files, table locks, .NET object
graphs, or 1,000-operation default. `WVTM 1` is a smaller binary contract sized
for the current 63-page commit batch and native memory goals.

## Verification

The portable self-test covers deterministic equal-input bytes, exact framing,
put/delete decoding, empty values, invalid index, malformed magic/version/
header/flags/count/length, invalid kinds and mutation flags, zero and oversized
keys, oversized values, delete values, truncation, duplicates, reversed order,
trailing mutations, and invalid component construction.

## Exclusions and next step

`WVTM 1` does not provide transaction IDs, replay protection, expected-version
checks, isolation, concurrency, page planning, publication, indexes, schemas,
collections, JSON bodies, or SQL mutation syntax.

The immediate successor consumes one valid mutation set and one stable
committed root, rewrites shared ancestors only once, rejects plans that exceed
the 63-page commit ceiling, and publishes exactly one new generation.
