# Windvale native hosted-verifier runtime header

## Status and scope

This contract transfers construction of the compiler-aligned verifier's exact
4,096-byte initial runtime header into portable Windvale. It reuses the shared
`WVHR 1` request and `WVHS 1` response envelopes but admits only `WVHV 1`
profile 2 through the verifier-specific metadata owner. The separate
compiler-family runtime-header constructor continues to interpret `WVHB`
profiles 1 through 7; equal numeric profile values do not imply equal authority.

This slice constructs a service-free byte value. Native request production,
startup instantiation, container planning and publication remain later process
boundaries.

## `WVHR 1` request

The request is exactly 1,048 little-endian bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVHR` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `1,048` |
| 12 | 4 | target | `1` Windows x64 or `2` Linux x64 |
| 16 | 4 | verifier profile | `2` |
| 20 | 4 | reserved | Zero |
| 24 | 1,024 | metadata | Exact admitted `WVHV 1` record for the target |

Size, magic, version, request fields, and metadata admission fail separately.
Failure offset identifies the rejected request boundary.

## `WVHS 1` response

Failure is the shared 32-byte `WVHS 1` response. Statuses are `Invalid_size`
1, `Invalid_magic` 2, `Invalid_version` 3, `Invalid_request` 4, and
`Invalid_metadata` 5.

Success is 4,128 bytes: the 32-byte response header followed by the exact
4,096-byte runtime header. The response reports status zero, failure offset
1,048, and header length 4,096.

## Constructed runtime header

| Offset | Bytes | Content |
| ---: | ---: | --- |
| 0 | 112 | Execution context 7 with 16,000,000,000-instruction budget |
| 112 | 104 | Initial service table 5 |
| 216 | 48 | Target-specific output table 1 |
| 264 | 136 | Target-specific file-input table 1 with one snapshot slot |
| 400 | 80 | Zero reserved file-output region |
| 480 | 1,024 | Exact admitted `WVHV 1` metadata |
| 1,504 | 2,592 | Zero reserved tail |

Every pointer and initial used/count field is zero. The record arena is 2 MiB,
the hosted text arena is 128 MiB, and call depth is 1,024. Linux starts with
console and diagnostic targets 1 and 2; Windows binds those targets at startup.
File-input name and data strides are 1 MiB and 4 MiB. Path scratch is 2,097,154
bytes on Windows and 1,048,577 bytes on Linux. The read-only verifier has no
file-output table or output scratch.

## Ownership and evidence

[`Native-Hosted-Verifier-Runtime-Header-Core.wv`](../Runtime/Windvale/Native-Hosted-Verifier-Runtime-Header-Core.wv)
owns construction. The separate
[`Native-Hosted-Verifier-Metadata-Admission.wv`](../Runtime/Windvale/Native-Hosted-Verifier-Metadata-Admission.wv)
owns metadata admission. A small bridge is the root of
[`Windvale-Native-Hosted-Verifier-Runtime.wvproj`](../Windvale-Native-Hosted-Verifier-Runtime.wvproj).

The native project front door constructs an exact 17,941-byte WVB with SHA-256
`cf27254409ab5d574f6b6b19feb5958d97c3076a5f3b0806208437cfde04114e`.
One focused current-host test proves its service-free `Main(bytes) -> bytes`
shape, executes the Windvale interpreter and native backend, and compares both
Windows and Linux results byte for byte with the frozen C# recovery oracle. Ten
malformed requests agree across both Windvale execution modes. C# is not used
to compile the production module and remains differential evidence only.
