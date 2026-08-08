# Windvale native hosted fixed-service acquisition

## Status and scope

This command stages the nine fixed hosted-service leaves around the separately
produced variable `Enumˉname` service. It replaces managed selection, platform
choice, source-order mapping, and copy orchestration for these immutable
resources without duplicating their Windvale-owned machine-code generators.

The command is an acquisition boundary, not an identity oracle. It admits the
exact target-specific lengths and writes the same immutable snapshots it read.
The downstream native metadata-request producer recomputes SHA-256 over the
actual staged fragment and all ten service resources and rejects any incorrect
leaf before construction can continue.

## Command contract

```text
wvhostfixedservices <windows|linux> <chunk-prefix> <fragment-chunks> <service-1> <service-2> <service-3> <service-4> <service-5> <service-6> <service-8> <service-11> <service-12>
```

The fragment count is one through eight. Output resources use the canonical
source-geometry names `<chunk-prefix>.chunk-N`: fixed services occupy the nine
service positions around the deliberately unwritten service-7 slot. Inputs
must be distinct, must not alias any output name textually, and must have the
established target-specific lengths. The prefix is nonempty and at most 4,076
UTF-8 bytes.

Every input is read once into an immutable Windvale `bytes` snapshot. The
command validates all paths and all nine snapshots before the first output
call, then writes those exact snapshots. Content rejection returns 2, reports
one diagnostic line, and does not start publication. Wrong argument count
returns 64. Success returns zero and reports nine resources. The surrounding
orchestrator must use a private prefix, produce service 7 separately, and
require successful native metadata-request evidence before consuming the
staged set.

The application declares exactly `console.write_line`,
`diagnostic.write_line`, `file.read_bytes`, `file.write_bytes`,
`process.argument`, and `process.argument_count`.

## Fixed resource lengths

| Service | Windows bytes | Linux bytes |
| ---: | ---: | ---: |
| 1, console output | 258 | 213 |
| 2, argument count | 5 | 5 |
| 3, argument snapshot | 70 | 70 |
| 4, file input | 1,218 | 996 |
| 5, UTF-8 | 800 | 800 |
| 6, diagnostic output | 258 | 213 |
| 8, text concatenation | 249 | 249 |
| 11, `u32` formatting | 191 | 191 |
| 12, file output | 787 | 823 |

## Exact identities

- `windows-x64-hosted-fixed-services-v1`, producing `.exe`;
- `linux-x64-hosted-fixed-services-v1`, producing `.elf`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Fixed-service acquisition WVB | 7,491 | `048deb0818f11c61c2dd16b6bbcde8f7f58eb351c59149332d12bac6256797c0` |
| Native WVO | 58,340 | `674b063490c33477655233f508b337b826a448928913185cdb78e2ec1c1b78b1` |
| Windows application | 75,264 | `7f923dc636da591ac719f07a5f3c4f1f2ce24ae5866ba2176ce8dacf615583b0` |
| Linux application | 77,824 | `707144072747186ee2fd77e0a27c920a96fac03fe76b1bcaa90b7b4cb1db2dde` |

The native Project 1 front door reproduces the WVB byte for byte, and the
digest-bound native lowerer reproduces its exact Stage 0 WVO. Package wiring
remains deletion-bound Stage 0 evidence until the ordered native pipeline and
final retirement gate qualify.

## Retirement boundary

The fixed leaves remain checked-in generated artifacts with focused source-
reconstruction and identity tests. This command removes the managed service
selector and staging loop from the candidate construction path. It does not
weaken or duplicate the downstream digest gate, construct the variable enum
service, launch child processes, or own temporary-resource cleanup.
