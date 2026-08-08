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
wvhostplan <runtime.wvhr> <plan.wvcd>
```

The runtime header contains canonical hosted metadata at offset 480. The tool
derives the target and implemented profile from that metadata, builds the exact
4,128-byte `WVCR 1` request, and invokes
`Linkerˉnativeˉhostedˉcontainerˉconstruction.Main`. It writes only a successful
`WVCD 1` response whose envelope, accepted request length, and status are exact.

The implemented container-version mapping is `3 -> 1`, `5 -> 2`, `6 -> 3`,
`7 -> 4`, `8 -> 5`, `9 -> 6`, and `10 -> 7`. The shared planner independently
revalidates the matching target, profile, metadata magic, limits, service
placements, layout, and target table. An unknown or inconsistent value is
therefore rejected rather than inferred.

Identical input/output names are rejected with usage status 64. A malformed or
rejected runtime returns status 2, writes one diagnostic, and leaves an existing
output unchanged. Success returns zero and reports the exact plan byte count.

The module declares exactly `console.write_line`, `diagnostic.write_line`,
`file.read_bytes`, `file.write_bytes`, `process.argument`, and
`process.argument_count`. Its native fragment requires the existing nine
services used by the segmenter. The Stage 0 package uses the established
compiler-authority host envelope; module and target identities keep planner
semantics separate, and no new hosted metadata profile is introduced.

## Targets and exact identities

- `windows-x64-hosted-container-planner-v1`, producing `.exe`;
- `linux-x64-hosted-container-planner-v1`, producing `.elf`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Planner WVB | 37,289 | `81cf3932c5e1d4f711b779c515a718ec1acd32c09ae17031aa63b8a66f5ce788` |
| Windows planner | 584,704 | `e401ad5aef792a49be72cf711cfc427a859fe4a534aa780ad47d3b4a2c12a5dc` |
| Linux planner | 585,728 | `8032370c7391bbc6afa94c1e8804db78f682da4e57144a2907394e202806c0d3` |

The WVB reconstructs through the native Project 1 front door. Focused
current-host evidence builds the public CLI target, executes a real plan without
loading .NET, matches the retained fragment byte-for-byte, rejects inconsistent
metadata while preserving an existing plan, and rejects an input/output alias.

## Retirement boundary

The new C# target writer is deletion-bound package layout and identity wiring.
The planner source, runtime behavior, layout semantics, and failure decisions
are Windvale-owned. The ordinary managed hosted-application builders still
dispatch the retained planner fragment internally. The standalone
[platform-byte producer](Windvale-Native-Hosted-Container-Platform-Bytes.md)
now consumes this plan for PE/ELF-owned regions, and the standalone
[startup producer](Windvale-Native-Hosted-Container-Startup.md) consumes its
target table with the canonical WVO. Completing the process pipeline still
requires remaining runtime/resource production, composition, Linux execution,
and the grouped dual-host gate.
