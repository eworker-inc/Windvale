# Windvale native hosted-verifier metadata request

## Status and scope

This contract transfers pure construction of the exact 384-byte `WVVR 1`
request into portable Windvale. It binds verifier target and entry, the shared
six-service native publication plan, and seven nonzero SHA-256 identities into
the request consumed by the verifier metadata constructor.

The shared native hosted metadata-request process now acquires immutable
verifier and service resources, calculates all seven digests, and emits this
evidence. A small second native tool invokes the constructor and writes the
successful `WVVR` payload.

## `WVVE 1` evidence

The input is exactly 352 little-endian bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVVE` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `352` |
| 12 | 4 | target | `1` Windows x64 or `2` Linux x64 |
| 16 | 4 | verifier profile | `2` |
| 20 | 4 | native entry | Within the native fragment |
| 24 | 96 | publication request | Exact six-service `WVPQ 1` request |
| 120 | 32 | native digest | Nonzero SHA-256 |
| 152 | 192 | service digests | Six ordered nonzero SHA-256 values |
| 344 | 8 | reserved | Zero |

The publication request contains the nonempty native fragment and exact
service IDs 1 through 6 in order. The shared Windvale publication planner must
accept it and return six bounded placements. Geometry comes only from that
planner; the evidence cannot supply offsets or bundle length independently.

## `WVVD 1` response

Failure is 32 bytes. Statuses distinguish invalid size, magic, version, fixed
fields, publication plan, and digest evidence. Failure offset identifies the
rejected boundary.

Success is 416 bytes: a 32-byte `WVVD 1` header followed by exact 384-byte
`WVVR 1`. Windvale writes bundle offset 4,096, planner-derived fragment/bundle
extents and service placements, profile 2, six services, all seven supplied
digests, and zero reserved fields.

## Ownership and evidence

[`Native-Hosted-Verifier-Metadata-Request-Core.wv`](../Runtime/Windvale/Native-Hosted-Verifier-Metadata-Request-Core.wv)
owns validation and construction. A small bridge is the root of
[`Windvale-Native-Hosted-Verifier-Metadata-Request.wvproj`](../Windvale-Native-Hosted-Verifier-Metadata-Request.wvproj).

The native project front door constructs an exact 15,070-byte WVB with SHA-256
`fc87cfad498befe8af90fc5201e07c15e13c4a9363b73c344e1f6e49519dd55a`.
One focused current-host test executes the interpreter and native backend for
Windows and Linux, compares each request byte for byte with the frozen C#
oracle, and covers thirteen malformed evidence cases. C# does not compile the
production module and remains differential evidence only.

The hosted request wrapper is an exact 17,319-byte native-built WVB with
SHA-256
`e7ecb1251664430055fc26bb70371065b72ff988532af8e0897fb2acae406048`.
It accepts two distinct resource names, reads `WVVE`, requires the exact
successful `WVVD` response, and writes only the 384-byte request.

Its recovery-only retained targets are
`windows-x64-hosted-verifier-metadata-request-v1` and
`linux-x64-hosted-verifier-metadata-request-v1`. Their exact applications are
188,928-byte Windows SHA-256
`4888d4c5252164e4a2637f78dadb5e1228044ac6834525f3cd00fb3a6bbe0b0e`
and 188,416-byte Linux SHA-256
`3827deb66f8bb15585e02ba7e2e01a217cc9375eb05ade4c70a91930b0af8803`.
The C# writer owns only deletion-bound recovery target/identity wiring.
[Decision 0466](../Documents/Decisions/0466-Native-WVHV-Request-Container-Reconstruction.md)
adds exact native reconstruction of both products; independent Linux execution
and promotion remain pending.
