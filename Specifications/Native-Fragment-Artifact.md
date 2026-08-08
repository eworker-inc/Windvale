# Windvale native fragment artifact (`WVNF 1.0`)

## Purpose and status

`WVNF 1.0` is the versioned serialization of one verified Windvale x86-64
native fragment. It preserves the complete input required by the existing
native fragment verifier so a retained native consumer can be loaded without
decoding and lowering its source WVB in the normal runtime.

WVNF is derived machine evidence. Canonical WVB remains the portable semantic
and distribution contract. A WVNF is valid only for its explicit target, ABI,
architecture, and runtime contracts; it must be rejected rather than adapted
when any identity is unsupported. Source WVBs and their exact reconstruction
remain recovery and differential evidence.

WVO is not interchangeable with WVNF. WVO separates sections, clears
relocation placeholders, and omits the native target, ABI, nominal metadata,
and runtime-service requirements. Reconstructing a fragment from WVO alone
would require a linker plus guessed or separately hardcoded verifier input.
WVNF instead retains the already linked, base-independent code and all verifier
evidence directly.

## Encoding

All integers are little-endian. Strings are a `u32` byte count followed by
strict UTF-8, contain at least one byte, and contain at most 255 bytes. No
record admits flags or reserved bits in version 1.0.

The fixed 48-byte header is:

| Offset | Width | Field |
| ---: | ---: | --- |
| 0 | 4 | ASCII magic `WVNF` |
| 4 | 2 | major version `1` |
| 6 | 2 | minor version `0` |
| 8 | 4 | exact total artifact bytes |
| 12 | 4 | code bytes |
| 16 | 4 | symbol count |
| 20 | 4 | patch count |
| 24 | 4 | nominal-type count |
| 28 | 4 | required-service count |
| 32 | 4 | fragment alignment |
| 36 | 4 | signed native ABI version |
| 40 | 1 | object architecture |
| 41 | 1 | flags, zero |
| 42 | 2 | reserved, zero |
| 44 | 4 | target-name bytes |

The target bytes and complete final fragment code follow the header. Symbols,
patches, nominal types, and required services then follow in their declared
orders. The final service byte must end the artifact exactly.

### Symbols and patches

A symbol contains binding `u8`, kind `u8`, zero `u16`, code offset `u32`, size
`u32`, and one length-prefixed machine name. A patch contains kind `u8`, zero
flags `u8`, zero reserved `u16`, code offset `u32`, target-symbol index `u32`,
and signed addend `i32`.

The symbol index avoids repeating attacker-controlled names in every patch.
Decoding resolves it back to the exact symbol name expected by the in-memory
fragment contract. WVNF code already contains the applied base-independent
displacement; the retained patch records let the native verifier independently
recalculate and compare every field.

### Nominal metadata

Each nominal type contains kind `u8`, zero flags `u8`, zero reserved `u16`, its
name string, and item count `u32`. Version 1.0 admits record kind `1` and enum
kind `2` only.

A record item contains its field-name string followed by this 16-byte value
shape: kind `u8`, element kind `u8`, zero reserved `u16`, signed nominal-type
index `i32`, signed element nominal-type index `i32`, and maximum `u32`.
Version 1.0 admits only canonical native scalar, text, bytes, or enum fields;
element kind is `Void`, element nominal index is `-1`, and maximum is zero.

An enum item contains its member-name string and signed value `i32`. Each
required service is one closed `Nativeˉservice` byte. The native fragment
verifier remains responsible for name validity, uniqueness and order, nominal
references, service order, symbol ranges, patch meaning, entry shape, and the
independently decoded x86-64 instruction contract.

## Limits and validation

- complete artifact: at most 64 MiB;
- code: 1 through 32 MiB;
- symbols: at most 4,096;
- patches: at most 65,536;
- nominal types: at most 1,024;
- record fields: at most 64 per record;
- enum members: at most 256 per enum; and
- required services: at most 12.

Readers check the outer size before copying, bound every count before
allocation, use checked offsets and lengths, reject unknown enum values and
nonzero reserved fields, require strict UTF-8, resolve every patch index, and
require exact end of input. `Readˉandˉverify` then runs the existing native
fragment verifier; structural decoding alone never authorizes execution.

`WNF1001` through `WNF1011` identify size, truncation, magic, version, total
length, architecture, count, record, UTF-8, and trailing-input failures.
`WNF2001` and `WNF2002` identify an encoder size failure or source metadata
outside the serializable contract.

Retained production WVNF files require an external exact length and SHA-256
identity in addition to format verification. The digest binds the selected
derived artifact; it does not replace structural or machine-code verification.

## Current retained runtime fragments

Decision 0368 retains these exact `WVNF 1.0` artifacts:

| Consumer | Bytes | SHA-256 |
| --- | ---: | --- |
| Segmented enum metadata | 115,167 | `d2f53cd0fdd7812699a06234e19586f18492ffbca68ae0e5f507b09253c5a39b` |
| Executable-image layout | 61,583 | `9deeb8c4ab8f080cbc187036e0b015932379956930ec9cd1b7f51f7d1daa1f47` |
| Publication lifetime | 46,125 | `4d87911f2f442e6a2e4dd2364138f35a0037ddc0bff0775a16e37156768777a8` |

The normal runtime checks each identity, calls `Readˉandˉverify`, and requires
the exact bytes-input/descriptor-result entry shape before execution. Their
portable source WVBs remain outside the runtime assembly as reproducible
qualification, differential, and recovery evidence.
