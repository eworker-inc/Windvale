# Windvale native hosted-container segmenter

## Status and scope

This contract packages the portable `WVHT 1` to `WVHU 1` segment constructor
as one standalone native Windows/Linux tool. It is a source candidate for the
hosted-container publication path; it does not yet replace the managed session
that dispatches and concatenates segments.

The canonical source module is
`Nativeˉhostedˉcontainerˉsegmenterˉtool`, built by
`Windvale-Native-Hosted-Container-Segmenter-Tool.wvproj`. Shared construction
logic is owned by the focused
`Linkerˉnativeˉhostedˉcontainerˉsegmentationˉcore` module. The retained
service-free fragment wrapper and this hosted command each expose exactly one
`Main` entry rather than broadening the native-fragment export contract.

## Command contract

```text
wvhostsegment <request.wvht> <response.wvhu>
```

The command requires exactly two arguments and rejects textually identical
input and output paths with status 64. It reads one bounded immutable request,
calls the same portable constructor used by the retained WVNF fragment, and
writes only a structurally successful `WVHU 1` response. Construction
rejection returns status 2, reports one diagnostic line, and does not open or
change the output. Success returns zero and reports the response byte count.

The application declares exactly `console.write_line`,
`diagnostic.write_line`, `file.read_bytes`, `file.write_bytes`,
`process.argument`, and `process.argument_count`. Its fragment directly
requires nine services: console output, both process-argument services, file
input, diagnostic output, enum naming, text concatenation, `u32` formatting,
and file output. The existing compiler-authority package adds its internal
strict UTF-8 leaf; no new platform assembly or host callback is introduced.

## Hosted profile and targets

The segmenter owns hosted profile 7 with metadata magic `WVHG`, metadata
format 1, container-format version 10, and profile flags 8. All Windvale-owned
metadata, runtime-header, layout, and platform-byte admission paths accept
profiles 1 through 7 and reject every other value.

The public construction targets are:

- `windows-x64-hosted-container-segmenter-v1`, producing `.exe`;
- `linux-x64-hosted-container-segmenter-v1`, producing `.elf`.

Both targets use the existing hosted compiler-authority runtime and exact
ten-service bundle. The produced applications are independently parsed by the
existing hosted PE/ELF verifiers before publication.

## Exact candidate identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Segmenter WVB | 23,896 | `5fed7bad6fca036297b97b3a5ec2eea999e52682b2055abe278592b46b3b0d3f` |
| Windows application | 312,320 | `931b0bff9138f5ce7ef0142f7bb29e84d72055802325f9fe5fe3d217548ef4ad` |
| Linux application | 311,296 | `d33d0a0f4aacf8dc0276451aeaf0794a5723f1c57cc435a9b8751bab3021b4e6` |

The focused current-host evidence reconstructs the WVB through the native
Project 1 front door, constructs both packages, executes one real bounded
request directly, compares the result with the retained native fragment, and
proves that a malformed request preserves an existing output. Independent
Linux execution and grouped dual-host qualification remain deferred to the
final retirement gate.

## Retirement boundary

The new C# writer and target routing are Stage 0 package wiring. They are not
the segment algorithm and are deletion-bound once a digest-bound native
launcher owns package selection. The separate
[segment-request producer](Windvale-Native-Hosted-Container-Segment-Request.md)
now constructs exact `WVHT` inputs from immutable `WVSG` source geometry; the
retained C# request builder is differential/recovery evidence. The separate
[immutable segment-set contract](Windvale-Native-Hosted-Container-Segment-Set.md)
now reconstructs and admits every ordered response without concatenating it.
The next slice feeds that admitted set to the existing native durable
multi-chunk publisher, removing managed segment dispatch, concatenation, and
final publication from the normal route.
