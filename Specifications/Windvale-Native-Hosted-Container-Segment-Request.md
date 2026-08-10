# Windvale native hosted-container segment-request producer

## Status and scope

This contract constructs one exact `WVHT 1` request from a successful complete
`WVCD 1` plan and six immutable source regions described by `WVSG 1`. It moves
final application-segment selection, source intersection, and request-byte
construction out of the managed hosted-container materialization session.

One invocation produces one request. A read-only count mode admits the same
plan and resources and reports the exact bounded iteration count without host
decoding of `WVCD`. Ordered process invocation and temporary-resource lifecycle
remain the next orchestration boundary; no partial multi-file set is treated
as committed output.

## Command contract

```text
wvhostsegmentrequest <plan.wvcd> <sources.wvsg> <chunk-prefix> <segment-index> <request.wvht>
wvhostsegmentrequest <plan.wvcd> <sources.wvsg> <chunk-prefix> count
```

The decimal `u32` segment index must select a canonical 4,194,144-byte segment
or the exact shorter final segment. The command admits the complete successful
container plan and its 128-byte layout header. `WVSG` must describe exactly six
ordered logical regions whose output offsets and lengths match header, startup,
service bundle, imports, runtime, and relocation fields in that plan. Empty
target-specific imports or relocation regions retain their ordinals and use
the canonical source-manifest anchors: bundle end for empty imports and runtime
end for empty relocations, rather than the plan's zero absent-section sentinel.

Every declared chunk resource is length-validated before output. The producer
then emits the exact 32-byte `WVHT` header, exact 128-byte plan header, and only
the ordered source bytes intersecting the selected segment. Fill gaps remain
omitted because the existing Windvale segment constructor owns zero fill.

Count mode performs the same complete-plan, geometry, and resource admission,
derives the ceiling division by 4,194,144 in Windvale, writes no file, and
reports `segments=N`. It is process-control output rather than a new serialized
format.

Control, derived source, and output names must not alias textually. Rejection
returns status 2, reports one diagnostic line, and preserves an existing
output. Wrong argument count returns 64. Success returns zero and reports the
segment index and exact request byte count.

The application declares exactly `console.write_line`,
`diagnostic.write_line`, `file.read_bytes`, `file.write_bytes`,
`process.argument`, and `process.argument_count`.

## Targets and exact identities

- `windows-x64-hosted-container-segment-request-v1`, producing `.exe`;
- `linux-x64-hosted-container-segment-request-v1`, producing `.elf`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Container-segment request WVB | 45,109 | `ae47ede7efb28af2a17c1c1530035e5fd9b1cc1f4d4ba416801b61d8f3d1da89` |
| Windows application | 526,848 | `b9477d3fb5206bb5df4a4e8ee4f48b081ef201d667edda99854cb857dd62c2d7` |
| Linux application | 528,384 | `34026f3f03bbda40ebf5be08f0ca0d2a9e9f1bf13abc29e0db6e81e837884f38` |

The WVB reconstructs byte-for-byte through the native Project 1 front door.
The package writers are deletion-bound Stage 0 target and identity wiring.

## Retirement boundary

The focused current-host evidence crosses the bundle region over two immutable
resources, matches the frozen C# `WVHT` oracle byte-for-byte, executes the
public native process without loading the CLR, and proves malformed, invalid-
segment, alias, and output-preservation behavior.
The same native process now supplies the admitted final iteration count.

The managed `Build_request` is now differential/recovery evidence. Ordered
invocation of the service-bundle and hosted-container request/response tools,
including Decision 0405's upstream publication request, private cleanup, Linux
execution, normal-path promotion, and the grouped retirement gate remain.
