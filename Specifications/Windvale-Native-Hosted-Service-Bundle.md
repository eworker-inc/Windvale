# Windvale native hosted service-bundle producer

## Status and scope

This contract packages the portable segmented native service-bundle
materializer as one standalone Windows/Linux command. It consumes one exact
`WVSQ 2` request and writes the complete exact `WVSI 2` response for that
canonical segment.

The command does not select a native fragment or service leaves, construct the
publication-plan request, acquire immutable source resources, or orchestrate a
multi-segment bundle. Those remain a preceding resource-evidence boundary. This
command owns request validation, canonical fragment/service copying, alignment
fill, response admission, and one immutable segment result.

## Command contract

```text
wvhostbundle <request.wvsq> <response.wvsi>
```

The input is the exact bounded `WVSQ 2` envelope defined by
[native service-bundle materialization](Windvale-Native-Service-Bundle-Materialization.md).
The shared portable core validates the embedded `WVPQ 1` publication request,
canonical segment extent, source intersections, and complete payload. It emits
the exact `WVSI 2` response, including the 40-byte evidence header and the
constructed segment.

The hosted shell accepts only a successful response whose total length,
accepted request length, embedded-plan length, segment offset, segment length,
service count, and payload extent agree with the admitted request. Identical
input/output names return usage status 64. Any request or response rejection
returns status 2 and leaves an existing output unchanged. Success returns zero
and reports the complete response byte count.

The module declares exactly `console.write_line`, `diagnostic.write_line`,
`file.read_bytes`, `file.write_bytes`, `process.argument`, and
`process.argument_count`. Its native fragment requires the existing nine host
services shared by the hosted-container transition tools.

## Targets and exact identities

- `windows-x64-hosted-service-bundle-v1`, producing `.exe`;
- `linux-x64-hosted-service-bundle-v1`, producing `.elf`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Service-bundle producer WVB | 20,144 | `5d807ac96d6e5e89cf45a5ba2e30b336e05fcfd208ca186ff94301a4b931146a` |
| Windows service-bundle producer | 220,672 | `aaa241a80550bb2c3a20b6abf648bda5d8eea0c1a847bb5924eb25af73bb3a42` |
| Linux service-bundle producer | 221,184 | `096e32397d37852f0f91634e25358421718896098b1202f871bbf5d2d022ca3a` |

The WVB reconstructs through the native Project 1 front door. Focused
current-host evidence builds the public CLI target, materializes one canonical
hosted-tool fixture request exactly, independently admits the response, observes
no CLR load, preserves an existing output after request corruption, and rejects
an output alias.

## Retirement boundary

`WVSQ` validation, segmented bundle-byte construction, `WVSI` response
admission, and immutable response-file production now have a native process
boundary on both hosts. The new C# target writer is deletion-bound package
layout and identity wiring.

The [native request producer](Windvale-Native-Hosted-Service-Bundle-Request.md)
now selects source intersections and constructs one exact `WVSQ` request from
immutable `WVSG` resources. Ordered invocation remains to be orchestrated. The
[native metadata-request producer](Windvale-Native-Hosted-Metadata-Request.md)
now recomputes multi-segment evidence and constructs `WVHM`. Complete pipeline
composition, Linux execution, promotion, and the grouped dual-host retirement
gate remain pending.
