# Windvale native hosted-container startup producer

## Status and scope

This contract packages plan-to-startup request projection and the existing
portable hosted-startup instantiator as one standalone native Windows/Linux
command. It is the third resource producer in the replacement host-package
construction pipeline, after the planner and platform-byte producer.

The command does not assemble startup source, define startup instructions,
plan the container, construct runtime data, materialize final segments, or
publish a destination. The canonical Windows and Linux WVA/WVO objects remain
the only startup machine-code source. The tool verifies the selected WVO's
exact identity, constructs `WVSI 1` from the plan's target table, and invokes
the existing portable instantiation core.

## Command contract

```text
wvhoststartup <plan.wvcd> <startup.wvo> <response.wvsd>
```

The first input is one successful `WVCD 1` plan: 364 bytes for Windows or 256
bytes for Linux. The command admits the plan envelope, target, implemented
profile, startup address, startup byte count, and exact target-table extent.
Target `1` requires the 4,398-byte Windows startup WVO with 40 symbols, 59
relocations, and SHA-256
`dbf9314d43b47ffc5d3cdeef3c439456b295ac5c3a1cda0b1faaff6227910161`.
Target `2` requires the 2,454-byte Linux startup WVO with 26 symbols, 32
relocations, and SHA-256
`1b8c08308d3f7320b741ae86022400ced6748352314b7f27954ec1c5a7345946`.

The producer constructs the exact 40-byte `WVSI 1` header, appends the plan's
canonical target table and complete admitted WVO, and calls
`Linkerˉnativeˉhostedˉstartupˉinstantiation.Nativeˉhostedˉstartupˉinstantiate`.
It writes only an exact successful `WVSD 1` response: 1,586 bytes for Windows
or 841 bytes for Linux, including the 32-byte response header and relocated
startup code.

All three path names must be distinct. An alias is rejected with usage status
64. A malformed plan, wrong or changed startup WVO, rejected instantiation, or
invalid response returns status 2, writes one diagnostic, and leaves an
existing output unchanged. Success returns zero and reports the exact response
byte count.

The module declares exactly `console.write_line`, `diagnostic.write_line`,
`file.read_bytes`, `file.write_bytes`, `process.argument`, and
`process.argument_count`. Pure portable SHA-256 admits the startup object before
request construction. The native fragment requires only the existing nine
host services shared by the planner, platform-byte producer, and segmenter.

## Targets and exact identities

- `windows-x64-hosted-container-startup-v1`, producing `.exe`;
- `linux-x64-hosted-container-startup-v1`, producing `.elf`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Startup-producer WVB | 42,508 | `7c68e998940600ecb56534e05635510643ba1fd218bcd3fca9e23300e1380807` |
| Windows startup producer | 373,248 | `ec1fecf3c05b130554537be6d03a064e75c833ded55677abcbde0c13d0264b3e` |
| Linux startup producer | 372,736 | `6a58768a3aa137ffd1ba49f318fe0f129a5d7430a5d06d8126af31b4411f5e94` |

The WVB reconstructs through the native Project 1 front door. Focused
current-host evidence builds the public CLI target, executes a real plan and
canonical WVO without loading .NET, matches the retained service-free
instantiator response byte-for-byte, preserves an existing output after a WVO
mutation, and rejects an output alias.

## Retirement boundary

Startup target projection, WVO identity admission, request construction, and
instantiation now have one native process boundary on both hosts. No startup
machine code or relocation algorithm is copied into the hosted shell. The new
C# target writer is deletion-bound package layout and identity wiring.

The ordinary managed hosted-application builders still invoke the retained
fragment internally. Native service-bundle and metadata-request producers now
own their bounded outputs; ordered acquisition/request orchestration remains.
The standalone
[runtime-header producer](Windvale-Native-Hosted-Container-Runtime.md) now owns
the initial header, and the standalone
[metadata constructor](Windvale-Native-Hosted-Container-Metadata.md) owns raw
metadata output. Composition of the planner, resource producers, segmenter, and
publisher, Linux execution, promotion, and the grouped dual-host retirement
gate remain pending.
