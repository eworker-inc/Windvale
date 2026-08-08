# Windvale native hosted enum processes

## Status and scope

These paired commands construct the variable hosted `Enumˉname` service
resource without managed code. The first derives the existing bounded metadata
request from a canonical WVB; the second produces the exact fixed native leaf
followed by its admitted runtime-private metadata.

## Command contracts

```text
wvhostenumrequest <module.wvb> <request.wveq>
wvhostenumservice <request.wveq> <service.bin>
```

`wvhostenumrequest` is sequenced after the full native WVB verifier. It
revalidates the WVB 1.11 envelope with seven exact sections and the nominal-
type section covered by the current native x64 type decoder; it is not a
replacement for whole-module semantic verification. It
supports at most 64 nominal types and the current single complete `WVEQ 2`
group whose request and resulting `WVEN 1` each fit one 4 MiB Windvale byte
value. At least one enum member is required. Records receive zero-member
directory entries; enum values, strict source names, and lexical ranks are
copied or derived from the verified module.

`wvhostenumservice` accepts that bounded `WVEQ 2`, invokes the existing
Windvale-owned metadata core, independently checks its one-group `WVEC 1` and
`WVEN 1` envelopes, and writes the exact 323-byte Windvale-owned x86-64
`Enumˉname` leaf followed by the complete metadata block.

Both commands reject input/output aliases before reading, return 2 on rejected
content, preserve an existing output on rejection, and return 64 for the wrong
argument count. Each declares only console and diagnostic output, file
read/write, and process argument/count capabilities.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Enum-request WVB | 25,098 | `cd3332893277fbdc5c64e90e62900458bad506ec10be5d8b381ea9ca61a14b97` |
| Windows enum-request application | 279,040 | `64b6cad08646204af01dc6b6d06b581f54cfc2993ddb8f3d28b22b6f3f9cf032` |
| Linux enum-request application | 278,528 | `e601e3e9a9259f48c0f8d7e59f9212422d4f520ce4d4b5bbe30f6381e4970a9f` |
| Enum-service WVB | 17,511 | `2aaa45372322f39c751e6abb3062c72c14d949eb29c6edd7ca756d4378955255` |
| Windows enum-service application | 162,304 | `c4f2a7190ee68e39bc76f5870577be6db15e3763b18656ad40ec4ccd591cd1a8` |
| Linux enum-service application | 163,840 | `1c118fc24c2948a64cd9f6c1a49163cfc62333330b86b30f54998307fa6a99dc` |

The focused contract uses the native front door as the module producer and
retains the C# implementation only as a frozen differential oracle. Package
wiring remains deletion-bound Stage 0 evidence until ordered native
orchestration and the final retirement gate qualify.
