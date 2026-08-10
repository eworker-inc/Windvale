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
32 MiB native-code ceiling. It writes the exact 32-byte `WVMI 1` record with
zero reserved fields and no host-supplied bundle offset.

`evidence` admits canonical `WVSG 1` with eleven regions and eleven through
eighteen chunks. It copies each already admitted 20-byte chunk record exactly
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
| Orchestration-control WVB | 21,214 | `1d9f86cf636de119bde26a7b5fda5977e032db336d07c3937f0dd42df000e4bf` |
| Native WVO | 219,635 | `86ba4c10926dd95c4211859edef8604489d164f6b4a0e96e8ff8dafc9841036e` |
| Windows application | 236,032 | `d8b10130bc946261526ee0accc9fcbd42dbe2a5d9fd3e4d4f349038550c8c559` |
| Linux application | 237,568 | `45c8bf1163556c851db8b7fecb2556e899c816d06bd39209d65db942fea3c44a` |

The native Project 1 front door reproduces the WVB byte for byte, and the
digest-bound native lowerer reproduces the exact Stage 0 WVO. Package wiring
remains deletion-bound Stage 0 evidence until ordered orchestration and the
final grouped retirement gate qualify.

## Retirement boundary

Managed code no longer needs to serialize `WVMI` or translate `WVSG` into the
metadata evidence manifest in the candidate pipeline. Child-process ordering,
tool-package acquisition, service and container segment iteration, private
resource teardown, Linux execution, and promotion remain separate work.
