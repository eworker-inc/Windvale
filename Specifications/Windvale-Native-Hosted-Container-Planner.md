# Windvale native hosted-container planner

## Status and scope

This contract packages the portable `WVCR 1` to `WVCD 1` hosted-container
planner as a standalone native Windows/Linux command. It is the first process
boundary in the native host-package construction pipeline and produces the
same exact plan previously available only through a service-free fragment
dispatched by Stage 0.

The command does not construct startup bytes, PE/ELF headers, imports,
relocations, service bundles, final segments, or a public destination. It reads
one already admitted 4,096-byte hosted runtime header and delegates all layout
and target derivation to the shared portable construction core.

## Command contract

```text
wvhostplan <runtime.wvhr> <plan.wvcd> [publication-plan.wvcd]
```

The runtime header contains canonical hosted metadata at offset 480. The tool
derives the target and implemented profile from that metadata, builds the exact
4,128-byte `WVCR 1` request, and invokes
`Linkerˉnativeˉhostedˉcontainerˉconstruction.Main`. It writes only a successful
`WVCD 1` response whose envelope, accepted request length, and status are exact.

The implemented container-version mapping is `3 -> 1`, `5 -> 2`, `6 -> 3`,
`7 -> 4`, `8 -> 5`, `9 -> 6`, `10 -> 7`, and `11 -> 8`. The shared planner independently
revalidates the matching target, profile, metadata magic, limits, service
placements, layout, and target table. An unknown or inconsistent value is
therefore rejected rather than inferred.

The optional third output is needed by Profile 8 packaging. The tool writes the
complete real Profile-8 plan to `plan.wvcd` and derives a second
plan whose only change is profile 7 in the fixed 128-byte publication header.
It independently validates that header before writing either output. The
retained atomic publisher consumes that compatibility header; platform bytes,
startup, source-set geometry, and container bytes continue to use the real
Profile-8 plan. Other profiles produce an identical publication plan when the
optional output is requested.

Identical input/output names are rejected with usage status 64. A malformed or
rejected runtime returns status 2, writes one diagnostic, and leaves an existing
output unchanged. Success returns zero and reports the exact plan byte count.

The module declares exactly `console.write_line`, `diagnostic.write_line`,
`file.read_bytes`, `file.write_bytes`, `process.argument`, and
`process.argument_count`. Its native fragment requires the existing nine
services used by the segmenter. The Stage 0 package uses the established
compiler-authority host envelope; module and target identities keep planner
semantics separate. Decision 0900 introduces the exact compiler-analysis
profile and publication-plan bridge described above.

## Targets and last candidate identities

- `windows-x64-hosted-container-planner-v1`, producing `.exe`;
- `linux-x64-hosted-container-planner-v1`, producing `.elf`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Planner WVB | 47,889 | `82581cb918c2528941ac02ca31281f230a84f0afeb01d6b275b112f983ec4cc8` |
| Windows planner | 718,848 | `0978d70bd0774d89e8669127e90896ac571b6405ab6752b1b655df98bdbcf9b3` |
| Linux planner | 720,896 | `62ff98d4d50e35bdd0170b398866a170d28f98cf22965b25ca3554ae46aafd08` |

Decision 0900 reconstructs these current candidate identities with Profile 8
and the bounded publication-plan bridge. Local Windows packaging consumes a
real plan successfully. Independent Linux execution, cross-host reconstruction,
promotion, and grouped qualification remain.

## Retirement boundary

The new C# target writer is deletion-bound package layout and identity wiring.
The planner source, runtime behavior, layout semantics, and failure decisions
are Windvale-owned. The ordinary managed hosted-application builders still
dispatch the retained planner fragment internally. The standalone
[platform-byte producer](Windvale-Native-Hosted-Container-Platform-Bytes.md)
now consumes this plan for PE/ELF-owned regions, and the standalone
[startup producer](Windvale-Native-Hosted-Container-Startup.md) consumes its
target table with the canonical WVO. Completing the process pipeline still
requires metadata/service-bundle evidence and composition. The standalone
[runtime-header producer](Windvale-Native-Hosted-Container-Runtime.md) now
supplies this tool's raw input, with metadata supplied by the standalone
[metadata constructor](Windvale-Native-Hosted-Container-Metadata.md).
Metadata-request/service-bundle evidence, Linux execution, and the grouped
dual-host gate remain open.
