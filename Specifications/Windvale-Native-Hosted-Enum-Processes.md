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
revalidates the WVB 1.11 envelope with seven exact sections and a hosted-only
nominal directory reader; it is not a replacement for whole-module semantic
verification and does not widen the accepted-subset native lowerer. It supports
at most 128 nominal types, with no more than 64 records, 64 enums, or 64
variants, and the current single complete `WVEQ 2`
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
| Enum-request WVB | 30,759 | `682c2bf76569ba0ec6c58dfd3ade64d7582a9d22c397c55a22e1785fe8521fb6` |
| Windows enum-request application | 334,336 | `47394d8982403c3f473e2f62f33790fab9d12e4607f58e2ba603027738410908` |
| Linux enum-request application | 335,872 | `b428ca6305422bcd168029d451840db513543b7a3d578a9f989d8f6f9635fef0` |
| Enum-service WVB | 18,883 | `6e44a4c0f4d61ea9aa3d72442baba60080896c0cf7d3536b353fcd61ff48ec07` |
| Enum-service WVO | 167,750 | `e00168aac4422a6a38d6c7c202d8fc2377b7770c4f3fc144d1ce207271f978bb` |
| Enum-service raw fragment | 166,682 | `38ea83b0d417bdc57cd0c5b3bd29f8d9cb37a9575767401486fde6da2ded4cea` |
| Windows enum-service application | 184,832 | `741af74720dd67f45ad2fad2c2db706c946f80ed57299d4af2864dc5f1aa9107` |
| Linux enum-service application | 184,320 | `20b949369c323070fd1dd7f2719f4c11f0eb163376cab2bc07f1b92f3eb2834b` |

The focused contract uses the native front door as the module producer, the
digest-bound native WVB-to-WVO candidate as the object producer, and the
digest-bound native linker at base zero as the raw-fragment producer. Each
result must agree byte for byte with the frozen C# differential oracle. Package
wiring remains deletion-bound Stage 0 evidence until ordered native
orchestration and the final retirement gate qualify.
