# Windvale native hosted-verifier runtime header

## Status and scope

This contract transfers construction of the fixed hosted verifier's exact
4,096-byte initial runtime header into portable Windvale. It reuses the shared
`WVHR 1` request and `WVHS 1` response envelopes. It admits `WVHV 1` profiles
2, 6, 7, and 8 through the verifier-specific metadata owner. The separate
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
| 16 | 4 | verifier profile | `2`, `6`, `7`, or `8`, matching metadata |
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
| 264 | 136 | Target-specific file-input table 1 with one snapshot slot, or two for profile 7 |
| 400 | 80 | Zero reserved file-output region |
| 480 | 1,024 | Exact admitted `WVHV 1` metadata |
| 1,504 | 2,592 | Zero reserved tail |

Every pointer and initial used/count field is zero. The record arena is 2 MiB,
the hosted text arena is 128 MiB, and call depth is 1,024. Linux starts with
console and diagnostic targets 1 and 2; Windows binds those targets at startup.
File-input name and data strides are 1 MiB and 4 MiB. Profiles 2, 6, and 8
retain one snapshot slot. Profile 7 owns two immutable snapshot records, two
name strides, and two data strides because the console-application verifier
compares two input application chunks. In its file-input table, capacity at
absolute offset 288 is `2`, initial count at 292 is zero, name stride at 304 is
1,048,576, and data stride and maximum data bytes at 320 and 324 are 4,194,304.
Path scratch at 336 remains 2,097,154 bytes on Windows and 1,048,577 bytes on
Linux. Every pointer remains zero, and no profile has a file-output table or
output scratch.

For profile 7, the two-snapshot runtime layout is argument table `4,096/1,072`,
argument bytes `5,168/65,536`, snapshot table `70,704/64`, record arena
`73,728`, text arena `2,170,880`, name arena `136,388,608`, data arena
`138,485,760`, and input scratch `146,874,368`. The resulting virtual extent is
`148,975,616` bytes on Windows or `147,927,040` bytes on Linux. These derived
addresses are consumed by the separate startup/layout owner; this runtime
constructor serializes the profile and capacity but does not select startup
relocation targets.

## Ownership and evidence

[`Native-Hosted-Verifier-Runtime-Header-Core.wv`](../Runtime/Windvale/Native-Hosted-Verifier-Runtime-Header-Core.wv)
owns construction. The separate
[`Native-Hosted-Verifier-Metadata-Admission.wv`](../Runtime/Windvale/Native-Hosted-Verifier-Metadata-Admission.wv)
owns metadata admission. A small bridge is the root of
[`Windvale-Native-Hosted-Verifier-Runtime.wvproj`](../Windvale-Native-Hosted-Verifier-Runtime.wvproj).

The current profile-7-capable source constructs a 19,333-byte WVB with SHA-256
`fbd36782659cedebedfb24525bec1a97afee66d720982ebd11eaeab485419fe7`.
The focused current-host test retains its service-free
`Main(bytes) -> bytes` shape and ten profile-2 malformed requests. It now also
defines exact Windows/Linux profile-7 comparisons across the Windvale
interpreter, native backend, and frozen C# recovery oracle, including snapshot
capacity `2` at absolute offset 288. This source slice measured the WVB without
executing that test. C# is not used to compile the production module and
remains differential evidence only.
