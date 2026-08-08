# Windvale native hosted-startup instantiation

## Status and scope

`WVSI 1` and `WVSD 1` are linker-private contracts for instantiating the
canonical Windows and Linux hosted-tool startup objects. The canonical machine
code remains the existing WVA source and its exact WVO output. Windvale
validates the bounded startup-object profile and applies every `relative-i32`
relocation; it does not contain a second copy of the startup instructions.

This version accepts one x86-64 `.text` section, at most 4,096 code bytes, 128
symbols, 256 relocations, and 65,536 object bytes. The current objects are the
Windows 1,510-byte startup with 40 symbols and 58 relocations and the Linux
765-byte startup with 26 symbols and 31 relocations.

## Request envelope: `WVSI 1`

The request begins with a 40-byte header, followed by one `u32` absolute target
address for every canonical WVO relocation, followed by the complete WVO.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVSI`, `0x49535657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | Exact request length |
| 12 | 4 | startup address | Exactly `4,096` in this hosted profile |
| 16 | 4 | code bytes | `4` through `4,096` |
| 20 | 4 | symbol count | `1` through `128` |
| 24 | 4 | relocation count | `1` through `256` |
| 28 | 4 | target count | Equal to relocation count |
| 32 | 4 | WVO bytes | `1` through `65,536`; consumes the request tail |
| 36 | 4 | reserved | Zero |

Target addresses appear in canonical relocation order. They include targets
for imports and locally defined symbols. The current managed bridge projects
this list temporarily; the native outer-container planner must take over that
projection before the bridge is retired.

## Admitted startup-object profile

Windvale requires canonical WVO 1.0, x86-64, no flags, exact declared counts,
one materialized code section named `.text`, alignment 16, and exact code and
memory sizes. Symbol records must be bounded, canonically ordered, uniquely
named by that ordering, and valid for either the code section or an import.
Every relocation must be a non-overlapping, ordered `relative-i32` record in
section zero, refer to an existing symbol, carry addend `-4`, and identify a
four-byte zero placeholder inside the code. Trailing bytes are rejected.

This is a deliberately narrower consumer of WVO 1.0, not a replacement for
the general object verifier. It owns only the exact hosted-startup shape that
the outer-container constructor needs.

## Response envelope: `WVSD 1`

The response begins with a 32-byte header. Successful responses append exactly
the relocated startup code.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVSD`, `0x44535657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `32` on failure; `32 + code bytes` on success |
| 12 | 4 | status | Status below |
| 16 | 4 | failure offset | Relevant request byte; request length on success |
| 20 | 4 | code bytes | Zero on failure; exact code length on success |
| 24 | 8 | reserved | Zero |

| Value | Meaning |
| ---: | --- |
| 0 | Valid; relocated startup bytes follow |
| 1 | Actual or declared request size is invalid |
| 2 | Request magic differs |
| 3 | Request version differs |
| 4 | Header, target count, target address, or reserved data is invalid |
| 5 | The WVO does not satisfy the hosted-startup profile |
| 6 | A relative relocation result is outside signed 32-bit range |

## Windvale owner and retained artifacts

The portable source exports service-free `Main(bytes) -> bytes`. The normal
linker embeds only the exact WVNF plus the two exact WVO resources; the WVB is
retained for reproducibility and recovery evidence.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Startup-instantiation WVB | 20,078 | `4cd40719ecbfe8f42f5ded4b0b2ba4df4e48a8463f4ea236c7c0831d22a3eb52` |
| Startup-instantiation WVNF | 185,841 | `b499e5f6ec3fb09c4efc33aa364533c6c6b0daa680fd3847b0054e2c7f346311` |
| Windows hosted startup WVO | 4,334 | `55f4782e976038c2d68bb91aeabb75518103524e9d5caaf1cc9f0662ab5a0feb` |
| Linux hosted startup WVO | 2,390 | `0df0525b35bbeb63492929d974326f328c247ce9313111ee6a8c1e321a2c22ff` |

The former C# template patchers remain under `Buildˉstage0` only for
differential and recovery evidence. The temporary C# target projection,
request/response adapter, and focused test retire after native outer-container
construction and native qualification consume this contract directly.
