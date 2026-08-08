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
and reconstructs the pinned WVB. Native durable publication, Linux execution,
and grouped dual-host qualification remain deferred.

## Retirement boundary

The C# test constructs independent fixtures and drives the retained reference
runtime. It is transition evidence, not product logic. It can leave the normal
test path when the digest-bound native launcher and native hostile-input lane
own equivalent evidence; the frozen recovery archive may retain it.

The next slice extracts the existing native durable multi-chunk transaction
behind a focused WVA-owned boundary. That publisher will consume the admitted
response payloads directly, skipping each 40-byte `WVHU 1` envelope, and will
atomically replace the destination without managed concatenation.

Decision 0389 supplies the shared native snapshot-table admission beneath that
publisher. Its hosted policy selects response snapshot ordinals `3,5,7...`,
requires an even complete table, skips the 40-byte response envelope, and
checks the aggregate payload ceiling. The paired platform transaction and real
hosted execution remain the next boundary.
