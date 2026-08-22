# Windvale native hosted orchestration control

## Status and scope

This paired command produces the two small control files that were still
constructed by managed orchestration before native hosted-container metadata
production: fixed `WVMI 1` target/profile input and `WVHS 1` evidence geometry
projected from admitted `WVSG 1` source geometry.

It keeps binary-format ownership in Windvale. A future Windows or Linux host
script only selects arguments, orders already digest-bound native processes,
and owns private temporary-resource cleanup.

## Commands

```text
wvhostcontrol metadata <windows|linux> <profile> <native-entry> <output.wvmi>
wvhostcontrol evidence <sources.wvsg> <output.wvhs>
```

`metadata` accepts profiles 1 through 7 and a native entry below the existing
64 MiB segmented native-code ceiling. It writes the exact 32-byte `WVMI 1` record with
zero reserved fields and no host-supplied bundle offset.

`evidence` admits canonical `WVSG 1` with eleven regions and eleven through
twenty-six chunks. It copies each already admitted 20-byte chunk record exactly
and projects each 16-byte source region to the corresponding 12-byte `WVHS 1`
identity region by omitting only its image placement. Logical offsets and
lengths remain unchanged. The result is self-checked against the exact source
before publication; the downstream metadata-request producer independently
admits it and hashes the actual named resources.

The evidence input and output must not alias textually. Content rejection
returns 2, reports one diagnostic line, and preserves an existing output.
Wrong mode or argument count returns 64. Each successful invocation writes one
file only. The application declares exactly console and diagnostic output,
file read/write, and process argument/count capabilities.

## Exact identities

- `windows-x64-hosted-orchestration-control-v1`, producing `.exe`;
- `linux-x64-hosted-orchestration-control-v1`, producing `.elf`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Orchestration-control WVB | 21,214 | `2bb4415cdb5b8f42c00f6d1e760d2aa0b52c5fe47f1617372eefa61c24d325f3` |
| Native WVO | 219,635 | `955e31d0d6a83ae306f885671ad2c0ad9c9dc09b366917f4c742ba8ad9e087d4` |
| Windows application | 236,032 | `2aa9fa9ea5d6d7c4860d65734de0fe538877700b9fcea5f6b8ca616e5cb2cd2f` |
| Linux application | 237,568 | `0bb6d21a928a880bfebdb522918d8aaa147bb3849f42b2c519bbdf2f7ca4a1d0` |

The native Project 1 front door reproduces the WVB byte for byte, and the
digest-bound native lowerer reproduces the exact Stage 0 WVO. Package wiring
remains deletion-bound Stage 0 evidence until ordered orchestration and the
final grouped retirement gate qualify.

## Retirement boundary

Managed code no longer needs to serialize `WVMI` or translate `WVSG` into the
metadata evidence manifest in the candidate pipeline. Child-process ordering,
tool-package acquisition, service and container segment iteration, private
resource teardown, Linux execution, and promotion remain separate work.
