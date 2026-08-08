# Windvale native hosted-container segment set

## Status and scope

This contract admits the complete immutable input set required to publish one
segmented hosted application. It joins a successful `WVCD 1` plan, the
canonical ordered `WVHT 1` requests, and their exact `WVHU 1` responses without
joining the response payloads into one managed byte array.

The portable
`Linkerˉnativeˉhostedˉcontainerˉsegmentˉsetˉcore` module owns manifest
admission. The hosted
`Nativeˉhostedˉcontainerˉsegmentˉsetˉadmissionˉtool` owns resource
acquisition and content admission. This slice is read-only: the destination
name is reserved and checked for aliases, but the following native publication
slice owns mutation.

## `WVHM 1` manifest

All integers are little-endian `u32`. The manifest is a 32-byte header followed
by exactly one 20-byte entry per segment.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVHM`, `0x4D485657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `32 + segment_count * 20` |
| 12 | 4 | application bytes | Exact successful-plan value |
| 16 | 4 | segment count | Canonical count, 1 through 31 |
| 20 | 4 | segment limit | `4,194,144` |
| 24 | 4 | complete plan bytes | Exact successful-plan envelope size |
| 28 | 4 | reserved | Zero |

Each entry contains the zero-based index, application offset, segment bytes,
request bytes, and response bytes. Entries are strictly canonical: offsets are
contiguous, every non-final segment is exactly 4,194,144 bytes, the final
segment consumes the remainder, requests are 160 through 4,194,304 bytes, and
each response is exactly 40 bytes plus its segment.

The maximum of 31 segments is intentional. Together with the plan and manifest,
the complete request/response set occupies at most 64 immutable file snapshots,
which is the current native hosted-resource bound.

## Admission command

```text
wvhostadmit <plan.wvcd> <segment-prefix> <manifest.wvhm> <destination>
```

The command derives `<segment-prefix>.request-N` and
`<segment-prefix>.response-N` in index order. It rejects empty or oversized
prefixes and any plan, manifest, derived resource, or destination alias before
the conflicting resource is read.

Admission validates the successful plan header and canonical complete plan
size, then validates the manifest. Every request must have the manifest-bound
size and carry the exact 128-byte layout header from the selected plan. The
tool reruns the shared Windvale segment constructor and requires byte-for-byte
agreement with the stored response, including successful status, application
size, offset, and segment length. A changed payload, mismatched plan header,
missing segment, reordered entry, or malformed envelope is rejected.

The tool declares only `diagnostic.write_line`, `file.read_bytes`,
`process.argument`, and `process.argument_count`. It performs no write and
does not load .NET when packaged as a native application.

## Exact candidate identity and evidence

The admission WVB is 31,271 bytes at SHA-256
`6ce0c3a4bf48b6d0db4c50574805655777be93f6a10555a4d423947b00bd0018`.
It reconstructs byte-for-byte through the native Project 1 front door.

The focused current-host test admits a real hosted-container segment set,
rejects payload corruption, rejects a request carrying a different valid plan
header, rejects a reordered manifest entry, rejects an alias before any read,
and reconstructs the pinned WVB. The paired publisher test additionally performs
real Windows durable publication without loading .NET. Linux execution and the
grouped dual-host qualification remain deferred.

## Retirement boundary

The C# test constructs independent fixtures and drives the retained reference
runtime. It is transition evidence, not product logic. It can leave the normal
test path when the digest-bound native launcher and native hostile-input lane
own equivalent evidence; the frozen recovery archive may retain it.

The paired native publisher consumes admitted response payloads directly, skips
each 40-byte `WVHU 1` envelope, and atomically replaces the destination without
managed concatenation. Its public targets are
`windows-x64-hosted-container-publisher-v1` and
`linux-x64-hosted-container-publisher-v1`.

Decision 0389 supplies the shared native snapshot-table admission beneath that
publisher. Its hosted policy selects response snapshot ordinals `3,5,7...`,
requires an even complete table, skips the 40-byte response envelope, and
checks the aggregate payload ceiling. The paired platform transaction and real
hosted execution remain the next boundary.

Decision 0390 supplies the reusable Linux mutation half. After admission and
host-identity checks, the hosted adapter can pass the same alternating response
selection directly to exclusive sibling creation, exact write/reread, rename,
and directory durability. Decision 0391 supplies the matching Windows
handle-relative transaction with current-host execution evidence. The paired
hosted adapters and launchers are now the remaining connection boundary.

Decision 0392 supplies the shared platform acquisition half. One Windows and
one Linux immutable-snapshot shell own runtime setup, complete resource
reopening and comparison, native identity, destination-alias rejection, and
durable-transaction invocation. The 14-line hosted policy entries select
ordinals `3,5,7...`, stride two, the 40-byte skip, and the hosted validator.
Decision 0393 packages the admission root with those shells, the existing
validator, the shared native publication-state object, and the durable
transactions. The exact Windows application is 379,904 bytes at SHA-256
`823b9ed3bafdb4a8cb8e5a5a3fe4c9d834f6702771766add5fbf439d8d5d2b37`;
the Linux application is 377,725 bytes at SHA-256
`02602e7fb552dafcb6bf2ed2a858eec9c17e257bfd4bc097c47f55fd155a50c9`.
Native reconstruction of the Stage 0 host-package layout, Linux execution,
promotion, and grouped qualification remain.
