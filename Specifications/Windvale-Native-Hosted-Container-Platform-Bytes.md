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

The input is exactly one successful `WVCD 1` plan: 368 bytes for Windows or 256
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

## Targets and last candidate identities

- `windows-x64-hosted-container-platform-bytes-v1`, producing `.exe`;
- `linux-x64-hosted-container-platform-bytes-v1`, producing `.elf`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Platform-byte WVB | 30,822 | `23ac48cf838f5091a8f178edc087529627890e5fff632986d0b998b3780dd1e5` |
| Windows platform-byte producer | 315,392 | `42fb0671b94962f1ce68cbc7452d47f9894568909b9fedc123e5e0dbed553892` |
| Linux platform-byte producer | 315,392 | `770531840b87326501b16401a0e10760805f761dfc170b883520bdc67d99d206` |

Decision 0900 reconstructs these current candidate identities after the exact
Profile-7/8 layout change. Local Windows-host cross-target packaging consumes
the new plans successfully. Independent Linux execution, cross-host
reconstruction, promotion, and grouped qualification remain.

## Retirement boundary

PE/ELF header, Windows import, and Windows relocation production now has a real
native process boundary on both hosts. The existing portable platform modules
remain the sole semantic owners; the new hosted source owns only arguments,
resource I/O, selection, response admission, and reporting.

The ordinary managed hosted-application builders still dispatch retained
service-free fragments internally. The standalone
[startup producer](Windvale-Native-Hosted-Container-Startup.md) now closes the
next resource boundary, and
[runtime-header production](Windvale-Native-Hosted-Container-Runtime.md) is now
also standalone. The preceding
[metadata constructor](Windvale-Native-Hosted-Container-Metadata.md) is
standalone as well. Native service-bundle and metadata-request producers now
own their bounded construction and evidence. Completing replacement requires
ordered resource/request orchestration and composition of the planner,
producers, segmenter, and publisher. Linux execution, promotion, and the
grouped dual-host retirement gate remain pending.
