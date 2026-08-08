# Windvale native hosted-container platform bytes

## Status and scope

This contract packages the existing portable Windows and Linux hosted-container
byte constructors as one standalone native command. It is the second producer
in the replacement host-package construction pipeline, after the standalone
planner, and emits the target-owned bytes needed by bounded materialization.

The command does not plan layout, instantiate startup code, construct the
service bundle or runtime header, materialize final segments, or publish a
destination. It selects one of the existing Windvale-owned platform
constructors from the admitted target in the plan; it does not duplicate PE or
ELF construction rules in its hosted shell.

## Command contract

```text
wvhostbytes <plan.wvcd> <regions.wvhb>
```

The input is exactly one successful `WVCD 1` plan: 360 bytes for Windows or 252
bytes for Linux. Target `1` selects
`Linkerˉnativeˉhostedˉcontainerˉwindows.Main`; target `2` selects
`Linkerˉnativeˉhostedˉcontainerˉlinux.Main`. Any other target is rejected.

The tool admits the selected constructor response before writing it. A Windows
success is exactly 4,652 bytes: a 32-byte `WVWB 1` response header followed by
the 512-byte PE header, 4,096-byte import page, and 12-byte relocation block. A
Linux success is exactly 4,128 bytes: a 32-byte `WVLB 1` response header followed
by the 4,096-byte ELF header page. The response binds its total length, success
status, input-plan length, and every payload extent.

Identical input/output names are rejected with usage status 64. A truncated,
malformed, rejected, or inconsistent plan returns status 2, writes one
diagnostic, and leaves an existing output unchanged. Success returns zero and
reports the exact response byte count.

The module declares exactly `console.write_line`, `diagnostic.write_line`,
`file.read_bytes`, `file.write_bytes`, `process.argument`, and
`process.argument_count`. Its native fragment requires the same existing nine
services as the planner and segmenter. The deletion-bound Stage 0 package writer
uses the established compiler-authority host envelope; it adds no product
semantics or new hosted metadata profile.

## Targets and exact identities

- `windows-x64-hosted-container-platform-bytes-v1`, producing `.exe`;
- `linux-x64-hosted-container-platform-bytes-v1`, producing `.elf`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Platform-byte WVB | 29,793 | `3cce3e2d548be4f9304a6e6ae62355d42b2879c4fe837283fb8415ea4d715732` |
| Windows platform-byte producer | 309,760 | `46db452f1356dadb93bf80d4a81a34cf73e02d0f45342309700b5892ea571f7b` |
| Linux platform-byte producer | 311,296 | `cf09c62056d4960e914504779973a7227bcb2d9879c4328496adb859f83c526d` |

The WVB reconstructs through the native Project 1 front door. Focused
current-host evidence builds the public CLI target, executes a real plan without
loading .NET, matches the retained platform fragment byte-for-byte, preserves an
existing output on rejection, and rejects an input/output alias.

## Retirement boundary

PE/ELF header, Windows import, and Windows relocation production now has a real
native process boundary on both hosts. The existing portable platform modules
remain the sole semantic owners; the new hosted source owns only arguments,
resource I/O, selection, response admission, and reporting.

The ordinary managed hosted-application builders still dispatch retained
service-free fragments internally. Completing their replacement requires
standalone startup instantiation and remaining runtime/resource production,
then composition of planner, platform-byte producer, startup producer,
segmenter, and publisher. Linux execution, promotion, and the grouped dual-host
retirement gate remain pending.
