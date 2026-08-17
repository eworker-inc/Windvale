# Workload 4 canonical output format

## Envelope

`WVFE 1` is a paper-only deterministic front-end artifact. It is not WVB, WIR,
WVO, an object file, or a source-freeze format.

All integers are unsigned little-endian. Text is canonical strict UTF-8.

| Offset/sequence | Width | Meaning |
| --- | ---: | --- |
| magic | 8 | ASCII `WVFE0001` |
| format version | 4 | exact value 1 |
| symbol count | 8 | number of symbol records |
| operation count | 8 | number of operation records |
| symbols | variable | ascending exact text order |
| operations | variable | deterministic bound-operation order |

Each symbol is `Ordinal:u32`, `Name_bytes:u64`, then exactly `Name_bytes` UTF-8
bytes. Names are nonempty and already validated. Ordinals are declaration-order
identities, so sorted symbol order need not match ordinal order.

## Operation tags

| Tag | Payload | Meaning |
| ---: | --- | --- |
| 1 | `Ordinal:u32` | Begin declaration |
| 2 | `Value:u64` | Push literal |
| 3 | `Ordinal:u32` | Load prior symbol |
| 4 | none | Add top two values |
| 5 | none | Multiply top two values |
| 6 | `Ordinal:u32` | End/store declaration |
| 7 | none | Return top value |

Unknown tags, truncated payloads, count/length overflow, invalid UTF-8, duplicate
symbol names, nonascending names, invalid ordinal use, stack underflow, missing
final Return, trailing bytes, or a file above its admitted maximum are malformed.
The later reader must reject them before expensive allocation.

## Publication

The encoder reserves the complete selected output maximum before writing. Each
append is all-or-nothing. A limit failure destroys the private partial builder,
emits `Outputˉlimit`, and publishes no bytes. Freeze is the sole publication
point.

Two executions with the same source bytes, limits, Foundation signature set,
and compiler version produce byte-identical output. Host, process, address, hash
seed, locale, newline convention, filesystem, and wall time never enter bytes.
