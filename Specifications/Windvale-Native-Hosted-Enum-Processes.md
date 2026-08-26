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

`wvhostenumrequest` is sequenced after the full native WVB verifier. Its current
digest-bound candidate uses the native x64 type reader that admits `u64` record
fields and WVB 1.30/1.31 callable descriptors; both receive zero-member
directory entries and do not change serialized enum metadata. It revalidates
the WVB 1.11, 1.30, or 1.31 envelope with seven exact sections and a hosted-only
type directory reader; it is not a replacement for whole-module semantic
verification. It supports at most 128 types, with no more than 116 records,
64 enums, 64 variants, or 128 callables within that total, and the current single complete `WVEQ 2`
group whose request and resulting `WVEN 1` each fit one 4 MiB Windvale byte
value. Records and variants receive zero-member directory entries; enum values,
strict source names, and lexical ranks are copied or derived from the verified
module. A module with no enum members still receives a complete zero-member
directory so the fixed hosted service layout and every nominal index remain
stable.

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
| Enum-request WVB | 48,791 | `b9078ff3ce1366c49e792e370d39b13a61bc38ef47763d464f12e5dd13d168fe` |
| Windows enum-request application | 544,256 | `22fd30575a7b3e81b13520c767f2f4e0f3e92b34686904b8c211ff14da42867b` |
| Linux enum-request application | 544,768 | `a3c17efee8150617fd5df9c5f19cc16cf5d3723fbf39f82509bff5d1f8c91e31` |
| Enum-service WVB | 18,883 | `6e44a4c0f4d61ea9aa3d72442baba60080896c0cf7d3536b353fcd61ff48ec07` |
| Enum-service WVO | 167,750 | `e00168aac4422a6a38d6c7c202d8fc2377b7770c4f3fc144d1ce207271f978bb` |
| Enum-service raw fragment | 166,682 | `38ea83b0d417bdc57cd0c5b3bd29f8d9cb37a9575767401486fde6da2ded4cea` |
| Windows enum-service application | 184,832 | `24e3b59da354a81a258a25694dd23379d19324c8f20e59a0d04e609338748f1d` |
| Linux enum-service application | 184,320 | `20b949369c323070fd1dd7f2719f4c11f0eb163376cab2bc07f1b92f3eb2834b` |

The focused contract uses the native front door as the module producer, the
digest-bound native WVB-to-WVO candidate as the object producer, and the
digest-bound native linker at base zero as the raw-fragment producer. Each
result is exact and digest-bound. `Package-Hosted-Wvb` consumes the separate
three-artifact enum-request candidate while retaining the remaining hosted
container toolset inventory unchanged. Independent Linux execution and the
grouped qualification gate remain pending.
