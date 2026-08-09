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
replacement for whole-module semantic verification. It supports at most 128
nominal types (with no more than 64 records or 64 enums) and the current
single complete `WVEQ 2`
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
| Enum-request WVB | 26,167 | `1471775aab260d48db4852cd055f04698b036224f877fcab958f3e1bd9814b83` |
| Windows enum-request application | 292,352 | `44e11d1105ab685e51ccce2dc6f800b0c2c1d7e897539cd7b65a436d4ff67f21` |
| Linux enum-request application | 294,912 | `c767e5f0c509e803dbcfe3fc1283f8bcf1208c80a0fee478d5348116f9187040` |
| Enum-service WVB | 18,976 | `493226f5b61894cb43e3428555e96293310c03571f6cff905eb50fabc7721676` |
| Enum-service WVO | 168,342 | `0ded580f703ae2d982740fe673d1e04dee581cab8785bb5d0ba8894800cb2963` |
| Enum-service raw fragment | 167,274 | `cec5c423e32a3c0bc5602551e2b1da2e82929b2edd84b2756c4062bf0f223870` |
| Windows enum-service application | 185,344 | `61d8b79ea57082c2ea85de5057a66e7c10045c44a9b8997d2ed491f3a1d90a83` |
| Linux enum-service application | 184,320 | `cd6f3b01df9a57bfe1acf2fa226c58f10c8ba51d2096a75572628cfbea427cf0` |

The focused contract uses the native front door as the module producer, the
digest-bound native WVB-to-WVO candidate as the object producer, and the
digest-bound native linker at base zero as the raw-fragment producer. Each
result must agree byte for byte with the frozen C# differential oracle. Package
wiring remains deletion-bound Stage 0 evidence until ordered native
orchestration and the final retirement gate qualify.
