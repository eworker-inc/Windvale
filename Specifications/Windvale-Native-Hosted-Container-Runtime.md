# Windvale native hosted-container runtime-header producer

## Status and scope

This contract packages canonical metadata admission and the existing portable
runtime-header constructor as one standalone native Windows/Linux command. It
produces the exact 4,096-byte raw runtime header consumed directly by the
standalone hosted-container planner.

The command does not construct metadata, service-bundle code, container layout,
startup code, final segments, or a destination application. It derives target
and hosted profile only from an admitted `WVH* 1` metadata record, constructs
the existing `WVHR 1` request, invokes the existing portable core, admits the
`WVHS 1` response, and writes its raw runtime-header payload.

## Command contract

```text
wvhostruntime <metadata.wvhm> <runtime.wvhr>
```

The metadata input is exactly 1,024 bytes. Its target field selects Windows x64
or Linux x64. Container version `3` maps to hosted profile 1; versions `5`
through `10` map to profiles 2 through 7. The shared metadata admission module
then validates the complete profile, capability, service, adapter, extent,
digest, limit, and reserved-byte contract before request construction.

The tool constructs the exact 1,048-byte `WVHR 1` request and calls
`Runtimeˉnativeˉhostedˉtoolˉruntimeˉheader.Nativeˉhostedˉtoolˉruntimeˉheaderˉbuild`.
It admits only the exact successful 4,128-byte `WVHS 1` response, including its
accepted-request length and embedded metadata equality, then writes the
4,096-byte payload. That raw output is already the planner's input contract.

Identical input/output names are rejected with usage status 64. A malformed or
inconsistent metadata record, rejected constructor response, or response whose
metadata changed returns status 2, writes one diagnostic, and leaves an existing
output unchanged. Success returns zero and reports exactly 4,096 bytes.

The module declares exactly `console.write_line`, `diagnostic.write_line`,
`file.read_bytes`, `file.write_bytes`, `process.argument`, and
`process.argument_count`. Its native fragment requires the existing nine host
services used by the other hosted-container transition tools.

## Targets and exact identities

- `windows-x64-hosted-container-runtime-v1`, producing `.exe`;
- `linux-x64-hosted-container-runtime-v1`, producing `.elf`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Runtime-header producer WVB | 23,475 | `08533638a689db5b12628ec401c9d4c395507d2dcbda424dc40b36d1c29aa310` |
| Windows runtime-header producer | 248,832 | `5e1c241ca5c5e2fce166f2d88899dafbbab88a8dd90bfbb1a1d81863a9a1c286` |
| Linux runtime-header producer | 249,856 | `de729966eca15d95f5ae89b5416c6d3f061635a6634807556ac817b094548604` |

The WVB reconstructs through the native Project 1 front door. Focused
current-host evidence builds the public CLI target, consumes Windvale-constructed
metadata, matches the retained runtime-header fragment exactly without loading
.NET, preserves an existing output after a metadata mutation, and rejects an
output alias.

## Retirement boundary

Metadata-to-`WVHR` projection, runtime-header construction, response admission,
and raw planner-input production now have a native process boundary on both
hosts. The new C# target writer is deletion-bound package layout and identity
wiring.

The standalone
[metadata constructor](Windvale-Native-Hosted-Container-Metadata.md) now
supplies this command's raw input from an exact `WVHM 1` request. Native request
construction from immutable fragment/service evidence, service-bundle resource
production, segment-request orchestration, complete pipeline composition,
Linux execution, promotion, and the grouped dual-host retirement gate remain.
