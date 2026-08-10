# Windvale native hosted-verifier metadata request

## Status and scope

This contract transfers pure construction of the exact profile-2/profile-8
384-byte and profile-6/profile-7 624-byte `WVVR 1` requests into portable Windvale. It
binds verifier target and entry, the matching fixed native publication plan,
and one nonzero SHA-256 identity for the fragment plus every ordered service
into the request consumed by the verifier metadata constructor.

The six-service native hosted metadata-request process acquires immutable
verifier and service resources and emits the seven-digest evidence. The
publisher-base metadata owner separately derives the twelve-digest profile-6
or profile-7 evidence directly from an admitted eleven-service `WVSQ 2`
request. A small second native tool invokes this constructor and writes the
successful `WVVR` payload for either exact evidence shape.

## `WVVE 1` evidence

The profile-2/profile-8 input is exactly 352 little-endian bytes:

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVVE` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `352` |
| 12 | 4 | target | `1` Windows x64 or `2` Linux x64 |
| 16 | 4 | verifier profile | `2` or `8` |
| 20 | 4 | native entry | Within the native fragment |
| 24 | 96 | publication request | Exact six-service `WVPQ 1` request |
| 120 | 32 | native digest | Nonzero SHA-256 |
| 152 | 192 | service digests | Six ordered nonzero SHA-256 values |
| 344 | 8 | reserved | Zero |

The publication request contains the nonempty native fragment and exact
service IDs 1 through 6 in order. The shared Windvale publication planner must
accept it and return six bounded placements. Geometry comes only from that
planner; the evidence cannot supply offsets or bundle length independently.

Profiles 6 and 7 use an exact 572-byte `WVVE 1` record: the same 24-byte prefix,
a 156-byte eleven-service publication request, twelve nonzero 32-byte digests,
and eight zero reserved bytes. Their fixed service IDs are 1 through 11 in
order; the distinct profile field preserves the WVO-inspector and
console-application-verifier identities.

## `WVVD 1` response

Failure is 32 bytes. Statuses distinguish invalid size, magic, version, fixed
fields, publication plan, and digest evidence. Failure offset identifies the
rejected boundary.

Success is 416 bytes for profiles 2 and 8, or 656 bytes for profiles 6 and 7: a
32-byte `WVVD 1` header followed by the exact 384-byte or 624-byte `WVVR 1`.
Windvale writes bundle offset 4,096, planner-derived fragment/bundle extents and
service placements, the selected profile, six or eleven services, all supplied
digests, and zero reserved fields.

## Ownership and evidence

[`Native-Hosted-Verifier-Metadata-Request-Core.wv`](../Runtime/Windvale/Native-Hosted-Verifier-Metadata-Request-Core.wv)
owns validation and construction. A small bridge is the root of
[`Windvale-Native-Hosted-Verifier-Metadata-Request.wvproj`](../Windvale-Native-Hosted-Verifier-Metadata-Request.wvproj).

The current profile-7-capable source constructs a 16,138-byte WVB with SHA-256
`040a30618f3ef760c782fa3cc3014f675f7ac952fd94700e4b894cd6b4145335`.
The focused current-host test retains its profile-2 malformed cases and now
defines exact Windows/Linux interpreter, native-backend, and frozen-oracle
comparisons for profile 7. This source slice measured the WVB without executing
that test. C# does not compile the production module and remains differential
evidence only.

The retained hosted request wrapper is an exact 18,272-byte native-built WVB with
SHA-256
`5265436ff876131ffd593e607df5e83d30b035bcfe1ea889e939f953a7e2d8f4`.
It accepts two distinct resource names, reads `WVVE`, requires the exact
successful `WVVD` response, and writes only the admitted request payload. Its
profile-7-capable source rebuild is repinned by the focused source test and
installed by the separately owned hosted-toolset refresh.

Its recovery-only retained targets are
`windows-x64-hosted-verifier-metadata-request-v1` and
`linux-x64-hosted-verifier-metadata-request-v1`. Their exact applications are
195,072-byte Windows SHA-256
`562f32e9a2d31c6852bbf4e8d8fb7904f966e525025df3106bcb332908ba232e`
and 196,608-byte Linux SHA-256
`c9d0f8a655daeb92539eaff6224010b422c8d4b8fb280488ba89fa01af55ac31`.
The C# writer owns only deletion-bound recovery target/identity wiring.
[Decision 0466](../Documents/Decisions/0466-Native-WVHV-Request-Container-Reconstruction.md)
adds exact native reconstruction of both products; independent Linux execution
and promotion remain pending.
