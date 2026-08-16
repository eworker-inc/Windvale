# Windvale database JSON protocol envelope

## Status and scope

`Windvaleˉdatabaseˉjsonˉprotocol` defines the first portable, capability-free
version-1 request and response envelope for a future Windvale database server.
It admits or emits at most 65,536 bytes and builds on the [strict JSON value
boundary](Windvale-Database-Json-Value.md). Transport framing, authentication,
authorization, server scheduling, and operation-specific body contracts remain
separate boundaries.

The protocol uses exact case-sensitive external JSON names. Unknown envelope
members reject. All required members must occur once; the JSON boundary already
rejects semantic duplicate names.

## Request envelope

A request is one JSON object with these members in any order:

| Member | Required | Contract |
| --- | --- | --- |
| `contractVersion` | yes | Exact JSON integer `1` |
| `requestId` | yes | 1 through 128 ASCII bytes matching `[A-Za-z0-9][A-Za-z0-9._:@-]*` |
| `databaseId` | yes | 1 through 64 ASCII bytes matching `[a-z][a-z0-9._-]*` |
| `operation` | yes | `ping`, `get`, `put`, `delete`, `scan`, `transaction`, or `query` |
| `deadlineMilliseconds` | yes | JSON integer from 0 through 300,000 |
| `minimumCommittedSequence` | no | JSON integer from 0 through `2^64 - 1` |
| `body` | yes | One admitted JSON object |

The deadline is a caller budget, not permission to run indefinitely and not a
promise that work starts. A zero deadline means no caller-supplied additional
wait; server policy may reject it before work begins. The minimum committed
sequence is a read-freshness condition, not a snapshot grant.

Successful decoding owns one exact admitted envelope byte sequence. It decodes
only the bounded request ID, database ID, and operation name and returns the
body as an offset and length into that owned sequence. It does not allocate a
second copy of the body.

## Response envelope

A response is one JSON object with exactly these members:

| Member | Contract |
| --- | --- |
| `contractVersion` | Exact JSON integer `1` |
| `requestId` | The same bounded identifier grammar as requests |
| `status` | `ok` or `error` |
| `observedCommittedSequence` | JSON integer from 0 through `2^64 - 1` |
| `body` | One admitted JSON object |

The observed sequence reports the committed generation against which the
server formed the response. It does not by itself prove that a mutation
committed; the operation-specific response body and mutation ambiguity contract
must say that explicitly.

## Deterministic encoding

The request encoder emits members in the table order and omits
`minimumCommittedSequence` only when the caller marks it absent. The response
encoder emits members in the table order. Both use compact UTF-8 JSON without
insignificant whitespace, preserve admitted body spelling, reject invalid
inputs before output, and reject a completed envelope larger than 65,536 bytes.

Request and database identifiers deliberately use an ASCII grammar that needs
no JSON escaping. This makes encoding smaller and prevents different escaped
spellings from becoming different protocol identities.

## Errors and exclusions

Decoding distinguishes invalid JSON, a non-object envelope, unknown or missing
members, unsupported versions, invalid identifiers, operations, deadlines,
sequences, statuses, and non-object bodies. A JSON failure retains the strict
JSON error and byte offset; an envelope failure reports its own byte offset.

This first contract does not define the members allowed inside operation or
response bodies, error-code bodies, cursors, transaction idempotency,
authentication, network framing, or SQL/query payloads. Each body contract must
be strict and versioned before the corresponding operation is served. An
object body is not authority: database capabilities and server policy remain
separate.
