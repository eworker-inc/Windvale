# Windvale native hosted publication-request producer

## Status and scope

This contract constructs the exact canonical ten-service `WVPQ 1` request
from admitted immutable fragment/service source geometry described by
`WVSG 1`. It removes publication-request byte construction from the managed
hosted tool pipeline without treating geometry as content identity or authority.

The request records only fragment and service extents. The command therefore
does not acquire source chunks; the later service-bundle request producer
validates the actual immutable resources before using their bytes.

## Command contract

```text
wvhostpublicationrequest <sources.wvsg> <request.wvpq>
```

`WVSG` must describe exactly eleven ordered regions. Region zero is the
nonempty native fragment at image offset zero. Regions one through ten are
the nonempty service leaves in canonical hosted-tool order: service IDs `1`
through `8`, then `11` and `12`.

The command writes the exact 144-byte `WVPQ 1` envelope with all reserved
fields zero. Before publication it executes the Windvale publication planner
and requires the resulting image extent and every service ID, placement, and
length to reproduce the supplied `WVSG` geometry exactly. This prevents a
well-formed but inconsistent geometry manifest from becoming a request.

Input and output names must not alias textually. Rejection returns status 2,
reports one diagnostic line, and preserves an existing output. Wrong argument
count returns 64. Success returns zero and reports the exact request length.

The application declares exactly `console.write_line`,
`diagnostic.write_line`, `file.read_bytes`, `file.write_bytes`,
`process.argument`, and `process.argument_count`.

## Targets and exact identities

- `windows-x64-hosted-publication-request-v1`, producing `.exe`;
- `linux-x64-hosted-publication-request-v1`, producing `.elf`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Publication-request WVB | 22,067 | `7d525451a92d2f0969e5c9006b43f16cd5485fe7791526e4769a920ec01ad430` |
| Windows application | 240,640 | `6d382e6d3a1442fdbf0cf46ff6cc52aabfd1bd6fed86171775d8acc1fdeef0b1` |
| Linux application | 241,664 | `7a3c97a9e8abc36accc54e94a7abe968486ac679dd5a34b5f18b86a68ab2dd15` |

The WVB reconstructs byte-for-byte through the native Project 1 front door.
The package writers are deletion-bound Stage 0 target and identity wiring.

## Retirement boundary

The focused current-host evidence compares the complete request byte-for-byte
with the frozen C# recovery oracle, exercises the public current-host package
without loading the CLR, rejects malformed and placement-inconsistent
geometry without changing output, rejects an input/output alias, and
reconstructs the WVB through the native front door.

The retained managed `Build_request` is now differential/recovery evidence.
Ordered process invocation, response and manifest lifecycle, Linux execution,
normal-path promotion, and the grouped retirement gate remain.
