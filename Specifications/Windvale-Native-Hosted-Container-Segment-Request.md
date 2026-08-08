# Windvale native hosted-container segment-request producer

## Status and scope

This contract constructs one exact `WVHT 1` request from a successful complete
`WVCD 1` plan and six immutable source regions described by `WVSG 1`. It moves
final application-segment selection, source intersection, and request-byte
construction out of the managed hosted-container materialization session.

One invocation produces one request. Ordered process invocation and temporary
resource lifecycle remain the next orchestration boundary; no partial
multi-file set is treated as committed output.

## Command contract

```text
wvhostsegmentrequest <plan.wvcd> <sources.wvsg> <chunk-prefix> <segment-index> <request.wvht>
```

The decimal `u32` segment index must select a canonical 4,194,144-byte segment
or the exact shorter final segment. The command admits the complete successful
container plan and its 128-byte layout header. `WVSG` must describe exactly six
ordered logical regions whose output offsets and lengths match header, startup,
service bundle, imports, runtime, and relocation fields in that plan. Empty
target-specific imports or relocation regions retain their ordinals.

Every declared chunk resource is length-validated before output. The producer
then emits the exact 32-byte `WVHT` header, exact 128-byte plan header, and only
the ordered source bytes intersecting the selected segment. Fill gaps remain
omitted because the existing Windvale segment constructor owns zero fill.

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
| Container-segment request WVB | 42,788 | `f6bb1b03922296916b9afcfbe29e6ba5ce09c557a3345052272c0e58dcdfef00` |
| Windows application | 512,000 | `4b9cf3e689f348d2791c1eb1add11d3064bf665040999905c1484dcf79fcfe52` |
| Linux application | 512,000 | `487da501b797bd7285b29c034d30df4bb933b3382d632a19ac7bf6bdfd17ddfd` |

The WVB reconstructs byte-for-byte through the native Project 1 front door.
The package writers are deletion-bound Stage 0 target and identity wiring.

## Retirement boundary

The focused current-host evidence crosses the bundle region over two immutable
resources, matches the frozen C# `WVHT` oracle byte-for-byte, executes the
public native process without loading the CLR, and proves malformed, invalid-
segment, alias, and output-preservation behavior.

The managed `Build_request` is now differential/recovery evidence. Ordered
invocation of the service-bundle and hosted-container request/response tools,
including Decision 0405's upstream publication request, manifest publication,
Linux execution, normal-path promotion, and the grouped retirement gate remain.
