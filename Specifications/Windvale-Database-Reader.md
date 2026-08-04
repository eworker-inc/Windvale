# Experimental Windvale Database reader

## Status and scope

This document specifies the implemented `WVDB 1` read-only experiment. It is a
bounded format-reader and exact-lookup exercise, not an accepted durable
database format. The magic, version, limits, checksum, key/value types, module
API, and file extension may all be replaced without a compatibility reader
while Windvale retains its current early-development format policy.

The portable implementation is
[`Libraries/Database/Wvdb-Reader.wv`](../Libraries/Database/Wvdb-Reader.wv).
It accepts immutable `bytes`; it does not open a path, own a file, mutate
storage, start a service, or receive ambient authority. Hosted application and
future Windvale OS service adapters remain separate capability-bearing layers.

## Purpose

The experiment proves that current Windvale can:

- validate a bounded page envelope as untrusted input;
- return nominal recoverable validation and lookup results;
- validate ordered branch and leaf entries and exact page padding;
- traverse a bounded B+tree search path; and
- return an exact `u32` key to `i32` value lookup without a .NET database
  engine behind the Windvale API.

It deliberately does not specify mutation, a write-ahead log, recovery,
transactions, snapshots, concurrency, caching, SQL, graph operations, a wire
protocol, or PostgreSQL or EWDB compatibility.

## Bounds and scalar rules

All integers are little-endian. Checked Windvale arithmetic applies while
reading the format.

| Property | `WVDB 1` experimental value |
| --- | ---: |
| Database header | 32 bytes |
| Page size | 256 bytes |
| Page header | 32 bytes |
| Entry size | 8 bytes |
| Maximum pages | 64 |
| Maximum entries per page | 28 |
| Maximum declared lookup depth | 16 |
| Maximum complete input | 16,416 bytes |
| Key | `u32` |
| Leaf value | `i32` |

These small values make malformed-input and complete-boundary execution cheap.
They are not performance recommendations for a later storage engine. The
durable direction uses `u64` byte offsets, page identities, generations, and
log positions under
[Decision 0207](../Documents/Decisions/0207-U64-Binary-Fields-For-Durable-Storage.md);
this experimental format is not widened in place.

## Database header

The input length must equal `32 + page_count * 256` exactly.

| Offset | Width | Field | Required value |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVDB` (`0x42445657` little-endian) |
| 4 | 4 | format version | `1` |
| 8 | 4 | total length | exact complete input length |
| 12 | 4 | page size | `256` |
| 16 | 4 | page count | `1..64` |
| 20 | 4 | root page identifier | less than page count |
| 24 | 4 | lookup depth limit | `1..16` |
| 28 | 4 | reserved | zero |

Page identifier `n` begins at `32 + n * 256`.

## Page envelope

Every declared page is validated before lookup, including pages outside the
selected search path.

| Offset in page | Width | Field | Required value |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVPG` (`0x47505657` little-endian) |
| 4 | 4 | page version | `1` |
| 8 | 4 | page identifier | physical page identifier |
| 12 | 4 | page kind | `1` leaf or `2` branch |
| 16 | 4 | entry count | `0..28`; a branch requires at least one |
| 20 | 4 | reserved | zero |
| 24 | 4 | checksum | bounded additive checksum below |
| 28 | 4 | reserved | zero |
| 32 | variable | entries | `entry_count * 8` bytes |
| after entries | variable | padding | zero through byte 255 |

The experimental checksum is the `u32` sum of every byte in the 256-byte page
except bytes 24 through 27. Its maximum possible value is 64,260, so it does
not depend on wrapping arithmetic. This checksum detects the corruption cases
needed by the reader experiment; it is weak, is not cryptographic
authentication, and must not be carried into a durable format without a new
decision.

## Entries and ordering

A leaf entry contains an exact `u32` key followed by an `i32` value. Leaf keys
are strictly increasing. An empty leaf is valid.

A branch entry contains an inclusive maximum `u32` key followed by a child page
identifier. Branch maximum keys are strictly increasing, every child is less
than page count, and the final maximum key is `4294967295`. Lookup selects the
first branch entry whose maximum key is greater than or equal to the requested
key.

The reader validates page-local order and child bounds. It does not yet prove
global separator/leaf-range consistency or that every page is reachable from
the root.

## Portable API

The module exports:

```text
Wvdbˉreaderˉvalidate(Input: bytes) -> Wvdbˉvalidationˉresult
Wvdbˉreaderˉlookup(Input: bytes, Key: u32) -> Wvdbˉlookupˉresult
```

Validation returns `Valid` or `Failure(Wvdbˉfailure)`. Lookup returns
`Found(i32)`, `Missing`, or `Failure(Wvdbˉfailure)`. A failure carries a
`Wvdbˉerror` and the exact input offset associated with the rejected field or
page.

The errors distinguish input size, database magic/version/header, page
magic/version/identity/kind/count/reserved fields, checksum, entry ordering,
child bounds, padding, immediate self-cycles, and exhausted depth. A longer
cycle is bounded by the depth limit and is reported as `Invalidˉdepth`.

Lookup validates the complete envelope first and then follows the selected
path with recursion bounded by the declared limit. It returns `Missing` only
after reaching a valid leaf that does not contain the key.

## Verification contract

The Stage 0 oracle constructs format bytes independently in C#. The Windvale
reader test covers:

- found and missing keys on both sides of a branch root;
- negative `i32` values and the maximum `u32` query;
- one-page empty and 64-page maximum inputs;
- empty, truncated, oversized, bad-magic, unsupported-version, and inconsistent
  headers;
- invalid page magic, version, identity, kind, count, reserved fields,
  checksum, order, child, and padding;
- an immediate cyclic child; and
- a valid 17-page chain that exceeds the declared depth limit.

The library is compiled twice to prove deterministic bytes, pinned by its
current SHA-256 in the Seed tests, verified as WVB, composed with a portable
bytes adapter, and executed by the reference runtime. The existing
Windvale-written source-to-WVB conformance case also consumes this source and
requires byte-for-byte agreement with Stage 0.

## Evolution boundary

An application may embed these bytes as immutable data or obtain them through
an explicitly authorized adapter, but the input snapshot must fit the bounds
above. Whole-file hosted reading does not provide database durability.

The next database format must be separately proposed if it adds larger page
identities, variable keys or values, global tree proofs, page caching, mutable
files, a WAL, flush semantics, atomic root publication, recovery, or concurrent
ownership. No `WVDB 1` experimental byte is migration evidence for that later
format.
